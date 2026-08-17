"""
Reference CW decoder for Hamlet — validated against Tim's 2026-08-17 captures.

Pipeline (all block-streaming-friendly, no FFT, numpy only):
  1. TX/mute guard   : broadband RMS collapse -> freeze everything, holdoff on recovery
  2. Acquisition     : coarse Goertzel scan 300-900 Hz, 40 Hz ENBW
  3. Fine tracking   : 7 Goertzel bins, 5 Hz spacing, 50 ms window / 10 ms hop (~20 Hz ENBW),
                       re-centered when the peak walks
  4. Gate            : per-3s two-means threshold on dB envelope, refuses below 6 dB contrast,
                       6 dB hysteresis, de-glitch
  5. Clock           : two-means on mark durations, dah/dit ratio must land in 2.5-3.8
                       (IC-7300 keys 2.8-4.5); no valid clock -> emit nothing
  6. Gaps            : classified by clustering the gaps themselves (handles tight fists
                       whose inter-element gaps are shorter than their dits)
  7. Characters      : table allowed to say no; unbroken long tone -> "carrier", never letters
  8. Confidence      : worse of (timing margin, SNR margin) per character
"""
import numpy as np, wave, sys

MORSE = {'.-':'A','-...':'B','-.-.':'C','-..':'D','.':'E','..-.':'F','--.':'G','....':'H',
'..':'I','.---':'J','-.-':'K','.-..':'L','--':'M','-.':'N','---':'O','.--.':'P','--.-':'Q',
'.-.':'R','...':'S','-':'T','..-':'U','...-':'V','.--':'W','-..-':'X','-.--':'Y','--..':'Z',
'-----':'0','.----':'1','..---':'2','...--':'3','....-':'4','.....':'5','-....':'6',
'--...':'7','---..':'8','----.':'9','-..-.':'/','..--..':'?','.-.-.':'<AR>','-...-':'<BT>',
'...-.-':'<SK>','-.--.':'<KN>','.-.-.-':'.'}

def load_wav(path):
    w = wave.open(path)
    fs = w.getframerate()
    x = np.frombuffer(w.readframes(w.getnframes()), dtype=np.int16).astype(np.float64)/32768.0
    w.close()
    return x, fs

def goertzel_power(x, fs, f0, N, hop):
    """Power at f0 over sliding windows of N samples, hopped by `hop`. Vectorized reference
    for what a per-sample streaming Goertzel produces in C#."""
    n = np.arange(N)
    coeff = np.exp(-2j*np.pi*f0/fs*n)
    starts = np.arange(0, len(x)-N, hop)
    out = np.empty(len(starts))
    for i, s in enumerate(starts):
        out[i] = np.abs(np.dot(x[s:s+N], coeff))
    return out / N

# ---------------- 1. TX / mute guard ----------------
def mute_mask(x, fs, frame_ms=10, thresh_db=-60, holdoff_ms=150):
    fl = int(fs*frame_ms/1000)
    nfr = len(x)//fl
    rms = 20*np.log10(np.sqrt(np.mean(x[:nfr*fl].reshape(nfr, fl)**2, axis=1))+1e-9)
    muted = rms < thresh_db
    # extend each mute by holdoff for AGC recovery
    hold = int(np.ceil(holdoff_ms/frame_ms))
    out = muted.copy()
    idx = np.where(muted)[0]
    for i in idx:
        out[i:i+hold+1] = True
    return out, frame_ms   # per-frame mask

def frac_active(mask): return 1.0 - mask.mean()

# ---------------- 2. acquisition ----------------
def acquire_tone(x, fs, mask, frame_ms):
    N = int(fs*0.025); hop = int(fs*0.010)          # 25 ms window ~ 40 Hz ENBW
    best_f, best_score = None, 0.0
    for f0 in np.arange(300, 901, 25):
        p = goertzel_power(x, fs, f0, N, hop)
        t_ms = (np.arange(len(p))*hop/fs*1000/frame_ms).astype(int)
        t_ms = np.clip(t_ms, 0, len(mask)-1)
        p = p[~mask[t_ms]]                          # active audio only
        if len(p) < 50: continue
        pdb = 20*np.log10(p+1e-9)
        score = np.percentile(pdb, 90) - np.percentile(pdb, 30)   # keyed tone -> big spread
        if score > best_score: best_score, best_f = score, f0
    return best_f

# ---------------- 3. fine tracking + envelope ----------------
def fine_envelope(x, fs, f_center, mask, frame_ms):
    N = int(fs*0.050); hop = int(fs*0.010)          # 50 ms / 10 ms  -> ~20 Hz ENBW
    offsets = np.arange(-15, 16, 5)
    powers = np.stack([goertzel_power(x, fs, f_center+o, N, hop) for o in offsets])
    k = np.argmax(powers, axis=0)
    env = powers[k, np.arange(powers.shape[1])]
    f_inst = f_center + offsets[k]
    t = np.arange(len(env))*hop/fs
    fr = np.clip((t*1000/frame_ms).astype(int), 0, len(mask)-1)
    active = ~mask[fr]
    return t, 20*np.log10(env+1e-9), f_inst, active

# ---------------- 4. gate ----------------
def two_means(v, it=15):
    c1, c2 = np.percentile(v, 15), np.percentile(v, 85)
    for _ in range(it):
        a = v[np.abs(v-c1) <= np.abs(v-c2)]; b = v[np.abs(v-c1) > np.abs(v-c2)]
        if len(a): c1 = a.mean()
        if len(b): c2 = b.mean()
    return c1, c2

def gate(t, edb, active, win_s=3.0, min_contrast_db=6.0, hyst_db=6.0):
    dt = t[1]-t[0]
    W = int(win_s/dt)
    th_mid = np.full(len(edb), np.nan)
    contrast = np.full(len(edb), 0.0)
    for a in range(0, len(edb), W//2):
        b = min(a+W, len(edb))
        w = edb[a:b][active[a:b]]
        if len(w) < 20: continue
        c1, c2 = two_means(w)
        if c2-c1 >= min_contrast_db:
            th_mid[a:b] = (c1+c2)/2
            contrast[a:b] = np.maximum(contrast[a:b], c2-c1)
    key = np.zeros(len(edb), bool); on = False
    for i in range(len(edb)):
        if not active[i] or np.isnan(th_mid[i]): on = False; key[i] = False; continue
        hi, lo = th_mid[i]+hyst_db/2, th_mid[i]-hyst_db/2
        if not on and edb[i] > hi: on = True
        elif on and edb[i] < lo: on = False
        key[i] = on
    return key, contrast

def deglitch(key, dt, min_ms):
    mn = max(1, int(min_ms/1000/dt))
    k = key.copy()
    for target, val in [(False, True), (True, False)]:
        d = np.diff(k.astype(int)); s = list(np.where(d != 0)[0]+1)
        bounds = [0]+s+[len(k)]
        for a, b in zip(bounds[:-1], bounds[1:]):
            if k[a] == target and (b-a) < mn: k[a:b] = val
    return k

def runs(key, t, active=None, border_ms=60):
    d = np.diff(key.astype(int)); s = np.where(d == 1)[0]+1; e = np.where(d == -1)[0]+1
    if len(e) and len(s) and e[0] < s[0]: e = e[1:]
    n = min(len(s), len(e))
    dt = t[1]-t[0]
    marks = (e[:n]-s[:n])*dt*1000
    spaces = (s[1:n]-e[:n-1])*dt*1000
    starts = t[s[:n]]
    trunc = np.zeros(n, bool)
    if active is not None:
        w = max(1, int(border_ms/1000/dt))
        for i in range(n):
            a0 = max(0, s[i]-w); b0 = min(len(active), e[i]+w)
            if (~active[a0:s[i]]).any() or (~active[e[i]:b0]).any():
                trunc[i] = True          # evidence truncated by a mute boundary
    return marks, spaces, starts, trunc

# ---------------- 5+6. clock and gap classification ----------------
def fit_clock(marks):
    if len(marks) < 8: return None
    c1, c2 = two_means(marks)
    ratio = c2/max(c1, 1e-9)
    if not (2.5 <= ratio <= 3.8): return None
    if c1 < 30 or c1 > 350: return None            # 4-40 WPM sanity
    return c1, c2

def classify_gaps(spaces, dit):
    """Cluster gaps into intra/char/word. Falls back to dit multiples if too few."""
    g = np.sort(spaces[spaces < 12*dit])
    intra_hi = 0.85*dit                            # default boundaries
    char_hi = 3.0*dit
    if len(g) >= 10:
        # find the two largest multiplicative jumps in sorted gaps
        r = g[1:]/np.maximum(g[:-1], 1e-9)
        j = np.argsort(r)[::-1][:2]
        cuts = sorted(np.sqrt(g[i]*g[i+1]) for i in j if r[i] > 1.25)
        if len(cuts) == 2: intra_hi, char_hi = cuts
        elif len(cuts) == 1: intra_hi = cuts[0]
    return intra_hi, char_hi

# ---------------- 7+8. characters with confidence ----------------
def decode(marks, spaces, starts, clock, contrast_db, trunc=None):
    if trunc is None: trunc = np.zeros(len(marks), bool)
    dit, dah = clock
    mid = np.sqrt(dit*dah)
    intra_hi, char_hi = classify_gaps(spaces, dit)
    out = []
    sym, sym_t0, margins = '', None, []
    def flush(end_t):
        if not sym: return
        if tainted:
            out.append(('▯', sym, sym_t0, end_t, 0.0)); return
        # timing margin: worst element's distance from the dit/dah boundary, 0..1
        tm = min(margins) if margins else 0.0
        ch = MORSE.get(sym)
        snr_m = min(1.0, max(0.0, (contrast_db-6.0)/14.0))   # 6 dB->0, 20 dB->1
        conf = min(tm, snr_m)
        if ch is None:
            out.append(('?', sym, sym_t0, end_t, 0.0))
        else:
            out.append((ch, sym, sym_t0, end_t, conf))
    tainted = False
    for i, m in enumerate(marks):
        if sym == '': sym_t0 = starts[i]; tainted = False
        if trunc[i]: tainted = True
        if m >= 8*dah:                                        # unbroken long tone
            flush(starts[i]); sym=''; margins=[]
            out.append(('<carrier>', '', starts[i], starts[i]+m/1000, 1.0))
            continue
        sym += '.' if m < mid else '-'
        margins.append(min(1.0, abs(np.log(m/mid)) / np.log(np.sqrt(dah/dit))))
        end_t = starts[i]+m/1000
        if i < len(spaces):
            sp = spaces[i]
            if sp > char_hi:
                flush(end_t); sym=''; margins=[]; out.append((' ', '', end_t, end_t, 1.0))
            elif sp > intra_hi:
                flush(end_t); sym=''; margins=[]
    flush(starts[-1]+marks[-1]/1000 if len(marks) else 0)
    return out

# ---------------- top level ----------------
def run(path):
    x, fs = load_wav(path)
    mask, frame_ms = mute_mask(x, fs)
    print(f"{path.split('/')[-1]}: {len(x)/fs:.1f}s, active {frac_active(mask)*100:.0f}%"
          f" (rest = own-TX/mute, frozen)")
    f0 = acquire_tone(x, fs, mask, frame_ms)
    if f0 is None:
        print("  no keyed tone found in 300-900 Hz -> emit nothing"); return
    t, edb, f_inst, active = fine_envelope(x, fs, f0, mask, frame_ms)
    f_used = np.median(f_inst[edb > np.percentile(edb[active], 85)])
    key, contrast = gate(t, edb, active)
    key = deglitch(key, t[1]-t[0], 20)             # pre-clock: 20 ms
    marks, spaces, starts, trunc = runs(key, t, active)
    clock = fit_clock(marks[~trunc])               # truncated marks are not evidence
    if clock is None:
        print(f"  tone at ~{f_used:.0f} Hz but element timings do not cluster as Morse"
              f" -> emit nothing (this is the honest output)"); return
    dit, dah = clock
    key = deglitch(key, t[1]-t[0], 0.4*dit)        # post-clock: 0.4 dit
    marks, spaces, starts, trunc = runs(key, t, active)
    cbar = np.median(contrast[contrast > 0]) if (contrast > 0).any() else 0
    chars = decode(marks, spaces, starts, clock, cbar, trunc)
    wpm = 1200/dit
    print(f"  tone {f_used:.0f} Hz | dit {dit:.0f} ms dah {dah:.0f} ms ({wpm:.1f} WPM)"
          f" | gate contrast {cbar:.0f} dB")
    line, confline = [], []
    for ch, sym, t0, t1, conf in chars:
        if ch == ' ': line.append(' '); confline.append(' ')
        elif ch == '<carrier>': line.append('<carrier>'); confline.append(' '*9)
        elif ch == '?': line.append('▯'); confline.append(' ')
        else:
            line.append(ch)
            confline.append('#' if conf > 0.5 else ('+' if conf > 0.25 else '.'))
    print(f"  DECODE : {''.join(line)}")
    print(f"  conf   : {''.join(confline)}   (# high  + mid  . low  ▯ unresolved)")
    for ch, sym, t0, t1, conf in chars:
        if ch not in (' ',):
            print(f"    {t0:6.2f}s  {ch:>10s}  {sym:<8s} conf {conf:.2f}")

if __name__ == '__main__':
    for p in sys.argv[1:]: run(p)

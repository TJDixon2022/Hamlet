"""Semi-Markov (explicit-duration) Viterbi CW decoder.

The whole keying sequence explains the envelope: states are dit, dah, inside gap,
letter gap, word gap, each with a duration range in dits. A noise burst is rejected
not because of its level but because no legal sequence of columns explains it.
"""
import sys, wave, numpy as np
from scipy.signal import butter, lfilter

MORSE = {'.-':'A','-...':'B','-.-.':'C','-..':'D','.':'E','..-.':'F','--.':'G','....':'H','..':'I',
'.---':'J','-.-':'K','.-..':'L','--':'M','-.':'N','---':'O','.--.':'P','--.-':'Q','.-.':'R','...':'S',
'-':'T','..-':'U','...-':'V','.--':'W','-..-':'X','-.--':'Y','--..':'Z','-----':'0','.----':'1',
'..---':'2','...--':'3','....-':'4','.....':'5','-....':'6','--...':'7','---..':'8','----.':'9',
'..--..':'?','--..--':',','.-.-.-':'.','-..-.':'/','-...-':'<BT>','.-.-.':'<AR>','...-.-':'<SK>',
'-.--.':'<KN>','.-...':'<AS>','-.-.-':'<KA>'}

def load(path):
    w = wave.open(path); sr = w.getframerate(); n = w.getnframes()
    x = np.frombuffer(w.readframes(n), dtype=np.int16).astype(float) / 32768
    return x, sr

def envelope_db(x, sr, pitch, bw, hop_ms=5):
    t = np.arange(len(x)) / sr
    m = x * np.exp(-2j * np.pi * pitch * t)
    b, a = butter(4, bw / (sr / 2))
    env = np.abs(lfilter(b, a, m))
    hop = int(sr * hop_ms / 1000)
    env = env[::hop]
    return 20 * np.log10(env + 1e-9)

def decode(db, dit_hops, s_on=4.0, s_off=4.0, top=None, floor=None, verbose=False):
    """db: envelope in dB per hop. dit_hops: dit length in hops."""
    n = len(db)
    if floor is None: floor = np.percentile(db, 20)
    if top is None: top = np.percentile(db, 90)
    # emissions (log-likelihood) per hop for on / off
    e_on = -0.5 * ((db - top) / s_on) ** 2
    e_off = -0.5 * ((db - floor) / s_off) ** 2
    # cumulative
    c_on = np.concatenate([[0], np.cumsum(e_on)])
    c_off = np.concatenate([[0], np.cumsum(e_off)])
    d = dit_hops
    # states: 0 dit,1 dah,2 inside gap,3 letter gap,4 word gap
    rng = {0: (0.5*d, 1.6*d), 1: (2.0*d, 4.5*d), 2: (0.4*d, 1.7*d), 3: (1.8*d, 5.0*d), 4: (5.0*d, 14*d)}
    rng = {k: (max(1, int(round(a))), max(2, int(round(b)))) for k, (a, b) in rng.items()}
    on = {0: True, 1: True, 2: False, 3: False, 4: False}
    # allowed predecessors
    pred = {0: [2, 3, 4, -1], 1: [2, 3, 4, -1], 2: [0, 1], 3: [0, 1], 4: [0, 1, -1]}
    # duration prior: slight preference for mid-range -> log prob per segment: -log(range) (uniform)
    NEG = -1e18
    best = np.full((5, n + 1), NEG); back = np.zeros((5, n + 1, 2), dtype=np.int64)
    # start: allow any state to start at 0 (position j means segment ends at j)
    start_ok = {0, 1, 4, 3, 2}
    for j in range(1, n + 1):
        for s in range(5):
            lo, hi = rng[s]
            if j < lo: continue
            durs = np.arange(lo, min(hi, j) + 1)
            i = j - durs  # segment starts
            seg = (c_on[j] - c_on[i]) if on[s] else (c_off[j] - c_off[i])
            # predecessor best at i
            prev = np.full(len(i), NEG)
            prev_state = np.full(len(i), -1)
            for p in pred[s]:
                if p == -1:
                    cand = np.where(i == 0, 0.0, NEG)
                else:
                    cand = best[p, i]
                better = cand > prev
                prev[better] = cand[better]; prev_state[better] = p
            tot = prev + seg - np.log(hi - lo + 1)
            k = int(np.argmax(tot))
            if tot[k] > NEG / 2:
                best[s, j] = tot[k]; back[s, j] = (prev_state[k], i[k])
    # end
    s = int(np.argmax(best[:, n])); j = n; segs = []
    while j > 0:
        p, i = back[s, j]; segs.append((s, i, j)); s, j = int(p), int(i)
        if s < 0: break
    segs.reverse()
    return segs

def to_text(segs):
    out = []; cur = ''
    for s, i, j in segs:
        if s == 0: cur += '.'
        elif s == 1: cur += '-'
        elif s == 3:
            out.append(MORSE.get(cur, '■' if cur else '')); cur = ''
        elif s == 4:
            out.append(MORSE.get(cur, '■' if cur else '')); out.append(' '); cur = ''
    if cur: out.append(MORSE.get(cur, '■'))
    return ''.join(out).replace('  ', ' ')

def estimate_dit(db, hop_ms, lo_ms=30, hi_ms=200):
    """Dit from the envelope's own rhythm: autocorrelation of the on/off pattern."""
    x = db - np.median(db); x = (x > 0).astype(float); x -= x.mean()
    ac = np.correlate(x, x, 'full')[len(x) - 1:]
    ac /= ac[0] + 1e-9
    lo, hi = int(lo_ms / hop_ms), int(hi_ms / hop_ms)
    # first strong peak corresponds to 2 dits (dit + inside gap)
    k = lo + int(np.argmax(ac[lo:hi]))
    return max(2, k // 2)

if __name__ == '__main__':
    path, pitch, t0, t1 = sys.argv[1], float(sys.argv[2]), float(sys.argv[3]), float(sys.argv[4])
    bw = float(sys.argv[5]) if len(sys.argv) > 5 else 60
    dit_ms = float(sys.argv[6]) if len(sys.argv) > 6 else 0
    x, sr = load(path)
    x = x[int(t0 * sr):int(t1 * sr)]
    hop_ms = 5
    db = envelope_db(x, sr, pitch, bw, hop_ms)
    if dit_ms == 0:
        d = estimate_dit(db, hop_ms); print(f"dit estimate {d*hop_ms} ms")
    else:
        d = int(round(dit_ms / hop_ms))
    floor = np.percentile(db, 20); top = np.percentile(db, 90)
    print(f"floor {floor:.1f} top {top:.1f} contrast {top-floor:.1f} dB, dit {d*hop_ms} ms")
    segs = decode(db, d)
    print(to_text(segs))

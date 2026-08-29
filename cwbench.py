#!/usr/bin/env python3
"""
Standalone CW decode bench implementing Guenther (1973) classification.
No dependency on Hamlet. Reads a WAV, prints text.
"""
import sys, wave, numpy as np

MORSE = {'.-':'A','-...':'B','-.-.':'C','-..':'D','.':'E','..-.':'F','--.':'G',
'....':'H','..':'I','.---':'J','-.-':'K','.-..':'L','--':'M','-.':'N','---':'O',
'.--.':'P','--.-':'Q','.-.':'R','...':'S','-':'T','..-':'U','...-':'V','.--':'W',
'-..-':'X','-.--':'Y','--..':'Z','-----':'0','.----':'1','..---':'2','...--':'3',
'....-':'4','.....':'5','-....':'6','--...':'7','---..':'8','----.':'9',
'-...-':'=','-..-.':'/','.-.-.':'<AR>','...-.-':'<SK>','-.--.':'(','.-.-':'<AA>',
'--..--':',','.-.-.-':'.','..--..':'?','-....-':'-',"---...":':','.--.-.':'@'}

def load(path):
    w = wave.open(path); sr = w.getframerate()
    d = np.frombuffer(w.readframes(w.getnframes()), dtype=np.int16).astype(float)
    return sr, d

def find_tone(sr, d, lo=300, hi=1200):
    N = 16384
    S = np.zeros(N//2+1); n=0
    for i in range(0, len(d)-N, N//2):
        S += np.abs(np.fft.rfft(d[i:i+N]*np.hanning(N))); n+=1
    S /= max(n,1)
    fr = np.fft.rfftfreq(N, 1/sr)
    m = (fr>=lo)&(fr<=hi)
    idx = np.argmax(S[m]); f = fr[m][idx]
    # parabolic interpolation on the magnitude peak
    j = np.where(m)[0][idx]
    if 0 < j < len(S)-1:
        a,b,c = S[j-1],S[j],S[j+1]
        denom = (a-2*b+c)
        if denom != 0:
            f = fr[j] + 0.5*(a-c)/denom * (fr[1]-fr[0])
    return f

def envelope(sr, d, f0, bw=40.0, step_ms=1.0):
    t = np.arange(len(d))/sr
    iq = d*np.exp(-2j*np.pi*f0*t)
    k = max(1, int(sr/bw))
    env = np.abs(np.convolve(iq, np.ones(k)/k, 'same'))
    step = max(1, int(sr*step_ms/1000.0))
    return env[::step], step_ms

def key_states(env, ms_per):
    """Adaptive threshold: midpoint between running noise floor and signal peak,
    in dB, per Guenther's 'convert to dc pulses while discriminating noise'."""
    e = 20*np.log10(env + 1e-12)
    win = int(2000/ms_per)                       # 2 s window
    out = np.zeros(len(e), dtype=bool)
    half = max(1, win//2)
    for i in range(0, len(e), half):
        a, b = i, min(i+half, len(e))
        c, dd = max(0, i-half), min(i+win, len(e))   # context wider than the slice
        seg = e[c:dd]
        if len(seg) < 10:
            continue
        lo = np.percentile(seg, 20); hi = np.percentile(seg, 95)
        if hi - lo < 6:                          # nothing keyed here
            continue
        thr = lo + 0.5*(hi-lo)
        out[a:b] = e[a:b] > thr
    return out

def _rle(on, ms_per):
    r = []; cur = on[0]; n = 0
    for v in on:
        if v == cur: n += 1
        else:
            r.append([cur, n*ms_per]); cur = v; n = 1
    r.append([cur, n*ms_per])
    return r

def _merge(r):
    out = []
    for v, ms in r:
        if out and out[-1][0] == v: out[-1][1] += ms
        else: out.append([v, ms])
    return out

def runs(on, ms_per, min_ms=15):
    """Remove runs too short to be real, then merge the neighbours they joined.
    Repeat until stable -- dropping a run without merging destroys every
    duration that follows it."""
    r = _merge(_rle(on, ms_per))
    while True:
        short = [i for i, (v, ms) in enumerate(r) if ms < min_ms]
        if not short: break
        r = _merge([[v, ms] for v, ms in r if ms >= min_ms])
        if len(short) == 0: break
        if all(ms >= min_ms for v, ms in r): break
    return [(v, ms) for v, ms in r]

class Guenther:
    """Guenther 1973, sections 3.1-3.6."""
    N = 8
    def __init__(self, dit0=60.0):
        self.dot = dit0            # floating average of last N dots
        self.dash = dit0*3         # floating average of last N dashes
        self.cw = dit0*5           # character/word average, dot-preceded spaces only
        self.have_dot = False; self.have_dash = False

    def pulse_boundary(self):
        # eq 3.2 -- NOT the midpoint; biased toward the dot average
        return self.dot + 0.4*(self.dash - self.dot)

    def feed_pulse(self, ms):
        b = self.pulse_boundary()
        is_dash = ms > b
        if is_dash:
            self.dash += (ms - self.dash)/self.N
            self.have_dash = True
        else:
            self.dot += (ms - self.dot)/self.N
            self.have_dot = True
        return '-' if is_dash else '.'

    def symbol_boundary(self, prev_was_dash):
        # eq 3.4 -- spaces after a dash are shorter, so the boundary slopes down
        b = self.pulse_boundary()
        if not prev_was_dash:
            return b
        dash = min(self.dash, 2*self.dash)   # eq: max dash = 2x running average
        return max(b - 0.35*(dash - self.dot), 0.5*self.dot)

    def cw_boundary(self, prev_was_dash, adjust=0.0):
        # eq 3.5/3.6 -- same slope correction applied to the char/word boundary
        base = self.cw
        if prev_was_dash:
            base = base - 0.35*(self.dash - self.dot)
        return max(base + adjust, self.pulse_boundary()*1.2)

    def feed_space(self, ms, prev_was_dash, adjust=0.0):
        sb = self.symbol_boundary(prev_was_dash)
        if ms <= sb:
            return 'sym'
        # eq 3.3 -- the char/word average is fed only by dot-preceded spaces
        if not prev_was_dash:
            self.cw += (ms - self.cw)/self.N
        return 'word' if ms > self.cw_boundary(prev_was_dash, adjust) else 'char'

def decode(path, adjust=0.0, verbose=False):
    sr, d = load(path)
    f0 = find_tone(sr, d)
    env, ms_per = envelope(sr, d, f0)
    on = key_states(env, ms_per)
    rr = runs(on, ms_per)
    if not rr:
        return f0, ''
    pulses = [ms for v, ms in rr if v]
    seed = np.percentile(pulses, 25) if pulses else 60.0
    g = Guenther(dit0=seed)
    out = []; sym = ''; prev_dash = False
    for v, ms in rr:
        if v:
            e = g.feed_pulse(ms); sym += e; prev_dash = (e == '-')
        else:
            kind = g.feed_space(ms, prev_dash, adjust)
            if kind == 'sym': continue
            out.append(MORSE.get(sym, '#' if sym else ''))
            sym = ''
            if kind == 'word': out.append(' ')
    if sym: out.append(MORSE.get(sym, '#'))
    return f0, ''.join(out)

if __name__ == '__main__':
    for p in sys.argv[1:]:
        f0, txt = decode(p)
        print(f'{p.split("/")[-1]}  tone {f0:.1f} Hz')
        print(f'   {txt}')
        print()

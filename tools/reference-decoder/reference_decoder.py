"""
reference_decoder.py — a working probabilistic CW decoder, for reference only.

WHAT THIS IS
    A ~120-line demonstration that a segmental (semi-Markov) Viterbi decoder,
    with no thresholding anywhere, reads Hamlet's own off-air recordings to
    legible text and stays silent on recordings holding no station.

    It is NOT production code, it is NOT on Hamlet's decode path, and nothing
    in src/ should call it. It exists so a session porting this to C# has an
    implementation to check its output against, rather than a description in
    a work order.

WHY IT EXISTS
    Hamlet thresholds the envelope into hard key-down/key-up runs, fits speed
    by clustering those run lengths, and picks its analysis window from the
    fitted speed. That is a loop: noise chatter shortens the fitted dit, a
    short dit reads as a fast fist, a fast fist selects a wider bandwidth,
    more noise crosses the threshold. Measured: senders working near 14 WPM
    fitted at 22-56 WPM, and eight of nine recordings sat at 75 Hz.

    This decoder cannot have that loop, because nothing here measures speed
    from run lengths. Speed is an outer hypothesis and the audio picks.

THE THREE IDEAS, which are the whole of it
    1. Never threshold. Per-sample log-likelihoods of key-down and key-up
       against a noise model, carried forward as numbers.
    2. Speed is a hypothesis, not a measurement. Several are tried; the one
       with the best total likelihood wins.
    3. Segment boundaries and character boundaries are chosen together, by
       dynamic programming over whole elements, not by comparing gaps to
       thresholds one at a time.

    These are Bell 1977's ideas (E. L. Bell, "Optimal Bayesian estimation of
    the state of a probabilistically mapped memory-conditional Markov process
    with application to manual morse decoding"), reduced to something small.
    Bell's own implementation is ~5,400 lines and carries ~20 parallel paths
    with Kalman-filtered likelihoods; ag1le/morse-wip is a GPL-3.0 C++ port.

MEASURED, on the recordings in tests/fixtures/cw/captured/
    003016  LR 24.2  22 WPM  I= HADA KPA15TT ITWAS JUNK = ESTILL HVE MY ETO 91B TT JUST VFB TUBELIN
    003126  LR 30.9  28 WPM  A OM = I WATCH AT LEAST 2 MOVIES A DAY WID X# WHY NOT ... WESTERNS
    003758  LR 39.2  16 WPM  KIS QRL TU ... AA4MP/4 QNIK
    004507  LR 32.5  18 WPM  E JJ AT ARRL DOT NET = EACH STATION HANDLING THIS MESSAGE PE
    014854  LR  6.1          (no station -- gate closes)
    014935  LR  2.8          (no station -- gate closes)

    The likelihood ratio separates cleanly: 24-39 with a station, 3-6 without.
    Any gate between 10 and 20 preserves HM-DEC-120 -- silence on an empty
    band -- while reading every station. That property is not traded away to
    get the character counts; it falls out of having a null hypothesis.

WHAT IS NOT DEMONSTRATED HERE
    Streaming. This runs offline over whole 30-second files. Real-time needs a
    sliding window and a decision delay (Bell used ~1000 ms), and that is the
    one piece nobody has measured yet. Do not assume it is free.

    The speed search is an outer loop over twelve hypotheses. Fine at file
    granularity; its cost per second of audio in a live terminal is unmeasured.

    No answer key exists for any of these recordings. The text above is what
    this decoder produced, not an adjudicated truth (SS12.5).

Requires numpy. Run:  python reference_decoder.py <wavfile> [more wavfiles]
"""
import sys, wave
import numpy as np

MORSE = {
    '.-':'A','-...':'B','-.-.':'C','-..':'D','.':'E','..-.':'F','--.':'G','....':'H',
    '..':'I','.---':'J','-.-':'K','.-..':'L','--':'M','-.':'N','---':'O','.--.':'P',
    '--.-':'Q','.-.':'R','...':'S','-':'T','..-':'U','...-':'V','.--':'W','-..-':'X',
    '-.--':'Y','--..':'Z','-----':'0','.----':'1','..---':'2','...--':'3','....-':'4',
    '.....':'5','-....':'6','--...':'7','---..':'8','----.':'9','-...-':'=','.-.-.':'+',
    '-..-.':'/','..--..':'?','-....-':'-','.--.-.':'@','...-.-':'%',
}

# Element kinds: (duration in units, is key-down, token)
KINDS = [(1,True,'.'), (3,True,'-'), (1,False,''), (3,False,'|'), (7,False,' ')]

GATE = 15.0          # log-likelihood ratio per sample below which nothing is emitted.
                     # Measured separation is 3-6 (no station) against 24-39 (station).
                     # Provisional. Wants an evening's captures scored against it.


def find_tone(path, lo=300.0, hi=1200.0):
    """Loudest bin in the CW range. Independent of any decoder state."""
    w = wave.open(path); n = w.getnframes(); sr = w.getframerate()
    x = np.frombuffer(w.readframes(n), dtype=np.int16).astype(float) / 32768.0
    N = 1 << 15
    S = np.zeros(N // 2 + 1)
    for i in range(0, max(1, len(x) - N), N // 2):
        S += np.abs(np.fft.rfft(x[i:i + N] * np.hanning(N)))
    fr = np.fft.rfftfreq(N, 1.0 / sr)
    b = (fr > lo) & (fr < hi)
    return float(fr[b][np.argmax(S[b])])


def envelope(path, tone, bw_hz=60.0, hop_ms=5.0):
    """Quadrature mixdown to the tone, boxcar to bw_hz, sampled every hop_ms."""
    w = wave.open(path); n = w.getnframes(); sr = w.getframerate()
    x = np.frombuffer(w.readframes(n), dtype=np.int16).astype(float) / 32768.0
    t = np.arange(len(x)) / sr
    k = int(sr / bw_hz)
    e = np.abs(np.convolve(x * np.exp(-2j * np.pi * tone * t), np.ones(k) / k, 'same'))
    return e[::int(sr * hop_ms / 1000.0)], hop_ms


def loglik_streams(e):
    """Per-sample log-likelihood of key-down and key-up. No threshold is formed.

    Noise scale from the lower quartile, signal amplitude from the upper tail.
    Bell does this properly with a tracked noise power estimate feeding Kalman
    recursions; this is the cheap version and it is where a port should improve.
    """
    sd = max(np.percentile(e, 25) * 0.6, 1e-6)
    amp = max(np.percentile(e, 97), sd * 1.05)
    key_up   = -0.5 * (e / sd) ** 2 - np.log(sd)
    key_down = -0.5 * ((e - amp) / sd) ** 2 - np.log(sd)
    return key_down, key_up, amp / sd


def decode_at(e, hop_ms, wpm, on_ll, off_ll):
    """Segmental Viterbi at one speed hypothesis.

    Every path is a chain of whole elements and gaps that must alternate
    key-down / key-up. A segment's score is the summed per-sample likelihood
    over its span, plus a Gaussian penalty on how far its length sits from
    the 1 / 3 / 7 unit the model expects. Cumulative sums make the span score
    O(1), so the whole thing is O(samples x durations x kinds).
    """
    unit = (1200.0 / wpm) / hop_ms
    n = len(e)
    con = np.concatenate([[0.0], np.cumsum(on_ll)])
    cof = np.concatenate([[0.0], np.cumsum(off_ll)])

    best = np.full(n + 1, -np.inf); best[0] = 0.0
    back = np.zeros((n + 1, 2), dtype=np.int32); back[:, 0] = -1
    was_on = np.zeros(n + 1, dtype=bool)

    for i in range(1, n + 1):
        for ki, (units, is_on, _tok) in enumerate(KINDS):
            want = units * unit
            dlo = max(1, int(want * 0.45))
            dhi = max(dlo + 1, int(want * 2.2))
            for d in range(dlo, min(dhi, i) + 1):
                j = i - d
                if best[j] == -np.inf:
                    continue
                if j > 0 and was_on[j] == is_on:      # elements must alternate
                    continue
                span = (con[i] - con[j]) if is_on else (cof[i] - cof[j])
                z = (d - want) / max(want * 0.35, 1.0)
                sc = best[j] + span - 0.5 * z * z
                if sc > best[i]:
                    best[i] = sc; back[i] = (j, ki); was_on[i] = is_on

    seq = []; i = n
    while i > 0 and back[i, 0] >= 0:
        j, ki = back[i]; seq.append(ki); i = j
    seq.reverse()

    sym = []; out = []
    for ki in seq:
        _units, is_on, tok = KINDS[ki]
        if is_on:
            sym.append(tok)
        elif tok in ('|', ' '):
            if sym:
                out.append(MORSE.get(''.join(sym), '#')); sym = []
            if tok == ' ':
                out.append(' ')
    if sym:
        out.append(MORSE.get(''.join(sym), '#'))
    return best[n], ''.join(out)


def decode(path, speeds=np.arange(10, 34, 2.0)):
    """Returns (likelihood ratio per sample, best wpm, text, tone).

    Text is empty when the ratio is below GATE -- an empty band emits nothing,
    because the all-key-up hypothesis is explicitly modelled and competes.
    """
    tone = find_tone(path)
    e, hop = envelope(path, tone)
    on_ll, off_ll, _snr = loglik_streams(e)
    null = off_ll.sum()

    best = None
    for wpm in speeds:
        score, text = decode_at(e, hop, wpm, on_ll, off_ll)
        if best is None or score > best[0]:
            best = (score, wpm, text)

    ratio = (best[0] - null) / len(e)
    return ratio, best[1], (best[2] if ratio >= GATE else ''), tone


if __name__ == '__main__':
    if len(sys.argv) < 2:
        print(__doc__); raise SystemExit(2)
    for path in sys.argv[1:]:
        ratio, wpm, text, tone = decode(path)
        print(f'{path}')
        print(f'   tone {tone:.0f} Hz   {wpm:.0f} WPM   ratio {ratio:.1f}'
              f'{"" if text else "   (below gate, nothing emitted)"}')
        if text:
            print(f'   {text}')

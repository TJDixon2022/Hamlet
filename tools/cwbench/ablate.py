#!/usr/bin/env python3
"""
Measure Guenther's three boundary ideas one at a time, in the bench.

Work instruction 050, task 4. Each of the three is switched off on its own and
the bench's reading is scored against the same adjudicated truth Hamlet is scored
against, so the numbers here and the numbers there mean the same thing.

**The scoring is a transcription of `CwAccuracy`**, semi-global alignment with
the same tie-break: on a tie the longer alignment wins, so a trailing wrong
character reads as a substitution rather than a refusal. Yield is correct over
truth; precision is correct over asserted.

**This scores the bench and never Hamlet.** Nothing measured here is a claim
about the application, and the bench has no refusal at all -- on audio holding no
station it emits text anyway, which is why its precision is not comparable to
Hamlet's on a capture Hamlet declines.
"""
import sys, os, importlib.util

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, '..', '..'))

spec = importlib.util.spec_from_file_location(
    'cwbench', os.path.join(HERE, 'cwbench.py'))
bench = importlib.util.module_from_spec(spec)
spec.loader.exec_module(bench)

# The adjudicated corpus, copied from tools/Hamlet.PitchRank/Program.cs.
# A second copy is a thing that can drift, so it is checked: `verify` reports any
# capture named here that the tool does not name, and the reverse.
TRUTHS = [
    ("cw-2026-08-17-013347", "VA3VRR"),
    ("cw-2026-08-17-134712", "N4L"),
    ("cw-2026-08-18-003758", "AA4MP/4 QNIK"),
    ("cw-2026-08-24-012403", "DE KD0UN KD0UN K"),
    ("cw-2026-08-18-004507",
     "AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAGE P"),
    ("cw-2026-08-22-031838", "2, 2, AND 2 WITH A MEAN OF 2.9. PRE"),
    ("cw-2026-08-22-031905", "DICTED 10.7 CENTIMETER FLUX IS 125, 125"),
    ("cw-2026-08-22-031948", "110, 110, AND 110 WITH A MEAN OF 117"),
    ("cw-2026-08-22-032012",
     "N OF 117. LINKS TO ARTICLES OR OTHER WEBSITES MENTI"),
    ("cw-2026-08-22-032050",
     "THIS BULLETIN CAN BE FOUND IN TELEPRINTER, PACKET, AND INTE"),
    ("cw-2026-08-22-032113", "ACKET, AND INTERNET VERSIONS"),
    ("cw-2026-08-22-032129", "2026 PROPAGATION FORECAST BULLETIN ARLP034"),
]


def find(capture):
    for folder in ('captured', 'captured/unadjudicated'):
        p = os.path.join(ROOT, 'tests/fixtures/cw', folder, capture + '.wav')
        if os.path.exists(p):
            return p
    return None


def score(read, truth):
    """Semi-global alignment. Returns (correct, subs, ins, dels, asserted)."""
    n, m = len(truth), len(read)
    NEG = -10 ** 9
    # cost[i][j] = best score aligning truth[:i] with read[:j]
    cost = [[NEG] * (m + 1) for _ in range(n + 1)]
    ln = [[0] * (m + 1) for _ in range(n + 1)]
    cor = [[0] * (m + 1) for _ in range(n + 1)]
    sub = [[0] * (m + 1) for _ in range(n + 1)]
    ins = [[0] * (m + 1) for _ in range(n + 1)]
    dele = [[0] * (m + 1) for _ in range(n + 1)]

    cost[0][0] = 0
    for j in range(1, m + 1):
        cost[0][j] = -j
        ins[0][j] = j
        ln[0][j] = j
    for i in range(1, n + 1):
        cost[i][0] = -i
        dele[i][0] = i
        ln[i][0] = i

    for i in range(1, n + 1):
        for j in range(1, m + 1):
            hit = truth[i - 1].upper() == read[j - 1].upper()
            options = [
                (cost[i - 1][j - 1] + (1 if hit else -1), 'diag'),
                (cost[i - 1][j] - 1, 'del'),
                (cost[i][j - 1] - 1, 'ins'),
            ]
            best = max(o[0] for o in options)
            # **THE TIE-BREAK TAKES THE LONGER ALIGNMENT**, so a trailing wrong
            # character reads as a substitution and not as a refusal.
            pick = max((o for o in options if o[0] == best),
                       key=lambda o: {'diag': 2, 'del': 1, 'ins': 1}[o[1]])
            cost[i][j] = best
            k = pick[1]
            if k == 'diag':
                cor[i][j] = cor[i - 1][j - 1] + (1 if hit else 0)
                sub[i][j] = sub[i - 1][j - 1] + (0 if hit else 1)
                ins[i][j] = ins[i - 1][j - 1]
                dele[i][j] = dele[i - 1][j - 1]
                ln[i][j] = ln[i - 1][j - 1] + 1
            elif k == 'del':
                cor[i][j], sub[i][j] = cor[i - 1][j], sub[i - 1][j]
                ins[i][j] = ins[i - 1][j]
                dele[i][j] = dele[i - 1][j] + 1
                ln[i][j] = ln[i - 1][j] + 1
            else:
                cor[i][j], sub[i][j] = cor[i][j - 1], sub[i][j - 1]
                ins[i][j] = ins[i][j - 1] + 1
                dele[i][j] = dele[i][j - 1]
                ln[i][j] = ln[i][j - 1] + 1

    return cor[n][m], sub[n][m], ins[n][m], dele[n][m], len(read.strip())


class Variant(bench.Guenther):
    """Guenther with one of the three ideas switched off."""

    def __init__(self, dit0, off=None):
        super().__init__(dit0)
        self.off = off

    def _off(self, name):
        return name in (self.off or ())

    def pulse_boundary(self):
        if self._off('boundary'):
            # Idea 1 off: the plain midpoint between the two averages.
            return self.dot + 0.5 * (self.dash - self.dot)
        return super().pulse_boundary()

    def symbol_boundary(self, prev_was_dash):
        if self._off('sloped'):
            # Idea 2 off: one threshold, whatever preceded it.
            return self.pulse_boundary()
        return super().symbol_boundary(prev_was_dash)

    def cw_boundary(self, prev_was_dash, adjust=0.0):
        if self._off('sloped'):
            return max(self.cw + adjust, self.pulse_boundary() * 1.2)
        return super().cw_boundary(prev_was_dash, adjust)

    def feed_space(self, ms, prev_was_dash, adjust=0.0):
        if not self._off('dotfed'):
            return super().feed_space(ms, prev_was_dash, adjust)
        # Idea 3 off: every non-symbol space feeds the character/word average.
        sb = self.symbol_boundary(prev_was_dash)
        if ms <= sb:
            return 'sym'
        self.cw += (ms - self.cw) / self.N
        return 'word' if ms > self.cw_boundary(prev_was_dash, adjust) else 'char'


def read(path, off):
    import numpy as np
    sr, d = bench.load(path)
    f0 = bench.find_tone(sr, d)
    env, ms_per = bench.envelope(sr, d, f0)
    on = bench.key_states(env, ms_per)
    rr = bench.runs(on, ms_per)
    if not rr:
        return ''
    pulses = [ms for v, ms in rr if v]
    seed = np.percentile(pulses, 25) if pulses else 60.0
    g = Variant(seed, off)
    out, sym, prev_dash = [], '', False
    for v, ms in rr:
        if v:
            e = g.feed_pulse(ms); sym += e; prev_dash = (e == '-')
        else:
            kind = g.feed_space(ms, prev_dash)
            if kind == 'sym':
                continue
            out.append(bench.MORSE.get(sym, '#' if sym else ''))
            sym = ''
            if kind == 'word':
                out.append(' ')
    if sym:
        out.append(bench.MORSE.get(sym, '#'))
    return ''.join(out)


def run(off, label, show=False):
    tc = co = su = ii = de = asserted = 0
    for capture, truth in TRUTHS:
        p = find(capture)
        if p is None:
            print(f'  MISSING {capture}')
            continue
        text = read(p, off)
        c, s, i, d, a = score(text, truth)
        tc += len(truth); co += c; su += s; ii += i; de += d; asserted += a
        if show:
            print(f'  {capture}  {text}')
    yld = co / tc if tc else 0
    prec = co / asserted if asserted else 0
    print(f'{label:34s} truth {tc:4d}  yield {yld:.3f}  '
          f'precision {prec:.3f}  correct {co:3d}  subs {su:3d}  '
          f'ins {ii:3d}  dels {de:3d}')
    return yld, prec


if __name__ == '__main__':
    show = '-v' in sys.argv
    print('Guenther ablation over the adjudicated corpus. '
          'Bench only; not a claim about Hamlet.\n')
    base = run(None, 'all three on (the bench as shipped)', show)
    print()
    for off, label in (
        (('boundary',), 'idea 1 OFF: dit/dah at the midpoint'),
        (('sloped',), 'idea 2 OFF: spaces not conditioned'),
        (('dotfed',), 'idea 3 OFF: every space feeds the average'),
        (('boundary', 'sloped'), 'ideas 1 AND 2 off, only 3 on'),
        (('boundary', 'sloped', 'dotfed'), 'all three OFF: textbook thresholds'),
    ):
        got = run(off, label, show)
        print(f'{"":34s} -> yield {got[0]-base[0]:+.3f}  '
              f'precision {got[1]-base[1]:+.3f} against all three on')

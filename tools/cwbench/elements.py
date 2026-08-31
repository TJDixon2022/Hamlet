# -*- coding: utf-8 -*-
"""The reference bench's element stream, printed with times.

WHY THIS EXISTS (work instruction 056, task 4)

    Same audio, same pitch: an independent decoder reads `CQ ... K` off
    cw-2026-08-31-003229 and Hamlet reads a storm of `E`. The difference is
    not in the letters -- both use the same Morse table -- so it is in the
    element stream each one built before any letter was named, and neither
    decoder printed that stream.

    This prints the reference's. Hamlet's comes out of its own decoder, which
    now carries the winning path's elements on the result (task 2). The two
    are aligned in time and read side by side.

WHAT IT IMPLEMENTS

    The parameters the work instruction states for the reference reading, and
    nothing else:

      * quadrature mixdown at a stated pitch,
      * a 25 ms integrator,
      * a threshold at the envelope's 98th percentile minus 6 dB,
      * minimum run 15 ms, dropped AND MERGED.

    The merge is the part that matters and is why this is not a five-line
    script. Dropping a short run without merging the two it separated leaves
    those two as separate runs of the same state, so every duration after it
    is wrong -- unit 054 proved Hamlet's own `Runs` does exactly that, and it
    is one of the named candidate causes this comparison is testing.

    NOT a Hamlet component, NOT on any decode path, and it shares no code
    with src/. Requires numpy.

    python tools/cwbench/elements.py <wav> --pitch 583.5 [--from 0 --to 30]
"""
import argparse
import os
import sys
import wave

import numpy as np

MORSE = {
    '.-': 'A', '-...': 'B', '-.-.': 'C', '-..': 'D', '.': 'E', '..-.': 'F',
    '--.': 'G', '....': 'H', '..': 'I', '.---': 'J', '-.-': 'K', '.-..': 'L',
    '--': 'M', '-.': 'N', '---': 'O', '.--.': 'P', '--.-': 'Q', '.-.': 'R',
    '...': 'S', '-': 'T', '..-': 'U', '...-': 'V', '.--': 'W', '-..-': 'X',
    '-.--': 'Y', '--..': 'Z', '-----': '0', '.----': '1', '..---': '2',
    '...--': '3', '....-': '4', '.....': '5', '-....': '6', '--...': '7',
    '---..': '8', '----.': '9', '-...-': '=', '-..-.': '/', '..--..': '?',
    '-....-': '-', '.-.-.': '+', '...-.-': '%',
}


def load(path):
    handle = wave.open(path)
    rate = handle.getframerate()
    raw = handle.readframes(handle.getnframes())
    return rate, np.frombuffer(raw, dtype=np.int16).astype(float) / 32768.0


def envelope(rate, data, pitch, integrator_ms=25.0, step_ms=1.0):
    """Quadrature mixdown to the pitch, boxcar of the stated length.

    **THE WIDTH IS A TIME AND NOT A BANDWIDTH**, which is the form the work
    instruction states it in and the form that reproduces the quoted reading.
    A boxcar of T seconds has a nominal bandwidth of 1/T, so the 25 ms here is
    40 Hz -- close enough to Hamlet's own 45 Hz that the order rules the
    integrator an unlikely cause, and far enough from the 25 Hz a careless
    reading of "25 ms integrator" produces that the two give different text.
    """
    t = np.arange(len(data)) / rate
    baseband = data * np.exp(-2j * np.pi * pitch * t)
    width = max(1, int(rate * integrator_ms / 1000.0))
    smoothed = np.abs(np.convolve(baseband, np.ones(width) / width, 'same'))
    step = max(1, int(rate * step_ms / 1000.0))
    return smoothed[::step], step_ms


def key_states(env, setback_db=6.0, percentile=98.0):
    """One threshold for the whole file, at a high percentile less a setback.

    Deliberately NOT adaptive. Unit 051's finding was that an adaptive
    threshold computed over a mostly-quiet window sits where the noise is, and
    003229's station is present for only part of the file -- so a fixed
    threshold referenced to the loud end is the comparison worth having.
    """
    db = 20 * np.log10(env + 1e-12)
    top = np.percentile(db, percentile)
    return db > (top - setback_db), top - setback_db


def _rle(on, ms_per):
    out = []
    current = bool(on[0])
    count = 0
    for value in on:
        if bool(value) == current:
            count += 1
        else:
            out.append([current, count * ms_per])
            current = bool(value)
            count = 1
    out.append([current, count * ms_per])
    return out


def _merge(runs):
    """Join adjacent runs of the same state. THIS is the half Hamlet omits."""
    out = []
    for value, ms in runs:
        if out and out[-1][0] == value:
            out[-1][1] += ms
        else:
            out.append([value, ms])
    return out


def runs(on, ms_per, shortest_ms=15.0):
    """Drop runs too short to be real, then merge the neighbours they joined."""
    r = _merge(_rle(on, ms_per))
    while True:
        if all(ms >= shortest_ms for _, ms in r):
            break
        kept = [[v, ms] for v, ms in r if ms >= shortest_ms]
        if not kept or len(kept) == len(r):
            break
        r = _merge(kept)
    return [(v, ms) for v, ms in r]


def stamped(runs_list, start_ms=0.0):
    """Each run with the time it began, so two streams can be aligned."""
    at = start_ms
    out = []
    for value, ms in runs_list:
        out.append((at, value, ms))
        at += ms
    return out


def spell(runs_list):
    """Guenther 1973's own running classifier, from `cwbench.py`.

    Imported rather than reimplemented. The letters here are only labels on the
    element stream, and a second copy of the classifier would let the labels
    drift from the ones the bench actually prints -- which is the whole reason
    the reference reading in the work instruction can be checked at all.
    """
    import cwbench

    marks = [ms for v, ms in runs_list if v]

    if not marks:
        return ''

    guenther = cwbench.Guenther(dit0=np.percentile(marks, 25))
    text = []
    symbol = ''
    previous_was_dash = False

    for value, ms in runs_list:
        if value:
            element = guenther.feed_pulse(ms)
            symbol += element
            previous_was_dash = element == '-'
            continue

        kind = guenther.feed_space(ms, previous_was_dash)

        if kind == 'sym':
            continue

        text.append(MORSE.get(symbol, '#' if symbol else ''))
        symbol = ''

        if kind == 'word':
            text.append(' ')

    if symbol:
        text.append(MORSE.get(symbol, '#'))

    return ''.join(text)


def main(argv=None):
    parser = argparse.ArgumentParser()
    parser.add_argument('wav')
    parser.add_argument('--pitch', type=float, required=True)
    parser.add_argument('--integrator', type=float, default=25.0,
                        help='boxcar length in milliseconds')
    parser.add_argument('--setback', type=float, default=6.0)
    parser.add_argument('--shortest', type=float, default=15.0)
    parser.add_argument('--from', dest='start', type=float, default=None)
    parser.add_argument('--to', dest='end', type=float, default=None)
    args = parser.parse_args(argv)

    rate, data = load(args.wav)
    sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

    env, ms_per = envelope(rate, data, args.pitch, args.integrator)
    on, cut = key_states(env, args.setback)
    r = runs(on, ms_per, args.shortest)
    marked = stamped(r)

    print('file       %s' % os.path.basename(args.wav))
    print('pitch      %.1f Hz   integrator %.0f ms   threshold p98 - %.0f dB '
          '(= %.1f dB)   shortest run %.0f ms'
          % (args.pitch, args.integrator, args.setback, cut, args.shortest))
    print('reads      %s' % spell(r))
    print()
    print('%-9s %-5s %8s' % ('at (s)', 'state', 'ms'))

    shown = 0
    for at_ms, value, ms in marked:
        seconds = at_ms / 1000.0
        if args.start is not None and seconds + ms / 1000.0 < args.start:
            continue
        if args.end is not None and seconds > args.end:
            continue
        print('%9.3f %-5s %8.0f' % (seconds, 'MARK' if value else 'gap', ms))
        shown += 1

    print()
    print('%d runs shown of %d' % (shown, len(marked)))
    return 0


if __name__ == '__main__':
    sys.exit(main())

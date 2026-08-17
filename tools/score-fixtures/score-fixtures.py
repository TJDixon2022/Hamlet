# -*- coding: utf-8 -*-
"""Score every rebuilt CW fixture with the validated reference decoder.

HM-OPEN-018 phase 4. **No rebuilt fixture may judge Hamlet until cwdecoder.py
has scored well on it.** A fixture the reference cannot decode is a bad fixture,
not a Hamlet failure, and before this there was no way to tell those apart --
the old noiseless fixtures certified a decoder that could not read a real
signal, and the reference scores zero on every one of them.

The score is written into each fixture's own sidecar, so it travels with the
fixture and a test can insist it is there.

Run from the repository root, after the fixtures have been generated:

    python tools/score-fixtures/score-fixtures.py
"""
import io
import os
import re
import subprocess
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
FIXTURES = os.path.join(ROOT, 'tests', 'fixtures', 'cw', 'receiver')
REFERENCE = os.path.join(ROOT, 'cwdecoder.py')


class ReferenceFailed(Exception):
    """The reference could not be run at all, which is not a bad score.

    Kept as its own exception so the two can never be conflated. A decoder that
    read nothing is a statement about the fixture; a decoder that crashed is a
    statement about the tooling, and last time the second was written into the
    sidecars as the first.
    """


def expected_of(sidecar):
    """What the fixture says was sent, with prosign carets removed."""
    for line in sidecar.split('\n'):
        if line.startswith('text '):
            return line.split(None, 1)[1].strip().replace('^', '')
    return ''


def score(decoded, expected):
    """How much of the message came back, ignoring spacing.

    Deliberately crude. This is a gate on whether a fixture is decodable at
    all, not a measure of decoder quality, and a precise metric here would
    invite tuning the fixtures to it.
    """
    a = re.sub(r'[^A-Z0-9/?]', '', decoded.upper())
    b = re.sub(r'[^A-Z0-9/?]', '', expected.upper())

    if not b:
        return 0.0

    # Longest common subsequence over the two strings.
    previous = [0] * (len(b) + 1)
    for i in range(1, len(a) + 1):
        current = [0] * (len(b) + 1)
        for j in range(1, len(b) + 1):
            current[j] = (previous[j - 1] + 1 if a[i - 1] == b[j - 1]
                          else max(previous[j], current[j - 1]))
        previous = current

    return previous[len(b)] / len(b)


def run_reference(wav):
    """What cwdecoder.py makes of one file."""
    # **THE CHILD MUST BE TOLD TO SPEAK UTF-8.** The reference prints a box
    # character for anything it could not resolve, and on a Windows console
    # codepage that kills the process part-way through its own output. The
    # scorer then reported "read nothing" for fixtures the reference had in
    # fact decoded almost perfectly, which would have condemned good fixtures
    # as bad ones -- the gate failing in the direction that destroys evidence.
    environment = dict(os.environ, PYTHONIOENCODING='utf-8')

    result = subprocess.run(
        [sys.executable, REFERENCE, wav],
        capture_output=True, timeout=600, env=environment)

    output = result.stdout.decode('utf-8', errors='replace')
    errors = result.stderr.decode('utf-8', errors='replace').strip()

    # **THE GATE MAY NOT FAIL SILENTLY.** It already did once, in the direction
    # that destroys evidence: the reference prints a box for anything it could
    # not resolve, that killed the child on a Windows console codepage, and
    # fixtures it had decoded almost perfectly were written down as unreadable.
    # A crash is now its own outcome and can never be mistaken for a decoder
    # that read nothing, because those two say opposite things about a fixture.
    if result.returncode != 0 or errors:
        raise ReferenceFailed(
            'the reference exited %d on %s%s' % (
                result.returncode, os.path.basename(wav),
                ': ' + errors.splitlines()[-1] if errors else ''))

    if 'DECODE' not in output:
        for phrase in ('no keyed tone found', 'do not cluster as Morse'):
            if phrase in output:
                return None, phrase
        return None, 'produced no decode line'

    line = [l for l in output.split('\n') if 'DECODE' in l][0]
    return line.split(':', 1)[1].strip(), None


def main():
    if not os.path.isdir(FIXTURES):
        print('no fixtures generated yet: ' + FIXTURES)
        return 1

    names = sorted(
        f[:-4] for f in os.listdir(FIXTURES) if f.endswith('.wav'))

    if not names:
        print('no fixtures found in ' + FIXTURES)
        return 1

    failures = 0
    crashes = 0

    for name in names:
        wav = os.path.join(FIXTURES, name + '.wav')
        notes = os.path.join(FIXTURES, name + '.txt')

        sidecar = io.open(notes, encoding='utf-8').read()
        expected = expected_of(sidecar)

        try:
            decoded, refusal = run_reference(wav)
        except (ReferenceFailed, subprocess.TimeoutExpired) as failure:
            # Written into the sidecar as what it is, and counted separately, so
            # a broken gate cannot look like a set of bad fixtures.
            summary = 'reference    COULD NOT BE RUN: %s' % failure
            crashes += 1
            kept = '\n'.join(
                l for l in sidecar.split('\n')
                if not l.startswith('reference'))
            io.open(notes, 'w', encoding='utf-8', newline='\n').write(
                kept.rstrip() + '\n' + summary + '\n')
            print('%-20s   ----  GATE FAILED: %s' % (name, failure))
            continue

        if decoded is None:
            summary = 'reference    read nothing (%s)' % refusal
            got = 0.0
        else:
            got = score(decoded, expected)
            summary = 'reference    %.0f%% of the message  [%s]' % (
                got * 100, decoded)

        # Replace any previous score rather than stacking them up.
        kept = '\n'.join(
            l for l in sidecar.split('\n')
            if not l.startswith('reference'))

        io.open(notes, 'w', encoding='utf-8', newline='\n').write(
            kept.rstrip() + '\n' + summary + '\n')

        edge = 'snrDb         0.0 dB' in sidecar
        verdict = 'edge tier, refusal is correct' if edge else (
            'ok' if got >= 0.5 else 'BAD FIXTURE')

        if not edge and got < 0.5:
            failures += 1

        print('%-20s %5.0f%%  %s' % (name, got * 100, verdict))

    print()
    print('%d fixture(s) the reference could not read above the edge tier'
          % failures)

    if crashes:
        print('%d fixture(s) the gate could not be run on at all -- this is a'
              ' tooling failure and says nothing about those fixtures' % crashes)

    return 1 if (failures or crashes) else 0


if __name__ == '__main__':
    sys.exit(main())

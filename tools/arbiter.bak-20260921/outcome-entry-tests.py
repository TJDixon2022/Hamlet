# -*- coding: utf-8 -*-
"""The tests for `outcome-entry.py`, run one at a time by exact name.

    python tools/arbiter/outcome-entry-tests.py <ExactName>

WHY A PYTHON TEST AND NOT A dotnet ONE. The thing under test is a Python
script that writes a Markdown file. There is no .NET assembly it could be
reached through, and PHASE_PLAN.md's rule is that a unit runs only the test
it constructs in that instruction, by exact name, in the foreground - which
this obeys: a name is REQUIRED, there is no run-everything mode, and each
case is a separate process from every other.

WHAT BREAKAGE THESE WOULD HAVE CAUGHT, which PHASE_PLAN.md requires a new
test to name.

    `docs/phase-send-run/PHASE_OUTCOME.md` records every unit of the send
    phase TWICE. `## UNIT 262 - STEP 3` and `## UNIT 5 - STEP 3` are one
    unit, and so are 253/1, 254/2, 255/3, 256/4, 260/3, 261/4, 263/6,
    264/7 and 265/8. Thirteen units, twenty-six entries, and a reader
    counting entries counts the phase's work at twice its size.

    The cause is that `outcome-append.bat` takes the unit number from its
    caller and its two callers disagree about what a unit number is:
    `run-unit.bat:534` passes `%UNIT%`, the work-instruction number, and
    `run-phase.bat:373` passes `%ITER%`, the loop's iteration counter, set
    to 0 at `run-phase.bat:127` and incremented at `:171`. Both fire during
    the same run.

    No test existed for any of it, because nothing in the repository
    exercised `outcome-entry.py` at all. These run it as the launcher runs
    it - a real process, the values in the environment - and assert on the
    bytes it leaves behind.

Every case builds its own throwaway root under a temporary directory and
touches nothing in the tree.
"""
import os
import re
import shutil
import subprocess
import sys
import tempfile

HERE = os.path.dirname(os.path.abspath(__file__))
ENTRY = os.path.join(HERE, 'outcome-entry.py')

# The header an outcome file starts life with, trimmed to what the writer
# and the reader both actually anchor on.
HEADER = (
    '# PHASE_OUTCOME.md\r\n'
    '\r\n'
    '## PHASE\r\n'
    '\r\n'
    'PHASE: a phase under test\r\n'
    'PHASE_SET: 2026-09-07\r\n'
    'STEP: A | not started | the row knows where the contact stands\r\n'
    '\r\n'
    '---\r\n'
    '\r\n'
    '## Entries\r\n'
)

FIELDS = {
    'OA_APPROACH': 'an approach',
    'OA_HIT': 'what it hit',
    'OA_MOVE': 'continue',
    'OA_WHY': 'because',
    'OA_DECIDED': 'none',
    'OA_LICENCE': 'none',
    'OA_COST': 'unknown',
    'OA_ACCOMPLISHED': 'what the step accomplished',
    'OA_FATE': 'executed',
    'OA_STATE': 'partial',
    'OA_STATEWHY': 'not recorded',
}


class Failed(Exception):
    """One assertion said no."""


def check(condition, message):
    if not condition:
        raise Failed(message)


def make_root(instruction='# Work instruction 266 - the record is honest\r\n'):
    """A throwaway repository root with an outcome file and an instruction."""
    root = tempfile.mkdtemp(prefix='outcome-entry-test-')
    outcome = os.path.join(root, 'PHASE_OUTCOME.md')

    with open(outcome, 'wb') as handle:
        handle.write(HEADER.encode('ascii'))

    if instruction is not None:
        with open(os.path.join(root, 'WORK_INSTRUCTIONS.md'), 'wb') as handle:
            handle.write(instruction.encode('ascii'))

    return root, outcome


def append(root, outcome, unit, step, **overrides):
    """Run the writer exactly as the launcher runs it: a process, and the
    values in the environment."""
    env = dict(os.environ)
    env.update(FIELDS)
    env.update(overrides)
    env['OA_UNIT'] = unit
    env['OA_STEP'] = step
    env['OA_ROOT'] = root

    run = subprocess.run(
        [sys.executable, ENTRY, outcome],
        env=env, capture_output=True, text=True, timeout=60)

    check(run.returncode == 0,
          'the writer exited %d: %s' % (run.returncode, run.stderr))


def read(outcome):
    with open(outcome, 'rb') as handle:
        return handle.read().decode('ascii')


def headings(text):
    return [line for line in text.splitlines() if line.startswith('## UNIT ')]


# --------------------------------------------------------------------------
# THE CASE THIS UNIT WAS WRITTEN FOR.
# --------------------------------------------------------------------------
def OneUnitAppendedTwiceUnderBothNumberingRoutesProducesOneEntry():
    """Work instruction 266, appended once as unit 266 by `run-unit.bat`'s
    route and once as unit 9 by `run-phase.bat`'s iteration counter, is one
    unit and gets one entry."""
    root, outcome = make_root()

    try:
        # The unit's own append, by its work-instruction number.
        append(root, outcome, '266', 'A')

        # The loop's append, moments later, by the iteration counter. Same
        # run, same unit, same step - and the arbiter's own judgment fields,
        # which is what makes this second call worth keeping the facts of.
        append(root, outcome, '9', 'A',
               OA_COST='13.1356565', OA_HIT='section 4 wants a ruling: no')

        text = read(outcome)
        found = headings(text)

        check(len(found) == 1,
              'expected ONE entry heading, got %d: %r' % (len(found), found))
        check(found[0] == '## UNIT 266 - STEP A',
              'the one entry should be under the work-instruction number, '
              'not the iteration number: %r' % found[0])
        check(not any('UNIT 9' in line for line in found),
              'the iteration number should not head an entry: %r' % found)

        # AND IT IS STILL ON THE FACE OF THE RECORD. The number the caller
        # asked for is what the launcher printed on screen, and a reader who
        # goes looking for it should find it said rather than find nothing.
        check('called as UNIT 9' in text,
              'the number the second route asked for is not recorded anywhere')

        # NOTHING THE SECOND ROUTE RECORDED IS LOST. It is the route that
        # carries the run's real cost and the arbiter's judgment, and an
        # entry that silently dropped them would be a worse record than the
        # duplicate it replaces.
        check('13.1356565' in text,
              "the second route's cost is not in the file")
        check('section 4 wants a ruling: no' in text,
              "the second route's HIT is not in the file")
    finally:
        shutil.rmtree(root, ignore_errors=True)


def TheSameUnitOnADifferentStepIsStillItsOwnEntry():
    """One unit may advance two steps and each gets its own entry - `UNIT 253
    - STEP 0` and `UNIT 253 - STEP 1` are two facts, not a duplicate. The
    de-duplication must not swallow this."""
    root, outcome = make_root(
        '# Work instruction 253 - the dummy load is gone\r\n')

    try:
        append(root, outcome, '253', '0')
        append(root, outcome, '253', '1')

        found = headings(read(outcome))

        check(found == ['## UNIT 253 - STEP 0', '## UNIT 253 - STEP 1'],
              'two steps of one unit should be two entries: %r' % found)
    finally:
        shutil.rmtree(root, ignore_errors=True)


def AnEntryWrittenWithNoWorkInstructionToResolveAgainstKeepsItsNumber():
    """The resolution is a tie-break for the launcher's two routes and never
    a takeover. With no `WORK_INSTRUCTIONS.md` to read - appending an old
    entry by hand, or a repository that has none - the number the caller
    gave is the number written."""
    root, outcome = make_root(instruction=None)

    try:
        append(root, outcome, '241', 'A')

        found = headings(read(outcome))

        check(found == ['## UNIT 241 - STEP A'],
              'the caller\'s number should stand: %r' % found)
    finally:
        shutil.rmtree(root, ignore_errors=True)


CASES = {
    name: value for name, value in list(globals().items())
    if callable(value) and re.match(r'^[A-Z]', name)
}


def main(argv):
    if len(argv) != 1 or argv[0] not in CASES:
        sys.stderr.write(
            'Run one case by its exact name:\n\n'
            + ''.join('    %s\n' % name for name in sorted(CASES))
            + '\nThere is deliberately no run-everything mode.\n')
        return 2

    name = argv[0]

    try:
        CASES[name]()
    except Failed as failure:
        sys.stdout.write('FAILED  %s\n        %s\n' % (name, failure))
        return 1
    except Exception as error:  # noqa: BLE001 - a broken case is a red too
        sys.stdout.write('ERROR   %s\n        %r\n' % (name, error))
        return 1

    sys.stdout.write('PASSED  %s\n' % name)
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))

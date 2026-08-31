# -*- coding: utf-8 -*-
"""Write PROJECT_STATUS.md, CLAUDE.md 13.1.

Why this exists rather than write-status.bat: that script writes six lines --
STATE, PHASE, BALL, NEXT_PASTE, UPDATED, NOTE -- and the file in the tree
carries eight, PROJECT and TASK among them. Running it would silently drop two
fields the panel reads, and drop the prose below the separator with them.

The three things it makes impossible are kept:

  1. A zero-byte result. Content is built in a temp file and moved over the
     real one only after it is verified non-empty.
  2. A typed timestamp. UPDATED is read from the clock here and cannot be
     passed in.
  3. An invented STATE or BALL. Both are checked against the lists in
     CLAUDE.md 13.1 and the script refuses.

Usage, from the repository root:

    python tools/write-status.py STATE PHASE TASK BALL NEXT_PASTE "NOTE"

Example:

    python tools/write-status.py EXECUTING 12 "2 of 6" code none \
      "Task 2 - per-element pitch, measuring against the corpus"
"""
import datetime
import os
import sys

STATES = (
    'PREPARING_PROMPT',
    'ANSWERING_QUESTIONS',
    'EXECUTING',
    'COMPLETED',
    'BLOCKED',
)

BALLS = ('code', 'web', 'tim', 'unassigned')

PROSE = """
Written by a Claude Code session per CLAUDE.md 13. Protocol 2 is not in this
repository, so the header names which protocol this is written against rather
than conformance anybody here can check.
"""


def main(argv):
    if len(argv) != 6:
        sys.stderr.write(__doc__)
        return 1

    state, phase, task, ball, paste, note = argv

    if state not in STATES:
        sys.stderr.write(
            'ERROR: STATE was "%s".\nMust be one of: %s\n'
            'Nothing written. The previous status still stands.\n'
            % (state, ' '.join(STATES)))
        return 1

    if ball not in BALLS:
        sys.stderr.write(
            'ERROR: BALL was "%s".\nMust be one of: %s\n'
            'Nothing written. The previous status still stands.\n'
            % (ball, ' '.join(BALLS)))
        return 1

    # Read from the clock, with the local UTC offset. Never typed.
    updated = datetime.datetime.now().astimezone().isoformat(timespec='seconds')

    body = (
        'PROJECT: Hamlet\n'
        'STATE: %s\n'
        'PHASE: %s\n'
        'TASK: %s\n'
        'BALL: %s\n'
        'NEXT_PASTE: %s\n'
        'UPDATED: %s\n'
        'NOTE: %s\n'
        '\n---\n%s' % (state, phase, task, ball, paste, updated, note, PROSE))

    root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    out = os.path.join(root, 'PROJECT_STATUS.md')
    tmp = out + '.tmp'

    with open(tmp, 'w', encoding='utf-8', newline='\n') as handle:
        handle.write(body)

    if os.path.getsize(tmp) == 0:
        os.remove(tmp)
        sys.stderr.write(
            'ERROR: temp file is zero bytes. PROJECT_STATUS.md is untouched.\n')
        return 1

    os.replace(tmp, out)
    sys.stdout.write(body)
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))

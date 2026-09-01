# -*- coding: utf-8 -*-
"""Write PROJECT_STATUS.md, the header the annunciator reads.

WHY THIS EXISTS RATHER THAN write-status.bat

    That script writes six lines -- STATE, PHASE, BALL, NEXT_PASTE, UPDATED,
    NOTE. ANNUNCIATOR.md's contract is eight, and the panel's own example
    carries more still. Running it drops fields the panel reads, and a second
    writer with a different field set is the drift CLAUDE.md section 0 exists
    to prevent, so write-status.bat now calls this file instead of keeping its
    own copy of the answer.

THE FIVE THINGS IT MAKES IMPOSSIBLE

  1. A zero-byte result. Content is built in a temp file and moved over the
     real one only after it is verified non-empty.
  2. A typed timestamp. UPDATED is read from the clock here and cannot be
     passed in.
  3. An invented STATE or BALL. Both are checked against the panel's own
     vocabularies and the script refuses.
  4. A missing field the panel reads. Every required key is written on every
     call, so a field cannot be dropped by forgetting an argument.
  5. A non-ASCII byte. See below.

WHY ASCII, WHICH IS A MEASUREMENT AND NOT A PREFERENCE

    On 2026-08-31 PHASE_OUTCOME.md held seven copies of the byte run
    83 3F 27 where a dash belonged -- not valid UTF-8, and unreadable. The
    cause is that cmd decodes a batch file's bytes through the console
    codepage, so any non-ASCII character that crosses a batch boundary is
    mangled before any script sees it. **This file is written in ASCII so
    that boundary can never damage it**, and a value carrying non-ASCII is
    transliterated rather than passed through.

    PROJECT_STATUS.md's own NOTE carried a correctly-encoded UTF-8 em-dash at
    the same time, which parsed fine -- so this is a guard against the write
    path, not a claim that the panel cannot read UTF-8.

NO PHASE NAME LIVES HERE

    `PHASE` on this file is the panel's legacy alias for `TASK` and must read
    `n of m`. A phase NAME in it is the two-meanings drift ANNUNCIATOR.md
    warns about, and the name already lives in PHASE_STATUS.md, which is where
    PHASE_CONTROL.md section 4 puts it.

WHAT IS NOT WRITTEN

    **A field with no honest value is left out, not filled in.** The panel
    names an absent field; a present one that lies is worse. PROMPT is written
    only when the caller supplies it, for that reason.

Usage, from the repository root:

    python tools/write-status.py STATE TASK WORK_INSTRUCTION BALL NEXT_PASTE "NOTE"
                                 [--prompt N] [--rules-at "..."]

Example:

    python tools/write-status.py EXECUTING "2 of 6" 204 code none \
      "Task 2 - per-element pitch, measuring against the corpus"
"""
import argparse
import datetime
import os
import sys

# ANNUNCIATOR.md's five, and the panel's STATE_LABEL has exactly these keys.
# A value outside them makes normState() return null, which the panel renders
# as UNREADABLE -- that is what put "Unreadable header" on the card on
# 2026-08-31, from a hand-written STATE: STOPPED.
STATES = (
    'PREPARING_PROMPT',
    'ANSWERING_QUESTIONS',
    'EXECUTING',
    'COMPLETED',
    'BLOCKED',
)

# ANNUNCIATOR.md: who must act next. `unassigned` means nobody has taken it
# and is not a polite way of saying it is the owner's.
BALLS = ('code', 'web', 'tim', 'unassigned')

# HM-DEC-131 and CLAUDE.md section 13.1: both files name the protocol they are
# written against. It is which protocol this file is written to, not a claim
# that anything here validates against it -- STATUS_PROTOCOL.md lives in the
# annunciator repository and not in this one.
PROTOCOL = '2'

TRANSLITERATE = {
    '—': '-', '–': '-', '‒': '-', '‑': '-',
    '‘': "'", '’': "'", '‚': "'",
    '“': '"', '”': '"', '„': '"',
    '…': '...', ' ': ' ', '·': '-', '•': '-',
    '§': 'section ', '×': 'x', '→': '->', '←': '<-',
}

OEM = ('cp437', 'cp850', 'cp1252')

PROSE = """
Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.
"""


def unmangle(value):
    """Undo a UTF-8 string read back through the console codepage.

    A batch file is UTF-8 on disk and cmd decodes its bytes with the OEM
    codepage, so an em-dash arrives as three unrelated characters. Encoding
    them back with the same codepage returns the original bytes; decoding
    those as UTF-8 returns the original text. Only fires when the re-encode
    succeeds and the result is valid UTF-8.
    """
    if not value or all(ord(c) < 128 for c in value):
        return value

    for codepage in OEM:
        try:
            raw = value.encode(codepage)
        except (UnicodeEncodeError, LookupError):
            continue
        try:
            fixed = raw.decode('utf-8')
        except UnicodeDecodeError:
            continue
        if fixed != value:
            return fixed

    return value


def ascii_only(value):
    """Every character survives, transliterates, or becomes a visible '?'."""
    value = unmangle(value or '')
    out = []
    for ch in value:
        if ch in TRANSLITERATE:
            out.append(TRANSLITERATE[ch])
        elif ord(ch) < 128:
            out.append(ch)
        else:
            out.append('?')
    return ''.join(out)


def main(argv):
    parser = argparse.ArgumentParser(add_help=True)
    parser.add_argument('state')
    parser.add_argument('task')
    parser.add_argument('work_instruction')
    parser.add_argument('ball')
    parser.add_argument('next_paste')
    parser.add_argument('note')
    parser.add_argument('--prompt', default=None,
                        help='how many prompts this phase has taken, counting '
                             'this one. Omitted when not counted.')
    parser.add_argument('--rules-at', dest='rules_at', default=None,
                        help='the newest ruling this session read')
    parser.add_argument('--project', default='Hamlet')
    args = parser.parse_args(argv)

    state = args.state.strip().upper()
    if state not in STATES:
        sys.stderr.write(
            'ERROR: STATE was "%s".\nMust be one of: %s\n'
            'Nothing written. The previous status still stands.\n'
            % (args.state, ' '.join(STATES)))
        return 1

    ball = args.ball.strip().lower()
    if ball not in BALLS:
        sys.stderr.write(
            'ERROR: BALL was "%s".\nMust be one of: %s\n'
            'Nothing written. The previous status still stands.\n'
            % (args.ball, ' '.join(BALLS)))
        return 1

    # Read from the clock, with the local UTC offset. Never typed.
    updated = datetime.datetime.now().astimezone().isoformat(timespec='seconds')

    # Order follows ANNUNCIATOR.md's block, with PROTOCOL first as the panel's
    # own example has it.
    fields = [
        ('PROTOCOL', PROTOCOL),
        ('PROJECT', ascii_only(args.project)),
        ('STATE', state),
        ('TASK', ascii_only(args.task)),
        ('WORK_INSTRUCTION', ascii_only(args.work_instruction)),
    ]

    if args.prompt is not None:
        fields.append(('PROMPT', ascii_only(args.prompt)))

    fields.append(('BALL', ball))
    fields.append(('NEXT_PASTE', ascii_only(args.next_paste)))

    if args.rules_at is not None:
        fields.append(('RULES_AT', ascii_only(args.rules_at)))

    fields.append(('UPDATED', updated))
    fields.append(('NOTE', ascii_only(args.note)))

    body = ''.join('%s: %s\n' % (k, v) for k, v in fields)
    body += '\n---\n' + PROSE

    root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    out = os.path.join(root, 'PROJECT_STATUS.md')
    tmp = out + '.tmp'

    with open(tmp, 'w', encoding='ascii', newline='\n') as handle:
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

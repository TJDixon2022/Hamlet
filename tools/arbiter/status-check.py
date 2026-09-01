# -*- coding: utf-8 -*-
"""Validate PROJECT_STATUS.md and PHASE_STATUS.md, and print every check.

WHY IT EXISTS

    Every fault these two files have had in this project came from a different
    session writing prose by hand. There is no single writer to harden, so the
    guard has to sit downstream of all of them and run after every unit.

    Measured faults it is built against, all of them real in this tree:

      * `STATE: STOPPED` - not one of the panel's five words, which made the
        whole card read UNREADABLE HEADER while every byte in the file was
        valid.
      * `UPDATED` two hours ahead of the clock, from a session composing seven
        consecutive timestamps instead of reading one.
      * seven copies of the byte run 83 3F 27 in PHASE_OUTCOME.md, which is
        not valid UTF-8 at all.
      * a UTF-8 BOM in front of `PHASE:` on line 1, which stops a parser
        anchored at line start from matching the first key.
      * PROTOCOL and WORK_INSTRUCTION simply absent.

WHAT IT DOES NOT DO

    **It does not halt the phase.** A failing check is recorded and named so
    the fault reaches the owner attached to the unit that caused it. A status
    file is a report about work, not the work; refusing to continue because a
    caption is wrong would throw away a good unit.

EXIT CODES

    0   every check passed
    1   at least one check failed
    2   usage, or the repository root does not exist

Usage:

    python tools/arbiter/status-check.py [root]
"""
import datetime
import os
import re
import sys

# The panel's own STATE_LABEL keys. A value outside them makes normState()
# return null and the card renders UNREADABLE.
STATES = ('PREPARING_PROMPT', 'ANSWERING_QUESTIONS', 'EXECUTING',
          'COMPLETED', 'BLOCKED')

BALLS = ('code', 'web', 'tim', 'unassigned')

PHASE_STATES = ('not started', 'in progress', 'partial', 'blocked', 'done')

# Required of PROJECT_STATUS.md by ANNUNCIATOR.md's block, plus PROTOCOL,
# which HM-DEC-131 and CLAUDE.md 13.1 require of both files.
PROJECT_REQUIRED = ('PROTOCOL', 'STATE', 'TASK', 'WORK_INSTRUCTION',
                    'BALL', 'NEXT_PASTE', 'UPDATED', 'NOTE')

# PHASE_CONTROL.md section 4. HEARTBEAT is the launcher's and is deliberately
# not required: a reader finding it absent is reading a loop that has not
# written one, which is a fact about the loop and not a fault in the file.
PHASE_REQUIRED = ('PHASE', 'PHASE_SET', 'DESCRIPTION', 'CURRENT_STEP',
                  'WORK_INSTRUCTION')

# How far ahead of the clock is tolerated. Zero would fail on the second a
# session writes the file and the check reads it back.
FUTURE_GRACE = datetime.timedelta(minutes=2)


class Report(object):
    """Every check prints, passed or failed."""

    def __init__(self):
        self.failed = 0
        self.passed = 0

    def check(self, name, ok, detail=''):
        if ok:
            self.passed += 1
            print('    PASS  %-46s %s' % (name, detail))
        else:
            self.failed += 1
            print('    FAIL  %-46s %s' % (name, detail))
        return ok

    def note(self, name, detail):
        print('    ----  %-46s %s' % (name, detail))


def header_of(text):
    """The leading run of KEY: value lines, the panel's own parse rule.

    Stops at the first line that is not a key line. Prose below is ignored,
    which is what lets a file keep notes without breaking.
    """
    fields = {}
    order = []
    for line in text.split('\n'):
        line = line.rstrip('\r')
        m = re.match(r'^([A-Za-z][A-Za-z0-9_]*)\s*:\s*(.*)$', line)
        if not m:
            break
        key = m.group(1).upper()
        if key not in fields:
            order.append(key)
        fields.setdefault(key, []).append(m.group(2).strip())
    return fields, order


def parse_when(raw):
    """ISO 8601 with or without an offset. None where it will not parse."""
    if not raw:
        return None
    value = raw.strip().replace('Z', '+00:00')
    try:
        return datetime.datetime.fromisoformat(value)
    except ValueError:
        pass
    for form in ('%Y-%m-%d %H:%M:%S', '%Y-%m-%d %H:%M', '%Y-%m-%dT%H:%M:%S'):
        try:
            return datetime.datetime.strptime(raw.strip(), form)
        except ValueError:
            continue
    return None


def transport(report, path, raw):
    """BOM, encoding and stray bytes. The checks a parser trips over first."""
    label = os.path.basename(path)

    report.check(
        '%s: no byte-order mark' % label,
        raw[:3] != b'\xef\xbb\xbf',
        'a BOM before the first key stops ^KEY: matching')

    try:
        text = raw.decode('utf-8')
        decoded = True
    except UnicodeDecodeError as exc:
        decoded = False
        text = raw.decode('utf-8', 'replace')
    report.check('%s: decodes as UTF-8' % label, decoded,
                 '' if decoded else 'invalid bytes in the file')

    # Mojibake: the signatures this project has actually carried, plus any
    # replacement character, which means something already lost a decode.
    marks = []
    if b'\x83?\'' in raw:
        marks.append('83 3F 27')
    for bad in ('\ufffd', 'â€”', 'â€™', 'Â§', 'Ã©'):
        if bad in text:
            marks.append(repr(bad))
    report.check('%s: no mojibake' % label, not marks,
                 ('found ' + ', '.join(marks)) if marks else '')

    report.check(
        '%s: no stray carriage return' % label,
        raw.count(b'\r') == raw.count(b'\r\n'),
        'a lone CR splits a line for some readers')

    return text


def updated_check(report, label, fields):
    """UPDATED present, parseable, and NOT ahead of the machine clock."""
    values = fields.get('UPDATED')
    if not report.check('%s: UPDATED present' % label, bool(values)):
        return

    raw = values[0]
    when = parse_when(raw)
    if not report.check('%s: UPDATED parses' % label, when is not None, raw):
        return

    now = datetime.datetime.now().astimezone()
    if when.tzinfo is None:
        now = now.replace(tzinfo=None)

    ahead = when - now
    report.check(
        '%s: UPDATED not ahead of the clock' % label,
        ahead <= FUTURE_GRACE,
        ('%s is %s ahead' % (raw, ahead)) if ahead > FUTURE_GRACE
        else ('%s' % raw))


def required_check(report, label, fields, required):
    for key in required:
        report.check('%s: %s present' % (label, key),
                     bool(fields.get(key)),
                     '' if fields.get(key) else 'absent')


def duplicate_check(report, label, fields, repeats=()):
    """A duplicate key is a silent override, except where one is the format.

    Measured on the panel: a second `STATE:` line overrode the first and the
    card showed BLOCKED for a file whose first STATE said EXECUTING, with
    nothing on the face saying a choice had been made.

    **`STEP:` in PHASE_STATUS.md is not that.** PHASE_CONTROL.md section 4
    specifies one STEP line per step, repeated, so flagging it would be the
    check misreading the format as a fault - which it did on its first run.
    """
    dupes = [k for k, v in fields.items() if len(v) > 1 and k not in repeats]
    report.check('%s: no duplicate keys' % label, not dupes,
                 ('last one wins: ' + ', '.join(dupes)) if dupes else
                 ('STEP repeats by design' if repeats else ''))


def check_project(report, root):
    path = os.path.join(root, 'PROJECT_STATUS.md')
    label = 'PROJECT_STATUS.md'
    print('  %s' % label)
    if not report.check('%s: exists' % label, os.path.exists(path)):
        return
    raw = open(path, 'rb').read()
    text = transport(report, path, raw)
    fields, _ = header_of(text)
    required_check(report, label, fields, PROJECT_REQUIRED)
    duplicate_check(report, label, fields)
    updated_check(report, label, fields)

    state = (fields.get('STATE') or [''])[0]
    report.check('%s: STATE is one of the five' % label,
                 state in STATES, state or 'absent')

    ball = (fields.get('BALL') or [''])[0].lower()
    report.check('%s: BALL is known' % label, ball in BALLS, ball or 'absent')

    task = (fields.get('TASK') or [''])[0]
    ok = bool(re.search(r'\d+\s*(of|/)\s*\d+', task)) or task.strip() in ('-', '')
    report.check('%s: TASK reads n of m' % label, ok, task)


def check_phase(report, root):
    path = os.path.join(root, 'PHASE_STATUS.md')
    label = 'PHASE_STATUS.md'
    print('  %s' % label)
    if not os.path.exists(path):
        # A project not under phase control simply has no file. That is the
        # degraded case PHASE_CONTROL.md section 4 names, and it is not a fault.
        report.note('%s: absent' % label,
                    'not under phase control - nothing to check')
        return
    report.check('%s: exists' % label, True)
    raw = open(path, 'rb').read()
    text = transport(report, path, raw)
    fields, _ = header_of(text)
    required_check(report, label, fields, PHASE_REQUIRED)
    duplicate_check(report, label, fields, repeats=('STEP',))
    if fields.get('UPDATED'):
        updated_check(report, label, fields)

    steps = []
    for line in text.split('\n'):
        m = re.match(r'^STEP:\s*(\d+)\s*\|\s*([^|]*?)\s*\|', line.rstrip('\r'))
        if m:
            steps.append((int(m.group(1)), m.group(2)))
        elif not re.match(r'^([A-Za-z][A-Za-z0-9_]*)\s*:', line.rstrip('\r')):
            break

    report.check('%s: has at least one STEP line' % label, bool(steps),
                 '%d found' % len(steps))

    bad = [s for s in steps if s[1] not in PHASE_STATES]
    report.check('%s: every STEP state is known' % label, not bad,
                 ('unknown: ' + ', '.join('%d=%s' % b for b in bad)) if bad else '')

    cur = (fields.get('CURRENT_STEP') or [''])[0]
    numbers = [n for n, _ in steps]
    ok = cur.isdigit() and int(cur) in numbers
    report.check('%s: CURRENT_STEP names a real step' % label, ok,
                 ('%s, steps are %s' % (cur or 'absent', numbers)) if not ok else cur)

    if steps and all(s[1] == 'done' for s in steps):
        report.check('%s: all done, CURRENT_STEP is the highest' % label,
                     cur.isdigit() and int(cur) == max(numbers),
                     '%s vs %s' % (cur, max(numbers)))


def main(argv):
    root = argv[0] if argv else r'C:\Source\HamLet'
    if not os.path.isdir(root):
        sys.stderr.write('ERROR: no such repository root: %s\n' % root)
        return 2

    print('============================================================')
    print(' status-check')
    print('   root : %s' % root)
    print('============================================================')
    print('')

    report = Report()
    check_project(report, root)
    print('')
    check_phase(report, root)
    print('')
    print('  %d passed, %d failed' % (report.passed, report.failed))
    print('')
    if report.failed:
        print('  status-check FAILED - the phase is NOT halted. The fault is')
        print('  recorded against the unit that caused it.')
    return 1 if report.failed else 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))

# -*- coding: utf-8 -*-
"""Append one PHASE_OUTCOME.md entry, in ASCII, with no byte-order mark.

WHY THIS EXISTS, AND IT IS A MEASUREMENT RATHER THAN A PREFERENCE

    `outcome-append.bat` used to write the entry with

        >>"%FILE%" echo WHY: %WHY%

    and cmd's `echo` emits bytes in the CONSOLE's active codepage, not in
    the file's. On 2026-08-31 PHASE_OUTCOME.md held SEVEN copies of the
    byte run 83 3F 27 where a dash belonged. That run is not valid UTF-8:
    0x83 is 'a with a circumflex' in CP437 and CP850, 0x3F is the '?' an
    unmappable character becomes, and 0x27 is a best-fit apostrophe. It is
    a UTF-8 punctuation character decoded as CP1252 and re-encoded through
    the OEM codepage, and it is IRREVERSIBLE -- the three surviving bytes
    do not say which character they came from.

    The header write had a second, separate defect: PowerShell 5.1's
    `Set-Content -Encoding utf8` writes UTF-8 WITH a byte-order mark, and
    EF BB BF in front of `PHASE:` on line 1 means a parser anchored on
    `^PHASE:` or on `^[A-Za-z_]` does not match line 1 at all.

WHY PYTHON AND NOT MORE POWERSHELL

    The entry's values are ordinary English and carry quotes, ampersands,
    percent signs and carets. Getting those through
    cmd -> powershell -Command intact needs escaping that has already
    failed twice in this repository, and CLAUDE_CODE.md 11 names composing
    file content inside nested shell quoting as a recurring corruption.
    **The values are passed in the environment**, which carries a string
    across a process boundary untouched, and the file is written by one
    call with the encoding stated explicitly.

WHAT IT GUARANTEES, AND WHAT IT ONLY USUALLY DOES

    **Guaranteed: what lands in the file is valid ASCII with no BOM.** That is
    the property every reader downstream depends on, and it does not depend on
    what the caller sent.

    **Usually: the original character is recovered.** cmd decodes a batch
    file's bytes through the console codepage before any script sees a value,
    so an em-dash arrives as three unrelated characters -- see `unmangle`,
    which reverses that where it can. Where it cannot, the character becomes a
    visible '?' rather than an invalid byte.

    **Callers should still write ASCII in the first place.** A recovery that
    works is not a licence to depend on it.

ONE UNIT, ONE ENTRY, AND WHERE THE SECOND NUMBER CAME FROM

    `outcome-append.bat` takes the unit number from its caller, and its two
    callers disagree about what a unit number IS. `run-unit.bat:534` passes
    `%UNIT%`, which is the work-instruction number. `run-phase.bat:373`
    passes `%ITER%`, which is the loop's iteration counter - set to 0 at
    `run-phase.bat:127` and incremented at `:171`. **Both fire during the
    same run**, so every unit of the send phase landed twice under two
    different numbers: `## UNIT 262 - STEP 3` and `## UNIT 5 - STEP 3` are
    one unit, and so are twelve other pairs.

    Neither caller is wrong about its own number and neither can see the
    other, so the fix is here, at the one place both routes pass through.

    **THE NUMBER IS RESOLVED FROM THE TREE.** The launcher has exactly one
    authoritative answer to *which unit is this* - the instruction it just
    ran - and it is written at the top of `WORK_INSTRUCTIONS.md` as
    `# Work instruction <n> - <title>`, with `PHASE_STATUS.md`'s
    `WORK_INSTRUCTION:` line as the fallback.

    **IT IS A TIE-BREAK AND NOT A TAKEOVER.** With no instruction to read
    against, the caller's number stands. `OA_UNIT_EXACT` turns the
    resolution off outright, for appending an old entry by hand, and where
    the resolved number differs from the one asked for, the entry says so
    on its face in `UNIT_AS_CALLED:` rather than quietly relabelling
    somebody's record.

    **AND A SECOND APPEND FOR THE SAME UNIT AND STEP IS NOT A SECOND
    ENTRY.** It is written into the first entry as a `###` continuation
    naming only the fields whose values differ, so the unit appears once at
    `##` level and nothing either route recorded is lost - the second route
    is the one carrying the run's real cost and the arbiter's judgment.
    Two different STEPS of one unit are two different facts and stay two
    entries: `UNIT 253 - STEP 0` and `UNIT 253 - STEP 1` are not a
    duplicate.

    **NOTHING IS EVER REWRITTEN.** The file is read to find out what is
    already in it; every byte this script emits still goes on the end.

    Tested by `outcome-entry-tests.py`, one case at a time by exact name.

Usage, with every value in the environment as OA_<FIELD>:

    python tools/arbiter/outcome-entry.py <file>
"""
import os
import re
import sys

# Characters worth spelling rather than replacing. Everything else outside
# ASCII becomes '?', which is visible and is never mistaken for a real value.
TRANSLITERATE = {
    '—': '-',    '–': '-',    '‒': '-',   '‑': '-',
    '‘': "'",    '’': "'",    '‚': "'",
    '“': '"',    '”': '"',    '„': '"',
    '…': '...',  ' ': ' ',    '·': '-',   '•': '-',
    '§': 'section ',
    '×': 'x',    '→': '->',   '←': '<-',
    # The mojibake this file exists against, mapped back where it is still
    # distinguishable. `` is what a lone 0x83 decodes to, and the
    # three-character run below is the CP1252 mis-read of a UTF-8 em-dash.
    'â€”': '-',
    '': '-',
}

FIELDS = [
    'STEP', 'APPROACH', 'HIT', 'MOVE', 'WHY', 'DECIDED', 'LICENCE',
    'COST', 'ACCOMPLISHED', 'FATE', 'STATE_AFTER', 'STATE_WHY',
]

# The environment variable each field is read from. STATE_AFTER and
# STATE_WHY do not match their own names, which is why this is a table.
SOURCE = {
    'STATE_AFTER': 'OA_STATE',
    'STATE_WHY': 'OA_STATEWHY',
}


# The codepages a Windows console hands a batch script, most likely first.
OEM = ('cp437', 'cp850', 'cp1252')

# `# Work instruction 266 - the record is honest ...`, and `# Work instruction
# 041 - ...` too: the leading zeroes are dropped so 041 and 41 are one unit.
HEADING = re.compile(r'^#+\s*work\s+instruction\s+0*(\d+)\b', re.IGNORECASE)

# `WORK_INSTRUCTION: 266 - the record is honest ...` in PHASE_STATUS.md.
KEY = re.compile(r'^WORK_INSTRUCTION:\s*0*(\d+)\b')

# `## UNIT 266 - STEP A`. The step is free text - the re-cut phase's steps are
# letters where the previous cut's were digits - so it is matched as text.
ENTRY = re.compile(r'^##\s+UNIT\s+(\S+)\s+-\s+STEP\s+(.+?)\s*$')

FIELD = re.compile(r'^([A-Z][A-Z_]*):\s?(.*)$')


def unmangle(value):
    """Undo a UTF-8 string that was read back through the console codepage.

    **MEASURED, NOT GUESSED.** Feeding `dash - here` with a real em-dash
    through a .bat argument and printing the codepoints on the far side gives
    U+0393 GREEK CAPITAL GAMMA -- which is byte 0xE2 in CP437. The batch file
    is UTF-8 on disk, cmd decodes its bytes with the OEM codepage, and the
    three bytes of an em-dash arrive as three unrelated characters.

    That is lossless while it stays in memory: encoding those characters back
    with the same codepage returns the original bytes, and decoding those as
    UTF-8 returns the original text. So the dash is recovered here rather than
    written out as three question marks.

    **It only fires when it is sure.** The value must contain non-ASCII, the
    re-encode must succeed, and the result must be valid UTF-8. Prose that is
    genuinely in one of these codepages almost never decodes as UTF-8 by
    accident, and where this guesses wrong the output is still ASCII -- the
    cost is a '?' where a '-' belonged, not a corrupt byte.
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
    """Every character either survives, transliterates, or becomes '?'."""
    if value is None:
        return ''

    value = unmangle(value)

    for bad, good in TRANSLITERATE.items():
        if len(bad) > 1:
            value = value.replace(bad, good)

    out = []
    for ch in value:
        if ch in TRANSLITERATE:
            out.append(TRANSLITERATE[ch])
        elif ord(ch) < 128:
            out.append(ch)
        else:
            out.append('?')
    return ''.join(out)


def repository_root():
    """Where to look for the instruction this unit is running.

    `OA_ROOT` first, so a test can point this somewhere harmless. Otherwise
    two directories up from this file, which is the repository root by this
    script's own location and not by the working directory - the launcher
    calls it from wherever the run happened to start.
    """
    override = os.environ.get('OA_ROOT')

    if override:
        return override

    return os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)),
                                        os.pardir, os.pardir))


def read_text(path):
    """The file, or None. Never raises on a file that is not there."""
    try:
        with open(path, 'rb') as handle:
            return handle.read().decode('utf-8', 'replace')
    except OSError:
        return None


def resolve_unit(called, root):
    """Which unit this really is, and what it was called.

    Returns (unit, called). They differ exactly when the caller passed
    something other than the number the tree says is running - which is the
    iteration counter, every time it has happened.
    """
    if os.environ.get('OA_UNIT_EXACT'):
        return called, called

    for name, pattern in (('WORK_INSTRUCTIONS.md', HEADING),
                          ('PHASE_STATUS.md', KEY)):
        text = read_text(os.path.join(root, name))

        if text is None:
            continue

        for line in text.splitlines():
            match = pattern.match(line)

            if match:
                return match.group(1), called

    # NOTHING TO RESOLVE AGAINST, SO THE CALLER'S NUMBER STANDS. Appending an
    # old entry by hand is a real thing to do and this is not the place to
    # start guessing at it.
    return called, called


def existing_entry(path, unit, step):
    """The fields of the last entry already recorded for this unit and step.

    None where there is no such entry. The file is READ here and never
    written back: the duplicate has to be detected before it can be avoided,
    and reading is not rewriting.
    """
    text = read_text(path)

    if text is None:
        return None

    found = None
    fields = None

    for line in text.splitlines():
        heading = ENTRY.match(line)

        if heading:
            if heading.group(1) == unit and heading.group(2) == step:
                fields = {}
                found = fields
            else:
                fields = None

            continue

        if fields is None:
            continue

        field = FIELD.match(line)

        if field:
            fields[field.group(1)] = field.group(2)

    return found


def new_entry(unit, called, step, values):
    """The entry as it has always been written, with one line added."""
    lines = ['', '## UNIT %s - STEP %s' % (unit, step), '']

    # THE SUBSTITUTION IS ON THE FACE OF THE RECORD OR IT IS NOT HONEST. A
    # reader who goes looking for the iteration number the launcher printed
    # on screen finds it here rather than finding nothing.
    if called != unit:
        lines.append('UNIT_AS_CALLED: %s' % called)

    for field in FIELDS:
        lines.append('%s: %s' % (field, values[field]))

    return lines


def continuation(unit, called, step, values, already):
    """A second append for the same unit and step, folded into its entry.

    Only what differs is written. The two routes send almost the same
    sentences - the arbiter composes its fields from the same decision block
    the unit recorded - and repeating the identical ones would put the
    duplicate back in a smaller typeface.
    """
    lines = [
        '',
        '### ALSO RECORDED FOR UNIT %s - STEP %s' % (unit, step),
        '',
        'A second append for the same unit and the same step, called as UNIT %s.'
        % called,
        'One unit is one entry, so what this route recorded is folded in here',
        'rather than written as a second entry. Only what differs is listed.',
        '',
    ]

    differs = [field for field in FIELDS
               if values[field] != already.get(field, '')]

    if not differs:
        lines.append('Nothing this route recorded differs from the entry above.')

        return lines

    for field in differs:
        lines.append('%s: %s' % (field, values[field]))

    return lines


def main(argv):
    if len(argv) != 1:
        sys.stderr.write(__doc__)
        return 2

    path = argv[0]
    called = ascii_only(os.environ.get('OA_UNIT', ''))
    step = ascii_only(os.environ.get('OA_STEP', ''))
    unit, called = resolve_unit(called, repository_root())

    values = {}

    for field in FIELDS:
        source = SOURCE.get(field, 'OA_' + field)
        values[field] = ascii_only(os.environ.get(source, ''))

    already = existing_entry(path, unit, step)

    if already is None:
        lines = new_entry(unit, called, step, values)
    else:
        lines = continuation(unit, called, step, values, already)

    body = '\r\n'.join(lines) + '\r\n'

    # **APPENDED, NEVER REWRITTEN.** Everything under `## UNIT` in that file
    # is append-only; this opens in append mode so no existing byte is read
    # back and written out again, which is the one way a writer can corrupt
    # a record it was not asked to touch.
    with open(path, 'ab') as handle:
        handle.write(body.encode('ascii'))

    sys.stdout.write('outcome-entry: UNIT %s - STEP %s%s\n'
                     % (unit, step,
                        '' if called == unit else ' (called as %s)' % called))

    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))

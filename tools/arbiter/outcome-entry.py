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

Usage, with every value in the environment as OA_<FIELD>:

    python tools/arbiter/outcome-entry.py <file>
"""
import os
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


def main(argv):
    if len(argv) != 1:
        sys.stderr.write(__doc__)
        return 2

    path = argv[0]
    unit = ascii_only(os.environ.get('OA_UNIT', ''))
    step = ascii_only(os.environ.get('OA_STEP', ''))

    lines = ['', '## UNIT %s - STEP %s' % (unit, step), '']

    for field in FIELDS:
        source = SOURCE.get(field, 'OA_' + field)
        lines.append('%s: %s' % (field, ascii_only(os.environ.get(source, ''))))

    body = '\r\n'.join(lines) + '\r\n'

    # **APPENDED, NEVER REWRITTEN.** Everything under `## UNIT` in that file
    # is append-only; this opens in append mode so no existing byte is read
    # back and written out again, which is the one way a writer can corrupt
    # a record it was not asked to touch.
    with open(path, 'ab') as handle:
        handle.write(body.encode('ascii'))

    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))

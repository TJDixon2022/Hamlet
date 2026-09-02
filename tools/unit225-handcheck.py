"""The six rules validate-output.bat holds, transcribed so they can be checked.

WRITTEN IN UNIT 225 AND NOT EXECUTED IN IT. The permission layer in that session
refused to run `tools\\arbiter\\validate-output.bat` under every invocation tried -
directly, through `cmd /c`, and through `bash` - exactly as unit 224 reported, and
then refused to run this script as well. The check that was actually performed on
unit 225's report was by hand, with `grep`, `sed` and `od` against the five
structural facts below, and it is reported as a hand check rather than as an exit
zero.

This is checked in because the rules were read out of the batch file's own body
rather than out of CLAUDE_CODE.md, and a session that CAN run a script should not
have to read the batch file again to get them. It is not a second validator and
must not be treated as one: CPS-DEC-066's whole point is that the two independent
checks are the standard and the batch file, and a third copy that drifts is worse
than none. If it ever disagrees with the batch file, THE BATCH FILE IS RIGHT.
"""

import re
import sys

lines = open("output.md", encoding="utf-8-sig").read().split("\n")
head = lines[:60]
failed = 0


def say(ok, rule, detail):
    global failed
    print(("  ok      " if ok else "  FAILED  ") + rule + "  " + detail)
    if not ok:
        failed += 1


# rule 1 - a UNIT: line above section 1, parseable
unit = [line for line in head if line.startswith("UNIT:")]
at_unit = next((i for i, l in enumerate(lines) if l.startswith("UNIT:")), None)
at_one = next((i for i, l in enumerate(lines) if l.startswith("## 1.")), None)
say(
    bool(unit) and at_unit is not None and at_one is not None and at_unit < at_one,
    "rule 1",
    unit[0] if unit else "no UNIT: line",
)

# rules 2 and 3 - the four top-level sections, in order, and no fifth
found = [line[3:].strip() for line in lines if line.startswith("## ")]
wanted = [
    "1. What Claude did",
    "2. What the owner should expect",
    "3. What you should see",
    "4. What's blocking us",
]
say(found == wanted, "rule 2/3", " ~ ".join(found))

# rule 4 - section 4 present even when empty
say(
    any(line.startswith("## 4. What's blocking us") for line in lines),
    "rule 4",
    "section 4 present",
)

# rule 5 - section 3 non-empty
a = next(i for i, l in enumerate(lines) if re.match(r"^## 3\. ", l))
b = next(i for i, l in enumerate(lines) if re.match(r"^## 4\. ", l))
count = len([l for l in lines[a + 1 : b] if l.strip()])
say(count > 0, "rule 5", "section 3 has %d non-blank lines" % count)

# rule 6 - the ordering block, presence only
header = len([l for l in head if re.search("READ IN THIS ORDER", l)])
a_lines = len([l for l in head if re.match(r"^A\.", l)])
b_lines = len([l for l in head if re.match(r"^B\.", l)])
c_lines = len([l for l in head if re.match(r"^C\.", l)])
counted = len([l for l in head if re.search(r"raises \d+ item", l)])
say(
    header >= 1 and a_lines >= 1 and b_lines >= 1 and c_lines >= 1 and counted >= 1,
    "rule 6",
    "header %d, A %d, B %d, C %d, section-4 count line %d"
    % (header, a_lines, b_lines, c_lines, counted),
)

print("")
print(
    "  HAND CHECK: all six rules passed"
    if failed == 0
    else "  HAND CHECK: %d rule(s) failed" % failed
)
sys.exit(1 if failed else 0)

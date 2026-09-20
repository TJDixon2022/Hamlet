"""Builds output.md: this unit's report, then unit 362's section 4 carried verbatim.

The carried block is taken as bytes from the commit that holds it (a7f81c48), never
retyped, because HM-DEC-139 says verbatim and a retyped queue drifts.
"""
import subprocess

CARRIED_FROM = "a7f81c48"
FIRST_LINE_OF_SECTION_4_BODY = 203  # its "## 4. What's blocking us" heading is line 202

head = open(r"C:\Source\HamLet\.u365-head.md", encoding="utf-8").read()

previous = subprocess.run(
    ["git", "show", CARRIED_FROM + ":output.md"],
    cwd=r"C:\Source\HamLet",
    capture_output=True,
    check=True,
).stdout.decode("utf-8")

lines = previous.split("\n")
assert lines[FIRST_LINE_OF_SECTION_4_BODY - 2].startswith("## 4."), lines[FIRST_LINE_OF_SECTION_4_BODY - 2]
carried = "\n".join(lines[FIRST_LINE_OF_SECTION_4_BODY - 1:])

heading = (
    "### Asks still outstanding - carried from unit 362's section 4, per HM-DEC-139, verbatim\n"
    "\n"
    "The words below are unit 362's, from its line under `## 4. What's blocking us` to its end, as\n"
    "committed in `a7f81c48`, taken as bytes and not retyped. Only that top-level heading is\n"
    "dropped, so this report keeps four sections. Unit 364's, 363's and the older queues are inside\n"
    "it already. **This unit answers none of them**, except that decision AY's reading of unit 362's\n"
    "items 1 and 3 - both carried to Tim, neither chased - is the arbiter's and is recorded in\n"
    "`WORK_INSTRUCTIONS.md`, not here.\n"
    "\n"
)

open(r"C:\Source\HamLet\output.md", "w", encoding="utf-8", newline="").write(head + heading + carried)
print("output.md written:", len(head + heading + carried), "characters")

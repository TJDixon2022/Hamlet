"""What unit 323 appended to PHASE_OUTCOME.md, as data rather than as a script.

THIS DOES NOT WRITE. Unit 323's session could not execute python, cmd.exe or a
shell script at all - every invocation came back "requires approval" and the
session was non-interactive - so its entry was written straight into
PHASE_OUTCOME.md in outcome-append.bat's own field order, with CRLF endings, and
the header's STEP: 4 line was left where it stood.

The fields are kept here so the entry can be checked against what was intended
rather than reconstructed from the file it was written into, which is the reason
units 289 to 296 each carry a wrapper. Running this file appends nothing; a
second entry for the same unit and step is the fault outcome-entry.py exists to
fold away.
"""

TASK_1 = {
    "heading": "## UNIT 323 - STEP 4",
    "STEP": "4",
    "APPROACH":
        "Checked the four gate facts, read PHASE_PLAN.md R11 to R14 against the tree, and ran "
        "the carry-forward list filtered and foregrounded before changing a line.",
    "HIT":
        "STEP 4 IS BLOCKED BY TWO THINGS THIS TREE ALREADY HOLDS THE ANSWER TO. CanTransmitIn "
        "answers true for null, FT8 and FT4 and for nothing else, and a test pins its source "
        "text character for character - so the door is shut and a test says it must stay shut. "
        "Everything behind the door was built and proved by units 318 to 322: the modulator, "
        "the four macros, the unslotted send with its 30 s cap, the certainty gate, the turn "
        "indicator, the conversation card, and six transmit events with no production call site.",
    "MOVE":
        "Open the door on R11, rewrite the guard on R12 so it guards the click rule instead of "
        "the shut door, and give the six events their call sites.",
    "WHY":
        "The owner has said what the measure of the unit is - he wants to see PSK31 "
        "transmitting - and nothing but the guard and the predicate stands between the press "
        "and the air.",
    "DECIDED": "Nothing yet. Task 1 changes no production file except the version.",
    "LICENCE":
        "Work instruction 323 tasks 1 to 5 under PHASE_PLAN.md R1, R2, R6, R8, R10, R11, R12, "
        "R13 and R14, CLAUDE.md 0.2, HM-DEC-084 and HM-DEC-155.",
    "COST":
        "one session, no test suite run, the carry-forward list of 44 named types at 265 of "
        "265 green before anything changed - app 145 and engine 120",
    "ACCOMPLISHED":
        "The starting position is measured rather than assumed, and the two rulings that "
        "unblock step 4 are confirmed present in the plan.",
    "FATE": "executed",
    "STATE_AFTER": "blocked",
    "STATE_WHY":
        "STEP 4 IS CARRIED AT blocked UNTIL THE DOOR IS OPEN. Task 1 only measured; the tasks "
        "that advance it are 1b onward.",
}

if __name__ == "__main__":
    raise SystemExit(
        "unit 323's entry is already in PHASE_OUTCOME.md. This file records it; "
        "it does not append it."
    )

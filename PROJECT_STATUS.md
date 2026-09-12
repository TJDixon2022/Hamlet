PROTOCOL: 2
PROJECT: Hamlet
STATE: EXECUTING
TASK: 1 of 7
WORK_INSTRUCTION: 330 - maintenance round one, and the achievements page
BALL: code
NEXT_PASTE: output.md -> the arbiter
RULES_AT: HM-DEC-161 (2026-09-11)
UPDATED: 2026-09-12T11:08:44-04:00
NOTE: Task 1, the tray mark is sized and the two size tests are reconciled - 20 of 20 green. Watched red first: Unit300SizesTests.WhatTheMarkDrawsAtEachSize failed on "the resting mark filled something at 16 px", which is the contradiction with Unit303OptionBTests measured rather than read. Numbers: the drawn quill goes 10.6 x 5.3 px to 27.0 x 13.5 px, the box 27 to 32 because the orbit ring has to go round the drawing, and the status bar grows 41 to 46 px outside as the cost. The app carry-forward invocation with the mark tests, BindingHealthTests and VoiceTests is 123 of 123 green. Committing task 1. Version bumped 1.13.14 to 1.13.15 and the UNIT 330 entry appended to PHASE_OUTCOME.md under step 6 before anything else moved. Measured first: the tray quill's box is the ruled 27 px but the renderer scales the 44-unit file to 62 per cent of the box, so the drawn vane is about 10.6 x 5.3 px and the ruled number was never on the glass.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

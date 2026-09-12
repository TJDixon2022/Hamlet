PROTOCOL: 2
PROJECT: Hamlet
STATE: EXECUTING
TASK: 1 of 8
WORK_INSTRUCTION: 331 - maintenance round one, continued
BALL: code
NEXT_PASTE: output.md -> the arbiter
RULES_AT: HM-DEC-161 (2026-09-11)
UPDATED: 2026-09-12T12:52:40-04:00
NOTE: Task 1 verified rather than redone - unit 330 did land it. The scale is taken off the vane's own bounds in the SVG so the ink is TrayQuill = 27 px tall in a TraySide = 32 px box, the two size tests are reconciled on option B, and both remaining filled-disc strings in MainWindow.axaml read correctly in context - one is the corrected comment saying what the disc was and is not, the other is about a status light in the count badge. Ten of ten green on Unit300SizesTests, Unit301MarkSizeTests, Unit303OptionBTests and the binding sweep. Task 0 committed as dcfabc5 - 185 of 185 green on the carry-forward list before anything moved, app 100 in 11 s and engine 85 in 4 s, the UNIT 331 entry appended under step 6 and 1.13.15 bumped to 1.13.16. Nothing of unit 330's was uncommitted, so nothing was recovered or stashed. Now verifying task 1: unit 330's commit 79dbc50 claims the tray vane is drawn at the ruled 27 px off its own bounds rather than 62 per cent of the box, the two size tests reconciled on option B, and the filled-disc comment corrected - about to run the three mark tests and BindingHealthTests to see whether the claim holds at HEAD.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

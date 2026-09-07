PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 3 of 5
WORK_INSTRUCTION: 260 - the menu under the mouse, and a route to the radio
BALL: claude
NEXT_PASTE: output.md -> Claude Web
RULES_AT: HM-DEC-157 (2026-09-06)
UPDATED: 2026-09-06T23:59:40-04:00
NOTE: Task 3 green - seven tests, all passing, no device and no port opened. The red was watched first by removing the guards: unconditional construction would not even compile against Ft8TransmitSequence's null check, and forced past it the sink factory was reached with an empty name and the connect threw ArgumentNullException at Ft8TransmitSequence.cs:224. The arming grep run again is unchanged in shape - one Arm call site, MainWindowViewModel.cs:8149 inside SendMessage - and _armedSend now has a second assignment which is the connect path and nothing else. Committing, then task 4, the drop candidate.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

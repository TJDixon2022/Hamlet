PROTOCOL: 2
PROJECT: Hamlet
STATE: running
TASK: TASK 1 of 5
WORK_INSTRUCTION: 380 - the record says what was on screen: one event a change, sampled, and unit 337 diagnosed from the file with no screenshot
BALL: claude
NEXT_PASTE: none
RULES_AT: HM-DEC-165 (2026-09-19)
UPDATED: 2026-09-21T16:28:07-04:00
NOTE: Task 1 measured, all seven items. The budget is decided by arithmetic: 3360 rows an hour counted off a 14-decode slot over 8 slots, one on_screen line 267 bytes measured on the file (142 of it the schema-B envelope), so one event per row is 876 kB an hour - 17.5 times 3.4's 50 kB. 50 kB buys 191 lines an hour, one every 18.8 s, 0.80 a slot - LESS THAN ONE, so the sampling window cannot be the slot. Two findings against section 5: the squelch is not a visibility gate on a text row at all, and decodes_drawn already carries per-slot counts section 5 does not mention. Committing the trace.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

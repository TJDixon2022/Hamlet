PROTOCOL: 2
PROJECT: Hamlet
STATE: RUNNING
TASK: 2 of 5
WORK_INSTRUCTION: 270 - after his last transmission, the screen still says where the contact stands
BALL: claude
NEXT_PASTE: output.md -> Claude Web
RULES_AT: HM-DEC-158 (2026-09-07)
UPDATED: 2026-09-07T12:27:49-04:00
NOTE: THE HOLE IS THERE AND IT IS MEASURED, NOT REASONED. Task 1's trace is committed and pushed (7fdc88b). Task 2's measurement ran green once, alone by exact name, foregrounded, 20 s of wall time because it waits for a real slot boundary. On that run: the exchange was CQ W1ABC EM12 at 16:27:15, the operator's W1ABC KC3QIS FN00 at 16:27:30, KC3QIS W1ABC R-09 at 16:27:45, the operator's own W1ABC KC3QIS RRR at 16:28:00, and then nothing. THE LEDGER READS IsComplete=True AND "complete, 0 slots" at 16:28:00; THE NEWEST ROW ON THE TABLE STILL READS "your move, 0 slots" at 16:27:45; AND NEITHER LINE IN THE SEND AREA CARRIES ANY OF THE FOUR STATE WORDS, which are read off the enum rather than written out. It was added to the committed walk file rather than copied into a new one - Panel(), Heard, ClickAsync and WaitForSlotAsync are reused and no existing method lost a line. Next: task 3, the fourth line in the Send area, watched red before it is made green.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

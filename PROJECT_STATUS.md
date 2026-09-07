PROTOCOL: 2
PROJECT: Hamlet
STATE: RUNNING
TASK: 3 of 5
WORK_INSTRUCTION: 270 - after his last transmission, the screen still says where the contact stands
BALL: claude
NEXT_PASTE: output.md -> Claude Web
RULES_AT: HM-DEC-158 (2026-09-07)
UPDATED: 2026-09-07T12:37:06-04:00
NOTE: GREEN, AND THE LINE IS A MEASUREMENT WITH A MOMENT ON IT. On a run tonight, after CQ W1ABC EM12 at 16:34:30, the operator's grid at 16:34:45, KC3QIS W1ABC R-09 at 16:35:00 and his own W1ABC KC3QIS RRR at 16:35:15, the Send area reads: "Where the contact with W1ABC stands: complete, 0 slots, read at the 16:35:15 UTC slot. That is what passed between you, counted in slots; it is not advice about what to send next, and nothing is closed or withheld by it." The four must-pass assertions are green - the state words come out of Ft8ContactStates.Read against the application's own ledger and are not written as literals; both contact cells on the table read "your move, 0 slots" before and after his last send; and a second boundary with nothing armed left the sink at 2 calls and the port at 4 frames with the line unchanged. BOTH SHOULD-PASS ITEMS GOT AN ANSWER: a CQ books nobody and the line names no station, and a later decode does NOT move the line - it stands at the slot it was read at while the row that decode places carries "complete, 0 slots", which is the design and the line's own wording covers it. ALL SEVEN COMMITTED TESTS FROM THE NAMED EXCEPTION PASS, each run alone by exact name and foregrounded, and none had to change. Task 2's measurement re-run after the green and still passes unchanged. Next: commit the green, then task 4's bookkeeping.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

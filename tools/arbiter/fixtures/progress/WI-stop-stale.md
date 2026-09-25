# Instruction for the progress fixture - a stop 4 standing on a dead unit

Built by unit 088 task 4 beside `WI-stop.md`. The same shape, MOVE: stop, but its
WHY rests on a premise the launcher can test: that a unit is executing now. At the
moment the launcher reads this the session lock at the root is free - its own units
take the lock per run and release it, and the door refused to start if anything else
held it - so nothing is executing here and the claim is false. HamLet, 2026-09-25:
"its unit 439 is executing now", about a session dead since 13:34.

```
ARBITER-DECISION
STEP: 2
APPROACH: hold criterion 2.3 until the running unit finishes - this is the owner's to sequence
MOVE: stop
WHY: criterion 2.3 of the plan cannot be moved while its unit 439 is executing now, and only the owner can say whether to wait for it or kill it - it is his to sequence
STATE: in progress
DECIDED: none
LICENCE: none
ACCOMPLISHED: nothing - the sequencing is the owner's
ADVANCES: step 2 criterion 3
END-ARBITER-DECISION
```

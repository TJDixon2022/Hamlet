PROTOCOL: 2
PROJECT: Hamlet
STATE: EXECUTING
TASK: 3 of 5
WORK_INSTRUCTION: 253 - the dummy load is gone, and the abort is watched to fire
BALL: code
NEXT_PASTE: none
RULES_AT: HM-DEC-156 (2026-09-06)
UPDATED: 2026-09-06T18:47:05-04:00
NOTE: Tasks 1 and 2 pushed. Task 3 is reading, and it has found the half that is missing: Ic7300Rig.AbortCw already writes 0x17 0xFF straight at the port on this thread and never throws, and ISerialPort.Write is the synchronous seam it uses - but 0x1C 00 exists only as a read, so there is no PTT-off fallback anywhere in the engine and nothing attempts a second write when the first one throws. That is what task 4 builds.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

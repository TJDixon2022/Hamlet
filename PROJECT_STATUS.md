PROTOCOL: 2
PROJECT: Hamlet
STATE: EXECUTING
TASK: 4 of 5
WORK_INSTRUCTION: 253 - the dummy load is gone, and the abort is watched to fire
BALL: code
NEXT_PASTE: none
RULES_AT: HM-DEC-156 (2026-09-06)
UPDATED: 2026-09-06T19:05:10-04:00
NOTE: The abort is built and green. Fourteen cases, watched failing first: nine of fourteen red against a deliberately incomplete first version carrying the stop code and no fallback, then fourteen of fourteen green in 22 ms once 1C 00 00 went in beside 17 FF. The no-await tripwire was itself watched to fire - an await was put on the path, the test went red, the await came out. Nothing calls TransmitAbort.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

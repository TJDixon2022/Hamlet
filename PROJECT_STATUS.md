PROTOCOL: 2
PROJECT: Hamlet
STATE: RUNNING
TASK: 2 of 6
WORK_INSTRUCTION: 206
BALL: claude
NEXT_PASTE: output.md -> Claude Web
RULES_AT: HM-DEC-152 (2026-08-31)
UPDATED: 2026-09-01T10:47:22-04:00
NOTE: The read landed and it turned up a defect on the way. The two scalars the port needs are not in crc.h where the instruction expects them but in constants.h, and the existing macro reader was silently dropping one of them because the pinned header has mixed line endings and the regex anchor only matches before a newline. Both the anchor and a missing cast form are now handled, so leg A has something to assert against. The task 1 stamp in the previous commit was composed rather than read, which is corrected here from the clock.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

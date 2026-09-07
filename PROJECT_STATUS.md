PROTOCOL: 2
PROJECT: Hamlet
STATE: RUNNING
TASK: 2 of 5
WORK_INSTRUCTION: 263 - the stop stops the audio too
BALL: claude
NEXT_PASTE: output.md -> Claude Web
RULES_AT: HM-DEC-157 (2026-09-06)
UPDATED: 2026-09-07T01:34:00-04:00
NOTE: THE RED IS WATCHED AND QUOTED. Both fakes now have a play that takes time and honours the token; four new tests in TheStopStopsTheAudioTooTests run the stop into a live transmission. Against the tree as it stands, 3 of the 4 fail and the numbers are the unit's evidence: wire at the stop FE FE 94 E0 1C 00 01 FD | FE FE 94 E0 17 FF FD | FE FE 94 E0 1C 00 00 FD - the carrier is off - and the sink played 151680 of 151680 samples anyway, 12640 ms of audio of which 8345 ms went out AFTER the operator pressed stop, with the run reporting itself Sent. Task 1's trace is committed and pushed at 894de90.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

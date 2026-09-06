PROTOCOL: 2
PROJECT: Hamlet
STATE: COMPLETED
TASK: 5 of 5
WORK_INSTRUCTION: 255 - key, transmit, unkey, with the unkey guaranteed
BALL: tim
NEXT_PASTE: output.md -> Claude Web
RULES_AT: HM-DEC-157 (2026-09-06)
UPDATED: 2026-09-06T19:56:38-04:00
NOTE: Five of five done and pushed, 1.12.85 to 1.12.89, a patch a task, nothing dropped. Hamlet can key a radio, hand it a transmission and come out of transmit - 6 of 6 failure modes leave it unkeyed with the bytes on the wire to prove it, and TransmitAbort has its first caller. The gate refuses at zero writes on all three of its permissive branches without Check being touched. The signal starts 0.5 s into the slot, which closes step 2 criterion 5's open half, and the record carries eleven fields and no string parameter at all. Step 3 recorded partial: criteria 2, 4, 5 and 6 met, and 1 and 3 - the device and the loopback - deliberately not attempted and left whole for the next unit. Nothing is blocking. Every UPDATED this session was read from the clock.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

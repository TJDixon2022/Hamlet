PROTOCOL: 2
PROJECT: Hamlet
STATE: RUNNING
TASK: 4 of 5
WORK_INSTRUCTION: 256 - the audio leaves the machine, and Hamlet's decoder hears it come back
BALL: claude
NEXT_PASTE: output.md -> Claude Web
RULES_AT: HM-DEC-157 (2026-09-06)
UPDATED: 2026-09-06T20:26:25-04:00
NOTE: Three of three decoded and are pushed. The loopback has now been widened to run the whole send path rather than the sink alone - the gate, the keying frame to unit 253's fake transport, the real sink into the real endpoint, the guaranteed unkey, and unit 255's record written at the end - so the same three transmissions also carry criterion 4's evidence and the record is asserted to hold no callsign. ABOUT TO RUN IN REAL TIME AGAIN, roughly 45 seconds for the class. Then task 4 writes up the level: the sink wrote peak 1.0 at 0 dBFS with nothing clipped, and that is a development-machine figure, not the radio's.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

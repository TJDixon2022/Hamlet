PROTOCOL: 2
PROJECT: Hamlet
STATE: RUNNING
TASK: 3 of 5
WORK_INSTRUCTION: 256 - the audio leaves the machine, and Hamlet's decoder hears it come back
BALL: claude
NEXT_PASTE: output.md -> Claude Web
RULES_AT: HM-DEC-157 (2026-09-06)
UPDATED: 2026-09-06T20:23:09-04:00
NOTE: IT DECODED, and it is committed and pushed on its own. "CQ KC3QIS FN00" left the sound card, came back through WasapiLoopbackCapture, AudioTap, Ft8Resample and Ft8SlotDecoder, and read back as "CQ KC3QIS FN00" - whole text, ordinal, 606720 of 606720 samples in 12.657 s, 14.686 s wall clock. The same capture handed over without the resampler returned nothing, which is the rate lie caught. ABOUT TO RUN IN REAL TIME AGAIN: the two further messages of other shapes, roughly 30 seconds. The tap's live meter reads the silence after the transmission rather than the transmission, so the capture's own peak is now reported beside it.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

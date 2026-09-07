PROTOCOL: 2
PROJECT: Hamlet
STATE: RUNNING
TASK: 2 of 5
WORK_INSTRUCTION: 256 - the audio leaves the machine, and Hamlet's decoder hears it come back
BALL: claude
NEXT_PASTE: output.md -> Claude Web
RULES_AT: HM-DEC-157 (2026-09-06)
UPDATED: 2026-09-06T20:08:55-04:00
NOTE: The machine answered. Four active render endpoints, all 48000 Hz 2 ch 32-bit float behind an Extensible tag, and WasapiLoopbackCapture started and delivered 16 callbacks in a second - so LOOPBACK ROUTE: device, and the file fallback is not needed. The shell refused device enumeration and the throwaway xunit test the instruction names was used instead. Now the render sink itself is going in, watched to fail first on the breakage that matters: returning SamplesPlayed without waiting for the card's buffer to drain, which is a radio unkeying while audio is still in flight.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

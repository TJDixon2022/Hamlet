PROTOCOL: 2
PROJECT: Hamlet
STATE: RUNNING
TASK: 3 of 5
WORK_INSTRUCTION: 256 - the audio leaves the machine, and Hamlet's decoder hears it come back
BALL: claude
NEXT_PASTE: output.md -> Claude Web
RULES_AT: HM-DEC-157 (2026-09-06)
UPDATED: 2026-09-06T20:20:37-04:00
NOTE: ABOUT TO RUN IN REAL TIME - a single loopback is roughly fifteen seconds in which nothing is written here. The chain is assembled: ComposeSignal at the endpoint's own 48000 Hz, played by the real sink to the display-audio endpoint, WasapiLoopbackCapture on that same endpoint, the engine's own Downmix into AudioTap, Ft8Resample.ToFt8Rate down to 12000, and Ft8SlotDecoder. The same capture is decoded twice in one run so the rate lie - 48 kHz handed over as though it were 12 kHz - is watched returning nothing at no extra wall-clock cost. One message first, committed on its own before anything else is tried.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

PROTOCOL: 2
PROJECT: Hamlet
STATE: RUNNING
TASK: 2 of 5
WORK_INSTRUCTION: 256 - the audio leaves the machine, and Hamlet's decoder hears it come back
BALL: claude
NEXT_PASTE: output.md -> Claude Web
RULES_AT: HM-DEC-157 (2026-09-06)
UPDATED: 2026-09-06T20:16:38-04:00
NOTE: The breakage was watched and it is real. Built without the drain, the sink reported 96000 of 96000 samples played in 1.810 s of a 2.000 s tone - 190 ms of audio still inside the card at the moment the caller was told the transmission had gone out. With the drain restored the same test reads 2.009 s and passes. Sound is now leaving this machine, on a display-audio endpoint chosen rather than defaulted to, at peak 0.25 with nothing clipped. The remaining task 2 tests - the loud refusal for a device that is not there, the rate refusal, the cancelled short play through the untouched sequence - go next.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

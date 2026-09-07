PROTOCOL: 2
PROJECT: Hamlet
STATE: EXECUTING
TASK: 2 of 5
WORK_INSTRUCTION: 262 - the rate the endpoint actually speaks
BALL: code
NEXT_PASTE: none
RULES_AT: HM-DEC-157 (2026-09-06)
UPDATED: 2026-09-07T01:22:15-04:00
NOTE: Both transmit fakes now declare a rate and refuse every other one, in the same words WasapiTransmitSink.cs:306 uses, defaulted off so no existing test changes meaning. The new test drives the application's own send path against a sink declaring 48000 Hz and is RED ON PURPOSE, 3 of 4 failing: rate asked for 12000, keyed True, 3 frames on the wire, samples played empty, and the operator would read "the samples are at 12000 Hz and the endpoint speaks 48000 Hz". Arming grep before task 3 is 1, 1, 1. Committing the red, then task 3 turns it green.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

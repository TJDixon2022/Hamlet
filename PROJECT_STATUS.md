PROTOCOL: 2
PROJECT: Hamlet
STATE: EXECUTING
TASK: 3 of 5
WORK_INSTRUCTION: 262 - the rate the endpoint actually speaks
BALL: code
NEXT_PASTE: none
RULES_AT: HM-DEC-157 (2026-09-06)
UPDATED: 2026-09-07T01:41:05-04:00
NOTE: The send path now composes at the rate the endpoint declares. ITransmitAudioSink carries EndpointSampleRate and its closing para no longer instructs a caller to do something no caller could do; BuildTheArmedSend reads the rate off the sink it just built and refuses in words, naming the device and its rate, where FT8 cannot be built at it. Task 2's test is green 4 of 4 - 48000 asked and 48000 got, 606720 of 606720 samples played, keyed and ordinary unkey, 12.64 s. OneClickSendsExactlyOneMessageTests 8 of 8 and TheSendPathReachesARealRadioTests 7 of 7. Arming grep unchanged before and after: 1, 1, 1, and the Arm line is still inside SendMessage. Committing, then task 4's round trip at each rate.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

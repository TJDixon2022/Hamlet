PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 5 of 8
WORK_INSTRUCTION: 293 - one click, one FT4 transmission, through the same abort
BALL: claude
NEXT_PASTE: output.md -> Claude Web
RULES_AT: HM-DEC-160 (2026-09-08)
UPDATED: 2026-09-09T11:53:28-04:00
NOTE: Task 5 done - criterion 3 closed. One click one FT4 transmission asserted four ways; the abort fires on the FT4 path and it is the same two frames, FE FE 94 E0 17 FF FD then 1C 00 00; the stop unarms, fires and names which; a licence refusal and a fit refusal each leave the wire literally empty; a decode, four ticks and a countdown arm nothing; an unreadable read-back keys nothing. TransmitAbort needed nothing and was not touched. Earlier: task 4 done - the log now says the mode it was made in. A contact made with FT4 chosen writes MODE=MFSK plus SUBMODE=FT4 and never says FT8; FT8 writes MODE=FT8 with SUBMODE absent, not empty. The mode comes off DigitalMode through unit 291's table, not a string and not the chip's lit state. The FT4 achievement lights off a record the write path made. 78 log control tests green. Earlier: task 3 done - OperatorSend carries the grid, Sendable and both refusal sentences read their numbers off it, SendMessage arms on the 7.5 s grid and the tick recognises it. One click now composes 60480 FT4 samples and arms them for 15:45:22.5. Found and fixed a screen this unit would have broken - the send line formatted HH:mm:ss and read four of FT4's eight boundaries half a second early. Arm still has exactly one caller in src/, counted by reading the tree. 9 new tests green, 122 control tests green, both FT8 sentences byte-identical.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

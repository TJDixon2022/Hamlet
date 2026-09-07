PROTOCOL: 2
PROJECT: Hamlet
STATE: RUNNING
TASK: 3 of 5
WORK_INSTRUCTION: 270 - after his last transmission, the screen still says where the contact stands
BALL: claude
NEXT_PASTE: output.md -> Claude Web
RULES_AT: HM-DEC-158 (2026-09-07)
UPDATED: 2026-09-07T12:32:47-04:00
NOTE: THE RED IS WATCHED AND ALL FOUR FAIL FOR THE ONE REASON THIS UNIT EXISTS TO FIX - "there is no TextBlock called DigitalContactStandsText on the realized window." Four tests in a new file, each run alone by exact name, foregrounded: the line names the station and reads complete (18 s, and on that run the ledger printed IsComplete True and "complete, 0 slots" at 16:32:30 while the screen had nowhere to say it), the line is inside DigitalSendReserved with DigitalStopButton still usable (967 ms), a CQ books nobody so the line names no station (678 ms), and a later decode leaves the line where it was while the row it places carries the state (25 s). They are on a realized window rather than a view-model string because must-pass 3 asserts where the control sits; the walk's file builds no window, which is why task 2's measurement joined it and these did not. NEXT: the product line - an ObservableProperty following the shape of _digitalSendLine at :8149, set in the same Dispatcher post at :8489-8490, asking Ft8MessageSplit for the addressee and Ft8ContactStates for the state, and a TextBlock appended last in the Send area StackPanel so nothing above DigitalStopButton moves.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

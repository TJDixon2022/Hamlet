PROTOCOL: 2
PROJECT: Hamlet
STATE: RUNNING
TASK: 4 of 5
WORK_INSTRUCTION: 263 - the stop stops the audio too
BALL: claude
NEXT_PASTE: output.md -> Claude Web
RULES_AT: HM-DEC-157 (2026-09-06)
UPDATED: 2026-09-07T02:09:00-04:00
NOTE: THE OPERATOR IS TOLD WHAT WAS STOPPED. Ft8StopResult carries three facts and Ft8StopOutcome has six values; the two new ones say a transmission was stopped, which used to be indistinguishable from a stop pressed at an idle radio because _armed is already null mid-slot. The sentence he is left looking at now reads: Stopped: "W1ABC KC3QIS -10" went out for about 3.3 of its 12.6 seconds in the slot at 05:42:15 UTC, and the rest of it did not. The radio was told to stop transmitting. Before tonight that line said the transmission was stopped after 40890 of 151680 samples - the engine's own words, in samples, at an operator. Engine stop suites 24 of 24, app stop tests 9 of 9, the five app send-area suites 28 of 28. Three outcome assertions changed meaning and are updated with the reason at each; every byte assertion is untouched. Task 3 is committed and pushed at d862dd7. Earlier: GREEN, AND THE NUMBER IS 8345 ms -> 15 ms. Ft8ArmedSend now carries one cancellation source beside _armed, created under the same lock in the same block that takes the send, linked to the caller's token, cleared and disposed in a finally; StopNow cancels it through StopTheAudio() on the calling thread. Order is un-arm, abort, audio - the frames go before this unit's new line runs at all. All 4 of TheStopStopsTheAudioTooTests green: audio after the stop 15 ms measured at real time with a 12.64 s transmission in flight, StopNow returned in 0.5 ms against a stated 250 ms bound, and the cancel made to really throw (a callback registered on the token from the sink's side) still leaves 17 FF and 1C 00 00 on the wire. The red it was watched at, committed at 6aa5fd8: the same wire, and the sink playing 151680 of 151680 samples anyway with the run reporting itself Sent. Neighbours checked: the six-state table 12 of 12 with bytes quoted for each, OneClickSendsExactlyOneMessageTests 8 of 8 with no edit to its assertions, the app's TheOperatorCanStopItTests 8 of 8 including a new one that clicks the real button into a live transmission and stops it. The arming grep is 1 before and after and _sequence.RunAsync still has exactly one caller. One inherited red found, not mine and not repaired: ExactlyOneFileInTheShippedTreeCallsTheSequence has been failing since unit 260 put "new Ft8TransmitSequence" in MainWindowViewModel.cs 15 commits ago.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

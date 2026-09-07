PROTOCOL: 2
PROJECT: Hamlet
STATE: IN_PROGRESS
TASK: 1 of 6
WORK_INSTRUCTION: 271 - four faults the operator found on the air
BALL: claude
NEXT_PASTE: output.md -> Claude Web
RULES_AT: HM-DEC-158 (2026-09-07)
UPDATED: 2026-09-07T13:25:56-04:00
NOTE: TASK 1 MEASURED AND IT IS THE HEADLINE - HAMLET TRANSMITTED SOMETHING NOBODY COULD DECODE, TWICE, ON A LIVE ANTENNA. "VP2MAA KC3QIS FN00DJ" composes as NonstandardCallsign with the bits reading "<VP2MAA KC3QIS> FN00DJ", callsign hashed, and Ft8Sharp's own Ft8SlotDecoder returns NOTHING off that slot. "CQ KC3QIS FN00DJ" composes as Standard with the bits reading "<CQ KC3QIS> <FN00DJ>", callsign hashed, decoder returns NOTHING. The four-character forms both read back as themselves. THE SIX-CHARACTER GRID WAS NEITHER TRUNCATED NOR REJECTED, IT WAS ENCODED AS SOMETHING ELSE: the standard packing succeeds and reads back "...FN00", the round-trip guard correctly refused that truncation, and the words then fell through to the hashing pass where "VP2MAA KC3QIS" hashed as ONE callsign and the bracket-stripping comparison accepted it. messageLength comes from send.Transmission.Text.Length in Ft8TransmitSequence.Recorded and measures THE COMPOSED STRING, not the encoded message. Measured at 12000 and re-packed at the 48000 the endpoint ran at, same type and same bits both times. Moving to task 2.

PREVIOUS UNIT (270), FOR CONTEXT: ALL FIVE TASKS DONE AND PUSHED, output.md WRITTEN AND VALIDATED - all seven rules passed, validate-output exit 0, reached through validate-output.proj because the direct spellings were refused, which is the parked permitted-spellings bug and is not raised. STEP E IS in progress AND NO CRITERION OF IT IS CLAIMED: all three are Tim's at his own radio and none could have been closed here. DRIFT GOES TO 2 (was 1) AND IS WRITTEN AS 2. THE NUMBER IS never -> 0 SLOTS: how long after his own last message completes a contact before anything on Hamlet's screen says so, which before tonight was never if the station had gone quiet. THE HOLE WAS MEASURED BEFORE IT WAS CLOSED - on a run tonight the ledger read IsComplete True and "complete, 0 slots" at 16:37:15 while both rows on the table read "your move, 0 slots" and neither Send area line carried any of the four state words. THE LINE NOW READS: "Where the contact with W1ABC stands: complete, 0 slots, read at the 16:35:15 UTC slot. That is what passed between you, counted in slots; it is not advice about what to send next, and nothing is closed or withheld by it." THE FALLBACK WAS NOT TAKEN - task 1 q3 found Ft8MessageSplit.Split and IsCallToAnyone public and already used from the app, so the line asks the same two methods the ledger asks and no second splitter exists. FOUR REDS WATCHED AND COMMITTED FIRST (2a67260), all four with the same sentence: there is no TextBlock called DigitalContactStandsText on the realized window. ALL SEVEN COMMITTED TESTS FROM THE NAMED EXCEPTION PASS, each alone by exact name and foregrounded, none had to change; twelve distinct test invocations in all, no suite, nothing unfiltered, nothing backgrounded. NOTHING IN THE KEYING PATH WAS TOUCHED and no row already on the table was rewritten - the cells were captured before and after his last send and are identical. SECTION 4 RAISES 4 ITEMS AND NONE ASKS FOR A RULING. SHACK_FACTS.md was not touched and step D was not touched.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 4 of 7
WORK_INSTRUCTION: 233
BALL: claude
NEXT_PASTE: none
RULES_AT: HM-DEC-152 (2026-08-31)
UPDATED: 2026-09-03T12:31:00-04:00
NOTE: Task 4 built and green, 12 of 12 in the telemetry classes. AppEvents.Ft8SlotsRead writes one ft8_slot line per slot NoteSlot sees - both the press and the running watch go through that one funnel - carrying the corrected slot start, all five counts, the top Costas match counts, the sample rate, the clock offset and its age. A refusal is its own line with the sentence verbatim, not a silence. Levelled warn where candidates were found and none became words, info otherwise, in the manner DecodeWindow already levels a quiet band. One design call worth naming: the method is NOT handed the Ft8Reception, because that carries Ft8Decode rows and an FT8 message is very often a pair of callsigns - it takes the census list, the refusal string and the offset, so HM-DEC-018 is enforced by the signature rather than remembered at the call site, and a new test fails if a later unit widens it. Ft8Reception gained an Offset and Ft8SlotCensus a SampleRate so the reader hands over what it actually applied. CallsignPrivacyTests walk bumped 63 -> 64 and both new events added to it. Standing worry for the report: telemetry has no file at all for 2026-09-03 although settings.json was written today at 12:34:32 UTC, and TelemetryCategories is empty so Decode is on by default - so the sink this task writes into recorded nothing on the morning the bench check ran. Next: task 5, the sheet.

---

## What this file is

Volatile state, overwritten whole at each write. `PROTOCOL: 2` names
`STATUS_PROTOCOL.md`, which is not in this repository - the header is read as
which protocol this is written against, not as conformance anything here can
check.

Branch, commit and working-tree state are never reported here. The panel reads
those from `.git` itself.

Prose below the `---` is free. Nothing reads it, and no key of this format's own
may appear below the rule.

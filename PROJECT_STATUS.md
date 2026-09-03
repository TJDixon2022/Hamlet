PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 3 of 7
WORK_INSTRUCTION: 233
BALL: claude
NEXT_PASTE: none
RULES_AT: HM-DEC-152 (2026-08-31)
UPDATED: 2026-09-03T12:12:00-04:00
NOTE: Task 3 built and green. Ft8Reception gains an init-only Slots list of a new Ft8SlotCensus record - corrected slot start, the five counts straight off Ft8SlotResult, and the three highest Costas match counts. CandidatesFound did not move and no existing construction of Ft8Reception changed, so nothing that reads it had to be touched. The one real design call: Ft8Reader now builds the waterfall once through Ft8Monitor and runs a mirror Ft8SyncSearch on it before handing the same waterfall to the decoder, because Ft8SlotResult carries no candidate list and the top scores are needed exactly in the case where nothing decoded and there is no message to read a score off. The mirror is built from the decoder's own published CandidateLimit and MinimumScore; nothing in src/Ft8Sharp was touched. 16 of 16 green across TheDigitalTabDecodesWhatItKept, RealOffAirAudioReachesTheTab and TheSlotCutter. Root version 1.12.36 -> 1.12.37 under HM-DEC-150. Next: task 4, one telemetry line per decoded slot - and the trace found telemetry wrote nothing at all for 2026-09-03, which is itself going in the report.

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

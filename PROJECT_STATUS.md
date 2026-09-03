PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 6 of 7
WORK_INSTRUCTION: 233
BALL: claude
NEXT_PASTE: none
RULES_AT: HM-DEC-152 (2026-08-31)
UPDATED: 2026-09-03T13:04:00-04:00
NOTE: Task 6 built and green, 6 of 6. ACapturedFileDiagnosesItselfTests writes a WAV to a temp folder from Ft8Waveform, reads it back with WavAudio.Read, runs Ft8Reader.Read at the rate the file declares, and asserts the census is REPORTED - every field populated, the stages narrowing in order with none exceeding the one before, the top Costas match counts strongest first and at most three, the sample rate the file's own and not the resampler's 12000, and the refusal sentence present exactly when no whole slot was cut and absent otherwise. Driven at 44100 and 48000 so the resampler is in the path; 44100 is deliberately not a whole ratio of 12000. No recording of any kind is committed and there is NO fixture expecting zero decodes - a slot with nothing in it is asserted to be COUNTED, all zeroes, which is a different claim from asserting that a particular recording decodes nothing. The scratch harness from tasks 1 and 2 was emptied rather than deleted: rm and git clean are both outside this unit's permission scope, so the file is untracked, contains only a comment, and was never staged. Next: the standing gates, then task 7 which is the named drop candidate.

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

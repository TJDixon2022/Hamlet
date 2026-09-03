PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 3 of 7
WORK_INSTRUCTION: 236
BALL: claude
NEXT_PASTE: none
RULES_AT: HM-DEC-152 (2026-08-31)
UPDATED: 2026-09-03T14:49:29-04:00
NOTE: The line is written and it has been read whole rather than key by key. ft8_slot now carries audioPeakDbFullScale, audioRmsDbFullScale, audioSamples, audioZeroSamples and audioZeroSampleFraction beside sampleRate. Printed as it will actually appear on disk, decoded case: {"outcome":"decoded",...,"sampleRate":12000,"audioPeakDbFullScale":-2.05,"audioRmsDbFullScale":-14.17,"audioSamples":180000,"audioZeroSamples":13,"audioZeroSampleFraction":7.2E-05,...}. Digital silence: the same shape with both levels NULL, audioZeroSamples 720000 of audioSamples 720000 and the fraction at 1. The two are now different facts in the file. Four new assertions in EverySlotLeavesALineTests, one of which walks the forbidden key list - snr, signal, strength and a bare db are all absent. The refusal branch is untouched and writes no level at all, asserted. CALLSIGNPRIVACYTESTS BEFORE 4 of 4 GREEN, AFTER 4 of 4 GREEN, event method count unmoved at 65 because no AppEvents method was added - the payload grew, and the walk now hands Ft8SlotsRead a measured level AND a refused one so both branches of the new fields are walked. EverySlotLeavesALineTests 6 -> 10, all green. Next: task 4, why this device.

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

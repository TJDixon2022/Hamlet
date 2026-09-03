PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 2 of 7
WORK_INSTRUCTION: 236
BALL: claude
NEXT_PASTE: none
RULES_AT: HM-DEC-152 (2026-08-31)
UPDATED: 2026-09-03T14:46:31-04:00
NOTE: THE NUMBERS ARE IN AND THE TWO CASES ARE NOW DIFFERENT FACTS. Ft8SlotLevel added beside the census - peak and rms in decibels relative to full scale, both NULLABLE, plus the sample count and the count of samples exactly zero - and computed in Ft8Reader.Read from slot.Audio before the resampler. Four tests, 4 of 4 green off TRX Counters, and the pinned clone at C:\Source\ft8_lib IS present so the off-air one ran rather than skipping. Measured: DIGITAL SILENCE reads peak none, rms none, 720000 of 720000 samples exactly zero, fraction 1.0000. REAL OFF-AIR (20m_busy/test_21.wav, 15 s at 12000 Hz, 140 candidates, 40 became text) reads peak -2.0541 dBFS, rms -14.1684 dBFS, 13 zero samples of 180000, fraction 0.0001, crest 12.11 dB. Synthesized noise at a thousandth of full scale reads peak -60.0000, rms -64.7653, no zeros at all - which is the whole point, a quiet band is not a dead input. The half-scale sine read -6.0206 and -9.0309 against arithmetic predicting -6.0206 and -9.0309, printed before the twentieth-of-a-decibel bound was asserted. The all-zero refusal is a null and NOT minus ninety, because AudioTap.ToDb floors a zero there and that is a plausible number somebody would average. Next: task 3, the level into the ft8_slot telemetry line.

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

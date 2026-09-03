PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 1 of 7
WORK_INSTRUCTION: 236
BALL: claude
NEXT_PASTE: none
RULES_AT: HM-DEC-152 (2026-08-31)
UPDATED: 2026-09-03T14:40:46-04:00
NOTE: Task 1's trace is answered and it found something that changes what task 2 is worth. THE BEFORE IS NOT ZERO, but it is close and it is not on the slot record. A level EXISTS in this tree - AudioTap.Level (AudioTap.cs:109), an AudioLevel of PeakDb/RmsDb/FloorDb/Clipping computed on the very tap the FT8 slot watch reads - and AppEvents.DecodeQuality (AppEvents.cs:1059) writes inputPeakDb, inputRmsDb, inputFloorDb and nearlySilent into telemetry. It is not per slot, it is not tied to a slot boundary, and NoteDecodeQuality (MainWindowViewModel.cs:4202) is rate-limited by a did-it-move test whose starting state is CwDecodeReport.None, whose Level.PeakDb is AudioLevel.SilenceDb = -90 - which is exactly what a digitally silent input reads, because AudioTap.ToDb floors zero at -90. So a morning of pure silence moves nothing and writes NOT ONE decode_quality line. Also confirmed: AudioTap.PeakOf floors an all-zero recording at -90 dBFS, which is the plausible-number-for-no-measurement that HM-DEC-009 forbids, so it is the arithmetic to follow and not the refusal. Section 2's claims 1, 2, 3, 4, 5, 6, 7, 10, 11, 12 and 13 all check out against the tree. Next: task 2, the level on every slot.

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

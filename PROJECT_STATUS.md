PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 2 of 7
WORK_INSTRUCTION: 235
BALL: claude
NEXT_PASTE: none
RULES_AT: HM-DEC-152 (2026-08-31)
UPDATED: 2026-09-03T13:47:00-04:00
NOTE: IT WRITES, and worse than expected. Run (a) alone changed settings.json (1200 -> 1352 bytes, 106BEE -> 555B1D) and touched spots.db-shm. Stopped there; (b) and (c) not run. Vehicle proved inert first - two back-to-back snapshot runs byte-identical. Three addresses, all in the CONSTRUCTOR at MainWindowViewModel.cs:2307-2534: :2415 opens the real spots.db, :2482 calls PickByline which saves at :2582, and :2523 fires ResolveProfileAsync which does a LIVE CallookCallsignLookup on the operator's callsign and saves the answer at :8147. Measured against the backup: callsign survived because the default at OperatorProfile.cs:44 is a hardcoded real one, but GridSquare, Latitude, Longitude and LicenseClass were all rewritten by that lookup. Committing task 2, then task 3's build.

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

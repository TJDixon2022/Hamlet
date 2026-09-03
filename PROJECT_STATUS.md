PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 4 of 7
WORK_INSTRUCTION: 235
BALL: claude
NEXT_PASTE: none
RULES_AT: HM-DEC-152 (2026-08-31)
UPDATED: 2026-09-03T13:53:35-04:00
NOTE: THE SEAM TOOK, first attempt, and it is proved by re-measurement rather than by assertion alone. SettingsStore.DataFolder now has an internal setter and the four paths under it are computed on each read instead of captured at static init; a ModuleInitializer in Hamlet.App.Tests points the whole lot plus CaptureFolder at a per-process temp folder. Two new tests, both of which would have failed this morning. Then the decisive part: the SAME nine tests of TheTabHearsEverySlotTests re-run with the seam in place, snapshot either side - byte-identical across all fourteen files. 2 -> 0. Earlier notes below. Task 3 done and task 5 taken with it. The shell BUILDS - dotnet build -c Release src/Hamlet.App/Hamlet.App.csproj, exit 0, 0 warnings, 5.0 s, with TreatWarningsAsErrors and GenerateDocumentationFile both on. Version read off the binary, not off the props file: AssemblyVersion 1.12.38.0, Win32 FileVersion 1.12.38.0, ProductVersion 1.12.38+daeccb3, so App.axaml.cs:37 will stamp 1.12.38. dotnet publish is not in this sandbox's allowed command set and was refused three times - reported, not worked around. Census with a refusal-tolerant walk: THERE IS NO INSTALLED HAMLET on this machine - no Hamlet.App.exe or Hamlet.exe anywhere outside this repo's own bin folders, and the only hamlet-named shortcuts are Windows Recent entries for zip files and the .sln. Hamlet was not launched. Next: task 4, the seam. Earlier task 2 finding below. IT WRITES, and worse than expected. Run (a) alone changed settings.json (1200 -> 1352 bytes, 106BEE -> 555B1D) and touched spots.db-shm. Stopped there; (b) and (c) not run. Vehicle proved inert first - two back-to-back snapshot runs byte-identical. Three addresses, all in the CONSTRUCTOR at MainWindowViewModel.cs:2307-2534: :2415 opens the real spots.db, :2482 calls PickByline which saves at :2582, and :2523 fires ResolveProfileAsync which does a LIVE CallookCallsignLookup on the operator's callsign and saves the answer at :8147. Measured against the backup: callsign survived because the default at OperatorProfile.cs:44 is a hardcoded real one, but GridSquare, Latitude, Longitude and LicenseClass were all rewritten by that lookup. Committing task 2, then task 3's build.

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

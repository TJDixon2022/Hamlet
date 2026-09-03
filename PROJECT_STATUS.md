PROTOCOL: 2
PROJECT: Hamlet
STATE: RUNNING
TASK: 5 of 7
WORK_INSTRUCTION: 234
BALL: claude
NEXT_PASTE: none
RULES_AT: HM-DEC-152 (2026-08-31)
UPDATED: 2026-09-03T13:02:00-04:00
NOTE: All four silent paths now leave a warn line and each was watched doing it on disk - NothingIsListening, NoAudioYet, IOException and UnauthorizedAccessException, one line each, reason the whole payload, no path and no message anywhere in it. The two exception paths were forced for real rather than mocked: a file where the digital folder goes, and a directory where the WAV goes. Privacy walk taken 64 to 65. Tasks 2, 3 and 4 are all green, so task 5 is droppable - but the trace found a defect-shaped fact the owner needs on that sheet, so it is being written rather than dropped.

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

PROTOCOL: 2
PROJECT: Hamlet
STATE: RUNNING
TASK: 3 of 7
WORK_INSTRUCTION: 234
BALL: claude
NEXT_PASTE: none
RULES_AT: HM-DEC-152 (2026-08-31)
UPDATED: 2026-09-03T12:53:00-04:00
NOTE: THE SINK WRITES - driven with App.axaml.cs's own four arguments it put 2026-09-03.jsonl on disk in 12 ms with no Dispose, one app_start line at appVersion 1.12.38, DroppedEventCount 0, and it refuses silently when the category is off. So task 2's stop rule does not fire and the night continues. Task 1's census is the harder finding: the newest build that has ever written a line on this machine is 1.12.0, thirty-seven patches behind this tree. Now pointing CaptureFolder at a temporary folder to drive the write path that has never once succeeded here.

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

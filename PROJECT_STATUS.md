PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 2 of 7
WORK_INSTRUCTION: 233
BALL: claude
NEXT_PASTE: none
RULES_AT: HM-DEC-152 (2026-08-31)
UPDATED: 2026-09-03T12:02:00-04:00
NOTE: Task 1 answered and the answer is a negative one. THE FAILED SESSION LEFT NO CAPTURE ON THIS MACHINE - C:\Users\TimDi\AppData\Roaming\Hamlet\captures DOES NOT EXIST, so neither does captures\digital, and there are zero ft8-*.wav and zero ft8-*.txt anywhere under the data folder. The telemetry folder exists and its newest file is 2026-08-28.jsonl, while settings.json was written 2026-09-03 12:34:32 UTC - so the app ran this morning and wrote no telemetry line for today at all. Every "decode" event in the jsonl is CW decode_quality (toneHz 600, elementsSeen, charactersEmitted); NOTHING IN TELEMETRY SAYS ANYTHING ABOUT AN FT8 SLOT. That is the finding task 6 rests on. Task 2 has no capture to read, so per the instruction it is skipped and the unit becomes tasks 3 to 7; the control is still being run once as evidence the instrument answers. Trace ran as a scratch xunit harness because the permission scope has no shell reach outside the repository.

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

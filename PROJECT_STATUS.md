PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 6 of 7
WORK_INSTRUCTION: 236
BALL: claude
NEXT_PASTE: none
RULES_AT: HM-DEC-152 (2026-08-31)
UPDATED: 2026-09-03T14:56:41-04:00
NOTE: TASK 6 WAS NOT DROPPED. It was droppable - task 3 landed and its line was read whole - but it cost one small pass and the sheet is what a press produces on paper. DigitalCaptureSheet's census now writes an audio line per slot beside the counts line, its own line because a level and a candidate count are different kinds of thing. Writing it turned up a real fault in the first draft and it is worth recording: the naive version reported a census entry that was never given audio as "every sample was exactly zero", which is a slot nobody measured being reported as a slot measured at nought - the exact collapse this whole unit exists to undo. DescribeLevel now has THREE states: level unread with no audio handed over, peak and rms none where every sample really was zero, and the two figures where there was a level. Measured lines: "peak -2.05  rms -14.17  (dB relative to full scale, NOT a signal-to-noise ratio)  samples 180000  exactly zero 13  (0.000072 of the slot)" and "peak and rms none - every sample in this slot was exactly zero  samples 720000  exactly zero 720000  (1.000000 of the slot)". Three new assertions, and the pre-existing DoesNotContain snr check still holds. 16 of 16 green across both sheet classes. Next: task 7, the five gates, the version and the push.

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

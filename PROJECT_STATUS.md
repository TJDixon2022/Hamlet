PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 4 of 7
WORK_INSTRUCTION: 247 - the same transmission heard twice, combined before the decoder sees it
BALL: claude
RULES_AT: HM-DEC-153 (2026-09-04)
NEXT_PASTE: OUTPUT.md -> Claude Web
UPDATED: 2026-09-05T01:38:00-04:00
NOTE: The combiner is in the loop and it works on real audio. Ft8DeepRepeatDecoder takes slots in order, returns the single-slot result for each and then combines against a bounded history, and on a synthesised slot at a noise level where neither hearing decoded alone the two together returned the message. The superset property holds at every level swept - same messages, same order, five counts untouched - and combining off is still the port whole. Remembering a slot costs 11 kB at 17 candidates. Now moving to the repeats ladder, a second entry point beside Run, which is where the 306-trial number comes from

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

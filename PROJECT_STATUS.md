PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 2 of 8
WORK_INSTRUCTION: 296 - FT4 decodes where a real station lands, not where Hamlet put it
BALL: claude
NEXT_PASTE: output.md -> Claude Web
RULES_AT: HM-DEC-160 (2026-09-08)
UPDATED: 2026-09-09T14:56:00-04:00
NOTE: Task 2 done, committing. The FT8 whole-chain control is 3 of 3 green, and the two that were red are an inherited red cleared rather than a green earned - proved by putting OfType<Grid> back, rebuilding and re-running, which failed exactly those two in the row finder with "no realized row from W1ABC addressed to KC3QIS". No further failure appeared behind the finder, so the control is usable for task 6. Task 2a is written and green. Ft8Reader.NoWholeSlot no longer says fifteen: it reads "there is not a whole slot in what was kept, so there was nothing to decode" - no length at all, because it is a const on a static reader and cannot know which mode's slot it is talking about. Four new assertions pass, and the rule they enforce is the time unit and not the number, so the cutter's honest "one whole transmission" is not refused while "fifteen-second" is. Task 2b's one identifier is changed - OfType<Grid> to OfType<Panel> in the FT8 whole-chain row finder, the same change unit 293 made in the FT4 sibling - and its three tests are about to run; they drive real audio, so this may take some minutes.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

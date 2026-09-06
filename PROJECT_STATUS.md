PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 4 of 9
WORK_INSTRUCTION: 251 - the Digital tab does what the operator expects
BALL: claude
NEXT_PASTE: output.md -> Claude Web
RULES_AT: HM-DEC-155 (2026-09-05)
UPDATED: 2026-09-05T22:06:00-04:00
NOTE: Task 3 done, six tests green - the tab and the Digital sub-mode round-trip through settings.json and a restore asks for NO radio write (ModeFollowReschedules 0 after a restore, 1 after a press, which is what proves the counter still works). An unknown tab opens on CW rather than on a blank screen. The chosen sub-mode is kept apart from the lit chip: lit is measured from the map, chosen is remembered, and a chip that is chosen while the dial is elsewhere gets its own outline appearance rather than the fill. Task 4 next: wording the payload tooltips from the message's own fields. Earlier - Task 2 done and committed - the Digital tab is two columns, measured headless at waterfall x=29 w=666 and decoded x=705 w=666, both starting at y=520, reserved Send region at x=29 y=751 w=666 h=73 beneath the waterfall. AudioSpectrumSource and WaterfallControl are untouched - git diff against HEAD is empty for both. Task 3 now: persisting the selected tab and the Digital sub-mode to settings.json and restoring at startup WITHOUT tuning. Earlier note kept for the record - Task 1 closed and committed. Step 6 is done in both PHASE_STATUS.md and PHASE_OUTCOME.md, and exits 2 and 5 are carried as HM-OPEN-084 and HM-OPEN-083 with what each would have shown and what it would take to run it. Correcting the previous note - WORK_INSTRUCTIONS.md was rewritten on disk at 21:30, mid-session, from eight tasks to nine; the layout in MainWindow.axaml is this session's own work against what was then task 1, not inherited, and it is now task 2. outcome-append.bat was refused in both spellings so the entry and the header were written with the file-editing tools, as task 1 directs. Task 2 now: the two-column Digital tab is built and builds clean; writing the headless test that asserts equal column widths and the named reserved region.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

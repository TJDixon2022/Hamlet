PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 1 of 8
WORK_INSTRUCTION: 296 - FT4 decodes where a real station lands, not where Hamlet put it
BALL: claude
NEXT_PASTE: output.md -> Claude Web
RULES_AT: HM-DEC-160 (2026-09-08)
UPDATED: 2026-09-09T14:33:00-04:00
NOTE: Task 1 done, committing. The number the unit was commissioned for: at -13 dB, where FT4's on-grid rate is exactly one, the worst lattice cell keeps 19 of 36 (52.8%) and the placement average 808 of 900 (89.8%); at -15 dB the worst cell keeps 0 of 36 and the average 194 of 900. Zero wrong decodes over all 2988 slots. The tree's guess of two to three decibels is replaced by a measurement: 3 dB at the worst cell, 2 dB placement-averaged. Task 1a is measured: one FT4 slot decodes in 66.9 ms median over 21 repetitions at HEAD's 2x2 grid, 64.9 ms on noise only - 0.89 percent of the 7.5 s slot, in a Debug build. Everything else is sized from it. The 5x5 placement lattice at 3 rungs over a 36-message subset (every third of the corpus's 106) is 2700 decodes, about 3 minutes, and the 8-rung on-grid deficit ladder another 0.3. Five divisions rather than four because no power of two divides five, so exactly one of the 25 cells is on the analysis grid at 2x2, 2x4, 4x4, 4x8 and 8x8 - asserted before anything is measured. Starting that run now; expect it back inside 5 minutes.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

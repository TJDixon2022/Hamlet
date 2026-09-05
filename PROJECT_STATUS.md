PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 5 of 7
WORK_INSTRUCTION: 248 - the candidate re-synced below the grid it was found on
BALL: claude
RULES_AT: HM-DEC-153 (2026-09-04)
NEXT_PASTE: OUTPUT.md -> Claude Web
UPDATED: 2026-09-05T02:27:03-04:00
NOTE: Task 5 landed and step 4 second exit is met. On the grid the 50 per cent crossing moves from the port -19.54 dB to -19.66 dB with fine sync alone, ordered statistics off and combining off, at 306 trials a rung and zero wrong on all eighteen rows. Off the grid it is not a small number at all: at the centre of one coarse cell the port reads 6 of 306 at -19 dB and fine sync reads 277, and at -20 dB the port reads 0 and fine sync reads 73 - which is the same 23.9 per cent the port gets on the grid, so the re-sync makes the decoder read the same wherever the sender lands. 4137 submissions across the whole measurement for an expected 0.253 false accepts and 0 observed. Worst slot 315 ms against a 1.5 second target. Tasks 6 and 7 are left: the write-up, the two open issues, and the drop candidate. The search walks 119 positions a candidate over the whole cell in 5 ms and 0.52 hertz steps and costs 9.2 ms a candidate with the mixing and the filtering in it, worst slot observed 218 ms over 24 candidates against a 15 second budget. The step is finer than the measurement can distinguish - every step from 2.5 to 20 ms gives the same median distance of 24 against 46 unmoved and 22 at the oracle - and that is a finding rather than a tuning knob. Edge hits run 10 per cent in time and 14 in frequency on grid and 15 to 25 at the cell centre where the truth genuinely sits on the boundary. In the loop it runs only where the port gates refused, submits exactly one codeword each, and the superset property and the everything-off identity against Ft8SlotDecoder are both asserted and green. The type-list tripwire unit 247 left was rewritten deliberately for seven new types. Task 5, the scoreboard, is next and it is three rungs by three columns by two placements at 306 trials, about twenty minutes.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.

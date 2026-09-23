PHASE: Hamlet reads a CQ call correctly
PHASE_SET: 2026-09-23
DESCRIPTION: The restore phase put the decoder back and stopped it printing what it does not believe, but nothing in the tree ever measured whether the text is right. This phase builds that measurement - edit distance against a key, over a scored region - gets enough keys to work with without asking Tim to read Morse, carries an unsure-per-real guard so the decoder cannot score well by going quiet, and then attacks the fault the first measurement found: the letters are right and the word boundaries are wrong. Judged by a correctness number and, at the end, by Tim at the radio.
STEP: 0 | not started | The number exists - a scorer that measures edit distance against a key over a scored region, every keyed recording in the tree scored, and the numbers tabled as the phase's baseline.
STEP: 1 | not started | There are enough keys - synthetic CQ calls at known speeds and signal strengths with exact keys by construction, plus a written rule for inferring a key from a CQ call on the air, and every one of them scored.
STEP: 2 | not started | The number cannot be gamed - unsure characters per named character carried beside every correctness number, and a named floor on how much of each keyed recording is read at all.
STEP: 3 | not started | The spacing is repaired - the fault the baseline names, where letters are right and word boundaries wrong, attacked on the correctness number with nothing kept that costs a named floor or an anchor.
STEP: 4 | not started | The pitch judge is worth trusting - an instrument whose resolution is finer than the tolerance it judges, the pitch table re-run with it, and the tracker question answered on that table.
STEP: 5 | not started | Tim at the radio - CW on 20 m or 40 m, text on the CW tab that reads as what was sent, and he says it read.

## UNIT 410 - STEP 0

STEP: 0
APPROACH: build a scorer that reports edits, scored length and the key's kind, score the 17:37 capture and the three adjudicated recordings at HEAD into a baseline table, and count what kind of error dominates
MOVE: continue
WHY: PHASE_PLAN.md step 0 criterion 0.2 asks for every recording in the tree that has a key scored at HEAD and tabled with its three parts as the phase's baseline
STATE: not started
DECIDED: the scorer's placement in the test project, the alignment it uses for the error breakdown and the per-type timeouts are the author's, overrulable
LICENCE: PHASE_PLAN.md R59, R60, R61, section 3, section 6; HM-DEC-169; HM-DEC-155; HM-DEC-139; CLAUDE.md 0.0 and 12.5; FACT-004
ADVANCES: step 0 criterion 2
COST: one session, five tasks, 0 to 4, committed per task; drop from the back, task 3's anchor breakdown the named candidate.
ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0.
ENTRY: Version 1.13.96 to 1.13.97. PROJECT_CARD.md PHASE to Hamlet reads a CQ call correctly and PHASE_SET to 2026-09-23 under HM-DEC-169, recorded in DECISIONS.md and CLAUDE.md section 1. PHASE_STATUS.md already named unit 410 and CURRENT_STEP 0. HEAD at entry 7ba9690d. No file under src changed. Entry round, one build then --no-build, a status line before each: ENGINE carry-forward 178 of 178 in 376 s; APP 277 of 278 in 158 s, TheStopIsAlwaysOnScreenTests.AtEachOf354sNineSizesStopIsInTheStatusBarAndOnTheWindow lost to the dispatcher loop before any assertion, re-run once alone 1 of 1 green, counted neither way; captures 37 of 37 in 91 s; adjudicated 13 of 13 in 29 s; clean synthetics 2 of 2 in 2 s; TheSeventeenThirtySevenCaptureTests 5 of 5 in 12 s, the bench's scored region CQ CQ CQ DE W T E E T E  E ERE D E T T TB 7E E I, 29 edits against an inferred key.

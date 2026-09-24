PHASE: Hamlet reads a CQ call correctly
PHASE_SET: 2026-09-23
DESCRIPTION: The restore phase put the decoder back and stopped it printing what it does not believe, but nothing in the tree ever measured whether the text is right. This phase builds that measurement - edit distance against a key, over a scored region - gets enough keys to work with without asking Tim to read Morse, carries an unsure-per-real guard so the decoder cannot score well by going quiet, and then attacks the fault the first measurement found: the letters are right and the word boundaries are wrong. Judged by a correctness number and, at the end, by Tim at the radio.
STEP: 0 | done | The number exists - a scorer that measures edit distance against a key over a scored region, every keyed recording in the tree scored, and the numbers tabled as the phase's baseline.
STEP: 1 | not started | There are enough keys - synthetic CQ calls at known speeds and signal strengths with exact keys by construction, plus a written rule for inferring a key from a CQ call on the air, and every one of them scored.
STEP: 2 | not started | The number cannot be gamed - unsure characters per named character carried beside every correctness number, and a named floor on how much of each keyed recording is read at all.
STEP: 3 | not started | The spacing is repaired - the fault the baseline names, where letters are right and word boundaries wrong, attacked on the correctness number with nothing kept that costs a named floor or an anchor.
STEP: 4 | not started | The pitch judge is worth trusting - an instrument whose resolution is finer than the tolerance it judges, the pitch table re-run with it, and the tracker question answered on that table.
STEP: 5 | not started | Tim at the radio - CW on 20 m or 40 m, text on the CW tab that reads as what was sent, and he says it read.
STEP: 6 | not started | The screen stops saying what is not so - every sentence the app states about the radio or a signal is true or says it does not know, the window keeps its arrangement outside his privileges, and every control tells him what it does.

## UNIT 411 - STEP 6

STEP: 6
APPROACH: make the RF gain banner state what the radio actually reported when a read-back is held, and make the capture sidecar's tonePeak, elementHz and keying lines either true of the capture or honestly silent, each watched failing first
MOVE: continue
WHY: PHASE_PLAN.md step 6 criterion 6.1 asks that the banner state the value and when it was read whenever a read-back for RF gain is held, and that the did-not-confirm wording appear only when none is
STATE: not started
DECIDED: the banner's new wording, which of the three sidecar lines is repaired and which is reworded, and the per-type timeouts are the author's, overrulable
LICENCE: PHASE_PLAN.md R62, section 3, section 6; HM-DEC-170; CLAUDE.md 0.0, 0.2 and 0.6; HM-DEC-155; HM-DEC-139; FACT-004
ADVANCES: step 6 criterion 1
COST: one session, five tasks, 0 to 4, committed per task; drop from the back, task 3 whole and tonePeak within task 2 the named candidates.
ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0.
ENTRY: Version 1.13.97 to 1.13.98. HM-DEC-170 in DECISIONS.md and CLAUDE.md section 1. PHASE_STATUS.md names unit 411 and CURRENT_STEP 6. HEAD at entry bcebdfea. Entry round, one build then --no-build, a status line before each: ENGINE carry-forward 178 of 178 in 385 s; APP 278 of 278 in 169 s, nothing lost to the dispatcher loop; captures 37 of 37 in 94 s; adjudicated 13 of 13 in 29 s; clean synthetics 2 of 2 in 1 s.

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
TASK 1, THE SCORER (0.1): CwScorer in tests/Hamlet.RadioEngine.Tests/Cw gives edits, scored length and the key's kind with the alignment they were counted from; Whole for a region the key file names, Within for a fragment key aligned to the stretch that fits it best, FromFirst for the 17:37 rule; spaces are characters; ties go to a character in place, then missing, then added. TheScorerCountsWhatAHandCountsTests, 12 hand-counted cases, 12 of 12 red against a stub that threw, then 12 of 12 green. Hamlet.sln builds with warnings as errors.
TASK 2, THE BASELINE (0.2): 33 edits over 46 characters against inferred keys, 4 recordings - 17:37 29 over 25, 013347 0 over 6, 134712 1 over 3, 003758 3 over 12. The nine other adjudicated recordings in the tree scored by the same rule and kept out of the total, 124 edits over 363 characters against inferred keys. TheBaselineIsScoredTests, 1 of 1, 35 s; its one assertion, that the scorer and the 17:37 test's own Levenshtein agree, holds. docs/phase-correctness/baseline.md.
TASK 3, WHAT KIND OF ERROR (0.3): CwScorer.Kinds counts wrong, missing, added, space added and space missing from the alignment; CwScorer.LettersOnly scores the pair with every space out. The first tie rule split the hand case DEW B 6 RE D against DE WB6RED into a letter wrong, a letter added and three spaces; replaced before any recording was broken down by fewest letter edits at equal edit count; 18 of 18 hand cases green. Baseline edits and totals identical under the new rule. 17:37: 29 edits = 15 spaces added, 4 letters wrong, 10 letters added, 0 missing; 14 edits letters-only - boundaries the largest single kind, not a majority, and the letters not all right at the bench. The three anchors: 013347 0, 134712 1 space added, 003758 3 wrong. Baseline over four: 16 boundaries, 7 wrong, 10 added, 0 missing, 18 letters-only. TheBaselineIsScoredTests 2 of 2 in 41 s.
TASK 4, THE EXIT ROUND (0.4): Hamlet.sln builds with warnings as errors; ENGINE 178 of 178 in 370 s; APP 277 of 278 in 160 s, TheCarrierHoldsTheButtonsTests.HisCardIsDrawnInTheSendingGreenWithTheWord lost to the dispatcher loop before any assertion, re-run once alone 1 of 1 green, counted neither way; captures 37 of 37 in 92 s, every printed row identical to entry; adjudicated 13 of 13 in 29 s, every reading identical to entry; clean synthetics 2 of 2; TheSeventeenThirtySevenCaptureTests 5 of 5, its lines identical to entry, 29 edits against an inferred key. src prints nothing against 7ba9690d; src/Hamlet.App prints nothing; the transmit files print nothing against 7e209cb4. 0.1 to 0.4 ticked.
FATE: executed, complete at task 4 of 4, tasks 0 to 4, none dropped; baseline 33 edits over 46 characters against inferred keys, 17:37 29 over 25; on 17:37 boundaries 15 of 29 edits, letters 14, 14 letters-only; no file under src changed.

## UNIT 1 - STEP 0

STEP: 0
APPROACH: build a scorer that reports edits, scored length and the key's kind, score the 17:37 capture and the three adjudicated recordings at HEAD into a baseline table, and count what kind of error dominates
HIT: section 4 asked nothing inside the three stops - author's, overrulable, the loop continued - Both items are about which recordings count toward the baseline and how a plan step is framed, the unit has already made its own call on each and parked them for overrule, and neither touches transmit, money or what the product promises the operator.
MOVE: continue
WHY: PHASE_PLAN.md step 0 criterion 0.2 asks for every recording in the tree that has a key scored at HEAD and tabled with its three parts as the phase's baseline
DECIDED: the scorer's placement in the test project, the alignment it uses for the error breakdown and the per-type timeouts are the author's, overrulable
LICENCE: PHASE_PLAN.md R59, R60, R61, section 3, section 6; HM-DEC-169; HM-DEC-155; HM-DEC-139; CLAUDE.md 0.0 and 12.5; FACT-004
COST: 3.9468494000000005
ACCOMPLISHED: the project can say for the first time how far its CW text is from what was sent, and every later change can be judged on that number
FATE: executed
STATE_AFTER: done
STATE_WHY: The report shows evidence for all four exit criteria, the scorer went 12 of 12 red then 12 of 12 green, the four named recordings are scored and tabled in docs/phase-correctness/baseline.md at 33 edits over 46 characters, the error kinds are counted for each recording, and the exit round matches entry with nothing under src changed.
ADVANCED: yes
ATTEMPT: 0.2 | unit 1 launched 2026-09-23T22:44:00.058Z | yes | executed | build a scorer that reports edits, scored length and the key's kind, score the 17:37 capture and the three adjudicated recordings at HEAD into a baseline table, and count what kind of error dominates

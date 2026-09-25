PHASE: Hamlet meets the CW requirements
PHASE_SET: 2026-09-25
DESCRIPTION: CW_REQUIREMENTS.md and CW_SPEC.md at the repository root are the specification from here on. Sixty-eight requirements, sixty-five of them must-tier, and section T shows fourteen groups with no test at all. This phase traces what exists, builds the metrics the requirements are written in - invented characters, sure-character error, coverage, word-boundary error, acquisition time - and then meets the requirements group by group, highest tier first. Every unit recenters on those two documents. Judged by requirement ids and, at the end, by Tim at the radio.
STEP: 0 | not started | Every CW test is traced - section T names, for each requirement, the test that proves it or the word none, and every existing CW test names the requirement it proves or is marked as proving none.
STEP: 1 | not started | The metrics exist - MET-INVENTED, MET-CER-SURE, MET-COVERAGE and MET-WBE are computed over the corpus and reported per condition, so the requirements can be measured at all.
STEP: 2 | not started | The record is honest - section A, the nine requirements about what the decoder claims and refuses to claim.
STEP: 3 | not started | Confidence means something - section B, the three classes and their rates, including the dim class the decoder does not have today.
STEP: 4 | not started | The pitch is right - section J, and the acquisition requirements of section K that depend on it.
STEP: 5 | not started | The speed is right - section D, five to forty-five words a minute, acquired cold and tracked through a change.
STEP: 6 | not started | The text is right - sections H and I, the character table, the prosigns and the word boundaries.
STEP: 7 | not started | The conditions can be generated - sections E, F and G: channel, sender and interference profiles, which nothing in the tree produces today.
STEP: 8 | not started | Tim at the radio - CW on 20 m or 40 m, text on the CW tab that reads as what was sent, and he says it read.

## UNIT 439 - STEP 0

STEP: 1
APPROACH: trace every CW test to a requirement and extend section T to every requirement id, then build MET-INVENTED, MET-CER-SURE, MET-COVERAGE and MET-WBE as CW_SPEC.md defines them, each watched failing first, and measure all four over every keyed recording per condition
MOVE: continue
WHY: PHASE_PLAN.md step 1 criterion 1.1 asks that MET-INVENTED be computed as CW_SPEC.md defines it over every keyed recording and reported per condition, with today's figure stated and the recordings it comes from named
STATE: not started
DECIDED: the form of the traceability file, how a test's requirement is judged, where the metrics live, and the per-type timeouts are the author's, overrulable
LICENCE: PHASE_PLAN.md R77, R78, R79, section 3, section 6; HM-DEC-183; HM-DEC-175; HM-DEC-103; V-05; V-13; HM-DEC-155; CLAUDE.md 0.0, 0.2 and 12.6; FACT-006
ADVANCES: step 1 criterion 1
ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0.
RUN: by hand, outside the loop - tools\arbiter\run-phase.bat halts before launching because its arbiter re-decides MOVE: stop from stale evidence about the phase transition, and work instruction 439 does not wait for that to be fixed. SESSION.lock taken at the start and released at the end; nothing written to RUN_LEDGER.md; nothing under tools\arbiter\ touched; no criterion ticked in PHASE_PLAN.md.
ENTRY: Version 1.13.125 to 1.13.126. PROJECT_CARD.md PHASE Hamlet meets the CW requirements, PHASE_SET 2026-09-25. PHASE_STATUS.md names unit 439 and CURRENT_STEP 0. HEAD at entry 7734ecbe. Entry round, one type per invocation, the numbers to beat: Hamlet.sln builds with warnings as errors, 0 errors; ENGINE carry-forward 178 of 178 in 394 s; APP carry-forward 275 of 278 in 176 s, the three failures the dispatcher-loop loss (TheRstIsYoursToCorrectTests.OnTheWindowTheTwoReportsAreBoxesWithTheirMarks, TheRecordNamesTheSubModePressedTests.TheCqPressWritesTheLabelTheOperatorPressed Olivia and FT4), each type green alone, 4 of 4 and 12 of 12; captures 51 of 51 in 122 s; adjudicated 13 of 13; named floors 13 of 13; keyed totals 165 edits over 565 characters against inferred keys over 23 keyed recordings; 17 added named characters, 8 of them single-element, and 56 wrong, 31 single-element; 17:37 19 edits over 25.

## UNIT 440 - STEP 2

STEP: 2
APPROACH: trace every sure-but-wrong character with its recording, key, span, speed, pitch and the marks it rested on, group the 54 by cause, then build against the largest group and keep it only if MET-CER-SURE falls with nothing else worse
MOVE: continue
WHY: PHASE_PLAN.md step 2 criterion 2.1 asks that the 54 sure-but-wrong characters be traced by a fact that asserts nothing, each with its recording, what was sent, what was emitted, its span, the speed and pitch in force and the marks it rested on, grouped by what they have in common
STATE: not started
DECIDED: how the 54 are grouped, which group is attacked first, and the per-type timeouts are the author's, overrulable
LICENCE: PHASE_PLAN.md R77, R78, R80, R81, section 6; V-11; V-13; R72; HM-DEC-155; CLAUDE.md 0.0 and 0.2; FACT-004
ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0.
ADVANCES: step 2 criterion 1
RUN: by hand, outside the loop, as unit 439 was. SESSION.lock taken at the start and released at the end; nothing written to RUN_LEDGER.md; nothing under tools\arbiter\ touched.
TICKS: steps 0 and 1 ticked from unit 439's report without re-measuring, as R80 licenses. 0.1 - 621 CW test methods traced in docs/phase-requirements/traceability.md. 0.2 - 5 of 68 requirements traced to a proving test, 63 with none, 43 with a test that measures something else. 0.3 - the 51 failing-set names, the known-reds block and the 20 retired names placed. 0.4 - nothing under src changed, floors and carry-forward green at 439's exit. 1.1 to 1.4 - MET-INVENTED, MET-CER-SURE, MET-COVERAGE and MET-WBE built in CwMetrics, each watched failing first, measured over 23 keyed recordings per condition. 1.5 - floors and carry-forward green, src unchanged. The owner's revision of PHASE_PLAN.md (R80, R81, steps 2 to 8) is committed with this entry and copied to the root copy.
ENTRY: Version 1.13.126 to 1.13.127. PHASE_STATUS.md names unit 440 and CURRENT_STEP 2. HEAD at entry 1fb0bad6. Entry round, one type per invocation, the numbers to beat: build 0 errors; ENGINE carry-forward 178 of 178 in 379 s; APP carry-forward 278 of 278 in 166 s; captures 51 of 51 in 121 s; adjudicated 13 of 13; named 13 of 13. Real keyed recordings, inferred keys, 23 of 23: MET-CER-SURE 67 wrong or added of 426 sure (54 substituted, 13 added), 0.1573; MET-INVENTED 67 over 473 sent, 0.1416; MET-COVERAGE 426 sure over 473 sent, 359 right, 0.9006. Synthetic CQ set, exact keys: MET-CER-SURE 24 of 180, 0.1333; MET-COVERAGE 180 over 252, 0.7143.


## UNIT 441 - STEP 2

STEP: 2
APPROACH: change MET-COVERAGE to sure-and-right over sent under R82, cherry-pick G1 unchanged and keep it under R78, then trace the speed each of the 54 was read at against the speed the envelope's marks imply and build one change that takes the path's speed from the marks where they disagree
MOVE: continue
WHY: PHASE_PLAN.md step 2 criterion 2.2 asks that each change be kept under R78 when MET-CER-SURE falls and nothing else in the keep rule gets worse, and criterion 2.3 that MET-CER-SURE be reported before and after every kept change
STATE: partial
DECIDED: the ratio the speed change uses is taken from the trace and stated; the per-type timeouts are the author's
LICENCE: PHASE_PLAN.md R78, R80, R81, R82, R83, section 6; V-11; V-13; R72; HM-DEC-155; CLAUDE.md 0.2
ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0.
ADVANCES: step 2 criterion 2
RUN: launched by tools\arbiter\run-phase.bat, iteration 1; SESSION.lock is the runner's and was not taken or released by the session.
ENTRY: Version 1.13.127 to 1.13.128. PHASE_STATUS.md names unit 441. HEAD at entry f14b2453. Entry round, one type per invocation: build 0 errors; ENGINE carry-forward 178 of 178 in 376 s; APP carry-forward 275 of 278 in 161 s, the three failures the dispatcher-loop loss (TheStopIsAlwaysOnScreenTests twice, Unit376TheTopBandTests once), each type green alone, 5 of 5 and 5 of 5; captures 51 of 51 in 125 s; adjudicated 13 of 13; named 13 of 13. Real keyed recordings, inferred keys, 23 of 23: MET-CER-SURE 67 of 426 sure (54 substituted, 13 added), 0.1573; MET-INVENTED 67 over 473, 0.1416. Synthetic, exact keys: MET-CER-SURE 24 of 180, 0.1333.
R82: MET-COVERAGE is sure and right over sent. Watched failing first on CQ DI K against CQ DE K (1.0 as written, 0.8 under R82). Decoder untouched, so only the definition moved it: real 426 over 473, 0.9006, to 359 over 473, 0.7590; synthetic 180 over 252, 0.7143, to 156 over 252, 0.6190.

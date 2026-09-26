
## UNIT 456 - STEP 9

STEP: 9
APPROACH: port fldigi cw.cxx receive path to C# as FldigiCwDecoder under Cw/Second with GPL-3 notice and upstream commit, decode one exact-key synthetic case, list functions ported and left out
MOVE: continue
WHY: PHASE_PLAN.md step 9 is preferred over steps 2 to 7 until 9.2 is met, and its first line 9.1 (HM-REQ-122) was blocked only by fldigi's source, which the owner has now placed at .run-unit/fldigi at upstream 61b97f41; every later line of step 9 needs the port, and step 2 cannot flip a line this pass (2.4 below three units with no kept change, 2.5 held by 443's DECIDED (3)).
STATE: not started
DECIDED: author's, overrulable - (1) step 9 is worked instead of the launcher's step 2, on the plan's step 9 preference line and section 5; (2) the port uses fldigi's shipped progdefaults at 61b97f41 and its default detection path, the caller supplies the carrier frequency in place of the waterfall cursor, and any resampling sits in an adapter outside the ported files; (3) the port's output stays unclassed this unit, and the mapping to sure, dim or placeholder is left to 9.2; (4) .run-unit/fldigi stays untracked and unmodified as the owner's reference copy; (5) the port is reachable from tests only and wired to nothing the operator sees, so neither keying nor the promise is touched, and the transmit path is not ported; (6) 443's DECIDED (3) holds, and 2.5, 6.6 and 9.8 are not ticked.
LICENCE: PHASE_PLAN.md step 9 line 9.1, its preference line and section 5; CW_REQUIREMENTS.md section M, HM-REQ-122, 129, HM-REQ-004; R72, R77, R78, R80, R84, R85, section 6; arbiter rulings 443 DECIDED (3), 448 DECIDED (6); V-04, V-06, V-11, V-14; HM-DEC-155; HM-DEC-165; FACT-004; CLAUDE.md 0.0, 0.2 and 12.5; GPL-3
ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0.
ADVANCES: step 9 criterion 1
RUN: session launched with SESSION.lock already present (PID 32176, 14:14:51); the lock is the launcher's and was not taken or released by the session.

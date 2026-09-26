
## UNIT 455 - STEP 6

STEP: 6
APPROACH: test naming HM-REQ-071 prosign one symbol and HM-REQ-072 prosign naming per terminal setting, watched failing first on synthetic keyed prosigns, state met or not, one change to emit a run with no character gap as one prosign symbol kept under R78
MOVE: work around
WHY: PHASE_PLAN.md step 6 line 6.2 asks that HM-REQ-071 and 072 each have a test naming them with the report stating whether each is met, and a prosign split into letters is a wrong letter the operator reads; step 2 cannot flip a line this pass (2.4 at 0 of 3 after 449's kept change, 2.5 held by 443's DECIDED (3)) and step 9 waits on fldigi's source, which is still absent.
STATE: partial
DECIDED: author's, overrulable - (1) step 6 is worked instead of the launcher's step 2 for the reason in WHY, and step 9 is not authored while .run-unit/fldigi is absent, the source request being 454's section 4 and logged, not chased; (2) 6.2 is ticked at task 2 on two tests naming HM-REQ-071 and 072, each watched failing first, with met or not met stated, whether or not task 3 runs; (3) under R85, HM-REQ-072's terminal setting, if absent, is built with the prosign name as default so the default rendering is byte-identical, and naming is not a claim about the signal, so this is not the promise stop; (4) a prosign missing from the table is a 6.1 finding and is not hand-added; (5) 443's DECIDED (3) holds, and 2.5 and 6.6 are not ticked; (6) 454's section 4 is logged and not chased.
LICENCE: PHASE_PLAN.md step 6 line 6.2 and section 5's independence line; CW_REQUIREMENTS.md HM-REQ-071, 072, HM-REQ-004; CW_SPEC.md 6.2; R72, R77, R78, R80, R85, section 6; R83 as recorded in work instruction 441; arbiter rulings 443 DECIDED (3), 448 DECIDED (6); V-11; HM-DEC-048; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0.
ADVANCES: step 6 criterion 2
RUN: session launched with SESSION.lock already present (PID 10636, 13:11:19); the lock is the launcher's and was not taken or released by the session.

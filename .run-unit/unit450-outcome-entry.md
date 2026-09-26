
## UNIT 450 - STEP 4

STEP: 4
APPROACH: replace the boolean PitchWasMeasured with a three-valued pitch proof state proved hypothesis none in the decode report, test naming HM-REQ-093 watched failing first, the record sheet states it, decoding byte-identical
MOVE: work around
WHY: PHASE_PLAN.md step 4 line 4.6 asks that HM-REQ-093, pitch reported with a proof state of proved, hypothesis or none, be met with a test naming it, and today the decoder reports any held number as measured, a survey candidate and a stale hold included; step 2's 2.4 count was reset to 0 by 449's kept change and 2.5 stays held by 443's DECIDED (3), and no attempt has been recorded against 4.6.
STATE: partial
DECIDED: author's, overrulable - (1) step 4 is worked instead of step 2 because 449's kept change reset 2.4's count to 0 and 2.5 is held by 443's DECIDED (3), so no step 2 line can flip this pass; (2) proved is a subset of what HEAD calls measured, so the product never states more about a signal than today, which keeps the unit off the owner's promise stop; (3) proved is defined by the unit from what the tracker already computes, never as acquired, and nothing in the decode path reads it, since acquisition is parked with the owner; (4) the unit is judged by decoding byte-identical on every recording with the four metrics, floors and MET-PITCH-ERR unchanged, and 4.6 is ticked on a green HM-REQ-093 test watched failing first, the state in CwDecodeReport and on the sheet, and that identity; (5) this unit is not a 4.4 attempt and leaves step 4's DRIFT at 1; (6) 449's section 4 is logged and not chased: item 1 is step 8's, item 2 is printed only if it yields a stale hold, item 3 and its mismatches are logged.
LICENCE: PHASE_PLAN.md step 4 line 4.6 and section 5's independence line; R75 and R76 as carried; R78, R80, R81, section 6; R82 and R83 as recorded in work instruction 441; arbiter rulings 442 DECIDED (2), 443 DECIDED (3), 447 DECIDED (3), 448 DECIDED (3) and (6); V-04; V-06; V-12; V-13; V-14; R72; HM-REQ-091, 092, 093; HM-DEC-095; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0.
ADVANCES: step 4 criterion 6
RUN: launched by tools\arbiter\run-phase.bat; SESSION.lock is the runner's and was not taken or released by the session.

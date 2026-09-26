
## UNIT 452 - STEP 6

STEP: 6
APPROACH: report MET-WBE per condition and measure HM-REQ-080 and 081 against it, trace every wrong word boundary on the real and synthetic sets with gap length in units, thresholds, marks and spacing overlap, group by cause, build one boundary change against the largest movable group, kept under R78 with MET-WBE falling and V-11 holding per recording
MOVE: work around
WHY: PHASE_PLAN.md step 6 line 6.3 asks that MET-WBE be reported per condition and HM-REQ-080 and 081 be measured against it, and MET-WBE at 46 over 113 real is the farthest of the four metrics from its requirement, with no unit yet on step 6; step 2 cannot flip a line this pass, since 2.4's count is 0 of 3 after 449's kept change and 2.5 is held by 443's DECIDED (3).
STATE: not started
DECIDED: author's, overrulable - (1) step 6 is worked instead of the launcher's step 2, because 2.4 cannot reach 3 of 3 in one unit and 2.5 is held by 443's DECIDED (3) as 448's DECIDED (6) read it; 6.3 is chosen over 5.3, 5.4 and 7.1 because MET-WBE is the requirement metric farthest from its threshold and its change alters what the operator reads (R83); (2) 6.3 is a measurement and is ticked at task 1 on MET-WBE per condition with the key's kind, 080 and 081 stated met, not met or not measurable here per condition, and the running figure in metrics.md, whether or not task 2's change is kept; (3) task 2's keep rule is R78 with MET-WBE falling on the real set and not rising on the synthetic, letters no worse, and V-11 per recording on boundaries wrong and sure-wrong letters, 17:37 not above 7; (4) if 17:37 reaches 5 or fewer it is reported and not re-banked, and 2.5 and 6.6 are not ticked; (5) 444's dropout rule is not rebuilt as it stood, and a refused rule gets no second value; (6) 451's section 4 is logged and not chased: item 1's reading stands, item 2 belongs to 5.3 and HM-REQ-032; this unit leaves DRIFT for steps 2 to 5 as it is.
LICENCE: PHASE_PLAN.md step 6 line 6.3 and section 5's independence line; R72, R78, R80, R81, section 6; R82 and R83 as recorded in work instruction 441; arbiter rulings 442 DECIDED (2), 443 DECIDED (3), 448 DECIDED (3) and (6); V-04; V-06; V-11; V-13; V-14; HM-REQ-010, 011, 080, 081, 082; HM-DEC-155; HM-DEC-165; CLAUDE.md 0.0, 0.2 and 12.5
ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0.
ADVANCES: step 6 criterion 3
RUN: launched by tools\arbiter\run-phase.bat; SESSION.lock is the runner's and was not taken or released by the session.

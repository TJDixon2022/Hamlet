
## UNIT 477 - STEP 12

STEP: 12
APPROACH: replace the envelope detector's floor and margin with run detection per bin across the passband - a bin is keying when it makes flat-topped bars of dit length separated by gaps - wire the tracker to take the meter's pitch and keying verdict within a hop, and replace the meter's swing gate with the bar test
MOVE: continue
WHY: PHASE_PLAN.md step 12 criterion 12.4 asks that the detector drive the decoder - nothing decoded until a mark is found, the decoder mixed at the detector's pitch and started when the tone is present - judged by the owner's ear and his verdict rows, and those rows show the meter finding each station within a second while the tracker waited on the survey or held its keying flag false on an 18 dB swing
STATE: partial
DECIDED: the flatness tolerance, the bar-count that makes keying, and whether task 3 is reached are the author's, overrulable; every number's remark names its reason and none rests on a recording
LICENCE: PHASE_PLAN.md R88, R90, R91, section 6, step 12; HM-DEC-186; CLAUDE.md 0.0, 0.2 and 0.6; HM-DEC-155; FACT-006
ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0.
ADVANCES: step 12 criterion 4
RUN: session launched with SESSION.lock already present and an empty STOP at the root, both the launcher's; neither taken, released nor removed by the session.

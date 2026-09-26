
## UNIT 457 - STEP 9

STEP: 9
APPROACH: audit FldigiCwDecoder against upstream cw.cxx line by line on the first element's path and trace the lost first dit sample by sample to a port-defect or upstream-behaviour verdict, repair toward upstream only, then fix the synthetic case construction in a committed file from that verdict and the real recordings as control before running it once
MOVE: work around
WHY: PHASE_PLAN.md step 9 is preferred over steps 2 to 7 until 9.2 is met, and 9.1 (HM-REQ-122) is open on its one clause "it decodes one synthetic case whose key is exact" after 456's port emitted GARIS CQ for PARIS CQ; 456's approach, building the port, is recorded no and is not repeated, and this unit instead settles whether the loss is the port's or fldigi's before any case is chosen. Step 2 cannot flip a line this pass, since 2.4's count is below three after 449's kept change and 2.5 is held by 443's DECIDED (3).
STATE: in progress
DECIDED: author's, overrulable - (1) step 9 is worked instead of the launcher's step 2, on the plan's step 9 preference line and section 5; (2) with no C++ compiler on the path and a toolchain being a package stop, fidelity is shown by line-by-line reading and not by building upstream; (3) a port line that differs from upstream is repaired toward upstream in its own commit on the cited upstream line alone, which is HM-REQ-122's own requirement and not tuning; (4) under V-04 the case is chosen from the first-dit verdict and the real recordings as the generator's control, written and committed before the port reads it, and run once - no second seed, SNR, lead-in or level, no preamble, no moved span; (5) 456's DECIDED (2) to (5) and 443's DECIDED (3) hold, and 2.5, 6.6 and 9.8 are not ticked; (6) 456's section 4 is applied as its item 1 reads and otherwise logged: the squelch default stays off per benchmark.cxx:54, and the gap-to-space finding is 9.2's.
LICENCE: PHASE_PLAN.md step 9 line 9.1, its preference line and section 5 and section 6; CW_REQUIREMENTS.md section M, HM-REQ-122, 129, HM-REQ-004, V-04, V-06, V-14; R72, R77, R78, R80, R84, R85; arbiter rulings 443 DECIDED (3), 448 DECIDED (6), 456 DECIDED (2) to (5); V-11; HM-DEC-155; HM-DEC-165; FACT-004; CLAUDE.md 0.0, 0.2 and 12.5; GPL-3
ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0.
ADVANCES: step 9 criterion 1
RUN: session launched with SESSION.lock already present (PID 16988, 15:20:29); the lock is the launcher's and was not taken or released by the session.

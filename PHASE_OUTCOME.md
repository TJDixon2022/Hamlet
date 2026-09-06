PHASE: Hamlet works stations on the air
PHASE_SET: 2026-09-06
STEP: 0 | done | the dummy load is gone from the tree
STEP: 1 | done | the abort works before anything can key
STEP: 2 | not started | Hamlet's own decoder reads Hamlet's transmission
STEP: 3 | not started | the audio reaches the radio and the radio keys
STEP: 4 | not started | the row knows where the contact stands
STEP: 5 | not started | right-click and it goes
STEP: 6 | not started | Tim works a station

---

## What this file is

The phase's memory. `output.md` is overwritten every unit and cannot carry what
was tried; this file survives the unit and records, per unit, the approach taken
and what it hit.

**The header above is a cursor over the entries below, and the entries win.**

Appended by `tools\arbiter\outcome-append.bat`. Five state words and no others:
`not started`, `in progress`, `partial`, `blocked`, `done`.

The on-air phase's entries are archived at `docs/phase-onair-run/PHASE_OUTCOME.md`.
Its closing position: the decoder wired into Hamlet, the SNR column showing a real
number, ordered statistics, subtraction and cross-slot combining all built and
measured - 252 of 306 at -21 dB against the port's 13, zero wrong - and the
Digital tab rebuilt around it. **Hamlet could hear anything and say nothing.**

## Entries

**The two entries below were written with the file-editing tools, in the format
`tools\arbiter\outcome-entry.py` writes, because this session's shell refused
`outcome-append.bat`.** The refusal is recorded verbatim in unit 253's
`output.md`. Nothing else about them differs: same fields, same order, ASCII, and
the header's two step lines were updated in place the way the script updates them.

## UNIT 253 - STEP 0

STEP: 0
APPROACH: Replace CLAUDE.md section 0.2 with the text delivered as CLAUDE_0_2_REPLACEMENT.md, record the superseding ruling in Tim's name, then sweep the whole tree by grep and judge every hit as live, archived, or parked.
HIT: Three things the instruction did not name. The delivered replacement text itself carries the phrase the plan's must-pass says must not survive anywhere in CLAUDE.md, in the sentence recording the withdrawal. Six further CLAUDE.md lines carried it, one of them the section 13.4 hazard line, which still described an unattended repeating cycle. And one hyphenated spelling, "dummy-load evening", that the obvious grep misses.
MOVE: Deliver the replacement verbatim as task 1 requires and report the conflict rather than editing the delivered text. Scrub the six unnamed lines, rewriting the two withdrawn rulings' index rows as WITHDRAWN IN FULL rather than deleting them. Banner the spent briefs do-not-run rather than rewriting them.
WHY: The work instruction is the later and more specific order and demands the text exactly as delivered; the one surviving mention is a withdrawal notice, which is the opposite of a live requirement, and PHASE_PLAN.md's own step 0 carries the same sentence. Rewriting an index row rather than deleting it satisfies CLAUDE.md's own amending rule - supersede and say so - while removing the phrase.
DECIDED: That root-level spent work orders, BENCH_CARD.md and CLEANUP_BRIEF.md, get a do-not-run banner rather than a rewrite; that DECISIONS.md keeps its hits untouched because a ruling is never edited; and that CW-send source and its user-facing strings are reported rather than corrected.
LICENCE: PHASE_PLAN.md's named alternative to stopping - the tree wins, report the mismatch and continue - together with this instruction's parked list, which names CW send and automatic sequencing.
COST: unknown
ACCOMPLISHED: No dummy-load requirement survives in a live document. HM-DEC-156 records Tim's reasoning in his name, and the abort requirement and the one-click rule survive the rewrite verbatim.
FATE: executed
STATE_AFTER: done
STATE_WHY: All four must-pass criteria met, with one reported conflict between the plan's absolute wording and the text task 1 required verbatim.

## UNIT 253 - STEP 1

STEP: 1
APPROACH: Read what already exists for keying before building anything, then write the fourteen cases and a fake CI-V transport first, watch them fail against a deliberately incomplete abort, and only then put the second half in.
HIT: The same-thread, no-wait, never-throws shape already existed and was already right - Ic7300Rig.AbortCw goes straight at the port outside the command gate. Both missing halves were elsewhere. CI-V 1C 00 existed in the engine only as a read, so there was no PTT-off anywhere; and AbortCw is one write in one try that returns void, so a failed abort and a successful one leave the same trace, which is nothing.
MOVE: Build TransmitAbort as a new static type writing 17 FF and then 1C 00 00, each in its own try, neither reading the other's outcome, returning a record of both. Do not touch AbortCw, which is on the parked CW-send path.
WHY: 17 FF stops a message the radio is keying itself and means nothing to a transmission keyed by PTT with audio behind it, which is what this phase is about - so the fallback is not a retry, it is the other half of the answer, and it must go out whether or not the first landed.
DECIDED: To name only the receive value in CivConstants and deliberately not the value that keys the radio, because this unit built the abort before anything that transmits and a constant is the easiest thing in a codebase to reach for by accident.
LICENCE: Section 0.2, which requires the abort on every keying path, and step 1's own exit criterion that no transmitting code exists when the step closes.
COST: unknown
ACCOMPLISHED: A same-thread abort with both halves, watched failing first at 9 red of 14 and then green at 14 of 14 in 22 ms, fired from all four transmission states and against a dead transport, a gone port, a silent radio and a throwing write, inside a stated 50 ms bound. The no-wait assertion was itself watched to fire. Nothing in the tree calls it.
FATE: executed
STATE_AFTER: done
STATE_WHY: All five must-pass criteria met, including the last one - grep for TransmitAbort in src returns its own declaration and nothing else.

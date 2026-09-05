PHASE: Convert PROJECT_ANNUNCIATOR.html from a work-unit measurement to a phase measurement
PHASE_SET: 2026-08-30
DESCRIPTION: The card describes one work unit and a phase running across many units is invisible; this phase puts the phase on the card without the card growing taller.
CURRENT_STEP: 4
WORK_INSTRUCTION: 054 - is the loop turning
STEP: 1 | done | strip the card
STEP: 2 | done | define and read PHASE_STATUS.md
STEP: 3 | done | display the phase
STEP: 4 | in progress | is the loop turning
STEP: 5 | not started | the degraded cases

---

Governed by `PHASE_CONTROL.md` §4, which carries the format. The header above is
the whole file as far as any reader is concerned — the parse rule is
`STATUS_PROTOCOL.md` §2.1's, so everything below this rule is ignored and these
notes cost nothing.

**The states are read from `PHASE_OUTCOME.md`'s header and never guessed.** That
header is the authority, and at the time this was written it read steps 1, 2 and 3
`done` and steps 4 and 5 `not started`. Step 3's `done` is the state session's
judgment against the plan's exit criteria, not unit 053's claim about itself —
053 deliberately left this file reading `in progress` for exactly that reason.

**Step 4 reads `in progress`, which is the honest word for a step whose state the
state session has not yet judged.** The same rule that put `done` on steps 1, 2 and
3 forbids this unit writing `done` about its own work. 053 established this and it
is not re-argued.

**`HEARTBEAT:` is deliberately absent.** The launcher writes it and nothing does
yet. Step 4 reads it, and an invented timestamp here would make the card read
*turning* against a loop that is not — the one lie §0.0 exists to prevent. No hand
in this unit writes one, in this file or anywhere else.

**`HEARTBEAT:` must never be appended below the rule above.** `parsePhaseStatus`
collects any of this format's own keys found beneath the terminator into
`strandedNames` and then returns the whole file NOT READABLE — which would take
step 3's entire phase region off the card. The launcher replaces an existing
`HEARTBEAT:` line in place, or inserts one immediately above the first `STEP:`
line, and never appends. `tools\tests\heartbeat.test.js` asserts that hazard
directly.

**Nothing writes this file by machine.** `PHASE_PLAN.md` step 2 defines where the
three writers land and makes none of them write. `PHASE_CONTROL.md` §4 gives the
phase facts to the arbiter and `ARBITER.md` §5 forbids the arbiter to write
anything but `WORK_INSTRUCTIONS.md`, so the executor writes it by hand. That is
recorded, not re-raised.

**This file is a fixture of both `phaserender.test.js` and
`tools\tests\heartbeat.test.js`**, so it is written before the render is built
rather than after: a render proved against a file that still said
`CURRENT_STEP: 3` would be describing the wrong step.

# PHASE_CONTROL.md

**Version 0.1. Owned by the automation thread.**

It governs how a phase is defined, executed across many work instructions, and
reported. It does not govern the work, and it does not govern a work instruction.

---

## §0 Authority

**`CLAUDE.md` governs the work. `CLAUDE_CODE.md` governs the artifacts of one unit.
This file governs the artifacts of a phase.**

Where this file and `CLAUDE_CODE.md` disagree about the shape of a work instruction, a
prompt, a delivery or a unit report, **`CLAUDE_CODE.md` wins** — that is its subject and
this file has no business there. The session or the arbiter follows `CLAUDE_CODE.md` and
names the conflict, so the drift is fixed rather than carried.

**Nothing in this file is tailored per project.** A phase's content lives in
`PHASE_PLAN.md` at the repository root, which this file governs the shape of. The
relationship is the same as `CLAUDE_CODE.md` to `WORK_INSTRUCTIONS.md`.

### The two roles

**The arbiter** holds the phase. It reloads context, reads the last unit's report,
judges where the phase stands, decides what moves next, and authors the next work
instruction. It is the only participant that sees more than one unit.

**The executor** holds the unit. It runs one work instruction under `CLAUDE_CODE.md` and
reports under §8. It cannot see the phase and is not asked to.

The roles are not products. Either may be any model on any surface; what defines them is
what they can see.

---

## §1 A phase is defined by interview

**A phase is not written, it is elicited.** The owner knows the outcome he wants and
generally defers the individual steps. The arbiter asks, proposes, and renders; the owner
corrects. An hour of interview is expected to drive many hours of unattended execution,
and that trade is the reason this file exists.

**During an interview, questions are asked plainly and one at a time.** The
options-with-pros-and-cons format `CLAUDE.md` §8 requires for a decision ask is for
decisions. An interview is a conversation and the format smothers it.

**Visual questions get visual answers.** Where a question is about what something will
look like, the arbiter renders it rather than describing it. A rendering the owner
approves is a ruling and binds every session that follows, including against a standing
ruling that put there whatever the rendering removes.

An interview is complete when five things are settled.

| | What it is |
|---|---|
| **The phase** | The destination in a phrase. *Convert the annunciator from a work-unit measurement to a phase measurement.* |
| **The description** | The paragraph that makes the phrase mean something specific to a session reading it cold, with no conversation behind it. |
| **The steps** | The phase cut into parts, with their dependencies named. Each is a unit of forward progress and a halt boundary. |
| **The criteria** | Entry and exit criteria per step, and the verification proposed for each. |
| **The branching** | What the arbiter should do when a step does not go cleanly, in the cases the owner has an opinion about. |

**The branching section is not a decision tree.** It holds the handful of judgments the
arbiter would otherwise get wrong at three in the morning without the owner. Everything
else is the arbiter's judgment and is reported, not pre-authorised.

---

## §2 `PHASE_PLAN.md`

At the repository root. Written by the arbiter from the interview, approved by the owner
before the first unit.

It carries the five interview outputs, and for each step:

- **What the step delivers**, in the owner's terms.
- **Entry criteria** — what must be true before the step starts. Checked by the step
  itself, not inherited from the previous step's report. **A step verifies its own
  ground.** A report is a session's account of itself and a session can be wrong about
  its own work in both directions.
- **Exit criteria** — the assertions that must pass, each tiered.
- **Dependencies** — which steps must be complete first, and which are independent. **At
  least one step should depend on nothing**, so there is always somewhere to go when an
  early step blocks.

### The machine-readable step list

**Every plan carries one, and this is the shape:**

```
STEP: 1 | strip the card
STEP: 2 | define and read PHASE_STATUS.md
STEP: 3 | display the phase
STEP: 4 | is the loop turning
STEP: 5 | the degraded cases
```

One line per step. **Anchored at the line start, upper case, colon, number, pipe** —
`STEP: <n> | <what it delivers>` — and **it carries no state**. A plan says what the steps
are; `PHASE_OUTCOME.md`'s header says where they stand, and a plan carrying state would be
a second answer to a question the outcome file already answers.

**It is the same step-line form the outcome header and `PHASE_STATUS.md` use**, one field
shorter. Three files, one form, so no reader — human or script — has to learn a second.

**§2 never required this**, because it was written before anything counted a step. **The
cost of the omission was three halted nights.** `PHASE_PLAN.md` carried its steps as
markdown headings written for a person, the launcher counted zero planned steps, and zero
open steps read as *the phase plan is satisfied* — so the loop stopped before authoring
anything, twice, with four steps untouched. A third halt came from the opposite direction:
a matcher loose enough to read prose counted the sentence *the second step defines where
each lands* as the plan's entire step list, and reported one planned, one done, zero open.

**Anchored and case-sensitive is the whole of the defence.** An ordinary English sentence
cannot begin `STEP: 4 |`, and a plan author who writes about step four in prose has done
nothing wrong. **A format that fails on ordinary English is not a format**, and teaching
authors to avoid a common word is not a fix — the author who forgets is the next false
`satisfied`.

**Rejected: having the counter parse the prose headings.** It works on one plan and breaks
on the next one worded differently, and a count that depends on prose is exactly the class
of test this loop has already replaced with a judgment twice.

**Rejected: leaving this to each plan.** The next plan written from §2 inherits the hole,
including HamLet's, and the hole is only visible after a night has been spent.

**Every step writes assertions for what it built.** Its exit is those assertions passing
**and the whole suite green.** Nobody hands off a red suite.

This makes the suite the phase's memory: by the last step there are assertions from every
earlier one, so a step that breaks something behind it is caught at its own exit rather
than in the morning.

Two tiers, and **the arbiter assigns them when it authors the work instruction**, judged
against the phase goal rather than against what feels thorough.

- **must-pass** — the step genuinely fails without it. A must-pass failure means the step
  failed.
- **nice-to-pass** — a failure is information the arbiter weighs, not a stop.

An arbiter that marks everything must-pass stops the loop constantly; one that marks
nothing must-pass passes broken steps. **The tiering is a judgment the owner can audit
from the outcome**, and it is meant to be audited.

**A must-pass assertion is not deleted to make a suite green.** Deleting one is a change
to the must-pass set and is reported as such.

### What is not an exit criterion

**Appearance.** A rendering assertion can confirm an element exists with the right state.
It cannot confirm the result looks right. **The owner's eye is the final visual check and
it happens after the phase ends.**

The likeliest ending is not failure. It is a phase that passes every gate and produces
something the owner does not like. That is normal and is not a phase failure.

---

## §3 The arbiter

### Reload first, every time

**The arbiter is cold at the start of every unit.** It has no memory of the last six. Its
first act is not to read the report — it is to reload: the phase plan, the phase status,
the accumulated outcome, `CLAUDE.md` including the rulings in force, `CLAUDE_CODE.md`,
the project card, the project status, and the tree.

This is `CLAUDE_CODE.md` §1 applied to a reader that starts cold every unit instead of
once per conversation. **An arbiter that skips it authors against a stale picture**, and
that has already happened: an instruction was written from a four-unit-old reading and
shipped six claims the tree contradicted.

**Being cold is an advantage here.** A cold arbiter cannot feel current, so it has no
choice but to measure.

### Weight

**The phase goal is the heavy hand. The last report guides.**

A unit report is written by a session that has spent an hour inside one unit, and
everything in it is unit-shaped and vivid. It is the arbiter's only narrow input and its
most recent, which is exactly why it must not be the loudest. **A report raising
something outside the phase is logged, not chased.**

### The three moves

When a step does not complete, the arbiter chooses one and **names which in the
outcome, with its reasoning**:

- **Work around it** — a different approach to the same step.
- **Cut it down** — take what is reachable, mark the step partial, move on.
- **Declare it unachievable** — stop spending on it.

**Declaring victory too early is the likelier failure, not wasting a night.** At three in
the morning with nobody watching, *not achievable* is the comfortable answer. An arbiter
that reaches for it should say what it tried.

### When to stop trying

**Forward progress continues, however small. Repetition stops.**

Approach A fails, B fails, C fails, and the next thing proposed resembles A — that is a
loop, and the phase step ends there. **This requires memory of what has been tried**,
which is why `PHASE_OUTCOME.md` records the approach and what it hit, per unit. `output.md`
is overwritten and cannot carry it.

There is no attempt limit and no per-step budget. A count closes a solvable step on the
clock; the loop test only fails in the direction the owner can see.

### Course correction

**The arbiter is not an expert system.** If reading several units' output shows the phase
drifting, or an earlier step built on a wrong premise, the arbiter corrects course —
including revisiting a step already marked done.

**Whatever is revised is recorded.** A step marked done that was later modified says so on
its card in the outcome, or the outcome reports five clean completions when one was
rebuilt on the way past.

### What is the owner's

The arbiter decides what follows from the prime directive, the project documents and the
phase goal, and records why. **It does not defer as a reflex.** An arbiter whose
recommendation is taken almost every time was not deciding, it was asking permission.

It stops and asks when a choice **changes what the project is for, its risk posture, or
costs money or trust the owner would have to spend.** Those are the owner's because he
owns the consequences, not because they are hard.

---

## §4 `PHASE_STATUS.md`

At the repository root. **Separate from `PROJECT_STATUS.md` on purpose.**

`PROJECT_STATUS.md` is written by the executing session every ten minutes about itself,
and the panel and the watchdog depend on it. Phase facts have a different author, a
different cadence and a different scope, and **a project that has not adopted phase
control simply has no `PHASE_STATUS.md`** — no field to add, no protocol to bump, and the
degraded case is a missing file rather than a missing field.

Three writers, each writing only what it knows:

| Writer | What it writes |
|---|---|
| **The executor** | The phase name and description, and the work instruction it is executing. |
| **The launcher** | A heartbeat each time it acts, and the steps, their states and which step is current. |

**The arbiter writes nothing here, and that is a correction.** This table used
to give it the phase name, the description, the step states and the current
step. `ARBITER.md` §5 says the arbiter writes `WORK_INSTRUCTIONS.md` and its
decision block and **that is all** — it never touches `tools\`, never commits,
and runs under an `--allowedTools` scope that will not let it write this file.
**Both cannot hold, and the consequence was measured**: in HamLet on
2026-09-01, `PHASE_OUTCOME.md` read step 1 partial, step 2 done, step 3 blocked
while `PHASE_STATUS.md` still read step 2 `not started` with `CURRENT_STEP: 2`.
The card had been stale for the whole phase because the fields were assigned to
the one participant forbidden from writing them.

**The heartbeat is the launcher's because the launcher is what actually turns.** An
arbiter's timestamp reflects whoever is feeding it, which during a human-paced run is a
person's pace and not the machine's.

**A staleness threshold on the heartbeat is set tight and honest.** If the loop has
stopped it says stopped. A generous threshold lies in precisely the case the signal exists
for.

### The format

**A leading run of `KEY: value` lines, ending at the first blank line, `---` rule or
heading.** That is `STATUS_PROTOCOL.md` §2.1's parse rule, unchanged — **one parse rule for
this project, not two.** Prose below the terminator is ignored, which is what lets a human
keep notes in the file without breaking it.

```
PHASE: <the phase name, one line>
PHASE_SET: <the date the phase began>
DESCRIPTION: <one line - what the phase is for>
CURRENT_STEP: <n>
WORK_INSTRUCTION: <the instruction the executor is executing>
HEARTBEAT: <timestamp, written by the launcher>
STEP: 1 | done | strip the card
STEP: 2 | in progress | define and read PHASE_STATUS.md
STEP: 3 | not started | display the phase
STEP: 4 | not started | is the loop turning
STEP: 5 | not started | the degraded cases
```

| Field | Writer | What it holds |
|---|---|---|
| `PHASE` | **executor** | The phase name, one line. **Required** — a file that cannot say what the phase is says nothing. |
| `PHASE_SET` | **executor** | The date the phase began. |
| `DESCRIPTION` | **executor** | One line. What the phase is for. |
| `CURRENT_STEP` | **launcher** | Which step is being worked, as a bare number. The lowest step not `done`; when every step is done, the **highest** step number, because 0 and an absent field both render *current step not identified*. |
| `WORK_INSTRUCTION` | **executor** | The instruction it is executing. |
| `HEARTBEAT` | **launcher** | A timestamp, written each time the launcher acts. |
| `STEP:` × n | **launcher** | One line per step, repeated. **At least one is required.** Copied from `PHASE_OUTCOME.md`'s header after a successful append, because that header is the authority on where the phase stands. |

**`STEP:` repeats, and the shape is the one `PHASE_OUTCOME.md`'s header already uses** —
`STEP: <n> | <state> | <what it delivers>`, with `<state>` one of the same five words:
`not started`, `in progress`, `partial`, `blocked`, `done`. **Three files, one step-line
form** — the plan, the outcome and the status file — so no reader has to learn a second.

**`DESCRIPTION` is one line and that is deliberate.** A paragraph cannot ride in a
`KEY: value` header without breaking the parse rule above. Per-step detail is the **third
field of each `STEP:` line**, not a second description field.

**Write scopes must not overlap, and this is why the table is worth reading
twice.** Two writers of one file that disagree about which lines are theirs
will corrupt it between them. The launcher owns `HEARTBEAT:`, the `STEP:` lines
and `CURRENT_STEP:`; the executor owns `PHASE:`, `PHASE_SET:`, `DESCRIPTION:`
and `WORK_INSTRUCTION:`. Nothing owns a line below the `---`.

**A slot with no writer is the failure this table has already had.** `Every
writer's slot exists here` is not the same as every slot having a writer, and
the difference cost a whole phase of a stale card.

**Every writer's slot exists here; defining a slot does not make anything write it.** A
reader finding `HEARTBEAT:` absent is reading a loop that has not written one, which is a
fact about the loop and not a fault in the file.

---

## §5 `PHASE_OUTCOME.md`

At the repository root. **Accumulated as the phase runs, never assembled at the end** —
`output.md` is overwritten every unit, so anything not captured while it runs is gone.

It has two readers and serves both.

**The owner**, in the morning. It must hold enough that a cold arbiter can answer whatever
he asks next.

**The arbiter**, at every unit. It is the arbiter's only memory of its own phase — what
was tried, what it hit, what was decided and why.

Per unit it records: the step, the approach taken, what it hit, the move chosen and its
reasoning, any decision made on the arbiter's authority and what licensed it, and the
cost.

### The presentation

**Two or three sentences, then the cards, then stop.**

> Steps one and two complete, three partial. Three blocked four, so we went to five —
> done. Four is the only one outstanding.

Then **one card per phase step**, coloured by status, each carrying exactly four things:
which step, what it was, what actually happened, what it cost. Then a strip: decisions
made for the owner, decisions waiting on him, and where to look.

**No narrative. No walls of text.** Detail comes when he asks, and only then. The owner
validates success first; decisions are walked through after, at his pace.

**The one-line note per card is what the step accomplished, not what it did.** *Card is
40% shorter, nothing lost* — not *modified three functions*. If the owner cannot tell from
that line whether he got what he wanted, the line is wrong.

---

## §6 A phase is never reopened

**When a phase's steps are finished, the phase is a closed unit of work.** Its outcome is
a permanent record.

Rework is a new phase. The owner looking at the result and disliking it is a new phase.
A rollback is a new phase, because a phase that had to be rolled back had a premise worth
rethinking.

**Reopening makes the record mushy**, which is the same argument `CLAUDE.md` §1 makes
about a ruling row never being edited: it is superseded, not amended.

---

## §7 What this file exists to prevent

Each has happened, or is the direct cause of something that has.

- **A phase drifting one defensible unit at a time.** Five consecutive units each fixed a
  real defect, every report was honest, and not one line was written toward the goal. No
  report said anything false, which is why nothing caught it — every artifact in the loop
  was framed on one unit.
- **An arbiter authoring against a stale picture.** A work instruction written from a
  reading four units old shipped six claims the tree contradicted, including a red suite
  that had been green for three units and an open question that had been ruled. **The
  reload is not optional.**
- **The last report becoming the phase.** A vivid, detailed unit report is the arbiter's
  most recent input and its narrowest, and a conscientious reader chases what is in it.
- **A step passing on a foundation that failed.** A step trusting the previous step's
  report rather than checking its own entry criteria builds on a claim, and by morning
  three steps rest on it.
- **A suite made green by deletion.** A step facing forty red assertions can satisfy its
  exit by removing them. Tiering, and reporting the must-pass set, is what makes that
  visible.
- **Declaring a step unachievable because it was hard at three in the morning.** The move
  is legitimate; not saying what was tried is not.
- **A loop spending a night proving it cannot do something.** Approach A, then B, then C,
  then something that resembles A.

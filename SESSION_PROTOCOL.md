# Session protocol — how work units are scoped and how sessions report

**Status: in force. Ratified as HM-DEC-096, summarized in `CLAUDE.md` §12.**

Companion to `CLAUDE.md`. Governs how a work unit is written and how a session
reports at the end of one. Adapted from the method measured on the MURC
Simulator project, where the same model on the same repository went from a
5m17s session producing three fixes to consecutive 47m and 51m sessions
producing six phases, 41 tests, and a live security defect found and fixed.
Nothing about the model changed. The prompt shape and the placement of
conclusions changed.

Governed by `CLAUDE.md` §0 and §0.4. Nothing here relaxes §0.0, §0.1, §0.2, or
the attribution rule. Where this file and `CLAUDE.md` disagree, `CLAUDE.md`
wins and the disagreement is an open issue, not a judgement call.

---

## 12.1 The two failures this addresses

**A thin plan buys a thin session.** A session's authority to act is exactly as
wide as the plan it was given. A prompt naming three defects produces a session
that fixes three defects and stops, correctly. That is compliance, not
underperformance. When a session keeps stopping to ask about things the plan
plainly covers, **the plan was too thin, and that is Tim's to fix rather than
the session's.**

**A session with nowhere to put a conclusion hands back everything at one
priority.** The conclusions needing judgement and the ones that could not have
gone another way arrive in the same undifferentiated queue, and the ones that
need Tim get buried.

## 12.1 What a session may record itself — narrows §0.4

`CLAUDE.md` §9.5 states that a decision not in `DECISIONS.md` is not made, and
§0.4 reserves rulings to Tim. This narrows that reservation without removing
it.

A session may write an entry to `DECISIONS.md` when, and only when, **all four**
hold:

1. A governing principle in `CLAUDE.md` decides it, and **the reasoning runs one
   way** — once the constraint is stated, no second answer survives.
2. It supersedes no existing ruling and acts against none.
3. **It is not a trade-off.** If it weighs two costs against each other, it is
   Tim's.
4. **The report reproduces the entry in full**, so Tim can override it.

The test is not "is this obvious." Obvious is a feeling, and on the parent
project it was wrong eleven times. The test is whether an alternative can be
**stated** that survives the governing principle.

**The practical tell:** an entry containing "on balance", "the cleaner option",
or "we felt" has already failed the test, because each is the sound of two costs
being weighed.

**The attribution rule stays absolute and is not part of this relaxation.** No
entry claims Tim's authority for a ruling he did not make. That half was never
about volume. An entry written under this section says so on its face and
carries the id of the principle that decided it.

**Anything touching §0.0, §0.0.1, §0.1, §0.2, transmit, or what the display
asserts is Tim's, without exception** — those are precisely the places where a one-way
argument is most likely to be a blind spot rather than a proof.

## 12.2 How a session reports — MANDATORY

Every session report ends with these headings, in this order, with no prose
between them:

### `RECORDED`
Entries written under §12.1. Each with its id and its **full text**, not a
summary. Empty is a real answer and needs no apology.

### `NEEDS A RULING`
Proposals. No id assigned. Each in `DECISIONS.md`'s own format: the ruling
first, then the reasoning, then what was rejected and why. Ordered with the
one that blocks the most work first.

### `STATE`
Build status, test counts (passing and failing, with the failing ones named),
what was pushed and to which branch, and what remains unproven and why.
**A session on the development computer states that nothing in its report is
evidence about the radio** (`SHACK_FACTS.md`, HM-DEC-093).

An unlabelled mixture of the first two puts the triage back on Tim, which is
the cost this exists to remove.

## 12.3 How a work unit is written — Tim's side

- **Five or six phases**, each independently committable, ordered so each is
  buildable when reached.
- **Name the phase to drop** if the session runs out of room, and require it to
  **say** it dropped one rather than half-building it.
- **End with what not to do next**: "if you finish every phase, stop and
  report; do not start the next work unit." Without that clause a session
  finishing early wanders into unscoped work.
- A phase that cannot be built without a ruling says so in the plan, so the
  session raises it first rather than discovering it at the phase boundary.

## 12.4 Files — already governed, restated because it is the expensive one

`CLAUDE.md` §9.0, §9.3, §9.4 and §11 already carry this, and Hamlet's
`tools/get-files` and `tools/repo-listing` already implement it. Restated
because it is the rule whose breach costs the most:

- **Never edit a file whose current version was not read this session** — even
  a file Claude believes it wrote itself.
- **Pull whole folders**, not the three files that seem sufficient. A partial
  mirror has produced delivered defects repeatedly.
- **If something needed cannot be reached, say so and stop**, rather than
  reasoning around the gap. **A ruling made about code nobody read is a guess
  with a citation.**
- On the Claude Code surface the tree is read directly (§9.0); a Claude Code
  session that asks Tim to run `get-files` is introducing a stale copy where
  none existed.

## 12.5 Marked assumptions

Where a value cannot be confirmed, it goes in a data file under `/data`
carrying `source: guess` or `source: extrapolated` and a named `confirm` owner
— never omitted, never asserted. An unmarked wrong value is indistinguishable
from a right one; **a marked one is a question with an owner.**

This is the mechanism that makes proceeding without ratification affordable
rather than reckless, and it is the same instinct as §0.0 applied to the
repository instead of the screen.

## 12.6 Standing small rules

- **Complete files, never diffs or "change line N."** Files that compile
  without modification (§9.0).
- **Tests where a failure would otherwise be silent.** No coverage gate — a
  coverage number is a target that gets gamed, while a silent failure is a
  defect that ships.
- **Do not repair unrelated things on the way past.** Name them in
  `OPEN_ISSUES.md`, leave them.
- **A fixture built from the same misunderstanding as the code proves
  nothing.** Two faults in this repository survived months of green tests that
  way: the scope parser and its fixtures, and the CW noiseless fixtures
  (HM-OPEN-018). When a test passes and the instrument disagrees, suspect the
  fixture.

---

## One conflict, resolved

The source method says "ask questions one at a time, never multiple choice."
`CLAUDE.md` §0.3 / HM-DEC-010 requires every question to be a decision ask with
options A/B/C and their pros and cons in a table. **HM-DEC-010 wins and is not
amended.** The options table is what makes a question rulable in one word, and
nine architectural questions were ruled in nine words on 2026-08-17 because of
it. The source project's rule was written against unstructured multiple choice
offered in place of a recommendation, which is a different defect — and §0
already forbids that one by requiring Claude to state the industry-standard
answer and the reason rather than offering a convenience trade-off as an equal
branch.

STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.

---

# Work instruction 001 — instrument the decoder, fix one defect, take the baseline

Phase: the CW decoder stops emitting soup. This is the phase's first unit and
it builds the instrument every later unit is measured against. It changes what
the decoder *records*, not what it decodes.

## Why this unit exists

**The unit's number: the corpus has never been measured with a per-character
instrument attached.** The figures that exist — the share of emitted letters
that are `E` runs **10 % to 76 %** across the captures, the share of "words"
that are a single character runs **8 % to 84 %** — come from
`CW_REVIEW_BRIEF.md` §1, counted over transcript text after the fact. The
sidecar's own `snrDb` figures were shown to be fiction (§7 of the same brief:
a capture holding no station reported 54.7 dB), so nothing currently written
per character can be trusted to say whether that character was read from a
signal or minted from noise. `CW_CODE_REVIEW.md` (in this delivery, at the
repository root) diagnosed three faults and prescribed an order; every fix in
that order needs a before-table this unit produces.

**The phase's number (PROPOSAL — the connecting sentence is Tim's to ratify,
per CLAUDE_CODE.md §4.2):** E-share and single-character-word share in single
figures across the corpus, the three adjudicated readings intact, and nothing
invented above 3 dB on the sensitivity sweep. This unit moves that number by
making it measurable: the review's fixes are judged by how the witness-split
table moves, and after this unit that table exists, is reproducible from the
tree, and carries a per-character likelihood column nobody has to take on
faith.

**Build number this unit produces: 1.11.1** (new phase: `y` 10 → 11, `z`
resets; current version measured as 1.10.13 in `Directory.Build.props`).
Note: that file's comment still describes the superseded semantic-versioning
meanings — known at packaging time, parked below, do not fix it on the way
past.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and
report any mismatch. Report mismatches even when the work succeeds anyway.
This instruction was written from a 1.10.13 archive, and one gap is known
already: the archive's `DECISIONS.md` held HM-DEC-001–095 and 134–149 only.
**Rulings 096–133 were not readable when this was written**, and task 1 exists
partly because of that.

## Rulings in force

**HM-DEC-120** — transcribed from `CW_REVIEW_BRIEF.md` §10, because the full
decision-log text sits in the unreadable 096–133 range; **verify the wording
against the tree's `DECISIONS.md` before relying on it and report any
difference**:

> **Nothing is emitted on audio holding no signal.** This is the one property
> that has never been traded. The likelihood ratio against the all-key-up
> hypothesis currently gives: nothing invented from 18 dB down to 3 dB on the
> sensitivity sweep, and both captures holding no station are silent. A change
> that reads better and invents anything above 3 dB is a failed change.

Do not re-argue it. Nothing in this unit should move those figures at all —
this unit adds recording and fixes a guard that was documented as existing;
if any task moves the sweep's invention numbers, that is a finding to report,
not a trade to accept.

**Known shape conflict, pre-named so it is not rediscovered:**
`SESSION_PROTOCOL.md` §12.2 mandates a three-heading report
(`RECORDED` / `NEEDS A RULING` / `STATE`); `CLAUDE_CODE.md` §8 mandates five
sections and §0 says it wins on report shape. **Follow §8's five sections**
and name this conflict in the report so the project file gets fixed. §12.1's
substance survives the mapping: decisions the session recorded itself go in
full in section 1; rulings wanted go in section 4 in the decision log's
format.

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` §13 — `STATE`, `TASK: n of m`, `BALL`, `UPDATED` read from the
clock, and `NOTE` saying what is moving inside the task. The same every ten
minutes while a task runs. Task 4's corpus run is the long one; its ten-minute
notes should count captures processed.

## The tasks

### Task 1 — trace before building. Say what you find rather than confirming this list.

This instruction rests on four claims about the tree, written from the
archive. Answer each from the code and report the answers as findings with
file and line, before writing anything:

1. `CwProbabilisticDecoder.LogLikelihoods` forms its noise scale as
   `Percentile(sorted, 25) * 0.6` (archive: ~line 427). Not to change it —
   task 4's table is taken over the shipped model on purpose — but to confirm
   the baseline is measuring what the review reviewed.
2. `CwProbabilisticStream._refillHops` is assigned only inside `Restart()`,
   and `Restart()` is reachable only behind `CwDecoder.ClearOnAStationChange`,
   which is `const false` — so the `RefillSeconds` guard documented at length
   in the field's own remarks has never run in production. If the tree
   disagrees with any link of that chain, task 3 shrinks or dies; say so.
3. `CwProbabilisticStream.Read()` passes the measured unit as the decoder's
   only speed hypothesis when `measured.IsReady` and in range.
4. **Search the tree's `DECISIONS.md` — the whole file, including 096–133 —
   for any ruling that mandates the measured-unit override in claim 3 or the
   refill guard's current shape.** This instruction could not read that range.
   A ruling found there is transcribed into the report and changes nothing
   this unit does (task 4 measures the grid offline without touching the
   production default precisely so no ruling is acted against).

Then build and run the existing test suite once, and record the counts and
the names of any failures as the unit's green baseline.

### Task 2 — the per-character span log-likelihood

For every character the decoder emits, compute the log-likelihood of that
character's own span against all-key-up over the same span. The cumulative
sums in `DecodeAt` make this two subtractions per boundary; the character's
span is already chosen by the path that `Spell` walks. Carry it as a field on
`CwProbabilisticCharacter`, thread it through to `CwCharacter`, and write it
to the capture sidecar beside the character.

**Sidecar only. No display change of any kind** — what the screen asserts is
Tim's without exception (`CLAUDE.md` §0.0, `SESSION_PROTOCOL.md` §12.1), and
this field's purpose this unit is HM-DEC-007's: a wrong decode with its
evidence attached is a regression test. The reason the field exists at all:
the review demonstrated that characters minted from noise and characters read
from signal are separable by exactly this number, and every later unit's
acceptance rests on that separation being logged.

Tests where a failure would otherwise be silent: on a generated known-text
fixture, every real character's span LLR is large and positive; the field
survives to the sidecar; and a window that emits nothing writes nothing.

### Task 3 — initialize `_refillHops` in the constructor

Exactly as `Restart()` computes it, with the reason inline: the guard's own
documentation says less evidence has to mean silence rather than guesses, and
a field initialized only in an unreachable method is a guard that does not
exist. This is a defect fix against the code's own stated intent, not a
behavior choice.

Test: a fresh `CwProbabilisticStream` fed less than `RefillSeconds` of
envelope emits nothing, and one fed more behaves as before. If the sweep's
numbers move because early short-window reads were contributing emission,
**report the movement — in either direction — as a finding**; do not tune
anything to restore old numbers.

### Task 4 — the baseline table

Offline, over the six real captures and the sensitivity sweep, produce and
commit `ANALYSIS-cw-baseline-<date>.md` holding, per capture:

- E-share and single-character-word share of emitted text, split by whether
  `KeyingEnvelope`'s verdict said KEYING at that character's moment — the
  witness split from `CW_REVIEW_BRIEF.md` §5, now reproducible from the tree;
- the span-LLR distribution of emitted characters (P10 / median / P90), from
  task 2's instrument;
- the decode **twice**: as shipped, and with `atWordsPerMinute` passed null
  through the existing offline `Decode` overloads so the grid searches — the
  production default is untouched; the two columns exist so the next unit
  knows what the forced speed is worth before anyone proposes flipping it;
- the three adjudicated readings verbatim (`N4L` on 134712, `VA3VRR` on
  013347, `AA4MP/4 QNIK` on 003758) and both empty captures' emission, which
  must be none.

The sweep rows carry their invention figures at 18, 11 and 3 dB. Every number
in the table is measured this session by code committed this session — no
figure is copied forward from the review or the brief.

### Task 5 — the streaming gate separation *(the drop candidate)*

Run the streaming windower — not the offline whole-file path — over the
corpus and record the per-read likelihood ratio where a station is sending
and where none is. The 3–6 versus 24–39 separation that set `Gate = 15` was
measured by the offline reference on whole files; the instrument that
actually gates has never been measured, and the next unit re-derives the gate
from whatever this shows. Append the distributions to task 4's analysis file.

**This is the drop candidate.** If the unit runs out of room it is dropped
whole and the report says so; do not half-build the harness.

## Parked — do not touch, do not raise

Built from `CW_CODE_REVIEW.md`'s findings; each is real, each is a later unit
or a ruling Tim has not made:

- **`LogLikelihoods` itself, and the `Gate` constant** — unit 002's whole
  job, and the ruling on the model change is not yet made. Parked so the
  baseline is taken over the model the review reviewed.
- **Per-character gating and `■` emission** — changes what the display
  asserts; Tim's without exception.
- **`ClearOnAStationChange`** — re-enabling reverses a ruling Tim made on
  measurements; it is a decision ask, already drafted, not a task.
- **The `Skip()` splice wall and truncation taint** — same unit as the clear,
  so streaming hygiene lands together.
- **`CwToneSurvey`'s `well_separated` valve** — sits against HM-DEC-095;
  ruling drafted, not made.
- **`CwUnitEstimator.Runs` splitting instead of merging short runs** — real,
  measured-adjacent to the 24-on-18 fault, its own small unit after the
  forced-speed evidence from task 4 is in.
- **`FastestWpm` doc/code contradiction (remarks argue forty, constant is
  32)** and **the stale semver comment in `Directory.Build.props`** — both
  named at packaging; `OPEN_ISSUES.md` entries if absent, nothing more.

A parked item that turns out to block a task is raised once, saying it was
parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not change `LogLikelihoods`, `Gate`, or any decode constant.** A
  baseline taken over a changed model measures nothing; the table's whole
  value is that the review's diagnosis and this unit's numbers describe the
  same code.
- **Do not flip the production speed default.** Task 4 measures the grid
  offline beside it. A ruling in the unreadable 096–133 range may mandate the
  current behavior, and a default flipped against an unread ruling is the
  exact failure `CLAUDE_CODE.md` §11 records.
- **Do not touch any display, style, or ViewModel rendering.** The span LLR
  is sidecar-only this unit; the screen asserts nothing new.
- **Do not loosen HM-DEC-095's separation limit, confirmation rule, or
  plausibility bounds** — nothing in this unit needs the survey at all.

## Committing, pushing, reporting

Commit and push each task before starting the next; the report names the
branch and states whether each push succeeded.

Report per `CLAUDE_CODE.md` §8: five sections to `output.md` at the
repository root, overwritten, and printed to the session. **Section 3 leads
with the baseline table's headline — the per-capture E-share inside and
outside the witness verdict — because that is the number this unit was
commissioned to produce.** What Tim sees in the application this unit:
nothing — say so plainly per §9; this unit only makes every later unit
measurable and its regressions catchable. Section 5: the phase number as
proposed above, measured for the first time by this unit's table, `was:
unmeasurable — no per-character instrument existed`; build 1.11.1 confirmed
from `Directory.Build.props`.

**If you finish every task, stop and report. Do not start the next unit.**

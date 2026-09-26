PHASE: Hamlet meets the CW requirements
PHASE_SET: 2026-09-25
DESCRIPTION: CW_REQUIREMENTS.md and CW_SPEC.md at the repository root are the specification from here on. Sixty-eight requirements, sixty-five of them must-tier, and section T shows fourteen groups with no test at all. This phase traces what exists, builds the metrics the requirements are written in - invented characters, sure-character error, coverage, word-boundary error, acquisition time - and then meets the requirements group by group, highest tier first. Every unit recenters on those two documents. Judged by requirement ids and, at the end, by Tim at the radio.
STEP: 0 | Every CW test is traced - section T names, for each requirement, the test that proves it or the word none, and every existing CW test names the requirement it proves or is marked as proving none.
STEP: 1 | The metrics exist - MET-INVENTED, MET-CER-SURE, MET-COVERAGE and MET-WBE are computed over the corpus and reported per condition, so the requirements can be measured at all.
STEP: 2 | The decoder stops printing wrong letters with confidence - MET-CER-SURE driven toward HM-REQ-010's one in a hundred, from the 54 sure-but-wrong characters unit 439 measured.
STEP: 3 | The junk between words goes - MET-INVENTED driven toward HM-REQ-011's zero, from the 13 added characters unit 439 measured.
STEP: 4 | The pitch is right - section J, and the acquisition requirements of section K that depend on it.
STEP: 5 | The speed is right - section D, five to forty-five words a minute, acquired cold and tracked through a change.
STEP: 6 | The text is right - sections H and I, the character table, the prosigns and the word boundaries.
STEP: 7 | The conditions can be generated - sections E, F and G: channel, sender and interference profiles, which nothing in the tree produces today.
STEP: 8 | The record and the tests are put right - last, not first: the decision log, the traceability table, the 43 tests that measure something other than what their requirement states, and the 63 requirements with no test.

---

# The requirements phase - the reasoning under the step list

**Set 2026-09-25 by the web thread on Tim's ruling.** *"We will now focus moving forward on
finishing CW based on the specifications and requirements. I want the arbiter to recenter
itself during every iteration on these two documents."* And on unit tests: *"We should be
tracing to requirements."*

## §1 What this phase is

The correctness phase, *Hamlet reads a CQ call correctly*, is archived at
`docs/phase-correctness-run/` with **5.1 open - Tim's verdict, which stays his** - and with
3.6, 6.5, 7.1, 7.2, 7.4, 7.6 and 7.8 unmet. **Every one of those reappears here as a
requirement id**, measured the way the requirements measure rather than by character counts.
What it leaves behind, kept: the spacing repair (units 415 and 416, all keyed 217 edits to
167 over 565); the emission gate (placeholders 299 to 28 with no named character lost); the
floors and the scorer; the receiver conditions set from the radio's manual; the screen
sentences made true; the card that says where a station is.

**Why the phase changes shape.** Six units on 2026-09-25 built changes that read the opening
correctly - at 24 WPM, `EANQNID EAN■IK` where a week of junk had stood - and every one was
rejected because capture rows' character counts fell. **Character counts are not a requirement
anywhere in `CW_REQUIREMENTS.md`.** What is required is MET-INVENTED at zero, MET-CER-SURE
below 1%, coverage at or above 90%, and MET-WBE at or below 5%. A change that removes invented
characters *lowers* a character count and *meets* the requirements. The old keep rule was
measuring the wrong thing, and it cost six units.

## §2 What is the same

**`CW_REQUIREMENTS.md` and `CW_SPEC.md` at the repository root are the specification.** Every
unit of this phase reads them before it reads anything else. Where this plan and those
documents differ, **the documents win**, and the difference is reported as a finding.

Every ruling of the phases before stands. The ones this phase leans on: **R11** nothing at the
radio; **R12** a session rewrites its own tests; **R19** American; **R45** the form of a
criterion and of ADVANCES; **R54 and R65** a ruling is wanted only when a criterion of the step
in hand needs one, and nothing carried ever halts the loop; **R61** a key is inferred unless it
was transcribed; **R66** an adjudicated reading may move onto its own adjudicated text;
**R72** no word, dictionary or callsign prior, in any form - **which is HM-REQ-004, and Tim
ruled it on 2026-09-24 as HM-DEC-175, so section R's first row is answered**; **R74** Hamlet
sets the radio and the operator is not rig control; **R75** the tone tracker is open; **R76**
the pitch instrument is built and proved before the tracker is changed.

`CLAUDE.md` **§0.0** never present a guess as a decode; **§0.2** transmit safety; **§12.5** a
fixture built from the same misunderstanding as the code proves nothing; **HM-DEC-091**,
**HM-DEC-103**, **HM-DEC-155**, **HM-DEC-165**, **HM-DEC-168**, **FACT-004**, **FACT-006**.

## §R Rulings, Tim, 2026-09-25

**R77 - the requirements are the specification, and the tests trace to them.** *"We will now
focus moving forward on finishing CW based on the specifications and requirements."* Every
criterion below names requirement ids. **A CW test names the requirement it proves**; a test
that proves none is either evidence for a requirement nobody wrote down - which is a finding -
or it does not earn its runtime. Rejected: finishing the correctness phase's remaining
criteria first, because they are written against counts the requirements have superseded.

**R78 - the keep rule is the requirements' own metrics, not character counts.** A change is
kept when it moves a requirement's metric the right way and breaks no other requirement.
**MET-INVENTED at zero, MET-CER-SURE below 1%, MET-COVERAGE at or above 90%, MET-WBE at or
below 5%**, per condition, at the tier the requirement names. **The capture floors stay as
V-11's overfitting guard** - no change may redden an earlier capture to green a newer one -
**and stop being the keep rule.** A capture row's character count falling is a finding to
report, not a rejection, when no requirement's metric got worse.

**R79 - the loop runs unattended and recenters every iteration.** *"I want the Arbiter to
recenter itself during every iteration on these two documents."* Consequence: every
instruction's section 4 quotes the requirement ids it is aimed at, from the documents, and
every criterion below is a named requirement a report can measure. Only 8.1 waits on him.

**R80 - Tim, 2026-09-25: the record and the tests come last.** *"Record keeping is for
pussies. We do it at the end."* **No unit of this phase is authored for bookkeeping.** The
decision log's gaps, the traceability table, the 43 tests that measure something other than
what their requirement states, and the 63 requirements with no test are **step 8**, worked
after the decoder work. Consequences, binding on every unit:

- **A unit records a ruling only when the owner has given one in that unit's own instruction.**
  No unit is spent recording a ruling somebody else ordered.
- **A unit writes no traceability, no test inventory and no decision-log repair** unless a
  criterion it is working cannot be judged without it - and then it does the minimum and says
  so.
- **A criterion the manual unit 439 evidenced is ticked by the next loop unit at its task 0
  from that report**, without re-measuring. Steps 0 and 1 are met: 5 of 68 requirements traced,
  63 with none, 43 mismeasuring, and all four metrics built and measured.
- **A test is written only where a requirement this phase is meeting needs one to be judged.**

Rejected: fixing the decision log and the 43 tests first, which put four steps of scaffolding
in front of the two steps that change what the operator reads.

**R81 - Tim, 2026-09-25: the two numbers that change the text come first.** Unit 439 measured
**MET-INVENTED at 67** against HM-REQ-011's zero, and it is not what the tree had been counting:
**13 sure characters added and 54 sure characters wrong**, over 473 sent, against a tree figure
of 17 added letters. **The 54 wrong-when-sure are the larger half and had never been counted.**
They are `W1 TW /8 W H I O` where `W1AW/8 OHIO` was sent - the decoder printing the wrong letter
and believing it. So **step 2 is MET-CER-SURE and step 3 is MET-INVENTED**, ahead of the pitch
and the speed, because both are measurable today with the metrics unit 439 built and both change
what the operator reads. Rejected: opening with the invented characters, which are fewer and
which step 3 takes immediately after.

## §3 What is different from the phases before it

1. **A criterion is a requirement id.** A report states the requirement, the condition, the
   metric and the number. *"HM-REQ-011, MET-INVENTED, TX-ITU at 20 WPM on CH-AWGN: 17 to 0"*,
   never *"the litter is gone"*.
2. **A test names its requirement.** New tests are written to the verification table's form -
   method (T, A or I), condition, threshold, unknown-versus-wrong, truth grade.
3. **The documents are read first, every unit.** Not this plan's summary of them.

## §4 The steps

Exit criteria carry ids `N.k`; met is `[x]`; R45 gives the form. A step's exit is its own
assertions, the three floor tests, and `docs/carry-forward-tests.txt` run as its comment says -
never the whole suite.

## Step 0 - Every CW test is traced

**Delivers:** R77, and the map every later step is aimed with.

**Entry:** `PHASE_STATUS.md` names this phase; `CW_REQUIREMENTS.md` and `CW_SPEC.md` are at the
repository root.

**Exit:**
- [x] 0.1 Every CW test type and method in the tree is listed with the requirement id it proves, or the word `none`, in `docs/phase-requirements/traceability.md`, with the file and method named; a test that proves none is listed with one line saying what it does assert.
- [x] 0.2 Section T's table is extended to every requirement id in `CW_REQUIREMENTS.md`, each row naming the test that proves it or `none`, and the report states how many requirements have a test, how many have none, and how many have a test that does not measure what the requirement states.
- [x] 0.3 Every name in `docs/unit239-failing-set.txt`, the known-reds block and `docs/cw-retired-tests.txt` is placed in the same table: the requirement it was evidence for, or none.
- [x] 0.4 Nothing under `src` is changed, no test is repaired, retired or rewritten, and the three floor tests and both carry-forward lines are green at exit.

**Depends on:** nothing.

## Step 1 - The metrics exist

**Delivers:** the numbers the requirements are written in. Fourteen requirement groups have no
test today because these do not exist (section T).

**Entry:** step 0 done.

**Exit:**
- [x] 1.1 MET-INVENTED is computed as `CW_SPEC.md` defines it, over every keyed recording, and reported per condition; the report states today's figure and names the recordings it comes from.
- [x] 1.2 MET-CER-SURE and MET-COVERAGE are computed as the spec defines them, over every keyed recording, per confidence class, and reported per condition.
- [x] 1.3 MET-WBE is computed as the spec defines it, separately from character errors (HM-REQ-082), and the baseline is re-issued with it.
- [x] 1.4 Each metric is watched failing first against a case whose answer is known by construction, and each is reachable by the other CW test types so a later unit can judge a change with it.
- [x] 1.5 The three floor tests and both carry-forward lines are green at exit, and nothing under `src` is changed.

**Depends on:** step 0.

## Step 2 - The decoder stops printing wrong letters with confidence

**Delivers:** R81 and HM-REQ-010. The 54 sure-but-wrong characters unit 439 measured.

**Entry:** steps 0 and 1 met, ticked at task 0 from unit 439's report.

**Exit:**
- [x] 2.1 The 54 sure-but-wrong characters are traced: a fact that asserts nothing prints each one with its recording, what the key says was sent, what the decoder emitted, its span, the speed and pitch in force, and the marks it rested on; the report groups them by what they have in common.
- [x] 2.2 Each change is built in its own commit and kept under R78: MET-CER-SURE falls, MET-INVENTED does not rise, MET-COVERAGE does not fall, the three adjudicated readings are unchanged or move onto their own adjudicated text, and V-11 holds - no capture is reddened to green a newer one.
- [x] 2.3 MET-CER-SURE is reported before and after every kept change, per condition, with the key kind beside each number, and the running figure is in `docs/phase-requirements/metrics.md`.
- [ ] 2.4 After three consecutive units with no kept change the trace goes to `PARKED.md` and the step closes partial.
- [ ] 2.5 The three floor tests and both carry-forward lines are green at the exit of every commit of the step.

**Depends on:** steps 0 and 1.

## Step 3 - The junk between words goes

**Delivers:** R81 and HM-REQ-011. The 13 added characters, and the litter the operator reads
between real words.

**Entry:** steps 0 and 1 met.

**Exit:**
- [x] 3.1 The 13 sure added characters are traced the same way as 2.1, each with its recording, span, speed, pitch and the marks it rested on.
- [ ] 3.2 Each change is built in its own commit and kept under R78: MET-INVENTED falls, MET-CER-SURE does not rise, MET-COVERAGE does not fall, the adjudicated readings hold, and V-11 holds.
- [ ] 3.3 MET-INVENTED is reported before and after every kept change, per condition, with the key kind beside each number, and the running figure is in `metrics.md`.
- [ ] 3.4 The 7.052 traffic net's `EETTTEETTTTTTTTETTETETKTETEE` between `GRAY KC` and `LIVER VIA` is printed before and after, so the owner reads the difference rather than the number.
- [ ] 3.5 After three consecutive units with no kept change the trace goes to `PARKED.md` and the step closes partial.
- [ ] 3.6 The three floor tests and both carry-forward lines are green at the exit of every commit of the step.

**Depends on:** steps 0 and 1. Independent of step 2: when one blocks the arbiter works the other.

## Step 4 - The pitch is right

**Delivers:** section J, HM-REQ-090 to 094, and section K's 102 and 103 which depend on it.

**Entry:** steps 0 and 1 met.

**Exit:**
- [ ] 4.1 A pitch instrument finer than 25 Hz is built and shown, on tones known by construction, to land within one of its own bins of the truth, sharing no line of code with `CwToneTracker` or `CwToneSurvey` (R76); its error per case and its cost per hop are tabled.
- [ ] 4.2 MET-PITCH-ERR is measured with it on every capture, and the report names every case where the decoder demodulated more than 25 Hz from the tone the instrument found - `cw-2026-09-24-135641` and `-152135` at 75 Hz are the known ones.
- [ ] 4.3 HM-REQ-091, the tracked pitch chosen by keying quality and never by level alone or by the operator's configured pitch, has a test naming it and the report states whether it is met.
- [ ] 4.4 A change to `CwToneTracker` under R75 is judged by R78's keep rule with the instrument's table beside it, and the report names the clause of HM-DEC-095 or HM-DEC-127 it works against; after three consecutive units with no kept change the step closes partial.
- [ ] 4.5 HM-REQ-102 and HM-REQ-103 are measured on the 7.052 opening: no sure character while acquiring, and the opening characters not lost to acquisition, reported as text before and after.
- [ ] 4.6 HM-REQ-093, pitch reported with a proof state of proved, hypothesis or none, is met, with a test naming it.
- [ ] 4.7 The three floor tests and both carry-forward lines are green at exit.

**Depends on:** steps 0 and 1.

## Step 5 - The speed is right

**Delivers:** section D, HM-REQ-030 to 036, and section K's 100 and 101.

**Entry:** steps 0 and 1 met.

**Exit:**
- [ ] 5.1 HM-REQ-030 is met at both ends: the speed search reaches 5 and 45 words a minute, where it runs 8 to 40 at HEAD, judged by R78's keep rule with the decode time reported before and after.
- [ ] 5.2 `cw-2026-09-24-135641`, which read nothing in 44 minutes at about 44 words a minute, emits sure characters, and the report prints what it reads beside `nothing read`.
- [ ] 5.3 HM-REQ-031 and HM-REQ-033 are measured - speed within 10% of true after acquisition, and acquisition without a prior speed - and reported per condition.
- [ ] 5.4 MET-TACQ and MET-LAT exist and are measured, and HM-REQ-100 and HM-REQ-101 are reported against them.
- [ ] 5.5 HM-REQ-034, the speed reported with a proof state, and HM-REQ-035 and 036, state retained across a clear and across a pitch refinement, each have a test naming them and the report states whether each is met.
- [ ] 5.6 The three floor tests and both carry-forward lines are green at exit.

**Depends on:** steps 0 and 1.

## Step 6 - The text is right

**Delivers:** sections H and I, HM-REQ-070 to 073 and 080 to 084.

**Entry:** steps 0 and 1 met.

**Exit:**
- [ ] 6.1 The v1 character table of `CW_SPEC.md` is generated from the vendored source rather than written as constants, and HM-REQ-070 has a test naming it.
- [ ] 6.2 HM-REQ-071 and 072, prosigns emitted as one symbol and named per the terminal's setting, each have a test naming them and the report states whether each is met.
- [ ] 6.3 MET-WBE is reported per condition and HM-REQ-080 and 081 are measured against it.
- [ ] 6.4 HM-REQ-083, one set of word boundaries for any span between the live and settled renderings, has a test naming it.
- [ ] 6.5 HM-REQ-084 is measured on its named spans - `WEEKEND`, `THINKING`, `FLEX`, `ABOVE`, `BREEZE`, `USED TO USE A FIRM` - and the report prints what each reads at HEAD.
- [ ] 6.6 The three floor tests and both carry-forward lines are green at exit.

**Depends on:** steps 0 and 1.

## Step 7 - The conditions can be generated

**Delivers:** sections E, F and G. Nothing in the tree produces these, and without them no
requirement can be judged at its own condition (unit 439's finding 1).

**Entry:** steps 0 and 1 met.

**Exit:**
- [ ] 7.1 The generator produces the CH-* channel profiles `CW_SPEC.md` names, each carrying a shaped noise band and never digital silence (V-06), with the recipe for every case stated so another unit can rebuild them.
- [ ] 7.2 The generator produces the TX-* sender profiles, and HM-REQ-050 is measured on each must-tier profile at 15 dB reference.
- [ ] 7.3 The generator produces the INT-* interference profiles, and HM-REQ-060, 061, 062 and 066 are measured against them.
- [ ] 7.4 Every generated fixture carries its exact key by construction, and the report states what each does not prove (§12.5, V-04); a real capture is reported by the sender profile the spec names or as `not stated`, and is never counted toward a CH-* condition.
- [ ] 7.5 The three floor tests and both carry-forward lines are green at exit.

**Depends on:** steps 0 and 1. Independent of steps 2 to 6.

## Step 8 - The record and the tests are put right

**Delivers:** R80's deferred work, last. **No unit is authored against this step while any
criterion of steps 2 to 7 is open and authorable.**

**Entry:** steps 2 to 7 done or closed partial.

**Exit:**
- [ ] 8.1 HM-DEC-182 is recorded from work instruction 434's block, HM-DEC-166 gains its `CLAUDE.md` row, and `DecisionLogOrderTests` is green with no gap that is a missing record.
- [ ] 8.2 The false `UNIT 2 - STEP 1` entry of 2026-09-14 is corrected by appending a dated note naming it as recorded in error; the row itself is never edited or deleted.
- [ ] 8.3 The version scheme is settled one way: either HM-DEC-150 is superseded to say the patch counts units across phases, or the next phase opens at a new minor; the report states which and why.
- [ ] 8.4 Each of the 43 tests that measure something other than what their requirement states is either re-pointed at the requirement it proves or marked as proving none with its reason, in `traceability.md`; none is deleted.
- [ ] 8.5 Every requirement this phase met has a test naming it, and the count of requirements with no test is reported against unit 439's 63.
- [ ] 8.6 The three floor tests and both carry-forward lines are green at exit.

**Depends on:** steps 2 to 7.

## §5 Dependencies

Steps 0 and 1 are met by the hand-run unit 439 and are ticked at the next unit's task 0 from
its report. **Steps 2, 3, 4, 5, 6 and 7 each depend only on those two**, so there are six
independent places to route and the loop need never stall for want of work. **Step 8 is last
and no unit is authored against it while any criterion of steps 2 to 7 is open** (R80).

## §6 Branching

- **Three stops only**: keying, transmit or the radio's safety; money past the budget; a fact
  the product states to the operator about a signal, a station or a send. A threshold, a
  window, a test's shape, a filter, a timeout: decide, mark author's, continue.
- **The later ruling wins. A done step is closed. Every remaining step Tim's: halt.**
- **A stop 3 is legitimate only when a criterion of the step being worked cannot be met without
  the ruling** (R65). A carried ask, a section 4 question, a parked item or a finding noticed
  in passing **never halts the loop**.
- **Where this plan and the two documents differ, the documents win**, and the difference is a
  finding in the report.
- **R78 is the keep rule.** A change is kept on a requirement's metric, not on a character
  count. **A capture row's count falling is reported, not rejected**, when no requirement's
  metric got worse. **V-11 still stands**: no change may redden an earlier capture to green a
  newer one, and that is judged on the requirements' metrics.
- **A requirement with a TBD threshold** (HM-REQ-032, 065, 092) is measured and the measurement
  reported; the threshold is the owner's and its absence never halts a unit.
- **A test that proves no requirement is never deleted in this phase.** It is listed, and its
  fate is a later ruling (HM-DEC-103).
- **No fixture is admitted by lowering a gate** (V-04), and **no separation limit, confirmation
  rule or plausibility bound is loosened to pass a fixture** (V-14).
- **A run lost before any assertion** counts neither way and is re-run once.
- **No unit is authored for the record or for the tests** (R80) while a criterion of steps 2 to
  7 is open and authorable. A unit records a ruling only when its own instruction carries one.
- **Anything would change what keys or transmits.** `MOVE: stop`.
- **A package is needed.** `MOVE: stop`.
- **A CW test costing more than 300 s** never goes on a carry-forward line.

## §7 Carried

The correctness phase's 5.1, Tim's, at `docs/phase-correctness-run/`; its 3.6, 6.5, 7.1, 7.2,
7.4, 7.6 and 7.8, each reappearing here as a requirement id; every item of its `PARKED.md`;
unit 427's six findings, including the PSK31 CQ with a grid reading as no station, and the
three UI items that differ from what the owner asked; **the window reflowing when the radio
announces a frequency change into the data range, and the stale sentence saying the operator's
license covers Morse on an FT8 frequency** - both banked 2026-09-25 and not in this phase's
scope; the seven rulings of section R, of which the first is answered by R72; the attenuator
sentence outside an owned block.

## §8 Revision record

- **2026-09-25, after unit 439's hand run.** R80 the record and the tests come last, as step 8;
  R81 MET-CER-SURE and MET-INVENTED come first, as steps 2 and 3, on unit 439's measurement of
  67 invented - 13 added and 54 sure-but-wrong - where the tree had been counting 17. Steps 2
  to 8 rewritten; steps 0 and 1 met by the hand run.

- **2026-09-25.** Written from the interview: R77 to R79; nine steps; the criteria named
  against requirement ids; the keep rule moved from character counts to the requirements' own
  metrics after six units were rejected on counts that no requirement states.

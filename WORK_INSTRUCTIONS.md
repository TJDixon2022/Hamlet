# Work instruction 439 - trace the tests, then build the metrics

**Run this by hand, not through the loop.** `tools\arbiter\run-phase.bat` halts before it
launches anything: its arbiter re-decides `MOVE: stop` from stale evidence about a phase
transition that has already happened, and there is no mechanism for the owner to clear it.
That is the ClaudeProjectStatus thread's to fix. **This instruction does not wait for it.**

Steps 0 and 1 of the requirements phase, in one session. **Five tasks, drop from the back.**

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST EXIST:      CW_REQUIREMENTS.md
  MUST EXIST:      CW_SPEC.md
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all six are not as stated, you are in the wrong repository or the
specification is missing.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all six hold, say "Hamlet confirmed" and continue.
```

---

## 1. Running by hand: what is different

- **There is no arbiter and no judge.** Nothing will tick a criterion for you. **Do not tick
  a box in `PHASE_PLAN.md`** - report what you measured and let the owner or a later loop unit
  tick.
- **There is no launcher watchdog.** Keep the session's own discipline: one type per
  `dotnet test` invocation, each with its own `timeout`, a status line before each.
- **Write `output.md` at the root when you finish**, in the four canonical headings below, so
  the next loop unit and the owner read it the same way.
- **Do not write to `RUN_LEDGER.md`.** That is the launcher's file. Say in `output.md` that
  this unit ran by hand.
- **`SESSION.lock`**: take it at the start and release it at the end, as a loop session would,
  so nothing else touches the tree while you work.

## 2. The rules that killed sessions

**HM-DEC-155.** No suite. Only this unit's names and `docs\carry-forward-tests.txt`, run as
its top comment says. **Never background and poll.** The captures type is 51 rows and runs
about 120 s; give it 600 s. **A metric run over every keyed recording will be slow; give it
900 s and report what it took.**

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. Multi-step
commands go into `.run-unit\unit439-<name>.sh` and run with `sh`.

**The four report headings are exactly these, character for character:**

```
## 1. What Claude did
## 2. What the owner should expect
## 3. What you should see
## 4. What's blocking us
```

---

## 3. Why this unit exists

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  Trace every CW test to a requirement, then build the four
            metrics the requirements are stated in.
```

**Read `CW_REQUIREMENTS.md` and `CW_SPEC.md` before anything else.** They are the
specification. **Where this instruction and those documents differ, the documents win**, and
the difference goes in the report.

**Tim, 2026-09-25:** *"We will now focus moving forward on finishing CW based on the
specifications and requirements... This should also inform our policy on unit tests. We should
be tracing to requirements."*

**Why these two steps together.** Six units on 2026-09-25 built changes that read the 7.052
opening correctly - 24 WPM, `EANQNID EAN■IK` where a week of `E ET E E` had stood - and every
one was rejected because capture rows' **character counts** fell. Character counts are not a
requirement anywhere in `CW_REQUIREMENTS.md`. **R78 makes the keep rule the requirements' own
metrics, and those metrics do not exist**, so no unit can apply the new rule until they do.
The trace says what is already tested; the metrics make the rule usable. Neither changes the
decoder.

**The four metrics, and what each is for:**

- **MET-INVENTED** - HM-REQ-011 requires **zero**. The tree's figure today is **17 added
  letters** over the keyed recordings, 8 of them single-element: the stray `E`, `T` and `<BT>`
  litter, stated as the requirement states it.
- **MET-CER-SURE** - HM-REQ-010 requires **below 1%**. Nothing measures it today.
- **MET-COVERAGE** - HM-REQ-012 requires **at least 90%** of sent characters emitted sure.
  Without it, dimming everything satisfies HM-REQ-010; that is the cheat it guards.
- **MET-WBE** - HM-REQ-080 requires **zero** at 15 dB on a must-tier profile, HM-REQ-081 **at
  or below 5%** at the floor, and HM-REQ-082 requires it scored **separately from character
  errors**.

**What exists to build on:** `CwScorer` with `Whole`, `Within` and `FromFirst`, its `Kinds`
breakdown and `LettersOnly`; the keyed recordings with inferred keys; the synthetic CQ set with
exact keys; `docs\phase-correctness-run\` for the baseline's history.

---

## 4. Verify this instruction against the tree

Check, report any mismatch, repair nothing:

- **`CW_SPEC.md`'s definition of each of the four metrics**, quoted into the report: what counts
  as invented, what a sure-character error is, what coverage is measured against, how a
  word-boundary error is counted. **These definitions rule; section 3 above is a summary and
  may be wrong.**
- What `CW_SPEC.md` names as a **condition**, since every metric is reported per condition, and
  which conditions the corpus can currently stand for.
- The requirement count: sixty-eight, sixty-five must-tier, and section T's existing rows.
- Every CW test file under `tests\Hamlet.RadioEngine.Tests\Cw`, plus CW tests in the app
  project and anywhere else - **name where you looked**.
- `docs\unit239-failing-set.txt`, the known-reds block of `docs\carry-forward-tests.txt`, and
  `docs\cw-retired-tests.txt`: that each exists and how many names each holds.
- `CwScorer`'s surface, its `Kinds` counts and `LettersOnly`.
- Today's figures: all keyed edits over 565, and 17 added letters of which 8 single-element.

## 5. Rulings in force - do not re-argue

`docs\phase-requirements\PHASE_PLAN.md` R77, R78, R79 and §6. The ones that bind here:

**R77** the requirements are the specification, and a CW test names the requirement it proves.
**R78** a change is kept on a requirement's metric, not a character count; the capture floors
are V-11's overfitting guard only. **This unit changes no decoder code**, so it keeps nothing -
it builds the instruments later units keep changes with.
**R61 / V-13** an inferred key is labelled inferred everywhere, and disagreement with it is not
by itself proof the decoder is wrong - **so every metric reports the key kind beside the
number**.
**V-05** a floor is found by sweeping to where MET-INVENTED crosses zero, never translated from
another measurement - **so do not derive one metric from another**.
**§3.1** a number is always reported with its parts: the metric, the condition, the count, and
whether the key is exact or inferred. **Never a bare percentage.**
**§0.0** never state as known what is not known: **a test whose requirement is unclear is
listed as unclear, and a metric that cannot be computed for a recording says so and reports no
number.**
**HM-DEC-103** a fixture retires by ruling, one at a time - **so nothing retires here**.
**§0.2** nothing that keys or transmits is touched. **§12.6** repair nothing on the way past.
**HM-DEC-155**, **HM-DEC-165**, **FACT-004**, **FACT-006**.

**Record this in `DECISIONS.md`, newest first, and one row at the top of `CLAUDE.md` §1's
table dated 2026-09-25, headline **The CW requirements are the specification, and tests trace
to them**, ref HM-DEC-183:**

```
---
id: HM-DEC-183
date: 2026-09-25
refs: CW_REQUIREMENTS.md, CW_SPEC.md, docs/phase-requirements/PHASE_PLAN.md R77 R78 R79, docs/phase-correctness-run/, PROJECT_CARD.md, work instruction 439, HM-DEC-175
---

**`CW_REQUIREMENTS.md` and `CW_SPEC.md` are the specification for CW, and every CW test names
the requirement it proves.** Tim, 2026-09-25.

**What is archived.** *Hamlet reads a CQ call correctly*, set 2026-09-23, closes with its
spacing repair kept (all keyed 217 edits to 167 over 565), its emission gate kept (placeholders
299 to 28 with no named character lost), the receiver conditions set from the radio's manual,
the screen sentences made true, and 5.1 - Tim's verdict - open and still his. Its 3.6, 6.5,
7.1, 7.2, 7.4, 7.6 and 7.8 are unmet and each reappears in the new phase as a requirement id.

**Why the shape changes.** On 2026-09-25 six units built changes that read the 7.052 opening
correctly and every one was rejected because capture rows' character counts fell. Character
counts are not a requirement anywhere in `CW_REQUIREMENTS.md`. What is required is MET-INVENTED
at zero, MET-CER-SURE below 1 per cent, coverage at or above 90 per cent and MET-WBE at or
below 5 per cent. The keep rule was measuring the wrong thing.

**What is set.** *Hamlet meets the CW requirements*: trace every CW test to a requirement,
build the metrics the requirements are written in, then meet them group by group - honesty,
confidence, pitch, speed, text, and the generated conditions - and Tim at the radio. A change
is kept on a requirement's metric; the capture floors stay as V-11's overfitting guard and stop
being the keep rule. Every unit reads the two documents first, and where they differ from a
plan the documents win.

**Already answered.** Section R's first row, the knowledge rule HM-REQ-004, was ruled on
2026-09-24 as HM-DEC-175: no word, dictionary or callsign prior, in any form.

**Whose words are whose.** The ruling is Tim's; the wording is work instruction 439's record of
it. Rejected: finishing the correctness phase's remaining criteria first, because they are
written against counts the requirements have superseded.
```

---

## 6. The tasks

### Task 0 - the record

`docs\phase-requirements\PHASE_OUTCOME.md` gets a `## UNIT 439 - STEP 0` entry in the shape of
the archived phase's entries, from the decision block at the foot of this file, with a line
saying it ran by hand outside the loop. `PHASE_STATUS.md` names unit 439 and `CURRENT_STEP: 0`.
Patch-bump `Directory.Build.props`. `PROJECT_CARD.md` `PHASE: Hamlet meets the CW requirements`
and `PHASE_SET: 2026-09-25`. `DECISIONS.md` HM-DEC-183 and the `CLAUDE.md` row.

**Entry round:** both carry-forward lines, the three floor tests, the keyed totals and the
added-letter count, recorded as the numbers to beat.

**Drop candidate:** none.

### Task 1 - every test, and what it proves (0.1)

Walk every CW test in the tree - the engine's `Cw` folder, the app project's CW tests, and any
CW test anywhere else - and write `docs\phase-requirements\traceability.md`: one row per test
**method**, with its file, its type, its method, and **the requirement id it proves**, or
`none`.

- A test proves a requirement when what it asserts is what the requirement states. A test that
  asserts a mechanism the requirement does not name proves `none`, however useful.
- **Where it is unclear, write `unclear` and one line saying why.** Do not assign a requirement
  to fill the column.
- A test may prove more than one; name them all.
- For `none` and `unclear`, add one line saying what the test does assert.

**Count them**: tests traced, `none`, `unclear`.

**Drop candidate:** none.

### Task 2 - every requirement, and what proves it (0.2, 0.3)

The other direction, in the same file. Extend section T's table to **every requirement id**,
each row naming the test that proves it or `none`. Then state as numbers:

- how many requirements have at least one test;
- how many have none;
- **how many have a test that measures something other than what the requirement states.**

**Group the `none` rows by section** so the next unit knows which step is emptiest.

Then **0.3**: place every name in `docs\unit239-failing-set.txt`, the known-reds block, and
`docs\cw-retired-tests.txt` into the same table - the requirement it was evidence for, or
`none`. **Retire nothing, repair nothing, restore nothing.** A retired test that proves a
must-tier requirement is a finding, not a restoration.

**Drop candidate:** 0.3, with the counts stated.

### Task 3 - MET-INVENTED (1.1)

Build it first: HM-REQ-011 requires zero, and it is the number the phase's hardest fault is
stated in.

**Watch it fail first** against a pair whose answer is known by construction - a decode and a
key where the invented count can be counted by hand, written into the test.

Then measure over **every keyed recording**, reported **per condition** as `CW_SPEC.md` defines
a condition, with the key kind beside each number. **Print the table.** Today's expected total
is 17; if it comes out otherwise, that is a finding - say which recordings differ and by how
much.

**Drop candidate:** none.

### Task 4 - the other three, and the exit round (1.2, 1.3, 1.4, 1.5)

- **MET-CER-SURE**: of the characters emitted sure, the fraction wrong.
- **MET-COVERAGE**: of the characters sent, the fraction emitted sure.
- **MET-WBE**: word-boundary error, scored separately from character errors; `CwScorer.Kinds`
  already counts spaces added and missing - take the spec's definition over that breakdown and
  say how the two relate.

**The decoder has two classes today, sure and placeholder.** The dim class of HM-REQ-001 does
not exist and is step 3's. **Measure with what the decoder actually emits**, say so, and do not
invent a dim class here.

Each watched failing first on a known-by-construction case, each measured per condition with
the key kind beside the number, each table printed.

**1.4:** each of the four is reachable by the other CW test types, the way `CwScorer` is. Say
how, and name one type that now calls each.

**Exit round:** both carry-forward lines, the three floor tests with captures at 51, and every
type touched. **`git diff` over `src` between entry and exit prints nothing** - no decoder code
changed - and the transmit files print nothing against `7e209cb4`.

**Drop candidate:** MET-WBE last, then the baseline re-issue.

---

## 7. Parked - do not touch, do not raise

- **Every requirement's substance.** This unit traces and measures; steps 2 to 7 meet.
- **The dim class** (step 3's), **MET-TACQ and MET-LAT** (step 5's), **MET-PITCH-ERR** (step
  4's).
- **Any change to the decoder.** None here.
- **The launcher's stop 4.** The other thread's; do not touch `tools\arbiter\`.
- **The correctness phase's parked items and 5.1**, which travel with it.
- **The seven rulings of section R.** The owner's; the first is answered by HM-DEC-175.

## 8. What not to do

- **Do not change a file under `src` or any existing test file.**
- **Do not tick a criterion in `PHASE_PLAN.md`.** There is no judge in a hand run.
- **Do not write to `RUN_LEDGER.md`** or touch anything under `tools\arbiter\`.
- **Do not retire, repair or restore any test**, red or green.
- **Do not assign a requirement to a test to fill the column.** `unclear` is an answer.
- **Do not derive one metric from another** (V-05), and do not invent a dim class.
- **Do not report a number without its condition, its count and the key's kind.**
- **Do not tune a metric until a figure looks better.** They are instruments.
- **Do not touch what keys or transmits.**
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Report mismatches; repair nothing. American spelling. UTF-8. The four headings exactly.**

## 9. Committing and pushing

Commit per task. Push at the end and say whether it succeeded.

---

## 10. Reporting

`output.md` at the root, the four headings exactly as section 2 gives them. **Say in section 1
that this unit ran by hand, outside the loop, and why.**

```
READ IN THIS ORDER.

A. MET-INVENTED today over the keyed recordings, against HM-REQ-011's zero.
B. How many of the sixty-eight requirements have a test, how many have none,
   and how many have a test that measures something else.
C. The rest. Section 4 raises <n> items.
```

```
UNIT:       439 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
            ran by hand, outside the loop
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
NUMBER:     MET-INVENTED over the keyed recordings: <n> against HM-REQ-011's zero;
            requirements with a test <n> of 68, with none <n>, with a test that
            measures something else <n>
```

**Section 3 leads with MET-INVENTED's table, then the `none` rows grouped by section** - that
is the map the next six steps are aimed with.

**Section 2 tells the owner in one paragraph** that nothing in the app changed, what the
project can now measure that it could not before, and how much of the specification currently
has a test behind it.

---

```
ARBITER-DECISION
STEP: 1
APPROACH: trace every CW test to a requirement and extend section T to every requirement id, then build MET-INVENTED, MET-CER-SURE, MET-COVERAGE and MET-WBE as CW_SPEC.md defines them, each watched failing first, and measure all four over every keyed recording per condition
MOVE: continue
WHY: PHASE_PLAN.md step 1 criterion 1.1 asks that MET-INVENTED be computed as CW_SPEC.md defines it over every keyed recording and reported per condition, with today's figure stated and the recordings it comes from named
STATE: not started
DECIDED: the form of the traceability file, how a test's requirement is judged, where the metrics live, and the per-type timeouts are the author's, overrulable
LICENCE: PHASE_PLAN.md R77, R78, R79, section 3, section 6; HM-DEC-183; HM-DEC-175; HM-DEC-103; V-05; V-13; HM-DEC-155; CLAUDE.md 0.0, 0.2 and 12.6; FACT-006
ACCOMPLISHED: the project knows which parts of its own specification are tested, and can judge a change on what the requirements actually ask for - the rule six units were rejected against for want of it
ADVANCES: step 1 criterion 1
END-ARBITER-DECISION
```

# Work instruction 599 - The learned reader goes on

**Run budget: UNIT_RUN_BUDGET.md applies (HM-DEC-301) - one survey, one engine line, one app line,
at the end; quick measures per round.**

**Hand run. The owner has ruled: the learned reader, with its second model, is what Hamlet
reads with. Turn `CwRules.LearnedReader` on by default, keep the sequence reader behind the
same switch, restate the guards at the learned reader's figures, measure the dimming line
against the net's own errors, and record the ruling. Up to three rounds; the rounds fix the
switch, the tests and the records, never the model.**

**Standing rules** (HM-DEC-276, HM-DEC-284, HM-DEC-301) apply. `--no-build` after one build per
round, build servers shut down first, one test run at a time; a run stopped for memory is run
once more alone without asking.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted. The draft truth `cw-2026-10-19-141708.truth.md` is not edited. The sequence reader,
the gap learning, the marks, the lanes, the keyer, the wild bank and every board are not
touched; the model, the alphabet and the feature recipe are not touched.**

**Numbering.** This is unit 599, ruling HM-DEC-304. If taken, use the next free and say so.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwLearnedReader.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## 1. Why this unit exists

**What the operator reads with today is the sequence reader, and the learned reader beats it
on every real hand and on the air.** The last report, with the switch on and the second model:
KM3STU **55 of 65** against the shipped reader's 0; W1AW **2128** against 2101 (4 wrong
against 24); Ken **51 of 76** against 43; Bryan **124 of 144** against 118; the wild bank
**3271** against 2859; the fist bank **5086** against 3898; the 0 dB CQs **52 of 84** against
5; CPU on W1AW piece-04 **44.3 s** against 106.6 s. Against it: the twelve-plus-three **286**
against 308, the first recording 26 of 27 against whole, the easy fixtures 87 of 90, the 5 dB
CQs 83 of 84, letters in silences 11 against 1. The owner has heard the shipped reader on an
evening of real band since that report - three stations, two of them weak - and asked how to
turn the learned reader on. There is no control for it: the switch is `CwRules.LearnedReader`,
off by default in code since the reader shipped. **The number this unit is aimed at is what
the shipped build reads with the switch as it ships: the twelve-plus-three 308 and KM3STU 0 of
65 as shipped, becoming 286 and 55 of 65 as shipped.** The panel with the learned reader on
already shows pitch, a letter-rate speed marked `not proved`, and the light; nothing on
screen moves.

```
PHASE GOAL:   Hamlet meets its CW requirements.
UNIT GOAL:    The learned reader, second model, is what Hamlet reads with by default,
              every line green with it on, and the ruling is recorded.
ADVANCES:     task 2 (the switch). Task 3 (the dimming measure) is the drop candidate.
DRIFT:        0 consecutive units without advance (the last unit advanced).
```

---

## 2. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any
mismatch. Report the mismatch; do not repair the instruction. Mismatches go in the report even
when the work succeeded anyway.

Claims this instruction rests on, from the last report, each to be checked:
- `CwRules.LearnedReader` exists, off by default, wired as `CwRules.GapLearning` is wired. On,
  the sequence reader is not built; off, the learned reader is never constructed. The trace
  tests turn it on by `CwRules.Choose`.
- `src\Hamlet.RadioEngine\Cw\Model\cwnet.onnx` is the second model and
  `src\Hamlet.RadioEngine\Cw\Model\CWNET_SPEC.md` describes it; the vector files in
  `tests\fixtures\cwnet\` match it; `TheLearnedReaderTests` is 7 of 7 and on the engine
  line's carry-forward list.
- With the switch on the panel shows `CwDecoder.PanelPitchHz` on the `decoding at` line,
  `CwDecoder.WordsPerMinute` as a letter rate marked `not proved`, and the light from the
  newest provisional frames. With it off the panel is as shipped: `no station`, blank speed,
  no green light.
- A learned letter is dimmed where the net's probability is under one half.
- `TheScoreboardGuardsCwTests` (4 cases) guards the shipped figures 308, 2101, 3898 and
  2859; there may be other tests asserting the first recording whole, one letter in silences,
  the easy fixtures at 90 and the 5 dB CQs at 84. Task 1 finds every such assertion.
- The engine line is 175 of 175 and the app line 278 of 278 with the switch off.
- `docs\cw-survey-2026-10-09.md` carries a `Rerun` line marked *learned reader, second model*
  at 286 and 2128; `docs\cw-scoreboard.md` carries 598 rows marked *switch off*.
- An untracked `model\` at the root and an untracked `CwSequenceReader.cs.new`.
- The last report said the sequence reader does **not** feed the panel's pitch, speed and
  light, and that an earlier instruction claimed it did. Treat the report as right.

**Checks expected to fail:** none with the switch off. With the switch on, before task 2,
`TheScoreboardGuardsCwTests` and any test found in task 1 that asserts a shipped figure are
expected red; task 2 is what makes them green.

---

## 3. Rulings in force

Transcribed; do not re-argue any.

**HM-DEC-302 - The learned reader** (in force). A trained net - conv front end, two-layer
bidirectional GRU, CTC - reads the audio behind `CwRules.LearnedReader`, off by default. It
was trained on synthetic keying only and has heard no recording. The port is proved against a
shipped test vector; a mismatch is a bug in the port, never a reason to tune the model, the
alphabet or the feature recipe. *Rejected:* turning it on in that unit - the board fell to 238
and every guard would have had to be restated first. *Rejected:* dropping it - it beat the
shipped reader on both real hands in its first run, where four units of rules gained five
letters.

**HM-DEC-303 - The learned reader, second model** (in force). `cwnet.onnx` is replaced by a
model of the same shape, trained from the first on scenes the first never saw: the pitch at
the edge of a receiver passband, a residual floor outside the passband pumped by the
receiver's AGC, speeds down to 5 WPM, and silent scenes. It has heard no recording. The
feature recipe, the alphabet and the streaming rule are unchanged, so the port is unchanged;
the vector files are regenerated for the new model and the four vector tests pass. With the
switch on: KM3STU 55 of 65 (was 0), the twelve-plus-three 286 (was 238, shipped 308), W1AW
2128 (was 2048, shipped 2101), Bryan 124 of 144 (was 127, shipped 118), Ken 51 of 76 (was 51,
shipped 43), the wild bank 3271 (was 3307), the fist bank 5086 (was 5255), the first recording
26 of 27, letters in silences 11 (was 9), the random-keyed carrier at 20 of 20 seeds. The
panel with the switch on shows the learned letters' pitch, a speed from the letter rate, and
the light from the net's own activity. The switch ships off; the owner rules on turning it on.
*Rejected:* tuning the first model's features or thresholds to KM3STU - the recipe is the
contract with the port, and a model that needs the recipe bent for one station will need it
bent for the next. *Rejected:* training on the recordings - the recordings are the yardstick,
and a model that has heard them cannot be measured by them.

**HM-DEC-301 - Run budget** (in force). One survey, one engine line, one app line per unit, at
the end; quick measures per round; no rule-off survey; a work instruction that asks for more is
refused in output.md section 4. The lock is taken with the root named:
`tools\arbiter\lock.bat take C:\Source\HamLet`.

**HM-DEC-296 - Hamlet may know words** (in force). A vocabulary may inform the reader. It is
not used by the learned reader and is not this unit's business (parked, §6).

**The owner's standing orders**, not rulings: nothing keys or transmits; no recording is added
unless the owner says; the draft truth is corrected only by the owner's ear; one layout - nothing
on screen moves.

**The owner's ruling, 2026-11-06, to record as HM-DEC-304.** The owner asked how to turn the
learned reader on; that is the ruling asked for in the last two reports. Write to
`DECISIONS.md`, newest first, this text with the figures filled from the measure:

> **HM-DEC-304 - The learned reader is on.** `CwRules.LearnedReader` is on by default: what
> the operator reads with is the learned reader, second model. The sequence reader stays in
> the tree behind the same switch, off, untouched, and its own tests pin it on by
> `CwRules.Choose` so they keep testing it. The scoreboard guards are restated at the learned
> reader's figures: the twelve-plus-three <n> (was 308), W1AW <n> (was 2101), the fist bank
> <n> (was 3898), the wild bank <n> (was 2859); and every other test that asserted a shipped
> reader's figure now asserts the learned reader's - the first recording <n> of 27, letters
> in silences <n>, the easy fixtures <n> of 90, the 5 dB CQs <n> of 84 - each named in the
> unit's report. A learned letter is dimmed where the net's probability is under one half;
> measured on the twelve-plus-three, letters under one half are wrong <n> of <n> times and
> letters at or over it <n> of <n>. The panel shows the learned letters' pitch, a letter-rate
> speed marked `not proved`, and the light from the net's own activity. *Rejected:* removing
> the sequence reader - it is the only reader that measures a dit, and nothing is deleted for
> being off. *Rejected:* a control on the panel for the switch - the layout is the owner's,
> and two readers on offer to an operator is a choice nobody asked for. *Rejected:* holding
> the switch off until the twelve-plus-three recovers 308 - the twenty-two letters are spread
> across the owner's own boards while the real hands, the bulletin and the air all read
> better; the board is one yardstick and the air is the goal.

---

## 4. Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per `CLAUDE.md` -
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, and `NOTE` saying what is moving
inside the task, not restating the task name. The same every ten minutes while a task runs.

---

## 5. The tasks

Rules for every task: take the lock with **`tools\arbiter\lock.bat take C:\Source\HamLet`**,
release it at the end; write nothing to `RUN_LEDGER.md`; touch nothing under `tools\arbiter\`;
tick nothing in `PHASE_PLAN.md`; kill no process you did not start; one `dotnet test` per
line, filtered, with a `timeout`; apostrophes in quoted heredocs break, `;` is refused, Python
cannot run here, `-m` more than once for a multi-line commit; scripts in
`.run-unit\unit599-<name>.sh`, not committed; **no sed inserts into C# files** - edit with the
file tool. **Commit and push each task before starting the next.**

### Task 1 - Trace: what the switch touches

Say what you find rather than confirming this list. Nothing is built in this task.

1. Where `CwRules.LearnedReader` gets its default, by file and line, and every place that
   reads it: the engine, the app, the tab's chain, the survey, the trace tests, the probes.
2. **Every test that depends on the switch's default** - that is, every test that would change
   state if the default flipped - by name, and which way it would go. Expected: the
   scoreboard guards, any test asserting the first recording whole, one letter in silences,
   the easy fixtures at 90, the 5 dB CQs at 84, the panel showing `no station`, and any
   sequence-reader test that builds its reader through the default rather than naming it.
   Say which of these name their reader already and which ride the default.
3. **The quick measure, switch on, as it ships now** - the engine's figures for the
   twelve-plus-three with KM3STU's rows, W1AW, the first recording, the easy fixtures, the
   5 dB and 0 dB CQs, letters in silences, the random carrier. These are the before-numbers
   for the restated guards; where the last report already has them, say they match or do not.
4. Where the dimming line is applied (the probability compared with one half), and whether
   the net's per-letter probability is already kept somewhere a test can read it beside the
   board's right/wrong.

Report the findings before a line is written.

### Task 2 - The switch goes on

1. `CwRules.LearnedReader` defaults to **on**. The sequence reader is not touched; it is built
   when the switch is off exactly as today.
2. Every test from task 1 item 2 that rides the default and tests the **sequence reader** is
   pinned to it by `CwRules.Choose` (or however the trace tests pin the learned reader on),
   so it keeps testing what it tested. Nothing is deleted or skipped for being about the
   sequence reader; a test that exists to test the sequence reader still does.
3. Every test from task 1 item 2 that asserts a **shipped figure** is restated at the learned
   reader's figure from the quick measure: `TheScoreboardGuardsCwTests` at the learned
   reader's twelve-plus-three, W1AW, fist bank and wild bank; the first recording at its
   count of 27; letters in silences at its count; the easy fixtures at its count of 90; the 5
   dB CQs at its count of 84; the panel tests at what the learned reader shows. Each restated
   figure is the engine's measured figure, not a figure from this file. *Why restate rather
   than loosen:* a guard is a floor the next unit may not fall under; a guard at the old
   reader's figure for the new reader is a guard on nothing.
4. `TheLearnedReaderTests.TheSwitchOffIsSilent` still tests that the switch off builds no
   learned reader; rename nothing.
5. **Quick measure, as it now ships:** the twelve-plus-three with KM3STU's rows, W1AW, the
   first recording, letters in silences, the random carrier. Expected equal to task 1 item 3.
6. Commit, push.

### Task 3 - The dimming line, measured (the drop candidate)

The dimming line at one half was proposed two reports ago and has never been measured against
the net's errors. Measure it, do not move it:

- On the twelve-plus-three as the board scores it, for every printed letter: the net's
  probability for that letter and whether the board scored it right. Report four counts: under
  one half right, under one half wrong, at or over one half right, at or over one half wrong.
  Then the same four at one quarter and at three quarters, so the owner can see whether one
  half is where the wrong letters are.
- One `Survey`-filtered trace in `TheLearnedReaderTests` that prints these, on no line.
- The line stays at one half whatever the counts show; the counts go in the report and in
  HM-DEC-304. *Why not move it:* where a letter dims is what the display asserts, and that is
  the owner's, with the numbers in front of him.

If the probability is not kept anywhere a test can read it beside the board's scoring (task 1
item 4), say so and drop this task whole; do not plumb it this unit.

### Task 4 - The lines, the survey, the record

Run the app line, the engine line and the survey line **once, at the end**, with the switch
as it now ships (on). Rewrite `docs\cw-survey-2026-10-09.md` with a new `Rerun` line marked
*learned reader on*; every group.

- `PHASE_OUTCOME.md`, both copies: `## UNIT 599 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 599.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first: **HM-DEC-304** as transcribed in §3, figures filled.
- `docs\cw-scoreboard.md`: a 599 row in every table, marked *learned reader on - shipped*;
  the guard figures named in the scoreboard's own text updated to the restated ones.
- `CW_SPEC.md`, the section *The learned reader*: one paragraph saying the learned reader is
  what Hamlet reads with, the sequence reader is behind the switch, and what the guards are
  now.
- `CLAUDE.md`, wherever it states the shipped figures 308, 2101, 3898, 2859 or says the
  learned reader is off: updated to say what is true.
- Commit, push.

---

## 6. Parked - do not touch, do not raise

- **The random-keyed carrier** prints at 20 of 20 seeds; the net knows no words to refuse it.
  A vocabulary-aware decode on the net's scores is its own unit (HM-DEC-296 allows it).
- **Letters in silences, 11,** and the `E I EE` the owner saw on air between transmissions
  under the shipped reader: the learned reader's figure is restated as a guard this unit and
  brought down by a later model, not by a rule. Not traced here.
- **KM3STU's third recording** at 24 of 32: the model's business, parked with the model.
- **The speed marked `not proved`,** the pitch going blank after ten seconds of nothing, the
  letter rate reading low for its first ten seconds, the light updating once a second: built
  as the last unit decided, left as built; the owner has them in front of him now.
- **Ken's draft truth at 100-120 s** (`KB2UZK/M DE N3AFS K` against `N3AFS DE KB2UZY/M`): the
  owner's ear; not edited, not raised again.
- **The sequence reader not feeding the panel** when the switch is off: it is now off by
  default, so the panel the operator sees is the learned reader's. Not touched; the sequence
  reader is not this unit's.
- **The untracked `model\` at the root, `CwSequenceReader.cs.new`, and the uncommitted
  `PARKED.md`, `RUN_LEDGER.md`, `PROJECT_STATUS.md`, `tools\go-claude\GoClaude.bat`,
  `.run-unit\`, `cases-*.txt`:** the owner's to clean; known, not raised.
- **The asks outstanding** from units 587, 575, 562, 520, 440 and 487: carried forward as they
  are; not raised again.

A parked item that turns out to block a task is raised once, saying it was parked.

---

## 7. What not to do

Standing prohibitions are in `CLAUDE.md` and are not retyped. This unit's own:

- **Do not touch the model, the alphabet, the feature recipe or the vector files.** The
  recipe is the contract between the trained net and the port; this unit changes which reader
  is on, not what the reader is.
- **Do not delete, skip or weaken a sequence-reader test.** The sequence reader is off, not
  gone; a test that stops testing it is a deletion with extra steps.
- **Do not restate a guard above or below the measured figure.** A guard is the engine's own
  figure from this unit's measure, or it is a number somebody typed.
- **Do not put a control for the switch on the panel, and do not move anything on screen.**
  The layout is the owner's; the switch is code.
- **Do not move the dimming line.** Measure it; the line is the owner's.

---

## 8. Committing and pushing

Each task is committed and pushed before the next starts, branch `main`. The report names the
branch and says whether each push succeeded; a refused push is reported as refused, with the
reason.

---

## 9. Report

`output.md` at the root, per `CLAUDE_CODE.md` §8: the header block from the clock, then four
headings exactly: `## 1. What Claude did`, `## 2. What the owner should expect`,
`## 3. What you should see`, `## 4. What's blocking us`.

**Section 3 leads with the answer:** what the shipped build now reads with, and the shipped
figures - the twelve-plus-three with KM3STU's rows, W1AW, Ken and Bryan per 20 s, the wild
bank, the fist bank, the first recording, letters in silences, the easy fixtures, the CQs,
noise, the carriers, CPU - as they now stand with the switch as it ships, beside what shipped
before; then each guard restated, old figure to new, by test name; then the dimming counts at
one quarter, one half and three quarters, or that task 3 was dropped and why; then what the
panel shows in the owner's terms when he runs the app. Section 2, in plain words, what is now
true when the owner runs Hamlet and what will look wrong but is not - the speed marked `not
proved`, the pitch blank in silence, the light once a second. Section 1 per task, with every
test that was pinned or restated named. Section 4: anything needing a ruling, in the decision
log's format; then anything left, a line each.

**Write `output.md`, then stop.** Do not start the next unit.

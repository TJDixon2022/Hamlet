# Work instruction 600 - The dimming line, measured

**Run budget: UNIT_RUN_BUDGET.md applies (HM-DEC-301) - one survey, one engine line, one app line,
at the end; quick measures per round.**

**Hand run. The learned reader is what Hamlet reads with, and every learned letter is dimmed
where the net's probability is under one half. That line has never been measured, because the
scoreboard keeps no per-letter probability beside its right/wrong. This unit carries the
probability through to the scoreboard, measures the line at one quarter, one half and three
quarters on every recording the board scores, measures what the letters printed in silences
score, and records the counts. The line does not move. Nothing Hamlet prints changes. Up to
three rounds; the rounds fix the plumbing and the tests, never the model and never the line.**

**Standing rules** (HM-DEC-276, HM-DEC-284, HM-DEC-301) apply. `--no-build` after one build per
round, build servers shut down first, one test run at a time; a run stopped for memory is run
once more alone without asking.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted. The draft truth `cw-2026-10-19-141708.truth.md` is not edited. The sequence reader,
the gap learning, the marks, the lanes, the keyer, the wild bank and every board are not
touched; the model, the alphabet, the feature recipe and the vector files are not touched; the
dimming line stays at one half; nothing on screen moves.**

**Numbering.** This is unit 600, ruling HM-DEC-305. If taken, use the next free and say so.

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

**There is no number for the dimming line.** The learned reader dims a letter where the net's
probability for it is under one half; the line was proposed three reports ago and the last
unit was asked to measure it and dropped the task whole, because the scoreboard's printed-letter
record carries no probability a test can read beside the board's right/wrong. So the display
asserts something nobody has counted: the operator sees bright letters and dim letters and
nobody knows whether the dim ones are the wrong ones. The owner saw the shipped reader print
`E I EE` between transmissions on an evening of real band; the learned reader prints 11 letters
in the board's silences; whether those 11 are already dim is also uncounted. **The number this
unit is aimed at is the one that does not exist: on the twelve-plus-three, how often a letter
under one half is wrong and how often a letter at or over it is right, and how many of the 11
silence letters sit under the line.** The before-number is *none*. The board stays at 286 before
and after: this unit must not change a letter Hamlet prints.

```
PHASE GOAL:   Hamlet meets its CW requirements.
UNIT GOAL:    The net's per-letter probability reaches the scoreboard, the dimming line is
              measured at one quarter, one half and three quarters, and the counts are
              recorded; the line stays where it is and nothing Hamlet prints changes.
ADVANCES:     task 3 (the measure). Task 4 (the off-line set) is the drop candidate.
DRIFT:        0 consecutive units without advance (the last unit advanced).
```

This unit is the measurement, so the trace in task 1 is small and the measure is task 3; the
plumbing in task 2 is what the last unit said was missing.

---

## 2. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any
mismatch. Report the mismatch; do not repair the instruction. Mismatches go in the report even
when the work succeeded anyway.

Claims this instruction rests on, from the last report, each to be checked:
- `CwRules.LearnedReader` is **on** by default; the sequence reader is built only when it is
  off; sequence-reader tests pin it off by `CwRules.Off(CwRules.LearnedReader)` (the last
  report corrected an instruction that said `Choose`). The shipped version is 1.13.286.
- The dimming line is applied in `src\Hamlet.RadioEngine\Cw\CwLearnedReader.cs` near line 526,
  as `score >= 0.5 ? High : Low` or words to that effect, where `score` is the net's probability
  for the letter.
- The scoreboard scores a recording against its truth by edit distance; its printed-letter
  record (`Printed`, or whatever it is called) carries the letter and its time but **no
  probability**. That is why the last unit dropped the dimming measure.
- The guards as the last unit restated them, with the learned reader on: the twelve-plus-three
  **286**, invented 3, spaces right 70, added 4, letters in silences **11**; the first recording
  reads `FER CHAT<BT> BEST 7V 73 <SK> KC4ZP DE WA` (26 of 27); W1AW **2128** with 4 wrong; the
  fist bank guarded by rows at 1212, 1359, 1372 and 1346, fading 2544 less five percent; the
  easy fixtures 87 of 90; the 5 dB CQs 83 of 84; silence after a weak station at most 1. No test
  guards the wild bank. These are this unit's before-numbers and must be its after-numbers.
- `TheLearnedReaderTests` holds the vector tests and `TheSwitchOffIsSilent`; the four vector
  tests pass against `tests\fixtures\cwnet\`.
- The last report said some fifty-five test files ride the switch's default and are on neither
  the engine line nor the app line, so they have never been run with the learned reader on. Task
  1 finds the exact set; task 4 runs it once.
- The engine line and the app line were green with the switch on at the end of the last unit;
  the survey line carries a `Rerun` marked *learned reader on*.
- Untracked `model\` at the root and `CwSequenceReader.cs.new`; the owner's.

**Checks expected to fail:** none. If any guard above is red before a line is written, stop and
report it in section 4; the tree is not where the last report left it.

---

## 3. Rulings in force

Transcribed; do not re-argue any.

**HM-DEC-304 - The learned reader is on** (in force). `CwRules.LearnedReader` is on by default:
what the operator reads with is the learned reader, second model. The sequence reader stays in
the tree behind the same switch, off, untouched, and its own tests pin it on so they keep
testing it. The scoreboard guards are restated at the learned reader's figures, and every other
test that asserted a shipped reader's figure now asserts the learned reader's. A learned letter
is dimmed where the net's probability is under one half; the measure of that line was dropped
because the probability is not kept where a test can read it. The panel shows the learned
letters' pitch, a letter-rate speed marked `not proved`, and the light from the net's own
activity. *Rejected:* removing the sequence reader - it is the only reader that measures a dit,
and nothing is deleted for being off. *Rejected:* a control on the panel for the switch - the
layout is the owner's, and two readers on offer to an operator is a choice nobody asked for.
*Rejected:* holding the switch off until the twelve-plus-three recovers 308 - the twenty-two
letters are spread across the owner's own boards while the real hands, the bulletin and the air
all read better; the board is one yardstick and the air is the goal.

**HM-DEC-303 - The learned reader, second model** (in force). `cwnet.onnx` is a model of the
first's shape, trained from it on scenes the first never saw: the pitch at the edge of a
receiver passband, a residual floor outside the passband pumped by the receiver's AGC, speeds
down to 5 WPM, and silent scenes. It has heard no recording. The feature recipe, the alphabet
and the streaming rule are unchanged, so the port is unchanged. *Rejected:* tuning the features
or thresholds to one station - the recipe is the contract with the port, and a model that needs
the recipe bent for one station will need it bent for the next. *Rejected:* training on the
recordings - the recordings are the yardstick, and a model that has heard them cannot be
measured by them.

**HM-DEC-302 - The learned reader** (in force). A trained net - conv front end, two-layer
bidirectional GRU, CTC - reads the audio behind `CwRules.LearnedReader`. The port is proved
against a shipped test vector; a mismatch is a bug in the port, never a reason to tune the
model, the alphabet or the feature recipe.

**HM-DEC-301 - Run budget** (in force). One survey, one engine line, one app line per unit, at
the end; quick measures per round; no rule-off survey; a work instruction that asks for more is
refused in output.md section 4. The lock is taken with the root named:
`tools\arbiter\lock.bat take C:\Source\HamLet`.

**HM-DEC-296 - Hamlet may know words** (in force). A vocabulary may inform the reader. Not
used by the learned reader; not this unit's business (parked, §6).

**The owner's standing orders**, not rulings: nothing keys or transmits; no recording is added
unless the owner says; the draft truth is corrected only by the owner's ear; one layout -
nothing on screen moves.

**The record this unit writes, as HM-DEC-305.** This is a measurement recorded, not a ruling
asked for; the line stays at one half and the owner rules on it later with the numbers in front
of him. Write to `DECISIONS.md`, newest first, this text with the figures filled from the
measure:

> **HM-DEC-305 - The dimming line, measured.** The net's probability for each learned letter
> travels with the letter to the scoreboard, which now keeps it beside the board's right/wrong;
> nothing printed, dimmed or shown changed, the twelve-plus-three at 286 before and after. On
> the twelve-plus-three as the board scores it, letters under one half are wrong <n> of <n>
> and letters at or over one half are right <n> of <n>; at one quarter, under <n> wrong of <n>,
> over <n> right of <n>; at three quarters, under <n> wrong of <n>, over <n> right of <n>. On
> the fist bank rows: <the same at one half>. On W1AW: <the same at one half>. Of the <n>
> letters printed in the board's silences, <n> sit under one half and are already dim. The line
> stays at one half: where a letter dims is what the display asserts, and that is the owner's,
> with these numbers in front of him. *Rejected:* moving the line in this unit to the best of
> the three - a line chosen by the unit that measured it is a line chosen on one board, and the
> owner has not seen it. *Rejected:* dimming by the board's right/wrong instead of the net's
> probability - the board knows the truth and the operator's radio does not; the display may
> only use what the net knows at the time.

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
`.run-unit\unit600-<name>.sh`, not committed; **no sed inserts into C# files** - edit with the
file tool. **Commit and push each task before starting the next.**

### Task 1 - Trace: where the probability is born and where it dies

Say what you find rather than confirming this list. Nothing is built in this task.

1. **Where the probability is born.** In the learned reader, by file and line: what `score`
   is - the softmax of the net's logits at the frame the letter was emitted, or the best of a
   run of frames, or something else - and whether it is the same figure the dimming line
   compares with one half. Say exactly what the figure is so the record can say it.
2. **Where it dies.** The path from the learned reader's letter to the scoreboard's printed
   record: every type the letter passes through (the decoder's frame, the provisional and
   committed letter, the tab's chain, the scoreboard's record), by name, and the first one that
   drops the probability. If a type on the path already carries it and only the scoreboard's
   record does not, say so - the plumbing is then smaller than task 2 describes.
3. **How the board scores a printed letter.** The edit-distance alignment: whether it marks
   each printed letter right, substituted or inserted, and whether that mark is kept per letter
   or only summed. If only summed, task 2 keeps it per letter.
4. **The silences.** How the board counts "letters in silences" (the 11): which stretches of
   which recordings, and whether those letters go through the same printed record, so their
   probabilities can be read in task 3.
5. **The off-line set.** The exact list of test classes that ride the switch's default and are
   on neither the engine line nor the app line - the last report said about fifty-five files.
   Name them and say how they are filtered (category, trait, name), so task 4 can run them in
   one filtered `dotnet test`.
6. **Quick measure, as it ships now:** the twelve-plus-three with KM3STU's rows, letters in
   silences, W1AW, the first recording. Expected 286 / 11 / 2128 / 26 of 27; say match or not.

Report the findings before a line is written.

### Task 2 - The probability travels with the letter

1. Carry the net's probability from where it is born (task 1 item 1) to the scoreboard's
   printed record, through every type on the path (task 1 item 2). Where a type is shared with
   the sequence reader, the probability is **optional** (nullable, or a value the sequence
   reader never sets); the sequence reader is not touched and its letters carry none.
2. The scoreboard keeps, per printed letter, the probability and the board's own mark for that
   letter - right, substituted, inserted (task 1 item 3). Nothing about how the board scores
   changes; the figures it reports do not move.
3. **Nothing printed, dimmed or shown changes.** The dimming line still reads the same figure
   it read before; the panel is untouched; the reader's output is byte-identical.
4. `TheLearnedReaderTests`: one test that reads one recording through the chain and asserts
   that every learned letter on the scoreboard carries a probability in [0, 1], and that a
   sequence-reader run (pinned off with `CwRules.Off(CwRules.LearnedReader)`) carries none. On
   the engine line.
5. **Quick measure:** the twelve-plus-three with KM3STU's rows, letters in silences, W1AW, the
   first recording. **Must equal task 1 item 6 exactly** - 286, 11, 2128, 26 of 27 - or the
   plumbing changed what prints and the round is spent finding where. *Why exact:* a plumbing
   unit that moves a figure by one has done something else as well, and the measure in task 3
   would then be a measure of something other than the shipped reader.
6. Commit, push.

### Task 3 - The dimming line, measured (advances)

Measure; do not move.

1. One `Survey`-filtered trace in `TheLearnedReaderTests`, on no line, that runs the board and
   prints, for each of: the twelve-plus-three as one set; the fist bank rows as one set; W1AW;
   Ken 0-120 s and Bryan 120-300 s if the board has their truths; the first recording -
   twelve counts: at each of one quarter, one half and three quarters, letters under the line
   right / wrong, letters at or over the line right / wrong. *Wrong* is substituted or inserted;
   a truth letter the reader never printed is not a letter and is not counted. Also the number
   of letters each set holds and how many carried no probability (expected 0 with the switch
   on).
2. The same trace prints, for the letters the board counts as printed in silences: each
   letter, its probability, and the count under one half. *Why:* these are the `E I EE` the
   owner saw; if they are mostly already dim, the display is doing the right thing and the
   later model only has to finish the job; if they are bright, that is a finding.
3. The trace prints a short histogram of probability for right letters and for wrong letters
   on the twelve-plus-three, ten bins, so the owner can see the shape and not only three cuts.
4. Run it once; the figures go in the report and in HM-DEC-305.
5. Commit, push.

### Task 4 - The off-line set, once (the drop candidate)

The test classes from task 1 item 5 have never run with the learned reader on. Run them once,
in one filtered `dotnet test` with a `timeout`, and report: how many pass, how many fail, and
for each failure one line saying whether it fails because it asserted a shipped-reader figure
or shape (restate it at the learned reader's measured figure, as the last unit did for the
lines), because it tests the sequence reader through the default (pin it off with
`CwRules.Off(CwRules.LearnedReader)`), or for some other reason (leave it red, name it in
section 4). *Why once:* these are the tests the shipped reader is not measured by; a red among
them is a guard on nothing until someone looks, and nobody has.

**Budget.** This run is in addition to HM-DEC-301's three. If Claude Code reads HM-DEC-301 as
forbidding it, drop this task whole, say so in section 4, and the rest of the unit stands; do
not spend a round arguing it.

Commit, push.

### Task 5 - The lines, the survey, the record

Run the app line, the engine line and the survey line **once, at the end**. The survey's
figures are expected unchanged from the last `Rerun`; add a `Rerun` line marked *probability
plumbed - unchanged* only if the survey's own text expects a line per unit, else leave it.

- `PHASE_OUTCOME.md`, both copies: `## UNIT 600 - STEP 13`, one paragraph leading with the
  counts at one half.
- `PHASE_STATUS.md`, both copies: names 600.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first: **HM-DEC-305** as transcribed in §3, figures filled.
- `docs\cw-scoreboard.md`: a 600 row in every table, marked *probability plumbed - unchanged*,
  with the figures the board reports this unit; and a short new table or paragraph, *The dimming
  line*, carrying the twelve counts on the twelve-plus-three and the silence count.
- `CW_SPEC.md`, the section *The learned reader*: one paragraph saying what the probability is,
  that it travels to the scoreboard, where the line sits and what was measured.
- Commit, push.

---

## 6. Parked - do not touch, do not raise

- **Moving the dimming line.** Measured this unit; the owner rules with the numbers. Not moved,
  not proposed.
- **Letters in silences, 11,** and the `E I EE` between transmissions: counted and scored this
  unit, brought down by a later model, not by a rule. A third model was in training when the
  owner stopped it; it is the web session's business, not this tree's.
- **The random-keyed carrier** at 20 of 20 seeds; a vocabulary-aware decode is its own unit
  (HM-DEC-296 allows it).
- **KM3STU's third recording** at 24 of 32: the model's business.
- **The speed marked `not proved`,** the pitch going blank after ten seconds of nothing, the
  letter rate reading low for its first ten seconds, the light updating once a second: left as
  built; the owner has them in front of him.
- **Ken's draft truth at 100-120 s**: the owner's ear; not edited, not raised again.
- **The wild bank unguarded** and the fist bank guarded by rows rather than its total: as the
  last unit left them; a guard unit if the owner asks.
- **The untracked `model\` at the root, `CwSequenceReader.cs.new`, and the uncommitted
  `PARKED.md`, `RUN_LEDGER.md`, `tools\go-claude\GoClaude.bat`, `.run-unit\`, `cases-*.txt`:**
  the owner's to clean; known, not raised.
- **The asks outstanding** from units 587, 575, 562, 520, 440 and 487: carried forward as they
  are; not raised again.

A parked item that turns out to block a task is raised once, saying it was parked.

---

## 7. What not to do

Standing prohibitions are in `CLAUDE.md` and are not retyped. This unit's own:

- **Do not move the dimming line, and do not change what is dimmed.** The line is the owner's;
  this unit gives him the numbers to rule with. A unit that measures a line and moves it in the
  same breath has measured nothing the owner can check.
- **Do not change a figure Hamlet prints.** The quick measure in task 2 must equal task 1
  exactly; the plumbing carries a number alongside the letter and does nothing else.
- **Do not touch the model, the alphabet, the feature recipe or the vector files.** The recipe
  is the contract between the trained net and the port.
- **Do not let the probability reach the sequence reader or the panel.** The sequence reader
  has no such figure and is not taught one; the panel does not change this unit.
- **Do not delete, skip or weaken a test found red in task 4.** Restate a figure at the measured
  one, pin a reader, or leave it red and name it. A test that stops testing is a deletion with
  extra steps.

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

**Section 3 leads with the answer.** First line: whether anything Hamlet prints changed -
expected *no change on the air or on any board; the twelve-plus-three 286 before and after*.
Then the twelve counts on the twelve-plus-three, at one quarter, one half and three quarters,
as four numbers per cut; then the same at one half for the fist bank rows, W1AW and the real
hands; then the silence letters - each with its probability, and how many are under one half;
then the two histograms; then, in the owner's terms, what this says about the bright and dim
letters he sees on the panel. Then task 4: how many of the off-line set passed, how many were
restated, pinned or left red, by name. Section 2, in plain words: nothing looks different; the
bright/dim letters are now counted. Section 1 per task, with the exact figure the probability
is (task 1 item 1), every type the plumbing touched, and every test added, restated or pinned,
named. Section 4: anything needing a ruling, in the decision log's format - *not* a proposal to
move the line, which is parked; then anything left, a line each.

**Write `output.md`, then stop.** Do not start the next unit.

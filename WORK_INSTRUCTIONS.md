# Work instruction 598 - The learned reader, second model: the passband edge, and the panel

**Run budget: UNIT_RUN_BUDGET.md applies (HM-DEC-301) - one survey, one engine line, one app line,
at the end; quick measures per round.**

**Hand run. Replace the learned reader's model with one trained on scenes the first never saw,
prove the port still matches its vector, wire the panel to the learned reader, and measure, for
the owner's ruling on turning the switch on. The switch ships off. Up to three rounds; the rounds
fix the port and the panel, never the model.**

**Standing rules** (HM-DEC-276, HM-DEC-284, HM-DEC-301) apply. `--no-build` after one build per
round, build servers shut down first, one test run at a time; a run stopped for memory is run
once more alone without asking.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted. The draft truth `cw-2026-10-19-141708.truth.md` is not edited. The sequence reader,
the gap learning, the marks, the lanes, the keyer, the wild bank and every board are not
touched.**

**Numbering.** This is unit 598, ruling HM-DEC-303. If taken, use the next free and say so.

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

**With the switch on, the twelve-plus-three reads 238 against the shipped reader's 308, and
65 of the 70 letters lost are KM3STU's: his three recordings at 800 Hz score 0 of 65** (the
last report, section 2: the net printed `KM3SKE CQ`, `RR` and nothing, and the report confirmed
that is what the net prints, not the scorer dropping letters). On the same run the learned
reader read **Bryan 127 of 144 against 118** and **Ken 51 of 76 against 43**, the wild bank
3307 against 2859, the fist bank 5255 against 3898, W1AW 2048 against 2101, CPU 44.7 s
against 108.6 s. The board is the only row that fell, and one station is nearly all of it.

**Why he was not read, traced outside the tree.** KM3STU's tone sits at the top edge of the
IC-7300's passband (the filter runs about 340 to 870 Hz on these captures, and it is a wall: 25
dB down within 60 Hz of the edge). The radio's noise floor outside the passband sits *after*
the AGC, so when his keying pulls the gain down 20 dB the in-band noise falls and that floor
does not; after the feature recipe's per-frame median cancellation it shows as a **bright
shadow of his keying in the bins just above his tone**, and the radio's AGC rides the noise
itself, so the shadow flickers through every gap. Blank those five bins by hand and the first
model reads him at once (`R 55N 55N VA VA BK BK TU RON VA S72 KM3STU CQP`); so the model was
right and the shadow was unknown to it. The first model's synthetic radio had a sloping filter,
no floor after the AGC, and an AGC that moved the noise 1 to 5 dB. The second model,
`cwnet.onnx` in this zip, was trained from the first on scenes of the owner's radio as
measured - the wall, the post-AGC floor, the noise-riding AGC, the sender within 140 Hz of the
top edge - plus speeds down to 5 WPM and silent scenes. Read here before any code was written, it prints
**`2KW3SOEE CQ POTA DE KM3STUK KQ4PAK`** on his first recording (the first model: `JKM3SKE CQ`), **`K RRTNX AGN URNN MK RR UR 55N 55N VA VA BK BKT`**
on his second (`L RX RR`) and **`RNKU S72M3ST CQP`** on his third (`R`); Bryan **125 of
144** and Ken **73 of 89** by letter against the draft truth, where the first model read
128 and 66 the same way; the first recording 26 of 27, unchanged. **The number this unit is
aimed at is the twelve-plus-three with the switch on, 238, and inside it KM3STU, 0 of 65.** The
figures above are the model's reads here, not the engine's; task 1 produces the engine's.

```
PHASE GOAL:   Hamlet meets its CW requirements.
UNIT GOAL:    The learned reader reads the station at the passband edge, and the panel
              (pitch, speed, the light) shows what it hears, so the owner can rule on
              turning it on.
ADVANCES:     task 2 (the model) and task 3 (the panel). Task 3 is also the drop candidate.
DRIFT:        1 consecutive unit without advance (the last unit shipped the switch off and
              carried no count; this is the first count).
```

---

## 2. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any
mismatch. Report the mismatch; do not repair the instruction. Mismatches go in the report even
when the work succeeded anyway.

Claims this instruction rests on, from the last report, each to be checked:
- `src\Hamlet.RadioEngine\Cw\CwLearnedReader.cs` exists and is built to `CWNET_SPEC.md`; the
  model is `src\Hamlet.RadioEngine\Cw\Model\cwnet.onnx`, a content item copied beside the
  engine, loaded once with `IntraOpNumThreads = 1`.
- `CwRules.LearnedReader` exists, off by default, wired as `CwRules.GapLearning` is wired.
- `TheLearnedReaderTests` has four cases (`FeaturesMatchTheVector`, `TheNetMatchesTheVector`,
  `ReadsTheVector`, `TheSwitchOffIsSilent`), all passing, and is on no carry-forward line.
- The vector files sit in `tests\fixtures\cwnet\`.
- Each learned letter carries one mark measured afterwards: its span runs back from the frame
  that named it to the last letter, 0.6 s at most, and its pitch is the bin with the most raw
  power over that span.
- A learned letter is dimmed where the net's probability is under one half.
- With the switch on, the detector hands on no printed station, so the panel's pitch and speed
  have nothing to read and the light may not reflect what prints (the last report, section 4,
  item 3 - *from reading the code, not from running the app*).
- `docs\cw-survey-2026-10-09.md` carries a `Rerun` line marked *learned reader* at 238 and 2048.
- An untracked `model\` at the root and an untracked `CwSequenceReader.cs.new`.

**Checks expected to fail:** none. The engine line was 168 of 168 and the app line 278 of 278
with the switch off.

---

## 3. Rulings in force

Transcribed; do not re-argue either.

**HM-DEC-302 - The learned reader** (in force). A trained net - conv front end, two-layer
bidirectional GRU, CTC - reads the audio behind `CwRules.LearnedReader`, off by default. It
was trained on synthetic keying only and has heard no recording. The port is proved against a
shipped test vector; a mismatch is a bug in the port, never a reason to tune the model, the
alphabet or the feature recipe. *Rejected:* turning it on in that unit - the board fell to 238
and every guard would have had to be restated first. *Rejected:* dropping it - it beat the
shipped reader on both real hands in its first run, where four units of rules gained five
letters.

**HM-DEC-301 - Run budget** (in force). One survey, one engine line, one app line per unit, at
the end; quick measures per round; no rule-off survey; a work instruction that asks for more is
refused in output.md section 4. The lock is taken with the root named:
`tools\arbiter\lock.bat take C:\Source\HamLet`.

**HM-DEC-296 - Hamlet may know words** (in force). A vocabulary may inform the reader. It is
not used by the learned reader and is not this unit's business (parked, §6).

**The owner's standing orders**, not rulings: nothing keys or transmits; no recording is added
unless the owner says; the draft truth is corrected only by the owner's ear; one layout - nothing
on screen moves.

**Pending the owner, not ruled - treat as the last unit built them:** the switch stays off
this unit; the dimming line stays at one half. Neither is re-argued here; both are asked again
in section 4 of the report with this unit's figures beside them.

**To record, HM-DEC-303 - The learned reader, second model.** Write to `DECISIONS.md`, newest
first, this text with the figures filled from the measure:

> **HM-DEC-303 - The learned reader, second model.** `cwnet.onnx` is replaced by a model of
> the same shape, trained from the first on scenes the first never saw: the pitch at the edge
> of a receiver passband, a residual floor outside the passband pumped by the receiver's AGC,
> speeds down to 5 WPM, and silent scenes. It has heard no recording. The feature recipe, the
> alphabet and the streaming rule are unchanged, so the port is unchanged; the vector files are
> regenerated for the new model and the four vector tests pass [or: fail, at <cell>]. With the
> switch on: KM3STU <n> of 65 (was 0), the twelve-plus-three <n> (was 238, shipped 308), W1AW
> <n> (was 2048, shipped 2101), Bryan <n> of 144 (was 127, shipped 118), Ken <n> of 76 (was
> 51, shipped 43), the wild bank <n> (was 3307), the fist bank <n> (was 5255), the first
> recording <n> of 27, letters in silences <n> (was 9), the random-keyed carrier at <n> of 20
> seeds. The panel with the switch on shows the learned letters' pitch, a speed from the
> letter rate, and the light from the net's own activity [or: not wired - dropped]. The switch
> ships off; the owner rules on turning it on. *Rejected:* tuning the first model's features
> or thresholds to KM3STU - the recipe is the contract with the port, and a model that needs
> the recipe bent for one station will need it bent for the next. *Rejected:* training on
> the recordings - the recordings are the yardstick, and a model that has heard them cannot be
> measured by them.

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
`.run-unit\unit598-<name>.sh`, not committed; **no sed inserts into C# files** - edit with the
file tool. **Commit and push each task before starting the next.**

### Task 1 - Trace: what the engine reads today, and what the panel shows

Say what you find rather than confirming this list. Nothing is built in this task.

1. With the switch on and the **first** model (the one in the tree): the learned reader's full
   text for KM3STU's three recordings (`cw-2026-10-08-121324`, `-121357`, `-121414`), verbatim,
   and the twelve-plus-three's score with his rows marked. This is the before-number.
2. The panel with the switch on, **from running the app on a fixture, not from reading the
   code**: what the pitch shows, what the speed shows, what the light does while the learned
   reader prints. If a path from the learned reader to the panel already exists and is merely
   unwired, say so - that is tokens back.
3. Where the panel's pitch, speed and light are fed from today with the switch off (the
   detector's printed station, or whatever the tree does), by file and method, so task 3 feeds
   the same three things and nothing else.

Report the findings before a line is written.

### Task 2 - The second model

1. Replace `src\Hamlet.RadioEngine\Cw\Model\cwnet.onnx` with `model\cwnet.onnx` from this zip,
   and the four vector files in `tests\fixtures\cwnet\` with this zip's
   `cwnet-vector.wav`, `cwnet-vector-feat.csv`, `cwnet-vector-ids.txt`, `cwnet-vector-text.txt`
   (the text is still `ROGER TNX FER CALL`; **the ids differ**, since the net differs; the
   features and the wav are regenerated with the same seed and should be identical to the
   old - if `FeaturesMatchTheVector` changes state, the port or the fixture copy is wrong, not
   the recipe). Replace `src\Hamlet.RadioEngine\Cw\Model\CWNET_SPEC.md` with this zip's.
2. Run `TheLearnedReaderTests`. All four must pass. If `FeaturesMatchTheVector` fails, give
   the first differing cell - row, column, both values - and stop this task: the recipe did not
   change, so a failure here means the fixture copy or the port moved; find which.
3. **Quick measure, switch on:** the twelve-plus-three with KM3STU's rows, W1AW, the wild
   bank, the two real hands per 20 s as the survey scores them, the first recording, the 0 dB
   CQs, letters in silences, the random-keyed carrier.
4. Put the switch back off. Commit, push.

**Nothing in the model, the alphabet or the feature recipe is tuned.** If KM3STU still does
not read, that is the finding; it goes in the report with his verbatim text and the unit goes
on to task 3. The rounds do not touch the model.

### Task 3 - The panel reads the learned reader (the drop candidate)

With the switch on, the panel shows what the learned reader hears, in the same three places the
sequence reader feeds, and nothing on screen moves:

- **Pitch:** the pitch of the learned letters' marks (each letter already carries one, from
  the last unit), the median over the last ten printed letters; blank until there are three.
  *Why the median:* the net reads the louder of two stations, and one letter's mark can land
  on the other.
- **Speed:** from the letter rate - letters and spaces printed in the last 10 s, as words per
  minute at five characters a word (PARIS); blank under five characters. The net measures no
  dit, so this is a rate, and the report says so where the panel is described. *Why a rate and
  not a dit:* the net gives letters, not marks, and a dit measured from the afterwards-marks
  would be a second measurement of something the net does not assert.
- **The light:** lit while the net's newest provisional frames - the last second, not yet
  committed - carry a non-blank argmax, so it follows keying as it happens; dark in gaps and
  in noise. *Why provisional frames:* the committed stream is two seconds behind, and a light
  two seconds late tells the operator nothing.

The learned reader exposes these three as the sequence reader exposes its own, and the panel
reads whichever reader is on; with the switch off, nothing in this task runs. One test per
value in `TheLearnedReaderTests` (on the vector: a pitch within one bin of 579 Hz; a speed between 15
and 30 WPM - its letters run 0.26 s to 10.56 s, 18 characters, about 21 WPM by letter rate;
the light lit during the first `ROGER` and dark before 0.2 s), each under ten seconds.

If this task cannot be done inside the budget, drop it **whole** and say so; do not leave a
half-wired panel.

### Task 4 - The lines, the survey, the record

Run the app line, the engine line and the survey line **once, at the end** (the survey with
the switch **on**; no switch-off survey). `TheLearnedReaderTests` goes onto the engine line's
carry-forward list. Rewrite `docs\cw-survey-2026-10-09.md` with a new `Rerun` line marked
*learned reader, second model*; every group.

- `PHASE_OUTCOME.md`, both copies: `## UNIT 598 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 598.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first: **HM-DEC-303** as transcribed in §3, figures filled.
- `docs\cw-scoreboard.md`: a 598 row in every table, marked *learned reader, second model,
  switch off*.
- `CW_SPEC.md`, the section *The learned reader*: one paragraph added on what the second model
  was trained on and what the panel shows with the switch on.
- Put the switch back off before the lines run if any step left it on. Commit, push.

---

## 6. Parked - do not touch, do not raise

- **The random-keyed carrier** prints at 20 of 20 seeds, as shipped does; the net knows no
  words to refuse it. A vocabulary-aware decode on the net's scores is its own unit (HM-DEC-296
  allows it).
- **Ken's draft truth at 100-120 s** (`KB2UZK/M DE N3AFS K` against both readers'
  `N3AFS DE KB2UZY/M`): the owner's ear; not edited, not raised again.
- **The dimming line** at one half and **turning the switch on**: asked in the last report,
  pending the owner; this unit's report puts the new figures under the same two questions in
  section 4 and argues neither.
- **The untracked `model\` at the root, `CwSequenceReader.cs.new`, and the uncommitted
  `PARKED.md`, `RUN_LEDGER.md`, `PROJECT_STATUS.md`, `tools\go-claude\GoClaude.bat`,
  `.run-unit\`, `cases-*.txt`:** the owner's to clean; known, not raised.
- **W1AW's slow pieces** losing spaces (piece-01 38 of 63): the second model trained on
  5-9 WPM scenes for this; it is measured in task 2's W1AW figure and not traced further here.
- **The asks outstanding** from units 587, 575, 562, 520, 440 and 487: carried forward as they
  are; not raised again.

A parked item that turns out to block a task is raised once, saying it was parked.

---

## 7. What not to do

Standing prohibitions are in `CLAUDE.md` and are not retyped. This unit's own:

- **Do not tune the model, the alphabet or the feature recipe to a station.** The recipe is
  the contract between the trained net and the port; bent for one station it is wrong for the
  next, and the vector tests stop meaning anything.
- **Do not train, fine-tune or score the model on the recordings, and do not add a
  recording.** The recordings are the yardstick; a model that has heard them cannot be
  measured by them.
- **Do not touch the sequence reader, its marks, lanes, gap learning or guards, and do not
  restate any scoreboard guard.** The switch ships off; the shipped figures stand at 308, 2101,
  3898 and 2859 until the owner rules.
- **Do not move anything on screen.** The panel's three values appear where they appear now,
  same font, same place; the layout is the owner's.

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

**Section 3 leads with the answer:** KM3STU's three recordings, letters right of 65, first
model against second, with his verbatim text under each; then the twelve-plus-three, 238 to
what; then Ken and Bryan per 20 s, shipped, first model, second model, with the full text of
each; then the rest of the rows (W1AW, the wild bank, the fist bank, the easy fixtures, the 0
dB CQs, noise, the steady carrier, the random carrier, the first recording, letters in
silences, CPU on W1AW piece-04); then the vector tests; then what the panel shows with the
switch on, in the owner's terms, or that task 3 was dropped. Section 2, in plain words, what is
now true and what will look wrong but is not. Section 1 per task. Section 4: the two pending
questions (the switch, the dimming) restated with this unit's figures beside them, in the
decision log's format; then anything left, a line each.

**Write `output.md`, then stop.** Do not start the next unit.

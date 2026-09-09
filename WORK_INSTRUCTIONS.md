# Work instruction 292 - pressing FT4 tunes and decodes FT4

```
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
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

*The four checks were run against this tree while authoring: `SHACK_FACTS.md` present,
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` present, `CoreHMI.sln` absent,
`MURC.sln` absent. They are the project's four and not the arbiter's to change.*

---

## THE TWO RULES THAT KILLED SESSIONS

**Tim's rulings of 2026-09-05, HM-DEC-155.**

**1. A unit runs no test suite.** **Only the unit tests it constructs or rewrites in
this work instruction**, filtered by exact name, foregrounded, with a stated timeout.
**An unfiltered `dotnet test` on any project is forbidden.**

**2. Never background a command and poll for it.** The watchdog fires after twelve
minutes with no status write.

`dotnet build` is allowed, foregrounded, with a timeout.

**Tool fact, eighteen units old:** this shell will not carry a quoted heredoc
containing an apostrophe, and it collapses a doubled backslash inside one. Use script
files. **Keep apostrophes out of arguments to the arbiter scripts.**

**Tool fact, and it is now measured three times running.** Units 289, 290 and 291 all
reported that their shells refused every `.bat` in every invocation form tried. Unit 291
was told the authoring session had run `./tools/arbiter/outcome-read.bat` successfully
and was told not to assume the refusal; **it tried exactly that form and was refused
anyway.** **The authoring session for this instruction ran `outcome-read.bat` again, and
again it worked here.** So the two environments demonstrably differ, and **you are the
one that has been refused three times.** Try the one form once, and **the moment it is
refused, go to the file-editing tools and do not spend another call on it.** Task 5 says
what to write instead.

---

## Why this unit exists

**This is the fifth unit of the phase and the fourth that builds anything.**

**The count today.** Step 0 `done` in one unit. Step 1 `partial` after one unit. Step 2
`partial` after one unit. Step 3 `done` in one unit. **Steps 4, 5 and 6 are `not started`
and no unit has been spent on any of them.** Drift is 0 units without advance.
`outcome-read.bat` was run while authoring and confirms every one of those counts.

**Steps 5 and 6 are Tim's, at his radio. So step 4 is the last bench step in the phase,
and everything still unattended is inside it.**

**Step 4's entry is steps 1, 2 and 3, and all three are answered.** Step 1's remainder is
the **4.48 against 5.04** transmission figure, which is with Tim and which no unit may
settle - the decoder, the round trip and the zero wrong decodes are all met. Step 2's
remainder was **booked to step 4 by unit 291's arbiter on the record**: the five
on-screen sentences still naming fifteen seconds, the transmit guard, and the two
training-path copies all need a mode threaded to them, and **this is the unit that
threads it.** Step 3 is `done` on all four criteria.

**Step 4 is being taken in two units, and this is the first of them.** Its four criteria
split on a seam the tree put there rather than one the arbiter drew:

| | Criterion | This unit |
|---|---|---|
| 1 | pressing FT4 tunes to the band's FT4 frequency **and decodes**, through the same path the FT8 button uses | **yes** |
| 2 | the panel, conversation, ring, filters, tooltips, ledger and right-click menu all work unchanged, and the report names anything that did not | **yes** |
| 3 | one click, one transmission, through the same abort | **no - unit 293** |
| 4 | a whole exchange from one right click at the bench, transmit endpoint on a loopback | **no - unit 293** |

**Why the split, measured while authoring rather than assumed.** The receive half has its
seams already cut and unpressed. The transmit half **has no FT4 in it at all**: `grep FT4`
over `src/Hamlet.RadioEngine/Transmit/` and `.../Contacts/` returns nothing, `Ft8Composer`
composes FT8 alone, and `Ft8TransmitSequence`'s guard at `:497-530` measures against the
literals `Ft8Slots.SlotSeconds` and `Ft8Slots.TransmissionSeconds`. **Composing an FT4
waveform for the air and re-proving the abort through it is a unit's work on its own**,
and criterion 3 is one of the three things `PHASE_PLAN.md` says the arbiter may not reason
past. **A keying path done in the last hour of a long unit is exactly the thing that
ruling exists to prevent.** So the transmit half gets its own unit with room to do it
properly, and this one does not go near it.

**Expect step 4 to be read `partial` after tonight. That is the plan and not a
shortfall.** Two of four criteria met, with the other two named and booked, is what this
unit is for.

```
PHASE GOAL:   FT4 works exactly the way FT8 does.
UNIT GOAL:    Pressing FT4 on the Digital tab tunes to the band's FT4 frequency and
              decodes FT4 off the air, on a 7.5 s grid, through the same path the FT8
              button uses - and everything around it either still works or is named.
ADVANCES:     step 4, exit criteria 1 and 2. It also discharges step 2's remainder,
              which unit 291's arbiter booked here.
```

**Why the trace comes first, and it is sharper this time than a stale sentence.** The
Digital tab already keeps **two different facts about the mode apart on purpose**:
`DigitalModeChip.IsLit` is *the dial is inside this mode's block, and the map is what
answers*, and `IsChosen` is *this is the chip he pressed*. `DigitalModeChip.cs:70-78`
records that they disagree often and that **merging them would make a remembered press
look like a reading of the radio.** There is a third appearance for exactly that case,
`IsChosenElsewhere` at `:40`.

**So there is a wrong answer available to this unit that would look right.** Decoding a
slot as FT4, cutting it on a 7.5 s grid, or logging a contact as FT4 **because a chip is
lit** would be Hamlet asserting a measurement from a preference - and if it reaches the
log it is §0.0 in the one place `PHASE_PLAN.md` says outlives everything else. **Task 1
settles which fact drives the grid and the decoder before task 2 threads anything.**

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches. **Report
them; do not repair the instruction.** Unit 289 found four mismatches in its own
instruction, unit 290 found one understatement, and unit 291 found none in the
instruction but two elsewhere - **reporting them was worth more than a silent correction
would have been every time.**

**Every line below was read from the tree while authoring, at HEAD `9449d02`.**

- **Root version is `1.12.229`** (`Directory.Build.props:393`) and **`Ft8Sharp` is at
  `0.11.0`** (`src/Ft8Sharp/Directory.Build.props:438`). **Read them, do not assume** -
  the previous instruction said `1.12.223` and unit 291's six commits moved it.
- **`_digitalGrid` is `SlotGrid.Ft8` and nothing in the application changes it**
  (`MainWindowViewModel.cs:1661`). `DigitalGrid` at `:1675` is the reader;
  **`UseGridForTests` at `:1684` is the only writer and its own remark says *nothing in
  the application calls this*.** **This is the seam unit 290 cut and left unpressed, and
  its remark names step 4 by number.**
- **`SlotGrid` is a record struct at `src/Hamlet.RadioEngine/Audio/Ft8Slots.cs:137`**,
  with `SlotGrid.Ft8` at `:140` and **`SlotGrid.Ft4` at `:153` reading both its numbers
  from `Ft8Sharp.Ft4Timing`.** `Ft8SlotWatch.Grid` at `Ft8SlotWatch.cs:98` is an
  `init` property defaulting to `SlotGrid.Ft8`.
- **The chip press already tunes, and this is the part that is not new work.**
  `ChooseDigitalModeAsync` at `MainWindowViewModel.cs:1039` canonicalises the label,
  records the choice at `:1052` **whether or not the tune takes**, and calls
  `TuneToDigitalModeAsync` at `:1059`, which reads `DigitalCallingFrequencies.Find` at
  `:1062` and says why nothing moved where there is no row (`:1064-1081`).
- **The band data has five `FT4 sprint` rows** - `data/bands/us-neighborhoods.json` at
  `:155`, `:291`, `:578`, `:865` and `:1004`. **Count them yourself and say which bands
  they are**, because criterion 1 is *the band's FT4 frequency* and a band with no row is
  a legitimate refusal rather than a failure. `MainWindowViewModel.cs:1067` already
  records that the cited data has **no FT4 on 30 m or 17 m**.
- **The decode path is FT8-only by construction.** `OnSlotTick` at `:8928` arms nothing
  and returns early off the tab; `DecodeTheSlotAsync` at `:8979` calls **`Ft8Reader.Read`
  at `:8986`**, and `ShowDecodes` at `:8896` calls the same reader at `:8899`.
  **`Ft8Reader.Read` at `Ft8Reception.cs:421` takes an `Ft8DeepSlotDecoder?` and defaults
  it to Deep with both stages on at `:460`.**
- **The FT4 decoder exists and is the port's, not Deep's.**
  `src/Ft8Sharp/Dsp/Ft4SlotDecoder.cs:41`, with `Ft4WaterfallGeometry`,
  `Ft4SyncSearch` and `Ft4SoftSymbols` beside it, all built by unit 289.
  **There is no `Ft8Sharp.Deep` FT4 decoder of any kind.** `Ft4SlotDecoder.Decode` takes
  a span of samples at `:112` or a waterfall at `:117`, and exposes `CandidateLimit`,
  `MinimumScore`, `FirstBlockOffset` and `LastBlockOffset` at `:96-105`.
- **The sidecar records which decoder read a slot.** `Ft8DecoderIdentity` at
  `Ft8Reception.cs:292`, with `Port` = `"Ft8Sharp"` at `:301` and `Unrecorded` at `:298`,
  and its own remark at `:274-291` says a sheet that did not know **carries `Unrecorded`
  rather than naming a decoder it is guessing at.**
- **The one write path into the log hands in `ContactModes.Named("FT8")`** at
  `MainWindowViewModel.cs:10285-10299`, and the comment unit 291 left there says in its
  own words that **what makes that line say anything else is the Digital tab's mode
  wiring, which is step 4's.** `Ft8StationConditions.Mode` is a `ContactMode`, not a
  string, so **the mode threads as one object and `MODE`/`SUBMODE` cannot be set to
  disagree.**
- **The transmit guard is FT8's, in literals.** `Ft8TransmitSequence.cs:497-530` tests
  `send.StartSecondsIntoSlot >= Ft8Slots.SlotSeconds` and computes
  `left = Ft8Slots.SlotSeconds - send.StartSecondsIntoSlot`. **Read it, name it in the
  report, and change nothing in it. It is unit 293's.**
- **`DigitalModeChip.Labels` at `:60` carries the owner's four**, `Canonical` at `:111`
  drops a fifth, and `IsChosenElsewhere` at `:40` is the appearance for *pressed here,
  dial elsewhere*. `ChosenDigitalMode` is at `MainWindowViewModel.cs:322` and is read
  back out of settings at `:4335`.
- **`TheWholeChainRunsFromOneRightClickTests` is unit 268's and needs a real render
  endpoint**, and its own remark at `:45-48` says **a machine with no endpoint says so
  and stops, and that is a fact about the machine and not a failure.** **Do not run it;
  it is criterion 4 and it belongs to unit 293.** It is described here only so you know
  what is already built and do not rebuild it.

**Expected failures and expected awkwardness. None of these is a defect to chase.**

- **An FT4 decode cannot be compared with the port**, because `compareWithThePort`
  compares Deep against the port and **there is no Deep FT4.** If the settings flag is on
  and the mode is FT4, the honest answer is that no comparison ran. **Say so; do not
  invent one and do not silently drop the flag.**
- **A test that asserts the Digital tab is on a fifteen-second grid may go red for the
  right reason.** If one does, say which, say why, and **fix the test rather than the
  behaviour** - but if fixing it means changing what the screen *asserts to the operator*,
  stop and report it instead.
- **Four inherited red tests were found by unit 290** and are named in its section 4: two
  in `TheSheetSaysWhichAudioPathItRanOnTests`, `TheDecodedTableIsRealTests.
  NoInventedDecodeIsLeftInTheMarkup`, and `TheTabHearsEverySlotTests.
  AFullTableStillSaysNothingAboutWhatAMessageMeans`. **They are not on the known-reds list
  and the ruling on them is with Tim. Do not chase them and do not add to them.**
- **Three untracked leftovers are still in the tree** and neither unit 290 nor 291 could
  delete them - `.unit290-commit.txt`, `tools/census15.sh` and
  `tests/Ft8Sharp.Tests/Unit289SourceProbe.cs`. Task 5 says what to do.

Known reds, inherited, **never chased**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list tripwire;
`HM-OPEN-088`'s ten.

---

## Rulings in force

**Transcribed in full. Not to be re-argued by any unit, including this one.**

**Tim's, 2026-09-08:**

- **FT4 works exactly the way FT8 does.** **Anything FT8 does that FT4 does not is a gap
  to be named**, not a scope decision a unit may make. **This unit is where that ruling
  bites hardest**, because criterion 2 is a list of seven things FT8 does.
- **Where FT4's decoder lives is decided by what upstream does.** **Unit 288 read it and
  the condition is satisfied: `ft8_lib` carries FT4, so the port carries FT4 and the
  fidelity tests extend to cover it.** This question is closed. Do not reopen it.
- **The first contact in each mode is an achievement, and the six are CW, FT8, FT4,
  PSK31, WSPR and Voice.** Unit 287 built the card, unit 291 made the FT4 row light.
  **Do not re-argue the six.**

**Standing:**

- **`Ft8Sharp` remains a faithful MIT port.** `Ft8Sharp.Deep` is GPL-3.0. **Unit 289
  already put the FT4 decoder in the port. This unit calls it and does not change it.**
- **No algorithm comes from WSJT-X's source.** Published description only.
- **A wrong decode is counted separately from a missed one, everywhere.**
- **One click, one transmission.** Hamlet transmits because the operator clicked, **never
  on a timer, never on a decode, never to continue a contact.** **`PHASE_PLAN.md` warns
  that FT4's slots are half as long and the temptation is twice as strong.** Nothing in
  this unit transmits and nothing in this unit touches a keying path.
- **The abort.** Every path that keys the transmitter has a same-thread, no-await abort -
  CI-V `0x17` with `0xFF`, PTT off as the fallback. **Not this unit's; it is unit 293's,
  and it is why the transmit half was given its own unit.**
- **§0.0 and §12.1: what Hamlet asserts to the operator.** A picture that makes a claim
  nobody measured is the fault. **A remembered press is not a reading of the radio**
  (`DigitalModeChip.cs:70-78`, HM-DEC-092).
- **§0.2.1: no frequency written from memory.** The FT4 frequencies come from the cited
  band data, and a band with no row moves nothing and says so.
- **Every field is nullable and null means not observed.** A sheet that does not know
  which decoder ran says `Unrecorded` rather than naming one.
- **A unit may not add a test without naming the breakage it would have caught.**

---

## The questions that are with Tim, and how this unit stays clear of all four

**Do not settle any of them, and not from memory.**

1. **FT4's transmission: 4.48 seconds or 5.04?** Raised by 288, carried by 289, 290 and
   291. **`SlotGrid.Ft4` already reads both numbers from `Ft8Sharp.Ft4Timing` and the
   constant lives in one file**, so a ruling costs one edit. **Thread the grid; do not
   type either figure into a new place**, and **do not put a transmission length on the
   screen** where a slot length will do.
2. **The version scheme.** HM-DEC-150 against what `Directory.Build.props` has been
   doing. **With Tim since 288.** Bump as instructed below and do not resolve it.
3. **Unit 289's widened FT4 candidate sweep, -10 to 51 blocks.** With Tim for
   confirmation. **You will be calling the decoder that has it. Call it; do not retune
   it**, and if a decode misses, report the miss rather than widening anything.
4. **The four inherited reds unit 290 found.** **Do not chase them.**

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per `CLAUDE.md` -
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` **read from the clock, not composed**, and
`NOTE` saying what is moving inside the task. The same every ten minutes while a task is
running. **Use the file-editing tools if the shell refuses.**

*Unit 289 reported its intermediate `UPDATED:` stamps were composed rather than read and
ran an hour and a quarter fast. Read the clock.*

---

## Tasks

### Task 1 - the trace: which fact drives the mode, and where FT8 is welded in

**Reading and one test. Thread nothing in this task.**

**It runs first because the threading is mechanical and the choice underneath it is
not.** Criterion 2 is *the report names anything that did not work unchanged*, and a unit
that starts editing call sites produces a list of its own edits wearing a survey's
clothes - which is the fault unit 290 avoided by running its census first.

- **Answer the question task 1 exists for, in the report, before task 2:** **which fact
  drives the grid and the decoder - `IsChosen` or `IsLit`?** Read `DigitalModeChip.cs:30-94`
  and `:40` first. **State the answer, and state what happens in the `IsChosenElsewhere`
  case** - he pressed FT4 and the dial is in the FT8 block, or nowhere the map knows.
  **Whichever you choose, say what the tab does with the disagreement**, because both
  answers are defensible and only one of them can be on the screen.
- **Walk the press path with file and line**: `ChooseDigitalModeAsync` (`:1039`) →
  `ChosenDigitalMode` (`:1052`) → `TuneToDigitalModeAsync` (`:1059`) →
  `DigitalCallingFrequencies.Find` (`:1062`) → the read-back. **Say which bands have an
  FT4 row** and what the tab says on a band that does not.
- **Walk the decode path with file and line**: `OnSlotTick` (`:8928`) → `_slotWatch.Look`
  → `DecodeTheSlotAsync` (`:8979`) → `Ft8Reader.Read` (`Ft8Reception.cs:421`). **Name
  every place between the press and a decoded row that is FT8 by construction rather than
  by configuration**, and say for each whether it is a constant, a type or a default.
- **Count the fifteen-second assumptions still standing on the Digital tab**, against unit
  290's census. Unit 290 named five on-screen sentences, the transmit guard at
  `Ft8TransmitSequence.cs:497-530`, the field-guide row and two training-path copies.
  **The field-guide row was filled by unit 291, so that one is gone.** **Say what is left,
  with file and line, and say which of them this unit's threading reaches and which it
  does not.** This is the census half of criterion 2 and step 2's booked remainder in one
  reading.
- **Say what a slot decoded as FT4 must say about which decoder read it.** There is no
  Deep FT4 (`Ft8DecoderIdentity`, `Ft8Reception.cs:292-305`). **A sheet claiming
  `Ft8Sharp.Deep` read an FT4 slot would be naming a decoder that does not exist.**

**The test to construct:** one test asserting, against today's code, **the starting
position** - that with FT4 chosen the tab still cuts on a 15 s grid and still hands the
slot to the FT8 reader. **Rewritten in tasks 2 and 3 to assert the right answers.**
**The breakage it would have caught:** the FT4 button tuning the radio correctly and then
decoding nothing for the rest of the evening, with no sentence anywhere saying why - which
is what the tab does tonight.

**Report this walk in section 3 whatever else this unit achieves.**

### Task 2 - the grid follows the chosen mode

**Half of criterion 1, and all of step 2's booked remainder that this unit can reach.**

- **Give `_digitalGrid` a real route.** `UseGridForTests` (`:1684`) is the seam and its
  remark says the real route does not exist yet. **Build it from the fact task 1 named**,
  and **rewrite that remark to say what happened rather than leaving it predicting.**
- **The grid must reach the watch and the cutter, not just the two sentences.**
  `Ft8SlotWatch.Grid` (`Ft8SlotWatch.cs:98`) is an `init` property, and its own remark at
  `:86-98` says why changing grid mid-flight is the hazard - **read it before you change
  the grid at runtime**, and say in the report what happens to a slot in flight when the
  operator presses a different chip.
- **The two on-screen sentences must follow.** `DigitalModeStripLine` and
  `DigitalWaterfallSummary` already read `DigitalGrid` (`:943`, `:2672`). **Assert they
  say 7.5 with FT4 chosen and 15 with FT8 chosen**, and **assert neither states a
  transmission length**, because the 4.48 against 5.04 question is Tim's.
- **FT8 must be untouched.** **Assert that with FT8 chosen every boundary, every
  countdown and every sentence is what it was before this unit** - unit 290 pinned FT8 at
  3,888 tick-identical moments and that pin is your control. **If a tick moves, stop and
  report it.**
- **Tests run filtered by exact name, foregrounded, with a stated timeout. Report the
  counts.**

**The breakage this catches:** the ring counting down 7.5 s while the cutter still cuts on
15 s - two halves of the tab on different clocks, each of them internally consistent, and
nothing on screen able to say which is right.

### Task 3 - the decode runs FT4's decoder, and the sheet says so

**The other half of criterion 1.**

- **Route an FT4 slot to `Ft4SlotDecoder`.** `Ft8Reader.Read` (`Ft8Reception.cs:421`)
  takes an `Ft8DeepSlotDecoder?` and defaults to Deep; `Ft4SlotDecoder`
  (`src/Ft8Sharp/Dsp/Ft4SlotDecoder.cs:41`) is a different type in a different assembly
  with the same `Decode` shape. **The shape of the seam is yours** - a second reader
  method, a mode parameter, an interface, whatever the tree takes best. **Say in the
  report why you chose it**, and **prove FT8's route is unchanged rather than describing
  it as unchanged.**
- **Do not change `Ft8Sharp`.** Unit 289 built the FT4 decoder and nailed it to upstream
  symbol for symbol and sample for sample. **If you find you must change the port, that is
  a mismatch worth reporting before it is a version bump.**
- **The bench proof is Hamlet's own signal, and it is already available.** Unit 289 proved
  the round trip through `Ft8Sharp`. **What is unproved is the application path**: audio in
  at the tap, cut on a 7.5 s grid by the watch, read by the reader, arriving as a row.
  **Drive that path with an FT4 recording this unit makes, and assert the decoded text
  equals the text that went in.** **A test that calls `Ft4SlotDecoder` directly proves
  unit 289's work again and this unit's not at all.**
- **The sheet names the decoder honestly.** An FT4 slot was read by `Ft8Sharp`, **never by
  `Ft8Sharp.Deep`, which has no FT4 decoder.** `Ft8DecoderIdentity.Port` at `:301` is the
  value. **Assert it**, and **say what the sheet does with `compareWithThePort` turned on
  in FT4** - there is nothing to compare against and `Unrecorded` exists for exactly the
  case where nobody knows.
- **Zero wrong decodes, counted separately from missed ones.** Tim's standing ruling, and
  `PHASE_PLAN.md` repeats it. **Report both numbers even when one is zero.**
- **Tests run filtered by exact name, foregrounded, with a stated timeout. Report the
  counts.**

**The breakage this catches:** an FT4 slot handed to the FT8 decoder, which reads nothing
from it, and a tab that draws an empty table on a live band - indistinguishable on screen
from a quiet band or a wrong clock, which `MainWindowViewModel.cs:8884-8888` records as the
commonest newcomer failure in this mode.

### Task 4 - criterion 2: the seven things, each one measured

**Criterion 2 entire, and it is closed by naming rather than by fixing.**

`PHASE_PLAN.md`: *the panel, the conversation, the ring, the filters, the tooltips, the
ledger and the right-click menu all work unchanged, **and the report names anything that
did not***. **The criterion's own wording makes the report the deliverable.**

- **Take the seven in order and say for each: worked unchanged, worked with a change this
  unit made, or did not work - with file and line for anything in the last two
  categories.** Seven rows, no gaps. **A row you could not reach is `not reached` and says
  why**; it is not `worked`.
- **Under Tim's ruling, anything FT8 does that FT4 does not is a gap to be named, not a
  scope decision.** **So name every gap you find and fix none of them that is not already
  in tasks 2 and 3.** A gap named in this report is what unit 293 and step 6 are built
  from.
- **Two are already known to be gaps and are inherited from unit 291's walk**, which found
  them and deliberately left them: `ContactLogRow` (`ContactLogViewModel.cs:32`) maps a
  fixed column list with **no submode column**, and `LogContactViewModel.Fields` (`:72`,
  the `Mode` row at `:95`) is written out by hand with **no submode row**. **Both are in
  the ledger row of your seven.** They are the drop candidate in task 6 - **name them here
  whether or not task 6 runs.**
- **The right-click menu must be named against what it does today**, not against what it
  will do when FT4 can transmit. **A menu entry that composes a reply Hamlet cannot send
  is a gap, and it is unit 293's.** Say so; do not build it.
- **No new test is required by this task.** If you add one, **name the breakage it would
  have caught** - that rule has no exception.

**The breakage this catches:** step 4 being called done on a button that tunes and decodes
while the ledger silently drops the mode, the conversation cannot follow a 7.5 s exchange,
or the filters hide every FT4 row - each of them invisible from the decode table, and each
of them something Tim would find at the radio in step 5 with no note anywhere saying it was
known.

### Task 5 - bookkeeping

**File edits and two scripts. Do this even if task 6 is dropped, and do it before task 6.**

- **Append this unit's entry to `PHASE_OUTCOME.md`** through
  `tools/arbiter/outcome-append.bat`. **Try `./tools/arbiter/outcome-append.bat` once** -
  forward slashes, leading `./`, no `cd`, no apostrophes in the arguments. **The moment it
  is refused, stop trying.** Three units have now measured that refusal and a fourth
  measurement is worth nothing. Append with the file-editing tools in the format
  `outcome-entry.py` produces, **say so on the entry's own face** as 289, 290 and 291 did,
  and **commit the arguments the script would have been given** at
  `tools/arbiter/unit292-append.bat` so the entry can be replayed rather than
  reconstructed.
- **`PHASE_OUTCOME.md` carries two `STATE_AFTER` verdicts for each of units 289, 290 and
  291. Do not edit any of them.** The file's own rule is that the entries win. The
  arbiter's reading of where they disagree is in this instruction's opening and in the
  decision block, and it is the arbiter's to hold.
- **`PHASE_STATUS.md`'s `STEP:` lines, `CURRENT_STEP:` and `HEARTBEAT:`: do not write
  them.** They are the launcher's. Unit 291 reported them stale - `CURRENT_STEP:` reads
  `1` - and **reporting it again is correct; repairing it is not.** Set
  `WORK_INSTRUCTION:` only.
- **`RULES_AT`:** units 289, 290 and 291 all reported that `CPS-DEC-` appears nowhere in
  `CLAUDE.md`, `DECISIONS.md` or `PROJECT_STATUS.md`, that `HM-DEC-160` is this project's
  spelling, and that the three files already agree. **There is nothing to repair.** The
  reload still disagrees; **say it is the launcher's file and move on.**
- **`tools/arbiter/validate-output.bat`:** try it once, same form. If refused, **the route
  unit 243 built for exactly this deadlock is `dotnet build
  tools/arbiter/validate-output.proj`**, which runs the real validator unmodified - unit
  291 used it and got exit `0`. **Use that before you fall back to a hand check**, and if
  you do fall back, **say plainly that a hand check is not the same thing as the script
  exiting `0`.**
- **The three untracked leftovers.** `.unit290-commit.txt`, `tools/census15.sh` and
  `tests/Ft8Sharp.Tests/Unit289SourceProbe.cs`. **The third is a `.cs` file in a test
  project and a fresh clone does not have it**, so the tree you test is not the tree a
  clone builds - **three units old now and it is in section 4 as a standing item.**
  **Remove all three if your shell allows it. If it refuses, say so and leave them - do
  not commit them to make the warning go away.**

### Task 6 - the ledger shows the submode

**NAMED DROP CANDIDATE.**

Unit 291 found both of these, named them, and deliberately left them because no step 3
criterion touched them and a submode column would have shown an empty field on every FT8
contact. **After tasks 2 and 3 a contact can be decoded in a mode that needs one**, so the
argument has changed and this is where they land.

- **`ContactLogRow` (`ContactLogViewModel.cs:32`, columns at `:41` and `:64`)** maps a
  fixed list with a `Mode` column and no submode. An FT4 contact shows as `MFSK`.
- **`LogContactViewModel.Fields` (`:72`)** is written by hand, `new("Mode", observed.Mode
  ?? "", "MODE")` at `:95`, with no submode row. **The dialog shows the ADIF tag beside
  every value**, so the submode row must carry `SUBMODE` the way the mode row carries
  `MODE`.
- **Absent is absent, on screen as in the file.** An FT8 contact has no submode and
  **must not show an empty `SUBMODE` field or an empty column cell that reads as an
  observation**. `AdifLog.cs:8-13`: null means not observed. **Assert the FT8 case, not
  just the FT4 one.**

**If this unit is running long, drop this task whole and say it was dropped.** **No step 4
criterion depends on it** - criterion 1 is the tune and the decode, criterion 2 is the
naming, and **task 4 names both of these gaps whether or not this task fills them.**
**Do not drop it partly**: a log window with a submode column and a dialog without one is
worse than neither, because the operator learns to trust one screen and not the other.

**Criteria 1 and 2 closed with both gaps named is a good night. A submode column and a
decode path half-threaded is not.**

---

## Parked - do not touch, do not raise

- **Everything that keys the transmitter.** `Ft8TransmitSequence`, `Ft8ArmedSend`,
  `Ft8Composer`, `ITransmitAudioSink`, the abort, the PTT, the CI-V `0x17`. **Criteria 3
  and 4, unit 293.** Read the guard at `Ft8TransmitSequence.cs:497-530` and **name it in
  task 1's census; change nothing in it.**
- **`TheWholeChainRunsFromOneRightClickTests` and `TheLoopbackThroughTheApplicationsSendPathTests`.**
  **Do not run them and do not extend them.** They need a real render endpoint and they
  are criterion 4's, which is unit 293's.
- **Composing FT4 audio for the air.** There is no `Ft4Composer` and this unit does not
  write one. **The FT4 waveform for a decode test is a test fixture, not a transmission** -
  if you find yourself near `ComposeSignal` or a sink, you have wandered.
- **The 4.48 against 5.04 figure**, **the version scheme**, **the -10 to 51 candidate
  sweep**, and **the four inherited reds unit 290 found.** All four with Tim.
- **`Ft8Sharp` and `Ft8Sharp.Deep` source.** Unit 289 built the FT4 decoder. **Call it.**
- **The two field-guide frequencies unit 291 found disagreeing with the convention data** -
  RTTY at 7.062 against 7.040, PSK31 at 7.065 against 7.070. **Needs a citation and a
  ruling, and it is in section 4 already. Do not adjudicate it from memory.**
- **The frequency table's missing 30 m and 17 m FT4 rows.** Needs a citation. **A band
  with no row moves nothing and says so, and that is the correct behaviour.**
- **PSK31 and WSPR as modes.** Their chips are on the strip and neither has a path.
  **Nothing in this unit gives either one.**
- **The achievements card.** Unit 291 finished it. The FT4 row still reads *waiting on
  Hamlet* and **that stays true until unit 293** - Hamlet still cannot work a station on
  FT4. **Do not light it early.**
- **Automatic sequencing.** Still out, and FT4's shorter slots are the argument for it,
  which is why it stays a ruling rather than a temptation.
- **The whole asks queue** carried since unit 271.

---

## What not to do

- **Do not transmit, arm a transmission, or touch a keying path.** Unit 293's, and one of
  the three things `PHASE_PLAN.md` says the arbiter may not reason past.
- **Do not let a countdown, a decode or a slot boundary cause anything to be sent.** One
  click, one transmission. **FT4's slots are half as long and the temptation is twice as
  strong** - `PHASE_PLAN.md` says so in those words.
- **Do not drive the grid or the decoder from `IsLit` without saying you did and why.**
  A remembered press is not a reading of the radio, and the reverse is a trap too.
- **Do not write a frequency from memory.** §0.2.1. The band data is cited and five rows
  carry FT4.
- **Do not name `Ft8Sharp.Deep` as the decoder of an FT4 slot.** It has none.
- **Do not put a transmission length on the screen.** 4.48 against 5.04 is Tim's, and a
  sentence stating either would answer it.
- **Do not change `Ft8Sharp`.** If you must, report it before you bump it.
- **Do not change what an FT8 press does.** Not a boundary, not a tick, not a byte of a
  record. If one moves, **stop and report it.**
- **Do not fix the gaps task 4 names.** Naming is the criterion. Fixing the wrong one
  spends the night.
- **Do not settle any of the four questions with Tim.**
- **Do not write `PHASE_STATUS.md`'s `STEP:` lines, `CURRENT_STEP:` or `HEARTBEAT:`.**
- **Do not edit units 289's, 290's or 291's `PHASE_OUTCOME.md` entries.**
- **Do not commit the three untracked leftovers** to make a warning go away.
- **Do not run a test suite.** Filtered by exact name only.
- **Do not background a command and poll for it.**
- **Do not spend more than one call on a refused `.bat`.**
- **Do not ship a placeholder token in a reported number.**

---

## Committing and pushing

Commit and push each task before starting the next. **Bump the root version's patch by
one** if anything was committed - the scheme question is with Tim and is not yours.

**`Ft8Sharp` does not move this time.** Unit 289 built the FT4 decoder and this unit calls
it. **If you find you must change it, that is a mismatch worth reporting** before it is a
version bump.

---

## Reporting

`output.md` at the repository root, overwritten, four sections per `CLAUDE_CODE.md` §8.

**The ordering block comes first, before the header.** `validate-output.bat` refuses a
report without it.

```
A. The phase goal - FT4 works exactly the way FT8 does - and where every step stands:
   step 0 done; step 1 partial, its remainder the 4.48 against 5.04 question with Tim;
   step 2 partial, its remainder booked here; step 3 done; step 4 <state after this
   unit>; steps 5 and 6 not started and Tim's. Say whether this unit discharged step
   2's remainder or only part of it, and name what is left of it.

B. Step 4 and its four exit criteria, each named with met, not met, or booked to unit
   293:
   1. pressing FT4 tunes to the band's FT4 frequency and decodes, through the same
      path the FT8 button uses                                          must-pass
   2. the panel, conversation, ring, filters, tooltips, ledger and right-click menu
      all work unchanged, and the report names anything that did not    must-pass
   3. one click, one transmission, through the same abort               must-pass
   4. a whole exchange runs from one right click at the bench, with the transmit
      endpoint on a loopback                                            must-pass
   Criteria 3 and 4 were booked to unit 293 by the arbiter before this unit ran.
   Say so plainly rather than reporting them as failures, and say whether anything
   this unit found makes either of them harder or easier than the arbiter assumed.

C. This report's own findings, weighed against A and B. Say how many items section 4
   raises, and for each, whether it is in the way of a criterion in B or merely beside
   it. Say how many gaps task 4 named, and whether any of them is in the way of
   criterion 1 or of unit 293. If task 6 was dropped, say so here and say that no
   criterion in B depended on it, with both ledger gaps named and left standing.
```

Then the six-line header:

```
UNIT:       292 - <complete at task n of 6 | what was dropped> - <date time>
PHASE GOAL: FT4 works exactly the way FT8 does.
UNIT GOAL:  <one or two lines>
ADVANCED:   <yes or no, and which of step 4's criteria moved>
NUMBER:     how many places between the press and a decoded row were FT8 by
            construction, and how many gaps task 4 named. Two numbers, and the
            second stated even when it is zero.
DRIFT:      <n> consecutive units without advance
```

**Section 3 leads with four things, in this order:**

1. **The trace** - which fact drives the mode and what happens when the chip and the dial
   disagree; the press path and the decode path with file and line; **every place that was
   FT8 by construction, each marked constant, type or default**; and what is left of unit
   290's fifteen-second census. **This leads because the choice underneath the threading is
   the only part of this unit that could be wrong in a way the tests would not show.**
2. **An FT4 slot decoded through the application**, not through `Ft4SlotDecoder` directly -
   the text in, the grid it was cut on, the text out, **the decoder the sheet names**, and
   the wrong count beside the missed count.
3. **The seven things of criterion 2**, one row each: worked unchanged, worked with a
   change named here, did not work, or not reached and why. **No gaps in the table.**
4. **What FT8 did before and after**, showing that a press of FT8 moves the same
   boundaries, ticks the same countdown and decodes through the same reader it did at HEAD
   `9449d02`.

**Section 2 says what this means for the phase**: whether Tim pressing FT4 at his radio
would now see decoded text - **which is step 5's first criterion, so say plainly whether
step 5 is reachable after tonight or still waiting on unit 293** - and **what unit 293
inherits**, in particular anything task 1 or task 4 found about the transmit half that the
arbiter did not know when it split the step.

Write `output.md`, then stop.

---

```
ARBITER-DECISION
STEP: 4
APPROACH: thread the chosen digital mode through DigitalGrid and the slot decoder so pressing FT4 tunes and decodes FT4 through the same path the FT8 button uses, taking criteria 1 and 2 and booking the transmit half to the next unit
MOVE: continue
WHY: Step 4 is the last bench step and its entry - steps 1, 2 and 3 - is answered, with step 2's remainder already booked here by unit 291's arbiter. The loop test was run and found no resembling approach in any entry; step 4 has zero units spent, so this cannot be a loop. I scoped it to the receive half because the transmit half has no FT4 in it at all - grep over src/Hamlet.RadioEngine/Transmit and Contacts returns nothing, Ft8Composer is FT8-only and the guard at Ft8TransmitSequence.cs:497-530 measures against Ft8Slots literals - and criterion 3 is a keying path, which PHASE_PLAN.md names as one of the three things the arbiter may not reason past.
STATE: not started
DECIDED: Two things on my own authority, both about scope rather than about any of the four questions with Tim. First, I split step 4 across two units on a seam the tree already carries: criteria 1 and 2 are the receive half and their seams are cut and unpressed - _digitalGrid at MainWindowViewModel.cs:1661 with UseGridForTests as its only writer, and Ft4SlotDecoder built and proved by unit 289 - while criteria 3 and 4 need an FT4 composer that does not exist and a re-proof of the abort. I did NOT cut criteria 3 and 4 down or declare them unachievable; they are booked to unit 293 with their reason recorded, and I expect step 4 to read partial after tonight. Second, I ruled that criterion 2 is closed by naming rather than by fixing, which is what its own wording says - the report names anything that did not work unchanged - and that makes the ledger submode work a droppable task rather than a criterion. I did NOT settle the 4.48 against 5.04 figure, the version scheme, the widened candidate sweep or the four inherited reds; this unit is designed so none of the four can block it, and the grid it threads reads both timing numbers from Ft8Sharp.Ft4Timing so a ruling still costs one edit.
LICENCE: PHASE_PLAN.md step 4, whose entry is steps 1, 2 and 3 and all three are answered, together with its steps-are-a-hypothesis clause permitting the arbiter to take a step in the order and the portions the evidence supports, and its named-alternatives table - the tree disagrees with this plan, the tree wins, report the mismatch and continue. The step 1 and step 2 readings are ARBITER.md section 8, which makes STATE_AFTER evidence rather than verdict and leaves the arbiter to judge where two readings disagree.
ACCOMPLISHED: The FT4 button on the Digital tab stops being a button that does nothing. Pressing it takes the radio to the band's cited FT4 frequency, cuts the band into 7.5 second slots, and reads FT4 off the air onto the table - the same three things pressing FT8 does - and everything around it that does not yet follow is named with file and line rather than left for Tim to find at the radio.
ADVANCES: step 4, exit criteria 1 and 2 - the tune and the decode through the same path in tasks 2 and 3, and the seven-part census that closes criterion 2 in task 4. It also discharges step 2's remainder, which unit 291's arbiter booked to step 4 on the record, and it clears the last bench blocker in front of step 5, which is Tim's.
END-ARBITER-DECISION
```

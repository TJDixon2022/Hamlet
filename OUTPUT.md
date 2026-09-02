READ IN THIS ORDER

A. The phase goal. Hamlet hears FT8 off the radio and displays the decoded text
   on screen. Steps 1 to 5 are done; step 6 is blocked on two owner-class items
   and nothing tonight touched it; step 7 is partial and is the only step this
   unit works.

B. This step and its exit criteria. Step 7 - Hamlet displays decoded FT8. Four
   must-pass criteria: audio arrives in fifteen-second slots aligned to the
   quarter minute, asserted against synthesized audio and a controllable clock;
   the clock offset is measured and shown; decodes render on screen; and the
   standing pair - Ft8Sharp green, attribution and the channel tests. Then the
   arbiter halts and hands Tim a bench check. This unit MET criterion 1, which
   had nothing built against it, and MOVED criterion 3 from one press to every
   slot. It did not touch criterion 2, which unit 038 already met. Attribution
   cannot read clean and what replaced it is task 5. This unit does not declare
   step 7 closed.

C. This report. What it adds, weighed against A and B: slots decoded with
   nobody pressing anything went from 0 to 5, and the four that carried a
   transmission put CQ K1ABC FN42, CQ W9XYZ EM48, CQ VE7AA CN89 and CQ EA3QQ
   JN11 on the Digital tab's own table. Section 4 raises 4 items. None of them
   stands in the way of a criterion in B - three are recorded mismatches
   between this instruction and the tree, and the fourth is a judgment this
   unit made and named rather than a blockage.

UNIT: 225 - the Digital tab hears every slot, with nobody pressing anything
PHASE GOAL: Hamlet hears FT8 off the radio and displays the decoded text on screen
UNIT GOAL: Every completed fifteen-second slot decodes on its own and its messages appear on the Digital tab, with nobody pressing anything
ADVANCED: yes - step 7 criterion 1 met, criterion 3 extended from one press to every slot
NUMBER: slots decoded with nobody pressing anything, 0 -> 5
DRIFT: none - no ruling was re-argued, no threshold moved, no library file changed

---

## 1. What Claude did

Six tasks, **all six run and none dropped** - including task 4, which was the
named drop candidate. The window was not thin and the machinery task 3 built
made task 4 four lines, so it was measured rather than sacrificed.

### Task 1 - the ground, and four questions answered from the code

**Ft8Sharp at entry: 518 total / 517 passed / 0 failed / 1 skipped**, read from
the TRX `Counters` element and not from a console line. Unit 224's figure
exactly. The one skip is the table-write gate, skipped in every unit since 213.
5 m 14 s.

**Attribution from `2828ab6` at entry: 207 paths, 8 of them under `src/Hamlet.*`
or `tests/Hamlet.*`.** Named individually in task 5 below.

**The channel tests, the seven this instruction names, all green:** `Hamlet.App.Tests`
`DecisionLogOrderTests`, `DecisionEmissionTests`, `VersionTests` - **10 of 10**;
`Hamlet.RadioEngine.Tests` `AudioSeamTests`, `PrivilegeTests`,
`TheSlotCutterTests`, `TheClockIsMeasuredNotCorrectedTests` - **71 of 71**.

Then the four questions, from the code:

**1. Can a caller pull an exact past window out of `AudioTap`? Yes, and there is
no mapping from a moment to a sample index - one had to be built.**
`Window(long firstSample, int count)` addresses by the audio clock and copies
under the tap's own lock. `SamplesSeen` is monotonic: it is a `long` written only
by `+=` inside `Take`, under `_lock`, and never reset - `Forget()` clears the ring
and leaves the counter alone, deliberately. It is **read outside the lock**, which
is safe here rather than by accident: a naturally-aligned 64-bit read cannot tear
on this runtime, and there is no barrier, so a reader can see a slightly stale
value - and a stale, *smaller* value can only make `Window` refuse. It cannot
produce the wrong audio.

**But there is no defined mapping from a wall-clock instant to a sample index
anywhere in the tree.** The tap holds samples, a rate and no timestamps. One had
to be constructed, and it is the single most load-bearing thing in this unit; it
is set out in task 2.

**2. What happens when the requested window has fallen out of the ring? It
returns null.** Explicitly: `if (firstSample < oldest || firstSample + count >
SamplesSeen) return null;`. It does not throw and it does not return short, which
is what makes the watch's refusal honest rather than a policy layered on top.

**3. Which timer should carry a slot tick? `_decodeTimer`, and no new one is
wanted.** It runs at 250 ms while the decoder is listening, which is sixty looks
inside every fifteen-second slot, and it already holds the tap the watch needs.
`_clockTimer` at ten minutes is far too slow. A third timer would be another
thing to start, stop and dispose, and it would have to be kept in step with
`_decodeTimer` anyway. **And the tick rate does not decide how many decodes
happen**, because the watch de-duplicates - which is the property that made the
choice free.

**4. What breaks if `DigitalDecodes` grows? One member, and it is the one the
instruction named.** The full list, checked rather than assumed:

| member | assumed one press' worth? | what happened |
|---|---|---|
| `DigitalDecodedSummary` | **yes** - reads `DigitalDecodes[0].Utc` | **fixed**: reads `[^1]`, the most recent |
| `HasDigitalDecodes` | no - `Count > 0` | untouched |
| `DigitalModeStripLine` | no - reads `_digitalDecodeNote` | untouched; the note now describes the last slot rather than the last press |
| `_digitalDecodeNote` | no | untouched |
| `MainWindow.axaml` `Summary="{Binding DigitalDecodedSummary}"` | via the above | fixed by fixing the above |
| `MainWindow.axaml` `ItemsSource="{Binding DigitalDecodes}"` | **yes, in a way nobody had written down** | see below |
| `MainWindow.axaml` two `IsVisible` bindings on `HasDigitalDecodes` | no | untouched |

The `ItemsControl` finding is what set the bound. It sits inside a `ScrollViewer`
with unbounded height and **does not virtualise**, so every row is five live
`TextBlock`s whether or not it is on screen. The cost that bites first is layout,
not bytes.

**No premise of this instruction was already false.** Nothing in the tree watched
the clock and cut slots; the alignment existed and the trigger did not, exactly as
the instruction says.

### Task 2 - the slot watch, in the engine

`src/Hamlet.RadioEngine/Audio/Ft8SlotWatch.cs`. **12 engine tests, all green, 1 s.**

**It is a function of its arguments and reads no clock.** There is no
`DateTime.UtcNow` in the file. That is what makes criterion 1 assertable at all,
and it is why the watch is in the engine rather than inside a `DispatcherTimer`
handler - a timer callback cannot be driven across a boundary by a test.

**The mapping, and the hazard building it exposed.** The only anchor available is
that the newest sample the tap holds arrived at about this moment; everything
before it is counted back at the sample rate. That is the same assumption
`Ft8SlotCutter` has made about a capture press since work instruction 042, stated
explicitly rather than left implicit.

**It has exactly one failure mode and it is the §0.0 fault in its worst form, and
the instruction does not name it.** If the audio stream stalls, `SamplesSeen`
stands still while the clock runs on, and the last fifteen seconds in the ring
would be handed over wearing the current slot's timestamp - **a row that looks
exactly like a real decode and is not**. HM-DEC-090 caught the same shape once
already, where a stalled pipeline let a capture hand over the same thirty seconds
three times and the analysis beside it read as three measurements. So the watch
keeps an anchor - a sample index and the moment it was current - and refuses when
the audio has fallen more than a slot behind the clock. A sound card differing
from the PC by a hundred parts per million takes over forty hours to drift that
far, and every slot that comes back re-anchors. **The refusal is watched firing.**

**Thirty seconds turns out to be exactly enough, and it is written down.** A
boundary is never more than fifteen seconds back, so the slot wanted spans at most
thirty seconds back from now - `AudioTap.SecondsKept` to the second. A full ring
and a stream keeping up therefore always hold the completed slot. **Shorten the tap
and the watch starts missing slots.**

The five cases the instruction asks for, each asserted, plus four more:

| case | result |
|---|---|
| a look in the middle of a slot | nothing, over all 56 looks inside one slot |
| a look just past a boundary | **exactly the slot that ended**, 180 000 samples = 15.000 s, decoding to `CQ K1ABC FN42` |
| the same look repeated | nothing the second time, and nothing the third |
| a look after the audio has aged out | **`AudioAgedOut` and no short buffer** - reachable at start-up, when the ring has not filled |
| an offset that changes between looks | no duplicate and no gap across a two-second swing |
| the first look ever | **arms and claims nothing**, deliberately |
| a look several slots late | takes the last, counts 5 skipped, never walks back |
| a stalled stream | **`AudioStalled`, and it re-arms** |
| an unmeasured clock | `Ft8SlotCutter.NoOffset`, unchanged |
| a stale offset | the offset's own `Describe` words |

**No decoding is in it.** `Ft8Reader.Read` is untouched.

### Task 3 - the tab decodes with nobody pressing anything

**7 new app tests, and unit 224's 8 pass unedited. 15 green.**

The measurement, read back off the view model's own collection through a real
`AudioTap`, a real `Ft8SlotWatch` and the real reader, is in section 3.

What was decided, and where the reason lives in the code:

- **Rows append.** Continuous decoding is a session and a session accumulates.
- **Bounded at 500**, and the number is the markup's rather than memory's, for the
  `ItemsControl` reason above.
- **Nothing appears twice.** The key is the slot start, the frequency and the text,
  and keys fall off with the rows they belong to.
- **The retune rule, chosen and stated: clear beyond 3 kHz**, which is the
  receiver's own audio passband. Inside it the same transmissions are still
  arriving through the same filter, so a 500 Hz nudge keeps the session; outside it
  the rows are about a different piece of spectrum. Both directions asserted.
- **A refusal outranks the row count** on the panel summary and the mode strip,
  because a full table with an unmeasured clock is the state that reads most like
  a working session and is not one.
- **Off the Digital tab, one boolean**, and the watch re-arms.
- **The decode runs off the UI thread**, in the manner `QueryTheClockAsync` already
  uses: tens of milliseconds of signal processing on the dispatcher would stop the
  waterfall dead four times a minute.
- `DigitalIdleText.Decoded` still shows when nothing has decoded; the em dash
  stands in every `snr` cell; nothing new is said about what a message means.

### Task 4 - the named drop candidate, RUN and not dropped

`ShowDecodes` is now one line into `NoteSlot`. The press contributes its slots to
the same table through the same de-duplication instead of clearing it. **Measured
both ways:** a press over a slot the watch had already read leaves the session at
four rows and adds no duplicate; a press over a slot the watch never saw still
adds its row. Unit 224's eight tests pass unedited - the press's summary, its
strip line and its unmeasured-clock refusal all read exactly as they did.

### Task 5 - the evidence, and task 6 - the record

Both in full below and in section 3. `porting-notes.md` gains a unit-225 section;
`OPEN_ISSUES.md` has HM-OPEN-068 untouched and HM-OPEN-069 updated.

### The branch, and the push

Branch **`main`**. Four commits, **every one pushed successfully** to
`origin/main` as it was made: `cd30e5e` (task 2), `235c014` (task 3), `8f80784`
(task 4), `771754a` (tasks 5 and 6), and a fifth carrying this report. **No push
was refused.**

### The validator would not run, and this is a hand check

**`tools\arbiter\validate-output.bat` could not be executed in this session.** The
permission layer refused it directly, through `cmd /c` and through `bash`, which
is what unit 224 reported. So the six rules were read out of the script's own body
and **checked by hand** with `grep`, `sed` and `od`:

```
  rule 1  UNIT: line at 27, above "## 1." at 36, inside the first 60 lines   ok
  rule 2  the four "## " lines are 36, 233, 280, 410, in order, exact names  ok
  rule 3  there is no fifth "## " line anywhere in the file                  ok
  rule 4  line 410 is byte-for-byte "## 4. What's blocking us", ASCII quote  ok
  rule 5  section 3 runs 281 to 409 and is far from empty                    ok
  rule 6  "READ IN THIS ORDER" at 1, A. at 3, B. at 8, C. at 19, and
          "Section 4 raises 4 items" at 22 - all inside the first 60         ok
```

**That is a hand check and it is not claimed as an exit 0.** The rules were
transcribed into `tools/unit225-handcheck.py` so a session that can run a script
does not have to read the batch file again; **it was not executed either**, and it
says so in its own header. If it ever disagrees with the batch file, the batch file
is right.

## 2. What the owner should expect

### At the radio tomorrow morning

**Tune to 14.074, pick the Digital tab, and leave it.** That is the whole
procedure. There is no button to press any more and the *keep the last 30 seconds*
button is not the way to decode - it is for keeping diagnostic material.

**What should happen.** Within a slot or two the decoded table starts filling and
keeps filling, one block of rows every fifteen seconds. Callsigns accumulate down
the panel the way an FT8 operator actually watches a band. The collapsed header
reads the most recent slot's time and how many rows are showing.

**Give it three slots before deciding.** The first look after the tab opens arms
and claims nothing, and the first slot after that may be refused because the audio
ring had not filled. That is deliberate and it is one slot, not a fault.

### What will look wrong and is not

- **The `snr` column is a row of em dashes.** This decoder produces a Costas sync
  score and no decibels anywhere. A number under that heading would be read as a
  measurement. **HM-OPEN-068 is yours and is not decided here.**
- **The plain-English panel says nothing.** It carries the idle line you wrote in
  August and nothing else. What a message *means* is yours under §12.1; unit 224
  took three invented cards out and this unit put nothing back.
- **Changing band empties the table.** Deliberate: rows from 7.074 sitting under
  the same heading as rows from 14.074 would assert that all those stations were
  heard here. A small nudge - anything under 3 kHz - keeps the session.
- **Switching to the CW tab and back empties nothing, but there will be a gap.**
  The watch stops looking off the Digital tab and re-arms when it comes back, so
  the slot that closed while you were away is not claimed. The rows already on the
  table stay.
- **The table stops at 500 rows** and the oldest fall off the top.
- **If the clock has not been checked, the panel header says so in words** instead
  of showing an empty table. That sentence, not a blank screen, is the thing to
  read: it is the commonest newcomer failure in this mode and it looks exactly
  like a dead band.
- **The `dt` column reading about 1.4 on synthesized signals is the synthesizer's
  own padding**, not a clock error. Real signals will read whatever they read.

### What has not been shown and is not claimed

**Nothing in this unit has heard a radio.** Every figure here is over synthesized
audio through Hamlet's own ring buffer at Hamlet's own rates. The bench check at
14.074 is yours and no unit closes it. **This unit does not declare step 7
closed.**

## 3. What you should see

### Decoded text appeared on the Digital tab with nobody pressing anything, over five consecutive slots

**That is the question this unit was commissioned to ask, and the answer is yes.**
The clock was driven forward across five quarter-minute boundaries over
synthesized audio, through a real `AudioTap`, a real `Ft8SlotWatch` and the real
reader, and the rows were read back off the view model's own collection. Nothing
was pressed. What the table said:

```
  5 consecutive slots decoded, nobody pressing anything

  utc      snr   dt    hz     message
  142215    —    1.4   1241   CQ K1ABC FN42
  142230    —    1.4   1241   CQ W9XYZ EM48
  142245    —    1.4   1241   CQ VE7AA CN89
  142300    —    1.4   1241   CQ EA3QQ JN11

  summary [142300 UTC · 4 shown]
  strip   [one message out of one slot]
```

Five slots closed and four carried a transmission; the fifth, 14:23:15, was empty
band by construction and produced no row, which is the correct answer and not a
miss. **Every message came back as itself, under the quarter minute it was sent
in, and every `snr` cell is an em dash.**

The same five slots measured one layer down, in the engine, with the watch and the
reader and no view model at all: **5 slots, 4 messages, same text, same
timestamps.**

### Task 5 - the evidence step 7's fifth criterion now needs

**1. Attribution from `2828ab6`: 210 paths, and 11 are under Hamlet.** Named
individually, as the instruction requires:

```
  src/Hamlet.App/ViewModels/DigitalDecodeRow.cs
  src/Hamlet.App/ViewModels/MainWindowViewModel.cs
  src/Hamlet.App/Views/MainWindow.axaml
  src/Hamlet.RadioEngine/Audio/Ft8Reception.cs
  src/Hamlet.RadioEngine/Audio/Ft8Resample.cs
  src/Hamlet.RadioEngine/Audio/Ft8SlotWatch.cs          <- this unit
  src/Hamlet.RadioEngine/Hamlet.RadioEngine.csproj
  tests/Hamlet.App.Tests/ViewModels/TheDecodedTableIsRealTests.cs
  tests/Hamlet.App.Tests/ViewModels/TheTabHearsEverySlotTests.cs   <- this unit
  tests/Hamlet.RadioEngine.Tests/Audio/TheDigitalTabDecodesWhatItKeptTests.cs
  tests/Hamlet.RadioEngine.Tests/Audio/TheSlotWatchTests.cs        <- this unit
```

**The plan's reduction does not apply, and this is not a laundering.** The plan's
own words are that if a Hamlet path appears the unit says so and the reduction
does not apply. **Step 7 is by construction the step that reaches Hamlet** - its
Delivers line is *audio from Hamlet into the decoder* - so the criterion as
literally worded can never read clean again from here to the end of the phase.
What replaces it is items 2 and 3.

**This unit's own diff, `f9abec7..HEAD`, is six files** and shows what it actually
touched: `PHASE_STATUS.md`, `PROJECT_STATUS.md`, `MainWindowViewModel.cs`,
`Ft8SlotWatch.cs`, and the two new test files, plus the task 5/6 commit's
`Directory.Build.props`, `OPEN_ISSUES.md` and `porting-notes.md`. **Zero code
files under `src/Ft8Sharp/`**, confirmed by `git diff --name-only f9abec7..HEAD --
src/Ft8Sharp` returning nothing before the notes were written.

**2. The channel tests, all seven by name, re-run after the version bump:**

```
  Hamlet.App.Tests
    DecisionLogOrderTests, DecisionEmissionTests, VersionTests   10 of 10, 64 ms
  Hamlet.RadioEngine.Tests
    AudioSeamTests, PrivilegeTests, TheSlotCutterTests,
    TheClockIsMeasuredNotCorrectedTests                          71 of 71, 66 ms
```

**3. The tests over the code this unit changed, by named class:**

```
  Hamlet.App.Tests
    TheDecodedTableIsRealTests, TheTabHearsEverySlotTests,
    TheTabsAndTheWorkspacesTests, EveryResourceKeyResolvesTests  24 of 24, 3 s
  Hamlet.RadioEngine.Tests
    TheDigitalTabDecodesWhatItKeptTests, TheSlotWatchTests       19 of 19, 1 s
```

**The failing set is named rather than counted, and it is empty.** No test in any
named class failed, and no test that passed at entry is absent at exit: unit 224's
eight in `TheDecodedTableIsRealTests` and its four theory rows plus three facts in
`TheDigitalTabDecodesWhatItKeptTests` all ran and all passed, unedited.

**The two inherited CW reds were not run and are not claimed.**
`WhereTheTrackerStartsDoesNotDecideThis` and `AStationElsewhereIsStillFound` are
in `Hamlet.RadioEngine.Tests.Cw`, outside every filter above, and are not this
phase's.

**4. Ft8Sharp, entry against exit:**

```
  entry   518 total / 517 passed / 0 failed / 1 skipped   5 m 14 s
  exit    518 total / 517 passed / 0 failed / 1 skipped   5 m 01 s
```

Both from the TRX `Counters` element and not a console line. The one skip is the
table-write gate. **Unmoved, as it should be: this unit changed no library code
file.**

### The versions

Root **1.12.31 -> 1.12.32**, a patch under HM-DEC-150, with the reason written
into `Directory.Build.props` beside the bump. `Ft8Sharp` stays at **0.10.7** under
HM-DEC-152, because no library code file changed - only `porting-notes.md`, which
is the one permitted exception.

### Task 6 - what the next session inherits

`porting-notes.md` gains **"The continuous path - unit 225"**: what the watch is,
the clock contract and why it forces the engine rather than the shell, the
moment-to-sample-index mapping and the stall hazard it exposed, why thirty seconds
is exactly enough and what breaks if the tap is shortened, the five guarantees
with their reasons, the bound and the retune rule, and **five things a session
picking this up must not assume** - starting with *do not assume the watch
decodes* and *do not assume `SamplesSeen` is a timestamp*.

`OPEN_ISSUES.md`: **HM-OPEN-068 untouched and still Tim's.** HM-OPEN-069 updated -
**the hang did not recur and the `OutputPath` workaround held** across eight runs
in both Hamlet test projects and `Ft8Sharp`, no build was blocked, and the `~Views`
filter was never used. That is evidence that naming classes avoids the hang, not
that the hang is gone, and the entry says so; it also names the first place to
look and says plainly that it was not tested.

## 4. What's blocking us

**Nothing blocks a criterion in B.** Four items, three of them mismatches between
this instruction and the tree, reported per the instruction's own requirement that
mismatches go in the report even where the work succeeded anyway.

### 1. MISMATCH - the instruction says every row carries `SlotStartUtc`. It does not.

Task 3 says *Every row already carries `SlotStartUtc`*. `DigitalDecodeRow` carries
**`Utc`, a formatted `HHmmss` string**; it is `Ft8Decode` that carries
`SlotStartUtc` as a `DateTime`. The distinction matters and it changed the work:
`HHmmss` repeats every twenty-four hours, so a de-duplication key built from the
row would eventually collapse two different days into one. **The key is built from
the `Ft8Decode` instead**, using the round-trip `o` format. Reported and not
repaired in the instruction.

### 2. MISMATCH - two different channel-test lists are in force, and they disagree.

This instruction names **seven** classes. `src/Ft8Sharp/porting-notes.md` records,
at unit 222 and as a repair to a reproducibility gap unit 221 raised, a **fifteen**
class set across two filter strings, defined by a rule that can be rebuilt: *a
channel test is a test class that opens one of the three shared artifacts at run
time*. Four of this instruction's seven - `DecisionEmissionTests`,
`AudioSeamTests`, `PrivilegeTests`, `TheClockIsMeasuredNotCorrectedTests` - are not
in that recorded set. **I ran this instruction's seven**, all green, and I am
naming the disagreement rather than picking a winner. Whoever reconciles them
should know the recorded set has a stated rule behind it and this one does not.

### 3. CONFIRMED - the ruling unit 224 cited is not in the plan, and I could not find it either.

The arbiter's named check holds. `PHASE_PLAN.md` contains no 2026-09-02 ruling that
steps 6 and 7 depend on step 5 and not on each other; its step 7 section ends
`Depends on: step 6.` A repository-wide `git grep` finds the phrase only in
`PHASE_OUTCOME.md`, which is unit 224's own outcome entry, and in
`WORK_INSTRUCTIONS.md`, which is this instruction quoting it. **I looked and found
nowhere else.** Nothing in this unit rests on it.

### 4. A JUDGMENT THIS UNIT MADE, named rather than hidden: the stale-offset sentence is not the cutter's.

Task 2 says an unknown *or stale* offset returns *the cutter's own refusal
sentence, unchanged*. **`Ft8SlotCutter` has no stale sentence** - it has
`NoOffset`, which says the offset has not been measured, and that is not true of a
stale one. So: an unknown offset returns `Ft8SlotCutter.NoOffset` exactly and
unchanged, and a **stale** one returns `Ft8SlotWatch.StaleOffset` followed by
`ClockOffset.Describe`'s own words, which report the real age. That reuses
`ClockOffset` rather than writing a second opinion about the clock, which is what
the ruling in force asks for. **If the arbiter meant `NoOffset` in both cases, this
is one constant to change and the test names the sentence it asserts.**

### Parked and not re-raised

Step 6's shortfall and HM-OPEN-067; the `snr` column's contents; the plain-English
panel; the two inherited CW reds; the loose files at the repository root;
`PHASE_STATUS.md`'s `CURRENT_STEP: 6` and `PROJECT_STATUS.md`'s `RULES_AT`
mismatch. All confirmed still as the instruction describes them, none touched, none
of them blocking anything here.

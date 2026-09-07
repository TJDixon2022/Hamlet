READ IN THIS ORDER

A. THE PHASE GOAL. **Hamlet works stations on the air** — Tim answers a CQ on
14.074 or 7.074 from Hamlet and completes an exchange. Where every step stands
after tonight: **0, A, B and C were all `done` before this unit began** — 0 and A
by unit 266, B by unit 267, C by unit 268, whose clicked string decoded back off a
real sound card as `"W1ABC KC3QIS RRR"`. **D is left `in progress`, which is what
it can honestly be and no more.** E is `not started`. **All three of step D's exit
criteria are Tim's, at his own radio, and this unit closed none of them** — it
could not have; no unit can say what an IC-7300's ALC does at a given level,
because that number is not in this repository.

B. THIS STEP AND ITS EXIT CRITERIA. **Step D, *the drive level his radio wants*,**
and its three criteria:

1. *Tim sets the Transmit drive control and reads the dBFS and clip count under
   the waterfall.* **Half of this is a bench thing and it is this unit's whole
   subject.** Nobody had built it: the drive control was behind a `ShowDialog`
   modal covering the waterfall, the decode table and the always-pressable Stop
   button, and no dBFS or clip count appeared under the waterfall at all. **The
   bench half is now built and measured on runs tonight. The shack half — him,
   setting it — is still his**, and the criterion is not met.
2. *His radio's ALC behaviour at that level, in his words.* **Only he can meet it.
   Untouched tonight.**
3. *The value recorded in `SHACK_FACTS.md`.* **Only he can meet it. Untouched
   tonight — `SHACK_FACTS.md` was not opened for writing.**

C. WHAT THIS REPORT ADDS, AND WHETHER IT BEARS ON A OR B. It adds the answer to
the question this unit was commissioned to ask: **what it costs Tim to change the
drive between two fifteen-second slots, and whether the number he reads back is a
measurement or his own setting.** Both are answered on runs, not on reading.
**Section 4 raises 5 items. None of them is in the way of a criterion in B, and
none asks the owner to decide anything** — one is a shell refusal already worked
around by the plan's named alternative, one is a mistake of mine corrected in
place, two are record disagreements the instruction told me to report and not
repair, and one is a bench gap named for step E and deliberately not built.
**Task 3 did NOT take its fallback**: the route to the sink's own figures exists
and the keying path was not touched, so the readout is a measurement and says so.

---

```
UNIT:       269 — complete at task 6 of 6 — 2026-09-07 12:02
PHASE GOAL: Hamlet works stations on the air — Tim answers a CQ on 14.074 or
            7.074 and completes an exchange from Hamlet.
UNIT GOAL:  The Transmit drive control and the level readout are under the
            waterfall on the Digital tab, where step D says Tim reads them — one
            setting through one validator, the dBFS visible as he moves the
            control and before anything is sent, and after a send a figure that
            says which number it is and what its clip count counts.
ADVANCED:   no — no criterion of step D closed, and none could be: all three are
            Tim's at his own radio. What moved is the blocker under criterion 1 —
            the control is out of the modal dialog and under the waterfall, and
            the dBFS and clip count are there beside it, with the measured one
            read off the sink instead of off the setting he typed.
NUMBER:     1 window and 4 controls -> 0 windows and 1 control (what it costs to
            change the drive between two slots, with the band, the decode table
            and the Stop button hidden before and nothing hidden after)
DRIFT:      1 consecutive unit without advance (was 0) — and what was built in
            that unit is the bench half of step D's first criterion: a drive
            control and two named level figures under the waterfall, one of them
            the first measurement of what left the machine that any Hamlet screen
            has ever shown.
```

## 1. What Claude did

**Exit state: complete, at task 6 of 6.** All six tasks done, committed and
pushed, and **the named drop candidate was not dropped.**

Development machine, `C:\Source\HamLet`, project claimed and confirmed *Hamlet*
(all four identity checks pass), branch `main`, nine commits from `016411c` to
`7860c94`. Version `1.12.122` → `1.12.123`; `Ft8Sharp` untouched. `SHACK_FACTS.md`
FACT-004 throughout: **no serial port was opened, no render endpoint was opened,
nothing was keyed and no sound was made.** No figure below says anything about the
IC-7300.

### The trace, task 1, its own commit before task 2 began

`docs/unit269-the-level-trace.md`, six questions, each with a file, a line and a
quotation. **Its finding decided the shape of the whole night.**

**Question 3 — is there a route to the sink's own figures that leaves the keying
path untouched? Yes, and it is not the route unit 265 looked for.** Unit 265 was
right that there is none through what `Ft8TransmitSequence` returns: `PlayedAudio`
is `(int SamplesPlayed, TimeSpan Took)` and carries no level. **But the route does
not go through the sequence at all.** The application constructs the sink itself —
`MainWindowViewModel.cs:8026`, `sink = TransmitSinkFactory(endpoint)`, a **local**
that is read once for its rate and then dropped — so it can keep the reference and
read the two figures off it after the boundary has already returned, with nothing
keyed. **So task 3's fallback was written in advance, and was not needed.**

**Question 1 — what it costs him today.** `ShowDialog` confirmed at
`MainWindowViewModel.cs:4059`, over the same `AppSettings` instance the main window
holds (`:4057`). The waterfall (`MainWindow.axaml:3025`), the decode table
(`:3177`) and `DigitalStopButton` (`:3119`) are all on the window it is shown over.
**And the slot boundary keeps being driven while it is up**, from the code and not
from a guess: `_decodeTimer` is a 250 ms `DispatcherTimer` (`:3272` as it then
stood), `OnDecodeTick` calls `OnSlotTick`, `OnSlotTick`'s first line is
`DriveTheArmedSend()`, and the `await` at `:4059` yields to that same dispatcher.
A transmission he armed before opening the dialog still goes out behind it, and he
cannot see it go or reach Stop.

**Question 5 — the settings file.** Not a hazard, and the answer is *do nothing
extra*: `TheOperatorsFolderIsNotOursTests.cs:42-55` is a `[ModuleInitializer]` that
repoints `SettingsStore.DataFolder` at `%TEMP%\hamlet-app-tests-<pid>` before any
test in the assembly runs, so the committed drive tests already write a temp
`settings.json`. **No test of mine rewrites the operator's file.**

**Question 6 is "none"**, as the instruction expected: `grep -i drive` over
`MainWindow.axaml` returns one prose *"driver"* in a comment at `:2280` and nothing
else; `grep -i dbfs` returns nothing at all; and `WaterfallGain` at `:468` is the
receive display control and is not it.

### Task 2 — the control under the waterfall, watched red first

**The red was committed before it was made green** (`d4c6775`), and it was the
product's state and not the harness's:

> `TheControlUnderTheWaterfallOpensShowingTheLevelInForce` [FAIL, 728 ms] — *there
> is no NumericUpDown called "DigitalTransmitDriveBox" on the realized window.*
>
> `TheLevelInDbfsIsOnScreenBeforeAnythingIsSent` [FAIL, 586 ms] — *there is no
> TextBlock called "DigitalTransmitDriveNote" on the realized window.*

**Green now, all five, each run alone by exact name**, on a real `MainWindow` shown
headless on the Digital tab at 1400×1400 — and the control is found as a **visual
descendant of `DigitalSendReserved`**, so *on the window somewhere* would not pass.
`DigitalStopButton` is asserted in the same test to be visible, effectively enabled
and inside the window's own bounds, because a new control in the Send area that
pushed it off the screen would have broken the first of the three things no unit
may reason past. It is placed **below** the button row for exactly that reason.

**Sharing was possible and was taken rather than copying.**
`src/Hamlet.App/ViewModels/TransmitDrive.cs` now holds the percent-to-peak
conversion, the `Ft8Composer.DriveIsUsable` question and the note sentence, once;
`SettingsViewModel` calls it and **its committed strings are byte-identical**,
which is what the four committed `SettingsCarriesTheTransmitDriveTests` prove — all
four pass, each run alone by exact name.

The third assertion — a level the composer would refuse is not written — goes
through the view model and **not** through the spinner, deliberately: the spinner's
own 1..100 ends are the same two ends the Settings spinner carries, so a refusal
tested through it would agree by accident even if the refusal were missing. The
expected sentence is **read off `Ft8Composer`** in the test rather than written out
again.

### Task 3 — the readout, watched red first

Red at `9a16b82`: *there is no TextBlock called "DigitalTransmitLevelText" on the
realized window.* **Green now, all four, each run alone by exact name.**

**Exactly which files changed, and the point is what did not:**

| Changed | What |
|---|---|
| `src/Hamlet.RadioEngine/Transmit/ITransmitLevelReport.cs` | new; two properties, no methods |
| `src/Hamlet.RadioEngine/Audio/WasapiTransmitSink.cs` | the declaration line and two doc comments; **both properties untouched** |
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` | one field, one assignment beside the existing local in `BuildTheArmedSend`, the readout property, and one line where the boundary **has already returned** |
| `src/Hamlet.App/Views/MainWindow.axaml` | one `TextBlock` |
| `tests/Hamlet.App.Tests/FakeTransmitParts.cs` | the fake's two reported figures |

**Not changed, not one line: `Ft8TransmitSequence.RunAsync`, the key, the sink
call, the `finally`, the abort, the stop, `Ft8ArmedSend`, and
`ITransmitAudioSink.PlayAsync`.** That last one is asserted on the type in the
tests, both ways: the real sink implements the report, and the interface the
sequence talks through does **not**.

The one-click rule is asserted against the new reference, because a reference held
is a second thing that could reach the sink: second boundary `NothingArmed`, **1
sink call before and after, 2 frames on the wire before and after.**

`AfterASendTheLineSaysWhatLevelItWentOutAt` **did not have to change and was not
changed.**

### Task 4 — what the clip count can actually count

Measured, not reasoned, at the ceiling and no higher. **No clip was added and the
composer's ceiling was not raised.** See section 3.

### Tasks 5 and 6

Step D recorded `in progress` in `PHASE_OUTCOME.md`, appended by hand after the
script was refused for the thirteenth consecutive unit. `docs/unit269-what-step-e-asks.md`
written — the drop candidate, not dropped, building nothing.

### Decisions I made for myself, reproduced in full

1. **The drive control appears on the Digital tab *as well as* in Settings rather
   than moving out of it**, because both write one `AppSettings.TransmitDrivePeak`
   through one `Ft8Composer.DriveIsUsable`, and a control removed from Settings
   would break unit 265's committed suite for no gain. The arithmetic that would
   otherwise be copied went into one new file instead.
2. **The readout is required to name which quantity it shows**, and where a sink
   offers no report the line says there is no measurement rather than falling back
   to the composed peak in a measurement's words — because the honest failure of
   this unit is a screen that shows the operator his own setting back and calls it
   a measurement.
3. **Step D is recorded `in progress` and not `partial`**, because none of its
   three criteria is met and `partial` would claim one that is Tim's.
4. **Each of the two test files landed its red without the one assertion that
   could only be written against a type or property that did not exist yet**, and
   both of those assertions arrived with the green, said so at the site, and are
   green. The reason is narrow: a file that does not compile takes the whole test
   assembly down with it, including the seven committed tests this unit was
   permitted to run. **This is a sizing decision I made and it is reported as
   one.**

### Everything run tonight

**Fourteen distinct tests, every one filtered by exact name and foregrounded. No
suite, nothing unfiltered, nothing backgrounded.** The nine this unit built, and
the seven named committed ones from the instruction's bounded exception — all
seven pass, each run alone. `Hamlet.RadioEngine.Tests` was not run at all. None of
the inherited reds was run or chased.

## 2. What the owner should expect

**Unlike the last three units, something on your screen changes.**

**On the Digital tab, in the Send area under the waterfall, below the CQ and Stop
buttons, there are now three things that were not there this morning:**

- a **Transmit drive** spinner, in percent of full scale, 1 to 100;
- the line under it saying what that works out to — *How hard Hamlet drives the
  radio's input — **-12.0 dBFS** at this setting. This is a starting point, not a
  specification…* — which **moves as you move the spinner and is there before you
  transmit anything**;
- a second line below that, which after each send says **what the sound card was
  actually handed**, and before any send says *Nothing has been transmitted yet, so
  there is no measured level.*

**Nothing was taken away from Settings.** `TransmitDriveBox` is exactly where it
was, under the Transmit endpoint picker. **Both controls write the same one
setting**, so it does not matter which you use and they cannot disagree — change
it on the tab and the Settings screen shows the new value; change it in Settings
and the tab picks it up when the dialog closes.

**The Stop button did not move**, and a test asserts it is visible, enabled and
inside the window on every run.

**What will look wrong but is not:** there are now **two dBFS figures and two clip
counts** on that panel after a send, and they may show the same number. That is
deliberate. One is what Hamlet *built* — your own setting read back — and one is
what the sound card was *handed*. On an ordinary evening they agree; the point is
that when they do not, you can see it. Each sentence says which it is.

**And one of the two clip counts cannot move**, which is also deliberate and is now
said on the screen: the count in the Send area line is over the audio Hamlet
builds, and Hamlet will not build a sample outside the scale at any drive it
accepts. **"nothing clipped" there is arithmetic and is not evidence that your
drive is safe.** The count that can move is the measured one under the drive
control.

**No level was chosen for you and `SHACK_FACTS.md` was not touched.** The default
is still 25 %, which is unit 265's -12.04 dBFS, and nothing tonight is a
recommendation about your IC-7300 — this machine has never had a radio on it.

`docs/unit268-what-step-d-asks-tim.md`, the page you follow at the rig, has been
**corrected rather than rewritten**: same structure, same ALC procedure, same
`SHACK_FACTS.md` list, with sections 1 and 2 now matching the tree.

## 3. What you should see

### 1. What he does at the rig now, against what he had to do this morning

**This morning, between two fifteen-second slots:**

> Press **Settings**, which opens a modal window **over the waterfall, over the
> decode table and over the Stop button** → find the Transmit section → move
> `TransmitDriveBox` → close the window → right-click a station → wait a slot →
> read a sentence in the Send area → **press Settings again.**

**Slot boundaries keep arriving the whole time that dialog is up.** A transmission
already armed goes out behind it and he cannot see it go or reach Stop.

**Tonight:**

> Move the spinner under the waterfall. Read the line under it.

|  | This morning | Tonight |
|---|---|---|
| **Windows to open** | **1**, modal | **0** |
| **Controls to cross** | **4** | **1** |
| Hidden while he does it | the waterfall, the decode table, **and the Stop button** | nothing |

**That is the night's evidence.** The control is asserted to be a visual descendant
of `DigitalSendReserved`, the area directly beneath `DigitalWaterfallPanel`, on a
realized window — not merely present in markup.

### 2. The readout after one clicked send, as it appears on screen

One `SendCallToAnyoneCommand` click taken through `AtSlotBoundaryAsync` on
`FakePort` and a substituted sink factory, on a real window, run tonight. **The
fake reported a peak of 0.5 against a composed 0.25, so a readout showing the
setting could not have passed.** Word for word:

> **The sound card was handed -6.0 dBFS** — that is the peak the endpoint actually
> got, measured on the way out after clamping, and not the level Hamlet composed
> at. **4 samples had to be clamped on the way out.** Beyond this point are
> Windows' own volume for that device and the radio's input gain, which Hamlet
> cannot see.

and above it, in the Send area, unchanged from unit 265 except for one added
clause:

> Sent "CQ KC3QIS FN00" in the slot at 15:52:45 UTC. **It was composed at -12.0
> dBFS with nothing clipped** — that is the level Hamlet built… The clipped count
> here is of the audio Hamlet built, and the composer will not build above full
> scale, so on this path it is always none. What the sound card actually had to
> clamp is the measured line under the drive control.

**Which quantity each number is.** `-6.0 dBFS` is `ITransmitLevelReport.PeakWritten`
— the largest magnitude the sink actually wrote to the endpoint, measured after
clamping. `4 samples` is `ClippedSamples`, the sink's own clamp count. `-12.0 dBFS`
is `Ft8Transmission.PeakSample`, the peak of the array the composer produced, which
is the drive setting read back. `nothing clipped` is a count over that same
composed array.

**It came from the sink, not from the fallback.** The fallback was written in
advance and was not taken. What made that possible: the report is a separate
two-property interface that `ITransmitAudioSink` does not carry, read off the sink
the view model already built, after the boundary returned, with nothing keyed.

And before anything is sent, which nothing in Hamlet could show this morning:

> How hard Hamlet drives the radio's input — **-12.0 dBFS** at this setting. This
> is a starting point, not a specification. Set it against your own radio's ALC
> meter: turn it up until the ALC just begins to move and then back off…

with the same sentence reading **-8.0 dBFS** the moment the spinner is moved to
40 %.

### 3. The clip count measurement

**Drive 1.0 — the highest `Ft8Composer.DriveIsUsable` accepts, asserted as the
ceiling in the same test — composed through the application's own `ComposeSignal`
route at 48000 Hz:**

```
drive              : 1.000000  (0.00 dBFS), the ceiling
rate               : 48000 Hz
samples composed   : 606720
largest magnitude  : 1.000000
outside [-1, +1]   : 0
```

**The number is zero, and it is zero by construction.** The composer multiplies a
unit-amplitude sine by the drive (`Ft8Composer.cs:390`) and refuses a drive above
full scale, so no drive it accepts can put a sample outside the rails. **No clip
was added, the ceiling was not raised, and none of this is a recommendation about
a drive level** — the ceiling is the worst case for the question, not a level
anybody should set.

**Can the resampler overshoot? There is no resampler.** The one `ComposeSignal`
call site in `src/` composes at the endpoint's own declared rate (unit 262), so
nothing between the composer and the card can overshoot a composed peak there.

**What that changes for what the page tells him to watch.** It would have told him
to watch a number that cannot move, and a zero he read as *my drive is safe* would
have been the display being more confident than its input justified. **Both the
screen line and the page now say what each count is of**, and the page says
plainly: *Do not treat "nothing clipped" there as evidence that your level is
safe. It would say that at any drive Hamlet allows.* The count that can move is
the sink's, under the drive control.

## 4. What's blocking us

**Nothing is blocking. Five items, none in the way of a criterion, and none asking
for a ruling.**

### 1. `outcome-append.bat` refused again — the thirteenth consecutive unit

**No ruling wanted.** The plan's named alternative exists and was taken.

Both invocation forms were refused, verbatim: `"This command requires approval"`
for `tools\arbiter\outcome-append.bat`, and the same words for
`cmd //c tools\\arbiter\\outcome-append.bat`. The unit 269 entry was appended with
the file-editing tools in the format the existing entries use, with an
`APPENDED_BY:` line saying on its face that a script did not write it, and the
header's `STEP: D` line was moved in place, which is what the script does.

**A field-count mismatch, reported not repaired:** the instruction asks for
fourteen fields; `tools/arbiter/outcome-entry.py:115-118`'s own `FIELDS` list has
twelve. Twelve are written, plus `APPENDED_BY`.

**Seven shell refusals in total tonight**, recorded verbatim:
`"This Bash command contains multiple operations. The following part requires approval: ls ~/.nuget/packages/avalonia/ ; ls ~/.nuget/packages/"`;
`"Contains simple_expansion"`;
`"This Bash command contains multiple operations. The following part requires approval: grep -vE \"^\s*$\""`;
`"This Bash command contains multiple operations. The following part requires approval: git rm -q --cached .commit-msg.txt .oa-267.bat"`;
`"This command requires approval"` (three times — the bare `git rm`, and both
`outcome-append.bat` forms). **The file-editing tools were unaffected throughout,
the thirteenth consecutive unit to say so, and nothing halted the loop.** One
consequence worth naming: the NuGet package folder is outside the working
directory, so Avalonia's own `ShowDialog` source could not be quoted; the trace
says what it could establish from the tree alone and says which part it could not.

### 2. My own mistake, corrected in place

**No ruling wanted.** A `git add -A` at task 2 tracked `.commit-msg.txt` and
`.oa-267.bat`, which task 5 names explicitly as *report, do not repair*. **That was
a repair I was told not to make.** Both were removed from the index again in their
own commit (`9510527`) and left on disk, which is the state the instruction
describes and the state a fresh clone is in. The launcher's `.run-unit/` files,
`SESSION.lock` and `RUN_LEDGER.md` were already tracked and have ridden unit
commits since before unit 259; those were left as they were.

### 3. The record disagreements the instruction asked me to report — and one is in the instruction

**No ruling wanted; reported and not repaired, as told.**

`PROJECT_STATUS.md`'s `RULES_AT` reads `HM-DEC-158 (2026-09-07)`. `CLAUDE.md`
section 1's newest row is **`HM-DEC-152`, dated 2026-08-31**. **The instruction
names that row `CPS-DEC-0152`; no such id appears anywhere in this repository**, so
the disagreement is six rulings and nine days rather than a spelling. `HM-DEC-158`
is the phase re-cut, recorded in `PROJECT_CARD.md` by unit 266 and never added to
the decision log's index.

`.commit-msg.txt` and `.oa-267.bat` are untracked at the root — confirmed, and see
item 2.

### 4. A bench gap named for step E, and deliberately not built

**No ruling wanted.** From task 6, and it is unit 264's finding restated for a man
at a radio rather than for a test.

**A row's `Contact` cell is computed once and never recomputed.** `PlaceRow`
(`MainWindowViewModel.cs:7826`) sets it at `:7840` and has exactly two callers —
the decoder's door at `:7787` and `AddDecodeRowForTests` at `:7872`. `RecordSent`
at `:8474` books the send into the ledger and touches no row already on the table.
So **the row he right-clicked does not change when his own transmission goes out**;
the next row from that station does, fifteen seconds later. Inside a running
exchange that is invisible. **After his last message, with the other station gone
quiet, it is not** — the newest row for that callsign may be one placed before the
send, so the cell can read *your move* while the ledger already says complete.

**Named, not built.** It is a display refresh touching `PlaceRow`'s contract and
the table's identity, and it is not step E's to fix on the evening step E runs.
What step E should do is expect it and read the row that arrives **after** his last
transmission rather than the one he clicked.

### 5. The inherited reds, untouched and not chased

**No ruling wanted.** `CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW
cases in `docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list
tripwire; and
`TheSinkPlaysToANamedEndpointTests.ACancelledPlayGoesOutShortAndTheSequenceCallsItAudioFailed`,
which unit 265 recorded and units 266, 267 and 268 have left alone. **None of them
was run tonight** — running any would have been a test outside this unit's named,
bounded exception.

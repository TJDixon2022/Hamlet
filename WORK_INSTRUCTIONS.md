# Work instruction 269 - the drive control and the level readout where Tim reads them

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

---

## THE TWO RULES THAT KILLED SESSIONS

**Tim's rulings of 2026-09-05. Not this unit's to weigh.**

**1. A unit runs no test suite.** **A unit may run only the unit test it
constructs in that work instruction**, filtered by exact name, in the foreground,
with a stated timeout of a few minutes. **An unfiltered `dotnet test` on any
project is forbidden.**

**2. Never background a command and poll for it.** Sessions were killed by the
watchdog on 2026-09-05 and 2026-09-06, each sitting in
`until grep -q "exited with code" ...; do sleep 15; done` with a `900000` ms
timeout. **The watchdog fires after twelve minutes with no status write.**

`dotnet build` is allowed, foregrounded, with a timeout.

**THIS UNIT'S NAMED, BOUNDED EXCEPTION TO RULE 1 - seven committed tests, each
run alone by exact name.** They exist because this unit puts a second control
over a setting one control already writes, and a second readout beside a sentence
that already reports a level. **These seven are the ones it can break, and a unit
that breaks them without knowing has undone unit 265:**

```
SettingsCarriesTheTransmitDriveTests.TheBoxShowsTheLevelTheSettingsFileHolds
SettingsCarriesTheTransmitDriveTests.ChangingTheDriveWritesItIntoTheSettings
SettingsCarriesTheTransmitDriveTests.ALevelTheComposerWouldRefuseIsNotWrittenToTheSettings
SettingsCarriesTheTransmitDriveTests.TheNoteSaysItIsAStartingPointToBeSetAgainstTheRadiosAlc
TheApplicationSendsAtTheLevelTheOperatorSetTests.BothSendRoutesTransmitAtTheDriveLevelInSettings
TheApplicationSendsAtTheLevelTheOperatorSetTests.AnOperatorWhoSetsNothingStillDoesNotTransmitAtFullScale
TheApplicationSendsAtTheLevelTheOperatorSetTests.AfterASendTheLineSaysWhatLevelItWentOutAt
```

**It widens nothing else**: no suite, nothing unfiltered, nothing backgrounded,
and no test outside those seven and what this unit builds.

**This unit opens no audio device and plays no sound**, so nothing here runs for
the length of a slot. **State a timeout of five minutes on the test runs and five
on the builds** and foreground them.

---

## THE TOOL RULE

**This session's shell may refuse calls.** A refused shell call is a signal to
reach for the other tool, not to stop. **The file-editing tools have been
unaffected throughout** - twelve consecutive units have said so, and units 267 and
268 both appended their outcome entry by hand after `outcome-append.bat` was
refused in both spellings. Record every refusal verbatim. **Nothing in this unit
halts the loop.**

---

## Why this unit exists

**Four steps have closed, one unit each, and the drift counter is at 0.** Unit 266
closed step 0 and step A, unit 267 closed step B, unit 268 closed step C - all
four criteria on runs, the clicked string decoded back off a real card as
`"W1ABC KC3QIS RRR"`. **Steps D and E are all that remain and both are Tim's, at
the radio.** No unit can answer what his radio's ALC does at a given level; that
number is not in this repository.

**But step D's first criterion is not only his.** It reads:

> *Tim sets the Transmit drive control and reads the dBFS and clip count under the
> waterfall.*

**Half of that sentence is a thing the bench builds and nobody has built it.**
Unit 268's own section 4 reported the mismatch and did not repair it, as it was
told: there is no readout under the waterfall, and the figures live in a sentence
in the Send area after a send. **What that report did not name is the harder
half.** The Transmit drive control is in `SettingsWindow`, and
`MainWindowViewModel.OpenSettingsAsync` shows it with `ShowDialog` - **a modal
dialog, over the waterfall, over the decode table, and over the Stop button.**

**So step D as the tree stands asks Tim to do this, at his rig, between fifteen
second slots**: open a modal dialog that hides the band, move a slider, close it,
right-click a station, wait a slot, read a sentence in the Send area, and open the
dialog again. **With his radio's ALC needle as the thing he is actually watching.**

**And the number he would read back is the number he just set.** `LevelLine` at
`MainWindowViewModel.cs:8396` reports `Ft8Transmission.PeakSample` - the level
Hamlet *composed* at, which is the drive setting - and counts clipping over those
same composed samples, which the composer built inside the rails. **The sink
measured what actually went to the card and unit 265 recorded that every reader of
those two figures in the repository is a test.**

**This unit builds the bench half of step D's first criterion so that his evening
can close it.** It closes no criterion of step D - all three are his - and it must
not claim one.

```
PHASE GOAL: Hamlet works stations on the air.
UNIT GOAL:  The Transmit drive control and the level readout are under the
            waterfall on the Digital tab, where step D says Tim reads them - one
            setting written through one validator, the dBFS shown as he moves the
            control and before anything is sent, and after a send a figure that
            says which number it is and what its clip count counts.
ADVANCES:   None of step D's three exit criteria - all three are Tim's at the
            radio and this unit closes none of them. It clears the blocker under
            criterion 1: the control is behind a modal dialog that covers the
            waterfall, the decode table and the Stop button, and the dBFS and
            clip count appear nowhere under the waterfall at all.
```

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
**Report them; do not repair the instruction.**

Every line below was read off the tree while this instruction was written, on
2026-09-07, at `HEAD` `672df09`. **Line numbers move. Read them; do not assume
them. Where one is wrong, say so in section 4 and carry on with the rest.**

**The control, today:**

- `src/Hamlet.App/Views/SettingsWindow.axaml:262` - `x:Name="TransmitDriveBox"`,
  `Value="{Binding TransmitDrivePercent}"` at `:265`, the note bound at `:272`.
- `src/Hamlet.App/ViewModels/SettingsViewModel.cs:326` - `_transmitDrivePercent`;
  `OnTransmitDrivePercentChanged` at `:328` converts percent to peak at `:330`,
  **asks `Ft8Composer.DriveIsUsable` at `:336` and writes nothing when it says
  no**, writes `_settings.TransmitDrivePeak` at `:343` and calls
  `SettingsStore.Save` at `:344`. `TransmitDriveNote` at `:357` is the dBFS and
  the ALC sentence.
- `src/Hamlet.App/Settings/AppSettings.cs:252` - `TransmitDrivePeak`, defaulting
  to `Ft8Composer.DefaultDrivePeak`.
- `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:4055-4059` - the settings
  window is constructed with `new SettingsViewModel(_settings, _telemetry)` over
  **the same `AppSettings` instance** and shown with
  `await window.ShowDialog(desktop.MainWindow)`. **This is the modal claim. Check
  it.**
- `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:8192` - the send composes with
  `_settings.TransmitDrivePeak`.

**The readout, today:**

- `MainWindowViewModel.cs:8396` - `LevelLine`, reading `transmission.PeakSample`
  at `:8398` and counting samples outside `[-1, 1]` at `:8401-8407`. Its own
  remarks at `:8381-8390` say why it is not the sink's `PeakWritten`: **"there is
  no route to it from what `Ft8TransmitSequence` returns ... and building one
  would mean changing the keying path".**
- `MainWindowViewModel.cs:8357-8358` - `LevelLine` is appended to the `Sent ...`
  sentence and reaches the screen as `DigitalSendLine` (`:8104`).
- `src/Hamlet.App/Views/MainWindow.axaml:3129` -
  `x:Name="DigitalSendReservedLine"`, `Text="{Binding DigitalSendLine}"`, inside
  the Send area. The Digital tab's waterfall panel is at `:3025`
  (`DigitalWaterfallPanel`) with the control at `:3050`. **`grep -i drive` over
  `MainWindow.axaml` returns nothing about a transmit drive. Confirm that.**
- `MainWindow.axaml` - `x:Name="DigitalStopButton"`, the always-pressable Stop,
  is in the same Send area with the comment saying it is never hidden or made
  conditional.

**The figures the card actually saw:**

- `src/Hamlet.RadioEngine/Audio/WasapiTransmitSink.cs:228` - `PeakWritten`;
  `:238` - `ClippedSamples`; both assigned at `:407-408` in the PCM16 conversion.
- `src/Hamlet.RadioEngine/Transmit/ITransmitAudioSink.cs:59` - the interface, and
  **it carries neither**: `EndpointSampleRate` at `:81` and `PlayAsync` at `:92`
  returning `PlayedAudio`, which is
  `readonly record struct PlayedAudio(int SamplesPlayed, TimeSpan Took)` at `:15`.
- `MainWindowViewModel.cs:8026` - `sink = TransmitSinkFactory(endpoint)`, a
  **local**; `:8067` hands it to `new Ft8TransmitSequence(port, sink, ...)` inside
  a new `Ft8ArmedSend`, and **the view model keeps no reference to it.** The
  factory itself is `internal Func<string, ITransmitAudioSink> TransmitSinkFactory`
  at `:7963`, defaulting to `new WasapiTransmitSink(name)` at `:7964`.

**What is on the bench to build with:**

- `tests/Hamlet.App.Tests/FakeTransmitParts.cs:17` - `FakePort`; `:74` -
  `FakeSink : ITransmitAudioSink`, `EndpointSampleRate` at `:109`, `PlayAsync` at
  `:154`.
- `tests/Hamlet.App.Tests/ViewModels/SettingsCarriesTheTransmitDriveTests.cs` -
  `:43`, `:59`, `:85`, `:111`. It builds `AppSettings` in memory.
- `tests/Hamlet.App.Tests/ViewModels/TheApplicationSendsAtTheLevelTheOperatorSetTests.cs` -
  `:90`, `:160`, `:204`; the click-to-boundary shape this unit's task 3 copies.
- `docs/unit268-what-step-d-asks-tim.md` - the page Tim follows at the rig. **It
  currently points him at the Send area, because that is where the figures were
  when it was written.** Task 4 corrects it.
- Root version at `Directory.Build.props:205` is expected to read **1.12.122**.
  **Read it; do not assume.** `Ft8Sharp` does not move.

**What failures are expected, and what each one means:**

- **The route to the sink's own figures may not exist without touching the keying
  path.** That is task 1 question 3 and it has a fallback written in advance in
  task 3. **If the fallback is taken, the screen says which number it is showing
  and the report says the fallback was taken and why.** A readout that quietly
  shows the composed peak while implying it is what left the card is worse than no
  readout.
- **The clip count may be zero and unable to be anything else.** That is task 4's
  measurement and **zero is a real answer** - it changes what the screen and the
  page tell Tim to watch. Do not manufacture a clip to make the number look
  interesting.
- **`SettingsStore.Save` writes `%AppData%\Hamlet\settings.json` on the developer's
  own machine.** Task 1 question 5 asks what the committed drive tests do about
  that. **Whatever they do, do the same. Do not leave a test that rewrites the
  operator's settings file with a test value.**
- **A machine with no render endpoint is a normal machine, not a red.** Nothing in
  this unit needs one.

Known reds, inherited, **never chased**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list
tripwire; and
`TheSinkPlaysToANamedEndpointTests.ACancelledPlayGoesOutShortAndTheSequenceCallsItAudioFailed`,
which unit 265 recorded and units 266, 267 and 268 have left alone.

---

## Rulings in force

Transcribed from `PHASE_PLAN.md`. **Not to be re-argued by this unit.**

**The abort.** Every path that keys the transmitter has a same-thread, no-await
abort - CI-V `0x17` with `0xFF`, PTT off as the fallback. **Built and proven by
units 257 and 263. It is not to be weakened, made conditional, or routed around.**

**One click, one transmission.** Hamlet transmits because the operator clicked.
**Never on a timer, never on a decode, never to continue a contact.** A
transmission he did not ask for goes out over other people's band and cannot be
taken back.

**Licence privileges.** Hamlet never transmits outside them. **The Settings gate is
not bypassable from any send path.**

**Tim operates a licensed station on an antenna and Hamlet transmits on the air.**
**HM-DEC-008 and HM-DEC-098 are withdrawn in full.** The dummy load is **not a
stage, not a fallback, and not to be referenced.** **Do not propose it, do not
treat its absence as a risk, and do not add a compensating control in its place** -
no confirmation dialog, no power limit, no test mode.

**One click, one message.** **Right-click sends immediately, in the next slot, with
no confirmation.**

**Nothing is forbidden in the menu.** The expected next message is highlighted;
everything valid stays clickable. **FT8 loses transmissions constantly, so sending
the grid a second time is correct behaviour**, and a repeat shows its count.

**A contact is never closed by the app.** `73` is politeness. Complete means the
exchange has what a QSO needs. **Hamlet is not the radio police.**

**`Ft8Sharp` is a faithful MIT port and nothing changes a line of it.**
`Ft8Sharp.Deep` is GPL-3.0.

**The engine is not told that tabs exist** (§0.1). **Nothing interprets a message**
(§12.1) - a row's state is bookkeeping over which messages passed, not meaning.

**A bench step's criteria are all satisfiable on a machine with no radio, and a
bench step never defers a criterion to Tim.** **This unit is the mirror of that
rule**: step D is a shack step, its three criteria are Tim's, and **this unit
builds only the part of it that a machine with no radio can build.** It does not
close a criterion of step D and it does not move one out of step D.

**A unit may not add a test without naming the breakage it would have caught.**

**FACT-004, and this unit makes no sound.** No radio has ever been attached to
this machine. **The device in this unit is a fake sink and the port is
`FakePort`. Do not open a render endpoint and do not open a serial port.**
Nothing measured tonight says anything about the IC-7300, and **no number this
unit produces is a recommendation about his drive level.**

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` **read from the clock,
not composed** (unit 268 corrected itself on exactly this), and `NOTE` saying what
is moving inside the task. The same every ten minutes while a task is running. Use
the file-editing tools if the shell refuses.

---

## Tasks

### Task 1 - the trace, and it comes first, in its own commit

**Measure before building.** `docs/unit269-the-level-trace.md`, committed and
pushed **before task 2 begins**. Six questions, **each answered with a file, a
line and a quotation**, and an answer of "none" is a finding.

1. **What does it cost Tim to change the drive while he is operating?** Confirm
   `ShowDialog` at `MainWindowViewModel.cs:4059` and say what is unreachable while
   that dialog is up - name whether `DigitalStopButton`, the decode table and the
   waterfall are behind it. **Say whether a slot boundary is still driven while the
   dialog is open**, from the code, not from a guess.
2. **What exactly does the operator read after a send today?** Quote the string
   `LevelLine` produces at the current default, name which quantity each number is,
   and say whether anything on the Digital tab shows a level **before** a send.
3. **Is there a route to `WasapiTransmitSink.PeakWritten` and `ClippedSamples`
   that leaves the keying path untouched?** The keying path is
   `Ft8TransmitSequence.RunAsync`, the key, the sink call, the `finally`, the
   abort and the stop - **units 255, 261 and 263 proved it and this unit does not
   edit it.** The route this instruction expects to exist is: the app already
   constructs the sink itself at `MainWindowViewModel.cs:8026`, so it can keep the
   reference and read the two figures off it after the boundary returns, with a
   small read-only report interface implemented by `WasapiTransmitSink` and by the
   tests' `FakeSink`. **Say whether that works, what it touches, and what it does
   not** - and if it cannot be done without editing the keying path or
   `ITransmitAudioSink.PlayAsync`, **say so and task 3 takes its fallback.**
4. **Where does markup go?** Name the element under which a control and a readout
   belong in the Send area beneath the Digital waterfall, with the line number, and
   name the `x:Name`s already there that a headless test can find.
5. **What do the committed drive tests do about `SettingsStore.Save`?** They call
   into a view model that saves to `%AppData%`. Say what actually happens when
   `SettingsCarriesTheTransmitDriveTests.ChangingTheDriveWritesItIntoTheSettings`
   runs on this machine, and what the new tests must therefore do.
6. **What in the tree already shows a transmit level anywhere on the main
   window?** Name it with a line, or say "none". `WaterfallGain` is a receive
   display control and is **not** it - say so if that is what you find.

### Task 2 - the control under the waterfall, watched red first

**The goal task's first half.** A Transmit drive control in the Send area beneath
the Digital tab's waterfall, with the dBFS beside it, **so he never opens a dialog
to move it.**

Build it in `tests/Hamlet.App.Tests/ViewModels/` as
`TheDriveIsSetWhereHeIsLooking...` or a name you can defend, **red first, the red
committed before it is made green**, and assert:

1. **The control opens showing the level that is actually in force**, for the
   default and for a settings object holding something else.
2. **Moving it writes `AppSettings.TransmitDrivePeak`** - the same field, in the
   same units, that `SettingsViewModel` writes. **Percent on the screen, peak in
   the file**, exactly as `:330` does it.
3. **A level the composer would refuse is not written**, and the operator is told
   in **`Ft8Composer`'s own sentence** - `DriveIsUsable`'s `out` string, asked of
   the composer and not answered a second time here.
4. **The two views cannot disagree**: after the tab's control moves the level, a
   freshly constructed `SettingsViewModel` over the same `AppSettings` shows the
   new value.
5. **The dBFS is on screen before anything is transmitted** and changes as the
   control moves.

**The breakage this would have caught**, and write it into the file: **two
controls over one setting drifting apart** - a tab control writing a percentage
into a field that holds a peak, so `25` means 2500 % of full scale, or one that
does not save, so the level he set against his ALC is gone next launch. **Neither
would fail anything in the tree today**, because today there is only one control.

**Reuse, do not duplicate.** If the percent-to-peak conversion, the
`DriveIsUsable` question and the note sentence can be shared with
`SettingsViewModel` rather than copied, share them and say so. **If sharing would
mean rewriting `SettingsViewModel`'s committed behaviour, copy them and write in
the file that the two are worth joining** - unit 266's precedent for exactly this
call.

Then run the four `SettingsCarriesTheTransmitDriveTests` from the named exception,
**each alone by exact name**, and report them.

### Task 3 - the readout: which number it is, watched red first

**The goal task's second half.** After a send, under the waterfall, **the level
that reached the card and the count of what had to be clamped.**

Take task 1 question 3's answer:

- **If the route exists**: hold the sink the view model already builds, read
  `PeakWritten` and `ClippedSamples` off it after the boundary returns, and show
  them under the waterfall. **`Ft8TransmitSequence`, `Ft8ArmedSend`, the key, the
  sink call, the `finally`, the abort and the stop are not edited** - say in the
  report exactly which files changed.
- **The fallback, written in advance**: show the composed peak, as `LevelLine`
  does today, **and say on the screen that it is the level Hamlet composed at and
  not what the endpoint was handed.** Then say in the report and in the outcome
  entry that the fallback was taken and what stopped the other route.

Assert, in the same file as task 2 or a second one, **red first**:

1. **A clicked send through the application** - `SendMessageCommand` to
   `AtSlotBoundaryAsync`, on `FakePort` and a substituted sink factory, the shape
   `TheApplicationSendsAtTheLevelTheOperatorSetTests.cs:90` already uses - **leaves
   the readout under the waterfall reading what the sink reported**, with the fake
   sink reporting a peak **deliberately different from the composed peak** so that
   a readout showing the setting instead of the measurement fails.
2. **The readout says which number it is** - the string names the quantity, so an
   operator is never left guessing whether he is reading his own setting back.
3. **The real sink satisfies the same route**, asserted on the type, so the
   production path cannot be forgotten while the fake one passes.
4. **Nothing was transmitted that was not clicked**: zero further sink calls and
   the port frames unchanged across a second boundary with nothing armed.

**The breakage this would have caught**, and write it into the file: **a readout
that shows the operator his own setting and calls it a measurement.** At the rig
that is the difference between a level he has verified and a number he typed, and
nothing in the tree today can tell them apart.

Then run the three `TheApplicationSendsAtTheLevelTheOperatorSetTests` from the
named exception, **each alone by exact name**. **If
`AfterASendTheLineSaysWhatLevelItWentOutAt` has to change, change it with the
reason written at the site and quote the before and after in the report.**

### Task 4 - what the clip count can actually count, and the page corrected

**A number, then a page.**

**The number.** Step D asks Tim to read a clip count. **Measure whether it can
ever be anything but zero on Hamlet's own path.** Set the drive to the highest
value `Ft8Composer.DriveIsUsable` accepts, compose through the application's own
route at 48000 Hz, and count the samples outside the rails. **Report the number,
whatever it is.** Consider and report whether the resampler can overshoot a
composed peak - if it can, that is what the count is for; if the answer is a flat
zero by construction, **say so plainly** and make the screen line and the page say
what the count is of, rather than leaving him watching a number that cannot move.
**Do not add a clip, do not raise the composer's ceiling, and do not turn this
into a recommendation about his level.**

**The page.** Correct `docs/unit268-what-step-d-asks-tim.md` so it matches the
tree this unit leaves: where the control is now, where the two figures are now,
which quantity each one is, and what the clip count tells him and what it does
not. **Correct it; do not rewrite it into a new document, and do not decide his
number for him.** `SHACK_FACTS.md` is **not** touched - the value in it is
criterion 3 and it is his to write.

### Task 5 - the phase's bookkeeping

File edits only.

- **Append the unit 269 entry to `PHASE_OUTCOME.md`** with
  `tools\arbiter\outcome-append.bat`. **If the shell refuses it - it has refused
  twelve consecutive units - take the plan's named alternative**: append by hand
  with the file-editing tools, in the format the existing entries use, all fourteen
  fields, plus an `APPENDED_BY:` line saying on its face that a script did not
  write it, and record the refusal verbatim in section 4.
- **The header's `STEP: D` line moves from `not started` to `in progress`, and to
  nothing else.** **Not `done` and not `partial`**: none of step D's three
  criteria is met, because all three are Tim's at the radio. Beside it, in the
  entry, say in one sentence what was built and what remains his.
- `PHASE_STATUS.md`'s `STEP:` lines belong to the launcher. **Write only
  `WORK_INSTRUCTION:`.** Never write `HEARTBEAT:` by hand.
- **Report, do not repair**, the reload's disagreements: `PROJECT_STATUS.md`
  `RULES_AT` reads `HM-DEC-158 (2026-09-07)` while `CLAUDE.md` §1 holds
  `CPS-DEC-0152`, and `.commit-msg.txt` and `.oa-267.bat` are untracked at the
  root - a fresh clone does not have them. **Neither is this unit's to fix.**

### Task 6 - the named drop candidate: what step E asks of Hamlet

**`docs/unit269-what-step-e-asks.md`. If the night runs long, this is what goes.**
It is the last bench-side reading before the phase's final step and it **builds
nothing, adds no test and changes no product code.**

Step E's three criteria: he answers a CQ on 14.074 or 7.074 and completes an
exchange; the transmitted slots appear in telemetry and the row reads complete;
what he saw and anything that surprised him, recorded. **Name, with a file and a
line for each, what in the application each one leans on** - the menu and the
send path from step B, the ledger and the four row states from step A, the
telemetry line unit 264 proved off disk - **and say whether anything they need is
missing at the bench.** If something is, name it; **do not build it tonight.**

---

## Parked - do not touch, do not raise

- **The number Tim's radio wants.** Step D criterion 2 and criterion 3.
  **No figure this unit prints is advice about his drive**, and `SHACK_FACTS.md`
  is not edited.
- **Working a station.** Step E is his. **Task 6 names what it leans on and
  performs nothing.**
- **The licence guard's off switch.** Unit 267 measured it and left it as found.
  Logged with the owner. No criterion depends on it.
- **The phantom `STEP: 1` in `PHASE_OUTCOME.md`.** Reported by units 267 and 268.
  Chasing it means editing the launcher's scripts. **Leave it and do not raise it
  again.**
- **An open `MenuFlyout` swallowing the next press** - unit 268 section 4 item 3.
  A harness fact, already written down. **Nothing in this unit raises a flyout.**
- **The two scene generators that should be one** - unit 266's section 4 item 3.
- **Automatic sequencing, logging, FT4/PSK31/WSPR transmit and CW send.** Out of
  this phase.
- **Anything in `src/Ft8Sharp/`.**
- The OSD re-encoding count, `ReusableWindow`, `ProcessDelayForTests`, the tap's
  owner, the waterfall's first row, unit 237's Extensible conclusion, work
  instruction 231's four tree items, `validate-output.bat`'s permitted-spellings
  bug, the 101.33 ms pulse above 6 kHz, the CW decoder and its inherited reds.

---

## What not to do

- **Do not touch the keying path**: `Ft8TransmitSequence.RunAsync`, the key, the
  sink call, the `finally`, the abort and the stop. **If the readout cannot be had
  without touching it, it is not had tonight** - take task 3's fallback.
- **Do not weaken, make conditional, or route around the abort**, and **do not
  move, resize, hide or condition `DigitalStopButton`.** A new control in the Send
  area that pushes the Stop button off the visible area has broken the first of
  the three things no unit may reason past.
- **Do not add a confirmation, a power limit, a lockout or a test mode.** The
  drive control is `PHASE_PLAN.md` step D's own criterion and it is **not** a
  compensating control: it refuses exactly what `Ft8Composer.DriveIsUsable`
  refuses and nothing more. **Do not reference a dummy load.**
- **Do not open a render endpoint and do not open a serial port**, and do not play
  a sound. Nothing here needs one.
- **Do not add a button that transmits to test the level.** One click, one
  message, and the click is on a message. A tune-up tone that Hamlet keys on its
  own is a transmission the operator did not ask for.
- **Do not show a number without saying which number it is.** Composed peak and
  written peak are different quantities; unit 268's 456 ms and unit 263's 15-20 ms
  are the precedent for saying so on the face of it.
- **Do not write a second copy of the drive validation.** Ask `Ft8Composer`.
- **Do not change `Ft8Composer.DefaultDrivePeak`.** -12.04 dBFS is unit 265's and
  it is where Hamlet starts.
- **Do not claim a criterion of step D**, and do not record step D `done` or
  `partial`.
- **Do not leave a test that rewrites the developer's `settings.json`.**
- **Do not run a test suite**, and **do not run any test outside the seven named
  above and what this unit builds.**
- **Do not background a command and poll for it.**
- **Do not ship a placeholder token in a reported number.**

---

## Committing and pushing

Commit and push each task before starting the next. **Task 1 is its own commit
before task 2 begins**, and a watched red is committed before it is made green.
Bump the root version's patch by one from whatever work instruction 268 left -
expected `1.12.122` to `1.12.123`, **read it rather than assuming it**.
**`Ft8Sharp` does not move.**

---

## Reporting

`output.md` at the repository root, overwritten, four sections per
`CLAUDE_CODE.md` §8.

**It opens with the ordering block, before the header, and
`validate-output.bat` refuses a report without one.** Three parts, **every line
specific to this unit**:

- **A. THE PHASE GOAL.** Hamlet works stations on the air - Tim answers a CQ on
  14.074 or 7.074 from Hamlet and completes an exchange. **Say where every step
  stands after tonight**: 0, A, B and C all `done` before this unit began, by
  units 266, 267 and 268; **D whatever this unit leaves it at, which is expected
  to be `in progress` and cannot honestly be more**; E `not started`. **Say in
  that paragraph that D's three criteria are Tim's and that this unit closed
  none.**
- **B. THIS STEP AND ITS EXIT CRITERIA.** Step D, *the drive level his radio
  wants*, and its three criteria: Tim sets the Transmit drive control and reads
  the dBFS and clip count under the waterfall; his radio's ALC behaviour at that
  level, in his words; the value recorded in `SHACK_FACTS.md`. **For each of the
  three say who can meet it and whether tonight moved anything under it** -
  criterion 1's bench half is this unit's whole subject and **its shack half is
  still his**. Criteria 2 and 3 are untouched and must be said to be.
- **C. WHAT THIS REPORT ADDS, AND WHETHER IT BEARS ON A OR B.** **Name how many
  items section 4 raises and whether any of them is in the way of a criterion in
  B**, and say whether any asks the owner to decide something. **If the readout
  took task 3's fallback, that belongs here and not in a footnote.**

Then the six-line header: `UNIT`, `PHASE GOAL`, `UNIT GOAL`, `ADVANCED`, `NUMBER`,
`DRIFT`.

- **`ADVANCED`** - this unit is authored expecting to close **no** exit criterion,
  because step D's three are all Tim's. **Say `no - no criterion of step D closed,
  and none could be` and then say what did move.** Do not stretch a blocker
  cleared into a criterion met.
- **`DRIFT`** - it stood at **0** after unit 268. **A unit that closes no
  criterion makes it 1.** Write `1 consecutive unit without advance (was 0)`
  unless something unexpected actually closed a criterion, and say in the same
  breath what was built. **Do not write 0 to keep the counter tidy** - the counter
  exists to be read by somebody who is not here.
- **`NUMBER`** - the one number this unit is worth. The candidate is **how many
  windows Tim has to open, and how many controls he has to cross, to change the
  drive between two slots: before and after.** If a better one is measured, use
  it, and say what it is of.

**Section 3 leads with three things, in this order:**

1. **What he does at the rig now, against what he had to do this morning** -
   quoted as the sequence of actions, with the modal dialog named. **This is the
   night's evidence and nothing else leads.**
2. **The readout after one clicked send, quoted as it appears on screen**, with
   which quantity each number is, and whether it came from the sink or from the
   fallback.
3. **The clip count measurement** - the drive it was measured at, the sample
   count, and the number, **including if the number is zero**, with what that
   means for what the page tells him to watch.

**Section 2 says what changes for the owner**, and unlike the last three units
**something on his screen does change**: say what appears, where, and that nothing
was taken away from Settings. Say plainly that **no level was chosen for him and
`SHACK_FACTS.md` was not touched.**

**Section 4 says whether a ruling is wanted, and answers it in a sentence.** An
empty section 4 is a real answer. **If task 1 question 3 found no route to the
sink's figures, section 4 carries what was tried and what the fallback cost.**

```
ARBITER-DECISION
STEP: D
APPROACH: put the transmit drive control and the dBFS and clip readout under the waterfall on the Digital tab, out of the modal Settings dialog, and read the level off the sink the app already builds rather than off the setting the operator typed
MOVE: work around
WHY: No unit can perform step D - its three criteria are Tim at his own radio - but criterion 1 names a control and a readout "under the waterfall" that do not exist there: the drive is behind a ShowDialog modal that covers the waterfall, the decode table and the Stop button, and the dBFS and clip count appear only in a sentence after a send, reporting the level he set rather than the level the card was handed. Rather than declare the step unreachable or hand him an evening he cannot run, this unit builds the half of criterion 1 a machine with no radio can build and leaves the half only he can answer.
STATE: not started
DECIDED: Three on my own authority. First, the drive control appears on the Digital tab as well as in Settings rather than moving out of Settings, because both write one AppSettings.TransmitDrivePeak through one Ft8Composer.DriveIsUsable and a control removed from Settings would break unit 265's committed suite for no gain. Second, the readout is required to name which quantity it shows - composed peak or the peak the endpoint was handed - with a fallback written in advance, because the honest failure of this unit is a screen that shows the operator his own setting back and calls it a measurement. Third, step D is recorded in progress and not partial, because none of its three criteria is met and partial would claim one that is Tim's.
LICENCE: PHASE_PLAN.md step D's first exit criterion, which names the Transmit drive control and the dBFS and clip count under the waterfall; the plan's rule that a criterion needing the radio moves to a shack step, which leaves the bench half of criterion 1 as bench work rather than his; its named alternatives to stopping, including the file-editing tools where the shell refuses; and unit 268's section 4 item 2, which reported the plan-versus-tree mismatch and was told to report and not repair it.
ACCOMPLISHED: When Tim sits down at the radio he can set Hamlet's drive from the screen he is already looking at, without opening a dialog over the band and over the Stop button, and read what the level actually was after each slot - with the screen saying which number that is, so a level he verified is never confused with a number he typed. What his radio's ALC does at that level, and what the value should be, is still his to answer, and this unit chooses nothing for him.
ADVANCES: none - this unit closes no exit criterion of step D, because all three are Tim's at the radio. It clears the blocker under criterion 1: the Transmit drive control lives behind a modal dialog that covers the waterfall, the decode table and the always-pressable Stop button, and no dBFS or clip count appears under the waterfall at all.
END-ARBITER-DECISION
```

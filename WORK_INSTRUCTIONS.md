# Work instruction 268 - the whole chain runs from one right-click, at the bench

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

**Task 2 of this unit carries a named, bounded exception to rule 1, listing two
existing tests by exact name.** It is written out there with its reason. **It
widens nothing else**: no suite, nothing unfiltered, nothing backgrounded, and no
test outside those two and what this unit builds.

**A test here runs for the better part of a minute of wall clock and that is
correct.** One FT8 slot is 12.64 seconds of real audio out of a real sound card,
with a second of pre-roll and a second of post-roll around it. **State a timeout
of ten minutes on the test runs and five on the builds**, foreground them, and do
not mistake a slot going by for a hang.

---

## THE TOOL RULE

**This session's shell may refuse calls.** A refused shell call is a signal to
reach for the other tool, not to stop. **The file-editing tools have been
unaffected throughout.** Record every refusal verbatim. **Nothing in this unit
halts the loop.**

**This shell collapses a doubled backslash inside a quoted heredoc.** Use the
file-editing tools for anything with escapes in it.

---

## Why this unit exists

**Three steps have closed and the drift counter is at 0.** Unit 266 closed step 0
and step A; unit 267 closed step B, all four criteria on runs rather than on
committed markup, and a separate session judged it `done` against the criteria.
**Step C is `not started` and no unit has been spent on it.** Steps D and E are
Tim's, at the radio, and nothing before them is blocked by them.

**Step C is the last thing in this phase that can be proved without a radio.**
Everything after it is Tim. The parts it needs are all in the tree and were named,
with files and lines, by unit 267's task 5 in `docs/unit267-what-step-c-needs.md`:
the loopback endpoint, the fake port, the decoder, the real right-click, the real
Stop button, the telemetry line off disk. **None of them has to be invented. What
has never happened is joining them.**

**What is missing is one seam, and it is the same shape as the seam unit 267
closed.** Today the real endpoint is proved in a plain `[Fact]` sending a literal
the test chose for itself, and the real right-click is proved in an
`[AvaloniaFact]` that never keys anything. **So the one thing nobody has measured
is whether the text on the menu item the operator actually clicked is the text
that comes back out of the sound card** - and a menu that offered the wrong string
would pass every test in the tree.

```
PHASE GOAL: Hamlet works stations on the air.
UNIT GOAL:  Step C closes - one right-click drives menu, compose, key, play,
            unkey and telemetry through a real endpoint on a loopback with CI-V
            on a fake transport; the audio decodes back to the text on the menu
            item that was clicked; the operator's Stop button takes a
            transmission off the card from the middle of that chain; and nothing
            transmits that was not clicked, with the card open and silent.
ADVANCES:   Step C, all four exit criteria. Criteria 1, 2 and 4 by the chain test
            task 3 builds; criterion 3 by the abort run task 4 adds to it.
```

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
**Report them; do not repair the instruction.**

Every line below was read off the tree while this instruction was written, on
2026-09-07. **Line numbers move. Read them; do not assume them. Where one is
wrong, say so in section 4 and carry on with the rest.**

- **The loopback test is a plain `[Fact]`, not an `[AvaloniaFact]`** -
  `tests/Hamlet.App.Tests/ViewModels/TheLoopbackThroughTheApplicationsSendPathTests.cs:64`,
  with `AMessageTheApplicationSentLeavesThisMachineAndComesBack` at `:65`. **This
  is the fact the whole unit turns on.**
- That test already carries: `Preferred(out var why)` at `:67` with the graceful
  stop at `:69-79`; **`const string Message = "CQ KC3QIS FN00"` at `:84` - the
  literal a test chose**; the real `WasapiTransmitSink` through the panel's own
  factory at `:93-99`; `NAudio.Wave.WasapiLoopbackCapture` at `:119`;
  `panel.SendMessageCommand.Execute(Message)` at `:151`;
  `Assert.Equal(UnkeyRoute.OrdinaryUnkey, run.CameOutOfTransmit)` at `:189`;
  `new Ft8SlotDecoder().Decode(onTheGrid.Samples).Texts` at `:215` and the
  containment assertion at `:226`; `Preferred` itself at `:250`.
- **The right-click is an `[AvaloniaFact]`** -
  `tests/Hamlet.App.Tests/Views/TheMenuIsUnderTheMouseTests.cs:113`, with
  `EveryStationsPredictedMenuAppearsUnderTheMouse` at `:114`. Its `RightClick`
  helper is at `:496`, `RightClickRow` at `:506`, and the real
  `ContextRequestedEventArgs` is raised on the row control at `:527`.
- **The Stop button is an `[AvaloniaFact]` too** -
  `tests/Hamlet.App.Tests/Views/TheOperatorCanStopItTests.cs:240`,
  `AClickWhileTheToneIsPlayingStopsTheSoundAndNotJustTheCarrier` at `:241`;
  `Assert.Same(scene.Panel.StopSendingCommand, button.Command)` at `:467`; the
  `DigitalStopButton` found on the realized window at `:662`.
- **The telemetry read-back exists** -
  `tests/Hamlet.App.Tests/ViewModels/TheWholeContactWalksThroughTheApplicationTests.cs:256`,
  `OneSendLeavesOneLineOnDiskAndTheLineNamesNobody`, through the application's own
  `AppSettings` enabled-category predicate.
- **The fake CI-V transport exists** - `tests/Hamlet.App.Tests/FakeTransmitParts.cs:17`,
  `FakePort`; `FakeSink` is at `:74` and **is not what this unit uses for the
  chain**.
- **`Hamlet.App.Tests.csproj` references `Avalonia.Headless.XUnit` 11.3.0 at
  `:12` and does not name NAudio at all** - the loopback test reaches
  `NAudio.CoreAudioApi` transitively. **Confirm this; if the reference has to be
  made explicit, that is a project-file edit and it is allowed.**
- Root version at `Directory.Build.props:205` is expected to read **1.12.121**.
  **Read it; do not assume.** `Ft8Sharp` does not move.

**What failures are expected, and what each one means:**

- **The host question may fail.** Nothing in the tree is both a headless Avalonia
  window and an open WASAPI render endpoint. **If task 2's first assertion cannot
  be made to pass, that is a measurement, not a defeat** - take the named fallback
  in task 2 and say so.
- **A machine with no render endpoint is a normal machine, not a red.** The
  graceful stop at `:69-79` is precedent and **must survive into whatever this unit
  builds**. If `Preferred` returns null tonight, say so plainly, record step C as
  `blocked` on a fact about the machine, and do not manufacture a pass.
- **The abort run must not be read as a decode failure.** See task 4.

Known reds, inherited, **never chased**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list
tripwire; and
`TheSinkPlaysToANamedEndpointTests.ACancelledPlayGoesOutShortAndTheSequenceCallsItAudioFailed`,
which unit 265 recorded and units 266 and 267 left alone.

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
bench step never defers a criterion to Tim.** If a criterion here turns out to need
the radio, **say which, and say it belongs in step D or E** - do not leave it open
and do not defer it to him.

**A unit may not add a test without naming the breakage it would have caught.**

**FACT-004, and this unit makes sound.** No radio has ever been attached to this
machine. **The device in this unit is a sound card and the port is `FakePort`.**
Real audio out of a real render endpoint is what step C's first criterion asks
for and units 256, 262, 263 and 265 all did it. **A real serial port beside a real
sink would be a real transmission and there is no radio here to make one on: do
not open one.** Nothing measured tonight says anything about the IC-7300.

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, and `NOTE`
saying what is moving inside the task. The same every ten minutes while a task is
running. **A test that plays a 12.64-second slot will outlast a ten-minute
window: write the status line before you start the run, not after it.** Use the
file-editing tools if the shell refuses.

---

## Tasks

### Task 1 - the trace, and it comes first

**Measure before you build.** `docs/unit268-the-chain-trace.md`, six questions,
**each answered with a file, a line and a quotation**. Commit and push it as its
own commit before task 2.

`docs/unit267-what-step-c-needs.md` did the survey and this is not a repeat of it.
**Read it first, then answer only what it left open.**

1. **The click's own text.** When a menu item built by `SendFlyoutFor`
   (`MainWindow.axaml.cs`) is invoked, **what exact string reaches
   `SendMessageCommand`** - `Ft8SendOption.Text`, a header string, or something
   built again on the way? Quote the binding. **This is the string criterion 2
   must be compared against**, and if the menu item carries a decorated header
   rather than the message, say which property is the message.
2. **The host question, on paper before it is on the machine.** What does
   `[AvaloniaFact]` do to the thread - which dispatcher does the test body run on,
   and what in `TheMenuIsUnderTheMouseTests` or `TheOperatorCanStopItTests` pumps
   it? **Name what would have to be true for a `WasapiLoopbackCapture` callback
   and a 12.64-second `await` to coexist with it.**
3. **The telemetry line.** What does
   `TheWholeContactWalksThroughTheApplicationTests` do to make the application's
   own writer land a line on disk - which settings, which path, which predicate -
   and **what would the chain test have to set up to read one back?**
4. **The unkey and the frames.** Which property on the run carries the unkey route,
   and **what exactly does `FakePort` record** - byte counts, frames, or both? Quote
   the assertion the loopback test makes at `:189` and whatever
   `TheSendPathReachesARealRadioTests` asserts about port frames.
5. **The stop, from the button.** What does pressing `DigitalStopButton` reach, in
   order, down to `Ft8ArmedSend.StopNow`? **And what does the run report afterwards**
   - which outcome, and which of `Ft8StopResult`'s three facts says how much went
   out?
6. **What silence is.** Criterion 4 wants *nothing transmitted* proved with the card
   open rather than by an untouched counter. **What does the existing capture block
   give you to measure it with** - a sample count, an RMS, a peak? Name the figure
   and say what it reads on this machine when nothing is playing. **If the answer is
   that nothing in the tree measures a quiet capture, write "none" and say so.**

**Answer honestly where the answer makes a later task smaller.** Unit 267's
precedent stands and unit 264's before it: **a finding that the proof already
exists is a finding, not a shortfall, and no red is to be manufactured to fill a
task.**

### Task 2 - the host question, answered by building the first assertion

**This is the fork in the road and it is deliberately placed before the goal
task.** Nothing in the tree is both a headless Avalonia window and an open WASAPI
render endpoint, and unit 267 named that as the most expensive thing step C needs.
**Find out in the first half hour, not the last.**

Build the first assertion of the chain test:
`tests/Hamlet.App.Tests/ViewModels/TheWholeChainRunsFromOneRightClickTests.cs`, an
`[AvaloniaFact]` in a headless window that:

- chooses a render endpoint with the same `Preferred`-style logic and **keeps the
  graceful stop** - no endpoint is a fact about the machine, printed, not a red;
- opens a `WasapiLoopbackCapture` on it;
- builds the panel, sets `FakePort` for CI-V and the **real** `WasapiTransmitSink`
  through the panel's own factory;
- sends one message through `SendMessageCommand` and drives
  `AtSlotBoundaryAsync`;
- asserts the capture came back **not silent** - the figure task 1 q6 named.

**The breakage it would have caught**, which the rule requires named: **a send path
that keys the radio and plays nothing**, or plays to an endpoint other than the one
named in Settings. Every app-side send test today runs on `FakeSink`, whose
bookkeeping says a play happened because it was asked to; **only a card and a
capture can say a sound was made.**

**The bounded exception to the no-suite rule, and its reason.** You may run these
two committed tests, each alone by exact name, foregrounded, and no others beyond
what this unit builds:

| Test | Why |
|---|---|
| `AMessageTheApplicationSentLeavesThisMachineAndComesBack` | the loopback baseline. If it fails tonight the machine changed, and a new test must not be blamed for that |
| `ExactlyOneFileInTheShippedTreeCallsTheSequence` | criterion 4's static half - exactly one file in `src/` reaches a keying frame. It is a grep and not a run, which is why it belongs beside a run |

**No suite. Nothing unfiltered. Nothing backgrounded. Nothing outside those two.**

**If the two hosts will not join, take this fallback and do not stall.** Drive the
menu through the view's own `SendFlyoutFor` code path in a plain `[Fact]` - the
real flyout-building code, the real `Ft8SendOption`, the real command binding, but
no raised `ContextRequested` and no window. **Then say exactly that in the report
and in `PHASE_OUTCOME.md`: criterion 1's right-click is proved at the flyout
builder and not at the raised event, and the difference is that a defect in the
markup's own handler would not be caught.** That is a cut down, it is named in
advance, and it is worth more than an empty step. **Record what you tried and what
it did, verbatim, before you take it.**

### Task 3 - the goal task: the whole chain, from the click to the decode

**Criteria 1, 2 and 4, in one test method**, grown from what task 2 built.

**One right-click on a decoded row**, driven as `TheMenuIsUnderTheMouseTests`
drives it - a real `ContextRequested` on a real row control in a real window -
then **invoke the menu item itself**, not `SendMessageCommand` with a literal.

Then, in the same method:

1. **Compose, key, play, unkey, telemetry.** The boundary runs, the port carries the
   keying and unkeying frames, the run reports `UnkeyRoute.OrdinaryUnkey`, and the
   application's own writer leaves one `ft8_transmission` line on disk.
2. **The decode, and this is the assertion the step exists for.** Resample the
   captured audio with `Ft8Resample.ToFt8Rate` and decode it with `Ft8SlotDecoder`,
   and assert the decoded text is **the text carried by the menu item that was
   clicked**, read off the flyout - **not a literal the test chose, and not a
   round trip of one.** Print both strings side by side in the run output.
3. **Nothing transmits that was not clicked.** Drive a second slot boundary with
   nothing armed, **with the endpoint still open and the capture still running**,
   and assert the card was silent across it - the figure from task 1 q6, not an
   untouched counter. One right-click, one transmission, measured as sound.

**The breakage it would have caught:** a menu that offers one string and a send path
that transmits another. The loopback test sends a literal it chose itself; the menu
tests never transmit. **Between them there is no test in the tree that would fail if
the two disagreed**, and the operator would be told he sent one thing while another
went out over the band.

**The trap unit 267 wrote down, and it applies here.** A refusal comes back as
`Ft8ArmOutcome.Ran` at the boundary (`Ft8ArmedSend.cs:473` returns `Ran` for
anything the sequence ran); only `TransmitRun.Outcome` carries the real answer.
**Assert on the run's outcome, not on the arm outcome.**

**Watch it red first if a red can be had honestly** and commit the red with its
verbatim output. **If it is green whole on its first run, that is the finding: say
so, and do not manufacture a red.**

**No product code unless a defect is found.** If the decoded text and the clicked
text differ, **that is the headline of the night** - report it with both strings
quoted, and **do not touch the keying path, the abort, or
`Ft8TransmitSequence.RunAsync`.**

### Task 4 - the abort, from the middle of that chain

**Criterion 3.** A second method in the same class, sharing the same harness.

**Why it is a second run and not the same one, and this must not be rediscovered
as a red:** a stopped transmission does not decode. Criterion 2 needs a whole
12.64-second slot to reach the decoder and criterion 3 truncates one on purpose.
**Unit 267 found this while naming the pieces and it is written into
`docs/unit267-what-step-c-needs.md`.** A test that expected one transmission to do
both would go red looking like a decode failure.

The run: **the operator's own right-click** starts a transmission, and then **the
real `DigitalStopButton` in the real window is pressed** part-way through it - not
`StopNow` called directly. Assert:

- **the card goes quiet**, measured on the capture, and **say how many seconds of
  audio went out of the endpoint against the 12.64 the slot would have been**;
- **how long after the press it went quiet**, printed as a figure. Unit 263
  measured 20 ms on a real endpoint with a 200 ms buffer; **report what this run
  reads, whatever it is, and do not assert a tighter bound than the tree already
  states**;
- **the carrier came off the wire** - the abort's frames at `FakePort`.

**The breakage it would have caught:** a stop that takes the carrier off the wire
and leaves the card playing. That is unit 261's real defect and unit 263 fixed it -
but the operator's *press* is proved against a fake sink and the *card going quiet*
is proved in the engine with no operator, and **nothing joins the two.** A
regression that reconnected them wrongly would leave both existing tests green.

**Do not weaken, make conditional or route around the abort.** You are proving it,
not editing it.

### Task 5 - the phase's bookkeeping

**File edits only. No shell needed if the shell refuses.**

- Append this unit's entry to `PHASE_OUTCOME.md` through `outcome-append.bat`.
  **Unit 267 was refused both invocation forms**, verbatim `This command requires
  approval`. **Try it; if it refuses, append with the file-editing tools in the
  format the existing entries use and add an `APPENDED_BY:` line saying so on its
  face**, exactly as unit 267 did. This is the plan's own named alternative and it
  halts nothing.
- **Record step C's state honestly**, in the words the file permits and no others:
  `done` only if all four criteria are met on evidence you can quote from a run
  tonight; `partial` with the criterion named if not; `blocked` with the fact named
  if the machine had no render endpoint. **Do not record `done` for a criterion
  proved by reading the tree**, and **do not record `done` for criterion 1 if the
  right-click was the fallback** - record it and say which half is proved.
- If any criterion turns out to need Tim's radio, **say which and say it belongs in
  step D or E.** Do not leave it open and do not defer it to him. **None of step C's
  four should**; if one does, that is a finding about the plan and it belongs in
  section 4.
- `PHASE_STATUS.md`'s `STEP:` lines belong to the launcher. **Write only
  `WORK_INSTRUCTION:` there.**

### Task 6 - the page Tim reads at the radio

**Named drop candidate.** `docs/unit268-what-step-d-asks-tim.md`.

**Drop this if the night runs out. Do not drop task 5.**

Step C is the last thing that can be proved without a radio, so the next thing that
happens is Tim at his station. **One page, in his terms, that he can follow at the
rig**: where the Transmit drive control is, what the dBFS readout and the clip
count mean, what to watch on his own ALC, and what step D asks him to write into
`SHACK_FACTS.md` afterwards.

**Write down what to do; do not do it, and do not decide anything for him.** No
number is chosen here - the drive level his radio wants is the one number nobody in
this repository can know, which is why it is step D. **Do not add a control, do not
change a default, and do not touch `SHACK_FACTS.md`.**

---

## Parked - do not touch, do not raise

- **The licence guard's off switch.** Unit 267 measured it, answered it in a
  sentence and left it as found: with `RestrictTransmitToPrivileges` off,
  `Ft8TransmitSequence.Permits` still refuses and the operator gets a sentence
  rather than a transmission. **Logged with the owner. Not this unit's subject, and
  no criterion of step C depends on it.**
- **The phantom `STEP: 1` in `PHASE_OUTCOME.md`.** Unit 267 reported it; chasing it
  means editing the launcher's own scripts. **Leave it. Do not chase it and do not
  raise it again.**
- **The drive level and working a station.** Steps D and E, Tim's, at the radio.
  **Task 6 writes a page for him and performs nothing.**
- **Automatic sequencing.** Out of this phase.
- **The two scene generators that should be one** - unit 266's section 4 item 3.
- **The archived cut's steps 4 and 5 at `partial`.** That cut is finished.
- **Anything in `src/Ft8Sharp/`.**
- The OSD re-encoding count, `ReusableWindow`, `ProcessDelayForTests`, the tap's
  owner, the waterfall's first row, unit 237's Extensible conclusion, work
  instruction 231's four tree items, `validate-output.bat`'s permitted-spellings
  bug, the 101.33 ms pulse above 6 kHz, the CW decoder and its inherited reds.

---

## What not to do

- **Do not weaken, make conditional, or route around the abort** - `PHASE_PLAN.md`,
  the first of the three. Task 4 proves it and does not edit it.
- **Do not touch the keying path**: `Ft8TransmitSequence.RunAsync`, the key, the
  sink call, the `finally`, the abort, the stop. Units 255, 261 and 263 proved
  them.
- **Do not open a serial port.** CI-V is `FakePort` and nothing else. A real port
  beside a real sink is a real transmission and there is no radio here.
- **Do not add a second copy of the licence rule**, and do not re-argue the guard's
  off switch.
- **Do not compare the decode against a literal the test chose.** That is exactly
  what the tree already does and it is the thing this unit exists to replace.
- **Do not put criterion 2's decode and criterion 3's abort in the same
  transmission.** A stopped slot does not decode.
- **Do not assert a tighter timing bound than the tree already states.** The stop's
  bound is 250 ms and the card's quiet came 15-20 ms after it on this machine;
  report what you measure and do not turn a margin into the thing under test.
- **Do not turn a machine with no render endpoint into a red.** Keep the graceful
  stop.
- **Do not run a test suite**, and **do not run any test outside task 2's two named
  tests and what this unit builds.**
- **Do not background a command and poll for it.**
- **Do not grey out, disable, hide or remove a menu option**, and do not make the
  contact state decide which messages are valid.
- **Do not add a confirmation, a power limit, a test mode or any other compensating
  control**, and **do not reference a dummy load**.
- **Do not defer a criterion to Tim.** If one needs the radio, name it and put it in
  step D or E.
- **Do not ship a placeholder token in a reported number.**

---

## Committing and pushing

Commit and push each task before starting the next. **Task 1 is its own commit
before task 2 begins**, and a watched red is committed before it is made green.
Bump the root version's patch by one from whatever work instruction 267 left -
expected `1.12.121` to `1.12.122`, **read it rather than assuming it**.
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
  stands after tonight**: 0, A and B `done` before this unit began, C whatever this
  unit leaves it at and why, and D and E `not started` and both Tim's.
- **B. THIS STEP AND ITS EXIT CRITERIA.** Step C, *the whole chain runs from one
  click, at the bench*, and its four criteria: one right-click driving menu,
  compose, key, play, unkey and telemetry in one test with the endpoint on a
  loopback and CI-V on a fake transport; the audio decoding back to the message the
  operator clicked; the abort firing from the middle of that chain and the sound
  stopping; and nothing transmitting that the operator did not click, across the
  whole chain. **Say which were met and on what evidence - a run tonight, or the
  tree** - and **if criterion 1 was met by the fallback rather than by a raised
  right-click, say so in that line and not in a footnote.**
- **C. WHAT THIS REPORT ADDS, AND WHETHER IT BEARS ON A OR B.** **Name how many
  items section 4 raises and whether any of them is in the way of a criterion in
  C**, and say whether any asks the owner to decide something.

Then the six-line header: `UNIT`, `PHASE GOAL`, `UNIT GOAL`, `ADVANCED`, `NUMBER`,
`DRIFT` - the drift counter stood at **0** after unit 267.

**Section 3 leads with three things, in this order:**

1. **The clicked string and the decoded string, quoted side by side**, with the
   endpoint named and its sample rate, and the run's outcome and unkey route
   beside them. **This is the night's evidence and nothing else leads.**
2. **The abort from the middle of the chain**: how many seconds of the 12.64 left
   the card, how long after the button press it went quiet, and the abort's frames
   at the fake port.
3. **The card open and silent across a boundary nobody clicked** - the figure that
   says silence, and what it reads.

**Section 2 says what changes for the owner**: nothing on his screen, and what is
now proved is that the message he clicks is the message that leaves the machine,
that his Stop button takes a real transmission off a real card, and that the
machine makes no sound he did not ask for.

**Section 4 says whether a ruling is wanted, and answers it in a sentence.** An
empty section 4 is a real answer. **If the two hosts would not join, section 4
carries what was tried, verbatim, and what the fallback cost.**

```
ARBITER-DECISION
STEP: C
APPROACH: Join the loopback endpoint to a real right-click in one Avalonia headless test - two transmissions inside it, one decoded back to the clicked menu text and one aborted mid-slot with the card measured going quiet
MOVE: continue
WHY: Step C has had no unit spent on it, its entry steps A and B are both done, and the loop test found no resembling approach - the three tried so far are bookkeeping on step 0, the state transitions on step A and the licence gate on step B. Every piece step C needs exists in the tree with a file and a line; what has never happened is joining them, and the seam nobody has measured is whether the text on the menu item the operator clicked is the text that comes back out of the sound card.
STATE: not started
DECIDED: Three on my own authority. First, the host question - whether a headless Avalonia window can hold an open WASAPI render endpoint and a loopback capture - is task 2 rather than a discovery inside the goal task, with a named fallback written in advance (drive the view's own SendFlyoutFor code path in a plain Fact, and say in the report and the outcome entry that criterion 1's right-click is proved at the flyout builder and not at the raised event), because unit 267 named it the most expensive thing step C needs and a unit that finds it out at hour four has nothing to show. Second, criterion 3's abort is a second method in the same class rather than a second run inside one method, because a stopped transmission does not decode and one method holding two 15-second slots plus a pumped dispatcher makes the timeout the thing under test - criterion 1's "one test" is satisfied by the chain method, which is what that phrase names. Third, a bounded exception to the no-suite rule for exactly two committed tests, each alone by exact name: the loopback baseline, so a machine fact is not mistaken for a new test's failure, and the static one-caller tripwire, which is criterion 4's grep half and belongs beside criterion 4's run half.
LICENCE: PHASE_PLAN.md step C's four exit criteria, which name the loopback endpoint and the fake transport in the criterion itself; its rule that a bench step's criteria are all satisfiable with no radio and none deferred to Tim; its named alternatives to stopping - an approach that fails is abandoned with its cost recorded and another taken, and the file-editing tools are used where the shell refuses; and SHACK_FACTS.md FACT-004, under which the device is a sound card and the port is FakePort.
ACCOMPLISHED: The message Tim clicks is the message that leaves the machine - proved by decoding the sound off his own card back into text and matching it against the menu item he clicked, rather than against a string a test chose for itself. His Stop button takes a real transmission off a real card from the middle of the chain, not off a fake. Nothing goes out that he did not click, measured as a silent card rather than as an untouched counter. That is the last thing about Hamlet's transmit side that can be established without a radio; everything left is Tim at his own station.
ADVANCES: Step C, all four exit criteria - criteria 1, 2 and 4 by the chain test task 3 builds from the right-click through to the decode and the silent boundary, and criterion 3 by the abort run task 4 adds beside it.
END-ARBITER-DECISION
```

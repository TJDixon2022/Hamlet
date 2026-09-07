# Work instruction 260 - the menu under the mouse, and a route to the radio

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

**All four were checked against the tree at 2026-09-06T23:3x, at `HEAD 2a3a90a`,
while this instruction was written.** `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` are present; neither
`CoreHMI.sln` nor `MURC.sln` exists; the only solution at the root is
`Hamlet.sln`. Check them anyway.

---

## THE THREE RULES THAT KILLED FOUR SESSIONS

**Tim's rulings of 2026-09-05. Not this unit's to weigh.**

**1. A unit runs no test suite.** **A unit may run only the unit test it
constructs in that work instruction**, filtered by exact name, in the foreground,
with a stated timeout of a few minutes. **An unfiltered `dotnet test` on any
project is forbidden.**

**2. Never background a command and poll for it.** Three sessions were killed by
the watchdog on 2026-09-05, at 33 to 38 minutes, each sitting in
`until grep -q "exited with code" ...; do sleep 15; done` with a `900000` ms
timeout.

**3. THE WATCHDOG FIRES AFTER TWELVE MINUTES WITH NO STATUS WRITE.** It killed
unit 257 at fourteen minutes with three files written and none committed. Units
258 and 259 wrote `PROJECT_STATUS.md` repeatedly and lost nothing. **The status
write is part of the work, not part of the reporting.**

`dotnet build` is allowed, foregrounded, with a timeout.

**This unit opens no audio device, opens no serial port, and plays no sound.**
It *builds the routes* to both and proves the decision about them without taking
either. See *What not to do* §1, which is narrower this time than it was last
time and says exactly where the line is.

---

## THE TOOL RULE

**This session's shell may refuse calls.** A refused shell call is a signal to
reach for the other tool, not to stop. **The file-editing tools have been
unaffected throughout.** Record every refusal verbatim. **Nothing in this unit
halts the loop.**

Three tool facts earlier units paid for, carried forward unchanged:

1. **`tools\arbiter\outcome-append.bat` has been refused for seven consecutive
   units** - 253 through 259. **Expect an eighth.** Task 5 tells you what to do
   instead: append the entry with the file-editing tools, in the exact
   twelve-field format the existing entries use, ASCII only.
2. **`tools\arbiter\validate-output.bat` has been refused in eleven forms**
   across units 255, 256, 257, 258 and 259. **This shell may not start a batch
   file.** Try it once; if it is refused, check the report by hand against the
   rules in the reporting section below, say in the report that you did, and
   **quote no exit code**, because there is none.
3. **The shell has refused device enumeration, `powershell -NoProfile`,
   `git reset -q <paths>`, `sed -i`, and heredocs for commit messages.**
   `git restore --staged` and repeated `-m` flags do the same jobs. **You need
   device enumeration for nothing in this unit** - task 4 binds a list the
   application builds at run time and you never call it yourself.

**One more, measured while this instruction was written.** The arbiter's own
`outcome-read.bat` breaks on an apostrophe inside `--approach` - the quoting is
consumed by the batch file's inner PowerShell and it reports a parser error
instead of a reading. **Not yours to fix** (`tools\` is not yours to touch); it
is written here so the next arbiter does not spend a call on it.

---

## Why this unit exists

**The count today.** Step 0 `done`, one unit. Step 1 `partial` at four of five and
closed. Step 2 `partial`, two units, criterion 4 deferred to Tim. Step 3
`partial`, four units, closed on the loopback with the radio's own drive level
deferred to Tim. Step 4 `partial`, three units, closed at five of six with the
sixth met in substance. **Step 5 `partial`, two units** - which is one unit
recorded twice - **and step 6 cannot have one.**

**Step 5 is still the last step any unit can move, and it went to `partial` and
not to `done` for two named reasons.** Both are in unit 259's own record and both
were deliberate. Neither has been attempted:

1. **Criterion 2 is not on screen.** `SendMenuFor(row)` exists on the view model,
   returns the whole menu, and is asserted station by station - **and there is no
   `ContextFlyout` anywhere in `src/`.** I grepped the whole of `src/` for the
   word while writing this and it appears zero times. **No operator has ever
   right-clicked a decoded row.** The criterion is written as an operator's
   gesture - *right-click a decoded row and the menu offers...* - and a menu that
   cannot be reached with a mouse does not meet it.
2. **Criterion 3 has no route to a radio.** `private Ft8ArmedSend? _armedSend` at
   `MainWindowViewModel.cs:7896` is **assigned in exactly one place in the whole
   tree, and it is the test seam** - `UseArmedSendForTests` at `:8228`. There is
   no line in `src/` that constructs one. So on every real run of the application
   `_armedSend` is null and every click lands on the refusal at `:8009`.

**Step 6 is Tim answering a CQ on 14.074 and completing an exchange.** Its entry
is steps 0 to 5. **He cannot attempt it tonight**, and not because of a radio: he
cannot right-click a row, and if he could, the click would reach a null field and
say so. **Those two are what this unit is for.** They are the difference between
a phase whose last step is waiting on a man with a radio and a phase whose last
step is waiting on code nobody has written.

**What exists and is waiting to be joined.** Read out of the tree while this
instruction was written; line numbers below.

- `MainWindowViewModel.SendMenuFor(DigitalDecodeRow?)` at `:7934` - the whole
  menu for one row, built at the moment it is called. **Nothing calls it from the
  view.**
- `MainWindowViewModel.SendMessageCommand` - the `[RelayCommand]` over
  `SendMessage(string?)` at `:7982`. **It is the one entry point that arms.**
- `MainWindow.axaml.cs` - **114 lines**, whose own summary says its code-behind
  owns the facts the view knows and the ViewModel cannot. A `ContextRequested`
  handler belongs there and it is a small file.
- `CreateRig(string)` at `:9827` - `new Ic7300Rig(new SystemSerialPort(selection))`
  at `:9830`. **The port is constructed there and thrown away in the same
  expression.**
- `AppSettings.AudioOutputDeviceId` at `Settings/AppSettings.cs:215` - added by
  unit 259, **and nothing in `src/` reads it.** Its only mentions are one test and
  the generated XML doc.
- `WasapiTransmitSink.Endpoints()` at `Audio/WasapiTransmitSink.cs:243`, returning
  `IReadOnlyList<RenderEndpoint>` with `Id`, `Name`, `IsDefault`, `SampleRate`,
  `Channels`, `BitsPerSample`, `Encoding`. **The list a picker needs already
  exists.**

**The heavy hand is still one click, one message.** This unit is the first that
builds a path from an operator's mouse to a real serial port. **Everything that
could put an unasked transmission on the air is in task 3**, and task 3 is
written to make that impossible rather than unlikely.

```
PHASE GOAL:   Hamlet works stations on the air.
UNIT GOAL:    An operator right-clicks a decoded row with a mouse and the menu
              appears under it, built at the moment of the click, offering every
              message valid at that point with the expected one marked, nothing
              greyed out, and each repeat carrying the count as of that click.
              Choosing one goes through the one command that arms, and nothing
              else does. The application builds a real armed send when - and
              only when - a real radio is connected and a transmit endpoint is
              named in Settings, and refuses with words otherwise. Settings can
              name that endpoint. Proved on this machine with no device opened
              and no port opened.
ADVANCES:     step 5 - exit criterion 2 (right-click a decoded row and the menu
              offers every message valid at that point, with the expected one
              highlighted and none forbidden, a repeat showing its count) taken
              in its letter for the first time, on a real control tree; and
              criterion 3's application-side half (choosing one transmits in the
              next slot with no confirmation) by giving the send path the port
              and the named endpoint it has never had, so that step 6's entry
              stops being blocked by code and starts being a radio, an antenna
              and Tim.
```

---

## Verify this instruction against the tree

**Everything below was read from the tree at 2026-09-06T23:3x by a session that
could not run the application.** Where this instruction and the tree disagree,
**the tree wins** - `PHASE_PLAN.md` says so in its own table. **Report the
mismatch in section 3 and continue. Do not repair the instruction and do not
stop.**

Unit 256 found one of these wrong and reported it at no cost; unit 258 found two;
unit 259 found three, one of which was this instruction's predecessor claiming
`Ft8Composer` called `Ft8SlotDecoder` when it does not. **That is the behaviour
that is wanted.**

**What I measured, with where I read it:**

| Claim | Where |
|---|---|
| **`ContextFlyout` appears zero times in `src/`**, in markup or in code. | `grep -rn ContextFlyout src/` returns nothing |
| The decoded table is `ItemsControl x:Name="DigitalDecodedRows"` at `:3425`, bound to `DigitalVisibleDecodes`, `DataTemplate x:DataType="vm:DigitalDecodeRow"`, root `Grid ColumnDefinitions="76,48,48,54,*,148"` at `:3444`. **That `Grid` is the row and it is where a flyout attaches.** | `src/Hamlet.App/Views/MainWindow.axaml` |
| **`MainWindow.axaml.cs` is 114 lines.** `MainWindow.axaml` is 3,642. **Unit 259's report said the flyout needed "a code-behind handler in a 3,600 line file"; the 3,600-line file is the markup and the code-behind is 114 lines.** The objection that dropped it is smaller than the record says. | both files, `grep -c ""` |
| The code-behind already owns view-only facts and already adds a handler in its constructor: `AddHandler(KeyDownEvent, OnTuneKey, ...)` at `:20`, and `OnTuneKey` reads `DataContext is MainWindowViewModel vm` at `:83`. **The shape you need is already there twice.** | `src/Hamlet.App/Views/MainWindow.axaml.cs` |
| `public Ft8SendMenu? SendMenuFor(DigitalDecodeRow? row)` at `:7934`; returns null where the row names no station or the ledger has no record; otherwise `Ft8SendOptions.For(record, callsign, grid, MeasuredReport(row))`. **It reads the ledger when it is called**, so a menu built at click time carries counts as of the click. | `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` |
| `Ft8SendMenu(string Callsign, IReadOnlyList<Ft8SendOption> Options, IReadOnlyList<string> Absent)`; `Ft8SendOption(Ft8SendShape Shape, string Text, string Label, bool IsExpected, int SentBefore)`. | `src/Hamlet.RadioEngine/Contacts/Ft8SendOptions.cs:30`, `:40` |
| `[RelayCommand] private void SendMessage(string? text)` at `:7982`, generating `SendMessageCommand`. **`_armedSend.Arm` is called from it and from nowhere else.** | same view model, `:8017` |
| `private Ft8ArmedSend? _armedSend` at `:7896`. Read at `:8004`, `:8045`, `:8075`, `:8081`. **Assigned at `:8228` only, by `internal void UseArmedSendForTests(Ft8ArmedSend?)`.** No `new Ft8ArmedSend` exists in `src/`. | same view model |
| The refusal an operator sees today, verbatim: *"Hamlet composed \"...\" and sent nothing: no radio is connected and no transmit audio device is named in Settings."* | same view model, `:8009`-`:8011` |
| `private static IRig CreateRig(string selection)` at `:9827` returns `new TrainingRig()` for the training entry and otherwise `new Ic7300Rig(new SystemSerialPort(selection))`. **The `SystemSerialPort` is not kept.** Called once, from `ConnectToAsync` at `:6501`. | same view model |
| `private IRig? _rig` at `:218`, set on a successful connect and nulled in the disconnect `finally` at about `:9821`. | same view model |
| **`Ic7300Rig` holds its `ISerialPort` in a private field at `:39` and exposes no accessor.** | `src/Hamlet.RadioEngine/Rig/Ic7300Rig.cs` |
| `AppSettings.AudioInputDeviceId` at `:193`; **`AudioOutputDeviceId` at `:215`, and `grep -rn AudioOutputDeviceId src/` finds only its own declaration.** | `src/Hamlet.App/Settings/AppSettings.cs` |
| `WasapiTransmitSink(string device, int bufferMilliseconds = 200)` at `:110`. It takes an endpoint **id or exact friendly name**, throws on null or whitespace at `:112`, and throws where no such endpoint exists at `:119`. **It never plays to the machine default.** | `src/Hamlet.RadioEngine/Audio/WasapiTransmitSink.cs` |
| `public static IReadOnlyList<RenderEndpoint> Endpoints()` at `:243`; `RenderEndpoint(Id, Name, IsDefault, SampleRate, Channels, BitsPerSample, Encoding)` at `:26`. | same file |
| The Settings audio input picker is a `ComboBox` at `SettingsWindow.axaml:203`, `ItemsSource="{Binding AudioDevices}"`, `SelectedItem="{Binding AudioDevice}"`, `IsEnabled="{Binding HasAudioDevices}"`, with a note `TextBlock` above it at `:199`. The view model side is `AudioDevices` at `SettingsViewModel.cs:221`, `HasAudioDevices` at `:224`, chosen at `:151` and written back at `:281`. **Copy that shape; do not invent a second one.** | `src/Hamlet.App/Views/SettingsWindow.axaml`, `src/Hamlet.App/ViewModels/SettingsViewModel.cs` |
| `DigitalSendLicenceLine` at `:8164` and `HasDigitalSendLicenceLine` at `:8183` already carry `TransmitGuard`'s own `Reason` and `Citation`, **for display and never for permission** - the refusal that stops a transmission is inside `Ft8TransmitSequence.RunAsync`. Both are already bound in the Send area at `MainWindow.axaml:3112`-`:3113`. | view model and markup |
| `DigitalSendUnset` at `:8196` says out loud when the grid is not set. Bound at `:3118`-`:3120`. | same |
| The CQ button is `Button x:Name="DigitalSendCqButton"` at `MainWindow.axaml:3095`, `Command="{Binding SendCallToAnyoneCommand}"`. **Criterion 1 is met and is not yours.** | same markup |
| `AddDecodeRowForTests(utc, snr, dt, hz, message, slotStartUtc)` lets a test put a row on the view model **without opening a window**. Units 258 and 259 both used it. | view model, about `:7810` |
| Headless Avalonia is already set up: `Avalonia.Headless.XUnit` 11.3.0 in the test project, `[assembly: AvaloniaTestApplication(typeof(HeadlessApp))]` and `HeadlessApp.BuildAvaloniaApp()` in `tests/Hamlet.App.Tests/Views/BindingHealthTests.cs:9`-`:21`, and ten existing files use `[AvaloniaFact]`. **A real control tree can be built in a test on this machine. That is what makes criterion 2 reachable in its letter tonight.** | `tests/Hamlet.App.Tests/` |
| The scene corpus is `tests/fixtures/ft8/scenes/unit257-band-scene.corpus.txt` - 21 decode lines over twelve slots, five booked stations `G4XYZ`, `VK2PQ`, `K9RST`, `W1ABC`, `N5TT`, operator `KC3QIS`. | unit 258's report §3.1, verified against the file |
| Unit 259's predicted menus, with the exact texts, the expected one and the repeat counts for all five stations at the boundary of slot 13. **Reuse the prediction; do not write a second one.** | `docs/unit259-send-path-trace.md` §6 |
| The root version reads `1.12.105`. | `Directory.Build.props:205` |

**Expected failures: none in the build.** The tree was green at `HEAD 2a3a90a`
with unit 259's 26 new tests passing. **If the build is red before you have
changed anything, that is a finding worth its own paragraph** - say so and say
what it was.

**`tools/unit254-seam-grep.sh` is still there, untracked. Leave it. No fifth
deletion attempt.**

---

## What the reload measured, and what to do about it

**Five entries, none of them yours to repair.**

1. **`RULES_AT` disagrees with `CLAUDE.md` §1** - `PROJECT_STATUS.md` says
   `HM-DEC-157 (2026-09-06)`, the reload reads §1's highest as `CPS-DEC-0152`.
   **Units 255 through 259 have all answered this:** §1's table is `HM-DEC-`
   throughout, `CPS-DEC-` appears nowhere in this repository, and
   `HM-DEC-155/156/157` are in `DECISIONS.md` but not yet indexed into §1's
   table. **`RULES_AT` is ahead of the index, not ahead of the record. Change
   nothing. Do not re-derive it a sixth time.**
2. **`PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` are modified and
   uncommitted at the root.** **Commit them at the start of task 1**, the way
   units 256 through 259 committed their predecessors' leftovers.
3. **`SESSION.lock` is deleted and the deletion is unstaged.** It is the
   launcher's file. **Commit the deletion with the others and do not recreate
   it.** If committing it is refused, leave it and say so in one line; do not
   spend a second attempt on it.
4. **`.run-unit/` is the launcher's working area** and changes under your feet -
   `watched.rc` is new and untracked. **Include it or leave it as you find
   convenient, and do not spend a second on it either way.**
5. **`PHASE_STATUS.md`'s `CURRENT_STEP:` reads 1** and is stale. **It is the
   launcher's field.** Do not write `CURRENT_STEP:`, `HEARTBEAT:` or the `STEP:`
   lines in `PHASE_STATUS.md`. `WORK_INSTRUCTION:` is yours to set.
   `PHASE_OUTCOME.md`'s header **is** yours - task 5.

**And one about the record itself.** `PHASE_OUTCOME.md` holds several pairs of
entries describing the same unit twice with different judgment fields, and one
entry whose fields were computed from a report that was never written. **All of
them are on the record and none of them is yours to reconcile. Do not edit an
entry that is not your own. Append yours.**

---

## Steps 1, 2, 3 and 4 are closed and are not yours

**Do not reopen them, do not improve them, do not add a test to them.**

- **Step 1**, the abort: `partial` at four of five. `TransmitAbort` fires from
  inside `Ft8TransmitSequence` and needs nothing from you.
- **Step 2**, the waveform: `partial`, 112 of 117 read back. Criterion 4 deferred
  to Tim.
- **Step 3**, the audio path: `partial`. The loopback decoded 3 of 3 on the device
  route in unit 256. **What remains of step 3 is the drive level Tim reads off the
  radio.** Note carefully: **step 3's criteria are not what you are moving.** You
  are moving step 5's criterion 3, which is a different sentence about the same
  machinery - *choosing one transmits in the next slot* - and it is unmet on the
  application side because the application never builds a sender.
- **Step 4**, the ledger: `partial` at five of six. **You call the ledger. You do
  not change it, and you do not re-argue the four-slot gone-quiet threshold.**

**What step 5 already has, from unit 259, and you do not rebuild:** the option
list, the expected-message rule, the repeat counts, the arming, the one-click
assertion on the engine side, the CQ button, the Send area, the licence line and
the grid-unset line. **All of it is built and tested. You are adding the mouse
and the wire, and re-asserting that adding them did not break one click, one
message.**

---

## Rulings in force

**Transcribed from `PHASE_PLAN.md`. Not to be re-argued by any unit, including
this one.**

**The dummy load is withdrawn.** HM-DEC-008 and HM-DEC-098 superseded,
2026-09-06. **Do not reference it, do not propose it, do not treat its absence as
a risk.** *Tim operates a licensed station on an antenna and Hamlet transmits on
the air.*

**One click, one message.** Ruled 2026-09-06. Hamlet transmits because the
operator clicked. **Never on a timer, never on a decode, never to continue a
contact.** *This unit builds the first path from a mouse to a real port. Task 3
is written for that reason.*

**Right-click sends immediately, in the next slot, with no confirmation.** Ruled
2026-09-06. **No dialog, no "are you sure", no second click.** *Task 2 is the
right-click itself, and it adds no confirmation of any kind.*

**Nothing is forbidden in the menu.** The expected next message is highlighted;
everything valid stays clickable; **a repeat is correct behaviour and shows its
count.** FT8 loses transmissions constantly, so sending the grid a second time is
correct behaviour, not a mistake to be greyed out. *In a flyout that means no
`IsEnabled="False"` on an option, ever.*

**A contact is never closed by the app.** Complete is shown when the exchange has
what a QSO needs. **`73` is politeness, not a requirement.** **A complete contact
still offers `73`; a gone-quiet one still offers everything.**

**Licence privileges.** Hamlet never transmits outside them. **The Settings gate
is not bypassable from any send path.**

**The abort.** Every path that keys the transmitter has a same-thread, no-await
abort. **No unit ships a keying path before its abort is watched to fire.** It was
watched in unit 253 and it is inside `Ft8TransmitSequence`; **you add no second
route to a keying frame.**

**`Ft8Sharp` is a faithful MIT port and nothing in this phase changes a line of
it.** `Ft8Sharp.Deep` is GPL-3.0.

**The engine is not told that tabs exist** (§0.1).

**Nothing interprets a message** (§12.1). **A row's state is derived from which
messages passed between two callsigns, which is bookkeeping, not meaning.**

**A unit may not add a test without naming the breakage it would have caught.**

**Known reds, inherited, never chased:**
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list
tripwire.

---

## Status cadence

**This is the section that kept unit 257's work out of the tree and kept units
258 and 259 in it. Read it twice.**

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` **read from the clock,
never composed**, and `NOTE` saying what is moving inside the task.

**And in addition, all of these:**

- **Before every `dotnet build` and every filtered `dotnet test`.**
- **After every file you finish writing**, not after every batch of them.
- **Never more than eight minutes apart.** The watchdog fires at twelve, measured
  from the launch clock and not from your last output.

**A headless Avalonia test is slower to start than an engine test.** Write the
status line *before* you launch task 2's filtered run, not after it.

**Use the file-editing tools if the shell refuses.** Unit 253 wrote three
`UPDATED` values it composed, one of them 39 minutes ahead of the real clock.
**Read the clock.**

---

## Tasks

Five tasks. **Task 1 is bounded and mostly reading. Task 2 is the goal task and
task 3 is the dangerous one.** **Task 4 is the named drop candidate.**

### Task 1 - the two routes, measured. THE TRACE.

**Commit the uncommitted root records first**, as named above, before you touch
anything.

Then measure, and write it to `docs/unit260-route-trace.md`. **Every answer gets
a file and a line number.** Five questions and no others. **This is a trace, not
a survey. Unit 257 died writing a survey.**

1. **Where does a right-click on a decoded row arrive today?** Name the control
   in the row template that would receive it, its line, and whether anything in
   the tree handles `ContextRequested`, `PointerReleased` or a right button
   anywhere in `MainWindow.axaml` or its code-behind. **If the answer is nothing,
   say nothing and give the grep.**
2. **What does the row's `DataContext` carry at the moment of a right-click, and
   is it enough to call `SendMenuFor`?** Name `DigitalDecodeRow`'s members that
   `SendMenuFor` and `MeasuredReport` read, with lines. **Say whether the row
   object the template is bound to is the same instance the view model holds**,
   because that is what decides whether the handler can pass it straight through.
3. **What is the smallest seam that keeps the `SystemSerialPort` the app already
   constructs?** Read `CreateRig` at `:9827` and `ConnectToAsync` at `:6501`.
   Name the change in lines, and **name what happens on the training-radio route,
   which has no serial port at all.**
4. **What would the sink be constructed from, and where would it be constructed?**
   `AudioOutputDeviceId` at `AppSettings.cs:215`, `WasapiTransmitSink`'s
   constructor at `:110`, and what it does with an empty or unknown name at
   `:112` and `:119`. **Say plainly what a wrong or stale name does to an
   operator who clicks**, because that is the failure this unit has to make loud.
5. **Every place a transmission can begin, today.** Grep `src/` for `Arm(`, for
   `_armedSend`, for `Ft8TransmitSequence`, and for `RunAsync` on it. **List every
   hit with its line and say which is a call site and which is a declaration.**
   This is the baseline you will hold task 3 against: **the same grep after task 3
   must show one arming call site, and it must be the same one.**

**Commit the trace before task 2 starts.**

### Task 2 - the menu under the mouse. THE GOAL TASK.

**Criterion 2, in its letter, for the first time.** Right-click a decoded row and
the menu appears.

**The shape, and it is decided:** a `ContextRequested` handler in
`MainWindow.axaml.cs`, which takes the row out of the sender's `DataContext`,
calls `vm.SendMenuFor(row)` **at that moment**, builds the items from what comes
back, and shows them. **Not a bound property, not a static list built when the
row was created.** That distinction is the whole reason unit 259 dropped this and
it is the thing to get right: *the repeat count belongs to the click, not to the
row.*

What the menu shows, and every one of these is a ruling above, not a preference:

- **Every option in `Ft8SendMenu.Options`, in the order they arrive.** All of them
  clickable. **No `IsEnabled="False"` on an option, ever, for any reason** -
  not for the contact state, not for the licence, not for a repeat.
- **The expected one marked**, where `IsExpected` is true. Mark it in a way a
  reader sees without hovering; the text of the mark is yours.
- **A repeat carries its count** where `SentBefore > 0`, in words - unit 259's
  report already uses *acknowledge, 2nd time* and the trace predicts it for two
  stations. Use the same wording.
- **`Ft8SendMenu.Absent` reasons appear as an unclickable note**, not as greyed
  options. **An absent message is a message that does not exist, not a message
  withheld** - say that distinction in the code's own remarks.
- **`DigitalSendLicenceLine` appears as a note when `HasDigitalSendLicenceLine`**,
  which is criterion 6's first half in the place criterion 6 asks for it. **Every
  option stays clickable underneath it.** The refusal that stops a transmission
  is inside `Ft8TransmitSequence` and stays there.
- **Choosing one calls `SendMessageCommand` with the option's `Text` and does
  nothing else.** No second route, no dialog, no confirmation.
- **A row with no station record produces no menu**, and a right-click on it does
  nothing visible rather than showing an empty box.

**Watch it red first, and this is the red that matters:** build the flyout once
from a menu captured when the row was created, assert a station's repeat count,
send that station a message through `SendMessageCommand`, right-click again, and
**watch the count fail to move.** Quote that failure. Then move the call to the
handler and watch it move. **The breakage that test catches is a menu that lies
about what has already gone out**, which is exactly the fault that would make an
operator send a third `RRR` thinking it was his first.

**Assert it on a real control tree**, headless, with `[AvaloniaFact]` and the
`HeadlessApp` that already exists. Build the window or the row template, raise
the context request, read the items back, and compare the texts, the mark and
the counts **against `docs/unit259-send-path-trace.md` §6's committed
prediction** for all five stations. **That prediction was committed before the
option list existed and it is what makes this a measurement.**

**If headless Avalonia cannot raise a context request on this machine** - it is
the one thing here I could not run - **do not spend the night on it.** Fall back
to: assert the handler's own method directly with a row and a view model, on the
same five stations, and assert separately that the markup carries the flyout by
reading `MainWindow.axaml` in a test the way `EveryResourceKeyResolvesTests`
reads it. **Say in the report which route you took and why.** A criterion closed
on the second route is still closed with its figure and what was tried.

### Task 3 - a real route to a real radio. THE DANGEROUS TASK.

**This is where an unasked transmission would come from. Read it twice.**

Two joins, and nothing else:

**The port.** Keep the `SystemSerialPort` the application already constructs at
`:9830`, in a field beside `_rig`, set where the rig is set and **nulled in the
same `finally` that nulls `_rig`**. The training radio has no port and therefore
**sets nothing** - `TrainingRig` is a simulator and must never become a route to
a keying frame. Do not add an accessor to `Ic7300Rig`; unit 259's trace already
settled that the field beside `_rig` is the smaller seam and task 1 question 3
confirms it or corrects it.

**The sink.** Construct it from `AppSettings.AudioOutputDeviceId`, **through a
factory the tests can substitute** - a `Func<string, ITransmitAudioSink>` on the
view model defaulting to `name => new WasapiTransmitSink(name)`. **The default is
never invoked by any test in this unit**, and that is asserted: a test hands a
factory that records its calls and proves it was never called when the setting is
empty.

**Then `_armedSend` is built when, and only when, both exist.** Where either is
missing, `_armedSend` stays null and the click lands on the refusal at `:8009` -
**which is already correct and already worded.** Widen its wording only enough to
say which of the two is missing, and say it in the operator's terms.

**Five assertions, all of them without opening anything:**

1. **No port, no sink: nothing is armed and the sink factory is never called.**
   Watch it red first by building the armed send unconditionally and quoting what
   happens - **a `WasapiTransmitSink` constructor reached on a machine with no
   such endpoint**, which is an exception in a click handler.
2. **Training radio: nothing is armed**, even with an endpoint named. Quote the
   refusal.
3. **Port and sink both present, with fakes: exactly one transmission is armed**,
   and it goes through `SendMessageCommand` and no other route.
4. **One click, one message, re-asserted through the new construction path.**
   Unit 259 proved it against a hand-injected `Ft8ArmedSend`; prove it again
   against one this code built. **Two boundaries, one transmission.** Watch the
   red: without the clear, one click and three boundaries put three transmissions
   on the wire - unit 259 quoted that and you should be able to reproduce it.
5. **The grep from task 1 question 5, run again, is unchanged in shape:** one
   call site for `Arm`, and it is `SendMessage`. **Quote both greps side by
   side.** If task 3 added a second, you have built the fault this phase exists to
   prevent and the report says so before anything else.

**A stale or wrong endpoint name is the operator-facing hazard here.** The sink
throws where the endpoint is not found. **A click must not put an exception in
front of an operator**: catch it at the construction site, leave `_armedSend`
null, and let the Send area say the named device was not found. **Assert that
with a factory that throws.**

### Task 4 - naming the endpoint in Settings. **THE DROP CANDIDATE.**

A `ComboBox` in `SettingsWindow.axaml`, beside the input one at `:203`, bound to
a transmit-endpoint list on `SettingsViewModel` built from
`WasapiTransmitSink.Endpoints()`, written back to `AppSettings.AudioOutputDeviceId`
the way `AudioInputDeviceId` is written at `SettingsViewModel.cs:281`. **Copy the
existing shape exactly, including the note above it.** The note says what this
device is for in the operator's words: *the radio's own USB audio input, which is
what carries FT8 out of the computer.*

**Enumeration happens in the Settings view model at run time and you never call
`Endpoints()` from a test.** Where the list is empty the box is disabled and the
note says so, exactly as `HasAudioDevices` already does.

**If the night is short, this is what goes.** Say so in section 3 and say what
remains. **The fallback is real and must be named in the report if you drop it:**
`AudioOutputDeviceId` is persisted in `%AppData%\Hamlet\settings.json` and Tim can
put the endpoint's name in it by hand, so **step 6 is still attemptable without
this task.** That is why it is the drop candidate and not task 3.

### Task 5 - the record

`PHASE_OUTCOME.md`: **append one entry**, twelve fields, the exact format the
existing entries use, ASCII only, with the file-editing tools if
`outcome-append.bat` is refused for the eighth time. **Update the header's
`STEP:` lines to what you actually reached. Do not touch an existing entry.**

`PROJECT_STATUS.md`: final write, `WORK_INSTRUCTION: 260 - the menu under the
mouse, and a route to the radio`, clock read not composed.

Then `output.md`, per the reporting section below.

---

## Parked - do not touch, do not raise

**From `PHASE_PLAN.md`'s own list**, and it is not open for discussion: automatic
sequencing - *if I get a response, send `73`* is **deliberately out of this phase**
and is the single most tempting thing to build tonight, and it is more tempting
tonight than it has ever been because tonight the click reaches a wire; logging
(FG-004); FT4, PSK31 and WSPR transmit; CW send; the OSD re-encoding count;
`ReusableWindow`; `ProcessDelayForTests`; the tap's owner; the waterfall's first
row; unit 237's Extensible conclusion; work instruction 231's four tree items;
`validate-output.bat`'s permitted-spellings bug; the 101.33 ms pulse above 6 kHz;
the CW decoder and its inherited reds.

**And these seven, carried from units 253 to 259:**

- **`TransmitGuard.Check` permits when the operator's toggle is off.** Banked and
  the owner's. **The send path already treats that as a refusal inside itself and
  changes nothing about the guard.** Not re-raised, not widened.
- **Five routes in the tree reach a keying frame**, including `AutoCaller` at
  `Cw/AutoCall.cs:272`, which keys repeatedly from one operator start. **On the
  parked CW path. Logged. Do not touch it, and do not copy its shape** - it is
  the exact shape task 3 must not become.
- **`SetSettingAsync(CivWrites.AntennaTuner, CivWrites.TuneNow)` writes
  `1C 01 02`**, a tuning cycle that transmits, called by no line in the tree.
  **Logged. Parked.**
- **The IC-7300's USB modulation input level.** Deferred to Tim at step 2's
  criterion 4 and step 3's criterion 1. **Not this unit's and not inferable from
  this machine** - `SHACK_FACTS.md` FACT-004.
- **`ContactStage`, `SendOption` and `ContactShape`** on the parked CW path.
  **Do not reach for them and check for a collision before you name a type.**
- **The gone-quiet threshold of four slots** and the corpus's synthesized
  provenance. Both argued, both recorded, neither yours.
- **The CW auto-call block in `MainWindow.axaml` around `:495`-`:504` still
  describes sending into a dummy load** in user-facing text. Unit 253 reported it
  and left it as parked CW-send surface. **Logged again here so you do not
  rediscover it and chase it. It is not yours.**

---

## What not to do

**Citing rather than retyping where the rule is already written down.**

1. **Do not open a serial port and do not open an audio device.** FACT-004. This
   is narrower than it was last unit and the line is exact: **you may write the
   code that would open them; you may not execute it.** No test constructs a
   `WasapiTransmitSink` or a `SystemSerialPort`. Every transmission in this unit
   goes to a fake through a substituted factory. **A device opened here measures
   this machine and says nothing about the radio.**
2. **Do not change a line of `src/Ft8Sharp/` or `src/Ft8Sharp.Deep/`.**
3. **Do not change `Ft8TransmitSequence`, `TransmitAbort`, `TransmitGuard`,
   `WasapiTransmitSink`, `Ft8Composer`, `Ft8ContactLedger`, `Ft8ContactStates`,
   `Ft8SendOptions` or `Ft8ArmedSend`.** **Call them. Read them. Leave them.**
   Unit 259 built the last four for exactly this caller. If one genuinely cannot
   be called without a change, that is a finding for section 3 with the line
   quoted - not a licence to edit it.
4. **Do not add a second route to a keying frame, and do not add a second route
   to `Arm`.** Task 1 question 5 takes the baseline and task 3 assertion 5 holds
   you to it.
5. **Do not build automatic sequencing.** No *if he answers, send the report*. No
   retry. No continuation. **One click, one message.**
6. **Do not add a confirmation.** Ruled 2026-09-06. No dialog, no "are you sure",
   no armed state he has to approve.
7. **Do not disable, grey, hide, sort away or forbid an option in the flyout**,
   on the contact state, the licence, a repeat count or anything else. Ruled
   2026-09-06 and asserted by unit 259's reflection test over the option types.
8. **Do not interpret a message.** §12.1. A label naming a field shape is
   arithmetic; wording what a station meant is not.
9. **Do not invent a grid, a callsign, a frequency, a licence class or a device
   name.** §0.0. **In particular: do not fall back to the default audio endpoint.**
   `WasapiTransmitSink` refuses to, on purpose, and playing FT8 into laptop
   speakers because a name was missing is the failure that refusal exists to
   prevent. **An unset endpoint is a refusal with words, never a guess.**
10. **Do not write a second predicted-menu table.** Unit 259 committed one before
    the option list existed; that is what makes it evidence. Read it and compare.
11. **Do not edit `PHASE_OUTCOME.md`'s existing entries**, including the several
    that disagree with each other. Append yours; leave the record.
12. **Do not run an unfiltered `dotnet test`, and do not background a command and
    poll for it.** The rules at the top killed four sessions.
13. **Do not chase the known reds.**
14. **Do not hand-write `HEARTBEAT:`, `CURRENT_STEP:` or the `STEP:` lines in
    `PHASE_STATUS.md`.**
15. **Do not re-derive `RULES_AT`.** Five units have answered it.
16. **Do not write a survey.** Task 1 is five questions with line numbers.

---

## Committing and pushing

Commit and push **each task before starting the next**. Bump the root version's
patch by one per task from `1.12.105`, so `1.12.106` through `1.12.110`.
**`Ft8Sharp` does not move.**

**Task 1 commits twice**: once for the uncommitted root records exactly as they
arrived, and once for the trace - **and task 1 question 5's baseline grep must be
in a commit before task 3 starts.** That is what makes assertion 5 a comparison
rather than a claim.

---

## Logged, not chased

**Unit 259's section 4 raised nothing and asked for no ruling.** It said in one
sentence that nothing is blocking. **Nothing is banked from the last unit.**

Five things carried forward so they are not lost, none of them a ruling request:

1. **`validate-output.bat` could not be started in eleven forms across five
   units.** Logged, and the tool rule tells you what to do about it.
2. **Unit 259's report called `MainWindow.axaml.cs` a 3,600 line file.** It is
   114 lines; the markup is 3,642. **Corrected here, logged, and it is why task 2
   is affordable tonight.** Not a fault of that unit's work - it dropped the
   flyout for a second and better reason, the repeat count, which task 2 solves
   properly.
3. **`AudioOutputDeviceId` has had no reader since it was written**, on purpose,
   waiting for you. Task 3 is where it stops being unreachable.
4. **`BoundariesBetween` includes `from` when `from` is itself a boundary.**
   Measured by unit 258. **Relevant to you** if task 3's assertions count
   boundaries - read `Ft8Slots.cs:209-232` before you write that arithmetic, and
   do not write a second copy of it.
5. **`_armedSend` is assigned only by a test seam today**, so every existing test
   of the send path proves the path and not the wiring. **That is the gap task 3
   closes**, and it is worth one sentence in the report saying so.

---

## Reporting

`output.md` at the repository root, four sections per `CLAUDE_CODE.md` §8.

**First, above everything, the ordering block. `validate-output.bat` refuses a
report without it.** It must carry the literal words `READ IN THIS ORDER`, then
three paragraphs beginning `A.`, `B.` and `C.` at the start of a line, all inside
the first 60 lines of the file, and **C must contain the literal phrase `raises N
items`** with a real number.

- **A - the phase goal and where every step stands.** Hamlet works stations on the
  air. Step 0 `done`. Steps 1, 2, 3 and 4 `partial` and closed, **and say that
  what is outstanding in each is either on the record or a figure only Tim can
  read off a radio.** **Step 5 entering this unit at `partial` with criteria 2 and
  3 named as its two gaps, and leaving it at whatever you actually reached.** Step
  6 not started - **and this is the sentence that matters this time: say plainly
  whether Tim can now right-click a row and have the click reach a radio if one is
  connected and an endpoint is named, or what still stands between him and
  trying.**
- **B - step 5's six exit criteria and which were met.** In the plan's own words:
  a CQ button from the operator's own settings, no typing; right-click offers
  every message valid at that point with the expected one highlighted and none
  forbidden, a repeat showing its count; choosing one transmits in the next slot
  with no confirmation; one click sends exactly one message, asserted by a test;
  what is being sent and to whom in the reserved Send area; out of licence
  privileges the menu says so and sends nothing. **One line each, on quoted
  evidence.** Criteria 1, 4, 5 and 6 stood met entering this unit - **say whether
  they still stand after the wiring, because that is not free.** For criterion 2
  say whether the menu was reached with a real context request on a control tree
  or by the fallback route, and which. **For criterion 3 say exactly what is now
  wired, what is still proved on a fake, and what a real radio would change.**
- **C - what this report adds, weighed against A and B.** How many items section 4
  raises, in the words `raises N items`, and **whether any of them is in the way of
  a criterion named in B.** If none is, say so - that is a real answer and it is
  not a ruling request.

**Then the six-line header block**, from the clock, never composed:
`UNIT:`, `PHASE GOAL:`, `UNIT GOAL:`, `ADVANCED:`, `NUMBER:`, `DRIFT:`.

**`NUMBER:` for this unit is how many of the corpus's five stations the flyout
put the predicted menu under the mouse for, of five** - and *predicted* means
`docs/unit259-send-path-trace.md` §6, committed before the option list existed.
**If you took task 2's fallback route, `NUMBER:` counts the same five stations
through that route and the report says so in the same line.** **`DRIFT:` - unit
259 reported 0.**

**Section 3 leads with three things, in this order:**

1. **That no new route to a transmission exists.** The task 1 question 5 grep and
   the same grep after task 3, **quoted side by side**, showing one arming call
   site and that it is `SendMessage`. Then the four remaining assertions of task
   3: nothing armed with no port, nothing armed on the training radio, the sink
   factory never called with an empty setting, and one click producing exactly one
   transmission across two boundaries through the send path this unit built -
   **with the red you watched first for each.** **This is the phase's one
   unrecoverable fault and this paragraph is the evidence it cannot happen.** It
   comes first for that reason and not because it is the largest piece of work.
2. **The right-click, station by station.** All five, one block each: what
   appeared under the mouse, which was marked expected, which carried a repeat
   count, what the absent note said, and unit 259's prediction beside it. **The
   station the ledger reads as `complete` gets its own paragraph** showing `73`
   and everything else still under the mouse and nothing greyed. **And the red
   you watched first** - the menu built once at row creation, and the repeat count
   that did not move after a send. Quote it.
3. **What the app can now reach, and what it cannot.** Where the port is kept and
   where it is nulled; where the sink is constructed and from what; what happens
   to an operator who clicks with a stale endpoint name; what the Send area says
   in each case and what it said before. Then **whether task 4 shipped or was
   dropped** - if dropped, say so here, say that `AudioOutputDeviceId` can still be
   set by hand in `%AppData%\Hamlet\settings.json`, and say that step 6 is
   therefore still attemptable.

**Section 4 is for what is genuinely in the way.** *Nothing is blocking* is a real
answer and is written as one sentence. **A note, an observation or a recommendation
you have already acted on is not a ruling request** - put it in section 3. **A
criterion you could not fully reach is not a blockage either** - it is a figure and
what was tried, and `PHASE_PLAN.md`'s own table says to close it that way and
continue. Ask the owner to decide something only where work is actually stopped
until he does.

**Write `output.md` before you stop, for any reason at all** - complete, blocked,
failed or out of time are all reported the same way. Then stop. Do not start the
next unit.

---

```
ARBITER-DECISION
STEP: 5
APPROACH: Put the ContextFlyout on the decoded row and wire the send path to a real serial port and a named output endpoint chosen in Settings
MOVE: continue
WHY: Step 5 closed partial with two named, deliberate gaps that no unit has attempted - no ContextFlyout exists anywhere in src, so criterion 2's operator gesture has never happened, and _armedSend is assigned only by UseArmedSendForTests, so on every real run the application has no sender at all and criterion 3 cannot occur outside a test. Both are step 6's entry conditions and neither needs a radio to build. The loop test was run on this approach and returned NOT FOUND against all fourteen entries; the two step 5 entries are one unit recorded twice and both read "proved against the fake port and fake sink with no device opened", so this is the deliberate complement of what was tried and the remainder that unit named, not a repeat of it.
STATE: partial
DECIDED: Four on my own authority. First, step 5 is taken a second time rather than the phase being called satisfied, because a must-pass written as an operator's gesture is not met by a method no view calls, and declaring victory early is the likelier failure. Second, the flyout is built by a ContextRequested handler that calls SendMenuFor at the moment of the click rather than binding a list, which answers the repeat-count objection that caused unit 259 to drop it; I also measured that MainWindow.axaml.cs is 114 lines and not the 3,600-line file that report named, so the objection is cheaper than the record says. Third, the sink is constructed through a substitutable factory defaulting to WasapiTransmitSink so that the wiring is asserted without any test opening a device, and no test constructs a real sink or a real port - FACT-004 stands and the last mile is still step 6's. Fourth, the Settings endpoint picker is the drop candidate rather than the wiring, because AudioOutputDeviceId is persisted in settings.json and can be set by hand, so dropping it leaves step 6 attemptable while dropping the wiring would not.
LICENCE: PHASE_PLAN.md's rulings of 2026-09-06 - one click one message, right-click sends immediately in the next slot with no confirmation, nothing forbidden in the menu, and Tim operates a licensed station on an antenna and Hamlet transmits on the air - together with its named alternatives to stopping: the tree wins, report the mismatch and continue, and a target not reached is closed with the figure reached and what was tried. The steps are a hypothesis not a contract licenses taking a partial step again on its unattempted criteria. SHACK_FACTS.md FACT-004 licenses the substituted factory and forbids the inference that would otherwise close criterion 3 on this machine. The three the arbiter may not reason past license the ordering: the abort was watched to fire in unit 253 and lives inside Ft8TransmitSequence, the gate is inside it too, and this unit adds no second route to either.
ACCOMPLISHED: Tim right-clicks a decoded station with a mouse and the menu is there under it - every message valid at that point, the one that conventionally comes next marked, nothing greyed out, and each repeat carrying the count as of that click rather than as of the row. One of those clicks now reaches a real serial port and a real audio endpoint when a radio is connected and a device is named, instead of reaching a field nothing ever fills, and it still arms exactly one transmission and still refuses in words when either is missing. What is left after this is Tim, a radio and an antenna.
ADVANCES: step 5 - exit criterion 2 (right-click a decoded row and the menu offers every message valid at that point, with the expected one highlighted, none forbidden, and a repeat showing its count) taken in its letter for the first time on a real control tree, and criterion 3's application-side half (choosing one transmits in the next slot with no confirmation) by giving the send path the port and the named endpoint it has never had - which together clear what stands between step 6's entry and Tim attempting it.
END-ARBITER-DECISION
```

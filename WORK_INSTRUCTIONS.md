# Work instruction 259 - right-click, and it goes

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

**All four were checked against the tree at 2026-09-06T23:0x, at `HEAD e1f0b23`,
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
unit 257 at fourteen minutes with three files written and none committed. Unit
258 wrote `PROJECT_STATUS.md` eight times and lost nothing. **The status write is
part of the work, not part of the reporting.**

`dotnet build` is allowed, foregrounded, with a timeout.

**This unit opens no audio device, opens no serial port, and plays no sound.**
Every transmission in it goes to a fake sink and a fake port that already exist
in the tree. See *What not to do* §1.

---

## THE TOOL RULE

**This session's shell may refuse calls.** A refused shell call is a signal to
reach for the other tool, not to stop. **The file-editing tools have been
unaffected throughout.** Record every refusal verbatim. **Nothing in this unit
halts the loop.**

Three tool facts earlier units paid for, carried forward unchanged:

1. **`tools\arbiter\outcome-append.bat` has been refused for six consecutive
   units** - 253 through 258. **Expect a seventh.** Task 5 tells you what to do
   instead: append the entry with the file-editing tools, in the exact
   twelve-field format the existing entries use, ASCII only.
2. **`tools\arbiter\validate-output.bat` has been refused in nine forms** across
   units 255, 256, 257 and 258. **This shell may not start a batch file.** Try it
   once; if it is refused, check the report by hand against the rules in the
   reporting section below, say in the report that you did, and **quote no exit
   code**, because there is none.
3. **The shell has refused device enumeration, `powershell -NoProfile`,
   `git reset -q <paths>`, `sed -i`, and heredocs for commit messages.**
   `git restore --staged` and repeated `-m` flags do the same jobs. You need
   device enumeration for nothing in this unit.

---

## Why this unit exists

**The count today.** Step 0 `done`, one unit. Step 1 `partial` at four of five and
closed. Step 2 `partial`, criterion 4 deferred to Tim. Step 3 `partial`, four
units, closed on the loopback with the radio's own drive level deferred to Tim.
Step 4 `partial`, three units, closed last night at five of six with the sixth met
in substance. **Step 5 has had no unit spent on it. Step 6 cannot have one.**

**So this is the last step any unit can move.** Everything that remains unmet in
steps 1 to 4 is either closed on the record or is a number only Tim can read off a
radio that has never been attached to this machine. Step 6 is Tim keying a
transmitter. **Step 5 is the whole of what is left to build**, and after tonight
the phase either has an operator who can click and be heard, or it does not.

**What exists and is waiting to be joined.** These are not hopes; they were read
out of the tree while this instruction was written and the line numbers are below.

- `Ft8TransmitSequence.RunAsync(OperatorSend)` - gate, key, play, unkey, abort on
  every failing path. **Its own remarks say: "Nothing in this repository calls it.
  The first caller is step 5's right-click."** That sentence is tonight's work.
- `Ft8ContactLedger` and `Ft8ContactStates` - what passed each way with each
  station, and which of the four states that is. **`RecordSent` exists and nothing
  calls it**, put there so this unit adds one line beside the send.
- `Ft8Composer.ComposeSignal` - words into the 12.64 s that goes on the air.
- `DigitalSendReserved` - a named, empty, bordered region under the waterfall,
  kept clear for exactly this, carrying a line that says transmit is not built.
  **That line becomes false tonight.**

**The heavy hand is one click, one message.** Hamlet transmits because the
operator clicked - never on a timer, never on a decode, never to continue a
contact. A transmission he did not ask for is this phase's one unrecoverable
fault, because it goes out over other people's band and cannot be taken back.
**Everything in this unit that waits for a slot boundary is the place that fault
would come from**, and task 3 is written to make it impossible rather than
unlikely.

```
PHASE GOAL:   Hamlet works stations on the air.
UNIT GOAL:    The operator right-clicks a decoded row and Hamlet offers every
              message that is valid at that point, with the expected one
              highlighted, none forbidden, and a repeat showing its count.
              Choosing one arms exactly one transmission in the next slot, with
              no confirmation, and nothing further goes out without another
              click. A CQ button does the same from his own settings with no
              typing. What is being sent, and to whom, appears in the reserved
              Send area. Out of licence privileges the menu says so and sends
              nothing. Proved on this machine against a fake port and a fake
              sink, with no device opened.
ADVANCES:     step 5 - exit criteria 1 (a CQ button from the operator's own
              settings, no typing), 2 (right-click offers every valid message,
              expected highlighted, none forbidden, a repeat shows its count),
              3 (choosing one transmits in the next slot with no confirmation),
              4 (one click sends exactly one message, asserted by a test),
              5 (what is being sent, and to whom, in the reserved Send area)
              and 6 (out of privileges the menu says so and sends nothing).
```

---

## Verify this instruction against the tree

**Everything below was read from the tree at 2026-09-06T23:0x by a session that
could not run the application.** Where this instruction and the tree disagree,
**the tree wins** - `PHASE_PLAN.md` says so in its own table. **Report the
mismatch in section 3 and continue. Do not repair the instruction and do not
stop.**

Unit 256 found one of these wrong and reported it at no cost; unit 258 found two
and reported them at no cost. **That is the behaviour that is wanted.**

**What I measured, with where I read it:**

| Claim | Where |
|---|---|
| `Ft8TransmitSequence` is a sealed class taking `ISerialPort`, `ITransmitAudioSink`, an optional `TransmitGuard`, an optional `ITelemetry` and two CI-V addresses. | `src/Hamlet.RadioEngine/Transmit/Ft8TransmitSequence.cs:197`, ctor `:216` |
| `RunAsync(OperatorSend, CancellationToken)` returns `TransmitRun`. **It never throws and never leaves the radio keyed.** | same file, `:249` |
| `OperatorSend(Ft8Transmission, long FrequencyHz, LicenseClass, bool GuardEnabled, DateTime SlotStartUtc, double StartSecondsIntoSlot)`. **The slot's start and the offset arrive as values and are recorded, not waited for** - "a path that waits for a moment is a path that can arrive at one on its own". | same file, `:92`, and the remarks at `:182` |
| `TransmitRun.Sent`, `.CameOutOfTransmit`, `.RadioIsInReceive`, `.Reason`, `.Citation`. | same file, `:129`, `:132`, `:163` |
| The gate is **inside** `RunAsync`, before anything that can key, and is narrower there than `TransmitGuard.Check` is in general: an overridden permit and an unknown-class permit are both refusals inside this path. | same file, `:254`-`:263`, and the remarks at `:189` |
| `TransmitGuard.Check(LicenseClass, long frequencyHz, TransmitMode, bool guardEnabled)` returns `TransmitDecision(MayTransmit, Reason, Citation, WasOverridden)`. | `src/Hamlet.RadioEngine/Licensing/TransmitGuard.cs:67`, `:11` |
| `Ft8Composer.ComposeSignal(text, sampleRate, baseFrequencyHz)` is **the route that goes on the air**; `Compose` is the route that goes into a decoder. They differ in one call. | `src/Hamlet.RadioEngine/Transmit/Ft8Composer.cs:259`, and the remarks above it |
| `Ft8ComposeResult` carries `Composed`, `Transmission`, `Refusal` and `Explanation` - **a refusal, not an exception**. | same file, `:131`-`:150` |
| `Ft8ContactLedger(string operatorCallsign)`; `For(string?)` returns `Ft8StationRecord?`; `RecordHeard` `:196`; **`RecordSent(string?, DateTime)` at `:225` with no caller in `src/`**. | `src/Hamlet.RadioEngine/Contacts/Ft8ContactLedger.cs:152`, `:162`, `:183` |
| `Ft8StationRecord` exposes `Callsign`, `Heard`, `HeardToUs`, `Sent`, `LastHeard`, `LastHeardToUs`, `LastSent`, `SlotsSinceHeard(now)`, `SlotsSinceSent(now)`. | same file, `:19`-`:76` |
| `Ft8ContactStates.Read(record, nowUtc)` returns `Ft8ContactRead(Callsign, State, Slots)` with `.Words` and `.Text`; `IsComplete(record)` at `:186`; `GoneQuietAfterSlots = 4` at `:98`. | `src/Hamlet.RadioEngine/Contacts/Ft8ContactState.cs` |
| `Ft8MessageSplit.Split`, `.IsCallToAnyone`, `.IsGrid`, `.IsReport(text, out rogered, out decibels)` - **one set of parsing rules, moved into the engine by unit 258**. | `src/Hamlet.RadioEngine/Contacts/Ft8MessageSplit.cs:53`, `:86`, `:102`, `:114` |
| `Ft8Slots.SlotSeconds = 15`, `SlotStart(trueUtc)` `:182`, `IntoSlot(trueUtc)` `:195`, `TransmissionFits(secondsAfterBoundary)` `:162`, `BoundariesBetween` `:209`. **There is no `NextSlot` helper. Whatever you write for it, write it once.** | `src/Hamlet.RadioEngine/Audio/Ft8Slots.cs:119` |
| `MainWindowViewModel` holds `private Ft8ContactLedger? _contacts` at `:1100`, built in `ContactTextFor` at `:7863`, fed by `RecordHeard` at `:7867`. **Every row goes through `PlaceRow` at `:7768`.** | `src/Hamlet.App/ViewModels/MainWindowViewModel.cs` |
| `AddDecodeRowForTests(utc, snr, dt, hz, message, slotStartUtc)` at `:7810` lets a test assert row text **without opening a window**. Unit 258 used it and it worked. | same file |
| `MainWindowViewModel.LicenseClass` at `:2508` reads `_settings.Operator.LicenseClass`; `private long _frequencyHz` at `:447`, seeded from `_selectedBand.Band.JumpHz` at `:3098` and updated by `OnRigFrequencyChanged` at `:9042`. | same file |
| `private IRig? _rig` at `:217`; `internal void UseRigForTests(IRig?)` at `:6863`; the rig is built at `:9473` as `new Ic7300Rig(new SystemSerialPort(selection))`. | same file |
| **`Ic7300Rig` holds its `ISerialPort` in a private field at `:39` and exposes no accessor for it.** So the app has no route today to hand `Ft8TransmitSequence` a port. **Task 1 settles what the smallest seam is.** | `src/Hamlet.RadioEngine/Rig/Ic7300Rig.cs` |
| `AppSettings` has `AudioInputDeviceId` at `:193` and **no output device field at all**. So the app cannot name a transmit endpoint today. | `src/Hamlet.App/Settings/AppSettings.cs` |
| `OperatorProfile.Callsign` defaults to `"KC3QIS"` at `:44`; **`GridSquare` at `:92` defaults to `""`** and may well be empty on this machine. | `src/Hamlet.App/Settings/OperatorProfile.cs` |
| `DigitalSendReserved` is a `Border`, `MinHeight="72"`, at `MainWindow.axaml:3074`, holding one `TextBlock` named `DigitalSendReservedLine` at `:3082` whose text is *"Send lives here when it is built. Hamlet does not transmit yet..."*. **Its own comment says the transmit phase drops into it.** | `src/Hamlet.App/Views/MainWindow.axaml` |
| The decoded table is `ItemsControl x:Name="DigitalDecodedRows"` at `:3388`, bound to `DigitalVisibleDecodes`, with a `DataTemplate x:DataType="vm:DigitalDecodeRow"` whose root is a `Grid ColumnDefinitions="76,48,48,54,*,148"` at `:3407`. **That `Grid` is where a `ContextFlyout` goes.** | same file |
| The fakes already exist: `tests/Hamlet.RadioEngine.Tests/Transmit/FakeTransmitAudioSink.cs`, and a fake CI-V transport used by `TheUnkeyHappensWhateverGoesWrongTests` that can refuse to open, hang on a read, throw on a write, and die between the key and the unkey. **Reuse them. Do not write a third fake.** | `tests/Hamlet.RadioEngine.Tests/Transmit/` |
| The scene corpus is `tests/fixtures/ft8/scenes/unit257-band-scene.corpus.txt` - 21 decode lines over twelve slots, five booked stations `G4XYZ`, `VK2PQ`, `K9RST`, `W1ABC`, `N5TT`, operator `KC3QIS`. | unit 258's report §3.1, verified against the file |
| The root version reads `1.12.100`. | `Directory.Build.props:205` |

**Expected failures: none in the build.** The tree was green at `HEAD e1f0b23`
with unit 258's 51 new tests all passing. **If the build is red before you have
changed anything, that is a finding worth its own paragraph** - say so and say
what it was.

**`tools/unit254-seam-grep.sh` is still there, untracked, from unit 254. Leave
it. No fourth deletion attempt.**

---

## What the reload measured, and what to do about it

**Four disagreements, none of them yours to repair.**

1. **`RULES_AT` disagrees with `CLAUDE.md` §1** - `PROJECT_STATUS.md` says
   `HM-DEC-157 (2026-09-06)`, the reload reads §1's highest as `CPS-DEC-0152`.
   **Units 255, 256, 257 and 258 have all answered this:** §1's table is
   `HM-DEC-` throughout, `CPS-DEC-` appears nowhere in this repository, and
   `HM-DEC-155/156/157` are in `DECISIONS.md` but not yet indexed into §1's
   table. **`RULES_AT` is ahead of the index, not ahead of the record. Change
   nothing. Do not re-derive it a fifth time.**
2. **`PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` are modified and
   uncommitted at the root.** **Commit them at the start of task 1**, the way
   units 256, 257 and 258 committed their predecessors' leftovers. `.run-unit/`
   is the launcher's working area and changes under your feet; include it or
   leave it as you find convenient, and do not spend a second on it either way.
3. **`PHASE_STATUS.md`'s `CURRENT_STEP:` reads 1** and is stale. **It is the
   launcher's field.** Do not write `CURRENT_STEP:`, `HEARTBEAT:` or the `STEP:`
   lines in `PHASE_STATUS.md`. `WORK_INSTRUCTION:` is yours to set.
   `PHASE_OUTCOME.md`'s header **is** yours - task 5.
4. **Step 3's state is recorded twice and the two disagree** - unit 256's own
   entry reads `done`, the judging session's reads `partial` on criterion 1's
   radio half. **Both are on the record and neither is yours to reconcile.** The
   same is true of the `UNIT 5 - STEP 4` entry whose judgment fields describe a
   report unit 257 never wrote. **Do not edit either. Append yours.**

---

## Steps 1, 2, 3 and 4 are closed and are not yours

**Do not reopen them, do not improve them, do not add a test to them.**

- **Step 1**, the abort: `partial` at four of five. `TransmitAbort` fires from
  inside `Ft8TransmitSequence` and needs nothing from you.
- **Step 2**, the waveform: `partial`, 112 of 117 read back. Criterion 4 deferred
  to Tim.
- **Step 3**, the audio path: `partial`. The loopback decoded 3 of 3 on the device
  route. What remains is the drive level Tim reads off the radio.
- **Step 4**, the ledger: `partial` at five of six, the sixth met on a synthesized
  corpus. **You call the ledger. You do not change it, and you do not re-argue the
  four-slot gone-quiet threshold.**

**You need four things from all of that and only four:** `Ft8ContactLedger` says
what passed with a station, `Ft8ContactStates` says which of the four that is,
`Ft8Composer.ComposeSignal` turns words into the audio that goes on the air, and
`Ft8TransmitSequence.RunAsync` keys, plays and unkeys. **Call them. Do not change
them.**

---

## Rulings in force

**Transcribed from `PHASE_PLAN.md`. Not to be re-argued by any unit, including
this one.**

**The dummy load is withdrawn.** HM-DEC-008 and HM-DEC-098 superseded,
2026-09-06. **Do not reference it, do not propose it, do not treat its absence as
a risk.**

**One click, one message.** Ruled 2026-09-06. Hamlet transmits because the
operator clicked. **Never on a timer, never on a decode, never to continue a
contact.** *This unit is the one that could break it. Task 3 is written for that
reason.*

**Right-click sends immediately, in the next slot, with no confirmation.** Ruled
2026-09-06. **No dialog, no "are you sure", no second click.**

**Nothing is forbidden in the menu.** The expected next message is highlighted;
everything valid stays clickable; **a repeat is correct behaviour and shows its
count.** FT8 loses transmissions constantly, so sending the grid a second time is
correct behaviour, not a mistake to be greyed out.

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
messages passed between two callsigns, which is bookkeeping, not meaning.** That
sentence is your licence for the option list and it is also your limit: **you may
count what passed and say which message conventionally comes next in an FT8
exchange, which is format arithmetic. You may not say what anybody meant.**

**A unit may not add a test without naming the breakage it would have caught.**

**Known reds, inherited, never chased:**
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list
tripwire.

---

## Status cadence

**This is the section that kept unit 257's work out of the tree and kept unit
258's in it. Read it twice.**

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` **read from the clock,
never composed**, and `NOTE` saying what is moving inside the task.

**And in addition, all of these:**

- **Before every `dotnet build` and every filtered `dotnet test`.**
- **After every file you finish writing**, not after every batch of them.
- **Never more than eight minutes apart.** The watchdog fires at twelve, measured
  from the launch clock and not from your last output.

**Use the file-editing tools if the shell refuses.** Unit 253 wrote three
`UPDATED` values it composed, one of them 39 minutes ahead of the real clock.
**Read the clock.**

---

## Tasks

Five tasks. **Task 1 is bounded and mostly reading. Task 2 is the goal task and
task 3 is the dangerous one.** Task 5 carries the named drop candidate.

### Task 1 - what the send path can actually reach. THE TRACE.

**Commit the uncommitted root records first**, as named above, before you touch
anything.

Then measure, and write it to `docs/unit259-send-path-trace.md`. **Every answer
gets a file and a line number.** Six questions, and no others - this is a trace,
not a survey, and unit 257 died writing a survey:

1. **Where does the app know the frequency the licence gate must be asked
   about?** `_frequencyHz` at `MainWindowViewModel.cs:447` is seeded from the
   band's `JumpHz` and updated by `OnRigFrequencyChanged`. **Is it live and true
   while the Digital tab is running, or is it the band's jump frequency until a
   radio moves?** Say which, because criterion 6 is asserted against it.
2. **Can the app hand `Ft8TransmitSequence` an `ISerialPort` today?**
   `Ic7300Rig._port` is private at `:39`. **Name the smallest seam that gives the
   app one** - a property on `Ic7300Rig`, or the view model keeping the port it
   already constructs at `:9473`. **Do not build a second CI-V transport and do
   not open a port.** If the seam costs more than a few lines, say so and say what
   the send path does instead.
3. **What does the operator's settings supply?** Callsign, grid square, licence
   class, and the guard toggle - **the actual values on this machine**, not the
   defaults in the source. **`GridSquare` may be empty**; if it is, say so, and
   see the ruling below.
4. **Where would the transmit audio endpoint come from?** `AppSettings` has
   `AudioInputDeviceId` at `:193` and no output field. `WasapiTransmitSink` at
   `src/Hamlet.RadioEngine/Audio/WasapiTransmitSink.cs:110` takes a device name
   and **fails loudly rather than playing to whatever the machine defaults to**.
   Say what it would take to name one - **and open nothing.**
5. **Confirm the four markup facts** in the table above: `DigitalSendReserved` at
   `:3074`, `DigitalSendReservedLine` at `:3082`, `DigitalDecodedRows` at `:3388`,
   the row template's root `Grid` at `:3407`. Any that is wrong, say so.
6. **PREDICT THE MENU, BEFORE YOU BUILD IT.** For each of the five stations in
   `tests/fixtures/ft8/scenes/unit257-band-scene.corpus.txt`, read at the boundary
   of slot 13 - the moment unit 258 used - write down **every message the menu
   should offer that station, which one is the expected next one, and any repeat
   count.** One block per station. **Commit this before task 2 writes a line of
   the option list**, or the score is written after the fact and means nothing.

**`NUMBER:` is scored against question 6** and against nothing else: how many of
the five stations the option list produced the predicted set for, of five.

**The bound on this task, and it is a cap not a target: one build and one hour of
reading.** If a question cannot be answered from the tree, write down that it
cannot and why, and move on. **An unanswered question is a finding. A missing
task 2 is a lost night.**

### Task 2 - what may be sent, and which one is expected. THE GOAL TASK.

**A type in `src/Hamlet.RadioEngine/Contacts/` that, given a station record, the
operator's callsign and his grid, returns the ordered list of every message that
is valid at that point.** Suggested name `Ft8SendOptions`; the name is yours.

Each option carries, at least:

- **the message text exactly as it would go on the air** - `W1ABC KC3QIS FN00`;
- **whether it is the expected next one** - exactly one, or none where nothing is
  expected;
- **how many times that same message has already been sent to that station**, so
  the menu can say *grid, 2nd time*;
- **a short label a reader sees** - *grid*, *report*, *roger and report*,
  *acknowledge*, *73*. **A label is format arithmetic, not meaning** - it names
  which field shape the message is, the way `AddresseeHelp` names which field is
  the addressee. It never says what the station wants.

**And a route for the CQ**, which has no station record: from the operator's
callsign and grid alone, `CQ KC3QIS FN00`.

**NOTHING IS REMOVED FROM THE LIST, EVER.** A complete contact still offers `73`
and everything else. A gone-quiet one offers everything. **There is no method on
this type that can withhold an option**, and you assert that the way unit 258 did
- by reflection over every public member against the verbs *hide*, *close*,
*forbid*, *deny*, *block*, *grey*, *dim*, *disable*, *exclude* - and you say so in
the report in those words.

**Watch it fail first, and this is the breakage to name:** build it returning
**only the expected next message**, and watch the *nothing is forbidden* case go
red - a station the ledger reads as `complete` is offered nothing at all, so the
operator who wants to send `73`, or his grid a second time because the first was
lost, has no way to. **Then put the whole list in.** Quote the red and the green.

**Assert it against task 1's written-down prediction, station by station**, with
the sets quoted. It must not crash on the two messages the splitter refuses.

**The grid may be empty, and this is ruled rather than left to you.** With no grid
in Settings, the CQ is `CQ KC3QIS` - a legal FT8 message - and the grid options
are absent from the list with the reason said out loud in the Send area. **Never
invent, default or infer a grid** (§0.0). `FN00` in the plan is Tim's own square,
not a fallback.

### Task 3 - one click, one message. THE DANGEROUS TASK.

**A command in the app that takes one option and arms exactly one transmission at
the next slot boundary.**

What it does, in order: read the operator's callsign, grid, licence class, guard
toggle and the frequency from task 1's answer; `Ft8Composer.ComposeSignal` the
text; work out the next slot boundary from `Ft8Slots.SlotStart` and
`SlotSeconds`; build one `OperatorSend` with that boundary and the 0.5 s offset
unit 255 recorded; call `Ft8TransmitSequence.RunAsync` **once**; and then call
`_contacts.RecordSent(text, slotStartUtc)` - **the one line unit 258 left
`RecordSent` in the tree for.**

**THE FOUR THINGS THIS MUST NOT DO, AND EACH GETS AN ASSERTION:**

1. **It never asks for confirmation.** No dialog, no second click, no armed state
   the operator has to confirm. Ruled 2026-09-06.
2. **After a send, nothing further transmits without another click.** *This is
   criterion 4 and it is a must-pass.* **Watch it fail first:** build the arming so
   the armed send is not cleared once it has fired, drive two slot boundaries past
   it, and watch a second transmission go out that nobody asked for. **Quote that
   red.** Then clear it, and assert that two boundaries produce one transmission.
   **That red is the whole reason this task exists** - it is the phase's one
   unrecoverable fault, caught on a fake port where it costs nothing.
3. **A second click replaces the armed send; it never adds one.** Two clicks a
   second apart are one transmission in the next slot, not two.
4. **Nothing arms itself.** No decode, no timer, no state change and no retry may
   reach this command. **Assert it by reading the file** the way
   `NothingInTheSequenceCanStartATransmissionOnItsOwn` reads
   `Ft8TransmitSequence.cs`, and name the one entry point that exists.

**An armed send that has not yet keyed can be cancelled**, and cancelling it is a
flag set on the same thread, not an await. Once it has keyed, the route out is
`Ft8TransmitSequence`'s own abort and you add nothing beside it.

**Proved against the fakes that already exist** - `FakeTransmitAudioSink` and the
fake CI-V transport in `tests/Hamlet.RadioEngine.Tests/Transmit/`. **Open no port
and no device.** `SHACK_FACTS.md` FACT-004 rules that no radio has ever been
attached to this machine, so a real port here measures nothing about the radio.
**Criterion 3 is met on the fake wire - the keying frame, the samples, the unkey,
and the recorded slot being the next boundary - and the last mile to a real radio
is step 6's and Tim's.** Say exactly that in the report; do not claim more.

**If task 1's answer to question 2 is that no seam reaches a port**, the command
refuses with words - *no radio is connected* - and the test drives the sequence
through the test seam instead. **That is an acceptable landing and it is reported
as one.** What is not acceptable is a command that silently does nothing.

### Task 4 - the licence, and the Send area's words

**Criterion 6: out of licence privileges, the menu says so and sends nothing.**

- **The menu still lists everything.** *Nothing is forbidden in the menu* is a
  ruling about the contact state, not about the licence. What the menu gains is a
  line saying the licence refuses at this frequency, in `TransmitDecision`'s own
  `Reason` and `Citation` - **asked of `TransmitGuard.Check` for display, and not
  a second copy of the rule.**
- **The send refuses at the gate inside `Ft8TransmitSequence.RunAsync`**, which
  already treats an overridden permit and an unknown class as refusals in this
  path. **Assert zero bytes reached the fake port and the fake sink was never
  touched**, the way unit 255 did.
- **Do not change `TransmitGuard.Check` or any existing caller of it.**

**Criterion 5: the Send area.** `DigitalSendReservedLine` at
`MainWindow.axaml:3082` currently says *"Send lives here when it is built. Hamlet
does not transmit yet."* **That sentence becomes false tonight and must not
survive.** In its place: what is being sent and to whom, bound to a view-model
string, **formatted in the view model so a test can assert it without opening a
window** - the same precedent `DigitalDecodeRow` and unit 258's `Contact` cell
set. It says what went out, to whom, and in which slot; when nothing has been
sent it says so plainly; when the licence refused it says that instead.

**Criterion 1: the CQ button.** Sends `CQ KC3QIS FN00` from
`OperatorProfile.Callsign` and `.GridSquare`, **with no typing**, through exactly
the same command task 3 built. **One send path, not two.** With no grid set it is
`CQ KC3QIS` and the Send area says the grid is unset.

**The transmit endpoint.** Add `AudioOutputDeviceId` to `AppSettings` beside
`AudioInputDeviceId` at `:193`, in the same shape, if that is one field and
whatever migration the existing ones have. **A Settings screen for it is out of
scope**; name it as what remains. **If the field costs more than that, leave it
out, have the send refuse with words when no endpoint is named, and report it.**
**Never play to whatever the machine defaults to.**

### Task 5 - the markup, and the record. **Drop candidate here.**

**The right-click.** A `ContextFlyout` on the row template's root `Grid` at
`MainWindow.axaml:3407`, listing the options from task 2 with the expected one
highlighted and the repeat count shown beside a repeat. The `MenuItem` pattern
with a bound `ItemsSource` and a command reached through
`$parent[ItemsControl].((vm:MainWindowViewModel)DataContext)` is already used in
this file at `:1299`-`:1396` - **follow it rather than inventing one.**

**THE NAMED DROP CANDIDATE IS THE ROW'S `ContextFlyout` MARKUP.** If this unit
runs long, **drop it and keep everything else** - the option list, the command,
the CQ button and the Send area line. Say in the report that you dropped it and
what remains: criterion 2 met in substance with an asserted option list and one
afternoon from being on screen, and criteria 1 and 5 whole. **A half-edited
3,600-line `.axaml` at the end of a long night is a broken window, and unit 258
kept its markup change small enough to land - the flyout is bigger than the column
was.**

**Nothing in tasks 2, 3 or 4 is a drop candidate.** If you are out of time before
task 3 is finished, write the report on what exists and name what is missing -
**do not** shed the one-click assertion to reach the markup. **The assertion is the
point of the unit.**

**Then the record:**

- **`PHASE_OUTCOME.md`** - append this unit's entry. Try
  `tools\arbiter\outcome-append.bat` once; when it is refused, write the entry
  with the file-editing tools in the exact twelve-field format the existing
  entries use, same names, same order, **ASCII only**, and update the header's
  `STEP: 5` line in place. **Record the refusal verbatim in the report.**
- **The greps that prove there is exactly one way to key.** `new
  Ft8TransmitSequence` and `RunAsync` appear in `src/` only in the one command
  task 3 built; nothing else in `src/` constructs a sequence or a sink; nothing
  you added names PTT, CI-V or `TransmitAbort` directly. **Quote them.**
- **`PROJECT_STATUS.md`** per the cadence. **Not `CURRENT_STEP:`, not
  `HEARTBEAT:`, not the `STEP:` lines.**

---

## Parked - do not touch, do not raise

**From `PHASE_PLAN.md`'s own list**, and it is not open for discussion: automatic
sequencing - *if I get a response, send `73`* is **deliberately out of this phase**
and is the single most tempting thing to build tonight; logging (FG-004); FT4,
PSK31 and WSPR transmit; CW send; the OSD re-encoding count; `ReusableWindow`;
`ProcessDelayForTests`; the tap's owner; the waterfall's first row; unit 237's
Extensible conclusion; work instruction 231's four tree items;
`validate-output.bat`'s permitted-spellings bug; the 101.33 ms pulse above 6 kHz;
the CW decoder and its inherited reds.

**And these six, carried from units 253 to 258:**

- **`TransmitGuard.Check` permits when the operator's toggle is off.** Banked and
  the owner's. **Your send path already treats that as a refusal inside itself and
  changes nothing about the guard.** Not re-raised, not widened.
- **Five routes in the tree reach a keying frame**, including `AutoCaller` at
  `Cw/AutoCall.cs:272`, which keys repeatedly from one operator start. **On the
  parked CW path. Logged. Do not touch it, and do not copy its shape.**
- **`SetSettingAsync(CivWrites.AntennaTuner, CivWrites.TuneNow)` writes
  `1C 01 02`**, a tuning cycle that transmits, called by no line in the tree.
  **Logged. Parked.**
- **The IC-7300's USB modulation input level.** Deferred to Tim at step 2's
  criterion 4 and step 3's criterion 1. **Not this unit's and not inferable from
  this machine** - `SHACK_FACTS.md` FACT-004.
- **`ContactStage`, `SendOption` and `ContactShape`.** Unit 257's survey settled
  them: right shape, wrong ruling, parked CW path. **Do not reach for them, and
  if you name your option type `SendOption` you will collide with one of them -
  check first.**
- **The gone-quiet threshold of four slots** and the corpus's synthesized
  provenance. Both argued, both recorded, neither yours.

---

## What not to do

**Citing rather than retyping where the rule is already written down.**

1. **Do not open a serial port and do not open an audio device.** FACT-004. Every
   transmission in this unit goes to a fake. **A device opened here measures this
   machine and says nothing about the radio.**
2. **Do not change a line of `src/Ft8Sharp/` or `src/Ft8Sharp.Deep/`.**
3. **Do not change `Ft8TransmitSequence`, `TransmitAbort`, `TransmitGuard`,
   `WasapiTransmitSink`, `Ft8Composer`, `Ft8ContactLedger` or
   `Ft8ContactStates`.** **Call them. Read them. Leave them.** If one of them
   genuinely cannot be called without a change, that is a finding for section 3
   with the line quoted - not a licence to edit it.
4. **Do not add a second route to a keying frame.** There is one, it is
   `Ft8TransmitSequence`, and its abort was watched to fire. Task 5 proves it with
   a grep.
5. **Do not build automatic sequencing.** No *if he answers, send the report*. No
   retry. No continuation. **One click, one message**, and the sequence is a later
   ruling once the single shot has been watched on a real band.
6. **Do not add a confirmation.** Ruled 2026-09-06. No dialog, no "are you sure".
7. **Do not remove, grey out, sort away or forbid an option** on the basis of the
   contact state. Unit 252 removed row dimming on Tim's ruling; do not reintroduce
   it in another shape.
8. **Do not interpret a message.** §12.1. A label naming a field shape is
   arithmetic; wording what a station meant is not, and **`Ft8Vocabulary.Explain`'s
   table was closed by Tim on 2026-09-04.**
9. **Do not invent a grid, a callsign, a frequency or a licence class.** §0.0. If
   Settings has not got one, the message that needs it is absent and the Send area
   says why.
10. **Do not edit `PHASE_OUTCOME.md`'s existing entries**, including the two that
    disagree with themselves. Append yours; leave the record.
11. **Do not run an unfiltered `dotnet test`, and do not background a command and
    poll for it.** The rules at the top killed four sessions.
12. **Do not chase the known reds.**
13. **Do not hand-write `HEARTBEAT:`, `CURRENT_STEP:` or the `STEP:` lines in
    `PHASE_STATUS.md`.**
14. **Do not re-derive `RULES_AT`.** Four units have answered it.
15. **Do not write a survey.** Task 1 is six questions with line numbers. Unit 257
    died writing a survey and unit 258 landed by reading it in ten minutes.

---

## Committing and pushing

Commit and push **each task before starting the next**. Bump the root version's
patch by one per task from `1.12.100`, so `1.12.101` through `1.12.105`.
**`Ft8Sharp` does not move.**

**Task 1 commits twice**: once for the uncommitted root records exactly as they
arrived, and once for the trace - **and the trace's question 6, the predicted
menus, must be in a commit before task 2 starts.** That is what makes `NUMBER:`
a measurement rather than a claim.

---

## Logged, not chased

**Unit 258's section 4 raised nothing and asked for no ruling.** The judging
session read it and returned *none*. **Nothing is banked from the last unit.**

Four things carried forward so they are not lost, none of them a ruling request:

1. **`validate-output.bat` could not be started in nine forms across four
   units.** Logged, and the tool rule tells you what to do about it.
2. **`Ft8ContactLedger.RecordSent` has had no caller since it was written**, on
   purpose, waiting for you. Task 3 is where it stops being unreachable.
3. **`AudioTap.Level` is a 0.2 s moving meter** and reads `NearlySilent` on audio
   that decoded perfectly. **Logged. Not in your way** - you play nothing.
4. **`BoundariesBetween` includes `from` when `from` is itself a boundary**, which
   unit 257's survey got wrong and unit 258 measured. **Relevant to you**, because
   task 3 counts to the next boundary. Read `Ft8Slots.cs:209-232` before you write
   that arithmetic, and write it once.

---

## Reporting

`output.md` at the repository root, four sections per `CLAUDE_CODE.md` §8.

**First, above everything, the ordering block. `validate-output.bat` refuses a
report without it.** It must carry the literal words `READ IN THIS ORDER`, then
three paragraphs beginning `A.`, `B.` and `C.` at the start of a line, all inside
the first 60 lines of the file, and **C must contain the literal phrase `raises N
items`** with a real number.

- **A - the phase goal and where every step stands.** Hamlet works stations on the
  air. Step 0 `done`. Steps 1, 2, 3 and 4 `partial` and closed, **and say what is
  outstanding in each is either on the record or a figure only Tim can read off a
  radio.** **Step 5 entering this unit at `not started` and leaving it at whatever
  you actually reached.** Step 6 not started, **and say plainly whether what you
  built is enough for Tim to attempt it, or what stands between.**
- **B - this step and its exit criteria, and which were met.** Step 5's six, in
  the plan's own words: a CQ button from the operator's own settings, no typing;
  right-click offers every message valid at that point with the expected one
  highlighted and none forbidden, a repeat showing its count; choosing one
  transmits in the next slot with no confirmation; one click sends exactly one
  message, asserted by a test; what is being sent and to whom in the reserved Send
  area; out of licence privileges the menu says so and sends nothing. **Say which
  of the six stand met, one line each, on quoted evidence** - and for criterion 3
  say plainly that the transmission was proved on a fake port and a fake sink and
  what that leaves for step 6.
- **C - what this report adds, weighed against A and B.** How many items section 4
  raises, in the words `raises N items`, and **whether any of them is in the way of
  a criterion named in B.** If none is, say so - that is a real answer and it is
  not a ruling request.

**Then the six-line header block**, from the clock, never composed:
`UNIT:`, `PHASE GOAL:`, `UNIT GOAL:`, `ADVANCED:`, `NUMBER:`, `DRIFT:`.

**`NUMBER:` for this unit is how many of the corpus's five stations the option
list produced the predicted set of messages for, of five** - and *predicted* means
what task 1 question 6 wrote down and committed **before** task 2 existed, not what
came out. **`DRIFT:` - unit 258 reported 0.**

**Section 3 leads with three things, in this order:**

1. **The one-click assertion, and the red you watched first.** Quote the failing
   run where an armed send that was not cleared transmitted a second time at the
   next boundary with nobody clicking, and the green where two boundaries produce
   one transmission. **Name the entry point - the only one - by which a
   transmission can begin**, and quote the grep or the file-read that proves there
   is no other. **This is the phase's one unrecoverable fault and this paragraph is
   the evidence it cannot happen**; it comes first for that reason and not because
   it is the largest piece of work.
2. **The menu, station by station, against what task 1 predicted.** Every one of
   the five, one block each: the options offered, which was expected, any repeat
   count, and the predicted set beside it. **The station the ledger reads as
   `complete` gets its own paragraph** showing that `73` and everything else are
   still offered, with the sentence saying nothing was removed and the reflection
   assertion that nothing could be. **And the red you watched first** - the
   expected-message-only list, and which station it left with nothing to send.
3. **What the licence gate did, and where the send path lives.** The refusal with
   its `Reason` and `Citation`, **zero bytes at the fake port and the fake sink
   never touched**, quoted. Then the file and line of the option list, of the
   command, and of `RecordSent`'s one call site; what the Send area says now and
   what the sentence it replaced said; where the frequency, callsign, grid and
   licence class come from; and **whether the app can reach a real port and a real
   endpoint, or what stands in the way.** **If the drop candidate was dropped, say
   so here and say what remains.**

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
APPROACH: Wire the right-click menu, the CQ button and the Send area to the ledger and the transmit sequence so one click sends exactly one message in the next slot, proved against the fake port and fake sink already in the tree with no device opened
MOVE: continue
WHY: Step 5 has had no unit spent on it, both its entry steps are substantively met with only radio-side figures outstanding, and it is the last step any unit can move - step 6 is Tim keying a transmitter. The loop test was run on this approach and returned NOT FOUND against all twelve entries; none of the tried approaches - a documentation sweep, an abort, a waveform seam, a keying sequence, a render sink, a contact ledger - is the operator-facing join, and Ft8TransmitSequence's own remarks name step 5's right-click as the caller it was built for.
STATE: not started
DECIDED: Four on my own authority. First, criterion 3 is taken on the fake port and fake sink that units 253 to 255 left in the tree rather than on a real device, because SHACK_FACTS.md FACT-004 rules no radio has ever been attached to this machine, so the last mile is step 6's and is named rather than claimed. Second, the option list is built in the engine as a testable list with the expected one marked and repeats counted, not in the markup, so criterion 2 can be asserted without opening a window - the precedent unit 258 set with the contact cell. Third, criterion 6 is resolved against the nothing-is-forbidden ruling this way: the menu still lists every message and gains a line carrying TransmitGuard's own Reason and Citation, and the refusal that actually stops a transmission stays inside Ft8TransmitSequence where it already is, so no second copy of the licence rule is written. Fourth, with no grid in Settings the CQ is CQ KC3QIS and the grid options are absent with the reason said out loud, because inventing a locator is the fault section 0.0 exists for.
LICENCE: PHASE_PLAN.md's own rulings of 2026-09-06 - one click one message, right-click sends immediately in the next slot with no confirmation, nothing forbidden in the menu, and a contact never closed by the app - together with its named alternatives to stopping: the tree wins, report the mismatch and continue, and where the radio is wanted the step is closed on what can be proved here with what Tim must do named. SHACK_FACTS.md FACT-004 licenses the fake port and fake sink and forbids the inference that would otherwise close criterion 3. The three the arbiter may not reason past license the ordering directly: the abort was watched to fire in unit 253 and lives inside Ft8TransmitSequence, so a caller may now ship, and it ships with the gate inside it and with one click proved to send exactly one message.
ACCOMPLISHED: The operator right-clicks a station on the Digital tab and Hamlet offers him every message that is valid at that point, with the one that conventionally comes next highlighted, nothing greyed out, and a repeat showing that it is a repeat. He clicks one and exactly one transmission is armed for the next slot - no dialog, no second click, and nothing further goes out until he clicks again, which is watched failing first and then held. A CQ button does the same from his own callsign with no typing, the reserved space under the waterfall says what is going out and to whom, and outside his licence privileges the menu says so and not a byte reaches the radio. What is left after this is Tim, a radio and an antenna.
ADVANCES: step 5 - exit criterion 1 (a CQ button sending CQ KC3QIS FN00 from the operator's own settings with no typing), criterion 2 (right-click offers every message valid at that point with the expected one highlighted, none forbidden, and a repeat showing its count), criterion 3 (choosing one transmits in the next slot with no confirmation), criterion 4 (one click sends exactly one message, asserted by a test that nothing further transmits without another click), criterion 5 (what is being sent and to whom appears in the Send area reserved beneath the waterfall) and criterion 6 (out of licence privileges the menu says so and sends nothing).
END-ARBITER-DECISION
```

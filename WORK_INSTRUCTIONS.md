# Work instruction 255 - key, transmit, unkey, with the unkey guaranteed

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

**All four were checked against the tree at 2026-09-06 while this instruction was
written.** `SHACK_FACTS.md` and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs`
are present; neither `CoreHMI.sln` nor `MURC.sln` exists; the only solution at the
root is `Hamlet.sln`. Check them anyway.

---

## THE TWO RULES THAT KILLED THREE SESSIONS

**Tim's rulings of 2026-09-05. Not this unit's to weigh.**

**1. A unit runs no test suite.** **A unit may run only the unit test it
constructs in that work instruction**, filtered by exact name, in the foreground,
with a stated timeout of a few minutes. **An unfiltered `dotnet test` on any
project is forbidden.**

**2. Never background a command and poll for it.** Three sessions were killed by
the watchdog on 2026-09-05, at 33 to 38 minutes, each sitting in
`until grep -q "exited with code" ...; do sleep 15; done` with a `900000` ms
timeout. **The watchdog fires after twelve minutes with no status write.**

`dotnet build` is allowed, foregrounded, with a timeout.

**One named exception, granted by the arbiter and narrow.** This unit adds to
`src/Hamlet.RadioEngine/Transmit/Ft8Composer.cs`, which unit 254's corpus test
stands on. **You may re-run
`HamletsOwnDecoderReadsBackWhatHamletComposedTests`, filtered by exact name,
foregrounded, with a stated timeout**, to prove you did not break it. That is one
named test, not a suite, and the two rules above are otherwise untouched.

---

## THE TOOL RULE

**This session's shell may refuse calls.** A refused shell call is a signal to
reach for the other tool, not to stop. **The file-editing tools have been
unaffected throughout.** Record every refusal verbatim. **Nothing in this unit
halts the loop.**

**Three tool facts earlier units paid for:** this shell collapses a doubled
backslash inside a quoted heredoc, so use the file-editing tools for anything
with escapes in it; unit 253 recorded `grep` aborting with exit 134 on
multi-pattern and `-c` invocations, so run one pattern at a time if that happens;
and **`tools\arbiter\outcome-append.bat` has been refused for three consecutive
units** - 253 twice, 254 once. Expect it, and see task 5.

---

## Why this unit exists

**The count today.** Step 0 `done`, one unit spent. Step 1 `partial`, two units.
Step 2 `partial`, two units. **Steps 3, 4, 5 and 6 have had no unit spent on
them at all.** Hamlet can now compose a message into a slot of audio its own
decoder reads back - 112 of 117 identical, 5 conditional and proved, 0 failed -
and it has an abort that fires from every state in 22 ms. **Neither has a
caller. Nothing in this repository can key a radio on purpose.**

**Step 3 is the join, and it is the one step where a mistake goes out over other
people's band.** So this unit takes the half of it that can be proved without a
radio and without an audio device: **the sequence** - gate, key, play, unkey -
with the unkey guaranteed on every path, the abort as its first caller, and the
licence gate refusing inside the path rather than beside it.

**What this unit deliberately does not do: it opens no real serial port and no
real audio device, and it plays no sound.** Step 3's first and third exit
criteria - audio into the radio's USB input, and the loopback that captures and
decodes it - are **the next unit's**, and they are left whole and named. The
reason is not caution for its own sake: `SHACK_FACTS.md` FACT-004 rules that no
radio has ever been attached to this machine, so device work here proves a device
here and nothing about the radio, while the *shape* of a keying path - what
happens when the audio throws, what happens when the port is gone, what refuses
before anything keys - is fully provable against fakes tonight and is the part
that must be right before any real device is opened.

**This ordering is the phase's own.** Unit 253 built the abort before anything
could key. This unit builds the sequence before anything can play.

```
PHASE GOAL:   Hamlet works stations on the air.
UNIT GOAL:    One call keys the radio, hands a transmission to an audio sink,
              and unkeys - and the unkey happens even when the sink throws, the
              port is gone or the licence gate refuses. TransmitAbort gets its
              first caller. Nothing opens a device and nothing in the tree
              calls it yet.
ADVANCES:     step 3 - exit criteria 2 (key, transmit, unkey, with the unkey
              happening even if the audio path throws), 4 (the transmitted slot
              recorded to telemetry with what was sent and when), 5 (the licence
              gate is in the path and refuses out-of-privilege frequencies,
              asserted by a test) and 6 (nothing keys without an operator action
              reaching this code). It also closes the open half of step 2's
              criterion 5 by deciding where in the slot the transmission starts.
```

---

## Verify this instruction against the tree

**Nothing below describes the tree with authority. It was read at 2026-09-06 and
line numbers move.** Check every claim, match on the symbol rather than the line,
and **report mismatches in section 4 - do not repair this instruction and do not
stop on one.**

What was read, and where:

- **`src/Hamlet.RadioEngine/Civ/TransmitAbort.cs`** - `public static class
  TransmitAbort`, `Fire(ISerialPort port, byte radioAddress, byte
  controllerAddress)` returning `AbortRecord(AbortAttempt CwStop, AbortAttempt
  PttOff)`, with `AnythingReachedTheRadio`. Its own remarks say **"Every caller
  from step 3 onward logs this"** and **"Nothing in this repository calls it."**
  This unit is that caller.
- **`src/Hamlet.RadioEngine/Civ/CivConstants.cs`** - `CmdSendCwMessage = 0x17`
  at `:56`, `CwStopByte = 0xFF` at `:67`, `CmdTransceiverControl = 0x1C` at
  `:78`, `SubPtt = 0x00` at `:83`, **`PttOff = 0x00` at `:96`. There is no
  `PttOn`.** Unit 253 withheld it deliberately - "a constant is the easiest
  thing in a codebase to reach for by accident". See task 2.
- **`src/Hamlet.RadioEngine/Transport/ISerialPort.cs`** and `SystemSerialPort.cs`
  are the whole transport surface. **`tests/Hamlet.RadioEngine.Tests/Rig/FakeSerialPort.cs`
  already exists** - unit 253's. Read it before writing a second fake.
- **`src/Hamlet.RadioEngine/Transmit/Ft8Composer.cs`** - `Ft8Composer.Compose(string?
  text, int sampleRate = 12000, float baseFrequencyHz = 1000)` at `:212` returns
  `Ft8ComposeResult`; `Ft8Transmission(Text, ReadsBackAs, Type, Samples,
  SampleRate, BaseFrequencyHz, CarriesHashedCallsign)` at `:76`, with
  `SlotSeconds` and `PeakSample`. **`Compose` calls `Ft8Waveform.SynthesizeSlot`,
  so `Samples` is the padded 15 s slot with the signal centred** - 1.180 s of
  silence at each end, measured by unit 254.
- **`src/Ft8Sharp/Encode/Ft8Waveform.cs:142`** - `Synthesize` returns the 12.64 s
  signal with no padding. `PaddingSampleCount` at `:125`. **Parked: not a line of
  `src/Ft8Sharp/` changes.**
- **`src/Hamlet.RadioEngine/Audio/Ft8Slots.cs`** - `public static class Ft8Slots`
  at `:119`, `SlotSeconds = 15` at `:126`, **`TransmissionSeconds = 12.64` at
  `:135`**, `TransmissionFits(double)` at `:162`, `TrueUtc` at `:174`,
  `SlotStart(DateTime)` at `:182`, `IntoSlot(DateTime)` at `:195`. **The slot
  clock already exists and this unit does not write a second one.**
  `ClockOffset` at `:20` and `SntpClock.cs` are beside it.
- **`src/Hamlet.RadioEngine/Licensing/TransmitGuard.cs`** - `Check(LicenseClass,
  long frequencyHz, TransmitMode, bool guardEnabled)` at `:67` returns
  `TransmitDecision(bool MayTransmit, string Reason, string Citation, bool
  WasOverridden)`. **It returns `MayTransmit: true` in three different ways**:
  the privileges permit it `:72`; the licence class is unknown `:77-85`; or the
  operator's guard toggle is off `:87-90`, which sets `WasOverridden: true`.
  Read all three branches yourself. See task 3.
- **`src/Hamlet.RadioEngine/Telemetry/ITelemetry.cs`** - `TelemetryCategory` has
  `Diagnostics, Rig, Tuning, Explore, Decode, Performance` and **no transmit
  category**; the enum's own comment says "adding a category is a deliberate act,
  not a string typed at a call site". `TelemetryEvent`, `ITelemetry.Write`,
  `NullTelemetry`. `tests/Hamlet.RadioEngine.Tests/Telemetry/RecordDisciplineTests.cs`
  holds `ADecodeWindowCannotCarryDecodedText` at `:105` - **read it; it is the
  shape your telemetry test takes.**
- **Audio output: the engine has none.** `src/Hamlet.RadioEngine/Audio/` holds
  `IAudioSource.cs`, `WasapiAudioSource.cs` and `AudioTap.cs`, all capture. The
  only playback in the tree is `src/Hamlet.App/Audio/ModeAudioPlayer.cs:92`, a
  `WaveOutEvent` for training tones, **in the UI project, which this unit does
  not touch.** Verify that and report it, because the sink interface you write in
  task 2 is written on the assumption that nothing to reuse exists.
- **`tests/Hamlet.RadioEngine.Tests/Transmit/`** holds unit 254's three classes:
  `HamletsOwnDecoderReadsBackWhatHamletComposedTests`,
  `TheSeamTurnsWordsIntoASlotOfAudioTests`,
  `WhatTheTransmissionLooksLikeAsAudioTests`. The last one contains
  `TheSeamNamesNoRadioNoDeviceAndNoEncoderOfItsOwn`, **which will go red if you
  put a rig type, a PTT or a CI-V reference into `Ft8Composer.cs`.** That is
  correct behaviour and it constrains where your code goes. Do not weaken it.
- **Root version was `1.12.85` in `Directory.Build.props:205`.** Read it; do not
  assume it.
- **`docs/unit255-*` is free; `docs/unit254-combining-depth.md` belongs to the
  previous phase.** The numbering collided when this phase restarted. **Do not
  overwrite any existing `docs/unit25*` file.**

Arithmetic worth checking rather than trusting: at 12000 Hz the composed slot is
180,000 samples and the signal inside it is 151,680; the difference is 28,320,
which unit 254 measured as 14,160 samples of silence at each end, 1.180 s.
**Whether a transmission should start at the slot boundary or a stated moment
after it is task 4's question and not a claim here.**

Known reds, inherited, **never chased**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list
tripwire.

---

## What the reload measured, and what to do about it

**Reported to you, not repaired by the arbiter.** Handle these in task 1 and do
not spend a night on them.

1. **Four modified files at the root are uncommitted** - `PHASE_OUTCOME.md`,
   `PHASE_STATUS.md`, `RUN_LEDGER.md`, `WORK_INSTRUCTIONS.md`. They are unit
   254's records and this instruction. **Commit them with your first task.**
2. **`tools/unit254-seam-grep.sh` is untracked**, a leftover of a shell call unit
   254 could not delete - both `rm` and `git rm` were refused. **Try once to
   delete it. If the shell refuses, say so verbatim and leave it.** It is in no
   commit.
3. **`RULES_AT` disagrees**: `PROJECT_STATUS.md` says `HM-DEC-157 (2026-09-06)`
   while the reload reports `CLAUDE.md` §1's highest as `CPS-DEC-0152`, a prefix
   this project does not use. `HM-DEC-155`, `156` and `157` are all in
   `DECISIONS.md`. **Spend five minutes, say in section 3 what `CLAUDE.md` §1
   actually holds, and change nothing.** It is a pointer, not a rule.

---

## Step 2 is closed at partial, and one half of it is yours

**The arbiter closed step 2 `partial` on 2026-09-06.** Criteria 1, 2 and 3 are
met: the geometry and continuous phase, 112 of 117 messages read back through
`Ft8SlotDecoder` with the 5 hashed ones proved conditional, and byte-identity by
reuse of the port's existing comparison against upstream's WAV.

**Criterion 4 is deferred to the operator and no unit will close it.** Level and
clipping are measured and pinned - peak 1.000000, PCM16 running -32766 to
+32767 - but the figure that matters, **the level the IC-7300's USB modulation
input expects, is nowhere in this repository**, and `SHACK_FACTS.md` FACT-004
forbids inferring it from this machine. `PHASE_PLAN.md`'s own table says: *the
radio is wanted - close on the loopback or the recording, mark it deferred, name
what Tim must do.* **What Tim must do: set the drive level at the radio by
watching ALC on the first live transmission, and write the figure into
`SHACK_FACTS.md` as a fact.** Do not measure a development-machine endpoint and
offer it instead.

**Criterion 5's open half is this unit's, in task 4.** The 12.64 s length holds
exactly at both rates; where the transmission sits does not. Unit 254 measured
the port centring it in the slot and reported, correctly, that **on the air that
is wrong by about a second** and that the fix belongs where the playing is
scheduled. That is here.

**Do not re-open criteria 1, 2 or 3. Do not attempt criterion 4.**

---

## Rulings in force

**Not to be re-argued by any unit.**

**Tim's, 2026-09-06:**

- **Tim operates a licensed station on an antenna and Hamlet transmits on the
  air.** **HM-DEC-008 and HM-DEC-098 are withdrawn in full.** The dummy load is
  **not a stage, not a fallback, and not to be referenced.** **Do not propose it,
  do not treat its absence as a risk to be mitigated, and do not add a
  compensating control in its place.**
- **One click, one message.** Hamlet transmits because the operator clicked, and
  for no other reason. **Never on a timer, never on a decode, never to continue a
  contact.** A transmission he did not ask for is this phase's one unrecoverable
  fault.
- **Right-click sends immediately, in the next slot, with no confirmation.**
- **Nothing is forbidden in the menu.** The expected next message is highlighted;
  everything valid stays clickable; a repeat is correct behaviour and shows its
  count.
- **A contact is never closed by the app.** `73` is politeness, not a
  requirement.

**Standing, and unaffected by the above:**

- **Every path that keys the transmitter has a same-thread, no-await abort** -
  CI-V `0x17` with `0xFF`, PTT off as the fallback. **No unit ships a keying path
  before its abort is watched to fire.** It was watched, 14 of 14 in 22 ms, unit
  253.
- **Hamlet never transmits outside the operator's licence privileges. The
  Settings gate is not bypassable from any send path.**
- **Section 0.0.** Never present a guess as a decode.
- **Section 0.1.** The engine is not told that tabs exist, and references no UI
  assembly.
- **Section 12.1.** Nothing interprets a message.
- **HM-DEC-018.** **Telemetry never carries a callsign, decoded message content,
  or anything identifying a person or a contact.** `DECISIONS.md` records this
  applied to exactly your case - a CW `CQ` that was transmitted - and rules that
  *"the length, the count, the duration, the frequency and the mode make the
  transmission fully diagnosable and identify nobody"*. **That is the shape your
  telemetry takes and the question is already answered.**
- **`Ft8Sharp` is a faithful MIT port and nothing in this phase changes a line of
  it.** `Ft8Sharp.Deep` is GPL-3.0.

**And the plan's own alternative to stopping, which applies to this unit
directly:** if the tree disagrees with this instruction, **the tree wins** -
report the mismatch and continue.

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` **read from the clock,
never composed**, and `NOTE` saying what is moving inside the task. The same
every ten minutes while a task is running. **Use the file-editing tools if the
shell refuses.**

**Unit 253 wrote three `UPDATED` values it composed, one of them 39 minutes ahead
of the real clock.** Read the clock. A panel watching a future timestamp is being
lied to.

---

## Tasks

Five tasks. **Task 1 is reading only. Task 2 is the goal task.** Task 4 carries
the named drop candidate.

### Task 1 - what can key a radio today, and what plays audio

**Reading only. Change nothing under `src/` or `tests/` in this task.** Commit
the four uncommitted root files at the start of it.

- **Every route in the tree that can key a transmitter today**, with file and
  line. Unit 253 named `Ic7300Rig.SendCwAsync` and `CivWrites.TuneNow`. **Find
  out whether that list is complete** rather than repeating it - search for the
  CI-V transmit command, for PTT, and for anything that writes `0x1C`.
- **What the CI-V surface already has and what it deliberately lacks.** Say what
  `CivWrites` does today and confirm that no constant in the tree turns the
  transmitter on.
- **What `TransmitAbort.Fire` costs to call** - what it needs, what it returns,
  and whether anything about it makes it awkward to call from inside a `finally`.
- **The licence gate's three permitting branches**, quoted, with what each one
  returns.
- **The telemetry surface**: the categories, what `RecordDisciplineTests` already
  enforces, and what HM-DEC-018 permits a transmit event to carry.
- **The slot clock**: what `Ft8Slots` already computes, and whether anything in
  it needs a real clock or an SNTP answer to be testable.
- **Audio output in the engine: is there any?** Answer it from the tree.
- **Say what is missing**, explicitly, rather than assuming it is nothing.

**Write it to `docs/unit255-keying-path-survey.md`** with file and line
throughout, and **mark each of step 3's six exit criteria met or not met, by
what evidence, and by which unit it will be met if not this one.**
*must-pass: the survey exists and carries that per-criterion table.*

### Task 2 - the sequence: gate, key, play, unkey. THE GOAL TASK.

**The build. It opens nothing and it plays nothing.**

- A new type in `Hamlet.RadioEngine` - **`src/Hamlet.RadioEngine/Transmit/` is
  the suggested home; put it where it belongs and say where** - that takes an
  `Ft8Transmission` and drives one transmission from start to finish: **check the
  gate, key, hand the samples to a sink, unkey.**
- **It implements neither end.** The CI-V end is `ISerialPort`, which already
  exists. The audio end is **a small interface this unit declares** - one method
  that takes the samples and a rate and returns when they have been played, and
  something that says how long that took. **Name it for what it is and keep it
  narrow enough that a real WASAPI implementation and a fake both fit it.** This
  unit writes the fake only. *must-pass*
- **The unkey is guaranteed and this is the criterion the unit exists for.** The
  radio comes out of transmit **when the sink throws, when the sink returns
  early, when the sink is cancelled, when the port throws on the way out, and
  when the sequence itself throws.** `finally`, or better. *must-pass*
- **`TransmitAbort.Fire` is the abnormal path and this unit is its first
  caller.** On any failure, the abort fires and its `AbortRecord` is carried out
  in the result rather than swallowed - its own remarks require the caller to log
  it. **A normal, successful unkey is not an abort** and should not pretend to be
  one; say which of the two happened. *must-pass*
- **The constant that keys the radio.** `CivConstants` has `PttOff` and no
  `PttOn`, withheld on purpose by unit 253. **Add it now, in that file, with a
  comment saying why it was withheld and what changed** - and **do not add a bare
  "key the radio" helper to `CivWrites`.** The keying write lives behind this
  sequence, where the unkey is guaranteed, and nowhere else. *must-pass*
- **Nothing keys without an operator action reaching this code.** No timer, no
  `Task.Delay`-driven send, no clock-triggered entry, no decode-driven caller,
  and no public method that starts a transmission without being handed the
  operator's intent explicitly. **A test greps the executable body for `Timer`,
  `Delay`, `Interval`, `Elapsed`, `Schedule` and friends and fails on a hit**,
  the way `TheSeamNamesNoRadioNoDeviceAndNoEncoderOfItsOwn` already does for the
  composer. *must-pass*
- **Nothing in the tree calls it when this unit closes.** Prove it with a grep
  and put the grep in the report. The first real caller is step 5's right-click.
  *must-pass*
- **Watched to fail first.** Break the unkey deliberately - the obvious break is
  to unkey on the success path only, so that a throwing sink leaves the radio
  keyed - and **watch the test go red before you watch it go green.** Name the
  breakage and what it caught. **The breakage this test would have caught is the
  one you just watched it catch**, which is what `PHASE_PLAN.md` requires of any
  new test. *must-pass*
- Run the tests **filtered by exact name, foregrounded, with a stated timeout.**

### Task 3 - the licence gate is inside the path, not beside it

- **The sequence refuses unless the gate positively permits.** For this send
  path, **`WasOverridden: true` is a refusal and an unknown licence class is a
  refusal.** Only a permit that carries a citation and was not overridden lets
  anything key. *must-pass*
- **Do not change `TransmitGuard.Check`.** Every existing caller keeps exactly
  the behaviour it has today. The narrowing lives in the send path, which is new
  code, which loosens nothing that exists. **Report the discrepancy between
  `Check`'s three permitting branches and §0.2's "not bypassable from any send
  path" - do not resolve it for the other callers.** That question is banked for
  the owner and was banked by unit 253; **it is not re-raised here and it is not
  in your way**, because this path answers it for itself. *must-pass*
- **The test that matters is not that it returned false.** It is that **the fake
  transport saw zero bytes** - an out-of-privilege frequency, an unknown class
  and an overridden guard each refuse **without a single byte reaching the port
  and without the sink being touched.** *must-pass*
- **The refusal says why, in the operator's words**, carrying the citation the
  gate returned. A refusal that does not say why is the thing HM-DEC-012 was
  about.

### Task 4 - when it starts, and what the record says. **Drop candidate here.**

- **Where in the slot the transmission starts, decided and measured.**
  `Ft8Composer.Compose` returns the padded 15 s slot with the signal centred.
  **A transmission on the air starts shortly after the slot boundary.** So the
  send path plays **the 12.64 s signal**, not the padded slot, and starts it at
  a stated offset from the boundary that `Ft8Slots` computes.
  - **Additive only**: leave `Compose` and its existing route exactly as they
    are - unit 254's 117-message corpus decodes a full slot and must keep
    passing. Add a second route, from the port's own `Ft8Waveform.Synthesize`.
    **Do not trim the padding off an array and call it a signal** where the port
    will hand you the signal directly. *must-pass*
  - **State the offset you chose and why**, in seconds, and **measure the result
    from the arrays rather than asserting it**: signal length, start offset,
    and that it fits inside the slot. `Ft8Slots.TransmissionFits` exists.
    *must-pass*
  - **This closes the open half of step 2's criterion 5.** Say so in the report.
- **The transmitted slot is recorded to telemetry, with what was sent and
  when - within HM-DEC-018.**
  - Add the transmit category to `TelemetryCategory`. The enum's own comment
    says that is a deliberate act; this is one, so say why in the commit.
  - **The event carries the shape of the transmission and never its words**: the
    slot's start in UTC, the frequency, the duration, the sample rate, the
    message *type* and the message *length*. **No callsign. No message text.
    Not `Ft8Transmission.Text` and not `ReadsBackAs`.** *must-pass*
  - **A test proves the words cannot get in**, in the shape
    `ADecodeWindowCannotCarryDecodedText` already uses at
    `RecordDisciplineTests.cs:105`: compose something with a callsign in it, run
    a transmission through a recording sink, and assert the callsign appears
    nowhere in what was written. *must-pass*
  - This is not a new ruling and is not to be re-argued: `DECISIONS.md` already
    ruled it for a transmitted CW `CQ`.

**THE DROP CANDIDATE: the on-disk telemetry round trip through `JsonlTelemetry`,
and the 48000 Hz repeat of the placement measurement.** If this unit runs long,
**shed those two and keep the 12000 Hz placement measurement, the fake-sink
telemetry assertion and the test that the words cannot get in.** Say in the
report that they were dropped and why. **Do not shed task 2 or task 3 to save
task 4.**

### Task 5 - the record

**File edits only.**

- Append this unit's entry to `PHASE_OUTCOME.md` through
  `tools\arbiter\outcome-append.bat`. **It has been refused for three
  consecutive units. If it is refused again, append with the file-editing tools
  in the exact format the existing entries use** - twelve fields, same names,
  same order, ASCII - **and record the refusal verbatim.**
- **Step 1's header line stays `partial`** and **step 2's header line stays
  `partial`.** Neither is this unit's to change; step 2's criterion 4 is
  deferred to the operator and is not a state change.
- Set step 3's header line to the state the work actually reached. **It will not
  be `done`** - criteria 1 and 3 are the next unit's - so the honest answer is
  `partial` or `in progress`, and which one is your reading of what you finished.
- **`PHASE_STATUS.md`'s `CURRENT_STEP:` still says 1 and is stale.** Set it to 3.
  Do not write a `HEARTBEAT:` line by hand and do not put any key below the
  `---` rule.
- `PROJECT_STATUS.md` final write, `UPDATED` read from the clock.

---

## Parked - do not touch, do not raise

- **Opening a real audio device, playing sound, the WASAPI render seam, and the
  loopback.** The next unit's, and named as such.
- **Anything to do with the radio's own input level.** Deferred to Tim.
- **Contact state and the four row states.** Step 4.
- **The right-click menu, the CQ button, the Send area, any UI at all.** Step 5.
- **`Ic7300Rig`, `CivWrites.TuneNow`, `BandScanner`, `AbortCw`, CW send,
  auto-CQ.** Name them in the survey; change none of them.
- **Automatic sequencing.** Deliberately out of this phase.
- **Logging**, FT4, PSK31, WSPR transmit.
- **Anything in `src/Ft8Sharp/` and `src/Ft8Sharp.Deep/`.**
- **`src/Hamlet.App/`** in its entirety, including the dummy-load string in
  `MainWindow.axaml` that unit 253 logged.
- The OSD re-encoding count, `ReusableWindow`, `ProcessDelayForTests`, the tap's
  owner, the waterfall's first row, unit 237's Extensible conclusion, work
  instruction 231's four tree items, `validate-output.bat`'s permitted-spellings
  bug, the 101.33 ms pulse above 6 kHz, the CW decoder and its inherited reds.

---

## Logged, not chased

**Unit 254's section 4 raised nothing - it reported no blockage and asked for no
ruling.** Two things it recorded in section 3 are carried here so they are not
lost, and **neither is a ruling request**:

1. **The five hashed compound callsigns that do not read back alone.** Proved
   conditional, correct FT8 behaviour, not a defect. Nothing to do.
2. **The slot placement finding.** It is not logged - it is task 4.

**Unit 253's banked item stands and is still the owner's:** `TransmitGuard.Check`
permits when the operator's toggle is off. **Task 3 answers it for this send path
only, by construction, and does not touch the other callers.** Do not widen it.

---

## What not to do

- **Do not open a serial port, an audio device, or anything else that touches
  hardware.** Every test in this unit runs against a fake.
- **Do not play sound.**
- **Do not write a WASAPI render class, a device enumerator, or a loopback.**
  That is the next unit and taking it tonight is how this one fails to land.
- **Do not change a line of `src/Ft8Sharp/`** - not a constant, not a comment.
- **Do not change `TransmitGuard.Check`, and do not change any existing
  caller's behaviour** to make your gate consistent with itself.
- **Do not change `Ft8Composer.Compose`'s signature or what it returns.** Add
  beside it.
- **Do not weaken `TheSeamNamesNoRadioNoDeviceAndNoEncoderOfItsOwn`** to let your
  new code sit in `Ft8Composer.cs`. If it goes red, your code is in the wrong
  file.
- **Do not put a callsign or a message's text into telemetry.** Not once, not in
  a debug line, not behind a flag.
- **Do not add a compensating control** - no confirmation dialog, no power limit,
  no test mode, no dummy load.
- **Do not wire the sequence to anything.** No caller, no UI, no service
  registration.
- **Do not close, reopen or re-argue steps 1 or 2.**
- **Do not run a test suite.** Only the tests you just wrote, plus the one named
  exception granted above, filtered by exact name, foregrounded, with a timeout.
- **Do not background a command and poll for it.**
- **Do not run `Hamlet.App.Tests`.**
- **Do not compose a timestamp.**

---

## Committing and pushing

Commit and push each task before starting the next. Bump the root version's patch
by one per task from `1.12.85`. **`Ft8Sharp` does not move.**

---

## Reporting

`output.md` at the repository root, overwritten, four sections per
`CLAUDE_CODE.md` section 8.

**First, above everything, the ordering block. `validate-output.bat` refuses a
report without it.** It must carry the literal words `READ IN THIS ORDER`, then
three paragraphs beginning `A.`, `B.` and `C.` at the start of a line, all inside
the first 60 lines of the file, and **C must contain the literal phrase `raises N
items`** with a real number.

- **A - the phase goal and where every step stands.** Hamlet works stations on
  the air. Step 0 done; step 1 partial at four of five; **step 2 partial, closed
  by the arbiter, with criterion 4 deferred to Tim and criterion 5's open half
  taken by this unit**; **step 3 is this unit's and is not finished by it**;
  steps 4, 5 and 6 not started.
- **B - this step and its exit criteria, and which were met.** Step 3's six:
  audio plays to the radio's USB input; **key, transmit, unkey, with the unkey
  happening even if the audio path throws**; a loopback proves the whole chain;
  the transmitted slot recorded to telemetry; **the licence gate in the path,
  asserted by a test**; **nothing keys without an operator action reaching this
  code**. **Say which of the six this unit met, and say plainly that 1 and 3 -
  the device and the loopback - were not attempted and are the next unit's.** Do
  not report a criterion as met because it was designed for.
- **C - what this report adds, weighed against A and B.** How many items section
  4 raises, in the words `raises N items`, and **whether any of them is in the
  way of a criterion named in B.** If none is, say so - that is a real answer and
  it is not a ruling request.

**Then the six-line header block**, from the clock, never composed:
`UNIT:`, `PHASE GOAL:`, `UNIT GOAL:`, `ADVANCED:`, `NUMBER:`, `DRIFT:`.
**`NUMBER:` for this unit is how many of the failure modes in task 2 leave the
radio unkeyed**, of how many tried - the sink throwing, the sink cancelled, the
port throwing on the way out, the gate refusing, and whatever else you exercise.
**`DRIFT:` - work instruction 254 reported 0.**

**Section 3 leads with three things, in this order:**

1. **The unkey, and what was watched.** Every failure mode you exercised, named,
   and for each one what the fake transport actually saw on the wire - the key
   byte, the unkey byte, the abort's two frames - **quoted as bytes, not
   described.** And the breakage you watched go red before it went green.
2. **What already existed and what this unit added** - the sequence's file and
   line, the audio interface's, the new CI-V constant's, the grep showing
   **nothing in the tree calls the sequence**, and the grep showing no timer can
   start one. Confirm no line of `src/Ft8Sharp/` changed and that
   `TransmitGuard.Check` is untouched.
3. **Where the transmission starts and what telemetry recorded** - the offset you
   chose and why, the measured signal length and start, the telemetry line's
   fields quoted in full, and **the proof that no callsign reached it**. If the
   drop candidate was dropped, say so here.

**Section 4 is for what is genuinely in the way.** *Nothing is blocking* is a
real answer and is written as one sentence. **A note, an observation or a
recommendation you have already acted on is not a ruling request** - put it in
section 3. Ask the owner to decide something only where work is actually stopped
until he does.

Write `output.md`, then stop. Do not start the next unit.

---

```
ARBITER-DECISION
STEP: 3
APPROACH: Build the transmit sequence - gate, key, play through an audio sink interface, unkey guaranteed even when the sink throws - against a fake CI-V transport and a fake sink, with the licence gate refusing inside the path and TransmitAbort as its first caller, opening no real device and playing no sound
MOVE: continue
WHY: Step 3 has had no unit spent on it, and it is the join the whole phase turns on - steps 5 and 6 both wait on something that keys. Two of its six criteria need hardware this machine does not have, since SHACK_FACTS.md FACT-004 rules no radio has ever been attached here and no measurement of this machine's endpoints says anything about the radio, so this unit takes the four that are fully provable against fakes and leaves the render device and the loopback whole and named for the next one. The loop test was run on this approach and returned NOT FOUND; step 3 has zero units spent, nothing on it has failed, and unit 253's fake-transport method being reused is a method in common, not a repeated approach.
STATE: not started
DECIDED: Three on my own authority. First, step 2 is closed at partial and its criterion 4 is deferred to Tim rather than chased - the IC-7300's USB modulation input level is nowhere in this repository and FACT-004 forbids inferring it here, so what Tim must do is named instead. Second, step 3 is split: the sequence tonight against fakes, the render device and the loopback next, because the shape of a keying path must be right before any real device is opened and a device measured here measures the wrong hardware. Third, the send path treats TransmitGuard's overridden permit and its unknown-class permit as refusals, without changing TransmitGuard.Check or any existing caller - which implements PHASE_PLAN.md's non-negotiable that the gate is not bypassable from any send path, strictly narrows new code only, and leaves unit 253's banked owner-class question about the other callers exactly where it was.
LICENCE: PHASE_PLAN.md, the steps are a hypothesis not a contract - the arbiter may replace a step with a better approach and move a target found to have been measured wrong, recording the evidence - together with its named alternatives to stopping: a target not reached is closed with the figure reached, and where the radio is wanted the step is marked deferred with what Tim must do. The three things the arbiter may not reason past license the ordering directly: the abort was watched to fire in unit 253, so a keying path may now ship, and it ships with the gate in it.
ACCOMPLISHED: Hamlet can key a radio, hand it a transmission, and come out of transmit - and it comes out of transmit when the audio throws, when the audio is cancelled, when the port dies on the way out and when the licence gate refuses before a single byte is sent. The abort has its first caller. Nothing in the tree can start a transmission, nothing can start one on a timer, and the record of a transmission carries its shape without carrying anybody's callsign.
ADVANCES: step 3, exit criteria 2, 4, 5 and 6 - key/transmit/unkey with the unkey surviving a throwing audio path, the transmitted slot recorded to telemetry, the licence gate in the path asserted by a test, and nothing keying without an operator action reaching the code. Criteria 1 and 3, the device and the loopback, are deliberately left for the next unit. It also closes the open half of step 2's criterion 5, where in the slot the transmission starts, which unit 254 measured wrong in the port and correctly refused to fix there.
END-ARBITER-DECISION
```

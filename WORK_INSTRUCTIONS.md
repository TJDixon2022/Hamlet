# Work instruction 256 - the audio leaves the machine, and Hamlet's decoder hears it come back

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

**Rule 2 is sharper than usual in this unit, and read this before task 3.** This
is the first unit in the phase whose tests **run in real time**: one FT8
transmission is 12.64 seconds of audio and a loopback plays it and captures it at
the speed of sound leaving a sound card. **A test that plays ten messages is over
two minutes of wall clock in one command, before the decode.** Task 3 caps the
count for that reason and the cap is a *must-pass*, not a suggestion. Write
`PROJECT_STATUS.md` before you start a real-time test run and again when it
returns, so the watchdog sees a live loop.

---

## THE TOOL RULE

**This session's shell may refuse calls.** A refused shell call is a signal to
reach for the other tool, not to stop. **The file-editing tools have been
unaffected throughout.** Record every refusal verbatim. **Nothing in this unit
halts the loop.**

**Four tool facts earlier units paid for:**

1. **`tools\arbiter\outcome-append.bat` has now been refused for four
   consecutive units** - 253 twice, 254 once, 255 twice. **Expect it.** See task 5.
2. **`tools\arbiter\validate-output.bat` was refused in seven forms** by unit
   255, including with the sandbox override. **This shell will not start a batch
   file.** Try it once; if it is refused, validate the report by hand against the
   validator's own seven rules the way unit 255 did, and say plainly that it was
   a hand check and not an exit code.
3. **The shell refuses compound commands** - anything with `&&`, a `;`, or a pipe
   into a second program is judged as multiple operations and refused whole. Run
   one program per call. **The arbiter authoring this instruction was refused
   twice on exactly that, including a `powershell -NoProfile -Command` that
   enumerated sound devices** - which is why task 1 exists in the shape it does.
4. This shell collapses a doubled backslash inside a quoted heredoc, so use the
   file-editing tools for anything with escapes in it; and unit 253 recorded
   `grep` aborting with exit 134 on multi-pattern and `-c` invocations, so run one
   pattern at a time if that happens.

---

## Why this unit exists

**The count today.** Step 0 `done`, one unit. Step 1 `partial`, two units. Step 2
`partial`, two units. **Step 3 `partial`, two units** - four of its six criteria
met. **Steps 4, 5 and 6 have had no unit spent on them at all.**

Hamlet composes a message into a slot of audio its own decoder reads back, has an
abort watched to fire 14 of 14 in 22 ms, and has a sequence that keys, plays and
unkeys with the unkey surviving all six failure modes anyone could construct.
**Every one of those was proved against a fake.** `ITransmitAudioSink` has one
implementation in this repository and it is `FakeTransmitAudioSink`. **No sample
Hamlet has ever composed has left this machine.**

**That is the gap this unit closes, and it is step 3's own closing evidence.**
`PHASE_PLAN.md` step 3, criterion 3, in its own words: *a loopback proves the
whole chain - generate, play, capture on the tap, decode, and get the message
back. **This needs no antenna and no radio state** and is the closing evidence
for this step.* The plan asserts the loopback is reachable without hardware. This
unit is where that assertion gets tested.

**Criterion 1 is cut down and you are told so up front.** *Audio plays to the
radio's USB input at the right device, rate and level.* `SHACK_FACTS.md` FACT-004
rules that the IC-7300's USB codec is not present on this machine and that **what
format tag, channel count, sample rate or encoding it declares is unknown from
this side and may not be inferred.** So the radio-side half of criterion 1 is not
reachable by any unit and is **deferred to Tim, alongside step 2's criterion 4**.
The half that is reachable - a real render path that opens a *named* endpoint,
asks for a rate, reports the rate it actually got, converts the floats to
whatever the endpoint speaks, and says how many samples went out - is this
unit's, and it is most of the engineering.

**The distinction you must hold all night:** a measurement of this machine's
render endpoint is **evidence about Hamlet's software chain** and is exactly what
the loopback is for. The same measurement offered as **evidence about the radio**
is the mistake FACT-004 names. Every device figure in your report carries the
words *development machine*.

```
PHASE GOAL:   Hamlet works stations on the air.
UNIT GOAL:    A composed FT8 transmission is played to a real output endpoint
              by a real ITransmitAudioSink, captured back off that same
              endpoint, resampled, and decoded by Ft8SlotDecoder to the message
              that went in. The whole chain, on one machine, with no radio and
              no antenna.
ADVANCES:     step 3 - exit criterion 3 (a loopback proves the whole chain:
              generate, play, capture, decode, get the message back) taken
              whole, and exit criterion 1 (audio plays at the right device,
              rate and level) cut down to the render path, the device, the rate
              and the level a development machine can prove, with the radio's
              own expected input level named and left with Tim.
```

---

## Verify this instruction against the tree

**Nothing below describes the tree with authority. It was read at 2026-09-06 and
line numbers move.** Check every claim, match on the symbol rather than the line,
and **report mismatches in section 4 - do not repair this instruction and do not
stop on one.**

What was read, and where:

- **`src/Hamlet.RadioEngine/Transmit/ITransmitAudioSink.cs`** - `interface
  ITransmitAudioSink` at `:40`, one method `Task<PlayedAudio> PlayAsync(
  ReadOnlyMemory<float> samples, int sampleRate, CancellationToken)` at `:51`,
  and `readonly record struct PlayedAudio(int SamplesPlayed, TimeSpan Took)` at
  `:15`. **This is the interface you implement and you do not change it.** Its
  remarks at `:35` say *"nothing in this repository implements it for a real
  device"* and *"the render implementation is the next unit's."* **You are that
  unit; update that paragraph to say what now does.**
- **`src/Hamlet.RadioEngine/Transmit/Ft8TransmitSequence.cs`** - `OperatorSend(
  Ft8Transmission, long FrequencyHz, LicenseClass, bool GuardEnabled, DateTime
  SlotStartUtc, double StartSecondsIntoSlot)` at `:92`; `TransmitRun` at `:117`
  with `RadioIsInReceive` at `:163`; `Ft8TransmitSequence` at `:197`; `RunAsync`
  at `:249`. **The gate is the first thing in `RunAsync` and nothing before it
  can key** - the comment at the top of the method body says so. `PlayAsync` is
  called at roughly `:286` and a short play is already treated as `AudioFailed`.
  **Do not change this file's behaviour.** If your real sink makes it necessary,
  that is a finding for section 4, not an edit.
- **`src/Hamlet.RadioEngine/Audio/WasapiAudioSource.cs`** - `WasapiAudioDevices :
  IAudioDevices` at `:17` with `List()` at `:20`, which enumerates
  **`DataFlow.Capture` only** (`:30`) and returns an empty array rather than
  throwing on a machine with no sound card. `WasapiAudioSource : IAudioSource` at
  `:121`. **Read the remarks at `:108`: "The only class in the engine that knows
  what a sound device is."** That sentence is a design claim and your new sink
  will make it false unless you deal with it. See task 2.
- **`src/Hamlet.RadioEngine/Audio/AudioTap.cs`** - `AudioTap` at `:68`,
  `SecondsKept = 30` at `:76`, `Take(ReadOnlySpan<float>, int)` at `:192`,
  `Snapshot()` at `:313`, `Window(long, int)` at `:381`, `Tail(TimeSpan)` at
  `:426`, `AudioLevel(PeakDb, FloorDb, ...)` at `:17` with `NearlySilent` at
  `:42`. **This is "the tap" criterion 3 names.** It is a ring buffer fed by
  `Take`; it opens nothing itself.
- **`src/Hamlet.RadioEngine/Audio/Ft8Resample.cs`** - `ToFt8Rate(MonoAudio)` at
  `:59`, `TargetSampleRate = 12_000` at `:32`, `Resample(...)` at `:83`. **The
  decoder wants 12 kHz and an endpoint will not give you 12 kHz.** This is how
  you get there and you do not write a second resampler.
- **`src/Hamlet.RadioEngine/Audio/WavAudio.cs`** - `MonoAudio(int SampleRate,
  float[] Samples)` at `:9`, `Write(string, MonoAudio)` at `:42`, `Read(string)`
  at `:97`.
- **`src/Ft8Sharp/Dsp/Ft8SlotDecoder.cs`** - `Ft8SlotDecoder` at `:51`,
  constructor at `:81`, **`Decode(ReadOnlySpan<float> samples)` at `:133`**
  returning `Ft8SlotResult`. **Parked: not a line of `src/Ft8Sharp/` changes.**
  `src/Hamlet.RadioEngine/Transmit/Ft8Composer.cs` already constructs and calls
  it - **read how, and reuse that, rather than working out the constructor's
  arguments from scratch.**
- **`tests/Hamlet.RadioEngine.Tests/Transmit/`** holds
  `FakeTransmitAudioSink.cs`, `HamletsOwnDecoderReadsBackWhatHamletComposedTests`,
  `TheSeamTurnsWordsIntoASlotOfAudioTests`,
  `WhatTheTransmissionLooksLikeAsAudioTests`,
  `TheUnkeyHappensWhateverGoesWrongTests`,
  `TheLicenceGateIsInsideThePathTests` and
  `WhereTheTransmissionStartsAndWhatTheRecordSaysTests`. **All seven are green as
  of unit 255 and none of them opens a device. Do not make any of them open
  one.**
- **`WhatTheTransmissionLooksLikeAsAudioTests` contains
  `TheSeamNamesNoRadioNoDeviceAndNoEncoderOfItsOwn`**, which goes red if a rig
  type, a PTT or a CI-V reference is put into `Ft8Composer.cs`. **Do not weaken
  it.** If it goes red, your code is in the wrong file.
- **NAudio 2.2.1 is already referenced** by `src/Hamlet.RadioEngine/Hamlet.RadioEngine.csproj:22`
  and by `src/Hamlet.App/Hamlet.App.csproj:30`. **No new package reference is
  needed and none is to be added.** `tests/Hamlet.RadioEngine.Tests` gets NAudio
  transitively through the engine - verify that rather than adding it.
- **The only playback in the repository is `src/Hamlet.App/Audio/ModeAudioPlayer.cs:92`**,
  a `WaveOutEvent` for training tones, **in the UI project, which this unit does
  not touch.** Read it for how the existing code drives NAudio output; copy
  nothing from it into the engine without saying why.
- **Root version was `1.12.89` in `Directory.Build.props:205`.** Read it; do not
  assume it.
- **`docs/unit256-*` is free.** **Do not overwrite any existing `docs/unit25*`
  file** - the numbering collided when this phase restarted and
  `docs/unit254-combining-depth.md` belongs to the previous phase.

Arithmetic worth checking rather than trusting: at 12000 Hz the signal
`Ft8Composer.ComposeSignal` produces is 151,680 samples, 12.64 s; at 48000 Hz it
is 606,720. An endpoint that negotiates 48000 Hz stereo 32-bit float wants
606,720 frames and 4,853,760 bytes for one transmission. **Check that against
what the endpoint actually declares rather than against this paragraph.**

Known reds, inherited, **never chased**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list
tripwire.

---

## What the reload measured, and what to do about it

**Reported to you, not repaired by the arbiter.** Handle these in task 1 and do
not spend a night on them.

1. **Three modified files at the root are uncommitted** - `PHASE_OUTCOME.md`,
   `PHASE_STATUS.md`, `RUN_LEDGER.md` - plus this instruction. They are unit
   255's records. **Commit them with your first task.**
2. **`tools/unit254-seam-grep.sh` is still untracked.** Unit 254 could not delete
   it and unit 255's single attempt was refused verbatim. **Do not try a third
   time.** It is in no commit, it is under `tools/`, and it has now cost two
   units a paragraph each. **Leave it, and say in one line that it is still
   there.** That is the whole of your obligation to it.
3. **`RULES_AT` disagrees again**, identically: `PROJECT_STATUS.md` says
   `HM-DEC-157 (2026-09-06)` while the reload reports `CLAUDE.md` §1's highest as
   `CPS-DEC-0152`. **Unit 255 already answered this and the answer stands** -
   §1's table is `HM-DEC-` throughout, its top row is `HM-DEC-152`, the prefix
   `CPS-DEC-` appears nowhere in this repository, and `HM-DEC-155/156/157` are in
   `DECISIONS.md` but not yet indexed into §1's table. **`RULES_AT` is ahead of
   the index, not ahead of the record.** **Spend nothing on it. Change nothing.
   Do not re-derive it.** One sentence in section 3 pointing at unit 255's
   finding is the correct treatment.

---

## Steps 1 and 2 are closed and are not yours

**Step 1 is `partial` at four of five** and stays there. Its fifth criterion -
*no transmitting code exists yet when this step closes* - was ruled unmeetable in
its letter, because `Ic7300Rig.SendCwAsync` and `CivWrites.TuneNow` pre-date the
phase and sit on parked surfaces. **Do not reopen it.**

**Step 2 is `partial`** and stays there. Criteria 1, 2 and 3 are met. **Criterion
4's remaining half - the level the IC-7300's USB modulation input expects - is
deferred to Tim and no unit will close it.** What Tim must do, unchanged and to
be repeated in your report: **set the drive level at the radio by watching ALC on
the first live transmission, and write the figure into `SHACK_FACTS.md` as a
fact.** Criterion 5 was closed by unit 255 at 0.5 s into the slot.

**Do not measure a development-machine endpoint and offer its level as the
radio's.** That is the single most tempting error available to this unit, because
you will have a real peak and a real RMS in your hand by task 2 and they will
look like an answer.

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
  before its abort is watched to fire.** It was watched, unit 253, and it has its
  first caller as of unit 255.
- **Hamlet never transmits outside the operator's licence privileges. The
  Settings gate is not bypassable from any send path.**
- **Section 0.0.** Never present a guess as a decode. **A loopback that did not
  decode is a loopback that did not decode.**
- **Section 0.1.** The engine is not told that tabs exist, and references no UI
  assembly.
- **Section 12.1.** Nothing interprets a message.
- **HM-DEC-018.** **Telemetry never carries a callsign, decoded message content,
  or anything identifying a person or a contact.** Unit 255's `TransmitRecord`
  has no string parameter at all, deliberately. **Keep it that way.**
- **`Ft8Sharp` is a faithful MIT port and nothing in this phase changes a line of
  it.** `Ft8Sharp.Deep` is GPL-3.0.
- **`SHACK_FACTS.md` FACT-004.** There are two computers and only one has a
  radio. **No measurement of the development machine's audio endpoints says
  anything about the radio.** An absent device, an empty folder or a capture
  count of zero here is the expected state and is not a finding. **Which machine
  a piece of evidence came from is part of the evidence.**

**And the plan's own alternative to stopping, which applies to this unit
directly:** if the tree disagrees with this instruction, **the tree wins** -
report the mismatch and continue. If **the machine** disagrees with this
instruction - there is no render endpoint, or loopback capture will not start -
**that is task 1's finding and task 3 has a named second route. Take it and say
you took it. Do not stop.**

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` **read from the clock,
never composed**, and `NOTE` saying what is moving inside the task. The same
every ten minutes while a task is running. **Use the file-editing tools if the
shell refuses.**

**And once more before and once after every real-time audio run in task 3.** A
single loopback run is a quarter of a minute of silence to a watchdog that fires
at twelve.

**Unit 253 wrote three `UPDATED` values it composed, one of them 39 minutes ahead
of the real clock.** Read the clock.

---

## Tasks

Five tasks. **Task 1 is measuring only. Task 3 is the goal task.** Task 4 carries
the named drop candidate.

### Task 1 - what this machine can actually play and capture

**Measuring and reading only. Change nothing under `src/` in this task.** Commit
the uncommitted root files at the start of it.

**This task exists because the arbiter could not run it.** Two attempts to
enumerate sound devices from the authoring shell were refused. **So this
instruction does not know whether this machine has a render endpoint**, and
everything after it is written to work either way. **Answer it first, from the
machine, before you write a line of task 2.**

Measure and write down:

- **Does this machine have an active render endpoint?** Enumerate
  `DataFlow.Render`, `DeviceState.Active`. Name each one, its friendly name, and
  which is the default for `Role.Console`. **If the shell refuses to run a
  command that enumerates them, write a tiny throwaway xunit test that does it
  and prints, run it filtered by exact name** - that is a permitted use of the
  test runner because it is a test this instruction constructs.
- **For the default render endpoint: what mix format does it declare?** Sample
  rate, channel count, bit depth, encoding tag. **Label it *development machine*
  and say in the same sentence that it says nothing about the IC-7300's codec.**
- **Is `WasapiLoopbackCapture` available and does it start?** This is the
  go/no-go for task 3's first route. Start it, take one buffer, stop it, and say
  what came back - the format, and whether any samples arrived at all.
- **Is there a render endpoint that is not the speakers?** A virtual cable, a
  monitor's audio, a disconnected headphone jack. **Say which endpoints exist and
  which you would choose**, because task 3 plays 12.64 seconds of FT8 tones out
  loud on this machine and it may be the middle of the night.
- **What `WasapiAudioDevices.List()` covers and does not.** It enumerates
  `DataFlow.Capture` only. **Say whether a render list belongs beside it, in it,
  or nowhere** - and note that `IAudioDevices` is an existing interface with
  existing callers you are not to break.
- **How `Ft8Composer` constructs and calls `Ft8SlotDecoder`**, with the file and
  line, and what `Ft8SlotResult` gives you back. **You will reuse that exact
  construction in task 3.**
- **What `AudioTap.Take` needs to be fed** and what `Snapshot`, `Window` and
  `Tail` return, and whether the tap can hold 12.64 s at the endpoint's rate
  inside its `SecondsKept = 30`.
- **Say what is missing**, explicitly, rather than assuming it is nothing.

**Write it to `docs/unit256-render-and-loopback-survey.md`** with file and line
throughout for the code, and with the words *development machine* on every device
figure. **End it with an explicit go/no-go line for task 3's first route:**
`LOOPBACK ROUTE: device` or `LOOPBACK ROUTE: file`, with the measurement that
decided it.
*must-pass: the survey exists, carries the go/no-go line, and every device figure
is labelled with which machine it came from.*

### Task 2 - a real sink, and the device it opens

**The render implementation. It plays sound; it keys nothing.**

- **A real `ITransmitAudioSink`** using NAudio's WASAPI output. It takes the
  samples and a rate, plays them, waits for the buffer to drain, and returns a
  `PlayedAudio` with **how many samples actually went out and how long it took,
  both measured rather than assumed.** *must-pass*
- **It opens a device chosen by the caller and never silently falls back.** A
  device id or name comes in; if it is not there, the sink **fails loudly**
  rather than playing to whatever the machine happens to default to. **A
  transmission going to the wrong endpoint is the software shape of a
  transmission going out on the wrong band.** *must-pass*
- **It states the rate it asked for and the rate it got.** WASAPI shared mode
  will resample or refuse; exclusive mode will refuse or accept. **Say which mode
  you used and why, and report both rates.** If the endpoint will not take the
  transmission's rate, that is a fact to report, not a reason to quietly hand it
  a different one. *must-pass*
- **The float-to-endpoint conversion is yours and it is where clipping lives.**
  Samples arrive in -1 to +1. State the peak you wrote, the format you wrote it
  in, and whether anything clipped. *must-pass*
- **It knows nothing about a radio.** No PTT, no `ISerialPort`, no rig type, no
  CI-V. A grep of its body proves it, in the shape
  `TheSeamNamesNoRadioNoDeviceAndNoEncoderOfItsOwn` already uses. *must-pass*
- **Where it goes, and the sentence it breaks.**
  `WasapiAudioSource.cs:108` says *"the only class in the engine that knows what
  a sound device is."* **Your sink is a second one.** Decide where it lives, say
  why in the report, and **either keep that sentence true or amend it to say what
  is now also true and why.** Do not leave a design claim in the tree that your
  own code falsifies. *must-pass*
- **Watched to fail first.** The breakage to watch: **a sink that returns
  `SamplesPlayed = samples.Length` without waiting for the buffer to drain.** It
  passes a naive test and is a lie - the radio unkeys while audio is still in the
  card. Build it that way, watch a test that measures elapsed time against the
  transmission's own duration go **red**, then fix it and watch it go green.
  **Name the breakage and what it caught.** *must-pass*
- **A short play is still a short play.** Prove the real sink reports fewer
  samples when it is cancelled part way through, and that
  `Ft8TransmitSequence` still calls that `AudioFailed` - **without changing
  `Ft8TransmitSequence`.** *must-pass*
- Run the tests **filtered by exact name, foregrounded, with a stated timeout.**
  **A cancelled part-play is a real-time test; keep it to one or two seconds of
  audio, not 12.64.**

### Task 3 - the loopback. THE GOAL TASK.

**Compose, play, capture, decode, and get the message back.** This is step 3's
criterion 3 in the plan's own words and it is the reason this unit exists.

- **The chain, end to end, in one test:** `Ft8Composer` composes a message →
  the task 2 sink plays it to a real render endpoint → the same endpoint is
  captured → the capture goes through `AudioTap` → `Ft8Resample.ToFt8Rate` →
  `Ft8SlotDecoder.Decode` → **the message that comes out is the message that went
  in, compared as text.** *must-pass*
- **The capture goes through `AudioTap`.** Criterion 3 says *capture on the tap*
  and the tap exists. Feed it with `Take` and read it with `Window` or `Tail`.
  **Do not build a second ring buffer.** *must-pass*
- **THE CAP: at most three messages, and one of them is enough to pass.**
  Three transmissions is roughly 40 seconds of wall clock before decoding.
  **Prove one message end to end first, commit that, and only then try the other
  two.** *must-pass: the test run's wall-clock time is stated in the report.*
- **The two routes, and task 1 chose between them.**
  - **`LOOPBACK ROUTE: device`** - the real one. Render to an endpoint,
    `WasapiLoopbackCapture` that same endpoint, decode. **This is the route that
    proves the chain and it is the one to attempt.**
  - **`LOOPBACK ROUTE: file`** - the named fallback, if and only if task 1
    measured that this machine has no usable render endpoint or that loopback
    capture will not start. Play through the real sink to whatever will take it,
    and capture the samples the sink actually wrote to the device buffer on their
    way out - **the conversion, the rate and the format are still exercised;
    only the sound card is not.** **Say in the report, in one plain sentence,
    that the device half was not proved and why.**
  - **You may not silently take the file route.** Taking it requires task 1's
    measurement quoted in the report. *must-pass*
- **Watched to fail first, and the breakage is specific.** **Lie about the
  capture's sample rate** - hand `Ft8SlotDecoder` the endpoint's 48 kHz samples
  as though they were 12 kHz, or skip `Ft8Resample` entirely - and **watch the
  decode return nothing.** That is the real defect this test exists to catch: a
  chain that plays perfectly good audio and hears silence because one number was
  wrong on the way back. Watch it red, fix it, watch it green. *must-pass*
- **A decode that did not happen is reported as a decode that did not happen**
  (§0.0). If the message does not come back, **say so with what did come back -
  how many candidates, what SNR, what the tap's `AudioLevel` said** - and the
  unit is still a success for having measured it. **Do not loosen the comparison
  to make it pass.** Do not compare on a substring, a prefix, or a callsign; the
  whole message text or nothing. *must-pass*
- **It plays sound out of this machine.** Say so in the report - which endpoint,
  at what peak, for how long, how many times. That is a side effect on a real
  computer and it is part of what happened.
- Run it **filtered by exact name, foregrounded, with a stated timeout that
  accounts for real time**, and write `PROJECT_STATUS.md` before and after.

### Task 4 - the level, the rate, and what stays Tim's. **Drop candidate here.**

- **State what the render path measured**, all labelled *development machine*:
  the endpoint's declared format, the rate asked and the rate got, the peak and
  RMS of what was written, and whether anything clipped.
- **And then state, in the same section, that none of it is the radio's number.**
  Step 2's criterion 4 stays deferred: **Tim sets the drive level at the radio by
  watching ALC on the first live transmission and writes the figure into
  `SHACK_FACTS.md` as a fact.** *must-pass: the report says both things, and does
  not offer the first as the second.*
- **Mark step 3's six criteria, one line each, met or not met, on what evidence**
  - the four unit 255 met, criterion 3 as this unit leaves it, and criterion 1 as
  the cut-down it is, with the deferred half named. *must-pass*
- **The transmission that went through the loopback is recorded to telemetry the
  same way**, through unit 255's `TransmitRecord`, **and still carries no
  callsign.** Re-run the existing discipline test rather than writing a new one,
  and say it is green.

**THE DROP CANDIDATE: the second and third loopback messages, and the WAV
artefact of the captured audio.** If this unit runs long, **shed those and keep
one message proved end to end, the red-then-green rate breakage, and the level
and format measurement.** The count is the right thing to shed because each
message is 12.64 seconds of wall clock and the first one proves the chain; the
second and third only widen it. **Say in the report that they were dropped and
why. Do not shed task 2 or task 3's single message to save task 4.**

### Task 5 - the record

**File edits only.**

- Append this unit's entry to `PHASE_OUTCOME.md` through
  `tools\arbiter\outcome-append.bat`. **It has been refused for four consecutive
  units. Try it once.** If it is refused, append with the file-editing tools in
  the exact format the existing entries use - twelve fields, same names, same
  order, ASCII - **and record the refusal verbatim.**
- **Step 1's header line stays `partial`. Step 2's header line stays `partial`.**
  Neither is this unit's.
- **Set step 3's header line to the state the work actually reached.** `done` is
  available to you this time and it is the honest answer **if and only if** the
  loopback decoded on the device route and criterion 1's reachable half is
  measured, with the radio half named as deferred the way step 2's criterion 4
  is. **If the loopback did not decode, or you took the file route, it is
  `partial`** - and say which criterion is short. **Do not write `done` to tidy
  the file.**
- `PHASE_STATUS.md`'s `CURRENT_STEP:` is 3. **Leave it at 3 unless you closed
  step 3**, in which case the next unstarted step is 4. Do not write a
  `HEARTBEAT:` line by hand and do not put any key below the `---` rule.
- `PROJECT_STATUS.md` final write, `UPDATED` read from the clock.

---

## Parked - do not touch, do not raise

- **Contact state and the four row states.** Step 4, and it is next.
- **The right-click menu, the CQ button, the Send area, any UI at all.** Step 5.
- **Anything Tim does at the radio.** Step 6.
- **`Ic7300Rig`, `CivWrites.TuneNow`, `BandScanner`, `AbortCw`, `KeyerCwSender`,
  `CwTransmitter`, `AutoCaller`, CW send, auto-CQ.** Unit 255's survey named all
  five keying routes; **change none of them and do not re-survey them.**
- **`TransmitGuard.Check` and its three permitting branches.** Unit 253's banked
  owner-class question stands and is still the owner's. The send path answers it
  for itself. **Do not widen it, do not re-raise it, do not touch another
  caller.**
- **`Ft8TransmitSequence`'s behaviour**, the unkey guarantee, and the abort.
  Proved in unit 255. You supply it a real sink; you do not rewrite it.
- **`Ft8Composer.Compose`'s signature and its existing route.** Additive only.
- **Anything in `src/Ft8Sharp/` and `src/Ft8Sharp.Deep/`.**
- **`src/Hamlet.App/`** in its entirety, including `ModeAudioPlayer` - read it,
  change it not - and the dummy-load string in `MainWindow.axaml` unit 253
  logged.
- **Automatic sequencing.** Deliberately out of this phase.
- **Logging**, FT4, PSK31, WSPR transmit.
- **`tools/unit254-seam-grep.sh`.** One line in the report; no third deletion
  attempt.
- **`RULES_AT`.** Answered by unit 255. One sentence pointing at it.
- The OSD re-encoding count, `ReusableWindow`, `ProcessDelayForTests`, the tap's
  owner, the waterfall's first row, unit 237's Extensible conclusion, work
  instruction 231's four tree items, `validate-output.bat`'s permitted-spellings
  bug, the 101.33 ms pulse above 6 kHz, the CW decoder and its inherited reds.

---

## What not to do

- **Do not open a serial port and do not key anything.** This unit's device is a
  sound card. `Ft8TransmitSequence` is still driven by `FakeSerialPort` in every
  test you write. **A real port plus a real sink is a real transmission, and this
  machine has no radio to make one on.**
- **Do not infer the IC-7300's expected input level, format, rate or channel
  count from this machine.** `SHACK_FACTS.md` FACT-004, and it is the error this
  unit is most exposed to.
- **Do not report an absent render endpoint or a failed loopback as a defect in
  Hamlet.** FACT-004 again: on this machine that is the expected state, and it is
  what the file route exists for.
- **Do not change `ITransmitAudioSink`'s signature.** Implement it. If it is
  genuinely wrong, that is section 4.
- **Do not change `Ft8TransmitSequence`, `TransmitGuard.Check`, `TransmitAbort`,
  or `Ft8Composer.Compose`.**
- **Do not change a line of `src/Ft8Sharp/`** - not a constant, not a comment.
- **Do not weaken `TheSeamNamesNoRadioNoDeviceAndNoEncoderOfItsOwn`**, and do not
  make any of unit 254's or 255's seven green test classes open a device.
- **Do not loosen the loopback's comparison.** Whole message text or nothing.
- **Do not write a second resampler, a second ring buffer, or a second decoder.**
  `Ft8Resample`, `AudioTap` and `Ft8SlotDecoder` exist.
- **Do not put a callsign or a message's text into telemetry.** Not once, not in
  a debug line, not behind a flag. `TransmitRecord` has no string parameter and
  is to keep having none.
- **Do not wire the sink to a UI, a service registration or a caller.** Nothing
  in the tree starts a transmission and that is still true when this unit closes.
  Prove it with the same grep unit 255 used.
- **Do not add a compensating control** - no confirmation dialog, no power limit,
  no test mode, no dummy load.
- **Do not add a package reference.** NAudio is already there.
- **Do not run a test suite.** Only the tests you construct here, filtered by
  exact name, foregrounded, with a timeout.
- **Do not background a command and poll for it.**
- **Do not run `Hamlet.App.Tests`.**
- **Do not compose a timestamp.**

---

## Committing and pushing

Commit and push each task before starting the next. Bump the root version's patch
by one per task from `1.12.89`. **`Ft8Sharp` does not move.**

**Commit task 3's first working message on its own, before attempting the second
and third.** The one thing this unit must not lose to a watchdog is a loopback
that decoded.

---

## Logged, not chased

**Unit 255's section 4 raised nothing** - it stated plainly that nothing is
blocking and asked the owner to decide nothing. **It is not a ruling request.**
Three things it recorded in section 3 are carried here so they are not lost, and
none of them is a ruling request:

1. **Five routes reach a keying frame, not two**, and `AutoCaller` at
   `Cw/AutoCall.cs:272` keys repeatedly from one operator start. **It is on the
   parked CW path and unchanged.** It is worth the phase's memory because it is
   the only thing in the engine that keys more than once per operator action, and
   *one click, one message* is this phase's non-negotiable. **Logged. Not this
   unit's. Do not touch it.**
2. **`SetSettingAsync(CivWrites.AntennaTuner, CivWrites.TuneNow)` writes
   `1C 01 02`**, a tuning cycle that transmits, reachable and called by no line in
   the tree. **Logged. Parked.**
3. **The hand-validated report.** `validate-output.bat` could not be started in
   seven forms. **Logged, and this instruction tells you what to do about it in
   the tool rule.**

**Unit 253's banked item stands and is still the owner's:** `TransmitGuard.Check`
permits when the operator's toggle is off. **Not re-raised, not widened, not in
this unit's way.**

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
  the air. Step 0 done; step 1 partial at four of five and closed; **step 2
  partial with criterion 4 deferred to Tim**; **step 3 entering this unit at
  partial with four of six met by unit 255, and leaving it at whatever this unit
  reached**; steps 4, 5 and 6 not started, **and step 4 is next**.
- **B - this step and its exit criteria, and which were met.** Step 3's six:
  audio plays to the radio's USB input at the right device, rate and level; key,
  transmit, unkey with the unkey surviving a throwing audio path; **a loopback
  proves the whole chain**; the transmitted slot recorded to telemetry; the
  licence gate in the path asserted by a test; nothing keys without an operator
  action reaching this code. **Say which of the six stand met now, distinguishing
  the four unit 255 met from what this unit did.** **Say plainly whether the
  loopback decoded, on which route, and what criterion 1's cut-down half leaves
  with Tim.**
- **C - what this report adds, weighed against A and B.** How many items section
  4 raises, in the words `raises N items`, and **whether any of them is in the
  way of a criterion named in B.** If none is, say so - that is a real answer and
  it is not a ruling request.

**Then the six-line header block**, from the clock, never composed:
`UNIT:`, `PHASE GOAL:`, `UNIT GOAL:`, `ADVANCED:`, `NUMBER:`, `DRIFT:`.
**`NUMBER:` for this unit is how many messages went out of the sound card and
came back through `Ft8SlotDecoder` as the same text, of how many tried.**
**`DRIFT:` - work instruction 255 reported 0.**

**Section 3 leads with three things, in this order:**

1. **The loopback, and what was watched.** Which route, the endpoint by name, the
   rate asked and the rate got, the wall-clock time of the run, the message in
   and the message out **quoted as text**, and the tap's `AudioLevel` on the
   captured audio. **And the breakage you watched go red before green** - the
   rate lie, and what the decoder returned when it was told 48 kHz was 12 kHz.
   **If it did not decode, this section says so first and says what did come
   back.**
2. **The sink, and where it lives** - its file and line, the mode it used, the
   float-to-endpoint conversion and the peak it wrote, the short-play case, and
   **what you did about `WasapiAudioSource.cs:108`'s claim to be the only class
   that knows what a sound device is.** Plus the grep showing nothing in the tree
   starts a transmission, and confirmation that `src/Ft8Sharp/`,
   `TransmitGuard.Check` and `Ft8TransmitSequence` are untouched.
3. **The level, the rate, and what stays Tim's** - every device figure labelled
   *development machine*, the six-criterion table for step 3, and the sentence
   naming what Tim must do at the radio. **If the drop candidate was dropped, say
   so here.**

**Section 4 is for what is genuinely in the way.** *Nothing is blocking* is a
real answer and is written as one sentence. **A note, an observation or a
recommendation you have already acted on is not a ruling request** - put it in
section 3. **A machine with no render endpoint is not a blockage** - it is task
1's measurement and task 3's second route. Ask the owner to decide something only
where work is actually stopped until he does.

Write `output.md`, then stop. Do not start the next unit.

---

```
ARBITER-DECISION
STEP: 3
APPROACH: Implement the WASAPI render sink and prove the loopback - compose, play to a real output endpoint, capture that same endpoint through AudioTap, resample and decode back to the message that went in - closing step 3 criterion 3, with criterion 1 cut down to what a development machine can prove
MOVE: cut down
WHY: Criterion 3, the loopback, is fully reachable here - PHASE_PLAN.md says in its own words that it needs no antenna and no radio state and is the closing evidence for this step - so it is taken whole and is the goal task. Criterion 1's radio-side half is not reachable by any unit, because SHACK_FACTS.md FACT-004 rules the IC-7300's codec is absent from this machine and may not be inferred from it, so it is cut down to the render path, the endpoint, the rate and the level, with the radio's own expected level deferred to Tim beside step 2's criterion 4. The loop test was run on this approach and returned NOT FOUND; the two step 3 entries are one unit recorded twice and both say "opening no device and playing no sound", so this is the deliberate complement of what was tried, not a repeat of it.
STATE: partial
DECIDED: Three on my own authority. First, criterion 1 is cut down rather than chased or declared unachievable - the reachable half is real engineering and the unreachable half gets the same named-operator-action treatment step 2's criterion 4 already has, which PHASE_PLAN.md's table licenses directly. Second, step 3 is taken again rather than step 4 being started, because the loopback was deferred once already by unit 255's split and deferring the plan's own named closing evidence a second time is how a step gets quietly abandoned; step 4 is unblocked, unstarted and named as next. Third, the unit is given two loopback routes with the device route mandatory unless task 1's measurement says otherwise, because the authoring shell refused twice to enumerate this machine's sound devices and I could not measure whether a render endpoint exists - so the instruction is written to land either way rather than to assume.
LICENCE: PHASE_PLAN.md's named alternatives to stopping - a target not reached is closed with the figure reached and what was tried, and where the radio is wanted the step is closed on the loopback with what Tim must do named - together with step 3's own criterion 3, which states that the loopback needs no antenna and no radio state. SHACK_FACTS.md FACT-004 licenses the cut-down of criterion 1 and forbids the inference that would otherwise close it.
ACCOMPLISHED: A message Hamlet composed leaves this computer as sound and comes back into Hamlet's own decoder as the same message - the whole chain proved on one machine with no radio and no antenna. The transmit path stops being a thing proved against fakes. What remains of step 3 after this is the one number only Tim can read off the radio.
ADVANCES: step 3, exit criterion 3 - a loopback proves the whole chain: generate, play, capture on the tap, decode, and get the message back - taken whole; and exit criterion 1 - audio plays at the right device, rate and level - cut down to the render implementation, the named endpoint, the rate asked against the rate got, and the level and format measured, with the IC-7300's expected input level left with Tim alongside step 2's criterion 4.
END-ARBITER-DECISION
```

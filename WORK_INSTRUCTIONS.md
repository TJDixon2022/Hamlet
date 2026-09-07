# Work instruction 262 - the rate the endpoint actually speaks

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

**All four were checked against the tree at `HEAD a81d214` while this instruction
was written.** `SHACK_FACTS.md` and
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
258 through 261 wrote `PROJECT_STATUS.md` repeatedly and lost nothing. **The
status write is part of the work, not part of the reporting.**

`dotnet build` is allowed, foregrounded, with a timeout.

**This unit opens no serial port and keys nothing.** It may *enumerate* audio
endpoints and, in task 5 only, *play into one* - and task 5 is the named drop
candidate. Everything else is proved against the `FakePort` and the transmit
fake that units 253 to 256 left in the tree.

---

## THE TOOL RULE

`tools\arbiter\outcome-append.bat` has been refused for **nine consecutive
units**, and `tools\arbiter\validate-output.bat` in seventeen forms across seven.
**Try each once, verbatim, and record the refusal text.** Then do the work with
the file-editing tools in the exact format the script writes - twelve fields,
same order, ASCII, existing entries untouched. **Do not spend a second call on a
refused form and do not treat a refusal as a halt.** `PHASE_PLAN.md`: *if the
shell refuses a call, use the file-editing tools.*

One measured note for the arbiter's own tool, recorded here because it cost a
call tonight: `outcome-read.bat --approach` embeds its argument in PowerShell
single quotes, so **an apostrophe in the approach text is a parse error**. It is
not this unit's to repair.

---

## Why this unit exists

**This is work instruction 262. Nine units have been spent on this phase, 253
through 261.** `PHASE_OUTCOME.md` carries eighteen entries because every unit
since 253 is recorded twice - once from the arbiter's decision block and once
from the session that judged the report. Step 0 is `done`; step 1 reads
`blocked`; steps 2 through 5 are `partial`; step 6 has not started.

```
PHASE GOAL: Hamlet works stations on the air
UNIT GOAL:  The application composes its transmission at the sample rate the
            chosen transmit endpoint actually declares, instead of at the
            decoder's 12000 Hz - because the real sink refuses a rate that is
            not the endpoint's own, and today the application asks it for one
            every single time. Where the endpoint speaks a rate FT8 cannot be
            built at, the send refuses in words at the moment the device is
            chosen, before anything keys.
ADVANCES:   Step 3, criterion 1 - "Audio plays to the radio's USB input at the
            right device, RATE and level." The device half was cut down and met
            by unit 256; the level half is Tim's and is deferred under FACT-004.
            The RATE half has never been measured on the application's own send
            path, and on that path it is wrong.
```

**Read the next paragraph before you form an opinion about the size of the job.**
Units 259, 260 and 261 each closed believing the send path was finished code.
Unit 260's report says *what stands between him and trying is no longer code*;
unit 261 found the missing stop button an hour later. Here is the third thing,
measured from the tree at `HEAD a81d214` while this was written:

| Measured | Where |
|---|---|
| The application composes with **no rate argument**, so it gets the default | `MainWindowViewModel.cs:8120` - `Ft8Composer.ComposeSignal(wanted)` |
| That default is **12000 Hz** | `Ft8Composer.cs:201` -> `Ft8Waveform.cs:59` |
| The real sink **throws** when the rate is not the endpoint's own | `WasapiTransmitSink.cs:301`, message at `:307-308` |
| The endpoint's rate is its **shared-mode mix format** - 48000 Hz on ordinary hardware and on the IC-7300's USB codec | `WasapiTransmitSink.cs:139`, published at `:194` |
| **`ITransmitAudioSink` has no member that tells a caller that rate.** Its own remarks say *"the caller composes at the rate the endpoint declares"* - and no caller can | `ITransmitAudioSink.cs`, the closing `<para>` |
| The radio is **keyed at line 283**; the sink is called at **287-289** | `Ft8TransmitSequence.cs` |

**So on Tim's first real click: PTT goes on, the sink throws, the `finally`
unkeys or fires the abort, and nothing goes out. Every time, on every endpoint
whose mix format is not 12000 Hz.** Nothing illegal happens and nothing is
damaged - the abort is sound and unit 261 proved it - but the phase goal is *a
contact*, and on the path as it stands there cannot be one.

**And here is why the whole suite is green over it.**
`tests/Hamlet.RadioEngine.Tests/Transmit/FakeTransmitAudioSink.cs:43-47` takes
any rate and records it as `RateAskedFor`. **The fake is more permissive than the
thing it stands for.** Every test that exercises the send path hands 12000 Hz to
a sink that will accept anything, and passes. That is the breakage task 2 exists
to catch, and naming it is what licenses adding the test at all.

---

## Verify this instruction against the tree

**Every file and line number above and below was read at `HEAD a81d214`. The tree
may have moved. Where this instruction and the tree disagree, THE TREE WINS.**

**Report the mismatch in section 3.4 of your report and continue. Do not repair
this file, do not adapt silently, and do not stop.** Unit 261 found five
mismatches this way and reporting them was the right answer every time - including
one where the instruction's own table row could not be honoured beside its own
prohibition. **If you find that here, the prohibition wins and the table loses,
and you say so.**

**What is expected to fail, and is not your problem:**

- The known inherited reds - `CwAdjudicationTests.ASpeedChangeInRealisticAudio`,
  the 51 CW cases in `docs/unit239-failing-set.txt`, and the
  `Ft8Sharp.Deep.Tests` whole-type-list tripwire. **Never chase them.**
- **`TheDigitalTabIsTwoColumnsTests` has one red**, and it is waiting on the
  owner. See *Parked*. **Do not touch it, do not edit its assertion, and do not
  count it as yours.**

**Expect `outcome-append.bat` and `validate-output.bat` to be refused.** That is
ten and eight units of the same behaviour; it is the harness, not you.

---

## Rulings in force

**Transcribed in full from `PHASE_PLAN.md`. Not this unit's to re-argue.**

**Tim operates a licensed station on an antenna and Hamlet transmits on the
air.** Ruled 2026-09-06. **HM-DEC-008 and HM-DEC-098, which required a dummy
load, are withdrawn in full** - not a stage, not a fallback, not to be referenced
by any unit. **Do not reference it, do not propose it, do not treat its absence
as a risk.**

**One click, one message.** Ruled 2026-09-06. Hamlet transmits because the
operator clicked. **Never on a timer, never on a decode, never to continue a
contact.**

**Right-click sends immediately, in the next slot, with no confirmation.** Ruled
2026-09-06.

**Nothing is forbidden in the menu.** The expected next message is highlighted;
everything valid stays clickable; a repeat is correct behaviour and shows its
count.

**A contact is never closed by the app.** Complete is shown when the exchange has
what a QSO needs. `73` is politeness, not a requirement.

**`Ft8Sharp` is a faithful MIT port and nothing in this phase changes a line of
it.** `Ft8Sharp.Deep` is GPL-3.0. **This matters tonight**: `Ft8Waveform.cs` is
in the port. You may *call* it at any rate it accepts. **You may not change a
line of it, and you may not change its default.**

**The engine is not told that tabs exist** (§0.1).

**Nothing interprets a message** (§12.1).

**The three the arbiter may not reason past.** (1) **The abort** - every path that
keys the transmitter has a same-thread, no-await abort, CI-V `0x17` with `0xFF`,
PTT off as the fallback. (2) **One click, one transmission** - a transmission he
did not ask for is this phase's one unrecoverable fault. (3) **Licence
privileges** - the Settings gate is not bypassable from any send path.

**`SHACK_FACTS.md` FACT-004: no radio has ever been attached to this machine.** No
measurement taken here says anything about the radio. **Do not infer a radio-side
figure and do not open a serial port to try.** You may enumerate and play into
*this machine's own* audio endpoints, as unit 256 did; that measures this machine
and is stated as such.

---

## One bookkeeping correction, ruled by the arbiter

**Step 1's header line reads `blocked`. It is wrong, and this instruction is the
recorded ruling that corrects it. Do this in task 1's commit.**

The session that judged unit 261's report answered `blocked` on this reasoning:
step 1's fifth criterion, *"No transmitting code exists yet when this step
closes"*, is now permanently violated by the keying sequence and the live CQ send
in the tree.

**That criterion is a sequencing gate and it says *yet*.** It exists so that no
keying path ships before a proven abort. It was measured met at unit 253's close
- a grep for `TransmitAbort` in `src/` returned its own declaration and nothing
else - and it was then overtaken by steps 3 and 5, which the plan's own ordering
required next. Read as a permanent invariant it makes step 1 unclosable unless
steps 3 and 5 are deleted, which would mean the plan forbids its own completion.

**`PHASE_PLAN.md` licenses this directly**: *the steps are a hypothesis, not a
contract* - the arbiter may *move a target found to have been measured wrong*,
recording the evidence in `PHASE_OUTCOME.md`. It is recorded in this unit's
arbiter entry.

**So: change `STEP: 1 | blocked | ...` to `STEP: 1 | partial | ...` in both
`PHASE_STATUS.md` and `PHASE_OUTCOME.md`'s header, and change nothing else about
step 1.** Do not edit unit 261's entry or any other entry - a unit rewriting an
entry that is not its own is worse than a record that disagrees with itself in
public, which is the rule unit 258 set and it stands. Note the change in section
3.4.

---

## Status cadence

**Write `PROJECT_STATUS.md` at least every eight minutes and always immediately
after a commit.** The watchdog fires at twelve minutes with no status write and it
has already killed one unit of this phase with its work uncommitted.

Update `TASK: n of 5`, `NUMBER:`, `NOTE:` and `UPDATED:` as you go. **The status
write is part of the work, not part of the reporting.** Commit and push each task
before starting the next, so that a kill costs one task and not a night.

---

## Tasks

**Five tasks. Task 1 is a trace and it comes first, because this unit must
measure what this machine's endpoints actually declare before it decides what the
application should compose at.**

### Task 1 - the trace: where the two rates meet, and what happens there

**Write `docs/unit262-rate-trace.md`. Every answer carries a file and a line
number, or a measured figure. Build nothing in this task.** Commit it before task
2 touches anything, together with the step 1 header correction above.

Answer these seven questions:

1. **What rate does the application compose at today, and where is that decided?**
   Follow it from the click to `Ft8Transmission.SampleRate`. Name every line the
   value passes through.
2. **What rate does the real sink demand, and where does it learn it?** Quote the
   comparison and the exception message verbatim.
3. **Where do the two meet on the live path?** Name the exact call and quote what
   the operator would see.
4. **Where is the radio keyed relative to that throw?** State it as an ordering of
   line numbers, and say what the wire shows and what `TransmitRun` reports.
   **This is the question that decides whether the fix belongs at the click or at
   the connect.**
5. **What does `WasapiTransmitSink.Endpoints()` publish on this machine?** Run it.
   For every active render endpoint: id, friendly name, declared rate, channels,
   bits. **If it returns nothing, say so plainly and continue** - that is a fact
   about this machine, not a failure, and unit 256 already recorded that a machine
   with no default render endpoint is a normal machine.
6. **Does `Ft8Composer.RateIsUsable` accept every rate those endpoints declare?**
   Test each one. Name any it refuses and quote the refusal. Also state whether
   `BaseFrequencyIsUsable` holds at each of those rates with the default base
   frequency.
7. **Why did no existing test catch this?** Name the fake, the line, and the one
   behaviour it does not reproduce.

**Do not infer question 5 or 6. Measure them.** If the shell refuses to run the
enumeration, say which spelling was refused and fall back to a filtered test that
prints the same table.

### Task 2 - watch it fail on the application's own path

**Make the fake faithful, then watch the red.**

Give the transmit fake the one behaviour it lacks: an optional declared endpoint
rate which, when set, makes `PlayAsync` throw exactly as `WasapiTransmitSink.cs:301`
throws, with the same shape of message. **Default it to accepting anything, so no
existing test changes meaning.**

Then a new test - `TheSendPathComposesAtTheEndpointsRateTests` in
`tests/Hamlet.App.Tests/` - that drives the **application's own** send path
(`SendMessage`, through `BuildTheArmedSend`, through the substitutable
`TransmitSinkFactory` at `MainWindowViewModel.cs:7944`) with a sink declaring
**48000 Hz**, and asserts a transmission goes out whole.

**Watch it go red and quote the output as output, not as description.** The red
must show three things: that the radio was keyed, that nothing was played, and
what the operator was told. **A red that only says "test failed" is not evidence
and will not do.**

**No device is opened in this task and no port. The fake and the substituted
factory are the whole apparatus.**

### Task 3 - the caller composes at the rate the endpoint declares

**Two changes and no more.**

**a. The interface publishes the rate.** `ITransmitAudioSink` gains a member
carrying the rate the sink will accept. `WasapiTransmitSink` already has the
value at `:194`; the fake returns whatever it was told. **Amend the interface's
closing `<para>` in the same edit** - it currently instructs a caller to do
something no caller could do, and leaving prose that a neighbour falsifies is the
mistake unit 256 named and fixed rather than left.

**b. The application asks, and refuses early where the answer is unusable.**
`BuildTheArmedSend` at `MainWindowViewModel.cs:7970` already refuses in words for
a missing port and for a missing endpoint, and already catches the sink
constructor's throw at `:8004-8012`. **Put the rate decision in the same place,
in the same shape.** Read the rate off the sink it has just built, keep it, and
have `SendMessage` at `:8120` compose at it.

**Where the endpoint declares a rate `Ft8Composer.RateIsUsable` refuses, set
`_transmitRefusal` and build no armed send at all** - so the click is answered
with words and the radio is never keyed. **The refusal must name the endpoint, the
rate it declared, and why FT8 cannot be built at it**, in the register the
neighbouring refusals use. A refusal that says only *cannot transmit* is not
acceptable here; the operator has to know to go and change his sound device.

**Then watch task 2 go green**, and re-run
`OneClickSendsExactlyOneMessageTests` filtered by name to confirm the arming
guards survive. **State both counts.**

**What must not change:** `Ft8Waveform`'s default, any line of `Ft8Sharp`, the
sink's refusal to resample, the keying order in `Ft8TransmitSequence`, and the
number of `.Arm(` call sites in `src/`. **Take the arming grep before task 3 and
again after it, and put both in section 3.1.** One click, one transmission is the
fault this phase cannot recover from, and a unit that touched the send path owes
that evidence first whether or not it expected to move it.

### Task 4 - the oracle, at the rate the send path will actually use

**A signal composed at 48000 Hz is only useful if it is still FT8.**

For **each rate this machine's endpoints declare** (from task 1 question 5), and
for 48000 Hz whether or not this machine has such an endpoint: compose a set of
messages and decode them back through Hamlet's own decoder, and assert the text
that comes back is the text that went in. **Reuse
`HamletsOwnDecoderReadsBackWhatHamletComposedTests`' existing construction** -
step 2's criterion 3 says *reuse it rather than writing a second encoder*, and
that applies to the round trip too.

**A dozen messages is enough and a hundred is not wanted here.** Step 2 already
proved the corpus at its own rate; what is unproved is the rate, so vary the rate
and hold the corpus small. Include one compound callsign, one grid, one report and
one `RR73`.

**State the sample count and the duration at each rate**, and confirm the
duration is 12.64 s at every one of them. A rate that changes the duration is a
defect and must be reported, not rounded.

### Task 5 - the loopback through the application's own path. NAMED DROP CANDIDATE.

**Drop this one if the night is short. Drop nothing else.**

Unit 256 proved the loopback - compose, play, capture, resample, decode - but it
composed at the endpoint's rate itself, inside its own test. **What has never been
proved is the same loopback driven by the application's send path**, which is
where the defect lives.

If task 1 question 5 found a usable render endpoint: run one transmission end to
end through `SendMessage` and the real sink, capture it, and decode it back.
**One message, not three.** If it found none, say so and stop - the wiring is
already proved by tasks 2 and 3 against the faithful fake, and this task is the
device-route confirmation of it, not its foundation.

**If you drop this task, say in section 2 exactly what is therefore unproved**, in
the operator's terms and not in the code's. That sentence is the reason the drop
candidate is named in advance rather than chosen at midnight.

---

## Parked

**Do not touch. Do not raise. Do not propose.**

- **The abort playing out the rest of the slot.** Unit 261 measured that a stop at
  second 3 takes PTT off within two frames and the sink plays out the remaining
  ~9.6 s into an unkeyed radio. **Nothing goes on the air.** The repair needs a
  cancellation source threaded from `Ft8ArmedSend` into `RunAsync`, and
  `CancellationTokenSource.Cancel()` runs its registrations synchronously on the
  calling thread - which would be the operator's UI thread at the one moment it
  must not block. **That is the property the abort may not have**, it deserves its
  own measured unit, and it is logged here rather than chased. **You may not
  thread a token tonight.**
- **`TheDigitalTabIsTwoColumnsTests` and HM-DEC-087.** Its assertion that the FT8
  Send area contains no `Button` went red at unit 260 and unit 261 made it
  unambiguous. **The owner has been asked whether it may be narrowed and has not
  answered.** A decision is not a session's to overturn. **Leave the test, leave
  the assertion, leave HM-DEC-087, and do not add a third button.**
- CW send, `AutoCaller`, `ScanViewModel`, `CivWrites.TuneNow` and the antenna
  tuner route. Automatic sequencing. Logging and FG-004. FT4, PSK31, WSPR.
- The level the IC-7300's USB modulation input expects. **FACT-004. It is Tim's to
  read off the radio and it is deferred, not open.**
- Everything in `PHASE_PLAN.md`'s *What is not in this phase*.

---

## What not to do

1. **Do not make the sink resample.** It refuses on purpose - unit 256's reasoning
   is that shared-mode WASAPI resamples anything handed to it silently, so a sink
   that accepted 12000 Hz would report *asked 12000 got 12000* while something
   nobody chose decided what the samples became. **The caller moves, not the
   sink.**
2. **Do not change `Ft8Waveform.DefaultSampleRate`.** It is `Ft8Sharp`, it is the
   rate the decoder works at, and the receive path depends on it. The send path
   asks for a different rate; it does not redefine the default.
3. **Do not key anything, and do not open a serial port.** FACT-004.
4. **Do not let the fix arrive after the keying frame.** If the rate cannot work,
   the refusal happens where the sink is built, not where the radio is keyed. Task
   1 question 4 is what tells you the ordering; if it says something other than
   this instruction expects, **the tree wins and you say so.**
5. **Do not add a route to a transmission.** `.Arm(` has exactly one call site in
   `src/` and `TheStopAddedNoNewRouteToATransmission` is a standing guard. Keep
   both greps.
6. **Do not repair the parked items**, however small they look at the end of the
   night. Item 1 in particular is a trap: it is three lines to write and it is the
   one change that could take the no-await property off the abort.
7. **Do not run a test suite.** Rule 1. Only the tests you construct plus the two
   named for re-running, filtered by exact name, foregrounded, with a stated
   timeout.
8. **Do not chase the inherited reds** and do not chase the red named in *Verify
   this instruction against the tree*.

---

## Committing and pushing

**Commit and push after every task, before the next one starts.** Unit 257 lost
three files to the watchdog with nothing committed; units 258 through 261 lost
nothing.

One patch bump of the root version per task, continuing from **`1.12.114`**, which
is what `Directory.Build.props:205` held at `HEAD a81d214`. **If the tree says
otherwise, continue from the tree.**

Conventional commit subjects in the register the recent history uses - what the
operator gets, not what the file is called.

**Push. A commit that is not pushed is not evidence.**

---

## Reporting

Write `output.md`. **`validate-output.bat` will very likely be refused - try it
once, quote the refusal, then check the report by hand against the seven rules the
script itself prints**, as unit 261 did.

### The ordering block comes first, before the header

**The file must open with the literal words `READ IN THIS ORDER`**, then three
paragraphs beginning `A.`, `B.` and `C.` at the start of a line, all within the
first 60 lines. **A report without this block is rejected by the validator.**

- **A - the phase goal and every step's state.** *Hamlet works stations on the
  air.* Step 0 `done`; step 1 `partial` after tonight's header correction, and say
  that it was `blocked` and why it is not; steps 2 to 5 `partial`; step 6 not
  started. **Then answer plainly: on how many of this machine's render endpoints
  could the application's send path have transmitted before tonight, and on how
  many can it now?** Both numbers, out of the total task 1 found.
- **B - this unit's step and its criterion.** This unit aims at **step 3**, and at
  criterion 1 in `PHASE_PLAN.md`'s own words: *"Audio plays to the radio's USB
  input at the right device, rate and level."* B must say which of those three
  words this unit moved, which was already met and by whom, and which remains
  deferred to Tim under FACT-004 - **and it must not claim the deferred one.**
  Then state whether a transmission composed at the endpoint's rate still decodes
  back through Hamlet's own decoder, with the figure from task 4.
- **C - this report's own findings weighed against A and B.** **State the literal
  phrase `raises N items` with a real number** for section 4. For each, say
  whether it is in the way of anything named in B. **If nothing is blocking, say
  so in one sentence** - `CLAUDE_CODE.md` §8 makes an empty section 4 a real
  answer, and a note recorded for the record is not a ruling request.

### Then the six-line header

```
UNIT: 262 - the rate the endpoint actually speaks
PHASE GOAL: Hamlet works stations on the air
UNIT GOAL: <as stated above>
ADVANCED: <yes or no, and which criterion - and if a task was dropped, what that
           leaves unproved>
NUMBER: <endpoints this machine declares on which the application's send path can
         transmit - before -> after, out of the total>
DRIFT: <count. It was 1 after unit 261. If this unit advances a criterion it goes
        to 0; if it does not, it goes to 2, and say so plainly either way>
```

### What section 3 must lead with

**Section 3.1 is the arming grep, before and after task 3, side by side.** Not
because it is the largest piece of work but because *one click, one transmission*
is the fault this phase cannot recover from, and this unit edited `SendMessage`.
The three counts unit 261 established - `_armedSend.Arm(` call sites,
`new Ft8ArmedSend`, `new Ft8TransmitSequence` - each at 1, and the arming line
still inside `SendMessage`'s body.

**Then section 3.2: the red, and then the green.** The task 2 red quoted as
output, showing the radio keyed, nothing played, and what the operator was told.
Then the same test green, with the rate asked and the rate got beside each other.

**Then section 3.3: the endpoint table from task 1 question 5**, every active
render endpoint on this machine with its declared rate, and beside each one
whether `Ft8Composer.RateIsUsable` accepts it and what the operator would now see
if he chose it. **Including the refusal wording for any endpoint that cannot
carry FT8** - that sentence is what a stranded operator reads at two in the
morning, so quote it exactly.

**Then section 3.4: mismatches** between this instruction and the tree, reported
and not repaired, and the step 1 header correction recorded as done.

**Four `##` headings, spelled and ordered as `CLAUDE_CODE.md` §4 requires, with
section 4 present and section 3 not empty.**

---

```
ARBITER-DECISION
STEP: 3
APPROACH: compose the transmission at the sample rate the transmit endpoint declares instead of the 12000 Hz default, so the real sink stops refusing every send
MOVE: continue
WHY: Step 3 criterion 1 names the right device, the right rate and the right level; the device half was cut down and met by unit 256 and the level half is deferred to Tim under FACT-004, but the rate half has never been measured on the application own send path, and I measured it wrong there - MainWindowViewModel.cs:8120 composes at Ft8Waveform default 12000 Hz, WasapiTransmitSink.cs:301 throws on any rate that is not the endpoint mix format, and Ft8TransmitSequence.cs:283 keys the radio before line 287 reaches the sink. So on every real click the radio keys, the sink throws, the finally unkeys and nothing goes out. The loop test returned NOT FOUND against all eighteen entries, and I judge it not a loop: unit 256 is the nearest neighbour and it composed at the endpoint rate inside its own loopback test, never touching the caller, which is the complement of this and not a repeat.
STATE: partial
DECIDED: Three on my own authority. First, step 1 is moved from blocked back to partial and the unit is told to correct both header lines: the judging session read criterion 5, no transmitting code exists yet when this step closes, as a permanent invariant, and read that way it makes step 1 unclosable unless steps 3 and 5 are deleted - the criterion says yet, it is a sequencing gate, it was measured met at unit 253 close by a grep that returned only its own declaration, and it was overtaken by the plan own ordering. Second, the fix is placed at BuildTheArmedSend rather than at the click, because the keying write at line 283 precedes the sink call at 287, so a rate that cannot work must be refused where the sink is constructed or the radio keys before anybody finds out. Third, the transmit fake is given the one behaviour it lacks - a declared rate it can refuse on - because a fake more permissive than the thing it stands for is why sixteen units of green tests sat over a path that cannot transmit, and that is the named breakage which licenses the new test.
LICENCE: PHASE_PLAN.md step 3 criterion 1 in its own words - audio plays at the right device, rate and level - together with the named alternatives to stopping: the tree wins, report the mismatch and continue, and a target not reached is closed with the figure reached and what was tried. The steps are a hypothesis not a contract licenses both taking a partial step again on unattempted ground and moving step 1 target found to have been measured wrong, recording the evidence. SHACK_FACTS.md FACT-004 licenses enumerating and playing into this machine own endpoints while forbidding any inference about the radio, and keeps the level half deferred. Step 2 criterion 3, reuse it rather than writing a second encoder, licenses task 4 reuse of the existing round trip.
ACCOMPLISHED: When Tim clicks a station tonight, the sound Hamlet builds is at the rate his radio sound card actually speaks, so it reaches the radio instead of being refused at the last inch after the transmitter has already been keyed. And where he picks a sound device that cannot carry FT8 at all, Hamlet tells him so when he picks it, by name, instead of keying his radio and then saying nothing went out.
ADVANCES: Step 3, criterion 1 - "Audio plays to the radio's USB input at the right device, rate and level" - the rate half, which no unit has measured on the application's own send path and which is wrong there today. The device half stands met from unit 256 and the level half stays deferred to Tim under FACT-004 and is not claimed.
END-ARBITER-DECISION
```

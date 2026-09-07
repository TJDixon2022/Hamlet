# Work instruction 257 - the row knows where the contact stands

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

**All four were checked against the tree at 2026-09-06, at `HEAD 875f176`, while
this instruction was written.** `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` are present; neither
`CoreHMI.sln` nor `MURC.sln` exists; the only solution at the root is
`Hamlet.sln`. Check them anyway.

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

**Unit 256's tests ran in real time and yours do not.** That unit played 12.64
seconds of audio out of a sound card per message and had to be capped for it.
**This unit opens no audio device and plays no sound.** Everything here is
samples in memory and text in files. The only thing in it that costs real
seconds is the decoder, and task 1 measures that before task 2 spends it.

---

## THE TOOL RULE

**This session's shell may refuse calls.** A refused shell call is a signal to
reach for the other tool, not to stop. **The file-editing tools have been
unaffected throughout.** Record every refusal verbatim. **Nothing in this unit
halts the loop.**

**Three tool facts earlier units paid for:**

1. **`tools\arbiter\outcome-append.bat` has now been refused for five
   consecutive units** - 253 twice, 254 once, 255 twice, 256 twice. **Expect
   it.** Task 5 tells you what to do instead, and units 253 to 256 all did it
   the same way: append the entry with the file-editing tools, in the exact
   twelve-field format the existing entries use, ASCII only.
2. **`tools\arbiter\validate-output.bat` was refused in seven forms** by unit
   255 and again by unit 256, including with the sandbox override. **This shell
   will not start a batch file.** Try it once; if it is refused, check the
   report by hand against the rules in the reporting section below, say in the
   report that you did, and **quote no exit code**, because there is none.
3. **The shell has refused device enumeration and `powershell -NoProfile`
   commands.** You need neither. If you find yourself wanting one, the
   file-editing tools and a filtered xunit test have covered every case so far.

**`./tools/arbiter/outcome-read.bat` did run for the arbiter this session**, with
forward slashes and a leading `./` and nothing in front of it. That is a fact
about one command, not a promise about the rest.

---

## Why this unit exists

**The count today.** Step 0 `done`, one unit. Step 1 `partial` at four of five
and closed. Step 2 `partial` with criterion 4 deferred to Tim. **Step 3 has had
four units spent on it and is closed.** **Steps 4, 5 and 6 have had no unit spent
on them at all.**

Hamlet can now compose a message, key a radio, play the audio out of this
computer and read it back through its own decoder as the same message - 3 of 3,
whole text, on a real device route. The transmit half of the phase exists as far
as a machine with no radio can take it.

**And Hamlet still cannot tell you where a contact stands.** The Digital tab
shows a table of messages: time, SNR, dt, Hz, text, newest first, trimmed at a
cap. Every row is an event. **Nothing in this repository holds two callsigns
together and says what has passed between them.** `DigitalDecodeRow` splits a
message into addressee, sender and payload for a tooltip and then throws the
relation away.

**That is what step 4 is, and it is the thing step 5 cannot be built without.**
Step 5 highlights the expected next message and shows a repeat's count -
*grid, 2nd time*. Neither of those is reachable until something knows what
already passed. Step 5 depends on steps 3 and 4; step 3 is closed; **step 4 is
the only thing standing between this phase and its last buildable step.**

**The heavy hand on this unit is the plan's own ruling that Hamlet reports and
does not rule.** A contact is never closed by the app. Nothing is forbidden,
hidden or greyed out. `73` is politeness. A station working three others at once
is a station being busy, not a fault. **Every one of those is a rule about what
this unit must refuse to build**, and it is easier to write the type that decides
than the type that only counts. Write the one that only counts.

```
PHASE GOAL:   Hamlet works stations on the air.
UNIT GOAL:    For each station heard, Hamlet holds which messages passed each
              way, when, and how many slots ago - and from that shows one of
              four states, waiting on him, your move, complete or gone quiet,
              with slot counts. It closes nothing, hides nothing, forbids
              nothing and interprets nothing. Proved against a recorded corpus
              of consecutive slots, on this machine, with no radio.
ADVANCES:     step 4 - exit criteria 1 (which messages passed each way, when,
              and how many slots ago), 2 (the four states with slot counts),
              3 (complete without 73), 4 (nothing closed, hidden or forbidden),
              5 (a station working three others reads as gaps) and 6 (derived
              from recorded captures, not from the air).
```

---

## Verify this instruction against the tree

**Everything below was read from the tree at 2026-09-06 by a session that could
not run the application.** Where this instruction and the tree disagree, **the
tree wins** - `PHASE_PLAN.md` says so in its own table. **Report the mismatch in
section 3 and continue. Do not repair the instruction and do not stop.**

Unit 256 found one of these wrong and reported it at no cost, which is exactly
right: work instruction 256 said `Ft8Composer` constructs and calls
`Ft8SlotDecoder`, and it does not - `Ft8Composer.cs:585` calls
`Ft8MessageDecoder.Decode`, the message layer, which never sees a sample. **The
correction is carried here so it is not made twice.**

**What I measured, with where I read it:**

| Claim | Where |
|---|---|
| One decoded message is `Ft8Decode(SlotStartUtc, OffsetSeconds, FrequencyHz, SyncScore, Message)` with `SignalToNoiseDb` added beside it. The relation between two stations exists **only inside `Message`**. | `src/Hamlet.RadioEngine/Audio/Ft8Reception.cs` |
| `Ft8Vocabulary.Split(string?)` returns `Ft8MessageFields(To, From, Payload)`, handles `CQ DX W1ABC FN42` as a four-word form, and returns **null** for anything that is not plainly three fields. | `src/Hamlet.App/ViewModels/Ft8Vocabulary.cs:48`, the record at `:294` |
| That splitter's own remarks say the engine could hand the fields over and does not, *because unit 241 may not change the engine. It is reported rather than worked around quietly.* | same file, the remarks above `:48` |
| The row is `DigitalDecodeRow`, a record; `Fields => Ft8Vocabulary.Split(Message)` at `:112`; `From(Ft8Decode)` at `:199`; **nothing on the row changes after it arrives** and `INotifyPropertyChanged` was deliberately removed in unit 252. | `src/Hamlet.App/ViewModels/DigitalDecodeRow.cs:67`, `:112`, `:199` |
| Rows land through `PlaceRow(DigitalDecodeRow.From(decode))`; the table is `DigitalDecodes` with a filtered `DigitalVisibleDecodes` beside it; **the oldest arrival is trimmed at `MaxDigitalDecodes`**; `AddDecodeRowForTests` exists for tests that open no window. | `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:1056`, `:1076`, `:7717`, `:7785` |
| Slot arithmetic already exists: `SlotSeconds = 15`, `SlotStart`, `IntoSlot`, `BoundariesBetween`. | `src/Hamlet.RadioEngine/Audio/Ft8Slots.cs:119`, `:126`, `:182`, `:209` |
| The operator's own callsign lives in the **app**, defaulting to `KC3QIS`. | `src/Hamlet.App/Settings/OperatorProfile.cs:44` |
| `ContactStage` is an existing five-stage contact model - `Calling`, `Answering`, `Exchanging`, `Confirming`, `SigningOff` - which offers **the one thing anybody would say next** under HM-DEC-059. It is on the CW send path. | `src/Hamlet.RadioEngine/Cw/ContactStage.cs:10` |
| `Ft8Composer.Compose` takes a base frequency, `BaseFrequencyIsUsable` validates one, and `DefaultBaseFrequencyHz` is the port's. **Several stations can be put in one slot at different frequencies.** | `src/Hamlet.RadioEngine/Transmit/Ft8Composer.cs:204`, `:224`, `:259`, `:361` |
| The slot decoder is `Ft8SlotDecoder`; the construction to reuse is in the round-trip test, not in `Ft8Composer`. | `src/Ft8Sharp/Dsp/Ft8SlotDecoder.cs:51`; `tests/Hamlet.RadioEngine.Tests/Transmit/HamletsOwnDecoderReadsBackWhatHamletComposedTests.cs` |
| **`tests/fixtures/ft8/captured/` holds a README and no captures.** Its README says zero real fixtures is the correct state on this machine, and that **a fixture naming a capture which is not there is a hard failure**. The only FT8 `.wav` in the tree is `tests/fixtures/ft8/example/ft8-example-244.wav`, which the same README says is **refused for scoring**. | `tests/fixtures/ft8/captured/README.md`; `docs/ft8-capture-fixture-format.md` |
| One slot's decode is already measured against a **fifteen-second hard budget** and passes it. | `tests/Hamlet.RadioEngine.Tests/Audio/WhatOneSlotCostsTests.cs:37` |
| The root version reads `1.12.94`. | `Directory.Build.props:205` |
| `docs/unit256-*.md` and `docs/unit257-combining-placement.md` **already exist from the previous phase's unit numbering.** Your documents take distinct stems - `docs/unit257-contact-state-*.md`. **Do not overwrite an archived document.** | `docs/` |

**Expected failures: none of these.** The build should be green before you start.
If it is not, say so in section 3 and work in the smallest area that compiles.
**`tools/unit254-seam-grep.sh` is still there, untracked, from unit 254. Leave
it. No third deletion attempt.**

---

## What the reload measured, and what to do about it

**Four disagreements, none of them yours to repair.**

1. **`RULES_AT` disagrees with `CLAUDE.md` §1** - `PROJECT_STATUS.md` says
   `HM-DEC-157 (2026-09-06)`, the reload reads §1's highest as `CPS-DEC-0152`.
   **Unit 255 already answered this and unit 256 confirmed it:** §1's table is
   `HM-DEC-` throughout, `CPS-DEC-` appears nowhere in this repository, and
   `HM-DEC-155/156/157` are in `DECISIONS.md` but not yet indexed into §1's
   table. **`RULES_AT` is ahead of the index, not ahead of the record. Change
   nothing. Do not re-derive it a third time.**
2. **`PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` are modified and
   uncommitted at the root**, along with the `.run-unit/` records. **Commit them
   at the start of task 1**, the way unit 256 committed unit 255's leftovers.
   They are the loop's own records and they belong in the tree.
3. **`PHASE_STATUS.md`'s `CURRENT_STEP:` reads 1** and is stale. **It is the
   launcher's field and unit 256 was right not to hand-write it.** Do not write
   `CURRENT_STEP:` or the `STEP:` lines in `PHASE_STATUS.md`. `WORK_INSTRUCTION:`
   is yours to set. `PHASE_OUTCOME.md`'s header **is** yours - task 5.
4. **Step 3's state is recorded twice and the two disagree.** The unit's own
   appended entry says `done`; the judging session that read the report against
   the criteria returned `partial`, on the grounds that criterion 1 - audio to
   the radio's USB input at the right level - is met only as a declared cut-down
   on a development machine. **Both are on the record and neither is yours to
   reconcile.** Step 3 is closed either way and this unit does not touch it.

---

## Steps 1, 2 and 3 are closed and are not yours

**Do not reopen them, do not improve them, do not add a test to them.**

- **Step 1**, the abort: closed `partial` at four of five. Its fifth criterion -
  *no transmitting code exists yet when this step closes* - was ruled unmeetable
  in its letter, because `Ic7300Rig.SendCwAsync` and `CivWrites.TuneNow` pre-date
  the phase and sit on parked surfaces. **Not to be reopened.**
- **Step 2**, the waveform: closed `partial`. 112 of 117 messages read back, the
  5 named and their condition proved. Criterion 4 is deferred to Tim: **the level
  the IC-7300's USB modulation input expects is not in this repository and
  `SHACK_FACTS.md` FACT-004 forbids inferring it here.**
- **Step 3**, the audio path: closed. The loopback decoded 3 of 3 on the device
  route. What remains is the drive level Tim reads off the radio by watching ALC.

**You need one thing from all of that and only one:** `Ft8Composer.Compose` turns
a message into a slot of samples, and `Ft8SlotDecoder` reads samples back into
messages. **Call them. Do not change them.**

---

## Rulings in force

**Transcribed from `PHASE_PLAN.md`. Not to be re-argued by any unit, including
this one.**

**The dummy load is withdrawn.** HM-DEC-008 and HM-DEC-098 superseded,
2026-09-06. **Do not reference it, do not propose it, do not treat its absence as
a risk.**

**One click, one message.** Ruled 2026-09-06. Hamlet transmits because the
operator clicked. **Never on a timer, never on a decode, never to continue a
contact.**

**Right-click sends immediately, in the next slot, with no confirmation.** Ruled
2026-09-06. *That is step 5. This unit builds none of it.*

**Nothing is forbidden in the menu.** The expected next message is highlighted;
everything valid stays clickable; **a repeat is correct behaviour and shows its
count.** FT8 loses transmissions constantly, so sending the grid a second time is
correct behaviour, not a mistake to be greyed out.

**A contact is never closed by the app.** Complete is shown when the exchange has
what a QSO needs. **`73` is politeness, not a requirement.** Nobody is obliged to
send it, an operator may be working three stations at once, and Hamlet is not the
radio police. **A row shows *waiting on him*, *your move*, *complete* or *gone
quiet*, with slot counts. It reports; it does not rule.**

**`Ft8Sharp` is a faithful MIT port and nothing in this phase changes a line of
it.** `Ft8Sharp.Deep` is GPL-3.0.

**The engine is not told that tabs exist** (§0.1).

**Nothing interprets a message** (§12.1). **A row's state is derived from which
messages passed between two callsigns, which is bookkeeping, not meaning.** That
sentence is your licence for everything in this unit and it is also your limit:
you may count what passed, you may not say what anybody meant by it.

**The three the arbiter may not reason past, carried in full:** the abort on
every keying path; one click, one transmission; licence privileges never
exceeded. **This unit keys nothing, so all three are satisfied by building
nothing that transmits** - and task 5 proves it with a grep rather than asserting
it in prose.

**A unit may not add a test without naming the breakage it would have caught.**

**Known reds, inherited, never chased:**
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list
tripwire.

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` **read from the clock,
never composed**, and `NOTE` saying what is moving inside the task. The same
every ten minutes while a task is running. **Use the file-editing tools if the
shell refuses.**

**And once before and once after task 2's corpus generation**, which is the one
command in this unit that may run for minutes without printing anything.

**Unit 253 wrote three `UPDATED` values it composed, one of them 39 minutes ahead
of the real clock.** Read the clock.

---

## Tasks

Five tasks. **Task 1 is measuring and reading only. Task 3 is the goal task.**
Task 5 carries the named drop candidate.

### Task 1 - what the tree already knows about a station. THE TRACE.

**Measuring and reading only. Change nothing under `src/` in this task.** Commit
the uncommitted root records first, as named above.

**This task exists because the last four units each found something the
instruction had assumed.** The seam's route order, the hashed callsign's
brackets, the padding that centres a transmission, the five keying routes, the
fresh `AudioClient` on every property read. **Every one was found by measuring
before building. Do that again.**

Write `docs/unit257-contact-state-survey.md` answering, each with a file and line:

1. **What a decoded slot actually hands over.** Confirm or correct my reading of
   `Ft8Decode`. **Is the message text the only place the relation between two
   stations lives?** If something else already carries it, say so and use it.
2. **The splitter.** What `Ft8Vocabulary.Split` accepts and what it refuses.
   **Then decide where the split lives for an engine-side ledger and say why.**
   There must be **one splitter in this repository, not two**. If you lift it
   into the engine, the app's `Split` delegates to it and every existing test of
   it keeps passing; if you leave it where it is, say how the engine gets fields
   without knowing the app exists. **Either answer is acceptable. Two copies of
   the parsing rules is not.**
3. **Slot arithmetic.** `Ft8Slots` already knows a slot is 15 seconds and where
   one starts. **Reuse it. Do not write a second one**, and say in the survey
   which call you will use for *how many slots ago*.
4. **What already tracks a station across time.** A line each, saying reuse or
   leave and why, for at least: `Explore/HeardWatch.cs`, `Explore/RecentStation.cs`,
   `Cw/ContactStage.cs`, `Cw/AutoCall.cs`. **`ContactStage` is the trap on this
   list.** It is the right shape and the wrong ruling: it exists to offer *the
   one thing anybody would say next*, and this phase forbids nothing and offers
   everything. **Say that in the survey so the next reader does not reach for
   it**, and touch neither it nor `AutoCall`.
5. **What our own sent messages are, to the ledger.** Unit 255's `TransmitRecord`
   carries eleven fields and **deliberately has no string parameter at all**, so
   a callsign cannot reach it and the ledger cannot learn what we sent from
   telemetry. **Say what the ledger takes instead** - it will be an explicit call
   made by whatever sends, which in this repository is nothing yet. Design it so
   step 5 can call it in one line.
6. **What FT8 material exists on this machine.** Verify my reading:
   `tests/fixtures/ft8/captured/` holds a README and no captures, and the worked
   example is refused for scoring. **If you find a real capture I did not, prefer
   it to anything synthesized and say so.**
7. **What one slot's decode costs.** Read it off `WhatOneSlotCostsTests` or
   `HowFastTheDecoderEatsAudioTests` if either already states a figure; measure
   it once, filtered and foregrounded, only if neither does.

**End the survey with one go/no-go line, exactly one of:**

```
CORPUS ROUTE: audio
CORPUS ROUTE: scripted decodes
```

**Take the audio route unless the measurement says you cannot.** The corpus is
better evidence when the decodes came out of Hamlet's own decoder rather than off
a keyboard - that is what criterion 6 is asking for. **Take the scripted route
only if one slot's decode times out of your budget**, and if you take it, put the
figure that decided it in the survey and in the report.

### Task 2 - a band scene, recorded in slots

**The corpus step 4's criteria 5 and 6 both name.** It goes in a **new** folder,
`tests/fixtures/ft8/scenes/`, with a README of its own.

**Do not put anything in `tests/fixtures/ft8/captured/`.** That folder is for
WSJT-X pairs, its README says a fixture naming an absent capture is a hard
failure, and only `provenance: wsjtx` may be scored against. **Your scene is not
a capture and must never be mistaken for one.** Its README says, in its own
words: what made it, that it is synthesized by Hamlet's own encoder, that it is a
scene for bookkeeping and **may never be scored against the decoder's accuracy**,
and what Tim would run at the shack to make a real one.

**The scene, in consecutive numbered slots**, containing all of:

- **A station calling CQ that the operator answers, run to a complete exchange
  with no `73` anywhere in it.** This is criterion 3's evidence.
- **One station working three others at once** - its messages to us interleaved
  with its messages to two other callsigns across the same slots, so that from
  our side its replies to us have gaps in them. **This is criterion 5's evidence
  and it is the one case the scene exists for.**
- **A station that answers once and is never heard again**, with five or more
  silent slots after it.
- **A station whose exchange is already complete and who then sends `73`
  anyway**, so the report can show that complete did not need it and that its
  arrival changed nothing.
- **At least one message the splitter refuses** - free text or a non-standard
  form - so that the ledger is watched not crashing on what it cannot split.

**Caps, and they are must-pass, not suggestions:** **no more than 12 slots**, no
more than **8 signals in any one slot**. On the audio route, compose each
station at its own base frequency, sum them, scale so nothing clips, and decode
each slot **once**. **What comes out of the decoder is the corpus - not what you
put in.** Count and record any message that did not decode. **If the
three-at-once station is one of the losses, regenerate that slot with more
frequency separation. Do not hand-write the missing line into the corpus.**

Write the corpus as a text file the test reads - one line per decode, slot,
frequency, message - and commit it. **No `.wav` artefact.** Units 254 and 256
both dropped one for the same reason: a committed binary is a second copy of what
a deterministic test regenerates.

### Task 3 - the ledger. THE GOAL TASK.

**A type in `src/Hamlet.RadioEngine/` that holds, per station, what passed each
way.** The folder is yours to choose from the trace; it is not `Audio/` and it is
not `Transmit/`.

It takes **the operator's own callsign as a parameter**. It never reads settings,
never names a tab, never opens anything. It is fed decodes, and separately told
what the operator sent, and it answers per station:

- **which messages passed each way, in order, each with the slot it was in;**
- **when the last one each way was, and how many slots ago;**
- and nothing else.

**Watch it fail first, and this is the breakage to name:** build it holding only
the **last** message per station, and watch the case that needs the whole
exchange go red - a station we have heard twice and answered once reads as though
the first exchange never happened, and a repeat can never be counted, which is
exactly what step 5's *grid, 2nd time* will need. **Then put the history in.**

Assert it against task 2's corpus, station by station, with the counts quoted.

**It decides nothing.** No method on it may hide a station, close a contact,
forbid a message or return a verdict about anybody's operating. If you find
yourself writing one, that is the ruling arriving in code and the answer is to
delete it.

### Task 4 - the four states, and what complete means

**Derive the four from the ledger, with slot counts:** *waiting on him*, *your
move*, *complete*, *gone quiet*. The words are `PHASE_PLAN.md`'s and are not to
be renamed. **A fifth state is not permitted without a sentence in the report
saying what it is and why the four could not carry it.**

**Assertions, all must-pass:**

1. **Complete on an exchange with no `73` in it** - both calls, both grids or
   reports, both acknowledgements. **The absence of `73` never withholds
   complete.**
2. **`73` arriving after complete changes nothing** about the state or about what
   remains available.
3. **The three-at-once station reads as gaps and is never *gone quiet* while it
   is transmitting in those slots.** Assert it against the corpus's own slots.
   *This is criterion 5 and it is the assertion that is easiest to write wrongly:
   a rule that counted silence from us rather than silence from him would pass
   every other case in the scene.*
4. **Gone quiet is a count of slots and is stated as one.** Never a verdict about
   the station, never a reason.
5. **Nothing is closed, hidden or forbidden.** A complete contact and a gone-quiet
   one are both still fully available. If the list of what may be sent is step
   5's and does not exist yet, **assert instead that this type exposes no member
   that could withhold anything**, and say so in the report in those words.

**The threshold that separates *gone quiet* from *waiting on him* is a number you
choose.** Make it a named constant with the arithmetic beside it - a slot is
15 seconds - and **report it as a choice, not as a specification.** This
repository holds no pinned document saying when an FT8 station has gone quiet,
the same way it held none for slot placement, and unit 255 handled that correctly
by recording the choice with its arithmetic.

### Task 5 - the row shows it, and the record. **Drop candidate here.**

**The state reaches the operator's row.** `PHASE_PLAN.md` criterion 2 says the
four states are *shown per row*, and `DigitalDecodeRow` already formats what a
reader sees so that a test can assert it **without opening a window**. Follow
that precedent: the state and its slot count become text on the row, asserted by
a view-model test through `AddDecodeRowForTests` or the equivalent the trace
found.

**THE NAMED DROP CANDIDATE IS THE MARKUP.** If this unit runs long, **drop the
`MainWindow.axaml` column, tooltip or template change and keep the tested string
on the row.** Say in the report that you dropped it and what remains. A row that
carries the right text with an asserting test is criterion 2 met in substance and
one afternoon from being on screen; a half-edited 3,000-line `.axaml` at the end
of a long night is a broken window.

**Then the record:**

- **`PHASE_OUTCOME.md`** - append this unit's entry. Try
  `tools\arbiter\outcome-append.bat` once; when it is refused, write the entry
  with the file-editing tools in the exact twelve-field format the existing
  entries use, same names, same order, **ASCII only**, and update the header's
  `STEP: 4` line in place. **Record the refusal verbatim in the report.**
- **The greps that prove this unit built nothing that transmits.** `new
  Ft8TransmitSequence` and `new WasapiTransmitSink` still appear only in tests;
  nothing in `src/` constructs either; nothing you added names `TransmitAbort`,
  PTT, CI-V or a rig. **Quote the greps.**
- **`PROJECT_STATUS.md`** per the cadence. **Not `CURRENT_STEP:`, not the
  `STEP:` lines.**

---

## Parked - do not touch, do not raise

**From `PHASE_PLAN.md`'s own list**, and it is not open for discussion:
automatic sequencing; logging (FG-004); FT4, PSK31 and WSPR transmit; CW send;
the OSD re-encoding count; `ReusableWindow`; `ProcessDelayForTests`; the tap's
owner; the waterfall's first row; unit 237's Extensible conclusion; work
instruction 231's four tree items; `validate-output.bat`'s permitted-spellings
bug; the 101.33 ms pulse above 6 kHz; the CW decoder and its inherited reds.

**And these four, carried from units 253 to 256:**

- **`TransmitGuard.Check` permits when the operator's toggle is off.** Banked and
  the owner's. Not re-raised, not widened, not in this unit's way.
- **Five routes in the tree reach a keying frame**, including `AutoCaller` at
  `Cw/AutoCall.cs:272`, which keys repeatedly from one operator start. **On the
  parked CW path. Logged. Do not touch it.**
- **`SetSettingAsync(CivWrites.AntennaTuner, CivWrites.TuneNow)` writes
  `1C 01 02`**, a tuning cycle that transmits, called by no line in the tree.
  **Logged. Parked.**
- **The IC-7300's USB modulation input level.** Deferred to Tim, twice over, at
  step 2's criterion 4 and step 3's criterion 1. **Not this unit's and not
  inferable from this machine** - `SHACK_FACTS.md` FACT-004.

---

## What not to do

**Citing rather than retyping where the rule is already written down.**

1. **Do not open an audio device and do not play sound.** Step 3's loopback is
   done and is not to be re-run. This unit is silent.
2. **Do not change a line of `src/Ft8Sharp/` or `src/Ft8Sharp.Deep/`.** The port
   is faithful and MIT; `PHASE_PLAN.md`'s rulings say nothing in this phase
   changes a line of it. **Call it. Read it. Leave it.**
3. **Do not change `Ft8TransmitSequence`, `TransmitAbort`, `TransmitGuard`,
   `WasapiTransmitSink` or `Ft8Composer`'s signature.** `Compose` may be
   *called* to build the corpus. It may not be edited to make that easier.
4. **Do not build a menu, a right-click, a CQ button or a Send area.** That is
   step 5 in full. Building it early is how this unit ends with a half-menu and
   no ledger.
5. **Do not make anything transmit, and do not add a caller to anything that
   can.** Nothing in `src/` constructs a transmit sequence today; **it must still
   be true when you finish**, and task 5 proves it with a grep.
6. **Do not interpret a message.** §12.1. Counting what passed between two calls
   is bookkeeping and is licensed by the plan in those words. Wording what a
   station meant, guessing at intent, or extending `Ft8Vocabulary.Explain`'s
   closed table is not - **Tim closed that table on 2026-09-04.**
7. **Do not add a fifth state, and do not rename the four.**
8. **Do not close, hide, grey out, sort away or forbid anything.** Unit 252
   already removed row dimming on Tim's ruling; do not reintroduce it in another
   shape.
9. **Do not write into `tests/fixtures/ft8/captured/`** or give anything you make
   the provenance `wsjtx`.
10. **Do not run an unfiltered `dotnet test`, and do not background a command and
    poll for it.** The two rules at the top killed three sessions.
11. **Do not chase the known reds** listed in the rulings.
12. **Do not hand-write `HEARTBEAT:`, `CURRENT_STEP:` or the `STEP:` lines in
    `PHASE_STATUS.md`.**
13. **Do not re-derive `RULES_AT`.** Units 255 and 256 both answered it. A third
    derivation is spent time.

---

## Committing and pushing

Commit and push **each task before starting the next**. Bump the root version's
patch by one per task from `1.12.94`, so `1.12.95` through `1.12.99`.
**`Ft8Sharp` does not move.**

**Commit task 2's corpus on its own, before task 3 begins.** The one thing this
unit must not lose to a watchdog is a scene that took minutes of decoding to
make.

---

## Logged, not chased

**Unit 256's section 4 raised nothing.** It said plainly that nothing is blocking
and asked the owner to decide nothing, so **it is not a ruling request.** Four
things it recorded elsewhere are carried here so they are not lost, and none of
them is a ruling request:

1. **The `Ft8Composer` / `Ft8SlotDecoder` mismatch in work instruction 256.**
   Corrected in this instruction's table above. **Logged and fixed at the
   source.**
2. **`MMDevice.AudioClient` activates a fresh, uninitialised client on every read
   of the property**, so initialising one and re-reading the property throws the
   initialised one away and fails later at `AUDCLNT_E_NOT_INITIALIZED`, nowhere
   near its cause. **Worth the phase's memory. Irrelevant to this unit, which
   opens no device.**
3. **`AudioTap.Level` is a 0.2 s moving meter**, so it reads `NearlySilent` on
   audio that decoded perfectly. `AudioTap.PeakOf` over a whole capture is the
   honest figure. **Logged. Not in your way.**
4. **The hand-checked report.** `validate-output.bat` could not be started in
   seven forms across two units. **Logged, and the tool rule tells you what to do
   about it.**

---

## Reporting

`output.md` at the repository root, overwritten, four sections per
`CLAUDE_CODE.md` §8.

**First, above everything, the ordering block. `validate-output.bat` refuses a
report without it.** It must carry the literal words `READ IN THIS ORDER`, then
three paragraphs beginning `A.`, `B.` and `C.` at the start of a line, all inside
the first 60 lines of the file, and **C must contain the literal phrase `raises N
items`** with a real number.

- **A - the phase goal and where every step stands.** Hamlet works stations on
  the air. Step 0 `done`. Step 1 `partial` at four of five and closed. Step 2
  `partial` with criterion 4 deferred to Tim. **Step 3 closed - and say that its
  two records disagree**, the unit's own entry reading `done` and the judging
  session's reading `partial` on criterion 1's radio half, neither of them
  yours. **Step 4 entering this unit at `not started` and leaving it at whatever
  you actually reached.** Steps 5 and 6 not started, **and step 5 is next and is
  unblocked only if step 4 gave it something to read.**
- **B - this step and its exit criteria, and which were met.** Step 4's six, in
  the plan's own words: which messages passed each way, when, and how many slots
  ago; the four states shown per row with slot counts; **complete means both
  calls, both grids or reports, both acknowledgements, and `73`'s absence never
  withholds it**; nothing is ever closed, hidden or forbidden; **a station
  working three others at once reads as gaps, not as a fault, proved against
  recorded slots where that happens**; derived from recorded captures, not from
  the air. **Say which of the six stand met, one line each, on quoted evidence** -
  and for criterion 6 say plainly which corpus route you took and what that makes
  the evidence worth.
- **C - what this report adds, weighed against A and B.** How many items section
  4 raises, in the words `raises N items`, and **whether any of them is in the
  way of a criterion named in B.** If none is, say so - that is a real answer and
  it is not a ruling request.

**Then the six-line header block**, from the clock, never composed:
`UNIT:`, `PHASE GOAL:`, `UNIT GOAL:`, `ADVANCED:`, `NUMBER:`, `DRIFT:`.

**`NUMBER:` for this unit is how many stations in the corpus the ledger read the
right state for, of how many stations the scene put in it** - and *the right
state* means the one the scene was built to produce, named per station before the
test ran, not after. **`DRIFT:` - work instruction 256 reported 0.**

**Section 3 leads with three things, in this order:**

1. **The station table, quoted.** Every station in the corpus, one row each:
   what passed from him, what passed from us, how many slots ago each was, the
   state, and the slot count shown with it. **The three-at-once station gets its
   own paragraph** showing the gaps in its replies to us and the sentence saying
   why that is not *gone quiet*. **The complete-without-`73` exchange is quoted
   message by message**, with the `73` that arrived later and changed nothing.
   **And the breakage you watched go red before green** - the last-message-only
   ledger, and which station read wrongly under it.
2. **The ledger, and where it lives** - its file and line, the operator's
   callsign arriving as a parameter, the splitter decision from task 1 and
   whether there is now one splitter or two, the gone-quiet threshold **with its
   arithmetic and the words saying it is a choice**, and the sentence saying what
   this type cannot do: hide, close, forbid or judge. **Plus the greps** showing
   nothing in `src/` constructs a transmit sequence or a sink, and that
   `src/Ft8Sharp/`, `Ft8TransmitSequence`, `TransmitGuard` and `TransmitAbort`
   are untouched.
3. **The corpus and what it is worth.** `CORPUS ROUTE:` as task 1 decided it and
   the figure that decided it; slots, stations, signals per slot; **how many
   messages decoded of how many composed**, and any that were lost; the wall
   clock; and **the sentence saying this scene is synthesized, is not a WSJT-X
   fixture, and may never be scored against the decoder's accuracy.** Name what
   Tim would run at the shack to make a real one. **If the drop candidate was
   dropped, say so here and say what remains.**

**Section 4 is for what is genuinely in the way.** *Nothing is blocking* is a
real answer and is written as one sentence. **A note, an observation or a
recommendation you have already acted on is not a ruling request** - put it in
section 3. **A criterion you could not fully reach is not a blockage either** -
it is a figure and what was tried, and `PHASE_PLAN.md`'s own table says to close
it that way and continue. Ask the owner to decide something only where work is
actually stopped until he does.

Write `output.md`, then stop. Do not start the next unit.

---

```
ARBITER-DECISION
STEP: 4
APPROACH: Build a per-station contact ledger in the engine from decoded slots - which messages passed each way and how many slots ago - and derive the four row states from it, proved against a recorded multi-slot band scene composed by Hamlet's own encoder and read back by its own decoder, including a station working three others at once
MOVE: continue
WHY: Step 4 has had no unit spent on it, its only entry criterion is step 0 which is done, and it is now the one thing standing between this phase and step 5 - the right-click menu cannot highlight an expected message or count a repeat until something holds what has passed between two callsigns, and nothing in this repository does. The loop test was run on this approach and returned NOT FOUND against all nine entries; step 4 has zero units spent, nothing on it has failed, and none of the tried approaches - a documentation sweep, an abort, a waveform seam, a keying sequence, a render sink - resembles receive-side bookkeeping.
STATE: not started
DECIDED: Three on my own authority. First, that criterion 6's recorded captures are satisfied by a scene composed by Hamlet's own encoder and read back through its own decoder, because I measured that tests/fixtures/ft8/captured/ holds a README and no captures and that the only FT8 wav in the tree is refused for scoring by its own README - so the choice is a synthesized scene with its provenance stated or no evidence at all, and the instruction requires the scene to be kept out of the WSJT-X fixture folder and never scored against the decoder. Second, that the ledger lives in the engine and takes the operator's callsign as a parameter rather than reading settings, so the engine is still not told that tabs exist while step 5 and any later logging can both read it. Third, that the drop candidate is the MainWindow.axaml markup rather than any part of the ledger, because a tested string on the row is criterion 2 met in substance and a half-edited three-thousand-line axaml at the end of a long night is a broken window.
LICENCE: PHASE_PLAN.md's named alternatives to stopping - the tree wins, report the mismatch and continue, and a target not reached is closed with the figure reached and what was tried - together with the plan's own ruling that a row's state is derived from which messages passed between two callsigns, which is bookkeeping, not meaning, which licenses the field split the ledger needs. SHACK_FACTS.md FACT-004 rules out the radio-side alternative to a synthesized scene.
ACCOMPLISHED: Hamlet stops showing a list of events and starts knowing where a contact stands. For each station it holds what passed each way and how long ago, and says one of four things about it - waiting on him, your move, complete, gone quiet - with the slot counts beside it. It says them about a station working three others at once without calling that a fault, it calls an exchange complete without waiting for a 73 nobody is obliged to send, and it closes, hides and forbids nothing. That is the last thing step 5's menu needs before a right-click can offer the operator anything.
ADVANCES: step 4 - exit criterion 1 (per station, which messages passed each way, when, and how many slots ago), criterion 2 (the four states shown per row with slot counts), criterion 3 (complete means both calls, both grids or reports, both acknowledgements, and 73's absence never withholds it), criterion 4 (nothing ever closed, hidden or forbidden), criterion 5 (a station working three others at once reads as gaps, proved against recorded slots) and criterion 6 (derived from recorded captures, not from the air).
END-ARBITER-DECISION
```

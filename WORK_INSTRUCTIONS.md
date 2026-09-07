# Work instruction 258 - the ledger, on the scene the last unit left

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

**All four were checked against the tree at 2026-09-06T22:25, at `HEAD 264cc8b`,
while this instruction was written.** `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` are present; neither
`CoreHMI.sln` nor `MURC.sln` exists; the only solution at the root is
`Hamlet.sln`. Check them anyway.

---

## THE THREE RULES THAT KILLED FOUR SESSIONS

**Tim's rulings of 2026-09-05, and one fact measured tonight. Not this unit's to
weigh.**

**1. A unit runs no test suite.** **A unit may run only the unit test it
constructs in that work instruction**, filtered by exact name, in the foreground,
with a stated timeout of a few minutes. **An unfiltered `dotnet test` on any
project is forbidden.**

**2. Never background a command and poll for it.** Three sessions were killed by
the watchdog on 2026-09-05, at 33 to 38 minutes, each sitting in
`until grep -q "exited with code" ...; do sleep 15; done` with a `900000` ms
timeout.

**3. THE WATCHDOG FIRES AFTER TWELVE MINUTES WITH NO STATUS WRITE, AND IT KILLED
THE LAST UNIT.** Unit 257 launched at 20:43, wrote `PROJECT_STATUS.md` once at
20:45:31, worked steadily, and was killed at 20:57 with three tasks of work
uncommitted and **no `output.md` at all**. It did nothing wrong except stay quiet
for eleven minutes while it wrote files. **Its report was never written, so the
loop recorded judgment fields against the previous unit's report instead.**

**So the status write is part of the work, not part of the reporting.** Write
`PROJECT_STATUS.md` when you finish a file, before you start a build, before you
start a filtered test run, and never let eight minutes pass without one. The
cadence section below says it again because this is the one failure mode that
loses everything this unit does.

`dotnet build` is allowed, foregrounded, with a timeout.

**This unit opens no audio device and plays no sound.** Everything in it is
samples in memory and text in files. The only thing that costs real seconds is
the decoder, and task 1 spends that once, at a known price: unit 257 measured one
slot at **582.5 ms through Deep via samples** and twelve slots at **6.99 s**.

---

## THE TOOL RULE

**This session's shell may refuse calls.** A refused shell call is a signal to
reach for the other tool, not to stop. **The file-editing tools have been
unaffected throughout.** Record every refusal verbatim. **Nothing in this unit
halts the loop.**

Three tool facts earlier units paid for, carried forward unchanged:

1. **`tools\arbiter\outcome-append.bat` has been refused for five consecutive
   units** - 253, 254, 255, 256, and unit 257's refusals are in
   `.run-unit/denials.txt` for a sixth. **Expect it.** Task 5 tells you what to
   do instead: append the entry with the file-editing tools, in the exact
   twelve-field format the existing entries use, ASCII only.
2. **`tools\arbiter\validate-output.bat` was refused in seven forms** by units
   255 and 256, and refused again for unit 257 in two forms. **This shell may not
   start a batch file.** Try it once; if it is refused, check the report by hand
   against the rules in the reporting section below, say in the report that you
   did, and **quote no exit code**, because there is none.
3. **The shell has refused device enumeration, `powershell -NoProfile`, and
   `sed -i`.** You need none of them.

---

## Why this unit exists

**The count today.** Step 0 `done`, one unit. Step 1 `partial` at four of five
and closed. Step 2 `partial` with criterion 4 deferred to Tim. Step 3 closed,
four units. **Step 4 has had one unit launched at it and that unit was killed
fourteen minutes in.** Steps 5 and 6 have had none.

**What unit 257 got into the tree before it died, and it is worth having:**

- `docs/unit257-contact-state-survey.md`, committed at `HEAD 264cc8b`. Three
  hundred and thirty-nine lines of measurement with files and line numbers.
  **Read it. Do not write it again.**
- Three uncommitted files under `tests/Hamlet.RadioEngine.Tests/Contacts/` and
  one uncommitted corpus at `tests/fixtures/ft8/scenes/`. **Never built, never
  proved, no report says a word about them.**

**What it did not get to is the whole of step 4.** There is no
`src/Hamlet.RadioEngine/Contacts/` folder. `Ft8MessageSplit` appears nowhere in
`src/`. Nothing in this repository holds two callsigns together and says what has
passed between them. The Digital tab still shows a table of events - time, SNR,
dt, Hz, text - and every row is an event that knows nothing about the row above
it.

**So the order of this unit is deliberately the opposite of the last one.** Unit
257 spent its fourteen minutes on the survey and the fixture and died before the
ledger existed. **This unit adopts the fixture in one bounded task and then
builds the ledger.** If anything in this unit is not reached, it must be the row's
markup - not the ledger, and not the four states.

**The heavy hand is the plan's own ruling that Hamlet reports and does not rule.**
A contact is never closed by the app. Nothing is forbidden, hidden or greyed out.
`73` is politeness. A station working three others at once is a station being
busy, not a fault. **Every one of those is a rule about what this unit must refuse
to build**, and it is easier to write the type that decides than the type that
only counts. Write the one that only counts.

```
PHASE GOAL:   Hamlet works stations on the air.
UNIT GOAL:    For each station heard, Hamlet holds which messages passed each
              way, when, and how many slots ago - and from that shows one of
              four states, waiting on him, your move, complete or gone quiet,
              with slot counts. It closes nothing, hides nothing, forbids
              nothing and interprets nothing. Proved against the recorded slot
              corpus already in the tree, on this machine, with no radio.
ADVANCES:     step 4 - exit criteria 1 (which messages passed each way, when,
              and how many slots ago), 2 (the four states shown per row with
              slot counts), 3 (complete without 73), 4 (nothing closed, hidden
              or forbidden), 5 (a station working three others reads as gaps)
              and 6 (derived from recorded captures, not from the air).
```

---

## Verify this instruction against the tree

**Everything below was read from the tree at 2026-09-06T22:25 by a session that
could not run the application.** Where this instruction and the tree disagree,
**the tree wins** - `PHASE_PLAN.md` says so in its own table. **Report the
mismatch in section 3 and continue. Do not repair the instruction and do not
stop.**

Unit 256 found one of these wrong and reported it at no cost, and unit 257
confirmed the correction: `Ft8Composer.cs:585` calls `Ft8MessageDecoder.Decode`,
the message layer, which never sees a sample. **The construction of a slot
decoder to reuse is in the round-trip test, not in `Ft8Composer`.**

**What I measured, with where I read it:**

| Claim | Where |
|---|---|
| **The engine has no `Contacts/` folder.** Its folders are `Audio, Bands, Civ, Cw, Explore, Licensing, Rig, Scan, Solar, Telemetry, Training, Transmit, Transport`. | `src/Hamlet.RadioEngine/` |
| **`Ft8MessageSplit` exists nowhere in `src/`.** The only occurrence of the name in the whole tree is a remark inside an uncommitted test file. | grep over `src/` and `tests/` |
| The splitter is still `Ft8Vocabulary.Split(string?)` returning `Ft8MessageFields?`, in the app, with the record declared at `:294`. | `src/Hamlet.App/ViewModels/Ft8Vocabulary.cs:48`, `:294` |
| Unit 257's survey **decided** that the splitter moves into the engine as `src/Hamlet.RadioEngine/Contacts/Ft8MessageSplit.cs` and the app's `Split` becomes a one-line forward, having counted **seventeen call sites** that all use `var` or name `Ft8MessageFields`. **The decision was made; the move was not.** | `docs/unit257-contact-state-survey.md` §2 |
| `Ft8Vocabulary.Explain` and its closed table **do not move.** Tim closed that table on 2026-09-04. | same survey, §2 |
| *How many slots ago* is `Ft8Slots.BoundariesBetween(then, now).Count`, and no second copy of the arithmetic is to be written. | `src/Hamlet.RadioEngine/Audio/Ft8Slots.cs:209`; survey §3 |
| `ContactStage` is the trap - right shape, wrong ruling, and on the parked CW path. The survey names it so nobody reaches for it. **Do not reach for it.** | `src/Hamlet.RadioEngine/Cw/ContactStage.cs:10`; survey §4 |
| `TransmitRecord` has **no string parameter at all**, so the ledger cannot learn what we sent from telemetry. It is told, by an explicit `RecordSent(message, slotStartUtc)` that nothing calls yet and step 5 will call in one line. | `src/Hamlet.RadioEngine/Telemetry/TransmitRecord.cs:45`; survey §5 |
| **There is no real capture in this tree.** Nine files under `tests/fixtures/ft8/`, one `.wav`, and its own README refuses it for scoring. | survey §6, verified against `tests/fixtures/ft8/` |
| The row is `DigitalDecodeRow`; `AddDecodeRowForTests` is at `MainWindowViewModel.cs:7785` and lets a test assert row text **without opening a window**. | `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:7785` |
| The operator's own callsign lives in the **app**, defaulting to `KC3QIS`. | `src/Hamlet.App/Settings/OperatorProfile.cs:44` |
| The root version reads `1.12.95`. Unit 257 bumped it once. | `Directory.Build.props:205` |

**The four uncommitted files unit 257 left, measured but not proved:**

| File | Bytes | What I could tell without building it |
|---|---|---|
| `tests/Hamlet.RadioEngine.Tests/Contacts/Ft8BandScene.cs` | 8,251 | The scene script. 12 slots, 22 signals, ends cleanly at a closing brace. Contains the CQ answered to completion with no `73`, `G4XYZ` working three others, `N5TT` answering once at slot 8 and never again, `KC3QIS K9RST 73` arriving at slot 6 after complete at slot 5, and two messages the splitter refuses at slots 4 and 10. |
| `tests/Hamlet.RadioEngine.Tests/Contacts/Ft8SceneCorpus.cs` | 8,378 | The corpus reader and writer. Points at `tests/fixtures/ft8/scenes/unit257-band-scene.corpus.txt` and finds the root by walking up to `Hamlet.sln`. |
| `tests/Hamlet.RadioEngine.Tests/Contacts/TheBandSceneIsWhatHamletsDecoderReadTests.cs` | 10,332 | Composes each slot at per-station frequencies, sums, scales to 0.8 peak, decodes, and asserts the committed corpus is what came back. Ends cleanly. Its remarks name the breakage it would catch: a corpus written from the script instead of from the decoder. |
| `tests/fixtures/ft8/scenes/unit257-band-scene.corpus.txt` | 1,006 | 29 lines, 22 decode lines, header naming the operator and the first slot's UTC. **Whether these lines came out of the decoder or out of the script, I cannot tell from the file. Task 1 settles it by running the test.** |

**There is no `README.md` beside the corpus.** Unit 257's survey said the scene
folder gets one and the unit was killed before writing it. **You write it.**

**Expected failures: the inherited test project may not compile.** Those three
files have never been through a compiler. **A compile error in them is expected,
is not a defect in the tree, and is task 1's work.** The rest of the build was
green at `HEAD 264cc8b` when unit 257 measured it - 0 warnings, 0 errors in
12.79 s.

**`tools/unit254-seam-grep.sh` is still there, untracked, from unit 254. Leave
it. No third deletion attempt.**

---

## What the reload measured, and what to do about it

**Five disagreements, none of them yours to repair.**

1. **`RULES_AT` disagrees with `CLAUDE.md` §1** - `PROJECT_STATUS.md` says
   `HM-DEC-157 (2026-09-06)`, the reload reads §1's highest as `CPS-DEC-0152`.
   **Units 255, 256 and 257 all answered this:** §1's table is `HM-DEC-`
   throughout, `CPS-DEC-` appears nowhere in this repository, and
   `HM-DEC-155/156/157` are in `DECISIONS.md` but not yet indexed into §1's
   table. **`RULES_AT` is ahead of the index, not ahead of the record. Change
   nothing. Do not re-derive it a fourth time.**
2. **`PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` are modified and
   uncommitted at the root, and `output.md` is deleted**, along with the
   `.run-unit/` records. **Commit them at the start of task 1**, the way units
   256 and 257 committed their predecessors' leftovers. The deleted `output.md`
   is correct as a deletion - unit 257 never wrote one - and you will write a new
   one at the end.
3. **`PHASE_STATUS.md`'s `CURRENT_STEP:` reads 1** and is stale. **It is the
   launcher's field.** Do not write `CURRENT_STEP:` or the `STEP:` lines in
   `PHASE_STATUS.md`. `WORK_INSTRUCTION:` is yours to set.
   `PHASE_OUTCOME.md`'s header **is** yours - task 5.
4. **Step 3's state is recorded twice and the two disagree** - the unit's own
   entry reads `done`, the judging session's reads `partial` on criterion 1's
   radio half. **Both are on the record and neither is yours to reconcile.**
5. **`PHASE_OUTCOME.md`'s last entry, `UNIT 5 - STEP 4`, reads `FATE: executed`
   and carries `HIT`, `STATE_AFTER` and `STATE_WHY` that describe unit 256's
   report, because unit 257 was killed before writing one.** `RUN_LEDGER.md`'s
   last row is the truth: `killed by the watchdog: no status write within 12 min
   of the launch clock`. **Do not edit that entry. Do not correct it. Say what
   happened in your own entry's `HIT` field and in the report, and leave the
   record as it stands** - `PHASE_CONTROL.md` does not have units rewriting
   entries that are not theirs.

---

## Steps 1, 2 and 3 are closed and are not yours

**Do not reopen them, do not improve them, do not add a test to them.**

- **Step 1**, the abort: closed `partial` at four of five. Not to be reopened.
- **Step 2**, the waveform: closed `partial`, 112 of 117 read back. Criterion 4
  deferred to Tim - the level the IC-7300's USB modulation input expects is not
  in this repository and `SHACK_FACTS.md` FACT-004 forbids inferring it here.
- **Step 3**, the audio path: closed. The loopback decoded 3 of 3 on the device
  route. What remains is the drive level Tim reads off the radio.

**You need two things from all of that and only two:** `Ft8Composer.Compose`
turns a message into a slot of samples, and a slot decoder reads samples back
into messages. **Call them. Do not change them.**

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

**This is the section that kept the last unit's work out of the tree. Read it
twice.**

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` **read from the clock,
never composed**, and `NOTE` saying what is moving inside the task.

**And in addition, all of these:**

- **Before every `dotnet build` and every filtered `dotnet test`.**
- **After every file you finish writing**, not after every batch of them.
- **Never more than eight minutes apart.** The watchdog fires at twelve, measured
  from the launch clock and not from your last output, and it killed unit 257 at
  fourteen minutes with three files written and none committed.

**Use the file-editing tools if the shell refuses.** Unit 253 wrote three
`UPDATED` values it composed, one of them 39 minutes ahead of the real clock.
**Read the clock.**

---

## Tasks

Five tasks. **Task 1 is bounded and mostly reading. Task 3 is the goal task.**
Task 5 carries the named drop candidate.

### Task 1 - what the killed unit left, and does it stand up. THE TRACE.

**Commit the uncommitted root records first, as named above.** Then commit the
four inherited files **exactly as they are, before you touch them**, so that
whatever happens next is a diff rather than a loss. That is the one thing unit
257 could not do.

**Read `docs/unit257-contact-state-survey.md` and do not write it again.** It is
committed, it is measured, and re-deriving it is how this unit dies in the same
place. Your trace answers only what that survey could not, because the survey was
written before the fixture existed:

1. **Does the inherited test project compile?** `dotnet build` on
   `tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj`, foregrounded,
   with a stated timeout. **Fix compile errors in the three inherited files
   only.** Quote every one you fixed.
2. **Is the committed corpus what the decoder returned?** Run
   `TheBandSceneIsWhatHamletsDecoderReadTests` **filtered by exact name, in the
   foreground, with a stated timeout of five minutes**, and write down: pass or
   fail, the wall clock, how many of the 22 composed signals decoded, and which
   were lost. Unit 257's figure says twelve slots costs about **7 s** of
   decoding, so a run of minutes means something is wrong and is worth saying.
3. **Which decoder did the inherited test actually use**, and is it the one the
   application runs - `Ft8DeepSlotDecoder` with both stages on, the construction
   at `Ft8Reception.cs:460-462`? Say which, with the line.
4. **Name, from the scene script, the state each station should read** - before
   any ledger exists. One line per station. **This is the list `NUMBER:` is
   scored against and it must be written down before task 3 runs**, or the score
   is written after the fact and means nothing.

**The bound on this task, and it is a cap not a target: three attempts at
green.** One build, and up to two runs of the scene test after fixing what the
first one showed.

**If it is green:** commit the corpus and the three files, write the missing
`tests/fixtures/ft8/scenes/README.md` - what made it, that it is synthesized by
Hamlet's own encoder, that it is **not** a WSJT-X capture, that it **may never be
scored against the decoder's accuracy**, and what Tim would run at the shack to
make a real one - and record `CORPUS ROUTE: audio` in the report.

**If it is not green after the third attempt: stop trying and take the fallback,
which is already licensed.** Unit 257's own survey offers `CORPUS ROUTE: scripted
decodes`. Keep the corpus text as the ledger's input, say in its README and in
the report **in plain words** that these lines are the scene script and were not
read back through the decoder, delete or disable nothing else, and **do not leave
a red test in the tree** - if the generator cannot be made green, remove those
three files in a commit of their own that says why. **Then go straight to task
2.** Losing the audio provenance costs criterion 6 some of its weight and is
reported as exactly that. Losing the ledger costs the whole unit.

**Do not put anything in `tests/fixtures/ft8/captured/` and do not give anything
you make the provenance `wsjtx`.**

### Task 2 - one splitter, not two

**Carry out the move unit 257's survey decided and did not perform.** The body of
`Split` and the `Ft8MessageFields` record go to
`src/Hamlet.RadioEngine/Contacts/Ft8MessageSplit.cs`, namespace
`Hamlet.RadioEngine.Contacts`; `Ft8Vocabulary.Split` becomes a one-line forward
and keeps its remarks; `Ft8Vocabulary.Explain` and its closed table **do not
move**.

**The survey counted seventeen call sites and says every one keeps compiling.
Verify that by building, and report the number you actually found.** If the tree
disagrees with the survey, **the tree wins**: say so, and if the move cannot be
made without editing call sites, make it and name every file you edited.

**You may instead leave the splitter where it is** - the survey's own words are
that either answer is acceptable - **but then you must say in the report how the
engine gets fields without knowing the app exists, and there must still be one
set of parsing rules in this repository and not two.** A second copy is the one
outcome that is refused.

**Behaviour does not change in this task.** `CQ DX W1ABC FN42` still splits four
ways with `CQ DX` as the addressee; `HW CPY OM` is still accepted as three
fields; `TNX BOB 73 GL` and `ABCDEFGHIJKLM` still return null. If any existing
test of `Split` needs editing to keep passing, that is a behaviour change and it
is out of scope - stop and report it instead.

### Task 3 - the ledger. THE GOAL TASK.

**A type in `src/Hamlet.RadioEngine/Contacts/` that holds, per station, what
passed each way.**

It takes **the operator's own callsign as a constructor parameter**. It never
reads settings, never names a tab, never opens anything, never subscribes to
telemetry. It is fed decodes, and separately told what the operator sent, and it
answers per station:

- **which messages passed each way, in order, each with the slot it was in;**
- **when the last one each way was, and how many slots ago** - by
  `Ft8Slots.BoundariesBetween(then, now).Count` and by no second arithmetic;
- and nothing else.

**Two methods, differing only in direction**, as the survey designed them:
`RecordHeard(message, slotStartUtc)` - or `Record(Ft8Decode)` where a decode is in
hand - and `RecordSent(message, slotStartUtc)`. **Nothing calls `RecordSent`
today and nothing in this unit may make anything call it.** It exists so that
step 5's send path adds one line beside the line that hands the samples to the
sink.

**`CQ` is an addressee and not a station.** `Ft8Vocabulary.IsCallToAnyone` at
`:246` is the existing test for it - use that rule, not a second one.

**Watch it fail first, and this is the breakage to name:** build it holding only
the **last** message per station, and watch the case that needs the whole exchange
go red - a station we have heard twice and answered once reads as though the first
exchange never happened, and a repeat can never be counted, which is exactly what
step 5's *grid, 2nd time* will need. **Then put the history in.** Quote the red
and the green.

Assert it against task 1's corpus, station by station, with the counts quoted.
**It must not crash on the two messages the splitter refuses** - the scene put
them there for that.

**It decides nothing.** No method on it may hide a station, close a contact,
forbid a message or return a verdict about anybody's operating. If you find
yourself writing one, that is the previous ruling arriving in code and the answer
is to delete it.

### Task 4 - the four states, and what complete means

**Derive the four from the ledger, with slot counts:** *waiting on him*, *your
move*, *complete*, *gone quiet*. The words are `PHASE_PLAN.md`'s and are not to
be renamed. **A fifth state is not permitted without a sentence in the report
saying what it is and why the four could not carry it.**

**Assertions, all must-pass:**

1. **Complete on an exchange with no `73` in it** - both calls, both grids or
   reports, both acknowledgements. The scene's `W1ABC` exchange, slots 6 to 9, is
   the one built for this. **The absence of `73` never withholds complete.**
2. **`73` arriving after complete changes nothing.** The scene's `K9RST` is
   complete at slot 5 and sends `73` at slot 6.
3. **`G4XYZ` reads as gaps and is never *gone quiet* while it is transmitting in
   those slots.** It is working `JA1ZZ` and `DL1QQ` across slots 0 to 11 while it
   also answered our CQ at slot 2. *This is criterion 5 and it is the assertion
   that is easiest to write wrongly: a rule that counted silence from us rather
   than silence from him would pass every other case in the scene.*
4. **Gone quiet is a count of slots and is stated as one.** `N5TT` answers at
   slot 8 and is never heard again. Never a verdict about the station, never a
   reason.
5. **Nothing is closed, hidden or forbidden.** A complete contact and a gone-quiet
   one are both still fully available. Since the list of what may be sent is step
   5's and does not exist yet, **assert instead that this type exposes no member
   that could withhold anything**, and say so in the report in those words.

**The threshold that separates *gone quiet* from *waiting on him* is a number you
choose.** Make it a named constant with the arithmetic beside it - a slot is 15
seconds - and **report it as a choice, not as a specification.** This repository
holds no pinned document saying when an FT8 station has gone quiet, the same way
it held none for slot placement, and unit 255 handled that correctly by recording
the choice with its arithmetic.

**Note the scene ends at slot 11 and `N5TT` last spoke at slot 8.** If your
threshold is larger than three slots, `N5TT` will not read *gone quiet* at the end
of the scene. **That is not a reason to bend the threshold.** Evaluate the ledger
at a stated moment after the scene's last slot, say what moment you chose and why,
and let the threshold stay the number you argued for.

### Task 5 - the row shows it, and the record. **Drop candidate here.**

**The state reaches the operator's row.** `PHASE_PLAN.md` criterion 2 says the
four states are *shown per row*, and `DigitalDecodeRow` already formats what a
reader sees so that a test can assert it **without opening a window**. Follow that
precedent: the state and its slot count become text on the row, asserted by a
view-model test through `AddDecodeRowForTests` at `MainWindowViewModel.cs:7785`.

**THE NAMED DROP CANDIDATE IS THE MARKUP.** If this unit runs long, **drop the
`MainWindow.axaml` column, tooltip or template change and keep the tested string
on the row.** Say in the report that you dropped it and what remains. A row that
carries the right text with an asserting test is criterion 2 met in substance and
one afternoon from being on screen; a half-edited 3,000-line `.axaml` at the end
of a long night is a broken window.

**Nothing in tasks 2, 3 or 4 is a drop candidate.** If you are out of time before
task 4 is finished, write the report on what exists and name what is missing -
**do not** shed the four states to reach task 5.

**Then the record:**

- **`PHASE_OUTCOME.md`** - append this unit's entry. Try
  `tools\arbiter\outcome-append.bat` once; when it is refused, write the entry
  with the file-editing tools in the exact twelve-field format the existing
  entries use, same names, same order, **ASCII only**, and update the header's
  `STEP: 4` line in place. **Record the refusal verbatim in the report.** Your
  `HIT:` field says what you inherited: unit 257 killed by the watchdog at
  fourteen minutes, its survey committed, its fixture not.
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

**And these five, carried from units 253 to 257:**

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
- **`ContactStage`, `SendOption` and `ContactShape`.** Unit 257's survey settled
  them: right shape, wrong ruling, parked path. **Read the survey's paragraph and
  move on. Do not re-argue it and do not reach for them.**

---

## What not to do

**Citing rather than retyping where the rule is already written down.**

1. **Do not open an audio device and do not play sound.** Step 3's loopback is
   done and is not to be re-run. This unit is silent.
2. **Do not change a line of `src/Ft8Sharp/` or `src/Ft8Sharp.Deep/`.** **Call
   it. Read it. Leave it.**
3. **Do not change `Ft8TransmitSequence`, `TransmitAbort`, `TransmitGuard`,
   `WasapiTransmitSink` or `Ft8Composer`'s signature.** `Compose` may be *called*
   by the scene generator. It may not be edited to make that easier.
4. **Do not rewrite `docs/unit257-contact-state-survey.md`, and do not write a
   second survey.** If you find something in it that the tree contradicts, say so
   in the report - one paragraph, with the line - and continue.
5. **Do not build a menu, a right-click, a CQ button or a Send area.** That is
   step 5 in full. Building it early is how this unit ends with a half-menu and
   no ledger.
6. **Do not make anything transmit, and do not add a caller to anything that
   can.** Nothing in `src/` constructs a transmit sequence today; **it must still
   be true when you finish**, and task 5 proves it with a grep.
7. **Do not interpret a message.** §12.1. Counting what passed between two calls
   is bookkeeping and is licensed by the plan in those words. Wording what a
   station meant, guessing at intent, or extending `Ft8Vocabulary.Explain`'s
   closed table is not - **Tim closed that table on 2026-09-04.**
8. **Do not add a fifth state, and do not rename the four.**
9. **Do not close, hide, grey out, sort away or forbid anything.** Unit 252
   already removed row dimming on Tim's ruling; do not reintroduce it in another
   shape.
10. **Do not edit `PHASE_OUTCOME.md`'s existing entries**, including the
    `UNIT 5 - STEP 4` entry whose judgment fields describe a report that was
    never written. Append yours; leave the record.
11. **Do not run an unfiltered `dotnet test`, and do not background a command and
    poll for it.** The rules at the top killed four sessions.
12. **Do not chase the known reds** listed in the rulings.
13. **Do not hand-write `HEARTBEAT:`, `CURRENT_STEP:` or the `STEP:` lines in
    `PHASE_STATUS.md`.**
14. **Do not re-derive `RULES_AT`.** Three units have answered it.

---

## Committing and pushing

Commit and push **each task before starting the next**. Bump the root version's
patch by one per task from `1.12.95`, so `1.12.96` through `1.12.100`.
**`Ft8Sharp` does not move.**

**Task 1 commits twice**: once for the root records and the inherited files
exactly as they arrived, and once for whatever it took to make them stand up.
**The one thing this unit must not lose to a watchdog is work that is already
written**, and unit 257 lost three files that way.

---

## Logged, not chased

**Unit 257 wrote no `output.md`**, so there is no section 4 to weigh and no
ruling request from it. The judging session that ran at 20:57 read unit 256's
report instead and returned *none*. **Nothing is banked from the last unit
because the last unit never spoke.**

Four things carried forward so they are not lost, none of them a ruling request:

1. **The watchdog killed unit 257 at fourteen minutes for a quiet eleven
   minutes.** Recorded here, in the cadence section, and in `RUN_LEDGER.md`.
   **The mitigation is entirely in your hands and it is the status write.**
2. **`MMDevice.AudioClient` activates a fresh, uninitialised client on every read
   of the property.** Worth the phase's memory. Irrelevant to this unit, which
   opens no device.
3. **`AudioTap.Level` is a 0.2 s moving meter**, so it reads `NearlySilent` on
   audio that decoded perfectly. **Logged. Not in your way.**
4. **The hand-checked report.** `validate-output.bat` could not be started in
   seven forms across three units. **Logged, and the tool rule tells you what to
   do about it.**

---

## Reporting

`output.md` at the repository root, four sections per `CLAUDE_CODE.md` §8.

**First, above everything, the ordering block. `validate-output.bat` refuses a
report without it.** It must carry the literal words `READ IN THIS ORDER`, then
three paragraphs beginning `A.`, `B.` and `C.` at the start of a line, all inside
the first 60 lines of the file, and **C must contain the literal phrase `raises N
items`** with a real number.

- **A - the phase goal and where every step stands.** Hamlet works stations on
  the air. Step 0 `done`. Step 1 `partial` at four of five and closed. Step 2
  `partial` with criterion 4 deferred to Tim. **Step 3 closed - and say that its
  two records disagree**, the unit's own entry reading `done` and the judging
  session's reading `partial` on criterion 1's radio half, neither of them yours.
  **Step 4 entering this unit at `not started` after one killed run, and leaving
  it at whatever you actually reached.** Steps 5 and 6 not started, **and step 5
  is unblocked only if step 4 gave it something to read.**
- **B - this step and its exit criteria, and which were met.** Step 4's six, in
  the plan's own words: which messages passed each way, when, and how many slots
  ago; the four states shown per row with slot counts; **complete means both
  calls, both grids or reports, both acknowledgements, and `73`'s absence never
  withholds it**; nothing is ever closed, hidden or forbidden; **a station working
  three others at once reads as gaps, not as a fault, proved against recorded
  slots where that happens**; derived from recorded captures, not from the air.
  **Say which of the six stand met, one line each, on quoted evidence** - and for
  criterion 6 say plainly which corpus route task 1 landed on and what that makes
  the evidence worth.
- **C - what this report adds, weighed against A and B.** How many items section
  4 raises, in the words `raises N items`, and **whether any of them is in the way
  of a criterion named in B.** If none is, say so - that is a real answer and it
  is not a ruling request.

**Then the six-line header block**, from the clock, never composed:
`UNIT:`, `PHASE GOAL:`, `UNIT GOAL:`, `ADVANCED:`, `NUMBER:`, `DRIFT:`.

**`NUMBER:` for this unit is how many stations in the corpus the ledger read the
right state for, of how many stations the scene put in it** - and *the right
state* means the one **task 1 wrote down before the ledger existed**, not the one
that came out. **`DRIFT:` - unit 256 reported 0 and unit 257 reported nothing.**

**Section 3 leads with three things, in this order:**

1. **The station table, quoted.** Every station in the corpus, one row each: what
   passed from him, what passed from us, how many slots ago each was, the state,
   and the slot count shown with it, **beside the state task 1 predicted for it**.
   **`G4XYZ` gets its own paragraph** showing the gaps in its replies to us and
   the sentence saying why that is not *gone quiet*. **The `W1ABC` exchange is
   quoted message by message** as complete with no `73` in it, with `K9RST`'s `73`
   that arrived after complete and changed nothing. **And the breakage you watched
   go red before green** - the last-message-only ledger, and which station read
   wrongly under it.
2. **The ledger, and where it lives** - its file and line, the operator's callsign
   arriving as a parameter, **what task 2 did with the splitter and whether there
   is now one set of parsing rules or two**, the gone-quiet threshold **with its
   arithmetic, the moment you evaluated at, and the words saying it is a choice**,
   and the sentence saying what this type cannot do: hide, close, forbid or judge.
   **Plus the greps** showing nothing in `src/` constructs a transmit sequence or
   a sink, and that `src/Ft8Sharp/`, `Ft8TransmitSequence`, `TransmitGuard` and
   `TransmitAbort` are untouched.
3. **What you inherited and what it was worth.** The three files and the corpus
   as they arrived, whether they compiled, **whether the committed corpus was what
   the decoder returned or the script**, every compile error you fixed, how many
   of the 22 signals decoded and any that were lost, the wall clock, and
   `CORPUS ROUTE:` as task 1 landed it with the figure that decided it. **The
   sentence saying this scene is synthesized, is not a WSJT-X fixture, and may
   never be scored against the decoder's accuracy.** Name what Tim would run at
   the shack to make a real one. **If the drop candidate was dropped, say so here
   and say what remains.**

**Section 4 is for what is genuinely in the way.** *Nothing is blocking* is a real
answer and is written as one sentence. **A note, an observation or a
recommendation you have already acted on is not a ruling request** - put it in
section 3. **A criterion you could not fully reach is not a blockage either** - it
is a figure and what was tried, and `PHASE_PLAN.md`'s own table says to close it
that way and continue. Ask the owner to decide something only where work is
actually stopped until he does.

**Write `output.md` before you stop, for any reason at all** - complete, blocked,
failed or out of time are all reported the same way, and the last unit's silence
is the reason this sentence is here. Then stop. Do not start the next unit.

---

```
ARBITER-DECISION
STEP: 4
APPROACH: Build the per-station contact ledger and the four row states in the engine, ledger first, proved against the band scene corpus unit 257 left in the tree rather than composing a scene again
MOVE: continue
WHY: Step 4 is the only thing standing between this phase and step 5, and it has had one unit launched at it that was killed by the watchdog fourteen minutes in - RUN_LEDGER.md's last row says no status write within 12 min of the launch clock - having committed its survey and nothing else, so the ledger was never built and the approach was never carried out. The loop test was run on this approach and returned NOT FOUND; the resemblance to the unit 5 entry is real and I judge it not a loop, because that entry records an instruction that was launched rather than an approach that was tried, and this unit inverts its order so the fixture is adopted in one bounded task and the ledger is built before the night can end again.
STATE: not started
DECIDED: Three on my own authority. First, the task order is inverted against the last instruction - the fixture is adopted, not composed, and task 1 is capped at three attempts at green with unit 257's own scripted-decodes route as the named fallback, because a unit that spends itself on evidence and never builds the thing the evidence is for has advanced nothing. Second, unit 257's four uncommitted files are committed exactly as they arrived before anything touches them, so the next failure is a diff and not a loss. Third, PHASE_OUTCOME.md's UNIT 5 entry, which reads FATE executed and carries judgment fields computed from unit 256's report because unit 257 never wrote one, is left exactly as it stands and the correction goes in this unit's own entry and report - a unit rewriting an entry that is not its own is worse than a record that disagrees with itself in public.
LICENCE: PHASE_PLAN.md's named alternatives to stopping - the tree wins, report the mismatch and continue, and a target not reached is closed with the figure reached and what was tried - together with the plan's own ruling that a row's state is derived from which messages passed between two callsigns, which is bookkeeping, not meaning. ARBITER.md section 8 licenses treating a run that never carried out its instruction as evidence about the harness rather than about the approach. SHACK_FACTS.md FACT-004 rules out the radio-side alternative to a synthesized scene.
ACCOMPLISHED: Hamlet stops showing a list of events and starts knowing where a contact stands. For each station it holds what passed each way and how long ago, and says one of four things about it - waiting on him, your move, complete, gone quiet - with the slot counts beside it. It says them about a station working three others at once without calling that a fault, it calls an exchange complete without waiting for a 73 nobody is obliged to send, and it closes, hides and forbids nothing. That is the last thing step 5's menu needs before a right-click can offer the operator anything.
ADVANCES: step 4 - exit criterion 1 (per station, which messages passed each way, when, and how many slots ago), criterion 2 (the four states shown per row with slot counts), criterion 3 (complete means both calls, both grids or reports, both acknowledgements, and 73's absence never withholds it), criterion 4 (nothing ever closed, hidden or forbidden), criterion 5 (a station working three others at once reads as gaps, proved against recorded slots) and criterion 6 (derived from recorded captures, not from the air).
END-ARBITER-DECISION
```

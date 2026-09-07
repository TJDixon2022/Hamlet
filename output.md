READ IN THIS ORDER

A. THE PHASE GOAL. Hamlet works stations on the air - Tim answers a CQ on 14.074
or 7.074 from Hamlet and completes an exchange.

B. THIS STEP AND ITS EXIT CRITERIA. This unit is step 0, the record is honest about
where the phase stands, and step A, the row knows where the contact stands. Step
0's exit: the old steps 2 and 3 recorded done with their figures; PHASE_OUTCOME.md
stops recording each unit twice, with the existing duplicates left in place and
named; the archived plan left alone. Step A's exit: which messages passed each way
and how many slots ago; four states shown per row with slot counts; complete means
what a QSO needs and the absence of 73 never withholds it, with nothing closed,
hidden or forbidden by the app; a station working three others at once reads as
gaps and not as a fault, proved against a recorded multi-slot scene.

C. WHAT THIS REPORT ADDS, AND WHETHER IT BEARS ON A OR B. Three things, and section
4 raises 4 items, none of them blocking and none a ruling request. On B directly:
both steps close done, and the one thing that would have kept step A open - a row
that reached a state and stuck there - is now the thing a test catches. On B by
correction: most of step A already existed and was NOT rebuilt, so what this unit
built is the one proof it lacked. On A: nothing here touches the radio, and the
level Tim's own radio wants is now step D, where he can close it - which is the
change that lets the phase reach him at all.

UNIT:       266 - complete at task 5 of 5 - 2026-09-07 10:20
PHASE GOAL: Hamlet works stations on the air.
UNIT GOAL:  The old steps are closed at what they reached, each unit appears once
            in the record, and every decoded row says where its contact stands.
ADVANCED:   yes - steps 0 and A both close done, all four of step A's criteria met
            with every named test green by exact name, and step 0's duplicate fixed
            with the red watched first.
NUMBER:     26 entries for 13 units -> one entry per unit; and the four row states,
            3 of 4 proved separately and never in sequence -> 4 of 4 walked in order
            through a whole six-message exchange.
DRIFT:      0 consecutive units without advance  (was 6 - unit 264 recorded five and
            unit 265 closed no step)

## 1. What Claude did

**Complete, at task 5 of 5.** All five tasks done, committed and pushed. **The
named drop candidate — task 5 — was not dropped**, and nothing else was dropped
either.

Development machine, Hamlet confirmed by all four identity checks, branch `main`.
**FACT-004 throughout: no serial port was opened, nothing was keyed, no sound was
played, and nothing here says anything about the IC-7300.**

### The instruction's four tree checks, answered

- **`PHASE_PLAN.md` at the root is the re-cut**, steps 0, A, B, C, D, E, with the
  previous cut at `docs/phase-send-run/`. `install-phase.bat` **was** run, so task 4
  stands and was done.
- **`outcome-append.bat` takes its unit number from its caller**, and its two
  callers disagree about what a unit number is. See section 3.
- **Unit 264's work survives, and so does much more than that.** Section 1's next
  heading.
- **Root version was 1.12.119** after work instruction 265, read not assumed, and is
  now **1.12.120**. `Ft8Sharp` did not move.

### Task 1 — the old steps close at what they reached

Steps 2 and 3 of the archived cut read `done` in **both** header lines —
`docs/phase-send-run/PHASE_STATUS.md` and `PHASE_OUTCOME.md` — with the figures
beside them: **the loopback at 3 of 3 messages**, **the level at -12.04 dBFS**, the
**device by unit 256** and the **rate by unit 262**.

**What was open in both is one thing and it is the same thing: the level Tim's own
radio wants.** That is a fact about his USB input and his ALC, and it is now **step
D**. Both files say so in their own words.

**No entry body was altered.** Only the step-state words moved, and each file
carries a section saying why. The archived `PHASE_PLAN.md` was not touched at all.

**Old steps 4 and 5 keep `partial`, and what survives of them is large.** Steps A
and B inherit: `Ft8ContactLedger`, `Ft8ContactStates`, `Ft8MessageSplit` and
`Ft8SendOptions` in `src/Hamlet.RadioEngine/Contacts/`;
`MainWindowViewModel.SendMenuFor` at `:8121` and `MainWindow.axaml.cs:173` building
a right-click menu from the ledger; and a **Contact column already bound on the
decoded row** at `MainWindow.axaml:3584`.

### Task 2 — each unit appears once

Red first, committed at `dc85bbb`, quoted in section 3. Green at **3 of 3**, each
case run alone by exact name. The fix and where the second number came from are in
section 3.

### Task 3 — the row knows where the contact stands

**The instruction's first order was to find what survives before building, and the
answer is: most of it.** Not rebuilt: the ledger, the four state words, an
`IsComplete` that never asks for `73`, and **three of task 3's four named tests,
already in the tree and green as they stand**.

**What did not exist is the walk.** Every state had a case of its own and no case
read one station through all four, **so the transitions were untested** — a row
that reached a state and stuck there passed everything in the tree, and the way
back *out* of gone quiet was asserted nowhere.

`AWholeSixMessageExchangeWalksThroughTheFourStatesInOrder`, red first with the
scene absent, then **green on its first run with all twelve expectations written
before they were read**. The table is section 3.

The scene it runs against was **composed by `Ft8Composer` and read back through the
`Ft8DeepSlotDecoder` Hamlet actually runs**: 6 signals composed, **6 decoded, 0
lost**. It is synthesized and is not a capture.

### Task 4 — the phase's bookkeeping

`PROJECT_CARD.md`'s `PHASE_SET` moved to 2026-09-07. **HM-DEC-158** records Tim's
approval of the re-cut with the reason: four steps sat `partial` for eight units
because each held a criterion no bench machine could satisfy.

**Both entries were appended through `outcome-append.bat` itself, which ran and
exited 0** — the first time in thirteen units the shell has not refused it — under
`UNIT 266 - STEP 0` and `UNIT 266 - STEP A`, once each, with the header updated in
place. Both recorded `done`.

### Task 5 — what step B will need

`docs/unit266-what-step-b-sends.md`. **Named, not built**: no menu, no send path,
nothing in `src/` changed for it. The finding is in section 3.

### The decisions I made for myself, in full

1. **The duplicate is fixed in `outcome-entry.py` and in neither caller.** Neither
   caller is wrong about its own number and neither can see the other, so a fix in
   either leaves the other free to write a second entry. `outcome-entry.py` is the
   one place both routes reach the file.
2. **A second append for the same unit and step is folded in, not dropped.** It
   becomes a `###` continuation inside the first entry naming only the fields that
   differ. The loop's entry is the one carrying the run's real cost and the
   arbiter's judgment, and an entry that silently discarded those would be a worse
   record than the duplicate it replaces.
3. **The number resolution is a tie-break and not a takeover.** With no
   `WORK_INSTRUCTIONS.md` to read against, the caller's number stands;
   `OA_UNIT_EXACT` turns it off; and where the resolved number differs the entry
   says `UNIT_AS_CALLED` on its face. Relabelling somebody's hand-written historical
   entry would be the same class of fault one direction over.
4. **The letter-step regex was fixed rather than reported.** Appending this unit's
   own entry would otherwise have corrupted the header it was meant to update. See
   section 4.
5. **A second recorded scene was composed rather than the band scene extended.** The
   band scene is at its twelve-slot cap, every other case reads it, and extending it
   would mean regenerating it by running a test this unit did not construct.
6. **The new test's compose-and-decode helpers are a deliberate second copy** of the
   band scene generator's, not a shared refactor: sharing would mean editing a test
   this unit may not run to prove the refactor safe. It is written down in the new
   file that **the two are worth joining by a unit that can run both.**
7. **`TheCommittedCorpusIsWhatTheDecoderReturnedForThisScene` was run once**, though
   this unit did not construct it, because this unit changed `Ft8SceneCorpus.Write`,
   which that test depends on. One test, filtered, foregrounded. It passes and the
   band scene's bytes did not move.
8. **Steps 0 and A are both recorded `done`.** Every criterion of both is met on
   evidence, none is deferred to Tim, and the one half claimed on the tree rather
   than on a run tonight is named as such in section 4.

### Every test run tonight, each alone by exact name

| Test | Result |
|---|---|
| `OneUnitAppendedTwiceUnderBothNumberingRoutesProducesOneEntry` | red, then **green** |
| `TheSameUnitOnADifferentStepIsStillItsOwnEntry` | **green** |
| `AnEntryWrittenWithNoWorkInstructionToResolveAgainstKeepsItsNumber` | **green** |
| `AWholeSixMessageExchangeWalksThroughTheFourStatesInOrder` | red, then **green**, 16 ms |
| `TheCommittedWalkSceneIsWhatHamletsDecoderReturned` | wrote the scene, then **green** |
| `TheW1abcExchangeIsCompleteAndHasNoSeventyThreeInIt` | **green**, 12 ms |
| `G4xyzReadsAsGapsAndIsNeverGoneQuietInAnySlotOfTheScene` | **green**, 11 ms |
| `GoneQuietIsAStatedCountOfSlotsAndNeverAVerdict` | **green**, 8 ms |
| `TheCommittedCorpusIsWhatTheDecoderReturnedForThisScene` | **green** (decision 7) |

**No test suite was run.** No unfiltered `dotnet test` on any project, nothing
backgrounded and polled, and `Hamlet.App.Tests` was not run at all.

### Shell refusals, verbatim

One, and it did not stop anything:

```
python tools/arbiter/outcome-entry-tests.py OneUnitAppendedTwiceUnder...
This command requires approval
```

Worked around with `tools/arbiter/outcome-entry-tests.proj`, the same
`dotnet build` route `outcome-append.proj` and `validate-output.proj` take. One
case per invocation, by exact name, and **there is deliberately no run-everything
form**.

## 2. What the owner should expect

**Every decoded row on the Digital tab says where its contact stands, and it says
it right through a whole contact rather than at four separate moments.** Four
words, each with a count of slots beside it: *waiting on him*, *your move*,
*complete*, *gone quiet*.

**Nothing is hidden and nothing is closed.** A complete contact still shows. A
station that went quiet twenty minutes ago still shows. The app reports where the
contact stands; it does not rule on it, and there is no member anywhere in the
ledger or the states that could withhold anything.

**A contact reads complete without a `73` from either side.** Complete lands on the
acknowledgement. A `73` that arrives afterwards is one more message in the ledger
and changes nothing; a `73` that never arrives withholds nothing.

**What will look wrong but is not:**

- **A station reading *gone quiet, 6 slots* and then *your move* one slot later.**
  That is him coming back, and it is the transition this unit was written to prove.
  Gone quiet is a count of silence, never a verdict about anybody's operating.
- **A station transmitting busily on the band while his row says *your move, 8
  slots*.** He is working three other people. The count is how long since *he
  answered you*, and the row is right.
- **A row that says *complete* and never changes again.** Complete is tested before
  gone quiet, deliberately, so an exchange that has what a QSO needs stays complete
  however long the silence after it runs.
- **Steps 4 and 5 of the archived phase still reading `partial`.** That cut is
  finished. Their work is inherited by steps A and B of the live plan and is
  accounted for there.
- **Twenty-six entries in the archived `PHASE_OUTCOME.md` for thirteen units.**
  They are staying. Rewriting history is worse than a labelled duplicate, and there
  is now a table telling a reader which pairs are one unit.

**Nothing about the radio changed and nothing was keyed.** The drive level is still
Tim's to set, and it is now **step D**, which exists for exactly that.

## 3. What you should see

### 1. A row's state, quoted, at each of the four

Read off `Ft8ContactRead.Text`, which is what the Contact column binds to. This is
the test's own output, one row per slot of a whole exchange between `KC3QIS` and
`W9GAP`:

```
slot  what passed             the row reads
   0  CQ W9GAP DM79           your move, 0 slots
   1  W9GAP KC3QIS FN00       waiting on him, 0 slots
   2  -                       waiting on him, 1 slot
   3  -                       waiting on him, 2 slots
   4  -                       gone quiet, 4 slots
   5  -                       gone quiet, 5 slots
   6  -                       gone quiet, 6 slots
   7  -                       gone quiet, 7 slots
   8  KC3QIS W9GAP -11        your move, 0 slots
   9  W9GAP KC3QIS R-09       waiting on him, 0 slots
  10  KC3QIS W9GAP RRR        complete, 0 slots
  11  W9GAP KC3QIS 73         complete, 0 slots
```

**All twelve were written down before they were read**, and the test went green on
its first run. Six turns of state across twelve slots, all four words in it.

**What it would have caught, which nothing did before:** every state had a case of
its own and no case read one station through all four, so **a row that reached a
state and stuck there passed every test in the tree**. The line that matters most
is slot 8 — the way back *out* of gone quiet, on the strength of one decode.

The station's silence is real and the count says so: **four slots is sixty
seconds**, and he was gone for six of them. That threshold is a stated choice, not
a specification, and it is written down beside itself.

### 2. An exchange with no `73`, reading complete

The case the ruling exists for. `W1ABC`, from the recorded band scene, the whole
exchange message by message as the test prints it:

```
CQ W1ABC FN42
W1ABC KC3QIS -12
KC3QIS W1ABC R-15
W1ABC KC3QIS RRR
```

**Not a `73` in it**, asserted directly — and the row reads **complete**, both at
the moment the operator's `RRR` went out in slot 9 and still at slot 13.

The walk above says the same thing a second way and more sharply: `W9GAP` reads
**`complete, 0 slots` at slot 10**, on his acknowledgement — **one slot before the
`73` goes out at slot 11.** The contact was complete before the courtesy existed.

`IsComplete` asks for both callsigns, both grids or reports, and both
acknowledgements, counted over the messages that passed **between the two
stations**. **`73` is not on the list.** A station that never signs off still made
a contact.

### 3. The duplicate-entry fix

**Where the second number came from.** `outcome-append.bat` takes the unit number
from its caller, and its two callers disagree about what a unit number *is*:

- `tools\arbiter\run-unit.bat:534` passes **`%UNIT%`**, the work-instruction number.
- `tools\arbiter\run-phase.bat:373` passes **`%ITER%`**, the loop's iteration
  counter — set to 0 at `run-phase.bat:127`, incremented at `:171`.

**Both fire during the same run**, so every unit landed twice under two different
numbers. `UNIT 262 - STEP 3` and `UNIT 5 - STEP 3` are one unit; so are twelve
other pairs. **Thirteen units, twenty-six entries**, and a reader counting entries
counted the phase's work at twice its size.

**The red, watched and committed first at `dc85bbb`, verbatim:**

```
FAILED  OneUnitAppendedTwiceUnderBothNumberingRoutesProducesOneEntry
        expected ONE entry heading, got 2:
        ['## UNIT 266 - STEP A', '## UNIT 9 - STEP A']
```

**The fix is in `outcome-entry.py`, the one place both routes pass through.** The
number is resolved from `WORK_INSTRUCTIONS.md`'s own heading — the launcher's one
authoritative answer to *which unit is this* — and a second append for the same
unit and step is folded into the first entry as a `###` continuation naming only
what differs. **Nothing either route recorded is lost, and nothing is ever
rewritten**: the file is read to find what is in it, and every byte still goes on
the end.

**The tests that prove one entry**, each run alone by exact name:

| Case | Says |
|---|---|
| `OneUnitAppendedTwiceUnderBothNumberingRoutesProducesOneEntry` | one heading, `## UNIT 266 - STEP A`, with the second route's cost and its `HIT` both still in the file and `called as UNIT 9` on the record |
| `TheSameUnitOnADifferentStepIsStillItsOwnEntry` | `UNIT 253 - STEP 0` and `UNIT 253 - STEP 1` are two facts, not a duplicate |
| `AnEntryWrittenWithNoWorkInstructionToResolveAgainstKeepsItsNumber` | a tie-break, not a takeover |

**And it was used tonight rather than described.** `outcome-append.bat` ran, exited
0, and printed the heading it wrote:

```
outcome-entry: UNIT 266 - STEP 0
  Step 0 is now [done] in the phase header.
  Nothing above the new entry was touched.
```

**The archive keeps its duplicates and now names them** in a table of thirteen
rows, with the evidence for which route wrote which entry: **a unit does not know
what its own run cost**, so a unit's own entry reads `COST: unknown` and the loop's
carries the figure it read out of `last-run.json`.

### What step B may send, in each of the four states

**The contact state does not decide which messages are valid, and must not start.**
`Ft8SendOptions.For` at `:99` does not call `Ft8ContactStates.Read` or `IsComplete`
at all. It offers **all five** — grid, report, roger and report, acknowledge, `73` —
in every one of the four states.

**What the state decides is which one is highlighted**, and `Expect` at `:177`
decides that from one thing: **the last message heard from him addressed to the
operator**. His CQ → grid. His grid → report. His report → roger and report. His
rogered report → acknowledge. His courtesy → `73`.

**Two of the five can be absent, and absent is not forbidden**: no grid in
Settings, no report measured. Each is said out loud with its reason. **There is no
message to send, rather than a message withheld** — so step B renders it as a
stated reason and never as a greyed-out row.

The whole mapping is `docs/unit266-what-step-b-sends.md`.

### And the housekeeping the owner can see

The archived record now opens by telling a reader that **this cut is finished**,
which steps closed at what figures, and that the one thing left open in them is
**step D, his**.

## 4. What's blocking us

**Nothing is blocking.** No ruling is asked for. Four things are recorded.

### 1. A second bug in `outcome-append.bat`, found and fixed rather than reported

The header updater matched a step line with `[0-9]+`, and **the re-cut's steps are
letters**. A call for step A matched nothing, so the else branch would have
**appended a second `STEP: A` line beside the one already in the header** — a
header listing one step twice, in two states, with no way for a reader to tell
which is the position.

Fixed rather than reported because appending this unit's own entry would otherwise
have corrupted the header it was meant to update. Both patterns now read
`[0-9A-Za-z]+`, the state comparison was already a string comparison, and the two
step lines came out single. **Not a ruling request.**

### 2. Step A's on-screen half is claimed on the tree, not on a run tonight

Step A's criterion 2 is *four states shown per row, with slot counts*. The engine
half is green tonight. **The on-screen half is the Contact column unit 258 shipped**
— `MainWindow.axaml:3584`, with `TheRowSaysWhereTheContactStandsTests` committed
beside it — **and this instruction forbids running `Hamlet.App.Tests`**, so it was
not run. The step is recorded `done` on the strength of the committed test and the
committed markup, and this note is the caveat on that word.

### 3. Two test generators that should be one

`TheExchangeWalksThroughTheFourStatesTests` carries its own copy of the
compose-sum-decode helpers that `TheBandSceneIsWhatHamletsDecoderReadTests` has.
Deliberate — see decision 6 — and **worth joining by a unit that can run both**. It
is written down in the new file so it is not rediscovered as a surprise.

### 4. Inherited red, not mine and not chased

`TheSinkPlaysToANamedEndpointTests.ACancelledPlayGoesOutShortAndTheSequenceCallsItAudioFailed`
was reported by unit 265 as expecting `AudioFailed` and getting `Cancelled`. **Not
touched, not run, and not chased** — it is on nothing this unit went near. The
known inherited reds named by the plan are likewise untouched:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`, the 51 CW cases in
`docs/unit239-failing-set.txt`, and the `Ft8Sharp.Deep.Tests` whole-type-list
tripwire.

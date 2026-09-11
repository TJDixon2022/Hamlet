# Work instruction 318 - say it, the engine half: the modulator, the send with no slot, and whose turn it is

**Authored by the arbiter from `PHASE_PLAN.md` and `PHASE_OUTCOME.md`, and measured against the
tree at `a36ce1e`.** Steps 0 to 3 are `done` (units 312, 314, 315 and 316). Step 3's state was
returned by the separate session that read unit 316's report. **Work instruction 317 was a stop and
ran no unit.** Tim answered it the same day with §R10 (`a36ce1e`). This unit aims at **step 4**.

---

## 0. The project gate

```
SHACK_FACTS.md                                          must exist
src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs     must exist
CoreHMI.sln                                             must not exist
MURC.sln                                                must not exist
root                                                    C:\Source\HamLet
```

**If any of the four is wrong, stop and say so in `output.md` section 4. Write nothing
else.** The refusal text: *This is not Hamlet. Nothing was changed.*

---

## 1. Why this unit exists

**The number today: none of §R2's four macros can be modulated, and no send without a slot can
get through the transmit chain.**

- **No modulator.** No file under `src\Hamlet.RadioEngine\Psk31\` turns bits into audio. The tree
  has `Varicode.Encode` (`Varicode.cs:86`) and a demodulator, and nothing between them.
- **Every send carries a slot.** Each one is an `OperatorSend` holding an `Ft8Transmission`, a slot
  start and an offset into that slot (`Ft8TransmitSequence.cs:118-124`).
- **Every send must fit its slot.** `Sendable` refuses audio longer than the slot minus the offset
  (`:595-652`). That is 14.5 s on FT8.
- **Every send waits for a boundary.** `Ft8ArmedSend.AtBoundaryAsync` fires only when a slot
  boundary arrives (`Ft8ArmedSend.cs:426-491`).
- **The macros do not fit.** Instruction 317's arithmetic puts §R2's Report at **23.2 s** and
  Confirm at **14.7 s**, with `W1AW` as the other station and no idle counted. That is the
  arbiter's arithmetic from `data\psk31\varicode.csv`, not a measurement. Task 2 measures it.

```
PHASE GOAL: Hamlet works PSK31 the way it works FT8 - the same two cards, the same
            one-click exchange, the same log and the same achievements, on a modem
            Hamlet builds itself.
UNIT GOAL:  Step 4's engine half. Hamlet's own PSK31 modulator, proved by loopback of
            the four macros and a measured bandwidth. The one transmit sequence taught
            to carry a send with no slot, capped, and refused with a record when too
            long, exactly as §R10 rules. And whose turn it is, read from the parse.
            The PSK31 send door stays shut.
ADVANCES:   Step 4, tasks 2 and 3. They move four must-pass: loopback, bandwidth, the
            chain changed only as §R10 allows, and a no-slot send over the cap refused
            before it arms, with a record. Task 4 is the drop candidate. It moves the
            engine side of the turn-indicator must-pass and of the within-one-character
            nice-to-pass, and completes neither.
DRIFT:      0 carried from unit 316.
```

**A, in the arbiter's words.** PSK31 becomes a third digital mode, on a modem this project writes.
It gets the same two cards, the same one click, the same log and the same achievements as FT8.
Steps 0 to 3 built the ear and the reading: every PSK31 station in the passband is a row, and every
row says who is speaking, to whom, and whether Hamlet is sure.

**B, in the arbiter's words.** Step 4 gives the mode its voice. Its eight must-pass criteria are:

1. A modulator sends §R2's four macros, and each comes back identical through Hamlet's own
   demodulator.
2. The signal is measured as BPSK at 31.25 baud with a raised-cosine envelope, under 100 Hz wide
   at -30 dB.
3. A CQ press makes a receipt with no station facts and no Log. An answer retires the receipt and
   opens a conversation card; two answers make two cards.
4. A macro is offered for one click only when the parser is certain whose turn it is.
5. A turn indicator replaces the slot clock on the PSK31 card and says *unknown* when it does not
   know.
6. Drive level and RF power are shown on the panel, with the defaults §R4 sets.
7. The chain to `Played` changes only as §R10 allows, and nothing keys at the bench.
8. A send with no slot that is longer than the cap is refused before it arms, and the refusal is a
   record.

### Why the door stays shut this unit - the arbiter's decision

**Four of the eight need no press** - numbers 1, 2, 7 and 8 above - and this unit takes those four.

**The other four are what make a press safe**: 3, 4, 5 and 6. §R4 exists because the drive level
*"is the difference between a clean 31 Hz signal and splatter across the whole watering hole"*.
FACT-005 is the only drive reading this project has: **FT8 at 0.25 put the IC-7300's ALC at -2.0 to
-1.5, inside the red zone.** FT8 has a constant envelope. A BPSK signal with a raised-cosine envelope
driven into ALC is the splatter §R4 is written against. *That last sentence is the arbiter's
reasoning, not a measurement.*

**So the door opens in the unit that builds §R4's defaults, not tonight.** `CanTransmitIn`
(`MainWindowViewModel.cs:1951-1954`) keeps refusing PSK31, and `ThePsk31TabIsInertTests` stays green
and unedited.

**The loop test.** It was run on this unit's `APPROACH` line and found it in no entry. **No
approach on step 4 has been tried.** Unit 316's section 4 is logged below as asks 25 to 30, not
chased. None of it is in the way of a criterion this unit aims at.

---

## 2. Verify this instruction against the tree

**Nothing here describes the tree.** The arbiter read the lines below at `a36ce1e`, and they are
still claims.

- **Check every one.**
- **Report every mismatch in section 1**, even where the work succeeded anyway.
- **Do not repair this instruction.**
- **Do not stop over a mismatch** unless a task becomes impossible. Then say which task and why.

**The chain:**
- **`OperatorSend`** is at `Ft8TransmitSequence.cs:118-151`. It holds `Transmission`,
  `FrequencyHz`, `LicenseClass`, `GuardEnabled`, `SlotStartUtc` and `StartSecondsIntoSlot`, plus an
  `init` `Grid` that defaults to `SlotGrid.Ft8`.
- **`RunAsync`** is at `:307-456`, in this order:
  - the gate at `:330`;
  - `Sendable` at `:341`, whose body is `:595-652`;
  - the one `CivConstants.PttOn` write at `:363`;
  - the `finally` at `:415-448`, which unkeys or fires `TransmitAbort.Fire` at `:446`;
  - `Recorded` at `:487-517`.
- **`Ft8Transmission`** is at `Ft8Composer.cs:95-102`. It holds `Text`, `ReadsBackAs`,
  `Ft8MessageType Type`, `Samples`, `SampleRate`, `BaseFrequencyHz` and `CarriesHashedCallsign`.
- **`Ft8ArmedSend`:**
  - `Arm` at `Ft8ArmedSend.cs:255-263` holds one field and replaces rather than adds;
  - `Cancel` is at `:267`;
  - `StopNow` at `:339-353` runs un-arm, then abort, then audio;
  - `AtBoundaryAsync` at `:426-491` keeps a send that is `NotDue`, discards one that is `TooLate`,
    and sets `_transmitting` under `_gate` before the await (`:457-458`).
- **`TransmitRecord`** is at `TransmitRecord.cs:55-67`. Its event name is `ft8_transmission`
  (`:70`), and `ToBag` (`:93-108`) writes `slotStartUtc` and `startSecondsIntoSlot`.
- **`SendMessage`:**
  - the mode gate is at `MainWindowViewModel.cs:12561`;
  - `Ft8ReadBack.Check` is at `:12626`;
  - `BookTheSend` is at `:12655`;
  - the one `Arm` call is at `:12683-12697`;
  - `StartSecondsIntoSlot = 0.5` is at `:12718`;
  - `DriveTheArmedSend` is at `:12769`, called from `:10560`.

**The tests that read the chain's own source:**
- `TheUnkeyHappensWhateverGoesWrongTests.NothingInTheSequenceCanStartATransmissionOnItsOwn` (`:356`)
- `OneClickSendsExactlyOneMessageTests.NothingInTheArmedSendCanStartATransmissionOnItsOwn` (`:275`)
- `TheOperatorsStopFiresFromEveryStateTests.NothingOnTheStopPathWaitsForAnything` (`:462`)
- `WhereTheTransmissionStartsAndWhatTheRecordSaysTests.ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt` (`:336`)
- `TheFitGuardAsksAboutTheGridTheSendIsOnTests.ArmHasExactlyOneCallerInSrc` (`:238`)

**PSK31:**
- `Varicode.Encode` (`Varicode.cs:86`) writes each code followed by `00`.
- `Psk31Demodulator(int sampleRate, double offsetHz)` is at `:141`, and `Add(ReadOnlySpan<float>)`,
  which returns a string, is at `:189`.
- `Psk31CarrierSearch` uses a `RealFft` (`:208`, `:254`).

**Settings.** `OperatorProfile` has `Callsign` (`:44`), `OperatorName` (`:77`), `Location` (`:80`)
and `GridSquare` (`:92`).

**Radio writes.** `CivWrites.RfPower` is at `CivWrites.cs:271`.

**Version.** `1.13.4` in `Directory.Build.props:599`.

**`docs\carry-forward-tests.txt`** has 22 app type names, plus
`BindingHealthTests.TheMainWindowBindsWithoutOneComplaint`, and 10 engine type names.

**The chain-guarding list** is every test class that names `Ft8TransmitSequence`,
`Ft8ArmedSend` or `OperatorSend`, as `git grep` found them.
- **Engine** (`tests\Hamlet.RadioEngine.Tests`):
  - `Transmit\`: `HamletDoesNotKeyWhatNobodyCanReadTests`, `OneClickSendsExactlyOneMessageTests`,
    `OneFt4ClickOneFt4TransmissionThroughTheSameAbortTests`,
    `TheFitGuardAsksAboutTheGridTheSendIsOnTests`, `TheLicenceGateIsInsideThePathTests`,
    `TheOperatorsStopFiresFromEveryStateTests`, `TheStopStopsTheAudioTooTests`,
    `TheTelemetryLineSaysWhatActuallyWentOutTests`, `TheUnkeyHappensWhateverGoesWrongTests`,
    `WhereTheTransmissionStartsAndWhatTheRecordSaysTests`.
  - `Audio\`: `TheLoopbackProvesTheWholeChainTests`, `TheRingCountsDownSevenAndAHalfTests`,
    `TheSinkPlaysToANamedEndpointTests`, `TheStopStopsARealEndpointTests`,
    `Unit294AnFt4RowCarriesAMeasuredReportTests`.
- **App** (`tests\Hamlet.App.Tests`): `Unit305StageTests`, `OneClickArmsExactlyOneMessageTests`,
  `TheApplicationSendsAtTheLevelTheOperatorSetTests`, `TheLicenceGateHoldsFromTheClickTests`,
  `TheReadoutSaysWhatTheCardWasHandedTests`, `TheSendPathComposesAtTheEndpointsRateTests`,
  `TheSendPathReachesARealRadioTests`, `TheWholeChainRunsFromOneRightClickTests`,
  `TheWholeFt4ChainRunsFromOneRightClickTests`, `TheMenuIsUnderTheMouseTests`,
  `TheOperatorCanStopItTests`.

**Five mismatches the arbiter already found. Report them; do not resolve them.**

1. **The reload's `RULES_AT` check** says `CLAUDE.md` section 1 holds `CPS-DEC-0161`. The
   arbiter's grep finds no `CPS-DEC` in `CLAUDE.md`, and finds `HM-DEC-161` at line 360. Unit 316
   found the same. It is the reload's reading, not the tree. Do not touch `tools\`.
2. **`PHASE_PLAN.md` §2 still says** *"Nothing in this phase changes what keys the transmitter;
   step 4 adds a second audio generator behind the same proved path."* Yet §8's revision record
   says §2's premise was corrected. **§R10's closing paragraph amends §2, and §R10 wins.** Do not
   edit the plan.
3. **`PHASE_PLAN.md` §2's nudge line** (*"sticky per station with a cap of two"*) is stale. Unit 316
   found Tim's 2026-09-11 withdrawal recorded in the tree. It is not this unit's surface.
4. **`output.md` is absent at the root.** `a36ce1e` removed it, and unit 316's report is at
   `2983c02:output.md`. Write a fresh one.
5. **`PHASE_STATUS.md` says `WORK_INSTRUCTION: 316`.** Work instruction 317 was a stop and ran no
   unit, so this is 318. Change only the line the launcher's prompt assigns you.

**Expected at the start, and not yours:** `.run-unit\*` and `PHASE_STATUS.md` modified by the
launcher.

**Expected failures:**
- **The known reds** in section 7. None of them is chased.
- **`TheFitGuardAsksAboutTheGridTheSendIsOnTests`** is on the known-red list as a whole class.
  **In task 1, run its five methods one at a time by exact name and record which are red.** After
  task 3, the same ones and no more.
- **`TheReadinessHoverTests.NoRowCarriesABearingInDegrees`** failed three times in unit 316 and
  passed twice. It depends on run order and was never measured on the tree before unit 316 (ask
  25). If it fails, record it and do not chase it.
- **`TheSinkPlaysToANamedEndpointTests`, `TheStopStopsARealEndpointTests` and
  `TheSendPathReachesARealRadioTests`** name a real endpoint or radio, and this machine has neither
  (FACT-006). The arbiter has not read whether they skip. Record their state in task 1; afterwards
  they must be no worse.

**The tool facts.**
- **Quoting.** The shell breaks on an apostrophe inside a quoted heredoc and collapses a doubled
  backslash. Write *do not*, write single backslashes, and check what landed on disk.
- **Refusals already measured.** Units 315 and 316 saw the shell refuse all of these with "requires
  approval":
  - `rm`, `git rm` and `mkdir`;
  - output redirects;
  - `tools\arbiter\validate-output.bat`;
  - `git worktree add`.
- **If you meet one**, report it once in section 4 and do not retry it in another form.

---

## 3. Rulings in force

**Do not re-argue any of these.**

**HM-DEC-155, Tim, 2026-09-05 - the two rules that killed sessions.**
1. **A unit runs no test suite.** It runs only its own test names, filtered by exact name,
   foregrounded, with a stated timeout:
   `timeout 480 dotnet test <project> --filter "FullyQualifiedName~TypeName.MethodName"`.
   It runs `docs\carry-forward-tests.txt` and the chain-guarding list the same way.
2. **Never background a command and poll it.** The watchdog fires at **twelve minutes with no
   status write.** A modulator for 23 seconds of audio runs in well under a second. One that takes
   longer is a finding about the code.

**`PHASE_PLAN.md` at the root - read it in full.** The parts this unit stands on, transcribed:

**§R10 - Tim, 2026-09-11: the transmit chain may carry a send that has no slot.**
> *Ruled on the arbiter's question of unit 317, option A. `OperatorSend` carries either a slot, as
> today, or* now. *A send with no slot is not held to a slot fit; it is held to **a stated maximum
> length** so a continuous carrier cannot run on - the unit states the number and why, and it is
> not more than thirty seconds, which is the Report macro with idle either side. The operator's
> click fires a no-slot send at once rather than at a boundary. **The gate, the single `PttOn` use
> site, the `finally` that unkeys or aborts, and `StopNow` stay one code path shared by all three
> modes.** FT8 and FT4 must stay byte-identical, proved by the tests that guard them today, run
> filtered. The transmission record says which mode went out and carries no slot where there was
> none.*
>
> **Rejected:** *a second sequence (a second `PttOn` site), an invented PSK31 slot (a slot that
> does not exist in the record that is evidence), shortened macros (§R2 overruled from underneath),
> and splitting a macro across slots (one click, several keyings).*
>
> *This ruling amends §2 of this plan, which said nothing in the phase changes what keys the
> transmitter. It does now, in the one way ruled above and no other. §6's `MOVE: stop` on the
> transmit chain stays in force for anything beyond it.*

**§R2 - The macro exchange Hamlet sends.** *The standard minimum, and nothing chatty:*
```
CQ:      CQ CQ CQ de KC3QIS KC3QIS KC3QIS pse K
Answer:  <HIS> de KC3QIS KC3QIS K
Report:  <HIS> de KC3QIS  RST 599 599  Name Tim Tim  QTH Trafford PA  Grid FN00 FN00  BTU <HIS> de KC3QIS K
Confirm: <HIS> de KC3QIS  R R  TNX for the QSO  73 73  <HIS> de KC3QIS SK
```
> *Name, QTH and grid come from Settings; nothing is typed at the moment of sending. Free typing is
> not in this phase.* **Rejected:** *a keyboard, because it makes the exchange something other than
> FT8's and reopens every question about what a card asserts.*

**§R1 - How much Hamlet asserts about an exchange it can only partly read.**
> *Strict on anything that drives a transmission; permissive on what the card displays; every
> guessed state marked as a guess. A macro is offered for one click only when the parser is certain
> whose turn it is. The card may show a state inferred from turnover words, elapsed silence and
> whose callsign appeared last, and when it does the state word is visibly a guess (§0.6: not by
> colour alone).*
>
> **Rejected:** *strict everywhere, because the card would say unknown through most real QSOs and
> read as broken; permissive everywhere, because a wrong guess that sends a macro into another
> operator's turn is a transmission Tim did not choose.*

**§R4 - Drive and power.** *Not this unit's to build; it is why the door stays shut.*
> *The step-4 unit sets the IC-7300's data-mode input level so that the ALC meter shows no
> deflection on the macro tone, and defaults the RF power for this mode to half the radio's rated
> output because the carrier is continuous. Both are settings Tim can change; both are shown on the
> mode's panel so he can see what Hamlet chose; neither is silent. The unit cites the page of the
> IC-7300 manual it read for the input-level setting rather than naming it here from memory.*
>
> **Rejected:** *full power, because splatter into 14.070 is what a beginner is most afraid of
> doing and this application exists to stop that.*

**§R5 - The reference implementation.**
> *`fldigi`, GPL-3, cloned outside the tree at `C:\Source\fldigi`, pinned (unit 314 recorded
> `61b97f4133c488063f3de1795c894d22d5032e8a` in `docs\psk31-reference.md`), never committed, never
> ported wholesale. What is read from it: the varicode table, the raised-cosine shaping and the
> demodulator structure. What is written for Hamlet is Hamlet's.*

**§3.1** - *There is no slot clock. On a PSK31 conversation card its place is taken by whose turn it
is: your turn, his turn, he is still sending (carrier present), unknown. Carrier-present is a fact
from the demodulator; the rest is from the parser and marked as §R1 says.*

**§3.3** - *It is a continuous carrier at full duty. On a 100 W IC-7300 this is a heat and linearity
question, and the drive level is the difference between a clean 31 Hz signal and splatter across
the whole watering hole.*

**§6 Branching, the calls already made:**
- **A must-pass ceiling missed by a little** is `partial`. The assertion is never deleted and the
  ceiling never loosened.
- **Anything touching the transmit chain, the keying path or `Played` beyond what §R10 allows** is
  `MOVE: stop`, because risk posture is the owner's. *What §R10 allows is ruled and is not a stop.*
- **A package** is `MOVE: stop`.
- **Reading fldigi tempts a port.** Read the structure and write Hamlet's own. If you cannot proceed
  without copying, stop and say what you would copy.
- **A defect in steps 0 to 3** found now is fixed in passing as the first task, and said so.
- **The parser cannot tell.** A high unknown rate is a number to report, not a reason to guess. If
  the unknown rate makes step 4 unusable, that is Tim's, with the number.

**§0.2** - one operator action, one transmission. As the chain's own remarks cite it, **the licence
gate is inside the path and not bypassable from any send path**, and the operator's stop *"cannot
be disabled, deferred, or made conditional"* (`Ft8ArmedSend.cs:289-291`).

**§0.1** - the engine is never told that tabs or settings exist. It is handed strings and numbers.
**§0.4** - a package is Tim's. **No DSP, FFT or audio package.** The tree has `RealFft`.

**HM-DEC-018** - quoted from `TransmitRecord.cs:10-15`: *"Telemetry never carries a callsign,
decoded message content, or anything identifying a person or a contact... 'the length, the count,
the duration, the frequency and the mode make the transmission fully diagnosable and identify
nobody'."*

**FACT-004** - a dev-machine result is an indication, never a finding. **FACT-006** - this machine
has no radio. **FACT-005** - *"Transmit drive: 25 per cent... ALC: -2.0 to -1.5, inside the red
zone... It is a reading of one radio's ALC on one band and it is not a specification."*

**Tim, 2026-09-06 - the dummy load is withdrawn in full.** There is no compensating control, so
there is no safe on-air test. **Every proof in this unit is at the bench, against a fake port and a
fake sink.**

---

## 4. Status cadence

Write `PROJECT_STATUS.md` per `CLAUDE.md` §13 **after every task, and at least every ten minutes.**
Write the status **before** starting a test run.

---

## 5. The tasks

**There are four.** Each names the test to watch failing first. **The unit's drop candidate is
task 4, whole.**

### Task 1 - entry, the trace, and the before-counts. No production change except the version.

**1a. Entry, run rather than read.** Step 4's entry is *the parser is certain whose turn it is on
the clean corpus entries*. Run `ThePsk31ExchangeParserTests` and `ThePsk31MessageSplitTests` by
exact name. **If either is red, step 4 has no ground. Report it and stop after this task.**

**1b. The trace. Say what you find rather than confirming this list.** Give file and line for each.
- **One FT8 CQ press, end to end:** `SendMessage`, compose, read-back, `BookTheSend`, `Arm`,
  `DriveTheArmedSend`, `AtBoundaryAsync`, `RunAsync`, gate, `Sendable`, `PttOn`, sink, `finally`,
  `Recorded`. **Name every FT8-only assumption a send with no slot would meet on the way:**
  `Ft8Transmission.Type`, `ReadsBackAs`, `CarriesHashedCallsign`, `SlotStartUtc`,
  `StartSecondsIntoSlot`, `Grid`, and the record's fields.
- **The five source-reading tests in section 2, and exactly what each counts or forbids.** A
  design that would trip one should be seen before it is built.
- **Whether any test asserts the exact field set** of the transmission record or its bag.
- **What the tree already has for making PSK31 audio:**
  - how the PSK31 fixtures were generated, and whether the generator is in the tree;
  - how long `Psk31Demodulator` takes to lock and open its squelch, and what it needs before and
    after the text to emit the first and last character;
  - the fixtures' sample rate;
  - the rate the transmit endpoint declares (`_transmitSampleRate`).

  **If a generator exists and is merely unwired, say so. That is the fix being smaller than the
  build.**
- **For the next unit, building nothing.** This is step 4's must-pass on drive and power:
  - does `CivWrites` have a data-mode input level (USB MOD level) write beside `RfPower`?
  - is an IC-7300 manual readable anywhere on this machine?

  §R4 requires a page citation, so say what exists.

**1c. The before-counts.**
- **Run both lists, filtered, in both projects:** `docs\carry-forward-tests.txt` and the
  chain-guarding list in section 2.
- **Record** every count and every red by name.
- **Run `TheFitGuardAsksAboutTheGridTheSendIsOnTests` method by method** (section 2).
- **Patch-bump the version, 1.13.4 -> 1.13.5.**

**Test watched failing first:** none.
**Drop candidate:** none. **Not droppable.**

---

### Task 2 - the modulator and the four macros, proved by loopback

**The work is two engine types in `src\Hamlet.RadioEngine\Psk31\`, named by the unit** (§0.1).

- **The modulator.**
  - **In:** text, sample rate, audio offset in Hz, and the peak amplitude asked for.
  - **Out:** samples in -1 to +1.
  - **Signal:** BPSK at 31.25 baud with a raised-cosine envelope, using **the bit-to-phase
    convention the tree's demodulator already reads** - taken from the demodulator and the pinned
    reference, not from memory.
  - **Idle:** before and after the text. State each length and why: lock, squelch, and flushing
    the last character.
- **The macros.**
  - **§R2's four texts**, filled from strings handed in: the operator's callsign, the other
    station's callsign, the name, the QTH and the grid.
  - **§R2's spacing exactly**, double spaces included.
  - **The grid is four characters, as §R2 writes it.** That is the arbiter's reading of §R2;
    Settings holds six in the tests (`FN00DJ`).
  - **No `KC3QIS` constant** in production.

**Test watched failing first:** `ThePsk31ModulatorTests`, engine. Watch it fail on a stub, then
green:

1. **Loopback.** Each of the four macros goes in with `W1AW` as the other station and test values
   for the rest. It is modulated at 48 000 Hz at 1000 Hz offset and fed to a fresh
   `Psk31Demodulator(48000, 1000)`. It **comes back identical**: exact string equality, both
   strings printed. Also check at 1500 Hz, and at the fixtures' rate if that is not 48 000.
   *must-pass*
2. **Bandwidth.** Measure the longest macro with the tree's `RealFft` and no package, stating the
   window, size and averaging. Print the width at -6, -20 and -30 dB relative to the spectral peak.
   **The -30 dB width is under 100 Hz.** *must-pass*
3. **31.25 baud and the envelope, measured.** Take the symbol length from the waveform - reversal
   spacing or envelope minima - and print it beside `sampleRate / 31.25`. The envelope reaches its
   minimum at each reversal and is flat between. *must-pass*
4. **The peak never exceeds the peak asked for.** The drive level is the caller's, and this type
   invents none. *must-pass*
5. **A printed table.** For each macro: characters, varicode bits, seconds of text, seconds with
   idle, and instruction 317's arithmetic beside them. *must-pass* - task 3's cap is chosen from
   it.
6. **The Report macro with a compound callsign** (e.g. `VP2V/W1AW`) as the other station: its
   seconds with idle, printed against the cap task 3 states. *nice-to-pass*

**Drop candidate:** assertion 6 only.

---

### Task 3 - the one sequence carries a send with no slot (§R10)

**3a. The pin, first, before any production change.** Write `TheFt8AndFt4SendsAreByteIdenticalTests`
(engine; the name is the unit's to adjust).
- **What it runs:** one FT8 and one FT4 `OperatorSend`, built exactly as the app builds them today,
  through `Ft8ArmedSend` and `RunAsync`, with a recording fake port and fake sink.
- **What it pins:**
  - the port's bytes, in order;
  - a SHA-256 of the samples handed to the sink;
  - the `TransmitRun`'s fields;
  - the telemetry bag, key by key.
- **Run it green on the unchanged chain and commit it on its own.** This is how *byte-identical* is
  measured rather than asserted.

**3b. The change.**

**What §R10 fixes, and is not the unit's to design:**
- `OperatorSend` carries a slot, or *now*.
- A send with no slot is held to **a stated cap of at most thirty seconds**, with the number and why.
- It fires **at once from the operator's action, never at a boundary**.
- **The gate, the single `PttOn` use site, the `finally` and `StopNow` stay one path for all three
  modes.**
- The record names the mode and **carries no slot where there was none**.
- The record holds no text and no callsign (HM-DEC-018).

**What is the unit's to design, and to state in the types' remarks and in section 1:**
- **How `OperatorSend` carries *now*.** Not a sentinel `DateTime` that reads as a slot.
- **How PSK31 audio travels.** No FT8 message type is invented for it.
- **How the armed send fires *now*.** Under `_gate`, with `_transmitting` set before the await,
  consumed exactly once.
- **Where the over-cap refusal happens.** It must come before anything is armed, and it must write a
  record.

**Test watched failing first:** `TheUnslottedSendTests`, engine, with a fake port and a fake sink.
Watch it fail, then green:

1. **A send with no slot, under the cap, fired *now*:**
   - the gate is asked first;
   - exactly one `PttOn` frame, then one `PttOff`;
   - the sink receives every sample;
   - the outcome is `Played`;
   - the record names the mode and carries no slot start and no offset.

   *must-pass*
2. **Over the cap, refused before it arms:**
   - `Armed` is null afterwards;
   - the port and the sink are untouched;
   - a Warn record gives the reason and the cap's number.

   Handed straight to `RunAsync`, the same send is refused too, as the backstop. *must-pass*
3. **The licence gate holds for it.** With the guard off, with an unknown class, or outside
   privileges: refused, nothing keyed. *must-pass*
4. **The operator's stop reaches it.** Call `StopNow` while it plays, with a sink that plays until
   cancelled. The abort fires, the audio is told to stop, the outcome is `Cancelled` and
   `RadioIsInReceive` is true. *must-pass*
5. **Two paths kept apart.** A boundary never runs a send with no slot, and *now* never runs a
   slotted send. *must-pass*
6. **3a's pin is green and unedited.** *must-pass*
7. **The source-reading tests from section 2 are green and unedited.**
   - `PttOn` has exactly one use site under `src\`, counted and printed.
   - `ArmHasExactlyOneCallerInSrc` gives the same result as in task 1.

   *must-pass*
8. **The chain-guarding list gives the same pass counts as task 1, and no test in it is edited.**
   *must-pass*

**Drop candidate:** none. It carries two of the step's must-pass criteria and the pin.

**If the design cannot meet assertions 6 to 8** without editing a guarding test or adding a second
`PttOn` site, **stop this task.** Report exactly what would have to change, and mark the step
`partial`. That is §6's line on the chain: it is Tim's question, not a design choice.

---

### Task 4 - whose turn it is, in the engine

**Drop candidate: this whole task.** It builds the engine behind §3.1's turn indicator and completes
no criterion by itself. The card is the next unit's.

**An engine type, named by the unit.**
- **In:**
  - the messages `Psk31MessageSplitter` yields on one channel, with their parses;
  - whether characters have arrived since the last message, which is the caller's carrier-present
    fact;
  - the operator's callsign.
- **Out:** one of *your turn*, *his turn*, *he is still sending* or *unknown*, and **whether it is
  certain**.
- **Rule:** stated in one sentence in the type's remarks.
- **Vocabulary:** §R3's, as it stands. A roger is still text (ask 28).

**Test watched failing first:** `ThePsk31TurnTests`, engine. Join each transcript's lines into one
stream, as unit 316 did, and feed it one character at a time. Watch it fail, then green:

1. **Certain *your turn*.** A certain message from another station, addressed to the operator,
   that hands over.
2. **Certain *his turn*.** A message the operator spoke that hands over.
3. **Still sending.** Characters are arriving after a turnover and no new turnover has come yet.
4. **An uncertain message is never a certain *your turn*.** That is `05-garbled` line 3, and the
   merged lines 4 and 5.
5. **`04-not-for-me` never yields *your turn*.**
6. ***Unknown* before any message.**
7. **The number §6 asks about, printed whatever it is.** Per transcript: messages addressed to the
   operator, and on how many of them *your turn* is certain.
8. **The within-one-character count.** Print how many characters after the other station's final
   turnover word the state changed. It should be at most one, per the split rule's edge.
   *nice-to-pass*

**Drop candidate:** the whole task.

---

## 6. Parked - do not touch, do not raise

- **The PSK31 send door, and everything a press reaches.** That means `CanTransmitIn`, a CQ press
  under PSK31, the PSK31 receipt, conversation cards, and answering a PSK31 row. **All of it opens
  in the next unit, together with §R4** (section 1). `ThePsk31TabIsInertTests` stays green and
  unedited.
- **§R4 drive level, RF power, and any new CI-V write.** Task 1b traces them; nothing is built.
- **§R6's clear spot for the transmit offset.** It is needed only once the door opens.
- **The §R1 certainty gate on offering a macro, and the turn indicator on the card.** Both need the
  card.
- **`SlotClock` under PSK31.**
- **Step 5.** RST in the log, and the achievements.
- **Asking the radio whether it keyed** (ask 1). Carry it; do not build it.
- **The card-rebuild root.**
- **fldigi.** Read the shaping structure at the pinned commit if you need it. Copy nothing.
- **Any package.**

**The asks queue.** Carried per HM-DEC-139, **verbatim where unresolved**, and **act on none of
them.**

1. **Does the transmission record ask the radio whether it keyed?** Unit 303's proposal,
   still Tim's. PSK31's continuous carrier makes it sharper.
2. **Nothing in this repository can look at a picture.** `Avalonia.Headless.Skia` is
   Tim's to add (§0.4).
3. **Three inherited reds, never chased** - two in `TheAchievementsScreenTests`, one in
   `TheFitGuardAsksAboutTheGridTheSendIsOnTests` (engine).
4. **`Ft8ContactCard.Closing`** is uncalled and left standing.
5. **`Ft8GlobePlot`'s unused framing constants.** Report; leave standing.
6. **The licence of `assets/world-flat-relief.png` is unknown.** Raise; do not resolve.
7. **The FT4 phase's run files.** Unit 315 recovered the plan and status into
   `docs\phase-ft4-run\`. The outcome file waits on item 19.
8. **The door sentence is a placeholder.** Carry.
9. **Acknowledgement indicators.** Named by Tim, not yet defined. Step 4's turn indicator
   may be what he meant; do not assume it is.
10. **Card ordering under scroll, and its root** - `DigitalCards.Clear()` then new cards
    every slot, per-card state lost. Not this phase's. Carry.
11. **The 2 px map outline** and **the 2.0x popup zoom cap** are sessions' numbers.
12. **Version numbering** - x.y.0 or x.y.1 for a phase's first unit under HM-DEC-150.
13. **The squelch threshold 0.90** is unit 314's number, `Psk31Demodulator.SquelchQuality`.
14. **Real off-air PSK31 audio** - only Tim can record it. Two or three minutes on 14.070
    with a few signals on it, any recorder, any rate, WAV, into
    `assets\fixtures\captured\`. Every number in this phase is synthetic until then.
15. **One conversation card is taller than the panel** (293 px in 220 px). Not broken.
16. *(Unit 315 closed the fixed 1000 Hz listening offset by its instruction's own
    condition. Kept here as a closed number so the list does not renumber.)*
17. **Hand wanted: run `tools\arbiter\validate-output.bat output.md`** - unit 315's
    shell refused it.
18. **Ruling wanted: record the 1500 Hz station's text in `manifest.json`, or accept unit
    315's derived reference.**
19. **Hand wanted:** `git show eb28430:PHASE_OUTCOME.md > docs\phase-ft4-run\PHASE_OUTCOME.md`,
    expected 168242 bytes, md5 `b60010069f7feea96e9d4be4e7e8c0f1`, then commit. *(Per its stat,
    `a36ce1e` committed a six-line file at that path. The arbiter did not check its content, and
    the 168242-byte file is not what landed.)*
20. **Hand wanted: `git rm src\Hamlet.RadioEngine\Psk31\Psk31Listening.cs`** - comment-only,
    builds.
21. **Ruling wanted: the carrier search's floor should be local to the signals before
    step 6** - at 48 kHz the median bin is below the radio's passband.
22. **Ruling wanted: is HM-DEC-161 the right id** for the phase-setting ruling?
23. **Ruling wanted: accept about a second of delay on PSK31 text, or reopen the step-1
    squelch.**
24. **Ruling wanted: the `snr` column's hover names FT8 and FT4 precision and not PSK31's.**
25. **Hand wanted (unit 316): say whether `TheReadinessHoverTests.NoRowCarriesABearingInDegrees`
    predates unit 316.** Run it alone at `eb67036`:
    `timeout 480 dotnet test tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj --filter "FullyQualifiedName~TheReadinessHoverTests.NoRowCarriesABearingInDegrees"`.
26. **Ruling wanted (unit 316): accept the replaced assertion in `ThePsk31HearsEveryoneTests`.**
    *"No row is ever on his side"* became *"a row is on his side only while its latest message is
    addressed to him"*.
27. **Ruling wanted (unit 316): the words `guess` and `unknown`** on a PSK31 row are the
    session's wording, not a ruling.
28. **Ruling wanted (unit 316): should §R3 recognise a roger (`R`, `RR`, `QSL`) before step 4?**
    Without one the corpus's `ack` kind is never produced.
29. **Ruling wanted (unit 316): accept the split rule's three edges.** A bare `73` does not end a
    message. A damaged sign does not split. A final `K` counts when the next character arrives.
30. **Ruling wanted, low (unit 316): the callsign shape is held in four places.** Consolidate them
    later, or leave them.
31. **Hand wanted (the arbiter, authoring 318):** `tools\arbiter\outcome-read.bat --approach`
    breaks when the approach contains an apostrophe. PowerShell reports *"Unexpected token"*, and
    the script still prints `Read complete.` and exits 0. **So a loop test on such a line reads as
    run and was not.** Not a unit's to touch.
32. **Hand wanted, for step 4's drive-and-power must-pass: put an IC-7300 manual where a session
    can read it, and name its path.** §R4 requires the unit to *"cite the page of the IC-7300
    manual it read"*, and the arbiter's search of the tracked tree found none. Task 1b reports
    whether one exists anywhere on this machine.

---

## 7. What not to do

- **No second `PttOn` site, no second sequence, no invented PSK31 slot, no shortened macro, no
  macro split across sends.** Each of these is §R10's rejected option, and each takes apart the
  one path that makes the abort trustworthy.
- **Do not edit any test on the chain-guarding list or `docs\carry-forward-tests.txt` to make it
  pass.** §R10's *byte-identical* is proved by those tests unedited. An edited guard proves
  nothing.
- **Do not open the PSK31 send door.** It protects 14.070 from a PSK31 carrier driven before §R4's
  defaults exist (FACT-005).
- **Nothing in the sequence or the armed send reads a clock, starts a timer or fires on its own.**
  A path that can wait for a moment can arrive at one by itself, and that is §0.2.
- **No new test opens a real COM port or a real sound endpoint.** There is no radio here
  (FACT-006) and no dummy load.
- **Hard-bench rules:**
  - no unfiltered `dotnet test`, and never background and poll;
  - do not edit `PHASE_PLAN.md`, `PHASE_STATUS.md` or `PHASE_OUTCOME.md`, except the line the
    launcher's prompt assigns, which you name in section 1;
  - do not touch `tools\`;
  - no `KC3QIS` in production code;
  - do not invent a ruling id;
  - do not call a computed appearance seen;
  - do not repair this instruction.
- **Do not chase these known reds:**
  - `CwAdjudicationTests.ASpeedChangeInRealisticAudio`;
  - the 51 CW cases in `docs\unit239-failing-set.txt`;
  - the `Ft8Sharp.Deep.Tests` whole-type-list tripwire;
  - `HM-OPEN-088`'s ten;
  - the two in `TheAchievementsScreenTests`;
  - `TheFitGuardAsksAboutTheGridTheSendIsOnTests` (engine).

---

## 8. Committing and pushing

- **Commit per task and push**, on `main`. The version bump goes in task 1.
- **Task 3a's pin is its own commit, pushed before 3b changes a line of the chain.**
- **Add this unit's new test classes to `docs\carry-forward-tests.txt`** in the task that makes
  them green.
- **The report and the final status go in one more commit**, pushed before the session stops.
- **Leave nothing of yours uncommitted.** Say in section 1 what was left and whose it is.
- **A refused push is reported as refused**, with the reason.

---

## 9. Reporting

**Write `output.md` at the repository root.**
- **Canonical headings:** `## 1. What Claude did`, `## 2. What the owner should expect`,
  `## 3. What you should see`, `## 4. What's blocking us`.
- **Every exit writes it** - complete, blocked, failed or stopped early.

**The ordering block first. `validate-output.bat` refuses a report without it:**

```
READ IN THIS ORDER.

A. The phase goal - Hamlet works PSK31 the way it works FT8. Steps 0 to 3 done,
   4 <state after this unit>, 5 and 6 not started.
B. Step 4 and its eight must-pass, each met or not met, with its number:
   - loopback: which of the four macros came back identical;
   - bandwidth: width in Hz at -30 dB, and whether it is under 100;
   - 31.25 baud and the envelope: the measured symbol length;
   - the chain changed only as R10 allows: the PttOn use-site count, the pin identical
     or not, the guarding list before and after, and the cap in seconds;
   - over the cap refused before it arms, with a record: yes or no;
   - receipt and cards, the certainty gate, the turn indicator on the card, and drive
     and power on the panel: not aimed at by this unit, so not met.
   The nice-to-pass: task 4's character count, or dropped.
C. The report last. Section 4 raises N new items on top of a carried queue of
   thirty-two; <say whether any is in the way of a criterion in B>.
```

**Then the header:**

```
UNIT:       318 - <complete|stopped> at task N of 4, <which dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no, and which step and criteria>
NUMBER:     macros modulated that loop back identical 0 -> n of 4; occupied width at
            -30 dB <Hz>; sends with no slot the chain accepts: none -> up to <cap> s;
            version 1.13.4 -> 1.13.5
DRIFT:      <n> consecutive units without advance  (was 0)
```

**Section 1 must carry all of these:**
- **The macro table** from task 2 assertion 5, with the loopback result on each row.
- **The bandwidth numbers**, and how they were measured.
- **The cap**, its number and why.
- **The design, in four sentences:** how *now* travels; where the over-cap refusal happens and what
  record it writes; how the record names the mode and carries no slot; how `StopNow` reaches a send
  with no slot.
- **The pin's values.**
- **The before and after counts** for both lists, with every red named.
- **Task 1b's answers**, including what exists for §R4.

**Section 3 must lead with this: nothing Tim can press has changed tonight.**
- **Pressing CQ under PSK31 still refuses, in words.** Say why in one sentence: the door opens with
  §R4's drive and power, because a PSK31 carrier driven into ALC splatters across 14.070.
- **Then what now exists, at the bench only:**
  - a PSK31 signal Hamlet makes, which its own ear reads back letter for letter;
  - a transmit path that can carry a PSK31 over with no slot, and refuses one that would run on;
  - FT8 and FT4 going out exactly as before;
  - if task 4 ran, whose turn it is, read from the text.
- **Say each of these once.** Every number here is from the bench and synthetic. Nothing keyed a
  radio. Every appearance claim is computed, not seen.

**Section 4 carries asks 1 to 32 verbatim where unresolved**, then this unit's own.

**Write `output.md`, then stop.** Do not start the next unit.

---

```
ARBITER-DECISION
STEP: 4
APPROACH: build the PSK31 modulator proved by loopback through the PSK31 demodulator, carry a capped unslotted send through the one transmit sequence under R10, and read whose turn it is in the engine, with the PSK31 send door kept shut
MOVE: continue
WHY: Tim answered the stop of instruction 317 with R10, so step 4 is open and is next in the one pipeline, and the loop test finds nothing tried on it. The four must-pass that need no press go first; the send door waits for the R4 drive and power defaults, because a PSK31 carrier at the only drive level ever measured (FACT-005, ALC in the red zone) is the splatter the plan names.
STATE: not started
DECIDED: four on the arbiter authority. The PSK31 send door stays shut this unit and opens in the unit that builds the R4 defaults. The R10 byte-identical requirement is measured by a pin written and run green on the unchanged chain before the chain changes. The R2 grid is four characters, as R2 writes it. Task 4, the turn reading, is the drop candidate because it completes no criterion by itself.
LICENCE: PHASE_PLAN.md step 4 entry and exit criteria, R10 (Tim, 2026-09-11), R1, R2, R4, R5, sections 3.1 and 3.3 and the section 6 branching; SHACK_FACTS.md FACT-005; ARBITER.md section 6, which makes the step, the approach, the tasks and the drop candidate the arbiter decision
ACCOMPLISHED: Hamlet can make a PSK31 signal of its own that its own ear reads back letter for letter inside a measured width; the one transmit path can carry a PSK31 over that has no slot and refuses one that would run on; FT8 and FT4 go out exactly as before; and nothing new can reach the air until drive and power are built
ADVANCES: step 4 - loopback of the four macros, the measured bandwidth under 100 Hz at -30 dB, the chain changed only as R10 allows with FT8 and FT4 byte-identical, and a no-slot send over the cap refused before it arms with a record; task 4, the drop candidate, moves the engine side of the turn indicator and its within-one-character nice-to-pass and completes neither
END-ARBITER-DECISION
```

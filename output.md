```
READ IN THIS ORDER.

A. The phase goal - Hamlet works PSK31 the way it works FT8. Steps 0 to 3 done,
   4 in progress (four of its eight must-pass met by this unit), 5 and 6 not started.
B. Step 4 and its eight must-pass, each met or not met, with its number:
   - loopback: MET - all four macros came back identical, at 48000/1000, 48000/1500 and 8000/1000 Hz;
   - bandwidth: MET - 57.1 Hz at -30 dB, under 100;
   - 31.25 baud and the envelope: MET - 1535.0 samples a symbol measured against 1536;
   - the chain changed only as R10 allows: MET - PttOn 1 use site; the pin identical
     (2 of 2, unedited); guarding list engine 82 of 93 before and after, app 50 of 63
     before and after, the same reds by name; cap 30 s. One of the five source-reading
     tests (ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt) was red before any
     change and is red the same way after;
   - over the cap refused before it arms, with a record: yes - MET;
   - receipt and cards, the certainty gate, the turn indicator on the card, and drive
     and power on the panel: not aimed at by this unit, so not met.
   The nice-to-pass: task 4's character count - dropped, with task 4 whole.
C. The report last. Section 4 raises 5 items, all new, on top of a carried queue of
   thirty-two; none is in the way of a criterion in B that this unit aimed at. Ask 32
   (a readable IC-7300 manual) is in the way of the drive-and-power must-pass next unit.
```

```
UNIT:       318 - complete at task 3 of 4, task 4 dropped - 2026-09-11 14:23
PHASE GOAL: PSK31 becomes Hamlet's third digital mode, on a modem Hamlet wrote, with FT8's
            two cards, one-click exchange, log and achievements.
UNIT GOAL:  Give PSK31 a voice at the bench only: a modulator its own ear reads back, a
            measured width, and the one transmit chain able to carry a capped send with no
            slot - while the PSK31 send door stays shut.
ADVANCED:   yes - step 4: loopback, bandwidth, the chain changed only as R10 allows, and a
            no-slot send over the cap refused before it arms with a record
NUMBER:     macros modulated that loop back identical 0 -> 4 of 4; occupied width at
            -30 dB 57.1 Hz; sends with no slot the chain accepts: none -> up to 30 s;
            version 1.13.4 -> 1.13.5
DRIFT:      0 consecutive units without advance  (was 0)
```

## 1. What Claude did

**Complete at task 3 of 4. Task 4, the named drop candidate, was dropped whole.** Machine
QUIVERFULL, project Hamlet (the gate's four checks held: `SHACK_FACTS.md` and
`CwProbabilisticDecoder.cs` exist, `CoreHMI.sln` and `MURC.sln` do not, root
`C:\Source\HamLet`), branch `main`. Commits, each pushed and accepted: `708a511` task 1,
`30d5c78` task 2, `0a6c3ea` task 3a the pin (alone, before the chain moved), `e247547`
task 3b, and the report commit after this file.

**Why task 4 was dropped.** It completes no criterion by itself, and at the end of task 3
the unit stood at about fifty minutes of the 45-to-60 window with the report still to write.
The engine side of the turn indicator and the within-one-character nice-to-pass are not
built; the next unit inherits both, together with the card.

**The line of `PHASE_STATUS.md` this session wrote:** `WORK_INSTRUCTION: 318 - say it, the
engine half: the modulator, the send with no slot, and whose turn it is`. Nothing else in
that file was written. Task 1's commit carried the file as it stood, which included the
launcher's `HEARTBEAT:` line beside that one - unit 316's task-1 commit did the same.

### Task 1 - entry, the trace, the before-counts

**Entry, run by exact name:** `ThePsk31ExchangeParserTests` 8 of 8, `ThePsk31MessageSplitTests`
3 of 3. Step 4 has its ground.

**Before any change, filtered and foregrounded:**

| List | Engine | App |
|---|---|---|
| carry-forward | 82 of 82 | 123 of 123 |
| chain-guarding | 82 of 93 | 50 of 63 |

**Every red on the chain-guarding list, before (and, identically, after):**

- Engine (11): `TheFitGuardAsksAboutTheGridTheSendIsOnTests.BothFt8RefusalSentencesAreWhereTheyWereBeforeThisUnit`
  (known red); `TheTelemetryLineSaysWhatActuallyWentOutTests.` `TheLengthCountsWhatWentOutAndNotWhatWasAskedFor`,
  `TheNewFieldCarriesNoWords`, `TheTwoBagsDifferInTheFieldThatWouldHaveShouted`,
  `AnOrdinaryTransmissionsLineIsUnchanged`, `WhatTheLineSaidOnTheSeventhOfSeptemberAndWhatItSaysNow`;
  `WhereTheTransmissionStartsAndWhatTheRecordSaysTests.` `AnAbortedTransmissionIsRecordedAsAWarningWithHowItCameOut`,
  `TheTransmittedSlotIsRecordedWithItsShapeAndItsMoment`, `ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt`;
  `TheSinkPlaysToANamedEndpointTests.ACancelledPlayGoesOutShortAndTheSequenceCallsItAudioFailed`;
  `TheLoopbackProvesTheWholeChainTests.AMessageHamletComposedLeavesThisMachineAndComesBackFromItsOwnDecoder`.
  **The eight record and telemetry reds all fail on `Assert.Single()`** finding 4 or 5 events:
  unit 305's `send_stage` events go to the same telemetry as the `ft8_transmission` record.
- App (13): `TheMenuIsUnderTheMouseTests.` `ARowThatNamesNoStationPutsNothingUnderTheMouse`,
  `OutOfPrivilegesTheMenuSaysSoAndForbidsNothing`, `BothListsCarryTheMenuAndLogIsOnTheRightOne`,
  `EveryStationsPredictedMenuAppearsUnderTheMouse`, `TheRepeatCountBelongsToTheClickAndNotToTheRow`,
  `AThirdPartyExchangeHasAMenuAndNoLog`, `WithNoGridTheReasonIsANoteAndTheRestStayClickable`,
  `ChoosingOneGoesThroughTheOneCommandThatArms`; `TheWholeFt4ChainRunsFromOneRightClickTests.`
  `TheWholeExchangeOnTheReportShapeRunsFromTwoRightClicks`,
  `OneRightClickOnAnFt4RowDrivesTheWholeChainAndTheAudioDecodesBack`,
  `TheSameChainThroughTheFakeSinkDecodesTheSamplesItWasHanded`; `TheWholeChainRunsFromOneRightClickTests.`
  `OneRightClickDrivesTheWholeChainAndTheAudioDecodesBackAsTheClickedText`,
  `TheOperatorsStopButtonTakesARealTransmissionOffTheCardMidSlot`.

**`TheFitGuardAsksAboutTheGridTheSendIsOnTests`, method by method, before and after:**
`AnFt4TransmissionFitsAnFt4SlotAndAnFt8OneDoesNot` green, `NeitherRefusalSentenceNamesTheOtherModesNumbers`
green, `BothFt8RefusalSentencesAreWhereTheyWereBeforeThisUnit` **red**, `ASendWithNoGridStatedIsOnFt8s`
green, `ArmHasExactlyOneCallerInSrc` green.

**The endpoint tests.** They did not skip. `TheSinkPlaysToANamedEndpointTests` 5 green and 1
red, `TheStopStopsARealEndpointTests` 1 green, `TheSendPathReachesARealRadioTests` all green -
before and after. This machine has sound endpoints; it has no radio (FACT-006).

**Version** 1.13.4 -> 1.13.5 (`Directory.Build.props:606` after the new comment).

#### 1b - the trace, file and line at `a36ce1e`

**One FT8 CQ press, end to end.** `SendMessage` (`MainWindowViewModel.cs:12535`) -> the mode
gate `CanTransmitIn` (`:12561`, body `:1951-1954`) -> `ComposeForTheChosenMode` (`:12598`,
calling `Ft8Composer.ComposeSignal` at `:12745` with `_transmitSampleRate` and
`TransmitDrivePeak`) -> `Ft8ReadBack.Check` (`:12626`) -> `BookTheSend` (`:12655`, body
`:11804`) -> `_armedSend.Arm(new OperatorSend(...){ Grid = grid })` (`:12683-12697`, offset
`StartSecondsIntoSlot = 0.5` at `:12718`) -> `OnSlotTick` calls `DriveTheArmedSend`
(`:10560`, body `:12769`) -> `AtSlotBoundaryAsync` (`:12806`) -> `Ft8ArmedSend.AtBoundaryAsync`
(`Ft8ArmedSend.cs:426-491`, `_transmitting` set under `_gate` at `:457-458`) -> `RunAsync`
(`Ft8TransmitSequence.cs:307-456`): gate `:330`, `Sendable` `:341` (body `:595-652`), `PttOn`
`:363`, sink `PlayAsync` `:372-374`, `finally` `:415-448` with `TransmitAbort.Fire` `:446`,
`Recorded` `:487-517`. The sequence is built once at `MainWindowViewModel.cs:11282`.

**Every FT8-only assumption a send with no slot met on the way:** `Sendable` read
`send.Grid`, `send.StartSecondsIntoSlot` and `send.Transmission.Samples`; `RunAsync` read
`send.Transmission.Samples` and `.SampleRate`; `Recorded` read `SlotStartUtc`,
`StartSecondsIntoSlot`, `Transmission.Type`, `ReadsBackAs.Length` and `CarriesHashedCallsign`;
`AtBoundaryAsync` read `SlotStartUtc` twice; `TransmitRecord` held a non-null `DateTime`, a
`double` offset and an `Ft8MessageType`; `DriveTheArmedSend` reads `Armed?.Grid` (app, not
changed).

**The five source-reading tests, and what each counts or forbids:**

1. `NothingInTheSequenceCanStartATransmissionOnItsOwn` - no `Timer`, `Delay`, `Interval`,
   `Elapsed`, `Schedul`, `Periodic`, `Recurring`, `Sleep`, `Stopwatch`, `TickCount`,
   `DateTime.UtcNow`, `DateTime.Now`, `Environment.Tick`, `PeriodicTimer`, `OnTick`,
   `AutoCall` or `Repeat` in `Ft8TransmitSequence.cs`. **It strips only `///` lines, so a
   `//` comment counts.**
2. `NothingInTheArmedSendCanStartATransmissionOnItsOwn` - the same words in `Ft8ArmedSend.cs`,
   and **exactly three lines containing `_armed =`, exactly one of them not null.**
3. `NothingOnTheStopPathWaitsForAnything` - `StopNow` is not async and returns `Ft8StopResult`;
   `StopNow`, `StopTheAudio` and `Cancel` contain no `await `, `async `, `Task`, `.Wait(`,
   `.Result`, `GetAwaiter`, `Sleep`, `Delay` or `Join(`; `StopNow` calls `StopTheAudio()` and
   `Cancel()`.
4. `ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt` - `Assert.Single` over every
   telemetry event, then no callsign, grid or message in the bag, `TransmitRecord`'s single
   constructor has no `string` parameter, and every string value is the slot moment or an
   enumeration name. **Red at entry on the `Assert.Single`, so the rest of it never runs.**
5. `ArmHasExactlyOneCallerInSrc` - exactly one `.Arm(` in code under `src\`, in
   `MainWindowViewModel.cs` (it prints `:8517` because `///` lines are removed first).

Beside them, `ExactlyOneFileInTheShippedTreeCallsTheSequence` requires `new Ft8TransmitSequence`
in `MainWindowViewModel.cs` only, `_sequence.RunAsync` in `Ft8ArmedSend.cs` only, and no other
file naming `Ft8TransmitSequence` in code.

**Whether any test asserted the record's exact field set:** no green test did. Test 4 above
constrains value types; the red `TheTelemetryLineSaysWhatActuallyWentOutTests` never reaches
its bag assertions. Task 3a's pin is now the test that holds the field set, key by key.

**What the tree had for PSK31 audio.** A generator exists: `assets/reference-modem.py`
(`assets/fixtures/README.md`), Python and numpy, fixed at 8000 Hz, 40 idle bits either side,
amplitude 0.5, raised-cosine pulse two symbols wide on each symbol centre. **It is in the tree
and cannot be wired**: it is Python with a package, and the engine is C# with no DSP package
(§0.4). It confirmed the convention; nothing was ported. The demodulator skips its first
symbol and opens its squelch only after `QualityWindow` (32) symbols; measured here, the
shortest idle before that still read back was **23 bits**, and after, **0 bits**. The
fixtures are **8000 Hz**. `_transmitSampleRate` defaults to `Ft8Composer.DefaultSampleRate`
(12 000) at `:11297` and is read off the sink on connect at `:11264`.

**For the next unit, building nothing (§R4).** `CivWrites` has **no data-mode input level
(USB MOD level) write** beside `RfPower` (`CivWrites.cs:271`); the USB modulation input is
named only in remarks citing FACT-004. **No IC-7300 manual is tracked in the repository**
(no file named for it); `CLAUDE.md` 13.4 cites "Full Manual A7292-4EX-6" page numbers for
other ratings. **Whether one exists elsewhere on this machine is not known**: the search
outside `C:\Source\HamLet` was refused (section 4, item 33).

**Mismatches between the instruction and the tree:**

- Every file and line claim in section 2 that this session checked held at `a36ce1e`:
  `OperatorSend`, `RunAsync` and its five sites, `Ft8Transmission`, the `Ft8ArmedSend` members,
  `TransmitRecord`, the `SendMessage` sites, the five test lines, `Varicode.Encode`, the
  demodulator's constructor and `Add`, `OperatorProfile`, `CivWrites.RfPower`, the version
  line, and the carry-forward list's 22 + 1 app and 10 engine names. The two
  `Psk31CarrierSearch` `RealFft` lines were not checked.
- **The chain-guarding list is not green at entry**, which the instruction does not say: 10
  engine reds and 13 app reds beyond the known `TheFitGuard` red, named above - including one
  of the five source-reading tests.
- **The endpoint tests do not skip** on this machine; they open real sound endpoints.
- The five mismatches the arbiter named: (1) `CLAUDE.md` holds `HM-DEC-161` at line 360 and no
  `CPS-DEC`, as stated; the reload itself was not run and `tools\` was not touched. (2)
  `PHASE_PLAN.md` §2 still carries the stale sentence; R10 was applied. (3) the §2 nudge line is
  as stated; not touched. (4) `output.md` was absent; this one is fresh. (5) `PHASE_STATUS.md`
  said 316; set to 318.

### Task 2 - the modulator and the four macros

**`Psk31Modulator`** (`src\Hamlet.RadioEngine\Psk31\Psk31Modulator.cs`): text, rate, offset and
peak in; samples out. Differential BPSK at 31.25 baud, a `0` bit a reversal and a `1` bit no
change, each character its varicode then `00` - the convention `Psk31Demodulator` reads. The
amplitude holds each symbol's sign at its centre and crosses between centres on half a
cosine, so it is flat across a boundary with no change and passes through zero on a
reversal; the first and last half symbols rise from and fall to silence. **Idle: 32 bits
before** (one `QualityWindow` of reversals, so the squelch has measured a whole window before
the first code; measured shortest 23), **16 bits after** (every code already ends `00`, and
the measured shortest was 0; the 16 are this unit's margin for receivers that are not Hamlet,
not a measurement). **`Psk31Macros`**: §R2's four texts to the space, every field handed in,
grid cut to four characters, a blank field or a character the varicode cannot carry refused.
No callsign is written into either file, and a test reads both to say so.

**`ThePsk31ModulatorTests`, 15 of 15**, watched 11 of 15 fail on a stub first.

**The macro table** (48 000 Hz; with the idle above):

| Macro | Characters | Varicode bits | Text s | With idle s | Instruction 317 | Loopback |
|---|---|---|---|---|---|---|
| CQ | 38 | 309 | 9.89 | 11.46 | not stated | identical at all three |
| Answer | 23 | 196 | 6.27 | 7.84 | not stated | identical at all three |
| Report | 96 | 726 | 23.23 | 24.80 | 23.2 | identical at all three |
| Confirm | 62 | 459 | 14.69 | 16.26 | 14.7 | identical at all three |

"All three" is 48 000 Hz at 1000 Hz, 48 000 Hz at 1500 Hz, and 8000 Hz at 1000 Hz; exact
string equality, both strings printed. **Report to `VP2V/W1AW`: 28.19 s with idle** against the
cap of 30 s (nice-to-pass 6, met).

**The bandwidth, and how it was measured.** The Report macro at 48 000 Hz, carrier 1000 Hz,
through the tree's `RealFft`: periodic Hann window of 32 768 samples (1.46 Hz a bin), half
overlap, power averaged over 74 windows; the width at a level is lowest to highest bin at or
above that many dB below the peak. **-6 dB 30.8 Hz, -20 dB 51.3 Hz, -30 dB 57.1 Hz.**

**31.25 baud and the envelope.** Symbol length from the envelope's minima over the idle:
**1535.03 samples** against `48000 / 31.25 = 1536`, 31.27 baud. All 363 reversal boundaries dip
under 3 per cent of the peak; all 411 no-change boundaries hold above 97 per cent across the
symbol either side. **Peak:** asked 1.0, 0.5, 0.25 and 0.1, the loudest sample equals each and
never exceeds it.

### Task 3 - the one sequence carries a send with no slot

**3a, the pin, `0a6c3ea`, committed and pushed alone before the chain moved.**
`TheFt8AndFt4SendsAreByteIdenticalTests` builds each send as `SendMessage` does (`ComposeSignal`
at 48 000 Hz and the default drive, 0.5 s in, the grid on the send), arms it, and hands it its
boundary through `Ft8ArmedSend` and `RunAsync` over a recording port, sink and telemetry.
**Its values:**

- port, both: `FE FE 94 E0 1C 00 01 FD FE FE 94 E0 1C 00 00 FD`;
- FT8: 606 720 samples at 48 000 Hz, SHA-256 `a66c5929374bb079f385a728cd614a14f9a394cfad8ea7570d4c6f6fdee22af1`,
  `SecondsOffered` 12.64;
- FT4: 241 920 samples at 48 000 Hz, SHA-256 `a224275149be459b4a22621ecd4240fbbb8c474e5435729af2446eeedeb9066b`,
  `SecondsOffered` 5.04;
- run, both: `Played`, reason empty, citation empty, keyed, unkeyed normally, no abort,
  `OrdinaryUnkey`, radio in receive;
- telemetry, both: four `send_stage` events (`gate_asked`, `keyed`, `handed_to_the_sound_card`,
  `unkeyed`), then `ft8_transmission` with `slotStartUtc`, `startSecondsIntoSlot`,
  `frequencyHz`, `durationSeconds`, `sampleRate`, `sampleCount`, `messageType` (`Standard`),
  `messageLength` (14), `carriedHashedCallsign`, `outcome`, `cameOutOfTransmit`, `keyed`,
  `stagesEntered`, each with its value type.

**Green before the change and green after, unedited.**

**The cap: 30 s.** R10's ceiling, and every §R2 macro fits under it with idle - the longest,
the Report, is 24.80 s to W1AW and 28.19 s to VP2V/W1AW. Anything under about 28.2 s would
refuse the Report to a compound callsign; the 1.8 s left over is all the room a longer name or
place in Settings gets, and a report longer than that is refused with its length.

**The design, in four sentences.**

1. **How now travels:** `OperatorSend.Now(UnslottedTransmission, frequency, class, guard)` makes
   a send whose `HasSlot` is false and whose PSK31 audio travels as an `UnslottedTransmission`
   (mode, samples, rate, character count - no text, and no FT8 message type), whose slot start,
   offset, grid and FT8 transmission throw instead of standing in, and which the operator's
   action fires through the new `Ft8ArmedSend.NowAsync` - that takes it under `_gate` by calling
   `Cancel()` (so the field keeps its three writes), sets `_transmitting` before the await, and
   calls the same `RunAsync`.
2. **Where the over-cap refusal happens:** `Arm`, before its lock, asks
   `Ft8TransmitSequence.RefusedBeforeArming`, which measures the audio against
   `OperatorSend.LongestUnslottedSeconds` and, over it, writes a Warn `ft8_transmission` record -
   `outcome: RefusedAsUnsendable`, `fit: LongerThanTheCap`, `audioSeconds`, `longestSeconds: 30` -
   and returns the refusal with nothing armed; `Sendable` asks the same question inside
   `RunAsync` as the backstop.
3. **How the record names the mode and carries no slot:** `TransmitRecord`'s slot start, offset,
   message type and hashed flag became nullable and a null field is not written, while `mode`,
   `fit`, `audioSeconds` and `longestSeconds` are written only for a send with no slot - so a
   slotted send still writes its twelve keys in their order, which the pin holds.
4. **How `StopNow` reaches it:** `NowAsync` installs the stop's cancellation source under
   `_gate` before awaiting exactly as `AtBoundaryAsync` does, so `StopNow` - unchanged - un-arms,
   fires the abort and cancels the audio; measured, `StoppedTheTransmissionAndToldTheRadio`, the
   run `Cancelled` and `RadioIsInReceive` true.

Also: a boundary that finds a no-slot send keeps it and answers `HasNoSlot`; `NowAsync` finding
a slotted send keeps it and answers `WaitsForItsSlot`. `Arm` now returns `TransmitRun?`, null
wherever it arms - every FT8 and FT4 caller ignores it and gets null.

**`TheUnslottedSendTests`, 9 of 9**, watched 7 of 9 fail on stubs (`Arm` without the check,
`NowAsync` answering nothing armed) first:

1. under the cap, now: gate asked first; wire `1C 00 01` then `1C 00 00` and nothing else; the
   sink got all 549 888 samples at 48 000 Hz; `Played`; the record says `mode=Psk31` and has no
   `slotStartUtc`, `startSecondsIntoSlot` or `messageType` - **met**;
2. over the cap (a 35.04 s Report with a long place name): `Arm` returned the refusal, `Armed`
   null, port and sink untouched, one Warn record with `fit=LongerThanTheCap`,
   `audioSeconds=35.04`, `longestSeconds=30`, reason naming "30 s"; `NowAsync` then found
   nothing; handed straight to `RunAsync`, refused the same way - **met**;
3. the licence gate: a Technician with the guard off, an unknown class, a Technician outside
   privileges - each `RefusedByLicence`, nothing keyed - **met**;
4. the stop while it plays - **met**, numbers above;
5. a boundary never runs a no-slot send, now never runs a slotted one - **met**;
6. the pin green and unedited - **met**;
7. `PttOn` has **one** use site (`Ft8TransmitSequence.cs:513` after the change);
   `ArmHasExactlyOneCallerInSrc` gives the same result as task 1; four of the five
   source-reading tests are green and unedited, and the fifth is red exactly as at entry -
   **met as far as the tree allows**;
8. the chain-guarding list gives the same pass counts, and no test in it was edited - **met**.

One case of this unit's own test was wrong first and was corrected: a General with the guard off
inside privileges is not an override, so the gate permits it and it played - identically for
FT8. The case now mirrors `TheLicenceGateIsInsideThePathTests`: a Technician with the guard off.

**After the change, filtered and foregrounded:**

| List | Engine | App |
|---|---|---|
| carry-forward | 108 of 108 (82 + 26 new) | 123 of 123 |
| chain-guarding | 82 of 93, the same 11 reds | 50 of 63, the same 13 reds |

The three new classes are on `docs\carry-forward-tests.txt`: `ThePsk31ModulatorTests`,
`TheFt8AndFt4SendsAreByteIdenticalTests`, `TheUnslottedSendTests`.
`ThePsk31TabIsInertTests` is green and unedited; `CanTransmitIn` is untouched.

**Left uncommitted, and whose:** `.run-unit\*` and `SESSION.lock`, the launcher's. Nothing of
this session's.

**This report was not validated by the script.** `tools\arbiter\validate-output.bat output.md`
was refused by the shell (*"This command requires approval"*) and was not retried in another
form. The seven rules were checked by hand against the script's own patterns and held; that is
not the exit 0 the prompt requires, and section 4 item 33 says so.

## 2. What the owner should expect

**Now true, at the bench.** Hamlet can make PSK31 audio of its own and read it back exactly.
The transmit sequence can carry a send with no slot, fired by an action rather than a boundary,
held to 30 s, and refused before arming - with a record - when it is longer. FT8 and FT4 put
the same bytes, samples, run and record through it as before.

**What will look wrong but is not:**

- **`ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt` is red.** It was red before this
  unit, on `Assert.Single` against unit 305's stage events; the no-callsign property it would
  check is checked for the PSK31 record in `TheUnslottedSendTests`.
- **The guarding lists show 11 and 13 reds.** Same names, same counts, before and after.
- **A PSK31 transmission record is written under `ft8_transmission`**, with a `mode` key. The
  event name was kept so a reader counting transmissions misses none; section 4 item 34.
- **Asking a send with no slot for its slot start, offset, grid or FT8 transmission throws.**
  That is deliberate: an FT8-only reader meeting a PSK31 send fails in place rather than
  reading a made-up slot. `DriveTheArmedSend` reads `Armed?.Grid` - harmless while the door is
  shut, and the door unit has to fire `NowAsync` in the same click (item 37).
- **The refusal sentence says "Psk31 audio"**, the enumeration's spelling. Nobody can see it
  while the door is shut (item 37).
- **`Arm` returns a value now.** Null for every FT8 and FT4 send.
- **The endpoint tests opened real sound endpoints** on this machine. They are old tests; no
  new test opens a real port or endpoint.

## 3. What you should see

**Nothing Tim can press has changed tonight.**

**Pressing CQ under PSK31 still refuses, in words** - "Hamlet cannot send PSK31 yet, so nothing
went out." The door opens with §R4's drive and power, because a PSK31 carrier driven into ALC
splatters across 14.070.

**What now exists, at the bench only:**

- a PSK31 signal Hamlet makes, which its own ear reads back letter for letter - all four macros,
  57.1 Hz wide at -30 dB;
- a transmit path that can carry a PSK31 over with no slot, and refuses one that would run on
  past 30 s;
- FT8 and FT4 going out exactly as before, pinned to the byte;
- task 4 did not run, so there is no reading of whose turn it is yet.

Every number here is from the bench and synthetic. Nothing keyed a radio. Every appearance
claim is computed, not seen.

## 4. What's blocking us

**Carried, verbatim where unresolved.**

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
    whether one exists anywhere on this machine. *(Unit 318: none tracked; outside the
    repository could not be searched - item 33.)*

**New from unit 318, most blocking first.**

33. **Hand wanted: this session could read nothing outside `C:\Source\HamLet`.** The file tools
    refused `C:\Source\fldigi\src\psk\psk.cxx` (*"--restricted confines the file tools to the
    working directory"*) and `find` over the user profile was blocked, so §R5's pinned reference
    was not read and no IC-7300 manual outside the tree could be looked for. The modulator's
    convention came from `Psk31Demodulator` and `assets/reference-modem.py` instead. The shell
    also asked for approval on `pwd -W`, a `sed` stage and a `grep -v` stage in pipelines; each
    was dropped, not retried in another form. **And `tools\arbiter\validate-output.bat
    output.md` was refused** (`cmd //c`, *"This command requires approval"*), as for units 315
    and 316, so **this report was not validated by the script** and item 17 stands. It was not
    retried in another form. The seven rules were checked by hand against the script's own
    patterns - four `## ` headings in order and no fifth, a `UNIT:` line, the ordering block's
    `READ IN THIS ORDER`, `A.`, `B.`, `C.` and `raises 5 item` inside the first 60 lines, and no
    placeholder token before section 1 - and that hand check is not the exit 0 the prompt asks
    for. *Reasoning:* §R5 names fldigi as what the shaping
    is read from, and §R4 needs a page citation. *Rejected:* copying either into the tree, which
    §R5 forbids. **Wanted:** the launcher grants read access to `C:\Source\fldigi` and to a named
    manual path, or Tim rules that the demodulator and the reference modem suffice for the shape.
34. **Ruling wanted: the record's event name and FT8/FT4's missing mode.** A PSK31 transmission is
    written under `ft8_transmission` with `mode: Psk31`; FT8 and FT4 records still do not say
    which of the two went out, and cannot gain a key without breaking tonight's pin. *Options:*
    keep the name and add `mode` to FT8 and FT4 under a re-pinned test; rename to a neutral event
    with a migration note; or accept as is. *Industry answer:* one event with a mode field on
    every record, re-pinned in its own commit.
35. **Ruling wanted: 23 reds on the chain-guarding list that are not on the known-red list.** The
    8 record and telemetry reds (unit 305's `send_stage` events defeat their `Assert.Single`),
    2 real-endpoint engine reds and 13 app reds, named in section 1 - including
    `ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt`, one of the five tests the instruction
    relies on to show the chain changed only as R10 allows. *Options:* put them on the known-red
    list by name, or give a unit to repairing them. *Rejected by this unit:* chasing them now,
    which the instruction forbids.
36. **Ruling wanted: accept the idle, 32 bits before and 16 after.** 32 is measured against
    Hamlet's own ear (shortest 23); 16 after is this unit's margin over a measured 0, for
    receivers nobody here has measured. A real receiver's needs want item 33's fldigi access or
    item 14's off-air audio.
37. **For the door unit, low: three edges the send path will meet.** `DriveTheArmedSend` reads
    `Armed?.Grid`, which throws for a no-slot send, so the PSK31 click must arm and call
    `NowAsync` in one handler; `Arm` returns the refusal, which the click must show; and the
    refusal sentence says "Psk31" where the operator reads "PSK31".

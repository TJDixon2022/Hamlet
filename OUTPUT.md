# OUTPUT.md

## 1. What Claude did

Claude Code on the development computer, in `C:\Source\HamLet`, on `main`. The
prompt said `PROJECT: Hamlet` and `WORK_INSTRUCTIONS.md` says the same; the tree
confirms it, with `Hamlet.sln` at the root, `Hamlet.*` namespaces, and
`PROJECT_CARD.md` naming Hamlet. `CwGate.cs` was present when the gate was
checked, as the order says it would be. **No radio was connected and nothing in
this report is evidence about the radio** (HM-DEC-093).

**The failing set: 55 before, 30 after.** Both numbers are the whole solution,
measured the same way — the before-set was taken by running the suite in a git
worktree at `862135a`, because the last report's figure was not a set of names and
a count without names cannot be diffed.

**Nothing was recorded under §12.1 this session.** Everything that needed a
judgement is in section 4.

### Task 1 — what still hung off the old path

| What | Needed | Where it comes from now |
|---|---|---|
| Element and character counters | The gate's edges and the settled pass's emissions | The working decoder's own settled characters — already re-pointed last session |
| `Watch` / `DecodeNote` | `CwSignalWatch`, fed by the gate | Nothing. Deleted: it can no longer be written and an always-empty note is not a note |
| Tip mark, ceiling note, revisions count | `CwSettledPass` reporting on itself | Nothing. One pass now, so a second pass's opinion of the first has no referent |
| `SpeedIsReacquiring` | Marks seen since the tracker moved | The tracker's own `Follows` count against a window's worth of audio |
| `WordsPerMinute` | The run-length clock fit | `Reading.WordsPerMinute`, the winning hypothesis |
| Transmit guard, audio tap, capture press, roster, sidecar, case measure | The tracker and the tap | Unchanged. None of them touched the gate |
| **`CwToneTracker.MidCharacter`** | The gate's elements in flight | **Nothing can supply it.** See section 4 |
| **`CwToneTracker.FollowSpeed`** | The old clock fit | **No obviously right supplier.** See section 4 |

The last two are the ones task 1 asked to be found before deleting, and they were
found afterwards instead, during task 5's measurement. That is the cost of this
report being late in the unit rather than early.

### Task 2 and 3 — deleted

`CwGate.cs`, `CwTiming.cs` (the estimator, `Refine`, the vote window, the element
floors), `CwSettledPass.cs`, `CwSignalWatch.cs`, `CwGapClasses.cs`, and the old
`Emit` sites with them. `CwDecoder` is now a host: it owns the tracker, the tap,
the transmit suspension and the counters, and it hands audio to
`CwProbabilisticStream`, which is the only thing that reads characters.

Also removed, because they were the deleted decoder's own readouts and could no
longer be written: `CwConfidenceModel` and `CwRevision` in `CwCharacter.cs` (both
unreferenced in `src` once the gate was gone), the view model's `DecodeNote`,
`CeilingNote`, `TipIsUnstable`, `TipMarkText`, `RevisionCount`, `RevisionsText`
and `SettledCount`, and the revisions row and its export command in the window.

`CwConfidenceModel` held `RefusalFloorDb = 14.0`, which is HM-DEC-120's number.
**The ruling's reasoning is in `DECISIONS.md` and is not lost**, but the constant
is, because nothing read it: the working decoder refuses on a likelihood ratio
against silence (`Gate = 15.0`), not on a signal margin.

### Task 4 — the tests, one by one

**Deleted, because their subject is the removed decoder (22 files):**

| File | Why |
|---|---|
| `CwDecoderTests` | The old decoder's own suite, end to end |
| `CwSettledPassTests` | The settled pass |
| `Fixtures/CwSettledSilenceTests`, `Fixtures/CwSettledGapTests` | The settled pass against the tip |
| `Fixtures/CwLeadingEdgeAccuracyTests` | The old streaming tip |
| `Fixtures/CwFarnsworthTests` | The gap classifier's three classes |
| `CwTwoClassGapTests` | The two-class gap fit |
| `CwMarkBoundaryTests`, `CwBoundaryMarginTests`, `CwEdgeCharacterTests` | `ClassifyMark`, the boundary margin, the edge elements |
| `AHeavyFistIsStillMorseTests`, `ASmearIsNotTwoLengthsTests`, `QuietMarksAreNotThisSendersTests` | The clock fit's clustering |
| `CwNoteHonestyTests` | `CwSignalWatch`'s notes |
| `CwRefusalFloorTests`, `CwFloorSweepTests` | The signal-margin refusal floor |
| `ViewModels/CwTerminalTests` | The terminal's settled-pass readouts |
| `MarkAmplitudeTests`, `WhereTheTransitionsClusterTests`, `ACarrierClockDoesNotSeparateTests`, `TheStationInTheRecordingIsN4LTests`, `TheStationInTheOtherRecordingIsVa3vrrTests` | **All five read marks and gaps by watching `Report.ElementsSeen` tick, which was the gate's edge clock.** With the gate gone that count moves in character-sized jumps and the extraction collapses to one twenty-two second mark |

**That last row is the one worth arguing about**, and it is in section 4: those two
station tests are the standing evidence behind HM-DEC-144 and HM-DEC-145.

**Kept and made to pass (8 files):**

| File | What was done |
|---|---|
| `CwDecodeHarness` | `State` and `Note` dropped from the result; the speed now comes from `Reading` |
| `CwSensitivity` | The `refusalFloorDb` and `gateWindowHops` knobs dropped; they addressed the gate |
| `Fixtures/CwTwoStationTests` | The speed-change and clock-loss tallies were the settled pass's signals; the tracker's own move is what the assertions read. HM-DEC-104's claim is untouched |
| `Fixtures/CwAdjudicationTests` | `decoder.State` to `decoder.Reading` |
| `CaseRosterSurvivesAnEveningTests` | The roster's speed column reads `decoder.WordsPerMinute` |
| `ARecordingWithKeyingInItIsReadTests` | The `ElementsSeen > 50` line removed: fifty was the gate's edges, and the field now counts resolved elements, so the same number against a different measurement is a threshold kept for its own sake. The claim the test names — the tone is found and characters come out — is untouched |
| `TheGateHasItsOwnWindowNowTests` | `AWiderGateInventsBelowTheRefusalFloor` removed with the knob it swept; the rest, including both silent recordings staying silent at every width, kept |
| `WhatBandwidthTheDecoderListensThroughTests` | The fitted-speed print reads `Reading` |

**Nothing was weakened to make it pass.** Where a threshold no longer measures
what it was calibrated against, the line was removed and said so above, rather
than moved.

### Task 5 — what was proved and what was not

**Both recordings holding no keying stay silent**, offline and streamed:
`014854` and `014935` emit nothing, and
`HoldingItLongStillSaysNothingAboutAnEmptyBand` passes.

**Every recording still decodes at or above its floor** —
`TheCapturesThatDecodeKeepDecodingTests` passes with 45, 100, 49, 47 and 48
characters against floors of 34, 8, 38, 25 and 14.

**And the text is worse on three of the four, which the floors could not see.**

| recording | before | now |
|---|---|---|
| `004507` | `E JJ AT ARRL DOT NE T <BT> E ACH STATION HANDLING THIS ME SSAG E PE` | `W AT ARRL DOT N E T <BT> E ACH STATION HANDLING ET HIS M E S S A G E P E` |
| `003758` | `KIS QRL TU E EAN EANDE AA4MP/4 QNIK` | `E URL TS EHEIISEIA■IH/5■IS E E E EAN EANQNI<HH>SK` |
| `003016` | `I<BT> HADA KPA15TT ITWAS JUNK <BT> ESTILL HVE MY ETO 91B TT JUST VFB TUBELIN` | `E ■I KPA1■IS<HH> ■NK <BT> STILLHVEMY ETO 91B E TT JEAST VFB TUBE LIN` |
| `003126` | `A OM <BT> E <BT> I WATCH AT L EAST 2 MOVI ES A DAY WID X# WHY NOT` | `E5 5 I■SH■5 MOVI ES A DAY WID X■ WHY N■TT E E , WESTERNSE, E` |

**And the sensitivity sweep now invents at every level**, which is HM-DEC-120's
property broken:

| | 18 dB | 10 dB | 5 dB | 3 dB | 2 dB | 0 dB |
|---|---|---|---|---|---|---|
| before, right / wrong | 1.00 / 0.00 | 1.00 / 0.00 | 1.00 / 0.00 | 1.00 / 0.00 | 0.92 / 0.00 | 0.00 / 0.00 |
| now, right / wrong | 0.81 / 0.11 | 0.78 / 0.11 | 0.67 / 0.19 | 0.64 / 0.22 | 0.64 / 0.22 | 0.61 / 0.25 |

**The cause is diagnosed exactly and it is one line.** `CwToneTracker.MidCharacter`
is HM-DEC-096 phase 3's interlock: the tracker may not jump to another part of the
band while a character is part-read, because the rest of that character then gets
assembled from a different station. The old decoder set it from the elements its
gate had in flight. Setting it to a constant `true` reproduces the old table
exactly — 1.00 and 0.00 at eighteen decibels, 27 characters, 26 at three, 22 at
two — which is the proof that this and nothing else is the difference. A constant
`true` is not the answer, because it also blocks every legitimate move to another
station, and `_tracker.HasKeying` was measured as a substitute and does not help:
the keying verdict takes three seconds to form and the damage happens inside them.

**Nothing was invented in its place.** What replaces that interlock decides what
the display asserts, and those are Tim's without exception (§12.1). It is the first
ask in section 4.

### Task 6 — the version

**`Directory.Build.props` moved 1.10.1 to 1.10.2**, one work unit, per HM-DEC-150.

**The previous bump did happen.** Commit `866d225` moved 1.10.0 to 1.10.1, and
the omission was in the report rather than in the tree.

### The order, checked against the rulings it cites

Two citations in `WORK_INSTRUCTIONS.md` name the wrong record, reported rather
than repaired:

- **"HM-DEC-146, `CwGate.ShortestVote` stays at 5."** HM-DEC-146 is the ruling
  that HM-DEC-119's mark-length figures hold at a hundred milliseconds and fail at
  fifty-six. The `ShortestVote` question is **HM-OPEN-053**, open and owned by
  Tim. It is moot now either way, which is what the order asked be said.
- **"§12.2, no radio on the development machine."** §12.2 is the report's four
  headings. The no-radio rule is **HM-DEC-093** and `SHACK_FACTS.md`.

### The inbound asks queue, checked against `OPEN_ISSUES.md`

Every id the queue names is `status: open` in the file, so nothing on it is
already closed. One discrepancy the other way: **`HM-OPEN-051` is recorded as
`open` while HM-DEC-143 says it closes it.** Named and left, per §12.6.

`HM-OPEN-053` (the vote window) and `HM-OPEN-051` (the keying gate on the settled
pass) both describe code that no longer exists.

## 2. What Tim should expect

**The app looks the same and reads worse.** Nothing visible moved except three
lines that could no longer be written: the decode note, the ceiling note and the
revisions row are gone from the terminal. The transcript, the speed badge, the
front-end chip, the roster and the sidecar are where they were.

Build clean, no warnings, version 1.10.2.

**30 failing, down from 55.** Of the 30, twenty-two were failing before this unit
started and are untouched by it — the six `CwDisplacementFloorTests`, the nine
`CwFixtureTests`, the four `CwReceiverFixtureTests`, `CwRefiningRetuneTests`'s
settling case, `HoldingTheWindowLongInTimeReadsMore` on `003016`, and the
scanner's end-to-end dwell.

**Eight are new, and they are all one fault.** `CwSensitivityTests` ×2,
`CwAcquisitionWindowTests` ×3, `CwAdjudicationTests.ClearingTheScreen…`,
`MostRealRecordingsSitInTheWidestWindow` and
`CwEmissionGateTests.NoSpeedIsNamedWithoutCharactersToNameItFrom`. The first six
are the mid-character interlock. The last is the reacquiring guard: the tracker
counts its first jump off the configured pitch as a move to another station, so on
a six-second fixture at 620 Hz the speed stays unnamed for the whole of it. **The
failure is on the safe side** — a speed withheld rather than a wrong one shown —
and the alternative was measured and broke `NoSpeedBetweenTwoStationsIsEverNamed`,
which is a §0.0 protection, so it was not taken.

**One thing to know about how this session went.** A revert command aimed at an
experiment took `CwDecoder.cs` back to its committed state, which is the old
decoder. The rewritten host was recovered in full from this session's own record
and the later edits re-applied, and the suite was re-run afterwards from scratch;
the numbers above are from that run. Nothing else in the tree was touched by it.

## 3. What we should do next

- **Rule on the mid-character interlock**, in section 4. It is worth 0.19 of the
  message on the sweep and a callsign on `003758`, and nothing else in the queue
  is worth as much.
- **Then re-measure the four recordings and the sweep**, and put the sweep's
  invented share under a test that fails rather than a table that prints.
- **Decide whether a mark-and-gap witness is rebuilt on `KeyingEnvelope`**, which
  is what HM-DEC-144 and HM-DEC-145 would need to have a test standing behind them
  again.
- **`ElementsSeen` and `ElementsResolved` now carry the same number.** Two fields
  with one meaning is how a sheet starts disagreeing with itself; one of them
  should go, and which one is a five-minute job once the interlock is settled.

## 4. What's blocking us

Nothing blocks the next unit. Two asks, both new, the first blocking the most.

> **The tracker's mid-character interlock is fed from the working decoder's own
> key-down likelihood, and until it is, the tracker does not jump to another part
> of the band at all while a station is being read.**
>
> `CwToneTracker.MidCharacter` stops the filter moving part-way through a
> character, because the rest of that character then arrives from somebody else
> and comes out as a letter nobody sent with clean timing. The gate supplied it
> from the elements it had in flight; the gate is gone, and with the input unset
> the sweep returns 0.11 of the message as invented characters at every level from
> eighteen decibels down, where it invented nothing at all. On `003758` it costs
> `AA4MP/4`.
>
> **The working decoder does know**: it computes a key-down and a key-up
> log-likelihood for every five millisecond hop, and the newest hop's answer is a
> statement it already makes rather than a threshold anybody invents. What it does
> not do is publish it, and publishing it touches the one decoder this unit was
> told not to touch.
>
> **Rejected: a constant `true`**, which reproduces the old numbers exactly and
> blocks every station change, including the answer to a CQ. **Rejected:
> `HasKeying`**, measured, no help, because the verdict takes three seconds to
> form and the damage is inside them. **Rejected: leaving it and calling the sweep
> a synthetic-fixture problem**, because three of the four real recordings read
> worse too.

> **HM-DEC-144 and HM-DEC-145 keep their findings and lose their instrument, and
> a mark-and-gap witness is rebuilt on `KeyingEnvelope` or it is not.**
>
> Both rulings adjudicated a callsign from the gate's own elements, cut by their
> own means: `N4L` at 56 ms to the dit, `VA3VRR` at 100. The five tests that read
> those elements watched `Report.ElementsSeen` tick once per gate edge, and that
> clock is gone, so all five were deleted with it. **The rulings stand — they are
> in the record and nothing here contradicts them — but nothing in the suite
> would now notice if they stopped being true.**
>
> `KeyingEnvelope` is kept by ruling, is the independent witness, and those tests
> already used it for pitch and for the noise floor. Rebuilding the extraction on
> it is a session's work and it produces **new numbers for an old finding**, which
> is adjudication and is not a session's to do alone (§12.5).
>
> **Rejected: keeping the five tests failing** as a reminder, which is a permanent
> red that means nothing. **Rejected: re-pointing them at the resolved-element
> count**, which is a count of what came out rather than of what went in and would
> answer a different question in the same test's name.

### Asks still outstanding

Carried per HM-DEC-139, verbatim until ruled.

- **Whether the sidecar's `text` should include the leading edge.** First made
  2026-08-20. Tim's. The change is not in the tree; `Transcript.PlainText` still
  holds settled characters only.
- **The evenings' captures from the 20th and 21st are not in the tree.** First
  made 2026-08-20. Waiting on the files.
- **Thirty seconds since the last character, for mode-follow's guard.** First made
  2026-08-21. The figure is in `src` and unruled.
- **Whether `RfGain`'s hundred per cent is a defect or the right answer.** First
  made 2026-08-21. Waiting on an evening at the rig.
- **The likelihood gate at 15.0.** First made 2026-08-21. Waiting on an evening at
  the rig; the number is in `CwProbabilisticDecoder`.
- **The keying meter's provisional thresholds.** First made 2026-08-20. In
  `CwKeyingThresholds`, marked provisional.
- **HM-OPEN-052**, **HM-OPEN-053**, **HM-OPEN-054.** 053 and 051 now describe
  deleted code.
- **HM-DEC-130**, a callsign too long for one keyer send. Waiting on a dummy-load
  evening.
- **HM-DEC-098**, whether an attended automatic cycle may reach an antenna.
  Waiting on a dummy-load evening.
- **HM-OPEN-033**, the cold-start bin choice.
- **HM-OPEN-007**, open since 2026-08-14.
- **The mid-character interlock**, first made today, above.
- **The mark-and-gap witness behind HM-DEC-144 and HM-DEC-145**, first made today,
  above.

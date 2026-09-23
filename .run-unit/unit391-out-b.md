
**TheAdjudicatedReadingsKeepReadingTests - 13 of 13 green, 364 s.** The eight required anchors
were all found: `VA3VRR`; `MP/4 QNIK` (reads `AA4MP/4 QNIK` whole); `DE KD0UN KD0UN K`;
`N HANDLING THIS MESSAG`; `, AND`; `110, AND 110 WITH A MEAN OF 117`; `R OTHER WEBSITES MENTI`;
and the fact `TheShortfallIsPrintedRatherThanPapered`. The five retired readings passed as
retired; two of their anchors appear (`INT`, `OPAGATION`) and three don't (`N4`, `DICTED 10.7`,
`ULLETIN CAN BE FO`).

**CwFixtureTests.TheCleanRecordingsDecodeExactly - 0 of 2, 10 s.** `clean-12wpm` and `clean-18wpm`
both read `""` against `CQ DE W1AW K`.

**Against `docs\unit239-failing-set.txt` (2026-09-03):** it lists 2 capture cases (`001520`,
`013637`) and both synthetics, all four red here. **The other 16 red capture cases are not on
that list.** That's a finding, not chased (section 9).

### Every walked commit, in one line each

In `docs/phase-cw/unit391-walk.md`, 85 rows. The clean synthetics were red at all 84 commits
above 07f0397a, in three stages: wrong letters from `8e3ee277` (08-21), unsure marks only from
`07260a2a` (08-25), and nothing from `43efc525` (09-03). **Probe at `7e209cb4` (08-25 14:54, the
floors' own commit):** captures **36 of 36** in 97 s, adjudicated **13 of 13** in 37 s; the
synthetics are 0 of 2 at `ca252057`, the same decoder source. Whether any newer commit is green on
captures was not measured.

### 0.4 - the seams

30 files have a `using Hamlet.RadioEngine.Cw` and name one of the 102 types declared under
`src\Hamlet.RadioEngine\Cw`: 9 under `src\Hamlet.App` and 21 under `tests\Hamlet.App.Tests`.
Twenty-seven files matched only the word `Outcome` with no Cw using, and `AchievementsViewModel.cs`
named Cw types in a comment. I dropped those 28 as name-only.

**What doesn't exist at 07f0397a**, from the right-hand column below. 12 rows name a type absent
there, 4 of them the ambiguous `Outcome`. The real gaps are all in `MainWindowViewModel`,
`ScanViewModel` and two app tests:
- **Types absent:** `CwProbabilisticDecoder`, `CwProbabilisticStream`, `CwElementPitch`,
  `CwPitchChoice`, `CwStreamSplit`, `Envelope` and `CwAccuracy.Outcome`.
- **`CwDecoder` members absent:** `AssertStation`, `AssertAt`, `Lock`, `Unlock`, `IsLocked`,
  `LockedToneHz`, `PitchWasAsserted`, `Ranked`, `Reading`, `Retuned`, `LeadingEdge`,
  `ListeningAfresh`, `DigitalMode`, `DecodingSuspended`, `RadioIsTransmitting`,
  `DecodeQueueDroppedChunks` and `DecodeQueueDroppedSamples`.
- **`CwCharacter` members absent:** `MarginLlr`, `MarginShareForRecord`,
  `SpanLogLikelihoodRatio` and `WidestRecordedLlr`.

Every transmit-side type the app names (`CwTransmitter`, `KeyerCwSender`, `TransmitChain`,
`TransmitReadiness`, `TransmissionWatch`, `TransmitNotes`, `ICwSender`, `AutoCall*`) exists at
07f0397a with every member listed. The members column is a grep, not a compiler (method in the
doc's head); step 1's build is the real check.

| File | Type | Members touched (grep) | At 07f0397a |
|---|---|---|---|

READ IN THIS ORDER.

A. The phase goal - Hamlet works Olivia the way it works PSK31. Steps 0,
   1 and 2 done and closed; step 3 was partial at the start of this unit
   - 3.0 met, 3.1 engine half, 3.6 measured on the engine - and is seven
   of seven met on this report's evidence, for the arbiter to mark done;
   steps 4-6 not started, and step 4's entry opens only when step 3 is
   done, which it is on this report if the arbiter marks it so.
B. Step 3's criteria this unit worked - 3.1 rows: rows 2 (2), 8/250 at 1000
   Hz and 16/500 at 2000 Hz (5 Hz), CERs 0.0000 and 0.0000 (0.05), the
   other callsign 0 and 0 (0), most rows at once 2. 3.2: transcripts 8
   (32 row readings, 31 parser verdicts), verdict differences 0 (0),
   parser diff none. 3.3: CQ filter, worked-fade, EntityOf and CQ guard,
   quill, hover - same each, code diff none in any of their files or
   members (the row gains Variant and HasVariant, named below). 3.4:
   factor 56 (24 raised: the -16 dB 16/500 file goes 55.2 characters
   without a new accepted block while its station still sends), windows
   38.230 / 28.672 / 22.938 s, rows ended 2 of 2, mid-transmission
   retires 0 (0) on 8 files. 3.5: 21 row events of 7 PSK31 names, mode
   olivia on each and the variant on each row's, text or callsign 0 (0).
   3.6: ratio 0.212 at 8 kHz and 0.245 at 48 kHz (1.0) with rows drawn.
   Met: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, with 3.0 from unit 363. Not met:
   none. Send controls under Olivia with rows: keyed 0 (0); PttOn 1 (1),
   Arm( 2 (2).
C. The report last: section 4 raises 12 items on top of the carried
   queue; none stands in the way of a step 3 criterion. Drawing rows
   opened no path toward a send: every send still goes through
   SendMessage, whose mode gate refused all four presses that reached it
   (decision AN, item 3). No row showed a character not its own
   station's. No feature's code changed for Olivia rows; the row path took
   the variant field, the mapping and a mode tag on four row events (item
   4). The lag measured from the row is 5.84 s median, 5.97 s worst,
   2.92 blocks, every variant alike. Carry-forward at the end: engine 142
   in 4 m 46 s (286 s), app 179 in 2 m 27 s, against the 480 s timeout.
   Item 1 is for whoever runs the loop: while this unit ran, the harness
   ended it, and an arbiter wrote a PHASE_OUTCOME entry for it that is
   false on three fields.

UNIT:       364 - complete at task 5 of 6, task 5 built - 2026-09-19 14:31
PHASE GOAL: Olivia in Hamlet as PSK31 is - every station heard, read, answered and logged - with the variant always taken from the signal (its RSID, or measured off a carrier that never announced itself) and never picked by the operator.
UNIT GOAL:  Put the engine's Olivia listener on the screen under the Olivia tab as PSK31 is on it - a row per station through the PSK31 row path with its variant, the same parser and row features reading it unchanged, rows that retire on a window scaled by the variant's timing and stay listed as ended, the row telemetry with mode olivia - and nothing new able to reach a send.
ADVANCED:   yes - Olivia stations appear on the Olivia panel as rows for the first time, and the five must-passes step 3 still had open are met with measurements.
NUMBER:     step 3 criteria met 1 of 7 -> 7 of 7; Olivia rows on screen 0 -> 2
DRIFT:      0

| Criterion | This unit's evidence | State |
| --- | --- | --- |
| 3.0 8/250 at -14 dB, CER <= 0.10 | unit 363: CER 0.0000 on five seeds at a measured -13.82 dB; not re-worked | **met** (unit 363) |
| 3.1 two-signal file yields two rows, own text <= 0.05, nothing of one in the other | through the app's tick and tap: rows 2, most at once 2; 8/250 at 1000 Hz and 16/500 at 2000 Hz, each showing its variant; CER 0.0000 and 0.0000; EI4GNB 0 times on KC3QIS's row and KC3QIS 0 times on EI4GNB's | **met** |
| 3.2 corpus through Olivia rows, same verdicts, no parser change | 8 transcripts, 32 row readings and 31 parser verdicts compared field by field: 0 differences; `Psk31ExchangeParser.cs` no diff against `75571f5d` | **met** |
| 3.3 CQ filter, worked-fade, EntityOf and CQ guard, quill, hover on Olivia rows, no change to their code | the same outcome as the PSK31 row on all five; no diff in `DecodedFilter.cs`, `DxccPrefixes.cs`, `NudgeSet.cs`, `NudgeWords.cs`, nor in `WantsRow`, `ApplyDecodedFilter`, `WorkedBeforeNote`, `MarkIfItOpensSomething`, `RowOpacity`, `SenderHelp`, `WholeMessage` | **met** |
| 3.4 retired when the signal goes, listed as ended, window = timing table x a stated factor | factor 56 in `timing.json`; 16/500 ended 28.682 s after its last block (window 28.672), 8/250 still open; 8/250 ended 38.288 s after (38.230); both listed, ended, text and center unchanged; 0 retires on 8 shipped files | **met** |
| 3.5 PSK31 row events with mode olivia and the variant, nothing personal | 21 events of 7 PSK31 row-event names on the two-signal feed plus one finished line; every one mode olivia, every row's with its variant; no word of either station's text, no callsign, not the operator's | **met** |
| 3.6 real-time ratio on the two-signal file under 1.0 (nice-to-pass) | with rows drawn, the whole tick: 0.212 at 8 kHz, 0.245 at 48 kHz; longest single tick 0.130 / 0.141 s | **met** |

## 1. What Claude did

**Complete: tasks 0 to 5 of 6, all built, none dropped** - task 5, the drop candidate, included.
Machine: Tim's development box, `C:\Source\HamLet`, gate held (`SHACK_FACTS.md` and
`CwProbabilisticDecoder.cs` present, `CoreHMI.sln` and `MURC.sln` absent), branch `main`. Commits,
**every push succeeded**: `6c632d3d` (task 0), `cbcde914` (decision AL), `f10e045b` (task 1),
`70014dfb` (task 2), `2caf9107` (task 2, §R12 seam rewrite in its own commit), `4ce4cd9c` (task 2
fix), `15d6ea69` (task 3), `64cb9cc0` (task 4), `5a769cf1` (task 5), `1079966a` (decision AN's card
press); this report is committed after them. Status written at every task boundary, after commits
and before every `dotnet test`, `EXECUTING` and `code` throughout.

### Task 0 - the unit opens

`PHASE_STATUS.md`: steps 0, 1 and 2 `done`, **step 3 `partial`**, `CURRENT_STEP: 3`,
`WORK_INSTRUCTION: 358` (stale, as expected). **Step 3's entry:** `olivia-two-signals-rsid.wav`
hashes as its manifest says, and the RSID detector reads **OLIVIA_8_250 (69) at 1000.29 Hz and
OLIVIA_16_500 (70) at 1999.66 Hz**, 15 tones right each, first tones at 0.465 and 0.464 s. The other
eight hash, **9 of 9**, inside `TheRsidDetectorTests` on the carry-forward run. `UNIT 364 - STEP 3`
appended to `PHASE_OUTCOME.md`, no earlier entry touched; version 1.13.50 -> 1.13.51. Carry-forward
before any change: **app 166 of 166 (36 s), engine 134 of 134 (4 m 39 s)**. The engine line ran
**twice**: my first run's `grep` pattern missed the summary line under normal verbosity, so the count
was lost and I ran it again. That is three invocations at task 0, not two. Commit `6c632d3d`.
**Decision AL** in its own commit (`cbcde914`): the engine line takes
`TheOliviaBelowTheNoiseTests.TheQsoIsReadBelowTheNoise` by method; run once more, **133 of 133 in
3 m 12 s**.

### Task 1 - the trace, before anything is built

Items 1 to 3 read from the source, lines as at `cbcde914`. Items 4 and 5 measured by `Unit364Trace`
(engine, asserts nothing, in `CpuMeasuredAlone`, not on the carry-forward line).

1. **The rate.** `OliviaListener` takes any rate at construction, and everything behind it (the
   RSID detector, `OliviaStream`, the search) was proved at 8 kHz on the mode author's 8 kHz files.
   The audio tap hands over the device's rate (`AudioTap.SampleRate`, 48 kHz on a sound card);
   `Psk31Resampler(deviceRate)` brings any rate to `Psk31Resampler.TargetSampleRate` = 8000. The app
   already runs one for PSK31 (`MainWindowViewModel.cs:2895`) and a second for the RSID detector
   under Olivia (:3103). **Feeding the listener takes one more `Psk31Resampler` and nothing else**:
   no new resampler class, no package.
2. **The row path.** `ShowPsk31Channels(IReadOnlyList<Psk31Channel>)` at :3524 takes
   `Psk31Channel(Id, OffsetHz, StrengthDb, Text, Readable)` (`Psk31Listener.cs:21-22`) and builds a
   `DigitalDecodeRow` per channel (:3583-3599) - `Hz` from the offset, `Snr` from the strength (a
   dash where it is NaN), `Message` from the text, `IsTextOnly: true`, and
   `Reading: ReadPsk31(channel)` (:2743), which feeds new characters to a `Psk31MessageSplitter` and
   through it `Psk31ExchangeParser`, and writes `psk31_line_parsed`. A channel no longer listed goes
   through `EndOrRemovePsk31Row` (:2596): kept and marked `Ended` if it read anything, removed if
   not, with `psk31_row_ended`. The features read the row: **the CQ filter** is
   `DecodedFilterRule.Wants(cq, row.Addressee)` (`DecodedFilter.cs:48`, applied in
   `ApplyDecodedFilter` at :5910), `Addressee` on a text-only row coming from `Reading`
   (`DigitalDecodeRow.cs:729`, :754); **worked-fade** is `row.WorkedBefore = WorkedBeforeNote(row.Sender)`
   (:3610, :14581) and `RowOpacity` (`DigitalDecodeRow.cs:393`); **`EntityOf` and its `CQ` guard**
   is `DxccPrefixes.EntityOf(Sender)` in `SenderHelp` (`DigitalDecodeRow.cs:900`,
   `DxccPrefixes.cs:104`); **the quill** is the row's nudge, set by `MarkIfItOpensSomething(row)`
   (:3606, :13965); **the hover** is `WholeMessage` (`DigitalDecodeRow.cs:787`). Every one reads
   `Sender`, `Addressee` or `Message`, so a row built by the same code from an Olivia channel gets
   them with no change. **Where a click reaches a send**: every send - the card's Answer, Report and
   Confirm (:5539, :5548), the macros (:15413), the typed line (:15553) and the CQ press (:16933) -
   goes through `SendMessage` (:14963), and its mode gate at :14989, `CanTransmitIn(ChosenDigitalMode)`
   (:2431), answers false for Olivia and refuses before either `Arm(` site (:15135, and :15334 inside
   `SendPsk31`, which only `SendMessage` calls after the gate). **That gate is the whole of 0.5's
   refusal, and decision AN's risk is exactly that it stays the only door**: drawing rows adds a card
   under Olivia wherever a station certainly calls the operator (`ShowPsk31Cards`, :3701), and every
   control on that card goes through the same door. **A second exposure, not a send**: `ReadPsk31`
   books a certain message addressed to the operator into the contact ledger with `RecordPsk31`
   (:2790). On an Olivia row that books an Olivia message with no mode on it. It keys nothing; it is
   reported in section 4.
3. **The PSK31 row events the app writes**, all in category `Psk31`, from `NotePsk31` (:3377) unless
   named: `psk31_listening_started` (:2906; dialHz, passband, sampleRate, deviceSampleRate,
   resampleRatio, squelchQuality, retirePasses, retireSeconds, retireRule, searchRule);
   `psk31_carrier_appeared` (carrierId, offsetHz, strengthDb, quality, passesAsCandidate);
   `psk31_carrier_retired` (carrierId, offsetHz, reason, lifetimeSeconds, charactersEmitted,
   linesParsed, secondsSinceLastCharacter; also from `ForgetPsk31` at :3979); `psk31_carrier_idling` /
   `_typing`; `psk31_search_pass`; `psk31_squelch` (offsetHz, open, quality, threshold);
   `psk31_reading` (offsetHz, reading, afcHz); `psk31_audio_level`; `psk31_line_parsed` (from
   `ReadPsk31`, :2763; offsetHz, kind, certain, turnover, toOperator, characters);
   `psk31_row_ended` (from `EndOrRemovePsk31Row`, :2627; offsetHz, characters, lines,
   lifetimeSeconds); `psk31_row_cleared`; `psk31_listening_stopped` (:4000). The capture events are
   the button's, not the rows'.
4. **The gaps** (every Olivia file in the manifest through the listener, quarter-second pieces, then
   flushed). Every variant's block is 2.048 s. Seconds a character from `timing.json`: 8/250 0.68267,
   16/500 0.512, 32/1000 0.4096; **24 times that: 16.384 s, 12.288 s, 9.830 s.**

   | File | Channel | Longest gap between accepted block ends | Longest the listener went with no new accepted block while he was still sending | Smallest whole factor that holds | Last accepted block to file end |
   | --- | --- | --- | --- | --- | --- |
   | 8/250 CQ | 8/250 rsid | 2.048 s = 3.0 ch | 8.142 s = 11.9 ch | 12 | 0.016 s |
   | 16/500 QSO | 16/500 rsid | 2.048 s = 4.0 ch | 8.142 s = 15.9 ch | 16 | 0.016 s |
   | 32/1000 QSO | 32/1000 rsid | 2.049 s = 5.0 ch | 8.142 s = 19.9 ch | 20 | 0.016 s |
   | 8/250 no-RSID | 8/250 blind | 2.048 s = 3.0 ch | 6.182 s = 9.1 ch | 10 | 0.016 s |
   | 16/500 -10 dB | 16/500 rsid | 2.048 s = 4.0 ch | 8.143 s = 15.9 ch | 16 | 0.013 s |
   | **16/500 -16 dB** | 16/500 rsid | **22.528 s = 44.0 ch** | **28.242 s = 55.2 ch** | **56** | 16.400 s |
   | two-signal | 16/500 rsid | 2.048 s = 4.0 ch | 8.142 s = 15.9 ch | 16 | **6.160 s** |
   | two-signal | 8/250 rsid | 2.048 s = 3.0 ch | 8.142 s = 11.9 ch | 12 | 0.016 s |
   | noise only | none | - | - | - | - |

   The middle column is what the retire is judged on, because the listener only learns of a block
   when it reaches the channel: on the clean files it is the wait from the burst's end to the first
   block reaching the channel. **At 24 the clean files hold (32/1000 by 1.7 s), and the -16 dB
   file's channel would be retired while its station was still sending.** By decision AJ's own rule
   the factor goes to **56**, the smallest whole number that holds on every shipped file, and the
   windows become 38.23 s / 28.67 s / 22.94 s. The -16 dB file is the one the plan says is below
   16/500's sensitivity (2.2, revised 2026-09-19): its reader shows 9 blocks of 77.
5. **The lag, from the row's side** - when each accepted block ended in the audio and when its
   text reached the channel:

   | File, channel | Accepted blocks | First block ends / reaches the channel | Median lag | Worst lag |
   | --- | --- | --- | --- | --- |
   | two-signal, 8/250 | 13 | 4.386 s / 10.250 s | 5.816 s | 5.970 s (2.92 blocks) |
   | two-signal, 16/500 | 10 | 4.386 s / 10.250 s | 5.864 s | 5.970 s (2.92 blocks) |
   | 16/500 QSO | 63 | 4.386 s / 10.250 s | 5.838 s | 5.972 s (2.92 blocks) |

   Three of the 8/250 channel's blocks and three of the QSO's reached it only at the flush, which a
   recording's end makes and the air does not: on the air those arrive as later audio pushes them
   through.

### Task 2 - the rows (3.1 rows half, 3.5)

- **Decision AE.** `HearOlivia` (`MainWindowViewModel.cs`, beside `HearRsid`) reads the tap as
  `HearPsk31` does, through its own `Psk31Resampler` to 8 kHz, into one `OliviaListener` over
  200-3000 Hz, and is called in the tick's Olivia branch only, after the capture and the RSID
  detector. Leaving the Olivia tab calls `ForgetOlivia` (written ended, stopped, rows ended as
  PSK31's are), from `OnChosenDigitalModeChanged`, which until now left an Olivia path alive on a
  press of PSK31 because it only forgot PSK31 when leaving PSK31.
- **Decision AF.** `ShowOliviaChannels` maps each live channel to `Psk31Channel(Id, center, NaN,
  Text, Readable: BlocksDecoded > 0)` and calls `ShowPsk31Channels(channels, variants)`. The row
  gained `Variant` and `HasVariant`, drawn in a leading `Auto` column of the text-row grid, hidden and
  zero-width on every other row. An ended channel is left out of the list, so the PSK31 path ends its
  row as it ends a carrier's.
- **Decision AG.** `OliviaChannel` gained `ShownCenterHz`, the center as of the last accepted
  block. The row shows it, and shows the tracked center only before the first block. **The strength
  cell is a dash**: the listener measures a block's S/N in its own units, not decibels in 2500 Hz.
- **Decision AK.** The PSK31 row events, with `mode: olivia`, and the variant on each row's:
  `psk31_carrier_appeared`, `psk31_squelch` (last block S/N against Olivia's threshold 4),
  `psk31_reading`, `psk31_line_parsed`, `psk31_row_ended`, `psk31_row_cleared`,
  `psk31_carrier_retired`, `psk31_audio_level`. The listener's own start and stop are
  `olivia_listening_started` / `_stopped` (a decision, below). `olivia_channel` and `olivia_block`
  are unchanged.
- **Decision AM.** The strip says `ListeningAcrossThePassband("Olivia")`, PSK31's sentence (still
  `NotYetReadable` behind a file that failed to read). The decoded-idle line (`DigitalIdleText.DecodedOlivia`)
  says a line per station, a few characters a block, and keeps its Capture sentence. The waterfall
  caption says PSK31's `Olivia, one continuous carrier a station` where it said `not read yet`.
  **`TheOliviaSeamTests` was rewritten under §R12 in its own commit** (`2caf9107`): three
  assertions guarded the shut door and went red when rows were drawn - the strip's *cannot read
  Olivia yet*, *no row* after the RSID burst, and *no `psk31_` event at all* - and now guard 0.5:
  the strip says nothing can be answered and `CanAnswerRowsForTests` is false, every row after the
  burst is an Olivia row and no card opens, and every `psk31_` event under the tab carries `mode:
  olivia`. 11 of 11.
- **`TheOliviaRowsTests` watched failing first** against stub seams: **5 of 6 red**, the noise test
  green against it as a listener that hears nothing draws nothing. Then 6 of 6. Every file
  hash-checked; each fed through the real tick a quarter-second at a time, then four blocks of the
  slowest variant (8.192 s) of seeded noise at RMS 0.001, because the app never flushes and the
  last blocks reach a channel only as later audio pushes them through.
- **The carry-forward, first run with the rows: app 171 of 172.** The red was
  `TheCaptureButtonTests.PressingUnderOliviaWritesTheWavAndTheEventsWithModeOlivia` - **this unit's
  doing, not a flake**: it reads `psk31_listening_started` under the Olivia tab as *a PSK31 listener
  supplied the capture*, and the Olivia listener's start had taken that name. Renamed
  (`4ce4cd9c`), the test not edited. Then **app 172 of 172 (2 m 8 s)**, **engine 133 of 133 (3 m 15 s)**.

### Task 3 - the parser and the row features (3.2, 3.3)

`TheOliviaRowsReadLikePsk31Tests`, **app project**, because the seams are there: an Olivia row by
`ShowOliviaChannelsForTests` (added, the Olivia twin of `ShowPsk31ChannelsForTests`), a PSK31 row by
`ShowPsk31ChannelsForTests`. **Watched failing first against a stub that drew no Olivia row: 6 of 6
red**, then removed. The first real run was 5 of 6: my CQ-filter test expected a non-CQ row dropped,
and **on a PSK31 row it is not** (item 5), so the test now holds the Olivia row to wherever the
PSK31 row lands. 6 of 6. **No source changed in this task.**

### Task 4 - the retire (3.4)

The factor is **56**, in `data/olivia/timing.json` as `retire_after_characters`, with a
`retire_about` saying why. `OliviaTiming.RetireAfterCharacters` reads it strictly, and
`RetireWindowSeconds(variant)` is the variant's seconds per character times it: **38.230 s, 28.672 s,
22.938 s**. `OliviaListener`, given the table (the app passes `OliviaData.Timing`; the engine tests
that pass nothing retire nothing, as before), retires in `Add` any channel whose `SamplesSeen` has
passed its last accepted block's end, or where it was read from, by more than the window. A retired
channel is ended, fed nothing more, and flagged `Retired`, with an `olivia_channel` event `retired`
carrying `windowSeconds`, `retireFactor` and `lastBlockEndSeconds`. The app writes
`psk31_carrier_retired` with reason `SignalGone`, the window and the factor. **Watched failing first
against stubs**: the derived-window test red and the app retire test red (*8/250 was never retired*).
The eight no-retire rows were green against the stub trivially, since it retired nothing, and got
their real test once the retire existed. Then engine `TheOliviaRetireTests` and
`TheOliviaDataTests` 15 of 15, app `TheOliviaRowsTests` 7 of 7. `TheOliviaRetireTests` joined the
engine line (it adds about 85 s, and the line stays under the 400 s the instruction sets).
**Carry-forward: engine 142 of 142 in 4 m 46 s; app 178 of 179 in 2 m 27 s.** The red,
`TheTestsStayOffTheNetworkTests.The354LayoutReadsTheSameNumbersTwiceRunning`, **passed alone on the
first rerun**. It realizes three whole windows twice and compares every box. This unit changed that
layout only inside the decoded-row template (a zero-width column on non-Olivia rows), and none of
its windows draws a text row. The app number is the list run's, 178 of 179.

### Task 5 - the real-time ratio with the rows drawn (3.6)

`TheOliviaRowsKeepUpTests`, app, in a `CpuMeasuredAlone` collection defined there (the app
assembly already runs nothing in parallel), **not on the carry-forward line**. Process CPU around the
whole tick - capture check, the RSID detector, the listener, the mapping, the row path, the events -
over the file and its quiet, rows drawn 2: **0.212 at 8 kHz, 0.245 at a 48 kHz device**, both
asserted under 1.0.

**Afterwards, decision AN's test was strengthened** (`1079966a`). Its first version pressed a card
whose Report had no text, because the test's settings had no name or location (PSK31's rule in
`MacroTextFor`), so no card button was ever pressed. With a name and place set, the card offers
Report with action Send, and the press is refused at the door. `TheOliviaRowsTests` 7 of 7, run by
name. **The last full app list run predates this test-only change.**

### Decisions this session made for itself, author's and overrulable

- **Factor 56**, by decision AJ's own rule (item 2).
- **The quiet after each file in the app tests**: four blocks of the slowest variant of seeded
  Gaussian noise at RMS 0.001 - not digital silence, whose noise measure would be exactly nothing -
  and for the retire test the longest window plus those four blocks.
- **The Olivia listener's start and stop are `olivia_listening_started` and
  `olivia_listening_stopped`**, not PSK31's names: they are not row events, and
  `psk31_listening_started` means *a PSK31 listener started* to a test §10 forbids me to edit.
- **The row's strength is a dash** (decision AG's mapping): Olivia's block S/N is not dB in 2500 Hz.
- **The row's variant is drawn first in the text-row grid**, muted, in its own column.
- **The decoded-idle sentence for Olivia** is new wording (DecodedOlivia): the old one was false
  once rows were drawn, and PSK31's names PSK31. The strip and the waterfall caption reuse
  PSK31's sentences with the mode's name.
- **Synthetic Olivia channels through the tick's own seam** in the AN and events tests. The
  fixture's CQs never finish a line (item 6), so a finished CQ, a line to the operator and one to
  another station were given through `ShowOliviaChannelsForTests`, from the mapping on.
- **`TheOliviaRetireTests` on the engine line** and `TheOliviaRowsReadLikePsk31Tests` on the app
  line; `Unit364Trace` and `TheOliviaRowsKeepUpTests` on neither.

### Verified against the tree - mismatches with section 5 of the instruction

- **None of substance in section 5's list.** HEAD `eb714fa4`, 1.13.50; `output.md` unit 363's,
  committed; `PHASE_STATUS.md` steps 0-2 `done`, step 3 `partial`, `CURRENT_STEP: 3`,
  `WORK_INSTRUCTION: 358`; `PHASE_OUTCOME.md` ending on `UNIT 2 - STEP 3`; the nine Olivia engine
  files; `OliviaListener`'s shape as stated; the PSK31 row path at the lines stated (±0); the corpus
  and its readers; `timing.json` at 0.683 s a character (0.68267); carry-forward 134 / 166 with the
  names as stated; `PttOn` 1 and `Arm(` 2; `assets\fixtures\captured\` holding only `README.md`.
- **Decision AH / task 3's wording: *the CQ filter keeps a CQ row and drops a non-CQ one*.** On a
  PSK31 row it drops nothing (item 5).
- **Section 1's cost note** (*every engine class that feeds a whole fixture costs about as much as
  `TheOliviaListenerTests`*) held: `TheOliviaRetireTests` added about 85 s.
- The expected mismatches were not rediscovered; none was edited.

## 2. What the owner should expect

- **Press Olivia and the stations on the air appear as lines**, one per station, each starting with
  its variant (`8/250`, `16/500`), in the same list PSK31 lines use, filling in a few characters at a
  time. A station that announced itself shows *heard, not readable yet* until its first block is
  read.
- **Text arrives about six seconds after it was sent** - three blocks - on every variant. That is
  the reader waiting for its frequency track to settle, and it is measured, not a fault.
- **A line goes grey and says `ended`** once its station has been quiet 56 characters' worth of its
  variant: 38 s at 8/250, 29 s at 16/500, 23 s at 32/1000. It stays on the list with its words, as
  PSK31 lines do.
- **The Answer, Report, Confirm, CQ and typed-line buttons under Olivia still send nothing** and say
  *Hamlet cannot send Olivia yet*. Step 4 opens them.
- **What will look wrong but is not:**
  - The strength column on an Olivia line is a dash, and the strip's sentence (PSK31's) still says
    each line shows *how strong it is* (item 8).
  - A station's last line in an over - `... pse K` - is not read as a CQ, and cannot be clicked,
    until whatever it sends next arrives (item 6).
  - With CQ only on, Olivia lines that are not CQs stay on the list, exactly as PSK31 lines do (item 5).
  - The engine carry-forward run takes 4 m 46 s and the app run 2 m 27 s.
  - `PHASE_STATUS.md`, `PHASE_OUTCOME.md` and `WORK_INSTRUCTIONS.md` show uncommitted changes this
    session did not make (item 1).

## 3. What you should see

**The Olivia tab with the two-signal recording fed** (computed through the app's own tick, not seen
on a screen; nothing here is evidence about the radio, FACT-004):

| Variant | Center | Text | State |
| --- | --- | --- | --- |
| 8/250 | 1000 Hz | `CQ CQ CQ de KC3QIS KC3QIS KC3QIS pse K` | live |
| 16/500 | 2000 Hz | `CQ CQ CQ de EI4GNB EI4GNB EI4GNB pse K` | live |

**And after the retire window** (the file, then quiet): the same two lines, the same text and the
same centers, both greyed and marked `ended`. The 16/500 line ended at 51.50 s of audio while the
8/250 line was still open, and the 8/250 line at 67.25 s.

**3.2 - the corpus through both rows**: 8 transcripts, 32 row readings after each line and 31
parser verdicts compared, every field: **0 differences**.

**3.3 - the features** (the same text on a PSK31 row and an Olivia row):

| Feature | PSK31 row | Olivia row | Code diff against `75571f5d` |
| --- | --- | --- | --- |
| CQ filter (CQ only on) | CQ shown; line to W9ZZZ shown; line to the operator on his side; unfinished line shown | the same, each | none (`DecodedFilter.cs`, `WantsRow`, `ApplyDecodedFilter`) |
| worked-fade | 0.55, tip as worked | 0.55, the same tip | none (`WorkedBeforeNote`, `RowOpacity`) |
| `EntityOf` and the `CQ` guard | TI2ABC -> Costa Rica; `CQ` refused | the same; *Who sent it. TI2ABC is a callsign from Costa Rica.* | none (`DxccPrefixes.cs`, `SenderHelp`) |
| quill | Visible, *Costa Rica · new country*, lift 1 | the same | none (`MarkIfItOpensSomething`, `NudgeSet.cs`, `NudgeWords.cs`) |
| hover | whole message under *TI2ABC · time* | the same body, byte for byte | none (`WholeMessage`) |

The parser, `Psk31MessageSplitter.cs` and `Psk31ExchangeParser.cs`: no diff. The row path's own
changed lines are listed in item 4.

**The retire table:**

| Variant | Seconds a character | Factor | Window | Longest the listener went with no new block while sending, any file |
| --- | --- | --- | --- | --- |
| 8/250 | 0.68267 | 56 | 38.230 s | 8.142 s (11.9 ch) |
| 16/500 | 0.512 | 56 | 28.672 s | **28.242 s (55.2 ch), the -16 dB file**; 8.143 s on the others |
| 32/1000 | 0.4096 | 56 | 22.938 s | 8.142 s (19.9 ch) |

**The row events as written** (two-signal feed, the first of each kind; start and stop since renamed
`olivia_listening_*`):

```
psk31_carrier_appeared {"carrierId":2,"offsetHz":1000.3,"strengthDb":null,"quality":null,"passesAsCandidate":null,"found":"rsid","mode":"olivia","variant":"8/250"}
psk31_squelch {"offsetHz":1000.3,"open":false,"quality":0,"threshold":4,"mode":"olivia","variant":"8/250"}
psk31_reading {"offsetHz":1000.3,"reading":false,"afcHz":0,"mode":"olivia","variant":"8/250"}
psk31_line_parsed {"offsetHz":1500,"kind":"Cq","certain":true,"turnover":true,"toOperator":false,"characters":32,"mode":"olivia","variant":"16/500"}
psk31_row_ended {"offsetHz":1000,"characters":38,"lines":0,"lifetimeSeconds":33.7,"mode":"olivia","variant":"8/250"}
psk31_carrier_retired {"carrierId":1,"offsetHz":2000,"reason":"SignalGone","lifetimeSeconds":48,"charactersEmitted":38,"linesParsed":0,"secondsSinceLastCharacter":22.8,"mode":"olivia","variant":"16/500","windowSeconds":28.672,"retireFactor":56}
```

**The lag, from the row's side** (`Unit364Trace`): first block ended 4.386 s and reached its channel
at 10.250 s on every file; median 5.816 s (8/250) and 5.864 s (16/500) on the two-signal file, 5.838 s
on the 16/500 QSO; worst 5.970-5.972 s, 2.92 blocks.

**The send controls under Olivia, two rows present plus two given through the seam:**

| Control | What happened |
| --- | --- |
| click on the fixture's 8/250 and 16/500 rows (Answer) | nothing: no finished CQ on them (item 6) |
| click on a finished Olivia CQ (Answer) | `send_requested`, `send_refused` - *Hamlet cannot send Olivia yet* |
| right-click (open a card) | nothing: `Psk31StationOn` answers only under PSK31 |
| CQ | `send_requested`, `send_refused` |
| the card's Report (action Send) | `send_requested`, `send_refused` |
| the card's typed line | `send_requested`, `send_refused` |

Four requested, four refused; no `send_stage`, composed, armed, keyed, `ptt` or `rsid_sent` line.
`PttOn` code lines 1 (`Ft8TransmitSequence.cs:513`), `Arm(` lines 2; no diff under `Transmit\`, in
`Psk31Modulator.cs` or `RsidBurst.cs`.

**The real-time table** (the whole tick, rows drawn):

| Device rate | Audio | CPU | Ratio (1.0) | The file alone | Longest single tick |
| --- | --- | --- | --- | --- | --- |
| 8 kHz | 37.00 s | 7.859 s | **0.212** | 0.212 | 0.130 s, the piece ending 3.50 s |
| 48 kHz | 37.00 s | 9.047 s | **0.245** | 0.246 | 0.141 s, the piece ending 3.50 s |

Unit 363's listener alone: 0.154. The piece at 3.50 s is where both RSID channels open and take
their replay.

## 4. What's blocking us

**Nothing blocks step 3 or step 4's entry.** Twelve new items. Item 1 is for whoever runs the loop;
the rest are findings, and none wants a ruling from the owner. The carried queue follows them.

### Raised by unit 364

**1. The harness ended this unit while it ran, and an arbiter wrote over the loop's files.**

*A finding about the loop, not the code; nothing here was edited or committed by this session.* At
about 14:05, with this unit at task 4, something outside the session rewrote three files:
- **`WORK_INSTRUCTIONS.md`** now reads *No unit authored - the arbiter stops: unit 364 is still
  running*, and tells this unit its instruction is at `6c632d3d`.
- **`PHASE_OUTCOME.md`** has a new last entry, `UNIT 3 - STEP 3`, with `FATE: executed`, `COST:
  17.930809499999995` (unit 363's) and `STATE_AFTER: partial`, judged from this report's task-1
  draft. The arbiter's own note says all three fields are false, and this report agrees.
- **`PHASE_STATUS.md`** lost its `HEARTBEAT` line.

This session committed `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `WORK_INSTRUCTIONS.md` at task 0,
as the instruction asked, and has not committed their later changes. `SESSION.lock` and
`.run-unit\` were never committed.

**2. The retire factor is 56, not 24.**

*A number, raised by decision AJ's own rule; the arbiter's to overrule.* The retire is judged on
what the listener knows, and a block reaches a channel about six seconds after it ends. So the
longest a sending station goes without a new accepted block, as the listener sees it, is 8.14 s on
every clean file: the wait from the burst to the first block. At 24 the clean files hold, 32/1000 by
1.7 s. **The -16 dB 16/500 file goes 28.24 s, 55.2 characters**, because its reader shows 9 blocks
of 77. So 24 would have ended that row while the station was still sending. At 56 it holds by
0.43 s. That file is the one the plan's revised 2.2 calls below 16/500's sensitivity. **A station
that stops is listed as live for 38 s at 8/250, where 24 would have given 16 s.**

**3. Decision AN held, and here is why.**

*A finding, for step 4.* Every send in the app goes through `SendMessage`, and its mode gate
(`CanTransmitIn`, `MainWindowViewModel.cs:2431`) refuses Olivia before either `Arm(` site. Drawing
rows added two ways to reach that door under Olivia: Answer on a finished CQ, and a card, which opens
where a station certainly calls the operator. Both reached it and were refused. **Step 4 opens the
door for Olivia at that one gate**, and every row control will then be live at once.

**4. The row path's changed lines, named (decision AF).**

*For the record of what the one path took.* `DigitalDecodeRow`: `Variant`, `HasVariant`.
`ShowPsk31Channels`: an optional `variants` map, `var variant = ...`, `&& shown.Variant == variant` in
the unchanged-row check, and `Variant = variant` on the new row. Four row events gain a mode tag:
`ReadPsk31`'s `LineParsed`, `EndOrRemovePsk31Row`'s `RowEnded`, and the two `RowCleared` calls, each
passing `OliviaTagFor(...)`, which is null on a PSK31 row. `AudioSecondsHeard` falls back to the
Olivia listener's clock. `MainWindow.axaml`'s text-row grid gained a leading column, so its four
other columns moved by one. **No line in the parser, the splitter or any 3.3 feature.**

**5. The CQ filter holds back no PSK31 or Olivia row - the instruction expected it to drop a non-CQ
one.**

*A mismatch with the tree, reported, not repaired.* `WantsRow` (`MainWindowViewModel.cs:2138`, unit
337): *the squelch is the only gate on a PSK31 row*; the CQ toggle applies to FT8 rows only, and a
line to the operator goes to his side. Olivia rows get exactly that. 3.3 asks for the same outcome
with no code change, and it is the same.

**6. An over's last line is not read until the next character arrives.**

*A finding, shared with PSK31.* The splitter closes a message on the character after the turnover.
The two-signal file's CQs end `pse K` with nothing after them, so on that file no line is parsed,
neither row is a clickable CQ, and `psk31_line_parsed` needed a line given through the seam. On the
air the station's next character, or the next station's, closes it.

**7. A certain message to the operator on an Olivia row is booked in the contact ledger with no
mode.**

*A finding for step 5, not a send.* `ReadPsk31` books such a message through
`Ft8ContactLedger.RecordPsk31` (`MainWindowViewModel.cs:2790`) for PSK31 rows, and now for Olivia
rows. The ledger carries no mode. It keys nothing. Logging Olivia with its submode is step 5's.

**8. An Olivia row's strength is a dash, and the strip's reused sentence promises one.**

*Wording, the arbiter's.* The listener's figure is a block's S/N in its own units, which is not
comparable to PSK31's dB in 2500 Hz, so the cell is honest and empty. The strip now says PSK31's
*... with where it sits, how strong it is and its text as it arrives*. That is true for PSK31 and
not for Olivia's strength.

**9. The tracked center wanders after a station stops, up to 19.5 Hz, until the retire.**

*A finding; decision AG's shown center hides it from the row.* On the two-signal file, after its last
block, the 16/500 channel's track moved to 2019.53 Hz and the 8/250's to 996.09 Hz. The rows showed
2000 and 1000 throughout, and nothing moved after the retire. The lag is 5.84 s median, the same on
every variant, because every block is 2.048 s.

**10. Two RSID detectors run under Olivia.**

*A cost, reported.* `HearRsid` (step 1's `rsid_heard`, criterion 1.6) and the listener's own
detector both run on the same audio. The ratios in section 3 include both. Folding `rsid_heard`
into the listener would save about 0.08 of real time, and would change a step 1 event's source.

**11. R27's across-tab half is logged, not built (decision AE).**

*For the plan's author.* An RSID heard under PSK31 or FT8 switches nothing; the Olivia listener runs
under the Olivia tab only.

**12. Tool facts and runs this session.**

*Reported, not repaired.*
- The engine line ran **three times at task 0**: my first `grep` pattern missed the summary line
  under normal verbosity. Every later filter kept `Passed!|Failed!`.
- `sed -i` on `output.md` and a `>` redirect into the root were refused as *outside the allowed
  working directories*. **So unit 363's sections 1-3 were removed with the file editor**, and the
  carried queue below is the original bytes: `git show 75571f5d:output.md | tail -n +338 | md5sum`
  gives `15c07dbd758513f633cdc449c55a332b`, and so did this file's carried block at task 1
  (`tail -n +127`) and in the final report (`tail -n +509`).
- `grep -o` with a quantifier, `git config` and `grep -v` in a pipe needed approval; `grep -n` on a
  `.trx` and `sh tools/status.sh ... && dotnet test ... | grep -E` ran.
- `TheTestsStayOffTheNetworkTests.The354LayoutReadsTheSameNumbersTwiceRunning` went red once in the
  list run and was green alone on the first rerun (task 4).
- `tools/status.sh` still writes `RULES_AT: HM-DEC-161` and `WORK_INSTRUCTION: 358`.

### Asks still outstanding - carried from unit 363's section 4, per HM-DEC-139, verbatim

**Nothing blocks step 3.** Nine new items, all findings; none wants a ruling from the owner. The
carried queue follows them.

### Raised by unit 363

**1. 3.0 was already met before this unit changed anything.**

*A finding, for the record of what advanced the phase.* The trace read decision W's audio at CER
0.0000 on all five seeds through `OliviaDemodulator.Decode` exactly as unit 362 left it: sync S/N
about 9.7, weakest accepted block 7.31 against the threshold's 4.0. So the below-noise claim at
8/250 is the mode's and the existing demodulator's, and this unit's contribution to 3.0 is the
fixture and the test. Unit 361 item 1's improvements were not needed and not tried.

**2. The engine carry-forward invocation now takes 4 m 40 s, and one of its names asserts nothing.**

*A cost, reported.* 124 tests in 1 m 39 s became 134 in 4 m 40 s: every listener row runs the RSID
detector, the streaming search and its readers over a whole file, and `CpuMeasuredAlone` runs those
classes one at a time. It is inside the 480 s timeout and the twelve-minute watchdog, with about three
minutes to spare. **`TheOliviaBelowTheNoiseTests.TheFurtherSeedsArePrinted` is on the line only
because the instruction named the class**, and it asserts nothing - the list's own rule keeps such
names off (`Unit337Measure`). It costs about 85 s. The type-and-method form,
`TheOliviaBelowTheNoiseTests.TheQsoIsReadBelowTheNoise`, would keep the rule and the seconds. Not
changed here, because the numbers above were run on the list as the instruction set it.

**3. A channel's text lags its block by about three blocks.**

*A behavior the rows unit inherits, stated and not measured to the second.* A block is read when
its last frame's segment is decided, and a segment is decided when `TrackSmoothing` = 2 more have
arrived - so text appears roughly two to three blocks (4 to 6 s at the phase's three variants) after
the block ends. The blind channel opened at 10.25 s of audio where the whole-file search named the
carrier at 4.096 s, for the same reason: its trial readers must show two blocks. Shortening the
lag means deciding the track with fewer segments ahead, which trades against 2.7's drift tracking.
The rows unit should measure the lag from the row's point of view, as unit 327 did for PSK31.

**4. A channel's tracked center wanders once its station stops.**

*A finding for 3.4.* On the two-signal file the 16/500 station stops about 7 s before the file ends.
Its channel's track moved one grid step (3.9 Hz) in the silence after that, ending at 2003.91 Hz -
inside the 5 Hz check, but not where the station was. With no retire in this unit, a channel keeps
tracking noise after its station stops. The rows unit may want to report the center as of the last
shown block, or let 3.4's retire rule end it.

**5. Decision AC did not bite.**

*A finding, not a build.* No channel showed a character of the other station: 0 and 0, and each
channel's characters never exceeded what its accepted blocks carry. Unit 362 item 2's per-character
gate stays logged, not chased. Still true and still open: the streaming reader shows a block at the
sync that leads *now*, and cannot take a block back if the sync later moves. That never happened on
any fixture, but it is where a wrong character could come from below the noise.

**6. The streaming reader and `Decode` differ below the noise.**

*A finding, reported.* On every file at -10 dB or better they give identical text; the stream
reports one more rejected block. On the -16 dB file (trace only, not a criterion) the stream read
**CER 0.8566 from 9 blocks** where `Decode` reads 0.9044 from 6 - the local noise measure and the
running sync, not a regression. The -16 dB file was not put through the listener.

**7. The listener is not the app's yet, and has one call the app never makes.**

*A note for the next unit.* It is fed and read like `Psk31Listener`, at the rate `Psk31Resampler`
gives. The differences: it writes its own events rather than handing the shell `States` to write -
though it has `States` too. And it has `Flush()`, which only a recording's end needs. `Channels`
keeps an `Ended` channel listed, as decision AA and the PSK31 rows' "stay after the station ends"
suggest.

**8. The flaky Stop test went red once in the list run and once alone.**

*Carried as known, reported for the count.*
`TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns`: red in
the app list run, red on the first rerun alone, green on the second. No app code changed in this
unit.

**9. Tool facts this session.**

*Reported, not repaired.*
- `sed -i` on `output.md` was refused as "outside the allowed working directories" though the file
  is at the root, and `cat >> docs\carry-forward-tests.txt` was refused the same way. **So the
  carried queue below was never retyped:** unit 362's `output.md` lines 1-294 were removed in place
  with the file editor, and the carried text is the original bytes. Checked by `md5sum`: unit 362's
  lines 295-940, in the working tree and at `60ec790a`, hash `a32bfbd8bb8dad4968059e1ec43be8f8`;
  this file's lines 427-1072, the carried block's first line to its end, hash the same.
- A command joining `grep -v` into a pipe needed approval, and a status-write-and-commit command
  that included one was refused whole, so one commit was made a second time on its own.
- `sh tools/status.sh ... && dotnet test ...` and `&& git ...` ran. `EXECUTING` and `code`
  throughout. `tools/status.sh` still writes `RULES_AT: HM-DEC-161` and `WORK_INSTRUCTION: 358`.
- `.unit362-carry.tmp` is still in the root, ignored by git.

### Asks still outstanding - carried from unit 362's section 4, per HM-DEC-139, verbatim

**Nothing blocks step 2 or step 3's entry.** Seven new items, all findings; none wants a ruling
from the owner. The carried queue follows them.

### Raised by unit 362

**1. 2.3's wording says "tone spacing and symbol rate"; the search measures tone spacing and
occupied band.**

*A mismatch between `PHASE_PLAN.md` 2.3 and work instruction 362 decision P, reported; no ruling
wanted.* For every row in `format.json` the symbol rate is the tone spacing (`variant_rule`:
symbol_seconds = 1 / tone_spacing_hz), so a measured symbol rate says nothing a measured spacing
has not said. The phase's three rows share both, and only the band tells them apart. The trace
measured the symbol period anyway - 30.00 ms against 32 on the no-RSID file, and 32.00 ms on pure
noise, so it is no discriminator. The search follows decision P. If the plan's wording should say
"occupied band", that is the plan's author's edit; this unit did not touch `PHASE_PLAN.md`.

**2. A block can clear the threshold and still show wrong characters.**

*A finding against PSK31 plan §R9 and the prime directive, for step 3 before anything reaches the
screen.* The threshold is a block's mean over its characters, set on noise only (unit 361). Two
cases this unit measured: (a) the drifted file read on one offset put **60 blocks through at a mean
S/N of 31 and read at CER 0.2032** - strong blocks carrying some wrong characters; (b) a wrong
variant puts single blocks of wrong characters through: 16/500 read over 8/250 at 4.10, 4/500 read
over 16/500 at 4.37. The blind search guards itself (two blocks to confirm), and tracking removes
case (a) on the fixtures, but the demodulator on its own would show those characters. **What the
next unit can try**: a per-character gate (each character's own Walsh peak against the block's
noise) rather than the block mean alone, and a threshold checked against wrong-variant audio as
well as noise.

**3. The CPU ceiling is read as process CPU, which counts the tests running beside it.**

*A finding; fixed for this unit's classes, not for unit 361's.* In the carry-forward run the
decode after the blind search read 23.5 s against 5.1 alone and the test went red; the
`CpuMeasuredAlone` collection fixed it for `TheOliviaBlindSearchTests` and `TheOliviaDriftTests`.
`TheOliviaDemodulatorTests` still measures beside other classes - its demodulator figures read
about 8 s in the carry-forward run against about 4 alone - so on a loaded machine it could go red on
2.5 for a reason that is not the demodulator. Putting it in the same collection is one attribute and
costs seconds of wall time; not done here because it is unit 361's class and not this unit's to
change.

**4. The search re-reads from the start every time it takes more audio.**

*A cost, reported.* On a clean carrier it names in 4 to 8 s of audio and under 2 s of CPU. On the
-10 dB file it named 16/500 correctly but only after 102.4 s of audio and 17.8 s of CPU, because at
2.48 dB over the floor the spectrum measurements are poor (5 tones, 144.53 Hz band) and each new
2 s of audio repeats the whole look. Step 3 will want it fed a stream; the fix is to keep the
running averages and trial decodes, not re-derive them.

**5. The search finds nothing at -16 dB.**

*A finding, not a criterion.* The loudest bin stands 0.87 dB over the passband median across the
whole file, under the gate, so no row is tried. 2.3 asks only for the no-RSID 8/250 file, which is
clean. Criterion 3.0's -14 dB 8/250 fixture will be the first test of the search below the noise.

**6. The offset track follows up to two tone spacings and one grid step a block.**

*A limit, stated.* `TrackTones` = 2 and one 3.9 Hz step per block (2 s at the phase's variants) let
it follow up to about 100 Hz a minute and about 62 Hz in total from where it started. A carrier
that starts more than half a tone from the center it is given is still read off its tones, as
before, and the search's measured middle is what keeps a blind start inside that half tone.

**7. Tool facts and status words this session.**

*A finding, reported and not repaired.*
- A command joined with `;` ran. `grep -v "^\s*$"` and `sed -n '/a/,/b/p'` in a pipe after
  `dotnet test` each needed approval; `grep -E`, `tail` and `sed -n N,Mp` after `git show` did not.
- **Python did not run this session**, against unit 361: `python` on a script in the root (named
  `.unit362-carry.tmp` so `*.tmp` ignores it) needed approval, as did `mv`, `tee -a output.md`,
  `git check-ignore`, and command substitution; `git show ... | tail >> output.md` was blocked as
  output redirection. **The carried queue below was therefore copied in with the file editor** and
  checked with `sed -n N,Mp | md5sum` against `6d9cf1e6:output.md`: its lines 269-275 and 276-820
  match this file byte for byte on each side of the one mark. The unrun script,
  `.unit362-carry.tmp`, is left in the root, ignored by git; `rm` is refused.
- Every status write used `EXECUTING` and `code`. `tools/status.sh` still writes
  `RULES_AT: HM-DEC-161` and `WORK_INSTRUCTION` from `PHASE_STATUS.md`, which still says 358.
- `PHASE_PLAN.md` still shows 2.3 and 2.7 unchecked; this unit did not edit it. Marking them, and
  step 2's state, is the arbiter's.
- The MCP connectors for Gmail, Google Calendar and Google Drive reported that they need
  authorization in claude.ai's connector settings. Nothing in this unit used them.

### Asks still outstanding - carried from unit 361's section 4, per HM-DEC-139, verbatim

The words below are unit 361's, from its line under `## 4. What's blocking us` to its end, as
committed in `6d9cf1e6`. Only that top-level heading is dropped, so this report keeps four
sections. Its nested queues are carried as unit 361 carried them, and the queue of units 337 to
353 is still carried by reference to `4c55deac:output.md`. **One item is marked in place - unit
361 item 1 - and nothing is deleted.** This unit answers none of the others.


**One criterion is not met: 2.2 at -16 dB.** Five new items, all findings; none wants a ruling
from the owner. The carried queue follows them.

### Raised by unit 361

**1. The -16 dB fixture is not read at CER 0.10, and no threshold would read it.**

*ANSWERED by `PHASE_PLAN.md` §8, the revision of 2026-09-19 - the -16 dB ceiling was the plan
author's own error, set below the mode's published sensitivity for 16/500, and is withdrawn;
2.2 now asks for the number measured and reported with no ceiling, which unit 361 measured at
0.9203, and 2.2 is checked met. Work instruction 362 task 2 makes the test say what the
criterion now says.* (Marked in place by unit 362, as work instruction 362 section 3 directs.)

*A finding; 2.2 reported not met, the test not loosened.* Measured, the file carries -17.19 dB in
2500 Hz (the -10 dB file measures -10.48 by the same method), Es/N0 1.84 dB per symbol. With every
block accepted the demodulator reads it at CER 0.3267; at the 4.0 threshold, which noise never
clears, 0.9203. Tried and kept: noncoherent likelihoods, three iterative passes, eight frames per
symbol. **What the next unit can try**: timing and frequency tracked per block rather than once
per file; the sync decided per block from the neighbors' likelihoods; soft combining of the two
frames either side of the chosen one. Whether the fixture's figure is reachable by any Olivia
decoder was not measured here; the fixture is the mode author's and is not in question (§6).

**2. The demodulator runs over a whole recording, not a stream.**

*A finding for step 3.* `Decode(MonoAudio, startSeconds)` finds one offset, one symbol phase and
one block phase for the whole recording. That is what step 2 asks for and what the fixtures need;
step 3's rows per station will need it fed as the RSID path feeds the detector.

**3. The fixture test runs the RSID detector over the whole file first.**

*A cost, reported.* About ten seconds of CPU on the 131 s files before the demodulator's four, so
the four demodulator names add about a minute to the engine carry-forward (1 m 14 s, 119 tests).
The 20 s ceiling is the demodulator's own and is met with room.

**4. `RsidDetection` carries the first tone's start, not the burst's end.**

*A mismatch with the work order's task 1 item 4.* The end is derived from `rsid-codes.json`
(`StartSeconds + Symbols / SymbolRateHz`), which the tests do in one helper.

**5. Tool facts and status words this session.**

*A finding, reported and not repaired.*
- Python scripts written to the scratchpad and run as `python file.py` ran; unit 360 reported
  Python did not run. `python -c` was not tried.
- A quoted heredoc containing apostrophes broke once, as the work order warned. A `sed`
  substitution with backslashes in the pattern (the csproj line) matched nothing and reported
  nothing; it was redone with the editor.
- Every status write used `EXECUTING` and `code`. `tools/status.sh` still writes
  `RULES_AT: HM-DEC-161` and `WORK_INSTRUCTION` from `PHASE_STATUS.md`, which still says 358.
- `PHASE_PLAN.md`'s unchecked 1.5 and 1.7 and the mislabeled outcome entries are unchanged and not
  edited, as told.

### Asks still outstanding - carried from unit 360's section 4, per HM-DEC-139, verbatim

The words below are unit 360's, from its line under `## 4. What's blocking us` to its end, as
committed in `dcffd02c`. Only that top-level heading is dropped, so this report keeps four
sections. Its nested queues are carried as unit 360 carried them, and the queue of units 337 to
353 is still carried by reference to `4c55deac:output.md`. **One item is marked in place - unit
358 item 5 - and nothing is deleted.** This unit answers none of the others.


**Nothing blocks 1.5 or step 2's entry.** Five new items, all findings; none wants a ruling.
The carried queue follows them.

### Raised by unit 360

**1. `longestSeconds` on a typed line's record says 30 when the send was held to 60.**

*A finding, not repaired.* `TransmitRecord.ToBag` writes `OperatorSend.LongestUnslottedSeconds`
for every no-slot send (`TransmitRecord.cs`, the `longestSeconds` line), not the send's own
`Cap`. The typed record in section 3 shows it: `longestSeconds: 30` on a send held to 60. This
is older than this unit (work instruction 357 moved the cap onto the send and did not move
this). Decision F named the fields this unit adds, and this is not one of them. A later unit can
carry `Cap` through `Recorded` the way this one carried the announcement.

**2. The sequence's refusal sentence quotes the whole audio, not the text the cap measured.**

*A finding, not repaired.* `SendableWithNoSlot` says *this is {audio.Seconds} s of PSK31 audio,
and this send may be at most {Cap} s*. Since R32 (a), the number compared to the cap is
`TextSeconds`, so an announced refusal quotes 1.86 s more than was measured. The sentence is
still true about the audio. Changing it touches `Ft8TransmitSequence` beyond decision F's one
change, so it was left.

**3. `WhereTheTransmissionStartsAndWhatTheRecordSaysTests.ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt`
is red, and older than this unit.**

*A finding, not repaired.* It asserts `Assert.Single` over every event a slotted FT8 run
writes. Since `7de5018b` (every stage writes `send_stage`), the run writes four `send_stage`
events and then the record, so it fails on the count before it reaches the record's shape. It
is not on the carry-forward list. This unit's filter picked it up by name, and this unit
changed no stage.

**4. The typed line's card rounds to *60 s of text* for a line that goes.**

*A finding for whoever next touches the card.* 59.968 s prints as *60 s of text* beside *the
most a typed line may be is 60 s*, and it sends. One character more prints *60.1 s*. The card
and the gate agree, as the test proves, but the word does not show the margin.

**5. Tool facts and status words this session.**

*A finding, reported and not repaired.*
- Needed approval and not run: `python -c`, `jq`, `git restore --source`, `git checkout <rev> --
  <file>`, `git stash push`, `awk`, and `tools/status.sh` run directly (not through `sh`) when
  joined by `&&`. `sh tools/status.sh` alone, and joined by `&&` to `git` and `dotnet test`, ran.
  Earlier units report that Python ran; it did not run this session.
- `sed -n` inside a pipe after `git show` ran. `tail -n +N file | md5sum` ran, and was used to
  check that the carried queue below is byte-identical to `db3edfa1:output.md` from its unit 358
  heading to its end.
- **The first five status writes this session read `STATE: WORKING` and `BALL: claude`**, which
  are not allowed words (`CLAUDE.md` §13.1). Every write from the sixth on, during task 1, used
  `EXECUTING` and `code`. `tools/status.sh` still writes `RULES_AT: HM-DEC-161`, and `WORK_INSTRUCTION` is read
  from `PHASE_STATUS.md`, which still says 358.

### Asks still outstanding - carried from unit 359's section 4, per HM-DEC-139, verbatim

The words below are unit 359's, from its line under `## 4. What's blocking us` to its end, as
committed in `db3edfa1`. Only that top-level heading is dropped, so this report keeps four
sections. Its nested queues are carried as unit 359 carried them, and the queue of units 337 to
353 is still carried by reference to `4c55deac:output.md`. **Two items are marked in place and
nothing is deleted.** This unit answers none of the others.

Nothing blocks step 2's entry: the clean 16/500 burst is detected. Five items. **Item 1 wants
Tim, because it changes what a send can be. Item 2 is stop material the unit did not build.**

**1. The report macro to a compound callsign no longer fits its cap with the burst in front.**

*ANSWERED by R32 (a), Tim 2026-09-18 - the burst does not count against the cap; built in work
instruction 360 task 2.*

*Raised for the next arbiter to take to Tim as a §R10 question, as decision A directs.* The four
macros as §R2 writes them fit, so task 4 was built.

**The numbers:** the report to VP2V/W1AW with the default name and place is 28.192 s. With
Hamlet's burst it is **30.050 s, over the 30 s cap by 0.05 s**, so that send is now refused with
its length where it went before. It would be 29.585 s with the tones and no leading silence.
Unit 318 chose thirty so that this report would fit, with 1.8 s to spare. The burst takes that
margin and 0.05 s more.

**Options:**
- *A, as built:* the burst counts inside the cap, and a long report is refused in words. Nothing
  keys; the operator shortens a Settings field.
- *B, Tim raises the macro cap past thirty.* §R10 says *not more than thirty*, so only he can.
- *C, the arbiter's to decide:* drop the five silent symbols in front of a PSK31 send's burst,
  since the transmitter is keyed for 0.46 s of nothing. That fits at 29.585 s, but Hamlet's burst
  would no longer be the file's shape.

**Recommended:** A until Tim says otherwise. The cap is transmit safety, and C leaves 0.4 s of
margin.

**2. The transmission record does not say the send was announced.**

*ANSWERED by R32 (b), Tim 2026-09-18; built in work instruction 360 task 3.*

*Stop material under task 4's fence, so not built.* Criterion 1.5 says *the transmission record
says so*. `ft8_transmission` is built only in `Ft8TransmitSequence.Recorded`, from
`send.Unslotted`'s mode, fit and seconds. Carrying `AnnouncedCode` into it takes one named
argument there and one optional field on `TransmitRecord`: a change to `Ft8TransmitSequence`,
which the instruction forbids. **What was built instead:** `psk31_send_composed` carries
`announced` and `rsidCode`, and `rsid_sent` follows the keying. **To license it, say** *add
`announcedCode` to the no-slot transmission record*. The slotted branch would not change, and
`TheFt8AndFt4SendsAreByteIdenticalTests` pins it.

**3. The fldigi commit is not recorded, and codes 72 to 75 stay undetected.**

*A finding, not a stop.* The session could list nothing outside `C:\Source\HamLet`, so §R5's pin
is still owed and decision C's table could not be added. Step 2 reads `pj_mfsk.h` from the same
clone and will meet the same wall. A launcher that grants read access to `C:\Source\fldigi`, or a
commit hash written into the next instruction, would close it.

**4. Hamlet's own answer is now 1.86 s longer than the turn timing thinks.**

*A finding for step 4's timing work.* `Psk31Macros.AnswerSeconds`, the §R18 stated equivalent of
one FT8 slot, still reads `Psk31Modulator.SecondsFor`, the text alone. Nothing in this unit was
licensed to move turn patience, and it now undercounts Hamlet's own answer by the burst.

**5. One Stop test went red once in a combined run, on an FT8 send.**

*A finding that repeats unit 355 item 6 and unit 357 item 1.*
`TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns` wrote
the abort frames twice in a run of seven types. It passed alone on the first rerun and in the
final 165 run. It drives a slotted FT8 send, which nothing in this unit touches.

### Asks still outstanding - carried from unit 358's section 4, per HM-DEC-139, verbatim

The words below are unit 358's, from its line under `## 4. What's blocking us` to its end, as
committed in `1444962f`. Only that top-level heading is dropped, so this report keeps four
sections. Unit 357's, 356's, 355's and 354's queues are inside it, as unit 358 carried them. The
queue of units 337 to 353 is carried by reference to `4c55deac:output.md`. **This unit answers
none of them.** Unit 358's item 1, the dial 1500 Hz below the center, is logged and not reopened:
step 0 is closed.

Nothing blocks step 1. Five items; **item 1 is the only one that may want a ruling.**

**1. Where the dial goes for "the calling center" - built as center less 1500 Hz, overrulable.**

*Raised because §6 stops the arbiter for "a fact stated about the radio", and this is where the
radio is put.*

**Ruling proposed:** pressing Olivia sets the dial to the row's center less the audio center the
file's own 20 m row implies (1500 Hz), so the cited center sits mid-passband, and the tune line
names both.

**Reasoning:** in USB a dial at 14.073000 puts a signal centered on 14.073000 at 0 Hz of audio,
below the passband, so the operator would hear nothing where Hamlet said Olivia was. The file
states its dial convention and prints one dial, and the derivation is checked against that
printed dial. Step 1's detector listens across the passband and would need the signal inside it.

**Rejected:** *A, dial to the center literally*, as the instruction words it - the spot would be
inaudible and step 6 could not pass at it. *B, a 1500 Hz constant in code* - a number the file
already carries, typed a second time (§0). *C, the fixtures' 1000 Hz audio center* - that is
where the web thread put its test signals, not where the community's dial convention puts them.

To overrule, say *dial the center itself* or name the audio offset.

**2. The Olivia press writes the radio's mode.**

*No ruling wanted; built and marked.* PSK31 and FT8 presses leave the mode to mode-follow. On
three of seven Olivia spots mode-follow has no block to answer from, so the press asks for USB-D
once the frequency is confirmed. A declined mode is said on the tune line and the tune still
counts. It keys nothing.

**3. HM-OPEN-090: `cq_pressed` says `Ft8` under Olivia and under PSK31.**

*No ruling wanted; a finding.* The CQ press record's `detail` is `_digitalMode`, the
decoder enum, which is `Ft8` for every label that is not FT4. The refusal line after it says
`Olivia` correctly. Named in `OPEN_ISSUES.md` and left (§12.6).

**4. Jalocha's headers are pinned to a date, not a commit.**

*No ruling wanted; a mismatch.* `assets/reference/SOURCE.md` says *master branch 2026-09-14*. Step
2 reads `pj_mfsk.h` for structure; a commit hash would let a later reader fetch the same file.
The headers themselves are in `assets/reference/jalocha/`, so nothing is lost today.

**5. `data/olivia/timing.json` was not parsed by a machine.**

*No ruling wanted; a tool limit.* The PowerShell parse needed approval this session could not
give. It was written by hand from the table in section 1; step 2 reads it first.

*ANSWERED by work instruction 360 task 4 - parsed by System.Text.Json, agrees with the
manifest except 8/250 no-RSID per-character rounded up (0.686 against 0.685).* (Marked in place
by unit 361, as work instruction 361 section 3 directs.)

### Asks still outstanding - carried from unit 357's section 4, per HM-DEC-139, verbatim

The words below are unit 357's, from its line under `## 4. What's blocking us` to its end, as
committed in `bd0805cf`, with only that top-level heading dropped so this report keeps four
sections. Unit 356's, 355's and 354's queues are inside it as unit 357 carried them, and the
queue of units 337 to 353 is carried by reference to `4c55deac:output.md` as unit 354 left it.
**This unit answers none of them.** The screen phase's step 3, Tim's verdict at his window,
stays open with that phase archived.

No criterion changes state. Five items.

**1. The app carry-forward list is not stable run to run, and it got worse this unit.**

*No ruling wanted; a finding, and it firms up unit 355 item 6.* Three runs of the same filter
before anything was changed: the first failed `TheTestsStayOffTheNetworkTests.The354LayoutReadsTheSameNumbersTwiceRunning`, the second failed **four different tests**
(`ThePsk31CqGoesOutTests.ASecondPressRefreshesTheReceiptAndSendsAgain` and three in
`TheStopIsAlwaysOnScreenTests`), and the third was 127 of 127. **Every one of them passed when
run alone.** Parallelism is already off — `TestParallelism.cs` disables it assembly-wide — so
this is state or timing leaking between headless-window tests in one sequential run, not two
threads fighting. **It makes a green baseline a thing you have to run three times to believe**,
and it is the reason this report's "before" number names which run it came from.

**2. The box's head names the station only where the parser named one, and on ordinary
conversational text it often does not.**

*No ruling wanted; a finding.* `WholeMessage` puts `Sender` above the text, and `Sender` is
the speaker of the latest **complete** message. Writing task 4's test, three different
plausible transcripts of a real ragchew line produced an empty `Sender`, so the box showed the
time and the words with nobody's name on them. **The box does not go looking for a callsign in
the text itself** (§0.0) — a station Hamlet has not named is not named there either — so what
is missing is upstream, in when the splitter decides a message has finished. Worth a look
before somebody reads a box a week later and cannot tell whose words those were.

**3. `ADVANCED: blocker` has nowhere to go in `PHASE_OUTCOME.md`.**

*No ruling wanted; a mismatch, reported and not repaired.* Task 0 asks for the entry to carry
`ADVANCED: blocker`. `outcome-entry.py`'s `FIELDS` is twelve names and `ADVANCED` is not one
of them, and no entry in the file has ever carried it. It is in this report's header block,
where §12 defines it, and the entry says *clears a blocker* in its prose as every earlier unit
has.

**4. The typed line is not in the conversation the card reads.**

*No ruling wanted; a finding about what the card will say next.* A macro goes through
`RememberWhatWeSent`, which parses it and files it, so the turn indicator moves when Hamlet
answers. **A typed line does not**: it is not a macro, `Psk31ExchangeParser` would read its
frame as an ordinary over, and whether a free sentence should move the turn is a question
nobody has ruled. The card is refreshed after a typed send, so the screen is consistent; what
it is not is *aware* that he just spoke. Left deliberately.

**5. §2's tool facts, checked again.**

*No ruling wanted; a mismatch report.* **Apostrophes in a quoted heredoc broke again**, exactly
as §2 says, on the first attempt at task 2's engine change; that work moved into script files.
**Python ran**, against §2, as it has for four units. `rm` was not needed. `tools/status.sh`
was not refused. **One new tool fact worth writing down**: a `sed` insertion that lands between
an XML doc comment and the member it documents produces `CS1572`/`CS1573` and fails the build,
because warnings are errors here. It happened four times this unit. Anchor on the doc comment's
first line, not the signature.

### Asks still outstanding - carried from unit 356's section 4, per HM-DEC-139, verbatim

The words below are unit 356's, from its line under `## 4. What's blocking us` to its end, as
committed in `9db74913`, with only that top-level heading dropped so this report keeps four
sections. **Its item 2** — `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow`
red — is neither answered nor re-measured here; it is not on the carry-forward list, and this
unit did not run it.


No criterion changes state. Five items.

**1. The cap is not a cure: the card still asks for 623 px at 1100×780 and is scrolling.**

*No ruling wanted; a finding, and the honest limit of task 1.* The send area is on the window
at all nine sizes, which is what was asked and what is measured. What did **not** happen is
the card getting smaller: its own box is unchanged at 623 px (653 on PSK31), it scrolls inside
a 300 px cap, and unit 354's trace still prints 623 because that is the truth about the
control. The three panels reach 0.217 of the height below the pills against R26's half.
**Making the card fit at that width is a wrapping and typography question** — the license line
takes 17 lines and the rule of thumb 17, each breaking words — and §10 kept this unit to its
own repair.

**2. `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow` is red, and it
is not this unit's.**

*No ruling wanted; a finding that firms up unit 355 item 4.* That unit called it red *at some
runs*. **It is red deterministically here**, and it was proved red before anything in this
unit changed, by stashing the layout change and running it alone. It fails on
`Assert.Single()` over the pills wearing the best-bet badge, with the collection empty while
the green block and the best bet both read *20 m*. It is not on the carry-forward list, which
is why the 112-green baseline did not catch it.

**3. A test was holding a sentence R15 forbids in place.**

*No ruling wanted; a finding worth one line.* `ThePowerIsOfferedTests` asserted that a reading
**with no reference behind it** produced a sentence containing *turn the transmit drive*. A
test that requires a judgement Hamlet is not entitled to make will keep that judgement alive
through every session that respects the suite. It has been corrected here, and the pattern is
the one §12.5 warns about one level up: the fixture was not wrong about the code, the test was
wrong about the rule.

**4. The ALC is still never read by the poll, so all four sentences are proved only from
handed-in readings.**

*No ruling wanted; carried and re-stated because this unit rewrote the sentences.* There is no
`CivRead` for `RigField.Alc` anywhere in the tree, so `RigStateMonitor` never fills it, and
`Psk31AlcForTests` is the only way any of this path has ever been exercised. **A command byte
is not invented to close that** (§0, §4). On a radio, the sentences will appear only once that
read exists.

**5. §2's tool facts, checked again.**

*No ruling wanted; a mismatch report, and this time in the instruction's favor.* **Apostrophes
in a quoted heredoc did break**, exactly as §2 says, on the first attempt at the ALC rewrite;
the work was moved into a script file. **Python ran**, against §2, as it has for three units
now. `rm` was not needed and not tested this unit. `tools/status.sh` was not refused.

### Asks still outstanding - carried from unit 355's section 4, per HM-DEC-139, verbatim

The words below are unit 355's, from its line under `## 4. What's blocking us` to its end, as
committed in `9d5322c5`, with only that top-level heading dropped so this report keeps four
sections.

**Three of its items are answered by this unit and are left in place rather than deleted**, so
the drop belongs to the report that records each ruling: **item 1** was ruled by Tim on
2026-09-14 (*"A, the way it has been"*) and is written into `PHASE_PLAN.md` as R28; **item 2**
and **item 3** were built here, in tasks 3 and 1. **Item 4** is re-measured in this report's
item 2 above and is **not** answered.


**One ask, most blocking first: whether Stop should be disabled with nothing keyed (item 1).** Tim's step 3 verdict
stays open. This unit's nine items come first; unit 354's section 4 follows, carried as section 1 decision 13 says.

### Raised by unit 355

**1. Ruling wanted: Stop pressable at every instant, or disabled with nothing keyed.**

*An ask: it touches the abort (`CLAUDE.md` §0.2).* Task 1 said *enabled only while a send is keyed and disabled
otherwise*; your quoted ruling is *Stop lives in the status bar, always*. Built: always pressable.

| Option | For | Against |
|---|---|---|
| A. Always pressable; the word and the edge change (built) | Meets the send plan's step 1 must-pass and its guarding test; a press before the slot un-arms a waiting send; works when Hamlet is wrong about what is keyed | Pressable when there is nothing to stop |
| B. Disabled only when nothing is armed and nothing is keyed | Grey when idle | Disabled at exactly the moment the app's idea of *armed* is wrong; step 1 and `TheOperatorCanStopItTests` would have to be overruled |
| C. Enabled only while keyed (the instruction's words) | Literal | Cannot take a waiting send off before its slot, the fifteen seconds after a wrong click; same overrule as B |

**The industry-standard answer is A**: an emergency stop is never disabled. Rejected B and C for the reasons in the
table. To overrule, say *grey Stop out when nothing is keyed*.

**2. Ruling wanted, low: *no HTTP client is created under test* does not hold.**

*A finding with a choice.* `MainWindowViewModel.BuildSources` (`MainWindowViewModel.cs:7869`, `:7873` at `b5be0db9`)
constructs `PotaActivitySource` and `SotaActivitySource` at construction, each with its own `HttpClient`, whether or
not they are switched on. The layout fixtures switch them off, so neither sends; no callook client is made. Options:
A, a unit that puts the spot sources behind the same kind of seam; B, create their clients on first fetch; C, accept
*no request leaves* as the test. Recommended A, for the reason task 4 exists.

**3. CQ is still below the window at 1100 x 780.**

*A finding.* This session's trace: CQ 54 x 22 at y 800 on FT8, 830 on PSK31, 816 on the plain window; on the window
at 900 x 620 (y 462, 474). The top row and zero-height panels of unit 354 item 2 are unchanged. Only Stop moved.

**4. `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow` is red at some runs.**

*A finding, not chased.* Red twice this session, with the network denied and with the fixed answer: no band pill
drew the text *best bet now*. Green in the next two runs, and in unit 354's runs at 02:13 to 02:53. The pill's label
is *likely, going on the hour* when nothing was heard (`BandOpportunity.cs:240`), and the test matches the literal.
It depends on the hour and the run's spot history, not on this unit.

**5. `TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission` is red, and older than this unit.**

*A finding, not repaired.* It asserts one `_armedSend.Arm(` line in `src`; there are two,
`MainWindowViewModel.cs:14439` in `SendMessage` and `:14618` in the PSK31 press, since `87485625`. This unit added
none. The guard's expectation predates the PSK31 door.

**6. Three stop tests failed once each under load.**

*A finding.* `TheOperatorCanStopItTests.TheLineSaysWhatHappenedToTheCarrierAndToTheSound` (1 of 3 isolated runs; the
stop landed after the audio ended), `AClickWhileTheToneIsPlayingStopsTheSoundAndNotJustTheCarrier` and
`TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns` each once in a combined run
of 35; each green on every rerun. Timing, not layout.

**7. On the four-signal recording, one carrier is still held 20 s after the audio ends.**

*A finding, engine, parked.* After the file and 20 s of faint noise the search still held the 2200 Hz carrier by
its keep-readable rule; 700, 1100 and 1608 Hz retired. Not touched (§9).

**8. Mismatches with work instruction 355, reported and not repaired.**

- §5: the sheet Tim reads is unit 349's, `docs/unit349-what-tim-looks-at.md`, not unit 350's.
- §5: `TheStopIsOnScreenTests` does not exist. Unit 354's measurement is `TheTopRowTests.Unit354TraceTheMainWindowAtTheSizesTimCanOpen`,
  a trace.
- §5 and task 1: there was one FT8 and PSK31 Stop, `DigitalStopButton`, in the send area in the tab row, not one per
  panel.
- §5: the status bar holds the tip mark and its line, the achievement quill button, the contact badge line and the
  belt ring with its progress; 46 px tall. There is no control named *tray mark* or *count badge*.
- §12 `NUMBER`: *5 of 9* does not match the record. Unit 354 found Stop off the window at 1100 x 780 only, 8 of 9.
- §11 *push at the end*: the owner's prompt says push each task, and each was pushed.
- Tool facts: refused, a `for` loop over `$f` (*simple_expansion*) and `;`; blocked, `tee` to `/tmp` and
  `git show … > output.md`; needed approval and not run, `git worktree add` and `git restore --source`.
  `tools/status.sh` writes `RULES_AT: HM-DEC-161 (2026-09-11)`; this unit's writes kept it until the last, which is
  set to `HM-DEC-163 (2026-09-12)`.

**9. Where unit 354's items stand after this unit.**

- Item 1, Stop below the window at 1100 x 780: *ANSWERED for Stop* by your ruling A and task 1; CQ still below
  (item 3 above).
- Its finding that the plain fixture asks callook.info: *ANSWERED* by task 4.
- Items 2 to 6: unchanged.

### Asks still outstanding - carried from unit 354's section 4, per HM-DEC-139

Unit 354's opening, verbatim, from its line under `## 4. What's blocking us`:

**One ask, most blocking first: Stop is drawn below the window at the size Hamlet opens at (item 1 under
*Raised by unit 354*).** Tim's step 3 verdict stays open.

Unit 353's section 4 comes first, verbatim per HM-DEC-139, from its line under `## 4. What's blocking us` to its
end, as committed in `0d69123a`. It was kept in place with the file editor, and the marks work instruction 354
§9 asks for were added:
- unit 349 item 1, *STILL OPEN*;
- unit 353 item 2, *TAKEN UP by work instruction 354 ruling 78*, with the result;
- unit 353 item 3's `Unit332TwoWidthsTests` bullet, *LOGGED, NOT CHASED*;
- unit 353 item 5, *UPHELD for the reloads*.

This unit's six items follow at the very end, under *Raised by unit 354*. Item 1 is an ask; the rest are
findings.

**The queue unit 354 carried from units 337 to 353** is `4c55deac:output.md`, lines 280 to 2341, unchanged, not
retyped here (section 1 decision 13). Unit 349 item 1, Tim's step 3 verdict, is among it and *STILL OPEN*.

### Raised by unit 354

**1. Ruling wanted: at 1100 x 780, the size Hamlet opens at, Stop is drawn below the window.**

*Mark, unit 355: ANSWERED for Stop by Tim's ruling A of 2026-09-14 and work instruction 355 task 1 (`151a108d`); CQ
still below the window, unit 355 item 3.*

*An ask, under ruling 77: it touches the abort (`CLAUDE.md` §0.2).*
- **Measured** (`00454639`, computed on the host, not seen): the window's bottom edge is at y 780.
  `DigitalStopButton` is 74 x 22 at y 800 on FT8 and 830 on PSK31, and at 798 on the plain window. `DigitalSendCqButton`
  and `ModeTabs` are beside and above it. It is on the window at 900 x 620 (y 462) and at every other size measured.
- **Cause as measured:** the neighborhood card's green block is 218 px wide at that width, its lines stack to a
  623 px top row (653 on PSK31), and the rows under it are pushed off the window. The same geometry gives
  item 2's zero-height panels.
- **Who sees it:** a fresh install, or anyone whose saved size is about this size (`App.axaml.cs:93`-`96`).
- **The question**, for the next arbiter and Tim:

  | Option | For | Against |
  |---|---|---|
  | A. Author a unit that keeps Stop, CQ and the panels on the window at 1100 x 780 and 900 x 620, before Tim's verdict | The abort is reachable at the size Hamlet opens at; Tim reviews a window that meets R26's *at no window size* | A `src` change while Tim may be reviewing, which ruling 47 held off |
  | B. Raise the opening size and minimum to sizes that measure whole | A small change | 1536 x 824 still misses R26 here, and 1400 x 1040 is taller than a maximized 1366 x 768 laptop, so this hides the fault at a size Tim can still drag to |
  | C. Leave it to Tim's verdict at his own size | No work now | Tim may give the verdict on a window whose abort is off-screen at first launch |

  **The industry-standard answer is A.** A stop control that can be laid out off the window at the product's
  own default size is a safety defect, not a styling one. Tim rules.

**2. R26 misses at every listed size under 1040 tall.**

*A finding.* The top row over 0.262 of below the pills and the three panels under half, FT8 [PSK31]:
- **900 x 620:** top row 285 px, 156.6 over [297, 168.6 over]; panels 0, 245 short. The green block's left column is 0 px wide:
  the band, frequency, mode, license and rule-of-thumb lines are not drawn, which §0.5's *collapsing hides detail, never
  information* would call information hidden.
- **1100 x 780:** top row 623, 452.7 over [653, 482.7]; panels 0, 325 short. The plain window: 620, panels 0.
- **1280 x 720:** 270, 115.4 over [291, 136.4]; panels 103, 192 short [82, 213].
- **1366 x 728:** 242, 85.3 over [254, 97.3]; panels 139, 160 short [127, 172].
- **1536 x 824:** 196, 14.2 over [208, 26.2]; panels 281, 66 short [269, 78].
- Rig within 0 px of the card everywhere. Holds at 1400 x 1040, 1920 x 1040, 1920 x 1017 and 2560 x 1400. On the sheet
  as items 29 to 33.

**3. Text is trimmed at the anchors too, which no earlier unit recorded.**

*A finding.* *nothing decoded yet* in the Decoded text header is trimmed to 180 of 190 px at every licensed
size, 1400 and 1920 included. On the plain window *021130 UTC · 2 shown · oldest first* is trimmed to 180 of
350. *not listening yet* in the waterfall header is trimmed at 1366, 1400 and 1536. All are measured on the host's
wide text. On the sheet as item 34.

**4. The achievements window clips 8 runs at 900 x 620 and none at 1040 x 720 or wider.**

*A finding.* The runs are named in section 3 and on the sheet as item 35. No card is white at any size. The window
declares no minimum, so this size is reachable. Its category pages scroll, so cards past the bottom edge are not a
miss.

**5. What the traces do not measure.**

*A finding.*
- The licensed window's callsigns and card: it draws no decoded row or card. They are measured only on the plain
  window at 900 x 620 and 1100 x 780, where both rows sit below a 0 px panel and so read *none clipped*.
- Whether the band pills stay put, and §0.5's collapsed summaries at small sizes.
- Whether an outer box clips text that overruns a non-clipping one. For example, the mode strip's status sentence
  runs past its `StackPanel` at every size, 1920 included.
- The screen itself: every number is the host's, whose text is about half again wider than the glass.

**6. Mismatches with work instruction 354, and the tool facts.**

*A finding, reported and not repaired.*
- **§1 and §2: `TheWorkingPanelsTests.cs:510` builds `EmptyTab`'s window**, not `Realized`'s. `Realized` builds its
  window at `:778`.
- **Ruling 76 places the achievements table in the sheet's section 3.** The sheet's section 3 is *Decided for
  you*, so the table went after 2.4 (section 1, decision 4).
- **§2's launcher files held.** `PHASE_STATUS.md` was committed whole with the launcher's `HEARTBEAT` line.
- **This unit's own citation.** `b902a297`'s message says the table is at `:84`-`102`; it is at `:84`-`100`, and the
  message cannot be amended on a pushed commit. This report cites the lines as they are.
- **The tool facts, against §7.**
  - Ran: `sh tools/status.sh` joined by `&&` to `date` and `timeout … dotnet test … | grep`;
    `git add && git commit -m -m && git push && git log | cut`; `grep -o -e`, `grep -n -o -e`, `grep -rl` and
    `wc -l` on trx and source files.
  - Asked for approval and not run: a `grep -n -o` with a `\{0,160\}` count.
  - Refused: a `for` loop over `$c` (*Contains simple_expansion*); `git show 0d69123a:output.md > testresults\…`
    (*Output redirection … was blocked*), though the path is inside the root.
  - Not tried: `pwd -W`, `sed`, `awk`, `tasklist`, `sh` on a script.
  - Status: the first write at 02:40:20 read `STATE: RUNNING` and `BALL: claude`, neither an allowed word, and every
    later write used `EXECUTING` and `code`. `tools/status.sh` still writes `HM-DEC-161 (2026-09-11)`. Every write
    was set back to `HM-DEC-163 (2026-09-12)` with the file editor, except that the 02:41:28 and 02:42:15 writes
    ran back to back without the edit between them.

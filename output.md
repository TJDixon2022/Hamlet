READ IN THIS ORDER.

A. The phase goal - Hamlet works Olivia the way it works PSK31. Steps 0,
   1 and 2 done and closed; step 3 was not started, none of seven met, at
   the start of this unit and is one of seven met (3.0) with 3.1's engine
   half met and 3.6 measured on the engine, on this report's evidence,
   for the arbiter to mark; steps 4-6 not started, and step 4's entry
   opens only when step 3 is done.
B. Step 3's criteria this unit worked - 3.0 below the noise: SNR set -14,
   measured -13.82 dB; RSID read 8/250 at 1000.16 Hz; CER 0.0000 (0.10) on
   seed 363, and 0.0000, 0.0000, 0.0000, 0.0000 on the four others. 3.1
   engine half: channels 2 (2), 8/250 at 1000.00 Hz and 16/500 at 2003.91
   Hz (5 Hz), CERs 0.0000 and 0.0000 (0.05), characters of the other
   station 0 and 0 (0). 3.6: ratio 0.154 (1.0) on the engine, worst piece
   0.115 s. Met: 3.0; engine half met: 3.1. Not worked, the next unit's:
   3.2, 3.3, 3.4, 3.5, and the rows half of 3.1. Step 2's rows through the
   listener: clean CERs 0.0000 / 0.0000 / 0.0000 (0.0000), -10 dB 0.0000,
   noise channels 0 (0); and the no-RSID file one channel found blind at
   1000.00 Hz, CER 0.0000 (0.05).
C. The report last: section 4 raises 9 items on top of the carried queue;
   none stands in the way of 3.0 or 3.1. 3.0 was met on every seed, not
   only the stated one - and by the demodulator as unit 362 left it, with
   no change; the trace read 0.0000 on all five before a line was written
   (item 1). No channel ever showed the other station's characters, so
   decision AC did not bite and nothing was built for it (item 5).
   Nothing in pj_mfsk.h had to be copied; it was not opened this unit.
   The listener is ready for the next unit to draw rows from as it
   stands, with three things it should know first: text reaches a
   channel about three blocks after the block began (item 3), a channel's
   tracked center wanders a grid step once its station stops and no
   retire exists yet (item 4), and the engine carry-forward run now takes
   4 m 40 s (item 2).

UNIT:       363 - complete at task 4 of 5, task 4 built - 2026-09-19 12:38
PHASE GOAL: Olivia in Hamlet as fully as PSK31 - every station heard, read, answered and logged - with the variant always taken from the signal itself (its RSID, or measured off a carrier that never announced itself) and never picked by the operator.
UNIT GOAL:  Prove Olivia reads below the noise - the mode author's 8/250 QSO at -14 dB in 2500 Hz, RSID in front, read at CER 0.10 or under from its own RSID - and build the engine's one Olivia receiver: audio in as it arrives, one channel per station by RSID or blind search, each with its own reader and its text as blocks are accepted, two stations on the two-signal file with nothing of one in the other, at a real-time ratio reported.
ADVANCED:   yes - 3.0, the phase's headline claim, is met on five noise draws out of five, and 3.1's engine half is met: two stations read at once, each only its own.
NUMBER:     step 3 criteria met 0 of 7 -> 1 of 7 (3.0; 3.1 engine half; 3.6 measured on the engine); stations read at once 1 -> 2
DRIFT:      0

| Criterion | This unit's evidence | State |
| --- | --- | --- |
| 3.0 8/250 at -14 dB in 2500 Hz, RSID in front, CER <= 0.10 | seed 363: SNR measured back -13.82 dB; RSID OLIVIA_8_250 at 1000.16 Hz; CER **0.0000**, 251 of 251, 84 blocks / 0, 5.4 s CPU. Four further seeds 0.0000 each, not asserted | **met** |
| 3.1 two-signal file yields two rows, own text <= 0.05, nothing of one in the other | listener: exactly 2 channels, most at once 2; 8/250 at 1000.00 Hz and 16/500 at 2003.91 Hz, both by RSID; CER 0.0000 and 0.0000; the other callsign 0 and 0 times. No rows drawn | **engine half met** |
| 3.2 transcript corpus through Olivia rows, same verdicts | - | not worked, the next unit's |
| 3.3 CQ filter, worked-fade, EntityOf, quill, hover on Olivia rows | - | not worked, the next unit's |
| 3.4 retire window from the timing table | - (no channel retires in this unit, decision AA) | not worked, the next unit's |
| 3.5 row telemetry with mode olivia and the variant | - (the listener's own `olivia_channel` and `olivia_block` events exist, decision AD) | not worked, the next unit's |
| 3.6 real-time ratio on the two-signal file under 1.0 | 0.154 (4.453 s CPU for 28.98 s), longest single Add 0.115 s wall; 16/500 131 s file 0.109, noise 0.079 | **measured on the engine** |

## 1. What Claude did

**Complete: tasks 0 to 4 of 5, all built, none dropped.** Machine: Tim's development box,
`C:\Source\HamLet`, gate held (`SHACK_FACTS.md` and `CwProbabilisticDecoder.cs` present,
`CoreHMI.sln` and `MURC.sln` absent), branch `main`. Five task commits, **every push succeeded**:
`40bc67bb` (task 0), `bca75bf7` (1), `49c66245` (2), `f66a1e69` (3), `0b9e99e1` (4); this report is
committed after them. Status written at every task boundary and before every `dotnet test`,
`EXECUTING` and `code` throughout.

### Task 0 - the unit opens

`PHASE_STATUS.md`: steps 0, 1 and 2 `done`, **step 3 `not started`**, `CURRENT_STEP: 3`,
`WORK_INSTRUCTION: 358` (stale, as expected). **Step 3's entry:** `olivia-two-signals-rsid.wav`
hashes as its manifest says, and the RSID detector reads **OLIVIA_8_250 (69) at 1000.29 Hz and
OLIVIA_16_500 (70) at 1999.66 Hz**, 15 tones right each, first tones at 0.465 and 0.464 s. The other
eight hash, **9 of 9** - all nine inside `TheRsidDetectorTests` on the carry-forward run, which hashes
every file it reads against the manifest. `UNIT 363 - STEP 3` appended to `PHASE_OUTCOME.md`, no
earlier entry touched; version 1.13.49 -> 1.13.50. Carry-forward before any change: **app 166 of 166
(45 s), engine 124 of 124 (1 m 39 s)**, first runs. The engine line ran **twice**: the first run's
summary line was cut off by my own `grep`/`head` filter, so I ran it again for the count. That is
three invocations at task 0, not two.

### Task 1 - the trace (`Unit363Trace`, asserts nothing, in `CpuMeasuredAlone`)

1. **The -14 dB fixture, decision W's recipe, before any change.** Burst's first tone at 0.465 s by
   the detector; **slice ends at sample 18580 = 2.3225 s**, located as first tone + (15 tones + 5
   silence symbols) / 10.7666 Hz from `rsid-codes.json` - the trailing silence taken as the file's
   own silence count, which it states for the front of the burst. Checked against the CQ file's own
   samples: the mean square between the tones' end (sample 14865) and the slice end is 0.000000000,
   and the first sample over 1% of the peak after the tones is sample 18602, **22 samples (2.75 ms)
   after the slice end** - the Olivia tones begin where the slice ends. The QSO file follows whole
   (1 376 512 samples). Signal power over the QSO alone 0.093929; noise variance 3.775039 (so the
   noise in 2500 Hz is 25.1 times the signal). **SNR set -14.00, measured back -13.82 dB** on seed 363;
   the same method reads the shipped -10 dB file at **-10.48 dB**, as unit 361 found. The detector
   reads OLIVIA_8_250 at 1000.16 Hz, 14 tones right. **`Decode` at that detection: CER 0.0000, 84
   blocks / 0, sync S/N 9.87, lowest accepted block 8.21, 5.17 s CPU.** The other four seeds:
   measured -13.90 / -14.03 / -14.18 / -13.90 dB, RSID at 999.75 / 1000.36 / 999.42 / 999.42 Hz
   (13 to 15 tones right), **CER 0.0000 on each**, 84 / 0, sync S/N 9.60 to 9.85, lowest accepted
   7.31 to 7.52. **This is the number 3.0 started from: already met.**
2. **The two-signal file through what exists.** 8/250 at 1000.29 Hz: 13 blocks / 0, sync S/N 85.11,
   first block 2.338 s, **CER 0.0000 against KC3QIS's half, KC3QIS 3 times, EI4GNB 0 times.** 16/500
   at 1999.66 Hz: 10 / 3, sync S/N 84.04, **CER 0.0000 against EI4GNB's half, EI4GNB 3 times, KC3QIS 0
   times.** Against the wrong half each reads 0.4737 - the two CQs share every character but the
   callsigns. `OliviaBlindSearch`, told nothing: **2 candidates after 8.192 s**, 8/250 at 1000.00
   (S/N 79.05 on 2 blocks) and 16/500 at 2000.00 (83.55 on 2); at 2000 it also weighed 4/500, which
   put 2 blocks through at 4.30 and lost on S/N.
3. **What in `Decode` and `Search` looks at audio not yet arrived** (lines as at `60ec790a`).
   `OliviaDemodulator.cs`: :228 the frame count from the whole recording, and :225-228 the last
   window read past its end as silence; :231-264 every frame's power held; :267-268 and :471-554 the
   offset track, each segment's figure summed over `TrackSmoothing` segments **after** it (:503) and
   the path **backtracked from the last segment** (:537-551); :283 and :565-604 the noise as the
   median of **every** frame's tone energies; :335-355 the sync as the best sum over **every** block
   at 8 x 64 phases; :374-426 every block read at that one sync, those before it was evident
   included. What a stream holds instead: the frames not yet decided; each segment's figure and the
   forward figures of the last decided one, deciding a segment once `TrackSmoothing` more have
   arrived; the energies of the nearest decided segments for the noise; the 8 x 64 running sums;
   the likelihoods of the frames of blocks not yet read; and where the last shown block ended.
   `OliviaBlindSearch.cs`: :123-150 `Search` re-runs `LookAt` over `audio[..taken]` each time
   `taken` grows by a block - the whole history per step; :162 the Welch average over all of it;
   :247 and :329-463 the tones over all of it; :266-302 a trial `Decode` of all of it per row
   weighed. A stream holds a running set of recent spectra, the recent audio for the tones, and
   trial readers that keep reading.
4. **`Psk31Listener` as the app uses it.** Made at `MainWindowViewModel.cs:2896` at
   `Psk31Resampler.TargetSampleRate`; fed at :2958 with whatever the audio tap holds since the last
   tick, through the resampler; it cuts that into one-symbol pieces itself (`Psk31Listener.cs:189`,
   :260-292). A channel is `Psk31Channel(Id, OffsetHz, StrengthDb, Text, Readable)` (:21-22). A new
   channel is replayed the last `ReplaySeconds` = 3.0 s (:167, :375-381, :423-443). `States` gives
   `Psk31ChannelState(Id, OffsetHz, Open, Quality, AfcHz, Characters, FirstCharacterSeconds)`
   (:230-241, `Psk31Watch.cs:135`); the app writes events from them at :3179, :3444 and :3975 and
   drains `Watch` at :3386-3422. `Channels` is read at :3500 (`ShowPsk31Channels`). **The listener
   writes no telemetry itself**; the app does, from `States` and `Watch`.
5. **What blocks cost in time.** 64 symbols a block: **2.048 s at 8/250, 16/500 and 32/1000** (1.024 s
   at 4/250, 8/500, 16/1000; 0.512 s at 4/500). On the three clean files the first accepted block
   begins **0.480 s after the burst's end** and ends 2.528 s after it. The replay decision Y derives
   is therefore bounded below by the detector's delay (a burst is handed back about one burst length,
   1.39 s, after it ends) plus that 0.48 s, and by the blind search's first look at two blocks.

### Task 2 - below the noise (3.0, `TheOliviaBelowTheNoiseTests`)

**No demodulator change: the trace had already read 0.0000 on all five seeds.** The test builds
decision W's audio in memory (`OliviaBelowTheNoise.Make`, written nowhere, the manifest untouched,
the entry count still nine), takes the variant and center from the made audio's RSID, and asserts
8/250 within 5 Hz of 1000, CER 0.10 or under, 2.5's 20 s of CPU, and §3.3's every character from an
accepted block (as unit 362's -16 dB test asserts it). **Watched failing first against a stub that
handed back the decode with its text emptied: CER 1.0000, red.** Then green, CER 0.0000. The four
further seeds are a second name that prints and asserts nothing. On the engine carry-forward line.

### Task 3 - the listener (3.1 engine half, `TheOliviaListenerTests`)

Three new engine classes under `src\Hamlet.RadioEngine\Olivia\`, wired into nothing (decision L):

- **`OliviaStream`**, made by `OliviaDemodulator.Open()`: `Decode`'s five steps kept and advanced.
  Each frame is transformed once, as its window arrives. The offset track is decided a segment at
  a time, forward only, once `TrackSmoothing` segments after it have arrived: the same figure and the
  same one-grid-step rule, but the segment takes the best figure at that point instead of a
  backtracked path. The noise and signal are measured over the `2 x TrackSmoothing + 1` decided
  segments nearest the one being read. The 8 x 64 sync sums are advanced as each block completes.
  A block is read at the sync that leads now as soon as its last frame is decided; once one is
  shown, nothing that starts inside it is read, and a new frame phase may start up to one symbol
  early. `Flush()` reads a recording's last block the way `Decode` does (the window past the end
  as silence); the app never calls it. Memory is trimmed to two blocks behind the newest frame.
- **`OliviaSearchStream`**: the blind search fed as audio arrives. A running set of the last
  `AverageBlocks` = 4 slowest-row blocks' spectra is kept. `OliviaBlindSearch`'s own region, gate,
  occupied-band and ranking steps are used, the tones are measured over the last two blocks, and
  **each row weighed is an `OliviaStream` that keeps reading** from the kept audio. The rule for
  naming is the same: at least `ConfirmBlocks` = 2, best S/N. A place already read is never
  searched.
- **`OliviaListener`**: `Add(samples)`, `Flush()`, `Channels` as `OliviaChannel(Id, Variant,
  CenterHz, Found, OpenedSeconds, Text, BlocksDecoded, BlocksRejected, Ended)`, `States` as
  `OliviaChannelState(...)` with counts and no text, `SamplesSeen`, `SampleRate`, `ReplaySeconds`.
  The streaming `RsidDetector` hears announcements, and an Olivia one opens a channel at its
  variant and center, read from the burst's end out of kept audio. The search's named carriers open
  channels found `blind`, each keeping the reader that confirmed it. Decision AA: the same place is
  within half the narrower bandwidth; the same variant announced there opens nothing and turns
  `blind` into `rsid`; a different variant ends the old channel and opens a new one. Nothing
  retires. Events (decision AD, category `Psk31`, as the Olivia events already use):
  `olivia_channel` (opened / announced / ended) and `olivia_block`, one for every block a channel
  reads.

**`Decode` and `Search` keep their results.** `OliviaDemodulator` only opens its block decoder,
likelihood helpers, noise measure and a few read-only properties to the stream, and gains
`Open()`. `OliviaBlindSearch`'s region, ranking and tones steps are pulled into internal helpers,
in the same order, unchanged. Their tests stayed green unedited on the carry-forward run.
**Decision V's eight rows, re-printed after the change, match unit 362's to the digit** (section 3).

**Tests watched failing first**, against an `Add` that returned at once: **7 of 8 red** - the noise
row is green against it, as a listener that hears nothing opens nothing. Then **8 of 8 green**.

**Carry-forward with `TheOliviaBelowTheNoiseTests` and `TheOliviaListenerTests` on the engine line:
engine 134 of 134 in 4 m 40 s** (was 124 in 1 m 39 s at task 0), first run; **app 165 of 166** on
the list run. The red, `TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns`,
failed again alone, then **passed alone on the second rerun**. It is a flake: this unit changed no
app code, and "the flaky Stop tests" are in the carried queue. The app number is the list run's,
165 of 166.

### Task 4 - the real-time ratio (3.6, `TheOliviaListenerKeepsUpTests`)

Process CPU around the feed alone, quarter-second pieces, in `CpuMeasuredAlone`: two-signal file
**0.154**, asserted under 1.0; the 131 s 16/500 file 0.109 and the noise 0.079, printed. The longest
single `Add`: **0.115 s wall** on the two-signal file, on the piece where both RSID channels open and
take their replay. **Not put on the carry-forward line** - my call, stated as a decision below.

### Decisions this session made for itself, author's and overrulable

- **The slice's trailing silence is `rsid-codes.json`'s `silence_symbols_before`, 5 symbols, laid
  after the tones.** The file states a silence count only for the front; the CQ file's own samples
  confirm the Olivia tones start 22 samples after that point.
- **Seeds 363 (asserted) and 3631-3634 (printed), written into the helper before any run.**
- **The streaming reader's rules**: the noise window of `2 x TrackSmoothing + 1` decided segments;
  the forward-only track decided `TrackSmoothing` segments late; the sync is the one that leads now;
  a shown block fixes the reading point, with a new frame phase allowed up to one symbol early; two
  blocks of frames kept behind the newest.
- **`OliviaSearchStream`'s three numbers**: `AverageBlocks` = 4 (the whole search named the
  two-signal file's carriers at four blocks), `ReplayBlocks` = 3 (a carrier starting at the audio's
  first sample still has its first block whole when the first look comes at two), `TrialBlocks` = 4
  (two to confirm and two for a region found while its carrier was still starting). **The listener's
  replay is `ReplayBlocks` x the slowest row's block, 6.144 s** - derived, not a literal.
- **The listener writes its own events**, where `Psk31Listener` leaves that to the app, because
  decision AD asks for the listener's own event and the Olivia engine classes already write theirs.
- **`TheOliviaListenerKeepsUpTests` is not on the carry-forward line.** 3.6 is nice-to-pass, the next
  unit measures it again with the rows drawn, and the engine invocation already went from 1 m 39 s
  to 4 m 40 s.

### Verified against the tree - mismatches with section 5 of the instruction

- **None of substance.** HEAD `60ec790a`, 1.13.49; `output.md` unit 362's, committed; `PHASE_STATUS.md`
  steps 0-2 `done`, step 3 `not started`, `CURRENT_STEP: 3`, `WORK_INSTRUCTION: 358`;
  `PHASE_OUTCOME.md` ending on `UNIT 1 - STEP 2` with `FATE: executed` and `STATE_AFTER: done`; the
  nine fixtures, seconds and texts as stated; the generator's five arguments and `SOURCE.md`'s
  *nothing here is built by a session* and `cRsId` port; the six Olivia files; `Decode`, `Search`,
  `TrackTones` 2, `TrackSmoothing` 2, `OffsetTrack`; `RsidDetector.Feed` and `Flush`;
  `Psk31Listener`'s shape; carry-forward 124 / 166 with the names as stated; `PttOn` 1
  (`Ft8TransmitSequence.cs:513`), `Arm(` 2; `assets\fixtures\captured\` holding only `README.md`.
- **Section 1's "unit 362 measured the engine invocation at 1 m 23 s for 124 tests"**: unit 362's
  report gives 1 m 23 s for **123**; its 124 came after task 4, with no duration. This unit measured
  124 in 1 m 39 s.
- The expected mismatches were not rediscovered; none was edited.

## 2. What the owner should expect

- **Hamlet's engine now hears more than one Olivia station at once.** Give it the passband as it
  arrives and it opens a channel for every station that announces itself, and for one that does
  not, each read by its own reader, each with only its own text. On the two-signal recording that
  is two channels, KC3QIS's CQ on one and EI4GNB's on the other, letter for letter. **Nothing on
  screen changes**: it is wired to no panel (decision L), and the Olivia panel still says nothing
  decodes. Drawing the rows is the next unit's.
- **Olivia reads from under the noise.** An 8/250 QSO buried 14 dB below the noise in 2500 Hz -
  25 times more noise power than signal - is read letter for letter, on five different draws of the
  noise. That was already true of the demodulator unit 362 left; this unit proved it.
- **What will look wrong but is not:**
  - Every channel reports one **rejected** block where the whole-file decode reports none. That is
    a block read at a sync that led for a moment before the carrier began. It showed nothing.
  - A channel's text appears **seconds after the block was sent**, not as it arrives: the reader
    waits for the offset track to settle (item 3).
  - The engine carry-forward run takes **4 m 40 s** now (item 2).
  - `PHASE_STATUS.md` still says step 3 `not started` and unit 358, and `PROJECT_STATUS.md` says
    `WORK_INSTRUCTION: 358`, because `tools/status.sh` reads it from there.

## 3. What you should see

**No visible change in the application** - engine and tests only. What it proves (computed, not
seen; nothing here is evidence about the radio, FACT-004):

**3.0 - below the noise** (`TheOliviaBelowTheNoiseTests`, alone in its collection):

| | |
| --- | --- |
| Recipe | fldigi-port burst sliced from `olivia-8-250-cq-rsid.wav`, samples 0-18579 (to 2.3225 s: first tone 0.465 s by the detector + 20 symbols at 10.7666 Hz), then all of `olivia-8-250-qso-norsid.wav`; seeded white Gaussian noise over everything |
| SNR set | -14.00 dB in 2500 Hz (signal 0.093929 over the QSO part; noise variance 3.775039) |
| The method's check | shipped -10 dB file measures **-10.48 dB** |

| Seed | SNR measured back | RSID | CER (0.10) | Characters | Blocks decoded / rejected | Sync S/N | CPU |
| --- | --- | --- | --- | --- | --- | --- | --- |
| **363 (asserted)** | -13.82 dB | OLIVIA_8_250 at 1000.16 Hz, 14 right | **0.0000** | 251 of 251 | 84 / 0 | 9.87 | 5.42 s |
| 3631 | -13.90 dB | at 999.75 Hz, 14 right | 0.0000 | 251 of 251 | 84 / 0 | 9.85 | 5.22 s |
| 3632 | -14.03 dB | at 1000.36 Hz, 15 right | 0.0000 | 251 of 251 | 84 / 0 | 9.68 | 5.16 s |
| 3633 | -14.18 dB | at 999.42 Hz, 13 right | 0.0000 | 251 of 251 | 84 / 0 | 9.69 | 5.16 s |
| 3634 | -13.90 dB | at 999.42 Hz, 15 right | 0.0000 | 251 of 251 | 84 / 0 | 9.60 | 5.20 s |

The decoded text is the manifest's four-line QSO exactly, on all five. The weakest accepted block
on any seed stood at 7.31 against the threshold's 4.0.

**The listener on the two-signal file** (`TheOliviaListenerTests`, quarter-second pieces, told nothing):

| Channel | Found | Variant | Center (manifest, 5 Hz) | Opened | CER against its half (0.05) | Other station's callsign | Blocks decoded / rejected |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 2 | rsid | 8/250 | 1000.00 Hz (1000; detector 1000.29) | 3.500 s | **0.0000** | EI4GNB 0 times | 13 / 1 |
| 1 | rsid | 16/500 | 2003.91 Hz (2000; detector 1999.66) | 3.500 s | **0.0000** | KC3QIS 0 times | 10 / 4 |

Most channels at once across the whole feed: 2. Each channel's characters (38 each) are no more
than its accepted blocks could carry. The 16/500 center of 2003.91 is where its track ended, a grid
step off, in the silence after that station's last block (item 4).

**Decision Z's rows, the listener beside the whole-file numbers** (unit 362's in brackets):

| File | Channels (most at once) | Found | Variant, center | CER | Blocks | Listener CPU / ratio |
| --- | --- | --- | --- | --- | --- | --- |
| 8/250 CQ, clean | 1 (1) | rsid | 8/250, 1000.00 | **0.0000** (0.0000) | 13 / 1 (13 / 0) | 3.05 s / 0.105 |
| 16/500 QSO, clean | 1 (1) | rsid | 16/500, 1000.00 | **0.0000** (0.0000) | 63 / 1 (63 / 0) | 14.20 s / 0.108 |
| 32/1000 QSO, clean | 1 (1) | rsid | 32/1000, 1000.00 | **0.0000** (0.0000) | 51 / 1 (51 / 0) | 11.98 s / 0.112 |
| 16/500 -10 dB | 1 (1) | rsid | 16/500, 1000.00 | **0.0000** (0.0000) | 63 / 1 (63 / 0) | 14.19 s / 0.108 |
| 8/250 no-RSID | 1 (1) | **blind**, opened at 10.250 s | 8/250, 1000.00 | **0.0000** (0.0000 after the search) | 84 / 0 (84 / 0) | 21.84 s / 0.127 |
| noise only | **0 (0)** | - | - | no character | - | 2.73 s / 0.091 |

**The listener's §R13 events, as written** (two-signal file, printed by the test: the two opens and
the first three blocks):

```
Psk31 olivia_channel {"mode":"olivia","variant":"16/500","state":"opened","id":1,"found":"rsid","centerHz":1999.66,"atSeconds":3.5,"readFromSeconds":1.858,"replaySeconds":6.144}
Psk31 olivia_channel {"mode":"olivia","variant":"8/250","state":"opened","id":2,"found":"rsid","centerHz":1000.29,"atSeconds":3.5,"readFromSeconds":1.858,"replaySeconds":6.144}
Psk31 olivia_block {"mode":"olivia","variant":"16/500","id":1,"found":"rsid","centerHz":2000,"atSeconds":1.902,"snr":2.26,"accepted":false,"characters":0,"blocksDecoded":0,"blocksRejected":1,"threshold":4}
Psk31 olivia_block {"mode":"olivia","variant":"8/250","id":2,"found":"rsid","centerHz":1000,"atSeconds":1.878,"snr":3.32,"accepted":false,"characters":0,"blocksDecoded":0,"blocksRejected":1,"threshold":4}
Psk31 olivia_block {"mode":"olivia","variant":"16/500","id":1,"found":"rsid","centerHz":2000,"atSeconds":2.338,"snr":73.86,"accepted":true,"characters":4,"blocksDecoded":1,"blocksRejected":1,"threshold":4}
```

`characters` is a count. The first two blocks are the one rejected block each channel reports: read
at a sync that led before the carrier began. The test asserts every event is `mode: olivia` with a
variant, that no word of three letters or more from either station's text is in any of them, and that
no string value is 12 characters or longer.

**3.6 - the real-time table** (`TheOliviaListenerKeepsUpTests`, alone):

| File | Audio | CPU | Ratio (1.0) | Longest single Add |
| --- | --- | --- | --- | --- |
| two-signal | 28.98 s | 4.453 s | **0.154** | 0.115 s, the piece at 3.25 s |
| 16/500 QSO | 131.38 s | 14.313 s | 0.109 | 0.063 s |
| noise only | 30.00 s | 2.359 s | 0.079 | 0.021 s |

On the no-RSID file the longest single `Add` was **0.983 s** wall (`TheOliviaListenerTests`), on the
look that opened the trial readers and handed each 4 to 6 s of kept audio.

**Decision V's eight rows, `Decode` re-printed after the demodulator's access changes** (unit 362's in
brackets; all identical):

| Fixture | CER | Characters | Blocks decoded / rejected | Demodulator CPU |
| --- | --- | --- | --- | --- |
| 8/250 CQ | 0.0000 (0.0000) | 38 of 38 | 13 / 0 | 0.797 s (0.813) |
| 16/500 QSO | 0.0000 (0.0000) | 251 of 251 | 63 / 0 | 4.094 s (4.109) |
| 32/1000 QSO | 0.0000 (0.0000) | 251 of 251 | 51 / 0 | 3.625 s (3.656) |
| 16/500 -10 dB | 0.0000 (0.0000) | 251 of 251 | 63 / 0 | 4.453 s (4.469) |
| 16/500 -16 dB | 0.9044 (0.9044) | 24 of 251 | 6 / 57 (6 / 57) | 4.094 s (4.172) |
| noise as 8/250 | 0 characters (0) | 0 | 0 / 14, highest 3.45 (3.45) | 1.078 s (1.031) |
| noise as 16/500 | 0 characters (0) | 0 | 0 / 14, highest 3.27 (3.27) | 1.047 s (1.000) |
| noise as 32/1000 | 0 characters (0) | 0 | 0 / 14, highest 3.15 (3.15) | 1.406 s (1.078) |

## 4. What's blocking us

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

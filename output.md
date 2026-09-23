```
READ IN THIS ORDER.

A. The phase: CW decodes again. Steps 1, 2 and 4 are met on every criterion, and so is
   step 0. Step 3 is now ticked on all nine criteria, 3.8 by this unit. Step 5 is Tim's
   verdict at the radio. The phase now waits on Tim alone, subject to the one question
   in section 4 about what 3.8's tick is worth.
B. Step 3, criterion 3.8: 0 of 5 single-sender cases apart at entry and 0 of 5 at exit,
   out of 38 cases tabled. No tracker change was made, because none was licensed.
   #15 0.54, #43 2 + 7 and #44 0 + 7 are each measured, still red and still parked.
   3.8 is ticked on the instruction's rule. The rule admits only 5 cases, and on the
   three synthetics the sweep sits 60 to 85 Hz off the known tone.
C. The per-case table is in section 3 and docs/phase-cw/unit409-pitch.md. Section 4
   raises 1 item. It does not block 3.8 on the instruction's rule, but it asks whether
   the tick should stand.
```

```
UNIT:       409 - complete at task 4 of 4, none dropped - 2026-09-23 17:56
PHASE GOAL: bring the engine's CW decoder back to what it read on the air on 2026-08-25, hold it there with three floor tests, clear or park the inherited reds, then have Tim judge it at the radio
UNIT GOAL:  measure, on every saved recording, whether the decoder mixes at the pitch where an independent instrument hears the keying; change the tone tracker only where they are more than a bin apart; measure the three parked reds afterward
ADVANCED:   yes - 3.8, the last loop-movable criterion of step 3, is ticked on a measured per-case table
NUMBER:     single-sender cases more than one bin apart: 0 -> 0 of 5, from 38 cases tabled
DRIFT:      0 consecutive units without advance  (was 0)
```

## 1. What Claude did

**Complete at task 4 of 4, with tasks 0 to 4 all run and none dropped.** Task 2 made no change,
because the entry table licensed none, as the instruction orders. The session ran on QUIVERFULL in
`C:\Source\HamLet`, branch `main`, after the project gate confirmed Hamlet. Commits `eff57237`,
`96136b70`, `aa824eaf`, `e3a0aac8` and `a9b96c47`, plus this report's commit, were all pushed.

**Mismatches against section 5 of the instruction.** Each is reported here and none was repaired:
- **`LockedToneHz` is at line 279 of `CwDecoder.cs`, not `CwToneTracker.cs`.** It is NaN unless
  the operator locks. The pitch the decoder actually mixes at is `CwProbabilisticStream.ToneHz`,
  read as `CwDecoder.Stream.ToneHz`. `CwDecoder.Step` sets it on every hop from the lock, else the
  last pitch the tracker measured, else the tracker's bank centre. **That is the property used.**
- The app's "400 to 1200 Hz" label is at `MainWindowViewModel.cs` lines 11915 to 11916. The engine
  sweeps 300 to 900. This is parked, not edited.
- `PHASE_STATUS.md` and `PHASE_OUTCOME.md` still read step 0 and step 2 as `not started`, though
  both are ticked in the plan. The step lines were not edited. `CURRENT_STEP` was set to 3.
- **`WhatBandwidthTheDecoderListensThroughTests` is 3 of 6 at entry**, where unit 407 recorded
  4 of 6. The new red is `HoldingTheWindowLongInTimeReadsMore(003016)`, which reads 54 against 54.
  It asserts that holding the window reads more, and it reads the same. Unit 408 did not run this
  reader. It is in neither the 51-name set nor a carry-forward line. It was 3 of 6 at exit too.
- The rest held as stated:
  - the three anchors' recordings are `013347`, `134712` and `003758`, all among the 37 rows
  - the 17:37 WAV and key are present, and its `.txt` sidecar is absent
  - the recipes: #15 at 640 Hz, #43 and #44 at 615 Hz drifting ±3
  - `CwTwoStationTests` reads only the synthetic `receiver\two-station.wav`, so the single-sender
    rule's first clause removes no case

**Task 0.**
- Version 1.13.96. `PHASE_STATUS.md` names unit 409, and `PHASE_OUTCOME.md` has the unit 409 entry.
- `git diff 0534ca93 HEAD` over src, tests and the list printed nothing, because HEAD was
  `0534ca93`. **So the two carry-forward lines were taken as unit 408's exit**: engine 178 of 178,
  app 278 of 278.
- Entry round, one type per invocation after one build:
  - captures 37 of 37, adjudicated 13 of 13, synthetics 2 of 2
  - receiver 25 of 27, acquisition 11 of 12, 17:37 5 of 5
  - every tracker reader at unit 407's count, except the bandwidth reader above

**Task 1.** `TheTwoPitchesTableTests.EveryCaseIsTabled` is a printer that asserts nothing. It
reads 38 cases the way the floors do, one hop at a time: the 37 capture rows, with the three
anchors marked, and 17:37. It ran twice at entry, and the rows were identical. **5 of 38 cases are
single-sender, and 0 of them are apart.**

**Task 2.** No change. There was nothing apart to trace.

**Task 3.**
- The exit table matches entry line for line.
- #15, #43 and #44 were run by name. All three are still red, and each is pitched by
  `TheThreeParkedRedsArePitched`, a second printer.
- **3.8 is ticked in `PHASE_PLAN.md`**, with the caveats written into the tick.

**Task 4, the exit round.**
- Engine line 178 of 178 in 367 s.
- **App line 276 of 278, twice.** Every loss was the dispatcher loop, before any assertion.
  - `Unit376TheTopBandTests` and `ThePsk31OfferTests` were each lost in one run and green in the
    other.
  - **`TheWindowHoldsBelowItsMinimumTests` was lost in both runs.** The rule's one re-run was
    spent, so I ran that name alone once, outside the line, and it was 1 of 1 green. It counts
    neither way. Its green comes from that solo run and not from the line.
- Captures 37 of 37 and adjudicated 13 of 13, **every printed row and reading identical to entry**.
- Synthetics 2 of 2. Receiver 25 of 27. Acquisition 11 of 12. 17:37 5 of 5.
- Every tracker reader is at its entry count.
- The transmit files print nothing against `7e209cb4`. `src/Hamlet.App` prints nothing, and **all
  of `src` prints nothing**, against `0534ca93`. This unit changed no production code.

**Decisions I made myself, each the author's and overrulable:**
1. **The decoder's pitch is `Stream.ToneHz`**, since `LockedToneHz` is not what it mixes at. The
   most-held value is rounded to a hertz and weighted by hops, over the span from the hop the first
   named character settled to the hop the last one did.
2. **Second-best is taken on the 300 to 900 sweep**, the judging column, over candidates more than
   50 Hz from the winner.
3. **The keying verdict is `CwKeyingMeter`'s four-part test asked once of the whole recording.**
   The meter itself runs six-second windows and holds its verdict across quiet windows. Asked of a
   whole 30 s file, the swing figure runs lower. That is why 17:37 and 17 other cases fail on
   swing. I did not tune this, because the instruction says the rule is fixed.
4. **After the first entry run, I added the sweep's four keying figures to the printer's output**
   to explain the verdicts. The rows did not change, and the rule did not change.
5. **The parked reds are fed hop by hop from their own test's starting pitch.** The tests pump
   through `BufferedAudioSource`, so the chunking differs from theirs.
6. **The one solo run of `TheWindowHoldsBelowItsMinimumTests`**, described above.
7. **3.8 is ticked on the instruction's letter, and the doubt goes to section 4** rather than
   being settled by me. Ticking it and leaving it open are both readings I could defend. The
   instruction says what ticks it, and both conditions hold.

## 2. What the owner should expect

Nothing about how the CW tab behaves has changed, because this unit changed no decoder code. What
it adds is a measurement. The question was whether the decoder listens at the pitch where a
separate instrument hears somebody keying. On the five saved recordings that clearly hold one
sender, the answer is yes, within 5 Hz. That includes the anchor that carries `AA4MP/4 QNIK`. So
the tone tracker was left alone.

That does not mean the text is right. It also covers fewer recordings than it sounds. The
instrument only calls 5 of the 38 recordings "one clear sender". On the other 33 it either does
not hear keying strongly enough by its own bar, or it hears a second pitch almost as strong. And on
the three synthetic test signals, where the true pitch is known, the instrument itself lands 60 to
85 Hz high. The decoder is closer to the truth there than the instrument is. At 17:37, the bench
decoder sits at 575 Hz and the instrument at 600. That counts as agreement.

**What will look wrong but is not:**
- 3.8 is ticked while #15, #43 and #44 are still red. The criterion asks for them to be measured,
  and they were.
- One app test was lost to the test host twice. It passes alone, and the app code is untouched.

## 3. What you should see

**3.8: 0 of 5 single-sender cases apart, at entry and at exit.** The table is the same at both,
because nothing under `src` changed. The two sweep columns are both shown, the verdict is judged on
300 to 900, and only single-sender cases are judged.

| case | anchor | named | decoder Hz | low | high | share apart | sweep 300-900 | sweep 400-1200 | second-best | keying | single-sender | diff Hz | agree |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 013347 | anchor | 57 | 600 | 600 | 625 | 0.00 | 600 | 600 | 0.58 | no | no | 0 | agree |
| 134712 | anchor | 21 | 500 | 500 | 500 | 0.00 | 500 | 500 | 0.74 | no | no | 0 | agree |
| **004507** |  | 49 | 500 | 475 | 525 | 0.00 | 500 | 500 | 0.30 | keying | **yes** | 0 | agree |
| 012403 |  | 21 | 440 | 435 | 450 | 0.00 | 450 | 450 | 0.29 | no | no | 10 | agree |
| 031838 |  | 43 | 525 | 500 | 525 | 0.00 | 500 | 500 | 0.62 | keying | no | 25 | agree |
| **031905** |  | 36 | 500 | 300 | 500 | 0.34 | 500 | 500 | 0.49 | keying | **yes** | 0 | agree |
| 031948 |  | 31 | 500 | 500 | 500 | 0.00 | 500 | 500 | 0.67 | keying | no | 0 | agree |
| 032012 |  | 43 | 500 | 500 | 500 | 0.00 | 500 | 500 | 0.71 | keying | no | 0 | agree |
| 032050 |  | 44 | 500 | 325 | 500 | 0.23 | 500 | 500 | 0.57 | keying | no | 0 | agree |
| 032113 |  | 47 | 500 | 500 | 650 | 0.33 | 500 | 500 | 0.69 | keying | no | 0 | agree |
| 032129 |  | 65 | 650 | 500 | 650 | 0.65 | 500 | 500 | 0.69 | keying | no | 150 | apart |
| 013622 |  | 51 | 600 | 600 | 625 | 0.00 | 600 | 600 | 0.43 | no | no | 0 | agree |
| **003016** |  | 54 | 670 | 650 | 675 | 0.00 | 675 | 675 | 0.32 | keying | **yes** | 5 | agree |
| **003126** |  | 48 | 670 | 650 | 680 | 0.00 | 675 | 675 | 0.32 | keying | **yes** | 5 | agree |
| **003758** | anchor | 44 | 500 | 485 | 500 | 0.00 | 500 | 500 | 0.30 | keying | **yes** | 0 | agree |
| 001520 |  | 1 | 600 | 600 | 600 | 1.00 | 525 | 1000 | 1.00 | keying | no | 75 | apart |
| 001831 |  | 44 | 525 | 500 | 530 | 0.00 | 525 | 525 | 0.39 | no | no | 0 | agree |
| 001952 |  | 57 | 525 | 375 | 575 | 0.79 | 475 | 475 | 0.72 | no | no | 50 | apart |
| 002016 |  | 44 | 525 | 525 | 525 | 1.00 | 475 | 475 | 0.52 | no | no | 50 | apart |
| 011552 |  | 22 | 500 | 495 | 500 | 0.00 | 500 | 500 | 0.23 | no | no | 0 | agree |
| 012748 |  | 2 | 400 | 400 | 400 | 0.00 | 400 | 400 | 0.12 | no | no | 0 | agree |
| 012823 |  | 26 | 500 | 500 | 500 | 0.00 | 500 | 500 | 0.18 | no | no | 0 | agree |
| 012922 |  | 45 | 500 | 475 | 500 | 0.00 | 500 | 500 | 0.75 | keying | no | 0 | agree |
| 013010 |  | 48 | 500 | 475 | 505 | 0.00 | 500 | 500 | 0.31 | no | no | 0 | agree |
| 013150 |  | 51 | 500 | 495 | 525 | 0.09 | 525 | 525 | 0.41 | no | no | 25 | agree |
| 013303 |  | 44 | 500 | 495 | 505 | 0.00 | 500 | 500 | 0.32 | no | no | 0 | agree |
| 013402 |  | 56 | 525 | 525 | 550 | 0.00 | 550 | 550 | 0.57 | keying | no | 25 | agree |
| 013520 |  | 55 | 550 | 525 | 550 | 0.00 | 550 | 550 | 0.58 | keying | no | 0 | agree |
| 013637 |  | 60 | 525 | 525 | 550 | 0.00 | 550 | 550 | 0.60 | keying | no | 25 | agree |
| 021410 |  | 36 | 535 | 535 | 550 | 0.00 | 550 | 550 | 0.53 | no | no | 15 | agree |
| 021629 |  | 27 | 500 | 500 | 500 | 0.00 | 500 | 500 | 0.68 | no | no | 0 | agree |
| 021825 |  | 25 | 400 | 390 | 410 | 1.00 | 325 | 600 | 0.79 | no | no | 75 | apart |
| 125941 |  | 0 | 400 | 375 | 600 | 0.80 | 450 | 450 | 0.89 | no | no | 50 | apart |
| 014854 |  | 0 | 600 | 600 | 825 | 0.48 | 575 | 575 | 0.33 | no | no | 25 | agree |
| 014935 |  | 0 | 625 | 600 | 825 | 0.43 | 600 | 600 | 0.69 | no | no | 25 | agree |
| 014113 |  | 0 | 600 | 600 | 725 | 0.43 | 600 | 600 | 0.39 | no | no | 0 | agree |
| 014308 |  | 0 | 575 | 450 | 600 | 0.75 | 625 | 625 | 0.21 | no | no | 50 | apart |
| 17:37 |  | 46 | 575 | 575 | 600 | 0.00 | 600 | 600 | 0.07 | no | no | 25 | agree |

Each case is named by its capture time, dates 2026-08-17 to 2026-09-23.

**The sweeps disagree on two cases, and each is apart on either column:**
- `001520`: 525 against 1000, with the decoder at 600
- `021825`: 325 against 600, with the decoder at 400

**Outside the rule, 7 cases are apart.** On `031905`, a single-sender case, the most-held pitch
agrees, but the mix spends 0.34 of the named span at 300 Hz.

**The tracker change row:** no change was made, 0 cases were apart before and after, and nothing
was built, kept or reverted. The named counts, the anchors and 17:37, which stays 46 named, are
identical to entry, because `src` is untouched.

**The three parked reds, post-R56:**

| red | value | green | decoder | sweep | recipe |
|---|---|---|---|---|---|
| #15 `TheSlowEndReadsTheMessage(12, 18)` | 0.54 against 0.66 | no | 650, 650, 700 over its three seeds | 725 | 640 |
| #43 `TheEasyTierIsReadWhole(coverage-easy)` | 2 unreadable + 7 strangers | no | 625 | 675 | 615 ±3 |
| #44 `TheEasyTierIsReadWhole(exchange-easy)` | 0 + 7 | no | 625 | 675 | 615 ±3 |

#43 now reads 2 + 7 against `PARKED.md`'s 4 + 7. The difference is unit 408's gate removing two
placeholders, not anything done in this unit.

## 4. What's blocking us

**1. Should 3.8's tick stand on a table whose judge admits 5 of 38 cases and misses a known tone
by 60 to 85 Hz?**
- **Recommended ruling:** let the tick stand. Record in the plan that 3.8 is met on five
  single-sender recordings, and that the sweep's pitch resolution on a strong clean tone is
  coarser than the one-bin tolerance.
- **Reasoning:**
  - The instruction fixed the rule and the tolerance before the table was read, and both
    conditions for the tick hold.
  - On all five admitted cases the decoder is within 5 Hz of the sweep.
  - On the three synthetics with a known tone, the decoder is nearer the truth than the sweep:
    10 to 60 Hz high, against 60 to 85.
  - So nothing in the table argues for a tracker change.
- **Rejected: leaving 3.8 open.** No tracker change this phase allows could make the judge
  better, and changing the sweep is barred. An open 3.8 would be one no unit can close.
- **Rejected: widening the rule after the fact to take in the 11 keying cases that have a strong
  runner-up.** The instruction forbids changing the rule once the table is read. Of those 11, only
  `032129` is apart.
- **In the way of 3.8?** Not on the instruction's rule. It becomes blocking only if Tim decides
  the tick is worth less than it reads. Then 3.8 goes back to open, and the next unit needs a
  judge with finer pitch resolution, which the plan does not currently allow.

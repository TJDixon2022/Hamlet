UNIT:       044 — complete at task 4 of 4 — 2026-08-28 20:17
PHASE GOAL: Readable CW on the operator's screen — eighty percent of a strong signal read correctly, first time.
UNIT GOAL:  Make the ranked pitch drive the live decode, and stay silent when even the best candidate is poor.
ADVANCED:   no — the ranking is built, wired end to end and switched off, because driving the decode costs two adjudicated callsigns.
NUMBER:     34 of 44 offline -> 27 of 44 at the window the live decoder actually sees; on the operator's screen, unchanged.
DRIFT:      1 consecutive unit without advance  (was 0 — unit 1.12.6 recorded no count and its own ADVANCED was yes)

## 1. What Claude did

**Complete: all four tasks worked, and the goal task ships switched off.** Task 4
was not dropped for time — it applies only where task 3's floor is holding, and no
floor holds.

Development computer, prompt claimed `PROJECT: Hamlet`, branch `main`, version
`1.12.6` unchanged. **Nothing here is evidence about the radio**: no radio was
connected, and every number comes from WAV files already in the tree.

### Task 1 — the pedestal in the engine

`CwPitchRanking` now holds it, and **the engine's copy reproduces the tool's
forty-four rows byte for byte** — the check the order said to stop on if it failed.
Seven tests, all green.

**One finding worth more than the move itself: the generated fixtures cannot show
this fault.** The control test was written against `CwFixtureGenerator`'s shaped
band and failed, because there the *bare* score picks the station correctly, at
500 Hz scoring 39.45. The receiver's own 500 Hz filter empties the bins outside it
far harder than the generator's band shaping does, so a session working only from
synthetic audio would conclude there was nothing to fix. The test moved to
`cw-2026-08-28-004844`, where the bare score picks 875 Hz.

### Task 2 — the ranked pitch drives the decode, and is switched off

Built exactly as ruled: ranking on a four-second window, once on tune-in and again
when the reading sits under the gate for six seconds, the full path on the winner
alone. The mixdown takes it at `CwDecoder.Step` behind the operator's lock;
`CwDecodeReport.ToneHz` is fed from the pitch the decode used rather than from
`_tracker.ToneHz`; the sheet says the pitch was ranked and prints the winner's
score and the runner-up's; admission is untouched and still recorded.

**Then it was measured, and it costs two adjudicated anchors:**

| capture | with the tracker steering | with the ranking driving |
|---|---|---|
| `cw-2026-08-17-013347` | `… VA3VRR` at 625 Hz | nothing readable, at 775 Hz |
| `cw-2026-08-24-012403` | `DE KD0UN KD0UN K` at 440 Hz | `DE XD0UN KD0` at 450 Hz |

The unit's acceptance requires all twelve anchors green. **So it ships off**, as
`CwDecoder.RankThePitch`, on `ClearOnAStationChange`'s precedent — the machinery
stays, tested and measurable, and a test goes red the moment the default changes so
that whoever flips it re-measures those two first. With it off the shipped
behaviour is what it was this morning.

**The two failures have different causes and only one is the ranking being wrong.**

- On `012403` **the ranking picks the right bin.** The candidates are the tracker's
  coarse grid, so a ranked pitch is only ever a bin centre: it chooses 450 and the
  station sits at 440, and ten hertz turns `KD0UN` into `XD0UN`. The survey
  interpolates to the hertz and the ranking cannot. A refinement — take the
  survey's pitch where it falls inside the winning bin — was built, measured, and
  did not fire on this capture, so it was removed rather than shipped untested.
- On `013347` **the pedestal collapses.** The opening four seconds hold no station,
  so every bin's floor is tiny, the common floor is tiny with them, and the scale
  invariance the pedestal exists to remove comes straight back: the winner scores
  **5,521,967** at 775 Hz. **A degenerate pitch looks maximally healthy**, so the
  collapse test that would re-rank it can never fire.

**And the window's position matters more than its length, which no measurement
before this one separated.** The 34 of 44 that commissioned this unit was measured
on the *tail* of each recording. The live decoder ranks at tune-in, which is the
opening:

| stretch | 4 s | 6 s | 8 s | 12 s |
|---|---|---|---|---|
| tail | **34** | 34 | 35 | 34 |
| opening | **27** | 31 | 32 | 33 |

The ruling chose four seconds on cost and the accuracy at four seconds was never
measured. At the tail it costs nothing at all; at the opening it costs seven
captures against twelve seconds.

**The three good captures keep every readable token, and the strings are not
identical.** Reported precisely rather than as "unchanged": `004844` is identical;
`004902` drops one junk `E` (`IELE` to `IEL`); `004915` gains one (`QSAETU` to
`QSSAETU`). Every readable run — `TUES AU G 2 5`, `<BT> BRU C E <AR> NR 2 3 0`,
`WED AUG 26 W7GB QRU` — comes back character for character.

**The measured cost, not estimated.** A four-second sweep of twenty-five candidates
is 15.6 ms a candidate, **390 ms once**. Ranking ran once on six of the seven
captures of 2026-08-28 and twice on one, so on a thirty-second recording that is
about 1.3 % of one core — not a continuous load, because it is not a continuous
pass.

### Task 3 — the floor, swept and not shipped

**No floor both silences the phantoms and keeps every anchor, and it is not close.**

The four junk captures score **0.01, 0.33, 5.09 and 5.25**. Eight adjudicated
readings score below 5.09, and `cw-2026-08-17-134712`, which holds `N4L`, scores
**0.00**. The whole sweep, as the order requires rather than a chosen value:

| floor | captures silenced | anchors lost | anchors kept |
|---|---|---|---|
| 0.00 | 2 | 0 | 15 |
| 0.25 | 5 | 1 | 14 |
| 0.50 | 10 | 3 | 12 |
| 1.00 | 17 | 3 | 12 |
| 1.50 | 24 | 6 | 9 |
| 2.00 | 27 | 8 | 7 |
| 3.00 | 31 | 9 | 6 |
| 5.00 | 32 | 10 | 5 |
| 5.50 | 34 | 10 | 5 |
| 7.50 | 36 | 12 | 3 |
| 14.00 | 37 | 13 | 2 |

Which anchor each value costs, in the order they fall: 0.25 loses `134712`
(`N4L`, 0.00); 0.50 adds `031905` (0.27) and `012403` (`KD0UN`, 0.35); 1.50 adds
`032113` (1.27), `031948` (1.34) and `032129` (1.47); 2.00 adds `032012` (1.71) and
`031838` (1.73); 3.00 adds `032050` (2.63); 4.00 adds `004507` (the ARRL bulletin,
3.21); 7.00 adds `003758` (`AA4MP/4 QNIK`, 6.90); 7.50 adds `004915` (7.32); 14.00
adds `004844` (13.90).

**So no floor ships**, which is what the order said to do in this case. Unit
1.11.33 found no threshold separated the corpus in the old units; these are new
units and the finding repeats in them.

### Task 4 — not applicable rather than dropped

The line was to appear "where task 3's floor is holding". No floor holds, so there
is no state for it to describe, and inventing one would be a sentence about a
condition that never occurs. **Nothing on the screen moved.**

No decision was recorded under §12.1. Everything above is handed back.

## 2. What the owner should expect

**On a frequency where a station is sending, Hamlet lands on it exactly as often as
it did; on one where nothing is, the screen goes on filling with letters.** That is
the opposite of what the order asked this section to lead with, and the reason is
task 2: the ranking is built and switched off, because switching it on loses
`VA3VRR` and `KD0UN`.

What is now true of the tree:

- `CwPitchRanking` is in the engine with nine tests on it, and the tool calls it
  rather than keeping a second copy.
- `CwDecoder.RankThePitch` exists and is **false**. Setting it true is one line and
  a rebuild, and the two captures above are what it costs.
- The capture sheet will say a pitch was ranked, and print both scores, on any
  build where that switch is on.
- `tools/Hamlet.PitchRank` gained `live`, `floor` and `window`, so every figure
  above is one command away from being re-measured.

**What will look wrong but is not:**

- **The engine baseline is still 28 failing.** `CwEmissionGateTests.NoSpeedIsNamed­WithoutCharactersToNameItFrom` is among them and was among them this morning.
- **`Report.ToneHz` changed meaning even with the ranking off.** It used to be the
  tracker's pitch and is now the pitch the mixer was actually run at, which with
  ranking off is the last measured pitch rather than the tracker's live one. On the
  batches run it moved nothing; it is a real change and it is named here rather
  than left to be discovered.
- **The full engine suite has no result in this report.** It was killed twice
  mid-run, as it was in unit 1.12.6. What did run: the twelve adjudicated anchors
  green, the nine new tests green, and a thirty-one test batch over the pitch-hold,
  emission-gate, phantom-block and tracker-switch cases with one failure, that
  pre-existing one.
- **The app suite has no result either**, for the same reason. No app file changed
  except the sheet's pitch line.
- **`eng-final.txt` and `app-final.txt` are still modified in the working tree**,
  from commit `cf81849`, and were left alone (§12.6).

## 3. What you should see

**With the ranked pitch driving the live decode, the three good captures read what
they read before and the four phantoms still fill the screen — and two adjudicated
callsigns disappear. The cost is 390 ms once at tune-in, about 1.3 % of one core
over a thirty-second recording.**

That is the answer to what this unit was commissioned to ask, and it is why nothing
reaches the operator tonight.

The useful part is that the two remaining faults are now named and separated, and
neither is the idea being wrong. **The ranking picks the right bin and cannot pick
the right pitch inside it** — twenty-five hertz of quantisation is the difference
between `KD0UN` and `XD0UN`. And **the pedestal needs a band with noise in it**: on
four seconds of an empty opening there is no floor to stand anything on, the score
blows up to five and a half million, and because that looks like the healthiest
signal Hamlet has ever seen, nothing re-ranks it.

Both have obvious shapes of answer and neither is this unit's to choose.

## 4. What's blocking us

Two rulings, the first blocking the more work.

> **The ranking chooses the bin and the survey chooses the pitch inside it, and
> the ranking refuses on a band with no floor to stand on.**
>
> Ranking supplies the mixdown only as a bin; where the survey has a measured pitch
> inside the winning bin, that pitch is used. Measured: `cw-2026-08-24-012403`'s
> station sits at 440 and the ranking picks the 450 bin correctly, and the ten
> hertz costs `DE KD0UN KD0UN K`. Separately, where the band's common floor is too
> small for the pedestal to mean anything, nothing is ranked and the tracker keeps
> steering — `cw-2026-08-17-013347`'s opening four seconds score 5,521,967 at a
> pitch holding nothing.
>
> **Rejected: shipping the ranking as it stands.** It costs two adjudicated
> callsigns and the unit's own acceptance forbids it.
> **Rejected: catching the degenerate case with the collapse test.** Measured
> impossible in principle: the degenerate pitch scores five and a half million, so
> it looks healthier than any real station and the collapse test can never fire.
> **Rejected: a refinement built on the last measured pitch alone.** Built and
> measured this session; it did not fire on `012403`, so it was removed rather than
> shipped untested. What it needs is the tracker's live pitch as well, and that is
> a wider change than a session should make on its own.
> **What is not yet decided is where the "no floor to stand on" line sits**, and
> that is a number, which means a sweep and not a judgement.

> **Where in the recording the ranking reads, given that position matters more
> than length.**
>
> Ranking the tail of a recording picks the station on 34 of 44 captures at four
> seconds; ranking the opening four seconds, which is what tune-in sees, picks it
> on 27. The four-second window was ruled on cost and its accuracy was never
> measured; at the tail four seconds costs nothing against twelve, and at the
> opening it costs seven captures.
>
> **Rejected: lengthening the window to twelve seconds.** It recovers most of the
> gap at the opening, 27 to 33, and costs 1240 ms a sweep against 390 — which is
> affordable at one sweep on tune-in, so this is a live option rather than a dead
> one and it is named as such.
> **Rejected: ranking later than tune-in without a rule for when.** "A few seconds
> after the operator stops turning the dial" is probably right and is a behaviour
> nobody has specified.

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140, from unit 1.12.6's list.

1. **`DRIFT` had no count to carry** — unit 1.12.6's block carried none. This
   report starts the chain at 1, on its own `ADVANCED: no`.
2. **The ranking's ten misses of forty-four are unexamined** — which captures, and
   whether they share a shape. Still unexamined; this unit measured the window
   instead.
3. **Admission admits a pitch 150 Hz off the station and holds it for forty-five
   seconds without a refresh**, the held peak decaying at exactly 1 dB per second
   because nothing refreshed it.
4. **The `reading` line's span wording needs approval.**
5. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
6. **Two stations closer than 125 Hz are not named.**
7. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
8. **Nothing checks that deleting a surface is not deleting a capability** — the
   operator has since found the favourites list gone. **The next unit unless he
   says otherwise.**
9. **A capture sheet carries a score of −68562.4** (`cw-2026-08-28-005158`), first
   raised in unit 1.12.6 and unruled. A number the operator can read with nothing
   beside it saying what it means.
10. **The full engine suite has now been killed mid-run in two consecutive
    sessions**, both times at around twenty minutes. Acceptance is being assembled
    from filtered batches, which is slower and leaves gaps. Raised as a working
    condition rather than a code fault.

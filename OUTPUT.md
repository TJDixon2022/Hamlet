UNIT:       056 — complete at task 6 of 6 — 2026-08-31 11:05
PHASE GOAL: Read 85% of the characters correctly on a capture where the pitch is right, precision before yield.
UNIT GOAL:  Lower the speed ceiling and re-sweep the hold-over; measure every element's own pitch; split near-tied senders; and find why Hamlet reads letters where an independent decoder reads `CQ … K`.
ADVANCED:   **yes.** Precision 0.889 → **0.901** with all five named locks green, and task 4's question answered by measurement; the unnamed character ratchet goes 15 of 38 red to 18 and that is section 4's first ask.
NUMBER:     **precision 0.889 -> 0.901, yield 0.872 -> 0.878, substitutions 20 -> 16.**
DRIFT:      0 consecutive units without advance.

## 1. What Claude did

**Complete. Six tasks of six, and task 6 — the named drop — was not dropped.**
Hamlet confirmed against all four gate checks before the order was read.
Development machine, project claimed `Hamlet`, branch `main`, five commits, every
push succeeded. **Nothing here is evidence about the radio.**

**Two acceptances were not met and are reported as named residues rather than
forced, and one unnamed regression guard moved the wrong way.** Task 3's split does not fire on `002829` and task 4's `003229` does not
read `CQ` through the shipped path. Both are measured refusals with their evidence
attached, which the order asks for in preference to a forced pass.

### The two rulings, recorded as the order gives them

> **HM-DEC (for Tim to enter): the corpus precision floor is redefined.** The
> average floor may move only when **every individual lock holds** — the
> clean-read locks, the adjudicated anchors, and both silence locks. An average
> can never again be traded against a collapsed easy read, because the easy reads
> are individually locked. **Unit 055's 0.889 against the prior 0.894 is accepted
> under this rule and is the worked example**: yield rose 0.750 → 0.872 while
> every individual lock stayed green.
>
> Rejected: reverting — it returns `003229` to a wall of blocks and `002443` to
> 48 `E`s from noise. Rejected: treating the average floor as inviolable on its
> own — its purpose was always carried by the per-capture locks.

> **HM-DEC (for Tim to enter): `CwProbabilisticDecoder.FastestWpm` is lowered
> from 40 to 30, provisionally.** The ceiling **rises again the day a capture
> shows something faster worth reading** — record that condition with the value.
> The hold-over's safety bound rises from 30 ms to 40 ms with it, reaching the
> lower half of the measured 32–53 ms dropouts.
>
> Rejected: keeping 40 — nothing in the corpus, the bulletins, or any capture the
> operator has sent runs above about 28 WPM, and the bound was sitting just under
> the fault it was built for.

### Task 1 — the ceiling, and the hold-over re-swept

**Baseline verified against the tree before anything changed:** precision 0.889,
yield 0.872, substitutions 20, and all five lock suites green at 42 tests. Every
figure matches the order's stated baseline exactly.

`FastestWpm` is 30 with the condition that raises it in a comment beside the
value. `LongestSafeHoldOverMs` is derived from it, so the safe bound went 30 ms to
40 without a second edit. **The ceiling alone moved precision 0.889 → 0.901,
yield 0.872 → 0.878, substitutions 20 → 16** — visible on `032050`, which goes
`TELEWRITTER, PACKETY AN MT TINTERE` to `TELEWRITTER, PACKETY AND INTERE`.

**The hold-over swept across the whole newly legal range:**

| hold | yield | precision | subs |
|---|---|---|---|
| **12 ms** | **0.878** | **0.901** | **16** |
| 16 ms | 0.875 | 0.926 | 14 |
| 20 ms | 0.878 | **0.939** | **12** |
| 24 ms | 0.878 | **0.939** | **12** |
| 28 ms | 0.870 | 0.930 | 15 |
| 32 ms | 0.870 | 0.920 | 17 |
| 36 ms | 0.841 | 0.910 | 24 |
| 40 ms | 0.841 | 0.910 | 24 |

Monotonic to 20, tied at 24, falling after. **On the average the answer is 20 and
it is not taken**, because it costs `cw-2026-08-22-031838`'s adjudicated `, AND`,
which the order says still governs. Measured at all three points, the read goes
`, 2, 2, AND 2` at twelve, `, 2, 2,■AND■2■` at sixteen, `, 2, 2,■■AND■■■` at
twenty. **The mechanism is visible in that progression**: bridging inside a
key-down lengthens the mark and shortens the gap after it, so this sender's
character gaps fall in among his element gaps and the spacing collapses into
blocks. **Twelve stands.**

**What the wider bound does to the shredded pair**, measured though nothing is
promised for them:

| | 12 ms | 20 ms | 40 ms |
|---|---|---|---|
| `003408` | 37 named, 25 blocks | 37, 24 | 33, 23 |
| `003419` | 30 named, 30 blocks | 30, 26 | 26, 28 |

Blocks fall a little and named characters fall with them. `73` survives at twelve
and twenty and is lost at forty. **Nothing reads these at any bound.**

### Task 2 — every element carries its own pitch

The decoder now hands out the winning path's own elements on the result,
**produced by the same walk that spells the text**, so the stream and the letters
cannot disagree. Each mark carries the frequency measured over its own samples by
a transform at that element's own bin spacing, Hann-windowed, with a parabola on
the log magnitude.

**The resolution is the element's own length and nothing buys past it**: a 190 ms
dah separates tones about 5 Hz apart, a 55 ms dit about 18 Hz. On clean
synthesized tones a single isolated tone is *located* far better than that — 0.05
to 0.23 Hz across seven cases — but locating one tone and separating two are
different questions, and the tests assert against the separation limit because
that is the claim task 3 rests on. An element too short to beat its own search
returns "nobody measured" rather than the mixdown pitch.

**Acceptance met: the corpus is identical to the digit.** 0.901 / 0.878 / 16
before and after the plumbing.

### Task 3 — the split is measured and the verdict is withheld

`CwStreamSplit` clusters an admitted station's marks by their measured pitch and
reports the two centres, their counts, the separation, the pooled scatter, the
separation in units of that scatter, and how many times the marks cross the
boundary in time order. **It returns no split, and that is the finding.**

Four criteria were surveyed across every capture in the tree. Each either misses
the case the order names or fires on a recording known to hold one operator.

| criterion | clean captures | `002829` | what breaks it |
|---|---|---|---|
| separation, Hz | 0.1 to 0.8 | 9.0 | `001831` gives 7.2 and `005051` 8.7; the only two clearing 15 are `003212` and `003229` |
| separation over scatter | 1.5 to 3.7 | 4.2 | `001831` scores **17.2** on 7.2 Hz, because a steady sender leaves nothing to divide by |
| handovers | 20 to 90 | 14 | a bisected heap crosses constantly; a real burst crosses twice, so the test has the sign the data does not |
| trough in sorted pitches | 0.10 Hz | 1.30 Hz | the wrong way round: that second sender is mostly dits, and letting dits vote lets every noise-fitted short mark vote |

**The second sender is not in dispute.** Read in time order at a mixdown of 608.5
Hz, `002829` puts **thirteen consecutive marks at 599 to 605 Hz between 13.58 and
15.45 seconds**, with 613 Hz either side. The eye finds it at once. What does not
exist is a rule that finds it without also finding one in `cw-2026-08-18-003758`,
which Hamlet reads at a precision of 1.000.

**So nothing is split** (§0.0, and the order's own "when in doubt, one sender").
`NoSenderIsSplitInTwoTests` locks the refusal so a later unit changing it does so
deliberately.

### Task 4 — the element streams agree, so the reading is lost after them

**The unit's centre, and the answer is a negative one.** Detail in section 3.

### Task 5 — the sheet speaks for elements

The capture sheet gains an `elementHz` line, measured over the audio in the file
at the pitch the decoder was following. Wording proposed in section 4 for a
ruling. The arithmetic locks stay green.

### Task 6 — the shredded pair, characterised

Measure only, nothing changed, nothing promised. Detail in section 3.

## 2. What the owner should expect

**`cw-2026-08-31-003229` now shows 27 named characters and 29 blocks at 586.2 Hz,
where unit 055 left it at 57 named and 38 blocks.**

    ■■■■■■■ ■■EEE ■ ■■ E ■SI ■ ■ ■ ■E<HH> E H ■ ■D ■ ■ ■ NEQ TIT K■G ■ ■ XA ■ ■ ■ EE■ E

**Fewer characters, and it is still not `CQ`.** The lower ceiling makes the
decoder emit less on this capture while the corpus improves, and since `003229`
has no adjudicated truth its named count measures nothing on its own. **What is
new is that Hamlet's own offline path reads the callsign attempt off the same
audio** — `CXSIT#DD # SXEIT#S # KA` free-running, and a literal `CQ SIT K8DZ`
held at 23 words a minute. The reading exists inside the decoder and does not
survive the streaming window.

**`002829`'s two streams, side by side.** There are two senders and Hamlet will
not yet say so, so what follows is the measurement rather than two decodes:

| | lower | upper |
|---|---|---|
| centre | **604.7 Hz** | **613.7 Hz** |
| marks that could vote | 7 | 22 |
| when | one burst, **13.58 to 15.45 s** | everywhere else |
| median mark | 55 ms | 60 ms |

At a mixdown of 608.5 Hz the two heaps sit at 601.4 and 613.7, **12.3 Hz apart**,
which is the figure the order names. The combined decode is unchanged and still
reads badly: 53 named, 17 blocks.

**What will look wrong but is not:**

- **`003229` names fewer characters than last unit.** Deliberate consequence of
  the ceiling; the corpus improved on every one of the three numbers.
- **Task 3 ships machinery that splits nothing.** That is the finding, not an
  unfinished job, and there is a test holding it.
- **The shredded pair still reads badly.** Section 3 says why nothing will read
  them.
- **`CwStreamSplit.LeastSeparation` and its neighbours are live constants behind a
  verdict that is hard-coded false.** They are the surveyed figures, kept so the
  unit that proves a criterion has somewhere to put it.

**Version unchanged at 1.12.7.** The order parks the version bump and forbids
raising it; HM-DEC-150 says a session bumps the patch every unit. That conflict is
section 4's first ask.

**Build clean, no new warnings. Six commits, all pushed to `main`.**

| suite | result |
|---|---|
| `TheSilencePropertyIsLockedTests` | 6 passing |
| `TheCleanReadsStayCleanTests` | 7 passing |
| `TheAdjudicatedReadingsKeepReadingTests` | 13 passing |
| `AStationIsABinThatSwingsTests` | 10 passing |
| `TheSheetDoesNotLieAboutArithmeticTests` | 6 passing |
| `EveryElementCarriesItsOwnPitchTests` | 11 passing — new |
| `NoSenderIsSplitInTwoTests` | 6 passing — new |
| `WhereHamletAndTheReferenceDivergeTests` | 4 passing — new |
| `TheSheetSaysWhatEachElementWasSentAtTests` | 4 passing — new |
| `TheCapturesThatDecodeKeepDecodingTests` | **18 of 38 failing, against 15 of 38 before this unit** |
| corpus | **0.901 / 0.878 / 16** |

**THE ONE RED NUMBER THAT IS PARTLY MINE, MEASURED RATHER THAN ASSUMED.** The
character-and-element ratchet is not one of the five locks the order names, and it
was **already 15 of 38 red at `bb3551b`, before this unit touched anything** — its
floors were set 2026-08-25 and nothing has maintained them since. I checked out
that commit into a detached worktree and ran the suite there rather than reasoning
about it.

**Three captures newly fall below their floors, and the ceiling change is the
cause:**

| capture | characters | elements | unsure |
|---|---|---|---|
| `003016` | 54 against a floor of 57 | 146 against 149 | 3 → **1** |
| `001831` | 53 against 55 | 124 against 124 | 10 → 18 |
| `021410` | 40 against 47 | 97 against 99 | 11 → **6** |

**None was fixed and none of the fifteen already failing was recovered.** Two of
the three mark *fewer* characters unsure than when their floors were set, which is
the decoder becoming more certain rather than less, and the suite's own remarks say
the unsure count is deliberately not asserted in either direction. **This is
section 4's second ask**: Tim's ruling bought 1.2 points of corpus precision and
cost three floors on a guard that was already two-fifths red, and whether that
trade stands is his.

**A flake, named so it is not mistaken for a regression.**
`CwToDataAndBackTests.ArrivingOnTheDigitalBlockLeavesTheRadioAbleToHearIt` failed
once inside the whole-solution run under heavy parallel load, and **passes at both
`bb3551b` and head when run on its own**. It is an async rig test with a scripted
radio; nothing in this unit touches that path.

**The whole-solution run was not carried to completion.** It ran an hour and three
quarters without finishing and was stopped; what replaced it is every suite named
above plus the ratchet comparison at both commits. **The suites not run this unit
are the analysis tables and the sensitivity sweeps**, which write records rather
than assert behaviour.

## 3. What you should see

**The question this unit was commissioned to ask was why Hamlet reads letters
where the reference reads `CQ … K`. The answer is that it does not. The two
element streams are the same stream, and the reading is lost after the elements,
inside Hamlet's own streaming window.**

**Hamlet's stream against the reference's, over the seconds the reference reads
`CQ` — 13.50 to 14.72 on `cw-2026-08-31-003229`, both at 583.5 Hz:**

```
      reference              Hamlet            apart
      MARK   150 ms          MARK   155 ms       5 ms
      gap     30 ms          gap     30 ms       0 ms
      MARK    65 ms          MARK    65 ms       0 ms
      gap     36 ms          gap     35 ms       1 ms
      MARK   161 ms          MARK   160 ms       1 ms
      gap     24 ms          gap     30 ms       6 ms
      MARK    68 ms          MARK    60 ms       8 ms
      gap    126 ms          gap    130 ms       4 ms
      MARK   155 ms          MARK   155 ms       0 ms
      gap     36 ms          gap     40 ms       4 ms
      MARK    98 ms          MARK    90 ms       8 ms
      gap     17 ms          gap     30 ms      13 ms
      MARK    68 ms          MARK    55 ms      13 ms
      gap     24 ms          gap     30 ms       6 ms
      MARK   157 ms          MARK   160 ms       3 ms
```

**Eight marks and seven gaps, in the same order, with the same alternation.** Nine
of the fifteen agree inside one hop and the worst disagrees by three. The absolute
times differ by a constant 48 ms, which is a centred 33 ms Hann against a centred
25 ms boxcar, and a constant offset cancels for a length.

**No marks split. No gaps missed. No marks invented.** The order's hypothesis —
that Hamlet's stream holds many short isolated marks — is false here.

**So none of the four named causes can be the difference**, because every one of
them corrupts the element stream and the element stream is not corrupt. Each was
measured anyway, in the order the work instruction gives them:

1. **Integrator width.** The reference's 25 ms boxcar is 40 Hz nominal against
   Hamlet's 45 Hz Hann. As the order guessed, unlikely, and now excluded.
2. **The threshold's placement on this capture.** The reference uses a fixed
   threshold at the 98th percentile less 6 dB over the whole file. Hamlet's own
   reads `CXSIT#DL … KA` at **every** pitch from 583.5 to 589 Hz, so its threshold
   is not putting the reading out of reach.
3. **The hold-over's reach after task 1.** Swept 12 to 40 ms. It moves the corpus
   and moves nothing on `003229`.
4. **The minimum-run drop-without-merge.** Real in the code, and it does not fire:
   **one run in 140** on `003229`, **none in 235** on `004507`, **none in 212** on
   `003758`. The hysteresis absorbs the notches first.
   `CwUnitEstimator.Elements` now counts them, because a silent filter cannot be
   diagnosed.

**What diverges is Hamlet from Hamlet.** Same core decoder, same audio, same
pitch:

| path | reads |
|---|---|
| offline, free-running | `CXSIT#DD # SXEIT#S # KA` |
| offline, held at 23 WPM | **`CQ SIT K8DZ# # DQ EITK#G # VA`** |
| **streaming, as shipped** | 27 named, 29 blocks, no `CQ` |

**The named residue, stated exactly:** the callsign attempt survives the offline
window and dissolves in the sliding one. The remaining difference is in the
streaming path — its twelve-second window, its per-window noise scale, its
settle-by-time, its held gap classes — and not in element extraction, thresholds,
bandwidth or the run filter. **That is a different investigation from the one this
order scoped, and it is where the next unit should start.**

**Task 6, the shredded pair: the fragments cluster at many pitches, not one, and
nothing will read these.** With per-element pitch available for the first time:

| | `003408` | `003419` | `003758` (control, reads at 1.000) |
|---|---|---|---|
| marks | 108 | 116 | 118 |
| mark lengths | smeared 20 to 240 ms, every bin filled | the same | **two sharp heaps**, 61 marks at 40–60 ms and 41 at 140–160 |
| pitch spread, p10 to p90 | **31.5 Hz** | **30.3 Hz** | **0.8 Hz** |
| pitch gatherings | 580–585 (11), 600–605 (39), 610–620 (41) | 580–585 (11), 600–605 (25), 615–620 (37) | one |

**Forty times the pitch spread of a capture Hamlet reads perfectly, and at least
three gatherings across 35 Hz inside a 45 Hz detector.** That is several stations
colliding rather than one station torn apart, and it is why the mark lengths have
no dit-and-dah structure to recover: the marks belong to different senders. **The
honest answer is that nothing reads this**, and no change to a threshold, a
bandwidth or a hold-over will change that.

**Nothing else in the application looks different.** The capture sheet gains one
line; the terminal, the panels and the decode on screen are as they were.

## 4. What's blocking us

> **The version bump is ruled or the order's park is upheld, and today they
> disagree.** `Directory.Build.props` says 1.12.7. HM-DEC-150 says the minor is
> the phase and the patch is the work unit, so a session reads the version, bumps
> the patch, and reports what it moved from and to — which makes this unit 1.12.8.
> Work instruction 056's parked list says the version bump is "still unruled, do
> not guess", and says not to raise it either.
>
> **The order is ten days newer than the ruling and more specific to this unit, so
> it was followed and the version was not touched.** Rejected: bumping on
> HM-DEC-150's authority, because the order forbids guessing here and a version
> is cheap to move and expensive to move wrongly. Rejected: staying silent, because
> §0 of `CLAUDE_CODE.md` requires a session that follows one document over another
> to name the conflict rather than leave the drift in place.

> **The capture sheet's element line reads as proposed below, or Tim rewords it.**
> §12.1: this is the sheet's voice and the sheet is evidence, so the wording is
> his.
>
> Measured, with elements present:
>
>     elementHz  29 elements measured, gathering at 604.7 and 613.7 Hz, 9.0 Hz
>                apart with 2.15 Hz of scatter inside them  (measured over this
>                recording; whether that is one operator or two is not something
>                Hamlet can yet tell you)
>
> Too few long elements:
>
>     elementHz  3 elements were long enough to measure a pitch from, which is too
>                few to say anything about how they spread
>
> No pitch measured at all:
>
>     elementHz  not measured  (no pitch was measured, so there is nothing for an
>                element's own pitch to be measured against)
>
> **The closing clause is the load-bearing part and it is deliberately flat.** It
> says Hamlet cannot yet tell one operator from two, which is true and is the
> whole of task 3's finding. Rejected: naming the two gatherings as senders, which
> is the untested criterion. Rejected: dropping the clause and printing the two
> figures bare, because two numbers labelled "gathering at" invite exactly the
> conclusion the measurement cannot support. **The per-stream lines task 5 asked
> for are not proposed at all**, because there are no streams to speak for.

> **The ratchet's three lost floors are accepted for what the ceiling bought, or
> the ceiling goes back.** `TheCapturesThatDecodeKeepDecodingTests` goes 15 of 38
> red to 18 of 38 on `FastestWpm` alone. It is not one of the five locks this order
> names, all five of which are green, and it was already two-fifths red before this
> unit began.
>
> **The ruling to lower the ceiling is yours and explicit, so it was executed and
> the cost is reported rather than used as grounds to revert.** Rejected: reverting
> the ceiling, because your ruling in this order rejects keeping 40 on measured
> grounds and a session does not overturn that on a guard the order does not name.
> Rejected: raising the three floors to what the decoder now produces, because
> §12.5 forbids lowering a floor to fit a change and that is what it would be.
> Rejected: staying quiet because the named locks are green, which is how a guard
> goes from fifteen red to eighteen without anybody deciding it should.
>
> **The suite itself may be the real ask.** Floors set on one day over unadjudicated
> audio, two-fifths of them broken, and no unit since has reported them. Either they
> are re-measured with the decoder as it now stands, or the suite is retired and its
> job given to the adjudicated anchors, which is where the guarding has actually been
> happening.

> **Whether the next unit works the streaming window.** Task 4's residue puts the
> whole remaining difference on `003229` inside the streaming path, and that path
> has never been measured against its own offline path on the corpus. The obvious
> first measurement is to score every capture both ways and see how wide the gap
> is — if the offline path reads the corpus materially better, that is the phase
> goal's shortest route and it is a bigger change than a threshold.
>
> Rejected: doing it inside this unit, because the order scoped task 4 to the
> element comparison and named causes, and a session that finds a bigger fish and
> chases it has left the plan.

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140.

1. **The ratchet's three lost floors** — raised above, 2026-08-31. **New.**
   `TheCapturesThatDecodeKeepDecodingTests`, 15 of 38 red to 18 of 38.
2. **The version bump against HM-DEC-150** — raised above, 2026-08-31. **New.**
   `Directory.Build.props` says 1.12.7.
3. **The capture sheet's element wording** — raised above, 2026-08-31. **New.**
   The line is in `MainWindowViewModel.ElementPitchLine` and shipping.
4. **The streaming window against the offline path** — raised above, 2026-08-31.
   **New.** Nothing changed for it.
5. **Hysteresis on the peak** — 2026-08-30, unit 053. Costed, not built, and this
   order forbade building it.
6. **The squelch and `013347`'s blocks** — 2026-08-30, unit 053. Materially
   improved in unit 055, 84% blocks to 36%.
7. **The `134712` carrier** — 2026-08-30, unit 052.
8. **The guard narrowing** — 2026-08-29, unit 051.
9. **The filter byte against HM-DEC-149** — HM-OPEN-062, unruled.
10. **The evidence term's unbounded scale** — unit 049.
11. **The answer key's licensing.**
12. **The mode and filter's place in the owned-settings contract** — unit 047.
13. **What the digital rows state for the five settings they are silent on.**
14. **The pedestal ranking**, measured at 34 of 44 and unbuilt.
15. **A dial move's threshold**, provisional at 500 Hz.
16. **The transcript break's wording.**

**Dropped from the queue this unit:** the 0.005 floor breach (2026-08-31, unit
055) and `FastestWpm` with the hold-over bound (2026-08-30, unit 054). Both were
ruled in this order and both are recorded in section 1.

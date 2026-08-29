UNIT:       044 — stopped at task 7 of 7 — 2026-08-28 23:36
PHASE GOAL: Readable CW on the operator's screen — eighty percent of a strong signal read correctly, first time.
UNIT GOAL:  Stop the decoder printing letters while it does not know the sender's speed, and find out why the confidence figure rose as the output got worse.
ADVANCED:   no — the goal task is task 2 and its own bar says ship nothing; the measurement it rests on is the unit's real product.
NUMBER:     task 2's cost — unit 036's bar was 2, 2 and 7 blocks; this refusal costs 356 of the 599 characters carrying the twelve adjudicated readings.
DRIFT:      4 consecutive units without advance  (was 3)

## 1. What Claude did

**Stopped at task 7 of 7. Tasks 4, 5 and 6 were not done and task 1 is half
done, all for the same reason: the eight 2026-08-29 captures are not in the
repository.** Task 2 was measured and **ships nothing, on the bar the order
itself sets.** Tasks 3 and 7 ran on the corpus that does exist.

**This is not the drop the order named** — task 7 was the drop candidate and it
is one of the three that landed.

Development computer, prompt claimed `PROJECT: Hamlet`, branch `main`, version
`1.12.6` unchanged. **Nothing here is evidence about the radio**: no radio was
connected.

### The blocker, unchanged from last unit

**None of the eight 2026-08-29 captures is in the tree.** Neither the three this
order is built on nor the five the last one was:

| | |
|---|---|
| `cw-2026-08-29-030850`, `-030940`, `-031024` | missing |
| `cw-2026-08-29-020541`, `-020616`, `-020707`, `-020809`, `-020938` | missing |

The newest audio remains `cw-2026-08-28-005243`. **So none of this order's stated
figures could be confirmed** — not 141 characters with 1 unsure, not 8224.4
against 11.1, not the station at 398.4 Hz, not 850 Hz holding nothing. They are
neither confirmed nor disputed here.

**What that costs**: task 1's reproduction half; task 4 entirely; task 5 entirely,
including the bulletin ground truth; task 6 entirely; and task 2's acceptance,
which is stated only on `-030940` and `-031024`.

### Task 1 — the emit seam, and unit 043's answer

**Both suites:** app **519 passing, 0 failing**. Engine **29 failing of 1976**,
excluding the five tests of `TheGateHasItsOwnWindowNowTests` which crash the host
(HM-OPEN-061). The order cites unit 041 at 28 of 1916 and 509 of 509; the totals
have grown with tests added since.

**The twenty-ninth is a second intermittent and it is not this unit's.**
`Rig.RigDisconnectTests.ARigWhoseReadLoopIsStuckStillDisconnects` failed in the
whole-suite run and **passes on its own**. `git diff` over `src/` between unit
043's last commit and this one is **empty** — this unit changed no product code at
all — so it cannot be a regression from here. The order names
`AConfirmedModeWriteFoldsTheDataVariantTooAsync` as a known intermittent; **this is
a second one**, and both are timing-sensitive tests about a stuck or slow link.

**The emit seam has exactly one refusal, and it is not either of the ones the
order asks about.**

| refusal | where | what it does |
|---|---|---|
| the character floor | `CwProbabilisticDecoder.Marked`, `:1513` | a character whose `SpanMargin` is below `CharacterMargin` = 1.0 becomes `#`, which renders as a block |

**Unit 036's refusal is absent.** Nothing anywhere keys emission to whether the
survey admitted keying. **Unit 043's refusal is absent too** — its task 2 was
blocked on this same missing audio, which is what my report of that unit says.
The order anticipated this and said to build beside where it would go rather than
on top of it; there is nothing there to build on top of.

**Where the assertion is actually made is `CwProbabilisticStream.Character`,
`:730`.** Every character whose pattern the alphabet knows is stamped
`CwConfidence.High`, unconditionally. **The only thing that can mark a character
unsure is the alphabet not recognising its pattern.** That is the mechanism behind
the order's "141 characters with 1 unsure": it is not a confidence judgement that
went wrong, it is that no confidence judgement is made at all.

**Why the clock is withdrawn**, with file and line, since this is what task 2 had
to wire to: `CwDecoder.SpeedIsReacquiring`, `:638`.

    _hasFollowed
        ? _lastSample - _samplesAtDiscontinuity < 12 seconds of samples
        : _probabilistic.Last.Text.Length == 0

**And this is the finding that reframes the unit.** `SpeedIsReacquiring` does not
mean *no clock fits*. It means *the tracker changed station less than twelve
seconds ago*, and the field's own documentation says so: the decoder reads a
window several seconds long, so while that window still holds audio from the
previous station it would name a speed between the two, describing neither.

**So `decoderWpm withdrawn` is not Hamlet saying it cannot tell a dit from a dah.
It is Hamlet saying its window may straddle two stations.** The order's reading of
that field — the premise task 2 is built on — does not match what the field
computes.

### Task 2 — no clock, no letters: measured, and it ships nothing

Wired as instructed to the decoder's own existing withdrawal condition rather
than a second test, and measured across all 44 captures before changing a line.

| | |
|---|---|
| characters emitted across the corpus | **1912** |
| of those, emitted while the clock is withdrawn | **1125 (59 %)** |
| captures emitting anything at all | 39 of 44 |
| captures where *some* output is withdrawn | **36 of 39** |
| captures where **every** character is withdrawn | 3 |

**And what it costs the twelve adjudicated readings, which is the bar:**

| capture | blocked | of | share |
|---|---|---|---|
| `cw-2026-08-17-134712` (`N4L`) | **63** | 63 | **100 %** |
| `cw-2026-08-22-031905` (`DICTED 10.7`) | **42** | 42 | **100 %** |
| `cw-2026-08-22-032113` | 46 | 55 | 84 % |
| `cw-2026-08-22-031838` | 40 | 57 | 70 % |
| `cw-2026-08-22-032050` | 37 | 53 | 70 % |
| `cw-2026-08-22-032129` | 41 | 66 | 62 % |
| `cw-2026-08-18-004507` (the ARRL bulletin) | 21 | 49 | 43 % |
| `cw-2026-08-18-003758` (`AA4MP/4 QNIK`) | 22 | 58 | 38 % |
| `cw-2026-08-17-013347` (`VA3VRR`) | 21 | 59 | 36 % |
| `cw-2026-08-22-031948` | 11 | 31 | 35 % |
| `cw-2026-08-22-032012` | 12 | 44 | 27 % |
| `cw-2026-08-24-012403` (`KD0UN`) | 0 | 22 | 0 % |
| **total** | **356** | **599** | **59 %** |

**Unit 036's bar was 2, 2 and 7 blocks on the good captures it named. This is 356,
and two adjudicated readings disappear completely.** The order's own instruction
is explicit: *if the cost across the corpus is materially larger than unit 036's,
stop and report rather than shipping. That is the same bar Tim applied last time
and it applies here.* **So nothing was shipped.**

**It is the same trade Tim rejected in unit 1.11.33, an order of magnitude worse.**
That refusal cost 89 characters across three good captures; this one costs 356
across eleven.

**And the reason is task 1's finding rather than bad luck.** The condition is not
*no clock*, it is *the tracker moved recently*, and on a thirty-second capture
with a few tracker switches that is most of the recording. A letters-refusal wired
to it blocks most of the corpus for a reason that has nothing to do with telling a
dit from a dah.

### Task 3 — what the fit figure computes

**It is not a goodness-of-fit measure. It is closer to a signal-to-noise ratio,
and it is unbounded.**

`CwProbabilisticDecoder.Decode`, `:812`:

    ratio = (bestScore - nothingAtAll) / envelope.Count

`bestScore` is the winning Viterbi path's total log-likelihood; `nothingAtAll` is
the sum of the key-up log-likelihood over every hop. So the figure is the mean
per-hop advantage of the best reading over "the key was up the whole time".

**The term that grows is the noise scale, not the reading.** From
`LogLikelihoods`, `:973`:

    keyUp[i]   = log(e) - 2*log(sigma) - e*e / (2*sigma*sigma)
    keyDown[i] = -HalfLogTwoPi - log(sigma) - (e-amplitude)^2 / (2*sigma*sigma)

On a hop where the envelope is loud, the null hypothesis must explain it as noise
and pays `-e²/2σ²`, which grows as **the square of the level over the noise
scale**. The best path avoids that wherever it calls the hop a mark. So the
difference between the two — which is the whole figure — **scales as
(amplitude/σ)², without a bound.** A loud signal in a bin whose estimated noise
floor is small produces an enormous number whether or not a single letter is
right.

**Measured across the corpus, on the 39 captures that emit anything:**

| | |
|---|---|
| fit figure, range | **−18.12 to 121.88** |
| median | 3.74 |
| correlation with the share of output that is `E`, `I`, `S` or `T` | **+0.228** |

**So the metric does lean the wrong way, and weakly.** The correlation is positive
— fragmentation and a high figure do travel together — but at +0.23 it is not
strong enough to be the explanation on its own. **The unbounded (amplitude/σ)²
term is the explanation**, and fragmentation is a fellow symptom rather than the
cause: both happen when the gate is chopping a signal that is loud relative to a
small estimated noise floor.

**A figure that can be −18 is worth as much attention as one that can be 8224.**
"Minus eighteen better than silence per hop" is on a sheet the operator reads, and
it means the best reading the decoder could find explained the audio worse than
assuming nothing was sent — which is a state that should produce no output at all.

**Is it the same root as `013347`'s 17.2 million and `001520`'s quadrillions?
Yes.** Same expression, same unbounded term. Unit 043 measured the same mechanism
from the other side: ranking pitches by this figure picked the emptiest bin in the
band, at 5,521,967, because when σ collapses the ratio explodes. **One defect,
four sightings.**

Changed nothing, as instructed.

### Task 7 — the withdrawal and the search edges

**How often the clock withdraws: on 36 of the 39 captures that emit anything, and
for 59 % of all characters.** A condition that is true most of the time is not
identifying an unusual state.

**Estimators at the edge of their own search space: two**, both at the top —
`cw-2026-08-22-032113` and `cw-2026-08-22-032129`, each settling at exactly 40
words a minute, which is `FastestWpm`. Both emit 55 and 66 characters. **An
estimator at its boundary is reporting failure rather than a value**, and both of
these are adjudicated anchors whose readings are being taken at a pinned number.

The order's claims about `-020809` pinning at 40 and `-030850`'s sweep pinning at
400 could not be checked; those files are not here.

No decision was recorded under §12.1.

## 2. What the owner should expect

**Nothing on the screen has changed.** When Hamlet does not know how fast the
sender is going it still shows letters, because the refusal that would stop it
costs 356 characters across eleven of your twelve adjudicated readings — including
every character of `N4L` and every character of `DICTED 10.7`. **The order's own
bar says stop and report at that cost, so nothing shipped.**

What is now true of the tree:

- `tools/Hamlet.PitchRank` gained `clock`, which produces the whole table above in
  one command, so every figure here is one line from being re-measured.
- No engine or app source changed. The suites are where they were.

**What will look wrong but is not:**

- **The engine suite read 29 rather than 28**, and the extra is
  `ARigWhoseReadLoopIsStuckStillDisconnects`, which passes on its own. No product
  code changed in this unit, so nothing here moved it.
- **Task 2 built nothing.** That is the instructed outcome at this cost, not an
  omission.
- **The full engine suite has no single clean run**, and the crash is wider than
  HM-OPEN-061 first recorded — a run excluding the class it names crashed anyway
  after 544 passing tests, which was logged against that issue last unit.
- **This order says it follows unit 043 and has not seen its report.** Unit 043 ran
  and is at `389cbe1`; its task 2 was blocked on this same missing audio, which is
  why the refusal this order expected to find is absent.

## 3. What you should see

**The confidence figure is not measuring how well the decoder is doing. It is
measuring how loud the signal is compared with the noise floor it estimated, and
nothing bounds it.**

`ratio = (bestScore − nothingAtAll) / hops`, and both halves carry a
`−e²/2σ²` term. The all-noise hypothesis has to explain every loud hop as noise
and pays that on each one; the best path avoids it wherever it calls the hop a
mark. **What is left over grows as the square of the level over the noise scale.**
A loud signal in a bin with a small estimated floor produces a huge number whether
the letters are right or wrong.

Across the corpus the figure runs from **−18.12 to 121.88** on real captures, and
its correlation with fragmented output is **+0.228** — leaning the wrong way, but
too weakly to be the story. **The story is that the quantity has no ceiling and no
floor.** That is why it read 8224 on garbage, and it is the same expression behind
`013347`'s 17.2 million, `001520`'s quadrillions, and the 5,521,967 unit 043
measured on an empty bin. **One defect, four sightings, and every number on the
sheet that derives from it inherits it.**

The second thing worth having is that **`decoderWpm withdrawn` does not mean what
the order reads it as meaning.** It means the tracker changed station within the
last twelve seconds — the field's own documentation says so. It is true for 59 %
of everything Hamlet emits. Building a letters-refusal on it blocks most of the
corpus for a reason unrelated to telling a dit from a dah, which is why the cost
came out where it did.

## 4. What's blocking us

**The audio, again, and it is now blocking two consecutive units.**

> **Tasks 1, 4, 5 and 6 need the eight 2026-08-29 captures in
> `tests/fixtures/cw/captured/unadjudicated/`**, with their sidecars. The three of
> 03:08–03:10 are the best test material this project has described — one
> frequency, one station, one variable, and a published bulletin as an answer key.
> None of it is here.

Then two rulings.

> **The fit figure is normalised so that it cannot grow without bound, and the
> sheets that quote it are re-read afterwards.**
>
> It is `(bestScore − nothingAtAll)/hops`, and both terms carry `−e²/2σ²`, so it
> scales as the square of the level over the estimated noise scale. Measured on
> real captures it runs −18.12 to 121.88; on degenerate bins it has produced
> 5,521,967, 17.2 million and quadrillions. **Every confidence number the operator
> reads derives from it**, so §0.0 rests on a quantity that is not bounded and not
> comparable between two recordings.
>
> **Rejected: treating the extreme values as outliers.** Four sightings across
> four units with one expression behind them is a defect, not a tail.
> **Rejected: fixing it in this unit.** The order says measure only and change
> nothing, and it is right — this needs its own unit with the whole corpus
> re-measured after, because every floor and every anchor is expressed in these
> units.
> **What it is not yet:** a proposal. Which normalisation is correct is a real
> question and this unit was not asked to answer it.

> **Whether a letters-refusal should be built on a different condition, since the
> one that exists is not about the clock.**
>
> `SpeedIsReacquiring` means the tracker moved within twelve seconds, not that no
> clock fits, and a refusal wired to it costs 356 of 599 adjudicated characters.
> **The fault the order describes is real** — a decoder that cannot resolve a dit
> from a dah should not print letters — **but this is not the flag for it.**
>
> **Rejected: shipping it anyway.** The order's own bar forbids it at this cost.
> **Rejected: exempting the anchors.** That is fitting the refusal to the test set.
> **What would be needed** is a condition that actually says no clock fits. The
> reference decoder ported in unit 045 has one — `FitClock` returns nothing when
> the marks do not form two lengths, and it refuses eleven of forty-four captures
> outright with no threshold anybody chose. **That is the closest thing in this
> tree to what task 2 was asking for**, and unit 045's report already asks whether
> to graft it onto the shipped path.

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140.

1. **The eight 2026-08-29 captures are not in the tree.** First raised in unit
   043's report; now blocking a second unit.
2. **The fit figure is unbounded** — raised here, and it subsumes the two open
   asks below it.
3. **`013347` returns 17.2 million and `001520` quadrillions.** Same root, answered
   here: yes.
4. **The pedestal ranking is measured at 34 of 44 and unbuilt** — unit 045's order
   made it its own unit if the reference lost, and it lost.
5. **A dial move's threshold is provisional at 500 Hz**, shipped in unit 043 with
   three candidates costed and awaiting a ruling.
6. **The transcript break's wording** — proposed in unit 043's report, unruled.
7. **The attenuator's condition on a live overflow reading**, and whether `CwPitch`
   should follow an admitted station — both raised in unit 043's report.
8. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
9. **The `reading` line's span wording needs approval.**
10. **Two stations closer than 125 Hz are not named.**
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **Nothing checks that deleting a surface is not deleting a capability** — the
    favourites list is gone.
13. **A capture sheet carries a score of −68562.4** (`cw-2026-08-28-005158`) —
    **and this unit explains it**: the fit figure has no floor either.
14. **The engine test host crashes**, and not only on the class HM-OPEN-061 names.
    Owned by Claude, not waiting on a ruling.
15. **A second intermittent** —
    `Rig.RigDisconnectTests.ARigWhoseReadLoopIsStuckStillDisconnects` fails in a
    whole-suite run and passes alone, first seen here. Like the one the orders
    already name, it is a timing-sensitive test about a slow link.

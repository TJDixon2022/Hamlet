UNIT:       054 — the threshold reference, and the hold-over — 2026-08-30
PHASE GOAL: 85% correct CW on a capture where the pitch is right, precision before yield.
UNIT GOAL:  Ship two changes to the detector: reference the threshold to the loudest signal, and bridge a dropout inside a key-down.
ADVANCED:   **yes, on one of the two.** Precision **0.888 → 0.894**, yield 0.745 → 0.750, substitutions 17 → 15. The threshold change was measured and refused.
NUMBER:     **precision 0.894, yield 0.750, substitutions 15** over 384 adjudicated characters.
DRIFT:      0 consecutive units without advance.

## 1. What Claude did

**Hamlet confirmed.** All four gate checks verified against the tree before the
order was read. Branch `main`, every task committed and pushed, every push
succeeded. **Nothing here is evidence about the radio.**

**Five of six tasks. Task 6 is dropped and it is the named drop candidate.** One
of the two ordered changes ships; the other was built, swept and refused.

**The nine captures of 2026-08-30 are not in the tree.** Said once, as the order
directs. Everything below is measured on the corpus that exists.

### Task 1 — what unit 053 landed, and what already bridges a dropout

**Unit 053 landed the bisect, the clean-read lock, the two-signal measurement and
the fading measurement.** The lock exists as `TheCleanReadsStayCleanTests` and
**governs this unit**, so it was not rebuilt. Corpus at head: precision 0.888,
yield 0.745.

**Two things already bridge a dropout, both in `CwUnitEstimator.Runs` at
`CwUnitEstimator.cs:543`** — so task 3 changed what is there rather than adding a
third:

| mechanism | parameter |
|---|---|
| Schmitt trigger | **±6 dB** about the cut |
| shortest recorded run | **2 hops = 10 ms** |
| detector | 5 ms hop, **45 Hz** integrator |

**A mismatch to report: the threshold is referenced to an Otsu two-class split,
not to a span between percentiles.** The span reference the order describes as
present was measured and refused in unit 051 and is not in the tree.

**Measured, the key number for task 3:** a dropout to the noise floor of up to
**20 ms is absorbed** and the mark stays whole at 220 ms; **at 30 ms it comes
apart.** Unit 053 measured the real dropouts at **32–53 ms**, so the fading sits
just past where the existing bridging gives out.

**One correction, recorded rather than deleted.** My first version of that test
asserted that a one-hop notch splits a mark, reasoning that `Runs` drops a short
run without merging the two it separated — **which it does do, in the code.**
Measured, a one-hop notch never reaches that line: the hysteresis absorbs it
first. The drop-without-merge is real and, at one hop, unreachable.

### Task 2 — the peak-referenced threshold, refused on three grounds

1. **A setback under 6 dB cannot work at all, and that is arithmetic.** The
   trigger opens at the cut **plus** `HysteresisDb`, which is 6 dB — so a setback
   of 5 puts the opening threshold a decibel **above** the envelope's own 98th
   percentile and nothing is ever key-down. **Setbacks of 3, 4 and 5 produce fewer
   than eight marks on every capture**, so **the order's recommended peak − 5 dB is
   unreachable while the hysteresis is ±6.**
2. **The usable range is not monotonic.** Dah CV on `013347`: 0.444 at Otsu,
   **0.113 at −6**, 0.413 at −8, 0.531 at −10, 0.486 at −12.
3. **The one candidate that helps costs precision.** At −6 dB: **0.840** against a
   floor of 0.888, yield 0.630, substitutions 33.

**The finding the order predicted is real, which is why the code is kept with its
numbers.** At −6 dB the worst captures improve exactly as described — `013347`
0.444 → 0.113, `134712` 0.647 → 0.275 — **while the captures that already read
cleanly get worse**, `004507` going 0.015 → 0.113. **Referencing to the peak buys
the hard captures and sells the easy ones**, which is the trade this project has
been making by accident and must not now make on purpose.

**A correction to unit 051's report.** Its fraction sweep computed precision over
`ScoredCharacters`, which counts blocks; the corpus figure divides by correct plus
substitutions plus insertions. **Those sweep numbers were never comparable with
the 0.888 they were compared against.** That refusal rested on three grounds and
the other two are unaffected, so the decision stands — but one leg of it was
measured wrongly and I found it by hitting the same bug here.

### Task 3 — the hold-over, and it ships

**A key-down that comes back inside 12 ms never ended.**

| hold-over | yield | precision | subs |
|---|---|---|---|
| 0 ms | 0.745 | 0.888 | 17 |
| 8 ms | 0.745 | 0.888 | 17 |
| **12 ms** | **0.750** | **0.894** | **15** |
| 16 ms | 0.742 | 0.905 | 18 |
| 24 ms | 0.734 | 0.898 | 23 |

**0 through 16 is non-decreasing, so 16 was the first candidate — and it broke an
anchor.** `cw-2026-08-22-031838` lost its adjudicated run `, AND`, and §12.5 does
not let a floor be lowered to fit a change. **12 ms holds every floor in the
suite, and it is the better point on two of the three numbers anyway**: best yield
and fewest substitutions in the whole sweep. The 1.1 points of precision given up
against 16 buy an anchor that stays.

**The bound is asserted, not remembered.** The hold-over must be shorter than a dit
at `FastestWpm`, which is **30 ms at 40 words a minute**, or it welds two elements
together across a real gap. **It applies only inside a key-down already admitted**,
so a dip while the key is up is never extended into a mark (§0.0, HM-DEC-120).

**It does not reach the fading and does not claim to.** Dropouts are 32–53 ms and
the safe bound is 30. What it buys is the stretch between the 20 ms the hysteresis
already absorbs and about 25.

**Dit CV barely moved and the decode improved anyway.** Across the whole sweep it
changes by hundredths and not always downward — `134712` runs 0.432, 0.432, 0.432,
0.428, 0.441, 0.462. **The scatter was a poor proxy for the reading**, and the
goal is stated in the reading. Task 3's stated acceptance was "dit CV falls"; **it
did not, and the change is adopted on precision instead** — that substitution of
criterion is flagged rather than glossed.

### Task 4 — the two together

| threshold | hold-over | yield | precision | subs |
|---|---|---|---|---|
| Otsu | 0 ms | 0.745 | 0.888 | 17 |
| **Otsu** | **12 ms** | **0.750** | **0.894** | **15** |
| peak − 6 dB | 0 ms | 0.630 | 0.840 | 33 |
| peak − 6 dB | 12 ms | 0.630 | 0.840 | 33 |

**They interact in exactly the way the order names.** The hold-over is worth six
points of precision with Otsu and **worth nothing at all** with the peak
reference, because that cut leaves so few marks that the dips no longer matter.
**So the better single change ships and the pair does not.**

### Task 5 — the instrument did not work, and its number must not be used

It returns **four neighbours on all twelve captures**, 30–100 Hz from the admitted
pitch and 9–24 dB below it. **That is what the first station's own spectral
structure looks like.** The floor reference is the median of the whole spectrum,
and on a band holding one strong station that median is noise, so every ripple and
skirt clears it.

**A second station is one that is keyed independently, and nothing here tests
that.** Reporting "12 of 12 captures have a second signal" would be false and
consequential — it is the opposite of what unit 053 established. **Until somebody
compares the neighbour's envelope against the first's, unit 053's finding stands:
every capture in this corpus has one dominant station**, which that unit
established by deliberately summing two captures to get a second.

### Task 6 — **dropped whole**

The named drop candidate. It is a written note about what the corpus structurally
cannot test, and **it wants task 5's count, which is the one measurement this unit
could not obtain.** Writing it without that would be composing the note from unit
052's presence table alone, which is already in that report.

**No decision was recorded under §12.1.**

## 2. What Tim should expect

**Precision 0.888 → 0.894. Yield 0.745 → 0.750. Substitutions 17 → 15.** All three
moved the right way, which has not happened in a while.

**What changed on the air:** a station that fades briefly mid-element is now more
likely to be read as one element rather than two. It is a small change — 12 ms —
and it is bounded so it can never weld two real elements together.

**What did not change:** the threshold. The peak reference was built as ordered,
swept, and refused; the code is in the tree with its numbers so nobody rebuilds
it.

**What will look wrong but is not:**

- **`CwUnitEstimator.PeakSetbackDb` exists and defaults to NaN**, which keeps
  Otsu. Measured and refused, kept with its numbers.
- **Dit CV did not fall**, which was task 3's stated acceptance. The decode
  improved regardless and the change is adopted on that.
- **Task 5 reports no count.** Its instrument does not separate a second station
  from the first's skirts.

**Build clean, no new warnings.** Version unchanged at 1.12.7 — still unruled and
I have not guessed again.

| suite | result |
|---|---|
| `TheSilencePropertyIsLockedTests` | **6 passing, 0 failing** — green, unmodified |
| `TheCleanReadsStayCleanTests` | **7 passing** — all three floors held |
| `TheAdjudicatedReadingsKeepReadingTests` | **13 passing** — including `031838`, which 16 ms broke |
| `TheShortRunFilterDropsWithoutMergingTests` | 2 passing — new this unit |
| corpus | **0.894 / 0.750 / 15** |

## 3. What we should do next

1. **The hold-over cannot reach the fading and the bound is why.** Dropouts are
   32–53 ms; the safe bound is 30 ms because `FastestWpm` is 40. **If the decoder's
   fastest speed were lowered to 30 WPM the bound becomes 40 ms** and most of the
   dropout range comes into reach. That is a ruling, not a measurement, and it is
   the single cheapest thing that would let this change do more.
2. **Build the second-station test task 5 needed** — a neighbour's envelope
   correlated against the first's. Everything about the operator's band that the
   corpus cannot represent turns on it.
3. **Then reconsider the threshold reference per capture** rather than globally.
   It helps the hard captures and hurts the easy ones; nothing was tried that
   applies it only where the easy ones are not.

## 4. What's blocking us

Nothing is blocked. **One ruling would materially help the change just shipped.**

> **Lower `CwProbabilisticDecoder.FastestWpm` from 40 to 30, which raises the safe
> hold-over bound from 30 ms to 40 ms.**
>
> The hold-over must be shorter than one dit at the fastest speed the decoder will
> consider, or it bridges a real inter-element gap and welds two elements into
> one. At 40 words a minute a dit is 30 ms. **The dropouts measured on this corpus
> are 32 to 53 ms, so the bound sits just below the fault** and the change adopted
> this unit can only reach the bottom edge of it.
>
> **At 30 words a minute the bound becomes 40 ms**, which covers the lower half of
> the measured dropout range.
>
> **What it costs is the claim to decode above 30 WPM.** Nothing in this corpus is
> that fast, and HM-DEC-103 generated 25 WPM coverage deliberately because nothing
> had ever been decoded above twenty — but a ceiling is a promise about what the
> application will attempt, and lowering one is the operator's call.
>
> **Rejected: raising the hold-over past the bound.** It welds elements, which is
> a wrong character rather than a missing one.
> **Rejected: sizing the hold-over per-capture from the measured speed.** The
> speed estimate is downstream of the very element measurements this corrupts, so
> it would be circular.
> **What this session could not settle** is what the fastest speed actually
> reaching this decoder is on the air, because the captures that would say are not
> in the tree.

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140.

1. **`FastestWpm` and the hold-over bound** — raised above, 2026-08-30.
2. **Hysteresis on the peak** — 2026-08-30, unit 053. Costed there, not built.
3. **The squelch and `013347`'s 84% blocks** — 2026-08-30, unit 053. Separate from
   the above.
4. **The `134712` carrier** — 2026-08-30, unit 052.
5. **The guard narrowing** — 2026-08-29, unit 051.
6. **The version bump** — 2026-08-29. `Directory.Build.props` still says 1.12.7.
7. **The filter byte against HM-DEC-149** — **HM-OPEN-062**, unruled.
8. **The evidence term's unbounded scale** (unit 049).
9. **The answer key's licensing.**
10. **The mode and filter's place in the owned-settings contract** — unit 047.
11. **What the digital rows state for the five settings they are silent on.**
12. **The pedestal ranking is measured at 34 of 44 and unbuilt.**
13. **A dial move's threshold is provisional at 500 Hz.**
14. **The transcript break's wording.**
15. **Whether `CwPitch` should follow an admitted station.**
16. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
17. **The `reading` line's span wording needs approval.**
18. **Two stations closer than 125 Hz are not named.**
19. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
20. **Nothing checks that deleting a surface is not deleting a capability.**
21. **The test host crashes** in both suites — **HM-OPEN-063**.
22. **`PROJECT_CARD.md` has no phase field.**

UNIT:       048 — stopped at task 4 of 8 — 2026-08-29 12:10
PHASE GOAL: Readable CW on the operator's screen — precision before yield: never a wrong character on screen, and as much of the traffic as that allows.
UNIT GOAL:  Rebuild the lattice so a real posterior can be computed, and move the decoder's confidence onto it.
ADVANCED:   yes, partly — the lattice is rebuilt and precision rose 0.761 to 0.766 with substitutions 61 to 58; the posterior it made possible is not discriminative and no gate moved onto it.
NUMBER:     precision 0.761 -> 0.766, yield 0.763 -> 0.768, substitutions 61 -> 58.
DRIFT:      0 consecutive units without advance  (was 2)

## 1. What Claude did

**Stopped at task 4 of 8.** Tasks 1, 2 and 3 landed. **Task 4 was measured and
ships nothing.** Tasks 5, 6, 7 and 8 were not started.

**Why stopping here rather than continuing:** tasks 5, 6 and 7 each require
"precision must not fall", measured against the gate task 4 would have installed.
**No gate was installed**, because the posterior does not separate right
characters from wrong ones well enough to gate on. Continuing would be measuring
three large changes against the metric this unit set out to replace, which is the
position units 044 to 046 were already stuck in. **That is a decision the finding
forces, and it is Tim's to overturn.**

Development computer, prompt claimed `PROJECT: Hamlet`, branch `main`, version
`1.12.6` unchanged. **Nothing here is evidence about the radio.** The eight
captures of 2026-08-29 are still not in the tree — a sixth consecutive unit.

**And unit 047's owed regression landed during this one: 28 failing of 1963 run,
the failing set byte-identical to unit 046's, and then the host aborted.** So 047
introduced no regression, and its report's unreplaced amendment line was the
honest record of a run the HM-OPEN-061 crash ended.

### Task 1 — the baseline and the lattice

**App 519 passing, 0 failing. Corpus yield 0.763, precision 0.761** over 384
adjudicated characters — both confirmed.

**The lattice, as it actually was:**

- **State**: hop only. `best[i]` is the score of the best path whose last segment
  ends at `i`, with `kindAt[i]` and `wasDown[i]` recording that segment.
- **Transitions**: from `j = i − span`, scored
  `best[j] + evidence − ½(log(span/want)/LengthToleranceShare)²`, `:1462–1472`.
- **Five kinds** (`:525`): dit and dah key-down at 1 and 3 units, and gaps at 1, 3
  and 7. `ShortestShare` 0.45, `LongestShare` 2.2, `LengthToleranceShare` 0.35.
- **`second[]`** kept the runner-up per hop (`:1475–1484`) and `MarginLlr` is
  `best[at] − second[at]`.
- **The speed grid** ran 8 to 40 WPM keeping only `bestScore` (`:794–805`).
- **Numerical guards**: none in the path scores. `LogLikelihoods` guards a NaN
  sigma and floors the envelope at 1e-12; nothing bounds the accumulated score.

**And `wasDown[j]` was read in exactly one place — the alternation check at
`:1458`** — which is what made the restructure tractable.

### Task 2 — the lattice indexed by `(hop, kind)`

`best`, `second`, `fromHop` and a new `fromKind` are now `[hop, kind]`. The
alternation rule is checked **against the state** rather than against whichever
path won at `j`. Five kinds, so five times the state and the same enumeration.
The duration densities and the evidence term are untouched, as the task requires.

**Measured over the scored corpus, before and after:**

| | before | after |
|---|---|---|
| yield | 0.763 | **0.768** |
| precision | 0.761 | **0.766** |
| substitutions | 61 | **58** |
| deletions | 30 | 31 |

**Precision rose, so the paths the old search discarded were not doing useful work
by accident.** The gains land where the reading was already nearly right:
`cw-2026-08-18-004507` from 0.930 to 0.947, `cw-2026-08-22-032113` from 0.821 to
0.857 with its deletions going to nought.

**And the transcripts read visibly better in places.** `PREDICTED 10.7` where there
was `DICTED 10.7` behind blocks; `LINKS TO A R T I C L E S O R OTHER WEBSITES
MENTI`; `IULLETIN CAN BE FO TA ND IN TELEWRITTER`. Those are the recovered paths.

`TheSilencePropertyIsLockedTests` green and unmodified; twelve adjudicated anchors
green.

### Task 3 — the posterior

Forward–backward over the new lattice, in `CwProbabilisticDecoder.Posterior.cs`.
Computed once for the winning speed rather than per hypothesis, because the
backward pass is O(hops × kinds² × span) and the grid runs thirty-three of them.

**Log domain throughout, and nothing exponentiates a raw score.** `LogSum` factors
out the larger term so `Math.Exp` only ever sees a non-positive number, and a gap
past −700 is dropped, which is exact rather than approximate. **Thirteen tests**,
including `1e300` in either argument, both infinities, digital silence, and every
posterior on three real captures asserted to lie in [0,1].

**Where no path reaches the end the answer is null**, not a normalisation by
nothing.

### Task 4 — the gate, and it does not open

**The correlation, beside the five that came before:**

| quantity | correlation with per-character correctness |
|---|---|
| `MarginLlr` | −0.341 |
| `MarginShareForRecord` | −0.286 |
| `SpanMarginForRecord` | −0.246 |
| **`Posterior`** | **+0.050** |

**It is the first of six that is not negative, and that is the whole of the good
news.** On 301 characters the standard error is about 0.058, **so +0.050 is
positive in sign and indistinguishable from zero in magnitude.** The medians say
it plainly: **0.8433 on right characters against 0.8382 on wrong.**

**The threshold sweep is the useful form of the same fact:**

| threshold | kept | blocked | yield | precision |
|---|---|---|---|---|
| none | 301 | 0 | 0.752 | 0.804 |
| 0.50 | 284 | 17 | 0.724 | **0.820** |
| 0.70 | 197 | 104 | 0.503 | **0.822** |
| 0.80 | 164 | 137 | 0.416 | 0.817 |
| 0.85 | 147 | 154 | 0.366 | 0.803 |
| 0.90 | 121 | 180 | 0.292 | 0.777 |
| 0.95 | 99 | 202 | 0.233 | 0.758 |
| 0.99 | 71 | 230 | 0.149 | 0.676 |
| 0.999 | 37 | 264 | 0.053 | 0.459 |

*(over the 301 characters inside an aligned truth span, which is a smaller set
than the 384 the corpus score uses — the two numbers are not the same measurement
and are not compared.)*

**Precision peaks at 0.822 and then falls.** Above about 0.85 the threshold blocks
correct characters faster than wrong ones, which is what a correlation of +0.05
looks like from the other side. **The best trade on the whole curve is 1.6 points
of precision for 2.8 points of yield, against a precision target of 0.99.**

**So no gate moved onto it.** Picking 0.50 off a plateau that sits inside the noise
is fitting noise, which is the failure unit 045 avoided on the filter width and
the standard this project holds.

No decision was recorded under §12.1.

## 2. What the owner should expect

**The decoder reads slightly better than it did and nothing on screen behaves
differently.** Precision 0.761 to 0.766, yield 0.763 to 0.768, three fewer wrong
letters across the corpus. A frequency with nothing on it still shows nothing —
the silence lock is green and untouched.

What is now true of the tree:

- The lattice is indexed by `(hop, kind)`. **A class of legal path that the search
  could not previously reach is now reachable**, and that is where the gain came
  from.
- A real posterior exists and is carried on every character. **Nothing consumes
  it.**
- `tools/Hamlet.PitchRank confidence` prints the correlation table and the
  threshold sweep in one command.

**What will look wrong but is not:**

- **The posterior is computed and ignored.** That is task 4's measured outcome,
  not an omission.
- **`+0.050` is reported as a pass in sign and a failure in substance.** The order
  says continue if it correlates positively. It does, barely — and the sweep is
  what decides whether that is usable, and it is not.
- **The engine regression's result is not in this report yet**; it is amended
  below. The app suite is 519 passing, 0 failing, and every targeted batch was
  green.

### Amendment — the engine regression

**Pending.** Replaced by the result and the comparison against unit 046's failing
set when the run lands. **If it is not replaced, the run did not finish** — the
HM-OPEN-061 host crash has now ended four of them, including unit 047's, and an
unreplaced line is the honest record of that rather than an omission.

## 3. What you should see

**The architectural fix worked and the thing it was for did not.**

Indexing the lattice by `(hop, kind)` recovered paths the search was discarding
for a reason that was not part of the model, and **precision rose 0.761 to 0.766
with substitutions falling 61 to 58.** That is a real gain from a change made for
a different reason entirely, and it settles the question units 044 and 046 left
open: those discarded paths were not helping by accident.

**But the posterior it made possible does not separate right from wrong.**
+0.050 on 301 characters, medians 0.8433 against 0.8382, and a threshold sweep
that peaks at 0.822 precision and then goes backwards.

**Why, and this is the part worth carrying forward:** the posterior at the winning
path's states is **near one almost everywhere** — median 0.84 on right and wrong
alike. The lattice is so dominated by a single path that the marginal barely
spreads, so the normalisation that was supposed to make the quantity meaningful
instead makes it uniform. **The level term does cancel, exactly as intended. What
is left over is not informative.**

**That is six quantities now, and the sixth fails differently from the first
five.** The five were anti-correlated because they carried loudness. This one is
uncorrelated because the model is too confident. **A decoder cannot measure its own
doubt while its evidence model admits only one plausible reading** — and that
points at the evidence term, not at the search, which is where tasks 5 and 6 were
already aimed.

## 4. What's blocking us

One ruling, and it decides the rest of this unit.

> **The confidence work moves to the evidence model, or the precision target is
> pursued another way.**
>
> Six quantities have been measured against per-character correctness. Five were
> negative because they carried an unbounded loudness term. The sixth, a proper
> posterior with that term cancelled, is **+0.050 — positive in sign and inside
> the noise** — because the marginal is near one almost everywhere. **Its
> threshold sweep peaks at 0.822 precision against a target of 0.99 and falls
> above 0.85.**
>
> **The diagnosis is that the model is overconfident, not that the search is
> wrong.** The lattice now finds every legal path and the posterior says almost
> all the probability sits on one of them, which cannot be true of audio a human
> reads at 76 %. That is the evidence term — the per-hop Gaussian and Rayleigh
> likelihoods — being far too sharp, so competing readings are driven to
> negligible probability whatever the audio says.
>
> **Rejected: gating on it at 0.50 anyway.** 1.6 points of precision for 2.8 of
> yield, chosen off a plateau inside the noise, and nowhere near the target.
> **Rejected: a seventh quantity of any family.** Six is enough to say the problem
> is not which number is read off the model.
> **What tasks 5 and 6 would do, and why they may now matter more than they did:**
> putting the speed inside the lattice and fitting the duration densities from the
> corpus both widen the model — more hypotheses genuinely in play, and durations
> whose spread matches real fists rather than textbook ratios. **If the
> overconfidence is in the duration penalty's width, task 6 addresses it
> directly.** That is a plausible route and it is not this session's to choose.

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140.

1. **The eight 2026-08-29 captures are not in the tree**, a sixth consecutive unit.
2. **The confidence question above** — six measured, and the sixth fails
   differently from the first five.
3. **The answer key's licensing** — §2.1 and HM-DEC-049 against vendoring an ARRL
   bulletin, which bounds how much truth the score can ever have.
4. **The mode and filter's place in the owned-settings contract** — unit 047.
5. **What the digital rows state for the five they are silent on** — unit 047.
6. **The pedestal ranking is measured at 34 of 44 and unbuilt.**
7. **A dial move's threshold is provisional at 500 Hz.**
8. **The transcript break's wording.**
9. **Whether `CwPitch` should follow an admitted station.**
10. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
11. **The `reading` line's span wording needs approval.**
12. **Two stations closer than 125 Hz are not named.**
13. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
14. **Nothing checks that deleting a surface is not deleting a capability.**
15. **The engine test host crashes**, wider than the class HM-OPEN-061 names, and
    it has now ended four full runs. Owned by Claude, not waiting on a ruling.

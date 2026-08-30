UNIT:       051 — the threshold, the window, and the squelch — 2026-08-30
PHASE GOAL: 85% correct CW, precision before yield.
UNIT GOAL:  Stop characters reaching the screen from a pitch nothing judged to be a station, and stop the survey refusing stations that are plainly there.
ADVANCED:   **half.** The invention is stopped and precision rose to 0.888. **The false rejection is not fixed**, the repair for it was measured and refused, and it cost four W1AW anchors that nothing now gives back.
NUMBER:     **precision 0.858 → 0.888, yield 0.914 → 0.745**, over 384 adjudicated characters.
DRIFT:      0 consecutive units without advance.

## 1. What Claude did

**Hamlet confirmed.** All four gate checks verified against the tree before the
order was read, with `Hamlet.sln` and `CLAUDE.md`'s header corroborating. Branch
`main`, every task committed and pushed, every push succeeded. **Nothing here is
evidence about the radio.**

**Five of seven tasks. Tasks 4 and 5 were not done and task 7 was, which inverts
the order's plan** — task 7 was the drop candidate and it is the one that could
be finished, because **the two captures this whole unit is about are not in the
repository.**

### The blocker, stated once

**`cw-2026-08-30-001650` and `cw-2026-08-30-001547` are not in the tree.** Nothing
under `tests/fixtures/` matches `2026-08-30`. Every acceptance criterion in tasks
3, 4 and 5 is written against them:

- Task 3 — *"On `cw-2026-08-30-001650` the survey admits 575–625 Hz, `unkeyed`
  reads NO, and duty reports about 23%."* Unverifiable.
- Task 4 — *"`-001547` either becomes admitted or stays refused."* Unverifiable.
- Task 5 — the captures become fixtures. **Impossible.**

Task 3's *change* was built and measured against the corpus that exists, which is
what settled it. Task 4 was not built: it changes admission, and building a second
unverifiable admission change straight after the first was measured and refused
would have been the same mistake twice.

### Task 1 — the refusal was withheld, not missed, and that changes the lesson

**The order says it has been ordered three times and this is the fourth. The tree
says it was ordered once and then blocked on a ruling.**

Commit **`1366199`, 2026-08-24**, in its own message: *"Refusing to decode at an
unmeasured pitch was built and measured and is not shipped. It costs N4L on
cw-2026-08-17-134712 and text on six other captures… Honesty and that callsign are
in tension and the ruling is Tim's."* The same paragraph still stands at
`CwDecoder.cs:780`.

So the session built it, measured its cost, escalated it, and declined to make the
operator's decision for him. **What took six days was the ruling** — and the ruling
arrived in the header of this order. That is a different failure from "nobody did
it", and it has a different fix: not more diligence, but a shorter path from a
recorded ask to a ruling.

**Every gate on the emit path, with what each tests:**

| gate | where | what it asks |
|---|---|---|
| window gate, 1.40 | `CwProbabilisticDecoder.cs:865` | is this stretch better explained by a message than by silence |
| character margin, 1.0 | `CwProbabilisticDecoder.cs:1643` | does this letter clear its own evidence (marks, never deletes) |
| refusal floor, 14 | HM-DEC-120, settled pass | is the whole reading strong enough to assert |

**All three ask about the audio at the chosen pitch. None asks whether anybody
chose the pitch.** That is the hole 61 characters came through.

### Task 2 — the squelch is wired, and the cost is real

Same condition the sheet already prints, asked one step earlier of the same field.
Blocks rather than deletions; a word gap asserts nothing and is left alone.

| | before | after |
|---|---|---|
| **precision** | 0.858 | **0.888** |
| yield | 0.914 | **0.745** |
| substitutions | 30 | 16 |

Precision rises three points, and it rises because the blocked characters were
wrong more often than the average.

**Per capture:** KD0UN, AA4MP/4 and VA3VRR unchanged at 1.000. The losses are the
ARRL bulletins — 032050 0.831→0.322, 032113 0.857→0.250, 032129 0.905→0.667,
032012 0.922→0.804. `N4L` to zero, which Tim ruled.

### Task 3 — built exactly as specified, measured, and refused on three counts

**The fraction sweep, all nine points:**

| fraction | yield | precision |
|---|---|---|
| 0.20 | 0.560 | 0.601 |
| 0.30 | 0.695 | 0.728 |
| 0.35 | 0.674 | 0.751 |
| 0.40 | 0.695 | 0.703 |
| 0.45 | 0.740 | 0.770 |
| **0.50** | 0.742 | **0.787** |
| 0.55 | 0.711 | 0.742 |
| 0.60 | 0.740 | 0.738 |

1. **Not monotonic** — up, down, up, down. The order forbids adopting off such a
   curve.
2. **Every candidate far below the floor.** Best 0.787 against 0.888 with Otsu and
   a hard floor of 0.858.
3. **It fails its own acceptance criterion.** That was *"on the known-good captures
   the threshold lands within a decibel or two of where it lands today."*

**Measured, per capture, in decibels:**

| capture | Otsu | percentile | move | p20 | p98 |
|---|---|---|---|---|---|
| `013347` | −68.4 | −63.8 | **+4.6** | **−110.2** | −17.4 |
| `134712` | −35.6 | −30.6 | **+4.9** | −41.3 | −19.9 |
| `003758` | −33.2 | −31.1 | +2.1 | −42.9 | −19.4 |
| `012403` | −30.2 | −28.5 | +1.7 | −36.8 | −20.2 |
| `004507` | −30.7 | −30.1 | +0.6 | −40.7 | −19.5 |
| `031838` | −32.3 | −29.0 | +3.3 | −38.4 | −19.6 |
| `031905` | −32.3 | −29.0 | +3.2 | −38.5 | −19.5 |
| `031948` | −32.9 | −29.1 | +3.8 | −38.6 | −19.5 |
| `032012` | −32.0 | −29.8 | +2.2 | −40.0 | −19.6 |
| `032050` | −31.6 | −29.3 | +2.3 | −39.1 | −19.5 |
| `032113` | −32.5 | −29.6 | +2.9 | −39.7 | −19.5 |
| `032129` | −32.8 | −29.7 | +3.0 | −40.0 | −19.5 |

**It lands 0.6 to 4.9 dB higher, median about 3.0, and higher on every single
capture** — which is exactly why yield collapsed. One capture in twelve is within
a decibel. On `013347` the twentieth percentile falls at **−110 dB**, because that
recording is mostly digital silence and **a percentile of silence is not a noise
floor.**

**So Otsu is right precisely where the order predicted it would be**, and its fault
is real and confined to the mostly-silent case. The function, the sweeps and the
threshold comparison are all kept in the tree with their numbers, so the next
session finds a measurement rather than an evening.

### Task 6 — `N4L` re-expressed

Retired with its reason **in the test itself**, not deleted. The recording still
runs, what it reads is still printed, and the line says what would bring it back.
HM-DEC-144 is not withdrawn: `N4L` is still what that station sent, and what is
withdrawn is the requirement that Hamlet read it, because the only way it ever did
was by luck.

### Task 7 — done, and it answers more than it was asked

**Ground truth is synthetic on purpose**: on a real capture nobody knows the
carrier to a tenth of a hertz, which is why unit 050 could not settle this at all.

| condition | error |
|---|---|
| **500.09 Hz at 18, 22, 25 WPM** | **−0.021, −0.022, −0.021 Hz** |
| 500.09 Hz, no noise at all | −0.022 Hz |
| five carriers × four speeds, busy message | never worse than ±0.03 Hz |
| 700 Hz, very low duty | **−1.25 Hz** |
| 800 Hz, very low duty | **+1.26 Hz** |

mean −0.005 Hz, spread 0.356, worst 1.255.

**The 1.1 Hz is not a keying floor.** At the exact carrier that retired `N4L` the
peak is accurate to two hundredths of a hertz. **Every outlier is a very low duty
message**, and ±1.25 Hz is the magnitude seen on the real capture — which is short
and sparse.

**So the answer is neither bias nor floor but duty**, and the fix is to measure the
peak over the stretch where somebody is actually keying. **That is task 4's window
chosen by signal strength, arrived at from the opposite direction** — which is the
strongest argument in this report for doing task 4 next.

**No decision was recorded under §12.1.** The floor violation below weighs two
costs and §12.1 puts anything touching what the display asserts with Tim.

## 2. What Tim should expect

**Precision is 0.888, up from 0.858. Yield is 0.745, down from 0.914.**

**Hamlet will no longer print letters on a frequency the survey has not admitted.**
It shows blocks instead, so you can see it heard something and would not name it.
That is the 61 characters stopped.

**And it will now stay silent on four W1AW bulletins it used to read.** That is the
other half of the same change and it is the leading ask below.

**What will look wrong but is not:**

- **Blocks where text used to be, on the ARRL bulletins.** The survey admits those
  stations partway through the recording, so the early part blocks and the later
  part reads. That is the squelch working; it is also the floor violation.
- **Task 7 done and tasks 4 and 5 not.** The captures those need are absent.
- **`CwUnitEstimator.Threshold` exists and nothing calls it.** Measured and
  refused, kept with its numbers.

**Build clean, no new warnings.** Version unchanged at 1.12.7 — the bump question
from unit 051's report is still unruled and I did not guess again.

| suite | result |
|---|---|
| `TheSilencePropertyIsLockedTests` | **6 passing, 0 failing** — green, unmodified |
| `TheAdjudicatedReadingsKeepReadingTests` | **9 passing, 4 failing** — the four are the ask below |
| `NothingActsOnTheAdmissionVerdictTests` | 2 passing |
| `IsTheHertzABiasOrAFloorTests` | 2 passing |
| `CwEmissionGateTests.NoSpeedIsNamedWithoutCharactersToNameItFrom` | **red before this unit** — verified by reverting |

## 3. What we should do next

**Task 1's answer first: nothing has ever acted on `unkeyed`, and the reason is
that the refusal was built on 2026-08-24 and withheld pending a ruling that
arrived with this order — not that it was forgotten.** Task 3's threshold sweep and
its duty table are reproduced above.

1. **Rule on the floor violation** (section 4). Nothing else in this unit is
   blocked, and this is.
2. **Get `cw-2026-08-30-001650` and `-001547` into the tree.** Three tasks of seven
   were written against them.
3. **Then task 4**, which task 7 independently argues for: choose the measurement
   window by signal strength. It is the likely fix for both the false rejection and
   the `N4L` hertz.

## 4. What's blocking us

One ruling, and it is the whole balance of this unit.

> **The squelch ships and the W1AW anchors fall, or the anchors hold and the
> invented characters come back. Both halves of the order cannot be satisfied at
> once, because the repair that was to reconcile them does not work.**
>
> Task 2 says wire the squelch. The prohibitions say **"Do not let a floor fall.
> Floors only rise"**, and name the W1AW seven. **Four of those seven anchors are
> now red** — `031905`, `032050`, `032113`, `032129`.
>
> **The order anticipated exactly this and resolved it with task 3**: *"after task 3
> these frequencies become admitted and decoding resumes legitimately. Both halves
> are needed."* **Task 3 was built as specified and refused on three independent
> measurements**, so that resolution is not available.
>
> **What the anchors actually record**: they were set on text that included
> stretches the survey never admitted. The squelch does not make Hamlet read those
> bulletins worse — it makes it decline to assert the part it was never entitled to
> assert. On `032129` the later, admitted part still reads `…ON FORECAST BUAELETIN
> ARLP034`.
>
> **Rejected: reverting task 2.** It restores 61 invented characters on an empty
> band, which is §0.0 broken and is the whole reason for the unit.
> **Rejected: moving the anchors down.** Floors are Tim's and lowering one to fit a
> change is the move §12.5 exists to stop.
> **Rejected: a narrower squelch.** Every narrowing I could construct is a second
> test for a state `unkeyed` already computes, which the order forbids outright.
> **What this session could not settle** is whether the anchors should be
> re-expressed with their reason, the way `N4L` just was — which would say plainly
> that four bulletins are read only in part, and why.

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140.

1. **The squelch against the W1AW floor** — raised above, 2026-08-30. In the tree
   from `95a5e06`.
2. **The two 2026-08-30 captures are not in the tree**, and three tasks needed
   them. **The eight 2026-08-29 captures are also still absent**, a tenth
   consecutive unit.
3. **The guard narrowing** — 2026-08-29, unit 051. In the tree at
   `tests/Hamlet.RadioEngine.Tests/Rig/RigStateModelTests.cs`.
4. **The version bump** — 2026-08-29. Two orders both called themselves 050 and one
   called itself 051 the day after another did; `Directory.Build.props` still says
   1.12.7 and I have not guessed twice.
5. **The filter byte against HM-DEC-149** — **HM-OPEN-062**, unruled.
6. **The evidence term's unbounded scale** (unit 049), to be re-measured against
   the new pitch before it is ruled on.
7. **The answer key's licensing.**
8. **The mode and filter's place in the owned-settings contract** — unit 047.
9. **What the digital rows state for the five settings they are silent on.**
10. **The pedestal ranking is measured at 34 of 44 and unbuilt.**
11. **A dial move's threshold is provisional at 500 Hz.**
12. **The transcript break's wording.**
13. **Whether `CwPitch` should follow an admitted station.**
14. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
15. **The `reading` line's span wording needs approval.**
16. **Two stations closer than 125 Hz are not named.**
17. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
18. **Nothing checks that deleting a surface is not deleting a capability.**
19. **The test host crashes** in both suites — **HM-OPEN-063**. Owned by Claude.
20. **`PROJECT_CARD.md` has no phase field.**

UNIT: 034 — stopped at task 6 of 8 — 2026-08-27 18:24

## 1. What Claude did

**Stopped. Tasks 1, 2, 5, 6 and 8 done; tasks 3, 4 and 7 not done, because task
2 barred them.**

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. **Hamlet confirmed.** Branch `main`
throughout, five commits, all pushed, none refused. Version 1.11.30 to 1.11.31
per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

**Tasks 3 and 4 were not dropped for time. The order forbids them:** *"Do not
build tasks 3 or 4 on a hypothesis task 2 did not confirm"*, and task 2
disconfirmed it. Task 7 is the corpus re-run that would have measured tasks 3 and
4, so it has nothing to measure. **Task 8, the named drop, was done.**

### Task 1 — the completed run, and what had been killing it

**1854 tests, 1826 passed, 28 failed, 18.5 minutes**, and the failing set is
**byte-identical by name** to the stable 28.

**What killed the previous three attempts was my own check, not the
environment.** At `-v n` the runner's summary reads `Total tests: / Passed: /
Failed:`; my monitor was waiting for `Passed!`, which that verbosity never
prints. **Two of the runs unit 1.11.30 reported as killed had very likely
finished**, and that report's caution about them was built on my own bad grep.
The count now comes from the summary the runner actually writes.

### Task 2 — the hypothesis is false, and what replaces it is worse

This is section 3's lead and is set out there.

### Task 5 — the junk is now capturable

The capture sheet gains `unkeyed`, which states **the conjunction nothing on it
could state**: characters reached the screen from a pitch the survey never
admitted keying at. Both halves were already recorded; the pair never was.

**And it corrects a sentence that stopped being true yesterday.**
`ToneForTheRecord` told every unmeasured pitch it was *the middle of the bank*.
Since unit 1.11.30 let the strongest bin choose, an unmeasured pitch is sometimes
a bin picked for being loudest and sometimes still a centre nobody chose. **Those
are different claims** and the field exists to keep them apart (§0.0).

### Task 6 — the list, with its own disproof at the top

Written into `CAPTURE_INVENTORY.md`. **Nothing was adjudicated.**

**The caveat leads, because it decides what the table is worth.**
`cw-2026-08-24-012403` is an **adjudicated anchor holding `DE KD0UN KD0UN K`**,
and its own sidecar reads `no keying, 6 ms key down, 17 dB swing`, `inThis 0
characters`, `text nothing read`. **On every axis the evidence has, a recording
with a ruled-on callsign in it is indistinguishable from one holding nothing.**

The axis that does separate, where it exists, is **how long the key was down**:
the meter's `no keying` verdicts sit at **4 to 7 ms** against a 60 ms dit at
twenty words a minute. **Both known-empty recordings predate the keying meter and
carry no `keying` line at all**, so they cannot be compared on the one axis that
would matter.

**One real candidate: `cw-2026-08-25-012748`** — two characters in thirty
seconds, no plausible element length, nobody claiming a station. **That is one
candidate and not a corpus.**

### Task 8 — `CwPitchRanking`'s fate

**Task 3's check-on-the-winner was never built, so nothing needs it, and its own
scoring is measured not to choose pitches — 0 of 4 in unit 1.11.29.** Task 2's
harness scored candidates inline with `DecodeUngated` and `LikelihoodRatio` in
about ten lines, which is what the class wraps.

**Recommendation: delete it.** It has no caller, no prospect of one, and a
measured record of choosing wrongly. **It is not deleted here** — that is one
line and it is Tim's, as the order says.

**One argument for keeping it, stated because it is the honest counterweight:**
it produced the 93-character figure that sent two units chasing a leak the
application does not have. Kept, with `Winner`'s warning intact, it is a standing
reminder that an offline sweep is not what the app runs.

## 2. What the owner should expect

**A station he can hear still does not reach the screen, and this unit found out
why — it is not what three orders assumed.**

**The two he can hear are not refused for being mistuned.** Swept a hertz at a
time across the integrator's own width, the best score `cw-2026-08-22-014113`
reaches anywhere is **0.84**, at 606 Hz — his own stated pitch — and
`cw-2026-08-26-125941` reaches **0.46**. The floor is 1.40. **A recording holding
nothing scores 1.05 on the same measure.**

**So at their own best pitch these stations are weaker evidence than an empty
band, and the gate is refusing them correctly on what it can measure.** The fault
is upstream of the gate, in what the envelope recovers, not in acquisition and
not in the floor.

**A dead frequency still stays quiet.** Both recordings holding nothing emit
nought, unchanged.

**What will look wrong and is not:** the capture sheet has a new `unkeyed` line
that usually says `nothing emitted`. It is meant to be dull; it is loud only on
the evening it matters.

| | baseline | end |
|---|---|---|
| engine | 28 of 1854, byte-identical by name | **a confirming run was started at 18:24 and had not returned** |
| app | 509 of 509 | **509 of 509** |

**Only app code and tests changed after the completed engine run**, so the engine
figure is expected to hold at 28 with the total rising to 1856 for two added
tests. **That is an expectation and it is labelled one.**

## 3. What you should see

**Task 2's answer: what the window ratio is made of on a station pointed at its
own pitch.** Twelve-second window, floor 1.40.

| capture | ratio | duty | keyed/quiet | pooled over keyed hops | what |
|---|---|---|---|---|---|
| `cw-2026-08-22-014113` | 0.74 | 0.531 | 3.94 | 1.39 | pointed within 7 Hz, silent |
| `cw-2026-08-26-125941` | 0.44 | 0.598 | 3.33 | 0.73 | pointed within 4 Hz, silent |
| `cw-2026-08-24-012403` | 1.69 | 0.590 | 4.79 | 2.86 | reads — the control |
| `cw-2026-08-20-014854` | 0.91 | 0.561 | 3.66 | **1.62** | **holds nothing** |
| `cw-2026-08-20-014935` | 0.07 | 0.699 | 2.78 | 0.10 | holds nothing |

**The dilution hypothesis is false, on two counts.**

**Duty is 0.53 to 0.70 on every recording, including both that hold nothing.** A
noise envelope has hops above and below its own fitted cut exactly as a keyed one
does, so **duty does not distinguish a sending station from an empty band** and
removing it removes nothing selective. There is no four-fifths of silence being
averaged in.

**And the proposed fix is worse than what it replaces.** Pooling over the keyed
hops lifts `cw-2026-08-20-014854` — which holds nothing — from 0.91 to **1.62,
over the floor**, while `cw-2026-08-26-125941` reaches only 0.73. **It admits an
empty recording and still refuses a real station**, which trades the one property
that may not be traded.

**A measurement error was caught on the way and is worth recording.** The first
cut tried was the midpoint of the envelope's decibel range, which reported duty
near 1.0 on every capture — one unusually quiet hop drags the minimum down and
the midpoint with it. **That would have rejected the hypothesis on an artifact of
the cut rather than on the audio.** The cut is fitted to the two heaps instead.

### And the deeper finding, which is why tasks 3 and 4 stopped

| capture | at the order's pitch | best pitch found | best score |
|---|---|---|---|
| `cw-2026-08-22-014113` | 0.74 at 600 Hz | **606 Hz** | **0.84** |
| `cw-2026-08-26-125941` | 0.44 at 400 Hz | 405 Hz | **0.46** |
| `cw-2026-08-24-012403` | 1.69 at 439.8 Hz | 440.8 Hz | 1.71 |
| `cw-2026-08-20-014854`, empty | 0.91 at 600 Hz | 586 Hz | **1.05** |

**Nothing in ±30 Hz rescues either station.** The best `014113` can do is 0.84 —
below the 1.05 an empty recording reaches. **The premise that these stations are
"pointed at and refused" is true, and the reason is that the decoder cannot see
them at all**, not that a threshold is misplaced.

**What the four captures emit after task 3: nothing, because task 3 was not
built.** No change satisfied the four acceptance lines, so per the order nothing
shipped.

## 4. What's blocking us

**The stations the operator can hear are below this decoder's sensitivity, and
that is a different problem from every one this project has been solving.**

Ruling asked for:

> **The next unit measures the envelope, not the gate.** At its own best pitch a
> station he can hear scores 0.84 where a recording holding nothing scores 1.05.
> No threshold placed anywhere separates those two, and no acquisition change
> reaches them — **three units have now been aimed at acquisition and a fourth
> at the gate, and the signal was never getting through the envelope.**
>
> **What to measure:** what `CwProbabilisticDecoder.Envelope` recovers from
> these two recordings against what a matched filter at the same pitch would,
> and against what the operator's ear does. The integrator is 60 Hz wide and
> parked (HM-OPEN, integrator width); **at 12 words a minute a 60 Hz filter is
> far wider than the signal**, so it passes noise the station is competing with.
>
> **What was rejected:** moving the floor, which would admit the empty
> recordings first; and any further acquisition work, which cannot help a
> station the envelope does not recover.

**This supersedes the framing of the last three orders**, and it is the one thing
that would change the answer.

---

**The empty corpus cannot be built from recorded evidence, and now there is proof
rather than an argument.**

An adjudicated anchor holding a ruled-on callsign reads `no keying, 6 ms key
down, nothing read, 0 characters` — identical to a recording holding nothing.
**Ask 8 in the standing list, "the keying meter found a station its verdict
denied", is now confirmed on a second capture and it is not a curiosity: it is
what stops the corpus being built.**

*Not proposed, because it needs a ruling:* whether Tim listens to
`cw-2026-08-25-012748` and the two 2026-08-20 recordings and rules on them. That
is three minutes of listening and it unblocks the acquisition floor.

---

**`CwPitchRanking` should be deleted and I have not deleted it.**

No caller, no prospect of one now task 3 is barred, and a measured record of
choosing wrongly on all four captures. One line, and it is Tim's.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Twenty-six inbound
after this unit. The oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150** — and
   HM-DEC-090, 095, 120, 125 and 127 are all inside it.
5. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
   **Section 4 now points at this as the next thing to measure.**
6. **A boxcar's nulls made two of five swept offsets pathological best cases.**
7. **Two stations closer than 125 Hz are not named** — the operator's item five.
8. **The keying meter** — its measurement found a station its verdict denied.
   **Confirmed on a second capture this unit, and it is what blocks the empty
   corpus.**
9. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
10. **The joint cutter cannot find word gaps on a compressed fist** (1.11.22).
11. **The constrained margin is bounded and still does not separate** (1.11.22).
12. **Four fixtures are absent and five acceptance lines were unmeasurable**
    (1.11.22).
13. **HM-DEC-086's supersession needs a record** (1.11.25).
14. **The phrasebook's arrival and the absent-widget news are gone** (1.11.25).
15. **The recent-places row has no home** (1.11.26), three options costed.
16. **The owned-property list has no enforcement of staying current** (1.11.27).
17. **A test resolved an ambiguous control by accident** (1.11.27).
18. **Nothing checks that deleting a surface is not deleting a capability**
    (1.11.28) — measured on three instances.
19. **The scanner and the calling cycle are attached to the rig with no
    control** (1.11.28), and one of them transmits.
20. **Thirteen dead `DataTemplate` blocks nothing can distinguish from live
    ones** (1.11.28).
21. **Whether every constructed view model should be reachable from a binding**
    (1.11.28).
22. **`013347` returns a likelihood ratio of 17.2 million**, with `001520`'s
    quadrillions. Parked, raised once.
23. **No capture reproduces the junk the operator is watching** (1.11.30) —
    **task 5 makes the next one carry the state that produced it.**
24. **The stations he can hear are below the decoder's sensitivity**, above.
25. **The empty corpus is blocked by the keying meter, with proof**, above.
26. **`CwPitchRanking` should be deleted**, above.

New this unit: **the sensitivity finding**, above; **the meter's second
disproof**, above; **the ranking's deletion**, above.

Closed this unit: **the completed engine run** — 1854, 28 failing,
byte-identical, and what had been killing it was my own grep. **Whether dilution
explains the refusal** — it does not, and the fix for it would admit an empty
recording. **Whether the stations are mistuned** — they are not; nothing within
30 Hz reaches the floor. **`CwPitchRanking`'s fate** — recommended for deletion.

Still open: **the lock's mixed help**; **three fixtures at accepted cost**; **an
unmeasured pitch costs `N4L`**; **the six-hertz window disagreement**;
**`CHANGELOG.md` at 1.9.0 against 1.11.31**; **the squelch has no axis**; **the
three morning captures of 2026-08-26**; **seven timing intermittents**.

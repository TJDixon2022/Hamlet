# Work instruction 021 — the gate cuts noise in half and calls it Morse

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. Branch `main` throughout, four commits, all
pushed, none refused. Version 1.11.17 to 1.11.18 per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

**All four tasks ran. Nothing was dropped.** Task 2 ships nothing, on the order's
own permitted outcome. Task 4, the drop candidate, was measured.

**The session dropped mid-task-2** and resumed. The apparatus was committed
before continuing, and no work was lost.

### The unit's premise is false, and finding that out is the unit's result

**A count of zero marks does not mean the gate was shut.** A run touching either
end of the history is truncated rather than counted, and a run spanning the whole
history touches both — so **a gate that never closes reports exactly what a gate
that never opens reports.** Nothing in this tree had ever distinguished them.

Re-measured with the two counted apart:

| | zero-mark % | **genuinely shut** | stuck wide open | truncated runs |
|---|---|---|---|---|
| **`cw-2026-08-17-013347`, the order's reference** | 65.0% | **0.0%** | **17.5%** | 47.5% |
| `cw-2026-08-17-013622` | 43.3% | **0.0%** | 3.8% | 39.5% |
| every other capture in the corpus | 0.0% | 0.0% | 0.0% | 0.0% |

**Not one of the 926 bins is shut.** And across all thirty-seven captures,
**zero have a gate genuinely shut on half their bins or more.**

There is no healthy reference signature in this corpus. The unit was built to
make the silence controls resemble `013347`, and `013347` does not have the
property it was chosen for.

### Task 1 — the signature across the whole corpus

Measured first with the order's own metric, before the confound was found:

- **1 capture of 37** reaches 50% or more zero-mark bins: `013347` at 65.0%.
- `013622` is next at 43.3%. `cw-2026-08-23-002016` is third at **0.1%**.
- **34 of 37 sit at exactly 0.0%** — not one bin in the band produces zero marks.
- **Ten of the twelve adjudicated anchors sit at 0.0%** while reading their
  callsigns. The seven ARRL bulletin captures all read anchors with every bin
  producing about fifteen marks a pass.

**So a mostly-shut gate is not a property of captures that read.** It is a
property of `013347` alone, and section 1 above shows it is not even that.

Baseline recorded by diffing which tests fail, per the order: **29 of 1841, being
the stable 28 plus one intermittent** — and a *different* intermittent from unit
1.11.17's. `ASweepIsAssembledFromItsPartsAndPublishedOnce` fired here where
`KnobTurn_TransceiveFrame_RaisesFrequencyChanged` fired last unit. The stable set
of 28 was taken as the intersection and used for the closing diff.

### Task 2 — both derivations built, both measured, neither ships

**Before either was tuned, the two quantities they key off were measured.**
Both interleave:

| capture | holds | spread at the pitch | lift at the pitch |
|---|---|---|---|
| `cw-2026-08-17-134712` | **`N4L`, reads** | **10.4 dB** | **3.0 dB** |
| `cw-2026-08-20-014854` | **nothing** | **12.0 dB** | **35.3 dB** |
| `cw-2026-08-20-014935` | **nothing** | 9.8 dB | 32.3 dB |
| `cw-2026-08-25-012823` | a station | 12.8 dB | 41.5 dB |
| `cw-2026-08-22-014113` | a station | 10.9 dB | 6.0 dB |
| `cw-2026-08-22-014308` | a station | 12.4 dB | 5.4 dB |
| `cw-2026-08-26-125941` | a station | 10.3 dB | 34.7 dB |
| `cw-2026-08-17-013347` | `VA3VRR`, reads | 53.6 dB | 10.9 dB |
| `cw-2026-08-18-004507` | the bulletin, reads | 18.3 dB | 25.9 dB |

**`N4L` reads a callsign at spread 10.4 while `014854`'s noise sits at 12.0** — a
minimum-spread bound anywhere near there shuts the station and admits the noise.
**And on lift the pair is inverted**: the noise stands 35.3 dB over its band floor
where the station that reads stands 3.0. The band floor is a median over distant
bins, so a uniformly noisy recording gives every bin a low lift and a quiet
recording gives everything a high one.

**Candidate A — the threshold above the band floor.** Swept at 3, 6, 10, 15 and
20 dB:

| floor | anchors held | `014854` shut / stuck | `014935` shut / stuck | stations with marks at pitch |
|---|---|---|---|---|
| **today** | **12/12** | 0.0% / 0.0% | 0.0% / 0.0% | **4 of 4** (15, 17, 10, 19) |
| 3 | 9/12 | 44.0% / **44.8%** | 45.1% / **45.5%** | 2 of 4 |
| 6 | — | 44.7% / 42.9% | 45.8% / 43.8% | 2 of 4 |
| 10 | **8/12** | 47.5% / 37.3% | 47.9% / 41.9% | 1 of 4 |
| 15 | — | 49.5% / 20.1% | 50.3% / 34.9% | 0 of 4 |
| 20 | 9/12 | 52.6% / 5.5% | 53.2% / 11.0% | 0 of 4 |

**It costs anchors at every setting, and half its quiet is a jammed gate.** At 3
dB the controls look 88.8% and 90.6% silent, of which nearly half is the gate
stuck wide open. It jams open on the stations too: `012823` and `125941` reach
**duty 1.00 at their own pitches** at every floor tried.

**Candidate B — two levels must be two things.** Swept at 12, 15, 20 and 25 dB:

| spread | anchors held | `014854` shut | `014935` shut | stuck-open | stations with marks |
|---|---|---|---|---|---|
| **today** | **12/12** | 0.0% | 0.0% | 0.0% | **4 of 4** |
| **12** | **11/12** | **49.6%** | **56.9%** | **0.0%** | 2 of 4 |
| 15 | 10/12 | 65.2% | 77.5% | 0.0% | 0–1 of 4 |
| 20 | **2/12** | 97.6% | 100.0% | 0.0% | 0 of 4 |
| 25 | — | 100.0% | 100.0% | 0.0% | 0 of 4 |

**B is the better mechanism and still fails.** It shuts bins outright with **no
stuck-open bins at any setting**, which is exactly the half A could not deliver.
At 12 dB it takes the silence controls to genuinely shut on half their bins,
emitting nothing, with eleven anchors surviving. **It loses the twelfth** —
`cw-2026-08-24-012403` drops `DE KD0UN KD0UN K` at every setting — and the
acceptance admits no shortfall.

**What the distance would have to be, and what it costs.** For B to leave every
anchor standing the bound must sit below 12 dB, and below 12 it shuts almost
nothing. For it to shut the controls to the 90%-plus the order asks for, the
bound must reach 20, which costs ten anchors of twelve. There is no window.

**And the finding that outlives both sweeps**: today's gate **already produces
marks at all four stations' own pitches** — 15, 17, 10 and 19. Neither candidate
improves on that at any setting; both reduce marks everywhere, on stations and
noise alike. **The gate is not failing to find the stations. It is finding
everything.**

### Task 3 — the corpus re-measured, because the instrument moved

Nothing shipped, so no decode changed. What did change is the measurement, and
re-running the corpus with shut, stuck-open and truncated counted apart gives the
table in section 1: **zero of thirty-seven captures have a genuinely shut gate on
half their bins.**

The four target captures, unchanged from unit 1.11.17: `012823` 41 characters at
450 Hz measured; `014113`, `014308` and `125941` **zero characters and no
measured pitch**.

**Is the fault now downstream for any of them? No — and that is the honest
answer.** The order asked this to be said for each. Nothing was fixed, so nothing
moved downstream; all four are exactly where they were, and the gate that was
supposed to be repaired turns out not to have the defect the unit named.

### Task 4 — the survey's history hop

| | |
|---|---|
| tracker hop | 240 samples = **5.00 ms** |
| survey hop | decimated by 2 = **10.00 ms** |
| de-glitch median | 3 hops = 30.0 ms window |
| shortest mark that survives it | **20.0 ms**, which is a dit at 60 WPM |

**So unit 1.11.15's claim needs narrowing.** A 31 ms dit is not lost — it
survives the de-glitch with room to spare. What it lacks is *resolution*: at 3.1
hops it is measured to ±10 ms, which is ±32%, and that is enough to smear a
dah/dit ratio of 3.3 into the 5.85 the survey reported.

**Five captures send at under four hops a dit:**

| capture | WPM | dit ms | hops per dit |
|---|---|---|---|
| `cw-2026-08-22-014308` | 31.6 | 38.0 | 3.8 |
| `cw-2026-08-17-013622` | 36.4 | 33.0 | 3.3 |
| `cw-2026-08-25-021825` | 38.7 | 31.0 | 3.1 |
| `cw-2026-08-26-125941` | 40.0 | 30.0 | 3.0 |
| **`cw-2026-08-20-014935`** | **44.4** | **27.0** | **2.7** |

**The last row is a silence control.** `KeyingEnvelope` reads "44 words a minute"
off a recording of nothing, which is its own comment on how much of this
measurement chain is describing noise.

### The suite

| | baseline | end |
|---|---|---|
| engine | 29 of 1841 (stable 28 + one intermittent) | **28 of 1841** |
| app | 503 passing | **503 passing, 0 failing** |

**Byte-identical to the stable 28 — nothing moved, and no intermittent fired this
run.** Diffed rather than totalled, per the order.

### Where the instruction and the tree disagree

- **The reference signature does not exist.** Section 1. This is the unit's
  central mismatch and it invalidates the acceptance as written.
- **Ten of twelve anchors sit at 0.0% zero-mark while reading their callsigns**,
  so "most bins producing no marks" was never a property of a healthy capture.
- **The baseline was 29, not 28**, and the intermittent that fired was a
  different one from last unit's. The stable set is 28.
- **`CLAUDE_CODE.md` is at 1.4** and §8 does specify four sections, as stated.
- **`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150**, nor
  Tim's rulings of 2026-08-25/26 including the one this unit acted under.

## 2. What Tim sees at the radio

**Nothing changed, and a station he can hear still does not reach the decoder.**

Two candidate repairs were built exactly as ruled and both were measured against
the anchors and the silence controls rather than against the captures that
motivated them. Neither passed. Under the ruling's own terms, neither ships.

**But the unit did not come back empty.** It found that the measurement the whole
plan rested on cannot tell a shut gate from one jammed wide open, and that the
capture chosen as the healthy example does not have the property it was chosen
for. Four days of work were aimed at making noisy captures resemble a reference
that was never measured correctly.

**The real shape of the problem is now visible**: the gate opens on essentially
every bin of every recording in this corpus, including two that hold nothing at
all, and it already opens on all four stations the operator can hear. It is not
missing them. It is finding them and everything else equally, and no threshold
placed on either quantity tried can tell those apart.

**What will look wrong and is not:**

- **Two commits that change no behaviour.** Both derivations ship switched off,
  and nothing in the application sets either. The suite is byte-identical.
- **The engine shows 28 where the last two units showed 29.** No intermittent
  fired this run. The stable set is unchanged.
- **`cw-2026-08-25-012823` still shows 41 characters of soup**, and the other
  three still show nothing.

## 3. What you should see

**Zero marks on the two silence controls, before and after: 0.0% and 0.0%, and
after — nothing shipped — 0.0% and 0.0%.** The candidate that came closest, B at
12 dB, would have made them 49.6% and 56.9% genuinely shut at the cost of one
adjudicated anchor.

**How many of the four stations the operator can hear produce marks at their own
pitches: all four, today, and all four before this unit began** — 15, 17, 10 and
19 marks. Neither candidate raised that number at any setting; both lowered it.

**And the number that undoes the unit's premise:**

| | zero-mark | **genuinely shut** | stuck open |
|---|---|---|---|
| `cw-2026-08-17-013347`, the reference | 926 of 1,425 | **0** | 249 |
| the whole corpus, captures with ≥50% shut | — | **0 of 37** | — |

**The suite**: engine 28 of 1841, byte-identical to the stable set; app 503 of
503.

## 4. What's blocking us

**The acceptance metric was wrong, and it should be restated before another unit
is written against it.**

Ruling asked for:

> **"Bins producing zero marks" is not the measurement. A run touching either end
> of the history is truncated rather than counted, so a gate that never closes
> and a gate that never opens both report nought marks. The acceptance is
> **`Shut`** — no marks *and* the gate held open under 5% of the history, or no
> gate ran at all — reported beside **`StuckOpen`** and **`Truncated`** so the
> three can never be added together again. On that measurement no capture in this
> corpus is healthy, and `cw-2026-08-17-013347` is 0.0% shut rather than 65%.**

The three predicates are in the tree and every table in this report uses them.
The evidence is in section 1: `013347`'s 926 no-mark bins are 0 shut, 249 stuck
wide open and 677 truncated.

*Rejected: candidate A as ruled.* Costs anchors at every setting — 9, 8 and 9 of
12 — and nearly half of the quiet it buys is the gate jammed open, including on
the stations it was meant to rescue.

*Rejected: candidate B as ruled.* The better mechanism, and it still loses
`cw-2026-08-24-012403` at every setting while shutting `cw-2026-08-22-014113`'s
own station to 90.5%. No bound leaves twelve anchors standing and shuts the
controls.

---

**Neither quantity separates a station from noise, measured before either was
tuned, and this is the finding to carry forward.**

`N4L` reads a callsign at spread **10.4 dB** and lift **3.0 dB**. The silence
control `cw-2026-08-20-014854` sits at spread **12.0 dB** and lift **35.3 dB** —
above it on both axes, and on lift by thirty-two decibels. Any bound on either
quantity that keeps `N4L` admits that noise, and any bound that refuses the noise
refuses `N4L`.

That is now the third pair of axes measured and rejected: separation and the
dah/dit ratio in unit 1.11.17, and the bin's level spread and its lift over the
band floor here.

---

**The gate opens on everything, and the next question is why it opens on
recordings holding nothing.**

Not a threshold question. On two recordings of an empty band, every bin produces
about nineteen marks a pass, and those marks survive a de-glitch median designed
to remove chatter. Something is producing structured-looking runs out of noise
before any threshold is consulted, and it is not the threshold's placement,
because moving it does not selectively remove them.

*Not proposed, because it needs a ruling:* the de-glitch median is 3 hops, which
at a 10 ms survey hop removes only runs of 10 ms or less. Noise crossing a
threshold produces runs of 20 and 30 ms freely, and those become marks. Whether
the de-glitch should scale with the speed being tracked, or whether the history
should be longer than three seconds so a run has somewhere to be short relative
to, are both changes to the instrument (HM-DEC-119).

---

**Task 4's finding narrows unit 1.11.15's claim rather than confirming it.**

A 31 ms dit is not lost to the 10 ms hop — it survives the de-glitch, whose floor
is 20 ms. What it lacks is resolution: 3.1 hops means ±32% on every mark length,
which is enough to turn a true 3.3 dah/dit ratio into the 5.85 the survey
reported. **The hop is a precision problem, not a detection problem**, and a
repair aimed at detection would miss.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Fifteen inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's rulings of 2026-08-25/26, including the one this unit acts under.**
5. **The tone tracker** — the gate is measured and neither candidate ships;
   confirmation, displacement and selection stay measured inert.
6. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
7. **The guard's gap is two to one**, calibrated on two empty captures; the
   operator's noise session crossed it live on 2026-08-26.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named** — the operator's own item
   five, and the last of his list not yet attempted.
10. **The keying meter** — its measurement found a station its verdict denied,
    and on `cw-2026-08-20-014935` it reads 44 words a minute off silence.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **The acceptance metric restatement**, above — the headline ask.
13. **Neither spread nor lift separates a station from noise**, above.
14. **The gate opens on everything, including two empty recordings**, above.
15. **The de-glitch removes only 10 ms runs at a 10 ms hop**, above.

New this unit: **the reference signature does not exist — `013347` is 0.0% shut,
not 65%**, above; **both ruled candidates measured and neither ships**, above;
**the hop is a precision problem rather than a detection one**, above.

Closed this unit: **the gate's threshold as the fault** — both derivations Tim
ruled were built and measured, and neither is it. **Unit 1.11.15's hop claim**,
narrowed to precision with the arithmetic behind it.

Still open: **the lock's mixed help**; **the "Hold this pitch" button**; **three
fixtures at accepted cost**; **`001520`'s quadrillions**; **the reference and
port integrator difference**; **an unmeasured pitch costs `N4L`**; **the
six-hertz window disagreement**; **the short-character bias**; **the Avalonia
geometry offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.18**; **the whole-file
second pass**; **the squelch has no axis**; **the three morning captures of
2026-08-26**; **six timing intermittents, two of which fired in the last three
runs**.

# Work instruction 024 — the operator's ear, and the unit a station keeps

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. Branch `main` throughout, four commits, all
pushed, none refused. Version 1.11.20 to 1.11.21 per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

**Task 1 shipped and was committed before task 2 began**, as the order requires.
**Task 2 did not separate, so task 3 was not built.** Task 4, the drop candidate,
ran with a limitation stated below.

## 2. What Tim can do tonight that he could not this morning

**Press "I hear a station" and Hamlet will decode where he says a station is.**

The button already banked the last half minute and added a row to the list. It
now also points the decoder at the strongest **keyed** bin in the band and holds
it there, bypassing admission entirely — the thing that has refused every station
he can hear for six units.

**It releases when he clears it or when he moves the dial.** Pressing "Hold this
pitch" lets go; a QSY lets go on its own, because a pitch asserted on one
frequency is not evidence about the next.

**The status line says what it did**, naming the frequency it took and saying
plainly that Hamlet did not find keying there, he did. The capture sheet says the
same: `NOT MEASURED: you said you could hear a station, so this is the loudest
bin in the band at that moment`. **`PitchWasMeasured` stays false throughout**, so
nothing anywhere implies Hamlet found what a human found.

**What it does on the four captures he can hear** — pressing, then carrying on
listening:

| capture | he hears it at | Hamlet takes | characters | before |
|---|---|---|---|---|
| `cw-2026-08-25-012823` | 500 Hz | **500.0 Hz — exact** | **12** | 41 |
| `cw-2026-08-22-014308` | 606 Hz | **625.0 Hz** | **35** | **0** |
| `cw-2026-08-22-014113` | 607 Hz | 600.0 Hz | 0 | 0 |
| `cw-2026-08-26-125941` | 403.5 Hz | 450.0 Hz | 0 | 0 |

**`cw-2026-08-22-014308` reads thirty-five characters where it read nothing**, and
that is the first movement on any of these four in this whole phase.

**Two still read nothing, and one of them is pointed within seven hertz of the
station.** `014113` is asserted at 600 against a station at 607 and still emits
nothing, so on that capture the fault is downstream of the pitch entirely. That
is a finding rather than a shortfall, and it names where the next unit goes.

**Both silence controls still read nothing with the assertion applied**, and the
automatic path is untouched.

**What will look wrong and is not:**

- **The engine shows 29 red where the stable set is 28.** One intermittent,
  `TheStateMonitorDoesNotHoldUpADisconnect`, verified passing alone.
- **The press takes a moment longer.** It now sweeps the band for the keyed pitch,
  on a background thread so the window does not freeze.
- **`cw-2026-08-25-012823` reads 12 where it read 41.** The 41 was soup off a
  wrong station at 450 Hz; the 12 come from the right one at 500.

## 3. What you should see

**Task 1, the four captures he can hear** — the table above. Pitch chosen, exact
on `012823`, nineteen hertz out on `014308`, and two that read nothing.

**Task 2's sentence:**

> **A station's good passes do not agree on a unit where noise's do not — the
> comparison is inverted. `cw-2026-08-20-014935`, which holds nothing, agrees
> tightest in the whole table at a coefficient of variation of 0.028;
> `cw-2026-08-17-013347`, which reads `VA3VRR`, agrees worst at 0.400.**

| capture | holds | good passes | units fitted (ms) | **spread** |
|---|---|---|---|---|
| **`cw-2026-08-20-014935`** | **nothing** | 6 | 28, 29, 30, 30, 30, 30 | **0.028** |
| `cw-2026-08-24-012403` | `DE KD0UN` | 3 | 28, 30, 30 | 0.032 |
| `cw-2026-08-26-125941` | a station | 12 | 25, 30 ×10, 37 | 0.078 |
| **`cw-2026-08-20-014854`** | **nothing** | 4 | 36, 40, 56, 60 | **0.212** |
| `cw-2026-08-17-134712` | `N4L` | 10 | 30 ×5, 31, 32, 42, 50, 50 | 0.226 |
| `cw-2026-08-25-012823` | a station | 12 | 32, 32, 35–40, 63, 63 | 0.260 |
| `cw-2026-08-18-004507` | the bulletin | 20 | 28–35, 58, 60, 60, 60 | 0.286 |
| **`cw-2026-08-17-013347`** | **`VA3VRR`** | 12 | 34, 35, 35, 35, 45, **90, 90, 93, 93, 94, 94, 96** | **0.400** |

**And the reason, which is a defect in the statistic rather than a fact about the
signal.** Almost every fitted unit in the table is **25 or 30 milliseconds** — the
bottom of the search range, which is bounded at the survey's own shortest dit.
Noise runs pile up at the de-glitch floor of 20 to 30 ms, so a unit at the search
floor fits them at a multiple of one and the residual is minimised there. **That
is agreement on the bound, not on a sender's speed.** `013347` scores worst
precisely because it is the one capture whose real dit is near 90 ms, and half its
passes correctly fit 90 to 96.

**The suite**: engine 29 of 1841 — the stable 28 plus one intermittent verified
alone; app 503 of 503.

### Task 4 — the decoder's own dit spread, with its limitation

| capture | reads | P10 | median | P90 | **spread** | LLR |
|---|---|---|---|---|---|---|
| `cw-2026-08-22-032129` | `OPAGATION` | 20.0 | 36.7 | 45.0 | **2.25×** | 7.2 |
| `cw-2026-08-22-031838` | `, AND` | 15.0 | 25.0 | 38.9 | 2.59× | 5.9 |
| `cw-2026-08-22-031905` | `DICTED 10.7` | 15.0 | 27.0 | 40.0 | 2.67× | 5.2 |
| `cw-2026-08-22-032050` | `ULLETIN CAN BE FO` | 15.0 | 26.7 | 43.3 | 2.89× | 4.9 |
| `cw-2026-08-18-004507` | the bulletin | 10.0 | 18.9 | 29.0 | 2.90× | 9.1 |
| `cw-2026-08-18-003758` | `MP/4 QNIK` | 10.0 | 21.7 | 31.7 | 3.17× | 6.9 |
| `cw-2026-08-22-031948` | the mean of 117 | 15.0 | 26.2 | 50.0 | 3.33× | 7.8 |
| `cw-2026-08-22-032012` | `R OTHER WEBSITES` | 10.0 | 21.7 | 40.0 | 4.00× | 6.1 |
| `cw-2026-08-17-013347` | `VA3VRR` | 20.0 | 30.0 | 85.0 | **4.25×** | **17,235,760** |

**The spread is 2.25× to 4.25× on every anchored capture**, consistent with unit
1.11.20's 2.47× on `134712`.

**Whether it correlates with what reads correctly could not be established, and
that is stated rather than glossed.** This measures the offline decoder at a fixed
600 Hz, not the tracked pitch the app uses, so **no anchor was held in this
configuration** and there is nothing to correlate against. The spread figures
stand; the correlation does not.

**`cw-2026-08-17-013347` returns a likelihood ratio of 17.2 million**, which is
`001520`'s quadrillions problem in a second capture. Parked, raised once.

## 4. What's blocking us

**Six axis families have now failed, and the sixth failed backwards.**

Ruling asked for:

> **Per-bin admission has now been measured against six families — separation,
> the dah/dit ratio, level spread, lift over the band floor, quantisation
> residual, and agreement between fitted units — and the sixth is inverted: a
> recording holding nothing agrees to a coefficient of variation of 0.028 while
> one reading `VA3VRR` agrees to 0.400. The question returns as a design one.
> Task 1 has shipped the operator a way round it in the meantime.**

*Rejected: agreement as a second condition on the held peak.* Measured across ten
captures. It does not separate and it is the wrong way up.

---

**A defect in the quantisation statistic itself, which affects how the last two
units' numbers should be read.**

The unit search is bounded below at `ShortestDitMs`, 25 ms, and noise runs pile up
at the de-glitch floor of 20 to 30 ms. So the residual is minimised at the search
bound, and almost every fitted unit across ten captures is 25 or 30. **Any future
use of this statistic must exclude the boundary, or the answer is about the bound
rather than the signal.**

Units 1.11.19 and 1.11.20 reported *residuals* rather than fitted units, so their
conclusions are unaffected — but the fitted unit was never examined until today,
and it should have been the first thing checked.

---

**One capture is now pointed within seven hertz of its station and still reads
nothing, which is new information.**

`cw-2026-08-22-014113` asserts at 600 Hz against a station at 607 and emits zero
characters, zero elements. Admission is bypassed, the pitch is right, and nothing
comes out — so on that capture the fault is **downstream of both**, in the
probabilistic decoder's own gate. `cw-2026-08-26-125941` is 46 Hz out and also
reads nothing, so it is not yet a clean case.

*Not proposed, because it needs a ruling:* the decoder's gate is 1.40 and unit
1.11.18 measured every likelihood ratio on these captures below it, peaking at
1.2. Whether an asserted pitch should also relax that gate — the operator having
already supplied the evidence a station is there — is the same question this
unit's first ruling answered for admission, one layer further down. **It touches
what the display asserts, so it is Tim's without exception** (§12.1).

---

**Two files were swept into a commit by `git add -A` and removed in the next
one.** An editor backup of `CLAUDE_CODE.md` that had sat untracked all day, and a
scratch probe. Both are out of the tree and the backup pattern is now ignored.
Recorded because a commit that adds files nobody chose is worth noticing.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Eighteen inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's rulings of 2026-08-25/26, including the two this unit acts under.**
5. **The tone tracker** — six families measured; the question is now a design one.
6. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named** — the operator's own item
   five, and the last of his list not yet attempted.
10. **The keying meter** — its measurement found a station its verdict denied.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **The gate opens on everything, including two empty recordings** (1.11.18).
13. **Pooling versus a held peak** (1.11.20) — **half closed**: the held peak's
    guard was to be agreement, and agreement is measured dead. The pooling half
    stands.
14. **Nineteen runs a pass is not enough evidence for a fit** (1.11.20).
15. **Agreement is inverted**, above — the headline ask.
16. **The quantisation statistic's unit search is biased to its own lower
    bound**, above.
17. **An asserted pitch does not relax the decoder's own gate**, above.
18. **`013347` returns a likelihood ratio of 17.2 million**, joining `001520`'s
    quadrillions. Parked, raised once.

New this unit: **agreement is inverted**, above; **the statistic's search bound
is an attractor**, above; **a capture pointed within seven hertz still reads
nothing**, above.

Closed this unit: **whether the operator may assert a station** — shipped, and it
reads thirty-five characters on a capture that read none. **Whether good passes
agree on a unit** — measured, and they do not.

Still open: **the lock's mixed help**; **three fixtures at accepted cost**; **the
reference and port integrator difference**; **an unmeasured pitch costs `N4L`**;
**the six-hertz window disagreement**; **the short-character bias**; **the
Avalonia geometry offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.21**; **the
whole-file second pass**; **the squelch has no axis**; **the three morning
captures of 2026-08-26**; **seven timing intermittents, four of which fired
today**.

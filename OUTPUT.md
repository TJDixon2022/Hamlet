# Work instruction 020 — the survey never admits the station

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. Branch `main` throughout, three commits, all
pushed, none refused. Version 1.11.16 to 1.11.17 per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

**All four tasks ran. Nothing was dropped.** Task 1 shipped the instrument. Task
2 took the second branch the order allows and ships nothing. Task 3 corrects a
shipped verdict's grounds without changing it. Task 4 measured.

### Task 1's sentence, which is the unit's whole purpose

> **Separation refuses all four stations, short by 1.8 to 2.3 against a bar of
> four — and it refuses the two recordings that hold nothing by the same amount,
> at 1.72 and 1.65.**

Measured per bin, per pass, with every test's value recorded:

| capture | holds | refusing test at the station's pitch | median | short by |
|---|---|---|---|---|
| `cw-2026-08-25-012823` at 500 Hz | a station | **separation**, 32 of 57 passes | 2.32 | **1.77** |
| `cw-2026-08-22-014113` at 600 Hz | a station | **separation**, 28 of 57 | 1.75 | **2.30** |
| `cw-2026-08-22-014308` at 625 Hz | a station | **ratio**, 23 of 57 | 4.24 | 0.44 |
| `cw-2026-08-26-125941` at 400 Hz | a station | **separation**, 38 of 57 | 1.81 | **2.19** |
| **`cw-2026-08-20-014854` at 600 Hz** | **nothing** | **separation**, 25 of 57 | **1.72** | **2.28** |
| **`cw-2026-08-20-014935` at 825 Hz** | **nothing** | **separation**, 26 of 57 | **1.68** | **2.35** |

**The station and the empty band produce the same number.** 1.75 at a station
the operator can hear; 1.72 on a recording of nothing at all.

### Task 2 — the second branch, and nothing ships

The order allows two outcomes: a threshold a real station misses narrowly, or a
test measuring the wrong quantity. **The numbers gave a third, and it points the
same way as the second.**

**No bound on separation admits the stations and refuses the noise.** Swept:

| bound | failing stations admitted at their pitch | silence controls leaking |
|---|---|---|
| **4.0 (today)** | 2 of 4 | `014935` — **1 bin** |
| 3.5 | 3 of 4 | `014854` 1 bin, `014935` 3 bins |
| 3.0 | **4 of 4** | `014854` **9 bins**, `014935` **8 bins** |
| 2.5 | 4 of 4 | 23 and 31 bins |
| 2.0 | 4 of 4 | 116 and 111 bins |

The four stations' **best** separation at their own pitches is 3.82, 3.03, 5.87
and 7.02. The two recordings holding nothing reach **3.58 and 4.92** somewhere in
the band — **higher than three of those four stations.** The distributions
interleave; there is no line to draw.

**The statistic is not the fault. What it is fed is.** Counting what the gate
hands the tests:

| capture | bins producing 0 marks | producing 8+ | median where the gate opened |
|---|---|---|---|
| **`cw-2026-08-17-013347`, reads `VA3VRR`** | **926 of 1,425** | 442 | 13 |
| `cw-2026-08-17-134712`, reads `N4L` | 0 | 1,176 | 16 |
| `cw-2026-08-25-012823` | 0 | 1,417 | 19 |
| `cw-2026-08-22-014113` | 0 | 1,419 | 19 |
| `cw-2026-08-22-014308` | 0 | 1,366 | 19 |
| `cw-2026-08-26-125941` | 0 | 1,420 | 20 |
| **`cw-2026-08-20-014854`, nothing** | **0** | 1,399 | 19 |
| **`cw-2026-08-20-014935`, nothing** | **0** | 1,409 | 20 |

**On seven of eight captures, every single bin in the band produces marks** —
about nineteen a pass, in bins holding nothing but noise. The gate's threshold
comes from each bin's own two levels, so a bin of pure noise has its noise cut in
half and yields a stream of structureless marks. Separation then correctly
reports a continuum, about 1.7, for the noise **and for the station**, because on
those captures it is looking at the same thing in both.

**`013347` is the exception and it is the one that reads a callsign.** Its gate
stays shut on 926 bins, and where it opens at 600 Hz it yields 12 marks with dit
92 ms, dah 275 ms and separation 5.92 — clean, first time, admitted 12 passes of
57.

**So nothing ships, on the order's own instruction.** Moving `MinimumSeparation`
to admit `014113` at 1.75 admits `014854`'s noise at 1.72, which is HM-DEC-120
broken on the order's own control. The measurement is written into the constant's
doc so the number cannot be re-opened without re-opening the threshold the marks
are cut at.

**One improvement to the instrument shipped with it**: separation is now computed
on the ratio-refusal path too, when a caller is watching. A refusal there used to
return before the scatter was computed, so the one statistic that decides keying
from noise was absent from exactly the rows a reader most wants to compare. It
costs nothing in production, where the sink is null.

### Task 3 — the valve verdict stands, and its stated grounds were partly wrong

Unit 1.11.15 rejected its admission valve reporting that it *"admits noise more
often than it admits the station"*, citing 6, 9, 13 and 6 admissions on four
captures it had been told were empty. **Two of those four hold stations.**
Re-run with them treated as what they are:

| capture | holds | valve admissions | at the station's own bins |
|---|---|---|---|
| `cw-2026-08-26-125941` | a station | 6 | 1 |
| `cw-2026-08-22-014113` | a station | 13 | 5 |
| `cw-2026-08-22-014308` | a station | 6 | 2 |
| **`cw-2026-08-17-013347`** | **anchored, reads `VA3VRR`** | **3** | **0** |
| `cw-2026-08-17-134712` | anchored, reads `N4L` | 32 | 7 |
| **`cw-2026-08-20-014854`** | **nothing** | **5** | 1 |
| **`cw-2026-08-20-014935`** | **nothing** | **8** | 2 |

**The verdict does not change: the valve admits 5 and 8 candidates on recordings
holding nothing, and HM-DEC-120 is absolute.** What changes is the sentence
around it. On correct evidence the valve admits noise *comparably* to stations
rather than more often, and 1.11.15's claim that 13 and 6 were noise admissions
was wrong — those were stations, and 5 of the 13 were at the station's own bins.

**And a fact that unit could not have seen**: the valve gives `013347` — the one
capture in this corpus that reads an adjudicated callsign — three admissions and
**none at the station's own pitch**. It would not have helped the case that
already works, while admitting 5 to 8 noise bins per silence control.

The rejection was right. The reasoning is now correct as well as the conclusion.

### Task 4 — what the four read, with nothing shipped

| capture | floor | characters | unsure | elements | pitch | measured |
|---|---|---|---|---|---|---|
| `cw-2026-08-25-012823` | 0 | 41 | 15 | 62 | 450.0 Hz | yes |
| `cw-2026-08-22-014113` | 0 | **0** | 0 | 0 | 600.0 Hz | **no** |
| `cw-2026-08-22-014308` | 0 | **0** | 0 | 0 | 575.0 Hz | **no** |
| `cw-2026-08-26-125941` | 0 | **0** | 0 | 0 | 400.0 Hz | **no** |

Unchanged, because nothing shipped. Three of the four still never obtain a
measured pitch at all; `012823` obtains one and it is the wrong station, at 450
rather than 500.

### The suite

| | baseline | end |
|---|---|---|
| engine | 28 failing of 1841 | **29 failing of 1841** |
| app | 503 passing | **503 passing, 0 failing** |

**The extra is a sixth intermittent and it is not this unit's.**
`Rig.CivRigTests.KnobTurn_TransceiveFrame_RaisesFrequencyChanged` failed once in
a full run and **passed three times out of three alone**. It is in the rig path;
this unit touched `CwToneSurvey` and `CwToneTracker` and nothing else. The
mid-unit baseline, taken with the instrument already in, was 28 of 1841 with a
byte-identical set.

### Where the instruction and the tree disagree

- **`cw-2026-08-22-014308`'s refusing test is the ratio, not separation.** The
  order predicted one wall; at that capture's own claimed pitch the ratio refuses
  23 passes at a median 4.24 against a ceiling of 3.8, with separation second at
  20 passes. Separation is still the dominant refuser across the whole recording
  (566 against 633 for the ratio), so the unit's premise holds in aggregate.
- **`cw-2026-08-26-125941` is admitted twice**, at 400 Hz with separation 7.02.
  The order describes it as never admitted. Two passes in 57 is not enough to
  confirm and the recording still reads nothing, but the claim of zero is wrong.
- **`cw-2026-08-25-012823` is admitted once at 500 Hz** — the real station — in
  57 passes. Unit 1.11.16 reported zero from the tracker's filtered verdict; the
  per-bin instrument sees one. **It does not change that unit's conclusion**,
  since one admission cannot confirm and confirmation needs two.
- **The `Clusters` test never refuses anything**, on any of the eight captures.
  It is the first of the seven and fired zero times in 11,400 bin readings. Every
  bin in every recording has two levels to find, which is the same finding as the
  gate-opens-everywhere table above, seen from the other end.
- **The baseline was 28 of 1841**, exactly as the order states.
- **`CLAUDE_CODE.md` §8 does say four sections**; its version line still reads
  1.3.
- **`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150**, nor
  Tim's rulings of 2026-08-25/26 including the one this unit acted under. This
  unit worked directly on HM-DEC-095's own tests and could not read its record.

## 2. What Tim sees at the radio

**A station he can hear still does not reach the decoder, and this unit says for
the first time exactly why.**

Nothing changed at the radio. What changed is that the refusal is no longer
invisible: every bin the survey looks at now records what each of its seven tests
measured and which one said no. Four days of work were spent building mechanisms
downstream of a decision nobody could see, and that is over.

**The answer is not a threshold that needs nudging.** On a recording holding
nothing but noise, every bin in the band produces about nineteen marks a pass,
because the gate's threshold is derived from each bin's own levels and cheerfully
cuts noise in half. The test that is supposed to tell keying from noise then sees
a structureless continuum in both, and says so — correctly — with the same number.

**Loosening it would let noise through.** Measured: the bound that admits all
four stations lets nine and eight noise bins through on the two recordings that
hold nothing. That is the one property this project has never traded.

**What will look wrong and is not:**

- **A commit that changes no behaviour, and a constant with a long new comment.**
  That is what a measured negative looks like in the tree.
- **The engine shows 29 red where it has shown 28 for six units.** The extra is a
  rig timing test that passes alone; it is named above and is not this unit's.
- **`cw-2026-08-25-012823` still shows 41 characters of soup.** Nothing shipped,
  so nothing moved.

## 3. What you should see

**Which test refuses a real station, and by how much: separation, short by 1.8 to
2.3 against a bar of four — and it refuses pure noise by the same amount, 1.72
and 1.68.**

**Does a station the operator can hear reach the decoder? No.** Three of the four
never obtain a measured pitch at all. The fourth obtains one and it is the wrong
station, 450 Hz where the sender is at 500.

**The bound sweep, which is why nothing shipped:**

| bound | stations taken | noise bins leaked |
|---|---|---|
| **4.0** | 2 of 4 | **1** |
| 3.0 | **4 of 4** | **17** |
| 2.0 | 4 of 4 | **227** |

**The gate, which is the actual fault:**

| | bins producing no marks |
|---|---|
| `cw-2026-08-17-013347`, reads `VA3VRR` | **926 of 1,425** |
| every other capture measured, stations and noise alike | **0** |

**The valve, on correct evidence**: still rejected, at 5 and 8 admissions on
recordings holding nothing — and it gives the one capture that reads a callsign
three admissions, none at its station's pitch.

**The suite**: engine 29 failing of 1841 with the extra named and shown to pass
alone; app 503 of 503.

## 4. What's blocking us

**The fault is located, it is one level above every test that has been examined,
and fixing it is a ruling rather than a session's change.**

Ruling asked for:

> **The gate's threshold is derived from each bin's own two levels, and in a bin
> holding only noise that splits the noise in half and manufactures marks. The
> threshold must be derived from something that knows the difference — the band's
> own noise floor, which `_bandNoise` already computes for the lift, or a
> requirement that a bin's two levels be far enough apart to be two things. Every
> admission test downstream is reasoning about marks cut out of nothing.**

The evidence is in section 1. On seven of eight captures **not one bin in the
band produces zero marks**, including both recordings that hold nothing, and the
median is nineteen a pass. On `cw-2026-08-17-013347`, the one capture that reads
an adjudicated callsign, **926 of 1,425 bin readings produce no marks at all** —
and that capture is also the only one whose station is admitted repeatedly, 12
passes of 57, first time, separation 5.92.

*Rejected: moving `MinimumSeparation`.* Measured across eight captures and swept
across six bounds. There is no value that admits the four stations and refuses
the two controls, because the controls' best bins reach 3.58 and 4.92 while three
of the four stations' best reach only 3.82, 3.03 and 5.87.

*Rejected: moving the ratio band.* The order forbade it and the numbers do not
ask for it. The ratio is the dominant refuser on only one of the four
(`014308`, by 0.44), and separation refuses that capture 20 further passes.

*Not proposed, because it needs a ruling:* the two candidate derivations above,
and unit 1.11.15's finding that the survey's ten-millisecond history hop cannot
resolve a 31 ms dit. **All three touch the instrument this project measures
everything else with, and HM-DEC-119 is explicit about that.** It is a work unit
with a corpus re-measurement in it.

---

**A sixth intermittent, and the count in the orders is stale again.**

`Rig.CivRigTests.KnobTurn_TransceiveFrame_RaisesFrequencyChanged` joins the five
already known. It failed once in a full run and passed three of three alone, in a
path this unit never touched. **Six tests that fail on timing rather than on
behaviour means a full-run count of 28 is no longer a number anyone can read
without diffing which tests moved**, which this report had to do. Worth its own
small unit.

---

**Three claims in the order were slightly wrong and one of them matters.**

`cw-2026-08-26-125941` is admitted twice, not never; `cw-2026-08-25-012823` is
admitted once at 500 Hz, not never; and `cw-2026-08-22-014308` is refused by the
ratio at its own pitch rather than by separation. **None changes the unit's
conclusion** — one or two admissions in 57 passes cannot confirm a station, and
confirmation needs two agreeing surveys. Recorded because the order asked, and
because "zero admissions in thirty seconds" was the unit's own headline number
and it is not exactly true.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Fourteen inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's rulings of 2026-08-25/26, including the one this unit acted under.**
5. **The tone tracker** — admission is now diagnosed and the ask is the gate
   threshold, above; confirmation, displacement and selection stay measured inert
   until it works.
6. **The integrator width** — settled at 45 Hz, with the caveat that the peak is
   sharp and may reflect what the decoder was fitted around.
7. **The guard's gap is two to one**, calibrated on two empty captures; the
   operator's noise session crossed it live on 2026-08-26.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named** — the operator's own item
   five, and the last of his list not yet attempted.
10. **The keying meter** — its measurement found a station its verdict denied.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **The gate opens on noise in every bin** — this unit's headline, above.
13. **A sixth intermittent**, above. The five-count in the orders is stale.
14. **`014113`/`014308` were mislabelled as silence controls in a shipped
    order** — **closed this unit**: the verdict that rested on it is re-measured
    and unchanged, with its grounds corrected.

New this unit: **the gate opens on noise in every bin, and that is why every
admission test sees a continuum**, above; **no bound on separation separates
stations from silence**, above; **a sixth intermittent**, above; **three small
factual corrections to the order**, above.

Closed this unit: **which test refuses a real station and by how much** — the
instrument exists and the answer is separation, by 1.8 to 2.3, with noise at the
same value. **Unit 1.11.15's valve verdict** — re-measured on correct evidence,
unchanged, grounds corrected.

Still open: **the lock's mixed help**; **the "Hold this pitch" button**; **three
fixtures at accepted cost**; **`001520`'s quadrillions**; **the reference and
port integrator difference**; **`CLAUDE_CODE.md`'s version line, still 1.3**;
**an unmeasured pitch costs `N4L`**; **the six-hertz window disagreement**; **the
short-character bias**; **the Avalonia geometry offset, still unexplained**;
**`CHANGELOG.md` at 1.9.0 against 1.11.17**; **the whole-file second pass**;
**the squelch has no axis**; **the three morning captures of 2026-08-26**; **the
speed ceiling may be short for a 36–43 WPM station**.

# Work instruction 019 — the audible station that reads nothing, and the station left mid-contact

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. Branch `main` throughout, three commits, all
pushed, none refused. Version 1.11.15 to 1.11.16 per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

**All four tasks ran. Nothing was dropped.** Task 2 and task 3 both ship
nothing, each on the ruling's own stated condition. Task 4 is void.

### The headline: two questions, one wall

The unit was written as two independent investigations. **They are one.** Both
mechanisms were built, both were swept, and neither reaches the fault — because
in both cases `CwToneSurvey` never admits the station as keying in the first
place, so nothing downstream ever runs.

- On `cw-2026-08-22-014113`, **no bin is admitted as keying at all**, in any
  survey, in thirty seconds.
- On `cw-2026-08-25-012823`, **500 Hz is never admitted even once**, while the
  survey names it the strongest thing in the band over and over.

This is the third consecutive unit to arrive here from a different direction:
1.11.15 found it on `cw-2026-08-26-125941`, task 2 found it through the
integrator, task 3 found it through confirmation.

### Where the instruction and the tree disagree

- **`014113` and `014308` are described here as carrying a station, and unit
  018's order listed them among "the empty captures"** used as noise controls.
  Both cannot be right. **The tree sides with this order**: the only two
  recordings any silence test names are `cw-2026-08-20-014854` and `-014935`,
  and neither `014113` nor `014308` appears in `NothingIsReadFromAudioWithNoKeying`
  or anywhere else as a silence control. Worth a ruling, because unit 018
  reported a valve as unsafe partly on their evidence.
- **The dates are wrong in the order**: they are `cw-2026-08-22-014113` and
  `-014308`, not `08-20`. Minor, but the order names them repeatedly.
- **The keying is 16.7 and 15.7 dB of swing, not nineteen**, at 600 and 625 Hz
  rather than 607. Measured with `KeyingEnvelope.Best`. Close enough that the
  premise stands; recorded because the work turned on those pitches.
- **The baseline was 28 failing of 1841, not 1831** — unit 1.11.15 added ten
  tests and did not move the failure count. Byte-identical set.
- **A sweep harness already existed**: `TheIntegratorBandwidthTable` carries the
  exact four widths this order names. It measures likelihood ratios through the
  offline envelope, not characters through the decoder, which is why task 2
  needed a new seam rather than a new table.
- **`CLAUDE_CODE.md` §8 does say four sections**; its version line still reads
  1.3.
- **`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150** nor
  Tim's rulings of 2026-08-25/26. Task 3 worked directly beside HM-DEC-095's
  confirmation constant and could not read its full record.

### Task 1 — the one-smear finding is refuted

Unit 1.11.6's finding was that on a recording that reads, the envelope's upper
quartile sits near its 97th percentile, and on these two it sits at a third of
it. **Measured in the tree today, with two anchored captures added as controls
that unit did not have:**

| capture | Q75/P97 | reads? |
|---|---|---|
| `cw-2026-08-22-014113` | 0.423 | **no** |
| `cw-2026-08-22-014308` | 0.577 | **no** |
| `cw-2026-08-18-004507` | 0.937 | yes |
| **`cw-2026-08-17-013347`** | **0.448** | **yes — `VA3VRR`, adjudicated** |
| **`cw-2026-08-17-134712`** | **0.238** | **yes — `N4L`, adjudicated** |

**Two recordings that read adjudicated callsigns sit at or below the two that
read nothing.** `N4L` reads at 0.238, well under both. So Q75/P97 does not
separate a readable station from an unreadable one, and the finding held only
because `004507` was its single control. The figure is real; it is not a
diagnosis.

**And it moved nothing.** Unit 1.11.15 shipped the release-on-QSY, which fires
only when the dial moves, and no capture test moves a dial. `014113`, `014308`
and `012823` are character-for-character what they were. The tasks below were
not re-aimed.

### Task 2 — the width is not the cause, and nothing ships

**The arithmetic, so the trade is legible:**

| width | integrator span | 18 WPM | 24 WPM | 30 WPM | 36 WPM |
|---|---|---|---|---|---|
| 20 Hz | 75.0 ms | 113% of a dit | 150% | 188% | 225% |
| 30 Hz | 50.0 ms | 75% | 100% | 125% | 150% |
| **45 Hz** | **33.4 ms** | **50%** | **67%** | **83%** | **100%** |
| 60 Hz | 25.0 ms | 38% | 50% | 63% | 75% |
| 90 Hz | 16.7 ms | 25% | 33% | 42% | 50% |
| 120 Hz | 12.5 ms | 19% | 25% | 31% | 38% |

The arithmetic does say a 24 WPM dit needs more than 60 Hz, so the sweep went to
90 and 120 as the order allows.

**Both captures emit zero characters at every width, at both the measured pitch
and the order's own 607 Hz.**

| | 20 | 30 | 45 | 60 | 90 | 120 Hz |
|---|---|---|---|---|---|---|
| `014113` characters | 0 | 0 | 0 | 0 | 0 | 0 |
| `014113` Q75/P97 | 0.403 | 0.395 | 0.423 | 0.460 | 0.527 | 0.574 |
| `014308` characters | 0 | 0 | 0 | 0 | 0 | 0 |
| `014308` Q75/P97 | 0.408 | 0.531 | 0.577 | 0.602 | 0.614 | 0.630 |

Widening does improve the two-state shape, monotonically and by a lot — 0.40 to
0.63. **It does not recover one character**, and the shape never approaches the
control's 0.83 to 0.95.

**The decisive test bypassed the tracker entirely** and handed the offline
decoder a pitch outright, at 500, 575, 600, 607, 625 and 825 Hz across three
widths. Every reading on both captures is **below the gate of 1.40** — the best
is 1.2, at 625 Hz and 45 Hz width. The one exception is `014308` at 825 Hz,
which returns 196 to 223 characters of `E## ## E ## E # #EE E AT E IEEN` — soup
from a bin nobody claims holds a station, and exactly what the gate exists to
refuse.

**So the smear is not the filter's doing.** The candidate cause named on
2026-08-24 is retired.

**And the sweep settled the width itself, which had been live since 1.11.7:**

| width | anchors held | anchored characters |
|---|---|---|
| 20 Hz | 3/12 | 14 |
| 30 Hz | 6/12 | 96 |
| **45 Hz** | **12/12** | **153** |
| 60 Hz | 2/12 | 24 |
| 90 Hz | 5/12 | 63 |
| 120 Hz | 4/12 | 32 |

**45 Hz is the only width that holds all twelve.** One step away, 60 Hz loses
ten of twelve. Silence held on both empty captures at every width.

**That peak is suspiciously sharp and the report should say so.** A smoothing
parameter whose neighbours cost ten of twelve anchors is not obviously a
physical optimum. The gate at 1.40, the character margin at 1.0 and the clock
fit were all measured with 45 Hz in place, so 45 Hz plausibly wins partly
because everything downstream was fitted around it. **What the sweep proves is
that 45 Hz is the width this decoder was built for, not that it is the best
width available to a decoder built differently.**

### Task 3 — the confirmation window, built, swept, and shipped as nothing

Confirmation now asks whether any of the last *n* surveys agrees within
`ConfirmWithinHz`, rather than the immediately previous one alone. At `n = 2` it
is byte-identical to what was there. Swept at 3, 4, 6 and 8 surveys, which is
one and a half to four seconds:

| window | captures whose acquisition moved | anchors held | `012823` ends at |
|---|---|---|---|
| 2 (today) | — | **12/12** | 450 Hz |
| 3 (1.5 s) | **16 of 37** | 11/12 | **625 Hz** |
| 4 (2.0 s) | **20 of 37** | 10/12 | 625 Hz |
| 6 (3.0 s) | **20 of 37** | 9/12 | 625 Hz |
| 8 (4.0 s) | **20 of 37** | 9/12 | 625 Hz |

The order allows **none** to move and requires all twelve anchors. Silence held
on both empty captures at every length.

**And it did not fix the capture it was ruled for.** At no window does `012823`
confirm 500 Hz — it moves *further* away, ending at 625 rather than 450, with
the decode character-for-character unchanged at 41.

**The premise was wrong, and measuring why is the unit's most useful result.**
The ruling supposed an intermittently-admitted station alternates 500, 450, 500,
450 and so never finds a consecutive pair. Logged survey by survey:

```
 3.03s  keyed  none  strongest   500  tracked 500
13.53s  keyed   450  strongest  none  tracked 500
14.03s  keyed  none  strongest   500  tracked 500
14.53s  keyed  none  strongest   500  tracked 450*
```

**450 Hz is the only bin ever admitted. 500 Hz is admitted zero times in thirty
seconds**, while the survey names it `Strongest` at 3.03, 14.03 and 14.53
seconds. There is no alternation. **A window cannot help a candidate that is
never nominated.**

The tracker rides 500 for eleven seconds on the cold-start "point at the loudest
thing" path, which is why it looks confirmed and is not — and is why
HM-DEC-127's displacement guard is inert, exactly as unit 1.11.11 diagnosed.

**What the window does to the two W1AW captures**, which the order asked about
either way: `cw-2026-08-22-031905` is **identical at every window** — it still
wanders 500, 300, 500, 300 and ends at 300. `cw-2026-08-22-032113` gains eight
characters but **loses its anchor** at every window past 2, and its text goes
from `A KET■ A N O INT ERNE T` to `A KET, ■ E ■ I I I E EI II H TI`, which is
more soup rather than less. Neither is recovered.

The measurement is written into `ConfirmWithinSurveys`' own doc comment so the
number cannot be re-opened without re-opening the admission it depends on.

### Task 4 — void

Unit 1.11.15 logged `marginLlr / spanLlr` as `CwCharacter.MarginShareForRecord`
and reported its first distribution: 1,583 characters, whole range −20.1 to
+2.45 against the raw margin's 2.98 × 10⁸, medians 0.004 anchored and 0.005
everything else. The task says it is void in that case, and it is.

### The suite

| | baseline | end |
|---|---|---|
| engine | 28 failing of 1841 | **28 failing of 1841** |
| app | 503 passing | **503 passing, 0 failing** |

Byte-identical failure set for a fifth unit. The mid-unit baseline read 28 of
1847; the six extra were scratch probes, since removed.

## 2. What Tim sees at the radio

**Nothing changes at the radio, and that is the correct outcome of this unit.**

Two mechanisms were built and both were measured before shipping. Neither
recovered a character on the captures they were built for, and both cost
something real elsewhere. Under the rulings' own conditions, neither ships.

**Fast senders that used to produce nothing still produce nothing.** The
integrator is not why. Widening it from 33 ms to 12.5 ms — a quarter of a 24 WPM
dit — does not recover one character on either capture, at any pitch, including
with the tracker taken out of the loop entirely.

**Contacts that rot halfway through still rot.** `cw-2026-08-25-012823` still
leaves the right station at fourteen seconds. The confirmation rule was not what
stopped it: the survey never nominated 500 Hz at all.

**What did change is that two long-open questions are now closed with numbers.**
The integrator width has been live since 2026-08-24 and is settled at 45 Hz on
measurement rather than inheritance. The confirmation window is settled at two
with the whole sweep recorded at the constant.

**What will look wrong and is not:**

- **Two commits that change no behaviour.** The constructor now takes two
  optional knobs nothing in the application passes, and a constant gained a long
  comment. That is what a measured negative looks like in the tree.
- **`ConfirmWithinSurveys` exists and is set to its old value.** The mechanism is
  real and its default is deliberate; the doc says why.
- **The engine still shows 28 red.** Same set as the last five units.

## 3. What you should see

**Do `cw-2026-08-22-014113` and `-014308` read? No — and not because of the
filter.** Zero characters at 20, 30, 45, 60, 90 and 120 Hz, at 600, 607 and 625
Hz, and zero with the tracker bypassed and the pitch handed over outright, where
every likelihood ratio lands under the gate of 1.40. `014113` has **no bin
admitted as keying at all** in thirty seconds.

**Does `cw-2026-08-25-012823` hold its station end to end? No.** It reaches 500
Hz at three seconds, rides it unconfirmed for eleven and a half, and leaves at
fourteen and a half. No confirmation window changes that, because **500 Hz is
never admitted as keying even once** while being named the strongest signal in
the band repeatedly.

**The two sweeps:**

| integrator | 20 | 30 | **45** | 60 | 90 | 120 Hz |
|---|---|---|---|---|---|---|
| anchors held | 3/12 | 6/12 | **12/12** | 2/12 | 5/12 | 4/12 |
| `014113` characters | 0 | 0 | **0** | 0 | 0 | 0 |

| window | **2** | 3 | 4 | 6 | 8 |
|---|---|---|---|---|---|
| acquisitions moved of 37 | **0** | 16 | 20 | 20 | 20 |
| anchors held | **12/12** | 11 | 10 | 9 | 9 |

**The refuted control:** `N4L` reads at Q75/P97 = 0.238 and `VA3VRR` at 0.448,
both at or below the two captures that read nothing.

**The suite**: engine 28 failing of 1841, unchanged; app 503 of 503.

## 4. What's blocking us

**Three units have now converged on one component from three directions, and it
has never been the subject of a work order.**

Ruling asked for:

> **The next unit is `CwToneSurvey.Judge` and nothing else. Admission is the
> wall: on `cw-2026-08-22-014113` no bin is admitted as keying in thirty
> seconds, on `cw-2026-08-25-012823` the station's own pitch is admitted zero
> times while being named the strongest signal repeatedly, and on
> `cw-2026-08-26-125941` a 31 ms dit is measured as 45. Everything downstream —
> the integrator, the confirmation rule, the gate, the displacement guard — is
> reasoning about a candidate that was never nominated.**

The evidence is in section 1. Three separate mechanisms were proposed by three
consecutive orders, all built, all measured, and none reaches the fault:

| unit | mechanism | result |
|---|---|---|
| 1.11.15 | the admission valve | admits noise more often than the station |
| 1.11.16 | the integrator width | zero characters at every width |
| 1.11.16 | the confirmation window | the pitch is never nominated to confirm |

*Rejected: another downstream mechanism.* The pattern is now three for three and
the reason is structural rather than a coincidence of tuning.

*Not proposed, because it needs a ruling first:* unit 1.11.15 measured that the
survey's history hop is ten milliseconds and cannot resolve a 31 ms dit, and
that its 6 dB hysteresis eats a fixed amount off every mark regardless of
length. Both are changes to the instrument this project measures everything else
with, and HM-DEC-119 is explicit about that. **It is a work unit with a corpus
re-measurement in it.**

---

**Two orders disagree about what `014113` and `014308` hold, and one of them was
used as evidence.**

Unit 018's order named them among "the empty captures" and required silence on
them; this order says they carry a station at 607 Hz. **The tree sides with this
order** — no silence test names either file, and the only two it names are
`014854` and `014935`. But unit 018 reported the admission valve as unsafe
partly because it "admitted noise" on `014113` and `014308`, and if those hold a
station then thirteen and six admissions there were **not** noise admissions.

**The valve's refusal stands on the other two captures alone**, which is 6 and 9
admissions against 6 on the capture that holds a station — still a refusal, and
a much narrower one than reported. Worth a ruling on what those two files hold,
because it changes what unit 018 measured.

---

**The 45 Hz optimum is sharp enough to be suspicious, and the report should not
launder that.**

45 Hz holds 12 of 12 anchors and 60 Hz holds 2. A smoothing parameter does not
usually have a cliff one step wide. The likely explanation is that the gate, the
character margin and the clock fit were all measured with 45 Hz in place, so the
sweep is measuring how well each width suits constants fitted around one of
them. **The width is settled for this decoder and is not established as
optimal**, and a future change to the gate or the margin re-opens it.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Sixteen inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150, nor for
   Tim's rulings of 2026-08-25 and 2026-08-26.**
5. **The tone tracker** — the confirmation-rule ask is now answered by task 3
   and replaced by the admission ask above; fist-quality selection is unmeasured.
6. **The integrator width** — **closed this unit.** Settled at 45 Hz by
   measurement, with the caveat above.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named.**
10. **The keying meter** — hidden behind a setting; the rebuild is its own unit.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **The margin does not separate and the reason rules out differences of
    log-likelihoods generally** (2026-08-26, unit 1.11.14). Answered in part by
    1.11.15's quotient, which escapes the scale problem and still does not
    separate.
13. **A fifth intermittent**,
    `Rig.ScopeOutputWriteTests.ConfirmedNeedsTheReadbackToAgree` (2026-08-26).
14. **The three captures of 2026-08-26** — `004808`, `004900`, `004952` — asked
    a fifth time in unit 1.11.15 and still absent. HM-DEC-126 closed an identical
    case after four asks.
15. **The survey's time resolution** (2026-08-26, unit 1.11.15). Now the
    headline ask above, with two more captures behind it.
16. **What `014113` and `014308` hold**, above. New this unit.

New this unit: **the survey's admission is the wall, from three directions**,
above; **two orders disagree about two captures**, above; **the 45 Hz optimum is
sharp enough to be an artifact of what was fitted around it**, above.

Closed this unit: **the one-smear finding**, refuted with two anchored controls
it never had. **The integrator width**, settled at 45 Hz by measurement, open
since 2026-08-24. **The confirmation window**, settled at two with the whole
sweep recorded at the constant.

Still open: **the lock's mixed help**; **the "Hold this pitch" button**; **three
fixtures at accepted cost**; **`001520`'s quadrillions**; **the reference and
port integrator difference**; **`CLAUDE_CODE.md`'s version line, still 1.3**;
**an unmeasured pitch costs `N4L`**; **the six-hertz window disagreement**;
**the short-character bias**; **the Avalonia geometry offset, still
unexplained**; **`CHANGELOG.md` at 1.9.0 against 1.11.16**; **the whole-file
second pass**; **the squelch has no axis**.

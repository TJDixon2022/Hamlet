# Work instruction 022 — Morse is quantised and noise is not

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. Branch `main` throughout, two commits, all
pushed, none refused. Version 1.11.18 to 1.11.19 per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

**Task 1 ran and did not separate, so tasks 2 and 3 were not built** — that is
the order's own instruction, not a shortfall. Task 4, the drop candidate, ran.

### Task 1's sentence

> **The fit residual does not separate. Every recording in this corpus — the four
> stations, the four that read adjudicated callsigns, and the two that hold
> nothing — scores between 0.258 and 0.409, and the two silence controls sit at
> 0.318 and 0.327, inside that range and better than `N4L` at 0.409.**

### Task 1 — the measurement, and a control that makes it decisive

The run stream today's gate produces, at each capture's own pitch, marks and gaps
together, fitted to the single Morse unit that minimises the distance to the
nearest integer multiple:

| capture | holds | runs | unit ms | **residual** | multiples used |
|---|---|---|---|---|---|
| `cw-2026-08-17-013347` | `VA3VRR` | 389 | 93.5 | **0.258** | 1,2,3,4,9 |
| `cw-2026-08-25-012823` | a station | 1,649 | 38.0 | **0.288** | 1–9 |
| `cw-2026-08-26-125941` | a station | 2,179 | 40.0 | **0.288** | 1–9 |
| `cw-2026-08-24-012403` | `DE KD0UN` | 1,673 | 47.0 | **0.299** | 1–9 |
| `cw-2026-08-22-014113` | a station | 1,919 | 41.5 | **0.308** | 1–9 |
| **`cw-2026-08-20-014854`** | **nothing** | 1,474 | 45.0 | **0.318** | 1–9 |
| `cw-2026-08-18-004507` | the bulletin | 1,290 | 58.0 | **0.319** | 1–9 |
| **`cw-2026-08-20-014935`** | **nothing** | 2,110 | 35.0 | **0.327** | 1–9 |
| `cw-2026-08-22-014308` | a station | 1,288 | 65.0 | **0.353** | 1–9 |
| `cw-2026-08-17-134712` | **`N4L`** | 1,638 | 50.0 | **0.409** | 1–9 |

**An empty band beats an adjudicated callsign by nine hundredths.** The two
silence controls sit sixth and eighth of ten. `N4L` — which every amplitude axis
also got wrong — is last.

Across **every bin in the band**, not only the station's own, the best residual
per capture runs 0.222 to 0.307, with the silence controls at **0.239 and
0.233** — mid-pack. **The median number of distinct multiples is 9.0 for every
capture in the list**, station and silence alike, so the hoped-for tell (noise
using only one multiple, Morse using several) is not there either.

**The control that settles it.** A statistic without its null value is not a
measurement, so the same fit was run on streams carrying no information at all:

| stream | n | unit ms | residual |
|---|---|---|---|
| uniform 20–100 ms | 1,600 | 30.5 | **0.230** |
| uniform 20–300 ms | 1,600 | 41.0 | **0.242** |
| uniform 10–60 ms | 1,600 | 26.0 | **0.257** |
| **real Morse, 50 ms unit, no jitter** | 800 | 50.0 | **0.000** |
| real Morse, 5% jitter | 800 | 50.0 | **0.054** |
| real Morse, 15% jitter | 800 | 50.0 | **0.132** |
| real Morse, 30% jitter | 800 | 56.0 | **0.182** |

**The statistic works.** Generated Morse scores nought, and 0.182 even with 30%
jitter on every element. **Random lengths score 0.23 to 0.26.**

**And every real capture in this corpus scores at or worse than random.** The
best — `VA3VRR` at 0.258 — is level with the null. The rest are above it.

**So the gate's run stream contains no recoverable Morse structure at all, on any
recording, including the four that read adjudicated callsigns.** That is a
stronger and more useful result than "the axis failed": it says the stream being
tested is not a description of keying on any capture, so no statistic computed
from it can separate anything.

Baseline recorded by diffing which tests fail: **28 of 1841, byte-identical to
the stable set.**

### Tasks 2 and 3 — not built

The order is explicit: *"If it does not, stop. Report the overlap, build nothing,
and say plainly that five axis families have now failed and per-bin admission
needs a ruling at a level above this unit."*

Nothing was built. The admission tests are untouched.

### Task 4 — the speed-scaled de-glitch

Today's de-glitch is a 3-hop median at a 10 ms survey hop, so it removes runs of
10 ms or less and the shortest surviving run is 20 ms — a dit at 60 words a
minute. What a floor scaled to the tracked dit would remove instead:

| capture | holds | dit ms | at 0.3 dit | at 0.4 dit |
|---|---|---|---|---|
| `cw-2026-08-25-012823` | a station | 30.0 | 0.0% | 0.0% |
| `cw-2026-08-22-014113` | a station | 60.0 | 0.3% | **14.7%** |
| `cw-2026-08-22-014308` | a station | 33.3 | 0.1% | 0.1% |
| `cw-2026-08-26-125941` | a station | 35.3 | 0.3% | 0.3% |
| **`cw-2026-08-20-014854`** | **nothing** | 35.0 | **0.0%** | **0.0%** |
| **`cw-2026-08-20-014935`** | **nothing** | 30.0 | **0.0%** | **0.4%** |
| `cw-2026-08-17-134712` | `N4L` | 47.5 | 1.1% | 1.1% |
| `cw-2026-08-18-004507` | the bulletin | 50.0 | 0.0% | 1.2% |

**It would remove essentially nothing, and no more from the noise than from the
stations.** The arithmetic is why: the tracked dits are 30 to 60 ms, so 0.3 to
0.4 of a dit is 9 to 24 ms, which is at or **below the 20 ms floor the fixed
de-glitch already enforces**. The proposal is a no-op on seven captures of eight
and a loosening on the slowest.

Measured only. Nothing changed.

### The suite

| | baseline | end |
|---|---|---|
| engine | 28 of 1841 (stable set) | **28 of 1841, byte-identical** |
| app | 503 passing | **503 passing, 0 failing** |

No intermittent fired in either run. Diffed rather than totalled, per the order.

### Where the instruction and the tree disagree

- **`fit_clock` and `well_separated` are in `cwdecoder.py` at the repository
  root, not in `tools/reference-decoder/`** — that folder holds `README.md` and
  `reference_decoder.py` and neither function is in it.
- **The precedent is not quite the idea.** `well_separated` is a *scatter*
  test — how far two cluster centres sit apart in their own spread — which is
  the same family as `MinimumSeparation`, already measured and rejected in unit
  1.11.17. The quantisation residual this unit measured is genuinely new to this
  tree, and it is the thing that failed.
- **`cw-2026-08-24-012403` is under `unadjudicated/`**, not beside the other
  anchored captures. Cost one run to find.
- **The baseline was 28, not 29** — no intermittent fired.
- **`CLAUDE_CODE.md` is at 1.4** and §8 specifies four sections, as stated.
- **`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150**, nor
  Tim's rulings of 2026-08-25/26 including the one this unit acted under.

## 2. What Tim sees at the radio

**Nothing changed, and a station he can hear still does not reach the decoder.**

The fifth axis family was measured and it failed like the other four. Under the
order's own terms nothing was built, so the app is exactly as it was.

**But this unit found something the previous four could not.** The four earlier
axes failed by overlapping — a station scoring 1.75 where silence scored 1.72.
This one failed differently, and the difference matters: **the run stream the
survey's gate produces is less consistent with Morse than random numbers are, on
every recording tested, including the ones that decode a callsign correctly.**

That means the survey is not looking at a poor description of the keying. It is
looking at something that is not a description of the keying at all. **Hamlet
reads `VA3VRR` and `N4L` from a completely different measurement** — the
probabilistic path, with its own envelope and its own integrator — and the survey
that decides which pitch to point at has never been reading the same thing.

**So the question is no longer which statistic to use.** It is why per-bin
admission is being asked of a stream that carries nothing, when a stream that
carries the answer already exists a few lines away.

**What will look wrong and is not:**

- **One commit that changes no behaviour**, adding a collection point nothing in
  the application uses. The suite is byte-identical.
- **The engine shows 28 where two of the last three runs showed 29.** No
  intermittent fired. The stable set is unchanged.
- **All four target captures are exactly where they were.** Nothing was built.

## 3. What you should see

**Does the fit residual separate a station from silence? No.**

| | residual |
|---|---|
| **`cw-2026-08-17-134712`, `N4L`, an adjudicated callsign that reads** | **0.409** |
| **`cw-2026-08-20-014854`, holds nothing** | **0.318** |
| **`cw-2026-08-20-014935`, holds nothing** | **0.327** |

**The two recordings of an empty band fit Morse better than the recording of a
callsign Hamlet reads correctly.**

**And the null value that makes it decisive:**

| | residual |
|---|---|
| generated Morse, no jitter | **0.000** |
| generated Morse, 30% jitter | **0.182** |
| **uniform random lengths** | **0.230 – 0.257** |
| **every real capture in the corpus** | **0.258 – 0.409** |

**Every real capture scores at or worse than random.**

**The speed-scaled de-glitch would remove 0.0% to 1.2%**, and no more from the
two silence controls than from the four stations.

**The suite**: engine 28 of 1841, byte-identical to the stable set; app 503 of
503.

## 4. What's blocking us

**Five axis families have now failed, and the reason found today is not that the
fifth was the wrong statistic.**

Ruling asked for:

> **Per-bin admission cannot be made to work from the survey's gate output,
> because that output carries no Morse structure on any recording in this corpus
> — including the four that decode adjudicated callsigns. Measured against its
> own null: generated Morse scores 0.000 and random lengths score 0.23, and every
> real capture scores 0.258 to 0.409. The next unit is not another statistic. It
> is whether the survey should be reading the probabilistic path's envelope —
> the measurement that already recovers `VA3VRR` and `N4L` — instead of a gate
> of its own.**

The evidence is in section 1. The five families, with the unit that measured each:

| axis | unit | how it failed |
|---|---|---|
| cluster separation | 1.11.17 | station 1.75, silence 1.72 |
| dah/dit ratio | 1.11.17 | dominant refuser on one capture only |
| bin level spread | 1.11.18 | `N4L` 10.4, silence 12.0 |
| lift over band floor | 1.11.18 | `N4L` 3.0, silence 35.3 |
| **quantisation residual** | **this unit** | **`N4L` 0.409, silence 0.318 — and every capture at or worse than random** |

*Rejected: a sixth statistic on the same stream.* The null control shows the
stream itself is the problem, not the choice of measure. A statistic that scores
0.000 on generated Morse and 0.23 on random numbers is working correctly, and it
reads the whole corpus as worse than random.

*Not proposed, because it needs a ruling:* the probabilistic path computes an
envelope through a Hann integrator at a known pitch and recovers characters from
it. The survey computes its own gate from raw bin levels and a hysteresis band,
and that is what produces nineteen structureless runs a pass in every bin of
every recording. Whether the survey can be fed the same envelope — and at what
cost, since it must scan the whole band rather than one pitch — is a design
question a level above this unit (HM-DEC-119).

---

**Unit 1.11.18's hop finding and this one point the same way, and together they
are stronger than either.**

That unit found the survey's run lengths are measured to ±32% at the speeds the
operator meets. This one finds the run stream is worse than random against a
quantisation test. A ±32% measurement error on a genuinely quantised signal would
give a residual near 0.16 by the jitter table above — not 0.26 to 0.41. **So
resolution alone does not explain it**, and something is generating runs that are
not elements at all.

---

**Task 4's proposal would be a loosening, not a tightening.**

Scaling the de-glitch to 0.3 or 0.4 of the tracked dit gives 9 to 24 ms at the
speeds in this corpus, and the fixed de-glitch already enforces a 20 ms floor. On
seven captures of eight it removes under 1.2%; on the slowest it would remove
14.7% of a station's own runs while removing 0.0% from one silence control. If
the de-glitch is to be reconsidered it has to go the other way.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Sixteen inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's rulings of 2026-08-25/26, including the one this unit acts under.**
5. **The tone tracker** — five axis families measured; the ask is now whether
   per-bin admission can work at all, above.
6. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
7. **The guard's gap is two to one**, calibrated on two empty captures; the
   operator's noise session crossed it live on 2026-08-26.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named** — the operator's own item
   five, and the last of his list not yet attempted.
10. **The keying meter** — its measurement found a station its verdict denied,
    and on `cw-2026-08-20-014935` it reads 44 words a minute off silence.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **The gate opens on everything, including two empty recordings** (1.11.18).
13. **The de-glitch removes only 10 ms runs at a 10 ms hop** (1.11.18) —
    **closed this unit**: scaling it to the tracked dit would loosen it, not
    tighten it.
14. **The survey's gate output carries no Morse structure on any capture**,
    above — the headline ask.
15. **The hop's ±32% cannot explain a worse-than-random residual**, above.
16. **`fit_clock` is at the repository root, not in `tools/reference-decoder/`**,
    and `well_separated` is a scatter test rather than a quantisation one — the
    precedent the order cited is a different family from the one it asked for.

New this unit: **the quantisation residual fails, and every capture scores at or
worse than random**, above; **the two findings together rule out resolution as
the explanation**, above.

Closed this unit: **whether structure separates** — measured with its own null
control, and it does not. **The speed-scaled de-glitch** — measured, and it would
loosen rather than tighten.

Still open: **the lock's mixed help**; **the "Hold this pitch" button**; **three
fixtures at accepted cost**; **`001520`'s quadrillions**; **the reference and
port integrator difference**; **an unmeasured pitch costs `N4L`**; **the
six-hertz window disagreement**; **the short-character bias**; **the Avalonia
geometry offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.19**; **the whole-file
second pass**; **the squelch has no axis**; **the three morning captures of
2026-08-26**; **six timing intermittents, none of which fired today**.

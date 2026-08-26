# Work instruction 023 — the survey reads the wrong thing

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. Branch `main` throughout, two commits, all
pushed, none refused. Version 1.11.19 to 1.11.20 per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

**Task 1 ran and did not separate, so tasks 2 and 3 were not built** — the
order's own instruction. Task 4, the drop candidate, ran, **and it overturned the
premise both this unit and the last one were built on.**

### Task 1's sentence

> **Stream B scores like stream A. The envelope reads 0.170 to 0.429 where the
> gate reads 0.258 to 0.409, the two silence controls land at 0.305 and 0.287
> inside the stations' range, and only one capture of ten improves.**

### Task 1 — the two streams side by side

Both scored with unit 1.11.19's statistic, unchanged. **B is reported at two
hops** because the envelope's is 5 ms and the survey's is 10, and a finer hop
changes run lengths independently of the signal:

| capture | holds | A runs | **A** | **B at 10 ms** | **B at 5 ms** |
|---|---|---|---|---|---|
| `cw-2026-08-17-013347` | `VA3VRR` | 389 | 0.258 | **0.170** | 0.200 |
| `cw-2026-08-25-012823` | a station | 1,649 | 0.288 | 0.276 | 0.292 |
| `cw-2026-08-18-004507` | the bulletin | 1,290 | 0.319 | 0.286 | 0.294 |
| **`cw-2026-08-20-014935`** | **nothing** | 2,110 | 0.327 | **0.287** | 0.318 |
| `cw-2026-08-26-125941` | a station | 2,179 | 0.288 | 0.296 | 0.324 |
| `cw-2026-08-24-012403` | `DE KD0UN` | 1,673 | 0.299 | 0.301 | 0.329 |
| **`cw-2026-08-20-014854`** | **nothing** | 1,474 | 0.318 | **0.305** | 0.354 |
| `cw-2026-08-22-014113` | a station | 1,919 | 0.308 | 0.326 | 0.366 |
| `cw-2026-08-22-014308` | a station | 1,288 | 0.353 | 0.355 | 0.418 |
| `cw-2026-08-17-134712` | **`N4L`** | 1,638 | 0.409 | **0.401** | 0.429 |

Null values, same run: uniform random **0.231**; generated Morse **0.000**, and
**0.187** at 30% jitter.

**One capture improves and it is the one that was already best.** `013347` falls
from 0.258 to 0.170 — below the random null, into jittered-Morse territory. It is
also the capture with 91 dB of swing and near-digital silence between elements.

**Everything else is unchanged or worse**, and the two silence controls sit at
0.287 and 0.305, better than three of the four stations and far better than
`N4L`, which reads an adjudicated callsign and scores worst in the table on both
streams.

So the envelope is not quantised where the gate is not, and tasks 2 and 3 were
not built.

### Task 4 — and it overturns the premise

The order asked what `N4L` looks like to the decoder. The answer is the most
important number in this unit.

**HM-DEC-144's hand-read elements** — the gate's own output over the 1.56 seconds
that spell the callsign, `225 30 55 | 180 | 55 40 55 40 60 40 55 30 245 | 150 |
60 25 245 40 55 40 55`:

| | |
|---|---|
| elements | 21 |
| fitted unit | 30.5 ms |
| **residual** | **0.173** |
| distinct multiples | 6 |

**0.173 is below the random null of 0.231 and level with Morse at 30% jitter.**

**The same gate, on the same capture, pooled over the whole recording, scores
0.409.** The structure is there. It was being averaged away.

**So the statistic was reading the wrong window, not the wrong stream** — and
that is HM-DEC-090's finding, in a place it was never applied. That ruling
established that the reported ratio and the located pitch were *"averages over
the ninety-six percent of a recording in which a station answering a call is
silent"*, and replaced both with held peaks. **The admission statistics were
never given the same treatment.**

Tested directly, per survey pass rather than pooled:

| capture | holds | pooled | **best pass** | P10 | median | passes under 0.20 |
|---|---|---|---|---|---|---|
| `cw-2026-08-18-004507` | the bulletin | 0.319 | **0.103** | 0.148 | 0.253 | **20 of 57** |
| `cw-2026-08-17-134712` | `N4L` | 0.409 | **0.133** | 0.184 | 0.275 | 10 of 56 |
| `cw-2026-08-26-125941` | a station | 0.288 | **0.146** | 0.179 | 0.240 | 12 of 57 |
| `cw-2026-08-17-013347` | `VA3VRR` | 0.258 | **0.150** | 0.150 | 0.197 | **12 of 18** |
| `cw-2026-08-24-012403` | `DE KD0UN` | 0.299 | **0.150** | 0.207 | 0.252 | **3 of 57** |
| **`cw-2026-08-20-014854`** | **nothing** | 0.318 | **0.152** | 0.204 | 0.241 | **4 of 57** |
| `cw-2026-08-25-012823` | a station | 0.288 | **0.157** | 0.177 | 0.221 | 12 of 57 |
| **`cw-2026-08-20-014935`** | **nothing** | 0.327 | **0.167** | 0.200 | 0.253 | **6 of 57** |
| `cw-2026-08-22-014308` | a station | 0.353 | **0.168** | 0.197 | 0.264 | 6 of 57 |
| `cw-2026-08-22-014113` | a station | 0.308 | **0.188** | 0.198 | 0.228 | 7 of 57 |

**Windowing recovers real structure — and noise recovers just as much.** Every
capture now has passes scoring 0.10 to 0.19, well below the random null. But
`cw-2026-08-24-012403`, which reads `DE KD0UN KD0UN K`, has **three** such passes
while `cw-2026-08-20-014854`, which holds nothing, has **four**.

The reason is sample size: about nineteen runs a pass, so a pass of noise fits a
freely-chosen unit well by chance often enough to match a real one.

**The decoder's own reading of `134712`** confirms the shape of the problem: it
emits 92 characters, of which `N4LQ` is one, and the dit its segmentation implies
runs from 25.0 ms at the tenth percentile to 61.7 at the ninetieth — a spread of
**2.47×** across characters. Over the seconds that decode, the envelope swings
**26.8 dB**.

### The suite

| | baseline | end |
|---|---|---|
| engine | 28 of 1841, identical to the stable set | **29 of 1841** |
| app | 503 passing | **503 passing, 0 failing** |

**The extra is a seventh intermittent and it is not this unit's.**
`Rig.CivRigTests.ARigWhoseReadLoopIsStuckStillDisconnects` failed once in a full
run and **passed three of three alone**. It is in the rig path; **no `src` file
was changed by this unit at all** — the only committed change is a test probe and
the version bump.

### Where the instruction and the tree disagree

- **The premise that the run stream carries nothing is wrong**, and unit
  1.11.19's headline with it. Over the seconds a station is actually sending, the
  gate's own elements score 0.173. Pooling destroyed it.
- **The envelope is not the answer.** It improves exactly one capture of ten, the
  one already cleanest, and leaves the silence controls scoring better than most
  stations.
- **`cw-2026-08-24-012403` was still under `unadjudicated/`** — the correction
  carried into this order was right.
- **`fit_clock` and `well_separated` are in `cwdecoder.py` at the root**, as the
  order states. Confirmed.
- **The baseline was 28, identical to the stable set**, as stated.
- **`CLAUDE_CODE.md` is at 1.4**; §8 specifies four sections.
- **`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150**, nor
  Tim's rulings of 2026-08-25/26.

## 2. What Tim sees at the radio

**Nothing changed, and a station he can hear still does not reach the decoder.**

The envelope was measured against the gate on the same captures at the same
pitches, and it is not better. Under the order's terms nothing was built.

**But the unit found that the last two units were chasing a ghost.** Unit
1.11.19 concluded the gate's run stream carries no Morse structure on any
recording. That was measured over whole recordings, and it is wrong: over the
1.56 seconds where `N4L` is actually being sent, the gate's own elements score
**0.173** — below random, level with Morse at 30% jitter. **The structure was
always there. It was being averaged against twenty-eight seconds of nobody
sending.**

**This project has fixed exactly this mistake once before.** HM-DEC-090 found the
reported SNR and the located pitch were averages over the silence in a recording
and replaced both with held peaks. The admission tests were never given the same
treatment, and they are averaging in exactly the same way.

**What the windowed measurement then shows is a harder problem, honestly.** Score
per pass instead of pooled and every capture produces passes that look like
Morse — including the two that hold nothing. Nineteen runs is not enough evidence
for a fit to mean anything on its own.

**What will look wrong and is not:**

- **One commit that touches no `src` file.** The measurement needed no engine
  change; the run-stream collection point shipped last unit.
- **The engine shows 29 where the baseline showed 28.** A seventh intermittent,
  named above, passing three of three alone.
- **All four target captures are exactly where they were.**

## 3. What you should see

**Stream A and stream B, same captures, same pitches, with the nulls beside
them** — the full tables are in section 1. The three rows that decide it:

| | A (the gate) | B (the envelope, 10 ms) |
|---|---|---|
| **`cw-2026-08-17-134712`, `N4L`, reads a callsign** | **0.409** | **0.401** |
| **`cw-2026-08-20-014854`, holds nothing** | **0.318** | **0.305** |
| **`cw-2026-08-20-014935`, holds nothing** | **0.327** | **0.287** |
| uniform random | — | **0.231** |
| generated Morse, 30% jitter | — | **0.187** |

**Both recordings of an empty band fit Morse better than the recording of a
callsign Hamlet reads, on both streams.**

**And the number that changes the phase:**

| the gate's own elements on `134712` | residual |
|---|---|
| **over the 1.56 s that spell `N4L`** (HM-DEC-144, hand-read) | **0.173** |
| pooled over the whole 30 s recording | **0.409** |

**The suite**: engine 29 of 1841 — the stable 28 plus a seventh intermittent that
passes alone; app 503 of 503.

## 4. What's blocking us

**The last two units' conclusion was an artifact of pooling, and the correction
is a ruling this project has already made once.**

Ruling asked for:

> **An admission statistic averaged over a whole recording is a statistic about
> the silence, not about the station. HM-DEC-090 established this for the
> reported SNR and the located pitch and replaced both with held peaks; the
> admission tests were never given the same treatment. Measured: the gate's own
> elements over the 1.56 seconds that spell `N4L` score 0.173 against a random
> null of 0.231, while the same gate pooled over the same recording scores 0.409.
> Unit 1.11.19's finding that the stream carries no Morse structure is withdrawn
> — it carries structure exactly where somebody is sending.**

*Rejected: the envelope as the survey's input.* Measured on all ten captures at
matched hop. It improves one — `013347`, already the cleanest — and leaves both
silence controls scoring better than most stations. It is not the fix.

*Rejected: per-pass scoring as it stands.* Measured. It recovers the structure,
and noise recovers as much: `012403` reads a callsign with three passes under
0.20 while `014854` holds nothing and has four. Nineteen runs a pass is not
enough evidence for a fit to carry a decision.

---

**The real question is now sharper than it has been in four units, and it is not
a statistic.**

Every axis so far has asked "is this bin keyed?" of a three-second window, and a
station answering a call is silent through most of any three seconds you pick.
The measurement that works — HM-DEC-090's — does not average; it holds a peak and
lets it decay.

*Not proposed, because it needs a ruling:* whether admission should hold the best
fit a bin has produced over a decaying window, the way the SNR and the pitch
already do, rather than scoring the window pooled. That would use the passes
where the sender is actually sending and discard the ones where he is not, which
is what a held peak is for. **The risk is the one HM-DEC-090's own guard names**:
a held maximum over noise rises to whatever the luckiest pass produced, and the
per-pass table shows noise producing passes at 0.152. So it needs a second
condition — how many good passes, or how close together — and that is a design
question rather than a constant.

---

**A seventh intermittent, in a unit that changed no engine code.**

`Rig.CivRigTests.ARigWhoseReadLoopIsStuckStillDisconnects`. Seven timing tests
now fail unpredictably; three different ones have fired in the last four runs.
**A full-run total is no longer a number anyone can read**, and every report now
has to diff which tests moved. Worth its own small unit.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Seventeen inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's rulings of 2026-08-25/26, including the one this unit acts under.**
5. **The tone tracker** — the ask is now pooling versus a held peak, above.
6. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named** — the operator's own item
   five, and the last of his list not yet attempted.
10. **The keying meter** — its measurement found a station its verdict denied,
    and on `cw-2026-08-20-014935` it reads 44 words a minute off silence.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **The gate opens on everything, including two empty recordings** (1.11.18).
13. **The hop's ±32% cannot explain a worse-than-random residual** (1.11.19) —
    **closed this unit**: the residual is not worse than random where somebody is
    sending, so there is nothing left for the hop to fail to explain.
14. **The survey's gate output carries no Morse structure** (1.11.19) —
    **withdrawn this unit**, above. It carries structure where somebody is
    sending; pooling destroyed it.
15. **Pooling versus a held peak**, above — the headline ask.
16. **Nineteen runs a pass is not enough evidence for a fit**, above.
17. **A seventh intermittent**, above.

New this unit: **the pooling artifact and the withdrawal of 1.11.19's
conclusion**, above; **the envelope is not the fix**, above; **a seventh
intermittent**, above.

Closed this unit: **whether the envelope is quantised where the gate is not** —
measured on ten captures at matched hop, and it is not, except on the one capture
already cleanest.

Still open: **the lock's mixed help**; **the "Hold this pitch" button**; **three
fixtures at accepted cost**; **`001520`'s quadrillions**; **the reference and
port integrator difference**; **an unmeasured pitch costs `N4L`**; **the
six-hertz window disagreement**; **the short-character bias**; **the Avalonia
geometry offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.20**; **the whole-file
second pass**; **the squelch has no axis**; **the three morning captures of
2026-08-26**.

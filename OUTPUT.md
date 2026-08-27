UNIT: 035 — complete at task 6 of 6 — 2026-08-27 19:10

## 1. What Claude did

**Complete. All six tasks ran, including the drop.**

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. **Hamlet confirmed.** Branch `main`
throughout, five commits, all pushed, none refused. Version 1.11.31 to 1.11.32
per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

**Nothing shipped from task 3, and that is the unit's result rather than a
failure to finish.** The model was built exactly as specified, measured across
the whole corpus, and moves every recording the wrong way.

### Task 1 — banked, deleted, and a completed run

`ANALYSIS-cw-key-up-is-not-noise-2026-08-27.md` is committed at the root. Both
captures it names were already in the tree with their sidecars.

**`CwPitchRanking` and its tests are deleted** per Tim's ruling.

**Suite to completion: 1852 tests, 1824 passed, 28 failed, 18.5 minutes, failing
set byte-identical by name.** The total is 1852 rather than the order's expected
1856 **because deleting the ranking took its four tests with it.**

### Task 2 — the premise holds, and two of my own measurements were wrong first

**Answer to the sentence the order asks for: yes.** Key-up sits **14.7 dB** above
the band floor on `014113`, **15.9 dB** on `014308`, and **36.7 dB** on
`012403`, which reads. It is well above the floor on captures that read as well
as on captures that do not, so tasks 3 to 5 proceeded by the order's own rule.

**And unit 1.11.31's conclusion was wrong, which this unit owns.** Band SNR
through Hamlet's own envelope is **21.1 dB** on both unread captures against 41.1
on the control. Calling them "below the decoder's sensitivity" was a statement
about the window ratio dressed as a statement about the signal. **The stations
are not weak.**

**Two errors in my own instruments were caught before they misled anything.** The
autocorrelation was unnormalised, so it fell away with lag and reported the
search floor on four captures of five; normalised by the overlap and started
above the integrator's own smoothing, `014113` reads **110 ms**, matching the
analysis to the millisecond. And the two-state fit was cut at the midpoint of the
range, which one quiet hop drags down; it is iterated to a fixed point now.

### Task 6 — the drop, done, and it undermines the order's evidence

Set out in section 3.

## 2. What the owner should expect

**A station he can hear still does not reach the screen, and nothing this unit
built changes that.** `014113`, `014308` and `125941` emit nothing;
`cw-2026-08-25-012823` still emits 41 characters of junk at 450 Hz.

**A dead frequency still stays quiet.** Both recordings holding nothing emit
nought, unchanged.

**What will look wrong and is not:** `FittedLogLikelihoods` is in the engine and
nothing in the application calls it. It is exercised by the measurement that
decided against it, and deleting it would throw away the ability to re-run that
comparison. **It is not a second decode path in the application.**

| | baseline | end |
|---|---|---|
| engine | 28 of 1852, byte-identical by name | **28, byte-identical — measured this unit** |
| app | 509 of 509 | **not re-run — no app file was changed** |

## 3. What you should see

**Task 2's sentence first, as the order requires.** **Yes — the key-up state sits
well above the band noise floor on captures that read and on captures that do
not**, at 14.7, 15.9 and 36.7 dB. **But it sits higher still on both recordings
that hold nothing, at 26.1 and 25.1 dB**, so it does not separate a station from
an empty band.

### The window ratios, before and after, against the floor of 1.40

| recording | assumed key-up | fitted key-up | move | what |
|---|---|---|---|---|
| `cw-2026-08-22-014113` | 0.88 | **0.79** | −0.09 | he hears it |
| `cw-2026-08-22-014308` | 0.82 | **0.69** | −0.13 | he hears it |
| `cw-2026-08-26-125941` | 0.51 | **0.44** | −0.07 | he hears it |
| `cw-2026-08-24-012403` | 1.69 | 1.56 | −0.13 | READS — control |
| `cw-2026-08-17-013347` | 3.21 | 2.12 | −1.09 | READS — `VA3VRR` |
| `cw-2026-08-17-134712` | 4.67 | 3.86 | −0.81 | READS — `N4L` |
| `cw-2026-08-18-004507` | 5.65 | 4.51 | −1.14 | READS — the bulletin |
| `cw-2026-08-20-014854` | 0.94 | 0.74 | −0.19 | **holds nothing** |
| `cw-2026-08-20-014935` | 0.11 | 0.16 | +0.05 | **holds nothing** |

**Unread captures lifted over the gate: 0 of 3. Every recording moves down.**

**The mechanism is arithmetic, not tuning, and it is the thing worth keeping from
this unit.** The window ratio is a comparison **against** the all-key-up
hypothesis. Fitting key-up makes that hypothesis explain the observed inter-mark
hops *better*, which raises the null and therefore **lowers** the ratio. **If
key-up genuinely is not the noise floor, the honest model must score keying lower
than the shipped one does** — which means the shipped Rayleigh-at-the-noise-scale
has been over-crediting keying all along, and the floor of 1.40 was calibrated
against that over-crediting.

**So the analysis is right about the signal and the fix it implies runs the wrong
way.**

### The number no observation model got past

**`cw-2026-08-20-014854`, which holds nothing, scores 0.94. `cw-2026-08-22-014113`,
which holds a station the operator can hear, scores 0.88.** Under the fitted model
they are 0.74 and 0.79. **Under both models the empty recording and the real
station are within a tenth of each other, on either side.** No threshold on this
quantity separates them, and that is why no floor and no acquisition change has
ever reached these captures.

### Task 6 — the reference decoder, and the order's evidence does not hold

The order says `cwdecoder.py` reads these captures and Hamlet does not.

| recording | `cwdecoder.py` | Hamlet |
|---|---|---|
| `cw-2026-08-22-014113` | `ET EEETTETEIEEETIEREEEEEETTEEEEEEEU EEE E EIT…` | nothing |
| `cw-2026-08-22-014308` | **emits nothing** — "timings do not cluster as Morse" | nothing |
| `cw-2026-08-24-012403` — adjudicated `DE KD0UN KD0UN K` | `EEIEIEETE▯ETTTITIEETTEEEEEEEEEE ETAEETT…` | **reads it at 84 %** |
| `cw-2026-08-20-014854` — **holds nothing** | **`▯ ▯▯I M YOY▯KB A NB ▯A IM`** | **nothing** |
| `cw-2026-08-20-014935` — **holds nothing** | emits nothing | nothing |

**It does not read either capture. It does not read the control Hamlet does read.
And it puts words on a recording that holds nothing**, which is the property
HM-DEC-120 exists to protect and which Hamlet holds and it does not.

**Hamlet is better than the reference on all five recordings measured.** The
claim that a published implementation reads what Hamlet cannot is not supported
by running it.

## 4. What's blocking us

**Two recordings the operator can hear cannot be told from an empty band by any
quantity this decoder computes, and that is now measured under two observation
models rather than one.**

Ruling asked for:

> **The next unit adjudicates by ear before it measures anything.** An empty
> recording scores 0.94 and a station he can hear scores 0.88 on the shipped
> model; 0.74 and 0.79 on the fitted one. **The two are inseparable on this
> quantity in both directions**, and four units have now searched for a
> threshold, an acquisition rule and an observation model that would part them.
>
> **What is missing is not a statistic. It is ground truth.** Nobody has ruled
> what `cw-2026-08-22-014113` and `cw-2026-08-22-014308` contain — not a
> callsign, not a word, not how fast. `cwdecoder.py` says `014308`'s timings do
> not cluster as Morse; the operator says he hears a station. **Those cannot both
> be tested against until somebody writes down what was sent.**
>
> **What was rejected:** another observation model, on the evidence of this
> unit — the premise was right and the fix ran backwards; and any further
> threshold work, because the empty recording is on the wrong side of every
> threshold tried.

**Three minutes with headphones on `014113` and `014308` is what unblocks this**,
and it is the same ask task 6 of unit 1.11.31 made about the empty corpus.

---

**The floor of 1.40 was calibrated against a model that over-credits keying.**

That follows from this unit's arithmetic and it is not a proposal to move it. If
the key-up state is not the noise floor — and it is 15 to 37 dB above it — then
the shipped Rayleigh gives the null too little credit, every ratio in the corpus
is inflated, **and the floor is a number fitted to that inflation.** Both remain
self-consistent, which is why nothing here breaks; but the next honest
observation model will need the floor re-derived with it, and that pair has to
move together or not at all.

---

**The order's supporting evidence did not survive being run.**

`cwdecoder.py` reads neither capture, does not read the control Hamlet reads, and
emits words on a recording holding nothing. **Two orders have now cited it as the
working reference.** If it is to keep that role it needs a measurement behind the
claim; if not, the reference is the port under `tools/reference-decoder/`.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Twenty-eight inbound
after this unit. The oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150** —
   HM-DEC-090, 095, 120, 125 and 127 are all inside it.
5. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
6. **A boxcar's nulls made two of five swept offsets pathological best cases.**
7. **Two stations closer than 125 Hz are not named** — the operator's item five.
8. **The keying meter** — its measurement found a station its verdict denied,
   confirmed on a second capture in 1.11.31.
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
    (1.11.28).
19. **The scanner and the calling cycle are attached to the rig with no
    control** (1.11.28), and one of them transmits.
20. **Thirteen dead `DataTemplate` blocks nothing can distinguish from live
    ones** (1.11.28).
21. **Whether every constructed view model should be reachable from a binding**
    (1.11.28).
22. **`013347` returns a likelihood ratio of 17.2 million**, with `001520`'s
    quadrillions. Parked, raised once.
23. **No capture reproduces the junk the operator is watching** (1.11.30).
24. **The empty corpus is blocked by the keying meter, with proof** (1.11.31).
25. **An offline sweep over a bank is not what the application runs**, and a
    figure from one was carried into two orders as though it described the app.
    **`CwPitchRanking`'s lesson, for `DECISIONS.md`, per task 1.**
26. **The two captures have no ground truth and cannot be told from an empty
    band**, above.
27. **The floor was calibrated against a model that over-credits keying**, above.
28. **`cwdecoder.py` does not read what two orders say it reads**, above.

New this unit: **the ground-truth ask**, above; **the floor's calibration**,
above; **the reference decoder**, above; **the ranking's lesson**, item 25.

Closed this unit: **a completed engine run** — 1852, 28 failing, byte-identical.
**Whether key-up is the noise floor** — it is not, by 15 to 37 dB, confirmed with
Hamlet's own instruments. **Whether the stations are weak** — they are not, 21.1
dB out of their band, and unit 1.11.31 was wrong. **Whether fitting key-up helps**
— it does not; every recording moves down and the reason is arithmetic.
**Whether the reference reads them** — it does not, and it is worse than Hamlet
on all five.

Still open: **the lock's mixed help**; **three fixtures at accepted cost**; **an
unmeasured pitch costs `N4L`**; **the six-hertz window disagreement**;
**`CHANGELOG.md` at 1.9.0 against 1.11.32**; **the squelch has no axis**; **the
three morning captures of 2026-08-26**; **seven timing intermittents**.

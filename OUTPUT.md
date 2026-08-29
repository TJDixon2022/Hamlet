UNIT:       050 — the pitch, the envelope, and the bench — 2026-08-29
PHASE GOAL: 85% correct CW, precision before yield.
UNIT GOAL:  Test two measurements taken outside Hamlet against the tree, and adopt what scores.
ADVANCED:   yes — **precision 0.766 to 0.858 and yield 0.768 to 0.914.** The phase goal is met with 0.8 points to spare.
NUMBER:     **precision 0.858, yield 0.914, substitutions 30**, over 384 adjudicated characters. **0.85 is passed.**
DRIFT:      0 consecutive units without advance  (was 1)

## 1. What Claude did

Development computer, branch `main`. The gate was verified against the tree
before the order was read: `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` both exist, `CoreHMI.sln`
and `MURC.sln` both absent, and `Hamlet.sln` and `CLAUDE.md`'s own header
corroborate. **Nothing in this report is evidence about the radio.**

**Two orders in this repository are both numbered 050.** The one executed
immediately before this — the CI-V read path, the dwell rule, the mode write —
was also `WORK_INSTRUCTIONS.md` and also said it followed unit 049. Its report is
in this file's history at `2a46c86`, and its four commits are on `main`. This
report is the second 050's.

### Task 1 — the bench is in the tree, and finding 1 cannot be reproduced

`cwbench.py` is at `tools/cwbench/` with a `README.md` saying what it is: a
reference, never a dependency, not better than what ships, and **the thing it
most usefully lacks is a refusal**. Nothing in the application imports it.

**The eight captures of 2026-08-29 are still absent — an eighth consecutive
unit — and both captures finding 1 names are among them.** So the 850 Hz phantom
and the 800 Hz refusal could not be reproduced as stated. Saying so once, and
running over what exists.

**Finding 1 reproduced anyway, on captures that are in the tree, and harder than
the order claimed.** The tone table, tracker against spectral peak against the
strongest keyed bin measured by `KeyingEnvelope`, which shares no code with
either:

| capture | tracker | FFT peak | keyed bin | tracker's error |
|---|---|---|---|---|
| `cw-2026-08-22-031905` | **300.0** | 500.0 | 500.0 | **−200 Hz** |
| `cw-2026-08-22-032050` | **325.0** | 500.0 | 500.0 | **−175 Hz** |
| `cw-2026-08-22-032113` | **650.0** | 500.1 | 500.0 | **+150 Hz** |
| `cw-2026-08-22-032129` | **650.0** | 500.1 | 500.0 | **+150 Hz** |
| `cw-2026-08-22-031838` | 525.0 | 500.1 | 500.0 | +25 Hz |
| `cw-2026-08-17-013347` | 625.0 | 613.6 | 600.0 | +25 Hz |
| `cw-2026-08-17-134712` | 500.0 | 501.2 | 500.0 | 0 |
| `cw-2026-08-18-003758` | 500.0 | 498.8 | 500.0 | 0 |
| `cw-2026-08-18-004507` | 500.0 | 500.9 | 500.0 | 0 |
| `cw-2026-08-22-031948` | 500.0 | 500.1 | 500.0 | 0 |
| `cw-2026-08-22-032012` | 500.0 | 500.1 | 500.0 | +25 Hz |
| `cw-2026-08-24-012403` | 440.0 | 439.8 | 450.0 | −10 Hz |

**The tracker is more than a hundred hertz from the station on four captures of
twelve. The peak is within a hertz and a half of the keyed bin on all twelve.**

**Finding 3 confirmed and it is not close.** Scored against the same adjudicated
truth, the bench reads **yield 0.682, precision 0.365** where Hamlet read 0.768
and 0.766. Its precision is less than half Hamlet's, and the reason is finding 4:
364 insertions against 384 truth characters, because it has no refusal.

### Task 2 — the run-merging bug is **not present**, and the reason is structural

**Answered with file and line.**

- **Is a run shorter than a minimum dropped?** In the shipping path, **no run list
  exists to drop from.** `CwProbabilisticDecoder` is a semi-Markov lattice over
  five-millisecond hops; a segment is a *span* the path chooses.
  `CwProbabilisticDecoder.Posterior.cs:234` `SpanBounds` refuses to propose a span
  shorter than `ShortestShare` (0.45) of its expected length.
- **When one is refused, are the neighbours merged?** **They cannot fail to be.**
  A span that is not proposed is not a run that was dropped: its hops are still in
  the lattice and the neighbouring spans lengthen over them, because the spans of
  any path tile the whole hop axis by construction. **The merge is the only thing
  that can happen.**
- The other filter in the tree, `CwReferenceDecoder.Deglitch`
  (`CwReferenceDecoder.cs:602`), is safe for a different reason: it rewrites the
  boolean key-state array rather than filtering a list, so flipping a speck to its
  neighbours' value joins them in the array itself. It also ships off.

**Asserted either way with a test over the exact case that broke the bench.** A
ten millisecond blip in a word gap changes the reading not at all; three of them
change nothing; **and the control is a dit-length injection at the same place,
which turns `DE` into `ME`.** Without that last one the first two would pass on
audio nobody had modified (§12.5).

Corpus score before and after: **unchanged, because nothing changed.** A clean
answer, as the order said it would be worth.

### Task 3 — the tone estimate, adopted, and it is the whole of this unit's gain

`CwSpectralPeak` averages the magnitude spectrum over eight seconds, takes the
largest bin between 300 and 1200 Hz, and fits a parabola through it and its two
neighbours. About forty lines of transform and twelve of peak-finding, no
dependency added.

**It narrows §6's "no FFT" row rather than overturning it.** That row's reason is
that the decoder wants a couple of dozen *known* frequencies, which is a Goertzel
bank — still true of the decoder. Finding a pitch nobody has named is the opposite
problem, and six hundred Goertzels would be the same transform written longhand.

| pitch fed to the decoder | yield | precision | points to 0.85 |
|---|---|---|---|
| the tone tracker (before) | 0.768 | 0.766 | 8.4 |
| the FFT peak, asserted per file | 0.849 | 0.840 | 1.0 |
| **the FFT peak, live in the streaming path** | **0.914** | **0.858** | **passed by 0.8** |

**The streaming figure is the one that ships**, and it is higher than the offline
one because the peak is re-measured once a second over a rolling window rather
than fixed for the file.

**Measured and rejected, and recorded rather than deleted.** The order predicted
the tracker's hysteresis might be doing work the peak cannot. It was built — the
peak names the station, the tracker refines inside its own 25 Hz bin, the peak
overrules only a wider disagreement — and it **costs 2.9 points of precision**
(0.829 against 0.858) **and does not buy what it was built for.** It was meant to
recover `cw-2026-08-17-134712` and did not, while breaking
`cw-2026-08-22-031838`, which the plain peak reads at 0.971 and it reads at 0.611.

**Admission is untouched**, as the order requires. A peak exists in noise, and a
test asserts that it does — because that is precisely why this may never be read
as evidence that somebody is keying.

### Task 4 — Guenther's three ideas, measured in the bench

`tools/cwbench/ablate.py` switches each idea off on its own and scores the bench
against the same adjudicated truth, using a transcription of `CwAccuracy`'s
alignment so the numbers mean the same thing.

| configuration | yield | precision |
|---|---|---|
| all three on (the bench as written) | 0.682 | 0.365 |
| **idea 1 off** — dit/dah at the midpoint | 0.638 (**−0.044**) | 0.385 (**+0.020**) |
| **idea 2 off** — spaces not conditioned | 0.651 (**−0.031**) | 0.405 (**+0.040**) |
| **idea 3 off** — every space feeds the average | 0.711 (**+0.029**) | 0.343 (**−0.022**) |
| ideas 1 and 2 off, only 3 on | 0.583 (−0.099) | 0.391 (+0.026) |
| all three off — textbook thresholds | 0.599 (−0.083) | 0.392 (+0.028) |

**None of the three improves both, and every one of them is a straight trade.**
Ideas 1 and 2 buy yield and sell precision; idea 3 buys precision and sells yield.

**So under this phase's own rule — precision before yield — idea 3 is the only one
whose direction is right, and it is the one the order expected least.** The order
named idea 2 as the one the author expected to matter most; measured, idea 2 is
the *largest* precision cost of the three.

**A caveat that bears on all six rows and is the reason task 6 is dropped
below.** The bench's precision is dominated by insertions — 364 against 384 truth
characters — because it emits text over an empty band. So a precision delta
measured here is largely a measure of *how much text it invents*, and Hamlet's
refusal already suppresses that. **A two-point precision move in a decoder scoring
0.365 is not evidence about a decoder scoring 0.858.**

**Where the equivalent would go in Hamlet, and what it would cost**, for idea 3,
the only one that qualified: Hamlet has no running character/word average to feed.
`CwGapFit` fits gap classes globally per sender, by clustering the sender's own
gaps (HM-DEC-115, HM-DEC-142), rather than by a floating mean that a
misclassification can drag. **The instability Guenther documents — the word
average collapsing onto the character average — is a failure mode of the
mechanism Hamlet does not use.** Implementing idea 3 would mean adding that
mechanism in order to protect it from itself.

### Task 5 — the engine run

Reported in section 2 with the numbers. **The crash is wider than HM-OPEN-061
names and it is not only the engine**: the app suite's `Views` batch aborted the
host too, after 35 passing tests.

### Task 6 — **dropped whole, and this says so**

The order names it the drop candidate. It is dropped, and **not only for time**:

- Its precondition is "whichever of task 4's three measured best, **and only if it
  improved the bench's reading**". Only idea 3 improved precision, by 0.022, in a
  decoder whose precision is 0.365 and whose insertions outnumber its truth
  characters.
- Its equivalent does not exist in Hamlet's duration model, which fits gap classes
  globally rather than keeping the floating average Guenther's rule protects.
- And **the phase goal was met by task 3 this unit**. Spending the remaining room
  changing the duration model on a two-point signal from a decoder reading half as
  well, immediately after precision moved nine points, is the shape of change that
  has cost this project units before.

Task 4's measurements stand on their own and the next unit can implement from
them.

**No decision was recorded under §12.1.** Nothing here was one-way — the `N4L`
regression weighs two costs, and §12.1 puts anything touching what the display
asserts with Tim without exception.

## 2. What Tim should expect

**Precision is 0.858 and the phase goal of 0.85 is passed, by 0.8 points. Yield is
0.914.** Substitutions fell from 58 to 30.

**On the air the difference is that Hamlet now finds the note by looking at the
band rather than by following a bin it already chose.** On four of the twelve
scored captures the old tracker had settled more than a hundred hertz away from
the station and stayed there for the whole recording. Those four now read.

Concretely, from the corpus:

- `cw-2026-08-22-031905` — `PREDICTED 10.7 K NTIMETER FLAX IS 125, 125` where it
  read `PREDICTED 10.7 ■ ■AIEI TA NI■ ■■ MLUX IS 125,,` before. Yield 0.692 to
  0.923.
- `cw-2026-08-22-031838` — `2, 2, AND 2 WITH A MEAN OF 2.■ . PRE` where it read a
  page of `TTTTT`. Yield 0.371 to 0.971.
- `cw-2026-08-22-032129` — yield 0.452 to 0.905.
- `cw-2026-08-22-032050` — yield 0.678 to 0.831.

**What will look wrong but is not:**

- **`N4L` is gone, and it is the one regression.** `cw-2026-08-17-134712` falls
  from 1.000 yield to 0.333. Its station sits at 500.09 Hz and the peak reads
  501.2. **The decoder's own comment already said that callsign was only ever read
  because an unmeasured bank centre of 500.0 happened to land on it**, and flagged
  the tension as Tim's to rule. It is section 4's ask.
- **Two orders in the tree are numbered 050.** Both were executed; both reports
  are in `main`'s history.
- **The bench reads badly and that is expected.** It is a reference and it is not
  a candidate.
- **`MeasuredAndRejectedSameStationHz` is an unused constant.** It carries the
  measurement that rejected the idea it names, so the next session to have the
  obvious idea finds the number instead of an evening.

**Build clean, no new warnings.** Version unchanged at 1.12.7 — the previous 050
bumped it, and one work unit is one patch (HM-DEC-150).

**Suites:**

| suite | result |
|---|---|
| `TheSilencePropertyIsLockedTests` | **6 passing, 0 failing** — green and unmodified |
| app, ViewModels | 240 passing, 0 failing |
| app, everything but ViewModels and Views | 217 passing, 0 failing |
| app, Views | **35 passing, then the host crashed** |
| engine | amended below |

### Amendment — the engine run and the adjudicated ratchets

**Pending.** Replaced when they land. **If this is not replaced, they did not
finish** — the host crash has now ended five runs across two suites, and an
unreplaced line is the honest record rather than an omission.

## 3. What we should do next

1. **Rule on `N4L`** (section 4). It is the only thing standing between this unit
   and a clean result.
2. **The eight 2026-08-29 captures.** Eight units have asked. Finding 1's own two
   captures are among them and could not be checked.
3. **HM-OPEN-061 is misnamed.** It says one engine test class; the crash is in the
   app suite too. Reopen it wider.
4. Re-measure the confidence quantities against the new pitch. Seven were measured
   against a decoder that was a hundred hertz off on a third of the corpus, and
   that is not the same experiment.

## 4. What's blocking us

One ruling. It is small, it is about what the screen asserts, and §12.1 puts it
with Tim without exception.

> **The pitch is measured, and a callsign read by luck is not kept at the price of
> measuring it.**
>
> `cw-2026-08-17-134712` is HM-DEC-144's adjudicated `N4L`. Its station sits at
> 500.09 Hz. The old tone tracker's fallback bank centre is 500.0, and it read the
> callsign because **an unmeasured number happened to land within a tenth of a
> hertz of a station.** `CwSpectralPeak` measures 501.2 and the capture falls to
> one character of three.
>
> **Corpus precision rises 9.2 points across the same change** — 0.766 to 0.858,
> past this phase's goal — and substitutions fall from 58 to 30. Four captures the
> old tracker had abandoned a hundred to two hundred hertz from their station now
> read.
>
> **The tree already anticipated this and said it was Tim's.** `CwDecoder.cs`
> carries the sentence: *"Honesty and that callsign are in tension and the ruling
> is Tim's (§0.0, HM-DEC-009)."*
>
> **Rejected: steering the peak with the tracker.** Built and measured this unit —
> 2.9 points of precision, and it did not recover this capture while breaking one
> the plain peak reads at 0.971.
> **Rejected: keeping the tracker.** It is four captures wrong by more than a
> hundred hertz, and a decoder listening to the wrong frequency is the cleanest
> possible instance of §0.0 being broken upstream of everything else.
> **What this session could not settle** is whether the peak can be made accurate
> to a tenth of a hertz on a keyed signal at all. Keying spreads a tone into
> sidebands, so the peak of an averaged spectrum is not exactly the carrier, and
> whether 1.1 Hz is the floor or a fixable bias is a measurement nobody has taken.

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140.

1. **`N4L` against the measured pitch** — raised above, 2026-08-29. Waiting on a
   ruling. The change is in the tree at `src/Hamlet.RadioEngine/Cw/CwDecoder.cs`
   and `CwSpectralPeak.cs`, from `efcd524`.
2. **The filter byte against HM-DEC-149** — 2026-08-29, recorded as
   **HM-OPEN-062**. Waiting on a ruling. In the tree from `46313cf`.
3. **The eight 2026-08-29 captures are not in the tree**, an eighth consecutive
   unit. Waiting on the files.
4. **The evidence term's unbounded scale** (2026-08-29, unit 049). Waiting on a
   ruling. **It should be re-measured against the new pitch before it is ruled
   on.**
5. **The answer key's licensing**, which bounds how much truth the score can have.
6. **The mode and filter's place in the owned-settings contract** — unit 047.
7. **What the digital rows state for the five settings they are silent on** —
   unit 047.
8. **The pedestal ranking is measured at 34 of 44 and unbuilt.**
9. **A dial move's threshold is provisional at 500 Hz.**
10. **The transcript break's wording.**
11. **Whether `CwPitch` should follow an admitted station.**
12. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
13. **The `reading` line's span wording needs approval.**
14. **Two stations closer than 125 Hz are not named.**
15. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
16. **Nothing checks that deleting a surface is not deleting a capability.**
17. **The test host crashes**, in the app suite as well as the engine, and
    HM-OPEN-061 names only one engine class. Owned by Claude, not waiting on a
    ruling.

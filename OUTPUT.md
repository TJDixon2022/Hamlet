# OUTPUT.md

## 1. What Claude did

**The three adjudicated readings, by name, after the change:**

| what | where | reads | right? |
|---|---|---|---|
| `N4L` (HM-DEC-144) | `cw-2026-08-17-134712` | `… E K E EN4LQ EK …` | **right** |
| `VA3VRR` (HM-DEC-145) | `cw-2026-08-17-013347` | `… E HA E WVRR VA3VRR E E` | **right** |
| `AA4MP/4 QNIK` (HM-DEC-126) | `cw-2026-08-18-003758` | `… 55H AA4MP/4 QNIK E E E …` | **right** |

**All three, and the gap mechanism is switched on.**

### Task 1 — the structure has to survive six seconds of new audio

**Counted read by read before choosing anything.** A read is half a second; the
figure is the longest run of consecutive reads that found a trough:

| | longest run |
|---|---|
| generated Morse, 12 wpm | **52** |
| generated Morse, 18 wpm | **30** |
| generated Morse, 25 wpm | **23** |
| **`004507`**, the file the coupling breaks | **36** |
| `014854`, holding no station | 16 |
| `003016` | 10 |
| `134712` | 6 |
| `003758` | 4 |
| `013622`, `014935` | 3 |
| `013347`, `003126` | 1 |

**Nothing measured sits between ten and twenty-three.** The requirement is
**twelve consecutive reads**, in the middle of that empty stretch, and it is a
mechanism rather than a threshold: **twelve reads is six seconds of audio the
first read never saw**, which is longer than any single gap at any speed the
decoder considers — a word gap at eight words a minute is about a second — so it
is evidence from many characters rather than from one stretch of quiet.

**What must persist is the structure and not the number.** Each read re-measures;
what is required to hold is that a trough exists between the same two clusters,
and the lengths handed to the decoder are the most recent read's. A sender's gaps
wander; the empty region between them does not.

**Established and abandoned on the same evidence.** Twelve consecutive reads
without a trough returns to one, three and seven units. That symmetry is
deliberate: a sender's spacing is a fact about the sender, and one window that
happened to catch a pause is not evidence that it changed. On `004507` the run
breaks once near the end and the structure survives it, which is the whole reason
for not abandoning on a single miss.

**When the structure is not established the decoder gets one, three and seven
units**, which is today's behaviour and remains the fallback. **Reset on the
window being emptied**, so a station change starts the evidence again.

**Where it was built.** In `CwProbabilisticStream`, which owns the reads —
`CwUnitEstimator` is a measurement and has no memory. That is the third time a
change from the analysis has had no site as written and been built where it can
exist, and it is reported rather than repaired.

### Task 2 — measured against the adjudicated readings

**All three right, and `004507` repaired in part:**

| | |
|---|---|
| the target | `ACH STATION HANDLING ET HIS MESSAGE PE` |
| before this unit | `EE AC H STA TI O N HAN D L I NG ET H IS M E S S A G E PE` |
| **now** | `EE AC H STA TI O N HANDLING ET HIS MESSAGE PE` |

**`HANDLING`, `ET HIS` and `MESSAGE` are whole**, where they were `HAN D L I NG`,
`ET H IS` and `M E S S A G E`. `AC H` and `STA TI O N` are still broken. Its
one-letter-word share fell from 64% to 48%.

**And `003016` gained more than `004507` did**: one-letter words 22% → **8%**,
`ITWASJUNK` and `STIL<AS>HVEMY` now running together rather than apart.

**No adjudicated reading was traded for it**, which was the gate.

### Task 3 — the clips, and which of them is deciding

**Counted per capture, over the reads that found a trough:**

| capture | reads with a trough | character clip bound | word clip bound |
|---|---|---|---|
| generated, 12 / 18 / 25 wpm | 52 / 37 / 26 | **0** | **0** |
| `004507` | 45 | 2 | **43** |
| `134712` | 15 | 2 | **15** |
| `003016` | 16 | 1 | 12 |
| `003758` | 18 | **13** | 3 |
| `014854` | 24 | **22** | 6 |
| `013347` | 6 | 5 | 3 |
| `013622` | 4 | 3 | 3 |
| `003126`, `014935` | 4, 3 | 0, 3 | 1, 0 |

**The word clip carries almost every real case and never binds on generated
Morse.** On `004507` it bound in 43 of 45 reads, so **the clip and not the
measurement decided that boundary**, exactly as the order suspected.

**That is the clip doing the job it was given** — the previous session's note was
that word gaps are too rare in thirty seconds for their cluster to be trustworthy —
**but a bound that never releases is not a measurement.** It is not widened here.
**What would establish it**: a capture whose word boundaries are known, which is
the same adjudicated transcript this phase has been missing all week. Until then
the honest statement is that Hamlet's word spacing on real audio comes from a
constant derived from one machine keyer.

### Task 4 — fine tone tracking, measured and not built

**The premise does not hold as written.** Hamlet's reported pitch does not come
from a 25 Hz grid: the coarse survey is on 25 Hz spacing, and the fine bank the
tracker reports from is not.

| capture | tracker | a Goertzel peak over the whole recording, to 0.1 Hz | off by |
|---|---|---|---|
| `004507` | 500.0 | 500.8 | **−0.8** |
| `003758` | 500.0 | 501.2 | **−1.2** |
| `134712` | 500.0 | 501.4 | **−1.4** |
| `003016` | 670.0 | 668.7 | **+1.3** |
| `003126` | 665.0 | 669.3 | −4.3 |
| `013347` | 625.0 | 613.7 | +11.3 |
| `013622` | 600.0 | 612.4 | −12.4 |
| `014854`, no station | 600.0 | 609.0 | −9.0 |
| `014935`, no station | 825.0 | 616.5 | +208.5 |

**On every capture holding a clear station the tracker is within 1.5 Hz**, and it
reports 665 and 670, which a 25 Hz grid cannot express. The two 11–12 Hz errors are
the two oldest and weakest captures. **Against the decoder's own 60 Hz bandwidth,
11 Hz costs very little**, so nothing was built: the change would buy accuracy the
measurement says is already there.

**What would establish the need** is the W1AW capture the claim came from, where
the sweep was reported 17 Hz off and 4 dB down. It is not in the tree.

### The signature, every capture, before and after

| capture | E | T | one-letter words |
|---|---|---|---|
| `013347` | 39% → 39% | 5% → 5% | 60% → 60% |
| `013622` | 50% → 50% | 7% → 7% | 33% → 33% |
| `134712` | 76% → 76% | 0% → 0% | 84% → 84% |
| **`004507`** | 20% → 20% | 14% → 14% | **64% → 48%** |
| **`003016`** | 13% → **10%** | 18% → 19% | **22% → 8%** |
| `003126` | 16% → 16% | 12% → 12% | 48% → 53% |
| `003758` | 43% → 43% | 0% → 0% | 65% → 65% |
| `014854`, `014935` | silent | silent | silent |

**Two captures moved and the rest are untouched**, which is the persistence rule
working: it fires on the two whose structure survives and refuses the others.
**Nothing is at the target.**

### HM-DEC-120

**Further ahead than it has ever been.** Both recordings holding no keying are
silent. The sweep now invents nothing from eighteen decibels down to **three**,
where last night it was clean to five:

| dB | 18 | 12 | 10 | 8 | 6 | 5 | 4 | 3 |
|---|---|---|---|---|---|---|---|---|
| right / wrong | 1.00/0.00 | 1.00/0.00 | 1.00/0.00 | 1.00/0.00 | 1.00/0.00 | 0.97/0.00 | 0.94/0.00 | 0.97/0.00 |

### The failing set

**28 before, 28 after**, and one moved: `HoldingTheWindowLongInTimeReadsMore` on
`003016` went green. Nothing went red. One of the 28 is the app flake
`TheFollowedSentenceReachesTheScreenTests`, which **passes when run alone** —
`HM-OPEN-055`.

### Mismatches and collisions

- **The seven W1AW captures, `2026-08-22.jsonl` and both analysis documents are
  still not in the tree.** Every figure quoted from them went unchecked. Everything
  above is measured here.
- **`Directory.Build.props` said 1.10.11**, as the order expected.
- **`CLAUDE_CODE.md` §8 names five report sections and `CLAUDE.md` §12.2 names
  four.** Under §0 the project's file wins on the four it names; section 5 is
  additive and is written. Reported, not repaired.
- **Mechanism or tuning:** the persistence rule is a mechanism with a measured
  empty stretch on both sides of it. The two clip ranges remain constants from one
  machine keyer, and task 3 says which is carrying.

## 2. What Tim should expect

**He will see more whole words on the clearest signals and no change at all on the
rest**, because the rule fires only where the sender's own spacing holds still for
six seconds.

| capture | before | now |
|---|---|---|
| `004507` | `EE AC H STA TI O N HAN D L I NG ET H IS M E S S A G E PE` | `EE AC H STA TI O N HANDLING ET HIS MESSAGE PE` |
| `003016` | `E IADA KPA15TT IT WAS JUNK ■ E STILL HVE MY E TO 91B ETT JETST VFB TUBELIN` | `E IADA KPA15TT ITWASJUNK <BT> STIL<AS>HVEMY ETO 91B ETT JETST VFB TUBELIN` |
| `013347` | `… E HA E WVRR VA3VRR E E` | unchanged |
| `013622` | `E I5 SHE II 5EIEIE EEUE TE ISE …` | unchanged |
| `134712` | `… E K E EN4LQ EK …` | unchanged |
| `003126` | `E S 5 IWATTCH AT L E<AS>T 2 MOVI ESA DAY WID X■ WHY NOT …` | `… 2 MOVIESADAY WID X■ WHY NOT …` |
| `003758` | `… 55H AA4MP/4 QNIK …` | unchanged |
| `014854`, `014935` | silent | silent |

Build clean, no warnings. **28 failing, one of them the known flake, and one test
went green.**

**What will look wrong and is not:** `003016` now runs `ITWASJUNK` together where
it read `IT WAS JUNK`. Its word spacing comes from the word clip, which bound in 12
of its 16 reads — the same finding as task 3.

## 3. What we should do next

- **The word clip is deciding the word spacing on real audio**, in 43 of 45 reads
  on `004507` and 15 of 15 on `134712`. It cannot be established without a capture
  whose word boundaries are known.
- **`AC H` and `STA TI O N` on `004507` are still broken** while `HANDLING` and
  `MESSAGE` are whole, which says the remaining fault on that file is not the gap
  boundary.
- **Fine tone tracking is measured and unbuilt**, and the measurement says it would
  buy about a hertz on the captures that read.
- **Get the seven W1AW captures across.** Three units have now been designed around
  figures nobody here can check.

## 4. What's blocking us

Nothing blocks the next unit.

### RECORDED

Nothing was recorded to `DECISIONS.md`.

### NEEDS A RULING

Nothing needs a ruling to proceed. The one thing worth a decision when there is
evidence for it:

> **The word-gap clip is a constant from one machine keyer and it is deciding
> Hamlet's word spacing on real audio.**
>
> `[3.5u, 6.5u]` bound in 43 of 45 reads on `004507` and in every read on
> `134712`, and never binds on generated Morse. It is doing what it was given to
> do — word gaps are too rare in twelve seconds for their cluster to be trusted —
> **but a bound that never releases is a constant wearing a measurement's
> clothes.**
>
> **It is not widened here**, because widening it to stop it binding is tuning to
> this corpus. **What would establish it is a capture whose word boundaries are
> known**, which is the adjudicated transcript this phase has been missing all
> week.

### STATE

Gate verified against the tree: `Hamlet.sln` and `CwUnitEstimator.cs` present, no
`CoreHMI.sln`, no `src\CoreHMI`, `PROJECT_CARD.md` says Hamlet. **This session ran
on the development computer with no radio connected, so nothing in this report is
evidence about the radio** (`SHACK_FACTS.md`, HM-DEC-093).

Tasks 1, 2 and 3 done. **Task 4 was measured and deliberately not built**, with the
measurement above; it was not half-built.

## 5. Where the phase stands

**Phase 10. The goal is 80% correct translation on a single clear CW signal.**

**The phase number cannot be stated, and that is unchanged from before this unit.**
No capture in this repository has an answer key; ARLP034 was never published; the
three adjudicated items are fragments rather than transcripts. **All three are read
correctly at this build, as they were at the last one.**

**Build: 1.10.11 → 1.10.12.**

### Asks still outstanding

Carried per HM-DEC-139, verbatim until ruled.

- **The word-gap clip is carrying every real case**, first made today, above.
- No capture has an answer key, so the phase's own number cannot be stated.
- Why the mark and gap classifiers disagree about the unit — structural.
- The narrow decoder-side filter for a crowded passband.
- HM-OPEN-056, the held-peak SNR.
- The seven W1AW captures and `2026-08-22.jsonl` are not in the tree.
- Whether a sender change can be decided by pitch distance at all — measured dead.
- Whether the window clear comes back on.
- The advice line asserting a cause the app can disprove.
- The sidecar asserting two incompatible things about one span.
- Whether the sidecar's `text` should include the leading edge.
- Thirty seconds since the last character, for mode-follow's guard.
- Whether `RfGain`'s hundred per cent is a defect or the right answer.
- The likelihood gate at 15.0; the keying meter's provisional thresholds.
- `FollowSpeed` has no supplier.
- The mark-and-gap witness behind HM-DEC-144 and HM-DEC-145.
- HM-OPEN-052, HM-OPEN-053, HM-OPEN-054, HM-DEC-130, HM-DEC-098, HM-OPEN-033,
  HM-OPEN-007.

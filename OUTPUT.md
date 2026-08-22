# OUTPUT.md

## 1. What Claude did

**`cw-2026-08-18-004507`, which repairing is what this unit is for:**

| | |
|---|---|
| before the measured unit | `E AT ARRL DOT NET <BT> EACH STATION HANDLING ET HIS M E S S A G E P E` |
| at the start of this unit | `E E E U T EA R R L D O T N E T <BT> E E A C H S TA TI O N HAN D L I NG ET HIS…` |
| **now** | `E E EA T AR RL D O T N E T <BT> EE AC H STA TI O N HAN D L I NG ET H IS M E S S A G E PE` |
| **with the change wired in** | `E E EA T AR RL D O T N E T <BT> EE ACH STATION HANDLING ET HIS MESSAGE PE` |

**The change repairs it, and it is not shipped.** Wired in it costs `VA3VRR` and
breaks `AA4MP/4 QNIK`, two of the three adjudicated readings, to repair one file.
The ledger is below and the decision is section 4's ask.

**What moved without it**: `004507` reads a little better than it did at the start
of this unit — `M E S S A G E PE` where it read `HAN D L I NG` and stopped — and
**all three adjudicated readings are now right**, which they were not this morning.
That came from the short-cluster median that shipped in the last commit of the
previous unit.

### The three adjudicated readings, by name

| what | where | now |
|---|---|---|
| `N4L` (HM-DEC-144) | `134712` | **right** — reads `EN4LQ EK`, and it read `R4LQ` this morning |
| `VA3VRR` (HM-DEC-145) | `013347` | **right** — reads `WVRR VA3VRR` |
| `AA4MP/4 QNIK` (HM-DEC-126) | `003758` | **right** — reads `AA4MP/4 QNIK`, and without the stray spaces it had this morning |

**All three are right.** Wiring the gap change in makes two of them wrong.

### What was built, and where it could exist

**The mechanism the order describes is real and it is built**:
`CwUnitEstimator.MeasureGaps` clusters this sender's gaps with 3-means **on the
logarithms**, puts each boundary at the geometric mean of adjacent centroids, and
holds the two boundaries inside `[1.3u, 2.6u]` and `[3.5u, 6.5u]` as a clip
rather than as the estimate.

**It is handed to the decoder rather than applied to a boundary**, because Hamlet
has no boundary to move: the Viterbi decides element and character boundaries
together, and the ratio penalty already crosses at the geometric mean of whatever
two lengths it is given. **So passing the measured lengths in as what each gap kind
expects places the boundary at the geometric mean of two things the sender actually
did, automatically.** That is the same change expressed where it can exist, which
is what the order asked be reported.

**One addition the order did not ask for, and it is the load-bearing one.** Three
centroids can always be found; what makes them worth using is a trough. **Each
boundary is accepted only if fewer gaps stand near it than near either cluster it
divides** — counted in equal windows on the logarithm, so nothing is chosen. It is
parameter-free.

### The boundaries, per capture, and whether they landed in dead space

| capture | u | boundaries | in units | from the gaps? |
|---|---|---|---|---|
| `013347` | 62.5 ms | 108.3 / 286.4 | 1.73u / 4.58u | no |
| `013622` | 62.5 | 108.3 / 286.4 | 1.73u / 4.58u | no |
| `134712` | 30.0 | 52.0 / 137.5 | 1.73u / 4.58u | no |
| **`004507`** | **50.0** | **92.6 / 325.0** | **1.85u / 6.50u** | **yes** |
| `003016` | 47.5 | 82.3 / 217.7 | 1.73u / 4.58u | no |
| `003126` | 45.0 | 77.9 / 206.2 | 1.73u / 4.58u | no |
| `003758` | 47.5 | 82.3 / 217.7 | 1.73u / 4.58u | no |
| `014854`, `014935` | 40.0, 20.0 | 69.3 / 183.3, 34.6 / 91.7 | 1.73u / 4.58u | no |
| **generated Morse, 18 wpm** | 65.0 | **103.8 / 292.3** | 1.60u / 4.50u | **yes** |

**One capture in nine has the structure, and it is the one the coupling is
breaking.** The other eight fall back to one, three and seven units, so nothing
about them changes. **Generated Morse does have it**, which is what says the eight
refusals are a fact about those recordings rather than about this code — the
control that makes the measurement mean anything.

**Mechanism found, not a parameter tuned**, for the clustering, the log domain and
the trough test. **The two clip ranges are constants** and they came from one
station on one machine keyer; on the one capture that used them the word boundary
was clipped, landing at exactly 6.50u.

### The ledger: what wiring it in does

| | with it | without it |
|---|---|---|
| `004507` | `EE ACH STATION HANDLING ET HIS MESSAGE PE` | `EE AC H STA TI O N HAN D L I NG ET H IS M E S S A G E PE` |
| `003126` | `I WATCHATLE<AS>T2 MOVI ES` | `IWATTCH AT L E<AS>T 2 MOVI ES` |
| `003016` | `IADAKPA15TTITWASJUNK` — words run together | `IADA KPA15TT IT WAS JUNK` |
| **`VA3VRR`** | **lost** — `RR EEAA3VETER` | **right** |
| **`AA4MP/4 QNIK`** | **broken** — `AA4M E T T E/4 QNIK` | **right** |
| `N4L` | right | right |

**Two adjudicated readings for one repaired file.** The order's acceptance is that
`004507` reads at least as well **and nothing else gets worse**, so it does not
ship. The whole-file measurement accepts only `004507`, but a twelve-second window
can show a trough the whole recording does not, which is how the other two are
lost.

### The signature, every capture, before and after

Before is this morning's tree; after is what shipped tonight.

| capture | E | T | one-letter words |
|---|---|---|---|
| `013347` | 53% → **39%** | 7% → 5% | 73% → **60%** |
| `013622` | 48% → 50% | 7% → 7% | 33% → 33% |
| `134712` | 65% → 76% | 0% → 0% | 93% → 84% |
| `004507` | 21% → 20% | 13% → 14% | 69% → **64%** |
| `003016` | 13% → 13% | 18% → 18% | 22% → 22% |
| `003126` | 22% → **16%** | 13% → 12% | 52% → **48%** |
| `003758` | 59% → **43%** | 25% → **0%** | 65% → 65% |
| `014854`, `014935` | silent | silent | silent |

**Nothing is at the target** — single figures for E and T, zero one-letter words —
and four captures moved toward it.

### Fine tone tracking

**Not attempted.** The order gates it on the first change landing cleanly and it
did not land. It is untouched and independent, and it is the first item in section
3.

### The failing set

**28 before, 28 after, the same 28 by name.** One app test,
`TheFollowedSentenceReachesTheScreenTests`, failed in the full run and **passes
when its class is run alone** — the flake already filed as `HM-OPEN-055`.

### Mismatches and collisions

- **The seven W1AW captures, `2026-08-22.jsonl` and both analysis documents are
  still not in the tree**, so every figure quoted from them went unchecked,
  including the 52/55/65 and 125/192 and 405/442 gap clusters this unit's change
  was designed around. **Everything above is measured here.**
- **`Directory.Build.props` said 1.10.10**, as the order expected.
- **HM-DEC-063, HM-DEC-150 and `CLAUDE_CODE.md` §4.11 agree**: the minor is the
  phase, the patch is the work unit, the number lives in `Directory.Build.props`
  alone. **A collision resolved, not a conflict**, and HM-DEC-150 is the governing
  text.
- **`CLAUDE_CODE.md` §8 names five report sections and `CLAUDE.md` §12.2 names
  four.** Under §0 the project's own file wins on the four it names, and section 5
  is written as additive. **Reported, not repaired.**

## 2. What Tim should expect

**He will see slightly more correct characters on a clear signal than he did this
morning, and the file this unit was aimed at is still the worst of them.**

| capture | this morning | now |
|---|---|---|
| `013347` | `… E HEA E WVRR VA3VRR E E` | `… E HA E WVRR VA3VRR E E` |
| `013622` | `E I5 ■E II 5EIEIE EEUE TE ISE …` | `E I5 SHE II 5EIEIE EEUE TE ISE …` |
| `134712` | `… E K E R4LQ EK …` | `… E K E EN4LQ EK …` — **`N4L` restored** |
| `004507` | `E E E U T EA R R L D O T N E T <BT> E E A C H S TA TI O N HAN D L I NG` | `E E EA T AR RL D O T N E T <BT> EE AC H STA TI O N HAN D L I NG ET H IS M E S S A G E PE` |
| `003016` | `E E IADA KPA15TT IT WAS JUNK ■ E STILL HVE MY E TO 91B …` | `E IADA KPA15TT IT WAS JUNK ■ E STILL HVE MY E TO 91B …` |
| `003126` | `E E E U E E <BT> IWATTCH AT L E<AS>T 2 MOVI ESA DAY …` | `E S 5 IWATTCH AT L E<AS>T 2 MOVI ESA DAY WID X■ WHY NOT …` |
| `003758` | ` EET T E T E T EE T E ETE E TTEEEIIIE T T EE …` | `E E EQR■HH 55H AA4MP/4 QNIK E E E EE EAN EANQNI■K …` — **the confirmed callsign back** |
| `014854`, `014935` | silent | silent |

Build clean, no warnings. **28 failing, the same 28 by name.**

**What will look wrong and is not:** `CwUnitEstimator.MeasureGaps` and the
decoder's ability to take a sender's own gap lengths are both in the tree and
nothing passes them. That is deliberate and it is section 4's ask.

## 3. What we should do next

- **Rule on the gap lengths**, section 4. The mechanism works where the structure
  exists; the question is whether a window may use it when the whole recording
  does not show it.
- **Fine tone tracking**, untouched and independent of all of this.
- **The clip ranges are the only constants in the estimator** and they came from
  one machine keyer. On the one capture that used them the word boundary was
  clipped hard, at exactly 6.50u, which means the clip and not the measurement
  decided it.
- **Get the seven W1AW captures across.** Every figure this unit was designed
  around is still unchecked.

## 4. What's blocking us

Nothing blocks the next unit.

### RECORDED

Nothing was recorded to `DECISIONS.md`.

### NEEDS A RULING

> **Whether a twelve-second window may take the gap lengths from its own gaps when
> the whole recording does not show the structure.**
>
> The mechanism is built and measured. It refuses where there is no trough to put
> a boundary in, which on a whole-recording measurement accepts one capture in nine
> — **`004507`, the file the coupling is breaking** — and refuses the other eight.
> Generated Morse passes, which is the control.
>
> **Wired in, it repairs that file**: `ACH STATION HANDLING` and `MESSAGE` come
> back whole. **And it costs `VA3VRR` and breaks `AA4MP/4 QNIK`**, because the
> decoder reads twelve seconds at a time and a window can show a trough the
> recording does not.
>
> | | ship it | leave it measured and off | require more evidence per window |
> |---|---|---|---|
> | `004507` | repaired | as it is, the worst capture here | unmeasured |
> | adjudicated readings | one of three right | **three of three right** | the point of it |
> | what it rests on | a trough in this window | nothing changes | a trough that survives more gaps than one window holds |
>
> **The industry-standard answer is the third**, and it is not built: require the
> structure to be found over more evidence than a single window — the estimator
> already runs on every read and the trough could be required to hold across
> several of them. That is a mechanism rather than a threshold, and it is the shape
> that would keep all three adjudicated readings and repair `004507` as well.
> **Leaving it off is the honest state until then**, because three adjudicated
> readings are the only ground truth this repository has.

### STATE

Gate verified against the tree: `Hamlet.sln` and `CwProbabilisticDecoder.cs`
present, no `CoreHMI.sln`, no `src\CoreHMI`, `PROJECT_CARD.md` says Hamlet. **This
session ran on the development computer with no radio connected, so nothing in this
report is evidence about the radio** (`SHACK_FACTS.md`, HM-DEC-093).

The gap change was built, measured and left off. **Fine tone tracking was not
started**, under the order's own gate, and was not half-built.

## 5. Where the phase stands

**Phase 10. The phase goal is 80% correct translation on a single clear CW
signal.**

**The phase number cannot be stated, and that is unchanged from before this unit.**
`PHASE_GOAL.md` says it itself: no capture in this repository has an answer key,
ARLP034 was never published, and the three adjudicated fragments — `N4L`,
`VA3VRR`, `AA4MP/4 QNIK` — are fragments rather than a transcript. **What can be
said is that all three are read correctly as of this build, which was not true this
morning.**

**Build: 1.10.10 → 1.10.11.**

### Asks still outstanding

Carried per HM-DEC-139, verbatim until ruled.

- **Whether a window may take its gap lengths from its own gaps**, first made
  today, above.
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

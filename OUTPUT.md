# OUTPUT.md

## 1. What Claude did

### Task 1: the elements spell `N4L`, reproduced inside the repository

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet` and the four gate checks all hold. **No radio is
attached, and nothing here needed one** (HM-DEC-093).

`TheStationInTheRecordingIsN4LTests` reads `cw-2026-08-17-134712.wav`, takes the
gate's own elements, and finds twenty-one of them between **21.45 s and 23.01 s**,
exactly the stretch the instruction named. **The boundaries scanned are 21.30 s to
23.10 s**, a little wider either side so a small change in the gate moves what
falls inside them rather than clipping an end off; the elements themselves land
where the instruction said.

```
  21.45s mark 225   21.48s gap  30   21.54s mark  55   21.72s gap 180
  21.77s mark  55   21.81s gap  40   21.87s mark  55   21.91s gap  40
  21.97s mark  60   22.01s gap  40   22.06s mark  55   22.09s gap  30
  22.34s mark 245   22.49s gap 150   22.55s mark  60   22.57s gap  25
  22.82s mark 245   22.86s gap  40   22.91s mark  55   22.95s gap  40
  23.01s mark  55
```

**The cuts are fitted from this stretch and from nothing else** (§12.5): the marks
are split at the midpoint of their own two means, at 147 ms, and the gaps at
theirs, at 100 ms. Nothing asks the decoder what a dit is, what a dah is or where a
character ends, because the decoder's answer to all three is what is under
investigation. That gives `-.` `....-` `.-..` — **N, 4, L**.

Dit **56.3 ms**, dah **238.3 ms**, ratio **4.24**, element gap **35.6 ms**,
character gap **165.0 ms**. About twenty-two words a minute, a heavy fist, and the
Farnsworth spacing HM-DEC-115 measured on a different station.

**HM-DEC-095's carrier finding is overturned and recorded as HM-DEC-144**, with the
element sequence and the callsign in the entry, indexed in `CLAUDE.md` §1.
Everything else in HM-DEC-095 stands.

`ACarrierNeverConvincesTheTrackerItIsAStation` is retired rather than deleted. It
read this file by name and required the tracker never to claim keying in it, which
made every change that let Hamlet notice the station fail the suite. **No recording
in this repository is established as a carrier**, so the property is now asserted
on audio whose truth is known by construction: a tone that never stops, in a
shaped band. **A synthesized fixture is the weaker evidence** (HM-DEC-091) and it
is what there is until a real carrier is recorded and kept. `TheStrongSignalThatIsNotKeyingIsReportedAsInterference`
keeps its measurements and carries a note saying its name is now wrong, because
what the survey should call that signal is HM-OPEN-054's question and is parked.

### Tasks 2 and 3: both built, both measured, neither shipped

**Nothing in `src/` changed this session.** The decoder is byte-identical to where
it started.

**Task 2, what was changed and why.** The rolling window holds three populations
and was being fitted with two. Inside the callsign the twenty most recent marks are
eleven of the station's, at 55 and 235 milliseconds, and nine slivers between 5 and
45 that the gate chopped out of band noise. A two-way fit puts the slivers in with
the dits and returns 45 for a dit of 56; seeded on percentiles in log space, which
is what HM-DEC-119 already does for the mark boundary, it does **worse** and returns
14, because the slivers are numerous enough to own a centre of their own. So the fit
was given a third cluster to put them in, fitted from the marks themselves with no
length declared too short to be an element, and the lowest set aside only when the
marks say it is a separate population, **by the same separation the mark boundary
already demands of its two**. The test is two-sided: the top two must be separated
as well, so a fit that has merely cut the dahs in half falls back to what it always
returned.

**It does exactly what it was built to do.** Instrumented inside the callsign, the
three centres come out at **14.3, 51.2 and 238.1 ms** and the drop fires. The dit
cluster is found.

**Task 3, `Refine` measured again on top of it.** Last session `Refine` alone read
five characters out of `cw-2026-08-20-014854`, a recording holding no keying at any
pitch, and was withdrawn. **The instruction's guess was right: the invention was the
poisoned dit, not `Refine`.** With the sample corrected first, `014854` returns to
**one** character, which is what it produces today, and `014935` stays at **zero**.

**The nine captures, three ways:**

| capture | today | task 2 | task 2 + `Refine` |
|---|---|---|---|
| `cw-2026-08-17-013347` | 8 | 8 | 8 |
| `cw-2026-08-17-013622` | 0 | 0 | 0 |
| **`cw-2026-08-17-134712`** | **0** | **0** | **0** |
| `cw-2026-08-18-004507` | 25 | 25 | 25 |
| `cw-2026-08-18-003016` | 38 | 38 | **43** |
| `cw-2026-08-18-003126` | 34 | 35 | **35** |
| `cw-2026-08-18-003758` | 14 | 14 | 14 |
| `cw-2026-08-20-014854` | 1 | 1 | **1** |
| `cw-2026-08-20-014935` | 0 | 0 | **0** |

Every floor holds and the two from the 19th produce no more than they do today.

**And both break the suite.** Task 2 alone breaks two easy-tier fixtures:
`exchange-easy` reads `VQCQDEN0CALLN0CALLK` and `tightfist-easy` reads `TEDETESTK`,
where both were whole. HM-DEC-114 makes an easy tier pass or fail, so that is a
hard failure and not a ratchet. Task 2 plus `Refine` additionally breaks
`clean-12wpm`, `clean-18wpm` and `TheBulletinsWordsComeOutAsWords`. Eleven failures
against the expected four.

**Why task 2 breaks them, and it is the finding worth keeping.** A tight fist runs
its elements together, so a merged pair lands at about two dits and a merged run at
about four. **That is a genuine third population of marks, and it is not chatter.**
By length alone the fit cannot tell a merged element from a sliver of noise: on
`134712` the three centres sit at 14, 51 and 238, and on `tightfist-easy` they sit
at a dit, a dah and a merge, and both look identical to a clustering that only
knows how long things are. What separates them is that the chatter is not part of
any character, which is structure, and structure is HM-OPEN-054's ground.

### The dit and the dah, before and after

| capture | dit today | dit task 2 | dit task 2 + `Refine` |
|---|---|---|---|
| `cw-2026-08-17-013347` | 87.0 ms | 87.0 | 100.0 |
| `cw-2026-08-17-013622` | 21.6 | 21.6 | 25.0 |
| `cw-2026-08-17-134712` | 24.5 | 24.5 | 25.0 |
| `cw-2026-08-18-004507` | 54.2 | 54.2 | 60.0 |
| `cw-2026-08-18-003016` | 47.9 | 47.9 | 55.0 |
| `cw-2026-08-18-003126` | 49.8 | 49.8 | 50.0 |
| `cw-2026-08-18-003758` | 34.3 | 19.3 | 19.7 |
| `cw-2026-08-20-014854` | 42.7 | 42.7 | 50.0 |
| `cw-2026-08-20-014935` | 40.9 | 38.4 | 90.0 |

**These are end-of-file figures and that is why most of them barely move.** By the
last sample of `134712` the window is entirely noise again, so the interesting
number is not here but inside the callsign, where the fitted dah goes from **5.2
dits to 4.66** against a hand-decoded truth of 4.24.

**A mismatch reported rather than filled in, and it is the second time.** The
instruction asks for the fitted dah in dits for all nine, before and after. It is
an internal of the mark fit with no reader on it. It was measured inside the
callsign on `134712` and on `cw-2026-08-18-004507`, where the three centres settle
at 18, 60 and 154 ms, and **the other seven were not read out**, because building a
reader for two changes that were then withdrawn was not the best use of the
session. The dit column above is the same quantity one step earlier and is measured
for all nine.

## 2. What Tim should expect

**No. The decoder still does not read `cw-2026-08-17-134712`**, and there is no
text to check against `N4L`.

**But the callsign is now proved in the tree**, and that is the thing this
recording has never had. `N4L` is the first adjudicated ground truth in it, and
every future change can be scored against a known-correct fragment rather than
against a feeling.

**Where it dies now, precisely.** With both changes in, through the whole callsign
the dit reads **35 to 40 ms** against a true 56.3 and coherence is **0.00**, so
`LooksLikeMorse` is false and nothing is emitted. The three-way fit **does** find
the dit cluster at 51.2 ms. What loses it is the next line:
`MedianOfShortCluster` takes the median of every mark below the dit-and-dah cut,
which is 145 ms here, and that **puts all nine chatter slivers straight back in**.
The cluster fit excludes them and the median re-admits them.

**That is one line, and it is the next step.** `MedianOfShortCluster` exists for
HM-DEC-095's reason, that a handful of very short marks survive the gate on any
real signal and an average is defenceless against them. Where a chatter cluster has
already been identified and set aside, the protection is doing the harm it was
written to prevent.

**Build clean, no warnings. 2,076 tests, four failing, and they are the four
expected:**

- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`
- `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`
- `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`
- `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`

Two tests were added, one was replaced, and `src/` is untouched. `ShortestVote` is
still 5, `MaximumRatio` is still 3.8, and the gate, the settled pass and the keying
meter were not touched.

Committed and pushed to `main`.

## 3. What we should do next

- **`MedianOfShortCluster`, with the three-way fit, in one unit.** It is the line
  the callsign dies on and both halves are measured. The easy-tier breakage has to
  be solved in the same unit, because the three-way fit alone causes it.
- **The merged-element problem is the real design question** and it deserves being
  stated as one: a mark twice the length of a dit is either a dah in a fist that
  runs its elements together or two elements the gate joined, and no measurement of
  length alone will say which.
- Record `cw-2026-08-17-134712` in the capture set as an adjudicated fixture whose
  known content is `N4L` at 21.45 to 23.01 seconds. It is the only capture with any
  ground truth in it.
- Keep a real carrier when one is heard. The retired test is now standing on
  synthesized audio, which is the weaker evidence.

## 4. What's blocking us

Nothing blocks the next unit.

**One ask, new this session.**

> **Whether a mark of about two dits is a dah from a fist that runs its elements
> together, or two elements the gate joined, is not decidable from the mark's
> length, and the next unit must not be written as though it were.**
>
> The three-way fit built this session finds `134712`'s three mark populations
> exactly, at 14, 51 and 238 milliseconds, and setting the lowest aside is right
> there. It is wrong on `tightfist-easy`, whose three populations are a dit, a dah
> and a merged pair, and dropping the lowest drops the dits. **Both look identical
> to a fit that only knows how long things are**, which is why task 2 helped one
> recording and broke two fixtures.
>
> **What separates them is whether the mark took part in a character**, and that is
> structure rather than length. It is adjacent to HM-OPEN-054, which is parked, and
> a unit written to "make the dit come from plausible elements" will walk into it
> without noticing.
>
> **Rejected: a length threshold below which a mark is chatter.** That is the sixth
> instance of the error class four rulings have now been spent closing.
> **Also rejected: dropping the lowest cluster only when it is a minority of the
> window**, which was measured against the numbers: the chatter is nine marks of
> twenty inside the callsign, so a minority test would refuse exactly the case the
> fit exists for.

### Asks still outstanding

- **How to tell a merged element from chatter.** First made 2026-08-20, this
  session. Waiting on Tim, or on a unit scoped to know it is the question. Nothing
  is in the tree.
- **The keying meter's provisional thresholds.** First made 2026-08-20. Waiting on
  one evening's roster scored against the `meter` column. **HM-DEC-144 settles the
  half of this that was in doubt**: the meter scored a real station, not a carrier,
  and scored it higher than four recordings that decoded.
- **Whether `SHACK_FACTS.md` still holds that CI-V Transceive is off.** First made
  2026-08-20. Waiting on one capture taken with the radio connected, so the
  `broadcast` line has something to report. The change is in the tree at
  `MainWindowViewModel.BroadcastDuringCapture` and `CivLinkHealth.LastTransceiveUtc`.
- **HM-DEC-130, whether a message too long for one keyer send may be split.**
  First made 2026-08-18. Waiting on the seam between two sends measured into the
  dummy load. `CwMessage.Split` already exists and nothing calls it for this.
- **HM-DEC-098, whether §0.2's first sentence is amended to permit an attended
  automatic transmit cycle on the air.** First made 2026-08-17. Waiting on every
  interlock watched to fire into the dummy load, including the link pulled
  mid-cycle. The cycle is built and is dummy-load only.
- **HM-OPEN-033, the cold-start bin choice and `prosigns-easy`.** First made
  2026-08-18; HM-DEC-129 scheduled it rather than closing it. Waiting on its own
  work order.
- **HM-OPEN-007.** Open and unruled since 2026-08-14, named in HM-DEC-140 as the
  reason the queue's own premise is worth re-testing. Waiting on Tim.

**One item leaves the queue.** Whether `cw-2026-08-17-134712` holds a station or a
carrier: ruled this session and recorded as HM-DEC-144. It is a station and its
callsign is `N4L`.

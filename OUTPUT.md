# Work instruction 013 — bank the evening, then read it again at its own note

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed it — `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does, `CLAUDE.md`'s header says Hamlet and the
solution is `Hamlet.sln`. Branch `main` throughout, three commits, all pushed,
none refused. Version 1.11.9 to 1.11.10 per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected; every number
comes from recordings in the tree.

**No decision was recorded under §12.1.** Everything needing a ruling is in
section 4.

### Where the instruction and the tree disagree

- **The engine baseline was 29 failing of 1674, not 30.** Both flaky rig tests
  passed on the baseline run.
- **The delivery held one file the instruction does not mention**:
  `docs/evidence/band-row-clipped-2026-08-25-1753utc.png`, the screenshot unit
  1.11.9's band-row ruling rests on. Banked with the rest.
- **`013520`'s counts are not the manifest's.** The manifest says 59 characters,
  1 unsure, 157 elements; replayed through the harness it is **60 characters, 5
  unsure, 153 elements**. The manifest's numbers were taken live at the radio's
  own pitch and the sound card's own chunking; these are a fixture replayed from
  600 Hz. Both are true of different instruments and only one can be a floor.
- **`012748`'s "Bug A" reads differently too**: the manifest says 2 characters
  from 113 marks; the replay gives 4 characters from 16 elements seen.
- **Task 3's "six" is now sixteen** — see below. The six was counted before the
  thirteen were banked.
- `CLAUDE_CODE.md` §8 says four sections; its version line still reads 1.3.
- `DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141 or 150, nor
  for Tim's three rulings of 2026-08-25.

### Task 1 — banked, and the two waiting measurements taken

Thirteen captures with sidecars, the manifest, the evening's case list,
`ANALYSIS-2026-08-25-session.md` itself — implemented from quotation by unit
1.11.8 because the document was never in the tree — and the band-row screenshot.
Floors for all thirteen, measured through the harness rather than copied.

**The thirteen pass every sweeping test in the suite unchanged**: chunk-size
invariance across five buffer sizes and both entry points, no impossible
key-down length, no window of an empty band claiming keying. The failure set is
identical to unit 1.11.9's, 29 either side of 115 new theory rows.

**The two reverted fixes' targets, re-measured now that their evidence exists.**
Both faults the analysis named have already been fixed by other work, and a
third is worse than it was described:

| claim | today |
|---|---|
| 10 WPM hypothesised on the 17.9 WPM `021825` | **18 WPM.** Gone. The tone settles at 400.0 Hz against a true 394.0 — six hertz, well inside a bin |
| 32 WPM hypothesised on the 22.5 WPM `012823` | **Withheld.** The guard reports no speed at all rather than a wrong one. Gone in its stated form |
| the pitch chooser misses `012823` by 50 Hz | **Real, and worse than described.** The tracker holds the correct 500 Hz for the first half of the recording and then moves to 450 and stays there, ending 49.8 Hz below the true 499.8. It finds the station and abandons it |

That last row is the live one, and it is HM-DEC-127's territory rather than a
selection fault: a confirmed station is not supposed to be abandoned.

### Task 2 — built, measured, reverted

`AudioTap` already keeps thirty seconds of raw audio, so the re-read needed no
new retention at all. The stream gained a record of the pitch each held hop was
demodulated at; on a **confirmed** measured pitch differing from any held hop's
by more than one coarse survey bin, and only while nothing had settled, the
stream rewound its own clock and re-mixed everything it held.

**Two false starts are worth recording because each looked right.** Comparing
the measured pitch against the *newest* hop's mix pitch makes the re-read never
fire on any capture in the tree — the tracker's bank has usually already walked
there, while the front of the same window is still at 600. And firing on the
first measured pitch rather than a confirmed one costs `031905` and `032129`,
because the first answer is often about to be corrected.

**It works, and it is larger than expected.** The adjudicated corpus moves from
**158 characters of 384 to 167**, and `cw-2026-08-18-003758` gives back
`AA4MP/4 QNIK` **whole** — twelve of twelve, the first time this fixture has
ever produced HM-DEC-126's callsign complete.

**It is blocked by four floors, and every one falls for the same reason: the
decoder emits fewer, better characters.**

| capture | characters | elements | unsure | adjudicated run |
|---|---|---|---|---|
| `cw-2026-08-18-003758` | 63 → **58** | 121 → **124** | 10 → 15 | 9 → **12**, the callsign whole |
| `cw-2026-08-18-004507` | 50 → **49** | 118 → 118 | 1 → 1 | 22 → **28** |
| `unadjudicated/cw-2026-08-22-031948` | 34 → **31** | 114 → **119** | 3 → **0** | 32 → 32 |
| `unadjudicated/cw-2026-08-25-012748` | 4 → **2** | 16 → **4** | 2 → 0 | — |

Three of the four **gain elements while losing characters**, and `031948` drops
its unsure count from three to nought. Only `012748` genuinely regresses.

**And the three captures this unit was commissioned around did not move at all.**
`032113`, `032012` and `032050` read 4, 22 and 17 characters of their adjudicated
lines before and after. Their first measured pitch lands at 21, 12 and 24 seconds
with 48, 14 and 50 characters already settled, so the re-read never fires on
them. The 4-becomes-22 measurement came from decoding a **whole file** at the
station's note; the re-read can only reach audio still held when the pitch is
first measured, and on those three that moment is far too late.

Reverted whole, because the instruction forbids lowering a floor without
qualification. The ruling ask is section 4's first item.

### Task 3 — the meter's remaining failures, diagnosed

**Sixteen recordings hold a station and read no keying, not six**, and they fail
two different ways — neither of which is the element median:

- **Eleven fail on the keying score.** `Score = ElementShare × ElementPurity`
  (`KeyingEnvelope.cs`, the `Score` property; the purity is computed in
  `Measure` as `elements.Count / runs.Count`). **The denominator is every
  threshold crossing regardless of length.** `cw-2026-08-17-013622` produces
  1171 runs and scores a purity of 0.107; `cw-2026-08-25-021825` produces 1553
  and 0.099 — while their element *share* is 0.21 and 0.24, meaning the real
  elements are all present and simply outvoted in the count. **Purity measures
  how tidy the gate is, not how much Morse is there.**
- **Five fail on the swing** that unit 1.11.9 added as the guard keeping the
  silence property, with a score already above the bar: `001831` 19.3,
  `013303` 18.5, `013150` 18.1, `013010` 18.0, `012748` 16.1.

**And the decisive finding: on the full corpus neither quantity separates at
all.** The four recordings holding nothing reach a score of 0.0594 and a swing
of 17.7. Six recordings holding a station score below 0.0594, and
`cw-2026-08-25-021825` — a real station with an eight-second call in it — swings
**12.6 dB, below every empty capture in the tree**. Unit 1.11.9's swing bar of 20
was calibrated on twenty-three captures and now sits inside the overlap.

No contained fix exists that costs zero empty Keying windows. A time-weighted
purity — elements' *duration* over all key-down duration rather than counts —
was measured and moves one capture of thirty-two. **The overlap is the answer**,
as the instruction allowed, and nothing was changed.

### Task 4 — built, measured, not shipped

Validity scored against the fitted clock as a second term: the bonus for
completing a letter the alphabet knows, multiplied by `exp(-off²/2)` on that
gap's own normalised length error, so it pays only where the letter and the clock
agree. It biases segmentation only; a pattern the alphabet does not know still
prints as the placeholder.

| weight | adjudicated characters | success tests + floors |
|---|---|---|
| 0 | 158 | **49 of 49** |
| 1.0 | 158 | 45 of 49 |
| 2.0 | 158 | 42 of 49 |
| 4.0 | 158 | 37 of 49 |

**The count never moves at any weight while the failures climb.** The largest
safe weight is nought, which is the instruction's own exit condition. This form
is in fact worse than unit 1.11.9's flat one, which at least moved the count at
weight 2.0 — by buying four characters of the bulletin with six of `VA3VRR`.
Nothing shipped and nothing left behind.

### The suite

| | baseline | end |
|---|---|---|
| engine | 29 failing of 1674 | **29 failing of 1789** |
| app | 483 passing, 0 failing | **487 passing, 0 failing** |

The failure set is **byte-identical** to the baseline. One app run showed a
single failure that a re-run did not reproduce and the detailed logger did not
name — a third intermittent, recorded rather than diagnosed.

## 2. What Tim should expect at the radio

**Nothing changed in the decoder.** Everything this unit measured about the
re-read, the meter and the cutter is measurement; none of it is in the tree. The
transcript, the meter and the band row behave exactly as they did last night.

**What did change is what the repository can prove.** The evening of 2026-08-25
is banked — thirteen captures, their sidecars, and the analysis document itself —
so the numbers the last three units worked from are now checkable rather than
quoted. Thirty-six recordings now carry floors and twelve carry adjudicated
anchors.

**What will look wrong and is not:**

- **The suite grew by 115 tests and the failures did not move.** The thirteen new
  captures pass everything that sweeps the corpus.
- **`013520` is floored at 60 characters, not the manifest's 59.** The manifest
  measured the radio; the floor measures the fixture. They are different
  instruments and the report says so rather than reconciling them.
- **Three units running have now ended with the headline feature measured and
  not shipped.** Each was blocked by a different guard doing its job: the floors
  here, the success tests in 1.11.9, the silence property in 1.11.8.

## 3. What you should see

**Adjudicated characters, before and after the re-read: 158 of 384 becomes 167
of 384** — and the anchors the twelve success tests guard sum to 153, which is
the number the instruction quotes. Those are three different figures and the
distinction matters: 153 is what is *guarded*, 158 is what is *achieved*, 167 is
what the re-read achieves.

**The three captures the unit was commissioned around did not move:**

| capture | before | after | why |
|---|---|---|---|
| `cw-2026-08-22-032113` | 4 | **4** | first measured pitch at 21.0 s, 48 characters already settled |
| `cw-2026-08-22-032012` | 22 | **22** | first measured pitch at 12.0 s, 14 already settled |
| `cw-2026-08-22-032050` | 17 | **17** | first measured pitch at 24.0 s, 50 already settled |

**Where the nine characters actually came from:**

| capture | before | after |
|---|---|---|
| `cw-2026-08-18-003758` — `AA4MP/4 QNIK` | 9 of 12 | **12 of 12, whole** |
| `cw-2026-08-18-004507` — the ARRL bulletin | 22 of 57 | **28 of 57** |

The re-read helps captures whose pitch is measured early and hurts none, and it
is structurally unable to reach the ones whose pitch is measured late. That is
the honest shape of the lever the last unit found by accident: it is real, and
it is not where the 4-becomes-22 measurement pointed.

## 4. What's blocking us

**May a character-count floor fall where an adjudicated-correctness anchor
rises?**

This is the ruling the re-read waits on, and the numbers are in section 1. On
`cw-2026-08-18-003758` the decoder emits five fewer characters, sees **three more
elements**, and gives back `AA4MP/4 QNIK` whole for the first time. On
`cw-2026-08-22-031948` it emits three fewer, sees five more elements, and drops
its unsure count from three to nought. On `cw-2026-08-18-004507` it emits one
fewer and six more of the bulletin is right.

The floors were built when nothing in the suite could score correctness. Twelve
success tests now can, and on these captures the two guards disagree: the count
says worse and the correctness says better. **A floor on raw character count is
measuring something the project no longer needs it to measure**, wherever an
adjudicated anchor covers the same recording.

*Rejected: shipping anyway.* The instruction forbids lowering a floor without
qualification and §0.4 makes this yours.

*Rejected: tuning the trigger until no floor moves.* Four gates were tried — the
newest hop's pitch, the first measured pitch, a confirmed pitch, and a cap of one
replay. The last two are in the numbers above; capping at one made `012748` worse
still, two characters to nought. Continuing would be fitting the mechanism to the
fixtures, which §12.5 forbids.

*Not rejected: `cw-2026-08-25-012748` is a genuine regression and would need its
own answer* — sixteen elements to four. It is the manifest's "Bug A" capture and
the only one where the re-read destroys rather than trades.

---

**The keying meter's calibration no longer separates, and that is new evidence
against a decision taken two units ago.**

Unit 1.11.9 moved the meter's verdict onto the element median and added the swing
at 20 dB as the guard that keeps the silence property, on the measurement that
the two empty captures swung 14.1 and 17.7 while every recording holding a
station cleared 18.9. **With the thirteen banked that is no longer true.**
`cw-2026-08-25-021825` holds a station and swings 12.6 dB — below all four empty
captures. Four more real captures sit between 16.1 and 17.9, inside the empties'
range.

So the meter is right about twenty of thirty-six and the bar that makes it safe
is now known to sit inside the overlap rather than in a gap. Nothing was changed:
every candidate that admits the sixteen also admits an empty band, and HM-DEC-120
is absolute.

*Rejected: lowering the swing bar.* It would cost empty Keying windows, which
nothing in this project trades.

*Not attempted: a purity that counts time rather than crossings.* It was measured
— it moves one capture of thirty-two — and it changes the meaning of a calibrated
constant, which is a ruling rather than a tuning.

---

**The pitch chooser abandons a station it has already found.**

Task 1's re-measurement of `cw-2026-08-25-012823` shows the tracker holding the
correct 500 Hz for the first half of the recording and then moving to 450 and
staying there. The analysis called this "a 50 Hz miss" and it is not a miss: it
is a confirmed station being left, which HM-DEC-127 rules against. Whatever
displaces it is upstream of everything this unit touched.

*Rejected: acting on it here.* The instruction scopes this unit to the re-read,
the meter and the cutter, and §12.6.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Thirteen inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's three rulings of 2026-08-25, one of which twelve tests rest on.**
5. **The tone tracker** — narrowed by the hold, not closed, and task 1's
   re-measurement sharpens it: on `012823` the tracker leaves a station it had.
6. **The integrator width** — bears on `014113`/`014308`.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named.**
10. **The keying meter** — task 3 acted on it and found the calibration inside an
    overlap rather than a gap.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

New this unit: **may a count floor fall where a correctness anchor rises**,
above; **the meter's swing bar sits inside the overlap**, above; **the pitch
chooser abandons a confirmed station on `012823`**, above.

Still open: **the lock's mixed help**; **the "Hold this pitch" button**; **three
fixtures at accepted cost**; **`001520`'s quadrillions**; **the reference/port
integrator difference**; **`CLAUDE_CODE.md`'s version line**; **an unmeasured
pitch costs `N4L`**; **`014113`/`014308`'s second mechanism**; **the six-hertz
window disagreement**; **the short-character bias**; **the Avalonia geometry
offset**; **`CHANGELOG.md` at 1.9.0 against a version of 1.11.10**; **two flaky
rig tests, and now a third intermittent in the app suite that a re-run did not
reproduce**.

Closed by delivery: **the thirteen captures of 2026-08-25** — the ask that led
four units. Closed by measurement: **the joint cutter's untried half**, which the
table in section 1 settles at a safe weight of nought.

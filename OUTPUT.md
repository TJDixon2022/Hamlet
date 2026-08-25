# Work instruction 011 — nothing stops it decoding silence

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet` and the tree confirmed it: `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` both exist, neither
`CoreHMI.sln` nor `MURC.sln` does, `CLAUDE.md`'s header says Hamlet and the
solution is `Hamlet.sln`. Branch `main` throughout, six commits, all pushed,
none refused. Version 1.11.7 to 1.11.8 per HM-DEC-150.

**Nothing in this report is evidence about the radio.** No rig was connected;
every number below comes from recordings already in the tree.

**No decision was recorded under §12.1.** Everything needing a ruling is in
section 4.

### Where the instruction and the tree disagree

- **The thirteen captures of 2026-08-25 are not in the tree.** Nothing matching
  `2026-08-25` exists outside the graphify cache. Every fallback path applied,
  and tasks 3 and 4 turn on this.
- **`ANALYSIS-2026-08-25-session.md` is not in the tree either.** The instruction
  cites it as the source of the whole unit. Everything attributed to it below is
  taken from the instruction's own quotation of it.
- **The engine suite is 1617 tests, not 1608.** The failure count, 32, matched.
- **The app suite was green at 483, not 1 failing.** The known flaky test passed
  on every run this session.
- **`■` is the placeholder and the tree is right**, as the instruction said. It
  is `Marked` writing `#`, which `CwProbabilisticStream.Character` renders as
  `MorseAlphabet.Unreadable`.
- **`CLAUDE_CODE.md` §8 says four sections** and its version line still reads 1.3.

### Task 1 — banked

Nine captures committed with their sidecars, the proposed W1AW truth beside them
marked as a proposal. The floors harness guarded five recordings of twenty-three;
it now guards all twenty-three, re-measured through its own drive path, with
elements and unsure counts recorded beside the characters.

Two old floors had gone stale enough to guard nothing: `cw-2026-08-18-004507` sat
at 25 while producing 50, and `003016` at 38 while producing 57.

**The unsure count is printed and deliberately not asserted, and that is a
departure from the instruction, stated rather than made quietly.** A `>=` floor
on unsure would forbid the decoder ever becoming more certain; a `<=` ceiling
would forbid it ever admitting doubt it currently hides, which HM-DEC-048 ranks
above a confident wrong letter. Neither is a property this project wants held.
The number's job is the trade, and printing it beside the characters is what
makes a trade visible.

**Two drive paths disagree, which is a finding rather than a rounding.** Fed hop
by hop through `Process`, or through `Listen` with a buffered source, the decoder
returns different counts on nine captures, and on `cw-2026-08-22-032113` it
tracks a different note: 650 Hz against 500. The floors are set from the lower.

### Task 2 — the gate, shipped

`CharacterMargin` moves from nought to one, in the units the window guard already
uses. Nought was where keying and silence explain the audio equally well, and
that reasoning holds; what it misses is that a letter beating silence by a
whisker is not a letter somebody sent.

**One is measured against the only thing that binds it.** The weakest character
on any recording holding words anybody has checked is **1.047**, a lone `E` on
`cw-2026-08-24-012403`. Next weakest is `VA3VRR` at 1.480 and the ARRL bulletin
at 1.635.

**A higher floor is not available on this quantity in any normalisation, and the
reason matters more than the number.** On `cw-2026-08-17-013347` the adjudicated
`VA3VRR` scores between 1.5 and 6.5 while the ninety-six characters of soup ahead
of it score between ten million and three hundred million. The soup outranks the
callsign by seven to eight orders of magnitude on the very quantity a gate sorts
by. Two normalisations were built and measured before the simpler form was taken:

| normalisation | what it does |
|---|---|
| divide by the window's own likelihood ratio | cuts `VA3VRR` **whole** at a floor of 0.1 and keeps every character of the soup |
| divide by a per-element-count median | moves the binding constraint not at all — `012403`'s `E` binds in that form too, at 0.1008 |
| per hop, as the quantity already is | the floor above |

**E and T measured separately, as instructed.** T is never at risk: on the
corroborated captures the weakest T is 4.133, four times the floor, and none
falls below it. E is the whole question and it comes out clean — the three
corroborated captures carrying E's keep every one (weakest 1.480, 3.628, 1.047),
while 31 soup E's go. The seven E's suppressed on a corroborated capture are all
on `cw-2026-08-17-134712`, in the trailing run that follows the `N4L` rather than
belonging to it, scoring 0.076 to 0.89.

### Task 3 — built, measured, reverted

The clock was made to eat only spans a decode had already found characters in: a
span-restricted `CwUnitEstimator.Measure`, evidence spans carried across reads,
and spans merged into runs of keying across every silence the decoder has a name
for.

**The first form was worse and the reason is structural.** A character's own span
holds its marks and the gaps inside it, so a set of bare character spans holds
element gaps and no others; asking for the short cluster of a set with only one
cluster splits it and takes the lower half, so the gap reads short and the speed
reads fast. `cw-2026-08-22-032012` went from 18 words a minute to 25 on a
bulletin whose own proposed reading puts it at 17 to 19.

**Merging fixed the bias and left nothing to show for it.** Every reported speed
came back identical, with one exception, `cw-2026-08-23-002016` moving 25 to 24,
which nobody can adjudicate. Against that: **ten banked floors broke, including
`cw-2026-08-17-134712`, which carries the adjudicated `N4L`.**

Reverted whole. The failure it was commissioned to fix — 32 words a minute
hypothesised on a 22.5 station, 10 on a 17.9 — lives entirely in the two captures
that are not here.

### Task 4 — built, measured, reverted

A keying-quality score was added to the survey's bin choice: `Duty` on
`KeyingCandidate`, a `LooksLikeAFist` test of ratio 2.4–3.6 and duty 18–55 %, and
a lexicographic `Beats` preferring a fist over a louder thing that is not one,
without touching the tracker's displacement rules.

**On the only captures in the tree with an independently documented carrier it is
anti-correlated with being the station.** The W1AW proposal puts all seven at
499.8–499.9 Hz, so the 500 Hz bin is the station:

| bin | duty | ratio | passes the fist band |
|---|---|---|---|
| the station, 500 Hz | 0.47 – 0.70, median 0.57 | 2.71 – 3.54 | **2 of 6** |
| everything else | 0.25 – 0.78, median 0.42 | 2.54 – 3.76 | **14 of 22** |

The chosen pitch moved on three captures and the net was a swap, not a gain:
`031905` moved onto the documented carrier and `031948` moved off it, leaving two
of seven at 500 Hz either way. And **five of the six candidates admitted while
quieter than the band beside them pass the fist band**, so the rule would promote
bins below the noise floor over a station fifty decibels above it.

**The band encodes "somebody calling", not "somebody keying".** Duty is a fact
about what is being sent: a bulletin is continuous traffic and a call is not.
Reverted whole.

### Task 5 — shipped, and the diagnosis

**Duty is on the capture sheet**, measured over the audio in the file at the
pitch the decoder was following, to a tenth of a per cent, and absent with a
reason where no pitch was measured.

**`competing` finds nothing because of what it searches, not because of its
thresholds.** `CwToneTracker.cs:583` iterates `_survey.Candidates()`, so the
entire universe of possible competitors is the set of bins the survey would be
willing to *track a station on*: `CwToneSurvey.cs:478` requires two separable
mark clusters and at least `MinimumMarks` = 8 clean marks
(`CwToneSurvey.cs:160`), and `Judge` at `CwToneSurvey.cs:824` then requires a
ratio inside 2.5–3.8, a dit of 25–200 ms, and a cluster separation of at least
4.0 (`CwToneSurvey.cs:137`). A second station in a pileup — overlapping the
first, sending fragments, its ratio distorted by the overlap — meets none of that.

Measured on the three pileup captures, at sixty sample points each:

| capture | admitted candidates per sample | competitor ever seen | rejected for being within 125 Hz | rejected for being 20 dB down |
|---|---|---|---|---|
| `cw-2026-08-23-001831` | 0.30 | 0 | **1** | 0 |
| `cw-2026-08-23-001952` | 0.07 | 0 | **0** | 0 |
| `cw-2026-08-23-002016` | 0.07 | 0 | **0** | 0 |

**Neither threshold is the binding constraint.** On recordings of many stations
calling at once the survey almost never holds two candidates at the same moment,
so there is nothing for either floor to reject. That sharpens the unruled
two-stations-closer-than-125-Hz ask: fixing `competing` does not require touching
125 Hz.

Diagnosed only, as instructed. Nothing was changed.

### Task 6 — the options table

In section 4. Nothing was built.

### Task 7 — not dropped; half shipped, half withheld

There was room, so it was worked rather than dropped.

**The sweep now looks exactly where the decoder looks**, 300 to 900 Hz, taken
from `CwToneTracker` rather than carried as a second copy. It ran 400 to 1200
while the tracker runs 300 to 900, which is the radio's own CW pitch range (§4),
so on `cw-2026-08-22-031905` the decoder tracks 300 Hz and the sweep could not
examine that pitch at all, and on `032050` it tracks 325. At the top it answered
1000 Hz on `cw-2026-08-23-001520`. The old floor was set against this receiver's
low-frequency rumble; that concern is not dismissed, it is **unexercised** — no
candidate below 400 Hz wins on any capture in the tree, and the tree holds no
recording of a station down there.

**The record no longer prints a key-down length nobody could send.** The figure
was the middle of every threshold crossing, and noise crosses a threshold
hundreds of times, so on a recording holding a real station the chatter
outnumbered the elements several to one: 331 runs of which 69 were elements on
the capture carrying `VA3VRR`, plain median **4 ms** against an element median of
88; 176 of which 27 on the one carrying `N4L`, **3 ms** against 55. A dit at
sixty words a minute is twenty. The sheet now carries the middle of the key-downs
that could be elements, and says so plainly where none could.

**The verdict is deliberately left on the old figure, and that is the withheld
half.** Moving it was built and measured: the meter goes from **10 recordings
right of 23 to 17**, seven fixed and none broken. It costs the silence property —
in single six-second windows `cw-2026-08-20-014854` scores 0.11 to 0.20 against a
bar of 0.10, and reads **Keying** on a capture holding no station. That is
section 4's first item.

### The suite

| | start | end |
|---|---|---|
| engine | 32 failing of 1617 | **29 failing of 1661** |
| app | 483 passing, 0 failing | **484 passing, 0 failing** |

Against the starting tree, four failures are fixed and one is new:

- fixed: `NothingTheDecoderWasSureOfIsWrong` for `clean-12wpm`, `clean-18wpm` and
  `prosigns-18wpm`, and
  `ScannerEndToEndTests.ADwellReachesTheDecoderAndTheVerdictCarriesItsConfidence`
- new: `CwFixtureTests.EveryRecordingGivesBackTheShareItShould(prosigns-18wpm)`

## 2. What Tim should expect

**On a quiet frequency, where last night the terminal filled with `E`s and `I`s,
it now fills with `■`.** That is the whole visible change and it is the one the
unit was for. The decoder has not become quieter about stations; it has stopped
naming letters it cannot back. Where it hears something it cannot account for it
says so instead of guessing, and §0.0 ranks a marked unknown above a wrong letter.

**On a rag chew nothing changes.** Every recording in the tree holding words
anybody has checked reads exactly as it did: `VA3VRR`, `AA4MP/4 QNIK`, the ARRL
bulletin and `DE KD0UN KD0UN K` are unchanged character for character.

**The capture sheet gains a `duty` line**, and its `keying` line now shows a
key-down length that could be Morse, or says no key-down was element length.

**What will look wrong and is not:**

- **`cw-2026-08-17-134712`'s unsure count goes from 10 to 42.** Those are the
  leading and trailing runs of `E` around the `N4L`, not the callsign. The
  callsign is untouched.
- **`prosigns-18wpm` now gives back nothing at all**, and it is a red test. Its
  span evidence is exactly nought — HM-OPEN-018's digital silence, which no
  receiver produces — so the old rule was printing letters backed by no
  measurement. The same fixture's `NothingTheDecoderWasSureOfIsWrong` went green
  in the same change. Two clean fixtures were already accepted cost for this
  reason; this is a third.
- **The keying witness still says "no keying" on recordings that plainly hold
  stations**, six of twenty-three. The repair that fixes seven of them is
  measured and withheld, for the reason in section 4.
- **Nothing from tasks 3 and 4 is in the tree.** Both were built to the
  instruction, both failed their own acceptance, and both were reverted rather
  than left half-live.

## 3. What you should see

**The gate's effect, per capture.** 81 letters become placeholders across the
corpus. "Real" is an adjudicated callsign, a banked floor or a W1AW proposed
line, and each capture is labelled as which:

| capture | holds | letters → `■` | what they were |
|---|---|---|---|
| `cw-2026-08-17-013347` | **`VA3VRR` adjudicated** (HM-DEC-145) | **0** | — |
| `cw-2026-08-18-004507` | **the ARRL bulletin**, corroborated | **0** | — |
| `cw-2026-08-24-012403` | **`DE KD0UN KD0UN K`**, the control | **0** | — |
| `cw-2026-08-17-134712` | **`N4L` adjudicated** (HM-DEC-144) | 8 | `HEEEEEEE`, the run *after* the callsign |
| `cw-2026-08-18-003758` | **`AA4MP/4 QNIK`** (HM-DEC-126) | 13 | `EEEEEEEHHEEEE` — the callsign is unchanged |
| `cw-2026-08-22-031838` | W1AW proposed line | 23 | soup; the `AND` it reads survives |
| `cw-2026-08-22-031905` | W1AW proposed line | 2 | `E`, `0` |
| `031948`, `032012`, `032050`, `032113`, `032129` | W1AW proposed lines | **0** each | — |
| `003016`, `003126`, `cw-2026-08-23-001520` | nothing adjudicated | **0** each | — |
| `cw-2026-08-17-013622` | nothing adjudicated | 8 | `EEEEIIEI` |
| `cw-2026-08-23-001831` | nothing adjudicated | 7 | `TTEEF?E` |
| `cw-2026-08-23-001952` | nothing adjudicated | 10 | `IESIEIIEHE` |
| `cw-2026-08-23-002016` | nothing adjudicated | 10 | `ISESEESISS` |
| both empty captures | nothing | **0** — they emit nothing at all | — |

**Not one real word was touched.** The two suppressions closest to real content
are on captures carrying corroborated readings, and in both the reading itself is
unchanged: `N4L` and `AA4MP/4 QNIK` come back character for character with their
surrounding soup marked instead.

The one character worth naming is `012403`'s: its unsure count went 0 to 1 in the
settled pass. It is a lone `E` between the second `KD0UN` and the closing `K`,
where the control's own reading has no letter at all. It is treated as real
anyway and the floor sits under it, because nobody has adjudicated it.

## 4. What's blocking us

**Should the keying witness's verdict move onto the element median, at the cost
of the silence property on the live path?**

The meter is wrong on thirteen of the twenty-three recordings in the tree, and
one number is most of it: the verdict tests a median taken over every threshold
crossing, and noise crosses a threshold hundreds of times. Moving that test onto
the median of the key-downs that could be elements takes it from **10 right of 23
to 17**, seven fixed and none broken, and every one of the seven is a recording
holding a station that the meter currently calls empty.

It costs the silence property. Measured whole-file the empty captures are safe —
`014854` scores 0.059 and `014935` 0.020 against a bar of 0.10 — but the meter
runs live on six-second windows, and in those `014854` reaches 0.11 to 0.20 and
then reads **Keying**. `ItSaysNoKeyingOnThePressesThatProducedNothing` fails.

*Rejected: shipping it anyway.* The instruction says do not trade the silence
property, without qualification, and a meter that announces a station on an empty
band is the failure this application exists to prevent.

*Rejected: raising the keying score bar to compensate.* That is tuning a
threshold until a red test goes green, and it trades against the meter's stated
purpose — the bar is deliberately near the noise because a meter that misses a
station is the failure that costs an evening.

*Not rejected, not attempted: a window-length or run-count requirement, so a
six-second window has to hold as much evidence as a whole file before the element
median means anything.* That is the shape of an answer and it needs its own
measurement rather than a guess at the end of a unit.

The measured half is in the tree: the record no longer prints an impossible
length, and the sweep can now see where the decoder looks. Only the verdict waits.

---

**The joint cutter — task 6's options table. Nothing built.**

The boundary cutter is the dominant residual on good signals and the analysis
brackets it from both ends: at 18 words a minute the gap classes are perfectly
separable (0.81 / 3.36 / 13.9 units) and the cutter still cut inside characters,
which is a decision-rule fault; at 30 words a minute element and character gaps
are four milliseconds apart and no per-gap rule can work.

**What the tree says about the mechanism, checked rather than assumed:** the
decoder already scores gaps jointly, through the `Kinds` table and the Viterbi
path in `CwProbabilisticDecoder.Decode`. What it does *not* do is score character
validity. `MorseAlphabet.Lookup` is called in `Spell`
(`CwProbabilisticDecoder.cs:1328` and `:1348`), which walks the **already-chosen**
winning path, so the path is committed before anything asks whether the letters it
spells exist. That is the gap all three options address or decline to.

| | what it is | what it costs | what it threatens |
|---|---|---|---|
| **A. Joint DP over a short window** | Fold character validity into the path score: a segmentation spelling letters the alphabet knows scores better than one that does not, decided over a window of several characters rather than one gap at a time, against the fitted clock. | The largest of the three. A new term in the scoring loop, a window length to choose, and a weight for validity against timing — and that weight is a constant nobody has measured. | **Prosigns and callsigns.** `/`, `?`, `<BT>`, `<AR>` and a callsign like `AA4MP/4` are exactly the strings a validity term punishes, and this project reads callsigns for a living (HM-DEC-073). It also risks the decoder preferring a *plausible* reading to a *measured* one, which is §0.0's own failure mode wearing a better score. |
| **B. An improved local rule** | Keep the per-gap decision and make it better: fitted classes with a hysteresis, or a refusal to cut where a gap sits between two classes, marking the character instead. | Small. One comparison changes. | Almost nothing — and it buys almost nothing at 30 words a minute, where the two classes are four milliseconds apart. A rule that cannot work on the fast end fixes the case already working. |
| **C. Do nothing** | Leave it. | Nothing. | The residual stays. On the evidence in the tree the cutter is not the top defect: the gate this unit shipped removed 81 invented letters, and the six recordings the keying witness still calls empty are a bigger visible fault than a mis-cut word. |

**A recommendation rather than a ruling.** B is not worth its own unit — it
cannot touch the fast end, which is where the failure was measured. Between A and
C the honest position is that **A should not be built until there is something to
measure it against.** Every fixture in this tree that would judge it is either
synthetic, unadjudicated, or one of four corroborated readings the current cutter
already gets right. A validity term that improves a number on unadjudicated audio
and quietly costs a prosign would look exactly like a success.

**What would change that:** the W1AW seven adjudicated. Seven captures of known
text at a known speed, with numbers and punctuation in them, is precisely the
corpus a validity term has to survive. That adjudication is the blocking
dependency, and it is already waiting on you.

---

**HM-DEC-126's own reopening condition has been met and nothing acted on it.**

That ruling closed HM-OPEN-026 on 2026-08-18 saying `cw-2026-08-18-003758` is
unobtainable, with the words "**this entry reopens if the file appears**". The
file was committed on 2026-08-20 in `6f93c32`, two days later, and has been in
the tree for five days. It is named by the floors harness and by
`ANALYSIS-cw-emit-decision-2026-08-24.md`, which reads `AA4MP/4 QNIK` off it.

This matters beyond bookkeeping, because HM-OPEN-026 recorded the gap that file
would fill: **this suite has no regression test for a success at all.** Every
ratchet in it is a ratchet on a failure getting less bad, so nothing in it can
tell a repair from a coincidence. This session's own measurement shows
`AA4MP/4 QNIK` reads correctly today and survives the new gate character for
character, so the first such test is now buildable.

*Rejected: building it in this unit.* Asserting that text is adjudicating it, and
adjudication is yours (§12.5). The floor is banked meanwhile.

---

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Thirteen inbound, none
ruled, the oldest open since 2026-08-14. Fourteen consecutive units have now
worked beside rulings they cannot read.** Two are new this unit and are the two
above; the eleven below are the instruction's own list, unchanged.

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
5. **The tone tracker** — narrowed by the hold; the 2026-08-25 table shows the
   hold is what made the decoder work.
6. **Whether the integrator ships at 45 Hz or 30 Hz** — bears on `014113` and
   `014308`.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named** — task 5's diagnosis bears
   on it and narrows it: the 125 Hz floor rejected one candidate across 180
   sample points on three pileup captures, so it is not what is stopping
   `competing` finding anything.
10. **The keying witness is wrong more often than right** — now 13 of 23 in this
    tree, and the repair for 7 of those 13 is measured and withheld above.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

New this unit: **whether the keying witness's verdict moves onto the element
median**, above; **HM-DEC-126's reopening condition has been met**, above.

Still open from earlier units: **the lock helping sometimes and hurting
sometimes**; **the "Hold this pitch" button**; **two clean fixtures at accepted
cost — now three, `prosigns-18wpm` having joined them**; **`001520`'s
quadrillions**; **the reference/port integrator difference**;
**`CLAUDE_CODE.md`'s version line**; **an unmeasured pitch costs `N4L`**;
**`014113`/`014308`'s second mechanism**; **the six-hertz window disagreement**;
**HM-OPEN-060**; **the short-character bias needs a per-character expectation**;
**the W1AW seven await your adjudication — the proposal is now in the tree, and
task 6's options table names that adjudication as its blocking dependency**.

Also noted and not acted on (§12.6): **`CHANGELOG.md` stops at 1.9.0** while the
version is now 1.11.8, and its own convention paragraph still describes
HM-DEC-063's meaning of minor and patch, which HM-DEC-150 superseded. **The two
drive paths disagree** about counts on nine captures and about the tracked note
on one. **Six of forty-five survey candidates are admitted while quieter than the
band beside them**, one at −5.0 dB of lift.

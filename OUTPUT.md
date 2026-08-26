# Work instruction 015, amended — the band row back where it was ruled

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed it — `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does, `CLAUDE.md`'s header says Hamlet and the
solution is `Hamlet.sln`. Branch `main` throughout, one commit for the new work,
pushed, not refused. Version 1.11.12 to 1.11.13 per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

### What was actually new

**`WORK_INSTRUCTIONS.md` still says 015 and is an amendment of the order this
session already executed**, not a new unit: twenty-four lines added and two
changed, all of them a fourth part to task 3. Tasks 1, 2, 4, 5 and 6 are
untouched from the earlier run — banked nothing, measured and rejected the
squelch, shipped the margin log, hid the sweep, dropped task 6 — and their
results stand as reported. **This report covers task 3 part 4 and restates the
rest only where the amendment changed what should be said.**

The file still carries no `ISSUED:` line, which §9.6 requires precisely so a
session can tell a fresh order from an amended one by date rather than by
diffing it against its own last commit.

### Task 3 part 4 — the band row

**All three faults were real, all three are fixed, and none of them was
visible to the test that had been guarding this area.**

**The order.** Tim's ruling of 2026-08-26 is title, line of Shakespeare, then
bands. Measured before: title at y 23, line at y 51, band row at **y 249** —
about two hundred pixels down, with the rig readout in between. The row moves to
the top of the canvas grid and the readout strip below it. Measured after: **y
87**, directly under the line, identical at both widths.

**`10 m` cut on its right.** Not a window-width fault: the row had 624 pixels of
horizontal headroom. The cut is inside the card. Measured per label, asking each
one what width it wanted against what it was given:

| card | width | label wants | label had | short by |
|---|---|---|---|---|
| `10 m` | 58 | 40 | **27** | **13** |
| `15 m` | 65 | 40 | 34 | 6 |
| `17 m` | 67 | 40 | 36 | 4 |
| `20 m` | 70 | 40 | 39 | 1 |
| `30 m`, `40 m`, `80 m` | 77–93 | 40 | 46–62 | — |

The four narrowest cards were all clipping their own names and `10 m` worst,
which is exactly `10 n` on the screen. **The cards are not shrunk to make room**
— the widths carry the wavelength — so both ends of the span are scaled by the
same factor, 58 to **71** and 93 to **114**, and every card stands in the same
proportion to every other as before. Seventy-one is measured rather than chosen:
forty for the label and thirty-one for the padding and the day-night icon that
share its line. After: every label gets exactly what it asked for, short by
nought, and the whole row is 691 pixels against 1168 available at the default
width.

**The badge showing only its bottom sliver.** The best-bet badge is drawn over
its card with a negative top margin so it costs the row no line of its own. The
clipping ancestor, named as the order asks: **the band row's own `ItemsControl`**,
whose bounds are exactly one card tall — 43 pixels — with the badge reaching nine
pixels above its top. Everything above the card's top edge was cut away. The row
keeps its height and stops clipping what deliberately overhangs it; the first
clipping ancestor above a card is now the **window** itself, at both widths.

**Nothing was verified by hit-testing.** Unit 1.11.9 asserted every card answered
a click at four widths while recording an unexplained disagreement between the
headless geometry and what is drawn — and the real window showed all three of
these faults anyway. Four tests now assert the geometry that causes each fault:
the row's position relative to the two things ruled above it (and that it is
*directly* under, not merely below — an ordering assertion alone would have
passed throughout the fault), each label against the width it asked for, the
row's right edge against the window, and the first clipping ancestor above a card.

### The suite

| | before | after |
|---|---|---|
| engine | 28 failing of 1831 | **28 failing of 1831**, unchanged |
| app | 489 passing | **497 passing, 0 failing** |

The engine is untouched, which is what an app-only change has to be.

## 2. What Tim sees at the radio

**The bands are the first thing under the byline again** — title, line of
Shakespeare, bands — rather than two hundred pixels down past the readout.

**`10 m` says `10 m`.** So do `15 m`, `17 m` and `20 m`, which were also cutting
their names by six, four and one pixel and would have gone the same way as the
window narrowed. The cards are bigger and their proportions are identical: `80 m`
is still exactly as much wider than `10 m` as it was.

**The best-bet badge is whole.** It hangs above its card by design and the row was
cutting off everything above the card's top edge.

**And the three things from the earlier run of this unit still stand**: the keying
sweep is off the terminal, its go-and-check-the-radio advice retires wherever a
tone is found, and the capture sheet carries the second-best margin beside
`spanLlr` with both clamped.

**What has not changed, and is worth repeating because it is the thing that was
asked for and did not survive:** the squelch does not ship. A quiet frequency
still does not stay quiet on screen, and old soup still shows as brightly as
current copy. The reasons are in section 4.

**What will look wrong and is not:**

- **The band row is visibly larger.** The cards grew by a factor of about one and
  a quarter so the narrowest could hold its own label. The ratio between them is
  unchanged.
- **The keying sweep panel is absent.** It is a setting that ships off, not a
  deletion, and it still writes to every capture sheet.

## 3. What you should see

**The band row, measured at both widths, before and after:**

| | before | after |
|---|---|---|
| row's top edge | y 249 | **y 87** |
| distance below the byline | 198 px | **36 px** |
| `10 m`'s label | 27 px of the 40 it wanted | **40 of 40** |
| cards clipping their names | **four** | **none** |
| first clipping ancestor above a card | `ItemsControl`, 43 px tall | **the window** |
| row width against the window at 1200 | 576 of 1168 | 691 of 1168 |

**The squelch's before and after, unchanged from the earlier run: 167 of 384
adjudicated characters either way, because it does not ship.** Every anchor is
green, `021629`'s exchange is present, `013520` and `013303` are byte-identical,
and the four empty captures are silent.

## 4. What's blocking us

**The squelch still needs an axis, and this amendment did not change that.**

Restated because it is the unit's own headline and nothing since has moved it.
Measured rolling at the tracked pitch on the decoder's own envelope, duty does not
separate: the four recordings holding nothing read median 0.57–0.63 by one
definition and 0.40–0.42 by the other, both mid-distribution, while `021629` —
whose `559 559 IN MI MI` must survive — reads 0.227, below the plan's own floor.
The fist ratio separates on medians and cannot be bounded: at the plan's band it
costs 46 adjudicated characters and silences `VA3VRR` and `N4L` outright; widened
until the anchors return it still costs thirty.

**The margin logged in task 4 is the candidate replacement** and one evening of
captures with it in the sheet gives a real distribution to set a bound from.

---

**The transcript's dimming still has no trigger that exists.**

Task 3 parts 1 and 2 define a separator inserted when the squelch has held ten
seconds, and dimming everything before the most recent separator. With no squelch
there is no separator, so the transcript renders exactly as it did — old soup as
bright as current copy, which is half of tonight's original complaint.

**The available alternative, unbuilt because it was not authorised**: dim
everything except the most recent stretch, using `CwTranscript.RecentCharacters`,
a constant of 240 already in the tree for exactly the notion of "recent". No
squelch, no separator, nothing deleted, everything still selectable. §0.0 makes
the screen yours and the mechanism is a different one from the ruling's, so it is
an ask rather than a change I should make.

---

**The headless-versus-real geometry offset is still unexplained, and this unit
shows what it costs.**

Unit 1.11.9 recorded that `TranslatePoint`, `TransformedBounds` and a hand-summed
layout chain all agree with each other and all disagree with the hit test by
about thirteen pixels vertically. It worked around it and left the cause open.
Tonight three faults sat in that same area behind a green test.

Measuring render geometry rather than hit-testing found all three immediately, so
the working rule is now **assert the geometry that causes the fault, not that a
point reaches a control**. But the offset itself is still unfound, and until it
is, any test in this area that depends on where a point lands is suspect.

*Rejected: chasing it in this unit.* The amendment scoped this to the band row,
and §12.6.

---

**Three of the last four units have had their headline feature measured and not
shipped, and the pattern is worth naming.**

1.11.8's clock diet and fist band, 1.11.9's validity term, and now this unit's
squelch. Each was reverted by a guard doing its job — floors, success tests, the
silence property — and each was proposed from a measurement taken through a
different instrument from the one it would run in. That is HM-DEC-119's lesson
and it has now cost four attempts.

**What would change it**: a shack plan whose numbers are computed by the same
code path the fix would run in, or delivered as a file the session can re-measure
rather than quoted. `BUILD_SESSION_2026-08-25.md` is still not in the tree.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Thirteen inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's five rulings of 2026-08-25, tonight's adoption of the build plan, or the
   band-row ruling of 2026-08-26 this unit acts under.**
5. **The tone tracker** — the confirmation rule's ask stands from 1.11.11; task 6
   was dropped, so its selection half is unmeasured.
6. **The integrator width** — bears on `014113`/`014308`.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named.**
10. **The keying meter** — hidden behind a setting; the rebuild is its own unit.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

New this unit: **the squelch needs an axis and neither candidate is one**;
**the transcript's dimming needs a trigger that exists**; **the build plan is not
in the tree**; **the headless-versus-real geometry offset is what hid three
visible faults behind a green test**.

Still open: **the lock's mixed help**; **the "Hold this pitch" button**; **three
fixtures at accepted cost**; **`001520`'s quadrillions**; **the reference/port
integrator difference**; **`CLAUDE_CODE.md`'s version line, and its orders'
missing `ISSUED:`**; **an unmeasured pitch costs `N4L`**; **`014113`/`014308`'s
second mechanism**; **the six-hertz window disagreement**; **the short-character
bias** (task 4 logged its replacement quantity); **`CHANGELOG.md` at 1.9.0
against 1.11.13**; **four intermittents**; **the whole-file second pass**; **the
confirmation rule cannot admit an intermittent station**; **tonight's three
captures were never delivered**.

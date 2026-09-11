# Work instruction 308 - the whole world on one map, true paths across it, and the CQ list starts nudging

**READ IN THIS ORDER.**

A. **The phase goal - FT4 does everything FT8 does.** This unit changed the shared
   FT8/FT4 surface again, so everything below changes for both modes at once. **No
   step of the phase moved.**

B. **Step 4 and its exit criteria** - pressing FT4 tunes and decodes; the panel, the
   conversation, the ring, the filters, the tooltips, the ledger and the right-click
   menu working unchanged; one click, one transmission; a whole exchange from one
   right click at the bench. **None was measured tonight**, and steps 5 and 6 are you
   at your own radio, which no session can meet. Step 4 stays `partial`.

C. **The report last, and section 4 raises 4 items** on top of a carried queue of ten.

```
UNIT:       308 - complete at task 6 of 6, none dropped - 2026-09-10 20:25
PHASE GOAL: FT4 does everything FT8 does. Steps 0 and 3 are done; 1, 2 and 4 are
            partial; 5 and 6 are you at your own radio.
UNIT GOAL:  Put every station Hamlet can decode somewhere on the map, draw the path
            the signal actually takes, and mark tonight's callers who would open
            something new.
ADVANCED:   no - the shared FT8/FT4 surface again, and steps 5 and 6 cannot be met
            by a session at all.
NUMBER:     coverage 40.4% -> 93.7% of the globe; path 1 straight line -> up to 181
            sampled points in 1 or 2 runs; marked CQ rows 0 -> capped at 2
DRIFT:      13 consecutive units without advance  (was 12, carried in the instruction)
```

## 1. What Claude did

Six tasks of six, none dropped. Development machine, prompt claimed `PROJECT: Hamlet`,
tree confirmed it. Branch **`main`**, five commits, **all pushed, none refused**. Root
version 1.12.271 to **1.12.272**. Nothing under `src/Ft8Sharp/` touched, nothing that
keys the transmitter touched, no package added.

**Every appearance claim in this report is computed, not seen.** Nothing in this
repository can look at a picture. What is asserted is arithmetic, control placement in
a visual tree, and the words a property returns.

### Task 1 - the trace

**1a. The unit number, and a bigger finding underneath it.** `PHASE_OUTCOME.md` carries
`UNIT 300` through `UNIT 305` and **no 306, no 307 and no 308**. So 308 is free. But it
also means **unit 306 appended no outcome entry** - its task list had no such task - and
**unit 307 never ran at all**: the git log goes straight from unit 306's report to this
unit's first commit, `docs/carry-forward-tests.txt` does not exist, and nothing in the
tree carries a restored starter card or a *booked implies present* guard.

**1b. The asset.** All four files landed. `world-flat-relief.png` is **698 x 381, RGBA,
486,177 bytes**, and hashes to `b3164c27…45796` exactly. `flat-map-anchors.csv` has 18
rows plus a header.

**1c. The map code.** `AzimuthalMap.Settings` is at `:52` as stated. **`Place` is at
`:127` and `FromCentre` at `:173`**, not `:98` and `:144` - unit 306 inserted
`NorthPolar` above them. **`Place` still returns offsets from the centre**, which task 2
depended on and which held. `Ft8GlobePlot`'s four framing constants are still unread by
anything in `src/`.

**1d. `ACHIEVEMENTS_PHILOSOPHY.md`**, restated:

- **§3.1** - a card does not exist on the screen until something next to it has been
  earned. **Absent, not dimmed**: an unearned thing is not shown greyed out, it is not
  shown.
- **§3.2** - earning one thing reveals more than it fills, so the set grows as he works
  rather than shrinking toward a finish line.
- **§3.6** - the CQ list itself should point at the callers who would open something,
  so the achievements are a reason to be on the air rather than a page to visit.
- **§3.7** - restraint. **If everything is marked, nothing is**, and an unmarked station
  must not read as worthless, because the contact he most wants may be an ordinary
  domestic one.

**1e. The achievements subsystem, and the finding that reshaped task 5.**

- An achievement is `AchievementCard` (`src/Hamlet.App/ViewModels/AchievementCard.cs`),
  with a `Kind` of `Record` or `Challenge`.
- **Challenges are computed from the log at read time**, not stored:
  `AchievementChallenges.For(log, grid)` returns **eight** cards, each with `Earned`.
- **Place cards are records of what has been worked.** `AchievementScreen.BuildPlaces()`
  iterates `_log.Continents` - the continents he **has** worked - and within each, the
  entities he has worked. **So the unearned country set does not exist as objects
  anywhere to be enumerated.** That is the finding, and it is better found here than in
  task 5.
- **It is derivable without the screen**, and that is what task 5 does:
  `DxccContinents` knows every entity and its continent, `DxccPrefixes.EntityOf`
  resolves a callsign, and `AchievementLog` knows what has been worked. **Visible** is
  an unworked entity on a worked continent; a **door** is any entity on a continent
  never worked. That maps onto your own words about South America exactly.
- Unit 279's fade is `DigitalDecodeRow.RowOpacity` at `:349` - `0.55` when worked.
- Unit 278's cached read is `_workedBefore ??= ReadWorkedBefore()`, and
  **`RefreshWorkedBefore()` is its invalidation**, called after a log write. Task 5
  hooks there.
- **The two inherited reds in `TheAchievementsScreenTests` are still red**, confirmed
  and left alone.

**1f.** The CQ card behaviour unit 305 built is intact - `ThePressingOfCqTests` is
green. There is no unit 307 starter card to confirm.

**1g. The carry-forward list does not exist**, so the fallback list ran. **All green
before this unit changed anything**: 21 in the app project across
`TheGlobePlacesStationsTests`, `TheGlobeLineTests`, `TheReadinessHoverTests`,
`ThePressingOfCqTests`, `TheSlotClockTests` and `BindingHealthTests`; 34 in the engine
project across `TheAzimuthalAssetTests` and `AzimuthalMapTests`.

### Mismatches against this instruction

Reported, not repaired.

1. **Unit 307 never ran.** Section 5 says it did and that its work may have moved
   things. Nothing of it is in the tree.
2. **`Place` and `FromCentre` are at `:127` and `:173`**, not `:98` and `:144`.
3. **`AzimuthalImage` is not "five numbers and a hash"** - it carries ten fields: a
   resource, a hash, width, height, two centre pixels, a centre latitude, a scale, a
   rotation and a rim.
4. **`Ft8GlobePlot.Caveat` no longer exists.** Unit 306 replaced it with `OffTheMap`.
5. **The globe was not inside the `i` tooltip.** It was behind a **separate globe glyph**
   beside the `i`, with its own tooltip. The blocker is the same and the fix is the same.
6. **Swapping the two scales moves Tokyo 73.4 px, not more than 100.** A swap moves a
   point by the difference of the scales times its own coordinates - 0.5091 × 139.65
   across and 0.5091 × 35.68 down. **The guard is kept and its floor is set from the
   measurement**, at 50 px; 73 px is a fifth of this picture's height.
7. **The Tokyo and Auckland vertex counts differ.** Measured: Tokyo **107 + 70 = 177**
   and Auckland **167 + 8 = 175**, against the instruction's 111 + 70 and 173 + 8. The
   instruction's own arithmetic is inconsistent too - it says four samples are dropped
   *and* that 111 + 70 is 181. **The segment counts and the midpoint are exactly as
   stated.**
8. **`TheFitGuardAsksAboutTheGridTheSendIsOnTests` is in the engine test project**, not
   the app one. Confirmed still red there, 1 of 5.
9. **Three test types had to be moved onto the new map** -
   `TheGlobePlacesStationsTests`, `TheGlobeLineTests` and `Unit299GlobeTests` - because
   they assert the polar behaviour task 2 replaces. Sydney is now placed, which is the
   point of the unit; what was a southern-hemisphere refusal is now an Antarctic one.
10. **This instruction names section 1 *What was done* and section 4 *What is
    blocking us*.** `tools/arbiter/validate-output.bat` requires the canonical
    headings - *What Claude did* and *What's blocking us* - and refuses the file
    without them. The canonical ones are used above.
11. **`OffTheMap` said "from the North Pole to the equator"**, which stopped being true
    the moment the map changed. Corrected.

### What was built

**Task 2.** `FlatWorldImage` and `FlatWorldMap.Relief` carry the six numbers and the
SHA-256 together, refuse a file they do not recognise, and place a station in absolute
pixels or refuse. **Sixteen anchors land within 0.0001 px**; Midway and McMurdo are
refused rather than clamped. The polar record and its seventeen tests are untouched.

**Task 3.** `GreatCirclePath` samples 180 segments, projects each, splits at the
antimeridian and ends a run where a sample has no place.

**Task 4.** The map is a row on the card face. The globe glyph is gone. Each marker
answers for itself through `Ft8GlobeControl.WordsAt`, station first where the two
overlap, and null rather than empty so no blank box follows the pointer across an ocean.

**Task 5.** `NudgeSet` derives what is in play; the row carries `Nudge`, `IsNudged`,
`NudgeIsDoor`, `RowLift` and `NudgeTip`; the panel marks, caps and invalidates.

**Task 6.** `NudgeWords` - a visible card is named, a door names nothing.

### Two failures that were my own fixtures, not the code

**The entity table calls the United States *United States of America*** and my first
fixture spelled it *United States*, so a worked country read as unworked. The helper now
takes callsigns and reads the entity name out of the table, which is the §12.5 rule
applied to my own test. And a loop produced the time `02:11:60`. Both fixed in the
fixtures.

### Tests

**No suite was run.** Every run was filtered by exact name, foregrounded, with a
480-second timeout, and the status was written after each.

| type | project | count | watched failing |
| --- | --- | --- | --- |
| `TheFlatWorldAssetTests` | engine | 5 | yes, at compile |
| `TheGreatCirclePathTests` | engine | 7 | yes, at compile |
| `TheGlobeOnTheCardFaceTests` | app | 4 | yes, at compile |
| `TheCqListNudgeTests` | app | 9 | yes |
| `TheNudgeHoverTests` | app | 5 | yes, at compile |
| `TheGlobePlacesStationsTests`, `TheGlobeLineTests`, `Unit299GlobeTests` | app | 14 | moved onto the new map |
| `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint` | app | 1 | re-run after both markup moves |

## 2. What the owner should expect

**The map has the world on it now.** You have been told 40.4% and the honest figure for
this picture is **93.7% of the globe** - 98.8% of the northern hemisphere and 88.6% of
the southern. **Everyone the last report named as having nowhere now has somewhere:**
Nairobi, Buenos Aires, Sydney, Cape Town, São Paulo, Lima, Auckland and Honolulu.

**Two places still have nowhere, and that is the whole list.** **Antarctica**, below
63.79°S - McMurdo is off the bottom of the file. And **a 4.5° strip of longitude west of
175.52°W**, which takes Midway, Baker and Howland. A station there gets no marker, no
path, and a card that says so in words.

**The line is not a line any more.** It is the great circle, sampled and drawn, and it
splits where it crosses the date line rather than running back across the picture. From
your grid to Tokyo it goes over northern Alaska, which is where the signal actually
goes.

**The map is on the card, not behind a glyph.** Hover either marker and it tells you who
it is; the map itself is simply there.

**Some rows in the CQ list now carry a quill.** At most two at a time. A station that
gets one keeps it while he is on the list.

**What will look wrong and is not.**

- **An unmarked row is not a lesser row.** Nothing about it is dimmed or annotated by
  this feature; the only fade on that list is the worked mark, which is a different fact.
- **A marked row never names an area you have not opened.** If the quill has a ring
  round it, the hover says something new would open and deliberately does not say what.
- **The pale curves on the ocean are artwork.** They are not a graticule, nothing reads
  or aligns to them, and Hamlet draws no tick or degree label of its own.
- **`assets/world-coastline.svg` and the polar map are both still in the tree** and
  nothing draws either. Left standing as instructed.

## 3. What you should see

**1. The map, on the card face.** The flat relief picture, whole, with two markers on
it. Your own marker is a ring at your grid; the station's is filled. `FN00DJ` places at
**178.3, 133.1** and Dublin at **315.0, 102.4** - both to within a ten-thousandth of a
pixel of anchors measured off the picture.

**2. The path.** A dashed arc of up to 181 points. To São Paulo and Cape Town it is one
run of 181. **To Tokyo it is two runs, 107 + 70**, split at the date line, and its
midpoint is **66.63°N, 155.18°W** - northern Alaska - placing at **37.9, 70.9**. The
straight line between the two markers passes **351.2 px** away from that. That number is
the difference between a picture that is true and one that is decorative.

**3. The marker words.**

```
You, in grid FN00DJ.
K9XP, in grid EN52. 540 miles west-northwest of you.
```

**4. The `i`, unchanged from unit 306** - five rows, none over 66 characters.

**5. A marked CQ row.** A quill in decode green, with an orbit ring where what would
open is an area you have never opened. The hover reads:

```
Costa Rica · new country
new area · would open something you have not seen yet
```

The second is a placeholder and its wording is a question for you.

## 4. What's blocking us

**1. The task 4 author's proposal, reproduced in full.**

> **Author's proposal, not Tim's ruling.** Unit 306 raised a blocker: the two sets of
> marker words are built and asserted, and they cannot be attached, because the globe
> lives inside the `i` tooltip and a tooltip inside a tooltip is not a thing. Three ways
> out were offered and none chosen, because each moves a picture on his screen.
> **The proposal: the map moves onto the card face, where it can own its own hit
> targets, and the `i` goes back to being text only.** The reasons: the map is the thing
> he will actually look at, and a picture that has to be hovered to be seen is a picture
> he will not see; the `i` was cut to five short rows by unit 306 and reads well as
> text; and this is the only one of the three routes that lets the marker hovers exist
> at all rather than deferring them again. Overruling it means the map goes back inside
> the `i` and the marker words stay unshown.

Built that way. One correction to it: the map was behind a **globe glyph** beside the
`i`, not inside the `i` itself. The blocker and the fix are unchanged.

**2. The door sentence, which is a placeholder.** `new area · would open something you
have not seen yet`. Wording is the product (§3.5) and no ruling has been given on this
one. It names no area, says nothing about being behind, and promises nothing.

**3. The licence of `assets/world-flat-relief.png` is unknown.** It is in the tree by
your ruling and carries no watermark, but no origin or licence is recorded, and Hamlet
is GPL-3.0. See `assets/PROVENANCE.md`. **Raised, not resolved, and no licence claim was
written anywhere in the tree.**

**4. Unit 307 never ran, and nothing in `PHASE_OUTCOME.md` records 306 either.** The
phase record has a two-unit hole in it.

### Asks still outstanding

Carried per HM-DEC-139.

1. **Does the transmission record ask the radio whether it keyed?** Unit 303's proposal,
   still yours: `Played` stays a statement about what the audio path did; a second,
   separate fact says what the radio did, read from `1C 00` and `15 11`, which Hamlet
   already polls four times a second; **unknown** where the radio does not answer.
   Touches what the display asserts, so yours without exception (§12.1).
2. **Nothing in this repository can look at a picture.** Ten units have now reported
   every appearance claim as computed rather than seen. Real pixels want
   `Avalonia.Headless.Skia`, and **a package is yours, not a session's** (§0.4). **This
   unit makes it sharper again**: it draws a photograph, two markers and a
   hundred-and-eighty-point polyline, and no test in this tree can see any of it.
3. **Three inherited reds, never chased.** Two in `TheAchievementsScreenTests` -
   `WsprIsNotAFirstAnybodyCanEarnAndTheCardSaysSo` and `TheWindowDrawsEverySixRows` -
   and one in `TheFitGuardAsksAboutTheGridTheSendIsOnTests`, **which is in the engine
   test project**. All three confirmed still red and left alone.
4. **The two-answer CQ rule.** The author's proposal from unit 305, not yet your ruling:
   the first answering station adopts the CQ card; a second gets a card of its own.
5. **Where the explanatory hover wording lives, if anywhere.** `Ft8ContactCard.Closing`
   is uncalled and left standing.
6. **Where the globe lives.** **Answered as the author's proposal and built that way** -
   item 1 above.
7. **The full-disc polar image. Superseded** by your ruling of 2026-09-10, carried out
   here. The polar record, its image and its seventeen tests stay in the tree.
8. **`Ft8GlobePlot`'s unused framing constants** - `MapWidth`, `MapHeight`, `Margin`,
   `SmallestFrame`. Still unread by anything in `src/`. Left standing.
9. **Unit numbering.** `PHASE_OUTCOME.md` had no `UNIT 308`, so the fold is clean - but
   it has no 306 or 307 either.
10. **The licence of `assets/world-flat-relief.png`** - item 3 above.

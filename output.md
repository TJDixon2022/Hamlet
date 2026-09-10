# Work instruction 301 — the hint mark, the achievement mark's size, and a real map

**READ IN THIS ORDER.** The goal task stopped where task 4 tells it to: **the map
image is not in the tree.** Everything else is built, and the map's arithmetic is
built and proved so that only the picture is missing.

A. **The phase goal — FT4 works exactly the way FT8 does.** This unit advanced no
   step of it. It repaired the hint mark, grew the achievement mark, and built the
   projection a real map needs.

B. **Step 4 and its exit criteria** — pressing FT4 tunes and decodes; the panel, the
   conversation, the ring, the filters, the tooltips, the ledger and the right-click
   menu all working unchanged; one click, one transmission; a whole exchange from one
   right click at the bench. **None was measured tonight.** Step 4 stays `partial`,
   and its second criterion now describes tooltips that can actually be hovered,
   which is the first time that has been true.

C. **The report last, and section 4 raises 4 items.** One is a thing only you can
   supply and it blocks the rest of the goal task. The other three are carried.

```
UNIT:       301 — complete at task 6 of 6, none dropped — 2026-09-09 22:54
PHASE GOAL: FT4 works exactly the way FT8 does.
UNIT GOAL:  The i mark can be hit, the achievement mark fills the status bar, and
            the globe shows a real map with the station in the right place.
ADVANCED:   no — no phase step moved. Step 4 stays partial: nothing tuned, nothing
            transmitted, no exchange run at the bench.
NUMBER:     the i mark's hit target, before and after
            BEFORE: 0 of 9 points across the mark were live. Not a thin outline —
            NOTHING. The centre, the corners and the edges all found whatever was
            behind it.
            AFTER: 9 of 9, across all 42 uses in eight windows, with the drawing
            untouched at 14 by 14.
DRIFT:      5 consecutive units without advance  (was 4, carried from unit 300)
```

---

## 1. What Claude did

**Complete. Six tasks of six, none dropped, including the named drop candidate.**
Development machine, prompt claimed `PROJECT: Hamlet`, branch `main`, three commits,
all pushed. Root version 1.12.263 to **1.12.264**, bumped once. **No file under
`src/Ft8Sharp/` was touched**, and nothing here reaches a send path.

**Nothing in this report is evidence about the radio.** Nothing was tuned, nothing
was transmitted, and no audio was captured.

**Nothing was recorded to `DECISIONS.md`.** The three judgements made on this
session's authority are in `PHASE_OUTCOME.md`'s entry and listed below.

### Verifying the instruction against the tree

Every claim held except one, and that one is the goal task.

| The instruction says | Measured |
|---|---|
| `HintMarkControl` draws a ring with no fill, a bare `Control` | **True.** `DrawEllipse(null, pen, …)` |
| It is used across the application | **True — 42 uses in 8 files.** Settings 23, MainWindow 10, Achievements 4, five windows with 1 each |
| Unit 299's globe is a `Border` with a transparent background | **True** |
| Unit 300 put the quill in the status bar | **True, at 20 × 20** |
| The belt ring sits beside it | **True** |
| Unit 299's globe hover draws a projected coastline | **True** |
| `assets/world-coastline.svg` becomes unused | **FALSE, and it is this unit's doing.** Task 3 stopped, the hover is untouched, so it is still drawn. Not deleted |
| Root version after unit 300 | **1.12.263**, read not assumed |
| **Tim supplied a real azimuthal map image** | **IT IS NOT IN THE TREE.** Nothing resembling it anywhere in the repository |

**And one mismatch in the instruction's own diagnosis, which made the fault worse
than described.** It has him "trying to hit a one-pixel outline". Measured, the ring's
outline was not live either: **nine points across the mark, the centre included,
found nothing at all.**

### Task 1 — the `i` mark can be hit

**Three answers, two of them measured wrong before the third.**

1. **A transparent fill on the drawn ring.** Reaches the picture and not the pointer.
   The mark stayed dead.
2. **Deriving from `Border`, then `Panel`.** Not available at all: **both seal
   `Render`**, so neither can also draw a ring.
3. **A transparent rectangle over the control's own bounds** — the same shape a
   `Border` paints for its background. This is what registers, and it is one line.

**So the target is the mark's whole box rather than the disc inside the ring**, larger
by the corners. That is the forgiving direction for somebody who has been aiming at
this and missing.

**The drawing is untouched**: same 14 × 14, same ring as an outline, same glyph. An
empty mark still measures to nothing, so a mark holding no sentence is still not
hoverable.

### Task 2 — the achievement mark fills the bar

Measured off the realized window, not read off the markup.

| | Before | After |
|---|---|---|
| Status bar, outside | 40.0 px | **40.0 px — unchanged** |
| Status bar, inside | 26.0 px | 30.0 px |
| The quill | 20.0 px | **26.0 px** |
| The belt ring | (its text) | **26.0 px, the same resource** |

**The bar did not grow.** The room came out of padding: the bar's own vertical padding
6 → 4 and the mark button's 3 → 2. Unit 141 spent a session winning 150 pixels back
and this spends none of them.

**The size is a resource, not a number typed twice.** `HmStatusMarkSize` is read by
both the quill and the belt ring, so they cannot drift into one looking noticeably
smaller. The orbit already follows the mark's bounds and was measured doing it.

### Tasks 3 and 4 — the projection is built; the image is not here

**No map was drawn.** Two attempts have failed and substituting a third is what the
instruction forbids. **The globe hover is exactly as it was.**

What is built is the half the image does not block: `AzimuthalMap` places a dot, and
its three configuration numbers are one record in one place carrying a comment saying
they describe the shipped image and must change with it.

### Task 5 — what was verified and what could not be

Below, in section 3.

### Task 6 — the outcome entry

Appended through `tools\arbiter\outcome-append.bat`, filed as **`UNIT 301 - STEP 4`**
with nothing renumbered by hand.

### The three things decided on this session's authority

1. **The hit target is the mark's whole box**, not the disc inside its ring.
2. **The test carries its own control for the harness.** A red that is not evidence
   is worse than no test.
3. **The projection is computed in a rearranged but algebraically identical form**,
   because the published one is numerically unstable at the antipode — measured, not
   assumed.

---

## 2. What the owner should expect

**The `i` works.** Point anywhere at one and the sentence comes up. Everywhere in the
application — Settings has 23 of them, the main window 10 — because it was one change
to the control they all share. It looks exactly the same as it did.

**The feather is big enough to notice.** It went from 20 pixels to 26 in a bar whose
inside is 30, so it fills the strip instead of floating in it, and the contact belt
ring beside it is the same size so the two read as a pair. **The status bar is the
same height it was.**

**The map is not a map yet, and that is because it is not here.** The globe hover is
unchanged: it still draws the coastline unit 299 built. Nothing was faked in its
place.

**What will look wrong and is not:**

- **The globe still looks like the old blobby coastline.** Deliberate. Task 4 says
  that if the image is missing, leave the hover alone and say so rather than draw
  something.
- **`assets/world-coastline.svg` is still there and still used.** The instruction
  expected it to go unused; it has not, for the same reason.
- **The character-ceiling test is red on two of five.** Both are the Digital tab,
  both were red before this unit, and that is `HM-OPEN-089` — ask 1, which the
  instruction parks.

**Build:** succeeded, 0 warnings, 0 errors. **Tests:** 23 constructed in this
instruction across three classes, **23 of 23 green**, each filtered by exact name and
foregrounded. **No suite was run** (HM-DEC-155). Gates re-run because this unit
touched their surfaces: `BindingHealthTests` green, `EveryResourceKeyResolvesTests`
green, unit 300's 13 still green.

**Pushed:** three commits to `main`. Nothing of this unit's is uncommitted.

---

## 3. What you should see

### 1. The hit test at the mark's centre, and how far it reaches

```
BEFORE                                   AFTER
points tried : 9                         points tried : 9
points missed: 9                         points missed: 0
   3.5, 3.5  found nothing
   3.5, 7    found nothing
   7,   7    found nothing               transparent Border, centre : Border
   10.5,10.5 found nothing               HintMarkControl,    centre : HintMarkControl
```

**It reaches 42 uses across eight windows** — `SettingsWindow` 23, `MainWindow` 10,
`AchievementsWindow` 4, and one each in `ContactLogWindow`, `DecisionLogWindow`,
`FavoritesWindow`, `LogContactWindow` and `RigDiagnosticsWindow`. One line in one
control.

**And the harness is carried in the test rather than trusted.** `InputHitTest` here
answers about whichever headless window it believes is on top: the same assertion
passes alone and fails in a class, and **a plain transparent `Border` — the known-good
shape — reports `nothing` under exactly those conditions too.** So the `Border` sits
in the same window as the mark and is hit at the same moment. Where the harness cannot
see the `Border`, it is not asked about the mark. The proof that cannot lie is the
deterministic one, watched failing: with the line removed, *nothing the mark draws
covers its own box*.

### 2. The status bar's height and the mark's size in it

```
status bar, outside : 40.0      (unchanged by this unit)
status bar, inside  : 30.0      (was 26.0 — padding 6 -> 4)
the mark            : 26.0      (was 20.0)
its button          : 30.0      (padding 3 -> 2)
the belt ring       : 26.0      (the same resource)
headroom            :  2.0 above and below
```

The orbit follows it: the ring is **18, 24 and 30 px across** at mark sizes of 20, 26
and 32.

### 3. The three projection numbers, and the points they were checked against

**The three numbers are `CentreLatitude`, `CentreLongitude` and `RimRadiusPixels`**,
in `AzimuthalMap.Settings`, one record in one place. The rim is the antipode, so the
third is half the width of the circle the map is drawn inside, in that file's own
pixels. **They describe one particular picture and must change with it** — replace the
image and leave them, and every dot lands somewhere plausible and untrue.

**They are not filled in, because the image is not here.** The figures below use a
test centre of 40°N 75°W at a 1,800 px rim, which is exactly ten pixels to the degree
so a reader can check the arithmetic by hand.

**Checked against `GridPath`, which shares no line of code with the projection** and
computes great-circle distance and initial bearing by its own spherical trigonometry.
On this projection those two quantities are exactly what a dot's position means.

| Station | On the map | `GridPath` says |
|---|---|---|
| his own centre | 0.0 px → 0 miles | 0 |
| **transatlantic:** Ireland | 459.0 px → **3,171** | **3,171** |
| **transatlantic:** Portugal | 497.6 px → **3,438** | **3,438** |
| across the pole: Japan | 978.4 px → 6,760 | 6,760 |
| south: Argentina | 761.6 px → 5,262 | 5,262 |
| near antipodal: Perth | 1,680.4 px → 11,610 | 11,610 |
| **antipodal:** 40°S 105°E | **1,800.0 px, on the rim** | **12,437** |

**Every bearing agrees to 0.00 degrees**, across Ireland 49.7, Portugal 69.0, Japan
332.2, Argentina 166.0, Perth 309.2 and Alaska 326.4. That is the check that catches a
mirrored sign: a distance alone would be identical if east and west were swapped.

**One defect was found by that checking, in the published formula itself.**
`k = c / sin c` divides by zero at the antipode; written literally, the point opposite
the centre landed **off the rim by more than twelve miles' worth of pixels** while
every real station was exact. What is used instead is the same equation rearranged —
the dot at distance `c` on its own bearing — which is algebraically identical with
nothing dividing by nothing. The antipode now lands on the rim to a thousandth of a
pixel.

### 4. What could not be verified, and why

**Nothing about appearance was verified, and ask 2 stands unchanged.** Nothing in this
repository can look at a picture. That was re-tested in unit 300 and the wall is the
same: `RenderTargetBitmap.Render` and `Save` both return without complaint and no file
appears on disk.

So, precisely:

- **The dots' correctness is arithmetic and it is checked.** Distance and bearing
  against independent trigonometry, to the mile and to a hundredth of a degree.
- **The map's appearance is not verifiable from here at all**, and there is no map to
  look at in any case.
- **The hit target's pointer behaviour was verified only in isolation.** Run alone the
  harness answers correctly; run in a class it cannot answer about a known-good
  `Border` either. What is verified in every run is the deterministic property: the
  mark now paints a 14 × 14 transparent fill over its own box where it painted none.
- **That the `i` is easier to hit in the hand was not verified.** It cannot be from
  here. **You are the instrument for that**, as you were for the comparison that
  produced this ruling.
- **The status bar figures are real measurements** off the realized window — layout
  ran and reported them — but **whether 26 pixels stops the feather looking lost is
  your judgement**, not something measured.

---

## 4. What's blocking us

### 1. The map image — this one is yours, and it blocks the goal task

**Nothing else stops task 3.** The projection is built, the three numbers have a home,
and the arithmetic is proved. What is missing is the picture.

**Drop the image into `assets/`** and supply, or let a unit measure off it:

- the **centre latitude and longitude** the map is drawn about, and
- the **pixel radius from its centre to its outer rim**, which is 180 degrees of arc.

`assets/azimuthal-map.md` carries this, what is already checked, and the five steps to
finish when it arrives. **Any raster format Avalonia can load will do.**

**No ruling is needed** — this is a file, not a judgement.

### 2. `Ft8Sharp` did not move

Stated because the instruction requires it. No file under `src/Ft8Sharp/` was read,
edited or built.

### Asks still outstanding

Carried verbatim per HM-DEC-139.

1. **`HM-OPEN-089` — the Digital tab is over its character ceiling and the figure
   moves.** *First made 2026-09-09, unit 300.* Two runs of the same tree an hour apart
   read **1281 and then 1293** while every other row held still, so **something on that
   tab composes text from the clock.** Two ceiling tests are red. **A ceiling set
   against a moving number cannot be set correctly**, so raising the row would only
   move the failure. **Waiting on:** somebody finding the moving string first.
   **Where it sits:** `tests/Hamlet.App.Tests/Views/HowMuchTheApplicationSaysTests.cs`,
   rows unchanged. **Tonight it moved again**, the working figure reading 1259 against
   1233 last night, which is that issue's own symptom appearing a third time.

2. **Nothing in this repository can look at a picture.** *First made 2026-09-09, unit
   300.* Four units running have now reported every appearance claim as computed
   rather than seen. Real pixels want `Avalonia.Headless.Skia`, and **a new package is
   a dependency decision rather than a session's** (§0.4). **Waiting on:** your ruling
   on whether that dependency is worth it. **Where it sits:** nowhere — no package has
   been added.

3. **A US state, from callook.** *First made 2026-09-08, unit 297; hit again by 298 and
   299.* Hamlet cannot name a US state: neither the grid square nor the DXCC entity
   carries one, so every place name for a US station stops at *the United States*.
   **Waiting on:** the parked callook instruction. **Where it sits:** nowhere.

**And one that is dropped rather than carried.** Unit 299 asked you to hover the `i`
mark and the globe side by side and say whether one was easier to hit. **You did, you
ruled, and task 1 is that ruling built.** The ask is closed and is not carried
forward.

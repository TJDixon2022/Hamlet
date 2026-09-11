# Work instruction 313 - the popup stops blurring, and the panel scrolls

**READ IN THIS ORDER.**

A. **The phase goal - Hamlet works PSK31 the way it works FT8.** A third digital mode
   with the same two cards, the same one-click exchange, the same log and the same
   achievements, on a modem Hamlet builds itself.

B. **The PSK31 phase's step 1 is next and this unit does not touch it.** Step 1 is a BPSK
   demodulator and varicode decoder proved against fixtures. **This unit advances no step
   of the phase**: it is carried repair of two FT8 and FT4 faults, on the two surfaces
   PSK31 will inherit, and the drift count rises by one honestly.

C. **The report last, and section 4 raises 5 items** on top of a carried queue of eleven,
   three of them from unit 312.

```
UNIT:       313 - complete at task 4 of 4, none dropped - 2026-09-11 09:41
PHASE GOAL: Hamlet works PSK31 the way it works FT8 - the same cards, the same one
            click, the same log, on a modem this project writes itself.
UNIT GOAL:  Two things on the shared digital screen are the wrong size. A picture
            enlarged past what it contains, and a panel that cuts cards off instead
            of letting him reach them.
ADVANCED:   no - carried repair. It advances no step of the PSK31 phase and says so.
NUMBER:     popup magnification 3.98x -> 2.00x; the For You viewport 0 px of scroll
            -> 224 px of viewport over a 1,748 px extent, every card reachable;
            version 1.13.0 -> 1.13.1
DRIFT:      1 consecutive unit without advance  (this one; the phase's count was 0)
```

**Every appearance claim in this report is computed, not seen - with two exceptions, and
they are worth having.** Nothing in this repository can look at a picture, so the layout
numbers below come from standing the real window up headless and reading back what the
layout did. **But this session could look at two actual images**: Tim's own screenshot,
which is how the green glyphs were identified and their colour sampled; and the map
bitmap cropped to the rectangle the new code computes, which is how the popup's contents
were checked. **Section 3 says which is which.** Nothing here was seen rendered by the
application, and nothing here is evidence about the radio - this machine has none.

## 1. What Claude did

**The gate passed on all four checks.** `SHACK_FACTS.md` present,
`src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` present, no `CoreHMI.sln`, no
`MURC.sln`, root at `C:\Source\HamLet`. Branch **`main`**, five commits, each pushed
before the next task started. Version **1.13.0 → 1.13.1**.

**The three questions, one line each.**

- **The popup's magnification before the cap was 3.98x.** `F4DIA` was framed at 180.7 by
  95.3 source pixels and drawn at 720 by 379.5, and unit 310's zoom floor permitted
  **4.13x** at its worst, which a same-state contact hit exactly. The author measured 3.9x
  from a 185 by 99 crop; the code's own numbers are 3.98 and 180.7 by 95.3.
- **The horizontal truncation was neither the same cause nor a different one: it is not
  the app.** The card measured **301 px wide inside a 331 px panel**, so nothing was
  clipped sideways. The screenshot is 1,207 px and cuts the rig display at the top right
  as well, which is not in that panel at all. **It is the screenshot's own edge.**
- **The green circled glyphs are unit 309's achievement quills, and they work.** A green
  ring with the quill vane inside, on `LZ1KU KN32` and `J38DX FK92`, both unworked
  countries. **Decode green `#3B6D11` exactly** - 21 pixels of it, sampled straight out of
  Tim's screenshot. He was told that list was a total failure. It is not: the marks are on
  it and they are the ruled colour.

### Task 1 - the trace

`UNIT 313` is appended to `PHASE_OUTCOME.md`, which is the PSK31 phase's file, as carried
repair advancing no step. **The hole ask 7 names is real and was not back-filled**: this
phase's record starts at unit 312, and 306, 307, 309 and 310 are in the FT4 phase's file,
which is in git history rather than at the root.

**Sixteen named types ran filtered and foregrounded before anything changed, all green.**

**The popup frame, with file and line.** `Ft8GlobePlot.Opened()` builds the box over every
sampled point and both markers, adds a 6 % margin, then `Fitted` applies
`ZoomFloorShare = 0.25` and clamps into the bitmap. **The floor was the wrong instrument**:
a quarter of a 698 px file is 174.5 px, and 174.5 px enlarged into a 720 px popup is 4.13
times. Set tight enough to stay sharp it would be a floor of the whole file, which is no
zoom at all.

**The resampling was already smooth.** No `RenderOptions` is set anywhere in `src/`, so
`DrawImage` took Avalonia's default filter, and the blur on the screenshot is a smooth one
rather than blocks. **It is now asked for explicitly** rather than left to a default that
can change under the project.

**The For You panel.** No `ScrollViewer` anywhere above the cards - the decoded list to its
left and the conversation rows beneath it both had one, and the cards were the one region
that did not. The cards control sat in an `Auto` row, so it asked for its whole content
height and got it: **1,748 px inside a panel 264 px tall, with five of six cards starting
below the bottom edge.** The sixth card is drawn and unreachable rather than not drawn.

### Task 2 - the zoom is capped

The frame is unit 310's box, unchanged and untouched, then **grown about its own centre
until neither dimension would magnify past the cap**, then slid back inside the picture.
Growing can only ever add, so it cannot put a sampled point outside the frame - and that
is asserted anyway, over eight paths, and after the slide as well.

| case | before | after |
| --- | --- | --- |
| `F4DIA` France | 3.98x | **2.00x** |
| `PY2ABC` Brazil | 2.36x | **2.00x** |
| `ZS1ABC` South Africa | 2.03x | **2.00x** |
| `W2ABC` a hundred miles away | 4.13x | **2.00x** |
| `JA1ABC` Tokyo, date line | 1.03x | 1.03x, whole world |
| `ZL1ABC` Auckland, date line | 1.03x | 1.03x, whole world |

**The cap needs the popup's size and does not hold one.** `OpenFrameFor` takes the box it
will be drawn in; the control passes what it was offered and remembers it, so the measure,
the render and the marker hovers ask one question and get one answer. The number itself is
`Ft8GlobePlot.ZoomCap` and lives in exactly one place.

### Task 3 - the panel scrolls

The cards now sit in the same kind of container the two lists either side of them already
had. **Two things were wrong, and the second only appeared once the first was fixed:**
giving the cards the star row cured the `Auto`-row overflow and left the viewport at
**110 px**, because the conversation region below is a star row too and a star row takes
its share whether or not its child is drawn. Its height is now zero while the messages are
shut, and the viewport went **110 → 224 px**.

**The scroll does not jump.** Parked half way down, a seventh station answering leaves the
offset exactly where it was - not thrown to the top, not thrown to the bottom.

**What it does not do, and why that is reported rather than fixed.** The cards below the
new one slide down by one card's height, because a new station is put at the top of this
panel. **Holding the card he was reading still is not achievable from where this task
sits**: the panel calls `DigitalCards.Clear()` and rebuilds every card on every slot, so
after a rebuild there is no *same card* for a scroll anchor to hold on to - a new object
with the same callsign is not the object the viewport was pointing at. The instruction
allows exactly this answer, and it is raised as ask 10 rather than solved by inventing an
ordering rule.

### Task 4 - what the two fixes look like

Taken rather than dropped. It is section 3.

### Four mismatches with the instruction, reported and not repaired

1. **The worked example's four edges do not match; its centre matches exactly.** The
   instruction gives the 2.0x crop as x 78.5 to 439.5, y 22.1 to 214.6. The code gives
   x 79.0 to 439.0, y 18.4 to 218.4. **Both centre on 259.0, 118.3.** The difference is
   the popup: the example is worked for about 722 by 385, which gives a crop of 361 by
   192.5, and `MainWindow.axaml` offers the map 720 by 400, which gives 360 by 200. Same
   centre, same cap, a different box - so the test asserts the centre and the cap rather
   than four numbers that bake a ceiling in.
2. **The magnification is 3.98x and the crop 180.7 by 95.3**, not 3.9x from 185 by 99.
   Close, and not the same numbers; the code's are in the report because they are the
   ones that were changed.
3. **The horizontal truncation is the screenshot's own crop**, not a fault of either kind
   the instruction offered. Measured: the card is narrower than the panel.
4. **One card does not fit the panel.** A conversation card with its map is 293 px and the
   panel gives 220 px of card room at a 1400 by 900 window, so a scroll bar appears with a
   single conversation. Not a fault and not something the instruction anticipated; the
   test that assumed one card needs no scroll was asserting something untrue and was
   rewritten to assert the empty case and the policy instead.

### Tests, all filtered by exact name, foregrounded, 480 s timeout

**No suite was run. No `dotnet test` on a whole project, and nothing was backgrounded or
polled** (HM-DEC-155).

| Test type | Result |
| --- | --- |
| `ThePopupZoomIsCappedTests` (new, task 2) | **6 of 6**, watched failing first |
| `ThePanelScrollsTests` (new, task 3) | **5 of 5**, watched failing first at five of five |
| `Unit313TraceTests`, `Unit313PanelTraceTests` (new, task 1) | 3 of 3, 1 of 1 |
| `TheMapOpensTests` | 9 of 9, before and after |
| `ThePanelHoldsThemAllTests`, `TheCqReceiptTests` | 6 of 6, 6 of 6 |
| `TheMapRowFitsTests`, `TheGlobeOnTheCardFaceTests` | 5 of 5, 4 of 4 |
| `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint` | 1 of 1, re-run after every markup change |
| `ThePressingOfCqTests`, `TheGlobeLineTests`, `TheCqListNudgeTests` | 5 of 5, 3 of 3, 9 of 9 |
| `TheNudgeHoverTests`, `TheConnectionLineReadsTests`, `ThePsk31SeamTests` | 6 of 6, 5 of 5, 7 of 7 |
| `VoiceTests` | 3 of 3 |
| `TheFlatWorldAssetTests`, `TheGreatCirclePathTests`, `ThePsk31ReferenceIsPinnedTests` (engine) | 5 of 5, 7 of 7, 2 of 2 |

**Nothing new is red.** The inherited reds in section 10 were not run and were not chased.

## 2. What the owner should expect

**The build is clean** - zero warnings, zero errors, `TreatWarningsAsErrors` on.

**Open a map and it will be half as close as it was.** Where it used to fill the popup
from a crop 181 px across, it now uses 360, which is about half the map's width. Short
contacts no longer fill the frame at all, and that is correct: two stations a couple of
hundred miles apart really are close together.

**The For You panel scrolls.** Every card is reachable, including the last, and nothing is
cut off sideways. With one conversation on screen you will still see a scroll bar, because
a card with its map on it is taller than the room the panel has.

**What will look wrong and is not.** A card you are reading slides down when a new station
answers, because new ones go on top. The scroll itself stays where you left it; what moves
is the content under it. That is ask 10 and it is yours to rule on.

**Nothing was restyled.** The line, the markers, the map row, the card and the dismiss X
are exactly as they were.

**Pushed to `main`**, five commits, nothing uncommitted:
`1590cc6`, `6558fad`, `03a527c` and this report.

## 3. What you should see

**Two of these I confirmed by looking at real pixels. The rest is computed. Here is
which.**

**The popup's crop - confirmed by looking.** I cropped `assets/world-flat-relief.png` to
the rectangle the new code computes and looked at the result. This is the source bitmap
seen through the computed window, **not the rendered control**, so it confirms what is in
the crop and not how Avalonia paints it.

```
before   x 169 to 350, y 71 to 166      181 x 95 source px at 4x
after    x  79 to 439, y  18 to 218     360 x 200 source px at 2x
```

**What is in the capped crop**: the whole of North America from the Gulf up to Baffin
Island, Greenland entire, the North Atlantic with the path arcing across it, Britain and
Ireland, Europe to about the Baltic, and north-west Africa down to the Sahara. The relief
reads as terrain - mountains and coast - rather than as blocks. The old crop held the same
arc across an ocean with only the edges of two continents in the corners, and visibly
softer.

**The panel with six cards - confirmed by standing the window up headless**, which lays
out and does not paint. At a 1400 by 900 window:

```
the panel        331 x 264
the card region  301 wide, 224 of viewport
the content      301 x 1748          <- was 1748 in a 264 panel with no way down

  VK2ABC   top    0   height 283
  JA1ABC   top  283   height 293
  G0ABC    top  576   height 293
  VE3XN    top  869   height 293
  W1ABC    top 1162   height 293
  K9XP     top 1455   height 293

scrolled to the bottom: offset 1524, showing to 1748 of 1748
```

**So the last card is reachable**, and the content is exactly as wide as the viewport, so
no card is cut off. **What I cannot tell you** is whether the scroll bar is visible, what
it looks like, or whether 2x reads as sharp on your monitor. Those want eyes on a screen.

## 4. What's blocking us

Nothing blocks step 1 of the PSK31 phase. Five items want your ruling.

1. **The cap is the author's number and not yours. Reproduced in full so one word changes
   it:**

   > **Author's proposal, not Tim's ruling.** Tim ruled *"cap zoom"* and did not name a
   > number. Measured against this bitmap, a popup about 722 px wide gives:
   >
   > | cap | crop from the source | how much of the map width |
   > | --- | --- | --- |
   > | 1.5x | 481 x 257 px | 69% |
   > | 2.0x | 361 x 192 px | 52% |
   > | 2.5x | 289 x 154 px | 41% |
   >
   > **2.0x is proposed.** It still halves the world rather than showing all of it, so the
   > popup is meaningfully closer than the card; and two-to-one on a smooth enlargement is
   > about the limit at which a relief bitmap still reads as terrain rather than as blocks.
   > 1.5x is safer and barely zoomed; 2.5x is visibly soft again. Overruling this means
   > changing one number.

   It is `Ft8GlobePlot.ZoomCap`, in one place.

2. **Card ordering under scroll** (inbound ask 10, raised and deliberately not ruled). New
   stations go to the top of the For You panel, so the card you are reading slides down
   when one answers. The scroll no longer jumps, but the content moves under it.
   **Holding a card still needs the panel to stop rebuilding every card every slot**,
   which is a change to how cards are made rather than to how they are shown.

3. **The cards are rebuilt from scratch on every slot, and one thing already depends on
   them not being.** `DigitalCards.Clear()` then new `Ft8ContactCard` objects means every
   piece of per-card state is lost each rebuild - including `MapIsOpen`, unit 310's open
   popup. **Not measured on a running app and not fixed here**; named because it is the
   same root as item 2.

4. **A larger source bitmap would let the popup do what it was asked to do** (inbound ask
   14, yours to take or leave). 698 by 381 is 1.86 pixels per degree. If a larger version
   of the same artwork has an identical crop, the four projection constants multiply by
   the width ratio with no refit. The cap is what you ruled and the cap is what is built;
   this is the option that would make the cap unnecessary.

5. **One conversation card is taller than the panel.** 293 px of card in 220 px of room at
   a 1400 by 900 window, so a scroll bar shows with a single contact. Nothing is broken by
   it. If you would rather the card were shorter, the map row is the part that could give.

### Asks still outstanding

Carried per HM-DEC-139, verbatim where unresolved.

1. **Does the transmission record ask the radio whether it keyed?** Unit 303's proposal,
   still Tim's: `Played` stays a statement about what the audio path did; a second,
   separate fact says what the radio did, read from `1C 00` and `15 11`, which Hamlet
   already polls four times a second; **unknown** where the radio does not answer. Touches
   what the display asserts, so Tim's without exception (§12.1).
2. **Nothing in this repository can look at a picture.** Eleven units have reported every
   appearance claim as computed rather than seen. **Both faults in this unit are
   appearance faults that no test in the tree could have caught** - a container that clips
   instead of scrolling, and a bitmap magnified until it is mush. Real pixels want
   `Avalonia.Headless.Skia`, and **a package is Tim's, not a session's** (§0.4).
3. **Three inherited reds, never chased.** Two in `TheAchievementsScreenTests` -
   `WsprIsNotAFirstAnybodyCanEarnAndTheCardSaysSo` and `TheWindowDrawsEverySixRows` - and
   one in `TheFitGuardAsksAboutTheGridTheSendIsOnTests`, **in the engine test project**.
4. **Where the explanatory hover wording lives, if anywhere.** `Ft8ContactCard.Closing`
   is uncalled and left standing.
5. **`Ft8GlobePlot`'s unused framing constants** - `MapWidth`, `MapHeight`, `Margin`,
   `SmallestFrame`. Report; leave standing.
6. **The licence of `assets/world-flat-relief.png` is unknown.** In the tree by Tim's
   ruling, no watermark, no recorded origin, and Hamlet is GPL-3.0. See
   `assets/PROVENANCE.md`. **Raise; do not resolve; write no licence claim anywhere.**
7. **`PHASE_OUTCOME.md` has a hole** - no `UNIT 306`, no `UNIT 307`, and possibly no 309
   or 310. **Append this unit; report the hole; do not back-fill.** Appended. The hole is
   confirmed: this file is the PSK31 phase's and starts at unit 312, and the FT4 phase's
   record, which held 309 and 310 and was missing 306 and 307, is in git history.
8. **The door sentence is a placeholder.** `new area · would open something you have not
   seen yet`. Wording is the product (§3.5) and Tim has given no ruling. Carry it.
9. **Acknowledgement indicators.** Tim named these as one of four things wrong with the
   screen and the author has not established which of two things he meant. **Not in this
   unit. Build nothing for it and do not guess at it.** Carried; nothing was built.
10. **Card ordering under scroll.** Tim's when he wants it: with conversation cards
    unlimited and a vertical scroll, a card he is mid-exchange with can scroll out of
    view, and arriving cards can move what is under his pointer. **Raised as item 2 above,
    now with a measurement behind it.**
11. **The outline width on the neighbourhood map is 2 px, a number unit 312 chose.** Tim
    is the only one who can see whether it reads. Carry.
12. **Version numbering for the phase.** Unit 312 landed on 1.13.0; a strict reading of
    HM-DEC-150 would have made it 1.13.1. **This unit takes a patch bump from whatever the
    tree says** and carries the question. Taken: 1.13.0 → 1.13.1.
13. **`PHASE_PLAN.md` §R6 says 14.0700-14.0725; the cited band row says 14.070-14.074. The
    row wins.** The author concedes it; §R6 is prose to correct when the plan is next
    touched. Carry.
14. **The source bitmap is small for anything larger than the card.** 698 x 381 for the
    whole world is **1.86 pixels per degree**. Tim has ruled the zoom capped rather than
    the image replaced, which settles this unit - but **a larger version of the same
    artwork would let the popup do what it was asked to do**, and if the crop is identical
    the four projection constants simply multiply by the width ratio with no refit.
    **Raise it as Tim's to take or leave.** Raised as item 4 above.

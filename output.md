# Work instruction 306 - the map renders, both stations are placed on it, and the `i` becomes bullets

**READ IN THIS ORDER.**

A. **The phase goal - FT4 does everything FT8 does.** This unit advanced no step of
   it. Steps 5 and 6 are you at your own radio, which no session can meet.

B. **Step 4 and its exit criteria** - pressing FT4 tunes and decodes; the panel, the
   conversation, the ring, the filters, the tooltips, the ledger and the right-click
   menu working unchanged; one click, one transmission; a whole exchange from one
   right click at the bench. **None was measured tonight.** Step 4 stays `partial`,
   though the card's own hover and its map both changed, so criterion 2 has two new
   things to look at.

C. **The report last, and section 4 raises 4 items** on top of a carried queue of six.

```
UNIT:       306 - complete at task 5 of 5, none dropped - 2026-09-10 19:01
PHASE GOAL: FT4 does everything FT8 does. Steps 0 and 3 are done; 1, 2 and 4 are
            partial; 5 and 6 are you at your own radio and no session can meet them.
UNIT GOAL:  Put the two stations on a real map, and make the `i` readable at a
            glance instead of readable at all.
ADVANCED:   no - this is the shared FT8/FT4 surface again and no step of the phase
            moves; steps 5 and 6 cannot be met by a unit at all.
NUMBER:     AzimuthalMap callers in the app 0 -> 1; stations placed 0 -> 2;
            hover one line -> 5 rows
DRIFT:      12 consecutive units without advance  (was 11, carried in the
            instruction)
```

## 1. What Claude did

**Complete. Five tasks of five, none dropped, including the named drop candidate.**
Development machine, prompt claimed `PROJECT: Hamlet`, and the tree confirmed it:
`SHACK_FACTS.md` and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` present,
`CoreHMI.sln` and `MURC.sln` absent. Branch **`main`**, six commits, **all pushed,
none refused**. Root version 1.12.270 to **1.12.271**. Nothing under `src/Ft8Sharp/`
was touched, nothing that keys the transmitter was touched, and no package was added.

**Nothing was written to `DECISIONS.md`.**

### Task 1 - the trace

**`PHASE_OUTCOME.md` carries no `UNIT 306`.** The fold is clear.

**The asset is in the tree and is the file the instruction describes.** 899 by 602,
RGBA, and its SHA-256 is `1e85c81b…272838` to the character.

**What drew the globe.** `Ft8GlobeControl` filled a sea-coloured rectangle, drew
`assets/world-coastline.svg` through a translate-and-scale taken from
`Ft8GlobePlot.Frame`, then a dashed line and two dots at screen scale - a ring for the
operator, filled for the station. `Ft8GlobePlot` carried **its own flat projection**,
`X = (longitude + 180) * 2` and `Y = (90 - latitude) * 2`, a 720 by 360 map, a zoom
fitted to both points with a 40 px margin and a 120 px floor, and a `Caveat` string
about straight lines on a flat map. The card builds it at `Ft8ContactCard.cs:326`.

**`AzimuthalMap.Settings` was exactly the three fields the instruction names**, at
`:52`. `Place` at `:98` returns **offsets from the centre, not absolute pixels**, with
y already flipped for the screen; `FromCentre` at `:144`. Its only caller in the tree
was `AzimuthalMapTests`.

**The operator's grid** reaches the card as `_operatorGrid` (`Ft8ContactCard.cs:81`),
handed in from `_settings.Operator.GridSquare` when the cards are rebuilt.
`OperatorLocation.FromGrid` returns null when it does not resolve, and the plot already
coped.

**`Detail` after unit 305** was one joined string, twelve facts on middle dots, bound
straight into a `HintMarkControl.Text` at `MainWindow.axaml:4482`.

**The finding that made this unit smaller.** The tree's existing `Place`, centred on
the pole, **reproduces the delivered image's documented formula exactly** once turned
by 289.109° and moved to the centre pixel. Measured against the fourteen city dots:
**identical to the direct formula to twelve decimal places, worst error 0.902 px, mean
0.43.** So there is no second projection anywhere, and a test asserts the two agree.

### Mismatches against this instruction

Reported, not repaired.

1. **The instruction's orientation figures are right and my own first check was
   wrong.** FN00DJ lands at **696.3, 325.1** against the stated 696, 325 - my first
   latitude and longitude for that grid were wrong, not the instruction. Dublin,
   Reykjavik and Anchorage all land where it says, and Sydney and Cape Town are off the
   bitmap as stated.
2. **The three-number record could not be extended without rewriting a proved test.**
   `AzimuthalMapTests` drives `Settings` as it stands and is green by inheritance, so
   the five numbers plus the hash went into a **new record**, `AzimuthalImage`, that
   parameterises the same `Place` rather than replacing it. The instruction says *the
   record needs* those fields; it does not say which record, and this way nothing proved
   gets rewritten.
3. **`GridPath.DescribeBearing` returns degrees, not one of sixteen points.** The
   instruction cites "`SpotDistance` and HM-DEC-038's sixteen compass points"; the
   sixteen points are in `OperatorLocation.DescribeCompass`. The hover was calling the
   degrees one, which is why task 4 had a number to remove.
4. **Task 2 could not be delivered without task 3's placement.** Rendering the new
   picture while the dots were still placed by the flat projection would have put every
   station in the wrong place on a real map, which is the exact §0.0 fault this unit is
   about. The projection moved in task 2's commit; the two-marker behaviour and its
   refusals in task 3's.
5. **`Unit299GlobeTests` had to be rewritten, and it is a third type.** The instruction
   authorises rewriting `TheReadinessHoverTests` and `Unit299HoverTests` for task 4;
   this one reads the coastline projection that task 2 removes, so it could not compile.
   What it guards is unchanged - one projection, belonging to the map being drawn, both
   dots placed by it, a station with no grid getting no dot. The coastline `desc` oracle
   and the four zoom tests are gone with the things they described.
6. **The flat-map caveat became a false sentence and was replaced.** On an azimuthal
   equidistant picture a line through the centre **is** the great-circle path. Carrying
   the old wording would have been telling you something untrue about a picture that is
   now right.

### The fault the tests caught

**The caption said Hamlet did not know where a station was, about a station who had
told it.** It keyed off whether a marker could be drawn, so `VK2ABC` in `QF56` - a grid
on the air, 9,600 miles measured - was described as one with no grid at all.
`AStationSouthOfTheEquatorIsNotPlacedAndNothingIsClaimed` failed on it. The caption now
separates *he sent no grid* from *this picture has nowhere to draw him*, and the
distance stays, because it is a fact about two grids rather than about the image.

### Tests

**No suite was run.** Every run was filtered by exact name, foregrounded, with a
480-second timeout, and `PROJECT_STATUS.md` was written after each.

- `TheAzimuthalAssetTests` - **17 tests**, in `tests/Hamlet.RadioEngine.Tests/Explore/`.
  Watched failing at compile, then green: hash, fourteen cities, refusal on a tampered
  byte, and the agreement with the generic projection.
- `TheGlobePlacesStationsTests` - 5 tests. Watched failing; one failed for a real
  reason, above.
- `TheGlobeLineTests` - 3 tests. Watched failing at compile.
- `TheReadinessHoverTests` - 4 tests, rewritten for rows. Watched failing at compile.
- `Unit299HoverTests`, `Unit299GlobeTests` - rewritten, per mismatches 3 and 5.
- `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint` - passes.

**22 green in the app project and 17 in the engine project, on this unit's names.**

**Every appearance claim here is computed, not seen.** Nothing in this repository can
look at a picture. What is asserted is the arithmetic that decides where a dot goes,
and the fourteen dots it is checked against were found in the image by measurement
rather than by this code.

## 2. What the owner should expect

**The globe is a photograph now.** Open the `i` on a card and the map under the dots is
the north-polar picture, not a drawn coastline, and it is the same picture whoever is at
the radio.

**Where you are on it comes from your own grid in Settings.** Change it and your marker
moves; the station you are working does not.

**The `i` is five bulleted rows** instead of one line of middle dots.

### The coverage limit, in plain words

**This picture runs from the North Pole to the equator, and that is the whole of it.**
Measured by area:

- **40.4% of the globe is on the picture.**
- **80.8% of the northern hemisphere is.**
- **19.2% of the northern hemisphere is inside the projection and cropped off the
  file**, because the disc is cut top and bottom by the frame.

So of the places you might work: **Dublin, Tokyo, Madrid, Singapore, Anchorage and
Reykjavik are on it. Nairobi, Buenos Aires, Sydney, Cape Town, São Paulo, Lima and
Auckland have no place on it at all - and neither does Honolulu**, which is northern
and falls in the cropped strip.

**A station Hamlet cannot place gets no marker and no line, and the card says so in
words** rather than putting him near the rim. The distance is still measured and still
shown. **This is the asset's coverage, not a bug**, and swapping in a full-disc image
later changes the numbers in one record and no code.

**What I could not check.** The instruction asks for the count of *your recent decodes*
falling outside. **This machine has no ledger from your station** (HM-DEC-093), so the
figures above are the geometry rather than your evening; the answer for your actual log
is one telemetry file away.

**Other things that will look wrong and are not.** `assets/world-coastline.svg` is
still in the tree and nothing draws it - it is not deleted, and `CoastlineUri` still
names it. The zoom is gone: the map is always the whole picture.

## 3. What you should see

**1. The globe shows the real map.** The north-polar photograph with its own degree
ring, coastlines and city dots, drawn whole, with the two markers on top of it. The ring
of degrees round the edge is part of the picture; Hamlet adds no tick, label or bearing
readout of its own.

**2. Where you are.** A ring marker at your own grid. `FN00DJ` lands at 696, 325 on the
picture - eastern North America, where it should be. Move your grid to `JO65` and the
marker moves 309 px across the map; the station stays put.

**3. Where the station you are working is.** A filled marker, and a dashed line between
the two **only when both can be drawn**. `EI4GNB` in `IO63` places at 531, 123, and the
words on his marker read:

```
EI4GNB, in grid IO63. 3,400 miles northeast of you.
```

**4. What the `i` now reads as, row by row:**

```
•  He hears you -9 dB · You hear him -12 dB · Decoder floor -21 dB
•  Grid EN52 · 540 miles west-northwest
•  1240 Hz in the passband · Dial 14.074000 MHz · 0.2 s into the slot
•  02:11:00 to 02:12:00 UTC · 1 slot ago
•  He last sent RR73
```

**Five rows, none longer than 66 characters, the last thing he sent on its own at the
bottom.** The degrees are gone and the compass word is there instead.

## 4. What's blocking us

**1. The bearing amendment, reproduced in full and marked as the author's.**

> **Author's proposal, marked as such under §4.4 and not Tim's ruling.** He said that
> about an image centred on his own station, where he sits at the middle and the line
> out of the middle *is* the direction. This asset is centred on the North Pole and he
> is off to one side of it, so a line no longer reads as a bearing from him. **The
> proposal: the degrees come off the hover, and the sixteen-point compass word stays.**
> HM-DEC-038 is otherwise untouched - it also feeds happening-now cards and map-dot
> tooltips, which have no line to read, and there the compass word is the only answer
> there is. Reproduce this block in section 4 for him to overrule.

Built that way. Overruling it means putting `288 degrees` back on one row.

**2. Where the marker hovers should surface.** The two sets of words are built and
asserted - *You, in grid FN00DJ* and the station's distance and compass word - and
**they are not attached to the dots**, because the globe already lives inside the `i`
tooltip and a tooltip inside a tooltip is not a thing. Three ways out: put the globe
somewhere it can own its own hit targets, put the station's words in the caption under
the map, or leave them unshown until the globe moves. **I did not choose one**, because
each changes where a picture lives on your screen.

**3. The full-disc image, now with numbers under it.** 40.4% of the globe, 80.8% of the
northern hemisphere, and Honolulu off the edge. If a full-disc equidistant image centred
on the pole is available, it changes `AzimuthalMap.NorthPolar`'s numbers and its hash
and nothing else - the code that reads them is done.

**4. `Ft8GlobePlot`'s framing constants are now unused.** `MapWidth`, `MapHeight`,
`Margin` and `SmallestFrame` describe the zoom that went. Nothing in `src/` reads them.
They are left standing rather than removed on my own judgment, in the same spirit as
`Ft8ContactCard.Closing`.

### Asks still outstanding

Carried inbound per HM-DEC-139, verbatim where unresolved, plus what this unit adds.

1. **Does the transmission record ask the radio whether it keyed?** Unit 303's proposal,
   still yours: `Played` stays a statement about what the audio path did; a second,
   separate fact says what the radio did, read from `1C 00` and `15 11`, which Hamlet
   already polls four times a second; **unknown** where the radio does not answer.
   Touches what the display asserts, so yours without exception (§12.1).
2. **Nothing in this repository can look at a picture.** Eight units have reported every
   appearance claim as computed rather than seen. Real pixels want
   `Avalonia.Headless.Skia`, and **a package is yours, not a session's** (§0.4). **This
   unit makes it sharper**: it renders a photograph and puts two dots on it, and no test
   in this tree can see either.
3. **Three inherited reds**, proved red before the units that found them, never chased:
   two in `TheAchievementsScreenTests`, one in
   `TheFitGuardAsksAboutTheGridTheSendIsOnTests`.
4. **The two-answer rule from unit 305.** Built as the author's proposal and still yours
   to overrule: the first answer adopts the CQ card; a second station answering the same
   CQ opens its own. Not rebuilt and not changed here.
5. **Where the explanatory hover wording lives, if anywhere.** Unit 305 took it off the
   hover and deleted nothing; `Ft8ContactCard.Closing` is uncalled and left standing.
   This unit did not delete it either.
6. **Unit numbering has collided.** Unit 305's report found three different orders
   sharing the number 305, all folded under `UNIT 305 - STEP 4`. **This one is 306 and
   `PHASE_OUTCOME.md` carried no `UNIT 306` when it started**, so the fold is clean.
7. **New: where the marker hovers surface**, item 2 above.
8. **New: the unused framing constants**, item 4 above.

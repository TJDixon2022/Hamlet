# Work instruction 313 - the popup stops blurring, and the panel scrolls

**This is unit 311 re-issued.** Unit 311 was delivered on 2026-09-11 and never ran; unit
312 confirmed that. Nothing in its scope has changed. What has changed is the tree around
it: the PSK31 phase's files are now at the root and the version is 1.13.x. This unit
advances no step of the PSK31 phase and says so.

---

## 0. The project gate

**This instruction is for Hamlet and nothing else.** Confirm the tree:

| check | expected |
| --- | --- |
| `SHACK_FACTS.md` | exists at the root |
| `src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` | exists |
| `CoreHMI.sln` | **must not exist** |
| `MURC.sln` | **must not exist** |
| root | `C:\Source\HamLet` |

The extraction gate beside the zip already ran these four. **If any is wrong now, stop
and say so in `output.md` section 4. Write nothing else.**

---

## 1. The two rules that killed sessions

Both HM-DEC-155, Tim, 2026-09-05.

**1. A unit runs no test suite.** Not `dotnet test`, not a whole project, not a whole
type unless the unit wrote the whole type. **Only this unit's own test names, filtered by
exact name, foregrounded, with a stated timeout:**

```
timeout 480 dotnet test <project> --filter "FullyQualifiedName~TypeName.MethodName"
```

Known reds you did not cause are in section 10. **An unfiltered run finds them, spends
twenty minutes, and tells you nothing about your work.**

**2. Never background a command and poll it.** No `&`, no `start`, no `nohup`, no loop
that sleeps and checks. **The watchdog fires at twelve minutes with no status write.** If
something genuinely needs longer than 480 seconds, that is a finding for section 4.

---

## 2. The tool fact

**The shell here breaks on an apostrophe inside a quoted heredoc, and it collapses a
doubled backslash.** Write *do not* rather than `don't` inside a quoted heredoc. Write
single backslashes in paths, or forward slashes. **Check what landed on disk rather than
what you typed.**

---

## 3. The asks queue

Carried per HM-DEC-139. **All of these come back in `output.md` section 4, verbatim where
unresolved.** Do not answer them yourself; do not delete one because it looks stale.

1. **Does the transmission record ask the radio whether it keyed?** Unit 303's proposal,
   still Tim's: `Played` stays a statement about what the audio path did; a second,
   separate fact says what the radio did, read from `1C 00` and `15 11`, which Hamlet
   already polls four times a second; **unknown** where the radio does not answer.
   Touches what the display asserts, so Tim's without exception (§12.1).
2. **Nothing in this repository can look at a picture.** Eleven units have reported every
   appearance claim as computed rather than seen. **Both faults in this unit are
   appearance faults that no test in the tree could have caught** - a container that
   clips instead of scrolling, and a bitmap magnified until it is mush. Real pixels want
   `Avalonia.Headless.Skia`, and **a package is Tim's, not a session's** (§0.4).
3. **Three inherited reds, never chased.** Two in `TheAchievementsScreenTests` -
   `WsprIsNotAFirstAnybodyCanEarnAndTheCardSaysSo` and `TheWindowDrawsEverySixRows` -
   and one in `TheFitGuardAsksAboutTheGridTheSendIsOnTests`, **in the engine test
   project**.
4. **Where the explanatory hover wording lives, if anywhere.** `Ft8ContactCard.Closing`
   is uncalled and left standing.
5. **`Ft8GlobePlot`'s unused framing constants** - `MapWidth`, `MapHeight`, `Margin`,
   `SmallestFrame`. Report; leave standing.
6. **The licence of `assets/world-flat-relief.png` is unknown.** In the tree by Tim's
   ruling, no watermark, no recorded origin, and Hamlet is GPL-3.0. See
   `assets/PROVENANCE.md`. **Raise; do not resolve; write no licence claim anywhere.**
7. **`PHASE_OUTCOME.md` has a hole** - no `UNIT 306`, no `UNIT 307`, and possibly no 309
   or 310. **Append this unit; report the hole; do not back-fill.**
8. **The door sentence is a placeholder.** `new area · would open something you have not
   seen yet`. Wording is the product (§3.5) and Tim has given no ruling. Carry it.
9. **Acknowledgement indicators.** Tim named these as one of four things wrong with the
   screen and the author has not established which of two things he meant. **Not in this
   unit. Build nothing for it and do not guess at it.** Carry the item.
10. **Card ordering under scroll.** Tim's when he wants it: with conversation cards
    unlimited and a vertical scroll, a card he is mid-exchange with can scroll out of
    view, and arriving cards can move what is under his pointer. **Task 3 gives the panel
    a scroll and must not invent an ordering rule. Raise it.**
11. **The outline width on the neighbourhood map is 2 px, a number unit 312 chose.** Tim
    is the only one who can see whether it reads. Carry.
12. **Version numbering for the phase.** Unit 312 landed on 1.13.0; a strict reading of
    HM-DEC-150 would have made it 1.13.1. **This unit takes a patch bump from whatever
    the tree says** and carries the question.
13. **`PHASE_PLAN.md` §R6 says 14.0700-14.0725; the cited band row says 14.070-14.074.
    The row wins.** The author concedes it; §R6 is prose to correct when the plan is next
    touched. Carry.
14. **The source bitmap is small for anything larger than the card.** 698 x 381 for the
    whole world is **1.86 pixels per degree**. Tim has ruled the zoom capped rather than
    the image replaced, which settles this unit - but **a larger version of the same
    artwork would let the popup do what it was asked to do**, and if the crop is
    identical the four projection constants simply multiply by the width ratio with no
    refit. **Raise it as Tim's to take or leave.**

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet works PSK31 the way it works FT8.
UNIT GOAL:  Two things on the shared digital screen are the wrong size - a panel
            that clips what it should scroll, and a picture enlarged past what
            it contains. Both are FT8/FT4 faults Tim reported before the phase
            turned, and both surfaces are the ones PSK31 will inherit.
ADVANCES:   No step of the PSK31 phase. This is carried repair work and the
            phase's drift count rises by one, honestly.
DRIFT:      1 expected. Read PHASE_OUTCOME.md in task 1 and report the number.
```

`assets/screenshot-popup-fuzzy-and-no-scroll.png` is Tim's screen.

**What is working on it, and should not be disturbed.** The map row has no grey filler
beside it. The popup opens with a dismiss X and a caption. The path arcs north across the
Atlantic to France with a ring at `FN00DJ` and a filled marker at `JN36`. Both markers
and the line read clearly. **Do not restyle any of that.**

### Fault 1 - the popup is fuzzy, and it is not a coding fault

Tim: *"this is fuzzy."*

**Measured.** `F4DIA` is in `JN36`, which places at **339.7, 118.6**; `FN00DJ` places at
**178.3, 133.1**. The sampled path between them occupies a box **161.4 px across and
29.4 px tall** on the bitmap. Framed to the popup's shape with a margin, that is a crop
of roughly **185 x 99 source pixels enlarged to about 722 x 385** - a **3.9x
magnification**. Every source pixel becomes a four-pixel block.

**There is no more detail in the file.** The zoom floor unit 310 asked for was the wrong
instrument: a floor tight enough to stay sharp is a floor of 1x, which is no zoom at all.
Tim was offered a larger source image or a cap, and **ruled the cap** - section 6, R9.

### Fault 2 - the For You panel clips instead of scrolling

Tim: *"For you is not scrolling."*

Unit 310's R6 ruled conversation cards unlimited with a vertical scroll. **What is there
is a fixed container that cuts off.** On the screenshot the panel is truncated at the
right edge as well as the bottom, so **check whether that is the same missing container
rather than a second fault** - it probably is, and reporting two fixes for one cause
would be wrong.

**This is the fault that matters more of the two.** A card scrolled out of reach is a
contact he cannot log.

---

## 5. Verify this instruction against the tree

**Units 309, 310 and 312 have run; 311 did not.** Unit 310's report confirmed the popup
with its quarter-of-file floor and the map row fitted; unit 312 confirmed 311 never ran
and left the root with the PSK31 phase's three files and version 1.13.0. **Do not fold
unit 312's work or anything of the PSK31 run into this unit.**

Named so the checking is concrete. **Report every mismatch; do not repair this
instruction; do not stop over a mismatch** unless a task is impossible, in which case say
which and why.

- Whatever unit 310 built for the popup and its frame - the zoom floor, the bounding box
  over the sampled path, the date-line fallback. `TheMapOpensTests`.
- Whatever unit 310 built for the panel - `ThePanelHoldsThemAllTests`, `TheCqReceiptTests`,
  `TheMapRowFitsTests`.
- `Ft8GlobeControl`, `Ft8GlobeControl.WordsAt`, `Ft8GlobePlot`, `FlatWorldImage`,
  `FlatWorldMap.Relief`, `GreatCirclePath`.
- The For You panel itself, in `MainWindow.axaml`, and what sets its height and width.
- **The green circled glyphs on some decoded rows** in
  `assets/screenshot-popup-fuzzy-and-no-scroll.png` - on `LZ1KU KN32` and `J38DX FK92`,
  both of which are unworked countries. **Establish what they are.** If they are the
  achievement quills from unit 309 then that unit ran and the marks work, which is worth
  knowing plainly. Report it either way; **change nothing about them.**
- Tests: `ThePressingOfCqTests`, `TheGlobeOnTheCardFaceTests`, `TheGlobeLineTests`,
  `TheCqListNudgeTests`, `TheNudgeHoverTests`, `TheConnectionLineReadsTests`,
  `TheFlatWorldAssetTests`, `TheGreatCirclePathTests`,
  `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint`.

---

## 6. Rulings in force

**R9 - Tim, 2026-09-11. The popup zoom is capped.** Offered a larger source image or a
cap on magnification, he ruled: *"cap zoom"*. **The image is not being replaced in this
unit.** Inbound ask 11 carries the larger-image option for him to take later.

**R10 - Tim, 2026-09-11.** *"fix for you scrolling"*.

**Unit 310's rulings, unchanged and not to be disturbed.** R1 two card types - a CQ
receipt and a conversation card. R2 the receipt is short: what was sent, when, Log,
dismiss, and **no station facts at all**. R3 one receipt, refreshed on repeat press, never
a second. R4 **the last call only and no count** - *"just the last I don't want reminders
of failures"*. R5 the receipt retires and is never adopted; two answers make two
conversation cards and no receipt. **R6 conversation cards unlimited with a vertical
scroll - task 3 is the half of this that was not delivered.** R7 the map row shrinks to
the map, nothing cropped, no stretch. R8 the map opens in a popup with a dismiss X, zoomed
to the path.

**Tim, 2026-09-10** - the map image, *"use this one, make it work."* **Tim, 2026-09-11** -
*"We need a better line… I want to clearly see the connection line."*

**Tim, 2026-09-10 - four rulings on the CQ-list nudge**, carried, **not built in this
unit**: the mark is a lift plus the quill vane in decode green `#3B6D11`; sticky per
station with a cap of two counted over stations; **the achievement set is the source and
no rarity ordering is to be invented**; a door is marked and never named.

**`ACHIEVEMENTS_PHILOSOPHY.md`, repository root, 2026-09-09.** §2 a wall of empty cards
reads as failure; §3.1 absent, not dimmed; §3.5 the teaching is the product; §4 counts say
**worked**, never **confirmed**; **§0.0 / HM-DEC-092 never present a guess as a decode,
and a picture binds as hard as a sentence**; §0.6 colour is never the only carrier; §0.5 /
HM-DEC-012 family colour is text colour only; §0.5.1 / HM-DEC-087 grey is reserved for a
control that cannot be used; **§0.2 one click, one transmission**; §0.1 the engine is
never told tabs exist; §2.1 nothing personal in telemetry.

**FACT-004** - a dev-machine result is an indication, never a finding. **FACT-006** - this
machine has no radio and has never logged a contact. **Every number in section 4 was
measured by the author on a machine with no radio.**

**Tim, 2026-09-06 - the dummy load is withdrawn in full**, superseding HM-DEC-008 and
HM-DEC-098. **No compensating control in its place.**

**HM-DEC-139** - the asks queue is carried inbound and outbound. **HM-DEC-155** - section 1.

---

## 7. Status cadence

`PROJECT_STATUS.md` per `CLAUDE.md` §13: **after every task, and at least every ten
minutes.** The watchdog fires at twelve minutes of silence. Write the status **before**
starting a long test run, saying what you are about to run.

---

## 8. The tasks

Four. Each names the test to watch failing first and one drop candidate. **Drop from the
back.**

### Task 1 - trace. No production file changes.

**This task writes nothing into `src/`.**

**1a.** Append this unit to `PHASE_OUTCOME.md` - **which is now the PSK31 phase's** - as
carried repair advancing no step. Report the version before and after; it is 1.13.x now
and takes a patch bump.

**1b. Stand the app up.** Get a conversation card, click the map, open the popup. Then get
enough cards on the For You panel to overflow it. **Do not touch anything that keys the
transmitter.**

**1c. The popup frame.** Answer with file and line:

- What chooses the crop today, and **what is the zoom floor unit 310 was asked to set** -
  the number it chose and where it lives?
- **What magnification does the `F4DIA` case actually get?** The author measures 3.9x from
  a 185 x 99 crop; report what the code does.
- What resampling does the enlarged bitmap use? **A nearest-neighbour enlargement at 2x
  looks worse than a smooth one and the difference is free**, so say which it is.

**1d. The For You panel.** Answer:

- What sets its height, and is there a scroll container anywhere in it?
- **Is the horizontal truncation the same cause or a different one?** State which.
- With six conversation cards, **what happens to the sixth** - is it drawn and clipped, or
  not drawn at all?

**1e. The green circled glyphs** on the decoded rows. What are they? **If they are unit
309's achievement quills, say so plainly** - Tim last saw that list with nothing on it and
told the author it was a total failure.

**1f.** Run, filtered and foregrounded, before changing anything: `TheMapOpensTests`,
`ThePanelHoldsThemAllTests`, `TheCqReceiptTests`, `TheMapRowFitsTests`, and
`BindingHealthTests.TheMainWindowBindsWithoutOneComplaint`, skipping any that do not
exist. Report each green or red. **A red here is inherited, not yours.** Update
`docs\carry-forward-tests.txt`, **with known reds never on it.**

**Test watched failing first:** none.
**Drop candidate:** none. **Not droppable.**

---

### Task 2 - the zoom is capped

**R9.** The frame still centres on the path. It is no longer allowed to enlarge past the
cap.

**The rule.** Compute the frame as unit 310 does - the box holding every sampled
great-circle point and both markers, plus margin, corrected to the popup's aspect. **Then,
if that frame would magnify the bitmap past the cap, enlarge the frame until the
magnification equals the cap**, keeping it centred on the same point.

**The cap. The author proposes 2.0x and this is marked as the author's choice, not Tim's
ruling. Reproduce this block in section 4 for him to overrule.**

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

**Clamping, and this one is not optional.** Once the frame is enlarged to the cap it can
run off the edge of the bitmap - the map only covers **x 0 to 697 and y 0 to 380**, and a
path near an edge will push the frame out. **Slide the frame back inside rather than
letting it hang off**, and if the frame is larger than the bitmap in a dimension, show the
whole of that dimension. **The path must stay inside the frame after the slide.**

**Worked example to test against.** The `F4DIA` path box centres on **259.0, 118.3**. At a
2.0x cap the crop is **x 78.5 to 439.5, y 22.1 to 214.6** - North America to eastern
Europe, wholly inside the bitmap, with the path in it.

**The two cases unit 310 already ruled stay as they are.** A date-line path shows the
whole world. A station with no place on the file has no map row, so no click and no popup.

**Use a smooth enlargement, not nearest-neighbour.** At 2x the difference is visible and
costs nothing.

**Test watched failing first:** `ThePopupZoomIsCappedTests`, app project. Watch it fail,
then green:

1. the `F4DIA` case is framed at the cap, not at 3.9x, and the crop matches the worked
   example within a tolerance you state
2. **the magnification never exceeds the cap** for any of: São Paulo, Cape Town, Tokyo,
   Auckland, a same-state contact a hundred miles away
3. **every sampled path point is still inside the frame** after capping and clamping
4. a frame that would run off an edge is **slid inside**, and the path is still in it
5. a date-line path still shows the whole world
6. the cap lives in **one named constant**, not in two places

**Drop candidate:** the same-state case in assertion 2. Keep the four long paths.

---

### Task 3 - the For You panel scrolls

**R10, and the unbuilt half of R6.** **This is the more important of the two faults: a
card that cannot be reached is a contact that cannot be logged.**

The panel holds N conversation cards plus at most one receipt. It needs a vertical scroll
around them. **Fix whatever task 1d found, and if the horizontal truncation has the same
cause, fix it in the same change and say so** rather than reporting two fixes for one
cause.

**Do not invent an ordering rule.** Keep whatever order the panel uses today and **raise
inbound ask 10 in section 4**: with eight cards and a scroll, what stays put when new
cards arrive is a decision Tim has not made, and a card moving under his pointer while he
reaches for Log is the thing that will bite.

**One thing to get right while in there.** A scroll that jumps to the top, or to the
bottom, every time a card is added or a slot lands would be worse than clipping. **Whatever
the scroll position is when a card arrives, it should still be pointing at the same card
afterwards.** If that is not achievable without an ordering rule, say so and leave the
position alone.

**Test watched failing first:** `ThePanelScrollsTests`, app project. Watch it fail, then
green:

1. with more cards than fit, **every card is reachable** - the last one included
2. **no card is clipped horizontally**
3. a card arriving **does not move the scroll position off the card it was on**
4. with one card, or none, the panel does not show a scroll it does not need
5. the receipt and the conversation cards are in the same scrolling region, so dismissing
   the receipt does not leave a gap at the top

Re-run `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint` and
`ThePanelHoldsThemAllTests` filtered. **Wrapping a panel in a new container in
`MainWindow.axaml` is exactly what the binding test exists to catch.**

**Drop candidate:** assertion 4, the no-scroll-when-not-needed case.

---

### Task 4 - say what the two fixes actually look like

**Drop candidate: this whole task.** It writes no production code.

Tim has twice been shown a report claiming something was built and then found it was not
on his screen. **Nothing in this repository can look at a picture**, and both faults in
this unit are of exactly that kind.

So: **stand the app up again after tasks 2 and 3, and describe what is there.** The
popup's crop in source pixels and what is visible in it. The panel with six cards, and
what happens when you scroll to the last one. **Say which of those you confirmed by
running the app and which only by arithmetic**, and put the distinction in section 3
rather than burying it.

**Test watched failing first:** none; this task asserts nothing.
**Drop candidate:** the whole task.

---

## 9. Parked

Not in this unit. Do not start any of it.

- **Replacing the map bitmap with a larger one** (inbound ask 11). Tim ruled the cap.
  **Raise the option; do not act on it.**
- **Acknowledgement indicators** (inbound ask 9). **Not defined. Build nothing.**
- **Card ordering under scroll** (inbound ask 10). Raise it; do not rule it.
- **The CQ-list achievement marks and the CQ starter card.** Units 309's work. Report what
  task 1 finds; **build nothing.**
- **The door sentence.** Still Tim's.
- **Pan, or zoom the operator can drive.** The popup frames the path and that is all.
- **The polar map** - `AzimuthalMap.NorthPolar`, `AzimuthalImage`,
  `TheAzimuthalAssetTests`. Green and untouched.
- **Deleting `assets\world-coastline.svg`, `CoastlineUri`, `Ft8GlobePlot`'s unused
  framing constants, or `Ft8ContactCard.Closing`.** Report; leave standing.
- **The achievements screen itself.**
- **Restyling the line, the markers, the map row or the card.** They are right. Leave them.
- **Back-filling `PHASE_OUTCOME.md`.**
- **`Avalonia.Headless.Skia` or any other package** (§0.4).

---

## 10. What not to do

- **No unfiltered `dotnet test`.** No whole project, no whole solution.
- **Never background a command and poll it.**
- **Do not touch anything that keys the transmitter.** The proved chain
  `cq_pressed → … → ft8_transmission Played` stays as it is - clock `measured`, offset
  0.033 s from `2026-09-10.jsonl`.
- **The dummy load is withdrawn in full** (Tim, 2026-09-06, superseding HM-DEC-008 and
  HM-DEC-098). **No compensating control in its place.**
- **Do not replace or re-scale the map bitmap.**
- **Do not stretch it to fill anything.** The projection depends on its aspect.
- **Do not let the frame leave the bitmap.** x 0 to 697, y 0 to 380.
- **Do not hard-code the popup's size.** Read it and derive the crop.
- **Do not invent an ordering rule for the card panel.**
- **Do not let `CQ` be looked up as a callsign anywhere.**
- **Do not draw a map**, and **do not read, align to or label the pale curves on the
  ocean** - they are artwork, not a graticule.
- **Do not add or vendor a package.**
- **Do not claim an appearance from computation and call it seen.** Say *computed*.
- **Do not invent a ruling id.** R1 to R10 carry no `HM-DEC-` number; cite them by date
  and quote.
- **Do not chase these known reds:** `CwAdjudicationTests.ASpeedChangeInRealisticAudio`;
  the 51 CW cases in `docs\unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests`
  whole-type-list tripwire; `HM-OPEN-088`'s ten; the two in
  `TheAchievementsScreenTests`; the one in
  `TheFitGuardAsksAboutTheGridTheSendIsOnTests` in the engine project.
- **Do not repair this instruction.** Report mismatches; keep working.
- **Do not write to `DECISIONS.md`.** §12.1.

---

## 11. Reporting

`output.md` at the repository root. **The canonical headings, which
`tools\arbiter\validate-output.bat` requires: `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.**

Open with the A/B/C ordering block, then the header:

```
A. The phase goal - ...
B. The PSK31 phase's step 1 is next and this unit does not touch it - say so.
C. The report last, and section 4 raises N items on top of a carried queue of eleven, three of them from unit 312.
```

```
UNIT:       313 - <complete|stopped> at task N of 4, <which dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no, and which step>
NUMBER:     <what moved, before -> after>
DRIFT:      <n> consecutive units without advance  (carried from PHASE_OUTCOME.md)
```

**Section 1 must answer three things in one line each, near the top:** what the popup's
magnification was before the cap; whether the horizontal
truncation was the same cause as the vertical; and **what the green circled glyphs on the
decoded rows are.**

**Section 4 must reproduce the task 2 author's-proposal block in full** so Tim can change
the cap with one word.

**Every appearance claim in this report is computed, not seen. Say so once, plainly - and
say which of this unit's two fixes you confirmed by standing the app up.**

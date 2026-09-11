# Work instruction 309 - the line he cannot see, and the marks that never appeared

**READ IN THIS ORDER.**

A. **The phase goal - FT4 does everything FT8 does.** This unit repaired the shared
   FT8/FT4 surface, so everything below lands on both modes at once.

B. **Step 4 and its exit criteria** - pressing FT4 tunes and decodes; the panel, the
   conversation, the ring, the filters, the tooltips, the ledger and the right-click
   menu working unchanged; one click, one transmission; a whole exchange from one
   right click at the bench. **Criterion 2 is the one this unit is about** - two things
   on that panel were asserted working and were not on your screen - but **none of the
   criteria was measured at a radio**, and steps 5 and 6 are you at yours. Step 4 stays
   `partial`.

C. **The report last, and section 4 raises 5 items** on top of a carried queue of twelve.

```
UNIT:       309 - complete at task 5 of 5, none dropped - 2026-09-10 21:20
PHASE GOAL: FT4 does everything FT8 does. Steps 0 and 3 are done; 1, 2 and 4 are
            partial; 5 and 6 are you at your own radio.
UNIT GOAL:  Find out why two things Hamlet asserts are on the screen are not on the
            screen, make them visible at the size they are actually drawn, and give
            the CQ list its marks.
ADVANCED:   no - the panel is repaired rather than advanced, and no exit criterion of
            step 4 can be met without you at your radio.
NUMBER:     CQ rows highlighted 0 -> 13 of 14 on an empty log, 2 of 14 with North
            America worked; strokes on the map 1 -> 2, cased; cards after a CQ press
            0 -> 1
DRIFT:      14 consecutive units without advance  (was 13, carried in the instruction)
```

## 1. What Claude did

Five tasks of five, none dropped. Development machine, prompt claimed `PROJECT: Hamlet`,
tree confirmed it. Branch **`main`**, six commits, **all pushed, none refused**. Root
version 1.12.272 to **1.12.273**. Nothing under `src/Ft8Sharp/` touched, **nothing that
keys the transmitter touched**, no package added, nothing ranked.

**Every appearance claim in this report is computed, not seen**, and this unit is what
that habit costs: two things unit 308 asserted green were not on your screen. **So say
plainly what that means now.** I have measured what the drawing code is handed and what
it decides. I have not seen a pixel. **Fault 2 I am confident about** - the marks are
data, and the data now says the right thing on the list the CQ filter shows. **Fault 1 I
am less confident about**: I know the line and both markers are drawn, I know they are
now drawn twice with a pale casing under the ink, and **whether that is legible on your
screen is a question only you can answer.**

### Task 1 - the two mechanisms, measured

**1a.** This unit is appended to `PHASE_OUTCOME.md` as `UNIT 309 - STEP 4`. **It still
carries no `UNIT 306` and no `UNIT 307`**, and neither was back-filled.

**1c. The line and the operator marker: drawn and invisible, not never drawn.**
Measured on the exact card from your screenshot - RD6OB in KN98 from FN00DJ:

```
HasOperator     : True
HasStation      : True
HasPath         : True
operator at     : 178.3, 133.1
station at      : 399.2, 113.9
path runs       : 1
```

True for a six-character grid, a four-character grid, a lower-case one and one with
spaces round it. **So the fault was never that something was not computed.**

**What was actually wrong is contrast against a photograph.** The operator marker is
`#C8842A` - amber - drawn as a **ring**, and eastern North America on this picture is
bright orange. The path is `#5F5C53`, a muted grey-brown, 1.4 px, dashed, over dark blue
ocean. **The station marker survived because it is a filled green dot**, which is the
one of the three that had a colour unlike its ground.

**1d. The marks: the derivation was never the reason.** Run against your own fourteen
rows, with North America worked:

```
TI2AIM     Costa Rica                Visible Costa Rica
J38DX      Grenada                   Visible Grenada
RD6OB      (the table declines)      None
VE3XN      Canada                    None
KJ3LLY     United States of America  None
... seven more United States rows    None
```

**That is exactly what the ruling asks for.** What stood between it and your screen was
a **cap of two**, which task 3 removed.

**1e. The CQ press: the five green tests never press the button.**
`ThePressingOfCqTests` drives `RecordSentForTests` - a seam that books straight into the
ledger - then calls `RebuildCardsForTests()` by hand. So it proves *a card is built from
a booked CQ* and says nothing about *pressing CQ books a CQ*. **And in the application it
did not**: the one production call to `RecordSent` is in the slot-boundary path, after a
transmission has gone out.

**1f.** `docs/carry-forward-tests.txt` created, with known reds deliberately off it.
**All 27 green before anything changed.**

### Mismatches against this instruction

Reported, not repaired.

1. **The 0.344 mechanism is not the mechanism.** The instruction says anything sized in
   bitmap coordinates is multiplied by 0.344 and disappears. **The control pushes no
   transform**: `At()` scales positions only, so the pen width and marker radius already
   reached the screen at their stated sizes. **The sizing rule is still right and is
   implemented** - constants in control units - but it was not the cause.
2. **`RD6OB` is not European Russia as far as this tree is concerned.**
   `DxccPrefixes.EntityOf("RD6OB")` returns nothing; the cited table declines the `RD6`
   prefix. Unit 252's rule is that Hamlet is silent where the table declines, so **that
   station can never be highlighted** by any version of this feature. That is a finding
   about the data, not the mark, and adding a prefix to a cited table is not a session's
   to do.
3. **The small green circled glyph** on the `J38DX` row could not be identified from the
   markup with confidence. It is **not** the quill: the quill is `AchievementMarkControl`
   bound to `IsNudged`, which was false for every row. Reported as unresolved rather than
   guessed.
4. **A third cause of no starter card, found on the way.** With no measured clock offset,
   `CardsNow` is null and `RebuildCards` builds nothing, so **no card of any kind can
   appear before the clock is measured.** Your own file reads `measured`, so it was not
   your cause - but it is a real one and it is named here.

### The defect this unit's own test found

**The path was built whenever both grids *resolved*, not when both were *placeable*.** A
station in Antarctica resolves perfectly well and has nowhere on this picture, so a line
ran from you toward the bottom edge and stopped where the samples fell off the file -
pointing at a marker deliberately not drawn. **A line to a place the picture says it
cannot show is the same claim as a dot there** (§0.0). Fixed, and the fix exposed an
ordering bug in the same constructor that the same test caught.

### What came out in task 3

The withdrawn rulings, and the four tests that asserted them, are gone:

- `TheCapHoldsAtTwoWhenSixCallersAreCandidates` - **the cap**
- `ADoorDisplacesTheWeakestVisibleMarkAndTheCountStaysTwo` - **the ordering**
- `ArrivalOrderBreaksTheTieBetweenTwoOfAKind` - **the tie-break**
- `AMarkedStationStaysMarkedAcrossThreeRebuilds` - **the stickiness mechanism**, which
  existed only to stop the ordering churning; with nothing ranked nothing churns, so the
  behaviour it asserted is now free rather than built

`MostMarkedStations`, the sticky dictionary and the displacement search came out with
them. **A structural test now reads the marking method and fails on `OrderBy`, `Max`,
`CompareTo`, `weakest`, `strongest` or a cap.**

### Tests

**No suite was run.** Every run filtered by exact name, foregrounded, 480-second timeout,
status written after each.

| type | project | count | watched failing |
| --- | --- | --- | --- |
| `Unit309TraceTests` | app | 7 | no - it measures |
| `TheConnectionLineReadsTests` | app | 5 | yes, at compile |
| `TheCqListNudgeTests` | app | 9 | 4 new, yes; 4 old removed |
| `TheCqPressMakesACardTests` | app | 4 | **yes, all four red against the tree** |
| `TheNudgeHoverTests` | app | 6 | yes |
| `ThePressingOfCqTests`, `TheGlobeLineTests`, `TheGlobeOnTheCardFaceTests`, `BindingHealthTests` | app | 14 | carried forward, green throughout |

**39 green across everything this unit touched.**

## 2. What the owner should expect

**The line should be visible now, and I cannot promise it.** Every stroke on the map is
drawn twice: a pale wider stroke underneath and the ink over it. That is what lets one
line read over dark blue ocean *and* bright orange land, where a single colour reads over
one or the other. Both markers are cased the same way. **If it still does not read, the
next thing to try is colour rather than weight**, and that is a decision I would want your
eye on rather than another computed guess.

**Your own marker should appear.** It was always being drawn - an amber ring on orange
land. It is now a ring with a pale ring outside it.

**The CQ list will light up, and on your log it may light up a lot.** With an empty log,
**13 of your 14 rows** are highlighted, because everything genuinely is new. With North
America worked, **2 of 14**. There is no cap and nothing is ranked.

**Pressing CQ gives you a card again**, at the press rather than after a transmission.

**What will look wrong and is not.**

- **Eight of your fourteen rows have no mark.** Canada and the United States are worked.
  **That is the restraint and it is the only one** - an unmarked row is not a lesser row
  and nothing about it is dimmed or annotated.
- **`RD6OB` will never be marked.** The cited prefix table does not know where he is, and
  Hamlet is silent where the table declines. Not a bug in the mark.
- **On an empty log every mark is a *door*** - you have opened no continent, so every
  caller opens one, and none of them names an area.
- **The starter card claims nothing about the air.** It says what was composed and when.

## 3. What you should see

**The line, at the size the card draws it.** The map renders about 240 x 131 from a 698 x
381 bitmap. **Measured, at that size and at double it:**

```
at 240 wide : stroke 2, casing 4, marker 4.5, operator at 61.3, 45.8
at 480 wide : stroke 2, casing 4, marker 4.5, operator at 122.6, 91.5
```

**The ink does not change size and the positions do**, which is the rule the instruction
asked for, with no 240 and no 0.344 written anywhere. The path from FN00DJ to KN98 draws
as **one run of 180 segments, 360 strokes** counting the casing. Your marker is a cased
ring; the station's is a cased filled dot; **shape carries the difference, not hue.**

**Your fourteen rows, answered row by row.** With North America worked, which is your log:

| row | entity | marked? |
| --- | --- | --- |
| `TI2AIM` | Costa Rica | **yes** - `Costa Rica · new country` |
| `J38DX` | Grenada | **yes** - `Grenada · new country` |
| `RD6OB` | the table declines | no - Hamlet does not know where he is |
| `VE3XN` | Canada | no - worked |
| `KJ3LLY` | United States | no - worked |
| `W4JNC` | United States | no - worked |
| `KD5USA` | United States | no - worked |
| `KF9UG` | United States | no - worked |
| `KS1WK` | United States | no - worked |
| `N4ZEK` | United States | no - worked |
| `KD8WYT` | United States | no - worked |
| the remaining three | United States and Canada | no - worked |

**With an empty log, 13 of the 14 are marked** and every one of them is a door.

**The starter card.** Press CQ and a card headed `CQ` appears, reading `Sent CQ KC3QIS
FN00. First at 02:11:00 UTC. Sent once.` Press twice more and the same card reads `Sent 3
times`.

## 4. What's blocking us

**1. Whether the line is legible.** I cased it and I cannot see it. **This is the one
thing in the unit I would most like you to look at**, and if it still does not read, say
so and the next lever is colour, not weight.

**2. The door sentence.** Three candidates, all checked by a test against §4 - worked
never confirmed, no area named, nothing that shames or promises:

| candidate | length | cost |
| --- | --- | --- |
| `new area · would open something you have not seen yet` | 53 | the placeholder. *Have not seen* is slightly odd - he has seen the world, he has not worked it |
| `new area · somewhere you have not worked yet` | 44 | the plainest, and *worked* is the word §4 asks for. Says less about what opens |
| `new area · this one opens a part of the map` | 43 | ties it to the map he is now looking at. Slightly more of a promise |

**I did not choose one.** Wording is the product (§3.5) and no ruling has been given.

**3. `RD6OB` and the prefix table.** One of the three stations you named cannot be
highlighted because the cited DXCC prefix table declines `RD6`. Adding a prefix to cited
data is a data change and yours, not a session's.

**4. A starter card needs a measured clock.** With no offset, no card of any kind can be
built. Your machine had one, so this was not your fault - but it means a first run on a
machine that cannot reach a time server shows nothing.

**5. The green circled glyph** on the `J38DX` row is unidentified. It is not the quill.

### Asks still outstanding

Carried per HM-DEC-139.

1. **Does the transmission record ask the radio whether it keyed?** Unit 303's proposal,
   still yours: `Played` stays a statement about what the audio path did; a second,
   separate fact says what the radio did, read from `1C 00` and `15 11`; **unknown**
   where the radio does not answer. Touches what the display asserts, so yours without
   exception (§12.1).
2. **Nothing in this repository can look at a picture.** Eleven units now. **This unit is
   the bill**: two things asserted green were not on your screen, and section 1 says
   plainly which of this unit's two fixes I am and am not confident about. Real pixels
   want `Avalonia.Headless.Skia`, and **a package is yours** (§0.4).
3. **Three inherited reds, never chased.** Two in `TheAchievementsScreenTests` -
   `WsprIsNotAFirstAnybodyCanEarnAndTheCardSaysSo` and `TheWindowDrawsEverySixRows` - and
   one in `TheFitGuardAsksAboutTheGridTheSendIsOnTests`, in the engine test project.
4. **The two-answer CQ rule.** The author's proposal from unit 305, not yet your ruling:
   the first answering station adopts the CQ card; a second gets a card of its own.
5. **Where the explanatory hover wording lives, if anywhere.** `Ft8ContactCard.Closing`
   is uncalled and left standing.
6. **Where the globe lives. Closed** - you said *"I like the map, and how it is placed in
   the card."* The author's proposal is accepted and the item is dropped from here on.
7. **The full-disc polar image. Superseded** by your ruling of 2026-09-10.
8. **`Ft8GlobePlot`'s unused framing constants** - `MapWidth`, `MapHeight`, `Margin`,
   `SmallestFrame`. Still unread by anything in `src/`. Left standing.
9. **The licence of `assets/world-flat-relief.png` is unknown.** In the tree by your
   ruling, no watermark, no recorded origin, and Hamlet is GPL-3.0. See
   `assets/PROVENANCE.md`. Raised, not resolved, and no licence claim written anywhere.
10. **`PHASE_OUTCOME.md` has a two-unit hole** - no `UNIT 306`, no `UNIT 307`. This unit
    appended itself and back-filled neither.
11. **The door sentence is a placeholder** - item 2 above, now with three candidates.
12. **Acknowledgement indicators.** You named these as one of four things wrong with the
    screen and which of two things you meant has not been established. **Nothing was built
    and nothing was guessed at.**

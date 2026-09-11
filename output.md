# Work instruction 310 - two kinds of card, a panel that holds all of them, and a map you can open

**READ IN THIS ORDER.**

A. **The phase goal - FT4 does everything FT8 does.** This unit worked the shared
   FT8/FT4 card panel and the map on a card face, so everything below lands on both
   modes at once.

B. **Step 4 and its exit criteria** - pressing FT4 tunes and decodes; the panel, the
   conversation, the ring, the filters, the tooltips, the ledger and the right-click
   menu working unchanged; one click, one transmission; a whole exchange from one right
   click at the bench. **Criterion 2 is the one this unit moved**: what the panel holds
   and what a card says both changed, deliberately. **No criterion was measured at a
   radio**, and steps 5 and 6 are you at yours. Step 4 stays `partial`.

C. **The report last, and section 4 raises 4 items** on top of a carried queue of ten.

```
UNIT:       310 - complete at task 5 of 5, none dropped - 2026-09-10 22:05
PHASE GOAL: FT4 does everything FT8 does. Steps 0 and 3 are done; 1, 2 and 4 are
            partial; 5 and 6 are you at your own radio.
UNIT GOAL:  Stop drawing a call to nobody as a conversation with somebody in
            Portugal, let the panel hold every conversation at once, fit the map
            row to the map, and let the map open.
ADVANCED:   no - the panel is repaired rather than advanced, and no exit criterion
            of step 4 can be met without you at your radio.
NUMBER:     card types 1 -> 2; grey beside the map at a 480 px card 260 px -> 0.16;
            map row 480 x 120 -> 220 x 120; the two stations 154 px apart on the
            card map -> 614 px in the popup, at the same size
DRIFT:      15 consecutive units without advance  (was 14, carried from unit 309)
```

**Every appearance claim in this report is computed, not seen.** Nothing in this
repository can look at a picture. What is asserted is the size a control asks for, the
window of the file it frames, the geometry a render would decide and the words a card
holds - never that any of it looked right on your screen.

## 1. What Claude did

Development computer, no radio attached; **nothing here is evidence about the radio.**
The prompt said `PROJECT: Hamlet` and the tree agrees: `Hamlet.sln` at the root,
`Hamlet.*` namespaces, `PROJECT_CARD.md` naming Hamlet. Branch **`main`**, six commits,
each pushed before the next task started. Version **1.12.273 -> 1.12.274**, bumped at
task 1.

**The three questions, one line each.**

- **Did unit 309 run? Yes.** `PHASE_OUTCOME.md` carries a `UNIT 309 - STEP 4` entry, its
  five commits are in `main` (`5c562ae`, `80142ab`, `b6be927`, `a36c1a1`, `ee9c97f`), and
  its code is in the tree. **The instruction's doubt was wrong**, and the two-unit hole in
  the phase record is 306 and 307, not 309.
- **Was a replaced conversation card destroyed or hidden? Neither, in every path I could
  drive.** Two stations answering one CQ produced two cards with the ledger holding both,
  and a second conversation arriving left the first card and its whole ledger history
  standing. **The fault you saw did not reproduce**, so what this unit shipped is the
  invariant asserted six ways so it cannot regress - not a fix for a loss I found.
- **Where `CQ` was being treated as a station: seven sites, all through one method.**
  `DxccPrefixes.EntityOf` answers **Portugal** for the literal string `CQ`, because CQ is
  a real Portuguese prefix alongside CT and CR. Its callers are `DigitalDecodeRow`,
  `Ft8ContactCard` (twice), `Ft8Vocabulary`, `AchievementLog`, `NudgeSet` and
  `DxccContinents` - so the card said Portugal, and **the CQ-list nudge marked your own
  general call as a door: a continent to go and chase.** The guard went into `EntityOf`
  itself, where the question is asked, rather than into the header where you saw it.

**Task 1 - the measurements, before a word was changed.** The carry-forward list ran 18
of 18 green first. The trace recorded the before-state in comments and now asserts the
after-state.

**Task 2 - the receipt stops being a conversation with blanks.** `Place`, `StateWord`,
`Sentence` and `ShowsGlobe` all answer for a receipt before they answer for a station, so
a call to anybody carries no callsign, no country, no grid, no distance, no map and no
sentence about somebody who does not exist. The message rows dropped the tally.

**Task 3 - one receipt, unlimited conversations, and `Adopt` withdrawn.** The ledger's
`Adopt` is replaced by `Retire`: when anybody answers, the receipt goes and **neither
station inherits the call.** This **withdraws the author's own unit 305 proposal**, which
had the first answering station take the CQ card over; R5 rules the opposite, and the
reason is plain once two stations answer, because a call to everybody belongs to nobody.
Two assertions in `ThePressingOfCqTests` asserted the withdrawn behaviour and were
rewritten:
`AnAnswerAdoptsTheCqCardAndTheExchangeContinuesInIt` became
`AnAnswerRetiresTheReceiptAndOpensTheStationOwnCard`, and
`ASecondAnsweringStationGetsItsOwnCard` now asserts that neither station inherits it.

**Task 4 - the map row shrinks to the map.** The row was taking the card's whole width
and fitting the picture inside it by height, so everything to the right was ground: at a
480 px card the image drew 219.8 px and **260 px of the row were grey.** The control now
measures itself, asking for the largest box at the map's own proportions that fits what
it is offered - the same box the render already draws into - and the border round it
shrinks to its child. Measured at offered widths 240, 320, 480 and 640: the row is
**220 x 120 every time**, the map 219.84 x 120 inside it, **aspect 1.8320 exactly**,
0.16 px spare. No number is hard-coded: both figures come from the picture record.

**Task 5 - the map opens.** A popup with a dismiss X, framed on the path. The frame is
the box holding **all 181 sampled great-circle points and both markers**, plus a margin
of 6 per cent of its own longer side, grown about its centre to a floor and clamped
inside the file.

**The zoom floor is a quarter of the file - 174.5 by 95.3 px - and here is the reason.**
The popup is about the width of the file itself, so a frame of a quarter of it is
magnified about four times, and at four times one pixel of the photograph has become a
block four pixels across. That is where a relief map stops carrying coastline and starts
carrying squares. **It is an arithmetic argument and not a seen one.** A 248-mile contact
therefore does not fill the frame, which is correct: two stations that close together
really are close together, and zooming until they looked far apart would assert something
the distance denies.

**Nothing was recorded in `DECISIONS.md`.** The instruction forbids it (§12.1) and no
entry was written.

### Three mismatches with the instruction, reported and not repaired

- **The Tokyo path cannot prove what it is named to prove.** The instruction uses FN00DJ
  to Tokyo as the case showing the frame follows the arc rather than the endpoints.
  Measured: that path **crosses the antimeridian**, so it is two runs against opposite
  edges and R8's own date-line rule takes it - it shows the whole world. The bulge is on
  screen either way, so assertion 3 passes, but by the wrong mechanism. **Moscow proves
  it instead**: FN00DJ to KO85 stays on one side of the date line, its arc peaks at y 72.8
  where 66.8N sits at y 70.5, and the frame starts at y **55.3** where an endpoints-only
  box would start at 97.3. Both cases are tested and both are named in the test's own
  remarks.
- **A station the picture cannot place does have a map row.** The instruction says it has
  none. `HasMap` is an *or*: the operator is placed even when the station is not, so the
  row appears with his own marker on it and the caption says where the picture stops.
  What is gated is **the opening**, which is what R8 is about - no path, nothing to zoom
  to, no button drawn and the command refuses on its own account.
- **And the instruction carries no `Asks still outstanding` queue inbound**, which §9.6
  and HM-DEC-139 require. It names "a carried queue of thirteen"; the queue in unit 309's
  report holds twelve, two of which that report already marked closed. Reconstructed
  below from that report and `OPEN_ISSUES.md`, with the two settled ones dropped.

### Tests, all filtered by exact name, foregrounded, 480 s timeout

**No suite was run. No `dotnet test` on a whole project, and nothing was backgrounded or
polled** (HM-DEC-155).

| Test type | Result |
|---|---|
| `TheMapRowFitsTests` (new, task 4) | **5 of 5**, watched failing at compile first |
| `TheMapOpensTests` (new, task 5) | **9 of 9**, watched failing at compile first |
| `TheCqReceiptTests` (new, task 2) | **6 of 6** - one added after the report quoted the hover |
| `ThePanelHoldsThemAllTests` (new, task 3) | 6 of 6 |
| `Unit310TraceTests` (new, task 1) | 4 of 4 |
| `TheCqPressMakesACardTests` | 4 of 4 |
| `ThePressingOfCqTests` | 5 of 5, two assertions rewritten |
| `TheCqListNudgeTests` | 9 of 9 |
| `TheGlobeOnTheCardFaceTests` | 4 of 4 |
| `TheGlobeLineTests` | 3 of 3 |
| `TheGlobePlacesStationsTests` | 6 of 6 |
| `Unit299GlobeTests` | 5 of 5 |
| `TheNudgeHoverTests` | 6 of 6 |
| `TheReadinessHoverTests` | 4 of 4 |
| `TheSlotClockTests` | 3 of 3 |
| `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint` | 1 of 1, re-run after each markup change |
| `TheFlatWorldAssetTests`, `TheGreatCirclePathTests` (engine) | 5 of 5, 7 of 7 |
| `VoiceTests` | 3 of 3, run because this unit adds copy the operator reads |

**Nothing new is red.** The inherited reds were not run and were not chased.

## 2. What the owner should expect

**The build is clean** - zero warnings, zero errors, `TreatWarningsAsErrors` on.

**Press CQ and you get a receipt, not a conversation.** No callsign, no country, no grid,
no distance, no map, and no sentence about a station. Press it again and the same receipt
refreshes to the newest slot; there is never a second one and nothing counts how many
times you have called.

**When somebody answers, the receipt goes and their own card opens.** Two stations
answering give two cards and neither inherits your call. **This is a change of behaviour
from unit 305** and it is R5, not a bug.

**The map row is now 220 px wide instead of the card's full width.** It sits at the left
of the row. The map itself is exactly the size it was - the grey to the right of it is
what went away, and that is 260 px at a 480 px card.

**Click the map and it opens.** The picture zooms to the path with a margin, there is an
X at the top right, and clicking outside also closes it. The marker hovers work inside
the popup - that was the instruction's drop candidate and it came for free, because the
hover asks the same one place for the frame that the render does.

**What will look wrong and is not.** A short contact does not fill the popup: that is the
zoom floor, and it is the honest picture. A path across the date line shows the whole
world rather than zooming: that is R8's own rule, because the two runs sit against
opposite edges and anything tighter needs a re-centred map this unit did not build. A
station in Antarctica still gets a map row with only your own marker on it, and it does
not open.

**What I cannot promise.** That any of it looks right. Every claim above is computed.

**Pushed to `main`**, six commits, nothing uncommitted:
`1d5f72b`, `936301b`, `9215875`, `22ff972`, `a00e900` and the hover fix of section 3.

## 3. What you should see

**The CQ receipt, word for word.** You have seen the wrong version, so here is the right
one rather than a description of it:

```
CQ
Calling
02:03:45 UTC · just now

Your call went out to anyone listening.

Sent CQ KC3QIS FN00 · At 02:03:45 UTC

[ Log this call ]

hover on that button:
  Opens the log window with your own call already filled in, so you can look at
  it before anything is written down. It transmits nothing.
```

`Calling` is the state word. `Your call went out to anyone listening.` is the whole
sentence. There is no country line, no grid, no distance, no map, no caption and no count
of how many times you have called - and where a conversation card would say *Waiting on
him*, this says nothing about anybody, because there is nobody.

**One more of the same fault turned up while quoting it, and it is fixed.** That hover
read *"Opens the log window with what passed between you and CQ already filled in"* - the
conversation wording with the literal string dropped into it, exactly like the country
line, one hover further down. It is the sixth commit of this unit and it has its own
test. **Quoting the thing word for word is what found it**; describing it would not have.

**The card's map row.** 220 px wide and 120 tall, left-aligned, the picture filling it to
within 0.16 px. Under it the caption you already have.

**The opened map.** Centred over the window, the station's callsign at the top left and
an X at the top right, the picture below it framed on the path with a margin, and the
same caption underneath. At a 720 by 400 ceiling, a contact to England puts the two
markers **614 px apart** where the whole-file map puts them 154 px apart at that same
size.

**One thing to check that I cannot.** Whether the popup lands somewhere sensible on your
screen and whether 720 by 400 is the right size for it. Both are numbers I chose, and
section 4 raises the second.

## 4. What's blocking us

Nothing blocks the next unit. Four items want your ruling.

1. **The popup's size is a number I chose: 720 by 400.** It is a ceiling rather than a
   shape - the control fits the frame inside it - but it is still a figure with no source,
   and the instruction says not to hard-code a width or a height. **The alternative is a
   share of the window**, which needs a converter this app does not have. Ruling wanted:
   the ceiling as it stands, a share of the window, or a size you name.

2. **Card ordering under scroll** (inbound ask 13, raised and deliberately not ruled).
   The panel now holds an unlimited number of conversations with a vertical scroll, and
   nothing says which one is at the top when six are live. **No ordering rule was
   invented**, per the instruction.

3. **The zoom floor is mine to revisit.** A quarter of the file, argued from magnification
   rather than from looking at it. If a contact across two states looks too far out when
   you open it, the number is the thing to change and the reason is written beside it.

4. **The single-card fault is unfound, not fixed.** Two stations answering produced two
   cards in every path I could drive. Whatever you saw came from somewhere this trace does
   not reach. **If it happens again, the useful thing is which two callsigns and roughly
   when** - the ledger and the telemetry will then say whether the card went or the
   booking did.

### Asks still outstanding

Carried per HM-DEC-139. **Two items were marked closed in unit 309's report and are
dropped here** rather than carried a second time - where the globe lives, and the
full-disc polar image.

1. **Does the transmission record ask the radio whether it keyed?** Unit 303's proposal,
   still yours: `Played` stays a statement about what the audio path did; a second,
   separate fact says what the radio did, read from `1C 00` and `15 11`; **unknown** where
   the radio does not answer. Touches what the display asserts, so yours without exception
   (§12.1).
2. **Nothing in this repository can look at a picture.** Twelve units now. This unit is
   another instalment of the bill: the map row and the popup are both asserted from
   computed sizes, and **the one fault of this kind you found yourself - `CQ  Portugal` -
   was invisible to every test in the tree.** Real pixels want `Avalonia.Headless.Skia`,
   and **a package is yours** (§0.4).
3. **Three inherited reds, never chased.** Two in `TheAchievementsScreenTests` -
   `WsprIsNotAFirstAnybodyCanEarnAndTheCardSaysSo` and `TheWindowDrawsEverySixRows` - and
   one in `TheFitGuardAsksAboutTheGridTheSendIsOnTests`, in the engine test project.
4. **The two-answer CQ rule. Ruled and dropped after this report** - R5 of 2026-09-11
   answers it: the receipt retires and each answering station opens its own card. The
   unit 305 proposal it replaces is withdrawn in the code and in its tests.
5. **Where the explanatory hover wording lives, if anything.** `Ft8ContactCard.Closing`
   is uncalled and left standing.
6. **`Ft8GlobePlot`'s unused framing constants** - `MapWidth`, `MapHeight`, `Margin`,
   `SmallestFrame`. Still unread by anything in `src/`, and the new framing does not use
   them either. Left standing.
7. **The licence of `assets/world-flat-relief.png` is unknown.** In the tree by your
   ruling, no watermark, no recorded origin, and Hamlet is GPL-3.0. See
   `assets/PROVENANCE.md`. Raised, not resolved, and no licence claim is written anywhere.
8. **`PHASE_OUTCOME.md` has a two-unit hole** - no `UNIT 306`, no `UNIT 307`. This unit
   appended itself and back-filled neither, and task 1 confirmed **309 is not one of the
   missing**.
9. **The door sentence is a placeholder.** Still yours.
10. **Acknowledgement indicators.** You named these as one of four things wrong with the
    screen and which of two things you meant has not been established. **Nothing was built
    and nothing was guessed at.**

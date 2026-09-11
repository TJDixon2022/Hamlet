# Work instruction 310 - two kinds of card, a panel that holds all of them, and a map you can open

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
2. **Nothing in this repository can look at a picture.** Ten units have reported every
   appearance claim as computed rather than seen, and **three faults in the last two
   units were things asserted green that were not on the screen.** Real pixels want
   `Avalonia.Headless.Skia`, and **a package is Tim's, not a session's** (§0.4).
3. **Three inherited reds, never chased.** Two in `TheAchievementsScreenTests` -
   `WsprIsNotAFirstAnybodyCanEarnAndTheCardSaysSo` and `TheWindowDrawsEverySixRows` -
   and one in `TheFitGuardAsksAboutTheGridTheSendIsOnTests`, **in the engine test
   project**.
4. **The two-answer CQ rule. RULED and superseded** - see section 6. The unit 305
   proposal (first answer adopts the CQ card) is **withdrawn**. Report it closed.
5. **Where the explanatory hover wording lives, if anywhere.** `Ft8ContactCard.Closing`
   is uncalled and left standing.
6. **Where the globe lives. Closed.** The map is a row on the card face and Tim has
   accepted it: *"I like the map, and how it is placed in the card."*
7. **The full-disc polar image. Superseded** by Tim's ruling of 2026-09-10.
8. **`Ft8GlobePlot`'s unused framing constants** - `MapWidth`, `MapHeight`, `Margin`,
   `SmallestFrame`. Report; leave standing.
9. **The licence of `assets/world-flat-relief.png` is unknown.** In the tree by Tim's
   ruling, no watermark, no recorded origin, and Hamlet is GPL-3.0. See
   `assets/PROVENANCE.md`. **Raise; do not resolve; write no licence claim anywhere.**
10. **`PHASE_OUTCOME.md` has a hole** - no `UNIT 306`, no `UNIT 307`, and possibly no
    `UNIT 309`. **Append this unit; report the hole; do not back-fill.**
11. **The door sentence is a placeholder.** `new area · would open something you have not
    seen yet`. Wording is the product (§3.5) and Tim has given no ruling. Carry it.
12. **Acknowledgement indicators.** Tim named these as one of four things wrong with the
    screen and the author has not established which of two things he meant. **Not in this
    unit. Build nothing for it and do not guess at it.** Carry the item.
13. **Card ordering under scroll.** New this unit, and Tim's when he wants it: with
    conversation cards unlimited and a vertical scroll, a card he is mid-exchange with can
    scroll out of view, and arriving cards can move what is under his pointer. **Task 3
    must not invent an ordering rule beyond what section 6 states. Raise it.**

---

## 4. Why this unit exists

```
PHASE GOAL: FT4 does everything FT8 does.
UNIT GOAL:  Make the card panel model what is actually true - a call to nobody is
            not a conversation, and a conversation is not the only one there is.
            Then make the map fit its row and open when clicked.
ADVANCES:   Step 4, criterion 2 - the panel and the conversation working. Shared
            FT8/FT4 surface, so it lands on both modes at once.
DRIFT:      carried. Read PHASE_OUTCOME.md in task 1 and carry the number forward.
```

**One fault underneath most of this.** The panel has a single card type and a single
card slot. Everything below is what that costs.

### Fault 1 - a call to nobody is being drawn as a conversation

`assets/screenshot-cq-card-wrong.png` is Tim's screen after pressing CQ. On it:

- the header reads **`CQ  Portugal`**. **`CQ` is a real Portuguese prefix** - CT, CR and
  CQ all belong to Portugal - so the DXCC lookup ran on the string `CQ` and matched.
  **Hamlet has labelled the operator's own general call as a station in Portugal.**
- the body reads **`You called CQ and he has not answered yet.`** **There is no him.**
  He called everyone. *Has not answered yet* frames ordinary silence twenty-three
  seconds into a slot as a station failing to come back.
- the button reads **`Waiting on him`**. Same fault.
- the caption reads **`Hamlet does not know where CQ is. He has not put a grid square on
  the air, and a callsign only names a country, which is not a point on a map.`** That
  sentence was built by unit 306 for a real station who sent no grid. **Applied to a CQ
  it explains the whereabouts of something that does not exist.**
- there is a **map row with an empty map** on it.

**None of these is a separate bug.** They are all conversation-card behaviour running on a
card that has no conversation, with the other station set to the literal string `CQ`.
Unit 305 already booked CQ under `Ft8ContactLedger.CallToAnyone`, so the **ledger** knows
the difference. **The card does not.**

**And it may be poisoning the nudge.** If the entity for a row or card is read the same
way anywhere else, **the operator's own CQ resolves as Portugal**, which is a worked or
unworked country that he never called.

### Fault 2 - the panel holds one card

Tim: *"seems like I'm limited to one contact card at a time. When I start a 2nd
conversation it replaces any existing card I have."*

**That makes his own ruling impossible.** Two stations answering one CQ must produce two
conversation cards; if the second replaces the first, the second answer wipes a station he
is mid-exchange with off the screen.

**Establish whether the first card is destroyed or merely hidden.** If the ledger still
holds the first exchange and only the panel dropped it, this is a display fault and the
history survives. **If the first conversation is discarded, he can lose a contact he was
part-way through logging**, and that is a different severity behind an identical screen.

### Fault 3 - the map row does not fit the map

`assets/screenshot-conversation-card-grey.png` is Tim's screen, and **the line and both
markers are working on it** - a ring at `FN00DJ`, a filled marker at `KL94`, and the arc
bending north over Europe to Saudi Arabia, which is the right path. Tim: *"BETTER, BUT
WHY THE WASTED SPACE ON THE RIGHT."*

**The bitmap is 698 x 381, an aspect of 1.8320.** The row is being given the card's full
width and the image is fitting to the row's height, so the remainder of the row is empty
grey. **Tim has ruled which way it is fixed** - section 6.

### Not a fault - what Tim wants added

**Click the map and it opens in a popup with a dismiss X, zoomed to the path.** Task 5.

---

## 5. Verify this instruction against the tree

**The author does not know whether unit 309 ran.** Its zip was delivered; no report has
been seen. `assets/screenshot-conversation-card-grey.png` shows the line and both markers
working, which is what unit 309's task 2 was written for, so **something changed** - but
that is inference, not a finding.

**Task 1 establishes it.** If unit 309 did not run:

- **do not fold its tasks into this one.** Report it and **keep this unit's scope
  exactly as written.** Its nudge work and its starter-card work stay its own and will be
  re-issued.
- **say so in one clear line in section 4**, because it changes what the owner does next.

Named so the checking is concrete. **Report every mismatch; do not repair this
instruction; do not stop over a mismatch** unless a task is impossible, in which case say
which and why.

- `Ft8ContactCard`, and what builds it - unit 306 reported `Ft8ContactCard.cs:326` for
  the plot and `_operatorGrid` at `:81`. `Ft8ContactCard.Closing`, uncalled.
- `Ft8ContactLedger.RecordSent`, `Ft8ContactLedger.CallToAnyone`, `IsCallToAnyone`, and
  whatever unit 305 built for `Adopt`.
- `Ft8GlobeControl`, `Ft8GlobeControl.WordsAt`, `Ft8GlobePlot`, `Ft8GlobePlot.OffTheMap`.
- `FlatWorldImage`, `FlatWorldMap.Relief`, `GreatCirclePath`.
- `DxccPrefixes.EntityOf` - **and whether anything guards it against the string `CQ`**.
- Whatever unit 309 added, if it ran: `TheConnectionLineReadsTests`, the control-space
  stroke and marker sizing, `docs\carry-forward-tests.txt`.
- Tests: `ThePressingOfCqTests` (5, app), `TheGlobeOnTheCardFaceTests` (4, app),
  `TheGlobeLineTests`, `TheGlobePlacesStationsTests`, `Unit299GlobeTests` (14 between
  them, app), `TheCqListNudgeTests` (9, app), `TheNudgeHoverTests` (5, app),
  `TheFlatWorldAssetTests` (5, engine), `TheGreatCirclePathTests` (7, engine),
  `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint`.

---

## 6. Rulings in force

**Tim, 2026-09-11. These are the rulings this unit carries out. Transcribed. No
`HM-DEC-` id has been assigned to any of them; do not invent one.**

**R1 - there are two card types.**

> *"There are two card type: my CQ and a conversation"*

A **CQ receipt** has no other station. A **conversation card** has one. They are not one
type with a station field that sometimes holds the string `CQ`.

**R2 - the CQ receipt is short.**

> *"the CQ Card is short - just a note that I transmitted it. Placeholder until it is
> answered"*

So the receipt carries: **what was sent, when it was sent, a Log option, and a dismiss
X.** It carries **no callsign, no entity or country, no grid, no distance, no bearing, no
pronoun, no map row, and no caption**, because none of those is true yet. **It is a
placeholder and should read as one** - quieter than a conversation card, not the same
frame with empty slots in it. **Empty slots are what produced Portugal.**

**R3 - one receipt, refreshed, never a second.**

> *"It just sits there until I dismiss it. If I do another CQ it is not a new card, just a
> refresh of the time slot"*

**The panel holds at most one receipt.** Pressing CQ again updates the existing one to the
new slot. **If a second receipt ever appears, something is wrong.**

**R4 - the last call only, and no count.**

> *"just the last I don't want reminders of failures"*

**No tally of how many times he has called.** Unit 305 built a *how many times sent*
count into the CQ card; **it comes off.** The time alone says what is useful - that it is
live and went out seconds ago - without scoring the silence.

**This extends to the wording.** *Has not answered yet* is a failure sentence even with
the pronoun removed. **The receipt says what happened, not what has not happened.**
`ACHIEVEMENTS_PHILOSOPHY.md` §2 is the same instinct applied to a different screen: a wall
of blanks reeks of failure, and so does a rising number on an unanswered call.

**R5 - the receipt retires; it is never adopted.**

> *"No, the placeholder goes away and two conversation card appear."*

Anyone answers, **the receipt goes** and a conversation card appears for that station. **A
second station answering gets a second conversation card.** Neither inherits the receipt.
**The unit 305 adopt proposal - first answer adopts the CQ card - is withdrawn by this
ruling.** The receipt otherwise leaves only when Tim dismisses it.

**R6 - conversation cards are unlimited.**

> *"THE NUMBER OF CONVERSATION IS UNLIMITED WE HAVE VERTICAL SCROLL"*

**Nothing replaces anything.** The panel holds N conversation cards plus at most one
receipt, each card keyed to its station, with a vertical scroll.

**R7 - the map row shrinks to the map.**

> Of three options put to him - fill the row and crop, shrink the row to the map, or
> stretch the map - **Tim selected the second.**

**No grey filler, nothing cropped, the whole world stays whole, and the card gets
shorter.** The map never gets wider than it is now. **Do not stretch the bitmap to fill a
row**: it breaks the projection and every marker lands in the wrong place, which is the
fault the measured constants exist to prevent.

**R8 - the map opens.**

> *"When I click on a map it gets enlarged"* … *"it is a popup with a dismiss X"* …
> *"zoomed into path"*

Task 5.

**Rulings already in force and unchanged.**

**HM-DEC-155 (Tim, 2026-09-05)** - section 1. **Tim, 2026-09-10** - the map image, *"use
this one, make it work."* **Tim, 2026-09-11** - *"We need a better line… I want to clearly
see the connection line."*

**Tim, 2026-09-10 - four rulings on the CQ-list nudge**, carried but **not built in this
unit**: the mark is a lift plus the quill vane in decode green `#3B6D11`; sticky per
station with a cap of two counted over stations; **the achievement set is the source and
no rarity ordering is to be invented**; a door is marked and never named.

**`ACHIEVEMENTS_PHILOSOPHY.md`, repository root, 2026-09-09.** §2 a wall of empty cards
reads as failure - **R4 is this rule on the card panel**; §3.1 absent, not dimmed; §3.5
the teaching is the product; §4 counts say **worked**, never **confirmed**, and nothing
shames; **§0.0 / HM-DEC-092 never present a guess as a decode, and a picture binds as hard
as a sentence - `CQ  Portugal` is exactly this fault**; §0.6 colour is never the only
carrier; §0.5 / HM-DEC-012 family colour is text colour only; §0.5.1 / HM-DEC-087 grey is
reserved for a control that cannot be used; **§0.2 one click, one transmission**; §0.1 the
engine is never told tabs exist; §2.1 nothing personal in telemetry.

**Unit 252** - the DXCC table, from the ARRL list, cited, silent where it declines.

**FACT-004** - a dev-machine result is an indication, never a finding. **FACT-006** - this
machine has no radio and has never logged a contact.

**Tim, 2026-09-06 - the dummy load is withdrawn in full**, superseding HM-DEC-008 and
HM-DEC-098. **No compensating control in its place.**

**HM-DEC-139** - the asks queue is carried inbound and outbound.

---

## 7. Status cadence

`PROJECT_STATUS.md` per `CLAUDE.md` §13: **after every task, and at least every ten
minutes.** The watchdog fires at twelve minutes of silence. Write the status **before**
starting a long test run, saying what you are about to run.

---

## 8. The tasks

Five. Each names the test to watch failing first and one drop candidate. **Drop from the
back.**

### Task 1 - trace. No production file changes.

**This task writes nothing into `src/`.**

**1a.** Append this unit to `PHASE_OUTCOME.md` and report what it carries. **Did unit 309
run?** Check the git log, `docs\carry-forward-tests.txt`, and whether
`TheConnectionLineReadsTests` exists. **Say yes or no in one line.** If no, **do not fold
unit 309's work into this unit.**

**1b. Stand the app up.** Press CQ and get the receipt card. Click a station and get a
conversation card. Then **click a second station.** Do not touch anything that keys the
transmitter.

**1c. The CQ card.** Answer with file and line:

- What builds the card after a CQ press, and **how does it differ from the path that
  builds a conversation card**? If it does not differ, say so plainly - that is the whole
  finding.
- **Where does `CQ` reach `DxccPrefixes.EntityOf`?** Is there any guard at all against
  the string `CQ`, or against a `CallToAnyone` booking being given an entity?
- **Is `CQ` treated as the other station anywhere else** - the nudge, the ledger, the
  worked-before cache, telemetry, the decoded list? **List every place.** This is the
  important half of 1c.

**1d. The single slot.** Click a second station with a first card open. **Is the first
card destroyed or hidden?** Does the ledger still hold the first exchange afterwards?
**Answer both, from a run rather than from reading.**

**1e. The map row.** What sizes the map row, and what sizes the image inside it? Report
the row's measured width and height and the image's, and **the aspect of each.** The
bitmap is 1.8320.

**1f.** Run, filtered and foregrounded, before changing anything: `ThePressingOfCqTests`,
`TheGlobeOnTheCardFaceTests`, `TheGlobeLineTests`,
`BindingHealthTests.TheMainWindowBindsWithoutOneComplaint`, and
`TheConnectionLineReadsTests` if it exists. Report each green or red. **A red here is
inherited, not yours.** If `docs\carry-forward-tests.txt` does not exist, create it from
that list, **with known reds never on it.**

**Test watched failing first:** none.
**Drop candidate:** none. **Not droppable.** Every task below depends on it.

---

### Task 2 - the CQ receipt becomes its own thing

**R1 and R2.** A receipt is not a conversation card with blanks in it.

**What it carries:** what was sent, when it was sent, Log, and a dismiss X. **That is
all.**

**What it must not carry, and each of these is on Tim's screenshot today:** a callsign in
the header, an entity or country, a grid, a distance, a bearing, a pronoun, a map row, a
caption about where anything is.

**The wording, per R4.** No count of calls. No sentence that frames silence as a failure
- *has not answered yet* comes off, and so does *Waiting on him*. **Say what happened:**
he called, and here is when. Seconds-ago is enough to show it is live.

**The guard that makes `CQ  Portugal` impossible rather than merely fixed.** A booking
under `CallToAnyone` **has no other station, and asking it for one is an error rather than
a lookup.** Put the guard where the question is asked, not only where the header is
drawn - task 1c listed every place that asks.

**Test watched failing first:** `TheCqReceiptTests`, app project. Watch it fail, then
green. It asserts, at minimum:

1. a CQ press produces a receipt, **and the receipt exposes no station, no entity, no
   grid, no distance and no map**
2. **`CQ` never reaches the entity lookup**, and a `CallToAnyone` booking asked for an
   entity raises rather than returning Portugal
3. no text on the receipt contains a third-person pronoun or the phrase *not answered*
4. **no count of calls appears anywhere on it**, however many times CQ has been pressed
5. a conversation card, by contrast, still carries all of those station facts unchanged

**Drop candidate:** assertion 3's pronoun sweep. Keep the wording change; drop the
automated sweep for it.

---

### Task 3 - one receipt, and as many conversations as he likes

**R3, R5, R6.**

**One receipt.** Pressing CQ again **refreshes the existing receipt to the new slot** and
does not add a second. **Assert that the panel can never hold two.**

**The receipt retires.** Anyone answers, **the receipt goes** and a conversation card
appears for that station. **Two stations answering produce two conversation cards and no
receipt.** The receipt is never converted, claimed or adopted into a conversation.
**Whatever unit 305 built for `Adopt` on the CQ card is withdrawn by R5** - report what
was there and what became of it.

**The panel holds a set.** N conversation cards plus at most one receipt, **each keyed to
its station**, vertical scroll, **nothing replacing anything**. Fix whatever task 1d
found. **If task 1d found the first conversation was destroyed rather than hidden, say so
in section 2 in plain words** - that is a fact about losing a contact, not a layout note.

**Ordering.** **Do not invent an ordering rule.** Keep whatever order the panel uses
today and **raise the question in section 4** (inbound ask 13): with eight cards and a
scroll, what stays put and what moves matters, and Tim has not ruled it.

**Test watched failing first:** `ThePanelHoldsThemAllTests`, app project. Watch it fail,
then green:

1. **two stations answering one CQ produce two conversation cards and no receipt**
2. a second conversation **does not replace the first**, and the first card and its ledger
   history both survive
3. six conversations produce six cards
4. **pressing CQ five times produces one receipt**, showing the fifth slot
5. dismissing the receipt removes it and touches no conversation card
6. the panel **never holds two receipts**

Re-run `ThePressingOfCqTests` filtered. If R4 or R5 makes one of its five assertions
false, **rewrite that assertion and say in `output.md` which one and why** - a test that
asserts a withdrawn behaviour is worse than no test.

**Drop candidate:** assertion 3, the six-card case. Keep assertion 2.

---

### Task 4 - the map row fits the map

**R7.** The row shrinks to the map. **No grey filler, nothing cropped, no stretch.**

The bitmap is **698 x 381, aspect 1.8320**. The map keeps that aspect exactly. The row
takes the map's height and the map's width; the card gets shorter; **the map never becomes
wider than it is now.**

**Do not hard-code a width, a height or a scale factor.** Unit 309 established that the
card draws this map at about 240 x 131, a scale of 0.344, and that anything sized in
bitmap coordinates disappears. **The same rule holds here:** the stroke widths and marker
radii stay sized in the control's own units, and only placement scales.

**Test watched failing first:** `TheMapRowFitsTests`, app project. Watch it fail, then
green:

1. the rendered map's aspect is **1.8320 within a tolerance you state**
2. **no part of the row is empty** beside the map
3. **no edge of the bitmap is cropped** - the north and south edges of the file are both
   inside the rendered area
4. changing the card's width changes the map's height in proportion, and the aspect holds

Re-run `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint` filtered. Changing a
row's sizing in `MainWindow.axaml` is what it exists to catch.

**Drop candidate:** assertion 4.

---

### Task 5 - click the map and it opens

**R8.** A popup with a dismiss X, zoomed to the path. **Conversation cards only - the
receipt has no map, so it has nothing to click.**

**How the frame is chosen.** Take the **sampled great-circle points the small map already
computes** - `GreatCirclePath`, 181 of them - plus both markers. Find the box that holds
all of them. Add a margin so nothing sits on an edge. Scale to fit the popup.

**The path defines the frame, not the two endpoints.** The arc bulges well north of both
stations: `FN00DJ` to Tokyo peaks at **66.8°N**, and a box drawn from the endpoints alone
crops the top off the thing being looked at.

**Three cases that need a rule, and here are the rules:**

- **A zoom floor.** There is no more detail in a 698 x 381 file than it holds. A short
  contact could otherwise fill the popup from a sixty-pixel crop and become coloured
  mush. **Set a floor, state the number you chose and why**; short contacts then do not
  fill the frame, which is correct.
- **A path that crosses the date line has two runs on opposite edges**, so the box that
  holds both is the whole map. **Those cases show the whole world.** Do not build a
  shifting or re-centred map in this unit.
- **A station with no place on the file has no path to frame.** Antarctica below 63.79°S
  and the strip west of 175.52°W are the whole list. **No map row, so no click, so no
  popup.**

**The marker hovers should work inside the popup.** Unit 306 built them and unit 308
attached them to dots a few pixels wide; the popup is the first place they are comfortable
to use.

**Test watched failing first:** `TheMapOpensTests`, app project. Watch it fail, then
green:

1. clicking the map on a conversation card opens the popup; the dismiss X closes it
2. **the frame contains every sampled path point and both markers**, with margin
3. **a Tokyo path is framed to include 66.8°N** - the endpoints-only box would not
4. a date-line path shows the whole world
5. the zoom floor holds for a short contact
6. a receipt has no map and nothing to click

**Drop candidate:** the marker hovers inside the popup. Ship the popup without them and
say so.

---

## 9. Parked

Not in this unit. Do not start any of it.

- **Acknowledgement indicators** (inbound ask 12). **Not defined. Build nothing.**
- **The CQ-list achievement marks.** Unit 309's task 3. **Not this unit**, even if task 1
  finds unit 309 did not run. Report; do not build.
- **The CQ starter-card regression.** Unit 309's task 4. Same.
- **Card ordering under scroll** (inbound ask 13). Raise it; do not rule it.
- **The door sentence.** Still Tim's.
- **The polar map** - `AzimuthalMap.NorthPolar`, `AzimuthalImage`,
  `TheAzimuthalAssetTests`. Green and untouched.
- **Deleting `assets\world-coastline.svg`, `CoastlineUri`, `Ft8GlobePlot`'s unused
  framing constants, or `Ft8ContactCard.Closing`.** Report; leave standing.
- **The achievements screen itself.**
- **Pan, or zoom the operator can drive.** The popup frames the path and that is all.
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
- **Do not let `CQ` be looked up as a callsign anywhere.**
- **Do not stretch the map bitmap to fill a row.** The projection depends on its aspect.
- **Do not hard-code the card's render size, a map width, or a scale factor.**
- **Do not draw a map.** No coastline is generated, traced or synthesised.
- **Do not read, align to or label the pale curves on the ocean.** They are artwork, not
  a graticule.
- **Do not add or vendor a package.**
- **Do not claim an appearance from computation and call it seen.** Say *computed*.
  **Three faults in the last two units are what that habit costs.**
- **Do not invent a ruling id.** R1 to R8 carry no `HM-DEC-` number; cite them by date and
  quote.
- **Do not invent an ordering rule for the card panel.**
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
B. Step 4 and its exit criteria - ...
C. The report last, and section 4 raises N items on top of a carried queue of thirteen.
```

```
UNIT:       310 - <complete|stopped> at task N of 5, <which dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no, and which step>
NUMBER:     <what moved, before -> after>
DRIFT:      <n> consecutive units without advance  (carried from PHASE_OUTCOME.md)
```

**Section 1 must answer three things in one line each, near the top:** whether unit 309
ran; whether a replaced conversation card was destroyed or hidden; and every place `CQ`
was being treated as a station.

**Section 3 must say what the CQ receipt now reads as, word for word.** Tim has seen the
wrong version; show him the right one rather than describing it.

**Every appearance claim in this report is computed, not seen. Say so once, plainly.**

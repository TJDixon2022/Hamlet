# Work instruction 305 - the CQ makes a card, the slot clock leaves the card panel, and the `i` hover is cut to facts

**READ IN THIS ORDER.**

A. **The phase goal - FT4 does everything FT8 does.** This unit advanced no step of
   it, and steps 5 and 6 are you at your own radio, which no session can meet.

B. **Step 4 and its exit criteria** - pressing FT4 tunes and decodes; the panel, the
   conversation, the ring, the filters, the tooltips, the ledger and the right-click
   menu working unchanged; one click, one transmission; a whole exchange from one
   right click at the bench. **None was measured tonight.** Step 4 stays `partial` -
   though the ring moved, so criterion 2 has something new to look at.

C. **The report last, and section 4 raises 3 items** on top of a carried queue of
   four.

```
UNIT:       305 - complete at task 5 of 5, none dropped - 2026-09-10 17:11
PHASE GOAL: FT4 does everything FT8 does. Steps 0 and 3 are done; 1, 2 and 4 are
            partial; 5 and 6 are you at your own radio and no session can meet them.
UNIT GOAL:  Make the screen answer you during the wait after a CQ - a card for the
            call itself, a countdown that exists when no cards do - and cut the `i`
            hover back to facts.
ADVANCED:   no - all three faults are in the shared FT8/FT4 surface and none is a
            phase step; steps 5 and 6 cannot be met by a unit at all.
NUMBER:     cards after a CQ 0 -> 1; countdown visible 0 -> all of it, with no cards
            and through a collapse; hover 875 -> 1015 measured -> 230
DRIFT:      11 consecutive units without advance  (was 10, read from the output.md
            this file overwrote)
```

## 1. What Claude did

**Complete. Five tasks of five, none dropped, including the named drop candidate.**
Development machine, prompt claimed `PROJECT: Hamlet`, and the tree confirmed it:
`SHACK_FACTS.md` and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` present,
`CoreHMI.sln` and `MURC.sln` absent. Branch **`main`**, five commits, **all pushed,
none refused**. Root version 1.12.269 to **1.12.270**. Nothing under `src/Ft8Sharp/`
was touched, nothing that keys the transmitter was touched, and no package was added.

**Nothing was written to `DECISIONS.md`.**

### Task 1 - the trace, and it found five things

**1. What a CQ press does to the model.** `Ft8ContactLedger.RecordSent`
(`src/Hamlet.RadioEngine/Contacts/Ft8ContactLedger.cs:244`) returned without booking
anything when `Ft8MessageSplit.IsCallToAnyone(fields.To)` was true. The ledger is
keyed by callsign and a CQ is addressed to nobody, so there was no station to key on;
the card list rebuilds from that ledger, so it stayed empty. **Confirmed by
measurement, not assumed: 0 cards.** Its one call site is
`MainWindowViewModel.cs:12133`, on the send path, which was not touched.

**2. Where the countdown ring lived.** `MainWindow.axaml`, inside the `DataTemplate`
of the `ItemsControl` named `DigitalContactCards`. **Worse than the instruction says**:
that control carries `ItemsSource="{Binding DigitalCards}"` *and*
`IsVisible="{Binding HasDigitalCards}"`, so with no cards it could not draw for two
reasons rather than one. **Its driver was already fine** - `RefreshTurn` rides
`_decodeTimer` and reads the clock and the ledger, and depends on no decode having
happened; every property it binds (`TurnRingCount`, `TurnRingSweep`, `TurnRingTip`,
`TurnRingIsUnknown`, `TurnRingIsOnAir`) was already on `MainWindowViewModel`.

**3. Where the `i` hover is composed.** **One place**: `Ft8ContactCard.Detail`
(`src/Hamlet.App/ViewModels/Ft8ContactCard.cs:433`), assembled from four private
composers - `Reports`, `WhereOnEarth`, `WhereOnTheBand`, `Slots` - plus a `Closing`
clause. **Measured, and the instruction's 875 is wrong in both directions: 1015
characters over 13 sentences where a dial is known, and 773 over 10 where it is not.**

**4. Whether the `i` mark draws. It does.** `HintMarkControl.Render`
(`src/Hamlet.App/Controls/HintMarkControl.cs:181`) draws a ring and a glyph, and
line 194 paints a transparent rectangle over the bounds as the hit target.
**Unit 301's hole was repaired.**

**5. What already existed unwired - and this is real tokens back.** The countdown's
whole view-model side was already shell-level, so task 3 was a placement change and
not a rebuild. Nothing existed for the CQ card.

### Mismatches against this instruction

Reported, not repaired.

1. **`output.md` was not unit 304's report.** It was the previous work instruction
   305's, carrying `DRIFT: 10`. I read the figure from there and incremented it, as
   instructed, and the provenance line above says so.
2. **Three different orders now share the number 305**, each with a different title.
   This one, *the offset reaches the snapshot and not the app*, and *the snapshot
   lands after the facts*. `PHASE_OUTCOME.md` folds appends by unit and step, so all
   three landed under `UNIT 305 - STEP 4`.
3. **The hover is 1015 characters, not 875.**
4. **The ring's containment is worse than described**, per task 1 above.
5. **The `i` mark is not a hole**, per task 1 above.
6. **§12.1 is *What a session may record itself*** (`CLAUDE.md:1148`); **§12.5 is the
   fixture rule** (`:1251`). *Nothing interprets a message* is neither of them and is
   not in §12 at all - it is this instruction's own words, and I followed it as one.
7. **This instruction's report template omits the A/B/C ordering block**, which
   `tools/arbiter/validate-output.bat` requires and refuses the file without. It is
   above, in the shape that tool and unit 050's ruling ask for.
8. **`CLAUDE.md` §0.2 does not carry the dummy-load sentences as a requirement.** It
   names the requirement once, to record that it is withdrawn in full. Nothing to fix.
9. **Task 3 undoes a deliberate earlier placement.** The ring was moved into the card
   on purpose, so its seconds would read as *how long to press this* rather than as a
   free-standing clock. Your instruction is explicit that it comes out; it is out, and
   there is exactly one ring, not two.

### What was built

**Task 2.** A call to anybody is booked under its own key,
`Ft8ContactLedger.CallToAnyone`. The card carries what was sent, when it was first
sent and how many times, and nothing else - no elapsed-time claim, no *waiting for a
reply*, no interpretation. It offers **Log** from the moment it appears and never a
send, because there is nobody to send to. When a station answers, `Adopt` moves the
record into theirs, so the CQ is the first thing in the exchange rather than a second
card beside it. **No lookup is ever made against `CQ`** - it is not a callsign.

**Task 3.** The countdown moved out of the card template to `SlotClock`, above both
panels and outside the collapsible, in `DigitalDecodedPanes`. One ring, one timing
source, no label; the number carries the meaning whatever the colour does.

**Task 4.** 1015 characters to **230**, twelve facts, all twelve figures kept.

**Task 5.** Answered as a written specification in section 4. **Not dropped.**

### Tests

**No suite was run.** Every run was filtered by exact name, foregrounded, with a
480-second timeout, and the status file was written after each.

- `ThePressingOfCqTests` - 5 tests, in `tests/Hamlet.App.Tests/ViewModels/`. **Your
  five names, used exactly.** Watched failing at compile - the types they name did
  not exist - then green.
- `TheSlotClockTests` - 3 tests, in `tests/Hamlet.App.Tests/Views/` rather than
  `ViewModels/`, because they stand the real window up. **Watched failing properly**:
  with the old markup stashed, all three red on `Assert.NotNull` for the control.
- `TheReadinessHoverTests` - 2 tests. Watched failing at **1015 against a 300 budget**.
- `Unit305TraceTests` - 2 tests, task 1's measurements.
- `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint` - passes on the XAML added.

**`Unit299HoverTests` was rewritten rather than left to fail.** Task 4 withdraws what
that unit was told to build, so four of its assertions named wording that no longer
exists. The facts it requires are unchanged; only the phrasing it matches moved.

**Every appearance claim here is computed, not seen.** The window is built headless
and the visual tree walked, which says a control exists, is visible and is where it
is, and says nothing about what it looks like.

## 2. What the owner should expect

**Press CQ and a card appears.** It is titled `CQ`, it says what went out, when, and
how many times, and it has a Log option on it from the first second.

**The countdown is on the screen before any of that.** It sits above the two panels
and stays there with no cards, with the panel collapsed, and before anything has ever
decoded.

**The `i` reads as a line of facts** separated by middle dots, about a fifth of its
old length.

**What will look wrong and is not.**

- **The ring is no longer beside the send button on a card.** There is one, above the
  panels. That is the move, and mismatch 8 above says what it costs.
- **The `i` no longer explains anything.** No "which is a roger and a goodbye in one",
  no "a four-character grid is a box about seventy miles across". The figures are all
  still there. The wording is in git history and was not deleted, moved or commented
  out.
- **A card headed `CQ` is not a station.** Nothing looks it up and nothing claims to
  know where it is.

**What I could not check.** `Ft8ContactCard.Closing` is now uncalled - it explained a
closing payload, which is a lesson rather than a fact. **It is left standing on
purpose**, because your instruction says that text is not to be deleted while its fate
is your question. And other tests read `Detail` and the card list; I rewrote the ones
about this hover and **the rule forbidding suites means I cannot tell you whether some
other test reads a phrase I changed.**

## 3. What you should see

**1. A CQ press now puts a card on the screen.** Measured, quoted from the test:

```
CARD  CQ
  face   : 02:11:00 UTC · 15 seconds ago
  action : Log this call
  detail : Sent CQ KC3QIS FN00. First at 02:11:00 UTC. Sent once.
```

Press it twice more and the same card reads `Sent 3 times`. **One card, never three.**

**2. When somebody answers, it becomes their card.** Same run, after `K9XP` comes
back:

```
CARD  K9XP
  face   : 02:11:15 UTC · 15 seconds ago
  action : Tell him how he is coming through
```

Your CQ is inside his record as the first thing you sent. The `CQ` card is gone,
because it became this one.

**3. The countdown is above the panels and no longer inside a card.** With zero cards
the control is present and visible, it is not a descendant of `DigitalContactCards`,
and it survives the panel being collapsed. It reads the same `Ft8Turn` values the
armed send is measured against - asserted against an independently computed
`Ft8Turn.Read`, agreeing to within the second the clock turned over.

**4. The `i` hover, whole, as it now reads:**

```
He hears you -9 dB · You hear him -12 dB · Decoder floor -21 dB · Grid EN52 ·
540 miles · 288 degrees · 1240 Hz in the passband · Dial 14.074000 MHz ·
0.2 s into the slot · 02:11:00 to 02:12:00 UTC · 1 slot ago · He last sent RR73
```

**230 characters. Twelve facts. 1015 before.**

**5. The globe still shows drawn blobs, and it is blocked, not broken.** Nothing was
drawn, substituted or attempted. Section 4 carries the specification.

## 4. What's blocking us

**1. The two-answer rule is the author's proposal, not your ruling.** Reproduced in
full, as instructed:

> *The first answer adopts the card; a second station answering the same CQ opens its
> own station card in the normal way. Nothing is discarded and nothing is swallowed,
> because nothing is hidden by the app.*

It is built that way and it is yours to overrule. The alternative worth naming: the
CQ card could stay standing until you clear it, with every answer opening its own
card. That keeps the count of calls visible for the whole session; it also leaves a
card on screen that nothing will ever finish.

**2. The task 5 specification - what lands the azimuthal map.**

- **The three numbers live in `AzimuthalMap.Settings`**, the record at
  `src/Hamlet.RadioEngine/Explore/AzimuthalMap.cs:52`: `CentreLatitude`,
  `CentreLongitude`, `RimRadiusPixels`. **The exact call sites that consume them** are
  `AzimuthalMap.Place(Settings, latitude, longitude)` at `:98`, which returns the
  pixel position of a station, and `AzimuthalMap.FromCentre(Settings, ...)` at `:144`,
  which returns its distance from the centre in pixels.
- **The image goes in `assets/`**, beside `assets/azimuthal-map.md`, and is linked as
  an `AvaloniaResource` in `src/Hamlet.App/Hamlet.App.csproj` next to line 57's
  `achievement-quill.svg`, with the same `Link="Assets\..."` shape.
- **What would still be missing, and it is more than the numbers: the consumer.**
  Nothing in `src/Hamlet.App` calls `AzimuthalMap` at all today - the only caller in
  the tree is `AzimuthalMapTests`. The card's globe is `Ft8GlobePlot` drawn by
  `Ft8GlobeControl` over `assets/world-coastline.svg`, on its own flat placement. So
  landing the image is two changes: the asset and its three numbers, then pointing the
  globe at `AzimuthalMap.Place`. On that projection a straight line from the centre is
  the great-circle path, so the old caveat is not carried across.
- **The third number must be measured off the delivered image.** The 371 from the
  817-by-916 sample is not it: that sample is centred on the wrong point and is not
  the asset.

**3. Whether the explanatory wording lives anywhere.** Task 4 took it off the hover
and, as instructed, deleted nothing. `Ft8ContactCard.Closing` is now uncalled and is
left standing. If the answer is *nowhere*, that method and the wording inside the four
composers can go in one commit; if it is *somewhere else*, it needs a home.

### Asks still outstanding

Carried inbound per HM-DEC-139, verbatim, plus what this unit adds.

1. **Does the transmission record ask the radio whether it keyed?** Unit 303's
   proposal, still yours to rule on: `Played` stays a statement about what the audio
   path did; a second, separate fact says what the radio did, read from `1C 00` and
   `15 11`, which Hamlet already polls four times a second; and where the radio does
   not answer, the second fact is **unknown**. This touches what the display asserts,
   so it is yours without exception (§12.1).
2. **Nothing in this repository can look at a picture.** Seven units have now reported
   every appearance claim as computed rather than seen. Real pixels want
   `Avalonia.Headless.Skia`, and **a package is a dependency decision and yours, not a
   session's** (§0.4). Raised once, not added.
3. **Three inherited reds, proved red before the units that found them**, never
   chased: two in `TheAchievementsScreenTests` and one in
   `TheFitGuardAsksAboutTheGridTheSendIsOnTests`.
4. **The azimuthal map asset.** Blocked on a file that is not in the tree, and it
   stays blocked. The specification above is what unblocks it.
5. **New: the two-answer rule**, item 1 of this section.
6. **New: whether the explanatory wording lives anywhere**, item 3 of this section.

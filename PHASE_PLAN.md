PHASE: Hamlet holds what it has
PHASE_SET: 2026-09-20
DESCRIPTION: A hardening phase, run unattended while Tim is away. Everything banked in the PSK31 and Olivia threads that is screen, record or test and needs neither the radio nor the owner. Judged by tests that ran and, at the end, by Tim at his window.
STEP: 0 | Settings survive an upgrade, and a send that cannot go names the true fault - no device chosen is its own refusal with a Settings link; a device that will not open carries its name, rate and OS error.
STEP: 1 | Hamlet opens whole - at the size it opens at and every size measured by unit 354, CQ, the mode tabs, the send area and Stop are on the window; the panels give up height before the send area ever does.
STEP: 2 | The record says what was true - the send press records the mode it was pressed under; every RSID code fldigi knows is in the data file with its tone sequence; the two headless flakes are made deterministic or named as environment and quarantined off the carry-forward list.
STEP: 3 | The record says what was on screen - for every decoded row and every card, whether it was drawn, filtered, scrolled away or folded, so an empty-looking screen can be diagnosed from the file without a screenshot.
STEP: 4 | The radio sheet - one page Tim reads at the radio for PSK31 and Olivia: what to press, what he should see at each step, what each refusal sentence means, where the capture goes, and what to send back if it fails.
STEP: 5 | Tim looks - at his window and, when he has time, at the radio, and says it passed.
STEP: 6 | The top gives back height - the pills, the neighborhood strip, the green zone and the rig display tightened to one band of about 180 px, the sun map keeping its size; the working panels get every pixel given up.
STEP: 7 | A PSK31 row right-clicks into a canned list - seven framed lines from an editable data file, one click sends - and the row's hover says what the row knows instead of repeating the text.
STEP: 8 | PSK31 and Olivia count for achievements - the Modes badge, the Hall of Fame firsts and the records from a PSK31 or Olivia contact, exactly as an FT8 contact earns them; measured first, then connected.
STEP: 9 | The contact Tim had - every card has an X; a station's live carrier is visible and holds the send buttons so Tim never keys on top of him; Log is on every conversation card from the start; every hand-back moves the turn, certain or guessed.
STEP: 10 | What Tim saw on 2026-09-22 - favorites are back, measured for when they vanished; a right-click on any row opens the menu; the sun map takes the top band's full height; a blind-found keyboard-mode carrier shows text only when its blocks decode.

---

# The hardening phase - the reasoning under the step list

**Set 2026-09-20 by the web thread on Tim's instruction:** *"Is there anything we can
start working on as I'll be gone for many hours? I'd like to continue progress."* The
Olivia phase halted correctly at 38 of 40 with only the owner's criteria left; it is
archived at `docs/phase-olivia-run/` with 5.4 and 6.1 open, to be closed by Tim.

## §1 What this phase is

Five steps of work that need no ruling and no radio, each already discussed and ruled
in conversation between 2026-09-11 and 2026-09-19, and one step of Tim looking. Nothing
here touches a decoder, a modulator or what keys, except step 0's refusal text at the
arm stage - which changes what is *said* about a send that does not go, never what goes.

## §2 What is the same

Every ruling of the PSK31, screen and Olivia plans stands (their `PHASE_PLAN.md` files
under `docs/phase-*-run/`). The ones leaned on: **R11** nothing at the radio; **R12** a
session rewrites its own tests; **R13** telemetry on every stage; **R14** nothing beyond
the criterion; **R19** American; **R31** unattended - criteria by id, a done step closed,
the owner's step ends the run, two rulings a unit; **§6** three stops only, the later
ruling wins. **HM-DEC-018, §2.1** nothing personal in an event. **§0.0** a sentence on the
screen is a claim; a refusal says the true reason.

## §R Rulings from the threads, applied here

**R33 - Tim, 2026-09-19: the settings loss.** *"The settings lost the listing setting."*
The transmit device setting was lost between 1.13.30 and 1.13.48 and FT8 could not send
for five days; unit 362's refusal line found it. A saved setting outlives an upgrade; a
send with no device chosen says so and opens Settings.

**R34 - Tim, 2026-09-14: Hamlet opens with every control on the window.** At 1100×780,
the size it opens at, CQ and the mode tabs were below the window (units 354, 355). The
send area never leaves the window; the panels give up height first, then the top row.

**R35 - the record's small lies.** `cq_pressed` writes `detail: Ft8` under PSK31 (seen
2026-09-11 and 09-14). Codes 72-75 have no tone sequence in `rsid-codes.json`; the
sequences are now in `assets/data/rsid-codes.json` from the same ported encoder, with
the `Squares` and `indices` tables themselves. Two app tests flake in headless runs
(`TheStopIsAlwaysOnScreenTests`, `TheTestsStayOffTheNetworkTests`, unit 365's report):
a flaking test on the carry-forward list is worse than none.

**R36 - Tim, 2026-09-11: telemetry rich enough to diagnose any issue.** Unit 322's item 3,
never taken: the record says a line was parsed but not whether the row was visible,
filtered, scrolled away or in a folded panel. That gap is why the CQ-filter fault of unit
337 needed a screenshot to find. Close it.

**R37 - the radio sheet.** Tim: *"I don't know anything about the radio."* Unit 349's sheet
was for the screen; there is none for the radio. One page, in his words, for the two
keyboard modes.

**R38 - Tim, 2026-09-21, on unit 375's two questions.** (a) Codes 72-75's tone sequences
ship in `rsid-codes.json`: they are the detector's data, and Hamlet's modulator makes only
8/250, 16/500 and 32/1000, so nothing announces a variant it cannot send. (b) The transmit
sequence's teardown abort pair, sent after the click's own, is accepted: a duplicate unkey
is the safe direction and nothing keys on it; logged as tidy-up, not a stop.

**R39 - Tim, 2026-09-21: the UI comes first.** Steps 6, 7 and 8 are worked before steps 3
and 4; step 3 depends on step 8. Ruled A on the top row: tighten everything to one band
of about 180 px - pills half height, the neighborhood strip thinner with its legend on
hover, the green zone one line, the rig display shorter with drive and power beside the
frequency - and the sun map keeps its size. Ruled C on the canned list: seven lines -
*Answer him · Send my report · Confirm and 73 · Say again? · Please repeat your report ·
QRZ? · 73 and out* - each framed with the callsigns and the hand-back, one click sends,
read from `data/psk31/canned.json` so Tim adds his own without a session. The hover on a
PSK31 row says what the row knows - station, country, grid and distance if sent, offset
and strength, when he started and stopped, whether he spoke to Tim, the parser's kind and
certainty, what a click and a right-click do - never the text again.

**R40 - Tim, 2026-09-21: keyboard modes earn achievements like FT8.** PSK31 and Olivia are
not connected to the achievement system as FT8 is. The unit measures which of the Modes
badge, the Hall of Fame firsts, the per-contact records and the CQ-list quill each mode
reaches today, then connects what is missing so a PSK31 or Olivia contact earns exactly
what an FT8 contact earns.

**R41 - Tim, 2026-09-21: Hamlet may transmit at all seven Olivia variants, each proved
by loopback.** R38 (a) rested on a wrong fact - the modulator is table-driven and makes
every variant in `data/olivia/format.json`. The four missing RSID sequences go in; a
variant goes on the air only once Hamlet's own modulator and demodulator round-trip it;
a variant that fails loopback stays off the air with a sentence saying so. Answering a
station at his variant is R27's whole point.

**R42 - Tim, 2026-09-21, on 6.1:** met at the measured floor. 180 px was the author's
number; the sun map's height makes 197 the arithmetic minimum and 214 is what was
reached with nothing lost. The criterion reads *at or under 220* and is checked.

**R43 - Tim, 2026-09-21: the UI first; the timing question is deferred.** Step 2 stays
partial where unit 377 left it - 2.4 open, the four new variants capped at the 30-second
fallback and refused above it, which is the safe direction. That is not a stop and not
the next unit's work. The arbiter goes to step 7, then step 8, before anything else.

**R44 - Tim, 2026-09-21, from the KC3FL contact.** Every card has an X; a live carrier is
visible and gates the buttons; Log on every card from the start; every hand-back moves the
turn. Step 9.

**R45 - the record's format, from the ClaudeProjectStatus arbiter, 2026-09-21.** Every
criterion is `- [ ] N.k text`, met is `[x]`, text identical in both states; a criterion
only the owner can judge ends with the owner's-verdict marker and nothing in the loop flips it;
no must-pass markers on criterion lines. Criteria marked *(nice to have)* in their text are not needed
for `done`. A unit's ADVANCES names `step N criterion k` or `none - clears a blocker:
<what>`; its WHY cites a line of this plan.

**R46 - Tim, 2026-09-22, four things on his screen.** (a) **Favorites vanished.** Saved
frequencies - the star on the rig display - so a clear, productive CW spot can be returned
to. Tim had them; a unit since removed them or their list, and the record must say which
before they are rebuilt; the saved frequencies may still be in his settings file. (b) A
right-click on any decoded row opens the menu, whether or not the parser read a callsign;
lines that need his callsign are disabled and say why; Capture and *make a card anyway*
are always on it. (c) The sun map takes the top band's full height - unit 376 kept it at
246 x 134 on the author's wording of 6.3, which is superseded. (d) A carrier the blind
search found shows text only when the mode's block decoding is confident (R9); the row
`4/500 sending Hk7DYYYzfzYXTDYY...` on 14.072 at 13:37 UTC was the fault.

## §4 The steps

Exit criteria carry ids `N.k`; met is `[x]`; R45 gives the form. A step's exit is its own assertions plus `docs/carry-forward-tests.txt` run as its comment says.

## Step 0 - Settings survive, and a refusal names the fault

**Delivers:** R33.

**Entry:** `PHASE_STATUS.md` names this phase; `TheSendReachesTheAirTests` green, run first.

**Exit:**
- [ ] 0.1 The commit and line between 1.13.30 and 1.13.48 that dropped the transmit device on load are named in the report.
- [x] 0.2 Settings files reconstructed from the tree at 1.13.30, 1.13.40 and HEAD each load with every value intact - transmit device, receive device, grid, callsign, license class, power offer, ALC reference - a missing new field takes its default, nothing present is dropped, and the file round-trips.
- [x] 0.3 At arm time, no transmit device chosen yields `send_refused reason no_transmit_device` and the sentence *No transmit device is chosen. Open Settings and pick the radio's sound card*, with Settings opened from the sentence.
- [x] 0.4 A chosen device that will not open yields `transmit_device_would_not_open` carrying the device name, the rate asked and the OS error text, in the event and on the panel.
- [x] 0.5 `TheSendReachesTheAirTests`, `TheUnslottedSendTests` and the byte-identical tests green and unedited.

*0.2 to 0.5 ticked by work instruction 385 task 0, transcribing the judging session's own verdict at `PHASE_OUTCOME.md:41`, verbatim: "Criteria 0.2 to 0.5 are met with quoted sentences, counts and green unedited suites." That is another session's verdict carried up to the record, not a judgment of this unit's. 0.1 stays unticked and step 0 stays `partial`.*

**Depends on:** nothing.

## Step 1 - Hamlet opens whole

**Delivers:** R34.

**Entry:** step 0 done; unit 354's nine-size test present and run first.

**Exit:**
- [x] 1.1 At 1100×780 and at each of unit 354's nine sizes, CQ, the mode tabs, the send area, the drive note and Stop are inside the window, asserted by measuring.
- [x] 1.2 The rule is stated and holds: when the window is too short, the working panels give up height first, then the top row; the send area's height is constant across sizes and it is never the thing that leaves.
- [x] 1.3 Below the sum of the minimums, the panels scroll inside themselves and the send area stays put.
- [x] 1.4 `BindingHealthTests` and the sheet's layout tests green.

**Depends on:** step 0.

## Step 2 - The record says what was true

**Delivers:** R35.

**Entry:** step 1 done.

**Exit:**
- [x] 2.1 `cq_pressed` and every send event carry the sub-mode the press was made under - PSK31, Olivia, FT8, FT4 - never a mapped family.
- [x] 2.2 `data/rsid/rsid-codes.json` is replaced by `assets/data/rsid-codes.json`, which carries tone sequences for codes 72-75 and the two tables; the detector reads all of them; every listed code round-trips through Hamlet's generator and detector; and under R41 every Olivia variant in `format.json` is proved by loopback - Hamlet modulates, Hamlet reads back identical - before its RSID sequence lets it on the air, with a variant that fails staying refused in a sentence.
- [x] 2.3 `TheStopIsAlwaysOnScreenTests` and `TheTestsStayOffTheNetworkTests` are run ten times each; either made deterministic with the cause named, or quarantined into a named non-carry-forward list with the environmental cause stated. No flaking test remains on `docs/carry-forward-tests.txt`.
- [x] 2.4 The carry-forward list runs green five times in a row. **Unit 386, 2026-09-22, and the two counts stand in the same sentence because work instruction 384 section 6 ruling 1 says neither may stand alone: 3 of 5 rounds were green on the first attempt with no re-run, and 5 of 5 were counted under ruling 1's re-run rule.** Over a tree frozen at commit `a91d6ed64674558df638310e931120a410a069ea`, with `git diff a91d6ed6 -- src tests assets data docs` printed empty at the end of round 5 and no commit made during the soak. Every round was both command lines as `docs/carry-forward-tests.txt` prints them at lines 7 and 9, unedited, one build each, foregrounded, with a status line immediately before each; a green round is app 245 of 245 and engine 150 of 150. **TWELVE invocations across the five rounds: ten green, TWO lost to the headless dispatcher loop at 1 ms before any assertion - round 1 app attempt 1 and round 5 app attempt 1, both recovered on the single allowed re-run - and NOT ONE RED ON AN ASSERTION.** The engine invocation was 150 of 150 on the first attempt in all five rounds and has never once met the fault. The difference between the two counts is the night's finding and it is the whole value of reporting two: the list itself did not fail once, and the 2 of 5 shortfall in the strict count is the Avalonia headless test window falling over, which is nobody's criterion.
- [x] 2.6 The archived Olivia record `docs/phase-olivia-run/PHASE_OUTCOME.md` gains an appended entry stating that its `UNIT 2 - STEP 1` of 2026-09-14 never ran - the launcher graded unit 358's leftover report as a second unit's - with the evidence (identical cost, no commit); the false row is not edited, per the append-only rule. **Unit 386, 2026-09-22, task 2, commit `a91d6ed6`.** The correction is appended at the end of that file and the evidence was measured rather than copied: identical cost, `COST: 15.045699500000005` at line 33 in `## UNIT 1 - STEP 0` and the same seventeen digits at line 48 in `## UNIT 2 - STEP 1`; no commit, `git log` over 2026-09-14 returning 77 commits belonging to exactly eleven units, 349 to 359 and no twelfth, with zero commits in the 28 minutes between `1444962f` at 11:48:17 and `c93159b6` at 12:16:33. **The false row is unedited - the diff is 36 insertions and 0 deletions, and the BOM+CRLF file is unnormalised.**

**Depends on:** step 1.

## Step 3 - The record says what was on screen

**Delivers:** R36.

**Entry:** step 8 done (R39: the UI steps come first).

**Exit:**
- [x] 3.1 Every decoded row writes one event when its visibility changes: drawn, filtered out (by which filter), scrolled out of view, in a folded panel, or removed - with the row's offset or slot and no callsign or text.
- [x] 3.2 Every card writes the same on appear, fold, scroll-out and dismiss.
- [x] 3.3 From the four-signal PSK31 fixture with the CQ filter on, the record alone says which rows were hidden and by what - the unit 337 fault reproduced and diagnosed from the file with no screenshot.
- [x] 3.4 The events are sampled so that a busy FT8 evening adds under 50 kB an hour, measured.
- [x] 3.5 The privacy scan is green over every new event.

**Depends on:** step 8.

## Step 4 - The radio sheet

**Delivers:** R37. `docs/RADIO_SHEET.md`, one page, for PSK31 and Olivia.

**Entry:** step 3 done.

**Exit:**
- [x] 4.1 For each mode: what to press and in what order; what should appear at each step, in the words the screen uses; what each refusal sentence means and what to do; where a capture goes and what to copy where; what to send back if it fails. Every sentence quoted from the tree, not paraphrased.
- [x] 4.2 A test asserts every quoted sentence exists in an operator-facing string.
- [ ] 4.3 No sentence tells the operator to touch the radio (R11).

**Depends on:** step 3.

## Step 5 - Tim looks

**Delivers:** Tim's verdict at his window, and at the radio when he has time.

**Entry:** step 4 done.

**Exit:**
- [ ] 5.1 Tim says it passed at his window and at the radio. No script can evaluate this.   *owner's verdict*

**Depends on:** step 4.

## Step 6 - The top gives back height

**Delivers:** R39's top row.

**Entry:** step 2 done, or partial with 2.1 and 2.3 met.

**Exit:**
- [x] 6.1 The top row - pills, neighborhood strip, green zone, rig display - measures at or under 220 px at 1920 and at 1400, from 300 (R42: 214 reached, floor 197); the working panels are taller by the difference.
- [x] 6.2 Nothing is lost: every pill, the strip's segments, the band and frequency, the best bet, the heard count, the drive and power offer are present; the strip's legend and the rule-of-thumb line move to a hover.
- [x] 6.3 The sun map keeps its size and its dot.
- [x] 6.4 Unit 354's nine sizes hold with the new top; `TheTopRowTests`, `TheWorkingPanelsTests`, `TheStopIsAlwaysOnScreenTests`, `BindingHealthTests` green.

**Depends on:** step 2.

## Step 7 - The canned list and the hover

**Delivers:** R39's list and hover.

**Entry:** step 6 done (R43: step 2 may be partial).

**Exit:**
- [x] 7.1 Right-click on a PSK31 row shows the seven lines from `data/psk31/canned.json`; one click sends the chosen line framed through the one unslotted sequence; the file's format is documented at its top; a malformed file is reported, not guessed.
- [ ] 7.2 Every canned send begins with its RSID, carries `announced`, counts against the cap, and writes `psk31_send_composed` with `macro: canned` and no text.
- [x] 7.3 The hover on a PSK31 row is the facts of R39 and never the text; a fact Hamlet lacks is absent.
- [x] 7.4 Olivia rows get the same list and hover by construction, asserted.
- [x] 7.5 The mode chip's fill and the send-status line name the mode that was chosen, never the family: under Olivia the Olivia chip is filled and the PSK31 chip is not, and a 29-second Olivia CQ reads *29 s of Olivia* (Tim's screen, 2026-09-21).

**Depends on:** step 6 only.

## Step 8 - Keyboard modes earn achievements

**Delivers:** R40.

**Entry:** step 7 done.

**Exit:**
- [x] 8.1 The report names, for PSK31 and for Olivia, which of the Modes badge, the Hall of Fame firsts, the per-contact records and the CQ-list quill each reaches today, measured from a fixture log.
- [x] 8.2 A logged PSK31 contact and a logged Olivia contact each earn the Modes badge's mode, the Hall of Fame first for that mode, and every per-contact record - country, state, grid, continent, miles - exactly as an FT8 contact earns them.
- [x] 8.3 The quill on a PSK31 or Olivia row means the same as on an FT8 row, from the same nudge.
- [x] 8.4 The scores and the total move on the achievements page for those contacts; `TheAchievementsPageTests` green.

**Depends on:** step 7.

## Step 9 - The contact Tim had

**Delivers:** R44 - the four things Tim saw in the KC3FL contact of 2026-09-21 17:44-17:48
UTC, read from the record: he answered, Tim sent the Report on top of the station's live
carrier, the station's 363-character reply was garbled, his second hand-back did not move
the card, and there was no way to log him.

**Entry:** step 8 done.

**Exit:**
- [ ] 9.1 Every conversation card and every receipt has a dismiss X; dismissing removes it and writes `card_dismissed`.
- [ ] 9.2 While a station's carrier is on the air his row and his card carry a color and the word *sending*; Report, Confirm, the canned lines and the typed line are held - greyed with *he is still sending* - until his carrier drops, and a send attempted during it is refused with that sentence and `send_refused reason his_carrier_live`; replayed from the 17:45:40-17:45:44 record as a fixture, Tim's Report would have been held.
- [ ] 9.3 Log is on every conversation card from the moment it exists, with the RST fields editable and defaulted to what was exchanged if anything was; a logged contact with no certain 73 logs what is known and nothing invented.
- [ ] 9.4 Every parsed line addressed to the operator with a hand-back moves the card's turn to *your turn* - marked as a guess when uncertain - not only the first answer; replayed from the 17:48:33 record, the card reads *your turn?*.
- [ ] 9.5 The four are on PSK31 and Olivia cards alike, asserted.

**Depends on:** step 8.

## Step 10 - What Tim saw on 2026-09-22

**Delivers:** R46.

**Entry:** step 9 done.

**Exit:**
- [x] 10.1 The report names the unit and the line that removed favorites or their list, and whether the saved frequencies survive in the settings file; favorites are restored - the star saves the dial and mode with a name, the list opens from the rig display, one click tunes, and the list persists across an upgrade under step 0's loader. **Unit 387: `a51bc2a6`, 2026-08-27 14:08, work instruction 029, `src/Hamlet.App/Views/MainWindow.axaml` - named with `git log -S`, not from a comment. Tim's ruling of that date removed the recent-places row and `ABANDONED_WIDGETS.md` records it; the favorites `ComboBox`, the favorite's name and its note left in the same `Grid` in the same commit AND ARE RECORDED NOWHERE. NOT this phase's own step 6: the star was measured drawn and clickable at 1920, 1400 and 1100x780 with `_starRect [62,2 67x26]` at all three, so the bail at `DrawStar` has never fired, and unit 376's `1c187df6` moved `PadBottom` 10 to 6, which is vertical. THE SAVED FREQUENCIES SURVIVE: 2 of 2 through today's loader and 2 of 2 again after a save-and-load, frequency and mode intact. BACK BY: a caret beside the star opening the view model's own `FavoriteMenu`, measured drawn and hittable at all nine of unit 354's sizes - star `[62,2 67x26]`, caret `[130,2 18x26]`, never overlapping - one line per saved place, `Manage favorites` at the foot, and one click measured putting a dial 20 kHz away back on 14,074,000 Hz. The top band did not grow by one pixel: 214 with the pills at 1920 and 1400.**
- [x] 10.2 A right-click on any decoded row - callsign read or not, live or ended - opens the menu; lines needing a callsign are disabled with a word; Capture and *make a card anyway* are always present. **Unit 387: 9 OF 9 FIXTURE ROWS OPEN A MENU WHERE 3 OF 9 DID, measured before and after on the same mixed fixture - six PSK31 carriers and four Olivia channels, station read and not, live and ended. `Capture` is live on every one of them and is the panel's own `CapturePsk31Command` asserted by reference, not a second way to start a capture. The lines that cannot be sent because Hamlet does not know the operator's own callsign are drawn grey and carry the word - *Answer him - needs your callsign in Settings* - and with his callsign, name, location and grid all set NOT ONE line on any row is grey and 15 live send lines remain, so no sendable line became unsendable. QUALIFIED, AND THE QUALIFICATION IS THE UNIT'S OWN FINDING: *make a card anyway* is present on every row, but on a row the parser read no callsign out of it is a NOTE saying why rather than a card - `OpenPsk31Card` returns at its first statement when `Psk31StationOn` is null (measured: the card count goes 0 to 0), so a working card there would be the second mechanism section 6 ruling 2(a) item 3 forbids building. A judging session should read that clause against R46(b)'s intent.**
- [ ] 10.3 The sun map grows to the top band's existing height at 1920 and at 1400 - the band itself does not grow, 214 px stands - keeping its aspect; the width it needs comes from the neighborhood strip beside it; its dot and caption kept. (Tim, 2026-09-22: vertical space is the concern, not horizontal; unit 387's reading that the map governs the band is superseded - the band governs the map.) **Unit 387: NOT MET AND REPORTED PARTIAL WITH BOTH NUMBERS, under section 6 ruling 2(b)'s *stop at what fits*. The map is 246 x 134 in a band of 214 - 62.6% of it, Tim's *two thirds* exactly - and it CANNOT BE MORE. Measured: the map already GOVERNS its card. `GreenZoneLeft` wants 25 px at 1920 and 54 at 1400, the map's column wants 145 in a row of 146, the card wants 178 and the rig column wants 178 TIED WITH MARGIN 0, so every pixel the map takes the card takes and the band takes - and the band is ratcheted at 214 by `Unit376TheTopBandTests.BandReachedWithThePills`, which the carry-forward name `TheTopBandIsOneShortRowAndThePanelsAreTallerByTheDifference` asserts, tighter than 6.1's ceiling of 220. THE ATTEMPT WAS MADE AND MEASURED: letting the map take the height available to it put it at 698 x 381, the card at 424 and THE BAND AT 460 PX, 240 over the ceiling; it was reverted in the same task. The 80 px the map does not have are the pills row and its margins (36), the card's own header and padding (32) and the caption and its gap (11). What shipped: the 134 stops being the mockup's dimension and becomes the band's budget with the arithmetic beside it, and `TheSunMapIsTheSizeItWasAndStillCarriesHisGrid` is REWRITTEN UNDER R12 AND RULING 2(b) to assert R46(c)'s rule in both directions - the map may never be smaller, and it may never leave a pixel of its own row unused, which nothing in the tree asserted before - with the band asserted beside it and every assertion about the grid marker and the caption kept.** **Unit 388, UNTICKED AND PARTIAL AT STAGE A: the map left the card's chrome for its own column in `TopRow` between the card and the rig face, and is now 327 x 178 at 1920 and 1400 where it was 246 x 134 - the full height of the band's card-and-rig row, 178 of the band's 214, 83.2% where it was 62.6% - at its own aspect, its width from the card and never the rig, its grid marker kept and its caption, words unchanged, beside it in the card's Favorites row; the band 214. Stage B, the map beside the pills as well, HELD AT 1920 (393 x 214, the band's whole height) AND FAILED AT 1400: the pills had 343 of the 675 px they need, wrapped to two rows, and the band went to 252, so it was reverted in the same task. With the map between the card and the rig face, the band's full 214 at 1400 needs the pills row to stand somewhere other than above the card, which is a layout change to Tim's pills and his.**
- [x] 10.4 A blind-found Olivia or PSK31 carrier shows characters only from blocks the decoder reports confident; otherwise the row reads *heard, not readable yet*; replayed from the 13:37 UTC record, the 4/500 row shows no text. **Unit 387, THE NAMED DROP CANDIDATE, NOT DROPPED: task 1 found the per-block confidence already crossing the seam, so nothing had to be invented. Replayed, the 4/500 row at 1500 Hz reads `heard, not readable yet` and carries NOT ONE of the characters nobody sent. The gate is both of the engine's own numbers: at least `OliviaBlindSearch.ConfirmBlocks` blocks read - the same bar the blind search clears before it names a row, whose own remark says *one block barely through is what a wrong row can do* - AND more blocks read than refused, which is the half nothing consulted: `BlocksRejected` had crossed the seam since unit 364 and the row path asked only `BlocksDecoded > 0`, so ONE ACCEPTED BLOCK IN TWELVE PUT ITS CHARACTERS ON THE SCREEN. Six boundary cases measured: 1/0 quiet, 1/11 quiet (Tim's row), 2/3 quiet, 2/2 shown, 2/0 shown, 12/11 shown, each asserting the row is EITHER the sentence OR what the channel read and never something between. An RSID-announced channel is not gated at all - the same one-block-in-twelve channel is withheld when the search found it and shown when the station announced it - and the gate is one-way, so a fade never takes words back. PSK31 needed nothing: its squelch already does this and `Psk31Listener` says so. NO ENGINE FILE CHANGED.**
- [x] 10.5 `BindingHealthTests`, `TheTopRowTests`, `TheSettingsSurviveAnUpgradeTests` and the carry-forward list green. **Unit 387 exit round, both command lines unedited, one build each, status written immediately before every invocation: APP 265 OF 265 in 2 m 42 s and ENGINE 150 OF 150 in 4 m 55 s, against the entry round's app 245 of 245 and engine 150 of 150 - the difference being this unit's twenty new names and nothing else. NO REGRESSION and nothing red that was green before (HM-DEC-165). `BindingHealthTests`, `TheTopRowTests` and `TheSettingsSurviveAnUpgradeTests` are all on that line and all green, the last at 16 of 16 with this unit's own new name in it. THE APP LINE TOOK THREE ATTEMPTS AND ALL OF IT IS RECORDED: attempt 1 lost one name to the headless dispatcher loop at 1 ms before any assertion, attempt 2 lost three, attempt 3 was clean on every one of the 265; across all three not one name ran and disagreed after this unit's own red was repaired. The engine line was green on its first attempt and has never once met the fault.**
- [x] 10.6 Favorites are a drop-down in the empty row under the green zone, inside the neighborhood card, the way Hamlet had them before 2026-08-27: one control reading *Favorites*, opening the named spots, one click tunes; the star on the rig face keeps saving; the caret unit 387 added beside the star is removed (Tim, 2026-09-22, marked on his screen). **Unit 388: ONE CONTROL, `GreenZoneFavorites`, a drop-down reading *Favorites*, in the neighborhood card directly under the green block - the row task 1 measured empty, 55 px at 1920 and 42 at 1400. Shaped after what `a51bc2a6^` shows: a drop-down beside the green block whose pick tuned and let it read its name again. Its list IS the view model's `FavoriteMenu`, asserted by identity - the Radio menu's own list - so nothing about a favorite is decided in two places: two saved places listed with `Manage favorites…` at the foot, and ONE CLICK MEASURED putting a dial at 14,094,000 Hz back on 14,074,000 at 1920 and at 1400, the control reading *Favorites* afterwards. With nothing saved it is still drawn and enabled and opens to the same note the rig face's list offered, *Nothing saved here yet - press the star to save where you are.*, not hittable, with `Manage favorites…` under it. THE STAR KEEPS SAVING (its test unedited, 1 of 1) at `[62,2 67x26]` at all nine of unit 354's sizes. THE CARET IS GONE: `RigDisplayControl.cs` differs from the commit before unit 387's caret by six comment lines and nothing else; no caret member remains, no ▾ is drawn, and a press every 6 px across the status strip outside the star saves nothing at any of the nine sizes. THE BAND 214 WITH THE PILLS AT 1920 AND 1400, NOT A PIXEL MORE. `TheFavoritesAreBackOnTheRigDisplayTests` rewritten under R12 and renamed `TheFavoritesAreUnderTheGreenZoneTests` with its carry-forward term, 5 of 5; no new event writer.**

**Depends on:** step 9.

## §5 Dependencies

Step 0 depends on nothing. The order the arbiter works is 1, 2, 6, 7, 8, 3, 4, 5 - the UI steps before the record steps by R39 - expressed in each step's Depends on.

## §6 Branching

- **Three stops only**: keying, transmit or the radio's safety; money past the budget; a
  fact the product states to the operator about the radio, a contact or a send. A hint,
  a label, a number, a layout, a test's shape, a mechanism arithmetic will not allow:
  decide, mark author's, continue.
- **The later ruling wins. A done step is closed. Every remaining step Tim's: halt.**
- **A must-pass missed by a little**: ship, report, `partial`, move on. Never loosen a test.
- **A flake cannot be made deterministic**: quarantine it with its cause named; that is
  meeting 2.3, not missing it.
- **Anything would change what goes on the air, or what keys.** `MOVE: stop`.
- **A package is needed.** `MOVE: stop`.
- **A file must be deleted.** Empty it, comment it, list it.

## §7 Carried

Every open ask of the Olivia phase from unit 368's queue; the PSK31 demodulator on
real air, waiting on a capture; Olivia 5.4 and 6.1; real flags on country cards; rank
names; the map bitmap's license; the id-scheme split.

## §8 Revision record

- **2026-09-21, evening.** R43: UI first, the timing question deferred; 2.1-2.3 checked; step 7 depends on 6 only.
- **2026-09-21, later.** R41 on the seven variants; R42 on 6.1; 7.5 added for the chip fill and the send line.
- **2026-09-21.** R38 on unit 375's two questions; R39 and R40 from Tim; steps 6-8 added and step 3 made to depend on step 8 so the UI is worked first.
- **2026-09-21, night.** Converted to the ClaudeProjectStatus arbiter's format (R45): no must-pass markers, 5.1 carries the owner's-verdict marker; 2.6 added for the false Olivia record entry; step 9 added from the KC3FL contact (R44).
- **2026-09-22.** Step 10 and R46 added from Tim's screen: favorites, the right-click on any row, the sun map at full height, blind-found text gated.
- **2026-09-22, afternoon.** 10.3 reworded - the band governs the map; 10.6 added - favorites as a drop-down under the green zone.

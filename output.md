A. THE PHASE GOAL. Hamlet reads a CQ call correctly - judged by a correctness number and, at
   the end, by Tim at the radio (criterion 5.1).
B. THIS STEP AND ITS EXIT CRITERIA. Step 6, the screen stops saying what is not so: 6.1 to 6.4,
   6.7 and 6.8 ticked, 6.6 the floors and carry-forward lines green at exit, 6.5 the Have a
   look button still open under HM-OPEN-087. This unit clears a blocker to 5.1 rather than
   ticking a step 6 criterion; 6.6 holds at its exit.
C. WHAT THIS REPORT ADDS. Item 14 built; a written answer on all fourteen of the owner's UI
   items; one word repaired. It bears on B through 6.6, which still holds, and through item 14,
   which is step 6's rule applied to the card. One finding bears on A: a PSK31 CQ that carries
   a grid is read as no station at all (P34).

READ IN THIS ORDER.

A. Item 14 built, with the card's wording quoted.
B. The fourteen, one line each: built-and-right, built-but-different with
   the difference quoted, or unverifiable.
C. The rest. Section 4 raises 6 items, none blocking.

UNIT:       427 - complete at task 4 of 4, none dropped - 2026-09-24 22:31
PHASE GOAL: Hamlet reads a CQ call correctly, and Tim at the radio agrees it read
UNIT GOAL:  a card says where a station is when his grid says otherwise than his prefix, and the owner gets a checked answer on every one of his fourteen UI items
ADVANCED:   no - a blocker-clear by design; no decoder path was touched
NUMBER:     of the owner's fourteen: 11 built-and-right, 3 different, 0 unverifiable
DRIFT:      0

## 1. What Claude did

**Complete, at task 4 of 4, none dropped.** Machine QUIVERFULL, project Hamlet, confirmed by
the gate (SHACK_FACTS.md and the Cw decoder present, no CoreHMI.sln or MURC.sln), branch main.
Six commits, all pushed with push rc 0: 44f93134, 32b0c155, fada9f7f, 8f0f4117, ddeb5c95
and the exit commit that carries this file.

**Task 0, the record.** HM-DEC-180 goes in DECISIONS.md, newest first, as the instruction wrote it.
The CLAUDE.md §1 row is dated 2026-09-24 under the headline *A station's grid beats his prefix's
entity*. PHASE_OUTCOME.md has `UNIT 427 - STEP 6` from the decision block. PHASE_STATUS.md
names unit 427, with CURRENT_STEP at 6. The version goes from 1.13.113 to 1.13.114. The
entry round:

- Build: 0 errors.
- Engine carry-forward: 178 of 178.
- App carry-forward: 277 of 278. The one loss was to the dispatcher loop, and that name passed
  alone.
- Captures 51 of 51. Adjudicated 13 of 13. Keyed floors 13 of 13.

**The instruction against the tree (section 5).** It matches, with two notes:
- A card's entity came only from the prefix, through `DxccPrefixes.EntityOf` in
  `Ft8ContactCard.WhereHeIs` and `CountryOf`, and in line 2 of the text row's hover.
- The grid reaches the card as `Ft8CardFacts.Grid` and the text row as `DigitalDecodeRow.HisGrid`.
- Nothing compared the two. `EntityQualifier`'s own remarks ruled the opposite (*where the two
  disagree the callsign wins*), and HM-DEC-180 now overrules that. **No grid-to-place table
  existed anywhere in the tree.**
- The engine says *entity*. The card said *country*, in `_country` and `CountryOf`.
- The new-entity quill is `NudgeSet.WouldOpen`, and it keyed on the callsign alone.
- `data/psk31/canned.json` has seven lines.
- No item turned out to have no seam. Every one of the thirteen was driven.

**Task 1, the grid wins.** I watched it fail first: `TheGridBeatsThePrefixTests` was red 1 of 4
(commit 32b0c155). Then I built it:
- `data/callsigns/grid-places.json` holds twelve boxes covering California, the lower 48,
  Alaska and Hawaii, each with a reason.
- `GridPlaces.Of` places a four-character square only where the whole square lies inside one
  box. `GridPlaces.Contradiction` compares that place with the prefix.
- Four places ask it: the card (its face line, its header, the globe caption and a hover row),
  line 2 of the text row's hover, and `NudgeSet.WouldOpen` and `Explain`, which now take the
  grid he sent.
- The PSK31 row's `HisGrid` is now set before the quill is asked.

After: 4 of 4. `ThePsk31ReadsTheConversationTests` failed on its hash pin of `NudgeSet.cs`. I
re-pinned it under HM-DEC-180, with a comment, as unit 327 did. That was a decision of my own.

**Task 2, the sweep.** `TheFourteenOnScreenTests` has twelve facts that assert nothing. Each
item is driven through the view model or a realized headless window, and what is there is
printed. Before counting item 13 as different, I checked that no unit 390 commit built it
(406d1efe to 74fd016a).

**Task 3, one word.** I made one repair: item 11's empty favorites line, watched red first (3
of 4). Items 1, 8 and 13 were larger than a word, so I parked them as P31 to P33.

**Task 4, the exit round.**
- `Hamlet.sln` builds non-incremental with warnings as errors: 0 warnings, 0 errors.
- App carry-forward: 277 of 278. The one loss was to the dispatcher loop
  (`TheFavoritesAreChipsTests.ThreeChipsCostTheTopBandNothing`), and it passed alone, 1 of 1.
- Engine carry-forward: 178 of 178.
- Captures 51 of 51 in 119 s. Adjudicated 13 of 13. Keyed floors 13 of 13.
- Every touched type, 27 in the app and 2 in the engine: all green except the two that were red
  at entry.
  - `Unit297CardSentenceTests` is 9 of 11. The same two failed with this unit's work stashed.
    Parked as P29.
  - `DecisionLogOrderTests` is red on the HM-DEC-166 gap only (P25). Its ordering half passes
    with the new row.
- `src/Hamlet.RadioEngine/Cw` shows nothing against entry. The transmit files show nothing
  against 7e209cb4.

**Nothing that was green at entry is red at exit.**

**Decisions I made for myself, all overrulable:**
- **The card's wording.** Quoted in section 2.
- **Drawing the box table myself.** The instruction needed to know where a grid square is, and
  nothing in the tree could say. I drew the boxes from border coordinates I know, not from a
  cited boundary file, and marked that in the file and as P30.
- **Saying the United States entity as *the lower 48*** in the card's sentence.
- **Re-pinning the NudgeSet.cs hash.**
- **The sweep's form:** one fact per item, catching exceptions so each prints.

## 2. What the owner should expect

**A station whose grid puts him somewhere other than his callsign's entity now says so on his
card.** For WL7E from CM98, the card reads:

- Face: *WL7E, an Alaska callsign, operating from CM98 in California. His grid decides the
  entity: the lower 48, not Alaska.*
- Header: *California · 2,200 miles*
- The `i` adds: *DXCC counts Alaska as an entity apart from the lower 48. A callsign says where
  it was issued; a grid he sends says where he is.*

His row carries no quill and neither does his card. An Alaska call from an Alaska grid, or one
that sent no grid, still earns the Alaska quill. On a PSK31 or Olivia row, line 2 of the hover
reads *an Alaska callsign, operating from CM98 in California*.

**What will look wrong but is not:**
- **Most contradictions are not caught yet.** The table knows only California, the lower 48,
  Alaska and Hawaii. A Canadian call from a US grid, or a US call from Puerto Rico, still reads
  as its prefix, exactly as before.
- **Border squares are never placed.** A square that straddles a border is left alone on
  purpose, so a station just across a line keeps his prefix's entity.
- **The empty favorites line** now reads *no spots saved yet - press the star to keep this one*.

## 3. What you should see

The owner's fourteen, as they are on screen tonight:

| # | Item | Verdict | What is there |
|---|---|---|---|
| 1 | Right-click canned list | **different** | The menu opens on every row. Lines needing his callsign are disabled with *needs your callsign in Settings*. On a row with a callsign, *make a card anyway* makes a card. **On a row with none it reads *Make a card anyway - Hamlet read no callsign on this row, so a card would have nobody on it...*, has no command, and a press makes no card.** P31. |
| 2 | Row hover | right | Station, entity, grid and miles where sent, offset and strength, *Heard since*, *This one is addressed to you.*, the kind and certainty, and what a click and a right-click do. Never the text. |
| 3 | The top gives back height | right | The band is 214 px with the pills and 178 without, at 1600 and at 1300. The pills are 30 px. Drive and power sit in the rig column. |
| 4 | Sun map | right | 393 x 214 at x 16 at 1600: the band's height, aspect 1.84. At 1300 it drops back into the card at 294 x 160. |
| 5 | Keyboard modes earn as FT8 | right | PSK31 and Olivia equal FT8 on all seven kinds and add their own first (*first_psk31*, *first_olivia*). The quill *Canada · new country* is on both rows. The per-contact records are held by the green log-path test, not re-driven here. |
| 6 | Chip and send line name the chosen mode | right | Under Olivia the Olivia chip is filled and PSK31 is not. The line reads *Sent "CQ CQ CQ de KC3QIS KC3QIS KC3QIS pse K" - 29 s of Olivia.* |
| 7 | Dismiss X, recorded | right | ✕ is on the card. After a press, `card_dismissed` is recorded once and the card's removal is recorded as dismissed. |
| 8 | His live carrier | **different** | *sending* on the row, *he is still sending* on the card, the action held, and a typed send refused. **The hold releases on K or BTU, and also on carrier drop. The refusal says *It goes out the moment his carrier drops*, but nothing goes out until he presses again.** Releasing on K or BTU is by design. P33. |
| 9 | Log on every card, RST his | right | Log is there half an exchange in. RST_SENT and RST_RCVD are editable, marked *heard by Hamlet* and *yours - typed by you*. Every other field is read-only. |
| 10 | Every hand-back moves the turn | right | A certain hand-back gives *Your turn*. A guessed one gives *Your turn?*. |
| 11 | Favorites chips | right after task 3 | Chips under the green block in the mode's ink, one click tunes, ✕ on hover, and the star fills on a saved spot. The empty line read *press ☆*; it now reads *no spots saved yet - press the star to keep this one*. |
| 12 | Blind carrier | right | A blind Olivia row reads *heard, not readable yet*. So does a PSK31 carrier with no sure characters. |
| 13 | His card while he talks | **different** | *KC3QIS de VE3YX GM OM ...* mid-over opens **no card** on PSK31 or Olivia, and the right-click names nobody. P32. |
| 14 | The grid wins | built | Section 2. |

## 4. What's blocking us

Nothing here blocks the phase (R65). Six items for the owner, most useful first:

1. **P34 - a PSK31 CQ with a grid in it is read as no station.** `CQ CQ CQ de VE3YX VE3YX FN03
   pse K` gives a row with no station, no entity and no kind. The same CQ without the grid
   reads fine. *Proposed:* a later unit takes it to the engine's parser, because it is the phase
   goal in the keyboard modes. *Rejected:* opening the parser in this unit, because the
   instruction kept it closed.
2. **P32 - item 13 is not built.** A station's first over still arriving opens no card.
   *Proposed:* treat it as a unit of its own that parses a growing row. *Rejected:* building it
   here, because the instruction says findings only.
3. **P33 - item 8's sentence promises a send that does not happen**, and the hold also lets go
   on carrier drop. *Proposed:* the owner rules on the sentence's words (the send path, §0.2)
   and on whether carrier drop should release the hold. *Rejected:* changing either here,
   because both are parked.
4. **P31 - item 1: should *make a card anyway* make a card for a row with no callsign?** Today
   it is a note, on §0.0 grounds. *Proposed:* the owner says which he meant. *Rejected:*
   guessing.
5. **P30 - the grid table covers four places and cites no boundary file.** *Proposed:* the
   owner rules on whether to cite a source and widen it. *Rejected:* widening it from memory.
6. **P29 - Unit297CardSentenceTests has two reds that predate this unit.** *Proposed:* a later
   unit retires or restores the two expectations. *Rejected:* touching them here, under §12.6.

Carried: P27, the attenuator sentence, is still the owner's, and 7.8 stays unticked.

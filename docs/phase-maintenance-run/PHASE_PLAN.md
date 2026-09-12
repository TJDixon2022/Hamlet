PHASE: The screen says what is true and looks like someone meant it
PHASE_SET: 2026-09-12
DESCRIPTION: A maintenance phase. Everything Tim banked in the PSK31 thread that is screen and not radio - the achievements pages, the green zone, the layout, the record - built and judged from his own window.
STEP: 0 | The green zone finishes - the band pills come off, the map takes the space, the stale line goes, the files that sessions cannot delete are listed once for Tim.
STEP: 1 | Every achievements category page is trading cards - the contact that earned each card with its path map, distance, band, mode and date; the next card carries who is calling now from an unworked place. All eight kinds.
STEP: 2 | The record is honest - States scored from the ADIF STATE field, rank names read from the points file, the points file documented, the 1400 px layout settled.
STEP: 3 | Tim looks at it - every page at his window size, and says it passed.

---

# The maintenance phase - the reasoning under the step list

**Set 2026-09-12 by the web thread on Tim's instruction:** *"do the cleanup stuff I've
been talking about. We're calling this a maintenance phase."* The PSK31 phase is archived
at `docs/phase-psk31-run/` with its step 6 open; Tim closes it at the radio when he
closes it, and that is not this phase's business.

## §1 What this phase is

Three steps of screen work and one of Tim looking. Nothing here touches the radio, the
transmit chain, a decoder or a parser. Everything here was seen by Tim on his own screen
between 2026-09-11 and 2026-09-12 and ruled on in conversation; the rulings are in §R
and the pictures he approved are under `assets/`.

## §2 What is the same

Every ruling of the PSK31 phase's plan stands - R1 through R20 in
`docs/phase-psk31-run/PHASE_PLAN.md` - and is not restated. The ones this phase leans on:
**R12** a session rewrites its own tests and never asks the owner; **R13** telemetry on
every new stage; **R14** tests prove criteria and nothing beyond; **R16** the two quills;
**R19** American; **R20** the achievements pages and the green zone as ruled; **§6's
later-ruling-wins rule**.

`ACHIEVEMENTS_PHILOSOPHY.md` governs the achievements screen: **§2** no wall of blanks;
**§3.1** inside a category nothing shows until something adjacent is earned, bent only by
Ruling C (the nearest unearned card in each kind is visible); **§3.5** the teaching is
the product; **§4** worked, never confirmed, nothing shames. `CLAUDE.md` **§0.0** a picture
binds as hard as a sentence; **§0.5** family color is text only; **§0.6** color never the
only carrier. **HM-DEC-155**, **HM-DEC-139**, **FACT-004**, **FACT-006**.

## §R Rulings, Tim, 2026-09-12

**R21 - the green zone.** *"Too redundant. We don't need the repeat of the band list on
the green. Maybe make the map bigger."* The band pills come off the panel - they are the
row above it - and the map takes their space. The band stays large on the left; the
heard-just-now count stays as a number on the right; the rule-of-thumb line stays.

**R22 - the category pages.** *"So boring."* *"Communicate visually and be appealing.
Not white bread boring."* *"I like it, just make sure all the other sub pages are as
interesting."* The shape is `assets/category-page-countries.png`: the category's color
band and emblem across the top with its count, score, level and a bar to the next level;
**each earned card is the contact that earned it** - the entity large, the callsign and
grid, a map of the path cropped to the two stations as the conversation card draws it,
the distance in large type, band, mode and date, the points; **the next card says what it
wants and the one thing Hamlet knows that helps** - who is calling CQ right now from a
place that would earn it, with distance, from the CQ list. Per kind: **Countries,
States, Grids** the contact that earned it; **Continents** seven badges, each the first
contact that opened it, the count of countries worked there since, and the unearned ones
naming the continent and who is calling from it now; **Total Miles** a tier is a bar
filling toward the next line and the contact that crossed it; **Bands** the first contact
on that band and which band is the best bet now for the next; **Modes** the first contact
in each mode and where the unearned mode lives and who is there; **Hall of Fame** the
contact that earned each first, and the nearest first as next. **Nothing white, nothing
empty.** No image assets; vector and the map the app already has.

**R23 - the record.** States are scored from the ADIF `STATE` field, which the log
already writes for a US contact; a contact with no `STATE` scores nothing. Rank names
are read from `achievement-points.json` under a `rank_names` key, defaulting to
`Rank 1`…`Rank 8` when absent, so Tim can name them without a session. The points file's
every key is documented in a comment block at its top.

**R24 - the 1400 px layout.** The author's proposal, marked for Tim: the split between
the decoded list and For You is a fraction of the tab, not a pixel count; at 1400 the
decoded list may abbreviate the message column to the callsign and grid rather than
shrink For You below what the card's table needs; the card's table goes under the map
only when even that is not enough. Tim overrules by looking.

**R25 - Tim looks.** Step 3 is his verdict at his window, at the size he uses.

## §3 What is different from a build phase

Nothing decodes, transmits or logs differently after this phase. A step's exit is what
is on the screen, asserted by computation and described in words, and then Tim's eyes.
**Every appearance claim is computed, not seen, and says so.**

## §4 The steps

Each step verifies its own ground. Exit criteria are tiered. **A step's exit is its own
assertions plus the carry-forward list, run as the list's own comment says** - not the
whole suite (HM-DEC-155; `CLAUDE_CODE.md` wins on unit shape).

## Step 0 - The green zone finishes, and the leftovers go

**Delivers:** the green zone without the band pills, the map at the width they held;
the stale line gone; the files sessions cannot delete listed once in the report for Tim.

**Entry:** the tree is Hamlet's; `PHASE_STATUS.md` names this phase.

**Exit:**
- The band pills are not on the green zone; the map is wider by the space they held,
  stated in pixels. *must-pass*
- The band, frequency, mode and license line stay on the left; the heard count stays on
  the right; the rule-of-thumb line stays. *must-pass*
- The line *Hamlet cannot work CW, PSK31 and Voice yet* exists nowhere. *must-pass*
- `BindingHealthTests` and `VoiceTests` green; the carry-forward list green. *must-pass*
- The report lists every file the session could not delete, once. *must-pass*

**Depends on:** nothing.

## Step 1 - Every category page is trading cards

**Delivers:** R22 on all eight kinds.

**Entry:** step 0 done - the achievements window opens to eight badges with no scroller
and a badge clicks in, checked by running `TheAchievementsPageClicksInTests` first.

**Exit:**
- A category page has the color band with count, score, level and a bar to the next
  level. *must-pass*
- An earned card carries the entity, callsign, grid, a path map cropped to the two
  stations, distance, band, mode, date and points, from the log entry that earned it.
  *must-pass*
- The next card names what it wants and, when anyone on the CQ list would earn it, lists
  them with distance; otherwise says no one is calling from there now. *must-pass*
- All eight kinds render this way, each with its own content per R22; Continents opens
  to seven badges and each to its countries. *must-pass*
- No string in any slot clips or wraps a word, asserted by measuring at 1400 and 1920.
  *must-pass*
- No card is a white rectangle: every card has a map, a bar, or a live list. *must-pass*
- Telemetry: `achievement_category_opened` carries the kind and the count of cards
  rendered. *must-pass*
- A card's map opens in the popup on click, as the conversation card's does.
  *nice-to-pass*

**Depends on:** step 0.

## Step 2 - The record is honest, and the layout settled

**Delivers:** R23 and R24.

**Entry:** step 1 done.

**Exit:**
- A US contact with `STATE` in the log scores its state; one without scores nothing; the
  States badge count matches the log. *must-pass*
- `rank_names` in the points file names the ranks; absent, the defaults show; the file's
  keys are documented at its top. *must-pass*
- At 1400 the decoded list and For You share the tab by R24, no callsign is clipped, and
  the card's table is beside the map or under it by the stated rule; at 1920 beside.
  *must-pass*
- The report states the split at both widths. *must-pass*

**Depends on:** step 1.

## Step 3 - Tim looks

**Delivers:** Tim opens every page at his window size and says it passed.

**Entry:** step 2 done.

**Exit:**
- Tim says it passed. *must-pass* No script can evaluate this.

**Depends on:** step 2.

## §5 Dependencies

Step 0 depends on nothing. Steps 1-3 are one pipeline. When a step blocks there is
nowhere to route; work it or halt.

## §6 Branching

- **A later ruling of Tim's contradicts a line of this plan.** The later ruling wins; the
  arbiter proceeds, notes it, does not stop.
- **A must-pass is missed by a little.** Ship, report the number, mark `partial`, move
  on. Never loosen a test.
- **A string will not fit.** Shorten it and say which, or widen the slot; never clip.
- **A fact for a card is missing from the log** - no grid, no date. The card shows what
  it has and no dash; a map with no grid is no map, and the card says so in a word.
- **Anything touches the radio, a decoder, a parser or the transmit chain.** `MOVE: stop`.
  This phase is screen only.
- **A package is needed.** `MOVE: stop`.
- **A file must be deleted.** Empty it, comment it, list it; never stop for it.
- **The watchdog.** Until the arbiter repository's process-polling watchdog is installed,
  status before every `dotnet` command, after every commit and after every task, read
  from the clock with `tools/status.sh`, never composed.

## §7 Carried

Every open ask of the PSK31 phase, from unit 333's queue, carried verbatim by every unit.
Plus: real flags on earned country cards - undecided, Tim's; the recording of real PSK31
audio - still the one thing that answers why a strong signal read badly; the id-scheme
split; the map bitmap's license.

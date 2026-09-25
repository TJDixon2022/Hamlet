# Work instruction 427 - the grid wins, and the fourteen are checked on screen

**Seed under `--seed`.** Thirteen of the owner's fourteen UI items are marked built by units
376 to 390. One, the grid-against-prefix rule, has never been written. **This unit builds
that one and then drives all fourteen to see what is actually on screen**, because ticked and
right have come apart twice this week. Five tasks, drop from the back.

**Status.** `sh tools/status.sh`, real clock, after every commit and every task, and
immediately before every `dotnet test`. **Write files as UTF-8.**

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite. Only this unit's names and `docs\carry-forward-tests.txt`, run as
its top comment says. **Never background and poll.** One type per invocation, each with its
own `timeout`. The captures type is 51 rows; give it 600 s.

**This unit works in the app project.** The headless dispatcher loop loses a name most runs;
a loss before any assertion is re-run once and counts neither way.

**The report's four top-level headings are exactly these, character for character:**

```
## 1. What Claude did
## 2. What the owner should expect
## 3. What you should see
## 4. What's blocking us
```

**The `UNIT:` line carries no parentheses**, and no `&`, `|`, `<`, `>`, `^`.

**Write `output.md` at the root before the session ends, whatever else happened.**

**Nothing in section 4 halts this phase** (R65). Park it and go on.

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. A bare
`git worktree`, `git checkout` and `git show` are refused at the prompt. Multi-step commands
go into `.run-unit\unit427-<name>.sh` and run with `sh`.

## 3. Asks still outstanding

Carried per HM-DEC-139, verbatim in section 4. **P27, the attenuator sentence outside an owned
block, is the owner's and is not this unit's** - 7.8 stays unticked until he rules. Nothing
else is this unit's to answer.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet reads a CQ call correctly.
UNIT GOAL:  A station's grid beats his prefix's entity on the card, and all
            fourteen of the owner's UI items are checked against the screen.
ADVANCES:   none - clears a blocker: criterion 5.1
DRIFT:      0
```

**The owner's list, 2026-09-21 to 09-23, in his words, with what the record says of each.**
Items 1 to 13 are marked built by units 376 to 390; unit 390's own report records *7.5 earned,
9.3 under ruling A, 9.2/9.4/9.5/7.2/4.3/0.1/10.3 ticked with numbers, favorites as chips*.
**Item 14 has never been written.**

1. A PSK31/Olivia row right-clicks into a canned list - seven framed lines from
   `data/psk31/canned.json`, one click sends. Menu opens on **every** row, callsign read or
   not; lines needing a callsign disabled with a word; Capture and *make a card anyway*
   always present, and *make a card anyway* makes a **card**, not a note. Units 378, 387, 390.
2. The row hover says what the row knows - station, country, grid and distance if sent, offset
   and strength, when he started and stopped, whether he spoke to you, the parser's kind and
   certainty, what a click and a right-click do - **never the text again**. Unit 378.
3. The top gives back height - one band, pills half height, strip thinner, green zone one
   line, rig display shorter with drive and power beside the frequency. Unit 376, 214 px.
4. The sun map grows to the band's height, aspect kept, width from the neighborhood strip; the
   band does not grow. Unit 389, 393 x 214 at the band's left edge; drops back below 1400 wide
   or when the strayed-frequency line shows.
5. PSK31 and Olivia count for achievements exactly as FT8 does - Modes badge, Hall of Fame
   first, every per-contact record, the quill. Unit 379.
6. The mode chip's fill and the send-status line name **the mode chosen, never the family** -
   under Olivia the Olivia chip is filled and PSK31 is not; a 29-second Olivia CQ reads
   *29 s of Olivia*. Unit 390 task 3.
7. Every card has a dismiss X, and the press is recorded. Unit 385.
8. A station's live carrier is visible - his row and card colored, the word *sending* - and the
   send buttons are held with *he is still sending* until his hand-back. Unit 385; **the hold
   releases on K or BTU, not on carrier drop, by design**.
9. Log is on every conversation card from the start; RST fields editable, heard values marked
   *heard* and typed ones *yours*, the rest of the dialog read-only. Unit 385, unit 390 task 5.
10. Every hand-back moves the turn, certain or guessed - *Your turn?* for a guess. Unit 385.
11. Favorites: the star saves dial and mode with a name; the spots live **under the green
    zone** as a row of **chips**, each in its mode's color, one click tunes, x on hover
    forgets, the star fills on a saved spot; empty reads *no spots saved yet - press the star
    to keep this one*. Unit 390.
12. A blind-found Olivia or PSK31 carrier shows text only from blocks the decoder reports
    confident; otherwise *heard, not readable yet*. Unit 387.
13. The man talking to you gets a card **while he is talking** - the parser reads a row's text
    as it grows; *KC3QIS de VE3YX* opens his card mid-over marked *he is sending to you*,
    buttons held until his hand-back; the right-click names him. Any keyboard mode. Unit 390.
14. **Not built.** When a station's grid contradicts his prefix's entity, **the grid wins**;
    the card says both - *WL7E, an Alaska callsign, operating from CM98 in California* - and
    **no new-entity quill for him**. The word is **entity**, with a note that DXCC counts
    Alaska apart from the lower 48.

---

## 5. Verify this instruction against the tree

Check, report any mismatch, repair nothing:

- Where a card's entity is decided from a callsign prefix, and where a grid is read from a
  received message.
- Whether anything already compares the two, and what an entity is called in the code today -
  *country*, *entity*, *DXCC* or otherwise.
- Where the new-entity quill is awarded, and what it keys on.
- `data/psk31/canned.json` exists with seven lines; the favorites chips live under the green
  zone; the mode chip is filled by chosen mode.
- **If any of items 1 to 13 cannot be driven by a test at all** - no seam, no view model
  property - say which, and task 2 reports them as unverifiable rather than as failures.

## 6. Rulings in force - do not re-argue

`PHASE_PLAN.md` R59 to R74, §3 and §6.

**The owner's ruling on item 14**, 2026-09-23: the grid wins; the card states both; no
new-entity quill; the word is *entity*, with a note that DXCC counts Alaska apart from the
lower 48.
**§0.0** never state as known what is not known - a card that gives an entity from a prefix
while a grid says otherwise is stating a falsehood.
**§0.2** nothing that keys or transmits is touched. **§0.6** color is never the sole carrier -
item 8's *sending* is a word as well as a color, and item 11's chips carry their mode in text.
**§0.5** family color is text only.
**HM-DEC-155**, **HM-DEC-165**, **FACT-004**, **FACT-006**.

**Record this in `DECISIONS.md`, newest first, and one row at the top of `CLAUDE.md` §1's table
dated 2026-09-24, headline **A station's grid beats his prefix's entity**, ref HM-DEC-180:**

```
---
id: HM-DEC-180
date: 2026-09-24
refs: work instruction 427, CLAUDE.md 0.0, the owner's UI list 2026-09-21 to 09-23 item 14
---

**When a station's grid contradicts the entity his callsign prefix implies, the grid wins.**
Tim, 2026-09-23.

**What is ruled.** The card states both - for example *WL7E, an Alaska callsign, operating
from CM98 in California* - and the station earns no new-entity quill on the strength of the
prefix. The word used is **entity**, and the card notes that DXCC counts Alaska apart from the
lower 48.

**Why.** A prefix says where a callsign was issued, not where the operator is. A grid he sent
says where he is. Stating the prefix's entity as his location while holding a grid that says
otherwise is CLAUDE.md 0.0 broken: the app would be stating as known something it holds
evidence against.

**Whose words are whose.** The ruling is Tim's; the wording is work instruction 427's record
of it.
```

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the record

`PHASE_OUTCOME.md` gets its `## UNIT 427 - STEP 6` entry from the decision block at the foot of
this file. `PHASE_STATUS.md` names unit 427. Patch-bump `Directory.Build.props`.
`DECISIONS.md` HM-DEC-180 and the `CLAUDE.md` row. **Entry round:** both carry-forward lines
and the three floor tests, recorded.

**Drop candidate:** none.

### Task 1 - the grid wins (item 14)

Watch a test fail first: a station whose prefix implies one entity and whose received grid
falls in another - `WL7E` from `CM98` is the owner's own example - and assert that the card
states both, in his form, and that no new-entity quill is awarded for that station.

Then build it. **The card's exact wording is the author's** and goes in the report, but it
must name the callsign's entity, say *operating from*, and give the grid and where it is, and
it must use the word **entity**. The DXCC note about Alaska goes wherever the card explains
itself.

**Where the grid is absent or unparseable, nothing changes** - the prefix's entity stands, and
the card says what it always said.

**Drop candidate:** none.

### Task 2 - the fourteen on screen

A fact that asserts nothing, in the app test project, driving each of items 1 to 13 and
printing **what is actually there**: for each item, the property, the text and the numbers.
Specifically:

- **1**: open the menu on a row with a callsign and on one without; print every entry, which
  are disabled, the disabled word, and whether *make a card anyway* produces a card or a note.
- **2**: print the hover text of a row, whole, and say whether the text of the transmission
  appears in it.
- **3, 4**: print the band's height, the pills' height, the sun map's width and height, and its
  left edge against the band's, at 1600 wide and at 1300 wide.
- **5**: print whether a PSK31 contact and an Olivia contact each reach the Modes badge, the
  Hall of Fame, the per-contact records and the quill.
- **6**: under Olivia, print which chip is filled and what the send-status line says for a
  29-second CQ.
- **7**: print whether a card has a dismiss X and whether the press is recorded.
- **8**: print the row and card state while a carrier is live, the word shown, and what the
  send buttons say; **say plainly that the hold releases on K or BTU rather than carrier drop**.
- **9**: print which Log fields are editable, and how heard and typed values are marked.
- **10**: print what the turn does on a certain hand-back and on a guessed one.
- **11**: print whether the spots are chips, where they sit, their colors, what the star does
  on a saved spot, and the empty text verbatim.
- **12**: print what a blind-found carrier shows before the decoder is confident.
- **13**: print whether a card opens mid-over for *KC3QIS de VE3YX*, what it is marked, and
  what the buttons say.

**Each is reported as built-and-right, built-but-different, or unverifiable, with the
difference quoted.** Repair nothing here - a difference is a finding for a later unit.

**Drop candidate:** items 3, 4 and 5 last, if the clock runs short.

### Task 3 - repair what is one line wrong

**Only** items task 2 found built-but-different where the difference is a word, a color or a
number the owner's list states plainly - the empty favorites text, the disabled word, *29 s of
Olivia*, a chip that should be filled. Each watched failing first, each in its own commit.

**Anything larger than that is a finding**, written to `docs\phase-correctness\PARKED.md` with
what was expected and what is there. **Do not rebuild a feature in this unit.**

**Drop candidate:** whole task. Say what was left.

### Task 4 - the exit round

`Hamlet.sln` builds with warnings as errors. Both carry-forward lines, the three floor tests
with captures at 51, and every type touched. `src\Hamlet.RadioEngine\Cw` prints nothing against
entry; the transmit files print nothing against `7e209cb4`.

---

## 9. Parked - do not touch, do not raise

- **P27, the attenuator sentence.** The owner's; 7.8 waits on him.
- **3.6 the stray letters, step 4 the pitch judge, 7.1 to 7.4 the speed ceiling and
  acquisition, 6.5 the dead button.** The arbiter routes there next; not this unit's.
- **The hold releasing on K or BTU rather than carrier drop.** By design; report it, do not
  change it.
- **Any feature rebuild.** Findings only.

## 10. What not to do

- **Do not rebuild anything in items 1 to 13.** Report the difference.
- **Do not award a new-entity quill on a prefix** the grid contradicts.
- **Do not use the word *country* where the ruling says *entity*.**
- **Do not change what keys or transmits, or the decoder.**
- **Do not halt for a question.** Park it.
- **Write `output.md` before the session ends.**
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Report mismatches; repair nothing beyond task 3's one-line cases. American spelling.
  UTF-8. The four headings exactly.**

## 11. Committing and pushing

Commit per task. Push at the end and say whether it succeeded.

---

## 12. Reporting

`output.md` at the root, the four headings exactly as section 1 gives them.

```
READ IN THIS ORDER.

A. Item 14 built, with the card's wording quoted.
B. The fourteen, one line each: built-and-right, built-but-different with
   the difference quoted, or unverifiable.
C. The rest. Section 4 raises <n> items, none blocking.
```

```
UNIT:       427 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
ADVANCED:   no - a blocker-clear by design
NUMBER:     of the owner's fourteen: <n> built-and-right, <n> different, <n> unverifiable
DRIFT:      0
```

**Section 3 leads with the fourteen-line table.** That is what the owner asked for.

---

```
ARBITER-DECISION
STEP: 6
APPROACH: build the grid-beats-prefix rule with the card stating both and no new-entity quill, then drive the owner's other thirteen UI items and print what is actually on screen for each, repairing only one-line differences
MOVE: continue
WHY: PHASE_PLAN.md step 6 asks that every sentence the app states about the radio or a signal be true or say it does not know, and a card that gives an entity from a prefix while holding a grid that contradicts it states a falsehood; the sweep clears the way to criterion 5.1, the owner's verdict
STATE: partial
DECIDED: the card's exact wording, the form of the sweep, which differences count as one-line repairs, and the per-type timeouts are the author's, overrulable
LICENCE: HM-DEC-180; PHASE_PLAN.md R65, R74, section 6; CLAUDE.md 0.0, 0.2, 0.5 and 0.6; HM-DEC-155; HM-DEC-139; FACT-004
ACCOMPLISHED: a station's card says where he actually is, and the owner has a written answer on all fourteen of the things he asked for this week
ADVANCES: none - clears a blocker: criterion 5.1
END-ARBITER-DECISION
```

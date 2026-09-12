# Work instruction 325 - the screen says what is true

**Seed under `--seed`.** Six things Tim saw at the radio on 2026-09-11, now ruled, all on
the digital screen PSK31 shares with FT8 - plus the one number that makes step 4 `done`.
**Six tasks, each small. Write status before every `dotnet` command.**

---

## 0. The project gate

```
SHACK_FACTS.md                                          must exist
src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs     must exist
CoreHMI.sln                                             must not exist
MURC.sln                                                must not exist
root                                                    C:\Source\HamLet
```

**If any of the four is wrong, stop and say so in `output.md` section 4. Write nothing
else.** The refusal text: *This is not Hamlet. Nothing was changed.*

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite; only this unit's names and `docs\carry-forward-tests.txt` -
**26 types, two builds, run exactly as the comment at the top of that file says.** Never
background and poll. **The watchdog kills at twelve minutes of silence: write
`PROJECT_STATUS.md` immediately before every `dotnet test` and `dotnet build`.**

---

## 2. The tool fact

**The shell breaks on an apostrophe inside a quoted heredoc and collapses a doubled
backslash.** Write *do not*; single backslashes; check what landed. **Commands joined by
`;` are refused.** This environment **cannot delete or rename files** - if a file must
go, empty it, leave a one-line comment, and raise it.

---

## 3. Asks still outstanding

Carried per HM-DEC-139 from unit 324's queue. **Every item comes back in section 4,
verbatim where unresolved.** Closed by ruling tonight, report as closed:

- **Unit 324 item 1**, the ALC zone - **closed by §R15**, task 6 builds it.
- **Unit 324 item 2**, the *heard, not readable yet* row - **accepted by Tim**; stays.
- **The quill cap**, **collapsed panels**, **the filter**, **the waiting card**, **spelling**
  - ruled §R16-§R19; this unit builds them.

Still open: unit 324 item 3 (the carry-forward cap of twenty versus 23 keep-rules -
**the list is 26, leave it**); item 4 (why a 62 dB carrier failed the keying-shape test -
**only a recording answers it**); item 5 and unit 323's item 51 (files that could not be
deleted); unit 323's items 48, 49, 52; everything from unit 322's queue.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet works PSK31 the way it works FT8.
UNIT GOAL:  The digital screen tells the truth about what it holds - a folded
            panel says what is in it, the filter is always there, a card you
            just called waits instead of giving up, every achievement worth
            chasing is marked and the two kinds look different - and step 4
            gets its last number without Tim reading a meter.
ADVANCES:   Step 4 to done if task 6 lands. The screen items are carried
            repair on the shared FT8/PSK31 surface.
DRIFT:      0 carried from unit 324.
```

**What Tim saw, 2026-09-11, with the file to prove it:**

- FT8 and FT4 "no longer worked." They were decoding - 31 rows on the table - into a
  panel he had collapsed two seconds earlier, and nothing on the screen said so.
- The CQ / Everything choice was not on the screen until something decoded, so he could
  not set it before the first slot.
- He answered D2IM at 21:56:15; 23 seconds later the card said *Gone quiet*, dimmed,
  because it was counting from D2IM's last message and not from Tim's call.
- Nine of fourteen rows carried the quill, two of them spinning - the cap of two showing
  as *two held, the rest passive*. He wants them all, and he wants the two kinds told
  apart.
- Every instruction to date was written British and the copy may be too.

---

## 5. Verify this instruction against the tree

**Names from units 305-324's reports.** Check; **report every mismatch; do not repair this
instruction; do not stop over a mismatch** unless a task is impossible.

- `PHASE_PLAN.md` carries §R15-§R19. **If not, stop and say so.**
- The two digital panels - *Decoded text*, *For you* - their collapse state, what
  persists it, `panel_toggled`; the filter chips (`everything`, `CQ`) and what hides them.
- `DigitalCards.Clear()` and the per-slot card rebuild (unit 313's root); unit 314's
  replace-in-place for the PSK31 row; the FT8 conversation card's *Waiting on him* /
  *Gone quiet* states and what clock they read; `RowOpacity` 0.55.
- The nudge: `NudgeSet`, `IsNudged`, `NudgeIsDoor`, `RowLift`, the quill and the orbit
  ring (unit 300), the cap of two and where it is counted.
- `RigField.Alc`, the `15 13` read unit 324 added and its PSK31-send-only gate,
  `MainWindowViewModel.Psk31AlcZone` (empty), the FT8 send path where the same read can
  be taken.
- `VoiceTests`; every operator-facing string.
- Tests: `TheCqListNudgeTests`, `TheNudgeHoverTests`, `ThePressingOfCqTests`,
  `TheCqReceiptTests`, `ThePanelHoldsThemAllTests`, `ThePanelScrollsTests`,
  `TheAlcIsReadTests`, `ThePowerIsOfferedTests`,
  `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint`.

---

## 6. Rulings in force

**`PHASE_PLAN.md` §R15-§R19**, tonight. **§R11-§R14** unchanged. **§R12**: a session
rewrites its own tests; never asks. **§R14**: tests prove criteria and nothing beyond.
**§R13**: every new stage writes its event.

**§0.6** - color is never the only carrier. **§0.5 / HM-DEC-012** - family color is text,
never a fill. **§0.0** - a picture binds as hard as a sentence: a folded panel that
looks empty is a false claim. **§0.2** - nothing here touches what keys. **HM-DEC-111** -
a reading carries its age. **HM-DEC-018, §2.1** - nothing personal in an event.
**`ACHIEVEMENTS_PHILOSOPHY.md`** §3.6 the nudge, §3.1 absent not dimmed - a door is
marked and never named. **HM-DEC-155**, **HM-DEC-139**, **FACT-004**, **FACT-006**, **the
dummy load withdrawn in full.**

---

## 7. Status cadence

`PROJECT_STATUS.md`: **after every task, at least every ten minutes, and immediately
before every `dotnet test` and `dotnet build`.**

---

## 8. The tasks

Six. Each names the test to watch failing first and one drop candidate. **Drop from the
back, but task 6 is the step-4 number - if time is short, drop 4 and 5 before 6.**

### Task 1 - a folded panel says what it holds

**§R17.** Append `UNIT 325` to `PHASE_OUTCOME.md`; patch-bump; run the carry-forward list
as its comment says, status first.

Then: a collapsed *Decoded text* header reads **`31 stations decoded · click to show
them`**, the count live, ticking as slots land. A collapsed *For you* header reads the
same shape - **`1 station calling you · click to show`**. While rows arrive into a
collapsed panel, the header text takes the family color until it is opened. **Neither
panel opens collapsed at startup**, whatever was remembered. In the voice the panels
already use; `VoiceTests` runs.

**Test watched failing first:** `TheFoldedPanelSaysWhatItHoldsTests`, app. Watch it fail,
then green: the count in the header matches the rows; it changes when a slot lands; the
header carries the family color only while collapsed with content; both panels open at
startup regardless of the remembered state; `BindingHealthTests` green.

**Drop candidate:** the family-color-while-arriving. Keep the count and the sentence.

---

### Task 2 - the filter is always there

**§R17.** The `everything` / `CQ` chips render on an empty list and are usable before
the first decode. The choice is remembered across a mode change.

**Test watched failing first:** `TheFilterIsAlwaysThereTests`, app. Watch it fail, then
green: chips present with zero rows; choosing CQ before any decode filters the first
slot's rows; switching PSK31 to FT8 keeps the choice.

**Drop candidate:** the remembered-across-modes assertion.

---

### Task 3 - a card you just called waits on him

**§R18.** After the operator transmits to a station, his card is **Waiting on him**,
undimmed, and the clock that decides *Gone quiet* starts at the operator's transmission.
*Gone quiet* fires only after **at least one full slot** in which he could have answered
and did not - FT8 15 s, FT4 7.5 s, PSK31 a stated equivalent since it has no slots (the
unit states it; the author suggests the length of the macro he would send back).

**The root.** Unit 313 found the cards are rebuilt from scratch every slot
(`DigitalCards.Clear()`), which is why per-card state such as *when did I last call him*
does not survive. **Fix the root here: cards are keyed by station and updated in place;
a card is created when its station first appears and removed when the conversation ends
or is dismissed.** Unit 314's replace-in-place for the PSK31 row is the shape. Report
what else this fixes - the popup closing itself every slot, the card jumping under the
pointer.

**Test watched failing first:** `TheCardWaitsOnHimTests`, app. Watch it fail, then green:

1. call a station whose last message is 90 s old; the card reads *Waiting on him*,
   undimmed, for the full slot after the send
2. one full slot with nothing from him: *Gone quiet*, dimmed
3. his reply inside that slot: the card advances and never showed *Gone quiet*
4. a card object survives three slot rebuilds unchanged in identity; its `MapIsOpen`
   survives with it
5. `ThePanelHoldsThemAllTests` and `ThePanelScrollsTests` still green

**Drop candidate:** assertion 4's `MapIsOpen`. Keep identity.

---

### Task 4 - every quill, two kinds

**§R16.** The cap of two comes off: **every station on the list that would earn anything
is marked.** Two forms:

- **counter** - new country, state, grid; nothing opens: the still quill, decode green
  `#3B6D11`, as now
- **door** - a first contact that opens a set: the quill with unit 300's orbit ring,
  **spinning, orange** - the orange from the palette the app has; name which, and it is
  not the mustard ruled out for the tray

Shape differs as well as color (§0.6). The hover already says *new country* or *would
open something new*; unchanged. The sticky-per-station rule stays; only the cap goes.

**Test watched failing first:** `TheCqListNudgeTests`, rewritten under §R12 for the
withdrawn cap. Watch it fail, then green: fourteen qualifying rows, fourteen marks; a door
row carries the ring and the orange and spins; a counter row carries the still green
quill; a worked station carries neither; no area is named on a door.

**Drop candidate:** the spin. Keep ring plus orange.

---

### Task 5 - American spelling

**§R19.** Every operator-facing string, every `VoiceTests` fixture, every hover, every
panel line: `color`, `neighborhood`, `gray`, `center`, `recognize`, `catalog`, and the
rest. **Not code identifiers, not rulings quoted from Tim, not the plan's own text.**
Report the count changed and the files.

**Test watched failing first:** extend `VoiceTests` by one: no operator-facing string
contains a listed British spelling. State the list in the test.

**Drop candidate:** the whole task. Report the count found if dropped.

---

### Task 6 - the ALC learns from FT8

**§R15.** The `15 13` read unit 324 gated to PSK31 sends now also runs during **FT8 and
FT4 sends**. The highest reading observed during a known-good FT8/FT4 send is stored as
**the reference**, with the time it was taken and its provenance (`learned from FT8 send
at <time>`), shown beside the PSK31 power offer. A PSK31 send whose reading exceeds the
reference by **more than a stated margin** gets §R11's sentence and event. **The margin
is the unit's to state and Tim's to overrule** - the author suggests one eighth of the
scale, 15 on 0-120, and says why in the report. **With no reference yet, the reading is
reported and nothing is judged; Hamlet never invents one.** `MainWindowViewModel.Psk31AlcZone`
is replaced by the learned reference.

**Telemetry (§R13):** the reference learned, with its value, time and the mode it came
from; every judged send with reading, reference, margin and verdict.

**Test watched failing first:** `TheAlcLearnsFromFt8Tests`, app. Watch it fail, then
green: a fed FT8 send at 70 sets the reference to 70 with provenance; a PSK31 send fed at
80 with margin 15 is not judged hot; at 90 it is, and the sentence and event fire; with no
FT8 send observed, a PSK31 send at 120 is reported and not judged; the reference carries
its age; `TheAlcIsReadTests` still green.

**Drop candidate:** none. **This is the step-4 number.**

---

## 9. Parked

- **Step 5** - RST in the log, ADIF, the achievements. The arbiter authors it next.
- **Real off-air audio.** Still the one thing that answers unit 324's item 4.
- **Files that cannot be deleted** in this environment.
- **Any change to what keys. Any package.**

---

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Status before every `dotnet`
  command.**
- **Do not touch anything that keys, arms, composes or plays.**
- **Do not invent an ALC number.** Learn it or report the reading unjudged.
- **Do not name a door's area anywhere.**
- **Do not use color as the only difference between the two quills.**
- **Do not let a panel open collapsed.**
- **Do not put a callsign, grid or text in any event.**
- **Do not edit the phase files** beyond the outcome append.
- **Do not add a package.**
- **Do not chase the known reds** (section 10 of unit 324, unchanged).
- **Do not repair this instruction.** Report mismatches; keep working.
- **Write American.**

---

## 11. Committing and pushing

Commit per task, push at the end. Nothing left uncommitted.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet works PSK31 the way it works FT8. Steps 0 to 3 done,
   4 <state after this unit>, 5 and 6 not started.
B. Step 4 and its must-pass - each met or not met, with the number.
C. The report last, and section 4 raises N items on top of the carried queue.
```

```
UNIT:       325 - <complete|stopped> at task N of 6, <which dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <step 4 to done if task 6 landed; else no>
NUMBER:     strings corrected <n>; cards surviving a slot rebuild 0 -> all;
            ALC reference <learned|none> with margin <n>
DRIFT:      <n> consecutive units without advance  (was 0)
```

**Section 2 must tell Tim, in plain words, what looks different on the screen tonight**
- the folded header with a count, the filter on an empty list, the card that waits, the
green and orange quills - **and that the ALC will start judging itself after his first
FT8 transmission with nothing for him to do.**

**Every appearance claim is computed, not seen. Say so once.**

---

```
ARBITER-DECISION
STEP: 4
APPROACH: make the folded panels say what they hold, keep the filter visible, key cards by station so they survive the slot rebuild and wait on a called station from the operator's send, mark every qualifying station with a green counter quill or an orange spinning door quill, sweep the copy to American, and learn the ALC reference from FT8 sends
MOVE: continue
WHY: six rulings from the owner's evening at the radio, all on the surface PSK31 shares with FT8, and the one number that keeps step 4 partial is now ruled to be learned rather than read
STATE: partial
DECIDED: the PSK31 equivalent of one slot for the waiting card, the ALC margin, and the orange from the existing palette are the unit's numbers to state; the per-slot card rebuild is fixed at the root rather than worked around a third time
LICENCE: PHASE_PLAN.md R15, R16, R17, R18, R19, R11, R12, R13, R14; CLAUDE.md 0.0, 0.6, 0.2; HM-DEC-111
ACCOMPLISHED: the screen stops lying about what it holds, a card Tim just called waits for the answer, every achievement worth chasing is marked and the two kinds look different, and the ALC judges itself from FT8 with nothing asked of him
ADVANCES: step 4 - the R11/R15 power criterion
END-ARBITER-DECISION
```

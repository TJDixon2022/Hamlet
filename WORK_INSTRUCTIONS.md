# Work instruction 390 - the phase closes: every open thing a session can take, taken

**Seed under `--seed`.** The last unit of *Hamlet holds what it has*. It takes every
unticked criterion a session can move, in one pass, and then the phase halts at 5.1 for
Tim. Nothing new is opened. **Six tasks, drop from the back.**

**Status.** `tools/status.sh`, real clock, after every commit and every task. **Write
files as UTF-8.**

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

**HM-DEC-155.** No suite; only this unit's names and `docs\carry-forward-tests.txt` as its
top comment says. Never background and poll.

## 2. The tool fact

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm`
is refused; Python cannot run here; `-m` more than once for a multi-line commit.

## 3. Asks still outstanding

Carried per HM-DEC-139 from unit 389's queue, **verbatim in section 4**. Answered here:
**9.3's conflict with the 2026-09-07 ruling - Tim ruled A on 2026-09-22** (task 3).
**7.5 never built** (task 1). **Favorites look boring** (task 2).

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet holds what it has.
UNIT GOAL:  The phase's open criteria that a session can move - 7.5, 9.3, 4.3,
            7.2, 0.1, 10.3's report, 9.2/9.4/9.5's re-measure - taken in one
            pass, plus the favorites row made worth looking at; then the loop
            halts at 5.1.
ADVANCES:   step 7 criterion 5
DRIFT:      carried.
```

**Tim, 2026-09-22:** *"This phase seems like it's gotten very confused. Can we write
something that's just going to wrap it all up and we'll start fresh?"* This is that. Every
step's open items in one unit, each with its criterion id; the record after this reads as
a phase that finished, not one that trailed off.

**And two things from his screen tonight:** *"Note Olivia and PSK31 still both indicated.
Why wasn't that fixed?"* - 7.5, in the plan since the 21st, never authored. And
*"Favorites looks boring! Sex it up"* - the drop-down 388 built is a grey button with a
caret.

---

## 5. Verify this instruction against the tree

- The mode chip's fill binding and the send-status line's mode word (units 374, 376):
  what they read - the family or `ChosenDigitalMode`.
- The favorites row 388 built under the green zone: its control, its data, the star on
  the rig face, the caret 389 may have removed.
- The Log dialog and the 2026-09-07 read-only ruling (`HM-DEC-` number: find it); the RST
  fields; how a heard value is stored versus one the operator typed.
- `docs/RADIO_SHEET.md` and unit 383's word list for 4.3; 7.2's canned-send events.
- Unit 389's report on 9.2, 9.4, 9.5 - what measured, what did not, and why.
- 0.1's negative from unit 369; 10.3's measure from unit 389.

**Report every mismatch; repair nothing but this unit's.**

## 6. Rulings in force

**`PHASE_PLAN.md` R33-R46 and §6.** **Tim, 2026-09-22, ruling A on 9.3**: RST editable,
heard values marked *heard*, typed values marked *yours*, both kept in the record; this
supersedes the 2026-09-07 read-only line for the RST fields only. **R45** on the form of
ADVANCES. **R12**, **R13**, **R14**, **R19**, **HM-DEC-165**. **§0.0** a value Hamlet did
not hear is never shown as heard.

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the record

Append `UNIT 390` to `PHASE_OUTCOME.md` under step 7. Patch-bump. Run the carry-forward
list.

**Drop candidate:** none.

### Task 1 - 7.5: the chip and the send line say the chosen mode, and the chips say press me

**Tim, 2026-09-22:** *"None of it screams click me. How do you know - take me to FT8, take
me to FT4? It's just kind of, oh by the way, this is on the band."* The mode strip is a
row of words with one tinted. It becomes **a row of buttons**: raised like the CW /
Digital / Voice tabs above them, a hand cursor, a hover state, each one plainly a thing
to press; the chosen one filled; the label *on this frequency* becomes **tune to:**,
because a press retunes the radio and that is the whole promise. The hover on each says
where it goes - *14.070 · PSK31* - from the cited row.

**And 7.5.** The filled chip and the *N s of <mode>* on the send-status line read
`ChosenDigitalMode`, the same field the record reads since unit 374 - never the family.
Under Olivia the Olivia chip is filled and the PSK31 chip is not; a 29-second Olivia CQ
reads *29 s of Olivia*.

**Test watched failing first:** `TheChipSaysTheChosenModeTests`, app: under each of FT8,
FT4, PSK31 and Olivia exactly one chip is filled and it is the chosen one; each chip is a
button with the hand cursor and a hover naming its frequency from the cited row; the label
reads *tune to:*; the send line names the chosen mode after a fed send; `BindingHealthTests`,
`VoiceTests`.

**Drop candidate:** the hover's frequency. Keep the buttons and 7.5. **Ticks 7.5.**

### Task 2 - the favorites row is chips

The row under the green zone becomes **a row of chips**, one per saved spot, in the style
of the band pills above: the spot's frequency and mode in the mode's family color as
text, the name Tim gave it beside, one click tunes, a small ✕ on hover forgets it, and the
star on the rig face fills when the dial is on a saved spot. Empty, the row reads *no
spots saved yet - press ☆ to keep this one*. The drop-down goes.

**Test watched failing first:** `TheFavoritesAreChipsTests`, app: three saved spots render
as three chips with the right words and colors; a click tunes; ✕ forgets and the list
persists; the empty sentence; the star fills on a saved spot; `TheTopRowTests` green and
the band no taller.

**Drop candidate:** the ✕ on hover. Keep the chips. **Refines 10.6; reports it.**

### Task 3 - 9.3: RST editable, heard and yours told apart

On the Log dialog the RST fields are editable. A value Hamlet heard is filled in and marked
*heard*; a value Tim types replaces it and is marked *yours*; the log entry and the ADIF
carry the value used, and the record carries which it was. Nothing else in the dialog
changes from the 2026-09-07 ruling.

**Test watched failing first:** `TheRstIsYoursToCorrectTests`, app: a heard 599 is shown
and marked; typing 579 replaces it, marks it, logs it; a contact with no heard report
starts blank and marked *yours* when typed; `ThePsk31LogsWithRstTests` green.

**Drop candidate:** none. **Ticks 9.3.**

### Task 4 - the re-measures: 9.2, 9.4, 9.5, 7.2, 4.3, 0.1

Each as unit 389 left it, taken to a tick or an honest untick with the number:

- **9.2**: the hold as built - releases on hand-back, greys the typed Send, offers no
  Report mid-over. State it, assert it, tick it as *met as built*; if the plan's wording
  must change to match, say which words.
- **9.4**: every parsed hand-back moves the turn; assert on the fixture; tick. The garbled
  hand-back is the demodulator's, named in section 4, not this criterion's.
- **9.5**: the four on Olivia as on PSK31; assert; tick.
- **7.2**: a canned send announced, recorded, capped; assert; tick.
- **4.3**: no sentence on the radio sheet sends the operator to the radio; assert against
  unit 383's list; tick.
- **0.1**: the negative stands as unit 369 measured it twice; tick with *met as a negative*.

**Test watched failing first:** the existing tests for each, run by name.

**Drop candidate:** any that does not measure out - untick, report the number, move on.

### Task 5 - 10.3's report

The map as 389 left it - 393 × 214 at the band's left edge; 327 × 178 when the strayed
line shows or under 1400 wide. State it, assert it, tick it.

**Drop candidate:** the whole task.

---

## 9. Parked

- **5.1.** Tim's. The loop halts there.
- **The demodulator on real air.** Waiting on a capture. Not this phase's.
- **Anything new.** The next phase is written fresh.

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Do not open anything new.** Close what is open.
- **Do not show a typed RST as heard.**
- **Do not touch the demodulator or what keys. No package. Report mismatches; repair
  nothing but this unit's. Write American. Write files as UTF-8.**

## 11. Committing and pushing

Commit per task; push at the end.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. After this unit every criterion a
   session can move is met or honestly unmet; 5.1 waits on Tim.
B. The criteria, one line each, met or not, with the number.
C. The report last, and section 4 carries the queue.
```

```
UNIT:       390 - <complete|stopped> at task N of 6, <which dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   yes
NUMBER:     unticked criteria a session can move <n> -> <m>
DRIFT:      carried
```

**Section 2 tells Tim in one paragraph what he sees now: a row of mode buttons with one lit,
favorites as chips, RST his to correct. Every appearance claim is computed, not seen.**

---

```
ARBITER-DECISION
STEP: 7
APPROACH: make the mode strip a row of pressable buttons that read the chosen mode, and the send line likewise, turn the favorites drop-down into chips, make the RST fields editable with heard and yours told apart, and take every other open criterion to a tick or an honest untick with its number
MOVE: continue
WHY: PHASE_PLAN.md criterion 7.5 has been open since 2026-09-21 and Tim saw it tonight; 9.3 is ruled A; the owner asked for the phase wrapped up so the next can start fresh
STATE: partial
DECIDED: the favorites chips' look is the author's, overrulable; the words heard and yours are the author's
LICENCE: PHASE_PLAN.md R39 (7.5), R44 (9.3), R46 (10.6), section 6; Tim 2026-09-22 ruling A; R12, R13, R14, R19; HM-DEC-165
ACCOMPLISHED: the phase is closed as far as a session can close it, with one chip lit, favorites worth looking at, and a log Tim can correct
ADVANCES: step 7 criterion 5
END-ARBITER-DECISION
```

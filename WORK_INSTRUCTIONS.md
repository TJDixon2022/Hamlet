# Work instruction 357 - say what you want to say

**Single session.** Tim's ruling of 2026-09-14 changes R2 of the PSK31 plan: **typed
text goes on the air.** This is the first unit since step 4 that touches what is sent;
it touches nothing about how it is keyed. **Five tasks.**

**Status.** `tools/status.sh`, real clock, after every commit and every task.

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

Carried per HM-DEC-139 from unit 356's queue, **verbatim in section 4**.

---

## 4. Why this unit exists

```
PHASE GOAL: The screen, done right.  (PSK31 carried work under it.)
UNIT GOAL:  Right-click any station's row and get his card; type a line on it
            and one click sends it, framed with the callsigns and the hand-back
            so a beginner never forgets them; read a whole message on hover.
ADVANCES:   none - clears a blocker: R2's no-keyboard rule, overruled by Tim
            2026-09-14, was the thing standing between PSK31 and a conversation.
DRIFT:      carried.
```

**Tim, 2026-09-14, on seeing a real QSO read off 14.070:** *"I want to add the ability
to send text I type. That is the thing PSK31 offers over FT8/4."* And the shape: *"I
right click on a message in the Everything list and it creates a card. On that card I
can do a standard answer but there is also a text block where I can enter text to
transmit."* And: *"I should be able to hover and see a whole message, not just cut off."*

**What this overrules.** PSK31 plan **R2** - *free typing is not in this phase; the
macros are the whole vocabulary*. Tim's later ruling wins (§6). **What it keeps:** **§0.2**
one click, one transmission; **R10** the one sequence, one `PttOn` site, the abort;
**R1** Report and Confirm are offered only on certainty. Typed text is Tim's call, and
Hamlet frames it so the two things a beginner gets wrong on a keyboard mode - the
callsigns and the hand-back - are never his to remember.

**The record behind it.** `2026-09-14.jsonl`, 14:00-14:02 UTC, 14.070: `KE0JBT` and
`N7WE` in a ragchew, read off the air with *guess* and *ended* marked - the screenshot
Tim sent. Three of Tim's own CQs went out that morning, keyed and confirmed; nobody
answered, and the only thing he could have sent was another CQ.

---

## 5. Verify this instruction against the tree

- The decoded row's right-click: what it offers today on an FT8 row, on a PSK31 CQ row,
  on a PSK31 non-CQ row; `ThePsk31ExchangeTests`; how a conversation card is created for
  a station and what `Answer` sends.
- The PSK31 conversation card: the turn indicator, the certainty gate, the Report and
  Confirm buttons, the receipt (R2-R5 of the card rulings).
- The macros of §R2 and their composer; `UnslottedTransmission`; the 30-second cap and
  where it is checked; `psk31_send_composed`, `psk31_send_refused` (reason `cap`).
- The row's text binding and its column width; the map popup (unit 310) as the model
  for a text popup.
- Tests: `TheUnslottedSendTests`, `TheFt8AndFt4SendsAreByteIdenticalTests`,
  `ThePsk31CqGoesOutTests`, `ThePsk31ExchangeTests`, `ThePsk31RowStaysTests`,
  `TheStopIsAlwaysOnScreenTests`, `TheRowShowsWhatWasHeardTests`.

**Report every mismatch; repair nothing but this unit's.**

## 6. Rulings in force

**Tim, 2026-09-14** as quoted, overruling PSK31 plan **R2**'s no-keyboard line and
nothing else of it. **§0.2**, **R10** - one click, one transmission, one keying path,
the abort. **R1** strict side unchanged for Report and Confirm. **R11** nothing at the
radio. **R13** telemetry. **R12**, **R14**, **R19**. **HM-DEC-018, §2.1** - typed text
never enters an event; its length does. **HM-DEC-155**, **HM-DEC-139**, **FACT-004**,
**FACT-006**, **the dummy load withdrawn.**

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the record

Append `UNIT 357` to `PHASE_OUTCOME.md` under step 3, `ADVANCED: blocker`. Patch-bump.
Add to `PHASE_PLAN.md`'s §R block, as Tim's ruling of 2026-09-14: **typed text goes on
the air from a station's card, framed by Hamlet, one click one transmission; R2's
no-keyboard line is withdrawn.** Run the carry-forward list.

**Drop candidate:** none.

### Task 1 - right-click any row makes his card

On the PSK31 list, **right-click on any row** - CQ or not, live or ended - makes a
conversation card for that station, the same card a certain answer makes, at *his
turn* or *unknown* as the parser has it. If a card for him exists, the right-click
focuses it. The row's text stays where it is. FT8 rows keep their existing menu.

**Test watched failing first:** extend `ThePsk31ExchangeTests`: right-click on a non-CQ
PSK31 row makes his card; on a CQ row the same card with Answer offered; a second
right-click focuses, never duplicates; `ThePanelHoldsThemAllTests` green.

**Drop candidate:** none.

### Task 2 - the text block, and one click sends it framed

On his card, under the turn indicator: **a text block and a Send button.** Send composes
one transmission:

```
<HIS> de KC3QIS  <what Tim typed>  BTU <HIS> de KC3QIS K
```

through the same composer and the same unslotted sequence the macros use - **no new
path, no new `PttOn` site** - and the block clears when it has gone out. The block is
offered whenever it is **not certainly his turn**; when the parser is not sure, the card
says so in a word beside the button - *not sure it is your turn* - and **sends anyway on
the click**, because typed text is Tim's call. Report and Confirm keep their certainty
gate as they are. `Stop` aborts a typed send like any other.

**Framing rules:** Tim's text is sent as typed, trimmed; Hamlet adds only the two frames;
a line that is only whitespace does not send; characters outside the varicode table are
dropped and the card says how many.

**Telemetry (§R13):** `psk31_send_composed` gains `macro: "typed"` with the character
count and seconds; **the text itself never enters the record.**

**Test watched failing first:** `TheTypedLineGoesOutTests`, app: a typed line composes the
framed text exactly; one click, one keying, one `Played`; the block clears; a whitespace
line sends nothing; a line with a character outside the table drops it and says so; the
send goes through `UnslottedTransmission` and `TheUnslottedSendTests`,
`TheFt8AndFt4SendsAreByteIdenticalTests` and `TheStopIsAlwaysOnScreenTests` are green
and unedited; the event carries a count and no text.

**Drop candidate:** the dropped-character sentence. Keep the drop.

### Task 3 - the cap fits a conversation

The 30-second cap was the author's number for the macros. A typed line plus its frame is
allowed **up to sixty seconds** at 31.25 baud - about 200 characters of text; the card
shows the seconds as you type; **past sixty it refuses with the count and the seconds,
and nothing is sent.** The macros keep their existing lengths.

**Test watched failing first:** extend `TheTypedLineGoesOutTests`: a 190-character line
sends; a 260-character line is refused with `psk31_send_refused reason cap` and the
card's words; the seconds shown match the composed length.

**Drop candidate:** the live seconds count while typing. Keep the refusal.

### Task 4 - the whole message, on hover and on click

Hovering a PSK31 row shows its full text, wrapped, in the hover. Clicking the text opens
it in a box like the map popup, with the station, the time and every line he sent while
the row lived, and a dismiss X. Ended rows too.

**Test watched failing first:** `TheWholeMessageTests`, app: a row whose text exceeds the
column carries the full text in its hover; clicking opens the box with the full text and
the station; the X closes it; `BindingHealthTests`, `VoiceTests`.

**Drop candidate:** the click box. Keep the hover.

---

## 9. Parked

- **Live keyboard-to-keyboard** (keys go out as pressed). Not ruled; not built.
- **The demodulator on real air.** After a capture exists.
- **Any second transmit path. Any package.**

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Do not add a second keying path.** The typed line goes through the one sequence.
- **Do not send anything Tim did not click.**
- **Do not put typed text in any event.**
- **Do not gate the typed line on certainty.** Say the doubt in a word; send on the click.
- **Do not touch the demodulator. No package. Report mismatches; repair nothing but this
  unit's. Write American.**

## 11. Committing and pushing

Commit per task; push at the end.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

```
READ IN THIS ORDER.

A. The phase goal - the screen, done right. Steps 0 to 2 done, 3 waits on Tim.
B. No criterion changes state; this unit clears the blocker under R2, overruled.
C. The report last, and section 4 raises N items on top of the carried queue.
```

```
UNIT:       357 - <complete|stopped> at task N of 5, <which dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   blocker
NUMBER:     things Tim can send on PSK31 4 -> 5 (a typed line); cap 30 s -> 60 s for typed
DRIFT:      carried
```

**Section 2 tells Tim, in plain words: right-click a station, type, click Send, what goes
out around his words, and that Stop still stops it. Section 3 prints one framed line
exactly as it would be sent. Every appearance claim is computed, not seen.**

---

```
ARBITER-DECISION
STEP: 3
APPROACH: make a right-click on any PSK31 row open the station's card, add a text block that sends one framed line through the existing unslotted sequence on one click, lift the cap to sixty seconds for typed text, and show a row's whole message on hover and click
MOVE: continue
WHY: Tim overruled R2's no-keyboard line after reading a real QSO off the air - typing is what PSK31 is for - and the framing keeps the beginner's two mistakes out of his hands while one click one transmission and the single keying path stay exactly as proved
STATE: blocked
DECIDED: the frame's exact words are the author's; the sixty-second cap is the author's number; the doubt word is the author's
LICENCE: Tim 2026-09-14 overruling PSK31 plan R2; CLAUDE.md 0.2; PSK31 plan R1, R10, R11, R13; HM-DEC-018
ACCOMPLISHED: Tim can hold a PSK31 conversation - answer, report, say something in his own words, confirm - without ever typing a callsign or a hand-back
ADVANCES: none - clears a blocker: R2, overruled
END-ARBITER-DECISION
```

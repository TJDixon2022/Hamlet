UNIT:       280 — complete at task 9 of 9 — 2026-09-08 11:46
PHASE GOAL: Hamlet works stations on the air. Not decodes them, not shows them —
            the operator makes a contact with his own radio.
UNIT GOAL:  The conversation reads forwards with his side on the right, the turn is
            a ring he reads at a glance, and the screen stops lecturing him while
            he operates.
ADVANCED:   no. Every bench step of this phase is closed and steps D and E are Tim
            at his own radio, so no unit can meet their criteria. The author said
            so at the head of this order and this report does not dress it up.
NUMBER:     1,367 -> 529 characters of permanently-visible text on the Digital tab,
            across 43 text blocks before and 42 after. **Measured off the realized
            window, not counted in source** — the whole Digital workspace holds ten
            literal strings totalling 71 characters, so a source count would have
            found nothing to count and then flattered the result.
DRIFT:      3 consecutive units without advance (was 2, carried from unit 279).

## 1. What Claude did

**Complete. Nine tasks of nine, none dropped.** Windows, `PROJECT: Hamlet` claimed
and confirmed against all four gate checks, branch `main`, pushed at every task.
Version `1.12.163` -> `1.12.173`.

**Two departures from the order, both reported rather than quiet.**

- **Task 6 was written after tasks 7 and 8.** It is the list of every string this
  unit removed, and writing it before the last two removals would have produced a
  list that was wrong by construction.
- **The baseline was measured before task 1**, in its own commit, because the
  unit's `NUMBER` cannot be recovered once the strings are gone.

**Task 1 — the conversation reads forwards** (`25a8e75`). Oldest at the top, his own
messages aligned right, received left, bordered and never filled with a family
colour. The sort toggle no longer reaches it and its tooltip says which list it
governs; **the left list still follows it, asserted, so unhooking one could not
quietly unhook both.** The For you column header came out: that side stopped being a
table, so `utc | message` labelled columns that no longer exist.

**Task 2 — two things left the conversation body** (`5ae1690`). `your move, 0 slots`
is gone; the ring above says it and would disagree with it the first time one of
them was wrong. `x2` moved out from inside the message text to a phrase beneath it.
**Where the fold stops is unchanged and ask 8 stays open.**

**Task 3 — the turn is a ring** (`3dde179`). Four draining arcs with the count
inside. See section 3.

**Task 4 — the screen stops lecturing** (`a2ec95c`). 1,249 -> 843.

**Task 5 — the count is not a whisper** (`9390e6b`). A figure at 20 point with the
word small beside it and the badge progress after, replacing a sentence at the tail
of a status bar. The dimmed row now reads *You logged a contact with CO8LY on
09/07/26*.

**Task 7 — a slot he transmitted in says so** (`db70f9b`). See section 3.

**Task 8 — the Send block is facts** (`9cbdd03`). 843 -> 529.

**Task 6 — the removal list** (`docs/unit280-what-was-removed.md`, `cd57f28`).

**Task 9 — the outcome entry** (`610f22b`), written by the tool, exit 0.

**One mismatch with the instruction.** It places the AGC paragraph *in the status
bar across the full width*. It is not there: `ReceiveAdvice` surfaces in a
collapsible Receive-help widget on the canvas, and the status bar carries
`StatusText`, the badge and the count. Reported, not repaired.

**One shell refusal**, verbatim: `/usr/bin/bash: -c: line 125: unexpected EOF while
looking for matching `'``, on a heredoc containing an apostrophe — the documented
fault, seventh unit. Worked around with a script file.

## 2. What the owner should expect

**The conversation reads the right way round and the screen stops talking at him.**

His own messages sit on the right in their own bubbles, what he heard sits on the
left, oldest at the top. Above them a ring drains with the seconds left in it and
two or three words beside it instead of two sentences. The contact count is a large
figure in the status bar with how far the next badge is, instead of a whisper after
somebody else's paragraph.

**What will look wrong and is not:**

- **The For you side has no column header.** It is a conversation, not a table.
- **The drive control says `-12.0 dBFS` and nothing else.** The ALC advice is on the
  `?` beside it.
- **A fine clock is a small glyph.** A clock that is *not* fine still gets words,
  because that is a fault.
- **The census reads `15:16:45 UTC · 0 candidates`** instead of a sentence. Every
  number it used to carry is still there.
- **The band is gone from the worked-station hover.** That is a fact removed rather
  than a sentence shortened, it was your ruling, and it is on the list under its own
  heading.
- **Inherited reds, untouched and unrun**: `CwAdjudicationTests.ASpeedChangeInRealisticAudio`,
  the 51 CW cases in `docs/unit239-failing-set.txt`, the `Ft8Sharp.Deep.Tests`
  whole-type-list tripwire.

## 3. What you should see

**1. The K9XP exchange as the panel now draws it**, printed by the reconstruction:

```
For you (K9XP):
    021100 sent      K9XP KC3QIS R-09
    021115 received  KC3QIS K9XP -09  (heard twice)
    021215 sent      K9XP KC3QIS R-09
    021245 received  KC3QIS K9XP -09
    021315 sent      K9XP KC3QIS RRR
```

**Oldest at the top.** The `sent` rows draw right-aligned in bordered bubbles and the
received ones left, so the alternation is read without reading. The fold is under the
message rather than inside it, and **there is no `your move, 0 slots` line anywhere
in the body.**

**2. The four ring states, as they render.**

```
his slot     solid 3px amber    "click a reply"      288°  count "12"
their slot   solid 3px muted    "yours is next"      288°  count "12"
on air       solid 6px green    the message going out 285° count "10"
unknown      dashed 3px         "nothing heard yet"        count "?"
```

**In grayscale they are still four.** A test strips hue out and compares dash
pattern, stroke thickness and caption: no two states match. Unknown is the dashed
one, on air is the thick one, and his slot and theirs differ by caption as well as
ink. **Two lengths share one shape** — a slot is 15 s and a transmission 12.64 s — so
the arc is drawn as a fraction of whichever this is: 8 of 15 and 7 of 12.64 both draw
about 195°, and the thickness and the caption say which it is rather than the angle.

**3. What was removed** — `docs/unit280-what-was-removed.md`, in full. The shape of
it:

- **five strings moved to hover**, the largest being the 325-character drive
  paragraph;
- **three replaced by shapes**, including `x2`, which was a correction as much as a
  shortening: it sat inside the message string where it read as part of what the
  station transmitted;
- **four removed outright**, each with why it was not a fact;
- **one fact removed and named under its own heading**: the band came out of the
  worked-station hover;
- **six things kept with the reason**, including the 124-character empty-band line.

**4. A slot he transmitted in, and a quiet one.**

```
transmitted slot : 15:16:45 UTC was yours - Hamlet was transmitting and did not listen
quiet slot       : 15:16:45 UTC · 0 candidates
```

The first was reading *…the search found no place in it that looked like the start of
an FT8 transmission, so nothing reached the decoder at all, read by Ft8Sharp.Deep with
fine sync and ordered statistics* — **for a slot Hamlet had deliberately not listened
to.** Nothing needed measuring: the transmission was already in `ft8_transmission`
telemetry with its `slotStartUtc`.

**Tests.** 66 passed and 1 skipped across the twelve classes this unit wrote or
rewrote. Every one was run because this unit changed code it guards. **Three watched
failing first**: the conversation came back reversed, the ring's caption came back a
sentence, and the transmitted slot came back claiming a search.

## 4. What's blocking us

Nothing blocks the next unit. Two things want a ruling.

---

**The bubble width and the fade were not looked at together, and they interact.**

A worked station's row is drawn at 0.55 opacity (unit 279) and is now also a bordered
bubble. **A faded border on a white bubble is a fainter thing than a faded row of
text was**, and I did not measure whether it is still obvious at a glance across a
column of fourteen rows a slot.

*Rejected: deepening the fade on my own authority.* §0.6 and the grayscale question
are yours, ask 9 is already open on exactly this, and changing the number without
looking at a screen would be guessing.

---

**`ReceiveAdvice` is 18 KB of prose that this unit did not sweep.**

The instruction pointed at the status bar and the paragraph is not there — it feeds a
collapsible Receive-help widget on the canvas. That widget is advice by definition,
and the same ruling plainly applies to it, but it is a different surface from the one
every task named and restructuring an engine file feeding it is not what this unit
was asked for.

*Rejected: sweeping it anyway.* The order names the Digital tab in every task and
lists the send path and the composer as parked; taking on a third panel on my own
reading of the ruling is the scope creep §12.6 exists to stop.

---

## Asks still outstanding

Carried per HM-DEC-139. Ten inbound, all carried, two added.

1. **Two issues of one work-instruction number.** Unit 271, and 252 before it.
   **The author's error.** No unit action.
2. **`PM95` reads *southern Japan***, unit 271. **Not a defect.**
3. **`HM-OPEN-083` and `HM-OPEN-084`.** In `OPEN_ISSUES.md` by HM-DEC-140.
4. **Three pixels.** Unit 279 measured 177 px against 180 and did not settle it.
   **Waiting on Tim.**
5. **`dt` and `hz` were never on the mine list.** **Waiting on Tim.**
6. **Where an outcome entry goes when the unit it corrects has none.** Unit 276, in
   the tree. **The ruling wants Tim's eye.**
7. **`PHASE_OUTCOME.md` is written by a tool no unit is told to run.** Task 9 wrote
   one because this order named the tool; **the general question is the author's.**
8. **Where the repeat fold stops**, unit 277, in the tree at `0c359cc`. **Task 2
   moved where the fold is shown and did not change where it stops.** Still open.
9. **Whether a faded row needs a second carrier of its meaning**, unit 279. **The
   hover now carries it in words**; whether it needs a glyph as well is still Tim's,
   and item 11 below is the same question from a new angle.
10. **Whether counting subjects is a new assertion**, unit 279, in the tree at
    `4d91bfe`, both tests passing. **The ruling wants Tim's eye.**
11. **Whether the fade is still obvious now the row is a bubble**, raised by this
    unit, 2026-09-08. Not measured; it interacts with item 9.
12. **Whether `ReceiveAdvice` is in scope for the same ruling**, raised by this unit,
    2026-09-08. Not swept, and the reason is in section 4.

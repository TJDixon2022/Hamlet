UNIT:       277 — complete at task 6 of 6 — 2026-09-07 23:41
PHASE GOAL: Hamlet works stations on the air. Not decodes them, not shows them —
            the operator makes a contact with his own radio.
UNIT GOAL:  The For you panel is one conversation with both sides in it, and a
            clock saying whose turn it is and how long is left.
ADVANCED:   no — it advances step E's bench half and closes no criterion, which
            the instruction said itself and this report does not upgrade. Step E's
            criteria are Tim at his own radio working a station, and no unit can
            meet them.
NUMBER:     0 -> 3 rows of his own. The K9XP exchange has one and the instruction
            named it: he answered 60 seconds after a message sent 15 seconds
            earlier, so one slot is the beat and he was three or four late. What
            the panel told him then was nothing at all — three identical received
            rows, his own three transmissions absent. It now draws all six moments
            in time order and says `Their slot` in the two slots his replies
            actually went out in.
DRIFT:      unknown — the instruction carried no drift count for this session to
            increment, the second unit running. It cannot be recovered from inside
            a session, because `output.md` is overwritten, so the next author has
            to reseed it.

## 1. What Claude did

**Complete. Six tasks of six, none dropped** — including task 5, the named drop
candidate, which earned its place. Windows, `PROJECT: Hamlet` claimed and confirmed
four ways against the tree, branch `main`, pushed at every task. Version
`1.12.143` -> `1.12.149`.

**Task 1 — reading only** (`docs/unit277-what-the-view-can-reach.md`, `f3a2ef7`).
**Three of the four things this unit needed already existed and none was on
screen**, which shrank the unit from building to surfacing.

- **The sent message's text survives in full.** `AtSlotBoundaryAsync` calls
  `_contacts.RecordSent(text, slotStartUtc)` where the transmission actually went
  out, and `Ft8LedgerMessage` keeps the text, its parsed fields and its slot.
  **Telemetry keeps only the length and cannot hold text by construction** —
  `TransmitRecord` has no string parameter and a reflection test asserts it.
  HM-DEC-018 is untouched by anything in this unit.
- **The parity is arithmetic.** A slot boundary's second is always 0, 15, 30 or 45,
  so `(second / 15) % 2` is the half, and a minute holds four slots, an even number,
  so it does not shift at a minute or an hour.
- **A 250 ms tick was already running** and calls `OnSlotTick` sixty times a slot,
  so the countdown needed no new timer and no second timing source.
- **One hazard found and not copied**: the send path's `?? DateTime.UtcNow` is
  defensible where a boundary must be picked for something already clicked, and a
  turn line has no such obligation.

**Task 2 — his own transmissions in the conversation** (`123330c`). **This reverses
unit 273's instruction**, which said a sent message is not a decode and does not
belong in a decoded list. It was right about what a sent message is and wrong about
what the panel is for. **Watched failing first, and the red is the evening itself**:
with the placement reverted the panel printed three identical received rows and
nothing else. A sent row's `snr` and `dt` are empty, not zero and not the dash — the
dash means a reading was attempted and failed, and a `0.0` in `dt` would read as a
transmission perfectly on the boundary, which is the exact thing this unit exists to
let him see he was not doing.

**Task 3 — the beat** (`0c359cc`). `Ft8Turn` in the engine, four states rather than
three and a fallback: no clock measured, no station heard, theirs, mine. **It counts
down and does nothing else** — nothing reads it, nothing arms on it, and there is no
line anywhere watching it reach zero. **Watched failing by mutation**, since the type
is new and had no prior red: inverting the parity comparison turns 3 of the 8 engine
tests red, so they discriminate rather than agreeing with whatever was written.

**Task 4 — one conversation, the others above it** (`9462de3`). Every other station
calling him is a row above the panel with its message count and how long it has been
quiet, one click from being the conversation. **Nothing is hidden**, and the summary
counts everybody rather than the rows on show: counting what is drawn would read
`1 for you` on an evening two stations were calling, and a collapsed panel saying
that is a false picture (§0.0, HM-DEC-092).

**Task 5 — the evening reconstructed** (`docs/unit277-the-k9xp-evening.md`,
`1db2ec3`). Every panel in it is printed by a test that places each message through
the application's own doors. **It found a defect that would otherwise have shipped**
— see section 2.

**Task 6 — the outcome entry** (`4177f92`), written by
`tools/arbiter/outcome-append.bat`, which exited 0. **That is itself news**:
`PHASE_OUTCOME.md:194` records the tool being refused by the shell for **fourteen
consecutive units**, with hand-editing having become the norm. Invoked through
`cmd //c` from a batch file it ran, appended the entry and updated the header in
place. **Step E stays `not started`**, because a busy unit must not look like a
closed step.

**Decisions this session made for itself:** where the repeat fold stops, which is
raised in section 4 rather than settled, because it is what the display asserts.
Nothing else touching §0.0, §0.2 or transmit.

## 2. What the owner should expect

**He can see both halves of a contact, and whose fifteen seconds it is.** The For
you panel now carries his own transmissions interleaved with what came in, marked
`sent` under the time and drawn in amber; above it a sentence says whose slot is
running and how many seconds are left; above that, any other station calling him.

**What will look wrong and is not:**

- **A repeat shows as one row with a count, like `-09 x2`, but only where nothing
  came between.** A repeat that arrives *after* he transmitted stays its own row.
  That is deliberate and is the whole diagnosis: folding it back above his own
  message would say only that the station repeated itself early on, and destroy the
  fact that it did not hear his answer. **Raised for a ruling in section 4.**
- **The For you panel shows one station at a time.** Everyone else is on the strip
  above it. The `n for you` summary counts them all, so it will read higher than the
  rows on screen, and that is correct.
- **`PHASE_OUTCOME.md` has nothing between `UNIT 272` and `UNIT 276`.** Units 273,
  274 and 275 wrote no entry. They are deliberately not back-filled.
- **Inherited reds, untouched and unrun**: `CwAdjudicationTests.ASpeedChangeInRealisticAudio`,
  the 51 CW cases in `docs/unit239-failing-set.txt`, the `Ft8Sharp.Deep.Tests`
  whole-type-list tripwire, and `HM-OPEN-086`.
- **`src/Ft8Sharp/` is untouched** and its version did not move.

**Three faults surfaced on the way, all mine, all fixed with the test that caught
them.** A slot was being treated as a condition of belonging to a conversation
rather than as its ordering, which emptied the panel entirely for rows decoded
before slots were carried. The order button un-folded the panel, because a reorder
streams every row back through the arrival path in display order and every repeat
met its successor instead of its predecessor. And **his own opening call was
invisible until somebody answered it**, which task 5's reconstruction caught and
which would have looked exactly like the bug this unit was written to fix.

## 3. What you should see

**1. The K9XP exchange as the panel would now draw it.** Printed by
`TheEveningOf20260908AsItWouldNowRead`, one panel per moment; the last of them:

```
=== 02:13:15 UTC, he transmits ===
  turn: Their slot, with 14 seconds left. Yours is next, so a message you click
        now goes out at the top of it.
  For you (K9XP):
      021100 sent      K9XP KC3QIS R-09
      021115 received  KC3QIS K9XP -09 x2
      021215 sent      K9XP KC3QIS R-09
      021245 received  KC3QIS K9XP -09
      021315 sent      K9XP KC3QIS RRR
```

He calls, they answer, **they answer again with the identical report**, he answers,
**they send the identical report a third time**. What he actually had was the middle
column of three rows all reading `KC3QIS K9XP -09`, and nothing else on this page.

**The diagnosis, which a test now asserts rather than a story telling it.** K9XP
transmits on the odd half, so the operator's slots are `:00` and `:30`.

| He transmitted at | Whose slot |
|---|---|
| `02:11:00` | **his own.** Correct. |
| `02:12:15` | **K9XP's.** |
| `02:13:15` | **K9XP's.** |

His opening call was on the beat and both replies after it were not. **My first
version of that assertion claimed all three were off the beat, and the test caught
it**; the sharper claim is the better diagnosis.

**2. The turn line, quoted, for a known parity and for a station that has not
transmitted.**

Known, and his own slot running:

> Your slot, with 8 seconds left of it. Anything you click now waits for the slot
> after this one, which is theirs, and arriving a slot late is how a station gives
> up and starts again.

Known, and theirs running:

> Their slot, with 8 seconds left. Yours is next, so a message you click now goes
> out at the top of it.

Nothing heard yet:

> Nobody has transmitted to you yet, so there is no pattern to read and Hamlet will
> not guess whose turn it is. 8 seconds left in this slot.

No clock measured:

> Hamlet has not measured the clock yet, so it cannot say where the slot boundaries
> fall or whose turn this is.

**Four sentences, none of which contains the word `unknown`**, and the countdown
still runs where the slot can be placed but the station cannot.

**3. Two stations calling at once.**

```
conversation: W1ABC
    021200 received  KC3QIS W1ABC -14 x2
waiting: 1 other station is calling you
    K9XP  2 messages, last 3 slots ago
```

Clicking `K9XP` switches to its conversation and W1ABC becomes the waiting row.
Neither loses anything, the summary reads `4 for you` throughout, and a message he
sent to a third station appears in neither, because a sent row belongs to whoever it
was addressed to.

**Tests.** 17 in `BothHalvesOfTheConversationTests`, 8 in
`TheBeatIsDerivedNotGuessedTests`, all green; plus the 61 in the eight classes whose
paths this unit rewrote, `VoiceTests` and `BindingHealthTests` among them, run
because I changed the code they guard.

## 4. What's blocking us

Nothing blocks the next unit. Three things want a ruling.

---

**A repeat folds only where nothing came between, and this asks you to confirm it.**

The instruction says three identical messages read as one row with a count. **Taken
literally on the real exchange that is wrong and destroys the unit's own purpose**:
the third `-09` arrived at `02:12:45`, *after* his transmission at `02:12:15`, and
folding it back into the row above that transmission would say only that K9XP
repeated itself early on. What it actually says is that K9XP did not hear his
answer, which is the most useful fact in the whole evening.

So it is built as `x2`, his answer, then a fresh `x1`. **It asserts strictly less
than the literal reading and hides nothing**, which is why it was the safe one to
ship, but it is what the display asserts and §12.1 reserves that to you without
exception.

*Rejected: folding across the whole exchange.* It makes the panel hide the thing the
panel exists to show.

*Rejected: not folding at all.* Three identical rows is what he had, and it reads as
three pieces of news rather than one fact repeated.

---

**The turn line speaks about the next click, not about a transmission already going
out.**

At `02:12:15`, the moment his reply was going out in K9XP's slot, the line read
*Their slot, with 14 seconds left. Yours is next, so a message you click now goes
out at the top of it.* That is correct, useful, and about the future. **The sentence
that would have ended the confusion on the spot is that the message he just sent is
going out in their slot**, and the panel does not say it.

*Rejected: saying it on my own authority.* It is a change to what the display
asserts. It is also close to a line that could be read as instruction rather than
observation, which wants your eye rather than mine.

---

**`PHASE_OUTCOME.md` is written by a tool no work instruction tells a unit to run,
and it has now failed twice over.**

Units 273, 274 and 275 wrote no entry, so the phase record has nothing between
`UNIT 272` and `UNIT 276`. This unit wrote one only because task 6 named the tool.
**And the tool had been refused by the shell for fourteen consecutive units** before
this session, per `PHASE_OUTCOME.md:194`; invoked through `cmd //c` from a batch file
it ran and exited 0, so the refusal was in how it was called rather than in the tool.

*Rejected: relying on habit.* Habit is what failed; three consecutive units had the
same habit and produced nothing.

*Rejected: back-filling the three.* Nobody running now ran them, and a reconstructed
entry is a fabricated record.

---

## Asks still outstanding

Carried per HM-DEC-139. Eight inbound; two are dropped as answered by this unit,
six carried verbatim, three added.

1. **Two issues of one work-instruction number.** Raised by unit 271, and unit 252
   before it. **The author's error: an executed order must never be amended, only
   succeeded.** No unit action.
2. **`PM95` reads *southern Japan***, raised by unit 271. **Not a defect**; the
   table is where to argue with it.
3. **`HM-OPEN-083` and `HM-OPEN-084`**, raised 2026-09-05. By HM-DEC-140 they live
   in `OPEN_ISSUES.md` and not on this queue.
4. **Three pixels.** Unit 275 left `VP2MAA KC3QIS FN00` three pixels over on the
   left list. **No unit action until Tim says.**
5. **`dt` and `hz` were never on the mine list.** Reported by unit 275, not
   repaired. **No unit action until Tim says.**
6. **Where an outcome entry goes when the unit it corrects has none.** Raised by
   unit 276, decided one way and written into `PHASE_OUTCOME.md`. **The ruling
   wants Tim's eye; the change is already in the tree.**
7. **`HM-OPEN-086`** — `TheWholeChainRunsFromOneRightClickTests` has the defect unit
   276 fixed elsewhere. **Found by reading, left alone under §12.6, and its own
   unit's work.** Note that unit 276's fix to `TheMenuIsUnderTheMouseTests` is
   now a second precedent for the repair.
8. **Where the repeat fold stops**, raised by this unit, 2026-09-08. Built the
   conservative way and in the tree at `0c359cc`; the ruling above is what wants his
   eye.
9. **What the turn line says during a transmission already going out**, raised by
   this unit, 2026-09-08. Nothing is in the tree for it; the line as built speaks
   about the next click.
10. **`PHASE_OUTCOME.md` is written by a tool no unit is told to run.** Item 7 of
    the inbound queue said task 6 of this unit would write one, and it has. **The
    underlying question is not closed**: three units wrote none, and the write is
    still not a task unless an instruction names it.

**Dropped as answered:** the inbound item 7's specific ask — that this unit write an
outcome entry — is done, and it is folded into item 10 above as the general question
it belongs to rather than being carried twice.

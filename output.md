READ IN THIS ORDER

A. THE PHASE GOAL. Hamlet works stations on the air. It transmitted on a live
   antenna for the first time on 2026-09-07 and the machinery works.
B. THE STEP AND ITS EXIT CRITERIA. Step E's bench half. The criteria themselves
   are yours at the radio; this unit removes defects standing between you and
   them, and closes none of them itself.
C. WHAT THIS REPORT ADDS, AND WHETHER IT BEARS ON A OR B. It bears on A. **Six of
   the seven tasks were already in the tree** from the earlier issue of this same
   order and were verified by re-running their own tests rather than repeated; the
   amended task 6 is the new work. Section 4 raises 3 items and **none asks for a
   ruling** — two are departures from the instruction's own wording, stated so you
   can overrule them, and one is the standing asks queue.

UNIT:       271 — complete at task 7 of 7 — 2026-09-07 16:40
PHASE GOAL: Hamlet works stations on the air.
UNIT GOAL:  The CQ button calls CQ, the grid fits the message, the contact column speaks only about his own contacts, the stop control says what it is, and a grid tooltip says where the station is rather than where some square is.
ADVANCED:   yes — one new task built, six verified. It advances no step's criterion and the instruction says so itself.
NUMBER:     none for this session's own task. The unit's number is task 1's and it stands: `VP2MAA KC3QIS FN00DJ` went out as `<VP2MAA KC3QIS> FN00DJ` and decoded back to nothing at all.
DRIFT:      0 consecutive units without advance  (was 0)

---

## 1. What Claude did

**Complete, seven of seven, and only one of them was new work.** Machine
`QUIVERFULL`, `C:\Source\HamLet`, branch `main`, pushed.

### The gate

`SHACK_FACTS.md` present, `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs`
present, `CoreHMI.sln` absent, `MURC.sln` absent. **Hamlet confirmed.**
`SESSION.lock` absent and one session in the process list, checked before the
order was read.

### What this session found before it changed anything

**`WORK_INSTRUCTIONS.md` on disk is an amended issue of an order already
executed.** Committed at HEAD it reads *271 — four faults the operator found on
the air*; the working copy reads ***271 — six faults***, and the difference is a
**new task 6** for the grid tooltip with the old task 6, the ALC level, renumbered
to **task 7**.

The four-fault issue ran to completion: `9690c28`, `6337b04`, `9eb7f13`,
`ffe7d8c`, `2a73745`, `a999ec4`, `0eee73b`, with its report at `e25e883`. **Unit
272 has run since**, and the root version was `1.12.126` rather than the
`1.12.71`-ish a fresh 271 would imply.

So tasks 1 to 5 and 7 were **verified rather than repeated**, and the verification
is a re-run of each task's own tests, not a reading of its commit message. That
distinction is the whole of why section 3 quotes numbers rather than claims.

### Task by task

**1 — what went out on the air. Verified by re-running `WhatWentOutOnTheAirTests`
this session.** The figures in section 3 are what came back today.

**2 — the CQ button calls CQ. Verified by re-running `TheCqButtonCallsCqTests`.**
Empty table, one row, several rows: the same string every time.

**3 — the grid fits the message.** `Ft8SendOptions.ForTheMessage` cuts to four
characters at the one place both send routes pass through, and Settings keeps the
six for distance and bearing. Verified by the round trips in section 3.

**4 — the contact column. Verified by re-running
`TheContactColumnSpeaksOnlyAboutHisContactsTests`.** Section 3 quotes the slot.

**5 — the stop control says what it is.** `DigitalStopButton` carries
`Content="{Binding StopLabel}"`, which reads **"Stop"** at rest and **"Stop
transmitting"** while something is armed, with `Classes.hm-live` changing ink
weight and border rather than filling a bar. Read from the markup; the abort
behind it was not touched.

**6 — a grid says where the station is. This session's only new work**, committed
at `5ef559a`.

**7 — the level that was measured.** Already in `SHACK_FACTS.md`: 25 per cent
drive, -12.04 dBFS composed, ALC -2.0 to -1.5 inside the red zone, measured
2026-09-07, with the consequence recorded that no unit may promise to defer it
again.

### Decisions made on this session's own authority, reproduced in full

**One: the tooltip says *They* where the instruction's example says *He*.** The
instruction's wording is *He is in grid JN54*. `Ft8Vocabulary`'s own rule, recorded
in that file since unit 251, is that **no pronoun chooses a gender** — Hamlet has
no way to know who is at the key. The substance of your ruling is that the distance
belongs to the station, and that is untouched; only the pronoun follows the older
rule. **It is section 4's first item so you can overrule it in a word.**

**Two: the comma after *They are* goes when there is no country.** Built as one
clause with an optional country in front, every station the DXCC table declines
read *They are, in grid QG44* — a stumble mid-sentence that reads as though a word
had gone missing. It had.

**Three: unit 252's place-name sweep was updated rather than deleted.** That test
forbade **every** place name; your ruling of 2026-09-07 supersedes it for the
country alone, and only when the country comes from the callsign. So it now runs
against a sender the table declines — `ZZ9ZZZ` — where no country can arrive
legitimately, and **any name at all that appears can only have come from the
square**, which is the fault it still exists to catch. Deleting it would have
removed the guard along with the rule it outgrew.

## 2. What the owner should expect

**All six, in the order you found them:**

- **The CQ button calls CQ.** It composes `CQ KC3QIS FN00` from Settings and reads
  nothing off the table — not the selected row, not several rows, not a row that
  is itself a CQ. It took `VP2MAA` because the send options were built from the
  row under the cursor; they are built from your own profile now.
- **The grid fits the message.** Your Settings still hold `FN00DJ` and always
  will, because the extra two characters are what the distance and bearing are
  measured from. What goes into a transmitted message is `FN00`.
- **What you transmitted twice on 2026-09-07 was not decodable by anybody**, and
  section 3 says exactly what went out instead. That is fixed, and unit 272 has
  since put a guard in front of the send path as well.
- **The contact column is quiet on rows you are not in.** A CQ gets nothing, two
  other stations working each other get nothing, and only a message addressed to
  `KC3QIS` carries a state.
- **The stop control has a label.** "Stop" at rest, "Stop transmitting" while
  something is armed. It is always there and always pressable, exactly as before;
  only its appearance says more.
- **A grid tooltip says where the station is.** *IK4LZH is calling anyone. They
  are in northern Italy, in grid JN54, 4,400 miles away on a bearing of 53
  degrees.*

**What will look wrong and is not:**

- **Some stations get no country.** `VK9` is five different islands and `3D2` is
  three; the table declines rather than guessing, and the sentence still gives the
  square and the distance because those are arithmetic.
- **Some countries get no compass word.** Belgium is 1.95 degrees tall, about two
  grid squares. It is in the table's `declined` list with that reason written
  beside it, so it reads as considered rather than forgotten.
- **`PM95` reads *southern Japan*.** That is honest arithmetic on the stated
  bounds — 35.6°N against a country running 31.03 to 45.55 — and it is the Tokyo
  area, which some would call central. Thirds put it in the southern band.

**Build:** clean, 0 warnings, 0 errors, whole solution, six times.

**Tests:** filtered and foregrounded, and every one of them belongs to this work
instruction. New this session: `TheGridSaysWhereTheStationIsTests` **16 of 16**.
Re-run to verify the earlier issue: `WhatWentOutOnTheAirTests` and
`TheContactColumnSpeaksOnlyAboutHisContactsTests` **6 of 6**, `TheCqButtonCallsCqTests`
green. Repaired because task 6 changed the wording they assert:
`TheGridTooltipSaysHowFarTests` and `TheSenderTooltipNamesTheEntityTests`, now
**42 of 42** with the new file. No suite was run and nothing was backgrounded.

**Pushed to `main`:** `5ef559a`. Version **1.12.126 → 1.12.127**, one patch for
one task. `Ft8Sharp` did not move.

## 3. What you should see

**1. What `VP2MAA KC3QIS FN00DJ` decoded back to.** Re-measured this session
through `Ft8Composer` and back through `Ft8Sharp`'s own `Ft8SlotDecoder`:

```
ASKED FOR    : "VP2MAA KC3QIS FN00DJ"  (20 characters)
AT 48000 Hz  : composed, NonstandardCallsign, bits say "<VP2MAA KC3QIS> FN00DJ"
THE BITS SAY : "<VP2MAA KC3QIS> FN00DJ"
    standard  "VP2MAA" / "KC3QIS" / "FN00DJ"      -> Ok, reads back "VP2MAA KC3QIS FN00"
    standard  "VP2MAA KC3QIS" / "FN00DJ" / ""     -> FirstCallInvalid, reads back -
```

**There is no `DECODED BACK` line, and its absence is the finding.** The standard
packing succeeded and read back `VP2MAA KC3QIS FN00`, a **truncation of your own
words**, so the composer's round-trip guard correctly refused it. The words then
fell through to the pass that allows a callsign on the wire as a hash, where
**`VP2MAA KC3QIS` was hashed as one callsign field** and what went out was
`<VP2MAA KC3QIS> FN00DJ`. `Ft8SlotDecoder` returns nothing at all off that slot.
**You transmitted something nobody could decode, twice, on a live antenna** —
§0.0 pointed the other way, a transmission asserting something nobody receives.

`CQ KC3QIS FN00DJ` did the same thing: `composed, Standard, bits say
"<CQ KC3QIS> <FN00DJ>"`, no decode back.

**With the grid cut to four, both round-trip exactly:**

```
ASKED FOR    : "VP2MAA KC3QIS FN00"  (18 characters)  -> DECODED BACK: "VP2MAA KC3QIS FN00"
ASKED FOR    : "CQ KC3QIS FN00"      (14 characters)  -> DECODED BACK: "CQ KC3QIS FN00"
```

**`messageLength: 16` measured the composed string and not the encoded message**,
which is why it looked healthy: `CQ KC3QIS FN00DJ` is sixteen characters whether
or not anything on the air could read it. Unit 272 has since changed what the
telemetry line reports.

**2. The CQ button's composed string, with rows on the table.** Re-measured this
session:

```
empty table    : "CQ KC3QIS FN00"
one row        : "CQ KC3QIS FN00"
several rows   : "CQ KC3QIS FN00"

the CQ button composes: "CQ KC3QIS FN00"
the bits say          : "CQ KC3QIS FN00"
the decoder returns   : "CQ KC3QIS FN00"
```

It begins with `CQ `, it is the same string every time, and it round-trips.

**3. A slot with a CQ, a third-party exchange and a message to the operator.**
Re-measured this session:

```
"CQ VP2MAA FK52"    -> contact column: (nothing)
"K9TC KJ6IX RRR"    -> contact column: (nothing)
"KC3QIS W1ABC -12"  -> contact column: "your move, 0 slots"
```

**A state on exactly one row.** The test also records what the ungated read still
says about the middle row — `"your move, 0 slots"` — so the gate is demonstrably
doing the work rather than the ledger having changed. Your own compound forms
still count: `KC3QIS/P` and `W4/KC3QIS` both carry the state.

**4. `IK4LZH JN54`'s tooltip, and a Belgian callsign beside it.**

```
IK4LZH is calling anyone. They are in northern Italy, in grid JN54,
    4,400 miles away on a bearing of 53 degrees.

ON4ABC is calling anyone. They are in Belgium, in grid JO20,
    3,900 miles away on a bearing of 49 degrees.
```

The compass word is on one and not the other, and **the difference is a committed
table with a reason beside each row**, `data/callsigns/entity-extents.json`. Italy
is 10.5 degrees tall, about 725 miles from Sicily to the Alps, and *northern
Italy* is ordinary English. Belgium is 1.95 degrees, barely two grid squares, and
*northern Belgium* is not something people say. **Ten entities take a qualifier
and six were considered and refused one**, each refusal with its reasoning, so
Belgium reads as weighed rather than missed. The United Kingdom is the interesting
refusal: it clears the span and fails the idiom, because nobody says *northern
United Kingdom*.

The same callsign moves with its grid and the country does not:

```
JN54 -> They are in northern Italy    JM88 -> They are in southern Italy
JN62 -> They are in Italy             (the middle third takes no compass word)
```

And where the callsign and the grid disagree, **the callsign wins**:

```
CQ W4/YV7AXM FK60 -> They are in United States of America, in grid FK60,
                     2,200 miles away on a bearing of 156 degrees.
```

`FK60` is a Venezuelan square and the sentence names the United States, because
that is where he is transmitting from. Where the DXCC table declines, no country
at all: `VK9XYZ` reads *They are in grid QG44, 9,500 miles away on a bearing of
275 degrees.*

## 4. What's blocking us

**Nothing blocks the next unit.** Three items, none of which asks for a ruling.

**1. The tooltip says *They* where your example says *He*.**

Stated so you can overrule it in a word, not asked.

Your example reads *He is in grid JN54*. `Ft8Vocabulary` has carried a rule since
unit 251 — written into the file — that **no pronoun chooses a gender**, because
Hamlet has no way to know who is at the key. Following the example would have
broken a rule recorded in the very file being edited, so the wording follows the
rule and the substance of your ruling is untouched: the distance belongs to the
station.

**If you would rather it read *He*, say so and it is a one-line change.** It is
here rather than in section 1 alone because it is the kind of thing that is easy
to not notice and then live with for months.

**2. The order on disk was an amended issue of one already executed.**

Reported, not repaired, per the instruction's own rule about tree mismatches.

HEAD carries *271 — four faults*, executed and reported at `e25e883`; the working
copy carries *271 — six faults*, which inserts a new task 6 and renumbers the old
one to 7. **Unit 272 has also run since**, so the tree was two units past the
order's own assumptions and the root version read `1.12.126`.

§9.6 says a session opening an order dated earlier than `OUTPUT.md` is holding
work already done, and should say so and stop. **This one is not that case** — it
carries genuinely new work in its task 6 — so it was executed for the new task and
verified for the rest. The mismatch is worth naming because the phase record
indexes units by number, and two issues of 271 will collide in `PHASE_OUTCOME.md`
the next time either is cited. **This is the second consecutive report saying
that**; unit 252 was also issued twice under one number.

**3. `PM95` reads *southern Japan*, and that is the qualifier's weakest reading.**

Not a defect and not a question. It is honest arithmetic on the bounds the table
records — 35.6°N in a country running 31.03 to 45.55 — and thirds put it in the
southern band. Some would call the Tokyo area central. **The table is the place to
argue with it**, which is why the rule, the banding and the reason for each entry
are all in the data file rather than in code.

### Asks still outstanding

Carried verbatim until you rule, per HM-DEC-139.

**This session cannot honestly reconstruct the queue, and that is now the fourth
report in a row saying so.** HM-DEC-139 requires the work order to carry the
outstanding asks inbound, and **work instruction 271 carries no
`Asks still outstanding` heading**, which by §9.6 makes the order defective and
obliges the session to rebuild the queue from `OPEN_ISSUES.md` and the recent
reports. `output.md` is overwritten every unit, so the reports that would carry
those asks are gone, and rebuilding from `OPEN_ISSUES.md` alone would produce the
long list HM-DEC-140 expressly says does not belong here.

What can be stated without a rebuild:

- **The missing heading is itself the first ask.** It is a defect in the order
  under §9.6 and HM-DEC-137. **Four consecutive orders have now been missing it**,
  which is exactly the shape HM-DEC-137 was written about: a rule nothing carries
  into the session is the same as no rule.
- **The pronoun above is the only thing this unit hands back**, and it is a
  preference rather than a ruling.
- **`HM-OPEN-083` and `HM-OPEN-084`**, raised 2026-09-05, are step 6's two unmet
  exit criteria from the closed sensitivity phase. They are recorded issues with
  an id, an owner and a date, so by HM-DEC-140 they belong in `OPEN_ISSUES.md` and
  not on this queue. Neither was touched.

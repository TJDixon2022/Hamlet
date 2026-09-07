# Work instruction 271 - four faults the operator found on the air

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## THE TWO RULES THAT KILLED SESSIONS

**Tim's rulings of 2026-09-05. Not this unit's to weigh.**

**1. A unit runs no test suite.** **A unit may run only the unit test it
constructs in that work instruction**, filtered by exact name, in the foreground,
with a stated timeout of a few minutes. **An unfiltered `dotnet test` on any
project is forbidden.**

**2. Never background a command and poll for it.** Sessions were killed by the
watchdog sitting in `until grep -q "exited with code" ...; do sleep 15; done`
against a twelve-minute watchdog.

`dotnet build` is allowed, foregrounded, with a timeout.

---

## THE TOOL RULE

**This session's shell may refuse calls.** A refused shell call is a signal to
reach for the other tool, not to stop. **The file-editing tools have been
unaffected throughout.** Record every refusal verbatim. **Nothing in this unit
halts the loop.**

**This shell collapses a doubled backslash inside a quoted heredoc.** Use the
file-editing tools for anything with escapes in it.

---

## Why this unit exists

**Hamlet transmitted on a live antenna for the first time on 2026-09-07**, twice,
both slots clean - keyed 0.5 s into the slot, 12.64 s exactly, 606,720 samples at
48 kHz on 14.074000, `outcome: Sent`, `cameOutOfTransmit: OrdinaryUnkey`. The ALC
sat at -2.0 to -1.5 inside the red zone at 25 per cent drive. **The machinery
works.**

**Tim then found four faults in twenty minutes at the radio.** All four are his,
observed on his own screen, and two of them are on the air.

```
PHASE GOAL:   Hamlet works stations on the air.
UNIT GOAL:    The CQ button calls CQ, the grid fits the message, the contact
              column speaks only about his own contacts, and the stop control
              says what it is.
ADVANCES:     step E's bench half. The criteria themselves are Tim's at the
              radio; these are four defects standing between him and them.
```

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
Report them; do not repair the instruction.

- The Send area under the waterfall holds a **CQ** button and, beside it, an
  **unlabelled orange block** which is the stop control.
- Pressing CQ on 2026-09-07 produced *Sent to VP2MAA, "VP2MAA KC3QIS FN00DJ" in
  the slot at 17:12:30 UTC*. **That is a reply, not a CQ.**
- The operator's grid in Settings is **`FN00DJ`**, six characters, marked
  `verified` from callook.info.
- **Every decoded message on his screen carries a four-character grid** - `FL20`,
  `JN86`, `EL29`, `EM16`, `EK57`, `JN03`.
- The `contact` column added by unit 266 reads `your move, 0 slots` on rows that
  are **conversations between two other stations** - `K9TC KJ6IX RRR` is KJ6IX
  telling K9TC he received, and Tim is in none of it.
- `ft8_transmission` telemetry records `messageType: Standard, messageLength: 16`.
  **16 is the length of the composed string, not of what was encoded.**
- Root version after work instruction 270. **Read it; do not assume.**

Known reds, inherited, **never chased**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list
tripwire.

---

## Rulings in force

**Tim's, 2026-09-07:**

- **The CQ button calls CQ.** It composes from his own settings and from nothing
  on the table.
- **The contact column speaks only about contacts he is in.** *"The your move text
  is obnoxious."* A row between two other stations says nothing there.
- **The stop control says what it is.**

**Standing:**

- **One click, one transmission.** **The abort is not to be weakened, made
  conditional, or routed around.**
- **Nothing is forbidden in the menu**, nothing is closed, hidden or ruled on. The
  app reports.
- **§0.0 and HM-DEC-092.** Never present a guess as a decode; a picture binds as
  hard as a sentence. **This unit's exposure is the grid**: a message that cannot
  be decoded by anybody is a transmission asserting something nobody receives.
- **HM-DEC-012 and §0.5.** Family colour is **text colour only**; a bar is never
  filled with it.
- **The dummy load is withdrawn in full.** Do not reference it.
- **`Ft8Sharp` is a faithful MIT port and nothing changes a line of it.**

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, and
`NOTE` saying what is moving inside the task. The same every ten minutes while a
task is running. **Use the file-editing tools if the shell refuses.**

---

## Tasks

### Task 1 - what actually went out on the air

**Reading and measuring. This is first because two faults are on the air and
their severity is unmeasured.**

- **Take the exact string the CQ button composed** - `VP2MAA KC3QIS FN00DJ` - and
  put it through the encoder, then through `Ft8Sharp`'s own decoder. **Report what
  comes back.**
- **Do the same for `CQ KC3QIS FN00DJ`.**
- **Say what the six-character grid did**: truncated to `FN00`, rejected, or
  encoded as something else. **Do not assume it was harmless.**
- If the message did not survive the round trip, **that is the headline finding**:
  Hamlet transmitted something nobody could decode, twice, on a live antenna.
- Report where `messageLength: 16` comes from and whether it describes the
  composed string or the encoded message. **A length that measures the wrong thing
  is worse than no length.**

### Task 2 - the CQ button calls CQ

- **The CQ button composes `CQ <callsign> <grid>` from Settings and from nothing
  else.** Not the selected row, not the last decoded message, not anything on the
  table.
- Find why it took VP2MAA and say so.
- Test, watched failing first: with a row selected, with several rows selected,
  and with a row that is itself a CQ, **the CQ button composes the same string
  every time** and it begins with `CQ `.

### Task 3 - the grid fits the message

- **A standard FT8 message carries a four-character grid.** The operator's grid is
  six and every station on his screen sends four.
- **Wherever a grid enters a transmitted message, it is the first four
  characters.** His settings keep the six - the extra two are used for distance and
  bearing and are not to be discarded there.
- **Every composed message round-trips**: compose, encode, decode with
  `Ft8Sharp`, and get back the same text. **That is the test, and it is the one
  that would have caught this.**
- Test, watched failing first: a CQ, a grid reply, a report, an `R-` report,
  `RRR`, `RR73` and `73` each round-trip, and a six-character grid in Settings
  produces a four-character grid in the message.

### Task 4 - the contact column speaks only about his contacts

- **A row shows a contact state only when the message is addressed to the
  operator's callsign.** Everything else says nothing in that column.
- **A CQ is not a contact.** It is an invitation, and it gets no state.
- **Two other stations working each other get no state**, whatever they are
  saying to one another.
- The ledger itself is unchanged - **this is what the column shows**, not what is
  tracked. Unit 266 built the ledger and it stays.
- Test, watched failing first: a slot holding a CQ, a third-party exchange and a
  message addressed to the operator produces a state on **exactly one row**.

### Task 5 - the stop control says what it is

- **It carries a label.** The most consequential control in the application is an
  unlabelled coloured block and the operator had to ask what it was.
- **The bar is not filled with a family colour** (HM-DEC-012, §0.5). Family colour
  is text colour only.
- **It reads as available or unavailable** - there is nothing to stop when nothing
  is transmitting, and it should look different then.
- **Do not change what it does.** The abort behind it is proven and is not this
  unit's to touch.

### Task 6 - the level that was measured

**Named drop candidate.**

Record in `SHACK_FACTS.md`, in the file's own format: **transmit drive 25 per
cent, -12.04 dBFS composed, ALC -2.0 to -1.5 inside the red zone, measured
2026-09-07 on the IC-7300 at 14.074 MHz.** Source: the operator, at the radio.

**This is the number no machine in this repository could know**, and four unit
reports promised to defer it to him before he had a control to set it with. **Once
it is written down, no unit may promise to defer it again.**

---

## Parked - do not touch, do not raise

- **The abort's behaviour.** Proven by units 257 and 263.
- **The contact ledger itself.** Unit 266. Only the column's display changes.
- **Automatic sequencing.** Out of this phase.
- **Steps D and E's own criteria**, which are Tim's at the radio.
- **Anything in `src/Ft8Sharp/`.**
- The OSD re-encoding count, `ReusableWindow`, `ProcessDelayForTests`, the tap's
  owner, the waterfall's first row, unit 237's Extensible conclusion, work
  instruction 231's four tree items, `validate-output.bat`'s permitted-spellings
  bug, the 101.33 ms pulse above 6 kHz, the CW decoder and its inherited reds.

---

## What not to do

- **Do not change the abort.**
- **Do not discard the six-character grid in Settings.** It is used for distance
  and bearing.
- **Do not change what the ledger tracks**, only what the column shows.
- **Do not fill a bar with a family colour.**
- **Do not assume the six-character grid was harmless.** Measure it.
- **Do not touch `src/Ft8Sharp/`.**
- **Do not run a test suite.** Only the tests you just wrote, filtered,
  foregrounded, with a timeout.
- **Do not background a command and poll for it.**
- **Do not run `Hamlet.App.Tests`.**
- **Do not ship a placeholder token in a reported number.**

---

## Committing and pushing

Commit and push each task before starting the next. Bump the root version's patch
by one from whatever work instruction 270 left. **`Ft8Sharp` does not move.**

---

## Reporting

`output.md` at the repository root, overwritten, four sections per
`CLAUDE_CODE.md` §8 with the header block above section 1.

**Section 3 leads with three things:**

1. **What `VP2MAA KC3QIS FN00DJ` decoded back to**, quoted - what actually went
   out over a live antenna, twice.
2. **The CQ button's composed string, quoted**, with a row selected - showing it
   begins with `CQ ` and ignores the table.
3. **A slot with a CQ, a third-party exchange and a message to the operator**,
   showing a contact state on exactly one row.

**Section 2 says what he will see change on his screen**, all four, in his own
terms.

Write `output.md`, then stop. Do not start the next unit.

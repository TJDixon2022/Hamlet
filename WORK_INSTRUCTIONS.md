# Work instruction 288 - where FT4 lives, decided by reading

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

**Tim's rulings of 2026-09-05, HM-DEC-155.**

**1. A unit runs no test suite.** **Only the unit tests it constructs or rewrites in
this work instruction**, filtered by exact name, foregrounded, with a stated timeout.
**An unfiltered `dotnet test` on any project is forbidden.**

**2. Never background a command and poll for it.** The watchdog fires after twelve
minutes with no status write.

`dotnet build` is allowed, foregrounded, with a timeout.

**Tool fact, fourteen units old:** this shell will not carry a quoted heredoc
containing an apostrophe, and it collapses a doubled backslash inside one. Use script
files.

---

## Why this unit exists

**This is the first unit of a new phase and it builds nothing.**

**FT4 is a button on the Digital tab that does nothing**, and Tim ruled on 2026-09-08
that **FT4 works exactly the way FT8 does.**

Most of the work looks done: FT4 shares FT8's 77-bit payload, its LDPC(174,91) code
and its CRC-14. What differs is the clock and the modulation - **7.5-second slots,
four tones, 4.48 seconds of transmission** - and everything above the decoder is
already mode-neutral or nearly so.

**But where the FT4 decoder goes is a real decision and it is not the author's to make
from memory.**

**The rule, ruled 2026-09-08:** if **`ft8_lib` carries FT4**, the port carries FT4 and
the fidelity tests extend to cover it. If it **does not**, FT4 is new work and belongs
in `Ft8Sharp.Deep` or a sibling - **because `Ft8Sharp`'s value is that it cannot drift
from upstream**, and that byte-fidelity is the instrument every measurement in the
sensitivity phase leaned on.

**This unit reads and reports. It decides that question with evidence and stops.**

```
PHASE GOAL:   FT4 works exactly the way FT8 does.
UNIT GOAL:    Where the FT4 decoder goes is answered from the tree, with what is
              already shared listed so nothing is written twice.
ADVANCES:     step 0.
```

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches. Report
them; do not repair the instruction.

- **`tools/build-ft8-oracle.bat`** builds `decode_ft8.exe` from a vendored or cloned
  `ft8_lib`. **Find where that source is** - it may be outside the repository.
- **`Ft8Sharp` is at `0.10.7`** and **51 of 51 fidelity tests** pass against upstream.
- **`Ft8Sharp.Deep` is GPL-3.0**, built in the sensitivity phase, and consumes the
  port's candidates and soft values.
- **Unit 245 found that nothing outside `Ft8Sharp` can construct an
  `Ft8CodewordResult`** - the port is deliberately closed, and that matters if FT4
  lives outside it.
- **Unit 287 found FT4 is an ADIF submode** - `MODE=MFSK, SUBMODE=FT4` - and
  `AdifContact` has no `SUBMODE` field. **`MODE=FT4` is invalid ADIF.**
- **The Digital tab's FT4 button exists** and unit 251 made the mode buttons tune.
  **Report what pressing FT4 does today.**
- Root version after unit 287 was **1.12.214**. **Read it, do not assume.**

Known reds, inherited, **never chased**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list tripwire;
`HM-OPEN-088`'s ten.

---

## Rulings in force

**Tim's, 2026-09-08:**

- **FT4 works exactly the way FT8 does.** **Anything FT8 does that FT4 does not is a
  gap to be named**, not a scope decision a unit may make.
- **Where FT4 lives follows what upstream does**, read rather than assumed.

**Standing:**

- **`Ft8Sharp` is a faithful MIT port and its byte-fidelity is not to be spent
  casually.** `Ft8Sharp.Deep` is GPL-3.0.
- **No algorithm comes from WSJT-X's source.** Published description only.
- **One click, one transmission.** **Nothing in this unit transmits.**
- **§0.0.** Never present a guess as a decode.

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, and `NOTE`
saying what is moving inside the task. The same every ten minutes while a task is
running. **Use the file-editing tools if the shell refuses.**

---

## Tasks

### Task 1 - what upstream actually carries

**Reading only. Build nothing, change nothing.**

- **Find `ft8_lib`'s source** and say where it is. `tools/build-ft8-oracle.bat` names
  a path; **check it exists** and report if it does not.
- **Does it carry FT4?** With file and line. **If it does**: its sync pattern, its
  symbol timing, its tone count, its slot length, and whether its encoder and decoder
  are shared with FT8 or separate.
- **If it does not carry FT4, say so plainly.** That is a finding and it decides the
  question in the other direction.
- **Do not infer from a filename.** A file called `ft4.c` that is a stub is not
  support.

### Task 2 - what `Ft8Sharp` already shares

**Reading only.**

- **A list, with file and line**, of everything in the port that FT4 would use
  unchanged: the message layer, the LDPC code, the CRC-14, the callsign fields,
  anything else.
- **And what it could not use**: anything that assumes eight tones, 15-second slots,
  0.16-second symbols or FT8's Costas array. **Name each with the assumption it
  makes.**
- **This list is what stops FT4 being written twice**, so err toward listing.

### Task 3 - the decision, with its reason

- **Port or sibling**, following the ruled rule and task 1's evidence.
- **If the port**: what the 51 fidelity tests become, and how upstream parity is still
  provable once the port carries something upstream may implement differently.
- **If a sibling**: how it reaches the shared parts, given unit 245 found the port
  deliberately closed - **and what has to open, minimally, for that to work.**
- **Say what you would need that you do not have.** A decision that needs a ruling
  from Tim is better named now than discovered in step 1.

### Task 4 - what the FT4 button does today

- **Press it, in the running application, and report what happens.** Unit 284 proved
  that standing the application up beats searching the source, and five orders before
  it proved the opposite.
- **Does it tune? Does it change the decoder? Does it change the slot grid?**
- **What breaks, and what silently does nothing.**

### Task 5 - the gaps, named

**Named drop candidate.**

**Tim's ruling is *exactly the way FT8 does*.** List what FT8 has that FT4 would not,
beyond the decoder: the log's missing `SUBMODE`, the achievements row, the frequency
table, the slot arithmetic, the sidecar, the turn ring, anything else.

**Name them. Fix none.** This is step 0 and the phase's later steps are where they go.

### Task 6 - the phase's bookkeeping

**File edits only.**

- `PROJECT_CARD.md` gains the new `PHASE` and `PHASE_SET` from `PHASE_STATUS.md`'s
  header. **This file changes only by ruling** (§13.3); the ruling is Tim's approval
  of `PHASE_PLAN.md` on 2026-09-08.
- Append that ruling to `DECISIONS.md`, next id in sequence.
- **Append this unit's entry to `PHASE_OUTCOME.md`** through
  `tools\arbiter\outcome-append.bat`. **If the shell refuses, append with the
  file-editing tools** and say so.
- **If `install-phase.bat` was not run**, say so and skip the card and the ruling.

---

## Parked - do not touch, do not raise

- **Building anything for FT4.** Steps 1 to 4.
- **`SUBMODE`.** Step 3 of the phase.
- **The whole asks queue** carried since unit 271.
- **PSK31, WSPR, Voice**, and a WSPR achievement measured from spots.
- **Automatic sequencing.**
- **Anything in `src/Ft8Sharp/`** - this unit changes no code at all.

---

## What not to do

- **Do not build anything.**
- **Do not decide where FT4 lives from memory.** Read upstream.
- **Do not infer support from a filename.**
- **Do not fix a gap task 5 finds.**
- **Do not touch `src/Ft8Sharp/`.**
- **Do not run a test suite.**
- **Do not background a command and poll for it.**
- **Do not ship a placeholder token in a reported number.**

---

## Committing and pushing

Commit and push each task before starting the next. Bump the root version's patch by
one if anything was committed. **`Ft8Sharp` does not move.**

---

## Reporting

`output.md` at the repository root, overwritten, four sections per
`CLAUDE_CODE.md` §8.

**NUMBER: how much of FT4 already exists in this tree** - the shared parts counted
against what would be new.

**Section 3 leads with four things:**

1. **Whether `ft8_lib` carries FT4**, with file and line, or that it does not.
2. **The decision** - port or sibling - and the reason it follows from task 1.
3. **What pressing the FT4 button does today.**
4. **The gaps**, listed, none fixed.

**Section 2 says what this means for the phase**: how much is already there and what
the first build unit will actually have to write.

Write `output.md`, then stop.

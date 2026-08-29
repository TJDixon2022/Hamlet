```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      CLAUDE.md
  MUST EXIST:      data/bands/us-neighborhoods.json
  MUST NOT EXIST:  ANNUNCIATOR_PANEL.md
  MUST NOT EXIST:  src/CoreHMI

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

# Why this unit exists

**A single `26` read returns a stale answer. Tim has to issue the query twice
before USB can be told from USB-D.** That is the number this unit is aimed at:
reads-to-a-correct-answer goes from 2 to 1.

Where it came from: Tim reported it at the bench on 2026-08-28 while recovering
from a mode fault on 20 m FT8. It has not been measured in code. **Task 1 is the
measurement, and if it does not reproduce, this unit stops there and says so** —
the rest of the unit rests on it.

The bench session behind it: the radio was in CW at 14.074, FIL2, 500 Hz. The
operator heard nothing, tuned up, and found FT8 at 14.075. **That is HM-DEC-054's
whale song, a second time, fourteen days after the ruling written to prevent it.**
The mode automation HM-DEC-056 built did not fire, because the operator was
tuning by hand rather than crossing a neighborhood boundary the app was watching.

```
PHASE GOAL:   not set — PROJECT_STATUS.md reads PHASE: — and PROJECT_CARD.md
              carries no phase field. See section 4 of the delivery message.
UNIT GOAL:    One CI-V read returns the current answer, and entering data
              territory and staying there sets the data mode without being asked.
ADVANCES:     none — no phase goal is set to advance. This is a defect unit.
```

---

# Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and
report any mismatch.

Every path, type name and behaviour below comes from `DECISIONS.md` refs and from
Tim's bench report, not from reading the code. Specifically unverified:

- `src/Hamlet.RadioEngine/Civ/CivReads.cs` and `CivWrites.cs` as the location of
  the frame reader and the mode write.
- `src/Hamlet.RadioEngine/Explore/ModeFollowPlan.cs` as the tune-in decision.
- `data/bands/us-neighborhoods.json` as having no passband field today.
- That the app reads `26` rather than `04` for the mode.

**Report the mismatch; do not repair the instruction.** Mismatches go in the
report even where the work succeeded anyway. No checks are expected to fail; if
any are already red when you arrive, name them with their ids so a known red is
not rediscovered next unit.

---

# Rulings in force

## HM-DEC-056 — transcribed, in force, do not re-argue

Hamlet writes to the radio for the first time, and what it writes is the mode:
tuning into a neighborhood sets the mode that neighborhood is worked in.

The command is `26`, not `06`. Command `06` sets a mode and a filter and has no
way at all to say whether the data variant is wanted (p. 19-8). Command `26`
carries the mode, a data mode flag and the filter, for the selected or unselected
VFO (p. 19-11). **Hamlet sends the data flag and skips the filter byte, because
the manual says the radio then picks that mode's own default filter, which is a
better answer than any Hamlet could invent for somebody else's rig.**

Nothing is assumed from having sent it. The radio acknowledges with FB or refuses
with FA, and anything else leaves the value UNKNOWN rather than set to what was
asked for. Every write the app makes on its own initiative is narrated in the
status line. The operator's own hand always wins: a mode change Hamlet did not
make suspends the automation until the next band change re-arms it, and suspended
is a visible state on screen rather than a silent one. A flip waits for the dial
to settle, so crossing three neighborhoods in one drag produces one change and
not three.

A new read came with it: command `26` in its read form reports the mode, the data
flag and the filter together, which is the only way to tell USB from USB-D —
command `04` says USB for both. `DataMode` is a first-class field with an unknown
state like every other, read on connect and when the diagnostics screen is
opened, and never in the poll loop.

**Rejected, and not to be revisited inside this unit:** command `06`; assuming a
write landed without an acknowledgement; a silent suspension when the operator's
hand intervenes; one change per neighborhood crossed during a drag.

## HM-DEC-054 — transcribed, in force, do not re-argue

The neighborhood map lives in `data/bands/us-neighborhoods.json` with a source on
every row. Convention and regulation are different files on purpose: `data/privileges`
says what may be transmitted and has legal weight, `data/bands` says what will
actually be found and has none.

**One editorial rule, stated in the file and applied everywhere.** Several sources
publish a watering hole as one dial frequency rather than a range. Those blocks
run from the published frequency to the next one, or three kilohertz, whichever
comes first, because these modes are worked in upper sideband with audio up to
about three kilohertz and that is where the signals land. The 070 Club states
exactly that, so even the width is cited rather than chosen.

**Rejected:** inventing a neighborhood from memory; coloring an unclaimed stretch
as Morse; the card inviting the operator to call where the map has a caution.

## Proposed, NOT in force — the filter byte

**This is a proposal from the web session, marked as such per §4.4. It is not a
ruling and no task below depends on it.** Tim rules on it before it is built.

HM-DEC-056 skipped the filter byte on the reasoning that the radio's own default
is better than one Hamlet invents. **The manual's filter table (p. 4-6) appears to
falsify the premise**: SSB-D's slots are not SSB's. SSB reads FIL1 3.0 kHz, FIL2
2.4, FIL3 1.8. SSB-D reads FIL1 3.0 kHz, **FIL2 1.2**, FIL3 500 Hz. A radio whose
remembered USB-D default is FIL2 lands on a 1.2 kHz window over a 3 kHz FT8 block
— under half of it — and produces a thinned version of the symptom HM-DEC-056
exists to prevent.

Against it: the filter is also how an operator deliberately narrows onto one
signal, and a per-mode default is a stored preference of his. Verify the table
against the manual before Tim rules; **the manual is cited and never committed**,
so this cannot be checked from the tree.

---

# Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` — `STATE`, `TASK: n of m`, `BALL`, `UPDATED` read from the clock, and
`NOTE` saying what is moving inside the task, not restating the task name. Do the
same every ten minutes while a task is running.

---

# Tasks

## Task 1 — trace the read path, and reproduce the double query

**Nothing is built until this reports.** Say what you find rather than confirming
this list.

Answer from the code: how a CI-V read is issued and how its reply is taken off
the wire; whether the reader matches on the command byte it asked for or takes
the next frame available; whether unsolicited transceive frames and the radio's
own echo have a route that is separate from a pending read; whether the mode read
uses `26` or `04`; and whether any caller already re-reads to work around a stale
answer.

Two mechanisms are the leading hypotheses and both are live by default on this
radio. CI-V USB Port defaults to **"Link to [REMOTE]"** (p. 12-8), which echoes
transmitted frames back. Transceive sends unsolicited frequency and mode
broadcasts whenever the dial moves. Either puts a frame in the buffer that a
next-frame-wins reader mistakes for its reply, and a second read succeeds because
the queue has drained.

**Produce the before-number**: reads required to obtain a correct
mode-and-data-variant answer, measured, with the frames traced.

**If the double query does not reproduce, stop here and report.** Do not build
tasks 2 and 3 against a fault that is not there.

## Task 2 — the frame reader returns the answer it asked for

A read matches on the command byte it issued, discards frames it did not ask for,
and **returns an explicit unknown on timeout rather than the next thing that
arrives**. Unsolicited broadcasts route to whatever consumes them and are never
swallowed by a pending read.

The reason, inline because a rule without it gets talked out of: a reader that
takes the next frame is correct exactly until the radio volunteers something, and
this radio volunteers constantly while the dial moves — which is precisely when a
scan is running and precisely when the answer matters.

## Task 3 — audit every CI-V read

If task 1 confirmed the mechanism, every read in the app has been capable of
returning the previous answer. Find them all. **Report the count**, and where a
caller carried a re-read as a workaround, remove it and say so — a workaround
left in place hides whether task 2 worked.

## Task 4 — the dwell rule

Entering data territory arms a timer. **The condition is the same neighborhood and
an unchanged frequency across consecutive polls spanning one second — not one
second spent inside the block.** Movement disqualifies, not position: a slow tune
sits inside a 3 kHz block for longer than a second while still moving, and Tim
crosses data every time he scans from CW to voice.

- Suppressed entirely while the scanner is running.
- Leaving and re-entering re-arms from zero.
- Leaving before maturity discards silently. A write that did not happen is not
  narrated. HM-DEC-056 already narrates the ones that do.

This extends HM-DEC-056's settle rule rather than replacing it; that ruling waits
for the dial to settle and this says how long and what disqualifies.

## Task 5 — data territory sets its mode on dwell

On a matured dwell inside a neighborhood whose mode is a data mode, send the
`26` frame per HM-DEC-056 — VFO selector, mode, data flag, **no filter byte**
until Tim rules on the proposal above.

Read back with `26` and the selector alone. **Unconfirmed leaves the mode
UNKNOWN**, per the ruling. Narrate in the app's voice.

The read-back is a second round trip and is not the fault task 2 fixes: task 1's
read discovers state, this one verifies a change. Two reads asking the *same*
question are the defect; two reads asking different questions are the design.

## Task 6 — the card says where the signals are — DROP CANDIDATE

**This is the drop candidate. It is dropped whole and you say that you dropped
it.** Not by position; it is named here.

`14.074` is a dial frequency and no station transmits on it. The energy sits as
audio offsets above the dial in upper sideband, which is the 3 kHz the editorial
rule in HM-DEC-054 already encodes. The card names the number; it should name the
dial and the block that number opens onto, so that **dead at the published
frequency and alive one kilohertz up reads as a correctly tuned radio** rather
than an empty band.

Tests: a fixture for CW/FIL2/500 Hz at 14.074 — the 2026-08-28 state; a scan pass
crossing three data neighborhoods without dwelling, asserting zero writes; a
reader fed its own echo then the true reply, asserting one read returns the true
reply.

---

# Parked — do not touch, do not raise

- **The Twin PBT.** No write exists in the command table and reading it needs
  `14 08` re-read column-aware — the row `CLAUDE.md` records as once confused with
  the CW pitch. Real, and its own unit.
- **RIT left on as a silent-radio cause.** Real, the command is unverified, and it
  is the same unit as the PBT.
- **The decoder.** Nothing in this unit is evidence about it.
- **`PROJECT_CARD.md` having no phase field.** Raised in the delivery message and
  changed only by ruling (§13.3). Not this unit's business.

A parked item that turns out to block a task is raised once, and says it was
parked.

---

# What not to do

Standing prohibitions live in `CLAUDE.md` and are cited, not retyped.

Unit-specific:

- **Do not send the filter byte.** HM-DEC-056 rejected it and the reversal is a
  proposal awaiting Tim. Building it would be a session overturning a ruling.
- **Do not write slot widths via `1A 03`.** Selecting a slot is a session choice;
  changing what a slot *means* rewrites a stored preference of the operator's.
- **Do not commit the IC-7300 manual.** HM-DEC-056: cited and never committed.
- **No transmit work.** §0.2 is untouched.

---

# Committing, pushing, reporting

Commit and push each task before starting the next. Name the branch in the report
and state whether the push succeeded; a refused push is reported as refused.

Write `output.md` at the repository root per `CLAUDE_CODE.md` §8 — the header
block from the clock, then the four sections. **Section 3 leads with the answer
this unit was commissioned to ask: how many reads it now takes to get a correct
mode-and-data-variant answer, before and after.**

`ADVANCED: no` is the expected answer here and is written without apology; carry
`DRIFT` forward from the block above, which reads 0 because no phase goal is set
to drift from — say so on the line.

**Every exit writes the report.** Complete, blocked, failed, or stopped early by
your own judgment. If you stop with tasks remaining, name them and say why in
section 1, and say whether what you dropped was task 6.

Then stop. Do not start the next unit.

# PHASE_PLAN.md

**Governed by `PHASE_CONTROL.md`. Approved by Tim, 2026-09-07. This replaces the
send phase plan of 2026-09-06, which is archived at
`docs/phase-send-run/PHASE_PLAN.md`.**

---

## The phase

**Hamlet works stations on the air.**

## The description

The transmit machinery is built and measured. What is left is the operator's side
of it and one evening at the radio.

**Eight units under the previous plan advanced real work and closed nothing.** The
stop button was proved - pressed 4.2 s into a 12.64 s transmission, the card quiet
20 ms later, **8.4 seconds of audio that would have gone out did not**, the button
returning in 1.3 ms. The whole chain was proved on one machine: composed, played
to a real render endpoint, captured back, decoded, **3 of 3 messages as the same
text**. A whole exchange was walked through the application. Transmit level went
from **0.00 dBFS, full scale, to -12.04 dBFS** with a control and a readout.

**And steps 2, 3, 4 and 5 were all still `partial`.**

**That was a defect in the plan, not in the work.** Every one of those steps
carried a criterion only Tim's radio could answer - *the right level* is a fact
about his USB input, and no unit on a machine with no radio can close it. So the
units did everything reachable, deferred the rest, and the steps stayed open.
Unit 265's own words: *a criterion deferred to an operator who has no control and
no number is not deferred; it is unclosable by anybody.*

**This plan is the same phase re-cut so its steps can finish.**

---

## The two rules this re-cut exists to enforce

**1. A bench step's criteria are all satisfiable on a machine with no radio.**
Anything that needs Tim's antenna, his USB input, or his ears belongs in a shack
step. **A bench step never defers a criterion to him.** If a criterion cannot be
met at the bench, it is in the wrong step and the arbiter moves it, recording why.

**2. A step has at most four criteria.** Step 3 had six, and each unit chipped one
corner while the step stayed open. **Few enough that one unit can close it** - so
a step closing means something, and the drift counter and stop 10 catch a real
stall instead of an unclosable step.

---

## This phase runs unattended to the bench steps' end

**Steps 0, A, B and C need no radio, no ruling and nothing from Tim.**

**Steps D and E are Tim at the radio**, they are last, and nothing before them is
blocked by them.

### The steps are a hypothesis, not a contract

The arbiter may, without asking, recording the evidence in `PHASE_OUTCOME.md`:
**reorder** steps that depend only on their stated entry; **replace** a step with
a better approach; **retire** one measurement shows cannot pay for itself, closing
it *unachievable* with the number; **add** a step the phase needs; **move a
target** found to have been measured wrong; and **move a criterion into a shack
step** when it turns out only the radio can answer it.

### Named alternatives to stopping, ruled in advance

| If | Do not stop. Instead |
|---|---|
| a criterion needs the radio | **move it to step D or E**, record the move, close the bench step on the rest |
| a target is not reached | close the step with the figure reached and what was tried |
| an approach fails | abandon it, record its cost, take another |
| a defect is found in the receive path | record it, work around it, continue |
| the tree disagrees with this plan | the tree wins. Report the mismatch and continue |
| a licence, naming or scope question arises | **already ruled below.** Do not raise it |
| the shell refuses a call | use the file-editing tools |

---

## The three things the arbiter may not reason past

1. **The abort.** Every path that keys the transmitter has a same-thread,
   no-await abort - CI-V `0x17` with `0xFF`, PTT off as the fallback. **Built and
   proven by units 257 and 263. It is not to be weakened, made conditional, or
   routed around.**
2. **One click, one transmission.** Hamlet transmits because the operator
   clicked. **Never on a timer, never on a decode, never to continue a contact.**
   A transmission he did not ask for goes out over other people's band and cannot
   be taken back.
3. **Licence privileges.** Hamlet never transmits outside them. The Settings gate
   is not bypassable from any send path.

---

## Rulings in force

**Not to be re-argued by any unit.**

**Tim operates a licensed station on an antenna and Hamlet transmits on the air.**
**HM-DEC-008 and HM-DEC-098 are withdrawn in full.** The dummy load is **not a
stage, not a fallback, and not to be referenced.** **Do not propose it, do not
treat its absence as a risk, and do not add a compensating control in its place** -
no confirmation dialog, no power limit, no test mode.

**One click, one message.** **Right-click sends immediately, in the next slot,
with no confirmation.**

**Nothing is forbidden in the menu.** The expected next message is highlighted;
everything valid stays clickable. **FT8 loses transmissions constantly, so sending
the grid a second time is correct behaviour**, and a repeat shows its count.

**A contact is never closed by the app.** `73` is politeness. Complete means the
exchange has what a QSO needs. **Hamlet is not the radio police.**

**`Ft8Sharp` is a faithful MIT port and nothing changes a line of it.**
`Ft8Sharp.Deep` is GPL-3.0.

**The engine is not told that tabs exist** (§0.1). **Nothing interprets a message**
(§12.1) - a row's state is bookkeeping over which messages passed, not meaning.

---

## What a unit runs

**A unit runs no test suite.** Tim runs them at the end of the phase.

**A unit may run only the unit test it constructs in that work instruction**,
filtered by exact name, foregrounded, with a stated timeout. **An unfiltered
`dotnet test` on any project is forbidden.**

**Never background a command and poll for it.** Sessions were killed by the
watchdog on 2026-09-05 and 2026-09-06 sitting in
`until grep -q "exited with code" ...; do sleep 15; done` against a twelve-minute
watchdog.

`dotnet build` is allowed, foregrounded, with a timeout.

**A unit may not add a test without naming the breakage it would have caught.**

Known reds, inherited, **never chased**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list
tripwire.

---

## Step 0 - the record is honest about where the phase stands

**Delivers:** the previous plan's steps closed at what they actually reached, and
the double-entry bug fixed.

**Entry:** none.

**Exit:**
- **Steps 2 and 3 of the old plan recorded `done`**, with the figures that closed
  them: the loopback at 3 of 3, the level at -12.04 dBFS, the rate and device by
  units 256 and 262. **What was open in them was the radio-side level, which is
  now step D.** *must-pass*
- **`PHASE_OUTCOME.md` stops recording each unit twice.** Every unit appears once
  by its real number, not also by the loop's iteration number - `UNIT 262 - STEP 3`
  and `UNIT 5 - STEP 3` are the same unit. **Fix `outcome-append.bat`; leave the
  existing duplicates in place and name them.** *must-pass*
- The archived plan is left alone. *must-pass*

**Depends on:** nothing.

---

## Step A - the row knows where the contact stands

**Delivers:** per-station contact state, shown, deciding nothing.

**Entry:** none.

**Exit:**
- Per station: which messages passed each way and how many slots ago. *must-pass*
- **Four states shown per row** - waiting on him, your move, complete, gone
  quiet - with slot counts. *must-pass*
- **Complete means the exchange has what a QSO needs**, and the absence of `73`
  never withholds it. **Nothing is closed, hidden or forbidden by the app.**
  *must-pass*
- **A station working three others at once reads as gaps, not a fault**, proved
  against a recorded multi-slot scene. *must-pass*

**Unit 264 already walked a whole exchange through the application** and read
`complete, 0 slots` off the row. **Check what survives before rebuilding it.**

**Depends on:** nothing.

---

## Step B - right-click and it goes

**Delivers:** the CQ button, the menu, and the Send area filled.

**Entry:** step A.

**Exit:**
- **A CQ button** sending `CQ KC3QIS FN00` from settings, no typing. *must-pass*
- **Right-click a decoded row**: every message valid at that point offered, **the
  expected one highlighted, none forbidden**, a repeat showing its count.
  *must-pass*
- **One click sends exactly one message** - after a send, nothing further
  transmits without another click, asserted by a test. *must-pass*
- What is being sent, and to whom, appears in the Send area beneath the waterfall;
  **out of licence privileges it says so and sends nothing.** *must-pass*

**Depends on:** step A.

---

## Step C - the whole chain runs from one click, at the bench

**Delivers:** click to keyed to unkeyed, proven end to end with no radio.

**Entry:** steps A and B.

**Exit:**
- **One right-click drives the whole chain**: menu, compose, key, play, unkey,
  telemetry - **exercised in one test**, with the transmit endpoint on a loopback
  and CI-V on a fake transport. *must-pass*
- **The audio that reaches the endpoint decodes back to the message the operator
  clicked.** *must-pass*
- **The abort fires from the middle of that chain** and the sound stops.
  *must-pass*
- **Nothing transmits that the operator did not click**, asserted across the whole
  chain rather than at the menu alone. *must-pass*

**This is the last thing that can be proved without a radio.** Everything after it
is Tim.

**Depends on:** steps A and B.

---

## Step D - the drive level his radio wants

**Delivers:** a transmit level set for his station.

**Entry:** step C. **Tim's, at the shack.**

**Exit:**
- Tim sets the Transmit drive control and reads the dBFS and clip count under the
  waterfall. *must-pass*
- **His radio's ALC behaviour at that level, in his words.** *must-pass*
- The value recorded in `SHACK_FACTS.md`, so no later unit promises to defer it
  again. *must-pass*

**No unit can perform this.** The one number nobody in the repository can know.

**Depends on:** step C.

---

## Step E - Tim works a station

**Delivers:** a contact.

**Entry:** step D.

**Exit:**
- **He answers a CQ on 14.074 or 7.074 and completes an exchange.** *must-pass*
- The transmitted slots appear in telemetry and the row reads complete.
  *must-pass*
- **What he saw, and anything that surprised him**, recorded. *must-pass*

**Depends on:** step D.

---

## What is not in this phase

- **Automatic sequencing.** *If I get a response, send `73`* - deliberately out.
  A later ruling once the single shot has been watched on a real band.
- **Logging.** FG-004 is named in Settings and is its own work.
- **FT4, PSK31, WSPR transmit**, and **CW send**.
- **The OSD re-encoding count**, `ReusableWindow`, `ProcessDelayForTests`, the
  tap's owner, the waterfall's first row, unit 237's Extensible conclusion, work
  instruction 231's four tree items, `validate-output.bat`'s permitted-spellings
  bug, the 101.33 ms pulse above 6 kHz, the CW decoder and its inherited reds.

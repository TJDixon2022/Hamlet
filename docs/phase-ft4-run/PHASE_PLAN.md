# PHASE_PLAN.md

**Governed by `PHASE_CONTROL.md`. Approved by Tim, 2026-09-08.**

---

## The phase

**FT4 works exactly the way FT8 does.**

## The description

Hamlet decodes FT8 off the air, answers a station, logs the contact and counts it
toward a rank. **FT4 is a button on the Digital tab that does nothing.**

**Most of the work is already done and this phase exists to prove that rather than
assume it.** FT4 shares FT8's 77-bit payload, its LDPC(174,91) code and its CRC-14 -
Franke and Taylor published both in the same QEX paper the port already cites. What
differs is the modulation and the clock: **7.5-second slots against 15, four tones
against eight, 4.48 seconds of transmission against 12.64**, and a different sync
pattern. It buys twice the rate for roughly 3.5 dB less sensitivity.

**Everything above the decoder is already mode-neutral or nearly so**: the panel, the
conversation, the turn ring, the filters, the tooltips, the contact ledger, the
right-click menu, the log, the achievements, and the whole transmit chain built in the
send phase.

**One thing is not, and it was found by unit 287 rather than assumed here.** FT4 is an
ADIF **submode** - `MODE=MFSK, SUBMODE=FT4` - and `AdifContact` has no `SUBMODE` field.
**`MODE=FT4` is invalid ADIF.** Until the log carries a submode, an FT4 contact either
does not log or logs as something it is not, and a log record is the one artefact in
this project that outlives everything else.

**A session reading this cold should understand:** the operator's ruling is *exactly
the way FT8 does*, so anything FT8 does that FT4 does not is a gap rather than a
choice; the port's byte-fidelity to upstream is the instrument every sensitivity
measurement leaned on and is not to be spent casually; and **the closing step is a
contact Tim makes on FT4, which no unit can perform for him.**

---

## The two rules this phase runs under

**1. A bench step's criteria are all satisfiable on a machine with no radio.** A bench
step never defers a criterion to Tim. If one turns out to need the radio, **it moves
to a shack step and the move is recorded.**

**2. A step has at most four criteria.** Few enough that one unit can close it.

**Both were learned the expensive way.** Under the first send plan, four steps sat
`partial` for eight units because each carried a criterion no bench machine could
satisfy.

---

## This phase runs unattended to the bench steps' end

**Steps 0 to 4 need no radio and nothing from Tim.** Steps 5 and 6 are Tim at his own
radio, they are last, and nothing before them is blocked by them.

### The steps are a hypothesis, not a contract

The arbiter may, without asking, recording the evidence in `PHASE_OUTCOME.md`:
**reorder** steps that depend only on their stated entry; **replace** a step with a
better approach; **retire** one measurement shows cannot pay for itself, closing it
*unachievable* with the number; **add** a step the phase needs; **move a target**
found to have been measured wrong; and **move a criterion to a shack step** when only
the radio can answer it.

### Named alternatives to stopping, ruled in advance

| If | Do not stop. Instead |
|---|---|
| a criterion needs the radio | move it to step 5 or 6, record the move, close the bench step on the rest |
| a target is not reached | close the step with the figure reached and what was tried |
| an approach fails | abandon it, record its cost, take another |
| the tree disagrees with this plan | the tree wins. Report the mismatch and continue |
| a licence, naming or scope question arises | **already ruled below.** Do not raise it |
| the shell refuses a call | use the file-editing tools |

---

## The three things the arbiter may not reason past

1. **The abort.** Every path that keys the transmitter has a same-thread, no-await
   abort - CI-V `0x17` with `0xFF`, PTT off as the fallback. Built and proven by units
   257 and 263. **FT4 transmits through it or it does not transmit.**
2. **One click, one transmission.** Hamlet transmits because the operator clicked.
   **Never on a timer, never on a decode, never to continue a contact.** FT4's slots
   are half as long and the temptation is twice as strong.
3. **What Hamlet asserts to the operator.** §0.0 and §12.1. **A log record naming a
   mode the contact was not made in is wrong for as long as the log exists.**

---

## Rulings in force

**Not to be re-argued by any unit.**

**FT4 works exactly the way FT8 does.** Tim, 2026-09-08. **Anything FT8 does that FT4
does not is a gap to be named**, not a scope decision a unit may make.

**Where FT4's decoder lives is decided by what upstream does, and step 0 finds out.**
If `ft8_lib` carries FT4, **the port carries FT4** and the fidelity tests extend to
cover it. If it does not, **FT4 is new work and belongs in `Ft8Sharp.Deep` or a
sibling**, because the port's value is that it cannot drift from upstream. **This is a
reading of the tree, not a preference**, and no unit may decide it from memory.

**`Ft8Sharp` remains a faithful MIT port.** `Ft8Sharp.Deep` is GPL-3.0.

**No algorithm comes from WSJT-X's source.** Published description only - the QEX
paper the port already cites - noted at the point of use.

**A wrong decode is counted separately from a missed one, everywhere.**

**The dummy load is withdrawn.** Tim operates a licensed station on an antenna.

---

## What a unit runs

**A unit runs no test suite.** **Only the unit tests it constructs or rewrites in that
work instruction**, filtered by exact name, foregrounded, with a stated timeout.
**An unfiltered `dotnet test` on any project is forbidden.**

**Never background a command and poll for it.** The watchdog fires after twelve
minutes with no status write.

`dotnet build` is allowed, foregrounded, with a timeout.

**A unit may not add a test without naming the breakage it would have caught.**

Known reds, inherited, **never chased**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list tripwire;
`HM-OPEN-088`'s ten.

---

## Step 0 - where FT4 lives, decided by reading

**Delivers:** the answer to where the decoder goes, from the tree rather than from
memory.

**Entry:** none.

**Exit:**
- **What `ft8_lib` actually carries**, read in `C:\Source\ft8_lib` or wherever the
  vendored copy is - FT4's constants, its sync, its symbol timing. **Report what is
  there, with file and line.** *must-pass*
- **What `Ft8Sharp` already shares with FT4**: the message layer, the LDPC code, the
  CRC-14, and anything else. **A list, so nothing is written twice.** *must-pass*
- **The decision, with its reason**: port or sibling, following the rule above.
  *must-pass*
- **What the 51 fidelity tests would have to become** if FT4 enters the port.
  *must-pass*

**Nothing is built in this step.**

**Depends on:** nothing.

---

## Step 1 - FT4 decodes a signal Hamlet made

**Delivers:** an FT4 decoder, proved against Hamlet's own encoder.

**Entry:** step 0.

**Exit:**
- **A message becomes FT4 symbols becomes audio and decodes back to the same
  message**, over at least a hundred messages including compound callsigns, grids,
  reports and `RR73`. *must-pass*
- **The timing is FT4's**: 7.5-second slots, 4.48 seconds of transmission, four tones.
  **Measured, not asserted.** *must-pass*
- **Zero wrong decodes** across the run. *must-pass*
- A sensitivity ladder at stated SNRs, with its trial count and its wrong count.
  *nice-to-pass*

**The receive path is its own oracle**, as it was for FT8: if Hamlet cannot read its
own FT4 transmission, nothing after this is worth building.

**Depends on:** step 0.

---

## Step 2 - the slot machinery is FT4's

**Delivers:** capture, cutting and the turn clock on a 7.5-second grid.

**Entry:** step 1.

**Exit:**
- **Slots are cut on 7.5-second boundaries** from corrected UTC, and a capture's
  sidecar says which grid it used. *must-pass*
- **The turn ring counts down 7.5 seconds**, and whose turn it is derives from what
  the other station sent on that grid. *must-pass*
- **A slot the operator transmitted in says so** and reports no search result, as
  unit 282 built for FT8. *must-pass*
- **Nothing assumes fifteen seconds anywhere**, and the report names what it found
  that did. *must-pass*

**The half-length slot is where FT8's assumptions will be hiding.**

**Depends on:** step 1.

---

## Step 3 - the log can say FT4

**Delivers:** `SUBMODE`, and an FT4 contact that logs correctly.

**Entry:** none. Independent of steps 1 and 2.

**Exit:**
- **`AdifContact` carries `SUBMODE`**, written and read, round-tripping like every
  other field. *must-pass*
- **An FT4 contact logs as `MODE=MFSK, SUBMODE=FT4`**, cited against the ADIF
  specification the log already cites. *must-pass*
- **A record with no submode still round-trips**, absent rather than empty. *must-pass*
- **The achievements screen's FT4 row can light**, and unit 287's four states still
  read correctly. *must-pass*

**Unit 287 found this and it is a prerequisite rather than a nicety.** `MODE=FT4` is
invalid ADIF and a contact logged that way is wrong for as long as the log exists.

**Depends on:** nothing.

---

## Step 4 - the FT4 button works

**Delivers:** the Digital tab's FT4 button doing what the FT8 button does.

**Entry:** steps 1, 2 and 3.

**Exit:**
- **Pressing FT4 tunes to the band's FT4 frequency** and decodes, through the same
  path the FT8 button uses. *must-pass*
- **The panel, the conversation, the ring, the filters, the tooltips, the ledger and
  the right-click menu all work unchanged**, and the report names anything that did
  not. *must-pass*
- **One click, one transmission**, through the same abort. *must-pass*
- **A whole exchange runs from one right click at the bench**, with the transmit
  endpoint on a loopback, as unit 256 proved for FT8. *must-pass*

**Anything FT8 does that FT4 does not is a gap.** Tim's ruling.

**Depends on:** steps 1, 2 and 3.

---

## Step 5 - Tim hears FT4

**Delivers:** FT4 decoded off the air.

**Entry:** step 4. **Tim's, at the shack.**

**Exit:**
- He tunes to an FT4 frequency and **reads decoded text**. *must-pass*
- **What the sidecar says** about arrival, the slot grid and the decoder. *must-pass*

**Depends on:** step 4.

---

## Step 6 - Tim works a station on FT4

**Delivers:** a contact.

**Entry:** step 5.

**Exit:**
- **He answers a CQ on FT4 and completes an exchange.** *must-pass*
- **It logs as `MODE=MFSK, SUBMODE=FT4`** and lights the achievements row. *must-pass*
- **What he saw, and anything that surprised him**, recorded. *must-pass*

**The half-length slot means half the time to decide**, and whether that is workable
with a right-click is a thing only he can say.

**Depends on:** step 5.

---

## What is not in this phase

- **PSK31, WSPR and Voice.** PSK31 shares nothing with FT8 and wants a different
  panel; WSPR is a beacon with no contacts; Voice has no path at all.
- **A WSPR achievement measured from spots.** On the asks queue and Tim's.
- **Automatic sequencing.** Still out, and FT4's shorter slots are the argument for
  it, which is why it stays a ruling rather than a temptation.
- **The whole asks queue** carried since unit 271.
- **`Ft8Sharp`'s sensitivity.** The 1.2 dB and everything in `Ft8Sharp.Deep` belong to
  the closed sensitivity phase.

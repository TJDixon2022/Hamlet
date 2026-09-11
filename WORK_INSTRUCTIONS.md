# Work instruction 317 - not authored: step 4 waits on one ruling

**Authored by the arbiter from `PHASE_PLAN.md` and `PHASE_OUTCOME.md`, and measured against
the tree at `2983c02`.** Steps 0 to 3 are `done` (units 312, 314, 315, 316; step 3's state was
returned by the separate session that read unit 316's report). Step 4 is next, and **its
delivery needs a ruling that is Tim's**. So this file authors no unit.

**A session handed this file runs no task, changes nothing and commits nothing.** It checks the
gate below, writes `output.md` saying *work instruction 317 is a stop and authors no unit*, and
stops.

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

## 1. Why there is no unit

**The number today: 0 of §R2's four macros can go on the air, and 2 of the 4 are too long for
the proved transmit chain to accept.** No PSK31 modulator exists. The one door, `SendMessage`,
refuses PSK31 by name (`CanTransmitIn`, `MainWindowViewModel.cs:1951-1954`, called at
`:12561`).

```
PHASE GOAL: Hamlet works PSK31 the way it works FT8 - the same two cards, the same
            one-click exchange, the same log and the same achievements, on a modem
            Hamlet builds itself.
UNIT GOAL:  none - the step 4 unit is not authored until Tim rules on the transmit chain.
ADVANCES:   none - no unit runs. This clears the blocker in front of step 4: sending the
            four macros through the proved chain, and that chain still reaching Played
            unchanged.
```

**A, in the arbiter's words.** A third digital mode that is Hamlet's own, on a modem this
project writes. It gets the same two cards, the same one click, the same log and the same
achievements as FT8. Steps 0 to 3 built the ear and the reading: every PSK31 station in the
passband is a row, and each row says who is speaking, to whom, and whether Hamlet is sure.

**B, in the arbiter's words.** Step 4 gives the mode its voice. A modulator sends §R2's four
macros, and loopback through Hamlet's own demodulator brings each one back identical. The
signal's bandwidth is measured. The macros go out through **the proved transmit chain, one
click one transmission**. The CQ receipt and the conversation cards come to the PSK31 panel,
and a turn indicator takes the slot clock's place. A macro is offered only when the parser is
certain whose turn it is. Drive and power are shown on the panel. **The plan's premise, in §2:**
*"Nothing in this phase changes what keys the transmitter; step 4 adds a second audio generator
behind the same proved path."*

**The loop test** on *PSK31 modulator loopback and macro send through the proved transmit chain*
found it in no entry. **No approach on step 4 has been tried. This is not a loop and not a
declaration that step 4 cannot be done.** It is the stop that `PHASE_PLAN.md` §6 names for
this exact case: *"Anything would touch the transmit chain, the keying path, or `Played`.
`MOVE: stop`. Risk posture is the owner's."*

### What the tree says about the chain

The plan's premise is that the proved path takes audio and does not care what made it. **Read
at `2983c02`, it is built around slots from one end to the other:**

- **`OperatorSend`** (`Ft8TransmitSequence.cs:118-151`) carries an **`Ft8Transmission`**. That
  record holds an FT8 message type, a read-back and a hashed-callsign flag
  (`Ft8Composer.cs:95-102`). It also carries **a slot start, a start offset into the slot and a
  `SlotGrid`**.
- **`Sendable`** (`Ft8TransmitSequence.cs:595-652`) refuses as `RefusedAsUnsendable` any audio
  longer than the slot minus the offset. The app arms every send at
  `StartSecondsIntoSlot = 0.5` (`MainWindowViewModel.cs:12718`). So FT8 allows **14.5 s** of
  audio, and FT4 allows 7.0 s.
- **`Ft8ArmedSend.AtBoundaryAsync`** (`Ft8ArmedSend.cs:426-491`) fires only **when a slot
  boundary arrives**. It is driven from the slot tick (`DriveTheArmedSend` at
  `MainWindowViewModel.cs:10560`) and throws away a send whose boundary has already passed.
- **`SendMessage`** runs `Ft8ReadBack.Check` on the FT8 record before it arms
  (`:12626`). It books the receipt against the next boundary (`:12655`). It builds the
  `OperatorSend` from the tab's slot grid (`:12657-12697`).
- **The record** is `ft8_transmission` (`TransmitRecord.cs:70`), written with the slot start
  and the FT8 message type.

**How long §R2's macros are.** This is the arbiter's own arithmetic, not a measurement, and a
unit would check it. Each character is its varicode length plus the two-bit gap, taken from
`data/psk31/varicode.csv`, at 31.25 baud, with `W1AW` as the other station and §R2's double
spaces kept. No idle carrier before or after is counted:

| Macro | Bits | Seconds | Fits the 14.5 s FT8 allows? |
|---|---|---|---|
| CQ | 309 | 9.9 | yes |
| Answer | 196 | 6.3 | yes |
| Report | 726 | **23.2** | **no** |
| Confirm | 459 | **14.7** | **no**, and further over once any idle is added |

A longer callsign on the other side makes both longer. Every file in `assets\fixtures\manifest.json`
agrees on the order of size: the QSO fixtures carry 255 characters in 66.56 s.

**So Report and Confirm cannot reach `Played` through the chain as it stands.** A transmission
at 100% duty cycle does not belong on a 15-second slot. PSK31 has no boundary to wait for, and
a reply that waits for one is not how the mode is worked. Every honest route changes
`OperatorSend`, `Sendable` or `Ft8ArmedSend`. The one route that changes none of them writes a
slot that does not exist into every transmission record.

---

## 2. The ruling wanted - for Tim, and one word answers it

**Question 1. May step 4 change the proved transmit chain so it carries a PSK31 transmission
that has no slot?**

- **A - one sequence, made to take a send with no slot. The arbiter recommends this.**
  `OperatorSend` carries either a slot, as today, or *now*. `Sendable` keeps the slot fit
  for sends with a slot, and a send with no slot is held to a stated maximum length instead,
  so a continuous carrier cannot run on. `Arm` stays the only way in, and the operator's click
  fires a send with no slot rather than a boundary. **The gate, the one `PttOn` use site, the
  `finally` that unkeys or aborts, and `StopNow` stay one code path**, shared by all three
  modes. FT8 and FT4 must stay byte-identical, proved by the tests that guard them today. The
  record says which mode went out and carries no slot where there was none.
- **B - a separate PSK31 sequence beside the FT8 one.** *Rejected by the arbiter:* it adds a
  second `PttOn` use site and a second abort path. `Ft8TransmitSequence`'s own remarks and
  §0.2 keep keying in one place precisely so nothing can key around it.
- **C - a made-up PSK31 "slot grid" long enough for the longest macro, leaving the chain's
  code untouched.** *Rejected by the arbiter:* every record would carry a slot that does not
  exist, and a CQ would wait for an invented boundary. That is §0.0, a guess presented as a
  fact, in the one record this project treats as evidence.
- **D - shorten §R2's macros to fit 14.5 s.** *Rejected by the arbiter:* the Report would lose
  its name, QTH and grid, which is §R2's exchange overruled from underneath. Confirm would
  still be marginal once idle is added.
- **E - split a long macro across several slots.** *Rejected by the arbiter:* one click would
  become several keyings, which breaks §0.2's one operator action, one transmission.

**Question 2. Until question 1 is answered, may the arbiter author the half of step 4 that does
not touch the chain?** That half is the modulator, loopback of the four macros, the measured
bandwidth, the turn indicator, the rule that a macro is offered only on certainty, and drive
and power shown on the panel. The send door keeps refusing PSK31 throughout. *The arbiter
recommends yes.* Either answer to question 1 needs every piece of it, and none of it keys
anything. **It is not authored now because `ARBITER.md` §6 says not to author around an
owner's question.**

*Why this is Tim's and not the arbiter's:* `CLAUDE.md` §0.2 makes the abort and the one-click
rule absolute. `PHASE_PLAN.md` §6 names any touch to the transmit chain, the keying path or
`Played` as `MOVE: stop`. And a carrier at full duty cycle for twenty-odd seconds through a
path proved for 12.64 s is a change to risk posture.

**Not asked, because it is already ruled:** §R4's half-power default uses a write Hamlet
already has, `CivWrites.RfPower` (`CivWrites.cs:271-273`, tier `Transmitted`). It adds no new
kind of radio write.

---

## 3. Logged, not chased

**Unit 316's section 4,** as the next authored unit should carry it:

1. **`TheReadinessHoverTests.NoRowCarriesABearingInDegrees` is red when run first.** Hand wanted:
   run it alone at `eb67036`. **It is on `docs\carry-forward-tests.txt`**, so the next unit's
   before-count may start with one red that is on no known-red list. The next instruction
   names it as expected.
2. The replaced assertion in `ThePsk31HearsEveryoneTests`. A ruling is wanted; it does not block
   step 4.
3. The words `guess` and `unknown`. They are a session's wording, and a ruling is wanted.
4. **Whether §R3 should recognise a roger (`R`, `RR`, `QSL`).** It bears on step 4's turn
   indicator and its nice-to-pass, not on a must-pass.
5. **The split rule's three edges.** A bare `73` does not split; a damaged sign does not split;
   a final `K` counts only when the next character arrives. These bear on the turn indicator
   changing within one character, a nice-to-pass.
6. The callsign pattern is held in four places. Low priority.

**Asks 1 to 24** stand as unit 316 carried them. **Ask 1**, whether the transmission record
asks the radio if it keyed, grows sharper under question 1: a record for a send with no slot
that did not key would be twenty seconds of silence with nothing to say so.

**The reload's disagreements.** `RULES_AT` reports `CPS-DEC-0161` in `CLAUDE.md` §1. Unit 316
found no such id and `HM-DEC-161` at line 360, so it is the reload's reading, not the tree. The
uncommitted `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md` and `.run-unit\*` are the
launcher's. The untracked `docs\phase-ft4-run\PHASE_OUTCOME.md` belongs to ask 19.

---

```
ARBITER-DECISION
STEP: 4
APPROACH: none authored - stop for an owner ruling on carrying an unslotted PSK31 transmission through the slot-shaped FT8 transmit sequence and armed send
MOVE: stop
WHY: Step 4 sends the four macros through the proved chain, but that chain is built around slots - OperatorSend carries an Ft8Transmission and a slot, Sendable refuses audio over 14.5 s, Ft8ArmedSend fires only at a boundary - and the Report (about 23.2 s) and Confirm (about 14.7 s) macros do not fit, so every honest route changes the chain, which PHASE_PLAN.md section 6 reserves to Tim. The question: may step 4 change the chain so one sequence carries a PSK31 send with no slot (option A in section 2, recommended, or B to E, or none), and until then may the arbiter author the half of step 4 that does not touch the chain, with the send door still refusing PSK31 (recommended: yes)?
STATE: not started
DECIDED: two on the arbiter's authority. That step 4's delivery cannot reach Played without changing OperatorSend, Sendable or Ft8ArmedSend, measured from the tree at 2983c02 with the macro lengths computed from data/psk31/varicode.csv and marked as arithmetic rather than measurement. And that the half of step 4 which does not touch the chain is held back, not authored around the question, and offered to Tim as question 2.
LICENCE: PHASE_PLAN.md section 6 (anything touching the transmit chain, the keying path or Played is MOVE: stop; risk posture is the owner's) and section 2 (nothing in this phase changes what keys the transmitter); ARBITER.md section 6 (stop, put the question in the block, do not author around it); CLAUDE.md section 0.2
ACCOMPLISHED: nothing is built tonight; Tim gets one measured question with a recommendation - may the one transmit sequence learn to send a PSK31 over that has no slot - and one yes or no on starting the modulator while he decides
ADVANCES: none - this unit clears a blocker: the conflict between step 4's four macros through the proved chain and the chain to Played staying unchanged, which the tree at 2983c02 cannot satisfy for the Report and Confirm macros
END-ARBITER-DECISION
```

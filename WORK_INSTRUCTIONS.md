# Work instruction 323 - say it: PSK31 goes out

**Seed for the relaunch under `--seed`.** Step 4 is `blocked` in the record on two rulings
that are now in `PHASE_PLAN.md` as §R11 and §R12. **This unit is the press half of step 4
and the eyes-on-the-prize unit (§R14): when it is done, Tim presses CQ under PSK31 and a
PSK31 signal leaves the radio.** The arbiter authors step 5 after it.

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

## 1. The two rules that killed sessions

Both HM-DEC-155, Tim, 2026-09-05.

**1. A unit runs no test suite.** Only this unit's own test names and
`docs\carry-forward-tests.txt`, filtered by exact name, foregrounded, with a stated
timeout. Known reds are in section 10 and never on the list.

**2. Never background a command and poll it.** The watchdog fires at **twelve minutes with
no status write.**

---

## 2. The tool fact

**The shell breaks on an apostrophe inside a quoted heredoc and collapses a doubled
backslash.** Write *do not*; write single backslashes; check what landed on disk.
**Compound commands joined by `;` are refused; run them one at a time.**

---

## 3. Asks still outstanding

Carried per HM-DEC-139, from unit 322's queue. **Every item comes back in section 4,
verbatim where unresolved.** Closed by ruling tonight, report as closed:

- **Item 43** - drive and power. **Closed by §R11.**
- **Item 45** - the `NothingOnTheCardTransmits` pins. **Closed by §R12.** Task 1 rewrites it.
- **Items 32 and 33** - the manual pages. **Closed**; recorded in `docs\psk31-reference.md`.

Still open and touched here:

- **Ask 1** - does the record say what the radio did. `psk31_radio_after_send` exists with
  no production call site. **This unit gives it one.**
- **Unit 322's item 1** - `psk31_lock` is a proxy. Rename it to what it measures
  (`psk31_reading`) in passing; do not build a real lock.
- **Unit 322's item 3** - the record does not say what was on screen. Not this unit's.
- **Off-air audio** - still none. Raised, not stopped for.

All others as unit 322 carried them.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet works PSK31 the way it works FT8.
UNIT GOAL:  Step 4, the press half. Pressing CQ under PSK31 sends the CQ macro
            once on a clear spot through the one sequence, and leaves a receipt.
            Clicking a station calling CQ sends the answer. The report and the
            confirm are offered when the parser is certain it is Tim's turn.
            Power is offered at half; the operator sets nothing at the radio.
ADVANCES:   Step 4 to done, if the remaining must-pass land.
DRIFT:      2 carried from unit 322. Expect 0.
```

**Tim, 2026-09-11:** *"Eyes on the prize. I want to see PSK31 coming and transmitting. Make
it happen."* And on what this application is for: *"I don't know anything about the radio.
I've had six years of failure. I'm writing this app for people who are the same way."*

**What exists** (units 318-322): the modulator; the four macros of §R2 composed and proved
by loopback; the unslotted `OperatorSend` under §R10 with its cap; FT8 and FT4 byte-identical;
the certainty gate; the turn indicator; the PSK31 conversation card; every transmit event
of unit 322 proved at the bench with no production call site. **The door is shut by
`CanTransmitIn` and by a test that says the door is shut.**

**What this unit does:** opens the door, on the ruling that says how. Nothing else.

---

## 5. Verify this instruction against the tree

**Names below come from units 318-322's reports.** Check; **report every mismatch; do not
repair this instruction; do not stop over a mismatch** unless a task is impossible.

- `PHASE_PLAN.md` at the root carries §R11 through §R14. **If it does not, stop and say
  so** - the plan delivery did not land.
- `ThePsk31ConversationCardTests.NothingOnTheCardTransmits` and its two pins: no `NowAsync`
  under `src\Hamlet.App`; `CanTransmitIn` exactly `null`, `FT8`, `FT4`.
- `CanTransmitIn` (`MainWindowViewModel.cs` about `:1951`), `SendMessage` (about `:12561`),
  the bolt unit 320 built, `UnslottedTransmission`, the cap and its number,
  `Ft8ContactLedger.CallToAnyone`, the CQ receipt (unit 310, no Log by R8), the PSK31
  conversation card (unit 319), the certainty gate, the turn indicator.
- The clear-spot search unit 320 did not build; `Psk31CarrierSearch`, whose candidate
  list is the input to it.
- `CivWrites.RfPower` (tier `Transmitted`, offered), `RigField.Alc`, the rig poll.
- The `psk31_send_*` and `psk31_radio_after_send` events and their tests (unit 322).
- Tests: `ThePsk31ModulatorTests`, `TheUnslottedSendTests`,
  `TheFt8AndFt4SendsAreByteIdenticalTests`, `ThePsk31OfferTests`, `ThePsk31TurnTests`,
  `ThePsk31ConversationCardTests`, `ThePsk31TabIsInertTests`, `ThePressingOfCqTests`,
  `TheCqReceiptTests`, `ThePsk31TransmitTelemetryTests`.

---

## 6. Rulings in force

**`PHASE_PLAN.md` §R10** - the one sequence carries a send with no slot, capped; one
`PttOn` site, one unkey path, one `StopNow`; FT8 and FT4 byte-identical.
**§R11** - the operator sets nothing at the radio; power offered at half; ALC in plain
words during a send. **§R12** - a session rewrites its own tests to guard the rule and not
the shut door; never asks the owner. **§R13** - telemetry must-pass. **§R14** - tests prove
criteria and nothing beyond. **§R1** - a macro is offered only on certainty. **§R2** - the
four macros, as written. **§R6** - a CQ goes out on a clear spot Hamlet finds, not a fixed
offset. **§R8** - the receipt carries no Log.

**Tim, 2026-09-11, R1-R6 on cards**: one receipt, refreshed on repeat press, last call
only, no count; retired when anyone certainly answers; two answers make two cards and
nobody inherits; conversation cards unlimited.

**§0.2** - one click, one transmission. **The one rule that is Tim's: Hamlet never
transmits without the operator's click.** Every test touched here guards that.

**§0.0 / HM-DEC-092**, **HM-DEC-084** (a radio setting is offered, never mirrored as a
control), **HM-DEC-018** and **§2.1** (nothing personal in the record), **HM-DEC-155**,
**HM-DEC-139**, **FACT-004**, **FACT-006**, **the dummy load withdrawn in full**.

---

## 7. Status cadence

`PROJECT_STATUS.md` per `CLAUDE.md` §13: **after every task, and at least every ten
minutes.** Write the status **before** starting a test run.

---

## 8. The tasks

Five. Each names the test to watch failing first and one drop candidate. **Drop from the
back.**

### Task 1 - the guard says what it is for, and the door has a handle

**1a.** Append `UNIT 323` to `PHASE_OUTCOME.md` under step 4. Patch-bump the version. Run
`docs\carry-forward-tests.txt` filtered before anything changes; report the count.

**1b. Rewrite `NothingOnTheCardTransmits` under §R12, in its own commit.** What it guards
after: **nothing on a PSK31 card, and nothing on the PSK31 panel, transmits without the
operator's click**; the only path to the radio is `SendMessage` behind the same bolt FT8
uses; a card's commands compose, they do not key. **The text-scan pins come out.** The
test stays on the carry-forward list under its name.

**1c. `CanTransmitIn` answers true for PSK31** through the same predicate as FT8 and FT4.
The bolt unit 320 built - *only drive and power can open* - is replaced by §R11: the drive
is what FT8 composes at, power is offered at half, and **nothing is required of the
operator** for the door to be open. Report what the bolt was and what it became.

**Test watched failing first:** the rewritten `NothingOnTheCardTransmits`, green against
the tree with the door open. `ThePsk31TabIsInertTests` is **retired** by this task - its
premise ends here - and its two assertions that still hold (no FT8 decoder under PSK31, no
slot grid) move into `ThePsk31PanelSpeaksPsk31Tests`. Say so.

**Drop candidate:** none. **Not droppable.**

---

### Task 2 - the press: CQ under PSK31 sends once and leaves a receipt

**The clear spot.** Before a CQ, take `Psk31CarrierSearch`'s current candidates and choose
an offset in the passband at least **150 Hz** from every held carrier and every candidate
over quality 0.4, preferring the middle of the widest gap. **State the rule in one place.**
If no such spot exists, **refuse in plain words** on the panel - *the band is too crowded
here to call without landing on someone* - and write `psk31_send_refused` with reason
`no_clear_spot`.

**The send.** One press: the CQ macro of §R2 composed at that offset, an unslotted
`OperatorSend` under §R10, through `SendMessage`, one keying, `Played`. **A second press
refreshes the receipt to the new time and sends again; it does not stack.**

**The receipt.** `Calling`, the sentence, the time, dismiss - **no Log, no count, no
station facts** (R2, R4, R8). Retired when **a certain answer** arrives - the parser says a
station is calling Tim and is sure - and that station gets a conversation card. **Two
certain answers, two cards, no receipt.** A guessed answer retires nothing.

**Telemetry (§R13):** `psk31_send_composed`, `send_stage` with `slotted: false`,
`psk31_send_keyed`, `psk31_send_unkeyed`, the record, and `psk31_radio_after_send` from the
next rig poll - **all now firing from production**, proved by the bench run.

**Test watched failing first:** `ThePsk31CqGoesOutTests`, app. Watch it fail, then green:

1. one press composes one CQ at a clear spot, arms one unslotted send, reaches `Played`
   once, and writes the six events in order
2. a second press refreshes the receipt and sends once more; the panel never holds two
   receipts
3. a certain answer retires the receipt and opens that station's card; a guessed one does
   not; two certain answers make two cards
4. with every gap under 150 Hz, the press refuses with the sentence and the event, and
   nothing reaches the sequence
5. `TheFt8AndFt4SendsAreByteIdenticalTests` and `TheUnslottedSendTests` still green,
   unedited

**Drop candidate:** the widest-gap preference. Keep the 150 Hz rule; take the first
clear spot.

---

### Task 3 - answering, and the rest of the exchange

**Clicking a station calling CQ** sends the Answer macro of §R2 at **that station's
offset**, once, and opens his conversation card with the turn indicator at *his turn*.

**On his card, when the parser is certain it is Tim's turn** (§R1 strict side), the next
macro is offered as one button - Report after his answer, Confirm after his report - and
one click sends it. **When the parser is not certain, no button is offered and the card
says why in plain words**: *waiting to be sure it is your turn*.

**73 closes it.** After Confirm has gone out and his 73 or SK is certain, the card shows
the exchange complete and the Log option - the Log lives here, on his card, never on the
receipt.

**Test watched failing first:** `ThePsk31ExchangeTests`, app. Watch it fail, then green:

1. clicking a CQ row sends one Answer at that offset and opens the card at *his turn*
2. a certain report from him offers Report; an uncertain line offers nothing and says so
3. Report goes out once per click; Confirm is offered only after his certain report
4. his certain 73 marks the exchange complete and shows Log
5. no macro is ever sent without a click - assert over the whole scripted exchange that
   the number of keyings equals the number of clicks

**Drop candidate:** assertion 4's Log. Keep the completion state.

---

### Task 4 - power offered at half, and the ALC in plain words

**§R11.** On the PSK31 panel, beside the drive: **the power offer**, defaulting to half the
radio's range, shown as a percentage, **offered** through `CivWrites.RfPower` the way
HM-DEC-084 says - accepted with one click, never written silently. **Nothing about the USB
MOD Level appears anywhere on the screen.**

**During a PSK31 send**, read `RigField.Alc` from the poll. If it is past the zone, the
panel says so **in a sentence for someone who has never seen an ALC meter**: what happened
and the one thing to do about it - turn the drive down a step. Write the reading with its
age (HM-DEC-111) in `psk31_send_keyed`'s successor event or its own.

**On this machine there is no radio.** The offer renders and its write is proved to reach
`CivWrites` and stop there; the ALC sentence is proved from a fed reading. Say so.

**Test watched failing first:** `ThePowerIsOfferedTests`, app. Watch it fail, then green:
the offer renders at half; accepting it makes exactly one `RfPower` write; declining makes
none; no control on the panel mirrors USB MOD Level; a fed ALC past the zone yields the
sentence and the event, and one inside the zone yields neither.

**Drop candidate:** the ALC sentence. Keep the offer; report ALC as ask.

---

### Task 5 - stand it up and press it

**Drop candidate: this whole task.** But do not drop it lightly - it is the one that
answers Tim's sentence.

With the app running on this machine, no radio: press PSK31, press CQ. Report **in plain
words what happened on the screen and in the record**, event by event, from the press to
`Played` to `psk31_radio_after_send` saying the radio did not answer. Then feed the
four-signal fixture, click the row calling CQ, and report the same for the Answer.

**Test watched failing first:** none; this task reports.
**Drop candidate:** the whole task.

---

## 9. Parked

- **Step 5** - RST in the log, ADIF, the achievements. The arbiter authors it next.
- **Real off-air audio.** Raised, not stopped for.
- **The card-rebuild root**, **card ordering**, **what was on screen** (unit 322 item 3).
- **A real bit-clock lock.** The event is renamed, not rebuilt.
- **Any second `PttOn` site, any second sequence, any invented slot.** §R10.
- **Any package.**

---

## 10. What not to do

- **No unfiltered `dotnet test`.** **Never background and poll.**
- **Do not add a second `PttOn` site or a second transmit sequence.**
- **Do not send without a click, and do not write a test that would let a send happen
  without one.**
- **Do not put a USB MOD Level control on the screen, or ask the operator to set one.**
- **Do not write RF power silently.** Offer it.
- **Do not ask the owner to approve a test rewrite.** §R12.
- **Do not add tests beyond what a criterion needs.** §R14.
- **Do not put a callsign, a grid or text in any event.**
- **Do not edit `PHASE_PLAN.md`, `PHASE_STATUS.md` or `PHASE_OUTCOME.md`** beyond the
  outcome append.
- **Do not chase these known reds:** `CwAdjudicationTests.ASpeedChangeInRealisticAudio`;
  the 51 CW cases in `docs\unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests`
  whole-type-list tripwire; `HM-OPEN-088`'s ten; the two in `TheAchievementsScreenTests`; the
  one in `TheFitGuardAsksAboutTheGridTheSendIsOnTests`; unit 320's item 46.
- **Do not repair this instruction.** Report mismatches; keep working.

---

## 11. Committing and pushing

Commit per task - task 1b in its own commit. Push at the end. Nothing left uncommitted.

---

## 12. Reporting

`output.md` at the repository root. **Canonical headings:** `## 1. What Claude did`,
`## 2. What the owner should expect`, `## 3. What you should see`,
`## 4. What's blocking us`.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet works PSK31 the way it works FT8. Steps 0 to 3 done,
   4 <state after this unit>, 5 and 6 not started.
B. Step 4 and its must-pass - each met or not met, with the number.
C. The report last, and section 4 raises N items on top of the carried queue.
```

```
UNIT:       323 - <complete|stopped> at task N of 5, <which dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no, and which step>
NUMBER:     macros that can go on the air 0 -> <n> of 4
DRIFT:      <n> consecutive units without advance  (was 2)
```

**Section 2 must say, in plain words for someone who knows nothing about the radio, what
happens when Tim presses CQ under PSK31 tonight, what he will see on the screen, what the
power offer is and that he need touch nothing at the radio, and what the one sentence
about ALC means if it ever appears.**

**Every appearance claim is computed, not seen. Say so once.**

---

```
ARBITER-DECISION
STEP: 4
APPROACH: rewrite the shut-door guard to guard the click rule, open CanTransmitIn for PSK31 through the same bolt as FT8, send the CQ macro once on a clear spot through the unslotted sequence, answer and offer the remaining macros on certainty, and offer power at half with the ALC read into a sentence
MOVE: continue
WHY: the two rulings that blocked step 4 are in the plan as R11 and R12; everything else step 4 needs was built by units 318 to 322 and is green; only the press half remains, and the owner has said what the measure of the unit is
STATE: blocked
DECIDED: the clear-spot rule (150 Hz from any held carrier or candidate over 0.4, widest gap preferred) is the unit's number to state; ThePsk31TabIsInertTests is retired and its surviving assertions rehomed, because its premise ends when the door opens
LICENCE: PHASE_PLAN.md R10, R11, R12, R13, R14, R1, R2, R6, R8; Tim 2026-09-11 on cards R1-R6; CLAUDE.md 0.2; HM-DEC-084
ACCOMPLISHED: Tim presses CQ under PSK31 and a PSK31 signal leaves the radio, once, on a clear spot, with a receipt that says so and nothing asked of him at the radio
ADVANCES: step 4 - the receipt and cards criterion, the power criterion, the certainty gate having a send behind it, and the transmit events having production call sites
END-ARBITER-DECISION
```

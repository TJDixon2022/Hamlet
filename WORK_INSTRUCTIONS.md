# Work instruction 362 - the FT8 send that stops after read-back, and the rule that it never happens again

**Single session. Transmit regression. Ahead of everything.** Nothing in the Olivia
phase runs until this is fixed. **Four tasks.**

**Tim, 2026-09-19, a guiding principle just below the prime directive:** *"We can't break
data modes that already work. FT8 and FT4 were solid. Now they're broken. Breaking
something and claiming success is not success."* This unit repairs the first case and
writes the rule where it is enforced.

**Status.** `tools/status.sh`, real clock, after every commit and every task.

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

## 1. The rules that killed sessions

**HM-DEC-155.** No suite; only this unit's names and `docs\carry-forward-tests.txt` as its
top comment says. Never background and poll.

## 2. The tool fact

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm`
is refused; Python cannot run here; `-m` more than once for a multi-line commit.

## 3. Asks still outstanding

Carried per HM-DEC-139 from the last report's queue, **verbatim in section 4**.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet works Olivia the way it works PSK31.  (paused for this)
UNIT GOAL:  An FT8 send reaches the air again, and a send that does not reach
            it says why in the record. Nothing else.
ADVANCES:   none - clears a blocker: FT8 sends stop after read-back on 1.13.48
            with no arm, no keying, no record and no refusal.
DRIFT:      carried.
```

**Tim, 2026-09-19:** *"FT8 is no longer transmitting. It seems to be queued but never
sent."*

**The record, `%AppData%\Hamlet\telemetry\2026-09-19.jsonl`, session `82dc251a`, version
1.13.48, 14.074:**

```
15:10:43.949  operator_action  send_requested  Digital  16 characters
15:10:43.949  send_stage       composed        Ft8
15:10:43.957  send_stage       read_back       Standard
              ... nothing ...
15:11:29.124  operator_action  send_requested  Digital  16 characters
15:11:29.124  send_stage       composed        Ft8
15:11:29.128  send_stage       read_back       Standard
              ... nothing ...
```

On every prior FT8 send in the record the stages continue: `armed`, the boundary,
`keyed`, `played`, and an `ft8_transmission` record. Here they end at `read_back`, and
**nothing says why** - no `send_refused`, no event of any kind. A silent drop after
read-back, twice, forty-five seconds apart.

**The suspects, in order.** Units 359 and 360 changed the send path to compose an RSID
burst in front of keyboard-mode sends and to write `announced` into the transmission
record; unit 360 also made the burst sit outside the cap. Those units' tests assert FT8
and FT4 audio byte-identical - which says nothing about whether an FT8 send is still
*armed*. Something between read-back and arming now takes a path an FT8 send does not
return from. Second suspect: the `announced` field on the record, or the cap check with
the burst's 2.3 s, applied to a slotted send that has no burst.

---

## 5. Verify this instruction against the tree

- The send path from `SendMessage` through read-back to `Ft8ArmedSend.Arm` and
  `AtBoundaryAsync`, as units 318, 323, 359 and 360 left it; every branch between
  `read_back` and `armed`; where `announced` is composed and where the burst is added;
  where the cap is checked and with what length.
- `send_stage` and `send_refused` events and where each is written; **every early
  return between read-back and arming that writes nothing.**
- The fixture that plays an FT8 send to `Played` at the bench
  (`ThePressingOfCqTests`, `TheUnslottedSendTests`, `TheFt8AndFt4SendsAreByteIdenticalTests`).

**Report every mismatch; repair nothing but the fault.**

## 6. Rulings in force

**§0.2** one click, one transmission - and a click that does not transmit says so.
**R10** one sequence, one `PttOn` site, the abort. **R13** every stage writes its event;
**a silent early return is a telemetry fault as well as a transmit fault.** **R32** the
burst is outside the cap - for keyboard-mode sends only. **HM-DEC-155**, **HM-DEC-139**,
**FACT-004**, **FACT-006**, **the dummy load withdrawn.**

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - reproduce at the bench

Append `UNIT 362` to `PHASE_OUTCOME.md` as carried repair, `ADVANCED: blocker`.
Patch-bump. Run the carry-forward list.

Then: stand the app up on FT8 at the bench and press CQ. **Does the send reach
`armed`?** Trace it stage by stage against the record above, and find, with file and
line, **where an FT8 send leaves the path after read-back and why**. If the bench does
not reproduce it, the difference between the bench and Tim's radio session is the
finding - say what it is.

**Drop candidate:** none.

### Task 1 - the send reaches the air, and a send that does not says why

Fix the fault. Then: **every return between `read_back` and `armed` that does not arm
writes `send_refused` with a reason**, so this can never be silent again.

**Test watched failing first:** `TheSendReachesTheAirTests`, app: an FT8 CQ at the bench
passes `composed`, `read_back`, `armed` and reaches `Played` at the boundary, with the
record written; the same for FT4; a PSK31 send still carries its burst and its
`announced` field; every early return between read-back and arming writes
`send_refused`, asserted by driving each one; `ThePressingOfCqTests`,
`TheUnslottedSendTests`, `TheFt8AndFt4SendsAreByteIdenticalTests` green.

**Drop candidate:** none.

### Task 2 - the carry-forward list guards every working mode

The test from task 1 goes on `docs\carry-forward-tests.txt`, and beside it **one guard
per working mode, permanently**: a send reaches `Played` at the bench and a fixture
decodes - FT8, FT4, PSK31 now; Olivia when it has them. Name the tests, from the
existing ones where they exist (`ThePressingOfCqTests`, `ThePsk31CqGoesOutTests`,
`ThePsk31DemodulatorTests` on the clean fixture, the FT4 equivalents). Every unit runs
the list before and after; **a red after is a regression, named as such in section 1
and section 4.**

**Drop candidate:** none.

### Task 3 - the rule, written where it is enforced

**`DECISIONS.md`**, as Tim's ruling of 2026-09-19 with the next `HM-DEC-` number, the
sentence above and this: *no unit is complete, whatever its own criteria say, if a mode
that reached the air before it does not reach the air after it, or a mode that read the
air before it reads less; the judge marks it partial at best, and the next unit's first
task is the repair before any new criterion.*

**`PHASE_PLAN.md` §6**, as the first line ahead of the three stops, citing that number.
The same line into `docs\phase-psk31-run\PHASE_PLAN.md` and `docs\phase-screen-run\PHASE_PLAN.md`
for the record, marked as added after the fact.

**Drop candidate:** none.

---

## 9. Parked

- **The Olivia phase.** Resumes after this.
- **Anything else.**

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Do not touch the Olivia demodulator, the RSID detector, or the plan.**
- **Do not remove the RSID prefix from PSK31 sends.** Fix the FT8 path beside it.
- **No package. Report mismatches; repair only the fault. Write American.**

## 11. Committing and pushing

Commit per task; push at the end.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet works Olivia the way it works PSK31. Paused; no step
   moves.
B. No criterion changes state; this clears a transmit blocker on FT8.
C. The report last, and section 4 raises N items on top of the carried queue.
```

```
UNIT:       362 - <complete|stopped> at task N of 4 - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   blocker
NUMBER:     FT8 sends reaching Played at the bench 0 -> 1; silent returns after
            read-back <n> -> 0; mode guards on the carry-forward list 0 -> 3
DRIFT:      carried
```

**Section 2 tells Tim in one paragraph why his CQ stopped and that it goes out now.**

---

```
ARBITER-DECISION
STEP: 2
APPROACH: trace an FT8 send from read-back to arming at the bench, fix the early return that drops it, make every non-arming return write send_refused, put a guard per working mode on the carry-forward list, and write the no-regression rule into DECISIONS.md and the plan
MOVE: continue
WHY: the owner's record shows two FT8 sends stopping after read-back with no arm, no record and no refusal on the build that added the RSID prefix; transmit on the mode he uses is broken and silent
STATE: not started
DECIDED: nothing beyond the fault
LICENCE: CLAUDE.md 0.2; PSK31 plan R10, R13; Olivia plan R32
ACCOMPLISHED: FT8 goes out again, no send can stop without saying why, and a mode that works stays working by rule and by test
ADVANCES: none - clears a blocker: FT8 sends silent after read-back on 1.13.48
END-ARBITER-DECISION
```

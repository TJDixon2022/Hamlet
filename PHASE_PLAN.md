PHASE: Hamlet holds what it has
PHASE_SET: 2026-09-20
DESCRIPTION: A hardening phase, run unattended while Tim is away. Everything banked in the PSK31 and Olivia threads that is screen, record or test and needs neither the radio nor the owner. Judged by tests that ran and, at the end, by Tim at his window.
STEP: 0 | Settings survive an upgrade, and a send that cannot go names the true fault - no device chosen is its own refusal with a Settings link; a device that will not open carries its name, rate and OS error.
STEP: 1 | Hamlet opens whole - at the size it opens at and every size measured by unit 354, CQ, the mode tabs, the send area and Stop are on the window; the panels give up height before the send area ever does.
STEP: 2 | The record says what was true - the send press records the mode it was pressed under; every RSID code fldigi knows is in the data file with its tone sequence; the two headless flakes are made deterministic or named as environment and quarantined off the carry-forward list.
STEP: 3 | The record says what was on screen - for every decoded row and every card, whether it was drawn, filtered, scrolled away or folded, so an empty-looking screen can be diagnosed from the file without a screenshot.
STEP: 4 | The radio sheet - one page Tim reads at the radio for PSK31 and Olivia: what to press, what he should see at each step, what each refusal sentence means, where the capture goes, and what to send back if it fails.
STEP: 5 | Tim looks - at his window and, when he has time, at the radio, and says it passed.

---

# The hardening phase - the reasoning under the step list

**Set 2026-09-20 by the web thread on Tim's instruction:** *"Is there anything we can
start working on as I'll be gone for many hours? I'd like to continue progress."* The
Olivia phase halted correctly at 38 of 40 with only the owner's criteria left; it is
archived at `docs/phase-olivia-run/` with 5.4 and 6.1 open, to be closed by Tim.

## §1 What this phase is

Five steps of work that need no ruling and no radio, each already discussed and ruled
in conversation between 2026-09-11 and 2026-09-19, and one step of Tim looking. Nothing
here touches a decoder, a modulator or what keys, except step 0's refusal text at the
arm stage - which changes what is *said* about a send that does not go, never what goes.

## §2 What is the same

Every ruling of the PSK31, screen and Olivia plans stands (their `PHASE_PLAN.md` files
under `docs/phase-*-run/`). The ones leaned on: **R11** nothing at the radio; **R12** a
session rewrites its own tests; **R13** telemetry on every stage; **R14** nothing beyond
the criterion; **R19** American; **R31** unattended - criteria by id, a done step closed,
the owner's step ends the run, two rulings a unit; **§6** three stops only, the later
ruling wins. **HM-DEC-018, §2.1** nothing personal in an event. **§0.0** a sentence on the
screen is a claim; a refusal says the true reason.

## §R Rulings from the threads, applied here

**R33 - Tim, 2026-09-19: the settings loss.** *"The settings lost the listing setting."*
The transmit device setting was lost between 1.13.30 and 1.13.48 and FT8 could not send
for five days; unit 362's refusal line found it. A saved setting outlives an upgrade; a
send with no device chosen says so and opens Settings.

**R34 - Tim, 2026-09-14: Hamlet opens with every control on the window.** At 1100×780,
the size it opens at, CQ and the mode tabs were below the window (units 354, 355). The
send area never leaves the window; the panels give up height first, then the top row.

**R35 - the record's small lies.** `cq_pressed` writes `detail: Ft8` under PSK31 (seen
2026-09-11 and 09-14). Codes 72-75 have no tone sequence in `rsid-codes.json`; the
sequences are now in `assets/data/rsid-codes.json` from the same ported encoder, with
the `Squares` and `indices` tables themselves. Two app tests flake in headless runs
(`TheStopIsAlwaysOnScreenTests`, `TheTestsStayOffTheNetworkTests`, unit 365's report):
a flaking test on the carry-forward list is worse than none.

**R36 - Tim, 2026-09-11: telemetry rich enough to diagnose any issue.** Unit 322's item 3,
never taken: the record says a line was parsed but not whether the row was visible,
filtered, scrolled away or in a folded panel. That gap is why the CQ-filter fault of unit
337 needed a screenshot to find. Close it.

**R37 - the radio sheet.** Tim: *"I don't know anything about the radio."* Unit 349's sheet
was for the screen; there is none for the radio. One page, in his words, for the two
keyboard modes.

## §4 The steps

Exit criteria carry ids `N.k`; met is `[x]`. A step's exit is its own assertions plus
`docs/carry-forward-tests.txt` run as its comment says.

## Step 0 - Settings survive, and a refusal names the fault

**Delivers:** R33.

**Entry:** `PHASE_STATUS.md` names this phase; `TheSendReachesTheAirTests` green, run first.

**Exit:**
- [ ] 0.1 The commit and line between 1.13.30 and 1.13.48 that dropped the transmit device on load are named in the report. *must-pass*
- [ ] 0.2 Settings files reconstructed from the tree at 1.13.30, 1.13.40 and HEAD each load with every value intact - transmit device, receive device, grid, callsign, license class, power offer, ALC reference - a missing new field takes its default, nothing present is dropped, and the file round-trips. *must-pass*
- [ ] 0.3 At arm time, no transmit device chosen yields `send_refused reason no_transmit_device` and the sentence *No transmit device is chosen. Open Settings and pick the radio's sound card*, with Settings opened from the sentence. *must-pass*
- [ ] 0.4 A chosen device that will not open yields `transmit_device_would_not_open` carrying the device name, the rate asked and the OS error text, in the event and on the panel. *must-pass*
- [ ] 0.5 `TheSendReachesTheAirTests`, `TheUnslottedSendTests` and the byte-identical tests green and unedited. *must-pass*

**Depends on:** nothing.

## Step 1 - Hamlet opens whole

**Delivers:** R34.

**Entry:** step 0 done; unit 354's nine-size test present and run first.

**Exit:**
- [ ] 1.1 At 1100×780 and at each of unit 354's nine sizes, CQ, the mode tabs, the send area, the drive note and Stop are inside the window, asserted by measuring. *must-pass*
- [ ] 1.2 The rule is stated and holds: when the window is too short, the working panels give up height first, then the top row; the send area's height is constant across sizes and it is never the thing that leaves. *must-pass*
- [ ] 1.3 Below the sum of the minimums, the panels scroll inside themselves and the send area stays put. *must-pass*
- [ ] 1.4 `BindingHealthTests` and the sheet's layout tests green. *must-pass*

**Depends on:** step 0.

## Step 2 - The record says what was true

**Delivers:** R35.

**Entry:** step 1 done.

**Exit:**
- [ ] 2.1 `cq_pressed` and every send event carry the sub-mode the press was made under - PSK31, Olivia, FT8, FT4 - never a mapped family. *must-pass*
- [ ] 2.2 `data/rsid/rsid-codes.json` is replaced by `assets/data/rsid-codes.json`, which carries tone sequences for codes 72-75 and the two tables; the detector reads all of them; a test asserts every listed code round-trips through Hamlet's generator and detector. *must-pass*
- [ ] 2.3 `TheStopIsAlwaysOnScreenTests` and `TheTestsStayOffTheNetworkTests` are run ten times each; either made deterministic with the cause named, or quarantined into a named non-carry-forward list with the environmental cause stated. No flaking test remains on `docs/carry-forward-tests.txt`. *must-pass*
- [ ] 2.4 The carry-forward list runs green five times in a row. *must-pass*

**Depends on:** step 1.

## Step 3 - The record says what was on screen

**Delivers:** R36.

**Entry:** step 2 done.

**Exit:**
- [ ] 3.1 Every decoded row writes one event when its visibility changes: drawn, filtered out (by which filter), scrolled out of view, in a folded panel, or removed - with the row's offset or slot and no callsign or text. *must-pass*
- [ ] 3.2 Every card writes the same on appear, fold, scroll-out and dismiss. *must-pass*
- [ ] 3.3 From the four-signal PSK31 fixture with the CQ filter on, the record alone says which rows were hidden and by what - the unit 337 fault reproduced and diagnosed from the file with no screenshot. *must-pass*
- [ ] 3.4 The events are sampled so that a busy FT8 evening adds under 50 kB an hour, measured. *must-pass*
- [ ] 3.5 The privacy scan is green over every new event. *must-pass*

**Depends on:** step 2.

## Step 4 - The radio sheet

**Delivers:** R37. `docs/RADIO_SHEET.md`, one page, for PSK31 and Olivia.

**Entry:** step 3 done.

**Exit:**
- [ ] 4.1 For each mode: what to press and in what order; what should appear at each step, in the words the screen uses; what each refusal sentence means and what to do; where a capture goes and what to copy where; what to send back if it fails. Every sentence quoted from the tree, not paraphrased. *must-pass*
- [ ] 4.2 A test asserts every quoted sentence exists in an operator-facing string. *must-pass*
- [ ] 4.3 No sentence tells the operator to touch the radio (R11). *must-pass*

**Depends on:** step 3.

## Step 5 - Tim looks

**Delivers:** Tim's verdict at his window, and at the radio when he has time.

**Entry:** step 4 done.

**Exit:**
- [ ] 5.1 Tim says it passed. No script can evaluate this. *must-pass*

**Depends on:** step 4.

## §5 Dependencies

Step 0 depends on nothing; steps 1-5 are one pipeline.

## §6 Branching

- **Three stops only**: keying, transmit or the radio's safety; money past the budget; a
  fact the product states to the operator about the radio, a contact or a send. A hint,
  a label, a number, a layout, a test's shape, a mechanism arithmetic will not allow:
  decide, mark author's, continue.
- **The later ruling wins. A done step is closed. Every remaining step Tim's: halt.**
- **A must-pass missed by a little**: ship, report, `partial`, move on. Never loosen a test.
- **A flake cannot be made deterministic**: quarantine it with its cause named; that is
  meeting 2.3, not missing it.
- **Anything would change what goes on the air, or what keys.** `MOVE: stop`.
- **A package is needed.** `MOVE: stop`.
- **A file must be deleted.** Empty it, comment it, list it.

## §7 Carried

Every open ask of the Olivia phase from unit 368's queue; the PSK31 demodulator on
real air, waiting on a capture; Olivia 5.4 and 6.1; real flags on country cards; rank
names; the map bitmap's license; the id-scheme split.

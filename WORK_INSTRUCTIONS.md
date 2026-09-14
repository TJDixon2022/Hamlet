# Work instruction 355 - Stop is always on screen, and what was heard stays

**Seed under `--seed`, then the loop continues to step 3.** Five tasks. Nothing here
touches a decoder or the transmit chain's behavior; task 1 moves where a control lives.

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

Carried per HM-DEC-139 from unit 354's queue, **verbatim in section 4**. Answered here:
**354's item 1** - the Stop control below the window - **ruled A by Tim, 2026-09-14**,
task 1. **354's finding on callook.info** - task 4.

---

## 4. Why this unit exists

```
PHASE GOAL: The screen, done right.
UNIT GOAL:  The transmit abort is on screen at every window size; a PSK31
            station's words stay on the list after he stops; the tests stop
            depending on the network; the sheet Tim reads matches the tree.
ADVANCES:   Step 3's ground - the sheet - and step 0's safety at small sizes.
DRIFT:      carried.
```

**Tim, 2026-09-14, ruling A on the Stop control:** *"Stop lives in the status bar,
always"* - the bar at the bottom of the window that never scrolls or collapses; visible
at every size; enabled only while something is keyed. Unit 354 measured the main window
at nine sizes and found Stop below the window at 1100×780.

**Tim, 2026-09-14, on PSK31 rows:** *"I've seen a few PSK31 phrases. In the past, they
disappear. There's no record of them."* True: a PSK31 row belongs to its carrier and is
retired with it, text and all (unit 324's retire rule). An FT8 message stays until
cleared. **A PSK31 row's text stays too.**

**Unit 354's finding:** the plain test fixture's view model asks callook.info for
KC3QIS's license class at construction, and the answer changes what the layout tests
measure - General lands or not depending on the network. Tests that depend on a web
lookup are not tests.

---

## 5. Verify this instruction against the tree

- The Stop control: where it lives on the PSK31 and FT8 panels, what enables it, the
  path from it to `StopNow` - **that path is not changed, only the control's parent.**
- The status bar at the bottom of the window: the tray mark, the count badge, its
  height, what else is on it.
- The PSK31 row: its lifecycle, the retire rule (`SignalGone`, `ListeningStopped`), what
  removes it from the list, how FT8 rows persist and are cleared.
- The view model's constructor and the callook.info lookup; the license-class field and
  what reads it; the test fixture that constructs the view model.
- `docs/` for the sheet unit 350 wrote (name it); `TheStopIsOnScreenTests` if 354 wrote
  one; `TheRowShowsWhatWasHeardTests`; `ThePsk31StationIdlesTests`; the nine-size layout
  test from 354.

**Report every mismatch; repair nothing but this unit's.**

## 6. Rulings in force

**Screen phase R26 and its rulings; §6 three stops only.** **PSK31 plan §0.2** one click,
one transmission - **the abort is the other half of that rule and is never off screen.**
**R9**, **R12**, **R13**, **R14**, **R19**. **§0.0** the sheet says what the tree draws.
**HM-DEC-155**, **HM-DEC-139**, **FACT-004**, **FACT-006**.

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 1 - Stop lives in the status bar

Append `UNIT 355` to `PHASE_OUTCOME.md` under step 3; patch-bump; run the carry-forward
list. Then: the Stop control moves to the status bar, right of center, the height of the
bar, **enabled only while a send is keyed and disabled otherwise**, its command the same
`StopNow` path as today - **the path is not touched, proved by the tests that guard it.**
It is present at every window size 354 measured, including 1100×780. The old Stop on the
panels goes.

**Test watched failing first:** `TheStopIsAlwaysOnScreenTests`, app: at each of 354's
nine sizes the Stop control is inside the window; it is disabled with nothing keyed and
enabled during a fed send; pressing it reaches `StopNow` exactly as before;
`TheUnslottedSendTests` and `TheFt8AndFt4SendsAreByteIdenticalTests` green and unedited;
`BindingHealthTests`.

**Drop candidate:** none.

### Task 2 - a PSK31 station's words stay

When a carrier is retired, **its row stays in the decoded list** with everything it
showed, marked *ended* in a word and in the row's state, dimmed to the worked-fade
opacity, in time order with the FT8 messages, until `clear` or the list's own cap
removes it - the same rule FT8 rows live by. A carrier that reappears at the same offset
within the retire window is the same row, resumed, not a new one.

**Telemetry:** `psk31_row_ended` (offset, characters, lines, lifetime) when the row is
kept after retire; `psk31_row_cleared` when the list drops it.

**Test watched failing first:** `ThePsk31RowStaysTests`, app: the four-signal fixture
leaves four ended rows on the list with their text after every carrier is gone; `clear`
removes them; a station returning at his offset within the window resumes his row; the
list's cap applies to ended rows as to FT8 rows; `TheRowShowsWhatWasHeardTests` and
`ThePsk31StationIdlesTests` green.

**Drop candidate:** the resume-at-the-same-offset case.

### Task 3 - an ended row is still a station

An ended PSK31 row keeps its quill, its country and its fade, and **if he was calling CQ,
clicking his row still sends the Answer on his offset** - he may be listening. The card
opens at *his turn* as it does for a live row.

**Test watched failing first:** extend `ThePsk31ExchangeTests` by one: clicking an ended
CQ row sends one Answer at his offset and opens his card.

**Drop candidate:** the whole task.

### Task 4 - the tests stay off the network

The view model's constructor takes its license lookup through a seam - an interface with
the live callook.info client behind it in the app and a fixed answer in tests. The test
fixture supplies *General* for KC3QIS explicitly. **No test in the tree makes a network
call**; assert it by running the plain fixture with the network denied.

**Test watched failing first:** `TheTestsStayOffTheNetworkTests`: the fixture constructs
with the seam's fixed answer; the layout tests 354 wrote measure the same numbers on two
consecutive runs; no HTTP client is created under test.

**Drop candidate:** the two-run stability assertion.

### Task 5 - the sheet matches the tree

Unit 350's sheet - the one Tim reads at his window for step 3 - is updated for tasks 1
and 2: where Stop is and when it is enabled; that PSK31 rows stay; the nine sizes with
Stop present at each. Every number on it comes from a test that ran this session.

**Test watched failing first:** none new; the sheet's own check, if 353 gave it one.

**Drop candidate:** the whole task; say so and Tim reads the old sheet with this report.

---

## 9. Parked

- **The demodulator on real air.** The unit after a capture exists in
  `assets\fixtures\captured\`. Nothing else in it.
- **Anything touching a decoder's behavior or the transmit chain's. Any package.**

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Do not change what `StopNow` does.** Move the control; keep the path.
- **Do not drop a PSK31 row's text when its carrier ends.**
- **Do not let a test reach the network.**
- **Do not touch the demodulator. No package. Report mismatches; repair nothing but this
  unit's. Write American.**

## 11. Committing and pushing

Commit per task; push at the end.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

```
READ IN THIS ORDER.

A. The phase goal - the screen, done right. Steps 0 to 2 done, 3 waits on Tim.
B. Step 3's ground - the sheet - and step 0's safety at small sizes; no criterion
   changes state.
C. The report last, and section 4 raises N items on top of the carried queue.
```

```
UNIT:       355 - <complete|stopped> at task N of 5, <which dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   no
NUMBER:     window sizes with Stop on screen 5 of 9 -> 9 of 9; PSK31 rows kept after
            the carrier ends 0 -> all
DRIFT:      carried
```

**Section 2 tells Tim where Stop is now and that PSK31 text stays. Every appearance
claim is computed, not seen.**

---

```
ARBITER-DECISION
STEP: 3
APPROACH: move the Stop control to the always-visible status bar without touching its path, keep a PSK31 row and its text on the list after its carrier ends, let an ended CQ row still be answered, put the license lookup behind a seam so tests stay off the network, and update the sheet Tim reads
MOVE: continue
WHY: Tim ruled A on the Stop control found off-screen at small sizes, and ruled that PSK31 text must not vanish; the network lookup makes the layout tests unstable; all of it is the ground step 3 is judged from
STATE: blocked
DECIDED: Stop's exact place on the bar and the ended row's word are the unit's; the retire window for resuming a row is the unit's number to state
LICENCE: screen phase R26 and section 6; PSK31 plan 0.2, R9, R12, R13, R14; Tim 2026-09-14
ACCOMPLISHED: Tim can always stop a transmission from any window size, and what a PSK31 station said stays on his screen after the station stops
ADVANCES: step 3's ground
END-ARBITER-DECISION
```

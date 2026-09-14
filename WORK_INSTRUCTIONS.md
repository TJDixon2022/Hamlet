# Work instruction 356 - the window opens whole, and the ALC sentence tells the truth

**Single session.** Four small things from the last three reports and Tim's screen.
Nothing touches a decoder or what keys.

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

Carried per HM-DEC-139 from unit 355's queue, **verbatim in section 4**. Answered here:
**355 item 1** - Stop always pressable - **ruled A by Tim, 2026-09-14**: *"A, the way it
has been"*; nothing to build, task 0 records it. **355 item 3** and **354 item 2** - CQ
and the tabs below the window at 1100×780 - task 1. **355 item 2** - idle HTTP clients
under test - task 3.

---

## 4. Why this unit exists

```
PHASE GOAL: The screen, done right.
UNIT GOAL:  Hamlet opens with every control on the window; the ALC sentence
            after a send says what it measured and never sends Tim to a meter;
            no test constructs a network client.
ADVANCES:   none - clears a blocker: unit 354 item 2 and unit 355 item 3, CQ
            and the mode tabs below the window at the size Hamlet opens at.
DRIFT:      carried.
```

**The window.** Hamlet opens at 1100×780. At that size, after unit 355, Stop is on the
status bar but **CQ and the mode tabs are at y 800-830 - below the window's bottom
edge** (355 item 3, traced). A new operator opens the app and cannot see the button that
calls CQ. Unit 354 measured the layout as holding only at 1040 tall and above.

**The ALC sentence.** Tim's screen, 2026-09-14, after an FT8 send to HA1BF:

> *Your radio's own level control read 62 out of 120 while that went out. Hamlet has
> not yet seen an FT8 or FT4 transmission on this radio to compare it with, so it is not
> judging it for you: look at the ALC bar on the radio, and if it goes past the marked
> zone, turn the transmit drive above down one step and send again.*

Two faults in one sentence. **It was an FT8 transmission** - the sentence is about it -
so under R15 the 62 it just read *is* the reference, and the sentence is computed before
the reference is stored. And **it tells Tim to read a meter and turn a knob**, which R11
forbids outright: *the operator sets nothing at the radio*. The whole message is four
lines where one fact belongs. Tim: *"this message is incorrect."*

**The tests.** 355 found `MainWindowViewModel.BuildSources` constructs the POTA and SOTA
spot sources with their own `HttpClient` at construction, on or off. The fixtures switch
them off so nothing is sent, but *no client is created under test* was the criterion
and it does not hold.

---

## 5. Verify this instruction against the tree

- The send area: CQ, the mode tabs, the drive note, where they sit in the layout and
  what gives the top row and panels their heights at 1100×780 (unit 354's nine-size
  fixture, `Realized` height overload).
- The ALC path: `15 13` read during a send (unit 324), the reference learned from FT8
  (unit 325, R15), where the reference is stored and when, and where the post-send
  sentence is composed - the text above is the current one.
- `MainWindowViewModel.BuildSources` `:7869`, `:7873`; `PotaActivitySource`,
  `SotaActivitySource`; the seam unit 355 added for the license lookup.
- `docs/` for the sheet Tim reads (unit 349's, updated by 355).
- Tests: `TheStopIsAlwaysOnScreenTests`, 354's nine-size test, `TheAlcLearnsFromFt8Tests`,
  `TheTestsStayOffTheNetworkTests`, `VoiceTests`, `BindingHealthTests`.

**Report every mismatch; repair nothing but this unit's.**

## 6. Rulings in force

**Screen phase R26; §6 three stops only.** **PSK31 plan R11** - the operator sets nothing
at the radio, reads no meter; **R15** - the ALC reference is learned from FT8 sends, the
highest reading observed; a PSK31 send above it by the margin gets a sentence; **with no
reference, report and judge nothing**. **Tim, 2026-09-14** - Stop always pressable, A.
**§0.0** a sentence on the screen is a claim. **R12**, **R14**, **R19**. **HM-DEC-155**,
**HM-DEC-139**, **FACT-004**, **FACT-006**.

## 7. Status cadence

As the header says.

---

## 8. The tasks

### Task 0 - the record

Append `UNIT 356` to `PHASE_OUTCOME.md` under step 3, `ADVANCED: no`. Patch-bump. Add
one line to `PHASE_PLAN.md`'s §R block, as Tim's ruling of 2026-09-14: **Stop is always
pressable and never grey.** Run the carry-forward list.

**Drop candidate:** none.

### Task 1 - Hamlet opens with every control on the window

At 1100×780 - and at every size 354 measured - **CQ, the mode tabs and the send area are
inside the window.** The rule R26 states holds where it can; where the window is too
short for the top row's 190 px plus the panels plus the send area, **the panels give up
height first, then the top row, and the send area is never the thing that leaves.** If
the window is shorter than the sum of the minimums, the working panels scroll inside
themselves and the send area stays put. State the minimums.

**Test watched failing first:** extend 354's nine-size test: at every size, CQ, the mode
tabs, Stop and the drive note are inside the window; the send area's height is
constant across sizes; `BindingHealthTests`.

**Drop candidate:** none.

### Task 2 - the ALC sentence

The post-send sentence is composed **after** the reference is updated, and reads, on
the send that sets or raises the reference:

> *Your radio's level control read 62 of 120 during this send. That is Hamlet's
> reference from now on; a PSK31 send that reads well above it will get a sentence here.
> Nothing for you to do.*

On a later send within the reference: *Level 58 of 120, inside the reference. Nothing for
you to do.* On a PSK31 send above the reference by the margin: R11's sentence - what
happened and the one thing to do, **at the drive control on the screen, never at the
radio**. **No sentence anywhere tells the operator to look at a meter or touch the
radio.** One line each; `VoiceTests` runs.

**Test watched failing first:** `TheAlcSentenceTests`, app: the first FT8 send at 62 sets
the reference and the sentence names it and says nothing to do; a later send inside it
gets the inside sentence; a PSK31 send above it gets the drive sentence; the words
*ALC bar*, *marked zone*, *on the radio* appear in no operator-facing string; `TheAlcLearnsFromFt8Tests`
green.

**Drop candidate:** none.

### Task 3 - no client under test

`PotaActivitySource` and `SotaActivitySource` take their `HttpClient` through the same
seam the license lookup uses (unit 355), created on first fetch and never in a
constructor. Under test, the seam supplies nothing and no client exists.

**Test watched failing first:** extend `TheTestsStayOffTheNetworkTests`: no `HttpClient`
is constructed anywhere when the plain fixture builds the view model; the spot sources
still fetch in the app when switched on.

**Drop candidate:** the whole task.

### Task 4 - the sheet

Update Tim's sheet for tasks 1 and 2: the opening size now shows every control; the ALC
sentence's three forms, word for word.

**Drop candidate:** the whole task.

---

## 9. Parked

- **The demodulator on real air** - the unit after a capture exists in
  `assets\fixtures\captured\`.
- **Anything touching a decoder or the transmit chain's behavior. Any package.**

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Do not let the send area leave the window at any size.**
- **Do not write a sentence that sends the operator to the radio.**
- **Do not touch the demodulator or what keys. No package. Report mismatches; repair
  nothing but this unit's. Write American.**

## 11. Committing and pushing

Commit per task; push at the end.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

```
READ IN THIS ORDER.

A. The phase goal - the screen, done right. Steps 0 to 2 done, 3 waits on Tim.
B. No criterion changes state; this unit clears the blocker under 354 item 2.
C. The report last, and section 4 raises N items on top of the carried queue.
```

```
UNIT:       356 - <complete|stopped> at task N of 5, <which dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   no
NUMBER:     sizes with CQ on the window <n> of 9 -> 9 of 9; operator-facing strings
            naming a meter <n> -> 0
DRIFT:      carried
```

**Section 3 prints the three ALC sentences word for word. Every appearance claim is
computed, not seen.**

---

```
ARBITER-DECISION
STEP: 3
APPROACH: keep the send area inside the window at every size by giving up panel and top-row height first, compose the post-send ALC sentence after the reference is stored and never send the operator to the radio, and put the spot sources' clients behind the seam so no test constructs one
MOVE: continue
WHY: Hamlet opens at a size where CQ is below the window, and the ALC sentence on Tim's screen contradicts both R11 and R15; both are on the screen step 3 is judged from
STATE: blocked
DECIDED: the height-giving order (panels, then top row, never the send area) is the unit's stated rule; the three sentence forms are the author's words for the operator
LICENCE: screen phase R26 and section 6; PSK31 plan R11, R15, R12, R14, R19; Tim 2026-09-14 on Stop; CLAUDE.md 0.0
ACCOMPLISHED: a new operator opens Hamlet and sees the button that calls CQ, and after a send is told one true thing and nothing to do
ADVANCES: none - clears a blocker: unit 354 item 2, CQ below the window at the opening size
END-ARBITER-DECISION
```

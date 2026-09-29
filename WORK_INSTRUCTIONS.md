# Work instruction 494 - a button for W1AW on every band it sends on

**Hand run. One small unit.** The owner cannot find CW to test with. W1AW sends Morse on a
published schedule at published frequencies. This unit puts one button per band on the CW tab so
he can go there in a click.

**No test against a recording, a fixture, a floor or copied telemetry** (R96). A headless test
driving synthetic values written in the test itself is allowed; nothing read from disk. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST EXIST:      CW_REQUIREMENTS.md
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all five are not as stated, refuse: reply with only the path you are in,
which checks failed, and "wrong project - nothing done."

If all five hold, say "Hamlet confirmed" and continue.
```

---

## 1. Rules

- Take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end. Write nothing
  to `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`. Tick nothing in `PHASE_PLAN.md`.
- One `dotnet test` invocation per line, filtered, with a `timeout`. Never background and poll.
  The app line loses names to the dispatcher loop; re-run once, count neither way.
- Apostrophes in quoted heredocs break; `;`, `rm` and `git rm` are refused; Python cannot run
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit494-<name>.sh`
  and are not committed.
- **Nothing that keys or transmits.** A button tunes the receiver and sets the mode. It never
  transmits, never changes transmit drive, and never touches the keyer. Follow the same path the
  existing frequency chips use (`14.048 CW`, `14.066 CW` on the neighborhood card), so nothing new
  is written to the radio.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. Why

The owner, 2026-09-29: *"There have been no CW activity for hours."* And: *"Let's do a small work
instruction to add a button for each band that supports W1AW. So when we're in CW mode on the CW
tab, it'll automatically select that. So you can go there."*

**W1AW is the ARRL's headquarters station.** It sends Morse code practice, qualifying runs and CW
bulletins on a fixed schedule, on the same frequencies year round. It is the one signal the owner
can count on: machine-sent, known speed, known content. Every decoder fault this project has
chased shows up plainly on it.

---

## 3. The frequencies, from the ARRL's own schedule

**W1AW Morse code frequencies, in MHz**, from `arrl.org/w1aw-operating-schedule` and the ARRL
bulletin archive:

| band | frequency |
|---|---|
| 160 m | 1.8025 |
| 80 m | 3.5815 |
| 40 m | 7.0475 |
| 20 m | 14.0475 |
| 17 m | 18.0975 |
| 15 m | 21.0675 |
| 10 m | 28.0675 |
| 6 m | 50.350 |
| 2 m | 147.555 |

**The IC-7300 covers 160 m through 6 m** (HF plus 50 MHz), so **2 m is not offered** - the radio
cannot tune it. **Say so in the report** rather than showing a button that cannot work.

**What W1AW sends, for the hover text:** code practice at 5 to 15 WPM (slow) or 10 to 35 WPM
(fast), and CW bulletins at 18 WPM. Times are US Central, the same all year, so **do not compute
or display a schedule** - the times move against UTC with daylight saving and a wrong time on
screen is a false sentence (§0.0). The hover says what it is and that the schedule is the ARRL's,
not that a transmission is happening now.

---

## 4. The change

On the **CW tab**, a row of buttons, one per band above: **`W1AW 40 m`**, **`W1AW 20 m`**, and so
on, each labelled with its band and carrying its frequency in its hover.

- **A press tunes the receiver to that frequency and sets CW**, by the same path the neighborhood
  card's existing frequency chips use. Nothing else changes.
- **A band the operator's licence does not cover on CW is shown but not pressable**, with its
  hover saying why - the same rule the rest of the app follows for privileges. The owner holds a
  General licence; check what the app already knows about his privileges rather than assuming.
- **A band the radio cannot tune is not shown at all.** 2 m, per §3.
- **The row is only on the CW tab**, and it does not move when the mode changes - R101, one
  layout. It must not resize any column or push anything (§0.6 and R101).
- **Every button says what it does on hover** (§0.6), naming the frequency, that W1AW is the
  ARRL's headquarters station, and that it sends code practice and bulletins on the ARRL's
  published schedule.
- **The frequencies live in one place** - a table in the app or in `data\bands\`, wherever the
  existing frequency chips keep theirs - with the ARRL as the source in a comment, so they can be
  corrected without hunting.

**Watch it fail first**, headless:

1. The row shows one button per tunable band, 160 m through 6 m, and **no 2 m button**.
2. Pressing `W1AW 20 m` asks for 14.0475 MHz and CW, and asks for nothing else.
3. A band outside the operator's CW privileges is present and not pressable.
4. The row is on the CW tab and nowhere else, and adding it moves no panel's position or size.

---

## 5. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 494 - STEP 11`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 494.
- Patch-bump `Directory.Build.props`.
- `DECISIONS.md`, newest first, **HM-DEC-199**, headline *W1AW's Morse frequencies are one button
  per band on the CW tab*, naming the ARRL schedule as the source and that 2 m is left out because
  the radio cannot tune it.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling - this is the owner's request, not a
  rule.

---

## 6. Report

Section 2, for the owner, in plain words: rebuild, go to the CW tab, press `W1AW 40 m` or
`W1AW 20 m`, and the radio tunes there in CW. What each band is for: 40 m after dark, 20 m in the
day, 80 m late at night.

Section 1: what changed, file by file, where the frequency table lives, and that the build and the
app line are green. Section 3: the four cases. Section 4: anything left, a line each.

# Work instruction 499 - the band plan says where the modes are

**Hand run. One unit.** The neighborhood map calls 7.0475 MHz a PSK31 frequency. It is W1AW's own
Morse frequency, and the app knows that from its own data file. That one wrong block sent the
radio into USB-D at 3 kHz and filled the CW terminal with junk.

**No test against a recording, a fixture, a floor or copied telemetry** (R96). Verify by building
`Hamlet.sln` with warnings as errors and running the app carry-forward line. **The owner's report
at the radio is the test.**

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
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit499-<name>.sh`
  and are not committed.
- **Nothing that keys or transmits.** This unit changes data the app reads and what it writes to
  the radio's **mode**, which §0.2 allows and §12.4 governs: no value guessed, every write named.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. What happened, and why it is self-inflicted

**The owner's screen, 2026-09-30 00:35 UTC.** He pressed `W1AW on 40 m`. The radio went to
7.0475 MHz. The neighborhood card then read:

> **`40 m  7.048 MHz  Digital · PSK31 · yours to use`**
> *"Your General license covers digital modes here"*
> *"PSK31 lives at 7.070; you are at 7.048"*

and the rig went to **USB-D with FIL1 at 3 kHz**. Hamlet's own send panel said *"The radio is in
USB rather than Morse."*

**The cascade, all from one wrong block:**

1. The map calls 7.0475 a data frequency.
2. Mode-follow therefore wrote the data variant, USB-D, over the CW the button had set.
3. USB-D takes the wide filter, 3 kHz.
4. **The CW detector was handed the whole 3 kHz passband** instead of 500 Hz around one tone. It
   read the tone at 1500 Hz, called 15 marks in four seconds out of everything else in the band,
   and the terminal filled with junk.

**None of that was the decoder.** The owner: *"This was a very avoidable error. It's a
self-inflicted wound."*

**And the app already held the right answer.** `data/bands/w1aw-morse.json` gives 7.0475 as W1AW's
**Morse** frequency. The map's block and the app's own data file disagree, and nothing checks them
against each other.

**What the band plan actually says.** On 40 m the ARRL plan puts **RTTY and data at 7.080 to
7.125**, and **PSK31 at 7.070**. Everything below 7.070 is CW. The card's own line - *"PSK31 lives
at 7.070; you are at 7.048"* - says the frequency is 22 kHz away from PSK31 while calling it PSK31.
**The 40 m data block is drawn about 22 kHz too low**, and 20 m's numbers - where 14.070 genuinely
is PSK31 - look to have been carried down.

---

## 3. The change

### One - the band plan is right, on every band

Find the data the neighborhood map's blocks come from - the file or table that gives each band its
CW, data, PSK31 and phone segments - and **check every band against the ARRL band plan**, not only
40 m.

- **Correct every block that is wrong**, and say in the report, band by band, what each was and
  what it became.
- **Cite the ARRL band plan in the data file** as the source, beside each band, so the next person
  can check it.
- **Do not guess a boundary.** Where the plan gives a range, use the range. Where it is a
  convention rather than a rule - a calling frequency, a watering hole - say so in the data rather
  than drawing it as a block.
- **160 m through 6 m**, every band Hamlet draws.

### Two - the data file and the map may not disagree

A test that **reads `data/bands/w1aw-morse.json` and asserts every Morse frequency in it falls in
a CW block of the map's own data.** Nine rows, nine assertions.

- **Red today on 7.0475.** Report which rows are red before the fix.
- This is the check that would have caught it, and it costs nothing to keep.

### Three - the W1AW button's CW sticks

Unit 495 said the button sets CW and holds mode-follow off. **It did not hold**: the radio was in
USB-D. Find out why - most likely mode-follow fires again when the radio reports the new frequency
back - and make the button's mode survive it.

- After a press, **the radio is in CW and stays in CW** until the operator changes band or mode
  himself.
- **Report what actually overrode it**, by file and line.
- With the map corrected, mode-follow would not fight the button on 7.0475 anyway. **Fix it
  regardless**: the same thing will happen on any Morse frequency near a block edge.

### Four - what must not change

The detector, the marks, the runs, the banking, the narrowness, the edges, the scope, the
terminal, the one layout, the preamp, the buttons. **This unit is data and mode, not CW.**

**Watch it fail first:**

1. The nine-row check, red on 7.0475, green after.
2. At 7.0475 the card reads **Morse**, not Digital or PSK31, and says the licence covers Morse
   there.
3. Pressing `W1AW on 40 m` leaves the radio in CW: one mode write, CW, and **no later write of a
   data variant** when the frequency is reported back.
4. Every band's blocks match the plan, as a table in the test.

---

## 4. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 499 - STEP 11`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 499.
- Patch-bump `Directory.Build.props`.
- `DECISIONS.md`, newest first, **HM-DEC-203**, headline *The band plan is checked against the
  app's own frequencies*, naming the cascade above: one wrong block put the radio in USB-D at
  3 kHz and filled the CW terminal with junk, and the app's own W1AW table held the right answer.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling.

---

## 5. Report

Section 2, for the owner, in plain words:

- rebuild;
- press `W1AW on 40 m`: the card says **Morse**, the radio goes to CW and stays there, and the
  filter is the CW one;
- **then look at the terminal on W1AW** - that is the first honest look at units 496 to 498 on a
  real signal, because until now the detector was being handed 3 kHz of band.

Section 1: what changed, file by file, **the band-by-band table of what each block was and
became**, what overrode the button's CW, and that the build and the app line are green.
Section 3: the four cases. Section 4: anything left, a line each.

# Work instruction 500 - the gaps belong to the sender's own dit

**Hand run. One unit.** Unit 499 put the chain right: CW, the 500 Hz filter, the card saying
Morse. The first honest look at a real signal followed, and the reader cuts every letter into
single marks.

**No test against a recording, a fixture, a floor or copied telemetry** (R96). A headless test
driving synthetic hops written in the test itself is allowed; nothing read from disk. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line. **The
owner's report at the radio is the test.**

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
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit500-<name>.sh`
  and are not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. What the owner saw, and what it says

**W1AW on 7.0475, 2026-09-30 02:09 UTC, and this time the chain is right:** CW, FIL2 at 500 Hz,
the card reading `40 m 7.048 MHz Morse · yours to use`, the button holding, the panel's own line
saying **`Sending now: code practice, slow, until 11:00 PM`**.

**The terminal read:**

```
ST ST TIE TETBA TE TE T TN ETET T TNI ET TI TETB ETE D TNTR T STE E E E E E E E E E I E
E E E E E E T E E E E E E E E E E E E E E E E E E E E E E I E E E E E E E E E T
```

**Almost every character is one or two marks.** That is not noise getting through - noise is what
units 497 and 498 cut from 36 marks a second to 5. **That is the sender's own marks being cut into
single-element letters**: the reader is reading the gaps *inside* a letter as gaps *between*
letters, so every dit and dah becomes its own `E` or `T`.

**Why it shows now and not in any test.** The panel says **slow code practice** - the ARRL sends
that at **5 to 15 WPM**. At 10 WPM a dit is 120 ms and the gap inside a letter is 120 ms; at 5 WPM
they are 240 ms. **Every synthetic case from unit 490 to unit 498 was built between 9 and 23 WPM**,
and the reader derives its character and word gaps from what it has measured. On a genuinely slow
sender those thresholds land short, and every intra-character gap reads as a character gap.

**It also explains why the banking did not save him.** A lone `T` is confirmed by the lone `E` that
follows it inside the window, so they all release. The banking is working; it is being fed letters
that were never letters.

---

## 3. The change - the gaps scale with the sender

In `CwRunReader`:

- **The sender's dit is measured from its own short marks**, and the three gaps follow it as Morse
  defines them: **one dit between elements, three between letters, seven between words.**
- **A gap of about one dit ends nothing.** It is the gap inside a letter, and the marks either side
  belong to the same character.
- **A gap of about three dits ends a letter.** A gap of about seven ends a word.
- **The boundaries between the three are the author's** - the geometric means are the obvious
  choice and unit 490 already uses them for the word gap - stated in the report with their reasons,
  and **derived from Morse's own 1:3:7, not from any recording and not tuned after a result.**
- **Until the sender's dit is known**, the reader must not guess short. Say in the report what it
  does before it has measured a dit, and what that costs on the first letter or two.
- **The dit must track a sender who changes speed**, since code practice runs from 5 to 35 WPM and
  the ARRL steps the speed within a session. Say how, and over how many marks.

**Do not change** the detector, the marks, the narrowness, the edges, unit 496's bin choice, the
banking's rule, the keyed-mark rule, the two-run rule, the scope, the terminal, the layout, the
preamp or the buttons. **This is the reader's gap arithmetic and nothing else.**

---

## 4. What to measure

**Watch it fail first**, with synthetic hops written in the test. **The speeds are the point:**

1. **`CQ CQ DE N0CALL N0CALL K` at 5, 10, 18 and 35 WPM.** Every one must read whole.
   **Red today at 5 and 10** - report what each reads before and after.
2. **`TEST DE W1AW K` at 5 and 10 WPM**, the case unit 498 proved at 23.
3. **A sender that changes speed mid-transmission**, 10 WPM to 20 WPM, as code practice does.
   Report what it reads.
4. **The existing cases stay exactly as they are**: the clean call, the call with bursts, the
   two-station case, the lone dit, the lone dah, the stray dit after the call, and both noise
   tests. **If any changes, say so with its text; do not force it.**

**Report, for each speed, the sender's true dit and the dit the reader measured**, so the owner can
see whether the measurement or the arithmetic was at fault.

---

## 5. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 500 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 500.
- Patch-bump `Directory.Build.props`.
- `DECISIONS.md`, newest first, **HM-DEC-204**, headline *A sender's gaps are its own dit times one,
  three and seven*, naming that every synthetic case had been built between 9 and 23 WPM and that
  W1AW's slow code practice at 5 to 15 WPM was read as single-element letters.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling.

---

## 6. Report

Section 2, for the owner, in plain words:

- rebuild;
- W1AW's slow code practice should read as words rather than as strings of `E` and `T`;
- fast sending is unaffected;
- **if it still comes out as single letters, the dit the reader measures is the thing to look at**,
  and section 3's table names it.

Section 1: what changed, file by file, the three gap boundaries and their reasons, what the reader
does before it knows the dit, and that the build and the app line are green. **Section 3: the four
speeds' text before and after, with the true dit beside the measured dit.** Section 4: anything
left, a line each.

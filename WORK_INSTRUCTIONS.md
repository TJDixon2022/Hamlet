# Work instruction 493 - one decoder, one truth

**Hand run. One unit.** There are two decoders printing to two surfaces. The scroll reads the
run reader and obeys the owner's rule. The terminal reads the old timing-only path and prints
`E` on noise. **This unit makes the terminal read the run reader, and retires the old path from
the screen.**

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
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit493-<name>.sh`
  and are not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. What the owner sees, and why

His screenshot, 2026-09-29 13:35 UTC, 20 m: the panel reads **`no keying · decoding at 352 Hz`**,
the scroll below is **empty** apart from the word *listening*, and the terminal above is **four
lines of `E`s**.

**Both halves are working as built, and that is the bug.**

- **The scroll reads the run reader** that units 490 to 492 built. It obeys R103 and R105: a
  letter needs a run of marks that agree on pitch and level, with a flat top and a dit's
  duration. On noise it finds nothing, so it shows nothing. **That is why he never sees an `E`
  down there.**
- **The terminal reads `CwProbabilisticDecoder`'s original path**, which has never been given a
  mark's pitch or amplitude. It measures how long the key was down in one mixed stream and fits
  letters to timing. Noise over a threshold for 40 ms is an `E`. **`decoding at 352 Hz` is that
  path's mixing pitch**, which unit 492 lists as outstanding.
- Unit 487 had gated the terminal on the blocks. **Unit 489 switched that gate off** on the
  author's recommendation, to get a strong W1AW signal reading again while the detector was
  pointed 50 Hz off the station. Units 490 to 492 fixed that: marks are now placed on the peak of
  their own lobe, pairing is fixed, and delivery is 15 ms rather than 445.

**The owner's ruling, R106, 2026-09-29:** *"I don't want to go back to something. I want to make
it work."* **One decoder, one truth.** The terminal reads the run reader - not the old path with a
gate in front of it. What is in the scroll is what is in the terminal, because they are the same
source.

---

## 3. The change

### One - the terminal reads the run reader

The CW terminal's text comes from `CwRunReader`, the same source the scroll's blocks and letters
come from.

- **A letter appears in the terminal exactly when it appears over the blocks**, and never
  otherwise. No gate, no filter, no second opinion - one source.
- The terminal keeps everything else it has: unit 487's printed-stays-printed, the owner's
  `Clear`, its scroll-back, its colours and its fonts.
- **Word gaps** come from the reader's own word-gap rule (unit 490), so the terminal's spacing is
  the reader's spacing.
- Unit 485's *no detection, no letters* becomes true by construction: there is no other source.

### Two - the old path leaves the screen

`CwProbabilisticDecoder`'s timing-only path **no longer feeds any surface**: not the terminal, not
the leading edge, not the scope, not the capture sheet's transcript.

- **It stays in the tree**, behind unit 489's `ReadsRuns` switch, so it can still be driven by a
  test and compared. **Nothing is deleted** - not the lattice, the speed grid, the unit estimator
  or the emission gate.
- **Unit 489's three switches become dead weight on the screen's path** and the report says so:
  with one source there is nothing left for them to gate. Leave them as they are; do not delete
  them.

### Three - the panel says what it is doing

- **`decoding at N Hz` becomes the printed sender's own pitch**, from the reader. **`no station`
  when the reader is printing nobody.** This is unit 492's outstanding item, and it must be done
  here: the old number is the retired path's and would now be a false sentence on screen (§0.0).
- Unit 491 skipped this for thread safety. **The reader runs on the audio thread and the panel on
  the screen's**, so hand the pitch across the same way the blocks already cross - the scope
  already reads the reader's marks, so follow that path and say how.
- **`tone N Hz heard`** stays as the detector's own, unchanged.

### Four - what must not change

- The detector, the marks, the runs, the two tolerances, the flatness rules, the pairing, unit
  491's nearest-bar change, unit 492's three mark rules and its three-window bound.
- The light, the blocks, the letters over the blocks, the one layout, the preamp, the verdict
  buttons and the verdict row.

**Watch it fail first**, with synthetic hops written in the test, and report each case's terminal
text beside its scroll letters:

1. **The clean call.** Terminal and scroll identical, reading `CQ CQ DE N0CALL N0CALL K`.
2. **The call with bursts in every gap.** Identical, reading the call whole.
3. **Loud noise, no station.** **Terminal empty.** Red today - the old path prints `E`s here, which
   is the owner's complaint - green when nothing prints.
4. **A lone dit and a lone dah.** Terminal empty.
5. **The call plus a second station 200 Hz away.** Terminal and scroll both read the printed
   station, identically.

**In every case, assert the terminal's text equals the scroll's letters.** That equality is the
unit.

---

## 4. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 493 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 493.
- Patch-bump `Directory.Build.props`.
- **Append R106 to the rulings section of both `PHASE_PLAN.md` copies**, in the owner's words
  above: one decoder, one truth; the terminal reads the run reader; the timing-only path leaves
  the screen and stays in the tree. **Touch no checkbox.**
- `DECISIONS.md`, newest first, **HM-DEC-198**, headline *One decoder, one truth: the terminal
  reads the run reader*, naming that two paths fed two surfaces and that the old one had never
  been given a mark's pitch or amplitude.

---

## 5. Report

Section 2, for the owner, in plain words:

- rebuild;
- on noise: an empty scroll **and an empty terminal**;
- on a station: the letters in the terminal are the same letters that sit over the blocks, at the
  same moment, because there is now one decoder;
- the panel says the pitch of the station it is printing, or *no station*;
- **if a real station reads nothing, the scroll will be empty too** - that is the diagnosis, and
  it is the one thing to report back.

Section 1: what changed, file by file, how the pitch crosses threads, and that the build and the
app line are green. **Section 3: the five cases, terminal text beside scroll letters, and that
they are equal in each.** Section 4: anything left, a line each.

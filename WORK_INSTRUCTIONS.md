# Work instruction 597 - The learned reader: a trained net reads the audio

**Run budget: UNIT_RUN_BUDGET.md applies - one survey, one engine line, one app line, at the
end; quick measures per round.** (`UNIT_RUN_BUDGET.md` is in this zip; it is recorded in the
tree by this unit as a standing ruling.)

**Hand run. Put a trained neural CW reader into the engine behind a switch, off by default,
prove the port against the shipped test vector, and measure it on the boards, the wild bank
and the two real hands. Up to three rounds; the rounds fix the port, never the model.**

**Why this unit exists.** Four units of gap learning moved the two real hands from 118 to 123
(Bryan) and 43 to 43 (Ken). Hand-written rules are at their ceiling. The owner asked for a
radical new input. One was built and proved outside the tree: a small network (conv front end,
two-layer bidirectional GRU, CTC) trained on synthetic keying only - the wild recipes, noise,
QSB, drift, a second station and, decisively, **receiver AGC**, which the IC-7300 applies and
which no rule in the tree models. Before any code was written for the tree it read, by letter,
**Bryan 128 of 144** (reader 118 shipped, 123 with gap learning), **Ken 66 of 89** (reader
43), the first recording 26 of 27, and W1AW in sentences. Its reads are printed in
`model\CWNET_SPEC.md`; the engine's port must print the same stretches.

**What the zip carries** under `model\`: `cwnet.onnx` (the net, 5.6 MB), `CWNET_SPEC.md`
(the exact features, the output alphabet, the streaming rule), `cwnet-vector.wav` (a 12 s
synthetic scene - **not a recording**), `cwnet-vector-feat.csv` (its feature matrix),
`cwnet-vector-ids.txt` (the net's argmax per frame) and `cwnet-vector-text.txt`
(`ROGER TNX FER CALL`). Copy `model\` to `src\Hamlet.RadioEngine\Cw\Model\` and the vector
files to the engine tests' fixtures as the tree keeps fixtures.

**Standing rules** (HM-DEC-276, HM-DEC-284) apply. `--no-build` after one build per round,
build servers shut down first, one test run at a time; a run stopped for memory is run once
more alone without asking.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted. The draft truth `cw-2026-10-19-141708.truth.md` is not edited. The sequence reader,
the gap learning, the marks, the lanes, the keyer, the wild bank and every board are not
touched.**

**Numbering.** This is unit 597, rulings HM-DEC-301 (the run budget) and HM-DEC-302 (the
learned reader). If taken, use the next free and say so.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      docs\cw-scoreboard.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwSequenceReader.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all five are not as stated, refuse: reply with only the path you are in,
which checks failed, and "wrong project - nothing done."

If all five hold, say "Hamlet confirmed" and continue.
```

---

## 1. Rules

- Take the lock with **`tools\arbiter\lock.bat take C:\Source\HamLet`** - the root is named;
  without it the lock lands on another project. Release it at the end. Write nothing to
  `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`. Tick nothing in `PHASE_PLAN.md`.
- **Kill no process you did not start.** A `testhost` that is not yours belongs to another
  project's run; wait for it or report it.
- One `dotnet test` invocation per line, filtered, with a `timeout`.
- Apostrophes in quoted heredocs break; `;` is refused; Python cannot run here; `-m` more than
  once for a multi-line commit. Scripts go in `.run-unit\unit597-<name>.sh`, not committed.
  **No sed inserts into C# files**: edit with the file tool.
- **The one layout holds**: nothing on screen moves. The learned reader's text appears where
  the sequence reader's text appears, in the same place, same font, same spacing rules.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.

---

## 2. The change

1. **The runtime.** Add the NuGet package `Microsoft.ML.OnnxRuntime` (CPU, the current
   stable) to `Hamlet.RadioEngine`. If restore cannot reach NuGet, stop: say so in section 4
   and do nothing else. The model file is a content item copied to the output next to the
   engine (`Cw\Model\cwnet.onnx`); the engine loads it once, `IntraOpNumThreads = 1`.

2. **`CwLearnedReader`** in `src\Hamlet.RadioEngine\Cw\`, built exactly to
   `model\CWNET_SPEC.md`: the decimation to 8 kHz with an anti-alias low-pass, the features
   (§2 of the spec - periodic Hann 256, hop 80, bins 8..33, log power, per-frame median across
   bins, per-bin 10th percentile, the one scale), the net, the greedy decode (§3), the sliding
   12 s window once a second with the 2 s commit lag and the continuous collapse (§4). It is
   fed the same audio the sequence reader is fed, before any pitch, lane or mark finding, and
   its text goes where the sequence reader's text goes.

3. **The switch.** `CwRules.LearnedReader`, **off by default**, wired exactly as
   `CwRules.GapLearning` is wired. On, the learned reader reads and the sequence reader's text
   is not shown; off, nothing in this unit runs. Both readers never print at once.

4. **The tests**, `TheLearnedReaderTests`, four cases, each under ten seconds:
   - `FeaturesMatchTheVector`: the engine's feature matrix for `cwnet-vector.wav` equals
     `cwnet-vector-feat.csv`, every cell within **1e-3**; on failure the message names the
     first cell that differs, its row, column, both values. **This is the test the rounds
     chase. Until it passes, nothing downstream is judged.**
   - `TheNetMatchesTheVector`: the argmax indices equal `cwnet-vector-ids.txt`.
   - `ReadsTheVector`: the decoded text is `ROGER TNX FER CALL`, through the streaming path
     (the audio fed in 100 ms pieces as the radio would), not through one call on the whole file.
   - `TheSwitchOffIsSilent`: with `LearnedReader` off, the learned reader is never constructed.

**The trace, first:** the learned reader's full text for Ken (0-120 s of the draft-truth
capture) and Bryan (120-300 s), W1AW piece-03 and piece-04 (first 60 s each) and the first
recording, verbatim, next to the stretches printed in `CWNET_SPEC.md`. Where the port is right
these agree to a letter or two per line; where they do not, the port is wrong somewhere
upstream of the net, and the round fixes that.

## 3. Rounds

Round one: the runtime, the reader, the switch, the tests, the trace; the quick measure with
the switch on (the twelve-plus-three, W1AW, the wild bank, **the two real hands per 20 s as
the survey scores them**). Rounds two and three, only if the vector test fails or the trace
disagrees with the spec's stretches: find where the engine's numbers leave the spec's
(features first, then resampling, then the window and commit), fix that, measure again. Twice
at most. **Nothing in the model, the alphabet or the feature recipe is "tuned"**: a mismatch
is a bug in the port.

## 4. What ships

The switch ships **off**. This unit proves the port and measures; the owner rules on turning
it on after seeing the figures. There is no bar to pass and nothing fails for a low figure:
the report says, row by row against the shipped reader (shipped figures from
`docs\cw-scoreboard.md`), what the learned reader printed - the twelve-plus-three, W1AW, the
wild bank, `bug`, `machine`, the easy fixtures, the 5 dB CQs, noise, the steady carrier, the
random-keyed carrier, the first recording, Ken and Bryan per 20 s, CPU on W1AW piece-04.
The guards are not changed. If `FeaturesMatchTheVector` still fails after three rounds, say
so first and give the first differing cell; the measure is still run and reported.

---

## 5. The lines, the survey, the record

Run the app line, the engine line and the survey line **once, at the end** (the survey with
the switch **on** - the shipped figures are already on the scoreboard; there is no switch-off
survey). Rewrite `docs\cw-survey-2026-10-09.md` with a new `Rerun` line marked
*learned reader*; every group.

- `UNIT_RUN_BUDGET.md` goes to the root as it is in the zip; **`CLAUDE.md`** gains, in its
  rules, the paragraph: *"Run budget (HM-DEC-301): one survey, one engine line, one app line
  per unit, at the end; quick measures per round; no rule-off survey; a work instruction that
  asks for more is refused in output.md section 4. The lock is taken with the root named:
  `tools\arbiter\lock.bat take C:\Source\HamLet`."*
- `PHASE_OUTCOME.md`, both copies: `## UNIT 597 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 597.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first: **HM-DEC-301** *Run budget*, the standing rule in the owner's
  words from `UNIT_RUN_BUDGET.md`, and the lock root; **HM-DEC-302** *The learned reader*,
  naming the model, what it was trained on, that it has heard no recording, the switch and its
  default, the vector tests and whether they pass, and the figures: the shipped reader beside
  the learned reader, every row of §4.
- `docs\cw-scoreboard.md`: a 597 row in every table, marked *learned reader, switch off*.
- `CW_SPEC.md`: a new section, *The learned reader*, three paragraphs - what it is, how it is
  fed and where its text goes, what it does not do (from `CWNET_SPEC.md` §5).
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-301 and HM-DEC-302.

---

## 6. Report

Section 2, for the owner, in plain words, leading with **Ken and Bryan: the learned reader
against the shipped reader, per 20 s, and the full text of each**; then whether the port
matches the vector; then the rest of the rows. Section 1: per round. **Section 3: the trace
texts beside the spec's stretches; the vector test's result (and the first differing cell if
any); the rounds' table; the survey's groups; CPU.** Section 4: anything left, a line each -
and the NuGet restore if it failed.

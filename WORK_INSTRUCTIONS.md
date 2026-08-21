# WORK_INSTRUCTIONS.md

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      Hamlet.sln
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwGate.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  src\CoreHMI

These four files are fixed. Do not substitute a different file for any of
them and do not report a check against a file this list does not name.

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## Why this unit exists

**Tim operates tonight. The decoder he has produces garbage on strong, clean
signals and he is replacing it.**

`010244`: S9+10, 24 dB envelope swing, dah/dit ratio 2.94 — textbook — and it read
`■V UR RST ■ 1■21T■`. That is not a noise problem and no amount of tuning has
touched it. A week of work on thresholds, floors, vote windows and analysis widths
has bought a few characters at a time and cost red tests each time.

**The architecture is the fault.** The decoder thresholds the envelope into hard
key-down/key-up runs, fits speed by clustering those run lengths, and picks its
analysis width from the fitted speed. Every stage depends on the one before and
the evidence is discarded at the first step, so nothing downstream can recover
from a wrong commit. Worse, it is a loop with positive feedback: chatter shortens
the fitted dit, a short dit reads as a fast fist, a fast fist widens the gate's
bandwidth, more noise crosses the threshold. Eight of nine recordings sat at 75 Hz
on senders working near fourteen words a minute.

**No serious Morse decoder thresholds.** Bell 1977, VE3NEA's CW Skimmer and
AG1LE's implementations all carry a probability of key state forward, hold several
speed hypotheses at once, and delay the decision so later evidence can revise an
earlier letter. That is why Skimmer changes a word after one more character
arrives and Hamlet cannot.

### The replacement, already written and measured

`tools\reference-decoder\reference_decoder.py` is in the tree. **Read it first. It
is the specification.** About 120 lines of Python, and on the repository's own
captures:

| recording | ratio | speed found | text |
|---|---|---|---|
| `003016` | 24.2 | 22 WPM | `I= HADA KPA15TT ITWAS JUNK = ESTILL HVE MY ETO 91B TT JUST VFB TUBELIN` |
| `003126` | 30.9 | 28 WPM | `A OM = I WATCH AT LEAST 2 MOVIES A DAY WID X# WHY NOT ... WESTERNS` |
| `003758` | 39.2 | 16 WPM | `KIS QRL TU ... AA4MP/4 QNIK` |
| `004507` | 32.5 | 18 WPM | `E JJ AT ARRL DOT NET = EACH STATION HANDLING THIS MESSAGE PE` |
| `014854` | 6.1 | — | nothing |
| `014935` | 2.8 | — | nothing |

Against the current decoder's 38, 35, 14 and 25 characters of fragments on those
same four files. **No seed. No operator speed. It found 22, 28, 16 and 18 words a
minute on its own.**

### Ruled by Tim, tonight

**The old decoder comes out. There is no toggle, no fallback and no parallel
path.** He has weighed a decoder that has never touched a radio against one that
is useless on a radio, and chosen. **He is not asking for perfection on the first
pass. He is asking for something with value that he can test at the rig.**

*Rejected: keeping the old decoder alongside.* Two decoders means an evening where
he cannot tell which produced a bad line.

*Rejected: staging this across three units.* He operates tonight.

---

## The three ideas, which are the whole of it

1. **Never threshold.** Every hop produces two numbers — the log-likelihood the
   key is down and the log-likelihood it is up — against a noise model. Nothing
   commits.
2. **Speed is a hypothesis, not a measurement.** A dozen speeds run in parallel
   and the accumulated likelihood picks. **The loop cannot exist**, because
   nothing measures speed from run lengths and nothing selects a bandwidth from a
   measured speed.
3. **Elements and characters are decided together, and late.** Dynamic programming
   over whole elements chooses all the boundaries at once against each speed
   hypothesis, instead of comparing one gap at a time to a threshold.

**Silence falls out of this rather than being bolted on.** "The whole stretch is
noise" is an explicit competing hypothesis. On the two recordings holding no
station it wins: ratios of 3 to 6 against 24 to 39 with a station, no overlap.
That is HM-DEC-120 satisfied by construction, not by a guard.

---

## Verify this instruction against the tree

- **Report mismatches; do not repair the instruction silently.**
- **Expected red before you start: five.**
  `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`,
  `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`,
  `TheToneIsFoundInRealisticAudio(farnsworth-heavy)`.
  **Several will be deleted by this unit. Report what the count is at the end and
  what each survivor is.**
- `HM-OPEN-055`: two rig tests flake intermittently and pass on a rerun. Not this
  unit. Do not chase them.

---

## Rulings in force

**HM-DEC-120 — nothing is emitted on audio holding no signal.** The one property
that must survive. **The likelihood-ratio gate is how, and it must be tested, not
assumed.**

**HM-DEC-091 — one source, and it says which.** Every number the new decoder puts
on a panel or in a sidecar says what it is a measurement of.

**HM-DEC-048 — nothing raises a confidence score.**

**HM-DEC-090 — the freshness guard on captures is untouched.**

**HM-DEC-093 — no radio on the development machine.** Everything here is verified
against recordings.

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` 13 —
the six fields 13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — Build it, and check it against the Python

`CwProbabilisticDecoder`, new, in `src\Hamlet.RadioEngine\Cw\`. Port
`reference_decoder.py` faithfully:

- Quadrature mixdown at the tracked tone, envelope at roughly 5 ms hops.
- Per-hop log-likelihoods of key-down and key-up from a noise scale and a signal
  amplitude. **No threshold is formed anywhere.**
- A segmental Viterbi over element kinds — dit, dah, inter-element gap, character
  gap, word gap — with a Gaussian penalty on how far a segment's length sits from
  the 1, 3 or 7 units the hypothesis expects, and elements forced to alternate.
- An outer loop over speed hypotheses; the best total likelihood wins.
- The likelihood ratio against the all-key-up hypothesis, per hop.

**The check that matters: run the Python on `cw-2026-08-18-004507.wav` and run
your C# on the same file, and report both strings side by side.** Three orders in
a row this week were written from measurements no session could reproduce. **If
the two do not substantially agree, the port is wrong — say so and stop rather
than shipping something plausible.**

---

## Task 2 — Make it stream

The reference runs offline over whole files. The terminal needs it live.

- A sliding window with a decision delay. Bell used about a second; **choose one,
  say what you chose and why.**
- Text already committed does not change under the operator. Text inside the delay
  may be revised — **that is the point of the architecture and the panel should
  show the difference**, in whatever way the terminal already distinguishes
  settled text from provisional.
- **Report the cost per second of audio.** The speed search is an outer loop over
  a dozen hypotheses and nobody has measured it live. **If it will not keep up,
  reduce the hypothesis count and say what you reduced it to** rather than
  shipping something that stalls at 9pm.

---

## Task 3 — Wire it to the terminal and take the old one out

**The new decoder is the only thing feeding the CW terminal.** No setting, no
fallback, no parallel path.

Then delete the old decode path and every test that exists only to describe its
behaviour — the thresholding gate, the run-length clock fit, `Refine`, the
speed-selected analysis width, the vote window, the element floors.

- **`CwToneTracker` and the coarse survey stay.** Finding a station is the one
  thing that works and this unit does not touch it.
- **The keying meter stays.** It is the independent witness and it shares no code
  with the decoder on purpose.
- **The capture press, the roster, the sidecar and the case measure stay
  untouched.** Tim marks cases tonight and that machinery is the only instrument
  the project has.
- **A test that fails only because the old decoder is gone is deleted with it.
  A test that asserts something still true is kept and made to pass.** Say which
  you did for each, in a list.

---

## Task 4 — Prove the silence

The gate is `GATE = 15.0` in the reference, sitting in a measured gap between 6
and 24 on six recordings. **It is provisional and the README says so.**

- Assert the two recordings holding no keying emit nothing.
- Run the synthesized sensitivity sweep and report the worst invention at every
  level.
- **If no single gate value both reads the stations and silences the empty band,
  say so and stop.** That is Tim's, and it is the one place this architecture
  could still fail him.

---

## Task 5 — What the operator sees. **THIS IS THE DROP CANDIDATE.**

The panel's existing language assumes a fitted speed and an operator seed. The new
decoder has neither — it reports the hypothesis that won. Update what the terminal
says about speed so it is not describing machinery that no longer exists.

**Drop it whole if the session is running long and say so.** Wrong wording beside
right text is survivable tonight. Wrong text is not.

---

## Parked — do not touch, do not raise

- **The gate width sweep and the 35 ms question.** Moot: there is no gate.
- **`Refine`, the element floor, the clock fit, `ShortestVote`.** All deleted with
  the old decoder. The asks about them close.
- **`RfGain` reading 100% with the knob at noon**; stations reading 375 to 825 Hz
  against a 600 Hz pitch; the lock lost at 25 to 27 seconds. Real, not this unit.
- **HM-OPEN-055**, the two flaking rig tests.
- **HM-DEC-098, HM-DEC-130, HM-OPEN-033, HM-OPEN-007.**

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: 9.5.1 one branch
and it is `main`, **and every session commits and pushes to it**; no interactive
or destructive git; do not invent a ruling id; do not touch coverage thresholds.

Unit-specific:

- **Do not leave a way to run the old decoder.** *Tim ruled it out explicitly. Two
  decoders means an evening where he cannot tell which produced a bad line.*
- **Do not form a threshold anywhere in the new decoder.** *That is the defect
  being removed. If a port step seems to need one, it is a mistranslation.*
- **Do not touch the survey, the keying meter, the capture press or the roster.*
  *He marks cases tonight.*
- **Do not adjudicate any capture or write an answer key.** *The reference
  decoder's output is what it emitted, not truth (§12.5).*
- **Do not spend the session perfecting word spacing.** *Every decoder in the
  field runs words together, including the best. `HADA` and `ITWAS` are readable.
  Wrong letters are not.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings: **What Claude did**, **What Tim should expect**, **What we should
do next**, **What's blocking us** — the last carrying **Asks still outstanding**
per HM-DEC-139.

**Section 1 opens with the two strings from task 1 side by side** — the Python's
and the C#'s on the same recording.

**Section 2 states in one sentence what he will see on the terminal tonight that
he did not see last night**, and lists anything that got worse.

**Stop and report.**

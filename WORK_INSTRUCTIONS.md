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
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## Why this unit exists

**Five sessions have been spent looking for a gate to stand in front of `Refine`.
The premise was wrong and it was the work orders' premise, not a session's.**

Five candidates for HM-OPEN-054 have been measured and eliminated: the survey's
verdict, the ratio band, the transition clock on the decoder's window, the peak's
prominence, and the clock on a window the transitions chose. Last session showed
they are one family — **all five ask whether a pattern of on and off times looks
like a person sending, and band noise through a narrow filter looks like a person
sending.** `cw-2026-08-20-014935` holds no keying at any pitch and produces
twenty-four bursts, the best fitting a clock at 0.736; `cw-2026-08-17-013347`
decodes a real callsign and manages 0.393.

**Tim's ruling, this session: attack the invention at its source instead.**

`Refine` invents because removing the gap average lengthens the dit until gate
chatter passes the coherence check. **That is arithmetic inside `Refine` and the
estimator around it. It has never been attacked there** — every session so far has
tried to catch the output afterwards.

**The family of transition-shape tests is abandoned. Do not propose a sixth.**

---

## Also this session: a second callsign

Last session's report states, in passing, that **`cw-2026-08-17-013347` decodes
`VA3VRR`**. Nobody has recorded it.

`N4L` in `cw-2026-08-17-134712` has been the only adjudicated ground truth in nine
real recordings for five sessions, and every argument in that time has rested on
it. **A second one doubles the evidence base.**

**Task 1 establishes it or reports that it cannot be established.** It is not a
formality: `013347` emits 8 characters at the decoder's current settings and 14 in
last session's measurement, so what it reads and what is true are not the same
thing, and a callsign asserted from a decode nobody checked is worth nothing.

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim; report mismatches and do
not repair the instruction silently.

- **Expected red, do not rediscover:**
  `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`,
  `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`.
  **2,100 tests, four failing. Anything above four is new.**
- The amplitude rule is in the tree. **`Refine` is not.**

---

## Rulings in force

**§9.5.1 — one branch, `main`, commit *and push*.**

**HM-DEC-144 — `cw-2026-08-17-134712` holds `N4L`, dit 56.3 ms, dah 238.3 ms.**

**HM-DEC-090 — marking is not a substitute for silence.** Seventeen hundred
characters once came out of half a minute of band noise, every one marked.
**`Refine` shipping with the invention marked low is not an option.**

**HM-DEC-114 — the easy tier passes or fails.**

**HM-DEC-048 — nothing raises a confidence score.**

**HM-OPEN-054 stays open and no sixth transition-shape test may be proposed.**

**The keying meter is not read by the decoder.**

**HM-OPEN-053 — `ShortestVote` stays at 5. `MaximumRatio` stays at 3.8.**

**HM-DEC-093 — no radio.**

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` §13 —
the six fields §13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — `VA3VRR`, established or not

Do for `cw-2026-08-17-013347` what was done for `N4L`: take the gate's own
elements across the stretch where the callsign is decoded, cut them by their own
fitted means, and read the letters out.

- **Cuts fitted from that stretch and nothing else** (§12.5). Do not ask the
  decoder what a dit is; that is what is under investigation.
- Report the element sequence, the letters it gives, the times, the dit, the dah
  and the ratio, exactly as HM-DEC-144 records them for `N4L`.
- **If it does not spell `VA3VRR`, say what it does spell and record nothing.** A
  callsign taken from an unchecked decode is not ground truth and would poison
  every measurement that later rests on it.
- If it does, record it as a decision entry with the sequence and the letters in
  it, indexed in `CLAUDE.md` §1, and pin it with a test in the manner of
  `TheStationInTheRecordingIsN4LTests`.

**This is worth the session on its own even if task 3 fails.**

---

## Task 2 — Where the invention comes from. **CHANGE NOTHING.**

Instrument `Refine` and the estimator around it on `cw-2026-08-20-014854` and
answer, from the run:

1. **What is the dit before `Refine` and after it, at each of the nine moments a
   character is invented?** Last session's figures: `U EE ■ ■` at the amplitude
   rule's settings, up to nine characters with `Refine` on.
2. **Which marks are in the window at those moments, and what are their lengths and
   heights?** The amplitude rule leaves this recording alone because it holds one
   height population — **say whether that is still true at the moment of each
   invention, or whether a second population appears.**
3. **What does coherence reach, and against what dit?** A dit fitted from chatter
   makes chatter look coherent; say by how much.
4. **On `cw-2026-08-18-003016`, which gains 38 to 43 characters with `Refine`, run
   the same instrumentation.** *The difference between the two is the whole
   question: same change, one recording improved and one invented.*

**Report all four before touching anything.**

---

## Task 3 — Fix it inside, only if task 2 named a cause

**If task 2 did not produce a single measured cause, stop and report.** Five
sessions have been lost to plausible stories; a sixth on a guess is not affordable.

If it did:

- The change must be **inside `Refine` or the estimator it feeds** — not a test
  standing in front of emission. *A gate is what was ruled against.*
- **Fitted, not a constant.** *Seventh instance of the error class five rulings
  have gone on closing.*
- It may make the decoder measure better. **It may not make it more willing to
  emit** (HM-DEC-048).

| | required |
|---|---|
| `cw-2026-08-20-014854` | **no more than 1** |
| `cw-2026-08-20-014935` | **0** |
| `004507` | ≥ 26 |
| `003016` | ≥ 38 |
| `003126` | ≥ 36 |
| `003758` | ≥ 14 |
| `013347` | ≥ 8 |
| the easy tier | **whole** |

Report `134712`'s dit against HM-DEC-144's **56.3 ms**, and re-run
`ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`. **If green,
print what it read and say whether `N4L` is in it, in the right place. Do not tune
anything to make it pass.**

---

## Parked — do not touch, do not raise

- **A sixth transition-shape test.** *Ruled out this session.*
- **Character structure**, and the keying meter as something the decoder reads.
- **`MaximumRatio`**, the three-way length fit, the speed-tracker rewrite.
- **Why the 19th's stations are missing from the audio.** Five theories dead.
- **The 69 and 233.**
- **Adjudicating by ear.** Tim's. *Task 1 is arithmetic on elements, not
  listening.*
- **HM-OPEN-052**, the five synthesized tests, rulings 096–133, the scorer,
  `CaptureAudioAsync` end to end, `TheRosterIsOneFilePerEvening`.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: §9.5.1 one branch
and it is `main`; no interactive or destructive git; do not invent a ruling id; do
not touch coverage thresholds.

Unit-specific:

- **Do not build a gate.** *Whatever shape it takes, a test standing between the
  decoder and emission is the family that has failed five times.*
- **Do not relax the withdrawal condition.** HM-DEC-090.
- **Do not record `VA3VRR` unless the elements spell it.** *Ground truth taken on
  trust is worse than none, because everything after it inherits the error.*
- **Do not tune to `014854` or to `134712`.** *The nine-capture table and the easy
  tier are the guards.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings, per §13: **What Claude did**, **What Tim should expect**, **What we
should do next**, **What's blocking us** — the last carrying **Asks still
outstanding** per HM-DEC-139.

**Section 1 opens with task 1**: whether `013347`'s elements spell `VA3VRR`.

**Section 2 says plainly whether anything shipped, and whether the project now has
two adjudicated recordings or one.**

**Stop and report.**

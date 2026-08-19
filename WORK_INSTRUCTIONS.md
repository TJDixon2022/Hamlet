# WORK_INSTRUCTIONS.md

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      Hamlet.sln
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwGate.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  src\CoreHMI

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## Why this unit exists

**There is no number for the thing Tim is trying to improve.**

Every figure this project has ever reported counts characters against an answer
key on one capture: 13 of 43 on the leading edge, 33 on the settled pass. Tonight's
goal is different and it is his words, not a restatement — **CW discerned in
seventy-five per cent of the cases he hears it.** Cases, not characters. A case is
a station he hears on the air; it succeeds if Hamlet produced text he could read.

Nothing in the application counts that. It cannot be derived from what exists,
because the denominator is his ear and every measurement Hamlet holds is
downstream of the decoder — **a decoder that misses a station also misses the
case, and the score comes out a hundred per cent.**

This unit builds the instrument, not the improvement. Tonight it produces a roster
of cases. The percentage arrives when he scores it.

**Ruled this session, all four by Tim:**

1. The measure is **cases, not characters** (option A).
2. The denominator is **his press**, and one press both marks the case and keeps
   the audio (option B) — not a derived signal, because the thing deciding a case
   occurred must sit outside the system being measured.
3. `ShortestVote` **stays at 5**. See "Rulings in force".
4. The verdict is **judged afterwards, from the roster and the audio** (option C) —
   not asked at the press, and **not derived from a character-count threshold.**

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Every path, method name and line number below
came from a copy of the source read in a chat session, not from the tree in front
of you. Check every claim against the files and report any mismatch.

- **Report the mismatch; do not repair the instruction.** A session that corrects
  it silently teaches nobody and leaves the next unit carrying the same wrong fact.
- **Mismatches go in the report even when the work succeeded anyway.**
- **Expected red, do not rediscover:** `APassThatReadSomethingEmitsSomething`,
  `TheBulletinDecodesToItsAnswerKey`, `ClearingTheTranscriptLeavesTheDecoderAlone`.
  Three failing out of 2,002. **Anything above three is new and belongs in the
  report.**

What the chat session believes it found, all of it to be checked:

- `MainWindowViewModel.CaptureAudioAsync` — a `[RelayCommand]` around line 3177 —
  reads the rig, takes `_decoder.Tap.Snapshot()`, and writes `cw-<stamp>.wav` plus
  `cw-<stamp>.txt` into `Path.Combine(SettingsStore.DataFolder, "captures")`.
- `CaptureNotes(audio, samplesSeen)` writes the sidecar: `captured`, `audioSeen`,
  `fingerprint`, `seconds`, `sampleRate`, `frequency`, `band`, `inputPeak`,
  `meterPeak`, `inputFloor`, `clipping`, `toneHz`, `snrDb`, `elements`,
  `characters`, `decoderWpm`.
- The press is bound in `src\Hamlet.App\Views\MainWindow.axaml` around line 962 —
  a button reading **Keep this audio**, `IsEnabled="{Binding IsDecoding}"`.
- `IsDecoding` is set true when the decoder starts listening, **not** when it
  produces text. So the button is available on a station Hamlet reads nothing of,
  which is the case that matters most to this unit. **Confirm this. If the button
  is unavailable when nothing is decoding, say so and stop — the whole measure
  depends on it.**
- The freshness guard (HM-DEC-090) refuses a write when `tap.SamplesSeen` equals
  the last capture's. With audio flowing it does not fire between stations.

---

## Rulings in force

**HM-DEC-090 — a capture that cannot prove it is fresh is not written.** Three
presses inside seventy seconds produced byte-identical files beside rig state that
differed on every one, and the operator reasoned from one recording presented as
three. **Do not weaken, bypass or add an override to this guard** in order to make
the roster tidier. A refused write is a case with no evidence, and the roster must
say so rather than silently omitting it.

**HM-DEC-091 — one source, and it says which.** Where the radio has been read, the
radio is the answer; where it has not, the field says so rather than presenting a
guess in the same shape as a measurement. Applies directly to every roster column.
**Do not invent an answer key for any capture.**

**HM-OPEN-053 — `CwGate.ShortestVote` stays at 5 for this unit.** Tim's ruling
today, and it reverses the recommendation the web session gave earlier in the same
conversation. 5 → 7 is measured at 13 → 27 of 43 on the bulletin's leading edge
and breaks five synthesized tests, two of them about finding a station at all.

*Rejected, and why:* shipping it tonight. Two of the five failures land on
acquisition, which is exactly the quantity this unit begins measuring, and a
station missed leaves no evidence — so a poor evening would be unattributable
between the change and the band. **You do not change the instrument and the
subject in the same run.** The ruling is carried forward unchanged in the asks.

*Also rejected:* the argument that the character gain is large enough to ship
anyway. It is large. It is a gain in the wrong unit for tonight.

**Do not re-argue either. Do not touch `ShortestVote`, `VoteShareOfDit`,
`LongestVote` or anything else in `CwGate`.**

**HM-DEC-093 — no radio.** Development is on the dev machine, which has no radio
attached. Testing is on the ham machine, which has one. **Everything in this unit
must be verifiable without a radio.**

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` §13 — `STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, and
`NOTE` saying what is moving inside the task rather than restating the task name.
Do the same every ten minutes while a task is running.

---

## Task 1 — Trace before anything is built

**Say what you find rather than confirming the list above.** It was written
without the tree in front of it.

Answer from the code, and report before writing a line:

1. What exactly does one press of **Keep this audio** write today — every file,
   every path, every field of the sidecar.
2. Is the button available when the decoder is listening but reading nothing?
   Quote what gates it.
3. Is there anything already in the tree that tallies captures, or any roster,
   index or per-evening file? If a roster already exists in some form, **say so —
   this unit may be smaller than it looks, and that is tokens back.**
4. What does the sidecar record about whether a station was found — which fields
   would a human read to decide "Hamlet got this one"?
5. Where does `SettingsStore.DataFolder` resolve to on this machine?

---

## Task 2 — The press marks a case

One press must record that **Tim heard a station**, which is a fact the
application does not currently hold. The audio and the sidecar already carry
everything about what Hamlet heard.

- The press keeps its existing behaviour entirely. Nothing about the WAV or the
  sidecar changes.
- The button's label and tooltip change to say what it now means. It is no longer
  only "keep this audio" — it is *I heard CW here.* The wording is yours; make it
  unambiguous that pressing it asserts the operator heard a station, whether or not
  anything appeared on screen.
- **A refused write still marks the case.** When the freshness guard declines, or
  when there is no audio, the case happened and the roster records it with the
  reason. This is the one place the guard's refusal must become visible rather than
  only a status line.

---

## Task 3 — The roster

An append-only file per evening in the captures folder — `cases-<yyyy-MM-dd>.txt`
— one row per press, written at the moment of the press.

Columns, and every one of them from a source that already exists:

| Column | From |
|---|---|
| `time` | UTC at the press |
| `frequency` | the same reading the sidecar uses (HM-DEC-091) |
| `band` | the same reading as the frequency |
| `wav` | the filename written, or `none` with the reason |
| `toneHz` | the decode report, `none` if no tone |
| `snrDb` | the decode report, `unread` if unread |
| `wpm` | `decoderWpm`, `not tracking` if not |
| `chars` | emitted and unsure |
| `read` | **left empty** |

**`read` is Tim's column and nothing writes to it.** It is not derived, not
defaulted, not pre-filled with a guess from the character count. A threshold
standing in for a judgement is the error class the last session tabulated five
times, and this is the column where it would be easiest to make it a sixth.

Header rows are fine. Fixed-width or tab-separated, your choice — it is read by a
person in a text editor and by task 5.

---

## Task 4 — Prove it without a radio

**This is the task that protects the evening.** A control that writes files fails
silently — no error, no file, discovered at eleven at night with a station coming
through.

Drive the whole path from a kept capture played back through the audio source the
application already uses for replay, and assert, in a test that runs in CI:

- a WAV appears,
- a sidecar appears with its fields populated,
- a roster row appears, matching the sidecar,
- a second press with no new audio produces **no WAV and a roster row saying why**.

Use `tests\fixtures\cw\captured\cw-2026-08-18-004507.wav`. Do not add a new
fixture and do not synthesize one.

---

## Task 5 — The scorer. **THIS IS THE DROP CANDIDATE.**

A small command that reads a roster, counts rows where `read` is filled in
affirmatively, and prints the percentage with both counts beside it.

**Drop it whole if the session is running past its window, and say in the report
that it was dropped.** Never half-built. Tim can count rows in a text editor
tomorrow; he cannot mark a case tonight without tasks 2 to 4.

---

## Parked — do not touch, do not raise

- **HM-OPEN-054**, how the settled pass tells keying from a carrier. Real, and the
  reason the transcript stops mid-contact on a slow sender. Its own unit, and it
  needs a ruling on which of three distinguishers to try.
- **HM-OPEN-052**, whether HM-DEC-097 is satisfied by existing silence. Tim's
  ruling, not a session's.
- **The five synthesized tests** that `ShortestVote` 7 would break. They are green
  today and stay green today.
- **The three expected failures.** Named above so they are not rediscovered.
- **Rulings 096–133 missing from `DECISIONS.md`.** Considered and dropped as not
  worth the time; the §1 index rows are substantial.

**A parked item that turns out to block a task is raised anyway, once, and says it
was parked.**

---

## What not to do

Standing prohibitions are in `CLAUDE.md` and are not retyped here. Cited, not
restated: §9.5.1 one branch and it is `main`; do not push; no interactive or
destructive git; do not invent a ruling id; do not touch coverage thresholds.

Unit-specific:

- **Do not touch `CwGate`, `CwSettledPass`, `CwToneSurvey` or `CwDecoder`.** No
  decoder behaviour changes in this unit. *This protects the baseline — tonight's
  roster is only meaningful if it measures the decoder Tim has been running.*
- **Do not fill, default or derive the `read` column.** *It is the one judgement
  the instrument is not allowed to make for him.*
- **Do not weaken HM-DEC-090's freshness guard.** *It exists because three
  identical recordings were once reasoned about as three pieces of evidence.*
- **Do not write captures into `tests\fixtures\cw\captured`.** *That folder holds
  the adjudicated evidence base; an evening of unscored audio in it dilutes the
  one thing this project trusts.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed to the session. Four
sections, no other headings: **What Claude did**, **What Tim should expect**,
**What you should see**, **What's blocking us** — the last carrying the standing
**Asks still outstanding** heading per HM-DEC-139, with `ShortestVote` still on it.

**Section 3 leads with the answer to the question this unit was commissioned to
ask:** can Tim mark a case tonight, in one press, and will it survive an evening —
with the evidence from task 4, not an assertion.

**If you finish every task, stop and report. Do not start the next unit.**

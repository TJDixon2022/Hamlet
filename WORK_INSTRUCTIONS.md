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

The previous session built the case roster and reported a gap it could see and Tim
could not, until it said so: **the roster records counts and never a character of
decoded text.** `chars` says `19 emitted, 6 unsure`, and nothing anywhere says what
those nineteen were.

That makes a row a pointer to evidence rather than evidence. Tonight Tim marks
every station he hears; tomorrow he scores each one as read or not read. With
counts alone, scoring thirty cases means opening thirty recordings and listening to
each — an evening's work, and the sort of evening that does not get spent. With the
text in the row, most cases are decided by reading the file and the audio is needed
only for the ambiguous ones.

**Ruled by Tim, this session:** add the decoded text to the roster row.

This is a small unit deliberately. It is going in on the day it is used, ahead of
an evening at the rig, on code that has not yet run on the machine that has a
radio. **Nothing else changes.**

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Every name and line number came from a copy of
the source read in a chat session. Check every claim against the files.

- **Report mismatches; do not repair the instruction silently.**
- **Mismatches go in the report even when the work succeeded anyway.**
- **Expected red, do not rediscover:** `APassThatReadSomethingEmitsSomething`,
  `TheBulletinDecodesToItsAnswerKey`, `ClearingTheTranscriptLeavesTheDecoderAlone`.
  The last session reported 2,005 tests with those three failing. **Anything above
  three is new and belongs in the report.**

What the chat session believes it found:

- `CwTranscript` — `src\Hamlet.App\ViewModels\CwTranscript.cs` — holds the settled
  text. `PlainText` returns all of it; `Tail(int count)` returns the last *n*
  characters, trimmed at the front; `IsEmpty` and `CharacterCount` are there.
- `MainWindowViewModel.Transcript` is the live instance, line 1252, and
  `Tail(28)` is already used around line 1433.
- The roster is `CwCaseRoster`, written by the previous session. **Its shape is not
  described here because the session that built it knows it better than this
  instruction does.** Read it.

---

## Rulings in force

**HM-DEC-091 — one source, and it says which.** A row that cannot say where a
field came from presents a guess in the shape of a measurement. Applies to the new
column exactly as to the others: when there is no text, the column says so.

**HM-DEC-090 — the freshness guard is untouched.** A press it refuses still lands
a row. That row gets the text column filled the same as any other, because the
operator heard a station whether or not a recording was written.

**HM-OPEN-053 — `CwGate.ShortestVote` stays at 5.** Carried forward unchanged for
the second unit running. Do not touch `CwGate`, `CwSettledPass`, `CwToneSurvey` or
`CwDecoder`. *This unit must not move the decoder, because tonight's roster is the
first measurement of the decoder Tim has been running and a changed decoder makes
it a measurement of nothing.*

**HM-DEC-093 — no radio.** Dev machine has none. Everything here must be
verifiable without one.

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` §13 — the six fields §13 names, using **`PHASE`**, not `TASK`. The
previous order asked for `TASK: n of m` and that was an error in the order, not in
§13; the last session flagged it and wrote both. Write `PHASE`. Also update every
ten minutes while a task is running, with `NOTE` saying what is moving inside the
task rather than restating its name.

---

## Task 1 — Add the text to the row

One new column at the end of the roster row, after `read` or before it, whichever
leaves the file readable — **but `read` stays the operator's column and stays
last if that is what keeps it obvious.** Your call on placement; say which you
chose and why.

- The text is the transcript's tail at the moment of the press. **`Tail(120)`** is
  the starting point: long enough to carry several overs at any speed, short enough
  that a row stays one line in a text editor. If the roster is tab-separated, the
  text must not contain a tab or a newline — replace them with a single space.
- **When the transcript is empty, the column says so** in the same manner as
  `none`, `unread` and `not tracking` already do. An empty cell and "Hamlet read
  nothing" must not look the same, because the second is the most important row on
  the sheet.
- The sidecar `.txt` gets the same text, in full rather than tailed, since it is
  not constrained to one line. **If adding it there is not trivial, skip it and say
  so** — the roster is what tomorrow is read from.

---

## Task 2 — Prove it, still without a radio

Extend the existing task-4 tests from the previous session rather than writing new
ones beside them.

- A press after real decoding lands a row whose text column carries what the
  decoder emitted, matching the transcript.
- A press with an empty transcript lands a row whose text column says so, and is
  not blank.
- A press refused by the freshness guard still lands a row with the text column
  filled.
- The row stays one line — assert no embedded tab or newline.

Use `tests\fixtures\cw\captured\cw-2026-08-18-004507.wav` through
`BufferedAudioSource`, as before. Do not add a fixture and do not synthesize one.

---

## Parked — do not touch, do not raise

- **Wiring `CaptureAudioAsync` itself into a test.** The last session found no seam
  and correctly declined to make one, because it would change the decode start
  path. **Still declined. Tim is checking that link by hand at the rig.** Do not
  attempt it, and do not add a seam "while you are here".
- **HM-OPEN-052, HM-OPEN-054, the five synthesized tests, the three expected
  failures, and rulings 096–133 missing from `DECISIONS.md`.** All as before.
- **The scorer**, dropped from the last unit. It stays dropped.

**A parked item that turns out to block a task is raised once, and says it was
parked.**

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: §9.5.1 one branch,
`main`; do not push; no interactive or destructive git; do not invent a ruling id;
do not touch coverage thresholds.

Unit-specific:

- **Do not change the decoder, the tap, the WAV writer, the freshness guard, the
  button, its label or its tooltip.** *All of that was settled yesterday and is
  going to the rig tonight; this unit adds one column.*
- **Do not fill, default or derive the `read` column.** *Still the one judgement
  the instrument is not allowed to make for him, and a text column sitting beside
  it makes deriving a verdict newly tempting.*
- **Do not rework the roster's format beyond the new column.** *A file whose
  columns moved between the test run and the evening is a file nobody trusts.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed to the session. Four
sections, no other headings: **What Claude did**, **What Tim should expect**,
**What you should see**, **What's blocking us** — the last carrying the standing
**Asks still outstanding** heading per HM-DEC-139, still seven, with
`ShortestVote` unchanged.

**Section 3 shows a real roster row from the test run, with the text column
populated**, so Tim can see what tomorrow's scoring will actually look like before
he relies on it.

**If you finish both tasks, stop and report.**

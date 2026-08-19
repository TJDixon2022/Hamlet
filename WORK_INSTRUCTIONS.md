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
them, and do not report a check against a file this list does not name.
A gate that drifts to match whatever the last session built stops being a
check against the wrong repository.

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## Why this unit exists

**The roster splits an operating evening in two.**

Tim operates evenings from Pennsylvania, UTC−4 at this time of year. The roster is
`cases-<yyyy-MM-dd>.txt` and the previous session's sample rows carry UTC stamps.
An evening beginning at eight o'clock local crosses midnight UTC after four hours —
so the stations heard before eight land in one file and everything after lands in
another, named for the following day.

Tomorrow morning he opens one file, scores it, and takes the count for the evening.
**The stations in the second file are simply absent from the percentage**, and
nothing on the sheet says they exist. A denominator that silently loses part of
itself is worse than no measure at all, because it reports a number with confidence.

This is a defect in the specification, not in the previous session's work — it
built what it was told to build. **He is operating tonight, so this goes in ahead
of the evening and nothing else moves.**

---

## Verify this instruction against the tree

**Nothing here describes the tree.** `CwCaseRoster` was written by the previous
session and this instruction has not read it. Every claim below is to be checked,
and the roster's actual shape governs.

- **Report mismatches; do not repair the instruction silently.**
- **Expected red, do not rediscover:** `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`. The last session
  reported 2,007 tests with those three failing. **Anything above three is new.**

---

## Rulings in force

**HM-DEC-091 — one source, and it says which.** This unit puts two clocks on one
sheet, which is exactly the shape of fault that ruling exists for. It is handled
below by making the file say which clock each field uses, in the file itself, not
in anybody's memory.

**HM-OPEN-053 — `CwGate.ShortestVote` stays at 5.** Third unit running. Do not
touch `CwGate`, `CwSettledPass`, `CwToneSurvey` or `CwDecoder`.

**HM-DEC-093 — no radio on this machine.**

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` §13 — the six fields §13 names, **`PHASE`**, `UPDATED` from the clock,
`NOTE` saying what is moving inside the task. Also every ten minutes while a task
runs.

---

## Task 1 — The file is named for the operating evening

The roster filename takes the **local** date, so one evening at the rig is one
file whatever UTC does.

- Read `CwCaseRoster` first and report how it names the file today, and which
  clock it uses. **If it already uses local time, say so and stop — the defect is
  in this instruction rather than the code, and that is tokens back.**
- The `time` column **stays UTC and stays as it is.** *Do not convert it and do not
  move it.* A column that changes meaning between the test run and the evening is
  a column nobody can trust, and UTC is the right clock for a record of the air.
- The first row of each file gains a line, above the column header, naming the
  local evening the file covers and stating that the times below are UTC. **The
  file must be unambiguous to somebody reading it cold in six months** — that is
  the whole of HM-DEC-091 applied here.
- Wording is yours. Say what you chose.

---

## Task 2 — Prove the crossing

A test that a press before local midnight and a press after **land in the same
file**, driven across a UTC date boundary.

- Do not wait for a clock and do not rely on the machine's timezone. If there is
  no seam to hand `CwCaseRoster` a time or a clock, **add one** — a clock
  abstraction on the roster only, not on the decoder, the tap or the view model.
  This is the one place in these three units where adding a seam is permitted, and
  it is permitted because the thing under test is a date boundary and there is no
  other way to reach one.
- Assert both rows are in one file, and that the file is named for the local date
  of the evening rather than the UTC date of the later press.
- The existing five tests stay green and are not rewritten.

---

## Parked — do not touch, do not raise

- **`CaptureAudioAsync` end to end.** Still no seam, still declined, still checked
  by hand at the rig. **The permission in task 2 does not extend to it.**
- **HM-OPEN-052, HM-OPEN-053, HM-OPEN-054**, the five synthesized tests, the three
  expected failures, rulings 096–133 missing from `DECISIONS.md`, and the scorer.
- **The sidecar's `text` line naming.** Settled — it stays `text`, not `read`.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: §9.5.1 one branch,
`main`; do not push; no interactive or destructive git; do not invent a ruling id;
do not touch coverage thresholds.

Unit-specific:

- **Do not change the columns, their order, their contents or the `read` column.**
  *Settled across two units and going to the rig tonight.*
- **Do not convert the `time` column to local.** *It is a record of the air.*
- **Do not change the capture WAV or sidecar filenames.** *They are stamped UTC
  and they are matched to rows by name; renaming them breaks yesterday's captures
  against today's roster.*
- **Do not add a timezone setting, a preference, or a way to choose.** *One
  operator, one shack, one clock.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings: **What Claude did**, **What Tim should expect**, **What you should
see**, **What's blocking us** — the last carrying **Asks still outstanding** per
HM-DEC-139, still seven, `ShortestVote` unchanged.

**Section 3 shows the header line and one row as they will appear tonight.**

**Stop and report. This is the last unit before he operates.**

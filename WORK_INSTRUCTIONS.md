STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.

---

# Work instruction 039 — the picture, the clock, and the first stage

**ISSUED: 2026-08-28. A fresh order, not an amendment. Follows unit 038.**

**Seven tasks; task 7 is the drop. This is a long unit by instruction.**

## Why this unit exists

**The operator tuned to 14.074 in USB-D on a live 20 m band and reported: the
Hamlet waterfall does not match the radio's waterfall in any way.**

His screenshot, at 16:41 UTC with the radio connected on COM3 and `FIL2`
selected, shows the Digital waterfall **full of energy and drawing nothing that
exists on the air**: the left third saturated to near-white, the right two-thirds
a uniform crosshatch, and **a hard vertical seam at a fixed screen position
between them.**

**No signal produces that.** FT8 on a busy midday band is narrow vertical dashes
on a dark floor, switching together every fifteen seconds. A hard boundary at a
fixed x with uniform fill on both sides is a **rendering fault**, not a band
condition.

**Audio is arriving.** That much the picture proves. What it does with the audio
is wrong.

Three other things are true from the same screenshot and from unit 038's report:

- **The clock has never been queried.** The strip reads `clock not checked yet,
  so slots cannot be cut`. Task 4 of 038 built the display and the arithmetic;
  **nothing runs the query.**
- **The capture press does nothing.** It was drawn unwired in 037 and 038 stopped
  before wiring it. **A control that looks pressable and does nothing is what
  HM-DEC-087 forbids**, and it is now the only lying control on a tab that has
  live values on it.
- **A full engine run is owed** from unit 038.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches,
including where the work succeeded anyway. Trust the tree over this order
everywhere they differ.

From unit 038's report, not measured by this author:

- `IAudioSource.SamplesReady` is an ordinary multicast event; the digital
  spectrum source rides it without disturbing the CW path.
- `ISpectrumSource` and `SpectrumFrame` predate 038 — bins as bytes 0–255, a low
  and high hertz, a timestamp, `IsSimulated` on the source.
- The transform: 4096 at 12 kHz, 16384 at 48 kHz, about 0.341 s, **2.93 Hz bins**,
  348 frames from a 30-second capture, deterministic across chunk sizes of 4096
  and 997.
- The noise floor is **the twenty-fifth percentile of the visible span** — changed
  during 038 from tracking the minimum bin, which saturated the picture and put a
  1500 Hz tone at 1289 Hz.
- Clock display, thresholds and slot arithmetic exist and are tested; **amber past
  half a second**; unknown is never zero.
- App 509 of 509. Engine 28 failing, **not re-run since 038 added four files**.

**Record the failing counts from the tree before task 2.**

## Rulings in force

**Transcribed with what was rejected. Do not re-argue either.**

**Tim's rulings, 2026-08-28:**

> **Digital captures go to `captures\digital\`.** One capture root, one sidecar
> format, one `get-files` habit with one more line.
>
> Rejected: mixing them with the CW captures in one folder — WSJT-X must be
> pointed at a folder of FT8 files, and hand-picking every time is the cost.
> Rejected: a separate capture root — that splits the habit.

> **The digital capture press works exactly the way the CW one does** — the same
> ring, the same window, the same sidecar, the same file shape. **No trimming.**
>
> This supersedes the four-complete-slots ruling of this morning *for the
> mechanism as first wired.* Tim's reason: the CW waveforms are already
> interpretable and the machinery works. **The consequence is stated and
> accepted:** a 30-second grab starting mid-slot leaves WSJT-X two partial slots
> it cannot score, so **these files are diagnostic material, not corpus.**
> Trimming returns when scoring starts. **Do not build trimming in this unit.**

> **The unit is large and carries many tasks.** The 45-to-60-minute window in
> `CLAUDE_CODE.md` is a floor against trivial units, not a ceiling to write down
> to.

> **The tail is the slot cutter, then the Costas sync search, with the sync search
> as the drop.**
>
> Rejected: the slot cutter alone — too thin a tail. Rejected: attempting the full
> C# decoder in the same unit as a render fix — two large things at once, and the
> drop would then have to be one of them.

> **The decoder will be written in C#, not wrapped.** Unit 038 found no C
> toolchain on the machine and Tim's own condition names that as the trigger.
> **This unit does not start the decoder proper** — only its first stage, in the
> tail, and only because that stage runs on the transform that already exists.

**Standing rulings this unit is bound by:**

- **HM-DEC-009** — never present a guess as a decode. A waterfall drawn at a
  guessed slot boundary, or a sync hit reported as a message, both break it.
- **HM-DEC-087** — a control's resting look says press me. An unwired press is a
  defect.
- **HM-DEC-026** — simulated signals say so.
- **§0.2** — no transmit. Nothing keys the radio.

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` — `STATE`,
`TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is moving
inside the task. Same every ten minutes while a task runs.

## The tasks

### Task 1 — the render path, traced end to end, and the engine run that is owed

**First: run the engine suite whole and record the number.** Unit 038 added four
files and did not re-run it. **If it is not 28, that is this unit's first finding
and it is reported before anything else.**

Then trace, and **say what you find rather than confirming this list**:

- **From `SpectrumFrame` to pixels.** Every step: how a frame's bins become a row,
  how rows become the bitmap, what the stride is, how the bitmap is blitted, and
  where the width in pixels is reconciled with the bin count.
- **The band mapping.** `BandLowHz` 200 and `BandHighHz` 3000 against a transform
  covering 0 to Nyquist. **Which bins are selected, and is the arithmetic the same
  on both sides of the seam the operator sees?**
- **The noise floor.** The twenty-fifth percentile — over what span. Per frame,
  per column, or over the whole visible bitmap? **A percentile taken over the
  wrong axis is the most likely single cause of a saturated left third.**
- **The colour ramp.** How a byte becomes a colour, and whether the ramp saturates
  before the top of its input range.
- **Whether the CW waterfall shares any of this code**, and where the two paths
  diverge. The CW waterfall has been trusted for weeks; **if it uses the same
  renderer with different parameters, the parameters are the fault.**

**Name the two or three candidate causes with file and line before changing
anything.**

### Task 2 — reproduce the fault against real radio audio

**The corpus already holds real audio from this operator's radio** — the CW
captures of 2026-08-17 through 2026-08-28. They are not FT8, but they are the
real codec, the real noise floor and the real dynamic range, which is what a
render fault reacts to.

**Build a test that runs a real capture through the digital spectrum source and
renders it**, then asserts what the operator can see by eye:

- **No hard vertical discontinuity** at a fixed column.
- **A known tone lands at its own frequency**, the check 038 already used.
- **The floor is a floor** — the quiet part of the band is dark, not saturated.

**The fault must reproduce in this test before task 3 changes a line.** A fix
that cannot be shown to fix anything is a guess (§0.4).

**If the fault does not reproduce on CW audio**, say so plainly — that is itself a
finding, and it means the cause is specific to what a crowded FT8 passband does to
the renderer. Then reproduce it with a synthesised frame sequence instead, and
**say in the report that the reproduction is synthetic.**

### Task 3 — fix it

Fix the causes task 1 named and task 2 reproduced.

**Acceptance, all asserted by test:**
- The reproduction from task 2 passes.
- A 1500 Hz tone still lands at 1500 Hz.
- Determinism across chunk sizes 4096 and 997 is intact — 038's property, not to
  be traded.
- **The picture drawn from a quiet capture is mostly dark.**

**Do not touch the CW waterfall's parameters.** If the fix requires changing
shared renderer code, **state exactly what the CW picture will look like
afterwards** and, if it changes at all, stop and report instead.

### Task 4 — the clock actually queries

Wire the SNTP query that 038 built the display for.

- On connect, and periodically after. **State the interval chosen and why.**
- **A query that fails leaves the offset unknown**, which is a real state with its
  own words. Never falls back to zero.
- Off the UI thread; a slow or dead server never stalls the tab.
- **The strip's live clock text replaces nothing static that unit 037 wrote.**

**Acceptance:** with a network, the strip reports a measured offset and its age;
with the network blocked, it says so in words and stays saying so.

### Task 5 — the capture press, wired the CW way

The press on the waterfall header does exactly what the CW press does, into
`captures\digital\`.

- **Same ring, same window length, same sidecar contents, same file shape.** The
  CW sidecar's rig state, plus the clock offset if one is known.
- **Filename distinguishes these from CW captures.**
- **No trimming, no slot alignment.** Ruled out of this unit.
- **The sidecar records that the file is untrimmed**, so a later scoring run can
  tell diagnostic material from corpus without opening the audio.
- The press's own label and the folder are the only new strings; **everything else
  on the tab stays as written.**

**Acceptance:** pressing it writes a WAV and a sidecar the operator can find, and
the header says it happened the way the CW press does.

### Task 6 — the slot cutter

Cut the audio into **15-second slots aligned to UTC quarter-minutes**, using the
offset from task 4.

- **Unknown offset means no slots are cut**, and the reason is observable.
- A short slot — the app started mid-slot — is discarded, and the discard count is
  observable (§0.0.1).
- Pure over samples and an elapsed time, tested without a wall clock.
- **Nothing consumes the slots yet.** This is the seam the decoder plugs into.

### Task 7 — the Costas sync search *(the drop candidate)*

The first real stage of the C# decoder, running on **the FFT frames, not the drawn
bitmap** — so it is independent of task 3's outcome.

- FT8 carries **three Costas arrays of seven symbols** at the start, middle and
  end of each transmission. Search a slot's frames for that pattern across
  candidate time and frequency offsets.
- **Report candidates, not messages.** Each carries a frequency, a time offset and
  a sync score. **Nothing goes on the decoded-text panel** — no message has been
  read and HM-DEC-009 is absolute here.
- **Mark located candidates on the waterfall** if the render work of task 3 makes
  that cheap; **skip the marking if it does not**, and say so.
- Tested against a fixture. **A synthesised FT8 slot is acceptable as a unit
  test**, and **is not evidence about yield** — say which it is in the report.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

The whole CW decoder stream and unit 036's residue. The scanner and the calling
cycle. `CHANGELOG.md`. The missing `DECISIONS.md` records including HM-DEC-086's
supersession. The phrasebook and the recent-places row. The prefix table and the
plain-English parser. **The mode strip's `reading it · 9 messages this slot`** —
this author proposed changing it and Tim held it; it stays until it is live.

**Both halves are required: do not touch them, and do not raise them.**

A parked item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **No transmit. Nothing keys the radio.**
- **Do not write any rig setting** — not mode, not filter, not dial. Tim sets the
  radio by hand. Hamlet reads and reports only.
- **Do not edit the CW decoder or the CW markup**, and do not change how the CW
  waterfall looks.
- **Do not build trimming or slot-aligned capture.** Ruled out of this unit.
- **Do not put anything on the decoded-text panel.** It stays as unit 037 wrote
  it until a message has actually been read.
- **Do not fix the waterfall by adjusting a constant until the picture looks
  better.** The fault is named in task 1, reproduced in task 2, fixed in task 3,
  in that order.
- **Do not trade determinism across chunk sizes.**
- **Do not score anything against synthetic audio.**
- **Do not write a colour literal.**
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 to `output.md` at the repository root, overwritten
and printed. **Read the file's own section count and follow it.**

**The section that says what the owner should expect leads with this: what the
waterfall was drawing, why, and what it draws now — and that the capture press
writes a file when pressed.**

**The section that reports measurements leads with the engine's failing count**,
owed since 038, and then the named cause of the render fault with file and line.

**If you finish every task, stop and report. Do not start the next unit.**

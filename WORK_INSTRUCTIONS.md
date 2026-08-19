PROJECT: Hamlet
ISSUED: 2026-08-19

# Work order — tuning from the app puts the old frequency back and holds it for thirty seconds

**One phase. Nothing else is in this order.** Finish it, report, stop.

Gate first (HM-DEC-099): verify `PROJECT: Hamlet` against `PROJECT_CARD.md` in
this tree and against the prompt you were pasted. Any disagreement, stop.

---

## The fact pattern, from the operator watching his own radio

**Turning the dial by hand tracks in real time.** Every time.

**Anything that tunes from the app lags about thirty seconds** — clicking a dot,
dragging the neighborhood map, changing band. **And the display snaps back to
where he was before**, holds the old frequency for roughly thirty seconds, then
catches up. The radio itself moves correctly and stays moved. It is the app's
picture that reverts.

This is ground truth. **Do not ask him to measure it again and do not gate any
part of this order on a connect.**

The dividing line is the write. A read-only case is perfect; every case involving
a tune write breaks. Whatever this is, it is in the write path or in what a write
does to reads in flight.

## What the telemetry supports, and what it does not

`%AppData%\Hamlet\telemetry\2026-08-19.jsonl`, build 1.10.0, session `6630ee0f`,
COM3.

**The poll is not the problem and the link is not the problem.**

- `sent` 1380, `answered` 1379, `unanswered` **0**, `sweepsDropped` 0.
- At 182.9 s into the session `Frequency` reports `ageSeconds` **0.1**, while the
  operator was turning the dial. The frequency is being observed within a tenth of
  a second when no write is involved.

**The app sees the new frequency immediately and then loses it.**

- 4.2 s — `map_dot_tuned` and `tune_requested`, 14.040 MHz.
- 4.7 s — `recent_dwell_short` for **14040000**. The app already holds the new
  frequency, half a second after the click.
- 61.9 s — heartbeat reports 14.040 as having last changed at about 34 s.
  **Thirty seconds after the click, for a value it already had at 4.7 s.**
- 38.2 s — `tune_requested` 14.061. Never observed at all before the operator
  moved the dial by hand.

**`ageSeconds` is time since the value last changed, not staleness.** It has now
been misread twice in this investigation and a wrong ruling was drafted on it.
**Do not diagnose from that field.** The last session already said so.

**The radio does not announce.** `inboundTransceive` 0, `inboundBroadcast` 0,
`radioIsBroadcasting` false, across 4,120 frames from the radio. CI-V Transceive
is off and Hamlet does not write the operator's settings. Everything Hamlet knows
about the frequency, it asked for. That is why the write path can poison it.

## The prime suspect, and it is in code you have already read

**HM-OPEN-042: a read issued with no expected response command completes on `FB`
or `FA`** — an acknowledgement, not a value frame. **A tune write's acknowledgement
is an `FB`.**

So a frequency read in flight when a tune goes out is completed by the tune's own
acknowledgement, resolves against the wrong frame, and the old value goes back
into the model. That is the snap-back, and it happens only when a write is
involved, which is exactly the dividing line the operator describes.

Then something holds it there for thirty seconds. Second half of the same defect,
and the second thing to find: HM-DEC-107 made tuning writes their own category and
a settle or quiet window after a write is the obvious shape, but **read the code
rather than taking that from me.** I have been wrong three times on this bug and
twice from misreading a field.

`DECISIONS.md` has no entries for HM-DEC-096 to 133; they exist only as
`CLAUDE.md` §1 index rows. For that range read the commits.

## The one phase

**1. Reproduce it in a test, at the seam.** A frequency read in flight, a tune
write issued, the write's `FB` arriving before the read's value frame. Assert the
read does not complete on the acknowledgement and that the model does not take the
pre-write value. This should fail at `HEAD`. **If it passes, say so and stop** —
the suspect is wrong and I would rather hear that than have it built around.

**2. Find the second half.** Whatever keeps the stale value in place for about
thirty seconds when the frequency is otherwise observed within a tenth of a
second. Name it exactly: which code path, which interval, which commit introduced
it.

**3. Fix both.** A read completes only on the frame that answers it. A write does
not silence the reads that keep the display honest. If the settle window has a
real reason behind it — not reading back mid-tune is defensible — then it is
bounded by the write completing, not by a timer, and it never leaves an older
value on screen presented as current (§0.0).

**4. The diagnostics did not catch this either, and that is part of the work.**
There is no event for a tune write completing, none for a read being resolved by
an unexpected frame, and none for the model's frequency moving *backwards*. That
last one is the signature of this entire bug and it is one line: a frequency that
returns to a value it previously held, within seconds of a write, is not a normal
observation. Emit it, and put the link's answer in the operator's reach rather
than only in a file he has to upload.

**5. Leave the tests that would have caught it.** The seam test from step 1, and
an assertion that after a tune the model never holds a frequency older than the
tune itself.

**6. While you are in the same code**, `scope_output_requested` logged
`outcome: failed` with `reason: confirmed` and `unansweredCommands: 0` while 2,748
scope frames arrived. It called a write that plainly succeeded a failure, and the
two fields contradict each other. Same readback fault, same family, fix it here.

Then write `PROJECT_STATUS.md`, commit and push to `main` (HM-DEC-113), and
report.

## Withdrawn before delivery

**HM-DEC-136 was drafted and is withdrawn.** It would have moved the frequency to
the live poll on the theory that the poll was too slow. The operator's manual
tuning tracking in real time disproves it: the poll is already fast enough, and
that ruling would have changed nothing while being reported as a fix. **Do not
write it.** Nothing supersedes HM-DEC-109 or HM-DEC-050 in this order.

## Named and left (§12.6) — and this time, left

- The automatic `27 11` request, which `scopeShare` 0.50 says is half the cable
  and which HM-DEC-062 forbids in terms. Real, not this bug, and not this order.
- HM-OPEN-042's remaining rungs beyond step 6.
- The record sweep for rulings resting on a write outcome (Tim ruled option B).
- `DECISIONS.md` missing entries for 096 to 133.
- HM-DEC-135 and `CLAUDE.md` §9.6, still unwritten after two orders.
- Mode follow, favorites, the recent list.

## Reporting

`OUTPUT.md`, four sections (HM-DEC-106).

**Section one opens with the mechanism**: which frame completed which read, which
code path held the stale value, and the commit each landed in.

**Section two opens with one sentence: whether clicking a dot now moves the
display and leaves it moved.** Not what would prove it. What you changed and why it
holds.

**Stop and report. Do not start anything else.**

PROJECT: Hamlet
ISSUED: 2026-08-18

# Work order — make the frequency track the dial, and make our diagnostics able to see when it doesn't

**One phase. Nothing else is in this order.** Finish it, report, stop.

Gate first (HM-DEC-099): verify `PROJECT: Hamlet` against `PROJECT_CARD.md` in
this tree and against the prompt you were pasted. Any disagreement, stop.

---

## What is true, and it is not in question

**The operator turned the dial by hand. Hamlet took thirty seconds to follow. He
changed bands. Thirty seconds. He watched it, repeatedly, on his own radio.**

That is ground truth in the manner of `SHACK_FACTS.md`. **Do not ask him to
measure it again, do not gate any part of this order on a connect, and do not
write a report whose conclusion is that one evening would settle it.** He has
already run the experiment and told us the result. A session that hands this back
for measurement has failed the order.

**It worked before. The last two builds broke it. That makes it ours.**

## Why thirty seconds is not approximately the symptom but exactly the symptom

- **HM-DEC-109** moved the frequency from `Never` to **`Session`** cadence,
  amending HM-DEC-050 for a third field, as a backstop for a broadcast missed at
  startup.
- **HM-DEC-078** records that `RigStateMonitor` raises `StateChanged` on every
  poll cycle whether anything changed or not, at a **250 millisecond** live
  interval.

So the screen repaints four times a second, holding a frequency the session poll
last refreshed. **The display is current and the value is a minute old.** The only
mechanism that was ever going to make it instant is the transceive broadcast, and
the broadcast is not arriving.

Two candidates for why, both ours, and **this order does not need to know which**:

- **The scope stream is starving the link.** Hamlet has requested `27 11`
  automatically at connect since 1.8.0. From p. 19-14 via HM-DEC-062, waveform
  data length is 475 divided into 11 parts over USB, so a sweep is on the order of
  600 bytes; 115200 8N1 carries 11,520 bytes a second, which is about nineteen
  sweeps and nothing else. HM-OPEN-042 found that the readback could not confirm
  that write, so Hamlet reported it failing while it may have been succeeding.
- **Transceive is off at the radio.** `1A 05 00 71` reads it. Hamlet has never
  asked. It is not in `SHACK_FACTS.md`, so unlike USB Port and baud it is
  genuinely unknown.

## The one phase

**1. The frequency stops riding on Session cadence, and this is the fix that
cannot fail.** When broadcasts are not arriving, the frequency is read at live
rate. A frequency read is six bytes out and eleven back; at four times a second
that is under seventy bytes on a cable carrying eleven thousand. HM-DEC-050
rationed that bus deliberately and this is not what it was rationing against — a
frequency that is a minute old is the failure that ruling exists to prevent, not
an acceptable cost of it.

Whether the fast read is unconditional or only while no broadcast has arrived
recently is yours to choose, and the second is better if it costs nothing to
build. **What is not open is the display following the dial in under a second.**
If you judge this recordable under §12.1, record it; if it weighs two costs, hand
it back — but build it either way, because the operator's radio is unusable and
the alternative to building it is another day of not tracking.

**2. Stop requesting `27 11` automatically.** HM-DEC-062 already forbids it in
terms: *nothing turns the scope on, that is a write, and this ruling is reads
only.* Removing it restores a standing ruling rather than departing from one, so
it needs no new authority. Find which commit made it automatic and in which
version, and say so. Reading `27 10` and `27 11` to report what is on stays; that
is the read HM-DEC-062 allows.

If a ruling in HM-DEC-096 to 133 authorized the write, those have no entries in
`DECISIONS.md` and exist only as `CLAUDE.md` §1 index rows — read the commit,
report the conflict with HM-DEC-062, and still remove it. A ruling made against a
standing ruling on an assumption the arithmetic breaks is Tim's to re-take, and
his radio does not wait on that.

**3. Read `1A 05 00 71` at connect, and say plainly if the radio is not
announcing.** Do not write it. Turning transceive on is his, exactly as the scope
is. But an app that silently tracks at poll speed because the radio is not
broadcasting is an app that looks broken, and §0.0 wants the condition stated.

**4. Now the diagnostics, because they should have caught this and did not.**

Everything we knew about this failure lived in a telemetry file the operator had
to upload. The app itself showed a confident wrong number for two builds and said
nothing. Three things, and they are the durable half of this order:

- **Link health is visible in the app**, not only in the record. Whether the radio
  is announcing, when the frequency was last confirmed and by which mechanism, and
  what share of the inbound bytes is scope waveform. The counters from the last
  session are already in the build.
- **The frequency's own age is on screen when it is old.** HM-DEC-111 already
  ruled that a provenance label carries its age, after a capture sidecar asserted
  a freshness it did not have. The rig display does the same thing today and
  nobody had looked at it in that light. A frequency older than a second or two,
  while connected, is not presented as a bare number.
- **A self-check at connect that states the result.** Hamlet already knows how to
  ask whether the scope is on, whether transceive is on, how many bytes are
  arriving and of what kind. It should assemble that into one sentence the
  operator can read, in his terms, through `VoiceTests`.

**5. Leave the tests that would have caught it.** A frequency that has not been
confirmed within a live interval while connected is a failing test, not a display
detail. And an assertion that fails if Hamlet ever writes `27 11`, since that went
in against a standing ruling and shipped twice with nothing noticing.

Then write `PROJECT_STATUS.md`, commit and push to `main` (HM-DEC-113), and
report.

## Named and left (§12.6) — and this time, left

Do not work these. If you think one belongs here, say so in the report and do not
start it.

- HM-OPEN-042's remaining rungs, and whether `27 11` confirms.
- The record sweep for rulings resting on a write outcome (Tim ruled option B; a
  later order).
- `DECISIONS.md` missing entries for 096 to 133.
- HM-DEC-135 and `CLAUDE.md` §9.6, still unwritten.
- Mode follow, favorites, the recent list.

## Reporting

`OUTPUT.md`, four sections (HM-DEC-106).

**Section two opens with one sentence: whether the frequency on screen now follows
the dial in under a second.** Not what would prove it, not what a connect would
settle. What you built and why it works. If you cannot say yes, say exactly what
is still in the way and why the live read did not close it.

Section one leads with the commit that made `27 11` automatic and what the
diagnostics now show that they could not show yesterday.

You may still say plainly that no radio was connected (HM-DEC-093). What you may
not do is make that the answer.

**Stop and report. Do not start anything else.**

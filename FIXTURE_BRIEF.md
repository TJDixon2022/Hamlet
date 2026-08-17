**PROJECT: Hamlet**

# Work order: rebuild the CW fixtures so they test a receiver instead of a fantasy

HM-OPEN-018. Ruled 2026-08-17. Six phases per `CLAUDE.md` §12.3, reported per
§12.2.

**Read first:** `CLAUDE.md` (especially §0.0, §12.5), `SESSION_PROTOCOL.md`,
`OPEN_ISSUES.md`, `DECISIONS.md`. Development computer — COM1 is a simulator
and nothing in this session is evidence about the radio.

## Why this session exists

The existing CW fixtures are tone-or-exact-digital-silence. The transmit-mute
guard correctly reads digital silence as a muted receiver, so the reference
chain scores **zero** on every one of them. **A real receiver never hands over
digital silence.** Those fixtures encode a physical impossibility and Hamlet
passes them partly because it lacks the guard.

This is the second time in this repository: the scope parser and the fixtures
that certified it were green for months while the instrument discarded every
frame (HM-DEC-094). §12.5 exists because of both. **When a test passes and the
instrument disagrees, suspect the fixture.**

## The standing trap this session must not fall into

Eleven tests are failing. It would be trivial to make them pass by rewriting
what they test. **That is forbidden.** The terms below exist to make it
impossible, and the previous session was right to refuse the shortcut and hand
this back.

---

## Phase 1 — the generator

A deterministic, regenerable synthesizer under `tests/` producing WAV plus
sidecar, byte-for-byte reproducible from its parameters. No recorded audio, no
hand-tuning.

Every fixture is built from a real receiver's constraints:

- Noise shaped to the **500 Hz FIL2 passband** — a 350–870 Hz shelf, roughly
  30 dB above the out-of-band floor, matching the measured captures.
- **Mutes at −90 dBFS, never zero** (`CW_RECEIVE_BRIEF.md` §4). The real
  captures bottom out near −82 dBFS. Exact zero is the defect being removed.
- Tone around 615 Hz with slow drift of a few Hz over ten seconds.
- Keying envelope with realistic rise and fall, not square edges.

## Phase 2 — messages and tiers

Four messages, none using a real callsign — **`N0CALL` throughout**:

1. A callsign exchange (`CQ CQ DE N0CALL N0CALL K`) — session 2's classifier
   needs it.
2. A prosign set, sent properly with no inter-character space.
3. A character-coverage string.
4. **The tight-fist message**: dit 105 ms, dah 283 ms, inter-element gaps
   **65 ms — shorter than the dits** — character gaps 130 ms, word gaps
   280 ms. Measured off the real answering station. This is the shape that
   breaks any fixed 1:3:7 gap assumption, and nothing in the current suite
   contains it.

Each message at **three SNR tiers**: ~15 dB (easy), ~5 dB (the real operating
point), ~0 dB (the edge).

**QSB on the 5 dB tier only** — 0.7 Hz fade, up to 25 dB depth, as measured
off-air. Not a full matrix.

**The 0 dB tier asserts copy-or-refuse, never copy** (HM-DEC-097). Below 0 dB
the decoder refuses by ruling, so a fixture at −1 dB or lower asserts silence.

## Phase 3 — the QSK preamble fixture

One fixture carrying ~12 seconds of element-patterned mutes ahead of the
message: the operator's own full-break-in transmission as the receiver hears
it. Mutes at −90 dBFS with ~24 ms of T/R hang, keyed at 20 WPM.

Assertions: **zero characters emitted during the preamble**; the message
decodes after it; marks bordering the frozen spans render as placeholders and
never as letters. This is the truncated-evidence rule getting a committed test
instead of relying on two real recordings that cannot be regenerated.

## Phase 4 — the reference-scores-first gate

**No rebuilt fixture may judge Hamlet until `cwdecoder.py` scores well on it.**
Run the reference against every generated fixture and record the score
alongside the fixture. A fixture the reference cannot decode is a bad fixture,
not a Hamlet failure.

This is the property the old fixtures lacked: it makes the fixtures themselves
falsifiable. Build it as a checked-in step, not a one-time manual pass, so the
next person to add a fixture cannot skip it.

## Phase 5 — adjudicate the eleven, one at a time

The eleven currently failing tests stay in place under their existing names.
**New fixtures land under new names.** For each of the eleven, state
individually:

- whether it fails against realistic audio too, and
- whether the fault is Hamlet's or the fixture's.

`ItGoesQuietRatherThanInventingLettersInTheNoise` is now governed by
HM-DEC-097 — the refusal floor is 0 dB — and its bound follows the ruling
rather than the other way round. `TheDecoderReadsAsFarDownAsItDidBefore`
already passes again; do not disturb it.

Report the eleven as a table. No summary verdict.

## Phase 6 — DROP THIS ONE IF SHORT OF ROOM

Retire superseded old fixtures, **one at a time with a recorded reason each**,
never wholesale. A fixture deleted without a reason is evidence destroyed.

If dropped, say it was dropped. Leaving both sets in place is the safe state.

**If every phase completes, stop and report. Do not start the next work unit,
and specifically do not chase the `MVRR` shortfall** — that is the next
session and it depends on these fixtures existing.

---

## What comes next, recorded so it is not rediscovered

On capture 013347 the reference chain reads `MVRRVA3VRR` and Hamlet's settled
pass reads `MVRR` — two characters at high confidence, then it stops partway
through the callsign. Tim ruled that this is chased **after** the fixture
rebuild, because the stop occurs on a signal with 0.7 Hz QSB and a fist whose
gaps are shorter than its dits, and both conditions are specified in phase 2.
The cause may be visible in the new fixtures without hunting, and any fix
written now would carry a regression test built on the wrong noise model.

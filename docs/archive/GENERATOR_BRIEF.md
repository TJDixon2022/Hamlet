**PROJECT: Hamlet**

# Work order: fix the generator, then measure what it was hiding

HM-DEC-101 through 104, ruled 2026-08-17. Six phases per `CLAUDE.md` §12.3,
reported per §12.2. Development computer; nothing here is evidence about the
radio.

**Read first:** `CLAUDE.md` (§0.0, §12.5), `SESSION_PROTOCOL.md`,
`FIXTURE_BRIEF.md`, `OPEN_ISSUES.md`, `DECISIONS.md`.

## Why this session exists

The scoring gate worked and the first thing it falsified was a fixture. Three
were held out — `tightfist-easy`, `tightfist-working`, `qsk-preamble` — and one
of those is a tight fist at the **easy** tier. The reference reads the *real*
tight fist at 0.74–1.00 confidence off capture 013347. A control that works on
real audio and fails on synthesized audio of the same shape means **the
synthesis is wrong.** Lowering the gate was rejected (§12.5, HM-DEC-101).

Everything else in this session waits on that, because a measurement taken
against unproved audio is a phantom.

---

## Phase 1 — fix the tightfist synthesis against the real capture

Capture 013347 is the control. Measured from it: tone ~615 Hz drifting a few
Hz, dit 106 ms, dah 283 ms (ratio 2.8, ≈11.4 WPM), inter-element gaps 60–70 ms,
character gaps 112–155 ms, word gaps 220–320 ms, QSB ~0.7 Hz to 25 dB depth,
0–5 dB SNR in the 500 Hz passband, mutes bottoming near −82 dBFS.

Compare `tightfist-easy` against those parameters until **the reference scores
it as well as it scores the real audio.** Likely suspects, in order: keying
envelope rise and fall shape, the gap distribution, whether the drift is
applied per-element rather than continuously, and the noise shaping across the
350–870 Hz shelf.

Fix the generator, not the fixture, and not the gate.

## Phase 2 — clear the other two hold-outs and re-run the gate

`tightfist-working` and `qsk-preamble` on the corrected generator. Re-score
every one of the thirteen: a generator change invalidates every score taken
under the old one. **Report the full table before and after** — a score that
moved is information about the generator.

Three known faults the gate caught last session are fixed and stay fixed: the
character boundary overwriting the last edge instead of appending, 18 WPM
compressing the window ratio to 2.45, and 25 dB of fade at 0.7 Hz across 15 s
deleting rather than fading. And the scorer's own defect — the reference
printing the unresolved-character glyph killed the child process on the Windows
console codepage, reporting well-decoded fixtures as unreadable. **The gate was
failing in the direction that destroys evidence.** Make that impossible, not
merely fixed: the scorer never fails silently, and an unreadable result is
distinguishable from a bad score.

## Phase 3 — re-measure the settled pass (HM-DEC-102)

The settled pass reads worse than the provisional tip on the 5 dB fading tier.
That is the opposite of why it exists — but the reference scored only 52–53%
on that tier, so nothing has been proved about Hamlet yet.

Re-measure on the corrected fixtures. If the reference's score rises and
Hamlet's gap closes, it was the generator. **If the gap survives on sound
audio, report the number and stop — do not fix it this session.** It becomes
its own work order. HM-OPEN-017's labelled-approximation fallback stays
available and is not taken on this evidence.

## Phase 4 — 25 WPM (HM-DEC-103)

Generate all three tiers at 25 WPM, gate them, and retire `clean-25wpm` **only
once its replacement passes** — one retirement, with its reason recorded.

**New failures are expected and wanted.** At 25 WPM the window's 30-element
floor binds rather than its 2.5-second one, and the ~4 s ceiling is nowhere
near; no test has ever exercised that path. Report what breaks; fix only what
is clearly a defect rather than a discovery.

## Phase 5 — segment joining, and the fixture four built phases have never had (HM-DEC-104)

Teach the generator to concatenate segments. Segments are generated complete,
each with its own keying envelope, and **joined across a gap, never
mid-character** — the seam must be a signal, not an artifact the decoder can
learn.

Then build the two-station fixture: one station at ~11 WPM around 615 Hz,
followed by a second answering at ~22 WPM at a different pitch, at the 5 dB
tier.

**This is the first committed test of four capabilities built on rulings
alone**: clock loss on discontinuity, the previous clock retained as a
candidate, tracker switching on keying structure, and the speed-change
annotation. Assert each explicitly, including that the switch does not occur
mid-character.

Then adjudicate the two outstanding tests:
`TheSpeedEstimateFollowsAChangeWithinAFewCharacters` against a genuine
mid-message speed change, and `ClearingTheTranscriptLeavesTheDecoderAlone`
re-pointed at a realistic fixture instead of a noiseless one.

## Phase 6 — DROP THIS ONE IF SHORT OF ROOM

Re-adjudicate the eleven against the corrected fixtures and update the phase-5
table from the previous session. Several entries were decided against audio
now known to be defective — `AFadingSignalComesBackRatherThanStayingDead` was
attributed to Hamlet on exactly that evidence and may not survive re-testing.

If dropped, say so; the old table stands with a note that it predates the
generator fix.

**If every phase completes, stop and report. Do not chase the `MVRR`
shortfall** — it is the next work order, and HM-DEC-101 makes phase 1 its
leading hypothesis. Report whether the corrected tightfist fixtures moved it,
but do not pursue it.

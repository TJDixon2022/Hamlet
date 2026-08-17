# Work order: make Hamlet read CW the operator's ear cannot

**Session scope.** Amend the receive chain in `src/Hamlet.RadioEngine/Cw` so it
decodes the signal class proven present in the 2026-08-17 off-air captures:
weak keyed CW at 0–5 dB SNR in the radio's 500 Hz passband, deep slow QSB,
a tight fist, an interfering carrier 12 Hz away, and the operator's own
full-break-in transmission chopping the audio for the first two-thirds of the
recording. A reference implementation (`cwdecoder.py`, in this repo alongside
this brief) has been validated against both captures and is the semantic
specification for this session. Port its behavior, not its structure.

This brief supersedes nothing. It amends the HM-DEC-048 chain — Goertzel bank,
adaptive gate, rolling speed derivation, table allowed to say no — with four
changes and three refusal rules. §0.0 binds throughout: every change below
creates a new place the decoder is allowed to say nothing, and none creates a
place it guesses.

---

## 1. Ground truth this session is built on

Two 30 s WAV captures, 48 kHz 16-bit mono, taken from the IC-7300 USB codec on
2026-08-17 ~01:34–01:37 UTC, radio on 14.055 MHz CW, FIL2 500 Hz, AGC FAST,
full break-in, keyer 20 WPM. Measured by offline analysis, decode confirmed by
hand from raw element timings:

**Capture 1 (`cw-2026-08-17-013347.wav`).**
- 0–19 s: the operator's own CQ at 20 WPM. Full QSK mutes the USB RX audio on
  every key-down: audio alternates between band noise near −15 dBFS and
  **digital silence**, mute depth 50–84 dB, mute durations = the operator's
  elements plus ~24 ms of T/R hang. The old chain read this as 1,211 elements
  of gate chatter.
- 19–30 s: an answering station. **615 Hz** audio pitch, drifting a few Hz.
  Dit **106 ms**, dah **283 ms** (ratio 2.8, ≈11.4 WPM). Inter-element gaps
  **60–70 ms — shorter than the dits**. Character gaps 112–155 ms, word gaps
  220–320 ms. Envelope SNR **12–20 dB in a 20 Hz detection bandwidth**, which
  is **0–5 dB in the 500 Hz passband**. QSB rate ~0.7 Hz, fade depth to 25 dB.
  Decodes to `VA3VRR` sent twice (preceded by a fragment). Every element is
  individually visible in a 20 Hz-bandwidth envelope.
- A weak steady carrier also sits near 614 Hz through parts of the tail.

**Capture 2 (`cw-2026-08-17-013622.wav`).**
- 0–12 s: operator's CQ again (same QSK muting).
- 12–30 s: the same station near 610 Hz, but S4 instead of S7, preamp off,
  ~10 dB worse, gate contrast only ~9 dB, and a second razor-thin carrier at
  **601.0 Hz** from ~21 s. Neither careful offline analysis nor the reference
  decoder produced a copy worth standing behind. Correct output for this
  capture is low-confidence dimmed characters and placeholders, or nothing.

**Why the ear beat the old chain:** an experienced ear is roughly a 50 Hz
filter with pattern integration. The chain was listening in a far wider
bandwidth and its floor/peak trackers had been destroyed by the QSK mutes
before the answer arrived. Both are fixed below; the goal of this session is
the reverse inequality — Hamlet copies what the ear cannot.

---

## 2. The four amendments

### 2.1 Transmit-mute guard (fixes the largest failure first)
Detect the operator's own transmission from the audio itself: broadband RMS
over 10 ms frames; below **−60 dBFS** is a mute (a band cannot do that through
a 500 Hz filter; only the T/R path can). While muted and for **150 ms** after
recovery (AGC settle): freeze the gate's floor and peak trackers exactly where
they were, feed the element pipeline nothing, and mark the span *frozen* in
the decoder's record. Cross-check against `TransmitStatus` from the rig model
(HM-DEC-050) when connected — but the audio tripwire must work standalone,
because the decoder also runs on WAV files with no radio attached.
Additionally clamp the floor tracker at an absolute minimum of **−75 dBFS**
under all circumstances: the floor must never chase digital silence, whatever
the cause.

### 2.2 Two-stage Goertzel: acquire wide, detect narrow (the filter)
Stays within the no-FFT ruling. Stage one, acquisition: the existing coarse
bank across 300–900 Hz (~25 Hz steps, ~25 ms windows), but score candidate
bins by **envelope spread** (p90 − p30 of the bin's dB envelope over active
audio), not by mean power — a keyed tone has spread; a steady carrier and
noise do not. Stage two, detection: center a fine bank on the winner — **bins
5 Hz apart spanning ±15 Hz, 50 ms window, 10 ms hop** (≈20 Hz ENBW at the
8 kHz internal rate: N≈400, hop≈80). The detection envelope is the max across
the fine bank per hop, which tracks drift for free; re-center the bank when
the peak walks to an edge bin. This narrowing is worth ~14 dB over the 500 Hz
passband and is the single change that makes the 0–5 dB station decodable.
Bandwidth follows the decoded speed: ~40 Hz ENBW before clock lock or above
~18 WPM, ~20 Hz once locked at slow speeds.

### 2.3 Gate: threshold from clustering, referenced to the fade
Per ~3 s window over the detection envelope in dB: fit two clusters
(two-means, ~15 iterations — allocation-free, ten lines). Threshold is the
cluster midpoint, recomputed per window so it rides QSB down and back.
**6 dB hysteresis** between the on and off decisions so a fade dip inside a
dah does not split it. De-glitch at 20 ms before clock lock, **0.4 × dit**
after.

### 2.4 Clock and gaps: proven from the signal, assumed from nothing
Derive the element clock by two-means clustering the mark durations.
**Accept only if the dah/dit ratio lands in 2.5–3.8** (the IC-7300 itself can
only key 2.8–4.5, Full Manual p. 4-21) and the dit lands in 30–350 ms
(≈4–40 WPM). Classify the three gap lengths by clustering **the gaps
themselves** — never by fixed multiples of the dit. The captured station's
inter-element gaps are shorter than its dits; a 1:3:7 assumption misreads a
real operator on the first night out.

---

## 3. The three refusal rules (§0.0, executable)

1. **Contrast refusal.** Gate cluster separation under 6 dB in a window →
   no gating in that window, nothing emitted from it.
2. **Clock refusal.** Mark durations that do not cluster at a valid ratio →
   no clock, nothing emitted, and the plain-language note says the tone was
   found but the timings did not look like Morse (HM-DEC-048's note rules
   apply: measurements, not diagnosis).
3. **Truncated-evidence refusal.** Any mark whose start or end lies within
   ~60 ms of a frozen span was not fully observed: exclude it from the clock
   fit and render its character as the placeholder, never a letter. (Without
   this rule the slivers of the answering station audible *between* the
   operator's own QSK elements decode as a confident string of E and T —
   the reference implementation reproduced exactly that defect before the
   rule, and it is the most seductive wrong output this feature can produce.)

Also: an unbroken tone longer than ~8 dahs is classified **carrier** and never
enters the element pipeline. The contested-signal veto (HM-DEC-048) is
unchanged and still applies — capture 2's 601 Hz carrier 9 Hz from the station
is its test case.

Confidence stays as ruled: worse of timing margin and SNR margin, nothing
raises it. Timing margin per element from distance to the dit/dah boundary;
SNR margin from gate contrast (6 dB → 0, 20 dB → 1).

---

## 4. Fixture (HM-DEC-048: synthesized, owned by no one)

The off-air WAVs must not enter `tests/fixtures/cw` — they carry a real
station's transmission. Synthesize a regenerable fixture from the measured
recipe instead, request file beside it as always:

- 8 kHz 16-bit mono, ~30 s. Tone 615 Hz with slow drift (±3 Hz over 10 s).
- Message at dit 106 ms, dah 283 ms, gaps 65/130/280 ms — use a neutral text
  (e.g. `TEST DE N0CALL`), not the real callsign.
- Two-path QSB at 0.7 Hz, fade depth 25 dB.
- Noise shaped to a 500 Hz passband (350–870 Hz shelf), SNR 3 dB in-passband.
- Interfering steady carrier 12 Hz below the tone at −10 dB relative.
- Preamble: 12 s of the noise+tone chopped by QSK-style mutes (element-patterned
  silence at −90 dBFS, 24 ms hang) to exercise the guard.

**Assertions:** zero characters emitted during the preamble; the message
decodes with ≥0.5 confidence on ≥80 % of characters; truncated slivers render
as placeholders; the carrier renders as carrier; byte-for-byte regeneration.
A second variant at 10 dB worse SNR must produce only low-confidence or
placeholder output — a test that the refusals fire is as load-bearing as the
test that the decode succeeds.

---

## 5. Record-keeping in the same delivery

- `OPEN_ISSUES.md`, two entries at severity worth fixing soon (§0.0.1 both):
  capture metadata wrote `frequency 7030000 / band 40m` while the rig was on
  14.055 MHz — capture headers must come from the rig model, not configuration;
  and `toneHz` reported 575/600 against a measured 614 — the tracker or the
  metadata writer is misreporting the note.
- `DECISIONS.md` entry for the amendments above once implemented, referencing
  HM-DEC-048 as amended-not-superseded, and this brief.
- Element/character counters stay (they diagnosed this failure); add a counter
  for spans frozen by the mute guard.

## 6. Constraints carried over, so nothing regresses

Goertzel only, no FFT (standards table). Determinism below the UI: same WAV
in, same text out; no wall clock in the decoder. Audio behind `IAudioSource`;
`IsSimulated` remains get-only. Nothing anywhere raises a confidence score.
American spelling. Tests name what they prove.

**Definition of done:** run the engine against the two off-air WAVs locally
(they stay out of the repo). Capture 1 must print `VA3VRR` twice at high
confidence with placeholders before it and silence during the CQ. Capture 2
must print only dimmed low-confidence output or placeholders — if it prints a
clean sentence from that audio, it is guessing, and it has failed.

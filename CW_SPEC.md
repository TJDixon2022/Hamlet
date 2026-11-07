**PROJECT: Hamlet**

# CW receive decoder — specification, v1 (draft for freeze)

| Field | Value |
|---|---|
| Version | 1.0-draft |
| Date | 2026-09-25 |
| Status | Awaiting stage 7 reconciliation and Tim's commit. Until then nothing in this file is a ruling. |
| Companion | `CW_REQUIREMENTS.md` — the requirements and their verification table. This file holds the vocabulary that file cites. |
| Supersedes | `CW_SPEC_SCOPE.md`, `CW_SPEC_DEFINITIONS.md`, `CW_REQ_CANDIDATES.md` (stage documents; delete after freeze). |
| Change rule | After freeze, this file changes only by a ruling recorded in `DECISIONS.md` that names the section changed. Sessions may not edit it. |

---

## 1. Purpose

The decoder has been improved for eleven weeks by chasing wherever the last
capture failed. Each session recentred on a symptom, and the rulings that
resulted are true but scattered, mixed with design choices, and measured
against the previous build rather than a fixed target. This specification and
its requirements give every arbiter iteration a fixed point: a work
instruction names the requirement ids it means to move, and the outcome
reports each as met, not met, or not yet measurable.

## 2. The system under specification

**Name.** The CW receive decoder: everything between audio samples arriving
from `IAudioSource` and the decoded record leaving the engine.

**Input.** A stream of audio samples at a known sample rate, from a receiver
whose passband, mode and CW pitch setting may be known from rig state or may
be unknown.

**Output.** A decoded record consisting of:

- characters, each carrying a confidence class (sure / dim / placeholder);
- word boundaries;
- the running speed estimate with its proof state (proved / hypothesis / none);
- the running pitch with its proof state; **retired 2026-10-05 (work instruction 545, HM-DEC-249): the pitch proof is removed with the tone tracker and the survey that set it**;
- the signal measurements the decoder acted on (tone level, noise reference,
  passband used), each with provenance and age;
- an explicit "nothing read" state, distinct from "nothing there".

**Outside the boundary.** The terminal that renders the record, the scanner
that moves the dial (FG-009), the transmit path, the keying sweep, the
capture and sidecar writer, the Explorer, the annunciator, the contact-state
model and its callsign claiming (HM-DEC-073, HM-DEC-076), mode
identification beyond "no Morse timings found", rendering choices (colours,
dimming style, placeholder glyph), and code tables other than ITU-R M.1677-1.
The specification says what the decoder must *report* so those surfaces can
be honest; it does not say how they render it.

Confirmed by Tim 2026-09-25.

**The reader finds the sequence, 2026-10-27 (the owner's ruling on HM-DEC-295).** Across the rig's passband a narrow lane every 25 Hz mixes the audio to its pitch and low-passes it at the width its sender's speed asks for, 15, 24 or 40 Hz, the nearest to 1.5 over his dit; every 5 ms it takes the lane's level in dB. Over the lane's last 12 s, every 2 s, an explicit-duration Viterbi finds the likeliest whole path through six states, dit, dah, the gap inside a letter, the gap between letters, the gap between words, and nobody keying, each with the durations and the two levels learned from the sender's own decode in three passes; a lane that has read its sender keeps his rhythm through a pause, and reads cold as well where its kept rhythm stops reading him. A lane prints only where its path's pattern is Morse: a dah share between 0.2 and 0.65, its dahs tight, key-down standing over key-up, and its gaps in order; of the lanes reading one station the best-separated prints, letters committed 2 s behind the newest audio, timed less the lane's delay. `CwSequenceReader`, behind `CwRules.SequenceReader`, on by default, is the only path to the printer: the grid's marks, the bank's handover and the column finder feed nothing, and the grid stays for the scope. The finders it replaces, their handover, shadows and holds, are recorded in `DECISIONS.md` from HM-DEC-273 to HM-DEC-294 and in this file's history.

## 2.1 What a hand sends (HM-DEC-296)

W1AW reads because it is a machine: gaps of 35, 100 and 240 ms, 1 : 2.9 : 6.9 dits, with a 10 to 90 per cent spread of 5 ms, so one gap in 889 lies near any class boundary. No measured hand keys that way. Each recipe's gaps are in its own dits and each spread is the standard deviation of the log, from the owner's analysis of 2026-11-03; the wild bank (`CwWildBank`) sends them.

| Recipe | Measured from | dit ms | dah/dit | dit spread | dah spread | inside (spread) | letter (spread) | word (spread) | habits |
|---|---|---|---|---|---|---|---|---|---|
| `mobile` | Bryan KB2UZK/M | 65 | 3.4 | 0.29 | 0.12 | 1.6 (0.10) | 4.4 (0.30) | 13.7 (0.40) | digits as words; short words together |
| `bug` | Ken N3AFS | 80 | 2.9 | 0.37 | 0.19 | 1.7 (0.23) | 4.3 (0.18) | 7.6 (0.29) | none |
| `farnsworth` | `144020` | 75 | 2.9 | 0.28 | 0.05 | 0.9 (0.05) | 5.3 (0.35) | 21 (0.45) | none |
| `tight` | `121414` | 58 | 2.5 | 0.25 | 0.16 | 1.0 (0.08) | 3.6 (0.25) | 8.5 (0.40) | none |
| `steady` | `200157` | 75 | 2.8 | 0.10 | 0.01 | 1.1 (0.08) | 4.0 (0.30) | 11.8 (0.25) | none |
| `machine` (control) | the fist bank's keyer | 80 and 60 | 3.0 | ±15% uniform | ±15% uniform | 1 (±20% uniform) | 2 to 3.5 uniform | 4 to 6 uniform | none |

**Three mechanisms cost a hand its letters, and none of them is the marks.** The cold inside-gap range, 0.4 to 1.7 dits, is wrong for a 1.6-dit hand, and the learning locks the first wrong pass in. Some of a hand is truly ambiguous by timing: Bryan spaces `579` like words and `I AM` like a letter, and no one boundary reads both. And a minority is swing in the marks themselves, Ken's dit log-spread 0.37 against W1AW's 0.09.

## 3. Four layers, kept apart

| Layer | Holds | Lives in | Changed by |
|---|---|---|---|
| Reference definitions | What Morse is, what a WPM is, what an HF channel does, how SNR is stated. Borrowed and cited. | This file, §6–§12 | Nobody; transcribed and re-verified against the vendored source. |
| Requirements | What the decoder shall do at its boundary. Behaviour only. | `CW_REQUIREMENTS.md` §A–§K | Tim, by ruling. |
| Verification | Per requirement: corpus, condition profile, metric, threshold, unknown-versus-wrong rule. | `CW_REQUIREMENTS.md` §V | Tim, by ruling. Thresholds may be `TBD` while marked. |
| Design constraints | How the current decoder happens to work. | `DECISIONS.md` | Sessions, freely, while requirements still verify. |

**The test for which layer a sentence belongs in:** if a completely different
decoder architecture could satisfy it, it is a requirement. If it names a
mechanism, it is a design constraint.

## 4. Requirement record and statement form

Each requirement carries `id`, `statement`, `rationale`, `source`,
`priority` (must / should / later), `status` (candidate / ratified /
superseded-by), and a row in the verification table. Ids are `HM-REQ-###`,
never reused or renumbered (ruled by Tim 2026-09-25; extends CLAUDE.md §2.1).

Statements use the EARS patterns (Mavin et al., 2009):

- Ubiquitous: *The decoder shall …*
- Event-driven: *When ⟨event⟩, the decoder shall …*
- State-driven: *While ⟨state⟩, the decoder shall …*
- Unwanted behaviour: *If ⟨condition⟩, then the decoder shall …*
- Optional: *Where ⟨feature⟩, the decoder shall …*

One "shall" per requirement. A statement that needs two verifications is two
requirements. A requirement that cannot name its verification stays a
candidate.

Every condition is a named profile from this file (`CH-*`, `TX-*`, `INT-*`,
`IMP-*`) and an SNR in the reference bandwidth (§8), never prose.

## 5. Measurement conventions (confirmed 2026-09-25)

1. **SNR is stated in a 2500 Hz reference bandwidth.** In-passband figures
   may also be reported, with the passband beside them. A figure with no
   bandwidth is not a figure.
2. **Accuracy is character error rate by edit-distance alignment** against
   truth, reported separately for sure characters and for all characters.
3. **Placeholders and dim characters are never scored wrong.** A sure
   character that disagrees with truth is the §0.0 failure and has its own
   column.
4. **Confidence is specified as selective classification:** a maximum error
   rate among sure characters *at* a minimum coverage, so "dim more" cannot
   satisfy a requirement.
5. **Speed is WPM by the PARIS convention**; timing is in units of the dit at
   that speed.
6. **Truth is graded** independent / inferred / synthetic, and every accuracy
   figure names its grade.

---

## 6. The code — ITU-R M.1677-1

**Source.** Recommendation ITU-R M.1677-1, *International Morse code* (2009).
The only authority this specification recognises for what a character is.
To be vendored (§12 item 1); the runtime table is generated from the vendored
file, not typed into code.

### 6.1 Nominal timing

| Element | Units |
|---|---|
| dit | 1 |
| dah | 3 |
| gap between elements of a character | 1 |
| gap between characters | 3 |
| gap between words | 7 |

One unit is the dit length. These are what a nominal keyer (TX-ITU) sends;
§10 records how far real senders depart.

### 6.2 Character set

The v1 table is, by ruling of 2026-09-25:

- the 26 letters A–Z and the accented É;
- figures 0–9;
- the punctuation M.1677-1 lists: full stop, comma, colon, question mark,
  apostrophe, hyphen, fraction bar `/`, brackets, inverted commas, double
  hyphen `=`, cross `+`, multiplication sign (same pattern as X), commercial
  at `@`;
- the M.1677-1 procedural signals: understood `VE`, error (eight dits),
  starting signal `CT`, end of work `SK`, invitation to transmit `K`,
  wait `AS`;
- the amateur prosigns **KN, BK, CL** from a cited ARRL source (§12 item 9).
  `AS` is already in M.1677-1 and is listed there.

Nothing else. Where one pattern has two names (`=` / `BT`, `+` / `AR`) it is
one symbol with a naming choice, not two entries. A pattern not in the table
is a placeholder, never the nearest letter. Non-Latin extensions are out of
scope for v1.

**Open behaviour (TBD, needs ruling):** whether the decoder acts on the error
signal (deleting the preceding word from the record) or only renders it. Until
ruled, it is rendered as the symbol and nothing is deleted.

### 6.3 Speed — PARIS

WPM is defined by the word PARIS, 50 units including the trailing word space:

- dit (ms) = 1200 / WPM
- element rate (baud) = WPM × 50 / 60

| WPM | dit (ms) | dah (ms) | char gap (ms) | word gap (ms) |
|---|---|---|---|---|
| 5 | 240 | 720 | 720 | 1680 |
| 10 | 120 | 360 | 360 | 840 |
| 13 | 92 | 277 | 277 | 646 |
| 20 | 60 | 180 | 180 | 420 |
| 25 | 48 | 144 | 144 | 336 |
| 30 | 40 | 120 | 120 | 280 |
| 35 | 34 | 103 | 103 | 240 |
| 40 | 30 | 90 | 90 | 210 |
| 45 | 27 | 80 | 80 | 187 |

The CODEX word (60 units) is not used. Citation to vendor: §12 item 4.

### 6.4 Farnsworth

Characters sent at one speed with the gaps between characters and words
stretched so the overall rate is lower. Described by two numbers: character
speed and overall speed. ARRL definition to vendor (§12 item 5). A Farnsworth
sender breaks 1:3:7 by design; the traffic net measured in HM-DEC-115 (57 ms
dit, gaps 40 / 240 / 500 ms) had an element gap shorter than the dit and a
character gap six times the element gap. This is the norm on the air.

---

## 7. The emission — bandwidth

**Source.** ITU-R SM.328 and SM.1138, emission class A1A. Necessary bandwidth
B_n = B × K, B in baud, K = 5 on fading circuits and 3 on non-fading circuits
**[verify — §12 item 3]**. At 25 WPM (≈21 baud) that is ≈100 Hz on a fading
path; at 45 WPM (≈37 baud) ≈190 Hz.

Amateur keying adds a rise/fall of a few milliseconds (ARRL Handbook key-click
guidance, ~5 ms **[verify]**). A detector whose analysis window exceeds the
rise time reads every mark short by a fixed amount (HM-DEC-119's measured
15–20 ms), so the receive analysis bandwidth and the emission bandwidth are
related quantities and both are stated when relevant.

**Consequence of the 45 WPM ceiling:** a 27 ms dit sits at the edge of a
500 Hz passband and of a 20 ms analysis window. Requirements at the top of
the speed range therefore constrain analysis bandwidth, and are verified
synthetically until a real capture above 30.6 WPM exists.

---

## 8. Signal-to-noise

### 8.1 Reference bandwidth

SNR is stated as if the noise were measured in **2500 Hz** (the WSJT-X
convention, used by AG1LE for published CW curves).

    SNR_2500 = SNR_BW − 10·log10(2500 / BW)

| Measurement bandwidth | Offset to 2500 Hz |
|---|---|
| 500 Hz (IC-7300 FIL2) | −7.0 dB |
| 250 Hz (FIL3) | −10.0 dB |
| ~50 Hz (30 ms Goertzel ENBW) | −17.0 dB |

Every SNR in the project record before this date is in-passband or
in-detector and is re-labelled when it enters a requirement. HM-DEC-097's
"0 dB in the passband" is **−7 dB reference**.

### 8.2 What the reference is

Noise is measured inside the receiver passband the decoder is listening
through, beside the tone, while the tone is keyed. When the passband is
unknown the figure is "unknown", never measured in the skirt.

### 8.3 Sensitivity floor and its speed scaling (ruled 2026-09-25)

The **sensitivity floor** is −7 dB reference at 20 WPM on CH-AWGN with
TX-ITU. It scales at **3 dB per doubling of speed** (matched-filter
energy-per-element): ≈ −10 dB at 10 WPM, ≈ −4 dB at 40 WPM, ≈ −13 dB at
5 WPM, ≈ −3.5 dB at 45 WPM. One measured anchor covers the ladder; the
scaling is verified at two more speeds.

---

## 9. Channel profiles — `CH-*`

### 9.1 ITU-R F.1487 Annex 3

Two independently fading paths of equal power, Gaussian Doppler spectrum,
defined by differential delay (ms) and frequency spread (Hz). Values to vendor
(§12 item 2); only CH-LM was read from the source this session.

| Profile | Latitude band, condition | Delay (ms) | Spread (Hz) | Status |
|---|---|---|---|---|
| CH-LQ | Low, quiet | 0.5 | 0.5 | [verify] |
| CH-LM | Low, moderate | 2 | 1.5 | confirmed |
| CH-LD | Low, disturbed | 6 | 10 | [verify] |
| CH-MQ | Mid, quiet | 0.5 | 0.1 | [verify] |
| CH-MM | Mid, moderate | 1 | 0.5 | [verify] |
| CH-MD | Mid, disturbed | 2 | 1 | [verify] |
| CH-MDV | Mid, disturbed NVIS | — | — | [read from Annex 3] |
| CH-HQ | High, quiet | 1 | 0.5 | [verify] |
| CH-HM | High, moderate | 3 | 10 | [verify] |
| CH-HD | High, disturbed | 7 | 30 | [verify] |
| CH-AWGN | No fading, white noise | — | — | project baseline |

The profiles are named by latitude band only. Hamlet serves users anywhere;
nothing in this specification refers to a geography.

### 9.2 Which profiles bind (ruled 2026-09-25)

| Tier | Profiles | Allowance against the CH-AWGN floor |
|---|---|---|
| must | CH-LM, CH-MM | +3 dB |
| must | CH-HM | +6 dB (10 Hz spread is auroral flutter; a narrowband detector feels it more) |
| should | CH-LD, CH-MD, CH-HD | TBD until measured |

### 9.3 Impairment and interference vocabulary

| Id | Term | Definition |
|---|---|---|
| IMP-QSB | Fading | Slow amplitude fading; the CH-* profiles, or a sinusoidal/Rayleigh envelope with depth and period stated where a named profile is more than the test needs. |
| IMP-QRN | Static | Impulsive noise; stated as impulse rate and peak-to-noise ratio. |
| IMP-FLUTTER | Auroral flutter | Rapid amplitude modulation at tens of hertz; CH-HM / CH-HD approximate it. |
| INT-ADJ(Δf, ΔdB, WPM) | Adjacent station | A second CW signal Δf hertz away, ΔdB relative to the wanted one, at its own speed. |
| INT-COCHAN | Co-channel station | A second signal within the detector bandwidth of the wanted one. The veto case. |
| INT-CARRIER(Δf, ΔdB) | Unkeyed carrier | A steady tone. The "loudest is not keyed" case. |
| INT-PILEUP | Pileup | Three or more stations inside the passband. The "emit nothing" case. |

### 9.4 Existing audio-domain simulators

PathSim (AE4JY) implements the Watterson model on audio with the CCIR
profiles built in; Morse Runner (VE3NEA) generates QSB, QRN, QRM, flutter and
sloppy senders on audio. Neither is adopted as code; both are references for
what the generator must reproduce. Availability and licence to verify (§12
item 7).

---

## 10. Sender profiles — `TX-*`

Reference for hand-sent variability: B. Gold, "Machine Recognition of
Hand-Sent Morse Code", IRE Trans. Information Theory, March 1959 **[verify
volume/pages — §12 item 6]**. Parameter ranges come from Gold and from this
project's captures.

| Id | Name | Definition | Tier (ruled 2026-09-25) |
|---|---|---|---|
| TX-ITU | Nominal keyer | Exactly 1:3:1:3:7. The KD0UN capture (3.06 / 2.89 / 6.92 units) is a real example. | must |
| TX-KEYER-W | Weighted keyer | Electronic keyer with weight and ratio adjusted: ratio 2.5–3.5, gaps ±30 % **[verify against WinKeyer documentation]**. | must |
| TX-FARNS | Farnsworth | Character speed above overall speed; character gap 3–7 units at character speed, word gap longer. HM-DEC-115's traffic net is the vendored example. | must |
| TX-TIGHT | Tight fist | Element gaps shorter than the dit; character gaps compressed. The 013347 capture (HM-DEC-101). | must |
| TX-BUG | Semi-automatic key | Mechanical dits, hand dahs; ratio 3.5–5, dits short and fast, gaps variable. | should |
| TX-STRAIGHT | Straight key | Everything hand-timed; ratio and gaps drift; speed wanders. | should |
| TX-SLOPPY | Poor sender | Morse Runner's "LID": inconsistent ratios, missing gaps, errors and corrections. | later — honesty rule only |

"A steady fist" means TX-ITU or TX-KEYER-W. A requirement naming no sender
profile applies to every must-tier profile.

---

## 11. Metrics — `MET-*`

| Id | Metric | Definition |
|---|---|---|
| MET-CER | Character error rate | (insertions + deletions + substitutions) / characters sent, by Levenshtein alignment to truth. |
| MET-CER-SURE | Sure error rate | MET-CER over characters emitted as sure; dim and placeholders excluded, not counted wrong. **The §0.0 number.** |
| MET-INVENTED | Invented share | (sure insertions + sure substitutions) / characters sent. |
| MET-COVERAGE | Coverage | Sure characters emitted / characters sent. |
| MET-RC | Risk–coverage curve | MET-CER-SURE against MET-COVERAGE as the confidence threshold sweeps; one curve per condition profile. |
| MET-CAL | Calibration | Reliability table with three bins (sure / dim / placeholder): observed accuracy per bin against its stated rate. |
| MET-WBE | Word boundary error | Word gaps inserted or deleted relative to truth / words sent. Scored on boundaries alone, separately from MET-CER. |
| MET-TACQ | Acquisition time | Seconds from first element on the air to first sure character, under a named run-up. |
| MET-LAT | Latency | Seconds (and characters) behind the air at which a sure character is emitted, steady state. |
| MET-WPM-ERR | Speed error | (estimated − true) / true, percent, after acquisition. |
| MET-PITCH-ERR | Pitch error | estimated − true, hertz, after acquisition. |
| MET-SENS | Sensitivity | Lowest SNR_2500 at which MET-CER-SURE < 1 % with MET-INVENTED = 0, on CH-AWGN, per speed. |

**Stated confidence rates (ruled 2026-09-25):** sure ≥ 99 % right;
dim ≥ 70 % right; placeholder carries no claim.

### 11.1 Human copy standards, for plain-language reading

| Standard | Defines |
|---|---|
| FCC amateur code element (historical) | One minute of solid copy out of five at the element speed. **[verify — §12 item 8]** |
| ARRL Code Proficiency Program | Certificates 10–40 WPM in 5 WPM steps for one minute solid from a W1AW run. **[verify]** |
| CWops CW Academy | Progressive levels to roughly 25–35 WPM. **[verify]** |
| The trained ear | Copies to about −7 dB reference (0 dB in a 500 Hz passband); HM-DEC-097's own reference. |

### 11.2 Prior decoders, for comparison of documented behaviour

fldigi CW modem (W1HKJ), CW Skimmer (VE3NEA), RSCW (PA3FWM), AG1LE's Bayesian
/ LSTM / CNN decoders with published CER-vs-SNR curves in the 2500 Hz
convention, ggmorse (Gerganov), MAUDE (MIT Lincoln Laboratory). None adopted.

---

## 12. To vendor before v1 freezes

Under `data/vendor/` with URL, retrieval date, clause cited, and a pinning
test (CLAUDE.md §0, §4). No requirement may cite a line of this file that
still carries [verify].

1. ITU-R M.1677-1 — timing table and character set (§6).
2. ITU-R F.1487 Annex 3 — full profile table (§9.1).
3. ITU-R SM.328 / SM.1138 — A1A bandwidth formula and K values (§7).
4. PARIS convention — ARRL Handbook chapter and edition (§6.3).
5. ARRL Farnsworth definition (§6.4).
6. Gold 1959 — citation and variability ranges (§10).
7. Morse Runner — licence and impairment parameter definitions; PathSim
   availability (§9.4).
8. Historical FCC copy rule, ARRL Code Proficiency rules, CWops level table
   (§11.1).
9. Amateur prosigns KN, BK, CL — one citable ARRL source (§6.2).

## 13. How the arbiter uses this, once frozen

- A work instruction names the `HM-REQ` ids it is meant to move.
- An outcome reports each named id as **met / not met / not yet measurable**,
  beside test totals.
- A change that moves no requirement and is not a design-constraint change
  recorded in `DECISIONS.md` is out of scope by definition.
- Harness floors ratchet against the thresholds in `CW_REQUIREMENTS.md` §V,
  not against the previous build.
- Conflict between a requirement and a later ruling is surfaced as a new
  ruling for Tim; it is never resolved by editing either file silently.

## 14. The learned reader

The learned reader (`CwLearnedReader`, HM-DEC-302) is a small trained network, a convolutional front end, two bidirectional GRU layers and CTC, that maps 26 bins of log spectrum to letters. It was trained on synthetic keying only and has heard no recording. `src/Hamlet.RadioEngine/Cw/Model/CWNET_SPEC.md` states exactly what the engine computes, and the shipped vector proves the port against it.

It is fed the audio the sequence reader is fed, before any pitch, lane or mark finding, decimated to 8 kHz, and runs on a sliding 12 s window once a second, committing frames 2 s behind the newest sample. Its letters go where the sequence reader's go, as dots, dashes, letter ends and word ends to the lookup table, behind `CwRules.LearnedReader`, off by default; on, the sequence reader is not built.

It does not find a pitch, a lane, a mark or a gap: it prints letters. It reads the louder of two stations, having been trained with a second station 4 to 25 dB down. It knows no words, and its errors are letter-gap splits and merges. It prints a word space itself, and the engine adds no space rule on top.

**The second model** (HM-DEC-303) is the first trained further, outside this tree, on scenes the first never saw: a receiver passband that is a wall, a residual noise floor outside it that sits after the receiver's AGC and so shows as a bright shadow of the keying in the bins beyond the edge, an AGC that rides the noise and flickers through the gaps, a sender within 140 Hz of the passband's top edge, speeds down to 5 WPM, and silent scenes. It has still heard no recording, and its features, alphabet and streaming rule are the first's, so the port did not change. With the switch on, the panel reads the learned reader: the scope's `decoding at` line is the median pitch of its last ten letters' marks, blank under three and again after ten seconds with nothing printed; the speed is a letter rate, the letters and word spaces of the last ten seconds at five characters a word, blank under five and marked not proved, because the net names letters and measures no dit; and the light is lit while the newest run's last second, not yet committed, holds a letter or a space, so it follows the keying once a second and is dark in long gaps and in noise. With the switch off, nothing here runs.

**The learned reader is what Hamlet reads with** (HM-DEC-304, the owner's ruling of 2026-11-06). `CwRules.LearnedReader` is on by default, so the chain the tab, the scan's ear and every test helper build reads with the learned reader, second model, and the panel shows its pitch, its letter-rate speed marked not proved, and its light. The sequence reader is still in the tree behind the same switch, off: turned off, the learned reader is never built and the sequence reader is built and reads exactly as it did, and its own tests and traces turn it off to keep testing it. The guards now hold the learned reader's own figures: the twelve-plus-three at 286 and W1AW at 2128 through the `599 ruled` rows of the scoreboard, the first recording as it reads it (26 of 27 letters), at most 11 letters in silences, the easy fixtures at 87 of 90, the 5 dB CQs at 83 of 84 with at most one letter in the silences after them, and each fist bank row at what it reads less 5%. A letter is dimmed where the net's probability is under one half, a line not yet measured against the net's errors.

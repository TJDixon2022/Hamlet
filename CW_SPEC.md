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
- the running pitch with its proof state;
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

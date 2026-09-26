**PROJECT: Hamlet**

# CW receive decoder — requirements, v1.1 (draft for freeze; §M added 2026-09-26)

| Field | Value |
|---|---|
| Version | 1.0-draft |
| Date | 2026-09-25 |
| Status | Every requirement below is **candidate** until stage 7 reconciliation and Tim's commit. |
| Vocabulary | `CW_SPEC.md`. Every `CH-*`, `TX-*`, `INT-*`, `MET-*` and every dB is defined there. SNR is always in the 2500 Hz reference. |
| Change rule | After freeze, a requirement changes only by a ruling in `DECISIONS.md` naming its id. A superseded requirement keeps its id with `status: superseded-by HM-REQ-###`. |

**Record reach.** Harvested from `DECISIONS.md` to HM-DEC-095 in full, the
`CLAUDE.md` index to HM-DEC-133, field reports and analyses to 2026-08-27,
and the 2026-09-23 key. Rulings HM-DEC-096 onward were reconciled through the
index only; HM-DEC-134 onward were **not reconciled** (Tim, 2026-09-25: not
material). Any conflict found later surfaces as a ruling, not an edit.

**Tier meanings.** *must*: v1 is not met without it. *should*: measured and
reported in v1; a floor is set when measured. *later*: named so it is not
argued about; no v1 obligation beyond the honesty requirements, which apply
to everything.

**Reading a verification row.** Method — T (test against fixture), A
(analysis of logged output), I (inspection). Condition — profiles from
`CW_SPEC.md`. Threshold — the number the harness floors against. Unknown vs
wrong — what a placeholder or dim character counts as for this metric. Truth
grade — the lowest grade acceptable for the corpus used.

---

## A. Honesty of the record

| id | statement | rationale | source | tier |
|---|---|---|---|---|
| HM-REQ-001 | The decoder shall attach exactly one confidence class — sure, dim or placeholder — to every character it emits. | §0.0: uncertainty is shown as uncertainty. The three classes carry the stated rates in `CW_SPEC.md` §11. | HM-DEC-048, C-028, C-003 | must |
| HM-REQ-002 | If an element pattern matches no entry in the v1 character table, then the decoder shall emit a placeholder. | The table is allowed to say no; the nearest letter is a guess. | HM-DEC-048, C-102 | must |
| HM-REQ-003 | The decoder shall derive each character's confidence class from measurements of the audio alone, and from no property of the surrounding text. | Nothing raises a score: not a spell check, a near-matching callsign, or a word that would make sense. A different architecture could satisfy this; it is behaviour, not design. | HM-DEC-048, HM-DEC-108, C-004 | must |
| HM-REQ-004 | The decoder shall use no knowledge beyond the character table and the timing of the audio when choosing a character. | No language model, no letter-frequency prior, no dictionary. **Needs ruling** — proposed in DEV_ANALYSIS_2026-08-27 §4, never ruled. | C-011 | must (pending ruling) |
| HM-REQ-005 | While no keyed tone is present in the passband, the decoder shall emit zero characters and report the band as "not measured". | Nothing is claimed from an empty band; no clock is fitted to noise gaps. | HM-DEC-048, HM-DEC-090, C-006, C-007 | must |
| HM-REQ-006 | If the measured mark and gap timings do not form Morse, then the decoder shall emit nothing sure. | Placeholders are allowed; letters are not. Holds after any joint-decoding change. | HM-DEC-048, C-005, C-012 | must |
| HM-REQ-007 | The decoder shall report "nothing read" and "nothing there" as two distinct states. | Pictures clause of §0.0; an empty transcript from a silent band and from a failed decode mean different things to the operator. | §0.0, HM-DEC-092, C-010 | must |
| HM-REQ-008 | When a co-channel signal (INT-COCHAN) is present within the detector bandwidth of the tracked tone, the decoder shall emit no sure character. | Veto, not a matter of degree. | HM-DEC-048, C-022 | must |
| HM-REQ-009 | The decoder shall report every value as a measurement of the audio and never as a statement about a station, the band, or anybody's equipment. | The app describes; it does not diagnose. | HM-DEC-088, HM-DEC-048, C-098, C-140 | must |

## B. Confidence — rates, coverage, calibration (ruled 2026-09-25)

| id | statement | rationale | source | tier |
|---|---|---|---|---|
| HM-REQ-010 | On every must-tier condition at or above the sensitivity floor, the decoder shall keep MET-CER-SURE below 1 %. | One in a hundred sure characters wrong is the operator-actionable limit (HM-DEC-108's one in four on a callsign is the failure). Same number everywhere; one rule. | Q8, HM-DEC-108, C-023 | must |
| HM-REQ-011 | On every condition at or above the sensitivity floor, the decoder shall keep MET-INVENTED at zero. | The prime directive as a number. | Q8, HM-DEC-117, HM-DEC-120, C-033 | must |
| HM-REQ-012 | On every must-tier condition at the sensitivity floor, the decoder shall emit at least 90 % of sent characters as sure (MET-COVERAGE ≥ 0.90). | Without a floor, dimming everything satisfies HM-REQ-010. Ninety percent is "solid with a few misses". Floors only rise. | Q9, HM-DEC-108, C-027 | must |
| HM-REQ-013 | On every must-tier sender profile at 15 dB reference on CH-AWGN, the decoder shall emit every sent character as sure and correct. | A loud clean signal is decoded whole or it is a defect. Coverage 100 %, MET-CER-SURE 0, MET-WBE 0. | HM-DEC-114, Q9, C-008 | must |
| HM-REQ-014 | Over the corpus of each must-tier condition, characters the decoder emits as dim shall be correct at least 70 % of the time. | "Dim" becomes a statement (usable as a hint, not to act on alone) rather than a colour. Below 70 % it should have been a placeholder. | Q10, HM-DEC-108 | must |
| HM-REQ-015 | The decoder shall make each character's confidence class available at the moment the character is emitted. | The scanner's stopping rule and the live terminal consume it; a class assigned only on settle is useless to them. | FG-009, C-141 | must |

## C. Refusal and sensitivity

| id | statement | rationale | source | tier |
|---|---|---|---|---|
| HM-REQ-020 | On CH-AWGN with TX-ITU at 20 WPM, the decoder shall meet HM-REQ-010 and HM-REQ-011 down to −7 dB reference. | HM-DEC-097's 0 dB in the 500 Hz passband, restated in the reference bandwidth. Unchanged in substance. | Q2, HM-DEC-097, C-030 | must |
| HM-REQ-021 | Across the speed range, the sensitivity floor of HM-REQ-020 shall scale by 3 dB per doubling of speed. | Energy per element; one anchor covers the ladder. | Q2, `CW_SPEC.md` §8.3 | must |
| HM-REQ-022 | If the measured SNR is below the sensitivity floor for the measured speed, then the decoder shall emit no sure character. | Refusal, not degraded copy: a marked-as-degraded wrong callsign is still actionable. | HM-DEC-097, C-030, C-032 | must |
| HM-REQ-023 | The decoder shall not perform worse on any metric at a higher SNR than at a lower SNR on the same condition profile. | Eighteen decibels returned worse copy than ten (HM-OPEN-016); a strong signal must not decode worse than a weaker one. | HM-OPEN-016, C-038 | must |

## D. Speed

| id | statement | rationale | source | tier |
|---|---|---|---|---|
| HM-REQ-030 | The decoder shall meet every must-tier requirement at any sending speed from 5 to 45 WPM inclusive. | Ruled 2026-09-25, both ends hard. Above 30.6 WPM is synthetic-only until a capture exists. | Q1, HM-DEC-103 | must |
| HM-REQ-031 | After acquisition on TX-ITU, the decoder shall report speed within 10 % of true (MET-WPM-ERR ≤ 10 %). | Clock error damages word spacing rather than letters (96 % letter agreement with a 40 % clock error); 10 % keeps the gap classes separable. | Q1, FIELD_REPORT §6, C-044 | must |
| HM-REQ-032 | When the sending speed changes by 25 % or more, the decoder shall report the new speed within N characters. | "Within a few characters" is the existing test's intent; N is **TBD, needs ruling** (recommended 5). | HM-OPEN-016, C-041 | must (threshold TBD) |
| HM-REQ-033 | The decoder shall acquire speed at any speed in range without a prior speed to iterate from. | Acquisition must not depend on already knowing the answer; 30 WPM from cold is the named case. | HM-DEC-122 rationale, C-047 | must |
| HM-REQ-034 | The decoder shall report the speed estimate with a proof state of proved, hypothesis, or none. | A speed the decoder has not earned is not a number. | FIELD_REPORT §0, C-009 | must |
| HM-REQ-035 | When the transcript is cleared, the decoder shall retain its speed, pitch and noise-floor state. | A clear that costs the decoder what it worked out is forbidden. | HM-DEC-051, C-049, C-143 | must |
| HM-REQ-036 | When the pitch is refined for the same station, the decoder shall retain its timing state. | One retune decodes and three do not; a refinement is not a new station. | HM-DEC-123, C-048 | must |

## E. Channel and fading (ruled 2026-09-25)

| id | statement | rationale | source | tier |
|---|---|---|---|---|
| HM-REQ-040 | On CH-LM and CH-MM with TX-ITU at 20 WPM, the decoder shall meet HM-REQ-010, HM-REQ-011 and HM-REQ-012 down to −4 dB reference. | Moderate low- and mid-latitude fading: 3 dB allowance against the AWGN floor. | Q3 | must |
| HM-REQ-041 | On CH-HM with TX-ITU at 20 WPM, the decoder shall meet HM-REQ-010, HM-REQ-011 and HM-REQ-012 down to −1 dB reference. | 10 Hz spread is auroral flutter; 6 dB allowance. | Q3 | must |
| HM-REQ-042 | On CH-LD, CH-MD and CH-HD with TX-ITU at 20 WPM, the decoder shall meet HM-REQ-010 and HM-REQ-011 down to an SNR to be set when first measured. | Disturbed profiles: measured and reported in v1; allowance TBD. | Q3 | should |
| HM-REQ-043 | While the signal is below the sensitivity floor during a fade, the decoder shall emit no sure character. | Nothing is invented during a fade. | Q3, HM-DEC-091, HM-OPEN-012, C-086 | must |
| HM-REQ-044 | When the signal returns above the sensitivity floor after a fade, the decoder shall resume emitting sure characters within one character. | A fading signal comes back rather than staying dead. | Q3, HM-OPEN-016, C-084 | must |

## F. Sender profiles (ruled 2026-09-25)

| id | statement | rationale | source | tier |
|---|---|---|---|---|
| HM-REQ-050 | On each of TX-ITU, TX-KEYER-W, TX-FARNS and TX-TIGHT, the decoder shall meet HM-REQ-013. | Every must-tier fist gets the 15 dB whole-message bar at its own timing. | Q4, HM-DEC-114, HM-DEC-101, C-104 | must |
| HM-REQ-051 | On each of TX-KEYER-W, TX-FARNS and TX-TIGHT on CH-AWGN, the decoder shall reach a sensitivity (MET-SENS) within 3 dB of its TX-ITU sensitivity. | Real timing costs a little; it must not cost the band. | Q4 | must |
| HM-REQ-052 | On TX-BUG and TX-STRAIGHT, the decoder shall meet HM-REQ-013 and a MET-SENS floor to be set when first measured. | Measured and reported in v1. | Q4 | should |
| HM-REQ-053 | On TX-SLOPPY, the decoder shall meet HM-REQ-011 and HM-REQ-006. | Honesty only: placeholders allowed, invented letters not. | Q4 | later |
| HM-REQ-054 | On TX-FARNS whose character gap is up to 7 units at character speed, the decoder shall place word boundaries where the sender placed them. | Nothing about 1:3:7 survives a traffic net; gaps are the sender's own. The behaviour under HM-DEC-115's clustering design. | HM-DEC-048, HM-DEC-095, HM-DEC-115, C-060, C-061 | must |

## G. Interference (ruled 2026-09-25)

| id | statement | rationale | source | tier |
|---|---|---|---|---|
| HM-REQ-060 | With INT-ADJ(100 Hz, 0 dB, any speed) present, the decoder shall hold the wanted station and meet HM-REQ-010 and HM-REQ-011 on it. | Two 25 WPM signals 100 Hz apart at equal strength are just resolvable (SM.328, K=5); the practical limit hams work to. | Q5 | must |
| HM-REQ-061 | With INT-ADJ(50 Hz, −6 dB, any speed) present, the decoder shall hold the wanted station and meet HM-REQ-010 and HM-REQ-011 on it. | "Someone tuned up beside me." | Q5 | must |
| HM-REQ-062 | With INT-CARRIER at any offset and any level present, the decoder shall not select the carrier over a keyed station. | A note is chosen by how it is keyed, never by how loud it is. | HM-DEC-095, C-072, C-073, C-079 | must |
| HM-REQ-063 | While INT-PILEUP is present with no readable station, the decoder shall emit placeholders and then nothing, and no sure character. | Blocks, then nothing — never confident invented text. Every zero-emitted capture with signal present is audited (V-09). | DEV_ANALYSIS §1, C-088 | must |
| HM-REQ-064 | When the tracked station ends and a second station begins at a different speed or pitch, the decoder shall emit no invented character across the handover. | Existing test intent. | HM-OPEN-016, HM-DEC-104, C-069, C-083 | must |
| HM-REQ-065 | While a station is confirmed, the decoder shall not abandon it for a candidate more than N dB below it. | N is the veto margin of HM-REQ-008; a candidate far below is not a station. **Threshold needs ruling** (recommended 6 dB). | HM-DEC-127, C-075 | must (threshold TBD) |
| HM-REQ-066 | When a second keyed signal is within the veto margin of the tracked one, the decoder shall report it as a competing station. | `competing: none found` beside a station 2.4 dB away is a false report. | DEV_ANALYSIS §3, C-097 | must |

## H. Character set (ruled 2026-09-25)

| id | statement | rationale | source | tier |
|---|---|---|---|---|
| HM-REQ-070 | The decoder shall recognise exactly the v1 character table of `CW_SPEC.md` §6.2, generated from the vendored M.1677-1 file and the cited prosign source. | Cited data, not constants. | Q6, CLAUDE.md §0 | must |
| HM-REQ-071 | When a run of elements is sent with no character gap in it and matches a prosign, the decoder shall emit it as one symbol. | Prosigns arrive as prosigns, never split into letters. | HM-DEC-048, C-100 | must |
| HM-REQ-072 | Where one pattern has both a punctuation name and a prosign name, the decoder shall emit one symbol and name it per the terminal's setting. | Naming, not a claim about the signal. | HM-DEC-048, C-101 | must |
| HM-REQ-073 | When the error signal (eight dits) is received, the decoder shall emit it as the error symbol. | Whether it also deletes the preceding word is **TBD, needs ruling** (`CW_SPEC.md` §6.2). Until ruled, nothing is deleted. | Q6 | must |

## I. Word boundaries (ruled 2026-09-25)

| id | statement | rationale | source | tier |
|---|---|---|---|---|
| HM-REQ-080 | On every must-tier sender profile at 15 dB reference on CH-AWGN, the decoder shall place every word boundary where the sender placed it (MET-WBE = 0). | Part of "decoded whole". `DEW B 6 RE D` is not whole. | Q7, HM-DEC-114, cw-2026-09-23 key, C-065, C-113 | must |
| HM-REQ-081 | On every must-tier condition at the sensitivity floor, the decoder shall keep MET-WBE at or below 5 % of words. | One misplaced space in twenty words is where a reader stops noticing. | Q7 | must |
| HM-REQ-082 | The decoder shall score word-boundary errors separately from character errors. | Edit-distance CER on raw text lets a cut count as insertions, understating letters and overstating the fault. | Q7, C-065 | must |
| HM-REQ-083 | The decoder shall emit one set of word boundaries for any span, such that a live rendering and a settled rendering of that span do not differ in boundaries. | The two passes disagreeing about where words are is worse than either being wrong alone. | HM-DEC-116, C-068 | must |
| HM-REQ-084 | On the acceptance spans named in DEV_ANALYSIS_2026-08-27 §4, the decoder shall emit `WEEKEND`, `THINKING`, `FLEX`, `ABOVE`, `BREEZE` and `USED TO USE A FIRM`. | Concrete, already-captured, already-named acceptance. Truth grade inferred; stated beside the row. | C-066 | must |

## J. Pitch

| id | statement | rationale | source | tier |
|---|---|---|---|---|
| HM-REQ-090 | The decoder shall acquire and track a keyed tone at any pitch from 300 to 900 Hz inclusive. | The radio's CW pitch range; a signal at the wrong pitch is still found. | HM-DEC-048, HM-DEC-049, HM-OPEN-016, C-070, C-071 | must |
| HM-REQ-091 | The decoder shall choose the tracked pitch by keying quality and never by level alone or by the operator's configured pitch. | A measurement pulled toward the setting is not a measurement. | HM-DEC-095, C-072, C-073, C-079 | must |
| HM-REQ-092 | After acquisition, the decoder shall report the pitch it is demodulating at, within N Hz of true (MET-PITCH-ERR ≤ N). | 12 of 14 captures wrong on a 25 Hz grid; fractional-hertz resolution is available for one FFT. N is **TBD, needs ruling** (recommended 5 Hz). | FIELD_REPORT §2, W1AW_BRIEF phase 5, C-076, C-077, C-078 | must (threshold TBD) |
| HM-REQ-093 | The decoder shall report pitch with a proof state of proved, hypothesis, or none. | Not a survey candidate, not a stale hold. | C-077 | must |
| HM-REQ-094 | While the operator is transmitting, the decoder shall not use that audio as evidence about any received station. | The operator's own transmission is not evidence about anybody else. | HM-DEC-095, C-074 | must |

## K. Acquisition time and latency (ruled 2026-09-25)

| id | statement | rationale | source | tier |
|---|---|---|---|---|
| HM-REQ-100 | When a transmission begins with a run-up of at least five characters, the decoder shall emit its first sure character within 2.0 s of the first element at 20 WPM, scaled by dit length at other speeds (MET-TACQ). | Real stations repeat; the decoder is not required to read the first letter it has ever heard. Two seconds is ten characters at 20 WPM, enough to fit clock and gap classes from data. | Q11, HM-DEC-103, HM-DEC-129, C-080 | must |
| HM-REQ-101 | In steady state, the decoder shall emit each sure character no more than two characters and no more than 3.0 s behind the air (MET-LAT). | The display stops feeling live beyond that. | Q11, DEV_ANALYSIS §4 | must |
| HM-REQ-102 | While acquiring, the decoder shall emit no sure character. | Candidates naming 325, 550, 725 Hz for a 640 Hz signal is a failure. | HM-DEC-125, C-082 | must |
| HM-REQ-103 | When a run-up precedes the message, the decoder shall not lose the opening characters of the message to acquisition. | Settling on 675 Hz for a 615 Hz signal and emitting nothing is its own defect. | HM-DEC-129, HM-OPEN-033, C-081 | must |

## L. Signal measurements reported

| id | statement | rationale | source | tier |
|---|---|---|---|---|
| HM-REQ-110 | The decoder shall measure the noise reference inside the receiver passband, beside the tone, while the tone is keyed. | Not an average over the silence between transmissions; not the skirt. | HM-DEC-088, HM-DEC-090, C-090, C-091, C-092 | must |
| HM-REQ-111 | If the receiver passband is unknown, then the decoder shall report the noise reference as "unknown". | Never measured in the skirt. | W1AW_BRIEF phase 4, FIELD_REPORT §4, C-092 | must |
| HM-REQ-112 | The decoder shall report every signal measurement with its bandwidth, its provenance and its age. | A value read earlier is not labelled fresh; a figure with no bandwidth is not a figure. | HM-DEC-131, `CW_SPEC.md` §5.1, C-094 | must |
| HM-REQ-113 | The decoder shall record the parameters it ran with (pitch, speed estimate, thresholds) alongside each output record. | §0.0.1: the app must be diagnosable. | §0.0.1, C-095 | must |
| HM-REQ-114 | The decoder shall report counts of characters emitted, dimmed, placeholders and rejections for each span. | Rejections reported without allocating per character. | HM-DEC-077, C-099 | must |
| HM-REQ-115 | The decoder shall not assert two incompatible things about the same span in one record. | Characters emitted versus empty transcript for one span (FIELD_REPORT §5). | C-096 | must |
| HM-REQ-116 | The decoder shall report tone level with an honest narrowband figure in dB relative to the noise reference. | `tonePeak` read 62–78 dB where an honest measurement gives ~26. | FIELD_REPORT §4, C-093 | must |

---

## M. Two decoders (ruled 2026-09-26)

Hamlet carries its own probabilistic decoder and a second decoder ported from fldigi's CW
modem (`src/cw_rtty/cw.cxx`, GPL-3, W1HKJ and AG1LE). The owner's ruling, 2026-09-26: *"Two
decoders must read the same audio. When the decoders agree, there's no problem. When they
disagree, we need to decide how to arbitrate that. Each decoder needs to give a confidence
score, and the higher score wins. On a tie, dim."* The order of work is fixed by these rows:
the second decoder exists and is compared before it votes, and it votes only once its
confidence is calibrated.

| id | statement | rationale | source | tier |
|---|---|---|---|---|
| HM-REQ-120 | Two decoders shall read the same audio: every character the operator sees has been read by both decoders from the same samples at the same time. | One decoder alone was three months of rediscovering what a mature one had settled; two opinions on the same audio is the point. | owner 2026-09-26, R84 | must |
| HM-REQ-121 | The operator shall see one transcript. Which decoder produced a character is never shown on the CW tab; the capture sheet records it per character. | The screen carries the reading, not the machinery. | owner 2026-09-26, CLAUDE.md §0.0 | must |
| HM-REQ-122 | The second decoder shall be a faithful port of its upstream source, receive path only, with its license, authors and upstream commit kept in the file, and no word, dictionary or callsign logic (HM-REQ-004). | A reference that has been improved is no longer a reference; the comparison is worthless. | R84, R72 | must |
| HM-REQ-123 | Before the second decoder votes, both decoders shall be scored on every keyed recording and the synthetic set through the same scorer and metrics, and the result tabled per recording and per condition. | Nothing arbitrates between two decoders whose relative performance is unknown. | R84, V-13 | must |
| HM-REQ-124 | Each decoder shall attach a calibrated confidence to every character it emits: over the keyed corpus, per condition, characters emitted at confidence p shall be right within 5 points of p. A decoder whose confidence is not calibrated on a condition does not vote on that condition; its output is advisory there. | Unit 442 measured our decoder certain and wrong 13 times in 47; an uncalibrated confidence picks the louder liar. fldigi's modem carries no confidence at all and must be given one. | HM-REQ-013, HM-REQ-014, unit 442 | must |
| HM-REQ-125 | Where both decoders emit the same character for the same span, it is emitted with the class of the more confident decoder. | Agreement is the easy case and is not to be made harder. | owner 2026-09-26 | must |
| HM-REQ-126 | Where the decoders disagree on a span, the character of the decoder with the higher calibrated confidence is emitted, and the disagreement is recorded on the sheet with both characters and both confidences. | The owner's rule: the higher score wins. | owner 2026-09-26 | must |
| HM-REQ-127 | Where the two confidences are within a margin of each other, the winning character is emitted in the dim class and never sure, and the sheet records the tie. The margin is **TBD, needs ruling** (recommended 0.05). | A tie is the decoder saying it does not know; the dim class exists for exactly that (HM-REQ-001). | owner 2026-09-26 | must (threshold TBD) |
| HM-REQ-128 | The arbitrated output shall be no worse than the better single decoder on every metric of §B and §I, on every condition. If arbitration loses to either decoder alone on any condition, it is switched off for that condition and the better decoder alone is used there. | Two decoders combined badly are worse than one; the combination has to earn its place. | owner 2026-09-26 | must |
| HM-REQ-129 | Where a technique of the second decoder is taken into the first, it is taken as a change to the first, judged as any other change, and the second decoder is left as ported. | The teacher is not edited to match the student. | R84 | must |

## V. Verification table

Method T = test against fixture, A = analysis of logged output, I =
inspection. Truth grade: the lowest grade acceptable. Where a row says
*synthetic only*, no real capture exists for the condition and the row says so
in every outcome report.

| id | method | condition | metric | threshold | unknown vs wrong | truth grade | notes |
|---|---|---|---|---|---|---|---|
| 001 | I, T | any | — | every emitted char has a class | — | synthetic | schema check |
| 002 | T | TX-ITU, invalid patterns injected | placeholder emitted | 100 % | placeholder is correct | synthetic | |
| 003 | T | same audio, text context varied | MET-COVERAGE, class per char | identical across contexts | — | synthetic | dictionary word vs nonsense of same timing |
| 004 | I | — | — | no LM / prior / dictionary in decode path | — | — | pending ruling |
| 005 | T | shaped noise, no tone | chars emitted; band state | 0; "not measured" | — | synthetic | V-06 noise band |
| 006 | T | non-Morse timings (random marks) | sure chars | 0 | placeholders allowed | synthetic | |
| 007 | T | silent band vs failed decode | reported state | two distinct values | — | synthetic | |
| 008 | T | INT-COCHAN | sure chars during overlap | 0 | placeholders allowed | synthetic | |
| 009 | I | all report strings | — | no diagnosis language | — | — | review checklist |
| 010 | T, A | every must condition ≥ floor | MET-CER-SURE | < 1 % | dim/placeholder excluded | inferred | one row per condition profile |
| 011 | T, A | every condition ≥ floor | MET-INVENTED | 0 | dim/placeholder excluded | inferred | |
| 012 | T | every must condition at floor | MET-COVERAGE | ≥ 0.90 | — | synthetic | |
| 013 | T | each must TX at 15 dB, CH-AWGN | MET-COVERAGE, MET-CER-SURE, MET-WBE | 1.00, 0, 0 | none permitted | synthetic + any real ≥ 15 dB | pass/fail, not a ratchet (V-08) |
| 014 | A | corpus per must condition | MET-CAL, dim bin | ≥ 0.70 | placeholder bin unscored | inferred | reliability table |
| 015 | T | streaming output | class present at emit time | 100 % | — | synthetic | |
| 020 | T | CH-AWGN, TX-ITU, 20 WPM, sweep | MET-SENS | ≤ −7 dB | dim/placeholder excluded | synthetic | HM-DEC-097 anchor |
| 021 | T | CH-AWGN, TX-ITU, 10 and 40 WPM | MET-SENS | ≤ −10 dB, ≤ −4 dB (±1 dB) | as 020 | synthetic | verifies scaling |
| 022 | T | CH-AWGN, below floor | sure chars | 0 | placeholders allowed | synthetic | floor found by sweep (V-05) |
| 023 | A | every profile, SNR ladder | all metrics | monotone in SNR | — | synthetic | |
| 030 | T | 5, 20, 45 WPM, TX-ITU, 15 dB and floor | as 013, 010, 011 | as those rows | as those rows | synthetic only above 30.6 | |
| 031 | T | TX-ITU, after acquisition | MET-WPM-ERR | ≤ 10 % | — | synthetic | |
| 032 | T | speed step ≥ 25 % | chars to new estimate | TBD (rec. 5) | — | synthetic | needs ruling |
| 033 | T | cold start at 5, 30, 45 WPM | speed acquired | yes | — | synthetic | |
| 034 | I, T | any | proof state field | present, three values | — | synthetic | |
| 035 | T | clear mid-transmission | state retained | yes | — | synthetic | |
| 036 | T | pitch refine ±10 Hz same station | timing state retained | yes | — | synthetic | |
| 040 | T | CH-LM, CH-MM, TX-ITU, 20 WPM, sweep | MET-SENS with 010/011/012 held | ≤ −4 dB | as 010 | synthetic | needs vendored profile values |
| 041 | T | CH-HM, TX-ITU, 20 WPM, sweep | same | ≤ −1 dB | as 010 | synthetic | |
| 042 | T | CH-LD, CH-MD, CH-HD | MET-SENS | TBD — baseline first | as 010 | synthetic | should |
| 043 | T | IMP-QSB deep fade below floor | sure chars in fade | 0 | placeholders allowed | synthetic | |
| 044 | T | IMP-QSB, return above floor | chars to first sure after return | ≤ 1 | — | synthetic | existing "fading signal comes back" test |
| 050 | T | TX-KEYER-W, TX-FARNS, TX-TIGHT at 15 dB | as 013 | as 013 | as 013 | synthetic + 013347 and HM-DEC-115 captures | |
| 051 | T | those profiles, CH-AWGN sweep | MET-SENS relative to TX-ITU | within 3 dB | as 010 | synthetic | |
| 052 | T | TX-BUG, TX-STRAIGHT | 013; MET-SENS | 013; TBD | as 013 | synthetic | should |
| 053 | T | TX-SLOPPY | MET-INVENTED; sure on non-Morse | 0; 0 | placeholders allowed | synthetic | later |
| 054 | T | TX-FARNS, char gap 3–7 units | MET-WBE | 0 at 15 dB | — | HM-DEC-115 capture (independent) + synthetic | |
| 060 | T | INT-ADJ(100, 0, 25) | 010, 011 on wanted | held | as 010 | synthetic | |
| 061 | T | INT-ADJ(50, −6, 25) | 010, 011 on wanted | held | as 010 | synthetic | |
| 062 | T | INT-CARRIER at 0, +10, +20 dB, offsets 50–500 Hz | station selected | keyed station | — | synthetic | HM-DEC-095 |
| 063 | T, A | INT-PILEUP | sure chars | 0 | placeholders allowed | real pileup captures | every zero-emit audited (V-09) |
| 064 | T | two-station handover | invented at handover | 0 | — | synthetic | existing `NothingIsInventedAtTheHandover` |
| 065 | T | confirmed station, candidate −N dB | station retained | yes | — | synthetic | N TBD |
| 066 | T | second signal within veto margin | competing reported | yes | — | synthetic | DEV_ANALYSIS 2.4 dB case |
| 070 | I, T | table generation | table = vendored set | exact | — | — | pinning test against `data/vendor/` |
| 071 | T | AR, SK, BT, KN, BK, CL, AS sent as runs | one symbol each | 100 % | — | synthetic | HM-DEC-124 caret fault must not recur |
| 072 | I | naming setting | one symbol, two names | — | — | — | |
| 073 | T | error signal | symbol emitted; nothing deleted | yes | — | synthetic | deletion TBD |
| 080 | T | each must TX at 15 dB | MET-WBE | 0 | — | synthetic + WB6RED key (inferred) | |
| 081 | T | each must condition at floor | MET-WBE | ≤ 5 % | — | synthetic | |
| 082 | I, A | scoring code | MET-WBE separate from MET-CER | yes | — | — | |
| 083 | A | streaming vs settled | boundary diff | 0 | — | synthetic | |
| 084 | T | named captures | emitted text | six named strings | — | inferred | pass/fail |
| 090 | T | 300, 400, 500, 600, 750, 875, 900 Hz | acquired | all | — | synthetic | existing wrong-pitch test |
| 091 | T | keyed weak + loud carrier; configured pitch ≠ true | pitch chosen | keyed, true | — | synthetic | |
| 092 | T, A | after acquisition | MET-PITCH-ERR | TBD (rec. 5 Hz) | — | FIELD_REPORT carriers (independent) | needs ruling |
| 093 | I, T | any | proof state field | present | — | synthetic | |
| 094 | T | own-TX audio present | received-station state unchanged | yes | — | synthetic | |
| 100 | T | 5-char run-up, 10/20/40 WPM | MET-TACQ | ≤ 2.0 s scaled | — | synthetic | |
| 101 | A | steady state | MET-LAT | ≤ 2 chars and ≤ 3.0 s | — | synthetic | |
| 102 | T | acquisition window | sure chars | 0 | placeholders allowed | synthetic | |
| 103 | T | run-up + message | opening chars lost | 0 | — | synthetic + 013347 | |
| 110 | I, T | keyed tone with known noise | noise reference | in-passband, during key-down | — | synthetic | |
| 111 | T | passband unknown | noise field | "unknown" | — | synthetic | |
| 112 | I | record schema | bandwidth, provenance, age present | all | — | — | |
| 113 | I | record schema | parameters present | all | — | — | |
| 114 | I, T | any span | counts present and sum | yes | — | synthetic | |
| 115 | A | sidecar-equivalent record | contradictions | 0 | — | any | |
| 116 | T | synthetic tone at known level | reported tone dB | within 1 dB of truth | — | synthetic | FIELD_REPORT §4 |
| 120 | I, T | any | — | every emitted char carries both decoders' readings on the sheet | — | synthetic | schema check |
| 121 | I | — | — | no decoder name on the CW tab | — | — | inspection |
| 122 | I | — | — | header carries license, authors, commit; no transmit path; no word logic | — | — | inspection of the ported file |
| 123 | T, A | all keyed and synthetic | MET-CER-SURE, MET-INVENTED, MET-COVERAGE, MET-WBE, both decoders | tabled per recording and condition | — | inferred | parity.md |
| 124 | T | per condition, keyed | confidence calibration | right within 5 pts of stated p | uncalibrated = no vote | inferred | per decoder |
| 125 | T | synthetic, agreement injected | class of emitted char | class of the more confident | — | synthetic | |
| 126 | T | synthetic, disagreement injected | emitted char, sheet record | higher confidence's char; both recorded | — | synthetic | |
| 127 | T | synthetic, tie injected | class | dim, never sure; tie recorded | dim is correct | synthetic | margin TBD |
| 128 | T, A | every condition | every metric of B and I | arbitrated ≥ better single decoder | — | inferred | switched off per condition on a loss |
| 129 | I | — | — | second decoder's diff against upstream is attribution only | — | — | inspection |

## V-rules. Corpus and verification rules (harvested, apply to every row)

| id | rule | source |
|---|---|---|
| V-01 | Every requirement is tested against recorded or generated WAV fixtures before live audio; every failure becomes a replayable case. | HM-DEC-007 |
| V-02 | On-air recordings are permanent, read-only fixtures. | HM-DEC-091 |
| V-03 | A real capture outranks a synthetic one when they disagree. | HM-DEC-091, HM-DEC-121 |
| V-04 | A fixture the reference decoder cannot read is a generator defect; the control for the generator is the real recording. Lowering the gate to admit a fixture is forbidden. | HM-DEC-101 |
| V-05 | A floor is found by sweeping and locating where MET-INVENTED crosses zero, never by translating another measurement. | HM-DEC-117 |
| V-06 | No synthetic fixture contains digital silence; every one carries a shaped noise band. | HM-OPEN-018, HM-DEC-127 |
| V-07 | Synthetic segments join across a gap, never mid-character. | HM-DEC-104 |
| V-08 | Rows marked pass/fail (013, 050, 080, 084) are never ratchets; a bar phased by speed is rejected. | HM-DEC-114 |
| V-09 | Every zero-emitted capture with signal present is audited to confirm nothing readable was silenced. | DEV_ANALYSIS §1 |
| V-10 | Floors only rise. Harness floors ratchet toward the thresholds above, not against the previous build. | W1AW_BRIEF, HM-DEC-126 |
| V-11 | No change may make an earlier capture or the synthetic corpus go red to make a newer one green. | W1AW_BRIEF overfitting guard |
| V-12 | Nothing is diagnosed against audio that has not itself been proved. | HM-DEC-102 |
| V-13 | An inferred key is labelled inferred everywhere; disagreement with it is not by itself proof the decoder is wrong. | cw-2026-09-23 key |
| V-14 | Loosening a separation limit, confirmation rule or plausibility bound to pass a fixture is forbidden. | HM-DEC-095, W1AW_BRIEF |
| V-15 | Field checks use the independent chain (separate Goertzel, 30 ms Hann, ~50 Hz ENBW, 2.5 ms hop, Otsu, Schmitt) so a decoder fault and a reference fault cannot share a cause. | FIELD_REPORT §0 |

---

## T. Traceability to the existing harness

Known from test names in HM-OPEN-016 and HM-DEC-104. A session extending this
table names the test file and method; rows marked *none* are the first work
the spec asks for.

| requirement | existing test | status |
|---|---|---|
| 044 | "fading signal comes back" (HM-OPEN-016) | exists; threshold to be set to ≤ 1 char |
| 064 | `NothingIsInventedAtTheHandover` | exists |
| 090 | "wrong pitch is still found" 400/500/750/875 (HM-OPEN-016) | exists; extend to 300 and 900 |
| 032 | "speed follows a change within a few characters" (HM-OPEN-016) | exists; N to be ruled |
| 083, 036, 064 | clock loss / retained clock / tracker switch / speed-change annotation (HM-DEC-104) | exist |
| 020, 022 | sensitivity sweep fixture (HM-DEC-088, HM-DEC-097, HM-DEC-120) | exists; re-label to reference bandwidth |
| 013 | 15 dB clean pass/fail (HM-DEC-114) | exists |
| 054 | HM-DEC-115 traffic-net capture | fixture exists; MET-WBE scoring does not |
| 084 | DEV_ANALYSIS acceptance strings | captures exist; test does not |
| 040, 041, 042 | — | none: needs channel simulator in generator |
| 050–053 | — | none: needs TX-* profiles in generator (TX-TIGHT capture exists) |
| 060–062, 066 | — | none: needs INT-* in generator |
| 010, 011, 012, 014 | — | none: needs MET-CER-SURE / MET-COVERAGE / MET-CAL scoring |
| 100, 101 | — | none: needs MET-TACQ / MET-LAT instrumentation |
| 092, 116 | FIELD_REPORT carrier captures | fixtures exist; test does not |
| 120–129 | none | §M is new on 2026-09-26; the port, the parity table and the calibration are the first work under it |

---

## R. Rulings still needed before freeze

| item | requirement | recommendation |
|---|---|---|
| Knowledge rule (no LM / prior / dictionary) | 004 | **ruled 2026-09-24 as HM-DEC-175 (R72): adopted** |
| Tie margin between decoders' confidences | 127 | 0.05 |
| Speed-change tracking, characters | 032 | 5 |
| Station-retention margin, dB | 065 | 6 |
| Pitch error, Hz | 092 | 5 |
| Error-signal deletion | 073 | render only; revisit after a capture shows one |
| Disturbed-profile allowances | 042 | measure first |
| TX-BUG / TX-STRAIGHT sensitivity | 052 | measure first |

## X. Excluded as design constraints

Harvested statements that name a mechanism and therefore stay in
`DECISIONS.md`, with the requirement they protect: Goertzel bank and adaptive
gate (HM-DEC-048 → 090, 110); confidence as worse-of-two / worse-of-three
(HM-DEC-048, HM-DEC-108 → 010–014); refusal floor of 14 margin units
(HM-DEC-120 → 020, 022); dit/dah boundary fitted between mark clusters
(HM-DEC-119 → 031); streaming pass adopting settled gap classes (HM-DEC-116,
HM-DEC-128 → 083); second-best definition for margin (DEV_ANALYSIS §2 → 010);
Viterbi joint decode (DEV_ANALYSIS §4 → 084); order-of-magnitude margin
separation (DEV_ANALYSIS "done means" → 014, replaced by the calibration
requirement).

## Counts

| | must | should | later | TBD threshold | pending ruling |
|---|---|---|---|---|---|
| Requirements | 75 | 2 | 1 | 4 | 0 |

Seventy-eight requirements: sixty-eight from 109 candidates and eleven rulings, plus the ten
of §M ruled 2026-09-26 (the must column includes the four with a TBD threshold; the knowledge
rule was ruled 2026-09-24 and is no longer pending).
Every row has a verification entry; the rows marked *none* in §T have no
harness test yet, and those are the spec's first work.

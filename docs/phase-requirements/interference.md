# INT-* interference profiles - what the generator mixes, and the recipe

Work instruction 469, `PHASE_PLAN.md` 7.3, `CW_SPEC.md` section 9.3, HM-REQ-060, 061, 062 and
066. Built in `tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/CwInterference.cs`; proved by
`TheInterferenceProfilesAreWhatTheySayTests` in the same folder. Test fixtures only: audio
synthesis, not a keyer, and nothing reaches the radio (`CLAUDE.md` 0.2).

**Every parameter below that is not a verification row's or section 9.3's is the arbiter's
reading under R85 (work instruction 469 DECIDED (4) to (6)), and overrulable.**

## 1. The profile list

Fixed before any code was written. Every interferer is a second layer mixed over **the wanted
station**: TX-ITU (1:3:1:3:7) keying `SyntheticCq.Text`, `CQ CQ CQ DE N0CALL N0CALL K`, at
600 Hz, on CH-AWGN at 15 dB in the 2500 Hz reference, the SNR set on the wanted station alone.
Every level is relative to the wanted station's key-down power.

| id | section 9.3's definition, quoted | parameter | value | where the value comes from | requirement served |
|---|---|---|---|---|---|
| INT-ADJ(Δf, ΔdB, WPM) | "A second CW signal Δf hertz away, ΔdB relative to the wanted one, at its own speed." | Δf, ΔdB, WPM | (100 Hz, 0 dB, 25 WPM) | verification row 060, `INT-ADJ(100, 0, 25)` | HM-REQ-060 |
| | | | (50 Hz, −6 dB, 25 WPM) | verification row 061, `INT-ADJ(50, −6, 25)` | HM-REQ-061 |
| | | | (100 Hz, 0, −2.4 and −6 dB, 25 WPM) | row 066's "second signal within veto margin"; −2.4 dB is the DEV_ANALYSIS case the row names; −6 dB is the veto margin read as HM-REQ-065's recommended 6 dB (DECIDED (6)); +100 Hz and 25 WPM this instruction's DECIDED (6) | HM-REQ-066 |
| | | sign of Δf | + and −, each | DECIDED (the task's "±"); row 060 and 061 state no side | HM-REQ-060, 061 |
| | | keying | TX-ITU, 1:3:1:3:7, no drift | DECIDED (4), TX-ITU nominal | all |
| | | text | `DE N0AAA UP`, repeated word by word to the wanted's end | `CwTwoInOnePassband.OtherText`, the tree's existing competing text; no real callsign | all |
| | | start | the first mark of the wanted's second word | DECIDED (5): "wanted" is the station present first | all |
| INT-COCHAN | "A second signal within the detector bandwidth of the wanted one. The veto case." | Δf | +10 Hz | DECIDED (4): inside the detector bandwidth the trace named, `CwProbabilisticDecoder.IntegratorBandwidthHz` = 45 Hz (Hann) centred on the tracked tone, so within ±22.5 Hz; 10 Hz is the arbiter's point inside it | HM-REQ-008 (produced and proved, not measured: parked) |
| | | ΔdB, WPM, keying | 0 dB, 25 WPM, TX-ITU | DECIDED (4) for 0 dB and TX-ITU; 25 WPM is verification row 060's speed | |
| | | text, start | as INT-ADJ | as INT-ADJ | |
| INT-CARRIER(Δf, ΔdB) | "A steady tone. The "loudest is not keyed" case." | Δf | +50, +100, +200, +500 Hz | verification row 062, "offsets 50–500 Hz", at the task's four points | HM-REQ-062 |
| | | ΔdB | 0, +10, +20 dB | verification row 062, "INT-CARRIER at 0, +10, +20 dB" | HM-REQ-062 |
| | | keying | none: steady from the first sample of the case to the last, phase 0 | DECIDED (4): "present from the first sample"; section 9.3's "steady" | HM-REQ-062 |
| INT-PILEUP | "Three or more stations inside the passband. The "emit nothing" case." | stations | three TX-ITU stations over the wanted, each at 0 dB | DECIDED (4): "three TX-ITU stations at equal level" | HM-REQ-063 (produced and proved, not measured: parked) |
| | | pitches | 525, 675 and 750 Hz (−75, +75, +150 Hz) | arbiter's: 525 and 675 Hz are the decision log's two-tone pitches (the 625/525 and 615/675 pairs unit 447's instrument cases carry), 750 Hz is from the same case list; all inside the generator's 350–870 Hz passband | |
| | | speeds | 12, 25 and 18 WPM, in that order of pitch | `SyntheticCq.Speeds`, one each: distinct speeds (DECIDED (4)) | |
| | | texts | `DE N0AAA UP`, `QRZ DE N0AAA`, `5NN TU`, each repeated to the wanted's end | arbiter's: no real callsign, no two alike | |
| | | start | as INT-ADJ, all three at once | DECIDED (5) | |

**Where the value is `TX-ITU nominal`**: every keyed interferer is 1:3:1:3:7 at its own speed
with 5 ms raised-cosine edges, keyed through the same generator edges as the wanted station.

**The wanted station is never keyed differently because an interferer is present.** Its key is
`SyntheticCq.Text` exact by construction (R61, V-13); an interferer's text goes in the sidecar and
is never scored as the wanted key (R72).

## 2. The recipe

Enough to rebuild any case sample for sample.

**Construction.** `CwInterference.Render(spec, wantedWpm, seed)`:

1. **The wanted station** is `CwChannel.RenderKeyed("CH-AWGN", 15, recipe, 600, seed, ...)` with
   `recipe = SyntheticCq.Recipe(wantedWpm, 0, seed) with { ToneHz = 600, DriftHz = 0 }`: TX-ITU,
   1:3:1:3:7, dit `1200 / wpm` ms, 5 ms raised-cosine edges, keyed through the generator's
   `KeyEdges` and `Gate`, after a 1.0 s lead-in and before a 1.5 s tail. Unit 461's band
   (`ShapedNoise(seed)` x 0.5, RMS 0.01) and unit 461's reference: the tone's power is
   `10^(15/10) x n0(600 Hz) x 2500`, so the SNR is set on the wanted station alone. Its key-down
   amplitude `A` is `CwChannelParts.Amplitude`.
2. **Each keyed interferer** is TX-ITU at its own speed and pitch `600 + Δf`, amplitude
   `A x 10^(ΔdB/20)`. Its text is its words taken in turn and cycled, each word kept only if
   its last key-up lands by the wanted's last; the kept text's `KeyEdges` are shifted so its
   first mark is the first mark of the wanted's second word. The envelope is
   `CwChannel.Envelope` (made `internal` for this, otherwise unchanged); the tone is
   `A' x envelope x cos(2 pi f n / 8000)`.
3. **A carrier** is `A x 10^(ΔdB/20) x cos(2 pi (600 + Δf) n / 8000)` on every sample of the
   case, from the first to the last.
4. **The mix** is `band + wanted + every layer`, summed in double. Where its peak passes 0.9 of
   full scale the whole sum is multiplied by `0.9 / peak`, which changes no SNR and no ratio;
   nothing is clamped and nothing clips.
5. **The sidecar** is `CwChannel`'s, then `intProfile`, `intDefinition`, `intMixer`, the wanted
   key (the text keyed, exact, the only key scored), the wanted pitch and amplitude, the seed,
   one `layer n` line per interferer (keyed or steady, pitch, offset, level, speed, start, the
   text actually keyed, "never scored"), and `headroom`.

**Seeds, fixed before any decode.** `CwInterference.Seed(wpm) = 469000 + ` the wanted speed's
index into `SyntheticCq.Speeds` (12, 18, 25 WPM): **469000, 469001, 469002**. The seed is the
band's; CH-AWGN has no paths and TX-ITU draws nothing, so every case at one speed and its
control, the wanted alone, share one band sample for sample.

**Texts.** The wanted: `CQ CQ CQ DE N0CALL N0CALL K` (`SyntheticCq.Text`). INT-ADJ and
INT-COCHAN: `DE N0AAA UP` (`CwTwoInOnePassband.OtherText`). INT-PILEUP: `DE N0AAA UP` at
525 Hz, `QRZ DE N0AAA` at 675 Hz, `5NN TU` at 750 Hz. As keyed at a wanted 18 WPM (from the
proof's sidecar lines): INT-ADJ at 25 WPM `DE N0AAA UP DE N0AAA UP DE N0AAA UP DE`; pileup
`DE N0AAA UP DE` (12 WPM), `QRZ DE N0AAA QRZ DE N0AAA QRZ DE N0AAA` (25 WPM),
`5NN TU 5NN TU 5NN TU 5NN TU 5NN TU` (18 WPM).

**Start times.** The wanted's first word `CQ` is 27 units and the word gap 7, so every keyed
interferer starts 34 dits after the 1.0 s lead-in: **4.400 s at 12 WPM, 3.267 s at 18 WPM,
2.632 s at 25 WPM**. A carrier starts at 0.000 s.

**Headroom factors** at a wanted 18 WPM, from the proof: none (x 1) for every INT-ADJ,
INT-COCHAN and every 0 dB carrier; about x 0.94 for a +10 dB carrier; about x 0.363 for a
+20 dB carrier; x 0.9925 for INT-PILEUP.

## 3. The proof

`TheInterferenceProfilesAreWhatTheySayTests`, 21 tests: 20 cases at a wanted 18 WPM (seed
469001) - INT-ADJ at (+100, 0, 25), (−100, 0, 25), (+50, −6, 25), (−50, −6, 25),
(+100, −2.4, 25), (+100, −6, 25); INT-COCHAN (+10, 0, 25); INT-CARRIER at +50, +100, +200,
+500 Hz by 0, +10, +20 dB; INT-PILEUP - and a fact that section 9.3's four ids are each produced.

Measured on the rendered audio before the band, never from the recipe (`CLAUDE.md` 12.5), by
code written for the test that shares nothing with the tracker, the survey or any front end:

- **pitch**: upward zero crossings, interpolated, over time, inside the runs of samples whose
  25 ms window is at key-down; the offset is layer less wanted, both measured, within **2 Hz**;
- **level**: the median power of the 25 ms windows (hop 5 ms) at or above half the loudest,
  layer over wanted, within **0.5 dB**;
- **keyed**: at least 5 falls from key-down to a hundredth of it, and a floor at least 20 dB
  under key-down; **steady**: every window of the whole case within **1 dB**, none falling;
- **INT-COCHAN**: the measured offset inside ±22.5 Hz, half the 45 Hz Hann detector bandwidth
  read from `CwProbabilisticDecoder.IntegratorBandwidthHz`, and not on the wanted's pitch;
- **INT-PILEUP**: at least 3 keyed layers inside the 350–870 Hz passband at pitches more than
  2 Hz from each other and from the wanted;
- **the mix**: every sample the band plus the wanted plus every layer, times the headroom
  factor, to within 1e-6; no sample over 0.9 FS; the band present in every 25 ms (V-06).

Two tones 10 Hz apart cannot be told apart in time on the mix, so each station is measured on
its own rendered layer and the mix is proved to be their sum.

**Watched red first**: with `HAMLET_UNIT469_INTERFERENCE=nominal` the mixer lays the wanted
station over itself - its pitch, its level, its keying - whatever the profile says. Red on all 20
interference cases (1 of 21 passed, the profile-list fact): every offset measured +0.000 Hz;
every stated level other than 0 dB off; every carrier keyed with 73 key-ups; INT-COCHAN on the
wanted's own pitch; INT-PILEUP 0 distinct stations. Saved as `.run-unit/unit469-int-red.txt`.
Then green, 21 of 21, `.run-unit/unit469-int-green.txt`: worst offset error 0.009 Hz, worst
level error 0.022 dB, every carrier's swing 0.04 dB or less.

## 4. The measurement - HM-REQ-060, 061, 062 and 066 on ours

2026-09-27, unit 469, `TheInterferenceIsMeasuredFact` (a printer; asserts only that 27 cases and
3 controls were read). **Every figure: synthetic, exact key** (`SyntheticCq.Text`, 21 characters,
7 words), CH-AWGN at 15 dB in the 2500 Hz reference on the wanted, decoder unchanged. Ours is
`CwDecoder` from 600 Hz hop by hop, scored by `TheRequirementsAreMeasuredTests.Measure` and
`CwMetrics` against the wanted key alone. Printout: `.run-unit/unit469-measure-t2.txt`.

**Readings, fixed before any decode and the arbiter's (DECIDED (5), (6)):** *held* - the tracked
pitch at each of the wanted's 7 word ends within 25 Hz of 600 Hz; *keyed station selected* -
every tracked pitch from the first sure character to the wanted's last key-up within 25 Hz of
600 Hz; *competing reported* - at any hop from the second station's first mark to the wanted's
last key-up, `CwDecodeReport.Competitor` names a tone within 25 Hz of the second station.
HM-REQ-010 is MET-CER-SURE below 1 % (so 0 errors at these counts; nothing sure is not met);
HM-REQ-011 is MET-INVENTED 0. A requirement is met only if all its cases are.

**Controls, the wanted alone on the same band:** 12 WPM 0/21, 18 WPM 0/21, both read whole;
**25 WPM MET-CER-SURE 1/22, MET-INVENTED 1/21**, `... N0CALL KK` - the wanted alone at 25 WPM
already adds a sure `K` (unit 468's TX-ITU finding), so no 25 WPM case can meet HM-REQ-010 here.

| req | interferer | wanted | ours read (key `CQ CQ CQ DE N0CALL N0CALL K`) | pitch at word ends | held / selected / competing | MET-CER-SURE | MET-INVENTED | case |
|---|---|---|---|---|---|---|---|---|
| 060 | INT-ADJ(+100, 0, 25) | 12 | `CQ CQ CQ DEE■H N0AAA UP DE N0AAA UP DE N0AAA ET TTT` | 600 600 600 600 **700** 600 600 | held **no** | 24/38 | 24/21 | not met |
| 060 | INT-ADJ(−100, 0, 25) | 12 | `CQ CQ CQ DEE■H N0AAA UP DE N0AAA UP DE N0AAA UP DE` | 600 600 600 600 **500 500 500** | held **no** | 23/37 | 23/21 | not met |
| 060 | INT-ADJ(+100, 0, 25) | 18 | `CQ CQ CQ DE N0CALL N0CALL K` | 600 … 575 | held | 0/21 | 0/21 | **met** |
| 060 | INT-ADJ(−100, 0, 25) | 18 | `CQ CQ CQ DE N0CALL N0CALL K` | 600 … 600 | held | 0/21 | 0/21 | **met** |
| 060 | INT-ADJ(+100, 0, 25) | 25 | `CQ CQ CQ DE N0CALL EA UP EALL KK` | 600 … 595 595 | held | 6/24 | 6/21 | not met |
| 060 | INT-ADJ(−100, 0, 25) | 25 | `CQ CQ CQ DE N0CALL EA UP DE L KK` | 600 595 600 … | held | 7/23 | 7/21 | not met |
| 061 | INT-ADJ(+50, −6, 25) | 12 | `CQ CQ CQ DE N0EE H UP SE N0AAA UT E DE N0A E ET K` | 600 600 600 600 **650** 600 600 | held **no** | 19/33 | 19/21 | not met |
| 061 | INT-ADJ(−50, −6, 25) | 12 | `CQ CQ CQ DE ESH N0AAA UP EI ET TT TTTE T TET TLL K` | 600 600 600 595 600 575 600 | held | 22/36 | 22/21 | not met |
| 061 | INT-ADJ(+50, −6, 25) | 18 | `CQ CQ CQ DE N0CALL NN0CALL K` | 600 … 575 | held | 1/22 | 1/21 | not met |
| 061 | INT-ADJ(−50, −6, 25) | 18 | `CQ CQ CQ DE N0CALL NN0CALL K` | 600 … 600 | held | 1/22 | 1/21 | not met |
| 061 | INT-ADJ(+50, −6, 25) | 25 | `CQ CQ CQ DE N0CALL N0CALL KK` | 600 … 605 … | held | 1/22 | 1/21 | not met (as its control) |
| 061 | INT-ADJ(−50, −6, 25) | 25 | `CQ CQ CQ DE N0CALL N0CALL KK` | 600 … 600 | held | 1/22 | 1/21 | not met (as its control) |
| 062 | INT-CARRIER(+50, 0) | 18 | `U   Q CQ DE I T<KN> TETE ET ETEE ETEE NN0ISL K` | **650** 600 605 605 610 600 600 | **the carrier** (334 of 3681 hops off) | 22/29 | 22/21 | not met |
| 062 | INT-CARRIER(+50, +10) | 18 | `T  E` | **650** 600 610 610 610 600 600 | **the carrier** (272 of 3619) | 1/2 | 1/21 | not met |
| 062 | INT-CARRIER(+50, +20) | 18 | `T T` | **650** 600 600 605 605 600 600 | **the carrier** (272 of 3619) | 2/2 | 2/21 | not met |
| 062 | INT-CARRIER(+100, 0) | 18 | `CT  EIQ CQ DE N0CALL N0CALL K` | **700** 600 … 575 | **the carrier** (300 of 3704) | 3/22 | 3/21 | not met |
| 062 | INT-CARRIER(+100, +10) | 18 | `T T  T CQ DE N0CAL■ N0CALL K` | **700** 600 … 575 | **the carrier** (272 of 3619) | 3/19 | 3/21 | not met |
| 062 | INT-CARRIER(+100, +20) | 18 | `T T E CQ DE N0CALL N0CALL K` | **700** 600 595 595 605 595 575 | **the carrier** (272 of 3619) | 3/20 | 3/21 | not met |
| 062 | INT-CARRIER(+200, 0) | 18 | `CQ CQ CQ DE N0CALL N0CALL K` | 600 … 575 | the keyed station | 0/21 | 0/21 | **met** |
| 062 | INT-CARRIER(+200, +10) | 18 | `T  T CQ DE N0CALL N0CALL K` | **800** 600 … 575 | **the carrier** (272 of 3619) | 2/19 | 2/21 | not met |
| 062 | INT-CARRIER(+200, +20) | 18 | `T T  T CQ DE N0CALL N0CALL K` | **800** 600 … 575 | **the carrier** (272 of 3619) | 3/20 | 3/21 | not met |
| 062 | INT-CARRIER(+500, 0) | 18 | `CQ CQ CQ DE N0CALL N0CALL K` | 600 … 575 | the keyed station | 0/21 | 0/21 | **met** |
| 062 | INT-CARRIER(+500, +10) | 18 | `CQ CQ CQ DE N0CALL N0CALL K` | 600 … 575 | the keyed station | 0/21 | 0/21 | **met** |
| 062 | INT-CARRIER(+500, +20) | 18 | `CQ CQ CQ DE N0CALL N0CALL K` | 600 … 575 | the keyed station | 0/21 | 0/21 | **met** |
| 066 | INT-ADJ(+100, 0, 25) | 18 | `CQ CQ CQ DE N0CALL N0CALL K` | 600 … 575 | competing named on 78 of 3400 hops: `125 Hz above at +4.3 dB relative (700 Hz)` | 0/21 | 0/21 | **met** |
| 066 | INT-ADJ(+100, −2.4, 25) | 18 | `CQ CQ CQ DE N00CALL N0CALL K` | 600 … 600 | named on 374 of 3400: `125 Hz above at -9.2 dB relative (725 Hz)` | 1/22 | 1/21 | **met** |
| 066 | INT-ADJ(+100, −6, 25) | 18 | `CQ CQ CQ DE N0CALL N0CALL K` | 600 … 600 | named on 392 of 3400: `125 Hz above at -12.9 dB relative (725 Hz)` | 0/21 | 0/21 | **met** |

Every control read `CQ CQ CQ DE N0CALL N0CALL K` whole at 12 and 18 WPM (pitch at word ends
`600 600 600 600 600 600 600` at 12, ending `575` at 18) and `... N0CALL KK` at 25.

**Per requirement, ours:**

- **HM-REQ-060: not met, 2 of 6 cases.** Met at 18 WPM on both sides. At 12 WPM the tracker is
  on the adjacent station at the fifth word end and ours prints the adjacent station's
  `N0AAA UP DE` as sure letters: the pitch was lost and the interferer's letters were printed
  as sure. At 25 WPM the pitch is held and the adjacent station's `EA UP` is printed sure
  inside the wanted's text.
- **HM-REQ-061: not met, 0 of 6.** At 12 WPM the adjacent station's letters are printed sure,
  once with the pitch lost (+50) and once with it held (−50). At 18 WPM one sure extra `N`;
  at 25 WPM the control's own extra `K` and nothing more.
- **HM-REQ-062: not met, 4 of 12.** The carrier is present from the first sample, so the
  tracker starts on it: at +50 and +100 Hz at every level and at +200 Hz at +10 and +20 dB,
  the tracked pitch at the first word's end is the carrier's and ours prints sure `T`s off it
  before moving to the keyed station. At +50 Hz, +10 and +20 dB, inside the detector's
  reach, the wanted is barely read at all. Met at +200 Hz 0 dB and at +500 Hz, where the
  carrier (1100 Hz) is outside the band's passband.
- **HM-REQ-066: met, 3 of 3, on the reading fixed before the decode.** The report names the
  second station on 2 %, 11 % and 12 % of the hops the two key together, never at 100 Hz: it
  names it only when a neighbouring 25 Hz bin puts it at 125 Hz, `CwCompetitor.SeparationHz`,
  the least offset the engine will name. The level it gives is +4.3 dB for a 0 dB station and
  about −9 and −13 dB for −2.4 and −6 dB stations. On a reading that asked for the report
  whenever both key, it would not be met; the reading is overrulable.

**Beside ours, for reading only:** the transcript the live path emits under
`CwSwitchTable.Live` as it stands is ours, character for character, in all 27 cases. The port
alone is worse in every case, or empty (the +50 Hz carrier at +20 dB and the +100 Hz carrier at
+10 and +20 dB).

## 5. What these fixtures do not prove

`CLAUDE.md` 12.5, V-04. An interferer here is a clean TX-ITU tone or a steady tone at one
level, with no fading, drift or chirp, keyed with the same 5 ms edges as the wanted. Nobody
tunes, nobody fades, nobody's fist is anything but a machine keyer. A result on them is a
result on section 9.3's words, not on a 40 m evening.

- **A carrier 500 Hz up is at 1100 Hz, outside the generator's 350–870 Hz band.** A real
  receiver's filter would take it down before the sound card; here it arrives at full level,
  which makes the case harder than the air, not easier.
- **Headroom scales the whole case.** A +20 dB carrier case has its band at about −49 dBFS
  against −40 dBFS in its control. Every SNR and ratio is as stated; the absolute level is not.
- **INT-COCHAN is one point, +10 Hz**, inside a detector bandwidth read as the integrator's
  45 Hz ENBW; nothing here says where "within" ends.
- **INT-PILEUP is three stations at equal level**, not a real pileup's dozen at a dozen levels.

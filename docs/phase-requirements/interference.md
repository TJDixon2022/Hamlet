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

## 4. What these fixtures do not prove

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

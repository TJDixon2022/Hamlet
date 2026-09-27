# TX-* sender profiles - what the generator keys, and the recipe

Work instruction 468, `PHASE_PLAN.md` 7.2, `CW_SPEC.md` section 10, HM-REQ-050. Built in
`tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/CwSender.cs`; proved by
`TheSenderProfilesAreWhatTheySayTests` in the same folder. Test fixtures only: audio
synthesis, not a keyer, and nothing reaches the radio (`CLAUDE.md` 0.2).

**Every parameter below is the arbiter's reading of `CW_SPEC.md` section 10 under R85, and
overrulable.** Where section 10 carries a `[verify]` mark, the row carries it too.

## 1. The profile list

One row per section 10 profile, in its order. Units are of the dit, which is `1200 / wpm` ms
at the character speed.

| id | tier | section 10's definition, quoted | parameter | number or range | source | `[verify]` | produced |
|---|---|---|---|---|---|---|---|
| TX-ITU | must | "Exactly 1:3:1:3:7. The KD0UN capture (3.06 / 2.89 / 6.92 units) is a real example." | dah; element, character, word gap | 3; 1, 3, 7 | section 10 | none | **produced** |
| TX-KEYER-W | must | "Electronic keyer with weight and ratio adjusted: ratio 2.5–3.5, gaps ±30 % [verify against WinKeyer documentation]." | dah | 2.5 to 3.5, drawn once per case | section 10 | `[verify against WinKeyer documentation]` | **produced** |
| | | | element, character, word gap | 1, 3, 7 each times its own factor 0.7 to 1.3, each drawn once per case | section 10 ("gaps ±30 %" read as ±30 % of each nominal gap) | `[verify against WinKeyer documentation]` | |
| TX-FARNS | must | "Character speed above overall speed; character gap 3–7 units at character speed, word gap longer. HM-DEC-115's traffic net is the vendored example." | character gap | 3 to 7 units at character speed, drawn once per case | section 10 | none | **produced** |
| | | | word gap | 8.7719 units (500 ms over a 57 ms dit) | section 10's vendored example, HM-DEC-115's traffic net (`CW_SPEC.md` 6.4) | none | |
| | | | dah; element gap | 3; 1 at character speed | TX-ITU nominal | none | |
| | | | character speed | the case's speed, 12, 18 or 25 WPM; the overall speed follows from the gaps and is below it | section 10 ("character speed above overall speed") | none | |
| TX-TIGHT | must | "Element gaps shorter than the dit; character gaps compressed. The 013347 capture (HM-DEC-101)." | dah; element, character, word gap | 283 / 105 = 2.6952; 65 / 105 = 0.6190, 130 / 105 = 1.2381, 280 / 105 = 2.6667 | the 013347 capture as HM-DEC-101 fitted the generator to it: `CwFixtureRecipe`'s defaults, dit 105 ms (`CwFixtureGenerator.cs` 38-57 and `KeyEdges`' remarks) | none in section 10; see section 4 item 2 on HM-DEC-145's figures | **produced** |
| TX-BUG | should | "Mechanical dits, hand dahs; ratio 3.5–5, dits short and fast, gaps variable." | dah | 3.5 to 5, drawn once per case | section 10 | none | **produced** (HM-REQ-052) |
| | | | element, character, word gap | 1, 3, 7 | TX-ITU nominal: "gaps variable" and "dits short and fast" carry no number | none | |
| TX-STRAIGHT | should | "Everything hand-timed; ratio and gaps drift; speed wanders." | - | - | - | - | **refused**: section 10 gives no number for how far the ratio and the gaps drift or how far the speed wanders, and that drift is the profile's defining feature. No range is invented (V-04). Source to vendor: `CW_SPEC.md` 12 item 6, Gold 1959. |
| TX-SLOPPY | later | "Morse Runner's "LID": inconsistent ratios, missing gaps, errors and corrections." | - | - | - | - | **refused**: section 10 gives no number for the inconsistency or for how often gaps go missing, and errors and corrections change the characters sent, which a sender here never does. Source to vendor: `CW_SPEC.md` 12 item 7, Morse Runner. |

The four must-tier profiles HM-REQ-050 names are all produced.

## 2. The recipe

Enough to rebuild any case byte for byte.

**Construction.** `CwSender.Render(id, channelId, snrDb, text, wpm, seed)`:

1. `CwSender.Timing(id, wpm, seed)` gives the four lengths in units of the dit `1200 / wpm` ms,
   drawing anything drawn from the seed.
2. `CwSender.Recipe(timing, text)` puts them into a `CwFixtureRecipe`: dit, dah, element gap,
   character gap and word gap in milliseconds; `Text` the key; `ToneHz` 600; `DriftHz` 0.
3. `CwChannel.RenderKeyed(channelId, snrDb, recipe, 600, seed, senderLine)` keys it with the
   generator's own `KeyEdges` and 5 ms raised-cosine `Gate`, and carries it over the channel
   exactly as unit 461's `CwChannel.Render` carries TX-ITU (`channels.md`). For CH-AWGN, that
   is one unfaded path over `CwFixtureGenerator.ShapedNoise(seed)` times 0.5, with the SNR in
   the 2500 Hz reference: tone power over the band's density at 600 Hz times 2500 Hz. There is
   never digital silence (V-06). `CwChannel.Render`'s own path now calls the same
   `RenderKeyed` with TX-ITU's recipe and its old sender line, so unit 461's cases render
   unchanged.
4. The sidecar is the channel's, followed by `txProfile`, `txDefinition`, `txSpeed` (character
   and overall), `txSeed`, `txSnr`, one `txParameter` line per length with its units,
   milliseconds and source, and `txKey`.

**The key is the text, exact by construction** (R61, V-13). A profile changes only when the key
goes down and up. It never changes a character: `CwChannel.Key` refuses any character the
table cannot spell, so nothing is in the key that is not in the audio.

**Seeds, fixed before any decode.** Each seed is `468000 + 100 x row + speed index`. The row is
the profile's row in section 10: TX-ITU 0, TX-KEYER-W 1, TX-FARNS 2, TX-TIGHT 3, TX-BUG 4. The
speed index is into `SyntheticCq.Speeds`: 12 WPM 0, 18 WPM 1, 25 WPM 2. So TX-FARNS at 18 WPM
is 468201. The same seed seeds the band (`ShapedNoise(seed)`), the channel paths
(`seed + 7919 k`) and the draws.

**How each drawn parameter is drawn.** The generator's xorshift32 is seeded
`(uint)seed ^ 0x7E5D`, and each draw is `((state & 0xFFFFFF) + 0.5) / 2^24`, uniform on
(0, 1), scaled linearly to the range. The order of draws:
- TX-KEYER-W: ratio, then the element, character and word factors.
- TX-FARNS: the character gap.
- TX-BUG: the ratio.
- TX-ITU and TX-TIGHT draw nothing.

One draw per case, not one per element. Electronic keyers and the bug's mechanical dits send
alike from one element to the next, and section 10 gives no jitter number for any produced
profile.

**The drawn values of task 1's fifteen cases**, as the green run's sidecars print them (units):

| profile | 12 WPM (seed) | 18 WPM (seed) | 25 WPM (seed) |
|---|---|---|---|
| TX-ITU | 3 : 1 : 3 : 7 (468000) | 3 : 1 : 3 : 7 (468001) | 3 : 1 : 3 : 7 (468002) |
| TX-KEYER-W | 3.3667 : 0.7302 : 2.8828 : 5.5765 (468100) | 3.3506 : 0.7115 : 3.2531 : 6.9448 (468101) | 3.3345 : 0.7291 : 3.8406 : 7.8123 (468102) |
| TX-FARNS | 3 : 1 : 4.5066 : 8.7719, overall 10.81 (468200) | 3 : 1 : 4.5672 : 8.7719, overall 16.18 (468201) | 3 : 1 : 4.6277 : 8.7719, overall 22.41 (468202) |
| TX-TIGHT | 2.6952 : 0.6190 : 1.2381 : 2.6667 (468300) | same (468301) | same (468302) |
| TX-BUG | 3.7535 : 1 : 3 : 7 (468400) | 3.7777 : 1 : 3 : 7 (468401) | 3.7052 : 1 : 3 : 7 (468402) |

Columns are dah : element gap : character gap : word gap. The overall speed is WPM over
`SyntheticCq.Text`, first mark to last.

**Read plainly:** the three KEYER-W ratios all fell between 3.33 and 3.37, and the three
FARNS character gaps between 4.51 and 4.63. The draws cover section 10's ranges only thinly
at three cases a profile. That is the fixed seeds' outcome, and nothing was re-seeded.

## 3. The proof

`TheSenderProfilesAreWhatTheySayTests.EachProducedProfileIsKeyedAsItsDefinitionStates`, for
the 15 cases, measures from `CwChannelParts.Signal`, the tone before the band is added. It
never reads the recipe. A mark is a run of samples the tone is keyed in, starting from the
silent sample at the key-down edge. Runs of zero samples under 2 ms (the tone crossing zero)
are bridged. The text says only which mark is a dit and which gap ends a character or a word.
The dit is the mean of the dits, and everything else is in units of it. The code shares
nothing with `CwUnitEstimator`, the lattice or any decoder timing code (`CLAUDE.md` 12.5).

It checks:
- the dah-to-dit ratio, and the element, character and word gaps, against the case's own
  timing, and against the definition's number or range;
- every mark and gap of a kind alike;
- the character speed from the measured dit;
- the overall speed over the text;
- for TX-FARNS, character speed above overall speed and word gap above character gap;
- for TX-TIGHT, element gaps below the dit and character gaps below 3;
- the key is the text;
- the sidecar carries the profile, seed, SNR, speed and every parameter;
- no output sample is digital silence.

**Tolerance.** The floor is 0.02 units. Above that, it is what the sampling allows: an edge is
found to one sample, so a length of G units over a dit of D samples can be out by
2 (1 + G) / D. That is 0.046 units for a 7.8-unit word gap at 25 WPM.

**Watched failing first.** With `HAMLET_UNIT468_SENDER=nominal`, each case keeps its timing and
its sidecar, but is keyed 1:3:1:3:7. The final red run failed all 12 cases of TX-KEYER-W,
TX-FARNS, TX-TIGHT and TX-BUG, and passed TX-ITU's 3
(`.run-unit/unit468-senders-red.txt`). Against `CwSender`, all 16 pass
(`.run-unit/unit468-senders-green.txt`).

The two earlier runs are kept, because each found a fault in the proof, not in the sender:
- `unit468-senders-red-first.txt`: TX-ITU at 25 WPM read 7.021 units for the word gap. The
  raised cosine is exactly zero at an on-grid edge, so counting keyed samples alone read every
  mark one sample short. Runs now start at the silent sample.
- `unit468-senders-green-first.txt`: TX-KEYER-W and TX-FARNS at 25 WPM were out by 0.022 and
  0.023 units on the word gap, against a flat 0.02, which is tighter than one sample per edge.
  The tolerance is now the sampling's.

## 4. What these fixtures do not prove

`CLAUDE.md` 12.5, V-04.

- **They are section 10's parameter ranges, not any operator's fist.** A floor measured on
  them is a floor on section 10's numbers and on this reading of them. It is not a floor on
  the band. No real hand-sent timing varies from one element to the next here: every dit of a
  case is the same length, and so is every gap of a kind.
- **Every `[verify]` value is unvendored.** TX-KEYER-W's ratio and gap ranges await the
  WinKeyer documentation. Gold 1959 (`CW_SPEC.md` 12 item 6) is the reference for hand-sent
  variability and is not in the tree.
- **TX-TIGHT is one capture's fist, as HM-DEC-101 fitted it:** dah 2.70, element gap 0.62,
  character gap 1.24 and word gap 2.67 units. HM-DEC-145's own measurement of the same
  capture reads dah 2.73, element gap 0.73 and character gap 1.49 from its callsign stretch,
  with no word gap. The two agree on the shape and differ by up to 0.25 units on the
  character gap. The generator's fit is used because it gives all four lengths from one
  source (section 4 of the report).
- **TX-FARNS's word gap is the traffic net's, 8.77 units, in every case.** Section 10 says only
  "longer". A Farnsworth sender whose word gap scales with its character gap is not
  represented.
- **TX-BUG's dits and gaps are nominal.** "Dits short and fast, gaps variable" has no number, so
  only the heavy dah is represented.
- **One tone at 600 Hz, no drift, no second station, CH-AWGN only in HM-REQ-050's cases.** The
  channel layer's own limits are in `channels.md`.
- **The characters are always `SyntheticCq.Text`'s.** A profile never changes what is sent, so
  nothing here tests a sender's errors, which is part of why TX-SLOPPY is refused.

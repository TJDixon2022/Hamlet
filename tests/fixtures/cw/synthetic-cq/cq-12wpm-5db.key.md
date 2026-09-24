# Answer key - cq-12wpm-5db

**Exact by construction.** This recording was generated, not received. The generator
keyed exactly the text below and nothing else, so the key is known for certain and
nobody had to read Morse to write it (PHASE_PLAN.md R61). It is labeled exact wherever
a number is reported against it.

**The key:**

    CQ CQ CQ DE N0CALL N0CALL K

**Scored region.** The whole decode against the whole key (`CwScorer.Whole`), the gaps
at the decode's two ends trimmed. Anything the decoder prints from the noise before the
first `C` or after the last `K` is its own error, because the generator sent nothing
there. `CwScorer.Within` is printed beside it for comparison and is not the number.

**The recipe**, enough to rebuild the file byte for byte (1.2). `CwFixtureRecipe` fed to
`CwFixtureGenerator.Generate`, written by `WavAudio.Write`; recipe fields not listed take
their defaults.

| field | value |
|---|---|
| Name | `cq-12wpm-5db` |
| Text | `CQ CQ CQ DE N0CALL N0CALL K` |
| speed | 12 wpm, the dit 1200 / 12 = 100 ms |
| DitMilliseconds | 1200.0 / 12, 100 to three places, 1 unit |
| DahMilliseconds | 3 x (1200.0 / 12), 300 to three places, 3 units |
| ElementGapMilliseconds | 1200.0 / 12, 100 to three places, 1 unit |
| CharacterGapMilliseconds | 3 x (1200.0 / 12), 300 to three places, 3 units |
| WordGapMilliseconds | 7 x (1200.0 / 12), 700 to three places, 7 units |
| SignalToNoiseDb | 5.0, tone RMS over noise RMS inside the 350-870 Hz passband |
| ToneHz | 615 |
| DriftHz | 3, either side, over 10 s |
| QsbHz, QsbDepthDb | 0, 0 - steady |
| PreambleSeconds | 0 - none |
| Seed | 20260925 |

The decode is `CwDecoder` at 8000 samples a second, fed hop by hop from a starting pitch
of 600 Hz, the path the floors use (`SyntheticCq.Read`).

**What this case does not prove** (1.4), in full in
`docs/phase-correctness/synthetic-cq.md`: the spacing is textbook, the same spacing the decoder
falls back to at `CwUnitEstimator.cs` 216, so a clean read here says nothing about a sender
who spaces letters five dits apart; the noise is generated, not the band's; one tone,
no second station; the keying is machine-perfect. **No synthetic case is ever the sole
evidence for keeping a change**, and this one is in no total that 3.2 judges.

# The CH-* channel profiles - how every case is built

Work instruction 461, PHASE_PLAN.md 7.1. Serves HM-REQ-013, 020, 040, 041 and 042 by making
them measurable; meets none of them. The code is
`tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/CwChannel.cs`; the proof is
`TheChannelProfilesAreWhatTheySayTests` beside it. Nothing under `src` is involved.

**What these fixtures do not prove (CLAUDE.md 12.5, V-04):** they are the published Watterson
model, not the band. A decoder that reads them has read the model; nothing here says how it reads
a real HF path. Every value marked `[verify]` is unvendored (CW_SPEC.md 12 item 2), so a floor
measured on it is a floor on the spec's number, not on ITU-R F.1487's.

## The profiles

CW_SPEC.md 9.1, as written, with 9.1's status carried into every case's sidecar.

| id | condition | delay (ms) | delay (samples) | spread (Hz) | sigma (Hz) | tap rate (/s) | status |
|---|---|---|---|---|---|---|---|
| CH-LQ | Low, quiet | 0.5 | 4 | 0.5 | 0.25 | 32 | [verify] |
| CH-LM | Low, moderate | 2 | 16 | 1.5 | 0.75 | 96 | confirmed |
| CH-LD | Low, disturbed | 6 | 48 | 10 | 5 | 640 | [verify] |
| CH-MQ | Mid, quiet | 0.5 | 4 | 0.1 | 0.05 | 6.4 | [verify] |
| CH-MM | Mid, moderate | 1 | 8 | 0.5 | 0.25 | 32 | [verify] |
| CH-MD | Mid, disturbed | 2 | 16 | 1 | 0.5 | 64 | [verify] |
| CH-HQ | High, quiet | 1 | 8 | 0.5 | 0.25 | 32 | [verify] |
| CH-HM | High, moderate | 3 | 24 | 10 | 5 | 640 | [verify] |
| CH-HD | High, disturbed | 7 | 56 | 30 | 15 | 1920 | [verify] |
| CH-AWGN | No fading, white noise | - | - | - | - | - | project baseline |
| CH-MDV | Mid, disturbed NVIS | **refused** | | | | | 9.1: [read from Annex 3] |

**CH-MDV is refused by name.** 9.1 gives no delay or spread. The call throws
`NotSupportedException`, saying that 9.1 gives no values and that the source to vendor is
CW_SPEC.md 12 item 2, ITU-R F.1487 Annex 3. No value is invented for it.

## The construction

For a keyed envelope `e(t)` and a pitch `f0`, the tone is

    s(t) = Re{ A [ h1(t) e(t) + h2(t) e(t - tau) ] exp(j 2 pi f0 t) }

and the output is `s(t) + n(t)`, clamped to +/-1. The clamp never acts: every case the proof
builds reports 0 samples clipped.

- **`e(t)`, TX-ITU.** `SyntheticCq.Recipe(wpm, 0, seed)` gives exactly 1:3:1:3:7 from the dit,
  where the dit is `1200 / wpm` ms. It carries `Text` = the key, `ToneHz` = the pitch and
  `DriftHz = 0`. It is keyed through `CwFixtureGenerator.KeyEdges`, which gives a lead-in of
  1.0 s before the first mark, and `CwFixtureGenerator.Gate`, which gives a raised-cosine edge
  of 5 ms inside each element. The case runs for 1.5 s after the last edge. The pitch is fixed
  and does not drift.
- **Two fading paths**, `h1` and `h2`, for every profile except CH-AWGN. They are independent.
  Each is `g_k / sqrt(2)`, where `g_k` is a unit-power complex Gaussian process. So the two
  paths carry equal power, and their sum carries the unfaded power: E|h1|^2 + E|h2|^2 = 1.
- **The delay** `tau` is the profile's delay in whole samples at 8000 Hz. Path two carries
  the envelope `tau` later: `e[n - round(tau * 8000)]`.
- **CH-AWGN** has `h1 = 1` and no second path.
- **`A`**, the unfaded tone's peak, is set from the SNR (below).

### The Doppler filter

Each `g_k` is built as follows:

1. **Sample rate.** The process is sampled at **64 x spread** taps a second, which is
   128 sigma, with sigma = spread / 2.
2. **The white noise.** Complex white Gaussian noise of unit power: xorshift32 uniforms and
   Box-Muller, the generator's own pair, each part at half the power.
3. **The filter.** The noise goes through an FIR taken from the Gaussian
   `h[m] = exp(-m^2 / (2 s^2))`, with `s = 128 / (2 pi sqrt 2) = 14.40` taps whatever the
   spread. It runs over `m = -73 .. 73` (5 s either side) and is normalised to
   `sum h[m]^2 = 1`. Its power response is `exp(-f^2 / (2 sigma^2))`: a Gaussian Doppler
   spectrum of two-sigma width = the spread, with no shift. Only fully overlapped outputs are
   used, so there is no start-up transient.
4. **To audio rate.** Linear interpolation to 8000 Hz, at `u = n * taprate / 8000`. The
   images near the tap rate sit about 42 dB down.

"Spread" means two sigma of each path's Gaussian Doppler power spectrum (R85). That is the
reading of Watterson, Juroshek and Bensema, IEEE Trans. Commun. Technol. COM-18(6), 1970, and
of CCIR Rec. 520 / ITU-R F.1487. Those sources are unvendored; this is how their published
descriptions define the spread.

### The noise band

- **What it is.** `CwFixtureGenerator.ShapedNoise(buffer, seed)`: 350 to 870 Hz, three
  cascaded RBJ bandpass biquads at 551.8 Hz with Q 1.061, and the white skirt 30 dB down in
  power (about 42 dB down in density). It is then multiplied by **0.5**, giving an RMS of
  0.01 (-40 dBFS), so a faded peak at +15 dB reference stays under full scale.
- **Where it goes in.** After the fading, and it is never faded. There is no digital silence
  anywhere (V-06).
- **Its equivalent noise bandwidth** is the band's total power over its one-sided density at
  the stated frequency. Measured (unit 461 task 1, two 410 s runs) against the three biquads'
  prediction:

| referred to | measured | predicted | offset to 2500 Hz |
|---|---|---|---|
| peak | 290.2 Hz | 293.5 Hz | -9.35 dB |
| 600 Hz | 320.1 Hz | 319.8 Hz | -8.93 dB |
| 615 Hz | 340.1 Hz | 342.1 Hz | -8.66 dB |

### The SNR, in the 2500 Hz reference

CW_SPEC.md 8.1 and 8.2: the noise is measured beside the tone.

    SNR_2500 = S / (N0(f0) * 2500 Hz)

- `S = A^2 / 2` is the tone's unfaded mean power in a mark. When the tone fades, S is also its
  long-run mean, because the two paths sum to unit power.
- `N0(f0)` is the band's one-sided density at the pitch, times 0.5^2. The layer computes it
  from the three biquads' response and the skirt (`CwChannel.BandDensity`). Measured and
  predicted agree within 0.05 dB (the table above).
- So `A = sqrt(2 * 10^(SNR/10) * N0(f0) * 2500)`.
- The in-passband figure, tone power over band power, is `SNR_2500 - offset`. At 600 Hz that
  is `SNR_2500 + 8.93 dB`. Every sidecar prints both.

### Sample rate

8000 samples a second, the generator's.

### The seed rule

One `int seed` per case:

- The band is `ShapedNoise(seed)`.
- Path k's white noise is xorshift32 seeded `(uint)(seed + 7919 k)` for k = 1, 2, with 0 read
  as 1.
- The same seed gives the same audio byte for byte. The proof asserts it.
- The unit's cases use seeds 461100 + the profile's row index above (LQ 0 ... AWGN 9). Task 3
  uses 461400 + 10 x row + the seed index 0 to 2.

## How to call it

    CwChannelCase c = CwChannel.Generate(
        profileId: "CH-LM",       // any id above; CH-MDV throws
        snrDb: -4,                // in the 2500 Hz reference
        message: SyntheticCq.Text,// letters, figures, prosign carets; spaces split words
        wordsPerMinute: 20,       // TX-ITU at this speed
        pitchHz: 600,
        seed: 461101);
    // c.Audio   - MonoAudio, 8000 Hz
    // c.Key     - the message as keyed, exact by construction
    // c.Sidecar - this page's recipe for the case, with the profile's status

A character the Morse table cannot key throws. It is never skipped, because a skipped
character would be in the key and not in the audio.

## What the proof measures

Everything is measured from the output: the audio, the pieces summed into it, and the path
gains applied. Nothing is read back from the recipe. The tolerances were stated before the
first run.

| # | measure | how | tolerance and why |
|---|---|---|---|
| 1 | spread | two sigma from the second moment of a Welch spectrum of each path's tap process, 2^18 taps | 10 %: the estimate scatters about 2 %; the nearest wrong readings are 41 % and 100 % out |
| 2 | power | long-run mean power of each path's taps; their ratio and their sum; each rendered path against its tap process over the same samples | 0.3 dB: a long-run mean scatters about 0.05 dB; the wrong builds are 3 dB or infinitely out |
| 3 | delay | onsets of every mark on path two against path one, off the tap-applied signals | one sample, 0.125 ms, the resolution |
| 4 | SNR | CH-AWGN at -7, -4 and +15 dB: tone power in marks over the band's measured density at the pitch (Welch, +/-20 Hz) x 2500 | 0.5 dB, the instruction's |
| 5 | V-06 | every 10 ms block of every output, at -7 and +15 dB, against the band's RMS before the first mark; and no clipping | no block 30 dB under: the band alone falls that far with a chance near 5e-9 a block |
| 6 | the key | the returned key equals the message | exact |
| 7 | determinism | the same seed gives byte-identical audio and sidecar; another seed does not | exact |
| 8 | CH-MDV | refused, with 9.1's silence and 12 item 2 in the reason | exact |

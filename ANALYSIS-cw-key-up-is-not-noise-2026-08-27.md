# The key-up state is not the noise floor, 2026-08-27

Measured outside Hamlet, from the WAV files, by the web session that wrote
work instruction 035. **Reproduce these in-tree before acting on them.**

## The stations are not weak

Narrowband SNR at the station's own pitch, signal power in ±30 Hz against the
median noise density 100–400 Hz either side:

| capture | pitch | band SNR |
|---|---|---|
| `cw-2026-08-22-014113` | 606.0 Hz | **16.6 dB** |
| `cw-2026-08-22-014308` | 606.0 Hz | **25.7 dB** |
| `cw-2026-08-24-012403` (reads) | 439.8 Hz | 37.3 dB |

Unit 1.11.31 concluded these stations are below the decoder's sensitivity.
**They are not.** 25.7 dB is a comfortable armchair copy.

## They are Morse

Envelope autocorrelation, first peak, and the phase-step spread over strong
samples:

| capture | first peak | second | phase-step sd |
|---|---|---|---|
| `014113` | 110 ms | 214 ms | 0.84° |
| `014308` | 118 ms | 243 ms | 1.23° |
| `012403` (reads) | 114 ms | 237 ms | 0.22° |

The same envelope periodicity as a capture that decodes a callsign, and stable
phase — not a phase-shift-keyed data mode. **These are keyed carriers.**

## Within-mark stability is not the difference

Median standard deviation of the dB envelope inside marks of 30 ms or longer:
`014113` 1.55 dB, `014308` 1.66 dB, `012403` 1.37 dB. **Flutter is not what
separates them.**

## What does differ: the two states, and where key-up sits

Two-means fitted to the dB envelope, 40 Hz integrator, against the band noise
floor measured at the same bandwidth 250 Hz away:

| capture | key-down | key-up | separation | key-up above band floor |
|---|---|---|---|---|
| `012403` reads | −22.6 | −36.6 | **14.1 dB** | **31.8 dB** |
| `014113` unread | −25.4 | −37.3 | **11.9 dB** | **18.5 dB** |
| `014308` unread | −28.1 | −39.2 | **11.1 dB** | **31.3 dB** |

**On all three — including the one that reads — the key-up state sits 18 to 32
decibels above the band noise floor.**

## Why that matters to this decoder

`CwProbabilisticDecoder.LogLikelihoods` scores key-up as **noise**, with the
scale taken from the envelope's own lower quartile. It is being asked to
explain a key-up state that is not noise. Where the two states separate by
14 dB it carries anyway; at 11–12 dB it does not, and the window ratio lands
at 0.84 and 0.44 against a floor of 1.40.

## What the published work does instead

**RSCW** (PA3FWM) sets its threshold so that *the average distance between the
threshold and the samples above it equals the average distance between the
threshold and the samples below* — both states fitted from the data.

**`cwdecoder.py`, in this repository's own root**, fits two means to the dB
envelope per window and thresholds between them. It reads these captures.

Neither assumes the key-up state is the noise floor.

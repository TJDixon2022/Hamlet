# CWNET - the learned CW reader: what the engine must compute

**Second model (unit 598).** The model `cwnet.onnx` (ONNX opset 17, 5.6 MB) maps a feature
matrix to per-frame letter scores. It is the first model trained further, on scenes the first
never saw: the owner's radio as measured on KM3STU's captures - a DSP passband that is a wall
(25 dB down within 60 Hz of the edge), a noise floor that sits *after* the AGC and so shows as a
bright shadow of the keying in the bins outside the passband, an AGC that rides the noise itself
and flickers in the gaps, a sender within 140 Hz of the passband's top edge - plus speeds down
to 5 WPM and silent scenes. It has still never heard a recording. **The feature recipe (§2), the
alphabet (§3) and the streaming rule (§4) are unchanged from the first model; only the weights
differ.** `cwnet-vector.wav` and `cwnet-vector-feat.csv` are byte-for-byte the first model's;
`cwnet-vector-ids.txt` is regenerated (one frame of 599 differs) and the text is still
`ROGER TNX FER CALL`. The vector's tone is at 579 Hz (bin 10-11); its letters run from 0.26 s
to 10.56 s, 18 characters with spaces, about 21 WPM by letter rate.

Read here, before any code was written, the second model printed (the first model's read in
brackets where it differed):

- Ken (0-120 s of the draft-truth capture), 73 of 89 letters by draft truth [66]:
  `F N DEALL4ANST SZ958N QTHIINWKENSENGTON,PT PANRPITTS NIURGH TU NAME IS KEN KEN SGHW+ KB2UM3DEN3ANS TN3AFS DE KB2UZY/M`
- Bryan (120-300 s), 125 of 144 [128]:
  `LOGM KEN NCE TOMSUNX FER RPRT URR 579 57N LILNAM IN MOBILE STN Y NAME BRYAN BRYAN ES QTH ISCHICOPEG, MA CHICOPEE M, MA PARKED IN MY MOBILE  SO BTLKEN N3AFS DE KB2UZY/ MK RRDE NIWARS N IY TOME`
- KM3STU `cw-2026-10-08-121324` (0-60 s): `2KW3SOEE CQ POTA DE KM3STUK KQ4PAK` [`JKM3SKE CQ`]
- KM3STU `cw-2026-10-08-121357` (0-60 s): `K RRTNX AGN URNN MK RR UR 55N 55N VA VA BK BKT` [`L RX RR`]
- KM3STU `cw-2026-10-08-121414` (0-60 s): `RNKU  S72M3ST CQP` [`R`] - still mostly lost; with the
  bins above his tone blanked by hand the same net prints `R 55N 55N VA VA BK BK TU RON VA S72 KM3STU CQP`,
  so what is left is the shadow, not the keying
- W1AW piece-03 (0-60 s): `WITH GROUND ENHANCEMENT MATERIAL GEFORE BURYING THE GROUND ROD. YOU CAN GET GEM FROM AN ELECTRICAL SUPPLY HOUSE. ATTACI THE GROUND TOTHE ANTENNAS RADIAL PLATE AND BOND CONNECTIT TO YOUR MAIN STATIO`
- W1AW piece-04 (0-60 s): `OAN COMPARE SIGNALS. TORTH EAST WEAK SIGNAL GROUP AND THE SAN DIEGO MIKROWAVE GROUP GET TOGETHER TO COMPARE AND MEASURE STATIONS. IT IS A CHANCE TO COMPARE MINIMUM DIS CE`
- the first recording: `FER CHAT= BST 7V 73 ! KC4ZGP DE WA`, 26 of 27 [same]

If the engine's port prints these stretches like this, the port is right. If it prints
garbage where these are readable, the port is wrong - the features, the resampling or the
window handling - never the model.

## 1. Input: audio

- Mono, **8,000 samples per second**, float in -1..1. Audio at the engine's native rate is
  **decimated with an anti-alias low-pass** (cut-off at or below 3.4 kHz) first. Integer
  ratios (48 kHz / 6, 16 kHz / 2) may use a windowed-sinc FIR then take every n-th sample.
- The level does not matter (the features normalise it); clipping does.

## 2. Features: one matrix per 12-second window

Given a window `x` of N samples (N = 96,000 for 12 s; any length of at least 1 s works):

1. **STFT.** Frame length 256, hop 80 (10 ms), **periodic Hann** window
   `w[n] = 0.5 - 0.5 cos(2 pi n / 256)`, n = 0..255 (sum = 128). No padding at either end:
   frame `t` covers samples `80 t .. 80 t + 255`, so T = floor((N - 256) / 80) + 1 frames
   (1,197 for 12 s). Spectrum `Z[t,k] = (1/128) * sum_n w[n] x[80 t + n] e^(-2 pi i k n / 256)`.
2. **Bins.** Keep k = 8..33 inclusive (250 Hz to 1,031 Hz at 31.25 Hz per bin): **26 bins**,
   in that order.
3. **Log power.** `L[t,b] = 10 log10(|Z|^2 + 1e-10)`.
4. **Cancel broadband gain (the receiver's AGC), per frame:** subtract from each row its
   **median over the 26 bins** (median of 26 values = mean of the 13th and 14th sorted).
5. **Noise-floor shape, per bin:** subtract from each column its **10th percentile over the
   T frames** (NumPy's default: linear interpolation between order statistics - sort the
   column, position p = 0.10 (T - 1), value = v[floor p] + (p - floor p)(v[floor p + 1] - v[floor p])).
6. **Scale, whole matrix:** `s = P97(L) - P10(L)` over all T x 26 values (same interpolation);
   `L = L / (s + 3)`.
7. Feed as float32, shape `[1, T, 26]`, input name `feat`.

`cwnet-vector-feat.csv` is this matrix for `cwnet-vector.wav` (12 s, 8 kHz, 16-bit; read it
as `sample / 32768`). The engine's matrix must match it to **1e-3** in every cell.

## 3. Output: letters per frame

Output name `logits`, shape `[1, T2, 47]`, T2 = ceil(T / 2) (one output frame per **20 ms**;
output frame `j` covers input frames `2j` and `2j + 1`, i.e. audio around `20 j` ms from the
window's start). Index 0 is the CTC blank. Indices 1..46 are, in order:

```
 1 ' '   2 '!'=<SK>  3 '&'=<AS>  4 '('=<KN>  5 '+'=<AR>  6 ','  7 '.'  8 '/'
 9 '0' 10 '1' 11 '2' 12 '3' 13 '4' 14 '5' 15 '6' 16 '7' 17 '8' 18 '9'
19 '='=<BT>  20 '?'
21 'A' 22 'B' 23 'C' 24 'D' 25 'E' 26 'F' 27 'G' 28 'H' 29 'I' 30 'J' 31 'K' 32 'L' 33 'M'
34 'N' 35 'O' 36 'P' 37 'Q' 38 'R' 39 'S' 40 'T' 41 'U' 42 'V' 43 'W' 44 'X' 45 'Y' 46 'Z'
```

**Greedy decode:** take the argmax index per output frame; walk the frames in time; emit a
character when the index changes and is not 0. (`prev` starts at 0.) Index 1 is a word space.
`cwnet-vector-ids.txt` holds the 599 argmax indices for the vector (this model's); `cwnet-vector-text.txt`
the decoded text `ROGER TNX FER CALL`. The engine must reproduce both.

## 4. Streaming: how the engine reads live

The net needs context on both sides of a letter, so the engine runs it on a **sliding 12 s
window, once a second**, and commits only frames that have at least **2 s of audio after
them** (the sequence reader's commit lag). Precisely, with absolute time in output frames of
20 ms:

- Keep a ring of the last 12 s of 8 kHz audio. Every second (every 8,000 new samples), take
  the whole ring (or everything so far, if under 12 s - at least 1 s), compute the features,
  run the net.
- Output frame `j` of a run whose window starts at absolute sample `S` is absolute frame
  `F = S / 160 + j`.
- A frame is **committed** when `F * 160 + 160 <= now - 16,000` (2 s behind the newest
  sample). Keep a single `committedUpTo` absolute frame; on each run, for every frame from
  `committedUpTo` up to the newest committable one, append its argmax index to the stream,
  and advance `committedUpTo`. Frames newer than that are provisional and are decided by a
  later run.
- The greedy collapse runs over the committed index stream **continuously** - `prev` carries
  across runs - so a letter spanning two runs is emitted once.
- A window shorter than 12 s at the start (the first seconds after the reader starts) is
  read as it is; its early frames commit as soon as they are 2 s old.

Cost: one 12 s window is about **35 ms on one core** in ONNX Runtime with
`IntraOpNumThreads = 1`. Set that; do not let the runtime take every core.

## 5. What the model does not do

- It does not find a pitch, a lane, a mark or a gap; it prints letters.
- It reads the **louder** of two stations; it was trained with a second station 4-25 dB down.
- It knows no words. Its errors are letter-gap splits and merges (`NMARS` for `N3AFS`,
  `GEFOR` for `BEFORE`); a vocabulary-aware decode on its scores is the next step, not this unit's.
- It prints a word space itself (index 1); the engine adds no space rule on top.

# PSK31 fixtures

All 8000 Hz, 16-bit, mono WAV. Generated 2026-09-11 by `assets/reference-modem.py`
on a machine with no radio (FACT-004: an indication, never a finding).

**Modulation convention used to make them, which the demodulator must match:**
BPSK at 31.25 baud, 256 samples per bit at 8 kHz. Differential: a `0` bit is a phase
reversal, a `1` bit is no change. Raised-cosine pulse shaping (alpha = 1) so the
envelope passes through zero at every reversal. Each character is its varicode
followed by `00`; idle is a run of `0`s (continuous reversals). 40 idle bits before
the text and 40 after.

**SNR** is referenced to a 2500 Hz bandwidth, the way an FT8 report is, so `-3 dB`
here is about `+16 dB` inside PSK31's own 31 Hz. These are not weak-signal fixtures;
weak-signal fixtures are a later unit's work and want real off-air audio.

`manifest.json` carries the SHA-256, duration, carrier, note, the exact text, and
the reference decoder's character error rate (`reference_cer`, edit distance over
reference length) and decoded length for each file. The reference decoder is the
Python in `reference-modem.py`: no squelch, crude matched filter, squaring AFC. It
emitted **112 garbage characters** from the noise-only file, which is the reason a
squelch rule is required and must be stated.

`qso-text.txt` is the exact text in the QSO fixtures, with CR LF line ends as sent.

## Added by work instruction 314 task 5

`psk31-snr-10db-1000hz.wav` - one rung below the weakest of the originals: **-10 dB
referenced to 2500 Hz**, which is about **+9 dB** inside PSK31's own 31 Hz. Made by
`assets/make-weak-fixture.py`, which imports `reference-modem.py` rather than
reimplementing the convention, with the seed fixed at 314 so the file can be made again
byte for byte.

**It is a measurement and not a gate.** `manifest.json` carries no `reference_cer` for
it, because the offline reference was not re-run over it, and no character error rate is
asserted against it anywhere. Hamlet's own decoder read it at **CER 0.1502**, 221
characters of 255, on 2026-09-11.

**It is still not weak-signal work.** That wants real off-air audio.


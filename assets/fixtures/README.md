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

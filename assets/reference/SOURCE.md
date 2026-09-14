# How the Olivia fixtures were made

`olivia-fixture-generator.cpp` drives **Pawel Jalocha's reference MFSK transmitter** -
`jalocha/pj_mfsk.h` and its headers, exactly as shipped in fldigi (`src/include/jalocha/`,
GPL-3, master branch 2026-09-14) - the way fldigi's own `src/olivia/olivia.cxx` drives it:
Tones, Bandwidth, SampleRate 8000, FirstCarrierMultiplier from the center and the
fc_offset formula, Preset, Start, PutChar while fewer than BitsPerSymbol characters are
queued, Stop, Output until Running is false. Jalocha wrote Olivia; this is the mode
author's own encoder, so the fixtures are **independent of anything Hamlet thinks Olivia
is**. That independence is the lesson of the PSK31 phase, whose fixtures were made by
the same hand that wrote the demodulator.

The RSID bursts in front of the fixtures were made by a port of fldigi's `cRsId::Encode`
and `cRsId::send` (`src/rsid/rsid.cxx`): 15 tones at 11025/1024 Hz spacing, 1024/11025 s
each, starting seven tone-steps below the center, five symbols of silence either side;
the `Squares` and `indices` tables and the mode codes are in `../data/rsid-codes.json`.

**Nothing here is built by a session.** It is read. The sessions build Hamlet's own
demodulator and modulator and prove them against these files.

Compiled and run on 2026-09-14 with g++ on Linux; `manifest.json` carries each file's
SHA-256, variant, center, whether an RSID burst precedes it, and the exact text.

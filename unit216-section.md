
## The join, and the first messages off somebody else's air — unit 216

**The two halves of a receiver meet.** For fourteen units this library could speak. Since unit 214 it
could see — hand it fifteen seconds of audio and it answers with a ranked list of *places* where
transmissions are, 56 of 56 at rank 1. Since unit 215 it could repair a damaged codeword or refuse
one, 0 wrong messages in 37 952 trials. **Nothing joined the two**, and unit 215's own report said so
in those words: no message had come off the air, in that unit or in any unit before it.

**Tonight it did, and tonight the number stopped being ours.** Every receive-side measurement this
phase had taken was against a signal this library synthesized itself, and a port that is wrong in the
same way at both ends of a round trip passes all of them. This unit decoded `ft8_lib`'s own off-air
recordings — somebody else's antenna, somebody else's band, somebody else's stations — against the
expected decode lists checked in beside them.

### What was ported, and from where

`src/Ft8Sharp/Dsp/Ft8SoftSymbols.cs`, from the pin at `9fec6ca39886edbf96f4f5e71edc76da5074e871`:

- **`ft8/decode.c`, `get_cand_mag`** — how a candidate's four position fields become one offset into
  the waterfall store.
- **`ft8/decode.c`, `ft8_extract_likelihood`** — the walk over the 58 data symbols, the step over the
  three sync blocks, and what happens to a symbol whose block falls outside the slot.
- **`ft8/decode.c`, `ft8_extract_symbol`** — the eight tone magnitudes and the three ratios they give.
- **`ft8/decode.c`, `ftx_normalize_logl`** — the rescale to a fixed variance, which unit 215 read and
  deliberately left for this unit.
- **`ft8/decode.c`, `ftx_decode_candidate`** — the order of the three steps, and that there is exactly
  one attempt per candidate.
- **`demo/decode_ft8.c`, `decode`** — the loop over candidates, the de-duplication rule, and the four
  application constants.

**Read and deliberately NOT ported:** `ft8_decode_multi_symbols`, the multi-symbol hypothesis. It is
declared once and defined once in `ft8/decode.c` and **never called** —
`UpstreamExtractionInventoryTests` asserts the count of mentions is exactly two, so that a re-pin
which starts calling it goes red. This is the same treatment unit 215 gave `ldpc_decode`.

### The shapes, as shapes

- **The candidate indexes the store in the store's own axis order**, block outermost and bin fastest,
  by folding time offset, time sub-offset, frequency sub-offset and bin offset into one running
  product.
- **The scorer and extraction enter through the same helper.** This is the structural fact that
  matters most: `ft8_sync_score` and `ft8_extract_likelihood` both open with `get_cand_mag`, so the
  blocks extraction reads are *by construction* the blocks the search scored. The port keeps the
  property by the same means — both go through `Ft8Waterfall.IndexOf` with the candidate's own fields
  and nothing else. **Unit 214 carried the block-to-sample alignment forward as unsettled; reading
  extraction does not settle it in the absolute, and it does settle that the two sides cannot
  disagree with each other.**
- **The sync blocks are stepped over, not through.** Data symbol *k* of 58 is channel symbol *k+7* for
  the first twenty-nine and *k+14* for the rest. The port does not lay this out a second time: it
  walks `Ft8SymbolEncoder.IsSyncSymbol`, and the inventory test asserts the two agree on all 58.
- **A symbol whose block falls outside the waterfall gives three zero ratios.** Not a refusal and not
  a skip. A zero is *no opinion*: the decoder is told nothing about those three bits and the code's
  redundancy supplies them. The search sweeps from ten blocks before the slot on purpose, so refusing
  those candidates would throw away the ones the sweep exists to catch.
- **The magnitudes are read as decibels, where the scorer reads the raw stored byte.** Two different
  reads of the same store, half a decibel per count apart, and both are kept.
- **The eight magnitudes are gathered in VALUE order through the FORWARD Gray map.** `s2[v]` is the
  strength of the tone that would have carried symbol value *v*.
- **Each ratio is a maximum over the four values whose bit is one, less a maximum over the four whose
  bit is zero.** Positive means the bit is one — unit 215's reading, conformed to and not re-argued.
  The port derives the partition from the bit position, and the inventory test asserts the derived
  partition equals upstream's three written lines, term for term.
- **The normalisation takes the population variance of all 174 with the mean removed *from the
  variance*, and multiplies every ratio by the square root of a fixed target over it.** The mean is
  never subtracted from the ratios themselves.
- **One attempt per candidate.** No sweep over neighbouring time or frequency offsets, no second
  hypothesis, no retry.
- **Duplicates are decided on the whole packed payload**, with the CRC used only to pick a hash
  bucket, and the text is produced only *after* a decode has been found to be new.

### The anchoring split

**7 strong, 8 weak, 3 weakest.**

Strong: the extraction and decode entry points and their signatures; the waterfall struct and its
documented axis order; the two magnitude macros; the data and channel symbol counts; the codeword
length; the Gray map's declaration.

Weak — every one an expression inside a static function body: the candidate's fold into an offset;
the `k + (k<29 ? 7 : 14)` step; the zero-fill for an out-of-range block; the value-order gather; the
three bit partitions; the variance formula; the extract-normalise-decode order; the payload
comparison in the demo.

Weakest: **the normalisation's target variance**, whose own comment beside it calls it an
"experimentally found coefficient" — one number chosen by measurement rather than derived from
anything; and the four application constants, which live in `demo/decode_ft8.c` and appear nowhere in
`ft8/`.

### The four application constants, and that nothing was tuned

All four are file-scope constants in `demo/decode_ft8.c` and none of them appears anywhere in `ft8/`.
**All four already match this library's defaults**: the minimum sync score and the candidate limit on
`Ft8SyncSearch` since unit 214, the LDPC iteration count on `LdpcDecoder` since unit 215, and the
decoded-message limit on `Ft8SlotDecoder` as of tonight — that fourth one had no counterpart before,
because nothing in this library returned a *list* of messages until the whole path existed.

**Nothing was tuned and nothing needed correcting.** When the reference recordings came back at 58.6
per cent, a candidate list of eight times the default was swept **as a measurement** and printed: it
returns the same 117 of 183 with the same 6 extras at 140, 280, 560 and 1120. The default remains
upstream's 140.

### The normalisation, measured

Ratios built at five input magnitudes, with variances of **0.0595, 0.9523, 22.8555, 380.9222 and
9523.05**, all land on **24.0000** after the rescale, with every sign untouched. The raw ratios
extraction delivers are differences of decibels, tens of dB apart, so their variance before
normalisation is far above 24; after it they are 24, which is the scale `fast_tanh`'s clamp at ±4.97
was tuned against. **Unit 215 measured that this decoder is not scale-free** — its soft sweep printed
the variance beside every row and said where its arrays left upstream's scale. That caveat is now
discharged: the path normalises, and it normalises to the figure read out of the pin rather than to
one chosen here.

### The reference recordings, and which rung criterion 3 stood on

**Rung 1 — a checked-in expected-decode file beside the recordings.** `test/wav` and
`test/wav/20m_busy` hold **69 recordings**, of which **60 carry a `.txt` file named for the
recording**, holding one line per message in the format `decode_ft8` prints. **1298 expected messages
in all.** Every one of the 60 is mono, 16-bit, 12 000 Hz and 15 seconds. The nine without a list are
`websdr_test14` to `20` at 6400 Hz and two 12 kHz re-samples of them.

**AND THE LISTS WERE NOT WRITTEN BY THE PINNED DECODER, which is provable from the lists themselves.**
`decode_ft8` computes its signal-to-noise column as `cand->score * 0.5f`, and `ftx_find_candidates`
refuses a candidate scoring below `kMin_score`, which is 10 — **so the lowest SNR the pinned decoder
can print is +5.0.** The column in these files runs **−24.0 to +20.0**, and **1078 of the 1298 lines
are below +5.0.** Some of them also carry a trailing country annotation that its `printf` does not
emit. So they are a *stronger* reference than the code this library was ported from, and **a shortfall
against them is not by itself evidence that this port is worse than `ft8_lib`.** Turning it into that
evidence needs `decode_ft8.exe`, which is not built on this machine — `HM-OPEN-065`.

**The normalisation applied to both sides of every comparison**, stated once and in one place: the
text is everything after the first tilde, trimmed, up to a run of **two or more spaces**. Nothing else
— no brackets stripped, no case folded, and `RR73` and `RRR` stay different messages. An FT8 message
is single-space separated between tokens, so a run of two is an unambiguous boundary and is what
separates a message from a country annotation.

**Hashed callsigns are compared like any other line and not excused.** 141 of the 1298 expected lines
name a station by an unresolved hash, printed as `<...>` — which is what upstream's own hash table
produced from the same recording. Both sides are in the same position and the comparison is fair.

### The de-duplication rule, and how the port reaches it

Upstream keys duplicates on the **whole packed payload**, ten bytes holding the 77 message bits and
their own CRC-14. The port compares **the 77 message bits**, and that is the same partition: the CRC
is a function of those bits, so two decodes agree on the payload exactly when they agree on the
message.

**One thing about how it gets them is worth writing down.** `Ft8CodewordDecoder` does not hand back
the codeword it accepted, and it is closed evidence this unit may not change, so `Ft8SlotDecoder`
recovers the bits by running the same deterministic decoder over the same ratios — **only for
candidates that already passed the gate**, so it costs one belief propagation per successful decode
and none per refusal. It is not a second CRC check; there is still exactly one of those in this
library. A later unit that wants to tidy it would have the gate return the payload.

### What tonight's evidence is

- **760 of 1298 expected messages matched, across 60 of upstream's own off-air recordings, on rung 1.**
  538 missed. **23 returned that are not on any expected list, out of 783 returned**, every one of
  them printed in full in the test output. Per-stage totals: 7803 candidates, 2733 reached parity,
  2733 passed the checksum, 2263 became text, 783 unique. **One file produced nothing** —
  `191111_110115.wav`, candidates found and none reaching parity. **No file skipped for its rate.**
- **56 of 56 corpus messages extract at 174 of 174 hard decisions** before any correction is involved,
  at four frequencies including exactly halfway between two bins and four offsets including two off
  both grids.
- **The alignment has exactly one place it works.** One block either side of the candidate the search
  returns gives 103 and 105 of 174; two blocks 100 and 98; the wrong time sub-offset 139; the wrong
  bin 113 and 131; the wrong frequency sub-offset 149. Chance is 87.
- **51 of 56 corpus messages come back as themselves through the whole path**, and the five that do
  not are the hashed-callsign entries, refused — and step 2's own decoder refuses the same 77 bits.
  **288 of 288** across the offset sweep. **51 of 51 in seeded noise** at a delivered −9.961 to
  −10.028 dB. **0 wrong messages** in every one of those.
- **20 overlapping transmissions become 20 messages, twice** — clean and in noise at a delivered
  −10.020 dB — with **0 extra that were not transmitted.**
- **Criterion 2 in the candidate sense**: an empty slot 0 messages; 239 candidates found in seeded
  noise over 20 slots and **0 messages**; **51 genuine transmissions carrying a wrong checksum, 114 of
  their candidates reaching parity with ZERO unsatisfied checks out of 83 — genuine members of the
  code — and 0 of 51 returning anything**; and 51 transmissions at −30 dB returning **0 wrong text**.

### What tonight's evidence explicitly does not show

- **It does not show that this port matches `ft8_lib`.** The expected lists are not the pinned
  decoder's output. Nothing here was held against `decode_ft8` running, because it is not built on
  this machine.
- **It does not show why 538 expected messages did not come back**, only where they did not die.
  **509 of the 531 misses — 95.9 per cent — had a kept candidate within 4 Hz of the frequency the list
  gives for them.** The place was found and the message was not recovered from it. That points at
  extraction fidelity or at the code's correcting power at real signal levels, and away from the
  search. **Eight times the candidate list changes nothing.** **Twenty clean overlapping transmissions
  all decode**, so it is not about having more than one signal in a slot.
- **It is not a sensitivity measurement.** Delivered ratios are stated for the fixtures and are not
  compared with the published threshold. That is step 6's.
- **Nothing here has been near a radio, a clock or a screen.** No audio came from an antenna, nothing
  is scheduled to a UTC slot, and nothing reaches a display.

### Divergences from upstream

**Three added, numbered on from twenty-one.**

**22 — a candidate whose eighth tone falls outside the kept bins is refused, where upstream reads past
the end of its array.** `ft8_extract_symbol` indexes eight bins from the candidate's own with no
bounds check at all; upstream relies on `ftx_find_candidates` never proposing such a candidate, and
`Ft8SyncSearch` has the same guarantee. But a caller can build a candidate by hand, and **there is no
faithful port of reading past the end of an array**, so this refuses with both numbers in the message.

**23 — 174 identical ratios are left alone, where upstream divides by zero.** `ftx_normalize_logl`
computes `sqrtf(24.0f / variance)` with no guard; an array whose variance is zero gives an infinity or
a NaN, and every ratio is then multiplied by it. It cannot arise from a real waterfall and it can
arise from a synthetic one, so the port returns the zero variance and changes nothing.

**24 — a full message list stops accepting, where upstream loops forever.** The demo's duplicate check
probes forward for an empty hash-table slot and terminates only on an empty slot or a duplicate; with
`kMax_decoded_messages` messages already stored and a new one arriving, neither is ever found. **The
case cannot arise in upstream's own use** — no slot it has been run on produces 50 distinct decodes —
and it is an infinite loop rather than a wrong answer, so it is not something to reproduce.
`Ft8SlotDecoder` stops adding at the limit and carries on.

**An addition that is not a divergence, recorded so it is not mistaken for one:** `Ft8SlotResult`
carries five stage counts — candidates, parity satisfied, checksum passed, became text, duplicates
suppressed — which upstream reports only through a debug log. They change no decision the path makes.
They exist because a report that says *nothing decoded* and stops is useless to whoever has to find
out why, and **candidates found with nothing past parity is a different fact from no candidates at
all.** They are five integers on a result type and are explicitly **not** the legibility surface the
plan parks as a phase of its own.

**A change to a test-project instrument, not a divergence and not library code:** `WavFile` now walks
the chunks after `fmt` to find `data` rather than requiring it to be second. Upstream's generator
always writes it second; **nine of the sixty reference recordings put a 158-byte chunk in between**,
and refusing those would have narrowed criterion 3 to fifty-one files for a reason that has nothing to
do with the audio. Every other refusal it carries is unchanged and still watched.

### The library's version

`0.9.0` → **`0.10.0`** under HM-DEC-152. The library gains the capability the whole phase is for:
**it can take audio and return the text that was in it.**

**What that does *not* claim.** **No audio comes from a radio** — every sample it has been given came
from a file or from its own synthesizer. **Nothing is scheduled to a UTC slot**; it decodes a buffer
somebody hands it and has no idea what time it is. **Nothing reaches a screen.** And **the sensitivity
is unmeasured** — 760 of 1298 against upstream's own recordings is where it stands tonight, that
figure is not comparable with any published threshold, and the measurement that would be is step 6's.
Still not a 1.x, and the reason is now a number rather than a missing piece.

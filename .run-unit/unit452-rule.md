# Unit 452 task 2 - the rule, set from the trace before any numbers were run

**Group chosen: G1**, 21 real and 18 synthetic of the 94 wrong boundaries - an inserted space kept
by the relabel against the path's textbook word-from (4.58 units) in a read that measured no
character gap while the stream held no gaps of the sender's own. Its cause is in the audio: on
each of these senders the kept gap is a letter space of 4.6 to 7 units of the unit in force and
shorter than that sender's shortest word space, and the letter and word spaces do not overlap
(`.run-unit/unit452-wbe-trace.txt`, `spaces` rows). The character gap was not measured because
the envelope's three heaps found no character heap: in 8 reads a key-up of 15 to 20 ms took the
shortest heap, in 29 there was no trough between the first two heaps, and 2 reads had under 12
gaps.

**Not attacked:** G2 (9 real, a measured character gap under the letter space - the measurement,
not the fallback, is wrong there); G3 and G7 (17:37, 7 real, overlapping spacing, recorded no by
unit 444); G4 (5 synthetic, the relabel's 1.53 share over the character-gap-5 sender's 1.4 ratio);
G5 and G6 (9 real, 24 synthetic at 0 dB, letters not read - not a spacing).

**The rule, in one sentence:** where a read measures no character gap and the stream holds no gaps
of the sender's own, the relabel takes the median of that read's own gaps between letters, when
it has at least the 12 the estimator asks for, as this sender's character gap, and takes out a
word gap shorter than it times the root of seven thirds, never lowering the boundary below the
path's textbook word-from.

- **R72:** it reads durations only - the spans the path already placed between letters - and never
  which letters they are.
- **How it differs from unit 444's rule (3 (d)):** 444 dropped key-ups under half a dit from the
  envelope's gap clustering in both `MeasureGaps` and `MeasureCharacterGap`, which moved the held
  structure the path reads with and so the letters. This touches neither estimator, the path, the
  held gaps, the speed or any letter. It only raises the relabel's fallback boundary, from the gaps
  the path placed between letters rather than from the envelope's heaps, so it can only ever take a
  space out. It answers the same windows 444's did where dropouts split the heaps, and also the 29
  with no trough and no dropout's help needed.
- **One value, tried once.** The share is the relabel's own `WordGapShare`; the median and the 12
  are the estimator's own habits. Nothing is chosen from a score.

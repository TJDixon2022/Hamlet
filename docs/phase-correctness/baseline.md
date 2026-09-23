# The correctness phase's baseline

**Measured 2026-09-23 by work instruction 410 at HEAD `bd875351`, whose `src` is byte-identical
to `7ba9690d`.** Printed by `TheBaselineIsScoredTests.TheBaselineIsPrinted`; raw output in
`.run-unit/unit410-baseline.txt`. Every number is three parts (PHASE_PLAN.md §3.1): edits,
scored length, and whether the key is exact or inferred. **Every key in this table is
inferred** (R61), and a disagreement with an inferred key is an indication, not proof the
decoder is wrong (FACT-004, CLAUDE.md 0.0).

**The phase's baseline: 33 edits over 46 characters, against inferred keys, over 4
recordings.**

## How it is scored

- **Edits** are Levenshtein between the scored region and the key, **spaces included**,
  nothing collapsed or folded (`tests/Hamlet.RadioEngine.Tests/Cw/CwScorer.cs`).
- **Scored length** is the key's length, spaces included. The decode's region length is
  printed beside it because the two differ.
- **The decode** is what `CwDecoder` settles fed hop by hop from the WAV at a starting pitch
  of 600 Hz, the path the floors and the anchors use.
- **17:37's region is its key file's**: from the first `C` of the first `CQ` to the last
  character emitted, gaps at its two ends trimmed, scored whole. Everything before it is
  unscored.
- **The adjudicated texts name no region.** Each covers a fragment of a longer recording, so
  the whole text is aligned to the stretch of the decode that fits it best and nothing on
  either side is scored (`CwScorer.Within`). Scoring the whole decode against a callsign
  would count everything else the recording holds as the decoder's error, and would be a
  key for audio nobody read.
- **Kind.** The 17:37 key file says *inferred, not transcribed*. The adjudicated texts come
  from `TheAdjudicatedReadingsKeepReadingTests.All`, whose provenance is a ruling
  (HM-DEC-145, HM-DEC-144, HM-DEC-126) or Tim's adjudication, and says neither exact nor
  transcribed. They are marked inferred, as the instruction orders when the file does not
  say.

## The baseline

| recording | edits | scored length | key | region length | scored region of the decode | key |
|---|---|---|---|---|---|---|
| `cw-2026-09-23-173723` | 29 | 25 | inferred | 48 | `CQ CQ CQ DE W T E E T E  E ERE D E T T TB 7E E I` | `CQ CQ CQ DE WB6RED WB6RED` |
| `cw-2026-08-17-013347` | 0 | 6 | inferred | 6 | `VA3VRR` | `VA3VRR` |
| `cw-2026-08-17-134712` | 1 | 3 | inferred | 2 | `N4` | `N4L` |
| `cw-2026-08-18-003758` | 3 | 12 | inferred | 12 | `EETMP/4 QNIK` | `AA4MP/4 QNIK` |
| **total** | **33** | **46** | **inferred** | | | |

`134712`'s `N4L` is retired as a reading anchor (Tim, 2026-08-30) and is scored here all the
same: retiring it withdrew the requirement that it read, not the ruling that `N4L` was sent.

## Outside the baseline - the other adjudicated recordings

`TheAdjudicatedReadingsKeepReadingTests` holds twelve recordings, not three. The nine the
instruction does not name are scored by the same rule and **kept out of the baseline total**
until a ruling says whether they join it (output.md section 4).

| recording | edits | scored length | key | region length | note |
|---|---|---|---|---|---|
| `cw-2026-08-24-012403` | 0 | 16 | inferred | 16 | work instruction 011 |
| `cw-2026-08-18-004507` | 15 | 57 | inferred | 67 | HM-DEC-115 |
| `cw-2026-08-22-031838` | 21 | 35 | inferred | 30 | Tim 2026-08-25 |
| `cw-2026-08-22-031905` | 17 | 39 | inferred | 39 | Tim 2026-08-25, retired as an anchor |
| `cw-2026-08-22-031948` | 2 | 36 | inferred | 35 | Tim 2026-08-25 |
| `cw-2026-08-22-032012` | 13 | 51 | inferred | 59 | Tim 2026-08-25 |
| `cw-2026-08-22-032050` | 22 | 59 | inferred | 52 | Tim 2026-08-25, retired as an anchor |
| `cw-2026-08-22-032113` | 11 | 28 | inferred | 34 | Tim 2026-08-25, retired as an anchor |
| `cw-2026-08-22-032129` | 23 | 42 | inferred | 31 | Tim 2026-08-25, retired as an anchor |
| **outside, total** | **124** | **363** | **inferred** | | |

## Running total

| unit | change | 17:37 | baseline total |
|---|---|---|---|
| 410 | none, the baseline | 29 edits over 25 characters, inferred key | 33 edits over 46 characters, inferred keys |

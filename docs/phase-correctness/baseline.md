# The correctness phase's baseline

**Measured 2026-09-23 by work instruction 410 at HEAD `bd875351`, whose `src` is byte-identical
to `7ba9690d`.** Printed by `TheBaselineIsScoredTests.TheBaselineIsPrinted`; raw output in
`.run-unit/unit410-baseline.txt`. Every number is three parts (PHASE_PLAN.md §3.1): edits,
scored length, and whether the key is exact or inferred. **Every key in this table is
inferred** (R61), and a disagreement with an inferred key is an indication, not proof the
decoder is wrong (FACT-004, CLAUDE.md 0.0).

**The phase's baseline: 33 edits over 46 characters, against inferred keys, over 4
recordings; 0 unsure per 47 named.**

**Re-issued by work instruction 412 (PHASE_PLAN.md 2.1, 2.2)** at HEAD `02ce4602`, whose `src`
is byte-identical to `bd875351`'s: every number unchanged, and two columns added. **Unsure per
named** is the guard R59 carries beside every correctness number: characters in the scored
region the decoder settled unsure (a placeholder, or a letter below high confidence, as
`CwDecoder` counts `CharactersUnsure`) over the named characters in the same region (neither
a space nor a placeholder; a prosign such as `<BT>` counts by the letters of its text, as it
does in the scored length). **Named floor** is how many named characters the recording must
read at all, over the whole recording, set here from what each reads at HEAD and asserted by
`TheNumberCannotBeGamedTests.EachKeyedRecordingIsReadAtAll`; a change that drops below it is a
regression whatever its edit count. Raw output in `.run-unit/unit412-baseline-restored.txt` and
`.run-unit/unit412-named-restored.txt`.

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

| recording | edits | scored length | key | region length | unsure per named | named floor | scored region of the decode | key |
|---|---|---|---|---|---|---|---|---|
| `cw-2026-09-23-173723` | 29 | 25 | inferred | 48 | 0 / 28 | 46 | `CQ CQ CQ DE W T E E T E  E ERE D E T T TB 7E E I` | `CQ CQ CQ DE WB6RED WB6RED` |
| `cw-2026-08-17-013347` | 0 | 6 | inferred | 6 | 0 / 6 | 57 | `VA3VRR` | `VA3VRR` |
| `cw-2026-08-17-134712` | 1 | 3 | inferred | 3 | 0 / 2 | 21 | `N4 ` | `N4L` |
| `cw-2026-08-18-003758` | 3 | 12 | inferred | 12 | 0 / 11 | 44 | `EETMP/4 QNIK` | `AA4MP/4 QNIK` |
| **total** | **33** | **46** | **inferred** | | **0 / 47** | **168** | | |

`134712`'s `N4L` is retired as a reading anchor (Tim, 2026-08-30) and is scored here all the
same: retiring it withdrew the requirement that it read, not the ruling that `N4L` was sent.

## Outside the baseline - the other adjudicated recordings

`TheAdjudicatedReadingsKeepReadingTests` holds twelve recordings, not three. The nine the
instruction does not name are scored by the same rule and **kept out of the baseline total**
until a ruling says whether they join it (output.md section 4).

| recording | edits | scored length | key | region length | unsure per named | named floor | note |
|---|---|---|---|---|---|---|---|
| `cw-2026-08-24-012403` | 0 | 16 | inferred | 16 | 0 / 13 | 21 | work instruction 011 |
| `cw-2026-08-18-004507` | 15 | 57 | inferred | 70 | 0 / 46 | 49 | HM-DEC-115 |
| `cw-2026-08-22-031838` | 21 | 35 | inferred | 36 | 1 / 23 | 43 | Tim 2026-08-25 |
| `cw-2026-08-22-031905` | 17 | 39 | inferred | 42 | 0 / 30 | 36 | Tim 2026-08-25, retired as an anchor |
| `cw-2026-08-22-031948` | 2 | 36 | inferred | 35 | 0 / 27 | 31 | Tim 2026-08-25 |
| `cw-2026-08-22-032012` | 13 | 51 | inferred | 59 | 0 / 43 | 43 | Tim 2026-08-25 |
| `cw-2026-08-22-032050` | 22 | 59 | inferred | 52 | 1 / 37 | 44 | Tim 2026-08-25, retired as an anchor |
| `cw-2026-08-22-032113` | 11 | 28 | inferred | 34 | 2 / 22 | 47 | Tim 2026-08-25, retired as an anchor |
| `cw-2026-08-22-032129` | 23 | 42 | inferred | 36 | 0 / 28 | 65 | Tim 2026-08-25, retired as an anchor |
| **outside, total** | **124** | **363** | **inferred** | | **4 / 269** | **379** | |

**What the guard column says at HEAD.** Unsure is nearly always nought: 4 of 316 named
characters across all thirteen regions, every one a placeholder. The emission bar of unit 408
leaves out what it cannot name rather than marking it, so the decoder today prints almost
nothing it is unsure of; the ratio earns its keep when a change starts printing doubt, and the
named floor is the half of the guard that bites on silence.

## What kind of error (0.3)

Printed by `TheBaselineIsScoredTests.TheErrorKindsArePrinted`, raw output in
`.run-unit/unit410-baseline-kinds.txt`. Counted from the scorer's alignment, not asserted.
An edit touching a space is a word boundary misplaced and nothing else: *space added* is a
space where none was sent, *space missing* none where one was. **Letters-only** is the same
pair scored with every space taken out of both - the edits left if boundaries cost nothing,
which depends on no tie rule.

| recording | edits | wrong | missing | added | space added | space missing | boundaries | letters-only edits |
|---|---|---|---|---|---|---|---|---|
| `cw-2026-09-23-173723` | 29 | 4 | 0 | 10 | 15 | 0 | **15** | 14 |
| `cw-2026-08-17-013347` | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `cw-2026-08-17-134712` | 1 | 0 | 0 | 0 | 1 | 0 | 1 | 1 |
| `cw-2026-08-18-003758` | 3 | 3 | 0 | 0 | 0 | 0 | 0 | 3 |
| **total** | **33** | **7** | **0** | **10** | **16** | **0** | **16** | **18** |

**17:37's alignment**, key over decode, `_` a space, `.` same, `x` wrong, `-` missing, `+`
added:

    key    CQ_CQ_CQ_DE_W             B6RE D      _WB  6RE D
    decode CQ_CQ_CQ_DE_W_T_E_E_T_E__E_ERE_D_E_T_T_TB_7E_E_I
    edit   .............+++++++++++++xx..+.++++++.x.++xx.+x

**What the counts say.** On 17:37 at the bench, word boundaries are the largest single kind,
15 of 29 edits, every one a space where none was sent; the other 14 are letters, 4 wrong and
10 added, and 14 edits remain even when spaces cost nothing. **At the bench the letters are
not all right.** Where the key has `WB6RED` the bench reads `W T E E T E  E ERE D`: the
stretch that should be `B` (`-...`) and `6` (`-....`) comes out as seven single-element
letters, `T` and `E`, each followed by a space - what a gap inside a character read as a
gap between words would print. The seven do not map element for element onto the nine
elements of `B6`, so that reading is the shape of the fault, not a trace of it; step 3's 3.1
is where it is traced. The sidecar's `DEW B 6 RE D W B`, which
PHASE_PLAN.md quotes and in which every letter is right, is the application's reading on the
day, not the bench's replay of the WAV. Over the four baseline recordings: 16 boundaries, 7
letters wrong, 10 letters added, none missing; 18 edits letters-only.

**How the tie rule bears on it.** Where two alignments reach the fewest edits, the scorer
takes the one with the fewest letter edits (`CwScorer`'s remarks). That leans toward
counting a boundary: on `134712` the missing `L` against a trailing space is counted as a
space added, where the first rule written counted it as a letter missing. The edit counts
do not move with the rule; the letters-only column is the check that does not depend on it.

## The guard, watched working (2.3)

Work instruction 412 made two deliberate changes that make the decoder quieter, measured each
with `TheBaselineIsScoredTests` and `TheNumberCannotBeGamedTests`, and took each back out
before anything was committed. Raw output in `.run-unit/unit412-baseline-suppressed*.txt`
and `.run-unit/unit412-named-suppressed*.txt`.

**The one that games the number: leave out every single-element character.** One condition
added to `CwProbabilisticDecoder.Judged`, `&& character.Pattern.Length > 1`, so no `E` and no
`T` is ever printed. It takes away 197 of the 547 named characters the thirteen keyed
recordings read, about a third, and:

| | baseline total | 17:37 | named floors |
|---|---|---|---|
| at HEAD | 33 edits over 46 characters, inferred keys | 29 edits over 25 | 13 of 13 held |
| `E` and `T` left out | **21 edits over 46 characters, inferred keys** | 18 edits over 25 | **13 of 13 broken** |

Twelve edits better and every floor red: 173723 reads 27 named against 46, 013347 27 against
57, 134712 10 against 21, 003758 30 against 44, down to 031948's 28 against 31. The anchors'
keys hold no `E` or `T`, so they lose nothing that is scored, and 17:37's `W T E E T E` junk
goes. **By its edits alone this is the best change the phase has seen, and it is a decoder
that cannot print the commonest letter in English.**

**The one that goes quietest: the emission bar raised from 1.0 to 100.** It does suppress
most of the output: the 13 keyed recordings keep 97 of their 547 named characters, 0 to 47 each, and 17:37 keeps
none at all, so its region is empty and it scores 25 edits over 25, better than 29. But it
breaks `VA3VRR` and the baseline total goes the other way, **42 edits over 46**, with 13 of 13
floors broken. Bars of 10 and 5 did the same, 34 edits each. Silence pays only on a key that
is scored whole; on a key aligned into its best stretch it costs.

Both taken back out: `src/Hamlet.RadioEngine/Cw` prints nothing against the entry HEAD, and
the re-run reads 33 edits over 46 and 13 of 13 floors held.

## Running total

| unit | change | 17:37 | baseline total |
|---|---|---|---|
| 410 | none, the baseline | 29 edits over 25 characters, inferred key | 33 edits over 46 characters, inferred keys |
| 412 | none kept; the guard's two columns added, 0 unsure per 47 named | 29 edits over 25 characters, inferred key, 0 unsure per 28 named | 33 edits over 46 characters, inferred keys, 0 unsure per 47 named |

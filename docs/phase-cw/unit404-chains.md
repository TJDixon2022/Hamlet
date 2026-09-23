# Unit 404 - the fourteen, traced to their chains

Task 1 of work instruction 404, criterion 4.7 of step 4. No code was written for this table. Every
number here is an indication (FACT-004). `Cw\` means `src\Hamlet.RadioEngine\Cw\`. Piece numbers
are `docs\phase-cw\unit395-rework.md` section 1.2's.

## 1. How the trace was run

- **A piece** is `git diff <hash>^ <hash> -- src/Hamlet.RadioEngine/Cw` with the eleven transmit
  files excluded, made by `.run-unit\unit404-pieces.sh` into `.run-unit\unit404-patches\`. No
  piece's `Cw` diff touches a transmit file; the exclusion removed nothing.
- **The check** is `.run-unit\unit404-trial.sh`, on a scratch repository holding HEAD's `Cw`
  folder (`bc2484d5`, `src` as `7e65aac4` left it). It applies each link of a chain in order,
  file by file, then runs `git apply --check` on each of the target's files. A file whose change
  is already in the tree - the reverse applies, or the piece creates a file HEAD already has - is
  dropped under unit 395's decision 3 and not counted as a refusal. A refused hunk in a link is
  applied around with `--reject` and counted against that link.
- **The search** is `.run-unit\unit404-shrink.sh`. Each of the fourteen was tried alone, then
  after the dependencies its row names, then after the whole prefix - every earlier piece of the
  45, oldest first. From the whole prefix, links were removed newest first; a removal was kept
  when the target refused no more hunks than the whole prefix left refusing and the remaining
  links together refused no more than before. What is left is irreducible: removing any one link
  makes the target or another link refuse. For `9c2a7f99` the search started from `aeea24f2`'s
  chain plus `aeea24f2`, a set already shown to clear it.
- Logs: `.run-unit\unit404-trace-alone.txt`, `-trace-named.txt`, `-trace-prefix.txt`,
  `-shrink-<n>.txt`, `-trace-merged.txt`.

## 2. Each of the fourteen, alone and in its minimal chain

| n | Hash | Refusing alone on HEAD | Minimal chain, oldest first | Hunks still refusing after the chain |
|---|---|---|---|---|
| 10 | 4786c7e7 | 5, `CwToneSurvey.cs` | 7, 9 | none |
| 11 | f2e1db7a | 4, `CwToneSurvey.cs` 3, `CwToneTracker.cs` 1 | 7, 9 | none |
| 13 | 386fdb5d | 5, `CwDecoder.cs:422`, `CwProbabilisticDecoder.cs` 3, `CwProbabilisticStream.cs:148` | 2, 3, 5, 12 | none in 13; link 12 refuses `CwDecodeReport.cs:58` |
| 15 | 4c6e4321 | 4 | 3, 12 | `CwDecodeReport.cs:65`; `CwPitchChoice.cs` already in the tree, dropped |
| 19 | 0f2089f3 | 6 | 1, 3, 12, 15 | `CwDecodeReport.cs:77`; `CwPitchChoice.cs` already in the tree, dropped |
| 21 | 62262b94 | 1, `CwDecoder.cs:382` | 1, 3, 12, 15, 19 | none |
| 24 | fc1ee77f | 8, `CwProbabilisticDecoder.cs` | 2, 3, 5, 12, 13 | none |
| 26 | 68a18d66 | 8 | 2, 3, 5, 12, 13, 24 | none |
| 27 | a91d8fe7 | 9, and `CwProbabilisticDecoder.Posterior.cs` missing | 1, 2, 3, 5, 12, 13, 15, 19, 24, 25, 26 | none |
| 29 | efcd5242 | 5, `CwDecoder.cs` | 1, 3, 12, 15, 19, 21 | none |
| 37 | aeea24f2 | 6, `CwDecoder.cs` | 1, 3, 12, 15, 19, 21, 29, 30 | none |
| 38 | a37cfcff | 8, and `CwJointCutter.cs` missing | 1, 2, 3, 5, 12, 13, 15, 19, 24, 25, 26, 27, 34 | none |
| 40 | ee2cba8d | 2, `CwUnitEstimator.cs:348`, `:694` | 31, 33, 34 | none |
| 44 | 9c2a7f99 | 5, `CwDecoder.cs` | 1, 3, 12, 15, 19, 21, 29, 30, 37 | none |

Every refusal left anywhere in these chains is one of three hunks in `CwDecodeReport.cs` - piece
12's at line 58, 15's at 65, 19's at 77. Each adds a record parameter where HEAD carries unit
392's seam properties `PitchWasAsserted => false` and `PitchChoice`, so no piece of the 45 clears
them; they are seams for task 2 under unit 395's decisions 3 and 4, as piece 12's was in unit 396.

**Two build links, a decision made by the session.** The apply check cannot see a type. Piece 19
calls `CwPitchRanking.Rank` and `CwPitchRanking.WindowSeconds`, which piece 18, `b48d1158`,
creates; piece 29 calls `CwSpectralPeak.Find`, which piece 28, `ade52536`, creates. Neither type
is at HEAD, and no other piece in these chains names a type that is neither at HEAD nor created
inside the chain (`.run-unit\unit404-types.sh`). Without 18 and 28 the chain cannot compile by
construction, and its verdict would be a build error that says nothing about the work. **They are
added as links**, each applying clean where it falls. Piece 18's own build error in unit 396,
`CoarseSpacingHz` private, is met inside the chain: piece 1 makes it public. Author's,
overrulable.

**The keying-meter half of `9c2a7f99` is not taken.** Section 9 of the instruction: it rides only
if the chain needs it to build. The piece's `CwDecoder.cs` hunks do not name the meter; its
`CwKeyingMeter.cs` hunk is left out.

## 3. The grouping

The instruction's rules, author's and overrulable: a chain nested in another is judged as the
longer, and two chains that share any piece are merged.

- 10 and 11 have the same chain, 7 and 9. Merged: **7, 9, 10, 11**. Piece 11 applies after 10.
- 13, 15, 19, 21, 24, 26, 27, 29, 37, 38 and 44 all contain pieces 3 and 12. Merged.
- 40's chain, 31, 33, 34, shares piece 34 with 38's. Merged into the same.

**Unit 395's three groups became two.** The lattice and the decoder's hooks are not separate:
`fc1ee77f`'s chain runs through piece 12, `f27174b5`, the operator's assertion, whose `CwDecoder`
hunk piece 13 stands on; and `a91d8fe7` and `a37cfcff` stand on the hook pieces 15 and 19.

Every link was checked in the merged order (`.run-unit\unit404-trace-merged.txt`): each of the
fourteen passes after the links before it, and the only refusals are the three `CwDecodeReport`
seam hunks.

## 4. The chain table

| Chain | Carries, of the fourteen | Pieces, oldest first | Hunks that would still refuse | Transmit files; halves R50 leaves out |
|---|---|---|---|---|
| **S, the survey** - 4 pieces | 10 `4786c7e7`, 11 `f2e1db7a` | 7 `7fb89d5e`, 9 `44cf3fc8`, 10 `4786c7e7`, 11 `f2e1db7a` | none | no transmit file. Outside `Cw`: `Directory.Build.props` from 10, not taken. No app, tools or test half. |
| **D, the decoder** - 24 pieces | 13 `386fdb5d`, 15 `4c6e4321`, 19 `0f2089f3`, 21 `62262b94`, 24 `fc1ee77f`, 26 `68a18d66`, 27 `a91d8fe7`, 29 `efcd5242`, 37 `aeea24f2`, 38 `a37cfcff`, 40 `ee2cba8d`, 44 `9c2a7f99` | 1 `2068f868`, 2 `6fc36a1e`, 3 `3e84ac74`, 5 `9de394da`, 12 `f27174b5`, 13 `386fdb5d`, 15 `4c6e4321`, 18 `b48d1158`, 19 `0f2089f3`, 21 `62262b94`, 24 `fc1ee77f`, 25 `71b4f044`, 26 `68a18d66`, 27 `a91d8fe7`, 28 `ade52536`, 29 `efcd5242`, 30 `95a5e063`, 31 `b7147b1f`, 33 `4935a4f8`, 34 `e6b1ece7`, 37 `aeea24f2`, 38 `a37cfcff`, 40 `ee2cba8d`, 44 `9c2a7f99` | `CwDecodeReport.cs` 58 of 12, 65 of 15, 77 of 19, all against unit 392's seam; `CwPitchChoice.cs` of 15 and 19 already in the tree, dropped | no transmit file. **App halves not taken:** `MainWindowViewModel` from 2, 3, 12, 13, 19, 21, 44; `AppSettings` from 13, the joint-cutter setting. **Engine halves outside `Cw` not taken:** `Audio\AudioTap.cs` from 1; `Audio\Ft8SlotWatch.cs` and `ReusableWindow.cs` from 44, the second already at HEAD. **Tools halves not taken:** `tools\Hamlet.PitchRank` from 18, 19, 26 to 29, 31, 33, 34, 37, 38, 40; `tools\cwbench` and `cwbench.py` from 28, 29, 38; `tools\write-status.py` from 38. **Test halves not taken:** from 1, 3, 15, 18, 19, 21, 25, 28, 29, 31, 37, 38, 40, 44. `data\bands` from 21 and `Directory.Build.props` from 13, not taken. The meter half of 44 not taken. |

Every one of the fourteen sits in exactly one chain: two in S, twelve in D. None fits no chain.

**What the merge costs, for the reader of the verdict.** D carries two links already judged out
with measured harm on their own: 34 `e6b1ece7`, which alone turned 14 captures and one
adjudicated anchor red in unit 397, and 30 `95a5e063`, which alone raised unsure on 25 of 37 in
unit 396. The merge rule puts them in the same commit as ten of the fourteen that do not stand on
them - only 38 and 40 need 34, only 37 and 44 need 30. D's verdict is one verdict on all 24.

## 5. Order for task 2

Smallest first: **S, 4 pieces**, then **D, 24 pieces**.

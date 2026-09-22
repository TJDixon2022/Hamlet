# Unit 391 - the last green commit and the first red one (step 0, criteria 0.2 and 0.3)

Work instruction 391 task 3, 2026-09-22. Each candidate in a detached worktree at
`C:/Source/HamLet-wt391`, the floor tests **as that commit has them**, same filters, a status
line before each, the worktree removed after each. Raw output:
`.run-unit/unit391-floors-<hash>-<n>.txt` (not committed).

## The answer

| | Commit | Date | Message |
|---|---|---|---|
| **0.2 last green on all three** | `07f0397a` | 2026-08-21 10:23 -0400 | feat(engine): give the gate its own analysis window |
| **0.3 first red** | `8e3ee277` | 2026-08-21 11:44 -0400 | feat(engine): decode CW by likelihood instead of by threshold |

- **Between 07f0397a and HEAD, 84 commits touch `src/Hamlet.RadioEngine/Cw`** (1622 commits on main in all).
- **At 07f0397a:** `CwFixtureTests.TheCleanRecordingsDecodeExactly` 2 of 2 (`CQ DE W1AW K` exact at 12 and
  18 wpm); `TheCapturesThatDecodeKeepDecodingTests` 5 of 5 (the table then held five recordings with
  character floors only: 013347 at 8, 004507 at 25, 003016 at 38, 003126 at 34, 003758 at 14);
  `TheAdjudicatedReadingsKeepReadingTests` **did not exist** (written 2026-08-25 at `f96b21fb`), and
  counts green under the instruction's rule.
- **At 8e3ee277**, 07f0397a's direct child: captures 5 of 5, adjudicated absent, and **the clean
  synthetics 0 of 2**, the cases it turned red: `clean-12wpm` read
  `ENCCTCMQQQ T DDEDE  A WWEWRJ11E1AAAWW W T...` and `clean-18wpm` read
  `E KCTCGQ Q N DEDE E WWAJ11AARW W N K`, both against `CQ DE W1AW K`. The commit adds
  `CwProbabilisticDecoder.cs` (482 lines) and `CwProbabilisticStream.cs` (280) and changes
  `CwDecoder.cs` and `MainWindowViewModel.cs`.

## Against the instruction's expectation

The instruction expected the green commit between 2026-08-25 and 2026-08-28. **It is not there.**
The clean synthetics are red at every one of the 51 commits touching `src/Hamlet.RadioEngine/Cw`
from HEAD back to 2026-08-24, so the task 3 fallback's condition ("no commit back to 2026-08-24 is
green on all three") holds. Section 4 of the instruction says to keep walking back when the
expectation fails, and the walk went on, one commit at a time, to the first green, 85 commits
down. 07f0397a meets criterion 0.2 as written. The fallback commit (newest green on the captures
type alone) was not walked for; see the probe below and output.md section 4.

## How the walk was run

Newest first over `git log -- src/Hamlet.RadioEngine/Cw`. At each commit the cheapest type,
the clean synthetics (10 s), was run first, and a commit red on it was red on the three and the
walk moved on, so the captures and adjudicated types were only run where the synthetics were
green. That order is the author's. No run was lost.

## What the clean synthetics read, commit by commit

The synthetics go through three stages: wrong letters from 08-21 (8e3ee277), all unsure marks
from 08-25 (`07260a2a`, "put a floor under what a character has to prove before it prints"), and
**nothing at all from 09-03** (`43efc525`, "the tap is fed from the callback, the decoder from a
queue"), which is still what HEAD reads. These are indications from one fixture pair, not a
diagnosis.

| # | Commit | Date | clean | clean-12wpm read | Message |
|---|---|---|---|---|---|
| 1 | `1a84188e` | 2026-09-03 | 0 of 2 | `""` | feat(engine): the callback budget is set, not inherited, and overruns are counted |
| 2 | `9c2a7f99` | 2026-09-03 | 0 of 2 | `""` | perf(engine): a reader on a timer stops allocating the audio it reads |
| 3 | `865e66d8` | 2026-09-03 | 0 of 2 | `""` | feat(engine): no CW decode in Digital mode |
| 4 | `43efc525` | 2026-09-03 | 0 of 2 | `""` | feat(engine): the tap is fed from the callback, the decoder from a queue |
| 5 | `2828ab69` | 2026-08-31 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | docs(docs): report work instruction 056 — the streams agree, the reading is lost after |
| 6 | `ee2cba8d` | 2026-08-31 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | test(engine): the element streams agree, so the reading is lost after them |
| 7 | `a09b36a7` | 2026-08-31 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): measure whether two people are sending, and withhold the verdict |
| 8 | `a37cfcff` | 2026-08-31 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): lower the speed ceiling to thirty, and carry every element out |
| 9 | `aeea24f2` | 2026-08-30 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): admit a station by how far its bin swings, not by its average |
| 10 | `efc33267` | 2026-08-30 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | fix(engine): the sheet stops lying about arithmetic, and tonight's captures land |
| 11 | `dfb357ef` | 2026-08-30 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | test(tools): the four configurations, and a carrier count that does not work |
| 12 | `e6b1ece7` | 2026-08-30 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): a key-down that comes back inside twelve milliseconds never ended |
| 13 | `4935a4f8` | 2026-08-30 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | test(engine): measure the peak-referenced threshold and refuse it |
| 14 | `c8685e4d` | 2026-08-30 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | test(engine): the peak window buys nothing, and N4L does not come back |
| 15 | `b7147b1f` | 2026-08-29 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | test(engine): measure the percentile threshold and refuse it, three ways |
| 16 | `95a5e063` | 2026-08-29 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): assert nothing from a pitch nobody judged to be a station |
| 17 | `efcd5242` | 2026-08-29 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): find the pitch from the band rather than from a bin already chosen |
| 18 | `ade52536` | 2026-08-29 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | test(tests): the bench's run-merging bug is not in Hamlet, and here is why |
| 19 | `a91d8fe7` | 2026-08-29 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): a temperature on the path score, swept |
| 20 | `68a18d66` | 2026-08-29 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | test(engine): carry the posterior to each character and measure it |
| 21 | `71b4f044` | 2026-08-29 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): a posterior over the lattice, in the log domain |
| 22 | `fc1ee77f` | 2026-08-29 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): index the lattice by hop and kind |
| 23 | `b8cad1f9` | 2026-08-29 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | test(tools): test every confidence the decoder has against correctness |
| 24 | `0f48c33e` | 2026-08-29 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): score a decode against what was actually sent |
| 25 | `62262b94` | 2026-08-28 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): start the decoder fresh when the dial actually moves |
| 26 | `ac1d56da` | 2026-08-28 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): port the reference decoder's chain into the engine |
| 27 | `0f2089f3` | 2026-08-28 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): let the ranking supply the mixdown pitch, and leave it off |
| 28 | `b48d1158` | 2026-08-28 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): rank candidate pitches against one band-wide noise floor |
| 29 | `f9c11989` | 2026-08-27 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): fit the key-up state, measure it, and ship nothing |
| 30 | `501e8e2d` | 2026-08-27 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | chore(engine): bank the key-up analysis and delete CwPitchRanking |
| 31 | `4c6e4321` | 2026-08-27 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): let the strongest bin choose the note, and record that it did |
| 32 | `8ca6a633` | 2026-08-27 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): rank candidate pitches by what the decoder reads at each |
| 33 | `386fdb5d` | 2026-08-26 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): decide the cuts and the characters together, behind a setting |
| 34 | `f27174b5` | 2026-08-26 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): the operator may assert a station, and Hamlet finds the pitch |
| 35 | `f2e1db7a` | 2026-08-26 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | test(engine): collect the raw run stream, marks and gaps apart |
| 36 | `4786c7e7` | 2026-08-26 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | test(engine): both gate derivations measured, neither ships |
| 37 | `44cf3fc8` | 2026-08-26 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | test(engine): build both gate-threshold derivations, both off by default |
| 38 | `1bf4372d` | 2026-08-26 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | test(engine): the separation bound must not move, and the reason is upstream |
| 39 | `7fb89d5e` | 2026-08-26 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): record which admission test refused which bin, and by how much |
| 40 | `3d4694e5` | 2026-08-26 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | test(engine): record that the confirmation window stays at two, and why |
| 41 | `9de394da` | 2026-08-26 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | test(engine): open the two constants a sweep has to vary |
| 42 | `39a42c3f` | 2026-08-26 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): put the margin's share of the span on the sheet |
| 43 | `3e84ac74` | 2026-08-26 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | fix(engine): let go of a pitch measured on a frequency the radio has left |
| 44 | `6fc36a1e` | 2026-08-25 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): log how close the argument was, beside how loud it was |
| 45 | `2068f868` | 2026-08-25 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(engine): read the first seconds again, at the note they were sent on |
| 46 | `ca252057` | 2026-08-25 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | fix(engine): one decoder, whatever size the audio arrives in |
| 47 | `2f52f6db` | 2026-08-25 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | fix(engine): the witness judges on element length, guarded by the swing |
| 48 | `cf83821f` | 2026-08-25 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | fix(engine): let the keying witness look where the decoder looks |
| 49 | `7c8a5f00` | 2026-08-25 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | feat(app): put the recording's keying duty on the capture sheet |
| 50 | `07260a2a` | 2026-08-25 | 0 of 2 | `"■ ■ ■ ■ ■  ■ ■ ■ ■■"` | fix(engine): put a floor under what a character has to prove before it prints |
| 51 | `13119e6e` | 2026-08-24 | 0 of 2 | `"■ E ■ I E  S E E H■"` | fix(engine): raise the speed ceiling to forty, and say when a winner is at the edge |
| 52 | `aa5a93c7` | 2026-08-24 | 0 of 2 | `"■ E ■ I E  S E E H■"` | feat(engine): hold the last measured pitch, and withdraw a refinement measured worse |
| 53 | `13661992` | 2026-08-24 | 0 of 2 | `"■ E ■ I E  S E E H■"` | feat(engine): say when the reported pitch is a bank centre rather than a station |
| 54 | `ff6590d6` | 2026-08-24 | 0 of 2 | `"■ E ■ I E  S E E H■"` | feat(engine): report where an admitted station sits, not which bin found it |
| 55 | `b022d925` | 2026-08-24 | 0 of 2 | `"■ E ■ I E  S E E H■"` | fix(engine): keep the quarter point as the silence test, and say what it costs |
| 56 | `61b423aa` | 2026-08-24 | 0 of 2 | `"■ E ■ I E  S E E H■"` | fix(engine): read nothing from digital silence rather than noise from a floor |
| 57 | `ab95f4d9` | 2026-08-24 | 0 of 2 | `"■■■ESHHISH■■<HH>■■■■■■ESS"` | fix(engine): move the guard to 1.40, inside the same measured gap |
| 58 | `1a5d9285` | 2026-08-24 | 0 of 2 | `"■■■ESHHISH■■<HH>■■■■■■ESS"` | fix(engine): re-express the guard in the units the scale now uses |
| 59 | `b65ac6b4` | 2026-08-24 | 0 of 2 | `"■■■ESHHISH■■<HH>■■■■■■ESS"` | feat(engine): take the noise scale from the Rayleigh identity, over a rolling span |
| 60 | `c052d100` | 2026-08-24 | 0 of 2 | `"■■■ESHHISH■■<HH>■■■■■■ESS"` | docs(docs): record the premise reproduced in-tree, and what disagreed |
| 61 | `4e4b9bac` | 2026-08-24 | 0 of 2 | `"QQQ T DDEDE  A WWEWRJ11E1AAAWW W T KK"` | feat(engine): let the operator hold the decoder's pitch, and show that it is held |
| 62 | `07d8dc23` | 2026-08-24 | 0 of 2 | `"QQQ T DDEDE  A WWEWRJ11E1AAAWW W T KK"` | feat(engine): let each character answer for itself, and keep the window as a guard |
| 63 | `f93c388f` | 2026-08-24 | 0 of 2 | `"QQQ T DDEDE  A WWEWRJ11E1AAAWW W T KK"` | test(engine): carry each character's span length, and measure the margins |
| 64 | `3c4b1b16` | 2026-08-23 | 0 of 2 | `"QQQ T DDEDE  A WWEWRJ11E1AAAWW W T KK"` | feat(engine): say when somebody else is keying in the same passband |
| 65 | `fe104f02` | 2026-08-23 | 0 of 2 | `"QQQ T DDEDE  A WWEWRJ11E1AAAWW W T KK"` | test(tests): measure what each integrator width buys and what it costs |
| 66 | `7907cc0e` | 2026-08-23 | 0 of 2 | `"QQQ T DDEDE  A WWEWRJ11E1AAAWW W T KK"` | feat(engine): window the integrator, and make the two envelope paths agree |
| 67 | `2eb40706` | 2026-08-23 | 0 of 2 | `"QQQ T DDEDE  A WWEWRJ11E1AAAWW W T KK"` | test(tests): put two senders in one passband and measure what survives |
| 68 | `baf51ff0` | 2026-08-23 | 0 of 2 | `"QQQ T DDEDE  A WWEWRJ11E1AAAWW W T KK"` | fix(engine): set the refill guard where a fresh stream can see it |
| 69 | `3b7f7e13` | 2026-08-23 | 0 of 2 | `"ENCCTCMQQQ T DDEDE  A WWEWRJ11E1AAAWW W T"···` | feat(engine): score every character's own span against the key never going down |
| 70 | `f48e6b19` | 2026-08-22 | 0 of 2 | `"ENCCTCMQQQ T DDEDE  A WWEWRJ11E1AAAWW W T"···` | feat(engine): use a sender's own gap lengths once the structure has held |
| 71 | `58768058` | 2026-08-22 | 0 of 2 | `"ENCCTCMQQQ T DDEDE  A WWEWRJ11E1AAAWW W T"···` | feat(engine): take a sender's gap lengths from the gaps, and measure what that costs |
| 72 | `76b295cc` | 2026-08-22 | 0 of 2 | `"ENCCTCMQQQ T DDEDE  A WWEWRJ11E1AAAWW W T"···` | feat(engine): measure the sender's dit instead of searching for it |
| 73 | `7f2e90f4` | 2026-08-22 | 0 of 2 | `"ENCCTCMQQQ T DDEDE  A WWEWRJ11E1AAAWW W T"···` | fix(engine): score a segment's length as a ratio, not as a difference |
| 74 | `df5f7e4c` | 2026-08-21 | 0 of 2 | `"ENCCTCMQQQ T DDEDE  A WWEWRJ11E1AAAWW W T"···` | test(engine): find where characters break, and measure three fixes that do not work |
| 75 | `ceb3bd5d` | 2026-08-21 | 0 of 2 | `"ENCCTCMQQQ T DDEDE  A WWEWRJ11E1AAAWW W T"···` | test(engine): show what the survey admitted, not only what it chose |
| 76 | `62a4648a` | 2026-08-21 | 0 of 2 | `"ENCCTCMQQQ T DDEDE  A WWEWRJ11E1AAAWW W T"···` | fix(engine): turn the window clear off, and keep the machinery |
| 77 | `d0fd0b56` | 2026-08-21 | 0 of 2 | `"ENCCTCMQQQ T DDEDE  A WWEWRJ11E1AAAWW W T"···` | feat(engine): empty the window when Hamlet crosses to somebody else |
| 78 | `67ac0796` | 2026-08-21 | 0 of 2 | `"ENCCTCMQQQ T DDEDE  A WWEWRJ11E1AAAWW W T"···` | feat(engine): build the window clear, and say why it is not switched on |
| 79 | `55362633` | 2026-08-21 | 0 of 2 | `"ENCCTCMQQQ T DDEDE  A WWEWRJ11E1AAAWW W T"···` | fix(engine): let the decoder say where it is in a character |
| 80 | `7fb7f5d0` | 2026-08-21 | 0 of 2 | `"ENCCTCMQQQ T DDEDE  A WWEWRJ11E1AAAWW W T"···` | fix(engine): hold the tracker inside a character again |
| 81 | `4bc3bce8` | 2026-08-21 | 0 of 2 | `"ENCCTCMQQQ T DDEDE  A WWEWRJ11E1AAAWW W T"···` | refactor(engine): delete the old CW decoder |
| 82 | `866d2259` | 2026-08-21 | 0 of 2 | `"ENCCTCMQQQ T DDEDE  A WWEWRJ11E1AAAWW W T"···` | fix(app): point every reported number at the decoder that decodes |
| 83 | `f28657b6` | 2026-08-21 | 0 of 2 | `"ENCCTCMQQQ T DDEDE  A WWEWRJ11E1AAAWW W T"···` | feat(app): stop decoding the operator's own sending, and stop the screen moving |
| 84 | `8e3ee277` | 2026-08-21 | 0 of 2 | `"ENCCTCMQQQ T DDEDE  A WWEWRJ11E1AAAWW W T"···` | feat(engine): decode CW by likelihood instead of by threshold |
| 85 | `07f0397a` | 2026-08-21 | 2 of 2 | `CQ DE W1AW K (exact)` | feat(engine): give the gate its own analysis window |

## One probe outside the walk: 7e209cb4, where the 08-25 floors were set

The author's addition, run once so step 1 has a second restore point on numbers rather than on
the record's word. `7e209cb4` (2026-08-25 14:54, "bank the evening of 2026-08-25 and floor all
thirteen") is the commit that wrote the 08-25 floor table. Its `src/Hamlet.RadioEngine/Cw` is
`ca252057`'s.

| Type at 7e209cb4, as that commit has it | Result | Wall |
|---|---|---|
| `TheCapturesThatDecodeKeepDecodingTests` | **36 of 36 green** | 97 s |
| `TheAdjudicatedReadingsKeepReadingTests` | **13 of 13 green** | 37 s |
| `CwFixtureTests.TheCleanRecordingsDecodeExactly` | not run at 7e209cb4; **0 of 2 at `ca252057`**, the same decoder source, reading `■ ■ ■ ■ ■  ■ ■ ■ ■■` | - |

So on 2026-08-25 the two capture-based floor tests were green and the clean synthetics were
already red. Whether any commit newer than 7e209cb4 is green on the captures type was **not
measured**; 7e209cb4 is a green point, not proven the newest one. The same type took 1995 s for
37 cases at HEAD against 97 s for 36 here: an indication, from one run each, that the decoder
at HEAD is about twenty times slower on the same captures.

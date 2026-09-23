#!/bin/sh
# Unit 391: write docs/phase-cw/unit391-walk.md.
cd /c/Source/HamLet || exit 1
doc=docs/phase-cw/unit391-walk.md
{
cat <<'EOF'
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
EOF
sh .run-unit/unit391-walktable.sh
cat <<'EOF'

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
EOF
} > $doc
wc -l $doc

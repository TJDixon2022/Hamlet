READ IN THIS ORDER.

A. The baseline table, with every row's edits, scored length and key kind.
B. Step 0's criteria: 0.1 the scorer, 0.2 the baseline, 0.3 the error kinds,
   0.4 the exit round. Steps 1 to 5 not started.
C. The rest. Section 4 raises 2 items.

**A. The phase goal and the baseline.** *Hamlet reads a CQ call correctly.* The baseline,
at HEAD, **all keys inferred**:

| recording | edits | scored length | key |
|---|---|---|---|
| `cw-2026-09-23-173723` | 29 | 25 | inferred |
| `cw-2026-08-17-013347` | 0 | 6 | inferred |
| `cw-2026-08-17-134712` | 1 | 3 | inferred |
| `cw-2026-08-18-003758` | 3 | 12 | inferred |
| **total** | **33** | **46** | **inferred** |

**B. Step 0 and its exit criteria.** 0.1 the scorer: met, 12 of 12 hand-counted cases red on
a stub, then green. 0.2 the baseline: met, the table above in
`docs/phase-correctness/baseline.md`. 0.3 the error kinds: met, counted per case below.
0.4 the exit round: met, every floor row identical to entry. All four ticked in
`PHASE_PLAN.md`. Steps 1 to 5 not started.

**C. What this report adds.** Bearing on A: this is the first number in the project that
says how far the text is from a key. Bearing on B and on step 3: **at the bench, the 17:37
letters are not all right.** 15 of its 29 edits are word boundaries and 14 are letters, and
14 edits are left even when spaces cost nothing. The plan's "every letter is right" is the
sidecar's reading and not the bench's.

```
UNIT:       410 - complete at task 4 of 4, none dropped - 2026-09-23 19:17
PHASE GOAL: When Tim's radio hears a CQ call, the text Hamlet prints is the call that was sent, measured as edits from a key and not just counted as characters.
UNIT GOAL:  Build an instrument that says how far a decode is from a key, point it at every recording that has one, and let the counts say what kind of error there is most of.
ADVANCED:   yes - step 0 criteria 0.1 to 0.4 met with measured evidence; the phase has a baseline to judge every later change on
NUMBER:     baseline edits, 17:37: 29 -> 29 over 25 characters, inferred key; baseline total 33 over 46 characters, inferred keys
DRIFT:      0
```

## 1. What Claude did

**Complete at task 4 of 4, tasks 0 to 4, none dropped.** Claude Code on QUIVERFULL, project
Hamlet (gate: `SHACK_FACTS.md` and `CwProbabilisticDecoder.cs` present, `CoreHMI.sln` and
`MURC.sln` absent, root `C:\Source\HamLet`), branch `main`, HEAD at entry `7ba9690d`.
Commits `9157c3c2`, `bd875351`, `df58aa6f`, `14ce9971`, and the report commit. Every push
succeeded.

**Task 0, the record.** Version 1.13.96 to 1.13.97. `PROJECT_CARD.md` `PHASE: Hamlet reads a
CQ call correctly`, `PHASE_SET: 2026-09-23`. HM-DEC-169 at the top of `DECISIONS.md` as given,
and its row at the top of `CLAUDE.md` §1. `PHASE_STATUS.md` already named unit 410 and
`CURRENT_STEP: 0`, so it was not edited. `PHASE_OUTCOME.md` has its `## UNIT 410 - STEP 0`
entry. **Entry round** (one build, then `--no-build`, a status line before each run): engine
carry-forward 178 of 178 in 376 s. App 277 of 278 in 158 s:
`TheStopIsAlwaysOnScreenTests.AtEachOf354sNineSizesStopIsInTheStatusBarAndOnTheWindow` was
lost to the dispatcher loop before any assertion, went 1 of 1 green when re-run once alone,
and counts neither way. Captures 37 of 37 in 91 s, adjudicated 13 of 13 in 29 s, clean
synthetics 2 of 2, `TheSeventeenThirtySevenCaptureTests` 5 of 5.

**Task 1, the scorer (0.1).** `tests/Hamlet.RadioEngine.Tests/Cw/CwScorer.cs` is public and
reachable by every Cw test type. It returns `CwScore`: edits, scored length, key kind
(`Exact` or `Inferred`), the scored region, and the alignment the edits were counted from.
- `Whole` scores a region the key file names.
- `Within` handles a key that covers a fragment of a recording. It aligns the whole key to
  the stretch of the decode that fits best and scores nothing on either side.
- `FromFirst` applies the 17:37 key file's rule: from the first `CQ` to the last character
  emitted, trimmed.

Edits are Levenshtein with spaces included and nothing collapsed.
`TheScorerCountsWhatAHandCountsTests` holds 12 hand-counted cases (`KITTEN`/`SITTING` 3,
`DEWB`/`DE WB` 1, `EEMP/4 QNIKK` inside `AA4MP/4 QNIK` 3, and so on). They went **12 of 12
red** against a stub that threw, then **12 of 12 green**
(`.run-unit/unit410-scorer-red.txt`, `-scorer-green.txt`).

**Task 2, the baseline (0.2).** `TheBaselineIsScoredTests.TheBaselineIsPrinted` decodes each
WAV hop by hop at 600 Hz, the same path the floors use, and prints the table.

**The phase's baseline is 33 edits over 46 characters, against inferred keys, over 4
recordings.**

| recording | edits | scored length | key | region of the decode |
|---|---|---|---|---|
| `cw-2026-09-23-173723` | 29 | 25 | inferred | `CQ CQ CQ DE W T E E T E  E ERE D E T T TB 7E E I` |
| `cw-2026-08-17-013347` | 0 | 6 | inferred | `VA3VRR` |
| `cw-2026-08-17-134712` | 1 | 3 | inferred | `N4 ` |
| `cw-2026-08-18-003758` | 3 | 12 | inferred | `EETMP/4 QNIK` |

Its one assertion is that the new scorer and the Levenshtein already in the 17:37 test
agree on 17:37, and it holds. The nine other adjudicated recordings in the tree are scored
by the same rule and kept outside the total: 124 edits over 363 characters against
inferred keys (section 4, item 1).

**Task 3, what kind of error (0.3).** `CwScorer.Kinds` counts from the alignment: letters
wrong, missing and added, and spaces added or missing. An edit that touches a space counts
as a boundary and nothing else. `CwScorer.LettersOnly` rescores the pair with every space
removed, which depends on no tie rule.

| recording | edits | wrong | missing | added | space added | space missing | letters-only |
|---|---|---|---|---|---|---|---|
| 17:37 | 29 | 4 | 0 | 10 | 15 | 0 | 14 |
| 013347 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| 134712 | 1 | 0 | 0 | 0 | 1 | 0 | 1 |
| 003758 | 3 | 3 | 0 | 0 | 0 | 0 | 3 |
| total | 33 | 7 | 0 | 10 | 16 | 0 | 18 |

**What the counts say.** On 17:37, word boundaries are the largest single kind, 15 of 29
edits, and every one of them is a space where none was sent. They are **not a majority**:
the other 14 edits are letters, and 14 edits remain when spaces cost nothing. Where the key
has `WB6RED`, the bench reads `W T E E T E  E ERE D`: single-element letters, each followed
by a space. That looks like gaps inside characters being read as gaps between words. It is
the shape of the fault, not a trace of it. The instruction expected 17:37 to be dominated
by boundaries. It is only a plurality, and at the bench the letters are not all right.

**Task 4, the exit round (0.4).** Build clean with warnings as errors. Engine 178 of 178 in
370 s. App 277 of 278 in 160 s:
`TheCarrierHoldsTheButtonsTests.HisCardIsDrawnInTheSendingGreenWithTheWord` was lost to the
dispatcher loop before any assertion, went 1 of 1 green when re-run once alone, and counts
neither way. Captures 37 of 37, with every printed row identical to entry. Adjudicated 13
of 13, with every reading identical to entry. Clean synthetics 2 of 2.
`TheSeventeenThirtySevenCaptureTests` 5 of 5, its lines identical to entry. **`git diff
7ba9690d HEAD -- src` prints nothing.** `src/Hamlet.App` prints nothing. The transmit files
print nothing against `7e209cb4`. Nothing is red that was green at entry.

**Section 5 checks against the tree.** Mismatches are reported here and nothing was
repaired.
- The 17:37 WAV and key are on disk and tracked, committed in `65b4f9d0`. **As stated.**
- `TheSeventeenThirtySevenCaptureTests` is 5 of 5. It replays the WAV hop by hop through
  `CwDecoder.Process` at 600 Hz, then calls `Flush`. **As stated.**
- **Mismatch.** `TheAdjudicatedReadingsKeepReadingTests` does not hold 13 cases over three
  recordings. It holds **12 theory rows over 12 recordings plus one fact**, which makes 13.
  `134712`'s row is retired and asserts nothing. The key material is each row's
  `Adjudicated` field. The asserted `Anchor` is a substring floor that the file itself
  calls "never an answer key". The file gives rulings as provenance and never says exact
  or transcribed, so those keys are marked inferred.
- The floors count named characters with placeholders separate. **As stated.** Captures
  run 37 of 37 in 91 to 92 s here, not 97 s.
- `Judged` (line 1247 of `CwProbabilisticDecoder.cs`) keeps a character only at
  `SpanMargin >= CharacterMargin`, which is 1.0 at line 302. **As stated.**
- **Mismatch.** §4's `CQ CQ CQ DEW B 6 RE D W B` is the sidecar's reading. The bench reads
  the text shown above. PHASE_PLAN.md's "46 named characters" counts the whole settled text
  (`named 46`), not the scored region: the region is 48 decoded characters against a
  25-character key.

**Decisions made for itself, author's, overrulable.**
1. *Scored length is the key's length over the region, spaces included.* The decode's
   region length is printed beside it. Reason: the key is what is being read, and the
   decode's length moves with every added character.
2. *An adjudicated text is scored with `Within`.* Reason: none of these texts names a
   region, and scoring the whole decode against a callsign would score audio nobody keyed
   (R61).
3. *The tie rule.* Among alignments with the fewest edits, the scorer takes the one with
   the fewest letter edits. After that it prefers a character in place, then missing, then
   added. The first rule written had only that second part. It split the hand case
   `DEW B 6 RE D` against `DE WB6RED` into a letter wrong, a letter added and three spaces,
   where a hand counts five spaces. It was replaced **before any recording was broken
   down**, and the baseline's edits did not move. This rule leans toward counting
   boundaries: `134712`'s missing `L` counts as a space added. That is why letters-only is
   printed beside it.
4. *The nine other adjudicated recordings are scored and kept out of the total* (section 4).
5. The per-type timeouts were 300 s for each Cw type and 480 s for the carry-forward lines.

## 2. What Tim should expect

Nothing about the app has changed. No file under `src` was touched, and the radio, the
screen and every decoder behave as they did this afternoon. This unit added a way of
measuring. **The number says how far the decoder's text is from a key: 29 single-character
edits separate what the bench reads on the 17:37 recording from the 25-character key, and
across the four keyed recordings it is 33 edits over 46 characters.** The number does not
say what was sent. The 17:37 key, `CQ CQ CQ DE WB6RED WB6RED`, is inferred from the shape of
a CQ call and from the decode itself. Nobody transcribed it off the air, so every figure
against it is an indication. The first third of that recording is never scored. What will
look wrong but is not: the bench's text for 17:37 is worse than what the CW tab showed on
the day. The bench replays the WAV through the test path, and every number in this phase
is measured that way so that numbers can be compared.

## 3. What you should see

**The answer to what this unit was commissioned to ask:** the phase's baseline is **33 edits
over 46 characters against inferred keys**. 17:37 is **29 edits over 25 characters against
an inferred key**. On 17:37, word boundaries are the largest kind of error, 15 of the 29,
but not a majority, and 14 edits are letters.

On the screen: nothing new, by design. In the tree: `docs/phase-correctness/baseline.md`
has the tables and the 17:37 alignment written out. `TheBaselineIsScoredTests` reprints
them on demand in about 40 s:

    timeout 300 dotnet test tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj --filter "FullyQualifiedName~.TheBaselineIsScoredTests."

## 4. What's blocking us

Nothing blocks. Both items block no criterion and are parked in
`docs/phase-correctness/PARKED.md` under R54, for overrule.

**1. Should the nine other adjudicated recordings join the baseline total?**
Ruling taken: the baseline is the four recordings the instruction names, and the nine are
printed beside it (124 edits over 363 characters against inferred keys).
Reasoning: 0.2 says "every recording in the tree that has a key", and the tree has twelve
adjudicated texts, not three. Step 3's 3.2 judges each change on "the total edit count
over all keyed recordings", so which recordings count decides what every repair is judged
on. Adding rows after the baseline is set would move the yardstick.
Rejected: silently scoring only three, which would hide nine keys the tree holds; and
folding all thirteen into the total, which changes the instruction's baseline without a
ruling.

**2. Is step 3 still framed right?**
Ruling taken: the plan is unchanged, and the finding goes to step 3's 3.1 trace.
Reasoning: step 3 is written as "letters right, word boundaries wrong", but that is the
sidecar's reading. At the bench, which is what the phase measures, 17:37's letters are not
all right: 14 of 29 edits are letters, and 14 remain letters-only. What the counts show
looks like gaps inside characters read as gaps between words, which is a spacing fault at
the element level, so step 3 may well stand as written.
Rejected: rewording the step here, which is Tim's plan and not this unit's.

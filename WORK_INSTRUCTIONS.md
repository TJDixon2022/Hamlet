# Work instruction 595 - Validity is words: the first use of the owner's ruling

**Hand run. One setting - the gap learning's validity judge replaced by a word judge - up to
three rounds, one commit per round plus the record.** The tag `before-gap-learning` stands.

**Where it stands** (1.13.281, `CwRules.GapLearning` in the tree, off, the mark fit removed):
with the rule on, the wild bank **3091** from 2859 and spaces added 263 from 400; the first
recording whole; the fist bank 4268 from 3898; the fading half 1993 from 1771; `mobile` 529,
`steady` 570, `tight` 499, `farnsworth` 565, `machine` 584, `bug` 344 against 345; Bryan 122
from 118; Ken 43 both ways; strays halved; the random carrier 6 seeds of 20 from 20. **And**
the twelve-plus-three **291** from 308, W1AW 2098, the easy fixtures **74 of 90**, the working
fixtures 5 from 19. **The costs have one cause:** validity is judged by the share of `E`, `T`
and `■` in a whole 12 s window. That refuses honest text - `tightfist-easy` sends `TEST DE
TEST`, seven of eleven letters `E` or `T`, and prints nothing - and refuses a station who began
mid-window, since half his window is noise laid as `E` and `T` (`221548`'s second station,
18 letters, held off the terminal from 18 to 30 s). The test does a necessary job: it keeps a
lane 50 Hz off `bug` from printing `TETE TTET` over `CQ CQ` (taking it out dropped him from 278
to 221). Letter counts are a poor judge of whether text is text.

**The owner's ruling HM-DEC-296: Hamlet may know words.** This unit is its first use, at the
lowest risk: **words decide validity, and nothing else.** The path, the gap fit, the medians,
the cold ranges and the start set are as built; the mark fit stays out; `bug`'s swing in the
marks is a later unit.

**Standing rules** (HM-DEC-276, HM-DEC-284) apply, with up to three rounds. Engine line 1500 s,
survey 2400 s, `--no-build` after one build per round, build servers shut down first, one test
run at a time; a run stopped for memory is run once more alone without asking.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted. The draft truth `cw-2026-10-19-141708.truth.md` is not edited.**

**Numbering.** This is unit 595, ruling HM-DEC-299. If taken, use the next free and say so.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      docs\cw-scoreboard.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwSequenceReader.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all five are not as stated, refuse: reply with only the path you are in,
which checks failed, and "wrong project - nothing done."

If all five hold, say "Hamlet confirmed" and continue.
```

---

## 1. Rules

- Take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end. Write nothing
  to `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`. Tick nothing in `PHASE_PLAN.md`.
- One `dotnet test` invocation per line, filtered, with a `timeout`.
- Apostrophes in quoted heredocs break; `;` is refused; Python cannot run here; `-m` more than
  once for a multi-line commit. Scripts go in `.run-unit\unit595-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves.
- The change is the validity judge inside the sequence reader's gap learning, and the vocabulary
  it reads. The path, the fit, the mark finders, the lane bank, the column finder, the seam's
  rules, the keyer and the wild bank are not touched. **Words decide nothing in the path**: no
  letter is changed, added or removed because of a word.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.

---

## 2. The vocabulary

A new file `src\Hamlet.RadioEngine\Cw\CwWords.cs` (or a resource beside it, embedded, one word
a line, upper case), **`CwVocabulary`**, holding:

- the Q-codes used on CW (`QTH QRL QRM QRN QSB QSL QSO QRZ QRS QRQ QRP QRT QRV QRX QSY QSK QSP QTR`,
  with or without `?`) and the prosigns the reader prints (`<BT> <AR> <SK> <KN> <AS> <KA>`);
- the abbreviations of the air: `CQ DE K KN AR SK BK TU TNX TKS FER FB UR URS RST RPRT ES HR HW
  CPY CPI AGN PSE PLS OM YL XYL NR NAME WX TEMP ANT RIG PWR WATTS GA GM GE GN GB CUL CUAGN
  73 88 DX TEST VY NW BTU SRI SORRY OK R RR WID WX HPE SN SOON HV HVE CONDX ABT ABOUT MNI
  MANY ANT DIPOLE VERT YAGI KEY BUG STN MOBILE QRP WPM` and the states' two-letter codes;
- **shapes**, matched by pattern, not list: a callsign (one or two letters, a digit, one to
  three letters, with an optional `/` suffix such as `/M /P /QRP` or a digit); an RST (`5` then
  two digits, or `5NN`-style with `N` for `9`, or `T` for `0`); a number of one to four digits; a
  US ZIP or a year is just a number;
- **common English**, the most frequent few thousand words (the table says; at least 3,000,
  from a frequency list the author can cite; a word of one or two letters only from the list of
  function words `A I AN IN ON AT TO OF IS IT BE BY DO GO HE ME MY NO OR SO UP US WE AM AS IF`
  and the abbreviations above). Given names in common use on the air, at least the few hundred
  most frequent American first names.

A word is **recognised** if it is in the list or matches a shape. `TheCwVocabularyTests`:
`TEST DE TEST` is all words; `GM KEN NICE TO MEET U ES TNX FER RPRT UR RST 579 579` is all but
at most two (`U` is on the list as an abbreviation); `TETE TTET ETETTE TTTTTE` has none;
`N3AFS KB2UZK/M W1AW` are all callsigns; `5NN 599 73` all shapes.

## 3. The change: validity is words

The validity figure the gap learning uses - at the choice of a cold start, at a warm lane's
re-read, and at the terminal - becomes **the recognised share**: of a lane's **last
`ValidityLetters` 12 printed letters** (fewer only when fewer have been printed; under
`ValidityLeast` 4 letters a lane is neither valid nor invalid and keeps whatever standing it
had), the share that fall inside recognised words, counted by letter, where the words are what
the lane's own spaces delimit, and a word cut by the window's edge is not counted either way.
A lane is **valid** when the share is at least `ValidWords` 0.5. The `E`, `T` and `■` share
goes; the near-lines share stays where it stands as built (say where).

- **At a cold start**: among the starts whose decode is valid, the likeliest; if none is, the
  most recognised.
- **At a warm lane**: re-read cold when the share falls under `ValidWords` for `InvalidPasses`
  2 passes running, not on one.
- **At the terminal**: a lane whose share is under `ValidWords` does not take the terminal;
  a station who has just begun (fewer than `ValidityLeast` letters printed) is judged as
  before this rule - the shipped reader's own terminal rule - so he is not held off.

**The trace, first, on the three losses and the one keep:** `tightfist-easy` (what the
shipped judge said of `TEST DE TEST` and what the word judge says); `221548`'s second station at
18 to 30 s (the lane's letters per second, the old share, the new share, the second the terminal
goes to him); `bug` at 20 WPM, 20 dB, 6 to 20 s (the off-pitch lane's `TETE TTET`, old and new
share, and that it still does not take the terminal); and `221828`, 8 letters lost to
`FitNearMost` last unit - its gaps near the lines, and whether the fit is kept or refused there,
since that loss is not the judge's and this unit should say so if it stays.

## 4. Rounds

Round one: the vocabulary, the change and the four traces, measured once (the survey, the
boards, the hard limits, the guards, CPU on W1AW piece-04). Rounds two and three: trace the
largest loss on the old boards against the shipped reader to the recording, to the second, to
the path; fix the one thing the trace names inside the judge or the fit's keep rule; measure
again. Twice at most. **No change to the path, the marks, or the vocabulary's shapes in rounds
two and three** - a word may be added to the list if a trace names it.

## 5. The bar, and the ship

**`GapLearning` ships on by default only if, at the end of the last round:**

- **the old boards hold:** the twelve-plus-three **308** or better, wrong not above 13,
  invented not above 4, spaces not worse than 92 of 110; **W1AW 2101** or better, wrong not above
  24; the fist bank **3,898** or better and its guard; the fading half and the strong flat hands
  as guarded; **the easy fixtures 90 of 90**;
- **the wild bank holds its gains:** score **3,050 or better**, spaces added 270 or fewer,
  invented not above 5, `bug` 340 or better, no recipe worse than the last unit's figure by
  more than 10;
- **the hard limits:** noise prints nothing, the steady carrier at none, no old capture above
  six strays per 10 s; the summed strays reported; the random-keyed carrier reported both ways;
  the 5 dB CQs 84 of 84; nothing in silences beyond the one guarded; **the first recording
  whole**;
- the working and edge fixtures reported; `two-station` 27 of 27; CPU under real time with
  margin, the vocabulary's cost said;
- **Ken and Bryan reported** per 20 s, not a condition.

**For the owner's ruling, not a fail:** everything holds but W1AW between 2080 and 2100, or
everything holds but the twelve-plus-three between 305 and 307, or everything holds but wrong on
one board up by one or two: put the full figures under *For the owner's ruling*; leave the rule
off; do not ship.

If it fails otherwise: say which, by how much, where, by path; leave the rule off; keep the tag
and the traces; say in one paragraph what the boards still lose and whether it is the judge,
the fit's keep rule, or a third thing.

If it ships: the first recording's guard is restated to the whole read; both scoreboard guards
at the figures that ship; the wild bank joins the guards (`TheWildBankTests.TheBankHolds`:
total, `bug`, spaces added) at the figures that ship less 5%; the fist bank's guard rows move
up to the figures that ship less 5%; `TheCwVocabularyTests` and `TheGapLearningTests` go on the
carry-forward list; `CW_SPEC.md` §2 says how the reader learns a hand's gaps and that words
judge validity, a paragraph each; the `Weak stations` row is unchanged.

---

## 6. The lines, the survey, the record

Run the app line, the engine line and the survey line once at the end; rewrite
`docs\cw-survey-2026-10-09.md` with a new `Rerun` line; every group before and after.

- `PHASE_OUTCOME.md`, both copies: `## UNIT 595 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 595.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-299**, headline *Validity is words*, citing HM-DEC-296
  as the ruling used, naming the vocabulary, what the four traces found, whether it shipped or
  waits on a ruling, and the figures before and after: the old boards, the easy fixtures, the
  wild bank, the two real hands, the first recording, the random carrier.
- `docs\cw-scoreboard.md`: a 595 row in every table at the figures that shipped.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-299.

---

## 7. Report

Section 2, for the owner, in plain words, leading with **the whole picture against the shipped
reader, row by row** - the old boards, the easy fixtures, the wild bank per recipe, Bryan and
Ken per 20 s with what they print, the first recording, the random carrier - whether it ships,
and the *For the owner's ruling* heading if it applies; then what the four traces found, a
sentence each. Section 1: per round. **Section 3: the four traces; the wild bank matrix; the
two real hands' stretches; the rounds' table; the survey's groups before and after; CPU and
the vocabulary's size.** Section 4: anything left, a line each.

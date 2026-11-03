# Work instruction 592 - The wild yardstick: hands as they are sent

**Hand run. No change to the reader. A new measure, two truths, a baseline, and the record.** One
commit for the work plus one for the record.

**Where it stands** (1.13.278, the sequence reader on by default): the twelve-plus-three 308,
W1AW 2101, the fist bank 3,898, the 5 dB CQs 84 of 84 - and on the air, hands the owner copies
by ear print as E's and T's. The owner stepped back on 2026-11-03 and the analysis (project
file `ANALYSIS-2026-11-03-wild-cw.md`, summarised here) found:

- **Marks are solved.** On every wild capture in hand (the Ken/Bryan capture, `144020`, `121414`,
  `200157`), 0.0% of dits sit above the dit/dah midpoint and 0.0% of dahs below it; contrast
  13.5 to 17.8 dB. The wild captures are not noisier than W1AW in any way that matters.
- **W1AW reads because it is a machine**: gaps 35 / 100 / 240 ms = 1 : 2.9 : 6.9 dits with a
  10-90% spread of 5 ms. One gap in 889 lies near any class boundary.
- **Wild hands lose on gaps.** Measured inside : letter : word ratios - Bryan KB2UZK/M
  1.6 : 4.4 : 13.1; Ken N3AFS 1.7 : 4.3 : 7.6; `144020` 0.9 : 5.3 : 21.3; `121414` 1.0 : 3.7 : 8.6;
  `200157` 1.1 : 4.0 : 11.8. No two hands alike, none 1 : 3 : 7. Three mechanisms: (a) the
  reader's cold inside-gap range (0.4-1.7 dits) is wrong for a 1.6-dit hand and the learning
  locks the first wrong pass in - Ken reads `A UE IE DE AAI D 5T AT ET` as learned and
  `I NEW BENSING TON ,P T PA NR PITTS` with the one inside/letter boundary moved from 140 to
  200 ms; (b) true ambiguity - Bryan spaces `579` like words and `I AM` like a letter, and no
  boundary from 110 to 240 ms reads both; ~13% of his gaps are in no-man's-land; (c) swing in
  marks, a minority (Ken's dit log-spread 0.37 against W1AW's 0.09).
- **The yardstick measured the wrong thing.** The fist bank's keyer sends 1 : 3 : 7 with
  symmetric jitter and varies only mark properties. It holds no 1.6-dit hand, no digits spaced
  like words, no letter gaps spread 2.5 to 5 dits. A reader scores 3,898 on it and reads Ken as
  `ETETTE TTTTTE`.

**The owner's rulings, 2026-11-03:** *Hamlet may know words* ("sure"); the yardstick is changed
before the reader is; no further units on marks, lanes, seams, handover or the opening as a mark
problem. This unit is the yardstick. The reader is not touched.

**Standing rules** (HM-DEC-276, HM-DEC-284) apply. Engine line 1500 s, survey 1800 s,
`--no-build` after one build, build servers shut down first, one test run at a time; a run
stopped for memory is run once more alone without asking.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio. No recording is added, removed or
promoted.** The Ken/Bryan capture `cw-2026-10-19-141708` is already in the tree.

**Numbering.** This is unit 592, ruling HM-DEC-296. If taken, use the next free and say so.

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
  once for a multi-line commit. Scripts go in `.run-unit\unit592-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves.
- **No file under `src\Hamlet.RadioEngine\Cw\` changes.** If a measurement needs a hook the
  reader does not have, report it under section 4 and measure without it.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.

---

## 2. The wild bank - hands with gaps as they are sent

The keyer that makes the fist bank's hands (the generator the survey's bank group uses) gains a
**gap recipe**: inside, letter and word gaps each drawn per gap from a log-normal around the
recipe's median with the recipe's spread (the spread is the standard deviation of the log), and
two **habits**: *digits spaced like words* (a gap between two digits is drawn from the word
class) and *short words run together* (a word gap after a one- or two-letter word is drawn from
the letter class). Marks also take a per-hand spread: dits and dahs log-normal around the median
with the recipe's spread. The current hands are the recipe `machine` (1 : 3 : 7, the existing
jitter) and are unchanged, so the existing bank's figures do not move.

The recipes, from the measured hands (gaps in the hand's own dits; spread = log std):

| Recipe | dit ms | dah/dit | dit spread | dah spread | inside (spread) | letter (spread) | word (spread) | habits |
|---|---|---|---|---|---|---|---|---|
| `mobile` (Bryan) | 65 | 3.4 | 0.29 | 0.12 | 1.6 (0.10) | 4.4 (0.30) | 13.7 (0.40) | digits as words; short words together |
| `bug` (Ken) | 80 | 2.9 | 0.37 | 0.19 | 1.7 (0.23) | 4.3 (0.18) | 7.6 (0.29) | none |
| `farnsworth` (144020) | 75 | 2.9 | 0.28 | 0.05 | 0.9 (0.05) | 5.3 (0.35) | 21 (0.45) | none |
| `tight` (121414) | 58 | 2.5 | 0.25 | 0.16 | 1.0 (0.08) | 3.6 (0.25) | 8.5 (0.40) | none |
| `steady` (200157) | 75 | 2.8 | 0.10 | 0.01 | 1.1 (0.08) | 4.0 (0.30) | 11.8 (0.25) | none |
| `machine` (control) | as the bank | 3.0 | as the bank | as the bank | 1 | 3 | 7 | none |

Each recipe at **two speeds** (its own dit, and its dit scaled to 20 WPM), **two levels** (20 dB
and 10 dB, flat), **two texts** - a QSO text in the air's own words (`GM KEN NICE TO MEET U ES
TNX FER RPRT UR RST 579 579 I AM IN MOBILE STN HR NAME BRYAN BRYAN ES QTH IS CHICOPEE, MA`) and
the bank's existing text - seeded by recipe as the bank is: **48 hands**, scored exactly as the
bank is scored (letters right, wrong, invented, spaces). A new survey group **`wild bank`**,
reported as the bank is: total, per recipe, per level, invented. A guard test
(`TheWildBankTests.TheBankIsMade`) that the 48 hands are generated and the truth is the text
sent; **no score guard yet** - the figures are the baseline.

Also, per hand, the **ambiguity figure**: the share of the sent gaps within ±25% of the midpoint
between the recipe's inside and letter medians, and between letter and word, in the survey
doc beside the score. It says how much of each hand is readable by timing at all.

## 3. Two real hands with truth

On `tests\fixtures\cw\captured\cw-2026-10-19-141708.wav`, 500 Hz: **Ken N3AFS from 0 to
120 s** and **Bryan KB2UZK/M from 120 to 300 s**. The truth below is a **draft** - the best
reads of the prototype, corrected by what the QSO must have said; `?` marks what is unknown.
Put it in the tree beside the capture as `cw-2026-10-19-141708.truth.md`, headed `DRAFT - the
owner corrects by ear`, with the two stretches as shown. The owner will correct it; until then
these two stretches are **reported, not guarded** (HM-DEC-284: no rule ships against a draft).

```
Ken N3AFS, 0-120 s, 500 Hz, straight key or bug, about 15 WPM
? ? <BT> ? UR RST 589 589 ? QTH IS NEW KENSINGTON, PA NR PITTSBURGH ?
NAME IS KEN KEN ? HW? KB2UZK/M DE N3AFS K
(at about 100 s Bryan begins: N3AFS DE KB2UZK/M)

Bryan KB2UZK/M, 120-300 s, 500 Hz, mobile, about 18 WPM
HELLO GM KEN NICE TO MEET U ES TNX FER RPRT UR RST 579 579 ? I AM IN MOBILE STN HR ?
NAME BRYAN BRYAN ES QTH IS CHICOPEE, MA CHICOPEE, MA PARKED IN MY MOBILE <BT> SO BTU
KEN N3AFS DE KB2UZK/M K
(after about 280 s nothing of Bryan's; what follows is not scored)
```

Score both stretches with the reader as shipped, the way the twelve-plus-three are scored
(letters right, wrong, invented, spaces; alignment against the truth with `?` matching
anything), and **per 20 s stretch** so the owner can see where it fails. A survey group
**`wild hands`** with these two rows. On the capture sheet nothing changes.

## 4. The baseline trace

For each of the two real hands and for the `mobile` and `bug` recipes at 20 dB, own speed, a
table in section 3 of the report: the hand's **sent** (or measured) gap medians - inside,
letter, word - against the reader's **learned** gap classes at 10 s, 30 s and the end of the
stretch; its learned dit and dah against the hand's; the share of the decode that is `E`, `T`
or `■`; and the text it printed. One table, these four rows. This is what the next unit (the
gap learning) is measured against.

## 5. The bar

This unit does not ship a reader change, so the bar is that the measure exists and nothing else
moved:

- the wild bank: 48 hands generated, truth is the text sent, the group in the survey with its
  figures and the ambiguity figures; the `machine` recipe reproduces the existing bank's hands
  bit for bit (say how that was checked);
- the two real hands: the draft truth in the tree, the scores per 20 s stretch in the survey;
- **the existing boards unchanged**: the twelve-plus-three 308, W1AW 2101, the fist bank 3,898,
  every guard green, the engine line green, the app line green;
- CPU of the survey reported (the two new groups add to it; say by how much).

If the `machine` recipe does not reproduce the existing bank, stop, keep the recipe code, leave
the survey's bank group on the old keyer, and say what differs.

---

## 6. The lines, the survey, the record

Run the app line, the engine line and the survey line once at the end; rewrite
`docs\cw-survey-2026-10-09.md` with a new `Rerun` line; every group before and after, the two
new groups added.

- `PHASE_OUTCOME.md`, both copies: `## UNIT 592 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 592.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-296**, headline *Hamlet may know words; the wild
  yardstick*, quoting the owner's rulings of 2026-11-03 as given above, the four findings of the
  analysis in a line each, what this unit built, and the baseline figures of the two new groups.
- `docs\cw-scoreboard.md`: a new table **Wild hands** (the two real stretches, right / wrong /
  invented / spaces, per unit) and a new table **Wild bank** (total, per recipe), each with its
  592 row as the baseline; the existing tables untouched.
- `CW_SPEC.md`: a new short section after §2, *what a hand sends*, with the recipe table and the
  three mechanisms, so the spec says why W1AW reads and a hand does not.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-296.

---

## 7. Report

Section 2, for the owner, in plain words: **the wild bank's baseline per recipe** (which hands
the shipped reader reads and which it does not, in a line each), **the two real hands** per 20 s
stretch, the ambiguity figures, and that the existing boards did not move. Section 1: what was
built. **Section 3: the baseline trace table of §4; the wild bank matrix (recipe × level ×
speed); the two real hands' stretches; the survey's groups before and after; CPU.** Section 4:
anything left, a line each - including whether the `machine` recipe reproduced the bank, and
which of the truth's `?` the reader itself can fill.

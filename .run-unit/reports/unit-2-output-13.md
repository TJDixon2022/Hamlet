READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 3 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 0 of 5, 8 0 of 6, 9 0 of 8 (blocked on fldigi's source).
B. Step 6, criterion 6.2: HM-REQ-071 not met (met on 5 of the 9 prosigns of
   `CW_SPEC.md` 6.2; VE, CT, BK and CL are not in the decoder's table), named by
   `TheProsignArrivesAsOneSymbolTests`; HM-REQ-072 met after task 3's kept change,
   named by `TheTwoNamedPatternIsNamedAsTheTerminalIsSetTests`; 6.2 ticked in both
   copies of `PHASE_PLAN.md`. 6.1 open (its table is where 071's four misses live),
   6.4 open, 6.6 red on the same three named floors.
C. This report adds the prosign trace, the two tests and the naming setting.
   Section 4 raises 3 items. None is in the way of a criterion in B. Item 1 is the
   CW reading this unit worked under. Items 2 and 3 are findings for 6.1, and 6.1
   is already open.

UNIT:       455 - complete at task 4 of 4, none dropped - 2026-09-26 14:09
PHASE GOAL: Hamlet's CW decoder meets the requirements in CW_REQUIREMENTS.md, each shown by a test that names it, ending with Tim reading real CW off the air.
UNIT GOAL:  A prosign sent as one unbroken run reaches the screen as one symbol, called by the name the operator chose, with a test for each of HM-REQ-071 and 072 saying whether that holds.
ADVANCED:   yes - 6.2 ticked: both requirements have a test naming them and a stated verdict, and 072 went from not met to met with every recording's text byte-identical
NUMBER:     HM-REQ-071 not met - met on 5 of 9 prosigns; HM-REQ-072 not met -> met; MET-CER-SURE real 33 of 436, synthetic 14 of 173 (unchanged)
DRIFT:      step 2 0; step 3 0; step 4 1; step 5 1; step 6 0 (was 1); step 9 1

## 1. What Claude did

**Complete: all four tasks were done, tasks 0 to 4, and nothing was dropped.** Task 3 ran only for
HM-REQ-072. No prosign run is split, so HM-REQ-071 had nothing that task 3's "if a run is split"
change could act on.

**Provenance.** Claude Code on the Windows development machine (QUIVERFULL). The prompt claimed
Hamlet, and the gate confirmed it: `SHACK_FACTS.md`, `CwProbabilisticDecoder.cs`,
`CW_REQUIREMENTS.md` and `CW_SPEC.md` are present, `CoreHMI.sln` and `MURC.sln` are absent, and the
root is `C:\Source\HamLet`. Branch `main`. Every commit was pushed to `origin/main`. Nothing in this
report is evidence about the radio.

**Commits:**

| task | commit | what |
|---|---|---|
| 0 | `54e74c8f` | entry: record edits, version 1.13.141 to 1.13.142, the runner's writes, entry round, text saved |
| 1 | `a2a9f596` | the prosign trace; nothing in `src` |
| 2 | `9469589f` | the two tests, each watched failing first; 6.2 ticked |
| 3 | `078a8303` | HM-REQ-072's naming setting, kept under R78 |
| 4 | this commit | exit round, report, status |

**Verified against the tree (§3). Where the tree differs from the instruction, the tree is the fact:**
- **`MorseAlphabet.cs` holds five prosigns:** `<AR>` `.-.-.`, `<SK>` `...-.-`, `<BT>` `-...-`,
  `<KN>` `-.--.` and `<AS>` `.-...`, plus `<HH>` for the error signal (073's, not touched).
  - **KN is in the table. BK and CL are not, and neither are VE and CT.**
  - At HEAD the name was fixed in code. Nothing the operator could set named `=`/`BT` or `+`/`AR`,
    as the instruction expected.
- **The fixture.** `prosigns-18wpm` (`CwFixtures.cs:99`) sends `W1AW DE K2ABC ^BT R TU ^SK`, so it
  carries only BT and SK. The instruction points at `CwProbabilisticDecoder.cs:1137`; that line
  only names the fixture in a comment. A prosign reaches the transcript at
  `CwProbabilisticDecoder.cs:1717` and `1737`, as `MorseAlphabet.Lookup(spelled) ?? "#"` on the
  whole run between character gaps.
- **Existing prosign tests. Neither names a requirement, and I did not re-point either (R80):**
  - `CwFixtureTests.TheProsignRecordingDecodesItsProsigns` asserts `<BT>` and `<SK>` on the fixture.
  - `TheProsignsFixtureAtABandTests` prints and asserts nothing.
  - **Mismatch:** `CwReceiverFixtureTests.cs:168` refers to a test named
    `TheProsignsArriveAsProsigns`, and no test of that name exists in the tree.
- **Expected failures, as stated:** the three named floors are red at the stated values. The app
  carry-forward line lost nothing on either run this unit.

**Task 0: the entry round, at HEAD `905ba7de` plus the record edits:**
- The build had 0 errors, in 17 s.
- Engine line 178 of 178, in 389 s. App line 278 of 278 on the first run, in 174 s.
- Captures 51 of 51. Adjudicated 13 of 13. Named floors 10 of 13, with the three red values as
  stated.
- The four metrics are in section 3's table below, and they equal 454's exit.
- 126 text lines were saved to `.run-unit/unit455-text-before.txt`, identical to 453's save.

**Task 1: the trace.** It lives as two facts in the 071 test class that assert nothing.
- **How each prosign was sent:**
  - Its pattern is written literally in the test from M.1677-1 (and ARRL for KN, BK and CL).
  - The generator's keying of `^XX` is checked against that literal, and all nine matched. Under
    §12.5, the key does not come from `MorseAlphabet`.
  - Each prosign was sent in `prosigns-18wpm`'s own shape, `W1AW DE K2ABC ^XX R TU ^XX`, at 18 wpm
    on the same noise band.
- **The results are the table in section 3.**
- **The real keys** carry two prosigns, both `<BT>`: in `cw-2026-08-18-004507` (`NET<BT>EAC`) and in
  `unadjudicated/cw-2026-09-24-004234` (`COM<BT>THA`). Both were emitted `<BT>`, sure.

**Task 2: the two tests. Both were watched failing first:**
- **`TheProsignArrivesAsOneSymbolTests` names HM-REQ-071.**
  - First run, on a deliberately wrong expectation (the bare letters): 10 of 10 red.
  - Corrected: 6 of 10 green. SK, AS, KN, BT and AR read as one symbol at both places, and A then R
    with a real gap reads as two letters.
  - Red: VE, CT, BK and CL. Each reads `■` mid-message and nothing at the end.
- **`TheTwoNamedPatternIsNamedAsTheTerminalIsSetTests` names HM-REQ-072.**
  - The one-symbol-per-send case was watched failing on a wrong count (17 for 16), then corrected
    to green.
  - At HEAD, the check for a naming setting was red: "HM-REQ-072 not met: no terminal setting names
    -...- and .-.-.".
- Both red tests are on no carry-forward line. 6.2 is ticked in both copies of `PHASE_PLAN.md`.

**Task 3: the change for HM-REQ-072, kept.**
- **What was built:**
  - `CwProsignNaming` (`Prosign`, `Punctuation`) and `MorseAlphabet.Name` in the engine.
  - `AppSettings.CwProsignNaming`, default `Prosign`.
  - `CwTranscript.Render`, used where the terminal draws each settled character
    (`CwTerminalControl.cs`) and in the tip.
  - A switch on the settings screen: "Show BT and AR in the CW terminal as = and +".
  - The decoder is untouched.
- **Judged under R78:**
  - All four metrics are identical before and after, real and synthetic.
  - The three floors are at their entry values. No capture changed, so V-11 holds.
  - Every recording's text is byte-identical to task 0's save.
- **Its test is 7 of 7** (one symbol per send, both names both ways, and the default).
- **Four failures in the app types the change touches** are also there at HEAD with the change
  stashed. None is on a carry-forward line:
  - `VoiceTests.NoOperatorFacingStringUsesABritishSpelling` (`centre` at `MainWindowViewModel.cs`
    12744 and 12752 at HEAD);
  - `HowMuchTheApplicationSaysTests` x2 (the CW tab at 554 against a ceiling of 550; not a surface
    this unit changed);
  - `SettingsCarriesTheTransmitDriveTests.TheNoteSaysItIsAStartingPointToBeSetAgainstTheRadiosAlc`.
- The settings window's text goes from 1626 to 1677 characters, against its ceiling of 1750.

**Decisions I made for myself:**
1. **The 071 test covers all nine prosigns of `CW_SPEC.md` 6.2, not only the five in the decoder's
   table.** The instruction says "each prosign in the table", and its §2 says the requirements win.
   HM-REQ-071's verification row lists AR, SK, BT, KN, BK, CL and AS. So four cases are committed
   red, and they stay red until 6.1 generates the table.
2. **At HEAD, 072's red was the setting's absence, found by reflection.** A typed test could not
   compile against a type that did not exist. Task 3 replaced it with the typed both-ways theory.
   Both runs are recorded: `.run-unit/unit455-req072-head2.txt`, then `unit455-req072-change2.txt`.
3. **The naming is applied on the screen only**: the terminal's settled text and the tip.
   - The transcript's own text keeps the decoder's symbol, and so do the callsign resolver, the case
     sheet and the collapsed panel's one-line summary (`Transcript.Tail`).
   - So under the punctuation setting, that 28-character summary still shows `<BT>`.
4. **I put the setting on the settings screen as a switch, not only in the settings file.** The
   requirement says "per the terminal's setting", and the operator can reach a switch without
   editing a file.
5. **The transmit files are unit 435's eleven.** That list is what "the transmit files" has meant
   in every exit round since. A wider grep of `src` also catches the FT8 transmit files, which have
   changed since `7e209cb4` in earlier units. None changed in this unit.
6. **To check whether the four app failures predate the change, I stashed the change, built HEAD,
   ran them, and restored it** (`.run-unit/unit455-athead.sh`).
7. **I did not enter section 4's two findings in `OPEN_ISSUES.md`.** §12.6 names that file, but its
   entries carry `HM-OPEN` ids, and assigning one is a record decision I left to the owner. Both
   findings are in section 4.

Nothing was recorded under `CLAUDE.md` §12.1. Nothing in this unit keys or transmits.

**Instruction check.** The prompt and the work instruction both carry the status cadence, and the
work instruction states the task count (4).

## 2. What the owner should expect

**What the operator sees:**
- When a station signs off with AR or SK sent as one run, the terminal shows `<AR>` or `<SK>` as one
  symbol, never `EN`, `RK` or loose letters. The same holds when a station sends BT between
  paragraphs (`<BT>`), and for KN and AS.
- On the settings screen there is now a switch, "Show BT and AR in the CW terminal as = and +". It
  is off by default, so the screen reads exactly as it did yesterday. Turned on, the next `<BT>`
  shows as `=` and the next `<AR>` as `+`. Characters already on screen stay as they were drawn.
- VE, CT, BK and CL still come out as the filled block `■` mid-over, because the decoder's table
  does not know them yet. Sent as the very last thing in an over, one of those four currently shows
  nothing at all.

**What will look wrong but is not:**
- `TheProsignArrivesAsOneSymbolTests` is red on 4 of its 12 cases (VE, CT, BK, CL). That is
  HM-REQ-071 measured and not met, committed red on purpose.
- The three named floors are red at the stated values: 17:37 38 of 46, `032113` 43 of 45, `032129`
  42 of 64.
- Four app tests outside the carry-forward line are red at HEAD without this change (voice spelling
  `centre`, the CW tab's text ceiling at 554 of 550, and the drive note).

**The build and tests at exit:** 0 errors. Engine line 178 of 178; app line 278 of 278 on the first
run. Pushed to `origin/main`.

## 3. What you should see

**The prosign table: every prosign, its pattern, and what the operator reads.** The two reads are
from a synthetic send of each prosign, taken mid-message and then at the end.

| prosign | pattern | in the decoder's table | at HEAD | after task 3, default | after task 3, switch on | one symbol? |
|---|---|---|---|---|---|---|
| AR | `.-.-.` | yes | `<AR>`, `<AR>` | `<AR>` | `+` | yes |
| SK | `...-.-` | yes | `<SK>`, `<SK>` | `<SK>` | `<SK>` | yes |
| BT | `-...-` | yes | `<BT>`, `<BT>` | `<BT>` | `=` | yes |
| KN | `-.--.` | yes | `<KN>`, `<KN>` | `<KN>` | `<KN>` | yes |
| AS | `.-...` | yes | `<AS>`, `<AS>` | `<AS>` | `<AS>` | yes |
| VE | `...-.` | no | `■`, nothing | same | same | not the prosign |
| CT | `-.-.-` | no | `■`, nothing | same | same | not the prosign |
| BK | `-...-.-` | no | `■`, nothing | same | same | not the prosign |
| CL | `-.-..-..` | no | `■`, nothing | same | same | not the prosign |
| A then R, gapped | `.-` `.-.` | - | `AR` as two letters | same | same | two, as sent |

- **HM-REQ-071: not met.** It is met on 5 of 9. None of the 9 is split into letters. The 4 misses
  are prosigns the table lacks.
- **HM-REQ-072: met**, where it was not met at HEAD.
- `prosigns-18wpm` reads `W1AW DE K2ABC <BT> R TU <SK>`. The two `<BT>`s in the real keys read
  `<BT>`.

**What changes for the operator:** they can now choose to see `=` and `+` instead of BT and AR.
Nothing else they read has changed.

**Entry and exit, side by side:**

| check | entry | exit |
|---|---|---|
| build | 0 errors | 0 errors |
| engine carry-forward | 178 of 178, 389 s | 178 of 178, 383 s |
| app carry-forward | 278 of 278, 174 s | 278 of 278, 160 s |
| captures | 51 of 51 | 51 of 51 |
| adjudicated | 13 of 13 | 13 of 13 |
| named floors | 10 of 13 (38, 43, 42) | 10 of 13 (38, 43, 42) |
| MET-CER-SURE | real, inferred: 33 of 436; synthetic, exact: 14 of 173 | same |
| MET-INVENTED | real 33 over 473; synthetic 14 over 252 | same |
| coverage (R82) | real 403 over 473; synthetic 159 over 252 | same |
| MET-WBE | real 37 (29 ins, 8 del) over 113; synthetic 44 (13, 31) over 84 | same |
| `TheProsignArrivesAsOneSymbolTests` | - | 8 of 12 (VE, CT, BK, CL red) |
| `TheTwoNamedPatternIsNamedAsTheTerminalIsSetTests` | 2 of 3 at HEAD | 7 of 7 |
| app types touched (14) | 117 of 121 with the change; the same 4 red at HEAD | 117 of 121 |
| `git diff 7e209cb4` over the eleven transmit files | - | prints nothing |
| every recording's text against task 0 | - | byte-identical, 126 lines |

## 4. What's blocking us

Nothing blocks the next step. Three items, the first a reading and the other two findings, all for
the record:

1. **The CW reading under R85.** HM-REQ-071 is read against `CW_SPEC.md` 6.2's nine prosigns and its
   verification row, not against the decoder's table. A prosign the table lacks is "not met", and
   its fix is 6.1's generated table (HM-REQ-070). No constant was added by hand.
2. **Finding for 6.1: KN's pattern `-.--.` is also M.1677-1's open bracket `(`.**
   - That makes it a two-named pattern that `CW_SPEC.md` 6.2 does not list as one. 6.2 names only
     `=`/`BT` and `+`/`AR`.
   - Read under R85, 6.2 wins: KN is named KN, and the switch does not rename it.
   - When 6.1 generates the table from the vendored M.1677-1 file, `(` and `)` will need the same
     ruling 6.2 gave `=` and `+`.
3. **Finding for 6.1: a run the table lacks is dropped entirely when it is the last thing sent.**
   - VE, CT, BK and CL each read `■` mid-message but emit nothing at all at the end of a send.
   - That is "something was heard and nothing is shown", which is §0.0's concern rather than 071's.
   - I named it here and did not repair it (§12.6).

**Asks still outstanding**
- From unit 454, 2026-09-26: **Ruling proposed: the owner places fldigi's source under
  `C:\Source\HamLet\.run-unit\fldigi\`.** The simplest way is to run
  `git clone https://github.com/w1hkj/fldigi.git C:\Source\HamLet\.run-unit\fldigi` from any shell,
  or to allow that one command in the session's permissions. It waits on the owner, and it blocks
  all of step 9. No change sits in the tree for it. `.run-unit\fldigi\` is still absent.
- From unit 453, 2026-09-26: whether 6.5 stands ticked on three spans measured and three not
  measurable here. It waits on the owner's ruling. The tick is in both copies of `PHASE_PLAN.md`,
  and the measurement is in `docs/phase-requirements/metrics.md`. R85 may settle it as a CW
  question read from the documents. No ruling is on file.
- From unit 453, 2026-09-26: whether HM-REQ-084 keeps `ABOVE` on 013637, which no decoder working
  from the audio alone can print as one word under R72. It waits on the owner's ruling. The
  requirement stands unedited in `CW_REQUIREMENTS.md` §I. No ruling is on file.

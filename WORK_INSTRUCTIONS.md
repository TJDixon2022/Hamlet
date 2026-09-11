# Work instruction 316 - read the conversation

**Authored by the arbiter from `PHASE_PLAN.md` and `PHASE_OUTCOME.md`.** Steps 0, 1 and 2
are `done` (units 312, 314, 315; step 2's state was returned by the separate session that
read unit 315's report). This unit aims at **step 3**.

---

## 0. The project gate

```
SHACK_FACTS.md                                          must exist
src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs     must exist
CoreHMI.sln                                             must not exist
MURC.sln                                                must not exist
root                                                    C:\Source\HamLet
```

**If any of the four is wrong, stop and say so in `output.md` section 4. Write nothing
else.** The refusal text: *This is not Hamlet. Nothing was changed.*

---

## 1. The two rules that killed sessions

Both HM-DEC-155, Tim, 2026-09-05.

**1. A unit runs no test suite.** Only this unit's own test names, filtered by exact
name, foregrounded, with a stated timeout:

```
timeout 480 dotnet test <project> --filter "FullyQualifiedName~TypeName.MethodName"
```

Plus `docs\carry-forward-tests.txt`, the same way. **Known reds are never on it** -
section 10 lists them. An unfiltered run finds them and tells you nothing.

**2. Never background a command and poll it.** The watchdog fires at **twelve minutes
with no status write.** A parser over 32 lines runs in milliseconds; one that takes
longer is a finding about the code.

---

## 2. The tool facts

**The shell breaks on an apostrophe inside a quoted heredoc and collapses a doubled
backslash.** Write *do not*; write single backslashes; check what landed on disk.

**Unit 315 measured the shell refusing `rm`, `git rm`, `mkdir`, output redirects and
`tools\arbiter\validate-output.bat`** with "This command requires approval". If you meet
the same, report it once in section 4 and do not retry the command in other forms - units
289 to 292 and 315 already measured that as refused.

---

## 3. Asks still outstanding

Carried per HM-DEC-139. **All of these come back in section 4, verbatim where
unresolved.** Items 17 to 24 are unit 315's section 4, logged by the arbiter and **not
this unit's to act on** - none of them is in the way of a step 3 criterion.

1. **Does the transmission record ask the radio whether it keyed?** Unit 303's proposal,
   still Tim's. PSK31's continuous carrier makes it sharper.
2. **Nothing in this repository can look at a picture.** `Avalonia.Headless.Skia` is
   Tim's to add (§0.4).
3. **Three inherited reds, never chased** - two in `TheAchievementsScreenTests`, one in
   `TheFitGuardAsksAboutTheGridTheSendIsOnTests` (engine).
4. **`Ft8ContactCard.Closing`** is uncalled and left standing.
5. **`Ft8GlobePlot`'s unused framing constants.** Report; leave standing.
6. **The licence of `assets/world-flat-relief.png` is unknown.** Raise; do not resolve.
7. **The FT4 phase's run files.** Plan and status recovered into `docs\phase-ft4-run\` by
   unit 315; the outcome file is an uncommitted placeholder waiting on item 19.
8. **The door sentence is a placeholder.** Carry.
9. **Acknowledgement indicators.** Named by Tim, not yet defined. Step 4's turn indicator
   may be what he meant; do not assume it is.
10. **Card ordering under scroll, and its root** - `DigitalCards.Clear()` then new cards
    every slot, per-card state lost. Not this phase's. Carry.
11. **The 2 px map outline** and **the 2.0x popup zoom cap** are sessions' numbers.
12. **Version numbering** - x.y.0 or x.y.1 for a phase's first unit under HM-DEC-150.
13. **The squelch threshold 0.90** is unit 314's number, `Psk31Demodulator.SquelchQuality`.
14. **Real off-air PSK31 audio** - only Tim can record it. Two or three minutes on 14.070
    with a few signals on it, any recorder, any rate, WAV, into
    `assets\fixtures\captured\`. Every number in this phase is synthetic until then.
15. **One conversation card is taller than the panel** (293 px in 220 px). Not broken.
16. *(Unit 315 closed the fixed 1000 Hz listening offset by its instruction's own
    condition. Kept here as a closed number so the list does not renumber.)*
17. **Hand wanted: run `tools\arbiter\validate-output.bat output.md`** - unit 315's
    shell refused it.
18. **Ruling wanted: record the 1500 Hz station's text in `manifest.json`, or accept unit
    315's derived reference.**
19. **Hand wanted:** `git show eb28430:PHASE_OUTCOME.md > docs\phase-ft4-run\PHASE_OUTCOME.md`,
    expected 168242 bytes, md5 `b60010069f7feea96e9d4be4e7e8c0f1`, then commit.
20. **Hand wanted: `git rm src\Hamlet.RadioEngine\Psk31\Psk31Listening.cs`** - comment-only,
    builds.
21. **Ruling wanted: the carrier search's floor should be local to the signals before
    step 6** - at 48 kHz the median bin is below the radio's passband.
22. **Ruling wanted: is HM-DEC-161 the right id** for the phase-setting ruling?
23. **Ruling wanted: accept about a second of delay on PSK31 text, or reopen the step-1
    squelch.**
24. **Ruling wanted: the `snr` column's hover names FT8 and FT4 precision and not PSK31's.**

---

## 4. Why this unit exists

**The number today: 0 of 32 corpus lines are parsed, because no parser exists.** Every
PSK31 row is `IsTextOnly`, so `DigitalDecodeRow.Fields` is null for it
(`DigitalDecodeRow.cs:482-483`), and with it `Addressee`, `Sender`, the CQ filter
(`MainWindowViewModel.cs:1668-1669`), the operator's own side (`:1650-1652`), the
worked-fade and the country in the hover. **The CQ filter shows 0 PSK31 rows whatever
they say.** That is unit 315's deliberate guard, and this unit is the step it was
waiting for.

```
PHASE GOAL: Hamlet works PSK31 the way it works FT8 - the same two cards, the same
            one-click exchange, the same log and the same achievements, on a modem
            Hamlet builds itself.
UNIT GOAL:  Step 3 - read the conversation. Each PSK31 row's free text becomes who is
            speaking, to whom, what kind of line it is, whether it hands the turn
            over, and whether Hamlet is sure - with an explicit unknown - so the CQ
            filter, the operator's side, the fade, the country and the quill work on
            PSK31 rows as they do on FT8 rows.
ADVANCES:   Step 3, tasks 2, 3 and 4. The exit criteria moved are the corpus
            per-line conclusions, the unknown rate, own-callsign recognition, the
            CQ filter, reuse with no change, guessed-versus-certain in the view
            model, and name and QTH never parsed; plus the no-report nice-to-pass.
            Task 5 is the drop candidate and moves no must-pass.
DRIFT:      0 carried from unit 315.
```

**A, in the arbiter's words.** A third digital mode that is Hamlet's own - the same two
cards, the same one click, the same log and achievements as FT8 - on a modem this
project writes. Steps 0 to 2 built the ear: every PSK31 station in the passband is a row
of text.

**B, in the arbiter's words.** Step 3 turns those rows from text into stations. A parser
reads free text for callsigns, `de`, `CQ`, RST, a grid, the turnover words and the
closers, and says **unknown** where the text does not support a state. It is proved
against `assets\fixtures\psk31-transcripts\corpus.json` line by line, never claims
certainty the corpus withholds, and states its unknown rate. Then the rows it reads feed
the code FT8 rows already use - the CQ filter, the operator's side, worked-fade,
`DxccPrefixes.EntityOf` with its `CQ` guard, and the nudge - **without changing that
code**. Nothing transmits and nothing new is clickable.

---

## 5. Verify this instruction against the tree

**Nothing here describes the tree.** Every path, member and count below comes from unit
315's report or from the arbiter reading a handful of lines. **Check every claim; report
every mismatch in section 1, even where the work succeeded anyway; do not repair this
instruction; do not stop over a mismatch** unless a task is impossible, in which case say
which and why.

- `DigitalDecodeRow` (`src\Hamlet.App\ViewModels\DigitalDecodeRow.cs`): `IsTextOnly`,
  `Fields => IsTextOnly ? null : Ft8Vocabulary.Split(Message)`, `Addressee`, `Sender`,
  `AddresseeHelp`, `RowOpacity => HasWorkedBefore ? 0.55 : 1.0`.
- `MainWindowViewModel`: `IsForHim` asking `Ft8MessageSplit.IsAddressedTo(row.Message,
  _settings.Operator.Callsign)` behind the `IsTextOnly` gate; `WantsRow` asking
  `DecodedFilterRule.Wants(ShowsCqOnly, row.Addressee)`; the PSK31 row built with
  `IsTextOnly: true` near `:2067`.
- `DxccPrefixes.EntityOf` (`src\Hamlet.RadioEngine\Explore\DxccPrefixes.cs:104`) and its
  guard refusing `Ft8CallToAnyone`, `CQDX` and `QRZ`.
- The nudge: `NudgeSet` (`src\Hamlet.RadioEngine\Contacts\NudgeSet.cs`), `NudgeWords`,
  `AchievementQuill` (`src\Hamlet.App\Controls\AchievementMarkControl.cs`).
- `assets\fixtures\psk31-transcripts\corpus.json` and its `README.md`: eight
  transcripts, 32 lines, operator `KC3QIS`, nine kinds.
- `docs\carry-forward-tests.txt`: 22 app names, 8 engine names.
- Version `1.13.3` in `Directory.Build.props`.

**Two mismatches the arbiter already found. Report them; do not resolve them.**

1. **The corpus disagrees with itself on the garbled transcript.** `README.md` says
   `unknown_rate_expected` is *the fraction of lines a correct parser is expected to mark
   uncertain*, 0.2 on `05-garbled`. But `05-garbled` has **two** of five lines at
   `certain: false` (lines 3 and 4), which is 0.4. The 0.2 matches only the one line
   whose speaker is `UNKNOWN`. **Do not edit the corpus.** Report both rates - the
   fraction of lines not certain, and the fraction with no speaker - beside the 0.2, and
   say which reading your number meets. The must-pass is *not lower than the corpus
   expects*; a parser that marks both lines uncertain meets it under either reading.
2. **The reload's `RULES_AT` check** reports `CLAUDE.md` section 1 holding `CPS-DEC-0161`
   while `PROJECT_STATUS.md` says `HM-DEC-161`. The arbiter's grep finds no `CPS-DEC-0161`
   in `CLAUDE.md`, and `HM-DEC-161` at line 360. It looks like the reload's reading, not
   the tree. Report what you find; do not touch `tools\`.

**Expected at the start, and not yours:** `PHASE_OUTCOME.md`, `PHASE_STATUS.md`,
`RUN_LEDGER.md` and `.run-unit\*` modified by the launcher; `docs\phase-ft4-run\PHASE_OUTCOME.md`
untracked (ask 19). **Expected failures:** none inherited among the names this unit
runs. Every new test is watched failing first.

---

## 6. Rulings in force

**Do not re-argue any of these.**

**`PHASE_PLAN.md` at the root - read it in full.** The parts this unit stands on,
transcribed:

**§1 - FT8's contact is a protocol. PSK31's is a conversation.** *PSK31 is BPSK at 31.25
baud carrying free text, keyboard to keyboard. No slots, no grammar, no acknowledgement
primitive.* "Exactly like FT8" is achievable at the product layer and is achieved by
Hamlet *sending a fixed macro exchange and parsing what comes back to infer the state.
Some of the time it cannot tell. The honest display for that is a state called
**unknown**.*

**§3.4 - Text arrives one character at a time.** *A "message" for the parser is what
arrived between two turnovers, not what arrived in a slot.*

**§3.5 - There is no fixed message length.** *Garbage from noise looks like text. Step 1
must state its squelch rule and step 3 must never parse a state out of a line the squelch
did not pass.*

**§R1 - How much Hamlet asserts about an exchange it can only partly read.** *Strict on
anything that drives a transmission; permissive on what the card displays; every guessed
state marked as a guess. A macro is offered for one click only when the parser is certain
whose turn it is. The card may show a state inferred from turnover words, elapsed silence
and whose callsign appeared last, and when it does the state word is visibly a guess
(§0.6: not by colour alone).* **Rejected:** *strict everywhere, because the card would say
unknown through most real QSOs and read as broken; permissive everywhere, because a wrong
guess that sends a macro into another operator's turn is a transmission Tim did not
choose.*

**§R3 - What the parser recognises, and what it will not.** *Callsigns (the same rule the
decoded list uses today), `de`, `CQ`, RST in `5NN` or `599` form, a grid square, the
turnover words `K`, `KN`, `BTU`, `OVER`, and the closers `73`, `SK`, `CL`. It also
recognises its own callsign, which is how it knows a line is addressed to Tim. Anything
else is text and is shown as text.* **Rejected:** *parsing name and QTH, because they are
for reading, not for state, and a wrong parse of them changes nothing Hamlet does.*

**§R9 - Squelch and the honest character.** *A character the demodulator was not sure of
is not shown - no `?`, no dimmed maybe. Silence.*

**§2, the FT8 surfaces reused unchanged:** the decoded-text list with the worked-fade at
0.55 opacity (unit 279); entity resolution through `DxccPrefixes.EntityOf` with the `CQ`
guard (unit 310, because `CQ` is a real Portuguese prefix and Hamlet had labelled the
operator's own general call a station in Portugal); and **the achievement nudge under the
four rulings of 2026-09-10**: lift plus the quill vane in decode green `#3B6D11`; sticky
per station with a cap of two; the achievement set is the source and no rarity ordering
is invented; a door is marked and never named.

**§6 Branching, the calls already made:** a missed ceiling is `partial`, never a
loosened or deleted assertion. **The parser cannot tell - the answer is `unknown`, and a
high unknown rate is a number to report, not a reason to guess.** Never loosen §R1's
strict side. A package is `MOVE: stop`. Anything touching the transmit chain, the keying
path or `Played` is `MOVE: stop`. A step-0, step-1 or step-2 defect found now is fixed in
passing as the first task and said so. The card-rebuild root is worked around, not
chased. Missing off-air audio is raised, not stopped for.

**§0.0 / HM-DEC-092 - never present a guess as a decode, and a display asserts as a
sentence does.** *"A waterfall asserts things and is harder to catch than a sentence: it
draws what was measured, an empty band renders empty, and an axis is a claim."* Here: a
coloured sender, a country in a hover, a row on the operator's side and a row in the CQ
filter are each a claim about what the text said.

**§0.1** - the engine is never told that tabs exist. The parser takes text and an
operator callsign and yields a parse; it does not know it is behind a tab.

**§0.2** - one click, one transmission. **Nothing in this unit transmits or adds a click
that could.** The PSK31 tab stays inert for sending (unit 314, `ThePsk31TabIsInertTests`).

**§0.4** - a package is Tim's. **No parsing, regex or NLP package.**

**§2.1** - nothing personal in telemetry. A parsed callsign is not a telemetry field.

**HM-DEC-054** - the band row is cited data. **HM-DEC-155** - section 1.
**HM-DEC-139** - the asks queue.

**Tim, 2026-09-06 - the dummy load is withdrawn in full.** No compensating control.

**FACT-004** - a dev-machine result is an indication, never a finding. **FACT-006** -
this machine has no radio. **The corpus is written, not recorded, and every audio
fixture is synthetic.**

---

## 7. Status cadence

`PROJECT_STATUS.md` per `CLAUDE.md` §13: **after every task, and at least every ten
minutes.** Write the status **before** starting a test run.

---

## 8. The tasks

Five. Each names the test to watch failing first and its drop candidate. **The unit's
drop candidate is task 5, whole.**

### Task 1 - entry, and the trace. No production changes except the version.

**1a. Entry criteria, run rather than read.** `ThePsk31HearsEveryoneTests`, filtered to
the two-signal case by exact name: two rows, one per carrier. **If it is not so, step 3
has no ground; report it and stop after this task.**

**1b. The trace.** Say what you find rather than confirming this list. With file and
line:
- **Every reader of a row's meaning.** Everything that reads `Fields`, `Addressee`,
  `Sender` or `Payload` - the CQ filter, `IsForHim`, `HasWorkedBefore` and
  `RowOpacity`, the sender hover and its `EntityOf` call, the nudge's input - and for
  each, whether it would run unchanged if a PSK31 row supplied those members from a
  PSK31 parse. **If the path already exists and is merely unwired, say so; that is the
  fix being smaller than the build.**
- **How a general call is spelled** where the CQ filter and `EntityOf` expect it
  (`Ft8CallToAnyone`, `CQDX`, `CQ ` prefixes), and so what a PSK31 `ANY` and `DX`
  addressee must be handed over as.
- **Where the operator's callsign comes from** (`_settings.Operator.Callsign`) and
  whether the engine can be given it without learning about settings.
- **What one PSK31 row holds.** Unit 315's four-signal sample shows the 1100 Hz row
  carrying *both* sides of a QSO on one frequency. Say how the row's text accumulates and
  where it is replaced in place.
- **Whether any click on a PSK31 row reaches a send path** today.
- **The `Ft8MessageSplit` hazard** unit 315 found - any three-word text read as an FT8
  message. Say every place it could still reach PSK31 text.

**1c.** Run `docs\carry-forward-tests.txt` filtered, both projects, before anything
changes. Report the counts. **Patch-bump the version, 1.13.3 -> 1.13.4.**

**Test watched failing first:** none.
**Drop candidate:** none. **Not droppable.**

---

### Task 2 - the parser, against the corpus

An engine type (§0.1), named by the unit. **Input:** one message's text and the
operator's callsign. **Output:** speaker, addressee (a callsign, `ANY`, `DX` or unknown),
kind (one of the corpus's nine), whether it hands the turn over, **whether the parser is
certain**, RST (or unknown), grid (or none). **The vocabulary is §R3's and nothing
more.** The rules it follows are the corpus's `rules` array; state each in the type's
remarks once, with where the corpus says it.

**How it decides certainty is the unit's to describe**, not this instruction's to
dictate. What it must be: **a damaged token makes the fields it touches unknown, never a
nearest guess** - `5#9` is not `599`, `K3A~C` is not `K3ABC`; and a clean closing
`<CALL> de <CALL> K` may still name the speaker while the line stays uncertain, as
`05-garbled` line 3 expects.

**Test watched failing first:** `ThePsk31ExchangeParserTests`, engine. Hash the corpus
file and print its SHA-256 in the report; there is no manifest for it, so the hash is
recorded, not checked. Watch it fail, then green:

1. every one of the 32 lines yields the `frm`, `to`, `kind` and `turnover` the corpus
   states - *must-pass*
2. **no line the corpus marks `certain: false` comes back certain**, and `05-garbled`
   line 3's RST is unknown, not `599` - *must-pass*
3. RST and grid where the corpus states them: `5nn` is `599`, `/P` is kept, lowercase
   reads as upper - *must-pass*
4. the unknown rate per transcript is computed and printed beside
   `unknown_rate_expected`, both readings of section 5's mismatch; on `05-garbled` it is
   **not lower** - *must-pass*
5. addressed-to-operator is recognised from the corpus's `operator` field, never a
   `KC3QIS` constant; `04-not-for-me` yields no line addressed to the operator -
   *must-pass*
6. the parse result has no member for name or QTH, asserted on the type's shape -
   *must-pass*
7. `03-no-report` lands on `end` for lines 3 and 4 with no report kind invented -
   *nice-to-pass*

**Drop candidate:** assertion 7. If it goes, say so and leave the step's must-pass
untouched.

---

### Task 3 - a message out of a growing row

§3.4: *a message for the parser is what arrived between two turnovers.* An engine type
that takes characters as they arrive on one channel and yields complete messages at a
turnover or closer, each handed to task 2's parser. **State the split rule in one
sentence, with what it does about a turnover word that is also ordinary text** (`K` in
`OK`, `SK` mid-line).

**Test watched failing first:** `ThePsk31MessageSplitTests`, engine. For each
transcript, **join its lines into one stream, as one frequency would carry them**, feed
it one character at a time, and parse what comes out. Watch it fail, then green:

1. on the seven transcripts other than `05-garbled`, the messages come back in the
   corpus's order and each parses as task 2's assertion 1 expects - *must-pass*
2. **on `05-garbled`, nothing comes back more certain than the least certain corpus line
   it contains.** Line 4 has no turnover and may run into line 5; if it does, the merged
   message may not be certain. Report exactly what came back - *must-pass*
3. text with no turnover yet yields no message, and the row still shows its text -
   *must-pass*

**Drop candidate:** none. It is how task 4 gets messages out of a live row. If
assertion 1 is red on some transcripts, **report which lines and mark the step
`partial`**; do not loosen it.

---

### Task 4 - rows become stations, through code that does not change

Each PSK31 row carries the parse of its **latest complete message**, and the members
every reader in task 1b named are supplied from it. **Unit 315's guard stays true to its
reason:** no PSK31 text is ever handed to `Ft8MessageSplit` or `Ft8Vocabulary.Split`.
Whether `IsTextOnly` is replaced, narrowed or kept is the unit's to decide and to report.

**Guessed and certain are distinguishable on the view model**, as a member a test reads
and a word a reader sees, **not by colour alone** (§R1). Which word is the unit's; say
what it is and that it is a session's wording.

**`IsForHim`, `WantsRow` and the row's own members may change.** **`DxccPrefixes.EntityOf`,
`NudgeSet`, `NudgeWords`, `AchievementQuill`, and the bodies of `HasWorkedBefore` and
`RowOpacity` may not.** That is the must-pass *with no change to their code*, and it is
measured: report `git diff <task 1 commit>..HEAD` over those files and members, which must
be empty.

**Test watched failing first:** `ThePsk31ReadsTheConversationTests`, app. Rows are built
from corpus lines through task 3's splitter; the operator's callsign is set in settings,
not in the code under test. Watch it fail, then green:

1. **the CQ filter selects exactly the rows whose latest message parses as a CQ** -
   `01` to `08` CQ lines in, including `CQ DX`; answers, reports, ends and chat out -
   *must-pass*
2. a row whose latest message is addressed to the operator's callsign is on his side and
   not in the left list; `04-not-for-me`'s rows are never on his side - *must-pass*
3. a row whose parse is uncertain exposes that as a member and a word; `05-garbled`
   line 3 is not shown as certain - *must-pass*
4. a row with no speaker has no sender, no addressee, no country and is not in the CQ
   filter - *must-pass*
5. **a Costa Rican station calling CQ** - a `TI` prefix, e.g. `CQ CQ CQ de TI2ABC TI2ABC K`
   - gets the same entity, fade and quill as an FT8 row from the same call under the same
   achievement state - *must-pass*
6. worked-fade dims a PSK31 row from a station worked before, to the same 0.55 -
   *must-pass*
7. the unchanged-code diff is empty, printed - *must-pass*
8. **no click on a PSK31 row reaches a send path**; `ThePsk31TabIsInertTests`,
   `ThePsk31HearsEveryoneTests`, `TheCqListNudgeTests`, `VoiceTests` and
   `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint` still green - *must-pass*

**Drop candidate:** none in this task. It carries four of the step's must-pass. If a
reader cannot run unchanged, **do not change it to make the test pass**: report which one,
what it would need, and mark the step `partial`.

---

### Task 5 - end to end, from audio

**Drop candidate: this whole task.** It moves no must-pass; it is the proof that tasks 2
to 4 hold on text that arrived through the demodulator rather than a JSON file.

`assets\fixtures\psk31-four-signals.wav`, hash checked against `manifest-step2.json`,
through the real tap and tick as unit 315's task 4 ran it. **Report a table:** each row's
offset, its latest message, the parse (speaker, addressee, kind, certain), and whether
the CQ filter and the operator's side took it. Assert only what the manifest's own
texts support: the CQ filter holds exactly the rows whose latest message, in the
manifest's text, is a CQ.

**Test watched failing first:** extend `ThePsk31ReadsTheConversationTests` by one.

**Drop candidate:** the whole task.

---

## 9. Parked - do not touch, do not raise

- **Step 4** - the modulator, macros, the CQ receipt and conversation cards under PSK31,
  the turn indicator, drive and power. **The transmit chain is not touched.** The parse
  this unit builds will feed the turn indicator; building the indicator is step 4's.
- **Step 5** - RST in the log, achievements. A parsed RST is not logged here.
- **Asks 17 to 24** - unit 315's section 4. Carry them verbatim; act on none. Item 23's
  one-second text delay bears on step 4's nice-to-pass, not on step 3.
- **The carrier search and the demodulator.** Step 2 is done. A defect found in them is
  fixed in passing only if it stops a task here, and said so (§6).
- **The card-rebuild root.** Work around it for PSK31 rows as units 314 and 315 did.
- **Acknowledgement indicators.** Build nothing.
- **Reading anything from fldigi.** The parser is not in the reference's scope (§R5).
- **`Avalonia.Headless.Skia` or any other package.**

---

## 10. What not to do

- **No unfiltered `dotnet test`.** **Never background and poll.**
- **Do not change `EntityOf`, the nudge, the quill or the fade to make a PSK31 row
  pass.** The criterion is that they run unchanged. A change there is the test lying
  about the thing it measures.
- **Do not let a guess read as a fact.** A field the text does not support is unknown; a
  near-miss callsign is not corrected to the nearest one (§R1, §0.0).
- **Do not hand PSK31 text to `Ft8MessageSplit` or `Ft8Vocabulary.Split`.** They read
  any three words as an FT8 message (unit 315).
- **Do not parse name or QTH** (§R3).
- **Do not add a click that transmits, and do not touch anything that keys the
  transmitter.** The chain `cq_pressed -> ... -> Played` stays as it is.
- **Do not edit the corpus, its README, or any fixture or manifest.** Mismatches are
  reported.
- **Do not edit `PHASE_PLAN.md`, `PHASE_STATUS.md` or `PHASE_OUTCOME.md`** - the launcher
  and the arbiter own them - except a line the launcher's prompt itself assigns to the
  session, which you name in section 1.
- **Do not hard-code `KC3QIS`** in production code.
- **Do not invent a ruling id.** **Do not claim an appearance from computation and call it
  seen.**
- **Do not chase these known reds:** `CwAdjudicationTests.ASpeedChangeInRealisticAudio`;
  the 51 CW cases in `docs\unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests`
  whole-type-list tripwire; `HM-OPEN-088`'s ten; the two in
  `TheAchievementsScreenTests`; the one in `TheFitGuardAsksAboutTheGridTheSendIsOnTests`.
- **Do not repair this instruction.** Report mismatches; keep working.

---

## 11. Committing and pushing

Commit per task and push, on `main`. The version bump goes in task 1. Add this unit's new
test classes to `docs\carry-forward-tests.txt` in the task that makes them green. The
report and final status go in one more commit, pushed before the session stops. Nothing
of yours left uncommitted; say what was left and whose it is in section 1. A refused push
is reported as refused, with the reason.

---

## 12. Reporting

`output.md` at the repository root. **Canonical headings:** `## 1. What Claude did`,
`## 2. What the owner should expect`, `## 3. What you should see`,
`## 4. What's blocking us`. **Every exit writes it** - complete, blocked, failed or stopped
early.

**The ordering block first - `validate-output.bat` refuses a report without it:**

```
A. The phase goal - Hamlet works PSK31 the way it works FT8. Steps 0, 1 and 2 done,
   3 <state after this unit>, 4-6 not started.
B. Step 3 and its exit criteria - each of the seven must-pass and the one nice-to-pass,
   met or not met, with the number: lines matching the corpus of 32; lines asserted
   certain that the corpus marks uncertain (must be 0); unknown rate per transcript
   beside expected, both readings on 05-garbled; own-callsign recognised; CQ filter
   exact; the unchanged-code diff empty or not; guessed-vs-certain exposed; name and QTH
   unparsed; 03-no-report ends on 73.
C. The report last, and section 4 raises N new items on top of a carried queue of
   twenty-four; <whether any is in the way of a criterion in B>.
```

Then the header:

```
UNIT:       316 - <complete|stopped> at task N of 5, <which dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no, and which step>
NUMBER:     <corpus lines parsed as the corpus states, 0 -> n of 32; PSK31 rows the
            CQ filter can select, 0 -> ...>
DRIFT:      <n> consecutive units without advance  (was 0)
```

**Section 1 must carry a table: every transcript, lines matched of its count, lines
wrongly asserted certain, unknown rate against expected.** Then the split rule and the
certainty rule, each in one sentence, and the unchanged-code diff, printed.

**Section 3 must lead with what Tim will see when he presses PSK31 now:** the CQ filter
showing the stations calling CQ; a station calling him on his side; a sender picked out
with its country where certain; a worked station faded; a quill on a CQ that opens a
door; and a guessed row saying so in a word. **Then what he must not expect:** no answering
a row, no sending, no turn indicator, no RST in the log - those are steps 4 and 5. **Every
appearance claim is computed, not seen, and the corpus is written, not recorded. Say each
once.**

**Section 4 carries asks 1 to 24 verbatim where unresolved**, then this unit's own.

Write `output.md`, then stop. Do not start the next unit.

---

```
ARBITER-DECISION
STEP: 3
APPROACH: parse each PSK31 message into speaker, addressee, kind, turnover and certainty against the transcript corpus, split the growing row at turnover words, and feed the parse to the unchanged FT8 row readers
MOVE: continue
WHY: Steps 0 to 2 are done, step 2 by the separate reading of unit 315's report, and step 3 is next in the plan's one pipeline with nothing blocking it; the loop test finds no approach tried on step 3. Unit 315's eight section 4 items are logged as asks 17 to 24, not chased - none bears on a step 3 criterion.
STATE: not started
DECIDED: three on the arbiter's authority. The corpus lines are joined into one stream to make §3.4's "a message is what arrived between two turnovers" measurable. The corpus's self-contradiction on 05-garbled (expected 0.2, two of five lines uncertain) is reported under both readings and not resolved. Task 5, end to end from the four-signal audio, is the drop candidate because it moves no must-pass.
LICENCE: PHASE_PLAN.md step 3 entry and exit criteria, §1, §3.4, §3.5, §R1, §R3, §R9 and §6 branching; ARBITER.md §6, which makes the tasks, the tiering and the drop candidate the arbiter's
ACCOMPLISHED: with PSK31 pressed, the CQ filter shows the stations calling CQ, a station calling Tim lands on his side, and every row says who is speaking and whether Hamlet is sure - with the same fade, country and quill an FT8 row gets, and nothing that transmits
ADVANCES: step 3 - the corpus per-line conclusions, the unknown rate, own-callsign recognition, the CQ filter, reuse with no change to their code, guessed-versus-certain in the view model, and name and QTH never parsed; plus the no-report nice-to-pass
END-ARBITER-DECISION
```

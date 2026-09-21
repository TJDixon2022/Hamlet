```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Step 0 partial, step 1 done, step 2
   partial on 2.4 alone, step 3 done by units 380 and 381, steps 6 and 8 done, step 7
   partial on 7.2 as the judging session returned and task 0 transcribed, step 5 not
   started and Tim's own. Step 4 had zero units before tonight. This is the first
   unit ever spent on it, and it is the last step a unit can do.
B. Step 4, criteria 4.1, 4.2 and 4.3 - the sheet, the proof and R11. 4.1 met: for
   each mode, the presses in order, 10 screen sentences quoted at the steps they
   belong to, 14 refusal sentences explained of the 9 the send path can produce plus
   the capture's own, the capture and record paths named, and what to send back. 4.2
   met: 26 quoted sentences, 21 checked against what the app itself produced and 5
   found in the source, plus the backward direction - 12 of 12 reachable refusals
   quoted. 4.3 met: 150 prose lines scanned against 11 words, 0 hits.
C. The report last. Section 4 raises 7 items on top of the carried twenty-eight, and
   none of them is in the way of a criterion in B. Unit 381's items 2, 3 and 4 came
   off the queue - two were measurements this instruction now states as fact and the
   third closed its own question - and its items 1 and 5 stay on. Step 7's verdict
   was transcribed and 7.2 is open and parked.
```

```
UNIT:       382 - complete at task 4 of 5 - 2026-09-21 19:13
PHASE GOAL: Hamlet keeps what the PSK31 and Olivia threads already banked - screen,
            record and test work that needs neither the radio nor the owner - and
            proves it with tests that ran, until Tim looks at it himself.
UNIT GOAL:  Tim has never operated PSK31 or Olivia and has said so. There was a sheet
            for the screen and none for the air. Write the one page he holds at the
            radio - what to press, what the screen says back, what each refusal means,
            where the files are, what to send back - with every sentence on it quoted
            from the program and proved to exist by a test.
ADVANCED:   step 4, criteria 4.1, 4.2 and 4.3 - the whole of the step
NUMBER:     operator-facing sentences the sheet quotes: 26, of which 21 were
            checked against the sentence Hamlet itself produced and 5 were found
            in the source; the sheet is 176 lines and 10.7 kB against a target of
            120 and 10
DRIFT:      none - this unit advanced, so the count stays at 0 (was 0)
```

## 1. What Claude did

**Complete, at task 4 of 5 - all five tasks done, none dropped.** Windows 11, `C:\Source\HamLet`,
branch `main`; the project gate passed against the tree - `SHACK_FACTS.md` and
`src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` both present, neither `CoreHMI.sln` nor
`MURC.sln` at the root.

**Task 0 - the record, two verdicts and the entry run.** `CURRENT_STEP` 0 to 4 and
`WORK_INSTRUCTION` 381 to 382, both stale; version 1.13.68 to 1.13.69 with its line in the log;
`UNIT 382 - STEP 4` appended to `PHASE_OUTCOME.md` with the editor. **Two verdicts a separate
session returned were transcribed, and neither is a judgment of this unit's:** step 3 is `done` on
unit 381, and **step 7 is `partial` on 7.2** - the `done` in both step 7 headers came from unit
378's own entry at `PHASE_OUTCOME.md:274`, while the separate judging session's entry at line 277
returned `STATE_AFTER: partial` at line 289. That is transcription and not a criticism of unit 378
or unit 379; 7.2 is parked and nothing under `src\` was touched for it. **Entry carry-forward, both
invocations, one build each, status written immediately before each: app 220 of 220 in 2 m 17 s and
engine 150 of 150 in 4 m 54 s, both green on the first attempt.** The seven fixture types task 1
borrows, run by name with `ViewTestsActThroughControlsTests` beside them: 71 of 72, the one red the
inherited `NoViewTestWritesAPropertyAControlOwns`.

**Task 1 - the harvest.** `Unit382Trace`, six `[Fact]` items, no window needed, no file under
`src\` touched and nothing asserted about the product. It reads the press sequence off the tree
with the line each label was read from, and it drives the shipped view model into every state the
PSK31 and Olivia send path can refuse in, printing what Hamlet itself put on the panel. **Eight of
the nine refusals were produced character for character.** Three measurements disagreed with the
instruction and the measurement won - they are section 4 items 1, 2 and 3.

**Task 2 - the sheet.** `docs\RADIO_SHEET.md`, six sections in the ruled order, the quote
convention at the top and the honesty line beside it. **Every `>` line was copied out of the
trace's printed output rather than typed from memory.** Olivia is written as differences rather
than as its own walk - the named drop candidate, taken, with every press and every sentence kept.

**Task 3 - the guards.** `TheRadioSheetQuotesTheScreenTests`, four names, reaching the sheet by the
walk to the repository root that `DecisionLogOrderTests.cs:49` already uses. 4.2 in both
directions, 4.3's word scan, and the size reported as a number rather than as a red. **The carry-
forward line went in the same commit, by type, with its paragraph.** The backward direction found a
real gap on its first run and **the sheet was fixed, never the test**.

**Task 4 - the exit run.** Both invocations, one build each, status immediately before each: **app
224 of 224 on the second attempt, engine 150 of 150 on the first.** Name for name against task 0's
220 and 150, this unit's four new guards the only difference. Four commits, one per task, each with
a pathspec, and the push at the end.

**Two decisions this session made for itself, both reproduced in full.**

**(1) The sheet quotes the mode gate's sentence even though that gate cannot fire under his two
modes.** Ruling 1 item 6 says a sentence Hamlet does not say does not go in the sheet. Hamlet does
say this one - it is produced character for character under WSPR - and the sheet's row names WSPR
as the chip it belongs to and says to press PSK31 or Olivia. **The alternative was leaving one of
the nine refusals off a page whose whole claim is that it leaves none off**, and a reader who did
meet it would have had nothing to look it up in. It is quoted, marked as belonging to a third chip,
and reported as section 4 item 1.

**(2) The R11 word scan matches whole words rather than substrings.** On its first run it reported
three hits, and all three were the word `rig` inside `Right-click` and `right now`. **A substring
scan answers a different question from R11's**, and the only way to make the sheet pass it would
have been to paraphrase ordinary English - which would have degraded the page to satisfy a
measurement. The scan is now `\brig\b` and the reason is in the test's own remarks. This is a
correction to a test this unit wrote tonight and is not a loosening of an inherited one: nothing
before this unit scanned the sheet at all, and the scan is stricter in every case that is actually
R11's.

## 2. What the owner should expect

**You can now sit down at the radio with one page open and know what to do.** `docs\RADIO_SHEET.md`
tells you which button to press and in what order for PSK31 and for Olivia, what the screen should
say back at each press - in the screen's own words, not a description of them - what every sentence
that refuses a send actually means and what to do about it, where your capture and your record are
kept, and exactly what to send back when something goes wrong and what is not worth sending. Every
sentence on it was checked against the program rather than remembered: twenty-one of the twenty-six
were compared with the string the application itself produced, character for character, and the
other five were found as literals in the source. It is checked the other way round too - every
refusal the send path can reach is on the page - so the page cannot quietly go out of date: a later
unit that adds a refusal, or changes a sentence, turns a test red until the page catches up.

**Nothing on your screen changed. No file under `src\` changed at all** - not a label, not a
sentence, not a layout. **No port was opened, no device was enumerated and nothing was keyed**
(FACT-004); every sample in every fixture lived in an array. **The page will look wrong in one
way and is not:** it is 176 lines rather than the 120 that was aimed at, because the sentences
Hamlet actually says are long and the page may not shorten them by rewording them.

## 3. What you should see

### The refusal table, read back out of the sheet, beside what Hamlet produced

Every row below is a line in section 4 of `docs\RADIO_SHEET.md`. **Kind (a)** means the test drove
the view model into that state and compared the sheet's quote with what Hamlet put on the panel,
character for character. **Kind (b)** means the literal was found in a file under
`src\Hamlet.App`. A `<marked>` part is a value spliced in at the moment the sentence is said, and
what is compared is each fixed part either side of it, in order.

| What refused | The sheet's quote | What Hamlet produced in the test | Kind |
|---|---|---|---|
| Nothing in the message | *There was nothing to send.* | identical | **(a)** |
| No way to key anything | *Hamlet composed the PSK31 call and sent nothing: no radio is connected and no transmit audio device is named in Settings.* | identical | **(a)** |
| No transmit device chosen | *No transmit device is chosen. Open Settings and pick the radio's sound card.* | identical, and it is the whole of `DigitalSendLine` and of `TransmitRefusalSentence` | **(a)** |
| The device would not open, on the send line | *Hamlet composed the PSK31 call and sent nothing: the transmit audio device named in Settings could not be opened. Device: `<the card's name>`. Rate asked for: `<n>` samples per second. The operating system said: `<what Windows said>`.* | `... Device: USB Audio CODEC. Rate asked for: 12000 samples per second. The operating system said: the device is in use by another application.` | **(a)** |
| The same fault, shown on its own | the same sentence without the wrapper and without the closing period | `the transmit audio device named in Settings could not be opened. Device: USB Audio CODEC. ...` | **(a)** |
| Nowhere clear to call | *Hamlet did not call: the band is too crowded here to call without landing on someone. It looks for a spot at least 150 Hz from every carrier being read and from every candidate over quality 0.4, in the middle of the widest such gap between 400 and 2200 Hz, and there is not one right now. Move the dial a little, or wait for somebody to finish.* | identical | **(a)** |
| Longer than the cap | *Hamlet did not send the PSK31 call: this is `<n>` s of Psk31 audio, and this send may be at most `<n>` s so that a continuous carrier cannot run on. Nothing keyed.* | `... this is 316.93 s of Psk31 audio, and this send may be at most 30 s ...` | **(a)** |
| Olivia cannot announce itself | *Hamlet did not call in Olivia: a send has to begin with the burst naming its variant, and `<which file, and what is wrong with it>`. So nothing went out, because a signal nobody can name the variant of is a signal nobody can read.* | produced with the three unreadable files named in the middle | **(a)** |
| The Olivia variant is not proved | *Hamlet did not send it: it has not proved to itself that it can read back Olivia `<variant>`, so it will not put that variant on the air.* | `... it can read back Olivia 8/250, so it will not put that variant on the air.` | **(a)** |
| The text holds what the mode cannot carry | *Hamlet did not send it: `<what the composer said>`.* | `Hamlet did not send it: the text holds U+2014, which Olivia cannot send as itself (Parameter 'text').` | **(a)** |
| The chosen mode cannot send at all | *Hamlet cannot send `<the mode>` yet, so nothing went out. It can hear this mode before it can answer in it, and sending anything else here would put the wrong kind of signal on a frequency people are using for `<the mode>`.* | produced under WSPR, which is the only chip it can fire under - section 4 item 1 | **(a)** |
| The right-click menu is empty | *Hamlet could not read its canned lines, so it is offering none: `<which file, and what is wrong with it>`* | not produced; found in `MainWindowViewModel.cs` | **(b)** |
| A capture kept nothing, PSK31 or Olivia | *Nothing arrived while it was running, so no file was kept.* | identical, from a second press with nothing heard | **(a)** |
| A capture would not write | *Could not write the capture: `<what Windows said>`* | not produced; found in `MainWindowViewModel.cs` | **(b)** |
| Nothing was listening | *Nothing is listening, so there is no audio to keep. Connect a radio or pick the training radio and press it again.* | not produced; found in `MainWindowViewModel.cs` | **(b)** |
| No audio had arrived | *No audio has arrived yet, so there is nothing to keep.* | not produced; found in `MainWindowViewModel.cs` | **(b)** |

**Backward: 12 of 12.** Every refusal the test can reach on the PSK31 and Olivia send path is
quoted on the sheet. That direction found a gap on its first run - the would-not-open reason shown
on its own under the send line is a different string from the send line's own sentence - and the
sheet gained a quote for it. **A refusal that exists and is not on the sheet fails that name**,
which is what stops the page going stale after tonight.

### The press walk, in the sheet's order, with the screen's words at each step

**PSK31.** Open the Digital tab; the mode strip is five chips under the caption *on this
frequency*. Press the **PSK31** chip and the line beside the strip becomes *listening for PSK31
across the whole passband. Every signal Hamlet is sure is PSK31 gets a line of its own below, with
where it sits, how strong it is and its text as it arrives, and a line from somebody calling
anybody carries an Answer that replies on his own frequency.* The decoded panel reads *nothing
decoded yet. Every station Hamlet is sure is sending PSK31 gets a line here, filling in a character
at a time as it arrives. A signal it is not sure of gets no line at all rather than a guess, so an
empty panel can mean a quiet band or a signal too rough to read.* The send area reads *nothing sent
yet* with the hover *Right-click a decoded row to choose a message, or press CQ.* Press **CQ** -
the one in the send area - and it reads *Sending "`<your line>`" now, at `<n>` Hz in the passband.*
**Right-click any row that names a station** for the seven lines from `data\psk31\canned.json`.
**Stop** is in the status bar, reads `Stop` at rest and `Stop transmitting` when something is
armed, and asks nothing first. When it has gone: *Sent "`<your line>`" - `<n>` s of PSK31.*

**Olivia, as differences.** The same walk. The variant comes from the station being answered and is
never chosen from a menu - there is no variant control anywhere in Hamlet. Every send begins with a
burst naming the variant, and a variant Hamlet has not read back off its own audio is refused. The
panel and the send line say Olivia: *Sent "`<your line>`" - `<n>` s of Olivia.*

### The paths, and what a returned file lets a reader say

| What | Where |
|---|---|
| The record | `%AppData%\Hamlet\telemetry\`, one file a day, `yyyy-MM-dd.jsonl`, in UTC |
| A PSK31 or Olivia capture | `%AppData%\Hamlet\captures\`, named `psk31-` or `olivia-yyyy-MM-dd-HHmmss.wav` |
| A `keep the last 30 seconds` capture | `%AppData%\Hamlet\captures\digital\`, named `ft8-yyyy-MM-dd-HHmmss.wav` |
| Settings, and your own canned lines | `%AppData%\Hamlet\settings.json` and `%AppData%\Hamlet\canned.json` |

**The two capture folders are not the same folder** and the sheet says so. **A capture's name is a
timestamp and nothing else** - no callsign, no band, no station (HM-DEC-018 §2.1) - and that
timestamp is the whole method of matching a returned WAV to a line in the record. Three events
answer Tim's four questions: **`on_screen`** says what was drawn, filtered, scrolled away, folded
or trimmed, with each row's own place in whole Hz, and is what answers *why did my list look
empty*; **`psk31_send_refused`** says which gate refused a press, by token and stage, and is what
answers *I pressed it and nothing went out*; **`psk31_capture_finished`** carries the seconds, the
bytes, the SHA-256 and what was being heard, and is what matches a file to its evening.

### The R11 scan

The word list, the author's, with its reason in the test's own remarks: **knob, VFO, PTT, mic gain,
RF gain, transceiver, rig, tune the radio, turn the radio, on the radio, at the rig's front.**
**150 prose lines scanned, 11 words, 0 hits.** The scan is over the sheet's own prose and
deliberately not over its `>` lines, because a quoted refusal is what the screen already says and
is out of this unit's hands - and two of the quoted sentences would hit the list, which is section
4 item 4.

### The sheet's measured size

**176 lines and 10,918 bytes - 10.7 kB - against a target of 120 lines and 10 kB.** Over on both.
**3,442 of those bytes are the 26 quote lines**, which the sheet's own convention forbids wrapping,
so five sentences occupy one very long line each. Ruling 2 item 2 says the content is the criterion
and the length is a target, and the number is reported rather than bought down by dropping a
refusal or paraphrasing a quote.

### The carry-forward counts, and the fate of every invocation

| Invocation | Attempt | Result | Fate |
|---|---|---|---|
| Entry, app | 1 | **220 of 220** in 2 m 17 s | green, no dispatcher loop |
| Entry, engine | 1 | **150 of 150** in 4 m 54 s | green |
| Entry, the seven borrowed types + `ViewTestsActThroughControlsTests` | 1 | 71 of 72 | the one red inherited |
| Exit, app | 1 | 223 of 224 | **the dispatcher loop** at 1 ms on `TheWorkingPanelsScrollInsideThemselvesRatherThanCollapsing`, before any assertion |
| Exit, app | 2 | **224 of 224** in 2 m 23 s | green |
| Exit, engine | 1 | **150 of 150** in 4 m 53 s | green |
| Exit, the same types + this unit's trace | 1 | 77 of 78 | the same one red, unmoved |

**Name for name against task 0, the only difference is this unit's four new names**, all in
`TheRadioSheetQuotesTheScreenTests`. **No regression, and nothing red that was green before**
(HM-DEC-165) - which is what should be expected, because no file under `src\` changed at all. **Not
one name failed an assertion in any of the seven invocations.**

## 4. What's blocking us

**1. The `mode` refusal cannot fire under PSK31 or Olivia, and the sheet quotes its sentence
anyway. A finding, and the decision beside it is the author's and overrulable.** The gate at
`MainWindowViewModel.cs:16400` writes `Psk31Events.SendRefused(..., "mode", ..., "gate")` only when
`IsPsk31Chosen`, and `CanTransmitIn` at `:2491` answers **true** for FT8, FT4, PSK31 and Olivia -
so the only chip the gate refuses is WSPR, where `IsPsk31Chosen` is false and no PSK31 token is
written at all. **The token at that site is unreachable in the shipped tree.** The sentence is not:
it is produced character for character under WSPR. Work instruction 382 counts nine refusals
"seven on the PSK31 and Olivia send path"; the measurement says the seventh of those seven is a
WSPR sentence. **Nothing was repaired.** It wants no ruling; if the arbiter wants the token gone or
the gate widened, that is a later unit's and it touches a send path.

**2. The four `DigitalCaptureRefusal` values belong to the other capture button. A finding.**
Section 5 of the instruction names them as the sheet's section 4 material, and they are - but all
four are written by `CaptureDigital`, the `keep the last 30 seconds` press, which writes
`ft8-<stamp>.wav` into `captures\digital`. **The PSK31 and Olivia press is a different control**
with its own folder (`captures`), its own file name (`<mode>-<stamp>.wav`), its own line on the
screen (`Psk31CaptureWhere`) and **two sentences of its own**, and it writes no
`DigitalCaptureRefusal` at all. The sheet carries all six and says which button each belongs to.
Nothing was repaired.

**3. Only one of the three `Content="CQ"` buttons sends, and the instruction left which to the
reading. Answered.** `MainWindow.axaml:3558` is `DigitalSendCqButton` on `SendCallToAnyoneCommand`
and is the one; `:3940` is the CW compose macro that fills a line and sends nothing; `:4215` is
`DigitalFilterCq`, the decoded list's filter chip. The sheet says so in section 2 step 5, because
pressing the filter and expecting a transmission is the exact mistake the page exists to prevent.

**4. Two sentences Hamlet says would hit R11's own word list, and this unit may not touch either.
A finding, and it is the honest weakness of 4.3.** The crowded-band refusal ends *Move the dial a
little, or wait for somebody to finish*, and the stop's own line can end *If it is still
transmitting, stop it at the radio.* Both are Hamlet's sentences, both predate this unit, and 4.1
forbids paraphrasing inside a quote - so the sheet quotes them as they are and the scan excludes
`>` lines. **The exclusion will look like a loophole and is not:** what R11 governs is what this
unit wrote. Reported, not repaired. If the arbiter wants either sentence changed, that is a source
change on a send path and it is the stop.

**5. Five of the twenty-six quotes got kind (b), and that is the weakness of the proof.** They are:
*on this frequency*, found in `DigitalIdleText.cs`; and the four capture sentences - *Hamlet could
not read its canned lines...*, *Could not write the capture: ...*, *Nothing is listening...* and
*No audio has arrived yet...* - all found in `MainWindowViewModel.cs`. **The reason each one could
not be produced:** the first is markup a headless fixture would have to draw a window to read, and
the other four need a state no fixture in this project can reach without a malformed canned file on
disk, a real audio tap, or a folder the process cannot write to. **The nine send-path refusals all
got kind (a)**, which is what ruling 1 item 4 asks for; the four capture fallbacks are the drop
candidate task 3 named, taken.

**6. The sheet is over its length target on both numbers. Reported, not bought down.** 176 lines
and 10.7 kB against 120 and 10 kB. **What bought it:** 26 quoted sentences on 26 unwrapped lines,
3,442 bytes between them, five of which are single sentences over 250 characters; and 14 refusal
rows each carrying what it says, what it means and what to do, which is 4.1's content. Ruling 2
item 2 rules this case and nothing was dropped.

**7. `validate-output.bat` refused for the fourth unit running, and the six rules are hand-checked
below. A finding about the harness, not about the report.** The command was run in the exact shape
section 2 names:

```
./tools/arbiter/validate-output.bat output.md
```

and the exact refusal was:

```
This command requires approval
```

That is the permission mode and not the syntax - **a non-interactive session cannot answer it** -
and it is the same refusal from the same shape that units 379, 380 and 381 all reported. **What
follows is a HAND-CHECK against the script's own source at `tools\arbiter\validate-output.bat`, not
a run of it, and it is worth exactly what a hand-check is worth: the script was not executed and
nothing independent agreed with me.**

| Rule, as the script states it | Hand-check | Verdict |
|---|---|---|
| 1 - a `UNIT:` line above section 1, parseable, within the first 60 lines | `UNIT:` is line 24; section 1 is line 41; the file is UTF-8 with **no BOM** | **ok** |
| 2 - the four top-level sections, in order, exact names | the only `^## ` lines are `1. What Claude did`, `2. What the owner should expect`, `3. What you should see`, `4. What's blocking us`, in that order, matching the script's `WANT` word for word including the apostrophe | **ok** |
| 3 - no fifth top-level section | four `^## ` lines and no more; the `### ` headings under section 3 and the carried queue are ones the script's own comment says it ignores | **ok** |
| 4 - section 4 present even when empty | present, and it is not empty | **ok** |
| 5 - section 3 non-empty | 90 non-blank lines between `## 3.` and `## 4.` | **ok** |
| 6 - the ordering block above the `UNIT:` line, with A, B, C, and C naming a count | `READ IN THIS ORDER.` line 2, `A.` line 4, `B.` line 9, `C.` line 16 - all inside the 60-line window - and C says *Section 4 raises 7 items*, which matches the script's `raises \d+ item` | **ok** |

**All six rules pass on the hand-check. Nothing verified them but me.** The ask stays on the queue
as unit 379's item 7; this unit adds only that it is now four units in a row.

**The `RULES_AT` id-scheme split, reported and not repaired.** `PROJECT_STATUS.md` reads
`HM-DEC-165 (2026-09-19)` because `tools/status.sh` writes that field as a literal, while
`CLAUDE.md` §1 holds `CPS-DEC-0165`. **`tools\` is not this unit's to edit.** Four units have now
reported it.

**No item of the kind section 12 singles out arose.** **No criterion needed a change under
`src\`**, and no file under `src\` was opened except to read it. No stage on the two modes' send
path was found silent - every one of the twenty-four `DigitalSendLine` sites has a sentence, so the
sheet never had to say *Hamlet says nothing here*. One sentence reads oddly and is reported rather
than repaired: **the cap refusal says `Psk31` where the whole of the rest of the screen says
`PSK31`**, because that word comes off the `UnslottedMode` enum's own name. It is quoted as it is.

### The carried queue, verbatim per HM-DEC-139 - twenty-eight, and this unit answers none of them

- **Unit 381 item 1** - `AScrollerThatSettlesSaysWhatIsInsideItOnBothPanels` waits 600 ms of wall
  clock for a 250 ms `DispatcherTimer` and loses that race under parallel load. Proved
  environmental against nine types unit 381 never touched; the pre-change rate is unmeasurable
  because `git stash` and `git checkout -- <paths>` were both refused. **Not on the carry-forward
  list; no task here opened that file, and it did not arise.**
- **Unit 380 item 1** - `AddDecodeRowForTests` is half a door: it reaches `PlaceRow` while the
  decoder's own door `AddDecodeRow` also keys the duplicate set and runs the trim.
- **Unit 380 item 4** - the file-lock `IOException`, the test's own reader racing the telemetry
  writer's background thread. **It arose once tonight, in this unit's own new trace**, and was
  answered inside the trace rather than reported as new: the reader now opens with
  `FileShare.ReadWrite`, or reads after the writer is closed. The ask stays on the queue because
  the shape is still there for any test that reads a live record.
- **Unit 379 item 1** - the CQ list's mode label.
- **Unit 379 item 3** - the `80m` spelling.
- **Unit 379 item 7** - `validate-output.bat` returns `This command requires approval` from the
  exact documented shape, which is the permission mode and not the syntax, and a non-interactive
  session cannot answer it. **Four units running.**
- **Unit 378 items 1, 3 and 4.**
- **Unit 377 item 4.**
- **Unit 376 items 3, 4 and 5.**
- **Unit 375 item 3** - Avalonia's headless `InvalidProgramException: You have caused dispatcher
  loop`, which kills an app invocation at about 1 ms in `HeadlessUnitTestSession.EnsureApplication`
  before any assertion and moves between names. **Twelve occurrences now**, tonight's being the
  first exit app attempt on `TheWorkingPanelsScrollInsideThemselvesRatherThanCollapsing` - a name
  it has landed on before, which is the first repeat in seven units.
- **Unit 375 item 4.**
- **Unit 374 item 3.**
- **Unit 373 item 2.**
- **Unit 372 items 4 and 7.**
- **Unit 371's five.**
- **Unit 369's four.**

**Which of unit 381's five came off, and why.** Item 2 - the record named two carriers before that
unit, not one - and item 3 - the one new key is three keys and the one new parameter is one record
- were measurements that refined their own instruction and won, and work instruction 382 section 3
states both as facts. Item 4 - `OnScreenBy` has no squelch token - closed the question instruction
381 asked. **Items 1 and 5 stay on, and with unit 380's two and the rest the queue is
twenty-eight.**

**Step 7 is `partial` on 7.2 and it is parked.** Task 0 transcribed the separate judging session's
verdict into both files and stopped there. Nothing in `src\` was touched for it, the token was not
changed, no macro row was routed through `SendCannedPsk31`, and unit 378's reading was not
re-argued.

**Step 5's entry is `step 4 done`, and step 5 is Tim's own.** Nothing here drafts his verdict,
presumes it, or writes a checklist for it.

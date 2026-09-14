```
READ IN THIS ORDER.

A. The phase goal - the screen, done right. Steps 0 to 2 done, 3 waits on Tim.
B. No criterion changes state; this unit clears the blocker under R2, overruled.
C. The report last, and section 4 raises 5 items on top of the carried queue.
```

```
UNIT:       357 - complete at task 5 of 5, none dropped - 2026-09-14 10:57
PHASE GOAL: The screen, done right, with PSK31's carried work under it.
UNIT GOAL:  Right-click any station's row and get his card; type a line on it and
            one click sends it, framed with the callsigns and the hand-back; read
            a whole message on hover.
ADVANCED:   blocker
NUMBER:     things Tim can send on PSK31 4 -> 5 (a typed line); cap 30 s -> 60 s
            for typed, and still 30 s for every macro
DRIFT:      carried
```

**Every appearance claim is computed, not seen**, and nothing here is evidence about the
radio: a fake port and a fake sink on a machine with none (FACT-004, FACT-006).

## 1. What Claude did

Gate passed on all five. Branch **`main`**, seven commits, pushed, nothing left uncommitted.
Version **1.13.43 → 1.13.44**.

### Task 0 — the record

`PHASE_PLAN.md` gains **R29** from Tim's words of 2026-09-14: **typed text goes on the air
from a station's card, framed by Hamlet.** It withdraws the no-keyboard line of PSK31 plan
**R2** and nothing else of it: the four macros stand and their words stand. What the ruling
writes down so a later unit cannot take it back quietly is what did **not** change — one
click one transmission, one keying sequence, the certainty gate on Report and Confirm, and
the rule that typed text never enters the record.

### Task 1 — a right-click on any row makes that station's card

Until now a card appeared only where a station had certainly addressed him, or where Hamlet
had already sent to one. **Neither is true of two people talking to each other**, which is
exactly what he was reading off 14.070.

`Psk31StationOn` asks only that the parser named somebody and that somebody is not him.
**`Psk31CqOn` is untouched and still strict**, because what it gates is a transmission:
answering a callsign that did not read cleanly would put a neighbour's call on the air.
Opening a card sends nothing, so it asks less. A second click cannot duplicate the card, and
brings it to the front.

### Task 2 — the text block, and one click sends it framed

Under the turn indicator: a text block and a Send button. One click composes

```
<HIS> de KC3QIS  <what he typed>  BTU <HIS> de KC3QIS K
```

and hands it to `SendMessage` — **the same door the four macros go through**: the same
composer, the same arming, the same `Ft8TransmitSequence` with the one `PttOn` site, the same
`Stop`. **No second keying path exists to assert about.**

**It is not gated on certainty.** Where the parser is unsure whose turn it is the card says
so beside the button and the click sends anyway; the block is withheld only where it is
*certainly* the other station's turn. A whitespace line sends nothing. A character outside
the varicode is dropped and counted rather than refusing the whole line.

### Task 3 — the cap fits a conversation

**The cap travels on the send rather than being a constant every send shares.**
`UnslottedTransmission` carries `LongestSeconds`, defaulting to the old thirty. A typed line
asks for sixty; **every macro is still held to thirty.**

Raising the constant would have been the smaller diff and the worse change: the cap exists
so a composing fault cannot leave a continuous carrier on the air, and doubling it for every
unslotted send would have doubled that exposure to buy something only one send needs.

### Task 4 — the whole message

Hover gives the full text, wrapped, with the station and the time; a click opens the same
thing in a light-dismiss box with an X. Ended rows too.

### Nothing was recorded under §12.1

R29 is Tim's ruling transcribed into `PHASE_PLAN.md`, as task 0 directed. No `DECISIONS.md`
entry was written.

### Tests

**No suite was run**; every name filtered, foregrounded, 480 s timeout (HM-DEC-155).

| Run | Result |
| --- | --- |
| Carry-forward, app, before | **127 of 127** on the third run — see section 4 item 1 |
| Carry-forward, engine, before | **86 of 86** |
| `ThePsk31ExchangeTests` extended (task 1) | **7 of 7**, watched failing 2 first |
| `TheTypedLineGoesOutTests` (new, tasks 2 and 3) | **9 of 9**, watched failing 3 then 3 first |
| `TheWholeMessageTests` (new, task 4) | **4 of 4** |
| Final app run, carry-forward plus every name touched | **148 of 148** |
| Final engine run | **86 of 86** |

**213 green before, 234 after, nothing new red.** `TheUnslottedSendTests`,
`TheFt8AndFt4SendsAreByteIdenticalTests` and `TheStopIsAlwaysOnScreenTests` are green and
**unedited**.

## 2. What the owner should expect

**The build is clean** — zero warnings, zero errors.

Right-click any station's row in the decoded list and his card opens, whether or not he was
calling CQ and whether or not his carrier is still up; right-click it again and the card
comes to the front rather than a second one appearing. On the card, under the line that says
whose turn it is, there is now a box you can type into and a Send button. One click sends one
transmission: Hamlet puts his callsign and yours in front of your words and `BTU <HIS> de
KC3QIS K` behind them, so the two things that are easy to forget on a keyboard mode are never
yours to remember, and what is in the box is exactly what goes out between them. The box tells
you how many seconds your line will take as you type it, refuses past sixty with the character
count so you know how much to cut, and **Stop stops a typed send exactly as it stops a CQ** —
it is the same keying path, not a new one. Hovering a row now shows the whole of what a
station said rather than the first few words, and clicking the text opens it in a box you can
dismiss.

**What will look wrong and is not.** The block is missing on a card when Hamlet is certain
the other station is still sending. Where it is unsure, the block is there with a word saying
so, and the click still sends: that is deliberate.

**Pushed to `main`.**

## 3. What you should see

**One framed line, exactly as it would go out:**

```
you type   Nice signal here in Trafford, running 40 watts

goes out   EI4GNB de KC3QIS  Nice signal here in Trafford, running 40 watts  BTU EI4GNB de KC3QIS K
           21.6 s of PSK31
```

**The cap, both sides of it:**

```
190 characters   48.7 s on the air                     sent
260 characters   87.1 s on the air, too long to send   refused, nothing keyed

the card says: Too long to send: 260 characters comes to 87.1 s on the air, and
the most a typed line may be is 60 s. Nothing was sent. Shorten it and press
Send again.

a macro's cap is 30 s and a typed line's is 60 s
```

**The record, with none of his words in it:**

```
{"macro":"typed","characters":77,"seconds":18.75,"capSeconds":60,"withinCap":true,"offsetHz":1234}
{"reason":"cap","macro":"typed","stage":"arm"}
```

The test types *meet me behind the barn at midnight* and then scans every line of the file for
*midnight*, *barn*, `EI4GNB` and `KC3QIS`. None of them is there.

**A right-click on two strangers talking, which is what he was reading:**

```
row    : 1234 Hz  CQ CQ CQ DE F4DIA F4DIA F4DIA K / F4DIA DE EI4GNB EI4GNB K
reading: Answer, speaker EI4GNB
cards  : 1 card(s) EI4GNB [Unknown]
```

**And the text block's offer:**

```
Unknown                can type True
His turn               can type False
His turn, a guess      can type True
Your turn              can type True
```

## 4. What's blocking us

No criterion changes state. Five items.

**1. The app carry-forward list is not stable run to run, and it got worse this unit.**

*No ruling wanted; a finding, and it firms up unit 355 item 6.* Three runs of the same filter
before anything was changed: the first failed `TheTestsStayOffTheNetworkTests.The354LayoutReadsTheSameNumbersTwiceRunning`, the second failed **four different tests**
(`ThePsk31CqGoesOutTests.ASecondPressRefreshesTheReceiptAndSendsAgain` and three in
`TheStopIsAlwaysOnScreenTests`), and the third was 127 of 127. **Every one of them passed when
run alone.** Parallelism is already off — `TestParallelism.cs` disables it assembly-wide — so
this is state or timing leaking between headless-window tests in one sequential run, not two
threads fighting. **It makes a green baseline a thing you have to run three times to believe**,
and it is the reason this report's "before" number names which run it came from.

**2. The box's head names the station only where the parser named one, and on ordinary
conversational text it often does not.**

*No ruling wanted; a finding.* `WholeMessage` puts `Sender` above the text, and `Sender` is
the speaker of the latest **complete** message. Writing task 4's test, three different
plausible transcripts of a real ragchew line produced an empty `Sender`, so the box showed the
time and the words with nobody's name on them. **The box does not go looking for a callsign in
the text itself** (§0.0) — a station Hamlet has not named is not named there either — so what
is missing is upstream, in when the splitter decides a message has finished. Worth a look
before somebody reads a box a week later and cannot tell whose words those were.

**3. `ADVANCED: blocker` has nowhere to go in `PHASE_OUTCOME.md`.**

*No ruling wanted; a mismatch, reported and not repaired.* Task 0 asks for the entry to carry
`ADVANCED: blocker`. `outcome-entry.py`'s `FIELDS` is twelve names and `ADVANCED` is not one
of them, and no entry in the file has ever carried it. It is in this report's header block,
where §12 defines it, and the entry says *clears a blocker* in its prose as every earlier unit
has.

**4. The typed line is not in the conversation the card reads.**

*No ruling wanted; a finding about what the card will say next.* A macro goes through
`RememberWhatWeSent`, which parses it and files it, so the turn indicator moves when Hamlet
answers. **A typed line does not**: it is not a macro, `Psk31ExchangeParser` would read its
frame as an ordinary over, and whether a free sentence should move the turn is a question
nobody has ruled. The card is refreshed after a typed send, so the screen is consistent; what
it is not is *aware* that he just spoke. Left deliberately.

**5. §2's tool facts, checked again.**

*No ruling wanted; a mismatch report.* **Apostrophes in a quoted heredoc broke again**, exactly
as §2 says, on the first attempt at task 2's engine change; that work moved into script files.
**Python ran**, against §2, as it has for four units. `rm` was not needed. `tools/status.sh`
was not refused. **One new tool fact worth writing down**: a `sed` insertion that lands between
an XML doc comment and the member it documents produces `CS1572`/`CS1573` and fails the build,
because warnings are errors here. It happened four times this unit. Anchor on the doc comment's
first line, not the signature.

### Asks still outstanding - carried from unit 356's section 4, per HM-DEC-139, verbatim

The words below are unit 356's, from its line under `## 4. What's blocking us` to its end, as
committed in `9db74913`, with only that top-level heading dropped so this report keeps four
sections. **Its item 2** — `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow`
red — is neither answered nor re-measured here; it is not on the carry-forward list, and this
unit did not run it.


No criterion changes state. Five items.

**1. The cap is not a cure: the card still asks for 623 px at 1100×780 and is scrolling.**

*No ruling wanted; a finding, and the honest limit of task 1.* The send area is on the window
at all nine sizes, which is what was asked and what is measured. What did **not** happen is
the card getting smaller: its own box is unchanged at 623 px (653 on PSK31), it scrolls inside
a 300 px cap, and unit 354's trace still prints 623 because that is the truth about the
control. The three panels reach 0.217 of the height below the pills against R26's half.
**Making the card fit at that width is a wrapping and typography question** — the license line
takes 17 lines and the rule of thumb 17, each breaking words — and §10 kept this unit to its
own repair.

**2. `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow` is red, and it
is not this unit's.**

*No ruling wanted; a finding that firms up unit 355 item 4.* That unit called it red *at some
runs*. **It is red deterministically here**, and it was proved red before anything in this
unit changed, by stashing the layout change and running it alone. It fails on
`Assert.Single()` over the pills wearing the best-bet badge, with the collection empty while
the green block and the best bet both read *20 m*. It is not on the carry-forward list, which
is why the 112-green baseline did not catch it.

**3. A test was holding a sentence R15 forbids in place.**

*No ruling wanted; a finding worth one line.* `ThePowerIsOfferedTests` asserted that a reading
**with no reference behind it** produced a sentence containing *turn the transmit drive*. A
test that requires a judgement Hamlet is not entitled to make will keep that judgement alive
through every session that respects the suite. It has been corrected here, and the pattern is
the one §12.5 warns about one level up: the fixture was not wrong about the code, the test was
wrong about the rule.

**4. The ALC is still never read by the poll, so all four sentences are proved only from
handed-in readings.**

*No ruling wanted; carried and re-stated because this unit rewrote the sentences.* There is no
`CivRead` for `RigField.Alc` anywhere in the tree, so `RigStateMonitor` never fills it, and
`Psk31AlcForTests` is the only way any of this path has ever been exercised. **A command byte
is not invented to close that** (§0, §4). On a radio, the sentences will appear only once that
read exists.

**5. §2's tool facts, checked again.**

*No ruling wanted; a mismatch report, and this time in the instruction's favor.* **Apostrophes
in a quoted heredoc did break**, exactly as §2 says, on the first attempt at the ALC rewrite;
the work was moved into a script file. **Python ran**, against §2, as it has for three units
now. `rm` was not needed and not tested this unit. `tools/status.sh` was not refused.

### Asks still outstanding - carried from unit 355's section 4, per HM-DEC-139, verbatim

The words below are unit 355's, from its line under `## 4. What's blocking us` to its end, as
committed in `9d5322c5`, with only that top-level heading dropped so this report keeps four
sections.

**Three of its items are answered by this unit and are left in place rather than deleted**, so
the drop belongs to the report that records each ruling: **item 1** was ruled by Tim on
2026-09-14 (*"A, the way it has been"*) and is written into `PHASE_PLAN.md` as R28; **item 2**
and **item 3** were built here, in tasks 3 and 1. **Item 4** is re-measured in this report's
item 2 above and is **not** answered.


**One ask, most blocking first: whether Stop should be disabled with nothing keyed (item 1).** Tim's step 3 verdict
stays open. This unit's nine items come first; unit 354's section 4 follows, carried as section 1 decision 13 says.

### Raised by unit 355

**1. Ruling wanted: Stop pressable at every instant, or disabled with nothing keyed.**

*An ask: it touches the abort (`CLAUDE.md` §0.2).* Task 1 said *enabled only while a send is keyed and disabled
otherwise*; your quoted ruling is *Stop lives in the status bar, always*. Built: always pressable.

| Option | For | Against |
|---|---|---|
| A. Always pressable; the word and the edge change (built) | Meets the send plan's step 1 must-pass and its guarding test; a press before the slot un-arms a waiting send; works when Hamlet is wrong about what is keyed | Pressable when there is nothing to stop |
| B. Disabled only when nothing is armed and nothing is keyed | Grey when idle | Disabled at exactly the moment the app's idea of *armed* is wrong; step 1 and `TheOperatorCanStopItTests` would have to be overruled |
| C. Enabled only while keyed (the instruction's words) | Literal | Cannot take a waiting send off before its slot, the fifteen seconds after a wrong click; same overrule as B |

**The industry-standard answer is A**: an emergency stop is never disabled. Rejected B and C for the reasons in the
table. To overrule, say *grey Stop out when nothing is keyed*.

**2. Ruling wanted, low: *no HTTP client is created under test* does not hold.**

*A finding with a choice.* `MainWindowViewModel.BuildSources` (`MainWindowViewModel.cs:7869`, `:7873` at `b5be0db9`)
constructs `PotaActivitySource` and `SotaActivitySource` at construction, each with its own `HttpClient`, whether or
not they are switched on. The layout fixtures switch them off, so neither sends; no callook client is made. Options:
A, a unit that puts the spot sources behind the same kind of seam; B, create their clients on first fetch; C, accept
*no request leaves* as the test. Recommended A, for the reason task 4 exists.

**3. CQ is still below the window at 1100 x 780.**

*A finding.* This session's trace: CQ 54 x 22 at y 800 on FT8, 830 on PSK31, 816 on the plain window; on the window
at 900 x 620 (y 462, 474). The top row and zero-height panels of unit 354 item 2 are unchanged. Only Stop moved.

**4. `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow` is red at some runs.**

*A finding, not chased.* Red twice this session, with the network denied and with the fixed answer: no band pill
drew the text *best bet now*. Green in the next two runs, and in unit 354's runs at 02:13 to 02:53. The pill's label
is *likely, going on the hour* when nothing was heard (`BandOpportunity.cs:240`), and the test matches the literal.
It depends on the hour and the run's spot history, not on this unit.

**5. `TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission` is red, and older than this unit.**

*A finding, not repaired.* It asserts one `_armedSend.Arm(` line in `src`; there are two,
`MainWindowViewModel.cs:14439` in `SendMessage` and `:14618` in the PSK31 press, since `87485625`. This unit added
none. The guard's expectation predates the PSK31 door.

**6. Three stop tests failed once each under load.**

*A finding.* `TheOperatorCanStopItTests.TheLineSaysWhatHappenedToTheCarrierAndToTheSound` (1 of 3 isolated runs; the
stop landed after the audio ended), `AClickWhileTheToneIsPlayingStopsTheSoundAndNotJustTheCarrier` and
`TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns` each once in a combined run
of 35; each green on every rerun. Timing, not layout.

**7. On the four-signal recording, one carrier is still held 20 s after the audio ends.**

*A finding, engine, parked.* After the file and 20 s of faint noise the search still held the 2200 Hz carrier by
its keep-readable rule; 700, 1100 and 1608 Hz retired. Not touched (§9).

**8. Mismatches with work instruction 355, reported and not repaired.**

- §5: the sheet Tim reads is unit 349's, `docs/unit349-what-tim-looks-at.md`, not unit 350's.
- §5: `TheStopIsOnScreenTests` does not exist. Unit 354's measurement is `TheTopRowTests.Unit354TraceTheMainWindowAtTheSizesTimCanOpen`,
  a trace.
- §5 and task 1: there was one FT8 and PSK31 Stop, `DigitalStopButton`, in the send area in the tab row, not one per
  panel.
- §5: the status bar holds the tip mark and its line, the achievement quill button, the contact badge line and the
  belt ring with its progress; 46 px tall. There is no control named *tray mark* or *count badge*.
- §12 `NUMBER`: *5 of 9* does not match the record. Unit 354 found Stop off the window at 1100 x 780 only, 8 of 9.
- §11 *push at the end*: the owner's prompt says push each task, and each was pushed.
- Tool facts: refused, a `for` loop over `$f` (*simple_expansion*) and `;`; blocked, `tee` to `/tmp` and
  `git show … > output.md`; needed approval and not run, `git worktree add` and `git restore --source`.
  `tools/status.sh` writes `RULES_AT: HM-DEC-161 (2026-09-11)`; this unit's writes kept it until the last, which is
  set to `HM-DEC-163 (2026-09-12)`.

**9. Where unit 354's items stand after this unit.**

- Item 1, Stop below the window at 1100 x 780: *ANSWERED for Stop* by your ruling A and task 1; CQ still below
  (item 3 above).
- Its finding that the plain fixture asks callook.info: *ANSWERED* by task 4.
- Items 2 to 6: unchanged.

### Asks still outstanding - carried from unit 354's section 4, per HM-DEC-139

Unit 354's opening, verbatim, from its line under `## 4. What's blocking us`:

**One ask, most blocking first: Stop is drawn below the window at the size Hamlet opens at (item 1 under
*Raised by unit 354*).** Tim's step 3 verdict stays open.

Unit 353's section 4 comes first, verbatim per HM-DEC-139, from its line under `## 4. What's blocking us` to its
end, as committed in `0d69123a`. It was kept in place with the file editor, and the marks work instruction 354
§9 asks for were added:
- unit 349 item 1, *STILL OPEN*;
- unit 353 item 2, *TAKEN UP by work instruction 354 ruling 78*, with the result;
- unit 353 item 3's `Unit332TwoWidthsTests` bullet, *LOGGED, NOT CHASED*;
- unit 353 item 5, *UPHELD for the reloads*.

This unit's six items follow at the very end, under *Raised by unit 354*. Item 1 is an ask; the rest are
findings.

**The queue unit 354 carried from units 337 to 353** is `4c55deac:output.md`, lines 280 to 2341, unchanged, not
retyped here (section 1 decision 13). Unit 349 item 1, Tim's step 3 verdict, is among it and *STILL OPEN*.

### Raised by unit 354

**1. Ruling wanted: at 1100 x 780, the size Hamlet opens at, Stop is drawn below the window.**

*Mark, unit 355: ANSWERED for Stop by Tim's ruling A of 2026-09-14 and work instruction 355 task 1 (`151a108d`); CQ
still below the window, unit 355 item 3.*

*An ask, under ruling 77: it touches the abort (`CLAUDE.md` §0.2).*
- **Measured** (`00454639`, computed on the host, not seen): the window's bottom edge is at y 780.
  `DigitalStopButton` is 74 x 22 at y 800 on FT8 and 830 on PSK31, and at 798 on the plain window. `DigitalSendCqButton`
  and `ModeTabs` are beside and above it. It is on the window at 900 x 620 (y 462) and at every other size measured.
- **Cause as measured:** the neighborhood card's green block is 218 px wide at that width, its lines stack to a
  623 px top row (653 on PSK31), and the rows under it are pushed off the window. The same geometry gives
  item 2's zero-height panels.
- **Who sees it:** a fresh install, or anyone whose saved size is about this size (`App.axaml.cs:93`-`96`).
- **The question**, for the next arbiter and Tim:

  | Option | For | Against |
  |---|---|---|
  | A. Author a unit that keeps Stop, CQ and the panels on the window at 1100 x 780 and 900 x 620, before Tim's verdict | The abort is reachable at the size Hamlet opens at; Tim reviews a window that meets R26's *at no window size* | A `src` change while Tim may be reviewing, which ruling 47 held off |
  | B. Raise the opening size and minimum to sizes that measure whole | A small change | 1536 x 824 still misses R26 here, and 1400 x 1040 is taller than a maximized 1366 x 768 laptop, so this hides the fault at a size Tim can still drag to |
  | C. Leave it to Tim's verdict at his own size | No work now | Tim may give the verdict on a window whose abort is off-screen at first launch |

  **The industry-standard answer is A.** A stop control that can be laid out off the window at the product's
  own default size is a safety defect, not a styling one. Tim rules.

**2. R26 misses at every listed size under 1040 tall.**

*A finding.* The top row over 0.262 of below the pills and the three panels under half, FT8 [PSK31]:
- **900 x 620:** top row 285 px, 156.6 over [297, 168.6 over]; panels 0, 245 short. The green block's left column is 0 px wide:
  the band, frequency, mode, license and rule-of-thumb lines are not drawn, which §0.5's *collapsing hides detail, never
  information* would call information hidden.
- **1100 x 780:** top row 623, 452.7 over [653, 482.7]; panels 0, 325 short. The plain window: 620, panels 0.
- **1280 x 720:** 270, 115.4 over [291, 136.4]; panels 103, 192 short [82, 213].
- **1366 x 728:** 242, 85.3 over [254, 97.3]; panels 139, 160 short [127, 172].
- **1536 x 824:** 196, 14.2 over [208, 26.2]; panels 281, 66 short [269, 78].
- Rig within 0 px of the card everywhere. Holds at 1400 x 1040, 1920 x 1040, 1920 x 1017 and 2560 x 1400. On the sheet
  as items 29 to 33.

**3. Text is trimmed at the anchors too, which no earlier unit recorded.**

*A finding.* *nothing decoded yet* in the Decoded text header is trimmed to 180 of 190 px at every licensed
size, 1400 and 1920 included. On the plain window *021130 UTC · 2 shown · oldest first* is trimmed to 180 of
350. *not listening yet* in the waterfall header is trimmed at 1366, 1400 and 1536. All are measured on the host's
wide text. On the sheet as item 34.

**4. The achievements window clips 8 runs at 900 x 620 and none at 1040 x 720 or wider.**

*A finding.* The runs are named in section 3 and on the sheet as item 35. No card is white at any size. The window
declares no minimum, so this size is reachable. Its category pages scroll, so cards past the bottom edge are not a
miss.

**5. What the traces do not measure.**

*A finding.*
- The licensed window's callsigns and card: it draws no decoded row or card. They are measured only on the plain
  window at 900 x 620 and 1100 x 780, where both rows sit below a 0 px panel and so read *none clipped*.
- Whether the band pills stay put, and §0.5's collapsed summaries at small sizes.
- Whether an outer box clips text that overruns a non-clipping one. For example, the mode strip's status sentence
  runs past its `StackPanel` at every size, 1920 included.
- The screen itself: every number is the host's, whose text is about half again wider than the glass.

**6. Mismatches with work instruction 354, and the tool facts.**

*A finding, reported and not repaired.*
- **§1 and §2: `TheWorkingPanelsTests.cs:510` builds `EmptyTab`'s window**, not `Realized`'s. `Realized` builds its
  window at `:778`.
- **Ruling 76 places the achievements table in the sheet's section 3.** The sheet's section 3 is *Decided for
  you*, so the table went after 2.4 (section 1, decision 4).
- **§2's launcher files held.** `PHASE_STATUS.md` was committed whole with the launcher's `HEARTBEAT` line.
- **This unit's own citation.** `b902a297`'s message says the table is at `:84`-`102`; it is at `:84`-`100`, and the
  message cannot be amended on a pushed commit. This report cites the lines as they are.
- **The tool facts, against §7.**
  - Ran: `sh tools/status.sh` joined by `&&` to `date` and `timeout … dotnet test … | grep`;
    `git add && git commit -m -m && git push && git log | cut`; `grep -o -e`, `grep -n -o -e`, `grep -rl` and
    `wc -l` on trx and source files.
  - Asked for approval and not run: a `grep -n -o` with a `\{0,160\}` count.
  - Refused: a `for` loop over `$c` (*Contains simple_expansion*); `git show 0d69123a:output.md > testresults\…`
    (*Output redirection … was blocked*), though the path is inside the root.
  - Not tried: `pwd -W`, `sed`, `awk`, `tasklist`, `sh` on a script.
  - Status: the first write at 02:40:20 read `STATE: RUNNING` and `BALL: claude`, neither an allowed word, and every
    later write used `EXECUTING` and `code`. `tools/status.sh` still writes `HM-DEC-161 (2026-09-11)`. Every write
    was set back to `HM-DEC-163 (2026-09-12)` with the file editor, except that the 02:41:28 and 02:42:15 writes
    ran back to back without the edit between them.

```
READ IN THIS ORDER.

A. The phase goal - the screen, done right. Steps 0 to 2 done, 3 waits on Tim.
B. No criterion changes state; this unit clears the blocker under 354 item 2.
C. The report last, and section 4 raises 5 items on top of the carried queue.
```

```
UNIT:       356 - complete at task 5 of 5, none dropped - 2026-09-14 10:02
PHASE GOAL: The screen, done right.
UNIT GOAL:  Hamlet opens with every control on the window; the sentence after a
            send says what it measured and never sends Tim to the radio; no test
            constructs a network client.
ADVANCED:   no
NUMBER:     sizes with CQ on the window 8 of 9 -> 9 of 9; operator-facing strings
            naming a meter on the radio 1 -> 0
DRIFT:      carried
```

**Every appearance claim is computed, not seen**, and nothing here is evidence about the
radio: this machine has none and every ALC reading is handed in (FACT-004, FACT-006).

## 1. What Claude did

Gate passed on all five. Branch **`main`**, six commits, pushed, nothing left uncommitted.
Version **1.13.42 → 1.13.43**.

### Task 0 — the record

`PHASE_PLAN.md` gains **R28** from Tim's own words of 2026-09-14, asked which of two ways
Stop should behave and answering *"A, the way it has been"*: **Stop is always pressable and
never grey.** It builds nothing, because unit 355 already built it that way and raised the
contradiction with its own instruction as an ask. `PHASE_OUTCOME.md` gains the `UNIT 356`
entry under step 3.

### Task 1 — CQ is on the window at every size

At 1100×780, the size Hamlet opens at, **CQ was drawn at y 800 and the mode chip strip at
y 847, below a window 780 tall.** Only that one of the nine sizes was wrong.

**The cause, measured.** The top row is an `Auto` row in a grid whose working panels take
the star, so the neighborhood card could ask for any height it liked and the star row simply
went to nothing. Its body measured **583 px** at that width and the row **631**, against
198, 204, 224, 250, 278 and 293 px everywhere else.

**The rule, stated by the instruction and marked as this unit's own:** the working panels
give up height first, then the top row, and the send area is never the thing that leaves.
`MaxHeight` on the top row is how the top row gives its share up, and the card body scrolls
inside the cap so nothing but detail is hidden (§0.5).

**300 px is measured rather than chosen** — it is above every sound reading, so it is inert
at the eight sizes that were already right and binds only where the card ran away. **CQ moves
from y 800 to y 469, the tabs from 847 to 516, and the panels from 0 px to 141.**

### Task 2 — the ALC sentence

Tim's screen, after an FT8 send to HA1BF: *"this message is incorrect."* **Two faults were
named in the instruction and the trace found a third.**

1. The sentence was composed in the send's `finally`, **before** `LearnTheAlcFrom` stored the
   reading it was about, so the send that set the reference could only ever say no reference
   existed.
2. It told him to look at the ALC bar on the radio and turn a knob, which **R11 forbids
   outright**.
3. **The FT8 path composed no sentence at all.** `ReadTheAlc` had one call site, in
   `FirePsk31Async`, so an FT8 send taught the reference silently and whatever stood on the
   panel was left from some earlier PSK31 send.

**Learn, then say.** `LearnTheAlcFrom` returns whether it stored, and the FT8 path now calls
`ReadTheAlc` with that answer. Four forms, one line each, printed whole in section 3, and
**not one names a meter or the radio**.

### Task 3 — no client under test

`PotaActivitySource` and `SotaActivitySource` kept the ingredients and **make the client on
the first fetch**, the seam the license lookup took in unit 355. The handed-in-client
constructor is unchanged, `Dispose` disposes only what exists, and the lock the fetch already
holds serves the creation too.

### Task 4 — the sheet

`docs/unit349-what-tim-looks-at.md`: the 1100×780 row re-measured, **item 28 recorded as
fixed**, **item 29 rewritten rather than removed** because a cap is not a cure, and section
2.2 gains the four ALC sentences word for word.

### Three tests were holding the old behavior in place

Worth its own line, because one of them was holding a **rule-breaking** sentence:

- `TheAlcLearnsFromFt8Tests` pinned two phrases of the superseded wording. Updated to the new
  words; the claim it guards is unchanged.
- **`ThePowerIsOfferedTests` required `turn the transmit drive` in the sentence for a reading
  with no reference behind it.** That is R15 broken twice over: with no reference Hamlet
  reports and judges nothing, and an instruction to turn something down is a judgement that
  the reading is too high. Corrected.
- `TheTestsStayOffTheNetworkTests` carried a paragraph saying the no-client criterion did not
  hold and was not being made to. Replaced by the assertion.

### Nothing was recorded under §12.1

No `DECISIONS.md` entry was written. R28 is Tim's ruling transcribed into `PHASE_PLAN.md`, as
task 0 directed, not a session's own.

### Tests

**No suite was run**; every name filtered, foregrounded, 480 s timeout (HM-DEC-155). A status
line before every `dotnet` command.

| Run | Result |
| --- | --- |
| Carry-forward, app, before | **112 of 112** |
| Carry-forward, engine, before | **86 of 86** |
| `TheStopIsAlwaysOnScreenTests` extended (task 1) | **5 of 5**, watched failing 1 of 5 first |
| `TheAlcSentenceTests` (new, task 2) | **5 of 5**, watched failing 3 of 5 first |
| `TheTestsStayOffTheNetworkTests` extended (task 3) | **5 of 5**, watched failing 1 of 5 first |
| Final app run, carry-forward plus every name touched | **140 of 140** |
| Final engine run, carry-forward plus the spot sources | **102 of 102** |

**198 green before, 242 after, nothing new red.** One inherited red is named in section 4
and was proved red **before** this unit changed anything.

## 2. What the owner should expect

**The build is clean** — zero warnings, zero errors.

Open Hamlet at its usual size and the CQ button is there. It was being drawn twenty pixels
below the bottom edge of the window, along with the mode tabs, because the neighborhood card
at that width wanted six hundred pixels of height and nothing was stopping it taking them;
the top row is now capped, the card scrolls inside the cap if it needs to, and the three
working panels get back a hundred and forty pixels they never had at that size. After a send,
the line about your radio's level control now says one true thing and stops: on the send that
sets the reference it names it, on a later send it says you are inside it, and on a PSK31 send
that is too hot it points at the transmit drive on the screen. It will never again tell you
to look at a meter on the radio or turn a knob there, which is the thing it did on your screen
yesterday.

**What will look wrong and is not.** At 1100×780 the trace still prints a 623 px top row, and
it is right to: the card still asks for that much and is scrolling inside a 300 px cap rather
than having been made smaller. The panels at that size are 0.217 of the height below the pills,
which is better than the 0.000 they were and still short of R26's half.

**Pushed to `main`.**

## 3. What you should see

**The four ALC sentences, word for word:**

```
set the reference
  Your radio's level control read 62 of 120 during this send. That is Hamlet's
  reference from now on, and a PSK31 send that reads well above it will get a
  sentence here. Nothing for you to do.

inside it
  Level 58 of 120, inside the 62 Hamlet measured on a clean FT8 send. Nothing
  for you to do.

past it by the margin, on PSK31
  Your radio is holding this signal back: its level control read 90 of 120
  against the 62 Hamlet measured on a clean FT8 send, and that is what makes
  PSK31 spread into the people either side of you. Turn the transmit drive on
  this screen down one step and send again.

no reference yet
  Your radio's level control read 62 of 120 during this send. Hamlet has no
  reference to compare that with yet and is not judging it, and it takes one
  from your next FT8 or FT4 send. Nothing for you to do.
```

**What it said before, on your screen:**

```
Your radio's own level control read 62 out of 120 while that went out. Hamlet
has not yet seen an FT8 or FT4 transmission on this radio to compare it with,
so it is not judging it for you: look at the ALC bar on the radio, and if it
goes past the marked zone, turn the transmit drive above down one step and
send again.
```

**The window at 1100×780, before and after:**

```
before   DigitalSendCqButton   290,800  54 x 22   whole False
         DigitalModeChipStrip  222,847 250 x 18   whole False
         TopRow                 16,140 1068 x 631
         panels 0 px

after    DigitalSendCqButton   290,469  54 x 22   whole True
         DigitalModeChipStrip  222,516 250 x 18   whole True
         TopRow                 16,140 1068 x 300
         panels 141 px
```

**And the nine sizes, after:**

```
1920 x 1040  CQ y 367   1100 x 780  CQ y 469   1400 x 1040  CQ y 393
 900 x 620   CQ y 462   1280 x 720  CQ y 447   1920 x 1017  CQ y 367
1366 x 728   CQ y 419   1536 x 824  CQ y 373   2560 x 1400  CQ y 367

the send area is 22 px tall at every one of them, 23 at 900 x 620
```

**No client under test:**

```
after construction: POTA client made False, SOTA client made False
after one fetch:    POTA client made True
```

## 4. What's blocking us

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

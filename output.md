```
READ IN THIS ORDER.

A. The phase goal - the screen, done right. Steps 0 to 2 done (units 351,
   347, 348). Step 3 waits on Tim's verdict at his window size, on
   docs/unit349-what-tim-looks-at.md, which this unit brought up to date
   (72901b0f).
B. Step 3's ground - the sheet - and step 0's safety at small sizes; no
   criterion changes state. What this unit did for it:
   1. Stop on the window at the nine sizes: 8 of 9 -> 9 of 9, in the
      status bar; at 1100 x 780 Stop's box is 997,723 74 x 36 on a 780 px
      window (was y 800, below it). Its path to StopNow untouched.
   2. PSK31 rows kept after the carrier ends: 0 -> every row that read a
      character; four of four on the four-signal recording, 483 characters.
   3. Test view models that can reach callook.info: every one -> none.
   4. The sheet: Stop, the nine sizes and PSK31 rows re-stated from this
      session's runs.
C. The report last. Section 4 raises 9 items on top of the carried queue.
   Item 1 is a transmit-side ask: task 1 said Stop should be disabled with
   nothing keyed, and it was built pressable at every instant instead.
```

```
UNIT:       355 - complete at task 5 of 5, none dropped - 2026-09-14 09:22
PHASE GOAL: The main window laid out as Tim's approved mockup and every achievements category page as trading cards, proven by tests in steps 0 to 2, and then Tim at his own window saying it passed, which is the one step left.
UNIT GOAL:  Keep the transmit abort on the window at every size by moving Stop into the status bar without touching its path; stop a PSK31 station's words vanishing when his carrier ends; take the license lookup off the network in tests; bring the sheet Tim reads up to date with all of it.
ADVANCED:   no - step 3 still waits on Tim's verdict; no exit criterion changed state
NUMBER:     window sizes with Stop on the window 8 of 9 -> 9 of 9; PSK31 rows kept after the carrier ends 0 -> all that read a character (4 of 4 on the four-signal recording)
DRIFT:      1 consecutive unit without advance (was 0 in unit 354's report; the instruction's block says carried and gives no number)
```

## 1. What Claude did

**Complete at task 5 of 5, nothing dropped.** Machine QUIVERFULL; project Hamlet, gate passed
(`SHACK_FACTS.md` and `src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` present, `CoreHMI.sln` and `MURC.sln`
absent, root `C:\Source\HamLet`); branch `main`, each task committed and pushed. Every appearance claim is
computed on the headless host, not seen.

**Before anything changed:** carry-forward app 111 of 111, engine 86 of 86. Patch bump 1.13.41 -> 1.13.42;
`UNIT 355` appended to `PHASE_OUTCOME.md` under step 3.

### Task 1 - Stop lives in the status bar (`151a108d`)

- `DigitalStopButton` moved from the send area in the tab row to the status bar's last column
  (`MainWindow.axaml`, status bar grid now five columns). Same command, `StopSendingCommand`, same path to
  `Ft8ArmedSend.StopNow`; nothing under `ViewModels` changed for it. Same `hm-stop` styles, words, tip.
- `TheStopIsAlwaysOnScreenTests` watched red 4 of 4 (at 1100 x 780 Stop's centre was y 791 on a 780 px window, and
  a click there landed on nothing), then green:
  - in the status bar and whole on the window at unit 354's nine sizes on FT8 and PSK31, and on the plain window at
    900 x 620 and 1100 x 780 on the Digital, CW and Voice tabs;
  - 36 px tall (the bar's inside), right of the bar's centre, in no collapsible panel, the only button bound to the
    stop command, the bar 46 px with it and without it;
  - at rest at 1100 x 780: *Stop*, not live, enabled, command can execute, a hit test at its centre lands on it;
  - armed at 1100 x 780: *Stop transmitting*, 206 x 36 at 865,723; a real click un-arms, the wire takes
    `17 FF` then `1C 00 00`, the slot boundary finds nothing armed, the sink is never touched;
  - keyed at 1100 x 780: enabled, a real click puts both abort frames on the wire while the tones run, and the
    sound stops by its token.
- `TheUnslottedSendTests` and `TheFt8AndFt4SendsAreByteIdenticalTests` 11 of 11, unedited. `BindingHealthTests`
  green. `TheTopRowTests.DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop` now asserts CQ in the send
  area and Stop in the status bar.
- `TheOperatorCanStopItTests` (not on the carry-forward list, the sheet's *never run*) ran: 6 of 7 in the guard group.
  `TheStopAddedNoNewRouteToATransmission` is red and older than this unit (section 4 item 5).

### Task 2 - a PSK31 station's words stay (`66bb7b0b`)

- A retired PSK31 row that read any words is kept: `DigitalDecodeRow.Ended` set in place, *ended* drawn beside the
  station (`DecodedRowEndedWord`), `RowOpacity` 0.55. It joins the arrival order with the FT8 rows, so the order
  button keeps it and `TrimDigitalDecodes` (the cap loop that was inline in `AddDecodeRow`, line for line for a list
  with no PSK31 row) drops it at 500. Clear and the retune clear remove it. Leaving the tab ends rows too.
- A carrier back within 16 Hz and 15 s of audio resumes the same row, earlier words first, reading carried on.
- Telemetry: `psk31_row_ended` (offsetHz, characters, lines, lifetimeSeconds) and `psk31_row_cleared`
  (offsetHz, characters, reason `clear`, `retune` or `cap`). Counts and a frequency, never words.
- `ThePsk31RowStaysTests` watched red 4 of 4, then green: the four-signal recording leaves four ended rows holding
  all their text, 120 + 101 + 120 + 142 = 483 characters, with four `psk31_row_ended` events (lifetimes 32 to 57.3 s);
  Clear removes two ended rows with two `clear` events; a return at 1093 Hz resumes the 1085 Hz row and a station at
  1600 Hz is a new row; the order button keeps an ended row and 500 FT8 rows drop it with one `cap` event and
  *oldest 1 dropped*.
- `TheRowShowsWhatWasHeardTests`, `ThePsk31StationIdlesTests` (app and engine) and `ThePsk31CarrierLivesTests`
  green, unedited.

### Task 3 - an ended row is still a station (`1a0d5c99`)

- The ended row keeps its reading; `RefreshPsk31Card` falls back to it under a key below nought (no channel id is),
  and `RememberWhatWeSent` counts his side from it. No send path changed: `AnswerPsk31` goes through `SendMessage`.
- `ThePsk31ExchangeTests.ClickingAnEndedCqRowStillAnswersHimAndOpensHisCard` watched red (the Answer went, no card
  opened), then red again with the card at *Unknown* (the Answer had merged before his CQ), then green: station,
  country and fade kept; one transmission; the Answer macro; `psk31_send_composed` at 1234 Hz; one W1AW card at
  *His turn*. The class 4 of 4.

### Task 4 - the tests stay off the network (`b5be0db9`)

- The seam is the existing `ICallsignLookup`. `MainWindowViewModel` gained a three-argument constructor; the
  two-argument one the app calls takes `DefaultLicenseLookup`, the live client (`LiveCallookLookup`, one polite
  `CallookCallsignLookup` per request, as `ResolveProfileAsync` built inline). The test assembly's module
  initializer sets the default to `NetworkDeniedLookup` (a transport failure, nothing sent);
  `TheWorkingPanelsTests.Realized` and `TheTopRowTests.Realized` hand in `FixedLicenseLookup`, General for KC3QIS.
- `TheTestsStayOffTheNetworkTests` watched red 4 of 4 on each test's first line (the default was
  `LiveCallookLookup`; no view model built), then green: a two-argument view model under test has the network
  denied; a handed-in denied lookup is asked once and the class stays Unknown; the plain fixture asks the fixed
  answer once and draws General (*7.028 MHz · yours to use*); the licensed fixture takes it too; plain 900 x 620 and
  1100 x 780 and licensed 1400 x 1040 read the same boxes twice running (1400: top row 216, panels 424 each).
- Not asserted: *no HTTP client is created under test* does not hold (section 4 item 2).

### Task 5 - the sheet matches the tree (`72901b0f`)

`docs/unit349-what-tim-looks-at.md`: the opening says what unit 355 changed; section 1 a bullet; 2.1 CQ and Stop
split, a table of Stop at the nine sizes; the sizes table's 900 x 620 and 1100 x 780 rows; 2.2 PSK31 rows stay;
3.1 ruling 5; section 4 item 28 now about CQ only. Numbers from this session: `TheStopIsAlwaysOnScreenTests` and
`Unit354TraceTheMainWindowAtTheSizesTimCanOpen` at `b5be0db9`, `ThePsk31RowStaysTests` at `66bb7b0b`,
`ThePsk31ExchangeTests` at `1a0d5c99`. The sheet has no check of its own; nothing in `tests` or `tools` reads it.

### After

App: the carry-forward list with this unit's four new or extended classes, `TheWorkingPanelsTests`,
`TheTopRowTests`, `LicenseResolverTests`, `Unit302CardNameTests`, `HowFastTheDecodedTableGrowsTests` and
`TheDecodedOrderAndClearTests`, 170 of 170 at `b5be0db9`. Engine: no file under `src\Hamlet.RadioEngine` or its
tests changed since the seed (`git diff --stat 4ac119ef`, empty), so its 86 of 86 was not re-run.

### Existing tests this unit edited

- `TheTopRowTests`: the send-area assertion now puts Stop in the status bar; `Realized` hands in the fixed answer.
- `TheWorkingPanelsTests.Realized`: hands in the fixed answer.
- `ThePsk31HearsEveryoneTests.Watch`: skips ended rows, so a carrier is gone when its row ends. The bound is unchanged.
- `ThePsk31ReadsTheConversationTests.TheCodeTheseRowsReuseIsUnchanged`: the `RowOpacity` pin moved to the new line,
  as unit 324 moved it.
- `ThePsk31ExchangeTests`: one test added; `Panel` takes an optional telemetry.

### Decisions made for this unit

1. **Stop is pressable at every instant, not disabled with nothing keyed.** Task 1's clause was not built:
   `docs/phase-send/PHASE_PLAN.md` step 1, *it cannot be disabled, deferred, or made conditional* (must-pass),
   `TheOperatorCanStopItTests.TheStopIsOnScreenAndPressableBeforeAnythingHappens` guards it, and a press before the
   slot boundary, when nothing is keyed, is how an armed send is taken off. Section 4 item 1.
2. **Stop is the status bar's last column**, so it never moves when the contact count appears or goes.
3. **Stop shows on every tab**, since the bar is the window's.
4. **A PSK31 row that never read a character still goes with its carrier.** *heard, not readable yet* is Hamlet's
   sentence; `ThePsk31CarrierLivesTests.AHeardRowGoesWhenItsCarrierGoes` stays green unedited.
5. **Leaving the PSK31 tab ends rows rather than removing them**; only a clear, a retune or the cap removes them.
6. **The resume window is 16 Hz and 15 s of audio** (`Psk31ResumeWithinHz`, `Psk31ResumeWithinSeconds`).
7. **Every PSK31 row now joins the arrival order**, so the order button keeps live rows too, where it used to drop
   them; the cap passes over a row with a carrier still under it.
8. **`psk31_row_cleared` carries a reason**, `clear`, `retune` or `cap`.
9. **Both layout fixtures get the fixed answer**, though the instruction named the plain one: the licensed fixture's
   resolve also asked callook.info.
10. **"The network denied" is a refusing lookup set by a module initializer**, not an operating-system block.
11. **The two-run assertion realizes the windows twice in one test**, not two test runs.
12. **The four-signal test leaves the tab after 20 s of faint noise**: one carrier is still held (section 4 item 7).
13. **Unit 354's section 4 is carried in part verbatim and in part by reference.** The seed deleted `output.md`;
    `git restore --source` needed approval and `git show … > output.md` was blocked, so the file could not be
    kept in place. Unit 354's own opening and its *Raised by unit 354* are below verbatim; the queue it carried
    from units 337 to 353 is at `4c55deac:output.md`, lines 280 to 2341, not retyped.

### Commits

`151a108d` task 1 · `66bb7b0b` task 2 · `1a0d5c99` task 3 · `b5be0db9` task 4 · `72901b0f` task 5 · this report
with `PHASE_OUTCOME.md` and `PROJECT_STATUS.md`.

## 2. What the owner should expect

- **Stop is at the bottom right of the window, in the status bar**, on every tab and at every size. It says
  *Stop* with nothing going and *Stop transmitting* while a send waits for its slot or is on the air, and it is
  never grey. Pressing it does exactly what the old Stop did. Escape still stops the calling cycle and the scan, as
  before, and not this Stop.
- **CQ has not moved.** At 1100 x 780, the size Hamlet opens at, CQ and the mode tabs are still below the window's
  bottom edge.
- **A PSK31 station's words stay after he stops**, faded, with *ended* beside him, until you press Clear, retune far
  enough to clear the list, or 500 newer rows push them off. A row that only ever said *heard, not readable yet*
  still disappears with its carrier.

**What will look wrong but is not:**
- A faded PSK31 row is not a worked station; the word *ended* says which.
- Choosing FT8 no longer empties the PSK31 rows off the list.
- A station who drops his carrier for a moment and comes back near the same frequency continues his row rather
  than starting a new one.
- An ended CQ row still offers *Answer*, and pressing it transmits. He may be listening; he may have gone.
- Tests: nothing to see. They no longer ask callook.info anything.

## 3. What you should see

**Stop is on the window at 9 of 9 sizes, where it was off at the size Hamlet opens at, and a PSK31 station's words
stay on the list after he stops - four of four rows with all 483 characters on the test recording.** Computed on the
host, not seen.

- Open Hamlet at its first size: Stop sits at the right end of the grey bar along the bottom, 74 x 36.
- Press CQ on PSK31 or FT8 and watch it read *Stop transmitting*, wider and with a heavier edge.
- Listen to PSK31 until a station stops: his line stays, softer, with *ended*. Press Clear and it goes.
- No visible change from task 4.

## 4. What's blocking us

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

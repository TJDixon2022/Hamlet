READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Step 0 done. Step 1 **done**, five
   units spent, 1.1, 1.3 and 1.4 banked by unit 373. Step 2 **partial**. Steps 3, 4,
   5 not started.
B. Step 1 - Hamlet opens whole. 1.2 **met**: the sweep now runs
   **920** to 620 and TheWorkingPanelsLoseHeightBeforeTheTopRowLosesAny counts
   **4** shrinks where it counted 0, so the type is **4** of 4; 1.1, 1.3 and 1.4
   carried met and re-checked **green**. Then step 2 - 2.1 **met**: under PSK31,
   cq_pressed writes detail **PSK31** where it wrote Ft8.
C. The report last. Section 4 raises **3** items on top of the carried twelve, and
   **none of them is** in the way of a criterion in B - they are two findings and a
   note, none wanting a ruling. **Step 1 closed and step 2 opened.**

```
UNIT:       374 - complete at task 4 of 5 - 2026-09-20 22:29
PHASE GOAL: Harden what Hamlet already has. Five steps of screen, record and test
            work that needs neither the radio switched on nor the owner at the desk,
            run unattended while he is away, and judged at the end by him at his
            window.
UNIT GOAL:  Close step 1 by measuring the order of surrender where height is really
            surrendered - above the floor unit 373 built, not in the flat band below
            it - and then open step 2 by making the record name the sub-mode the
            press was made under instead of the two-member family it maps to.
ADVANCED:   step 1, criterion 1.2 - which closes step 1 - and step 2, criterion 2.1
NUMBER:     the shrinks the guard counts in its sweep: 0 -> 4
DRIFT:      none
```

## 1. What Claude did

**Complete, at task 4 of 5 (tasks 0 to 4). Nothing was dropped.** Windows 11, project gate
`PROJECT: Hamlet` verified against the tree - `SHACK_FACTS.md` and
`src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` present, `CoreHMI.sln` and `MURC.sln` absent,
root `C:\Source\HamLet` - branch `main`, pushed.

**Task 3 was the named drop candidate and it was not dropped**, because task 2 left step 1 closed,
which is the fence the instruction put on it.

**The carry-forward list was run from the file's own two command lines, never from
`tools/run-carry-forward.sh`** (§1), and **that script was not repaired** - it is still not the
carry-forward list and this unit did not touch it.

**Entry, both invocations, one build each, status written immediately before each: app 206 of 207,
engine 146 of 146.** Two things about the app count. Its **total** is 207 rather than unit 373's 206
because unit 373 added a name to the line after its own exit run, so one more name matches the
filter; that is arithmetic, not a finding. Its **one red** is Avalonia's headless
`InvalidProgramException: You've caused dispatcher loop` after 1 ms, which is the test-session fault
§5 named - **the invocation was re-run, as instructed**, and the fault moved: two names on the first
attempt (`ThePsk31OfferTests.TheOfferIsOneButtonAndItIsTheOneTheEngineNamed` and
`ThePowerIsOfferedTests.TheOfferRendersAtHalfAndNothingMirrorsTheUsbModLevel`), a different single
name on the re-run. A fault that names a different test each time is the session and not the test.

**Exit, after the last change: app 207 of 207, engine 146 of 146. No regression, and the app run came
back cleaner than it went in** - the session fault did not recur in either exit invocation.

**Task 1 built nothing.** `Unit374TraceTests`, one type, asserting nothing about behavior. It
measured the height ladder at widths 1100 and 900 from 1040 to 620 in steps of 40, on both of the
type's windows, and it drove a real view model through the chip press on each of the strip's four
labels and read the record back off the file. **The ladder is what chose the sweep**, and the record
trace is what let §6's second ruling be applied to a measurement instead of to a reading of the file.

**Task 2 is criterion 1.2 and it closed step 1, in its own commit under §R12.** The sweep went
`780, 740, 700, 660, 620` to `920, 880, 840, 800, 780, 740, 700, 660, 620`. **Nothing was loosened
and nothing was removed**: every height from 780 to 620 is still swept, `shrinks.Count >= 2` stands
at its threshold, the 0.5 px slack is untouched, and everything `shrinks.Take(2)` asserted it still
asserts. **One assertion was added** - below the floor the panel row is constant, `TopRow` still
gives up none, and the canvas scrolls instead. The type's own remarks were rewritten to say what the
numbers are now and why the sweep starts where it starts. **4 of 4**, and the rest of step 1
re-checked at **33 of 33**.

**Task 3 is criterion 2.1.** The four operator-action sites carry `ChosenDigitalMode`, or
`StartupSnapshot.Unknown` where nothing has been pressed. **The send events were measured per path
and changed nowhere**, which §R14 asks for explicitly: under PSK31 and Olivia the press leaves by
`SendUnslotted` above the three slotted writes, and that path already recorded `Psk31` and `Olivia`
by name. `DigitalDecoderStarted` keeps `_digitalMode` per §6's ruling and now says why in the file.
`parts.AppDigitalMode` is the startup snapshot and was left. One test type is the evidence,
**12 of 12**. **`Unit305ActionTests`, `Unit305HonestTests`, `CallsignPrivacyTests` and
`ThePsk31SeamTests` were all read and run, and none of them had to change** - so there is no second
§R12 commit at this task. **Not one sample of composed audio, no `Arm` site and no `PttOn` site was
touched.**

**The application was shown unchanged where it had to be.** `git worktree add` is refused here, so
the same thing was established the way unit 373 did it: `git diff 47367dad -- src/` (task 0's commit)
**read empty** across the whole of tasks 1 and 2. The only `src/` change in the unit is task 3's, and
it is four `detail` arguments, one new private property and one comment.

**One name was added to `docs\carry-forward-tests.txt`** and the app line was then **re-run as
amended**, 211 of 211, so the list is proved green as it is written rather than as it was before the
edit.

**Decisions this session made for itself, reproduced in full.** *The top of the sweep is 920.* §6's
first ruling said to measure where the panel row stops growing and put at least two transitions above
it, and not to guess the top. The ladder says it stops growing at 780 at width 1100. A sweep topped
at 840 gives exactly two shrinks, which meets the precondition **on the edge**; 920 gives four. **920
was taken for the margin**, because a sweep that meets its own precondition by exactly nothing is the
shape that broke this test in the first place. *The flat band is asserted from 780 and the scroll
assertion starts below it.* At 780 the canvas viewport equals its extent exactly - 185 and 185 - so
nothing has to be scrolling there yet; the constancy of the panel row and of `TopRow` is asserted at
780 and at every height below it, and the scrolling is asserted only where the canvas actually
scrolls. *The added assertion went inside the existing name rather than into a fifth name*, so the
type is 4 of 4 and not 5 of 5, which is what B's line is counted against. *One name went on the
carry-forward list.* §4's default is to add nothing; the reasoning for departing from it is in
section 4 and in the file's own paragraph.

**What the reload measured and this unit did not repair**, as §5 asks: `PROJECT_STATUS.md`'s
`RULES_AT` reads `HM-DEC-165 (2026-09-19)` while `CLAUDE.md` §1 holds `CPS-DEC-0165` - the id-scheme
split carried in `PHASE_PLAN.md` §7, **not repaired**. `SESSION.lock` and the files under
`.run-unit\` are the harness's and were **not restored, deleted or committed**.

## 2. What the owner should expect

Two things changed, and only one of them is anything you can see. **The rule that Hamlet gives up its
panels before it gives up the row with the CQ button in it is now checked across the whole range of
window heights you can drag to, instead of only at the short end.** That matters because unit 373's
repair - the one that stopped the three working panels vanishing at small sizes - also meant that at
the short end nothing shrinks at all any more, so the test that was supposed to watch the order of
surrender had run out of window to watch it in; the fix was to look where height actually is given
up, which is at the tall end, and to add a new check that says the short end is *supposed* to be flat
and that the workspace scrolls there instead. **Nothing about the window itself changed** - this unit
changed a test and not the layout, and the panel heights at all nine of the sizes measured back in
unit 354 come out exactly as they did after unit 373: 452, 73, 73, 90, 90, 230, 426, 429 and 829
pixels. Every appearance claim here is **computed, not seen** (FACT-004): these are measurements off
a headless window, not a look at a screen.

**And when you press PSK31 and call CQ, the diagnostic file now says PSK31 rather than FT8.** It said
FT8 before because the code that wrote that line was using a two-value setting - FT8 or FT4 - that
everything else in the digital tab correctly derives from, and PSK31 and Olivia both map onto FT8 in
it; that is right for choosing a grid and a decoder and wrong as a name for what you pressed. **This
matters to you only when something goes wrong and you send the file back** - but that is exactly when
it matters, because an evening on PSK31 and an evening on FT8 read as the same evening. One line in
that file still says FT8 under PSK31 on purpose: the one that records **which decoder started**,
because under PSK31 the decoder that starts genuinely is FT8's, and changing it would put a lie in
the file to take one out. **Nothing in this unit transmits**, nothing was armed or keyed, and not one
sample of composed audio changed.

## 3. What you should see

### The height ladder - task 1, and it is what says the order is a fact about Hamlet

Widths 1100 and 900, every height from 1040 to 620 in steps of 40, on both of the type's windows.
Columns: the height asked for and the height drawn, `TopRow`, the panel row, the three panels
together, the send area, and `WorkspaceCanvasScroller`'s viewport and extent. **This table comes
before any prose about 1.2 because it is what shows the old sweep's red was an artifact of where that
sweep happened to stop, and not a broken rule.**

**Width 1100, the pinned-facts window**

| asked | drawn | TopRow | panel row | panels | send | viewport / extent |
|---|---|---|---|---|---|---|
| 1040 | 1040 | 300 | 331 | 993 | 22 | 445 / 445 |
| 1000 | 1000 | 300 | 291 | 873 | 22 | 405 / 405 |
| 960 | 960 | 300 | 251 | 753 | 22 | 365 / 365 |
| 920 | 920 | 300 | 211 | 633 | 22 | 325 / 325 |
| 880 | 880 | 300 | 171 | 513 | 22 | 285 / 285 |
| 840 | 840 | 300 | 131 | 393 | 22 | 245 / 245 |
| 800 | 800 | 300 | 91 | 273 | 22 | 205 / 205 |
| **780** | 780 | 300 | **71** | 213 | 22 | 185 / 185 |
| 740 | 740 | 300 | 71 | 213 | 22 | 145 / 185 |
| 700 | 700 | 300 | 71 | 213 | 22 | 105 / 185 |
| 660 | 660 | 300 | 71 | 213 | 22 | 65 / 185 |
| 620 | 620 | 300 | 71 | 213 | 22 | 25 / 185 |

**The panel row stops growing at 780. Seven shrink transitions above it, none below.**

**Width 1100, the window with content** - the same shape 2 px taller in the panel row at every
height, which is the card's own chrome: 333, 293, 253, 213, 173, 133, 93, **73**, 73, 73, 73, 73;
panels 999, 879, 759, 639, 519, 399, 279, 219, 219, 219, 219, 219; `TopRow` 300 and the send area 22
at all twelve; the same viewports and extents. **Binds at 780. Seven above, none below.**

**Width 900, the pinned-facts window**

| asked | TopRow | panel row | panels | send | viewport / extent |
|---|---|---|---|---|---|
| 1040 | 293 | 338 | 1014 | 23 | 452 / 452 |
| 1000 | 293 | 298 | 894 | 23 | 412 / 412 |
| 960 | 293 | 258 | 774 | 23 | 372 / 372 |
| 920 | 293 | 218 | 654 | 23 | 332 / 332 |
| 880 | 293 | 178 | 534 | 23 | 292 / 292 |
| 840 | 293 | 138 | 414 | 23 | 252 / 252 |
| 800 | 293 | 98 | 294 | 23 | 212 / 212 |
| 780 | 293 | 78 | 234 | 23 | 192 / 192 |
| **740** | 293 | **71** | 213 | 23 | 152 / 185 |
| 700 | 293 | 71 | 213 | 23 | 112 / 185 |
| 660 | 293 | 71 | 213 | 23 | 72 / 185 |
| 620 | 293 | 71 | 213 | 23 | 32 / 185 |

**Binds at 740 - one step lower than at 1100. Eight shrink transitions above it, none below.** The
window-with-content ladder at 900 is the same 2 px taller: 340, 300, 260, 220, 180, 140, 100, 80,
**73**, 73, 73, 73.

**And which top the sweep needs, measured at width 1100 rather than chosen:**

| a sweep topped at | heights | shrinks, pinned-facts | shrinks, with content | meets `>= 2` |
|---|---|---|---|---|
| 1040 | 12 | 7 | 7 | yes |
| 1000 | 11 | 6 | 6 | yes |
| 960 | 10 | 5 | 5 | yes |
| **920** | **9** | **4** | **4** | **yes - taken** |
| 880 | 8 | 3 | 3 | yes |
| 840 | 7 | 2 | 2 | yes, on the edge |
| 800 | 6 | 1 | 1 | no |

### The four names, before and after

| name | before | after |
|---|---|---|
| `TheSendAreaIsTheSameHeightAtEveryHeightInTheSweep` | green, 5 heights x 2 windows = 10 measurements | **green, 9 x 2 = 18.** The send area is 22 px at every one, including six heights taller than the old sweep ever reached |
| `TheWorkingPanelsLoseHeightBeforeTheTopRowLosesAny` | **RED** on `shrinks.Count >= 2` - 0 shrinks in the sweep | **green.** 4 shrinks on each window; the first two asserted panels-smaller and `TopRow`-unchanged as before; **plus 5 heights of the new flat-band assertion per window** |
| `TheTopRowGivesUpItsShareOnlyAfterThePanelsHaveAndNeverExceedsItsCap` | green, 10 measurements | **green, 18.** `TopRow` is 300 - its cap - at every height from 1040 to 620 and never past it |
| `AtEveryHeightInTheSweepTheSendAreaAndStopAreWholeOnTheWindow` | green, 5 x 2 x 5 controls = 50 | **green, 9 x 2 x 5 = 90.** CQ, the mode tabs, the reserved send area, the drive note and Stop are all whole and visible at every height, tall ones included |

**3 of 4 -> 4 of 4.** **No tall-window finding**: §6 clause 5 said that if the send area were not
22 px at some tall height, or Stop left the window there, that would be a real fault to report - it
is not, and nothing was widened or narrowed to make it so.

**The assertion that is new**, in full: at 780 and at every height below it, the panel row equals
what it is at 780 (71 px pinned-facts, 73 px with content) and `TopRow` equals what it is at 780
(300); and at every height **strictly** below 780, the canvas reports an extent greater than its
viewport - 185 in 145, 185 in 105, 185 in 65, 185 in 25 - so the panels stop surrendering height at
the floor and the canvas scrolls in their place. **That asserts more than the type asserted before,
not less.**

### The rest of step 1, re-checked and not assumed

| type | criterion | count |
|---|---|---|
| `TheWindowHoldsBelowItsMinimumTests` | 1.3 | **3 of 3**, and **unmodified** |
| `BindingHealthTests` | 1.4 | 1 of 1 |
| `TheTopRowTests` | 1.4 | 15 of 15 |
| `TheWorkingPanelsTests` | 1.4 | 8 of 8 |
| `TheStopIsAlwaysOnScreenTests` | 1.1, 1.4 | 5 of 5 |
| | | **33 of 33 in one filtered run** |

### The nine sizes, beside unit 373's row

| size | 372 | 373 | **374** | canvas |
|---|---|---|---|---|
| 1920 x 1040 | 450 | 452 | **452** | 547 / 547, fits |
| 900 x 620 | 0 | 73 | **73** | 32 / 185, scrolls |
| 1100 x 780 | 71 | 73 | **73** | 185 / 185, fits |
| 1280 x 720 | 50 | 90 | **90** | 157 / 185, scrolls |
| 1366 x 728 | 86 | 90 | **90** | 183 / 185, scrolls |
| 1536 x 824 | 228 | 230 | **230** | 325 / 325, fits |
| 1400 x 1040 | 424 | 426 | **426** | 521 / 521, fits |
| 1920 x 1017 | 427 | 429 | **429** | 524 / 524, fits |
| 2560 x 1400 | 827 | 829 | **829** | 907 / 907, fits |

**Identical to unit 373's, to the pixel.** The send area is 22 px at eight of the nine and 23 px at
900 x 620, as it was. **The layout did not move.**

### What each of the four labels writes in `detail`, before and after

| label pressed | `cq_pressed` before | after | the other three actions before | after |
|---|---|---|---|---|
| **PSK31** | `Ft8` | **`PSK31`** | `Ft8` | **`PSK31`** |
| **Olivia** | `Ft8` | **`Olivia`** | `Ft8` | **`Olivia`** |
| **FT8** | `Ft8` | **`FT8`** | `Ft8` | **`FT8`** |
| **FT4** | `Ft4` | **`FT4`** | `Ft4` | **`FT4`** |
| *nothing chosen* | `Ft8` | **`unknown`** | - | - |

**The one sentence the instruction asked for: under PSK31, `cq_pressed` wrote `detail: Ft8` - §R35
reproduced exactly, as a measurement and not a recollection.** The `mode` field was and remains
`Digital`, which is the operating mode and a different question.

**The send events, per path, measured.** Under PSK31 the file holds, in order: `state_changed`
(mode `PSK31`), `operator_action` `cq_pressed`, `operator_action` `send_requested`,
`psk31_send_composed`, `send_stage` composed **detail `Psk31`**, `send_stage` armed detail `now`,
`psk31_send_alc`, `psk31_send_keyed`, `psk31_send_unkeyed`, `rsid_sent` mode `BPSK31`,
`psk31_radio_after_send`. Under Olivia the same list with mode `Olivia` and `olivia` on the composed,
keyed and unkeyed lines, `send_stage` composed **detail `Olivia`**, and `rsid_sent` mode `OLIVIA`.
**`send_refused_after_read_back` never appears under either**, and neither does the slotted
`send_stage` write, because line 15675 sends both modes down `SendUnslotted` and returns above them.
**So the three slotted sites (15703, 15740, 15791) are unreachable under the two labels that map to
the wrong family, and under FT8 and FT4 the family *is* the sub-mode. They needed no change and none
was made** (§R14).

### The criterion's evidence, and the four readers

`TheRecordNamesTheSubModePressedTests`, one type in `tests\Hamlet.App.Tests\Telemetry\`, **12 of 12**:
the CQ press writes the pressed label on each of the four (4 cases); no operator action names the
mapped family under a mode that is not FT8, over CQ, answer, card-open and typed (4 cases); the send
path names the mode the send was made in, under PSK31 and Olivia (2 cases); the decoder line still
says `Ft8` beside a press that says `PSK31`; and nothing chosen says `unknown`. **Every name sweeps
the whole file** for the operator's callsign, the other station's, the grid and the words that were
typed, and finds none of them (HM-DEC-018 §2.1).

**`Unit305ActionTests`, `Unit305HonestTests`, `CallsignPrivacyTests` and `ThePsk31SeamTests` were
read and run, and none of them had to change.** None asserted `detail: Ft8`: unit 305's two names
assert that the actions are *present* and in order, `CallsignPrivacyTests` sweeps for personal data
and a mode label is not personal, and `ThePsk31SeamTests` asserts that a mode press writes no
`cq_pressed` at all. With `TheSendReachesTheAirTests`, `TheOliviaSendTests`, `TheTypedLineGoesOutTests`
and the two PSK31 telemetry types, **70 of 70 green**.

### The carry-forward counts

| | app | engine |
|---|---|---|
| **entry, before any change** | 206 of 207 (one headless session fault; re-run, fault moved) | 146 of 146 |
| **exit, after the last change** | **207 of 207** | **146 of 146** |
| **exit, with the added name** | **211 of 211** in 2 m 18 s | - |

**Name for name against the entry run there is no regression, and one red became green** - the entry
run's only red was the session fault and it did not recur. **The two session faults, counted as §5
asked:** the dispatcher-loop fault hit 2 of 4 full app invocations, naming two tests once and a
different single test once, and neither exit run;
`TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns` **flaked 0
of 5 executions this session**, against 1 of 4 for unit 373 and 3 of 7 for unit 372. **Neither was
chased** - 2.3 is parked.

## 4. What's blocking us

**Nothing is blocking us, and that is a real answer. Step 1 closed and step 2 opened.** Three items
from this unit, none of them wanting a ruling and none of them in the way of a criterion, then the
carried queue.

### Raised by this unit

**1. The app carry-forward invocation's total is 207, not the 206 unit 373 reported.**

*A finding, and it resolves itself.* Unit 373 added
`TheWindowHoldsBelowItsMinimumTests.TheWorkingPanelsScrollInsideThemselvesRatherThanCollapsing` to
the app command line **after** its own exit run, and reported "206 of 206 before this unit and 206 of
206 after, with this name then added on top". So the line as it stands matches one more name than the
line that produced that figure. **Nothing is wrong**; the next unit should expect 211 now, which is
207 plus this unit's four added theory cases. Raised so that nobody reads a rising count as a
regression.

**2. Unit 373's carry-forward name was on the command line but not in the file's own project list.**

*A note, and this unit acted on it.* `docs\carry-forward-tests.txt` carries both a runnable command
line and a human-readable list of names by project underneath. Unit 373's name was added to the
command line and never to the list, so the file disagreed with itself - the same class of fault the
file's own "WHERE EACH NAME LIVES IS NOW CHECKED" paragraph exists to prevent. **It was written into
the list, which changes nothing that runs.** Reported because it is somebody else's entry.

**3. `tools/run-carry-forward.sh` is still not the carry-forward list, and was not repaired here.**

*A finding, carried forward from unit 373's item 4 as an observation rather than a request.* §1 of
this unit's instruction absorbed that item as an instruction and said the script is not this unit's
to repair, and **it was not**: both invocations were run from `docs\carry-forward-tests.txt`'s own
two command lines, and the name added at task 4 went into that file and not into the script.
Restated once here so it does not fall out of the record, since nothing has fixed it for several
units now.

### Asks still outstanding - the carried queue, per HM-DEC-139

**Unit 373's item 1 was answered in work instruction 374 §6's first ruling** - the sweep is extended
upward rather than the floor retuned - **and its items 3 and 4 were absorbed into this instruction's
§5 and §1** - the corrected 125 px of chrome, and the instruction to run the list from the list.
**Twelve remain, verbatim below, and this unit answers none of them.**

#### Carried from unit 373's section 4

**2. `ApplyBestBet` leaves a stale `BestBetLabel` on every band it un-badges.**

*A finding, and it is not a defect the operator can see.* `MainWindowViewModel.ApplyBestBet` writes
`BestBetLabel` and `BestBetTooltip` only inside `if (isBest)`, so a band that *was* the best bet keeps
the words it earned after the badge moves away. It is invisible in the application, because the badge
`Border` is bound to `IsBestBet` and is not drawn - HM-DEC-046 is not broken on screen. **It matters
because it made a test turn on the wall clock**: the fixture sets `IsBestBet` by hand without going
through the ranking, and a band carrying a stale label then wears it. *Not repaired* - it is not this
unit's criterion, and §R14 says a unit does not fix doors it is not building. The test no longer
depends on it either way.

#### Carried from unit 372's section 4

**4. Step 2's flake is measured, named, and not chased.**

*A finding, recorded for the unit that does step 2.*
`TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns` was red
in **three of seven** runs of its type and **two of seven** runs of the name alone. It is the only
one of the type's five names that was ever red, and it was green in both carry-forward runs. It
always fails the same way: the abort pair is on the wire **twice** - expected key-on, abort, PTT off
and got those plus a second abort and a second PTT off. **The numbers are in
`docs\unit372-flake-measurement.md`** rather than only here, because this file is overwritten and
step 2 would otherwise inherit a rumor. Which of the click's own `StopNow` and the sequence's unkey
wrote the second pair **was not determined**, because the instruction said record and do not chase,
and choosing between them would be a guess presented as a finding (§R14).

**7. The live `PHASE_PLAN.md`'s criterion boxes are never ticked, by any unit.**

*A finding, reported and not repaired.* All four of step 1's boxes are still `- [ ]`, and so are all
five of step 0's, which unit 369 met four of. The state that is actually maintained is in
`PHASE_STATUS.md` and `PHASE_OUTCOME.md`, and this unit followed that convention rather than
starting a second record. **It is the same drift unit 369 raised about the archived Olivia plan**,
one phase earlier. *Ruling wanted only if the boxes are meant to be the record* - if they are, a
unit should be told to tick them.

#### Carried from unit 371's section 4

**2. A guessed answer with no speaker still opens no card.**

*A finding.* The card is keyed by station, so a parse Hamlet cannot put a name to opens nothing -
its row is on his side and marked a guess, which is where it was before. In the record, the parser
named the speaker; had it not, the card would still not appear.

**3. The turn indicator keeps its words rather than a question mark.**

*A finding, the author's call under the decision block.* The card says *Your turn, a guess*; the
order suggested *his turn?*. In grayscale a word survives and a question mark is easy to miss
(§0.6), and the word was already in the tree from unit 319.

**4. Six tests asserted the shut door, two of them on the carry-forward list.**

*A finding about coverage, not a defect.* The door was guarded in six places, which is why the
middle carry-forward run was 204 of 206. All six now guard the rule, each in a §R12 commit.

**5. `psk31_answer_taken` fires once per station per session, not per line.**

*A finding.* A station who answers, goes, and answers again writes one line. The card's own
history is what carries the rest, and `psk31_line_parsed` already writes every line.

**6. Python runs here, contrary to the order's tool facts.**

*A mismatch, reported for the next order.* Scripts written to the scratchpad and run as
`python file.py` worked throughout, as in units 361 and 362.

#### Carried from unit 369's section 4

**2. The `no_transmit_device` refusal was never wrong about the device - the
refusal Tim actually saw was `transmit_device_would_not_open`, and the split above
assumes his device id was still in the file at that moment.** If instead the file
had been reset to defaults, he would have seen `no_transmit_device`, and unit 362's
report says he saw the other. *That means the device id survived and the device
genuinely would not open* - which is a different fault from the settings loss, and
this unit repaired both without proving which one he hit. *Ruling wanted:* whether
that matters enough to chase. His telemetry from 2026-09-14 to 09-19 would settle
it in one read; nothing in this repository has it.

**3. `LearnedAlcReference.Ago()` counts only in seconds and whole minutes.** Now
that the reference survives a restart, a legitimate value is *4320 minutes ago*.
Honest but poor. *Ruling wanted:* whether to extend it to hours and days. *Not done
here* because `TheAlcSentenceTests` asserts the current forms and §10 said not to
touch wording beyond the refusal's.

**4. The archived Olivia plan's checkboxes say 25 of 40, not 38 of 40.**
`docs/phase-olivia-run/PHASE_PLAN.md` has 25 boxes ticked and 15 open, including
several the unit reports say were met (1.5, 4.1-4.8, 5.1-5.3). HM-DEC-166 records
38 of 40 as ruled, which comes from the reports. *Reported, not repaired* - §5 says
repair nothing but this unit's, and an archived phase's plan is not this unit's.

**5. No settings file in the tree was ever reconstructed before this unit, and the
three fixtures are mine.** They carry the right key set, but their *values* are
invented - nobody's real 1.13.30 file was available. They prove the shape loads,
not that Tim's particular file does. *Raised once, not a blocker.*

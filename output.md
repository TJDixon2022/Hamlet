```
READ IN THIS ORDER.

A. The phase goal - Hamlet works Olivia the way it works PSK31. Step 0 met on all
   six criteria (computed), steps 1-6 not started.
B. Step 0's criteria 0.1 to 0.6 - all six met, each with its number below.
C. The report last, and section 4 raises 5 items on top of the carried queue.
   Item 1 bears on A and B: where the dial goes for "the calling center".
```

```
UNIT:       358 - complete at task 6 of 6 (tasks 0 to 5), none dropped - 2026-09-14 11:43
PHASE GOAL: Olivia on Hamlet end to end, the way PSK31 is - hear it, read it, answer
            it, log it - with the variant taken from the signal's own RSID and
            never picked by the operator.
UNIT GOAL:  Make Olivia exist everywhere a mode exists - the tab, the cited
            calling spot, the panel, the Capture button, the RSID and calling data
            read from the tree - with nothing decoding and nothing able to send.
ADVANCED:   yes - step 0's six criteria met, each by a test watched red first
NUMBER:     fixtures hashed 0 -> 9 (9 of 9 match); calling rows read 0 -> 8
DRIFT:      0
```

**Every appearance claim is computed, not seen**, and nothing here is evidence about the
radio: a fake radio, a fake tap and a telemetry file on a development machine with none
(FACT-004, FACT-006).

| Criterion | State | Number |
| --- | --- | --- |
| 0.1 tunes to the band's calling spot from `data/bands/olivia-calling.json`; panel names the mode, says nothing decodes | met | 7 of 7 bands ask the radio for their own row's dial; the tune line names the row's center |
| 0.2 Digital family, text color only; log offers `OLIVIA`; telemetry mode field Olivia | met | `ModeGuide.FamilyFor` Digital; `MODE=OLIVIA`; `"mode":"Olivia"` with no callsign, grid, name or place |
| 0.3 Capture button on the Olivia panel writes a 48 kHz WAV | met | 48000 Hz, 1.7-2.05 s of 2 s fed, `olivia_capture_*` with `mode: olivia` |
| 0.4 both files in the tree with citations, read at startup; malformed reported, not guessed | met | 8 rows, 8 codes; a cut-off copy yields the sentence and a null value |
| 0.5 no other decoder, no path to the send chain; BindingHealth, Voice, carry-forward green | met | 0 slot looks in 30 ticks, no `psk31_` event, send refused at the gate; carry-forward app 144 of 144, engine 86 of 86 |
| 0.6 map picks out the Olivia spot (nice-to-pass) | met | the spot is each band's cited center, on the map, with no block outlined |

## 1. What Claude did

**Complete: all six tasks, 0 to 5, none dropped** (tasks 4 and 5 were the drop candidates and
both were built). Development machine QUIVERFULL; the prompt claimed `PROJECT: Hamlet` and the
tree confirmed it (`SHACK_FACTS.md` and `CwProbabilisticDecoder.cs` present, `CoreHMI.sln` and
`MURC.sln` absent, root `C:\Source\HamLet`). Branch **`main`**, six task commits, each pushed
before the next task: `aa1763dd`, `d24958aa`, `317f51c4`, `272e4dba`, `d0680121`, `6da71715`.
Version **1.13.44 -> 1.13.45**.

### Task 0 - the phase opens

`PHASE_OUTCOME.md` carries `UNIT 358` under step 0. `PROJECT_CARD.md` `PHASE` and `PHASE_SET`
move to the Olivia phase and 2026-09-14. The nine fixtures hash as `manifest.json` says, 9 of 9:

```
bfdbff07...  olivia-8-250-cq-rsid.wav        match
0b2b554e...  olivia-16-500-qso-rsid.wav      match
9cb7f62e...  olivia-32-1000-qso-rsid.wav     match
d2ad6150...  olivia-8-250-qso-norsid.wav     match
1a58f793...  olivia-16-500-qso-snr-10db.wav  match
2c26c0d7...  olivia-16-500-qso-snr-16db.wav  match
fd89d10b...  olivia-two-signals-rsid.wav     match
16b16db7...  olivia-noise-only-30s.wav       match
de6b20fe...  psk31-cq-rsid.wav               match
```

Carry-forward before any change: **app 143 of 143, engine 86 of 86.**

**Recorded in `DECISIONS.md` as Tim's ruling, not one this session made**, with its index row
in `CLAUDE.md` §1, in full:

> id: HM-DEC-164
> date: 2026-09-14
> refs: PHASE_PLAN.md, PHASE_STATUS.md, PROJECT_CARD.md, HM-DEC-163, docs/phase-screen-run/, assets/fixtures/olivia/, work instruction 358 task 0
>
> **The phase is "Hamlet works Olivia the way it works PSK31", set 2026-09-14, seven
> steps numbered 0 to 6.** Tim, 2026-09-14, after an interview with the web thread
> before five days away; the rulings from that interview are R27 to R31 in
> `PHASE_PLAN.md`. **PSK31 is tabled after unit 357**, with the typed line delivered.
>
> **It supersedes HM-DEC-163's screen phase, "The screen, done right"**, which is
> archived in `docs/phase-screen-run/` with its step 3 - Tim's verdict at his window -
> still open.
>
> `PROJECT_CARD.md` changes only by ruling (13.3), and this is the ruling that changes
> it. `PHASE` and `PHASE_SET` move; nothing else on the card does.
>
> **Recorded by work instruction 358, the seed unit of the phase, under 12.1 as a
> ruling the owner gave, not one a session made.** What was rejected is not recorded
> here; the interview's reasoning is in `PHASE_PLAN.md` sections 1 to 3.

**Nothing was recorded under §12.1 on this session's own authority.**

### Task 1 - the data is in the tree and read

`data/rsid/rsid-codes.json` and `data/bands/olivia-calling.json`, byte-identical to the
`assets/data/` originals (sha256 `e0d7578d...` and `cfa304aa...`), both embedded in the engine
beside `us-neighborhoods.json`. Three engine types, no literal frequency or code in any of them:

- `RsidCodes` - the codes and fifteen-tone sequences, strict: a code that is not a whole number
  or a sequence that is not as long as the burst fails the whole file.
- `OliviaCallingTable` - the rows, `CallingRowFor(band)` (the 8/250 row, matching `20 m` to
  the file's `20m`), and `DialHzFor(row)`.
- `OliviaData` - reads both once; a missing or malformed file leaves its value null and puts a
  sentence in `Problem` naming the file and what was wrong. The view model reads it when the
  window is built.

The sentence, as a cut-off copy produces it:

```
Hamlet could not read its Olivia calling table, data/bands/olivia-calling.json, because it is
not readable JSON (line 18), so there is no Olivia spot to tune to and none is guessed.
```

### Task 2 - Olivia is a mode everywhere PSK31 is a mode

- **The strip:** `Olivia` is the fifth label. Chip comparisons now ignore case, because
  upper-casing one side worked only while every label was an acronym.
- **The press:** tunes to the band's 8/250 row through `TuneToOliviaAsync`, confirms by read-back
  exactly as the FT8 press does, then asks for USB-D from `ModeFollowPlan.TargetForMode`. An
  unreadable table moves nothing and the strip carries the engine's sentence.
- **The panel:** the strip line is the existing *cannot read it yet* sentence, the decoded panel
  says nothing is coming and what Capture is for, and the waterfall header reads
  `Olivia, not read yet`. No word about slots anywhere on it.
- **The log:** `ContactModes.Logged` is the six plus `Olivia` as `MODE=OLIVIA`, no submode.
  `Named` reads it. **`Six` is unchanged**, so no achievement row appears (step 5's).
- **Telemetry:** the press writes `"mode":"Olivia"`, nothing personal.
- **The gates:** `CanDecode` and `CanTransmitIn` stay false for Olivia, so the slot watch is
  not asked, the PSK31 listener does not start, no card appears and the one send door refuses.

### Task 3 - the Capture button

The unit 344 press shows under PSK31 and Olivia. Under Olivia the tick hands a running capture
the tap's device stream and nothing else; no listener, resampler or search is made. The mode is
taken at the press: `olivia-<time>.wav`, `olivia_capture_started` and `olivia_capture_finished`
with `mode: olivia`. PSK31's events keep their names and category and gain `mode: psk31`.

### Task 4 - the neighborhood map

There is no Olivia block in the band data, so the PSK31 outline would pick out nothing. The
view model hands the map `MapChosenSpotHz`, the cited center under Olivia and null otherwise,
and the map draws it as a dashed line in the digital family's ink with the chosen mode's name.
`NeighborhoodMapControl.IsSpotOnMap` is the rule a test asks.

### Task 5 - the timing table's first row

`data/olivia/timing.json`, marked `source: estimated`, `confirm: step 2`, with its method:
seconds from the manifest, less 2.32 s where `rsid` is true, over characters counted by machine
(38 for the CQ, 251 for the QSO). The 2.32 s agrees with `rsid-codes.json`: 25 symbols at
10.7666 Hz is 2.322 s.

| Variant | s per character | From |
| --- | --- | --- |
| 8/250 | 0.683, plus 0.72 s fixed | line through both 8/250 fixtures (0.702 and 0.686 each) |
| 16/500 | 0.514 | one fixture; the noisy copies are the same length |
| 32/1000 | 0.416 | one fixture |

**An upper bound**: a length includes preamble, tail and block padding. Nothing reads it.

### Decisions this session made for itself - the author's, marked and overrulable

**These are the arbiter's kind under R31 (a number, a mechanism, a layout), not §12.1 entries,
and none is in `DECISIONS.md`.** Item 1 is raised again in section 4 because it may be the other
kind.

1. **The dial is the row's center less the audio center the file's own 20 m row implies.** The
   file gives `center_hz` for every band and `dial_hz` only for 20 m (14,071,500 against
   14,073,000), and says its dial is *the USB dial for a 1500 Hz audio center*. The engine
   derives 1500 from that row, rejects the file if two rows disagree, and dials center less
   1500 on every band. The tune line names the center. The instruction says *tunes to the
   calling center*; dialing the center itself would put the signal at 0 Hz of audio.
2. **The press asks the radio for USB-D itself.** Mode-follow would give USB-D on 80, 40, 20 and
   17 m, where the dial lands in a PSK31 or FT8 block. On 30 m and 10 m it lands in an automatic
   stations block and on 15 m in no block, where mode-follow rightly says nothing. The write is
   a mode, not a keying, and copies `FollowTheMapAsync`'s idiom.
3. **The label is `Olivia`**, a name, not `OLIVIA`; the ADIF spelling stays `MODE=OLIVIA`.
4. **The log offers Olivia through a new `ContactModes.Logged`** rather than by adding it to
   `Six`, which the achievements count.
5. **Olivia's capture events are filed under `Diagnostics`**, because there is no Olivia
   category, and are named for the mode.
6. **The map picks out a spot, not a block**, as a dashed line and the mode's name.
7. **8/250's timing is the two-point line**; the other two are plain ratios.

### Tests

**No suite was run.** Every name filtered, foregrounded, 480 s timeout (HM-DEC-155).

| Run | Result |
| --- | --- |
| Carry-forward app, before any change | **143 of 143** |
| Carry-forward engine, before any change | **86 of 86** |
| `TheOliviaDataTests` (new, engine, task 1) | watched red (CS0234, types absent), then **4 of 4** |
| `TheOliviaSeamTests` (new, app, task 2) | watched red (CS1061, CS0117), then **7 of 7**; one red on the way was the test's own sweep matching the category name `transmit`, fixed in the test |
| `TheCaptureButtonTests` extended (task 3) | watched red (press not offered under Olivia), then **6 of 6** |
| `TheOliviaSeamTests` extended by one (task 4) | watched red (CS1061, CS0117), then **8 of 8** |
| `BindingHealthTests` and `VoiceTests`, after tasks 2 and 4 | **13 of 13**, then **14 of 14** with the seam |
| Carry-forward app, after tasks 2-3 and again after 4-5 | **144 of 144** both times |
| Carry-forward engine plus `TheOliviaDataTests`, after 2-3 and again at the end | **90 of 90** both times |

**229 green before, 234 at the end on the carry-forward invocations, nothing red.**
`TheStopIsAlwaysOnScreenTests`, `TheUnslottedSendTests` and
`TheFt8AndFt4SendsAreByteIdenticalTests` are green and unedited.

### Section 5 of the instruction, checked against the tree

- `PHASE_STATUS.md` line 1 names the Olivia phase with seven steps: **yes**, so
  `install-phase.bat` had run. `PROJECT_STATUS.md` still read unit 358's earlier stop (BLOCKED).
- `docs/phase-screen-run/` holds three files: **yes** (`PHASE_OUTCOME.md`, `PHASE_PLAN.md`,
  `PHASE_STATUS.md`).
- `assets/fixtures/olivia/`: nine WAVs and `manifest.json`; `assets/data/rsid-codes.json`;
  `assets/data/olivia-calling.json`; `assets/reference/SOURCE.md` and
  `olivia-fixture-generator.cpp`: **all present**, hashes above.
- The PSK31 shape: the strip (`DigitalModeChip.Labels`), `DigitalModeFor`,
  `DigitalCallingFrequencies.Find`, `ContactModes`, the telemetry mode field
  (`NoteTheSubMode`), the readiness line (`DigitalIdleText`), the palette (`ModePalette`,
  `ModeGuide.FamilyFor`, which already sorted `OLIVIA` into Digital): **all found**.
- `PROJECT_CARD.md` read `The screen, done right` and 2026-09-12 before task 0.

**Mismatches, reported and not repaired:**

- `PHASE_PLAN.md` R27 and R29 place the files at `data/rsid-codes.json` and
  `data/olivia-calling.json`; the instruction says `data/rsid/` and `data/bands/`. The
  instruction was followed.
- `ThePsk31TabIsInertTests` was retired by unit 323 and is a file of comments; its surviving
  assertions are in `ThePsk31PanelSpeaksPsk31Tests`, which this unit copied.
- §3 says the carried asks are *verbatim in section 4*; the instruction's section 4 holds none.
  They are carried below from unit 357's report, `bd0805cf:output.md`.
- *Tunes to the calling center*: see decision 1 and section 4 item 1.
- `assets/reference/SOURCE.md` pins Jalocha's headers to *master branch 2026-09-14*, a date and
  not a commit. Section 4 item 4.
- `cq_pressed` in the record says `"detail":"Ft8"` under Olivia, and under PSK31 before this
  unit. Filed as HM-OPEN-090 under §12.6 and not repaired.
- Unit 357 reported the app carry-forward at 148 after; this unit measured 143 before. Unit
  357's figure included its own other names.
- `Directory.Build.props` has no version comment for 1.13.42 -> 1.13.43 or 1.13.43 -> 1.13.44.
- §2's tool facts: `mkdir` and `cp` needed approval, so the two data files were written with
  the editor and hash identical to the originals; a `for` loop over `$f` and a PowerShell
  command with `$` were refused (*simple_expansion*); a PowerShell JSON parse without `$` needed
  approval, so **`timing.json` was not machine-parsed**. `rm`, `;`, heredocs and Python were
  not needed. Several `-m` on one commit worked. `tools/status.sh` still writes
  `RULES_AT: HM-DEC-161 (2026-09-11)`.

## 2. What the owner should expect

**Every build in the session succeeded, and warnings are errors here.** Olivia is a fifth chip
on the Digital tab's strip. Pressing it on a band with a calling row tunes the radio to that
row's spot in USB-D and says where it went; with no radio connected it says where it would go.
The panel under it stays empty and says Hamlet cannot read Olivia yet. The waterfall header
reads `Olivia, not read yet`. The neighborhood map draws a dashed line labeled `Olivia` at the
calling spot. The Capture button is there and keeps two minutes of what the radio hears, named
`olivia-<time>.wav`. Nothing on the tab can transmit: the CQ press is refused with *Hamlet
cannot send Olivia yet*.

**What will look wrong and is not.**

- **The dial reads 14.071500 on 20 m, not 14.073000.** The spot is 14.073000 and sits in the
  middle of the passband; the tune line says both numbers (section 4 item 1).
- **Pressing Olivia can change the radio's mode to USB-D on any band**, including 30, 15 and
  10 m where pressing PSK31 would not.
- **No Olivia card on the Achievements screen.** The log can write `MODE=OLIVIA`; the records
  are step 5's.
- **PSK31 capture events now carry `mode: psk31`.** Their names and category are unchanged.
- **`data/olivia/timing.json` does nothing yet.**

**Pushed to `main`.**

## 3. What you should see

**Olivia exists as a mode and cannot send.** Computed from the tests, with the fake radio that
confirms every frequency and declines every mode:

```
press Olivia on each band (asked / cited center)
  80 m   3581500 /  3583000     40 m   7071500 /  7073000     30 m  10141500 / 10143000
  20 m  14071500 / 14073000     17 m  18101500 / 18103000     15 m  21071500 / 21073000
  10 m  28121500 / 28123000     every one: mode Usb, data True

tune line, a radio that takes the mode:
  Olivia on 20 m: the radio confirmed 14.071500 MHz in USB-D, which puts the calling spot at
  14.073000 MHz in the middle of the passband. The spot is a community convention, not a band plan.

strip line   : the radio is on the Olivia calling frequency and Hamlet cannot read Olivia yet,
               so nothing will appear below. You can still hear it, and it sounds like a warble
               you could almost hum.
decoded idle : nothing decoded, because Hamlet cannot read Olivia yet. Nothing will appear here
               until it can. Capture keeps two minutes of what the radio hears, so the evening's
               signals can be used to teach it.
waterfall    : 200-3000 Hz · Olivia, not read yet
log entry    : Olivia -> MODE=OLIVIA
send line    : Hamlet cannot send Olivia yet, so nothing went out.
slot looks   : 0 in 30 ticks
map spot     : 14073000 on 20 m, no block outlined
capture      : olivia-<time>.wav, 48000 Hz; olivia_capture_finished {"mode":"olivia", ...}
```

The *radio that takes the mode* line is composed from the same format string the fake produced
with *but the radio did not take USB-D* in its place; no real radio was asked.

## 4. What's blocking us

Nothing blocks step 1. Five items; **item 1 is the only one that may want a ruling.**

**1. Where the dial goes for "the calling center" - built as center less 1500 Hz, overrulable.**

*Raised because §6 stops the arbiter for "a fact stated about the radio", and this is where the
radio is put.*

**Ruling proposed:** pressing Olivia sets the dial to the row's center less the audio center the
file's own 20 m row implies (1500 Hz), so the cited center sits mid-passband, and the tune line
names both.

**Reasoning:** in USB a dial at 14.073000 puts a signal centered on 14.073000 at 0 Hz of audio,
below the passband, so the operator would hear nothing where Hamlet said Olivia was. The file
states its dial convention and prints one dial, and the derivation is checked against that
printed dial. Step 1's detector listens across the passband and would need the signal inside it.

**Rejected:** *A, dial to the center literally*, as the instruction words it - the spot would be
inaudible and step 6 could not pass at it. *B, a 1500 Hz constant in code* - a number the file
already carries, typed a second time (§0). *C, the fixtures' 1000 Hz audio center* - that is
where the web thread put its test signals, not where the community's dial convention puts them.

To overrule, say *dial the center itself* or name the audio offset.

**2. The Olivia press writes the radio's mode.**

*No ruling wanted; built and marked.* PSK31 and FT8 presses leave the mode to mode-follow. On
three of seven Olivia spots mode-follow has no block to answer from, so the press asks for USB-D
once the frequency is confirmed. A declined mode is said on the tune line and the tune still
counts. It keys nothing.

**3. HM-OPEN-090: `cq_pressed` says `Ft8` under Olivia and under PSK31.**

*No ruling wanted; a finding.* The CQ press record's `detail` is `_digitalMode`, the
decoder enum, which is `Ft8` for every label that is not FT4. The refusal line after it says
`Olivia` correctly. Named in `OPEN_ISSUES.md` and left (§12.6).

**4. Jalocha's headers are pinned to a date, not a commit.**

*No ruling wanted; a mismatch.* `assets/reference/SOURCE.md` says *master branch 2026-09-14*. Step
2 reads `pj_mfsk.h` for structure; a commit hash would let a later reader fetch the same file.
The headers themselves are in `assets/reference/jalocha/`, so nothing is lost today.

**5. `data/olivia/timing.json` was not parsed by a machine.**

*No ruling wanted; a tool limit.* The PowerShell parse needed approval this session could not
give. It was written by hand from the table in section 1; step 2 reads it first.

### Asks still outstanding - carried from unit 357's section 4, per HM-DEC-139, verbatim

The words below are unit 357's, from its line under `## 4. What's blocking us` to its end, as
committed in `bd0805cf`, with only that top-level heading dropped so this report keeps four
sections. Unit 356's, 355's and 354's queues are inside it as unit 357 carried them, and the
queue of units 337 to 353 is carried by reference to `4c55deac:output.md` as unit 354 left it.
**This unit answers none of them.** The screen phase's step 3, Tim's verdict at his window,
stays open with that phase archived.

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

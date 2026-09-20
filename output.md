READ IN THIS ORDER.

A. The phase goal - Hamlet works Olivia the way it works PSK31. Steps 0
   to 3 done and closed; step 4 was 1 of 8 met at the start of this unit
   (4.1, with 4.2 and 4.4 at the engine only) and is 6 of 8 after it;
   steps 5 and 6 not started. Step 4 cannot be done on this unit alone -
   4.5 and 4.8 are the next unit's.
B. Step 4's criteria this unit worked. 4.2: sends read back by the
   detector as the variant sent 6 of 6, within 0.34 Hz against an
   allowance of 5, records announced 6 of 6. 4.6: PttOn 1, Arm( 2, real
   ports keyed 0, Stop aborted the 8/250 send mid-play yes (1032 of
   323160 samples out), FT8/FT4 byte-identical green, PSK31 send guards
   green. 4.3: CQ center from the table on 2 bands, clear spot found at
   875 Hz with a station on the 1500 Hz calling spot and 625 Hz between
   them, receipt station facts 0 and Log absent, a certain answer retired
   it yes. 4.4: caps 82.60 / 61.95 / 49.56 s as the product of the file's
   121 characters and the variant's own rate, longest 8/250 Report armed
   and played (102 characters, 71.99 s), longest text that actually fits
   per variant 120 / 120 / 115, patience scaled at the table and nothing
   in the application consumes a time-based patience - every turn is
   decided by Psk31Turn.Read at ShowPsk31Cards from the parse and the
   carrier-present fact, reading no clock. 4.7: power offer under Olivia
   yes, ALC sentence yes. Met: 4.2, 4.3, 4.4, 4.6, 4.7. Not met: none of
   the five. Entry: chain-guarding reds before 27, after 27, new 0.
C. The report last: section 4 raises 8 items on top of the carried queue;
   none stands in the way of a step 4 criterion. The send needed no line
   on decision AZ's forbidden list, so there is no stop material. No
   working mode's send or read guard went red after green. The PttOn and
   Arm( counts did not move, 1 and 2 before and after. The carry-forward
   seconds are app 2 m 34 s and engine 5 m 1 s against the 480 s timeout,
   the engine invocation's margin now about three minutes.

```
UNIT:       366 - complete at task 5 of 5, task 5 built - 2026-09-19 20:39
PHASE GOAL: Olivia becomes a mode Hamlet works like PSK31 - hears it, reads it,
            answers it, logs it - with the variant taken from the signal's own
            RSID and never from the operator.
UNIT GOAL:  Make the Olivia press go out. Open the one mode gate so the modulator
            unit 365 built reaches the one unslotted sequence that already keys
            FT8, FT4 and PSK31 - at the row's variant and center, or at the cited
            calling spot for a CQ - with its RSID in front, PSK31's receipt,
            PSK31's Stop, the cap and the patience scaled by the variant, and the
            power offer and the ALC as PSK31 has them.
ADVANCED:   yes - step 4 criteria 4.2, 4.3, 4.4, 4.6 and 4.7 all met, and Olivia
            is the fourth mode that can reach the air
NUMBER:     step 4 criteria met 1 of 8 -> 6 of 8; modes that reach the air 3 -> 4
DRIFT:      0 - 0 consecutive units without advance (was 0)
```

## 1. What Claude did

**Complete, at task 5 of 5, with task 5 built rather than dropped.** Development computer, prompt
gated `PROJECT: Hamlet`, confirmed against the tree (`SHACK_FACTS.md` and
`src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` present, no `CoreHMI.sln`, no `MURC.sln`,
root `C:\Source\HamLet`); branch `main`, eight commits, every push succeeded. **Nothing in this
report is evidence about the radio** (`SHACK_FACTS.md`, HM-DEC-093, FACT-004): every figure was
computed, no port was opened and nothing keyed. **No Olivia signal from Hamlet has been on the air,
and none will be until Tim presses it at step 6.**

**The one door opened, and nothing else in the chain moved.** `CanTransmitIn` gained one line so it
admits Olivia. `SendPsk31` became `SendUnslotted` and composes with `OliviaModulator` where Olivia
is chosen, at the row's variant and center or at the cited calling center for a CQ, held to
`OliviaTiming`'s cap, and arms at the **existing** unslotted `Arm` site that PSK31 already uses.
**No new `PttOn` site, no new `Arm` line, and not one edit to `Ft8TransmitSequence`, `Ft8ArmedSend`,
`UnslottedTransmission.Fit`, `Cap`, `OperatorSend`, `RsidBurst`, the abort or the FT8, FT4 and PSK31
branches of `SendMessage`.** **Nothing on decision AZ's forbidden list was needed**, so there is no
`MOVE: stop` material in this unit.

**Task 0 - the unit opens.** Step 4's entry first, decision BJ's before-run of
`docs\chain-guarding-tests.txt` on unchanged `HEAD` `ab8392c0`: engine 98 of 110 in 56 s with twelve
inherited reds, app 48 of 63 in 51 s with fifteen. `ThePsk31CqGoesOutTests` green by name 4 of 4 and
`TheFt8AndFt4SendsAreByteIdenticalTests` green inside the engine run, so the PSK31 loopback chain is
unchanged and the entry is open. All nine Olivia fixtures hash as `manifest.json` says, **9 of 9**.
Carry-forward before any change: app **188 of 188** (2 m 20 s), engine **146 of 146** (4 m 41 s).
Version 1.13.53 to 1.13.54. `UNIT 366 - STEP 4` appended to `PHASE_OUTCOME.md` with no earlier entry
touched.

**Task 1 - the trace** (`Unit366Trace`, engine test project, asserts nothing, not on the
carry-forward line). Its findings are in section 3. The one that decided the shape of task 2:
**`OliviaModulator.Compose` had no call site in `src/` at all**, and every line a press would have
to reach - `CallToAnyoneText`, `Psk31CqOn`, `Psk31AnswerLabelFor`, `card.IsPsk31`,
`HasPsk31PowerOffer` - is an application property, none of them on the forbidden list.

**Task 2 - the gate and the send** (4.2 whole, 4.6). `TheOliviaSendTests`, nine names at that task
and twenty-one by the end of the unit. Decision BI's two refusal classes were rewritten first,
**each in its own commit, before the door moved** - see section 3 for which assertions changed.
Olivia's send guard `AnOliviaCqReachesTheAir` joined the app carry-forward line and the guard table.

**Task 3 - the CQ at the cited spot and its receipt** (4.3), four names, **and one repair**.
`RebuildCards` read `IsPsk31Chosen` alone when deciding whether it had a moment to stamp a card
with, so under Olivia the CQ booked its receipt into the ledger and the panel built no card from
it: the press was recorded and the operator saw nothing. The gate widened to Olivia, which is
decision BC's one path rather than a second.

**Task 4 - the cap and the patience at the press** (4.4 whole), six names. One engine addition,
`OliviaModulator.TextSeconds`, so the typed line's card can say what a line costs in the mode it
would go out in without composing the audio on every keystroke.

**Task 5 - the power offer and the ALC** (4.7), two names. **Built, not dropped.** The gate widened
rather than the code being copied or the sentence forked.

**Four decisions this session made for itself, all overrulable, all in section 4's terms.**

- **The strip's sentence stops saying nothing on a line can be answered.**
  `DigitalIdleText.ListeningAcrossThePassband` ended *"Nothing on those lines can be answered yet"*.
  That was true of PSK31 for the one unit between its listener and its send path and has been false
  ever since, and opening the gate made it false for Olivia too. It now says what a line actually
  offers. **It is a shared sentence**, so PSK31's strip changed with it.
- **The power offer and the ALC's holding-back sentence name the mode he is on.** They read PSK31
  while only PSK31 could reach them. Under Olivia that is a sentence about a mode he is not using
  (§0.0). The learned reference keeps its own `Mode`, so a reference taken on FT8 still reads *"the
  62 Hamlet measured on a clean FT8 send"* inside an Olivia sentence.
- **The no-announcement refusal is checked against the panel's own reading of the file**, before the
  spot is chosen, rather than caught out of the composer. The panel already says which file failed,
  and this is the sentence that names it. The composer's own throw is still caught.
- **The property names `HasPsk31PowerOffer` and `Psk31AlcLine` were not renamed.** Decision BG
  allows a rename; it would touch the view and every test that binds them, to no end the operator
  can see.

**Nothing was recorded in `DECISIONS.md`.** No ruling of this unit's met §12.1's four tests; the four
choices above are reported for overrule instead.

**Verifying the instruction against the tree (§5), every mismatch reported and none repaired.**

1. **`PHASE_STATUS.md` says `CURRENT_STEP: 3` and step 3 `partial`**, not `CURRENT_STEP: 4` with
   step 3 `done` as section 5 states. The launcher's file was left as the launcher leaves it; only
   `WORK_INSTRUCTION` was moved from 365 to 366, because `tools/status.sh` reads it from there.
2. **`PHASE_PLAN.md` leaves 4.1 unchecked** though unit 365 met it, as well as 1.5 and 1.7.
   Reported, not edited.
3. **`docs\chain-guarding-tests.txt` lists 15 Transmit and Audio classes on the engine project**,
   not thirteen, plus two more added since unit 318. All 17 were run before and after.
4. **`PHASE_OUTCOME.md` carries two step 4 entries whose decision letters clash**, as section 5
   says. Neither was edited; this unit's letters start at AZ.
5. **The longest Report the app composes is 102 characters, not the 96 `timing.json`'s `cap_method`
   names.** 96 is the Report to `W1AW`; the compound-callsign one is longer and is what 4.4 is
   measured on. `timing.json` was not edited (§10).
6. Everything else section 5 describes held: `CanTransmitIn` at :2446 with its two callers,
   one `PttOn` code line, two `Arm(` call lines with :15723 the unslotted one, `Psk31ClearSpot` at
   :16226, `OliviaTiming`'s counts 121 / 242 / 32 / 56, `OliviaCallingTable` and its panel
   sentences, and `assets\fixtures\captured\` holding only `README.md`.

## 2. What the owner should expect

**Hamlet answers in Olivia now.** Press CQ on the Olivia tab and a signal goes out on the band's own
Olivia calling spot, taken from the cited table rather than from a number in the code, announced by
its variant so the other end's software follows it across. Press Answer on a station's row and
Hamlet replies at that station's own variant and on his own frequency, without ever asking you which
variant he is using - his RSID said, or the blind search found it, and that is where the answer
goes. The receipt behaves exactly as PSK31's does and a station coming back retires it. The slow
variants are allowed the length they need: a full report at 8/250 takes seventy-two seconds and goes
out whole, where PSK31's thirty-second cap would have refused it. Stop stops it part way through.
The power offer and the ALC sentence read as they do on PSK31.

**Nothing has been on the air.** Every figure in this report was computed on the development
machine, against a fake serial port and a fake sound card. The first Olivia signal Hamlet puts on a
real antenna will be the one Tim presses at step 6.

**What will look wrong and is not.**

- **The PSK31 mode strip's sentence changed.** It used to end *"Nothing on those lines can be
  answered yet"*, which stopped being true of PSK31 several units ago and stopped being true of
  Olivia today. It now says what a line actually offers. That is a deliberate change to PSK31's own
  screen and it is in section 4 to be overruled.
- **The power offer says "Olivia sends a steady carrier" under Olivia.** Same sentence, reading the
  mode you are on.
- **Twenty-seven tests on the chain-guarding list are red.** They were red before this unit, they
  are the same twenty-seven by name, and that list carries known reds on purpose - it is the list
  of what guards the chain, not a list that should be green.
- **Three reds sit outside both lists**, as they did before this unit:
  `TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission`,
  `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow` and
  `WhereTheTransmissionStartsAndWhatTheRecordSaysTests.ATransmitRecordCannotCarryTheMessageOrThe`
  `CallsignInIt`.
- **A compound callsign still opens no card.** Section 4 item 1: the PSK31 exchange parser reads no
  speaker from a line whose callsign carries a slash. That is not new and it is not Olivia's.

**What this unit does not do.** 4.5 - the card's *move up 500 Hz and switch to 16/500* - and 4.8 -
the turn indicator landing within one block of the turnover word - were not in it and are not
built. Step 4 stays `partial` at 6 of 8. Logging an Olivia contact is step 5 and is untouched.

## 3. What you should see

**The answer this unit was commissioned for: an Olivia press goes out.** The gate opened, and the
send reached the sound card through the one existing sequence with no new keying path.

### The operator's evening, press by press

**He presses Olivia on 20 m.** The dial goes to 14.071500 MHz, which is the cited table's
14.073000 MHz calling center with the passband's 1500 Hz allowed for. The strip says Hamlet is
listening for Olivia across the whole passband and that a line from somebody calling anybody carries
an Answer. The power offer is on the panel: *"Olivia sends a steady carrier, so it runs warmer than
voice. Hamlet can set your radio's transmit power to 50% for you. Nothing else on the radio changes,
and nothing is set unless you press this."*

**Two stations appear.** The two-signal fixture, fed to the real tick a quarter-second at a time,
gives two rows: `8/250` at 1000 Hz and `16/500` at 2000 Hz, each with its own text, neither carrying
the other's callsign. Nothing told the listener where they were or which variant they were.

**He presses CQ.** The panel says *Sending "CQ CQ CQ de K1ABC K1ABC K1ABC pse K" now, at 1500 Hz in
the passband.* The record then reads, stage by stage:

```
send_stage composed                | Olivia
psk31_send_composed                | macro cq, 35 characters, 26.93 s, cap 82.60303 s,
                                   | withinCap true, offsetHz 1500, announced true,
                                   | rsidCode 69, mode olivia, variant 8/250
send_stage armed                   | now
send_stage gate_asked              |
send_stage keyed                   |
send_stage handed_to_the_sound_card|
send_stage unkeyed                 |
psk31_send_keyed / _unkeyed        | 26.93 s, aborted false, mode olivia, variant 8/250
ft8_transmission                   | mode Olivia, frequencyHz 14071500, durationSeconds 26.93,
                                   | sampleRate 12000, sampleCount 323160, messageLength 35,
                                   | outcome Played, cameOutOfTransmit OrdinaryUnkey,
                                   | keyed true, fit Fits, audioSeconds 26.93,
                                   | announced true, rsidCode 69,
                                   | announcementSeconds 1.8575833333333334
```

Not one line of it holds a word he sent or his callsign. The fake wire took the keying frame; the
fake sound card was called once.

**He presses Answer on the 8/250 row.** *Sending "N1XYZ de K1ABC K1ABC K" now, at 1000 Hz in the
passband.* **He presses the card's Report on the 16/500 row.** Out at 2000 Hz at 16/500. Neither
press asked him which variant, and no control on the window offers one.

**He types a line on an 8/250 card.** Before he presses anything the card says *"47.1 s of text"* -
the seconds at 8/250, where PSK31 would have said 16.9 for the same words. A line long enough to be
refused says *"too long to send"* first, and the press then refuses with *"Hamlet did not send the
Olivia call: this is 301.36 s of Olivia audio, and this send may be at most 165 s so that a
continuous carrier cannot run on. Nothing keyed."*

**He presses Stop part way through an 8/250 send.** *Stopped: "CQ CQ CQ de K1ABC K1ABC K1ABC pse K"
was going out and Hamlet stopped sending it part way through, and the radio was told to stop
transmitting.* 1032 of 323160 samples had gone out; the record reads `outcome Cancelled`,
`cameOutOfTransmit TheAbort`, and `psk31_send_unkeyed` carries `aborted true`. No new abort route
and no new keying path: PSK31's own.

### The detector's read-back of each send's burst

The audio the **sound card was actually handed** was fed whole to `RsidDetector` with no variant, no
center and no start time (decision I). It found one burst each time:

```
send                         | variant sent | code read       | center sent | center read | error Hz
CQ on 20 m                   | 8/250        | OLIVIA_8_250 69 |     1500.00 |     1499.94 |    0.06
CQ on 40 m                   | 8/250        | OLIVIA_8_250 69 |     1000.00 |     1000.32 |    0.32
CQ moved off a busy spot     | 8/250        | OLIVIA_8_250 69 |      875.00 |      874.68 |    0.32
Answer to the 8/250 row      | 8/250        | OLIVIA_8_250 69 |     1000.00 |     1000.32 |    0.32
Answer to the 16/500 row     | 16/500       | OLIVIA_16_500 70|     2000.00 |     1999.66 |    0.34
longest Report at 8/250      | 8/250        | OLIVIA_8_250 69 |     1500.00 |     1499.94 |    0.06
```

**6 of 6 read back as the variant that was sent, worst error 0.34 Hz against an allowance of 5.**
Every one of their records says `announced: true` with that code. The 5 Hz was not loosened.

### The CQ's spot

```
band  | table's center | dial        | offset in the passband | what was in the way | spot chosen
20 m  | 14.073000 MHz  | 14.0715 MHz |                1500 Hz | nothing             | 1500 Hz
40 m  |  7.073000 MHz  |  7.0720 MHz |                1000 Hz | nothing             | 1000 Hz
20 m  | 14.073000 MHz  | 14.0715 MHz |                1500 Hz | an 8/250 station    |  875 Hz
```

The center is the table's own less the dial, so moving the dial moves it: the two bands give 1500
and 1000 Hz from one line of arithmetic and a literal appears nowhere. **With a station sitting on
the calling spot the call moved rather than refusing**, by `Psk31ClearSpot`'s unchanged rule - *at
least 150 Hz from every carrier being read and from every candidate over quality 0.4, in the middle
of the widest such gap between 400 and 2200 Hz* - landing 625 Hz clear of him. `no_clear_spot` was
not reached on any run.

**Decision BB's finding, reported rather than repaired.** The 150 Hz margin is measured between
carrier **centers**, and it was chosen for a PSK31 signal 31 Hz wide. Olivia is 219 to 977 Hz wide,
so the margin means less than it reads. Measured on the two-signal fixture's own carriers, handed in
as the Olivia listener gives them:

```
carriers heard: 8/250 at 1000 Hz occupies 890.6 - 1109.4 Hz; 16/500 at 2000 Hz occupies 1765.6 - 2234.4 Hz
the rule's spot: 1500.0 Hz
  an 8/250 send there occupies 1390.6 - 1609.4 Hz - 281.3 Hz clear of one, 156.3 Hz clear of the other
  a 16/500 send there occupies 1265.6 - 1734.4 Hz - 156.3 Hz and  31.3 Hz
  a 32/1000 send there occupies 1015.6 - 1984.4 Hz - OVERLAPS BOTH
```

**A CQ is always 8/250 (decision BA), and at 8/250 the rule is comfortably right.** It is a reply at
a wide variant that would sit close, and a reply goes where the station is rather than on a chosen
spot, so nothing in this unit lands on anybody. What the next unit would change, if the owner wants
it: give `Psk31ClearSpot` the width of the signal being placed and clear that plus the margin, which
is one parameter and no new rule. **A second clear-spot rule was not invented** (decision BB).

### The cap table

```
variant  | s per char | count | cap s   | longest text that actually fits | longest keying, burst in
8/250    | 0.68267    |  121  |  82.603 | 120 characters (82.42 s)        |  84.46 s + 1.86 s burst
16/500   | 0.51200    |  121  |  61.952 | 120 characters (61.94 s)        |  63.81 s + 1.86 s burst
32/1000  | 0.40960    |  121  |  49.562 | 115 characters (47.60 s)        |  51.42 s + 1.86 s burst
8/250    | 0.68267    |  242  | 165.206 | 240 characters typed (164.34 s) | 167.06 s + 1.86 s burst
16/500   | 0.51200    |  242  | 123.904 | 240 characters typed (123.38 s) | 125.76 s + 1.86 s burst
32/1000  | 0.40960    |  242  |  99.123 | 240 characters typed ( 98.80 s) | 100.98 s + 1.86 s burst
```

Every cap is asserted as the **product** of `timing.json`'s count and that variant's own seconds per
character, never as a number. **The longest text that actually fits is measured, not assumed** - it
is short of the count because the air sends whole blocks, which is decision BD's answer to unit 365
item 1 taken to the press. At 32/1000 a block carries five characters, so the granularity is
coarsest there and 115 is the honest figure.

**The longest Report the app composes**, to the compound callsign `VP2V/W1AW`, is 102 characters:

```
VP2V/W1AW de K1ABC  RST 599 599  Name Pat Pat  QTH Boston MA  Grid FN42 FN42  BTU VP2V/W1AW de K1ABC K
```

At 8/250 it is **71.99 s against the 82.603 s cap**, it was **armed and played** through the one
door, and its record reads `outcome Played`, `keyed true`, one call to the sound card. PSK31's
thirty seconds would have refused it, which is what *a Report at 8/250 is allowed its length* means.

**Nothing else's cap moved**, each asserted: `OperatorSend.LongestUnslottedSeconds` 30, the typed
line's 60, a PSK31 macro held to 30, the FT8 slot 15, the FT4 slot 7.5.

**The typed line's card sentence at 8/250, verbatim**, for a framed line of 69 characters:

```
47.1 s of text
```

and past the cap:

```
299 s of text, too long to send
```

**The patience** (decision BE): `32 x 0.68267 = 21.8454 s` at 8/250, `16.3840` at 16/500,
`13.1072` at 32/1000, no two alike, asserted as the product. **Nothing in the application consumes
a time-based patience, and nothing was invented so that a criterion had something to point at.**
Every turn in Hamlet is decided at `MainWindowViewModel.cs:4107` by `Psk31Turn.Read`, from the
channel's finished messages, whether characters have arrived since the last of them, and the
operator's callsign - no clock at all. `Psk31Macros.AnswerSeconds`, the only patience the tree
states, has no caller in `src/`; a source scan asserts that it still has none.

### The receipt, quoted

```
is a receipt : True
callsign     : [CQ]
place        : []
state word   : [Calling]
sentence     : [Your call went out to anyone listening.]
action label : []
shows globe  : False
```

**No place, no country, no grid, no distance, no bearing, no Log button** - asserted on the same
properties `TheCqReceiptTests` asserts for PSK31. **What retired it:** a certain message addressed to
the operator arriving on an Olivia row, through the same certainty gate, leaving that station's own
card in the receipt's place. Until this unit no receipt appeared under Olivia at all: `RebuildCards`
was gated on PSK31 and the press booked into the ledger with nothing drawn.

### Decision BI: what the two refusal classes assert now

`TheOliviaSeamTests` (rewritten in commit `6061cbce`, its own):

- `ThePanelNamesTheModeAndSaysNothingCanBeAnswered` -> `ThePanelNamesTheModeAndPromisesNoSlot`.
  **Gone:** `Assert.Contains("can be answered yet", ...)` and `Assert.False(CanAnswerRowsForTests)`.
  **Kept:** the mode is named on the strip and on the decoded idle; nothing bound on a panel for a
  mode with no slots says "slot". **Added:** the strip says what a line actually offers.
- `NoDecoderIsAttachedAndNoPathReachesAnythingThatKeys` ->
  `NoDecoderIsAttachedAndNothingIsComposedUntilAPress`. **Gone:** the CQ press, *"cannot send
  Olivia"*, `send_refused` as the last action, and the sweep forbidding `composed` and `send_stage`.
  **Kept:** the slot watch is not asked, no other mode's decoder runs, no row and no card appear on
  that feed. **Added:** the transmit category is empty across thirty ticks, because a transmission
  takes a click - which is the half of §0.2 that outlives the gate.

`TheOliviaRowsTests` (rewritten in commit `13588eb4`, its own):

- `WithRowsPresentEverySendControlRefusesAndNothingKeys` ->
  `WithRowsPresentNothingIsComposedUntilAPressAndEachPressCarriesItsRowsVariant`. **Gone:** *"cannot
  send Olivia"* on every press, the count of at least four `send_refused`, and `composed`, `armed`
  and `send_stage` in the word sweep. **Kept and added:** no composed line and no stage before the
  first press; one composition per press that got past the gate and not one more; each composition
  carrying `mode olivia` and the variant of the row it was addressed to, with the CQ at the calling
  variant and no variant appearing that no signal announced; and nothing keyed, because that panel
  has no armed send and every press ends at `no_transmit_path`.

**No other existing test was edited to make a change pass.**

### The runs, before and after, by name

**Chain-guarding (`docs\chain-guarding-tests.txt`, decision BJ), all 17 engine and 11 app classes:**

```
                    before (ab8392c0)        after task 2
engine      98 of 110,  56 s          98 of 110,  46 s
app         48 of  63,  51 s          48 of  63,  45 s
```

**The same twenty-seven reds by name, before and after, none new.** Engine: the six in
`TheTelemetryLineSaysWhatActuallyWentOutTests`, the three in
`WhereTheTransmissionStartsAndWhatTheRecordSaysTests`, the two in
`TheFitGuardAsksAboutTheGridTheSendIsOnTests`,
`TheSinkPlaysToANamedEndpointTests.ACancelledPlayGoesOutShortAndTheSequenceCallsItAudioFailed` and
`TheLoopbackProvesTheWholeChainTests.AMessageHamletComposedLeavesThisMachineAndComesBackFromItsOwn`
`Decoder`. App: the eight in `TheMenuIsUnderTheMouseTests`, the four in
`TheWholeFt4ChainRunsFromOneRightClickTests`, the two in `TheWholeChainRunsFromOneRightClickTests`
and `TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission`. These are carried as ask 35
and were not chased.

**Carry-forward (`docs\carry-forward-tests.txt`, HM-DEC-165), two invocations, one build each:**

```
                 before                         after
app      188 of 188, 2 m 20 s          189 of 189, 2 m 34 s   (+1: Olivia's send guard)
engine   146 of 146, 4 m 41 s          146 of 146, 5 m 1 s
PttOn code lines       1                      1
Arm( call lines        2                      2
```

**Every per-mode guard green before and after: FT8 send and read, FT4 send and read, PSK31 send and
read, Olivia read, modulate and - for the first time - send.** No red after that was green before.

**One app red was seen and it was a flake, reported with the run it came from.** The first after-run
read 188 of 189 on
`ThePsk31OfferTests.TheOfferIsOneButtonAndItIsTheOneTheEngineNamed`, which is green alone and green
on a second run of the same invocation; the final run of the unit, after task 5, read **189 of 189**
and that is the number reported. It touches the power offer's button, which this unit did change, so
it is named here rather than waved through: it passed alone before task 5's change and after it.

**Seconds against the 480 s timeout:** app 154 s, engine 301 s. The engine invocation's margin is
about **three minutes**, unchanged from unit 365's, because **nothing this unit added went on the
engine line**.

### Olivia's row on the guard table now reads

```
Olivia   read      TheOliviaDemodulatorTests (the clean fixtures)                    engine
         modulate  TheOliviaModulatorTests.EachMacroAndATypedLineComeBackIdentical   engine
         send      TheOliviaSendTests.AnOliviaCqReachesTheAir                        app
```

### The criterion table

```
id  | state                   | this unit's numbers
4.1 | met (unit 365)          | 30 of 30 loopbacks identical; every measure inside tolerance
4.2 | met                     | 6 of 6 read back as the variant sent, worst error 0.34 Hz of 5;
    |                         | 6 of 6 records announced with the right code
4.3 | met                     | center from the table on 2 bands, 1500 and 1000 Hz; spot found at
    |                         | 875 Hz with a station on the calling spot; receipt station facts 0,
    |                         | Log absent; a certain answer retired it
4.4 | met                     | caps 82.603 / 61.952 / 49.562 s as the product; the 102-character
    |                         | Report armed and played at 71.99 s; one character past what fits
    |                         | refused LongerThanTheCap; PSK31's 30 and 60 and the FT8 and FT4
    |                         | caps unmoved; patience 21.845 / 16.384 / 13.107 s
4.5 | not worked - next unit  | -
4.6 | met                     | PttOn 1, Arm( 2, real ports keyed 0, Stop aborted mid-play,
    |                         | FT8/FT4 byte-identical green, PSK31 send guards green
4.7 | met                     | power offer under Olivia yes, settles as PSK31's; ALC read 95 of
    |                         | 120 against a reference of 62 learned on FT8, judged, sentence
    |                         | produced and naming FT8
4.8 | not worked - next unit  | -
```

**Step 4 is 6 of 8 and stays `partial`.**

## 4. What's blocking us

**Nothing blocks step 4, and nothing here is stop material.** Eight items. **The send needed no line
on decision AZ's forbidden list**, no working mode's send or read guard went red after green, and
the `PttOn` and `Arm(` counts did not move. None of the eight stands in the way of 4.2, 4.3, 4.4,
4.6 or 4.7, and none of them is 4.5's or 4.8's.

### Raised by this unit

**1. A compound callsign opens no card, so its Report cannot be reached by a button.**

*A finding about a shared PSK31 path, reported and not repaired* (§R14, §10). `Psk31ExchangeParser`
reads no speaker from a line whose callsign carries a slash: fed `K1ABC de VP2V/W1AW VP2V/W1AW K`
the row shows an empty sender, no conversation opens and no card appears. **This is not Olivia's and
it is not new** - it is the same parser PSK31 rows go through - but it is why 4.4's *longest Report
armed and played* went through `SendMessage` directly rather than through the card's own Report
button. The one door, the mode gate, the variant, the composer, the cap, the licence gate and the
one keying site all applied unchanged; what was not exercised is the button. **If the owner wants it
exercised, the repair is in the parser and it changes what PSK31 does too**, which is why a session
did not make it.

**2. Three operator-facing sentences changed, two of them on PSK31's own screen.**

*The arbiter's, under §6's* a hint, a label *clause; overrulable, and named because one of them
appears on a screen this phase is not about.*

- `DigitalIdleText.ListeningAcrossThePassband` lost *"Nothing on those lines can be answered yet"*
  and gained *"and a line from somebody calling anybody carries an Answer that replies on his own
  frequency"*. **It is shared with PSK31**, where the old clause had been false for several units.
- `Psk31PowerOffer` opened *"PSK31 sends a steady carrier"* and now opens with the chosen mode.
- The ALC's holding-back form said *"that is what makes PSK31 spread into the people either side of
  you"* and now names the chosen mode. **The reference keeps its own mode** two clauses back, so a
  reference learned on FT8 still says FT8.

*Rejected:* leaving them, which ships three sentences that assert something untrue of the mode the
operator is on (§0.0); and forking each into a PSK31 copy and an Olivia copy, which decision BG
forbids and which is a second place for them to drift.

**3. The transmission record cannot carry the variant without touching a forbidden line.**

*A gap between decision BH and decision AZ, resolved the safe way and reported.* BH asks that the
no-slot record carry `mode` Olivia **and the variant**. The record carries `mode: Olivia`,
`announced: true`, `rsidCode` and `announcementSeconds` already; adding a `variant` field means
editing `TransmitRecord` and the line in `Ft8TransmitSequence` that builds it, and AZ forbids both.
**So the variant is on the application's own send lines** - `psk31_send_composed`,
`psk31_send_keyed`, `psk31_send_unkeyed` and `psk31_send_refused` all carry `mode` and `variant` -
**and the record names the variant by its code**, 69 being `OLIVIA_8_250`, 70 `OLIVIA_16_500`, 71
`OLIVIA_32_1000`, which is one lookup in `rsid-codes.json` away from the word. If the owner wants
the word on the record itself, it is a field on `TransmitRecord` and one line in the sequence, and
it is a unit that is allowed to touch them.

**4. `Psk31ClearSpot`'s margin is measured between centers, and Olivia is wide.**

*Decision BB's finding, with the numbers, shipped as the PSK31 rule unchanged.* The full arithmetic
is in section 3. In short: 150 Hz between carrier centers was chosen for a 31 Hz signal, and at
16/500 or 32/1000 a spot that satisfies the rule can sit 31 Hz from a station's edge or overlap him
outright. **Nothing this unit does lands on anybody** - a CQ is always 8/250, where the rule has
room to spare, and a reply goes where the station already is - so this is a finding about what the
rule would do if a later unit called at a wide variant. *Rejected:* a second clear-spot rule for
Olivia, which decision BB forbids. The change, if it is wanted, is to hand `Choose` the width of the
signal being placed.

**5. The Olivia tab was handing the clear-spot rule an empty band.**

*Found by the trace and repaired inside the task, reported because the shape of it will recur.*
`ClearSpotForTheCall` read `_psk31`, the PSK31 listener, which does not run under Olivia - so with
two stations on the screen it handed the rule an empty list and answered 1300 Hz. **The honest state
of a search that has not run is not the same as the honest state of one that has** (§0.0). It now
hands over the Olivia listener's own carriers. **Anything else that reads `_psk31` to learn what is
on the band will have the same fault**, and this unit did not sweep for more of them.

**6. Two application paths were gated on PSK31 and silently did nothing under Olivia.**

*Both repaired inside their tasks; named together because they are one pattern.* `RebuildCards`
asked `IsPsk31Chosen` when deciding whether it had a moment to stamp a card with, so the Olivia CQ
booked its receipt into the ledger and no card was drawn - **a press that was recorded and invisible**.
`HasPsk31PowerOffer` asked the same question, so the offer never appeared. Both now answer for both
keyboard modes. **The pattern is a `Psk31`-named member that is really about *unslotted* or
*keyboard mode***, and there are more of them: `ShowsSlotClock`, `DigitalDecodedIdle`,
`CallToAnyoneText` and `Psk31CqOn` were each found and widened by hand in this unit. A sweep for the
rest is worth a task in the next unit.

**7. One app carry-forward red was a flake, and it touches what this unit changed.**

*Reported with the run each number came from* (§5). `ThePsk31OfferTests.TheOfferIsOneButtonAndItIs`
`TheOneTheEngineNamed` was red once in the first after-run of the app invocation, green alone, and
green in both later full runs of the same invocation, including the final one after task 5. **It
touches the power offer's button, which task 5 widened**, so it is named rather than waved through:
it passes alone both before and after that change, and the 189 of 189 reported is the final run's.
`TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns`, which
unit 365 item 7 reported as a **deterministic** red, was **green in every run of this unit** -
before any change and after every task. Decision BF said report it and do not chase it; what is
reported is that it is not deterministic.

**8. Tool facts this session.**

*Reported, not repaired.*

- **`>` redirection is refused anywhere under the root**, including into `.run-unit\`, with the
  message that the only allowed working directory is `C:\Source\HamLet` itself. `sed -i` on a source
  file was refused the same way. **Python did not run** (`python -c` needed approval), and
  `git restore --source` and `git checkout <rev> -- <file>` both needed approval, as §2 records.
- **So the carried queue below was not moved as bytes and it was not retyped either.** `output.md`
  from `HEAD` was still in the working tree untouched, so this report **replaced only its lines 1 to
  264 with the editor and left line 265 to the end exactly where they were**. That is the one path
  that neither needs a refused command nor risks a transcription error. **Both hashes are reported
  and they match:** `git show ab8392c0:output.md | tail -n +265 | md5sum` gives
  `fe891bea7e4e2f4a2dc91f1d491ce4ae`, and `tail -n +608 output.md | md5sum` over the carried block
  in this file gives `fe891bea7e4e2f4a2dc91f1d491ce4ae`.
- **`tools\arbiter\validate-output.bat` could not be run**, as unit 365 found - the direct call
  needs approval - so **this report's shape was checked by hand** against the rules the script
  prints: the ordering block with A, B and C and C's item count; the `UNIT:` line above section 1;
  the four `##` sections in order with their exact names and no fifth; section 3 non-empty; and
  `###` nested under them and nowhere else.
- A commit message with an apostrophe in it broke a quoted heredoc, as §2 says; every commit message
  this unit wrote went into a file under `.run-unit\` with the editor and was passed with
  `git commit -F`. `sh tools/status.sh ... && dotnet test ...` and `&& git ...` ran. A `grep -E`
  alternation ran where `\|` would have been refused. `echo` into a file was refused.
- `.u365-head.md` and `.u365-report.py` are **still untracked in the root** and were not committed
  and not built on, as §5 says. `rm` is still refused.

### Unit 365's section 4, and what answers it

**Carried below verbatim and marked nowhere** (§3). For the next author's convenience, and without
touching the words: **items 1 and 6 are answered by decision BD** - the counts in `timing.json` stay
as measured, the count is a bound on the text rather than a promise, refusal is the safe direction,
and what this unit adds is the measured longest text that fits per variant (120 / 120 / 115) plus
the card stating a typed line's seconds before the press. **Item 2 is answered by decision BE** and
by the measurement in section 3: nothing in the application consumes a time-based patience.
**Item 3 is answered in this report's section 1, mismatch 4** - the two clashing step 4 entries are
left as history and this unit's letters start at AZ. **Item 7 is decision BF**, and this unit found
its red is not deterministic. **Item 8 is answered by this unit**: Olivia has a send guard now.
**Items 4, 5 and 9 are findings and are carried unanswered.**

### Asks still outstanding - carried from unit 365's section 4, per HM-DEC-139, verbatim

The words below are unit 365's, from its line under `## 4. What's blocking us` to its end, as
committed in `ab8392c0`. Only that top-level heading is dropped, so this report keeps four sections.
Unit 362's, 364's, 363's and the older queues are inside it already. **This unit answers none of
them beyond what the paragraph above names.**

**Nothing blocks the gate unit.** Nine items; none of them stands in the way of 4.1, 4.2 or 4.4.

### Raised by this unit

**1. A macro within a few characters of the cap count does not fit at 8/250.**

*The arbiter's arithmetic, applied as written, reported as a number.* Decision AV counts the cap in
characters (121 for a macro) and charges it at the variant's seconds per character, which gives
82.603 s at 8/250. But the air sends whole blocks of three characters plus one symbol of tail, and
Hamlet adds the burst's five symbols of silence before the first symbol, so 121 characters actually
take 84.464 s - `LongerThanTheCap`. The Report, at 96 characters, fits with 16 s to spare, so 4.4's
stated test is met; what does not hold is the reading that "121 characters always fit". *The
alternative that was rejected:* deriving the count from whole blocks and the pause instead, which
would have been a different rule from the one decision AV states, and a session is not to re-argue
a ruling in force. If the owner wants the count to mean "any text of this length fits", the rule
becomes floor((cap seconds - pause - tail) / block seconds) x characters per block, which at 8/250
gives 120 characters and at 32/1000 gives 120 too.

**2. Which patience should the Olivia turn indicator keep?**

*Tim's, because it is about what a card tells him and how long it waits.* The only patience in the
tree is `Psk31Macros.AnswerSeconds`, R18's stated equivalent of one FT8 slot, and it has two
properties that do not carry over cleanly: it depends on the two callsigns (7.84 s for W1AW and
KC3QIS, longer for two long calls), and nothing in the application actually calls it - only
`TheCardWaitsOnHimTests` does. This unit counted it at the test callsigns, so `patience_characters`
is 32 and 8/250's patience is 21.845 s. *The alternative rejected:* `Psk31Listener.IdleAfterSeconds`
(2 s), which is about a station idling mid-sentence rather than about waiting for a turn, and would
have made the Olivia patience a tenth of PSK31's.

**3. `PHASE_OUTCOME.md` carries a step 4 entry for an approach that was not built.**

*A record mismatch, reported and not repaired.* The `## UNIT 1 - STEP 4` block above this unit's
entry describes opening `CanTransmitIn`, an Olivia send through the sequence, and an
`AnOliviaCqReachesTheAir` send guard, under decision letters AO to AX that mean different things
from this unit's AO to AY. It has `FATE: executed` and `STATE_AFTER: in progress`, and no report
exists for it. The next unit's author reads that file; two different step 4 plans with clashing
decision letters in it is a place to get the wrong instruction.

**4. `PHASE_PLAN.md` leaves 1.5 and 1.7 unchecked.**

*Reported, not repaired*, per §5. Step 1 is done on units 359 and 360.

**5. The silence after the RSID burst is not in the data file.**

*A fact taken from the fixtures and from `SOURCE.md`, not from `rsid-codes.json`.* The file states
`silence_symbols_before: 5` and nothing about after. Hamlet now sends five symbols of silence after
the tones too, which is what the author's three fixtures measure (0.467 s, within a millisecond of
each other). If that belongs in the file as its own field, it is a one-line data change and a line
in `RsidBurst`'s reading - but `RsidBurst` is one of the files this unit was told not to touch.

**6. A typed line at 8/250 may key for 165 seconds.**

*Raised as a number, not as an objection.* `cap_typed_characters` is 242, which at 8/250 is 165.2 s
of continuous keying for one send. That follows straight from R32 and decision AV - PSK31's own 60 s
counted in characters and charged at the variant's rate - and R32 says a cap is neither the radio's
safety nor a promise. It is stated here because 165 s of carrier is a different thing on the air
from 60, and the operator may want a shorter typed-line count before the gate opens.

**7. The Stop test's red is deterministic, not a coin toss.**

*A finding for the gate unit, reported and not repaired* (§12.6). `KeyedAtTheOpeningSizeAClick...`
fails because the abort pair is written twice - `1C 00 01 | 17 FF | 1C 00 00 | 17 FF | 1C 00 00`
where the test expects one pair - so a Stop click and the `finally` are both aborting. It is red on
a tree this unit did not change, it is in the safe direction (an extra abort, never a missing one),
and 4.6 will have to look at it when Stop is proved against an Olivia send.

**8. Olivia still has no send guard.**

*By design.* The carry-forward row says `send none yet - added the unit the gate opens`, and 4.2 is
reported as the engine half met, not as *every send*, because no send exists yet.

**9. Tool facts this session.**

*Reported, not repaired.*
- **Python did not run**, against §2's recorded fact from unit 362: `python file.py` needed approval
  in three forms and was never run. `tee`, `>` redirection anywhere under the root, `git restore
  --source` and `git checkout <rev> -- <file>` were all refused or needed approval. **So the carried
  queue below was read with `git show ... | sed -n N,Mp` and written back with the file editor**,
  which is the one path left; it is the first time since unit 360 that it could not be moved as
  bytes. **It was checked afterwards and it is byte-identical:** `git show a7f81c48:output.md |
  sed -n '204,1108p' | md5sum` and `tail -n +363 output.md | md5sum` both give
  `93f818aff42e7e5e6b724cc3d8f14b03`; only the source block's leading blank line is dropped.
  `.u365-head.md` and `.u365-report.py`, the assembler that could not be run, are left in the
  root untracked, because `rm` is refused.
- A `grep` pattern containing `\|` was read as several operations and refused; `grep -E` with the
  same alternation ran. A `for` loop over a variable was refused as *simple_expansion*.
- **`tools\arbiter\validate-output.bat` could not be run** - both `cmd /c` and the direct call
  needed approval - so this report's shape was checked by hand against the rules the script prints:
  the ordering block with A, B and C and C's item count, the `UNIT:` line above section 1, the four
  `##` sections in order with their exact names, no fifth, section 3 non-empty, and `###` nested
  under them.
- `sh tools/status.sh ... && dotnet test ...` and `&& git ...` ran. Every status write used
  `EXECUTING` and `code`. `tools/status.sh` now writes `RULES_AT: HM-DEC-165 (2026-09-19)` and takes
  `WORK_INSTRUCTION` from `PHASE_STATUS.md`, which this unit set to 365.
- The MCP connectors for Gmail, Google Calendar and Google Drive reported that they need
  authorization in claude.ai's connector settings. Nothing in this unit used them.

### Asks still outstanding - carried from unit 362's section 4, per HM-DEC-139, verbatim

The words below are unit 362's, from its line under `## 4. What's blocking us` to its end, as
committed in `a7f81c48`. Only that top-level heading is dropped, so this report keeps four
sections. Unit 364's, 363's and the older queues are inside it already. **This unit answers none of
them**, except that decision AY's reading of unit 362's items 1 and 3 - both carried to Tim,
neither chased - is the arbiter's and is recorded in `WORK_INSTRUCTIONS.md`, not here.

**Nothing blocks FT8.** Seven new items, the first one asked of the owner and not blocking. The
carried queue follows them.

### Raised by this unit

**1. The retry at the press is transmit-adjacent, and it is the author's repair.**

*Asked, not blocking; shipped as described.* A press with a radio connected and nothing armed now
builds the transmit path once more before refusing. It opens the named transmit audio device at
the click, which unit 260 deliberately did only at connect so a device exception could not reach
the operator mid-answer; `BuildTheArmedSend` catches that exception and refuses in words, so it
does not. It keys nothing, adds no keying site and changes nothing about one press being one
transmission. **Rejected**: leaving the build at connect only, which keeps the outage that stopped
two sends until a reconnect; rebuilding on every Settings change, which the order did not name and
which touches the Settings window. Overrule and the retry is two lines to remove, and the record
lines stay either way.

**2. Which reason stopped the sends on 2026-09-19 is not known.**

*A finding.* The old build wrote none of `BuildTheArmedSend`'s refusals to the record. The first
connect on this build writes `transmit_path`, and a press that still cannot go writes
`send_refused` with the reason.

**3. The starter card is booked before the arm check.**

*A finding, not changed.* `BookTheSend` runs before `_armedSend` is checked, so a press that goes
nowhere still shows a card. That is probably the *"seems to be queued"* in the report. Work
instruction 309 put the card at the press on purpose, so moving it is a ruling and not a repair.

**4. The before-change carry-forward was run on unchanged `HEAD` in a separate worktree.**

*A departure, reported in section 1.* The numbers are real and were taken from the unchanged
commit.

**5. `ThePowerIsOfferedTests.TheOfferRendersAtHalfAndNothingMirrorsTheUsbModLevel` threw
Avalonia's dispatcher-loop error once.**

*A flake, not a regression.* Green on two targeted runs beside the new class and on the full
rerun.

**6. The order's number and its tool facts.**

*Mismatches, reported in section 1.* The number 362 is reused, there is no `ISSUED:` line, and
Python ran, contrary to the order.

**7. Olivia has no send guard.**

*By design, for now.* It gets one the unit Olivia first transmits, and the list says so.

### Asks still outstanding - carried from unit 364's section 4, per HM-DEC-139, verbatim

The words below are unit 364's, from its line under `## 4. What's blocking us` to its end, as
committed in `eeaca3c8`. Only that top-level heading is dropped, so this report keeps four
sections. This unit answers none of them.


**Nothing blocks step 3 or step 4's entry.** Twelve new items. Item 1 is for whoever runs the loop;
the rest are findings, and none wants a ruling from the owner. The carried queue follows them.

### Raised by unit 364

**1. The harness ended this unit while it ran, and an arbiter wrote over the loop's files.**

*A finding about the loop, not the code; nothing here was edited or committed by this session.* At
about 14:05, with this unit at task 4, something outside the session rewrote three files:
- **`WORK_INSTRUCTIONS.md`** now reads *No unit authored - the arbiter stops: unit 364 is still
  running*, and tells this unit its instruction is at `6c632d3d`.
- **`PHASE_OUTCOME.md`** has a new last entry, `UNIT 3 - STEP 3`, with `FATE: executed`, `COST:
  17.930809499999995` (unit 363's) and `STATE_AFTER: partial`, judged from this report's task-1
  draft. The arbiter's own note says all three fields are false, and this report agrees.
- **`PHASE_STATUS.md`** lost its `HEARTBEAT` line.

This session committed `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `WORK_INSTRUCTIONS.md` at task 0,
as the instruction asked, and has not committed their later changes. `SESSION.lock` and
`.run-unit\` were never committed.

**2. The retire factor is 56, not 24.**

*A number, raised by decision AJ's own rule; the arbiter's to overrule.* The retire is judged on
what the listener knows, and a block reaches a channel about six seconds after it ends. So the
longest a sending station goes without a new accepted block, as the listener sees it, is 8.14 s on
every clean file: the wait from the burst to the first block. At 24 the clean files hold, 32/1000 by
1.7 s. **The -16 dB 16/500 file goes 28.24 s, 55.2 characters**, because its reader shows 9 blocks
of 77. So 24 would have ended that row while the station was still sending. At 56 it holds by
0.43 s. That file is the one the plan's revised 2.2 calls below 16/500's sensitivity. **A station
that stops is listed as live for 38 s at 8/250, where 24 would have given 16 s.**

**3. Decision AN held, and here is why.**

*A finding, for step 4.* Every send in the app goes through `SendMessage`, and its mode gate
(`CanTransmitIn`, `MainWindowViewModel.cs:2431`) refuses Olivia before either `Arm(` site. Drawing
rows added two ways to reach that door under Olivia: Answer on a finished CQ, and a card, which opens
where a station certainly calls the operator. Both reached it and were refused. **Step 4 opens the
door for Olivia at that one gate**, and every row control will then be live at once.

**4. The row path's changed lines, named (decision AF).**

*For the record of what the one path took.* `DigitalDecodeRow`: `Variant`, `HasVariant`.
`ShowPsk31Channels`: an optional `variants` map, `var variant = ...`, `&& shown.Variant == variant` in
the unchanged-row check, and `Variant = variant` on the new row. Four row events gain a mode tag:
`ReadPsk31`'s `LineParsed`, `EndOrRemovePsk31Row`'s `RowEnded`, and the two `RowCleared` calls, each
passing `OliviaTagFor(...)`, which is null on a PSK31 row. `AudioSecondsHeard` falls back to the
Olivia listener's clock. `MainWindow.axaml`'s text-row grid gained a leading column, so its four
other columns moved by one. **No line in the parser, the splitter or any 3.3 feature.**

**5. The CQ filter holds back no PSK31 or Olivia row - the instruction expected it to drop a non-CQ
one.**

*A mismatch with the tree, reported, not repaired.* `WantsRow` (`MainWindowViewModel.cs:2138`, unit
337): *the squelch is the only gate on a PSK31 row*; the CQ toggle applies to FT8 rows only, and a
line to the operator goes to his side. Olivia rows get exactly that. 3.3 asks for the same outcome
with no code change, and it is the same.

**6. An over's last line is not read until the next character arrives.**

*A finding, shared with PSK31.* The splitter closes a message on the character after the turnover.
The two-signal file's CQs end `pse K` with nothing after them, so on that file no line is parsed,
neither row is a clickable CQ, and `psk31_line_parsed` needed a line given through the seam. On the
air the station's next character, or the next station's, closes it.

**7. A certain message to the operator on an Olivia row is booked in the contact ledger with no
mode.**

*A finding for step 5, not a send.* `ReadPsk31` books such a message through
`Ft8ContactLedger.RecordPsk31` (`MainWindowViewModel.cs:2790`) for PSK31 rows, and now for Olivia
rows. The ledger carries no mode. It keys nothing. Logging Olivia with its submode is step 5's.

**8. An Olivia row's strength is a dash, and the strip's reused sentence promises one.**

*Wording, the arbiter's.* The listener's figure is a block's S/N in its own units, which is not
comparable to PSK31's dB in 2500 Hz, so the cell is honest and empty. The strip now says PSK31's
*... with where it sits, how strong it is and its text as it arrives*. That is true for PSK31 and
not for Olivia's strength.

**9. The tracked center wanders after a station stops, up to 19.5 Hz, until the retire.**

*A finding; decision AG's shown center hides it from the row.* On the two-signal file, after its last
block, the 16/500 channel's track moved to 2019.53 Hz and the 8/250's to 996.09 Hz. The rows showed
2000 and 1000 throughout, and nothing moved after the retire. The lag is 5.84 s median, the same on
every variant, because every block is 2.048 s.

**10. Two RSID detectors run under Olivia.**

*A cost, reported.* `HearRsid` (step 1's `rsid_heard`, criterion 1.6) and the listener's own
detector both run on the same audio. The ratios in section 3 include both. Folding `rsid_heard`
into the listener would save about 0.08 of real time, and would change a step 1 event's source.

**11. R27's across-tab half is logged, not built (decision AE).**

*For the plan's author.* An RSID heard under PSK31 or FT8 switches nothing; the Olivia listener runs
under the Olivia tab only.

**12. Tool facts and runs this session.**

*Reported, not repaired.*
- The engine line ran **three times at task 0**: my first `grep` pattern missed the summary line
  under normal verbosity. Every later filter kept `Passed!|Failed!`.
- `sed -i` on `output.md` and a `>` redirect into the root were refused as *outside the allowed
  working directories*. **So unit 363's sections 1-3 were removed with the file editor**, and the
  carried queue below is the original bytes: `git show 75571f5d:output.md | tail -n +338 | md5sum`
  gives `15c07dbd758513f633cdc449c55a332b`, and so did this file's carried block at task 1
  (`tail -n +127`) and in the final report (`tail -n +509`).
- `grep -o` with a quantifier, `git config` and `grep -v` in a pipe needed approval; `grep -n` on a
  `.trx` and `sh tools/status.sh ... && dotnet test ... | grep -E` ran.
- `TheTestsStayOffTheNetworkTests.The354LayoutReadsTheSameNumbersTwiceRunning` went red once in the
  list run and was green alone on the first rerun (task 4).
- `tools/status.sh` still writes `RULES_AT: HM-DEC-161` and `WORK_INSTRUCTION: 358`.

### Asks still outstanding - carried from unit 363's section 4, per HM-DEC-139, verbatim

**Nothing blocks step 3.** Nine new items, all findings; none wants a ruling from the owner. The
carried queue follows them.

### Raised by unit 363

**1. 3.0 was already met before this unit changed anything.**

*A finding, for the record of what advanced the phase.* The trace read decision W's audio at CER
0.0000 on all five seeds through `OliviaDemodulator.Decode` exactly as unit 362 left it: sync S/N
about 9.7, weakest accepted block 7.31 against the threshold's 4.0. So the below-noise claim at
8/250 is the mode's and the existing demodulator's, and this unit's contribution to 3.0 is the
fixture and the test. Unit 361 item 1's improvements were not needed and not tried.

**2. The engine carry-forward invocation now takes 4 m 40 s, and one of its names asserts nothing.**

*A cost, reported.* 124 tests in 1 m 39 s became 134 in 4 m 40 s: every listener row runs the RSID
detector, the streaming search and its readers over a whole file, and `CpuMeasuredAlone` runs those
classes one at a time. It is inside the 480 s timeout and the twelve-minute watchdog, with about three
minutes to spare. **`TheOliviaBelowTheNoiseTests.TheFurtherSeedsArePrinted` is on the line only
because the instruction named the class**, and it asserts nothing - the list's own rule keeps such
names off (`Unit337Measure`). It costs about 85 s. The type-and-method form,
`TheOliviaBelowTheNoiseTests.TheQsoIsReadBelowTheNoise`, would keep the rule and the seconds. Not
changed here, because the numbers above were run on the list as the instruction set it.

**3. A channel's text lags its block by about three blocks.**

*A behavior the rows unit inherits, stated and not measured to the second.* A block is read when
its last frame's segment is decided, and a segment is decided when `TrackSmoothing` = 2 more have
arrived - so text appears roughly two to three blocks (4 to 6 s at the phase's three variants) after
the block ends. The blind channel opened at 10.25 s of audio where the whole-file search named the
carrier at 4.096 s, for the same reason: its trial readers must show two blocks. Shortening the
lag means deciding the track with fewer segments ahead, which trades against 2.7's drift tracking.
The rows unit should measure the lag from the row's point of view, as unit 327 did for PSK31.

**4. A channel's tracked center wanders once its station stops.**

*A finding for 3.4.* On the two-signal file the 16/500 station stops about 7 s before the file ends.
Its channel's track moved one grid step (3.9 Hz) in the silence after that, ending at 2003.91 Hz -
inside the 5 Hz check, but not where the station was. With no retire in this unit, a channel keeps
tracking noise after its station stops. The rows unit may want to report the center as of the last
shown block, or let 3.4's retire rule end it.

**5. Decision AC did not bite.**

*A finding, not a build.* No channel showed a character of the other station: 0 and 0, and each
channel's characters never exceeded what its accepted blocks carry. Unit 362 item 2's per-character
gate stays logged, not chased. Still true and still open: the streaming reader shows a block at the
sync that leads *now*, and cannot take a block back if the sync later moves. That never happened on
any fixture, but it is where a wrong character could come from below the noise.

**6. The streaming reader and `Decode` differ below the noise.**

*A finding, reported.* On every file at -10 dB or better they give identical text; the stream
reports one more rejected block. On the -16 dB file (trace only, not a criterion) the stream read
**CER 0.8566 from 9 blocks** where `Decode` reads 0.9044 from 6 - the local noise measure and the
running sync, not a regression. The -16 dB file was not put through the listener.

**7. The listener is not the app's yet, and has one call the app never makes.**

*A note for the next unit.* It is fed and read like `Psk31Listener`, at the rate `Psk31Resampler`
gives. The differences: it writes its own events rather than handing the shell `States` to write -
though it has `States` too. And it has `Flush()`, which only a recording's end needs. `Channels`
keeps an `Ended` channel listed, as decision AA and the PSK31 rows' "stay after the station ends"
suggest.

**8. The flaky Stop test went red once in the list run and once alone.**

*Carried as known, reported for the count.*
`TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns`: red in
the app list run, red on the first rerun alone, green on the second. No app code changed in this
unit.

**9. Tool facts this session.**

*Reported, not repaired.*
- `sed -i` on `output.md` was refused as "outside the allowed working directories" though the file
  is at the root, and `cat >> docs\carry-forward-tests.txt` was refused the same way. **So the
  carried queue below was never retyped:** unit 362's `output.md` lines 1-294 were removed in place
  with the file editor, and the carried text is the original bytes. Checked by `md5sum`: unit 362's
  lines 295-940, in the working tree and at `60ec790a`, hash `a32bfbd8bb8dad4968059e1ec43be8f8`;
  this file's lines 427-1072, the carried block's first line to its end, hash the same.
- A command joining `grep -v` into a pipe needed approval, and a status-write-and-commit command
  that included one was refused whole, so one commit was made a second time on its own.
- `sh tools/status.sh ... && dotnet test ...` and `&& git ...` ran. `EXECUTING` and `code`
  throughout. `tools/status.sh` still writes `RULES_AT: HM-DEC-161` and `WORK_INSTRUCTION: 358`.
- `.unit362-carry.tmp` is still in the root, ignored by git.

### Asks still outstanding - carried from unit 362's section 4, per HM-DEC-139, verbatim

**Nothing blocks step 2 or step 3's entry.** Seven new items, all findings; none wants a ruling
from the owner. The carried queue follows them.

### Raised by unit 362

**1. 2.3's wording says "tone spacing and symbol rate"; the search measures tone spacing and
occupied band.**

*A mismatch between `PHASE_PLAN.md` 2.3 and work instruction 362 decision P, reported; no ruling
wanted.* For every row in `format.json` the symbol rate is the tone spacing (`variant_rule`:
symbol_seconds = 1 / tone_spacing_hz), so a measured symbol rate says nothing a measured spacing
has not said. The phase's three rows share both, and only the band tells them apart. The trace
measured the symbol period anyway - 30.00 ms against 32 on the no-RSID file, and 32.00 ms on pure
noise, so it is no discriminator. The search follows decision P. If the plan's wording should say
"occupied band", that is the plan's author's edit; this unit did not touch `PHASE_PLAN.md`.

**2. A block can clear the threshold and still show wrong characters.**

*A finding against PSK31 plan §R9 and the prime directive, for step 3 before anything reaches the
screen.* The threshold is a block's mean over its characters, set on noise only (unit 361). Two
cases this unit measured: (a) the drifted file read on one offset put **60 blocks through at a mean
S/N of 31 and read at CER 0.2032** - strong blocks carrying some wrong characters; (b) a wrong
variant puts single blocks of wrong characters through: 16/500 read over 8/250 at 4.10, 4/500 read
over 16/500 at 4.37. The blind search guards itself (two blocks to confirm), and tracking removes
case (a) on the fixtures, but the demodulator on its own would show those characters. **What the
next unit can try**: a per-character gate (each character's own Walsh peak against the block's
noise) rather than the block mean alone, and a threshold checked against wrong-variant audio as
well as noise.

**3. The CPU ceiling is read as process CPU, which counts the tests running beside it.**

*A finding; fixed for this unit's classes, not for unit 361's.* In the carry-forward run the
decode after the blind search read 23.5 s against 5.1 alone and the test went red; the
`CpuMeasuredAlone` collection fixed it for `TheOliviaBlindSearchTests` and `TheOliviaDriftTests`.
`TheOliviaDemodulatorTests` still measures beside other classes - its demodulator figures read
about 8 s in the carry-forward run against about 4 alone - so on a loaded machine it could go red on
2.5 for a reason that is not the demodulator. Putting it in the same collection is one attribute and
costs seconds of wall time; not done here because it is unit 361's class and not this unit's to
change.

**4. The search re-reads from the start every time it takes more audio.**

*A cost, reported.* On a clean carrier it names in 4 to 8 s of audio and under 2 s of CPU. On the
-10 dB file it named 16/500 correctly but only after 102.4 s of audio and 17.8 s of CPU, because at
2.48 dB over the floor the spectrum measurements are poor (5 tones, 144.53 Hz band) and each new
2 s of audio repeats the whole look. Step 3 will want it fed a stream; the fix is to keep the
running averages and trial decodes, not re-derive them.

**5. The search finds nothing at -16 dB.**

*A finding, not a criterion.* The loudest bin stands 0.87 dB over the passband median across the
whole file, under the gate, so no row is tried. 2.3 asks only for the no-RSID 8/250 file, which is
clean. Criterion 3.0's -14 dB 8/250 fixture will be the first test of the search below the noise.

**6. The offset track follows up to two tone spacings and one grid step a block.**

*A limit, stated.* `TrackTones` = 2 and one 3.9 Hz step per block (2 s at the phase's variants) let
it follow up to about 100 Hz a minute and about 62 Hz in total from where it started. A carrier
that starts more than half a tone from the center it is given is still read off its tones, as
before, and the search's measured middle is what keeps a blind start inside that half tone.

**7. Tool facts and status words this session.**

*A finding, reported and not repaired.*
- A command joined with `;` ran. `grep -v "^\s*$"` and `sed -n '/a/,/b/p'` in a pipe after
  `dotnet test` each needed approval; `grep -E`, `tail` and `sed -n N,Mp` after `git show` did not.
- **Python did not run this session**, against unit 361: `python` on a script in the root (named
  `.unit362-carry.tmp` so `*.tmp` ignores it) needed approval, as did `mv`, `tee -a output.md`,
  `git check-ignore`, and command substitution; `git show ... | tail >> output.md` was blocked as
  output redirection. **The carried queue below was therefore copied in with the file editor** and
  checked with `sed -n N,Mp | md5sum` against `6d9cf1e6:output.md`: its lines 269-275 and 276-820
  match this file byte for byte on each side of the one mark. The unrun script,
  `.unit362-carry.tmp`, is left in the root, ignored by git; `rm` is refused.
- Every status write used `EXECUTING` and `code`. `tools/status.sh` still writes
  `RULES_AT: HM-DEC-161` and `WORK_INSTRUCTION` from `PHASE_STATUS.md`, which still says 358.
- `PHASE_PLAN.md` still shows 2.3 and 2.7 unchecked; this unit did not edit it. Marking them, and
  step 2's state, is the arbiter's.
- The MCP connectors for Gmail, Google Calendar and Google Drive reported that they need
  authorization in claude.ai's connector settings. Nothing in this unit used them.

### Asks still outstanding - carried from unit 361's section 4, per HM-DEC-139, verbatim

The words below are unit 361's, from its line under `## 4. What's blocking us` to its end, as
committed in `6d9cf1e6`. Only that top-level heading is dropped, so this report keeps four
sections. Its nested queues are carried as unit 361 carried them, and the queue of units 337 to
353 is still carried by reference to `4c55deac:output.md`. **One item is marked in place - unit
361 item 1 - and nothing is deleted.** This unit answers none of the others.


**One criterion is not met: 2.2 at -16 dB.** Five new items, all findings; none wants a ruling
from the owner. The carried queue follows them.

### Raised by unit 361

**1. The -16 dB fixture is not read at CER 0.10, and no threshold would read it.**

*ANSWERED by `PHASE_PLAN.md` §8, the revision of 2026-09-19 - the -16 dB ceiling was the plan
author's own error, set below the mode's published sensitivity for 16/500, and is withdrawn;
2.2 now asks for the number measured and reported with no ceiling, which unit 361 measured at
0.9203, and 2.2 is checked met. Work instruction 362 task 2 makes the test say what the
criterion now says.* (Marked in place by unit 362, as work instruction 362 section 3 directs.)

*A finding; 2.2 reported not met, the test not loosened.* Measured, the file carries -17.19 dB in
2500 Hz (the -10 dB file measures -10.48 by the same method), Es/N0 1.84 dB per symbol. With every
block accepted the demodulator reads it at CER 0.3267; at the 4.0 threshold, which noise never
clears, 0.9203. Tried and kept: noncoherent likelihoods, three iterative passes, eight frames per
symbol. **What the next unit can try**: timing and frequency tracked per block rather than once
per file; the sync decided per block from the neighbors' likelihoods; soft combining of the two
frames either side of the chosen one. Whether the fixture's figure is reachable by any Olivia
decoder was not measured here; the fixture is the mode author's and is not in question (§6).

**2. The demodulator runs over a whole recording, not a stream.**

*A finding for step 3.* `Decode(MonoAudio, startSeconds)` finds one offset, one symbol phase and
one block phase for the whole recording. That is what step 2 asks for and what the fixtures need;
step 3's rows per station will need it fed as the RSID path feeds the detector.

**3. The fixture test runs the RSID detector over the whole file first.**

*A cost, reported.* About ten seconds of CPU on the 131 s files before the demodulator's four, so
the four demodulator names add about a minute to the engine carry-forward (1 m 14 s, 119 tests).
The 20 s ceiling is the demodulator's own and is met with room.

**4. `RsidDetection` carries the first tone's start, not the burst's end.**

*A mismatch with the work order's task 1 item 4.* The end is derived from `rsid-codes.json`
(`StartSeconds + Symbols / SymbolRateHz`), which the tests do in one helper.

**5. Tool facts and status words this session.**

*A finding, reported and not repaired.*
- Python scripts written to the scratchpad and run as `python file.py` ran; unit 360 reported
  Python did not run. `python -c` was not tried.
- A quoted heredoc containing apostrophes broke once, as the work order warned. A `sed`
  substitution with backslashes in the pattern (the csproj line) matched nothing and reported
  nothing; it was redone with the editor.
- Every status write used `EXECUTING` and `code`. `tools/status.sh` still writes
  `RULES_AT: HM-DEC-161` and `WORK_INSTRUCTION` from `PHASE_STATUS.md`, which still says 358.
- `PHASE_PLAN.md`'s unchecked 1.5 and 1.7 and the mislabeled outcome entries are unchanged and not
  edited, as told.

### Asks still outstanding - carried from unit 360's section 4, per HM-DEC-139, verbatim

The words below are unit 360's, from its line under `## 4. What's blocking us` to its end, as
committed in `dcffd02c`. Only that top-level heading is dropped, so this report keeps four
sections. Its nested queues are carried as unit 360 carried them, and the queue of units 337 to
353 is still carried by reference to `4c55deac:output.md`. **One item is marked in place - unit
358 item 5 - and nothing is deleted.** This unit answers none of the others.


**Nothing blocks 1.5 or step 2's entry.** Five new items, all findings; none wants a ruling.
The carried queue follows them.

### Raised by unit 360

**1. `longestSeconds` on a typed line's record says 30 when the send was held to 60.**

*A finding, not repaired.* `TransmitRecord.ToBag` writes `OperatorSend.LongestUnslottedSeconds`
for every no-slot send (`TransmitRecord.cs`, the `longestSeconds` line), not the send's own
`Cap`. The typed record in section 3 shows it: `longestSeconds: 30` on a send held to 60. This
is older than this unit (work instruction 357 moved the cap onto the send and did not move
this). Decision F named the fields this unit adds, and this is not one of them. A later unit can
carry `Cap` through `Recorded` the way this one carried the announcement.

**2. The sequence's refusal sentence quotes the whole audio, not the text the cap measured.**

*A finding, not repaired.* `SendableWithNoSlot` says *this is {audio.Seconds} s of PSK31 audio,
and this send may be at most {Cap} s*. Since R32 (a), the number compared to the cap is
`TextSeconds`, so an announced refusal quotes 1.86 s more than was measured. The sentence is
still true about the audio. Changing it touches `Ft8TransmitSequence` beyond decision F's one
change, so it was left.

**3. `WhereTheTransmissionStartsAndWhatTheRecordSaysTests.ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt`
is red, and older than this unit.**

*A finding, not repaired.* It asserts `Assert.Single` over every event a slotted FT8 run
writes. Since `7de5018b` (every stage writes `send_stage`), the run writes four `send_stage`
events and then the record, so it fails on the count before it reaches the record's shape. It
is not on the carry-forward list. This unit's filter picked it up by name, and this unit
changed no stage.

**4. The typed line's card rounds to *60 s of text* for a line that goes.**

*A finding for whoever next touches the card.* 59.968 s prints as *60 s of text* beside *the
most a typed line may be is 60 s*, and it sends. One character more prints *60.1 s*. The card
and the gate agree, as the test proves, but the word does not show the margin.

**5. Tool facts and status words this session.**

*A finding, reported and not repaired.*
- Needed approval and not run: `python -c`, `jq`, `git restore --source`, `git checkout <rev> --
  <file>`, `git stash push`, `awk`, and `tools/status.sh` run directly (not through `sh`) when
  joined by `&&`. `sh tools/status.sh` alone, and joined by `&&` to `git` and `dotnet test`, ran.
  Earlier units report that Python ran; it did not run this session.
- `sed -n` inside a pipe after `git show` ran. `tail -n +N file | md5sum` ran, and was used to
  check that the carried queue below is byte-identical to `db3edfa1:output.md` from its unit 358
  heading to its end.
- **The first five status writes this session read `STATE: WORKING` and `BALL: claude`**, which
  are not allowed words (`CLAUDE.md` §13.1). Every write from the sixth on, during task 1, used
  `EXECUTING` and `code`. `tools/status.sh` still writes `RULES_AT: HM-DEC-161`, and `WORK_INSTRUCTION` is read
  from `PHASE_STATUS.md`, which still says 358.

### Asks still outstanding - carried from unit 359's section 4, per HM-DEC-139, verbatim

The words below are unit 359's, from its line under `## 4. What's blocking us` to its end, as
committed in `db3edfa1`. Only that top-level heading is dropped, so this report keeps four
sections. Its nested queues are carried as unit 359 carried them, and the queue of units 337 to
353 is still carried by reference to `4c55deac:output.md`. **Two items are marked in place and
nothing is deleted.** This unit answers none of the others.

Nothing blocks step 2's entry: the clean 16/500 burst is detected. Five items. **Item 1 wants
Tim, because it changes what a send can be. Item 2 is stop material the unit did not build.**

**1. The report macro to a compound callsign no longer fits its cap with the burst in front.**

*ANSWERED by R32 (a), Tim 2026-09-18 - the burst does not count against the cap; built in work
instruction 360 task 2.*

*Raised for the next arbiter to take to Tim as a §R10 question, as decision A directs.* The four
macros as §R2 writes them fit, so task 4 was built.

**The numbers:** the report to VP2V/W1AW with the default name and place is 28.192 s. With
Hamlet's burst it is **30.050 s, over the 30 s cap by 0.05 s**, so that send is now refused with
its length where it went before. It would be 29.585 s with the tones and no leading silence.
Unit 318 chose thirty so that this report would fit, with 1.8 s to spare. The burst takes that
margin and 0.05 s more.

**Options:**
- *A, as built:* the burst counts inside the cap, and a long report is refused in words. Nothing
  keys; the operator shortens a Settings field.
- *B, Tim raises the macro cap past thirty.* §R10 says *not more than thirty*, so only he can.
- *C, the arbiter's to decide:* drop the five silent symbols in front of a PSK31 send's burst,
  since the transmitter is keyed for 0.46 s of nothing. That fits at 29.585 s, but Hamlet's burst
  would no longer be the file's shape.

**Recommended:** A until Tim says otherwise. The cap is transmit safety, and C leaves 0.4 s of
margin.

**2. The transmission record does not say the send was announced.**

*ANSWERED by R32 (b), Tim 2026-09-18; built in work instruction 360 task 3.*

*Stop material under task 4's fence, so not built.* Criterion 1.5 says *the transmission record
says so*. `ft8_transmission` is built only in `Ft8TransmitSequence.Recorded`, from
`send.Unslotted`'s mode, fit and seconds. Carrying `AnnouncedCode` into it takes one named
argument there and one optional field on `TransmitRecord`: a change to `Ft8TransmitSequence`,
which the instruction forbids. **What was built instead:** `psk31_send_composed` carries
`announced` and `rsidCode`, and `rsid_sent` follows the keying. **To license it, say** *add
`announcedCode` to the no-slot transmission record*. The slotted branch would not change, and
`TheFt8AndFt4SendsAreByteIdenticalTests` pins it.

**3. The fldigi commit is not recorded, and codes 72 to 75 stay undetected.**

*A finding, not a stop.* The session could list nothing outside `C:\Source\HamLet`, so §R5's pin
is still owed and decision C's table could not be added. Step 2 reads `pj_mfsk.h` from the same
clone and will meet the same wall. A launcher that grants read access to `C:\Source\fldigi`, or a
commit hash written into the next instruction, would close it.

**4. Hamlet's own answer is now 1.86 s longer than the turn timing thinks.**

*A finding for step 4's timing work.* `Psk31Macros.AnswerSeconds`, the §R18 stated equivalent of
one FT8 slot, still reads `Psk31Modulator.SecondsFor`, the text alone. Nothing in this unit was
licensed to move turn patience, and it now undercounts Hamlet's own answer by the burst.

**5. One Stop test went red once in a combined run, on an FT8 send.**

*A finding that repeats unit 355 item 6 and unit 357 item 1.*
`TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns` wrote
the abort frames twice in a run of seven types. It passed alone on the first rerun and in the
final 165 run. It drives a slotted FT8 send, which nothing in this unit touches.

### Asks still outstanding - carried from unit 358's section 4, per HM-DEC-139, verbatim

The words below are unit 358's, from its line under `## 4. What's blocking us` to its end, as
committed in `1444962f`. Only that top-level heading is dropped, so this report keeps four
sections. Unit 357's, 356's, 355's and 354's queues are inside it, as unit 358 carried them. The
queue of units 337 to 353 is carried by reference to `4c55deac:output.md`. **This unit answers
none of them.** Unit 358's item 1, the dial 1500 Hz below the center, is logged and not reopened:
step 0 is closed.

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

*ANSWERED by work instruction 360 task 4 - parsed by System.Text.Json, agrees with the
manifest except 8/250 no-RSID per-character rounded up (0.686 against 0.685).* (Marked in place
by unit 361, as work instruction 361 section 3 directs.)

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

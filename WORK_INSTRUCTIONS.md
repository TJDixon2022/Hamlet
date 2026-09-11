# Work instruction 314 - hear one: the PSK31 tab stops pretending, then reads a signal

**PSK31 phase, step 1.** Step 0 is `done` (unit 312). Unit 313 was carried repair.

---

## 0. The project gate

**This instruction is for Hamlet and nothing else.** Confirm the tree:

| check | expected |
| --- | --- |
| `SHACK_FACTS.md` | exists at the root |
| `src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` | exists |
| `CoreHMI.sln` | **must not exist** |
| `MURC.sln` | **must not exist** |
| root | `C:\Source\HamLet` |

The extraction gate beside the zip already ran these four. **If any is wrong now, stop
and say so in `output.md` section 4. Write nothing else.**

---

## 1. The two rules that killed sessions

Both HM-DEC-155, Tim, 2026-09-05.

**1. A unit runs no test suite.** Not `dotnet test`, not a whole project, not a whole
type unless the unit wrote the whole type. **Only this unit's own test names, filtered by
exact name, foregrounded, with a stated timeout:**

```
timeout 480 dotnet test <project> --filter "FullyQualifiedName~TypeName.MethodName"
```

Known reds you did not cause are in section 10. **An unfiltered run finds them, spends
twenty minutes, and tells you nothing about your work.**

**2. Never background a command and poll it.** No `&`, no `start`, no `nohup`, no loop
that sleeps and checks. **The watchdog fires at twelve minutes with no status write.**
**A demodulator test over a 66-second fixture at 8 kHz is not a long test** - it is half
a million samples and should run in well under a second. If one takes longer than a few
seconds, that is a finding about the code, not a reason for a longer timeout.

---

## 2. The tool fact

**The shell here breaks on an apostrophe inside a quoted heredoc, and it collapses a
doubled backslash.** Write *do not* rather than `don't` inside a quoted heredoc. Write
single backslashes in paths, or forward slashes. **Check what landed on disk rather than
what you typed.**

---

## 3. Asks still outstanding

Carried per HM-DEC-139. **All of these come back in `output.md` section 4, verbatim where
unresolved.** Do not answer them yourself; do not delete one because it looks stale.

1. **Does the transmission record ask the radio whether it keyed?** Unit 303's proposal,
   still Tim's. PSK31 sharpens it: a continuous carrier that did not key is a long
   silence, not a missed slot.
2. **Nothing in this repository can look at a picture.** `Avalonia.Headless.Skia` is
   Tim's to add (§0.4).
3. **Three inherited reds, never chased.** Two in `TheAchievementsScreenTests`, one in
   `TheFitGuardAsksAboutTheGridTheSendIsOnTests` (engine).
4. **Where the explanatory hover wording lives.** `Ft8ContactCard.Closing` is uncalled.
5. **`Ft8GlobePlot`'s unused framing constants.** Report; leave standing.
6. **The licence of `assets/world-flat-relief.png` is unknown.** Raise; do not resolve.
7. **`PHASE_OUTCOME.md`** - this phase's record starts at unit 312; 306, 307, 309 and
   310 are in the FT4 phase's file in git history. Not to be back-filled.
8. **The door sentence is a placeholder.** Carry.
9. **Acknowledgement indicators.** Named by Tim, not yet defined. The *say it* unit
   builds a turn indicator that may be what he meant. Do not assume it is.
10. **Card ordering under scroll.** Raised three times, unruled. **Unit 313 found the
    root**: `DigitalCards.Clear()` then new `Ft8ContactCard` objects every slot, so all
    per-card state is lost four times a minute, including `MapIsOpen`. Not fixed. Not
    this unit's. Carry.
11. **The outline width on the neighbourhood map is 2 px**, a number unit 312 chose.
12. **Version numbering** - 1.13.0 versus a strict HM-DEC-150 reading. Patch bump here.
13. **`PHASE_PLAN.md` §R6 says 14.0700-14.0725; the cited row says 14.070-14.074. The
    row wins.** Prose to correct when the plan is next touched.
14. **A larger source bitmap** would make the popup's zoom cap unnecessary. Tim's.
15. **One conversation card is taller than the panel** (293 px in 220 px). Not broken.
16. **The popup zoom cap of 2.0x is the author's number.** `Ft8GlobePlot.ZoomCap`.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet works PSK31 the way it works FT8.
UNIT GOAL:  Step 1 - hear one. A single-channel BPSK demodulator and varicode
            decoder, proved against shipped fixtures with a stated character
            error rate, and wired so that pressing PSK31 shows text arriving.
            First, the PSK31 tab is made inert, because today it transmits FT8.
ADVANCES:   Step 1, wholly if tasks 2-4 land; task 1 is a step-0 defect repaired
            in passing and is not scored as an advance.
DRIFT:      1 carried from unit 313. Expect 0 if step 1 moves to done.
```

### What Tim did, and what happened

Tim, 2026-09-11: *"I clicked PSK31, saw the radio move. Saw one CQ (in 10 minutes), did a
respond, got nothing back."*

There is no PSK31 decoder in the tree. So the CQ he saw was **FT8's decoder, still
running under the PSK31 tab**, hearing either a real FT8 signal that wandered into the
passband or a false decode out of noise. And his response **sent an FT8 message on
14.070**, into the PSK31 watering hole, where nobody was listening for it. **Unit 312
fixed the sentence on the panel and left the machinery connected.** That is the
instruction's fault: it said what not to build and did not say to disconnect what was
already there. Task 1 disconnects it. **It comes before the demodulator because a tab
that can transmit the wrong mode into the wrong segment is worse than a tab that does
nothing.**

Also from Tim, on the CQ receipt (2026-09-11): *"CQ should not have log option, that is
self-gratification."* **Ruling: the receipt carries no Log.** Unit 305's *Log from first
appearance* and R2's Log were the author's proposals; both are withdrawn. A CQ is not a
contact; there is nothing to log until someone answers, and then the Log belongs on his
conversation card. Task 1 takes it off.

### What step 1 is

**PSK31 on the wire.** Binary phase-shift keying at **31.25 baud** - 256 samples per bit
at 8 kHz - carrying text in **varicode**, a self-delimiting variable-length code where
common letters are short: space is `1`, `e` is `11`, `t` is `101`. Every code starts and
ends with `1` and none contains `00`, so **`00` is the character separator** and a
decoder needs no framing beyond splitting on it. The keying is **differential**: a `0`
bit is a phase reversal, a `1` bit is no change, so the receiver compares each bit to
the one before and never needs absolute phase. Idle is a run of `0`s - continuous
reversals - which is what a station sends between characters and before it starts
typing. The envelope is **raised-cosine shaped** so it passes through zero at every
reversal, which keeps the signal 31 Hz wide instead of splattering.

**So a single-channel demodulator is:** mix the audio down at the chosen offset to a
complex baseband; low-pass or matched-filter it to about the bit rate; recover the bit
clock - the envelope dips to zero at reversals, and the symbol centres are the points of
maximum energy; sample once per bit; multiply each sample by the conjugate of the last
to get the differential phase; the sign of the real part is the bit; split the bit
stream on `00`; look each code up in the table. Plus two things a real signal needs:
**AFC**, because a real carrier drifts and the operator did not tune to the exact hertz,
and **a squelch**, because the same machinery run on noise produces characters -
`assets/fixtures/README.md` records that the author's unsquelched reference decoder
**emitted 112 garbage characters from thirty seconds of Gaussian noise.**

### The fixtures, and where they came from

`assets/fixtures/` - seven WAV files at 8 kHz mono, a `manifest.json` with SHA-256,
duration, carrier, the exact text, and the **author's reference decoder's character
error rate for each**, and `qso-text.txt`. **Generated by `assets/reference-modem.py`
on a machine with no radio, FACT-004.** The QSO text is a four-line standard exchange
between `KC3QIS` and `W1AW` - 255 characters, 66.6 seconds at 31.25 baud.

| file | what it proves |
| --- | --- |
| `psk31-clean-1000hz.wav` | the decoder works at all |
| `psk31-snr+10db-1000hz.wav`, `+3db`, `-3db` | it works with noise on it |
| `psk31-drift-1000-to-1020hz.wav` | AFC holds a carrier that drifts 20 Hz in a minute |
| `psk31-noise-only-30s.wav` | **the squelch produces nothing from nothing** |
| `psk31-two-signals-1000-1500hz.wav` | a single channel at 1000 Hz ignores a signal at 1500 Hz |

**SNR is referenced to 2500 Hz**, the way an FT8 report is. **These are not weak-signal
fixtures** - `-3 dB` in 2500 Hz is about `+16 dB` inside PSK31's own 31 Hz. Weak-signal
work wants real off-air audio, which only Tim can record, and is raised in section 4.

**The varicode table** is `assets/varicode.csv`, 256 rows, extracted mechanically from
fldigi's `src/psk/pskvaricode.cxx` with the citation in `assets/varicode-SOURCE.md`. It is
the published G3PLX table; fldigi is the citation for the exact bits. **Transcribing it
into the tree with that citation is data, not a port.**

**`assets/reference-modem.py` is the author's, not fldigi's.** It shows the modulation
convention the fixtures were made with. Read it for the convention; **do not port it** -
it has no squelch, a crude matched filter, and a squaring AFC that only works offline.
The convention it documents is the thing to match.

---

## 5. Verify this instruction against the tree

**Everything below about existing code comes from units 312 and 313's reports.** Check
and **report every mismatch; do not repair this instruction; do not stop over a
mismatch** unless a task is impossible.

- `DigitalModeChip.cs:61` `Labels`; `MainWindowViewModel.cs:1109` `ChooseDigitalModeAsync`;
  `DigitalCallingFrequencies.Find(band, label)`; `ModeFollowPlan.cs:219`;
  `ContactModes.cs:147` (`MODE=PSK`, `SUBMODE=PSK31`); the engine's `DigitalMode` with two
  members and `DigitalModeFor`, which **maps PSK31 onto `Ft8`** - that mapping is the
  fault task 1 removes; `DigitalIdleText.ModeStripFor(grid)`; `_digitalMode.ToString()`.
- The FT8 audio source and where the FT8 decoder is fed from it - task 4 feeds the PSK31
  demodulator from the same source.
- `Ft8ContactLedger.CallToAnyone`; the CQ receipt as unit 310 built it, with its Log.
- `docs/psk31-reference.md` - the fldigi pin at `61b97f41`, sparse clone at
  `C:\Source\fldigi`.
- Tests: `ThePsk31SeamTests` (7), `ThePsk31ReferenceIsPinnedTests` (2), `TheCqReceiptTests`,
  `ThePanelHoldsThemAllTests`, `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint`,
  `docs\carry-forward-tests.txt`.

---

## 6. Rulings in force

**Tim, 2026-09-11 - the receipt carries no Log.** *"CQ should not have log option, that is
self-gratification."* Supersedes unit 305's *Log from first appearance* and R2's Log.

**`PHASE_PLAN.md`** at the root. §1: PSK31's contact is a conversation, not a protocol.
§3: no slot clock, RST not dB, continuous carrier, characters not messages, no fixed
length. **§R1**: strict on anything that drives a transmission, permissive on display,
guessed states marked - **nothing in this unit displays a state; it displays text.**
**§R3**: the parser is a later step; this unit parses nothing. **§R5**: fldigi is read,
never ported wholesale. **§R6 as corrected by ask 13**: the row says 14.070-14.074.

**§0.0 / HM-DEC-092 - never present a guess as a decode.** For a text mode this is the
squelch: **a character that the demodulator was not sure of is not shown.** No partial
credit, no dimmed maybe-characters, no `?` in place of a doubtful one. Silence.

**§0.2 - one click, one transmission.** Task 1 makes the PSK31 tab unable to transmit
anything. Nothing in tasks 2-4 adds a click that transmits.

**§0.1** - the engine is never told that tabs exist. The demodulator takes audio and an
offset; it does not know it is behind a tab.

**Tim, 2026-09-06 - the dummy load is withdrawn in full.** No compensating control.

**HM-DEC-054** - the band row is cited data. **HM-DEC-155** - section 1.
**HM-DEC-139** - the asks queue.

**FACT-004** - a dev-machine result is an indication, never a finding. **FACT-006** -
this machine has no radio. **Every fixture and every reference number in this
instruction is an indication.**

---

## 7. Status cadence

`PROJECT_STATUS.md` per `CLAUDE.md` §13: **after every task, and at least every ten
minutes.** Write the status **before** starting a test run.

---

## 8. The tasks

Five. Each names the test to watch failing first and one drop candidate. **Drop from the
back - but task 1 is never dropped.**

### Task 1 - the PSK31 tab is inert, and the receipt loses Log

**This task comes first and cannot be dropped**, because until it lands the PSK31 tab can
transmit FT8 into the PSK31 segment.

**Trace first.** Append `UNIT 314` to `PHASE_OUTCOME.md` under step 1. Patch-bump the
version. Run `docs\carry-forward-tests.txt` filtered before changing anything. Then with
file and line: **where does the FT8 decoder get attached when PSK31 is chosen** - the
`DigitalModeFor` mapping onto `Ft8` and whatever consumes it - and **where does a click
on a decoded row reach the send path** under that tab?

**Then disconnect.** With PSK31 chosen:

- **no FT8 decoder runs.** Not hidden - not running. The slot grid is not started.
- **the decoded list is empty** and the CQ filter shows nothing, until task 4 gives it
  PSK31 text.
- **no row is clickable and no path from that tab reaches anything that keys the
  transmitter.** The CQ button under PSK31 does nothing and says why, in the voice the
  readiness hover uses.
- FT8 and FT4 are unchanged. Assert it.

**And the receipt.** Remove the Log option from the CQ receipt. What remains is what Tim
ruled: `Calling`, the sentence, the time, the dismiss X, and *show the messages* if it is
already there. **Nothing else comes off and nothing goes on.**

**Test watched failing first:** `ThePsk31TabIsInertTests`, app project. Watch it fail,
then green: no decoder attached under PSK31; no slot grid started; the list empty; no row
command enabled; the CQ press under PSK31 reaches no send path; FT8 and FT4 unchanged.
Extend `TheCqReceiptTests` by one: the receipt exposes no Log command. Re-run
`ThePsk31SeamTests` and `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint`
filtered.

**Drop candidate:** none. **Not droppable.**

---

### Task 2 - the varicode table, cited, and a codec over it

`assets/varicode.csv` into the engine as data with `assets/varicode-SOURCE.md` as its
citation - the way unit 252 carried the DXCC table. A `Varicode` type that encodes a
character to its bits and decodes a bit stream to characters by splitting on `00`.

**Test watched failing first:** `TheVaricodeTests`, engine project. Watch it fail, then
green:

1. all 256 codes round-trip
2. no code contains `00`; every code starts and ends with `1`; all 256 are distinct
3. space is `1`, `e` is `11`, `t` is `101`, `o` is `111`, `a` is `1011`
4. the QSO text in `qso-text.txt` encodes to a bit string and decodes back identical,
   with `00` between every character
5. a stream with a run of idle `0`s before and after the text decodes to the text alone

**Drop candidate:** assertion 5.

---

### Task 3 - the demodulator, proved against the fixtures

`Psk31Demodulator`, engine, **§0.1 - it takes samples at 8 kHz and an offset in hertz
and yields characters; it knows nothing about tabs, radios or panels.** One channel.
Fed a stream, it emits characters as they complete.

**What it must do, per section 4:** mix down at the offset; low-pass or matched-filter
to the bit rate; recover the bit clock from the envelope; sample once per bit;
differential detect; split on `00`; look up. **AFC**: track the carrier so a drift of
20 Hz in a minute is held. **Squelch: state the rule you chose** - signal quality,
envelope depth at reversals, differential phase confidence, whatever it is - **and the
number**, and why. The rule must make the noise-only fixture produce **nothing**.

**Character error rate** is edit distance between the decoded text and `qso-text.txt`,
divided by the length of the reference, after trimming leading and trailing idle. The
reference decoder scored **0.0000 on every QSO fixture**; the ceilings below are
deliberately looser than that because a squelched, streaming decoder with a real AFC
loop is a harder thing than an offline reference, and **a demodulator that hits these
numbers has met step 1**.

**Test watched failing first:** `ThePsk31DemodulatorTests`, engine project. Watch it
fail, then green:

| fixture | must |
| --- | --- |
| clean | CER ≤ 0.01 |
| +10 dB | CER ≤ 0.02 |
| +3 dB | CER ≤ 0.05 |
| -3 dB | CER ≤ 0.10 |
| noise-only | **zero characters emitted** |
| two signals, channel at 1000 Hz | CER ≤ 0.05 against the QSO text, and **none of `EI4GNB`'s text appears** |
| drift | CER ≤ 0.05 with AFC |

Plus: every fixture's SHA-256 matches `manifest.json` before it is used; the squelch rule
and its number are asserted to exist in one named place; and **each fixture decodes in
under five seconds** on this machine, reported as a number.

**Report the CER you got on every fixture as a number, beside the reference's.** If a
ceiling is missed, **say so and ship the decoder anyway** - a decoder at CER 0.12 on the
-3 dB fixture is a finding about the -3 dB fixture, not a reason to withhold the mode.

**Drop candidate:** the drift row and the AFC. Ship without AFC and say so; a real
station will then need to be tuned within a few hertz, which is the *hear everyone* unit's
problem to remove.

---

### Task 4 - pressing PSK31 shows text arriving

Wire the demodulator behind the PSK31 tab, **fed from the same audio source the FT8
decoder uses**, at a fixed offset of **1000 Hz above the dial** - so 14.071.000 with the
dial on the cited 14.070.000. One channel. Text appears in the decoded-text panel **as
characters arrive**, one line per signal, the way the *hear everyone* unit will need it,
even though there is only one signal for now.

**The panel line changes** from *Hamlet cannot read PSK31 yet* to a line in the same
voice saying it is listening at one spot and what that spot is. **`VoiceTests` runs**
because this adds copy.

**What is not built:** no rows to click, no parser, no CQ filter behaviour beyond
empty, no cards, no transmit. **The tab stays inert for sending.** Task 1's assertion
that no path reaches the send path is re-run after this task.

**Test watched failing first:** `ThePsk31PanelHearsTests`, app project. Watch it fail,
then green: with the clean fixture played through the audio source, the panel's text
accumulates the QSO text within CER 0.01; the line names the listening spot; no row is
clickable; `ThePsk31TabIsInertTests` still green.

**Drop candidate:** the panel line change. Keep the wiring; leave the old line and say so.

---

### Task 5 - a synthetic weak-signal fixture

**Drop candidate: this whole task.**

Using the convention in `assets/fixtures/README.md`, make one more fixture at **-10 dB in
2500 Hz** (about +9 dB in 31 Hz) and report the decoder's CER on it as a number, **with no
ceiling asserted**. This is a measurement, not a gate. Record it in `manifest.json` with
its SHA-256.

**Test watched failing first:** extend `ThePsk31DemodulatorTests` by one: the fixture
exists and its hash matches. No CER assertion.

**Drop candidate:** the whole task.

---

## 9. Parked

Not in this unit. Do not start any of it.

- **Hear everyone** - finding signals across the passband. The demodulator is one
  channel at one offset.
- **Read the conversation** - the parser, callsigns, turnover words, states.
- **Say it** - the modulator, the macros, the turn indicator, the `SlotClock` replacement.
  **The transmit chain is not touched.**
- **Log and achievements** - RST, the mode's records.
- **Real off-air audio.** Only Tim can record it. Raised, not done.
- **The card-rebuild root** (ask 10). Not this unit's.
- **Acknowledgement indicators** (ask 9). Build nothing.
- **Reading anything from fldigi other than the varicode table already extracted.**
- **`Avalonia.Headless.Skia` or any other package** (§0.4).

---

## 10. What not to do

- **No unfiltered `dotnet test`.** No whole project, no whole solution.
- **Never background a command and poll it.**
- **Do not touch anything that keys the transmitter.** The proved chain
  `cq_pressed → … → ft8_transmission Played` stays as it is. **Task 1 makes the PSK31
  tab unable to reach it; nothing later in this unit reconnects it.**
- **The dummy load is withdrawn in full** (Tim, 2026-09-06). **No compensating control.**
- **Do not show a character the demodulator was not sure of.** No `?`, no dim maybe.
- **Do not port `reference-modem.py` or anything from fldigi.** Match the convention;
  write Hamlet's own.
- **Do not hard-code 14.070.** The offset is 1000 Hz above whatever the cited row says.
- **Do not add a row to `data/bands/`.**
- **Do not add or vendor a package.** No FFT library, no DSP package - the engine already
  has what FT8 and CW use.
- **Do not claim an appearance from computation and call it seen.** Say *computed*.
- **Do not invent a ruling id.**
- **Do not chase these known reds:** `CwAdjudicationTests.ASpeedChangeInRealisticAudio`;
  the 51 CW cases in `docs\unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests`
  whole-type-list tripwire; `HM-OPEN-088`'s ten; the two in
  `TheAchievementsScreenTests`; the one in
  `TheFitGuardAsksAboutTheGridTheSendIsOnTests`.
- **Do not repair this instruction.** Report mismatches; keep working.
- **Do not write to `DECISIONS.md`.** §12.1.

---

## 11. Reporting

`output.md` at the repository root. **Canonical headings:** `## 1. What Claude did`,
`## 2. What the owner should expect`, `## 3. What you should see`,
`## 4. What's blocking us`.

Open with the A/B/C ordering block, then the header:

```
A. The phase goal - ...
B. Step 1 and its exit criteria - ...
C. The report last, and section 4 raises N items on top of a carried queue of sixteen.
```

```
UNIT:       314 - <complete|stopped> at task N of 5, <which dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no, and which step>
NUMBER:     <what moved, before -> after>
DRIFT:      <n> consecutive units without advance  (was 1)
```

**Section 1 must carry a table: every fixture, the reference CER, your CER, and the
decode time in seconds.** And the squelch rule with its number in one sentence.

**Section 2 must tell Tim what he will see when he presses PSK31 now, and what he must
not expect** - one signal at one spot, no clicking, no sending - and **ask him for an
off-air recording**: a few minutes of audio on 14.070, in words that say what to do.

**Every appearance claim in this report is computed, not seen. Say so once, plainly.**

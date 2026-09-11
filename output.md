# Work instruction 314 - hear one: the PSK31 tab stops pretending, then reads a signal

**READ IN THIS ORDER.**

A. **The phase goal - Hamlet works PSK31 the way it works FT8.** A third digital mode
   with the same two cards, the same one-click exchange, the same log and the same
   achievements, on a modem this project writes itself.

B. **Step 1 and its exit criteria** - a recorded fixture decodes to its known text with
   the character error rate stated; the varicode table is cited; the reference commit is
   recorded and the clone is outside the tree; the squelch rule is stated and a
   noise-only fixture produces no text. Nice: a carrier drifting 20 Hz in a minute holds
   lock. **All four must-pass are met and so is the nice-to-pass. Step 1 is `done`** -
   and every fixture is synthetic and nothing was measured at a radio.

C. **The report last, and section 4 raises 5 items** on top of a carried queue of
   sixteen.

```
UNIT:       314 - complete at task 5 of 5, none dropped - 2026-09-11 15:42
PHASE GOAL: Hamlet works PSK31 the way it works FT8 - the same cards, the same one
            click, the same log, on a modem this project writes itself.
UNIT GOAL:  Stop the tab decoding and transmitting FT8 on a PSK31 frequency, then
            build a demodulator that actually reads the mode and put its text on
            the panel.
ADVANCED:   yes - step 1, wholly. Task 1 is a step-0 defect repaired in passing and
            is not scored as the advance.
NUMBER:     PSK31 characters read 0 -> 255 of 255 at CER 0.0000 on six fixtures;
            slot looks under PSK31 30 -> 0; version 1.13.1 -> 1.13.2
DRIFT:      0 consecutive units without advance  (was 1)
```

**Every appearance claim in this report is computed, not seen.** Nothing in this
repository can look at a picture. And **nothing here is evidence about the radio**: this
machine has none, every fixture is synthetic and made by the author's own modem, so all
of it is an indication rather than a finding (FACT-004, FACT-006).

## 1. What Claude did

**The gate passed on all four checks.** Branch **`main`**, seven commits, each pushed
before the next task started. Version **1.13.1 → 1.13.2**.

### The fixtures, measured

**Every fixture's SHA-256 is checked against `manifest.json` before a sample is read.**

| fixture | reference CER | Hamlet's CER | ceiling | characters | seconds |
| --- | --- | --- | --- | --- | --- |
| clean | 0.0000 | **0.0000** | 0.01 | 255 of 255 | 0.02 |
| +10 dB | 0.0000 | **0.0000** | 0.02 | 255 of 255 | 0.02 |
| +3 dB | 0.0000 | **0.0000** | 0.05 | 255 of 255 | 0.02 |
| -3 dB | 0.0000 | **0.0000** | 0.10 | 255 of 255 | 0.02 |
| drift, 20 Hz in a minute | 0.0000 | **0.0000** | 0.05 | 255 of 255 | 0.02 |
| two signals, channel at 1000 Hz | 0.0000 | **0.0000** | 0.05 | 255, no `EI4GNB` | 0.02 |
| noise only, 30 s | not stated | **zero characters** | must be none | 0 | 0.01 |
| **-10 dB, made by task 5** | not run | **0.1502** | **none asserted** | 221 of 255 | 0.02 |

**The squelch rule, in one sentence:** the mean of `|Re(d)|` over the mean of `|d|` across
a rolling window of 32 symbols, where `d` is each symbol times the conjugate of the one
before - **the number is 0.90**, on a scale whose ends are 0.637 for uniform noise phase
and 1.0 for clean keying. It measures **shape rather than loudness**, because loudness
cannot tell a station from a burst of noise at the same level.

### Task 1 - the tab stops pretending

**What happened to Tim, and why.** He pressed PSK31, saw a CQ, answered it and got
nothing back. There is no PSK31 decoder in this tree, so what he saw was **FT8's decoder
still running under the PSK31 tab**, and what he sent was **an FT8 message on 14.070**.

Two places, with file and line:

- **`MainWindowViewModel.cs:10228`, `OnSlotTick`.** It gates on `IsDigitalMode`, which is
  the *tab* and not the sub-mode, so the slot watch went on cutting FT8 slots and the FT8
  decoder went on reading them whatever chip was pressed.
- **`MainWindowViewModel.cs:12138`, `SendMessage`.** It composes through `DigitalModeFor`,
  which answers `Ft8` for anything that is not FT4. That is the right default for a slot
  grid and it was being read as an answer to *what can this send*.

**Unit 312 fixed the sentence and left the machinery connected**, and that was this
author's doing: the instruction said what not to build and did not say to disconnect what
was already there.

**Two gates at the two doors.** The tick returns before the watch is asked - *not
running*, not hidden. The send door refuses in words, and being the only call site in
`src/` that composes a signal, it covers the CQ button, the right-click answer and
anything a later unit adds. **`CanDecode` and `CanTransmitIn` are written separately on
purpose**: this very unit gives PSK31 a decoder, and one predicate would have re-opened
the transmitter with it.

Measured: thirty ticks under PSK31 give **0 slot looks and 0 rows**; the same thirty under
FT8 and FT4 give **30 looks each**, and FT8 still reaches the send door.

**And the receipt loses Log** (Tim, 2026-09-11: *"CQ should not have log option, that is
self-gratification."*). This supersedes unit 305's *Log from first appearance* and unit
310's R2, both the author's reasoning rather than his ruling. **Taking the button off made
the quiet link appear in its place** - its rule read *Log is not already the action here*,
which became true the moment the action became None. Both are shut.

### Task 2 - the varicode

`data/psk31/varicode.csv` with `varicode-SOURCE.md` beside it, embedded and read at load,
the way unit 252 carried the DXCC list. All 256 round-trip, all 256 distinct, none
contains `00`, every one starts and ends with `1`, shortest 1 bit and longest 12. The QSO
text is 255 characters and 2,000 bits - **7.8 bits a character** - and survives 40 idle
bits either side.

### Task 3 - the demodulator

Mix down at the offset; average over one bit, which is the matched filter for the
transmitter's raised-cosine pulse; find the symbol instant from where the envelope is
strongest; one sample a bit; multiply by the conjugate of the last; the sign of the real
part is the bit; split on `00`; look it up.

**One real bug, found by measuring rather than reasoning.** The first draft decayed the
timing profile by 0.995 a sample - a time constant shorter than a single bit - so the
clock chased whichever symbol had just gone past. It cost **CER 0.1976 at +10 dB where
clean was 0.0237**, and the symbol instant moving by a whole bin could sample a period
twice or not at all, which loses a bit and destroys a whole character rather than
damaging one. The cadence is fixed now and nudged one sample at a time, and the profile
remembers about eight symbols. **That one change took every QSO fixture to 0.0000.**

**The AFC only tracks while the squelch is open.** On noise the squared phase is uniform,
so a loop running all the time would take a random walk away from the frequency the
operator tuned to and be pointing at nothing when a station did start.

### Task 4 - the panel hears

The demodulator sits behind the tab, fed from **the same audio tap the FT8 decoder is fed
from**, at one spot **1000 Hz above the dial** (`Psk31Listening.OffsetHz`, one place).
Text lands as characters complete rather than a message at a time.

Measured through the real tap and the real tick in quarter-second lumps: **255 of 255
characters at CER 0.0000 on one row**, 126 of them half way through, and what was there
half way is still the start of what is there at the end.

**Nothing is parsed.** No callsign is read out of the text, no state inferred, no row
answerable, and the send door still refuses.

### Task 5 - a weak-signal fixture

`psk31-snr-10db-1000hz.wav`, made by `assets/make-weak-fixture.py`, which **imports the
author's `reference-modem.py` rather than reimplementing the convention** - a second
implementation would be a second convention that could drift. Seed fixed at 314, recorded
in `manifest.json` with its SHA-256. **CER 0.1502, and no ceiling asserted**, because a
number invented here would have no evidence behind it.

### Two superseded assertions rewritten, and one test of my own narrowed

1. **`ThePressingOfCqTests.TheCqCardCarriesTheLogOptionBeforeAnybodyAnswers`** asserted
   the withdrawn behaviour. Rewritten as
   `TheCqCardCarriesNoWayToLogAndTheStationCardDoes`, which also asserts the half worth
   keeping: **the moment somebody answers, the Log is on their card.**
2. **`ThePsk31SeamTests.ThePanelNamesTheModeAndSaysNothingDecodes`** asserted the panel
   says Hamlet cannot read PSK31, which stopped being true in task 4. Rewritten to assert
   the line is about PSK31 and says nothing about slots, which is what it was protecting.
3. **`ThePsk31ReferenceIsPinnedTests.NoSourceFileHoldsAPathIntoTheClone` was my own test
   and it was wrong.** Unit 313 wrote it as a search for the word `fldigi` anywhere under
   `src/`, which is broader than its own summary and broader than §R5: the rule is *read,
   never ported wholesale*, and **a citation naming where a published table came from is
   what that rule requires**. Task 2 did exactly as instructed and this test failed it.
   Narrowed to a path into the clone, **and proved to still catch one**: a probe comment
   holding the real path made it fail, and removing the probe made it pass.

### Two mismatches with the instruction, reported and not repaired

1. **The instruction's `reference_cer` is `null` for the noise-only fixture**, not a
   number, so a reader that assumes a double throws. Handled as *not stated* rather than
   invented.
2. **`assets/reference-modem.py` has no entry point** - it is the functions only - so
   task 5 imports it rather than running it. Nothing in the instruction says otherwise;
   noting it because "using the convention in the README" reads as though the file could
   be run.

### Tests, all filtered by exact name, foregrounded, 480 s timeout

**No suite was run. No `dotnet test` on a whole project, and nothing was backgrounded or
polled** (HM-DEC-155).

| Test type | Result |
| --- | --- |
| `ThePsk31TabIsInertTests` (new, task 1) | **5 of 5**, watched failing first |
| `TheVaricodeTests` (new, engine, task 2) | **5 of 5**, watched failing first |
| `ThePsk31DemodulatorTests` (new, engine, tasks 3 and 5) | **9 of 9**, watched failing first |
| `ThePsk31PanelHearsTests` (new, task 4) | **4 of 4**, watched failing first |
| The carry-forward list, 24 types, before anything changed | **146 of 146** |
| Everything above plus the list, after | **170 green, nothing red** |
| `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint` | 1 of 1, re-run after each change |
| `VoiceTests` | 3 of 3, run because this unit adds copy |

**Nothing new is red.** The inherited reds in section 10 were not run and were not chased.

## 2. What the owner should expect

**The build is clean** - zero warnings, zero errors, `TreatWarningsAsErrors` on.

**Press PSK31 and the radio goes to the cited watering hole, and then Hamlet listens.**
Text appears in the decoded panel as it arrives, letter by letter, the way a typed
conversation actually arrives. On the fixtures it reads a full four-line QSO with no
errors at all.

**What you must not expect, and this matters:**

- **One signal at one spot.** Hamlet listens 1,000 Hz above the dial and nowhere else.
  There will be other stations across the passband and it is not looking at them. The
  panel line says so in words, because otherwise an empty list reads as an empty band.
- **No clicking.** No row can be answered, no card appears, and there is nothing to
  right-click.
- **No sending.** The CQ button under PSK31 refuses and says why. **This is the fix for
  what happened to you**: the tab was composing FT8 and putting it on 14.070, which is
  why nobody came back. Answering in PSK31 is the *say it* step.
- **Nothing is understood.** The text is text. No callsign is read out of it and no state
  is inferred; that is the *read the conversation* step.

**And the CQ receipt no longer offers a Log**, as you ruled.

### One thing I need from you, and it is the only thing that can move this further

**A few minutes of real off-air audio on 14.070.** Everything above is measured against
fixtures the author generated on a machine with no radio, so it proves the arithmetic and
proves nothing about the air. Real PSK31 has drifting carriers, fading, several stations
in the passband at once, and noise that is not Gaussian.

**What to do:** tune to 14.070 in USB-D when the band is open, and record the receive
audio for **two or three minutes** - any recorder that takes the USB codec's input, at any
sample rate, saved as WAV. Somewhere with a few signals on it is far more useful than a
clean one. Drop it in `assets/fixtures/captured/` and say roughly what you could see on
the waterfall. **That one file turns every number in this report from an indication into
something worth arguing with.**

**Pushed to `main`**, seven commits, nothing uncommitted.

## 3. What you should see

**The panel line, word for word**, with PSK31 pressed and the dial where it is:

```
listening for PSK31 at one spot, 1000 hertz above the dial, which puts it at
14.071000 MHz just now. Anything further along the band is out there and Hamlet
is not looking at it yet, so a quiet list here does not mean a quiet band.
```

**The send refusal, word for word**, if you press CQ there:

```
Hamlet cannot send PSK31 yet, so nothing went out. It can hear this mode before
it can answer in it, and sending anything else here would put the wrong kind of
signal on a frequency people are using for PSK31.
```

**What the decoded panel reads from the clean fixture** - one row, growing:

```
CQ CQ CQ de KC3QIS KC3QIS KC3QIS pse K
KC3QIS de W1AW W1AW K
W1AW de KC3QIS RST 599 599 Name Tim Tim QTH Trafford PA Grid FN00 FN00 BTU W1AW de KC3QIS K
KC3QIS de W1AW R R TNX Tim UR 599 599 Name Bob QTH Newington CT Grid FN31 73 73 KC3QIS de W1AW SK
```

255 characters of 255, no errors, and **nothing at all from the noise-only fixture.**

**The CQ receipt, with the Log gone:**

```
CQ
Calling
02:03:45 UTC · just now

Your call went out to anyone listening.

Sent CQ KC3QIS FN00 · At 02:03:45 UTC
```

No button, no link, no hover. Dismiss is still there.

**What I cannot tell you.** Whether any of it looks right on your screen, and anything at
all about how it behaves on a real signal. Both want a radio.

## 4. What's blocking us

Nothing blocks the *hear everyone* step. Five items want your ruling or your radio.

1. **Real off-air audio on 14.070, two or three minutes.** Section 2 says how. **This is
   the one item that changes what the next step can honestly claim**, and only you can
   do it.

2. **The squelch number is the author's, 0.90.** Chosen from the arithmetic - 0.637 for
   noise, 1.0 for clean keying - and confirmed against the fixtures rather than tuned
   until they passed. On real audio it may prove tight or slack, and that is what the
   recording would settle. It is `Psk31Demodulator.SquelchQuality`, in one place.

3. **The listening spot is 1,000 Hz above the dial, and that is a starting place rather
   than a measurement.** PSK31 activity spreads a few kilohertz above the calling
   frequency; one channel at one offset is what step 1 built. `Psk31Listening.OffsetHz`.

4. **`CanDecode` and `CanTransmitIn` are two predicates that agree today.** They are
   written separately because hearing a mode comes before answering in it. When the *say
   it* step gives PSK31 a modulator, only the second one moves.

5. **The card-rebuild root is still open** (ask 10). It is not this unit's, and it now has
   a second consequence worth knowing: the PSK31 text row is replaced in place rather than
   the list rebuilt, for exactly the reason unit 313 measured on the card panel.

### Asks still outstanding

Carried per HM-DEC-139, verbatim where unresolved.

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
7. **`PHASE_OUTCOME.md`** - this phase's record starts at unit 312; 306, 307, 309 and 310
   are in the FT4 phase's file in git history. Not to be back-filled.
8. **The door sentence is a placeholder.** Carry.
9. **Acknowledgement indicators.** Named by Tim, not yet defined. The *say it* unit builds
   a turn indicator that may be what he meant. Do not assume it is.
10. **Card ordering under scroll.** Raised three times, unruled. **Unit 313 found the
    root**: `DigitalCards.Clear()` then new `Ft8ContactCard` objects every slot, so all
    per-card state is lost four times a minute, including `MapIsOpen`. Not fixed. Not this
    unit's. Carry.
11. **The outline width on the neighbourhood map is 2 px**, a number unit 312 chose.
12. **Version numbering** - 1.13.0 versus a strict HM-DEC-150 reading. Patch bump here:
    1.13.1 → 1.13.2.
13. **`PHASE_PLAN.md` §R6 says 14.0700-14.0725; the cited row says 14.070-14.074. The row
    wins.** Prose to correct when the plan is next touched.
14. **A larger source bitmap** would make the popup's zoom cap unnecessary. Tim's.
15. **One conversation card is taller than the panel** (293 px in 220 px). Not broken.
16. **The popup zoom cap of 2.0x is the author's number.** `Ft8GlobePlot.ZoomCap`.

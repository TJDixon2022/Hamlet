```
READ IN THIS ORDER.

A. The phase goal - Hamlet works Olivia the way it works PSK31. Step 0 done
   and closed; step 1 partial (six of seven criteria met, 1.5 met except
   that ft8_transmission does not say the send was announced); steps 2-6
   not started.
B. Step 1's criteria 1.1 to 1.7: bursts read right 8 of 8, worst center
   error 0.77 Hz, false detections 0 on 2 files, the -16 dB burst read yes,
   Hamlet's burst read back 12 of 12, shipped bursts matched 30 of 30
   symbols, PSK31 sends announced 5 of 5, FT8/FT4 byte-identical yes,
   rsid_heard/rsid_sent asserted yes, real-time ratio 0.075 on both files.
C. The report last: section 4 raises 5 items on top of the carried queue.
   Two bear on B. Item 1: the report macro to a compound callsign
   (VP2V/W1AW) is now 30.05 s with the burst and is refused. Item 2: the
   transmission record's "announced" needs a change to Ft8TransmitSequence,
   which is stop material, so it was not built. Task 4 WAS built: the four
   macros fit under decision A. The fldigi commit was NOT recorded; the
   clone could not be read from this session (item 3).
```

```
UNIT:       359 - complete at task 7 of 7 (tasks 0 to 6), none dropped, none not built - 2026-09-14 12:56
PHASE GOAL: Olivia on Hamlet end to end, the way PSK31 is - heard, read, answered and
            logged - with the variant taken from the signal's own RSID announcement
            and never chosen by the operator.
UNIT GOAL:  Hear an RSID burst anywhere in the passband and name the mode, variant
            and center from it on every one of the mode author's fixtures; make
            Hamlet's own burst and read it back; put the BPSK31 burst in front of
            every PSK31 send, with FT8 and FT4 unchanged.
ADVANCED:   yes - six of seven step 1 criteria met, each by a test watched red first; 1.5 met except the ft8_transmission field
NUMBER:     bursts read right 0 -> 8 of 8; PSK31 sends announced 0 -> 5 of 5
DRIFT:      0
```

**Every appearance claim is computed, not seen.** Nothing here is evidence about the radio: a
fake port, a fake sink, a fake tap and telemetry files, on a development machine with none
(FACT-004, FACT-006).

| Criterion | State | Number |
| --- | --- | --- |
| 1.1 each RSID fixture one detection, right code, variant and center within 5 Hz; no-RSID and noise-only none | met | 6 of 6 single-burst files one detection each, worst 0.77 Hz; `olivia-8-250-qso-norsid` 0, `olivia-noise-only-30s` 0 |
| 1.2 two-signal fixture yields two, 8/250 at 1000 and 16/500 at 2000 | met | 69 at 1000.29 Hz, 70 at 1999.66 Hz |
| 1.3 the -16 dB fixture's RSID detected | met | 70 at 1000.77 Hz, 12 of 15 tones right, quality 0.305 |
| 1.4 Hamlet's burst reads back as its code at its center, and matches the shipped sequence | met | loopback 12 of 12 (4 codes x 3 centers), worst 0.36 Hz; fldigi's shipped bursts 15 of 15 symbols in each of 2 files, 30 of 30 |
| 1.5 a PSK31 CQ begins with the BPSK31 burst by loopback; the record says so; FT8 and FT4 byte-identical | partial | CQ reads back code 1 at 1000.32, 1437.69 and 1000.32 Hz (48 kHz); a press reads back at 1299.64 against its 1300 Hz offset; text samples identical to task 1's hash at 12 and 48 kHz; `psk31_send_composed` says `announced: true, rsidCode: 1`; **`ft8_transmission` does not** (section 4 item 2); `TheFt8AndFt4SendsAreByteIdenticalTests` green, unedited |
| 1.6 `rsid_heard` (code, variant, center, quality) and `rsid_sent`, no callsign | met | both asserted against the written lines, no callsign, grid, name, place or text |
| 1.7 real time on the four-signal PSK31 and two-signal Olivia fixtures, ratio reported (nice-to-pass) | met | cpu/audio 0.075 on `assets/fixtures/psk31-four-signals.wav` (8000 Hz) and 0.075 on `olivia-two-signals-rsid.wav` |

## 1. What Claude did

**Complete: all seven tasks, 0 to 6, none dropped and none not built.** Task 6 was the drop
candidate and was built. Development machine QUIVERFULL. The prompt claimed `PROJECT: Hamlet`,
and the gate confirmed it: `SHACK_FACTS.md` and `CwProbabilisticDecoder.cs` present, `CoreHMI.sln`
and `MURC.sln` absent, root `C:\Source\HamLet`. The session lock was this session's
(12:13:00). Branch **`main`**, every commit pushed before the next task began, and every push
succeeded:

- `c93159b6` task 0
- `489e9c5d` task 1
- `069598ce` task 2
- `b7be8e6e` task 3
- `ca3b57fe` task 4
- `6a360988` task 4, the §R12 rewrite in its own commit
- `24400909` task 5
- the carry-forward list, a separate commit
- `72a92122` task 6
- this report

Version **1.13.45 -> 1.13.46**.

**§R5's fldigi commit is not recorded.** `C:\Source\fldigi` could not be read: listing
`C:/Source/fldigi/src/rsid` was blocked because the session may only list inside
`C:\Source\HamLet`. So it is unknown whether the clone exists at all, and whether `rsid.cxx` and
`rsid_defs.cxx` are in it. **Nothing was read from fldigi, and nothing was ported.** The only
fldigi commit the tree cites is `61b97f41`, in the engine project's comment on the PSK31
varicode, which is the PSK31 phase's. This session could not check that the clone is at that
commit.

### Task 0 - the unit opens

`PHASE_STATUS.md` has step 0 `done`. `psk31-cq-rsid.wav` and `olivia-8-250-cq-rsid.wav` were hashed
first, then the other seven: **9 of 9 match the manifest**. `UNIT 359 - STEP 1` was appended to
`PHASE_OUTCOME.md` at the end of the file, in unit 358's shape. No earlier entry was touched.
Carry-forward before any change: **app 144 of 144, engine 86 of 86**, both from their first
run. `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `WORK_INSTRUCTIONS.md` were committed as they
stood. `SESSION.lock`, `RUN_LEDGER.md`, `.run-unit\` and the deletion of `output.md` were not
committed.

### Task 1 - the trace, before anything was built

A measurement, `Unit359Trace`, which asserts nothing (the shape of `Unit337Measure`). Its
numbers are in commit `489e9c5d`'s message, which was written before task 2 began.

**1. The fixtures, measured from the audio.** All nine are PCM format 1, one channel, 8000 Hz,
16-bit. The bursts were found by lining the file's sequences up against the tone energies,
tone *k* at center + (k - 7) x 10.7666 Hz:

| Fixture | Seconds (manifest) | Burst, first tone to last | Strongest tone right | Next signal |
| --- | --- | --- | --- | --- |
| `olivia-8-250-cq-rsid` | 28.978 (28.98) | 0.464 to 1.858 s | 15 of 15 | 2.338 s |
| `olivia-16-500-qso-rsid` | 131.378 (131.38) | 0.464 to 1.858 s | 15 of 15 | 2.338 s |
| `olivia-32-1000-qso-rsid` | 106.802 (106.8) | 0.464 to 1.858 s | 15 of 15 | 2.338 s |
| `olivia-16-500-qso-snr-10db` | 131.378 (131.38) | 0.464 to 1.858 s | 15 of 15 | in the noise |
| `olivia-16-500-qso-snr-16db` | 131.378 (131.38) | 0.453 to 1.846 s (1/8-symbol steps) | 13 of 15 | in the noise |
| `olivia-two-signals-rsid`, 1000 Hz | 28.978 (28.98) | 0.464 to 1.858 s | 15 of 15 | - |
| `olivia-two-signals-rsid`, 2000 Hz | | 0.464 to 1.858 s | 15 of 15 | - |
| `psk31-cq-rsid` | 14.770 (14.77) | 0.464 to 1.858 s | 15 of 15 | 2.318 s |
| `olivia-8-250-qso-norsid` | 172.064 (172.06) | none; best chance fit 6 of 15 | - | - |
| `olivia-noise-only-30s` | 30.000 (30) | none; best chance fit 6 and 7 of 15 | - | - |

So each shipped burst is **5 silent symbols (0.464 s), 15 tones (1.393 s), then about 5 silent
symbols** before the signal: 2.32 s in all, as `SOURCE.md` says.

**2. The reference.** Unreadable; see above. Not a stop, as the task says.

**3. The data.**
- **Keys:** `rsid-codes.json` carries exactly the seven keys section 5 names.
- **Values:** symbol rate 10.7666015625, 15 symbols, 5 silent before, first tone -7.
- **Codes:** BPSK31 1, 8/250 69, 16/500 70, 32/1000 71, 8/500 72, 16/1000 73, 4/500 74, 4/250 75.
- **Sequences:** there are **four**, for BPSK31, 8/250, 16/500 and **32/1000, confirmed**.
- **Missing tables:** there is no `Squares` or `indices` table.
- **Source pin:** the source is pinned to *master 2026-09-14*.

**This is enough to detect and generate every fixture's code with no literal in code.** One
gap: the file does not say how many tones a burst's alphabet has. The detector takes the span
from the highest tone any sequence uses, which is 14, so 15 tones.

**4. The send path, as it was.**
- **Where the samples are made:** `Psk31Modulator.Modulate`, at `_transmitSampleRate`. That is
  the rate the transmit endpoint declared, and `Ft8Composer.DefaultSampleRate` (12000) until one
  declares a rate.
- **Where the cap is checked:** `UnslottedTransmission.Fit`. `Ft8ArmedSend.Arm` refuses a send
  over the cap, and the sequence refuses it again.
- **What the record carries:** a no-slot `ft8_transmission` carries `mode`, `frequencyHz`,
  `durationSeconds`, `sampleRate`, `sampleCount`, `messageLength`, `outcome`, `cameOutOfTransmit`,
  `keyed`, `fit`, `audioSeconds`, `longestSeconds` and `stagesEntered`.

The seconds, identical at 12000 and 48000 Hz:

| Send | Seconds today | + 15 tones (1.393) | + the file's burst (1.858) | + silence both sides (2.322) | Cap |
| --- | --- | --- | --- | --- | --- |
| CQ | 11.456 | 12.849 | 13.314 | 13.778 | 30 |
| Answer to W1AW | 7.840 | 9.233 | 9.698 | 10.162 | 30 |
| Report to W1AW, Tim, Trafford PA | 24.800 | 26.193 | 26.658 | 27.122 | 30 |
| Confirm to W1AW | 16.256 | 17.649 | 18.114 | 18.578 | 30 |
| Report to **VP2V/W1AW** | 28.192 | 29.585 | **30.050 over** | **30.514 over** | 30 |
| Typed line, 235 characters | 59.936 | 61.329 over | 61.794 over | 62.258 over | 60 |

Today's CQ at 1000 Hz and peak 0.25, SHA-256 of its samples:
- **12000 Hz:** `1a26b6d6bb17e0f16b26c85d4e48075c90fb1f1a6d27f4390362e4354d7ca764`
- **48000 Hz:** `e45bc60d6d39fd2d39768a8645f6b8f362e3f15f2a38c45dfbaa7315f4f934e1`

`CivConstants.PttOn` code lines: **1**. `_armedSend.Arm(` lines: **2**.

**5. The Olivia tab.** The tick handed a running capture the tap's device stream, at the device
rate, and nothing else. No resampler was made.

**Before-numbers, as the instruction gave them and as measured:**
- bursts read right: 0 of 8, since the tree had no detector
- false detections: 0
- PSK31 sends announced: 0 of 5

### Task 2 - the detector (1.1, 1.2, 1.3)

`src/Hamlet.RadioEngine/Rsid/RsidDetector.cs` is Hamlet's own. `RsidCodes` now also reads
`silence_symbols_before` and `first_tone_offset_symbols`, as required keys.

**How it works:**
- **Frames:** every quarter symbol, it measures one symbol's worth of audio on a grid of
  frequencies half a tone apart across the passband.
- **Per center:** for each grid point that could be a center, it notes the strongest tone per
  frame.
- **A burst:** 15 of those notes, one symbol apart, that agree with a file sequence in all but 4
  places.
- **Refinement:** the center and the start are refined by a parabola through the neighbors'
  energy.
- **Passband:** `Psk31CarrierSearch.PassbandLowHz` to `PassbandHighHz`, 200 to 3000 Hz, the
  span the Digital waterfall header shows.
- **Output:** each detection gives the code, fldigi's name, the mode and variant read off that
  name, the center, a quality, the tones right, and the first tone's time.

`TheRsidDetectorTests` hashes each fixture against the manifest first. Watched red against an
empty stub (8 of 10 failed; the two no-burst cases passed vacuously), then **14 of 14** with
`TheOliviaDataTests`. Per-fixture numbers are in section 3.

### Task 3 - Hamlet's own burst (1.4)

`src/Hamlet.RadioEngine/Rsid/RsidBurst.cs` makes the file's silence and then the 15 tones.
- **Tones and phase:** at the code's sequence, phase-continuous.
- **Ramp:** each end rises and falls over an eighth of a symbol, along half a cosine.
- **Level:** the caller's peak.
- **No sequence:** a code without one throws, and no sequence is derived.

`TheRsidBurstTests` was watched red (8 of 8 against a stub), then **8 of 8**:
- **File's tones:** every code with a sequence makes the file's tones, measured from the samples.
- **Loopback:** 12 of 12 at 500, 1500 and 2500 Hz, at 12000 Hz, worst 0.36 Hz.
- **Shipped bursts:** fldigi's bursts in `psk31-cq-rsid.wav` and `olivia-8-250-cq-rsid.wav`
  match Hamlet's sequence **15 of 15 each**, and match what Hamlet makes.
- **Length:** correct to the sample at 8000, 12000, 44100 and 48000 Hz. At 12000 that is 5573
  samples of silence and 16718 of tones, 1.8576 s.

### Task 4 - every PSK31 send begins with its announcement (1.5, `rsid_sent` of 1.6)

**Built.** Decision A's condition held: the four macros as §R2 writes them, plus the burst, are
at most 27.122 s. See section 4 item 1 for the compound callsign.

**What changed:**
- **The composition:** `Psk31Modulator.Compose` puts the BPSK31 burst in front of the text's
  samples, centered on the send's own offset at the same drive. This is the one composition site
  on the send path, so the four macros and the typed line all go through it.
- **The code:** `UnslottedTransmission` gains `AnnouncedCode`, an init property.
- **The composition record:** `psk31_send_composed` gains `announced` and `rsidCode`.
- **`rsid_sent`:** code, mode, variant and center, written in `FirePsk31Async` once the run says
  the keying frame was taken.
- **The typed line:** `Psk31Modulator.SentSecondsFor`, the burst plus the text, now drives the
  typed line's *too long to send*, through `Psk31Macros.TypedSeconds` and `SendTypedPsk31`.
- **Left alone:** `Ft8TransmitSequence`, the gate, `Stop` and the cap's value.

`ThePsk31SendIsAnnouncedTests` (app project) was watched red (9 of 10; the site count passed
vacuously), then **10 of 10**:
- loopback at three offsets and two rates
- the samples after the burst identical to task 1's hashes
- answer, report, confirm and a typed line each beginning with the burst, with their text
  samples identical to `Modulate`
- one `Psk31Modulator.Compose(` line in the application
- a CQ press and a typed-line press each played with the burst first, their records saying so,
  nothing personal
- a 359-character typed line, 58.176 s on its text, is 60.034 s with the burst; the card says
  *too long to send* and the press is refused
- `PttOn` 1, `Arm(` 2

**One test was rewritten under §R12**, in its own commit (`6a360988`):
`ThePsk31TransmitTelemetryTests.EachOfTheFourMacrosWritesWhatItComposed`. It subtracted only
the idle before comparing to unit 317's text table. It now subtracts the 1.858 s burst too, and
guards the same rule. It was 5 of 5 after.

### Task 5 - `rsid_heard`, live under Olivia (1.6)

**The wiring:**
- **The tick:** under Olivia, the tick passes the tap's new samples (`HearRsid`) through a
  `Psk31Resampler` to 8 kHz and into an `RsidDetector`.
- **The event:** each detection writes `RsidEvents.Heard`, `rsid_heard` under the Decode
  category.
- **Other ticks:** any tick that is not an Olivia tick drops the detector.
- **A gap:** if the tap has moved past the audio, the detector is dropped too.
- **The codes:** they come from the view model's own Olivia data, so an unreadable table means
  nothing is listened for.

`TheOliviaSeamTests` gained three cases. Watched red (2 of 11, the two Olivia cases), then
`TheOliviaSeamTests`, `BindingHealthTests` and `VoiceTests` were **17 of 17**:
- **Under Olivia:** fldigi's 8/250 burst through the tap at 8000 and at 48000 Hz writes exactly
  one `rsid_heard`. The radio is asked for nothing more, and the dial, frequency, tab, rows and
  cards are unchanged.
- **Under FT8:** the same audio writes no `rsid_` event.

**The carry-forward list** gained `TheRsidDetectorTests` and `TheRsidBurstTests` (engine), and
`ThePsk31SendIsAnnouncedTests` and `TheOliviaSeamTests` (app).

**Which is true:** unit 358's *app 143 to 144* was the listed names' own count, and
`TheOliviaSeamTests` had never been on the list. Final runs, each from its first run: **app
165 of 165, engine 105 of 105.**

### Task 6 - the detector keeps up (1.7)

`TheRsidDetectorTests.TheDetectorKeepsUpWithRealTime` asserts a ratio under 1.0 and prints it:
- `assets/fixtures/psk31-four-signals.wav`: the 8000 Hz file of the pair, 38.59 s, cpu/audio
  **0.075**, no burst heard
- `olivia-two-signals-rsid.wav`: 8000 Hz, 28.98 s, cpu/audio **0.075**, both bursts heard

That is process CPU, measured in the filtered engine carry-forward run.

### Decisions this session made for itself - the author's, marked and overrulable

1. **Decision A was read on §R2's four macros as they are written, to W1AW**, all of which fit
   with the burst. The report to a compound callsign does not, and is section 4 item 1.
   §R10's own words tie the thirty seconds to *the Report macro with idle either side*. The
   28.19 s compound-callsign reasoning is unit 318's, not a ruling.
2. **Hamlet's burst is the file's 5 silent symbols, then the 15 tones: 1.8576 s.** Nothing
   comes after it; the PSK31 idle starts at once. fldigi's shipped bursts have about 5 more
   silent symbols after them. The file names only the silence before, so no second number was
   invented.
3. **Up to 4 wrong symbols of 15 are allowed** (`RsidDetector.WrongSymbolsAllowed`). The -16 dB
   burst read 12 right, one over the line.
4. **Quarter-symbol frames, a half-tone grid, and the PSK31 search's 200 to 3000 Hz passband.**
5. **The burst's ends are ramped over an eighth of a symbol and its phase is continuous**, so it
   does not click.
6. **If the codes cannot be read, a PSK31 send goes out unannounced** and its record says
   `announced: false`. No code is guessed.
7. **`rsid_sent` is written after the keying frame was taken, under Transmit; `rsid_heard` under
   Decode.** There is no RSID category.
8. **`Psk31Modulator.SecondsFor` stays the text's own length.** The cap estimate uses the new
   `SentSecondsFor`, and the turn timing that reads `SecondsFor` did not move (section 4
   item 4).
9. **The RSID listener runs at the PSK31 path's 8 kHz, through the same resampler**, and starts
   over on any non-Olivia tick or gap.
10. **The mode and variant are read off fldigi's name**: the part before the first underscore,
    then the rest joined with slashes. `RsidCodes.ModeOf` and `VariantOf` hold the one rule.

**Nothing was recorded in `DECISIONS.md`.**

### Tests

**No suite was run.** Every invocation was filtered and foregrounded under a 480 s timeout, with
a status write immediately before it (HM-DEC-155).

| Run | Result |
| --- | --- |
| Carry-forward app, before any change | **144 of 144** |
| Carry-forward engine, before any change | **86 of 86** |
| `Unit359Trace` (measurement, asserts nothing) | 1 of 1 |
| `TheRsidDetectorTests` against a stub, then built, with `TheOliviaDataTests` | red 8 of 10, then **14 of 14** |
| `TheRsidBurstTests` against a stub, then built | red 8 of 8, then **8 of 8** |
| `ThePsk31SendIsAnnouncedTests` against stubs | red 9 of 10 |
| The same with `TheStopIsAlwaysOnScreenTests`, `ThePsk31TransmitTelemetryTests`, `TheTypedLineGoesOutTests`, `ThePsk31CqGoesOutTests`, `ThePsk31ConversationCardTests`, `ThePsk31ExchangeTests` | announced **10 of 10**. Two reds: the §R12 test above, and `TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns` (an **FT8** slotted send; extra abort frames; **green alone on rerun 1**) |
| `ThePsk31TransmitTelemetryTests` after the §R12 rewrite | **5 of 5** |
| Engine guards: byte-identical, unslotted, modulator, detector, burst | **45 of 45**, byte-identical and unslotted unedited |
| `TheOliviaSeamTests` extended, then with `BindingHealthTests` and `VoiceTests` | red 2 of 11, then **17 of 17** |
| Carry-forward app, final, with the two app names added | **165 of 165**, first run |
| Carry-forward engine, final, with the two engine names added | **105 of 105**, first run |

**230 green before, 270 at the end on the carry-forward invocations.** The two known reds,
`TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission` and
`TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow`, are not on the list
and were not run. The `Arm(` count they depend on is still 2.

### Section 5 of the instruction, checked against the tree

**Confirmed:**
- **The data file:** its keys, values and codes; four sequences, 32/1000 included; no `Squares`
  or `indices`; the date pin.
- **The manifest:** nine files, seven with RSID, eight bursts, the centers, two without RSID.
- **`Rsid\`:** it held `RsidCodes.cs` only, and `OliviaData.cs` reads the data.
- **The send path:** `Compose` returned an `UnslottedTransmission`; `SendPsk31` arms through
  `OperatorSend.Now` and fires through `Ft8ArmedSend.NowAsync`.
- **Line numbers:** the one `PttOn` write is at `Ft8TransmitSequence.cs:513`. Before this unit's
  edits, the `Arm(` lines were at `:15038` and `:15235`, and `LongestTypedSeconds = 60` at
  `:15391`.
- **The card:** `Ft8ContactCard` measures against the 60, through `Psk31Macros.TypedSeconds`.
- **The four-signal recording:** it has its `-48k` twin; 8000 and 48000 Hz, 38.59 s.

**Mismatches, reported and not repaired:**
- **The cap is on `OperatorSend`, and the first authoring was right.** `LongestUnslottedSeconds =
  30` is declared at `Ft8TransmitSequence.cs:133` *inside* `public sealed record OperatorSend`,
  which is declared in that file at `:117`. There is no `Ft8TransmitSequence.LongestUnslottedSeconds`.
  Section 5's correction is the mismatch.
- **Only two of the five guards are app tests.** `TheOliviaSeamTests` is in
  `tests\Hamlet.App.Tests\ViewModels\` and `TheStopIsAlwaysOnScreenTests` in `...\Views\`. The
  other three are in the engine project: `TheFt8AndFt4SendsAreByteIdenticalTests` and
  `TheUnslottedSendTests` under `Transmit\`, `ThePsk31ModulatorTests` under `Psk31\`.
- **Task 3 says both "15 tones plus the silence the file names" and "the file's symbol count over
  its symbol rate".** Built as the silence then the tones. The tone section is asserted to be the
  symbol count over the rate to the sample, and the whole burst the silence plus that.
- **`SOURCE.md` says fldigi's burst has five silent symbols either side**, and the shipped audio
  agrees. The data file names only the silence before.
- **`SOURCE.md` says the `Squares` and `indices` tables are in the data file.** They are not;
  expected.
- **Decision C:** codes 72 to 75 have no sequence, stay undetected, and no table was added,
  because fldigi was unreadable. No criterion names them.
- **`PHASE_STATUS.md` still says `WORK_INSTRUCTION: 358`**, so every status write in this unit
  carried unit 358's instruction name. The file is the launcher's and was committed as it stood.
- **`CLAUDE.md` §13.1's field is `PHASE`; `tools/status.sh` writes `TASK`**, which the prompt
  also asks for. `RULES_AT` still says HM-DEC-161, and the table's highest row is HM-DEC-164.
  Both expected.
- **The `UNIT 2 - STEP 1` outcome entry reads `FATE: executed` for a run that never happened.**
  Its `COST` is 15.045699500000005, the same as `UNIT 1`'s. `RUN_LEDGER.md`'s last line is the
  11:58 halt on the session lock. Not edited.
- **`PHASE_PLAN.md` R27 and R29 name `data/rsid-codes.json`; the tree has `data/rsid/`.** Expected.
- **Tool facts, against §2:**
  - A `for` loop over `$f` was refused (*simple_expansion*).
  - A command with `( ... || true)` needed approval.
  - A `grep -v "^\s*$"` in a pipe needed approval, and so did an anchored `grep -n` on
    `ITelemetry.cs`.
  - `ls` on `C:/Source/fldigi` was blocked.
  - `&&` chains, `| tail`, `| grep -E` and several `-m` on one commit all worked.
  - `mkdir`, `cp`, `rm`, `;`, heredocs and Python were not needed.
  - The report was written with the editor, because unit 355 found `git show > file` blocked.

## 2. What the owner should expect

**Every build in the session succeeded, and warnings are errors here.** Pushed to `main`.

**PSK31 sends now start with a short warble.** Before the PSK31 carrier, Hamlet sends fldigi's
RSID announcement for BPSK31: about half a second of silence, then 1.4 s of fifteen stepped tones
centered where the PSK31 signal will be. Any fldigi on the band with RSID reception on will see
*BPSK31* announced at that spot. Every macro and every typed line is 1.86 s longer on the air.

**The Olivia tab now listens for announcements and writes down what it hears.** Nothing on the
screen changes when a burst arrives: no mode, variant, tab or dial moves and no row appears
(decision B; that is step 3's). What changes is the telemetry file: each burst heard is one
`rsid_heard` line.

**What will look wrong and is not.**
- **A PSK31 report to a compound callsign, with the default name and place, is now refused as too
  long** (30.05 s against 30). That really is a change, and it is section 4 item 1, not a fault.
- **The longest typed line is about 1.9 s shorter**, and the card counts that in its *too long to
  send*.
- **The `ft8_transmission` line for a PSK31 send does not mention the announcement.** The
  composition line and `rsid_sent` do (section 4 item 2).
- **`psk31_send_composed` seconds are 1.86 s larger than before.**
- **Under Olivia, Hamlet uses a little more processor**: the detector runs at about 0.075 of real
  time at 8 kHz.
- **Codes 72 to 75 (Olivia 8/500, 16/1000, 4/500, 4/250) are never heard.** The data file has
  no tones for them.

## 3. What you should see

**Hamlet reads 8 of 8 RSID bursts in the mode author's fixtures, the worst center 0.77 Hz out,
and hears none in the two files without one. Every PSK31 send it makes, 5 of 5 kinds, now
begins with the BPSK31 announcement, and the detector reads Hamlet's own back.** On screen,
PSK31 shows nothing new and the Olivia panel stays as it was. The change is on the air, and in
the telemetry file.

The eight bursts, from `TheRsidDetectorTests`:

```
fixture                          expected       detected            error    quality  tones right
olivia-8-250-cq-rsid.wav         69 at 1000 Hz  69 at 1000.32 Hz    0.32 Hz  0.903    15
olivia-16-500-qso-rsid.wav       70 at 1000 Hz  70 at 1000.32 Hz    0.32 Hz  0.902    15
olivia-32-1000-qso-rsid.wav      71 at 1000 Hz  71 at 1000.32 Hz    0.32 Hz  0.903    15
olivia-16-500-qso-snr-10db.wav   70 at 1000 Hz  70 at 1000.37 Hz    0.37 Hz  0.590    14
olivia-16-500-qso-snr-16db.wav   70 at 1000 Hz  70 at 1000.77 Hz    0.77 Hz  0.305    12
olivia-two-signals-rsid.wav      69 at 1000 Hz  69 at 1000.29 Hz    0.29 Hz  0.896    15
olivia-two-signals-rsid.wav      70 at 2000 Hz  70 at 1999.66 Hz    0.34 Hz  0.897    15
psk31-cq-rsid.wav                 1 at 1000 Hz   1 at 1000.32 Hz    0.32 Hz  0.901    15

olivia-8-250-qso-norsid.wav      detections 0
olivia-noise-only-30s.wav        detections 0
```

**The PSK31 CQ loopback, from Hamlet's own send:** a press on the fake radio chose 1300 Hz and
played 159763 samples at 12000 Hz. The detector read them back as **BPSK31, code 1, at
1299.64 Hz**, 15 of 15 tones right. Composed at a fixed offset, the CQ reads back at 1000.32 Hz
(12 kHz), 1437.69 Hz (for 1437.5) and 1000.32 Hz (48 kHz).

**The events as written:**

```
{"ts":"2026-09-14T16:51:18.107Z","sessionId":"9b12d797","level":"info","appVersion":"rsid","category":"decode","event":"rsid_heard","data":{"code":69,"mode":"OLIVIA","variant":"8/250","centerHz":1000.3,"quality":0.903,"tonesRight":15}}
{"ts":"2026-09-14T16:56:39.142Z","sessionId":"4a42919e","level":"info","appVersion":"359","category":"psk31","event":"psk31_send_composed","data":{"macro":"cq","characters":38,"seconds":13.31,"capSeconds":30,"withinCap":true,"offsetHz":1300,"announced":true,"rsidCode":1}}
{"ts":"2026-09-14T16:56:39.159Z","sessionId":"4a42919e","level":"info","appVersion":"359","category":"transmit","event":"rsid_sent","data":{"code":1,"mode":"BPSK31","variant":"","centerHz":1300}}
```

`appVersion` is the name the test gave its telemetry file. The `rsid_heard` line is from the
Olivia seam test at 8000 Hz; the 48000 Hz run wrote the same data.

## 4. What's blocking us

Nothing blocks step 2's entry: the clean 16/500 burst is detected. Five items. **Item 1 wants
Tim, because it changes what a send can be. Item 2 is stop material the unit did not build.**

**1. The report macro to a compound callsign no longer fits its cap with the burst in front.**

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

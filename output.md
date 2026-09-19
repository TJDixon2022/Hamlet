READ IN THIS ORDER.

A. The phase goal - Hamlet works Olivia the way it works PSK31. Step 0 done
   and closed; step 1 done (1.5 met by this unit's measurements; the separate
   reading decides); steps 2-6 not started, step 2's entry open.
B. Step 1's one open criterion, 1.5, in its R32 wording - PSK31 records
   saying announced 5 of 5 with code 1; the compound-callsign report
   fits yes at 28.192 s of text (30.050 s with its burst); the typed-line
   card and gate agree yes (373 characters, 59.968 s of text, goes on both;
   374, 60.096 s, refused on both); FT8/FT4 byte-identical yes; PttOn 1 and
   Arm( 2 unchanged yes. 1.1-1.4, 1.6, 1.7 still green on the carry-forward
   list yes (engine 111 of 111; app 165 of 166 in the combined run, the one red
   a known-flaky layout test that passed alone on the first rerun).
C. The report last: section 4 raises 5 items on top of the carried queue.
   None stands in the way of 1.5 or of step 2's entry. Task 2 did not stop
   at the transmit chain: nothing it needed touched the gate, PttOn, the
   finally, StopNow or the cap's value. Task 4 found nothing in pj_mfsk.h
   that cannot be written as Hamlet's own. The format's constants (the
   scrambling code, the shift of 13, the character-to-Walsh mapping, the Gray
   code) have to match bit for bit, so they belong in a cited data file, not
   in code.

UNIT:       360 - complete at task 5 of 5 (tasks 0 to 4), task 4 built - 2026-09-18 22:10
PHASE GOAL: Hamlet hears, reads, answers and logs Olivia the way it already does PSK31, with the variant taken from the signal's own RSID announcement and never picked by the operator.
UNIT GOAL:  Make every PSK31 send's transmission record say it was announced and with which code, and hold the cap to the text alone with the burst outside it, so step 1's last criterion closes and step 2 can start.
ADVANCED:   yes - 1.5 now has measured support on every clause: 5 of 5 records say announced with code 1, and the compound-callsign report fits and is armed
NUMBER:     PSK31 records saying announced 0 -> 5 of 5; compound-callsign report refused -> fits
DRIFT:      0

| Criterion | State after this unit | Evidence |
| --- | --- | --- |
| 1.1 each RSID fixture one detection, none on no-RSID and noise | green | `TheRsidDetectorTests`, engine carry-forward 111 of 111 |
| 1.2 two-signal fixture two detections | green | same run |
| 1.3 the -16 dB burst detected | green | same run |
| 1.4 Hamlet's burst reads back and matches the file's sequence | green | `TheRsidBurstTests`, same run |
| **1.5** CQ begins with the burst by loopback; the record says `announced: true` with the code; FT8/FT4 byte-identical; the burst outside the cap | **met** | loopback: `ThePsk31SendIsAnnouncedTests`; record: 5 of 5, code 1; `TheFt8AndFt4SendsAreByteIdenticalTests` green, unedited; VP2V/W1AW report 28.192 s of text fits and is armed |
| 1.6 `rsid_heard` and `rsid_sent`, no callsign | green | `TheOliviaSeamTests` and `ThePsk31SendIsAnnouncedTests`, app carry-forward |
| 1.7 keeps up with real time (nice-to-pass) | green | `TheRsidDetectorTests.TheDetectorKeepsUpWithRealTime` in the engine run; the ratio was not re-read this unit (unit 359: 0.075) |

## 1. What Claude did

**Complete: tasks 0 to 4, five of five, task 4 built.** Machine: the development PC,
`C:\Source\HamLet`, project Hamlet, branch `main`. Every commit was pushed to `origin/main` and
every push succeeded: `0a55852c`, `1ac1f84b`, `b5259457`, `186c1a65`, `ebc653fa`, `b79fb3f0`, and
this report.

### Task 0 - the unit opens

- The gate held: `SHACK_FACTS.md` and `src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs`
  exist, `CoreHMI.sln` and `MURC.sln` do not, root `C:\Source\HamLet`.
- `PHASE_STATUS.md`: step 0 `done`, step 1 `partial`.
- The nine fixtures hash as `manifest.json` says, `psk31-cq-rsid.wav` first: **9 of 9**.
- `UNIT 360 - STEP 1` appended at the end of `PHASE_OUTCOME.md`; no earlier entry touched.
- Version 1.13.46 -> **1.13.47**.
- Carry-forward before any change: **app 165 of 165, engine 105 of 105**, first run of each.
- Committed and pushed as `0a55852c`.

### Task 1 - the trace, before anything is built

**1. Every place a no-slot send's length is judged.**

- `UnslottedTransmission.Fit` (`UnslottedTransmission.cs:81`-`85`) is **the one judge**:
  `Seconds > Cap`, where `Seconds` is all the samples over the rate, the burst included.
- `Ft8ArmedSend.Arm` (`Ft8ArmedSend.cs:279`) asks `Ft8TransmitSequence.RefusedBeforeArming`,
  which asks `SendableWithNoSlot` (`Ft8TransmitSequence.cs:781`). That reads `audio.Fit` and
  writes the refusal sentence from `audio.Seconds` and `audio.Cap`. The same method is the
  sequence's backstop inside `RunAsync`. **Neither measures anything itself**; both read `Fit`.
- The typed line's *too long to send*, in two places, both through
  `Psk31Modulator.SentSecondsFor` (text + `RsidBurst.Seconds`):
  - the card's word, `Ft8ContactCard.TypedSecondsWord` (`Ft8ContactCard.cs:246`-`249`), via
    `Psk31Macros.TypedSeconds` (`Psk31Macros.cs:155`);
  - the press's note, `MainWindowViewModel.cs:15563`.
- `psk31_send_composed`'s `withinCap` (`Psk31Events.cs:448`): `seconds <= capSeconds`, with
  `seconds` = `composed.Seconds`, the burst included (`MainWindowViewModel.cs:15303`).
- Nothing else found. `TransmitRecord` writes `longestSeconds` but judges nothing.

**2. The seconds of each send, measured today** (`Unit359Trace`, 12000 and 48000 Hz identical).
`Compose` now puts the burst in front, so the trace's first column is text + burst. The text
column is that less the 1.8576 s burst.

| Send | Text s | Text + burst s | Cap | Today |
| --- | --- | --- | --- | --- |
| CQ | 11.456 | 13.314 | 30 | fits |
| Answer to W1AW | 7.840 | 9.698 | 30 | fits |
| Report to W1AW | 24.800 | 26.658 | 30 | fits |
| Confirm to W1AW | 16.256 | 18.114 | 30 | fits |
| Report to VP2V/W1AW | 28.192 | **30.050** | 30 | **refused** |
| Typed line, 227 characters | 58.112 | 59.970 | 60 | fits |

**Unit 359's table still holds**, number for number. The trace's longest typed line is now
227 characters rather than 235, because `TypedSeconds` has counted the burst since unit 359.

**3. What `ft8_transmission` carries today.**
- A PSK31 send: `mode`, `frequencyHz`, `durationSeconds`, `sampleRate`, `sampleCount`,
  `messageLength`, `outcome`, `cameOutOfTransmit`, `keyed`, `fit`, `audioSeconds`,
  `longestSeconds`, `stagesEntered`. **Nothing about the announcement.**
- An FT8 send: `slotStartUtc`, `startSecondsIntoSlot`, `frequencyHz`, `durationSeconds`,
  `sampleRate`, `sampleCount`, `messageType`, `messageLength`, `carriedHashedCallsign`,
  `outcome`, `cameOutOfTransmit`, `keyed`, `stagesEntered`.
- Found along the way: `longestSeconds` is always `OperatorSend.LongestUnslottedSeconds`
  (30), even on a typed line held to 60 (`TransmitRecord.cs:166`).

**4. Tests that assert the burst counts inside the cap.** One:
`ThePsk31SendIsAnnouncedTests.TheTypedLinesTooLongEstimateIncludesTheBurst`. It also asserts
`SentSecondsFor` = text + burst. It is the §R12 rewrite for task 2.
`TheUnslottedSendTests.ASendWithNoSlotLongerThanTheCapIsRefusedBeforeItArmsWithARecord` compares
whole audio to the cap. Its report to VP2V/W1AW with a long place is over on its text alone, so
it still holds under D.

**5. Counts.** `CivConstants.PttOn` code lines **1**; `_armedSend.Arm(` lines **2**.

This trace was committed as `1ac1f84b`, before task 2 wrote a line.

### Task 2 - the burst is outside the cap (1.5, R32 a)

Built decisions D and E. **No change to the gate, `PttOn`, the `finally`, `StopNow`, the cap's
value, or any keying or arming site.** The sequence did not change in this task.

- `UnslottedTransmission` gains `AnnouncementSamples` (init), `Announced`,
  `AnnouncementSeconds` and `TextSeconds`. Only `Psk31Modulator.Compose` sets
  `AnnouncementSamples`, from `burst.Length`, the burst it actually put in front.
- `Fit` holds `TextSeconds` (the samples after the announcement) to `Cap`. The excusal is
  bounded: the claimed announcement must be no longer than
  `RsidBurst.LengthInSamples(codes, SampleRate)`, must carry a code the file has a sequence for,
  and must be no longer than the audio. Otherwise the send is `LongerThanTheCap`. Unannounced
  audio has `AnnouncementSamples` 0 and is measured whole. No literal code, tone, rate or length
  is in the code: all of them come from `OliviaData.Current.Rsid`.
- E: `Psk31Macros.TypedSeconds` and the press's note now use `Psk31Modulator.SecondsFor`, the
  framed text alone. The card reads *N s of text* (it read *N s on the air*), and the note reads
  *comes to N s of text*. `psk31_send_composed`'s `withinCap` takes the announcement off.
  **`SentSecondsFor` is gone**: nothing used it once the card and the note moved. No file was
  emptied.
- **Tests, watched red first** (against a stub with the properties and no behavior):
  `TheUnslottedSendTests` gained four. Three were red and went green:
  `TheReportToACompoundCallsignFitsWithItsBurstOutsideTheCap` (28.192 s of text, 1.858 s burst,
  30.050 s audio, `Fits`, armed, played, one key and one unkey, every sample handed over);
  `ATextAtTheCapWithItsBurstFitsAndOneSampleMoreDoesNot` at 12000 and 48000 Hz (360000 and
  1440000 text samples fit; one more is refused); and
  `AnAnnouncementLongerThanTheBurstOrWithNoCodeIsLongerThanTheCap` (five cases, each with 10 s
  of text). `UnannouncedAudioIsMeasuredWholeAsBefore` was green before and after, as a guard
  of *as before* should be.
- **§R12 rewrite, in its own commit (`b5259457`)**:
  `ThePsk31SendIsAnnouncedTests.TheTypedLinesTooLongEstimateIncludesTheBurst` became
  `TheTypedLinesCardAndGateAgreeOnTheTextAlone`. Watched red first (*61.8 s on the air, too long
  to send* for 59.968 s of text), then green: **373 characters, 59.968 s of text (61.826 s with
  the burst), is not too long on the card and goes. 374 characters, 60.096 s, is too long on the
  card and refused at the press.** No other test needed a rewrite.
- Run filtered, green and unedited: `TheFt8AndFt4SendsAreByteIdenticalTests` (engine run 32 of
  32 with `TheUnslottedSendTests` and `ThePsk31ModulatorTests`) and
  `TheStopIsAlwaysOnScreenTests` (app run 29 of 29 with `ThePsk31SendIsAnnouncedTests`,
  `TheTypedLineGoesOutTests` and `ThePsk31TransmitTelemetryTests`).
- Committed `186c1a65`.

### Task 3 - the record says it was announced (1.5, R32 b)

Built decision F. `TransmitRecord` gains `Announced`, `RsidCode` and `AnnouncementSeconds`,
optional and written only when not null. `Ft8TransmitSequence.Recorded` fills them on the
no-slot branch only: `announced`, `rsidCode` only when announced, and `announcementSeconds` (0
when not announced). **This is the one change the sequence took.** `audioSeconds` is still the
whole audio. The slotted branch is untouched.

- **Watched red first** on the missing `announced` key, both tests, then green:
  - `ThePsk31SendIsAnnouncedTests.EachOfTheFiveKindsOfSendRecordsThatItWasAnnounced` (app). It
    presses CQ, then Answer, Report and Confirm through the textbook exchange, then a typed
    line, on the fake radio. Composed in that order; five played; **5 of 5 records say
    `announced: true`, `rsidCode: 1`, `announcementSeconds` 1.8575833 s**, which is
    `RsidBurst.LengthInSamples` at 12000 Hz over the rate. Nothing personal: no callsign (his or
    mine), grid, place or word of the typed text in any record.
  - `TheUnslottedSendTests.TheRecordSaysWhetherTheSendWasAnnounced` (engine). A composed CQ
    writes `announced: true`, the code and the burst's seconds. The same text unannounced writes
    `announced: false`, no `rsidCode` key, and `announcementSeconds` 0. `audioSeconds` is the
    whole audio in both.
- **The codes cannot be made unreadable from a test.** `OliviaData.Current` is read once from
  the embedded file. So the unannounced case is the `UnslottedTransmission` that `Compose`
  returns in that case (the text's samples, no code), handed to the real sequence. It is not a
  run with the file actually broken.
- FT8: `TheFt8AndFt4SendsAreByteIdenticalTests`, which pins the FT8 and FT4 records key by key,
  is green and unedited. So no new key reached a slotted record.
- `PttOn` 1, `Arm(` 2 (`TheKeyingAndArmingSitesAreUnchanged`).
- **Carry-forward, both invocations, after the build:** engine **111 of 111** (105 plus this
  unit's 6 new cases in `TheUnslottedSendTests`). App **165 of 166** in the combined run; the one
  red, `TheTestsStayOffTheNetworkTests.The354LayoutReadsTheSameNumbersTwiceRunning`, **passed
  alone on the first rerun**. That is the test unit 357 item 1 names as flaky. The app count
  after the build is 166 because this unit added one test method. **The list gains no name:**
  no new test class was made (`Unit360Trace` is a trace that asserts nothing).
- Committed `ebc653fa`.

### Task 4 - step 2's ground, measured and nothing built

`Unit360Trace` (engine, `tests\...\Olivia\`) prints and asserts nothing, the shape of
`Unit359Trace`. Committed `b79fb3f0`.

**1. Step 2's entry check.** The clean 16/500 fixture's RSID is **still detected**:
`OLIVIA_16_500 (70) at 1000.32 Hz, tones right 15`.

**2. `assets\reference\jalocha\pj_mfsk.h` (2367 lines), read for structure.**

| Lines | Part | What an Olivia receiver needs from it |
| --- | --- | --- |
| 1-32 | includes (`pj_fht.h`, `pj_gray.h`, `pj_fft.h` and others), `Exp2`, `Log2` | - |
| 33-55 | the symbol shape in the frequency domain: coefficient tables from gMFSK and DM780 | the transmitter's pulse shape; a receiver may use its own window |
| 56-239 | `MFSK_Modulator` | step 4, not a receiver |
| 240-289, 600-701 | `BoxFilter`, `CircularBuffer` | utilities |
| 290-599 | `MFSK_InputProcessor`: overlapped-FFT input conditioning | optional |
| **702-1047** | **`MFSK_Demodulator`: tone detection.** An FFT per symbol with two spectra slices per symbol (`SpectraPerSymbol = 2`, `:718`), a symbol-shaped window, and soft bits per tone with Gray decoding (`:952`-`:1038`) | tone detection |
| **1058-1218** | **`MFSK_Encoder`: the other side of the code.** Character to a Walsh function (inverse FHT, `:1150`-`:1166`); scrambling by `ScramblingCodeOlivia` (`:1076`, `ScrambleFHT` `:1168`-`:1177`); interleave by rotating each character's bits across the symbols, shifted 13 per character for Olivia (5 for Contestia) (`EncodeBlock`, `:1178`-`:1206`) | the definition the decoder inverts |
| **1219-1431** | **`MFSK_SoftDecoder`: the Walsh-function decode.** De-interleave on input (`:1320`-`:1326`), descramble and FHT (`DecodeCharacter`, `:1328`-`:1380`), best-correlation character out | the error correction |
| 1432-1615 | `RateConverter` | Hamlet has its own rate handling |
| 1616-1850 | `MFSK_Transmitter` | step 4 |
| 1851-1928 | a usage comment for `MFSK_Receiver` | - |
| **1929-2367** | **`MFSK_Receiver`: symbol and block sync.** A search over `2 * SyncMargin + 1` frequency offsets and `SlicesPerSymbol * SymbolsPerBlock` block phases (`:2088`-`:2089`), integrated over `SyncIntegLen` blocks, with an S/N threshold (defaults margin 8, integration 4, threshold 3.0, `:2031`-`:2033`) and best phase and offset tracking (`:2240`-`:2360`) | sync |

**What could not be written as Hamlet's own without copying: no algorithm.** The FFT tone
detection, the block-phase and offset search, and the FHT correlation decode are standard
techniques. Hamlet can write them from the structure above, the way the RSID detector was
written. **What has to be taken from the source is the format's constants**, because a decoder
must match them bit for bit: the 64-bit Olivia scrambling code, the shift of 13, the
character-to-Walsh-index mapping (`Char < SymbolsPerBlock` gives +1, otherwise -1 at
`Char - SymbolsPerBlock`), and the Gray code. These are facts of the format, not expression. By
R27's pattern they belong in a cited data file (`rsid-codes.json` is the precedent), not as
literals in code. Nothing here needs a package or a port of the file.

**3. `data\olivia\timing.json`, parsed by machine** (System.Text.Json, in the trace; `python`
and `jq` both needed approval this session). Its keys: `_about`, `source` (*estimated*),
`confirm`, `written_by`, `method`, `why_an_upper_bound`, `excluded`, `variants`. Against the
manifest:

| Variant | File | Seconds (file / manifest) | Characters | RSID | Per character (file / computed) | |
| --- | --- | --- | --- | --- | --- | --- |
| 8/250 | `olivia-8-250-cq-rsid.wav` | 28.98 / 28.98 | 38 / 38 | 2.32 / true | 0.702 / 0.702 | agrees |
| 8/250 | `olivia-8-250-qso-norsid.wav` | 172.06 / 172.06 | 251 / 251 | 0 / false | **0.686 / 0.685** | per-character rounded up |
| 16/500 | `olivia-16-500-qso-rsid.wav` | 131.38 / 131.38 | 251 / 251 | 2.32 / true | 0.514 / 0.514 | agrees |
| 32/1000 | `olivia-32-1000-qso-rsid.wav` | 106.8 / 106.8 | 251 / 251 | 2.32 / true | 0.416 / 0.416 | agrees |

172.06 / 251 = 0.68550, so 0.685 to three places, where the file has 0.686. The 8/250 line
fit (0.683 s per character, 0.72 s fixed) is unaffected: it is computed from the seconds, not
the rounded figures. The file's 2.32 s RSID is the burst with five silent symbols either side
(2.3220 s). Hamlet's own burst keeps only the silence in front (1.8576 s), which is decision G.

### Decisions this session made for itself - the author's, marked and overrulable

1. **The card's wording.** *N s on the air* became *N s of text*, and the note's *s on the
   air* became *s of text*. The number no longer includes the burst, so *on the air* would have
   understated what keys by 1.86 s.
2. **`SentSecondsFor` is deleted** rather than kept for telemetry. Decision E left this to the
   unit, and nothing read it after the change.
3. **`announcementSeconds` is written as 0 on an unannounced send** rather than left out.
   Decision F says `rsidCode` is absent when false and does not say which for the seconds; 0 is
   the measured length.
4. **`Announced` requires both a code and a length above 0.** A code with no samples claimed
   excuses nothing and records `announced: false`.
5. **The unannounced case in task 3 is proved at the engine**, with the transmission `Compose`
   makes when the codes are unreadable, because the codes cannot be broken from a test.

### Section 5 of the instruction, checked against the tree

Held: HEAD `7bb6b253`; `UnslottedTransmission` `:62` and `:81`-`:85`;
`LongestUnslottedSeconds = 30` at `Ft8TransmitSequence.cs:133`; `LongestTypedSeconds = 60` at
`:15489`; `Compose` `:148`; `SentSecondsFor` `:111`; `Psk31Macros.cs:155`;
`MainWindowViewModel.cs:15563`; `Recorded` `:637`; `TransmitRecord.cs:70`;
`psk31_send_composed` already carrying `announced` and `rsidCode`; carry-forward 165 and 105;
`PttOn` 1, `Arm(` 2; Jalocha's headers in `assets\reference\jalocha\`.

Mismatches:
- **`assets\fixtures\captured\` exists.** It holds one file, `README.md`. The instruction says
  it does not exist.
- **R32 (a)'s *a fixed 2.3 seconds*** is the burst with silence on both sides. Hamlet's burst
  is 1.8576 s, which the instruction's own *1.86 s* and decision G match. Not re-argued: the
  excusal is the burst's measured length, whatever that is.
- The known ones, not rediscovered: `PHASE_PLAN.md` step 1 still shows 1.7 unchecked;
  `PHASE_STATUS.md` still says `WORK_INSTRUCTION: 358` (so every status write this unit carries
  358); `RULES_AT` still HM-DEC-161; the plan's data file names; the mislabeled outcome entries;
  the two red tests off the list.

## 2. What the owner should expect

- **A PSK31 report to a compound callsign goes again.** With the default name and place it is
  28.19 s of text and 30.05 s on the air, and it is armed and sent.
- **Any PSK31 send can key for up to the cap plus 1.86 s**: 31.86 s for a macro and 61.86 s
  for a typed line. That is R32 (a), and it is bounded. Nothing can claim more than the file's
  burst as announcement.
- **The typed line's card now reads *N s of text*.** A line may be up to 60 s of text; the burst
  in front is extra. Near the limit the card can read *60 s of text* and still send (59.968 s
  rounds to 60), and one character more reads *60.1 s of text, too long to send*.
- **Every PSK31 `ft8_transmission` line now ends its announcement with three keys:**
  `announced`, `rsidCode` and `announcementSeconds`. An FT8 or FT4 line is exactly what it was.
- **What will look wrong but is not:** a refused PSK31 send's sentence still reads *this is N s
  of PSK31 audio*, where N is the whole audio with the burst. The cap measured the text, so N is
  1.86 s more than the number compared to the cap. See section 4 item 2.

## 3. What you should see

**The five `ft8_transmission` records** for the five kinds of PSK31 send, as written by
`EachOfTheFiveKindsOfSendRecordsThatItWasAnnounced` on the fake radio at 12000 Hz
(`ts` and `sessionId` dropped):

```
cq      {"mode":"Psk31","frequencyHz":14070000,"durationSeconds":13.313583333333334,"sampleRate":12000,"sampleCount":159763,"messageLength":38,"outcome":"Played","cameOutOfTransmit":"OrdinaryUnkey","keyed":true,"fit":"Fits","audioSeconds":13.313583333333334,"announced":true,"rsidCode":1,"announcementSeconds":1.8575833333333334,"longestSeconds":30,"stagesEntered":"gate_asked | keyed | handed_to_the_sound_card | unkeyed"}
answer  {"mode":"Psk31","frequencyHz":14070000,"durationSeconds":9.697583333333334,"sampleRate":12000,"sampleCount":116371,"messageLength":23,"outcome":"Played","cameOutOfTransmit":"OrdinaryUnkey","keyed":true,"fit":"Fits","audioSeconds":9.697583333333334,"announced":true,"rsidCode":1,"announcementSeconds":1.8575833333333334,"longestSeconds":30,"stagesEntered":"gate_asked | keyed | handed_to_the_sound_card | unkeyed"}
report  {"mode":"Psk31","frequencyHz":14070000,"durationSeconds":26.657583333333335,"sampleRate":12000,"sampleCount":319891,"messageLength":96,"outcome":"Played","cameOutOfTransmit":"OrdinaryUnkey","keyed":true,"fit":"Fits","audioSeconds":26.657583333333335,"announced":true,"rsidCode":1,"announcementSeconds":1.8575833333333334,"longestSeconds":30,"stagesEntered":"gate_asked | keyed | handed_to_the_sound_card | unkeyed"}
confirm {"mode":"Psk31","frequencyHz":14070000,"durationSeconds":18.113583333333334,"sampleRate":12000,"sampleCount":217363,"messageLength":62,"outcome":"Played","cameOutOfTransmit":"OrdinaryUnkey","keyed":true,"fit":"Fits","audioSeconds":18.113583333333334,"announced":true,"rsidCode":1,"announcementSeconds":1.8575833333333334,"longestSeconds":30,"stagesEntered":"gate_asked | keyed | handed_to_the_sound_card | unkeyed"}
typed   {"mode":"Psk31","frequencyHz":14070000,"durationSeconds":16.769583333333333,"sampleRate":12000,"sampleCount":201235,"messageLength":58,"outcome":"Played","cameOutOfTransmit":"OrdinaryUnkey","keyed":true,"fit":"Fits","audioSeconds":16.769583333333333,"announced":true,"rsidCode":1,"announcementSeconds":1.8575833333333334,"longestSeconds":30,"stagesEntered":"gate_asked | keyed | handed_to_the_sound_card | unkeyed"}
```

**One FT8 record beside them**, as `TheFt8AndFt4SendsAreByteIdenticalTests` pins it and as it
still passes: no `announced`, `rsidCode` or `announcementSeconds` key.

```
ft8     slotStartUtc 2026-09-11T18:00:00.0000000Z, startSecondsIntoSlot 0.5, frequencyHz 14074000, durationSeconds 12.64, sampleRate 48000, sampleCount 606720, messageType Standard, messageLength 14, carriedHashedCallsign False, outcome Played, cameOutOfTransmit OrdinaryUnkey, keyed True, stagesEntered gate_asked | keyed | handed_to_the_sound_card | unkeyed
```

**The cap table, after this unit.** The burst is 1.858 s at 12000 and 48000 Hz alike.

| Send | Text s | Burst s | Total on the air s | Cap (measures the text) | Now |
| --- | --- | --- | --- | --- | --- |
| CQ | 11.456 | 1.858 | 13.314 | 30 | fits |
| Answer to W1AW | 7.840 | 1.858 | 9.698 | 30 | fits |
| Report to W1AW | 24.800 | 1.858 | 26.658 | 30 | fits |
| Confirm to W1AW | 16.256 | 1.858 | 18.114 | 30 | fits |
| Report to VP2V/W1AW | 28.192 | 1.858 | 30.050 | 30 | **fits** (was refused) |
| Typed line, 373 × `e` | 59.968 | 1.858 | 61.826 | 60 | **fits** |
| Typed line, 374 × `e` | 60.096 | 1.858 | 61.954 | 60 | refused, card and press |

Task 4's findings are in section 1 under its own heading.

**On the screen:** a report to a compound callsign sends where it was refused, and the typed
line's card says *s of text*. Everything else is in the telemetry file. **All of this is
computed, not seen, and none of it is evidence about the radio** (FACT-004).

## 4. What's blocking us

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

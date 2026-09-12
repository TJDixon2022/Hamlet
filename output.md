```
READ IN THIS ORDER.

A. The phase goal - Hamlet works PSK31 the way it works FT8. Steps 0 to 3 done,
   4 in progress, 5 and 6 not started - unchanged by this unit.
B. Step 4 and its must-pass - unchanged; this unit repairs step 2 on real air and
   the ground under step 4. Its two unmeasured criteria are now measured: the
   occupied bandwidth is 55.7 Hz at -30 dB, and the ALC has the manual's read.
C. The report last, and section 4 raises 5 items on top of the carried queue.
```

```
UNIT:       324 - complete at task 4 of 4, none dropped - 2026-09-11 20:12
PHASE GOAL: A PSK31 station on the air becomes a row Hamlet can read, answer,
            work and log, through the same panel, the same one-click exchange
            and the same records FT8 already uses.
UNIT GOAL:  The receive path on real air: run PSK31 at the rate it was proved
            at, stop killing carriers for going quiet, say so when Hamlet can
            hear a station it cannot read, and make the carry-forward list a
            list again so the next unit is not killed by the watchdog running it.
ADVANCED:   yes - step 4's two unmeasured criteria are measured; step 2's
            behaviour on 48 kHz audio is proved and its passband corrected
NUMBER:     carry-forward types 46 -> 26, run seconds 41 builds -> 25 s over two;
            characters from the 48 kHz four-signal fixture 0 -> 483 at CER
            0.0000 on all four stations; occupied bandwidth at -30 dB 55.7 Hz
DRIFT:      0 consecutive units without advance  (was 0)
```

## 1. What Claude did

**Complete, at task 4 of 4. Nothing was dropped.** Neither named drop candidate -
task 3's dimmed row, task 4b's ALC read - was taken.

Machine QUIVERFULL, project claimed and confirmed Hamlet (`SHACK_FACTS.md` and
`src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` present, `CoreHMI.sln` and
`MURC.sln` absent, root `C:\Source\HamLet`), branch `main`, four commits pushed.

**Every appearance claim in this report is computed, not seen.** No radio was
connected to this machine, every fixture is synthetic, and nothing here was
watched on a screen (FACT-004, FACT-006).

### Task 1 - the carry-forward list stops being a suite

`PHASE_OUTCOME.md` gained a `UNIT 324 - STEP 4` entry after unit 323's; the
version went 1.13.9 to 1.13.10.

The list was **46 named types, not the 41 the instruction says**, and six of them
were filed under the wrong project: `ThePsk31TelemetryTests`,
`ThePsk31TransmitTelemetryTests`, `ThePsk31PanelSpeaksPsk31Tests`,
`ThePsk31CqGoesOutTests`, `ThePsk31ExchangeTests` and `ThePowerIsOfferedTests`
are all app types and were listed under the engine heading, **where a filter for
them matches nothing at all**. A unit running the list one name at a time ran six
builds that tested nothing and said so to nobody. `CallsignPrivacyTests`, which
the instruction names as a keep, was not on the list at all.

Pruned to **23**, every one of the **24** dropped names written into
`docs\carry-forward-dropped.txt` with the unit that added it, and the two exact
invocations written at the top of `carry-forward-tests.txt` so no future session
runs it forty-one times. Run that way before anything changed: **138 of 138
green, app 66 and engine 72, 35 s of wall clock across two builds.** Well under
the six-minute bar.

The list ends the unit at **26**: this unit's three guards and
`CallsignPrivacyTests` were added. **159 of 159 green in 25 s** at the close.

**A decision this session made for itself.** The instruction caps the list at
twenty and its own keep-rules name twenty-three. The keep-rules won and the count
is reported: dropping a guard the instruction named, to satisfy an arithmetic, is
a coverage decision the owner did not make, and the cap exists for the wall clock,
which is 25 s. It is item 3 in section 4.

### Task 2 - the PSK31 path runs at the rate it was proved at

`Psk31Resampler` puts the device's audio on **8 000 Hz** at the boundary where it
enters the PSK31 path, and nothing past it knows what the sound card did. It is a
streaming filter, not a block one: it holds the tail its filter still needs and
carries the output phase from lump to lump, so **the answer does not depend on
where the tick cuts the stream** - asserted by pushing the same audio through in
even quarter-second lumps and in ragged ones and comparing sample for sample.
`Psk31Demodulator` and `Psk31CarrierSearch` are **unedited by this task**.

The filter is `SincKernel`, which moved out of `Ft8Resample` into its own file so
that the two resamplers share one set of taps rather than a copy each (§0).
`Ft8Resample`'s arithmetic is unchanged and its callers are green.

**The passband comes from the mode now: 200 to 3 000 Hz.** The evening that found
this fault recorded `passbandLowHz: 64, passbandHighHz: 23936` - a claim to be
searching twenty-four kilohertz of a receiver that passes three.
`psk31_listening_started` gained `deviceSampleRate` and `resampleRatio` beside
`sampleRate`, because the old line said `sampleRate: 48000` and nothing in it
said whether that was the device or the decoder. It was both, and that was the
fault.

**`ThePsk31PathRunsAtItsRateTests`, app, 6 of 6 green.** The four-signal fixture
raised to 48 kHz (a new file, hash pinned in `manifest-step2.json`) yields the
same four carriers and **CER 0.0000 on all four**; noise at 48 kHz yields nothing;
the record reads `deviceSampleRate 48000, sampleRate 8000, resampleRatio 6,
passband 203 to 3000 Hz`.

**And the case the instruction expected to fail did not, which is a finding.** It
was watched with the wiring reverted - the listener opened at the tap's own 48 kHz
and handed unfiltered samples - and **all four rows still read their text at CER
0.0000**. The search and the demodulator take their samples-per-symbol from the
rate they are given, so they scale. **The rate alone is not what silenced the
operator's 893 Hz station.** What fails without the resampler is the record. It is
item 4 in section 4.

Two further assertions were added because the supplied ones cannot fail for the
right reason: the 48 kHz fixture holds nothing above 4 kHz, so a path that
decimated without filtering would read it perfectly. **A 6 kHz tone at 48 kHz
arrives at its fold frequency 80.5 dB down**, and at 8 kHz the resampler hands the
samples straight back untouched.

### Task 3 - a carrier lives while the signal does

**The retire rule.** A carrier is retired when the search stops finding the signal
at its offset: the same candidate test the spectrum nominates on, applied to the
**newest** window, failing on **eight passes in a row**. A pass is half a spectrum
window - 0.128 s at 8 kHz - so eight is **1.02 s**, one whole `SpectrumSeconds`,
which is the shortest run one unlucky window cannot produce and the same order as
the anti-flicker hold it replaces.

The newest window rather than the one-second average, and that is measured, not
preferred: the average falls about 0.6 dB a pass, so a carrier 62 dB over the
floor - which is what the operator's own station read - would take **thirteen
seconds** to decay under the candidate bar, and the row would sit there for all of
them claiming a station that had stopped.

`LostLock`, `Silence` and `OneWay` are gone as retire reasons. **`SignalGone` and
`ListeningStopped` are the two ways out.** Not producing characters is a state of
a live carrier.

**Why that matters, in arithmetic.** The squelch measures 32 symbols - 1.02 s -
before it opens, and a character is held a further 1.02 s until the search has
vouched for the carrier after it. That is about two seconds before a first
character may be shown, and the operator's carrier was being killed at **1.9 s**.
**No carrier that short could ever have said anything.**

**One more thing moved, found by a test.** A listed carrier's offset is now the
last one actually measured. Held to the end of its own energy, the 700 Hz carrier
had been reading out to 705.3 Hz off the angle of noise.

**The row.** A held carrier with no text yet and its squelch shut shows as a row,
dimmed to unit 279's 0.55, saying **heard, not readable yet**. Author's choice,
marked as one. Once a character arrives the row lifts and fills, and **what was
read is never taken back off the screen** because the squelch shut again
afterwards. `psk31_lock` is now `psk31_reading`, which is what it measured.

**`ThePsk31CarrierLivesTests`, app, 6 of 6 green.** A new fixture - one station,
six seconds of idle, the station again, four seconds of band noise, hash pinned -
keeps **one** carrier across the gap, retired **once**, `SignalGone`, lifetime
29.3 s, 80 characters from both halves.

**A deviation from the instruction's assertion 2, reported.** The four-signal
fixture retires three with `SignalGone` and one with `ListeningStopped` - the
2200 Hz station is still transmitting when the audio runs out. That is unit 322's
own finding and the reason `ListeningStopped` exists; the assertion allows it by
name and forbids anything else.

**And assertion 3 is proved from a fed channel rather than from audio, which is a
weaker seam and is reported as one.** No synthetic fixture in this tree produces a
carrier the search lists and the demodulator cannot read: every synthetic station
is either keying properly or gone. That case is what real air produced.

### Task 4 - the two measurements

**4a. Occupied bandwidth, measured and stated: 55.7 Hz.** The CQ macro modulated
at 8 kHz, carrier 1 000 Hz, by a Welch average - periodic Hann, 16 384 samples,
0.49 Hz a bin, half overlap, 10 windows power-averaged - is **22.5 Hz wide at
-6 dB, 45.9 Hz at -20 dB and 55.7 Hz at -30 dB**. Step 4's exit asks for under
100. The Report macro at 48 kHz, which unit 318 measured, reads 57.1 Hz by the
same method, now one helper rather than two copies of it.

**4b. The ALC has a read.** `CivReads.Alc` is **CI-V `15 13`**, *Read ALC meter
level*, `00 00` = minimum to `01 20` = maximum - BCD, so the scale is **0 to
120** - the row beside `15 11` and `15 12` that Hamlet already reads. It is
decoded through the same `CivValues.Level` the other meters use, asked for
**only while a send is running** including on the connect sweep, and marked
unknown the moment the transmitter stops.

**Unit 323's zone figure of 128 is gone and nothing replaces it.** It was half of
the **0-255** range the transmit power *setting* uses; that is a different
control's range. The manual gives the scale and says nothing about where the ALC
zone ends on it. So `psk31_send_alc` carries `scaleTop: 120`, `judged: false` and
`zone: null`, and the sentence says what the meter read and asks the operator to
compare it with the zone his own radio draws. **The threshold is item 1 in
section 4.**

**`TheAlcIsReadTests`, app, 8 of 8 green**: the command is the manual's and is on
the read list; `00 00` decodes to 0, `00 60` to 60 and `01 20` to 120, and a
non-BCD reply is unknown rather than zero; **nothing asked for the ALC in 3 915
reads at rest, six reads arrived while sending, and none after**; with no radio
the record says `measured: false`. The read is cited in
`docs\psk31-reference.md`.

### Tests this unit changed rather than added, under §R12

Three, each named because a session that quietly edits a test it did not write has
moved a goalpost:

- `ThePsk31CarrierSearchTests` - one assertion, because the retire rule is counted
  in passes now and a pass is a different length at a different rate. The
  equivalent in seconds is asserted off the search itself.
- `ThePsk31ReadsTheConversationTests` - the source-text pin on `RowOpacity`, which
  gained a second reason to fade. Unit 279's number and mechanism are unchanged
  and are still asserted by behaviour.
- `ThePowerIsOfferedTests` - the ALC sentence case, which pinned the invented
  `"zone":128`.

## 2. What the owner should expect

**On 14.070 tonight, the station at 893 Hz should become a row with his text in
it, and stay there while he is sending.** Before this unit that row appeared for
about two seconds, showed nothing, and vanished - three times in a minute. It was
being killed a second after Hamlet stopped being able to read it, and Hamlet needs
about two seconds before it may show a first character, so it never got to say
anything. **A carrier is now retired when the signal goes and never because it has
gone quiet.** A station idling between words is a station.

**A station Hamlet can hear but cannot read is now a faded row that says so in
words**: *heard, not readable yet*, dimmed the same way a station already in the
log is dimmed. That is deliberate, so that an empty band and a band full of
signals Hamlet cannot make out stop looking identical. **It is this session's
choice and not a ruling** - say the word and it goes, or stops being dimmed.

**He still touches nothing at the radio.** Nothing in this unit changes a setting,
keys anything, or asks him to move a knob. The PSK31 path now resamples the sound
card's audio to the rate it was built at, which he will not see or hear; it is why
the numbers in the diagnostic file changed.

**What will look wrong and is not.** Rows will stay on the list a second or two
longer after a station stops than they did - that is the new rule holding a carrier
while its signal is still measurable, and it is on purpose. And after a PSK31 send
the ALC line no longer says the radio is being driven too hard; it says what the
radio's own meter read, out of 120, and asks him to look at the bar on the radio.
That is a step back on purpose: the number the old sentence judged against was
invented in the last unit, and nobody can tell Hamlet where that line is yet.

## 3. What you should see

**The question this unit was commissioned to ask is whether a real PSK31 station
becomes a row with text, and the answer is: the two faults that stopped it are
fixed, and one of them was not the fault the instruction named.**

- **Text where there was none.** The four-signal fixture, at the 48 kHz his sound
  card actually delivers, reads **483 characters across four stations at a
  character error rate of 0.0000** - the same as at 8 kHz. On his evening the same
  path produced **17 carriers and 0 characters**.
- **A carrier that stops being readable is no longer killed.** A station that
  idles for **six seconds** in the middle of a transmission stays **one** row,
  retired **once**, at the end, because its signal went. The record says
  `SignalGone`.
- **Hamlet says when it can hear and cannot read.** The row reads *heard, not
  readable yet* and is faded to the same 0.55 a worked station fades to, and it
  fills the moment a character arrives.
- **The signal Hamlet sends is 55.7 Hz wide** at 30 dB below its peak. Step 4
  wanted that stated and under 100.
- **The radio's own level meter can be read now**, on the scale its manual gives -
  0 to 120 - and only while a transmission is running. With no radio in front of
  this machine the record says nothing was measured rather than reporting a zero.
- **A unit is no longer killed by the list it runs first.** 46 named types run as
  41 builds became 26 run as two, 25 seconds end to end.

**Every one of these is computed on this machine from synthetic audio.** Nothing
here was seen on a screen or heard on the air, and the fixture at 48 kHz was made
by raising the 8 kHz one rather than recorded off a receiver.

## 4. What's blocking us

### 1. Where does the ALC zone end on the meter's own 0-120 scale?

**Ruling wanted.** `15 13` answers `00 00` to `01 20`, and that is all the manual
says. For data modes it says *adjust the device's output level within the ALC zone*
(p. 4-31) and gives no number. Unit 323 used **128**, which it got by halving the
**0-255** range the transmit power *setting* uses - a different control's range,
and the figure was that unit's own invention. **It has been removed and nothing
replaces it.** `SHACK_FACTS.md` FACT-005 records the operator's own reading off
the meter's face - *-2.0 to -1.5, inside the red zone* - which is a third scale
again, and converting between them would be inventing the same number twice.

Until a figure is ruled, Hamlet reports the reading and passes no judgement on it.
The judging sentence is built and fires the day a number goes into
`MainWindowViewModel.Psk31AlcZone`, with nothing else to move. **What would settle
it: one reading off the radio while transmitting, with the needle at the top of
the marked zone.** Rejected: keeping 128 on the 0-255 assumption, because it is
wrong about the scale; and picking half of 120, because that is the same invention
with a smaller number.

### 2. The *heard, not readable yet* row - author's choice, awaiting the owner

**Ruling wanted, and nothing depends on it.** A held carrier with nothing read off
it yet shows as a row, faded to 0.55, with those words where the text goes. It
exists so an empty list and a band full of signals Hamlet cannot read look
different (§0.0, HM-DEC-092). It can be removed, or kept and not dimmed, in one
place. Rejected: a blank row, which reads as *he has not said anything yet*; and
showing nothing at all, which is the thing the operator asked about in the first
place.

### 3. The carry-forward cap says twenty and the keep-rules name twenty-three

**Ruling wanted.** Work instruction 324 task 1b caps the list at twenty and then
names keeps - five transmit-chain guards, every PSK31 type from units 314 to 322,
`BindingHealthTests` and `CallsignPrivacyTests` - that come to twenty-three. This
session kept all twenty-three and added its own three guards, because dropping a
named guard to satisfy an arithmetic is a coverage decision the owner did not make.
**26 types, two builds, 25 seconds.** If the cap is meant to bind, say which guards
come off; the three this unit added are the first candidates, since the ground they
cover is fresh.

### 4. The rate was not what silenced the 893 Hz station

**Raised, not blocking.** The work instruction's diagnosis - *at 48 kHz every bit
is 1 536 samples and nothing lines up* - was tested by reverting the wiring, and
the path read the 48 kHz fixture perfectly, CER 0.0000 on all four stations. The
search and the demodulator scale with the rate they are handed. The resample is
still right - the path runs at the one rate its thresholds were measured at, the
passband stops being a false claim, and the search does a sixth of the arithmetic
- but it is not the repair that makes his station readable. **The retire rule is**,
and the arithmetic above accounts for 1.9 s and zero characters exactly.

What remains unexplained is why a carrier 62 dB over the floor failed the
keying-shape measure within a second. **The thing that would answer it is the
two-minute WAV of 14.070 this phase has been asking for.** Every fixture behind
every number in this report is synthetic.

### 5. One file this session could not delete

**Housekeeping, no ruling needed.** `checknp.py` is at the repository root, two
lines, written to find out whether numpy was available for making a fixture. This
sandbox refuses every form of delete, so it is left untracked rather than
committed. Please delete it.

### Carried from unit 323, unresolved

Item 47 - no CI-V read for the ALC - **is closed by task 4b**. The rest are carried
as unit 323's instruction described them, and **this session could not reproduce
them verbatim**: `output.md` is overwritten in place, so unit 323's own wording is
gone.

- **The press events' order.**
- **The `no_transmit_path` token.**
- **`Psk31Macro.cs` misnamed.** Still `Psk31Macros.cs` on disk; nothing renamed.
- **The version at 1.13.9.** Now 1.13.10, by this unit's task 1.
- **The validator not runnable.** It ran here, through
  `dotnet build tools/arbiter/validate-output.proj`, and the result is at the foot
  of this report.

### Carried from unit 322's queue, per HM-DEC-139

Closed by ruling and reported as closed: **item 43** (drive and power, by §R11);
**item 45** (the pins, by §R12); **items 32 and 33** (the manual pages, in
`docs\psk31-reference.md`). **The quill cap of two is withdrawn by Tim,
2026-09-11** - *"I do not mind lots of achievement markers. It gives me an idea
there is a lot to do."* Every qualifying station is marked; not this unit's to
build; carried to 326.

Banked for 326 and untouched here: the collapsed panel with content, the
always-visible CQ/Everything filter, the card that waits on him from his own
transmission, the two forms of the quill, and the American-spelling sweep. **This
unit wrote American.**

**All others as unit 322 carried them**, and this session cannot reproduce those
verbatim either, for the same reason.

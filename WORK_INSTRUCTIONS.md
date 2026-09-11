# Work instruction 324 - PSK31 hears real air

**Seed for the relaunch under `--seed`.** Unit 323's first run was killed by the watchdog
at task 1 inside a 41-type carry-forward run; its second run completed - **the door is
open, all four macros go on the air, step 4 is `partial`** on two missing measurements.
This unit is small on purpose: the receive path on real air, the carry-forward list, and
the two measurements that make step 4 `done`. **Four tasks.**

---

## 0. The project gate

```
SHACK_FACTS.md                                          must exist
src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs     must exist
CoreHMI.sln                                             must not exist
MURC.sln                                                must not exist
root                                                    C:\Source\HamLet
```

**If any of the four is wrong, stop and say so in `output.md` section 4. Write nothing
else.** The refusal text: *This is not Hamlet. Nothing was changed.*

---

## 1. The rules that killed sessions - one of them killed the last one

**HM-DEC-155, Tim, 2026-09-05.** A unit runs no test suite; only its own names and the
carry-forward list, filtered, foregrounded, with a timeout. Never background and poll.

**The watchdog kills a session after twelve minutes with no write to `PROJECT_STATUS.md`.
It measures silence, not progress.** Unit 323 wrote one status at 17:43, then started 41
separate `dotnet test` runs - 41 builds - and was killed at 17:55 having done nothing
wrong except obey its instruction. So, in this unit and every one after:

- **Write `PROJECT_STATUS.md` immediately before every `dotnet test` and every `dotnet
  build`**, saying which. Not before the first one - before each one.
- **The carry-forward list runs as one invocation per test project**, with the names
  joined into a single `--filter`:
  `--filter "FullyQualifiedName~TypeA|FullyQualifiedName~TypeB|..."`. One build, not
  forty. Timeout 480 s.
- **If any single command could exceed eight minutes, split it or skip it and say so.**

---

## 2. The tool fact

**The shell breaks on an apostrophe inside a quoted heredoc and collapses a doubled
backslash.** Write *do not*; write single backslashes; check what landed on disk.
**Compound commands joined by `;` are refused; run them one at a time.**

---

## 3. Asks still outstanding

Carried per HM-DEC-139 from unit 322's queue. **Every item comes back in section 4,
verbatim where unresolved.** Closed by ruling tonight, report as closed:

- **Item 43** drive and power - **closed by §R11.** **Item 45** the pins - **closed by
  §R12.** **Items 32, 33** the manual pages - **closed**, in `docs\psk31-reference.md`.
- **The quill cap of two** - **withdrawn by Tim, 2026-09-11**: *"I do not mind lots of
  achievement markers. It gives me an idea there is a lot to do."* Every qualifying
  station is marked. Not this unit's to build; carry to 326.

New from Tim's evening at the radio, 2026-09-11, banked for units 324-326:

1. **PSK31 runs at 48 kHz and was proved at 8 kHz.** **This unit, task 2.**
2. **A collapsed panel with content is unmistakable.** Unit 326.
3. **The CQ / Everything filter is always visible**, on an empty list too. Unit 326.
4. **Retire a PSK31 carrier when the signal goes, not when the text pauses.** **This
   unit, task 3.**
5. **A carrier heard but not readable shows dimmed** - author's choice. **Task 3.**
6. **After you transmit to a station, the card waits on him from your transmission**, one
   full slot at least, undimmed. Unit 326.
7. **Two forms of the quill**: counters still in green; doors spinning in **orange**. Unit 326.
8. **American spelling** in every operator-facing string. Unit 326 sweeps; **this unit
   writes American.**

All others as unit 322 carried them.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet works PSK31 the way it works FT8.
UNIT GOAL:  A real PSK31 station on 14.070 becomes a row with text. Tonight it
            appears for two seconds with no text and vanishes. Then the
            carry-forward list stops being a suite, so the next unit lives.
ADVANCES:   Step 2's exit on real air; step 4 from partial to done if task 4
            lands its two measurements.
DRIFT:      0 carried from unit 323.
```

**Read from `%AppData%\Hamlet\telemetry\2026-09-11.jsonl`, sessions at 21:32 and 21:50,
version 1.13.8.** Tim on 14.070 with the app up:

- `psk31_listening_started`: **`sampleRate: 48000`, `passbandLowHz: 64`, `passbandHighHz:
  23936`.** The PSK31 path is taking the audio device's native rate. `Psk31Demodulator`
  and `Psk31CarrierSearch` were built and proved at **8000** - 256 samples per bit. At
  48 kHz every bit is 1,536 samples and nothing lines up.
- A real, strong carrier at **893 Hz**, 62-66 dB over the floor, appeared three times in
  a minute. **Each time: retired after 1.9 s, reason `LostLock`, 0 characters, 0 lines.**
  Fourteen minutes earlier: **17 carriers appeared, 0 characters emitted.**
- Why 1.9 s: `retireSeconds: 1`. The retire rule is *one second since the last
  character*. No character can arrive at the wrong rate, so every carrier is born and
  killed a second later. Tim: *"PSK is showing something in decoded but for only 2ish
  seconds then it all disappears."*

**Both faults are Hamlet's. Nothing about the radio, the level, or the band.** The search
found the station every time. That is what unit 322's telemetry was for, and it worked on
its first real evening.

---

## 5. Verify this instruction against the tree

**Names from units 314-322's reports.** Check; **report every mismatch; do not repair this
instruction; do not stop over a mismatch** unless a task is impossible.

- `PHASE_PLAN.md` at the root carries §R11-§R14. If not, **stop and say so.**
- `PHASE_OUTCOME.md` carries two `UNIT 323 - STEP 4` entries - the killed run and the
  completed one. **Leave both; append 324 after them.**
- Unit 323's items 47-53: no CI-V read for the ALC; the press events' order; the
  `no_transmit_path` token; `Psk31Macro.cs` misnamed; the version at 1.13.9; the
  validator not runnable. **Task 4 closes 47; the rest are carried.**
- Where the PSK31 path takes its audio: the source the FT8 decoder is fed from, its
  rate, and where `Psk31CarrierSearch` and each `Psk31Demodulator` receive samples.
  Whether the FT8 path resamples for `Ft8Sharp` and how.
- `Psk31Demodulator` - its samples-per-bit, its filters, its AFC, `SquelchQuality`.
  `Psk31CarrierSearch` - its window, `SignalHalfWidthHz`, `CandidateRatio`,
  `DynamicRangeDb`, and how the passband bounds are derived (today: from the rate).
- The retire rule and `retireSeconds`; `psk31_carrier_retired` and its reasons;
  `psk31_lock` (a proxy for *producing characters*).
- The PSK31 row type and how a row is shown, dimmed, retired.
- `docs\carry-forward-tests.txt` as it stands - **41 types**.
- Fixtures: `assets\fixtures\psk31-four-signals.wav` (8 kHz), `psk31-noise-only-30s.wav`,
  `manifest-step2.json`.
- Tests: `ThePsk31DemodulatorTests`, `ThePsk31CarrierSearchTests`,
  `ThePsk31HearsEveryoneTests`, `ThePsk31TelemetryTests`, `ThePsk31PanelSpeaksPsk31Tests`,
  `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint`.

---

## 6. Rulings in force

**`PHASE_PLAN.md`** §R1-§R14, unchanged. In particular **§R9**: a character the
demodulator is not sure of is not shown. **§R13**: every stage writes its event. **§R14**:
tests prove criteria and nothing beyond.

**Tim, 2026-09-11:** *"Nothing on the radio."* The operator adjusts nothing; Hamlet
adapts to the rate it is given.

**§0.0 / HM-DEC-092** - a row that appears is a row Hamlet is sure is a carrier; a row
Hamlet can hear but not read says so in words, not by looking like a readable one.
**§0.1** - the engine takes samples and a rate; it does not know about devices or tabs.
**§0.2** - nothing in this unit touches transmit. **HM-DEC-018, §2.1** - nothing personal
in an event. **HM-DEC-155**, **HM-DEC-139**, **FACT-004**, **FACT-006**, **the dummy load
withdrawn in full.**

---

## 7. Status cadence

`PROJECT_STATUS.md`: **after every task, at least every ten minutes, and immediately
before every `dotnet test` and `dotnet build`.** The watchdog fires at twelve minutes of
silence. **This is the rule that killed unit 323.**

---

## 8. The tasks

Four. Each names the test to watch failing first and one drop candidate.

### Task 1 - the carry-forward list stops being a suite

**1a.** Append `UNIT 324` to `PHASE_OUTCOME.md` under step 4. Patch-bump the version.

**1b. Prune `docs\carry-forward-tests.txt` to no more than twenty types.** Keep: every type
that guards the transmit chain (`TheUnslottedSendTests`,
`TheFt8AndFt4SendsAreByteIdenticalTests`, `ThePsk31ConversationCardTests`,
`ThePressingOfCqTests`, `TheCqReceiptTests`); every PSK31 type from units 314-322;
`BindingHealthTests.TheMainWindowBindsWithoutOneComplaint`; `CallsignPrivacyTests`. Drop
the rest and **record every dropped name in `docs\carry-forward-dropped.txt` with the
unit that added it**, so nothing is lost, only not re-run. Known reds never on either.

**1c. Write the one-invocation form** at the top of `carry-forward-tests.txt` as a comment:
the exact `dotnet test` line per project with the combined filter, so no future session
runs it forty times.

**1d. Run it that way, once, before anything changes.** Status write first. Report the
count and the wall-clock seconds. **If it exceeds six minutes, prune harder and say so.**

**Test watched failing first:** none.
**Drop candidate:** none. **Not droppable** - it is why 323 died.

---

### Task 2 - the PSK31 path runs at the rate it was proved at

**The rule.** The PSK31 path receives samples at **8,000 Hz**, whatever the device gives.
Resample once, at the boundary where the device's audio enters the PSK31 path - the same
place the FT8 path adapts for `Ft8Sharp`, if it does; report how FT8 does it and copy the
shape. `Psk31CarrierSearch` and `Psk31Demodulator` are **not changed** to take 48 kHz;
they are proved at 8 and stay at 8. The passband bounds come from the mode - **200 to
3,000 Hz** - not from the rate.

**Telemetry (§R13).** `psk31_listening_started` gains `deviceSampleRate` beside
`sampleRate`, and the resampler's ratio. `passbandLowHz`/`HighHz` read 200 and 3000.

**Test watched failing first:** `ThePsk31PathRunsAtItsRateTests`, app. Watch it fail, then
green:

1. the four-signal fixture, **resampled to 48 kHz on the way in** (the unit makes that
   file from the 8 kHz one and records its hash in `manifest-step2.json`), yields the same
   four carriers within 5 Hz and each decodes its text at or under 0.10 on its span
2. the noise-only fixture at 48 kHz yields zero carriers
3. `psk31_listening_started` reports `deviceSampleRate: 48000`, `sampleRate: 8000`,
   passband 200-3000
4. `ThePsk31DemodulatorTests` and `ThePsk31CarrierSearchTests` still green, **unedited**

**Drop candidate:** none. **Not droppable.**

---

### Task 3 - a carrier lives while the signal does

**The retire rule.** A carrier is retired when **the search no longer finds it** - the
candidate at its offset is gone or under the candidate bar for a stated number of
consecutive passes - **not when characters stop.** A PSK31 station idling between words
is a steady carrier with no characters for seconds at a time. **State the number of
passes and why.** Reasons: `SignalGone`, `ListeningStopped`. `LostLock` as a retire
reason goes away; *not producing characters* is a state of a live carrier, not its death.

**The row.** A held carrier whose squelch is closed - Hamlet can hear it, cannot yet read
it - **shows as a row, dimmed to unit 279's 0.55, with the words *heard, not readable
yet* in place of text**, and no offset-strength beyond what a readable row shows. When
the squelch opens and characters arrive, the row lifts and fills. **Author's choice,
marked as such; Tim may overrule.** It exists so an empty list and a band full of signals
Hamlet cannot read look different.

**Rename `psk31_lock` to `psk31_reading`**, since that is what it measures.

**Test watched failing first:** `ThePsk31CarrierLivesTests`, app. Watch it fail, then
green:

1. a fixture with a 6-second idle gap in the middle of a signal (the unit makes it from
   the reference convention and records its hash) keeps **one** carrier across the gap,
   retired once at the end with `SignalGone`
2. the four-signal fixture retires four carriers, each with `SignalGone`, none with a
   lifetime shorter than its signal
3. a held carrier with the squelch closed shows a dimmed row with the *heard* words and no
   text; when the squelch opens, the row lifts and text arrives
4. `psk31_reading` exists and `psk31_lock` does not; the privacy scan still green

**Drop candidate:** assertion 3, the dimmed row. Keep the retire rule.

---

### Task 4 - the two measurements that make step 4 done

**The state judge marked step 4 `partial` for two things, both measurements.**

**4a. Occupied bandwidth, stated and measured.** Step 4's exit says *occupied bandwidth
stated, under 100 Hz at -30 dB*. Modulate the CQ macro at 8 kHz, take its spectrum, and
report the width at -30 dB below the peak as a number in the report and in the test.
`ThePsk31ModulatorTests` gains one assertion.

**4b. The ALC has a read.** Unit 323's item 47: `RigField.Alc` exists and nothing fills
it. **The command is in the manual: CI-V command `15`, sub-command `13`, *Read ALC meter
level*, `00 00` = minimum to `01 20` = maximum - BCD, so the scale is 0 to 120.** Section
19 of the IC-7300 full manual, the CI-V command table, beside `15 11` (PO meter) and
`15 12` (SWR) which the tree already reads. Add the read to the poll **during a PSK31
send only**, cite it in `docs\psk31-reference.md`, and replace unit 323's zone figure of
128 - which was invented - with the manual's scale. **The zone itself**: the manual does
not give a number for data modes beyond *within the ALC zone*; **use the meter's marked
zone as the IC-7300 draws it, and if that cannot be sourced from the manual, say so and
leave the threshold as an ask rather than inventing a second number.** With the read in
place the sentence and the event unit 323 built can fire on a real radio. On this machine
there is no radio; prove the read reaches the poll and stops there.

**Test watched failing first:** the modulator assertion, and `TheAlcIsReadTests`, app:
the poll issues `15 13` only while a PSK31 send is keyed, never otherwise; the reading
is decoded from BCD on the 0-120 scale; with no radio the reading is absent and the
record says `measured: false`.

**Drop candidate:** 4b. Keep the bandwidth measurement; carry item 47.

---

## 9. Parked

- **Unit 326**: collapsed panels, the always-visible filter, the waiting card, the two
  quills, the spelling sweep.
- **Step 5.** The arbiter authors it after 326.
- **Real off-air audio.** Tim has a station at 893 Hz on 14.070 tonight; **a two-minute
  WAV of it is the fixture this phase wants.** Raised, not stopped for.
- **Any change to transmit. Any package.**

---

## 10. What not to do

- **No unfiltered `dotnet test`. No forty-invocation carry-forward. Never background and
  poll. Status before every `dotnet` command.**
- **Do not change `Psk31Demodulator` or `Psk31CarrierSearch` to run at 48 kHz.** Resample
  in.
- **Do not derive the passband from the sample rate.**
- **Do not retire a carrier for silence.**
- **Do not show a row Hamlet is not sure is a carrier**; show a *heard* row only for a held
  carrier.
- **Do not touch anything that keys, arms, composes or plays.**
- **Do not put a callsign, a grid or text in any event.**
- **Do not edit `PHASE_PLAN.md`, `PHASE_STATUS.md` or `PHASE_OUTCOME.md`** beyond the
  outcome append.
- **Do not add a package.**
- **Do not chase these known reds:** `CwAdjudicationTests.ASpeedChangeInRealisticAudio`;
  the 51 CW cases in `docs\unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests`
  whole-type-list tripwire; `HM-OPEN-088`'s ten; the two in `TheAchievementsScreenTests`;
  the one in `TheFitGuardAsksAboutTheGridTheSendIsOnTests`; unit 320's item 46.
- **Do not repair this instruction.** Report mismatches; keep working.
- **Write American.** Color, not colour.

---

## 11. Committing and pushing

Commit per task, push at the end. Nothing left uncommitted.

---

## 12. Reporting

`output.md` at the repository root. **Canonical headings:** `## 1. What Claude did`,
`## 2. What the owner should expect`, `## 3. What you should see`,
`## 4. What's blocking us`.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet works PSK31 the way it works FT8. Steps 0 to 3 done,
   4 in progress, 5 and 6 not started - unchanged by this unit.
B. Step 4 and its must-pass - unchanged; this unit repairs step 2 on real air and
   the ground under step 4.
C. The report last, and section 4 raises N items on top of the carried queue.
```

```
UNIT:       324 - <complete|stopped> at task N of 4, <which dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <step 4 to done if task 4 landed; else no>
NUMBER:     carry-forward types 41 -> <n>, run seconds <before> -> <after>;
            characters from the 48 kHz four-signal fixture 0 -> <n>;
            occupied bandwidth at -30 dB <n> Hz
DRIFT:      <n> consecutive units without advance  (carried)
```

**Section 2 must tell Tim, in plain words, what he will see on 14.070 now: the station at
893 Hz as a row with text; a station Hamlet can hear but not read as a dimmed row that
says so; and that he still touches nothing at the radio.**

**Every appearance claim is computed, not seen. Say so once.**

---

```
ARBITER-DECISION
STEP: 4
APPROACH: resample the PSK31 path to 8 kHz at the audio boundary, retire carriers on signal loss rather than silence, show heard-not-readable carriers dimmed, cut the carry-forward list to one invocation of at most twenty types, and measure the bandwidth and add the manual's ALC read
MOVE: continue
WHY: the record from the owner's evening shows a real carrier found every time and killed after 1.9 s with no characters, because the path runs at 48 kHz against an 8 kHz demodulator and retires on silence; and unit 323 died in a 41-type carry-forward run - both are repaired here before the door opens in 325
STATE: partial
DECIDED: the passband is 200-3000 Hz from the mode, not the rate; the retire-on-signal-gone pass count is the unit's to state; the heard-not-readable dimmed row is the author's choice marked for the owner
LICENCE: PHASE_PLAN.md R9, R13, R14; Tim 2026-09-11 nothing on the radio; CLAUDE.md 0.0, 0.1; HM-DEC-155 and the watchdog
ACCOMPLISHED: the station Tim heard at 893 Hz on 14.070 becomes a row with its text and stays while it is sending; and step 4 gets its two missing numbers
ADVANCES: step 2 on real air; step 4's two unmeasured criteria - bandwidth and the ALC read
END-ARBITER-DECISION
```

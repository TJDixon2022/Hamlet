```
READ IN THIS ORDER.

A. The phase goal - Hamlet works PSK31 the way it works FT8. Steps 0 to 3 done,
   4 blocked, 5 and 6 not started - unchanged by this unit.
B. Step 4 and its eight must-pass - unchanged; this unit is carried repair on the
   owner's ruling and advances no step.
C. The report last, and section 4 raises 6 items on top of a carried queue of
   forty-seven.
```

```
UNIT:       322 - complete at task 5 of 5, none dropped - 2026-09-11 17:05
PHASE GOAL: Hamlet works PSK31 the way it works FT8 - the same cards, the same
            one click, the same log, on a modem this project writes itself.
UNIT GOAL:  Every stage of the PSK31 path writes an event, so an empty list can
            be diagnosed from the file alone - band empty, or squelch shut.
ADVANCED:   no
NUMBER:     PSK31 events written 0 -> 38 on the four-signal fixture and 4 on the
            noise-only one; 13,708 bytes over 38.6 s of listening, about 355
            bytes a second; version 1.13.7 -> 1.13.8
DRIFT:      2 consecutive units without advance  (was 1)
```

**Every appearance claim in this report is computed, not seen**, and nothing here is
evidence about the radio: this machine has none and every fixture is synthetic
(FACT-004, FACT-006).

## 1. What Claude did

Gate passed on all four. Branch **`main`**, six commits, **nothing left uncommitted.**
Version **1.13.7 → 1.13.8**. The carry-forward list of 41 named types ran **255 of 255
green before anything changed** and **275 green after**, with this unit's four new types
on it.

### 1b, in one line

**It was not a code change.** The block that writes `state_changed digital_sub_mode` last
moved in unit 312 at 1.13.0 and nothing touched it between 1.13.2 and 1.13.4. It fires
from a generated property setter that short-circuits on an equal value, and the
constructor restores the remembered sub-mode by **assigning the backing field**, so once
PSK31 was the remembered mode, **pressing PSK31 was not a change and nothing was
written.** It is now written from the press, so a press that moves nothing still says so.

### Every event, when it fires, and which assertion proves it

| event | when | proved by |
| --- | --- | --- |
| `psk31_listening_started` | the tab is pressed and the listener opens | `PressingTheTabIsWrittenAndSoIsLeavingIt` |
| `psk31_listening_stopped` | the tab is left | same |
| `state_changed digital_sub_mode` | every press, changed or not | same, three presses three lines |
| `psk31_search_pass` | the set changes, else every 10 s | `NoiseProducesCandidatesThatNeverCrossAndNoCarriers` |
| `psk31_carrier_appeared` | a candidate crosses | `TheFourSignalFixtureGivesFourAppearancesAndFourRetirements` |
| `psk31_carrier_retired` | the retire rule fires, or listening stops | same |
| `psk31_squelch` | a held carrier opens or closes | the four-signal record, section 3 |
| `psk31_lock` | characters start or stop, or the AFC moves 2 Hz | same |
| `psk31_line_parsed` | the parser returns a verdict | `EveryParsedLineWritesItsVerdict`, 32 of 32 |
| `psk31_audio_level` | sampled, like `decode_quality` | the noise record, section 3 |
| `psk31_send_composed` | a macro is composed | `EachOfTheFourMacrosWritesWhatItComposed` |
| `psk31_send_refused` | the gate, the cap or the bolt refuses | `APsk31PressIsRefusedAndNothingReachesTheTransmitter` |
| `psk31_send_keyed` / `_unkeyed` | the keying site and its `finally` | `NothingPersonalReachesAnyOfIt` |
| `psk31_radio_after_send` | after unkey, from the next rig poll | `WithNoRadioTheRecordSaysTheRadioDidNotAnswer` |

### The four-signal fixture, end to end

**46 events, 13,708 bytes, 38 of them `psk31`, over 38.6 seconds of audio** — about **355
bytes a second of listening**, so a fifteen-minute session is around 320 kB. The
noise-only fixture: **12 events, 4,803 bytes, 4 of them `psk31`, over 30 seconds.**

### The privacy scan

**Clean, and scanned rather than trusted.** Every byte of every event written by the
receive and transmit assertions, against ten fixture callsigns, five grids, and six words
from the fixture text. **Nothing personal reaches the file.**

**And the privacy walk itself was already dark before this unit touched anything.**
`CallsignPrivacyTests` pinned 65 methods against 71: **six events added by units 315 to
320 had no line in the walk**, so for six events the scan was not running at all. Each
payload was read by hand - a server, a device, a mode, counts and two stable tokens,
nothing personal - and all six are in the walk now, which is green again.

### Three faults found by the new tests and fixed

1. **Leaving PSK31 for FT8 cleared nothing.** `FollowTheChosenMode` returns early when the
   mapped `DigitalMode` has not moved, and PSK31 and FT8 both map to `Ft8` - so the rows,
   the cards and the listener all survived under the other tab. It is the *label* that
   decides whether this mode is running.
2. **A carrier still being heard when listening stopped vanished from the record.** The
   file said it appeared and never said what became of it. Every appearance is accounted
   for now, with `ListeningStopped` as its own reason.
3. **Two clocks in one record.** Carrier lifetimes come from the search's sample count and
   the listening duration came from the wall clock, so a replay wrote *listened 0.2 s*
   beside *a carrier lived 34.7 s*. Every duration is in audio seconds now.

### Seven mismatches with the instruction, reported and not repaired

1. **1b assumes a code change and there was none** - the state changed, not the code.
2. **`appVersion` in `psk31_listening_started`'s data would be a second copy.** Schema B
   stamps it on every line already, so it is not repeated in the bag.
3. **"One `psk31_line_parsed` per line" cannot hold through the splitter.** `05-garbled`
   line 3 carries no turnover, so the splitter folds it into the next message - unit 316's
   ruled behaviour - and gives 31 for 32 lines. The corpus pairs lines with verdicts, so
   it is a parser fixture; through the parser it is 32 of 32.
4. **The composed seconds are 1.54 s longer than unit 317's table**, every one. Not drift:
   `IdleBitsBefore` 32 plus `IdleBitsAfter` 16 is **1.536 s exactly** at 31.25 baud. The
   table counted the text; the event reports what would be keyed, which is what the cap is
   measured against.
5. **Four of the six transmit events have no production call site**, because the step-4
   press half is blocked: `CanTransmitIn` still answers false for PSK31 and nothing in
   `src/Hamlet.App` reaches `UnslottedTransmission`. They are proved at the bench.
6. **The neighbourhood outline does not reproduce.** With PSK31 chosen on 20 m, exactly one
   block of thirteen is picked out and it is PSK31 at 14.070. Unit 312's mechanism works;
   what the screenshot shows cannot be reproduced from the view model.

7. **The ordering block the instruction prints will not validate.** Section 12 gives it as
   three lines, `A.` to `C.`; `validate-output.bat` rule 6 also requires a `READ IN THIS
   ORDER.` header above them and refuses the block without it. The header is written here
   and the instruction is not repaired.

**And item 17 does not reproduce either**: `validate-output.bat` ran here without being
refused.

### Tests

**No suite was run**; every name filtered, foregrounded, 480 s timeout (HM-DEC-155).

| Test type | Result |
| --- | --- |
| `ThePsk31TelemetryTests` (new, task 2) | **6 of 6**, watched failing first |
| `ThePsk31TransmitTelemetryTests` (new, task 3) | **5 of 5**, watched failing first |
| `ThePsk31PanelSpeaksPsk31Tests` (new, task 4) | **5 of 5**, watched failing first |
| `Unit322Dump` (new, task 5, a tool) | asserts nothing; writes the two files |
| `CallsignPrivacyTests` | **4 of 4**, red before this unit and green now |
| The carry-forward list, 41 types | **255 of 255** before, **275** after |

**Nothing new is red.** The known reds in section 10 were not run and not chased.

## 2. What the owner should expect

**The build is clean** - zero warnings, zero errors.

**Five minutes on 14.070 will now put about 100 kB in the file**, and it will answer the
question you asked. Here is how to read the one line that matters.

Open `%AppData%\Hamlet\telemetry\<today>.jsonl` and find `psk31_search_pass`. Each one
lists what the search saw:

- **No candidates at all** → the band really was quiet.
- **Candidates with `"quality": 0`** → there was energy there, and nothing keyed like
  PSK31. Noise.
- **Candidates with a quality between about 0.4 and 0.9 and `"crossed": false`** → **this
  is the squelch being too tight for real air.** Signals were there, they were keyed, and
  Hamlet would not take them. That is the finding to send back.

Two lines either side of it tell you the rest. `psk31_audio_level` says whether any audio
arrived at all - a peak near −90 with `nearlySilent true` is a cable or a device, not a
band. And `psk31_listening_started` says what the thresholds were that evening and **what
dial you were on**, which is worth checking first: a dial that is not on the watering hole
explains an empty list on its own.

**What has changed on the screen.** The waterfall caption no longer says *15 s slots*
under PSK31; it says the passband and the mode. The empty decoded panel no longer promises
a message per slot; it says every station Hamlet is sure of gets a line as its text
arrives, and that a signal it is not sure of gets none rather than a guess.

**Nothing else moved.** No behaviour of the transmit chain was touched, the PSK31 tab
still cannot send, and no event handler keys, arms or composes anything.

**Pushed to `main`**, six commits, nothing uncommitted.

## 3. What you should see

**The one line that answers the question, from the four-signal fixture:**

```
psk31_search_pass · changed · 10 candidates · 4 held
   1600 Hz  +4.0 dB  quality 0.986  crossed
    700 Hz  +1.5 dB  quality 0.975  crossed
   1100 Hz  -1.5 dB  quality 0.943  crossed
   2200 Hz  -4.5 dB  quality 0.880  crossed
   1985 Hz  -16.9 dB quality 0.234  -
   1758 Hz  -18.3 dB quality 0.322  -
```

**Four taken, two refused, and the numbers say why.**

**And from the noise-only fixture, the same event:**

```
psk31_audio_level   peak -16.2 dB · rms -26.2 dB · clipping false · nearlySilent false
psk31_search_pass · sampled · 15 candidates · 0 held · none crossed
   1690 Hz  -11.1 dB  quality 0
   3321 Hz  -12.8 dB  quality 0
    750 Hz  -13.1 dB  quality 0
psk31_listening_stopped  30.0 s · 0 carriers · 0 characters · 0 lines
```

**Audio present, search running, fifteen places measured, nothing keyed.** The band was
noise, and each of the three other explanations is ruled out by its own line.

**How each carrier ended:**

```
1099.2 Hz  LostLock          28.8 s  101 chars  2 lines   2.0 s since it last spoke
1607.9 Hz  LostLock          33.8 s  120 chars  3 lines   1.8 s
 699.2 Hz  LostLock          34.7 s  120 chars  3 lines   2.5 s
2200.0 Hz  ListeningStopped  38.6 s  142 chars  2 lines   0.1 s
```

**The new panel copy, word for word:**

```
200–3000 Hz · PSK31, one continuous carrier a station
```

```
nothing decoded yet. Every station Hamlet is sure is sending PSK31 gets a line
here, filling in a character at a time as it arrives. A signal it is not sure of
gets no line at all rather than a guess, so an empty panel can mean a quiet band
or a signal too rough to read.
```

**The full reading of both files is `docs/psk31-telemetry-reading.md`**, two pages, written
from the record and nothing else.

## 4. What's blocking us

Step 4 stays `blocked` on the two rulings this unit did not touch. Six items.

1. **`locked` is a proxy and the name overpromises.** The demodulator exposes no bit-clock
   lock, so `psk31_lock` reports *this channel has produced characters*. It usefully
   separates a carrier being read from one held and silent, and it is not what the word
   says. Either the demodulator exposes a real lock, or the field is renamed.

2. **The sampling cadence is on the wall clock while every duration is in audio seconds.**
   Deliberate - sampling throttles writes to a file, which is a real-time concern - and it
   shows in a replay, where 38.6 s of audio produce one `psk31_audio_level` instead of
   four. Worth a ruling if replays are ever to be read as though they were evenings.

3. **Nothing in the record says what was on screen.** It says a line was parsed; it does
   not say whether the row was visible, scrolled away, or filtered out. That is the next
   thing a screenshot would still be needed for.

4. **The eighth telemetry category.** HM-DEC-018 named six and this unit adds `Psk31` as an
   eighth. The ruling's substance - what may be in the file, and who may switch it off - is
   unchanged, and both hold. Named here because the count is in the ruling's own words.

5. **Item 43, drive and power, is still unruled** and this unit built nothing for it. The
   two manual pages are now recorded in `docs/psk31-reference.md`, marked
   cited-at-one-remove because no session here opened the manual.

6. **A leftover file is gone and that is worth one line.** `Psk31Listening.cs` had been
   emptied by unit 315 with a comment asking for it to be deleted by hand, because that
   session's shell refused `rm` and `git rm`. This one could, and did.

### Asks still outstanding

Carried per HM-DEC-139, from unit 320's forty-seven. The ones this unit touched:

- **Item 43** - drive and power against HM-DEC-084. **Put to Tim by the web thread;
  unruled. This unit builds nothing for it** but instruments what exists.
- **Item 45** - the `NothingOnTheCardTransmits` pins. **Put to Tim; unruled.** Not touched.
- **Items 32 and 33** - the manual page. **Answered by the web thread from the manual in
  its project knowledge:** USB MOD Level is `MENU » SET > Connectors`, default 50%, range
  0-100%, **page 12-10**; data-mode level is **page 4-31**, *adjust the device's output
  level within the ALC zone*. **Recorded in `docs\psk31-reference.md`.** The session still
  cannot read outside the tree; that stands.
- **Item 46** - the intermittent red in `ThePressingOfCqTests`. Not chased. **Green on
  every run this session.**
- **Item 17** - `validate-output.bat` refused. **Tried; it was not refused here.** It ran
  and reported.
- **Ask 1** - does the transmission record ask the radio whether it keyed. **Now a
  measurement**: `psk31_radio_after_send` carries what `1C 00` and `15 11` reported with
  the age of each reading, and with no radio it says `answered: false` with every field
  absent rather than zero. **The event has no production call site yet**, because the press
  half of step 4 is blocked.

All others as unit 320 carried them.

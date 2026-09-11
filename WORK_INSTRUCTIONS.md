# Work instruction 322 - the PSK31 path becomes visible in the record

**Single session, not the loop.** The loop is halted at step 4 `blocked` on two rulings Tim
has not given. This unit runs alone, on Tim's ruling of 2026-09-11: *"our prime focus in the
next unit … is to enhance the telemetry in all facets so that it is usable to diagnose any
issue."* It advances no step. It is the reason the next five minutes at the radio will mean
something.

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

## 1. The two rules that killed sessions

Both HM-DEC-155, Tim, 2026-09-05.

**1. A unit runs no test suite.** Only this unit's own test names and
`docs\carry-forward-tests.txt`, filtered by exact name, foregrounded, with a stated
timeout: `timeout 480 dotnet test <project> --filter "FullyQualifiedName~Type.Method"`.
Known reds are in section 10 and never on the list.

**2. Never background a command and poll it.** The watchdog fires at **twelve minutes with
no status write.**

---

## 2. The tool fact

**The shell breaks on an apostrophe inside a quoted heredoc and collapses a doubled
backslash.** Write *do not*; write single backslashes; check what landed on disk.
**Compound commands joined by `;` are refused; run them one at a time.**
`tools\arbiter\validate-output.bat output.md` has been refused to the last three sessions;
try it once, and if refused, check the seven rules by hand and say so.

---

## 3. Asks still outstanding

Carried per HM-DEC-139, from unit 320's forty-seven. **Every item comes back in section 4,
verbatim where unresolved.** The ones this unit touches:

- **Item 43** - drive and power against HM-DEC-084. **Put to Tim by the web thread; unruled.
  This unit builds nothing for it** but instruments what exists.
- **Item 45** - the `NothingOnTheCardTransmits` pins. **Put to Tim; unruled.** Not touched.
- **Items 32 and 33** - the manual page. **Answered by the web thread from the manual in
  its project knowledge:** USB MOD Level is `MENU » SET > Connectors`, default 50%, range
  0-100%, **page 12-10**; data-mode level is **page 4-31**, *adjust the device's output level
  within the ALC zone*. Record both in `docs\psk31-reference.md`. The session still cannot
  read outside the tree; that stands.
- **Item 46** - the intermittent red in `ThePressingOfCqTests`. Not chased.
- **Item 17** - `validate-output.bat` refused. Try once; report.
- **Ask 1** - does the transmission record ask the radio whether it keyed. **This unit
  sharpens it into a measurement**: task 4 records what the radio reports after a PSK31
  send, and says whether the answer is there or not.

All others as unit 320 carried them.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet works PSK31 the way it works FT8.
UNIT GOAL:  Every stage of the PSK31 path - audio in, search, squelch, lock,
            demodulate, parse, and on the transmit side compose, cap, arm, key,
            play, and what the radio said back - writes an event that lets a
            person diagnose the path from the file alone, with no screenshot.
ADVANCES:   No step. Carried repair on steps 1 to 4, on the owner's ruling.
DRIFT:      carried from PHASE_OUTCOME.md; expect it to rise by one.
```

### What the record shows today, measured

`%AppData%\Hamlet\telemetry\2026-09-11.jsonl`, read by the web thread: **12,587 events, ten
sessions, versions 1.12.271 to 1.13.4.** The FT8 path wrote 3,448 `ft8_slot`, 3,419
`decodes_drawn`, 2,317 `decode_quality`. **The PSK31 path wrote nothing.** Not one event
from the search, the squelch, a demodulator, or the parser, in any session, at any version -
including 1.13.2 where the demodulator was wired and 1.13.4 where the whole receive path was
running. The only PSK31 trace in the file is four `state_changed` events for
`digital_sub_mode`, and in the 1.13.4 session **even that is absent**: Tim was on the PSK31
tab for five minutes with the search running, the file records a CW decoder start and
nothing else.

**So this question cannot be answered from the record:** *the list was empty for five
minutes - was the band empty, or was the squelch too tight for real air?* Every threshold
in the path was set against synthetic fixtures. The first real signal it meets will either
appear or not, and if not, nothing today says why.

**HM-DEC-018** set the telemetry's shape: six categories, switchable, size-capped, **no
machine id, no callsigns, no message content, no upload.** §2.1: nothing personal. That
shape holds. What is added is events, not fields that would break it.

### What the FT8 path already does, which PSK31 copies

`decoder_started`, `digital_decoder_started`, `decode_quality` (input peak, RMS, floor,
clipping, nearly-silent), `ft8_slot`, `decodes_drawn`, `send_stage`, `ft8_transmission`,
`state_changed`, `operator_action`. **Read them first.** The PSK31 events use the same
envelope - `ts`, `sessionId`, `level`, `appVersion`, `category`, `event`, `data` - the same
switch, and the same size cap.

---

## 5. Verify this instruction against the tree

**Names below come from units 314 to 320's reports.** Check; **report every mismatch; do
not repair this instruction; do not stop over a mismatch** unless a task is impossible.

- The telemetry writer and its categories; where `decode_quality` is sampled; the size cap;
  the switch per category.
- `Psk31CarrierSearch` and its rule and numbers; `Psk31Demodulator`,
  `Psk31Demodulator.SquelchQuality` (0.90), its AFC and lock; the retire rule and its number;
  the parser (unit 316) and its per-line verdict shape - speaker, addressee, kind, turnover,
  certainty; `Psk31Listening`.
- The transmit side as units 318-320 left it: the modulator, the unslotted `OperatorSend`,
  the cap and its number, `SendMessage`, the bolt, `CanTransmitIn`, the transmission record
  for a no-slot send.
- Why `state_changed` for `digital_sub_mode` stopped firing between 1.13.2 and 1.13.4.
- The PSK31 panel: the waterfall header (*15 s slots*), the empty-list copy (*every message
  that comes out of a slot*), and the neighbourhood map outline unit 312 built.
- `docs\carry-forward-tests.txt`; `assets\fixtures\` - the seven from unit 314, the two from
  the phase delivery, `psk31-transcripts\corpus.json`.
- Tests: `ThePsk31CarrierSearchTests`, `ThePsk31HearsEveryoneTests`,
  `ThePsk31DemodulatorTests`, the parser tests from unit 316, `ThePsk31ModulatorTests`,
  `TheUnslottedSendTests`, `TheFt8AndFt4SendsAreByteIdenticalTests`, `ThePsk31TabIsInertTests`,
  and whatever tests the FT8 telemetry payloads (`tests\Hamlet.App.Tests` - HM-DEC-131 names
  *telemetry payloads* as app-layer facts with public promises).

---

## 6. Rulings in force

**Tim, 2026-09-11:** *"the telemetry should be rich and deep and capable of … being used to
diagnose any problem that comes up."* And: *"our prime focus in the next unit … is to enhance
the telemetry in all facets so that it is usable to diagnose any issue."*

**HM-DEC-018** - the telemetry's shape: six categories, all on, switchable; no machine id,
no callsigns, no message content, no upload; size-capped. **§2.1** - nothing personal. **A
PSK31 event carries an offset, a score, a kind, a count, a reason. Never a callsign, never
text, never a grid.**

**HM-DEC-111** - a provenance label carries its age. An event that reports a value the radio
gave says how old it is.

**HM-DEC-131** - the status file is not a log; the telemetry record is.

**§0.0 / HM-DEC-092** - never present a guess as a decode. **An event says what was
measured, and a field that was not measured is absent, not zero.**

**`PHASE_PLAN.md`** at the root, §R1-§R10, unchanged by this unit. **§6 branching**: anything
touching the transmit chain beyond R10 is `MOVE: stop`. **This unit reads the transmit
path and writes events from it; it changes no behaviour of it.**

**§0.2** - one click, one transmission. **No event handler may key, arm or compose anything.**

**HM-DEC-155**, **HM-DEC-139**, **FACT-004**, **FACT-006**, **the dummy load withdrawn in full.**

---

## 7. Status cadence

`PROJECT_STATUS.md` per `CLAUDE.md` §13: **after every task, and at least every ten
minutes.** Write the status **before** starting a test run.

---

## 8. The tasks

Five. Each names the test to watch failing first and one drop candidate. **Drop from the
back.**

### Task 1 - trace, and the two facts that must be answered first. No production changes.

**1a.** Append `UNIT 322` to `PHASE_OUTCOME.md` under step 4 as carried repair advancing no
step. Patch-bump the version. Run `docs\carry-forward-tests.txt` filtered before anything
changes; report the counts.

**1b. Why did `state_changed` for `digital_sub_mode` stop firing?** At 1.13.2 pressing PSK31
wrote it; at 1.13.4 it did not. Find the change, with commit and line. **Do not fix it in
this task**; task 2 owns it.

**1c. Read the FT8 telemetry path** - the writer, `decode_quality`'s sampling rule, the size
cap, and the app-layer test that pins its payload shape. **Task 2 copies that shape.**

**1d. Map the PSK31 path to its stages.** With file and line, every point where one of these
happens and nothing is recorded: audio arrives on the PSK31 path; the search runs a pass and
scores candidates; a candidate crosses the threshold and a carrier appears; a demodulator
locks, loses lock, moves its AFC; the squelch opens and closes; a character is emitted; a
line is split and parsed; a carrier is retired. And on the transmit side: a macro is
composed; the cap is checked; a no-slot send is armed, keyed, played, unkeyed; the bolt
refuses; the record is written; what the radio reported after.

**1e. Record the manual pages** from section 3 in `docs\psk31-reference.md`.

**Test watched failing first:** none.
**Drop candidate:** none. **Not droppable.**

---

### Task 2 - the receive path writes its record

Category `psk31`, on by default, switchable like the other six. **Every event carries the
envelope the FT8 events carry.** No callsign, no text, no grid, anywhere in `data`.

**The events, and what each must carry.** Names are the unit's if the tree has a convention;
the fields are not optional.

| event | when | data |
| --- | --- | --- |
| `psk31_listening_started` | the tab is pressed and the search starts | dial Hz, passband low and high Hz, sample rate, search interval, squelch threshold, retire seconds, app version |
| `psk31_listening_stopped` | the tab is left or the app stops | seconds listened, carriers seen, characters emitted, lines parsed |
| `psk31_search_pass` | **sampled** - once every ten seconds, and always on the pass where the candidate set changes | pass duration ms, candidate count, **each candidate: offset Hz, strength dB, quality score, crossed threshold** (capped at the twenty strongest), carriers currently held |
| `psk31_carrier_appeared` | a candidate crosses and a demodulator is created | offset Hz, strength dB, quality, how many passes it was a candidate before crossing |
| `psk31_carrier_retired` | the retire rule fires | offset Hz, reason (silence / lost lock / below threshold), seconds since last character, characters emitted, lines parsed, lifetime seconds |
| `psk31_squelch` | the squelch opens or closes on a held carrier | offset Hz, open true/false, quality at the moment, threshold |
| `psk31_lock` | a demodulator gains or loses bit-clock lock, or its AFC moves more than 2 Hz | offset Hz, locked true/false, AFC offset from the carrier's first estimate Hz |
| `psk31_line_parsed` | the parser returns a verdict for a line | offset Hz, kind, certain true/false, turnover true/false, addressed to the operator true/false, character count, **and nothing else** |
| `psk31_audio_level` | sampled like `decode_quality`, on the PSK31 path | peak dB, RMS dB, floor dB, clipping, nearly silent |
| `state_changed` `digital_sub_mode` | restored - fix what 1b found | as before |

**The rate.** `psk31_search_pass` is the expensive one; it is sampled, and the sample rule is
stated in one place. Everything else fires on change and is cheap. **Measure**: the
four-signal fixture end to end produces how many events and how many bytes; report both.

**Test watched failing first:** `ThePsk31TelemetryTests`, app project. Watch it fail, then
green:

1. the four-signal fixture, played through the PSK31 path, writes **exactly four
   `psk31_carrier_appeared` and four `psk31_carrier_retired`**, at 700, 1100, 1600 and
   2200 Hz within 5 Hz, each retire with a reason and a character count
2. the noise-only fixture writes `psk31_search_pass` events whose candidates **all have
   `crossed: false`**, and **zero** `psk31_carrier_appeared`
3. the corpus, fed through the parser, writes **one `psk31_line_parsed` per line** with
   `kind` and `certain` matching `corpus.json`
4. **privacy**: over every event written in assertions 1-3, no `data` value contains any
   callsign from the fixtures (`KC3QIS`, `W1AW`, `EI4GNB`, `F4DIA`, `VE3XN`, `G4XYZ`, `K3ABC`,
   `N4ZEK`, `KD8WYT`, `JA1XYZ`), any grid from them, or any run of five or more letters from
   the fixture text. **Assert by scanning the serialised JSON.**
5. pressing PSK31 writes `psk31_listening_started` and `state_changed digital_sub_mode`;
   leaving it writes `psk31_listening_stopped`
6. the `psk31` category switch off writes none of them and changes nothing else

**Drop candidate:** `psk31_lock`'s AFC-movement trigger. Keep gain and loss of lock.

---

### Task 3 - the transmit path writes its record

**Read-only on behaviour. §6 of the plan and §0.2: no event handler keys, arms or composes
anything. If instrumenting a point would change its behaviour, do not instrument it; report
it.**

| event | when | data |
| --- | --- | --- |
| `psk31_send_composed` | a macro is composed | macro kind (cq / answer / report / confirm), character count, seconds at 31.25 baud, cap seconds, within cap true/false, offset Hz |
| `psk31_send_refused` | the bolt, the cap, or the certainty gate refuses | reason, macro kind if known, stage |
| `send_stage` | **the existing event**, for a no-slot send | as today, plus `slotted: false` and the cap |
| `psk31_send_keyed` / `psk31_send_unkeyed` | the one `PttOn` site and the `finally` | seconds keyed, planned seconds, aborted true/false |
| `ft8_transmission` or its no-slot sibling | the record | mode, no slot where there was none, played true/false, seconds |
| `psk31_radio_after_send` | after unkey, from the next rig poll | what `1C 00` and `15 11` reported, **with the age of each reading** (HM-DEC-111), or `unanswered` |

**Assertion 4 of this task is ask 1 turned into a measurement**: does the record now say
what the radio did? On this machine the radio is absent, so the event says `unanswered` -
**and that absence is the finding, written as an event rather than a silence.**

**Test watched failing first:** `ThePsk31TransmitTelemetryTests`, app project. Watch it
fail, then green:

1. composing each of the four macros at the bench writes `psk31_send_composed` with the
   right kind and a seconds value within 0.5 s of the arithmetic in unit 317's table (9.9,
   6.3, 23.2, 14.7)
2. a macro over the cap writes `psk31_send_refused` with reason `cap` and **no `send_stage`**
3. with the bolt shut, a PSK31 press writes `psk31_send_refused` with reason `bolt` and
   **nothing reaches `send_stage`, `psk31_send_keyed` or the record**
4. `psk31_radio_after_send` exists and, with no radio, says `unanswered` with no invented
   values
5. privacy as task 2's assertion 4, over every event here
6. `ThePsk31TabIsInertTests`, `TheUnslottedSendTests` and
   `TheFt8AndFt4SendsAreByteIdenticalTests` still green and unedited

**Drop candidate:** `psk31_radio_after_send`. Report it as still ask 1 if dropped.

---

### Task 4 - the panel stops speaking FT8 under PSK31

Three leftovers from Tim's screenshot of 2026-09-11, 17:22 UTC:

- the waterfall header reads *200-3000 Hz · 15 s slots*. Under PSK31 there are no slots; the
  header says the passband and the mode.
- the empty decoded-text copy reads *every message that comes out of a slot lands here*.
  Under PSK31 it says, in the same voice, that every station Hamlet is sure is PSK31 gets a
  line as its text arrives.
- the neighbourhood map shows *CW main street* with 14.070 in the RTTY block. Unit 312's
  PSK31 outline is not lit with the tab selected. Find why; light it.

`VoiceTests` runs, because this changes copy the operator reads.

**Test watched failing first:** `ThePsk31PanelSpeaksPsk31Tests`, app. Watch it fail, then
green: no string containing *slot* is bound on the PSK31 panel; the outline for the
selected mode is the one lit. Re-run `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint`.

**Drop candidate:** the map outline. Keep the two strings.

---

### Task 5 - a diagnosis written from the file, by hand

**Drop candidate: this whole task.** It writes no production code.

Play the four-signal fixture through the running PSK31 path with telemetry on. Take the
resulting `.jsonl` and **write, in `docs\psk31-telemetry-reading.md`, the diagnosis a
person would make from the file alone**: when listening started and at what threshold,
what the search saw pass by pass, which carriers appeared and when, what the squelch did,
what the parser concluded, when each carrier was retired and why. **Then do the same for
the noise-only fixture.** Two pages. If the file cannot answer a question a person would
ask, that is a missing event, and it goes in section 4 by name.

**Test watched failing first:** none.
**Drop candidate:** the whole task.

---

## 9. Parked

- **Drive and power, item 43.** Unruled. Build nothing.
- **The `NothingOnTheCardTransmits` pins, item 45.** Unruled. Touch nothing.
- **The CQ press and receipt on PSK31.** Step 4's, after the rulings.
- **Steps 5 and 6.**
- **Reading `fldigi` or the manual from disk.** The session cannot; the pages are given.
- **The intermittent red, item 46.** Not chased.
- **`Avalonia.Headless.Skia` or any package.**
- **Any change to what keys, arms, composes or plays.**

---

## 10. What not to do

- **No unfiltered `dotnet test`.** **Never background and poll.**
- **Do not put a callsign, a grid, or message text in any event.** HM-DEC-018, §2.1.
- **Do not write a field that was not measured.** Absent, not zero, not `unknown` unless the
  radio was asked and did not answer.
- **Do not let an event handler key, arm, compose or play anything.**
- **Do not touch the transmit chain's behaviour.** Read it; write events from it.
- **Do not edit `NothingOnTheCardTransmits` or any carry-forward test.**
- **Do not edit `PHASE_PLAN.md`, `PHASE_STATUS.md` or `PHASE_OUTCOME.md`** beyond the outcome
  append.
- **Do not add or vendor a package.**
- **Do not claim an appearance from computation and call it seen.**
- **Do not chase these known reds:** `CwAdjudicationTests.ASpeedChangeInRealisticAudio`;
  the 51 CW cases in `docs\unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests`
  whole-type-list tripwire; `HM-OPEN-088`'s ten; the two in `TheAchievementsScreenTests`; the
  one in `TheFitGuardAsksAboutTheGridTheSendIsOnTests`; item 46.
- **Do not repair this instruction.** Report mismatches; keep working.

---

## 11. Committing and pushing

Commit per task, push at the end. Nothing left uncommitted; say so in section 1.

---

## 12. Reporting

`output.md` at the repository root. **Canonical headings:** `## 1. What Claude did`,
`## 2. What the owner should expect`, `## 3. What you should see`,
`## 4. What's blocking us`.

**The ordering block first:**

```
A. The phase goal - Hamlet works PSK31 the way it works FT8. Steps 0 to 3 done,
   4 blocked, 5 and 6 not started - unchanged by this unit.
B. Step 4 and its eight must-pass - unchanged; this unit is carried repair on the
   owner's ruling and advances no step.
C. The report last, and section 4 raises N items on top of a carried queue of
   forty-seven.
```

Then the header:

```
UNIT:       322 - <complete|stopped> at task N of 5, <which dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   no
NUMBER:     <events written per fixture, bytes per minute of listening>
DRIFT:      <n> consecutive units without advance  (carried, plus one)
```

**Section 1 must carry: the answer to 1b in one line; a table of every event, when it
fires, and which assertion proves it; the four-signal fixture's event count and bytes; and
the privacy scan's result.**

**Section 2 must tell Tim, in plain words, what five minutes on 14.070 will now put in the
file, and how to read the one line that says whether the band was empty or the squelch was
shut.**

**Every appearance claim is computed, not seen. Say so once.**

---

```
ARBITER-DECISION
STEP: 4
APPROACH: instrument every stage of the PSK31 receive and transmit path with events in a psk31 category, proved by assertion against the shipped fixtures, changing no behaviour
MOVE: continue
WHY: the owner ruled telemetry the prime focus; the record shows the PSK31 path writing nothing across ten sessions, so the question the owner asked at the radio cannot be answered from the file; this unit makes it answerable and touches no step criterion
STATE: blocked
DECIDED: the event set and its privacy rule are the instruction's; the event names follow the tree's convention if one exists; the search-pass sampling rule is the unit's to state
LICENCE: Tim 2026-09-11 on telemetry; HM-DEC-018; section 2.1; HM-DEC-111; HM-DEC-131; PHASE_PLAN.md section 6 for the transmit chain untouched
ACCOMPLISHED: after this, an empty PSK31 list can be diagnosed from the file - band empty or squelch shut - without a screenshot
ADVANCES: none
END-ARBITER-DECISION
```

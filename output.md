READ IN THIS ORDER.

A. The phase goal - Hamlet works Olivia the way it works PSK31. Steps 0-3 done
   (step 3 by plan section 8's revision, written into PHASE_STATUS.md at task 0);
   step 4 was 0 of 8 at the start of this unit; steps 5 and 6 not started.
B. Step 4, Say it. This unit aimed at 4.1 (loopback identical at each variant, and
   tone spacing, symbol rate, preamble and occupied bandwidth within the stated
   tolerances of the author's audio), 4.2 engine half (the composed burst read
   back as the variant sent) and 4.4 engine half (cap and patience as timing-table
   characters, a Report at 8/250 fitting). Met: 4.1 whole, 4.2 engine half, 4.4
   engine half. Not met: none of the three; nothing was measured short of a
   tolerance, and the widest miss was 0.48% of a 5% allowance. 4.3, 4.5, 4.6, 4.7
   and 4.8 were not attempted; they wait on the gate.
C. The report last. Section 4 raises 9 items on top of the carried queue; none
   stands in the way of a criterion in B. Item 1 (a macro within a few characters
   of the cap count does not fit at 8/250) and item 2 (which patience the Olivia
   turn indicator should keep) shape the gate unit's arithmetic, not this unit's
   result.

```
UNIT:       365 - complete at task 5 of 5 - 2026-09-19 18:03
PHASE GOAL: Olivia becomes a mode Hamlet works like PSK31 - hears it, reads it,
            answers it, logs it - with the variant taken from the signal's own
            RSID and never from the operator.
UNIT GOAL:  Give Hamlet a voice in Olivia: a modulator for 8/250, 16/500 and
            32/1000 whose signal Hamlet's own receiver reads back word for word
            and which measures like the mode author's own transmitter, announced
            by its variant's RSID, with its length counted in the variant's
            characters. Nothing is wired to the send gate and nothing keys.
ADVANCED:   step 4 - 4.1 (whole), 4.2 (engine half), 4.4 (engine half)
NUMBER:     step 4 criteria met 0 -> 1 of 8 whole (4.1), plus the engine halves of
            4.2 and 4.4; variants modulated and read back identical 0 -> 3 of 3;
            working modes with a modulate guard 3 -> 4
DRIFT:      none - 0 consecutive units without advance (was 0; unit 364 closed
            step 3's criteria)
```

## 1. What Claude did

**Complete, at task 5 of 5.** Development computer, prompt gated `PROJECT: Hamlet`, confirmed
against the tree (`SHACK_FACTS.md` and `src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs`
present, no `CoreHMI.sln`, no `MURC.sln`, root `C:\Source\HamLet`); branch `main`, six commits,
all pushed. **Nothing in this report is evidence about the radio** (`SHACK_FACTS.md`,
HM-DEC-093, FACT-004): everything here was computed, and nothing keyed.

**Built: Hamlet's own `OliviaModulator`** (`src\Hamlet.RadioEngine\Olivia\OliviaModulator.cs`,
357 lines), the demodulator's inverse. Every format fact is `format.json`'s through `OliviaData`
- characters per block, the mask, the upper half negated, the Walsh butterfly, the scrambling
code and its shift of 13, the rotation of a character's bit across the tone's bits, which sign
sets a bit, the Gray table, and each variant's tones, spacing, symbol length and first tone.
`Modulate` makes the audio; `Compose` returns an `UnslottedTransmission` with the variant's RSID
burst in front, `AnnouncedCode` and `AnnouncementSamples` taken from the burst actually placed,
and `LongestSeconds` from the timing table.

**`pj_mfsk.h` was read for transmit structure only, and nothing was copied.** What it supplied,
with the lines: each symbol is its tone under a raised cosine two symbol periods long, the next
starting one period later (`:121`, `:223-236`); a symbol's tone starts a quarter turn either way
from where the last one's phase had run to (`:174-187`); a short final block is filled with the
null character (`:1819-1826`); and **there is no start-up preamble** - the first block's first
symbol begins the signal (`:1813-1834`). Hamlet's version is arithmetic per sample in hertz and
seconds, not the author's tap buffer and cosine table. The author's transmitter also divides each
symbol period of its output by that period's own peak (`:1839-1844`); Hamlet does not, because a
raised cosine overlapped by half already sums to the peak asked for and the drive level is the
operator's. **No package was added and nothing was copied**, so neither of §6's stops was reached.
The fixture generator was not compiled and no WAV was written.

**Measured before building** (`Unit365Trace`, task 0). All nine fixtures hash as `manifest.json`
says. The author's three clean fixtures, by the code task 2 then used on Hamlet: 8/250 8 tones at
31.254 Hz, 16/500 16 at 31.250, 32/1000 32 at 31.250; symbol rate 31.250 Hz on all three; 99%
bandwidth 249.41, 494.24 and 986.20 Hz; **preamble 0.467 s on all three**, which is five RSID
symbols of silence after the burst's last tone; and the data is exactly whole blocks of 64 symbols
plus one symbol's tail, which is how the absence of a preamble was confirmed rather than assumed.
PSK31 is **0.246941 s a character** over the four framed macros with the idle and reference symbol
left out.

**Three decisions this session made for itself, all overrulable, all in section 4's terms.**

- **The silence after the burst is five RSID symbols**, the same count the file states before it
  (`silence_symbols_before`). `rsid-codes.json` has no field for silence after; `SOURCE.md` says
  the fixtures' bursts were made the way fldigi sends RSID, with five symbols either side, and
  the measurement above agrees to about a millisecond. It is outside the announcement, so the cap
  counts it.
- **No burst, no Olivia send.** Where the codes carry no burst for the variant, `Compose` composes
  nothing and says so, rather than returning audio with `AnnouncedCode` null. An Olivia signal
  names its variant only by its burst, and a receiver left to guess the variant reads nothing
  (§0.0). PSK31, whose mode a receiver reads without an announcement, still goes unannounced in
  that case, and that behavior is unchanged.
- **The quarter turns' directions are seeded** (`PhaseSeed = 365`), so the same text makes the same
  audio to the sample. The author takes them from `rand()`; any fair choice serves a receiver that
  does not read phase.

**Two departures from the order, both reported here rather than discovered later.**

- **`Compose` was built at task 2, not task 3.** Decision AS measures the preamble from the end of
  the RSID burst, and only `Compose` places that burst and the pause after it, so task 2's
  measurement could not exist before it. Task 3 kept its own test (`EverySendBeginsWithItsRsid`)
  and the move of the loopback to decision AT's form.
- **`tools\status.sh` was edited** to write `RULES_AT: HM-DEC-165 (2026-09-19)`, because §5 said to
  update `RULES_AT` with the status write and the value is generated by that script. The reload
  tool was not touched.

**`UnslottedMode.Olivia` was added**, under decision AP's condition: **no line of
`Ft8TransmitSequence` had to change**. The sequence reads `Mode` only into its record
(`:667`) and into two sentences (`:271`, `:800`), and its one `switch` is on `Fit` (`:788`), not on
the mode. The send guards were run straight after adding it: `TheUnslottedSendTests`,
`TheFt8AndFt4SendsAreByteIdenticalTests`, `ThePsk31ModulatorTests` and `TheRsidBurstTests`, 41 of
41 green.

**What was not touched, checked by diff against `cf62bd9d`:** no line in `Hamlet.App`, in
`Ft8TransmitSequence`, in `RsidBurst`, `RsidDetector`, `OliviaDemodulator` or `OliviaListener`.
`CanTransmitIn` still refuses Olivia. `OperatorSend.LongestUnslottedSeconds` is still 30 and `Fit`
is unchanged. **`PttOn` write lines 1 and `Arm(` call lines 2, before and after.** The whole unit
is nine files: the modulator, `OliviaTiming`, `UnslottedTransmission`'s enum and two doc lines,
`timing.json`, the carry-forward list, and four test files.

**No new telemetry event** (decision AW): an engine object nothing sends has no stage to record.

**Nothing was recorded in `DECISIONS.md`.** No ruling of this unit's met §12.1's four tests; the
three choices above are reported for overrule instead.

**Verifying the instruction against the tree (§5), every mismatch reported and none repaired.**

1. **Step 3 read `partial` in `PHASE_STATUS.md`** while `PHASE_PLAN.md` §8's revision of
   2026-09-19 evening checks 3.0 to 3.6 and says step 4 is next. Decision AO applied at task 0:
   step 3 is `done` and `CURRENT_STEP: 4`, citing the plan; `PHASE_OUTCOME.md`'s earlier
   `STATE_AFTER` lines were left as history.
2. **`PROJECT_STATUS.md`'s `RULES_AT` said HM-DEC-161.** Now HM-DEC-165 (2026-09-19), from
   `tools\status.sh`.
3. **`PHASE_PLAN.md` leaves 1.5 and 1.7 unchecked** though step 1 is done on units 359 and 360.
   Reported, not edited.
4. **`PHASE_OUTCOME.md` already carried a `## UNIT 1 - STEP 4` entry** written against a different
   approach - one whose decision AO opens `CanTransmitIn` and adds an Olivia send through the
   sequence - with `FATE: executed`, `STATE_AFTER: in progress` and no report. It is not this
   unit's instruction and it is not what was built. Left as history; flagged in section 4 because
   the next unit's author reads that file.
5. `UnslottedTransmission` is as described: per-send `LongestSeconds`, `AnnouncedCode`,
   `AnnouncementSamples`, and a `Fit` that excuses only the burst `RsidBurst` makes for that code
   at that rate. `Psk31Modulator.Compose` was the shape followed.
6. `format.json` and `timing.json` read through `OliviaData` and `OliviaTiming`;
   `retire_after_characters` is 56 and was not touched.
7. `olivia-fixture-generator.cpp` and `SOURCE.md` read: the fixtures are the mode author's own
   encoder driven as fldigi drives it, with fldigi's RSID burst prepended. Not compiled.
8. **The PSK31 turn indicator's patience, which unit 364 did not name:** the only patience the tree
   states is `Psk31Macros.AnswerSeconds` (`src\Hamlet.RadioEngine\Psk31\Psk31Macros.cs:61`), R18's
   stated PSK31 equivalent of one FT8 slot - the time the `Answer` macro takes on the air, **7.84 s**
   for W1AW and KC3QIS. It is not a constant: it depends on the two callsigns. Nothing outside
   `TheCardWaitsOnHimTests` calls it, so the PSK31 card's waiting is not driven by it today. That
   is section 4's item 2.

## 2. What the owner should expect

Hamlet can now make Olivia. It builds the signal itself, at 8/250, 16/500 and 32/1000, with the
RSID burst in front that tells the other end's software which variant is coming, and its own
receiver reads that signal back letter for letter - every macro and a typed line, at two different
places in the passband, thirty of thirty exact. Measured against the mode author's own recordings
with one measuring tool used on both, Hamlet's signal sits on the same tones at the same spacing,
runs at the same symbol rate, takes the same width of the band and leaves the same gap after its
RSID: the widest difference anywhere was a twentieth of the allowance. **Nothing goes on the air
yet, and nothing keys**: the Olivia tab's send controls still refuse, `CanTransmitIn` has not been
opened, and no application file was changed at all. **What will look wrong and is not:** the app
test run shows one red out of 188, but it is one of two headless flakes this tree already carries
and it was red before this unit started - `TheStopIsAlwaysOnScreenTests` red four times of five
before any change, green in the run after; `TheTestsStayOffTheNetworkTests` red once in the run
after and green alone. Every name that was green before is green after. The next unit connects
this modulator to the CQ button.

## 3. What you should see

**Hamlet's Olivia against the mode author's, measured by the same code** (task 2,
`TheOliviaModulatorTests.TheSignalMatchesTheAuthorsAudio`, each variant composing that fixture's
own text at its own center, 8000 Hz):

```
variant  | quantity          | author   | Hamlet   | difference        | tolerance
8/250    | tone spacing Hz   |  31.2535 |  31.2440 | -0.0096 (-0.03%)  | 0.5%  (0.1563)
8/250    | symbol rate Hz    |  31.2502 |  31.2500 | -0.0002 ( 0.00%)  | 0.5%  (0.1563)
8/250    | 99% bandwidth Hz  | 249.4128 | 250.6194 | +1.2066 (+0.48%)  | 5%   (12.4706)
8/250    | preamble s        |   0.4671 |   0.4660 | -0.0012 (-0.25%)  | one symbol (0.0320)
16/500   | tone spacing Hz   |  31.2496 |  31.2497 | +0.0001 ( 0.00%)  | 0.5%  (0.1562)
16/500   | symbol rate Hz    |  31.2500 |  31.2500 |  0.0000 ( 0.00%)  | 0.5%  (0.1562)
16/500   | 99% bandwidth Hz  | 494.2376 | 494.1957 | -0.0419 (-0.01%)  | 5%   (24.7119)
16/500   | preamble s        |   0.4674 |   0.4667 | -0.0008 (-0.16%)  | one symbol (0.0320)
32/1000  | tone spacing Hz   |  31.2500 |  31.2500 |  0.0000 ( 0.00%)  | 0.5%  (0.1562)
32/1000  | symbol rate Hz    |  31.2500 |  31.2500 | -0.0001 ( 0.00%)  | 0.5%  (0.1563)
32/1000  | 99% bandwidth Hz  | 986.1961 | 986.7782 | +0.5821 (+0.06%)  | 5%   (49.3098)
32/1000  | preamble s        |   0.4676 |   0.4665 | -0.0011 (-0.23%)  | one symbol (0.0320)
```

Tones found: 8, 16 and 32 on both sides. Data length: author 26.652 / 129.052 / 104.476 s,
Hamlet 26.652 / 129.051 / 104.476 s. **No tolerance was loosened and none was missed**, so 4.1's
second half is met rather than shipped partial.

**The loopback** (`TheOliviaModulatorTests.EachMacroAndATypedLineComeBackIdentical`): the four
PSK31 macros as the app frames them (CQ 38, Answer 23, Report 96, Confirm 62 characters, for
KC3QIS and W1AW, Tim, Trafford PA, FN00DJ) and one framed typed line of 110 characters, at 1000 Hz
and at 1500 Hz, **the variant and center taken from what `RsidDetector` read off the composed
audio and never from the test**:

```
8/250   : 10 of 10 identical (exact string equality), decode cpu 0.48 - 2.50 s
16/500  : 10 of 10 identical, decode cpu 0.39 - 2.00 s
32/1000 : 10 of 10 identical, decode cpu 0.39 - 1.63 s
```

Not one block was rejected in thirty decodes, and no decode produced a character the text did not
have.

**The RSID read-back** (`TheOliviaModulatorTests.EverySendBeginsWithItsRsid`, 4.2's engine half),
six composed sends:

```
8/250   at 1000 Hz -> OLIVIA_8_250   (69) at 1000.32 Hz, 15 of 15 tones, quality 0.903, Fits
8/250   at 1500 Hz -> OLIVIA_8_250   (69) at 1499.94 Hz, 15 of 15 tones, quality 0.849, Fits
16/500  at 1000 Hz -> OLIVIA_16_500  (70) at 1000.32 Hz, 15 of 15 tones, quality 0.902, Fits
16/500  at 1500 Hz -> OLIVIA_16_500  (70) at 1499.94 Hz, 15 of 15 tones, quality 0.838, Fits
32/1000 at 1000 Hz -> OLIVIA_32_1000 (71) at 1000.32 Hz, 15 of 15 tones, quality 0.903, Fits
32/1000 at 1500 Hz -> OLIVIA_32_1000 (71) at 1499.94 Hz, 15 of 15 tones, quality 0.848, Fits
```

Each record says `announced: true` with its code, and `AnnouncementSamples` is exactly the burst's
own length (1.858 s), so the cap measures what follows it.

**The cap and the patience** (`TheOliviaTimingTests`, 4.4's engine half). PSK31 is 0.246941 s a
character; the counts in `timing.json` are macro **121**, typed line **242**, patience **32**,
being floor(30 s), floor(60 s) and ceiling(7.84 s) at that rate:

```
variant  | s per character | macro cap | typed cap | patience
8/250    | 0.68267         |  82.603 s | 165.206 s | 21.845 s
16/500   | 0.51200         |  61.952 s | 123.904 s | 16.384 s
32/1000  | 0.40960         |  49.562 s |  99.123 s | 13.107 s
```

The framed Report at 8/250 is 96 characters, 66.032 s of text after a 1.858 s announcement,
against a cap of 82.603 s: **Fits**. A text one character over the macro count (122) is 84.464 s:
**LongerThanTheCap**. `OperatorSend.LongestUnslottedSeconds` did not move.

**The carry-forward list, before the first change and after the last** (HM-DEC-165):

```
                 before                         after
engine   143 of 143, 4 m 43 s          146 of 146, 4 m 43 s   (+3: the new modulate guard)
app      187 of 188, 2 m 18 s          187 of 188, 2 m 19 s
PttOn write lines      1                      1
Arm( call lines        2                      2
```

**No regression: every name green before is green after.** The single app red is not the same name
in the two runs and neither is this unit's:
`TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns` was red
in the before run and red on four reruns alone, green on the fifth, and green in the after run;
`TheTestsStayOffTheNetworkTests.The354LayoutReadsTheSameNumbersTwiceRunning` was red in the after
run and green alone. Both are named as flakes in unit 362's report (items 8 and 9), and no
application file was touched by this unit.

**Olivia's row on the guard table now reads:**

```
Olivia   read      TheOliviaDemodulatorTests (the clean fixtures)                    engine
         modulate  TheOliviaModulatorTests.EachMacroAndATypedLineComeBackIdentical   engine
         send      none yet - added the unit the gate opens
```

## 4. What's blocking us

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

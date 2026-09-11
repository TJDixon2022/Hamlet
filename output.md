# Work instruction 315 - hear everyone

**READ IN THIS ORDER.**

A. **The phase goal - Hamlet works PSK31 the way it works FT8.** Steps 0 and 1 done,
   2 done by this unit's measurements, 3-6 not started.

B. **Step 2 and its exit criteria** - all six must-pass met, and one of the two
   nice-to-pass. Two-signal: two rows, 1000 Hz at CER 0.0000 and 1500 Hz at 0.0000,
   nothing of one in the other. Four-signal: four rows, 0.0000 each over its span.
   Noise-only: zero rows. Retire rule stated, 2.5 s after the carrier stops, measured
   1.75 to 2.50 s after each row last grew. Found by measurement, with no offset list
   anywhere. Real time 0.004, 0.17 s for 38.59 s of audio. Nice-to-pass: 100 Hz apart,
   both at 0.0000 - met. Off-air audio - not met, none exists.

C. **The report last, and section 4 raises 8 items** on top of a carried queue of
   sixteen. None is in the way of a criterion in B. Item 2 is the weakest ground under
   the first criterion: the 1500 Hz station's text is recorded nowhere in the tree, and
   its reference was derived.

```
UNIT:       315 - complete at task 5 of 5, none dropped - 2026-09-11 11:59
PHASE GOAL: A third digital mode that works like FT8 - same cards, same one click,
            same log and achievements - on a PSK31 modem Hamlet writes itself.
UNIT GOAL:  Find every PSK31 carrier in the passband by measuring, give each its own
            demodulator, and give each its own row of text in the list FT8 uses.
ADVANCED:   yes - step 2, all six must-pass and the 100 Hz nice-to-pass met.
NUMBER:     PSK31 signals read at once 1 -> 4 on the four-signal fixture, every row
            at CER 0.0000; fixed offsets 1 -> 0; version 1.13.2 -> 1.13.3
DRIFT:      0 consecutive units without advance  (was 0)
```

**Every appearance claim in this report is computed, not seen.** Nothing in this
repository can look at a picture. **Nothing here is evidence about the radio either.**
This machine has none, every fixture is synthetic, and every number is an indication
rather than a finding (FACT-004, FACT-006).

## 1. What Claude did

**Complete: task 5 of 5, nothing dropped.** Development machine, Windows 11. The
prompt claimed project Hamlet, and all four gate checks confirmed it: `SHACK_FACTS.md`
and `CwProbabilisticDecoder.cs` present, neither `.sln` present, root
`C:\Source\HamLet`. Branch **`main`**.

**Commits, each pushed before the next task started:** `28a21be` task 1, `974b395`
task 2, `9f3db75` task 3, `8007dc8` task 4, `2428e22` task 5. The report and final
status follow in one more commit.

**Left uncommitted:**
- `.run-unit/*` and `SESSION.lock`, which belong to the launcher.
- One placeholder at `docs/phase-ft4-run/PHASE_OUTCOME.md`, which the shell would not let
  me delete (task 1e below).

**The validator could not be run.** `tools\arbiter\validate-output.bat output.md` was
refused with "This command requires approval", and this session cannot give one. **So
there is no exit code, and by the prompt's own rule this report is not proved valid.**
I checked its seven rules with grep instead, using the script's own patterns, and all
seven hold:
- the `UNIT:` line is at line 22, above section 1
- the four headings are exact and in order, with no fifth, and section 4 is present
- section 3 has 26 non-blank lines
- the ordering block has its header, A, B, C and a count of section 4's items
- there is no placeholder token before section 1

Section 4 item 1 asks for the real run.

### The fixtures, measured

**Hash check first.** Every fixture's SHA-256 is checked against its manifest before a
sample is read.

| fixture | carriers the search found | each row's CER over its span | reference CER | real time |
| --- | --- | --- | --- | --- |
| clean, 1000 Hz | 1000.0 Hz, one | **0.0000**, 255 of 255 | 0.0000 | not measured |
| noise only, 30 s | **none** | **no rows** | 112 garbage characters, unsquelched | not measured |
| two signals, 1000 and 1500 Hz | 1000.0, 1500.0 to 1501.5 | 1000: **0.0000**; 1500: **0.0000** against the derived reference | 1000: 0.0000; 1500: not stated | not measured |
| four signals, 700 / 1100 / 1600 drifting / 2200 | 699.0-700.1, 1099.7-1100.5, **1600.2 to 1608.2 under one id**, 2200.0-2200.1 | **0.0000, 0.0000, 0.0000, 0.0000** | 0.0000 each over its span; 0.125, 0.386, 0.183, 0.000 unsquelched | **0.004** at 8 kHz; 0.145 at 48 kHz |
| two signals 100 Hz apart | 1000.0, 1099.8-1100.0 | **0.0000, 0.0000** | 0.0000, 0.0083 | not measured |
| +10, +3, -3 dB, 1000 Hz | 1000 Hz each | not run through the panel this unit | 0.0000 | not measured |

**Strength.** Four-signal: 1600 +5.7 > 700 +3.5 > 1100 +0.4 > 2200 -2.5 dB, the order
the fixture was made in. On the three fixtures with a stated SNR, the strength read
+10.1, +3.2 and -2.9 dB against +10, +3 and -3. That is a measurement, not a gate.

**The search rule, in one sentence** (`Psk31CarrierSearch.SearchRule`):
1. A Hann spectrum nominates each place whose power, summed over plus and minus 32 Hz,
   stands at least 2 times the floor's. The floor is the median bin or 70 dB under the
   strongest place, whichever is higher.
2. A probe at each place's power centroid averages the square of the unit differential
   product over 32 symbols.
3. It is listed as a carrier when that average is at least **0.6**, at least one symbol
   in ten reverses, and 0.6 of the power within three half-widths sits inside one.
4. It stays listed while the average is at least 0.4.

**The retire rule, in one sentence** (`Psk31Listener.RetireRule`):
- **The rule.** A channel goes when its carrier goes, which is 1 s after the keying
  measure last passed.
- **The number.** That puts it within **2.5 s** of the carrier stopping: the measure
  lets go in about 0.93 s, the hold is 1 s, and one spectrum window plus one quarter-second
  tick make up the rest.
- **After the last character.** On these fixtures, which idle 1.28 s after the last
  character, that is within 3.8 s of it.

### Task 1 - the phase opens, and the trace

**1a. Entry, run rather than read.** `ThePsk31DemodulatorTests`:
- `TheCleanFixtureDecodes`: CER **0.0000**.
- `TheSquelchProducesNothingFromNothing`: **0 characters**.

**1b. The phase files.** `PHASE_STATUS.md` line 1 reads `PHASE: Hamlet works PSK31 the way it
works FT8`. Its seven `STEP:` lines are the PSK31 steps, 0 and 1 `done` and 2 to 6
`not started`. **One line was edited, and only because the prompt assigns it to the
session:** `WORK_INSTRUCTION:` went from `314 - hear one...` to `315 - hear everyone`.
The instruction's section 10 says not to edit that file at all. The prompt is the more
specific of the two on that one line, so I followed it and left everything else alone.

**1c. `PROJECT_CARD.md`.**
- **Before:** `PHASE: FT4 works exactly the way FT8 does` and `PHASE_SET: 2026-09-08`.
- **After:** `PHASE: Hamlet works PSK31 the way it works FT8` and `PHASE_SET: 2026-09-11`.

**1d. The ruling, recorded under 12.1 as one Tim gave**, with its row in the
`CLAUDE.md` section 1 index. Reproduced in full:

> id: HM-DEC-161 · date: 2026-09-11 · refs: PHASE_PLAN.md, PHASE_STATUS.md,
> PROJECT_CARD.md, HM-DEC-160, docs/phase-ft4-run/, work instruction 315 task 1
>
> **The phase is "Hamlet works PSK31 the way it works FT8", set 2026-09-11, seven
> steps numbered 0 to 6.** Tim, 2026-09-11: *"let's finish implementing PSK31"*.
> **It supersedes HM-DEC-160's phase, "FT4 works exactly the way FT8 does"**, which
> closed at his word the same day: *"FT8 and FT4 seem pretty solid"*.
>
> `PROJECT_CARD.md` changes only by ruling (13.3), and this is the ruling that changes
> it. `PHASE` and `PHASE_SET` move; nothing else on the card does.
>
> **Recorded by work instruction 315, the seed unit of the phase, under 12.1 as a
> ruling the owner gave, not one a session made.** The reasoning is his and is not
> restated here beyond his words. `PHASE_PLAN.md` carries the plan written from his
> instruction *"I want it to work. Figure it out."*, and its §R rulings are the
> author's, each overrulable with one word, and none of them is this entry.
>
> **What was rejected is not recorded**, because he gave no alternative to rule
> against. The FT4 phase's plan, status and outcome are kept in `docs/phase-ft4-run/`,
> recovered from git history because the phase was installed without
> `install-phase.bat` moving them aside.

The id is the file's next one, as unit 288 did for HM-DEC-160; section 4 item 6 asks
about that.

**1e. The FT4 run, two files recovered of three.**
- **Source.** The last tree holding all three FT4 versions is `eb28430`, the parent of
  `c8da770`, which overwrote them.
- **Last FT4 commits.** `PHASE_OUTCOME.md` 1d5f72b, `PHASE_PLAN.md` 048a158,
  `PHASE_STATUS.md` eb28430.
- **Byte counts, from the blobs.** `PHASE_OUTCOME.md` **168242**, `PHASE_PLAN.md`
  **11383**, `PHASE_STATUS.md` **1174**.
- **Recovered.** `PHASE_PLAN.md` and `PHASE_STATUS.md`, written with the file tool. Each
  md5 equals its blob's (`908e41f4...`, `c41c6109...`).
- **Not recovered: `PHASE_OUTCOME.md`.** The shell sandbox blocked the redirect,
  `mkdir` and `rm`. `git restore --worktree`, `git archive --output`, `tee` and `git rm`
  each needed an approval this session cannot give. The file carries long runs of
  repeatedly re-encoded text that cannot be copied by hand byte for byte. A first
  transcription matched its blob for 200 of 493 lines before I stopped.
- **The placeholder.** I could not delete my partial copy, so I overwrote it with a note
  saying it is not the outcome file, and left it uncommitted.
- **What the outcome file carries.** `docs/phase-ft4-run/README.md` gives the one
  command that finishes the job. The instruction says `UNIT 300` through `UNIT 310`; the
  file carries **`## UNIT 288` to `## UNIT 305`, then 309 and 310**, with no entries for
  306, 307, 308 or 311.

**1f. The trace, with lines as they stood at the start of this unit.**
- **The audio source.** `AudioTap` (`src/Hamlet.RadioEngine/Audio/AudioTap.cs:68`) is fed
  in `CwDecoder.cs:888` at whatever rate the capture device reports
  (`WasapiAudioSource.cs:192`). **Nothing here fixes it at 8 kHz.**
- **How the FT8 list takes a row.** `MainWindowViewModel.PlaceRow` (`:10706`) inserts
  into `DigitalDecodes` at `:10769`. The row type is `DigitalDecodeRow`
  (`DigitalDecodeRow.cs:132`): offset in the `Hz` cell, strength in the `Snr` cell.
- **How the CQ filter selects.** `WantsRow` (`:1667`) asks
  `DecodedFilterRule.Wants(ShowsCqOnly, row.Addressee)` (`DecodedFilter.cs:48`).
- **How unit 314's row was replaced in place.** `ShowPsk31Text` (`:2034-2056`) assigned
  `DigitalDecodes[_psk31Row] = row`.
- **What `Psk31Listening.OffsetHz` fed.** The demodulator's constructor (`:1983`), the
  row's `Hz` cell (`:2040`), the strip sentence (`:3771-3774`) and one test
  (`ThePsk31PanelHearsTests.cs:128`).
- **Carry-forward, before anything changed.** Engine **63 of 63**, app **108 of 108**.

### Task 2 - the carrier search

**`Psk31CarrierSearch`**, in the engine, knowing nothing of tabs (§0.1).
- **How it measures.** A spectrum finds where the energy is. A probe at each such place
  measures keying shape: the differential product as a unit phasor, squared. Both BPSK
  states square to the same point, so the average's magnitude is 1.0 on keying and
  about 0.1 on noise, at any tuning error up to half the baud. Its angle *is* the error,
  which is how the offset comes out to a fraction of a hertz.
- **Two further guards.** At least one reversal in ten, because a steady whistle
  squares cleanly too. And a width rule, added in task 4.
- **Retuning.** A probe that moves rotates its averages rather than restarting them.

**Watched red 4 of 6 against a stub, then green, now 8 of 8** with task 5's case and a
strength measurement added. Two of the original six passed against the empty stub,
**`TheNoiseOnlyFixtureYieldsNoCarriers` included**, because a search that hears nothing
trivially lists nothing on noise.

**One fault found by running.** The clean fixture is noiseless, so its median floor is
16-bit quantisation. With that floor alone the search listed its carrier **and nineteen
keyed products of it**, 95 to 110 dB under it. **The fix was a nomination floor 70 dB
under the strongest place**, not a change to the test. On real HF audio the median floor
is always higher.

### Task 3 - a demodulator per carrier, into the list

**`Psk31Listener`**, in the engine:
- **Channels.** One `Psk31Demodulator` per listed carrier, made when the carrier appears
  and fed the last 3 s of audio so the station's first words are not lost. Dropped when
  the carrier goes.
- **Found by running: unit 314's squelch stays open on noise for seconds after a strong
  carrier stops.** It averages by energy, so the carrier's last seconds outweigh the
  noise that follows. That is exactly the garbage the author's unsquelched reference
  produced.
- **How I handled it, without touching step 1's rule.** A character is shown only once
  the search has measured the carrier still keyed a whole measure (1.024 s) after it.
  What never gets that far is dropped. **The cost is about a second of delay on every
  character.** How many characters were dropped on the fixtures was not counted.
- **Rows.** In `MainWindowViewModel`, one `DigitalDecodeRow` per channel: replaced in
  place, removed when its carrier retires, and removed on a mode change.

**Found by reading: `Ft8MessageSplit.Split` treats any three-word text as an FT8
message.** A PSK31 row reading `CQ CQ CQ` would have been given an addressee, filtered as
a CQ, and one reading `KC3QIS de W1AW` put on the operator's side.
`DigitalDecodeRow.IsTextOnly` stops it: a PSK31 row has no fields, and `IsForHim`
skips it.

**What became of `Psk31Listening.OffsetHz`: gone.** Nothing reads a fixed offset. The
shell refused both `rm` and `git rm`, so `Psk31Listening.cs` is committed as a
comment-only file saying so. The strip sentence no longer says "one spot", and the two
tests that pinned it were **rewritten, not deleted**, to pin the new one.

**`ThePsk31HearsEveryoneTests` watched red 4 of 5, then green**, now 7 of 7.
- **What stayed green against unit 314's single channel.** The noise-only case, for the
  same trivial reason as in task 2.
- **Re-run afterwards, all green:** `ThePsk31TabIsInertTests`, `ThePsk31SeamTests`,
  `ThePsk31PanelHearsTests` (the clean fixture still reads 0.0000),
  `VoiceTests`, and `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint`.

**The 1500 Hz reference is derived, and here is how.**
- **Not recorded.** `manifest.json` says only "EI4GNB calling CQ".
  `assets/reference-modem.py` holds the modem, not the script that made the file.
- **Why `C` would not do.** Against `manifest-step2.json`'s `C`, which has the line
  three times, the row read 1.0169, because it holds six.
- **The derived reference.** The recorded line is 320 varicode bits, 10.24 s. It is
  repeated as many whole times as fit in the 66.56 s file less the 80 idle bits, which is
  **6**. That count comes from the file and the line, not from the decode.

### Task 4 - real time, measured

The four-signal fixture, end to end through the real tap and tick in quarter-second lumps:
- **The panel, end to end: 0.17 s for 38.59 s of audio, ratio 0.004.** That is elapsed
  time on one thread, which bounds that thread's CPU from above.
- **Process CPU over the same span: 0.16 s.**
- **What dominates is the search.** The listener alone took 0.14 s: the search 0.11 s,
  the four demodulators 0.04 s.

**At 48 kHz**, on the fixture raised by a windowed-sinc interpolator, printed and not
asserted: **5.61 s, ratio 0.145.** That run found a fault.
- **The ghost.** The search listed a **fifth carrier at 3999.9 Hz for 27 s, and its
  row read 78 characters of nothing.** This is the old Nyquist edge, where folded noise
  has only two phases and squares as cleanly as BPSK.
- **The width rule.** I added **`NarrowEnough` = 0.6**: of the power over the floor
  within three half-widths, 0.6 must sit inside one.
- **After it.** 4 channels at 48 kHz, and every earlier result unchanged.

**The timing test and both task 5 cases were not watched failing.** The code they
exercise was committed before they were written, and a timing test that fails only
against a slower build cannot honestly be staged.

### Task 5 - two carriers 100 Hz apart

Found at 1000.0 and 1099.8-1100.0 Hz, both +7.2 dB. The rows read **0.0000** against `A`
and **0.0000** against `C`. `manifest-step2.json` does not say which text is on which
carrier; `A` at 1000 and `C` at 1100 is its listing order, and the four-signal file's.

**Carry-forward after everything, with both new classes added to the list:** engine
**71 of 71**, app **115 of 115**.

### Verified against the tree, and what did not match

**Found as named:**
- `Psk31Demodulator`, `SquelchQuality` 0.90, `Psk31Listening.OffsetHz` 1000 and
  `Varicode`.
- The step-2 fixtures, with hashes equal to `manifest-step2.json`.
- `corpus.json`, present and not read.
- `docs/carry-forward-tests.txt` and `docs/psk31-reference.md`.
- All five named tests.
- The card still holding the FT4 phase, as predicted.

**Did not match:**
- **`psk31-four-signals.wav` is 38.59 s, not 66.6 s** (308736 samples at 8 kHz).
  `psk31-two-signals-100hz-apart.wav` is 33.57 s.
- **`manifest.json` lists eight fixtures, not seven.** The eighth is unit 314's -10 dB.
- **The FT4 outcome file carries units 288-305, 309 and 310**, not 300 through 310 alone.
- **The phase was installed through `install-phase.bat` after all.** Commit `f2a87e1`
  moved the pre-launch PSK31 files into `docs/phase-psk31-prelaunch/`. The FT4 files had
  already been overwritten at the root 10 hours earlier by `c8da770`, unit 312's own
  task-1 commit.
- **The 1500 Hz station's text is not recorded**, as described under task 3.

**No other decision was recorded in `DECISIONS.md`.**

## 2. What the owner should expect

**Press PSK31 and Hamlet now listens to the whole passband.** Every signal it is sure is
PSK31 gets its own row in the decoded list:
- when it first went up
- its strength in decibels over the noise in 2500 Hz, the same terms as FT8's column
- where it sits in hertz
- its text, growing

On the fixtures, four stations at once each read with no errors, and thirty seconds of
noise produced nothing at all.

**What will look wrong but is not:**
- **Text runs about a second behind the audio.** That is the price of never showing a
  character from after a station stopped. See section 4 item 7.
- **A row disappears about two seconds after its station stops sending.** The
  instruction asked for exactly that: a carrier that stops is retired, not left as a
  ghost. A station sitting on idle is still sending and stays.
- **Rows are in the order they appeared, not by frequency.** A drifting station's hertz
  figure moves as it drifts.
- **`src/Hamlet.RadioEngine/Psk31/Psk31Listening.cs` is an empty file** with a note in
  it, and **`docs/phase-ft4-run/PHASE_OUTCOME.md` is a one-paragraph placeholder**,
  uncommitted. The shell would not delete either. Section 4 items 3 and 4.
- **Strength on a noiseless fixture reads +95 dB.** That is honest about a file with no
  noise in it, and says nothing about a radio.

**Build and tests:**
- **Builds succeeded** for the engine, the app and both test projects.
- **Every test this unit ran is green:** the whole carry-forward list, engine 71 of 71
  and app 115 of 115, which includes this unit's two new classes.
- **The known reds were not run**, per HM-DEC-155.
- **Pushed to `main`.**

## 3. What you should see

**Several rows, one per station, each with an offset and its text scrolling in.**

**The strip line with PSK31 pressed, word for word:**

```
listening for PSK31 across the whole passband. Every signal Hamlet is sure is PSK31
gets a line of its own below, with where it sits, how strong it is and its text as it
arrives. Nothing on those lines can be answered yet.
```

**What the decoded list reads from the four-signal fixture, about 30 seconds in.** The
time cell is when each row went up; strengths are the whole-decibel form of the medians
measured:

```
hhmmss  +6   1604   CQ CQ CQ de EI4GNB EI4GNB EI4GNB pse K ...
hhmmss  +4    700   CQ CQ CQ de KC3QIS KC3QIS KC3QIS pse K ...
hhmmss  +0   1100   KC3QIS de W1AW W1AW K / W1AW de KC3QIS RST 599 599 Name Tim Tim ...
hhmmss  -3   2200   VE3XN de F4DIA UR RST 579 579 Name Marc QTH Lyon HW? ...
```

Then the 1100 Hz row goes at 30.0 s, the 1600 Hz row at 35.0 s and the 700 Hz row at
36.0 s. The 2200 Hz row is still there when the file ends.

**What you must not expect:**
- **No clicking.** No row can be answered, and a right-click offers nothing.
- **No sending.** CQ under PSK31 still refuses and says why.
- **No callsigns picked out.** A row is text. Nothing is coloured as a sender or
  addressee, the CQ filter shows nothing, and nothing lands on your side of the panel
  even when the text says `KC3QIS`. That is the *read the conversation* step.

**What I cannot tell you:** how any of it looks on your screen, and how it behaves on a
real band. Both want a radio.

## 4. What's blocking us

Nothing blocks step 3. Eight items want a ruling or a hand. The gate this report is
judged by comes first, then the one closest to a criterion.

1. **Hand wanted: run `tools\arbiter\validate-output.bat output.md` and read its exit
   code.**
   - *Why.* The prompt makes exit 0 the gate for the whole unit, and this session's shell
     refused the script with "This command requires approval". Section 1 says how the
     seven rules were checked by hand, and they hold, but a hand check is not the
     script's exit code.
   - *Rejected.* Retrying the script in other forms, which units 289 to 292 already
     measured as refused, and calling the report valid without it.

2. **Ruling wanted: record the 1500 Hz station's text in `manifest.json`, or accept the
   derived reference.**
   - *Why.* Must-pass 1 says each row decodes "its own text". The two-signal file's 1500
     Hz text is written down nowhere in the tree. I derived it, and it reads 0.0000. But
     a reference a session derived is weaker than one the author recorded, and it
     assumes the generator filled the file with whole lines.
   - *Rejected.* Measuring against `C` as it stands (1.0169 for the wrong reason), and
     taking the line count from the decode, which would be circular.

3. **Hand wanted: finish the FT4 run archive.** From the repository root:
   `git show eb28430:PHASE_OUTCOME.md > docs\phase-ft4-run\PHASE_OUTCOME.md`, which
   replaces the placeholder. It should come out at 168242 bytes, md5
   `b60010069f7feea96e9d4be4e7e8c0f1`. Then commit.
   - *Why.* The shell refused every way git could write that file.
   - *Rejected.* A hand copy, which would not be byte-exact on that file's re-encoded
     lines.

4. **Hand wanted: `git rm src\Hamlet.RadioEngine\Psk31\Psk31Listening.cs`.**
   - *Why.* The class is gone and the shell refused to delete the file. It is empty and
     builds.
   - *Rejected.* Keeping the class as dead code, because a fixed offset left lying around
     is one somebody picks up again.

5. **Ruling wanted: the search's floor should be local to where the signals are, before
   step 6.**
   - *Why.* In the application the tap is fed at the sound card's rate, commonly
     48 kHz. There, the median bin is the empty spectrum above the radio's passband, far
     under the band's noise. So every noise bump is nominated and gets a probe, which is
     why 48 kHz measured 0.145 rather than roughly 0.03. It still keeps up, and the
     keying and width rules still reject the bumps. But it wastes CPU on the UI thread,
     and it leans on the width rule for exactly the kind of ghost task 4 found.
   - *Rejected, for now.* Redesigning the floor inside this unit, past its tasks. The
     real device rate has not been measured, because this machine has no radio.

6. **Ruling wanted: is HM-DEC-161 the right id?**
   - *Why.* The instruction says "do not invent a ruling id". `DECISIONS.md` gives every
     entry one, supersedes by id, and unit 288 took the next one for the FT4 phase's
     ruling. The ruling is Tim's words; only the number is the session's.
   - *Rejected.* An entry with no id, which the file and `DecisionLogOrderTests` would
     treat as malformed.

7. **Ruling wanted: accept a second of delay on PSK31 text, or reopen the step-1
   squelch.**
   - *Why.* `Psk31Demodulator`'s squelch averages by energy and stays open on noise for
     seconds after a strong carrier stops. The listener hides that by showing a
     character only once the search vouches for the carrier a second later. A squelch
     that weighted each symbol equally would close in about 0.3 s. But it is step 1's
     stated rule and your number, 0.90 on its current scale.
   - *Rejected.* Changing step 1's rule in this unit, and showing text first and taking
     it back later.

8. **Ruling wanted: the `snr` column's hover names FT8's and FT4's precision and not
   PSK31's.**
   - *Why.* PSK31 rows now carry strength in that column on the same terms. It measured
     within 0.2 dB of the stated SNR on three fixtures. What the hover says about it is
     display wording, and that is yours.
   - *Rejected.* Writing it here, without a ruling on the words.

### Asks still outstanding

Carried per HM-DEC-139, verbatim where unresolved.

1. **Does the transmission record ask the radio whether it keyed?** Unit 303's proposal,
   still Tim's. PSK31's continuous carrier makes it sharper.
2. **Nothing in this repository can look at a picture.** `Avalonia.Headless.Skia` is
   Tim's to add (§0.4).
3. **Three inherited reds, never chased** - two in `TheAchievementsScreenTests`, one in
   `TheFitGuardAsksAboutTheGridTheSendIsOnTests` (engine).
4. **`Ft8ContactCard.Closing`** is uncalled and left standing.
5. **`Ft8GlobePlot`'s unused framing constants.** Report; leave standing.
6. **The licence of `assets/world-flat-relief.png` is unknown.** Raise; do not resolve.
7. **The FT4 phase's run files were never archived under `docs\`.** The web thread
   overwrote them at the root on 2026-09-11 instead of running `install-phase.bat`. They
   are in git history. **Task 1 recovers them into `docs\phase-ft4-run\`.** *Unit 315:
   plan and status recovered and checked; the outcome file is waiting on section 4 item
   3.*
8. **The door sentence is a placeholder.** Carry.
9. **Acknowledgement indicators.** Named by Tim, not yet defined. Step 4's turn indicator
   may be what he meant; do not assume it is.
10. **Card ordering under scroll, and its root** - `DigitalCards.Clear()` then new cards
    every slot, per-card state lost. Not this phase's. Carry.
11. **The 2 px map outline** and **the 2.0x popup zoom cap** are sessions' numbers.
12. **Version numbering** - x.y.0 or x.y.1 for a phase's first unit under HM-DEC-150.
    *Unit 315 took a patch: 1.13.2 -> 1.13.3.*
13. **The squelch threshold 0.90** is unit 314's number, `Psk31Demodulator.SquelchQuality`.
14. **The listening offset 1000 Hz** is unit 314's starting place - **this unit replaces
    it with a search**, which closes the item if task 3 lands. *Unit 315: task 3 landed;
    the offset is gone and the search replaces it. Closed by the instruction's own
    condition, not by a ruling.*
15. **Real off-air PSK31 audio** - only Tim can record it. Two or three minutes on 14.070
    with a few signals on it, any recorder, any rate, WAV, into
    `assets\fixtures\captured\`. Every number in this phase is synthetic until then.
16. **One conversation card is taller than the panel** (293 px in 220 px). Not broken.

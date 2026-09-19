PHASE: Hamlet works Olivia the way it works PSK31
PHASE_SET: 2026-09-14
DESCRIPTION: A fourth digital mode, MFSK with error correction that reads below the noise, on the same cards, macros, typed line, log and achievements as PSK31; the variant chosen by the signal's own announcement, never by the operator.
STEP: 0 | The seam - Olivia exists as a mode. The cited calling table, a tab that tunes to the 8/250 spot, a panel that names itself, the Capture button, the RSID and calling data in the tree. Nothing decodes.
STEP: 1 | Hear the announcement - RSID detected across the passband, the mode, variant and offset read from it, proved on every RSID fixture; and every keyboard-mode send Hamlet makes begins with its own RSID, PSK31 included.
STEP: 2 | Hear one - an Olivia demodulator for a named variant at a named offset, MFSK with the mode's error correction, proved against the mode author's own audio at 8/250, 16/500 and 32/1000, clean and below the noise; a blind variant search for a carrier that never announced itself.
STEP: 3 | Hear everyone and read - a row per station from every RSID heard or blind-found, the PSK31 parser reading them unchanged, the two-signal fixture yielding two rows.
STEP: 4 | Say it - Hamlet's own modulator for the variants, every send announced by RSID, the PSK31 macros and the typed line with their timing scaled to the variant, the move-off-and-widen macro, through the one proved chain.
STEP: 5 | Log and achievements - MODE OLIVIA with its submode in the log and the export, the mode's records revealed by the first contact.
STEP: 6 | Tim at the radio - tune to the calling spot, see text, work a station, log it. Only he can close it.

---

# The Olivia phase - the reasoning under the step list

**Set 2026-09-14 by the web thread after an interview with Tim, before five days away.**
The screen phase is archived at `docs/phase-screen-run/` with its step 3 - Tim's verdict -
open; PSK31 is tabled with unit 357 delivered. **This phase is also a test of the
arbiter: the plan is written to be run unattended for its length.**

## §1 What Olivia is, and why it is next

MFSK - one of 8, 16 or 32 tones at a time across 250, 500 or 1000 Hz - with forward
error correction on top. It reads around -13 dB where PSK31 falls apart at -10, and it
shrugs off drift, multipath and fading. Slow: about PSK31's speed at 16/500, half that
at 8/250. A keyboard mode with the same conversation shape as PSK31 - CQ, answer,
report, name and QTH, 73 - so everything above the modem is already built.

**What makes it hard for a beginner, and what this phase does about each:** which
variant (the signal announces it - §R27); where (the cited table - §R29); the etiquette
of moving off the calling frequency and widening (a macro - §R29).

## §2 What is the same

Every ruling of the PSK31 plan (`docs/phase-psk31-run/PHASE_PLAN.md`, R1-R20 and the
card rulings R1-R6) and of the screen plan stands. The ones this phase leans on: the two
card types; one click one transmission and the one keying path (R10); the certainty gate
on Report and Confirm (R1); the typed line framed by Hamlet (Tim, 2026-09-14, unit 357);
nothing at the radio (R11); the ALC learned from FT8 (R15); telemetry on every stage
(R13); a session fixes its own tests (R12); nothing beyond the criterion (R14); American
(R19); the three stops (§6); the later ruling wins.

## §3 What is different

1. **The variant.** A signal is one of several variants and a decoder set to the wrong
   one reads nothing. The signal's RSID says which.
2. **Speed.** Every timing rule - the send cap, the turn indicator's patience, the retire
   window - scales with the variant's seconds per character, taken from the mode author's
   audio (the fixtures' manifest gives seconds and characters for each) and never fixed
   in seconds.
3. **The error correction.** Olivia's decoder either has a block or does not; there are no
   garbled letters, only missing ones. R9 - a character not sure of is not shown - is
   the mode's own behavior.

## §R Rulings, Tim, 2026-09-14, from the interview

**R27 - RSID, both ways, always, Hamlet-wide.** Every keyboard-mode transmission Hamlet
sends begins with the RSID burst naming its mode and variant - Olivia, and PSK31
retroactively. Hamlet listens for RSID across the passband and when one arrives sets the
mode, the variant and the offset itself. The operator never picks a variant. A carrier
that never announced itself gets the blind search of step 2 as a fallback. The tables
and codes are `data/rsid-codes.json`, ported from fldigi's `rsid.cxx` (GPL-3).

**R28 - Identical to PSK31 above the modem.** The same receipt, conversation card, Answer,
Report, Confirm on certainty, the typed line framed with the callsigns and the hand-back,
the same parser. The mode chip says Olivia, the row says the variant, the calling spot is
Olivia's. Timing rules scale with the variant (§3.2).

**R29 - The established locations, as FT8, FT4 and PSK31 have.** The calling table is
cited data, `data/olivia-calling.json`, a community convention and labeled as one.
Pressing Olivia tunes to the 8/250 spot for the band. After a certain answer on the
calling frequency the card offers one click - *move up 500 Hz and switch to 16/500* -
which sends the line saying so, retunes, changes variant, and the other end's RSID
confirms he followed.

**R30 - Synthetic fixtures first, the capture button from step 0.** Tim has no time to
generate fldigi audio. So the fixtures under `assets/fixtures/olivia/` were made by the
web thread from **the mode author's own transmitter** - Pawel Jalocha's `pj_mfsk.h` as
shipped in fldigi, compiled and driven exactly as fldigi drives it - with RSID bursts
from fldigi's own encoder. They are independent of anything Hamlet thinks Olivia is,
which is the lesson of the PSK31 phase. `assets/reference/SOURCE.md` says how. The
Capture button is on the Olivia panel from step 0 so the first evening's air becomes the
fixture for the next unit.

**R31 - This phase runs unattended.** Progress is counted in criteria by id. A done step
is closed. The owner's step ends the run. Two rulings per unit at most. A question about
layout, wording, a number or a mechanism is the arbiter's to answer, mark and continue.

**R32 - Tim, 2026-09-18, on unit 359's two questions.** (a) **The RSID burst does not
count against the send cap.** The cap measures the macro or the typed line; the
announcement is a fixed 2.3 seconds in front of it. (b) **The transmission record says a
send was announced** - `announced: true` and the RSID code on every keyboard-mode send.
Both were the arbiter's to decide under §6 and it stopped instead; **a cap the plan
calls the author's number, and a field on a record, are neither the radio's safety nor
a promise, and are never a stop.**

## §4 The steps

Each step verifies its own ground. Exit criteria carry ids `N.k`; met is `[x]`. A step's
exit is its own assertions plus `docs/carry-forward-tests.txt` run as its comment says -
not the whole suite (HM-DEC-155).

## Step 0 - The seam

**Delivers:** Olivia exists as a mode. The tab, the family color as text, the log mode,
the telemetry mode field, the cited calling table, the RSID data, the Capture button.
Pressing it tunes USB-D to the band's 8/250 spot and shows a panel that names itself and
says nothing can be read yet.

**Entry:** `PHASE_STATUS.md` names this phase; `assets/fixtures/olivia/manifest.json`
hashes match its nine files.

**Exit:**
- [x] 0.1 Pressing Olivia tunes to the calling center for the band from `data/olivia-calling.json`, not a constant, and the panel names the mode and says nothing decodes yet. *must-pass*
- [x] 0.2 The mode is in the Digital family, text color only; the log offers `OLIVIA`; the telemetry mode field says Olivia. *must-pass*
- [x] 0.3 The Capture button from unit 344 is on the Olivia panel and writes a 48 kHz WAV as it does on PSK31. *must-pass*
- [x] 0.4 `data/rsid-codes.json` and `data/olivia-calling.json` are in the tree with their citations and are read at startup; a malformed file is reported, not guessed. *must-pass*
- [x] 0.5 Under Olivia no other mode's decoder runs and no path reaches the send chain; `BindingHealthTests`, `VoiceTests` and the carry-forward list green. *must-pass*
- [x] 0.6 The neighborhood map picks out the Olivia spot when the tab is selected. *nice-to-pass*

**Depends on:** nothing.

## Step 1 - Hear the announcement

**Delivers:** an RSID detector across the passband - the 15-tone burst at 11025/1024 Hz
spacing, decoded through the ported tables to a mode code - yielding mode, variant and
center offset; and an RSID burst generator, so that every keyboard-mode send Hamlet makes
begins with its own announcement.

**Entry:** step 0 done; `psk31-cq-rsid.wav` and `olivia-8-250-cq-rsid.wav` hash as the
manifest says, checked first.

**Exit:**
- [x] 1.1 Each RSID fixture yields exactly one detection with the right code, variant and center within 5 Hz; the no-RSID and noise-only fixtures yield none. *must-pass*
- [x] 1.2 The two-signal fixture yields two detections, 8/250 at 1000 and 16/500 at 2000. *must-pass*
- [x] 1.3 The -16 dB fixture's RSID is detected. *must-pass*
- [x] 1.4 Hamlet's own burst generator, fed a code, produces tones that the detector reads back as that code at that center, and matches the shipped burst's tone sequence from `rsid-codes.json`. *must-pass*
- [ ] 1.5 A PSK31 CQ from Hamlet now begins with the BPSK31 RSID burst, proved by loopback through the detector; the transmission record says `announced: true` with the code (R32); FT8 and FT4 sends are byte-identical to before; the burst is outside the cap (R32). *must-pass*
- [x] 1.6 `rsid_heard` (code, variant, center, quality) and `rsid_sent` events, no callsign. *must-pass*
- [ ] 1.7 The detector keeps up with real time on the four-signal PSK31 fixture and the two-signal Olivia fixture, ratio reported. *nice-to-pass*

**Depends on:** step 0.

## Step 2 - Hear one

**Delivers:** `OliviaDemodulator` - MFSK with the mode's Walsh-function block coding and
scrambling, symbol and block synchronization, for a named variant at a named offset -
proved against the mode author's own audio; and a blind variant search for a carrier
that never announced itself. The reference is fldigi's `pj_mfsk.h`, read at the pinned
clone and never ported wholesale (PSK31 plan R5); the structure may be followed, the code
is Hamlet's.

**Entry:** step 1 done; the clean 16/500 fixture's RSID detected, checked first.

**Exit:**
- [ ] 2.1 The clean 8/250, 16/500 and 32/1000 fixtures decode to their text at CER at or under 0.01 each, the variant taken from RSID. *must-pass*
- [ ] 2.2 The -10 dB fixture decodes at or under 0.05 and the -16 dB fixture at or under 0.10. *must-pass*
- [ ] 2.3 The no-RSID 8/250 fixture is found by the blind search - the variant identified within a stated time from the tone spacing and symbol rate - and decodes at or under 0.05. *must-pass*
- [ ] 2.4 The noise-only fixture emits zero characters. *must-pass*
- [ ] 2.5 Every fixture's hash matches the manifest before use; each decodes in under twenty seconds of CPU, reported. *must-pass*
- [ ] 2.6 The seconds-per-character for each variant is measured from the fixtures and stored as the mode's timing table, from which every later timing rule derives. *must-pass*
- [ ] 2.7 A carrier that drifts 20 Hz over a minute (a fixture the unit makes from the shipped one) holds. *nice-to-pass*

**Depends on:** step 1.

## Step 3 - Hear everyone and read

**Delivers:** every RSID heard, and every blind-found carrier, gets its own demodulator
and its own row in the decoded list, text as blocks arrive, the variant on the row; the
PSK31 parser reads the rows unchanged; the CQ filter, worked-fade, entity resolution,
the quill and the row's whole-message hover all work on Olivia rows; rows stay after the
station ends, as PSK31 rows do.

**Entry:** step 2 done; the two-signal fixture's RSIDs detected, checked first.

**Exit:**
- [ ] 3.1 The two-signal fixture yields two rows, each with its variant and its own text at or under 0.05, nothing of one in the other. *must-pass*
- [ ] 3.2 The transcript corpus fed through Olivia rows yields the same verdicts as through PSK31 rows - no parser change. *must-pass*
- [ ] 3.3 The CQ filter, worked-fade, `EntityOf` with its `CQ` guard, the quill and the hover run on Olivia rows with no change to their code. *must-pass*
- [ ] 3.4 A row is retired when its signal goes and stays on the list marked ended; the retire window is the variant's timing table times a stated factor. *must-pass*
- [ ] 3.5 Telemetry: the PSK31 row events with `mode: olivia` and the variant; nothing personal. *must-pass*
- [ ] 3.6 Real-time ratio on the two-signal fixture under 1.0, reported. *nice-to-pass*

**Depends on:** step 2.

## Step 4 - Say it

**Delivers:** Hamlet's own `OliviaModulator` for 8/250, 16/500 and 32/1000; every send
begins with its RSID; the four macros and the typed line through the one unslotted
sequence with the cap and the turn indicator's patience scaled by the variant's timing
table; the calling spot from the cited table; the move-off-and-widen macro of R29.

**Entry:** step 3 done; the loopback chain of the PSK31 phase unchanged, checked by its
guarding tests first.

**Exit:**
- [ ] 4.1 Loopback: each macro and a typed line, modulated by Hamlet at each variant, decodes through Hamlet's demodulator identical; and the modulated signal's tone spacing, symbol rate, preamble length and occupied bandwidth match the mode author's fixture for that variant within stated tolerances. *must-pass*
- [ ] 4.2 Every send begins with its RSID; the detector reads it back as the variant sent. *must-pass*
- [ ] 4.3 The CQ goes out on the calling center from the cited table for the band on a clear spot; the receipt carries no station facts and no Log; a certain answer retires it. *must-pass*
- [ ] 4.4 The cap and the turn indicator's patience are the timing table times stated factors, not fixed seconds; a Report at 8/250 is allowed its length. *must-pass*
- [ ] 4.5 After a certain answer on the calling frequency the card offers *move up 500 Hz and switch to 16/500*; one click sends the line, retunes, changes variant, and the card says whether the other end's RSID followed. *must-pass*
- [ ] 4.6 Nothing keys at the bench; one `PttOn` site; FT8, FT4 and PSK31 sends byte-identical to before except PSK31's new RSID prefix; Stop aborts an Olivia send. *must-pass*
- [ ] 4.7 The power offer, the ALC reference and the ALC sentence work on Olivia sends as on PSK31. *must-pass*
- [ ] 4.8 The turn indicator changes within one block of the other station's turnover word. *nice-to-pass*

**Depends on:** step 3.

## Step 5 - Log and achievements

**Delivers:** an Olivia contact logs with RST and grid, exports as ADIF `MODE=OLIVIA`
with the variant as `SUBMODE` (`OLIVIA 16/500`), and the mode's records are revealed by
the first Olivia contact and absent before it.

**Entry:** step 4 done; a loopback exchange reaches 73, checked first.

**Exit:**
- [ ] 5.1 An Olivia contact logs with RST and grid; the ADIF carries `MODE=OLIVIA` and the variant submode. *must-pass*
- [ ] 5.2 Before the first Olivia contact no earned Olivia card is visible; after it the mode's records appear; the Modes badge counts it. *must-pass*
- [ ] 5.3 The two inherited reds in `TheAchievementsScreenTests` are not made worse. *must-pass*
- [ ] 5.4 The export imports cleanly into one named logger. *nice-to-pass*

**Depends on:** step 4.

## Step 6 - Tim at the radio

**Delivers:** Tim tunes to the calling spot, sees text, works a station, logs it.

**Entry:** step 5 done.

**Exit:**
- [ ] 6.1 Tim says it passed. No script can evaluate this. *must-pass*

**Depends on:** step 5.

## §5 Dependencies

Step 0 depends on nothing; steps 1-6 are one pipeline. When a step blocks there is
nowhere to route; work it or halt.

## §6 Branching

- **The arbiter stops for three things only**: keying, transmit or the radio's safety;
  money past the budget; a decision that changes what the product promises the operator
  - a fact stated about the radio, a contact or a send. A hint, a label, a number, a
  layout, a mechanism arithmetic will not allow: the arbiter decides, marks it author's
  and overrulable, and continues.
- **A later ruling of Tim's contradicts a line of this plan.** The later ruling wins.
- **A must-pass ceiling is missed by a little.** Ship, report the number, `partial`,
  move on. Never loosen a test.
- **A done step is closed.** Only Tim reopens it.
- **Every remaining step is Tim's.** Halt.
- **A fixture will not decode at all.** That is a finding about the demodulator, not the
  fixture - the fixtures are the mode author's. Report the spectrum measured against the
  manifest, mark `partial`, and name what the next unit tries.
- **Real off-air audio appears** under `assets/fixtures/captured/`. The next unit proves
  the demodulator against it before anything else.
- **Reading `pj_mfsk.h` tempts a port.** Read the structure; write Hamlet's own. If a
  unit cannot proceed without copying, `MOVE: stop` and say what it would copy.
- **A package is needed.** `MOVE: stop`.
- **Anything touches the transmit chain beyond adding an audio generator and an RSID
  prefix behind the one sequence.** `MOVE: stop`.
- **A file must be deleted.** Empty it, comment it, list it.
- **Status.** `tools/status.sh`, real clock, after every commit and every task.

## §7 Carried

Every open ask of the screen phase, from unit 357's queue, verbatim by every unit. Plus:
the PSK31 demodulator's garble on real air, waiting on a capture; the quill's cap and
the rank names; real flags on country cards; the id-scheme split; the map bitmap's license.

## §8 Revision record

- **2026-09-18.** R32 on Tim's rulings; 1.5 reworded to carry them; criteria met by units 358 and 359 checked as the record has them.

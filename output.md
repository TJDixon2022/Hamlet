READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 3 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 0 of 5, 8 0 of 6, 9 0 of 8.
B. Step 9, criterion 9.1: HM-REQ-122 - FldigiCwDecoder exists under Cw\Second\ at upstream
   61b97f41, with the GPL-3 notice, the authors and the commit in all 7 ported files. The
   synthetic case was not read: its key is `PARIS CQ` and the port emitted `GARIS CQ `.
   34 functions ported, 54 left out. 9.1 is NOT ticked. 9.2 to 9.8 are open, as they
   were; 9.8 is red on the three named floors and was not ticked (443 DECIDED (3)).
C. This report adds the port, the trace of fldigi's receive path, a test naming HM-REQ-122
   (red on the case, green on all seven headers), and a first look over 15 recordings.
   Section 4 raises 3 items:
   - Item 2 is in the way of ticking 9.1.
   - None is in the way of 9.2's scoring, which needs only the port, and the port exists.
   - Whether 9.2 may start while 9.1 is open is the next author's call under the plan's order.

UNIT:       456 - complete at task 5 of 5, none dropped - 2026-09-26 15:14
PHASE GOAL: Hamlet's CW decoder meets every requirement in CW_REQUIREMENTS.md, each shown by a test that names it; section M now adds a second decoder that reads the same audio and is scored beside ours.
UNIT GOAL:  Put fldigi's CW receiver into the tree as a faithful C# port, licence, authors and upstream commit intact, and show that it reads one synthetic send whose key is exact.
ADVANCED:   no - 9.1 not ticked: the port exists with its headers and its function list, but on the exact case it emits GARIS CQ for PARIS CQ; it was not tuned (HM-REQ-129)
NUMBER:     HM-REQ-122 not met; synthetic case key PARIS CQ emitted GARIS CQ (fldigi's spacing, trailing space included); functions ported 34, left out 54; our decoder's text byte-identical yes
DRIFT:      step 2 0; step 3 0; step 4 1; step 5 1; step 6 0; step 9 2 (was 1)

## 1. What Claude did

**Complete: tasks 0 to 5 were all done, and nothing was dropped.** Task 4, the drop candidate,
ran. 9.1 is not ticked, because the synthetic case was not read. Per the instruction, the port was
not tuned toward the key.

**Provenance.** Claude Code on the Windows development machine (QUIVERFULL). The prompt claimed
Hamlet, and the gate confirmed it: the four MUST EXIST files are present, `CoreHMI.sln` and
`MURC.sln` are absent, and the root is `C:\Source\HamLet`. Branch `main`. Every commit was pushed
to `origin/main`. Nothing here is evidence about the radio, and nothing keys or transmits.

**Commits:**

| task | commit | what |
|---|---|---|
| 0 | `15c90c0b` | entry: record edits, version 1.13.142 to 1.13.143, the runner's root writes, entry round, text saved |
| 1 | `f957c8ec` | the trace, `.run-unit/unit456-fldigi-rx.txt`; nothing in `src` |
| 2 | `a16e831e` | the port: 7 ported files and the rate adapter under `Cw/Second` |
| 3 | `d33656d5` | `TheSecondDecoderIsAFaithfulPortTests`; a read-only key-event log on the port |
| 4 | `95c62d60` | the first look, `.run-unit/unit456-first-look.txt` |
| 5 | this commit | exit round, report, status |

**Verified against the tree (§3):**
- **The clone.** `.run-unit/fldigi/.git/HEAD` points at `refs/heads/master` =
  `61b97f4133c488063f3de1795c894d22d5032e8a`, and the origin is
  `https://github.com/w1hkj/fldigi.git`. The six named paths are present, and the clone is untracked
  and unmodified.
- **Mismatch: the working tree is partial.** Only `src/cw_rtty`, `src/filters`, `src/include` and
  `src/misc` are checked out.
  - Missing, and read by the receive path: `src/misc/status.cxx` (the squelch defaults),
    `modem.cxx` (where `metric` starts) and `fl_digi.cxx` (`put_rx_char`, `display_metric`).
  - A read of the clone's own object store (`git --git-dir=.run-unit/fldigi/.git ls-tree`) was
    refused. That is recorded here as a denial and was not worked around. Nothing was fetched.
- **`Cw/Second`** was absent at entry.
- **Sample rates.**
  - fldigi's CW modem runs at 8000 Hz (`CW_SAMPLERATE`, cw.h:41).
  - `CwProbabilisticDecoder` takes whatever rate the audio carries. The captures are 68 WAVs at
    48000 Hz and 1 at 8000. The synthetic set is 12 at 8000 (read from each WAV header).
- **Defaults.** Every `progdefaults` field the receiver reads is in section 3 with its
  `configuration.h` line.
- **Expected failures:**
  - The three named floors are red at 38, 43 and 42, at entry and at exit.
  - The app line lost 1 test to Avalonia's "dispatcher loop" on the exit round's first run, and
    was 278 of 278 on its one rerun. At entry it was 278 of 278 on the first run.

**Task 1, the trace.** I read `cw.cxx`, `cw.h`, `morse.cxx`, `morse.h`, `fftfilt.cxx`/`.h`,
`filters.cxx`/`.h` (`Cmovavg`), `gfft.h`, `misc.h`, `complex.h`, `modem.h` and `configuration.h`.
- **The receiver's modes:**
  - **Default, ported:** the hysteresis detector with timing and the table.
  - **Matched filter, ported as its switch, off:** the same code under `CWmfilt`.
  - **SOM decoding, left out and named:** a separate classifier with its own table
    (`CWuseSOMdecoding`, off). The port refuses the setting rather than ignoring it.
  - **`rx_FIRprocess`:** declared at cw.h:239, with no definition at this commit.
  - **`view_cw`:** a display, left out.
- **R72:** there is no word, dictionary or callsign logic anywhere in the path.

**Task 2, the port.** `FldigiCwDecoder`, `FldigiFftFilter`, `FldigiFft`, `FldigiMovingAverage`,
`FldigiMorse`, `FldigiMisc` and `FldigiProgdefaults`, one per upstream unit.
- Each carries its upstream header verbatim, the upstream path, the fldigi URL and the commit.
- Each method names the function it ports.
- **`FldigiRateAdapter` is Hamlet's own**, outside the ported files, and is not fldigi's method. It
  is a 481-tap Blackman-windowed sinc low-pass at 3600 Hz that keeps every 6th sample, for 48000
  to 8000 Hz. fldigi's own conversion uses libsamplerate, outside the receive path. No package was
  added.
- **Not wired to anything.** No change to `CwProbabilisticDecoder`, the stream, `MorseAlphabet`,
  the tracker, the scorer, the sheet or the CW tab.

**Task 3, the case.** Its recipe is in section 3. The runs, in order, all recorded:
1. **Deliberately wrong expectation `PARIS CQ X`, lead-in 3 s: failed.** Emitted `NES CQ `.
   fldigi's AGC starts `noise_floor` at 1.0 and had not come down by then; `CWlower` sat above
   `CWupper` at the first letters. (`.run-unit/unit456-second-fail-first.txt`)
2. **Lead-in set to 10 s: emitted `GARIS CQ `.** That is five of fldigi's slowest time constants:
   decay weight 1000 at the 500 Hz decision rate, 2 s each. I derived the 10 s from fldigi's
   constants, not by trying values against the key. (`unit456-second-run2.txt`)
3. **Key events traced, where the first dot goes.** (`unit456-second-run3.txt`)
   - With the squelch off, fldigi's thresholds sit inside the noise: `CWupper` 0.832 against
     `CWlower` 0.829. It keys on noise, with 186 key events before the first mark.
   - The send's first dot joins a noise key-down that was already under way, which gives element
     1168, a dash.
   - A noise spike right after it sets `RS_IDLE` (cw.cxx:818-821). The next key-down from idle
     clears what was held (cw.cxx:793-798).
   - So `.--.` arrives as `--.`, which reads G.
4. **Expectation corrected to `PARIS CQ `, fldigi's own spacing: red, `GARIS CQ `.**
   - fldigi prints a word space after more than 4 dot lengths of silence, which gives the trailing
     space. No space goes before the first letter, because `space_sent` starts true.
   - The seven header cases are green.

I checked the harness first, as the instruction says: rate 8000, level (tone peak 0.5, noise RMS
0.112) and frequency 600 as keyed. I compared the port against upstream at the lines that drop the
dot. It follows them, so there was nothing to repair toward upstream.

**Task 4, the first look.** The printout is in section 3.

**Task 5, the exit round.** Every figure is in section 3. None moved.

**Decisions I made for myself:**
1. **Squelch off.** Its shipped default is in `status.cxx`, which is not in the clone.
   - fldigi's own file benchmark ships `sql = false, sqlevel = 0.0` (benchmark.cxx:54, fields at
     benchmark.h:28-32), so off is how fldigi itself runs on files.
   - Both values are constructor parameters. `metric` starts at 0.0, and only the squelch reads it.
2. **The 10 s lead-in**, derived from fldigi's time constants as above. The first run at 3 s is
   recorded, and both are in the test's remarks.
3. **The FFT is ported as an equivalent, not line for line.** `gfft.h` is 3392 lines of Green's
   radix-2/4/8. What `fftfilt` reads from it is the DFT: forward unscaled, inverse scaled 1/N
   (gfft.h:2253). A radix-2 Cooley-Tukey gives the same numbers to rounding, not bit for bit.
   Section 3 lists this as a departure.
4. **I added a read-only record to the port:** `Text`, `Emissions` and `KeyEvents`. Each is marked
   "not fldigi's". It records state and changes nothing the receiver reads. I added it to answer
   task 3's "print what it emitted with fldigi's speed and thresholds". It is observation, not a
   technique, so HM-REQ-129 stands.
5. **The new test is committed red on the case**, on 455's precedent for a requirement measured and
   not met. The instruction's exit-state line ("named types green at every commit, except the three
   floors") does not cover that. I chose red over a skip or an expectation that matches the garble.
6. **The first look's three captures:**
   - 17:37 (a red floor, key inferred);
   - `cw-2026-08-17-013347` (VA3VRR, adjudicated);
   - `unadjudicated/cw-2026-09-24-004108`, the first of the ten with a keyed stretch.

   The pitch given is the median of `CwPitchInstrument`'s windows, not our tracker's.
7. **I committed the root files the runner wrote** (PHASE_OUTCOME, PHASE_STATUS, RUN_LEDGER,
   WORK_INSTRUCTIONS) with task 0, as 453 to 455 did. The runner's `.run-unit` state files, `STOP`
   and `SESSION.lock` were left as they were, also as in 455. PHASE_STATUS's `CURRENT_STEP` came in
   at 2 from the runner and was set to 9 in both copies.

**Refused calls:**
- The object-store read above.
- A `cd` into the clone before a git command.
- Several compound shell lines. I split each one and ran it as separate commands.
- One heredoc write into `.run-unit`. I wrote that file with the editor instead.

None hid a result.

**Instruction check.** The prompt and the work instruction both carry the status cadence, and the
work instruction states the task count (5).

## 2. What the owner should expect

Nothing the operator sees changes yet. The CW tab, the transcript, the sheet and every recording's
text are exactly as they were, byte for byte. Hamlet now carries a second, known CW reader: fldigi's
receiver, ported with its licence and authors. The next units will score it beside Hamlet's own.
Two things will look wrong but are not:
- `TheSecondDecoderIsAFaithfulPortTests.ItReadsASendWhoseKeyIsExact` is red. That is the second
  reader, faithfully, losing the first dot of a send that follows noise.
- The three named floors are red at 38, 43 and 42, as before.

## 3. What you should see

**Upstream functions in the receive call graph, at `61b97f41`:**

| file:line | function | verdict | why, if left out |
|---|---|---|---|
| cw.cxx:719 | cw::rx_process | ported | |
| cw.cxx:717 | cwprocessing guard | ported | |
| cw.cxx:386 | cw::reset_rx_filter | ported | the two REQ() UI lines are dropped |
| cw.cxx:683 | cw::rx_FFTprocess | ported | |
| cw.cxx:593 | cw::decode_stream | ported | the scope pipe and syncscope are display; put_rx_char becomes the output |
| cw.cxx:771 | cw::handle_event | ported | |
| cw.cxx:754 | cw::usec_diff | ported | |
| cw.cxx:524 | cw::update_tracking | ported | |
| cw.cxx:466 | cw::sync_parameters | ported | the nanoIO block is keying; put_cwRcvWPM is UI |
| cw.cxx:443 | cw::sync_transmit_parameters | in part | the send lengths the receiver reads are ported; edges, QSK and risetime are transmit |
| cw.cxx:299 | cw::cw | in part | tx frequency, risetime, QSK, farnsworth, create_edges, nano, UI and the cwio thread are left out |
| cw.cxx:256 | cw::init | in part | the waterfall frequency is replaced by the caller's; the outbuf/qskbuf memset and nanoIO are transmit |
| cw.cxx:240 | cw::rx_init | in part | scope mode, status and viewcw are UI |
| misc.h:59, 53 | decayavg, clamp | ported | |
| fftfilt.cxx:105, 69, 124, 55, 202; fftfilt.h:66, 47, 51 | fftfilt(f,len), init_filter, create_filter, clear_filter, run, create_lpf, fsinc, _blackman | ported | |
| gfft.h:3316, 3332 | g_fft::ComplexFFT, InverseComplexFFT | as equivalent | departure 1 below |
| filters.cxx:261, 273, 294, 304 | Cmovavg ctor, run, setLength, reset | ported | |
| morse.h:45; morse.cxx:177, 167, 242, 44 | cMorse ctor, init, enable, rx_lookup, cw_table | ported | |

**Left out: 54 entries.** The full list with lines is in `.run-unit/unit456-fldigi-rx.txt`.
- **Transmit:** tx_init 226, create_edges 932, nco 969, qsknco 976, send_symbol 1004, send_ch 1071,
  tx_process 1151, cMorse tx_lookup/tx_length/tx_print.
- **Keying:** open/close_CW_KEYLINE 1301/1340, flrig_cwio_send 1370, cwio_key 1409, cwio_ptt 1427,
  cwio_now 1457, cwio_bit 1489, send_cwio 1557, the cwio_calibrate family 1618-1681, the cwio
  threads 1695/1719, send_CW 1747, and the six CAT_keying functions 1772-1932.
- **UI:** update_Status 538, update_syncscope 550, clear_syncscope 568, inc/dec/toggleWPM
  1257-1281, calWPM, view_cw.cxx, put_rx_char/display_metric.
- **SOM mode:** normalize 164, find_winner 189, som_table 86.
- **Not called by the receive path:** mixer 576, fftfilt band-pass ctor 95, create_hpf, flush_size
  83, rtty_filter 245, Cmovavg::value, g_fft's real-FFT and scale functions.
- **Destructors:** ~cw 292, ~fftfilt 112, ~Cmovavg 268.
- **Not defined at this commit:** rx_FIRprocess.

**`progdefaults` read, at their shipped defaults (configuration.h line):**
- **Speed and filter:** CWspeed 18 (513), CWbandwidth 150 (525), CWmfilt false (540), CWtrack true
  (537), CWrange 10 (546), CWlowerlimit 5 (549), CWupperlimit 50 (552).
- **Thresholds:** CWupper 0.6 (531) and CWlower 0.4 (528). Both are overwritten at every decision
  sample (cw.cxx:640-641).
- **Detector timing and noise:** CW_noise `'*'` (249), cwrx_attack 1 = 200 (555), cwrx_decay 1 =
  1000 (559), CWuseSOMdecoding false (543).
- **Prosigns:** CW_use_paren false (617), CW_prosigns `=~<>%+&{}` (620), CW_prosign_display false
  (623).
- **The table's switches:** the extended-character switches (629-662) and the punctuation switches,
  all true (219-248).
- **Replaced by the caller:** CWsweetspot 1500 (201), in place of the caller's frequency.
- **Not read by the receive path:** CWmfiltlen 100 (534) is not read at this commit. CWfarnsworth 18
  (516) is read only for transmit.
- **The squelch:** sqlonoff and sldrSquelchValue are not in the tree. They are off, per
  benchmark.cxx:54.

**Departures forced by C# (upstream line):**
1. **The FFT:** `g_fft` Green radix-2/4/8 (gfft.h:3316, 3332, 2253) → iterative radix-2 with the same
   scaling. It agrees to rounding.
2. **The complex type:** `cmplx` = `std::complex<double>` (complex.h:31) → `System.Numerics.Complex`,
   and `abs` → `Complex.Abs`.
3. **Settings:** the process-wide `progdefaults` → one `FldigiProgdefaults` per decoder. The
   receiver writes into it: CWupper and CWlower at cw.cxx:640-641, and CWbandwidth at 354 and 396.
4. **The frequency:** the static `modem::frequency` (modem.h:49), set from the waterfall
   (cw.cxx:262-270) → a constructor argument from the caller.
5. **Statics made instance fields:** `static bool cwprocessing` (cw.cxx:717), and the function
   statics `space_sent` and `last_element` (cw.cxx:773-774).
6. **The Morse table:** the static `cMorse::cw_table` (morse.cxx:44) → an instance array. UTF-8 byte
   strings → .NET strings.
7. **Output:** `put_rx_char` per byte (cw.cxx:671-674) → the whole string appended to `Text`, with
   one `Emissions` entry.
8. **Buffers:**
   - `std::string rx_rep_buf` → `StringBuilder`, and `memset(cw_buffer)` → `Array.Clear`.
   - `const double *buf, int len` → `ReadOnlySpan<double>`.
   - `cmplx **out` → `out Complex[]`, the same buffer.
9. **Rounding and integer width:**
   - `round()` (cw.cxx:326, 416, 451) → `Math.Round(..., AwayFromZero)`.
   - `long int` → `long`. Every value fits in 32 bits either way.
10. **Startup:** `cw::cw` then `cw::init` run together in the constructor. fldigi calls init when
    the modem starts. `modem::metric` starts at 0.0, because modem.cxx is not in the tree.
11. **SOM mode:** `CWuseSOMdecoding` true → `NotSupportedException`, because the mode is not ported.
12. **update_tracking:** its function statics `min_dot` and `max_dash` (cw.cxx:526-527) → `const`,
    with the same values, 48 and 5760.

None of these changes a decision.

**The synthetic case.**
- **Recipe:**
  - **Send:** `PARIS CQ` at 18 WPM (fldigi's CWspeed), one unit 1200/18 ms, marks 1 and 3, gaps
    1, 3 and 7.
  - **Key:** patterns typed from M.1677-1 in the test, and checked against the rendered marks before
    decoding (§12.5).
  - **Tone:** 600 Hz, peak 0.5, raised-cosine 5 ms edges, at 8000 Hz.
  - **Timing:** 10.0 s lead-in and 1.5 s tail.
  - **Noise:** white Gaussian from xorshift32 and Box-Muller, seed 20260926, shaped by a 257-tap
    Blackman-windowed sinc band-pass to 300-2800 Hz. It is scaled to tone power over noise power
    of +10 dB, which in that 2500 Hz band is the `CW_SPEC.md` 8.1 reference SNR. Noise RMS 0.11180.
- **The port** is given 600 Hz with the squelch off.
- **Key `PARIS CQ`. Emitted `GARIS CQ `.** The emissions, with fldigi's speed and thresholds:

| text | at s | held | speed | two_dots | CWupper | CWlower |
|---|---|---|---|---|---|---|
| G | 11.008 | `--.` | 19 | 965 | 0.3344 | 0.3083 |
| A | 11.520 | `.-` | 19 | 972 | 0.3487 | 0.3191 |
| R | 12.160 | `.-.` | 19 | 993 | 0.3696 | 0.3366 |
| I | 12.672 | `..` | 19 | 993 | 0.3647 | 0.3326 |
| S | 13.184 | `...` | 19 | 993 | 0.3644 | 0.3324 |
| space | 13.312 | | 19 | 993 | 0.3490 | 0.3188 |
| C | 14.336 | `-.-.` | 18 | 1014 | 0.3850 | 0.3497 |
| Q | 15.360 | `--.-` | 18 | 1022 | 0.4407 | 0.3987 |
| space | 15.488 | | 18 | 1022 | 0.4179 | 0.3784 |

The times are at the end of each 1024-sample filter block. Why P reads G is traced in section 1,
task 3.

**The first look.** Printed, not scored; scoring is 9.2's. The whole printout is in
`.run-unit/unit456-first-look.txt`. A sample, key / ours / fldigi:

- `cq-18wpm-15db`: `CQ CQ CQ DE N0CALL N0CALL K` / `CQ CQ CQ DE N0CALL N0CALL K` / `EQ CQ DE N0CALL N0CALL K`
- `cq-12wpm-5db`: same key / `CQ CQ CQ DE N0CALL N0CALL K` / `* CQ DE 0CADL N0CAL K`
- `cq-18wpm-15db-char5`: same key / `C QCQCQDEN0 C A E LN 0CAL L K` / `E Q C Q D E N 0 C A L L N 0 C A L L K`
- `cq-18wpm-0db`: same key / nothing / `SEBE NIE`
- 17:37: `CQ CQ CQ DE WB6RED WB6RED` (inferred) / `T EABNIREDWBZ WB6RED CQ CQ CQ DEWB6 RE D W B 7E E I` / `TBZ WBHE C CQDW SRED`

What it shows:
- At 15 dB fldigi reads the body and loses the start of every send.
- At a 5-unit character gap it spaces every letter, by its own rule: a word space after more than
  4 dots.
- At 0 dB it prints letters where ours prints nothing.

**Entry and exit:**

| check | entry | exit |
|---|---|---|
| build | 0 errors, 17 s | 0 errors |
| engine carry-forward | 178 of 178, 375 s | 178 of 178, 379 s |
| app carry-forward | 278 of 278, 166 s, first run | 277 of 278 first run (dispatcher loop), 278 of 278 on the one rerun |
| captures | 51 of 51 | 51 of 51 |
| adjudicated | 13 of 13 | 13 of 13 |
| named floors | 10 of 13 (38, 43, 42) | 10 of 13 (38, 43, 42) |
| MET-CER-SURE | real, inferred: 33 of 436; synthetic, exact: 14 of 173 | same |
| MET-INVENTED | real 33 over 473; synthetic 14 over 252 | same |
| coverage (R82) | real 403 over 473; synthetic 159 over 252 | same |
| MET-WBE | real 37 (29 ins, 8 del) over 113; synthetic 44 (13, 31) over 84 | same |
| `TheSecondDecoderIsAFaithfulPortTests` | - | 7 of 8: headers 7 of 7; the case red, `GARIS CQ ` |
| `git diff 7e209cb4` over the eleven transmit files | - | prints nothing |
| every recording's text against task 0 | - | byte-identical, 126 lines |
| `git status` | - | `.run-unit/fldigi/` untracked |

The exit prints are in `.run-unit/unit456-tx-exit.txt`.

## 4. What's blocking us

Three items, most-blocking first. None needs the owner under R85; each is a reading recorded so the
next author can overrule it.

1. **9.1's case is not read, and the reading of why stays with the next unit.**
   - **The reading:** V-04 says a fixture the reference decoder cannot read is a generator defect,
     with the real recording as the generator's control. The port is now a reference reader.
     - So the next step is on the case, not the port. Test whether a noise-led send is what fldigi
       meets on the air, against the real captures in the first look: fldigi also loses the start
       there.
     - Then choose a case construction from that evidence, never from trying values against the key.
   - **Why this unit did not re-choose the case:** re-choosing a seed, an SNR or a lead-in until
     the text comes out right is tuning toward the key. HM-REQ-122 and 129, and the instruction,
     forbid it.
   - **Rejected:** mapping the garble into the expectation, which would assert a wrong letter as
     right (§0.0). Also rejected: a skip, which would hide a measured result.
2. **The squelch default was read from benchmark.cxx, not status.cxx.**
   - **The reading:** fldigi's own file benchmark ships the squelch off (benchmark.cxx:54). The
     port does the same, and both values are parameters.
   - **Rejected:** guessing `status.cxx`'s initialiser, which the partial clone does not hold and
     whose object-store read was refused.
   - **If the owner's copy is ever completed,** the next unit checks `sqlonoff` there. No change
     sits waiting for it.
3. **Finding for 9.2, not in its way:** fldigi keys on band noise with the squelch off. It loses the
   start of a send, and it turns a 5-unit character gap into a word space. 9.2's scorer will count
   both against the second decoder, and both are upstream behaviour (HM-REQ-129). 9.2 states how
   the unclassed output, fldigi's `*` included, is mapped.

**Asks resolved:** unit 454's ask for fldigi's source is answered. The clone is at
`.run-unit/fldigi` at `61b97f41`.

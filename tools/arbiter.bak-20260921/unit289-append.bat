@echo off
rem ============================================================
rem  unit289-append.bat  -  unit 289's one call to
rem                         outcome-append.bat
rem
rem  WHY A SCRIPT AND NOT A COMMAND LINE. Fifteen units of
rem  measured shell behaviour: this shell will not carry a quoted
rem  heredoc containing an apostrophe, it collapses a doubled
rem  backslash inside one, and an apostrophe inside an argument to
rem  outcome-read.bat aborted its inner PowerShell mid-run while
rem  the script still exited 0. There are no apostrophes below.
rem
rem  It is committed so the arguments this unit passed can be read
rem  afterwards rather than reconstructed from the entry.
rem ============================================================

setlocal

call "%~dp0outcome-append.bat" ^
  289 ^
  1 ^
  "done" ^
  "Ported upstream FT4 into new files beside the FT8 ones, nailed the encoder to gen_ft8 -ft4 symbol for symbol and the waveform to its own WAV sample for sample BEFORE writing the decoder, then round tripped 106 messages through Hamlet own chain." ^
  "Upstream own candidate sweep of -10 to 19 blocks cannot reach an FT4 signal centred in its own slot. At a 0.048 s block that sweep ends at 0.912 s and the generator starts the signal at 1.23 s. Measured in task 1, and it is why upstream decoder reads zero messages out of upstream generator." ^
  "continue" ^
  "Step 1 had all four exit criteria untried and every later step depends on an FT4 decoder existing. Three must-pass criteria were reachable in one unit because 28 of 33 port files were already protocol-neutral and upstream carried working C for every one of the rest." ^
  "The FT4 candidate sweep is widened to blocks -10 to 51 where the demo application uses -10 to 19. That bound is not in the ft8 library at all - it is a file-scope judgement in demo decode_ft8.c about how much work to do, the same class as kMin_score and kMax_candidates, and it was already a constructor parameter in this port. 51 is 156 blocks in a slot less 105 in a transmission. Nothing about the modulation, the tables, the codeword or the waveform is changed. The 4.48 against 5.04 timing question is NOT settled here - it stays with Tim, and the constant now lives in exactly one file so a ruling costs one edit." ^
  "PHASE_PLAN.md named-alternatives table - the tree disagrees with this plan, the tree wins, report the mismatch and continue - together with the arbiter own moved second exit criterion for step 1." ^
  "one session" ^
  "Hamlet can make an FT4 transmission and read it back as the message it started as, 106 of 106 with zero wrong decodes, and its tones and its samples are proved identical to upstream own generator rather than only to itself." ^
  "" ^
  "executed" ^
  "Three must-pass criteria met and evidenced: 106 of 106 messages round tripping to themselves including compound callsigns, grids, reports and RR73; the timing measured from the audio with the 4.48 against 5.04 disagreement named with both numbers; and zero wrong decodes counted separately from zero missed. The nice-to-pass sensitivity ladder is the named drop candidate and is unmet."

endlocal & exit /b %ERRORLEVEL%

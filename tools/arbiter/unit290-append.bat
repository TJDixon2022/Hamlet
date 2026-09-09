@echo off
rem ============================================================
rem  unit290-append.bat  -  unit 290's one call to
rem                         outcome-append.bat
rem
rem  IT WAS NOT RUN. This session's shell refused every .bat in
rem  three invocation forms - a relative path with redirection,
rem  cmd //c, and ./tools/arbiter/... - and the session is non
rem  interactive, so there was nobody to approve them. The entry
rem  was written with the file-editing tools instead, in the
rem  format outcome-entry.py produces, and says so on its face.
rem
rem  Unit 289 measured the same refusal and work instruction 290
rem  anticipated it. This file is committed so the arguments can
rem  be replayed rather than reconstructed from the entry.
rem
rem  NO APOSTROPHES BELOW. Sixteen units of measured shell
rem  behaviour: this shell will not carry a quoted heredoc
rem  containing one, and an apostrophe inside an argument to
rem  outcome-read.bat aborted its inner PowerShell mid-run while
rem  the script still exited 0.
rem ============================================================

setlocal

call "%~dp0outcome-append.bat" ^
  290 ^
  2 ^
  "done" ^
  "Surveyed every fifteen second assumption in the tree BEFORE touching any arithmetic, so criterion 4 is a census rather than a list of this unit own edits, then made the slot grid a value the callers pass instead of a const and threaded it through the cutter, the watch, the sidecar, the turn ring and the two sentences on screen." ^
  "Ft8Slots.SlotStart could not express a 7.5 second boundary at all, and it is arithmetic rather than a constant. It computed (trueUtc.Second / (int)SlotSeconds) * (int)SlotSeconds and built a DateTime from whole seconds, so at 7.5 the cast is 7 - a seven second grid wearing a 7.5 second grid name, right at :00 by coincidence and wrong at the other seven boundaries in the minute, out by three and a half seconds at :52.5. The DateTime constructor has no field for the half either. Rewritten in ticks anchored on the minute, which is the largest unit both lengths divide exactly." ^
  "continue" ^
  "Step 2 entry condition was satisfied by step 1 closing, and step 2 is on the critical path for step 4. The census had to run first because the fourth criterion is closed by naming rather than by fixing, and a unit that starts changing call sites writes a list of its own edits wearing a survey clothes." ^
  "Two things on my own authority, both about display rather than measurement. First, the ring shows one decimal where the slot is not a whole number of seconds and whole seconds where it is - rounding up in whole seconds would have opened an FT4 slot at 8, and 8 is a slot length nothing in this application is cutting on. SecondsLeft keeps its int and its ceiling and nothing on screen reads it any more. Second, the sidecar and the two on-screen sentences name the slot LENGTH rather than the mode name, because the length is what was measured and the name is a label - and because the 4.48 against 5.04 question is open, so a sheet reading FT4 would be answering it. I did NOT settle that question, the candidate sweep or the version scheme." ^
  "Work instruction 290 tasks 1 to 7, under the arbiter decision block for step 2, together with PHASE_PLAN.md named-alternatives table - the tree disagrees with this plan, the tree wins, report the mismatch and continue." ^
  "one session, seven of seven tasks, no test suite run, every test filtered by exact name and foregrounded, five dotnet builds" ^
  "Hamlet can cut, watch, count and count down a 7.5 second slot from corrected UTC; a capture says on its own face which grid it was cut on; a slot the operator keyed still says he keyed it rather than claiming the band was empty; and every place in the tree still assuming fifteen seconds is named with file and line - 47 arithmetic, 72 prose and 15 on screen in the application, plus 4 arithmetic and 12 prose in the port, which is named and left alone." ^
  "" ^
  "executed" ^
  "All four must-pass criteria are met and evidenced. One: a minute of audio cuts 4 FT8 slots on the quarter minutes and 8 FT4 slots at :00 :07.5 :15 :22.5 :30 :37.5 :45 :52.5, boundaries from corrected UTC, and the sidecar prints 9 boundaries, corrected to UTC, on 7.50 s slots, 5.04 s transmission. Two: the ring reads 7.5 down to 0.1 and never 8 and never 0, parity is 0 1 0 1 0 1 0 1 across the minute and unchanged an hour and a day later, and running the countdown to zero transmits nothing. Three: 300 of 300 moments across four FT4 slots resolve the transmitted-slot key correctly with 0 misses. Four: the census, with the arithmetic list in full. FT8 is unchanged - 3888 moments tick-identical to the arithmetic this unit replaced."

endlocal & exit /b %ERRORLEVEL%

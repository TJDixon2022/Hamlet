@echo off
rem ============================================================
rem  unit292-append.bat  -  unit 292's one call to
rem                         outcome-append.bat
rem
rem  IT WAS NOT RUN. Work instruction 292 said the authoring
rem  session had run outcome-read.bat successfully again, told
rem  this unit that the two environments demonstrably differ, and
rem  told it to try the one form once and stop the moment it was
rem  refused. It was tried, once, in exactly that form -
rem  ./tools/arbiter/outcome-append.bat, forward slashes, leading
rem  ./, no cd, no redirection. The sandbox answered "This command
rem  requires approval", and this session is non interactive, so
rem  there was nobody to approve it.
rem
rem  That is the FOURTH consecutive unit to measure the refusal:
rem  289, 290, 291 and now 292. No further call was spent on it.
rem
rem  The entry was written with the file-editing tools instead, in
rem  the format outcome-entry.py produces, and says so on its
rem  face. This file is committed so the arguments can be replayed
rem  rather than reconstructed from the entry.
rem
rem  NO APOSTROPHES BELOW. Eighteen units of measured shell
rem  behaviour: this shell will not carry a quoted heredoc
rem  containing one, and an apostrophe inside an argument to the
rem  arbiter scripts aborted their inner PowerShell mid-run while
rem  the script still exited 0.
rem ============================================================

setlocal

call "%~dp0outcome-append.bat" ^
  292 ^
  4 ^
  "partial" ^
  "Answered which of the chips two facts may drive a decoder BEFORE threading anything, and measured the starting position rather than describing it - four real FT4 transmissions through the tabs own ShowDecodes, returning nought - then made the mode one value that the grid, the watch, the cutter, the sidecar and the decoder are all derived from, so no two of them can disagree." ^
  "The reader took the mode and the grid as one argument on purpose, and the reason turned out to be load bearing at task 2. With the grid threaded and the decoder not, the same recording cut four correct 7.5 second slots and still returned nought messages - the tab on two clocks, each half internally consistent, and nothing on screen able to say which was right. That halfway state is asserted in the test file rather than skipped past." ^
  "continue" ^
  "Step 4 is the last bench step, its entry - steps 1, 2 and 3 - was answered, and step 2s remainder was booked here by unit 291s arbiter. The receive half had its seams cut and unpressed by units 289 and 290; the transmit half has no FT4 in it at all, so criteria 3 and 4 were booked to unit 293 before this unit ran." ^
  "Two things on my own authority, both about shape. First, IsChosen drives the grid and the decoder and IsLit may not. Which mode Hamlet is trying to decode is the operators instruction; where the dial is, is the maps reading, and a decoder that changed under him when he turned the dial would let the map override a button he pressed. In the IsChosenElsewhere case the tab runs the mode he chose and adds no second voice - the chip already has its own appearance for the disagreement and DigitalTuneLine already says the tune did not take. Second, the mode travels as one DigitalMode value rather than as a grid and a decoder, for the same reason unit 291 made the contact mode one object: two arguments that must agree are how a reader comes to cut 7.5 second slots and hand them to FT8s decoder. I did NOT settle the 4.48 against 5.04 figure, the version scheme, the widened candidate sweep or the four inherited reds, and the grid still reads both timing numbers from Ft8Sharp.Ft4Timing so a ruling still costs one edit." ^
  "Work instruction 292 tasks 1 to 6, under the arbiter decision block for step 4, together with PHASE_PLAN.md named-alternatives table - the tree disagrees with this plan, the tree wins, report the mismatch and continue." ^
  "one session, six of six tasks, nothing dropped and the named drop candidate taken whole, no test suite run, every test filtered by exact name and foregrounded, nine dotnet builds" ^
  "Pressing FT4 on the Digital tab stops being a button that does nothing. It takes the radio to the bands cited FT4 frequency, cuts the band into 7.5 second slots, and reads FT4 off the air onto the table - the same three things pressing FT8 does - and the thirteen things around it that do not yet follow are named with file and line rather than left for Tim to find at the radio." ^
  "" ^
  "executed" ^
  "Criteria 1 and 2 are met and criteria 3 and 4 were booked to unit 293 by the arbiter before this unit ran, so the step is partial by plan rather than by shortfall. One: four FT4 transmissions driven in at MainWindowViewModel.ShowDecodes with FT4 chosen come back as four rows whose text equals the text that went in - 0 missed, 0 wrong - cut on 7.5 second boundaries and named as read by Ft8Sharp, the port, never Ft8Sharp.Deep which has no FT4 decoder. The same recording at HEAD 9449d02 returned nought. Two: the seven surfaces are walked with no gaps - panel, conversation, ring and filters work, the ring and half the panel because of a change this unit made; tooltips and ledger do not; the right-click menu works up to the point where it would key, which is unit 293s. Thirteen gaps named and none fixed, and task 6 filled the two ledger ones the instruction listed separately from them. FT8 is unchanged: the same recording gives two fifteen second slots read by Deep with both stages on, every boundary in a minute tick identical to Ft8Slots own arithmetic, both screen sentences byte identical after a round trip through FT4, Ft8Turn still recording a null grid on FT8, and the nine FT8 real-audio reader tests 9 of 9."

endlocal & exit /b %ERRORLEVEL%

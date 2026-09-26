@echo off
rem ============================================================
rem  unit294-append.bat  -  unit 294's one call to
rem                         outcome-append.bat
rem
rem  IT WAS NOT RUN AND IT WAS NOT ATTEMPTED. Work instruction
rem  294 records that units 289, 290, 291 and 292 each measured
rem  the same refusal - the sandbox answering "This command
rem  requires approval" to every .bat under tools/arbiter, in a
rem  non interactive session with nobody to approve it - that
rem  unit 293 was told a fifth measurement was worth nothing and
rem  did not attempt one, and that this unit should go straight
rem  to the file-editing tools. It did.
rem
rem  The entry was written with those tools instead, in the format
rem  outcome-entry.py produces, and says so on its face. This file
rem  is committed so the arguments can be replayed rather than
rem  reconstructed from the entry.
rem
rem  NO APOSTROPHES BELOW. Twenty units of measured shell
rem  behaviour: this shell will not carry a quoted heredoc
rem  containing one, and an apostrophe inside an argument to the
rem  arbiter scripts aborted their inner PowerShell mid run while
rem  the script still exited 0.
rem ============================================================

setlocal

call "%~dp0outcome-append.bat" ^
  294 ^
  4 ^
  "done" ^
  "Drove the two surfaces before changing either - a station heard four FT4 slots ago read your move 2 slots and the menu offered 3 shapes against FT8s 5 - then threaded SlotGrid through the ledgers slot arithmetic and deleted the second copy of it in the view model, built an FT4 signal to noise estimator in Ft8Sharp.Deep that composes Ft8DeepBaseband rather than parameterising or copying it, measured its error at five rungs FT4 actually decodes at against a threshold written down first, and only then let the report reach the row and the menu." ^
  "The ledgers two defects were one defect. Unit 293s section 4 gave gone quiet and slots ago as separate, and the tree says otherwise: the decision at Ft8ContactState.cs:141 is sinceHeard greater or equal GoneQuietAfterSlots, in slots, and sinceHeard is SlotsAgos answer - so putting the grid into SlotsAgo put it into the threshold with it and GoneQuietAfterSlots was never touched. The second surprise was in the measurement rather than the code: the estimators error is not flat, running from -0.03 dB at -13 dB to -1.20 dB at -1 dB, because GFSK smoothing puts a signal proportional term into the three wrong bins the noise is estimated from. It reads low on strong stations. Reported and not corrected, because a correction fitted to that table would be a fit." ^
  "continue" ^
  "Step 4s criterion 2 names seven surfaces and two of them - the ledger and the right click menu - are the two the independent STATE_AFTER on unit 293 says still do not do on FT4 what they do on FT8. The menu gap reached past this step: step 6s must pass is that Tim answers a CQ and completes an exchange, a conventional exchange carries a signal report each way, and Hamlet could not offer report or roger and report on an FT4 row at all because no FT4 row had a measured ratio. Both items were explicitly parked by unit 293s arbiter and dropped whole as unit 293s task 8, so neither had been tried." ^
  "Three things on my own authority, none of them touching the four questions with Tim. First, the seam for the estimator is neither of the two the instruction offered. Giving Ft8DeepBaseband a geometry would change a file on FT8s decode path, and copying its mixer is what the ports whole argument is against; so Ft4DeepBaseband composes - it asks Ft8DeepBaseband.Build for the mixing, filtering and decimation, which are protocol free, and does FT4s own four tone correlation over the samples it exposes. Not one line of a decode path file changed and no DSP is duplicated. Where the tones sit is read back from CentreFrequencyHz rather than predicted, so no FT8 constant appears in FT4s arithmetic. The one price - Build refuses a rate at which FT8s symbol is not a whole number of baseband samples, which FT4 inherits - is named in the file rather than papered over. Second, I made the ledgers grid a required parameter with no default. A default of FT8s grid would have kept every caller compiling and let the next FT4 caller reintroduce the same silence; instead the four FT8 control suites now name SlotGrid.Ft8 at every call, which turns FT8 unchanged from a claim in a report into an assertion in a test. Third, I measured the ramp symbols rather than excluding them: they carry tone 0, which is known, and a known tone is measurable - the figure is quoted both ways, +0.011 dB, rather than the decision being asserted. I did NOT settle the 4.48 against 5.04 figure, the version scheme, the widened candidate sweep or the four inherited reds, and I put no transmission length on any screen." ^
  "PHASE_PLAN.md step 4, whose criterion 2 names the ledger and the right click menu among its seven surfaces, together with Tims ruling of 2026-09-08 that anything FT8 does that FT4 does not is a gap rather than a scope decision a unit may make; its steps are a hypothesis clause; and its named alternatives table - the tree disagrees with this plan, the tree wins, report the mismatch and continue. The step 4 reading where PHASE_OUTCOME.md carries two STATE_AFTER verdicts is ARBITER.md section 8, which makes STATE_AFTER evidence rather than verdict." ^
  "one session, seven of seven tasks, none dropped, no test suite run, every test filtered by exact name and foregrounded with a stated timeout, every dotnet build foregrounded with the same timeout and none of them counted" ^
  "Tim can answer a station on FT4 the way he answers one on FT8 - with a signal report he can actually send, whose error is measured and stated rather than assumed. An FT4 row that read a dash now reads a number, the right click menu offers the same five message shapes it offers on FT8, and the report the menu carries is the number the row shows. The contact column has stopped telling him a station was heard half as long ago as it was and holding a row at your move for one that fell silent half a minute earlier. With that, the two surfaces of the last bench step that still behaved as though FT4 were FT8 behave as FT4, and what stands between him and a contact is the radio rather than the bench." ^
  "" ^
  "executed" ^
  "All four exit criteria are met and criterion 2s two outstanding surfaces are closed, under either reading of it. THE LEDGER: a station heard four FT4 slots ago read your move 2 slots at HEAD 36dc001 and reads gone quiet 4 slots now; gone quiet was first reported at FT4 slot 8, sixty seconds, and is reported at slot 4, thirty seconds, which is the true count; the walk asserts the count at every slot 1 to 12, which is where a partial fix would break. The forbidden second copy of the arithmetic at MainWindowViewModel.cs:2524 was removed rather than threaded, as the ledgers own remark requires. THE MENU: 5 shapes on FT4 against 5 on FT8, the same five in the same order, with the FT4 report carrying the rows own number - and 3 with no measurement, the reason said out loud, which is what an FT8 row in that position does. The figure was gated on measurement and not on hope: Ft4DeepSignalToNoise mean absolute error against the delivered ratio is 0.58 dB with a 95th percentile of 1.41 dB over 970 messages at five rungs, against a 2 dB threshold written into the test before it was run; the reference offset is derived at FT4s own symbol period, 20.79 dB against FT8s 26.02, and nothing is fitted. The FT4 candidate time bias was measured rather than inherited - one distinct value, -0.048 s, on all 529 on grid trials. End to end through Ft8Reader.Read a slot delivered at -8.01 dB reads -8.35 dB and the cell says -8. The estimator decides nothing: the same FT4 slot decoded again after it has run returns the identical Ft8SlotResult and the samples come back unchanged. FT8 is unchanged and proved so rather than described so: Ft8Unit251SnrAgreementTests re run gives 0.26 dB and 0.62 dB over 510 of 510 decoded, identical to unit 251s record, and the four contact control suites assert every slot count they asserted at HEAD 36dc001. Two inherited tests went red for the right reason and the tests were fixed, not the behaviour. Two remaining items are named and neither is in the way of a criterion: FT4s decoder loses sensitivity off the analysis grid, decoding 105 of 106 on grid and 17 of 106 at the cell centre at -13 dB, which belongs to unit 289s widened sweep and is with the owner; and task 7s two FT8 worded sentences on FT4 paths were fixed and are no longer standing."

endlocal & exit /b %ERRORLEVEL%

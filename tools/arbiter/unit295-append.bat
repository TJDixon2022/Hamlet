@echo off
rem ============================================================
rem  unit295-append.bat  -  unit 295's one call to
rem                         outcome-append.bat
rem
rem  IT WAS NOT RUN AND IT WAS NOT ATTEMPTED. Work instruction
rem  295 records that units 289, 290, 291 and 292 each measured
rem  the same refusal - the sandbox answering "This command
rem  requires approval" to every .bat under tools/arbiter, in a
rem  non interactive session with nobody to approve it - that
rem  units 293 and 294 were told a further measurement was worth
rem  nothing and did not attempt one, and that this unit should
rem  go straight to the file-editing tools. It did.
rem
rem  This session met the same refusal independently, on
rem  sh tools/unit295-census15.sh, which is recorded in that file.
rem  That is a seventh session refused and not a seventh
rem  measurement of these scripts.
rem
rem  The entry was written with the file-editing tools instead, in
rem  the format outcome-entry.py produces, and says so on its own
rem  face. This file is committed so the arguments can be replayed
rem  rather than reconstructed from the entry.
rem
rem  NO APOSTROPHES BELOW. Twenty one units of measured shell
rem  behaviour: this shell will not carry a quoted heredoc
rem  containing one, and an apostrophe inside an argument to the
rem  arbiter scripts aborted their inner PowerShell mid run while
rem  the script still exited 0.
rem ============================================================

setlocal

call "%~dp0outcome-append.bat" ^
  295 ^
  4 ^
  "done" ^
  "Drove the report leg end to end at HEAD before changing a line - a station calling with a grid, synthesized as FT4 at a commanded ratio and put through the tabs own decode path, then one right click to the wire and back off a real loopback - so the starting position was on the record before anything moved; then rewrote the three decode column tooltips to name both modes explicitly with every figure read out of the file that recorded it; then re ran the whole bench exchange on the report shape, two legs and two clicks, with the ratio the row showed, the number the menu carried and the number the decoder read back asserted as one equality rather than three prints." ^
  "The report leg already worked, untouched, and that is the finding rather than a disappointment - unit 294 built it carefully and the trace shows it does not stop at any stage. What the unit is worth is that criterion 4 stops resting on a test whose written justification is false and starts resting on a capture. The second surprise was the FT8 control: TheWholeChainRunsFromOneRightClickTests is 1 of 3 green on this machine and the two reds die in the row finder before the chain begins, on OfType Grid where unit 280 made the For you row root a StackPanel - the same defect unit 293 fixed in the FT4 sibling and named in the file. It was proved inherited rather than argued: this units only src change was edited back to exactly what HEAD efa0732 held, the project rebuilt and the class re run, and the identical two failures came back. The third was the census - Ft8Reader.NoWholeSlot at Ft8Reception.cs:405 still says there is not a whole fifteen second slot and reaches two bound properties, while the cutter above it has cut on mode.Grid since unit 292. Unit 290 fixed the cutters own two sentences and missed the one above them." ^
  "continue" ^
  "The session that read unit 294s report against step 4 returned partial and named two things: the tooltips, which are criterion 2s one untouched surface of seven, and the bench exchange never re run after the menu gained the report shapes, which is criterion 4. Both were bench satisfiable and the second is the last thing standing between Tim and step 6s exchange, which is at the radio where no unit can repair anything. A signal report is the one thing Hamlet composes that is a measurement rather than a fixed string - RRR and 73 survive the wire every time if they survive it once, and a report carries a number that came off Hamlets own receiver and lands in another operators log." ^
  "Two things on my own authority, neither touching the questions with Tim. First, the three tooltips name both modes explicitly rather than following DigitalMode. Both shapes were honest and the instruction left the choice open. A string naming both is true whichever mode the tab is running, so it cannot go stale the way a mode following one does when a third mode arrives; the operator switching between FT8 and FT4 wants to know what the other mode does, which is exactly what a mode following string hides; and the rejected shape needed a view model property and a new binding on a live column header, which is more surface than a wording fix should buy. Second, I struck unit 293s remark at TheWholeFt4ChainRunsFromOneRightClickTests.cs:40-43 rather than editing it, and kept the RRR leg it justified. The sentence was the stated reason a criterion was met; correcting it in place would leave the criterion resting on a repaired excuse, where striking it and adding the report shape proof beside it makes the criterion rest on a capture. I did NOT settle the 4.48 against 5.04 figure, the version scheme, the widened candidate sweep, the estimators strong signal bias or the four inherited reds, I put no transmission length on any screen, I added no binding for the unbound tooltip, and I did not repair the FT8 test finder or the censuss one false sentence - both are named with their line numbers and left to the owner." ^
  "PHASE_PLAN.md step 4, whose criterion 2 names the tooltips among its seven surfaces and whose criterion 4 is a whole exchange from one right click at the bench, together with Tims ruling of 2026-09-08 that anything FT8 does that FT4 does not is a gap rather than a scope decision a unit may make; its steps are a hypothesis clause; and its named alternatives table - the tree disagrees with this plan, the tree wins, report the mismatch and continue. CLAUDE.md sections 0.0 and 12.1, what Hamlet asserts to the operator, is what the tooltip half of the unit rests on: a tooltip quoting a precision measured for a different mode is the same fault as a log record naming a mode the contact was not made in." ^
  "one session, six of six tasks, none dropped including the named drop candidate, no test suite run, every test filtered by exact name or exact class name and foregrounded, every dotnet build foregrounded, and one measured revert and rebuild to establish that an inherited red was inherited" ^
  "The signal report Tim sends on FT4 is proved to arrive as the number his own screen showed him - measured off his receiver, formatted, packed, keyed, played out of a real sound card, captured on a real loopback and decoded back as itself, with the log on that leg writing MODE=MFSK, SUBMODE=FT4 and RST_SENT=-08 - rather than assumed to survive because the RRR beside it did. And the columns he hovers have stopped telling him FT8s grid and FT8s measurement precision while he is running FT4: the snr column now carries 0.26 dB and 0.62 dB over 510 messages attributed to FT8 and 0.58 dB and 1.41 dB over 970 attributed to FT4, with FT4s strong signal bias disclosed rather than hidden, and the utc and contact columns say what a slot is on each mode instead of asserting fifteen seconds flat." ^
  "" ^
  "executed" ^
  "All four exit criteria are met and both of the two the independent STATE_AFTER on unit 294 named are closed. CRITERION 4, OFF CARRY FORWARD: the whole exchange runs on the report shape from two right clicks on a REAL WASAPI render endpoint with a REAL loopback capture - S34J55x, one of 4 active endpoints and not the default - and it is stated as a loopback because it was one, not a fake sink. The snr cell showed -8, the menu carried W1ABC KC3QIS -08, 241920 samples went out at 48000 Hz, and the FT4 decoder read W1ABC KC3QIS -08 back off the capture; those three numbers are compared in a single Assert.Equal on a tuple. No row was seeded: the row was made by synthesizing FT4 audio at a commanded -8.00 dB and putting it through the tabs own ShowDecodes, so the number the whole proof is about came off the estimator and not out of the test. Unit 293s stated reason for proving the criterion on RRR - that every FT4 rows ratio is null - is struck from the file that asserted it. CRITERION 2: the tooltips were the last of its seven surfaces never touched and all three are rewritten and pinned. The pin reads every figure out of the file that recorded it - docs/unit251-snr-trace.md for FT8, Ft8Reception.ReadFt4 for FT4, SlotGrid for the two grids, the ports symbol encoders for the seven and three tones - rather than carrying its own copies, and it separately asserts that neither modes clause carries the others numbers. HmDecodeContactHelp was measured to be bound to nothing, corrected anyway because a false sentence in the tree is false whether or not a screen shows it, and given no binding. CRITERION 3: re asserted on the report leg - one click, one transmission, OrdinaryUnkey on both legs, exactly four CI-V frames for two clicks, NothingArmed at the boundary nobody clicked, and Ft8ArmedSend.Arm counted at exactly one caller in src by scanning every .cs. CRITERION 1 is unit 292s and was not re run. THE FT8 CONTROL IS 1 OF 3 GREEN AND THE RED IS INHERITED, MEASURED RATHER THAN ARGUED. The FT8 snr tooltip still carries unit 251s figures, asserted against unit 251s own document. Ft8Unit251SnrAgreementTests was not run and did not need to be: Ft8Sharp.Tests references only Ft8Sharp and Ft8Sharp.Deep and cannot see Hamlet.App, which is the only project this unit changed a line of. TWO ITEMS NAMED AND NEITHER IN THE WAY OF A STEP 4 CRITERION: the FT8 finder above, and the re censuss one live on screen fifteen second assumption at Ft8Reception.cs:405, which belongs to step 2, which stays exactly as partial as it was."

endlocal & exit /b %ERRORLEVEL%

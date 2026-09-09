@echo off
rem ============================================================
rem  unit293-append.bat  -  unit 293's one call to
rem                         outcome-append.bat
rem
rem  IT WAS NOT RUN AND IT WAS NOT ATTEMPTED. Work instruction
rem  293 records that units 289, 290, 291 and 292 each measured
rem  the same refusal - the sandbox answering "This command
rem  requires approval" to every .bat under tools/arbiter, in a
rem  non interactive session with nobody to approve it - that the
rem  authoring environment and this one demonstrably differ, and
rem  that a fifth measurement is worth nothing. It told this unit
rem  to go straight to the file-editing tools. It did.
rem
rem  The entry was written with those tools instead, in the format
rem  outcome-entry.py produces, and says so on its face. This file
rem  is committed so the arguments can be replayed rather than
rem  reconstructed from the entry.
rem
rem  NO APOSTROPHES BELOW. Nineteen units of measured shell
rem  behaviour: this shell will not carry a quoted heredoc
rem  containing one, and an apostrophe inside an argument to the
rem  arbiter scripts aborted their inner PowerShell mid run while
rem  the script still exited 0.
rem ============================================================

setlocal

call "%~dp0outcome-append.bat" ^
  293 ^
  4 ^
  "done" ^
  "Measured what the transmit path actually did with FT4 chosen BEFORE changing a line of it - it composed 12.64 s of FT8 tones, armed them for a fifteen second boundary on an FT4 calling frequency, keyed, played, unkeyed and said Sent - then built an FT4 composer beside FT8s over a shared builder, put the grid on the operators own send record so the fit guard and both refusal sentences follow it, and made the log write the mode the tab was running." ^
  "Threading the grid through the arm alone would have been worse than not threading it. DriveTheArmedSend computed Ft8Slots.SlotStart, a quarter minute whatever mode was running, so an FT4 send armed for 07.5 would have come back NotDue at 00 and TooLate at 15 - discarded every time, with the operator having clicked and nothing ever going out. Four of FT4s eight boundaries a minute are not quarter minutes, so half of every operators clicks would have vanished silently. The tick now reads the boundary off the armed sends own grid." ^
  "continue" ^
  "Criteria 3 and 4 were the only unmet criteria of the last bench step in the phase, booked to this unit by unit 292s arbiter on the record before that unit ran, and untried. Every piece they needed already existed: unit 289 built and nailed Ft4SymbolEncoder and Ft4Waveform in the port, unit 290 made the slot grid a value expressible at 7.5 seconds, unit 291 made the log able to say MFSK plus FT4, and unit 292 made DigitalMode the one value the receive half derives from." ^
  "Three things on my own authority, all about shape rather than about any of the four questions with Tim. First, the FT4 composer is Ft8Composers sibling and not its branch: both are faces onto a shared DigitalComposer that holds the packing, the round trip, the five refusals and the drive, and what each face supplies is a ComposeGeometry - four constants and three port calls read off Ft8Sharp. The two honest alternatives were one implementation taking the modulation as a value or two copies of a file, and the copy is what the ports whole argument is against. Second, the grid rides on OperatorSend rather than on Ft8TransmitSequence, because the sequence is built once when the radio connects and outlives any number of presses of the mode strip - a grid held there would be a second place the operators choice lives, able to disagree with the audio in the very record it was being asked about. It is an init property defaulting to FT8s grid, which is the status quo written down and is the same rule DigitalModes.Grid already states. Third, I fixed one screen this units own change would otherwise have broken: SendingLine formatted HH mm ss, which is every FT8 boundary exactly and four of FT4s eight wrong by half a second, and a sentence naming a moment the transmission does not start at is 0.0 in the one line telling him what is about to go on the band. I did NOT settle the 4.48 against 5.04 figure - the refusal sentences read the occupancy off the grid and no literal was typed anywhere - and I did not settle the version scheme, the widened candidate sweep or the four inherited reds. The absent FT4 signal to noise ratio stays a named gap and no ratio was invented or substituted." ^
  "Work instruction 293 tasks 1 to 7, under the arbiter decision block for step 4, together with PHASE_PLAN.md named-alternatives table - the tree disagrees with this plan, the tree wins, report the mismatch and continue." ^
  "one session, seven of eight tasks, task 8 dropped whole as the named drop candidate, no test suite run, every test filtered by exact name and foregrounded with a stated timeout, twelve dotnet builds" ^
  "Hamlet can answer a station on FT4. One right click composes an FT4 transmission, arms it for the next 7.5 second boundary, keys the radio, plays it and unkeys through the same abort every FT8 transmission goes through - proved on a real sound card with a real loopback capture, the decoder reading back the exact string the menu item was carrying - and the contact it makes is written into the permanent log as the mode it was actually made in rather than as FT8. With that, every bench step in the phase is answered and what remains is Tim at his own radio." ^
  "" ^
  "executed" ^
  "All four exit criteria are met. Criteria 1 and 2 were met by unit 292 and are unchanged. Criterion 3: one click one transmission asserted four ways on FT4 - nothing but an arm arms, a second arm replaces, four FT4 boundaries after one arm produce exactly one keying counted on the bytes, and a decode, four ticks and a countdown arm nothing with the wire literally empty; the abort fires on the FT4 path and it is the same three frames in the same order an FT8 failure produces, with CameOutOfTransmit reporting TheAbort and RadioIsInReceive true; the stop unarms, fires and names which of the six happened; and a licence refusal and a fit refusal each leave the wire empty rather than keying and then aborting. Ft8ArmedSend.Arm still has exactly one caller in src, counted by reading every .cs file under it. Criterion 4: one real ContextRequested on the realized FT4 row, the acknowledgement invoked through its own command with its own CommandParameter, 241920 samples of FT4 tones out of a real display audio endpoint at 48 kHz peaking at -12.0 dBFS, captured on a real loopback, cut into one 7.5 s slot and read back by the tabs own path as W1ABC KC3QIS RRR - the string the item carried - with 1 transmission in, 0 missed and 0 wrong, the sheet naming Ft8Sharp and never Ft8Sharp.Deep. That is a real render endpoint and not a fake sink; the fake sink run beside it is reported as the fake it is. The log writes MODE=MFSK plus SUBMODE=FT4 on FT4 and MODE=FT8 with SUBMODE absent on FT8. FT8 is unchanged: the composed array is sample identical with the port, the boundary is still a quarter minute, and both refusal sentences are byte identical to what this tree produced at HEAD 9a81d4d."

endlocal & exit /b %ERRORLEVEL%

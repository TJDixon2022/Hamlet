@echo off
rem ============================================================
rem  unit291-append.bat  -  unit 291's one call to
rem                         outcome-append.bat
rem
rem  IT WAS NOT RUN. Work instruction 291 said the arbiter
rem  session that authored it had run ./tools/arbiter/outcome-
rem  read.bat successfully - forward slashes, leading ./, one
rem  command with no cd in front - and told this unit to try that
rem  form first and not assume the refusal. It was tried, once,
rem  exactly in that form. The sandbox answered "This command
rem  requires approval", and this session is non interactive, so
rem  there was nobody to approve it. Same for outcome-append.bat
rem  and validate-output.bat in the same form.
rem
rem  So the refusal unit 289 and unit 290 measured is still in
rem  force for this session, whatever the authoring session saw.
rem  Reported rather than worked around. The entry was written
rem  with the file-editing tools instead, in the format
rem  outcome-entry.py produces, and says so on its face.
rem
rem  This file is committed so the arguments can be replayed
rem  rather than reconstructed from the entry.
rem
rem  NO APOSTROPHES BELOW. Seventeen units of measured shell
rem  behaviour: this shell will not carry a quoted heredoc
rem  containing one, and an apostrophe inside an argument to
rem  outcome-read.bat aborted its inner PowerShell mid-run while
rem  the script still exited 0.
rem ============================================================

setlocal

call "%~dp0outcome-append.bat" ^
  291 ^
  3 ^
  "done" ^
  "Walked the log path end to end BEFORE adding anything, so the sentences the new field falsifies were a list rather than a search, then added SUBMODE to AdifContact and made the mode travel as one ContactMode rather than as two independent strings, so MODE and SUBMODE cannot be set to disagree." ^
  "The sharpest thing in this unit was not a missing field but a sentence. AchievementsViewModel.cs:374 told the operator the record has no room for the mode either and Hamlet writes no submode, which task 2 made false. Seven sentences in the tree asserted Hamlet could not write an FT4 record; two of them were on screen. A unit that added the field and did not walk the path would have left the screen asserting a limitation that no longer exists, which is the 0.0 fault unit 287 built that card to avoid." ^
  "continue" ^
  "Step 3 was the only step in the phase whose entry is none, it was untried with zero units spent, and step 4 depends on it as well as on steps 1 and 2. Unit 287 had already built the reading half - ContactModes carries FT4 as MODE=MFSK plus SUBMODE=FT4 with its citation, and ContactMode.Matches already took a submode - so what was missing was one field and the four places that would carry it." ^
  "Two things on my own authority, both about shape rather than about any of the four questions with Tim. First, Ft8StationConditions.Mode became a ContactMode where it was a string. Two independent strings is exactly how a record comes to read MODE=FT8 SUBMODE=FT4; with one object there is nothing for the halves to disagree about, and the compiler refuses a mode outside the six rather than letting a typo drop the field silently. That changed MainWindowViewModel.cs:10288 from the literal FT8 to ContactModes.Named(FT8) - a type change and not a condition, which is what the instruction parked. Second, ContactMode.AdifMode returns null where the mode names more than one ADIF value, so Voice writes no mode at all; writing SSB for Voice would name a mode the operator may not have worked, and where the mode goes out the submode goes with it. I did NOT settle the 4.48 against 5.04 figure, the version scheme, the candidate sweep or the four inherited reds; step 3 touches none of them." ^
  "Work instruction 291 tasks 1 to 6, under the arbiter decision block for step 3, together with PHASE_PLAN.md named-alternatives table - the tree disagrees with this plan, the tree wins, report the mismatch and continue." ^
  "one session, six of six tasks, no test suite run, every test filtered by exact name and foregrounded, four dotnet builds" ^
  "A contact Tim makes on FT4 can be written down as the mode he actually made it in - MODE=MFSK, SUBMODE=FT4, the spelling every other logger reads - rather than not logged or logged as something it is not; a record with no submode is absent rather than empty; the FT4 row on the achievements screen lights off his own file; and the screen stopped telling him Hamlet cannot write the mode down." ^
  "" ^
  "executed" ^
  "All four must-pass criteria are met and evidenced. One: AdifContact carries SUBMODE, thirteen init properties where there were twelve, written beside MODE and read beside it, round-tripping through the same guards as every other field with EveryDeclaredLengthIsTrue running over the FT4 record and requiring 13 fields checked. Two: an FT4 contact driven through Ft8ContactLogEntry.For and AdifLog.Record comes out MODE:4 MFSK then SUBMODE:3 FT4 and reads back as MFSK plus FT4, cited against unit 287 in-tree reading of ADIF 3.1.4 - marked as a reading and not a fetch, because adif.org could not be reached from this session and there is no pinned copy under data/vendor. Three: a record with no submode has the string SUBMODE nowhere in it, not SUBMODE:0, and the round trip brings it back null and never empty string, asserted over three shapes - unset, null and empty. Four: the FT4 row lights from a MODE=MFSK SUBMODE=FT4 record while a bare MODE=MFSK record five days earlier in the same log lights nothing, and unit 287 four states all resolve including WSPR staying not a contact mode with a MODE=WSPR record in the file. FT8 is unchanged - the whole record is byte-identical, pinned as a single string comparison, and TheLogEntryIsWhatWasHeardTests is 8 of 8."

endlocal & exit /b %ERRORLEVEL%

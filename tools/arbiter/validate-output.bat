@echo off
rem ============================================================
rem  validate-output.bat  -  is that report shaped like a report?
rem
rem      validate-output.bat [path-to-output.md]
rem
rem      0  valid - every rule passed
rem      1  A RULE FAILED - the failing rules are named above
rem      2  the file is absent or unreadable
rem
rem  IT HOLDS ITS OWN COPY OF THE RULES AND PRINTS THEM.
rem  CPS-DEC-066: two independent checks that must agree is what
rem  CLAUDE_CODE.md section 5 already describes when it says the
rem  same gate runs twice. This script does NOT read section 8's
rem  shape out of CLAUDE_CODE.md at run time - parsing one out of
rem  the other collapses them into one check wearing two coats,
rem  and a reformatted standard would silently disable it. Two
rem  copies are only safe because the rules applied are printed,
rem  so a divergence is visible in this output instead of assumed
rem  away. Nothing else compares them.
rem
rem  THE RULES, from CLAUDE_CODE.md section 8:
rem    1  a UNIT: line above section 1, parseable
rem    2  the four top-level sections, in order, exact names
rem    3  no FIFTH top-level section
rem    4  section 4 present even when empty
rem    5  section 3 non-empty
rem    6  THE ORDERING BLOCK above the UNIT: line - A the phase
rem       goal, B the step and its exit criteria, C the report last,
rem       and C naming how many items section 4 raises. Added by
rem       050. Presence only; the content is a reading.
rem    7  no placeholder token in the header block. Added by 249
rem       after 248 shipped SUITE_TOTAL_PENDING on its NUMBER: line.
rem
rem  ON "NO OTHER HEADINGS", and why ### is allowed. Section 8
rem  reads "Four sections, in this order, no other headings." The
rem  sentence is about the four sections and their ORDER, and
rem  section 8 adjudicates exactly one boundary case - the UNIT:
rem  line - as "not a fifth heading", which is the concern it is
rem  voicing: a fifth SECTION. CLAUDE_CODE.md itself nests ###
rem  under ## throughout, section 8's own list asks section 1 for
rem  content that in practice needs internal structure, and every
rem  report from 023 onward has used ### without objection. So
rem  this script checks the four ## and IGNORES ### and deeper.
rem  It prints that it does, because it is the one rule here that
rem  is a reading rather than a quotation, and a validator that
rem  failed every real report is a validator nobody runs.
rem
rem  Batch, not PowerShell: a .ps1 will not run on this machine.
rem  The inline powershell -NoProfile -Command calls are not
rem  script files and are used where cmd cannot count lines under
rem  a heading.
rem
rem  Generated 2026-08-28 for: work instructions 038 task 4
rem ============================================================

setlocal

set "RC=0"
set "FILE=%~1"
if "%FILE%"=="" set "FILE=C:\Source\HamLet\output.md"

echo.
echo ============================================================
echo  validate-output
echo    file : %FILE%
echo ============================================================
echo.
echo  Rules applied, held by this script and NOT read from
echo  CLAUDE_CODE.md at run time ^(CPS-DEC-066^):
echo    1  a UNIT: line above section 1, parseable
echo    2  the four top-level sections, in order, exact names
echo    3  no fifth top-level section
echo    4  section 4 present even when empty
echo    5  section 3 non-empty
echo    -  ### and deeper are IGNORED: section 8's "no other
echo       headings" is read as governing top-level sections
echo    -  section names are joined with ~ for display: a ^| in an
echo       echo is a PIPE, and echoing the list raw made cmd try to
echo       run "2." as a command
echo.

if not exist "%FILE%" (
  echo   UNREADABLE - no such file: %FILE%
  echo   Absent is not invalid. Nothing was read, so nothing is known
  echo   about the shape of a report that may never have been written.
  set "RC=2"
  goto :end
)

set /a FAILED=0

rem --- rule 1: the UNIT: line -----------------------------------
rem  Read through PowerShell rather than findstr: a report saved as
rem  UTF-8 WITH A BOM puts three bytes in front of the U in UNIT, and
rem  findstr /b then reports no UNIT: line at all - accusing a correct
rem  report of a fault it does not have. Found by running this against
rem  a BOM'd fixture, not by reading it.
set "UNITLINE="
for /f "usebackq delims=" %%L in (`powershell -NoProfile -Command "$l=(Get-Content -LiteralPath '%FILE%' -TotalCount 60 | Where-Object { $_.TrimStart([char]0xFEFF) -like 'UNIT:*' } | Select-Object -First 1); if($l){ $l.TrimStart([char]0xFEFF) }"`) do set "UNITLINE=%%L"
if defined UNITLINE (
  echo   ok      rule 1  UNIT: line present
  echo                   %UNITLINE%
) else (
  echo   FAILED  rule 1  no UNIT: line above section 1
  echo                   Section 8: output.md is overwritten in place, so
  echo                   without it "this session did not write the report"
  echo                   and "this is last week's report" are the same file.
  set /a FAILED+=1
)

rem --- rules 2 and 3: the four top-level sections ----------------
set "SECS="
for /f "usebackq tokens=* delims=" %%L in (`powershell -NoProfile -Command "(Select-String -Path '%FILE%' -Pattern '^## ' | ForEach-Object { $_.Line.Substring(3).Trim() }) -join ' ~ '"`) do set "SECS=%%L"

set "WANT=1. What Claude did ~ 2. What the owner should expect ~ 3. What you should see ~ 4. What's blocking us"
if "%SECS%"=="%WANT%" (
  echo   ok      rule 2  four top-level sections, in order, exact names
  echo   ok      rule 3  no fifth top-level section
) else (
  echo   FAILED  rule 2/3  the top-level sections are not the four expected
  echo                   expected : %WANT%
  echo                   found    : %SECS%
  set /a FAILED+=1
)

rem --- rule 4: section 4 present even when empty -----------------
findstr /b /c:"## 4. What's blocking us" "%FILE%" >nul
if errorlevel 1 (
  echo   FAILED  rule 4  section 4 is absent
  echo                   "Empty is a real answer" - an absent section 4 is
  echo                   not the same thing and does not say it.
  set /a FAILED+=1
) else (
  echo   ok      rule 4  section 4 present
)

rem --- rule 5: section 3 non-empty -------------------------------
set "S3="
for /f "usebackq delims=" %%N in (`powershell -NoProfile -Command "$t=Get-Content -LiteralPath '%FILE%'; $a=($t | Select-String -Pattern '^## 3\. ' | Select-Object -First 1).LineNumber; $b=($t | Select-String -Pattern '^## 4\. ' | Select-Object -First 1).LineNumber; if($a -and $b){ ($t[$a..($b-2)] | Where-Object { $_.Trim() -ne '' }).Count } else { '' }"`) do set "S3=%%N"
if not defined S3 (
  echo   FAILED  rule 5  section 3 could not be measured
  set /a FAILED+=1
) else if "%S3%"=="0" (
  echo   FAILED  rule 5  section 3 is empty
  echo                   Section 8: section 3 leads with the answer to the
  echo                   question the unit was commissioned to ask.
  set /a FAILED+=1
) else (
  echo   ok      rule 5  section 3 has %S3% non-blank lines
)

rem --- rule 6: the ordering block --------------------------------
rem  THE READER IS THE ARBITER AND THE REPORT IS WHAT IS IN FRONT OF
rem  IT. 050's ruling: the ordering goes where the reader is, because
rem  a standard read at minute zero is not what a session an hour in
rem  is looking at - CLAUDE_CODE.md section 7's own argument about
rem  the status cadence, one artifact over.
rem
rem  THIS CHECKS PRESENCE, NOT CONTENT. Whether A restates the goal
rem  truly, and whether C's judgment about section 4 is right, are
rem  readings and no script can make them. What is mechanical is that
rem  the block is there and that C committed to a NUMBER of section 4
rem  items - a block with the count left out is one nobody filled in.
rem
rem  FLAT, NOT A PARENTHESISED BLOCK. The first draft of this rule was
rem  spliced inside rule 5's else and cmd answered ") was unexpected
rem  at this time." 045 lost an iteration to the same shape.
set "OB=0"
for /f "usebackq delims=" %%B in (`powershell -NoProfile -Command "$t=Get-Content -LiteralPath '%FILE%' -TotalCount 60; $h=($t | Where-Object { $_ -match 'READ IN THIS ORDER' } | Measure-Object).Count; $a=($t | Where-Object { $_ -match '^A\.' } | Measure-Object).Count; $b=($t | Where-Object { $_ -match '^B\.' } | Measure-Object).Count; $c=($t | Where-Object { $_ -match '^C\.' } | Measure-Object).Count; $n=($t | Where-Object { $_ -match 'raises \d+ item' } | Measure-Object).Count; if($h -ge 1 -and $a -ge 1 -and $b -ge 1 -and $c -ge 1 -and $n -ge 1){ '1' } elseif($h -ge 1 -or $a -ge 1){ '2' } else { '0' }"`) do set "OB=%%B"
if "%OB%"=="1" goto :ob_ok
if "%OB%"=="2" goto :ob_partial
echo   FAILED  rule 6  no ordering block above the UNIT: line
echo                   050's ruling: A the phase goal, B the step and its
echo                   exit criteria, C the report last. It goes where the
echo                   reader is.
set /a FAILED+=1
goto :ob_done
:ob_partial
echo   FAILED  rule 6  the ordering block is incomplete
echo                   It needs the header, an A., a B., a C., and C must
echo                   name how many items section 4 raises.
set /a FAILED+=1
goto :ob_done
:ob_ok
echo   ok      rule 6  ordering block present, A B C, and C names a count
:ob_done

rem --- rule 7: no placeholder in the header block -----------------
rem  UNIT 248 SHIPPED "Ft8Sharp.Tests SUITE_TOTAL_PENDING" ON ITS
rem  NUMBER: LINE. The header block is the one part the owner reads
rem  first, and that line named a total nobody read back.
rem
rem  A PLACEHOLDER IS NOT A SMALL MISTAKE HERE. Every other rule in
rem  this file checks that a report is SHAPED like a report; this one
rem  checks that the shape was filled in. A report with all four
rem  sections and an unfilled number passes rules 1 to 6 and is still
rem  a failed report.
rem
rem  IT LOOKS AT THE HEADER BLOCK ONLY, AND THE BLOCK IS EVERYTHING
rem  BEFORE THE "## 1." HEADING - not a line count. The first cut of
rem  this rule read 60 lines, the window rule 6 uses, and MISSED 248'S
rem  OWN TOKEN: that report's header runs past line 60 because its
rem  NUMBER: and TESTS: lines wrap, and the token sits at line 71. A
rem  rule that cannot catch the case it was written for is worse than
rem  none, so the boundary is the heading rather than a guess at how
rem  long a header gets.
rem
rem  A placeholder deeper in the prose may be a session quoting one,
rem  naming one, or explaining why it refused to ship one, and a rule
rem  that failed those would be a rule nobody runs. This unit own
rem  report quotes 248's token in section 3 and must still pass.
rem  must still pass.
rem
rem  THE TOKENS ARE THE ONES THIS PROJECT HAS ACTUALLY PRODUCED plus
rem  the usual suspects. It is a list rather than a pattern because a
rem  pattern loose enough to catch them all catches real prose too.
set "PH=0"
for /f "usebackq delims=" %%P in (`powershell -NoProfile -Command "$a=Get-Content -LiteralPath '%FILE%'; $m=($a | Select-String -Pattern '^## 1\.' | Select-Object -First 1); $i=if($m){$m.LineNumber}else{[Math]::Min(80,$a.Count)}; $t=$a[0..([Math]::Max(0,$i-2))]; $n=($t | Where-Object { $_ -cmatch '_PENDING|PENDING_|TBD|TODO|FIXME|XXX|<FILL|FILL IN>|PLACEHOLDER' } | Measure-Object).Count; $n"`) do set "PH=%%P"
if "%PH%"=="0" goto :ph_ok
echo   FAILED  rule 7  a placeholder token is in the header block
echo                   The header is what the owner reads first. Unit 248
echo                   shipped SUITE_TOTAL_PENDING on its NUMBER: line and
echo                   named a total nobody read back.
echo                   Read the number back, or say plainly that it was not
echo                   measured. "not measured" is a real answer; a token
echo                   left in is not.
set /a FAILED+=1
goto :ph_done
:ph_ok
echo   ok      rule 7  no placeholder token in the header block
:ph_done

echo.
if %FAILED%==0 (
  echo   VALID - all seven rules passed.
  set "RC=0"
) else (
  echo   INVALID - %FAILED% rule^(s^) failed, named above.
  set "RC=1"
)

rem ============================================================
:end
echo.
echo validate-output exit %RC%
endlocal & exit /b %RC%

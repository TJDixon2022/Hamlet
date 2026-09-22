@echo off
rem ============================================================
rem  readkey.bat  -  read one KEY: value out of a .md, whatever
rem                  transport the file arrived in.
rem
rem      call readkey.bat <file> <KEY> <VARNAME>
rem
rem  Sets <VARNAME> to the value of the FIRST line matching
rem  ^KEY: in the file, trimmed. Leaves it UNDEFINED where the
rem  file is absent or carries no such line - those are two
rem  different facts to the caller and neither is an empty
rem  string it might mistake for a value.
rem
rem  AND IT SAYS WHEN IT HAD TO NORMALIZE. One line on stdout,
rem  so the fact reaches the report and the ledger rather than
rem  nowhere. The owner's ruling of 2026-09-01.
rem
rem  WHY THIS EXISTS RATHER THAN SEVEN LOCAL FIXES. 058 task 1
rem  measured every reader in tools\arbiter\ against four
rem  transports. PowerShell was already tolerant of all four -
rem  Get-Content returns the same three lines from LF, CRLF,
rem  CR-only and CRLF+BOM, and strips the BOM. findstr was the
rem  only reader that failed, and it failed in TWO
rem  COMPLEMENTARY WAYS:
rem
rem    CR-only     findstr /b finds only the FIRST field. The
rem                whole file is one line to it, so every key
rem                below line 1 reports rc=1 - absent.
rem    CRLF+BOM    findstr /b fails on the FIRST field only.
rem                Three bytes sit in front of the key.
rem
rem  Between them they cover every line in the file, which is
rem  why this could not be left to care.
rem
rem  WHY THE READER CHANGED RATHER THAN THE FILE BEING COPIED.
rem  The other option was normalizing into a scratch copy and
rem  running findstr over that. Rejected: it needs a writable
rem  scratch path in every root this runs against, it needs
rem  cleanup on every exit arm including the failing ones, and
rem  IT CREATES A SECOND COPY OF A FILE THE CALLER IS ABOUT TO
rem  MAKE A DECISION FROM - the stale-copy class CPS-DEC-001
rem  and CPS-DEC-008 both exist to keep out of this project.
rem  Changing the reader costs one PowerShell launch, which
rem  every one of these scripts already pays several times.
rem
rem  THE PRECEDENT IS IN THIS REPOSITORY ALREADY.
rem  validate-output.bat's rule 1 met the BOM half of this and
rem  fixed it exactly this way, and its comment records that it
rem  was found by running a BOM'd fixture rather than by
rem  reading the code.
rem
rem  NO setlocal. The caller needs the variable this sets, and
rem  endlocal would discard it. Nothing here writes a file, so
rem  there is no state to scope. Delayed expansion is NOT
rem  turned on - this file does not have it and does not need
rem  it.
rem ============================================================

set "RK_FILE=%~1"
set "RK_KEY=%~2"
set "RK_VAR=%~3"

if "%RK_FILE%"=="" goto :rkdone
if "%RK_KEY%"==""  goto :rkdone
if "%RK_VAR%"==""  goto :rkdone

set "%RK_VAR%="
set "RK_SHAPE="

if not exist "%RK_FILE%" goto :rkdone

rem  ONE LAUNCH, BOTH FACTS. The shape and the value come back on
rem  their own tagged lines so the caller gets the value in a
rem  variable and the transport note on stdout, without a second
rem  read of the same bytes - two reads of one file is two
rem  answers waiting to disagree.
rem
rem  The bytes are read whole and split on a regex that accepts
rem  all three separators, rather than through Get-Content, so
rem  the shape can be REPORTED rather than only survived.
for /f "usebackq tokens=1,* delims=|" %%A in (`powershell -NoProfile -Command "$p='%RK_FILE%'; $k='%RK_KEY%'; $b=[System.IO.File]::ReadAllBytes($p); $bom=($b.Length -ge 3 -and $b[0] -eq 239 -and $b[1] -eq 187 -and $b[2] -eq 191); $s=[System.Text.Encoding]::UTF8.GetString($b); if($bom){ $s=$s.Substring(1) }; $crlf=$s.Contains([string][char]13+[string][char]10); $cr=[regex]::IsMatch($s,[string][char]13+'(?!'+[string][char]10+')'); $shape=@(); if($bom){$shape+='bom'}; if($cr){$shape+='cr'} elseif($crlf){$shape+='crlf'}; if($shape.Count -eq 0){$shape=@('lf')}; 'SHAPE|'+($shape -join '+'); if($s.IndexOf([char]0xFFFD) -ge 0 -or [regex]::IsMatch($s,[char]0x00E2+[char]0x0080+'['+[char]0x0080+'-'+[char]0x00BF+']|'+[char]0x00C3+'['+[char]0x0080+'-'+[char]0x00BF+']'+[char]0x00C2)){ 'MOJI|1' }; foreach($ln in ([regex]::Split($s,[string][char]13+[string][char]10+'|'+[string][char]10+'|'+[string][char]13))){ if($ln.StartsWith($k+':')){ 'VALUE|'+$ln.Substring($k.Length+1).Trim(); break } }"`) do (
  if "%%A"=="SHAPE" set "RK_SHAPE=%%B"
  if "%%A"=="MOJI"  set "RK_MOJI=1"
  if "%%A"=="VALUE" set "%RK_VAR%=%%B"
)

rem  IT SAYS SO, AND ONLY WHEN THERE IS SOMETHING TO SAY. A plain
rem  LF file prints nothing: a line on every read would be noise
rem  on every run, and a note nobody reads is the silence this
rem  ruling exists to end wearing a different coat.
rem  MOJIBAKE IS CONTENT AND IS NEVER NORMALIZED AWAY - the ruling.
rem  It is DETECTED and reported under its own word, because a
rem  corruption and a transport difference looking alike is how the
rem  double-encoding spread through PHASE_OUTCOME.md for units before
rem  anyone noticed. Nothing is repaired and no byte is changed, and
rem  THE VALUE IS STILL RETURNED: a corrupt NOTE must not cost the
rem  caller the field it actually asked for.
if defined RK_MOJI echo   MOJIBAKE: %RK_FILE% carries double-encoded or invalid bytes - NOT normalized, NOT repaired, read it
if not defined RK_SHAPE goto :rkdone
if "%RK_SHAPE%"=="lf" goto :rkdone
echo   normalized: %RK_FILE% is %RK_SHAPE% - read as line breaks, nothing else changed

:rkdone
set "RK_FILE="
set "RK_KEY="
set "RK_VAR="
set "RK_SHAPE="
set "RK_MOJI="
goto :eof

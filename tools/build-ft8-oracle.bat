@echo off
rem ==================================================================
rem  build-ft8-oracle.bat
rem
rem  Builds ft8_lib's own generator, demo/gen_ft8.c, into
rem  C:\Source\ft8_lib\build\gen_ft8.exe
rem
rem  WHAT IT IS FOR. This is the oracle for step 3 criterion 2. Our
rem  C# encoder produces 79 tones for a message; this produces the
rem  tones Goba's own program makes for the same message. If they
rem  match bit for bit, ten units of message-layer work are settled
rem  against the world instead of against themselves.
rem
rem  IT IS NEVER PART OF HAMLET. It lives beside the pinned clone,
rem  outside the tree, and is never committed. It is a test oracle
rem  and nothing in the shipped library calls it.
rem
rem  WHY CLANG AND NOT MSVC. Measured 2026-09-01. ft8_lib is written
rem  for gcc/clang and MSVC refuses it twice over: gen_ft8.c uses
rem  C99 variable length arrays, which MSVC has never supported in C
rem  mode, and message.c calls stpcpy, which is POSIX and absent from
rem  the Microsoft CRT. Compiling as C++ with /TP trades those for
rem  const-conversion errors. Clang handles the VLAs and the define
rem  below supplies stpcpy.
rem
rem  WHY THE STACK FLAG. Measured 2026-09-01. gen_ft8.c sizes its
rem  audio buffers as C99 variable length arrays from the signal
rem  length, so a 15-second transmission is allocated on the stack.
rem  Clang on Windows defaults to a 1 MB stack and the program dies
rem  with STATUS_STACK_OVERFLOW - not always interactively, where the
rem  shell may hand it more, but reliably under an automated caller.
rem  -Wl,/STACK:16777216 asks the linker for 16 MB.
rem
rem  Re-run this after re-pinning ft8_lib.
rem ==================================================================

setlocal

set "CLONE=C:\Source\ft8_lib"
set "OUTDIR=%CLONE%\build"
set "EXE=%OUTDIR%\gen_ft8.exe"
set "DEC=%OUTDIR%\decode_ft8.exe"

echo.
echo  build-ft8-oracle
echo    clone  : %CLONE%
echo    output : %EXE%
echo             %DEC%
echo.

if not exist "%CLONE%\demo\gen_ft8.c" (
  echo  ERROR: the pinned clone is not at %CLONE%
  echo  Expected %CLONE%\demo\gen_ft8.c and it is not there.
  exit /b 2
)

rem --- find clang, x64 only. The ARM64 build is also installed and
rem --- cannot run here, so the known x64 path is tried first and a
rem --- search is only the fallback. Measured 2026-09-01: clang is at
rem --- VC\Tools\Llvm\x64\bin\clang.exe under the VS install root.
set "CLANG="

for %%R in (
  "%ProgramFiles%\Microsoft Visual Studio\18\Insiders"
  "%ProgramFiles%\Microsoft Visual Studio\2026\Insiders"
  "%ProgramFiles%\Microsoft Visual Studio\2026\Community"
  "%ProgramFiles%\Microsoft Visual Studio\2026\Professional"
  "%ProgramFiles%\Microsoft Visual Studio\2026\Enterprise"
  "%ProgramFiles%\Microsoft Visual Studio\2022\Community"
  "%ProgramFiles%\Microsoft Visual Studio\2022\Professional"
  "%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise"
) do (
  if not defined CLANG if exist "%%~R\VC\Tools\Llvm\x64\bin\clang.exe" set "CLANG=%%~R\VC\Tools\Llvm\x64\bin\clang.exe"
)

if not defined CLANG (
  echo    no clang at a known path, searching...
  for /f "delims=" %%C in ('dir /b /s "%ProgramFiles%\Microsoft Visual Studio\clang.exe" 2^^^>nul ^^^| findstr /i /c:"\\Llvm\\x64\\"') do (
    if not defined CLANG set "CLANG=%%C"
  )
)

if not defined CLANG (
  echo  ERROR: no x64 clang.exe found.
  echo.
  echo  Install it: Visual Studio Installer, Modify, Individual
  echo  components, tick "C++ Clang Compiler for Windows".
  echo  The C++ workload alone is not enough - MSVC cannot build this.
  echo.
  echo  If it IS installed, run this and send the path:
  echo    dir /b /s "%ProgramFiles%\Microsoft Visual Studio\clang.exe"
  exit /b 3
)

echo    clang  : %CLANG%
echo.

if not exist "%OUTDIR%" mkdir "%OUTDIR%"

"%CLANG%" -D_CRT_SECURE_NO_WARNINGS "-Dstpcpy(d,s)=(strcpy(d,s),(d)+strlen(s))" ^
  -I "%CLONE%" -o "%EXE%" ^
  "%CLONE%\demo\gen_ft8.c" ^
  "%CLONE%\ft8\constants.c" ^
  "%CLONE%\ft8\crc.c" ^
  "%CLONE%\ft8\encode.c" ^
  "%CLONE%\ft8\text.c" ^
  "%CLONE%\ft8\message.c" ^
  "%CLONE%\common\wave.c" -Wl,/STACK:16777216 2>&1 | findstr /i ": error"

if not exist "%EXE%" (
  echo.
  echo  BUILD FAILED. No executable at %EXE%
  echo  Any error lines are above; warnings are suppressed.
  exit /b 4
)

echo  Built. Proving it runs and produces tones...
echo.
"%EXE%" "CQ K1ABC FN42" "%TEMP%\oracle-selftest.wav"
if errorlevel 1 (
  echo.
  echo  BUILT BUT WOULD NOT RUN. Exit %ERRORLEVEL%.
  exit /b 5
)

del "%TEMP%\oracle-selftest.wav" >nul 2>&1

echo  Generator built. Now the decoder...
echo.

rem  THE DECODER, WHICH STEP 6 WANTS FIRST. HM-OPEN-065. Unit 220
rem  measured that 96 of 169 strong misses are not present as far as
rem  THIS receiver can see, and the question left is whether they are
rem  present as far as the pin can see. Only upstream's own decoder
rem  answers that, and a unit may not run a compiler - ARBITER.md
rem  section 6 makes that the owner's class.
rem
rem  decode.c and decode_ft8.c both use C99 variable length arrays, so
rem  the same clang reasoning applies as for the generator. audio.c
rem  guards PortAudio behind USE_PORTAUDIO, which is deliberately NOT
rem  defined - the WAV path is what the oracle needs and the live
rem  capture path is not.
rem  AND THE FFT, WHICH ONLY THE DECODER NEEDS. monitor.c calls
rem  kiss_fftr_alloc and kiss_fft_alloc, and those live in the
rem  clone fft folder rather than under ft8 or common - which is
rem  why the generator built without them and the decoder would
rem  not link. Measured 2026-09-02 as two undefined symbols.
rem
rem  THREE MORE POSIX SHIMS, AND THEY COST NOTHING. Measured
rem  2026-09-02: decode_ft8.c calls clock_gettime, CLOCK_REALTIME
rem  and gmtime_r, none of which the Microsoft CRT has. All three
rem  are inside the live-capture slot-timing loop at lines 345-360
rem  - the branch that waits for the top of a fifteen second slot
rem  from a sound card. The WAV path never enters it. So the
rem  shims only have to compile, not to keep time: gmtime_r maps
rem  to the Microsoft gmtime_s with its arguments the other way
rem  round, and clock_gettime is filled from time(), which is
rem  whole seconds and would be useless for slot timing and is
rem  never asked to do it.
"%CLANG%" -D_CRT_SECURE_NO_WARNINGS "-Dstpcpy(d,s)=(strcpy(d,s),(d)+strlen(s))" ^
  "-DCLOCK_REALTIME=0" ^
  "-Dclock_gettime(c,ts)=((ts)->tv_sec=time(0),(ts)->tv_nsec=0,0)" ^
  "-Dgmtime_r(t,tm)=(gmtime_s((tm),(t))==0?(tm):0)" ^
  -I "%CLONE%" -o "%DEC%" ^
  "%CLONE%\demo\decode_ft8.c" ^
  "%CLONE%\ft8\constants.c" ^
  "%CLONE%\ft8\crc.c" ^
  "%CLONE%\ft8\encode.c" ^
  "%CLONE%\ft8\decode.c" ^
  "%CLONE%\ft8\ldpc.c" ^
  "%CLONE%\ft8\text.c" ^
  "%CLONE%\ft8\message.c" ^
  "%CLONE%\common\wave.c" ^
  "%CLONE%\common\monitor.c" ^
  "%CLONE%\common\audio.c" ^
  "%CLONE%\fft\kiss_fft.c" ^
  "%CLONE%\fft\kiss_fftr.c" -Wl,/STACK:16777216 2>&1 | findstr /i ": error"

if not exist "%DEC%" (
  echo.
  echo  THE GENERATOR BUILT AND THE DECODER DID NOT.
  echo  No executable at %DEC%. Errors are above; warnings are hidden.
  echo  The generator is usable and step 3 stands. HM-OPEN-065 does not.
  exit /b 6
)

echo  Decoder built. Proving it reads a file the generator wrote...
echo.
"%EXE%" "CQ K1ABC FN42" "%TEMP%\oracle-roundtrip.wav" >nul 2>&1
"%DEC%" "%TEMP%\oracle-roundtrip.wav"
if errorlevel 1 (
  echo.
  echo  BUILT BUT WOULD NOT RUN. Exit %ERRORLEVEL%.
  exit /b 7
)
del "%TEMP%\oracle-roundtrip.wav" >nul 2>&1

echo.
echo  ==================================================================
echo   BOTH ORACLES READY
echo     %EXE%
echo     %DEC%
echo   Step 3 criterion 2 is measurable, and HM-OPEN-065 is cleared.
echo  ==================================================================
exit /b 0

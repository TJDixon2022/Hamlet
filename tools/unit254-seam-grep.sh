#!/bin/sh
# Unit 254, task 2: the seam names nothing that can key a radio, open a device,
# start a thread or read a clock. One pattern per invocation - unit 253 recorded
# grep aborting with exit 134 on multi-pattern and -c invocations, so each is run
# on its own and the count is taken from the line count instead of from -c.
f=src/Hamlet.RadioEngine/Transmit/Ft8Composer.cs
for p in TransmitAbort PTT Ptt CivFrame CivWrites CivConstants ISerialPort SerialPort Ic7300 IRig RigState AudioDevice IAudioSource Wasapi NAudio Thread DateTime Stopwatch Environment.TickCount Task async await File. Console. Directory. Process
do
  n=$(grep -F "$p" "$f" | wc -l | tr -d ' ')
  printf '%-20s %s\n' "$p" "$n"
done

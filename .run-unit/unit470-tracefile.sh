#!/bin/sh
# unit 470 - append a run's tables to the trace file.
# Usage: sh .run-unit/unit470-tracefile.sh <trace-suffix> <opening-suffix> "<heading>"
cd /c/Source/HamLet || exit 1
T=.run-unit/unit470-trace.txt
{
  echo ""
  echo "$3 (printouts .run-unit/unit470-trace-$1.txt and unit470-opening-$2.txt)"
  echo "---------------------------------------------------------------------------------------------"
  grep -a -E "^\s*(acquiring|which|states|verdict|dim|449|total|first sure) \|" .run-unit/unit470-trace-$1.txt | sed -E "s/^\s+//"
  grep -a -E "^\s*(opening|hm-req-102|first sure) \|" .run-unit/unit470-opening-$2.txt | sed -E "s/^\s+//"
} >> $T
wc -l $T

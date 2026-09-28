#!/bin/sh
# usage: sh unit474-status.sh STATE TASK BALL NEXT_PASTE "NOTE"
cd /c/Source/HamLet || exit 1
TS=$(date +%Y-%m-%dT%H:%M:%S%:z)
{
  echo "PROTOCOL: 2"
  echo "PROJECT: Hamlet"
  echo "STATE: $1"
  echo "TASK: $2"
  echo "WORK_INSTRUCTION: 474 - a light that says I think I hear CW, a pitch strip, and two buttons that write the owner verdict to telemetry"
  echo "BALL: $3"
  echo "NEXT_PASTE: $4"
  echo "RULES_AT: HM-DEC-184 (2026-09-27)"
  echo "UPDATED: $TS"
  echo "NOTE: $5"
  echo ""
  sed -n '/^---$/,$p' PROJECT_STATUS.md
} > PROJECT_STATUS.md.new
mv PROJECT_STATUS.md.new PROJECT_STATUS.md
head -10 PROJECT_STATUS.md

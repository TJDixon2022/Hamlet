#!/bin/sh
# usage: sh unit476-status.sh STATE TASK BALL NEXT_PASTE "NOTE"
cd /c/Source/HamLet || exit 1
TS=$(date +%Y-%m-%dT%H:%M:%S%:z)
{
  echo "PROTOCOL: 2"
  echo "PROJECT: Hamlet"
  echo "STATE: $1"
  echo "TASK: $2"
  echo "WORK_INSTRUCTION: 476 - the oscilloscope: a mark is the envelope over a threshold, at any pitch"
  echo "BALL: $3"
  echo "NEXT_PASTE: $4"
  echo "RULES_AT: HM-DEC-185 (2026-09-28)"
  echo "UPDATED: $TS"
  echo "NOTE: $5"
  echo ""
  sed -n '/^---$/,$p' PROJECT_STATUS.md
} > PROJECT_STATUS.md.new
mv PROJECT_STATUS.md.new PROJECT_STATUS.md
head -10 PROJECT_STATUS.md

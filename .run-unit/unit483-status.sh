#!/bin/sh
# usage: sh .run-unit/unit483-status.sh STATE TASK BALL NEXT_PASTE NOTE
# UPDATED is read from the clock here, never typed.
cd /c/Source/HamLet || exit 2
now=$(date +%Y-%m-%dT%H:%M:%S%z | sed 's/\([0-9][0-9]\)$/:\1/')
{
  echo "PROTOCOL: 2"
  echo "PROJECT: Hamlet"
  echo "STATE: $1"
  echo "TASK: $2"
  echo "WORK_INSTRUCTION: 483 - the owner's rows name the gate"
  echo "BALL: $3"
  echo "NEXT_PASTE: $4"
  echo "RULES_AT: HM-DEC-188 (2026-09-28)"
  echo "UPDATED: $now"
  echo "NOTE: $5"
  echo ""
  echo "---"
  echo ""
  echo "Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md."
  echo ""
  echo "PROTOCOL names which protocol this header is written against. The long form,"
  echo "STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this"
  echo "one, so nothing here can check conformance to it -- the field says what the"
  echo "file was written to, not that anybody validated it."
} > PROJECT_STATUS.md
echo "status written $now"

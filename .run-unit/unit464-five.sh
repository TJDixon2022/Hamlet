#!/bin/sh
# unit 464 - one gate of the exit five after a build: both carry-forward lines and the three floor tests, each its own invocation.
# Usage: sh .run-unit/unit464-five.sh <suffix> "<n of 4>" <engine|app|named|captures|adjudicated>
cd /c/Source/HamLet || exit 1
case "$3" in
  engine) sh .run-unit/unit464-cf.sh engine "$1" "$2" "Commit gate $1: the engine carry-forward line, 178 expected" ;;
  app) sh .run-unit/unit464-cf.sh app "$1" "$2" "Commit gate $1: the app carry-forward line, 278 expected" ;;
  named) sh .run-unit/unit464-round.sh named "$1" "$2" ;;
  captures) sh .run-unit/unit464-round.sh captures "$1" "$2" ;;
  adjudicated) sh .run-unit/unit464-round.sh adjudicated "$1" "$2" ;;
esac

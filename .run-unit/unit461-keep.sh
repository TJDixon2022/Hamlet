#!/bin/sh
# unit 461 - keep or restore the good CwChannel.cs around the watched-red builds.
# Usage: sh .run-unit/unit461-keep.sh save|restore
cd /c/Source/HamLet || exit 1
F=tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/CwChannel.cs
K=.run-unit/unit461-CwChannel-good.tmp
case "$1" in
  save) cp "$F" "$K" ;;
  restore) cp "$K" "$F" ;;
esac
md5sum "$F" "$K"

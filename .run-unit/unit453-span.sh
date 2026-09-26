#!/bin/sh
# unit 453 - build, then run the named-span printer alone.
# Usage: sh .run-unit/unit453-span.sh <suffix> "<n of 3>" "<note>"
cd /c/Source/HamLet || exit 1
sh .run-unit/unit453-build.sh "span-$1" "$2" "$3: building the named-span printer"
sh .run-unit/unit453-run.sh "span-$1" engine 300 "$2" "$3: printing the named spans" "FullyQualifiedName~.WhatTheNamedWordsReadTests." --no-build

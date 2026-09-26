#!/bin/sh
# unit 452 - one app type alone, --no-build.
# Usage: sh .run-unit/unit452-alone.sh <TypeName> <suffix> "<n of 3>"
cd /c/Source/HamLet || exit 1
sh .run-unit/unit452-run.sh "app-$1-$2" app 300 "$3" "App line loss rerun alone: $1" "FullyQualifiedName~.$1." --no-build

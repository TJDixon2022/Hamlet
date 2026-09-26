#!/bin/sh
# unit 450 - one app type alone, --no-build.
# Usage: sh .run-unit/unit450-alone.sh <TypeName> <suffix> "<n of 3>"
cd /c/Source/HamLet || exit 1
sh .run-unit/unit450-run.sh "app-$1-$2" app 300 "$3" "App line loss rerun alone: $1" "FullyQualifiedName~.$1." --no-build

#!/bin/sh
# unit 453 - one app type alone, --no-build.
# Usage: sh .run-unit/unit453-alone.sh <TypeName> <suffix> "<n of 3>"
cd /c/Source/HamLet || exit 1
sh .run-unit/unit453-run.sh "app-$1-$2" app 300 "$3" "App line loss rerun alone: $1" "FullyQualifiedName~.$1." --no-build

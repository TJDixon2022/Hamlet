#!/bin/sh
# unit 443 - build, then run the added-character trace alone at the tree as it stands.
# Usage: sh .run-unit/unit443-added.sh <suffix> "<n of 3>" "<note>"
cd /c/Source/HamLet || exit 1
sh .run-unit/unit443-build.sh "added-$1" "$2" "$3: building"
sh .run-unit/unit443-run.sh "added-$1" engine 600 "$2" "$3: tracing every sure added letter" "FullyQualifiedName~.WhereTheSureAddedLettersComeFromTests." --no-build

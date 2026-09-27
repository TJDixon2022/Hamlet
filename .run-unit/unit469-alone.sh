#!/bin/sh
# unit 465 - an app type lost to the headless dispatcher loop, run alone (DECIDED (9)).
# Usage: sh .run-unit/unit469-alone.sh <short-name> <TypeName> <suffix> "<n of 4>"
sh /c/Source/HamLet/.run-unit/unit469-run.sh "app-alone-$1-$3" app 300 "$4" "App type $2 alone after a dispatcher-loop loss (DECIDED 9)" "FullyQualifiedName~.$2." --no-build

#!/bin/sh
# unit 445 - one touched or dependent type per invocation, no build.
# Usage: sh .run-unit/unit445-types.sh <TypeName> <timeout-s>
cd /c/Source/HamLet || exit 1
sh .run-unit/unit445-run.sh "type-$1" engine "$2" "3 of 3" "Exit round: touched type $1 alone" "FullyQualifiedName~.$1." --no-build

#!/bin/sh
# unit 462 - after a parity run: keep parity.md's copy under .run-unit, then restore the committed parity.md (the run rewrites only its decode-time rows).
# Usage: sh .run-unit/unit470-paritykeep.sh <suffix>
cd /c/Source/HamLet || exit 1
cp docs/phase-requirements/parity.md .run-unit/unit470-parity-$1.md
echo "== decode time, $1"
grep -A4 "^| condition | recordings | audio s" .run-unit/unit470-parity-$1.md | tail -2
echo "== per condition, $1"
grep -E "^\| \*\*(real HF|synthetic), all\*\*" .run-unit/unit470-parity-$1.md
git checkout -- docs/phase-requirements/parity.md
git status --short docs/phase-requirements/parity.md

#!/bin/sh
# unit 459 - after a parity run: the port's texts against task 0's, parity.md's decode-time and per-condition rows kept.
# Usage: sh .run-unit/unit459-judge.sh <suffix>
cd /c/Source/HamLet || exit 1
sh .run-unit/unit459-portsave.sh "$1" "$1" before
cp docs/phase-requirements/parity.md .run-unit/unit459-parity-$1.md
echo "== decode time, $1"
grep -A6 "^## 4. Decode time" docs/phase-requirements/parity.md | tail -2
echo "== decode time, 458's committed parity.md"
git show 19109b51:docs/phase-requirements/parity.md | grep -A6 "^## 4. Decode time" | tail -2
echo "== per condition, $1"
grep -E "^\| \*\*(real HF|synthetic), all\*\*" docs/phase-requirements/parity.md

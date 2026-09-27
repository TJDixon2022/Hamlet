#!/bin/sh
# unit 464 - assemble output.md: part a, the held-out tables from calibration.md section 3, part b; then validate.
cd /c/Source/HamLet || exit 1
{
  cat .run-unit/unit464-output-a.md
  sed -n '/^## 3. The held-out reliability tables/,/^## 4. MET-CAL/p' docs/phase-requirements/calibration.md | sed -e '1d' -e '$d' -e 's/^\*\*\(ours\|port\), /**\1, /'
  cat .run-unit/unit464-output-b.md
} > output.md
grep -c "" output.md
grep -n "^## " output.md
sh .run-unit/unit464-validate.sh

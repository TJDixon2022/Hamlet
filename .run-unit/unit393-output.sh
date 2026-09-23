cd /c/Source/HamLet
cp output.md .run-unit/unit392-output-kept.md
{
  cat .run-unit/unit393-output-head.md
  sed -n 302,339p .run-unit/unit392-output-kept.md
  echo ""
  sed -n 351,442p .run-unit/unit392-output-kept.md
} > output.md
wc -l output.md
grep -n "^## \|^UNIT:" output.md
sed -n 1p .run-unit/unit392-output-kept.md | od -c | head -1
file output.md

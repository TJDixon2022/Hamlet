cd /c/Source/HamLet
D=docs/phase-cw/unit394-reds.md
cp output.md .run-unit/unit393-output.md
{
  cat .run-unit/unit394-out-a.md
  sed -n 58,110p $D
  cat .run-unit/unit394-out-b.md
  sed -n 17,33p $D
  cat .run-unit/unit394-out-c.md
  sed -n 204,211p $D
  cat .run-unit/unit394-out-d.md
  sed -n 206,266p .run-unit/unit393-output.md
  echo ""
  sed -n 283,415p .run-unit/unit393-output.md
} | tr -d "\r" > output.md
wc -l output.md
grep -n "^## " output.md
grep -n "^UNIT:" output.md

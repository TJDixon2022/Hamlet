cd /c/Source/HamLet
for f in entry rule
do
  echo "== $f"
  sed -n '/openingFrom: 30, openingTo: 46.2/,/TIMELINE/p' .run-unit/unit436-opening-$f.txt > .run-unit/unit436-opensum-$f.txt
  grep -E "^ ?summary \| (speed from the estimator|held speed wpm|single element)[^|]* \| opening \|" .run-unit/unit436-opensum-$f.txt
  echo "opening rows: $(grep -cE '^ ?opening \| [0-9]' .run-unit/unit436-opensum-$f.txt)"
  echo "grid rows: $(grep -E '^ ?opening \| [0-9]' .run-unit/unit436-opensum-$f.txt | grep -c '| grid |')"
  echo "placeholder rows: $(grep -E '^ ?opening \| [0-9]' .run-unit/unit436-opensum-$f.txt | grep -c '■')"
  grep -E "^ ?opening \| text" .run-unit/unit436-opensum-$f.txt
done

cd /c/Source/HamLet/.run-unit
for f in "$@"
do
  echo "=================== $f"
  cat "$f"
done

cd /c/Source/HamLet
# Every commit from unit 395's entry to HEAD, oldest first, with whether it touched src.
git log --reverse --format="%h %s" ee0ea0dc..HEAD | cut -c1-110 > .run-unit/unit397-log.txt
for h in $(git log --reverse --format="%h" ee0ea0dc..HEAD); do
  n=$(git diff --name-only $h^ $h -- src | wc -l)
  echo "$h src-files=$n"
done > .run-unit/unit397-log-src.txt
paste -d " " .run-unit/unit397-log-src.txt .run-unit/unit397-log.txt | grep -E "piece|seam|pair" | awk '{print $1, $2, substr($0, index($0,$4))}' | cut -c1-120

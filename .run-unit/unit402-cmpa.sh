cd /c/Source/HamLet
# Usage: sh .run-unit/unit402-cmpa.sh <tag> - compare a gate round against entry
T=$1
echo "captures rows:"; sh .run-unit/unit402-cmp.sh .run-unit/unit402-floors-1.txt .run-unit/unit402-cap-$T.txt
echo "adjudicated:"; sh .run-unit/unit402-adjcmp.sh .run-unit/unit402-adjud-entry.txt .run-unit/unit402-adjud-$T.txt
for W in gate adj fixtures disp acq recv syn capsig silence whygate twostation; do
  if [ -f .run-unit/unit402-$W-$T-list.txt ]; then
    echo "--- $W list diff against entry:"
    diff .run-unit/unit402-$W-entry-list.txt .run-unit/unit402-$W-$T-list.txt
  fi
done
echo "--- printed numbers:"
grep -hE "^ (real signal|speeds named|no speed|read between|no reading|final speed|named|[0-9]+ wpm at|[a-z-]+-easy:|[0-9]+ characters during|own transmit)" .run-unit/unit402-gate-$T.txt .run-unit/unit402-adj-$T.txt .run-unit/unit402-twostation-$T.txt .run-unit/unit402-silence-$T.txt .run-unit/unit402-acq-$T.txt .run-unit/unit402-recv-$T.txt 2>/dev/null | cut -c1-200

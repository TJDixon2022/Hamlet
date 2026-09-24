#!/bin/sh
# unit 422 - copy unit 421's run scripts, renamed.
cd /c/Source/HamLet/.run-unit || exit 1
for s in cf build round run alone commit srcdiff transmit
do
  sed "s/unit421/unit422/g; s/unit 421/unit 422/g" unit421-$s.sh > unit422-$s.sh
  echo "copied $s"
done
ENTRY=$(git rev-parse --short HEAD)
echo "entry $ENTRY"
sed -i "s/23457c4b/$ENTRY/g" unit422-srcdiff.sh

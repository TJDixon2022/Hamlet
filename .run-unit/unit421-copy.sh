#!/bin/sh
# unit 421 - copy unit 420's run scripts, renamed.
cd /c/Source/HamLet/.run-unit || exit 1
for s in cf build round run alone commit srcdiff transmit
do
  if [ -f unit420-$s.sh ]
  then
    sed "s/unit420/unit421/g; s/unit 420/unit 421/g; s/of 3>/of 4>/g" unit420-$s.sh > unit421-$s.sh
    echo "copied $s"
  fi
done
sed -i "s/b378abeb/23457c4b/g" unit421-srcdiff.sh

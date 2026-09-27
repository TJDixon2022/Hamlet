#!/bin/sh
# unit 462 - copy 461's generic helpers and 459's port save under 462's name.
cd /c/Source/HamLet/.run-unit || exit 1
for n in run build round cf commit textsave tick tx gitstate validate v11 status
do
  sed 's/unit461/unit462/g' unit461-$n.sh > unit462-$n.sh
done
for n in portsave judge
do
  sed 's/unit459/unit462/g' unit459-$n.sh > unit462-$n.sh
done
cat >> unit462-round.sh <<'EOF'
case "$1" in
  parity)
    sh $R "parity-$2" engine 600 "$3" "Both decoders scored alike over every keyed recording" "FullyQualifiedName~.BothDecodersAreScoredAlikeTests." --no-build
    ;;
  second)
    sh $R "second-$2" engine 120 "$3" "The port's own tests" "FullyQualifiedName~.TheSecondDecoderIsAFaithfulPortTests." --no-build
    ;;
  pairs)
    sh $R "pairs-$2" engine 300 "$3" "459's red test, reported only" "FullyQualifiedName~.TheSpeedFollowsTheSendersMarkPairsTests." --no-build
    ;;
esac
EOF
ls unit462-*

#!/bin/sh
# unit 424 - who calls the voices, and every other phrase that could be about the preamp.
cd /c/Source/HamLet || exit 1
echo "== callers"
grep -rn "ReceiveAdvice.For\|RigObservations.For\|ReceiveObstructions.For\|OverflowAdviceFor\|OverflowAdvice\b\|LastReceiverSetup\b" src --include=*.cs --include=*.xaml | cut -c1-220
echo "== P.AMP and front end gain phrases"
grep -rn -i "P\.AMP\|P.AMP/ATT\|front end\b\|front-end" src --include=*.cs --include=*.xaml | cut -c1-220 | head -60
echo "== overflow in the app"
grep -rn "RigField.Overflow\|FrontEndIsOverloading" src/Hamlet.App --include=*.cs --include=*.xaml | cut -c1-200
echo "== band edges in data"
grep -rln "50000000\|50.0\|\"6 m\"\|\"6m\"" data | head

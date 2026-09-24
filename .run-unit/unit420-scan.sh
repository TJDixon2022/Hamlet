#!/bin/sh
# unit 420 - scan the tests that consult the conditions for anything pinning a block's silence.
cd /c/Source/HamLet || exit 1
for f in \
  tests/Hamlet.RadioEngine.Tests/Explore/EveryModeAnswersForEverySettingTests.cs \
  tests/Hamlet.RadioEngine.Tests/Explore/TheBlockStatesWhatTheModeNeedsTests.cs \
  tests/Hamlet.App.Tests/ViewModels/ModeFollowsTheMapAgainTests.cs \
  tests/Hamlet.App.Tests/Views/HowMuchTheApplicationSaysTests.cs \
  tests/Hamlet.App.Tests/Views/TheStatusBarStopsLecturingTests.cs \
  tests/Hamlet.App.Tests/Views/WhichPathsPutNarrationOnTheBarTests.cs \
  tests/Hamlet.RadioEngine.Tests/Rig/TheRoundTripLandsInTheSamePlaceTests.cs \
  tests/Hamlet.RadioEngine.Tests/Rig/TheTuneInSetsOnlyWhatIsInTheWayTests.cs \
  tests/Hamlet.RadioEngine.Tests/Rig/HamletSaysWhatItChangedTests.cs \
  tests/Hamlet.RadioEngine.Tests/Rig/AValueAlreadyRightIsNotWrittenTests.cs \
  tests/Hamlet.RadioEngine.Tests/Rig/OneVoicePerFieldTests.cs \
  tests/Hamlet.RadioEngine.Tests/Rig/TheBannerSaysWhatTheRadioReadBackTests.cs \
  tests/Hamlet.RadioEngine.Tests/Rig/ThePreampFollowsItsOwnTextTests.cs
do
  echo "=== $f"
  grep -n -i "ShortName\|Absent\|Count == 0\|Assert.Empty\|family\|\.Modes\|ForBlock\|QRP\|CW DX\|Coverage\|Neighborhoods\|_000_000\|000000" "$f" | head -30
done

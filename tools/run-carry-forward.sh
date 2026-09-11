#!/bin/sh
# Runs docs/carry-forward-tests.txt filtered by exact type name, foregrounded.
# Unit 323. Not a suite run (HM-DEC-155): every name here is on the list.

APP="TheGlobeOnTheCardFaceTests TheGlobeLineTests TheGlobePlacesStationsTests Unit299GlobeTests TheCqListNudgeTests TheNudgeHoverTests ThePressingOfCqTests TheReadinessHoverTests TheSlotClockTests TheCqReceiptTests ThePanelHoldsThemAllTests TheMapRowFitsTests TheMapOpensTests VoiceTests ThePsk31SeamTests ThePopupZoomIsCappedTests ThePanelScrollsTests TheConnectionLineReadsTests ThePsk31TabIsInertTests ThePsk31PanelHearsTests ThePsk31HearsEveryoneTests ThePsk31ReadsTheConversationTests ThePsk31ConversationCardTests Hamlet.App.Tests.ViewModels.ThePsk31OfferTests ThePsk31CardIsHandedHisGridTests BindingHealthTests.TheMainWindowBindsWithoutOneComplaint ThePsk31TransmitTelemetryTests ThePsk31PanelSpeaksPsk31Tests ThePsk31CqGoesOutTests ThePsk31ExchangeTests ThePowerIsOfferedTests"

ENGINE="TheFlatWorldAssetTests TheGreatCirclePathTests ThePsk31ReferenceIsPinnedTests TheVaricodeTests ThePsk31DemodulatorTests ThePsk31CarrierSearchTests ThePsk31ExchangeParserTests ThePsk31MessageSplitTests ThePsk31ModulatorTests TheFt8AndFt4SendsAreByteIdenticalTests TheUnslottedSendTests ThePsk31TurnTests Hamlet.RadioEngine.Tests.Psk31.ThePsk31OfferTests TheAzimuthalAssetTests AzimuthalMapTests ThePsk31TelemetryTests"

filter() {
  out=""
  for n in $1; do
    if [ -z "$out" ]; then out="FullyQualifiedName~$n"; else out="$out|FullyQualifiedName~$n"; fi
  done
  echo "$out"
}

case "$1" in
  app)
    exec dotnet test tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj --no-restore --nologo -v q --filter "$(filter "$APP")"
    ;;
  engine)
    exec dotnet test tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj --no-restore --nologo -v q --filter "$(filter "$ENGINE")"
    ;;
  *)
    echo "usage: run-carry-forward.sh app|engine" >&2
    exit 2
    ;;
esac

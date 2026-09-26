#!/bin/sh
# unit 455 - task 3 commit: the HM-REQ-072 naming setting, kept.
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "4 of 4" code none "Task 3 kept: 072 met, text byte-identical; task 4 exit round - carry-forward lines, floors, metrics, touched types"
sh .run-unit/unit455-commit.sh .run-unit/unit455-msg3.txt PROJECT_STATUS.md src/Hamlet.RadioEngine/Cw/CwProsignNaming.cs src/Hamlet.RadioEngine/Cw/MorseAlphabet.cs src/Hamlet.App/Settings/AppSettings.cs src/Hamlet.App/ViewModels/CwTranscript.cs src/Hamlet.App/ViewModels/MainWindowViewModel.cs src/Hamlet.App/ViewModels/SettingsViewModel.cs src/Hamlet.App/Views/SettingsWindow.axaml src/Hamlet.App/Controls/CwTerminalControl.cs tests/Hamlet.App.Tests/Cw/TheTwoNamedPatternIsNamedAsTheTerminalIsSetTests.cs .run-unit/unit455-*

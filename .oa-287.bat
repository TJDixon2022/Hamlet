@echo off
rem Unit 287's call into outcome-append.bat, written to a file because this
rem session's shell mangles or refuses long quoted argument lists.
call "%~dp0tools\arbiter\outcome-append.bat" ^
 287 ^
 E ^
 "not started" ^
 "Read what MODE actually holds and what ADIF spells each of the six modes before matching on anything, then build an achievements window beside the contact log carrying the belt and the first of each mode, and reuse BadgeAward and BadgeWindow so a mode first announces itself without a second notice." ^
 "Five of the six cannot be earned and they cannot for four different reasons, not the two the order names. The order says CW has no send path; HM-DEC-059 built one and MainWindowViewModel attaches a CwTransmitter over a KeyerCwSender, so what CW lacks is a way into the log rather than a way onto the air. FT4 and PSK31 are ADIF submodes of MFSK and PSK and AdifContact carries no SUBMODE at all, so they are unrepresentable rather than unearned. Voice is not an ADIF value. And Tools > My contacts is under Radio, not Tools, and has been since unit 278." ^
 "build on" ^
 "The card had to say which reason applies to which row or it would be six rows that all look like things he has not got round to, and most of them are not his to have done. Four states, four marks, four words and a sentence each, so it reads in grayscale. The count is one reading through ContactLogStore, so the belt, the card and the log window cannot come to disagree about what is in the file." ^
 "That the achievements item goes beneath My contacts under Radio rather than under Tools, because beside the contact log is the half of the ruling that carries its own reason and Tools was a factual belief about where the log is; that CW reads as waiting on Hamlet with its own sentence naming the log rather than the send path, because a sentence on screen that is not true is worse than a missing row; and that a beacon mode is never earned including from a MODE=WSPR record, which is legal ADIF any logger would write and still describes a signal nobody answered." ^
 "Work instruction 287 tasks 1 to 6, and Tim's rulings of 2026-09-08 that there is an achievements screen beside the contact log, that the first contact in each mode is an achievement, that text goes where he hovers and a fault speaks unasked, and that the count is every logged contact." ^
 unknown ^
 "Hamlet can tell him what he has done and, for the five things he has not, which of them are his to go and do and which are waiting on the application. One of the six can be earned today and the screen says so without a row that looks earnable and never can be." ^
 "" ^
 executed ^
 "Every bench step of this phase is closed and steps D and E are Tim at his own radio, so no unit can meet their criteria. This one gives him a reading of his own log rather than changing what Hamlet can do, and closes nothing."
exit /b %ERRORLEVEL%

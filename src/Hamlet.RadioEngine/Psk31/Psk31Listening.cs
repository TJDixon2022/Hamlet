// Psk31Listening held one number, OffsetHz = 1000: the single spot above the dial that
// work instruction 314's one PSK31 channel listened at, "a starting place rather than a
// measurement, and the step that finds signals for itself will not need it."
//
// Work instruction 315 task 3 replaced it with Psk31CarrierSearch, which measures where
// every carrier is, and Psk31Listener, which gives each one a demodulator. Nothing reads
// a fixed offset any more, so the constant is gone rather than left for somebody to pick
// up again.
//
// This file is empty because the session that removed the class could not delete it:
// the shell refused both rm and git rm. Delete it by hand with
//     git rm src\Hamlet.RadioEngine\Psk31\Psk31Listening.cs

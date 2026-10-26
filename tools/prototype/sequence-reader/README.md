# Sequence decoder prototype (Python), 2026-10-26

Decodes CW by finding the most likely whole keying sequence (dit, dah, inside gap,
letter gap, word gap, each with learned durations) that explains a narrow lane's
envelope - a semi-Markov Viterbi decoder - instead of deciding each mark on its own.

Files
- cwhmm.py   envelope (mix to pitch, 4th-order Butterworth low-pass, dB every 5 ms),
             the decoder, the Morse table, text output
- cwhmm2.py  the self-learning version: three passes, each pass re-learns the sender's
             dit, dah, gap medians and the on/off levels from its own decode
- synth.py   generates keyed test audio (raised-cosine 5 ms edges, SNR in the 520 Hz
             passband, optional 12 dB QSB, dah ratio, jitter)

Run
  python3 cwhmm2.py <wav> <pitch Hz> <t0 s> <t1 s> <lane bw Hz> <dit guess ms>
  e.g. python3 cwhmm2.py cw-2026-10-19-141708.wav 500 120 180 10 100

Results measured on 2026-10-26 (see the chat):
- synthetic 18 WPM CQ+ragchew, lane 10 Hz: 10 dB whole, 5 dB whole, 0 dB whole but one
  letter; 12 dB QSB at 0.7 Hz at 10 and 5 dB: whole.
- 12 WPM fist, dah 1.7 x dit, 15% jitter, 20 dB, lane 30 Hz: ~52 of 60.
- cw-2026-10-19-141708 at 500 Hz, 120-300 s (KB2UZK/M): GM KEN NICE TO MEET ... TNX FER
  RPRT UR 579 ... I AM IN MOBILE STN ... NAME BRYAN BRYAN ES QTH IS CHICOPEE, MA ...
  PARKED IN MY MOBILE <BT> ... KEN N3AFS DE KB2UZK/M K
- same file 0-60 s at 500 Hz (N3AFS): ... 589 ... QTH IS NEW KENSINGTON, PA  (Ken is
  at 500 Hz, zero-beat with his partner; the 437 Hz signal is a third station)
- where nothing is: the decoder prints dit-only junk; a real station shows dah share
  0.34-0.42 and dah-cluster tightness 0.07-0.09, nothing shows 0.09 and 0.12-0.19 -
  the "someone there" test is a pattern test, not a level.

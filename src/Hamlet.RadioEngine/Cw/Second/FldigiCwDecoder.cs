// ----------------------------------------------------------------------------
// Ported to C# for Hamlet from fldigi - receive path only (HM-REQ-122).
// Upstream:      https://github.com/w1hkj/fldigi
// Commit:        61b97f4133c488063f3de1795c894d22d5032e8a
// Upstream files: src/cw_rtty/cw.cxx and src/include/cw.h
//
// The receive path from cw::rx_process down, and nothing else of fldigi. No
// transmit, keying, QSK, sync-scope, waterfall or status-bar code is ported.
// Where fldigi takes the carrier frequency from the waterfall cursor, the
// caller gives it here. Every departure forced by C++ to C# is listed in
// work instruction 456's report with its upstream line.
//
// The upstream header of cw.cxx follows, verbatim.
// ----------------------------------------------------------------------------
// cw.cxx  --  morse code modem
//
// Copyright (C) 2006-2010
//		Dave Freese, W1HKJ
//		   (C) Mauri Niininen, AG1LE
//
// This file is part of fldigi.  Adapted from code contained in gmfsk source code
// distribution.
//  gmfsk Copyright (C) 2001, 2002, 2003
//  Tomi Manninen (oh2bns@sral.fi)
//  Copyright (C) 2004
//  Lawrence Glaister (ve7it@shaw.ca)
//
// Fldigi is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Fldigi is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with fldigi.  If not, see <http://www.gnu.org/licenses/>.
// ----------------------------------------------------------------------------

using System.Numerics;
using System.Text;

namespace Hamlet.RadioEngine.Cw.Second;

/// <summary>
/// One string fldigi's receiver hands to <c>put_rx_char</c>, with the
/// receiver's own state at that moment.
/// </summary>
/// <param name="Text">What fldigi prints: a character, a prosign such as <c>&lt;BT&gt;</c>, a space, or its <c>CW_noise</c> character.</param>
/// <param name="InputSample">How many 8000 Hz samples had been given to the decoder when it was printed.</param>
/// <param name="ReceiveSpeed"><c>cw_receive_speed</c>, words a minute.</param>
/// <param name="TwoDots"><c>two_dots</c>, the dot/dash split, in samples.</param>
/// <param name="Upper"><c>progdefaults.CWupper</c>, the key-down threshold.</param>
/// <param name="Lower"><c>progdefaults.CWlower</c>, the key-up threshold.</param>
/// <param name="Representation">The dots and dashes looked up, or empty for a word space.</param>
public sealed record FldigiCwEmission(
    string Text,
    long InputSample,
    int ReceiveSpeed,
    long TwoDots,
    double Upper,
    double Lower,
    string Representation);

/// <summary>
/// fldigi's CW receive modem, ported as the second decoder (HM-REQ-122,
/// R84). Samples in at 8000 Hz, characters out, carrier frequency given by
/// the caller. Left as ported (HM-REQ-129).
/// </summary>
/// <remarks>
/// <para>ITS OWN OUTPUT, UNCLASSED. fldigi has no confidence; this emits what
/// fldigi prints, its <c>CW_noise</c> character included, and nothing maps it
/// to sure, dim or placeholder here. That is 9.2's.</para>
/// <para>Reachable from tests only; wired to nothing the operator sees
/// (HM-REQ-121, criterion 9.6).</para>
/// </remarks>
public sealed class FldigiCwDecoder
{
    // cw.h:41
    /// <summary>fldigi's CW modem sample rate (cw.h:41).</summary>
    public const int CW_SAMPLERATE = 8000;

    // cw.h:45, 48-49, 55, 70, 90; cw.cxx:81
    private const int MAX_MORSE_ELEMENTS = 6;
    private const int CW_SUCCESS = 0;
    private const int CW_ERROR = -1;
    private const int KWPM = 12 * CW_SAMPLERATE / 10;
    private const int TRACKING_FILTER_SIZE = 16;
    private const int DEC_RATIO = 16;
    private const int CW_FFT_SIZE = 2048;

    // modem.h:45
    private const double TWOPI = 2.0 * Math.PI;

    // cw.h:74
    private enum CW_RX_STATE
    {
        RS_IDLE = 0,
        RS_IN_TONE,
        RS_AFTER_TONE,
    }

    // cw.h:80
    private enum CW_EVENT
    {
        CW_RESET_EVENT,
        CW_KEYDOWN_EVENT,
        CW_KEYUP_EVENT,
        CW_QUERY_EVENT,
    }

    private readonly FldigiProgdefaults progdefaults;

    // progStatus.sqlonoff and progStatus.sldrSquelchValue (status.h:93, 88).
    private readonly bool sqlonoff;
    private readonly double sldrSquelchValue;

    // modem members (modem.h): frequency is static upstream, one per decoder here.
    private readonly double frequency;
    private readonly int samplerate;
    private double bandwidth;
    private bool cwTrack;
    private double metric;
    private readonly FldigiMorse morse;

    // cw members (cw.h), receive side. Some are written and never read by the
    // receive path, upstream as here; they are kept so the state is fldigi's.
#pragma warning disable CS0414
    private int symbollen;
    private double FFTphase;
    private double FFTvalue;
    private uint smpl_ctr;
    private double agc_peak;
    private bool use_matched_filter;
    private double upper_threshold;
    private double lower_threshold;
    private readonly FldigiFftFilter cw_FFT_filter;
    private readonly FldigiMovingAverage bitfilter;
    private readonly FldigiMovingAverage trackingfilter;
    private CW_RX_STATE cw_receive_state;
    private CW_RX_STATE old_cw_receive_state;
    private int cw_speed;
    private int cw_send_speed;
    private int cw_receive_speed;
    private int cw_upper_limit;
    private int cw_lower_limit;
    private long cw_noise_spike_threshold;
    private long cw_send_dot_length;
    private long cw_send_dash_length;
    private long cw_receive_dot_length;
    private long cw_receive_dash_length;
    private readonly StringBuilder rx_rep_buf = new();
    private int cw_rr_current;
    private uint cw_rr_start_timestamp;
    private uint cw_rr_end_timestamp;
    private long two_dots;
    private readonly float[] cw_buffer = new float[512];
    private int cw_ptr;
    private double lowerwpm;
    private double upperwpm;
    private double noise_floor;
    private double sig_avg;
    private double siglevel;
    private bool use_paren;
    private string prosigns;
#pragma warning restore CS0414

    // Upstream `static bool cwprocessing` (cw.cxx:717), one per decoder here.
    private bool cwprocessing;

    // Upstream function statics of handle_event (cw.cxx:773-774), one per decoder here.
    private int space_sent = 1;
    private int last_element = 0;

    // Not fldigi's: what it printed, and how many samples it had been given.
    private readonly StringBuilder printed = new();
    private readonly List<FldigiCwEmission> emissions = new();
    private long input_samples;
    private string last_representation = string.Empty;

    /// <summary>
    /// Ports <c>cw::cw</c> (cw.cxx:299) and then <c>cw::init</c> (cw.cxx:256),
    /// receive side, with the carrier frequency given in place of the waterfall's.
    /// </summary>
    /// <param name="frequency">The carrier's audio frequency in hertz, as fldigi's waterfall cursor would give it.</param>
    /// <param name="progdefaults">Settings; null for fldigi's shipped defaults at 61b97f41.</param>
    /// <param name="sqlonoff">
    /// <c>progStatus.sqlonoff</c>. Its shipped default is in src/misc/status.cxx,
    /// which is not in the owner's partial clone; off, which leaves fldigi's
    /// detector as the only gate.
    /// </param>
    /// <param name="sldrSquelchValue"><c>progStatus.sldrSquelchValue</c>, read only with the squelch on.</param>
    public FldigiCwDecoder(
        double frequency,
        FldigiProgdefaults? progdefaults = null,
        bool sqlonoff = false,
        double sldrSquelchValue = 0)
    {
        this.progdefaults = progdefaults ?? new FldigiProgdefaults();
        this.sqlonoff = sqlonoff;
        this.sldrSquelchValue = sldrSquelchValue;

        if (this.progdefaults.CWuseSOMdecoding)
        {
            throw new NotSupportedException("fldigi's SOM decoding mode is not ported (work instruction 456, task 1).");
        }

        // modem::modem, not in the tree: the table, and metric starting at zero.
        morse = new FldigiMorse(this.progdefaults);
        metric = 0.0;

        // cw::cw (cw.cxx:299), receive side.
        this.frequency = frequency;

        samplerate = CW_SAMPLERATE;

        cw_speed = (int)this.progdefaults.CWspeed;
        bandwidth = this.progdefaults.CWbandwidth;

        cw_send_speed = cw_speed;
        cw_receive_speed = cw_speed;
        two_dots = 2 * KWPM / cw_speed;
        cw_noise_spike_threshold = two_dots / 4;
        cw_send_dot_length = KWPM / cw_send_speed;
        cw_send_dash_length = 3 * cw_send_dot_length;
        symbollen = (int)Math.Round(samplerate * 1.2 / this.progdefaults.CWspeed, MidpointRounding.AwayFromZero);

        rx_rep_buf.Clear();

        cwTrack = true;
        FFTphase = 0.0;
        FFTvalue = 0.0;

        upper_threshold = this.progdefaults.CWupper;
        lower_threshold = this.progdefaults.CWlower;

        agc_peak = 1.0;

        use_matched_filter = this.progdefaults.CWmfilt;

        bandwidth = this.progdefaults.CWbandwidth;
        if (use_matched_filter)
        {
            bandwidth = 5.0 * this.progdefaults.CWspeed / 1.2;
            this.progdefaults.CWbandwidth = (int)bandwidth;
        }

        cw_FFT_filter = new FldigiFftFilter(1.0 * this.progdefaults.CWbandwidth / samplerate, CW_FFT_SIZE);

        int bfv = symbollen / (2 * DEC_RATIO);
        if (bfv < 1) bfv = 1;

        bitfilter = new FldigiMovingAverage(bfv);

        trackingfilter = new FldigiMovingAverage(TRACKING_FILTER_SIZE);

        sync_parameters();

        noise_floor = 1.0;
        sig_avg = 0.0;

        // cw::init (cw.cxx:256), receive side.
        trackingfilter.reset();
        two_dots = (long)trackingfilter.run(2 * cw_send_dot_length);

        morse.init();
        use_paren = this.progdefaults.CW_use_paren;
        prosigns = this.progdefaults.CW_prosigns;

        rx_init();
    }

    /// <summary>The carrier frequency the caller gave, in hertz.</summary>
    public double Frequency => frequency;

    /// <summary>Everything fldigi would have printed so far, in order.</summary>
    public string Text => printed.ToString();

    /// <summary>Each string printed, with the receiver's speed and thresholds at that moment.</summary>
    public IReadOnlyList<FldigiCwEmission> Emissions => emissions;

    /// <summary><c>cw_receive_speed</c> now, words a minute.</summary>
    public int ReceiveSpeed => cw_receive_speed;

    /// <summary>Ports <c>cw::rx_init</c> (cw.cxx:240), receive side.</summary>
    public void rx_init()
    {
        cw_receive_state = CW_RX_STATE.RS_IDLE;
        smpl_ctr = 0;
        cw_rr_current = 0;
        cw_ptr = 0;
        agc_peak = 0;
    }

    /// <summary>Ports <c>cw::rx_process</c> (cw.cxx:719): one block of 8000 Hz audio.</summary>
    /// <param name="buf">The samples, full scale plus and minus one.</param>
    /// <returns>Zero, as upstream.</returns>
    public int rx_process(ReadOnlySpan<double> buf)
    {
        if (use_paren != progdefaults.CW_use_paren ||
            prosigns != progdefaults.CW_prosigns)
        {
            use_paren = progdefaults.CW_use_paren;
            prosigns = progdefaults.CW_prosigns;
            morse.init();
        }

        if (cwprocessing)
            return 0;

        cwprocessing = true;

        reset_rx_filter();

        rx_FFTprocess(buf);

        cwprocessing = false;

        return 0;
    }

    // Ports cw::reset_rx_filter (cw.cxx:386), receive side.
    private void reset_rx_filter()
    {
        if (use_matched_filter != progdefaults.CWmfilt ||
            cw_speed != progdefaults.CWspeed ||
            (bandwidth != progdefaults.CWbandwidth && !use_matched_filter))
        {
            use_matched_filter = progdefaults.CWmfilt;
            cw_send_speed = cw_speed = (int)progdefaults.CWspeed;

            if (use_matched_filter)
            {
                bandwidth = 5.0 * progdefaults.CWspeed / 1.2;
                progdefaults.CWbandwidth = (int)bandwidth;
            }
            else
            {
                bandwidth = progdefaults.CWbandwidth;
            }

            cw_FFT_filter.create_lpf(1.0 * bandwidth / samplerate);
            FFTphase = 0;

            two_dots = 2 * KWPM / cw_speed;
            cw_noise_spike_threshold = two_dots / 4;
            cw_send_dot_length = KWPM / cw_send_speed;
            cw_send_dash_length = 3 * cw_send_dot_length;
            symbollen = (int)Math.Round(samplerate * 1.2 / progdefaults.CWspeed, MidpointRounding.AwayFromZero);

            FFTphase = 0.0;
            FFTvalue = 0.0;
            smpl_ctr = 0;

            rx_rep_buf.Clear();

            int bfv = symbollen / (2 * DEC_RATIO);
            if (bfv < 1) bfv = 1;

            bitfilter.setLength(bfv);

            siglevel = 0;
        }
    }

    // Ports cw::sync_transmit_parameters (cw.cxx:443), the lengths the receiver reads.
    private void sync_transmit_parameters()
    {
        cw_send_dot_length = (long)(KWPM / progdefaults.CWspeed);
        cw_send_dash_length = 3 * cw_send_dot_length;

        int nusymbollen = (int)Math.Round(samplerate * 1.2 / progdefaults.CWspeed, MidpointRounding.AwayFromZero);

        if (symbollen != nusymbollen)
        {
            symbollen = nusymbollen;
        }
    }

    // Ports cw::sync_parameters (cw.cxx:466), receive side.
    private void sync_parameters()
    {
        sync_transmit_parameters();

        // check if user changed the tracking or the cw default speed
        if ((cwTrack != progdefaults.CWtrack) ||
            (cw_send_speed != progdefaults.CWspeed))
        {
            trackingfilter.reset();
            two_dots = 2 * cw_send_dot_length;
        }
        cwTrack = progdefaults.CWtrack;
        cw_send_speed = (int)progdefaults.CWspeed;

        // Receive parameters:
        lowerwpm = cw_send_speed - progdefaults.CWrange;
        upperwpm = cw_send_speed + progdefaults.CWrange;
        if (lowerwpm < progdefaults.CWlowerlimit)
            lowerwpm = progdefaults.CWlowerlimit;
        if (upperwpm > progdefaults.CWupperlimit)
            upperwpm = progdefaults.CWupperlimit;
        cw_lower_limit = (int)(2 * KWPM / upperwpm);
        cw_upper_limit = (int)(2 * KWPM / lowerwpm);

        if (cwTrack)
        {
            cw_receive_speed = (int)(KWPM / (two_dots / 2));
        }
        else
        {
            cw_receive_speed = cw_send_speed;
            two_dots = 2 * cw_send_dot_length;
        }

        if (cw_receive_speed > 0)
            cw_receive_dot_length = KWPM / cw_receive_speed;
        else
            cw_receive_dot_length = KWPM / 5;

        cw_receive_dash_length = 3 * cw_receive_dot_length;

        cw_noise_spike_threshold = cw_receive_dot_length / 2;
    }

    // Ports cw::update_tracking (cw.cxx:524).
    private void update_tracking(int dur_1, int dur_2)
    {
        const int min_dot = KWPM / 200;
        const int max_dash = 3 * KWPM / 5;
        if ((dur_1 > dur_2) && (dur_1 > 4 * dur_2)) return;
        if ((dur_2 > dur_1) && (dur_2 > 4 * dur_1)) return;
        if (dur_1 < min_dot || dur_2 < min_dot) return;
        if (dur_2 > max_dash || dur_2 > max_dash) return;

        two_dots = (long)trackingfilter.run((dur_1 + dur_2) / 2);

        sync_parameters();
    }

    // Ports cw::decode_stream (cw.cxx:593), receive side; put_rx_char becomes Print.
    private void decode_stream(double value)
    {
        string sc = string.Empty;
        int attack = 0;
        int decay = 0;
        switch (progdefaults.cwrx_attack)
        {
            case 0: attack = 400; break;
            case 1: default: attack = 200; break;
            case 2: attack = 100; break;
        }
        switch (progdefaults.cwrx_decay)
        {
            case 0: decay = 2000; break;
            case 1: default: decay = 1000; break;
            case 2: decay = 500; break;
        }

        sig_avg = FldigiMisc.decayavg(sig_avg, value, decay);

        if (value < sig_avg)
        {
            if (value < noise_floor)
                noise_floor = FldigiMisc.decayavg(noise_floor, value, attack);
            else
                noise_floor = FldigiMisc.decayavg(noise_floor, value, decay);
        }
        if (value > sig_avg)
        {
            if (value > agc_peak)
                agc_peak = FldigiMisc.decayavg(agc_peak, value, attack);
            else
                agc_peak = FldigiMisc.decayavg(agc_peak, value, decay);
        }

        float norm_noise = (float)(noise_floor / agc_peak);
        float norm_sig = (float)(sig_avg / agc_peak);
        siglevel = norm_sig;

        if (agc_peak != 0)
            value /= agc_peak;
        else
            value = 0;

        metric = 0.8 * metric;
        if ((noise_floor > 1e-4) && (noise_floor < sig_avg))
            metric += 0.2 * FldigiMisc.clamp(2.5 * (20 * Math.Log10(sig_avg / noise_floor)), 0, 100);

        float diff = norm_sig - norm_noise;

        progdefaults.CWupper = norm_sig - (0.2 * diff);
        progdefaults.CWlower = norm_noise + (0.7 * diff);

        if (!sqlonoff || metric > sldrSquelchValue)
        {
            // Power detection using hysterisis detector
            // upward trend means tone starting
            if ((value > progdefaults.CWupper) && (cw_receive_state != CW_RX_STATE.RS_IN_TONE))
            {
                handle_event(CW_EVENT.CW_KEYDOWN_EVENT, ref sc);
            }
            // downward trend means tone stopping
            if ((value < progdefaults.CWlower) && (cw_receive_state == CW_RX_STATE.RS_IN_TONE))
            {
                handle_event(CW_EVENT.CW_KEYUP_EVENT, ref sc);
            }
        }

        if (handle_event(CW_EVENT.CW_QUERY_EVENT, ref sc) == CW_SUCCESS)
        {
            // put_rx_char(sc[n], ...) for each character of sc (cw.cxx:671-674).
            if (sc.Length > 0)
                Print(sc);
        }
    }

    // Ports cw::rx_FFTprocess (cw.cxx:683).
    private void rx_FFTprocess(ReadOnlySpan<double> buf)
    {
        Complex z;
        Complex[] zp;
        int n;
        int k = 0;
        int len = buf.Length;

        while (len-- > 0)
        {
            z = new Complex(buf[k] * Math.Cos(FFTphase), buf[k] * Math.Sin(FFTphase));
            FFTphase += TWOPI * frequency / samplerate;
            if (FFTphase > TWOPI) FFTphase -= TWOPI;

            k++;
            input_samples++;

            n = cw_FFT_filter.run(z, out zp); // n = 0 or filterlen/2

            if (n == 0) continue;

            for (int i = 0; i < n; i++)
            {
                // update the basic sample counter used for morse timing
                ++smpl_ctr;

                if (smpl_ctr % DEC_RATIO != 0) continue; // decimate by DEC_RATIO

                // demodulate
                FFTvalue = Complex.Abs(zp[i]);
                FFTvalue = bitfilter.run(FFTvalue);

                decode_stream(FFTvalue);
            }
        }
    }

    // Ports cw::usec_diff (cw.cxx:754).
    private static int usec_diff(uint earlier, uint later)
    {
        return (earlier >= later) ? 0 : (int)(later - earlier);
    }

    // Ports cw::handle_event (cw.cxx:771).
    private int handle_event(CW_EVENT cw_event, ref string sc)
    {
        int element_usec; // Time difference in usecs

        switch (cw_event)
        {
            case CW_EVENT.CW_RESET_EVENT:
                sync_parameters();
                cw_receive_state = CW_RX_STATE.RS_IDLE;
                cw_rr_current = 0; // reset decoding pointer
                cw_ptr = 0;
                Array.Clear(cw_buffer);
                smpl_ctr = 0; // reset audio sample counter
                rx_rep_buf.Clear();
                break;
            case CW_EVENT.CW_KEYDOWN_EVENT:
                // A receive tone start can only happen while we
                // are idle, or in the middle of a character.
                if (cw_receive_state == CW_RX_STATE.RS_IN_TONE)
                    return CW_ERROR;
                // first tone in idle state reset audio sample counter
                if (cw_receive_state == CW_RX_STATE.RS_IDLE)
                {
                    smpl_ctr = 0;
                    rx_rep_buf.Clear();
                    cw_rr_current = 0;
                    cw_ptr = 0;
                }
                // save the timestamp
                cw_rr_start_timestamp = smpl_ctr;
                // Set state to indicate we are inside a tone.
                old_cw_receive_state = cw_receive_state;
                cw_receive_state = CW_RX_STATE.RS_IN_TONE;
                return CW_ERROR;
            case CW_EVENT.CW_KEYUP_EVENT:
                // The receive state is expected to be inside a tone.
                if (cw_receive_state != CW_RX_STATE.RS_IN_TONE)
                    return CW_ERROR;
                // Save the current timestamp
                cw_rr_end_timestamp = smpl_ctr;
                element_usec = usec_diff(cw_rr_start_timestamp, cw_rr_end_timestamp);

                // make sure our timing values are up to date
                sync_parameters();
                // If the tone length is shorter than any noise cancelling
                // threshold that has been set, then ignore this tone.
                if (cw_noise_spike_threshold > 0
                    && element_usec < cw_noise_spike_threshold)
                {
                    cw_receive_state = CW_RX_STATE.RS_IDLE;
                    return CW_ERROR;
                }

                // Set up to track speed on dot-dash or dash-dot pairs (Lawrence Glaister, ve7it).
                if (last_element > 0)
                {
                    // check for dot dash sequence (current should be 3 x last)
                    if ((element_usec > 2 * last_element) &&
                        (element_usec < 4 * last_element))
                    {
                        update_tracking(last_element, element_usec);
                    }
                    // check for dash dot sequence (last should be 3 x current)
                    if ((last_element > 2 * element_usec) &&
                        (last_element < 4 * element_usec))
                    {
                        update_tracking(element_usec, last_element);
                    }
                }
                last_element = element_usec;
                // ok... do we have a dit or a dah?
                // a dot is anything shorter than 2 dot times
                if (element_usec <= two_dots)
                {
                    rx_rep_buf.Append(FldigiMorse.CW_DOT_REPRESENTATION);
                    cw_buffer[cw_ptr++] = (float)last_element;
                }
                else
                {
                    // a dash is anything longer than 2 dot times
                    rx_rep_buf.Append(FldigiMorse.CW_DASH_REPRESENTATION);
                    cw_buffer[cw_ptr++] = (float)last_element;
                }
                // We just added a representation to the receive buffer.
                // If it's full, then reset everything as it probably noise
                if (rx_rep_buf.Length > MAX_MORSE_ELEMENTS)
                {
                    cw_receive_state = CW_RX_STATE.RS_IDLE;
                    cw_rr_current = 0; // reset decoding pointer
                    cw_ptr = 0;
                    smpl_ctr = 0; // reset audio sample counter
                    return CW_ERROR;
                }
                else
                {
                    // zero terminate representation
                    cw_buffer[cw_ptr] = 0.0f;
                }
                // All is well.  Move to the more normal after-tone state.
                cw_receive_state = CW_RX_STATE.RS_AFTER_TONE;
                return CW_ERROR;
            case CW_EVENT.CW_QUERY_EVENT:
                // this should be called quite often (faster than inter-character gap) It looks after timing
                // key up intervals and determining when a character, a word space, or an error char '*' should be returned.
                // CW_SUCCESS is returned when there is a printable character. Nothing to do if we are in a tone
                if (cw_receive_state == CW_RX_STATE.RS_IN_TONE)
                    return CW_ERROR;
                // compute length of silence so far
                sync_parameters();
                element_usec = usec_diff(cw_rr_end_timestamp, smpl_ctr);
                // SHORT time since keyup... nothing to do yet
                if (element_usec < (2 * cw_receive_dot_length))
                    return CW_ERROR;
                // MEDIUM time since keyup... check for character space
                // one shot through this code via receive state logic
                // FARNSWOTH MOD HERE -->
                if (element_usec >= (2 * cw_receive_dot_length) &&
                    element_usec <= (4 * cw_receive_dot_length) &&
                    cw_receive_state == CW_RX_STATE.RS_AFTER_TONE)
                {
                    // Look up the representation
                    last_representation = rx_rep_buf.ToString();
                    sc = morse.rx_lookup(last_representation);
                    if (sc.Length == 0)
                    {
                        // invalid decode... let user see error
                        sc = progdefaults.CW_noise == '*' ? "*" :
                              progdefaults.CW_noise == '_' ? "_" :
                              progdefaults.CW_noise == ' ' ? " " : string.Empty;
                    }
                    rx_rep_buf.Clear();
                    cw_receive_state = CW_RX_STATE.RS_IDLE;
                    cw_rr_current = 0; // reset decoding pointer
                    space_sent = 0;
                    cw_ptr = 0;

                    return CW_SUCCESS;
                }
                // LONG time since keyup... check for a word space
                // FARNSWOTH MOD HERE -->
                if ((element_usec > (4 * cw_receive_dot_length)) && space_sent == 0)
                {
                    last_representation = string.Empty;
                    sc = " ";
                    space_sent = 1;
                    return CW_SUCCESS;
                }
                // should never get here... catch all
                return CW_ERROR;
        }
        // should never get here... catch all
        return CW_ERROR;
    }

    // Not fldigi's: stands where decode_stream calls put_rx_char (cw.cxx:671-674).
    private void Print(string sc)
    {
        printed.Append(sc);
        emissions.Add(new FldigiCwEmission(
            sc, input_samples, cw_receive_speed, two_dots,
            progdefaults.CWupper, progdefaults.CWlower, last_representation));
    }
}

namespace Hamlet.RadioEngine.Audio;

/// <summary>One capture device the operator could listen to.</summary>
/// <param name="Id">Stable identifier, which is what gets stored in settings.</param>
/// <param name="Name">What the operating system calls it.</param>
/// <param name="IsDefault">True when this is the system's default capture device.</param>
public sealed record AudioDevice(string Id, string Name, bool IsDefault = false)
{
    /// <summary>
    /// True when the name looks like the IC-7300's own USB codec.
    /// </summary>
    /// <remarks>
    /// Derived rather than stored, so nothing can claim to be the radio by
    /// having a flag set on it. See <see cref="LooksLikeRadioCodec"/> for what
    /// this is matching and why it is only ever a suggestion.
    /// </remarks>
    public bool LooksLikeRadio => LooksLikeRadioCodec(Name);

    /// <summary>
    /// Whether a device name looks like the radio's USB audio codec.
    /// </summary>
    /// <param name="name">The device name as the operating system reports it.</param>
    /// <returns>True when it matches the codec's documented name.</returns>
    /// <remarks>
    /// <para>CLAUDE.md §4 records what the IC-7300 presents: a standard
    /// Windows audio device called "USB Audio CODEC". That is a generic
    /// USB-audio class name rather than anything Icom stamped on it, so other
    /// hardware can carry it too.</para>
    /// <para>WHICH IS WHY THIS ONLY EVER PRESELECTS. It picks the likely device
    /// so that somebody who has never opened a sound settings dialog is not
    /// stopped at the first step, and the operator can always choose a
    /// different one. Hamlet does not claim the device it picked is the radio,
    /// because a name is not proof and §0.0 does not stop applying because the
    /// subject is a sound card.</para>
    /// </remarks>
    public static bool LooksLikeRadioCodec(string? name)
        => name is not null
           && name.Contains("USB Audio CODEC", StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Lists the capture devices available on this machine.
/// </summary>
/// <remarks>
/// A seam of its own, separate from <see cref="IAudioSource"/>, because
/// enumerating devices and capturing from one are different jobs with
/// different failure modes. A machine with no sound card at all must list
/// nothing and carry on, not throw on startup.
/// </remarks>
public interface IAudioDevices
{
    /// <summary>Every capture device, or an empty list when there are none.</summary>
    /// <returns>The devices. Never null, never throws.</returns>
    IReadOnlyList<AudioDevice> List();
}

/// <summary>
/// Which of the five rules picked the capture device (unit 236).
/// </summary>
/// <remarks>
/// <para>**THE CHOICE WAS MADE FOR THE OPERATOR AND RECORDED NOWHERE.** Unit 235
/// measured that <c>AudioInputDeviceId</c> is not present in his settings at all,
/// so <see cref="AudioDeviceChoice.Choose"/> has run with a null remembered id on
/// every launch this program has ever had. If the radio is off, its USB cable is
/// out, or Windows has renamed the codec, the fall-through quietly hands Hamlet
/// the machine's default input — and everything downstream works perfectly, on
/// room noise, forever.</para>
/// <para>**IT IS THE BRANCH AND IT IS NEVER THE DEVICE** (HM-DEC-018). A device
/// name can carry a computer's name, a person's name or the model of somebody's
/// headset, and none of that belongs in a file the operator might paste into a
/// public issue. The shell's existing <c>AppEvents.AudioDeviceChosen</c> already
/// records a boolean for the same reason, and this follows it.</para>
/// </remarks>
public enum AudioDeviceChoiceReason
{
    /// <summary>The machine offered no capture devices, so nothing was chosen.</summary>
    NothingToChooseFrom,

    /// <summary>
    /// The device the operator chose last time, still plugged in. **The only one
    /// of the five that is his own decision** rather than Hamlet's guess.
    /// </summary>
    OperatorsRemembered,

    /// <summary>
    /// A device whose name matches the radio's USB codec. A suggestion and not a
    /// proof — see <see cref="AudioDevice.LooksLikeRadioCodec"/>.
    /// </summary>
    LooksLikeRadio,

    /// <summary>Whatever Windows calls the default capture device.</summary>
    SystemDefault,

    /// <summary>
    /// The first device in the list, because none of the rules above matched.
    /// </summary>
    FirstInTheList,
}

/// <summary>
/// A chosen capture device and which rule chose it (unit 236).
/// </summary>
/// <param name="Device">The device to open, or null where there was none.</param>
/// <param name="Reason">Which of the five rules picked it.</param>
public readonly record struct AudioDeviceChoiceResult(
    AudioDevice? Device,
    AudioDeviceChoiceReason Reason);

/// <summary>
/// Picks which capture device to listen to, given what is available and what
/// the operator chose last time.
/// </summary>
/// <remarks>
/// Pure and testable on purpose. Device selection is exactly the kind of
/// startup logic that is hard to reproduce by hand and easy to get wrong when
/// somebody unplugs a cable, so the rules live here where a test can hold them
/// still rather than inside the WASAPI implementation where they could only be
/// checked by owning the hardware.
/// </remarks>
public static class AudioDeviceChoice
{
    /// <summary>
    /// The device to open.
    /// </summary>
    /// <param name="devices">What the machine offers.</param>
    /// <param name="rememberedId">The id stored in settings, or null.</param>
    /// <returns>The chosen device, or null when there is nothing to choose.</returns>
    /// <remarks>
    /// The remembered device wins whenever it is still plugged in, because the
    /// operator's own choice outranks any guess Hamlet makes. After that the
    /// radio's codec wins, then the system default, then whatever is first. A
    /// remembered device that has vanished falls through quietly rather than
    /// failing: somebody who unplugged the radio still wants the app to open.
    /// </remarks>
    public static AudioDevice? Choose(
        IReadOnlyList<AudioDevice> devices, string? rememberedId)
        => ChooseWithReason(devices, rememberedId).Device;

    /// <summary>
    /// The device to open, and which of the five rules picked it (unit 236).
    /// </summary>
    /// <param name="devices">What the machine offers.</param>
    /// <param name="rememberedId">The id stored in settings, or null.</param>
    /// <returns>The chosen device and the branch that chose it.</returns>
    /// <exception cref="ArgumentNullException">The device list is null.</exception>
    /// <remarks>
    /// <para>**THE RULES ARE UNCHANGED AND THIS IS WHERE THEY NOW LIVE.**
    /// <see cref="Choose"/> is this method with the reason dropped, so every
    /// existing caller keeps its behaviour exactly and there is one copy of the
    /// order rather than two that could drift.</para>
    /// <para>**WHY THE REASON EXISTS AT ALL.** On 2026-09-03 the owner sat at
    /// 14.074 and saw nothing on screen, and no file anywhere said which sound
    /// card Hamlet had opened or why. Four of these five branches are Hamlet
    /// guessing; only <see cref="AudioDeviceChoiceReason.OperatorsRemembered"/> is
    /// his own decision, and unit 235 measured that his settings have never held
    /// one.</para>
    /// </remarks>
    public static AudioDeviceChoiceResult ChooseWithReason(
        IReadOnlyList<AudioDevice> devices, string? rememberedId)
    {
        ArgumentNullException.ThrowIfNull(devices);

        if (devices.Count == 0)
        {
            return new AudioDeviceChoiceResult(
                null, AudioDeviceChoiceReason.NothingToChooseFrom);
        }

        if (!string.IsNullOrWhiteSpace(rememberedId))
        {
            var remembered = devices.FirstOrDefault(
                d => string.Equals(d.Id, rememberedId, StringComparison.Ordinal));

            if (remembered is not null)
            {
                return new AudioDeviceChoiceResult(
                    remembered, AudioDeviceChoiceReason.OperatorsRemembered);
            }
        }

        if (devices.FirstOrDefault(d => d.LooksLikeRadio) is { } codec)
        {
            return new AudioDeviceChoiceResult(
                codec, AudioDeviceChoiceReason.LooksLikeRadio);
        }

        if (devices.FirstOrDefault(d => d.IsDefault) is { } fallback)
        {
            return new AudioDeviceChoiceResult(
                fallback, AudioDeviceChoiceReason.SystemDefault);
        }

        return new AudioDeviceChoiceResult(
            devices[0], AudioDeviceChoiceReason.FirstInTheList);
    }
}

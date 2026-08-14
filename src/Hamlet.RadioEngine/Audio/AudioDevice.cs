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
    {
        if (devices.Count == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(rememberedId))
        {
            var remembered = devices.FirstOrDefault(
                d => string.Equals(d.Id, rememberedId, StringComparison.Ordinal));

            if (remembered is not null)
            {
                return remembered;
            }
        }

        return devices.FirstOrDefault(d => d.LooksLikeRadio)
               ?? devices.FirstOrDefault(d => d.IsDefault)
               ?? devices[0];
    }
}

namespace Altzone.Scripts.Service.Audio
{
    /// <summary>
    /// AudioManager service.
    /// </summary>
    public interface IAudioManager
    {
        float MasterVolume { get; set; }
        float MenuEffectsVolume { get; set; }
        float GameEffectsVolume { get; set; }
        float GameMusicVolume { get; set; }
    }
}
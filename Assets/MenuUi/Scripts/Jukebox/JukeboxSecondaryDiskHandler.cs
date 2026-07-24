using Altzone.Scripts.Audio;

namespace MenuUI.Scripts.Jukebox
{
    public class JukeboxSecondaryDiskHandler : JukeboxDiskBase
    {
        private void OnEnable()
        {
            JukeboxManager.Instance.OnPreviewStart += StartDiskSpin;
            JukeboxManager.Instance.OnPreviewEnd += StopDiskSpin;
        }

        private void OnDisable()
        {
            JukeboxManager.Instance.OnPreviewStart -= StartDiskSpin;
            JukeboxManager.Instance.OnPreviewEnd -= StopDiskSpin;

            StopDiskSpin();
        }
    }
}

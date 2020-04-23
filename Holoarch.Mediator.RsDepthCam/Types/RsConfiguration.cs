using Intel.RealSense;

namespace Holoarch.Mediator.RsDepthCam
{
    public struct RsConfiguration
    {
        public enum Mode
        {
            Live, Playback, Record
        }

        public Mode PlayMode;
        public RsVideoStreamRequest[] Profiles;
        public string RequestedSerialNumber;
        public string PlaybackFile;
        public string RecordPath;

        public Config ToPipelineConfig()
        {
            Config cfg = new Config();

            switch (PlayMode)
            {
                case Mode.Live:
                    cfg.EnableDevice(RequestedSerialNumber);
                    foreach (var p in Profiles)
                        cfg.EnableStream(p.Stream, p.StreamIndex, p.Width, p.Height, p.Format, p.Framerate);
                    break;

                case Mode.Playback:
                    if (string.IsNullOrEmpty(PlaybackFile))
                    {
                        PlayMode = Mode.Live;
                    }
                    else
                    {
                        cfg.EnableDeviceFromFile(PlaybackFile);
                    }

                    break;

                case Mode.Record:
                    foreach (var p in Profiles)
                    {
                        cfg.EnableStream(p.Stream, p.StreamIndex, p.Width, p.Height, p.Format, p.Framerate);
                    }

                    if (!string.IsNullOrEmpty(RecordPath))
                    {
                        cfg.EnableRecordToFile(RecordPath);
                    }

                    break;
            }

            return cfg;
        }
    }
}
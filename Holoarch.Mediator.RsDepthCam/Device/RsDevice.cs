using System;
using Intel.RealSense;
using System.Linq;
using System.Threading;

namespace Holoarch.Mediator.RsDepthCam
{
    public class RsDevice : RsFrameProvider
    {
        private Thread worker;
        private readonly AutoResetEvent stopEvent = new AutoResetEvent(false);
        private Pipeline m_pipeline;
        private float m_DepthScale = float.NegativeInfinity;

        /// <summary>
        /// The parallelism mode of the module
        /// </summary>
        public enum ProcessMode
        {
            Multithread,
            UnityThread,
        }

        // public static RsDevice Instance { get; private set; }

        /// <summary>
        /// Threading mode of operation, Multithread or UnityThread
        /// </summary>
        public ProcessMode processMode = ProcessMode.Multithread;

        // public bool Streaming { get; private set; }

        /// <summary>
        /// Notifies upon streaming start
        /// </summary>
        public override event Action<PipelineProfile> OnStart;

        /// <summary>
        /// Notifies when streaming has stopped
        /// </summary>
        public override event Action OnStop;

        /// <summary>
        /// Fired when a new frame is available
        /// </summary>
        public override event Action<Holoarch.Mediator.Commons.Frame> OnNewSample;

        /// <summary>
        /// User configuration
        /// </summary>
        public RsConfiguration DeviceConfiguration = new RsConfiguration
        {
            PlayMode = RsConfiguration.Mode.Live,
            RequestedSerialNumber = string.Empty,
            Profiles = new RsVideoStreamRequest[] 
            {
            new RsVideoStreamRequest { Stream = Stream.Depth, StreamIndex = -1, Width = 640, Height = 480, Format = Format.Z16 ,Framerate = 30 }
            ////new RsVideoStreamRequest {Stream = Stream.Infrared, StreamIndex = -1, Width = 640, Height = 480, Format = Format.Y8 , Framerate = 30 },
            ////new RsVideoStreamRequest {Stream = Stream.Color, StreamIndex = -1, Width = 640, Height = 480, Format = Format.Rgb8 , Framerate = 30 }
            }
        };

        public void Enable()
        {
            m_pipeline = new Pipeline();

            using (var cfg = DeviceConfiguration.ToPipelineConfig())
            {
                ActiveProfile = m_pipeline.Start(cfg);
            }

            DeviceConfiguration.Profiles = ActiveProfile.Streams.Select(RsVideoStreamRequest.FromProfile).ToArray();
            m_DepthScale = ActiveProfile.Device.Sensors[0].DepthScale;

            if (processMode == ProcessMode.Multithread)
            {
                stopEvent.Reset();
                worker = new Thread(WaitForFrames);
                worker.IsBackground = true;
                worker.Start();
            }

            start();
        }

        public override void Dispose()
        {
            Disable();
            GC.SuppressFinalize(this);
        }

        public void Update()
        {
            if (!Streaming)
            {
                return;
            }
           
            if (processMode != ProcessMode.UnityThread)
            {
                return;
            }

            FrameSet frames;

            if (m_pipeline.PollForFrames(out frames))
            {
                using (frames)
                {
                    RaiseSampleEvent(frames);
                }
            }
        }

        public PipelineProfile GetActiveProfile()
        {
            return ActiveProfile;
        }

        private void start()
        {
            Streaming = true;
            OnStart?.Invoke(ActiveProfile);
        }

        private void RaiseSampleEvent(Frame frame)
        {
            var onNewSample = OnNewSample;
            if (onNewSample != null)
            {
                var genFrame = frame.RealsenseFrameToGenericFrame(m_DepthScale);
                onNewSample(genFrame);
            }
        }

        /// <summary>
        /// Worker Thread for multithreaded operations
        /// </summary>
        private void WaitForFrames()
        {
            while (!stopEvent.WaitOne(0))
            {
                using (var frames = m_pipeline.WaitForFrames())
                {
                    RaiseSampleEvent(frames);
                }
            }
        }

        private void Disable()
        {
            OnNewSample = null;

            // OnNewSampleSet = null;
            if (worker != null)
            {
                stopEvent.Set();
                worker.Join();
            }

            if (Streaming && OnStop != null)
            {
                OnStop();
            }

            if (ActiveProfile != null)
            {
                ActiveProfile.Dispose();
                ActiveProfile = null;
            }

            if (m_pipeline != null)
            {
                m_pipeline.Dispose();
                m_pipeline = null;
            }

            Streaming = false;
        }
    }
}

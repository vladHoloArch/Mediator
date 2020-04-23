using System;
using Intel.RealSense;

namespace Holoarch.Mediator.RsDepthCam
{
    public abstract class RsFrameProvider : IDisposable
    {
        public bool Streaming { get; protected set; }

        public PipelineProfile ActiveProfile { get; protected set; }

        public abstract event Action<PipelineProfile> OnStart;

        public abstract event Action OnStop;

        public abstract event Action<Holoarch.Mediator.Commons.Frame> OnNewSample;

        public abstract void Dispose();
    }
}

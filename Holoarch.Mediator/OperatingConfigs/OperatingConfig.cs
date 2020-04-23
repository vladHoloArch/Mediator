using System;
using Holoarch.Mediator.Commons;

namespace Holoarch.Mediator
{
    internal abstract class OperatingConfig
    {
        public abstract void Enable(Action<PlaneResult> onRes);

        public abstract void Disable();

        protected abstract void OnNewSample(Frame i_Frame);

        protected Action<PlaneResult> MediatorResultFunc { get; set; }
    }
}

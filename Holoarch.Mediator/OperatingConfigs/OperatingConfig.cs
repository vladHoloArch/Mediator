using Holoarch.Mediator.Commons;
using System;

namespace Holoarch.Mediator
{
    internal abstract class OperatingConfig
    {
        public abstract void Enable(Action<Result> onRes);

        public abstract void Disable();

        protected abstract void OnNewSample(Frame i_Frame);

        protected Action<Result> MediatorResultFunc { get; set; }
    }
}

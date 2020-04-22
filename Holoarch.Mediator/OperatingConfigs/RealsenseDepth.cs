using Holoarch.Mediator.Commons;
using Holoarch.Mediator.PlaneFromDepth;
using Holoarch.Mediator.RsDepthCam;
using System;

namespace Holoarch.Mediator
{
    internal class RealsenseDepth : OperatingConfig
    {
        private PlaneFitter m_PlaneFitter;

        public RsDevice Source { get; private set; }

        public override void Enable(Action<Result> onRes)
        {
            MediatorResultFunc = onRes;
            Source = new RsDevice(); 
            Source.OnNewSample += OnNewSample;
            Source.Enable();
        }

        public override void Disable()
        {
            if (m_PlaneFitter != null)
            {
                m_PlaneFitter.OnRes -= OnRes;
                m_PlaneFitter.Dispose();
            }

            if (Source != null)
            {
                Source.OnNewSample -= OnNewSample;
                Source.Dispose();
                Source = null;
            }
        }

        protected override void OnNewSample(Frame i_Frame)
        {
            //Console.WriteLine("frame recevied from device");

            if (m_PlaneFitter == null)
            {
                m_PlaneFitter = new PlaneFitter();
                m_PlaneFitter.OnRes += OnRes;
                m_PlaneFitter.init(i_Frame.DepthScale);
            }

            m_PlaneFitter.onNewDepthSample(i_Frame);
        }

        private void OnRes(Result i_Result)
        {
            MediatorResultFunc?.Invoke(i_Result);
        }
    }
}

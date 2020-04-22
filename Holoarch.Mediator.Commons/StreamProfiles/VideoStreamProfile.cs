namespace Holoarch.Mediator.Commons
{
    public class VideoStreamProfile : StreamProfile
    {
        private Intrinsics m_Intrinsics;

        public Intrinsics GetIntrinsics()
        {
            return m_Intrinsics;
        }

        public void SetIntrinsics(Intrinsics i_Intrinsics)
        {
            m_Intrinsics = i_Intrinsics;
        }
    }
}

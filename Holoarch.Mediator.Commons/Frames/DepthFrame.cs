namespace Holoarch.Mediator.Commons
{
    public class DepthFrame : Frame
    {
        public short[] Pixels { get; set; }

        public VideoStreamProfile GetProfile() 
        {
            return Profile as VideoStreamProfile;
        }

        public override void Dispose()
        {
            Pixels = null;
            base.Dispose();
        }
    }
}

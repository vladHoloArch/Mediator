namespace Holoarch.Mediator.Commons
{
    public class DepthFrame : Frame
    {
        public VideoStreamProfile GetProfile() 
        {
            return Profile as VideoStreamProfile;
        }
    }
}

using Intel.RealSense;

namespace Holoarch.Mediator.RsDepthCam
{
    public static class Extensions
    {
        public static Commons.DepthFrame RealsenseFrameToGenericFrame(this Frame i_Frame, float i_DepthScale)
        {
            Commons.DepthFrame res = new Commons.DepthFrame();
            res.Data = i_Frame.Data;
            res.DepthScale = i_DepthScale;

            var videoStreamProfile = i_Frame.GetProfile<VideoStreamProfile>();
            res.Height = videoStreamProfile.Height;
            res.Width = videoStreamProfile.Width;

            res.Pixels = new short[res.Width * res.Height];
            System.Runtime.InteropServices.Marshal.Copy(i_Frame.Data, res.Pixels, 0, res.Width * res.Height);

            Commons.VideoStreamProfile genVideoStreamProfile = new Commons.VideoStreamProfile();
            genVideoStreamProfile.Stream = (Commons.Stream)videoStreamProfile.Stream;
            genVideoStreamProfile.Format = (Commons.Format)videoStreamProfile.Format;

            var intrinsics = videoStreamProfile.GetIntrinsics();
            Commons.Intrinsics genIntrinsics = new Commons.Intrinsics()
            {
                Width = intrinsics.width,
                Height = intrinsics.height,
                Ppx = intrinsics.ppx,
                Ppy = intrinsics.ppy,
                Fx = intrinsics.fx,
                Fy = intrinsics.fy,
                Model = (Commons.Distortion)intrinsics.model,
                Coeffs = intrinsics.coeffs
            };

            genVideoStreamProfile.SetIntrinsics(genIntrinsics);

            res.IsComposite = i_Frame.IsComposite;
            res.Profile = genVideoStreamProfile;

            return res;
        }
    }
}

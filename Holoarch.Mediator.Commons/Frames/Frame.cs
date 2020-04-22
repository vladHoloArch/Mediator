using System;

namespace Holoarch.Mediator.Commons
{
    public class Frame
    {
        public float DepthScale;
        public int Width { get; set; }

        public int Height { get; set; }

        public StreamProfile Profile { get; set; }

        public IntPtr Data { get; set; }

        public bool IsComposite { get; set; }

        public StreamProfile GetProfile<T>() where T : StreamProfile
        {
            return Profile;
        }
    }
}
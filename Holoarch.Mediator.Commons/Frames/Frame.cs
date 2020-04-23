using System;

namespace Holoarch.Mediator.Commons
{
    public class Frame : IDisposable
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

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
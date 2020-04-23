using System.Runtime.InteropServices;

namespace Holoarch.Mediator.Commons
{
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Intrinsics
    {
        /// <summary> Width of the image in pixels </summary>
        public int Width;

        /// <summary> Height of the image in pixels </summary>
        public int Height;

        /// <summary> Horizontal coordinate of the principal point of the image, as a pixel offset from the left edge </summary>
        public float Ppx;

        /// <summary> Vertical coordinate of the principal point of the image, as a pixel offset from the top edge </summary>
        public float Ppy;

        /// <summary> Focal length of the image plane, as a multiple of pixel width </summary>
        public float Fx;

        /// <summary> Focal length of the image plane, as a multiple of pixel height </summary>
        public float Fy;

        /// <summary> Distortion model of the image </summary>
        public Distortion Model;

        /// <summary> Distortion coefficients, order: k1, k2, p1, p2, k3 </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public float[] Coeffs;

        public override string ToString()
        {
            return $"(width:{Width}, height:{Height}, ppx:{Ppx}, ppy:{Ppy}, fx:{Fx}, fy:{Fy}, model:{Model}, coeffs:[{Coeffs[0]}, {Coeffs[1]}, {Coeffs[2]}, {Coeffs[3]}, {Coeffs[4]}])";
        }

        /// <summary>
        /// Gets the horizontal and vertical field of view, based on video intrinsics
        /// </summary>
        /// <value>horizontal and vertical field of view in degrees</value>
        public float[] FOV
        {
            get
            {
                return new float[]
                {
                    (float)(System.Math.Atan2(Ppx + 0.5f, Fx) + System.Math.Atan2(Width - (Ppx + 0.5f), Fx)) * 57.2957795f,
                    (float)(System.Math.Atan2(Ppy + 0.5f, Fy) + System.Math.Atan2(Height - (Ppy + 0.5f), Fy)) * 57.2957795f
                };
            }
        }
    }
}

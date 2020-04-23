namespace Holoarch.Mediator.Commons
{
    public enum Distortion
    {
        /// <summary> Rectilinear images. No distortion compensation required.</summary>
        None = 0,

        /// <summary> Equivalent to Brown-Conrady distortion, except that tangential distortion is applied to radially distorted points</summary>
        ModifiedBrownConrady = 1,

        /// <summary> Equivalent to Brown-Conrady distortion, except undistorts image instead of distorting it</summary>
        InverseBrownConrady = 2,

        /// <summary> F-Theta fish-eye distortion model</summary>
        Ftheta = 3,

        /// <summary> Unmodified Brown-Conrady distortion model</summary>
        BrownConrady = 4,

        /// <summary Four parameter Kannala Brandt distortion model</summary>
        KannalaBrandt4 = 5,
    }
}

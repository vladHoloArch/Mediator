using System;
using System.Numerics;

namespace Holoarch.Mediator.PlaneFromDepth
{
    public static class Extensions
    {
        public static float GetDistanceToPoint(this Plane plane, Vector3 point)
        {
            float res = float.PositiveInfinity;
            Vector3 n = plane.Normal;

            res = Math.Abs(n.X * point.X + n.Y * point.Y + n.Z * point.Z + plane.D) /
                           n.Length();

            return res;
        }
    }
}

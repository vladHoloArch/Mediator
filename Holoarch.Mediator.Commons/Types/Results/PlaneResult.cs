using System.Numerics;

namespace Holoarch.Mediator.Commons
{
    public class PlaneResult : Result
    {
        public Plane P;
        public Vector3[] PlaneCorners;
        public Vector3 PlaneCentroid;
        public float Distance;
        public float Angle;
        public float AngleX;
        public float AngleY;
        public bool IsPlaneReconstructed = true;
    }
}

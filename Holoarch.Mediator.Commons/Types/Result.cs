using System.Numerics;

namespace Holoarch.Mediator.Commons
{
    public class Result
    {
        public Plane p;
        public Vector3[] planeCorners;
        public Vector3 planeCentroid;
        public float distance;
        public float angle;
        public float angleX;
        public float angleY;
        public bool isPlaneReconstructed = true;
    }
}

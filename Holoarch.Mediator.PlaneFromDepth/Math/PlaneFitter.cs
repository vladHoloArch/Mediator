using Holoarch.Mediator.Commons;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace Holoarch.Mediator.PlaneFromDepth
{
    public class PlaneFitter : IDisposable
    {
        #region Fields

        // Region of interest, taken from the center of the point set
        private ROI m_Roi;

        private Ransac m_Ransac;

        // Distance measuring units
        private float m_Units;

        // Frame intrinsics
        private Intrinsics m_Intrinsics;

        private FrameQueue<DepthFrame> m_Queue;

        // AnalyzeDepthImage() in a worker thread, when false terminates execution
        private bool m_IsRunning;

        // Worker thread 
        private Thread m_Thread;

        // This is the difference between 1 and the smallest floating point number of type float that is greater than 1
        private static readonly float FLT_EPSILON = 1.192092896e-07F;

        // Update the component utilizing the Result 
        ///<see cref="Result"/>
        private Action updateInstructions;

        public event Action<Result> OnRes;

        #endregion

        public void init(float i_DepthScale)
        {
            m_Units = i_DepthScale;
            m_Ransac = new Ransac();
            m_Queue = new FrameQueue<DepthFrame>(100);
            m_IsRunning = true;
            m_Thread = new Thread(AnalyzeDepthImage);
            m_Thread.Start();
        }

        public void Dispose()
        {
            m_IsRunning = false;
            GC.SuppressFinalize(this);
        } 

        public void onNewDepthSample(Frame i_Frame)
        {
            if (m_Queue == null)
                return;
            try
            {
                if (i_Frame.IsComposite)
                {
                    //using (var depthFrame = i_Frame as DepthFrame)
                    var depthFrame = i_Frame as DepthFrame;
                    m_Queue.Enqueue(depthFrame);
                    return;
                }
            }
            catch (System.Exception e)
            {

            }
        }

        #region Private Methods

        #region Operation Realted Methods


        #endregion// operation related methods

        #region Depth and Plane Calculations

        private void AnalyzeDepthImage()
        {
            while (m_IsRunning)
            {
                if (m_Queue != null)
                {
                    DepthFrame f;

                    if (m_Queue.Dequeue(out f))
                    {
                        var o_Result = new Result();
                        o_Result.isPlaneReconstructed = true;
                        var profile = f.GetProfile();
                        var streamType = profile.Stream;
                        var streamFormat = profile.Format;

                        if (streamType == Stream.Depth && streamFormat == Format.Z16)
                        {
                            m_Intrinsics = profile.GetIntrinsics();
                            m_Roi = setROI();
                            var data = f.Data;
                            int width = f.Width;
                            int height = f.Height;
                            var pixels = new short[width * height];
                            System.Runtime.InteropServices.Marshal.Copy(data, pixels, 0, width * height);

                            List<Vector3> roiPixels = new List<Vector3>();

                            // converting pixels to point in 3d space
                            for (int y = m_Roi.minY; y < m_Roi.maxY; y++)
                                for (int x = m_Roi.minX; x < m_Roi.maxX; x++)
                                {
                                    ushort depthRaw = (ushort)pixels[y * width + x];

                                    // depthRaw if 0 means there's no depth, the pixel is at depth zero just like any other pixel in a photo
                                    if (depthRaw != 0)
                                    {
                                        float[] pixel = { x, y };
                                        Vector3 point = new Vector3();
                                        var distance = depthRaw * m_Units;

                                        projectPixelToPoint(ref point, m_Intrinsics, pixel, distance);

                                        roiPixels.Add(new Vector3(point.X, -point.Y, point.Z));
                                    }
                                }

                            if (roiPixels.Count < 3) continue;

                            roiPixels.Reverse();

                            var plane = planeFromPoints(roiPixels);

                            // The points in RoI don't span a valid plane which is kind of useless
                            if (plane == new Plane(0, 0, 0, 0))
                            {
                                continue;
                            }

                            //Vector3 planeFitPivot = approximateIntersection(plane, m_Intrinsics.width / 2f, m_Intrinsics.height / 2f, 0f, 1000f);
                            Vector3[] planeCorners = new Vector3[4];


                            planeCorners[0] = approximateIntersection(plane, m_Roi.minX, m_Roi.minY, 0f, 1000f);
                            planeCorners[1] = approximateIntersection(plane, m_Roi.maxX, m_Roi.minY, 0f, 1000f);
                            planeCorners[2] = approximateIntersection(plane, m_Roi.maxX, m_Roi.maxY, 0f, 1000f);
                            planeCorners[3] = approximateIntersection(plane, m_Roi.minX, m_Roi.maxY, 0f, 1000f);

                            o_Result.p = plane;
                            o_Result.planeCorners = planeCorners;

                            if (!isPlaneValid(o_Result))
                            {
                                o_Result.planeCorners = null;
                                o_Result.isPlaneReconstructed = false;
                            }

                            // Resulting plance distance from the camera
                            o_Result.distance = (-plane.D * 1000);

                            OnRes?.Invoke(o_Result);
                        }
                    }
                    else
                        Thread.Sleep(4);
                }
            }
        }

        // Region of interest in the frame taken from the center
        private ROI setROI()
        {
            ROI res = new ROI
            {
                minX = (int)(m_Intrinsics.width  * (0.5 - 0.5 * 0.4)),
                minY = (int)(m_Intrinsics.height * (0.5 - 0.5 * 0.4)),
                maxX = (int)(m_Intrinsics.width  * (0.5 + 0.5 * 0.4)),
                maxY = (int)(m_Intrinsics.height * (0.5 + 0.5 * 0.4))
            };

            return res;
        }

        private Plane planeFromPoints(List<Vector3> i_Points)
        {
            if (i_Points.Count < 3) throw new System.ArgumentException("Not enough points to calculate plane");

            i_Points = new List<Vector3>(m_Ransac.GetInliers(i_Points, null, true));

            Vector3 sum = new Vector3();
            foreach (Vector3 v in i_Points) sum += v;

            Vector3 centroid = sum / i_Points.Count;

            double xx = 0, xy = 0, xz = 0, yy = 0, yz = 0, zz = 0;

            foreach (var p in i_Points)
            {
                Vector3 temp = p - centroid;
                xx += temp.X * temp.X;
                xy += temp.X * temp.Y;
                xz += temp.X * temp.Z;
                yy += temp.Y * temp.Y;
                yz += temp.Y * temp.Z;
                zz += temp.Z * temp.Z;
            }

            double detX = yy * zz - yz * yz;
            double detY = xx * zz - xz * xz;
            double detZ = xx * yy - xy * xy;

            double detMax = Math.Max((float)detX, Math.Max((float)detY, (float)detZ));
            if (detMax <= 0) return new Plane(0, 0, 0, 0);

            Vector3 dir;
            if (detMax == detX)
            {
                float a = (float)((xz * yz - xy * zz) / detX);
                float b = (float)((xy * yz - xz * yy) / detX);
                dir = new Vector3(1, a, b);
            }
            else if (detMax == detY)
            {
                float a = (float)((yz * xz - xy * zz) / detY);
                float b = (float)((xy * xz - yz * xx) / detY);
                dir = new Vector3(a, 1, b);
            }
            else
            {
                float a = (float)((yz * xy - xz * yy) / detZ);
                float b = (float)((xz * xy - yz * xx) / detZ);

                dir = new Vector3(a, b, 1);
            }

            dir = Vector3.Normalize(dir);
            dir = -dir;

            return new Plane(dir.X, dir.Y, dir.Z, -(centroid.X * dir.X + centroid.Y * dir.Y + centroid.Z * dir.Z));
        }

        private void projectPixelToPoint(ref Vector3 o_Point, Intrinsics i_Intrin, float[] i_Pixel, float depth)
        {
            float x = (i_Pixel[0] - i_Intrin.ppx) / i_Intrin.fx;
            float y = (i_Pixel[1] - i_Intrin.ppy) / i_Intrin.fy;

            if (i_Intrin.model == Distortion.InverseBrownConrady)
            {
                float r2 = x * x + y * y;
                float f = 1 + i_Intrin.coeffs[0] * r2 + i_Intrin.coeffs[1] * r2 * r2 + i_Intrin.coeffs[4] * r2 * r2 * r2;
                float ux = x * f + 2 * i_Intrin.coeffs[2] * x * y + i_Intrin.coeffs[3] * (r2 + 2 * x * x);
                float uy = y * f + 2 * i_Intrin.coeffs[3] * x * y + i_Intrin.coeffs[2] * (r2 + 2 * y * y);
                x = ux;
                y = uy;
            }

            if (i_Intrin.model == Distortion.Ftheta)
            {
                float rd = (float)Math.Sqrt(x * x + y * y);

                if (rd < FLT_EPSILON)
                {
                    rd = FLT_EPSILON;
                }

                float r = (float)Math.Tan((i_Intrin.coeffs[0] * rd) / Math.Atan(2 * Math.Tan(i_Intrin.coeffs[0] / 2.0f)));

                x *= r / rd;
                y *= r / rd;
            }

            o_Point.X = depth * x;
            o_Point.Y = depth * y;
            o_Point.Z = depth;
        }

        private Vector3 approximateIntersection(Plane i_Plane, float x, float y, float min, float max)
        {
            Vector3 point = new Vector3();
            var f = evaluatePixel(i_Plane, x, y, max, ref point);
            if (Math.Abs(max - min) < 1e-3)
                return point;

            var n = evaluatePixel(i_Plane, x, y, min, ref point);
            if (f * n > 0) return new Vector3();

            var avg = (max + min) / 2;
            var mid = evaluatePixel(i_Plane, x, y, avg, ref point);
            if (mid * n < 0) return approximateIntersection(i_Plane, x, y, min, avg);

            return approximateIntersection(i_Plane, x, y, avg, max);
        }

        private float evaluatePixel(Plane i_Plane, float x, float y, float distance, ref Vector3 output)
        {
            float[] pixel = { x, y };
            projectPixelToPoint(ref output, m_Intrinsics, pixel, distance);

            return evaluatePlane(i_Plane, output);
        }

        private float evaluatePlane(Plane plane, Vector3 point)
        {
            return plane.Normal.X * point.X + plane.Normal.Y * point.Y + plane.Normal.Z * point.Z + plane.D;
        }

        private bool isPlaneValid(Result i_Plane)
        {
            Vector3[] p = { i_Plane.planeCorners[0], i_Plane.planeCorners[1], i_Plane.planeCorners[2], i_Plane.planeCorners[3] };
            List<float> angles = new List<float>();
            bool res = false;

            for (int i = 0; i < p.Length; i++)
            {
                Vector3 p1 = p[i];
                Vector3 p2 = p[(i + 1) % p.Length];

                if ((p2 - p1).Length() < 1e-3)
                    break;

                p1 = Vector3.Normalize(p1);
                p2 = Vector3.Normalize(p2);

                angles.Add((float)(Math.Acos((Vector3.Dot(p1, p2)) / Math.Sqrt(p1.Length() * p2.Length()))));
            }

            int minuses = 0, pluses = 0;

            for (int i = 0; i < angles.Count; i++)
            {
                if (angles[i] < 0) minuses++;
                else if (angles[i] > 0) pluses++;
            }

            if (minuses == 4 || pluses == 4) res = true;

            return res;
        }
        
        #endregion// depth and data calculations

        #endregion// private methods
    }
}

using System;
using System.Collections.Generic;
using System.Numerics;

namespace Holoarch.Mediator.PlaneFromDepth
{
    public class Ransac
    {
        private int m_NumberOfIterations = 15;

        public Vector3[] GetInliers(List<Vector3> points, Vector3[] pointsArr, bool planeFitter, Plane fitted = default, float threshold = 0.01f)
        {
            Random r = new Random();
            List<Vector3> inliers = null;
            Plane plane = default;
            int maxNumberOfIterationsCalculated = (int)Math.Ceiling(Math.Log10(1f - 0.99f) / Math.Log10(1 - Math.Pow(1f - 0.01f, 3)));
            m_NumberOfIterations = maxNumberOfIterationsCalculated > m_NumberOfIterations?  maxNumberOfIterationsCalculated : m_NumberOfIterations;
            float bestScore = float.PositiveInfinity;
            Vector3 bestPointA = Vector3.Zero;
            Vector3 bestPointB = Vector3.Zero;
            Vector3 bestPointC = Vector3.Zero;
            Vector3 centroid   = Vector3.Zero;
            List<Vector3> currentInliers = null;
            List<Vector3> newPoints = null;

            if (planeFitter)
            {
                //newPoints = new List<Vector3>();

                //for (int i = 0; i < points.Count; i++)
                //{
                //    if (points[i] != Vector3.Zero)
                //    {
                //        newPoints.Add(points[i]);
                //    }
                //}

                newPoints = points;
            }
            else
            {
                newPoints = new List<Vector3>(pointsArr);
            }

            int amountOfPoints = newPoints.Count;
            int firstIdx = 0;
            int secondIdx = 0;
            int thirdIdx = 0;

            if (!planeFitter)
            {
                currentInliers = new List<Vector3>();

                for (int j = 0; j < amountOfPoints; j++)
                {
                    float currentError = fitted.GetDistanceToPoint(newPoints[j]);

                    if (currentError < threshold)
                    {
                        currentInliers.Add(newPoints[j]);
                    }
                    else
                    {
                        currentInliers.Add(Vector3.Zero);
                    }
                }

                inliers = currentInliers;
            }
            else
            {
                for (int i = 0; i < m_NumberOfIterations; i++)
                {
                    while (plane.Normal == Vector3.Zero)
                    {
                        firstIdx = r.Next(0, amountOfPoints);
                        secondIdx = r.Next(0, amountOfPoints);
                        thirdIdx = r.Next(0, amountOfPoints);
                        plane = Plane.CreateFromVertices(newPoints[firstIdx], newPoints[secondIdx], newPoints[thirdIdx]);
                    }

                    currentInliers = new List<Vector3>();
                    float currentScore = 0f;

                    for (int j = 0; j < amountOfPoints; j++)
                    {
                        float currentError = plane.GetDistanceToPoint(newPoints[j]);

                        if (currentError < threshold)
                        {
                            currentScore += currentError;
                            currentInliers.Add(newPoints[j]);
                        }
                        else
                        {
                            if (!planeFitter)
                                currentInliers.Add(Vector3.Zero);
                            currentScore += threshold;
                        }
                    }

                    if (currentScore < bestScore)
                    {
                        bestScore = currentScore;
                        inliers = currentInliers;
                        bestPointA = newPoints[firstIdx];
                        bestPointB = newPoints[secondIdx];
                        bestPointC = newPoints[thirdIdx];
                    }
                }
            }

            return inliers.ToArray();
        }

        private bool checkIfInBounds(Vector3 p, float bounds = 0.8f)
        {
            return p.X <= bounds && p.Y < bounds && p.Z <= bounds;
        }

        private Vector3[] getBounds(Vector3[] points)
        {
            Vector3[] max = new Vector3[2];
            max[0] = max[1] = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3[] min = new Vector3[2];
            min[0] = min[1] = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].X > max[0].X)
                {
                    max[1] = max[0];
                    max[0] = points[i];
                }
                else if (points[i].X > max[1].X && points[i].X < max[0].X)
                {
                    max[1] = points[i];
                }
                else if (points[i].X < min[0].X)
                {
                    min[1] = min[0];
                    min[0] = points[i];
                }
                else if (points[i].X > min[0].X && points[i].X < min[1].X)
                {
                    max[1] = points[i];
                }
            }

            return new Vector3[] { min[1], max[1] };
        }
    }

}

using Holoarch.Mediator.Commons;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Holoarch.Mediator
{
    public class Mediator
    {
        private static OperatingConfig m_WorkConfig;
        private static List<Vector3> vectors = new List<Vector3>();

        public Mediator(Enum i_Mode)
        {
            switch (i_Mode)
            {
                case OperatingMode.Depth.Realsense:
                    m_WorkConfig = new RealsenseDepth();
                    m_WorkConfig.Enable(onRes);
                    break;
            }
        }

        public static void Main(string[] args)
        {
            var mediator = new Mediator(OperatingMode.Depth.Realsense);

            while (!Console.KeyAvailable)
            {

            }

            m_WorkConfig.Disable();

            Vector3 standardDeviation = getStandardDeviation(vectors);
        }

        private static Vector3 getStandardDeviation(List<Vector3> vectors)
        {
            Vector3 mean = Vector3.Zero;

            for (int i = 0; i < vectors.Count; i++)
            {
                mean += vectors[i];
            }

            mean /= vectors.Count;
            List<float> valxMinMeanSquared = new List<float>(vectors.Count);
            List<float> valyMinMeanSquared = new List<float>(vectors.Count);
            List<float> valzMinMeanSquared = new List<float>(vectors.Count);

            for (int i = 0; i < vectors.Count; i++)
            {
                valxMinMeanSquared.Add((float)Math.Pow(vectors[i].X - mean.X, 2));
                valyMinMeanSquared.Add((float)Math.Pow(vectors[i].Y - mean.Y, 2));
                valzMinMeanSquared.Add((float)Math.Pow(vectors[i].Z - mean.Z, 2));
            }

            Vector3 res = new Vector3();
            float sum = 0;

            for (int i = 0; i < valxMinMeanSquared.Count; i++)
            {
                sum += valxMinMeanSquared[i];
            }

            res.X = (float)Math.Sqrt(sum / valxMinMeanSquared.Count);
            sum = 0;

            for (int i = 0; i < valyMinMeanSquared.Count; i++)
            {
                sum += valyMinMeanSquared[i];
            }

            res.Y = (float)Math.Sqrt(sum / valyMinMeanSquared.Count);
            sum = 0;

            for (int i = 0; i < valzMinMeanSquared.Count; i++)
            {
                sum += valzMinMeanSquared[i];
            }

            res.Z = (float)Math.Sqrt(sum / valzMinMeanSquared.Count);

            return res;
        }

        private void onRes(Result i_Result)
        {
            Console.WriteLine(i_Result.p.Normal);
            vectors.Add(i_Result.p.Normal);
        }
    }

    //public class Program
    //{
    //    public static void Main(string[] args)
    //    {
    //        var mediator = new Mediator(WorkMode.Depth.Realsense);

    //        while (!Console.KeyAvailable)
    //        {

    //        }

    //    }
    //}
}

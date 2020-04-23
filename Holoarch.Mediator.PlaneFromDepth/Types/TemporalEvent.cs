/// -----------------------------------------------------------------
///   Namespace:      Name
///   Class:          TemporalEvent
///   Description:    A simple decision making class that decides which user instruction to display	
///   Author:         Vlad         Date: 	8/8/19
///   Notes:          First Release
///   Revision History:
///   Name:           Date:        Description:	
/// -----------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Holoarch.Mediator.PlaneFromDepth
{
    public class TemporalEvent
    {
        // Time window, everything prior is disregarded
        private float m_Window;

        // first = time, second = 
        private List<Tuple<float, bool>> m_Measurements;

        // Default window is 33.3333333...ms which is 30fps
        public TemporalEvent(float i_Window = .033f)
        {
            m_Window = i_Window;
            m_Measurements = new List<Tuple<float, bool>>();
        }

        public void AddValue(bool i_Val)
        {
            m_Measurements.Add(new Tuple<float, bool>(Global.Time.ElapsedMilliseconds, i_Val));
        }

        public bool Eval()
        {
            if (Global.Time.ElapsedMilliseconds < m_Window)
            {
                return false;
            }

            int from = m_Measurements.Count - 1;

            // Discard older than assigned window values
            for (; from > 0; from--)
            {
                if ((Global.Time.ElapsedMilliseconds - m_Measurements[from].Item1) > m_Window)
                {
                    break;
                }
            }

            m_Measurements.RemoveRange(0, from);
            int trues = 0;

            // Count truths
            for (int i = 0; i < m_Measurements.Count; i++)
            {
                if (m_Measurements[i].Item2)
                {
                    trues++;
                }
            }

            return trues * 2 > m_Measurements.Count;
        }
    }
}
namespace Holoarch.Mediator.Commons
{
    public class FrameQueue<T> where T : Frame
    {
        private System.Collections.Concurrent.ConcurrentQueue<T> m_Q = new System.Collections.Concurrent.ConcurrentQueue<T>();
        private object m_LockObject = new object();

        public int Limit { get; set; } = 1;

        public FrameQueue(int i_Limit = 1)
        {
            Limit = i_Limit;
        }

        public void Enqueue(T i_Frame)
        {
            lock (m_LockObject)
            {
                m_Q.Enqueue(i_Frame);
                T overflow;

                while (m_Q.Count > Limit && m_Q.TryDequeue(out overflow)) ;
            }
        }

        public int Count { get { return m_Q.Count; } }

        public bool Dequeue(out T o_Frame)
        {
            lock (m_LockObject)
            {
                bool res = false;
                T tRet;

                if (m_Q.TryDequeue(out tRet))
                {
                    res = true;
                    o_Frame = tRet;
                }
                else
                    o_Frame = default;

                return res;
            }
        }

        public Frame Peek()
        {
            lock (m_LockObject)
            {
                T tRet;

                if (m_Q.TryPeek(out tRet))
                    return tRet;
                else
                    return default;
            }
        }
    }
}

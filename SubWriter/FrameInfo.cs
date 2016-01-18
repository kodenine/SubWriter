using System;
using System.Collections.Generic;
using System.Text;

namespace Subwriter
{
    /// <summary>
    /// FrameInfo describes an instance of a frame
    /// </summary>
    public class FrameInfo
    {
        private double m_fDuration;
        private double m_dFrameRate;

        public double FrameRate
        {
            get { return m_dFrameRate; }
            set { m_dFrameRate = value; }
        }
        public double Duration
        {
            get { return m_fDuration; }
            set { m_fDuration = value; }
        }
        public int FrameNumber
        {
            get
            {
                double frame = m_fDuration * m_dFrameRate;
                return (int)frame;
            }
        }
        public int Hour
        {
            get { return ((int)m_fDuration) / 3600; }
        }
        public int Minute
        {
            get { return (((int)m_fDuration) - (Hour * 3600)) / 60; }
        }
        public int Second
        {
            get { return (int)m_fDuration - (Hour * 3600 + Minute * 60); }
        }
        public int MilliSecond
        {
            get
            {
                double diff = (m_fDuration - (int)m_fDuration) * 1000;
                return (int)diff;
            }
        }

    }
}

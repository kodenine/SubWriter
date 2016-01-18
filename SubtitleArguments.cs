using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subwriter
{
    public class SubtitleArguments
    {
        public enum ActionType { Help, Process }



        public string m_strFiles;
        private string m_strSubtitlePrefix;

        private double m_dFrameRate;
        private double m_dSubDuration;
        private string m_strSubtitleFileName;
        private string m_strChapterFileName;

        public SubtitleFormat SubtitleFormat { get; set; }
        public BaseChapterFormat ChapterFormat { get; set; }

        public bool Recursive { get; set; }

        public bool Scenalyzer { get; set; }
        public List<string> Filenames { get; set; } = new List<string>();

        public ActionType Action { get; set; } = ActionType.Help;

        public string SubtitlePrefix
        {
            get { return m_strSubtitlePrefix; }
            set { m_strSubtitlePrefix = value; }
        }
        public double FrameRate
        {
            get { return m_dFrameRate; }
            set { m_dFrameRate = value; }
        }
        public double SubDuration
        {
            get { return m_dSubDuration; }
            set { m_dSubDuration = value; }
        }
        public string SubtitleFileName
        {
            get { return m_strSubtitleFileName; }
            set { m_strSubtitleFileName = value; }
        }
        public string ChapterFileName
        {
            get { return m_strChapterFileName; }
            set { m_strChapterFileName = value; }
        }
    }
}

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

        public ActionType Action { get; set; } = ActionType.Help;

        public List<string> Filenames { get; set; } = new List<string>();
                
        public double FrameRate { get; set; }
        public string SubtitlePrefixFilename { get; set; }
        public double SubDuration { get; set; }
        public string SubtitleFileName { get; set; }
        public BaseSubtitleFormat SubtitleFormat { get; set; }

        public string ChapterFileName { get; set; }
        public BaseChapterWriter ChapterFormat { get; set; }

        public bool Recursive { get; set; }
        public bool Scenalyzer { get; set; }
    }
}

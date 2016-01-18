using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subwriter
{
    public class SubtitleFormat
    {
        public static SubtitleFormat Standard { get; set; } = new SubtitleFormat( nameof(SubtitleFormat.Standard), ".txt");
        public static SubtitleFormat Subrip { get; set; } = new SubtitleFormat( nameof(SubtitleFormat.Subrip), ".srt");
        public static SubtitleFormat Subviewer { get; set; } = new SubtitleFormat( nameof(SubtitleFormat.Subviewer), ".sub" );
        public static SubtitleFormat MicroDvd { get; set; } = new SubtitleFormat( nameof(SubtitleFormat.MicroDvd), "mdvd" );
        public static SubtitleFormat Spruce { get; set; } = new SubtitleFormat( nameof( SubtitleFormat.Spruce ), ".stl" );
        public static SubtitleFormat Encore { get; set; } = new SubtitleFormat( nameof( SubtitleFormat.Encore ), ".txt" );

        public SubtitleFormat ( string name, string fileExtension )
        {
            Name = name;
            FileExtension = fileExtension;
        }

        public string Name { get; set; }
        public string FileExtension { get; set; }
    }
}

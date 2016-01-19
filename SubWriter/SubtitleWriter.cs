using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subwriter
{
    public abstract class BaseSubtitleFormat
    {
        public string FileExtension { get; set; }

        public StreamWriter CreateFile( string filename, bool addDefaultPrefix )
        {
            FileInfo chapterFile = new FileInfo( filename );
            StreamWriter chapterFileStream = chapterFile.CreateText();

            return chapterFileStream;
        }

        protected virtual void AddPrefixToFileStream( StreamWriter subtitleStreamWriter )
        { }
    }

    public class StandardSubtitleFormat : BaseSubtitleFormat
    {
        public StandardSubtitleFormat()
        {
            FileExtension = ".txt";
        }
    }

    public class SubripSubtitleFormat : BaseSubtitleFormat
    {
        public SubripSubtitleFormat()
        {
            FileExtension = ".srt";
        }
    }

    public class SubviewerSubtitleFormat : BaseSubtitleFormat
    {
        public SubviewerSubtitleFormat()
        {
            FileExtension = ".sub";
        }
    }

    public class MicroDvdSubtitleFormat : BaseSubtitleFormat
    {
        public MicroDvdSubtitleFormat()
        {
            FileExtension = ".mdvd";
        }
    }

    public class SpruceSubtitleFormat : BaseSubtitleFormat
    {
        public SpruceSubtitleFormat()
        {
            FileExtension = ".stl";
        }

        protected override void AddPrefixToFileStream( StreamWriter subtitleStreamWriter )
        {
            subtitleStreamWriter.WriteLine( "//Font select and font size" );
            subtitleStreamWriter.WriteLine( "$FontName       = Arial" );
            subtitleStreamWriter.WriteLine( "$FontSize       = 30" );
            subtitleStreamWriter.WriteLine();
            subtitleStreamWriter.WriteLine( "//Character attributes (global)" );
            subtitleStreamWriter.WriteLine( "$Bold           = FALSE" );
            subtitleStreamWriter.WriteLine( "$UnderLined     = FALSE" );
            subtitleStreamWriter.WriteLine( "$Italic         = FALSE" );
            subtitleStreamWriter.WriteLine();
            subtitleStreamWriter.WriteLine( "//Position Control" );
            subtitleStreamWriter.WriteLine( "$HorzAlign      = Center" );
            subtitleStreamWriter.WriteLine( "$VertAlign      = Bottom" );
            subtitleStreamWriter.WriteLine( "$XOffset        = 0" );
            subtitleStreamWriter.WriteLine( "$YOffset        = 0" );
            subtitleStreamWriter.WriteLine();
            subtitleStreamWriter.WriteLine( "//Contrast Control" );
            subtitleStreamWriter.WriteLine( "$TextContrast           = 15" );
            subtitleStreamWriter.WriteLine( "$Outline1Contrast       = 8" );
            subtitleStreamWriter.WriteLine( "$Outline2Contrast       = 15" );
            subtitleStreamWriter.WriteLine( "$BackgroundContrast     = 0" );
            subtitleStreamWriter.WriteLine();
            subtitleStreamWriter.WriteLine( "//Effects Control" );
            subtitleStreamWriter.WriteLine( "$ForceDisplay   = FALSE" );
            subtitleStreamWriter.WriteLine( "$FadeIn         = 10" );
            subtitleStreamWriter.WriteLine( "$FadeOut        = 10" );
            subtitleStreamWriter.WriteLine();
            subtitleStreamWriter.WriteLine( "//Other Controls" );
            subtitleStreamWriter.WriteLine( "$TapeOffset          = FALSE" );
            subtitleStreamWriter.WriteLine( "//$SetFilePathToken  = <<:>>" );
            subtitleStreamWriter.WriteLine();
            subtitleStreamWriter.WriteLine( "//Colors" );
            subtitleStreamWriter.WriteLine( "$ColorIndex1    = 0" );
            subtitleStreamWriter.WriteLine( "$ColorIndex2    = 1" );
            subtitleStreamWriter.WriteLine( "$ColorIndex3    = 2" );
            subtitleStreamWriter.WriteLine( "$ColorIndex4    = 3" );
            subtitleStreamWriter.WriteLine();
            subtitleStreamWriter.WriteLine( "//Subtitles" );
        }
    }

    public class EncoreSubtitleFormat : BaseSubtitleFormat
    {
        public EncoreSubtitleFormat()
        {
            FileExtension = ".txt";
        }
    }
    
}

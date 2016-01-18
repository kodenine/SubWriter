using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace Subwriter
{
    /// <summary>
    /// Main and search pocedures contained within ConsoleSubWriter
    /// </summary>
    public class VideoProcessor
    {
        public class StatusEventHandler : EventArgs
        {
            public StatusEventHandler( string message )
            {
                Message = message;
            }

            public string Message { get; set; }
        }

        private SubtitleArguments _args;

        public event StatusHandler Status;
        public delegate void StatusHandler( VideoProcessor m, StatusEventHandler e );

        public VideoProcessor( SubtitleArguments args )
        {
            _args = args;
        }


        //Search Function
        public bool Process()
        {
            bool success = false;

            String currentDirectory = Environment.CurrentDirectory;

            //Create recursively a list with all the files complying with the criteria
            List<FileInfo> files = new List<FileInfo>();

            foreach (string fileName in _args.Filenames )
            {
                //Eliminate white spaces
                var trimmedFileName = fileName.Trim();

                var filename = Path.GetFileName( trimmedFileName );
                var fileDirectory = Path.GetDirectoryName( trimmedFileName );

                files.AddRange( GetFiles( fileDirectory, trimmedFileName, _args.Recursive ) );
            }

            if ( files.Count == 0 )
            {
                success = false;
                UpdateStatus( "no files to process" );
            }

            if ( success )
            { 
                StreamWriter subtileStreamWriter = _args.SubtitleFormat.CreateFile();
                StreamWriter chapterStreamWriter = _args.ChapterFormat.CreateFile();


            FileInfo SubtitleFile = new FileInfo( _args.SubtitleFileName );
            StreamWriter SubtitleFS = SubtitleFile.CreateText();
            FileInfo ChapterFile = new FileInfo( _args.ChapterFileName );
            StreamWriter ChapterFS = ChapterFile.CreateText();

            switch (m_iChapterFormat)
            {
                case c_iCF_IFO:
                    break;
                case c_iCF_SpruceMaestro:
                    ChapterFS.WriteLine("$Spruce_IFrame_List\r\n");
                    break;
            }
            if (m_strSubtitlePrefix == "auto")
            {
                switch (m_iSubtitleFormat)
                {
                    case c_iSF_SPRUCE:
                        SubtitleFS.WriteLine("//Font select and font size");
                        SubtitleFS.WriteLine("$FontName       = Arial");
                        SubtitleFS.WriteLine("$FontSize       = 30");
                        SubtitleFS.WriteLine();
                        SubtitleFS.WriteLine("//Character attributes (global)");
                        SubtitleFS.WriteLine("$Bold           = FALSE");
                        SubtitleFS.WriteLine("$UnderLined     = FALSE");
                        SubtitleFS.WriteLine("$Italic         = FALSE");
                        SubtitleFS.WriteLine();
                        SubtitleFS.WriteLine("//Position Control");
                        SubtitleFS.WriteLine("$HorzAlign      = Center");
                        SubtitleFS.WriteLine("$VertAlign      = Bottom");
                        SubtitleFS.WriteLine("$XOffset        = 0");
                        SubtitleFS.WriteLine("$YOffset        = 0");
                        SubtitleFS.WriteLine();
                        SubtitleFS.WriteLine("//Contrast Control");
                        SubtitleFS.WriteLine("$TextContrast           = 15");
                        SubtitleFS.WriteLine("$Outline1Contrast       = 8");
                        SubtitleFS.WriteLine("$Outline2Contrast       = 15");
                        SubtitleFS.WriteLine("$BackgroundContrast     = 0");
                        SubtitleFS.WriteLine();
                        SubtitleFS.WriteLine("//Effects Control");
                        SubtitleFS.WriteLine("$ForceDisplay   = FALSE");
                        SubtitleFS.WriteLine("$FadeIn         = 10");
                        SubtitleFS.WriteLine("$FadeOut        = 10");
                        SubtitleFS.WriteLine();
                        SubtitleFS.WriteLine("//Other Controls");
                        SubtitleFS.WriteLine("$TapeOffset          = FALSE");
                        SubtitleFS.WriteLine("//$SetFilePathToken  = <<:>>");
                        SubtitleFS.WriteLine();
                        SubtitleFS.WriteLine("//Colors");
                        SubtitleFS.WriteLine("$ColorIndex1    = 0");
                        SubtitleFS.WriteLine("$ColorIndex2    = 1");
                        SubtitleFS.WriteLine("$ColorIndex3    = 2");
                        SubtitleFS.WriteLine("$ColorIndex4    = 3");
                        SubtitleFS.WriteLine();
                        SubtitleFS.WriteLine("//Subtitles");
                        break;
                }
            }
            else {
                try
                {
                    FileInfo SubtitlePrefix = new FileInfo(m_strSubtitlePrefix);
                    StreamReader SubtitlePrefixFS = SubtitlePrefix.OpenText();
                    SubtitleFS.Write(SubtitlePrefixFS.ReadToEnd());
                    SubtitlePrefixFS.Close();
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("Subtitle Prefix File ({0}) not found!", m_strSubtitlePrefix);
                    PrintHelp();
                    return;
                }
            }

            String strResults = "";
            int FileCount = 0;
            int SubCount = 1;
            bool bEmpty = true;
            FrameInfo Total = new FrameInfo();
            Total.FrameRate = m_dFrameRate;
            Total.Duration = 0;
            // Sorts the values of the ArrayList using the reverse case-insensitive comparer.
            // IComparer myComparer = new myReverserClass();
            // m_arrFiles.Sort( 1, 3, myComparer );
            m_arrFiles.Sort();
            IEnumerator enm = m_arrFiles.GetEnumerator();
            while (enm.MoveNext())
            {
                try
                {
                    // Open: (string)enm.Current;
                    FilgraphManager m_objFilterGraph = null;
                    IMediaPosition m_objMediaPosition = null;
                    m_objFilterGraph = new FilgraphManager();
                    m_objFilterGraph.RenderFile((string)enm.Current);
                    m_objMediaPosition = m_objFilterGraph as IMediaPosition;
                    string filename = Path.GetFileNameWithoutExtension((string)enm.Current);
                    if (m_bScenalyzer)
                        filename = ScenalyzerFormat(filename);
                    FileCount++;
                    FrameInfo Start = new FrameInfo();
                    Start.FrameRate = m_dFrameRate;
                    Start.Duration = Total.Duration;
                    Start.Duration += 1;
                    FrameInfo End = new FrameInfo();
                    End.FrameRate = m_dFrameRate;
                    End.Duration = Start.Duration + m_dSubDuration - 0.5;

                    /* switch(m_iChapterFormat)
					{
						case c_iCF_IFO:
							ChapterFS.WriteLine(String.Format("{0}",Total.FrameNumber));
							break;
						case c_iCF_SpruceMaestro:
							ChapterFS.WriteLine(String.Format("{0:00}:{1:00}:{2:00}:{3:00}",
												Total.Hour,Total.Minute,Total.Second,(Total.MilliSecond/10)));
							break;
					} */
                    WriteChapter(ChapterFS, Total);
                    Total.Duration += m_objMediaPosition.Duration - 2;

                    if (End.Duration > Total.Duration)
                    {
                        End.Duration = Total.Duration - 0.1;
                    }

                    // while (End.Duration < Total.Duration)
                    // {
                    switch (m_iSubtitleFormat)
                    {
                        case c_iSF_STANDARD:
                            SubtitleFS.WriteLine("FileName: " + (string)enm.Current + "\t");
                            SubtitleFS.WriteLine(String.Format("Duration: {0:00}:{1:00}:{2:00}.{3:000} to \r\n",
                                Start.Hour, Start.Minute, Start.Second, Start.MilliSecond));
                            break;
                        case c_iSF_SUBRIP:
                            SubtitleFS.WriteLine(String.Format("{0}", SubCount));
                            SubtitleFS.WriteLine(String.Format("{0:00}:{1:00}:{2:00},{3:000} --> {4:00}:{5:00}:{6:00},{7:000}",
                                Start.Hour, Start.Minute, Start.Second, Start.MilliSecond,
                                End.Hour, End.Minute, End.Second, End.MilliSecond));
                            SubtitleFS.WriteLine(String.Format("{0}", filename));
                            SubtitleFS.WriteLine("");
                            break;
                        case c_iSF_SPRUCE:
                            SubtitleFS.WriteLine(String.Format("{0:00}:{1:00}:{2:00}:{3:00},{4:00}:{5:00}:{6:00}:{7:00},{8}",
                                Start.Hour, Start.Minute, Start.Second, (Start.MilliSecond / 10),
                                End.Hour, End.Minute, End.Second, (End.MilliSecond / 10), filename));
                            break;
                        case c_iSF_ENCORE:
                            SubtitleFS.WriteLine(String.Format("{0:00}:{1:00}:{2:00}:{3:00} {4:00}:{5:00}:{6:00}:{7:00} {8}",
                                Start.Hour, Start.Minute, Start.Second, (Start.MilliSecond / 10),
                                End.Hour, End.Minute, End.Second, (End.MilliSecond / 10), filename));
                            break;
                    }
                    //	Start.Duration += m_dSubDuration;
                    //	End.Duration = Start.Duration + m_dSubDuration - 0.5;
                    SubCount++;
                    // }
                    // Process file for frames and duration
                    bEmpty = false;
                    if (m_objMediaPosition != null) m_objMediaPosition = null;
                    if (m_objFilterGraph != null) m_objFilterGraph = null;
                }
                catch (SecurityException)
                {
                    strResults += "\r\n" + (string)enm.Current + ": Security Exception\r\n\r\n";
                }
                catch (FileNotFoundException)
                {
                    strResults += "\r\n" + (string)enm.Current + ": File Not Found Exception\r\n";
                }
            }
            Total.Duration -= 0.5;
            WriteChapter(ChapterFS, Total);
            if (bEmpty == true)
                Console.WriteLine("No matches found!");
            else
                Console.WriteLine(String.Format("Processed {0} Files!\r\n", FileCount));
            Console.WriteLine(strResults);
            SubtitleFS.Close();
            ChapterFS.Close();

            success = true;

            return success;
        }


        //Build the list of Files
        private List<FileInfo> GetFiles( String directory, String filename, bool recursive )
        {
            List<FileInfo> results = new List<FileInfo>();

            //search pattern can include the wild characters '*' and '?'
            string[] fileList = Directory.GetFiles( directory, filename );
            for ( int i = 0; i < fileList.Length; i++ )
            {
                if ( File.Exists( fileList[i] ) )
                    results.Add( new FileInfo( fileList[i] ) );
            }
            if ( recursive == true )
            {
                //Get recursively from subdirectories
                string[] dirList = Directory.GetDirectories( directory );
                for ( int i = 0; i < dirList.Length; i++ )
                {
                    results.AddRange( GetFiles( dirList[i], filename, true ) );
                }
            }

            return results;
        }

        // Format Scenalyzer named files
        private string ScenalyzerFormat( string ScenalyzerFileName )
        {
            string processing = ScenalyzerFileName.Replace( "scene'", "" );
            processing = processing.Replace( "_joined", "" );
            string year = processing.Substring( 0, 4 );
            string month = processing.Substring( 4, 2 );
            string day = processing.Substring( 6, 2 );
            string mytime = processing.Substring( 9, 8 );
            mytime = mytime.Replace( ".", ":" );
            if ( Regex.Match( mytime, "##:##:##" ).Success == false )
            {
                mytime = "00:00:00";
            }
            string title = "";
            if ( processing.Length > 17 )
            {
                title = " - " + processing.Substring( 17 );
            }
            // string myDateTimeValue = "2/16/1992 12:15:12";
            string myDateTimeValue = month + "/" + day + "/" + year + " " + mytime;
            DateTime myDateTime = Convert.ToDateTime( myDateTimeValue );
            processing = String.Format( "{0:MMMM} {0:dd}, {0:yyyy}{1}", myDateTime, title );
            return processing;
        }

        // Write Chapter
        private void WriteChapter( StreamWriter ChapterFileStream, FrameInfo CurrentFrame )
        {
            switch ( m_iChapterFormat )
            {
                case c_iCF_IFO:
                    ChapterFileStream.WriteLine( String.Format( "{0}", CurrentFrame.FrameNumber ) );
                    break;
                case c_iCF_SpruceMaestro:
                    ChapterFileStream.WriteLine( String.Format( "{0:00}:{1:00}:{2:00}:{3:00}",
                        CurrentFrame.Hour, CurrentFrame.Minute, CurrentFrame.Second, (CurrentFrame.MilliSecond / 10) ) );
                    break;
            }
        }

        private void UpdateStatus( string message )
        {
            if ( Status != null )
            {
                Status( this, new StatusEventHandler( message ) );
            }
        }
    }
}

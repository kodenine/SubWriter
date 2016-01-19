using QuartzTypeLib;
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

        private const double SECONDS_DELAY_BEFORE_SUBTITLE_DISPLAY = 5;

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

                bool addDefaultPrefix = !String.IsNullOrEmpty( _args.SubtitlePrefixFilename ) && !_args.SubtitlePrefixFilename.Equals( "auto", StringComparison.OrdinalIgnoreCase );
                StreamWriter subtileStreamWriter = _args.SubtitleFormat.CreateFile( _args.SubtitleFileName, addDefaultPrefix );
                StreamWriter chapterStreamWriter = _args.ChapterFormat.CreateFile( _args.ChapterFileName );
                

                if ( addDefaultPrefix )
                {
                    FileInfo SubtitlePrefix = new FileInfo( _args.SubtitlePrefixFilename );
                    StreamReader SubtitlePrefixFS = SubtitlePrefix.OpenText();
                    subtileStreamWriter.Write(SubtitlePrefixFS.ReadToEnd());
                    SubtitlePrefixFS.Close();
                }
            
                int FileCount = 0;
                int SubCount = 1;
                bool bEmpty = true;
                FrameInfo Total = new FrameInfo();
                Total.FrameRate = _args.FrameRate;
                Total.Duration = 0;

                // Sorts the values of the list
                _args.Filenames.Sort();
                foreach( string filename in _args.Filenames )
                { 
                    try
                    {
                        FilgraphManager m_objFilterGraph = null;
                        IMediaPosition m_objMediaPosition = null;
                        m_objFilterGraph = new FilgraphManager();
                        m_objFilterGraph.RenderFile( filename );
                        m_objMediaPosition = m_objFilterGraph as IMediaPosition;
                        string formattedFilename = Path.GetFileNameWithoutExtension( filename );
                        if ( _args.Scenalyzer )
                        {
                            formattedFilename = ScenalyzerFormat( formattedFilename );
                        }
                        FileCount++;
                        FrameInfo Start = new FrameInfo();
                        Start.FrameRate = _args.FrameRate;
                        Start.Duration = Total.Duration;
                        Start.Duration += SECONDS_DELAY_BEFORE_SUBTITLE_DISPLAY;
                        FrameInfo End = new FrameInfo();
                        End.FrameRate = _args.FrameRate;
                        End.Duration = Start.Duration + _args.SubDuration;

                        WriteChapter(chapterStreamWriter, Total);
                        Total.Duration += m_objMediaPosition.Duration;

                        if (End.Duration > Total.Duration)
                        {
                            End.Duration = Total.Duration - (1 / _args.FrameRate);
                        }


                        WriteSubtitles( subtileStreamWriter, formattedFilename, SubCount, Start, End )
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

        private void WriteSubtitles( StreamWriter subtileStreamWriter, string formattedFilename, int subNumber, FrameInfo start, FrameInfo end )
        {
            switch ( _args.SubtitleFormat.GetType() )
            {
                case typeof( StandardSubtitleFormat ):
                    subtileStreamWriter.WriteLine( "FileName: " + formattedFilename + "\t" );
                    subtileStreamWriter.WriteLine( String.Format( "Duration: {0:00}:{1:00}:{2:00}.{3:000} to \r\n",
                        start.Hour, start.Minute, start.Second, start.MilliSecond ) );
                    break;
                case typeof( SubripSubtitleFormat ):
                    subtileStreamWriter.WriteLine( String.Format( "{0}", subNumber ) );
                    subtileStreamWriter.WriteLine( String.Format( "{0:00}:{1:00}:{2:00},{3:000} --> {4:00}:{5:00}:{6:00},{7:000}",
                        start.Hour, start.Minute, start.Second, start.MilliSecond,
                        end.Hour, end.Minute, end.Second, end.MilliSecond ) );
                    subtileStreamWriter.WriteLine( String.Format( "{0}", formattedFilename ) );
                    subtileStreamWriter.WriteLine( "" );
                    break;
                case typeof( SpruceSubtitleFormat ):
                    subtileStreamWriter.WriteLine( String.Format( "{0:00}:{1:00}:{2:00}:{3:00},{4:00}:{5:00}:{6:00}:{7:00},{8}",
                        start.Hour, start.Minute, start.Second, (start.MilliSecond / 10),
                        end.Hour, end.Minute, end.Second, (end.MilliSecond / 10), formattedFilename ) );
                    break;
                case typeof( EncoreSubtitleFormat ):
                    subtileStreamWriter.WriteLine( String.Format( "{0:00}:{1:00}:{2:00}:{3:00} {4:00}:{5:00}:{6:00}:{7:00} {8}",
                        start.Hour, start.Minute, start.Second, (start.MilliSecond / 10),
                        end.Hour, end.Minute, end.Second, (end.MilliSecond / 10), formattedFilename ) );
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

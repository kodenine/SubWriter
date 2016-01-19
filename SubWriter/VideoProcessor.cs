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
            bool success = true;

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
                using ( BaseChapterWriter chapterWriter = _args.ChapterWriterFactory.Create( _args.ChapterFileName ) )
                using ( BaseSubtitleWriter subtitleWriter = _args.SubtitleWriterFactory.Create( _args.SubtitleFileName ) )
                {
                    bool addDefaultPrefix = !String.IsNullOrEmpty( _args.SubtitlePrefixFilename ) && !_args.SubtitlePrefixFilename.Equals( "auto", StringComparison.OrdinalIgnoreCase );
                    if ( addDefaultPrefix )
                    {
                        FileInfo SubtitlePrefix = new FileInfo( _args.SubtitlePrefixFilename );
                        StreamReader SubtitlePrefixFS = SubtitlePrefix.OpenText();
                        subtitleWriter.Prefix = SubtitlePrefixFS.ReadToEnd();
                        SubtitlePrefixFS.Close();
                    }

                    int fileCount = 0;
                    int subCount = 1;
                    bool empty = true;
                    FrameInfo totalFrameInfo = new FrameInfo();
                    totalFrameInfo.FrameRate = _args.FrameRate;
                    totalFrameInfo.Duration = 0;
                    string previousFormattedFilename = null;

                    // Sorts the values of the list
                    _args.Filenames.Sort();
                    foreach ( string filename in _args.Filenames )
                    {
                        try
                        {
                            FilgraphManager filterGraphManager = null;
                            IMediaPosition mediaPosition = null;
                            filterGraphManager = new FilgraphManager();
                            filterGraphManager.RenderFile( filename );
                            mediaPosition = filterGraphManager as IMediaPosition;
                            string formattedFilename = Path.GetFileNameWithoutExtension( filename );
                            if ( _args.Scenalyzer )
                            {
                                formattedFilename = ScenalyzerFormat( formattedFilename );
                            }
                            fileCount++;
                            FrameInfo Start = new FrameInfo();
                            Start.FrameRate = _args.FrameRate;
                            Start.Duration = totalFrameInfo.Duration;
                            Start.Duration += SECONDS_DELAY_BEFORE_SUBTITLE_DISPLAY;
                            FrameInfo End = new FrameInfo();
                            End.FrameRate = _args.FrameRate;
                            End.Duration = Start.Duration + _args.SubDuration;

                            bool writeMarkers = (_args.IncludeDuplicates || previousFormattedFilename != formattedFilename);

                            if ( writeMarkers )
                            {
                                chapterWriter.WriteChapter( totalFrameInfo );
                            }
                            totalFrameInfo.Duration += mediaPosition.Duration;

                            if ( writeMarkers )
                            {
                                if ( End.Duration > totalFrameInfo.Duration )
                                {
                                    End.Duration = totalFrameInfo.Duration - (1 / _args.FrameRate);
                                }

                                subtitleWriter.WriteSubtitle( formattedFilename, subCount, Start, End );
                            }
                            subCount++;
                            previousFormattedFilename = formattedFilename;

                            empty = false;
                            if ( mediaPosition != null ) mediaPosition = null;
                            if ( filterGraphManager != null ) filterGraphManager = null;
                        }
                        catch ( SecurityException ex )
                        {
                            UpdateStatus( $"File, '{filename}' was unable to be opened due to a security exception: {ex.Message}" );
                        }
                        catch ( FileNotFoundException )
                        {
                            UpdateStatus( $"File, '{filename}' was not found" );
                        }
                    }
                    totalFrameInfo.Duration -= (1 /_args.FrameRate);
                    chapterWriter.WriteChapter( totalFrameInfo );
                    if ( empty == true )
                    {
                        success = false;
                        UpdateStatus( "No matches found!" );
                    }
                    else
                    {
                        UpdateStatus( $"Processed {fileCount} Files!" );
                    }
                }
            }

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
        
        private void UpdateStatus( string message )
        {
            if ( Status != null )
            {
                Status( this, new StatusEventHandler( message ) );
            }
        }
    }
}

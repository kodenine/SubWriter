using Subwriter.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subwriter
{
    public abstract class BaseSubtitleWriter : IDisposable
    {
        private string _filename;

        private StreamWriter _subtitleStreamWriter;
        protected StreamWriter SubtitleStreamWriter
        {
            get
            {
                if ( _subtitleStreamWriter == null )
                {
                    CorrectFileExtension();
                    FileInfo chapterFile = new FileInfo( _filename );
                    _subtitleStreamWriter = chapterFile.CreateText();
                    AddPrefix();
                }

                return _subtitleStreamWriter;
            }
        }

        public string FileExtension { get; set; }

        public string Prefix { get; set; } = String.Empty;

        public BaseSubtitleWriter( string filename )
        {
            _filename = filename;
        }

        public abstract void WriteSubtitle( string formattedFilename, int subNumber, FrameInfo start, FrameInfo end );

        private void CorrectFileExtension()
        {
            if ( Path.HasExtension( _filename ) )
            {
                _filename = _filename.Replace(
                    Path.GetExtension( _filename ),
                    FileExtension );
            }
            else
            {
                _filename += FileExtension;
            }
        }

        private void AddPrefix()
        {
            if ( !String.IsNullOrWhiteSpace( Prefix ) )
            {
                _subtitleStreamWriter.WriteLine( Prefix );
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose( bool disposing )
        {
            if ( !disposedValue )
            {
                if ( disposing )
                {
                    // dispose managed state (managed objects).
                    if ( _subtitleStreamWriter != null )
                    {
                        _subtitleStreamWriter.Close();
                        _subtitleStreamWriter = null;
                    }
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                disposedValue = true;
            }
        }

        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BaseSubtitleWriter() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose( true );
            // uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    public class StandardSubtitleWriter : BaseSubtitleWriter
    {
        public StandardSubtitleWriter( string filename ) : base( filename )
        {
            FileExtension = ".txt";
        }

        public override void WriteSubtitle( string formattedFilename, int subNumber, FrameInfo start, FrameInfo end )
        {
            SubtitleStreamWriter.WriteLine( "FileName: " + formattedFilename + "\t" );
            SubtitleStreamWriter.WriteLine( String.Format( "Duration: {0:00}:{1:00}:{2:00}.{3:000} to \r\n",
                start.Hour, start.Minute, start.Second, start.MilliSecond ) );
        }
    }

    public class SubripSubtitleWriter : BaseSubtitleWriter
    {
        public SubripSubtitleWriter( string filename ) : base( filename )
        {
            FileExtension = ".srt";
        }

        public override void WriteSubtitle( string formattedFilename, int subNumber, FrameInfo start, FrameInfo end )
        {
            SubtitleStreamWriter.WriteLine( String.Format( "{0}", subNumber ) );
            SubtitleStreamWriter.WriteLine( String.Format( "{0:00}:{1:00}:{2:00},{3:000} --> {4:00}:{5:00}:{6:00},{7:000}",
                start.Hour, start.Minute, start.Second, start.MilliSecond,
                end.Hour, end.Minute, end.Second, end.MilliSecond ) );
            SubtitleStreamWriter.WriteLine( String.Format( "{0}", formattedFilename ) );
            SubtitleStreamWriter.WriteLine( "" );
        }
    }

    public class SubviewerSubtitleWriter : BaseSubtitleWriter
    {
        public SubviewerSubtitleWriter( string filename ) : base( filename )
        {
            FileExtension = ".sub";
        }

        public override void WriteSubtitle( string formattedFilename, int subNumber, FrameInfo start, FrameInfo end )
        {
            throw new NotImplementedException();
        }
    }

    public class MicroDvdSubtitleWriter : BaseSubtitleWriter
    {
        public MicroDvdSubtitleWriter( string filename ) : base( filename )
        {
            FileExtension = ".mdvd";
        }

        public override void WriteSubtitle( string formattedFilename, int subNumber, FrameInfo start, FrameInfo end )
        {
            throw new NotImplementedException();
        }
    }

    public class SpruceSubtitleWriter : BaseSubtitleWriter
    {
        public SpruceSubtitleWriter( string filename ) : base( filename )
        {
            FileExtension = ".stl";
            Prefix = Settings.Default.SpruceSubtitlePrefix;
        }

        public override void WriteSubtitle( string formattedFilename, int subNumber, FrameInfo start, FrameInfo end )
        {
            SubtitleStreamWriter.WriteLine( String.Format( "{0:00}:{1:00}:{2:00}:{3:00},{4:00}:{5:00}:{6:00}:{7:00},{8}",
                start.Hour, start.Minute, start.Second, (start.MilliSecond / 10),
                end.Hour, end.Minute, end.Second, (end.MilliSecond / 10), formattedFilename ) );
        }
    }

    public class EncoreSubtitleFormat : BaseSubtitleWriter
    {
        public EncoreSubtitleFormat( string filename ) : base( filename )
        {
            FileExtension = ".txt";
        }

        public override void WriteSubtitle( string formattedFilename, int subNumber, FrameInfo start, FrameInfo end )
        {
            SubtitleStreamWriter.WriteLine( String.Format( "{0:00}:{1:00}:{2:00}:{3:00} {4:00}:{5:00}:{6:00}:{7:00} {8}",
                start.Hour, start.Minute, start.Second, (start.MilliSecond / 10),
                end.Hour, end.Minute, end.Second, (end.MilliSecond / 10), formattedFilename ) );
        }
    }
    
}

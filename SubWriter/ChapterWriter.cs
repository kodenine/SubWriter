using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subwriter
{    
    public abstract class BaseChapterWriter : IDisposable
    {
        private string _filename;

        private StreamWriter _chapterStreamWriter;
        protected StreamWriter ChapterStreamWriter
        {
            get
            {
                if ( _chapterStreamWriter == null )
                {
                    CorrectFileExtension();

                    FileInfo chapterFile = new FileInfo( _filename );
                    _chapterStreamWriter = chapterFile.CreateText();
                    AddPrefix();
                }

                return _chapterStreamWriter;
            }
        }


        public string FileExtension { get; protected set; }

        public BaseChapterWriter( string filename )
        {
            _filename = filename;
        }
        
        public abstract void WriteChapter( FrameInfo currentFrame );

        protected virtual void AddPrefix()
        { }
        
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose( bool disposing )
        {
            if ( !disposedValue )
            {
                if ( disposing )
                {
                    // dispose managed state (managed objects).
                    if ( _chapterStreamWriter != null )
                    {
                        _chapterStreamWriter.Close();
                        _chapterStreamWriter = null;
                    }
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                disposedValue = true;
            }
        }

        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BaseChapterWriter() {
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

    public class IfoChapterWriter : BaseChapterWriter
    {
        public IfoChapterWriter( string filename ) : base( filename )
        {
            FileExtension = ".txt";
        }

        public override void WriteChapter( FrameInfo currentFrame )
        {
            ChapterStreamWriter.WriteLine( String.Format( "{0}", currentFrame.FrameNumber ) );
        }
    }

    public class SpruceMaestroChapterWriter : BaseChapterWriter
    {
        public SpruceMaestroChapterWriter( string filename ) : base( filename )
        {
            FileExtension = ".chp";
        }

        protected override void AddPrefix()
        {
            ChapterStreamWriter.WriteLine( "$Spruce_IFrame_List\r\n" );
        }

        public override void WriteChapter( FrameInfo currentFrame )
        {
            ChapterStreamWriter.WriteLine( String.Format( "{0:00}:{1:00}:{2:00}:{3:00}",
                currentFrame.Hour, currentFrame.Minute, currentFrame.Second, (currentFrame.MilliSecond / 10) ) );
        }
    }
}

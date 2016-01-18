using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subwriter
{    
    public abstract class BaseChapterFormat
    {
        public string FileExtension { get; protected set; }

        public virtual StreamWriter CreateFile( string filename )
        {
            FileInfo chapterFile = new FileInfo( filename );
            StreamWriter chapterFileStream = chapterFile.CreateText();

            return chapterFileStream;
        }
    }

    public class IfoChapterFormat : BaseChapterFormat
    {
        public IfoChapterFormat()
        {
            FileExtension = ".txt";
        }
    }

    public class SpruceMaestroChapterFormat : BaseChapterFormat
    {
        public SpruceMaestroChapterFormat()
        {
            FileExtension = ".chp";
        }
    }
}

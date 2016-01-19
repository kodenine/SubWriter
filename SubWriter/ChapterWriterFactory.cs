using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subwriter
{
    public interface IChapterWriterFactory
    {
        BaseChapterWriter Create( string filename );
    }

    public class ChapterWriterFactory<T> : IChapterWriterFactory where T : BaseChapterWriter
    {
        public BaseChapterWriter Create( string filename )
        {
            return (BaseChapterWriter)Activator.CreateInstance( typeof( T ), filename );
        }
    }
}

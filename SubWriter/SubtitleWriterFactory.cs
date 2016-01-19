using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subwriter
{
    public interface ISubtitleWriterFactory
    {
        BaseSubtitleWriter Create( string filename );
    }

    public class SubtitleWriterFactory<T> : ISubtitleWriterFactory where T : BaseSubtitleWriter
    {
        public BaseSubtitleWriter Create( string filename )
        {
            return (BaseSubtitleWriter)Activator.CreateInstance( typeof( T ), filename );
        }
    }

}

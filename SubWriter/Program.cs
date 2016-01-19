using System;
using System.Collections;
using System.IO;
using System.Security;
using CommandLine.Utility;
using System.Text.RegularExpressions;

using QuartzTypeLib;
using System.Reflection;

namespace Subwriter
{
	/// <summary>
	/// Main
	/// </summary>
	class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		// [STAThread]
		static void Main(string[] args)
		{
            string versionNumber = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine( $"subwriter v{versionNumber}" );

            // Command line parsing
            Arguments commandLineArgs = new Arguments(args);

            ArgumentParser subWriterArgumentParser = new ArgumentParser( commandLineArgs );

            var subWriterArgumentParserResult = subWriterArgumentParser.Validate();

            if ( !subWriterArgumentParserResult.Success )
            {
                Console.WriteLine( "Invalid arguments: " + subWriterArgumentParserResult.ValidationMessage );
                PrintHelp();
            }

            var subWriterArguments = subWriterArgumentParser.Parse();

            switch ( subWriterArguments.Action )
            {
                case SubtitleArguments.ActionType.Process:
                    ProcessArguments( subWriterArguments );
                    break;

                default:
                    PrintHelp();
                    break;
            }
		}

        private static void ProcessArguments( SubtitleArguments subWriterArguments )
        {
            Console.WriteLine( $"Processing {subWriterArguments.Filenames.Count} file paths" );
            Console.WriteLine( $"Writing chapter file to '{subWriterArguments.ChapterFileName}'" );
            Console.WriteLine( $"Writing subtitle file to '{subWriterArguments.SubtitleFileName}'" );

            if ( subWriterArguments.Scenalyzer )
            {
                Console.WriteLine( "Using scenalyzer file format" );
            }

            VideoProcessor subwriter = new VideoProcessor( subWriterArguments );
            subwriter.Status += ConsoleWriteStatus;
            bool success = subwriter.Process();
            subwriter.Status -= ConsoleWriteStatus;

            if ( success )
            {
                Console.WriteLine( "process completed successfully!" );
            }
            else
            {
                Console.WriteLine( "process was unsuccessful!" );
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine( "Usage: subwriter [/h|/H]" );
            Console.WriteLine( "       subwriter [/r] [/fr:(*NTSC,PAL,##)] [/sfn:(*subtitle.txt)]" );
            Console.WriteLine( "                 [/cfn:(*chapters.txt)] [/sub:(*str,sv,mdvd,stl,txt)]" );
            Console.WriteLine( "                 [/chp:(*ifo,sm)] [/spf:(*auto,filename)]" );
            Console.WriteLine( "                 [/scenalyzer] /F:files" );
        }

        private static void ConsoleWriteStatus( VideoProcessor m, VideoProcessor.StatusEventHandler e )
        {
            Console.WriteLine( e.Message );
        }
	}
}

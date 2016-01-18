using System;
using System.Collections;
using System.IO;
using System.Security;
using CommandLine.Utility;
using System.Text.RegularExpressions;

using QuartzTypeLib;

namespace Subwriter
{
	/// <summary>
	/// Main
	/// </summary>
	class Program
	{
		//Print Help
		private static void PrintHelp()
		{
			Console.WriteLine("Usage: subwriter [/h|/H]");
			Console.WriteLine("       subwriter [/r] [/fr:(*NTSC,PAL,##)] [/sfn:(*subtitle.txt)]"); 
			Console.WriteLine("                 [/cfn:(*chapters.txt)] [/sub:(*str,sv,mdvd,stl,txt)]");
			Console.WriteLine("                 [/chp:(*ifo,sm)] [/spf:(*auto,filename)]");
			Console.WriteLine("                 [/scenalyzer] /F:files");
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		// [STAThread]
		static void Main(string[] args)
		{
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
                    ConsoleSubWriter subwriter = new ConsoleSubWriter();
                    subwriter.Process();
                    break;

                case default:
                    PrintHelp();
                    break;
            }
		}
	}
}

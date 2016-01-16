using System;
using System.IO;

// Usage: chapvert [/h|/H]
//        chapvert inchapter.txt outchapter.chp
// This program converts ifo chapters to Spruce Maestro Chapters


namespace chapvert_ns
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class ConsoleChapVert
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			int count = (args.GetUpperBound(0) - args.GetLowerBound(0)) + 1;
			if(count != 2)
			{
				Console.WriteLine("Sent {0} arguments, 2 required!",count);
				PrintHelp();
				return;
			}
			string IFOChapters = args[0];
			string MaestroChapters = args[1];

			FileInfo IFOFile = new FileInfo(IFOChapters);
			StreamReader IFOFS = IFOFile.OpenText();

			FileInfo MaestroFile = new FileInfo(MaestroChapters);
			StreamWriter MaestroFS = MaestroFile.CreateText();

			MaestroFS.WriteLine("$Spruce_IFrame_List\r\n");

			String line;

			while ((line = IFOFS.ReadLine()) != null) 
			{
				int frames = Convert.ToInt32(line);
				double seconds = (double)frames / 30;
				DateTime ChapterPoint = DateTime.MinValue;
				ChapterPoint = ChapterPoint.AddSeconds(seconds);
				// DateTime ChapterPoint = Convert.ToDateTime(seconds);
				MaestroFS.WriteLine("{0:HH}:{0:mm}:{0:ss}:{0:ff}",ChapterPoint);
				Console.WriteLine("{0:HH}:{0:mm}:{0:ss}:{0:ff}",ChapterPoint);
			}

			IFOFS.Close();
			MaestroFS.Close();

		}

		private static void PrintHelp()
		{
			Console.WriteLine("Usage: chapvert [/h|/H]");
			Console.WriteLine("       chapvert inchapter.txt outchapter.chp"); 
		}

	}
}

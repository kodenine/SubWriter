using System;
using System.Collections;
using System.IO;
using System.Security;
using CommandLine.Utility;
using System.Text.RegularExpressions;

using QuartzTypeLib;

// subwriter [/h|/H] - Usage Help
//
// subwriter [/r] [/fr:(*NTSC,PAL,##)] [/sfn:(*subtitle.txt)]
//           [/cfn:(*chapters.txt)] [/sub:(*str,sv,mdvd,stl,txt)]
//           [/chp:(*ifo,sm)] [/spf:(*auto,filename)] [/scenalyzer]
//           /F:files
//
// /n - prefix each line of output with line number;
// /r - recursive search in subdirectories;
// /sub - output of subtitle.  Default is SubRip(srt).  Output options are:
//    --> sv - SubViewer Format:
//        Example
//        [INFORMATION] 
//        [TITLE]xxxxxxxxxx 
//        [AUTHOR]xxxxxxxx 
//        [SOURCE]xxxxxxxxxxxxxxxx 
//        [FILEPATH] 
//        [DELAY]0 
//        [COMMENT] 
//        [END INFORMATION] 
//        [SUBTITLE] 
//        [COLF]&HFFFFFF,[STYLE]bd,[SIZE]18,[FONT]Arial 
//        00:00:41.00,00:00:44.40 
//        The Age of Gods was closing.
//        Eternity had come to an end. 
//
//        00:00:55.00,00:00:58.40 
//        The heavens shook as the armies 
//        of Falis, God of Light... 
//     
//	  --> srt - (Default) Output Subrip Format:
//        Example
//        1 00:02:26,407 --> 00:02:31,356 
//        Detta handlar om min storebrors 
//        kriminella beteende och foersvinnade. 
// 
//        2 00:02:31,567 --> 00:02:37,164 
//        Vi talar inte laengre om Wade. Det aer 
//        som om han aldrig hade existerat
//    --> mdvd - Output Micro-DVD Format:
//        Example
//        {1025}{1110}The Age of Gods was closing.|Eternity had come to an end.
//        {1375}{1460}The heavens shook as the armies|of Falis, God of Light...
//    --> stl - Output to Spruce Subtitles Format:
//        Example
//        //Font select and font size
//        $FontName       = Arial
//        $FontSize       = 30
//        
//        //Character attributes (global)
//        $Bold           = FALSE
//        $UnderLined     = FALSE
//        $Italic         = FALSE
//        
//        //Position Control
//        $HorzAlign      = Center
//        $VertAlign      = Bottom
//        $XOffset        = 0
//        $YOffset        = 0
//        
//        //Contrast Control
//        $TextContrast           = 15
//        $Outline1Contrast       = 8
//        $Outline2Contrast       = 15
//        $BackgroundContrast     = 0
//        
//        //Effects Control
//        $ForceDisplay   = FALSE
//        $FadeIn         = 0
//        $FadeOut        = 0
//        
//        //Other Controls
//        $TapeOffset          = FALSE
//        //$SetFilePathToken  = <<:>>
//        
//        //Colors
//        $ColorIndex1    = 0
//        $ColorIndex2    = 1
//        $ColorIndex3    = 2
//        $ColorIndex4    = 3
//        
//        //Subtitles
//        00:00:01:00,00:00:05:15,20030704 Night of the 4th of July
//        00:00:06:00,00:00:10:15,20030704 Night of the 4th of July
//    --> txt - Encore File format
//        00:00:00:00 00:00:01:00 Test 1
// /chp - Chapter Format:
//    --> ifo:
//        0
//        1000
//    --> sm (Spruce Maestro) Format:
//        $Spruce_IFrame_List
//
//        00:00:00:00
//        00:04:56:14
//        00:11:42:09
// /cpf - Subtitle Prefix.  Either Auto (progam puts in what's needed) or a file with the prefix
//        definitions in it
// /fr - frame rate at which the subtitles are calculated from.  NTSC = 30, PAL = 25.
//       Or you can specify a number.
// /sfn - Subtitles File Name aka the name of the file with the subtitles in it
// /scenalyzer - Format Date and Caption from scenalyzer files.
// /cfn - Chapters File Name aka the name of the file with the chapters in it
//
// /F:files - the list of input files. The files can be separated by commas as in /F:file1,file2,file3
//and wildcards can be used for their specification as in /F:*file?.txt;
//
//Example:
//
// subwriter /F:*.avi

namespace subwriter_ns
{

	/// <summary>
	/// FrameInfo describes an instance of a frame
	/// </summary>
	class FrameInfo
	{
		private double m_fDuration;
		private double m_dFrameRate;

		public double FrameRate
		{
			get { return m_dFrameRate; }
			set { m_dFrameRate = value; }
		}
		public double Duration 
		{
			get { return m_fDuration; }
			set { m_fDuration = value; }
		}
		public int FrameNumber
		{
			get
			{ 
				double frame = m_fDuration * m_dFrameRate; 
				return (int)frame; 
			}
		}
		public int Hour
		{
			get {return ((int)m_fDuration) / 3600; } 
		}
		public int Minute 
		{
			get { return (((int)m_fDuration)  - (Hour * 3600)) / 60; } 
		}
		public int Second 
		{
			get { return (int)m_fDuration - (Hour * 3600 + Minute * 60); } 
		}
		public int MilliSecond
		{ 
			get 
			{
				double diff = (m_fDuration - (int)m_fDuration) * 1000; 
				return (int)diff;
			} 
		}

	}

	/// <summary>
	/// Main and search pocedures contained within ConsoleSubWriter
	/// </summary>
	class ConsoleSubWriter
	{
		
		//Option Flags
		const uint c_iSF_STANDARD = 0;
		const uint c_iSF_SUBRIP = 1;
		const uint c_iSF_SUBVIEWER = 2;
		const uint c_iSF_MICRODVD = 3;
		const uint c_iSF_SPRUCE = 4;
		const uint c_iSF_ENCORE = 5;
		const double c_dFR_NTSC = 29.97;
		const double c_dFR_PAL = 25;
		private string[] m_strSF_extention = new string[] {".txt",".srt",".sub",".mdvd",".stl",".txt"};
		const uint c_iCF_IFO = 0;
		const uint c_iCF_SpruceMaestro = 1;
		private string[] m_strCF_extention = new string[] {".txt",".chp"};
		private bool m_bRecursive;
		private bool m_bScenalyzer;
		private string m_strFiles;
		private string m_strSubtitlePrefix;
		private uint m_iSubtitleFormat;
		private double m_dFrameRate;
		private double m_dSubDuration;
		private string m_strSubtitleFileName;
		private string m_strChapterFileName;
		private uint m_iChapterFormat;
		//ArrayList keeping the Files
		private ArrayList m_arrFiles = new ArrayList();

		/* public class myReverserClass : IComparer  
		{

			// Calls CaseInsensitiveComparer.Compare with the parameters reversed.
			int IComparer.Compare( Object x, Object y )  
			{
				return( (new CaseInsensitiveComparer()).Compare( y, x ) );
			}

		} */

		//Properties
		public string ChapterFileExtention(uint ChapterFormat)
		{
			return m_strCF_extention[ChapterFormat];
		}
		public string SubtitleFileExtention(uint SubtitleFormat)
		{
			return m_strSF_extention[SubtitleFormat];
		}
		public bool Recursive
		{
			get { return m_bRecursive; }
			set { m_bRecursive = value; }
		}
		public bool Scenalyzer
		{
			get { return m_bScenalyzer; }
			set { m_bScenalyzer = value; }
		}
		public string Files
		{
			get { return m_strFiles; }
			set { m_strFiles = value; }
		}
		public uint SubtitleFormat
		{
			get { return m_iSubtitleFormat; }
			set { m_iSubtitleFormat = value; }
		}
		public uint ChapterFormat
		{
			get { return m_iChapterFormat; }
			set { m_iChapterFormat = value; }
		}
		public string SubtitlePrefix
		{
			get { return m_strSubtitlePrefix; }
			set { m_strSubtitlePrefix = value; }
		}
		public double FrameRate
		{
			get { return m_dFrameRate; }
			set { m_dFrameRate = value; }
		}
		public double SubDuration
		{
			get { return m_dSubDuration; }
			set { m_dSubDuration = value; }
		}
		public string SubtitleFileName
		{
			get { return m_strSubtitleFileName; }
			set { m_strSubtitleFileName = value; }
		}
		public string ChapterFileName
		{
			get { return m_strChapterFileName; }
			set { m_strChapterFileName = value; }
		}
		//Build the list of Files
		private void GetFiles(String strDir, String strExt, bool bRecursive)
		{
			//search pattern can include the wild characters '*' and '?'
			string[] fileList = Directory.GetFiles(strDir, strExt);
			for(int i=0; i<fileList.Length; i++)
			{
				if(File.Exists(fileList[i]))
					m_arrFiles.Add(fileList[i]);
			}
			if(bRecursive==true)
			{
				//Get recursively from subdirectories
				string[] dirList = Directory.GetDirectories(strDir);
				for(int i=0; i<dirList.Length; i++)
				{
					GetFiles(dirList[i], strExt, true);
				}
			}
		}

		// Format Scenalyzer named files
		private string ScenalyzerFormat(string ScenalyzerFileName)
		{
			string processing = ScenalyzerFileName.Replace("scene'","");
			processing = processing.Replace("_joined","");
			string year = processing.Substring(0,4);
			string month = processing.Substring(4,2);
			string day = processing.Substring(6,2);
			string mytime = processing.Substring(9,8);
			mytime = mytime.Replace(".",":");
			if(Regex.Match(mytime,"##:##:##").Success == false)
			{
				mytime = "00:00:00";
			}
			string title = "";
			if(processing.Length > 17)
			{
				title = " - "+processing.Substring(17);
			}
			// string myDateTimeValue = "2/16/1992 12:15:12";
			string myDateTimeValue = month+"/"+day+"/"+year+" "+mytime;
			DateTime myDateTime = Convert.ToDateTime(myDateTimeValue);
			processing = String.Format("{0:MMMM} {0:dd}, {0:yyyy}{1}",myDateTime,title);
			return processing;
		}

		// Write Chapter
		private void WriteChapter(StreamWriter ChapterFileStream, FrameInfo CurrentFrame)
		{
			switch(m_iChapterFormat)
			{
				case c_iCF_IFO:
					ChapterFileStream.WriteLine(String.Format("{0}",CurrentFrame.FrameNumber));
					break;
				case c_iCF_SpruceMaestro:
					ChapterFileStream.WriteLine(String.Format("{0:00}:{1:00}:{2:00}:{3:00}",
						CurrentFrame.Hour,CurrentFrame.Minute,CurrentFrame.Second,(CurrentFrame.MilliSecond/10)));
					break;
			}
		}

		//Search Function
		public void Search()
		{
			String strDir = Environment.CurrentDirectory;
			//First empty the list
			m_arrFiles.Clear();
			//Create recursively a list with all the files complying with the criteria
			String[] astrFiles = m_strFiles.Split(new Char[] {','});
			for(int i=0; i<astrFiles.Length; i++)
			{
				//Eliminate white spaces
				astrFiles[i] = astrFiles[i].Trim();
				GetFiles(strDir, astrFiles[i], m_bRecursive);
			}
			//Now all the Files are in the ArrayList, open each one
			//iteratively and look for the search string
			FileInfo SubtitleFile = new FileInfo(m_strSubtitleFileName);
			StreamWriter SubtitleFS = SubtitleFile.CreateText();
			FileInfo ChapterFile = new FileInfo(m_strChapterFileName);
			StreamWriter ChapterFS = ChapterFile.CreateText();
			
			switch(m_iChapterFormat)
			{
				case c_iCF_IFO:
					break;
				case c_iCF_SpruceMaestro:
					ChapterFS.WriteLine("$Spruce_IFrame_List\r\n");
					break;
			}
			if(m_strSubtitlePrefix == "auto")
			{
				switch(m_iSubtitleFormat)
				{
					case c_iSF_SPRUCE:
						SubtitleFS.WriteLine("//Font select and font size");
						SubtitleFS.WriteLine("$FontName       = Arial");
						SubtitleFS.WriteLine("$FontSize       = 30");
						SubtitleFS.WriteLine();
						SubtitleFS.WriteLine("//Character attributes (global)");
						SubtitleFS.WriteLine("$Bold           = FALSE");
						SubtitleFS.WriteLine("$UnderLined     = FALSE");
						SubtitleFS.WriteLine("$Italic         = FALSE");
						SubtitleFS.WriteLine();
						SubtitleFS.WriteLine("//Position Control");
						SubtitleFS.WriteLine("$HorzAlign      = Center");
						SubtitleFS.WriteLine("$VertAlign      = Bottom");
						SubtitleFS.WriteLine("$XOffset        = 0");
						SubtitleFS.WriteLine("$YOffset        = 0");
						SubtitleFS.WriteLine();
						SubtitleFS.WriteLine("//Contrast Control");
						SubtitleFS.WriteLine("$TextContrast           = 15");
						SubtitleFS.WriteLine("$Outline1Contrast       = 8");
						SubtitleFS.WriteLine("$Outline2Contrast       = 15");
						SubtitleFS.WriteLine("$BackgroundContrast     = 0");
						SubtitleFS.WriteLine();
						SubtitleFS.WriteLine("//Effects Control");
						SubtitleFS.WriteLine("$ForceDisplay   = FALSE");
						SubtitleFS.WriteLine("$FadeIn         = 10");
						SubtitleFS.WriteLine("$FadeOut        = 10");
						SubtitleFS.WriteLine();
						SubtitleFS.WriteLine("//Other Controls");
						SubtitleFS.WriteLine("$TapeOffset          = FALSE");
						SubtitleFS.WriteLine("//$SetFilePathToken  = <<:>>");
						SubtitleFS.WriteLine();
						SubtitleFS.WriteLine("//Colors");
						SubtitleFS.WriteLine("$ColorIndex1    = 0");
						SubtitleFS.WriteLine("$ColorIndex2    = 1");
						SubtitleFS.WriteLine("$ColorIndex3    = 2");
						SubtitleFS.WriteLine("$ColorIndex4    = 3");
						SubtitleFS.WriteLine();
						SubtitleFS.WriteLine("//Subtitles");
						break;
				}
			}else{
				try
				{
					FileInfo SubtitlePrefix = new FileInfo(m_strSubtitlePrefix);
					StreamReader SubtitlePrefixFS = SubtitlePrefix.OpenText();
					SubtitleFS.Write(SubtitlePrefixFS.ReadToEnd());
					SubtitlePrefixFS.Close();
				}
				catch(FileNotFoundException)
				{
					Console.WriteLine("Subtitle Prefix File ({0}) not found!",m_strSubtitlePrefix);
					PrintHelp();
					return;
				}
			}

			String strResults = "";
			int FileCount = 0;
			int SubCount = 1;
			bool bEmpty = true;
			FrameInfo Total = new FrameInfo();
			Total.FrameRate = m_dFrameRate;
			Total.Duration = 0;
			// Sorts the values of the ArrayList using the reverse case-insensitive comparer.
			// IComparer myComparer = new myReverserClass();
			// m_arrFiles.Sort( 1, 3, myComparer );
			m_arrFiles.Sort();
			IEnumerator enm = m_arrFiles.GetEnumerator();
			while(enm.MoveNext())
			{
				try
				{
					// Open: (string)enm.Current;
					FilgraphManager m_objFilterGraph = null;
					IMediaPosition m_objMediaPosition = null;
					m_objFilterGraph = new FilgraphManager();
					m_objFilterGraph.RenderFile((string)enm.Current);
					m_objMediaPosition = m_objFilterGraph as IMediaPosition;
					string filename = Path.GetFileNameWithoutExtension((string)enm.Current);
					if(m_bScenalyzer)
						filename = ScenalyzerFormat(filename);
					FileCount++;
					FrameInfo Start = new FrameInfo();
					Start.FrameRate = m_dFrameRate;
					Start.Duration = Total.Duration;
					Start.Duration += 1;
					FrameInfo End = new FrameInfo();
					End.FrameRate = m_dFrameRate;
					End.Duration = Start.Duration + m_dSubDuration - 0.5;

					/* switch(m_iChapterFormat)
					{
						case c_iCF_IFO:
							ChapterFS.WriteLine(String.Format("{0}",Total.FrameNumber));
							break;
						case c_iCF_SpruceMaestro:
							ChapterFS.WriteLine(String.Format("{0:00}:{1:00}:{2:00}:{3:00}",
												Total.Hour,Total.Minute,Total.Second,(Total.MilliSecond/10)));
							break;
					} */
					WriteChapter(ChapterFS, Total);
					Total.Duration += m_objMediaPosition.Duration - 2;

					if(End.Duration > Total.Duration)
					{
						End.Duration = Total.Duration - 0.1;
					}

					// while (End.Duration < Total.Duration)
					// {
					switch(m_iSubtitleFormat) 
					{
						case c_iSF_STANDARD:
							SubtitleFS.WriteLine("FileName: " + (string)enm.Current + "\t");
							SubtitleFS.WriteLine(String.Format("Duration: {0:00}:{1:00}:{2:00}.{3:000} to \r\n",
								Start.Hour,Start.Minute,Start.Second,Start.MilliSecond));
							break;
						case c_iSF_SUBRIP:
							SubtitleFS.WriteLine(String.Format("{0}",SubCount));
							SubtitleFS.WriteLine(String.Format("{0:00}:{1:00}:{2:00},{3:000} --> {4:00}:{5:00}:{6:00},{7:000}",
								Start.Hour,Start.Minute,Start.Second,Start.MilliSecond,
								End.Hour,End.Minute,End.Second,End.MilliSecond));
							SubtitleFS.WriteLine(String.Format("{0}",filename));
							SubtitleFS.WriteLine("");
							break;
						case c_iSF_SPRUCE:
							SubtitleFS.WriteLine(String.Format("{0:00}:{1:00}:{2:00}:{3:00},{4:00}:{5:00}:{6:00}:{7:00},{8}",
								Start.Hour,Start.Minute,Start.Second,(Start.MilliSecond/10),
								End.Hour,End.Minute,End.Second,(End.MilliSecond/10),filename));
							break;
						case c_iSF_ENCORE:
							SubtitleFS.WriteLine(String.Format("{0:00}:{1:00}:{2:00}:{3:00} {4:00}:{5:00}:{6:00}:{7:00} {8}",
								Start.Hour,Start.Minute,Start.Second,(Start.MilliSecond/10),
								End.Hour,End.Minute,End.Second,(End.MilliSecond/10),filename));
							break;
					}
					//	Start.Duration += m_dSubDuration;
					//	End.Duration = Start.Duration + m_dSubDuration - 0.5;
					SubCount++;
					// }
					// Process file for frames and duration
					bEmpty = false;
					if (m_objMediaPosition != null) m_objMediaPosition = null;
					if (m_objFilterGraph != null) m_objFilterGraph = null;
				}
				catch(SecurityException)
				{
					strResults += "\r\n" + (string)enm.Current + ": Security Exception\r\n\r\n";	
				}
				catch(FileNotFoundException)
				{
					strResults += "\r\n" + (string)enm.Current + ": File Not Found Exception\r\n";
				}
			}
			Total.Duration -= 0.5;
			WriteChapter(ChapterFS, Total);
			if(bEmpty == true)
				Console.WriteLine("No matches found!");
			else
				Console.WriteLine(String.Format("Processed {0} Files!\r\n",FileCount));
				Console.WriteLine(strResults);
			SubtitleFS.Close();
			ChapterFS.Close();
		}

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
			Arguments CommandLine = new Arguments(args);
			if(CommandLine["h"] != null || CommandLine["H"] != null)
			{
				PrintHelp();
				return;
			}
			// Console.WriteLine();
			// The working object
			ConsoleSubWriter subwriter = new ConsoleSubWriter();
			if(CommandLine["sfn"] != null)
				subwriter.SubtitleFileName = (string)CommandLine["sfn"];
			else
				subwriter.SubtitleFileName = String.Format("subtitle.srt");
			if(CommandLine["cfn"] != null)
				subwriter.ChapterFileName = (string)CommandLine["cfn"];
			else
				subwriter.ChapterFileName = String.Format("chapters.txt");
			if(CommandLine["sub"] != null) 
				switch((string)CommandLine["sub"]) 
				{
					case "str":
						subwriter.SubtitleFormat = c_iSF_SUBRIP;
						break;
					case "sv":
						goto default;
					case "mdvd":
						goto default;
					case "stl":
						subwriter.SubtitleFormat = c_iSF_SPRUCE;
						break;
					case "txt":
						subwriter.SubtitleFormat = c_iSF_ENCORE;
						break;
					default:
						Console.WriteLine("Unsupported format!\r\n");
						PrintHelp();
						return;
				}
			else
				subwriter.SubtitleFormat = c_iSF_SUBRIP;
			if(Path.HasExtension(subwriter.SubtitleFileName))
				subwriter.SubtitleFileName = subwriter.SubtitleFileName.Replace(
					Path.GetExtension(subwriter.SubtitleFileName),
					subwriter.SubtitleFileExtention(subwriter.SubtitleFormat));
			else
				subwriter.SubtitleFileName += subwriter.SubtitleFileExtention(subwriter.SubtitleFormat);

			if(CommandLine["fr"] != null)
				switch((string)CommandLine["fr"])
				{
					case "NTSC":
						subwriter.FrameRate = c_dFR_NTSC;
						break;
					case "PAL":
						subwriter.FrameRate = c_dFR_NTSC;
						break;
					default:
						try
						{
							String framerate = (string)CommandLine["fr"];
							subwriter.FrameRate = Convert.ToDouble(framerate);
							break;
						}
						catch(InvalidCastException e)
						{
							Console.WriteLine("Unsupported frame rate!\r\n");
							PrintHelp();
							return;	
						}
				}
			else
				subwriter.FrameRate = c_dFR_NTSC;
			if(CommandLine["chp"] != null)
				switch((string)CommandLine["chp"])
				{
					case "ifo":
						subwriter.ChapterFormat = c_iCF_IFO;
						break;
					case "sm":
						subwriter.ChapterFormat = c_iCF_SpruceMaestro;
						break;
					default:
						Console.WriteLine("Unsopported Chapter Format");
						PrintHelp();
						return;
				}
			else
				subwriter.ChapterFormat = c_iCF_IFO;

			if(Path.HasExtension(subwriter.ChapterFileName))
				subwriter.ChapterFileName = subwriter.ChapterFileName.Replace(
					Path.GetExtension(subwriter.ChapterFileName),
					subwriter.ChapterFileExtention(subwriter.ChapterFormat));
			else
				subwriter.ChapterFileName += subwriter.ChapterFileExtention(subwriter.ChapterFormat);
			if(CommandLine["spf"] != null)
			{
				subwriter.SubtitlePrefix = (string)CommandLine["spf"];
			}
			else
			{
				subwriter.SubtitlePrefix = "auto";
			}

			subwriter.SubDuration = 10;
			if(CommandLine["F"] != null)
				subwriter.Files = (string)CommandLine["F"];
			else
			{
				Console.WriteLine("Error: No Search Files specified!");
				Console.WriteLine();
				PrintHelp();
				return;
			}
			subwriter.Recursive = (CommandLine["r"] != null);
			subwriter.Scenalyzer = (CommandLine["scenalyzer"] != null);

			// Do the search
			subwriter.Search();
		}
	}
}

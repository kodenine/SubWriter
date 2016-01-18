 subwriter [/h|/H] - Usage Help

 Will create chapter and subtitle files for files when merged together.

 subwriter [/r] [/fr:(*NTSC,PAL,##)] [/sfn:(*subtitle.txt)]
           [/cfn:(*chapters.txt)] [/sub:(*str, sv, mdvd, stl, txt)]
[/chp:(*ifo, sm)]
[/spf:(*auto, filename)]
[/scenalyzer]
           /F:files

 /n - prefix each line of output with line number;
 /r - recursive search in subdirectories;
 /sub - output of subtitle.Default is SubRip(srt).  Output options are:
    --> sv - SubViewer Format:
        Example
        [INFORMATION]
        [TITLE] xxxxxxxxxx
        [AUTHOR]xxxxxxxx
        [SOURCE] xxxxxxxxxxxxxxxx
        [FILEPATH]
        [DELAY]0 
        [COMMENT]
[END INFORMATION]
[SUBTITLE]
[COLF]&HFFFFFF,[STYLE]
bd,[SIZE]18,[FONT]
Arial 
        00:00:41.00,00:00:44.40 
        The Age of Gods was closing.
        Eternity had come to an end.

        00:00:55.00,00:00:58.40 
        The heavens shook as the armies
        of Falis, God of Light...
     
	  --> srt - (Default) Output Subrip Format:
        Example
        1 00:02:26,407 --> 00:02:31,356 
        Detta handlar om min storebrors
        kriminella beteende och foersvinnade.
 
        2 00:02:31,567 --> 00:02:37,164 
        Vi talar inte laengre om Wade.Det aer
        som om han aldrig hade existerat
    --> mdvd - Output Micro-DVD Format:
        Example
        { 1025}{1110}The Age of Gods was closing.|Eternity had come to an end.
        { 1375}{1460}The heavens shook as the armies|of Falis, God of Light...
    --> stl - Output to Spruce Subtitles Format:
        Example
        //Font select and font size
        $FontName       = Arial
        $FontSize       = 30
        
        //Character attributes (global)
        $Bold           = FALSE
        $UnderLined     = FALSE
        $Italic         = FALSE
        
        //Position Control
        $HorzAlign      = Center
        $VertAlign      = Bottom
        $XOffset        = 0
        $YOffset        = 0
        
        //Contrast Control
        $TextContrast           = 15
        $Outline1Contrast       = 8
        $Outline2Contrast       = 15
        $BackgroundContrast     = 0
        
        //Effects Control
        $ForceDisplay   = FALSE
        $FadeIn         = 0
        $FadeOut        = 0
        
        //Other Controls
        $TapeOffset          = FALSE
        //$SetFilePathToken  = <<:>>
        
        //Colors
        $ColorIndex1    = 0
        $ColorIndex2    = 1
        $ColorIndex3    = 2
        $ColorIndex4    = 3
        
        //Subtitles
        00:00:01:00,00:00:05:15,20030704 Night of the 4th of July
        00:00:06:00,00:00:10:15,20030704 Night of the 4th of July
    --> txt - Encore File format
        00:00:00:00 00:00:01:00 Test 1
 /chp - Chapter Format:
    --> ifo:
        0
        1000
    --> sm (Spruce Maestro) Format:
        $Spruce_IFrame_List

        00:00:00:00
        00:04:56:14
        00:11:42:09
 /cpf - Subtitle Prefix.Either Auto (progam puts in what's needed) or a file with the prefix
        definitions in it
 /fr - frame rate at which the subtitles are calculated from.NTSC = 30, PAL = 25.
       Or you can specify a number.
 /sfn - Subtitles File Name aka the name of the file with the subtitles in it
 /scenalyzer - Format Date and Caption from scenalyzer files.
 /cfn - Chapters File Name aka the name of the file with the chapters in it

 /F:files - the list of input files.The files can be separated by commas as in /F:file1, file2, file3
and wildcards can be used for their specification as in /F:*file?.txt;

Example:

 subwriter /F:*.avi

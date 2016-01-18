using CommandLine.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subwriter
{
    public class ArgumentParser
    {
        public class ValidationResult
        {
            public bool Success { get; set; } = false;
            public string ValidationMessage { get; set; }
        }


        private Arguments _args;


        public ArgumentParser( Arguments args )
        {
            _args = args;
        }

        public ValidationResult Validate()
        {
            ValidationResult result = new ValidationResult();
            result.Success = true;
            result.ValidationMessage = null;

            if ( _args["h"] != null && _args["help"] != null )
            {
                string[] validSubtitleAbbrivations = new string[] { "str", "sv", "mdvd", "stl", "txt" };

                string subArg = (string)_args["sub"];
                if ( subArg != null && validSubtitleAbbrivations.Contains( subArg.ToUpper() ) )
                {
                    result.Success = false;
                    result.ValidationMessage = $"'{subArg}' is an unsupported subtitle format";
                }

                string framerateArg = (string)_args["fr"];
                if ( framerateArg != null &&
                     !framerateArg.Equals( "NTSC", StringComparison.OrdinalIgnoreCase ) &&
                     !framerateArg.Equals( "PAL", StringComparison.OrdinalIgnoreCase ) )
                {
                    double framerateParseResult;
                    if ( !Double.TryParse( framerateArg, out framerateParseResult ) )
                    {
                        result.Success = false;
                        result.ValidationMessage = $"'{framerateArg}' is not a supported frame rate";
                    }
                }

                string chapterArg = (string)_args["chp"];
                if ( chapterArg != null &&
                     !chapterArg.Equals( "ifo", StringComparison.OrdinalIgnoreCase ) &&
                     !chapterArg.Equals( "sm", StringComparison.OrdinalIgnoreCase ) )
                {
                    result.Success = false;
                    result.ValidationMessage = $"'{chapterArg} is not a supported chapter format";
                }

                if ( (string)_args["F"] == null )
                {
                    result.Success = false;
                    result.ValidationMessage = "No files were specified";
                }
            }

            return result;
        }

        public SubtitleArguments Parse()
        {
            var result = new SubtitleArguments();

            if ( _args["h"] != null || _args["help"] != null )
            {
                result.Action = SubtitleArguments.ActionType.Help;
            }
            else
            {
                result = ParseProcessArguments();
                result.Action = SubtitleArguments.ActionType.Process;
            }

            return result;
        }

        private SubtitleArguments ParseProcessArguments()
        {
            var result = new SubtitleArguments();

            if ( _args["sfn"] != null )
            {
                result.SubtitleFileName = (string)_args["sfn"];
            }
            else
            {
                result.SubtitleFileName = "subtitle.srt";
            }

            if ( _args["cfn"] != null )
            {
                result.ChapterFileName = (string)_args["cfn"];
            }
            else
            {
                result.ChapterFileName = "chapters.txt";
            }

            string subArg = (string)_args["sub"];
            if ( subArg != null )
            {
                switch ( subArg.ToLower() )
                {
                    case "str":
                        result.SubtitleFormat = SubtitleFormat.Subrip;
                        break;
                    case "sv":
                        goto default;
                    case "mdvd":
                        goto default;
                    case "stl":
                        result.SubtitleFormat = SubtitleFormat.Spruce;
                        break;
                    case "txt":
                        result.SubtitleFormat = SubtitleFormat.Encore;
                        break;
                    default:
                        throw new ArgumentException( $"'{subArg}' is not a supported subtitle format!", "sub" );
                }
            }
            else
            {
                result.SubtitleFormat = SubtitleFormat.Subrip;
            }

            if ( Path.HasExtension( result.SubtitleFileName ) )
            {
                result.SubtitleFileName = result.SubtitleFileName.Replace(
                    Path.GetExtension( result.SubtitleFileName ),
                    result.SubtitleFormat.FileExtension );
            }
            else
            {
                result.SubtitleFileName += result.SubtitleFormat.FileExtension;
            }

            string framerateArg = (string)_args["fr"];
            if ( framerateArg != null )
            {
                switch ( framerateArg.ToLower() )
                {
                    case "ntsc":
                        result.FrameRate = FrameRateType.Ntsc;
                        break;
                    case "pal":
                        result.FrameRate = FrameRateType.Pal;
                        break;
                    default:
                        double framerate;
                        if ( Double.TryParse( framerateArg, out framerate ) )
                        {
                            result.FrameRate = framerate;
                        }
                        else
                        {
                            throw new ArgumentException( $"'{framerateArg}' is not a supported frame rate", "fr" );
                        }
                        break;
                }
            }
            else
            {
                result.FrameRate = FrameRateType.Ntsc;
            }

            string chapterFormatArg = (string)_args["chp"];
            if ( chapterFormatArg != null )
            {
                switch ( chapterFormatArg )
                {
                    case "ifo":
                        result.ChapterFormat = new IfoChapterFormat();
                        break;
                    case "sm":
                        result.ChapterFormat = new SpruceMaestroChapterFormat();
                        break;
                    default:
                        throw new ArgumentException( $"'{chapterFormatArg}' is not a supported chapter format", "chp" );
                }
            }
            else
            {
                result.ChapterFormat = new IfoChapterFormat();
            }

            if ( Path.HasExtension( result.ChapterFileName ) )
            {
                result.ChapterFileName = result.ChapterFileName.Replace(
                    Path.GetExtension( result.ChapterFileName ),
                    result.ChapterFormat.FileExtension );
            }
            else
            {
                result.ChapterFileName += result.ChapterFormat.FileExtension;
            }

            if ( _args["spf"] != null )
            {
                result.SubtitlePrefix = (string)_args["spf"];
            }
            else
            {
                result.SubtitlePrefix = "auto";
            }

            result.SubDuration = 10;

            string filesArg = (string)_args["F"];
            if ( filesArg != null )
            {
                result.Filenames = new List<string>( filesArg.Split( new Char[] { ',' } ) );
            }
            else
            {
                throw new ArgumentException( "No files were specified!", "F" );
            }

            result.Recursive = (_args["r"] != null);
            result.Scenalyzer = (_args["scenalyzer"] != null);

            result.Action = SubtitleArguments.ActionType.Process;

            return result;
        }
    }
}

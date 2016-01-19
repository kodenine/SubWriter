﻿using CommandLine.Utility;
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

                string subtitlePrefixFilename = (string)_args["spf"];
                if ( !String.IsNullOrEmpty( subtitlePrefixFilename ) && 
                     !subtitlePrefixFilename.Equals( "auto", StringComparison.OrdinalIgnoreCase ) )
                {
                    FileInfo subtitlePrefixFileInfo = new FileInfo( subtitlePrefixFilename );
                    if ( !subtitlePrefixFileInfo.Exists )
                    {
                        result.Success = false;
                        result.ValidationMessage = $"Subtitle prefix filename, '{subtitlePrefixFilename}' doesn't exist";
                    }
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
                        result.SubtitleWriterFactory = new SubtitleWriterFactory<SubripSubtitleWriter>();
                        break;
                    case "sv":
                        goto default;
                    case "mdvd":
                        goto default;
                    case "stl":
                        result.SubtitleWriterFactory = new SubtitleWriterFactory<SpruceSubtitleWriter>();
                        break;
                    case "txt":
                        result.SubtitleWriterFactory = new SubtitleWriterFactory<EncoreSubtitleFormat>();
                        break;
                    default:
                        throw new ArgumentException( $"'{subArg}' is not a supported subtitle format!", "sub" );
                }
            }
            else
            {
                result.SubtitleWriterFactory = new SubtitleWriterFactory<SubripSubtitleWriter>();
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
                        result.ChapterWriterFactory = new ChapterWriterFactory<IfoChapterWriter>();
                        break;
                    case "sm":
                        result.ChapterWriterFactory = new ChapterWriterFactory<SpruceMaestroChapterWriter>();
                        break;
                    default:
                        throw new ArgumentException( $"'{chapterFormatArg}' is not a supported chapter format", "chp" );
                }
            }
            else
            {
                result.ChapterWriterFactory = new ChapterWriterFactory<IfoChapterWriter>();
            }

            if ( _args["spf"] != null )
            {
                result.SubtitlePrefixFilename = (string)_args["spf"];
            }
            else
            {
                result.SubtitlePrefixFilename = "auto";
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
            result.IncludeDuplicates = (_args["id"] != null);

            result.Action = SubtitleArguments.ActionType.Process;

            return result;
        }
    }
}

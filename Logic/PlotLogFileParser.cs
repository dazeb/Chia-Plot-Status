﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ChiaPlotStatus
{
    /**
     * Parses a log file and generates PlotLogs for each plotting process in the log file
     * (multiple PlotLogs per file if plot create --num n is used)
     * Tails the log file if it is still being written. Call Parse() before each access
     */
    public class PlotLogFileParser
    {
        private TailLineEmitter TailLineEmitter { get; }
        private List<PlotLog> PlotLogs { get; } = new List<PlotLog>();
        public string LogFile;
        public string LogFolder;

        public PlotLogFileParser(string path)
        {
            this.LogFile = path;
            this.LogFolder = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar));
            this.TailLineEmitter = new TailLineEmitter(path, (line) =>
            {
                CurrentPlotLog().IsLastLineTempError = false;
                switch (line)
                {
                    case var _ when plotSizeRg.IsMatch(line):
                        CurrentPlotLog().PlotSize = int.Parse(plotSizeRg.Matches(line)[0].Groups[1].Value);
                        break;
                    case var _ when bufferSizeRg.IsMatch(line):
                        CurrentPlotLog().Buffer = int.Parse(bufferSizeRg.Matches(line)[0].Groups[1].Value);
                        break;
                    case var _ when bucketsRg.IsMatch(line):
                        CurrentPlotLog().Buckets = int.Parse(bucketsRg.Matches(line)[0].Groups[1].Value);
                        break;
                    case var _ when currentBucketRg.IsMatch(line):
                        CurrentPlotLog().CurrentBucket = 1 + int.Parse(currentBucketRg.Matches(line)[0].Groups[1].Value);
                        break;
                    case var _ when threadsRg.IsMatch(line):
                        CurrentPlotLog().Threads = int.Parse(threadsRg.Matches(line)[0].Groups[1].Value);
                        break;
                    case var _ when startDateRg.IsMatch(line):
                        var dateTimeStr = startDateRg.Matches(line)[0].Groups[1].Value;
                        CurrentPlotLog().StartDate = DateTime.ParseExact(dateTimeStr, "ddd MMM d HH:mm:ss yyyy", new System.Globalization.CultureInfo("en-US"), DateTimeStyles.AllowWhiteSpaces);
                        break;
                    case var _ when plotNameRg.IsMatch(line):
                        CurrentPlotLog().PlotName = plotNameRg.Matches(line)[0].Groups[1].Value;
                        break;
                    case var _ when phase1Rg.IsMatch(line):
                        CurrentPlotLog().Phase1Seconds = int.Parse(phase1Rg.Matches(line)[0].Groups[1].Value);
                        break;
                    case var _ when phase1Table.IsMatch(line):
                        CurrentPlotLog().Phase1Table = int.Parse(phase1Table.Matches(line)[0].Groups[1].Value);
                        break;
                    case var _ when phase1Rg.IsMatch(line):
                        CurrentPlotLog().Phase1Seconds = int.Parse(phase1Rg.Matches(line)[0].Groups[1].Value);
                        break;
                    case var _ when phase1Table.IsMatch(line):
                        CurrentPlotLog().Phase1Table = int.Parse(phase1Table.Matches(line)[0].Groups[1].Value);
                        break;
                    case var _ when phase2Rg.IsMatch(line):
                        CurrentPlotLog().Phase2Seconds = int.Parse(phase2Rg.Matches(line)[0].Groups[1].Value);
                        break;
                    case var _ when phase2Table.IsMatch(line):
                        CurrentPlotLog().Phase2Table = int.Parse(phase2Table.Matches(line)[0].Groups[1].Value);
                        break;
                    case var _ when phase3Rg.IsMatch(line):
                        CurrentPlotLog().Phase3Seconds = int.Parse(phase3Rg.Matches(line)[0].Groups[1].Value);
                        break;
                    case var _ when phase3Table.IsMatch(line):
                        CurrentPlotLog().Phase3Table = int.Parse(phase3Table.Matches(line)[0].Groups[1].Value);
                        break;
                    case var _ when phase4Rg.IsMatch(line):
                        CurrentPlotLog().Phase4Seconds = int.Parse(phase4Rg.Matches(line)[0].Groups[1].Value);
                        break;
                    case var _ when totalTimeRg.IsMatch(line):
                        var curPlot = CurrentPlotLog();
                        curPlot.TotalSeconds = int.Parse(totalTimeRg.Matches(line)[0].Groups[1].Value);
                        if (curPlot.StartDate != null)
                            curPlot.FinishDate = ((DateTime)curPlot.StartDate).AddSeconds(curPlot.TotalSeconds);
                        break;
                    case var _ when approximateWorkingSpace.IsMatch(line):
                        CurrentPlotLog().ApproximateWorkingSpace = approximateWorkingSpace.Matches(line)[0].Groups[1].Value;
                        break;
                    case var _ when destinationDirectory.IsMatch(line):
                        CurrentPlotLog().DestDrive = destinationDirectory.Matches(line)[0].Groups[1].Value;
                        break;
                    case var _ when finalFileSize.IsMatch(line):
                        CurrentPlotLog().FinalFileSize = finalFileSize.Matches(line)[0].Groups[1].Value;
                        break;
                    case var _ when writePloblemRg.IsMatch(line):
                    case var _ when readPloblemRg.IsMatch(line):
                        CurrentPlotLog().IsLastLineTempError = true;
                        CurrentPlotLog().Errors++;
                        break;
                        break;
                    case var _ when tmpFolders.IsMatch(line):
                        var match = tmpFolders.Matches(line)[0];
                        var plotLog = CurrentPlotLog();
                        if (plotLog.Tmp1Drive != null) // this is a new plot in the same logfile (--num n used)
                        {
                            plotLog = NextPlotLog();
                        }
                        plotLog.Tmp1Drive = match.Groups[1].Value;
                        plotLog.Tmp2Drive = match.Groups[2].Value;
                        break;
                    default:
                        // Console.WriteLine($"{line}: not all upper or lower");
                        break;
                }
                var cPlotLog = CurrentPlotLog();
                cPlotLog.LogFile = this.LogFile;
                cPlotLog.LogFolder = this.LogFolder;
                cPlotLog.UpdateProgress();
            });
        }

        /**
         * Can be called as often as needed as it does not reparse what was already processed.
         */
        public List<PlotLog> ParsePlotLog()
        {
            this.TailLineEmitter.ReadMore();
            CurrentPlotLog().FileLastWritten = File.GetLastWriteTime(this.LogFile);
            return PlotLogs;
        }

        private PlotLog CurrentPlotLog()
        {
            if (PlotLogs.Count == 0)
            {
                PlotLogs.Add(new PlotLog());
            }
            return PlotLogs[PlotLogs.Count - 1];
        }

        private PlotLog NextPlotLog()
        {
            var oldPlotLog = CurrentPlotLog();
            oldPlotLog.IsLastInLogFile = false;
            var newPlotLog = new PlotLog();
            // when plot create --num n is used parameters stay the same
            newPlotLog.Buckets = oldPlotLog.Buckets;
            //newPlotLog.Threads = oldPlotLog.Threads;
            newPlotLog.Buffer = oldPlotLog.Buffer;
            newPlotLog.Tmp1Drive = oldPlotLog.Tmp1Drive;
            newPlotLog.Tmp2Drive = oldPlotLog.Tmp2Drive;
            newPlotLog.LogFile = oldPlotLog.LogFile;
            newPlotLog.LogFolder = oldPlotLog.LogFolder;
            PlotLogs.Add(newPlotLog);
            return newPlotLog;
        }


        // interesting data from logfiles as regex
        // TODO: was multiline really necessary?
        static Regex plotSizeRg = new Regex("^Plot size is: (\\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex bufferSizeRg = new Regex("^Buffer size is: (\\d+)MiB", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex bucketsRg = new Regex("^Using (\\d+) buckets", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex threadsRg = new Regex("^Using (\\d+) threads of stripe size (\\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex startDateRg = new Regex("^Starting phase 1/4: Forward Propagation into tmp files\\.\\.\\. (.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex phase1Rg = new Regex("^Time for phase 1 = (\\d+)\\.\\d+ seconds", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex phase2Rg = new Regex("^Time for phase 2 = (\\d+)\\.\\d+ seconds", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex phase3Rg = new Regex("^Time for phase 3 = (\\d+)\\.\\d+ seconds", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex phase4Rg = new Regex("^Time for phase 4 = (\\d+)\\.\\d+ seconds", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex totalTimeRg = new Regex("^Total time = (\\d+)\\.\\d+ seconds", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex plotNameRg = new Regex("^Renamed final file from \".+\" to (\".+\")", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex currentBucketRg = new Regex("^\\tBucket (\\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex phase1Table = new Regex("^Computing table (\\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex phase2Table = new Regex("^scanned table (\\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex phase3Table = new Regex("^Compressing tables (\\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex tmpFolders = new Regex("^Starting plotting progress into temporary dirs: (.*) and (.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex writePloblemRg = new Regex("^Only wrote \\d+ of \\d+ bytes at", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex readPloblemRg = new Regex("^Only read \\d+ of \\d+ bytes at", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex approximateWorkingSpace = new Regex("^Approximate working space used \\(without final file\\): (\\d+\\.\\d+ .*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex finalFileSize = new Regex("^Final File size: (\\d+\\.\\d+ .*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex destinationDirectory = new Regex("^Final Directory is: (.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Approximate working space used (without final file)
    }
}
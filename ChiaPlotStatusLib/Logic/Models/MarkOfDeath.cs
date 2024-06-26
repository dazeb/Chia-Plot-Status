﻿using ChiaPlotStatusLib.Logic.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiaPlotStatus.Logic.Models
{
    /**
     * Manually marks a PlogLog as dead so it can be hidden
     */
    public class MarkOfDeath
    {
        public string LogFolder { get; set; }
        public string LogFile { get; set; }
        public int PlaceInLogFile { get; set; }
        public DateTime? DiedAt { get; set; }

        public MarkOfDeath() { }

        public MarkOfDeath(PlotLogReadable plotLogReadable) {
            this.LogFolder = plotLogReadable.LogFolder;
            this.LogFile = plotLogReadable.LogFile;
            this.PlaceInLogFile = int.Parse(plotLogReadable.PlaceInLogFile.Split("/")[0]);
            // here and not in field definition to stay compatible to pre DiedAt versions
            this.DiedAt = DateTime.Now;
        }

        public MarkOfDeath(CPPlotLogReadable plotLogReadable)
        {
            this.LogFolder = plotLogReadable.LogFolder;
            this.LogFile = plotLogReadable.LogFile;
            this.PlaceInLogFile = 1;
            this.DiedAt = DateTime.Now;
        }

        public bool IsMatch(PlotLog plotLog)
        {
            if (!string.Equals(this.LogFolder, plotLog.LogFolder)) return false;
            string logFileName = plotLog.LogFile.Substring(plotLog.LogFile.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            if (!string.Equals(this.LogFile, logFileName)) return false;
            if (this.PlaceInLogFile != plotLog.PlaceInLogFile) return false;
            return true;
        }

        public bool IsMatch(PlotLogReadable plotLog)
        {
            if (!string.Equals(this.LogFolder, plotLog.LogFolder)) return false;
            if (!string.Equals(this.LogFile, plotLog.LogFile)) return false;
            if (!string.Equals(this.PlaceInLogFile, plotLog.PlaceInLogFile)) return false;
            return true;
        }

        public bool IsMatch(CPPlotLog plotLog)
        {
            if (!string.Equals(this.LogFolder, plotLog.LogFolder)) return false;
            string logFileName = plotLog.LogFile.Substring(plotLog.LogFile.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            if (!string.Equals(this.LogFile, logFileName)) return false;
            return true;
        }

        public bool IsMatch(CPPlotLogReadable plotLog)
        {
            if (!string.Equals(this.LogFolder, plotLog.LogFolder)) return false;
            if (!string.Equals(this.LogFile, plotLog.LogFile)) return false;
            return true;
        }

        public override bool Equals(object? obj)
        {
            return obj is MarkOfDeath death &&
                   LogFolder == death.LogFolder &&
                   LogFile == death.LogFile &&
                   PlaceInLogFile == death.PlaceInLogFile;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LogFolder, LogFile, PlaceInLogFile);
        }
    }
}

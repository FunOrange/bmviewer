﻿using osu.Framework.Graphics.Primitives;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bmviewer
{
    class Plotter
    {
        // Input: (time, strain) data points, one for each hit object
        // TODO: Optimize plot rendering:
        // When playing, only plot data in the interval [gameTime-windowSize, gameTime]
        // On pause, plot all data.
        public void InitAimStrainMeter(Plot aimStrainMeter)
        {
            aimStrainMeter.Frame(false);
            aimStrainMeter.Style(Style.Blue2);
        }
        public void InitSortedPeaksPlot(Plot sortedPeaksPlot)
        {
            sortedPeaksPlot.Frame(false);
            sortedPeaksPlot.Style(Style.Blue2);
            sortedPeaksPlot.Title("Sorted Section Strain Peaks");
            sortedPeaksPlot.YLabel("Aim Strain");
            sortedPeaksPlot.XLabel("Rank");
            sortedPeaksPlot.Axis(x1: -0.5, x2: PEAKS_TO_DISPLAY + 0.5, y1: 0, y2: 100);
            sortedPeaksPlot.Ticks(displayTicksXminor: false);
            sortedPeaksPlot.Grid(xSpacing: 1);
        }
        public void InitAimStrainGraph(Plot aimStrainPlot)
        {
            aimStrainPlot.Frame(false);
            aimStrainPlot.Style(Style.Blue2);
            aimStrainPlot.YLabel("Aim Strain");
            aimStrainPlot.XLabel("Time (ms)");
            aimStrainPlot.Ticks(displayTicksXminor: false);
            aimStrainPlot.Grid(xSpacing: 400);
        }
        public void PlotFullAimStrainGraph(Plot aimStrainPlot,
            List<double> aimStrainPeakTimes , List<double> aimStrainPeaks,
            List<double> denseAimStrainTimes, List<double> denseAimStrainValues,
            List<double> aimStrainTimes     , List<double> strainPerObjectValues)
        {
            PlotAimStrainPeaks(aimStrainPlot, aimStrainPeakTimes, aimStrainPeaks);
            PlotAimStrainMainGraph(aimStrainPlot, denseAimStrainTimes, denseAimStrainValues);
            PlotAimStrainPerObject(aimStrainPlot, aimStrainTimes, strainPerObjectValues);
        }
        public void PlotPartialAimStrainGraph(Plot aimStrainPlot, double windowStartTime, double windowEndTime,
            List<double> aimStrainPeakTimes , List<double> aimStrainPeaks,
            List<double> denseAimStrainTimes, List<double> denseAimStrainValues,
            List<double> aimStrainTimes     , List<double> strainPerObjectValues)
        {
            // Filter data to only points that lie within visible time window for better performance
            // Aim strain peaks
            double windowMargin = 400;
            List<double> windowedAimStrainPeakTimes = new List<double>();
            List<double> windowedAimStrainPeaks = new List<double>();
            foreach (var (time, value) in Enumerable.Zip(aimStrainPeakTimes, aimStrainPeaks, (time, value) => (time, value)))
            {
                if (time >= windowStartTime - windowMargin && time <= windowEndTime + windowMargin)
                {
                    windowedAimStrainPeakTimes.Add(time);
                    windowedAimStrainPeaks.Add(value);
                }
            }
            PlotAimStrainPeaks(aimStrainPlot, windowedAimStrainPeakTimes, windowedAimStrainPeaks);


            // Main aim strain graph
            windowMargin = 100;
            List<double> windowedDenseAimStrainTimes = new List<double>();
            List<double> windowedDenseAimStrainValues = new List<double>();
            foreach (var (time, value) in Enumerable.Zip(denseAimStrainTimes, denseAimStrainValues, (time, value) => (time, value)))
            {
                if (time >= windowStartTime - windowMargin && time <= windowEndTime + windowMargin)
                {
                    windowedDenseAimStrainTimes.Add(time);
                    windowedDenseAimStrainValues.Add(value);
                }
            }
            PlotAimStrainMainGraph(aimStrainPlot, windowedDenseAimStrainTimes, windowedDenseAimStrainValues);


            // Per object strain arrows
            windowMargin = 100;
            List<double> windowedAimStrainTimes = new List<double>();
            List<double> windowedStrainPerObjectValues = new List<double>();
            foreach (var (time, value) in Enumerable.Zip(aimStrainTimes, strainPerObjectValues, (time, value) => (time, value)))
            {
                if (time >= windowStartTime - windowMargin && time <= windowEndTime + windowMargin)
                {
                    windowedAimStrainTimes.Add(time);
                    windowedStrainPerObjectValues.Add(value);
                }
            }
            PlotAimStrainPerObject(aimStrainPlot, windowedAimStrainTimes, windowedStrainPerObjectValues);

        }
        public void PlotAimStrainMainGraph(Plot aimStrainPlot, List<double> times, List<double> values)
        {
            if (times.Count == 0)
                return;
            aimStrainPlot.PlotScatter(
                times.ToArray(), values.ToArray(),
                lineWidth: 1,
                color: Color.FromArgb(255, 85, 85),
                markerSize: 0,
                markerShape: MarkerShape.none
            );
        }
        // Draws the upwards arrows corresponding to each object's individual aim strain value
        public void PlotAimStrainPerObject (Plot aimStrainPlot, List<double> times, List<double> objectStrainValues)
        {
            if (times.Count == 0)
                return;
            for (int i = 0; i < times.Count; i++)
                aimStrainPlot.PlotArrow(
                    times[i], objectStrainValues[i], times[i], 0,
                    lineWidth: 1,
                    arrowheadWidth: 2,
                    arrowheadLength: 4,
                    color: Color.FromArgb(180, 255, 85, 85)
                );
        }

        // Draws the purple bars and open circles corresponding to section peaks
        public void PlotAimStrainPeaks(Plot aimStrainPlot, List<double> times, List<double> peaks)
        {
            if (times.Count == 0)
                return;
            double sectionCenterTime(double time) => (int)(time / 400.0) * 400 + 200;
            var sectionTimes = Enumerable.Range(0, times.Count).Select(i => sectionCenterTime(times[i])).ToArray();
            aimStrainPlot.PlotBar(
                sectionTimes, peaks.Select(x=>Math.Round(x)).ToArray(),
                barWidth: 400,
                fillColor: Color.FromArgb(20, 189, 147, 249),
                outlineWidth: 0,
                showValues: true,
                valueColor: Color.FromArgb(200, 200, 200)
            );
            aimStrainPlot.PlotScatter(
                times.ToArray(), peaks.ToArray(),
                lineWidth: 0,
                lineStyle: LineStyle.None,
                color: Color.FromArgb(189, 147, 249),
                markerSize: 10,
                markerShape: MarkerShape.openCircle
            );
        }

        public void UpdateAimStrainMeter(Plot aimStrainMeter, List<double> denseAimStrainTimes, List<double> denseAimStrainValues, List<double> aimStrainPeakTimes, List<double> aimStrainPeaks, int gameTime)
        {
            double strain = Math.Round(CalculateCurrentStrain(denseAimStrainTimes, denseAimStrainValues, gameTime));

            // Show aim strain peak
            double sectionStart = (gameTime / 400) * 400;
            var indicesInsideSection = Enumerable.Range(0, denseAimStrainTimes.Count - 1)
                .Where(i => denseAimStrainTimes[i] >= sectionStart)
                .Where(i => denseAimStrainTimes[i] <= gameTime);
            double currentStrainPeak = 0;
            foreach (int i in indicesInsideSection)
                currentStrainPeak = Math.Max(denseAimStrainValues[i], currentStrainPeak);
            aimStrainMeter.PlotBar(
                new double[] { 0 },
                new double[] { currentStrainPeak },
                fillColor: Color.FromArgb(40, 189, 147, 249),
                outlineWidth: 0
            );
            aimStrainMeter.PlotText(
                Math.Round(currentStrainPeak).ToString(),
                0, currentStrainPeak + 20,
                color: Color.FromArgb(230, 230, 230),
                alignment: TextAlignment.middleCenter,
                fontSize: 15
            );

            aimStrainMeter.PlotBar(
                new double[] { 0 },
                new double[] { strain },
                fillColor: Color.FromArgb(255, 85, 85),
                outlineWidth: 0
            );
            aimStrainMeter.Ticks(displayTicksX: false);
            aimStrainMeter.Grid(enableVertical: false);
            aimStrainMeter.Axis(y1: 0, y2: denseAimStrainValues.Max() + 50);
            aimStrainMeter.Frame(false);
        }

        public (List<double>, List<double>) CalculateDenseStrainValues(List<double> times, List<double> strainValues)
        {
            double STRAIN_DECAY_RATE = 0.15; // in one second, initial value decays by this amount
            double TIME_GRANULARITY = 10.0;

            var dataPoints = new List<(double time, double strain)>();
            for (double time = times.Min(); time <= times.Max(); time += TIME_GRANULARITY)
            {
                // find most recent (time, strain) pair before this time
                int i = times.Count - 1;
                while (i >= 0 && times[i] > time)
                    i--;
                if (i != -1)
                {
                    double lastKnownStrainTime = times[i];
                    double lastKnownStrainValue = strainValues[i];
                    double timeDelta = time - lastKnownStrainTime;
                    double calculatedStrain = lastKnownStrainValue * Math.Pow(STRAIN_DECAY_RATE, timeDelta / 1000.0);
                    dataPoints.Add((time, calculatedStrain));
                }
                else
                    dataPoints.Add((time, 0));
            }
            // Insert two data points for each discontinuity in aim strain graph
            for (int i = 1; i < times.Count; i++)
            {
                double timeDelta = times[i] - times[i - 1];
                double calculatedStrain = strainValues[i - 1] * Math.Pow(STRAIN_DECAY_RATE, timeDelta / 1000.0);
                dataPoints.Add((times[i] - 1, calculatedStrain));
                dataPoints.Add((times[i], strainValues[i]));
            }
            var sortedDataPoints = dataPoints.OrderBy(p => p.time);
            var t = sortedDataPoints.Select(p => p.time);
            var s = sortedDataPoints.Select(p => p.strain);
            return (t.ToList(), s.ToList());
        }
        private double CalculateCurrentStrain(List<double> times, List<double> strainValues, double time)
        {
            double STRAIN_DECAY_RATE = 0.15; // in one second, initial value decays by this amount
            if (time < times.First())
                return 0;

            // find most recent (time, strain) pair before this time
            int i = times.Count - 1;
            while (i >= 0 && times[i] > time)
                i--;
            double lastKnownStrainTime = times[i];
            double lastKnownStrainValue = strainValues[i];
            double timeDelta = time - lastKnownStrainTime;
            return lastKnownStrainValue * Math.Pow(STRAIN_DECAY_RATE, timeDelta / 1000.0);
        }

        private static double PEAKS_TO_DISPLAY = 22;
        private static double OLD_THRESHOLD = 1000;
        internal bool ShouldRedrawSortedPeaksPlot(List<double> aimStrainPeakTimes, List<double> aimStrainPeaks, int gameTime)
        {
            // get all aim strains before gameTime, limit view to only the first PEAKS_TO_DISPLAY peaks
            var sortedPeaks = Enumerable.Zip(aimStrainPeakTimes, aimStrainPeaks, (time, value) => (time, value))
                .Where(peak => peak.time < gameTime)
                .OrderBy(peak => peak.value)
                .Reverse();
            var allRanks = Enumerable.Range(0, sortedPeaks.Count()).Select(x => (double)x).ToArray();
            var rankedPeaks = sortedPeaks.Zip(allRanks, (p, rank) => (rank, p.time, p.value))
                .Where(p => p.rank <= PEAKS_TO_DISPLAY);
            var newPeaks = rankedPeaks.Where(p => gameTime <= p.time + OLD_THRESHOLD);
            return newPeaks.Count() > 0 || LastSortedPeaksPlotContainedNewPeaks;
        }

        public bool LastSortedPeaksPlotContainedNewPeaks = true;
        public void PlotSortedPeaks(Plot sortedPeaksPlot, List<double> aimStrainPeakTimes, List<double> aimStrainPeaks, double gameTime)
        {

            // Mark 7 (50%), 22 (90%), and 28 (95%)
            sortedPeaksPlot.PlotVLine(7.5, color: Color.FromArgb(255, 121, 198), lineStyle: LineStyle.Dot);
            sortedPeaksPlot.PlotVLine(22.5, color: Color.FromArgb(255, 121, 198), lineStyle: LineStyle.Dot);
            sortedPeaksPlot.PlotVLine(28.5, color: Color.FromArgb(255, 121, 198), lineStyle: LineStyle.Dot);

            // get all aim strains before gameTime, limit view to only the first PEAKS_TO_DISPLAY peaks
            var sortedPeaks = Enumerable.Zip(aimStrainPeakTimes, aimStrainPeaks, (time, value) => (time, value))
                .Where(peak => peak.time < gameTime)
                .OrderBy(peak => peak.value)
                .Reverse();
            var allRanks = Enumerable.Range(0, sortedPeaks.Count()).Select(x => (double)x).ToArray();
            var rankedPeaks = sortedPeaks.Zip(allRanks, (p, rank) => (rank, p.time, p.value))
                .Where(p => p.rank <= PEAKS_TO_DISPLAY);
            var limitedRanks = rankedPeaks.Select(p => p.rank).ToArray();

            // Plot old peaks
            var oldPeaks = rankedPeaks.Where(p => gameTime > p.time + OLD_THRESHOLD);
            if (oldPeaks.Count() > 0)
                sortedPeaksPlot.PlotBar(
                    oldPeaks.Select(p => p.rank).ToArray(),
                    oldPeaks.Select(p => Math.Round(p.value)).ToArray(),
                    barWidth: 0.95,
                    fillColor: Color.FromArgb(70, 189, 147, 249),
                    outlineWidth: 0
                );

            // Plot recent peaks
            var newPeaks = rankedPeaks.Where(p => gameTime <= p.time + OLD_THRESHOLD);
            foreach ((double rank, double time, double value) in newPeaks)
            {
                var progress = (gameTime - time) / OLD_THRESHOLD;
                int transparency = (int)(255 - progress * (255 - 70));
                sortedPeaksPlot.PlotBar(
                    new double[] {rank},
                    new double[] {Math.Round(value)},
                    barWidth: 0.95,
                    fillColor: Color.FromArgb(transparency, 189, 147, 249),
                    outlineWidth: 0
                );
            }
            LastSortedPeaksPlotContainedNewPeaks = newPeaks.Count() > 0;

            // Plot weighted peaks
            var weightedPeaks = rankedPeaks
                .Select(p => p.value * Math.Pow(0.90, p.rank))
                .Select(x => Math.Round(x))
                .ToArray();
            if (weightedPeaks.Length > 0)
            {
                sortedPeaksPlot.PlotBar(
                    limitedRanks,
                    weightedPeaks,
                    barWidth: 0.95,
                    fillColor: Color.FromArgb(70, 189, 147, 249),
                    outlineWidth: 0
                );
            }
            //sortedPeaksPlot.AxisAuto(xExpandOnly: true, yExpandOnly: true);
            sortedPeaksPlot.Axis(x1: -0.5, x2: PEAKS_TO_DISPLAY + 0.5, y1: 0, y2: aimStrainPeaks.Max());
        }
    }
}

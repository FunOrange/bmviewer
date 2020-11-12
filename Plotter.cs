using osu.Framework.Graphics.Primitives;
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
        }
        public void PlotAimStrainGraph(Plot aimStrainPlot, List<double> times, List<double> values)
        {
            aimStrainPlot.Frame(false);
            aimStrainPlot.Style(Style.Blue2);
            aimStrainPlot.YLabel("Aim Strain");
            aimStrainPlot.XLabel("Time (ms)");
            aimStrainPlot.Ticks(displayTicksXminor: false);
            aimStrainPlot.Grid(xSpacing: 400);
            //for (double time = 0; time < times.Last(); time += 400)
            //    aimStrainPlot.PlotVLine(time, color: Color.FromArgb(100, 31, 119, 180), lineStyle: LineStyle.Dash);

            aimStrainPlot.PlotScatter(
                times.ToArray(), values.ToArray(),
                lineWidth: 1,
                color: Color.FromArgb(255, 85, 85),
                markerSize: 0,
                markerShape: MarkerShape.none
            );
        }
        public void PlotAimStrainPerObject (Plot aimStrainPlot, List<double> times, List<double> objectStrainValues)
        {
            for (int i = 0; i < times.Count; i++)
                aimStrainPlot.PlotArrow(
                    times[i], objectStrainValues[i], times[i], 0,
                    lineWidth: 1,
                    arrowheadWidth: 2,
                    arrowheadLength: 4,
                    color: Color.FromArgb(180, 255, 85, 85)
                );
        }

        public void PlotAimStrainPeaks(Plot aimStrainPlot, List<double> times, List<double> peaks)
        {
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
            double TIME_GRANULARITY = 10.0; // warning: low value causes lag

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

        public void PlotSortedPeaks(Plot sortedPeaksPlot, List<double> aimStrainPeakTimes, List<double> aimStrainPeaks, double gameTime)
        {
            double OLD_THRESHOLD = 600;

            // Mark 7 (50%), 22 (90%), and 28 (95%)
            sortedPeaksPlot.PlotVLine(7.5, color: Color.FromArgb(255, 121, 198), lineStyle: LineStyle.Dot);
            sortedPeaksPlot.PlotVLine(22.5, color: Color.FromArgb(255, 121, 198), lineStyle: LineStyle.Dot);
            sortedPeaksPlot.PlotVLine(28.5, color: Color.FromArgb(255, 121, 198), lineStyle: LineStyle.Dot);

            // get all aim strains before gameTime
            var sortedPeaks = Enumerable.Zip(aimStrainPeakTimes, aimStrainPeaks, (time, value) => (time, value))
                .Where(peak => peak.time < gameTime)
                .OrderBy(peak => peak.value)
                .Reverse();
            var ranks = Enumerable.Range(0, sortedPeaks.Count()).Select(x => (double)x).ToArray();
            var rankedPeaks = sortedPeaks.Zip(ranks, (p, rank) => (rank, p.time, p.value));

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

            // Plot weighted peaks
            foreach ((double rank, double time, double value) in rankedPeaks)
            {
                double weightedValue = value * Math.Pow(0.90, rank);
                sortedPeaksPlot.PlotBar(
                    new double[] {rank},
                    new double[] {Math.Round(weightedValue)},
                    barWidth: 0.95,
                    fillColor: Color.FromArgb(70, 189, 147, 249),
                    outlineWidth: 0
                );
            }

            sortedPeaksPlot.AxisAuto(xExpandOnly: true, yExpandOnly: true);
            sortedPeaksPlot.Axis(x1: -0.5, x2: 30.5, y1: 0, y2: aimStrainPeaks.Max());
            sortedPeaksPlot.Ticks(displayTicksXminor: false);
            sortedPeaksPlot.Grid(xSpacing: 1);
        }
    }
}

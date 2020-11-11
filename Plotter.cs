using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bmviewer
{
    class Plotter
    {
        // Input: (time, strain) data points, one for each hit object
        public void PlotAimStrain(Plot aimStrainPlot, List<double> times, List<double> values)
        {
            aimStrainPlot.Clear();
            //aimStrainPlot.Ticks(false, false);
            aimStrainPlot.Frame(false);
            aimStrainPlot.Style(Style.Blue2);
            aimStrainPlot.YLabel("Aim Strain");
            aimStrainPlot.XLabel("Time (ms)");
            //aimStrainPlot.PlotScatter(
            //    times.ToArray(), values.ToArray(),
            //    lineWidth: 0,
            //    color: System.Drawing.Color.FromArgb(255, 85, 85),
            //    markerSize: 6,
            //    markerShape: MarkerShape.filledCircle
            //);
            var (interTimes, interStrainValues) = CalculateIntermediateStrainValues(times, values);
            aimStrainPlot.PlotScatter(
                interTimes, interStrainValues,
                lineWidth: 1,
                color: System.Drawing.Color.FromArgb(255, 85, 85),
                markerSize: 0,
                markerShape: MarkerShape.none
            );
        }
        public void UpdateAimStrainMeter(Plot aimStrainMeter, List<double> aimStrainTimes, List<double> aimStrainValues, int gameTime)
        {
            double strain = Math.Round(CalculateCurrentStrain(aimStrainTimes, aimStrainValues, gameTime));
            aimStrainMeter.Clear();
            aimStrainMeter.PlotBar(
                new double[] { 0 },
                new double[]{ strain },
                showValues: true,
                fillColor: System.Drawing.Color.FromArgb(255, 85, 85),
                outlineColor: System.Drawing.Color.FromArgb(255, 85, 85),
                valueColor: System.Drawing.Color.FromArgb(240, 240, 240)
            );
            aimStrainMeter.Ticks(displayTicksX: false);
            aimStrainMeter.Grid(enableVertical: false);
            aimStrainMeter.Axis(y1: 0, y2: aimStrainValues.Max());
            aimStrainMeter.Frame(false);
        }

        private (double[], double[]) CalculateIntermediateStrainValues(List<double> times, List<double> strainValues)
        {
            double STRAIN_DECAY_RATE = 0.15; // in one second, initial value decays by this amount
            double TIME_GRANULARITY = 100.0; // warning: low value causes lag

            var dataPoints = new List<(double, double)>();
            for (double time = 0; time <= times.Max(); time += TIME_GRANULARITY)
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
            var sortedDataPoints = dataPoints.OrderBy(p => p.Item1);
            var t = sortedDataPoints.Select(p => p.Item1);
            var s = sortedDataPoints.Select(p => p.Item2);
            return (t.ToArray(), s.ToArray());
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
    }
}

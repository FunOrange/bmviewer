using SkiaSharp;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using osu.Game.Beatmaps;
using osu.Game.IO;
using osu.Game.Rulesets.Objects;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Objects.Legacy.Osu;
using System.Diagnostics;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Skills;
using ScottPlot;
using osu.Framework.Audio.Track;

namespace bmviewer
{
    public partial class Form1 : Form
    {
        OsuBeatmap beatmap;
        OsuDifficultyCalculator calculator;
        int gameTime = 0;
        Stopwatch stopwatch = new Stopwatch();
        long stopwatchStartTime = 0;

        // Aim strain
        List<double> aimStrainTimes;
        List<double> aimStrainValues;

        // Skin elements

        public Form1()
        {
            InitializeComponent();
            DrawFunctions.LoadSkin();
            //LoadBeatmapFromFile(@"C:\Program Files\osu!\Songs\1091022 Minase Inori - brave climber\Minase Inori - brave climber (-Brethia) [courage 1.32x (250bpm) AR10].osu");
            LoadBeatmapFromFile(@"C:\Program Files\osu!\Songs\1200218 The Living Tombstone - Goodbye Moonmen- Rick and Morty Remix\The Living Tombstone - Goodbye Moonmen- Rick and Morty Remix (Kyrian) [Pog].osu");
            InitAimStrainMeter();
            stopwatch.Start();
            gameTimer.Start();
        }

        private void InitAimStrainMeter()
        {
            aimStrainMeter.plt.Frame(false);
            aimStrainMeter.plt.Style(Style.Blue2);
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = @"C:\Program Files\osu!\Songs";
                openFileDialog.Filter = "beatmap (*.osu)|*.osu|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = "Select beatmap";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    filePath = openFileDialog.FileName;
            }
            if (filePath == "")
                return;

            LoadBeatmapFromFile(filePath);
        }

        private Beatmap ReadBeatmap(string beatmapPath)
        {
            using (var stream = File.OpenRead(beatmapPath))
            using (var streamReader = new LineBufferedReader(stream))
                return osu.Game.Beatmaps.Formats.Decoder.GetDecoder<Beatmap>(streamReader).Decode(streamReader);
        }
        private void LoadBeatmapFromFile(string beatmapPath)
        {
            var convertBeatmap = ReadBeatmap(beatmapPath);
            var osuBeatmap = (OsuBeatmap)new OsuBeatmapConverter(convertBeatmap).Convert();
            var processor = new OsuBeatmapProcessor(osuBeatmap);
            processor.PreProcess();
            processor.PostProcess();


            var workingBeatmap = new PpWorkingBeatmap(convertBeatmap);
            calculator = new OsuDifficultyCalculator(new OsuRuleset(), workingBeatmap);
            var diffAttributes = calculator.Calculate(new Mod[] { });
            var aim = (Aim)diffAttributes.Skills[0];
            aimStrainTimes = aim.RawStrainTimes;
            aimStrainValues = aim.RawStrainValues;
            PlotAimStrain(aim.RawStrainTimes, aim.RawStrainValues);
            Text = $"bmviewer - {convertBeatmap}";
            beatmap = osuBeatmap;

            timeUpDown.Maximum = (int)beatmap.HitObjects.Last().StartTime + 10000;
            trackBar1.Maximum = (int)beatmap.HitObjects.Last().StartTime + 10000;
            gameTime = (int)beatmap.HitObjects.First().StartTime - 1000;
            SetGameTime(gameTime);
            stopwatchStartTime = gameTime;
        }

        // Input: (time, strain) data points, one for each hit object
        private void PlotAimStrain(List<double> times, List<double> values)
        {
            aimStrainPlot.plt.Clear();
            //aimStrainPlot.plt.Ticks(false, false);
            aimStrainPlot.plt.Frame(false);
            aimStrainPlot.plt.Style(Style.Blue2);
            aimStrainPlot.plt.YLabel("Aim Strain");
            aimStrainPlot.plt.XLabel("Time (ms)");
            //aimStrainPlot.plt.PlotScatter(
            //    times.ToArray(), values.ToArray(),
            //    lineWidth: 0,
            //    color: System.Drawing.Color.FromArgb(255, 85, 85),
            //    markerSize: 6,
            //    markerShape: MarkerShape.filledCircle
            //);
            var (interTimes, interStrainValues) = CalculateIntermediateStrainValues(times, values);
            aimStrainPlot.plt.PlotScatter(
                interTimes, interStrainValues,
                lineWidth: 1,
                color: System.Drawing.Color.FromArgb(255, 85, 85),
                markerSize: 0,
                markerShape: MarkerShape.none
            );

            aimStrainPlot.Render();
        }

        private void UpdateAimStrainMeter()
        {
            double strain = Math.Round(CalculateCurrentStrain(aimStrainTimes, aimStrainValues, gameTime));
            aimStrainMeter.plt.Clear();
            aimStrainMeter.plt.PlotBar(
                new double[] { 0 },
                new double[]{ strain },
                showValues: true,
                fillColor: System.Drawing.Color.FromArgb(255, 85, 85),
                outlineColor: System.Drawing.Color.FromArgb(255, 85, 85),
                valueColor: System.Drawing.Color.FromArgb(240, 240, 240)
            );
            aimStrainMeter.plt.Ticks(displayTicksX: false);
            aimStrainMeter.plt.Grid(enableVertical: false);
            aimStrainMeter.plt.Axis(y1: 0, y2: aimStrainValues.Max());
            aimStrainMeter.plt.Frame(false);
            aimStrainMeter.Render();
        }

        // Fill in strain data points between hit objects
        // Return: times in first list, values in second list
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
                double calculatedStrain = strainValues[i-1] * Math.Pow(STRAIN_DECAY_RATE, timeDelta / 1000.0);
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

        private void skControl_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            var surface = e.Surface;
            var width = e.Info.Width;
            var height = e.Info.Height;
            var canvas = surface.Canvas;

            canvas.Clear(SKColors.Black);
            RenderGame(canvas, width, height);
            canvas.Flush();
        }

        private void RenderGame(SKCanvas canvas, int width, int height)
        {
            if (beatmap == null)
                return;

            // filter visible objects
            bool ObjectIsVisible(HitObject obj)
            {
                switch (obj)
                {
                    case HitCircle c:
                        return gameTime > (c.StartTime - c.TimePreempt) && gameTime < (c.StartTime + 250);
                    case Slider s:
                        var mul1 = beatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier;
                        var mul2 = beatmap.ControlPointInfo.DifficultyPointAt(s.StartTime).SpeedMultiplier;
                        double sliderMultiplier = mul1 * mul2;
                        double velocity = 100 * sliderMultiplier / beatmap.ControlPointInfo.TimingPointAt(s.StartTime).BeatLength;
                        double slideDuration = s.Path.Distance / velocity;
                        double totalSlideDuration = slideDuration * (s.RepeatCount + 1);
                        return gameTime > (s.StartTime - s.TimePreempt) && gameTime < (s.StartTime + totalSlideDuration + 250);
                    default:
                        return false;
                }
            }
            var visibleObjects = beatmap.HitObjects.Where(ObjectIsVisible);

            // draw objects
            foreach (OsuHitObject obj in visibleObjects.AsEnumerable().Reverse())
            {
                switch (obj)
                {
                    case HitCircle c:
                        DrawFunctions.DrawHitCircle(canvas, gameTime, c);
                        break;
                    case Slider s:
                        var mul1 = beatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier;
                        var mul2 = beatmap.ControlPointInfo.DifficultyPointAt(s.StartTime).SpeedMultiplier;
                        double sliderMultiplier = mul1 * mul2;
                        double velocity = 100 * sliderMultiplier / beatmap.ControlPointInfo.TimingPointAt(s.StartTime).BeatLength;
                        DrawFunctions.DrawSlider(canvas, gameTime, s, velocity);
                        break;
                }
            }
            // draw approach circles on top of everything
            foreach (OsuHitObject obj in visibleObjects.Where(o => gameTime <= o.StartTime).Reverse())
                DrawFunctions.DrawApproachCircle(canvas, gameTime, obj);
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            SetGameTime((int)stopwatchStartTime + (int)stopwatch.ElapsedMilliseconds);
        }

        private void restartButton_Click(object sender, EventArgs e)
        {
            if (gameTimer.Enabled)
                playPauseButton_Click(this, EventArgs.Empty);
            SetGameTime((int)beatmap.HitObjects.First().StartTime - 1000);
            playPauseButton_Click(this, EventArgs.Empty);
        }

        private void SetGameTime(int time)
        {
            if (time == gameTime)
                return; // prevent recursive calls
            if (time > timeUpDown.Maximum)
                return;

            gameTime = time;
            timeUpDown.Value = gameTime;
            trackBar1.Value = gameTime;
            skControl.Invalidate();

            // Update aim strain plot time range
            aimStrainPlot.plt.Axis(x1: gameTime - 2000, x2: gameTime, y1: 0, y2: aimStrainValues.Max());
            aimStrainPlot.Render();

            // Update aim strain meter
            UpdateAimStrainMeter();
        }

        private void playPauseButton_Click(object sender, EventArgs e)
        {
            if (gameTimer.Enabled)
            {
                gameTimer.Stop();
                stopwatch.Stop();
            }
            else
            {
                SetGameTime((int)timeUpDown.Value);
                stopwatchStartTime = gameTime;
                gameTimer.Start();
                stopwatch.Restart();
            }
        }

        private void timeUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (gameTimer.Enabled)
                return;
            SetGameTime((int)timeUpDown.Value);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (gameTimer.Enabled)
                return;
            SetGameTime(trackBar1.Value);
        }
    }
}

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
using OpenTK.Graphics.OpenGL;

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
        Plotter plotter = new Plotter();
        List<double> aimStrainTimes;
        List<double> aimStrainValues;
        List<double> denseAimStrainTimes;
        List<double> denseAimStrainValues;
        List<double> aimStrainPeakTimes;
        List<double> aimStrainPeaks;
        List<double> aimStrainPerObjectValues;

        // Skin elements

        public Form1()
        {
            InitializeComponent();
            DrawFunctions.LoadSkin();
            //LoadBeatmapFromFile(@"C:\Program Files\osu!\Songs\1091022 Minase Inori - brave climber\Minase Inori - brave climber (-Brethia) [courage 1.32x (250bpm) AR10].osu");
            //LoadBeatmapFromFile(@"C:\Program Files\osu!\Songs\1200218 The Living Tombstone - Goodbye Moonmen- Rick and Morty Remix\The Living Tombstone - Goodbye Moonmen- Rick and Morty Remix (Kyrian) [Pog].osu");
            LoadBeatmapFromFile(@"C:\Program Files\osu!\Songs\919187 765 MILLION ALLSTARS - UNION!!\765 MILLION ALLSTARS - UNION!! (Fu3ya_) [We are all MILLION!! 1.48x (255bpm) AR10 OD10].osu");
            plotter.InitAimStrainMeter(aimStrainMeter.plt);
            plotter.InitSortedPeaksPlot(sortedPeaksPlot.plt);
            aimStrainMeter.Render();
            sortedPeaksPlot.Render();
            stopwatch.Start();
            gameTimer.Start();
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
            // Load osu beatmap
            var convertBeatmap = ReadBeatmap(beatmapPath);
            beatmap = (OsuBeatmap)new OsuBeatmapConverter(convertBeatmap).Convert();
            var processor = new OsuBeatmapProcessor(beatmap);
            processor.PreProcess();
            processor.PostProcess();

            // Calculate difficulty
            var workingBeatmap = new PpWorkingBeatmap(convertBeatmap);
            calculator = new OsuDifficultyCalculator(new OsuRuleset(), workingBeatmap);
            var diffAttributes = calculator.Calculate(new Mod[] { });
            var aim = (Aim)diffAttributes.Skills[0];
            aimStrainTimes = aim.StrainTimes;
            aimStrainValues = aim.StrainValues;
            aimStrainPeakTimes = aim.strainPeakTimes;
            aimStrainPeaks = aim.strainPeaks;
            aimStrainPerObjectValues = aim.StrainPerObjectValues;
            (denseAimStrainTimes, denseAimStrainValues) = plotter.CalculateDenseStrainValues(aim.StrainTimes, aim.StrainValues);

            // Plot aim strain
            PerfStopwatch.Start("Aim Strain Graph");
            aimStrainPlot.plt.Clear();
            plotter.InitAimStrainGraph(aimStrainPlot.plt);
            plotter.PlotFullAimStrainGraph(
                aimStrainPlot.plt,
                aimStrainPeakTimes, aimStrainPeaks,
                denseAimStrainTimes, denseAimStrainValues,
                aimStrainTimes, aimStrainPerObjectValues);
            aimStrainPlot.Render();
            PerfStopwatch.Stop();

            Text = $"bmviewer - {convertBeatmap}";

            // Update GUI controls
            // TODO: Use events to update values
            timeUpDown.Minimum = (int)beatmap.HitObjects.First().StartTime - 1000;
            trackBar1.Minimum = (int)beatmap.HitObjects.First().StartTime - 1000;
            timeUpDown.Maximum = (int)beatmap.HitObjects.Last().StartTime + 10000;
            trackBar1.Maximum = (int)beatmap.HitObjects.Last().StartTime + 10000;

            // Set initial game time
            SetGameTime((int)beatmap.HitObjects.First().StartTime - 1000);
            stopwatchStartTime = gameTime;
        }

        // Fill in strain data points between hit objects
        // Return: times in first list, values in second list

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
            long totalMs = 0;

            gameTime = time;
            timeUpDown.Value = gameTime;
            trackBar1.Value = gameTime;
            PerfStopwatch.Start("Game Draw:".PadRight(25));
            skControl.Invalidate();
            totalMs += PerfStopwatch.Stop();

            // Update aim strain plot time range
            PerfStopwatch.Start("Aim strain plot update".PadRight(25));
            aimStrainPlot.plt.Clear();
            plotter.PlotPartialAimStrainGraph(
                aimStrainPlot.plt,
                gameTime - 2000, gameTime,
                aimStrainPeakTimes, aimStrainPeaks,
                denseAimStrainTimes, denseAimStrainValues,
                aimStrainTimes, aimStrainPerObjectValues);
            aimStrainPlot.plt.Axis(x1: gameTime - 2000, x2: gameTime, y1: 0, y2: aimStrainValues.Max() + 50);
            aimStrainPlot.Render();
            totalMs += PerfStopwatch.Stop();

            // Update aim strain meter
            PerfStopwatch.Start("Aim strain meter update".PadRight(25));
            aimStrainMeter.plt.Clear();
            plotter.UpdateAimStrainMeter(aimStrainMeter.plt, denseAimStrainTimes, denseAimStrainValues, aimStrainPeakTimes, aimStrainPeaks, gameTime);
            aimStrainMeter.Render();
            totalMs += PerfStopwatch.Stop();

            // Update sorted peaks graph
            PerfStopwatch.Start("Sorted peaks plot update".PadRight(25));
            if (plotter.ShouldRedrawSortedPeaksPlot(aimStrainPeakTimes, aimStrainPeaks, gameTime))
            {
                sortedPeaksPlot.plt.Clear();
                plotter.PlotSortedPeaks(sortedPeaksPlot.plt, aimStrainPeakTimes, aimStrainPeaks, gameTime);
                sortedPeaksPlot.Render();
            }
            totalMs += PerfStopwatch.Stop();
            Console.WriteLine($"total: {totalMs} ms ({1000 / totalMs} Hz)");
            Console.WriteLine("");
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

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

namespace bmviewer
{
    public partial class Form1 : Form
    {
        OsuBeatmap beatmap;
        OsuDifficultyCalculator calculator;
        int gameTime = 0;
        Stopwatch stopwatch = new Stopwatch();
        long stopwatchStartTime = 0;

        // Skin elements

        public Form1()
        {
            InitializeComponent();
            DrawFunctions.LoadSkin();
            beatmap = LoadBeatmapFromFile(@"C:\Program Files\osu!\Songs\1091022 Minase Inori - brave climber\Minase Inori - brave climber (-Brethia) [courage 1.32x (250bpm) AR10].osu");
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

            beatmap = LoadBeatmapFromFile(filePath);
            gameTime = 0;
        }

        private Beatmap ReadBeatmap(string beatmapPath)
        {
            using (var stream = File.OpenRead(beatmapPath))
            using (var streamReader = new LineBufferedReader(stream))
                return osu.Game.Beatmaps.Formats.Decoder.GetDecoder<Beatmap>(streamReader).Decode(streamReader);
        }
        private OsuBeatmap LoadBeatmapFromFile(string beatmapPath)
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
            PlotAimStrain(aim.RawStrainTimes, aim.RawStrainValues);
            Text = $"bmviewer - {convertBeatmap}";
            return osuBeatmap;
        }

        private void PlotAimStrain(List<double> times, List<double> values)
        {
            aimStrainPlot.plt.Clear();
            aimStrainPlot.plt.Ticks(false, false);
            aimStrainPlot.plt.Frame(false);
            aimStrainPlot.plt.Style(Style.Blue2);
            aimStrainPlot.plt.YLabel("KPS");
            float w = 0.05f;
            //aimStrainPlot.plt.PlotVSpan(y1: 13.33 - w, y2: 13.33 + w, label: "200 bpm streams");
            aimStrainPlot.plt.PlotScatter(
                times.ToArray(), values.ToArray(),
                //lineWidth: 0,
                color: System.Drawing.Color.FromArgb(255, 85, 85),
                markerSize: 6,
                markerShape: MarkerShape.filledCircle
            );
            aimStrainPlot.plt.Axis(null, null, 0, null);
            aimStrainPlot.plt.TightenLayout(padding: 0);
            aimStrainPlot.Render();
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
            SetGameTime(0);
        }

        private void SetGameTime(int time)
        {
            gameTime = time;
            timeUpDown.Value = gameTime;
            skControl.Invalidate();

            // Update plot time axis
            aimStrainPlot.plt.Axis(x1: gameTime - 2000, x2: gameTime);
            aimStrainPlot.Render();
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

        private void CalculateStrain()
        {

        }
    }
}

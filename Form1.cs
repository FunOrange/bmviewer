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

namespace bmviewer
{
    public partial class Form1 : Form
    {
        OsuBeatmap beatmap;
        int gameTime = 0;
        Stopwatch stopwatch = new Stopwatch();

        // Skin elements

        public Form1()
        {
            InitializeComponent();
            DrawFunctions.LoadSkin();
            beatmap = LoadBeatmapFromFile(@"C:\Program Files\osu!\Songs\24601 Hatsune Miku - Everless\Hatsune Miku - Everless (eveless) [Insane].osu");
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
            Text = $"bmviewer - {convertBeatmap}";
            return osuBeatmap;
        }

        private void renderButton_Click(object sender, EventArgs e)
        {
            skControl.Invalidate();
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
            SetGameTime(gameTime + (int)stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
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
        }

        private void playPauseButton_Click(object sender, EventArgs e)
        {
            if (gameTimer.Enabled)
                gameTimer.Stop();
            else
                gameTimer.Start();

            if (stopwatch.IsRunning)
                stopwatch.Stop();
            else
                stopwatch.Restart();
        }

        private void timeUpDown_ValueChanged(object sender, EventArgs e)
        {
            SetGameTime((int)timeUpDown.Value);
        }
    }
}

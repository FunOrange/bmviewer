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
            var convertBeatmap = ReadBeatmap(@"C:\Program Files\osu!\Songs\24601 Hatsune Miku - Everless\Hatsune Miku - Everless (eveless) [Insane].osu");
            beatmap = (OsuBeatmap)(new OsuBeatmapConverter(convertBeatmap).Convert());
            Text = $"bmviewer - {beatmap}";
            Console.WriteLine(beatmap);
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

            // Load beatmap
            //beatmap = LoadBeatmap(filePath);
            //Text = $"bmviewer - {beatmap}";
            //gameTime = 0;
        }

        private Beatmap ReadBeatmap(string beatmapPath)
        {
            using (var stream = File.OpenRead(beatmapPath))
            using (var streamReader = new LineBufferedReader(stream))
                return osu.Game.Beatmaps.Formats.Decoder.GetDecoder<Beatmap>(streamReader).Decode(streamReader);
        }

        private void LoadSkin()
        {

        }

        private void renderButton_Click(object sender, EventArgs e)
        {
            skControl.Invalidate();
        }

        private void skControl_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            Console.WriteLine("redraw");
            var surface = e.Surface;
            var width = e.Info.Width;
            var height = e.Info.Height;
            var canvas = surface.Canvas;

            Render(canvas, width, height);
            canvas.Flush();
        }

        private void Render(SKCanvas canvas, int width, int height)
        {
            if (beatmap == null)
                return;

            // draw the game
            canvas.Clear(SKColors.Black);

            // draw objects
            foreach (OsuHitObject obj in beatmap.HitObjects.AsEnumerable().Reverse())
            {
                switch (obj)
                {
                    case HitCircle hitCircle:
                        DrawFunctions.DrawHitCircle(canvas, gameTime, hitCircle);
                        break;
                    case Slider slider:
                        DrawFunctions.DrawSlider(canvas, gameTime, slider);
                        break;
                }
            }
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

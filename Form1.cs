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

namespace bmviewer
{
    public partial class Form1 : Form
    {
        Beatmap beatmap;
        public Form1()
        {
            InitializeComponent();
            beatmap = LoadBeatmap(@"C:\Program Files\osu!\Songs\24601 Hatsune Miku - Everless\Hatsune Miku - Everless (eveless) [Normal].osu");
            Console.WriteLine(beatmap);
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
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
            beatmap = LoadBeatmap(filePath);
        }

        private Beatmap LoadBeatmap(string beatmapPath)
        {
            using (var stream = File.OpenRead(beatmapPath))
            using (var streamReader = new LineBufferedReader(stream))
                return osu.Game.Beatmaps.Formats.Decoder.GetDecoder<Beatmap>(streamReader).Decode(streamReader);
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

            DrawGame(canvas, width, height);
            canvas.Flush();
        }

        private void DrawGame(SKCanvas canvas, int width, int height)
        {
            if (beatmap == null)
                return;

            // draw the game
            // todo: may need to make a beatmap wrapper class to access shit
            var a = beatmap.HitObjects;
            var b = a.Where(obj => obj is HitCircle);
            foreach (var o in beatmap.HitObjects)
            {
                if (o is osu.Game.Rulesets.Objects.Legacy.Osu.ConvertHit)
                    Console.WriteLine(o);
            }

            // draw on the canvas
            var paint1 = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.Blue,
                StrokeWidth = 15
            };
            canvas.Clear(SKColors.Black);
            canvas.DrawCircle(50, 50, 30, paint1);
        }
    }
}

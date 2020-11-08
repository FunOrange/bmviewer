using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using osu.Game.Beatmaps;

namespace bmviewer
{
    public partial class Form1 : Form
    {
        Beatmap beatmap;
        public Form1()
        {
            InitializeComponent();
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "beatmap (*.osu)|*.osu|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = "Select beatmap";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;

                    //Read the contents of the file into a stream
                    var fileStream = openFileDialog.OpenFile();

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                }
            }
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

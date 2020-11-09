using osu.Game.Rulesets.Osu.Objects;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bmviewer
{
    class DrawFunctions
    {
        // Visual parameters
        static float approachTimeMs = 600;

        public static void DrawHitCircle(SKCanvas canvas, int gameTime, HitCircle circle)
        {
            // note is in the future
            if (circle.StartTime >= gameTime)
            {
                int timeDelta = (int)circle.StartTime - gameTime;
                double fadeStart = circle.TimePreempt;
                double fadeDuration = circle.TimeFadeIn;
                double fadeEnd = fadeStart - fadeDuration;
                double transparency =
                    (timeDelta > circle.TimePreempt) ? 0 :
                    (timeDelta > fadeEnd) ? 255 - 255 * ((timeDelta - fadeEnd) / fadeDuration) :
                    255;
                var circleStyle = new SKPaint
                {
                    IsAntialias = true,
                    Color = SKColor.FromHsv(0, 0, 80, (byte)transparency),
                    StrokeWidth = 15
                };
                canvas.DrawCircle(circle.X, circle.Y, (float)circle.Radius / 2.0f, circleStyle);
            }
        }
        public static void DrawSlider(SKCanvas canvas, int gameTime, Slider slider)
        {
            // note is in the future
            if (slider.StartTime >= gameTime)
            {
                int timeDelta = (int)slider.StartTime - gameTime;
                double fadeStart = slider.TimePreempt;
                double fadeDuration = slider.TimeFadeIn;
                double fadeEnd = fadeStart - fadeDuration;
                double transparency =
                    (timeDelta > slider.TimePreempt) ? 0 :
                    (timeDelta > fadeEnd) ? 255 - 255 * ((timeDelta - fadeEnd) / fadeDuration) :
                    255;
                var sliderHeadStyle = new SKPaint
                {
                    IsAntialias = true,
                    Color = SKColor.FromHsv(0, 0, 80, (byte)transparency),
                    StrokeWidth = 15
                };
                var sliderBodyStyle = new SKPaint
                {
                    IsAntialias = true,
                    // 40, 42, 54
                    Color = new SKColor(40, 42, 54, (byte)transparency),
                    StrokeWidth = 15
                };

                // Draw slider body
                var sliderPath = new List<osuTK.Vector2>();
                slider.Path.GetPathToProgress(sliderPath, 0, 1);
                foreach (var point in sliderPath)
                    canvas.DrawCircle(slider.X + point.X, slider.Y + point.Y, (float)slider.Radius / 2.0f, sliderBodyStyle);

                // Draw slider head
                canvas.DrawCircle(slider.X, slider.Y, (float)slider.Radius / 2.0f, sliderHeadStyle);
            }
        }
    }
}

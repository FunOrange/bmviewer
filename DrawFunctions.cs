using osu.Game.Beatmaps.ControlPoints;
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
            // t0 => circle is starting to fade in
            // t1 => circle is now fully visible
            // t2 => circle is starting to fade out
            // t3 => circle is no longer visible
            double t0 = circle.StartTime - circle.TimePreempt;
            double t1 = t0 + circle.TimeFadeIn;
            double t2 = circle.StartTime;
            double t3 = t2 + 250;

            // progress 0 -> 1
            double fadeInProgress = (gameTime - t0) / (t1 - t0);
            double fadeOutProgress = (gameTime - t2) / (t3 - t2);

            double transparency =
                (gameTime < t0) ? 0                           :
                (gameTime < t1) ? 255 * fadeInProgress        :
                (gameTime < t2) ? 255                         :
                (gameTime < t3) ? 255 * (1 - fadeOutProgress) :
                                  0;

            double scale =
                (gameTime < t2) ? 1.0 :
                                  1.0 + 0.3 * Math.Sqrt(Math.Sin(Math.PI * fadeOutProgress / 2.0));

            var circleStyle = new SKPaint
            {
                IsAntialias = true,
                Color = SKColor.FromHsv(0, 0, 80, (byte)transparency),
                StrokeWidth = 15
            };
            canvas.DrawCircle(circle.X, circle.Y, (float)(scale * circle.Radius) / 2.0f, circleStyle);
        }
        public static void DrawSlider(SKCanvas canvas, int gameTime, Slider slider, TimingControlPoint tp)
        {
            //// slider head hasn't been hit yet
            //if (gameTime < slider.StartTime)
            //{
            //    int timeDelta = (int)slider.StartTime - gameTime;
            //    double fadeStart = slider.TimePreempt;
            //    double fadeDuration = slider.TimeFadeIn;
            //    double fadeEnd = fadeStart - fadeDuration;
            //    double transparency =
            //        (timeDelta > slider.TimePreempt) ? 0 :
            //        (timeDelta > fadeEnd) ? 255 - 255 * ((timeDelta - fadeEnd) / fadeDuration) :
            //        255;
            //}
            //// slider head is being hit
            //else if (gameTime < slider.StartTime + 250)
            //{
            //    double transparency =
            //        (gameTime < slider.EndTime) ? 255
            //        : 255.0 * (gameTime - slider.EndTime) / 250.0;
            //    var sliderBodyStyle = new SKPaint
            //    {
            //        IsAntialias = true,
            //        // 40, 42, 54
            //        Color = new SKColor(40, 42, 54, (byte)transparency),
            //        StrokeWidth = 15
            //    };
            //    // Draw slider body
            //    var sliderPath = new List<osuTK.Vector2>();
            //    slider.Path.GetPathToProgress(sliderPath, 0, 1);
            //    foreach (var point in sliderPath)
            //        canvas.DrawCircle(slider.X + point.X, slider.Y + point.Y, (float)slider.Radius / 2.0f, sliderBodyStyle);
            //}


            // t0 => slider is starting to fade in
            // t1 => slider is now fully visible
            // t2 => slider has started sliding
            // t3 => slider has finished sliding
            // t4 => slider is no longer visible
            double t0 = slider.StartTime - slider.TimePreempt;
            double t1 = t0 + slider.TimeFadeIn;
            double t2 = slider.StartTime;
            double t3 = t2 + 50; // TODO
            double t4 = t3 + 250;

            // progress 0 -> 1
            double fadeInProgress = (gameTime - t0) / (t1 - t0);

            ////////////////////////////////
            // draw slider body
            ////////////////////////////////
            double bodyFadeOutProgress = (gameTime - t3) / 250.0;
            double bodyTransparency =
                (gameTime < t0)       ? 0                               :
                (gameTime < t1)       ? 255 * fadeInProgress            :
                (gameTime < t4)       ? 255                             :
                (gameTime < t4 + 250) ? 255 * (1 - bodyFadeOutProgress) :
                                        0;
            var sliderBorderStyle = new SKPaint
            {
                IsAntialias = true,
                // 40, 42, 54
                Color = new SKColor(40, 40, 54, (byte)bodyTransparency),
                StrokeWidth = 15
            };
            var sliderBodyStyle = new SKPaint
            {
                IsAntialias = true,
                // 40, 42, 54
                Color = new SKColor(20, 21, 27, (byte)bodyTransparency),
                StrokeWidth = 15
            };
            var sliderPath = new List<osuTK.Vector2>();
            slider.Path.GetPathToProgress(sliderPath, 0, 1);
            foreach (var point in sliderPath)
                canvas.DrawCircle(slider.X + point.X, slider.Y + point.Y, (float)slider.Radius / 2.0f, sliderBorderStyle);
            foreach (var point in sliderPath)
                canvas.DrawCircle(slider.X + point.X, slider.Y + point.Y, 0.8f * (float)slider.Radius / 2.0f, sliderBodyStyle);

            ////////////////////////////////
            // draw slider head
            ////////////////////////////////
            double headFadeOutProgress = (gameTime - t2) / 250.0;
            double headTransparency =
                (gameTime < t0)       ? 0                               :
                (gameTime < t1)       ? 255 * fadeInProgress            :
                (gameTime < t2)       ? 255                             :
                (gameTime < t2 + 250) ? 255 * (1 - headFadeOutProgress) :
                                        0;
            double headScale =
                (gameTime < t2) ? 1.0 :
                                  1.0 + 0.3 * Math.Sqrt(Math.Sin(Math.PI * headFadeOutProgress / 2.0));

            var circleStyle = new SKPaint
            {
                IsAntialias = true,
                Color = SKColor.FromHsv(0, 0, 80, (byte)headTransparency),
                StrokeWidth = 15
            };
            canvas.DrawCircle(slider.X, slider.Y, (float)(headScale * slider.Radius) / 2.0f, circleStyle);

        }

        private void SlideDuration(Slider s)
        {
            var length = s.Path.Distance;
            //return length / (SliderMultiplier \* 100) \* beatLength
        }
    }
}

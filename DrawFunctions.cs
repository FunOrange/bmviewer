using osu.Framework.Allocation;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
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
        static float approachCircleMaxScale = 3.7f;

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
                IsStroke = true,
                Color = SKColor.FromHsv(0, 0, 80, (byte)transparency),
                StrokeWidth = 6
            };
            canvas.DrawCircle(circle.StackedPosition.X, circle.StackedPosition.Y, (float)(scale * circle.Radius) / 2.0f, circleStyle);

            //if (gameTime < t2)
            //{
            //}
        }
        public static void DrawSlider(SKCanvas canvas, int gameTime, Slider slider, double velocity)
        {
            double slideDuration = slider.Path.Distance / velocity;
            double totalSlideDuration = slideDuration * (slider.RepeatCount + 1);
            // t0 => slider is starting to fade in
            // t1 => slider is now fully visible
            // t2 => slider has started sliding
            // t3 => slider has finished sliding
            // t4 => slider is no longer visible
            double t0 = slider.StartTime - slider.TimePreempt;
            double t1 = t0 + slider.TimeFadeIn;
            double t2 = slider.StartTime;
            double t3 = t2 + totalSlideDuration;
            double t4 = t3 + 250;

            // progress 0 -> 1
            double fadeInProgress = (gameTime - t0) / (t1 - t0);

            ////////////////////////////////
            // draw slider body
            ////////////////////////////////
            double bodyFadeOutProgress = (gameTime - t3) / (t4 - t3);
            double bodyTransparency =
                (gameTime < t0) ? 0 :
                (gameTime < t1) ? 255 * fadeInProgress :
                (gameTime < t3) ? 255 :
                (gameTime < t3 + 250) ? 255 * (1 - bodyFadeOutProgress) :
                                        0;

            var sliderBorderStyle = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round,
                IsAntialias = true,
                Color = new SKColor(98, 114, 164, (byte)(0.5f*bodyTransparency)),
                StrokeWidth = (float)slider.Radius
            };
            var sliderBodyStyle = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round,
                IsAntialias = true,
                Color = new SKColor(0, 0, 0, (byte)(0.4f*bodyTransparency)),
                StrokeWidth = 0.8f * (float)slider.Radius
            };
            var sliderPath = new List<osuTK.Vector2>();
            slider.Path.GetPathToProgress(sliderPath, 0, 1);
            var drawPath = new SKPath();
            foreach (var pathPoint in sliderPath)
            {
                var absPoint = new SKPoint(slider.StackedPosition.X + pathPoint.X, slider.StackedPosition.Y + pathPoint.Y);
                if (drawPath.PointCount == 0)
                    drawPath.MoveTo(absPoint);
                else
                    drawPath.LineTo(absPoint);
            }
            canvas.DrawPath(drawPath, sliderBorderStyle);
            canvas.DrawPath(drawPath, sliderBodyStyle);

            ////////////////////////////////
            // draw slider follow circle
            ////////////////////////////////
            if (gameTime > slider.StartTime && gameTime < slider.StartTime + totalSlideDuration)
            {
                int slideNumber = (int)((gameTime - slider.StartTime) / slideDuration);
                double relativeSliderTime = gameTime - slider.StartTime;
                while (relativeSliderTime > slideDuration)
                    relativeSliderTime -= slideDuration;
                double progress =
                    (slideNumber % 2 == 0) ? relativeSliderTime / slideDuration
                                           : 1 - relativeSliderTime / slideDuration;
                var followCirclePosition = slider.Position + slider.Path.PositionAt(progress);
                var followCircleStyle = new SKPaint
                {
                    IsAntialias = true,
                    IsStroke = true,
                    Color = SKColor.FromHsv(0, 0, 80),
                    StrokeWidth = 4
                };
                canvas.DrawCircle(followCirclePosition.X, followCirclePosition.Y, 0.85f*(float)slider.Radius / 2.0f, followCircleStyle);
            }

            ////////////////////////////////
            // draw slider head
            ////////////////////////////////
            double headFadeOutProgress = (gameTime - t2) / 250.0;
            double headTransparency =
                (gameTime < t0) ? 0 :
                (gameTime < t1) ? 255 * fadeInProgress :
                (gameTime < t2) ? 255 :
                (gameTime < t2 + 250) ? 255 * (1 - headFadeOutProgress) :
                                        0;
            double headScale =
                (gameTime < t2) ? 1.0 :
                                  1.0 + 0.3 * Math.Sqrt(Math.Sin(Math.PI * headFadeOutProgress / 2.0));

            var circleStyle = new SKPaint
            {
                IsAntialias = true,
                IsStroke = true,
                Color = SKColor.FromHsv(0, 0, 80, (byte)headTransparency),
                StrokeWidth = 6
            };
            canvas.DrawCircle(slider.StackedPosition.X, slider.StackedPosition.Y, (float)(headScale * slider.Radius) / 2.0f, circleStyle);
        }
        public static void DrawApproachCircle(SKCanvas canvas, int gameTime, OsuHitObject obj)
        {
            double t0 = obj.StartTime - obj.TimePreempt;
            double t1 = t0 + obj.TimeFadeIn;
            double fadeInProgress = (gameTime - t0) / (t1 - t0);
            double transparency =
                (gameTime < t0) ? 0 :
                (gameTime < t1) ? 255 * fadeInProgress
                                : 255;
            float approachProgress = (float)((obj.StartTime - gameTime) / obj.TimePreempt);
            float approachCircleScale = 1.0f + approachProgress * (approachCircleMaxScale - 1.0f);
            var approachCircleStyle = new SKPaint
            {
                IsAntialias = true,
                IsStroke = true,
                Color = SKColor.FromHsv(0, 0, 80, (byte)transparency),
                StrokeWidth = 4
            };
            canvas.DrawCircle(obj.StackedPosition.X, obj.StackedPosition.Y, (approachCircleScale * (float)obj.Radius) / 2.0f, approachCircleStyle);
            return;
        }
    }
}

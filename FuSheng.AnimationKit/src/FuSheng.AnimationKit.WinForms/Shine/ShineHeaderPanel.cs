using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FuSheng.AnimationKit.WinForms
{
    /// <summary>
    /// A gradient header panel with luminous ice-reflection shine animation.
    /// </summary>
    public class ShineHeaderPanel : Control
    {
        private readonly Timer animationTimer;
        private float hoverProgress;
        private float targetHoverProgress;
        private float lightSweep = -0.55F;

        public int Radius { get; set; } = 22;
        public Color GradientStart { get; set; } = Color.FromArgb(59, 130, 246);
        public Color GradientMiddle { get; set; } = Color.FromArgb(87, 196, 238);
        public Color GradientEnd { get; set; } = Color.FromArgb(139, 92, 246);
        public Color TextColor { get; set; } = Color.White;
        public Color SubtitleColor { get; set; } = Color.FromArgb(245, 252, 255);
        public Color VersionColor { get; set; } = Color.FromArgb(238, 248, 255);

        public string TitleText { get; set; } = "FuShengSDK";
        public string SubtitleText { get; set; } = "";
        public string VersionText { get; set; } = "V1.0";

        public Font TitleFont { get; set; } = new Font("Microsoft YaHei UI", 19F, FontStyle.Bold);
        public Font SubtitleFont { get; set; } = new Font("Microsoft YaHei UI", 14.5F, FontStyle.Bold);
        public Font VersionFont { get; set; } = new Font("Microsoft YaHei UI", 17F, FontStyle.Bold);

        public bool ShineEnabled { get; set; } = true;
        public bool GlowEnabled { get; set; } = true;
        public bool ShadowEnabled { get; set; } = true;
        public AnimationQuality Quality { get; set; } = AnimationQuality.Standard;
        public float ShineSpeed { get; set; } = 0.018F;

        public ShineHeaderPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            DoubleBuffered = true;
            BackColor = Color.FromArgb(246, 248, 252);

            animationTimer = new Timer { Interval = 16 };
            animationTimer.Tick += delegate
            {
                hoverProgress = AnimationMath.Approach(hoverProgress, targetHoverProgress, 0.16F);

                if (targetHoverProgress > 0.5F && ShineEnabled && Quality != AnimationQuality.Minimal)
                {
                    lightSweep += ShineSpeed;
                    if (lightSweep > 1.45F)
                    {
                        lightSweep = -0.55F;
                    }
                }

                Invalidate();

                if (targetHoverProgress < 0.5F && Math.Abs(hoverProgress - targetHoverProgress) < 0.01F)
                {
                    hoverProgress = 0F;
                    lightSweep = -0.55F;
                    animationTimer.Stop();
                    Invalidate();
                }
            };
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            targetHoverProgress = 1F;
            animationTimer.Start();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            targetHoverProgress = 0F;
            animationTimer.Start();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Color back = AnimationMath.ResolveBackColor(this, Color.FromArgb(246, 248, 252));
            e.Graphics.Clear(back);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (Width <= 4 || Height <= 4)
            {
                return;
            }

            float t = AnimationEasing.Smooth(hoverProgress);

            RectangleF shadowRect = new RectangleF(7, 7 + t * 1.5F, Width - 14, Height - 16);
            if (ShadowEnabled && Quality != AnimationQuality.Minimal)
            {
                int layers = Quality == AnimationQuality.Low ? 3 : 5;
                AnimationMath.DrawSoftShadow(e.Graphics, shadowRect, Radius, Color.FromArgb(37, 99, 235), 28 + 52 * t, 4 + 3 * t, layers);
            }

            RectangleF rect = new RectangleF(
                2 - t * 1.2F,
                2 - t * 1.2F,
                Width - 5 + t * 2.4F,
                Height - 8 + t * 1.6F);

            Color start = AnimationMath.LerpColor(GradientStart, AnimationMath.Lighten(GradientStart, 14), t);
            Color middle = AnimationMath.LerpColor(GradientMiddle, AnimationMath.Lighten(GradientMiddle, 10), t);
            Color end = AnimationMath.LerpColor(GradientEnd, AnimationMath.Lighten(GradientEnd, 18), t);

            using (GraphicsPath path = AnimationMath.RoundPath(rect, Radius))
            using (LinearGradientBrush brush = new LinearGradientBrush(rect, start, end, 9F))
            {
                ColorBlend blend = new ColorBlend
                {
                    Positions = new[] { 0F, 0.52F, 1F },
                    Colors = new[] { start, middle, end }
                };
                brush.InterpolationColors = blend;

                e.Graphics.FillPath(brush, path);

                if (GlowEnabled && Quality == AnimationQuality.High)
                {
                    using (GraphicsPath glow = new GraphicsPath())
                    {
                        RectangleF ellipse = new RectangleF(Width * 0.45F, -Height * 0.75F, Width * 0.74F, Height * 1.95F);
                        glow.AddEllipse(ellipse);

                        using (PathGradientBrush glowBrush = new PathGradientBrush(glow))
                        {
                            glowBrush.CenterColor = Color.FromArgb((int)(55 + 45 * t), 255, 255, 255);
                            glowBrush.SurroundColors = new[] { Color.FromArgb(0, 255, 255, 255) };
                            e.Graphics.SetClip(path);
                            e.Graphics.FillPath(glowBrush, glow);
                            e.Graphics.ResetClip();
                        }
                    }
                }

                if (ShineEnabled && t > 0.02F && Quality != AnimationQuality.Low && Quality != AnimationQuality.Minimal)
                {
                    float bandX = -Width * 0.55F + lightSweep * Width * 1.55F;
                    using (GraphicsPath shine = new GraphicsPath())
                    using (SolidBrush shineBrush = new SolidBrush(Color.FromArgb((int)(42 * t), 255, 255, 255)))
                    {
                        shine.AddPolygon(new[]
                        {
                            new PointF(bandX, rect.Top - 30),
                            new PointF(bandX + 86, rect.Top - 30),
                            new PointF(bandX + 184, rect.Bottom + 30),
                            new PointF(bandX + 98, rect.Bottom + 30)
                        });

                        e.Graphics.SetClip(path);
                        e.Graphics.FillPath(shineBrush, shine);
                        e.Graphics.ResetClip();
                    }
                }

                using (Pen topPen = new Pen(Color.FromArgb(50 + (int)(55 * t), 255, 255, 255), 1.4F))
                {
                    e.Graphics.DrawArc(topPen, rect.X + 8, rect.Y + 8, Radius * 2F, Radius * 2F, 190, 42);
                    e.Graphics.DrawLine(topPen, rect.X + Radius + 8, rect.Y + 7, rect.Right - Radius - 8, rect.Y + 7);
                }
            }

            DrawHeaderText(e.Graphics, t);
        }

        private void DrawHeaderText(Graphics graphics, float hover)
        {
            using (StringFormat titleFormat = new StringFormat())
            using (StringFormat subtitleFormat = new StringFormat())
            using (StringFormat versionFormat = new StringFormat())
            using (SolidBrush titleBrush = new SolidBrush(TextColor))
            using (SolidBrush subtitleBrush = new SolidBrush(SubtitleColor))
            using (SolidBrush versionBrush = new SolidBrush(VersionColor))
            {
                titleFormat.Alignment = StringAlignment.Near;
                titleFormat.LineAlignment = StringAlignment.Center;
                titleFormat.Trimming = StringTrimming.EllipsisCharacter;

                subtitleFormat.Alignment = StringAlignment.Near;
                subtitleFormat.LineAlignment = StringAlignment.Center;
                subtitleFormat.Trimming = StringTrimming.EllipsisCharacter;

                versionFormat.Alignment = StringAlignment.Far;
                versionFormat.LineAlignment = StringAlignment.Center;
                versionFormat.Trimming = StringTrimming.EllipsisCharacter;

                float textLift = -1.5F * hover;
                RectangleF titleRect = new RectangleF(34, 18 + textLift, Width - 220, 42);
                RectangleF subtitleRect = new RectangleF(34, 66 + textLift, Width - 220, 36);
                RectangleF versionRect = new RectangleF(Width - 150, 0 + textLift, 115, Height - 4);

                graphics.DrawString(TitleText, TitleFont, titleBrush, titleRect, titleFormat);
                graphics.DrawString(SubtitleText, SubtitleFont, subtitleBrush, subtitleRect, subtitleFormat);
                graphics.DrawString(VersionText, VersionFont, versionBrush, versionRect, versionFormat);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                animationTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

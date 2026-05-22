using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace FuSheng.AnimationKit.WinForms
{
    internal sealed class FragmentTransitionLayer : Control
    {
        private float progress;

        public List<FragmentSprite> Sprites { get; set; } = new List<FragmentSprite>();
        public RectangleF PageRect { get; set; }
        public FragmentTransitionOptions Options { get; set; } = new FragmentTransitionOptions();

        public float Progress
        {
            get { return progress; }
            set
            {
                progress = AnimationMath.Clamp(value, 0F, 1F);
                Invalidate();
            }
        }

        public FragmentTransitionLayer()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            DoubleBuffered = true;
            BackColor = Color.FromArgb(246, 248, 252);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.Low;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            e.Graphics.CompositingQuality = CompositingQuality.AssumeLinear;

            DrawFusionPage(e.Graphics);

            if (Sprites == null || Sprites.Count == 0)
            {
                return;
            }

            foreach (FragmentSprite sprite in Sprites)
            {
                DrawSpriteFast(e.Graphics, sprite);
            }
        }

        private void DrawFusionPage(Graphics graphics)
        {
            if (PageRect.Width <= 2 || PageRect.Height <= 2)
            {
                return;
            }

            FragmentTransitionOptions options = Options ?? new FragmentTransitionOptions();
            float raw = AnimationMath.Clamp(Progress, 0F, 1F);
            float mix = AnimationEasing.InOutCubic(raw);
            float bell = (float)Math.Sin(raw * Math.PI);
            float fusion = AnimationEasing.InOutSine(AnimationMath.Clamp(bell, 0F, 1F));

            Color left = AnimationMath.LerpColor(
                Color.FromArgb(219, 234, 254),
                Color.FromArgb(237, 233, 254),
                mix);

            Color right = AnimationMath.LerpColor(
                Color.FromArgb(224, 242, 254),
                Color.FromArgb(250, 232, 255),
                mix);

            Color border = AnimationMath.LerpColor(
                Color.FromArgb(191, 219, 254),
                Color.FromArgb(196, 181, 253),
                mix);

            using (GraphicsPath pagePath = AnimationMath.RoundPath(PageRect, options.Radius))
            using (SolidBrush whiteBrush = new SolidBrush(Color.White))
            using (Pen borderPen = new Pen(Color.FromArgb(170, border), 1.2F))
            {
                graphics.FillPath(whiteBrush, pagePath);

                if (options.EnableGradientFusion && fusion > 0.01F && options.Quality != AnimationQuality.Minimal)
                {
                    int alpha = options.Quality == AnimationQuality.Low ? (int)(54F * fusion) : (int)(92F * fusion);
                    using (LinearGradientBrush brush = new LinearGradientBrush(
                        PageRect,
                        Color.FromArgb(alpha, left),
                        Color.FromArgb(alpha, right),
                        18F + 24F * mix))
                    {
                        graphics.SetClip(pagePath);
                        graphics.FillRectangle(brush, PageRect);
                        graphics.ResetClip();
                    }

                    if (options.EnableLightBand && options.Quality != AnimationQuality.Low)
                    {
                        float bandX = PageRect.Left - PageRect.Width * 0.35F + AnimationEasing.InOutSine(raw) * PageRect.Width * 1.7F;
                        using (GraphicsPath band = new GraphicsPath())
                        using (SolidBrush bandBrush = new SolidBrush(Color.FromArgb((int)(28F * fusion), 255, 255, 255)))
                        {
                            band.AddPolygon(new[]
                            {
                                new PointF(bandX, PageRect.Top),
                                new PointF(bandX + PageRect.Width * 0.16F, PageRect.Top),
                                new PointF(bandX + PageRect.Width * 0.38F, PageRect.Bottom),
                                new PointF(bandX + PageRect.Width * 0.20F, PageRect.Bottom)
                            });

                            graphics.SetClip(pagePath);
                            graphics.FillPath(bandBrush, band);
                            graphics.ResetClip();
                        }
                    }
                }

                graphics.DrawPath(borderPen, pagePath);
            }
        }

        private void DrawSpriteFast(Graphics graphics, FragmentSprite sprite)
        {
            if (sprite == null || sprite.Image == null)
            {
                return;
            }

            FragmentTransitionOptions options = Options ?? new FragmentTransitionOptions();
            float local = (Progress - sprite.Delay) / Math.Max(0.01F, sprite.Duration);
            local = AnimationMath.Clamp(local, 0F, 1F);

            if (Progress < sprite.Delay && sprite.Incoming)
            {
                return;
            }

            float moveEase = sprite.Incoming ? AnimationEasing.OutQuint(local) : AnimationEasing.InOutCubic(local);
            RectangleF rect = LerpRect(sprite.FromRect, sprite.ToRect, moveEase);
            float rotation = options.EnableRotation && options.Quality != AnimationQuality.Low && options.Quality != AnimationQuality.Minimal
                ? sprite.RotationFrom + (sprite.RotationTo - sprite.RotationFrom) * moveEase
                : 0F;

            float alpha = sprite.Incoming
                ? AnimationEasing.InOutCubic(AnimationMath.Clamp(local * 1.3F, 0F, 1F))
                : 1F - AnimationEasing.InOutCubic(AnimationMath.Clamp((local - 0.08F) / 0.92F, 0F, 1F));

            if (alpha <= 0.03F)
            {
                return;
            }

            float centerX = rect.X + rect.Width / 2F;
            float centerY = rect.Y + rect.Height / 2F;
            GraphicsState state = graphics.Save();

            graphics.TranslateTransform(centerX, centerY);
            if (Math.Abs(rotation) > 0.2F)
            {
                graphics.RotateTransform(rotation);
            }

            Rectangle drawRect = Rectangle.Round(new RectangleF(-rect.Width / 2F, -rect.Height / 2F, rect.Width, rect.Height));

            if (alpha >= 0.96F)
            {
                graphics.DrawImage(sprite.Image, drawRect);
            }
            else
            {
                using (ImageAttributes attributes = CreateAlphaAttributes(alpha))
                {
                    graphics.DrawImage(sprite.Image, drawRect, 0, 0, sprite.Image.Width, sprite.Image.Height, GraphicsUnit.Pixel, attributes);
                }
            }

            graphics.Restore(state);
        }

        private static ImageAttributes CreateAlphaAttributes(float alpha)
        {
            ColorMatrix matrix = new ColorMatrix();
            matrix.Matrix00 = 1F;
            matrix.Matrix11 = 1F;
            matrix.Matrix22 = 1F;
            matrix.Matrix33 = AnimationMath.Clamp(alpha, 0F, 1F);
            matrix.Matrix44 = 1F;

            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            return attributes;
        }

        private static RectangleF LerpRect(RectangleF from, RectangleF to, float amount)
        {
            amount = AnimationMath.Clamp(amount, 0F, 1F);
            return new RectangleF(
                from.X + (to.X - from.X) * amount,
                from.Y + (to.Y - from.Y) * amount,
                from.Width + (to.Width - from.Width) * amount,
                from.Height + (to.Height - from.Height) * amount);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && Sprites != null)
            {
                foreach (FragmentSprite sprite in Sprites)
                {
                    if (sprite != null && sprite.Image != null)
                    {
                        sprite.Image.Dispose();
                        sprite.Image = null;
                    }
                }
                Sprites.Clear();
            }
            base.Dispose(disposing);
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FuSheng.AnimationKit.WinForms
{
    /// <summary>
    /// A WinForms button with luminous hover shine, gradient surface, soft shadow, and press animation.
    /// </summary>
    public class ShineButton : Control
    {
        private readonly Timer animationTimer;
        private Color currentColor;
        private Color targetColor;
        private float hoverProgress;
        private float targetHoverProgress;
        private float pressProgress;
        private float targetPressProgress;
        private float shineProgress = -0.45F;
        private bool mouseDown;

        public Color BaseColor { get; set; } = Color.FromArgb(59, 130, 246);
        public Color HoverColor { get; set; } = Color.FromArgb(37, 99, 235);
        public Color PressedColor { get; set; } = Color.FromArgb(29, 78, 216);
        public Color TextColor { get; set; } = Color.White;
        public int Radius { get; set; } = 18;
        public bool ShineEnabled { get; set; } = true;
        public bool ShadowEnabled { get; set; } = true;
        public float ShineSpeed { get; set; } = 0.030F;
        public AnimationQuality Quality { get; set; } = AnimationQuality.Standard;

        public ShineButton()
        {
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
            BackColor = Color.FromArgb(246, 248, 252);

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            SetStyle(ControlStyles.StandardClick, false);

            currentColor = BaseColor;
            targetColor = BaseColor;

            animationTimer = new Timer { Interval = 16 };
            animationTimer.Tick += delegate
            {
                currentColor = AnimationMath.LerpColor(currentColor, targetColor, 0.22F);
                hoverProgress = AnimationMath.Approach(hoverProgress, targetHoverProgress, 0.20F);
                pressProgress = AnimationMath.Approach(pressProgress, targetPressProgress, 0.28F);

                if (targetHoverProgress > 0.5F && ShineEnabled && Quality != AnimationQuality.Minimal)
                {
                    shineProgress += ShineSpeed;
                    if (shineProgress > 1.25F)
                    {
                        shineProgress = -0.55F;
                    }
                }

                Invalidate();

                if (ColorDistance(currentColor, targetColor) < 4 &&
                    Math.Abs(hoverProgress - targetHoverProgress) < 0.01F &&
                    Math.Abs(pressProgress - targetPressProgress) < 0.01F &&
                    targetHoverProgress < 0.5F)
                {
                    currentColor = targetColor;
                    hoverProgress = targetHoverProgress;
                    pressProgress = targetPressProgress;
                    shineProgress = -0.45F;
                    animationTimer.Stop();
                    Invalidate();
                }
            };
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            currentColor = BaseColor;
            targetColor = BaseColor;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            targetHoverProgress = 1F;
            if (!mouseDown)
            {
                AnimateTo(HoverColor);
            }
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            mouseDown = false;
            targetHoverProgress = 0F;
            targetPressProgress = 0F;
            AnimateTo(BaseColor);
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            mouseDown = true;
            targetPressProgress = 1F;
            AnimateTo(PressedColor);
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            mouseDown = false;
            targetPressProgress = 0F;

            if (ClientRectangle.Contains(e.Location))
            {
                targetHoverProgress = 1F;
                AnimateTo(HoverColor);
                OnClick(EventArgs.Empty);
            }
            else
            {
                targetHoverProgress = 0F;
                AnimateTo(BaseColor);
            }

            base.OnMouseUp(e);
        }

        private void AnimateTo(Color color)
        {
            targetColor = color;
            if (!animationTimer.Enabled)
            {
                animationTimer.Start();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Color parentBack = AnimationMath.ResolveBackColor(this, Color.FromArgb(246, 248, 252));
            e.Graphics.Clear(parentBack);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (Width <= 4 || Height <= 4)
            {
                return;
            }

            float hover = AnimationEasing.Smooth(hoverProgress);
            float press = AnimationEasing.Smooth(pressProgress);

            RectangleF rect = new RectangleF(
                0.8F + press * 1.3F,
                1.2F - hover * 0.8F + press * 1.5F,
                Width - 1.6F - press * 2.6F,
                Height - 2.4F - press * 2.4F);

            Color topColor = AnimationMath.Lighten(currentColor, 9 + (int)(6 * hover));
            Color bottomColor = AnimationMath.Darken(currentColor, 6 + (int)(8 * hover));

            if (ShadowEnabled && Quality != AnimationQuality.Minimal)
            {
                int shadowLayers = Quality == AnimationQuality.Low ? 3 : 5;
                AnimationMath.DrawSoftShadow(e.Graphics, rect, Radius, currentColor, 16 + 34 * hover - 5 * press, 2.5F + hover * 2F, shadowLayers);
            }

            using (GraphicsPath path = AnimationMath.RoundPath(rect, Radius))
            using (LinearGradientBrush brush = new LinearGradientBrush(rect, topColor, bottomColor, 90F))
            {
                e.Graphics.FillPath(brush, path);

                if (Quality != AnimationQuality.Minimal)
                {
                    using (LinearGradientBrush upperGlow = new LinearGradientBrush(
                        rect,
                        Color.FromArgb(46 + (int)(34 * hover), 255, 255, 255),
                        Color.FromArgb(0, 255, 255, 255),
                        90F))
                    {
                        e.Graphics.SetClip(path);
                        e.Graphics.FillRectangle(upperGlow, rect.X, rect.Y, rect.Width, rect.Height * 0.45F);
                        e.Graphics.ResetClip();
                    }
                }

                if (ShineEnabled && hover > 0.03F && Quality != AnimationQuality.Low && Quality != AnimationQuality.Minimal)
                {
                    float x = -Width * 0.45F + shineProgress * Width * 1.65F;
                    using (GraphicsPath shine = new GraphicsPath())
                    using (SolidBrush shineBrush = new SolidBrush(Color.FromArgb((int)(38 * hover), 255, 255, 255)))
                    {
                        shine.AddPolygon(new PointF[]
                        {
                            new PointF(x, rect.Top - 20),
                            new PointF(x + 42, rect.Top - 20),
                            new PointF(x + 108, rect.Bottom + 20),
                            new PointF(x + 66, rect.Bottom + 20)
                        });

                        e.Graphics.SetClip(path);
                        e.Graphics.FillPath(shineBrush, shine);
                        e.Graphics.ResetClip();
                    }
                }

                using (Pen border = new Pen(Color.FromArgb(60 + (int)(55 * hover), 255, 255, 255), 1.15F))
                {
                    e.Graphics.DrawPath(border, path);
                }
            }

            Rectangle textRect = new Rectangle((int)rect.X, (int)(rect.Y - hover * 0.6F + press * 1.0F), (int)rect.Width, (int)rect.Height);
            TextRenderer.DrawText(e.Graphics, Text, Font, textRect, TextColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                animationTimer.Dispose();
            }
            base.Dispose(disposing);
        }

        private static int ColorDistance(Color a, Color b)
        {
            return Math.Abs(a.R - b.R) + Math.Abs(a.G - b.G) + Math.Abs(a.B - b.B) + Math.Abs(a.A - b.A);
        }
    }
}

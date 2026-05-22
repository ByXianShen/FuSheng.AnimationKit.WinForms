using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FuSheng.AnimationKit.WinForms
{
    public static class AnimationMath
    {
        public static float Approach(float value, float target, float speed)
        {
            return value + (target - value) * speed;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static int ClampInt(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static Color LerpColor(Color from, Color to, float amount)
        {
            amount = Clamp(amount, 0F, 1F);
            int r = (int)(from.R + (to.R - from.R) * amount);
            int g = (int)(from.G + (to.G - from.G) * amount);
            int b = (int)(from.B + (to.B - from.B) * amount);
            int a = (int)(from.A + (to.A - from.A) * amount);
            return Color.FromArgb(a, r, g, b);
        }

        public static Color Darken(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Max(0, color.R - amount),
                Math.Max(0, color.G - amount),
                Math.Max(0, color.B - amount));
        }

        public static Color Lighten(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(255, color.R + amount),
                Math.Min(255, color.G + amount),
                Math.Min(255, color.B + amount));
        }

        public static Color WithAlpha(Color color, int alpha)
        {
            return Color.FromArgb(ClampInt(alpha, 0, 255), color.R, color.G, color.B);
        }

        public static Color ResolveBackColor(Control control, Color fallback)
        {
            Control current = control == null ? null : control.Parent;
            while (current != null)
            {
                if (current.BackColor.A == 255)
                {
                    return current.BackColor;
                }
                current = current.Parent;
            }
            return fallback;
        }

        public static GraphicsPath RoundPath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();

            if (rect.Width <= 1 || rect.Height <= 1)
            {
                path.AddRectangle(new RectangleF(rect.X, rect.Y, Math.Max(1F, rect.Width), Math.Max(1F, rect.Height)));
                return path;
            }

            float r = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2F);
            if (r < 1F)
            {
                path.AddRectangle(rect);
                return path;
            }

            float d = r * 2F;
            path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void DrawSoftShadow(Graphics graphics, RectangleF rect, float radius, Color color, float alpha, float yOffset, int layers = 5)
        {
            if (graphics == null || alpha <= 0 || layers <= 0)
            {
                return;
            }

            for (int i = layers; i >= 1; i--)
            {
                float spread = i * 2.2F;
                int layerAlpha = (int)(alpha * (layers - i + 1) / (layers * 1.65F));

                RectangleF shadowRect = new RectangleF(
                    rect.X - spread,
                    rect.Y - spread + yOffset,
                    rect.Width + spread * 2F,
                    rect.Height + spread * 2F);

                using (GraphicsPath path = RoundPath(shadowRect, radius + spread))
                using (SolidBrush brush = new SolidBrush(WithAlpha(color, layerAlpha)))
                {
                    graphics.FillPath(brush, path);
                }
            }
        }
    }
}

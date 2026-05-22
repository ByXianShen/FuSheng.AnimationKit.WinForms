using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace FuSheng.AnimationKit.WinForms
{
    /// <summary>
    /// Captures visible child controls as image fragments and animates them during page switching.
    /// </summary>
    public sealed class FragmentTransitionManager : IDisposable
    {
        private readonly Control host;
        private Timer timer;
        private DateTime startTime;
        private FragmentTransitionLayer layer;
        private Control activeFromPage;
        private Control activeToPage;
        private Action completionCallback;

        public FragmentTransitionOptions Options { get; } = new FragmentTransitionOptions();
        public bool IsRunning { get; private set; }
        public event EventHandler Completed;

        public FragmentTransitionManager(Control host)
        {
            this.host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public void Switch(Control fromPage, Control toPage, Action completed = null)
        {
            if (fromPage == null) throw new ArgumentNullException(nameof(fromPage));
            if (toPage == null) throw new ArgumentNullException(nameof(toPage));
            if (IsRunning || fromPage == toPage) return;
            if (host.Width <= 0 || host.Height <= 0) return;

            StopInternal(false);

            activeFromPage = fromPage;
            activeToPage = toPage;
            completionCallback = completed;

            Rectangle pageBounds = fromPage.Bounds;
            if (pageBounds.Width <= 0 || pageBounds.Height <= 0)
            {
                pageBounds = host.ClientRectangle;
            }

            host.SuspendLayout();

            toPage.Bounds = pageBounds;
            fromPage.Visible = true;
            toPage.Visible = true;
            toPage.SendToBack();
            fromPage.BringToFront();

            fromPage.PerformLayout();
            toPage.PerformLayout();

            FragmentTransitionOptions snapshotOptions = Options.Clone();
            List<FragmentSprite> sprites = new List<FragmentSprite>();
            sprites.AddRange(CaptureTransitionSprites(fromPage, false, pageBounds, snapshotOptions.Seed + 17, snapshotOptions));
            sprites.AddRange(CaptureTransitionSprites(toPage, true, pageBounds, snapshotOptions.Seed + 839, snapshotOptions));

            if (snapshotOptions.HidePagesDuringTransition)
            {
                fromPage.Visible = false;
                toPage.Visible = false;
            }

            layer = new FragmentTransitionLayer
            {
                Bounds = host.ClientRectangle,
                PageRect = pageBounds,
                Options = snapshotOptions,
                Sprites = sprites,
                Progress = 0F,
                BackColor = host.BackColor
            };

            host.Controls.Add(layer);
            layer.BringToFront();
            host.ResumeLayout(false);

            IsRunning = true;
            startTime = DateTime.UtcNow;

            timer = new Timer { Interval = Math.Max(1, snapshotOptions.TimerInterval) };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        public void Cancel()
        {
            StopInternal(true);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (layer == null)
            {
                StopInternal(false);
                return;
            }

            float progress = (float)((DateTime.UtcNow - startTime).TotalMilliseconds / Math.Max(1, Options.Duration));
            if (progress >= 1F)
            {
                progress = 1F;
            }

            layer.Progress = progress;
            layer.Invalidate();

            if (progress >= 1F)
            {
                StopInternal(false);
            }
        }

        private List<FragmentSprite> CaptureTransitionSprites(Control page, bool incoming, Rectangle pageBounds, int seed, FragmentTransitionOptions options)
        {
            List<FragmentSprite> sprites = new List<FragmentSprite>();
            if (page == null || host == null)
            {
                return sprites;
            }

            List<Control> targets = GetCaptureTargets(page, options.CaptureMode);
            Random random = new Random(seed);
            int index = 0;
            int maxSprites = options.Quality == AnimationQuality.Low ? Math.Min(options.MaxSprites, 24) : options.MaxSprites;
            if (options.Quality == AnimationQuality.Minimal)
            {
                maxSprites = Math.Min(maxSprites, 8);
            }

            foreach (Control control in targets)
            {
                if (sprites.Count >= maxSprites) break;
                if (control == null || !control.Visible || control.Width <= 2 || control.Height <= 2) continue;

                Bitmap bitmap = null;
                try
                {
                    bitmap = new Bitmap(control.Width, control.Height, PixelFormat.Format32bppArgb);
                    control.DrawToBitmap(bitmap, new Rectangle(Point.Empty, control.Size));
                }
                catch
                {
                    bitmap?.Dispose();
                    continue;
                }

                Point screenPoint = control.PointToScreen(Point.Empty);
                Point hostPoint = host.PointToClient(screenPoint);
                RectangleF targetRect = new RectangleF(hostPoint.X, hostPoint.Y, control.Width, control.Height);
                RectangleF scatterRect = CreateScatterRect(targetRect, pageBounds, random, incoming, index, options);

                float angle = options.EnableRotation ? (float)((random.NextDouble() - 0.5) * options.RotationAmount) : 0F;
                FragmentSprite sprite = new FragmentSprite
                {
                    Image = bitmap,
                    FromRect = incoming ? scatterRect : targetRect,
                    ToRect = incoming ? targetRect : scatterRect,
                    RotationFrom = incoming ? angle : 0F,
                    RotationTo = incoming ? 0F : angle,
                    Incoming = incoming,
                    Delay = incoming
                        ? Lerp(options.IncomingDelayMin, options.IncomingDelayMax, (float)random.NextDouble())
                        : Lerp(0F, options.OutgoingDelayMax, (float)random.NextDouble()),
                    Duration = incoming
                        ? Lerp(options.IncomingDurationMin, options.IncomingDurationMax, (float)random.NextDouble())
                        : Lerp(options.OutgoingDurationMin, options.OutgoingDurationMax, (float)random.NextDouble()),
                    Depth = incoming ? 100 + index : index
                };

                sprites.Add(sprite);
                index++;
            }

            return sprites;
        }

        private List<Control> GetCaptureTargets(Control page, FragmentCaptureMode mode)
        {
            List<Control> targets = new List<Control>();

            if (mode == FragmentCaptureMode.Auto)
            {
                foreach (Control child in page.Controls)
                {
                    if (child is TableLayoutPanel)
                    {
                        foreach (Control grandChild in child.Controls)
                        {
                            targets.Add(grandChild);
                        }
                        return targets;
                    }
                }

                mode = FragmentCaptureMode.DirectChildren;
            }

            if (mode == FragmentCaptureMode.DirectChildren)
            {
                foreach (Control child in page.Controls)
                {
                    targets.Add(child);
                }
                return targets;
            }

            CollectLeafControls(page, targets);
            return targets;
        }

        private static void CollectLeafControls(Control parent, List<Control> targets)
        {
            foreach (Control child in parent.Controls)
            {
                if (child.Controls.Count == 0)
                {
                    targets.Add(child);
                }
                else
                {
                    CollectLeafControls(child, targets);
                }
            }
        }

        private RectangleF CreateScatterRect(RectangleF source, Rectangle pageBounds, Random random, bool incoming, int index, FragmentTransitionOptions options)
        {
            float centerX = source.X + source.Width / 2F;
            float centerY = source.Y + source.Height / 2F;
            float pageCenterX = pageBounds.X + pageBounds.Width / 2F;
            float pageCenterY = pageBounds.Y + pageBounds.Height / 2F;

            double angle = random.NextDouble() * Math.PI * 2.0;
            float distance = Lerp(options.ScatterDistanceMin, options.ScatterDistanceMax, (float)random.NextDouble());

            if (options.Quality == AnimationQuality.Low)
            {
                distance *= 0.60F;
            }
            else if (options.Quality == AnimationQuality.Minimal)
            {
                distance *= 0.35F;
            }

            float fromCenterBiasX = (centerX - pageCenterX) * 0.10F;
            float fromCenterBiasY = (centerY - pageCenterY) * 0.10F;

            float x = centerX + (float)Math.Cos(angle) * distance + fromCenterBiasX;
            float y = centerY + (float)Math.Sin(angle) * distance + fromCenterBiasY;

            if ((index % 3) == 0)
            {
                x += incoming ? -44F : 44F;
            }
            else if ((index % 3) == 1)
            {
                y += incoming ? 34F : -34F;
            }

            float scale = 0.88F + (float)random.NextDouble() * 0.16F;
            float width = Math.Max(24F, source.Width * scale);
            float height = Math.Max(18F, source.Height * scale);

            float minX = pageBounds.X + 28F;
            float maxX = pageBounds.Right - 28F;
            float minY = pageBounds.Y + 28F;
            float maxY = pageBounds.Bottom - 28F;

            x = AnimationMath.Clamp(x, minX, maxX);
            y = AnimationMath.Clamp(y, minY, maxY);

            return new RectangleF(x - width / 2F, y - height / 2F, width, height);
        }

        private static float Lerp(float from, float to, float amount)
        {
            amount = AnimationMath.Clamp(amount, 0F, 1F);
            return from + (to - from) * amount;
        }

        private void StopInternal(bool cancelled)
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }

            if (layer != null)
            {
                host.Controls.Remove(layer);
                layer.Dispose();
                layer = null;
            }

            if (activeFromPage != null && activeToPage != null)
            {
                activeFromPage.Visible = cancelled;
                activeToPage.Visible = !cancelled;
                activeToPage.BringToFront();
            }

            IsRunning = false;
            Action callback = completionCallback;
            completionCallback = null;
            activeFromPage = null;
            activeToPage = null;

            if (!cancelled)
            {
                callback?.Invoke();
                Completed?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            StopInternal(true);
        }
    }
}

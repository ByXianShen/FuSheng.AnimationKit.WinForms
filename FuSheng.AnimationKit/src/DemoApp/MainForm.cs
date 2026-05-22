using System;
using System.Drawing;
using System.Windows.Forms;
using FuSheng.AnimationKit.WinForms;

namespace DemoApp
{
    public class MainForm : Form
    {
        private readonly Panel pageHost;
        private readonly Panel pageOne;
        private readonly Panel pageTwo;
        private readonly FragmentTransitionManager transition;
        private bool showingFirst = true;

        public MainForm()
        {
            Text = "FuSheng AnimationKit Demo";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(920, 680);
            BackColor = Color.FromArgb(246, 248, 252);
            Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Regular);

            TableLayoutPanel root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = BackColor,
                Padding = new Padding(18, 16, 18, 12),
                ColumnCount = 1,
                RowCount = 3
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 124F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 82F));
            Controls.Add(root);

            ShineHeaderPanel header = new ShineHeaderPanel
            {
                Dock = DockStyle.Fill,
                TitleText = "FuSheng AnimationKit",
                SubtitleText = "Luminous shine and fragment transition for WinForms.",
                VersionText = "Demo",
                Margin = new Padding(0, 0, 0, 4)
            };
            root.Controls.Add(header, 0, 0);

            pageHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BackColor,
                Padding = new Padding(0, 12, 0, 12)
            };
            root.Controls.Add(pageHost, 0, 1);

            pageOne = CreatePage("Page One", "This page uses normal WinForms controls. The transition manager captures them as fragments.", Color.FromArgb(59, 130, 246));
            pageTwo = CreatePage("Page Two", "Elements scatter, fade, rotate, and regroup into the next page.", Color.FromArgb(139, 92, 246));
            pageHost.Controls.Add(pageOne);
            pageHost.Controls.Add(pageTwo);
            pageTwo.Visible = false;
            pageHost.Resize += delegate { LayoutPages(); };
            LayoutPages();

            transition = new FragmentTransitionManager(pageHost);
            transition.Options.Duration = 1680;
            transition.Options.Quality = AnimationQuality.Standard;

            TableLayoutPanel buttons = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = BackColor,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(0, 10, 0, 0)
            };
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            root.Controls.Add(buttons, 0, 2);

            ShineButton high = CreateButton("High Quality", Color.FromArgb(59, 130, 246));
            high.Click += delegate { transition.Options.Quality = AnimationQuality.High; SwitchPage(); };

            ShineButton standard = CreateButton("Standard", Color.FromArgb(129, 140, 248));
            standard.Click += delegate { transition.Options.Quality = AnimationQuality.Standard; SwitchPage(); };

            ShineButton low = CreateButton("Low Quality", Color.FromArgb(16, 185, 129));
            low.Click += delegate { transition.Options.Quality = AnimationQuality.Low; SwitchPage(); };

            buttons.Controls.Add(high, 0, 0);
            buttons.Controls.Add(standard, 1, 0);
            buttons.Controls.Add(low, 2, 0);
        }

        private void LayoutPages()
        {
            Rectangle bounds = new Rectangle(0, 12, pageHost.ClientSize.Width, pageHost.ClientSize.Height - 24);
            pageOne.Bounds = bounds;
            pageTwo.Bounds = bounds;
        }

        private Panel CreatePage(string title, string description, Color accent)
        {
            Panel page = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(36, 28, 36, 28),
                ColumnCount = 1,
                RowCount = 5
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            page.Controls.Add(layout);

            Label titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft YaHei UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.White
            };
            layout.Controls.Add(titleLabel, 0, 0);

            Label descLabel = new Label
            {
                Text = description,
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft YaHei UI", 12.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.White
            };
            layout.Controls.Add(descLabel, 0, 1);

            ShineButton primary = CreateButton("Luminous Shine Button", accent);
            primary.Margin = new Padding(0, 12, 0, 12);
            layout.Controls.Add(primary, 0, 2);

            ShineButton secondary = CreateButton("Another Animated Element", AnimationMath.Darken(accent, 20));
            secondary.Margin = new Padding(0, 12, 0, 12);
            layout.Controls.Add(secondary, 0, 3);

            return page;
        }

        private ShineButton CreateButton(string text, Color color)
        {
            return new ShineButton
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(8, 0, 8, 0),
                Font = new Font("Microsoft YaHei UI", 12.5F, FontStyle.Bold),
                BaseColor = color,
                HoverColor = AnimationMath.Darken(color, 18),
                PressedColor = AnimationMath.Darken(color, 32),
                Radius = 18
            };
        }

        private void SwitchPage()
        {
            if (transition.IsRunning) return;

            if (showingFirst)
            {
                transition.Options.FromAccent = Color.FromArgb(59, 130, 246);
                transition.Options.ToAccent = Color.FromArgb(139, 92, 246);
                transition.Switch(pageOne, pageTwo, delegate { showingFirst = false; });
            }
            else
            {
                transition.Options.FromAccent = Color.FromArgb(139, 92, 246);
                transition.Options.ToAccent = Color.FromArgb(59, 130, 246);
                transition.Switch(pageTwo, pageOne, delegate { showingFirst = true; });
            }
        }
    }
}

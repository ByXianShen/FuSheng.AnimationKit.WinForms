using System.Drawing;

namespace FuSheng.AnimationKit.WinForms
{
    public enum FragmentCaptureMode
    {
        Auto,
        DirectChildren,
        RecursiveLeafControls
    }

    public class FragmentTransitionOptions
    {
        public int Duration { get; set; } = 1680;
        public int TimerInterval { get; set; } = 15;
        public int Radius { get; set; } = 24;
        public int Seed { get; set; } = 147;
        public AnimationQuality Quality { get; set; } = AnimationQuality.Standard;
        public FragmentCaptureMode CaptureMode { get; set; } = FragmentCaptureMode.Auto;

        public bool EnableRotation { get; set; } = true;
        public bool EnableGradientFusion { get; set; } = true;
        public bool EnableLightBand { get; set; } = true;
        public bool HidePagesDuringTransition { get; set; } = true;

        public float RotationAmount { get; set; } = 9F;
        public float ScatterDistanceMin { get; set; } = 78F;
        public float ScatterDistanceMax { get; set; } = 198F;
        public float IncomingDelayMin { get; set; } = 0.30F;
        public float IncomingDelayMax { get; set; } = 0.52F;
        public float OutgoingDelayMax { get; set; } = 0.13F;
        public float IncomingDurationMin { get; set; } = 0.58F;
        public float IncomingDurationMax { get; set; } = 0.74F;
        public float OutgoingDurationMin { get; set; } = 0.48F;
        public float OutgoingDurationMax { get; set; } = 0.61F;

        public int MaxSprites { get; set; } = 80;
        public Color FromAccent { get; set; } = Color.FromArgb(59, 130, 246);
        public Color ToAccent { get; set; } = Color.FromArgb(139, 92, 246);

        public FragmentTransitionOptions Clone()
        {
            return (FragmentTransitionOptions)MemberwiseClone();
        }
    }
}

using System.Drawing;

namespace FuSheng.AnimationKit.WinForms
{
    internal sealed class FragmentSprite
    {
        public Bitmap Image { get; set; }
        public RectangleF FromRect { get; set; }
        public RectangleF ToRect { get; set; }
        public float RotationFrom { get; set; }
        public float RotationTo { get; set; }
        public float Delay { get; set; }
        public float Duration { get; set; }
        public bool Incoming { get; set; }
        public int Depth { get; set; }
    }
}

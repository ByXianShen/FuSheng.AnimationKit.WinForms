using System;

namespace FuSheng.AnimationKit.WinForms
{
    public static class AnimationEasing
    {
        public static float Smooth(float value)
        {
            value = AnimationMath.Clamp(value, 0F, 1F);
            return value * value * (3F - 2F * value);
        }

        public static float InOutCubic(float value)
        {
            value = AnimationMath.Clamp(value, 0F, 1F);
            if (value < 0.5F)
            {
                return 4F * value * value * value;
            }

            float f = -2F * value + 2F;
            return 1F - (f * f * f) / 2F;
        }

        public static float OutQuint(float value)
        {
            value = AnimationMath.Clamp(value, 0F, 1F);
            float t = 1F - value;
            return 1F - t * t * t * t * t;
        }

        public static float InOutSine(float value)
        {
            value = AnimationMath.Clamp(value, 0F, 1F);
            return (float)(-(Math.Cos(Math.PI * value) - 1.0) / 2.0);
        }
    }
}

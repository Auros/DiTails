using UnityEngine;

namespace DiTails.Utilities
{
    internal static class Constants
    {
        private static readonly Gradient _colorGradient = new Gradient()
        {
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(Color.black, 0),
                new GradientColorKey(new Color(1f, 0.188f, 0.188f), .33f),
                new GradientColorKey(Color.yellow, .66f),
                new GradientColorKey(new Color(0.388f, 1f, 0.388f), 0.95f),
                new GradientColorKey(Color.cyan, 1f)
            }
        };

        public static Color Evaluate(float value)
        {
            return _colorGradient.Evaluate(value);
        }
    }
}
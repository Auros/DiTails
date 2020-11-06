using UnityEngine;

namespace DiTails.Utilities
{
    internal static class Extensions
    {
        public static void CopyToClipboard(this string toCopy)
        {
            var editor = new TextEditor { text = toCopy };
            editor.SelectAll();
            editor.Copy();
        }
    }
}
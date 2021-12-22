using System.Reflection;
using System;
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

        public static U Upgrade<T, U>(this T monoBehaviour) where U : T where T : MonoBehaviour
        {
            return (U)Upgrade(monoBehaviour, typeof(U));
        }

        public static Component Upgrade(this Component monoBehaviour, Type upgradingType)
        {
            var originalType = monoBehaviour.GetType();

            var gameObject = monoBehaviour.gameObject;
            var upgradedDummyComponent = Activator.CreateInstance(upgradingType);
            foreach (FieldInfo info in originalType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                info.SetValue(upgradedDummyComponent, info.GetValue(monoBehaviour));
            }
            UnityEngine.Object.DestroyImmediate(monoBehaviour);
            bool goState = gameObject.activeSelf;
            gameObject.SetActive(false);
            var upgradedMonoBehaviour = gameObject.AddComponent(upgradingType);
            foreach (FieldInfo info in upgradingType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                info.SetValue(upgradedMonoBehaviour, info.GetValue(upgradedDummyComponent));
            }
            gameObject.SetActive(goState);
            return upgradedMonoBehaviour;
        }

    }
}
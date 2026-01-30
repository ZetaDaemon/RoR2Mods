using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;
using static RoR2.ColorCatalog;

#pragma warning disable Publicizer001 // Accessing a member that was not originally public
namespace ZetaItemBalance
{
    /*
    Credit to https://github.com/AndreLouisIssa/RoR2TierScaling for essentially all of the code here, used with permission
    */
    public static class Utils
    {
        public static Dictionary<ItemTier, Color> tierColors = new Dictionary<ItemTier, Color>();
        public static Dictionary<ItemTier, ItemTierDef> tiers = new Dictionary<ItemTier, ItemTierDef>();

        public static ItemTier ItemTierIndex(ItemTierDef tier)
        {
            var i = ItemTier.NoTier;
            if (tier is null)
            {
                return i;
            }
            if (Enum.TryParse(tier.name.Substring(0, tier.name.Length - 3), out i)) { }
            else if (Enum.TryParse(tier.name.Substring(0, tier.name.Length - 7), out i)) { }
            else
            {
                i = tier._tier;
            }
            if (i == ItemTier.AssignedAtRuntime)
            {
                i = ItemTier.NoTier;
            }
            return i;
        }

        public static void OnItemTierCatalogInit()
        {
            foreach (var t in RoR2.ContentManagement.ContentManager._itemTierDefs)
            {
                var i = ItemTierIndex(t);
                if (i is ItemTier.NoTier)
                    continue;
                tiers[i] = t;
                tierColors[i] = Border(GetColor(t.colorIndex), GetColor(t.darkColorIndex));
                ;
            }
        }

        public static Color Border(Color colorA, Color colorB)
        {
            Color.RGBToHSV(colorA.NoAlpha(), out var hA, out var sA, out var vA);
            Color.RGBToHSV(colorB.NoAlpha(), out var hB, out var sB, out var vB);
            return Color.HSVToRGB((hA + hB) / 2, (sA + sB) * 0.6f, (vA + vB) * 0.45f);
        }

        public static Texture2D Duplicate(this Texture texture, Rect? proj = null)
        {
            if (proj is null)
            {
                proj = new Rect(0, 0, texture.width, texture.height);
            }
            var rect = (Rect)proj;
            texture.filterMode = FilterMode.Point;
            RenderTexture rt = RenderTexture.GetTemporary(texture.width, texture.height);
            rt.filterMode = FilterMode.Point;
            RenderTexture.active = rt;
            Graphics.Blit(texture, rt);
            Texture2D texture2 = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
            texture2.ReadPixels(new Rect(rect.x, texture.height - rect.height - rect.y, rect.width, rect.height), 0, 0);
            texture2.Apply();
            RenderTexture.active = null;
            return texture2;
        }

        public static IEnumerable<Tuple<int, int>> GetEnumerator(this Texture2D texture)
        {
            for (int x = 0; x < texture.width; x++)
            for (int y = 0; y < texture.height; y++)
            {
                yield return new Tuple<int, int>(x, y);
            }
        }

        public static Texture2D Duplicate(this Texture texture, Func<int, int, Color, Color> func, Rect? proj = null)
        {
            if (proj is null)
            {
                proj = new Rect(0, 0, texture.width, texture.height);
            }
            var t = texture.Duplicate(proj);
            foreach (var xy in t.GetEnumerator())
            {
                var x = xy.Item1;
                var y = xy.Item2;
                t.SetPixel(x, y, func(x, y, t.GetPixel(x, y)));
            }
            t.Apply();
            return t;
        }

        public static Texture2D ToTexture2D(this Texture texture)
        {
            return (texture is Texture2D t2D) ? t2D : texture.Duplicate();
        }

        public static Texture2D ToReadable(this Texture texture)
        {
            var t2D = texture.ToTexture2D();
            return (t2D.isReadable) ? t2D : t2D.Duplicate();
        }

        public static Color NoAlpha(this Color color)
        {
            return new Color(color.r, color.g, color.b);
        }

        public static Color Border(Color color)
        {
            Color.RGBToHSV(color.NoAlpha(), out var h, out var s, out var v);
            return Color.HSVToRGB(h, s * 1.2f, v * 0.9f);
        }

        public static Texture2D Stain(Texture texture, Color stain)
        {
            Color? aura = null;
            return texture.Duplicate(
                (x, y, c) =>
                {
                    if (aura is null)
                    {
                        aura = Border(c);
                    }
                    var a = aura.Value;
                    var d = c - a;
                    var m = d.r * d.r + d.g * d.g + d.b * d.b;
                    var s = (float)Math.Abs(Math.Tanh(4 * m));
                    return Color.Lerp(stain, c, s).AlphaMultiplied(c.a);
                }
            );
        }

        public static void FixIconRarity(ItemDef item, ItemTier desiredTier)
        {
            var color = tierColors[desiredTier];
            var sprite = item.pickupIconSprite;
            var texture = sprite.texture.ToReadable();

            texture = Stain(texture, color);
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            item.pickupIconSprite = sprite;
        }
    }
}

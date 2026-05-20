using System.IO;
using UnityEngine;

namespace ProjectDaydream.Common
{
    public static class Utilities
    {
        public static void UpdateBar(this RectTransform fillBar, float numerator, float denominator, RectTransform backgroundBar)
        {
            var value = numerator / denominator;
            var barSize = Mathf.Abs(backgroundBar.rect.width);
            fillBar.offsetMax = new Vector2(-(barSize - (barSize * value)), 0);
        }

        public static Vector3 InventoryPoolPosition => new Vector3(50, -100, 50);
        
        public static Texture2D ReadImageFromFile(string filePath) {

            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath)) 	{
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); // Auto resizes the image
            }
            return tex;
        }
        
        public static Texture2D SaveCameraView(Camera cam)
        {
            var width = 160;
            var height = 90;
            RenderTexture screenTexture = new RenderTexture(width, height, 16);
            cam.targetTexture = screenTexture;
            RenderTexture.active = screenTexture;
            cam.Render();
            cam.targetTexture = null;

            Texture2D renderedTexture = new Texture2D(width, height);
            renderedTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            RenderTexture.active = null;

            return renderedTexture;
        }
    }
}
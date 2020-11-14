using System;
using System.Drawing;
using System.Numerics;

namespace EuropeanWars.GameMap {
    [Serializable]
    struct BorderPoint {
        private static readonly Vector2[] neighbours = new Vector2[4] {
            new Vector2(0, -1),
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(-1, 0),
        };

        public Vector2 position;
        public Vector2 fixedPosition;
        public string borderProvince;

        public static BorderPoint GetPoint(int x, int y, Bitmap bitmap) {
            BorderPoint result = new BorderPoint();
            result.position = new Vector2(x, y);
            result.fixedPosition = result.position;

            Color pixel = bitmap.GetPixel(x, y);
            bool bo = true;

            for (int i = 0; i < neighbours.Length; i++) {
                Color b = bitmap.GetPixel(x + (int)neighbours[i].X, y + (int)neighbours[i].Y);
                if (pixel != b) {
                    result.fixedPosition += neighbours[i] * 0.5f;
                    result.borderProvince = b.R.ToString("X2") + b.G.ToString("X2") + b.B.ToString("X2");
                    return result;
                }
            }
            return new BorderPoint() { position = new Vector2(-1, -1) };
        }
    }
}

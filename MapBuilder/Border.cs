using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace EuropeanWars.GameMap {
    [Serializable]
    class Border {
        public string firstProvince;
        public string secondProvince;
        public List<List<Vector2>> vertices = new List<List<Vector2>>();
    }
}

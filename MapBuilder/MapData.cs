using System;
using System.Collections.Generic;

namespace EuropeanWars.GameMap {
    [Serializable]
    class MapData {
        public List<Province> provinces = new List<Province>();
        public List<Border> borders = new List<Border>();
    }
}

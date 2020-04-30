using System;
using System.Collections.Generic;
using System.Numerics;

namespace MapBuilder {
    [Serializable]
    class Mesh {
        public List<Vector2> vertices = new List<Vector2>();
        public List<int> indices = new List<int>();
    }
}

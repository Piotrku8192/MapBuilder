using System;

namespace MapBuilder {
    [Serializable]
    class Province {
        public Mesh mesh = new Mesh();
        public Border[] borders = new Border[0];
    }
}

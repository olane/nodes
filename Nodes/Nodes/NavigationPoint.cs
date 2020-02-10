using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Nodes
{
    public class NavigationPoint
    {
        public GraphPoint graphRef;
        public NavigationPoint Parent;
        public float F;
        public float G;
        public float H;
    }
}

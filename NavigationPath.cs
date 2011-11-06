using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Nodes
{
    public class NavigationPath
    {
        public List<Vector2> Points;
        public int startNodeId;
        public int endNodeId;

        public NavigationPath()
        {
            Points = new List<Vector2>();
        }
    }
}

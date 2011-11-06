using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Nodes
{
    public class GraphPoint
    {
        public Vector2 Position;
        public List<GraphPoint> connectedNodes;

        public GraphPoint()
        {
            connectedNodes = new List<GraphPoint>();
        }
    }
}

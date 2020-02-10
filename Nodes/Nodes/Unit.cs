using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Nodes
{
    public class Unit
    {

        public int OwnerId
        { get; set; }

        public Vector2 Position
        { get; set; }

        public Vector2 Velocity
        { get; set; }

        public int DestinationId
        { get; set; }

        public int SourceId
        { get; set; }

        public float AStarPathProgress
        { get; set; }

        public int AStarPathId
        { get; set; }

        public Unit(int owner, Vector2 pos, Vector2 vel, int destination, int source)
        {
            OwnerId = owner;
            Position = pos;
            Velocity = vel;
            DestinationId = destination;
            SourceId = source;
        }

    }
}

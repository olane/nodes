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
    class Node
    {
        public Node(Vector2 pos, int units, int owner)
        {
            Position = pos;
            UnitCount = units;
            OwnerId = owner;
        }     

        public Vector2 Position
        { get; set; }

        public int UnitCount
        { get; set; }

        public int OwnerId
        { get; set; }



        

    }
}

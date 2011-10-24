using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
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
            position = pos;
            unitCount = units;
            ownerId = owner;
        }

        private Vector2 position;
        private int unitCount;
        private int ownerId;

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        public int UnitCount
        {
            get { return unitCount; }
            set { unitCount = value; }
        }
        public int OwnerId
        {
            get { return ownerId; }
            set { ownerId = value; }
        }



        

    }
}

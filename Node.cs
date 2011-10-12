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
        private Vector2 position;
        private uint unitCount;
        private uint ownerId;

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        public uint UnitCount
        {
            get { return unitCount; }
            set { unitCount = value; }
        }
        public uint OwnerId
        {
            get { return ownerId; }
            set { ownerId = value; }
        }

    }
}

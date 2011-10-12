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
    class Unit
    {
        private uint ownerId;
        private Vector2 position;
        private Vector2 velocity;
        private uint destinationId;

        public uint OwnerId
        {
            get { return ownerId; }
            set { ownerId = value; }
        }
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        public uint DestinationId
        {
            get { return destinationId; }
            set { destinationId = value; }
        }
    }
}

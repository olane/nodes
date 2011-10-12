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
    class Player
    {
        private Color color;
        private bool isAlive;
        private bool isHuman;

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }
        public bool IsAlive
        {
            get { return isAlive; }
            set { isAlive = value; }
        }
        public bool IsHuman
        {
            get { return isHuman; }
            set { isHuman = value; }
        }

    }
}

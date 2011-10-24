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
        public Color Color
        { get; set; }

        public bool IsAlive
        { get; set; }

        public bool IsHuman
        { get; set; }

    }
}

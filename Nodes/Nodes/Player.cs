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
    public class Player : ICloneable
    {
        public Color Color
        { get; set; }

        public bool IsAlive
        { get; set; }

        public bool IsHuman
        { get; set; }

        public float GrowthRate
        { get; set; }


        public Player(Color color, bool isAlive, bool isHuman, float growthRate)
        {
            Color = color;
            IsAlive = isAlive;
            IsHuman = isHuman;
            GrowthRate = growthRate;
        }

        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}

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
    public class Node : ICloneable
    {
        private float _unitProgress;

        public Vector2 Position
        { get; set; }

        public int UnitCount
        { get; set; }

        public int OwnerId
        { get; set; }

        public float UnitProgress
        {
            get
            {
                return _unitProgress;
            }
            set
            {
                if (value >= 1)
                {
                    _unitProgress = value % 1;
                    UnitCount += 1;
                }
                else
                {
                    _unitProgress = value;
                }
            }
        }

        public bool Selected
        { get; set; }

        public Node(Vector2 pos, int units, int owner, Random r)
        {
            UnitProgress = (float)(r.NextDouble());
            Position = pos;
            UnitCount = units;
            OwnerId = owner;
            Selected = false;
        }

        public float CalcNodeRadius()
        {
            float x = UnitCount + UnitProgress;
            return (float)(140/(1 + Math.Pow(2, -x/25)) - 65);
            //return 5 + UnitCount + UnitProgress;
        }

        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion


    }
}

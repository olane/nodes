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
        private float _unitProgress;

        public Vector2 Position
        { get; set; }

        public int UnitCount
        { get; set; }

        public int OwnerId
        { get; set; }

        public float BuildSpeed
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

        public Node(Vector2 pos, int units, int owner, float speed, Random r)
        {
            UnitProgress = (float)(r.NextDouble());
            Position = pos;
            UnitCount = units;
            OwnerId = owner;
            BuildSpeed = speed;
            Selected = false;
        }
        

    }
}

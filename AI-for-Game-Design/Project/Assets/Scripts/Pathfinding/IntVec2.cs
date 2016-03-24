using UnityEngine;
using System.Collections;

namespace Graph
{
    /// <summary>
    /// X-Y pair, as integers for easy access of array.
    /// </summary>
    public struct IntVec2
    {
        public readonly int x, y;

        /// <summary>
        /// Short for new IntVec2(0, 1);
        /// </summary>
        public static IntVec2 up { get { return new IntVec2(0, 1); } }
        /// <summary>
        /// Short for new IntVec2(0, -1);
        /// </summary>
        public static IntVec2 down { get { return new IntVec2(0, -1); } }
        /// <summary>
        /// Short for new IntVec2(1, 0);
        /// </summary>
        public static IntVec2 right { get { return new IntVec2(1, 0); } }
        /// <summary>
        /// Short for new IntVec2(-1, 0);
        /// </summary>
        public static IntVec2 left { get { return new IntVec2(-1, 0); } }
        /// <summary>
        /// Short for new IntVec2(-1, 1);
        /// </summary>
        public static IntVec2 up_left { get { return new IntVec2(-1, 1); } }
        /// <summary>
        /// Short for new IntVec2(-1, -1);
        /// </summary>
        public static IntVec2 down_left { get { return new IntVec2(-1, -1); } }
        /// <summary>
        /// Short for new IntVec2(1, 1);
        /// </summary>
        public static IntVec2 up_right { get { return new IntVec2(1, 1); } }
        /// <summary>
        /// Short for new IntVec2(1, -1);
        /// </summary>
        public static IntVec2 down_right { get { return new IntVec2(1, -1); } }

        public static explicit operator Vector2(IntVec2 v)
        {
            return new Vector2(v.x, v.y);
        }

        public IntVec2(int xIn, int yIn)
        {
            x = xIn;
            y = yIn;
        }

        /// <summary>
        /// Returns best match of input float.
        /// </summary>
        /// <param name="xIn">x as float</param>
        /// <param name="yIn">y as float</param>
        public IntVec2(float xIn, float yIn)
        {
            x = Mathf.RoundToInt(xIn);
            y = Mathf.RoundToInt(yIn);
        }

        public static explicit operator IntVec2(Vector2 v)
        {
            return new IntVec2(v.x, v.y);
        }

        public static IntVec2 operator +(IntVec2 v1, IntVec2 v2)
        {
            return new IntVec2(v1.x + v2.x, v1.y + v2.y);
        }

        public static IntVec2 operator -(IntVec2 v1, IntVec2 v2)
        {
            return new IntVec2(v1.x - v2.x, v1.y - v2.y);
        }

        override
        public string ToString()
        {
            return "(" + x + "," + y + ")";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGeneratorTest.Utils.Geometry
{
    /* Point order
     * 0 ......... 1
     *   ......... 
     *   ......... 
     *   ......... 
     *   ......... 
     *   ......... 
     *   ......... 
     * 3 ......... 2
     */

    /* Line order
     *       1
     *   ....>....
     *   .........
     *   .........
     * 0 V.......V 2
     *   .........
     *   .........
     *   ....>....
     *       3
     */
     
    public enum LineDirection
    {
        Vertical   = 0,
        Horizontal = 1,
        Diagonal   = 2
    }

    public enum RectDirection : int
    {
        Left   = 0,
        Top    = 1,
        Right  = 2,
        Bottom = 3
    }

    public static class EnumExtension
    {
        public static T Shift<T>(this T r, int n) where T : Enum
        {
            if (!EnumCheck(r.GetType()))
                throw new Exception();
            return (T)(object)((int) (object) r + n % Enum.GetValues(typeof(T)).Length);
        }

        private static bool EnumCheck(Type t)
        {
            var counter = 0;
            foreach (var x in Enum.GetValues(t))
            {
                if ((int) x == counter)
                    counter++;
                else
                    return false;
            }

            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.GeometryIO
{
    public class IOBone
    {
        public IOBone Parent { get; set; }
        public List<IOBone> Children { get; set; } 
        public Vector3 Translation { get; set; }

        public IOBone()
        {
            Children = new List<IOBone>();
        }
    }
}

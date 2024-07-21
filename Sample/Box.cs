using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public class Box : UtilityLib.TreeNodes.TreeNode<Box>
    {
        public int BoxNumber;
        public string BoxName;
        
        public Box(int boxNumber, string boxName) : base()
        {
            this.BoxNumber = boxNumber;
            this.BoxName = boxName;
        }
    }
}

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckInterSect.Library
{
    public class DistinctCategory : IEqualityComparer<Category>
    {
        public bool Equals(Category x, Category y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(Category obj)
        {
            return 1;
        }
    }
}

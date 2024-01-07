using System.Collections.Generic;

namespace Magneplot.Generator.Models
{
    public abstract class ModelSource
    {
        public abstract string Name { get; }

        public abstract List<Face> GetModel();
    }
}

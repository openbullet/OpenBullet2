using RuriLib.Models.Blocks;
using System.Collections.Generic;

namespace RuriLib.Models.Trees
{
    public class CategoryTreeNode
    {
        public CategoryTreeNode Parent { get; set; } = null;
        public List<CategoryTreeNode> SubCategories { get; set; } = new List<CategoryTreeNode> { };
        public List<BlockDescriptor> Descriptors { get; set; } = new List<BlockDescriptor> { };
        public string Name { get; set; } = null;

        public bool IsRoot => Parent == null;
    }
}

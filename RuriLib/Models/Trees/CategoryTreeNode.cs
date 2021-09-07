using RuriLib.Models.Blocks;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Models.Trees
{
    public class CategoryTreeNode
    {
        public CategoryTreeNode Parent { get; set; } = null;
        public List<CategoryTreeNode> SubCategories { get; set; } = new List<CategoryTreeNode> { };
        public List<BlockDescriptor> Descriptors { get; set; } = new List<BlockDescriptor> { };
        public string Name { get; set; } = null;

        public bool IsRoot => Parent == null;

        public BlockCategory Category
        {
            get
            {
                if (Descriptors.Count > 0)
                {
                    return Descriptors.First().Category;
                }

                var category = SubCategories.First().Category;
                category.Name = Name;
                return category;
            }
        }
    }
}

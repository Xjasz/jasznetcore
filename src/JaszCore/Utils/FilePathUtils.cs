using System.Linq;

namespace JaszCore.Core
{
    public class FilePathUtils
    {
        public FilePathUtils(string[] path) => Items = path;

        public FilePathUtils(FilePathUtils parent, string name) => Items = parent.Items.Concat(new string[] { name }).ToArray();

        public string[] Items { get; }

        private string _name;
        public string Name { get => _name ?? (_name = GetName()); }

        private FilePathUtils _parent;
        public FilePathUtils Parent { get => _parent ?? (_parent = GetParent()); }

        private string GetName()
        {
            return Items[^1];
        }

        private FilePathUtils GetParent()
        {
            if (Items.Length == 1)
            {
                return null;
            }
            return new FilePathUtils(Items.Skip(0).Take(Items.Length - 1).ToArray());
        }


        public override string ToString()
        {
            return string.Join("/", Items);
        }

    }
}

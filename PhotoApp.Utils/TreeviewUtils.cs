using System;
using System.Collections.Generic;
using System.Linq;

namespace PhotoApp.Utils
{
    public class TreeviewUtils
    {
        public class AlbumFolder
        {
            public AlbumFolder()
            {
                Children = new List<AlbumFolder>();
            }
            public string Id { get; set; }

            public string Header { get; set; }

            public List<AlbumFolder> Children { get; set; }

            public bool IsLeaf => Children.Count == 0;
        }

        public static void FillTree(ref AlbumFolder root, List<string> values)
        {
            while (values.Count != 0)
            {
                if (root != null && root.Children != null && root.Children.Any( x => x.Header == values[0]))
                {
                    AlbumFolder existingchild = root.Children.FirstOrDefault( x => x.Header == values[0]);
                    if (existingchild != null)
                    {
                        values.RemoveAt(0);
                        FillTree(ref existingchild, values);
                    }
                    continue;
                }
                AlbumFolder child = new AlbumFolder
                {
                    Header = values[0]
                };
                //if (root.Children == null)
                //{
                //    root.Children = new List<TreeviewStructure>();
                //}
                root.Children.Add(child);
                values.RemoveAt(0);
                FillTree(ref child, values);
            }
        }

        public static AlbumFolder FindRootNode(Dictionary<string, string> csvDictionary)
        {
            List<string> rootValues = new List<string>();
            foreach (string value in csvDictionary.Values)
            {
                if (value.Contains("/"))
                {
                    string[] splittedValues = value.Split("/", StringSplitOptions.RemoveEmptyEntries);
                    if (splittedValues.Length != 0)
                    {
                        rootValues.Add(splittedValues[0]);
                    }
                }
            }
            if (rootValues.Distinct().Count() == 1)
            {
                return new AlbumFolder
                {
                    Header = rootValues[0]
                };
            }
            return null;
        }
    }
}
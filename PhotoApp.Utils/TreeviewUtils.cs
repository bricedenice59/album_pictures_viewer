using System;
using System.Collections.Generic;
using System.Linq;
using PhotoApp.Utils.Models;

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

            public int NumberOfPhotos { get; set; }
        }

        public static void FillTree(ref AlbumFolder root, string id, int nbPhotos, List<string> values)
        {
            while (values.Count != 0)
            {
                if (root != null && root.Children != null && root.Children.Any( x => x.Header == values[0]))
                {
                    AlbumFolder existingchild = root.Children.FirstOrDefault( x => x.Header == values[0]);
                    if (existingchild != null)
                    {
                        values.RemoveAt(0);
                        FillTree(ref existingchild, id, nbPhotos, values);
                    }
                    continue;
                }
                AlbumFolder child = new AlbumFolder
                {
                    Id = id,
                    Header = values[0],
                    NumberOfPhotos = nbPhotos
                };
                root.Children.Add(child);
                values.RemoveAt(0);
                FillTree(ref child, id, nbPhotos, values);
            }
        }

        public static AlbumFolder FindRootNode(List<AlbumModelDto> albums)
        {
            List<string> rootValues = new List<string>();
            foreach (var value in albums)
            {
                if (value.Path.Contains("/"))
                {
                    string[] splittedValues = value.Path.Split("/", StringSplitOptions.RemoveEmptyEntries);
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
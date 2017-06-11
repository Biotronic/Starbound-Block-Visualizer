using System;
using System.Collections.Generic;
using System.IO;

namespace StarboundVisualizer.Components
{
    public class PathFinder
    {
        public static List<string> Paths { get; set; }

        static PathFinder()
        {
            Paths = new List<string> { @"D:\GitHub\FrackinUniverse\", @"E:\Starbound\unpacked\" };
        }

        public static string FindFile(string filename, string basePath = null)
        {
            filename = filename.Replace('/', '\\');


            if (filename.StartsWith(@"\"))
            {
                foreach (var path in Paths)
                {
                    var combined = Path.Combine(path, filename.Substring(1));
                    if (File.Exists(combined)) return combined;
                }
            }
            else if (basePath != null)
            {
                foreach (var path in Paths)
                {
                    var relativePath = new Uri(path).MakeRelativeUri(new Uri(basePath)).ToString();
                    if (Path.IsPathRooted(relativePath)) continue;

                    foreach (var otherPath in Paths)
                    {
                        var v2 = Path.Combine(otherPath, relativePath, filename);
                        if (File.Exists(v2)) return v2;
                    }
                }
                var combined = Path.Combine(basePath, filename);
                if (File.Exists(combined)) return combined;
            }
            if (File.Exists(filename)) return filename;
            return null;
        }
    }
}

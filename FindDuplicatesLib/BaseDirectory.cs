using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace FindDuplicates
{
    public class BaseDirectory
    {
        public IEnumerable<FileItem> GetFiles(string path)
        {
            var queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0 && !stopRequested)
            {
                path = queue.Dequeue();
                foreach (string subDir in Directory.GetDirectories(path))
                    queue.Enqueue(subDir);

                string[] files = Directory.GetFiles(path);
                for (var i = 0; i < files.Length; i++)
                    yield return new FileItem(files[i]);
            }
        }


        public BaseDirectory(List<string> pathsToSearch) 
        {
            paths = pathsToSearch;
        }

        private List<FileItem> ListFilesFromAllDirs() 
        {
            var toSearch = new List<FileItem>();

            foreach (var p in paths)
            {
                statusUpdater("listing directory: " + p);
                if (stopRequested)
                    return toSearch;
                var files = GetFiles(p);
                toSearch.AddRange(files.ToList());
            }

            return toSearch;
        }

        public Dictionary<long, List<FileItem>> SameSizeFiles() 
        {
            var toSearch = ListFilesFromAllDirs();
            statusUpdater("Comparing file sizes.");
            var LengthToFile = new Dictionary<long, List<FileItem>>();
            var totalFileCnt = 0;
            var relevantFileCnt = 0;
            foreach (var f in toSearch) 
            {
                if (stopRequested)
                    return LengthToFile;
                ++totalFileCnt;
                if (totalFileCnt % 1000 == 0 || relevantFileCnt % 1000 == 0)
                    statusUpdater("total files: "+ totalFileCnt + " relevant: " + relevantFileCnt);

                if (!filter(f))
                    continue;

                ++relevantFileCnt;
                var size = f.Size();
                if (LengthToFile.ContainsKey(size))
                    LengthToFile[size].Add(f);
                else 
                    LengthToFile[size] = new List<FileItem> { f };
            }

            statusUpdater("Found " + totalFileCnt + " files of which " + relevantFileCnt + " are relevant.");
            return LengthToFile;
        }

        public List<List<FileItem>> Multiples() 
        {
            var LengthToFile = SameSizeFiles();
            statusUpdater("detecting multiples");
            var ret = new List<List<FileItem>>();
            var candidateCnt = 0;
            long toSave = 0;
            foreach (var x in LengthToFile) 
            {
                if (stopRequested)
                    return ret;
                var list = x.Value;
                if (list.Count > 1) 
                {
                    ++candidateCnt;
                    if (candidateCnt % 1000 == 0)
                        statusUpdater(candidateCnt + " candiates");
                    var hashToFile = new Dictionary<string, List<FileItem>>();
                    foreach (var f in list) 
                    {
                        var h = f.FileHash();
                        if (hashToFile.ContainsKey(h))
                            hashToFile[h].Add(f);
                        else 
                            hashToFile[h] = new List<FileItem> { f };
                    }
                    var keysWithOne = hashToFile.Keys.Where(k => hashToFile[k].Count <= 1);
                    foreach (var k in keysWithOne)
                        hashToFile.Remove(k);

                    if (hashToFile.Count == 0)
                        continue;

                    var dupeList = new List<FileItem>();
                    foreach (var v in hashToFile) 
                    {
                        toSave += (v.Value.Count - 1) * v.Value[0].Size();
                        dupeList.AddRange(v.Value);
                    }

                    ret.Add(dupeList);
                }
            }

            var suffixFactor = 1.0;
            var suffixName = "Bytes";
            if (toSave > 1024 * 1024)
            {
                suffixFactor = 1024.0 * 1024.0;
                suffixName = "MBytes";
            }
            else if (toSave > 1024) 
            {
                suffixFactor = 1024.0;
                suffixName = "KBytes";
            }
            var spaceToSave = ((double) toSave) / suffixFactor;
            statusUpdater("detected " + ret.Count + " sets of multiples with: " 
                + spaceToSave + " " + suffixName + " of redundant space.");
            return ret;
        }

        public Action<string> statusUpdater = s => { };
        public Func<FileItem, bool> filter = i => { return true; };
        private readonly List<string> paths;
        public volatile bool stopRequested = false;
    }
}

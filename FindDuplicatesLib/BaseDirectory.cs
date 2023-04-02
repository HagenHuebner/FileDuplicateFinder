using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace FindDuplicates
{
    public class BaseDirectory
    {
        public static bool PathsShareDirectory(string pathA, string pathsB) 
        {
            var aParts = pathA.Split(Path.DirectorySeparatorChar);
            var bParts = pathsB.Split(Path.DirectorySeparatorChar);

            string[] longer;
            string[] shorter;
            if (aParts.Length < bParts.Length)
            {
                shorter = aParts;
                longer = bParts;
            }
            else
            {
                shorter = bParts;
                longer = aParts;
            }

            for (var i = 0; i < shorter.Length; ++i) 
            {
                if (shorter[i] != longer[i])
                    return false;
            }

            return true;
        }

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
                    yield return new FileItemImpl(files[i]);
            }
        }

        public BaseDirectory(List<string> pathsToSearch) 
        {
            paths_ = pathsToSearch;
        }

        private List<FileItem> ListFilesFromAllDirs() 
        {
            var toSearch = new List<FileItem>();

            foreach (var p in paths_)
            {
                statusUpdater("Listing directory: " + p);
                if (stopRequested)
                    return toSearch;
                var files = GetFiles(p);
                toSearch.AddRange(files.ToList());
            }

            return toSearch;
        }

        public static Dictionary<string, List<string>> GroupByDirectory(Queue<string> filePaths) 
        {
            var pathToFileList = new Dictionary<string, List<string>>();

            while (filePaths.Count > 0) 
            {
                var next = filePaths.Dequeue();
                var dir = Path.GetDirectoryName(next);
                if(!pathToFileList.ContainsKey(dir))
                    pathToFileList[dir] = new List<string> { next };
                else
                    pathToFileList[dir].Add(next);
            }

            return pathToFileList;
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
                if (totalFileCnt % 500 == 0 || relevantFileCnt % 500 == 0)
                    statusUpdater("total files: "+ totalFileCnt + " relevant: " + relevantFileCnt);

                if (!Filter(f))
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

        private bool Filter(FileItem f) 
        {
            return f.Size() >= minSize;
        }

        public List<DuplicateSet> Multiples()
        {
            var LengthToFile = SameSizeFiles();
            statusUpdater("Scanning " + LengthToFile.Count + " files.");
            var ret = new List<DuplicateSet>();
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
                    if (candidateCnt % 200 == 0)
                        statusUpdater(candidateCnt + " candiates found.");
                    var hashToFileList = new Dictionary<string, List<FileItem>>();
                    foreach (var f in list)
                    {
                        var h = f.FileHash();
                        if (hashToFileList.ContainsKey(h))
                            hashToFileList[h].Add(f);
                        else
                            hashToFileList[h] = new List<FileItem> { f };
                    }
                    var keysWithOne = hashToFileList.Keys.Where(k => hashToFileList[k].Count <= 1);
                    foreach (var k in keysWithOne)
                        hashToFileList.Remove(k);

                    if (hashToFileList.Count == 0)
                        continue;

                    foreach (var k in hashToFileList.Keys)
                    {
                        var lst = hashToFileList[k];
                        toSave += (lst.Count - 1) * lst[0].Size();
                        ret.Add(new DuplicateSet(lst));
                    }
                }
            }

            ShowSummary(ret, toSave);
            ret.Sort((a, b) =>
            {
                if (a.Items.Count == b.Items.Count)
                    return 0;
                else if (a.Items.Count > b.Items.Count)
                    return -1;
                else
                    return 1;
            });
            return ret;
        }

        private void ShowSummary(List<DuplicateSet> result, long toSave)
        {
            if (result.Count == 0)
                statusUpdater("No duplicates found.");
            else 
            {
                statusUpdater("Found " + result.Count + " sets of duplicates with: "
                    + DuplicateSet.FormatSize(toSave) + " of redundant space.");
            }
        }

        public Action<string> statusUpdater = s => { };
        public long minSize = 0;
        private readonly List<string> paths_;
        public volatile bool stopRequested = false;
    }
}

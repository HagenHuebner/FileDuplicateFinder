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
            var watcher = new ProgressWatcher(paths_.Count);
            statusUpdater(watcher.MkUpdate("Listing directories"));
            foreach (var p in paths_)
            {
                if (stopRequested)
                    return toSearch;
                var files = GetFiles(p);
                toSearch.AddRange(files.ToList());
                if (watcher.IncrementAndCheckProgress())
                    statusUpdater(watcher.MkUpdate("Listed directory: " + p));
            }
            statusUpdater(new StatusUpdate("Finished listing", 100));
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
            var watcher = new ProgressWatcher(toSearch.Count);
            statusUpdater(new StatusUpdate("Comparing file sizes.", 0));
            var LengthToFile = new Dictionary<long, List<FileItem>>();
            var totalFileCnt = 0;
            var relevantFileCnt = 0;
            foreach (var f in toSearch) 
            {
                if (stopRequested)
                    return LengthToFile;

                ++totalFileCnt;
                if (watcher.IncrementAndCheckProgress()) 
                {
                    statusUpdater(new StatusUpdate("total files: "+ totalFileCnt
                        + " relevant: " + relevantFileCnt, watcher.Percentage));
                }

                if (!Filter(f))
                    continue;

                ++relevantFileCnt;
                var size = f.Size();
                if (LengthToFile.ContainsKey(size))
                    LengthToFile[size].Add(f);
                else 
                    LengthToFile[size] = new List<FileItem> { f };
            }

            statusUpdater(new StatusUpdate("Found " + totalFileCnt + " files of which "
                + relevantFileCnt + " are relevant.", 100));
            return LengthToFile;
        }

        private bool Filter(FileItem f) 
        {
            return f.Size() >= minSize;
        }

        public List<DuplicateSet> Multiples()
        {
            var LengthToFile = SameSizeFiles();
            var progWatcher = new ProgressWatcher(LengthToFile.Count);
            statusUpdater(new StatusUpdate("Scanning " + LengthToFile.Count + " files.", 0));
            var ret = new List<DuplicateSet>();
            long toSave = 0;
            foreach (var x in LengthToFile)
            {
                if (stopRequested)
                    return ret;
                var list = x.Value;
                if (progWatcher.IncrementAndCheckProgress())
                    statusUpdater(progWatcher.MkUpdate("Checking candidates"));
                if (list.Count > 1)
                {
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
                statusUpdater(new StatusUpdate("No duplicates found.", 100));
            else 
            {
                var update = new StatusUpdate("Found " + result.Count + " sets of duplicates with: "
                    + DuplicateSet.FormatSize(toSave) + " of redundant space.", 100);
                statusUpdater(update);
            }
        }

        public Action<StatusUpdate> statusUpdater = s => { };
        public long minSize = 0;
        private readonly List<string> paths_;
        public volatile bool stopRequested = false;
    }
}

using FindDuplicates;

class Programm
{
    private static void WriteOuput(List<DuplicateSet> tuples)
    {
        using StreamWriter file = new(@"C:\dev\filehash\FindDuplicates\output.txt");
        foreach (var entry in tuples)
        {
            file.WriteLine("----  " + entry.Items.Count + " ----");
            foreach (var x in entry.Items)
                file.WriteLine(x.FullPath);
        }
    }

    public static int Main(string[] args)
    {
        var bd = new BaseDirectory(new List<string> { @"C:\dev\filehash\FindDuplicates\FindDuplicatesTest\testDir" })
        {
            statusUpdater = s => { Console.WriteLine(s); },
            minSize = 1024 * 1024
        };

        var tuples = bd.Multiples();

        tuples.Sort((a, b) =>
        {
            if (a.Items.Count == b.Items.Count)
                return 0;
            else if (a.Items.Count > b.Items.Count)
                return -1;
            else
                return 1;
        });


        WriteOuput(tuples);
        return 0;
    }
}


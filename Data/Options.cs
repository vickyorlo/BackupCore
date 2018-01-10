using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;

namespace BackupCore
{
    class Options
    {
        [Value(0, Required = false,
          HelpText = "Path to the configuration file to process. Will override any other options.")]
        public string Configuration { get; set; }

        [Option('i', "input",
         HelpText = "Input paths for the files to back up.")]
        public IList<string> Inputs { get; set; }

        [Option('o', "output",
         HelpText = "Destination paths for the files. Supply one or one for each input path.")]
        public IList<string> Outputs { get; set; }

        [Option('c', "compare", Default = "bydate",
         HelpText = "The method to compare files in the destination by. Valid options are \"bydate\" for write time date comparison and \"byhash\" for file hash comparison.")]
        public string ComparisonMethod { get; set; }

        [Option('h', "history", Default = 0, Required = false,
         HelpText = "The amount of historical copies to keep.")]
        public int History { get; set; }

        [Option('a', "archive", Default = false,
         HelpText = "Whether or not to put the resulting backup in an archive.")]
        public bool Archive { get; set; }

        [Option('p', "password", Default = "",
         HelpText = "The password to put on the archive. Ignore for no password.")]
        public string Password { get; set; }

        [Option('v', "verbose",
        HelpText = "Turn on to see the full program state output.")]
        public bool Verbose { get; set; }

        [Usage(ApplicationAlias = "Backup Core")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Reading configuration from an ini file", new Options { Configuration = "test.ini" });
                yield return new Example("Reading configuration from command line parameters", new Options
                {
                    Inputs = new List<string>() { "./Source" },
                    Outputs = new List<string>() { "./Destination" },
                    ComparisonMethod = "bydate",
                    History = 1,
                    Archive = true,
                    Password = "test"
                });
                yield return new Example("Multiple sources, one destination directory", new Options
                {
                    Inputs = new List<string>() { "./Source", "./Source2", "./Source3" },
                    Outputs = new List<string>() { "./Destination" },
                });
                yield return new Example("Multiple sourcesa, multiple destinations", new Options
                {
                    Inputs = new List<string>() { "./Source", "./Source2" },
                    Outputs = new List<string>() { "./Destination", "./Destionation2" },
                });
            }
        }
    }
}
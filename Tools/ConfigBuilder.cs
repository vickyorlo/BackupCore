
using System;
using System.Collections.Generic;
using System.Collections;
using BackupCore;
using IniParser;
using IniParser.Model;
using System.IO;

namespace BackupCore
{
    class ConfigBuilder
    {
        public List<BackupAction> LoadConfig(Options options)
        {
            Program.Verbose = options.Verbose;
            if (options.Configuration != null)
            {
                return ReadConfigurationFromFile(options);
            }
            else
            {
                return ReadConfigurationFromCommandLine(options);
            }
        }

        private List<BackupAction> ReadConfigurationFromCommandLine(Options options)
        {
            List<BackupAction> backupActionList = new List<BackupAction>();
            CompareMethod comparator;
            switch (options.ComparisonMethod)
            {
                case "bydate": comparator = CompareMethod.WriteTimeComparator; break;
                case "byhash": comparator = CompareMethod.HashComparator; break;
                default: throw new ArgumentException("Invalid value in 'compare' setting!");
            }

            if (options.Outputs.Count == 1)
            {
                for (int i = 0; i < options.Inputs.Count; i++)
                {
                    backupActionList.Add(new BackupAction("backup", options.Inputs[i], options.Outputs[0] + "/" + Path.GetFileName(options.Inputs[i]) + "/", new FileBackup(), comparator, (int)options.History, options.Archive, options.Password));
                }
            }
            else if (options.Inputs.Count == options.Outputs.Count)
            {
                for (int i = 0; i < options.Inputs.Count; i++)
                {
                    backupActionList.Add(new BackupAction("backup", options.Inputs[i], options.Outputs[i], new FileBackup(), comparator, (int)options.History, options.Archive, options.Password));
                }
            }
            else
            {
                throw new ArgumentException("The number of destinations must be equal to 1 or the number of sources!");
            }

            return backupActionList;
        }

        private List<BackupAction> ReadConfigurationFromFile(Options options)
        {
            IniData data;
            try
            {
                var parser = new FileIniDataParser();
                data = parser.ReadFile(options.Configuration);
            }
            catch (Exception e)
            {
                Console.Error.Write(e);
                throw new ArgumentException("Configuration file does not exist or invalid format! /n Check the example ");
            }
            var flags = data["Flags"];
            var files = data["Files"];
            string actionName = flags["profile"];
            IBackup mode;
            switch (flags["mode"])
            {
                case "database": mode = new DatabaseBackup(); break;
                case "simple": mode = new FileBackup(); break;
                default: throw new ArgumentException("Invalid value in 'mode' setting!");
            }
            CompareMethod comparator;
            switch (flags["compare"])
            {
                case "bydate": comparator = CompareMethod.WriteTimeComparator; break;
                case "byhash": comparator = CompareMethod.HashComparator; break;
                default: throw new ArgumentException("Invalid value in 'compare' setting!");
            }

            int copies = Convert.ToInt32(flags["history"]);
            bool archive = (flags["archive"] == "yes" || flags["archive"] == "true") ? true : false;
            string password = flags["password"];

            string[] sources = files["sources"].Replace("\"", "").Split(",");
            string[] destination = files["destinations"].Replace("\"", "").Split(",");


            List<BackupAction> backupActionList = new List<BackupAction>();
            if (destination.Length == 1)
            {
                foreach (var source in sources)
                {
                    backupActionList.Add(new BackupAction(actionName, source, destination[0], mode, comparator, copies, archive, password));
                }
            }
            else if (sources.Length == destination.Length)
            {
                for (int i = 0; i < sources.Length; i++)
                {
                    backupActionList.Add(new BackupAction(actionName, sources[i], destination[i], mode, comparator, copies, archive, password));
                }
            }
            else
            {
                throw new ArgumentException("The number of destinations must be equal to 1 or the number of sources!");
            }
            return backupActionList;
        }

    }
}

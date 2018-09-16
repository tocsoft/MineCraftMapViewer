using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RebuildMinecraftMap
{
    class Program
    {
        static void Main(string[] args)
        {
            // hosting location 
            var rootOutputFolder = Path.Combine(Path.GetDirectoryName(new Uri(typeof(Program).Assembly.Location).LocalPath), "RenderedMaps");

            var worldfolder = Environment.ExpandEnvironmentVariables(@"%localappdata%\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\\minecraftWorlds");
            // lets extract the list of worlds from the users maps 


            var folders = Directory.EnumerateDirectories(worldfolder).ToList();
            Dictionary<string, string> names = new Dictionary<string, string>();
            Console.WriteLine("Which world would you like to generate map for?");
            for (var i = 0; i < folders.Count; i++)
            {
                var name = File.ReadAllText(Path.Combine(folders[i], "levelname.txt"));
                // TODO parse marker to change colors
                Console.WriteLine($@"{i + 1}. {name}");
            }
            Console.WriteLine($@"a. All maps");
            Console.WriteLine($@"c. Cancel");

            var foldersToRender = ListMaps(folders);
            if (folders.Any())
            {
                var toolPath = GetVizPath();
                foldersToRender.AsParallel()
                    .ForAll(folder =>
                    {

                        var clonedFolder = CreateTempCopy(folder);
                        try
                        {
                            var name = Path.GetFileName(folder);
                            var outputFolder = Path.Combine(rootOutputFolder, name);
                            Directory.CreateDirectory(outputFolder);
                            var arg = $@"--flush --auto-tile --db ""{clonedFolder}"" --out ""{Path.Combine(outputFolder, "index")}"" --html-all";
                            var info = new ProcessStartInfo(toolPath, arg);
                            info.UseShellExecute = false;
                            var process = Process.Start(info);
                            process.WaitForExit();

                            var friendlyname = File.ReadAllText(Path.Combine(clonedFolder, "levelname.txt"));
                            File.WriteAllText(Path.Combine(outputFolder, "levelname.txt"), $"{Environment.MachineName} - {friendlyname}");
                        }
                        finally
                        {
                            DeleteFolder(clonedFolder);
                        }
                    });
            }
        }

        private static string CreateTempCopy(string SourcePath)
        {
            var destinationPath = Path.Combine(Path.GetTempPath(), "RebuildMinecraftMap", "TempCache", Guid.NewGuid().ToString());

            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(SourcePath, destinationPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(SourcePath, "*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(SourcePath, destinationPath), true);
            }

            return destinationPath;
        }

        private static void DeleteFolder(string SourcePath)
        {
            foreach (string newPath in Directory.GetFiles(SourcePath, "*.*",
                SearchOption.AllDirectories))
                File.Delete(newPath);

            Directory.Delete(SourcePath, true);
        }

        private static string GetVizPath()
        {
            var assembly = typeof(Program).Assembly;
            var name = assembly.GetManifestResourceNames().Where(x => x.EndsWith("mcpe_viz.win64.zip")).Single();

            using (var stream = assembly.GetManifestResourceStream(name))
            {
                //var hash = "mcpe_viz.win64";
                var hash = CalculateMd5(stream);
                stream.Position = 0;

                var temp = Path.GetTempPath();
                var folder = Path.Combine(temp, "RebuildMinecraftMap", hash);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                    using (var zip = Ionic.Zip.ZipFile.Read(stream))
                    {
                        zip.ExtractAll(folder);
                    }
                }

                return Path.Combine(folder, "mcpe_viz.exe");
            }
        }

        private static string CalculateMd5(Stream stream)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(stream);
                var base64String = Convert.ToBase64String(hash);
                return base64String;
            }
        }

        private static IEnumerable<string> ListMaps(List<string> folders)
        {

            while (true)
            {
                Console.Write("Choose and option > ");
                var option = Console.ReadLine();
                if (option.Equals("C", StringComparison.OrdinalIgnoreCase))
                {
                    return Enumerable.Empty<string>();
                }
                else if (option.Equals("a", StringComparison.OrdinalIgnoreCase))
                {
                    return folders;
                }
                else
                {
                    if (int.TryParse(option, out var index))
                    {
                        if (index >= 1 && index <= folders.Count)
                        {
                            return new[] {
                                folders[index-1]
                            };
                        }
                    }
                }
            }
        }
    }

    public class Worlds
    {

    }
}

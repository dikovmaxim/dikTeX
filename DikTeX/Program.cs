using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DikTeX
{
    class Program
    {
        public static string Content = "";



        public static List<string> Variables, Values;

        static void Main(string[] args)
        {
                 Values = new List<string>();
                 Variables = new List<string>();
            Console.WriteLine("Translating started...");
            try
            {
                Console.WriteLine("Reding source file...");
                Content = File.ReadAllText(args[0]);
            }
            catch
            {
                Console.WriteLine("Error reading source file");
                Environment.Exit(-1);
            }

            string[] lines = Content.Split('\n');
            foreach (string line in lines)
            {
                foreach (string var in Variables)
                {
                    try
                    {

                    Content = Content.Replace(line,line.Replace("{"+var+"}",Values[Variables.IndexOf(var)]));

                    }
                    catch { }

                }

                string[] command = line.Split(' ');
                switch (command[0])
                {
                    case "#IMPORT":
                        string downloadedString = "";
                        try
                        {
                            WebClient client = new WebClient();
                            downloadedString = client.DownloadString(command[1]);
                        }
                        catch {
                            downloadedString = "Error importing data...";
                        }
                        Content = Content.Replace(line,downloadedString);
                        break; 

                    case "#INCLUDE":
                        string fileData = "";


                            fileData = File.ReadAllText(new string(command[1].Where(c => !char.IsControl(c)).ToArray()));


                        Content = Content.Replace(line, fileData);
                        break;

                    case "#MACROS":
                        try
                        {
                            if (command[2] == "AS")
                            {
                                if (!Values.Contains(command[3])&&!Variables.Contains(command[1]))
                                {

                                     Variables.Add(command[1]);
                                     Values.Add(command[3]);
                                    Content = Content.Replace(line, "");

                                }
                            }
                            else if(command[2] == "UPDATE")
                            {
                                if (Variables.Contains(command[1]))
                                {

                                      Values[Variables.IndexOf(command[1])] = command[3];
                                    Content = Content.Replace(line, "");

                                }
                            }
                            
                            else
                            {
                                Content = Content.Replace(line, "Invalid macros operation");
                            }
                        }
                        catch {
                            Content = Content.Replace(line, "Syntax error occured. Check your code");
                            
                        }
                        break;

                    default:
                        break;
                }
            }
            if (File.Exists("output.tex"))
            {
                try{
                    File.Delete("output.tex");
                }
                catch { Console.WriteLine("Error deleting old file"); Environment.Exit(-1); }
            }
            try
            {
                Console.WriteLine("Writing output to file...");
                File.WriteAllText("output.tex",Content);

            }
            catch
            {
                Console.WriteLine("Error writing data to file");
                Environment.Exit(-1);
            }
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = @"C:\Program Files\MiKTeX\miktex\bin\x64\pdflatex.exe";
                p.StartInfo.Arguments = "--quiet output.tex";
                p.Start();
            }
            catch
            {
                Environment.Exit(-1);
            }
        }
    }
}

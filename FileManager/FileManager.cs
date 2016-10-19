////////////////////////////////////////////////////////////////////////////////
//  FileManager.cs - Provides functionality for directory and file management //
//  ver 0.5                                                                   //
//  Language:     C#, VS 2015, .NET Framework 4.5.2                           //
//  Platform:     Windows 10                                                  //
//  Application:  Test Harness, CSE681 - Project 2                            //
//  Author:       Burak Kakillioglu, Syracuse University                      //
//                bkakilli@syr.edu                                            //
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{

    // File Manager always gets paths for the target DLLs relative to the repository folder.
    // It works in the path that application runs.
    public class FileManager<TypeTestID> : IRepository, ILog
    {
        public Logger logger;
        static string TAG = "FileManager";

        string appLocation;
        string libDirectory;
        string repository;

        // Constructor
        public FileManager(string appLocation_, string libDirectory_, Logger logger_)
        {
            appLocation = appLocation_;
            libDirectory = libDirectory_;

            logger = logger_;
        }

        public bool copyLibraries(TypeTestID testID, List<string> fileList)
        {

            int numOfFilesNotFound = 0;
            Log(TAG, string.Format("Copying files from repository...\n"));
            foreach (string file in fileList)
            {
                string fileName = Path.GetFileName(file);

                try
                {
                    // file to be found and copied, the destination folder, repository root
                    bool fileFound = findAndCopyFromRepo(fileName, libDirectory, repository);
                    if (!fileFound)
                        numOfFilesNotFound++;
                }
                catch (Exception ex)
                {
                    Log(TAG, string.Format("\n", ex.Message));
                }
            }

            if (numOfFilesNotFound > 0)
            {
                Log(TAG, string.Format("\n{0} libraries are not found and not loaded from repository.\n", numOfFilesNotFound));
                return false;
            }
            else
            {
                Log(TAG, string.Format("DLLs are loaded from repository.\n"));
                return true;
            }
        }

        static public bool removeFolder(string path)
        {
            try
            {
                if(System.IO.Directory.Exists(path))
                    System.IO.Directory.Delete(Path.GetFullPath(path), true);
            }
            catch (Exception)
            {
                //
                return false;
            }
            return true;
        }

        public bool connectToRepo(string repository_)
        {
            // Application specific implementation.
            // repository_ is the relative path to the repo folder from application path.
            repository = absPath(repository_);
            return true;
        }

        public bool findAndCopyFromRepo(string targetFile, string libDirectory, string repository, bool overwrite = true)
        {
            // Application specific implementation.
            try
            {
                string destFile = Path.Combine(libDirectory, targetFile);
                string[] filesFound = Directory.GetFiles(
                    repository, targetFile, SearchOption.AllDirectories);

                if (filesFound.Length < 1)
                {
                    Log(TAG, string.Format(
                        "Error: There is no file found with search pattern \"{0}\" in repository.",
                        targetFile));
                    return false;
                }
                else
                {
                    if (filesFound.Length > 1)
                    {
                        Log(TAG, string.Format(
                            "Warning: Multiple files are found with search pattern \"{0}\" in repository.\n",
                            targetFile));
                    }

                    foreach(string sourceFile in filesFound)
                    {
                        System.IO.File.Copy(sourceFile, destFile, overwrite);
                        Log(TAG, string.Format("File copied: {0}", sourceFile));
                    }

                    Log(TAG, string.Format("\n"));
                }
                return true;
            }
            catch (Exception ex)
            {
                Log(TAG, string.Format("\n", ex.Message));
                return false;
            }
        }

        public string absPath(string relPath)
        {
            return Path.GetFullPath(Path.Combine(appLocation, relPath));
        }

        public static void writeToFile(string filePath, string text)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllText(filePath, text);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not write to the file:\n\t{0}.\n\tDetails: {1}\n", filePath, ex.Message);
            }
            
        }

        public static string readFile(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                return string.Format("File could not read:\n{0}\n", ex.Message);
            }
        }

        public void Log(string tag, string log)
        {
            logger.Log(tag, log);
        }

        public string getLog()
        {
            return logger.getLog();
        }

#if (FileManager_TEST)
        public static void Main(string[] args)
        {
            Console.Write("\n  Testing FileManager Project");
            Console.Write("\n ============================\n");

            string repository = args[0];
            string appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string libDirectory = "testLibDirectory";
            Logger logger = new Logger();
            logger.verbose = true;

            if(!Directory.Exists(Path.Combine(appLocation, libDirectory)))
                Directory.CreateDirectory(
                    Path.Combine(appLocation, libDirectory));

            FileManager<string> fm = new FileManager<string>(appLocation, libDirectory, logger);

            fm.connectToRepo(repository);
            List<string> fileList = new List<string>();
            fileList.Add("sampleFile1_does_not_exist.txt");
            fileList.Add("sampleFile2_does_not_exist.txt");
            fileList.Add("Driver.dll_exists.txt");

            string testID = "sampleTestID";

            Console.WriteLine("Existing files in fileList is copying to the {0} folder", libDirectory);
            fm.copyLibraries(testID, fileList);

            string testFile = Path.Combine(libDirectory, "testFile.txt");
            string sampleText = "Construction test\nfor the FileManager package.";
            FileManager<string>.writeToFile(testFile, sampleText);

            Console.WriteLine("Reading from file testFile.txt which has just been written:\n{0}",
                FileManager<string>.readFile(testFile));

            FileManager<string>.removeFolder(Path.Combine(appLocation, libDirectory));

        }
#endif
    }
}

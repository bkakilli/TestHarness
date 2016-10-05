using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{

    public class TempFolder<TypeTestID>
    {
        public TypeTestID uniqueId = default(TypeTestID);   // Unique id of test
        public string relativePath = "";
        public string fullPath = "";
        public List<string> fileList = null;

        public TempFolder (TypeTestID testID, string relativePath_, string fullPath_, List<string> fileList_ = null)
        {
            uniqueId = testID;
            relativePath = relativePath_;
            fullPath = fullPath_;
            fileList = fileList_ == null ? new List<string>() : fileList_;
        }

        public void show()
        {

        }
    }

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

        /*public bool removeTempFolder(TypeTestID testID)
        {
            // Delete a directory and all subdirectories with Directory static method...
            string path = folders[testID].fullPath;
            if (System.IO.Directory.Exists(path))
            {
                try
                {
                    folders.Remove(testID);
                    System.IO.Directory.Delete(path, true);
                }

                catch (System.IO.IOException e)
                {
                    Log(TAG, string.Format("{0}\n", e.Message));
                }
            }
            return true;
        }*/

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
    }
}

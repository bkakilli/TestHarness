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

        string appLocation;
        string testFolder;
        string repository;
        Dictionary<TypeTestID, TempFolder<TypeTestID>> folders;

        // Constructor
        public FileManager(string appLocation_, string testFolder_)
        {
            appLocation = appLocation_;
            testFolder = testFolder_;
            folders = new Dictionary<TypeTestID, TempFolder<TypeTestID>>();

            logger = new Logger("FileManager");
        }

        public bool createTempFolder(TypeTestID testID, List<string> fileList)
        {
            string relativePath = Path.Combine(testFolder, testID.ToString());
            string fullPath = absPath(relativePath);
            Directory.CreateDirectory(fullPath);

            TempFolder<TypeTestID> tf = new TempFolder<TypeTestID>(testID, relativePath, fullPath, fileList);

            int numOfFilesNotFound = 0;
            Console.WriteLine("A temporary test directory is created with name {0}", testID);
            Console.WriteLine("Loading files from repository...\n");
            foreach (string file in fileList)
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(tf.fullPath, fileName);

                string sourceFile = Path.Combine(repository, file);

                try
                {
                    //System.IO.File.Copy(sourceFile, destFile, true);

                    // file to be found and copied, the destination folder, repository root
                    bool fileFound = findAndCopyFromRepo(fileName, tf.fullPath, repository);
                    if (!fileFound)
                        numOfFilesNotFound++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            if (numOfFilesNotFound > 0)
            {
                Console.WriteLine("\n{0} libraries are not found and not loaded from repository.", numOfFilesNotFound);
                return false;
            }
            else
            {
                // Console.WriteLine("DLLs are loaded from repository.\n");
                folders.Add(testID, tf);
                return true;
            }
        }

        public string getPath(TypeTestID testID)
        {
            return folders[testID].fullPath;
        }

        public bool removeTempFolder(TypeTestID testID)
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
                    Console.WriteLine(e.Message);
                }
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

        public bool findAndCopyFromRepo(string targetFile, string destDirPath, string repository, bool overwrite = true)
        {
            // Application specific implementation.
            try
            {
                string destFile = Path.Combine(destDirPath, targetFile);
                string[] filesFound = Directory.GetFiles(
                    repository, targetFile, SearchOption.AllDirectories);
                
                if(filesFound.Length < 1)
                {
                    Console.WriteLine(
                        "Error: There is no file found with search pattern \"{0}\" in repository.",
                        targetFile);
                    return false;
                }
                else
                {
                    if (filesFound.Length > 1)
                    {
                        Console.WriteLine(
                            "Warning: Multiple files are found with search pattern \"{0}\" in repository. Each of them copied into the temporary directory and overwrited if exists.",
                            targetFile);
                        // Console.WriteLine("Each of following files are copied into temporary direcroy:");
                    }
                    foreach(string sourceFile in filesFound)
                    {
                        System.IO.File.Copy(sourceFile, destFile, overwrite);
                        // Console.WriteLine("\t" + sourceFile);
                    }

                }
                Console.WriteLine();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public string absPath(string relPath)
        {
            return Path.GetFullPath(Path.Combine(appLocation, relPath));
        }

        public void writeToFile(string filePath, string text)
        {
            try
            {
                File.WriteAllText(filePath, text);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not write to the file:\n\t{0}.\n\tDetails: {1}", filePath, ex.Message);
            }
            
        }

        public void Log(string log)
        {
            logger.Log(log);
        }

        public string getLog()
        {
            return logger.getLog();
        }
    }
}

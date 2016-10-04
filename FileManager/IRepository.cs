using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{
    interface IRepository
    {
        bool connectToRepo(string repo);
        bool findAndCopyFromRepo(string targetFile, string destDirPath, string repository, bool overwrite = true);
    }
}

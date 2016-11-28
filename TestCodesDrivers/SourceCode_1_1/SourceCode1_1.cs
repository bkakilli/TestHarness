/////////////////////////////////////////////////////////////////////////////
//  SourceCode.cs - Sample source code to test application                 //
//  ver 0.5                                                                //
//  Language:     C#, VS 2015, .NET Framework 4.5.2                        //
//  Platform:     Windows 10                                               //
//  Application:  Test Harness, CSE681 - Project 2                         //
//  Author:       Burak Kakillioglu, Syracuse University                   //
//                bkakilli@syr.edu                                         //
/////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{
    public class SourceCode1_1
    {
        public bool getFromSource2()
        {
            return SourceCode_1_2.someFunction();
        }

        public static void Main(string[] args)
        {

        }
    }
}

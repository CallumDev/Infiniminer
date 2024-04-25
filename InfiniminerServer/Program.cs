using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Infiniminer.Server
{
    class Program
    {
        static void RunServer()
        {
            bool restartServer = true;
            while (restartServer)
            {
                InfiniminerServer infiniminerServer = new InfiniminerServer();
                restartServer = infiniminerServer.Start();
            }
        }

        static void Main(string[] args)
        {
            #if DEBUG
                RunServer();
            #else
                try
                {
                    RunServer();
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message + "\r\n\r\n" + e.StackTrace);
                }
            #endif
        }
    }
}

using System;
using LibreLancer;

namespace Infiniminer;

class MainClass
{
    [STAThread]
    public static void Main(string[] args)
    {
        AppHandler.ProjectName = "Infiniminer";
        AppHandler.Run(() => new InfiniminerGame(args).Run());
    }
}
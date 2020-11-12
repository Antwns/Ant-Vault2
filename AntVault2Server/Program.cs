using AntVault2Server.ServerWorkers;
using System;

namespace AntVault2Server
{
    class Program
    {
        static void Main(string[] args)
        {
            MainServerWorker.StartAntVaultServer();
            Console.ReadLine();
        }
    }
}

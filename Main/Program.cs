using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Quotes;
using Core.Markets;
using DataLibrary;
using Info.Blockchain.API.Models;

namespace Main
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Library\\";
            string btcadd = "3CtPt6iSZvGBAUkmTmUeuAutbTPK4bu54b";

            BlockchainProvider data = new BlockchainProvider(path, useInternet: true);
            Address x = data.GetBlockchainData(btcadd);
            Console.WriteLine("End");
        }
    }
}

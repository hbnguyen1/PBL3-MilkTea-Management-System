using PBL3.Core;
using PBL3.Data;
using PBL3.GUI;
using PBL3.Manangers;
using PBL3.Models;
using System;
using System.Windows;

namespace PBL3
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            System.Windows.Application app = new System.Windows.Application();
            app.Run(new wDangNhap());
        }
    }
}
//develop branch
using System;
using System.Windows;
using PBL3.GUI;
using PBL3.Manangers;
using PBL3.Models;
using PBL3.Core;

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
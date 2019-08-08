using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace Fanytel
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string usersPath = @"D:\Dropbox\Text Files\Whatsdialer.txt";
        public static string transfersPath = @"D:\Dropbox\Text Files\WhatsdialerTransfers.txt";

        public static FanytelUser Reseller = new FanytelUser();
        public static ObservableCollection<FanytelUser> Users = new ObservableCollection<FanytelUser>();
        public static List<Transfer> Transfers = new List<Transfer>();
        //public static List<string> BlackListNumbers = new List<string>();

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!File.Exists(usersPath))
            {
                usersPath = @"\\محل\Text Files\Whatsdialer.txt";
                transfersPath = @"\\محل\Text Files\WhatsdialerTransfers.txt";
            }
            var currentProcess = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(currentProcess.ProcessName);
            var process = processes.FirstOrDefault(p => p.Id != currentProcess.Id);
            if (process != null)
            {
                SetForegroundWindow(process.MainWindowHandle);
                this.Shutdown();
            }

            string pathRes = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\whatsRes";
            string[] dataRes = File.ReadAllLines(pathRes);
            Reseller.PhoneNumber = dataRes[0];
            Reseller.Password = dataRes[1];

            GetUsers();
            GetTransfers();
        }
        static App()
        {
        }

        public static void SaveUsers()
        {
            List<string> uu = new List<string>();
            foreach (FanytelUser fu in Users)
            {
                string temp = fu.PhoneNumber + "," + fu.Password + "," + fu.Balance.ToString("0.00") + "," + fu.ResellerRate;

                uu.Add(temp);
            }

            File.WriteAllLines(App.usersPath, uu.ToArray());
        }
        public static void GetUsers()
        {
            if (File.Exists(usersPath))
            {
                Users.Clear();
                string[] usersData = File.ReadAllLines(usersPath);
                foreach (string s in usersData)
                {
                    string[] data = s.Split(new char[] { ',' });
                    FanytelUser u = new FanytelUser();
                    u.PhoneNumber = data[0];
                    u.Password = data[1];
                    u.Balance = double.Parse(data[2]);
                    u.ResellerRate = int.Parse(data[3]);

                    Users.Add(u);
                }
            }
            else
                File.Create(usersPath);
        }

        public static void GetTransfers()
        {
            if (File.Exists(App.transfersPath))
            {
                Transfers.Clear();
                foreach (string s in File.ReadAllLines(App.transfersPath))
                {
                    App.Transfers.Add(new Transfer(s));
                }
            }
        }

        public static void SaveTransfers()
        {
            File.WriteAllLines(App.transfersPath, App.Transfers.ConvertAll<string>(t => t.ToString()));
        }


    }
}

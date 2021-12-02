using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Auth.Utils;
using nanoAPIAdminPanel.Main;

namespace nanoAPIAdminPanel.Auth
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        //readonly httpclient
        public static readonly HttpClient nanoHttpclient = new HttpClient(new HttpClientHandler { UseCookies = false });
        public LoginWindow()
        {
            InitializeComponent();
            NanoLog("Login Window Initialized", ConsoleColor.White);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NanoLog("Login Window Loaded", ConsoleColor.White);
            UtilsManager.CheckServerHealth();
            if (UtilsManager.IsLoggedInAndVerified())
            {
                MainWindow main = new MainWindow();
                main.Show();
            }

        }

        public static void NanoLog(string msg, ConsoleColor c)
        {
            Console.ForegroundColor = c;
            if (c.Equals(ConsoleColor.Red))
            {
                //possible err
                Console.WriteLine("[POSSIBLE ERROR]");
                Console.WriteLine("[POSSIBLE ERROR - nanoAPI] - " + msg);
                Console.WriteLine("[POSSIBLE ERROR]");
            }
            else if (c.Equals(ConsoleColor.Yellow))
            {
                //action
                Console.WriteLine("[ACTION - nanoAPI] - " + msg);
            }
            else if (c.Equals(ConsoleColor.Green))
            {
                //worked / all good
                Console.WriteLine("[OK - nanoAPI] - " + msg);
            }
            else
            {
                Console.WriteLine("[nanoAPI] - " + msg);
            }
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            NanoLog("Login Button was Pressed", ConsoleColor.White);
            NanoLog("Comparing User Data with Database", ConsoleColor.Yellow);
            UtilsManager.Login(UserOrEmailInput.Text, PasswordInput.Password);
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            ServerResponseTxt.Text = UtilsManager.Health;
        }
    }
}

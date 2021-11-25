using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
            NanoLog("Creating Registry Keys", ConsoleColor.Yellow);
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\nanoAPIADMIN");


            //storing the values  
            NanoLog("Checking if Registry is null", ConsoleColor.White);
            if (string.IsNullOrEmpty((string?)key.GetValue("UserOrEmail")) || string.IsNullOrEmpty((string?)key.GetValue("Password")) || string.IsNullOrEmpty((string?)key.GetValue("Auth-Key")))
            {
                NanoLog("Registry is null.", ConsoleColor.Red);
                NanoLog("Setting UserOrEmail Value to: " + UserOrEmailInput.Text, ConsoleColor.Yellow);
                key.SetValue("UserOrEmail", UserOrEmailInput.Text);
                NanoLog("Setting Password Value to: " + PasswordInput.Password, ConsoleColor.Yellow);
                key.SetValue("Password", PasswordInput.Password);
                NanoLog("Setting Auth-Key Value to: null", ConsoleColor.Yellow);
                key.SetValue("Auth-Key", "null");
                NanoLog("Closing Registry", ConsoleColor.Yellow);
                key.Close();
            }
            NanoLog("Getting Self GetRequest", ConsoleColor.White);
            GetUserLoggedInAsync("https://api.nanosdk.net/user/self");
        }

        private void NanoLog(string msg, ConsoleColor c)
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

        private async void GetUserLoggedInAsync(string url)
        {
            NanoLog("Init Self GetRequest", ConsoleColor.White);
            NanoLog("Opening Registry keys", ConsoleColor.Yellow);
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\nanoAPIADMIN");
            NanoLog("Checking if keys are not null", ConsoleColor.White);
            if (key != null)
            {
                NanoLog("keys are not null.", ConsoleColor.Green);
                NanoLog("Setting UserOrEmail Key to Userinput", ConsoleColor.Yellow);
                UserOrEmailInput.Text = (string?)key.GetValue("UserOrEmail");
                NanoLog("Setting Password Key to Userinput", ConsoleColor.Yellow);
                PasswordInput.Password = (string?)key.GetValue("Password");
            }
            NanoLog("HttpClient Gets Self URL", ConsoleColor.White);
            var response = await nanoHttpclient.GetAsync(url);

            NanoLog("Getting Auth-Key data From Registry", ConsoleColor.White);
                var AuthKey = (string?)key.GetValue("Auth-Key");
            NanoLog("Setting Auth-Key as Cookie", ConsoleColor.Yellow);
                nanoHttpclient.DefaultRequestHeaders.Add("Auth-Key", HttpUtility.HtmlEncode(AuthKey));

            NanoLog("Getting Self URL Content", ConsoleColor.White);
            string result = await response.Content.ReadAsStringAsync();
            NanoLog("Getting Properties", ConsoleColor.White);
            var properties = JsonConvert.DeserializeObject<NanoUserData>(result);
            NanoLog("Closing Registry", ConsoleColor.Yellow);
            key.Close();
            NanoLog("Checking if Username is null", ConsoleColor.White);
            if (string.IsNullOrEmpty(properties.Username))
            {
                NanoLog("Username is null.", ConsoleColor.Red);
                NanoLog("Checking ServerHealth", ConsoleColor.White);
                GetHealthCheckAsync("https://api.nanosdk.net/health");
                NanoLog("Getting Registry Data", ConsoleColor.Yellow);
                RequestRegistryData();
            }
            else
            {
                NanoLog("Useranme ISNT null", ConsoleColor.Green);
                NanoLog("Checking if Permissions are Valid", ConsoleColor.White);
                if (properties.Permission == 10)
                {
                    NanoLog("Valid Permission.", ConsoleColor.Green);
                    NanoLog("Opening New Window", ConsoleColor.Yellow);
                    Main.MainWindow window = new Main.MainWindow();
                    window.InitializeComponent();
                    window.Show();
                    NanoLog("Closing Current Window", ConsoleColor.Yellow);
                    Close();
                }
                else
                {
                    NanoLog("inValid Permission.", ConsoleColor.Red);
                    NanoLog("Getting meme Error", ConsoleColor.White);
                    MessageBox.Show("YOUR NOT AN ADMIN GO AWAY");
                    NanoLog("Checking ServerHealth", ConsoleColor.White);
                    GetHealthCheckAsync("https://api.nanosdk.net/health");
                    NanoLog("Getting Registry Data", ConsoleColor.Yellow);
                    RequestRegistryData();
                }
            }
        }

        private void RequestRegistryData()
        {
            NanoLog("opening Registry", ConsoleColor.Yellow);
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\nanoAPIADMIN");
            NanoLog("Checking if keys are not null", ConsoleColor.White);
            if (key != null)
            {
                NanoLog("Keys are not null.", ConsoleColor.Green);
                NanoLog("Getting UserOrEmail Key", ConsoleColor.White);
                UserOrEmailInput.Text = (string?)key.GetValue("UserOrEmail");
                NanoLog("Getting Password Key", ConsoleColor.White);
                PasswordInput.Password = (string?)key.GetValue("Password");
                NanoLog("Closing Registry", ConsoleColor.Yellow);
                key.Close();
            }
        }

        private async void GetHealthCheckAsync(string url)
        {
            NanoLog("Getting Health URL", ConsoleColor.White);
            var response = await nanoHttpclient.GetAsync(url);
            NanoLog("Getting Health URL Content", ConsoleColor.White);
            string result = await response.Content.ReadAsStringAsync();
            NanoLog("Getting Properties", ConsoleColor.White);
            var properties = JsonConvert.DeserializeObject<ServerHealth>(result);
            NanoLog("Checking if Server status is not null", ConsoleColor.White);
            if (!string.IsNullOrEmpty(properties.Status))
            {
                NanoLog("Server status is not null.", ConsoleColor.Green);
                NanoLog("ServerResponse Text set to Current Server Status", ConsoleColor.Yellow);
                ServerResponseTxt.Text = properties.Status;
            }
            else
            {
                NanoLog("Server Status is Null", ConsoleColor.Red);
                NanoLog("ServerResponse Text set to null", ConsoleColor.Yellow);
                ServerResponseTxt.Text = "null";
            }
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            NanoLog("Login Button was Pressed", ConsoleColor.White);
            NanoLog("Comparing User Data with Database", ConsoleColor.Yellow);
            CheckValidLoginDataAsync(UserOrEmailInput.Text, PasswordInput.Password);
        }

        private async void CheckValidLoginDataAsync(string usernameOrEmail, string password)
        {
            NanoLog("Getting Content", ConsoleColor.White);
            var content = new StringContent(JsonConvert.SerializeObject(new LoginData
            {
                Username = usernameOrEmail,
                Password = password
            }));
            string nanoLoginURL = "https://api.nanosdk.net/user/login";
            NanoLog("Doing Post Request", ConsoleColor.Yellow);
            var response = await nanoHttpclient.PostAsync(nanoLoginURL, content);
            NanoLog("Reading content", ConsoleColor.White);
            string result = await response.Content.ReadAsStringAsync();
            NanoLog("Getting Properties", ConsoleColor.White);
            var loginProperties = JsonConvert.DeserializeObject<LoginBase<LoginResponse>>(result);
            NanoLog("Checking if User is (login)Valid", ConsoleColor.Yellow);
            if (loginProperties.message.Contains("Successfully executed login. Method used: Cookie. Cookie was returned"))
            {
                NanoLog("User is LoggedIn!", ConsoleColor.Green);
                NanoLog("Creating Registry Keys", ConsoleColor.Yellow);
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\nanoAPIADMIN");
                NanoLog("Setting UserOrEmail Key to " + UserOrEmailInput.Text, ConsoleColor.Yellow);
                key.SetValue("UserOrEmail", UserOrEmailInput.Text);
                NanoLog("Setting Password Key to **HIDDEN**", ConsoleColor.Yellow);
                key.SetValue("Password", PasswordInput.Password);
                NanoLog("Setting Auth-Key key to " + loginProperties.Data.AuthKey, ConsoleColor.Yellow);
                key.SetValue("Auth-Key", loginProperties.Data.AuthKey);
                NanoLog("Closing Registry", ConsoleColor.Yellow);
                key.Close();
                NanoLog("Opening New Window", ConsoleColor.Yellow);
                Main.MainWindow window = new Main.MainWindow();
                window.InitializeComponent();
                window.Show();
                NanoLog("Closing Current Window", ConsoleColor.Yellow);
                Close();

            }
            else
            {
                NanoLog("Error While trying to Login", ConsoleColor.Red);
                MessageBox.Show(loginProperties.message, "Response");
            }
        }

    }
}

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
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\nanoAPIADMIN");

            //storing the values  
            if (string.IsNullOrEmpty((string?)key.GetValue("UserOrEmail")) || string.IsNullOrEmpty((string?)key.GetValue("Password")) || string.IsNullOrEmpty((string?)key.GetValue("Auth-Key")))
            {
                key.SetValue("UserOrEmail", UserOrEmailInput.Text);
                key.SetValue("Password", PasswordInput.Password);
                key.SetValue("Auth-Key", "null");
                key.Close();
            }

            GetUserLoggedInAsync("https://api.nanosdk.net/user/self");
        }

        private async void GetUserLoggedInAsync(string url)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\nanoAPIADMIN");
            if (key != null)
            {
                UserOrEmailInput.Text = (string?)key.GetValue("UserOrEmail");
                PasswordInput.Password = (string?)key.GetValue("Password");
            }

            var response = await nanoHttpclient.GetAsync(url); //Request headers must contain only ASCII characters. Err
            try
            {
                var AuthKey = (string?)key.GetValue("Auth-Key");
                nanoHttpclient.DefaultRequestHeaders.Add("Auth-Key", HttpUtility.HtmlEncode(AuthKey));
            }
            catch (NullReferenceException)
            {
                key.Close();
            }
            string result = await response.Content.ReadAsStringAsync();
            var properties = JsonConvert.DeserializeObject<NanoUserData>(result);
            key.Close();
            if (string.IsNullOrEmpty(properties.Username))
            {
                GetHealthCheckAsync("https://api.nanosdk.net/health");
                RequestRegistryData();
            }
            else
            {
                if (properties.Permission == 10)
                {
                    //open new window
                }
                else
                {
                    MessageBox.Show("YOUR NOT AN ADMIN GO AWAY");
                    GetHealthCheckAsync("https://api.nanosdk.net/health");
                    RequestRegistryData();
                }
            }
        }

        private void RequestRegistryData()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\nanoAPIADMIN");
            if (key != null)
            {
                UserOrEmailInput.Text = (string?)key.GetValue("UserOrEmail");
                PasswordInput.Password = (string?)key.GetValue("Password");
                key.Close();
            }
        }

        private async void GetHealthCheckAsync(string url)
        {
            var response = await nanoHttpclient.GetAsync(url);
            string result = await response.Content.ReadAsStringAsync();
            var properties = JsonConvert.DeserializeObject<ServerHealth>(result);
            if (!string.IsNullOrEmpty(properties.Status))
            {
                ServerResponseTxt.Text = properties.Status;
            }
            else
            {
                ServerResponseTxt.Text = "null";
            }
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckValidLoginDataAsync(UserOrEmailInput.Text, PasswordInput.Password);
        }

        private async void CheckValidLoginDataAsync(string usernameOrEmail, string password)
        {
            var content = new StringContent(JsonConvert.SerializeObject(new LoginData
            {
                Username = usernameOrEmail,
                Password = password
            }));
            string nanoLoginURL = "https://api.nanosdk.net/user/login";
            var response = await nanoHttpclient.PostAsync(nanoLoginURL, content);
            string result = await response.Content.ReadAsStringAsync();

            var loginProperties = JsonConvert.DeserializeObject<LoginBase<LoginResponse>>(result);
            if (loginProperties.message.Contains("Successfully executed login. Method used: Cookie. Cookie was returned"))
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\nanoAPIADMIN");

                key.SetValue("UserOrEmail", UserOrEmailInput.Text);
                key.SetValue("Password", PasswordInput.Password);
                key.SetValue("Auth-Key", loginProperties.Data.AuthKey);
                key.Close();
                Close();
                //open new window

            }
            else
            {
                MessageBox.Show(loginProperties.message, "Response");
            }
        }

    }
}

using nanoAPIAdminPanel.Auth;
using nanoAPIAdminPanel.Main;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace Auth.Utils
{
    public static class UtilsManager
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        public static NanoUserData User;
        public static string Health;

        private const string BASE_URL = "https://api.nanosdk.net";
        private static readonly Uri UserSelfUri = new Uri(BASE_URL + "/user/self");
        private static readonly Uri ServerHealthUri = new Uri(BASE_URL + "/health");
        private static readonly Uri LoginUri = new Uri(BASE_URL + "/user/login");

        public static bool IsLoggedInAndVerified() => IsUserLoggedIn() && User.IsVerified;

        public static bool IsUserLoggedIn()
        {
            if (User == null && !string.IsNullOrEmpty(UtilsConfig.Config.AuthKey))
            {
                CheckUserSelf();
            }
            return User != null;
        }

        public static void OpenLoginWindow()
        {
            LoginWindow login = new LoginWindow();
            login.InitializeComponent();
            login.Show();
        }
        private static void ClearLogin()
        {
            LoginWindow.NanoLog("Clearing login data", ConsoleColor.Yellow);
            User = null;
            UtilsConfig.Config.AuthKey = null;
            UtilsConfig.Save();
            OpenLoginWindow();
        }
        private static async Task<HttpResponseMessage> MakeApiCall(HttpRequestMessage request)
        {
            if (!string.IsNullOrEmpty(UtilsConfig.Config.AuthKey))
            {
                //NEEDS FIX!!!
               // string authKey = UtilsConfig.Config.AuthKey;
               // request.Headers.Add("Auth-Key", authKey);
            }

            var response = await HttpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                LoginWindow.NanoLog("Got 401 from api, please reauthenticate.", ConsoleColor.Red);
                ClearLogin();
                throw new Exception("API Call could not be completed.");
            }

            return response;
        }
        public static async void CheckServerHealth()
        {
            LoginWindow.NanoLog("Checking ServerHealth", ConsoleColor.Yellow);
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = ServerHealthUri
            };

            var response = await MakeApiCall(request);

            string result = await response.Content.ReadAsStringAsync();
            var properties = JsonConvert.DeserializeObject<ServerHealth>(result);
            Health = properties.Status;
            LoginWindow.NanoLog("Successfully checked serveHealth", ConsoleColor.Green);
            LoginWindow.NanoLog("Status: " + Health, ConsoleColor.White);
        }
        private static async void CheckUserSelf()
        {
            LoginWindow.NanoLog("Checking user", ConsoleColor.Yellow);
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = UserSelfUri
            };

            var response = await MakeApiCall(request);

            string result = await response.Content.ReadAsStringAsync();
            var properties = JsonConvert.DeserializeObject<BaseResponse<NanoUserData>>(result);
            User = properties.Data;
            LoginWindow.NanoLog("Successfully checked user", ConsoleColor.Green);
        }

        public static async void Login(string username, string password)
        {
            LoginWindow.NanoLog("Trying to login", ConsoleColor.White);
            var content = new StringContent(JsonConvert.SerializeObject(new APILoginData
            {
                Username = username,
                Password = password
            }));
            var request = new HttpRequestMessage
            {
                RequestUri = LoginUri,
                Content = content,
                Method = HttpMethod.Post
            };

            var response = await MakeApiCall(request);
            string result = await response.Content.ReadAsStringAsync();
            var properties = JsonConvert.DeserializeObject<BaseResponse<LoginResponse>>(result);

            if (!response.IsSuccessStatusCode)
            {
                LoginWindow.NanoLog("Login failed", ConsoleColor.Red);
                ClearLogin();
                return;
            }

            UtilsConfig.Config.AuthKey = properties.Data.AuthKey;
            UtilsConfig.Save();
            LoginWindow.NanoLog("Successfully logged in",ConsoleColor.Green);

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(LoginWindow))
                {
                    (window as LoginWindow).Close();
                }
            }
            CheckUserSelf();
        }

        public static void Logout() => ClearLogin();

    }
}

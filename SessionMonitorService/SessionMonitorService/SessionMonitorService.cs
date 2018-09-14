using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SessionMonitorService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SessionMonitorService
{
    public partial class SessionMonitorService : ServiceBase
    {
        private Timer _timer;
        private HttpClient _httpClient;
        private int _sessionId;

        public class UserSession
        {
            public string UserName { get; set; }

            public string ComputerName { get; set; }

            public string Status { get; set; }

            public int SessionId { get; set; }
        }

        public SessionMonitorService()
        {
            CanHandleSessionChangeEvent = true;
            ServiceName = "SessionMonitorService";
            InitializeComponent();

            var url = System.Configuration.ConfigurationManager.AppSettings["url"];
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(url);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        protected override void OnStart(string[] args)
        {
            EventLog.WriteEntry("SessionMonitorService.OnStart", DateTime.Now.ToLongTimeString());

            _timer = new Timer();
            _timer.Elapsed += OnElapsedTime;
            _timer.Interval = 5000;
            _timer.Start();
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry("SessionMonitorService.OnStop", DateTime.Now.ToLongTimeString());
            _timer.Stop();
            _timer.Dispose();
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            _sessionId = changeDescription.SessionId;

            string userName = GetUserName(changeDescription.SessionId);
            string computerName = GetDomainName(changeDescription.SessionId);

            EventLog.WriteEntry("SessionMonitorService.OnSessionChange", 
                $"{DateTime.Now.ToLongTimeString()} - Session change notice received: {changeDescription.Reason} Session ID: {changeDescription.SessionId} User name: {userName}");

            switch (changeDescription.Reason)
            {
                case SessionChangeReason.SessionLogon:
                    EventLog.WriteEntry("SessionMonitorService.OnSessionChange: Logon");
                    SendSessionInfo(userName, computerName, "Logon", changeDescription.SessionId);
                    break;

                case SessionChangeReason.SessionLogoff:
                    EventLog.WriteEntry("SessionMonitorService.OnSessionChange Logoff");
                    SendSessionInfo(userName, computerName, "Logoff", changeDescription.SessionId);
                    break;

                case SessionChangeReason.SessionLock:
                    EventLog.WriteEntry("SessionMonitorService.OnSessionChange Lock");
                    SendSessionInfo(userName, computerName, "Lock", changeDescription.SessionId);
                    break;

                case SessionChangeReason.SessionUnlock:
                    EventLog.WriteEntry("SessionMonitorService.OnSessionChange Unlock");
                    SendSessionInfo(userName, computerName, "Logon", changeDescription.SessionId);
                    break;
            }
        }

        private void SendSessionInfo(string userName, string computerName, string status, int sessionId)
        {
            var task = Task.Run(async () => await _httpClient.PostAsync("users", SerializeObject(new { username = userName, computerName, status, sessionId })));
            var response = task.Result;
            if (response.IsSuccessStatusCode)
                EventLog.WriteEntry($"SessionMonitorService.OnSessionChange: {userName} is successfully post to the server");
            else
                EventLog.WriteEntry($"SessionMonitorService.OnSessionChange: Error whlie post {userName} to the server");
        }

        private void LockUser()
        {
            WinApiHelper.ExecuteAppAsLoggedOnUser("rundll32.exe user32.dll,LockWorkStation");
        }

        private void RemoveUser(int sessionId)
        {
            var task = Task.Run(async () => await _httpClient.DeleteAsync($"users/{sessionId}"));
            var response = task.Result;
            if (response.IsSuccessStatusCode)
            {
                EventLog.WriteEntry("SessionMonitorService.LogoffUser: Successfully deleted user from the server");
            }
            else
            {
                EventLog.WriteEntry("SessionMonitorService.LogoffUser: Error whlie delete user from the server");
            }
        }

        private void LogoffUser(int sessionId)
        {
            var userName = GetUserName(sessionId);
            if (!string.IsNullOrEmpty(userName))
            {
                if (WinApiHelper.WTSLogoffSession(IntPtr.Zero, sessionId, true))
                {
                    RemoveUser(sessionId);
                }
            }
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            var task = Task.Run(async () => await _httpClient.GetAsync("/sessions/" + GetDomainName(_sessionId)));
            var response = task.Result;
            if (response.IsSuccessStatusCode)
            {
                EventLog.WriteEntry("SessionMonitorService.LogoffUser: Successfully get sessions from the server");
                var contentTask = Task.Run(async () => await response.Content.ReadAsStringAsync());
                var sessions = JsonConvert.DeserializeObject<List<UserSession>>(contentTask.Result);
                foreach (var session in sessions)
                {
                    if (session.Status == "Logoff")
                    {
                        LogoffUser(session.SessionId);
                    }
                    if (session.Status == "Lock")
                    {
                        LockUser();
                    }
                }
                    
            }
            else
            {
                EventLog.WriteEntry("SessionMonitorService.LogoffUser: Error while get sessions from the server");
            }
        }

        private static string GetUserName(int sessionId)
        {
            string username = "SYSTEM";
            if (WinApiHelper.WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSUserName, out IntPtr buffer, out int strLen) && strLen > 1)
            {
                username = Marshal.PtrToStringAnsi(buffer);
                WinApiHelper.WTSFreeMemory(buffer);
            }
            return username;
        }

        private static string GetDomainName(int sessionId)
        {
            string domainName = "SYSTEM";
            if (WinApiHelper.WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSDomainName, out IntPtr buffer, out int strLen) && strLen > 1)
            {
                domainName = Marshal.PtrToStringAnsi(buffer);
                WinApiHelper.WTSFreeMemory(buffer);
            }
            return domainName;
        }

        private static StringContent SerializeObject<T>(T model)
        {
            return new StringContent(
                JsonConvert.SerializeObject(model, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }), 
                Encoding.UTF8, 
                "application/json");
        }
    }
}

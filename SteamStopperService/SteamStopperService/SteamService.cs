using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.ServiceProcess;
using System.Text.RegularExpressions;

namespace SteamStopperService
{
    public partial class SteamService : ServiceBase
    {
        private HttpListener listener;       
        
        public SteamService()
        {
            InitializeComponent();
            this.ServiceName = "SteamService";            
        }
        
        protected override void OnStart(string[] args)
        {
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:8088/steam/");
                listener.Start();
                StartListening(listener);
            }
            catch(Exception)
            {                
                listener.Stop();
            }
            finally
            {
                
            }
        }

        protected override void OnStop()
        {            
            try
            {
                listener.Stop();
            }
            finally
            {
                listener.Close();
            }
        }

        private async void StartListening(HttpListener listener)
        {
            while (listener.IsListening)
            {
                try
                {                    
                    var context = await listener.GetContextAsync();                 
                    var request = context.Request;
                    string username = Regex.Match(request.RawUrl, "steam/(.*)/stop$").Groups[1].Value;
                    string responseString = "";
                    if (request.RawUrl.EndsWith("/stop"))
                    {
                        var proc = GetProcessInfoByWMI("steam");
                        if (proc == null)
                        {
                            responseString = "steam not running";
                        }
                        else
                        {
                            if (username.ToLower() != proc.Username.ToLower())
                            {
                                StopProcess(proc.ProcessId);
                                responseString = "steam stopped, pid: " + proc.ProcessId.ToString();
                            }
                            else
                            {
                                responseString = "already running steam process on user: " + proc.Username;
                            }
                        }
                    }

                    var response = context.Response;

                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    using (System.IO.Stream output = response.OutputStream)
                    {
                        output.Write(buffer, 0, buffer.Length);
                    }                    
                }

                catch (Exception)
                {                    
                }
            }
        }

        private static ProcessInfo GetProcessInfoByWMI(string processName)
        {
            ManagementObjectSearcher Processes = new ManagementObjectSearcher("SELECT * FROM Win32_Process");

            var processes = Process.GetProcesses();
            var WMIProcessesList = Processes.Get().Cast<ManagementObject>().ToList();

            var searchedProcess = processes.FirstOrDefault(p => p.ProcessName.ToLower() == processName);

            if (searchedProcess != null)
            {
                var wmiproc = WMIProcessesList.FirstOrDefault(proc => (int)(uint)proc["ProcessId"] == searchedProcess.Id);

                var OwnerInfo = new string[2];
                wmiproc.InvokeMethod("GetOwner", OwnerInfo);
                string user = OwnerInfo[0];

                return new ProcessInfo() { ProcessId = searchedProcess.Id, ProcessName = searchedProcess.ProcessName, Username = user };
            }
            return null;
        }

        private static void StopProcess(int processid)
        {
            var process = Process.GetProcessById(processid);
            if (process != null)
            {
                process.Kill();
            }
        }
    }

    class ProcessInfo
    {
        public string Username { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
    }
}

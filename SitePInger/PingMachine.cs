using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.NetworkInformation;
using System.Net;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace SitePInger
{
    class PingMachine
    {
        public delegate void ResponseReceived(string response);
        public event ResponseReceived OnResponseReceived;
        public delegate void ProgressUpdated(int current, int goal);
        public event ProgressUpdated OnProgressUpdated;
        public static string LocalIp
        {
            get
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    throw new Exception("You are not connected to the internet");
                }

                var resolver = new string[] { "http://ipinfo.io/ip", "https://api.ipify.org/" };
                foreach(var res in resolver)
                {
                    try
                    {
                        return new WebClient().DownloadString(res);
                    }
                    catch
                    {
                        continue;
                    }
                }
                throw new Exception("Cannot acquire current IP address");
            }
        }

        public static string GetIpLocation(string ip)
        {
            var api = $"http://api.ipstack.com/{ip}";
            var key = "69a0c8dd2311ae8687580572d9b0f412";
            var client = new RestClient(api);
            var request = new RestRequest();
            request.AddParameter("access_key", key);
            var response = client.Execute(request);
            var obj = JObject.Parse(response.Content);
            var locs = new string[] { obj["region_name"].ToString(), obj["city"].ToString(), obj["country_name"].ToString() };
            return string.Join(", ", locs.Where(x => x.Trim().Length > 0));
        }

        public void Start(string[] ips, int count = 4)
        {
            var goal = ips.Length * count;
            var local = LocalIp;
            OnResponseReceived?.Invoke($"Current IP: {local} ({GetIpLocation(local)})");
            new Thread(new ThreadStart(() =>
            {
                foreach (var ip in ips.Select((value, index) => new { value, index }))
                {
                    var roundTrip = new List<int>();
                    var head = $">>>>>>>>>>>>>>>>> Started to ping [{ip.value}]:  ";
                    OnResponseReceived?.Invoke(head);
                    for (int i = 0; i < count; i++)
                    {
                        OnResponseReceived(DoPing(ip.value));
                        OnProgressUpdated(ip.index * 4 + i+1, goal);
                        Thread.Sleep(1000);
                    }
                }
                OnResponseReceived?.Invoke("================== Done ===================");
            })).Start();
        }

        private string DoPing(string ip)
        {
            using (var p = new Ping())
            {
                try
                {
                    var reply = p.Send(ip);
                    var date = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                    return  $"> [{date}] {ip}: {reply.Status.ToString().ToLower()} | {reply.RoundtripTime}ms";
                }
                catch (PingException ex)
                {
                    return ex.Message;
                }
            }
        }

    }
}

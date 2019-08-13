using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.NetworkInformation;

namespace SitePInger
{
    class PingMachine
    {
        public delegate void ResponseReceived(string response);
        public event ResponseReceived OnResponseReceived;
        public delegate void ProgressUpdated(int current, int goal);
        public event ProgressUpdated OnProgressUpdated;

        public void Start(string[] ips, int count = 4)
        {
            var goal = ips.Length * count;
            new Thread(new ThreadStart(() =>
            {
                foreach (var ip in ips.Select((value, index) => new { value, index }))
                {
                    var head = $">>>>>>>>>>>>>>>>> Started to ping [{ip.value}]:  ";
                    OnResponseReceived?.Invoke(head);
                    for (int i = 0; i < count; i++)
                    {
                        DoPing(ip.value);
                        OnProgressUpdated(ip.index * 4 + i+1, goal);
                        Thread.Sleep(1000);
                    }
                }
                OnResponseReceived?.Invoke("================== Done ===================");
            })).Start();
        }

        private void DoPing(string ip)
        {
            using (var p = new Ping())
            {
                try
                {
                    var reply = p.Send(ip);
                    var response = $"> {ip}: {reply.Status.ToString().ToLower()} | {reply.RoundtripTime}ms ";
                    OnResponseReceived?.Invoke(response);
                }
                catch (PingException ex)
                {
                    OnResponseReceived?.Invoke(ex.Message);
                }
            }
        }
    }
}

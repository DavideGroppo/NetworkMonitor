using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NetworkMonitor.Objects
{
    public class UIPingModel
    {
        private PingReply ping;

        public UIPingModel(PingReply p)
        {
            this.ping = p;
        }

        public string ip
        {
            get
            {
                return ping.Address.ToString();
            }
        }
        public string rtt
        {
            get
            {
                return ping.RoundtripTime.ToString();
            }
        }
        public string status
        {
            get
            {
                return ping.Status.ToString();
            }
        }
        public string ttl { 
            get
            {
                return ping.Options?.Ttl.ToString() == null ? "" : ping.Options.Ttl.ToString();
            }
        }

    }
}
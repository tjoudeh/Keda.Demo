using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keda.Demo.Contracts
{
    public class Recipient
    {
        public string? Name { get; set; }
        public byte NotficationChannel { get; set; }
        public string? NotificationAddress { get; set; }
    }
}

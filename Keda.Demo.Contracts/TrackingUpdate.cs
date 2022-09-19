using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keda.Demo.Contracts
{
    public class TrackingUpdate
    {
        public string? Code { get; set; }
        public string? Source { get; set; }
        public string? Destination { get; set; }
        public string? Description { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}

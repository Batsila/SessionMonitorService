using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionsControllerServer.Models
{
    public class UserSession
    {
        public string UserName { get; set; }

        public string ComputerName { get; set; }

        public string Status { get; set; }

        public int SessionId { get; set; }
    }
}

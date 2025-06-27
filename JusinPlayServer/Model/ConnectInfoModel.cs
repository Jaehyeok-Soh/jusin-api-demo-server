using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JusinChatServer.Model
{
    class ConnectInfoModel
    {
        public bool isHost { get; set; } = false;
        public int netId { get; set; } = 0;
        public bool team { get; set; } = true;
        public int job { get; set; } = 0;
    }
}

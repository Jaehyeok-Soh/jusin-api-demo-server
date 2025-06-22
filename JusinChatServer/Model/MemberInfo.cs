using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

using JusinChatServer;

namespace JusinChatServer.Model
{
    class MemberInfo
    {
        public TcpClient? Client { get; set; } = null;
        public PlayerModel? Player { get; set; } = null;
        public ConnectInfoModel ConnectInfo { get; set; } = new ConnectInfoModel();
        public MemberInfo() { }
        public MemberInfo(TcpClient _client, ConnectInfoModel _connectInfo, out bool lastTeam)
        {
            Client = _client;
            ConnectInfo = _connectInfo;
            lastTeam = !ConnectInfo.team;
        }
    }
}

using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace JusinChatServer.Model
{
    class SessionModel
    {
        public int Id { get; set; } = 0;
        public int Count { get; set; } = 0;
        //각 멤버 순환
        //멤버에게 다른 멤버의 정보를 제공
        //멤버는 플레이어블 정보를 제공
        public List<MemberInfo> Members { get; set; } = new List<MemberInfo>();
        public List<NetworkStream> streamList { get; set; } = new List<NetworkStream>();
        public bool isStart { get; set; } = false;
        public bool lastTeam { get; set; } = true;
        public bool IsClose { get; set; } = false;
    }
}

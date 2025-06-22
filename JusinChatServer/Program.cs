using JusinChatServer.Model;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using MongoDB.Bson;
using System.Security.Claims;

namespace JusinChatServer
{
    public class Program
    {
        static string msg = "";
        static List<ConnectInfoModel> listJoinMsg = new List<ConnectInfoModel>();
        static List<SessionModel> sessionList = new List<SessionModel>();

        static bool lastTeam = true;
        static int gId = 0;

        private static async Task Main()
        {
            var listener = new TcpListener(IPAddress.Any, 9000);
            listener.Start();
            Console.WriteLine("Server started.");

            sessionList.Add(new SessionModel()
            {
                Id = gId++
            });


            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();

                if (sessionList[sessionList.Count - 1].Members.Count < 3 && !sessionList[sessionList.Count - 1].isStart)
                {
                    var connectInfo = new ConnectInfoModel()
                    {
                        isHost = sessionList[sessionList.Count - 1].Members.Count == 0,
                        netId = client.Client.Handle.ToInt32() + sessionList[sessionList.Count - 1].Members.Count,
                        team = sessionList[sessionList.Count - 1].lastTeam
                    };

                    sessionList[sessionList.Count - 1].Members
                        .Add(
                            new MemberInfo(client,
                                connectInfo,
                                out lastTeam)
                            );
                    sessionList[sessionList.Count - 1].streamList.Add(client.GetStream());
                    sessionList[sessionList.Count - 1].lastTeam = lastTeam;
                    _ = JoinSeq();
                }

                if (sessionList[sessionList.Count - 1].Members.Count == 2 && !sessionList[sessionList.Count - 1].isStart)
                {
                    _ = WaitStartSeq(sessionList[sessionList.Count - 1].Members[0].Client);
                }

                if (sessionList[sessionList.Count - 1].Members.Count == 2)
                {
                    _ = HandleClient(sessionList.Count - 1);
                }

                if (sessionList[sessionList.Count - 1].isStart)
                {
                    sessionList.Add(new SessionModel()
                    {
                        Id = gId++
                    });
                    lastTeam = true;
                }
            }
        }

        static async Task HandleClient(int sessionId)
        {
            byte[] buffer = new byte[4096];
            var session = sessionList[sessionId];
            while (!session.IsClose)
            {
                if (session.isStart && !session.IsClose)
                {
                    foreach (var stream in session.streamList)
                    {
                        int len = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (len == 0) break;

                        msg = Encoding.UTF8.GetString(buffer, 0, len);
                        Console.WriteLine($"Clinet : {msg}");

                        if (msg == "Quit")
                        {
                            session.IsClose = true;
                            break;
                        }

                        foreach (var innerStream in session.streamList)
                        {
                            if (stream == innerStream)
                                continue;

                            byte[] data = Encoding.UTF8.GetBytes(msg);
                            await innerStream.WriteAsync(data, 0, data.Length);
                        }
                    }
                }
            }

            foreach (var member in session.Members)
            {
                member.Client.Close();
            }
        }

        static async Task JoinSeq()
        {
            //TODO:
            //접속자의 플레이어 m_bIsMine = true로 할당하는 방법
            //타 접속자의 정보도 추가해줘야함
            //신규 접속자 발생 시 기존 접속자에게 타 접속자의 정보 추가하는 방법

            var currentSession = sessionList[sessionList.Count - 1];
            var lastMember = currentSession.Members[currentSession.Members.Count - 1];


            listJoinMsg.Add(lastMember.ConnectInfo);

            if (listJoinMsg.Count > 0 && listJoinMsg.Count < 3)
            {
                foreach (var item in currentSession.Members)
                {
                    var stream = item.Client.GetStream();
                    byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(listJoinMsg));
                    await stream.WriteAsync(data, 0, data.Length);
                }
            }
        }

        static async Task WaitStartSeq(TcpClient client)
        {
            var stream = client.GetStream();
            byte[] buffer = new byte[1024];

            while (!sessionList[sessionList.Count - 1].isStart)
            {
                int len = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (len == 0) break;

                msg = Encoding.UTF8.GetString(buffer, 0, len);
                Console.WriteLine($"Start : {msg}");
                if (msg == "true")
                {
                    sessionList[sessionList.Count - 1].isStart = true;
                }
            }

            if (sessionList[sessionList.Count - 1].isStart)
            {
                listJoinMsg.Clear();
            }
        }
    }
}

using JusinChatServer.Model;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace JusinChatServer
{
    public class Program
    {
        static string msg = "";
        static List<SessionModel> sessionList = new List<SessionModel>();

        static bool lastTeam = true;
        static int gId = 0;
        static bool init = true;

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
                var session = sessionList[sessionList.Count - 1];

                if (session.isStart || session.IsClose || session.Members.Count >= 2)
                {
                    session = new SessionModel() { Id = gId++ };
                    sessionList.Add(session);
                }

                var connectInfo = new ConnectInfoModel()
                {
                    isHost = session.Members.Count == 0,
                    netId = client.Client.Handle.ToInt32() + session.Members.Count,
                    team = session.lastTeam
                };

                var member = new MemberInfo(client, connectInfo, out lastTeam);
                session.Members.Add(member);
                session.lastTeam = lastTeam;

                session.JoinMsg.Add(connectInfo);

                var json = JsonConvert.SerializeObject(session.JoinMsg);
                foreach (var item in session.Members)
                {
                    await item.Client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(json));
                }

                _ = HandleClient(sessionList.Count - 1, member);
            }
        }

        static async Task HandleClient(int sessionId, MemberInfo member)
        {
            byte[] buffer = new byte[1024];
            var session = sessionList[sessionId];
            var stream = member.Client.GetStream();

            while (!session.IsClose)
            {
                int len;
                try
                {
                    len = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (len == 0) break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    session.IsClose = true;
                    foreach (var item in session.Members)
                    {
                        item.Client.Close();
                    }
                    break;
                }

                msg = Encoding.UTF8.GetString(buffer, 0, len);
                if (msg == "Quit")
                {
                    session.IsClose = true;
                    foreach (var item in session.Members)
                    {
                        item.Client.Close();
                    }
                    break;
                }

                if (msg == "true" && !session.isStart)
                {
                    session.isStart = true;
                }

                byte[] data = Encoding.UTF8.GetBytes(msg);

                var tasks = session.Members
                    .Where(item => item != member)
                    .Select(item =>
                    {
                        var stream = item.Client.GetStream();
                        return stream.WriteAsync(data, 0, data.Length);
                    });

                await Task.WhenAll(tasks);
            }

            member.Client.Close();
        }

        static async Task JoinSeq()
        {
            //TODO:
            //접속자의 플레이어 m_bIsMine = true로 할당하는 방법
            //타 접속자의 정보도 추가해줘야함
            //신규 접속자 발생 시 기존 접속자에게 타 접속자의 정보 추가하는 방법

            var currentSession = sessionList[sessionList.Count - 1];
            var lastMember = currentSession.Members[currentSession.Members.Count - 1];


            //listJoinMsg.Add(lastMember.ConnectInfo);

            //if (listJoinMsg.Count > 0 && listJoinMsg.Count < 3)
            //{
            //    foreach (var item in currentSession.Members)
            //    {
            //        if (item.Client == null)
            //            continue;

            //        var stream = item.Client.GetStream();
            //        byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(listJoinMsg));
            //        await stream.WriteAsync(data, 0, data.Length);
            //    }
            //}
        }

        static async Task WaitStartSeq(TcpClient client)
        {
            var stream = client.GetStream();
            byte[] buffer = new byte[1024];

            //while (!sessionList[sessionList.Count - 1].isStart)
            //{
            //    int len = await stream.ReadAsync(buffer, 0, buffer.Length);
            //    if (len == 0) break;

            //    msg = Encoding.UTF8.GetString(buffer, 0, len);
            //    Console.WriteLine($"Start : {msg}");
            //    if (msg == "true")
            //    {
            //        sessionList[sessionList.Count - 1].isStart = true;
            //    }
            //    if (msg == "Quit")
            //    {
            //        sessionList[sessionList.Count - 1].IsClose = true;
            //        listJoinMsg.Clear();
            //    }
            //}

            //if (sessionList[sessionList.Count - 1].isStart)
            //{
            //    listJoinMsg.Clear();
            //}
        }
    }
}

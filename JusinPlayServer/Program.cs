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
        static int lastJob = 0;
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
                    lastJob = 0;
                }

                var connectInfo = new ConnectInfoModel()
                {
                    isHost = session.Members.Count == 0,
                    netId = client.Client.Handle.ToInt32() + session.Members.Count,
                    team = session.lastTeam,
                    job = lastJob++
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
            byte[] buffer = new byte[4096];
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

                try
                {
                    if (msg == "")
                        continue;
                    
                    DTOPlayer? dtoPlayer = JsonConvert.DeserializeObject<DTOPlayer>(msg);

                    if (dtoPlayer == null)
                        continue;

                    if (dtoPlayer.isQuit)
                    {
                        session.IsClose = true;
                        foreach (var item in session.Members)
                        {
                            item.Client.Close();
                        }
                        break;
                    }

                    if (dtoPlayer.isStart && !session.isStart)
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
            }

            member.Client.Close();
        }
    }
}

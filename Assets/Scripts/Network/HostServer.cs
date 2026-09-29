//using System;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//using System.Collections.Generic;
//using UnityEngine;

//namespace PvpGameServer
//{
//    // 서버와 클라이언트 똑같은 패킷 규격(클래스) 제작
//    public class GamePacket
//    {
//        public int packetType { get; set; }
//        public int player { get; set; }
//        public string data { get; set; }
//    }

//    public class Program : MonoBehaviour
//    {
//        // 접속한 유저들을 담아둘 방
//        static List<TcpClient> connectedClients = new List<TcpClient>();

//        // 상점 레디 카운트 추적
//        static int readyCount = 0;

//        // 비동기 작업을 위해 async Task로 변경
//        static async Task Main(string[] args)
//        {
//            // 서버의 IP와 포트를 설정. 7777번 문 개방
//            TcpListener server = new TcpListener(IPAddress.Any, 7777);
//            server.Start();
//            Console.WriteLine("[서버] 가동 시작. 포트: 7777");

//            // 무한히 접속 받기
//            while (true)
//            {
//                TcpClient client = await server.AcceptTcpClientAsync();
//                connectedClients.Add(client);

//                int playerIndex = connectedClients.Count; // 첫 접속자는 1, 두 번째는 2
//                Console.WriteLine($"[서버] {playerIndex}P 접속 완료! (IP: {client.Client.RemoteEndPoint}");

//                // 2명이 꽉 찼는지 검사
//                if (connectedClients.Count == 2)
//                {
//                    int gameSeed = UnityEngine.Random.Range(10000, 99999);
//                    Console.WriteLine("[서버] 2명 매칭 성공! 양쪽에 게임 시작 패킷 발송");

//                    GamePacket p1_Packet = new GamePacket { packetType = 0, player = 1, data = gameSeed.ToString() };
//                    // 1P에게 1P라고 할당
//                    SendPacket(connectedClients[0], p1_Packet);

//                    GamePacket p2_Packet = new GamePacket { packetType = 0, player = 2, data = gameSeed.ToString() };
//                    // 2P에게 2P라고 할당
//                    SendPacket(connectedClients[1], p2_Packet);

//                    SendTurnStart(1);
//                }
//                else if (connectedClients.Count == 1)
//                {
//                    GamePacket wait_Packet = new GamePacket { packetType = 0, player = playerIndex, data = "Wait" };
//                    SendPacket(client, wait_Packet);
//                }

//                // 접속한 유저의 채팅을 듣는 작업을 비동기로 둠
//                _ = HandleClientAsync(client, playerIndex);
//            }
//        }

//        // 개별 유저의 패킷을 처리하는 담당 함수
//        static async Task HandleClientAsync(TcpClient client, int playerIndex)
//        {
//            NetworkStream stream = client.GetStream();
//            byte[] buffer = new byte[1024];

//            try
//            {
//                while (true)
//                {
//                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
//                    if (bytesRead == 0) break;

//                    string rawMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
//                    string[] jsonMessages = rawMessage.Split("\n");

//                    foreach (string jsonMessage in jsonMessages)
//                    {
//                        if (string.IsNullOrEmpty(jsonMessage)) continue;

//                        GamePacket receivedPacket = JsonSerializer.Deserialize<GamePacket>(jsonMessage);

//                        if (receivedPacket == null) break;

//                        if (receivedPacket.packetType == 1)
//                        {
//                            Console.WriteLine($"[서버] 턴 요청 확인");

//                            int nextTurnPlayer = (playerIndex == 1) ? 2 : 1;

//                            SendTurnStart(nextTurnPlayer);

//                            continue;
//                        }
//                        else if (receivedPacket.packetType == 2)
//                        {
//                            Console.WriteLine($"[서버] {playerIndex}P 스핀 요청! 양쪽 클라이언트에 동시 스핀 명령 하달.");

//                            receivedPacket.data = UnityEngine.Random.Range(10000, 99999).ToString();
//                        }
//                        else if (receivedPacket.packetType == 3)
//                        {
//                            int newReelSeed = UnityEngine.Random.Range(10000, 99999);
//                            receivedPacket.data = $"{receivedPacket.data}_{newReelSeed}";
//                        }
//                        else if (receivedPacket.packetType == 4)
//                        {
//                            Console.WriteLine($"[서버] {playerIndex}P 상점 준비 완료");
//                            readyCount++;

//                            // 상대 레디 완료를 띄울 수 있게 방송
//                            BroadcastPacket(receivedPacket);

//                            if (readyCount == 2)
//                            {
//                                Console.WriteLine("[서버] 양쪽 모두 Ready 완료");
//                                readyCount = 0;
//                                int nextRoundSeed = UnityEngine.Random.Range(10000, 99999);

//                                GamePacket nextRoundPacket = new GamePacket { packetType = 5, player = 0, data = nextRoundSeed.ToString() };
//                                BroadcastPacket(nextRoundPacket);

//                                SendTurnStart(1);
//                            }

//                            continue;
//                        }

//                        BroadcastPacket(receivedPacket);

//                    }
//                }
//            }
//            catch (Exception)
//            {
//                Console.WriteLine($"[서버] {playerIndex}P 연결 끊김");
//            }
//            finally
//            {
//                connectedClients.Remove(client);
//                client.Close();

//                Console.WriteLine($"[서버] {playerIndex}P 소켓 자원 해제 완료");

//                if (connectedClients.Count == 1)
//                {
//                    Console.WriteLine("[서버] 남은 플레이어에게 게임 종료 패킷 발송");

//                    GamePacket disconnectPacket = new GamePacket
//                    {
//                        packetType = 99,
//                        player = 0,
//                        data = "EnemyDisconnected"
//                    };

//                    // 남은 1명에게 발송
//                    SendPacket(connectedClients[0], disconnectPacket);
//                }
//                else if (connectedClients.Count == 0)
//                {
//                    readyCount = 0;
//                    Console.WriteLine("[서버] 방이 모두 비워짐.");
//                }

//            }
//        }

//        // 특정 한 명에게 패킷 전송
//        static async void SendPacket(TcpClient targetClient, GamePacket packet)
//        {
//            string json = JsonSerializer.Serialize(packet) + "\n";
//            byte[] bytes = Encoding.UTF8.GetBytes(json);
//            await targetClient.GetStream().WriteAsync(bytes, 0, bytes.Length);
//            await targetClient.GetStream().FlushAsync();
//        }

//        // 방 안의 모두에게 패킷 전송
//        static async void BroadcastPacket(GamePacket packet)
//        {
//            string json = JsonSerializer.Serialize(packet) + "\n";
//            byte[] bytes = Encoding.UTF8.GetBytes(json);

//            foreach (var c in connectedClients.ToList())
//            {
//                await c.GetStream().WriteAsync(bytes, 0, bytes.Length);
//                await c.GetStream().FlushAsync();
//            }
//        }

//        static void SendTurnStart(int turnPalyerindex)
//        {
//            int resetSeed = UnityEngine.Random.Range(10000, 99999);

//            GamePacket turnPacket = new GamePacket
//            {
//                packetType = 1, // 턴 알림
//                player = turnPalyerindex,
//                data = resetSeed.ToString()
//            };
//            BroadcastPacket(turnPacket);
//        }
//    }
//}

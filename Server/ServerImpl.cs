using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcNetworking
{
    class ServerImpl : DataStorage.DataStorageBase
    {
        const string PLAYER_PREFIX = "/player";
        const string COUNT_FILE = "/count";
        Random rnd;

        public ServerImpl()
        {
            rnd = new Random();
        }

        public override Task<SaveResponse> SavePlayer(PlayerData request, ServerCallContext context)
        {
            return Task.FromResult<SaveResponse>(SavePlayerToFile(request));
        }

        public override Task<PlayerData> LoadPlayer(LoadRequest request, ServerCallContext context)
        {
            return Task.FromResult<PlayerData>(LoadPlayerFromFile(request));
        }

        string GetDir(int round)
        {
            return "players/round" + round;
        }

        int LoadPlayerCount(int round)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            string countPath = GetDir(round) + COUNT_FILE;
            int savedPlayers = 0;

            if (File.Exists(countPath))
            {
                FileStream countStream = new FileStream(countPath, FileMode.Open);
                savedPlayers = (int)formatter.Deserialize(countStream);
                countStream.Close();
            }

            Console.WriteLine("Found " + savedPlayers + " saved players at round " + round);

            return savedPlayers;
        }

        public SaveResponse SavePlayerToFile(PlayerData data)
        {
            if (!Directory.Exists(GetDir(data.Round)))
            {
                Directory.CreateDirectory(GetDir(data.Round));
            }

            BinaryFormatter formatter = new BinaryFormatter();
            string path = GetDir(data.Round) + PLAYER_PREFIX;
            string countPath = GetDir(data.Round) + COUNT_FILE;

            Console.WriteLine(path + "\n" + countPath);

            int savedPlayers = LoadPlayerCount(data.Round);

            FileStream countStream = new FileStream(countPath, FileMode.Create);
            formatter.Serialize(countStream, savedPlayers + 1);
            countStream.Close();

            FileStream stream = new FileStream(path + savedPlayers, FileMode.Create);
            data.SerializedPlayerData.WriteTo(stream);
            stream.Close();

            return new SaveResponse();
        }

        public PlayerData LoadPlayerFromFile(LoadRequest request)
        {
            string path = GetDir(request.Round) + PLAYER_PREFIX;

            int savedPlayers = LoadPlayerCount(request.Round);

            if (savedPlayers < 1) return null;

            int triesLeft = 10;
            while (triesLeft > 0)
            {
                int x = rnd.Next(savedPlayers - 1);

                Console.WriteLine("Loading player " + x + " from round " + request.Round);

                if (File.Exists(path + x))
                {
                    FileStream stream = new FileStream(path + x, FileMode.Open);
                    PlayerData data = new PlayerData {
                        Round = request.Round,
                        SerializedPlayerData = Google.Protobuf.ByteString.FromStream(stream),
                    };
                    stream.Close();

                    return data;
                }
                triesLeft--;
            }
            return null;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            const int Port = 8001;

            Server server = new Server
            {
                Services = { DataStorage.BindService(new ServerImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("DataStorage Server listening on port " + Port);
            Console.WriteLine("Press any key to stop server");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
            Console.WriteLine("Server Stopped!");
        }
    }
}

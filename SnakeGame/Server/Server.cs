using NetworkUtil;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml;
using WorldData;
using System.Diagnostics;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace Server;

class Server
{
    // A map of clients that are connected, each with an ID
    private Dictionary<long, SocketState> clients;
    private World theWorld;
    private GameSettings settings;

    static void Main(string[] args)
    {
        Server server = new Server();
        server.StartServer();

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        while (true)
        {
            while (stopWatch.ElapsedMilliseconds < server.settings.MSPerFrame) { }

            stopWatch.Restart();
            server.UpdateWorld();
        }
    }

    /// <summary>
    /// Initialized the server's state
    /// </summary>
    public Server()
    {
        clients = new Dictionary<long, SocketState>();
        theWorld = new World();
        settings = new GameSettings();
    }

    // updates the world
    public void UpdateWorld()
    {
        string snakesJSON = "";
        int powerupID = 1;

        if (theWorld.Powerups.Count > 0)
        {
            powerupID = theWorld.Powerups.Values.Count + 1;
        }
        
        if(theWorld.Powerups.Count < 100 && DateTime.Now.Millisecond % 50 == 0)
        {
            theWorld.Powerups.Add(powerupID, new Powerup(powerupID, theWorld));
        }

        HashSet<long> disconnectedClients = new HashSet<long>();

        lock (theWorld)
        {
            foreach (KeyValuePair<int, Snake> entry in theWorld.Snakes)
            {
                entry.Value.Update();

                snakesJSON += JsonConvert.SerializeObject(entry.Value) + "\n";
            }

            foreach (KeyValuePair<int, Powerup> entry in theWorld.Powerups)
            {
                snakesJSON += JsonConvert.SerializeObject(entry.Value) + "\n";
            }

            foreach (SocketState client in clients.Values)
            {
                if (!Networking.Send(client.TheSocket!, snakesJSON))
                {
                    disconnectedClients.Add(client.ID);
                    theWorld.Snakes[(int)client.ID].dc = true;
                }
            }
            foreach (long id in disconnectedClients)
            {
                RemoveClient(id);
            }
        }

    }


    /// <summary>
    /// Start accepting Tcp sockets connections from clients
    /// </summary>
    public void StartServer()
    {
        DataContractSerializer ser = new(typeof(GameSettings));
        XmlReader reader = XmlReader.Create("settings.xml");
        GameSettings? s = (GameSettings?)ser.ReadObject(reader);

        if (s != null)
        {
            settings = s;

            foreach (Wall w in settings.Walls)
            {
                theWorld.Walls.Add(w.wall, w);
            }

            theWorld.respawnRate = settings.RespawnRate;
            theWorld.size = settings.UniverseSize;

            // This begins an "event loop"
            Networking.StartServer(NewClientConnected, 11000);

            Console.WriteLine("Server is running accepting new clients");
        }
        else
        {
            Console.WriteLine("Unable to parse settings file");
        }
    }

    /// <summary>
    /// Method to be invoked by the networking library
    /// when a new client connects (see line 41)
    /// </summary>
    /// <param name="state">The SocketState representing the new client</param>
    private void NewClientConnected(SocketState state)
    {
        if (state.ErrorOccurred)
            return;

        // change the state's network action to the 
        // receive handler so we can process data when something
        // happens on the network
        state.OnNetworkAction = ReceiveMessage;

        Networking.GetData(state);
    }

    /// <summary>
    /// Method to be invoked by the networking library
    /// when a network action occurs (see lines 64-66)
    /// </summary>
    /// <param name="state"></param>
    private void ReceiveMessage(SocketState state)
    {
        // Remove the client if they aren't still connected
        if (state.ErrorOccurred)
        {
            RemoveClient(state.ID);
            return;
        }

        ProcessMessage(state);
        // Continue the event loop that receives messages from this client
        Networking.GetData(state);
    }


    /// <summary>
    /// Given the data that has arrived so far, 
    /// potentially from multiple receive operations, 
    /// determine if we have enough to make a complete message,
    /// and process it (print it and broadcast it to other clients).
    /// </summary>
    /// <param name="sender">The SocketState that represents the client</param>
    private void ProcessMessage(SocketState state)
    {
        string totalData = state.GetData();

        string[] parts = Regex.Split(totalData, @"(?<=[\n])");

        if (parts[0].Length < 16 && !parts[0].Contains("{"))
        {
            string name = parts[0].Substring(0, parts[0].Length - 1);
            theWorld.Snakes.Add((int)state.ID, new Snake((int)state.ID, name, theWorld));

            Networking.Send(state.TheSocket, state.ID + "\n");
            Networking.Send(state.TheSocket, settings.UniverseSize + "\n");

            string wallsJSON = "";
            foreach (Wall entry in settings.Walls)
            {
                wallsJSON += JsonConvert.SerializeObject(entry) + "\n";
            }

            Networking.Send(state.TheSocket, wallsJSON);

            // Save the client state
            // Need to lock here because clients can disconnect at any time
            lock (clients)
            {
                clients[state.ID] = state;
            }

            Console.WriteLine("Snake: " + name + " connected.");
        }

        // Loop until we have processed all messages.
        // We may have received more than one.
        foreach (string p in parts)
        {
            // Ignore empty strings added by the regex splitter
            if (p.Length == 0)
                continue;
            // The regex splitter will include the last string even if it doesn't end with a '\n',
            // So we need to ignore it if this happens. 
            if (p[p.Length - 1] != '\n')
                break;

            // Remove it from the SocketState's growable buffer
            state.RemoveData(0, p.Length);

            // handle movements

            string direction = p.Trim().Substring(0, p.Length - 1);
            Snake thisSnake = theWorld.Snakes[(int)state.ID];
            switch (direction)
            {
                case "{\"moving\":\"left\"}":
                    thisSnake.ChangeDirection("left");
                    break;
                case "{\"moving\":\"up\"}":
                    thisSnake.ChangeDirection("up");
                    break;
                case "{\"moving\":\"down\"}":
                    thisSnake.ChangeDirection("down");
                    break;
                case "{\"moving\":\"right\"}":
                    thisSnake.ChangeDirection("right");
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Removes a client from the clients dictionary
    /// </summary>
    /// <param name="id">The ID of the client</param>
    private void RemoveClient(long id)
    {
        Console.WriteLine("Client " + id + " disconnected");
        lock (clients)
        {
            clients.Remove(id);
        }
    }
}

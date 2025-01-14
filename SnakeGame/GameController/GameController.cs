using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NetworkUtil;
using WorldData;
using System.Xml.Linq;

namespace GameUtil
{

    public class GameController
    {
        private World theWorld;

        public delegate void ConnectedHandler();
        public event ConnectedHandler? Connected;

        public delegate void ErrorHandler(string err);
        public event ErrorHandler? Error;

        // A delegate and event to fire when the controller
        // has received and processed new info from the server
        public delegate void GameUpdateHandler();
        public event GameUpdateHandler? UpdateArrived;

        public GameController()
        {
            theWorld = new World();
        }

        /// <summary>
        /// State representing the connection with the server
        /// </summary>
        SocketState? theServer = null;

        public World GetWorld()
        {
            return theWorld;
        }

        public void MovePlayer(string dir)
        {
            // given snake head, loop for parts

            // check if is alive

            // if snake is alive, check input direction
            if (theServer != null)
            {
                Networking.Send(theServer.TheSocket, "{\"moving\":\"" + dir + "\"}\n");
            }
        }

        /// <summary>
        /// Begins the process of connecting to the server
        /// </summary>
        /// <param name="addr"></param>
        public void Connect(string addr, string name)
        {
            theWorld.mySnakeName = name;
            Networking.ConnectToServer(OnConnect, addr, 11000);
        }


        /// <summary>
        /// Method to be invoked by the networking library when a connection is made
        /// </summary>
        /// <param name="state"></param>
        private void OnConnect(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                // inform the view
                Error?.Invoke("Error connecting to server");
                return;
            }

            // inform the view
            Connected?.Invoke();

            theServer = state;

            Networking.Send(theServer.TheSocket, theWorld.mySnakeName + "\n");

            // Start an event loop to receive messages from the server
            state.OnNetworkAction = ReceiveMessage;
            Networking.GetData(state);


        }

        /// <summary>
        /// Method to be invoked by the networking library when 
        /// data is available
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveMessage(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                // inform the view
                Error?.Invoke("Lost connection to server");
                return;
            }

            ProcessMessages(state);

            // Continue the event loop
            // state.OnNetworkAction has not been changed, 
            // so this same method (ReceiveMessage) 
            // will be invoked when more data arrives
            Networking.GetData(state);
        }

        /// <summary>
        /// Process any buffered messages separated by '\n'
        /// Then inform the view
        /// </summary>
        /// <param name="state"></param>
        private void ProcessMessages(SocketState state)
        {
            string totalData = state.GetData();
            string[] parts = System.Text.RegularExpressions.Regex.Split(totalData, @"(?<=[\n])");

            // Loop until we have processed all messages.
            // We may have received more than one.

            List<string> newMessages = new List<string>();

            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;
                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[p.Length - 1] != '\n')
                    break;

                // build a list of messages to send to the view
                newMessages.Add(p);

                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);
            }

            if (Int32.TryParse(newMessages[0], out int i))
            {
                theWorld.mySnake = i;
                if (Int32.TryParse(newMessages[1], out int x))
                {
                    theWorld.worldSize = x;
                }
            }

            lock (theWorld)
            {
                foreach (string gameObject in newMessages)
                {
                    if (gameObject.StartsWith("{\"snake\":"))
                    {
                        Snake? s = JsonConvert.DeserializeObject<Snake>(gameObject);
                        if (s != null)
                        {
                            if (s.dc)
                                theWorld.Snakes.Remove(s.snake);
                            else
                                theWorld.Snakes[s.snake] = s;
                        }
                    }
                    if (gameObject.StartsWith("{\"wall\":"))
                    {
                        Wall? w = JsonConvert.DeserializeObject<Wall>(gameObject);
                        if (w != null)
                        {
                            theWorld.Walls[w.wall] = w;
                        }
                    }
                    if (gameObject.StartsWith("{\"power\":"))
                    {
                        Powerup? p = JsonConvert.DeserializeObject<Powerup>(gameObject);
                        if (p != null)
                        {
                            if (p.died)
                                theWorld.Powerups.Remove(p.power);
                            else
                                theWorld.Powerups[p.power] = p;
                        }
                    }

                }
            }

            lock (theWorld)
            {
                // inform the view
                UpdateArrived?.Invoke();
            }

        }

        /// <summary>
        /// Closes the connection with the server
        /// </summary>
        public void Close()
        {
            theServer?.TheSocket.Close();
        }


    }
}



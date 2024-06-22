using System;
using System.IO.Pipes;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.Serialization.Json;
using System.Threading;


/* 
 *                  License Apache-2.0
 * 
 * Copyright 2024
 * GitHub: https://github.com/sony671/CSharp-SingleInstanceHandler
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/
namespace SingleInstance
{
    /// <summary>
    /// Handles single instance of this application. Can call the first instance.
    /// </summary>
    public static class SingleInstanceHandler
    {
        public delegate void InstanceEvent(string[] SenderArgs);

        /// <summary>
        /// If the program receives a call from another Instance, then this <see cref="InstanceEvent"/> is called.
        /// </summary>
        public static event InstanceEvent OnReceiveArgsEvent;

        /// <summary>
        /// This apps Id
        /// </summary>
        private static string InstanceApplicationId = null;
        private static bool? IsFirstInstance = null;
        private static NamedPipeServerStream ServerStream = null;
        private static Mutex InstanceSection = null;

        /// <summary>
        /// Returns if the application is the first Instance else <see cref="Environment.Exit(int)"/> the program. If SendLineArgs then the program's args are sent to the first Instance.
        /// </summary>
        /// <param name="ApplicationId">An id of the application. It can be the name of the application. This is used to find an instance of this program.</param>
        /// <param name="SendLineArgs">If true, the Command line arguments are sent to the first Instance.</param>
        /// <exception cref="ArgumentNullException">If ApplicationId is null or empty.</exception> 
        public static void LaunchOrExit(string ApplicationId, bool SendLineArgs = true)
        {
            if (string.IsNullOrWhiteSpace(ApplicationId))
                throw new ArgumentNullException(nameof(ApplicationId));

            InstanceApplicationId = ApplicationId;
            if (GetIsFirstInstance(ApplicationId))
            {
                ActivateReceive();
                return;
            }


            if (SendLineArgs)
                SendArgsToInstance(Environment.GetCommandLineArgs().Skip(1).ToArray());

            Environment.Exit(0);
        }
        /// <summary>
        /// Returns True if the application is the first Instance else false. If SendLineArgs then the program's args are sent to the first Instance.
        /// </summary>
        /// <param name="ApplicationId">An id of the application. It can be the name of the application. This is used to find an instance of this program.</param>
        /// <param name="SendLineArgs">If true, the Command line arguments are sent to the first Instance.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">If ApplicationId is null or empty.</exception>
        public static bool CheckAndLaunch(string ApplicationId, bool SendLineArgs = true)
        {
            if (string.IsNullOrWhiteSpace(ApplicationId))
                throw new ArgumentNullException(nameof(ApplicationId));

            InstanceApplicationId = ApplicationId;
            ActivateReceive();

            if (GetIsFirstInstance(ApplicationId))
                return true;

            if (SendLineArgs)
            {
                SendArgsToInstance(Environment.GetCommandLineArgs());
            }

            return false;
        }


        /// <summary>
        ///     Returns if application is the first instance
        /// </summary>
        /// <returns>True if First Instance else false</returns>
        private static bool GetIsFirstInstance(string ApplicationId)
        {
            if (IsFirstInstance.HasValue)
                return IsFirstInstance.Value;


            InstanceSection = new Mutex(true, ApplicationId, out bool Instance);
            IsFirstInstance = Instance;
            return IsFirstInstance.Value;
        }


        /// <summary>
        ///     Uses named pipe to send the currently arguments to an already running instance.
        /// </summary>
        /// <param name="namedPipePayload"></param>
        private static void SendArgsToInstance(string[] args)
        {
            try
            {
                //"." or "localhost" to restrict to local calls
                using (var namedPipeClientStream = new NamedPipeClientStream(".", InstanceApplicationId, PipeDirection.Out))
                {
                    namedPipeClientStream.Connect(1000); // Maximum wait 1 seconds

                    var ser = new DataContractJsonSerializer(typeof(string[]));
                    ser.WriteObject(namedPipeClientStream, args);
                }
            }
            catch (Exception)// Error when connecting/sending. 
            {
                // ignores all Exception.
            }
        }


        /// <summary>
        /// Sets up the NamedPipeServerStream used to receive arguments. 
        /// </summary>
        private static void ActivateReceive()
        {
            ServerStream = new NamedPipeServerStream(
                InstanceApplicationId,
                PipeDirection.In,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            // Begin async wait for connections
            ServerStream.BeginWaitForConnection(OnReceive, ServerStream);
        }
        /// <summary>
        /// Calls OnReceiveArgsEvent and gives the arguments
        /// </summary>
        /// <param name="AsyncResult"></param>
        private static void OnReceive(IAsyncResult AsyncResult)
        {
            try
            {
                ServerStream.EndWaitForConnection(AsyncResult);

                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(string[]));
                OnReceiveArgsEvent?.Invoke((string[])ser.ReadObject(ServerStream));

                // Resets
                ServerStream.Disconnect();
                ServerStream.BeginWaitForConnection(OnReceive, ServerStream);
            }
            catch (Exception) // Error when receiving data
            {
                // ignores all Exception.
            }
        }
    }

}

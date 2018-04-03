using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CsgoDiscordRich
{
    public class Discord: IDisposable
    {
        private bool isDisposed = false;
        private bool isRunning = false;
        private bool isReady = false;
        public bool IsReady
        {
            get
            {
                return isReady;
            }
            set
            {
                isReady = value;
                if (!isReady)
                {
                    isRunning = false;
                }
            }
        }
        private Thread thread;

        public event Callback Ready;
        public event ErrorCallback Disconnected;
        public event ErrorCallback Errored;
        public event JoinRequestCallback JoinRequest;
        public event JoinCallback JoinGame;
        public event JoinCallback SpectateGame;

        private EventHandlers handlers = new EventHandlers();

        private string applicationId;
        private string steamId;

        public Discord(string applicationId, string steamId = null)
        {
            this.applicationId = applicationId;
            this.steamId = steamId;
            thread = new Thread(MainLoop);
        }

        ~Discord()
        {
            Dispose(false);
        }

        private void OnReady()
        {
            IsReady = true;
            Ready?.Invoke();
        }
        private void OnDisconnected(int errorCode, string message)
        {
            IsReady = false;
            Disconnected?.Invoke(errorCode, message);
        }
        private void OnError(int errorCode, string message)
        {
            IsReady = false;
            Errored?.Invoke(errorCode, message);
        }
        private void OnJoinGame(string secret)
        {
            JoinGame?.Invoke(secret);
        }
        private void OnSpectateGame(string secret)
        {
            SpectateGame?.Invoke(secret);
        }
        private void OnJoinRequest(in JoinRequestData data)
        {
            JoinRequest?.Invoke(in data);
        }

        public void Start()
        {
            handlers.Ready = OnReady;
            handlers.Errored = OnError;
            handlers.Disconnected = OnDisconnected;
            handlers.JoinRequest = OnJoinRequest;
            handlers.JoinGame = OnJoinGame;
            handlers.SpectateGame = OnSpectateGame;

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                InitializeWindows(applicationId, ref handlers, 1, steamId);
            }

            isRunning = true;
            thread.Start();
        }

        private void MainLoop()
        {
            while (isRunning)
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    RunCallbacksWindows();
                }
                Thread.Sleep(150);
            }
        }

        private void UpdateHandlers()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                UpdateHandlersWindows(ref handlers);
            }
        }

        public void ClearPresence()
        {
            if (!IsReady)
            {
                throw new Exception("Discord integration isn't running.");
            }
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                ClearPresenceWindows();
            }
        }

        public void UpdatePresence(in RichPresence presence)
        {
            if (!IsReady)
            {
                throw new Exception("Discord integration isn't running.");
            }
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                UpdatePresenceWindows(in presence);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposed) return;

            if (isDisposing)
            {
                IsReady = false;
            }
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                ShutdownWindows();
            }
            isDisposed = true;
        }

        #region Native Interop
        public delegate void Callback();
        public delegate void ErrorCallback(int errorCode, [MarshalAs(UnmanagedType.LPStr)] string error);
        public delegate void JoinCallback([MarshalAs(UnmanagedType.LPStr)] string value);
        public delegate void JoinRequestCallback(in JoinRequestData request);

        [StructLayout(LayoutKind.Sequential)]
        private struct EventHandlers
        {
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public Callback Ready;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public ErrorCallback Disconnected;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public ErrorCallback Errored;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public JoinCallback JoinGame;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public JoinCallback SpectateGame;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public JoinRequestCallback JoinRequest;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct JoinRequestData
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string UserId;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Username;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Discriminator;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Avatar;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RichPresence
        {
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 128)]
            public string State;
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 128)]
            public string Details;
            public long StartTimestamp;
            public long EndTimestamp;
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 32)]
            public string LargeImageKey;
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 128)]
            public string LargeImageText;
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 32)]
            public string SmallImageKey;
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 128)]
            public string SmallImageText;
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 128)]
            public string PartyId;
            int PartySize;
            int PartyMax;
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 128)]
            public string MatchSecret;
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 128)]
            public string JoinSecret;
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 128)]
            public string SpectateSecret;
            public short Instance;
        }

        [DllImport("win\\discord-rpc.dll", EntryPoint = "Discord_Initialize")]
        private static extern void InitializeWindows([MarshalAs(UnmanagedType.LPStr)] string applicationId, ref EventHandlers handlers, int autoRegister, [MarshalAs(UnmanagedType.LPStr)]  string steamId);

        [DllImport("win\\discord-rpc.dll", EntryPoint = "Discord_Shutdown")]
        private static extern void ShutdownWindows();

        [DllImport("win\\discord-rpc.dll", EntryPoint = "Discord_RunCallbacks")]
        private static extern void RunCallbacksWindows();

        [DllImport("win\\discord-rpc.dll", EntryPoint = "Discord_ClearPresence")]
        private static extern void ClearPresenceWindows();

        [DllImport("win\\discord-rpc.dll", EntryPoint = "Discord_UpdatePresence")]
        private static extern void UpdatePresenceWindows(in RichPresence presence);

        [DllImport("win\\discord-rpc.dll", EntryPoint = "Discord_UpdateHandlers")]
        private static extern void UpdateHandlersWindows(ref EventHandlers handlers);
        #endregion
    }
}

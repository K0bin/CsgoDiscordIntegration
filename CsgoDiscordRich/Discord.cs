using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CsgoDiscordRich
{
    public class Discord: IDisposable
    {
        public delegate void Callback();
        public delegate void ErrorCallback(int errorCode, [MarshalAs(UnmanagedType.LPStr)] string error);
        public delegate void JoinCallback([MarshalAs(UnmanagedType.LPStr)] string value);

        [StructLayout(LayoutKind.Sequential)]
        public struct EventHandlers
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
            public JoinCallback JoinRequest;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct JoinRequest
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

        private bool isDisposed = false;
        private bool isRunning = true;
        private Thread thread;
        public Discord(string applicationId, ref EventHandlers handlers, bool autoRegister = true, string steamId = null)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                InitializeWindows(applicationId, ref handlers, autoRegister ? 1 : 0, steamId);
            }
            thread = new Thread(MainLoop);
            thread.Start();
        }

        ~Discord()
        {
            Dispose(false);
        }

        private void MainLoop()
        {
            while (isRunning)
            {
                RunCallbacks();
            }
        }

        public void ClearPresence()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                ClearPresenceWindows();
            }
        }

        public void RunCallbacks()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                RunCallbacksWindows();
            }
        }

        public void UpdatePresence(in RichPresence presence)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                UpdatePresenceWindows(in presence);
            }
        }

        public void UpdateHandlers(ref EventHandlers handlers)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                UpdateHandlersWindows(ref handlers);
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
            }
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                ShutdownWindows();
            }
            isDisposed = true;
        }
    }
}

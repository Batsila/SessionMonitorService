using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SessionMonitorService
{

    internal class WinApiHelper
    {
        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const int TOKEN_QUERY = 0x00000008;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        internal const int TOKEN_ASSIGN_PRIMARY = 0x0001;
        internal const int TOKEN_DUPLICATE = 0x0002;
        internal const int TOKEN_IMPERSONATE = 0X00000004;
        internal const int TOKEN_ADJUST_DEFAULT = 0x0080;
        internal const int TOKEN_ADJUST_SESSIONID = 0x0100;
        internal const int MAXIMUM_ALLOWED = 0x2000000;
        internal const int CREATE_UNICODE_ENVIRONMENT = 0x00000400;
        internal const int NORMAL_PRIORITY_CLASS = 0x20;
        internal const int CREATE_NEW_CONSOLE = 0x00000010;

        internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        internal const string SE_TCB_NAME = "SeTcbPrivilege";
        internal const string SE_RESTORE_NAME = "SeRestorePrivilege";

        private static WindowsImpersonationContext impersonatedUser;
        public static IntPtr hToken = IntPtr.Zero;
        public static IntPtr dupeTokenHandle = IntPtr.Zero;
        const string SE_INCREASE_QUOTA_NAME = "SeIncreaseQuotaPrivilege";

        [DllImport("Wtsapi32.dll")]
        internal static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WtsInfoClass wtsInfoClass, out IntPtr ppBuffer, out int pBytesReturned);

        [DllImport("Wtsapi32.dll")]
        internal static extern void WTSFreeMemory(IntPtr pointer);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        internal static extern bool WTSLogoffSession(IntPtr hServer, int sessionId, bool bWait);

        [DllImport("kernel32", SetLastError = true), SuppressUnmanagedCodeSecurityAttribute]
        static extern bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateToken(IntPtr ExistingTokenHandle, int SECURITY_IMPERSONATION_LEVEL, ref IntPtr DuplicateTokenHandle);

        [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx")]
        static extern bool DuplicateTokenEx(IntPtr hExistingToken, Int32 dwDesiredAccess,
                            ref SECURITY_ATTRIBUTES lpThreadAttributes,
                            Int32 ImpersonationLevel, Int32 dwTokenType,
                            ref IntPtr phNewToken);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        public static bool ExecuteAppAsLoggedOnUser(string CmdLineArgs)
        {
            IntPtr duplicateToken = IntPtr.Zero;
            IntPtr shellProcessToken = IntPtr.Zero;

            var explorerProcess = Process.GetProcessesByName("explorer");
            if (explorerProcess == null || !explorerProcess.Any())
                return false;

            Process shellProcess = explorerProcess.First();
            shellProcess.StartInfo.LoadUserProfile = true;

            try
            {
                int tokenRights = MAXIMUM_ALLOWED;
                if (!OpenProcessToken(shellProcess.Handle, tokenRights, ref shellProcessToken))
                    return false;

                SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
                sa.nLength = Marshal.SizeOf(sa);

                if (!DuplicateTokenEx(shellProcessToken, tokenRights, ref sa, 2, 1, ref duplicateToken))
                    return false;

                SECURITY_ATTRIBUTES processAttributes = new SECURITY_ATTRIBUTES();
                SECURITY_ATTRIBUTES threadAttributes = new SECURITY_ATTRIBUTES();
                STARTUPINFO si = new STARTUPINFO();
                si.cb = (uint)Marshal.SizeOf(si);

                uint dwCreationFlags = NORMAL_PRIORITY_CLASS | CREATE_NEW_CONSOLE;
                if (!CreateProcessAsUser(duplicateToken, null, CmdLineArgs, ref processAttributes, ref threadAttributes, true, dwCreationFlags, IntPtr.Zero, null, ref si, out PROCESS_INFORMATION pi))
                    return false;

            }
            finally
            {
                if (shellProcessToken != null) CloseHandle(shellProcessToken);
                if (duplicateToken != null) CloseHandle(duplicateToken);
            }
            return true;
        }
    }

    struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public uint dwProcessId;
        public uint dwThreadId;
    }

    struct STARTUPINFO
    {
        public uint cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public uint dwX;
        public uint dwY;
        public uint dwXSize;
        public uint dwYSize;
        public uint dwXCountChars;
        public uint dwYCountChars;
        public uint dwFillAttribute;
        public uint dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SECURITY_ATTRIBUTES
    {
        public int nLength;
        public IntPtr lpSecurityDescriptor;
        public int bInheritHandle;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct TokPriv1Luid
    {
        public int Count;
        public long Luid;
        public int Attr;
    }

    internal enum WtsInfoClass
    {
        WTSUserName = 5,
        WTSDomainName = 7
    }

    public enum ShowCommands : int
    {
        SW_HIDE = 0,
        SW_SHOWNORMAL = 1,
        SW_NORMAL = 1,
        SW_SHOWMINIMIZED = 2,
        SW_SHOWMAXIMIZED = 3,
        SW_MAXIMIZE = 3,
        SW_SHOWNOACTIVATE = 4,
        SW_SHOW = 5,
        SW_MINIMIZE = 6,
        SW_SHOWMINNOACTIVE = 7,
        SW_SHOWNA = 8,
        SW_RESTORE = 9,
        SW_SHOWDEFAULT = 10,
        SW_FORCEMINIMIZE = 11,
        SW_MAX = 11
    }

}

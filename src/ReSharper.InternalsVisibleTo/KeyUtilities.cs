using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace ReSharper.InternalsVisibleTo
{
    /// <summary>
    /// This code was taken from an answer by Simon Mourier on StackOverflow
    /// https://stackoverflow.com/questions/16658541/enumerating-container-names-of-the-strong-name-csp/
    /// </summary>
    public static class KeyUtilities
    {
        public static IList<string> EnumerateKeyContainers(string provider)
        {
            ProvHandle prov;
            if (!CryptAcquireContext(out prov, null, provider, PROV_RSA_FULL, CRYPT_MACHINE_KEYSET | CRYPT_VERIFYCONTEXT))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            List<string> list = new List<string>();
            IntPtr data = IntPtr.Zero;
            try
            {
                int flag = CRYPT_FIRST;
                int len = 0;
                if (!CryptGetProvParam(prov, PP_ENUMCONTAINERS, IntPtr.Zero, ref len, flag))
                {
                    if (Marshal.GetLastWin32Error() != ERROR_MORE_DATA)
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                data = Marshal.AllocHGlobal(len);
                do
                {
                    if (!CryptGetProvParam(prov, PP_ENUMCONTAINERS, data, ref len, flag))
                    {
                        if (Marshal.GetLastWin32Error() == ERROR_NO_MORE_ITEMS)
                            break;

                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    list.Add(Marshal.PtrToStringAnsi(data));
                    flag = CRYPT_NEXT;
                }
                while (true);
            }
            finally
            {
                if (data != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(data);
                }

                prov.Dispose();
            }
            return list;
        }

        private sealed class ProvHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public ProvHandle()
                : base(true)
            {
            }

            protected override bool ReleaseHandle()
            {
                return CryptReleaseContext(handle, 0);
            }

            [DllImport("advapi32.dll")]
            private static extern bool CryptReleaseContext(IntPtr hProv, int dwFlags);

        }

        const int PP_ENUMCONTAINERS = 2;
        const int PROV_RSA_FULL = 1;
        const int ERROR_MORE_DATA = 234;
        const int ERROR_NO_MORE_ITEMS = 259;
        const int CRYPT_FIRST = 1;
        const int CRYPT_NEXT = 2;
        const int CRYPT_MACHINE_KEYSET = 0x20;
        const int CRYPT_VERIFYCONTEXT = unchecked((int)0xF0000000);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CryptAcquireContext(out ProvHandle phProv, string pszContainer, string pszProvider, int dwProvType, int dwFlags);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool CryptGetProvParam(ProvHandle hProv, int dwParam, IntPtr pbData, ref int pdwDataLen, int dwFlags);
    }
}
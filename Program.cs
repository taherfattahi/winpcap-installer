using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace WinpCapInstaller
{
    class Program
    {

        static string DLL_PACKET_NT4 = "Packet_nt4_x86.dll";
        static string DLL_PACKET_NT5_x86 = "Packet_nt5_x86.dll";
        static string DLL_PACKET_NT5_x64 = "Packet_nt5_x64.dll";
        static string DLL_PACKET_Vista_x86 = "Packet_Vista_x86.dll";
        static string DLL_PACKET_Vista_x64 = "Packet_Vista_x64.dll";
        static string DLL_PACKET = "Packet.dll";
        static string DLL_WPCAP_x86 = "wpcap_x86.dll";
        static string DLL_WPCAP_x64 = "wpcap_x64.dll";
        static string DLL_WPCAP = "wpcap.dll";
        static string DLL_PTHREAD_VC = "pthreadVC.dll";
        static string DRIVER_NPF_NT4 = "drivers\\npf_nt4_x86.sys";
        static string DRIVER_NPF_NT5_NT6_x86 = "drivers\\npf_nt5_nt6_x86.sys";
        static string DRIVER_NPF_NT5_NT6_x64 = "drivers\\npf_nt5_nt6_x64.sys";
        static string DRIVER_NPF = "npf.sys";
        static string DLL_DST_PATH = "C:\\windows\\System32";
        static string DLL_DST_PATH_x86_64 = "C:\\Windows\\sysWOW64";
        static string DRIVER_DST_PATH = "C:\\Windows\\System32\\Drivers";

        static void Main(string[] args)
        {
            isInstalledWinPcap();
            //installWinpCap();
            //uninstallWinpCap();

        }

        public static bool isInstalledWinPcap()
        {
            bool result = File.Exists(Path.Combine(DLL_DST_PATH, DLL_PTHREAD_VC));
            result = result && File.Exists(Path.Combine(DLL_DST_PATH, DLL_WPCAP));
            result = result && File.Exists(Path.Combine(DLL_DST_PATH, DLL_PACKET));
            result = result && File.Exists(Path.Combine(DRIVER_DST_PATH, DRIVER_NPF));

            if (Environment.Is64BitOperatingSystem)
            {
                result = result && File.Exists(Path.Combine(DLL_DST_PATH_x86_64, DLL_WPCAP));
                result = result && File.Exists(Path.Combine(DLL_DST_PATH_x86_64, DLL_PACKET));
            }

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C sc query npf",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            List<string> outputProcessList = new List<string>();
            while (!proc.StandardOutput.EndOfStream)
            {
                outputProcessList.Add(proc.StandardOutput.ReadLine());
            }

            for (int i = 0; i < outputProcessList.Count; i++)
            {
                if (outputProcessList[i].Contains("STATE") && outputProcessList[i].Contains("RUNNING"))
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }

            return result;
        }

        public static void installWinpCap()
        {
            string winpcap_path = "WinPcap";
            string pthread_dll = Path.Combine(winpcap_path, DLL_PTHREAD_VC);
            string wpcap_dll_x86 = Path.Combine(winpcap_path, DLL_WPCAP_x86);
            string wpcap_dll_x64 = "";

            if (Environment.Is64BitOperatingSystem)
            {
                wpcap_dll_x64 = Path.Combine(winpcap_path, DLL_WPCAP_x64);
            }

            int version = System.Environment.OSVersion.Version.Major;
            string packet_dll_x86 = "";
            string packet_dll_x64 = "";
            string driver_sys = "";
            if (version < 5)
            {
                // NT 4.0 Windows or older
                packet_dll_x86 = Path.Combine(winpcap_path, DLL_PACKET_NT4);
                packet_dll_x64 = "";
                driver_sys = Path.Combine(winpcap_path, DRIVER_NPF_NT4);
            }
            else if (version < 6)
            {
                // NT 5.0 Windows or older
                packet_dll_x86 = Path.Combine(winpcap_path, DLL_PACKET_NT5_x86);
                if (Environment.Is64BitOperatingSystem)
                {
                    packet_dll_x64 = Path.Combine(winpcap_path, DLL_PACKET_NT5_x64);
                    driver_sys = Path.Combine(winpcap_path, DRIVER_NPF_NT5_NT6_x64);
                }
                else
                {
                    packet_dll_x64 = "";
                    driver_sys = Path.Combine(winpcap_path, DRIVER_NPF_NT5_NT6_x86);
                }
            }
            else
            {
                // NT 6.0 Windows or newer
                packet_dll_x86 = Path.Combine(winpcap_path, DLL_PACKET_Vista_x86);
                if (Environment.Is64BitOperatingSystem)
                {
                    packet_dll_x64 = Path.Combine(winpcap_path, DLL_PACKET_Vista_x64);
                    driver_sys = Path.Combine(winpcap_path, DRIVER_NPF_NT5_NT6_x64);
                }
                else
                {
                    packet_dll_x64 = "";
                    driver_sys = Path.Combine(winpcap_path, DRIVER_NPF_NT5_NT6_x86);
                }
            }

            try
            {
                File.Copy(pthread_dll, Path.Combine(DLL_DST_PATH, DLL_PTHREAD_VC), true);

                if (wpcap_dll_x64 == "")
                {
                    File.Copy(wpcap_dll_x86, Path.Combine(DLL_DST_PATH, DLL_WPCAP), true);
                }
                else
                {
                    File.Copy(wpcap_dll_x86, Path.Combine(DLL_DST_PATH_x86_64, DLL_WPCAP), true);
                    File.Copy(wpcap_dll_x64, Path.Combine(DLL_DST_PATH, DLL_WPCAP), true);
                }

                if (packet_dll_x64 == "")
                {
                    File.Copy(packet_dll_x86, Path.Combine(DLL_DST_PATH, DLL_PACKET), true);
                }
                else
                {
                    File.Copy(packet_dll_x86, Path.Combine(DLL_DST_PATH_x86_64, DLL_PACKET), true);
                    File.Copy(packet_dll_x64, Path.Combine(DLL_DST_PATH, DLL_PACKET), true);
                }

                File.Copy(driver_sys, Path.Combine(DRIVER_DST_PATH, DRIVER_NPF), true);

            }
            catch (IOException iox)
            {
            }

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C sc create npf binPath=system32\\drivers\\npf.sys type=kernel start=auto error=normal tag=no DisplayName=\"NetGroup Packet Filter Driver\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            //while (!proc.StandardOutput.EndOfStream)
            //{
            //    var a = proc.StandardOutput.ReadLine();
            //}
            var proc1 = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C sc start npf",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc1.Start();
            //while (!proc1.StandardOutput.EndOfStream)
            //{
            //    var a = proc1.StandardOutput.ReadLine();
            //}
            //Process.Start("sc create npf binPath=system32\\drivers\\npf.sys type=kernel start=auto error=normal tag=no DisplayName=\"NetGroup Packet Filter Driver\"");
            //Process.Start("sc start npf");
        }

        public static void uninstallWinpCap()
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C sc stop npf",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();

            var proc1 = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C sc delete npf",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc1.Start();

            if (File.Exists(Path.Combine(DLL_DST_PATH, DLL_PTHREAD_VC)))
                File.Delete(Path.Combine(DLL_DST_PATH, DLL_PTHREAD_VC));

            if (File.Exists(Path.Combine(DLL_DST_PATH, DLL_WPCAP)))
                File.Delete(Path.Combine(DLL_DST_PATH, DLL_WPCAP));

            if (File.Exists(Path.Combine(DLL_DST_PATH, DLL_PACKET)))
                File.Delete(Path.Combine(DLL_DST_PATH, DLL_PACKET));

            if (File.Exists(Path.Combine(DRIVER_DST_PATH, DRIVER_NPF)))
                File.Delete(Path.Combine(DRIVER_DST_PATH, DRIVER_NPF));

            if (Environment.Is64BitOperatingSystem)
                if (File.Exists(Path.Combine(DLL_DST_PATH_x86_64, DLL_WPCAP)))
                    File.Delete(Path.Combine(DLL_DST_PATH_x86_64, DLL_WPCAP));

            if (File.Exists(Path.Combine(DLL_DST_PATH_x86_64, DLL_PACKET)))
                File.Delete(Path.Combine(DLL_DST_PATH_x86_64, DLL_PACKET));
        }

    }
}

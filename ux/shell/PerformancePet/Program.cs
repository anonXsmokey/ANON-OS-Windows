using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ANON.PerformancePet;

internal static class Program
{
    private const string LogFile = @"C:\ProgramData\ANON\Logs\performance-pet.log";

    [STAThread]
    private static void Main()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(LogFile)!);
        using var mutex = new Mutex(true, "ANON.PerformancePet", out var first);
        if (!first) return;

        ApplicationConfiguration.Initialize();
        using var app = new PerformancePetApplication();
        Application.Run(app);
    }

    private sealed class PerformancePetApplication : ApplicationContext
    {
        private readonly NotifyIcon _tray;
        private readonly System.Windows.Forms.Timer _timer;
        private readonly CpuSampler _cpu = new();
        private readonly MemorySampler _memory = new();
        private int _frame;
        private bool _reducedMotion;

        public PerformancePetApplication()
        {
            _tray = new NotifyIcon
            {
                Visible = true,
                Text = "ANON Performance Pet",
                Icon = PetIcon.Create(0, 0, 0, false)
            };

            var menu = new ContextMenuStrip();
            menu.Items.Add("ANON Performance Pet", null, (_, _) => ShowStatus());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Open Task Manager", null, (_, _) => OpenTaskManager());
            menu.Items.Add("Toggle Reduced Motion", null, (_, _) =>
            {
                _reducedMotion = !_reducedMotion;
                WriteLog($"Reduced motion: {_reducedMotion}");
                UpdateIcon();
            });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Exit Pet", null, (_, _) => ExitThread());
            _tray.ContextMenuStrip = menu;
            _tray.DoubleClick += (_, _) => OpenTaskManager();

            _timer = new System.Windows.Forms.Timer { Interval = 180 };
            _timer.Tick += (_, _) => UpdateIcon();
            _timer.Start();

            Application.ApplicationExit += (_, _) =>
            {
                _timer.Stop();
                _tray.Visible = false;
                _tray.Icon?.Dispose();
                _tray.Dispose();
            };

            WriteLog("ANON Performance Pet started");
        }

        private void UpdateIcon()
        {
            var cpu = _cpu.ReadPercent();
            var memory = _memory.ReadPercent();
            _frame++;
            var motion = _reducedMotion ? 0 : _frame;
            var gaming = File.Exists(@"C:\ProgramData\ANON\gaming-mode.flag");

            var next = PetIcon.Create(cpu, memory, motion, gaming);
            var previous = _tray.Icon;
            _tray.Icon = next;
            previous?.Dispose();
            _tray.Text = $"ANON Pet  •  CPU {cpu:0}%  •  RAM {memory:0}%";
        }

        private void ShowStatus()
        {
            var cpu = _cpu.ReadPercent();
            var memory = _memory.ReadPercent();
            MessageBox.Show(
                $"ANON PERFORMANCE PET\n\nCPU  {cpu:0}%\nRAM  {memory:0}%\n\nThe pet accelerates with system load.",
                "ANON Performance",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private static void OpenTaskManager()
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "taskmgr.exe",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                WriteLog($"Task Manager launch failed: {ex.Message}");
            }
        }

        protected override void ExitThreadCore()
        {
            WriteLog("ANON Performance Pet stopped");
            base.ExitThreadCore();
        }
    }

    private static void WriteLog(string message)
    {
        try { File.AppendAllText(LogFile, $"{DateTime.Now:O} {message}{Environment.NewLine}"); }
        catch { }
    }

    private sealed class CpuSampler
    {
        private long _lastIdle;
        private long _lastKernel;
        private long _lastUser;

        public double ReadPercent()
        {
            if (!GetSystemTimes(out var idle, out var kernel, out var user)) return 0;

            var idleValue = ToLong(idle);
            var kernelValue = ToLong(kernel);
            var userValue = ToLong(user);
            var idleDelta = idleValue - _lastIdle;
            var totalDelta = (kernelValue - _lastKernel) + (userValue - _lastUser);

            _lastIdle = idleValue;
            _lastKernel = kernelValue;
            _lastUser = userValue;

            if (totalDelta <= 0) return 0;
            return Math.Clamp((1.0 - idleDelta / (double)totalDelta) * 100.0, 0, 100);
        }

        private static long ToLong(FILETIME value) =>
            ((long)value.dwHighDateTime << 32) | value.dwLowDateTime;

        [StructLayout(LayoutKind.Sequential)]
        private struct FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }

        [DllImport("kernel32.dll")]
        private static extern bool GetSystemTimes(out FILETIME idle, out FILETIME kernel, out FILETIME user);
    }

    private sealed class MemorySampler
    {
        [DllImport("kernel32.dll")]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX status);

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        public double ReadPercent()
        {
            var status = new MEMORYSTATUSEX { dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>() };
            return GlobalMemoryStatusEx(ref status) ? status.dwMemoryLoad : 0;
        }
    }

    private static class PetIcon
    {
        public static Icon Create(double cpu, double memory, int frame, bool gaming)
        {
            using var bitmap = new Bitmap(32, 32);
            using var g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            var accent = gaming
                ? Color.FromArgb(255, 255, 72, 96)
                : Color.FromArgb(255, 80, 210, 255);
            var bob = (frame % 4) - 1;

            using var body = new SolidBrush(Color.FromArgb(240, 15, 18, 27));
            using var line = new Pen(accent, 2.2f);
            using var eye = new SolidBrush(accent);

            g.FillEllipse(body, 7, 10 + bob, 18, 15);
            g.FillPolygon(body, new[]
            {
                new Point(8, 13 + bob), new Point(8, 4 + bob), new Point(14, 10 + bob)
            });
            g.FillPolygon(body, new[]
            {
                new Point(24, 13 + bob), new Point(24, 4 + bob), new Point(18, 10 + bob)
            });
            g.DrawArc(line, 5, 8 + bob, 22, 20, 200, 140);
            g.FillEllipse(eye, 12, 15 + bob, 2, 2);
            g.FillEllipse(eye, 19, 15 + bob, 2, 2);

            var speed = (int)Math.Clamp(cpu / 12, 0, 6);
            for (var i = 0; i < speed; i++)
            {
                var x = 3 - i;
                g.DrawLine(line, x, 25 + (i % 2) * 2, x + 4, 25 + (i % 2) * 2);
            }

            var handle = bitmap.GetHicon();
            try
            {
                using var source = Icon.FromHandle(handle);
                return (Icon)source.Clone();
            }
            finally
            {
                DestroyIcon(handle);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr handle);
    }
}

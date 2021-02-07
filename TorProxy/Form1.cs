using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace TorProxy
{
    public partial class Form1 : Form
    {
        private readonly Job processJob;
        private Process hostProcess;
        private readonly RegistryKey settingsKey;

        public Form1()
        {
            InitializeComponent();

            processJob = new Job();
            settingsKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings", true);
        }

        private void connectBtn_Click(object sender, EventArgs e)
        {
            connectBtn.Enabled = false;

            outTxt.AppendText("Изменение системных параметров..." + Environment.NewLine);

            settingsKey.SetValue("ProxyEnable", 1, RegistryValueKind.DWord);
            settingsKey.SetValue("ProxyServer", "socks=127.0.0.1:9050", RegistryValueKind.String);

            outTxt.AppendText("Подключение к Tor прокси..." + Environment.NewLine);

            hostProcess = new Process();
            hostProcess.StartInfo = new ProcessStartInfo(@"Tor\tor.exe", @"-f Data\Tor\torrc")
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Normal,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            hostProcess.OutputDataReceived += HostProcess_OutputDataReceived;
            hostProcess.Start();
            hostProcess.BeginOutputReadLine();

            processJob.AddProcess(hostProcess.Handle);

            disconnectBtn.Enabled = true;
        }

        private void disconnectBtn_Click(object sender, EventArgs e)
        {
            disconnectBtn.Enabled = false;

            hostProcess.Kill();
            outTxt.AppendText("Подключение разорвано!" + Environment.NewLine);

            outTxt.AppendText("Восстановление системных параметров..." + Environment.NewLine);

            settingsKey.SetValue("ProxyEnable", 0, RegistryValueKind.DWord);

            outTxt.AppendText("Системные параметры восстановлены" + Environment.NewLine);

            connectBtn.Enabled = true;
        }

        private void HostProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (outTxt.InvokeRequired)
            {
                Action log = () => LogText(e.Data);
                outTxt.Invoke(log);
            }
            else
            {
                LogText(e.Data);
            }
        }

        private void LogText(string text)
        {
            outTxt.AppendText(text + Environment.NewLine);

            if (text?.Contains("Bootstrapped 100% (done): Done") ?? false)
            {
                outTxt.AppendText("Подключение установлено!" + Environment.NewLine);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (disconnectBtn.Enabled)
            {
                disconnectBtn_Click(sender, e);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (connectBtn.Enabled)
            {
                connectBtn_Click(sender, e);
            }
        }
    }
}

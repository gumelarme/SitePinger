using System;
using System.Data;
using System.Linq;
using System.IO;
using System.Windows.Forms;

namespace SitePInger
{
    public partial class Form1 : Form
    {
        PingMachine pinger;
        private string[] defaultSite = new string[]
        {
            "vultr.com",
            "digitalocean.com",
            "bluehost.com",
            "domainesia.com",
            "hostinger.com",
            "45.127.133.52",
            "207.246.107.236",
            "45.32.100.168", //Singapore
            "108.61.201.151", //Tokyo
            "108.61.212.117", //Sydney
            "149.248.50.81", //Toronto
            "104.156.230.107", //Silicon Valley
        };

        public Form1()
        {
            InitializeComponent();
            pinger = new PingMachine();
            pinger.OnResponseReceived += Pinger_OnResponseReceived;
            pinger.OnProgressUpdated += Pinger_OnProgressUpdated;
        }

        private void Pinger_OnProgressUpdated(int current, int goal)
        {
            this.Invoke(new Action(() =>
            {
                progressBar.Value = current;
                progressBar.Maximum = goal;
            }));
        }

        private void Pinger_OnResponseReceived(string response)
        {
            this.Invoke(new Action(() =>
            {
                txtResponse.Text += response + Environment.NewLine;
                txtResponse.SelectionStart = txtResponse.Text.Length;
                txtResponse.ScrollToCaret();
            }));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtIPAddress.Text = string.Join(Environment.NewLine, defaultSite);
            this.Icon = ResourceIcon.pingpong_vW5_icon;
        }

        private void BtnStartPing_Click(object sender, EventArgs e)
        {
            txtResponse.Text = string.Empty;
            pinger.Start(txtIPAddress.Text
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(x => x.Trim())
                .Where(x => x != string.Empty)
                .ToArray()
                );
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            using(var save = new SaveFileDialog())
            {
                save.AddExtension = true;
                save.Filter = "Text Files (*.txt)|txt";
                save.DefaultExt = "txt";
                if (save.ShowDialog() != DialogResult.OK) return;
                File.WriteAllText(save.FileName, txtResponse.Text);
            }
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}

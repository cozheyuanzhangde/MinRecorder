using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio;
using NAudio.Wave;
using System.Runtime.InteropServices;

namespace MinRecorder
{
    public partial class Form1 : Form
    {
        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        public Form1()
        {
            InitializeComponent();
        }

        public enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117
        }


        private float getScalingFactor()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();
            int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);

            float ScreenScalingFactor = (float)PhysicalScreenHeight / (float)LogicalScreenHeight;

            return ScreenScalingFactor;
        }

        bool folderSelected = false;
        string outputPath = "";

        //Wave Format for silent wave of output
        WaveFormat wavefmt = new WaveFormat(44100, 16, 2);

        ScreenRecorder screenRec = new ScreenRecorder(new Rectangle(), "");

        private void Form1_Load(object sender, EventArgs e)
        {
            MinimizeBox = false;
            MaximizeBox = false;
            Rectangle bounds = Screen.FromControl(this).Bounds;
            double zoomFactor = getScalingFactor();
            bounds.Height = Convert.ToInt32(bounds.Height * zoomFactor);
            bounds.Width = Convert.ToInt32(bounds.Width * zoomFactor);
            label1.Text = bounds.Width.ToString() + " " + bounds.Height.ToString();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            screenRec.Stop();
            Application.Restart();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "Select an Output Folder";

            if(folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                outputPath = folderBrowser.SelectedPath;
                folderSelected = true;

                Rectangle bounds = Screen.FromControl(this).Bounds;
                double zoomFactor = getScalingFactor();
                bounds.Height = Convert.ToInt32(bounds.Height * zoomFactor);
                bounds.Width = Convert.ToInt32(bounds.Width * zoomFactor);
                screenRec = new ScreenRecorder(bounds, outputPath);

            }
            else
            {
                MessageBox.Show("Please select a folder", "Error");
            }
        }

        private void tmrRecord_Tick(object sender, EventArgs e)
        {
            screenRec.RecordAudio();
            screenRec.RecordVideo();

            timeDisplay.Text = screenRec.GetElapsed() + "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(folderSelected == true)
            {
                timer1.Start();
                var silence = new SilenceProvider(wavefmt).ToSampleProvider();
                WaveOut player = new WaveOut();
                player.Init(silence);
                player.Play();
            }
            else
            {
                MessageBox.Show("You must select an output folder before recording", "Error");
            }
        }
    }
}

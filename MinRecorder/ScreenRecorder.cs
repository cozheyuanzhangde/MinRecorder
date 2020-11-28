using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using Accord.Video.FFMPEG;

namespace MinRecorder
{
    class ScreenRecorder
    {
        //Video variables:
        private Rectangle bounds;          //bounds of the screen
        private string outputPath = "";    //final video location/path
        private string tempPath = "";      //temp location/path to store screenshots
        private int fileCount = 1;
        private List<string> inputImageSequence = new List<string>();

        //File variables:
        private string audioName = "mic.wav";
        private string videoName = "video.mp4";
        private string finalName = "FinalVideo.mp4";

        //Time Variable:
        Stopwatch watch = new Stopwatch();

        //Audio Variables:
        public static class NativeMethods     //import dll
        {
            [DllImport("winmm.dll", EntryPoint = "mciSendStringA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
            public static extern int record(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);
        }

        public ScreenRecorder(Rectangle b, string outPath)
        {
            CreateTempFolder("tempScreenshots");

            bounds = b;
            outputPath = outPath;
        }

        private void CreateTempFolder(string name)
        {
            if(Directory.Exists("D://"))
            {
                string pathName = $"D://{name}";
                Directory.CreateDirectory(pathName);
                tempPath = pathName;
            } else
            {
                string pathName = $"C://{name}";
                Directory.CreateDirectory(pathName);
                tempPath = pathName;
            }
        }

        private void DeletePath(string targetDir)
        {
            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            foreach(string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach(string dir in dirs)
            {
                DeletePath(dir);
            }

            Directory.Delete(targetDir, false);
        }

        private void DeleteFileExcept(string targetFile, string excFile)
        {
            string[] files = Directory.GetFiles(targetFile);

            foreach(string file in files)
            {
                if(file != excFile){
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
            }
        }

        public void CleanUp()
        {
            if (Directory.Exists(tempPath))
            {
                DeletePath(tempPath);
            }
        }

        public string GetElapsed()
        {
            return string.Format("{0:D2}:{1:D2}:{2:D2}", watch.Elapsed.Hours, watch.Elapsed.Minutes, watch.Elapsed.Seconds);
        }

        public void RecordVideo()
        {
            watch.Start();

            using(Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using(Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size); //create screenshots
                }
                string name = tempPath + "//screenshot-" + fileCount + ".png";
                bitmap.Save(name, ImageFormat.Png);
                inputImageSequence.Add(name);
                fileCount++;

                bitmap.Dispose();
            }
        }

        public void RecordAudio()
        {
            NativeMethods.record("open new Type waveaudio Alias recsound", "", 0, 0);
            NativeMethods.record("record recsound", "", 0, 0);
        }

        private void SaveAudio()
        {
            string audioPath = "save recsound " + outputPath + "//" + audioName;
            NativeMethods.record(audioPath, "", 0, 0);
            NativeMethods.record("close recsound", "", 0, 0);
        }

        private void SaveVideo(int width, int height, int frameRate)
        {
            using (VideoFileWriter vFwriter = new VideoFileWriter())
            {
                vFwriter.Open(outputPath + "//" + videoName, width, height, frameRate, VideoCodec.MPEG4);

                foreach(string imageLoc in inputImageSequence)
                {
                    Bitmap imageFrame = System.Drawing.Image.FromFile(imageLoc) as Bitmap;
                    vFwriter.WriteVideoFrame(imageFrame);
                    imageFrame.Dispose();
                }

                vFwriter.Close();
            }
        }


        private void CombineVideoAndAudio(string video, string audio)
        {
            string command = $"/C ffmpeg -i \"{video}\" -i \"{audio}\" -shortest {finalName}";      //"/k" to NOT see command line prompt
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                FileName = "cmd.exe",
                WorkingDirectory = outputPath,
                Arguments = command
            };

            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }
        }

        public void Stop()
        {
            watch.Stop();

            int width = bounds.Width;
            int height = bounds.Height;
            int frameRate = 10;

            SaveAudio();

            SaveVideo(width, height, frameRate);

            CombineVideoAndAudio(videoName, audioName);

            DeletePath(tempPath);


        }
    }
}

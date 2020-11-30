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
using NAudio.Wave;


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

        //Audio setup:
        private LoopbackRecorder sysRecorder = new LoopbackRecorder();

        //File variables:
        private string audioName = "mic.wav";
        private string videoName = "video.mp4";
        private string finalName = "FinalVideo.mp4";

        //Time Variable:
        Stopwatch watch = new Stopwatch();

        public ScreenRecorder(Rectangle b, string outPath)
        {
            CreateTempFolder("m1nr3c0rd3t3mpf01d3r");

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
            // Define the output wav file of the recorded audio
            string outputFilePath = @"C:\Users\BrianZ\Desktop\testFiles\mic.wav";
            sysRecorder.StartRecording(outputFilePath);
         
        }

        private void SaveAudio()
        {
            sysRecorder.StopRecording();
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

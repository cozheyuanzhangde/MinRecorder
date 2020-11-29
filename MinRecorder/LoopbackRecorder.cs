using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace MinRecorder
{
    class LoopbackRecorder
    {
        private IWaveIn _waveIn;
        private WaveFileWriter _writer;
        private bool _isRecording = false;

        //constructor
        public LoopbackRecorder()
        {
        }

        public void StartRecording(string fileName)
        {
            // If we are currently recording, exit function.
            if (_isRecording == true)
            {
                return;
            }
            _fileName = fileName;
            _waveIn = new WasapiLoopbackCapture();
            _writer = new WaveFileWriter(fileName, _waveIn.WaveFormat);
            _waveIn.DataAvailable += OnDataAvailable;
            _waveIn.RecordingStopped += OnRecordingStopped;
            _waveIn.StartRecording();
            _isRecording = true;
        }

        public void StopRecording()
        {
            if (_waveIn == null)
            {
                return;
            }
            _waveIn.StopRecording();
        }

        void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            // Writer Close() needs to come first otherwise NAudio will lock up.
            if (_writer != null)
            {
                _writer.Close();
                _writer = null;
            }
            if (_waveIn != null)
            {
                _waveIn.Dispose();
                _waveIn = null;
            }
            _isRecording = false;
            if (e.Exception != null)
            {
                throw e.Exception;
            }
        } // end void OnRecordingStopped

        void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            _writer.Write(e.Buffer, 0, e.BytesRecorded);
            //int secondsRecorded = (int)(_writer.Length / _writer.WaveFormat.AverageBytesPerSecond);
        }

        private string _fileName = "";
        public string FileName
        {
            get
            {
                return _fileName;
            }
        }
    }
}

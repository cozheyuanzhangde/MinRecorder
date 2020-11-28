using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

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

    }
}

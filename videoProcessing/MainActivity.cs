using Android.App;
using Android.Widget;
using Android.OS;

using OpenCV;
using OpenCV.Core;

using System;
using System.Threading;
using System.Timers;

namespace videoProcessing
{

    [Activity(Label = "videoProcessing", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private OpenCV.VideoIO.VideoCapture _video = null;

        private Mat _firstFrame = new Mat();
        private Mat _background = new Mat();
        private Mat _newFrame = new Mat();
        private Mat _grayFrame = new Mat();
        private Mat _diffFrame = new Mat();
        private Mat _thrFrame = new Mat();


        private string filename = "Assets://Some_Video_Mp4";

        private double frameNumber;
        private int _frameCount = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            
            try
            {
                //Open video capture object
                _video = new OpenCV.VideoIO.VideoCapture(filename);
                frameNumber = _video.Get(5);
            }
            catch (NullReferenceException)
            {
                //Put somthing here to handle video not being found
            }

            Button _startButton = FindViewById<Button>(Resource.Id.startButt);
            System.Timers.Timer _vcTimer = new System.Timers.Timer(20);
            
            //press start button to begin frame processing
            _startButton.Click += delegate
            {
                if(_video != null)
                {
                    if(_video.Grab())
                    {
                        if(_video != null)
                        {
                            _video.Set(1, OpenCV.VideoIO.Videoio.CapPropPosFrames);
                            //grab original frame
                            _video.Retrieve(_background, 0);

                            //convert first frame to grayscale
                            OpenCV.ImgProc.Imgproc.CvtColor(_background, _firstFrame, OpenCV.ImgProc.Imgproc.ColorBgr2gray);
                            
                            _vcTimer.Elapsed += ProcessFrame;
                            _vcTimer.Enabled = true;
                        }
                    }
                }
            };
        }

        private void ProcessFrame(Object sender, System.Timers.ElapsedEventArgs e)
        {
            if(_video.Grab())
            {
                _frameCount = _frameCount + 1;
                if(_video != null)
                {
                    //Get the second frame
                    _video.Retrieve(_newFrame, 0);

                    //Convert second frame to grayscale 
                    OpenCV.ImgProc.Imgproc.CvtColor(_newFrame, _grayFrame, OpenCV.ImgProc.Imgproc.ColorBgr2gray);

                    //perform frame differencing
                    OpenCV.Core.Core.Absdiff(_firstFrame, _grayFrame, _diffFrame);

                    //threshold intensity image at a given sensitivity value (20)
                    OpenCV.ImgProc.Imgproc.Threshold(_diffFrame, _thrFrame, 20, 255, OpenCV.ImgProc.Imgproc.ThreshBinary);

                    //bilateral filter to get edges
                    OpenCV.ImgProc.Imgproc.BilateralFilter(_thrFrame, _thrFrame, 11, 17, 17);

                    //erode and dialate to get rid of noise
                    OpenCV.ImgProc.Imgproc.Erode(_thrFrame, _thrFrame, null, new Point(-1, -1), 2, OpenCV.Core.Core.BorderConstant, OpenCV.Core.morp);


                    
                }
            }
        }
    }
}


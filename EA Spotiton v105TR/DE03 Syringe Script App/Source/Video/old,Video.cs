using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace Aurigin
{

    // This class contains methods that allow us to load the 
    // uEye cameras, display the live video, adjust the settings
    // read the camera settings, record and save avis
    // 
    public class Video
    {
        private uEye.Camera         SideCamera, BackCamera;
        private uEye.Tools.Video    VideoRecording;
        IntPtr displayHandleSideCamera = IntPtr.Zero;
        IntPtr displayHandleBackCamera = IntPtr.Zero;
        //       String SideCameraParameterFilepath = "TipInspCameraSN003_Trapezoid.ini";
        //       String BackCameraParameterFilepath = "GridCameraSN173.ini";
        String SideCameraParameterFilepath = "3130,TipCam.ini";           //PKv5.4.0,   PKv5.5.0
        String BackCameraParameterFilepath = "3130,GridCam.ini";

        //  private double                  constantMinFPS = 1.00;
        // private double                  constantMaxFPS = 63.00;
        // private double                  constantPixelSpeed = 28.00;
        private string                  datedVideoFolder=null;
        private int                     currentVideoNumber = -1;
        private string                  lastVideoFileName = null;           // PKv5.5.0.5
      
        int currentCamera = 1; //0 - error, 1 - back camera, 2 - side camera (Tip Cam)

        int SequenceCount = 0;

        //private uEye.Info.Camera info_camera;
        uEye.Defines.Status statusRet = 0;

        private static int stVideoTime=0; //  System.Environment.TickCount for timing the video,   do these really need to be static ??
        private static int videoID = 0;   //  For using uEye.Tools

        // PKv5.5.0  
        private bool isLive = true;     //    Keep track of whether live video is turned on
        private bool isDroplet = false; //   Outline found droplet

        private int sideCameraFrameCountGrayScale = 0;     // TODO when i want to start adding in frame acquisition for subsequent image processing.    
        private int backCameraFrameCountGrayScale = 0;

        private int sideCameraFrameCountAll = 0;          
        private int backCameraFrameCountAll = 0;            



        public Video()
        {
          InitCamera();
        }

        private void InitCamera()
        {
            SideCamera = new uEye.Camera();
            BackCamera = new uEye.Camera();
            VideoRecording = new uEye.Tools.Video();


            // Open SideCamera,  Droplet inspection camera.
            statusRet = SideCamera.Init(2);                     
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Camera initializing failed, Tip Camera");
                Environment.Exit(-1);
            }

            // Load Camera Parameters
            statusRet = SideCamera.Parameter.Load(SideCameraParameterFilepath);
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Parameter Loading failed, Tip Camera");
            }
                
            statusRet = SideCamera.Memory.Allocate();
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Allocate Memory failed, Tip Camera");
                Environment.Exit(-1);
            }           

            SideCamera.IO.Flash.SetMode(uEye.Defines.IO.FlashMode.Off);     // PKv5.5.0  ,   Disable the free running strobe to start with.

            // Start Live Video
            statusRet = SideCamera.Acquisition.Capture();
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Start Live Video failed, Tip Camera");
            }
         
            // Open BackCamera,  grid
            statusRet = BackCamera.Init(1);                                
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Camera initializing failed, Grid Camera");
                Environment.Exit(-1);
            }
            statusRet = BackCamera.Parameter.Load(BackCameraParameterFilepath);
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Parameter Loading failed, Grid Camera");
            }
            statusRet = BackCamera.Memory.Allocate();
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Allocate Memory failed, Grid Camera");
                Environment.Exit(-1);
            }

            BackCamera.IO.Flash.SetMode(uEye.Defines.IO.FlashMode.Off);         // PKv5.5.0  Disable the free running strobe to start with.

            // Start Live Video
            statusRet = BackCamera.Acquisition.Capture();
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Start Live Video failed, Grid Camera");
            }
                       
            // Connect on events
            SideCamera.EventFrame += onFrameEventSide;
           // SideCamera.EventSequence += onSequenceEventSide;
            BackCamera.EventFrame += onFrameEventBack;

        }

        // since we have only one display window in our main gui
        // we need to be able to switch between the camera feeds
        public void setDisplayHandle(IntPtr InputDisplayHandle)
        {
            if (currentCamera == 1)
            {
                displayHandleBackCamera = InputDisplayHandle;
            }
            else if(currentCamera == 2)
            {
                displayHandleSideCamera = InputDisplayHandle;
            }
        }

        public void LoadCamParametersFromFile(string fileName)
        {

            if (currentCamera == 2)
            {
                //// Stop Live Video,  if it happens to be going...  Commented out for Spotiton
                //statusRet = SideCamera.Acquisition.Stop();

                //Int32[] memList;
                //SideCamera.Memory.GetList(out memList);
                //SideCamera.Memory.Free(memList);

                //// Load Camera Parameters
                statusRet = SideCamera.Parameter.Load(fileName);
                if (statusRet != uEye.Defines.Status.Success)
                {
                    MessageBox.Show("Video.cs:  Loading Camera Parameters failed");
                }

                //// allocate new standard memory           Commented out for Spotiton  PKv5.5.0.1
                //SideCamera.Memory.Allocate();

                //// Start live video,  if it was live before.
                //if (isLive)
                //{
                //    statusRet = SideCamera.Acquisition.Capture();
                //}
            }
            else    // Must be the back / grid camera.
            {
                // Stop Live Video,  if it happens to be going...   Commented out for Spotiton   PKv5.5.0.1
                //statusRet = BackCamera.Acquisition.Stop();

                //Int32[] memList;
                //BackCamera.Memory.GetList(out memList);
                //BackCamera.Memory.Free(memList);

                //// Load Camera Parameters
                statusRet = BackCamera.Parameter.Load(fileName);
                if (statusRet != uEye.Defines.Status.Success)
                {
                    MessageBox.Show("Video.cs:  Loading Camera Parameters failed, Second Camera");
                }

                // allocate new standard memory
                //BackCamera.Memory.Allocate();

                //// Start live video,  if it was live before.
                //if (isLive)
                //{
                //    statusRet = BackCamera.Acquisition.Capture();
                //}
            }
        }

        // filename = "",  will prompt the user for the filename to save.

        public void SaveCamParametersToFile(string fileName)
        {
            // Lets try the very simple version (dont stop acquisition)

            //// Save Camera Parameters
            if (currentCamera == 2)
            {
                statusRet = SideCamera.Parameter.Save(fileName);
                if (statusRet != uEye.Defines.Status.Success)
                {
                    MessageBox.Show("Video.cs:  Saving Camera Parameters failed (side Camera)");
                }
            }
            else
            {
                statusRet = BackCamera.Parameter.Save(fileName);
                if (statusRet != uEye.Defines.Status.Success)
                {
                    MessageBox.Show("Video.cs:  Saving Camera Parameters failed (side Camera)");
                }
            }
        }


        // swtiching the display handle from one camera to the other
        // display handle is generated by the PictureBox in the Main GUI Form
        public void setSwitchCamera()
        {
            if (currentCamera==1)
            {
                displayHandleSideCamera = displayHandleBackCamera;
                displayHandleBackCamera = IntPtr.Zero;
                currentCamera = 2;
            }
            else if (currentCamera==2)
            {
                displayHandleBackCamera = displayHandleSideCamera;
                displayHandleSideCamera = IntPtr.Zero;
                currentCamera = 1;
            }
        }

        // PKv5.5.0  Added a few different ways to refer to the same camera.
        // overloaded definition for setSwitchCamera, this definition switches to a specific 
        // camera.
        public void setSwitchCamera(string name)
        {
            if ((name == "Back")|(name == "Grid"))
            {
                if (currentCamera != 1)
                {
                    displayHandleBackCamera = displayHandleSideCamera;
                    displayHandleSideCamera = IntPtr.Zero;
                    currentCamera = 1;
                }
            }
            else if ((name == "Side")|(name == "Tip")|(name == "Droplet"))
            {
                if (currentCamera != 2)
                {
                    displayHandleSideCamera = displayHandleBackCamera;
                    displayHandleBackCamera = IntPtr.Zero;
                    currentCamera = 2;
                }
            }
            else { MessageBox.Show("No such camera"); }
        }

        public string getCurrentCameraName()
        {
            string output;
            if (currentCamera == 1) output = "Back";
            else { output = "Side"; }
            return output;
        }
  
        // this functions just returns a frame from the live feed of the Back camera
        public Bitmap GetCenterofTipOnFrame()
        {
            Bitmap Frame = null;
            Int32 s32MemIDMaster;
            
            BackCamera.Memory.GetActive(out s32MemIDMaster);
            BackCamera.Memory.CopyToBitmap(s32MemIDMaster, out Frame);
                      
            return Frame;
            
        }

        //public double getMinExposure()
        //{
        //    Int32 currentPixelClock = 0;
        //    double minExposure = 0;

        //    if (currentCamera == 1)
        //    {
        //        BackCamera.Timing.PixelClock.Get(out currentPixelClock);
        //    }
        //    else if (currentCamera == 2)
        //    {
        //        SideCamera.Timing.PixelClock.Get(out currentPixelClock);
        //    }
        //    minExposure = (constantPixelSpeed / currentPixelClock) / constantMinFPS;

        //    return minExposure;
        //}

        //public double getMaxExposure()
        //{
        //    Int32 currentPixelClock = 0;
        //    double maxExposure = 0;
            
        //    if (currentCamera == 1)
        //    {
        //        BackCamera.Timing.PixelClock.Get(out currentPixelClock);
        //    }
        //    else if (currentCamera == 2)
        //    {
        //        SideCamera.Timing.PixelClock.Get(out currentPixelClock);
        //    }
        //    maxExposure = (constantPixelSpeed / currentPixelClock) / constantMaxFPS;

        //    return maxExposure;
        //}

        //public double currentExposure()
        //{
        //    double currentExposure = 0;
        //    if (currentCamera == 1)
        //    {
        //        BackCamera.Timing.Exposure.Get(out currentExposure);
        //    }
        //    else if (currentCamera == 2)
        //    {
        //        SideCamera.Timing.Exposure.Get(out currentExposure);
        //    }

        //    return currentExposure;
        //}
        
        //public void setCameraExposure(double inputExposure)
        //{
        //    if (currentCamera == 1)
        //    {
        //        BackCamera.Timing.Exposure.Set(inputExposure);
        //    }
        //    else if (currentCamera == 2)
        //    {
        //        SideCamera.Timing.Exposure.Set(inputExposure);
        //    }
        //}
              
        // reads the values from the camera settings and returns the values to be used 
        // in setting up the parameters for PixelClock TrackBar
        public void getTrackBallValuePixelClock(out Int32 TrackBallValue, out Int32 minPixelClock, out Int32 maxPixelClock, out Int32 incPixelClock)
        {
            TrackBallValue = 0;
            minPixelClock = 0;
            maxPixelClock = 0;
            incPixelClock = 0;
            if (currentCamera == 1)
            {
                BackCamera.Timing.PixelClock.Get(out TrackBallValue);
                BackCamera.Timing.PixelClock.GetRange(out minPixelClock, out maxPixelClock, out incPixelClock);
            }
            else if (currentCamera == 2)
            {
                SideCamera.Timing.PixelClock.Get(out TrackBallValue);
                SideCamera.Timing.PixelClock.GetRange(out minPixelClock, out maxPixelClock, out incPixelClock);
            }
            
        }
       
        // takes the input value and sets in camera
        public void setPixelClock(Int32 inputPixelClock)
        {
            Int32 temp = -1;
            if (currentCamera == 1)
            {
                BackCamera.Timing.PixelClock.Set(inputPixelClock);
                BackCamera.Timing.PixelClock.Get(out temp);
            }
            else if (currentCamera == 2)
            {
                SideCamera.Timing.PixelClock.Set(inputPixelClock);
                SideCamera.Timing.PixelClock.Get(out temp);
            }   
        }

        // takes the input value and sets in camera
        public void setFPS(Int32 inputFPS)
        {
            double currentFPS,minFPS,maxFPS,incFPS;

            if (currentCamera == 1)
            {
                //read the range values from camera
                BackCamera.Timing.Framerate.GetFrameRateRange(out minFPS, out maxFPS, out incFPS);
                //calculate the FPS from trackbar value and range settings
                currentFPS = (inputFPS*incFPS*maxFPS)/(maxFPS-minFPS);
                // send FPS to camera
                BackCamera.Timing.Framerate.Set(currentFPS);
            }
            // same as above, different Camera
            else if (currentCamera == 2)
            {
                SideCamera.Timing.Framerate.GetFrameRateRange(out minFPS, out maxFPS, out incFPS);
                currentFPS = (inputFPS * incFPS * maxFPS) / (maxFPS - minFPS);
                SideCamera.Timing.Framerate.Set(currentFPS);
            }
        }

        // reads the FPS value from the respective cameras
        public double getFPS()
        {
            double output = 0;
            if (currentCamera == 1)
            {
                BackCamera.Timing.Framerate.Get(out output);
            }
            else if (currentCamera == 2)
            {
                SideCamera.Timing.Framerate.Get(out output);
            }
            return output;
        }

        // returns the values for FPS trackbar on the mainform GUI
        public void getTrackBallValueFPS(out double currentFPS, out double minFPS, out double maxFPS, out double incFPS,out Int32 currentFPS_trackvalue, out Int32 range_trackvalue)
        {
            currentFPS = minFPS = maxFPS = incFPS = 0;
            currentFPS_trackvalue = range_trackvalue = 0;
            if (currentCamera == 1)
            {
                // read the range settings from camera
                BackCamera.Timing.Framerate.GetFrameRateRange(out minFPS, out maxFPS, out incFPS);
                // read current FPS from camera
                BackCamera.Timing.Framerate.Get(out currentFPS);
                //calculate the trackbar range from camera information
                range_trackvalue =  Convert.ToInt32((maxFPS-minFPS)/incFPS);
                //calculate the trackbar value/position based on camera's current settings
                currentFPS_trackvalue = Convert.ToInt32((currentFPS-minFPS)/incFPS);
            }
                // same thing as above, just for different camera
            else if (currentCamera == 2)
            {
                SideCamera.Timing.Framerate.GetFrameRateRange(out minFPS, out maxFPS, out incFPS);
                SideCamera.Timing.Framerate.Get(out currentFPS);
                range_trackvalue = Convert.ToInt32((maxFPS - minFPS) / incFPS);
                currentFPS_trackvalue = Convert.ToInt32((currentFPS - minFPS) / incFPS);
            }
        }

        // read, calculate and return the values necessary to populate the trackbar on the mainform GUI
        public void getTrackBallValueExposure(out double currentExp, out double minExp, out double maxExp, out double incExp, out Int32 currentExp_trackvalue, out Int32 range_trackvalue)
        {
            currentExp = minExp = maxExp = incExp = 0;
            currentExp_trackvalue = range_trackvalue = 0;
            if (currentCamera == 1)
            {
                //read the range Exposure settings from the camera
                BackCamera.Timing.Exposure.GetRange(out minExp, out maxExp, out incExp);
                // read the current exposure
                BackCamera.Timing.Exposure.Get(out currentExp);
                // calculate the trackbar range from the camera's settings
                range_trackvalue = Convert.ToInt32((maxExp - minExp) / incExp);
                // calcualte the trackbar value/position from the camera's current settings
                currentExp_trackvalue = Convert.ToInt32((currentExp - minExp) / incExp);
            }
                // same as above, just different camera
            else if (currentCamera == 2)
            {
                SideCamera.Timing.Exposure.GetRange(out minExp, out maxExp, out incExp);
                SideCamera.Timing.Exposure.Get(out currentExp);
                range_trackvalue = Convert.ToInt32((maxExp - minExp) / incExp);
                currentExp_trackvalue = Convert.ToInt32((currentExp - minExp) / incExp);
            }
        }

        // take input exposure value from the Exposure trackbar on MainForm GUI and 
        // calculate the corresponding Exposure, send it to camera
        public void setExposure(Int32 inputExposure)
        {
            double currentExp, minExp, maxExp, incExp;
            if (currentCamera == 1)
            {
                // read the Exposure range from the camera
                BackCamera.Timing.Exposure.GetRange(out minExp, out maxExp, out incExp);
                // calculate the actual exposure based on the range and trackbar value
                currentExp = (inputExposure * incExp * maxExp) / (maxExp - minExp);
                // send the calculated exposure value to the camera
                BackCamera.Timing.Exposure.Set(currentExp);
            }
                // same as above, just different camera
            else if (currentCamera == 2)
            {
                SideCamera.Timing.Exposure.GetRange(out minExp, out maxExp, out incExp);
                currentExp = (inputExposure * incExp * maxExp) / (maxExp - minExp);
                SideCamera.Timing.Exposure.Set(currentExp);
            }
        }

        // read exposure value from the camera
        public double getExposure()
        {
            double output = 0;
            if (currentCamera == 1)
            {
                BackCamera.Timing.Exposure.Get(out output);
            }
            else if (currentCamera == 2)
            {
                SideCamera.Timing.Exposure.Get(out output);
            }
            return output;
        }

        public void setGain(int gain)
        {
            if (currentCamera == 2)
                SideCamera.Gain.Hardware.Scaled.SetMaster(gain);
            else
                BackCamera.Gain.Hardware.Scaled.SetMaster(gain);
        }

        public int getGain()
        {
            int gain = 0;

            if (currentCamera == 2)
                SideCamera.Gain.Hardware.Scaled.GetMaster(out gain);
            else
                BackCamera.Gain.Hardware.Scaled.GetMaster(out gain);

 //         Console.WriteLine("Reading Camera Gain Factor = {0}", gain);
            return gain;
        }

        private void onFrameEventSide(object sender, EventArgs e)
        {

            uEye.Camera SideCamera = sender as uEye.Camera;
            Int32 s32MemIDMaster;
            SideCamera.Memory.GetActive(out s32MemIDMaster);
            SideCamera.Display.Render(s32MemIDMaster, displayHandleSideCamera, uEye.Defines.DisplayRenderMode.FitToWindow);

            // PKv5.5.0
            sideCameraFrameCountAll++;
            if (sideCameraFrameCountAll == Int32.MaxValue)          // Once in a lifetime code !
                sideCameraFrameCountAll = 0;
        }

        private void onFrameEventBack(object sender, EventArgs e)
        {

            uEye.Camera BackCamera = sender as uEye.Camera;
            Int32 s32MemIDMaster;
            BackCamera.Memory.GetActive(out s32MemIDMaster);
            BackCamera.Display.Render(s32MemIDMaster, displayHandleBackCamera, uEye.Defines.DisplayRenderMode.Normal);

            // PKv5.5.0

            backCameraFrameCountAll++;
            if (backCameraFrameCountAll == Int32.MaxValue)          // Once in a lifetime code !
                backCameraFrameCountAll = 0;
        }

        private void onSequenceEventSide(object sender, EventArgs e)
        {
            uEye.Camera Camera = sender as uEye.Camera;
            SequenceCount++;
        }

        private uEye.Camera getCurrentCamera()
        {
            if (currentCamera == 1) return BackCamera;
            else return SideCamera;
        }

        // PKv5.5.0.5  Will return the filename (full path) of the last video file.

        public string GetLastVideoFileName()
        {
            return lastVideoFileName;
        }


        // PKv5.4.0 Added parameters for videoQuality: 1 to 100,  videoWidth 8 to 800 (will use closest multiple of 8).

        public int RecordVideo(string VideoDirectory, int videoQuality, int videoWidth)
        {
            double fps = 0.0;


            if (Path.GetDirectoryName(datedVideoFolder) != VideoDirectory)
            {
                datedVideoFolder = null;
                CheckFolderDate(VideoDirectory);
                CheckVideoNumber(datedVideoFolder);
            }
            if (datedVideoFolder == null)  CheckFolderDate(VideoDirectory); 
            if (currentVideoNumber == -1)  CheckVideoNumber(datedVideoFolder);
            
            string videoName = datedVideoFolder + Path.DirectorySeparatorChar + currentVideoNumber.ToString()+".avi";
            lastVideoFileName = videoName;          // Lets store this away  PKv5.5.0.5
            
            if(currentCamera == 1)
            {

                // PKv5.4.0 Do not do this (next line commented out)  
                // BackCamera.Video.SetFrameRate(25);

                /* PKv5.5.0 Commented out the following 3 lines as it will not be focus of release.

                int useVideoWidth = (int) 8* (videoWidth / (int) 8);
                Console.WriteLine(" Video width = {0}", useVideoWidth);
                BackCamera.Size.AOI.Set(0, 0, useVideoWidth, 600);    

                */

                // PKv5.4.0 Lets try a smaller region of interest (400,600)
                // Did not work to speed up the video capture...  Need to investigate further.
                // Fast enough for now and a cool feature.
                // Once timing to speed up is OK then i will add pass variable to put the width starting point
                // as a passed in parameter.


                /* Experimeted with uEye.Tools functions but could not get it to work.

                videoID = 0;                        
                uEye.Defines.Status uEyeStatusReturned;


                uEyeStatusReturned = VideoRecording.Init(BackCamera, out videoID);
                if (uEyeStatusReturned != uEye.Defines.Status.Success)
                {
                    Console.WriteLine("VideoRecording.Init  call failed return value -> {0}", (int) uEyeStatusReturned);
                }

                uEyeStatusReturned = VideoRecording.SetImageSize(videoID, uEye.Defines.ColorMode.Mono8, 400, 600);
                if (uEyeStatusReturned != uEye.Defines.Status.Success)
                {
                    Console.WriteLine("VideoRecording.SetImageSize  call failed return value -> {0}", (int) uEyeStatusReturned);
                }

                BackCamera.Timing.Framerate.Get(out fps);   // PKv5.4.0 Lets match the video frame rate to the camera frame rate
                VideoRecording.SetFramerate(videoID, fps);
                VideoRecording.SetQuality(videoID, 50);
                VideoRecording.Open(videoID, videoName);
                VideoRecording.Start(videoID);

                */

                // This works but does not seem to go faster when we trim down the image size


                BackCamera.Timing.Framerate.Get(out fps);   // PKv5.4.0 Lets match the video frame rate to the camera frame rate
                BackCamera.Video.SetFrameRate(fps);          
                BackCamera.Video.ResetCount(); // PKv5.4.0,  Reset the count so we know how many frames were obtainied.
                BackCamera.Video.SetQuality(videoQuality);        // PKv5.4.0 Instead of 100 as it was before.
                BackCamera.Video.Start(videoName);

                currentVideoNumber += 1;
            }
            if (currentCamera == 2)
            {
                // PKv5.4.0 Do not do this (next line commented out)  
                // SideCamera.Video.SetFrameRate(25);

                SideCamera.Timing.Framerate.Get(out fps);   // PKv5.4.0 Lets match the video frame rate to the camera frame rate
                SideCamera.Video.SetFrameRate(fps);
                SideCamera.Video.ResetCount();  // PKv5.4.0
                SideCamera.Video.SetQuality(100);
                SideCamera.Video.Start(videoName);
                currentVideoNumber += 1;
            }

            stVideoTime = System.Environment.TickCount;

            return (currentVideoNumber - 1);
        }

        // PKv5.5.0   

        public void StartLiveAcquistion()
        {
            if (currentCamera == 2)
            {
                SideCamera.Trigger.Set(uEye.Defines.TriggerMode.Off);       // Turn off triggered acquisition
                isDroplet = false;                                          // Dont display the droplet with graphics.

                statusRet = SideCamera.Acquisition.Capture();
                if (statusRet != uEye.Defines.Status.Success)
                {
                    MessageBox.Show("Start Live Video failed, Tip Camera");
                }
                isLive = true;
            }
            else
            {
                BackCamera.Trigger.Set(uEye.Defines.TriggerMode.Off);       // Turn off triggered acquisition
                isDroplet = false;                                          // Dont display the droplet with graphics.

                statusRet = BackCamera.Acquisition.Capture();
                if (statusRet != uEye.Defines.Status.Success)
                {
                    MessageBox.Show("Start Live Video failed, Grid, 2nd Camera");
                }
                isLive = true;
            }
        }

        // PKv5.5.0  Added ability to start and stop live acquisition.

        public void StopLiveAcquistion()
        {
            if (currentCamera == 2)
            {
                statusRet = SideCamera.Acquisition.Stop();
                if (statusRet != uEye.Defines.Status.Success)
                {
                    MessageBox.Show("Stop Live Video failed, Tip Camera");
                }
                isLive = false;
            }
            else
            {
                statusRet = BackCamera.Acquisition.Stop();
                if (statusRet != uEye.Defines.Status.Success)
                {
                    MessageBox.Show("Stop Live Video failed, Grid 2nd Camera");
                }
                isLive = false;
            }
        }

        // PKv4.3.1
        // Pass it in the amount of time you need to wait in usec after receiving the trigger

        public void StartTriggeredAcquisition(int delayAfterTrigger, int flashDuration_usec, int flashDelay_usec)
        {

            uEye.Camera tempCam;
            tempCam = (currentCamera == 2) ? SideCamera : BackCamera;       // PKv4.1.1  Lets try a very compact / shorthand way to do this.


            // Set up the flash on triggered acquisition
            tempCam.IO.Flash.SetMode(uEye.Defines.IO.FlashMode.TriggerHighActive);
            setFlashDuration((uint)flashDelay_usec, (uint)flashDuration_usec);            //    Set the flash delay and duration,  minimum delay seems to be 40 here

            UInt32 u32Delay, u32Duration;
            tempCam.IO.Flash.GetParams(out u32Delay, out u32Duration);
            Console.WriteLine("Reading Flash Parameters: Delay ={0}, Duration={1}", u32Delay, u32Duration);

            uint u32Min, u32Max, u32Inc;
            tempCam.IO.Flash.GetDurationRange(out u32Min, out u32Max, out u32Inc);
            Console.WriteLine("Reading Flash Duration Range: Min ={0}, Max={1}, Inc={2}", u32Min, u32Max, u32Inc);

            tempCam.IO.Flash.GetDelayRange(out u32Min, out u32Max, out u32Inc);
            Console.WriteLine("Reading Flash Delay Range: Min ={0}, Max={1}, Inc={2}", u32Min, u32Max, u32Inc);

            // Now grab a frame.
            tempCam.Trigger.Set(uEye.Defines.TriggerMode.Lo_Hi);      // Will look for one trigger
            tempCam.Trigger.Delay.Set(delayAfterTrigger);
            tempCam.Timeout.Set(uEye.Defines.TimeoutMode.Trigger, 500);  // 5 Second timeout, strange 10msec increments for the method.
  //        getGrayScaleFrame = true;           // TODO

            int temp;
            tempCam.Trigger.Delay.Get(out temp);
            Console.WriteLine("Reading TRIGGER Delay: {0}", temp);

            tempCam.Acquisition.Freeze();   // Tried a timeout here.. but it seems to hang
        }


        public void setFlashDuration(uint flashDelay, uint flashDuration_usec)
        {
            if (currentCamera == 2)
                SideCamera.IO.Flash.SetParams(flashDelay, flashDuration_usec);
            else
                BackCamera.IO.Flash.SetParams(flashDelay, flashDuration_usec);

        }

        // PK5.5.0  Running count of the number of gray scale frames acquired.
        // TODO updrade for grayscale acquisition.  

        public int GetFrameCount()
        {
            //if (currentCamera == 2)
            //    return sideCameraFrameCountGrayScale;
            //else
            //    return backCameraFrameCountGrayScale;

            if (currentCamera == 2)
                return sideCameraFrameCountAll;
            else
                return backCameraFrameCountAll;
        }

        public int EnableStrobe(int strobeDuration, int strobeDelay)            // PKv5.4.0  added this method to turn on the free running strobe
        {
            if (currentCamera == 1)
            {
                BackCamera.IO.Flash.SetParams((uint)strobeDelay, (uint)strobeDuration);
                BackCamera.IO.Flash.SetMode(uEye.Defines.IO.FlashMode.FreerunHighActive);
            }
            else
            {
                SideCamera.IO.Flash.SetParams((uint)strobeDelay, (uint)strobeDuration);
                SideCamera.IO.Flash.SetMode(uEye.Defines.IO.FlashMode.FreerunHighActive);
            }
            return 0;
        }

        public int DisableStrobe()
        {
            if (currentCamera == 1)
            {
                BackCamera.IO.Flash.SetMode(uEye.Defines.IO.FlashMode.Off);         // PKv5.4.0  added this method to turn off the free running strobe
            }
            else
            {
                SideCamera.IO.Flash.SetMode(uEye.Defines.IO.FlashMode.Off);         // PKv5.4.0  added this method to turn off the free running strobe
            }
            return 0;
        }

        public int StopRecordingVideo()
        {
            int etEntering = System.Environment.TickCount - stVideoTime; // Elapsed Video recording time.

            uint frameCount=0, lostCount=0; // PKv5.4.0
            double frameRate=0.0; // PKv5.4.0
            if (currentCamera == 1)
            {

                BackCamera.Video.GetFrameCount(out frameCount);
                BackCamera.Video.GetLostCount(out lostCount);
                BackCamera.Video.GetFrameRate(out frameRate);
                BackCamera.Video.Stop();
                
                /* Tried using the Tools but did not work 

                VideoRecording.GetFrameCount(videoID, out frameCount);
                VideoRecording.Stop(videoID);
                VideoRecording.Close(videoID);

                */


                BackCamera.Size.AOI.Set(0, 0, 800, 600);    // PKv5.4.0 Restore the AOI

            }
            if (currentCamera == 2)
            {
                SideCamera.Video.GetFrameCount(out frameCount);
                SideCamera.Video.GetLostCount(out lostCount);
                SideCamera.Video.GetFrameRate(out frameRate);
                SideCamera.Video.Stop();
 
            }

            int etExiting = System.Environment.TickCount - stVideoTime; // Elapsed Video recording time.

            Console.WriteLine("frameCount={0}, lostCount(frames)={1}, frameRate (fps)={2:0.0}", frameCount, lostCount, frameRate);

            Console.WriteLine("Elasped Time in msec (entering / exiting) = {0} / {1} ", etEntering, etExiting);

            double calculatedFPS = 1000 * frameCount / etEntering;

            double numberOfMissingFrames = (frameRate * etEntering / 1000) - frameCount;

            Console.WriteLine("Calulated FPS = {0:0.0}", calculatedFPS);

            Console.WriteLine("Calulated missing frames = {0:0}", numberOfMissingFrames);


            return (currentVideoNumber - 1);
        }
        private void CheckVideoNumber(string directoryPath)
        {
            string[] filePaths = Directory.GetFiles(directoryPath, "*.avi");
            string fileName;
            int largestVideoNumber = 0;
            int tempVideoNumber = 0;
            foreach (string fileNameIterator in filePaths)
            {
                fileName = Path.GetFileName(fileNameIterator);
                string[] fileNameParts = fileName.Split('.');
                try { tempVideoNumber = Convert.ToInt16(fileNameParts[0]); }
                catch { }
                if (tempVideoNumber > largestVideoNumber)
                { largestVideoNumber = tempVideoNumber; }

            }
             currentVideoNumber = largestVideoNumber+1;
        }
        private void CheckFolderDate(string inputDirectoryPath)
        {
            DateTime thisDay = DateTime.Today;
            DateTime tempDate;
            
            string[] directoryPaths = Directory.GetDirectories(inputDirectoryPath);
            string folderNames;
            foreach (string directoryNameIterator in directoryPaths)
            {
                folderNames = Path.GetFileName(directoryNameIterator);

                if (DateTime.TryParse(folderNames, out tempDate))
                {
                    if (tempDate == thisDay)
                        {
                            datedVideoFolder = directoryNameIterator;
                        }
                }
            }
            if (datedVideoFolder==null)
            {
                datedVideoFolder = inputDirectoryPath + Path.DirectorySeparatorChar + thisDay.ToString("M-d-yyyy");
                Directory.CreateDirectory(datedVideoFolder);
            }
        }

        //   PKv5.5.0.4  2017-02-22 ,  Try to save the image.

        public int SaveImage(string fileName)
        {
            Bitmap bitmap;
            if (currentCamera == 1)
            {
                Int32 s32MemID;
                BackCamera.Memory.GetActive(out s32MemID);
                BackCamera.Memory.ToBitmap(s32MemID, out bitmap);
            }
            else
            {
                Int32 s32MemID;
                SideCamera.Memory.GetActive(out s32MemID);
                SideCamera.Memory.ToBitmap(s32MemID, out bitmap);
            }
            bitmap.Save(fileName);   // Save the grayscale bitmap
            return 0;
        }

    }
}

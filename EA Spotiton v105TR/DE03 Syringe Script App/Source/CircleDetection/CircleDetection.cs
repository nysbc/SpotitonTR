using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Windows.Forms;

namespace Aurigin
{
    public class CircleDetection
    {
        private Bitmap img = null;

        public PointF getCircleCenter(Bitmap InputImage)
        {
            PointF OutputCenter = new Point();

            Image<Bgr, Byte> img =
              new Image<Bgr, byte>(InputImage);//.Resize(400, 400, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR, true);

            //Convert the image to grayscale and filter out the noise
            Image<Gray, Byte> gray = img.Convert<Gray, Byte>().PyrDown().PyrUp();

            //additional processing by Venkat
            int w = 400;
            int h = 400;
            int sigma1 = 1;
            int sigma2 = 1;
            int k = 4;


            w = (w % 2 == 0) ? w - 1 : w;
            h = (h % 2 == 0) ? h - 1 : h;
            //apply gaussian smoothing using w, h and sigma 
            var gaussianSmooth = gray.SmoothGaussian(w, h, sigma1, sigma2);
            //obtain the mask by subtracting the gaussian smoothed image from the original one 
            var mask = gray - gaussianSmooth;
            //add a weighted value k to the obtained mask 
            mask *= k;
            //sum with the original image 
            gray += mask;
            
            Image<Gray, Byte> summed_image = gray;
            //circleImageBox.Image = gray;

            #region circle detection
         //   Stopwatch watch = Stopwatch.StartNew();
            double cannyThreshold = 30.0;
            double circleAccumulatorThreshold = 80;

            CircleF[] circles = summed_image.HoughCircles(
                new Gray(cannyThreshold),
                new Gray(circleAccumulatorThreshold),
                2.0, //Resolution of the accumulator used to detect centers of the circles
                20.0, //min distance 
                5, //min radius
                0 //max radius
                )[0]; //Get the circles from the first channel
            //   watch.Stop();
            //    msgBuilder.Append(String.Format("Hough circles - {0} ms;", watch.ElapsedMilliseconds));
            #endregion

            #region draw circles
            Image<Bgr, Byte> circleImage = img.CopyBlank();
            foreach (CircleF circle in circles)
            {
                circleImage.Draw(circle, new Bgr(Color.Brown), 2);
           //     Console.WriteLine(circle.Center);
                OutputCenter = circle.Center;
            }
            //circleImageBox.Image = circleImage;
            #endregion
            
            return OutputCenter;
        }
    }
}

using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fibers_Density_Estimater.Headers
{
    public class FibreInfo
    {
        public string ID { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public bool GT { get; set; } = false;
        public int Rotation { get; set; } = 0;
        public float NumberOfFibres_GT { get; set; }
        public float NumberOfFibres_Calculated { get; set; }
        //public float Length_mm { get; set; }
        public float Density { get; set; }
        public float Min_Spacing { get; set; }
        public float Max_Spacing { get; set; }
        public float Evaluation { get; set; }
        //public bool Orientation { get; set; }
        public List<int> Peaks { get; set; } = new List<int>();

        public FibreInfo DeepCopy()
        {
            FibreInfo Temp = new FibreInfo();
            Temp.ID = ID;
            Temp.GT = GT;
            Temp.NumberOfFibres_GT = NumberOfFibres_GT;
            Temp.NumberOfFibres_Calculated = NumberOfFibres_Calculated;
            //Temp.Length_mm = Length_mm;
            Temp.Density = Density;
            Temp.Min_Spacing = Min_Spacing;
            Temp.Max_Spacing = Max_Spacing;
            Temp.Evaluation = Evaluation;
            //Temp.Orientation = Orientation;
            foreach(int peak in Peaks)
            {
                Temp.Peaks.Add(peak);
            }

            return Temp;
        }
    }

    //public class Detected_Fibres
    //{
    //    public int Fibres_Number { get; set; } = 0;
    //    public List<int> Fibres_Positions { get; set; }
    //}

    public class Papyri_Fibres
    {
        public List<FibreInfo> Read_Fibre_Images(string Images_Path)
        {
            List<FibreInfo> Results = new List<FibreInfo>();
            // loop over all valid files with ".csv" extension
            IEnumerable<string> Images;
            Images = Directory.EnumerateFiles(Images_Path, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".jpeg") ||
                s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".tiff") || s.ToLower().EndsWith(".png")
                || s.ToLower().EndsWith(".bmp"));

            foreach (string img in Images)
            {
                FibreInfo Current_Fibre_Info = new FibreInfo();
                Current_Fibre_Info.FilePath = img;
                string OnlyFileName = System.IO.Path.GetFileNameWithoutExtension(img);
                Current_Fibre_Info.FileName = System.IO.Path.GetFileName(img);
                string[] FileName = OnlyFileName.Split('_');
                if (FileName.Length > 1)
                {
                    if (FileName[0] == "GT")
                    {
                        Current_Fibre_Info.GT = true;
                        Current_Fibre_Info.NumberOfFibres_GT = float.Parse(FileName[1]);
                        string Current_ID = "";
                        if (FileName.Length > 2)
                        {
                            for (int i = 2; i < FileName.Length; i++)
                            {
                                Current_ID += FileName[i] + '_';
                            }
                            Current_ID = Current_ID.Substring(0, Current_ID.Length - 1);
                        }
                        else
                        {
                            Current_ID = "No_ID";
                        }
                        Current_Fibre_Info.ID = Current_ID;
                    }
                    else
                    {
                        Current_Fibre_Info.ID = OnlyFileName;
                    }
                }
                else
                {
                    Current_Fibre_Info.ID = FileName[0];
                }
                Results.Add(Current_Fibre_Info);
            }

            return Results;
        }
        
        public List<FibreInfo> Populate_Fibre_Info(List<FibreInfo> Fibres_List, float Length)
        {
            List<FibreInfo> Results = new List<FibreInfo>();
            foreach(var fibre in Fibres_List)
            {
                float Current_Fibres_Number;
                if(fibre.GT == true)
                {
                    Current_Fibres_Number = fibre.NumberOfFibres_GT;
                }
                else
                {
                    Current_Fibres_Number = fibre.NumberOfFibres_Calculated;
                }

                fibre.Density = Current_Fibres_Number / Length;

                Results.Add(fibre);
            }

            return Results;
        }

        public Mat Non_Maxima_Suppression(Mat src, bool remove_plateaus)
        {
            Mat Result = new Mat();
            // find pixels that are equal to the local neighborhood not maximum (including 'plateaus')
            Cv2.Dilate(src, Result, new Mat());
            Cv2.Compare(src, Result, Result, CmpTypes.GE);

            // optionally filter out pixels that are equal to the local minimum ('plateaus')
            if (remove_plateaus)
            {
                Mat non_plateau_mask = new Mat();
                Cv2.Erode(src, non_plateau_mask, new Mat());
                Cv2.Compare(src, non_plateau_mask, non_plateau_mask, CmpTypes.GT);
                Cv2.BitwiseAnd(Result, non_plateau_mask, Result);
            }

            return Result;
        }

        // function that finds the peaks of a given hist image
        public List<int> Find_Hist_Peaks(Mat _src, Size ksize, float min_scale = 0.8F, float max_scale = 0.2F, bool remove_plateus = true)
        {
            List<int> Results = new List<int>();
            if (ksize == null)
            {
                ksize.Width = 9;
                ksize.Height = 9;
            }

            Mat hist = _src.Clone();
            hist.ConvertTo(hist, MatType.CV_32F);

            for (int row = 0; row < hist.Rows; row++)
            {
                float Position = hist.Get<float>(row, 0);
            }

            // find the min and max values of the hist image
            double min_val, max_val;
            Cv2.MinMaxLoc(hist, out min_val, out max_val);

            Mat mask = new Mat();
            Cv2.GaussianBlur(hist, hist, ksize, 0); // smooth a bit in order to obtain better result

            mask = Non_Maxima_Suppression(hist, remove_plateus); // extract local maxima

            //hist.ConvertTo(hist, MatType.CV_32F);
            Mat maxima = new Mat();   // output, locations of non-zero pixels   
            Cv2.FindNonZero(mask, maxima);

            maxima.ConvertTo(maxima, MatType.CV_16U);
            Mat[] All_Channels = maxima.Split();
            Mat maxima_Channel_2 = All_Channels[1];
            for (int row = 0; row < maxima_Channel_2.Rows; row++)
            {
                int first = maxima_Channel_2.Get<UInt16>(row, 0);
            }

            for (int row = 0; row < maxima_Channel_2.Rows; row++)
            {
                int Position = maxima_Channel_2.Get<UInt16>(row,0);            
                float val = hist.Get<float>(Position, 0);

                // filter peaks
                if ((min_val < (val * min_scale)) && val > (max_val * max_scale))
                    Results.Add(Position);
                //if ((val > (max_val * scale)))
                //    Results.Add(Position);
            }

            return Results;
        }

        public Mat Calculate_Vertical_Projection(Mat Image_Gray)
        {
            Mat Result = new Mat();

            for (int Col = 0; Col < Image_Gray.Width; Col++)
            {
                int Current_Counter = 0;
                for (int Row = 0; Row < Image_Gray.Height; Row++)
                {
                    Current_Counter += Image_Gray.Get<byte>(Row, Col);
                }
                Result.Add(Current_Counter);
            }

            return Result;
        }

        public Mat Calculate_Projection_Histogram(Mat Projection)
        {
            int Width = Projection.Height, Height = 100;
            Mat Result = new Mat(new Size(Width, Height), MatType.CV_8UC1, Scalar.All(255));
            
            int[] hdims = { Width }; // Histogram size for each dimension

            // Get the max value of histogram
            double minVal, maxVal;
            Cv2.MinMaxLoc(Projection, out minVal, out maxVal);

            Scalar color = Scalar.All(100);
            // Scales and draws histogram
            Projection = Projection * (maxVal != 0 ? Height / maxVal : 0.0);
            for (int j = 0; j < hdims[0]; ++j)
            {
                int binW = (int)((double)Width / hdims[0]);
                Result.Rectangle(new Point(j * binW, Result.Rows - (int)(Projection.Get<int>(j))), new Point((j + 1) * binW, Result.Rows), color, -1);
            }

            return Result;
        }

        public void RotateImage(double angle, double scale, Mat src, Mat dst)
        {
            var imageCenter = new Point2f(src.Cols / 2f, src.Rows / 2f);
            var rotationMat = Cv2.GetRotationMatrix2D(imageCenter, angle, scale);
            Cv2.WarpAffine(src, dst, rotationMat, src.Size());
            using (Mat mask = dst.InRange(new Scalar(0, 0, 0), new Scalar(1, 1, 1)))
            {
                dst.SetTo(new Scalar(255,255,255), mask);
            }
        }

        public double Rotation_GUI(string Path, Mat Original_Image)
        {
            double Result;

            using (var dst = new Mat())
            {
                Original_Image.CopyTo(dst);

                var angle = 0.0;
                var scale = 1.0;

                var window = new Window("Rotate", image: dst);
                var angleTrackbar = window.CreateTrackbar(
                    trackbarName: "Rotation", initialPos: 10, max: 20,
                    callback: pos =>
                    {
                        angle = (pos - 10);

                        RotateImage(angle, scale, Original_Image, dst);
                        window.Image = dst;
                    });

                //angleTrackbar.Callback.DynamicInvoke(0);

                var key = Cv2.WaitKey();

                Result = angle;

                window.Dispose();
                dst.ImWrite(Path);
                Window.DestroyAllWindows();
            }

            return Result;
        }

        public FibreInfo Line_Detector(Mat Original_Image, Uploaded_Images _Uploaded_Images)
        {
            FibreInfo Result = new FibreInfo();
            Mat Temp_Img = Original_Image.Clone();

            /// Split channels of coloured image
            if (_Uploaded_Images.Channel > 0 && (_Uploaded_Images.Channel) <= Temp_Img.Channels())
            {
                Mat[] All_Channels = Temp_Img.Split();
                Mat One_Channel_Image = All_Channels[(_Uploaded_Images.Channel - 1)].Clone();
                Temp_Img.Release();
                Temp_Img = One_Channel_Image.Clone();
            }
            else
            {
                Cv2.CvtColor(Temp_Img, Temp_Img, ColorConversionCodes.BGR2GRAY);
                Temp_Img.ConvertTo(Temp_Img, MatType.CV_8UC1);
            }

            if (_Uploaded_Images.Apply_HistEqu)
            {
                Temp_Img.EqualizeHist();
            }

            /// Invert grayscale image
            Mat Inverted_Image = new Mat();
            Cv2.BitwiseNot(Temp_Img, Inverted_Image);

            /// Contrast Limiting Adaptive Histogram Equalisation
            if (_Uploaded_Images.Apply_CLAHE)
            {
                Size TilesGridSize = _Uploaded_Images.TilesGridSize;
                CLAHE Contrast_Optimiser = CLAHE.Create();
                Contrast_Optimiser.ClipLimit = _Uploaded_Images.ClipLimit;
                Contrast_Optimiser.TilesGridSize = TilesGridSize;
                Contrast_Optimiser.Apply(Inverted_Image, Inverted_Image);
            }

            Mat Vertical_Projection = Calculate_Vertical_Projection(Inverted_Image);

            Size ksize = _Uploaded_Images.ksize;
            List<int> Current_Peaks = Find_Hist_Peaks(Vertical_Projection, ksize, _Uploaded_Images.Min_Scale, _Uploaded_Images.Max_Scale);

            // remove peaks at the borders
            double border = Convert.ToDouble(_Uploaded_Images.DetectionBorder / 100.0) * Convert.ToDouble(Vertical_Projection.Rows);
            foreach (var peak in Current_Peaks)
            {
                if (peak > border && peak < (Vertical_Projection.Rows - border))
                {
                    Result.Peaks.Add(peak);
                }
            }

            return Result;
        }

        public FibreInfo Line_Detector(Mat Original_Image, Temp_Parameters _Temp_Parameters)
        {
            FibreInfo Result = new FibreInfo();
            Mat Temp_Img = Original_Image.Clone();

            /// Split channels of coloured image
            if (_Temp_Parameters.Channel > 0 && (_Temp_Parameters.Channel) <= Temp_Img.Channels())
            {
                Mat[] All_Channels = Temp_Img.Split();
                Mat One_Channel_Image = All_Channels[(_Temp_Parameters.Channel - 1)].Clone();
                Temp_Img.Release();
                Temp_Img = One_Channel_Image.Clone();
            }
            else
            {
                Cv2.CvtColor(Temp_Img, Temp_Img, ColorConversionCodes.BGR2GRAY);
                Temp_Img.ConvertTo(Temp_Img, MatType.CV_8UC1);
            }

            if (_Temp_Parameters.Apply_HistEqu)
            {
                Temp_Img.EqualizeHist();
            }

            /// Invert grayscale image
            Mat Inverted_Image = new Mat();
            Cv2.BitwiseNot(Temp_Img, Inverted_Image);

            /// Contrast Limiting Adaptive Histogram Equalisation
            if (_Temp_Parameters.Apply_CLAHE)
            {
                Size TilesGridSize = _Temp_Parameters.TilesGridSize;
                CLAHE Contrast_Optimiser = CLAHE.Create();
                Contrast_Optimiser.ClipLimit = _Temp_Parameters.ClipLimit;
                Contrast_Optimiser.TilesGridSize = TilesGridSize;
                Contrast_Optimiser.Apply(Inverted_Image, Inverted_Image);
            }

            Mat Vertical_Projection = Calculate_Vertical_Projection(Inverted_Image);

            Size ksize = _Temp_Parameters.ksize;
            List<int> Current_Peaks = Find_Hist_Peaks(Vertical_Projection, ksize, _Temp_Parameters.Min_Scale, _Temp_Parameters.Max_Scale);

            // remove peaks at the borders
            double border = Convert.ToDouble(_Temp_Parameters.DetectionBorder / 100) * Convert.ToDouble(Vertical_Projection.Rows);
            foreach (var peak in Current_Peaks)
            {
                if (peak > border && peak < (Vertical_Projection.Rows - border))
                {
                    Result.Peaks.Add(peak);
                }
            }

            return Result;
        }

        public float Evaluate_Detection_Results(List<FibreInfo> Fibres_List)
        {
            float Result;

            foreach (var fibre in Fibres_List)
            {
                // Evaluate detection results if ground-truth information is available
                if (fibre.GT)
                {
                    float GTN = fibre.NumberOfFibres_GT;
                    float CN = fibre.NumberOfFibres_Calculated;
                    if (GTN <= 0)
                    {
                        fibre.Evaluation = 0;
                        break;
                    }
                    if (CN <= 0)
                    {
                        fibre.Evaluation = 0;
                        break;
                    }

                    fibre.Evaluation = ((GTN - (Math.Abs(GTN - CN))) / GTN) * 100;
                    if (fibre.Evaluation < 0)
                    {
                        fibre.Evaluation = 0;
                    }
                }
            }

            // Calculate total evaluation of all detections of images with ground-truth information
            float Total_Evaluation = 0;
            float Counter = 0;
            foreach (var fibre in Fibres_List)
            {
                if (fibre.GT)
                {
                    Total_Evaluation += fibre.Evaluation;
                    Counter++;
                }
            }
            Total_Evaluation = Total_Evaluation / Counter;
            Result = Total_Evaluation;

            return Result;
        }

        public void Min_Max_Spacing(FibreInfo Fibre, float Length, int Img_Width)
        {
            // Calculate Min and Max spacing between detected lines
            if (Fibre.Peaks.Count() > 0)
            {

                List<int> Spacings = new List<int>();

                int Previous_Peak = Fibre.Peaks[0];

                for (int peak = 1; peak < Fibre.Peaks.Count(); peak++)
                {
                    Spacings.Add(Fibre.Peaks[peak] - Previous_Peak);
                    Previous_Peak = Fibre.Peaks[peak];
                }
                Spacings.RemoveAll(item => item == 0);
                float PixelPerMM = Length / Img_Width;

                if (Spacings.Count() > 0)
                {
                    Fibre.Min_Spacing = Spacings.Min() * PixelPerMM;
                    Fibre.Max_Spacing = Spacings.Max() * PixelPerMM;
                }
                else
                {
                    Fibre.Min_Spacing = 0;
                    Fibre.Max_Spacing = 0;
                }
            }
            else
            {
                Fibre.Min_Spacing = 0;
                Fibre.Max_Spacing = 0;
            }

        }

        public void Save_Detections_As_Images(Mat Img, Uploaded_Images _Uploaded_Images, string Histogram_Path, string Pattern_Path, string Enhanced_Path)
        {
            Mat TempImg = Img.Clone();

            /// Split channels of coloured image
            if (_Uploaded_Images.Channel > 0 && (_Uploaded_Images.Channel) <= TempImg.Channels())
            {
                Mat[] All_Channels = TempImg.Split();
                Mat One_Channel_Image = All_Channels[(_Uploaded_Images.Channel - 1)].Clone();
                TempImg.Release();
                TempImg = One_Channel_Image.Clone();
            }
            else
            {
                Cv2.CvtColor(TempImg, TempImg, ColorConversionCodes.BGR2GRAY);
                TempImg.ConvertTo(TempImg, MatType.CV_8UC1);
            }

            if (_Uploaded_Images.Apply_HistEqu)
            {
                TempImg.EqualizeHist();
            }

            /// Invert grayscale image
            Mat Inverted_Image = new Mat();
            Cv2.BitwiseNot(TempImg, Inverted_Image);

            /// Contrast Limiting Adaptive Histogram Equalisation
            if (_Uploaded_Images.Apply_CLAHE)
            {
                Size TilesGridSize = _Uploaded_Images.TilesGridSize;
                CLAHE Contrast_Optimiser = CLAHE.Create();
                Contrast_Optimiser.ClipLimit = _Uploaded_Images.ClipLimit;
                Contrast_Optimiser.TilesGridSize = TilesGridSize;
                Contrast_Optimiser.Apply(Inverted_Image, Inverted_Image);
            }

            Inverted_Image.ImWrite(Enhanced_Path);

            Mat Vertical_Projection = Calculate_Vertical_Projection(Inverted_Image);

            Mat Histogram_Image = Calculate_Projection_Histogram(Vertical_Projection);

            FibreInfo Current_Fibre_Info = new FibreInfo();
            
            Current_Fibre_Info = Line_Detector(Img, _Uploaded_Images);

            foreach (int peak in Current_Fibre_Info.Peaks)
            {
                Histogram_Image.Line(new Point(peak, Histogram_Image.Height), new Point(peak, 0), new Scalar(0, 0, 0), 2);
            }
            Histogram_Image.ImWrite(Histogram_Path);

            Mat Pattern = new Mat(Histogram_Image.Size(), Histogram_Image.Type());
            Pattern.SetTo(255);
            foreach (int peak in Current_Fibre_Info.Peaks)
            {
                Pattern.Line(new Point(peak, Pattern.Height), new Point(peak, 0), new Scalar(0, 0, 0), 2);
            }
            Pattern.ImWrite(Pattern_Path);
        }

        public void Save_Detections_As_Images(Mat Img, Temp_Parameters _Temp_Parameters, string Histogram_Path, string Pattern_Path, string Enhanced_Path)
        {
            Mat TempImg = Img.Clone();

            /// Split channels of coloured image
            if (_Temp_Parameters.Channel > 0 && (_Temp_Parameters.Channel) <= TempImg.Channels())
            {
                Mat[] All_Channels = TempImg.Split();
                Mat One_Channel_Image = All_Channels[(_Temp_Parameters.Channel - 1)].Clone();
                TempImg.Release();
                TempImg = One_Channel_Image.Clone();
            }
            else
            {
                Cv2.CvtColor(TempImg, TempImg, ColorConversionCodes.BGR2GRAY);
                TempImg.ConvertTo(TempImg, MatType.CV_8UC1);
            }

            if (_Temp_Parameters.Apply_HistEqu)
            {
                TempImg.EqualizeHist();
            }

            /// Invert grayscale image
            Mat Inverted_Image = new Mat();
            Cv2.BitwiseNot(TempImg, Inverted_Image);

            /// Contrast Limiting Adaptive Histogram Equalisation
            if (_Temp_Parameters.Apply_CLAHE)
            {
                Size TilesGridSize = _Temp_Parameters.TilesGridSize;
                CLAHE Contrast_Optimiser = CLAHE.Create();
                Contrast_Optimiser.ClipLimit = _Temp_Parameters.ClipLimit;
                Contrast_Optimiser.TilesGridSize = TilesGridSize;
                Contrast_Optimiser.Apply(Inverted_Image, Inverted_Image);
            }

            Inverted_Image.ImWrite(Enhanced_Path);

            Mat Vertical_Projection = Calculate_Vertical_Projection(Inverted_Image);

            Mat Histogram_Image = Calculate_Projection_Histogram(Vertical_Projection);

            FibreInfo Current_Fibre_Info = new FibreInfo();

            Current_Fibre_Info = Line_Detector(Img, _Temp_Parameters);

            foreach (int peak in Current_Fibre_Info.Peaks)
            {
                Histogram_Image.Line(new Point(peak, Histogram_Image.Height), new Point(peak, 0), new Scalar(0, 0, 0), 2);
            }
            Histogram_Image.ImWrite(Histogram_Path);

            Mat Pattern = new Mat(Histogram_Image.Size(), Histogram_Image.Type());
            Pattern.SetTo(255);
            foreach (int peak in Current_Fibre_Info.Peaks)
            {
                Pattern.Line(new Point(peak, Pattern.Height), new Point(peak, 0), new Scalar(0, 0, 0), 2);
            }
            Pattern.ImWrite(Pattern_Path);
        }
    }
}
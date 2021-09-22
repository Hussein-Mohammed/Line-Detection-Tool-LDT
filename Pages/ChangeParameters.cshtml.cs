using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fibers_Density_Estimater.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCvSharp;

namespace Fibers_Density_Estimater.Pages
{
    public class ChangeParametersModel : PageModel
    {
        private readonly Temp_Parameters _Temp_Parameters;
        private readonly Uploaded_Images _Uploaded_Images;
        private IWebHostEnvironment _hostingEnvironment;
        public ChangeParametersModel(Uploaded_Images Uploaded_Images, IWebHostEnvironment hostingEnvironment, Temp_Parameters Temp_Parameters)
        {
            _Uploaded_Images = Uploaded_Images;
            _hostingEnvironment = hostingEnvironment;
            _Temp_Parameters = Temp_Parameters;
        }

        public void OnGet()
        {
            int NumOfChannels = 1;
            for (int SameFN = 0; SameFN < _Uploaded_Images.Images.Count(); SameFN++)
            {
                if (_Temp_Parameters.ID == _Uploaded_Images.Images[SameFN].ID)
                {
                    Papyri_Fibres PFA = new Papyri_Fibres();
                    Mat Original_Image = new Mat(_Uploaded_Images.Images[SameFN].FilePath, ImreadModes.Color);
                    NumOfChannels = Original_Image.Channels();
                }
            }
            ViewData["MaxChannels"] = NumOfChannels;
        }

        public ActionResult OnPostChangeParameters(bool Rotate, float Length, float Min_Threshold, float Max_Threshold,
            double ClipLimit, bool Apply_HistEqu, bool Apply_CLAHE, int Channel, int Hist_Blur_Size, int TilesGridSize, double DetectionBorder)
        {
            if (Rotate)
                _Temp_Parameters.Rotate = true;
            else
                _Temp_Parameters.Rotate = false;

            _Temp_Parameters.Length_mm = Length;
            _Temp_Parameters.Min_Scale = Min_Threshold;
            _Temp_Parameters.Max_Scale = Max_Threshold;
            _Temp_Parameters.ClipLimit = ClipLimit;

            if (Apply_CLAHE)
                _Temp_Parameters.Apply_CLAHE = true;
            else
                _Temp_Parameters.Apply_CLAHE = false;

            if (Apply_HistEqu)
                _Temp_Parameters.Apply_HistEqu = true;
            else
                _Temp_Parameters.Apply_HistEqu = false;

            _Temp_Parameters.Channel = Channel;
            _Temp_Parameters.ksize = new Size(Hist_Blur_Size, Hist_Blur_Size);
            _Temp_Parameters.TilesGridSize = new Size(TilesGridSize, TilesGridSize);
            _Temp_Parameters.DetectionBorder = DetectionBorder;

            // Prepare the folders ///////////////////////////////////////////////////////
            string Preprocessed_Folder = "Preprocessed";
            string Pattern_Folder = "Pattern";
            string Histogram_Folder = "Histogram";
            string Enhanced_Folder = "Enhanced";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string Preprocessed_Path = Path.Combine(webRootPath, Preprocessed_Folder);
            string Pattern_Path = Path.Combine(webRootPath, Pattern_Folder);
            string Histogram_Path = Path.Combine(webRootPath, Histogram_Folder);
            string Enhanced_Path = Path.Combine(webRootPath, Enhanced_Folder);

            if (!Directory.Exists(Preprocessed_Path))
            {
                Directory.CreateDirectory(Preprocessed_Path);
            }
            Directory.EnumerateFiles(Preprocessed_Path).ToList().ForEach(f => System.IO.File.Delete(f));

            if (!Directory.Exists(Pattern_Path))
            {
                Directory.CreateDirectory(Pattern_Path);
            }
            Directory.EnumerateFiles(Pattern_Path).ToList().ForEach(f => System.IO.File.Delete(f));

            if (!Directory.Exists(Histogram_Path))
            {
                Directory.CreateDirectory(Histogram_Path);
            }
            Directory.EnumerateFiles(Histogram_Path).ToList().ForEach(f => System.IO.File.Delete(f));

            if (!Directory.Exists(Enhanced_Path))
            {
                Directory.CreateDirectory(Enhanced_Path);
            }
            Directory.EnumerateFiles(Enhanced_Path).ToList().ForEach(f => System.IO.File.Delete(f));
            // End of folders preparation //////////////////////////////////////////////////////////////

            for (int SameFN = 0; SameFN < _Uploaded_Images.Images.Count(); SameFN++)
            {
                if (_Temp_Parameters.ID == _Uploaded_Images.Images[SameFN].ID)
                {
                    Papyri_Fibres PFA = new Papyri_Fibres();
                    Mat Original_Image = new Mat(_Uploaded_Images.Images[SameFN].FilePath, ImreadModes.Color);

                    // Rotate the image if "Rotate" option is selected
                    if (_Temp_Parameters.Rotate)
                    {
                        Cv2.Rotate(Original_Image, Original_Image, RotateFlags.Rotate90Clockwise);
                    }

                    string Current_Preprocessed_Path = Path.Combine(Preprocessed_Path, _Uploaded_Images.Images[SameFN].FileName);
                    Original_Image.ImWrite(Current_Preprocessed_Path);

                    FibreInfo Current_Fibre_Info = new FibreInfo();
                    Mat TempImg = Original_Image.Clone();
                    Current_Fibre_Info = PFA.Line_Detector(TempImg, _Temp_Parameters);

                    // Generate Histogram and Pattern Images
                    string Current_Histogram_Path = Path.Combine(Histogram_Path, _Uploaded_Images.Images[SameFN].FileName);
                    string Current_Pattern_Path = Path.Combine(Pattern_Path, _Uploaded_Images.Images[SameFN].FileName);
                    string Current_Enhanced_Path = Path.Combine(Enhanced_Path, _Uploaded_Images.Images[SameFN].FileName);

                    PFA.Save_Detections_As_Images(TempImg, _Temp_Parameters, Current_Histogram_Path, Current_Pattern_Path, Current_Enhanced_Path);

                    break;
                }
            }

            return RedirectToPage("Details");
        }
    }
}
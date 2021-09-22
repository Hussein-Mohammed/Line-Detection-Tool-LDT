using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Fibers_Density_Estimater.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using OpenCvSharp;

namespace Fibers_Density_Estimater.Pages
{
    public class DetailsModel : PageModel
    {
        public static string Current_File_Name { get; set; } = "";
        public static string Current_Preprocessed_Path { get; set; } = "";
        public static string Current_Original_Path { get; set; } = "";
        public static string ID { get; set; } = "";
        public static double Current_Rotation_Angle { get; set; } = 0;
        public static string ImageSource { get; set; } = "Original";

        private readonly Temp_Parameters _Temp_Parameters;
        private readonly Uploaded_Images _Uploaded_Images;
        private IWebHostEnvironment _hostingEnvironment;
        public DetailsModel(Uploaded_Images Uploaded_Images, IWebHostEnvironment hostingEnvironment, Temp_Parameters Temp_Parameters)
        {
            _Uploaded_Images = Uploaded_Images;
            _Temp_Parameters = Temp_Parameters;
            _hostingEnvironment = hostingEnvironment;
        }

        public void OnGet()
        {
            ViewData["ImageSource"] = ImageSource; 
            ViewData["OrgImg"] = Current_File_Name;
            ViewData["OrgImg_WithoutExtension"] = Path.GetFileNameWithoutExtension(Current_File_Name);
        }

        public ActionResult OnPostRotate()
        {
            ViewData["ImageSource"] = ImageSource;
            ViewData["OrgImg"] = Current_File_Name;
            ViewData["OrgImg_WithoutExtension"] = Path.GetFileNameWithoutExtension(Current_File_Name);
            Mat Original_Image = new Mat(Current_Original_Path, ImreadModes.Color);

            Papyri_Fibres LDT = new Papyri_Fibres();
            Current_Rotation_Angle = LDT.Rotation_GUI(Current_Preprocessed_Path, Original_Image);
            Mat RotatedImg = new Mat(Current_Preprocessed_Path, ImreadModes.Color);

            // Recalculate everything according to the new rotation
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

            if (!Directory.Exists(Pattern_Path))
            {
                Directory.CreateDirectory(Pattern_Path);
            }

            if (!Directory.Exists(Histogram_Path))
            {
                Directory.CreateDirectory(Histogram_Path);
            }
            if (!Directory.Exists(Enhanced_Path))
            {
                Directory.CreateDirectory(Enhanced_Path);
            }

            // End of folders preparation //////////////////////////////////////////////////////////////

            for (int SameFN = 0; SameFN < _Uploaded_Images.Images.Count(); SameFN++)
            {
                if (ID == _Uploaded_Images.Images[SameFN].ID)
                {
                    Papyri_Fibres PFA = new Papyri_Fibres();

                    FibreInfo Current_Fibre_Info = new FibreInfo();
                    Mat TempImg = RotatedImg.Clone();
                    Current_Fibre_Info = PFA.Line_Detector(RotatedImg, _Uploaded_Images);

                    // Generate Histogram and Pattern Images
                    string Current_Histogram_Path = Path.Combine(Histogram_Path, _Uploaded_Images.Images[SameFN].FileName);
                    string Current_Pattern_Path = Path.Combine(Pattern_Path, _Uploaded_Images.Images[SameFN].FileName);
                    string Current_Enhanced_Path = Path.Combine(Enhanced_Path, _Uploaded_Images.Images[SameFN].FileName);

                    PFA.Save_Detections_As_Images(RotatedImg, _Uploaded_Images, Current_Histogram_Path, Current_Pattern_Path, Current_Enhanced_Path);

                    break;
                }
            }
            return Page();
        }

        public ActionResult OnPostShowDetails(string FN)
        {
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
                if (FN == _Uploaded_Images.Images[SameFN].ID)
                {
                    Papyri_Fibres PFA = new Papyri_Fibres();
                    Mat Original_Image = new Mat(_Uploaded_Images.Images[SameFN].FilePath, ImreadModes.Color);
                    string Current_PreprocessedImg_Path = Path.Combine(Preprocessed_Path, _Uploaded_Images.Images[SameFN].FileName);

                    // Rotate if Horizontal orientation is selected
                    //if (!_Uploaded_Images.Orientation)
                    //{
                    //    Cv2.Rotate(Original_Image, Original_Image, RotateFlags.Rotate90Clockwise);
                    //}
                    Original_Image.ImWrite(Current_PreprocessedImg_Path);

                    ViewData["OrgImg"] = _Uploaded_Images.Images[SameFN].FileName;
                    ViewData["OrgImg_WithoutExtension"] = Path.GetFileNameWithoutExtension(_Uploaded_Images.Images[SameFN].FileName);
                    ViewData["ImageSource"] = ImageSource;
                    ID = _Uploaded_Images.Images[SameFN].ID;
                    Current_File_Name = _Uploaded_Images.Images[SameFN].FileName;
                    Current_Preprocessed_Path = Current_PreprocessedImg_Path;
                    Current_Original_Path = _Uploaded_Images.Images[SameFN].FilePath;

                    FibreInfo Current_Fibre_Info = new FibreInfo();
                    Mat TempImg = Original_Image.Clone();
                    Current_Fibre_Info = PFA.Line_Detector(TempImg, _Uploaded_Images);

                    // Generate Histogram and Pattern Images
                    string Current_Histogram_Path = Path.Combine(Histogram_Path, _Uploaded_Images.Images[SameFN].FileName);
                    string Current_Pattern_Path = Path.Combine(Pattern_Path, _Uploaded_Images.Images[SameFN].FileName);
                    string Current_Enhanced_Path = Path.Combine(Enhanced_Path, _Uploaded_Images.Images[SameFN].FileName);

                    PFA.Save_Detections_As_Images(TempImg, _Uploaded_Images, Current_Histogram_Path, Current_Pattern_Path, Current_Enhanced_Path);

                    break;
                }
            }

            return Page();
        }

        public ActionResult OnPostResetRotation()
        {
            return OnPostShowDetails(ID);
        }

        public ActionResult OnPostApplyRotation()
        {
            if (Current_Rotation_Angle == 0)
            {
                return RedirectToPage("Results");
            }

            Papyri_Fibres PFA = new Papyri_Fibres();

            string Fibres_Folder = "Fibres";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string Fibres_Path = Path.Combine(webRootPath, Fibres_Folder);
            if (!Directory.Exists(Fibres_Path))
            {
                Directory.CreateDirectory(Fibres_Path);
            }

            for (int SameFN = 0; SameFN < _Uploaded_Images.Images.Count(); SameFN++)
            {
                if (ID == _Uploaded_Images.Images[SameFN].ID)
                {
                    Mat Original_Image = new Mat(_Uploaded_Images.Images[SameFN].FilePath, ImreadModes.Color);
                    PFA.RotateImage(Current_Rotation_Angle, 1, Original_Image, Original_Image);

                    string Current_OrgImg_Path = Path.Combine(Fibres_Path, _Uploaded_Images.Images[SameFN].FileName);
                    Original_Image.ImWrite(Current_OrgImg_Path);
                }
            }

            // Check whether there is at least one file with a valid extension
            IEnumerable<string> ValidFiles = Directory.EnumerateFiles(Fibres_Path, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".jpeg") ||
            s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".tiff") || s.ToLower().EndsWith(".png")
            || s.ToLower().EndsWith(".bmp"));

            for (int fbr = 0; fbr < ValidFiles.Count(); fbr++)
            {
                string Current_Image_Path = ValidFiles.ElementAt(fbr);
                Mat Original_Image = new Mat(Current_Image_Path, ImreadModes.Color);

                // Rotate if Horizontal orientation is selected
                //if (!_Uploaded_Images.Orientation)
                //{
                //    Cv2.Rotate(Original_Image, Original_Image, RotateFlags.Rotate90Clockwise);
                //}

                // Detect peaks in histogram
                FibreInfo Current_Fibre_Info = new FibreInfo();
                Current_Fibre_Info = PFA.Line_Detector(Original_Image, _Uploaded_Images);
                _Uploaded_Images.Images[fbr].NumberOfFibres_Calculated = Current_Fibre_Info.Peaks.Count();
                _Uploaded_Images.Images[fbr].Peaks = Current_Fibre_Info.Peaks;
                _Uploaded_Images.Images[fbr].Density = _Uploaded_Images.Images[fbr].NumberOfFibres_Calculated / (_Uploaded_Images.Length_mm / 10);

                // Calculate Min and Max spacing between detected lines
                PFA.Min_Max_Spacing(_Uploaded_Images.Images[fbr], _Uploaded_Images.Length_mm, Original_Image.Width);
            }

            _Uploaded_Images.Total_Evaluation = PFA.Evaluate_Detection_Results(_Uploaded_Images.Images);

            return RedirectToPage("Results");
        }

        public ActionResult OnPostApplyRotationToAll()
        {
            if (Current_Rotation_Angle == 0)
            {
                return RedirectToPage("Results");
            }

            Papyri_Fibres PFA = new Papyri_Fibres();

            string Fibres_Folder = "Fibres";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string Fibres_Path = Path.Combine(webRootPath, Fibres_Folder);
            if (!Directory.Exists(Fibres_Path))
            {
                Directory.CreateDirectory(Fibres_Path);
            }

            // Rotate all images
            for (int SameFN = 0; SameFN < _Uploaded_Images.Images.Count(); SameFN++)
            {
                Mat Original_Image = new Mat(_Uploaded_Images.Images[SameFN].FilePath, ImreadModes.Color);
                PFA.RotateImage(Current_Rotation_Angle, 1, Original_Image, Original_Image);

                string Current_OrgImg_Path = Path.Combine(Fibres_Path, _Uploaded_Images.Images[SameFN].FileName);
                Original_Image.ImWrite(Current_OrgImg_Path);
            }

            // Check whether there is at least one file with a valid extension
            IEnumerable<string> ValidFiles = Directory.EnumerateFiles(Fibres_Path, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".jpeg") ||
            s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".tiff") || s.ToLower().EndsWith(".png")
            || s.ToLower().EndsWith(".bmp"));

            for (int fbr = 0; fbr < ValidFiles.Count(); fbr++)
            {
                string Current_Image_Path = ValidFiles.ElementAt(fbr);
                Mat Original_Image = new Mat(Current_Image_Path, ImreadModes.Color);

                // Rotate if Horizontal orientation is selected
                //if (!_Uploaded_Images.Orientation)
                //{
                //    Cv2.Rotate(Original_Image, Original_Image, RotateFlags.Rotate90Clockwise);
                //}

                // Detect peaks in histogram
                FibreInfo Current_Fibre_Info = new FibreInfo();
                Current_Fibre_Info = PFA.Line_Detector(Original_Image, _Uploaded_Images);
                _Uploaded_Images.Images[fbr].NumberOfFibres_Calculated = Current_Fibre_Info.Peaks.Count();
                _Uploaded_Images.Images[fbr].Peaks = Current_Fibre_Info.Peaks;
                _Uploaded_Images.Images[fbr].Density = _Uploaded_Images.Images[fbr].NumberOfFibres_Calculated / (_Uploaded_Images.Length_mm / 10);

                // Calculate Min and Max spacing between detected lines
                PFA.Min_Max_Spacing(_Uploaded_Images.Images[fbr], _Uploaded_Images.Length_mm, Original_Image.Width);
            }

            _Uploaded_Images.Total_Evaluation = PFA.Evaluate_Detection_Results(_Uploaded_Images.Images);

            return RedirectToPage("Results");
        }

        public ActionResult OnPostChangeParameters()
        {
            _Temp_Parameters.ID = ID;
            return RedirectToPage("ChangeParameters");
        }

        public ActionResult OnPostResetParameters()
        {
            _Temp_Parameters.Rotate = false;
            _Temp_Parameters.Length_mm = _Uploaded_Images.Length_mm;
            _Temp_Parameters.Min_Scale = _Uploaded_Images.Min_Scale;
            _Temp_Parameters.Max_Scale = _Uploaded_Images.Max_Scale;
            _Temp_Parameters.ClipLimit = _Uploaded_Images.ClipLimit;
            _Temp_Parameters.Apply_CLAHE = _Uploaded_Images.Apply_CLAHE;
            _Temp_Parameters.Apply_HistEqu = _Uploaded_Images.Apply_HistEqu;
            _Temp_Parameters.Channel = _Uploaded_Images.Channel;
            _Temp_Parameters.ksize = _Uploaded_Images.ksize;
            _Temp_Parameters.TilesGridSize = _Uploaded_Images.TilesGridSize;
            _Temp_Parameters.DetectionBorder = _Uploaded_Images.DetectionBorder;

            return OnPostShowDetails(ID);
        }

        public ActionResult OnPostApplyChangesToAll()
        {
            //if (_Temp_Parameters.Rotate)
            //    _Uploaded_Images.Orientation = false;
            //else
            //    _Uploaded_Images.Orientation = true;

            _Uploaded_Images.Length_mm = _Temp_Parameters.Length_mm;
            _Uploaded_Images.Min_Scale = _Temp_Parameters.Min_Scale;
            _Uploaded_Images.Max_Scale = _Temp_Parameters.Max_Scale;
            _Uploaded_Images.ClipLimit = _Temp_Parameters.ClipLimit;
            _Uploaded_Images.Apply_CLAHE = _Temp_Parameters.Apply_CLAHE;
            _Uploaded_Images.Apply_HistEqu = _Temp_Parameters.Apply_HistEqu;
            _Uploaded_Images.Channel = _Temp_Parameters.Channel;
            _Uploaded_Images.ksize = _Temp_Parameters.ksize;
            _Uploaded_Images.TilesGridSize = _Temp_Parameters.TilesGridSize;
            _Uploaded_Images.DetectionBorder = _Temp_Parameters.DetectionBorder;

            Papyri_Fibres PFA = new Papyri_Fibres();

            string Fibres_Folder = "Fibres";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string Fibres_Path = Path.Combine(webRootPath, Fibres_Folder);
            if (!Directory.Exists(Fibres_Path))
            {
                Directory.CreateDirectory(Fibres_Path);
            }

            // Check whether there is at least one file with a valid extension
            IEnumerable<string> ValidFiles = Directory.EnumerateFiles(Fibres_Path, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".jpeg") ||
            s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".tiff") || s.ToLower().EndsWith(".png")
            || s.ToLower().EndsWith(".bmp"));

            for (int fbr = 0; fbr < ValidFiles.Count(); fbr++)
            {
                string Current_Image_Path = ValidFiles.ElementAt(fbr);
                Mat Original_Image = new Mat(Current_Image_Path, ImreadModes.Color);

                // Rotate if Horizontal orientation is selected
                if (_Temp_Parameters.Rotate)
                {
                    Cv2.Rotate(Original_Image, Original_Image, RotateFlags.Rotate90Clockwise);
                }
                Original_Image.ImWrite(Current_Image_Path);

                // Detect peaks in histogram
                FibreInfo Current_Fibre_Info = new FibreInfo();
                Current_Fibre_Info = PFA.Line_Detector(Original_Image, _Uploaded_Images);
                _Uploaded_Images.Images[fbr].NumberOfFibres_Calculated = Current_Fibre_Info.Peaks.Count();
                _Uploaded_Images.Images[fbr].Peaks = Current_Fibre_Info.Peaks;
                _Uploaded_Images.Images[fbr].Density = _Uploaded_Images.Images[fbr].NumberOfFibres_Calculated / (_Uploaded_Images.Length_mm / 10);

                // Calculate Min and Max spacing between detected lines
                PFA.Min_Max_Spacing(_Uploaded_Images.Images[fbr], _Uploaded_Images.Length_mm, Original_Image.Width);
            }

            _Uploaded_Images.Total_Evaluation = PFA.Evaluate_Detection_Results(_Uploaded_Images.Images);

            return RedirectToPage("Results");
        }

        public ActionResult OnPostChangeImage()
        {
            ViewData["ImageSource"] = ImageSource;
            ViewData["OrgImg"] = Current_File_Name;
            ViewData["OrgImg_WithoutExtension"] = Path.GetFileNameWithoutExtension(Current_File_Name);

            if (ImageSource == "Processed")
            {
                ViewData["ImageSource"] = "Original";
                ImageSource = "Original";
            }
            else if (ImageSource == "Original")
            {
                ViewData["ImageSource"] = "Processed";
                ImageSource = "Processed";
            }

            return Page();
        }

        public ActionResult OnPostDownloadAllImages()
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            string Temp_Folder = "Temp";         
            string TempPath = Path.Combine(webRootPath, Temp_Folder);       
            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }
            Directory.EnumerateFiles(TempPath).ToList().ForEach(f => System.IO.File.Delete(f));

            string ImageCombination_Folder = "ImageCombination";
            string ImageCombination_Path = Path.Combine(webRootPath, ImageCombination_Folder);
            if (!Directory.Exists(ImageCombination_Path))
            {
                Directory.CreateDirectory(ImageCombination_Path);
            }
            Directory.EnumerateFiles(ImageCombination_Path).ToList().ForEach(f => System.IO.File.Delete(f));
            string Preprocessed_Folder = "Preprocessed";
            string Pattern_Folder = "Pattern";
            string Histogram_Folder = "Histogram";
            string Enhanced_Folder = "Enhanced";
            string Preprocessed_Path = Path.Combine(webRootPath, Preprocessed_Folder);
            string Pattern_Path = Path.Combine(webRootPath, Pattern_Folder);
            string Histogram_Path = Path.Combine(webRootPath, Histogram_Folder);
            string Enhanced_Path = Path.Combine(webRootPath, Enhanced_Folder);

            if (!Directory.Exists(Preprocessed_Path))
            {
                Directory.CreateDirectory(Preprocessed_Path);
            }

            if (!Directory.Exists(Pattern_Path))
            {
                Directory.CreateDirectory(Pattern_Path);
            }

            if (!Directory.Exists(Histogram_Path))
            {
                Directory.CreateDirectory(Histogram_Path);
            }
            if (!Directory.Exists(Enhanced_Path))
            {
                Directory.CreateDirectory(Enhanced_Path);
            }

            string Preprocessed_Path_Full = Path.Combine(Preprocessed_Path, Current_File_Name);
            string Enhanced_Path_Full = Path.Combine(Enhanced_Path, Current_File_Name);
            string Histogram_Path_Full = Path.Combine(Histogram_Path, Current_File_Name);
            string Pattern_Path_Full = Path.Combine(Pattern_Path, Current_File_Name);

            Mat Preprocessed = new Mat(Preprocessed_Path_Full, ImreadModes.Color);
            Mat Enhanced = new Mat(Enhanced_Path_Full, ImreadModes.Color);
            Mat Histogram = new Mat(Histogram_Path_Full, ImreadModes.Color);
            Mat Pattern = new Mat(Pattern_Path_Full, ImreadModes.Color);

            List<Mat> ConcatenatedImages = new List<Mat>();
            Mat WhiteSpace = new Mat(new Size(Preprocessed.Width, 5), Preprocessed.Type());
            WhiteSpace.SetTo(new Scalar(100,255,100));

            Mat Preprocessed_Enhanced_Histogram_Pattern = new Mat();
            ConcatenatedImages.Add(Preprocessed);
            ConcatenatedImages.Add(WhiteSpace);
            ConcatenatedImages.Add(Enhanced);
            ConcatenatedImages.Add(WhiteSpace);
            ConcatenatedImages.Add(Histogram);
            ConcatenatedImages.Add(WhiteSpace);
            ConcatenatedImages.Add(Pattern);
            Cv2.VConcat(ConcatenatedImages, Preprocessed_Enhanced_Histogram_Pattern);
            ConcatenatedImages.Clear();
            
            string Preprocessed_Enhanced_Histogram_Pattern_Path = Path.Combine(ImageCombination_Path, "Original_Enhanced_Histogram_Pattern_" + Current_File_Name);
            bool ImgWriting = Preprocessed_Enhanced_Histogram_Pattern.ImWrite(Preprocessed_Enhanced_Histogram_Pattern_Path);

            //using (var window = new Window("window", image: Preprocessed_Enhanced_Histogram_Pattern, flags: WindowMode.Normal))
            //{
            //    Cv2.WaitKey();
            //}
            //Window.DestroyAllWindows();

            Mat Preprocessed_Histogram_Pattern = new Mat();
            ConcatenatedImages.Add(Preprocessed);
            ConcatenatedImages.Add(WhiteSpace);
            ConcatenatedImages.Add(Histogram);
            ConcatenatedImages.Add(WhiteSpace);
            ConcatenatedImages.Add(Pattern);
            Cv2.VConcat(ConcatenatedImages, Preprocessed_Histogram_Pattern);
            ConcatenatedImages.Clear();
            string Preprocessed_Histogram_Pattern_Path = Path.Combine(ImageCombination_Path, "Original_Histogram_Pattern_" + Current_File_Name);
            Preprocessed_Histogram_Pattern.ImWrite(Preprocessed_Histogram_Pattern_Path);

            Mat Preprocessed_Pattern = new Mat();
            ConcatenatedImages.Add(Preprocessed);
            ConcatenatedImages.Add(WhiteSpace);
            ConcatenatedImages.Add(Pattern);
            Cv2.VConcat(ConcatenatedImages, Preprocessed_Pattern);
            ConcatenatedImages.Clear();
            string Preprocessed_Pattern_Path = Path.Combine(ImageCombination_Path, "Original_Pattern_" + Current_File_Name);
            Preprocessed_Pattern.ImWrite(Preprocessed_Pattern_Path);

            Mat Enhanced_Histogram_Pattern = new Mat();
            ConcatenatedImages.Add(Enhanced);
            ConcatenatedImages.Add(WhiteSpace);
            ConcatenatedImages.Add(Histogram);
            ConcatenatedImages.Add(WhiteSpace);
            ConcatenatedImages.Add(Pattern);
            Cv2.VConcat(ConcatenatedImages, Enhanced_Histogram_Pattern);
            ConcatenatedImages.Clear();
            string Enhanced_Histogram_Pattern_Path = Path.Combine(ImageCombination_Path, "Enhanced_Histogram_Pattern_" + Current_File_Name);
            Enhanced_Histogram_Pattern.ImWrite(Enhanced_Histogram_Pattern_Path);

            Mat Enhanced_Pattern = new Mat();
            ConcatenatedImages.Add(Enhanced);
            ConcatenatedImages.Add(WhiteSpace);
            ConcatenatedImages.Add(Pattern);
            Cv2.VConcat(ConcatenatedImages, Enhanced_Pattern);
            ConcatenatedImages.Clear();
            string Enhanced_Pattern_Path = Path.Combine(ImageCombination_Path, "Enhanced_Pattern_" + Current_File_Name);
            Enhanced_Pattern.ImWrite(Enhanced_Pattern_Path);

            Mat Enhanced_Histogram = new Mat();
            ConcatenatedImages.Add(Enhanced);
            ConcatenatedImages.Add(WhiteSpace);
            ConcatenatedImages.Add(Histogram);
            Cv2.VConcat(ConcatenatedImages, Enhanced_Histogram);
            ConcatenatedImages.Clear();
            string Enhanced_Histogram_Path = Path.Combine(ImageCombination_Path, "Enhanced_Histogram_" + Current_File_Name);
            Enhanced_Histogram.ImWrite(Enhanced_Histogram_Path);

            Mat Preprocessed_Histogram = new Mat();
            ConcatenatedImages.Add(Preprocessed);
            ConcatenatedImages.Add(WhiteSpace);
            ConcatenatedImages.Add(Histogram);
            Cv2.VConcat(ConcatenatedImages, Preprocessed_Histogram);
            ConcatenatedImages.Clear();
            string Preprocessed_Histogram_Path = Path.Combine(ImageCombination_Path, "Original_Histogram_" + Current_File_Name);
            Preprocessed_Histogram.ImWrite(Preprocessed_Histogram_Path);

            Mat Preprocessed_Enhanced = new Mat();
            ConcatenatedImages.Add(Preprocessed);
            ConcatenatedImages.Add(WhiteSpace);
            ConcatenatedImages.Add(Enhanced);
            Cv2.VConcat(ConcatenatedImages, Preprocessed_Enhanced);
            ConcatenatedImages.Clear();
            string Preprocessed_Enhanced_Path = Path.Combine(ImageCombination_Folder, "Original_Enhanced_" + Current_File_Name);
            Preprocessed_Enhanced.ImWrite(Preprocessed_Enhanced_Path);

            // create a new archive      
            string ImageName = Path.GetFileNameWithoutExtension(Current_File_Name);
            string archive = TempPath + "\\ImageCombinations_" + ImageName + ".zip";
            ZipFile.CreateFromDirectory(ImageCombination_Path, archive);

            return File("/Temp/ImageCombinations_" + ImageName + ".zip", "application/zip", "ImageCombinations_" + ImageName + ".zip");
        }
    }
}
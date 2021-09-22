using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Fibers_Density_Estimater.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using OpenCvSharp;

namespace Fibers_Density_Estimater.Pages
{
    [DisableRequestSizeLimit]
    public class IndexModel : PageModel
    {
        public IEnumerable<string> ValidFiles { get; set; }
        public float Length { get; set; } = 20;
        public bool Orientation { get; set; } = true;


        // needed to get the wwwroot directory
        private IWebHostEnvironment _hostingEnvironment;
        private readonly Uploaded_Images _Uploaded_Images;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, IWebHostEnvironment hostingEnvironment, Uploaded_Images Uploaded_Images)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _Uploaded_Images = Uploaded_Images;
        }

        public void OnGet()
        {
            string Fibres_Folder = "Fibres";
            string Preprocessed_Folder = "Preprocessed";
            string Pattern_Folder = "Pattern";
            string Histogram_Folder = "Histogram";
            string Temp_Folder = "Temp";
            string Results_Folder = "Results";
            string Enhanced_Folder = "Enhanced";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string TempPath = Path.Combine(webRootPath, Temp_Folder);
            string Results_Path = Path.Combine(webRootPath, Results_Folder);        
            string Preprocessed_Path = Path.Combine(webRootPath, Preprocessed_Folder);
            string Pattern_Path = Path.Combine(webRootPath, Pattern_Folder);
            string Histogram_Path = Path.Combine(webRootPath, Histogram_Folder);
            string Fibres_Path = Path.Combine(webRootPath, Fibres_Folder);
            string Enhanced_Path = Path.Combine(webRootPath, Enhanced_Folder);
            string ImageCombination_Folder = "ImageCombination";
            string ImageCombination_Path = Path.Combine(webRootPath, ImageCombination_Folder);
            if (!Directory.Exists(ImageCombination_Path))
            {
                Directory.CreateDirectory(ImageCombination_Path);
            }
            Directory.EnumerateFiles(ImageCombination_Path).ToList().ForEach(f => System.IO.File.Delete(f));
            if (!Directory.Exists(Fibres_Path))
            {
                Directory.CreateDirectory(Fibres_Path);
            }
            Directory.EnumerateFiles(Fibres_Path).ToList().ForEach(f => System.IO.File.Delete(f));
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
            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }
            Directory.EnumerateFiles(TempPath).ToList().ForEach(f => System.IO.File.Delete(f));
            if (!Directory.Exists(Results_Path))
            {
                Directory.CreateDirectory(Results_Path);
            }
            Directory.EnumerateFiles(Results_Path).ToList().ForEach(f => System.IO.File.Delete(f));
            if (!Directory.Exists(Enhanced_Path))
            {
                Directory.CreateDirectory(Enhanced_Path);
            }
            Directory.EnumerateFiles(Enhanced_Path).ToList().ForEach(f => System.IO.File.Delete(f));

            _Uploaded_Images.Images.Clear();
        }

        public IActionResult OnPostUpload(IFormFile[] files)
        {
            string Fibres_Folder = "Fibres";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string Fibres_Path = Path.Combine(webRootPath, Fibres_Folder);
            if (!Directory.Exists(Fibres_Path))
            {
                Directory.CreateDirectory(Fibres_Path);
            }
            Directory.EnumerateFiles(Fibres_Path).ToList().ForEach(f => System.IO.File.Delete(f));

            if (files != null && files.Count() > 0)
            {
                // Iterate through uploaded files array
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // Extract file name from whatever was posted by browser
                        var fileName = System.IO.Path.GetFileName(file.FileName);

                        // If file with same name exists delete it
                        if (System.IO.File.Exists(fileName))
                        {
                            System.IO.File.Delete(fileName);
                        }

                        // Create new local file and copy contents of uploaded file
                        using (var localFile = System.IO.File.OpenWrite(Fibres_Path + "/" + fileName))
                        using (var uploadedFile = file.OpenReadStream())
                        {
                            uploadedFile.CopyTo(localFile);
                        }
                    }
                }

                // Check whether there is at least one file with a valid extension
                ValidFiles = Directory.EnumerateFiles(Fibres_Path, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".jpeg") ||
                s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".tiff") || s.ToLower().EndsWith(".png")
                || s.ToLower().EndsWith(".bmp"));
                ViewData["ValidFiles"] = ValidFiles.Count();
                if (ValidFiles.Count() <= 0)
                {
                    ViewData["Message"] = "Uploaded files are not valid!";
                    ViewData["Colour"] = "Red";
                    return Page();
                }

                //return RedirectToPage("Results");
                ViewData["Message"] = "Files uploaded successfully";
                ViewData["Upload"] = "Success";
                return Page();
            }
            ViewData["Message"] = "Uploaded files are not valid";
            ViewData["Colour"] = "Red";
            return Page();
        }

        public ActionResult OnPostAnalyse(float Length, bool Orientation)
        {
            string Fibres_Folder = "Fibres";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string Fibres_Path = Path.Combine(webRootPath, Fibres_Folder);
            if (!Directory.Exists(Fibres_Path))
            {
                Directory.CreateDirectory(Fibres_Path);
            }

            Papyri_Fibres PFA = new Papyri_Fibres();
            _Uploaded_Images.Images = PFA.Read_Fibre_Images(Fibres_Path);
            _Uploaded_Images.Images = PFA.Populate_Fibre_Info(_Uploaded_Images.Images, Length);
            _Uploaded_Images.Orientation = Orientation;
            _Uploaded_Images.Length_mm = Length;

            // Check whether there is at least one file with a valid extension
            ValidFiles = Directory.EnumerateFiles(Fibres_Path, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".jpeg") ||
            s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".tif") || s.ToLower().EndsWith(".tiff") || s.ToLower().EndsWith(".png")
            || s.ToLower().EndsWith(".bmp"));

            for (int fbr = 0; fbr < ValidFiles.Count(); fbr++)
            {
                string Current_Image_Path = ValidFiles.ElementAt(fbr);
                
                // Read the image
                Mat Original_Image = new Mat(Current_Image_Path, ImreadModes.Color);

                // If the extension is "Tiff", delete the image as "jpg"
                if(Path.GetExtension(Current_Image_Path).ToLower() == ".tif" || Path.GetExtension(Current_Image_Path).ToLower() == ".tiff")
                {
                    System.IO.File.Delete(Current_Image_Path);
                    Current_Image_Path = Path.ChangeExtension(Current_Image_Path, ".jpg");
                    Original_Image.ImWrite(Current_Image_Path);
                    _Uploaded_Images.Images[fbr].FilePath = Current_Image_Path;
                    _Uploaded_Images.Images[fbr].FileName = System.IO.Path.GetFileName(Current_Image_Path);
                }
                
                // Rotate if Horizontal orientation is selected
                if (!_Uploaded_Images.Orientation)
                {
                    Cv2.Rotate(Original_Image, Original_Image, RotateFlags.Rotate90Clockwise);
                    Original_Image.ImWrite(Current_Image_Path);
                }
                
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
    }
}

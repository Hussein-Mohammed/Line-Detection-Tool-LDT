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
using OpenCvSharp;

namespace Fibers_Density_Estimater.Pages
{
    public class ResultsModel : PageModel
    {
        private readonly Uploaded_Images _Uploaded_Images;
        private IWebHostEnvironment _hostingEnvironment;
        public ResultsModel(Uploaded_Images Uploaded_Images, IWebHostEnvironment hostingEnvironment)
        {
            _Uploaded_Images = Uploaded_Images;
            _hostingEnvironment = hostingEnvironment;
        }

        public void OnGet()
        {
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
        }

        public ActionResult OnPostDownloadResults()
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            string Temp_Folder = "Temp";
            string Results_Folder = "Results";
            string TempPath = Path.Combine(webRootPath, Temp_Folder);
            string Results_Path = Path.Combine(webRootPath, Results_Folder);
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

            var Results_csv = new System.Text.StringBuilder();
            Results_csv.AppendLine("Image,Fibres_GT,Fibres_Estimated,Density_Estimated,Spacing_Min_Estimated,Spacing_Max_Estimated,Accuracy");

            foreach (var img in _Uploaded_Images.Images)
            {
                string Current_Evaluation;
                if (img.GT)
                {
                    Current_Evaluation = Math.Round(img.Evaluation, 2).ToString();
                }
                else
                {
                    Current_Evaluation = "GT Not Provided";
                }

                Results_csv.AppendLine(img.FileName + "," + Convert.ToString(img.NumberOfFibres_GT) + "," + Convert.ToString(img.NumberOfFibres_Calculated)
                    + "," + Convert.ToString(Math.Round(img.Density,2)) + "," + Convert.ToString(Math.Round(img.Min_Spacing,2)) + "," + Convert.ToString(Math.Round(img.Max_Spacing,2))
                    + "," + Current_Evaluation);
            }
           
            if(_Uploaded_Images.Total_Evaluation != 0)
            {
                Results_csv.AppendLine("");
                string TotalEvaluation = Math.Round(_Uploaded_Images.Total_Evaluation, 2).ToString();
                Results_csv.AppendLine("Total Evaluation," + TotalEvaluation);
            }

            System.IO.File.WriteAllText(Results_Path + "/Results.csv", Results_csv.ToString());
            Results_csv.Clear();

            // create a new archive          
            string archive = TempPath + "\\Results.zip";
            ZipFile.CreateFromDirectory(Results_Path, archive);

            return File("/Temp/Results.zip", "application/zip", "Results.zip");
        }

        public ActionResult OnPostDownloadResultsDe()
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            string Temp_Folder = "Temp";
            string Results_Folder = "Results";
            string TempPath = Path.Combine(webRootPath, Temp_Folder);
            string Results_Path = Path.Combine(webRootPath, Results_Folder);
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

            var Results_csv = new System.Text.StringBuilder();
            Results_csv.AppendLine("Image;Fibres_GT;Fibres_Estimated;Density_Estimated;Spacing_Min_Estimated;Spacing_Max_Estimated;Accuracy");

            foreach (var img in _Uploaded_Images.Images)
            {
                string Current_Evaluation;
                if (img.GT)
                {
                    Current_Evaluation = Math.Round(img.Evaluation, 2).ToString();
                }
                else
                {
                    Current_Evaluation = "GT Not Provided";
                }

                Results_csv.AppendLine(img.FileName + ";" + Convert.ToString(img.NumberOfFibres_GT) + ";" + Convert.ToString(img.NumberOfFibres_Calculated)
                    + ";" + Convert.ToString(Math.Round(img.Density, 2)) + ";" + Convert.ToString(Math.Round(img.Min_Spacing, 2)) + ";" + Convert.ToString(Math.Round(img.Max_Spacing, 2))
                    + ";" + Current_Evaluation);
            }

            if (_Uploaded_Images.Total_Evaluation != 0)
            {
                Results_csv.AppendLine("");
                string TotalEvaluation = Math.Round(_Uploaded_Images.Total_Evaluation, 2).ToString();
                Results_csv.AppendLine("Total Evaluation;" + TotalEvaluation);
            }

            System.IO.File.WriteAllText(Results_Path + "/Results_DE.csv", Results_csv.ToString());
            Results_csv.Clear();

            // create a new archive          
            string archive = TempPath + "\\Results_DE.zip";
            ZipFile.CreateFromDirectory(Results_Path, archive);

            return File("/Temp/Results_DE.zip", "application/zip", "Results_DE.zip");
        }
    }
}
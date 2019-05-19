using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Upload_Download_3.Models;

namespace Upload_Download_3.Controllers
{
    public class FileUploadController : Controller
    {
        // GET: FileUpload
        public ActionResult Index()
        {
            var today = DateTime.Now.ToShortDateString();
            ViewBag.Today = today;

            var items = GetFiles();
            return View(items);
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            var updatedItems = GetFiles();

            var today = DateTime.Now;
            ViewBag.Today = today;

            var deadline = Convert.ToDateTime("05/30/2019");
            if(today > deadline)
            {
                ViewBag.Deadline = "The closure date is up! New upload is disabled!";
                return View(updatedItems);
            }

            var items = GetFiles();
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpeg", ".jpg", ".png", ".gif" };
                    var checkExtension = Path.GetExtension(file.FileName).ToLower();
                    
                    if (!allowedExtensions.Contains(checkExtension))
                    {
                        ViewBag.Message = "Invalid file type!";
                        return View(items);
                    }
                    
                    string filename = String.Concat(Path.GetFileNameWithoutExtension(file.FileName), "-", "MyFile", Path.GetExtension(file.FileName).ToLower());
                    string path = Path.Combine(Server.MapPath("~/Files"), Path.GetFileName(filename));
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully!";
                }
                catch(Exception ex)
                {
                    ViewBag.Message = "Error: " + ex.Message.ToString();
                }
            }
            else
            {
                ViewBag.Message = "You have not specified a file!";
            }

            updatedItems = GetFiles();
            return View(updatedItems);
        }

        public FileResult Download(string fileName)
        {
            var FileVirtualPath = "~/Files/" + fileName;
            return File(FileVirtualPath, "application/force- download", Path.GetFileName(FileVirtualPath));
        }

        private List<string> GetFiles()
        {
            var dir = new DirectoryInfo(Server.MapPath("~/Files"));
            FileInfo[] fileNames = dir.GetFiles("*.*");

            List<string> items = new List<string>();
            foreach(var file in fileNames)
            {
                items.Add(file.Name);
            }
            return items;
        }

        public FileResult Zip()
        {
            string[] filePaths = Directory.GetFiles(Server.MapPath("~/Files/"));
            List<FileModel> files = new List<FileModel>();
            foreach (string filePath in filePaths)
            {
                files.Add(new FileModel()
                {
                    FileName = Path.GetFileName(filePath),
                    FilePath = filePath
                });
            }

            using (ZipFile zip = new ZipFile())
            {
                zip.AlternateEncodingUsage = ZipOption.AsNecessary;
                zip.AddDirectoryByName("Files");
                foreach (FileModel file in files)
                {
                        zip.AddFile(file.FilePath, "Files");
                }
                string zipName = String.Format("Zip_{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    zip.Save(memoryStream);
                    return File(memoryStream.ToArray(), "application/zip", zipName);
                }
            }
        }
    }
}
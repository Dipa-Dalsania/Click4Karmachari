using Imagekit.Models;
using Imagekit.Sdk;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ClickKarmachari.Models
{
    public class CommonResponce
    {
        public bool EnableError { get; set; }
        public string ErrorTitle { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class ImageKiT_ImageProcess
    {
        static String ImageKit_PublicKey = ConfigurationManager.AppSettings["ImageKit_PublicKey"].ToString();
        static String ImageKit_PrivateKey = ConfigurationManager.AppSettings["ImageKit_PrivateKey"].ToString();
        static String ImageKit_URL = ConfigurationManager.AppSettings["ImageKit_URL"].ToString();
        static String ImageKit_Folder = ConfigurationManager.AppSettings["ImageKit_Folder"].ToString();
        static ImagekitClient imagekit = new ImagekitClient(ImageKit_PublicKey, ImageKit_PrivateKey, ImageKit_URL);
     
        public async Task<object> DeleteFile(string filename, string folderFrom)
        {
            try
            {
                GetFileListRequest model = new GetFileListRequest
                {
                    Name = "filename",
                    Type = "file",
                    Limit = 10,
                    Skip = 0,
                    Sort = "ASC_CREATED",
                    FileType = "image",
                    Path = $"{ImageKit_Folder}/{folderFrom}/"
                };
                ResultList res = imagekit.GetFileListRequest(model);

                if (res?.FileList?.Any() == true) // Safer null checks and using Any() for readability
                {
                    string fileId = res.FileList.First().fileId;
                    await imagekit.DeleteFileAsync(fileId);
                }
            }
            catch { }
            return null;
        }
        public async Task<bool> DeleteFileAsync(string filename, string folderFrom)
        {
            try
            {
                var fileListRequest = new GetFileListRequest
                {
                    Name = filename,
                    Type = "file",
                    Path = $"{ImageKit_Folder}/{folderFrom}/",
                    Limit = 1
                };

                var res = imagekit.GetFileListRequest(fileListRequest);

                var fileId = res?.FileList?.FirstOrDefault()?.fileId;
                if (!string.IsNullOrEmpty(fileId))
                {
                    await imagekit.DeleteFileAsync(fileId);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed
            }

            return false;
        }
        public async Task<bool> DeleteFolderAsync(string folderFrom)
        {
            try
            {
                string path = $"{ImageKit_Folder}/{folderFrom}/";
                var hasMoreFiles = true;
                int skip = 0;
                int batchSize = 100;

                while (hasMoreFiles)
                {
                    var fileListRequest = new GetFileListRequest
                    {
                        Path = path,
                        Limit = batchSize,
                        Skip = skip
                    };

                    var res = imagekit.GetFileListRequest(fileListRequest);

                    if (res?.FileList?.Count > 0)
                    {
                        foreach (var file in res.FileList)
                        {
                            await imagekit.DeleteFileAsync(file.fileId);
                        }

                        skip += batchSize;
                    }
                    else
                    {
                        hasMoreFiles = false;
                    }
                }

                // Now delete the folder via REST API since SDK does not support it
                using (var httpClient = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Delete, $"https://api.imagekit.io/v1/folder?folderPath={Uri.EscapeDataString(path)}");
                    var byteArray = Encoding.ASCII.GetBytes($"{ImageKit_PublicKey}:{ImageKit_PrivateKey}");
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    var response = await httpClient.SendAsync(request);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed
            }

            return false;
        }
        public CommonResponce UploadFile(string folderFrom, HttpPostedFileBase file, string oldfilename, int size = 20)
        {

            int MaxContentLength = 1024 * 1024 * size;
            //var allowedExtensions = new[] { ".jpg", ".png", ".jpeg", ".gif" };
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff", ".svg" };

            var fileName = Path.GetFileName(file.FileName);
            var ext = Path.GetExtension(file.FileName);

            if (!allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
            {
                return new CommonResponce { EnableError = false, ErrorTitle = "Failure", ErrorMsg = "You have select Invalid File, allowed only jpg, png, jpeg." };
            }
            if (file.ContentLength > MaxContentLength)
            {
                return new CommonResponce { EnableError = false, ErrorTitle = "Failure", ErrorMsg = "Your file is too large, maximum allowed size is: " + MaxContentLength + " MB." };
            }

            byte[] FILEBYT = new byte[file.ContentLength];
            file.InputStream.Read(FILEBYT, 0, file.ContentLength);
            string uniqueId = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + Guid.NewGuid().ToString("N").Substring(0, 8); // Keep the GUID short
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
            if (fileNameWithoutExt.Length > 20)
            {
                fileName = fileNameWithoutExt.Substring(0, 15) + uniqueId + ext;
            }
            else
            {
                fileName = fileNameWithoutExt + uniqueId + ext;
            }

            //if (fileName.ToString().Length > 20) { fileName = fileName.Substring(0, 15).ToString() + ext; }

            FileCreateRequest ob = new FileCreateRequest
            {
                file = FILEBYT,
                fileName = fileName,
                folder = $"{ImageKit_Folder}/{folderFrom}/",
                
            };
            Result resp2 = imagekit.Upload(ob);
            if (resp2.HttpStatusCode != 200)
            {
                return new CommonResponce { EnableError = false, ErrorTitle = "Failure", ErrorMsg = "Your Image Uploadng Failed, Please Try Again." };
            }

            return new CommonResponce { EnableError = true, ErrorTitle = "Success", ErrorMsg = resp2.name.ToString() };
        }
        public CommonResponce UploadFile(string folderFrom, byte[] FILEBYT, string fileName, string oldfilename, int size = 5)
        {
            var ext = Path.GetExtension(fileName);
            string uniqueId = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + Guid.NewGuid().ToString("N").Substring(0, 8); // Keep the GUID short
            if (fileName.Length > 20)
            {
                fileName = fileName.Substring(0, 15) + uniqueId + ext;
            }
            else
            {
                fileName = fileName + uniqueId + ".xlsx";
            }

            FileCreateRequest ob = new FileCreateRequest
            {
                file = FILEBYT,
                fileName = fileName,
                folder = $"{ImageKit_Folder}/{folderFrom}/"
            };
            Result resp2 = imagekit.Upload(ob);
            if (resp2.HttpStatusCode != 200)
            {
                return new CommonResponce { EnableError = false, ErrorTitle = "Failure", ErrorMsg = "Your Image Uploadng Failed, Please Try Again." };
            }

            Thread t1 = new Thread(new ThreadStart(async () => await new ImageKiT_ImageProcess().DeleteFile(oldfilename, folderFrom)));
            t1.Start();

            return new CommonResponce { EnableError = true, ErrorTitle = "Success", ErrorMsg = resp2.name.ToString() };
        }
        public CommonResponce UploadDocument(string folderFrom, HttpPostedFileBase file, string oldfilename, int size = 5)
        {
            int MaxContentLength = 1024 * 1024 * size;
            var allowedExtensions = new[] { ".Jpg", ".JPG", ".png", ".jpg", ".jpeg", ".pdf", ".XLS", ".XLSX", ".DOC", ".PPT", ".xls", ".xlsx", ".txt", ".docx", ".doc", ".pptx", ".ppt", ".potx", ".ppsx" };
            var fileName = Path.GetFileName(file.FileName);
            var ext = Path.GetExtension(file.FileName);
            if (!allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
            {
                return new CommonResponce { EnableError = false, ErrorTitle = "Failure", ErrorMsg = "You have select Invalid File, allowed only jpg, png, jpeg, pdf." };
            }
            if (file.ContentLength > MaxContentLength)
            {
                return new CommonResponce { EnableError = false, ErrorTitle = "Failure", ErrorMsg = "Your file is too large, maximum allowed size is: " + MaxContentLength + " MB." };
            }

            byte[] FILEBYT = new byte[file.ContentLength];
            file.InputStream.Read(FILEBYT, 0, file.ContentLength);
            string uniqueId = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + Guid.NewGuid().ToString("N").Substring(0, 8); // Keep the GUID short
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
            if (fileNameWithoutExt.Length > 20)
            {
                fileName = fileNameWithoutExt.Substring(0, 15) + uniqueId + ext;
            }
            else
            {
                fileName = fileNameWithoutExt + uniqueId + ext;
            }


            FileCreateRequest ob = new FileCreateRequest
            {
                file = FILEBYT,
                fileName = fileName,
                folder = $"{ImageKit_Folder}/{folderFrom}/"
            };
            Result resp2 = imagekit.Upload(ob);
            if (resp2.HttpStatusCode != 200)
            {
                return new CommonResponce { EnableError = false, ErrorTitle = "Failure", ErrorMsg = "Your Image Uploadng Failed, Please Try Again." };
            }

            Thread t1 = new Thread(new ThreadStart(async () => await new ImageKiT_ImageProcess().DeleteFile(oldfilename, folderFrom)));
            t1.Start();

            return new CommonResponce { EnableError = true, ErrorTitle = "Success", ErrorMsg = resp2.name.ToString() };
        }
        static bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri result) &&
                   (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
        static string IdentifyMyLinkType(string url)
        {
            // Enhanced Dropbox pattern to catch all possible formats including scl/fi
            if (Regex.IsMatch(url, @"^(https?:\/\/)?(www\.|dl\.)?(dropbox\.com|dropboxusercontent\.com).*"))
            {
                return "Dropbox";
            }
            else if (Regex.IsMatch(url, @"^(https?:\/\/)?(www\.)?(drive\.google\.com\/(file\/d\/|open\?id=|uc\?id=|uc\?export=download&id=|drive\/folders\/|thumbnail\?id=))|(lh3\.googleusercontent\.com\/)"))
            {
                return "Google Drive";
            }
            else if (Regex.IsMatch(url, @"^(https?:\/\/)?.*\.(jpg|jpeg|png|webp)(\?.*)?$", RegexOptions.IgnoreCase))
            {
                return "Direct Image Link";
            }
            else
            {
                return "Unknown";
            }
        }
        static string ExtractFileId(string googleDriveUrl)
        {
            var match = Regex.Match(googleDriveUrl, @"(?:/d/|id=|thumbnail\?id=|uc\?id=|lh3\.googleusercontent\.com/)([a-zA-Z0-9_-]+)");
            return match.Success ? match.Groups[1].Value : null;
        }
        static string ExtractConfirmationToken(string responseBody)
        {
            var match = Regex.Match(responseBody, @"confirm=([a-zA-Z0-9_-]+)");
            return match.Success ? match.Groups[1].Value : null;
        }
        static async Task<string> DownloadGoogleDriveFile(string googleDriveUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                string fileId = ExtractFileId(googleDriveUrl);
                if (string.IsNullOrEmpty(fileId))
                    throw new Exception("Invalid Google Drive URL.");

                string baseDownloadUrl = $"https://drive.google.com/uc?export=download&id={fileId}";

                using (var initialResponse = await client.GetAsync(baseDownloadUrl))
                {
                    if (!initialResponse.IsSuccessStatusCode)
                        throw new Exception("Failed to access Google Drive file.");

                    string responseBody = await initialResponse.Content.ReadAsStringAsync();

                    // Check if a confirmation token is required
                    string confirmationToken = ExtractConfirmationToken(responseBody);

                    if (!string.IsNullOrEmpty(confirmationToken))
                    {
                        // Rebuild the download URL with the confirmation token
                        string confirmedDownloadUrl = $"https://drive.google.com/uc?export=download&confirm={confirmationToken}&id={fileId}";
                        return confirmedDownloadUrl;
                    }
                    else
                    {
                        // No confirmation required, return the direct link
                        return baseDownloadUrl;
                    }
                }
            }
        }
        static string ConvertDropboxLink(string dropboxUrl)
        {
            if (dropboxUrl.Contains("?dl=0") || dropboxUrl.Contains("?dl=1"))
            {
                return dropboxUrl.Replace("?dl=0", "?raw=1").Replace("?dl=1", "?raw=1");
            }
            else if (dropboxUrl.Contains("&dl=0") || dropboxUrl.Contains("&dl=1"))
            {
                return dropboxUrl.Replace("&dl=0", "&raw=1").Replace("&dl=1", "&raw=1");
            }
            else
            {
                // If the link doesn't have ?dl=0 or &dl=0, just append ?raw=1 or &raw=1
                return dropboxUrl.Contains("?") ? dropboxUrl + "&raw=1" : dropboxUrl + "?raw=1";
            }
        }
        static async Task<string> GenerateDirectDownloadLinkAsync(string url, string linkType)
        {
            switch (linkType)
            {
                case "Google Drive":
                    return await DownloadGoogleDriveFile(url);
                case "Dropbox":
                    return ConvertDropboxLink(url);
                case "Direct Image Link":
                    return url; // Already a direct link
                default:
                    return url;
            }
        }
        static string GetImageNameFromUrl(string url)
        {
            Uri uri = new Uri(url);
            string fileName = Path.GetFileName(uri.AbsolutePath);

            if (fileName.Contains("?"))
                fileName = fileName.Split('?')[0]; // Remove query parameters

            string uniqueId = DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName) ?? "UnknownFile";
            string fileExtension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(fileExtension))
                fileExtension = ".png";
            // Truncate filename for better readability
            if (fileNameWithoutExt.Length > 20)
                fileNameWithoutExt = fileNameWithoutExt.Substring(0, 15);

            return $"{fileNameWithoutExt}_{uniqueId}{fileExtension}";
        }
        public async Task<CommonResponce> UploadFileFromDropbox1AsyncWithDistributorIDinFileName(string folderFrom, string BrandID, string dropboxUrl, string oldFilename, int size = 5)
        {
            try
            {
                if (!IsValidUrl(dropboxUrl))
                {
                    return new CommonResponce { EnableError = false, ErrorTitle = "Error", ErrorMsg = "Not Valid URL" };
                }

                string tempurl = IdentifyMyLinkType(dropboxUrl.Trim());
                if (tempurl == "Unknown")
                {
                    return new CommonResponce { EnableError = false, ErrorTitle = "Error", ErrorMsg = "Unsupported URL format" };
                }

                string DirectLink = await GenerateDirectDownloadLinkAsync(dropboxUrl, tempurl);
                string uniquefilename = GetImageNameFromUrl(DirectLink);
                string localPath = Path.Combine("D:\\COMMONFOLDER", uniquefilename);
                using (HttpClient client = new HttpClient())
                {
                    // Add timeout and user agent
                    client.Timeout = TimeSpan.FromMinutes(5);
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(DirectLink);
                        Console.WriteLine($"Response Status: {response.StatusCode}");
                        foreach (var header in response.Headers)
                        {
                            Console.WriteLine($"Header: {header.Key} = {string.Join(", ", header.Value)}");
                        }
                        response.EnsureSuccessStatusCode();

                        using (FileStream fs = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await response.Content.CopyToAsync(fs);
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        return new CommonResponce
                        {
                            EnableError = false,
                            ErrorTitle = "Download Error",
                            ErrorMsg = $"Failed to download file: {ex.Message}"
                        };
                    }
                }

                // Verify file was downloaded and has content
                if (!File.Exists(localPath) || new FileInfo(localPath).Length == 0)
                {
                    return new CommonResponce
                    {
                        EnableError = false,
                        ErrorTitle = "Error",
                        ErrorMsg = "Downloaded file is empty or missing"
                    };
                }

                // Rest of your existing upload code...
                using (var fileStream = new FileStream(localPath, FileMode.Open, FileAccess.Read))
                {
                    var fileBytes = new byte[fileStream.Length];
                    await fileStream.ReadAsync(fileBytes, 0, (int)fileStream.Length);
                    FileCreateRequest request = new FileCreateRequest
                    {
                        file = fileBytes,
                        fileName = uniquefilename,
                        folder = $"{ImageKit_Folder}/{folderFrom}/{BrandID}"
                    };
                    Result response11 = imagekit.Upload(request);
                    try
                    {
                        File.Delete(localPath);
                    }
                    catch { /* Ignore cleanup errors */ }

                    if (response11.HttpStatusCode != 200)
                        return new CommonResponce
                        {
                            EnableError = false,
                            ErrorTitle = "Failure",
                            ErrorMsg = "Image upload failed. Please try again."
                        };

                    return new CommonResponce
                    {
                        EnableError = true,
                        ErrorTitle = "Success",
                        ErrorMsg = $"{BrandID}/{response11.name}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new CommonResponce
                {
                    EnableError = false,
                    ErrorTitle = "Error",
                    ErrorMsg = $"Exception: {ex.Message}"
                };
            }
        }
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows.Forms;

namespace LibraryFilesDownLoader
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //собираем адрес plm сервера
            string httpAdress = string.Format("http://{0}:{1}/{2}", Properties.Settings.Default.plmAdress,
                                                                    Properties.Settings.Default.plmPort,
                                                                    Properties.Settings.Default.plmHttpHandler);

            //Создаем http запрос
            HttpWebRequest httpRequest = WebRequest.Create(httpAdress) as HttpWebRequest;
            httpRequest.Method = "Post";
            httpRequest.ContentType = "application/x-www-form-urlencoded";
            httpRequest.Headers.Add(HttpRequestHeader.CacheControl, "no-cache, no-store, must-revalidate");
            httpRequest.Headers.Add(HttpRequestHeader.Pragma, "no-cache");
            httpRequest.Headers.Add(HttpRequestHeader.Expires, "0");

            //поток результата запроса
            Stream httpResultStream = null;

            try
            {
                httpResultStream = httpRequest.GetRequestStream();
            }
            catch (System.Net.WebException e)
            {
                MessageBox.Show(e.Message);
            }

            WebResponse response = httpRequest.GetResponse();
            httpResultStream = response.GetResponseStream();
            
            string fileName = string.Empty;
            string header = response.Headers.Get("Content-Disposition");
            if (header != null)
            {
                string templ = "filename=";
                int indOf = header.IndexOf(templ);
                if (indOf != -1)
                {
                    header = header.Substring(indOf+templ.Length);
                    templ = ";";
                    indOf = header.IndexOf(templ);
                    if (indOf != -1)                    
                        fileName = header.Substring(0, indOf).Trim();
                    else
                        fileName = header.Trim();
                }
            }

            if (fileName == string.Empty)
                MessageBox.Show("Не удалось получить архив.");
            else
            {
                try
                {
                   using (var folderBrowser = new FolderBrowserDialog())
                   {
                       folderBrowser.Description = "Выберите папку для сохранения...";

                       if (folderBrowser.ShowDialog() != DialogResult.OK)
                           return;

                       var basePath = folderBrowser.SelectedPath;
                       var folder = new DirectoryInfo(basePath);

                       //этот участок заменить на decompress
                       //вариант с добавленной System.IO.Compression.FileSystem
                       using (var zip = new System.IO.Compression.ZipArchive(httpResultStream, ZipArchiveMode.Read))
                       {
                           foreach (System.IO.Compression.ZipArchiveEntry entry in zip.Entries)
                           {
                               entry.ExtractToFile(Path.Combine(folder.FullName, entry.FullName), true);
                           }
                       }
                       MessageBox.Show(string.Format("Библиотечные файлы сохранены в {0}", folder.ToString()));

                       //вариант БЕЗ добавленной System.IO.Compression.FileSystem
                       /*using(var zip = new System.IO.Compression.ZipArchive(httpResultStream, ZipArchiveMode.Read))
                       {
                           foreach (System.IO.Compression.ZipArchiveEntry entry in zip.Entries)
                           {
                               
                               using (var file = File.Create(folder.FullName + "\\" + entry.FullName))
                               {
                                   Stream strm = entry.Open();
                                   CopyStream(strm, file);
                                   strm.Close();
                                   strm.Dispose();
                               }
                           }
                       }*/
                      
                       //вариант без распаковки архива
                       /*
                       using (var file = File.Create(folder.FullName + "\\" + fileName))
                       {
                           httpResultStream.CopyTo(file);
                       }*/
                   }
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            response.Close();
            response.Dispose();
        }

        private static void CopyStream(Stream src, Stream dest)
        {
            var buffer = new byte[8192];

            for (; ; )
            {
                int numRead = src.Read(buffer, 0, buffer.Length);

                if (numRead == 0)
                    break;

                dest.Write(buffer, 0, numRead);
            }
        }
    }
    
}

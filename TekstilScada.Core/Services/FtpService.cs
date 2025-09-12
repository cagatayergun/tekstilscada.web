// Services/FtpService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WinSCP; // WinSCP kütüphanesini kullanıyoruz

namespace TekstilScada.Services
{
    public class FtpService
    {
        private readonly string _host;
        private readonly string _username;
        private readonly string _password;
        private readonly int _port;

        public FtpService(string host, string username, string password)
        {
            var hostParts = host.Split(':');
            _host = hostParts[0];
            _port = hostParts.Length > 1 ? int.Parse(hostParts[1]) : 21;
            _username = username;
            _password = password;
        }

        private SessionOptions GetSessionOptions()
        {
            return new SessionOptions
            {
                Protocol = Protocol.Ftp,
                HostName = _host,
                PortNumber = _port,
                UserName = _username,
                Password = _password,
                FtpSecure = FtpSecure.None
            };
        }

        public async Task UploadFileAsync(string remoteFilePath, string fileContent)
        {
            var sessionOptions = GetSessionOptions();
            string tempFile = Path.GetTempFileName();
            try
            {
                // *******************************************************************
                // *** NİHAİ DÜZELTME: KARAKTER KODLAMASI UNICODE OLARAK DEĞİŞTİRİLDİ ***
                // *******************************************************************
                // HMI'ın dosyayı doğru okuyabilmesi ve boyutunun orijinal dosya ile
                // eşleşmesi için dosyayı ASCII yerine Unicode (UTF-16 LE) olarak yazıyoruz.
                await File.WriteAllTextAsync(tempFile, fileContent, Encoding.Unicode);

                TransferOptions transferOptions = new TransferOptions();
                transferOptions.FilePermissions = new FilePermissions { Octal = "777" };
                transferOptions.ResumeSupport.State = TransferResumeSupportState.On;
                transferOptions.TransferMode = TransferMode.Binary; // <-- BU SATIRI EKLEYİN
                await Task.Run(() =>
                {
                    using (var session = new Session())
                    {
                        session.Open(sessionOptions);
                        session.PutFiles(tempFile, remoteFilePath, false, transferOptions).Check();
                    }
                });
            }
            finally
            {
                // İşlem bitince geçici dosyayı mutlaka sil.
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        #region Mevcut Metotlar (Değişiklik Yok)
        public async Task<List<string>> ListDirectoryAsync(string remoteDirectory)
        {
            var fileList = new List<string>();
            var sessionOptions = GetSessionOptions();

            await Task.Run(() =>
            {
                using (var session = new Session())
                {
                    session.Open(sessionOptions);
                    RemoteDirectoryInfo directoryInfo = session.ListDirectory(remoteDirectory);

                    foreach (RemoteFileInfo fileInfo in directoryInfo.Files)
                    {
                        if (!fileInfo.IsDirectory)
                        {
                            fileList.Add(fileInfo.Name);
                        }
                    }
                }
            });

            return fileList;
        }

        public async Task<string> DownloadFileAsync(string remoteFilePath)
        {
            var sessionOptions = GetSessionOptions();
            string content = "";
            string tempFile = Path.GetTempFileName();
            try
            {
                await Task.Run(() =>
                {
                    using (var session = new Session())
                    {
                        session.Open(sessionOptions);
                        var transferOptions = new TransferOptions { TransferMode = TransferMode.Binary };
                        session.GetFiles(remoteFilePath, tempFile, false, transferOptions).Check();
                    }
                });

                // HMI'dan gelen dosya içeriğini önce byte dizisi olarak oku.
                byte[] fileBytes = await File.ReadAllBytesAsync(tempFile);
                // Ardından, byte dizisini Unicode (UTF-16) olarak metne dönüştür.
                content = Encoding.Unicode.GetString(fileBytes);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
            return content;
        }
        #endregion
    }
}
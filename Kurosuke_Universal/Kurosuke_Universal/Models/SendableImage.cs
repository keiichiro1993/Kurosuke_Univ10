using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography;
using System.IO;

namespace Kurosuke_Universal.Models
{
    public class SendableImage
    {
        public BitmapImage image { get; set; }
        public string path { get; set; }
        public HttpBufferContent httpContent { get; set; }
        public StorageFile file;

        public SendableImage(StorageFile file)
        {
            this.file = file;
            path = file.Path;
            Init();
        }

        public async void Init()
        {
            image = new BitmapImage();
            using (var stream = await file.OpenReadAsync())
            {
                if (stream == null)
                {
                    throw new Exception("画像のロードに失敗しました。");
                }

                byte[] bytes = null;
                bytes = new byte[stream.Size];
                using (DataReader reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(bytes);
                }

                IBuffer buffer = null;
                using (DataWriter writer = new DataWriter())
                {
                    writer.WriteBytes(bytes);
                    buffer = writer.DetachBuffer();
                }

                httpContent = new HttpBufferContent(buffer);
            }

            using (var stream = await file.OpenReadAsync())
            {
                image.SetSource(stream);
            }
        }
    }
}

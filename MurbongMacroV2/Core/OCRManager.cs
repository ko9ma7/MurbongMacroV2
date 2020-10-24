using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using System.Configuration;
using System;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;

namespace MurbongMacroV2.Core
{
    public static class OCRManager
    {
        public static async Task<string> ExtractTextAsync(Image img,string lang)
        {
            MemoryStream memoryStream = new MemoryStream();
            InMemoryRandomAccessStream randStream = new InMemoryRandomAccessStream();
            string result = "";
            try
            {
                img.Save(memoryStream, ImageFormat.Bmp);
                await randStream.WriteAsync(memoryStream.ToArray().AsBuffer());
                if (!OcrEngine.IsLanguageSupported(new Language(lang)))
                    Console.Write("This language is not supported!");
                OcrEngine ocrEngine = OcrEngine.TryCreateFromLanguage(new Language(lang));
                if (ocrEngine == null)
                {
                    ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
                }
                var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(randStream);
                OcrResult ocrResult = await ocrEngine.RecognizeAsync(await decoder.GetSoftwareBitmapAsync());
                result = ocrResult.Text;
                return result;
            }
            finally
            {
                memoryStream.Dispose();
                randStream.Dispose();
                GC.Collect(0);
            }
        }
    }
}

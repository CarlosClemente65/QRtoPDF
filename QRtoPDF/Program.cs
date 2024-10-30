using System;
using System.Drawing;
using ZXing;
using PdfSharp.Drawing;
using System.Drawing.Imaging;
using PdfSharp.Pdf.IO;
using System.IO;

namespace QRtoPDF
{
    internal class Program
    {
        static int margen = 2;
        static int anchoQR = 35 + (margen * 2);
        static string pdfEntrada = string.Empty;
        static string pdfSalida = string.Empty;
        static string textoQR = string.Empty;

        static void Main(string[] args)
        {
            if (args.Length < 5)
            {
                Console.WriteLine("Uso: TuProyecto <pdf_entrada> <pdfSalida> <anchoQR> <margen> <texto_qr>");
                return;
            }

            pdfEntrada = args[0];
            pdfSalida = args[1];
            margen = Convert.ToInt32(args[3]);
            anchoQR = Convert.ToInt32(args[2]) + (margen * 2);
            textoQR = args[4];

            var imagenQR = GeneradorQR(); // tamaño en mm

            InsertarQR(imagenQR);
        }




        public static Bitmap GeneradorQR()
        {
            // Convertir tamaño de mm a píxeles (suponiendo 96 DPI)
            int tamañoPx = (int)(anchoQR * 300 / 25.4);
            int margenPx = (int)(margen * 300 / 25.4);
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = tamañoPx + margenPx,
                    Height = tamañoPx + margenPx,
                    Margin = margen
                }
            };
            return writer.Write(textoQR);
        }

        public static void InsertarQR(Bitmap qrImage)
        {
            using (var document = PdfReader.Open(pdfEntrada, PdfDocumentOpenMode.Modify))
            {
                var page = document.Pages[0];
                using (var ms = new System.IO.MemoryStream())
                {
                    qrImage.Save(ms, ImageFormat.Png);
                    ms.Position = 0;

                    var xImage = XImage.FromStream(ms);

                    // Convertir las dimensiones de la imagen QR de píxeles a puntos para PDF
                    double ancho = (anchoQR + margen) * 2.83465;
                    double alto = (anchoQR + margen) * 2.83465;
                    float anchoImagen = qrImage.Width;

                    // Crear un objeto XGraphics para la página
                    XGraphics gfx = XGraphics.FromPdfPage(page);

                    // Dibujar la imagen del código QR en la página
                    gfx.DrawImage(xImage, 0, 0, ancho, alto); // Posición y tamaño en puntos

                }
                document.Save(pdfSalida);
            }
        }
    }
}

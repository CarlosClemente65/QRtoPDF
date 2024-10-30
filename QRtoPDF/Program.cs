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
        static int margen = 4; //Margen por defecto
        static int anchoQR = 35 + (margen * 2); //Tamaño del QR por defecto 35 mm
        static double posicionX = 0; //Posicion de la imagen en pixels (alineacion izquierda)
        static string pdfEntrada = string.Empty;
        static string pdfSalida = string.Empty;
        static string alineacion = "IZQUIERDA";
        static string textoQR = string.Empty;

        // Convertir tamaño de mm a píxeles (suponiendo 96 DPI)
        static int tamañoQRPx = (int)(anchoQR * 96 / 25.4);
        static int margenPx = (int)(margen * 96 / 25.4);

        static void Main(string[] args)
        {
            if (args.Length < 6)
            {
                Console.WriteLine("Uso: QRtoPDF <pdf_entrada> <pdfSalida> <anchoQR> <margen> <alineacion> <texto_qr>");
                return;
            }

            pdfEntrada = args[0];
            pdfSalida = args[1];
            margen = Convert.ToInt32(args[3]);
            anchoQR = Convert.ToInt32(args[2]) + (margen * 2);
            alineacion = args[4].ToUpper();
            textoQR = args[5];
            
            //Ajuste del tamaño y margen a los pasados por parametro
            tamañoQRPx = (int)(anchoQR * 96 / 25.4);
            margenPx = (int)(margen * 96 / 25.4);

            if (alineacion == "DERECHA")
            {
                posicionX = (210 - anchoQR - (margen * 2)) * 72 / 25.4;
            }


            var imagenQR = GeneradorQR(); // tamaño en mm

            InsertarQR(imagenQR);
        }




        public static Bitmap GeneradorQR()
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = tamañoQRPx,
                    Height = tamañoQRPx,
                    Margin = 0
                }
            };


            Font font = new Font("Arial", 10, FontStyle.Bold);
            int tamañoBitmap = tamañoQRPx + (margenPx * 2) + (font.Height * 2);

            Bitmap qrImage = writer.Write(textoQR);
            Bitmap bitmapConTexto = new Bitmap(tamañoBitmap, tamañoBitmap);

            using (Graphics graphics = Graphics.FromImage(bitmapConTexto))
            {
                graphics.Clear(Color.White);

                StringFormat formatoTexto = new StringFormat
                {
                    Alignment = StringAlignment.Center
                };

                // Dibujar la imagen del QR en el centro
                graphics.DrawImage(qrImage, margenPx, font.Height, tamañoQRPx, tamañoQRPx);

                // Dibujar el texto superior
                graphics.DrawString("QR tributario", font, Brushes.Black, new RectangleF(0, 0, tamañoBitmap, font.Height), formatoTexto);

                // Dibujar el texto inferior
                graphics.DrawString("VERI*FATU", font, Brushes.Black, new RectangleF(0, tamañoQRPx + font.Height + margenPx, tamañoBitmap, font.Height), formatoTexto);
            }


            return bitmapConTexto;
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
                    double ancho = qrImage.Width * 72 / 96;
                    double alto = qrImage.Width * 72 / 96;
                    //float anchoImagen = qrImage.Width;

                    // Crear un objeto XGraphics para la página
                    XGraphics gfx = XGraphics.FromPdfPage(page);

                    

                    // Dibujar la imagen del código QR en la página
                    gfx.DrawImage(xImage, posicionX, 0, ancho, alto); // Posición y tamaño en puntos

                }
                document.Save(pdfSalida);
            }
        }
    }
}

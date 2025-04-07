using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Web.UI;

namespace ImageConverter
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnConvertImage_Click(object sender, EventArgs e)
        {
            lblResult.Text = ""; // Clear the label text after successful upload
            if (imgFileUpload.HasFile)
            {
                try
                {
                    // Get the uploaded file
                    HttpPostedFile uploadedFile = imgFileUpload.PostedFile;

                    // Load the image into a Bitmap object
                    using (Bitmap originalImage = new Bitmap(uploadedFile.InputStream))
                    {
                        // Get the selected format
                        string selectedFormat = FormatDropDown.SelectedValue.ToLower();

                        // Handle ICO conversion separately
                        if (selectedFormat == "ico")
                        {
                            ConvertToIco(originalImage, uploadedFile);
                            return; // Exit after converting to ICO
                        }

                        // Prepare to convert the image to the selected format
                        ImageFormat imageFormat = GetImageFormat(selectedFormat);

                        // Generate the new file name
                        string convertedFileName = Path.GetFileNameWithoutExtension(uploadedFile.FileName) + "." + selectedFormat;

                        // Create a memory stream to hold the converted image
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            // Convert and save the image to the memory stream
                            originalImage.Save(memoryStream, imageFormat);
                            memoryStream.Position = 0; // Reset stream position

                            CreateSuccessCookie();

                            // Clear response and set headers for download
                            Response.Clear();
                            Response.ContentType = GetContentType(selectedFormat);
                            Response.AddHeader("Content-Disposition", $"attachment; filename={convertedFileName}");
                            Response.BinaryWrite(memoryStream.ToArray());
                            Response.End(); // End the response
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblResult.Text = "An error has occured. Error Message : " + ex.Message;
                }
            }
            else
            {
                lblResult.Text = "Please upload an image first.";
            }
        }

        


        private void ConvertToIco(Bitmap originalImage, HttpPostedFile uploadedFile)
        {
            string convertedFileName = Path.GetFileNameWithoutExtension(uploadedFile.FileName) + ".ico";

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(memoryStream))
                {
                    // ICO Header
                    writer.Write((short)0);        // Reserved, always 0
                    writer.Write((short)1);        // Type, 1 for icon (.ico)
                    writer.Write((short)5);        // Number of images in the file (5 sizes: 16x16, 32x32, 64x64, 128x128, 256x256)

                    // Write image directory entries for each icon size
                    long[] imageOffsetPositions = new long[5];  // For 5 sizes
                    int[] imageSizes = { 16, 32, 64, 128, 256 };

                    for (int i = 0; i < imageSizes.Length; i++)
                    {
                        writer.Write((byte)imageSizes[i]); // Icon width
                        writer.Write((byte)imageSizes[i]); // Icon height
                        writer.Write((byte)0);             // Number of colors (0 if not using a color palette)
                        writer.Write((byte)0);             // Reserved
                        writer.Write((short)1);            // Color planes
                        writer.Write((short)32);           // Bits per pixel
                        writer.Write(0);                   // Image size (placeholder, will update later)
                        imageOffsetPositions[i] = memoryStream.Position; // Store position for image offset
                        writer.Write(0);                   // Image data offset (placeholder, will update later)
                    }

                    // Now write the actual bitmap data for each size with high-quality resizing
                    for (int i = 0; i < imageSizes.Length; i++)
                    {
                        using (MemoryStream imageStream = new MemoryStream())
                        {
                            // Resize the original image to the current icon size using high-quality resizing
                            Bitmap resizedImage = ResizeImageWithQuality(originalImage, imageSizes[i], imageSizes[i]);

                            // Flip the image vertically before saving as ICO
                            resizedImage.RotateFlip(RotateFlipType.RotateNoneFlipY);

                            // Save the resized image to BMP format and get pixel data as DIB (bottom-up)
                            SaveAsIcoFormat(resizedImage, imageStream);

                            // Get the image size
                            int imageSize = (int)imageStream.Length;

                            // Update the image size placeholder
                            long currentPosition = memoryStream.Position;
                            memoryStream.Seek(imageOffsetPositions[i] - 4, SeekOrigin.Begin);
                            writer.Write(imageSize);  // Write the correct image size
                            memoryStream.Seek(imageOffsetPositions[i], SeekOrigin.Begin);
                            writer.Write((int)currentPosition); // Write the correct image data offset

                            // Write the actual image data (DIB format)
                            memoryStream.Seek(0, SeekOrigin.End);
                            writer.Write(imageStream.ToArray());
                        }
                    }

                    CreateSuccessCookie();

                    // Clear response and set headers for download
                    Response.Clear();
                    Response.ContentType = "image/x-icon";
                    Response.AddHeader("Content-Disposition", $"attachment; filename={convertedFileName}");
                    Response.BinaryWrite(memoryStream.ToArray());
                    Response.End(); // End the response
                }
            }
        }


        void CreateSuccessCookie()
        {
            HttpCookie httpCookie = new HttpCookie("ConversionResult");
            httpCookie.Value = "Success";
            Response.Cookies.Add(httpCookie);
        }

        // Helper method to save image as DIB (BMP without the file header) and include transparency mask
        private void SaveAsIcoFormat(Bitmap image, Stream outputStream)
        {
            // Save the image as BMP
            using (MemoryStream bmpStream = new MemoryStream())
            {
                image.Save(bmpStream, ImageFormat.Bmp);

                // Read the BMP data
                byte[] bmpData = bmpStream.ToArray();

                // BMP headers are 54 bytes long (14 bytes file header, 40 bytes info header)
                const int headerSize = 54;

                // Copy the BMP info header (40 bytes) to the DIB output
                outputStream.Write(bmpData, 14, 40); // Write only the BITMAPINFOHEADER

                // Adjust the height in the BMP header to double the image height (including mask)
                using (BinaryWriter bmpWriter = new BinaryWriter(outputStream, System.Text.Encoding.Default, true))
                {
                    // Seek to the height field in the BITMAPINFOHEADER (offset 8 bytes into the header)
                    outputStream.Seek(8, SeekOrigin.Begin);

                    // Write the doubled height (image height + mask height)
                    bmpWriter.Write(image.Height * 2);

                    // Restore position to end of stream to continue writing the image data
                    outputStream.Seek(0, SeekOrigin.End);
                }

                // BMP stores rows in top-down order, but ICO expects bottom-up order.
                int rowSize = ((image.Width * 32 + 31) / 32) * 4; // Calculate the size of each row, considering padding

                // Write the image data in bottom-up order (last row first)
                for (int y = image.Height - 1; y >= 0; y--)
                {
                    outputStream.Write(bmpData, headerSize + y * rowSize, rowSize);
                }

                // Write the AND mask (used for transparency in ICO)
                byte[] andMask = new byte[rowSize * image.Height];
                outputStream.Write(andMask, 0, andMask.Length); // The mask can be zeroed out (no transparency)
            }
        }

        private Bitmap ResizeImageWithQuality(Bitmap originalImage, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                // Set high-quality rendering options
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                // Draw the resized image
                graphics.DrawImage(originalImage, 0, 0, width, height);
            }
            return resizedImage;
        }




        private ImageFormat GetImageFormat(string format)
        {
            switch (format)
            {
                case "png":
                    return ImageFormat.Png;
                case "jpeg":
                case "jpg":
                    return ImageFormat.Jpeg;
                case "bmp":
                    return ImageFormat.Bmp;
                case "gif":
                    return ImageFormat.Gif;
                case "tiff":
                    return ImageFormat.Tiff;
                // For unsupported formats, throw an exception or handle accordingly
                default:
                    throw new NotSupportedException($"The selected format '{format}' is not recognized.");
            }
        }

        private string GetContentType(string format)
        {
            switch (format)
            {
                case "png":
                    return "image/png";
                case "jpeg":
                case "jpg":
                    return "image/jpeg";
                case "bmp":
                    return "image/bmp";
                case "gif":
                    return "image/gif";
                case "ico":
                    return "image/x-icon";
                case "tiff":
                    return "image/tiff";
                // Other content types can be added here
                default:
                    return "application/octet-stream";
            }
        }



    }
}

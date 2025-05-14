using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace AttendanceSystem.Helpers
{
    public class QRCodeHelper
    {
        /// <summary>
        /// 生成学生的二维码
        /// </summary>
        /// <param name="studentId">学生学号</param>
        /// <param name="size">二维码大小</param>
        /// <returns>包含学生ID的二维码图像</returns>
        public static Bitmap GenerateQRCode(string studentId, int size = 200)
        {
            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    DisableECI = true,
                    CharacterSet = "UTF-8",
                    Width = size,
                    Height = size,
                    Margin = 1,
                    ErrorCorrection = ErrorCorrectionLevel.H
                }
            };

            return barcodeWriter.Write(studentId);
        }

        /// <summary>
        /// 保存学生的二维码到文件
        /// </summary>
        /// <param name="studentId">学生学号</param>
        /// <param name="filePath">文件保存路径</param>
        /// <param name="size">二维码大小</param>
        public static void SaveQRCodeToFile(string studentId, string filePath, int size = 200)
        {
            var qrCode = GenerateQRCode(studentId, size);
            qrCode.Save(filePath, ImageFormat.Png);
        }

        /// <summary>
        /// 导出所有学生的二维码
        /// </summary>
        /// <param name="students">学生列表</param>
        /// <param name="folderPath">导出文件夹路径</param>
        public static void ExportAllStudentQRCodes(System.Collections.Generic.List<Models.Student> students, string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            foreach (var student in students)
            {
                string filePath = Path.Combine(folderPath, $"{student.StudentId}_{student.Name}.png");
                SaveQRCodeToFile(student.StudentId, filePath);
            }

            MessageBox.Show($"已成功导出 {students.Count} 个学生二维码到 {folderPath}", "导出成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 从图像中解码二维码
        /// </summary>
        /// <param name="image">包含二维码的图像</param>
        /// <returns>解码结果，如果解码失败则返回null</returns>
        public static string DecodeQRCode(Bitmap image)
        {
            try
            {
                var barcodeReader = new BarcodeReader
                {
                    Options = new DecodingOptions
                    {
                        TryHarder = true,
                        PossibleFormats = new[] { BarcodeFormat.QR_CODE }
                    }
                };

                var result = barcodeReader.Decode(image);
                return result?.Text;
            }
            catch
            {
                return null;
            }
        }
    }
} 
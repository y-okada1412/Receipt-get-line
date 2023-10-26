/*
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;

class Program
{
    static void Main()
    {
        // 1. レシート画像の読み込み
        var image = new UMat("C:/Users/yuma/OneDrive/Training/program_training/C_sharp/ReceiptGetLine/seiyu.jpg", ImreadModes.Color);

        // 2. グレースケール変換
        var gray = new UMat();
        CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);

        // 3. エッジ検出
        var edges = new UMat();
        CvInvoke.Canny(gray, edges, 50, 150);

        // 4. Hough変換による直線検出
        LineSegment2D[] lines = CvInvoke.HoughLinesP(
           edges,
           1, // ピクセル解像度
           Math.PI / 180.0, // 角度の解像度
           20, // 閾値
           30, // 最小の線の長さ
           10); // 2点間の最大ギャップ

        // 最も長い直線を取得
        double maxLineLength = 0;
        LineSegment2D longestLine = new LineSegment2D();
        foreach (var line in lines)
        {
            var length = line.Length;
            if (length > maxLineLength)
            {
                maxLineLength = length;
                longestLine = line;
            }
        }

        // 5. 最も長い直線の角度を計算
        double angle = Math.Atan2(longestLine.P2.Y - longestLine.P1.Y, longestLine.P2.X - longestLine.P1.X) * 180.0 / Math.PI;

        // 6. 画像のアフィン変換を使用して補正
        Mat rotationMatrix = new Mat();
        CvInvoke.GetRotationMatrix2D(new System.Drawing.PointF(image.Size.Width / 2, image.Size.Height / 2), angle, 1, rotationMatrix);
        var rotatedImage = new UMat();
        CvInvoke.WarpAffine(image, rotatedImage, rotationMatrix, image.Size);

        // 補正した画像を保存
        rotatedImage.Save("C:/Users/yuma/OneDrive/Training/program_training/C_sharp/ReceiptGetLine/corrected_receipt.jpg");
    }
}

*/







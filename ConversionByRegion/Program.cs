
using System.IO;
using System;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts.Unicode;




namespace JSONReadingExample
{
    class Program
    {
        static void Main(string[] args)
        {

            var filePath = "SevenReciept.json";
            // var filePath = "taxireceipt.json";
            // var filePath = "seiyureceipt.json";
            string jsonStr = File.ReadAllText(filePath, Encoding.UTF8);

            var jsonData = JsonSerializer.Deserialize<ReceiptAnalysis>(jsonStr);

        //Make square on the Receipt image
            var imgPath = "7-11_strait_img.png";
            // var imgPath = "taxireceipt_img.jpg";
            // var imgPath = "seiyu.jpg";
            using (Image<Rgba32> image = Image.Load<Rgba32>(imgPath))
            {
                foreach (var ReadResult in jsonData.AnalyzeResult.ReadResults)
                {
                    foreach (var line in ReadResult.Lines)
                    {
                        var boundingBox = line.BoundingBox;
                        SixLabors.ImageSharp.PointF[] points = 
                        {
                            new SixLabors.ImageSharp.PointF((float)boundingBox[0]/2, (float)boundingBox[1]/2),
                            new SixLabors.ImageSharp.PointF((float)boundingBox[2]/2, (float)boundingBox[3]/2),
                            new SixLabors.ImageSharp.PointF((float)boundingBox[4]/2, (float)boundingBox[5]/2),
                            new SixLabors.ImageSharp.PointF((float)boundingBox[6]/2, (float)boundingBox[7]/2),
                            // new SixLabors.ImageSharp.PointF((float)(boundingBox[0]) , (float)(boundingBox[1])),
                            // new SixLabors.ImageSharp.PointF((float)(boundingBox[2] ), (float)(boundingBox[3] )),
                            // new SixLabors.ImageSharp.PointF((float)(boundingBox[4] ), (float)(boundingBox[5])),
                            // new SixLabors.ImageSharp.PointF((float)(boundingBox[6] ), (float)(boundingBox[7])),
                        };
                        image.Mutate(ctx => ctx.DrawPolygon(Color.Red, 2, points));
                    }
                }
                image.Save("output.png");
                // image.Save("output_taxi.png");
                // image.Save("output_seiyu.png");
            }

        //Rotate adress
            List<boxDetail> details = new List<boxDetail>();
            List<double> bbwidth = new List<double>();
            List<(List<Line> Lines,double MidY, double Threshold,double leftX,double rightX)> groupedLines = new List<(List<Line>, double, double, double, double)>();

            foreach(var ReadResult in jsonData.AnalyzeResult.ReadResults) 
            {
                //search biggest boundingbox and rotate
                for (int i = 0; i < ReadResult.Lines.Count(); i++)
                {
                    double width = Calcwidth(ReadResult.Lines[i]);
                    bbwidth.Add(width);

                }

                int NumOfMaxIdx = bbwidth.IndexOf(bbwidth.Max());
                
                double biggestWidth = Calcwidth(ReadResult.Lines[NumOfMaxIdx]);
                var biggestCenter = CalcCenter(ReadResult.Lines[NumOfMaxIdx].BoundingBox);
                var BiggestData = RotDiv(ReadResult.Lines[NumOfMaxIdx].BoundingBox);
                double div = CalcDiv(BiggestData.leftBb,BiggestData.rightBb);
                Console.WriteLine("------------------------------");
                Console.WriteLine("Index of width:");
                for(int i = 0; i < bbwidth.Count(); i++)
                {
                    Console.WriteLine("{0} : {1}",ReadResult.Lines[i].Text, bbwidth[i]);
                }
                
                Console.WriteLine("biggest content:{0}",ReadResult.Lines[NumOfMaxIdx].Text);
                Console.WriteLine("div={0}",div);
                Console.WriteLine("------------------------------");

                for (int i = 0; i < ReadResult.Lines.Count(); i++)
                {
                    double bias = CalcBias(ReadResult.Lines[i]);
                    double Centertheta = -Math.Atan(bias);
                    var center = CalcCenter(ReadResult.Lines[i].BoundingBox);
                    double widthContent = Calcwidth(ReadResult.Lines[i]);
                    List<double> bbRe = RotateBb(ReadResult.Lines[i].BoundingBox, center.x, center.y, Centertheta);

                    if (center.x > biggestCenter.x )
                    {
                        bbRe = AddDiv(bbRe,div);
                    }
                    // boundingbox double to int
                    List<int> intbb = bbRe.Select(d => (int)Math.Round(d)).ToList();
                    ReadResult.Lines[i].BoundingBox = intbb;


                }




        //Determing whether it's the same line and grouping the lines

                foreach (var line in ReadResult.Lines)
                {
                    bool lineGrouped = false;
                    
                    //adress of BoundingBox
                    double rightYMid = (line.BoundingBox[1] + line.BoundingBox[7]) / 2.0;
                    double leftYMid = (line.BoundingBox[3] + line.BoundingBox[5]) / 2.0;
                    double leftXMid = (line.BoundingBox[0] + line.BoundingBox[6]) / 2.0;
                    double rightXMid = (line.BoundingBox[2] + line.BoundingBox[4]) / 2.0;
                    double currentThreshold = Math.Abs(line.BoundingBox[1] - line.BoundingBox[7]) / 2.0;


                    for (int i = 0; i < groupedLines.Count; i++)
                    {

                        double groupedYMid = groupedLines[i].MidY;
                        double groupedleftX = groupedLines[i].leftX;
                        double groupedrightX = groupedLines[i].rightX;
                        var currentGroup = groupedLines[i];


                        if (Math.Abs(groupedYMid - leftYMid) < groupedLines[i].Threshold && (groupedleftX >= rightXMid || groupedrightX <= leftXMid))
                        {
                            groupedLines[i].Lines.Add(line);
                            lineGrouped = true;
                            groupedLines[i] = (currentGroup.Lines,rightYMid,currentGroup.Threshold,groupedleftX,rightXMid);

                            break;
                        }
                    }

                    if (!lineGrouped)
                    {
                        groupedLines.Add((new List<Line> {line}, rightYMid, currentThreshold,leftXMid,rightXMid));
                    }
                }
            }

        //Output all Lines
            int numLine = 0;
            foreach (var group in groupedLines)
            {   numLine += 1;
                StringBuilder str = new StringBuilder();
                foreach (var line in group.Lines.OrderBy(l => l.BoundingBox[0]))
                {
                    str.Append(line.Text + " ");
                }
                // Console.WriteLine("Line {0}:\nText:{1}\nadressY:{2} rangeY:{3}",numLine, str.ToString(),group.MidY,group.Threshold);
                Console.WriteLine("Line {0}:{1}", numLine, str.ToString());
            }

        //aquarebox print on image after rotate receipt
            using (Image<Rgba32> image = Image.Load<Rgba32>(imgPath))
            {
                foreach (var ReadResult in jsonData.AnalyzeResult.ReadResults)
                {
                    foreach (var line in ReadResult.Lines)
                    {
                        var boundingBox = line.BoundingBox;
                        SixLabors.ImageSharp.PointF[] points = 
                        {
                            new SixLabors.ImageSharp.PointF((float)boundingBox[0]/2, (float)boundingBox[1]/2),
                            new SixLabors.ImageSharp.PointF((float)boundingBox[2]/2, (float)boundingBox[3]/2),
                            new SixLabors.ImageSharp.PointF((float)boundingBox[4]/2, (float)boundingBox[5]/2),
                            new SixLabors.ImageSharp.PointF((float)boundingBox[6]/2, (float)boundingBox[7]/2),
                            // new SixLabors.ImageSharp.PointF((float)(boundingBox[0]) , (float)(boundingBox[1])),
                            // new SixLabors.ImageSharp.PointF((float)(boundingBox[2] ), (float)(boundingBox[3] )),
                            // new SixLabors.ImageSharp.PointF((float)(boundingBox[4] ), (float)(boundingBox[5])),
                            // new SixLabors.ImageSharp.PointF((float)(boundingBox[6] ), (float)(boundingBox[7])),
                        };
                        image.Mutate(ctx => ctx.DrawPolygon(Color.Blue, 2, points));
                    }
                }
                // image.Save("output.png");
                // image.Save("output_taxi.png");
                image.Save("output_sevenEleven_after.png");
            }
        }

        //rotate a BoundingBox
        public static List<double> RotateBb(List<int> bb, double Cx, double Cy, double angle)
        {
            List<double> bbRe = new List<double>();
            for (int i = 0; i < 4; i++)
            {
                double x = bb[2 * i];
                double y = bb[2 * i + 1];
                bbRe.Add(Cx + (x - Cx) * Math.Cos(angle) - (y - Cy) * Math.Sin(angle));
                bbRe.Add(Cy + (x - Cx) * Math.Sin(angle) + (y - Cy) * Math.Cos(angle));
            }
            return bbRe;
        }
        static double CalcDiv(List<double> bb1, List<double> bb2)
        {
            double sum1 = bb1.Where((val, idx) => idx % 2 == 1).Sum();
            double sum2 = bb2.Where((val, idx) => idx % 2 == 1).Sum();
            
            //if sum1 > sum2 => div =='+'
            return sum1 /4 - sum2/ 4;
        }
        static double CalcBias(Line line)
        {
            return (double)((((line.BoundingBox[3] + line.BoundingBox[5]) / 2.0) - ((line.BoundingBox[7] + line.BoundingBox[1]) / 2.0)) / (((line.BoundingBox[2] + line.BoundingBox[4]) / 2.0) - ((line.BoundingBox[6] + line.BoundingBox[0]) / 2.0)));
        }

        public static (double x, double y) CalcCenter(List<int> bb)
        {
            double x = bb.Where((val, idx) => idx % 2 == 0).Sum() / 4;
            double y = bb.Where((val, idx) => idx % 2 == 1).Sum() / 4;
            return (x, y);
        }
        public static double CalcAngle(int x1,int y1,int x2, int y2)
        {   double ang;
            if(x1 - x2 != 0)
            {
                ang = (y1 - y2) / (x1 - x2);
            }
            else
            {
                ang = double.PositiveInfinity;
            }
            return ang;
        }
        public static List<double> AddDiv(List<double> bb, double div)
        {
            for (int i = 0; i < 4; i++)
                bb[2 * i + 1] -= div;
            return bb;
        }
        public static double Calcwidth (Line line)
        {
            return (Math.Abs(line.BoundingBox[0] - line.BoundingBox[2])+ Math.Abs(line.BoundingBox[4] - line.BoundingBox[6]))/2.0;
        }
        
         public static (double thetaL, double thetaR, List<double> leftBb, List<double> rightBb) RotDiv(List<int> bb)
        {
            double LCx = (bb[0] + bb[6]) / 2;
            double LCy = (bb[1] + bb[7]) / 2;
            double RCx = (bb[2] + bb[4]) / 2;
            double RCy = (bb[3] + bb[5]) / 2;

            double Langle = CalcAngle(bb[0],bb[1],bb[6],bb[7]);
            double Rangle = CalcAngle(bb[2],bb[3],bb[4],bb[5]);

            double thetaL = (Math.PI / 2 - Math.Abs(Math.Atan(Langle))) * Math.Sign(Langle);
            double thetaR = (Math.PI / 2 - Math.Abs(Math.Atan(Rangle))) * Math.Sign(Rangle);

            List<double> leftBb = RotateBb(bb, LCx, LCy, thetaL);
            List<double> rightBb = RotateBb(bb, RCx, RCy, thetaR);

            Console.WriteLine("ROTATE BY LEFT");
            Console.WriteLine($"LCx={LCx},LCy={LCy},Langle={Langle},thetaL={thetaL}");
            // OutBb(leftBb);
            Console.WriteLine("--------------------------------");
            Console.WriteLine("ROTATE BY RIGHT");
            Console.WriteLine($"RCx={RCx},RCy={RCy},Rangle={Rangle},thetaR={thetaR}");
            // OutBb(rightBb);

            return (thetaL, thetaR, leftBb, rightBb);
        }
        static List<(List<(double,int)>,double)> JugeBiasGroup (double bias,int num,List<(List<(double baias,int num)>,double avg)> biases)
        {
            int nearestNum = -1;
            for (int i = 0; i < biases.Count; i++)
            {
                if (bias > 0 && biases[i].avg > 0 && Math.Abs(biases[i].avg - bias) < 0.05 )
                {
                    if (nearestNum == -1 )
                    {
                        nearestNum = i;
                    }
                    else if (Math.Abs(biases[i].avg - bias) < Math.Abs(biases[nearestNum].avg - bias)) 
                    {
                        nearestNum = i;
                    }

                    
                }
                else if (biases[i].avg < 0 && bias <0 && Math.Abs(biases[i].avg - bias) < 0.05)
                {
                    if (nearestNum == -1 )
                    {
                        nearestNum = i;
                    }
                    else if (Math.Abs(biases[i].avg - bias) < Math.Abs(biases[nearestNum].avg - bias)) 
                    {
                        nearestNum = i;
                    }

                }
            }
            if (nearestNum == -1)
            {
                biases.Add((new List<(double,int)> {(bias,num)}, bias));
            }
            else{
                var oldList = biases[nearestNum].Item1;
                double newAvg = (biases[nearestNum].avg * biases[nearestNum].Item1.Count + bias) / (biases[nearestNum].Item1.Count + 1);
                oldList.Add((bias,num));
                biases[nearestNum] = (oldList,newAvg);
            }
            return biases;
        }

    }
}

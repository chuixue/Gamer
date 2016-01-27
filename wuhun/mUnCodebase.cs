using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
namespace wuhun
{
    class UnCodebase
    {
        public Bitmap bmpobj;
        public Bitmap image;
        //public Bitmap part1;
        //public Bitmap part2;
        public string txt;
        public UnCodebase(Bitmap pic)
        {
            // if (pic.PixelFormat == PixelFormat.Format8bppIndexed)
            bmpobj = new Bitmap(pic); //转换为Format32bppRgb
            image = new Bitmap(pic);
            //part1 = bmpobj.Clone(new Rectangle(0, 0, bmpobj.Width / 2, bmpobj.Height), bmpobj.PixelFormat);
            //part2 = bmpobj.Clone(new Rectangle(bmpobj.Width / 2,0,bmpobj.Width / 2, bmpobj.Height), bmpobj.PixelFormat);
            //bmpobj = part2;

        }
        /// <summary>
        /// 根据RGB，计算灰度值
        /// </summary>
        /// <param name="posClr">Color值</param>
        /// <returns>灰度值，整型</returns>
        private int GetGrayNumColor(System.Drawing.Color posClr)
        {
            return (posClr.R * 19595 + posClr.G * 38469 + posClr.B * 7472) >> 16;
        }
        public void DealDG2()
        {
            for (int i = 0; i < bmpobj.Width; i++)
            {
                for (int j = 0; j < bmpobj.Height; j++)
                {
                    int r, g, b;
                    Color clr = bmpobj.GetPixel(i, j);
                    r = clr.R; g = clr.G; b = clr.B;
                    float Scale=1.5f;  
                    r=(int)(r*Scale); 
                    g=(int)(g*Scale); 
                    b=(int)(b*Scale); 
                    r=(r<0)?0:((r>255)?255:r); 
                    g=(g<0)?0:((g>255)?255:g); 
                    b=(b<0)?0:((b>255)?255:b);
                    bmpobj.SetPixel(i, j, Color.FromArgb(r, g, b));
                }
            }
        }
        //*************改变亮度和对比度
        public void AddDiff(ref Bitmap src)
        {
            List<int> RGB = new List<int>();
            RGB.Add(0); RGB.Add(0); RGB.Add(0);
            for (int i = 0; i < src.Width; i++)
            {
                for (int j = 0; j < src.Height; j++)
                {
                    Color clr = src.GetPixel(i, j);
                    RGB[0] += clr.R; RGB[1] += clr.G; RGB[2] += clr.B;
                }
            }
            RGB[0] = RGB[0] / (src.Height * src.Width);
            RGB[1] = RGB[1] / (src.Height * src.Width);
            RGB[2] = RGB[2] / (src.Height * src.Width);
            for (int i = 0; i < src.Width; i++)
            {
                for (int j = 0; j < src.Height; j++)
                {
                    int r, g, b;
                    Color clr = src.GetPixel(i, j);
                    r = clr.R; g = clr.G; b = clr.B;
                    r -= RGB[0]; g -= RGB[1]; b -= RGB[2];
                    r = (int)(r * 4);
                    g = (int)(g * 4);
                    b = (int)(b * 4);
                    r += (int)(RGB[0] * 2);
                    g += (int)(RGB[1] * 2);
                    b += (int)(RGB[2] * 2);
                    r = r < 0 ? 0 : r; g = g < 0 ? 0 : g; b = b < 0 ? 0 : b;
                    r = r > 255 ? 255 : r; g = g >255 ? 255 : g; b = b >255 ? 255 : b;
                    src.SetPixel(i, j, Color.FromArgb(r, g, b));
                }
            }
        }
        //***********颜色翻转
        public void Rec(ref Bitmap src)
        {
            for (int i = 0; i < src.Width; i++)
            {
                for (int j = 0; j < src.Height; j++)
                {
                    Color clr = src.GetPixel(i, j);
                    src.SetPixel(i, j, Color.FromArgb(255 - clr.R, 255 - clr.G, 255 - clr.B));
                }
            }
        }
        //***********黑白化
        public void BlackOrWhite(ref Bitmap src)
        {
            for (int i = 0; i < src.Width; i++)
            {
                for (int j = 0; j < src.Height; j++)
                {
                    Color clr = src.GetPixel(i, j);
                    if (clr.R < 255)
                        src.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                    else
                        src.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                }
            }
        }
        //***********图片缩放
        public void ChangeSize(ref Bitmap src, int width=2,int height=3)
        {
            int W = src.Width * width;
            int H = src.Height * height;
            Bitmap bmp = new Bitmap(W, H);
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    int x = (int)(i * src.Width / W);
                    int y = (int)(j * src.Height / H);
                    Color clr = src.GetPixel(x, y);
                    bmp.SetPixel(i, j, clr);
                }
            }
            src = bmp;
        }
        //*************图片灰度化
        public void GrayByPixels(ref Bitmap src)
        {
            long sum = 0;
            for (int i = 0; i < src.Width; i++)
            {
                for (int j = 0; j < src.Height; j++)
                {
                    int tmpValue = GetGrayNumColor(src.GetPixel(i, j));
                    src.SetPixel(i, j, Color.FromArgb(tmpValue, tmpValue, tmpValue));
                    sum += Color.FromArgb(tmpValue, tmpValue, tmpValue).ToArgb();
                }
            }
            sum = sum / (bmpobj.Width * bmpobj.Height);
            //DealMap(sum);
        }
        //****************************************************************
        public void DealMap()
        {
            Rec(ref image);
            AddDiff(ref image);
            BlackOrWhite(ref image);
            ChangeSize(ref image);
            bmpobj = image;
        }
        public void DealMap2()
        {
            Rec(ref bmpobj);
            AddDiff(ref bmpobj);
            BlackOrWhite(ref bmpobj);
            ChangeSize(ref bmpobj);
        }
        public void DealText()
        {
            if(IsRed(image))GetSingleColor(ref image);else GrayByPixels(ref image);
            AddDiff(ref image);
            BlackOrWhite(ref image); 
            ChangeSize(ref image, 3, 4);
            Rec(ref image); 
            bmpobj = image;
        }
        //判断是否是红色文字
        private bool IsRed(Bitmap src)
        {
            int index = 0;
            for (int i = 0; i < src.Width; i++)
            {
                List<string> line = new List<string>();
                for (int j = 0; j < src.Height; j++)
                {
                    Color clr = src.GetPixel(i, j);
                    if (clr.R > 120 && clr.G < 20 & clr.B < 20) index++;
                    if (index>50)return true ;
                }
            }
            return false;
        }
        //处理红色文字图片
        private void GetSingleColor(ref Bitmap src)
        {
            for (int i = 0; i < src.Width; i++)
            {
                List<string> line = new List<string>();
                for (int j = 0; j < src.Height; j++)
                {
                    Color clr = src.GetPixel(i, j);
                    if (clr.R>120 && clr.G<20 & clr.B<20) //H = 255;
                        src.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                    else
                        src.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                }
            }
        }
      
        public void GrayByPixelsEx()
        {
            //处理坐标数字信息
            //DealMap();return;

            //Rec(ref part2);
            //ChangeSize(ref part2,3,5);
            //AddDiff(ref part2);
            //BlackOrWhite(ref part2);
            GrayByPixels(ref image);
            //ChangeSize(ref part2,3,4);
            AddDiff(ref image);
            BlackOrWhite(ref image);
            ChangeSize(ref image, 3, 4);

            bmpobj = image;
            return;
        }
        public void DealMapEx(long avg=0)
        {
            List<List<Color>> lsAll = new List<List<Color>>();
            List<List<long>> lsS = new List<List<long>>();
            List<double> lsAvg = new List<double>();
            for (int i = 0; i < bmpobj.Width; i++)
            {
                List<Color> lsClr = new List<Color>();
                List<long> lsv = new List<long>();
                for (int j = 0; j < bmpobj.Height; j++)
                {
                    //int tmpValue = GetGrayNumColor(bmpobj.GetPixel(i, j));
                    Color clr = bmpobj.GetPixel(i, j);
                    
                    //60-80,75-90,100-115
                    long temp = (clr.ToArgb() - avg) * 2;
                    if(clr.R<255)
                        bmpobj.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                    else
                        bmpobj.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                    //*/
                    lsClr.Add(clr);
                    lsv.Add(long.Parse(clr.ToArgb().ToString()));
                    //bmpobj.SetPixel(i, j, Color.FromArgb(tmpValue, tmpValue, tmpValue));
                }
                lsAvg.Add(lsv.Average());
                lsS.Add(lsv);
                lsAll.Add(lsClr);//60-80,75-90,100-115
            }
        }
    }
}

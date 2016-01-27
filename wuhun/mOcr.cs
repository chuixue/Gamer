using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wuhun
{
    class OCR
    {
        private string[] Nums;
        private string path = "image/number/";
        public List<int[][]> data = new List<int[][]>();
        List<List<int>> lsLH = new List<List<int>>();//最长连线-列
        List<List<int>> lsLW = new List<List<int>>();//最长连线-行
        List<int> lsSH = new List<int>();   //连线特征和-列
        List<int> lsSW = new List<int>();   //连线特征和-行
        List<double> lsM = new List<double>();//特征值加权和
        private List<Bitmap> lsImg = new List<Bitmap>();
        public List<Bitmap> lsIMAGE = new List<Bitmap>();
        private int W = 12;
        private int H = 24;
        public string Temp = "";
        public OCR()
        {
            this.Nums = new string[11];
            for (int i = 0; i < 10; i++)
            {
                this.Nums[i] = i.ToString();
                Bitmap btp = new Bitmap(this.path + this.Nums[i] + ".png");
                this.lsImg.Add(btp);
            }
            this.Nums[10] = "d";
            Bitmap dtp = new Bitmap(this.path + this.Nums[10] + ".png");
            this.lsImg.Add(dtp);

            for (int i = 0; i < this.Nums.Length; i++)
            { 
                int[][] tp=new int[W][];
                List<List<int>> ls = new List<List<int>>();
                for (int j = 0; j < W; j++)
                {
                    tp[j] = new int[H];
                    List<int> lt = new List<int>();
                    for (int k = 0; k < H; k++)
                    {
                        Color clr = lsImg[i].GetPixel(j, k);
                        tp[j][k] = (clr.R > 125) ? 0 : 1;
                        lt.Add(clr.R);
                    }
                    ls.Add(lt);
                }
                this.data.Add(tp);
                this.lsLH.Add(getLongLine(tp, 0)); //纵向最长线
                this.lsLW.Add(getLongLine(tp, 1)); //横向最长线
                this.lsSH.Add(0);
                this.lsSW.Add(0);
                this.lsLH[i].ForEach(delegate(int e) { this.lsSH[i] += e; });
                this.lsLW[i].ForEach(delegate(int e) { this.lsSW[i] += e; });
                this.lsM.Add(lsSH[i]*0.2 + lsSW[i]*0.8);
            }
        }
        //
        public int getMinDiff(List<int> lsSrc, List<int> lsTar) 
        {
            int a=0, b=0, c=0,d=0;
            for (int i = 0; i < lsSrc.Count; i++)
            {
                a += Math.Abs(lsTar[i] - lsSrc[i]);
                if (i > 0) b += Math.Abs(lsTar[i] - lsSrc[i - 1]);
                if (i > 1) c += Math.Abs(lsTar[i] - lsSrc[i - 2]);
                if (i > 2) d += Math.Abs(lsTar[i] - lsSrc[i - 3]);
            }
            int temp=(a < b) ? a : b;
            temp = (temp < c) ? temp : c;
            return (temp < d) ? temp : d;
        }
        public List<int> getLongLine(int[][]dt,int style=0)
        {
            int width = (style == 0) ? W : H;
            int height = (style == 0) ? H : W;
            List<int> lt = new List<int>();
            for (int j = 0; j < width; j++)
            {
                int max = 0;
                int c = 0;
                for (int k = 0; k < height; k++)
                {
                    int temp = (style == 0) ? dt[j][k] : dt[k][j];
                    if (temp == 1) c++;
                    else
                    {
                        max = (c > max) ? c : max;
                        c = 0;
                    }
                }
                lt.Add(max);
            }
            return lt;
        }
        //获取距离某元素最小的数组索引
        private List<int> sortMinDiff(List<double> lsData, double value)
        {
            List<int> lsIndex = new List<int>();
            int index = 0;
            lsData.ForEach(delegate(double d) { lsIndex.Add(index++); });
            for (int i = 0; i < lsData.Count; i++)
            {
                for (int j = 0; j < lsData.Count-i-1; j++)
                {
                    double dif1 = Math.Abs(lsData[j] - value);
                    double dif2 = Math.Abs(lsData[j+1] - value);
                    if (dif1 > dif2)
                    {
                        double temp = lsData[j];
                        lsData[j] = lsData[j+1];
                        lsData[j+1] = temp;
                        int tI = lsIndex[j];
                        lsIndex[j] = lsIndex[j+1];
                        lsIndex[j+1] = tI;
                    }
                }
            }
            return lsIndex;
        }
      
        public string Recognize(Bitmap img)
        {
            Point pt = getImageBegin(img);
            Bitmap n ;
            string rst = "";
            for (int i = 0; i < 7; i++)
            {
                n = img.Clone(new Rectangle(pt.X, pt.Y, 12, 24), img.PixelFormat);
                lsIMAGE.Add(n);
                string tp = this.getSingleNum(n);
                if (tp == "s") tp = " ";
                if (tp == "d") tp = ",";
                rst += tp;
                pt.X += this.W;
            }
            return rst.Trim();
        }
        private string getSingleNum(Bitmap sng)
        {
            int stp = 0;
            int[][] tp = this.ImageToArray(sng,ref stp);
            if (stp == 0) return "";

            List<int> lsH = getLongLine(tp, 0);
            List<int> lsW = getLongLine(tp, 1);
            int sH = 0, sW = 0;
            lsH.ForEach(delegate(int e) { sH += e; });
            lsW.ForEach(delegate(int e) { sW += e; });
            double M = sH * 0.2 + sW * 0.8;
            List<int> lsLike = sortMinDiff(this.lsM, M);
            int min = -1, index = -1;
            List<int> lstp = new List<int>();
            for (int i = 0; i < lsLike.Count; i++)
            {
                int d = lsLike[i];
                int hd = getMinDiff(lsH, lsLH[d]);
                int wd = getMinDiff(lsW, lsLW[d]);
                int temp = hd + wd;
                lstp.Add(temp);
                if (temp == 0) return this.Nums[d];
                if (min == -1 || min > temp)
                {
                    min = temp; index = d;
                }
            }
            return this.Nums[index];
        }
        //确定图片中文字起始位置
        public Point getImageBegin(Bitmap img)
        {
            int M = img.Height / 2;
            int x=0, y=0;
            //确定x
            bool b = false;
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                { 
                    Color clr = img.GetPixel(i, j);
                    if (clr.R == 0) { b = true; break; }
                }
                if (b) { x = i; break; }
            }
            //确定y
            b = false;
            for (int j = M; j >0; j--)
            {
                b = false;
                for (int i = 0; i < img.Width; i++)
                {
                    Color clr = img.GetPixel(i, j);
                    if (clr.R != 255) { b = true; break; }
                }
                if (!b) { y = j; break; }
            }
            return new Point(x,y+1);
        }
        public int[][] ImageToArray(Bitmap img)
        {
            int sum = 0;
            return ImageToArray(img,ref sum);
        }
        //图片转换为数据矩阵
        public int[][] ImageToArray(Bitmap img,ref int sum)
        {
            sum = 0;
            int[][] tp = new int[img.Width][];
            for (int j = 0; j < img.Width; j++)
            {
                tp[j] = new int[img.Height];
                for (int k = 0; k < img.Height; k++)
                {
                    Color clr = img.GetPixel(j, k);
                    tp[j][k] = (clr.R == 0) ? 1 : 0;
                    sum += tp[j][k];
                }
            }
            return tp;
        }
    }
}

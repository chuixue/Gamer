using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace wuhun
{
    public class Mapping
    {
        public float Kx;
        public float Ky;
        public float ScaleX;
        public float ScaleY;
        public Mapping()
        {
            this.Kx = -0.492f;
            this.Ky = 0.507f;
            this.ScaleX = 0.369f;
            this.ScaleY = 0.368f;
        }
        //求两直线交点
        public static PointF GetLinkPoint(float k1,float b1,float k2,float b2)
        {
            if (k1 == k2) return new PointF(-1, -1);
            PointF rst = new PointF();
            rst.X = (b2 - b1) / (k1 - k2);
            rst.Y = k1 * rst.X + b1;
            return rst;
        }
        //求两点间距离
        public static float GetDis(PointF pt1,PointF pt2)
        {
            return (float)Math.Sqrt((pt1.X - pt2.X) * (pt1.X - pt2.X) + (pt1.Y - pt2.Y) * (pt1.Y - pt2.Y));
        }

        //屏幕 2 客户端
        public PointF ScreenToClient(Point ptCient, Point ptScreen, Point pt)
        {
            PointF rst = new PointF();
            float k1, k2, b1, b2;
            k1 = this.Kx;
            k2 = this.Ky;
            b1 = pt.Y - k1 * pt.X;
            b2 = ptScreen.Y - k2 * ptScreen.X;
            PointF lp = GetLinkPoint(k1, b1, k2, b2);
            float disX = GetDis(lp, pt) * this.ScaleX;
            float disY = GetDis(lp, ptScreen) * this.ScaleY;
            float Bx,By;
            Bx = ptScreen.Y - ptScreen.X * Kx;//法线1
            By = ptScreen.Y - ptScreen.X * Ky;//法线2
            int t = WhichSide(Kx, Bx, lp);
            int Vy = (t == 0) ? 1 : -1;
            t = WhichSide(Ky, By, pt);
            int Vx = (t == 0) ? 1 : -1;
            rst.X = ptCient.X + disX * Vx;
            rst.Y = ptCient.Y + disY * Vy;
            return rst;
        }
        public Point ScreenToClientInt(Point ptCient, Point ptScreen, Point pt)
        {
            PointF rst = ScreenToClient(ptCient, ptScreen, pt);
            return new Point((int)Math.Round(rst.X), (int)Math.Round(rst.Y));
        }
        public Point ClientToScreenInt(Point ptCient, Point ptScreen, Point pt)
        { 
            PointF rst=ClientToScreen(ptCient, ptScreen, pt);
            return new Point((int)Math.Round(rst.X), (int)Math.Round(rst.Y));
        }
        //客户端 2 屏幕
        public PointF ClientToScreen(Point ptCient, Point ptScreen, Point pt)
        {
            PointF rst = new PointF();
            float disY, disX;
            disY = Math.Abs((ptCient.Y - pt.Y) / this.ScaleY);
            disX = Math.Abs((ptCient.X - pt.X) / this.ScaleX);
            //求中间点-Y坐标变动
            float bZ, by;
            bZ = ptScreen.Y - ptScreen.X * Kx;//法线1
            by = ptScreen.Y - ptScreen.X * Ky;//法线2
            List<PointF> ls = GetPointByDis(Ky, by, ptScreen, disY);
            int v = (ptCient.Y - pt.Y) > 0 ? 1 : 0;
            int t = WhichSide(Kx, bZ, ls[0]);
            PointF ptTemp = (t == v) ? ls[0] : ls[1];
            //求目标点-X坐标变动
            float bx;
            bx = ptTemp.Y - ptTemp.X * Kx;
            ls = GetPointByDis(Kx, bx, ptTemp, disX);
            v = (ptCient.X - pt.X) > 0 ? 1 : 0;
            t = WhichSide(Ky, by, ls[0]);
            rst = (t == v) ? ls[0] : ls[1];
            return rst;
        }
        //求直线上和某点距离为dis的点坐标
        public static List<PointF> GetPointByDis(float k, float b, PointF ptO, float dis)
        {
            PointF pt1 = new PointF();
            PointF pt2 = new PointF();
            float temp, tp;
            temp = (dis * dis - ptO.X * ptO.X - (b - ptO.Y) * (b - ptO.Y)) / (k * k + 1);
            tp = (2 * k * (b - ptO.Y) - 2 * ptO.X) / (k * k + 1);
            temp = (float)Math.Sqrt(temp + (tp / 2) * (tp / 2));
            pt1.X = (temp - tp / 2);
            pt2.X = (-temp - tp / 2);
            pt1.Y = (k * (temp - tp / 2) + b);
            pt2.Y = (k * (-temp - tp / 2) + b);
            List<PointF> ls = new List<PointF>();
            ls.Add(pt1);
            ls.Add(pt2);
            return ls;
        }
        //判断点处于斜线哪边
        private int WhichSide(float k, float b, PointF pt)
        {
            double temp;
            temp = (pt.Y - b) / (1.0 * k) - pt.X;
            return (temp > 0) ? 1 : 0;
        }
        
    }



    
}

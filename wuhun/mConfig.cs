using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wuhun
{
    public class Person
    {
        public string userName = "";
        public IntPtr hwnd;
        public Point PosNow=new Point();
        public Rect rect=new Rect();
    }
    public class Config
    {
        public string Name = "small";
        public Rectangle rect_client;//客户端区域
        public Rectangle rect_pos_now;//当前坐标区域·
        public Rectangle rect_pos_temp;//鼠标移动显示坐标区域
        public Rectangle rect_map_now;//当前地图名称区域
        public Rectangle rect_map_big;//Tab大地图区域
        public Rectangle rect_map_core;//Tab大地图核心区域
        public Rectangle rect_map_text;//Tab大地图文字区域
        public Rectangle rect_map_small;//右上角小地图区域
        public Rectangle rect_find;//目标搜索区域大小
        public Point pos_map_mid;//TAB地图核心区域中心点
        public static long Mouse_Normal = 66539;   //一般，游戏外
        public static long Mouse_Fight = 74122035;  //战斗
        public static long Mouse_Talk = 2359539;    //对话
        public static long Mouse_Game = 66531;  //游戏中
        public static long Mouse_Life_Medi = 4653541;    //采药
        public static long Mouse_Life_Wood = 7210057;    //采木
        public static long Mouse_Life_Cott = 2818693;    //采棉
        public static long Mouse_Life_Ore = 132259;    //采矿

        public List<List<string>> lsTxtData=new List<List<string>>();
        public List<string> lsTxt=new List<string>();
        private void setTxt()
        {
            string[] st;
            lsTxt.Add("城市地图");
            st = new string[] { "肺岫", "肺地田" };
            lsTxtData.Add(st.ToList());
            
        }
        public bool CheckMap(string temp,string map)
        {
            int index = lsTxt.IndexOf(map);
            if (index == -1) return false;
            return (lsTxtData[index].IndexOf(temp) == -1) ? false : true;
        }

        public Config()
        {
            setTxt();
            this.rect_client = new Rectangle(150, 0, 1068, 696);//1068+50
            this.rect_pos_now = new Rectangle(892, 2, 70, 20);
            this.rect_pos_temp = new Rectangle(625, 116, 77, 14);
            this.rect_map_now = new Rectangle(945, 2, 88, 19);
            this.rect_map_big = new Rectangle(144, 74, 736, 503);
            this.rect_map_core = new Rectangle(150, 115, 552, 452);//150, 115, 552, 452
            this.rect_map_small = new Rectangle(892, 23, 160, 124);
            this.rect_map_text = new Rectangle(468, 77, 100, 22);
            this.pos_map_mid=new Point(rect_map_core.Left + rect_map_core.Width / 2 +2,rect_map_core.Top + rect_map_core.Height / 2-1);
            this.rect_find = new Rectangle(this.rect_client.Width / 2 - 75, this.rect_client.Height / 2 - 75,150,150);
                //Rectangle rt = new Rectangle(890,0,70,22);//
            //Rectangle rt = new Rectangle(945, 0, 88, 22);//
            Rectangle rt = new Rectangle(892, 23, 160, 124);//
        }
        public static Rect Rect(int x, int y, int right, int bottom)
        {
            Rect rt=new Rect();
            rt.Left = x;
            rt.Top = y;
            rt.Right= right;
            rt.Bottom = bottom;
            return rt;
        }
    }
    
}

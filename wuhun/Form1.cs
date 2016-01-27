using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging; 
using System.IO;
using System.Threading;
using System.Net;
//using tesseract;
using OCR.TesseractWrapper;

namespace wuhun
{
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
    public struct POINTAPI
    {
        public int x;
        public int y;
    }
    public struct CURSORINFO
    {
        public Int32 cbSize;
        public Int32 flags;
        public IntPtr hCursor;
        public Point ptScreenPos;
    }

    public partial class Form1 : Form
    {
        
        
        

        private Person PSN;
        private Config CFG;
        public Mapping MAPP;
        private string imagePath;
        private mPublic F;
        private Rectangle RMap;
        //private TesseractProcessor ocrNumber;
        private OCR ocrNumber;
        private TesseractProcessor ocrText;
        private bool bInitN;
        private bool bInitT;
        public Thread thKill;
        public bool stopKill;

        
        public Form1()
        {
            InitializeComponent();
            this.PSN = new Person();
            this.imagePath = Application.StartupPath + "/image";
            if (Directory.Exists(this.imagePath) == false) Directory.CreateDirectory(this.imagePath);
            this.imagePath += "/";
            Init();
            this.F = new mPublic();
            this.CFG = new Config();
            this.MAPP = new Mapping();
            this.RMap = CFG.rect_map_now;
            this.thKill = new Thread(new ThreadStart(AutoKill));
            this.stopKill = true;
            this.ocrNumber = new OCR();
            /*
            //初始化数字识别引擎
            ocrNumber = new TesseractProcessor();
            bInitN = ocrNumber.Init(@".\", "chi_sim", 3);
            if (!bInitN) { MessageBox.Show("智能识别引擎初始化失败！"); return; }
            ocrNumber.SetVariable("tessedit_char_whitelist", ".,2134567890 ");
             * */
            //初始化汉字引擎
            ocrText = new TesseractProcessor();
            bInitT = ocrText.Init(@".\", "chi_sim", 3);
            if (!bInitT) { MessageBox.Show("智能识别引擎初始化失败！"); return; }
            ocrText.SetVariable("tessedit_char_blacklist", "ﬁ蜘ﬁ'£郜QWERTYUIOPASDFGHJKLZXCVBNM1234567890μqwertyuiopasdfghjklzxcvbnm"); 
            
            
        }
        private void Init()
        {
            IntPtr hwnd = FindWindow("ArkEngine GameClient main window class", null);
            StringBuilder text = new StringBuilder(1024);
            int i = GetWindowText(hwnd, text, 1024);
            string title = text.ToString();
            string[] tp = title.Split(new string[] { " - " }, StringSplitOptions.None);
            this.PSN.hwnd = hwnd;
            this.PSN.userName = tp[tp.Length - 1];
            
            ShowWindow(hwnd, new IntPtr(SW_SHOWNOACTIVATE));
            SetForegroundWindow(hwnd);
        }
        private void cout(string str){ MessageBox.Show(str); }
        private void cout(double str){ MessageBox.Show(str.ToString()); }
        private void cout(int str){ MessageBox.Show(str.ToString()); }
        private void cout(List<double> ls)
        {
            string str = "";
            for (var i = 0; i < ls.Count; i++) str += ls[i].ToString() + ",  ";
            MessageBox.Show(str);
        }
        private void ShowClient()
        {
            ShowWindow(PSN.hwnd, new IntPtr(SW_SHOWNOACTIVATE));
            SetForegroundWindow(PSN.hwnd);
        }
        private void SetPointMid()
        {
            setPoint(new Point(CFG.rect_map_core.Width / 2, CFG.rect_map_core.Height / 2));
        }
        private void SetPointMapMid()
        {
            Thread.Sleep(200);
            setPointA(point(CFG.rect_map_core.Left + CFG.rect_map_core.Width / 2 +2,CFG.rect_map_core.Top + CFG.rect_map_core.Height / 2-1));
        }
        private bool IsClientOK()
        {
            Thread.Sleep(100);
            ShowClient();
            Thread.Sleep(100);
            SetPointMid();
            Point pt = new Point();
            GetCursorPos(ref pt);
            IntPtr hwnd = WindowFromPoint(pt.X, pt.Y);
            if (PSN.hwnd != hwnd) 
            {
                MessageBox.Show("未检测到客户端运行！"); return false; 
            }
            return true;
        }
        private void SendKeyA(int num)
        {
            keybd_event((byte)num, 0, 0, 0); //按下LWIN
            keybd_event((byte)num, 0, 2, 0); //释放LWIN
        }
        private void SendKeyA(Keys key)
        {
            keybd_event((byte)key, 0, 0, 0); //按下LWIN
            keybd_event((byte)key, 0, 2, 0); //释放LWIN
        }
        private void SendKeyA(Keys key1,Keys key2)
        {
            keybd_event((byte)key1, 0, 0, 0); //按下LWIN
            keybd_event((byte)key2, 0, 0, 0); //释放LWIN
            keybd_event((byte)key1, 0, 2, 0); //按下LWIN
            keybd_event((byte)key2, 0, 2, 0); //释放LWIN
        }
        private void SendKeyA_(Keys key1, int key2)
        {
            keybd_event((byte)key1, 0, 0, 0); //按下LWIN
            keybd_event((byte)key2, 0, 0, 0); //释放LWIN
            keybd_event((byte)key2, 0, 2, 0); //按下LWIN
            keybd_event((byte)key1, 0, 2, 0); //释放LWIN
        }
        private void SendKeyA_(Keys key)//局部
        {
            SendMessage(PSN.hwnd, WM_KEYDOWN, (byte)key, 0);
        }
        

        //****************分离颜色
        public Color GetColor(IntPtr hwnd, int x, int y,ref int value)
        {
            Color color = Color.Empty;
            if (hwnd != null)
            {
                IntPtr hDC = GetDC(hwnd);
                int colorRef = GetPixel(hDC, x, y);
                color = Color.FromArgb(
                    (int)(colorRef & 0x000000FF),
                    (int)(colorRef & 0x0000FF00) >> 8,
                    (int)(colorRef & 0x00FF0000) >> 16);
                ReleaseDC(hwnd, hDC);
                value = colorRef;
            }
            return color;
        }
        private Color GetColor(int x,int y,ref int value)
        {
            POINTAPI pt = point(x,y);
            return GetColor(PSN.hwnd,pt.x,pt.y,ref value);
        }
        private Color GetColor(POINTAPI pt, ref int value)
        {
            return GetColor(PSN.hwnd, pt.x, pt.y, ref value);
        }
        private void setPointA(POINTAPI pt)//全局
        {
            ClientToScreen(PSN.hwnd, ref pt);
            SetCursorPos(pt.x, pt.y);
        }
        private void setPoint(Point pt)//后台
        {
            POINTAPI temp = point(pt.X,pt.Y);
            ClientToScreen(PSN.hwnd, ref temp);
            SetCursorPos(temp.x, temp.y);
        }
        private POINTAPI point(int x, int y)
        {
            POINTAPI pt = new POINTAPI();
            pt.x = x;
            pt.y = y;
            return pt;
        }


        //窗体相关
        //************************************************************************
        private bool CheckMap(int x)
        {
            return true;
            
        }
        private void button3_Click(object sender, EventArgs e)
        {
            string[] ls = textBox1.Text.Split(new char[3] { '.', ',', ' ' });
            Point pt = new Point(int.Parse(ls[0]), int.Parse(ls[1]));
            MovoTo(pt);
            //SetPointMapMid();


        }
        private void click(Point pt)
        {
            POINTAPI temp = point(pt.X, pt.Y);
            ClientToScreen(PSN.hwnd, ref temp);
            mouse_event(MOUSEEVENTF_LEFTDOWN, temp.x, temp.y, 0, 0);//点击
            mouse_event(MOUSEEVENTF_LEFTUP, temp.x, temp.y, 0, 0);//抬起
        }
        private bool MovoTo(int X,int Y)
        {
            return MovoTo(new Point(X,Y));
        }
        //移动到某位置
        private bool MovoTo(Point pt)
        {
            string txt = "当前坐标：";
            pt = new Point(pt.Y,pt.X);
            Point ptn = new Point();
            if (GetPosCurrent(ref ptn) != 1) return false;
            txt += ptn.X.ToString() + "," + ptn.Y.ToString() + "\r\n";
            txt += CFG.pos_map_mid.X.ToString() + "," + CFG.pos_map_mid.Y.ToString() + "\r\n";
            if(!OpenMapEx())return false;
            PSN.PosNow = new Point(ptn.Y,ptn.X);
            Point ptNew=MAPP.ClientToScreenInt(ptn,CFG.pos_map_mid,pt);
            setPoint(ptNew);
            Thread.Sleep(200);
            click(ptNew);
            txt += ptNew.X.ToString() + "," + ptNew.Y.ToString();
            textBox3.Text = txt;
            return true;
        }
        private bool OpenMapEx()
        {
            if(OpenMap() != 1){
                SendKeyA(Keys.Escape);
                if(OpenMap() != 1){
                    SendKeyA(Keys.Escape);
                    if(OpenMap() != 1)return false;
                }
            }
            return true;
        }
        //1-成功  0-无客户端  -1-执行失败
        private int OpenMap()
        {
            Thread.Sleep(300);
            if (!IsClientOK()) return 0;
            Thread.Sleep(200); 
            SendKeyA(Keys.Tab);
            Thread.Sleep(200);
            Bitmap image = GetImageEx(CFG.rect_map_text, 1);
            //pictureBox1.Image = image;
            string txt = GetChiSim(image).Replace("\n\n", "");
            textBox2.Text = txt;
            if (CFG.CheckMap(txt, "城市地图")) return 1;
            return -1;
        }
        //1-成功  0-无客户端  -1-执行失败
        private int GetPosCurrent(ref Point pt)
        {
            if (!IsClientOK()) return 0;
            Bitmap image = GetImageEx(CFG.rect_pos_now,1);
            COUT("正在识别...");
            string txt = getNumber(image);
            COUT("当前坐标：" + txt);
            Point ptt = new Point();
            txt=txt.Trim();
            txt=txt.Replace(". ", ",");
            txt = txt.Replace(", ", ",");
            txt = txt.Replace("， ", ",");
            textBox2.Text = txt;
            string[] ls=txt.Split(new char[3] {'.',',',' '});
            if (ls.Length != 2) return -1;
            ptt.X = int.Parse(ls[0]);
            ptt.Y = int.Parse(ls[1]);
            if (ptt.X.ToString().Length != ls[0].Length || ptt.Y.ToString().Length != ls[1].Length) return -1;
            pt.X = ptt.Y;
            pt.Y = ptt.X;
            return 1;
        }
        //获取当前地图名称
        private string GetMap()
        {
            Bitmap image = GetImageEx(CFG.rect_map_now, 1);
            Bitmap bmp = new Bitmap(image.Width, image.Height);
            bmp = image.Clone(new Rectangle(1, 0, image.Width - 3, image.Height - 2), image.PixelFormat);
            lbmap.Text = "正在识别...";
            string txt = GetChiSim(bmp);
            lbmap.Text = "当前地图：" + txt;
            return txt;
        }
        private string getNumber(Bitmap image)
        {
            UnCodebase ud = new UnCodebase(image);
            ud.DealMap2();
            image = ud.bmpobj;
            pictureBox1.Image = image;
            return ocrNumber.Recognize(image);
        }
        private void button7_Click(object sender, EventArgs e)
        {
            CheckPos(new Point(424,297));

            return;
            Rectangle rt = new Rectangle(350,350,200,200);
            Bitmap bp = GetImageEx(CFG.rect_find, 1);
            pictureBox1.Image = bp;
            string[] ls = textBox1.Text.Split(new char[3] { '.', ',', ' ' });
            //Point pt = new Point(int.Parse(ls[0]), int.Parse(ls[1]));
            //MovoTo(215, 273);
            

            //MAPP.ClientToScreenInt(,);
            return;

        }
        private void COUT(string s)
        {
            lbadd.Text = s;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Point pt = new Point();
            GetPosCurrent(ref pt);
            lbadd.Text = "当前坐标：" + pt.X + "," + pt.Y;
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            GetMap();
        }
        //**************截图，保存
        private string GetImage(Rectangle rect)
        {
            //SetCursorPos(rect.X,rect.Y);
            Bitmap image = new Bitmap(rect.Width, rect.Height);
            Graphics imgGraphics = Graphics.FromImage(image);
            imgGraphics.CopyFromScreen(rect.X, rect.Y, 0, 0, new Size(rect.Width, rect.Height));            
            //以JPG文件格式来保存 
            string filePath = imagePath + DateTime.Now.ToFileTime().ToString() + ".jpg";
            image.Save(filePath, ImageFormat.Jpeg);
            pictureBox1.Image = Image.FromFile(filePath);
            return filePath;
        }
        private Bitmap GetImageEx(Rectangle rect,int style=0)
        {
            //SetCursorPos(rect.X,rect.Y);
            Bitmap image = new Bitmap(rect.Width, rect.Height);
            Graphics imgGraphics = Graphics.FromImage(image);
            POINTAPI pt = new POINTAPI();
            pt.x = rect.X; pt.y = rect.Y;
            if (style != 0) ClientToScreen(PSN.hwnd, ref pt);
            imgGraphics.CopyFromScreen(pt.x, pt.y, 0, 0, new Size(rect.Width, rect.Height));
            string filePath = imagePath + DateTime.Now.ToFileTime().ToString() + ".jpg";
            image.Save(filePath, ImageFormat.Jpeg);
            return image;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            Point pt = new Point();
            int clrValue=0;
            GetCursorPos(ref pt);
            POINTAPI ptt = point(pt.X,pt.Y);
            ScreenToClient(PSN.hwnd,ref ptt);
            label1.Text = ptt.x.ToString() + "," + ptt.y.ToString();
            Color clr=GetColor(new IntPtr(0), pt.X, pt.Y,ref clrValue);
            label2.BackColor = clr;
            label2.Text = clr.R.ToString() + "," + clr.G.ToString() + "," + clr.B.ToString() + "," + clrValue.ToString()+"\n";
            label2.Text += clr.GetHue().ToString() + "," + clr.GetSaturation().ToString() + "," + clr.GetBrightness().ToString();
            
            //label1.Text = PSN.hwnd.ToString() + "," + thKill.ThreadState.ToString() + "," + thKill.IsAlive.ToString();
            IntPtr hwnd = WindowFromPoint(pt.X, pt.Y);
            label4.Text = hwnd.ToString(); 
            //label3.Text=

            
        }
        /*
        private string GetNumber(Bitmap image)
        {
            UnCodebase ud = new UnCodebase(image);
            ud.DealMap2();
            image = ud.bmpobj;
            pictureBox1.Image = image;
            if (!bInitT)
            {
                ocrNumber = new TesseractProcessor();
                bool inited = ocrNumber.Init(@".\", "chi_sim", 3);
                if (!inited) { MessageBox.Show("智能识别引擎初始化失败！"); return ""; }
                ocrNumber.SetVariable("tessedit_char_whitelist", ".,3214567890 ");
            }
            ocrNumber.Clear();
            ocrNumber.ClearAdaptiveClassifier();
            string result = ocrNumber.Recognize(image);
            return result;
        }
         * */
        private string GetChiSim(Bitmap image)
        {
            UnCodebase ud = new UnCodebase(image); 
            ud.DealText();
            image = ud.image;
            //pictureBox1.Image = image; textBox3.Text = ud.txt;
            if (!bInitT)
            {
                ocrText = new TesseractProcessor();
                bInitT = ocrText.Init(@".\", "chi_sim", 3);
                if (!bInitT) { MessageBox.Show("智能识别引擎初始化失败！"); return ""; }
                ocrText.SetVariable("tessedit_char_blacklist", "1234567890μqwertyuiopasdfghjklzxcvbnm'I ﬂU]M.H ﬂO");
            }
            ocrText.Clear();
            ocrText.ClearAdaptiveClassifier();
            string result = ocrText.Recognize(image);
            return result;
        }

       
        private void button4_Click(object sender, EventArgs e)
        {
            if (!IsClientOK()) return; ;
            SendKeyA(Keys.Tab);
            
            
            SetWindowPos(PSN.hwnd, IntPtr.Zero, CFG.rect_client.Left, CFG.rect_client.Top, CFG.rect_client.Width, CFG.rect_client.Height, SWP_SHOWWINDOW);

        }
        //线程控制检测
        private void Check()
        {
            if (stopKill) thKill.Abort();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            if (thKill.IsAlive) thKill.Abort();
            thKill = new Thread(new ThreadStart(AutoKill));
            thKill.Start();
            stopKill = false;

        }
        private void AutoKill()
        {
            if (!IsClientOK()) return; 
            int tm = 0;
            while (true)
            {
                Thread.Sleep(500);
                SendKeyA((Keys)18, (Keys)192);
                for (int i = 0; i < 12; i++)
                {
                    Thread.Sleep(300);
                    SendKeyA(Keys.D3);
                    SendKeyA((Keys)192);
                }
                Check();
                SendKeyA((Keys)18, (Keys)192);
                for (int i = 0; i < 12; i++)
                {
                    Thread.Sleep(300);
                    SendKeyA(Keys.D3);
                    SendKeyA((Keys)192);
                }
                Check();
                if (tm % 50 == 0)
                {
                    SendKeyA(Keys.D5);
                    Thread.Sleep(500);
                    SendKeyA(Keys.D6);
                    Thread.Sleep(500);
                }
                Thread.Sleep(5000);
                tm += 8;
                if(tm>800)break;
            }
            
            thKill.Abort();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            thKill.Abort();
            stopKill = true;
        }

        private string getTxt(int[][] dt)
        {
            string txt = "";
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    txt += dt[j][i].ToString();
                }
                txt += "\r\n";
            }
            return txt;
        }
        public void RunMoney()
        { 
            
        }
        private void MoveGame(int style)
        {
            switch (style)
            {
                case 0: MoveKey(0); Thread.Sleep(100); MoveKey(1); break;
                case 1: MoveKey(1); Thread.Sleep(100); MoveKey(2); break;
                case 2: MoveKey(2); Thread.Sleep(100); MoveKey(3); break;
                case 3: MoveKey(3); Thread.Sleep(100); MoveKey(0); break;
            }
        }
        private void MoveKey(int style)
        {
            switch (style)
            {
                case 0:SendKeyA(40);break;//上38
                case 1:SendKeyA(40);break;//左37
                case 2:SendKeyA(40);break;//下40
                case 3:SendKeyA(40);break;//右39
            }
        }
        private bool CheckPos(Point target)
        {
            if (!IsClientOK()) return false; ;
            Point now = new Point();
            int r=GetPosCurrent(ref now);
            if (r != 1) if (!IsClientOK()) return false; 
            r = GetPosCurrent(ref now);
            if (r != 1) return false;
            int x = target.X - now.Y;
            int y = target.Y - now.X;
            if (x < 0) MoveGame(1); else if (x > 0) MoveGame(3);
            if (y < 0) MoveGame(0); else if (y > 0) MoveGame(2);
            return true;
        }
        private bool OpenChenShanyan()
        {
            Point pt = new Point();
            bool b = findTarget(Config.Mouse_Talk, ref pt);
            
            
            return false;
        }
        private void button8_Click(object sender, EventArgs e)
        {
            Thread.Sleep(4000);
            SendKeyA(39);
            MessageBox.Show("123"); return;
            Point pt = new Point();
            bool b=findTarget(Config.Mouse_Talk,ref pt);
            if (b) MessageBox.Show(pt.X.ToString());

            return;
            //Point ptn = MAPP.ClientToScreenInt(new Point(246, 234), new Point(422, 335), new Point(299, 199));
            //Bitmap src = new Bitmap("image/number/a.png");
            //pictureBox1.Image = src;//
            int stIndex = 0;//6,42//6
            int w = 12;// int.Parse(textBox1.Text);//36
            //Bitmap n = src.Clone(new Rectangle(568, 6, 12, 25), src.PixelFormat);
            //Bitmap n = src.Clone(new Rectangle(260, 0, 120, src.Height), src.PixelFormat);
            //pictureBox1.Image = n;
            //n.Save("image/number/6.png", ImageFormat.Jpeg);
            //src = n;
            return;
                                 
            //Text = "     " + ptn.X + "," + ptn.Y;
            /*
            return;
            IntPtr hwnd = IntPtr.Zero;   //PSN.hwnd
            IntPtr hdc = GetDC(hwnd);
            POINTAPI pt1 = point(441,319);
            POINTAPI pt2 = point(614,406); 
            MoveToEx(hdc, pt1.x, pt1.y, IntPtr.Zero);
            LineTo(hdc,pt1.x,pt2.y);
            //ReleaseDC(hwnd,hdc);
            */
        }

        private List<Point> getNinePoint(Rectangle rect)
        {
            int M = 3;
            List<Point> ls = new List<Point>();
            int singleX = rect.Width / M;
            int singleY = rect.Height / M;
            int beginX = rect.Left + singleX / 2;
            int beginY = rect.Top + singleY / 2;
            for (int i = 0; i < M; i++)
                for (int j = 0; j < M; j++)
                    ls.Add(new Point(beginX + i * singleX, beginY + j * singleY));
            return ls;
        }
        private bool findTarget(long target,ref Point pt)
        {
            Rectangle rt=CFG.rect_find;
            List<Point> ls=getNinePoint(rt);
            for (int i = 0; i < ls.Count; i++)
            { 
                Point temp=ls[i];
                setPoint(temp);
                Thread.Sleep(200);
                if (CompareMouse(target)) { pt = temp; return true; }
            }
            return false;
        }
        private bool CompareMouse(long target)
        {
            long now = getMouseStyle();
            return now == target;
        }
        private long getMouseStyle()
        {
            CURSORINFO pci = new CURSORINFO();
            pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO)); ;
            bool bl = GetCursorInfo(ref pci);
            return (long)(pci.hCursor);
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
        
            //textBox3.Text = pci.hCursor+","+pci.flags.ToString();
            return;
            POINTAPI pt1 = point(210, 280);
            POINTAPI pt2 = point(482, 146);
            ClientToScreen(PSN.hwnd,ref pt1);
            ClientToScreen(PSN.hwnd, ref pt2);
            IntPtr hw = IntPtr.Zero;   //PSN.hwnd
            IntPtr hdc = GetDC(hw);
            MoveToEx(hdc, pt1.x, pt1.y, IntPtr.Zero);
            LineTo(hdc, pt2.x, pt2.y);

            pt1 = point(496, 454);
            pt2 = point(179, 293);
            ClientToScreen(PSN.hwnd, ref pt1);
            ClientToScreen(PSN.hwnd, ref pt2);
            MoveToEx(hdc, pt1.x, pt1.y, IntPtr.Zero);
            LineTo(hdc, pt2.x, pt2.y);

            pt1 = point(485, 301);
            pt2 = point(325, 223);
            ClientToScreen(PSN.hwnd, ref pt1);
            ClientToScreen(PSN.hwnd, ref pt2);
            MoveToEx(hdc, pt1.x, pt1.y, IntPtr.Zero);
            LineTo(hdc, pt2.x, pt2.y);

            pt1 = point(485, 301);
            pt2 = point(340, 375);
            ClientToScreen(PSN.hwnd, ref pt1);
            ClientToScreen(PSN.hwnd, ref pt2);
            MoveToEx(hdc, pt1.x, pt1.y, IntPtr.Zero);
            LineTo(hdc, pt2.x, pt2.y);

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
       
        /*
        private void End()
        {
            if (_ocrProcessor != null)
            {
                _ocrProcessor.End();
                _ocrProcessor = null;
            }
        }
         * */
        //*********************Win32 Api
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("User32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, uint wMsg, int wParam, int lParam);
        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow", SetLastError = true)]
        public static extern void SetForegroundWindow(IntPtr hwnd);
        [DllImport("User32.dll")]
        public static extern int GetWindowText(IntPtr handle, StringBuilder text, int MaxLen);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern int ShowWindow(IntPtr hwnd, IntPtr nCmdShow);
        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        public static extern int SetCursorPos(int x, int y);
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref Point lpPoint);
        [DllImport("user32", EntryPoint = "ClientToScreen")]
        public static extern int ClientToScreen(IntPtr hwnd, ref POINTAPI lpPoint);
        [DllImport("user32", EntryPoint = "ScreenToClient")]
        public static extern int ScreenToClient(IntPtr hwnd, ref POINTAPI lpPoint);
        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, System.Int32 dwRop);
        [DllImport("gdi32.dll")]
        public static extern int GetPixel(IntPtr hDC, int x, int y);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        [DllImport("user32.dll", EntryPoint = "WindowFromPoint")]//指定坐标处窗体句柄
        public static extern IntPtr WindowFromPoint(int xPoint, int yPoint);
        [DllImport("User32.dll")]
        public static extern void keybd_event(Byte bVk, Byte bScan, Int32 dwFlags, Int32 dwExtraInfo);
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("Gdi32.dll")]static extern int MoveToEx(IntPtr hdc, int x, int y, IntPtr lppoint);
        [DllImport("Gdi32.dll")]static extern int LineTo(IntPtr hdc, int X, int Y);
        [DllImport("Gdi32.dll")]static extern IntPtr SelectObject(IntPtr hdc, IntPtr obj);
        [DllImport("Gdi32.dll")]static extern IntPtr CreatePen(int fnPenStyle, int width, int color);
        [DllImport("user32.dll", EntryPoint = "GetCursorInfo")]private static extern bool GetCursorInfo(ref CURSORINFO cInfo);
        [DllImport("user32.dll")]public static extern UInt32 SendInput(UInt32 nInputs, Input[] pInputs, int cbSize);

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_NOZORDER = 0x0004;
        const UInt32 SWP_NOREDRAW = 0x0008;
        const UInt32 SWP_NOACTIVATE = 0x0010;
        const UInt32 SWP_FRAMECHANGED = 0x0020;
        const UInt32 SWP_SHOWWINDOW = 0x0040;
        const UInt32 SWP_HIDEWINDOW = 0x0080;
        const UInt32 SWP_NOCOPYBITS = 0x0100;
        const UInt32 SWP_NOOWNERZORDER = 0x0200;
        const UInt32 SWP_NOSENDCHANGING = 0x0400;
        const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        public const int SW_HIDE = 0;
        public const int SW_NORMAL = 1;
        public const int SW_MAXIMIZE = 3;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;
        public const int SW_RESTORE = 9;
        public const int SW_SHOWDEFAULT = 10;
        public const int WM_KEYDOWN = 0X100;
        public const int WM_KEYUP = 0X101;
        public readonly int MOUSEEVENTF_LEFTDOWN = 0x0002;//模拟鼠标移动
        public readonly int MOUSEEVENTF_MOVE = 0x0001;//模拟鼠标左键按下
        public readonly int MOUSEEVENTF_LEFTUP = 0x0004;//模拟鼠标左键抬起
        public readonly int MOUSEEVENTF_ABSOLUTE = 0x8000;//鼠标绝对位置
        public readonly int MOUSEEVENTF_RIGHTDOWN = 0x0008; //模拟鼠标右键按下 
        public readonly int MOUSEEVENTF_RIGHTUP = 0x0010; //模拟鼠标右键抬起 
        public readonly int MOUSEEVENTF_MIDDLEDOWN = 0x0020; //模拟鼠标中键按下 
        public readonly int MOUSEEVENTF_MIDDLEUP = 0x0040;// 模拟鼠标中键抬起 

        private void SendKeyDown(short key) 
        { 
            Input[] input = new Input[1]; 
            input[0].type = INPUT.KEYBOARD; 
            input[0].ki.wVk = key;
            input[0].ki.time = Win32.GetTickCount();
            if (Win32.SendInput((uint)input.Length, input, Marshal.SizeOf(input[0])) < input.Length) 
            { throw new Win32Exception(Marshal.GetLastWin32Error()); } 
        }
        private void SendKeyUp(short key) 
        { 
            Input[] input = new Input[1]; 
            input[0].type = INPUT.KEYBOARD; 
            input[0].ki.wVk = key; 
            input[0].ki.dwFlags = KeyboardConstaint.KEYEVENTF_KEYUP;
            input[0].ki.time = Win32.GetTickCount();
            if (Win32.SendInput((uint)input.Length, input, Marshal.SizeOf(input[0])) < input.Length) 
            { throw new Win32Exception(Marshal.GetLastWin32Error()); } 
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct MOUSEINPUT { public int dx; public int dy; public int mouseData; public int dwFlags; public int time; public IntPtr dwExtraInfo;    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct KEYBDINPUT { public short wVk; public short wScan; public int dwFlags; public int time; public IntPtr dwExtraInfo;    }
    [StructLayout(LayoutKind.Explicit)]
    internal struct Input { [FieldOffset(0)]        public int type;        [FieldOffset(4)] public MOUSEINPUT mi;        [FieldOffset(4)] public KEYBDINPUT ki;        [FieldOffset(4)] public HARDWAREINPUT hi;    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct HARDWAREINPUT { public int uMsg; public short wParamL; public short wParamH;    }
    internal class INPUT { public const int MOUSE = 0; public const int KEYBOARD = 1; public const int HARDWARE = 2;    }
    internal static class KeyboardConstaint
    {
        internal static readonly short KEYEVENTF_KEYUP = 0x0002;
    }
    internal static class Win32
    {
        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        internal static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        internal static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("User32.dll", EntryPoint = "SendInput", CharSet = CharSet.Auto)]
        internal static extern UInt32 SendInput(UInt32 nInputs, Input[] pInputs, Int32 cbSize);
        [DllImport("Kernel32.dll", EntryPoint = "GetTickCount", CharSet = CharSet.Auto)]
        internal static extern int GetTickCount();
        [DllImport("User32.dll", EntryPoint = "GetKeyState", CharSet = CharSet.Auto)]
        internal static extern short GetKeyState(int nVirtKey);
        [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
    
}
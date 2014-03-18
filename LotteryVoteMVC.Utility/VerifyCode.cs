using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing.Drawing2D;
using System.Web;

namespace LotteryVoteMVC.Utility
{
    /// <summary>
    /// 验证码操作类
    /// </summary>
    public class VerifyCode
    {
        private string _generateCode = string.Empty;
        private const double PI2 = 6.283185307179586476925286766559;

        /// <summary>
        ///绘制的符号数量
        /// </summary>
        /// <value>The code count.</value>
        public int CodeCount { get; set; }
        /// <summary>
        /// 生成的验证码
        /// </summary>
        /// <value>The generate code.</value>
        public string GenerateCode { get { return _generateCode; } }
        public string FontFamilyStr { get { return FontFamily.Name; } set { FontFamily = new System.Drawing.FontFamily(value); } }
        /// <summary>
        /// 验证码字体
        /// </summary>
        /// <value>The font family.</value>
        public FontFamily FontFamily { get; set; }
        /// <summary>
        /// 字体大小
        /// </summary>
        /// <value>The size of the font.</value>
        public int FontSize { get; set; }
        /// <summary>
        ///是否扭曲
        /// </summary>
        /// <value><c>true</c> if wave; otherwise, <c>false</c>.</value>
        public bool Wave { get; set; }
        /// <summary>
        /// 扭曲方向,true为纵向,false为横向
        /// </summary>
        /// <value><c>true</c> if [wave dir]; otherwise, <c>false</c>.</value>
        public bool WaveDirc { get; set; }
        /// <summary>
        /// 波形扭曲幅度
        /// </summary>
        /// <value>The wave value.</value>
        public int WaveValue { get; set; }
        /// <summary>
        /// 波形起始相位
        /// </summary>
        /// <value>The wave phase.</value>
        public double WavePhase { get; set; }
        /// <summary>
        /// 是否存在干扰线
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has lines; otherwise, <c>false</c>.
        /// </value>
        public bool HasLines { get; set; }
        public VerifyCode(int codeCount)
        {
            CodeCount = codeCount;
            FontFamily = new FontFamily("Arial");
            FontSize = 40;
            Wave = true;
            WaveDirc = false;
            WaveValue = 12;
            WavePhase = 10;
        }

        /// <summary>
        /// 创建图片并输出
        /// </summary>
        /// <param name="context">The context.</param>
        public void CreateImage(HttpContextBase context)
        {
            CreateCheckCodeImage(CreateRandomCode(CodeCount), context);
        }

        /// <summary>
        /// 生成随机数
        /// </summary>
        /// <param name="codeCount">The code count.</param>
        /// <returns></returns>
        private string CreateRandomCode(int codeCount)
        {
            int number;

            Random random = new Random();

            for (int i = 0; i < codeCount; i++)
            {
                //随机整数
                number = random.Next();

                //字符从0-9,A-Z中随机产生,对应的ASCII码为 48-57,65-90
                number = number % 36;

                if (number < 10)
                {
                    number += 48;
                }
                else
                {
                    number += 55;
                }
                _generateCode += ((char)number).ToString();
            }
            return _generateCode;
        }

        /// <summary>
        /// 生成验证码图片
        /// </summary>
        /// <param name="checkCode">The check code.</param>
        /// <param name="context">The context.</param>
        private void CreateCheckCodeImage(string checkCode, HttpContextBase context)
        {
            //若验证码为空,直接返回
            if (string.IsNullOrEmpty(checkCode))
                return;
            //根据验证码的长度确定输出图片的长度
            Bitmap image = new Bitmap((int)Math.Ceiling((double)(checkCode.Length * FontSize)), (int)(FontSize * 1.8));
            //创建Graphs对象
            Graphics g = Graphics.FromImage(image);

            try
            {
                Random random = new Random();
                //清空图片背景色
                g.Clear(Color.White);
                if (HasLines)
                {
                    //画图片的背景噪音线25条
                    for (int i = 0; i < 25; i++)
                    {
                        //噪音线的坐标(x1,y1),(x2,y2)
                        int x1 = random.Next(image.Width);
                        int x2 = random.Next(image.Width);
                        int y1 = random.Next(image.Height);
                        int y2 = random.Next(image.Height);
                        //用银色画出噪音线
                        g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                    }
                }
                //输出图片中的验证码
                Font font = new Font(FontFamily, FontSize, (FontStyle.Bold | FontStyle.Italic));
                //线性渐变笔刷
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Green, RandomColor(), 1.2f, true);
                g.DrawString(checkCode, font, brush, 0, 0);

                //图片的前景噪音点 
                for (int i = 0; i < 50; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);
                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }
                //画图片的边框线
                g.DrawRectangle(new Pen(Color.Peru), 0, 0, image.Width - 1, image.Height - 1);

                if (Wave)//是否扭曲
                    image = TwistImage(image, WaveDirc, WaveValue, WavePhase);

                //创建内存刘用于输出图片
                using (MemoryStream ms = new MemoryStream())
                {
                    //图片格式指定为JPG
                    image.Save(ms, ImageFormat.Jpeg);
                    //清空缓冲区流中的所有输出
                    context.Response.ClearContent();
                    //输出流的HTTP MIME类型设置
                    context.Response.ContentType = "image/Jpeg";
                    //输出图片的二进制流
                    context.Response.BinaryWrite(ms.ToArray());
                }
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }

        /// <summary>
        /// 正弦曲线Wave扭曲图片
        /// </summary>
        /// <param name="srcBmp">源图片.</param>
        /// <param name="bxDir">扭曲方向</param>
        /// <param name="dMultValue">波形的幅度倍数,越大扭曲的程度越高,一般为3</param>
        /// <param name="dPhase">波形的起始相位,取值区间[0-2*PI]</param>
        /// <returns></returns>
        private Bitmap TwistImage(Bitmap srcBmp, bool bxDir, double dMultValue, double dPhase)
        {
            Bitmap destBmp = new Bitmap(srcBmp.Width, srcBmp.Height);

            //将位图背景填充为白色
            Graphics graph = Graphics.FromImage(destBmp);
            graph.FillRectangle(new SolidBrush(Color.White), 0, 0, destBmp.Width, destBmp.Height);
            graph.Dispose();

            double dBaseAxisLen = bxDir ? (double)destBmp.Height : (double)destBmp.Width;

            for (int i = 0; i < destBmp.Width; i++)
            {
                for (int j = 0; j < destBmp.Height; j++)
                {
                    double dx = 0;
                    dx = bxDir ? (PI2 * (double)j) / dBaseAxisLen : (PI2 * (double)i) / dBaseAxisLen;
                    dx += dPhase;
                    double dy = Math.Sin(dx);

                    //取得当前点的颜色
                    int nOldX = 0, nOldY = 0;
                    nOldX = bxDir ? i + (int)(dy * dMultValue) : i;
                    nOldY = bxDir ? j : j + (int)(dy * dMultValue);

                    Color color = srcBmp.GetPixel(i, j);
                    if (nOldX >= 0 && nOldX < destBmp.Width && nOldY >= 0 && nOldY < destBmp.Height)
                    {
                        destBmp.SetPixel(nOldX, nOldY, color);
                    }
                }
            }
            return destBmp;
        }

        /// <summary>
        /// 随机颜色
        /// </summary>
        /// <returns></returns>
        private Color RandomColor()
        {
            Random randomNumFirst = new Random(DateTime.Now.Millisecond);
            Random randomNumSencond = new Random(DateTime.Now.Millisecond);

            int red = randomNumFirst.Next(256);
            int green = randomNumSencond.Next(256);
            int blue = (red + green > 400) ? 0 : 400 - red - green;
            blue = blue > 255 ? 255 : blue;
            return Color.FromArgb(red, green, blue);
        }
    }
}

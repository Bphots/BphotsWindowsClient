using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ImageProcessor;
using ImageProcessor.ImageProcessing;
using ImageProcessor.Ocr;

namespace OcrDebugger
{
    class Program
    {
        static void Main(string[] args)
        {
            string a = @"拉格纳罗斯阿巴瑟祖尔金加尔鲁什阿努巴拉克阿塔尼斯阿尔萨斯阿兹莫丹光明之翼陈迪亚波罗精英牛头人酋长弗斯塔德加兹鲁";
            var sb1 = new StringBuilder();
            for (int i = 0; i < 55; i++)
            {
                int x = 29 + (i)*35;
                int y = 260;
                int width = x + 27;
                int height = 285;
                sb1.AppendLine(a[i] + " " + x + " " + y + " " + width + " " + height + " 0");
            }
            string b = @"维伊利丹吉安娜安娜乔汉娜凯尔萨斯凯瑞甘卡拉辛姆李奥瑞克克尔苏加德丽丽莫拉莉斯中尉玛法里奥穆拉丁奔波尔霸纳兹波诺";
            for (int i = 0; i < 55; i++)
            {
                int x = 29 + (i) * 35;
                int y = 215;
                int width = x + 27;
                int height = 240;
                sb1.AppendLine(b[i] + " " + x + " " + y + " " + width + " " + height + " 0");
            }
            string c = @"娃雷诺雷加尔雷克萨重锤军士桑娅缝合怪希尔瓦娜斯塔萨达尔屠夫失落的维京人萨尔泰凯斯泰瑞尔泰兰德乌瑟尔维拉半藏扎加";
            for (int i = 0; i < 55; i++)
            {
                int x = 29 + (i) * 35;
                int y = 170;
                int width = x + 27;
                int height = 195;
                sb1.AppendLine(c[i] + " " + x + " " + y + " " + width + " " + height + " 0");
            }
            string d = @"拉泽拉图古加尔露娜拉格雷迈恩李敏祖尔德哈卡猎空克罗米麦迪文古尔丹奥莉尔马萨伊尔阿拉纳克查莉娅萨穆罗瓦里安瓦莉拉";
            for (int i = 0; i < 55; i++)
            {
                int x = 29 + (i) * 35;
                int y = 125;
                int width = x + 27;
                int height = 150;
                sb1.AppendLine(d[i] + " " + x + " " + y + " " + width + " " + height + " 0");
            }
            string e = @"源氏普罗比斯卡西娅卢西奥狂鼠阿莱克丝塔萨斯托科夫布雷泽正在选择中";
            for (int i = 0; i < 32; i++)
            {
                int x = 29 + (i) * 35;
                int y = 80;
                int width = x + 27;
                int height = 105;
                sb1.AppendLine(e[i] + " " + x + " " + y + " " + width + " " + height + " 0");
            }
            var text = sb1.ToString();
            Console.Write(text);


            OcrEngine.Debug = true;
            if (args.Length >= 4)
            {

                foreach (var hero in OcrEngineLatin.Candidates)
                {
                    OcrEngine.CandidateHeroes.Add(hero);
                }
            }
            else
            {

                foreach (var hero in OcrEngineAsian.Heroes)
                {
                    OcrEngine.CandidateHeroes.Add(hero);
                }
            }
            
            var angle = args[2].Trim() == "L" ? (float)29.7 : (float)-29.7;
            using (
                var recognizer = new Recognizer(args.Length >= 4 ? OcrLanguage.English : OcrLanguage.SimplifiedChinese,
                    args[0]))
            {

                var sb = new StringBuilder();
                recognizer.Recognize(args[1], angle, sb, "1");
                Console.WriteLine(sb.ToString());
                Console.WriteLine(@"Press any key to exit.");
                Console.ReadKey();
            }
        }


    }
}

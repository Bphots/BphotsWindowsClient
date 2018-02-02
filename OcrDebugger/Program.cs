﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DotNetHelper;
using ImageProcessor;
using ImageProcessor.ImageProcessing;
using ImageProcessor.Ocr;
using StatsFetcher;

namespace OcrDebugger
{
    class Program
    {
        static void Main(string[] args)
        {
            FileProcessor.ProcessLobbyFile(@"D:\qqytqqyt\Documents\HeroesBpProject\TempWriteReplayP1\replay.server.battlelobby");

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
            string f = @"沃斯卡娅铸造厂花村诅咒谷弹头枢纽站布莱克西斯禁区失落洞窟末日塔炼狱圣坛永恒战场蛛后墓天空殿恐魔园鬼灵矿巨龙镇";
            for (int i = 0; i < 54; i++)
            {
                int x = 53 + (i) * 35;
                int y = 72;
                int width = x + 27;
                int height = 97;
                sb1.AppendLine(f[i] + " " + x + " " + y + " " + width + " " + height + " 0");
            }
            var text = sb1.ToString();
            Print();
            foreach (var hero in OcrEngineAsian.Heroes)
            {
                OcrEngine.CandidateHeroes.Add(hero);
            }
            OcrEngine.Debug = true;
            var dir = @"D:\qqytqqyt\Documents\HeroesBpProject\test\AutoTest\";
            var recognizer = new Recognizer(OcrLanguage.SimplifiedChinese, dir);
            int correctCount = 0;
            int sum = 0;
            foreach (FilePath path in Directory.GetFiles(dir + @"left"))
            {
                if (File.Exists(path) && (path.GetFileExt() == ".tiff" || path.GetFileExt() == ".bmp"))
                {
                    var sb = new StringBuilder();
                    // This path is a file
                    recognizer.Recognize(path, (float)29.7, sb, 3);
                    var correct = !string.IsNullOrEmpty(sb.ToString()) && path.GetFileNameWithoutExtension().StartsWith(sb.ToString());
                    if (correct)
                        correctCount++;
                    sum++;
                    Console.WriteLine(sb + " " + (correct ? correct.ToString() : path.GetFileNameWithoutExtension()));
                }
            }
            foreach (FilePath path in Directory.GetFiles(dir + @"right"))
            {
                if (File.Exists(path) && (path.GetFileExt() == ".tiff" || path.GetFileExt() == ".bmp"))
                {
                    var sb = new StringBuilder();
                    // This path is a file
                    recognizer.Recognize(path, (float)-29.7, sb, 3);
                    var correct = !string.IsNullOrEmpty(sb.ToString()) && path.GetFileNameWithoutExtension().StartsWith(sb.ToString());
                    if (correct)
                        correctCount++;
                    sum++;
                    Console.WriteLine(sb + " " + (correct ? correct.ToString() : (path.GetFileNameWithoutExtension() + " " + correct)));
                }
            }
            Console.WriteLine(correctCount + " / " + sum);
            //var sb = new StringBuilder();
            //recognizer.Recognize(args[1], angle, sb, "1");
            //Console.WriteLine(sb.ToString());
            //Console.WriteLine(@"Press any key to exit.");
            Console.ReadKey();

            //if (args.Length >= 4)
            //{

            //    foreach (var hero in OcrEngineLatin.Candidates)
            //    {
            //        OcrEngine.CandidateHeroes.Add(hero);
            //    }
            //}
            //else
            //{

            //    foreach (var hero in OcrEngineAsian.Heroes)
            //    {
            //        OcrEngine.CandidateHeroes.Add(hero);
            //    }
            //}
            
            //var angle = args[2].Trim() == "L" ? (float)29.7 : (float)-29.7;
            //using (
            //    var recognizer = new Recognizer(args.Length >= 4 ? OcrLanguage.English : OcrLanguage.SimplifiedChinese,
            //        args[0]))
            //{

            //    var sb = new StringBuilder();
            //    recognizer.Recognize(args[1], angle, sb, "1");
            //    Console.WriteLine(sb.ToString());
            //    Console.WriteLine(@"Press any key to exit.");
            //    Console.ReadKey();
            //}
        }


        private static void Print()
        {
            string a = @"拉格纳罗斯阿巴瑟祖尔金加尔鲁什阿努巴拉克阿塔尼斯阿尔";
            var sb1 = new StringBuilder();
            for (int i = 0; i < 26; i++)
            {
                int x = 106 + (i) * 42;
                int y = 548;
                int width = x + 27;
                int height = 575;
                sb1.AppendLine(a[i] + " " + x + " " + y + " " + width + " " + height + " 0");
            }
            string b = @"萨斯阿兹莫丹光明之翼陈迪亚波罗精英牛头人酋长弗斯塔德";
            for (int i = 0; i < 26; i++)
            {
                int x = 106 + (i) * 42;
                int y = 506;
                int width = x + 27;
                int height = 533;
                sb1.AppendLine(b[i] + " " + x + " " + y + " " + width + " " + height + " 0");
            }
            string c = @"加兹鲁维伊利丹吉安娜安娜乔汉娜凯尔萨斯凯瑞甘卡拉辛姆";
            for (int i = 0; i < 26; i++)
            {
                int x = 106 + (i) * 42;
                int y = 464;
                int width = x + 27;
                int height = 491;
                sb1.AppendLine(c[i] + " " + x + " " + y + " " + width + " " + height + " 0");
            }
            string d = @"李奥瑞克克尔苏加德丽丽莫拉莉斯中尉玛法里奥穆拉丁奔波";
            for (int i = 0; i < 26; i++)
            {
                int x = 106 + (i) * 42;
                int y = 422;
                int width = x + 27;
                int height = 449;
                sb1.AppendLine(d[i] + " " + x + " " + y + " " + width + " " + height + " 0");
            }
            string e = @"尔霸纳兹波诺娃雷诺雷加尔雷克萨重锤军士桑娅缝合怪希尔";
            for (int i = 0; i < 26; i++)
            {
                int x = 106 + (i) * 42;
                int y = 380 + 1;
                int width = x + 27;
                int height = 407 + 1;
                sb1.AppendLine(e[i] + " " + x + " " + y + " " + width + " " + height + " 0");
            }
            string f = @"瓦娜斯塔萨达尔屠夫失落的维京人萨尔泰凯斯泰瑞尔泰兰德";
            for (int i = 0; i < 26; i++)
            {
                int x = 106 + (i) * 42;
                int y = 338 + 1;
                int width = x + 27;
                int height = 365 + 1;
                sb1.AppendLine(f[i] + " " + x + " " + y + " " + width + " " + height + " 0");
            }
            string g = @"乌瑟尔维拉半藏扎加拉泽拉图古加尔露娜拉格雷迈恩李敏祖";
            for (int i = 0; i < 26; i++)
            {
                int x = 106 + (i) * 42;
                int y = 296 + 2;
                int width = x + 27;
                int height = 323 + 2;
                sb1.AppendLine(g[i] + " " + x + " " + y + " " + width + " " + height + " 0");
            }
            string h = @"尔德哈卡猎空克罗米麦迪文古尔丹奥莉尔马萨伊尔阿拉纳克";
            for (int i = 0; i < 26; i++)
            {
                int x = 106 + (i) * 42;
                int y = 254 + 2;
                int width = x + 27;
                int height = 281 + 2;
                sb1.AppendLine(h[i] + " " + x + " " + y + " " + width + " " + height + " 0");
            }
            string ii = @"查莉娅萨穆罗瓦里安瓦莉拉源氏普罗比斯卡西娅卢西奥狂鼠";
            for (int i = 0; i < 26; i++)
            {
                int x = 106 + (i) * 42;
                int y = 212 + 2;
                int width = x + 27;
                int height = 239 + 2;
                sb1.AppendLine(ii[i] + " " + x + " " + y + " " + width + " " + height + " 0");
            }
            string j = @"阿莱克丝塔萨斯托科夫布雷泽正在选择中沃斯卡娅铸造厂花";
            for (int i = 0; i < 26; i++)
            {
                int x = 106 + (i) * 42;
                int y = 170 + 3;
                int width = x + 27;
                int height = 197 + 3;
                sb1.AppendLine(j[i] + " " + x + " " + y + " " + width + " " + height + " 0");
            }
            string k = @"村诅咒谷弹头枢纽站布莱克西斯禁区失落洞窟末日塔炼狱圣";
            for (int i = 0; i < 26; i++)
            {
                int x = 106 + (i) * 42;
                int y = 128 + 3;
                int width = x + 27;
                int height = 155 + 3;
                sb1.AppendLine(k[i] + " " + x + " " + y + " " + width + " " + height + " 0");
            }
            string l = @"坛永恒战场蛛后墓天空殿恐魔园鬼灵矿巨龙镇黑心湾";
            for (int i = 0; i < 23; i++)
            {
                int x = 106 + (i) * 42;
                int y = 86 + 4;
                int width = x + 27;
                int height = 113 + 4;
                sb1.AppendLine(l[i] + " " + x + " " + y + " " + width + " " + height + " 0");
            }
            var text = sb1.ToString();
        }

    }
}
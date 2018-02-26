using System;
using System.Collections.Generic;
using System.Linq;
using Tesseract;

namespace ImageProcessor.Ocr
{
    public class OcrEngineSimplifiedChinese : OcrEngineAsian
    {
        public OcrEngineSimplifiedChinese()
        {
            Engine = new TesseractEngine(@".\tessdata\zhCN", "chi_hots", EngineMode.TesseractOnly);
            Engine.DefaultPageSegMode = PageSegMode.SingleWord;
            PickingText = "正在选择中";
            CandidateHeroes.Add(PickingText);
        }

        public static List<string> TessdataFilePaths => new List<string>
        {
            TessdataBasePath + @"zhCN\tessdata\chi_hots.traineddata"
        };
    }

    public class OcrEngineTraditionalChinese : OcrEngineAsian
    {
        public OcrEngineTraditionalChinese()
        {
            Engine = new TesseractEngine(@".\tessdata\zhTW", "chi_tra", EngineMode.TesseractOnly);
            Engine.DefaultPageSegMode = PageSegMode.SingleWord;
            PickingText = "正在選擇";
            CandidateHeroes.Add(PickingText);
        }

        public static List<string> TessdataFilePaths => new List<string>
        {
            TessdataBasePath + @"zhTW\tessdata\chi_tra.config",
            TessdataBasePath + @"zhTW\tessdata\chi_tra.traineddata"
        };
    }

    public class OcrEngineAsian : OcrEngine
    {
        private const int FullyMatchScore = 2;

        public static readonly List<string> Maps = new List<string>
        {
            "沃斯卡娅铸造厂",
            "花村",
            "诅咒谷",
            "弹头枢纽站",
            "布莱克西斯禁区",
            "失落洞窟",
            "末日塔",
            "炼狱圣坛",
            "永恒战场",
            "蛛后墓",
            "天空殿",
            "恐魔园",
            "鬼灵矿",
            "巨龙镇"
        };

        public static readonly List<string> Heroes = new List<string>
        {
            "拉格纳罗斯",
            "阿巴瑟",
            "祖尔金",
            "加尔鲁什",
            "阿努巴拉克",
            "阿塔尼斯",
            "阿尔萨斯",
            "阿兹莫丹",
            "光明之翼",
            "陈",
            "迪亚波罗",
            "玛维",
            "精英牛头人酋长",
            "弗斯塔德",
            "加兹鲁维",
            "伊利丹",
            "吉安娜",
            "安娜",
            "乔汉娜",
            "凯尔萨斯",
            "凯瑞甘",
            "卡拉辛姆",
            "李奥瑞克",
            "克尔苏加德",
            "丽丽",
            "莫拉莉斯中尉",
            "玛法里奥",
            "穆拉丁",
            "奔波尔霸",
            "纳兹波",
            "诺娃",
            "雷诺",
            "雷加尔",
            "雷克萨",
            "重锤军士",
            "桑娅",
            "缝合怪",
            "希尔瓦娜斯",
            "塔萨达尔",
            "屠夫",
            "失落的维京人",
            "萨尔",
            "泰凯斯",
            "泰瑞尔",
            "泰兰德",
            "乌瑟尔",
            "维拉",
            "半藏",
            "扎加拉",
            "泽拉图",
            "古",
            "加尔",
            "露娜拉",
            "格雷迈恩",
            "李敏",
            "祖尔",
            "德哈卡",
            "猎空",
            "克罗米",
            "麦迪文",
            "古尔丹",
            "奥莉尔",
            "马萨伊尔",
            "阿拉纳克",
            "查莉娅",
            "萨穆罗",
            "瓦里安",
            "瓦莉拉",
            "源氏",
            "普罗比斯",
            "卡西娅",
            "卢西奥",
            "狂鼠",
            "阿莱克丝塔萨",
            "斯托科夫",
            "布雷泽",
            "D.Va",
            "正在选择中"
        };

        private static int CalculateScore(string original, string target)
        {
            var score = 0;
            original = original.Replace("\n", string.Empty);
            if (original.Trim().Length == target.Length)
                original = original.Trim();
            if (original.Replace(" ", string.Empty).Length == target.Length)
                original = original.Replace(" ", string.Empty);
            if (original.Length == target.Length)
            {
                for (var i = 0; i < original.Length; ++i)
                {
                    if (original[i] == target[i])
                        score += FullyMatchScore;
                }
            }
            else
            {
                for (var i = 0; i < original.Length; ++i)
                {
                    if (target.Contains(original[i]))
                    {
                        score += 1;
                        if (i < original.Length && i < target.Length && original[i] == target[i])
                            score += 1;
                    }
                }
            }

            return score;
        }

        public static bool LookFor(TesseractEngine engine, string path, string textToLookFor)
        {
            engine.SetVariable("tessedit_char_whitelist", textToLookFor);
            using (var pix = Pix.LoadFromFile(path))
            using (var page = engine.Process(pix))
            {
                var text = page.GetText();
                return text.Replace("\n", string.Empty).Replace(" ", string.Empty) == textToLookFor;
            }
        }

        private static bool IsMathcingDva(string text)
        {
            return text.Contains("VA");
        }


        public override OcrResult ProcessOcr(double count, string path, HashSet<string> candidates)
        {
            var ocrResult = new OcrResult();

            var checkDva = false;
            var textCount = (int) count;
            var heroesSet = new HashSet<char>();
            foreach (
                var character in candidates.Where(c => c.Count() == textCount).SelectMany(c => c).Where(c => c > 255))
                heroesSet.Add(character);
            if (textCount == 2 && Math.Abs(count - 2.5) < 0.35)
            {
                checkDva = true;
                heroesSet.Add('D');
                heroesSet.Add('V');
                heroesSet.Add('A');
            }

            if (!heroesSet.Any())
                return ocrResult;

            var whiteList = heroesSet.Aggregate(string.Empty, (current, character) => current + character);
            Engine.SetVariable("tessedit_char_whitelist", whiteList);

            using (var pix = Pix.LoadFromFile(path))
            using (var page = Engine.Process(pix))
            {
                var text = page.GetText();
                if (checkDva && IsMathcingDva(text))
                {
                    ocrResult.Results["D.Va"] = new MatchResult
                    {
                        Key = text,
                        Value = "D.Va",
                        InDoubt = false,
                        Score = 6,
                        Trustable = true,
                        FullyTrustable = true
                    };
                    return ocrResult;
                }

                var textTrim = text.Trim();
                if (textCount == 2 && textTrim.Length >= 2 && textTrim[0] != '丽' && textTrim[0] == textTrim[1])
                {
                    text = @"丽X";
                }

                foreach (var hero in candidates.Where(s => s.Length == textCount))
                {
                    var score = CalculateScore(text, hero);

                    if (score > 0)
                    {
                        var trustable = !checkDva &&
                                        ((score/(double) (textCount*FullyMatchScore) > 0.66 && count >= 2) ||
                                         (score/(double) (textCount*FullyMatchScore) >= 0.5 && count >= 4));
                        if (!trustable && hero == "加尔")
                            continue;

                        ocrResult.Results[hero] = new MatchResult
                        {
                            Key = text,
                            Value = hero,
                            InDoubt = score/(double) (textCount*FullyMatchScore) <= 0.5 || textCount <= 1,
                            Score = score,
                            Trustable = trustable,
                            FullyTrustable = textCount >= 2 && score == textCount*FullyMatchScore
                        };
                    }
                }
                return ocrResult;
            }
        }

        public override OcrResult ProcessOcr(string path, HashSet<string> candidates)
        {
            var ocrResult = new OcrResult();

            var heroesSet = new HashSet<char>();
            foreach (var character in candidates.SelectMany(c => c))
                heroesSet.Add(character);
            if (!heroesSet.Any())
                return ocrResult;

            var whiteList = heroesSet.Aggregate(string.Empty, (current, character) => current + character);
            Engine.SetVariable("tessedit_char_whitelist", whiteList);

            var inDoubt = false;
            using (var pix = Pix.LoadFromFile(path))
            using (var page = Engine.Process(pix))
            {
                var text = page.GetText();
                var match = string.Empty;
                var maxScore = 0;
                foreach (var candidate in candidates)
                {
                    var score = CalculateScore(text, candidate);
                    if (maxScore == score)
                        inDoubt = true;
                    if (maxScore < score)
                    {
                        maxScore = score;
                        match = candidate;
                    }
                }
                ocrResult.Results[match] = new MatchResult
                {
                    Key = text,
                    Value = match,
                    InDoubt = inDoubt,
                    Score = maxScore,
                    Trustable = maxScore/(double) (match.Length*FullyMatchScore) >= 0.3,
                    FullyTrustable = maxScore / (double)(match.Length * FullyMatchScore) >= 0.66
                };
                return ocrResult;
            }
        }
    }
}
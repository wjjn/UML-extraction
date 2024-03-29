﻿using Newtonsoft.Json;
using project1;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;

namespace project1
{
    public class words_result
    {
        public Probability probability { get; set; }
        public string words { get; set; }
    }

    public class Probability
    {
        /// <summary>
        /// 行置信度平均值方差
        /// </summary>
        public double variance { get; set; }
        /// <summary>
        /// 行置信度平均值
        /// </summary>
        public double average { get; set; }

        /// <summary>
        /// 行置信度最小值
        /// </summary>
        public double min { get; set; }
    }

    public class Result
    {
        public long log_id { get; set; }
        public int words_result_num { get; set; }
        public List<words_result> words_result { get; set; }
    }
}
public class ImgClass
{
    private string location;
    private Bitmap Name;
    private Bitmap Attribute;
    private Bitmap Function;

    override public string ToString()
    {
        //生成名称
        string name;
        Result r;
        var image = MainWindow.ImgToBase64String(this.Name1);
        var options = new Dictionary<string, object>{
                {"language_type", "CHN_ENG"},
                //{"detect_direction", "true"},
                //{"detect_language", "true"},
                {"probability", "false"}
                };
        string result;
        // 带参数调用通用文字识别, 图片参数为本地图片
        try
        {
            result = MainWindow.client.GeneralBasic(image, options).ToString();
            result = MainWindow.client.GeneralBasic(image, options).ToString();
            r = JsonConvert.DeserializeObject<Result>(result);
            name = r.words_result[0].words;
            return name;
        }
        catch (Exception exp)
        {
            _ = MessageBox.Show(exp.Message);
        }
        return null;
    }

    public ImgClass(Bitmap name, Bitmap attribute, Bitmap function, string location)
    {
        this.Name = name;
        this.Attribute = attribute;
        this.Function = function;
        this.Location = location;
    }

    public string Location { get => location; set => location = value; }
    public Bitmap Name1 { get => Name; set => Name = value; }
    public Bitmap Attribute1 { get => Attribute; set => Attribute = value; }
    public Bitmap Function1 { get => Function; set => Function = value; }

    public bool Totext()
    {
        //生成名称
        //string text;
        Result rA;
        Result rF;
        //属性识别

        var image1 = MainWindow.ImgToBase64String(this.Attribute1);
        var image2 = MainWindow.ImgToBase64String(this.Function1);
        var options = new Dictionary<string, object>{
                {"language_type", "CHN_ENG"},
                //{"detect_direction", "true"},
                //{"detect_language", "true"},
                {"probability", "false"}
                };
        string Attributeresult;
        string Functionresult;
        // 带参数调用通用文字识别, 图片参数为本地图片
        try
        {
            //名称
            Console.WriteLine("------------------------------");
            Console.WriteLine(this.ToString());
            //识别属性
            Attributeresult = MainWindow.client.AccurateBasic(image1, options).ToString();
            rA = JsonConvert.DeserializeObject<Result>(Attributeresult);
            Console.WriteLine("属性共有"+ rA.words_result_num.ToString()+"个");
            foreach (var item in rA.words_result)
            {
                Console.WriteLine(item.words.ToString());
            }

            //识别方法
            Functionresult = MainWindow.client.AccurateBasic(image2, options).ToString();
            rF = JsonConvert.DeserializeObject<Result>(Functionresult);
            Console.WriteLine("方法共有" + rF.words_result_num.ToString() + "个");
            foreach (var item in rF.words_result)
            {
                Console.WriteLine(item.words.ToString());
            }
            return true;
        }
        catch (Exception exp)
        {
            _ = MessageBox.Show(exp.Message);
        }
        return true;
    }
}
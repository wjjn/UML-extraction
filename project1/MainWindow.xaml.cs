using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using AForge;
using AForge.Imaging.Filters;
using System.IO;
using AForge.Imaging;
using AForge.Math.Geometry;
using System.Drawing.Imaging;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace project1
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    


    public partial class MainWindow : Window
    {
        Bitmap source;
        Bitmap classImg;
        private ObservableCollection<ImgClass> classlist;
        List<Bitmap> tempimgs = new List<Bitmap>();
        public ObservableCollection<ImgClass> Classlist { get => classlist; set => classlist = value; }

        public static byte[] ImgToBase64String(Bitmap bmp)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                return arr;
            }
            catch (Exception exp)
            {
                _ = MessageBox.Show(exp.Message);
                return null;
            }
        }
        private BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            Bitmap bitmapSource = new Bitmap(bitmap.Width, bitmap.Height);
            int i, j;
            for (i = 0; i < bitmap.Width; i++)
                for (j = 0; j < bitmap.Height; j++)
                {
                    Color pixelColor = bitmap.GetPixel(i, j);
                    Color newColor = Color.FromArgb(pixelColor.R, pixelColor.G, pixelColor.B);
                    bitmapSource.SetPixel(i, j, newColor);
                }
            MemoryStream ms = new MemoryStream();
            bitmapSource.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(ms.ToArray());
            bitmapImage.EndInit();

            return bitmapImage;
        }

        //设置百度云APPID/AK/SK

        //private string APP_ID = "16876280";
        private static readonly string API_KEY = "75iuZ7hyOdL7oGWowzjmTKbX";
        private static readonly string SECRET_KEY = "UkzoMQGF5XjfLnyNXk4UEzYomgVdqt9D";
        public static Baidu.Aip.Ocr.Ocr client = new Baidu.Aip.Ocr.Ocr(API_KEY, SECRET_KEY);
        //client.Timeout = 60000;  // 修改超时时间

        public void GeneralBasicDemo()
        {
            var image = File.ReadAllBytes("图片文件路径");
            // 调用通用文字识别, 图片参数为本地图片，可能会抛出网络等异常，请使用try/catch捕获
            var result = client.GeneralBasic(image);
            Console.WriteLine(result);
            // 如果有可选参数
            var options = new Dictionary<string, object>
            {
            {"language_type", "CHN_ENG"},
            //{"detect_direction", "true"},
            //{"detect_language", "true"},
            {"probability", "true"}
            };
             
            // 带参数调用通用文字识别, 图片参数为本地图片
            result = client.GeneralBasic(image, options);
            Console.WriteLine(result);
        }
        public void AccurateBasicDemo()
        {
            var image = File.ReadAllBytes("图片文件路径");
            // 调用通用文字识别（高精度版），可能会抛出网络等异常，请使用try/catch捕获
            var result = client.AccurateBasic(image);
            Console.WriteLine(result);
            // 如果有可选参数
            var options = new Dictionary<string, object>{
                {"detect_direction", "true"},
                {"probability", "true"}
                };
            // 带参数调用通用文字识别（高精度版）
            result = client.AccurateBasic(image, options);
            Console.WriteLine(result);
        }
        public MainWindow()
        {
            try
            {
                Console.WriteLine(System.IO.Directory.GetCurrentDirectory());
                source = new Bitmap(@"..\..\Resources\"
            + @"class2.jpg", true);

            }
            catch (ArgumentException)
            {
                MessageBox.Show("图片路径错误");
            }

            //添加灰度滤镜
            FiltersSequence seq = new FiltersSequence
            {
                Grayscale.CommonAlgorithms.BT709,
                new OtsuThreshold()
            };
            Bitmap temp = seq.Apply(source);

            // 从图像中提取blob
            try
            {
                BlobCounter blobCounter = new BlobCounter
                {
                    ObjectsOrder = ObjectsOrder.XY,
                    FilterBlobs = true,
                    MinHeight = 10,
                    MaxHeight = 500,
                    MinWidth = 10,
                    MaxWidth = 500
                };
                blobCounter.ProcessImage(temp);
                Blob[] blobs = blobCounter.GetObjectsInformation();
                SimpleShapeChecker shapeChecker = new SimpleShapeChecker();
                for (int i = 0, n = blobs.Length; i < n; i++)
                {
                    List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);
                    if (shapeChecker.IsQuadrilateral(edgePoints, out List<IntPoint> corners))
                    {
                        if (shapeChecker.CheckPolygonSubType(corners) == PolygonSubType.Rectangle)
                        {
                            //分割图片
                            QuadrilateralTransformation quadTransformer = new QuadrilateralTransformation(corners);
                            classImg = quadTransformer.Apply(temp);
                            classImg.Save("classes" + i.ToString() + ".jpg", ImageFormat.Jpeg);
                            tempimgs.Add(classImg);

                        }
                    }
                }

                //对抽取出的BLob进行排序并检查
                //blobs.OrderBy(Blob => Blob.Rectangle.X).ThenBy(Blob => Blob.Rectangle.Y);

                //初始化Imgclass
                classlist = new ObservableCollection<ImgClass>();
                for (int i = 0, n = blobs.Length; i < n; i += 3)
                {
                    classlist.Add(new ImgClass(tempimgs[i], tempimgs[i + 1], tempimgs[i + 2], (i / 3).ToString()));
                }


            }
            catch (Exception exp)
            {
                _ = MessageBox.Show(exp.Message);
            }

            InitializeComponent();
            classbox.ItemsSource = classlist;
            // Set the PictureBox to display the image.
            SourceImage.Source = BitmapToBitmapImage(source);
            //classbox.DisplayMemberPath = "location";

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {


            // Set the PictureBox to display the image.
            SourceImage.Source = BitmapToBitmapImage(source);

            // Display the pixel format in Label1.
            this.Title = "Pixel format: " + source.PixelFormat.ToString();

            //文字识别

            // 参数
            int i = Convert.ToInt32(classbox.SelectedItem.ToString());
            var image = ImgToBase64String(classlist[i].Name1);

            var options = new Dictionary<string, object>{
                {"language_type", "CHN_ENG"},
                //{"detect_direction", "true"},
                //{"detect_language", "true"},
                {"probability", "true"}
                };
            // 带参数调用通用文字识别, 图片参数为本地图片
            try
            {
                var result = client.GeneralBasic(image, options);
                Console.WriteLine(result);
            }
            catch (Exception exp)
            {
                _ = MessageBox.Show(exp.Message);
            }
        }

        private void Classbox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            img1.Source = BitmapToBitmapImage(classlist[classbox.SelectedIndex].Name1);
            img2.Source = BitmapToBitmapImage(classlist[classbox.SelectedIndex].Attribute1);
            img3.Source = BitmapToBitmapImage(classlist[classbox.SelectedIndex].Function1);
        }
    }
}
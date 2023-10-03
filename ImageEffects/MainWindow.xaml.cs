using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Point = System.Drawing.Point;

namespace ImageEffects
{
    public partial class MainWindow : Window
    {
        bool opened = false;
        string filePath = string.Empty;

        TransformedBitmap bitmap = new TransformedBitmap();
        ColorMatrix cm =  new ColorMatrix(new float[][]
            {
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0} 
            });
        float l, r, g, b;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if(opened)
            {
                var sfd = new SaveFileDialog();
                sfd.Filter = "Image Files(*.jpg; *.jpeg; *.bmp)|*.jpg; *.jpeg; *.bmp";
                sfd.Title = "Save picture as ";
                if (sfd.ShowDialog() == true)
                {
                    SaveFile(sfd.FileName);
                }
            }
        }
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (opened) Reload();
            var openDialog = new OpenFileDialog();
            openDialog.Filter = "Image files (*.JPG, *.PNG)|*.jpg; *.png";

            if (openDialog.ShowDialog() == true)
            {
                opened = true;
                CreateCacheFile();
                filePath = openDialog.FileName;
                bitmap = SetScaleProperly();
                SaveBufferFile();
                pictureBox.Source = bitmap;
            }
        }
        private TransformedBitmap SetScaleProperly()
        {
            var img = new BitmapImage(new Uri(filePath));
            bitmap = img.Height > 1000 || img.Width > 1000 ? bitmap = new TransformedBitmap(img, new ScaleTransform(img.Height / 2 / img.PixelHeight, img.Width / 2 / img.PixelWidth)) :
                bitmap = new TransformedBitmap(img, new ScaleTransform(img.Height / img.PixelHeight, img.Width / img.PixelWidth));
            double koef;
            while (bitmap.Height > paper.Height + 1 || bitmap.Width > paper.Width + 1 || (bitmap.Height > paper.Height + 1 && bitmap.Width > paper.Width + 1))
            {
                if (bitmap.Width > paper.Width + 1)
                {
                    koef = img.Height / img.Width;
                    bitmap = new TransformedBitmap(bitmap, new ScaleTransform(paper.Width * koef / bitmap.PixelHeight, paper.Width / bitmap.PixelWidth));
                }
                else if (bitmap.Height > paper.Height + 1)
                {
                    koef = img.Width / img.Height;
                    bitmap = new TransformedBitmap(bitmap, new ScaleTransform(paper.Height / bitmap.PixelHeight, paper.Height * koef / bitmap.PixelWidth));
                }
            }
            return bitmap;
        }
        private void ApplyFilter()
        {
            if (!opened)
            {
                MessageBox.Show("Please open an image");
            }
            else
            {
                CreateCacheFile();

                var imgHome = new System.Windows.Controls.Image();
                imgHome.Source = BitmapFromUri(new Uri("imageCache.png", UriKind.Relative));
                pictureBox.Source = imgHome.Source;
            }
        }
        private void CreateCacheFile()
        {
            Bitmap newImage = ConvertImageColor();
            newImage.Save("imageCache.png");
        }
        public Bitmap ConvertImageColor()
        {
            Image originalImage = Image.FromFile("buffer.png");
            Bitmap newImage = ToColorTone(originalImage);
            originalImage.Dispose();
            return newImage;
        }
        private Bitmap ToColorTone(Image image)
        {
            ImageAttributes ImAttribute = new ImageAttributes();
            ImAttribute.SetColorMatrix(cm);

            Point[] points =
            {
                new Point(0, 0),
                new Point(image.Width - 1, 0),
                new Point(0, image.Height - 1),
            };
            var rect = new System.Drawing.Rectangle(0, 0, image.Width, image.Height);

            Bitmap myBitmap = new Bitmap(image.Width, image.Height);
            using (Graphics graphics = Graphics.FromImage(myBitmap))
            {
                graphics.DrawImage(image, points, rect, GraphicsUnit.Pixel, ImAttribute);
            }
            return myBitmap;
        }
        public static ImageSource BitmapFromUri(Uri source)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = source;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.EndInit();
            return bitmap;
        }
        private void blue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            b = (float)blueSlider.Value / 100;
            bText.Content = (int)blueSlider.Value;
            cm = SetColorMatrixValue(r, g, b, l);
            ApplyFilter();
        }
        private void green_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            g = (float)greenSlider.Value / 100;
            gText.Content = (int)greenSlider.Value;
            cm = SetColorMatrixValue(r, g, b, l);
            ApplyFilter();
        }
        private void redSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            r = (float)redSlider.Value / 100;
            rText.Content = (int)redSlider.Value;
            cm = SetColorMatrixValue(r, g, b, l);
            ApplyFilter();
        }
        private void alpha_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            l = (float)alphaSlider.Value / 100;
            lText.Content = (int)alphaSlider.Value;
            cm = SetColorMatrixValue(r, g, b, l);
            ApplyFilter();
        }
        private ColorMatrix SetColorMatrixValue(float r, float g, float b, float l)
        {
            return new ColorMatrix(new float[][]
            {
                new float[] {(.5f + r) * 2f, 0, 0, 0, 0}, // red
                new float[] {0, (.5f + g) * 2f, 0, 0, 0}, // green
                new float[] {0, 0, (.5f + b) * 2f, 0, 0}, //blue
                new float[] {0, 0, 0, 1, 0},
                new float[] {l, l, l, 0, 1} // luminocity
            });
        }
        private void reload_Click(object sender, RoutedEventArgs e)
        {
            if(!opened) MessageBox.Show("Please open an image");
            else Reload();
        }
        private void Reload()
        {
            pictureBox.Source = SetScaleProperly();
            redSlider.Value = 0;
            greenSlider.Value = 0;
            blueSlider.Value = 0;
            alphaSlider.Value = 0;
        }
        private void SaveBufferFile()
        {
            JpegBitmapEncoder jpg = new JpegBitmapEncoder();
            jpg.Frames.Add(BitmapFrame.Create(bitmap));
            using (Stream stm = File.Create("buffer.png"))
            {
                jpg.Save(stm);
            }
        }
        private void SaveFile(string path)
        {
            var imageToSave = Image.FromFile("imageCache.png");
            var i2 = new Bitmap(imageToSave);
            i2.Save(path);
            imageToSave.Dispose();
            i2.Dispose();
        }
        private void Cyber_Click(object sender, RoutedEventArgs e)
        {
            cm = SetColorMatrixValue(.34f, -.04f, .44f, -.33f);
            ApplyFilter();
        }
        private void Vesto_Click(object sender, RoutedEventArgs e)
        {
            cm = SetColorMatrixValue(.12f, -.07f, -.19f, .15f);
            ApplyFilter();
        }
        private void Snusoed_Click(object sender, RoutedEventArgs e)
        {
            cm = SetColorMatrixValue(-1.52f, 1.07f, 1.52f, -1.15f);
            ApplyFilter();
        }
        private void NewYork_Click(object sender, RoutedEventArgs e)
        {
            cm = SetColorMatrixValue(0, 0, .12f, -.07f);
            ApplyFilter();
        }
        private void Juma_Click(object sender, RoutedEventArgs e)
        {
            cm = SetColorMatrixValue(-.37f, .18f, .19f, .11f);
            ApplyFilter();
        }
        private void Valencia_Click(object sender, RoutedEventArgs e)
        {
            cm = SetColorMatrixValue(.15f, .29f, 0, -.2f);
            ApplyFilter();
        }
        private void Demption_Click(object sender, RoutedEventArgs e)
        {
            cm = SetColorMatrixValue(1.2f, -.2f, -.2f, -1.9f);
            ApplyFilter();
        }
        private void Vesper_Click(object sender, RoutedEventArgs e)
        {
            cm = SetColorMatrixValue(.01f, .08f, -.03f, .06f);
            ApplyFilter();
        }

    }
}

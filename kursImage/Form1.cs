using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kursImage
{
    public partial class Form1 : Form
    {
        private Image imgImage;//храним наше изображение      

        public Form1()
        {
            InitializeComponent();
        }


        //Открываем изображение и загружаем его в переменную imgImage
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Файлы Изображений (*.jpg; *.bmp) | *.jpg; *.bmp";
                openFileDialog.ShowDialog();

                imgImage = Image.FromFile(openFileDialog.FileName);
                //copyImg = Image.FromFile(openFileDialog.FileName);
                pictureBox1.Image = imgImage;//загружаем наше изображение в пик
                label1.Text = openFileDialog.FileName;
                //  var a = new Bitmap(imgImage);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Произошла ошибка, либо не выбрано изображение");
            }

        }

        //По смещению трекбара1 проивзодим размытие на полученное значение
        // во всех случаях работа ведется с изображением находящимся в picturebox
        private async void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            trackBar1.Enabled = false;// если происходит работа, то кнопки не доступна, чтоб несколько раз не вызвать одно и то же
            try
            {
                var blur = new GaussianBlur(pictureBox1.Image as Bitmap);//чтоб не делать постоянно сброс
                var step = trackBar1.Value;
                Bitmap result = null;
                var result2 = await Task<Bitmap>.Factory.StartNew(() =>
                {
                    return result = blur.Process(step); ;
                });
                pictureBox1.Image = result2;
            }
            catch (Exception exception)
            {
                MessageBox.Show("Произошла ошибка, либо не выбрано изображение");
                trackBar1.Enabled = true;
            }
            trackBar1.Enabled = true;
        }

        //Фильтр Лапласса для увеличения резкости изображения
        private async void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            try
            {
                var lapl = new Laplass();
                Bitmap result = null;
                var result2 = await Task<Bitmap>.Factory.StartNew(() =>
                {
                    return result = lapl.Laplas(pictureBox1.Image as Bitmap); ;
                });
                pictureBox1.Image = result2;
            }
            catch (Exception exception)
            {
                MessageBox.Show("Произошла ошибка, либо не выбрано изображение");
                button2.Enabled = true;
            }
            button2.Enabled = true;
        }


        #region drag-and-drop
        //Подключаем отображение значка перетягивания под мышкой
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
        //Во время перетягивания ищем позицию picturebox1 и если отпустили перетягиваемый файл над ним,
        // то открываем его.
        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            int x = this.PointToClient(new Point(e.X, e.Y)).X;

            int y = this.PointToClient(new Point(e.X, e.Y)).Y;

            if (x >= pictureBox1.Location.X && x <= pictureBox1.Location.X + pictureBox1.Width && y >= pictureBox1.Location.Y && y <= pictureBox1.Location.Y + pictureBox1.Height)
            {

                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                try
                {
                    button1.Enabled = false;
                    imgImage = Image.FromFile(files[0]);
                    pictureBox1.Image = imgImage;
                    label1.Text = files[0];
                    Bitmap a = new Bitmap(imgImage);
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Произошла ошибка, либо не выбрано изображение");
                    button1.Enabled = true;
                }
                button1.Enabled = true;
            }
        }
        #endregion
        //Медианная фильтрация для удаления шума, берет значение с трекбара
        // значения в очень маленьком диапазоне т.к. указываются значения
        // соседних пикселей для которых происходит обработка
        private async void trackBar2_MouseUp(object sender, MouseEventArgs e)
        {
            trackBar2.Enabled = false;
            try
            {
                Bitmap result = null;
                var val = trackBar2.Value;
                var result2 = await Task<Bitmap>.Factory.StartNew(() =>
                {
                    return result = MedianFilter(pictureBox1.Image as Bitmap, val);
                });
                pictureBox1.Image = result2;
            }
            catch (Exception exception)
            {
                MessageBox.Show("Произошла ошибка, либо не выбрано изображение");
                trackBar2.Enabled = true;
            }
            trackBar2.Enabled = true;
        }


        //Загружаем исходное изображение в picturebox1
        private void button4_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = Image.FromFile(label1.Text);
        }



        #region работа с изображением
        public static Bitmap MedianFilter(Bitmap Image, int Size)
        {
            Bitmap TempBitmap = null;
            Bitmap NewBitmap = null;
            try { 
            TempBitmap = Image;
            NewBitmap = new Bitmap(TempBitmap.Width, TempBitmap.Height);
            /*   Graphics NewGraphics = Graphics.FromImage(NewBitmap);
             NewGraphics.DrawImage(TempBitmap, new Rectangle(0, 0, TempBitmap.Width, TempBitmap.Height),
                  new Rectangle(0, 0, TempBitmap.Width, TempBitmap.Height), System.Drawing.GraphicsUnit.Pixel);
              NewGraphics.Dispose();*/
            Random TempRandom = new Random();
            int ApetureMin = -(Size / 2);
            int ApetureMax = (Size / 2);
            for (int x = 0; x < NewBitmap.Width; ++x)
            {
                for (int y = 0; y < NewBitmap.Height; ++y)
                {
                    List<int> RValues = new List<int>();
                    List<int> GValues = new List<int>();
                    List<int> BValues = new List<int>();
                    for (int x2 = ApetureMin; x2 < ApetureMax; ++x2)
                    {
                        int TempX = x + x2;
                        if (TempX >= 0 && TempX < NewBitmap.Width)
                        {
                            for (int y2 = ApetureMin; y2 < ApetureMax; ++y2)
                            {
                                int TempY = y + y2;
                                if (TempY >= 0 && TempY < NewBitmap.Height)
                                {
                                    Color TempColor = TempBitmap.GetPixel(TempX, TempY);
                                    RValues.Add(TempColor.R);
                                    GValues.Add(TempColor.G);
                                    BValues.Add(TempColor.B);
                                }
                            }
                        }
                    }
                    RValues.Sort();
                    GValues.Sort();
                    BValues.Sort();
                    Color MedianPixel = Color.FromArgb(RValues[RValues.Count / 2],
                       GValues[GValues.Count / 2],
                        BValues[BValues.Count / 2]);
                    NewBitmap.SetPixel(x, y, MedianPixel);
                }
            }
                }catch(Exception ex)
            {
                MessageBox.Show("Выберите изображение!");
            }
            return NewBitmap;
        }

        public static Bitmap GenerateNoise(Bitmap bitmap, int percent)
        {
            Bitmap finalBmp = null;
            try
            {
                finalBmp = (Bitmap)bitmap.Clone();
                Random r = new Random();

                for (int x = 0; x < finalBmp.Width; x++)
                {
                    for (int y = 0; y < finalBmp.Height; y++)
                    {
                        var state = r.Next(0, 100);
                        if (state < percent)
                        {
                            int num = r.Next(0, 256);
                            finalBmp.SetPixel(x, y, Color.FromArgb(255, num, num, num));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Выберите изображение!");
            }

            return finalBmp;
        }

        #endregion

        private async void trackBar3_MouseUp(object sender, MouseEventArgs e)
        {
            trackBar3.Enabled = false;
            try
            {
                // var copy = pictureBox1.Image;
                var step = trackBar3.Value;
                var result2 = await Task<Bitmap>.Factory.StartNew(() =>
                {
                    return GenerateNoise(pictureBox1.Image as Bitmap, step);
                });
                pictureBox1.Image = result2;
            }
            catch (Exception exception)
            {
                MessageBox.Show("Произошла ошибка, либо не выбрано изображение");
                trackBar3.Enabled = true;
            }

            trackBar3.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = "bmp";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                //т.к. наше готовое изображение это то что хранится в 
                //пик боксе, то его и сохраняем


                //оно будет думать что Image - это битмап, потом у него мы и вызываем метод сейв.
                //у тебя там вроде bmp, так что если что знаешь как поменять, или еще кнопку добавишь, ток там укажи другой формат.
                (pictureBox1.Image as Bitmap).Save(dialog.FileName, ImageFormat.Bmp);
            }
        }

    }
}

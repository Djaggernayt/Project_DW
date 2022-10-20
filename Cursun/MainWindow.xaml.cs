using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using Color = System.Drawing.Color;

namespace Cursun
{
    /// <summary>
    /// Форма предназначена для закодирования в изображение символов
    /// </summary>
    public partial class MainWindow : Window
    {
        const int ENCRYP_PESENT_SIZE = 1;
        const int ENCRYP_TEXT_SIZE = 3;
        const int ENCRYP_TEXT_MAX_SIZE = 1999;

        private BitArray ByteToBit(byte src)//перевод из byte in Bit
        {
            BitArray bitArray = new BitArray(8);
            bool st = false;
            for (int i = 0; i < 8; i++)
            {
                if ((src >> i & 1) == 1)
                {
                    st = true;
                }
                else st = false;
                bitArray[i] = st;
            }
            return bitArray;
        }

        private byte BitToByte(BitArray scr) // перевод из bit in byte
        {
            byte num = 0;
            for (int i = 0; i < scr.Count; i++)
                if (scr[i] == true)
                    num += (byte)Math.Pow(2, i);
            return num;
        }

        /*Проверяет, зашифрован ли файл,  возвраещает true, если символ в первом пикслеле равен / иначе false */
        private bool isEncryption(Bitmap scr)
        {
            byte[] rez = new byte[1];
            System.Drawing.Color color = scr.GetPixel(0, 0);
            BitArray colorArray = ByteToBit(color.R); //получаем байт цвета и преобразуем в массив бит
            BitArray messageArray = ByteToBit(color.R); ;//инициализируем результирующий массив бит
            messageArray[0] = colorArray[0];
            messageArray[1] = colorArray[1];

            colorArray = ByteToBit(color.G);//получаем байт цвета и преобразуем в массив бит
            messageArray[2] = colorArray[0];
            messageArray[3] = colorArray[1];
            messageArray[4] = colorArray[2];

            colorArray = ByteToBit(color.B);//получаем байт цвета и преобразуем в массив бит
            messageArray[5] = colorArray[0];
            messageArray[6] = colorArray[1];
            messageArray[7] = colorArray[2];
            rez[0] = BitToByte(messageArray); //получаем байт символа, записанного в 1 пикселе
            string m = Encoding.GetEncoding(1251).GetString(rez);
            if (m == "/")
            {
                return true;
            }
            else return false;
        }

        /*Нормализует количество символов для шифрования,чтобы они всегда занимали ENCRYP_TEXT_SIZE байт*/
        private byte[] NormalizeWriteCount(byte[] CountSymbols)
        {
            int PaddingByte = ENCRYP_TEXT_SIZE - CountSymbols.Length;

            byte[] WriteCount = new byte[ENCRYP_TEXT_SIZE];

            for (int j = 0; j < PaddingByte; j++)
            {
                WriteCount[j] = 0x30;
            }

            for (int j = PaddingByte; j < ENCRYP_TEXT_SIZE; j++)
            {
                WriteCount[j] = CountSymbols[j - PaddingByte];
            }
            return WriteCount;
        }

        /*Записыает количество символов для шифрования в первые биты картинки */
        private void WriteCountText(int count, Bitmap src)
        {
            byte[] CountSymbols = Encoding.GetEncoding(1251).GetBytes(count.ToString());

            if (CountSymbols.Length < ENCRYP_TEXT_SIZE)
            {
                CountSymbols = NormalizeWriteCount(CountSymbols);
            }

            for (int i = 0; i < ENCRYP_TEXT_SIZE; i++)
            {
                BitArray bitCount = ByteToBit(CountSymbols[i]); //биты количества символов
                Color pColor = src.GetPixel(0, i + 1);
                BitArray bitsCurColor = ByteToBit(pColor.R); //бит цветов текущего пикселя
                bitsCurColor[0] = bitCount[0];
                bitsCurColor[1] = bitCount[1];
                byte nR = BitToByte(bitsCurColor); //новый бит цвета пиксея

                bitsCurColor = ByteToBit(pColor.G);//бит бит цветов текущего пикселя
                bitsCurColor[0] = bitCount[2];
                bitsCurColor[1] = bitCount[3];
                bitsCurColor[2] = bitCount[4];
                byte nG = BitToByte(bitsCurColor);//новый цвет пиксея

                bitsCurColor = ByteToBit(pColor.B);//бит бит цветов текущего пикселя
                bitsCurColor[0] = bitCount[5];
                bitsCurColor[1] = bitCount[6];
                bitsCurColor[2] = bitCount[7];
                byte nB = BitToByte(bitsCurColor);//новый цвет пиксея

                Color nColor = Color.FromArgb(nR, nG, nB); //новый цвет из полученных битов
                src.SetPixel(0, i + 1, nColor); //записали полученный цвет в картинку
            }
        }

        /*Читает количество символов для дешифрования из первых бит картинки*/
        private int ReadCountText(Bitmap src)
        {
            byte[] rez = new byte[ENCRYP_TEXT_SIZE];
            for (int i = 0; i < ENCRYP_TEXT_SIZE; i++)
            {
                Color color = src.GetPixel(0, i + 1);
                BitArray colorArray = ByteToBit(color.R); //биты цвета
                BitArray bitCount = ByteToBit(color.R); ; //инициализация результирующего массива бит
                bitCount[0] = colorArray[0];
                bitCount[1] = colorArray[1];

                colorArray = ByteToBit(color.G);
                bitCount[2] = colorArray[0];
                bitCount[3] = colorArray[1];
                bitCount[4] = colorArray[2];

                colorArray = ByteToBit(color.B);
                bitCount[5] = colorArray[0];
                bitCount[6] = colorArray[1];
                bitCount[7] = colorArray[2];
                rez[i] = BitToByte(bitCount);
            }
            string m = Encoding.GetEncoding(1251).GetString(rez);
            return Convert.ToInt32(m, 10);
        }

        public MainWindow()
        {
            InitializeComponent();


        }

        private void write_Click(object sender, RoutedEventArgs e)
        {
            string FilePic;// загрузка файлов img and txt
            string FileText;
            OpenFileDialog dPic = new OpenFileDialog();
            dPic.Filter = "Файлы изображения|*.bmp;*.jpg;*.png|Все файлы|*.*";
            if (dPic.ShowDialog() == true)
            {
                FilePic = dPic.FileName;
            }
            else
            {
                FilePic = "";
                return;
            }

            FileStream rFile;
            Bitmap bPic;
            try
            {
                rFile = new FileStream(FilePic, FileMode.Open); //открываем поток
                bPic = new Bitmap(rFile);
            }
            catch (Exception)
            {
                
                MessageBox.Show("Ошибка открытия файла", "Ошибка");
                return;
            }

            

            OpenFileDialog dText = new OpenFileDialog();
            dText.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (dText.ShowDialog() == true)
            {
                FileText = dText.FileName;
            }
            else
            {
                FileText = "";
                return;
            }
            try
            {
                tb.Text = File.ReadAllText(FileText, Encoding.Default);
            }
            catch (Exception)
            {
                rFile.Close();
                MessageBox.Show("Ошибка открытия файла", "Ошибка");
                return;
            }

            FileStream rText;
            BinaryReader bText;
            try
            {
                rText = new FileStream(FileText, FileMode.Open); //открываем поток
                bText = new BinaryReader(rText, Encoding.ASCII);
            }
            catch (Exception)
            {
                rFile.Close();
                MessageBox.Show("Ошибка открытия файла", "Ошибка");
                return;
            }
            

            

            List<byte> bList = new List<byte>();
            while (bText.PeekChar() != -1)
            { //считали весь текстовый файл для шифрования в лист байт
                bList.Add(bText.ReadByte());
            }
            int CountText = bList.Count; // в CountText - количество в байтах текста, который нужно закодировать
            bText.Close();
            rFile.Close();

            //проверям, что размер не выходит за рамки максимального, поскольку для хранения размера используется
            //ограниченное количество байт
            if (CountText > (ENCRYP_TEXT_MAX_SIZE - ENCRYP_PESENT_SIZE - ENCRYP_TEXT_SIZE))
            {
                MessageBox.Show("Размер текста велик для данного алгоритма, уменьшите размер", "Информация");
                return;
            }

            //проверяем, поместится ли исходный текст в картинке
            if (CountText > (bPic.Width * bPic.Height))
            {
                MessageBox.Show("Выбранная картинка мала для размещения выбранного текста", "Информация");
                return;
            }

            //проверяем, может быть картинка уже зашифрована
            if (isEncryption(bPic))
            {
                MessageBox.Show("Файл уже зашифрован", "Информация");
                return;
            }

            byte[] Symbol = Encoding.GetEncoding(1251).GetBytes("/");
            BitArray ArrBeginSymbol = ByteToBit(Symbol[0]);
            Color curColor = bPic.GetPixel(0, 0);
            BitArray tempArray = ByteToBit(curColor.R);
            tempArray[0] = ArrBeginSymbol[0];
            tempArray[1] = ArrBeginSymbol[1];
            byte nR = BitToByte(tempArray);

            tempArray = ByteToBit(curColor.G);
            tempArray[0] = ArrBeginSymbol[2];
            tempArray[1] = ArrBeginSymbol[3];
            tempArray[2] = ArrBeginSymbol[4];
            byte nG = BitToByte(tempArray);

            tempArray = ByteToBit(curColor.B);
            tempArray[0] = ArrBeginSymbol[5];
            tempArray[1] = ArrBeginSymbol[6];
            tempArray[2] = ArrBeginSymbol[7];
            byte nB = BitToByte(tempArray);

            Color nColor = Color.FromArgb(nR, nG, nB);
            bPic.SetPixel(0, 0, nColor);
            //то есть в первом пикселе будет символ /, который говорит о том, что картика зашифрована

            WriteCountText(CountText, bPic); //записываем количество символов для шифрования

            int index = 0;
            bool st = false;
            for (int i = ENCRYP_TEXT_SIZE + 1; i < bPic.Width; i++)
            {
                for (int j = 0; j < bPic.Height; j++)
                {
                    Color pixelColor = bPic.GetPixel(i, j);
                    if (index == bList.Count)
                    {
                        st = true;
                        break;
                    }
                    BitArray colorArray = ByteToBit(pixelColor.R);
                    BitArray messageArray = ByteToBit(bList[index]);
                    colorArray[0] = messageArray[0]; //меняем
                    colorArray[1] = messageArray[1]; // в нашем цвете биты
                    byte newR = BitToByte(colorArray);

                    colorArray = ByteToBit(pixelColor.G);
                    colorArray[0] = messageArray[2];
                    colorArray[1] = messageArray[3];
                    colorArray[2] = messageArray[4];
                    byte newG = BitToByte(colorArray);

                    colorArray = ByteToBit(pixelColor.B);
                    colorArray[0] = messageArray[5];
                    colorArray[1] = messageArray[6];
                    colorArray[2] = messageArray[7];
                    byte newB = BitToByte(colorArray);

                    Color newColor = Color.FromArgb(newR, newG, newB);
                    bPic.SetPixel(i, j, newColor);
                    index++;
                }
                if (st)
                {
                    break;
                }
            }
            MemoryStream ms = new MemoryStream();// выгрузка bmp в image
            ((System.Drawing.Bitmap)bPic).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            imagebox.Source = image;

            String sFilePic;// сохранение файла
            SaveFileDialog dSavePic = new SaveFileDialog();
            dSavePic.Filter = "Файлы изображений (*.bmp)|*.bmp;*.jpg;*.png|Все файлы (*.*)|*.*";
            if (dSavePic.ShowDialog() == true)
            {
                sFilePic = dSavePic.FileName;
            }
            else
            {
                sFilePic = "";
                return;
            };

            FileStream wFile;
            try
            {
                wFile = new FileStream(sFilePic, FileMode.Create); //открываем поток на запись результатов
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка открытия файла на запись", "Ошибка");
                return;
            }

            bPic.Save(wFile, System.Drawing.Imaging.ImageFormat.Bmp);
            wFile.Close(); //закрываем поток
        }

        private void read_Click(object sender, RoutedEventArgs e)
        {
            string FilePic;// Загрузка файла img
            OpenFileDialog dPic = new OpenFileDialog();
            dPic.Filter = "Файлы изображений (*.bmp)|*.bmp;*.jpg;*.png|Все файлы (*.*)|*.*";
            if (dPic.ShowDialog() == true)
            {
                FilePic = dPic.FileName;
            }
            else
            {
                FilePic = "";
                return;
            }

            FileStream rFile;
            Bitmap bPic;
            try
            {
                rFile = new FileStream(FilePic, FileMode.Open);//открываем поток
                bPic = new Bitmap(rFile);
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка открытия файла", "Ошибка");
                return;
            }
            
            if (!isEncryption(bPic))
            {
                MessageBox.Show("В файле нет зашифрованной информации", "Информация");
                rFile.Close();
                return;
            }

            int countSymbol = ReadCountText(bPic); //считали количество зашифрованных символов
            byte[] message = new byte[countSymbol];
            int index = 0;
            bool st = false;
            for (int i = ENCRYP_TEXT_SIZE + 1; i < bPic.Width; i++)
            {
                for (int j = 0; j < bPic.Height; j++)
                {
                    Color pixelColor = bPic.GetPixel(i, j);
                    if (index == message.Length)
                    {
                        st = true;
                        break;
                    }
                    BitArray colorArray = ByteToBit(pixelColor.R);
                    BitArray messageArray = ByteToBit(pixelColor.R); ;
                    messageArray[0] = colorArray[0];
                    messageArray[1] = colorArray[1];

                    colorArray = ByteToBit(pixelColor.G);
                    messageArray[2] = colorArray[0];
                    messageArray[3] = colorArray[1];
                    messageArray[4] = colorArray[2];

                    colorArray = ByteToBit(pixelColor.B);
                    messageArray[5] = colorArray[0];
                    messageArray[6] = colorArray[1];
                    messageArray[7] = colorArray[2];
                    message[index] = BitToByte(messageArray);
                    index++;
                }
                if (st)
                {
                    break;
                }
            }
            string strMessage = Encoding.GetEncoding(1251).GetString(message);//перевод изображение в byte

            string sFileText;
            SaveFileDialog dSaveText = new SaveFileDialog();
            dSaveText.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (dSaveText.ShowDialog() == true)
            {
                sFileText = dSaveText.FileName;
            }
            else
            {
                sFileText = "";
                rFile.Close();
                return;
            };


            FileStream wFile;
            try
            {
                wFile = new FileStream(sFileText, FileMode.Create); //открываем поток на запись результатов
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка открытия файла на запись", "Ошибка");
                rFile.Close();
                return;
            }
            MemoryStream ms = new MemoryStream();// выгрузка bmp в image
            ((System.Drawing.Bitmap)bPic).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            imagebox.Source = image;

            StreamWriter wText = new StreamWriter(wFile, Encoding.Default);
            wText.Write(strMessage);
            
            wText.Close();
            wFile.Close(); //закрываем поток
            rFile.Close();
            try
            {
                tb.Text = File.ReadAllText(sFileText, Encoding.Default);

            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка открытия файла", "Ошибка");
                return;
            }
            MessageBox.Show("Текст записан в файл", "Информация");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void mess_Click(object sender, RoutedEventArgs e)
        {
            About a = new About();
            a.Show();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SignLanguageRecognitionWpfDemo
{
    class utils
    {
        public void ShowErrorMsg(string msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public string GenLogString(string info)
        {
            DateTime dt = new DateTime();
            dt = System.DateTime.Now;
            string strFu = dt.ToString("yyyy-MM-dd HH:mm:ss");
            return string.Format("{0}: {1}\r\n", strFu, info);
        }

        public List<float> SkeletonDataAbs2Rel(List<float> dataList, int cropX, int cropY, float marginX=0.0f, float marginY=0.0f)
        {
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            for (int i = 0; i < dataList.Count; i++)
            {
                if (i % 2 == 0)
                {
                    minX = Math.Min(minX, dataList[i]);
                    maxX = Math.Max(maxX, dataList[i]);
                }
                else
                {
                    minY = Math.Min(minY, dataList[i]);
                    maxY = Math.Max(maxY, dataList[i]);
                }
            }

            minX -= marginX;
            maxX += marginX;
            minY -= marginY;
            maxY += marginY;

            for (int i = 0;i < dataList.Count; i++)
            {
                //X
                if(i % 2 == 0)
                {
                    dataList[i] = (dataList[i] - minX) / (maxX - minX) * cropX;
                }
                //Y
                else
                {
                    dataList[i] = (dataList[i] - minY) / (maxY - minY) * cropY;
                }
            }
            return dataList;
        }

        private void UpdateTextbox(TextBox tb, string text)
        {
            tb.AppendText(text);
        }

        public void AddLogInfo(TextBox tb, string logMsg)
        {
            Action<TextBox, string> updateAction = new Action<TextBox, string>(UpdateTextbox);
            tb.Dispatcher.BeginInvoke(updateAction, tb, logMsg);
        }

        //private void _UpdataResultLabel(Label label, string str)
        //{
        //    label.Content = str;
        //}

        //public void UpdataResultLabel(SynchronizationContext synchronization, Label label, string strResult)
        //{
        //    synchronization.Post(_UpdataResultLabel,)
        //    label.Dispatcher.Invoke(updateAction, strResult);
        //}

        public BitmapSource ConvertVisual2BitmapSource(DrawingVisual visual, double Width, double Height, float dpi)
        {
            BitmapImage bitmapImage = new BitmapImage();
            if (Math.Abs(Width) > 0.001 && Math.Abs(Height) > 0.001)
            {
                int dpWidth = (int)(Width * dpi / 96.0);
                int dpHeight = (int)(Height * dpi / 96.0);
                RenderTargetBitmap bitmap = new RenderTargetBitmap(dpWidth, dpHeight, dpi, dpi, PixelFormats.Pbgra32);
                bitmap.Render((Visual)visual);

                using (MemoryStream stream = new MemoryStream())
                {
                    BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(stream);
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    bitmapImage.StreamSource = new MemoryStream(stream.ToArray()); //stream;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                }
            }
            return (BitmapSource)bitmapImage;
        }

        public static List<KeyValuePair<float, bool>> TestFramesDiffList = new List<KeyValuePair<float, bool>>()
        {
            new KeyValuePair<float, bool>(3.98f,true),
            new KeyValuePair<float, bool>(4.01f,true),
            new KeyValuePair<float, bool>(2.8f,true),
            new KeyValuePair<float, bool>(4.5f,true),
            new KeyValuePair<float, bool>(3.05f,true),
            new KeyValuePair<float, bool>(3.7f,true),
            new KeyValuePair<float, bool>(2.51f,true),
            new KeyValuePair<float, bool>(2.28f,true),
            new KeyValuePair<float, bool>(3.37f,true),
            new KeyValuePair<float, bool>(0.93f,false),
            new KeyValuePair<float, bool>(4.41f,true),
            new KeyValuePair<float, bool>(3.04f,true),
            new KeyValuePair<float, bool>(2.25f,true),
            new KeyValuePair<float, bool>(4.16f,true),
            new KeyValuePair<float, bool>(1.5f,true),
            new KeyValuePair<float, bool>(0.24f,false),
            new KeyValuePair<float, bool>(1.57f,true),
            new KeyValuePair<float, bool>(3.28f,true),
            new KeyValuePair<float, bool>(4.81f,true),
            new KeyValuePair<float, bool>(4.45f,true),
            new KeyValuePair<float, bool>(2.13f,true),
            new KeyValuePair<float, bool>(4.68f,true),
            new KeyValuePair<float, bool>(0.13f,false),
            new KeyValuePair<float, bool>(2.28f,true),
            new KeyValuePair<float, bool>(2.5f,true),
            new KeyValuePair<float, bool>(4.81f,true),
            new KeyValuePair<float, bool>(1.33f,true),
            new KeyValuePair<float, bool>(0.76f,false),
            new KeyValuePair<float, bool>(2.57f,true),
            new KeyValuePair<float, bool>(1.06f,true),
            new KeyValuePair<float, bool>(0.41f,false),
            new KeyValuePair<float, bool>(2.88f,true),
            new KeyValuePair<float, bool>(4.68f,true),
            new KeyValuePair<float, bool>(1.58f,true),
            new KeyValuePair<float, bool>(0.87f,false),
            new KeyValuePair<float, bool>(0.78f,false),
            new KeyValuePair<float, bool>(3.42f,true),
            new KeyValuePair<float, bool>(3.63f,true),
            new KeyValuePair<float, bool>(1.43f,true),
            new KeyValuePair<float, bool>(2.21f,true),
            new KeyValuePair<float, bool>(2.86f,true),
            new KeyValuePair<float, bool>(3.82f,true),
            new KeyValuePair<float, bool>(1.27f,true),
            new KeyValuePair<float, bool>(0.04f,false),
            new KeyValuePair<float, bool>(0.77f,false),
            new KeyValuePair<float, bool>(3.63f,true),
            new KeyValuePair<float, bool>(2.98f,true),
            new KeyValuePair<float, bool>(1.44f,true),
            new KeyValuePair<float, bool>(3.3f,true),
            new KeyValuePair<float, bool>(1.25f,true),
            new KeyValuePair<float, bool>(0.56f,false),
            new KeyValuePair<float, bool>(3.58f,true),
            new KeyValuePair<float, bool>(4.09f,true),
            new KeyValuePair<float, bool>(3.66f,true),
            new KeyValuePair<float, bool>(2.63f,true),
            new KeyValuePair<float, bool>(3.42f,true),
            new KeyValuePair<float, bool>(1.98f,true),
            new KeyValuePair<float, bool>(2.26f,true),
            new KeyValuePair<float, bool>(0.58f,false),
            new KeyValuePair<float, bool>(3.46f,true),
            new KeyValuePair<float, bool>(2.2f,true),
            new KeyValuePair<float, bool>(0.74f,false),
            new KeyValuePair<float, bool>(0.45f,false),
            new KeyValuePair<float, bool>(3.67f,true),
            new KeyValuePair<float, bool>(2.09f,true),
            new KeyValuePair<float, bool>(0.48f,false),
            new KeyValuePair<float, bool>(2.8f,true),
            new KeyValuePair<float, bool>(2.93f,true),
            new KeyValuePair<float, bool>(2.37f,true),
            new KeyValuePair<float, bool>(2.16f,true),
            new KeyValuePair<float, bool>(4.55f,true),
            new KeyValuePair<float, bool>(4.13f,true),
            new KeyValuePair<float, bool>(4.23f,true),
            new KeyValuePair<float, bool>(2.39f,true),
            new KeyValuePair<float, bool>(3.93f,true),
            new KeyValuePair<float, bool>(2.51f,true),
            new KeyValuePair<float, bool>(0.13f,false),
            new KeyValuePair<float, bool>(1.73f,true),
            new KeyValuePair<float, bool>(1.73f,true),
            new KeyValuePair<float, bool>(2.38f,true),
            new KeyValuePair<float, bool>(0.7f,false),
            new KeyValuePair<float, bool>(2.7f,true),
            new KeyValuePair<float, bool>(0.69f,false),
            new KeyValuePair<float, bool>(4.87f,true),
            new KeyValuePair<float, bool>(1.26f,true),
            new KeyValuePair<float, bool>(0.44f,false),
            new KeyValuePair<float, bool>(0.96f,false),
            new KeyValuePair<float, bool>(0.33f,false),
            new KeyValuePair<float, bool>(1.27f,true),
            new KeyValuePair<float, bool>(4.11f,true),
            new KeyValuePair<float, bool>(1.64f,true),
            new KeyValuePair<float, bool>(0.86f,false),
            new KeyValuePair<float, bool>(4.84f,true),
            new KeyValuePair<float, bool>(1.43f,true),
            new KeyValuePair<float, bool>(3.25f,true),
            new KeyValuePair<float, bool>(3.58f,true),
            new KeyValuePair<float, bool>(1.63f,true),
            new KeyValuePair<float, bool>(1.93f,true),
            new KeyValuePair<float, bool>(1.09f,true),
            new KeyValuePair<float, bool>(0.96f,false),
            new KeyValuePair<float, bool>(2.53f,true),
            new KeyValuePair<float, bool>(3.14f,true),
            new KeyValuePair<float, bool>(4.4f,true),
            new KeyValuePair<float, bool>(3.71f,true),
            new KeyValuePair<float, bool>(1.52f,true),
            new KeyValuePair<float, bool>(4.77f,true),
            new KeyValuePair<float, bool>(3.82f,true),
            new KeyValuePair<float, bool>(3.09f,true),
            new KeyValuePair<float, bool>(0.75f,false),
            new KeyValuePair<float, bool>(2.94f,true),
            new KeyValuePair<float, bool>(2.79f,true),
            new KeyValuePair<float, bool>(4.95f,true),
            new KeyValuePair<float, bool>(4.81f,true),
            new KeyValuePair<float, bool>(3.35f,true),
            new KeyValuePair<float, bool>(2.83f,true),
            new KeyValuePair<float, bool>(1.23f,true),
            new KeyValuePair<float, bool>(2.39f,true),
            new KeyValuePair<float, bool>(3.57f,true),
            new KeyValuePair<float, bool>(2.04f,true),
            new KeyValuePair<float, bool>(4.9f,true),
        };

        public static List<float> TestKeyFrameSkeletonData = new List<float>()
        {
            197.0f, 228.0f, 179.0f, 283.0f, 177.0f, 334.0f, 183.0f, 350.0f, 283.0f, 224.0f, 299.0f, 275.0f, 295.0f, 323.0f, 285.0f, 342.0f, 197.0f, 228.0f, 177.0f, 279.0f, 177.0f, 322.0f, 182.0f, 338.0f, 283.0f, 224.0f, 301.0f, 274.0f, 296.0f, 313.0f, 284.0f, 329.0f, 197.0f, 228.0f, 175.0f, 277.0f, 174.0f, 307.0f, 181.0f, 316.0f, 283.0f, 224.0f, 305.0f, 273.0f, 285.0f, 309.0f, 281.0f, 311.0f, 197.0f, 228.0f, 162.0f, 271.0f, 180.0f, 298.0f, 186.0f, 301.0f, 282.0f, 223.0f, 308.0f, 271.0f, 287.0f, 289.0f, 276.0f, 294.0f, 197.0f, 227.0f, 160.0f, 268.0f, 184.0f, 290.0f, 191.0f, 286.0f, 282.0f, 223.0f, 311.0f, 269.0f, 281.0f, 277.0f, 271.0f, 277.0f, 198.0f, 227.0f, 167.0f, 270.0f, 187.0f, 270.0f, 197.0f, 271.0f, 282.0f, 223.0f, 312.0f, 266.0f, 276.0f, 266.0f, 267.0f, 263.0f, 198.0f, 227.0f, 168.0f, 270.0f, 188.0f, 264.0f, 201.0f, 259.0f, 282.0f, 222.0f, 308.0f, 264.0f, 270.0f, 248.0f, 262.0f, 252.0f, 198.0f, 226.0f, 168.0f, 267.0f, 236.0f, 250.0f, 255.0f, 244.0f, 282.0f, 222.0f, 307.0f, 262.0f, 256.0f, 246.0f, 247.0f, 248.0f, 198.0f, 226.0f, 170.0f, 265.0f, 239.0f, 247.0f, 260.0f, 238.0f, 282.0f, 221.0f, 306.0f, 261.0f, 237.0f, 244.0f, 213.0f, 240.0f, 197.0f, 225.0f, 170.0f, 263.0f, 238.0f, 233.0f, 234.0f, 232.0f, 282.0f, 220.0f, 297.0f, 251.0f, 237.0f, 241.0f, 214.0f, 236.0f, 197.0f, 225.0f, 170.0f, 262.0f, 239.0f, 233.0f, 260.0f, 228.0f, 282.0f, 220.0f, 297.0f, 252.0f, 237.0f, 239.0f, 213.0f, 231.0f, 199.0f, 223.0f, 169.0f, 259.0f, 243.0f, 237.0f, 246.0f, 229.0f, 282.0f, 218.0f, 298.0f, 251.0f, 235.0f, 233.0f, 220.0f, 226.0f, 199.0f, 223.0f, 169.0f, 260.0f, 239.0f, 232.0f, 260.0f, 228.0f, 282.0f, 218.0f, 298.0f, 251.0f, 235.0f, 233.0f, 216.0f, 228.0f, 199.0f, 223.0f, 169.0f, 260.0f, 239.0f, 231.0f, 260.0f, 229.0f, 282.0f, 218.0f, 298.0f, 251.0f, 246.0f, 230.0f, 232.0f, 226.0f, 199.0f, 223.0f, 169.0f, 260.0f, 239.0f, 233.0f, 261.0f, 230.0f, 282.0f, 218.0f, 298.0f, 251.0f, 236.0f, 236.0f, 217.0f, 229.0f, 198.0f, 222.0f, 169.0f, 261.0f, 242.0f, 240.0f, 244.0f, 234.0f, 282.0f, 218.0f, 304.0f, 255.0f, 253.0f, 236.0f, 240.0f, 232.0f, 198.0f, 222.0f, 168.0f, 262.0f, 238.0f, 239.0f, 259.0f, 233.0f, 282.0f, 218.0f, 305.0f, 255.0f, 240.0f, 240.0f, 222.0f, 237.0f, 198.0f, 222.0f, 169.0f, 263.0f, 241.0f, 239.0f, 245.0f, 235.0f, 282.0f, 218.0f, 305.0f, 255.0f, 240.0f, 240.0f, 221.0f, 242.0f, 198.0f, 223.0f, 169.0f, 265.0f, 236.0f, 243.0f, 256.0f, 231.0f, 282.0f, 219.0f, 306.0f, 255.0f, 255.0f, 237.0f, 235.0f, 239.0f, 198.0f, 223.0f, 171.0f, 267.0f, 233.0f, 238.0f, 252.0f, 230.0f, 282.0f, 219.0f, 306.0f, 254.0f, 265.0f, 224.0f, 250.0f, 229.0f, 198.0f, 223.0f, 171.0f, 268.0f, 204.0f, 257.0f, 211.0f, 256.0f, 281.0f, 219.0f, 307.0f, 254.0f, 266.0f, 223.0f, 253.0f, 225.0f, 198.0f, 223.0f, 171.0f, 268.0f, 216.0f, 261.0f, 221.0f, 263.0f, 281.0f, 220.0f, 307.0f, 254.0f, 284.0f, 237.0f, 266.0f, 224.0f, 198.0f, 224.0f, 171.0f, 269.0f, 208.0f, 266.0f, 213.0f, 264.0f, 281.0f, 220.0f, 308.0f, 254.0f, 264.0f, 226.0f, 253.0f, 220.0f, 198.0f, 224.0f, 174.0f, 273.0f, 208.0f, 266.0f, 217.0f, 264.0f, 281.0f, 220.0f, 310.0f, 258.0f, 239.0f, 261.0f, 218.0f, 263.0f, 198.0f, 225.0f, 174.0f, 273.0f, 229.0f, 265.0f, 244.0f, 263.0f, 281.0f, 220.0f, 309.0f, 259.0f, 253.0f, 268.0f, 237.0f, 264.0f, 198.0f, 225.0f, 175.0f, 274.0f, 194.0f, 269.0f, 209.0f, 267.0f, 281.0f, 220.0f, 309.0f, 260.0f, 239.0f, 266.0f, 223.0f, 265.0f, 197.0f, 225.0f, 174.0f, 275.0f, 208.0f, 269.0f, 210.0f, 266.0f, 281.0f, 220.0f, 309.0f, 260.0f, 252.0f, 260.0f, 240.0f, 249.0f, 197.0f, 225.0f, 174.0f, 275.0f, 193.0f, 270.0f, 207.0f, 269.0f, 281.0f, 220.0f, 309.0f, 260.0f, 258.0f, 243.0f, 244.0f, 241.0f, 196.0f, 227.0f, 157.0f, 268.0f, 190.0f, 283.0f, 189.0f, 284.0f, 281.0f, 220.0f, 311.0f, 263.0f, 277.0f, 256.0f, 273.0f, 254.0f, 196.0f, 227.0f, 156.0f, 265.0f, 175.0f, 284.0f, 182.0f, 292.0f, 281.0f, 220.0f, 312.0f, 264.0f, 282.0f, 268.0f, 278.0f, 270.0f, 196.0f, 226.0f, 156.0f, 265.0f, 172.0f, 287.0f, 173.0f, 301.0f, 281.0f, 221.0f, 319.0f, 265.0f, 291.0f, 284.0f, 285.0f, 289.0f, 196.0f, 226.0f, 157.0f, 265.0f, 168.0f, 297.0f, 165.0f, 309.0f, 282.0f, 221.0f, 317.0f, 266.0f, 294.0f, 306.0f, 291.0f, 306.0f, 197.0f, 226.0f, 160.0f, 267.0f, 163.0f, 304.0f, 159.0f, 319.0f, 282.0f, 221.0f, 315.0f, 266.0f, 301.0f, 311.0f, 295.0f, 324.0f, 197.0f, 226.0f, 162.0f, 268.0f, 160.0f, 312.0f, 156.0f, 330.0f, 282.0f, 221.0f, 314.0f, 267.0f, 302.0f, 316.0f, 299.0f, 336.0f, 196.0f, 227.0f, 171.0f, 276.0f, 158.0f, 322.0f, 156.0f, 338.0f, 282.0f, 221.0f, 303.0f, 276.0f, 303.0f, 327.0f, 301.0f, 348.0f, 196.0f, 228.0f, 184.0f, 290.0f, 179.0f, 346.0f, 181.0f, 356.0f, 282.0f, 223.0f, 293.0f, 283.0f, 294.0f, 339.0f, 290.0f, 351.0f
        };
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kursImage
{
    class Laplass
    {
        public Bitmap Laplas(Bitmap src)
        {
            Bitmap dest = null;
            try
            {
                dest = (Bitmap)src.Clone();
                BitmapData srcData = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                BitmapData destData = dest.LockBits(new Rectangle(0, 0, dest.Width, dest.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                IntPtr srcScan0 = srcData.Scan0;
                IntPtr destScan0 = destData.Scan0;
                int stride = destData.Stride;
                int strideX2 = stride * 2;
                int offset = stride - src.Width * 3;
                int startY = 0;
                int startX = 0;
                int endY = src.Height - 1;
                int endX = src.Width - 1;
                double pixel = 0;
                unsafe
                {
                    byte* pSrc = (byte*)(void*)srcScan0;
                    byte* pDest = (byte*)(void*)destScan0;
                    for (int i = startY; i < endY; i++)
                    {
                        for (int j = startX; j < endX; j++)
                        {

                            pixel = 5 * pSrc[3 + stride] - (pSrc[3] + pSrc[0 + stride] + pSrc[6 + stride] + pSrc[3 + strideX2]);

                            if (pixel > 255)
                                pixel = 255;

                            if (pixel < 0)
                                pixel = 0;

                            pDest[3 + stride] = (byte)pixel;


                            pixel = 5 * pSrc[4 + stride] - (pSrc[4] + pSrc[1 + stride] + pSrc[7 + stride] + pSrc[4 + strideX2]);
                            if (pixel > 255)
                                pixel = 255;

                            if (pixel < 0)
                                pixel = 0;

                            pDest[4 + stride] = (byte)pixel;

                            pixel = 5 * pSrc[5 + stride] - (pSrc[5] + pSrc[2 + stride] + pSrc[8 + stride] + pSrc[5 + strideX2]);

                            if (pixel > 255)
                                pixel = 255;
                            if (pixel < 0)
                                pixel = 0;
                            pDest[5 + stride] = (byte)pixel;
                            pSrc += 3;
                            pDest += 3;
                        }
                        pSrc += offset;
                        pDest += offset;
                    }
                }

                src.UnlockBits(srcData);
                dest.UnlockBits(destData);
                GC.Collect();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Выберите сначала изображение!");
            }
            return dest;
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

namespace DataParallelism
{
	public class DirectBitmap : IDisposable
	{
		public unsafe DirectBitmap(Bitmap bmp)
		{
			this.bmp = bmp;
			if(bmp.PixelFormat != PixelFormat.Format32bppArgb)
				throw new Exception($"Invalid image format '{bmp.PixelFormat}'");
			data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
			ptr = (int*)data.Scan0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe Color FastGetPixel(int x, int y)
		{
			if(x >= data.Width || y >= data.Height)
				throw new ArgumentOutOfRangeException();
			return Color.FromArgb(ptr[y * data.Width + x]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void FastSetPixel(int x, int y, Color color)
		{
			if(x >= data.Width || y >= data.Height)
				throw new ArgumentOutOfRangeException();
			ptr[y * data.Width + x] = color.ToArgb();
		}

		public void Dispose()
		{
			bmp.UnlockBits(data);
			bmp.Dispose();
		}

		public int Width => data.Width;
		public int Height => data.Height;

		private readonly Bitmap bmp;
		private readonly BitmapData data;
		private readonly unsafe int* ptr;
	}

	public static class ColorHelper
	{
		public static Color GrayScale(this Color color)
		{
			var gray = (byte)(0.3 * color.R + 0.59 * color.G + 0.11 * color.B);
			return Color.FromArgb(color.A, gray, gray, gray);
		}
	}
}
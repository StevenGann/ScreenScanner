using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;

//General To Do:
// - Add support for multiple monitors.

namespace ScreenScanner
{
	public class Scanner
	{
		//Public Fields
		public int SampleRate = 100;
		public bool Enabled = false;
		public Color Output = Color.FromArgb(0, 0, 0);

		//Private Fields
		private int screenHeight;
		private int screenWidth;
		private FixedSizedQueue<Color> rgbQ = new FixedSizedQueue<Color>(50); //TO DO: Make this adjustable at runtime.
		private Timer timer1 = new Timer();
		private Random rnd = new Random();

		//Constructor
		public Scanner()
		{
			screenHeight = GetScreenHeight();
			screenWidth = GetScreenWidth();

			timer1.Tick += timerTick;
		}

		//========================
		//Public Methods
		//========================
		public void Start()
		{
			timer1.Interval = SampleRate;
			timer1.Enabled = true;
			Enabled = true;
		}

		public void Stop()
		{
			timer1.Enabled = false;
			Enabled = false;
		}
		//========================

		//========================
		//Private Methods
		//========================
		private void timerTick(object sender, EventArgs e)
		{
			//int newX = rnd.Next(screenWidth);//(int)(((float)(rnd.Next(screenWidth)+rnd.Next(screenWidth)))/2.0f);
			//int newY = rnd.Next(screenHeight);//(int)(((float)(rnd.Next(screenHeight)+rnd.Next(screenHeight)))/2.0f);

			int newX = (int)(((float)(rnd.Next(screenWidth)+rnd.Next(screenWidth)))/2.0f);
			int newY = (int)(((float)(rnd.Next(screenHeight) + rnd.Next(screenHeight))) / 2.0f);

			Color newRGB = getRGB(newX, newY);

			rgbQ.Enqueue(newRGB);

			Output = averageQ();
		}

		private Color averageQ()
		{
			Color result;
			int sumR = 0;
			int sumG = 0;
			int sumB = 0;

			foreach (Color element in rgbQ)
			{
				sumR += element.R;
				sumG += element.G;
				sumB += element.B;
			}

			int avgR = (int)((float)sumR / (float)rgbQ.Count);
			int avgG = (int)((float)sumG / (float)rgbQ.Count);
			int avgB = (int)((float)sumB / (float)rgbQ.Count);

			result = Color.FromArgb(avgR, avgG, avgB);

			return result;
		}
		//========================

		//========================
		//=== Static Methods =====
		//========================
		static private Rectangle GetScreen()
		{
			return Screen.PrimaryScreen.Bounds;
		}

		static private int GetScreenHeight()
		{
			return Screen.PrimaryScreen.Bounds.Height;
		}

		static private int GetScreenWidth()
		{
			return Screen.PrimaryScreen.Bounds.Height;
		}

		static private Color getRGB(int x, int y)
		{
			Color color = Win32.GetPixelColor(x, y);
			return color;
		}
		//========================


	}
}

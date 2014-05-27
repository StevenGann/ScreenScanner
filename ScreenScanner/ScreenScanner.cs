﻿using System;
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
		public bool SaturationBoost = false;

		//Private Fields
		private int screenHeight;
		private int screenHeightMid;
		private int screenWidth;
		private int screenWidthMid;
		private FixedSizedQueue<Color> rgbQ;// 
		private Timer timer1 = new Timer();
		private Random rnd = new Random();

		//Constructor
		public Scanner()
		{
			screenHeight = GetScreenHeight();
			screenHeightMid = screenHeight / 2;
			screenWidth = GetScreenWidth();
			screenWidthMid = screenWidth / 2;

			rgbQ = new FixedSizedQueue<Color>(50);

			timer1.Tick += timerTick;
		}

		public Scanner(int sampleSize)
		{
			screenHeight = GetScreenHeight();
			screenWidth = GetScreenWidth();

			rgbQ = new FixedSizedQueue<Color>(sampleSize);

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

		public void SetSampleSize(int sampleSize)
		{
			rgbQ.Size = sampleSize;
		}
		//========================

		//========================
		//Private Methods
		//========================

		//Tick event handler for main timer object.
		//This is where most of the functionality takes place.
		private void timerTick(object sender, EventArgs e)
		{
			//To Do: Add method for switching pixel selection methods.
			int newX = screenWidthMid;
			int newY = screenHeightMid;

			//Linear Distribution
			//Completely random.
			//newX = rnd.Next(screenWidth);
			//newY = rnd.Next(screenHeight);

			//Logarithmic Distribution
			//Random, but more likely to select from closer to the center of the screen.
			newX = (int)(((float)(rnd.Next(screenWidth)+rnd.Next(screenWidth)))/2.0f);
			newY = (int)(((float)(rnd.Next(screenHeight) + rnd.Next(screenHeight))) / 2.0f);

			//Reverse Logarithmic Distribution
			//Random, but more likely to select from closer to the edge of the screen.
			/*
			int lin1 = rnd.Next(screenWidth);
			int lin2 = rnd.Next(screenWidth);
			newX = (int)(((float)(lin1 + lin2)) / 2.0f);
			if (newX > screenWidthMid)
			{
				newX += Math.Min(lin1, lin2);
			}
			else
			{
				newX -= Math.Min(lin1, lin2);
			}

			lin1 = rnd.Next(screenHeight);
			lin2 = rnd.Next(screenHeight);
			newY = (int)(((float)(lin1 + lin2)) / 2.0f);
			if (newY > screenHeightMid)
			{
				newY += Math.Min(lin1, lin2);
			}
			else
			{
				newY -= Math.Min(lin1, lin2);
			}
			*/
			Color newRGB = getRGB(newX, newY);

			rgbQ.Enqueue(newRGB);

			//To Do: Add a method for switching sample processing methods.

			Output = averageQ();

			//Max out saturation
			if (SaturationBoost == true)
			{
				Output = HSBtoRGB(Output.GetHue(), Math.Max((float)Output.GetSaturation(), 0.1f), Output.GetBrightness());
			}
		}

		//Flat average of entire sample set.
		//Returns the averaged Color struct.
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

		//Returns a Rectangle struct representing the dimensions of the main monitor.
		//Not actually used yet, but small enough to keep around anyway.
		static private Rectangle GetScreen()
		{
			return Screen.PrimaryScreen.Bounds;
		}

		//Returns the height of the primary display
		static private int GetScreenHeight()
		{
			return Screen.PrimaryScreen.Bounds.Height;
		}

		//Returns the width of the primary display
		static private int GetScreenWidth()
		{
			return Screen.PrimaryScreen.Bounds.Height;
		}

		//Returns the color of a given pixel as a Color struct
		static private Color getRGB(int x, int y)
		{
			Color color = Win32.GetPixelColor(x, y);
			return color;
		}

		//Converts HSB values to a Color struct
		//Courtesy of Stack Overflow
		//http://stackoverflow.com/questions/4106363/converting-rgb-to-hsb-colors
		public static Color HSBtoRGB(float hue, float saturation, float brightness)
		{
			int r = 0, g = 0, b = 0;
			if (saturation == 0)
			{
				r = g = b = (int)(brightness * 255.0f + 0.5f);
			}
			else
			{
				float h = (hue - (float)Math.Floor(hue)) * 6.0f;
				float f = h - (float)Math.Floor(h);
				float p = brightness * (1.0f - saturation);
				float q = brightness * (1.0f - saturation * f);
				float t = brightness * (1.0f - (saturation * (1.0f - f)));
				switch ((int)h)
				{
					case 0:
						r = (int)(brightness * 255.0f + 0.5f);
						g = (int)(t * 255.0f + 0.5f);
						b = (int)(p * 255.0f + 0.5f);
						break;
					case 1:
						r = (int)(q * 255.0f + 0.5f);
						g = (int)(brightness * 255.0f + 0.5f);
						b = (int)(p * 255.0f + 0.5f);
						break;
					case 2:
						r = (int)(p * 255.0f + 0.5f);
						g = (int)(brightness * 255.0f + 0.5f);
						b = (int)(t * 255.0f + 0.5f);
						break;
					case 3:
						r = (int)(p * 255.0f + 0.5f);
						g = (int)(q * 255.0f + 0.5f);
						b = (int)(brightness * 255.0f + 0.5f);
						break;
					case 4:
						r = (int)(t * 255.0f + 0.5f);
						g = (int)(p * 255.0f + 0.5f);
						b = (int)(brightness * 255.0f + 0.5f);
						break;
					case 5:
						r = (int)(brightness * 255.0f + 0.5f);
						g = (int)(p * 255.0f + 0.5f);
						b = (int)(q * 255.0f + 0.5f);
						break;
				}
			}
			return Color.FromArgb(Convert.ToByte(255), Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));
		}
		//========================


	}
}

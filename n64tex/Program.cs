using System;
using System.IO;
using System.Drawing;

namespace n64tex {
	class Program {
		static string[] formats = {
			"rgba5551",
			"i4",
			"i8",
			"ia4",
			"ia8",
			// TODO: ia16
			// TODO: ci4
			// TODO: ci8
		};

		public static double GetIntensity(Color color) {
			return (color.R + color.G + color.B) / 3;
		}

		static void Main(string[] args) {
			if (args.Length < 2) {
				Console.WriteLine("Usage: n64tex format pngfile");
				return;
			}

			string format = args[0];
			string filename = args[1];

			int pos = Array.IndexOf(formats, format);
			if (pos < 0) {
				Console.WriteLine("Invalid texture format " + format);
				return;
			}

			if (!File.Exists(Path.GetDirectoryName(filename) + "\\" + Path.GetFileName(filename))) {
				Console.WriteLine("The file " + filename + " does not exist");
				return;
			}

			Bitmap thePNG = new Bitmap(Path.GetDirectoryName(filename) + "\\" + Path.GetFileName(filename));

			// Delete the file if it exists.
			string newFileName = Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename) + "." + format;
			if (File.Exists(newFileName)) {
				File.Delete(newFileName);
			}

			try {
				// Create the file.
				using (FileStream fs = File.Create(newFileName)) {
					using (var writer = new BinaryWriter(fs)) {
						if (format == "rgba5551") {
							ushort[] RGBA5551Data = new ushort[thePNG.Height * thePNG.Width];
							for (int h = 0; h < thePNG.Height; h++) {
								for (int w = 0; w < thePNG.Width; w++) {
									Color pixel = thePNG.GetPixel(w, h);
									int red = pixel.R / 8 * 0x0800;
									int green = pixel.G / 8 * 0x0040;
									int blue = pixel.B / 8 * 0x0002;
									int alpha = pixel.A > 0 ? 1 : 0;
									RGBA5551Data[h * thePNG.Width + w] = (ushort)(red + green + blue + alpha);
								}
							}
							foreach (ushort item in RGBA5551Data) {
								if (BitConverter.IsLittleEndian) {
									var output = BitConverter.GetBytes(item);
									Array.Reverse(output);
									writer.Write(output);
								} else {
									writer.Write(item);
								}
							}
						}

						if (format == "i4") {
							int bytesRequred = (thePNG.Height * thePNG.Width) / 2;
							byte[] i4Data = new byte[bytesRequred];
							int i = 0;
							int p = 0;

							for (int h = 0; h < thePNG.Height; h++) {
								for (int w = 0; w < thePNG.Width; w++) {
									Color pixel = thePNG.GetPixel(w, h);
									int nibble = (int)GetIntensity(pixel) / 16;
									if (i % 2 == 0) {
										i4Data[p] = (byte)(nibble << 4);
									} else {
										i4Data[p] |= (byte)nibble;
										p++;
									}
									i++;
								}
							}
							foreach (byte item in i4Data) {
								writer.Write(item);
							}
						}

						if (format == "ia4") {
							int bytesRequred = (thePNG.Height * thePNG.Width) / 2;
							byte[] ia4Data = new byte[bytesRequred];
							int i = 0;
							int p = 0;

							for (int h = 0; h < thePNG.Height; h++) {
								for (int w = 0; w < thePNG.Width; w++) {
									Color pixel = thePNG.GetPixel(w, h);
									int nibble = ((byte)((int)GetIntensity(pixel) / 32) << 1) + (pixel.A > 0 ? 1 : 0);
									if (i % 2 == 0) {
										ia4Data[p] = (byte)(nibble << 4);
									} else {
										ia4Data[p] |= (byte)nibble;
										p++;
									}
									i++;
								}
							}
							foreach (byte item in ia4Data) {
								writer.Write(item);
							}
						}

						if (format == "i8") {
							int bytesRequred = (thePNG.Height * thePNG.Width);
							byte[] i8Data = new byte[bytesRequred];
							int i = 0;

							for (int h = 0; h < thePNG.Height; h++) {
								for (int w = 0; w < thePNG.Width; w++) {
									Color pixel = thePNG.GetPixel(w, h);
									i8Data[i++] = (byte)GetIntensity(pixel);
								}
							}
							foreach (byte item in i8Data) {
								writer.Write(item);
							}
						}

						if (format == "ia8") {
							int bytesRequred = (thePNG.Height * thePNG.Width);
							byte[] ia8Data = new byte[bytesRequred];
							int i = 0;

							for (int h = 0; h < thePNG.Height; h++) {
								for (int w = 0; w < thePNG.Width; w++) {
									Color pixel = thePNG.GetPixel(w, h);
									ia8Data[i++] = (byte)((((int)GetIntensity(pixel) / 16) << 4) + (pixel.A / 16));
								}
							}
							foreach (byte item in ia8Data) {
								writer.Write(item);
							}
						}

					}
				}
			} catch (Exception ex) {
				Console.WriteLine(ex.ToString());
			}
		}
	}
}
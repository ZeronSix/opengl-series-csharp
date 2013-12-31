/*
 tdogl.Bitmap
 
 WARNING - THIS IS NOT COMPLETELY PORTED CLASS
 
 Copyright 2012 Thomas Dalling - http://tomdalling.com/
 C# Port is made by Vyacheslav Zeronov - zeronsix@gmail.com
 C# Port is based on Pencil.Gaming library by Antonie Blom - https://github.com/antonijn/Pencil.Gaming

 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
 */
using System;
using System.Drawing;
using System.Drawing.Imaging;
using Pencil.Gaming.Graphics;
using BitmapImage = System.Drawing.Bitmap;
using BitmapPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace tdogl
{
    /// <summary>
    /// A bitmap image (i.e. a grid of pixels).
    /// 
    /// This is not really related to OpenGL, but can be used to make OpenGL textures using
    /// tdogl.Texture.
    /// </summary>
    public class Bitmap
    {
        /// <summary>
        /// Creates a new image with the specified width, height and format.
        ///
        /// Width and height are in pixels.
        /// </summary>
        public Bitmap(uint width, uint height, BitmapPixelFormat format, BitmapImage bmp)
        {
            _bitmap = bmp;
            _width = width;
            _height = height;
            _format = format;
        }

        /// <summary>
        /// Tries to load the given file into a tdogl::Bitmap.
        /// </summary>
        public static Bitmap LoadFromFile(string filePath)
        {
            BitmapImage img = new BitmapImage(filePath);
            return new Bitmap((uint)img.Width, (uint)img.Height, img.PixelFormat, img);
        }

        /// <summary>
        /// width in pixels
        /// </summary>
        public uint Width
        {
            get { return _width; }
        }

        /// <summary>
        /// height in pixels
        /// </summary>
        public uint Height
        {
            get { return _height; }
        }

        /// <summary>
        /// the pixel format of the bitmap
        /// </summary>
        public BitmapPixelFormat Format
        {
            get { return _format; }
        }

        /// <summary>
        ///  Pointer to the raw pixel data of the bitmap.
        /// 
        /// Each channel is 1 byte. The number and meaning of channels per pixel is specified
        /// by the `Format` of the image. The pointer points to all the columns of
        /// the top row of the image, followed by each remaining row down to the bottom.
        /// i.e. c0r0, c1r0, c2r0, ..., c0r1, c1r1, c2r1, etc
        /// </summary>
        public IntPtr PixelBuffer
        {
            get {
                _data = _bitmap.LockBits(new Rectangle(Point.Empty, _bitmap.Size),
                    ImageLockMode.ReadOnly, _format);
                return _data.Scan0;
            }
        }

        public void Unlock()
        {
            _bitmap.UnlockBits(_data);
        }

        /// <summary>
        /// Reverses the row order of the pixels, so the bitmap will be upside down.
        /// </summary>
        public void FlipVertically()
        {
            _bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }

        /// <summary>
        /// Rotates the image 90 degrees counter clockwise.
        /// </summary>
        public void Rotate90CounterClockwise()
        {
            _bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public Bitmap(Bitmap other)
        {
            _bitmap = other._bitmap;
            _width = other._width;
            _height = other._height;
            _format = other._format;
        }

        private BitmapData _data;
        private BitmapPixelFormat _format;
        private uint _width;
        private uint _height;
        private BitmapImage _bitmap;
    }
}

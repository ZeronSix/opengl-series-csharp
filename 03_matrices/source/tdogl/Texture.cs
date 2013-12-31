/*
 tdogl.Texture
 
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
using Pencil.Gaming.Graphics;
using BitmapPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace tdogl
{
    /// <summary>
    /// Represents an OpenGL texture
    /// </summary>
    public class Texture: IDisposable
    {
        private PixelInternalFormat TextureFormatForBitmapFormat(BitmapPixelFormat format)
        {
            switch (format) {
                case BitmapPixelFormat.Format16bppGrayScale: return PixelInternalFormat.Luminance;
                //case Format.GrayscaleAlpha: return PixelInternalFormat.LuminanceAlpha;
                case BitmapPixelFormat.Format24bppRgb: return PixelInternalFormat.Rgb;
                case BitmapPixelFormat.Format32bppArgb: return PixelInternalFormat.Rgba;
                default: throw new Exception("Unrecognised Bitmap.Format");
            }
        }

        private PixelFormat PixelFormatForBitmap(BitmapPixelFormat format)
        {
            switch (format) {
                case BitmapPixelFormat.Format16bppGrayScale: return PixelFormat.Luminance;
                //case Format.GrayscaleAlpha: return PixelInternalFormat.LuminanceAlpha;
                case BitmapPixelFormat.Format24bppRgb: return PixelFormat.Bgr;
                case BitmapPixelFormat.Format32bppArgb: return PixelFormat.Bgra;
                default: throw new Exception("Unrecognised Bitmap.Format");
            }
        }

        public Texture(Bitmap bitmap, TextureMagFilter minMagFilter = TextureMagFilter.Linear,
                                      TextureWrapMode wrapMode = TextureWrapMode.ClampToEdge)
        {
            GL.GenTextures(1, out _object);
            GL.BindTexture(TextureTarget.Texture2D, _object);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minMagFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)minMagFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapMode);
            GL.TexImage2D(TextureTarget.Texture2D,
                          0,
                          TextureFormatForBitmapFormat(bitmap.Format),
                          (int)bitmap.Width,
                          (int)bitmap.Height,
                          0,
                          PixelFormatForBitmap(bitmap.Format),
                          PixelType.UnsignedByte,
                          bitmap.PixelBuffer);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            bitmap.Unlock();
        }

        public void Dispose()
        {
            GL.DeleteTextures(1, ref _object);
        }

        /// <summary>
        /// The texture object, as created by glGenTextures
        /// </summary>
        public uint GLObject
        {
            get { return _object; }
        }

        /// <summary>
        /// The original width (in pixels) of the bitmap this texture was made from
        /// </summary>
        public float OriginalWidth
        {
            get { return _originalWidth; }
        }

        /// <summary>
        /// The original height (in pixels) of the bitmap this texture was made from
        /// </summary>
        public float OriginalHeight
        {
            get { return _originalHeight; }
        }

        private uint _object;
        private float _originalWidth;
        private float _originalHeight;
    }
}

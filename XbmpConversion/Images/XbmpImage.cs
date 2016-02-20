using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace XBMPConverter.Images
{
    public class XbmpImage
    {
        private bool _needsLoad = true;
        private BinaryReader _reader;
        private BinaryWriter _writer;
        public Bitmap Image;
        public string Parent;
        public int Unk0, Unk1, Unk2, Unk3, Unk4;
        public int Width, Height, Width2;

        public XbmpImage(string parent)
        {
            Parent = parent;
        }

        public string GetStats()
        {
            return "Unk0 " + Unk0 + "\nUnk1 " + Unk1 + "\nWidth " + Width + "\nHeight " + Height + "\nWidth2 " + Width2 + "\nUnk2 " + Unk2 + "\nUnk3 " + Unk3 + "\nUnk4 " + Unk4;
        }
        /// <summary>
        ///     Set image that you wish to convert ot the XBMP format
        /// </summary>
        /// <param name="newImage"></param>
        public void SetImage(Bitmap newImage)
        {
            _needsLoad = false;
            Image = newImage;
            Width = newImage.Width;
            Width2 = Width;
            Height = newImage.Height;
            Unk1 = Unk3 = 0;
            Unk2 = Unk4 = 3;
            Unk0 = Width*Height*4;
        }


        /// <summary>
        ///     Read the header of an XBMP image
        /// </summary>
        private void ReadHeader()
        {
            Unk0 = _reader.ReadInt32();
            Unk1 = _reader.ReadInt32();
            Width = _reader.ReadInt32();
            Height = _reader.ReadInt32();
            Width2 = _reader.ReadInt32();
            Unk2 = _reader.ReadInt32();
            Unk3 = _reader.ReadInt32();
            Unk4 = _reader.ReadInt32();
        }


        /// <summary>
        ///     Save writer the XBMP header for a custom image
        /// </summary>
        private void SaveHeader()
        {
            _writer.Write(Unk0);
            _writer.Write(Unk1);
            _writer.Write(Width);
            _writer.Write(Height);
            _writer.Write(Width2);
            _writer.Write(Unk2);
            _writer.Write(Unk3);
            _writer.Write(Unk4);
        }


        /// <summary>
        ///     Read each pixel of an XBMP image
        /// </summary>
        private void ReadImage()
        {
            Image = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    Image.SetPixel(x, y, Color.FromArgb(_reader.ReadInt32()));
                }
            }
        }

        /// <summary>
        ///     Save an image as XBMP
        /// </summary>
        private void SaveImage()
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    _writer.Write(Image.GetPixel(x, y).ToArgb());
                }
            }
        }

        /// <summary>
        ///     Load all the data we need from an XBMP image
        /// </summary>
        public void Load()
        {
            if (_needsLoad)
            {
                _reader = new BinaryReader(File.Open(Parent, FileMode.Open));
                _needsLoad = false;
                ReadHeader();
                ReadImage();
                _reader.Close();
            }
        }

        /// <summary>
        ///     Save your image as XBMP
        /// </summary>
        public void Save()
        {
            _writer = new BinaryWriter(File.Open(Parent, FileMode.Open));
            SaveHeader();
            SaveImage();
            _writer.Close();
        }

        public void Close()
        {
            _writer?.Close();
            _reader?.Close();
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XBMPConverter.Images;

namespace XBMPConverter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {


            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                var imagePath = openFileDialog1.FileName;
                if (File.Exists(imagePath))
                {
                    var xbmp = new XbmpImage(imagePath);
              //      xbmp.SetImage(Image.FromFile(imagePath));
                    xbmp.Load();
                    xbmp.Image.Save("tst.bmp", ImageFormat.Bmp);
               //     var image = (Bitmap) Image.FromFile("tst.bmp");
            //        xbmp.SetImage(image);
                  //   xbmp.Save();
                }


            }
        }
    }
}

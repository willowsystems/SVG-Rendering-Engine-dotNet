using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Svg;
using Svg.Transforms;

namespace SVGViewer
{
    public partial class SVGViewer : Form
    {
        private string currentFile;

        public SVGViewer()
        {
            InitializeComponent();
            currentFile = null;
        }

        private Bitmap renderFile(string fileName)
        {
            var displaySize = imageBox.Size;
            
            SvgDocument svgDoc = SvgDocument.Open(openSvgFile.FileName);
            var svgSize = svgDoc.GetDimensions();

            if (svgSize.Width == 0)
            {
                throw new Exception("SVG does not have size specified. Cannot work with it.");
            }

            var displayProportion = (displaySize.Height * 1.0f) / displaySize.Width;
            var svgProportion = svgSize.Height / svgSize.Width;

            float scalingFactor = 0f;
            int padding = 10; // screen pixels

            // if display is proportionally narrower than svg
            if (displayProportion > svgProportion)
            {
                // we pick the width of display as max and compute the scaling against that.
                scalingFactor = (( displaySize.Width - padding * 2 )  * 1.0f) / svgSize.Width;
            }
            else
            {
                // we pick the height of display as max and compute the scaling against that.
                scalingFactor = ((displaySize.Height - padding * 2) * 1.0f) / svgSize.Height;
            }

            if (scalingFactor < 0)
            {
                throw new Exception("Viewing area is too small to render the image");
            }

            // When proportions of drawing do not match viewing area, it's nice to center the drawing within the viewing area.
            int centeringX = Convert.ToInt16((displaySize.Width - (padding + svgDoc.Width * scalingFactor)) / 2);
            int centeringY = Convert.ToInt16((displaySize.Height - (padding + svgDoc.Height * scalingFactor)) / 2);

            // Remove the "+ centering*" to avoid growing and padding the Bitmap with transparent fill.
            svgDoc.Transforms = new SvgTransformCollection();
            svgDoc.Transforms.Add(new SvgTranslate(padding + centeringX, padding + centeringY));
            svgDoc.Transforms.Add(new SvgScale(scalingFactor));

            //// This scales the bitmap with the content
            //svgDoc.Width = new SvgUnit(svgDoc.Width.Type, padding + svgDoc.Width * scalingFactor);
            //svgDoc.Height = new SvgUnit(svgDoc.Height.Type, padding + svgDoc.Height * scalingFactor);

            // This keeps the size of bitmap fixed to stated viewing area. Image is padded with transparent areas.
            svgDoc.Width = new SvgUnit(svgDoc.Width.Type, displaySize.Width);
            svgDoc.Height = new SvgUnit(svgDoc.Height.Type, displaySize.Height);

            return svgDoc.Draw();
        }


        private void open_Click(object sender, EventArgs e)
        {
            if (openSvgFile.ShowDialog() == DialogResult.OK)
            {
                currentFile = openSvgFile.FileName;
                imageBox.Image = renderFile(currentFile);
            }
        }

        private void imageBox_SizeChanged(object sender, EventArgs e)
        {
            if (currentFile != null)
            {
                imageBox.Image = renderFile(currentFile);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (currentFile != null)
            {
                var b = renderFile(currentFile);

                b.Save(
                    currentFile + DateTime.Now.Ticks.ToString() + ".png"
                    , System.Drawing.Imaging.ImageFormat.Png
                );
            }
        }
    }
}

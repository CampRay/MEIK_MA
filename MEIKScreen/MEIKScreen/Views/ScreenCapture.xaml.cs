using MEIKScreen.Common;
using MEIKScreen.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MEIKScreen.Views
{
    /// <summary>
    /// ScreenCapture.xaml 的交互逻辑
    /// </summary>
    public partial class ScreenCapture : Window
    {
        public DelegateHelper.commonDelgateMethod callbackMethod;
        public double x;
        public double y;
        public double width;
        public double height;
        public bool isMouseDown = false;
        private Person selectedUser = null;
        public ScreenCapture()
        {
            InitializeComponent();
        }
        public ScreenCapture(object data)
            : this()
        {
            this.selectedUser = data as Person;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
            x = e.GetPosition(null).X;
            y = e.GetPosition(null).Y;
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this.isMouseDown)
            {
                double curx = e.GetPosition(null).X;
                double cury = e.GetPosition(null).Y;

                System.Windows.Shapes.Rectangle r = new System.Windows.Shapes.Rectangle();
                SolidColorBrush brush = new SolidColorBrush(Colors.White);
                r.Stroke = brush;
                r.Fill = brush;
                r.StrokeThickness = 1;
                r.Width = Math.Abs(curx - x);
                r.Height = Math.Abs(cury - y);
                cnv.Children.Clear();
                cnv.Children.Add(r);
                Canvas.SetLeft(r, x);
                Canvas.SetTop(r, y);
                if (e.LeftButton == MouseButtonState.Released)
                {
                    cnv.Children.Clear();
                    width = r.Width;
                    height = r.Height;
                    this.CaptureScreen(x, y, width, height);
                    this.x = this.y = 0;
                    this.isMouseDown = false;
                    this.Close();                    
                }
            }
        }

        public void CaptureScreen(double x, double y, double width, double height)
        {
            int ix, iy, iw, ih;
            ix = Convert.ToInt32(x);
            iy = Convert.ToInt32(y);
            iw = Convert.ToInt32(width);
            ih = Convert.ToInt32(height);
            string screenshotFolder = this.selectedUser.ArchiveFolder;
            try{                
                Bitmap image = new Bitmap(iw, ih, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                
                Graphics g = Graphics.FromImage(image);
                g.CopyFromScreen(ix-6, iy-6, 0, 0, new System.Drawing.Size(iw+12, ih), CopyPixelOperation.SourceCopy);                
                
                string imgFileName = screenshotFolder + System.IO.Path.DirectorySeparatorChar + this.selectedUser.Code+".png";
                //string jpgFileName = screenshotFolder + System.IO.Path.DirectorySeparatorChar + this.selectedUser.Code + ".jpg";
                image.Save(imgFileName, ImageFormat.Png);
                //image.Save(jpgFileName, ImageFormat.Jpeg);                
                image.Dispose();
                //File.Copy(imgFileName, jpgFileName);
                if (this.callbackMethod != null)
                {
                    this.callbackMethod(imgFileName);
                }
            }
            catch(Exception)
            {
                FileHelper.SetFolderPower(screenshotFolder, "Everyone", "FullControl");
                FileHelper.SetFolderPower(screenshotFolder, "Users", "FullControl");
                System.Windows.MessageBox.Show(this, App.Current.FindResource("Message_10").ToString());
            }
            //App.opendWin.Owner.Visibility = Visibility.Visible;
            App.opendWin.WindowState = WindowState.Normal;
            App.opendWin.Owner.WindowState = WindowState.Normal;
            this.Close();

            //SaveFileDialog dlg = new SaveFileDialog();
            //dlg.DefaultExt = "png";
            //dlg.Filter = "Png Files|*.png";
            //DialogResult res = dlg.ShowDialog();
            //if (res == System.Windows.Forms.DialogResult.OK)
                //image.Save(dlg.FileName, ImageFormat.Png);
        }

        //public void SaveScreen(double x, double y, double width, double height)
        //{
        //    int ix, iy, iw, ih;
        //    ix = Convert.ToInt32(x);
        //    iy = Convert.ToInt32(y);
        //    iw = Convert.ToInt32(width);
        //    ih = Convert.ToInt32(height);
        //    try
        //    {
        //        Bitmap myImage = new Bitmap(iw, ih);

        //        Graphics gr1 = Graphics.FromImage(myImage);
        //        IntPtr dc1 = gr1.GetHdc();
        //        IntPtr dc2 = Win32Api.GetWindowDC(Win32Api.GetForegroundWindow());
        //        Win32Api.BitBlt(dc1, ix, iy, iw, ih, dc2, ix, iy, 13369376);
        //        gr1.ReleaseHdc(dc1);
        //        SaveFileDialog dlg = new SaveFileDialog();
        //        dlg.DefaultExt = "png";
        //        dlg.Filter = "Png Files|*.png";
        //        DialogResult res = dlg.ShowDialog();
        //        if (res == System.Windows.Forms.DialogResult.OK)
        //            myImage.Save(dlg.FileName, ImageFormat.Png);
        //    }
        //    catch (Exception ex) {
        //        System.Windows.MessageBox.Show(ex.Message);
        //    }
        //}
    }
}

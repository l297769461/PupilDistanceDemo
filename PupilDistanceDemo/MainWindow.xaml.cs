using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PupilDistanceDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isLeftButtonDown = false;
        Image image;

        bool isPupilDistance = false;
        Size size;

        Point currentPoint;

        public MainWindow()
        {
            InitializeComponent();

            img1.MouseLeftButtonDown += Img_MouseLeftButtonDown;
            img1.MouseMove += Img_MouseMove;
            img1.MouseLeftButtonUp += Img_MouseLeftButtonUp;

            img2.MouseLeftButtonDown += Img_MouseLeftButtonDown;
            img2.MouseMove += Img_MouseMove;
            img2.MouseLeftButtonUp += Img_MouseLeftButtonUp;

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            size = new Size(img2.ActualWidth, img2.ActualHeight);
        }

        private void Img_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isLeftButtonDown = false;
            image = null;
        }

        private void Img_MouseMove(object sender, MouseEventArgs e)
        {
            if (isLeftButtonDown && sender is Image imgc)
            {
                var point = e.GetPosition(canvas);
                if (isPupilDistance && imgc.Tag is Line line)
                {
                    if (image.Equals(imgc))
                    {
                        var x = point.X;
                        var y = point.Y;
                        if (x > line.X1) x -= 2;
                        else x += 2;
                        if (y > line.Y1) y -= 2;
                        else y += 2;
                        line.X2 = x;
                        line.Y2 = y;
                    }
                }
                else if (sender is Image image)
                {
                    image.SetValue(Canvas.LeftProperty, point.X - currentPoint.X);
                    image.SetValue(Canvas.TopProperty, point.Y - currentPoint.Y);
                }
            }
        }

        private void Img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            image = sender as Image;
            isLeftButtonDown = true;
            if (sender is Image imgc)
            {
                currentPoint = e.GetPosition(imgc);

                if (isPupilDistance)
                {
                    if (imgc.Tag is Line line)
                    {
                        canvas.Children.Remove(line);
                    }

                    line = new Line();
                    line.StrokeThickness = 2;
                    line.Stroke = new SolidColorBrush(Colors.Red);
                    var point = e.GetPosition(canvas);
                    line.X1 = point.X - 1;
                    line.Y1 = point.Y - 1;
                    line.X2 = line.X1;
                    line.Y2 = line.Y1;
                    canvas.Children.Add(line);
                    imgc.Tag = line;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            isPupilDistance = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            isPupilDistance = false;
        }

        /// <summary>
        /// 计算瞳距
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var l1 = img1.Tag as Line;
            var l2 = img2.Tag as Line;

            if (l1 == null || l2 == null)
            {
                MessageBox.Show("请先 启用瞳距 ，再在图片上画线");
                return;
            }

            //获取第一个图片的线
            var length1 = Distance(new Point(l1.X1, l1.Y1), new Point(l1.X2, l1.Y2));

            //获取第二个图片的线
            var length2 = Distance(new Point(l2.X1, l2.Y1), new Point(l2.X2, l2.Y2));

            //利用三角函数计算出以第一个图的线为基准，延长第二个图的线
            var AC = Math.Abs(l2.X2 - l2.X1);
            var BC = Math.Abs(l2.Y2 - l2.Y1);

            var jiaodu = Math.Atan(BC / AC);
            var sinVal = Math.Sin(jiaodu);
            var cosVal = Math.Cos(jiaodu);
            var ac = cosVal * length1;
            var bc = sinVal * length1;

            double xnew = 0, ynew = 0;
            if (l2.X2 > l2.X1) xnew = ac + l2.X1;
            else xnew = l2.X1 - ac;

            if (l2.Y2 > l2.Y1) ynew = l2.Y1 + bc;
            else ynew = l2.Y1 - bc;

            l2.X2 = xnew;
            l2.Y2 = ynew;

            var wnew = length1 / (length2 / img2.ActualWidth);
            var hnew = length1 / (length2 / img2.ActualHeight);

            //以用户画的起点作为缩放中心
            var x = (double)img2.GetValue(Canvas.LeftProperty);
            var y = (double)img2.GetValue(Canvas.TopProperty);

            //起始点相对于图片的位置
            var l2xToimg = l2.X1 - x;
            var l2yToimg = l2.Y1 - y;

            //获取起始点相对于图片的新位置，缩放后
            var l2xToimgnew = l2xToimg / img2.ActualWidth * wnew;
            var l2yToimgnew = l2yToimg / img2.ActualHeight * hnew;

            img2.SetValue(Canvas.LeftProperty, l2.X1 - l2xToimgnew);
            img2.SetValue(Canvas.TopProperty, l2.Y1 - l2yToimgnew);

            //缩放
            img2.Width = wnew;
            img2.Height = hnew;
        }

        /// <summary>
        /// 计算点位之间的距离
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private double Distance(Point p1, Point p2)
        {
            double result = 0;
            result = Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
            return result;
        }

        /// <summary>
        /// 重置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            List<Line> l = new List<Line>();
            foreach (var item in canvas.Children)
            {
                if (item is Line line)
                {
                    l.Add(line);
                }
            }

            l.ForEach(c => canvas.Children.Remove(c));

            img2.Width = size.Width;
            img2.Height = size.Height;

            img2.SetValue(Canvas.LeftProperty, 380.0);
            img2.SetValue(Canvas.TopProperty, 100.0);
        }
    }
}

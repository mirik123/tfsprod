using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using TFSExt.ShowRevHist;

namespace tfsprod
{
    /// <summary>
    /// Interaction logic for WpfRevisionHistoryControl.xaml
    /// </summary>
    public partial class WpfRevisionHistoryControl : UserControl
    {
        public dynamic data = null;
        
        public void FillCanvasObjects()
        {
            double xdelta = canvas.ActualWidth / data.xlines, ydelta = canvas.ActualHeight / data.ylines, x = 0, y = 0;
            TextBlock txtblk;
            Line lnblk;
            Ellipse elblk;

            if (xdelta > 100) xdelta = 100;

            canvas.Children.Clear();
            foreach (BTreeItem itm in data.branches)
            {
                y = itm.relY * ydelta;
                                
                lnblk = new Line 
                { 
                    Y2 = y, 
                    StrokeThickness = 1, 
                    StrokeDashArray = new DoubleCollection(new double[] {2, 2}),
                    Stroke = Brushes.Black
                };
                Canvas.SetLeft(lnblk, x);
                Canvas.SetTop(lnblk, 10);     
                canvas.Children.Add(lnblk);

                elblk = new Ellipse
                {
                    Stroke = Brushes.Red,
                    Fill = Brushes.Red,
                    Height = 20,
                    Width = 20
                };
                Canvas.SetLeft(elblk, x-10);
                Canvas.SetTop(elblk, y);
                canvas.Children.Add(elblk);

                lnblk = new Line
                {
                    Y2 = canvas.ActualHeight - y,
                    Stroke = Brushes.Red,
                    StrokeThickness = 5
                };
                Canvas.SetLeft(lnblk, x);
                Canvas.SetTop(lnblk, y+10);
                canvas.Children.Add(lnblk);

                if (itm.parent != null)
                {
                    lnblk = new Line
                    {
                        X2 = xdelta * (itm.relX - itm.parent.relX),
                        Stroke = Brushes.Red,
                        StrokeThickness = 5
                    };
                    Canvas.SetLeft(lnblk, itm.parent.relX * xdelta);
                    Canvas.SetTop(lnblk, y+10);
                    canvas.Children.Add(lnblk);

                    elblk = new Ellipse
                    {
                        Stroke = Brushes.Red,
                        Fill = Brushes.Red,
                        Height = 20,
                        Width = 20
                    };
                    Canvas.SetLeft(elblk, itm.parent.relX * xdelta - 10);
                    Canvas.SetTop(elblk, y);
                    canvas.Children.Add(elblk);
                }

                foreach (var ch in itm.Items)
                {
                    var locY = ch.CreationDate.Subtract(itm.CreationDate).TotalDays * ydelta;
                    elblk = new Ellipse
                    {
                        Stroke = Brushes.Blue,
                        Fill = Brushes.Blue,
                        Height = 10,
                        Width = 10
                    };
                    Canvas.SetLeft(elblk, x-5);
                    Canvas.SetTop(elblk, y + locY);
                    canvas.Children.Add(elblk);

                    txtblk = new TextBlock { Text = ch.ChangesetId.ToString(), FontSize = 8 };
                    Canvas.SetLeft(txtblk, x+5);
                    Canvas.SetTop(txtblk, y + locY+5);
                    canvas.Children.Add(txtblk);
                }

                foreach (var relbr in itm.RelatedBranches)
                {
                    foreach (var ch in relbr.Item2)
                    {
                        var locY = ch.CreationDate.Subtract(itm.CreationDate).TotalDays * ydelta;
                        lnblk = new Line
                        {
                            X2 = xdelta * Math.Abs(itm.relX - relbr.Item1.relX),
                            StrokeThickness = 1,
                            StrokeDashArray = new DoubleCollection(new double[] { 2, 2 }),
                            Stroke = Brushes.Black
                        };
                        Canvas.SetLeft(lnblk, Math.Min(itm.relX, relbr.Item1.relX) * xdelta);
                        Canvas.SetTop(lnblk, y + locY+5);
                        canvas.Children.Add(lnblk);
                    }
                }

                txtblk = new TextBlock { Text = itm.Path, FontSize = 8 };
                Canvas.SetLeft(txtblk, x);
                Canvas.SetTop(txtblk, y);
                canvas.Children.Add(txtblk);

                x += xdelta;
            }          
        }
        
        public WpfRevisionHistoryControl()
        {
            InitializeComponent();

            Loaded += (sender, e) => FillCanvasObjects();
            SizeChanged += (sender, e) => FillCanvasObjects();
        }
    }
}

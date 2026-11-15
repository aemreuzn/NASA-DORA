using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NasaAstronautTraining.Controls;

public partial class CircularProgressBar : UserControl
{
    public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
        nameof(Progress),
        typeof(double),
        typeof(CircularProgressBar),
        new PropertyMetadata(0.0, OnProgressChanged));

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public CircularProgressBar()
    {
        InitializeComponent();
        Loaded += (s, e) => UpdateArc();
    }

    private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CircularProgressBar bar)
        {
            bar.UpdateArc();
        }
    }

    private void UpdateArc()
    {
        double val = Math.Clamp(Progress, 0.0, 100.0);

        // Renk kuralı: Kırmızı -> Sarı/Turuncu -> Yeşil
        Color strokeColor;
        if (val < 35.0)
        {
            strokeColor = Color.FromRgb(0xEF, 0x44, 0x44); // Kırmızı
        }
        else if (val < 75.0)
        {
            strokeColor = Color.FromRgb(0xF5, 0x9E, 0x0B); // Sarı / Amber
        }
        else
        {
            strokeColor = Color.FromRgb(0x10, 0xB9, 0x81); // Zümrüt Yeşili
        }

        var brush = new SolidColorBrush(strokeColor);
        brush.Freeze();
        ProgressArc.Stroke = brush;
        CenterDot.Fill = brush;

        if (val <= 0.0)
        {
            ProgressArc.Data = null;
            return;
        }

        if (val >= 99.9)
        {
            // Tam daire
            var fullGeom = new EllipseGeometry(new Point(22, 22), 19, 19);
            fullGeom.Freeze();
            ProgressArc.Data = fullGeom;
            return;
        }

        // Yay hesaplama
        double radius = 19.0;
        Point center = new Point(22, 22);
        Point startPoint = new Point(center.X, center.Y - radius);

        double angle = (val / 100.0) * 360.0;
        double rad = (angle - 90.0) * (Math.PI / 180.0);
        Point endPoint = new Point(
            center.X + radius * Math.Cos(rad),
            center.Y + radius * Math.Sin(rad));

        bool isLargeArc = angle > 180.0;

        var pathFigure = new PathFigure
        {
            StartPoint = startPoint,
            IsClosed = false,
            Segments = new PathSegmentCollection
            {
                new ArcSegment(endPoint, new Size(radius, radius), 0, isLargeArc, SweepDirection.Clockwise, true)
            }
        };

        var pathGeom = new PathGeometry();
        pathGeom.Figures.Add(pathFigure);
        pathGeom.Freeze();
        ProgressArc.Data = pathGeom;
    }
}

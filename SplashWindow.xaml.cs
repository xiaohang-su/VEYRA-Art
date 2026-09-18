using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ArtTest
{
    public partial class SplashWindow : Window
    {
        private Stopwatch _sw = new Stopwatch();
        private double _totalSeconds = 0;
        private const double AnimationDuration = 11.5;
        private bool _playing = true;

        private const double CenterX = 400;
        private const double CenterY = 300;

        private List<BgParticle> _bgParticles = new List<BgParticle>();
        private List<FlyParticle> _flyParticles = new List<FlyParticle>();
        private List<OrbitCube> _cubes = new List<OrbitCube>();

        private Random _rand = new Random();

        public SplashWindow()
        {
            InitializeComponent();
            Loaded += SplashWindow_Loaded;
            KeyDown += SplashWindow_KeyDown;
        }

        private void SplashWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitBgParticles();
            InitFlyParticles();
            InitCubes();
            _sw.Start();
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void SplashWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.R)
            {
                RestartAnimation();
            }
            else if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void RestartAnimation()
        {
            _sw.Restart();
            _playing = true;
            CoreLight.Opacity = 0;
            CoreLightScale.ScaleX = 0;
            CoreLightScale.ScaleY = 0;
            Crystal.Opacity = 0;
            CrystalScale.ScaleX = 0;
            CrystalScale.ScaleY = 0;
            CrystalBlur.Radius = 20;
            LoadingPanel.Opacity = 0;
            EnergyBar.Width = 0;
            Dot1.Fill = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x55));
            Dot2.Fill = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x55));
            Dot3.Fill = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x55));
            Dot4.Fill = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x55));
            StatusText.Text = "";
            foreach (var p in _flyParticles) p.Reset();
            foreach (var c in _cubes) c.Reset();
        }

        #region Init

        private void InitBgParticles()
        {
            for (int i = 0; i < 8; i++)
            {
                var p = new BgParticle
                {
                    X = _rand.Next(0, 800),
                    Y = _rand.Next(0, 600),
                    Size = _rand.Next(2, 5),
                    SpeedX = (_rand.NextDouble() - 0.5) * 0.3,
                    SpeedY = (_rand.NextDouble() - 0.5) * 0.3,
                    Opacity = _rand.NextDouble() * 0.3 + 0.1
                };
                var ellipse = new Ellipse
                {
                    Width = p.Size,
                    Height = p.Size,
                    Fill = new SolidColorBrush(Color.FromRgb(0x80, 0xDF, 0xFF)),
                    Opacity = p.Opacity
                };
                p.Element = ellipse;
                Canvas.SetLeft(ellipse, p.X);
                Canvas.SetTop(ellipse, p.Y);
                BgParticleCanvas.Children.Add(ellipse);
                _bgParticles.Add(p);
            }
        }

        private void InitFlyParticles()
        {
            for (int i = 0; i < 22; i++)
            {
                double angle = _rand.NextDouble() * Math.PI * 2;
                double dist = 400 + _rand.NextDouble() * 100;
                var p = new FlyParticle
                {
                    StartX = CenterX + Math.Cos(angle) * dist,
                    StartY = CenterY + Math.Sin(angle) * dist,
                    Delay = _rand.NextDouble() * 0.8,
                    Duration = 1.5 + _rand.NextDouble() * 0.5,
                    Size = _rand.Next(3, 7),
                    Progress = 0
                };
                var ellipse = new Ellipse
                {
                    Width = p.Size,
                    Height = p.Size,
                    Fill = new SolidColorBrush(Color.FromRgb(0x4F, 0xC3, 0xF7)),
                    Opacity = 0,
                    Effect = new BlurEffect { Radius = 2 }
                };
                p.Element = ellipse;
                FlyParticleCanvas.Children.Add(ellipse);
                _flyParticles.Add(p);
            }
        }

        private void InitCubes()
        {
            double[] sizes = { 45, 40, 28, 25, 22, 16 };
            double[] radii = { 160, 140, 120, 100, 130, 90 };
            double[] speeds = { 0.3, 0.35, 0.5, 0.6, 0.45, 0.7 };
            bool[] directions = { false, true, false, true, false, true };

            for (int i = 0; i < 6; i++)
            {
                var cube = new OrbitCube
                {
                    Size = sizes[i],
                    Radius = radii[i],
                    Angle = _rand.NextDouble() * Math.PI * 2,
                    AngularSpeed = speeds[i] * (directions[i] ? 1 : -1),
                    Delay = i * 0.15,
                    Element = CreateCube(sizes[i])
                };
                CubeCanvas.Children.Add(cube.Element);
                _cubes.Add(cube);
            }
        }

        private Border CreateCube(double size)
        {
            var border = new Border
            {
                Width = size,
                Height = size,
                Opacity = 0,
                BorderBrush = new SolidColorBrush(Color.FromArgb(0x80, 0x80, 0xDF, 0xFF)),
                BorderThickness = new Thickness(1.5),
                Background = new SolidColorBrush(Color.FromArgb(0x10, 0x4F, 0xC3, 0xF7)),
                CornerRadius = new CornerRadius(2)
            };
            var grid = new Grid();
            var line1 = new Rectangle { Height = 1, Fill = new SolidColorBrush(Color.FromArgb(0x30, 0x80, 0xDF, 0xFF)), VerticalAlignment = VerticalAlignment.Center };
            var line2 = new Rectangle { Width = 1, Fill = new SolidColorBrush(Color.FromArgb(0x30, 0x80, 0xDF, 0xFF)), HorizontalAlignment = HorizontalAlignment.Center };
            grid.Children.Add(line1);
            grid.Children.Add(line2);
            border.Child = grid;
            return border;
        }

        #endregion

        #region Frame Update

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            if (!_playing) return;

            _totalSeconds = _sw.Elapsed.TotalSeconds;

            if (_totalSeconds > AnimationDuration + 2.0)
            {
                RestartAnimation();
                return;
            }

            double t = _totalSeconds;

            UpdateBgParticles();
            UpdateCoreLight(t);
            UpdateFlyParticles(t);
            UpdateCrystal(t);
            UpdateCubes(t);
            UpdateLoading(t);
            UpdateFadeOut(t);
        }

        private void UpdateBgParticles()
        {
            foreach (var p in _bgParticles)
            {
                p.X += p.SpeedX;
                p.Y += p.SpeedY;
                if (p.X < 0) p.X = 800;
                if (p.X > 800) p.X = 0;
                if (p.Y < 0) p.Y = 600;
                if (p.Y > 600) p.Y = 0;
                Canvas.SetLeft(p.Element, p.X);
                Canvas.SetTop(p.Element, p.Y);
            }
        }

        private void UpdateCoreLight(double t)
        {
            if (t < 1.0)
            {
                CoreLight.Opacity = 0;
                return;
            }

            double appearT = Math.Clamp((t - 1.0) / 1.5, 0, 1);
            double ease = 1 - Math.Pow(1 - appearT, 3);

            double baseSize = 40 + ease * 30;
            double pulse = 1 + Math.Sin(t * 3) * 0.1;
            double size = baseSize * pulse;

            CoreLight.Opacity = ease * 0.9;
            CoreLightScale.ScaleX = size / 20;
            CoreLightScale.ScaleY = size / 20;
            Canvas.SetLeft(CoreLight, CenterX - size / 2);
            Canvas.SetTop(CoreLight, CenterY - size / 2);

            if (t > 4.5)
            {
                double fadeT = Math.Clamp((t - 4.5) / 1.0, 0, 1);
                CoreLight.Opacity = ease * 0.9 * (1 - fadeT);
            }
        }

        private void UpdateFlyParticles(double t)
        {
            double startTime = 2.5;
            foreach (var p in _flyParticles)
            {
                double localT = t - startTime - p.Delay;
                if (localT < 0)
                {
                    p.Element.Opacity = 0;
                    continue;
                }

                p.Progress = Math.Clamp(localT / p.Duration, 0, 1);
                double ease = p.Progress * p.Progress;

                double x = p.StartX + (CenterX - p.StartX) * ease;
                double y = p.StartY + (CenterY - p.StartY) * ease;

                double opacity = 1;
                if (p.Progress > 0.7)
                {
                    opacity = 1 - (p.Progress - 0.7) / 0.3;
                }

                p.Element.Opacity = opacity * 0.9;
                Canvas.SetLeft(p.Element, x - p.Size / 2);
                Canvas.SetTop(p.Element, y - p.Size / 2);
            }
        }

        private void UpdateCrystal(double t)
        {
            if (t < 4.5)
            {
                Crystal.Opacity = 0;
                return;
            }

            double formT = Math.Clamp((t - 4.5) / 1.5, 0, 1);
            double ease = 1 - Math.Pow(1 - formT, 3);

            Crystal.Opacity = ease;
            CrystalScale.ScaleX = ease;
            CrystalScale.ScaleY = ease;

            CrystalBlur.Radius = 20 * (1 - ease);

            CrystalRotate.Angle = 45 + (t - 4.5) * 8;

            double flow1 = (Math.Sin(t * 1.5) + 1) / 2;
            CrystalFlow1.Margin = new Thickness(0, 20 + flow1 * 60, 0, 0);
            double flow2 = (Math.Cos(t * 1.2) + 1) / 2;
            CrystalFlow2.Margin = new Thickness(0, 30 + flow2 * 50, 0, 0);

            Canvas.SetLeft(Crystal, CenterX - 70);
            Canvas.SetTop(Crystal, CenterY - 70);
        }

        private void UpdateCubes(double t)
        {
            double startTime = 6.0;
            foreach (var c in _cubes)
            {
                double localT = t - startTime - c.Delay;
                if (localT < 0)
                {
                    c.Element.Opacity = 0;
                    continue;
                }

                double spreadT = Math.Clamp(localT / 1.0, 0, 1);
                double spreadEase = 1 - Math.Pow(1 - spreadT, 3);

                c.Angle += c.AngularSpeed * 0.016;

                double currentRadius = c.Radius * spreadEase;
                double x = CenterX + Math.Cos(c.Angle) * currentRadius;
                double y = CenterY + Math.Sin(c.Angle) * currentRadius * 0.6;

                double depthScale = 0.7 + (y - CenterY + 100) / 200 * 0.6;
                depthScale = Math.Clamp(depthScale, 0.5, 1.3);

                c.Element.Opacity = spreadEase * 0.7;
                c.Element.RenderTransform = new ScaleTransform(depthScale, depthScale, c.Size / 2, c.Size / 2);
                Canvas.SetLeft(c.Element, x - c.Size / 2);
                Canvas.SetTop(c.Element, y - c.Size / 2);

                Canvas.SetZIndex(c.Element, (int)y);
            }
        }

        private void UpdateLoading(double t)
        {
            if (t < 8.0)
            {
                LoadingPanel.Opacity = 0;
                return;
            }

            double appearT = Math.Clamp((t - 8.0) / 0.5, 0, 1);
            LoadingPanel.Opacity = appearT;

            double loadT = Math.Clamp((t - 8.0) / 2.5, 0, 1);
            double ease = 1 - Math.Pow(1 - loadT, 2);

            EnergyBar.Width = 280 * ease;

            UpdateDot(Dot1, loadT, 0.15, "CAMERA ENGINE");
            UpdateDot(Dot2, loadT, 0.40, "MODEL RUNTIME");
            UpdateDot(Dot3, loadT, 0.65, "GPU CORE");
            UpdateDot(Dot4, loadT, 0.90, "NEURAL SYSTEM");
        }

        private void UpdateDot(Ellipse dot, double loadT, double threshold, string status)
        {
            if (loadT >= threshold)
            {
                double dotT = Math.Clamp((loadT - threshold) / 0.1, 0, 1);
                byte r = (byte)(0x33 + (0x80 - 0x33) * dotT);
                byte g = (byte)(0x33 + (0xDF - 0x33) * dotT);
                byte b = (byte)(0x55 + (0xFF - 0x55) * dotT);
                dot.Fill = new SolidColorBrush(Color.FromRgb(r, g, b));
                dot.Width = 8 + dotT * 4;
                dot.Height = 8 + dotT * 4;

                if (loadT >= threshold && loadT < threshold + 0.15)
                {
                    StatusText.Text = status + " ... OK";
                }
            }
        }

        private void UpdateFadeOut(double t)
        {
            if (t < 10.5) return;

            double fadeT = Math.Clamp((t - 10.5) / 1.0, 0, 1);
            double opacity = 1 - fadeT;

            MainCanvas.Opacity = opacity;
        }

        #endregion
    }

    public class BgParticle
    {
        public double X, Y, Size, SpeedX, SpeedY, Opacity;
        public Ellipse Element = null!;
    }

    public class FlyParticle
    {
        public double StartX, StartY, Delay, Duration, Size, Progress;
        public Ellipse Element = null!;

        public void Reset()
        {
            Progress = 0;
            Element.Opacity = 0;
        }
    }

    public class OrbitCube
    {
        public double Size, Radius, Angle, AngularSpeed, Delay;
        public Border Element = null!;

        public void Reset()
        {
            Angle = new Random().NextDouble() * Math.PI * 2;
            Element.Opacity = 0;
        }
    }
}

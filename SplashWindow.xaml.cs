using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace ArtTest
{
    public partial class SplashWindow : Window
    {
        private Stopwatch _sw = new Stopwatch();
        private double _totalSeconds = 0;
        private const double AnimationDuration = 12.0;
        private bool _playing = true;

        private const double CenterX = 400;
        private const double CenterY = 300;

        private List<BgParticle> _bgParticles = new List<BgParticle>();
        private List<FlyParticle> _flyParticles = new List<FlyParticle>();
        private List<OrbitCube> _cubes = new List<OrbitCube>();
        private List<CoreParticle> _coreParticles = new List<CoreParticle>();
        private List<Fragment> _fragments = new List<Fragment>();

        private Random _rand = new Random();
        private double _breathPhase = 0;
        private double _edgeGlowPhase = 0;
        private double _streamAPhase = 0;
        private double _streamBPhase = 0;
        private double _streamCPhase = 0;
        private bool _reconstructStarted = false;
        private bool _seedStarted = false;

        private BlurEffect _energyBarBlur = new BlurEffect { Radius = 3 };
        private System.Windows.Threading.DispatcherTimer _reconstructTimer = new System.Windows.Threading.DispatcherTimer();

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
            InitCoreParticles();
            InitFragments();
            
            EnergyBar.Effect = _energyBarBlur;
            
            // 初始化重建阶段的碎片动画定时器
            _reconstructTimer.Interval = TimeSpan.FromMilliseconds(16);
            _reconstructTimer.Tick += (s, args) =>
            {
                foreach (var f in _fragments)
                {
                    f.X += f.VX;
                    f.Y += f.VY;
                    f.VX *= 0.99; // 水平阻力
                    f.VY += 0.15; // 重力效果，抛物线下落
                    f.Rotation += f.RotSpeed * 0.016;
                    f.Opacity -= 0.008;
                    f.Element.Opacity = Math.Max(0, f.Opacity);
                    f.RotateTransform.Angle = f.Rotation;
                    Canvas.SetLeft(f.Element, f.X - f.Size / 2);
                    Canvas.SetTop(f.Element, f.Y - f.Size / 2);
                }
            };
            
            // 窗口直接显示，不做淡入（避免黑窗）
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
            _reconstructStarted = false;
            _seedStarted = false;
            CrystalCore.Opacity = 0;
            CrystalScale.ScaleX = 0;
            CrystalScale.ScaleY = 0;
            LoadingPanel.Opacity = 0;
            EnergyBar.Width = 0;
            FragmentCanvas.Children.Clear();
            _fragments.Clear();
            CoreLight.Opacity = 0;
            CoreLightScale.ScaleX = 0;
            CoreLightScale.ScaleY = 0;
            ReconSeed.Opacity = 0;
            SeedScale.ScaleX = 0;
            SeedScale.ScaleY = 0;
            StatusText.Text = "";
            foreach (var p in _flyParticles) p.Reset();
            foreach (var c in _cubes) c.Reset();
            foreach (var p in _coreParticles) p.Reset();
            
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        #region Init

        private void InitBgParticles()
        {
            for (int i = 0; i < 350; i++)
            {
                var p = new BgParticle
                {
                    X = _rand.Next(0, 800),
                    Y = _rand.Next(0, 600),
                    Size = _rand.Next(1, 5),
                    SpeedX = (_rand.NextDouble() - 0.5) * 0.3,
                    SpeedY = (_rand.NextDouble() - 0.5) * 0.3,
                    Opacity = _rand.NextDouble() * 0.3 + 0.05,
                    IsPurple = _rand.NextDouble() > 0.8
                };
                var ellipse = new Ellipse
                {
                    Width = p.Size,
                    Height = p.Size,
                    Fill = new SolidColorBrush(p.IsPurple ? 
                        Color.FromRgb(0xB3, 0x88, 0xFF) : 
                        Color.FromRgb(0x80, 0xDF, 0xFF)),
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
            for (int i = 0; i < 500; i++)
            {
                double angle = _rand.NextDouble() * Math.PI * 2;
                double dist = 350 + _rand.NextDouble() * 250;
                
                double type = _rand.NextDouble();
                int size;
                double duration;
                if (type < 0.7) { size = _rand.Next(2, 4); duration = 1.0 + _rand.NextDouble() * 0.5; }
                else if (type < 0.9) { size = _rand.Next(4, 6); duration = 1.5 + _rand.NextDouble() * 0.5; }
                else { size = _rand.Next(6, 9); duration = 2.0 + _rand.NextDouble() * 0.8; }

                bool isPurple = _rand.NextDouble() > 0.85;

                var p = new FlyParticle
                {
                    StartX = CenterX + Math.Cos(angle) * dist,
                    StartY = CenterY + Math.Sin(angle) * dist,
                    Delay = _rand.NextDouble() * 1.0,
                    Duration = duration,
                    Size = size,
                    IsPurple = isPurple,
                    Progress = 0
                };
                var ellipse = new Ellipse
                {
                    Width = p.Size,
                    Height = p.Size,
                    Fill = new SolidColorBrush(isPurple ? 
                        Color.FromRgb(0xB3, 0x88, 0xFF) : 
                        Color.FromRgb(0x4F, 0xC3, 0xF7)),
                    Opacity = 0,
                    Effect = new BlurEffect { Radius = size > 5 ? 3 : 1 }
                };
                p.Element = ellipse;
                FlyParticleCanvas.Children.Add(ellipse);
                _flyParticles.Add(p);
            }
        }

        private void InitCubes()
        {
            for (int i = 0; i < 4; i++)
            {
                var cube = new OrbitCube
                {
                    Size = 14 + _rand.Next(0, 8),
                    Radius = 100 + _rand.NextDouble() * 20,
                    Angle = _rand.NextDouble() * Math.PI * 2,
                    AngularSpeed = 0.8 + _rand.NextDouble() * 0.3,
                    Delay = i * 0.1,
                    Layer = 0,
                    RotSpeed = (_rand.NextDouble() - 0.5) * 4
                };
                cube.Element = CreateCube(cube.Size, true);
                cube.ScaleTransform = new ScaleTransform(1, 1, cube.Size / 2, cube.Size / 2);
                cube.RotateTransform = new RotateTransform(0, cube.Size / 2, cube.Size / 2);
                var group = new TransformGroup();
                group.Children.Add(cube.ScaleTransform);
                group.Children.Add(cube.RotateTransform);
                cube.Element.RenderTransform = group;
                CubeCanvas.Children.Add(cube.Element);
                _cubes.Add(cube);
            }
            for (int i = 0; i < 4; i++)
            {
                var cube = new OrbitCube
                {
                    Size = 22 + _rand.Next(0, 10),
                    Radius = 150 + _rand.NextDouble() * 30,
                    Angle = _rand.NextDouble() * Math.PI * 2,
                    AngularSpeed = 0.5 + _rand.NextDouble() * 0.2,
                    Delay = 0.3 + i * 0.15,
                    Layer = 1,
                    RotSpeed = (_rand.NextDouble() - 0.5) * 3
                };
                cube.Element = CreateCube(cube.Size, true);
                cube.ScaleTransform = new ScaleTransform(1, 1, cube.Size / 2, cube.Size / 2);
                cube.RotateTransform = new RotateTransform(0, cube.Size / 2, cube.Size / 2);
                var group = new TransformGroup();
                group.Children.Add(cube.ScaleTransform);
                group.Children.Add(cube.RotateTransform);
                cube.Element.RenderTransform = group;
                CubeCanvas.Children.Add(cube.Element);
                _cubes.Add(cube);
            }
            for (int i = 0; i < 3; i++)
            {
                var cube = new OrbitCube
                {
                    Size = 35 + _rand.Next(0, 15),
                    Radius = 200 + _rand.NextDouble() * 40,
                    Angle = _rand.NextDouble() * Math.PI * 2,
                    AngularSpeed = 0.3 + _rand.NextDouble() * 0.15,
                    Delay = 0.6 + i * 0.2,
                    Layer = 2,
                    RotSpeed = (_rand.NextDouble() - 0.5) * 2
                };
                cube.Element = CreateCube(cube.Size, false);
                cube.ScaleTransform = new ScaleTransform(1, 1, cube.Size / 2, cube.Size / 2);
                cube.RotateTransform = new RotateTransform(0, cube.Size / 2, cube.Size / 2);
                var group = new TransformGroup();
                group.Children.Add(cube.ScaleTransform);
                group.Children.Add(cube.RotateTransform);
                cube.Element.RenderTransform = group;
                CubeCanvas.Children.Add(cube.Element);
                _cubes.Add(cube);
            }
        }

        private void InitCoreParticles()
        {
            // 最初版没有核心粒子，留空
        }

        private void InitFragments()
        {
            // 预创建碎片对象，形状不规则，数量更多
            for (int i = 0; i < 70; i++)
            {
                double angle = _rand.NextDouble() * Math.PI * 2;
                double speed = 3 + _rand.NextDouble() * 15;
                var f = new Fragment
                {
                    X = CenterX,
                    Y = CenterY,
                    VX = Math.Cos(angle) * speed,
                    VY = Math.Sin(angle) * speed,
                    Size = _rand.Next(5, 35),
                    RotSpeed = (_rand.NextDouble() - 0.5) * 20,
                    Rotation = 0,
                    Opacity = 0,
                    Delay = _rand.NextDouble() * 0.1
                };

                // 随机选择碎片形状
                int shapeType = _rand.Next(4);
                UIElement shape;
                switch (shapeType)
                {
                    case 0: // 正方形
                        shape = new Border
                        {
                            Width = f.Size,
                            Height = f.Size,
                            CornerRadius = new CornerRadius(2),
                            Background = new SolidColorBrush(Color.FromArgb(0x80, 0x4F, 0xC3, 0xF7)),
                            BorderBrush = new SolidColorBrush(Color.FromArgb(0x90, 0xB0, 0xE8, 0xFF)),
                            BorderThickness = new Thickness(1),
                            Opacity = 0
                        };
                        break;
                    case 1: // 三角形
                        var triPath = new Path
                        {
                            Opacity = 0,
                            Fill = new SolidColorBrush(Color.FromArgb(0x80, 0x4F, 0xC3, 0xF7)),
                            Stroke = new SolidColorBrush(Color.FromArgb(0x90, 0xB0, 0xE8, 0xFF)),
                            StrokeThickness = 1
                        };
                        var triGeo = new PathGeometry();
                        var triFig = new PathFigure { IsClosed = true };
                        triFig.Segments.Add(new LineSegment(new Point(f.Size/2, 0), true));
                        triFig.Segments.Add(new LineSegment(new Point(f.Size, f.Size), true));
                        triFig.Segments.Add(new LineSegment(new Point(0, f.Size), true));
                        triGeo.Figures.Add(triFig);
                        triPath.Data = triGeo;
                        shape = triPath;
                        break;
                    case 2: // 菱形
                        var diamondPath = new Path
                        {
                            Opacity = 0,
                            Fill = new SolidColorBrush(Color.FromArgb(0x80, 0x4F, 0xC3, 0xF7)),
                            Stroke = new SolidColorBrush(Color.FromArgb(0x90, 0xB0, 0xE8, 0xFF)),
                            StrokeThickness = 1
                        };
                        var diamondGeo = new PathGeometry();
                        var diamondFig = new PathFigure { IsClosed = true };
                        diamondFig.Segments.Add(new LineSegment(new Point(f.Size/2, 0), true));
                        diamondFig.Segments.Add(new LineSegment(new Point(f.Size, f.Size/2), true));
                        diamondFig.Segments.Add(new LineSegment(new Point(f.Size/2, f.Size), true));
                        diamondFig.Segments.Add(new LineSegment(new Point(0, f.Size/2), true));
                        diamondGeo.Figures.Add(diamondFig);
                        diamondPath.Data = diamondGeo;
                        shape = diamondPath;
                        break;
                    default: // 长条
                        shape = new Border
                        {
                            Width = f.Size * 1.5,
                            Height = f.Size * 0.5,
                            CornerRadius = new CornerRadius(1),
                            Background = new SolidColorBrush(Color.FromArgb(0x80, 0x4F, 0xC3, 0xF7)),
                            BorderBrush = new SolidColorBrush(Color.FromArgb(0x90, 0xB0, 0xE8, 0xFF)),
                            BorderThickness = new Thickness(1),
                            Opacity = 0
                        };
                        break;
                }

                f.RotateTransform = new RotateTransform(0, f.Size / 2, f.Size / 2);
                shape.RenderTransform = f.RotateTransform;
                f.Element = shape;
                FragmentCanvas.Children.Add(shape);
                _fragments.Add(f);
            }
        }

        private Border CreateCube(double size, bool bright)
        {
            var border = new Border
            {
                Width = size,
                Height = size,
                Opacity = 0,
                BorderBrush = new SolidColorBrush(Color.FromArgb((byte)(bright ? 0x70 : 0x40), 0x80, 0xDF, 0xFF)),
                BorderThickness = new Thickness(1.5),
                Background = new SolidColorBrush(Color.FromArgb((byte)(bright ? 0x15 : 0x08), 0x4F, 0xC3, 0xF7)),
                CornerRadius = new CornerRadius(2)
            };
            var grid = new Grid();
            var line1 = new Rectangle { Height = 1, Fill = new SolidColorBrush(Color.FromArgb(0x20, 0x80, 0xDF, 0xFF)), VerticalAlignment = VerticalAlignment.Center };
            var line2 = new Rectangle { Width = 1, Fill = new SolidColorBrush(Color.FromArgb(0x20, 0x80, 0xDF, 0xFF)), HorizontalAlignment = HorizontalAlignment.Center };
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
                StartReconstruction();
                return;
            }

            double t = _totalSeconds;

            UpdateBgParticles();
            UpdateCoreLight(t);
            UpdateFlyParticles(t);
            UpdateCrystalCore(t);
            UpdateCubes(t);
            UpdateLoading(t);
        }

        private void StartReconstruction()
        {
            if (_reconstructStarted) return;
            _reconstructStarted = true;
            _playing = false;

            // 使用预创建的碎片，只显示和设置初始位置
            foreach (var f in _fragments)
            {
                f.X = CenterX;
                f.Y = CenterY;
                f.Opacity = 1;
                f.Element.Opacity = 1;
                f.Rotation = 0;
                f.RotateTransform.Angle = 0;
                Canvas.SetLeft(f.Element, f.X - f.Size / 2);
                Canvas.SetTop(f.Element, f.Y - f.Size / 2);
            }

            CrystalCore.BeginAnimation(OpacityProperty, 
                new DoubleAnimation(0, TimeSpan.FromSeconds(0.3)));
            CubeCanvas.BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, TimeSpan.FromSeconds(0.3)));
            LoadingPanel.BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, TimeSpan.FromSeconds(0.2)));

            // 不要取消Rendering，继续用它驱动碎片动画
            _reconstructTimer.Start();

            Dispatcher.BeginInvoke(new Action(async () =>
            {
                // 碎片还在飞的时候，就开始ReconSeed，不要停滞
                await System.Threading.Tasks.Task.Delay(400); // 提前开始，碎片还在飞
                StartReconSeed();
                await System.Threading.Tasks.Task.Delay(800); // 碎片和ReconSeed同时进行
                _reconstructTimer.Stop();
                var license = new LicenseWindow();
                Application.Current.MainWindow = license;
                license.Show();
                Close();
            }));
        }

        private void StartReconSeed()
        {
            if (_seedStarted) return;
            _seedStarted = true;

            var appearAnim = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            ReconSeed.BeginAnimation(OpacityProperty, appearAnim);

            var scaleXAnim = new DoubleAnimation(0, 3, TimeSpan.FromSeconds(1.2))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            var scaleYAnim = new DoubleAnimation(0, 3, TimeSpan.FromSeconds(1.2))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            SeedScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnim);
            SeedScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnim);
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
            if (t < 0.5) { CoreLight.Opacity = 0; return; }

            double appearT = Math.Clamp((t - 0.5) / 1.5, 0, 1);
            double ease = 1 - Math.Pow(1 - appearT, 3);

            _breathPhase += 0.015;
            double breath = 1 + Math.Sin(_breathPhase) * 0.02;

            double baseSize = 30 + ease * 25;
            double size = baseSize * breath;

            CoreLight.Opacity = ease * 0.85;
            CoreLightScale.ScaleX = size / 20;
            CoreLightScale.ScaleY = size / 20;
            Canvas.SetLeft(CoreLight, CenterX - size / 2);
            Canvas.SetTop(CoreLight, CenterY - size / 2);

            if (t > 4.5)
            {
                double fadeT = Math.Clamp((t - 4.5) / 0.8, 0, 1);
                CoreLight.Opacity = ease * 0.85 * (1 - fadeT);
            }
        }

        private void UpdateFlyParticles(double t)
        {
            double startTime = 1.0;
            foreach (var p in _flyParticles)
            {
                double localT = t - startTime - p.Delay;
                if (localT < 0) { p.Element.Opacity = 0; continue; }

                p.Progress = Math.Clamp(localT / p.Duration, 0, 1);
                double ease = p.Progress * p.Progress;

                double dx = CenterX - p.StartX;
                double dy = CenterY - p.StartY;
                double distance = Math.Sqrt(dx * dx + dy * dy);
                
                double bend = Math.Sin(ease * Math.PI) * 0.15;
                double perpX = -dy / distance * bend * 20;
                double perpY = dx / distance * bend * 20;

                double x = p.StartX + dx * ease + perpX;
                double y = p.StartY + dy * ease + perpY;

                double opacity = 1;
                if (p.Progress > 0.6)
                {
                    double absorb = (p.Progress - 0.6) / 0.4;
                    opacity = Math.Sin(absorb * Math.PI);
                }

                p.Element.Opacity = opacity * 0.9;
                Canvas.SetLeft(p.Element, x - p.Size / 2);
                Canvas.SetTop(p.Element, y - p.Size / 2);
            }
        }

        private void UpdateCrystalCore(double t)
        {
            if (t < 4.5) { CrystalCore.Opacity = 0; return; }

            double formT = Math.Clamp((t - 4.5) / 1.5, 0, 1);
            double ease = 1 - Math.Pow(1 - formT, 3);

            CrystalCore.Opacity = ease;
            CrystalScale.ScaleX = ease;
            CrystalScale.ScaleY = ease;

            // 模糊从大到小（伪3D效果）
            CrystalBlur.Radius = 20 * (1 - ease);

            // 持续缓慢旋转（45度基础 + 缓慢增加）
            CrystalRotate.Angle = 45 + (t - 4.5) * 8;

            // 内部光流移动
            double flow1 = (Math.Sin(t * 1.5) + 1) / 2;
            CrystalFlow1.Margin = new Thickness(0, 20 + flow1 * 60, 0, 0);
            double flow2 = (Math.Cos(t * 1.2) + 1) / 2;
            CrystalFlow2.Margin = new Thickness(0, 30 + flow2 * 50, 0, 0);

            // 晶体位置居中
            Canvas.SetLeft(CrystalCore, CenterX - 70);
            Canvas.SetTop(CrystalCore, CenterY - 70);
        }

        private void UpdateCubes(double t)
        {
            double startTime = 6.5;
            foreach (var c in _cubes)
            {
                double localT = t - startTime - c.Delay;
                if (localT < 0) { c.Element.Opacity = 0; continue; }

                double spreadT = Math.Clamp(localT / 1.2, 0, 1);
                double spreadEase = 1 - Math.Pow(1 - spreadT, 3);

                c.Angle += c.AngularSpeed * 0.016;

                double currentRadius = c.Radius * spreadEase;
                double x = CenterX + Math.Cos(c.Angle) * currentRadius;
                double y = CenterY + Math.Sin(c.Angle) * currentRadius * 0.55;

                double depthScale = 0.7 + (y - CenterY + 100) / 200 * 0.5;
                depthScale = Math.Clamp(depthScale, 0.5, 1.2);

                double layerOpacity = c.Layer == 0 ? 0.7 : c.Layer == 1 ? 0.5 : 0.35;

                c.Element.Opacity = spreadEase * layerOpacity;
                c.ScaleTransform.ScaleX = depthScale;
                c.ScaleTransform.ScaleY = depthScale;
                c.RotateTransform.Angle = c.RotSpeed * t % 360;
                
                Canvas.SetLeft(c.Element, x - c.Size / 2);
                Canvas.SetTop(c.Element, y - c.Size / 2);
                Canvas.SetZIndex(c.Element, (int)y);
            }
        }

        private void UpdateLoading(double t)
        {
            if (t < 8.0) { LoadingPanel.Opacity = 0; return; }

            double appearT = Math.Clamp((t - 8.0) / 0.5, 0, 1);
            LoadingPanel.Opacity = appearT;

            double loadT = Math.Clamp((t - 8.0) / 3.0, 0, 1);
            double ease = 1 - Math.Pow(1 - loadT, 2);

            EnergyBar.Width = 300 * ease;

            if (loadT > 0.85)
            {
                double accel = (loadT - 0.85) / 0.15;
                _energyBarBlur.Radius = 3 + accel * 4;
            }
            else
            {
                _energyBarBlur.Radius = 3;
            }

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
                var brush = (SolidColorBrush)dot.Fill;
                byte r = (byte)(0x33 + (0x80 - 0x33) * dotT);
                byte g = (byte)(0x33 + (0xDF - 0x33) * dotT);
                byte b = (byte)(0x55 + (0xFF - 0x55) * dotT);
                brush.Color = Color.FromRgb(r, g, b);
                
                double size = 8 + dotT * 4;
                dot.Width = size;
                dot.Height = size;

                double orbitAngle = loadT * 2;
                double ox = Math.Cos(orbitAngle) * 2;
                double oy = Math.Sin(orbitAngle) * 2;
                Canvas.SetLeft(dot, 4 + ox);
                Canvas.SetTop(dot, 4 + oy);

                if (loadT >= threshold && loadT < threshold + 0.15)
                {
                    StatusText.Text = status + " ... OK";
                }
            }
        }

        #endregion
    }

    public class BgParticle
    {
        public double X, Y, Size, SpeedX, SpeedY, Opacity;
        public bool IsPurple;
        public Ellipse Element = null!;
    }

    public class FlyParticle
    {
        public double StartX, StartY, Delay, Duration, Size, Progress;
        public bool IsPurple;
        public Ellipse Element = null!;

        public void Reset() { Progress = 0; Element.Opacity = 0; }
    }

    public class OrbitCube
    {
        public double Size, Radius, Angle, AngularSpeed, Delay, RotSpeed;
        public int Layer;
        public Border Element = null!;
        public ScaleTransform ScaleTransform = null!;
        public RotateTransform RotateTransform = null!;

        public void Reset()
        {
            Angle = new Random().NextDouble() * Math.PI * 2;
            Element.Opacity = 0;
        }
    }

    public class CoreParticle
    {
        public double Angle, Radius, Speed, Size, YOffset;
        public Ellipse Element = null!;

        public void Reset() { Element.Opacity = 0; }
    }

    public class Fragment
    {
        public double X, Y, VX, VY, Size, RotSpeed, Rotation, Opacity, Delay;
        public UIElement Element = null!;
        public RotateTransform RotateTransform = null!;
    }
}

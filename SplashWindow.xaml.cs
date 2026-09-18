using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace 美术测试
{
    public partial class SplashWindow : Window
    {
        private Stopwatch _sw = new Stopwatch();
        private double _totalSeconds = 0;
        private const double AnimationDuration = 11.5; // 动画总时长
        private bool _playing = true;

        // 中心坐标
        private const double CenterX = 400;
        private const double CenterY = 300;

        // 背景粒子
        private List<BgParticle> _bgParticles = new List<BgParticle>();
        // 飞行粒子
        private List<FlyParticle> _flyParticles = new List<FlyParticle>();
        // 立方体
        private List<OrbitCube> _cubes = new List<OrbitCube>();

        // 随机数
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
            // 重置所有元素
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
            // 重置粒子
            foreach (var p in _flyParticles) p.Reset();
            foreach (var c in _cubes) c.Reset();
        }

        #region 初始化

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
                // 从屏幕边缘随机位置出发
                double angle = _rand.NextDouble() * Math.PI * 2;
                double dist = 400 + _rand.NextDouble() * 100;
                var p = new FlyParticle
                {
                    StartX = CenterX + Math.Cos(angle) * dist,
                    StartY = CenterY + Math.Sin(angle) * dist,
                    Delay = _rand.NextDouble() * 0.8, // 错峰出发
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
            // 6个立方体：2大3中1小
            double[] sizes = { 45, 40, 28, 25, 22, 16 };
            double[] radii = { 160, 140, 120, 100, 130, 90 };
            double[] speeds = { 0.3, 0.35, 0.5, 0.6, 0.45, 0.7 }; // 角速度
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
            // 内部十字线增强立方体感
            var grid = new Grid();
            var line1 = new Rectangle { Height = 1, Fill = new SolidColorBrush(Color.FromArgb(0x30, 0x80, 0xDF, 0xFF)), VerticalAlignment = VerticalAlignment.Center };
            var line2 = new Rectangle { Width = 1, Fill = new SolidColorBrush(Color.FromArgb(0x30, 0x80, 0xDF, 0xFF)), HorizontalAlignment = HorizontalAlignment.Center };
            grid.Children.Add(line1);
            grid.Children.Add(line2);
            border.Child = grid;
            return border;
        }

        #endregion

        #region 帧更新

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            if (!_playing) return;

            _totalSeconds = _sw.Elapsed.TotalSeconds;

            // 循环播放
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
                // 边界循环
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
            // 1s开始出现，2.5s达到最大
            if (t < 1.0)
            {
                CoreLight.Opacity = 0;
                return;
            }

            double appearT = Math.Clamp((t - 1.0) / 1.5, 0, 1);
            // 缓动
            double ease = 1 - Math.Pow(1 - appearT, 3);

            double baseSize = 40 + ease * 30;
            // 呼吸脉动
            double pulse = 1 + Math.Sin(t * 3) * 0.1;
            double size = baseSize * pulse;

            CoreLight.Opacity = ease * 0.9;
            CoreLightScale.ScaleX = size / 20;
            CoreLightScale.ScaleY = size / 20;
            Canvas.SetLeft(CoreLight, CenterX - size / 2);
            Canvas.SetTop(CoreLight, CenterY - size / 2);

            // 晶体出现后光点淡出
            if (t > 4.5)
            {
                double fadeT = Math.Clamp((t - 4.5) / 1.0, 0, 1);
                CoreLight.Opacity = ease * 0.9 * (1 - fadeT);
            }
        }

        private void UpdateFlyParticles(double t)
        {
            // 2.5s开始飞，4.5s全部到达
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
                // 缓入
                double ease = p.Progress * p.Progress;

                double x = p.StartX + (CenterX - p.StartX) * ease;
                double y = p.StartY + (CenterY - p.StartY) * ease;

                // 到达后淡出
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
            // 4.5s开始成型，6s完全成型
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

            // 模糊从大到小
            CrystalBlur.Radius = 20 * (1 - ease);

            // 持续缓慢旋转（45度基础 + 缓慢增加）
            CrystalRotate.Angle = 45 + (t - 4.5) * 8;

            // 内部光流移动
            double flow1 = (Math.Sin(t * 1.5) + 1) / 2;
            CrystalFlow1.Margin = new Thickness(0, 20 + flow1 * 60, 0, 0);
            double flow2 = (Math.Cos(t * 1.2) + 1) / 2;
            CrystalFlow2.Margin = new Thickness(0, 30 + flow2 * 50, 0, 0);

            // 晶体位置居中
            Canvas.SetLeft(Crystal, CenterX - 70);
            Canvas.SetTop(Crystal, CenterY - 70);
        }

        private void UpdateCubes(double t)
        {
            // 6s开始扩散，8s全部到位
            double startTime = 6.0;
            foreach (var c in _cubes)
            {
                double localT = t - startTime - c.Delay;
                if (localT < 0)
                {
                    c.Element.Opacity = 0;
                    continue;
                }

                // 扩散阶段：从中心到轨道
                double spreadT = Math.Clamp(localT / 1.0, 0, 1);
                double spreadEase = 1 - Math.Pow(1 - spreadT, 3);

                // 持续旋转
                c.Angle += c.AngularSpeed * 0.016;

                double currentRadius = c.Radius * spreadEase;
                double x = CenterX + Math.Cos(c.Angle) * currentRadius;
                double y = CenterY + Math.Sin(c.Angle) * currentRadius * 0.6; // 椭圆轨道，增加空间感

                // 远近缩放：y越大（越靠下）显得越大
                double depthScale = 0.7 + (y - CenterY + 100) / 200 * 0.6;
                depthScale = Math.Clamp(depthScale, 0.5, 1.3);

                c.Element.Opacity = spreadEase * 0.7;
                c.Element.RenderTransform = new ScaleTransform(depthScale, depthScale, c.Size / 2, c.Size / 2);
                Canvas.SetLeft(c.Element, x - c.Size / 2);
                Canvas.SetTop(c.Element, y - c.Size / 2);

                // 遮挡排序：y大的在前面
                Canvas.SetZIndex(c.Element, (int)y);
            }
        }

        private void UpdateLoading(double t)
        {
            // 8s开始出现，10.5s加载完成
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

            // 4个点在25/50/75/100%时点亮
            UpdateDot(Dot1, loadT, 0.15, "CAMERA ENGINE");
            UpdateDot(Dot2, loadT, 0.40, "MODEL RUNTIME");
            UpdateDot(Dot3, loadT, 0.65, "GPU CORE");
            UpdateDot(Dot4, loadT, 0.90, "NEURAL SYSTEM");
        }

        private void UpdateDot(Ellipse dot, double loadT, double threshold, string status)
        {
            if (loadT >= threshold)
            {
                // 点亮动画
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
            // 10.5s开始淡出，11.5s完全淡出
            if (t < 10.5) return;

            double fadeT = Math.Clamp((t - 10.5) / 1.0, 0, 1);
            double opacity = 1 - fadeT;

            MainCanvas.Opacity = opacity;
        }

        #endregion
    }

    #region 辅助类

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

    #endregion
}

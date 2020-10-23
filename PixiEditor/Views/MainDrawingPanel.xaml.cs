﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PixiEditor.Models.Tools.Tools;
using PixiEditor.ViewModels;
using Xceed.Wpf.Toolkit.Core.Input;
using Xceed.Wpf.Toolkit.Zoombox;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for MainDrawingPanel.xaml.
    /// </summary>
    public partial class MainDrawingPanel : UserControl
    {
        // Using a DependencyProperty as the backing store for Center.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register("Center", typeof(bool), typeof(MainDrawingPanel), new PropertyMetadata(true, OnCenterChanged));

        // Using a DependencyProperty as the backing store for MouseX.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseXProperty =
            DependencyProperty.Register("MouseX", typeof(double), typeof(MainDrawingPanel), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for MouseX.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseYProperty =
            DependencyProperty.Register("MouseY", typeof(double), typeof(MainDrawingPanel), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for MouseMoveCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseMoveCommandProperty =
            DependencyProperty.Register("MouseMoveCommand", typeof(ICommand), typeof(MainDrawingPanel), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for CenterOnStart.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CenterOnStartProperty =
            DependencyProperty.Register("CenterOnStart", typeof(bool), typeof(MainDrawingPanel), new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for Item.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(object), typeof(MainDrawingPanel), new PropertyMetadata(default(FrameworkElement)));

        // Using a DependencyProperty as the backing store for Item.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsUsingZoomToolProperty =
            DependencyProperty.Register("IsUsingZoomTool", typeof(bool), typeof(MainDrawingPanel), new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for ZoomPercentage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomPercentageProperty =
            DependencyProperty.Register("ZoomPercentage", typeof(double), typeof(MainDrawingPanel), new PropertyMetadata(0.0, ZoomPercentegeChanged));

        // Using a DependencyProperty as the backing store for ViewportPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewportPositionProperty =
            DependencyProperty.Register("ViewportPosition", typeof(Point), typeof(MainDrawingPanel), new PropertyMetadata(default(Point), ViewportPosCallback));

        // Using a DependencyProperty as the backing store for MiddleMouseClickedCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MiddleMouseClickedCommandProperty =
            DependencyProperty.Register("MiddleMouseClickedCommand", typeof(ICommand), typeof(MainDrawingPanel), new PropertyMetadata(default(ICommand)));

        // Using a DependencyProperty as the backing store for MiddleMouseClickedCommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MiddleMouseClickedCommandParameterProperty =
            DependencyProperty.Register("MiddleMouseClickedCommandParameter", typeof(object), typeof(MainDrawingPanel), new PropertyMetadata(default(object)));

        public MainDrawingPanel()
        {
            InitializeComponent();
            Zoombox.ZoomToSelectionModifiers = new KeyModifierCollection() { KeyModifier.RightAlt };
        }

        public double ClickScale { get; set; }

        public Point ClickPosition { get; set; }

        public double ZoomPercentage
        {
            get { return (double)GetValue(ZoomPercentageProperty); }
            set { SetValue(ZoomPercentageProperty, value); }
        }

        public Point ViewportPosition
        {
            get { return (Point)GetValue(ViewportPositionProperty); }
            set { SetValue(ViewportPositionProperty, value); }
        }

        public bool Center
        {
            get => (bool)GetValue(CenterProperty);
            set => SetValue(CenterProperty, value);
        }

        public double MouseX
        {
            get => (double)GetValue(MouseXProperty);
            set => SetValue(MouseXProperty, value);
        }

        public double MouseY
        {
            get => (double)GetValue(MouseYProperty);
            set => SetValue(MouseYProperty, value);
        }

        public ICommand MouseMoveCommand
        {
            get => (ICommand)GetValue(MouseMoveCommandProperty);
            set => SetValue(MouseMoveCommandProperty, value);
        }

        public bool CenterOnStart
        {
            get => (bool)GetValue(CenterOnStartProperty);
            set => SetValue(CenterOnStartProperty, value);
        }

        public object Item
        {
            get => GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        public bool IsUsingZoomTool
        {
            get => (bool)GetValue(IsUsingZoomToolProperty);
            set => SetValue(IsUsingZoomToolProperty, value);
        }

        public ICommand MiddleMouseClickedCommand
        {
            get { return (ICommand)GetValue(MiddleMouseClickedCommandProperty); }
            set { SetValue(MiddleMouseClickedCommandProperty, value); }
        }

        public object MiddleMouseClickedCommandParameter
        {
            get { return (object)GetValue(MiddleMouseClickedCommandParameterProperty); }
            set { SetValue(MiddleMouseClickedCommandParameterProperty, value); }
        }

        private static void ZoomPercentegeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainDrawingPanel panel = (MainDrawingPanel)d;
            double percentage = (double)e.NewValue;
            if (percentage == 100)
            {
                panel.SetClickValues();
            }

            panel.Zoombox.ZoomTo(panel.ClickScale * ((double)e.NewValue / 100.0));
        }

        private static void ViewportPosCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainDrawingPanel panel = (MainDrawingPanel)d;
            if (PresentationSource.FromVisual(panel.Zoombox) == null)
            {
                panel.Zoombox.Position = default;
                return;
            }

            TranslateZoombox(panel, (Point)e.NewValue);
        }

        private static void TranslateZoombox(MainDrawingPanel panel, Point vector)
        {
            var newPos = new Point(
                panel.ClickPosition.X + vector.X,
                panel.ClickPosition.Y + vector.Y);
            panel.Zoombox.Position = newPos;
        }

        private static void OnCenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainDrawingPanel panel = (MainDrawingPanel)d;
            panel.Zoombox.CenterContent();
        }

        private void Zoombox_CurrentViewChanged(object sender, ZoomboxViewChangedEventArgs e)
        {
            Zoombox.MinScale = 32 / ((FrameworkElement)Item).Width;
            Zoombox.KeepContentInBounds = !(Zoombox.Scale > Zoombox.MinScale * 35.0);
        }

        private void Zoombox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ZoomPercentage == 100)
            {
                SetClickValues();
            }
        }

        private void SetClickValues()
        {
            if (!IsUsingZoomTool)
            {
                return;
            }

            ClickScale = Zoombox.Scale;
            SetZoomOrigin();
        }

        private void SetZoomOrigin()
        {
            var item = (FrameworkElement)Item;
            if (item == null)
            {
                return;
            }

            var mousePos = Mouse.GetPosition(item);
            Zoombox.ZoomOrigin = new Point(Math.Clamp(mousePos.X / item.Width, 0, 1), Math.Clamp(mousePos.Y / item.Height, 0, 1));
        }

        private void Zoombox_Loaded(object sender, RoutedEventArgs e)
        {
            if (CenterOnStart)
            {
                ((Zoombox)sender).CenterContent();
            }

            ClickScale = Zoombox.Scale;
        }

        private void Zoombox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            SetZoomOrigin();
        }

        private void MainDrawingPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsUsingZoomTool = ViewModelMain.Current.BitmapManager.SelectedTool is ZoomTool;
            Mouse.Capture((IInputElement)sender, CaptureMode.SubTree);
            ClickPosition = ((FrameworkElement)Item).TranslatePoint(new Point(0, 0), Zoombox);
            SetClickValues();
        }

        private void MainDrawingPanel_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ((IInputElement)sender).ReleaseMouseCapture();
        }

        private void Zoombox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed &&
                MiddleMouseClickedCommand.CanExecute(MiddleMouseClickedCommandParameter))
            {
                MiddleMouseClickedCommand.Execute(MiddleMouseClickedCommandParameter);
            }
        }
    }
}
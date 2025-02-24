﻿using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Tweaks;
using GTweak.Windows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace GTweak
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            BtnNotification.StateNA = SettingsRepository.IsViewNotification;
            BtnUpdate.StateNA = SettingsRepository.IsСheckingUpdate;
            BtnTopMost.StateNA = Topmost = SettingsRepository.IsTopMost;
            BtnSoundNtn.IsChecked = SettingsRepository.IsPlayingSound;
            SliderVolume.Value = SettingsRepository.Volume;
            LanguageSelectionMenu.SelectedIndex = SettingsRepository.Language switch
            {
                "en" => 0,
                "ko" => 1,
                _ => 2,
            };
            ThemeSelectionMenu.SelectedIndex = SettingsRepository.Theme switch
            {
                "Dark" => 0,
                "Light" => 1,
                _ => 2,
            };

            App.ImportTweaksUpdate += delegate { BtnMore.IsChecked = true; };
            App.ThemeChanged += delegate { this.Close(); new RebootWindow().ShowDialog(); };
        }

        #region Button Title/Animation Window
        private void SettingsMenuAnimation()
        {
            Dispatcher.Invoke(() =>
            {
                Storyboard storyboard = new Storyboard();

                DoubleAnimation rotateAnimation = new DoubleAnimation()
                {
                    From = 0.0,
                    To = 360,
                    EasingFunction = new QuadraticEase(),
                    SpeedRatio = 2.0,
                    Duration = TimeSpan.FromSeconds(1)
                };

                DoubleAnimation widthAnimation = new DoubleAnimation()
                {
                    From = SettingsMenu.Width != 400 ? 0 : 400,
                    To = SettingsMenu.Width != 400 ? 400 : 0,
                    EasingFunction = new QuadraticEase(),
                    SpeedRatio = 2.0,
                    Duration = TimeSpan.FromSeconds(1)
                };

                Timeline.SetDesiredFrameRate(rotateAnimation, 400);
                Timeline.SetDesiredFrameRate(widthAnimation, 400);

                Storyboard.SetTarget(rotateAnimation, ImageSettings);
                Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));

                storyboard.Children.Add(rotateAnimation);
                storyboard.Begin();
                SettingsMenu.BeginAnimation(WidthProperty, widthAnimation);
            });
        }

        private void SettingsMenu_QueryCursor(object sender, QueryCursorEventArgs e)
        {
            if (SettingsMenu.Width == 400)
                SettingsMenuAnimation();
        }

        private void ButtonSettings_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SettingsMenu.Width == 0 || SettingsMenu.Width == 400)
                SettingsMenuAnimation();
        }

        private void ButtonMinimized_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => this.WindowState = WindowState.Minimized;

        private void ButtonExit_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            switch (SystemTweaks.isTweakWorkingAntivirus)
            {
                case false:
                    this.Close();
                    break;
                case true:
                    new ViewNotification().Show("", "warn", (string)FindResource("windefclose_notification"));
                    break;
            }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Closing -= Window_Closing;
            e.Cancel = true;
            DoubleAnimation doubleAnim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.1));
            doubleAnim.Completed += delegate { this.Close(); };
            Timeline.SetDesiredFrameRate(doubleAnim, 400);
            BeginAnimation(OpacityProperty, doubleAnim);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation doubleAnim = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase()
            };
            doubleAnim.Completed += async delegate
            {
                if (QueryUpdates.IsNeedUpdate && SettingsRepository.IsСheckingUpdate)
                {
                    await Task.Delay(500);
                    new UpdateWindow().ShowDialog();
                }
            };
            Timeline.SetDesiredFrameRate(doubleAnim, 400);
            BeginAnimation(OpacityProperty, doubleAnim);
            new TypewriterAnimation(UtilityTitle.Text, UtilityTitle, TimeSpan.FromSeconds(0.4));
        }
        #endregion

        #region Settings Menu
        private void BtnNotification_ChangedState(object sender, EventArgs e) => SettingsRepository.ChangingParameters(!BtnNotification.State, "Notification");

        private void BtnUpdate_ChangedState(object sender, EventArgs e) => SettingsRepository.ChangingParameters(!BtnUpdate.State, "Update");

        private void BtnTopMost_ChangedState(object sender, EventArgs e)
        {
            SettingsRepository.ChangingParameters(!BtnTopMost.State, "TopMost");
            Topmost = !BtnTopMost.State;
        }

        private void BtnSoundNtn_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => SettingsRepository.ChangingParameters(!BtnSoundNtn.IsChecked, "Sound");

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderVolume.Value = SliderVolume.Value == 0 ? 1 : SliderVolume.Value;
            SettingsRepository.ChangingParameters(SliderVolume.Value, "Volume");
            SettingsRepository.waveOutSetVolume(IntPtr.Zero, ((uint)(double)(ushort.MaxValue / 100 * SliderVolume.Value) & 0x0000ffff) | ((uint)(double)(ushort.MaxValue / 100 * SliderVolume.Value) << 16));
        }

        private void LanguageSelectionMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (LanguageSelectionMenu.SelectedIndex)
            {
                case 0:
                    SettingsRepository.ChangingParameters("en", "Language");
                    App.Language = "en";
                    break;
                case 1:
                    SettingsRepository.ChangingParameters("ko", "Language");
                    App.Language = "ko";
                    break;
                default:
                    SettingsRepository.ChangingParameters("ru", "Language");
                    App.Language = "ru";
                    break;
            }
        }

        private void ThemeSelectionMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (ThemeSelectionMenu.SelectedIndex)
            {
                case 0:
                    SettingsRepository.ChangingParameters("Dark", "Theme");
                    App.Theme = "Dark";
                    break;
                case 1:
                    SettingsRepository.ChangingParameters("Light", "Theme");
                    App.Theme = "Light";
                    break;
                default:
                    SettingsRepository.ChangingParameters("System", "Theme");
                    App.Theme = "System";
                    break;
            }
        }

        private void BtnExport_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => Parallel.Invoke(SettingsRepository.SaveFileConfig);

        private void BtnImport_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => Parallel.Invoke(SettingsRepository.OpenFileConfig);

        private void BtnDelete_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => SettingsRepository.SelfRemoval();

        private void BtnContats_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image btnConcat = (Image)sender;
            Process.Start(btnConcat.Uid switch
            {
                "git" => "https://github.com/Greedeks",
                "tg" => "https://t.me/Greedeks",
                _ => "https://steamcommunity.com/id/greedeks/",
            });
        }
        #endregion
    }
}


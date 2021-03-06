﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Helpers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Processes;
using PixiEditor.Models.UserPreferences;
using PixiEditor.UpdateModule;
using PixiEditor.ViewModels;

namespace PixiEditor
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ViewModelMain viewModel;

        public MainWindow()
        {
            InitializeComponent();

            IServiceCollection services = new ServiceCollection()
                .AddSingleton<IPreferences>(new PreferencesSettings());

            DataContext = new ViewModelMain(services.BuildServiceProvider());

            StateChanged += MainWindowStateChangeRaised;
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            viewModel = (ViewModelMain)DataContext;
            viewModel.CloseAction = Close;
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void MainWindowStateChangeRaised(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                RestoreButton.Visibility = Visibility.Visible;
                MaximizeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                RestoreButton.Visibility = Visibility.Collapsed;
                MaximizeButton.Visibility = Visibility.Visible;
            }
        }

        private void MainWindow_Initialized(object sender, EventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => Helpers.CrashHelper.SaveCrashInfo((Exception)e.ExceptionObject);
#if RELEASE
            CheckForDownloadedUpdates();
#endif
        }

        private void CheckForDownloadedUpdates()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            UpdateDownloader.CreateTempDirectory();
            bool updateZipExists = Directory.GetFiles(UpdateDownloader.DownloadLocation, "update-*.zip").Length > 0;
            string[] updateExeFiles = Directory.GetFiles(UpdateDownloader.DownloadLocation, "update-*.exe");
            bool updateExeExists = updateExeFiles.Length > 0;

            string updaterPath = Path.Join(dir, "PixiEditor.UpdateInstaller.exe");

            if (updateZipExists || updateExeExists)
            {
                ViewModelMain.Current.UpdateSubViewModel.UpdateReadyToInstall = true;
                var result = ConfirmationDialog.Show("Update is ready to install. Do you want to install it now?");
                if (result == Models.Enums.ConfirmationType.Yes)
                {
                    if (updateZipExists && File.Exists(updaterPath))
                    {
                        InstallHeadless(updaterPath);
                    }
                    else if (updateExeExists)
                    {
                        OpenExeInstaller(updateExeFiles[0]);
                    }
                }
            }
        }

        private void InstallHeadless(string updaterPath)
        {
            try
            {
                ProcessHelper.RunAsAdmin(updaterPath);
                Close();
            }
            catch (Win32Exception)
            {
                MessageBox.Show(
                    "Couldn't update without administrator rights.",
                    "Insufficient permissions",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OpenExeInstaller(string updateExeFile)
        {
            bool alreadyUpdated = AssemblyHelper.GetCurrentAssemblyVersion() ==
                    updateExeFile.Split('-')[1].Split(".exe")[0];

            if (!alreadyUpdated)
            {
                RestartToUpdate(updateExeFile);
            }
            else
            {
                File.Delete(updateExeFile);
            }
        }

        private void RestartToUpdate(string updateExeFile)
        {
            Process.Start(updateExeFile);
            Close();
        }
    }
}
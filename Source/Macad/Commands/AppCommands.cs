﻿using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using Macad.Common;
using Macad.Common.Interop;
using Macad.Core;
using Macad.Core.Topology;
using Macad.Interaction;
using Macad.Interaction.Dialogs;
using Macad.Presentation;

namespace Macad.Window
{
    public static class AppCommands
    {
        public static ActionCommand ExitApplication { get; } = new(
            () =>
            {
                Application.Current.MainWindow.Close();
            })
        {
            Header = () => "Exit Program"
        };

        //--------------------------------------------------------------------------------------------------

        internal static RelayCommand InitApplication { get; } = new(
            () =>
            {
                Messages.Info("Welcome to Macad|3D.");

                var cmdArgs = AppContext.CommandLine;

                // Check for command line option to load project
                // Only load models here, because importers opens dialogs, which is not allowed before main window is shown
                if (cmdArgs.HasPathToOpen 
                    && DocumentCommands.OpenFile.CanExecute(cmdArgs.PathToOpen)
                    && PathUtils.GetExtensionWithoutPoint(cmdArgs.PathToOpen).Equals(Model.FileExtension))
                {
                    DocumentCommands.OpenFile.Execute(cmdArgs.PathToOpen);
                }

                if (AppContext.Current.Document == null)
                {
                    DocumentCommands.CreateNewModel.Execute();
                }
            });

        //--------------------------------------------------------------------------------------------------

        public static RelayCommand FinishWindowInit { get; } = new(
            () =>
            {
                // Check for update
                if (!AppContext.IsInSandbox && VersionCheck.IsAutoCheckEnabled)
                {
                    VersionCheck.BeginCheckForUpdate();
                }
                
                // Load other files than models
                var cmdArgs = AppContext.CommandLine;
                if (cmdArgs.HasPathToOpen 
                    && DocumentCommands.OpenFile.CanExecute(cmdArgs.PathToOpen)
                    && !PathUtils.GetExtensionWithoutPoint(cmdArgs.PathToOpen).Equals(Model.FileExtension))
                {
                    Dispatcher.CurrentDispatcher.InvokeAsync(() => DocumentCommands.OpenFile.Execute(cmdArgs.PathToOpen), DispatcherPriority.Loaded);
                }

            });

        //--------------------------------------------------------------------------------------------------

        public static RelayCommand<CancelEventArgs> PrepareWindowClose { get; } = new(
            (e) =>
            {
                if (DocumentCommands.SaveAll.CanExecute())
                {
                    var result = Dialogs.AskForSavingChanges();
                    switch (result)
                    {
                        case TaskDialogResults.Cancel:
                            e.Cancel = true;
                            return;

                        case TaskDialogResults.Yes:
                            DocumentCommands.SaveAll.Execute();
                            e.Cancel = DocumentCommands.SaveAll.CanExecute();
                            break;

                        case TaskDialogResults.No:
                            break;
                    }
                }
            });

        //--------------------------------------------------------------------------------------------------


        public static ActionCommand ShowAboutDialog { get; } = new(
            () =>
            {
                new AboutDialog
                {
                    Owner = MainWindow.Current
                }.ShowDialog(); 
            })
        {
            Header = () => "About Macad|3D...",
            Description = () => "Shows version and license information.",
        };

        //--------------------------------------------------------------------------------------------------

        public static ActionCommand ShowLayerEditor { get; } = new(
            () => MainWindow.Current?.Docking.ActivateToolAnchorable("Layers") )
        {
            Header = () => "Layer Editor",
            Description = () => "Opens the layer editor.",
            Icon = () => "Layer-Editor"
        };

        //--------------------------------------------------------------------------------------------------

        public static ActionCommand ShowShapeInspector { get; } = new(
            () =>
            {
                MainWindow.Current?.Docking.ActivateToolAnchorable("ShapeInspector");
            })
        {
            Header = () => "Shape Inspector",
            Description = () => "Opens the shape inspector.",
            Icon = () => "Tool-ShapeInspect"
        };

        //--------------------------------------------------------------------------------------------------

        public static ActionCommand ResetWindowLayout { get; } = new(
            () =>
            {
                MainWindow.Current?.Docking.LoadWindowLayout("Default");
            })
        {
            Header = () => "Reset Window Layout",
            Description = () => "Resets the window layout to the default layout.",
            Icon = () => "App-RestoreLayout"
        };

        //--------------------------------------------------------------------------------------------------

        public static ActionCommand<string> ShowHelpTopic { get; } = new(
            (topicId) =>
            {
                var windowHandle = new HandleRef(null, IntPtr.Zero);
                if (string.IsNullOrEmpty(topicId))
                {
                    Win32Api.HtmlHelp(windowHandle, "Macad.UserGuide.chm", Win32Api.HtmlHelpCommand.HH_DISPLAY_TOC, "");
                }
                else
                {
                    Win32Api.HtmlHelp(windowHandle, "Macad.UserGuide.chm", Win32Api.HtmlHelpCommand.HH_DISPLAY_TOPIC, $"html/"+topicId+".htm");
                }
            })
        {
            Header = (topicId) => "Show User Guide",
            Description = (topicId) => "Open and browse the Macad|3D User Guide.",
            Icon = (topicId) => "App-UserGuide"
        };

        //--------------------------------------------------------------------------------------------------

    }
}
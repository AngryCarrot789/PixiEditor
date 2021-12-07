﻿using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Controllers.Shortcuts;
using PixiEditor.Models.Position;
using System;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class IoViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand MouseMoveCommand { get; set; }

        public RelayCommand MouseDownCommand { get; set; }

        public RelayCommand KeyDownCommand { get; set; }

        public RelayCommand KeyUpCommand { get; set; }

        private bool restoreToolOnKeyUp = false;

        public IoViewModel(ViewModelMain owner)
            : base(owner)
        {
            MouseMoveCommand = new RelayCommand(MouseMove);
            MouseDownCommand = new RelayCommand(MouseDown);
            KeyDownCommand = new RelayCommand(KeyDown);
            KeyUpCommand = new RelayCommand(KeyUp);
        }

        public void MouseHook_OnMouseUp(object sender, Point p, MouseButton button)
        {
            GlobalMouseHook.OnMouseUp -= MouseHook_OnMouseUp;
            if (button == MouseButton.Left)
            {
                Owner.BitmapManager.MouseController.StopRecordingMouseMovementChanges();
            }

            Owner.BitmapManager.MouseController.MouseUp(new MouseEventArgs(
                Mouse.PrimaryDevice,
                (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
        }

        public void KeyDown(object parameter)
        {
            KeyEventArgs args = (KeyEventArgs)parameter;
            if (args.IsRepeat && !restoreToolOnKeyUp && Owner.ShortcutController.LastShortcut != null &&
                Owner.ShortcutController.LastShortcut.Command == Owner.ToolsSubViewModel.SelectToolCommand)
            {
                restoreToolOnKeyUp = true;
                ShortcutController.BlockShortcutExecution = true;
            }

            Owner.ShortcutController.KeyPressed(args.Key, Keyboard.Modifiers);
            Owner.ToolsSubViewModel.ActiveTool.OnKeyDown(args);
        }

        private void MouseDown(object parameter)
        {
            if (Owner.BitmapManager.ActiveDocument == null || Owner.BitmapManager.ActiveDocument.Layers.Count == 0)
            {
                return;
            }

            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                BitmapManager bitmapManager = Owner.BitmapManager;
                var activeDocument = bitmapManager.ActiveDocument;
                if (!bitmapManager.MouseController.IsRecordingChanges)
                {
                    bool clickedOnCanvas = activeDocument.MouseXOnCanvas >= 0 &&
                        activeDocument.MouseXOnCanvas <= activeDocument.Width &&
                        activeDocument.MouseYOnCanvas >= 0 &&
                        activeDocument.MouseYOnCanvas <= activeDocument.Height;
                    bitmapManager.MouseController.StartRecordingMouseMovementChanges(clickedOnCanvas);
                    bitmapManager.MouseController.RecordMouseMovementChange(MousePositionConverter.CurrentCoordinates);
                }
            }

            Owner.BitmapManager.MouseController.MouseDown(new MouseEventArgs(
                Mouse.PrimaryDevice,
                (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()));

            Coordinates cords = new Coordinates(
                (int)Owner.BitmapManager.ActiveDocument.MouseXOnCanvas,
                (int)Owner.BitmapManager.ActiveDocument.MouseYOnCanvas);
            Owner.BitmapManager.MouseController.MouseDownCoordinates(cords);

            // Mouse down is guaranteed to only be raised from within this application, so by subscribing here we
            // only listen for mouse up events that occurred as a result of a mouse down within this application.
            // This seems better than maintaining a global listener indefinitely.
            GlobalMouseHook.OnMouseUp += MouseHook_OnMouseUp;
        }

        /// <summary>
        ///     Method connected with command, it executes tool "activity".
        /// </summary>
        /// <param name="parameter">CommandParameter.</param>
        private void MouseMove(object parameter)
        {
            if (Owner.BitmapManager.ActiveDocument == null)
            {
                return;
            }

            Coordinates cords = new Coordinates(
                (int)Owner.BitmapManager.ActiveDocument.MouseXOnCanvas,
                (int)Owner.BitmapManager.ActiveDocument.MouseYOnCanvas);
            MousePositionConverter.CurrentCoordinates = cords;

            if (Owner.BitmapManager.MouseController.IsRecordingChanges && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                Owner.BitmapManager.MouseController.RecordMouseMovementChange(cords);
            }

            Owner.BitmapManager.MouseController.MouseMoved(cords);
        }

        private void KeyUp(object parameter)
        {
            KeyEventArgs args = (KeyEventArgs)parameter;
            if (restoreToolOnKeyUp && Owner.ShortcutController.LastShortcut != null &&
                Owner.ShortcutController.LastShortcut.ShortcutKey == args.Key)
            {
                restoreToolOnKeyUp = false;
                Owner.ToolsSubViewModel.SetActiveTool(Owner.ToolsSubViewModel.LastActionTool);
                ShortcutController.BlockShortcutExecution = false;
            }

            Owner.ToolsSubViewModel.ActiveTool.OnKeyUp(args);
        }
    }
}

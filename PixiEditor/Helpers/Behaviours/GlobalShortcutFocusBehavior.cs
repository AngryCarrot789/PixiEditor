﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;
using PixiEditor.Models.Controllers.Shortcuts;

namespace PixiEditor.Helpers.Behaviours
{
    public class GlobalShortcutFocusBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.GotKeyboardFocus += AssociatedObject_GotKeyboardFocus;
            AssociatedObject.LostKeyboardFocus += AssociatedObject_LostKeyboardFocus;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.GotKeyboardFocus -= AssociatedObject_GotKeyboardFocus;
            AssociatedObject.LostKeyboardFocus -= AssociatedObject_LostKeyboardFocus;
        }

        private void AssociatedObject_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            ShortcutController.BlockShortcutExecution = false;
        }

        private void AssociatedObject_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            ShortcutController.BlockShortcutExecution = true;
        }
    }
}
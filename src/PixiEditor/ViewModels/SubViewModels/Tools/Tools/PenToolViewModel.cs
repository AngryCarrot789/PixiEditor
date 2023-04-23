﻿using System.Windows.Input;
using System.Windows.Media;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Events;
using PixiEditor.Models.UserPreferences;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools
{
    [Command.Tool(Key = Key.B)]
    internal class PenToolViewModel : ShapeTool
    {
        private int actualToolSize;

        public override BrushShape BrushShape => BrushShape.Circle;

        public PenToolViewModel()
        {
            Cursor = Cursors.Pen;
            ActionDisplay = "Click and move to draw.";
            Toolbar = ToolbarFactory.Create<PenToolViewModel, BasicToolbar>(this);
            
            ViewModelMain.Current.ToolsSubViewModel.SelectedToolChanged += SelectedToolChanged;
        }

        public override string Tooltip => $"Pen. ({Shortcut})";

        [Settings.Inherited]
        public int ToolSize => GetValue<int>();

        [Settings.Bool("Pixel perfect", Notify = nameof(PixelPerfectChanged))]
        public bool PixelPerfectEnabled => GetValue<bool>();

        public override void OnLeftMouseButtonDown(VecD pos)
        {
            ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UsePenTool();
        }

        private void SelectedToolChanged(object sender, SelectedToolEventArgs e)
        {
            if (e.NewTool == this && PixelPerfectEnabled)
            {
                var toolbar = (BasicToolbar)Toolbar;
                var setting = (SizeSetting)toolbar.Settings[0];
                setting.Value = 1;
            }
            
            if (!IPreferences.Current.GetPreference<bool>("EnableSharedToolbar"))
            {
                return;
            }

            if (e.OldTool is not { Toolbar: BasicToolbar oldToolbar })
            {
                return;
            }
            
            var oldSetting = (SizeSetting)oldToolbar.Settings[0];
            actualToolSize = oldSetting.Value;
        }

        public override void OnDeselecting()
        {
            if (!PixelPerfectEnabled)
            {
                return;
            }

            var toolbar = (BasicToolbar)Toolbar;
            var setting = (SizeSetting)toolbar.Settings[0];
            setting.Value = actualToolSize;
        }

        private void PixelPerfectChanged()
        {
            var toolbar = (BasicToolbar)Toolbar;
            var setting = (SizeSetting)toolbar.Settings[0];

            setting.SettingControl.IsEnabled = !PixelPerfectEnabled;

            if (PixelPerfectEnabled)
            {
                actualToolSize = ToolSize;
                setting.Value = 1;
            }
            else
            {
                setting.Value = actualToolSize;
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using PixiEditor.Models.Undo;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.Tools.Tools
{
    public class SelectTool : ReadonlyTool
    {
        private Selection oldSelection;

        public SelectTool()
        {
            ActionDisplay = "Click and move to select an area.";
            Tooltip = "Selects area. (M)";
            Toolbar = new SelectToolToolbar();
        }

        public SelectionType SelectionType { get; set; } = SelectionType.Add;

        public override void OnRecordingLeftMouseDown(MouseEventArgs e)
        {
            SelectionType = Toolbar.GetEnumSetting<SelectionType>("SelectMode").Value;

            oldSelection = null;
            Selection selection = ViewModelMain.Current.BitmapManager.ActiveDocument.ActiveSelection;
            if (selection != null && selection.SelectedPoints != null)
            {
                oldSelection = selection;
            }
        }

        public override void OnStoppedRecordingMouseUp(MouseEventArgs e)
        {
            if (ViewModelMain.Current.BitmapManager.ActiveDocument.ActiveSelection.SelectedPoints.Count() <= 1)
            {
                // If we have not selected multiple points, clear the selection
                ViewModelMain.Current.BitmapManager.ActiveDocument.ActiveSelection.Clear();
            }

            ViewModelMain.Current.BitmapManager.ActiveDocument.UndoManager.AddUndoChange(
                new Change(
                    "SelectedPoints",
                    oldSelection.SelectedPoints,
                    new ObservableCollection<Coordinates>(ViewModelMain.Current.BitmapManager.ActiveDocument.ActiveSelection.SelectedPoints),
                    "Select pixels",
                    ViewModelMain.Current.BitmapManager.ActiveDocument.ActiveSelection));
        }

        public override void Use(Coordinates[] pixels)
        {
            Select(pixels);
        }

        public IEnumerable<Coordinates> GetRectangleSelectionForPoints(Coordinates start, Coordinates end)
        {
            RectangleTool rectangleTool = new RectangleTool();
            List<Coordinates> selection = rectangleTool.CreateRectangle(start, end, 1).ToList();
            selection.AddRange(rectangleTool.CalculateFillForRectangle(start, end, 1));
            return selection;
        }

        /// <summary>
        ///     Gets coordinates of every pixel in root layer.
        /// </summary>
        /// <returns>Coordinates array of pixels.</returns>
        public IEnumerable<Coordinates> GetAllSelection()
        {
            return GetAllSelection(ViewModelMain.Current.BitmapManager.ActiveDocument);
        }

        /// <summary>
        ///     Gets coordinates of every pixel in chosen document.
        /// </summary>
        /// <returns>Coordinates array of pixels.</returns>
        public IEnumerable<Coordinates> GetAllSelection(Document document)
        {
            return GetRectangleSelectionForPoints(new Coordinates(0, 0), new Coordinates(document.Width - 1, document.Height - 1));
        }

        private void Select(Coordinates[] pixels)
        {
            IEnumerable<Coordinates> selection = GetRectangleSelectionForPoints(pixels[^1], pixels[0]);
            ViewModelMain.Current.BitmapManager.ActiveDocument.ActiveSelection.SetSelection(selection, SelectionType);
        }
    }
}
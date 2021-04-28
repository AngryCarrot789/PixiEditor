﻿using System;
using System.Linq;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class DocumentViewModel : SubViewModel<ViewModelMain>
    {
        public const string ConfirmationDialogMessage = "Document was modified. Do you want to save changes?";

        public RelayCommand CenterContentCommand { get; set; }

        public RelayCommand ClipCanvasCommand { get; set; }

        public RelayCommand DeletePixelsCommand { get; set; }

        public RelayCommand OpenResizePopupCommand { get; set; }

        public DocumentViewModel(ViewModelMain owner)
            : base(owner)
        {
            CenterContentCommand = new RelayCommand(CenterContent, Owner.DocumentIsNotNull);
            ClipCanvasCommand = new RelayCommand(ClipCanvas, Owner.DocumentIsNotNull);
            DeletePixelsCommand = new RelayCommand(DeletePixels, Owner.SelectionSubViewModel.SelectionIsNotEmpty);
            OpenResizePopupCommand = new RelayCommand(OpenResizePopup, Owner.DocumentIsNotNull);
        }

        public void ClipCanvas(object parameter)
        {
            Owner.BitmapManager.ActiveDocument?.ClipCanvas();
        }

        public void RequestCloseDocument(Document document)
        {
            if (!document.ChangesSaved)
            {
                ConfirmationType result = ConfirmationDialog.Show(ConfirmationDialogMessage);
                if (result == ConfirmationType.Yes)
                {
                    Owner.FileSubViewModel.SaveDocument(false);
                }
                else if (result == ConfirmationType.Canceled)
                {
                    return;
                }
            }
            Owner.BitmapManager.CloseDocument(document);
        }

        private void DeletePixels(object parameter)
        {
            Owner.BitmapManager.BitmapOperations.DeletePixels(
                Owner.BitmapManager.ActiveDocument.Layers.Where(x => x.IsActive && x.IsVisible).ToArray(),
                Owner.BitmapManager.ActiveDocument.ActiveSelection.SelectedPoints.ToArray());
        }

        private void OpenResizePopup(object parameter)
        {
            bool isCanvasDialog = (string)parameter == "canvas";
            ResizeDocumentDialog dialog = new ResizeDocumentDialog(
                Owner.BitmapManager.ActiveDocument.Width,
                Owner.BitmapManager.ActiveDocument.Height,
                isCanvasDialog);
            if (dialog.ShowDialog())
            {
                if (isCanvasDialog)
                {
                    Owner.BitmapManager.ActiveDocument.ResizeCanvas(dialog.Width, dialog.Height, dialog.ResizeAnchor);
                }
                else
                {
                    Owner.BitmapManager.ActiveDocument.Resize(dialog.Width, dialog.Height);
                }
            }
        }

        private void CenterContent(object property)
        {
            Owner.BitmapManager.ActiveDocument.CenterContent();
        }
    }
}
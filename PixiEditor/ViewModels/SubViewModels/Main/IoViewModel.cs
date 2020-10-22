﻿using Microsoft.Win32;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using PixiEditor.Models.IO;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class IoViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand OpenNewFilePopupCommand { get; set; }
        public RelayCommand SaveDocumentCommand { get; set; }
        public RelayCommand OpenFileCommand { get; set; }
        public RelayCommand ExportFileCommand { get; set; } //Command that is used to save file

        public IoViewModel(ViewModelMain owner) : base(owner)
        {
            OpenNewFilePopupCommand = new RelayCommand(OpenNewFilePopup);
            SaveDocumentCommand = new RelayCommand(SaveDocument, Owner.DocumentIsNotNull);
            OpenFileCommand = new RelayCommand(Open);
            ExportFileCommand = new RelayCommand(ExportFile, CanSave);
            Owner.OnStartupEvent += Owner_OnStartupEvent;
        }

        private void Owner_OnStartupEvent(object sender, System.EventArgs e)
        {
            var lastArg = Environment.GetCommandLineArgs().Last();
            if (Importer.IsSupportedFile(lastArg) && File.Exists(lastArg))
            {
                Open(lastArg);
            }
            else
            {
                OpenNewFilePopup(null);
            }
        }

        /// <summary>
        ///     Generates new Layer and sets it as active one
        /// </summary>
        /// <param name="parameter"></param>
        public void OpenNewFilePopup(object parameter)
        {
            NewFileDialog newFile = new NewFileDialog();
            if (newFile.ShowDialog())
            {
                NewDocument(newFile.Width, newFile.Height);
            }
        }

        public void NewDocument(int width, int height, bool addBaseLayer = true)
        {
            Owner.BitmapManager.ActiveDocument = new Document(width, height);
            if (addBaseLayer)
            {
                Owner.BitmapManager.AddNewLayer("Base Layer");
            }
            Owner.ResetProgramStateValues();
        }


        /// <summary>
        ///     Opens file from path.
        /// </summary>
        /// <param name="path"></param>
        public void OpenFile(string path)
        {
            ImportFileDialog dialog = new ImportFileDialog();

            if (path != null && File.Exists(path))
                dialog.FilePath = path;

            if (dialog.ShowDialog())
            {
                NewDocument(dialog.FileWidth, dialog.FileHeight, false);
                Owner.BitmapManager.AddNewLayer("Image", Importer.ImportImage(dialog.FilePath, dialog.FileWidth, dialog.FileHeight));
            }
        }

        private void Open(string path)
        {
            if (Owner.UnsavedDocumentModified)
            {
                var result = ConfirmationDialog.Show(ViewModelMain.ConfirmationDialogMessage);
                if (result == ConfirmationType.Yes)
                {
                    SaveDocument(null);
                }
                else if (result == ConfirmationType.Canceled)
                {
                    return;
                }
            }

            Owner.ResetProgramStateValues();
            if (path.EndsWith(".pixi"))
                OpenDocument(path);
            else
                OpenFile(path);
        }

        private void Open(object property)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "All Files|*.*|PixiEditor Files | *.pixi|PNG Files|*.png",
                DefaultExt = "pixi"
            };
            if ((bool)dialog.ShowDialog())
            {
                if (Importer.IsSupportedFile(dialog.FileName))
                    Open(dialog.FileName);
                Owner.RecenterZoombox = !Owner.RecenterZoombox;
            }
        }

        private void OpenDocument(string path)
        {
            Owner.BitmapManager.ActiveDocument = Importer.ImportDocument(path);
            Exporter.SaveDocumentPath = path;
            Owner.UnsavedDocumentModified = false;
        }

        public void SaveDocument(bool asNew)
        {
            SaveDocument(parameter: asNew ? "asnew" : null);
        }

        private void SaveDocument(object parameter)
        {
            bool paramIsAsNew = parameter != null && parameter.ToString()?.ToLower() == "asnew";
            if (paramIsAsNew || Exporter.SaveDocumentPath == null)
            {
                var saved = Exporter.SaveAsEditableFileWithDialog(Owner.BitmapManager.ActiveDocument, !paramIsAsNew);
                Owner.UnsavedDocumentModified = Owner.UnsavedDocumentModified && !saved;
            }
            else
            {
                Exporter.SaveAsEditableFile(Owner.BitmapManager.ActiveDocument, Exporter.SaveDocumentPath);
                Owner.UnsavedDocumentModified = false;
            }
        }

        /// <summary>
        ///     Generates export dialog or saves directly if save data is known.
        /// </summary>
        /// <param name="parameter"></param>
        private void ExportFile(object parameter)
        {
            WriteableBitmap bitmap = Owner.BitmapManager.GetCombinedLayersBitmap();
            Exporter.Export(bitmap, new Size(bitmap.PixelWidth, bitmap.PixelHeight));
        }

        /// <summary>
        ///     Returns true if file save is possible.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private bool CanSave(object property)
        {
            return Owner.BitmapManager.ActiveDocument != null;
        }

    }
}
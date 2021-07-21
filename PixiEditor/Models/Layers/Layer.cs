﻿using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Position;
using PixiEditor.Models.Undo;
using PixiEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Layers
{
    [DebuggerDisplay("'{name,nq}' {width}x{height}")]
    public class Layer : BasicLayer
    {
        private const int SizeOfArgb = 4;
        private bool clipRequested;

        private bool isActive;

        private bool isRenaming;
        private bool isVisible = true;
        private WriteableBitmap layerBitmap;

        private string name;

        private Thickness offset;

        private float opacity = 1f;

        private string layerHighlightColor = "#666666";

        public Layer(string name)
        {
            Name = name;
            LayerBitmap = BitmapFactory.New(0, 0);
            Width = 0;
            Height = 0;
            LayerGuid = Guid.NewGuid();
        }

        public Layer(string name, int width, int height)
        {
            Name = name;
            LayerBitmap = BitmapFactory.New(width, height);
            Width = width;
            Height = height;
            LayerGuid = Guid.NewGuid();
        }

        public Layer(string name, WriteableBitmap layerBitmap)
        {
            Name = name;
            LayerBitmap = layerBitmap;
            Width = layerBitmap.PixelWidth;
            Height = layerBitmap.PixelHeight;
            LayerGuid = Guid.NewGuid();
        }

        public Dictionary<Coordinates, Color> LastRelativeCoordinates { get; set; }

        public string LayerHighlightColor
        {
            get => IsActive ? layerHighlightColor : "#00000000";
            set
            {
                SetProperty(ref layerHighlightColor, value);
            }
        }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                RaisePropertyChanged(nameof(IsActive));
                RaisePropertyChanged(nameof(LayerHighlightColor));
            }
        }

        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (SetProperty(ref isVisible, value))
                {
                    RaisePropertyChanged(nameof(IsVisibleUndoTriggerable));
                    ViewModelMain.Current.ToolsSubViewModel.TriggerCacheOutdated();
                }
            }
        }

        public bool IsVisibleUndoTriggerable
        {
            get => IsVisible;
            set
            {
                if (value != IsVisible)
                {
                    ViewModelMain.Current?.BitmapManager?.ActiveDocument?.UndoManager
                        .AddUndoChange(
                        new Change(
                            nameof(IsVisible),
                            isVisible,
                            value,
                            LayerHelper.FindLayerByGuidProcess,
                            new object[] { LayerGuid },
                            "Change layer visibility"));
                    IsVisible = value;
                }
            }
        }

        public bool IsRenaming
        {
            get => isRenaming;
            set
            {
                isRenaming = value;
                RaisePropertyChanged("IsRenaming");
            }
        }

        public WriteableBitmap LayerBitmap
        {
            get => layerBitmap;
            set
            {
                layerBitmap = value;
                RaisePropertyChanged(nameof(LayerBitmap));
            }
        }

        public float Opacity
        {
            get => opacity;
            set
            {
                if (SetProperty(ref opacity, value))
                {
                    RaisePropertyChanged(nameof(OpacityUndoTriggerable));
                    ViewModelMain.Current.ToolsSubViewModel.TriggerCacheOutdated();
                }
            }
        }

        public float OpacityUndoTriggerable
        {
            get => Opacity;
            set
            {
                if (value != Opacity)
                {
                    ViewModelMain.Current?.BitmapManager?.ActiveDocument?.UndoManager
                    .AddUndoChange(
                                   new Change(
                                   nameof(Opacity),
                                   opacity,
                                   value,
                                   LayerHelper.FindLayerByGuidProcess,
                                   new object[] { LayerGuid },
                                   "Change layer opacity"));
                    Opacity = value;
                }
            }
        }

        public int OffsetX => (int)Offset.Left;

        public int OffsetY => (int)Offset.Top;

        public Thickness Offset
        {
            get => offset;
            set
            {
                offset = value;
                RaisePropertyChanged("Offset");
            }
        }

        public int MaxWidth { get; set; } = int.MaxValue;

        public int MaxHeight { get; set; } = int.MaxValue;

        /// <summary>
        /// Changes Guid of layer.
        /// </summary>
        /// <param name="newGuid">Guid to set.</param>
        /// <remarks>This is potentially destructive operation, use when absolutelly necessary.</remarks>
        public void ChangeGuid(Guid newGuid)
        {
            LayerGuid = newGuid;
        }

        public IEnumerable<Layer> GetLayers()
        {
            return new Layer[] { this };
        }

        /// <summary>
        ///     Returns clone of layer.
        /// </summary>
        public Layer Clone(bool generateNewGuid = false)
        {
            return new Layer(Name, LayerBitmap.Clone())
            {
                IsVisible = IsVisible,
                Offset = Offset,
                MaxHeight = MaxHeight,
                MaxWidth = MaxWidth,
                Opacity = Opacity,
                IsActive = IsActive,
                IsRenaming = IsRenaming,
                LayerGuid = generateNewGuid ? Guid.NewGuid() : LayerGuid
            };
        }

        public void RaisePropertyChange(string property)
        {
            RaisePropertyChanged(property);
        }

        /// <summary>
        ///     Resizes bitmap with it's content using NearestNeighbor interpolation.
        /// </summary>
        /// <param name="width">New width.</param>
        /// <param name="height">New height.</param>
        /// <param name="newMaxWidth">New layer maximum width, this should be document width.</param>
        /// <param name="newMaxHeight">New layer maximum height, this should be document height.</param>
        public void Resize(int width, int height, int newMaxWidth, int newMaxHeight)
        {
            LayerBitmap = LayerBitmap.Resize(width, height, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
            Width = width;
            Height = height;
            MaxWidth = newMaxWidth;
            MaxHeight = newMaxHeight;
        }

        /// <summary>
        ///     Converts coordinates relative to viewport to relative to layer.
        /// </summary>
        public Coordinates GetRelativePosition(Coordinates cords)
        {
            return new Coordinates(cords.X - OffsetX, cords.Y - OffsetY);
        }

        /// <summary>
        ///     Returns pixel color of x and y coordinates relative to document using (x - OffsetX) formula.
        /// </summary>
        /// <param name="x">Viewport relative X.</param>
        /// <param name="y">Viewport relative Y.</param>
        /// <returns>Color of a pixel.</returns>
        public Color GetPixelWithOffset(int x, int y)
        {
            Coordinates cords = GetRelativePosition(new Coordinates(x, y));
            return GetPixel(cords.X, cords.Y);
        }

        /// <summary>
        ///     Returns pixel color on x and y.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y Coordinate.</param>
        /// <returns>Color of pixel, if out of bounds, returns transparent pixel.</returns>
        public Color GetPixel(int x, int y)
        {
            if (x > Width - 1 || x < 0 || y > Height - 1 || y < 0)
            {
                return Color.FromArgb(0, 0, 0, 0);
            }

            return LayerBitmap.GetPixel(x, y);
        }

        /// <summary>
        ///     Applies pixel to layer.
        /// </summary>
        /// <param name="coordinates">Position of pixel.</param>
        /// <param name="color">Color of pixel.</param>
        /// <param name="dynamicResize">Resizes bitmap to fit content.</param>
        /// <param name="applyOffset">Converts pixels coordinates to relative to bitmap.</param>
        public void SetPixel(Coordinates coordinates, Color color, bool dynamicResize = true, bool applyOffset = true)
        {
            SetPixels(BitmapPixelChanges.FromSingleColoredArray(new[] { coordinates }, color), dynamicResize, applyOffset);
        }

        /// <summary>
        ///     Applies pixels to layer.
        /// </summary>
        /// <param name="pixels">Pixels to apply.</param>
        /// <param name="dynamicResize">Resizes bitmap to fit content.</param>
        /// <param name="applyOffset">Converts pixels coordinates to relative to bitmap.</param>
        public void SetPixels(BitmapPixelChanges pixels, bool dynamicResize = true, bool applyOffset = true)
        {
            if (pixels.ChangedPixels == null || pixels.ChangedPixels.Count == 0)
            {
                return;
            }

            if (dynamicResize)
            {
                DynamicResize(pixels);
            }

            if (applyOffset)
            {
                pixels.ChangedPixels = GetRelativePosition(pixels.ChangedPixels);
            }

            LastRelativeCoordinates = pixels.ChangedPixels;

            using (BitmapContext ctx = LayerBitmap.GetBitmapContext())
            {
                foreach (KeyValuePair<Coordinates, Color> coords in pixels.ChangedPixels)
                {
                    if (OutOfBounds(coords.Key))
                    {
                        continue;
                    }

                    ctx.WriteableBitmap.SetPixel(coords.Key.X, coords.Key.Y, coords.Value);
                }
            }

            ClipIfNecessary();
        }

        /// <summary>
        ///     Converts absolute coordinates array to relative to this layer coordinates array.
        /// </summary>
        /// <param name="nonRelativeCords">absolute coordinates array.</param>
        public Coordinates[] ConvertToRelativeCoordinates(Coordinates[] nonRelativeCords)
        {
            Coordinates[] result = new Coordinates[nonRelativeCords.Length];
            for (int i = 0; i < nonRelativeCords.Length; i++)
            {
                result[i] = new Coordinates(nonRelativeCords[i].X - OffsetX, nonRelativeCords[i].Y - OffsetY);
            }

            return result;
        }

        /// <summary>
        ///     Resizes canvas to fit pixels outside current bounds. Clamped to MaxHeight and MaxWidth.
        /// </summary>
        public void DynamicResize(BitmapPixelChanges pixels)
        {
            if (pixels.ChangedPixels.Count == 0)
            {
                return;
            }

            ResetOffset(pixels);
            Tuple<DoubleCords, bool> borderData = ExtractBorderData(pixels);
            DoubleCords minMaxCords = borderData.Item1;
            int newMaxX = minMaxCords.Coords2.X - OffsetX;
            int newMaxY = minMaxCords.Coords2.Y - OffsetY;
            int newMinX = minMaxCords.Coords1.X - OffsetX;
            int newMinY = minMaxCords.Coords1.Y - OffsetY;

            if (!(pixels.WasBuiltAsSingleColored && pixels.ChangedPixels.First().Value.A == 0))
            {
                if ((newMaxX + 1 > Width && Width < MaxWidth) || (newMaxY + 1 > Height && Height < MaxHeight))
                {
                    IncreaseSizeToBottomAndRight(newMaxX, newMaxY);
                }

                if ((newMinX < 0 && Width < MaxWidth) || (newMinY < 0 && Height < MaxHeight))
                {
                    IncreaseSizeToTopAndLeft(newMinX, newMinY);
                }
            }

            // if clip is requested
            if (borderData.Item2)
            {
                clipRequested = true;
            }
        }

        /// <summary>
        ///     Changes size of bitmap to fit content.
        /// </summary>
        public void ClipCanvas()
        {
            DoubleCords points = GetEdgePoints();
            int smallestX = points.Coords1.X;
            int smallestY = points.Coords1.Y;
            int biggestX = points.Coords2.X;
            int biggestY = points.Coords2.Y;

            if (smallestX < 0 && smallestY < 0 && biggestX < 0 && biggestY < 0)
            {
                return;
            }

            int width = biggestX - smallestX + 1;
            int height = biggestY - smallestY + 1;
            ResizeCanvas(0, 0, smallestX, smallestY, width, height);
            Offset = new Thickness(OffsetX + smallestX, OffsetY + smallestY, 0, 0);
        }

        /// <summary>
        ///     Clears bitmap.
        /// </summary>
        public void Clear()
        {
            LayerBitmap.Clear();
            ClipCanvas();
        }

        /// <summary>
        ///     Converts layer WriteableBitmap to byte array.
        /// </summary>
        public byte[] ConvertBitmapToBytes()
        {
            LayerBitmap.Lock();
            byte[] byteArray = LayerBitmap.ToByteArray();
            LayerBitmap.Unlock();
            return byteArray;
        }

        private Dictionary<Coordinates, Color> GetRelativePosition(Dictionary<Coordinates, Color> changedPixels)
        {
            return changedPixels.ToDictionary(
                d => new Coordinates(d.Key.X - OffsetX, d.Key.Y - OffsetY),
                d => d.Value);
        }

        private Tuple<DoubleCords, bool> ExtractBorderData(BitmapPixelChanges pixels)
        {
            Coordinates firstCords = pixels.ChangedPixels.First().Key;
            int minX = firstCords.X;
            int minY = firstCords.Y;
            int maxX = minX;
            int maxY = minY;
            bool clipRequested = false;

            foreach (KeyValuePair<Coordinates, Color> pixel in pixels.ChangedPixels)
            {
                if (pixel.Key.X < minX)
                {
                    minX = pixel.Key.X;
                }
                else if (pixel.Key.X > maxX)
                {
                    maxX = pixel.Key.X;
                }

                if (pixel.Key.Y < minY)
                {
                    minY = pixel.Key.Y;
                }
                else if (pixel.Key.Y > maxY)
                {
                    maxY = pixel.Key.Y;
                }

                if (clipRequested == false && IsBorderPixel(pixel.Key) && pixel.Value.A == 0)
                {
                    clipRequested = true;
                }
            }

            return new Tuple<DoubleCords, bool>(
                new DoubleCords(new Coordinates(minX, minY), new Coordinates(maxX, maxY)), clipRequested);
        }

        private bool IsBorderPixel(Coordinates cords)
        {
            return cords.X - OffsetX == 0 || cords.Y - OffsetY == 0 || cords.X - OffsetX == Width - 1 ||
                   cords.Y - OffsetY == Height - 1;
        }

        private bool OutOfBounds(Coordinates cords)
        {
            return cords.X < 0 || cords.X > Width - 1 || cords.Y < 0 || cords.Y > Height - 1;
        }

        private void ClipIfNecessary()
        {
            if (clipRequested)
            {
                ClipCanvas();
                clipRequested = false;
            }
        }

        private void IncreaseSizeToBottomAndRight(int newMaxX, int newMaxY)
        {
            if (MaxWidth - OffsetX < 0 || MaxHeight - OffsetY < 0)
            {
                return;
            }

            newMaxX = Math.Clamp(Math.Max(newMaxX + 1, Width), 0, MaxWidth - OffsetX);
            newMaxY = Math.Clamp(Math.Max(newMaxY + 1, Height), 0, MaxHeight - OffsetY);

            ResizeCanvas(0, 0, 0, 0, newMaxX, newMaxY);
        }

        private void IncreaseSizeToTopAndLeft(int newMinX, int newMinY)
        {
            newMinX = Math.Clamp(Math.Min(newMinX, Width), Math.Min(-OffsetX, OffsetX), 0);
            newMinY = Math.Clamp(Math.Min(newMinY, Height), Math.Min(-OffsetY, OffsetY), 0);

            Offset = new Thickness(
                Math.Clamp(OffsetX + newMinX, 0, MaxWidth),
                Math.Clamp(OffsetY + newMinY, 0, MaxHeight),
                0,
                0);

            int newWidth = Math.Clamp(Width - newMinX, 0, MaxWidth);
            int newHeight = Math.Clamp(Height - newMinY, 0, MaxHeight);

            int offsetX = Math.Abs(newWidth - Width);
            int offsetY = Math.Abs(newHeight - Height);

            ResizeCanvas(offsetX, offsetY, 0, 0, newWidth, newHeight);
        }

        private DoubleCords GetEdgePoints()
        {
            Coordinates smallestPixel = CoordinatesCalculator.FindMinEdgeNonTransparentPixel(LayerBitmap);
            Coordinates biggestPixel = CoordinatesCalculator.FindMostEdgeNonTransparentPixel(LayerBitmap);

            return new DoubleCords(smallestPixel, biggestPixel);
        }

        private void ResetOffset(BitmapPixelChanges pixels)
        {
            if (Width == 0 || Height == 0)
            {
                int offsetX = Math.Max(pixels.ChangedPixels.Min(x => x.Key.X), 0);
                int offsetY = Math.Max(pixels.ChangedPixels.Min(x => x.Key.Y), 0);
                Offset = new Thickness(offsetX, offsetY, 0, 0);
            }
        }

        /// <summary>
        ///     Resizes canvas to new size with specified offset.
        /// </summary>
        private void ResizeCanvas(int offsetX, int offsetY, int offsetXSrc, int offsetYSrc, int newWidth, int newHeight)
        {
            int iteratorHeight = Height > newHeight ? newHeight : Height;
            int count = Width > newWidth ? newWidth : Width;

            using (BitmapContext srcContext = LayerBitmap.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                WriteableBitmap result = BitmapFactory.New(newWidth, newHeight);
                using (BitmapContext destContext = result.GetBitmapContext())
                {
                    for (int line = 0; line < iteratorHeight; line++)
                    {
                        int srcOff = (((offsetYSrc + line) * Width) + offsetXSrc) * SizeOfArgb;
                        int dstOff = (((offsetY + line) * newWidth) + offsetX) * SizeOfArgb;
                        BitmapContext.BlockCopy(srcContext, srcOff, destContext, dstOff, count * SizeOfArgb);
                    }

                    LayerBitmap = result;
                    Width = newWidth;
                    Height = newHeight;
                }
            }
        }
    }
}

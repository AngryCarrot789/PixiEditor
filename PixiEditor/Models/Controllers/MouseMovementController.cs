﻿using System;
using System.Collections.Generic;
using System.Windows.Input;
using PixiEditor.Models.Position;

namespace PixiEditor.Models.Controllers
{
    public class MouseMovementController
    {
        public event EventHandler StartedRecordingChanges;

        public event EventHandler<MouseEventArgs> OnMouseDown;

        public event EventHandler<MouseEventArgs> OnMouseUp;

        public event EventHandler<MouseMovementEventArgs> MousePositionChanged;

        public event EventHandler StoppedRecordingChanges;

        public List<Coordinates> LastMouseMoveCoordinates { get; set; } = new List<Coordinates>();

        public bool IsRecordingChanges { get; private set; }

        public bool ClickedOnCanvas { get; set; }

        public void StartRecordingMouseMovementChanges(bool clickedOnCanvas)
        {
            if (IsRecordingChanges == false)
            {
                LastMouseMoveCoordinates.Clear();
                IsRecordingChanges = true;
                ClickedOnCanvas = clickedOnCanvas;
                StartedRecordingChanges?.Invoke(this, EventArgs.Empty);
            }
        }

        public void RecordMouseMovementChange(Coordinates mouseCoordinates)
        {
            if (IsRecordingChanges)
            {
                if (LastMouseMoveCoordinates.Count == 0 || mouseCoordinates != LastMouseMoveCoordinates[^1])
                {
                    LastMouseMoveCoordinates.Insert(0, mouseCoordinates);
                    MousePositionChanged?.Invoke(this, new MouseMovementEventArgs(mouseCoordinates));
                }
            }
        }

        /// <summary>
        ///     Plain mouse move, does not affect mouse drag recordings.
        /// </summary>
        public void MouseMoved(Coordinates mouseCoordinates)
        {
            MousePositionChanged?.Invoke(this, new MouseMovementEventArgs(mouseCoordinates));
        }

        /// <summary>
        /// Plain mouse down, does not affect mouse recordings.
        /// </summary>
        public void MouseDown(MouseEventArgs args)
        {
            OnMouseDown?.Invoke(this, args);
        }

        /// <summary>
        /// Plain mouse up, does not affect mouse recordings.
        /// </summary>
        public void MouseUp(MouseEventArgs args)
        {
            OnMouseUp?.Invoke(this, args);
        }

        public void StopRecordingMouseMovementChanges()
        {
            if (IsRecordingChanges)
            {
                IsRecordingChanges = false;
                ClickedOnCanvas = false;
                StoppedRecordingChanges?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
﻿using System;
using DiscordRPC;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.UserPreferences;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class DiscordViewModel : SubViewModel<ViewModelMain>
    {
        private DiscordRpcClient client;
        private string clientId;
        private Document currentDocument;

        public bool Enabled
        {
            get => client != null;
            set
            {
                if (Enabled != value)
                {
                    if (value)
                    {
                        Start();
                    }
                    else
                    {
                        Stop();
                    }
                }
            }
        }

        private bool showDocumentName = IPreferences.Current.GetPreference(nameof(ShowDocumentName), true);

        public bool ShowDocumentName
        {
            get => showDocumentName;
            set
            {
                if (showDocumentName != value)
                {
                    showDocumentName = value;
                    UpdatePresence(currentDocument);
                }
            }
        }

        private bool showDocumentSize = IPreferences.Current.GetPreference(nameof(ShowDocumentSize), true);

        public bool ShowDocumentSize
        {
            get => showDocumentSize;
            set
            {
                if (showDocumentSize != value)
                {
                    showDocumentSize = value;
                    UpdatePresence(currentDocument);
                }
            }
        }

        private bool showLayerCount = IPreferences.Current.GetPreference(nameof(ShowLayerCount), true);

        public bool ShowLayerCount
        {
            get => showLayerCount;
            set
            {
                if (showLayerCount != value)
                {
                    showLayerCount = value;
                    UpdatePresence(currentDocument);
                }
            }
        }

        public DiscordViewModel(ViewModelMain owner, string clientId)
            : base(owner)
        {
            Owner.BitmapManager.DocumentChanged += DocumentChanged;
            this.clientId = clientId;

            Enabled = IPreferences.Current.GetPreference<bool>("EnableRichPresence");
            IPreferences.Current.AddCallback("EnableRichPresence", x => Enabled = (bool)x);
            IPreferences.Current.AddCallback(nameof(ShowDocumentName), x => ShowDocumentName = (bool)x);
            IPreferences.Current.AddCallback(nameof(ShowDocumentSize), x => ShowDocumentSize = (bool)x);
            IPreferences.Current.AddCallback(nameof(ShowLayerCount), x => ShowLayerCount = (bool)x);

            AppDomain.CurrentDomain.ProcessExit += (_, _) => Enabled = false;
        }

        public void Start()
        {
            client = new DiscordRpcClient(clientId);
            client.OnReady += OnReady;
            client.Initialize();
        }

        public void Stop()
        {
            client.ClearPresence();
            client.Dispose();
            client = null;
        }

        public void UpdatePresence(Document document)
        {
            if (client == null)
            {
                return;
            }

            RichPresence richPresence = NewDefaultRP();

            if (document != null)
            {
                richPresence.WithTimestamps(new Timestamps(document.OpenedUTC));

                richPresence.Details = ShowDocumentName ? $"Editing {document.Name}" : "Editing something (incognito)";

                string state = string.Empty;

                if (ShowDocumentSize)
                {
                    state = $"{document.Width}x{document.Height}";
                }

                if (ShowDocumentSize && ShowLayerCount)
                {
                    state += ", ";
                }

                if (ShowLayerCount)
                {
                    state += document.Layers.Count == 1 ? "1 Layer" : $"{document.Layers.Count} Layers";
                }

                richPresence.State = state;
            }

            client.SetPresence(richPresence);
        }

        private static RichPresence NewDefaultRP()
        {
            return new RichPresence
            {
                Details = "Staring at absolutely",
                State = "nothing",
                Buttons = new Button[]
                {
                    new Button() { Label = "Download PixiEditor", Url = "https://www.github.com/PixiEditor/PixiEditor/releases/latest" },
                    new Button() { Label = "Watch trailer", Url = "https://youtu.be/QKnXBUY0Pqk" }
                },

                Assets = new Assets
                {
                    LargeImageKey = "editorlogo",
                    LargeImageText = "You discovered PixiEditor's logo",
                    SmallImageKey = "github",
                    SmallImageText = "Download PixiEditor on GitHub (github.com/PixiEditor/PixiEditor)!"
                },
                Timestamps = new Timestamps()
                {
                    Start = DateTime.UtcNow
                }
            };
        }

        private void DocumentChanged(object sender, Models.Events.DocumentChangedEventArgs e)
        {
            if (currentDocument != null)
            {
                currentDocument.PropertyChanged -= DocumentPropertyChanged;
                currentDocument.LayersChanged -= DocumentLayerChanged;
            }

            currentDocument = e.NewDocument;

            if (currentDocument != null)
            {
                UpdatePresence(currentDocument);
                currentDocument.PropertyChanged += DocumentPropertyChanged;
                currentDocument.LayersChanged += DocumentLayerChanged;
            }
        }

        private void DocumentLayerChanged(object sender, Models.Controllers.LayersChangedEventArgs e)
        {
            UpdatePresence(currentDocument);
        }

        private void DocumentPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name" || e.PropertyName == "Width" || e.PropertyName == "Height")
            {
                UpdatePresence(currentDocument);
            }
        }

        private void OnReady(object sender, DiscordRPC.Message.ReadyMessage args)
        {
            UpdatePresence(Owner.BitmapManager.ActiveDocument);
        }

        ~DiscordViewModel()
        {
            Enabled = false;
        }
    }
}
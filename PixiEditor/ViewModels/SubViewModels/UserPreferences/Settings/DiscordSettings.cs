﻿namespace PixiEditor.ViewModels.SubViewModels.UserPreferences.Settings
{
    public class DiscordSettings : SettingsGroup
    {
        private bool enableRichPresence = GetPreference(nameof(EnableRichPresence), true);

        public bool EnableRichPresence
        {
            get => enableRichPresence;
            set
            {
                enableRichPresence = value;
                RaiseAndUpdatePreference(nameof(EnableRichPresence), value);
            }
        }

        private bool showDocumentName = GetPreference(nameof(ShowDocumentName), true);

        public bool ShowDocumentName
        {
            get => showDocumentName;
            set
            {
                showDocumentName = value;
                RaiseAndUpdatePreference(nameof(ShowDocumentName), value);
                RaisePropertyChanged(nameof(DetailPreview));
            }
        }

        private bool showDocumentSize = GetPreference(nameof(ShowDocumentSize), true);

        public bool ShowDocumentSize
        {
            get => showDocumentSize;
            set
            {
                showDocumentSize = value;
                RaiseAndUpdatePreference(nameof(ShowDocumentSize), value);
                RaisePropertyChanged(nameof(StatePreview));
            }
        }

        private bool showLayerCount = GetPreference(nameof(ShowLayerCount), true);

        public bool ShowLayerCount
        {
            get => showLayerCount;
            set
            {
                showLayerCount = value;
                RaiseAndUpdatePreference(nameof(ShowLayerCount), value);
                RaisePropertyChanged(nameof(StatePreview));
            }
        }

        public string DetailPreview
        {
            get
            {
                return ShowDocumentName ? $"Editing coolPixelArt.pixi" : "Editing something (incognito)";
            }
        }

        public string StatePreview
        {
            get
            {
                string state = string.Empty;

                if (ShowDocumentSize)
                {
                    state = "16x16";
                }

                if (ShowDocumentSize && ShowLayerCount)
                {
                    state += ", ";
                }

                if (ShowLayerCount)
                {
                    state += "2 Layers";
                }

                return state;
            }
        }
    }
}
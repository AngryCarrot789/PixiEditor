﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Platform;
using Newtonsoft.Json;
using PixiEditor.Avalonia.Helpers.Extensions;
using PixiEditor.Extensions;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Models.AppExtensions;
using PixiEditor.Models.IO;

namespace PixiEditor.Models.Localization;

internal class LocalizationProvider : ILocalizationProvider
{
    private Language debugLanguage;

    public string LocalizationDataPath { get; } = Path.Combine(Paths.DataResourceUri, "Localization", "LocalizationData.json");

    public LocalizationData LocalizationData { get; private set; }

    public Language CurrentLanguage { get; set; }

    public LanguageData SelectedLanguage { get; private set; }

    public LanguageData FollowSystem { get; } = new() { Name = "Follow system", Code = "system" };

    public event Action<Language> OnLanguageChanged;

    public void ReloadLanguage() => OnLanguageChanged?.Invoke(CurrentLanguage);

    public Language DefaultLanguage { get; private set; }

    private ExtensionLoader extensionLoader;

    public LocalizationProvider(ExtensionLoader extensionLoader)
    {
        this.extensionLoader = extensionLoader;
        ILocalizationProvider.SetAsCurrent(this);
    }

    public void LoadData(string currentLanguageCode = null)
    {
        JsonSerializer serializer = new();
        
        if (!AssetLoader.Exists(new Uri(LocalizationDataPath)))
        {
            throw new FileNotFoundException("Localization data file not found.", LocalizationDataPath);
        }
        
        using Stream stream = AssetLoader.Open(new Uri(LocalizationDataPath));
        LocalizationData = serializer.Deserialize<LocalizationData>(new JsonTextReader(new StreamReader(stream)) { Culture = CultureInfo.InvariantCulture, DateTimeZoneHandling = DateTimeZoneHandling.Utc });

        if (LocalizationData is null)
        {
            throw new InvalidDataException("Localization data is null.");
        }

        LoadExtensionLocalizationData(LocalizationData);

        if (LocalizationData.Languages is null || LocalizationData.Languages.Count == 0)
        {
            throw new InvalidDataException("Localization data does not contain any languages.");
        }

        LocalizationData.Languages.Add(FollowSystem);
        
        DefaultLanguage = LoadLanguageInternal(LocalizationData.Languages[0]);

        LoadLanguage(LocalizationData.Languages.FirstOrDefault(x => x.Code == currentLanguageCode, FollowSystem));
    }

    private void LoadExtensionLocalizationData(LocalizationData localizationData)
    {
        if(localizationData is null)
        {
            throw new InvalidDataException(nameof(localizationData));
        }

        if (extensionLoader?.LoadedExtensions is null)
        {
            return;
        }

        foreach (Extension extension in extensionLoader?.LoadedExtensions)
        {
            if (extension.Metadata.Localization is null)
            {
                continue;
            }

            localizationData.MergeWith(extension.Metadata.Localization.Languages, Path.GetDirectoryName(extension.Assembly.Location));
        }
    }

    public void LoadLanguage(LanguageData languageData)
    {
        if (languageData is null)
        {
            throw new ArgumentNullException(nameof(languageData));
        }
        
        if(languageData.Code == CurrentLanguage?.LanguageData.Code)
        {
            return;
        }
        
        bool firstLoad = CurrentLanguage is null;
        
        SelectedLanguage = languageData;

        if (languageData.Code == FollowSystem.Code)
        {
            string osLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            languageData = LocalizationData.Languages.FirstOrDefault(x => x.Code == osLanguage, LocalizationData.Languages[0]);
        }
        
        CurrentLanguage = LoadLanguageInternal(languageData);

        if (!firstLoad)
        {
            OnLanguageChanged?.Invoke(CurrentLanguage);
        }
    }

    public void LoadDebugKeys(Dictionary<string, string> languageKeys, bool rightToLeft)
    {
        debugLanguage = new Language(
            new LanguageData
        {
            Code = "debug",
            Name = "Debug"
        }, languageKeys, rightToLeft);

        CurrentLanguage = debugLanguage;
        
        OnLanguageChanged?.Invoke(debugLanguage);
    }

    private Language LoadLanguageInternal(LanguageData languageData)
    {
        string mainLocalePath = GetLocalePath(languageData);

        if (!AssetLoader.Exists(new Uri(mainLocalePath)))
        {
            throw new FileNotFoundException("Locale file not found.", mainLocalePath);
        }

        Dictionary<string, string> locale = new Dictionary<string, string>();

        languageData.AdditionalLocalePaths ??= new List<string>();
        int localesCount = 1 + languageData.AdditionalLocalePaths.Count;

        string[] allLocalePaths = new string[localesCount];
        allLocalePaths[0] = mainLocalePath;
        languageData.AdditionalLocalePaths.CopyTo(allLocalePaths, 1);

        foreach (string localePath in allLocalePaths)
        {
            if (!AssetLoader.Exists(new Uri(localePath)) && !File.Exists(localePath))
            {
                continue;
            }

            locale.AddRangeOverride(ReadLocaleFile(localePath));
        }

        if (locale is null)
        {
            throw new InvalidDataException("Locale is null.");
        }

        return new(languageData, locale, languageData.RightToLeft);
    }

    private IDictionary<string, string> ReadLocaleFile(string localePath)
    {
        JsonSerializer serializer = new();
        using Stream stream = AssetLoader.Open(new Uri(localePath));
        return serializer.Deserialize<Dictionary<string, string>>(new JsonTextReader(new StreamReader(stream)));
    }

    private string GetLocalePath(LanguageData languageData)
    {
        if (languageData.CustomLocaleAssemblyPath is not null)
        {
            return Path.Combine(languageData.CustomLocaleAssemblyPath, languageData.LocaleFileName);
        }

        return Path.Combine(Paths.DataResourceUri, "Localization", "Languages", languageData.LocaleFileName);
    }
}
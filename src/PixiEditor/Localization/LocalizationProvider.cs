﻿using System.IO;
using PixiEditor.Models.Commands.Attributes.Commands;

namespace PixiEditor.Localization;

internal class LocalizationProvider : ILocalizationProvider
{
    public string LocalizationDataPath { get; } = Path.Combine("Data", "Localization", "LocalizationData.json");
    public LocalizationData LocalizationData { get; private set; }
    public Language CurrentLanguage { get; set; }
    public event Action<Language> OnLanguageChanged;

    public void LoadData()
    {
        Newtonsoft.Json.JsonSerializer serializer = new();
        
        if (!File.Exists(LocalizationDataPath))
        {
            throw new FileNotFoundException("Localization data file not found.", LocalizationDataPath);
        }
        
        using StreamReader reader = new(LocalizationDataPath);
        LocalizationData = serializer.Deserialize<LocalizationData>(new Newtonsoft.Json.JsonTextReader(reader));
            
        if (LocalizationData is null)
        {
            throw new InvalidDataException("Localization data is null.");
        }
        
        if (LocalizationData.Languages is null || LocalizationData.Languages.Length == 0)
        {
            throw new InvalidDataException("Localization data does not contain any languages.");
        }
        
        LoadLanguage(LocalizationData.Languages[0]);
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
        
        string localePath = Path.Combine("Data", "Localization", "Languages", languageData.LocaleFileName);
        
        if(!File.Exists(localePath))
        {
            throw new FileNotFoundException("Locale file not found.", localePath);
        }
        
        Newtonsoft.Json.JsonSerializer serializer = new();
        using StreamReader reader = new(localePath);
        Dictionary<string, string> locale = serializer.Deserialize<Dictionary<string, string>>(new Newtonsoft.Json.JsonTextReader(reader));
        
        if (locale is null)
        {
            throw new InvalidDataException("Locale is null.");
        }
        
        bool firstLoad = CurrentLanguage is null;
        CurrentLanguage = new(languageData, locale);
        
        if (!firstLoad)
        {
            OnLanguageChanged?.Invoke(CurrentLanguage);
        }
    }
}

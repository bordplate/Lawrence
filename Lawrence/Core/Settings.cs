using System;
using System.Collections.Generic;
using System.IO;
using Nett;

namespace Lawrence.Core;

/// <summary>
/// A class for handling application settings using the TOML format.
/// </summary>
public class Settings {
    private static Settings _default;
    private readonly TomlTable _settingsTable;
    private readonly string _configFilePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="Settings"/> class.
    /// </summary>
    /// <param name="configFilePath">The path to the TOML settings file.</param>
    /// <param name="createIfNotExists">Whether to create a new file if the specified file does not exist.</param>
    /// <exception cref="FileNotFoundException">Thrown if the file does not exist and createIfNotExists is set to false.</exception>
    public Settings(string configFilePath, bool createIfNotExists = false) {
        if (!File.Exists(configFilePath)) {
            if (!createIfNotExists) {
                throw new FileNotFoundException($"Configuration file not found: {configFilePath}");
            }

            var file = File.Create(configFilePath);
            file.Close();
            file.Dispose();
        }

        _configFilePath = configFilePath;
        _settingsTable = Toml.ReadFile(configFilePath);
    }

    /// <summary>
    /// Gets the default instance of the <see cref="Settings"/> class.
    /// </summary>
    /// <param name="configFilePath">The path to the TOML settings file. Default value is "settings.toml".</param>
    /// <returns>The default instance of the <see cref="Settings"/> class.</returns>
    public static Settings Default(string configFilePath = "settings.toml") {
        return _default ??= new Settings(configFilePath, true);
    }

    /// <summary>
    /// Gets the value for the specified key from the settings. If the setting is not found and there is a fallback
    /// provided, the fallback will be written to the settings key in the configuration file. 
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <param name="fallback">The fallback value to return if the key is not found. Default value is default(T).</param>
    /// <param name="silent">If false, saves the fallback to the config, if true, does not save the fallback value to the config file.</param>
    /// <returns>The value associated with the specified key, or the fallback value if the key is not found.</returns>
    public T Get<T>(string key, T fallback = default, bool silent = false) {
        string[] keys = key.Split('.');
        TomlTable current = _settingsTable;

        try {
            for (int i = 0; i < keys.Length - 1; i++) {
                current = current.Get<TomlTable>(keys[i]);
                if (current == null) {
                    if (fallback != null) {
                        Set(key, fallback);
                    }

                    return fallback;
                }
            }

            return current.Get<T>(keys[keys.Length - 1]);
        } catch (KeyNotFoundException) {
            if (fallback != null && !silent) {
                Set(key, fallback);
            }

            return fallback;
        }
    }

    /// <summary>
    /// Sets the value for the specified key in the settings.
    /// </summary>
    /// <typeparam name="T">The type of the value to set.</typeparam>
    /// <param name="key">The key to set the value for.</param>
    /// <param name="value">The value to set for the specified key.</param>
    /// <exception cref="ArgumentException">Thrown if the specified key is not found.</exception>
    public void Set<T>(string key, T value) {
        string[] keys = key.Split('.');
        TomlTable current = _settingsTable;

        for (int i = 0; i < keys.Length - 1; i++) {
            if (!current.TryGetValue(keys[i], out TomlObject next)) {
                next = current.CreateEmptyAttachedTable();
                current.Add(keys[i], next);
            }

            if (!(next is TomlTable)) {
                throw new ArgumentException($"Cannot set value for key '{key}', because '{keys[i]}' is not a table.");
            }

            current = (TomlTable)next;
        }
        
        if (current.ContainsKey(keys[keys.Length - 1])) {
            current.Remove(keys[keys.Length - 1]);
        }

        switch (value) {
            case int conv:
                current.Add(keys[keys.Length - 1], current.CreateAttached(conv));
                break;
            case string conv:
                current.Add(keys[keys.Length - 1], current.CreateAttached(conv));
                break;
            case double conv:
                current.Add(keys[keys.Length - 1], current.CreateAttached(conv));
                break;
            case bool conv:
                current.Add(keys[keys.Length - 1], current.CreateAttached(conv));
                break;
            case IEnumerable<bool> conv:
                current.Add(keys[keys.Length - 1], current.CreateAttached(conv));
                break;
            case IEnumerable<string> conv:
                current.Add(keys[keys.Length - 1], current.CreateAttached(conv));
                break;
            case IEnumerable<long> conv:
                current.Add(keys[keys.Length - 1], current.CreateAttached(conv));
                break;
            case IEnumerable<int> conv:
                current.Add(keys[keys.Length - 1], current.CreateAttached(conv));
                break;
            case IEnumerable<double> conv:
                current.Add(keys[keys.Length - 1], current.CreateAttached(conv));
                break;
            case IEnumerable<float> conv:
                current.Add(keys[keys.Length - 1], current.CreateAttached(conv));
                break;
            case IEnumerable<TimeSpan> conv:
                current.Add(keys[keys.Length - 1], current.CreateAttached(conv));
                break;
            case IEnumerable<DateTime> conv:
                current.Add(keys[keys.Length - 1], current.CreateAttached(conv));
                break;
            case IEnumerable<DateTimeOffset> conv:
                current.Add(keys[keys.Length - 1], current.CreateAttached(conv));
                break;
            default:
                current.Add(keys[keys.Length - 1], current.CreateAttached(value));
                break;
        }

        Save();
    }

    /// <summary>
    /// Saves the settings to the file specified in the constructor.
    /// </summary>
    public void Save() {
        Toml.WriteFile(_settingsTable, _configFilePath);
    }
}

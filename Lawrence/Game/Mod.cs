using System;
using System.Collections.Generic;
using System.Linq;

using Lawrence.Core;

namespace Lawrence.Game;

public class Mod
{
    private readonly Settings _modSettings;

    private readonly string _modPath;

    public Mod(string configuration)
    {
        _modSettings = new Settings(configuration, false);

        if (_modSettings == null) {
            throw new Exception($"Couldn't load configuration file {configuration} for mod.");
        }

        _modPath = System.IO.Path.GetDirectoryName(configuration);
    }

    public Settings Settings() {
        return _modSettings;
    }

    public string Path() {
        return _modPath;
    }
}

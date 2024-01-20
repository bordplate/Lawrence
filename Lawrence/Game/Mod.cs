using System;
using System.Collections.Generic;
using System.Linq;

using Lawrence.Core;

namespace Lawrence
{
    public class Mod
    {
        private Settings _modSettings;

        private string _modPath;

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
}


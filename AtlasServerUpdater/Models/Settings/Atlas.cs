﻿using System.Collections.Generic;

namespace AtlasServerUpdater.Models.Settings
{
    public class Atlas
    {
        public string FolderPath { get; set; }
        public string Executable { get; set; }
        public List<string> BatchFiles { get; set; }
        public string ServerProcessName { get; set; }
    }
}

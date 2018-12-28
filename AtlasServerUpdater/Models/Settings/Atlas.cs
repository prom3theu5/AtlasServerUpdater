using System.Collections.Generic;

namespace AtlasServerUpdater.Models.Settings
{
    /// <summary>
    /// Class Atlas.
    /// </summary>
    public class Atlas
    {
        /// <summary>
        /// Gets or sets the folder path.
        /// </summary>
        /// <value>The folder path.</value>
        public string FolderPath { get; set; }
        /// <summary>
        /// Gets or sets the executable.
        /// </summary>
        /// <value>The executable.</value>
        public string Executable { get; set; }
        /// <summary>
        /// Gets or sets the batch files.
        /// </summary>
        /// <value>The batch files.</value>
        public List<string> BatchFiles { get; set; }
        /// <summary>
        /// Gets or sets the name of the server process.
        /// </summary>
        /// <value>The name of the server process.</value>
        public string ServerProcessName { get; set; }
    }
}

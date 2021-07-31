using OpenBullet2.Core.Exceptions;
using RuriLib.Models.Proxies;
using RuriLib.Models.Variables;
using RuriLib.Services;
using System.Collections.Generic;
using System.Linq;

namespace OpenBullet2.Core.Models.Debugger
{
    /// <summary>
    /// Options for the OpenBullet 2 config debugger.
    /// </summary>
    public class DebuggerOptions
    {
        /// <summary>
        /// The data under test.
        /// </summary>
        public string TestData { get; set; } = "";

        /// <summary>
        /// The Wordlist Type to use when slicing the <see cref="TestData"/>.
        /// </summary>
        public string WordlistType { get; set; }

        /// <summary>
        /// Whether the provided <see cref="TestProxy"/> should be used.
        /// </summary>
        public bool UseProxy { get; set; } = false;

        /// <summary>
        /// The proxy to use for remote connections.
        /// </summary>
        public string TestProxy { get; set; } = "";

        /// <summary>
        /// The type of <see cref="TestProxy"/>.
        /// </summary>
        public ProxyType ProxyType { get; set; } = ProxyType.Http;

        /// <summary>
        /// Whether to persist the logs from the previous debug.
        /// </summary>
        public bool PersistLog { get; set; } = false;

        /// <summary>
        /// The list of variables that were found during the last debug.
        /// </summary>
        public List<Variable> Variables { get; set; } = new List<Variable>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        public DebuggerOptions(RuriLibSettingsService settings)
        {
            var type = settings.Environment.WordlistTypes.FirstOrDefault();

            if (type == null)
            {
                throw new NoWordlistTypesExceptions();
            }

            WordlistType = type.Name;
        }
    }
}

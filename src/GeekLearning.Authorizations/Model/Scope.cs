namespace GeekLearning.Authorizations.Model
{
    using System.Collections.Generic;

    public class Scope
    {
        /// <summary>
        /// Nom unique
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Libellé
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Liste des périmètre dont le perimètre courrant hérite
        /// </summary>
        public ICollection<string> Parents => new HashSet<string>();
    }
}

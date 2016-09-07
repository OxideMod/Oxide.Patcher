namespace OxidePatcher
{
    public class ProjectConfiguration
    {
        //TODO: Create an AssembliesOutputDirectory and add support for outputing to a different directory
        // than the source.

        /// <summary>
        /// The directory where the patcher will locate the assemblies.
        /// </summary>
        public string AssembliesSourceDirectory { get; set; }
    }
}

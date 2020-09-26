namespace Yoakke.Lir.Backend
{
    /// <summary>
    /// The output that should be produced.
    /// </summary>
    public enum OutputKind
    {
        Ir,
        Assembly,
        Object,
        Executable,
        StaticLibrary,
        DynamicLibrary,
    }
}

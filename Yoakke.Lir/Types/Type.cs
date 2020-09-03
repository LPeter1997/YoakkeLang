namespace Yoakke.Lir.Types
{
    /// <summary>
    /// Base for every type.
    /// </summary>
    public abstract partial record Type 
    {
        public static readonly Void Void_ = new Void();
        public static readonly Int U32 = new Int(false, 32);
        public static readonly Int I32 = new Int(true, 32);

        public abstract override string ToString();
    }
}

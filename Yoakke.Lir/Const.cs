using System;
using Yoakke.Lir.Status;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir
{
    /// <summary>
    /// Some read-only constant embedded in the <see cref="Assembly"/>.
    /// </summary>
    public class Const : Value, ISymbol, IValidate
    {
        public override Type Type => new Type.Ptr(Value.Type);
        public string Name { get; }
        public Visibility Visibility { get; set; } = Visibility.Private;
        /// <summary>
        /// The constant <see cref="Value"/>.
        /// </summary>
        public readonly Value Value;

        /// <summary>
        /// The actual value <see cref="Type"/> of the <see cref="Const"/>.
        /// </summary>
        public Type UnderlyingType => Value.Type;

        /// <summary>
        /// Initializes a new <see cref="Const"/>.
        /// </summary>
        /// <param name="name">The name of the constant.</param>
        /// <param name="value">The <see cref="Value"/> of the constant.</param>
        public Const(string name, Value value)
        {
            Name = name;
            Value = value;
        }

        public override string ToValueString() => Name;
        public override string ToString() => 
            $"const {UnderlyingType.ToTypeString()} {Name} = {Value} [visibility = {Visibility}]";
        public override bool Equals(Value? other) => ReferenceEquals(this, other);
        public override int GetHashCode() => HashCode.Combine(typeof(Const), Name);
        // NOTE: Makes no sense to clone this
        public override Value Clone() => this;

        public void Validate(BuildStatus status)
        {
            if (Value is Register)
            {
                status.Report(new ValidationError(this, "A constant can't reference a register!"));
            }
        }
    }
}

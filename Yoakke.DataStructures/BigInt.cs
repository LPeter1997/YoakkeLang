using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Yoakke.DataStructures
{
    /// <summary>
    /// A fixed-size <see cref="BigInteger"/> reimplementation.
    /// This is to have fixed padding and such. Works with two's complement.
    /// </summary>
    public struct BigInt
    {
        private int width;
        private byte[] bytes;

        /// <summary>
        /// True, if the number should be handled as signed.
        /// </summary>
        public bool IsSigned { get; set; }

        /// <summary>
        /// True, if this number is even.
        /// </summary>
        public bool IsEven => bytes[0] % 2 == 0;
        /// <summary>
        /// True, if this number is odd.
        /// </summary>
        public bool IsOdd => !IsEven;
        /// <summary>
        /// True, if this number is 0.
        /// </summary>
        public bool IsZero => bytes.All(b => b == 0);
        /// <summary>
        /// The sign bit (MSB). True, if negative.
        /// </summary>
        public bool Sign 
        {
            get => this[width - 1];
            set => this[width - 1] = value;
        }

        /// <summary>
        /// Bitwise access.
        /// </summary>
        public bool this[int index]
        {
            get => (bytes[index / 8] >> (index % 8)) % 2 != 0;
            set
            {
                if (value) bytes[index / 8] |= (byte)(1 << (index % 8));
                else bytes[index / 8] &= (byte)~(1 << (index % 8));
            }
        }

        /// <summary>
        /// Initializes a new <see cref="BigInt"/> to zero.
        /// </summary>
        /// <param name="signed">True, if the number should be interpreted as signed.</param>
        /// <param name="width">The width of the integer in bits.</param>
        public BigInt(bool signed, int width)
        {
            int bytesWidth = (width + 7) / 8;
            this.width = width;
            bytes = new byte[bytesWidth];
            IsSigned = signed;
        }

        public BigInt(bool signed, int width, ReadOnlySpan<byte> byteSpan)
            : this(signed, width)
        {
            var bytes = byteSpan.ToArray();
            Array.Copy(bytes, this.bytes, Math.Min(bytes.Length, this.bytes.Length));
        }

        /// <summary>
        /// Initializes a new signed <see cref="BigInt"/>.
        /// </summary>
        /// <param name="signed">True, if the number should be interpreted as signed.</param>
        /// <param name="width">The width of the integer in bits.</param>
        /// <param name="value">The initial value.</param>
        public BigInt(bool signed, int width, Int64 value)
            : this(signed, width, BitConverter.GetBytes(value))
        {
        }

        /// <summary>
        /// Initializes a new unsigned <see cref="BigInt"/>.
        /// </summary>
        /// <param name="signed">True, if the number should be interpreted as signed.</param>
        /// <param name="width">The width of the integer in bits.</param>
        /// <param name="value">The initial value.</param>
        public BigInt(bool signed, int width, UInt64 value)
            : this(signed, width, BitConverter.GetBytes(value))
        {
        }

        public ReadOnlyMemory<byte> AsMemory() => bytes;
        public ReadOnlySpan<byte> AsSpan() => bytes;

        // Conversions

        public static explicit operator sbyte (BigInt i) => (sbyte) i.ToBigInteger(true );
        public static explicit operator byte  (BigInt i) => (byte)  i.ToBigInteger(false);
        public static explicit operator short (BigInt i) => (short) i.ToBigInteger(true );
        public static explicit operator ushort(BigInt i) => (ushort)i.ToBigInteger(false);
        public static explicit operator int   (BigInt i) => (int)   i.ToBigInteger(true );
        public static explicit operator uint  (BigInt i) => (uint)  i.ToBigInteger(false);
        public static explicit operator long  (BigInt i) => (long)  i.ToBigInteger(true );
        public static explicit operator ulong (BigInt i) => (ulong) i.ToBigInteger(false);

        /*
        public static implicit operator BigInt(sbyte i)  => new BigInt(true, 8, i);
        public static implicit operator BigInt(byte i)   => new BigInt(false, 8, i);
        public static implicit operator BigInt(short i)  => new BigInt(true, 16, i);
        public static implicit operator BigInt(ushort i) => new BigInt(false, 16, i);
        public static implicit operator BigInt(int i)    => new BigInt(true, 32, i);
        public static implicit operator BigInt(uint i)   => new BigInt(false, 32, i);
        public static implicit operator BigInt(long i)   => new BigInt(true, 64, i);
        public static implicit operator BigInt(ulong i)  => new BigInt(false, 64, i);
        */

        /// <summary>
        /// Returns a <see cref="BigInt"/> with all 1 bytes.
        /// </summary>
        /// <param name="signed">True, if should be signed.</param>
        /// <param name="width">The width of the requested <see cref="BigInt"/>.</param>
        /// <returns>A <see cref="BigInt"/> with all 1s.</returns>
        public static BigInt AllOnes(bool signed, int width) => new BigInt(signed, width, -1);

        /// <summary>
        /// Returns the largest <see cref="BigInt"/> possible with the given width.
        /// </summary>
        /// <param name="signed">True, if should be signed.</param>
        /// <param name="width">The width of the requested <see cref="BigInt"/>.</param>
        /// <returns>The largest <see cref="BigInt"/> possible with the given width and signedness.</returns>
        public static BigInt MaxValue(bool signed, int width)
        {
            var result = AllOnes(signed, width);
            if (signed) result.Sign = false;
            return result;
        }

        /// <summary>
        /// Returns the smallest <see cref="BigInt"/> possible with the given width.
        /// </summary>
        /// <param name="signed">True, if should be signed.</param>
        /// <param name="width">The width of the requested <see cref="BigInt"/>.</param>
        /// <returns>The smallest <see cref="BigInt"/> possible with the given width and signedness.</returns>
        public static BigInt MinValue(bool signed, int width)
        {
            var result = new BigInt(signed, width);
            if (signed) result.Sign = true;
            return result;
        }

        /// <summary>
        /// Returns the one's complement of the <see cref="BigInt"/>.
        /// </summary>
        public static BigInt operator ~(BigInt bigInt)
        {
            var result = new BigInt(bigInt.IsSigned, bigInt.width, bigInt.bytes);
            for (int i = 0; i < result.bytes.Length; ++i)
            {
                result.bytes[i] = (byte)~result.bytes[i];
            }
            return result;
        }

        /// <summary>
        /// Returns the two's complement of the <see cref="BigInt"/>.
        /// </summary>
        public static BigInt operator -(BigInt bigInt) =>
            ~bigInt + new BigInt(bigInt.IsSigned, bigInt.width, 1);

        /// <summary>
        /// Adds two <see cref="BigInt"/>s.
        /// </summary>
        public static BigInt operator +(BigInt lhs, BigInt rhs) => Add(lhs, rhs, out var _);

        /// <summary>
        /// Subtracts one <see cref="BigInt"/> from another.
        /// </summary>
        public static BigInt operator -(BigInt lhs, BigInt rhs) => Subtract(lhs, rhs, out var _);

        /// <summary>
        /// Multiplies two <see cref="BigInt"/>s.
        /// </summary>
        public static BigInt operator *(BigInt lhs, BigInt rhs) => Multiply(lhs, rhs, out var _);

        /// <summary>
        /// Divides two <see cref="BigInt"/>s.
        /// </summary>
        public static BigInt operator /(BigInt lhs, BigInt rhs) => Divide(lhs, rhs, out var _);

        /// <summary>
        /// Remainder of the division of two <see cref="BigInt"/>s.
        /// </summary>
        public static BigInt operator %(BigInt lhs, BigInt rhs)
        {
            Divide(lhs, rhs, out var rem);
            return rem;
        }

        /// <summary>
        /// Bitwise and.
        /// </summary>
        public static BigInt operator &(BigInt lhs, BigInt rhs)
        {
            AssertSameWidthAndSignedness(lhs, rhs);
            var result = new BigInt(lhs.IsSigned, lhs.width);
            for (int i = 0; i < result.bytes.Length; ++i)
            {
                result.bytes[i] = (byte)(lhs.bytes[i] & rhs.bytes[i]);
            }
            return result;
        }

        /// <summary>
        /// Bitwise or.
        /// </summary>
        public static BigInt operator |(BigInt lhs, BigInt rhs)
        {
            AssertSameWidthAndSignedness(lhs, rhs);
            var result = new BigInt(lhs.IsSigned, lhs.width);
            for (int i = 0; i < result.bytes.Length; ++i)
            {
                result.bytes[i] = (byte)(lhs.bytes[i] | rhs.bytes[i]);
            }
            return result;
        }

        /// <summary>
        /// Bitwise xor.
        /// </summary>
        public static BigInt operator ^(BigInt lhs, BigInt rhs)
        {
            AssertSameWidthAndSignedness(lhs, rhs);
            var result = new BigInt(lhs.IsSigned, lhs.width);
            for (int i = 0; i < result.bytes.Length; ++i)
            {
                result.bytes[i] = (byte)(lhs.bytes[i] ^ rhs.bytes[i]);
            }
            return result;
        }

        /// <summary>
        /// Bitshift left.
        /// </summary>
        public static BigInt operator <<(BigInt lhs, int amount)
        {
            var result = new BigInt(lhs.IsSigned, lhs.width);
            int byteShift = amount / 8;
            int bitShift = amount % 8;
            int shiftBack = 8 - bitShift;
            byte topMask = (byte)(0xff << shiftBack);
            // Byte-shift
            Array.Copy(lhs.bytes, 0, result.bytes, byteShift, result.bytes.Length - byteShift);
            // Bit-shift
            byte carry = 0;
            for (int i = 0; i < result.bytes.Length; ++i)
            {
                var nextCarry = (byte)((result.bytes[i] & topMask) >> shiftBack);
                result.bytes[i] = (byte)((result.bytes[i] << bitShift) | carry);
                carry = nextCarry;
            }
            return result;
        }

        /// <summary>
        /// Bitshift right.
        /// </summary>
        public static BigInt operator >>(BigInt lhs, int amount)
        {
            var result = new BigInt(lhs.IsSigned, lhs.width);
            int byteShift = amount / 8;
            int bitShift = amount % 8;
            int shiftFront = 8 - bitShift;
            byte bottomMask = (byte)(0xff >> shiftFront);
            // Byte-shift
            Array.Copy(lhs.bytes, byteShift, result.bytes, 0, result.bytes.Length - byteShift);
            // Bit-shift
            byte carry = 0;
            for (int i = result.bytes.Length - 1; i >= 0; --i)
            {
                var nextCarry = (byte)((result.bytes[i] & bottomMask) << shiftFront);
                result.bytes[i] = (byte)((result.bytes[i] >> bitShift) | carry);
                carry = nextCarry;
            }
            return result;
        }

        /// <summary>
        /// Equality comparison.
        /// </summary>
        public static bool operator ==(BigInt lhs, BigInt rhs)
        {
            AssertSameWidthAndSignedness(lhs, rhs);
            return lhs.bytes.SequenceEqual(rhs.bytes);
        }

        /// <summary>
        /// Inequality comparison.
        /// </summary>
        public static bool operator !=(BigInt lhs, BigInt rhs) => !(lhs == rhs);

        /// <summary>
        /// Less-than comparison.
        /// </summary>
        public static bool operator <(BigInt lhs, BigInt rhs)
        {
            AssertSameWidthAndSignedness(lhs, rhs);
            if (lhs.IsSigned)
            {
                if (lhs.Sign && !rhs.Sign) return true;
                if (!lhs.Sign && rhs.Sign) return false;
            }
            for (int i = lhs.bytes.Length - 1; i >= 0; --i)
            {
                if (lhs.bytes[i] < rhs.bytes[i]) return true;
                if (lhs.bytes[i] > rhs.bytes[i]) return false;
            }
            return false;
        }

        /// <summary>
        /// Greater-than comparison.
        /// </summary>
        public static bool operator >(BigInt lhs, BigInt rhs) => rhs < lhs;

        /// <summary>
        /// Less-than or equals comparison.
        /// </summary>
        public static bool operator <=(BigInt lhs, BigInt rhs) => !(lhs > rhs);

        /// <summary>
        /// Greater-than or equals comparison.
        /// </summary>
        public static bool operator >=(BigInt lhs, BigInt rhs) => !(lhs < rhs);

        /// <summary>
        /// Subtracts two <see cref="BigInt"/>s.
        /// </summary>
        /// <param name="lhs">The left-hand side operand.</param>
        /// <param name="rhs">The right-hand side operand.</param>
        /// <param name="overflow">Output variable to detect underflow.</param>
        /// <returns>The result of the subtraction.</returns>
        public static BigInt Subtract(BigInt lhs, BigInt rhs, out bool underflow) =>
            Add(lhs, -rhs, out underflow);

        // TODO: This overflow detection is incorrect for non-byte-multiple sized numbers!
        // Maybe it's even incorrect in the general case?
        /// <summary>
        /// Adds two <see cref="BigInt"/>s.
        /// </summary>
        /// <param name="lhs">The left-hand side operand.</param>
        /// <param name="rhs">The right-hand side operand.</param>
        /// <param name="overflow">Output variable to detect overflow.</param>
        /// <returns>The result of the addition.</returns>
        public static BigInt Add(BigInt lhs, BigInt rhs, out bool overflow)
        {
            AssertSameWidthAndSignedness(lhs, rhs);
            var result = new BigInt(lhs.IsSigned, lhs.width);
            byte carry = 0;
            for (int i = 0; i < result.bytes.Length; ++i)
            {
                unchecked
                {
                    result.bytes[i] = (byte)(lhs.bytes[i] + rhs.bytes[i]);
                    var maxByte = Math.Max(lhs.bytes[i], rhs.bytes[i]);
                    if (result.bytes[i] < maxByte)
                    {
                        // Overflow happened
                        result.bytes[i] += carry;
                        carry = 1;
                    }
                    else
                    {
                        result.bytes[i] += carry;
                        carry = 0;
                        if (result.bytes[i] < maxByte)
                        {
                            // Overflow happened
                            carry = 1;
                        }
                    }
                }
            }
            overflow = carry == 1;
            return result;
        }

        /// <summary>
        /// Multiplies two <see cref="BigInt"/>s.
        /// </summary>
        /// <param name="lhs">The left-hand side operand.</param>
        /// <param name="rhs">The right-hand side operand.</param>
        /// <param name="overflow">Output variable to detect overflow.</param>
        /// <returns>The result of the multiplication.</returns>
        public static BigInt Multiply(BigInt lhs, BigInt rhs, out bool overflow)
        {
            AssertSameWidthAndSignedness(lhs, rhs);
            // Russian peasant's method
            var result = new BigInt(lhs.IsSigned, lhs.width);
            overflow = false;
            while (!rhs.IsZero)
            {
                if (rhs.IsOdd)
                {
                    result = Add(result, lhs, out bool o);
                    overflow = overflow || o;
                }
                lhs <<= 1;
                rhs >>= 1;
            }
            return result;
        }

        /// <summary>
        /// Divides two <see cref="BigInt"/>s.
        /// </summary>
        /// <param name="lhs">The left-hand side operand.</param>
        /// <param name="rhs">The right-hand side operand.</param>
        /// <param name="remainder">Output variable for the remainder.</param>
        /// <returns>The result of the division.</returns>
        public static BigInt Divide(BigInt lhs, BigInt rhs, out BigInt remainder)
        {
            AssertSameWidthAndSignedness(lhs, rhs);
            // Binary long division
            if (rhs.IsZero) throw new DivideByZeroException();

            bool numSign = lhs.Sign;
            var sign = lhs.Sign != rhs.Sign;
            if (lhs.IsSigned)
            {
                if (lhs.Sign) lhs = -lhs;
                if (rhs.Sign) rhs = -rhs;
            }
            var quotient = new BigInt(lhs.IsSigned, lhs.width);
            remainder = new BigInt(lhs.IsSigned, lhs.width);

            for (int i = lhs.width - 1; i >= 0; --i)
            {
                remainder <<= 1;
                remainder[0] = lhs[i];
                if (remainder >= rhs)
                {
                    remainder -= rhs;
                    quotient[i] = true;
                }
            }
            if (lhs.IsSigned)
            {
                if (sign) quotient = -quotient;
                if (numSign) remainder = -remainder;
            }
            return quotient;
        }

        private static void AssertSameWidthAndSignedness(BigInt lhs, BigInt rhs)
        {
            if (lhs.width != rhs.width || lhs.IsSigned != rhs.IsSigned)
            {
                throw new ArgumentException("The widths or signedness don't match!");
            }
        }

        /// <summary>
        /// Converts this <see cref="BigInt"/> to a <see cref="BigInteger"/>.
        /// </summary>
        /// <param name="signed">True, if the result should be interpreted as signed.</param>
        public BigInteger ToBigInteger(bool signed) => new BigInteger(bytes, !signed);

        public override string ToString() => ToBigInteger(IsSigned).ToString();

        public override bool Equals(object? obj) => obj is BigInt bi && this == bi;
        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var b in bytes) hash.Add(b);
            return hash.ToHashCode();
        }

        public bool TryWriteBytes(Span<byte> destination, out int bytesWritten)
        {
            var bytes = AsSpan();
            var minSize = Math.Min(destination.Length, bytes.Length);
            for (int i = 0; i < minSize; ++i)
            {
                destination[i] = bytes[i];
            }
            bytesWritten = minSize;
            return destination.Length >= bytes.Length;
        }
    }
}

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
    /// A fixed-size <see cref="System.Numerics.BigInteger"/> reimplementation.
    /// This is to have fixed padding and such. Works with two's complement.
    /// </summary>
    public struct BigInt
    {
        private int width;
        private byte[] bytes;

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
        /// The sign bit. True, if negative.
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
                if (value)
                {
                    bytes[index / 8] |= (byte)(1 << (index % 8));
                }
                else
                {
                    bytes[index / 8] &= (byte)~(1 << (index % 8));
                }
            }
        }

        /// <summary>
        /// Initializes a new <see cref="BigInt"/> to zero.
        /// </summary>
        /// <param name="width">The width of the integer in bits.</param>
        public BigInt(int width)
        {
            int bytesWidth = (width + 7) / 8;
            this.width = width;
            bytes = new byte[bytesWidth];
        }

        private BigInt(int width, byte[] bytes)
            : this(width)
        {
            Array.Copy(bytes, this.bytes, Math.Min(bytes.Length, this.bytes.Length));
        }

        /// <summary>
        /// Initializes a new <see cref="BigInt"/>.
        /// </summary>
        /// <param name="width">The width of the integer in bits.</param>
        /// <param name="value">The initial value.</param>
        public BigInt(int width, Int64 value)
            : this(width, BitConverter.GetBytes(value))
        {
        }

        /// <summary>
        /// Initializes a new <see cref="BigInt"/>.
        /// </summary>
        /// <param name="width">The width of the integer in bits.</param>
        /// <param name="value">The initial value.</param>
        public BigInt(int width, UInt64 value)
            : this(width, BitConverter.GetBytes(value))
        {
        }

        /// <summary>
        /// Returns a <see cref="BigInt"/> with all 1 bytes.
        /// </summary>
        /// <param name="width">The width of the requested <see cref="BigInt"/>.</param>
        /// <returns>A <see cref="BigInt"/> with all 1s.</returns>
        public static BigInt AllOnes(int width) => new BigInt(width, -1);

        /// <summary>
        /// Returns the largest <see cref="BigInt"/> possible with the given width.
        /// </summary>
        /// <param name="width">The width of the requested <see cref="BigInt"/>.</param>
        /// <param name="signed">True, if should be signed.</param>
        /// <returns>The largest <see cref="BigInt"/> possible with the given width and signedness.</returns>
        public static BigInt MaxValue(int width, bool signed)
        {
            var result = AllOnes(width);
            if (signed) result.Sign = false;
            return result;
        }

        /// <summary>
        /// Returns the smallest <see cref="BigInt"/> possible with the given width.
        /// </summary>
        /// <param name="width">The width of the requested <see cref="BigInt"/>.</param>
        /// <param name="signed">True, if should be signed.</param>
        /// <returns>The smallest <see cref="BigInt"/> possible with the given width and signedness.</returns>
        public static BigInt MinValue(int width, bool signed)
        {
            var result = new BigInt(width);
            if (signed) result.Sign = true;
            return result;
        }

        /// <summary>
        /// Returns the one's complement of the <see cref="BigInt"/>.
        /// </summary>
        public static BigInt operator ~(BigInt bigInt)
        {
            var result = new BigInt(bigInt.width, bigInt.bytes);
            for (int i = 0; i < result.bytes.Length; ++i)
            {
                result.bytes[i] = (byte)~result.bytes[i];
            }
            return result;
        }

        /// <summary>
        /// Returns the two's complement of the <see cref="BigInt"/>.
        /// </summary>
        public static BigInt operator -(BigInt bigInt) => ~bigInt + new BigInt(bigInt.width, 1);

        /// <summary>
        /// Subtracts one <see cref="BigInt"/> from another.
        /// </summary>
        public static BigInt operator -(BigInt lhs, BigInt rhs) => Subtract(lhs, rhs, out var _);

        /// <summary>
        /// Adds two <see cref="BigInt"/>s.
        /// </summary>
        public static BigInt operator +(BigInt lhs, BigInt rhs) => Add(lhs, rhs, out var _);

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
            EnsureSameWidth(lhs, rhs);
            var result = new BigInt(lhs.width);
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
            EnsureSameWidth(lhs, rhs);
            var result = new BigInt(lhs.width);
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
            EnsureSameWidth(lhs, rhs);
            var result = new BigInt(lhs.width);
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
            var result = new BigInt(lhs.width);
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
            var result = new BigInt(lhs.width);
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
            EnsureSameWidth(lhs, rhs);
            return lhs.bytes.SequenceEqual(rhs.bytes);
        }

        /// <summary>
        /// Inequality comparison.
        /// </summary>
        public static bool operator !=(BigInt lhs, BigInt rhs) => !(lhs == rhs);

        /// <summary>
        /// Greater-than comparison.
        /// </summary>
        public static bool operator >(BigInt lhs, BigInt rhs)
        {
            EnsureSameWidth(lhs, rhs);
            for (int i = lhs.bytes.Length - 1; i >= 0; --i)
            {
                if (lhs.bytes[i] > rhs.bytes[i]) return true;
                if (lhs.bytes[i] < rhs.bytes[i]) return false;
            }
            return false;
        }

        /// <summary>
        /// Greater-than or equals comparison.
        /// </summary>
        public static bool operator >=(BigInt lhs, BigInt rhs)
        {
            EnsureSameWidth(lhs, rhs);
            for (int i = lhs.bytes.Length - 1; i >= 0; --i)
            {
                if (lhs.bytes[i] > rhs.bytes[i]) return true;
                if (lhs.bytes[i] < rhs.bytes[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// Less-than or equals comparison.
        /// </summary>
        public static bool operator <=(BigInt lhs, BigInt rhs) => rhs >= lhs;

        /// <summary>
        /// Less-than comparison.
        /// </summary>
        public static bool operator <(BigInt lhs, BigInt rhs) => rhs > lhs;

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
            EnsureSameWidth(lhs, rhs);
            var result = new BigInt(lhs.width);
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
            EnsureSameWidth(lhs, rhs);
            // Russian peasant's method
            var result = new BigInt(lhs.width);
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
            EnsureSameWidth(lhs, rhs);
            // Binary long division
            if (rhs.IsZero) throw new DivideByZeroException();

            var quotient = new BigInt(lhs.width);
            remainder = new BigInt(lhs.width);

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
            return quotient;
        }

        private static void EnsureSameWidth(BigInt lhs, BigInt rhs)
        {
            if (lhs.width != rhs.width)
            {
                throw new ArgumentException("The widths don't match!");
            }
        }

        /// <summary>
        /// Converts this <see cref="BigInt"/> to a <see cref="BigInteger"/>.
        /// </summary>
        /// <param name="signed">True, if the result should be a signed integer.</param>
        /// <returns>The <see cref="BigInteger"/>.</returns>
        public BigInteger ToBigInteger(bool signed = true) => new BigInteger(bytes, !signed);

        public override string ToString() => ToString(true);

        /// <summary>
        /// Returns the string representation of this <see cref="BigInt"/>.
        /// </summary>
        /// <param name="signed">True, if should be interpreted as a signed integer.</param>
        /// <returns>The string representation.</returns>
        public string ToString(bool signed) => ToBigInteger(signed).ToString();

        public override bool Equals(object? obj) => obj is BigInt bi && this == bi;
        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var b in bytes) hash.Add(b);
            return hash.ToHashCode();
        }
    }
}

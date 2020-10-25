using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Yoakke.DataStructures
{
    /// <summary>
    /// A buffer type that can read a sequence and peek arbitrary amounts forward.
    /// </summary>
    /// <typeparam name="T">The element type of the buffer.</typeparam>
    public class PeekBuffer<T>
    {
        /// <summary>
        /// The underlying buffer.
        /// Guaranteed to hold all the peeked elements since the last consumption.
        /// </summary>
        public IReadOnlyList<T> Buffer => buffer;
        /// <summary>
        /// True, if the buffer is out of elements to consume.
        /// </summary>
        public bool IsEnd => Buffer.Count == 0 && sourceEnded;

        private List<T> buffer = new List<T>();
        private IEnumerator<T> source;
        private bool sourceEnded;
        private bool hasLastConsumed;
        private T? lastConsumed;

        private PeekBuffer(IEnumerator<T> source)
        {
            this.source = source;
            sourceEnded = !source.MoveNext();
        }

        /// <summary>
        /// Initializes a new <see cref="PeekBuffer{T}"/>.
        /// </summary>
        /// <param name="elements">The element source.</param>
        public PeekBuffer(IEnumerable<T> elements)
            : this(elements.GetEnumerator())
        {
        }

        /// <summary>
        /// Inserts elements to the beginnning of the buffer.
        /// </summary>
        /// <param name="elements">The elements to insert.</param>
        public void PushFront(IEnumerable<T> elements) => buffer.InsertRange(0, elements);

        /// <summary>
        /// Consumes the next element in the buffer.
        /// </summary>
        /// <returns>The consumed element.</returns>
        public T Consume()
        {
            var value = Peek();
            Consume(1);
            return value;
        }

        /// <summary>
        /// Tries to consume the next element in the buffer.
        /// </summary>
        /// <param name="value">The consumed value.</param>
        /// <returns>True, if there was a value to consume in the buffer.</returns>
        public bool TryConsume([MaybeNullWhen(false)] out T value)
        {
            if (TryPeek(out value, 0))
            {
                Consume(1);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Consumes a given amount from the source, meaning they can't be peeked anymore.
        /// </summary>
        /// <param name="amount">The amount to consume.</param>
        /// <returns>The amount that was actually consumed. This can be less than the given amount, if the
        /// buffer was out of elements.</returns>
        public int Consume(int amount)
        {
            if (amount > 0) TryPeek(out var _, amount - 1);
            amount = Math.Min(amount, Buffer.Count);
            if (amount > 0)
            {
                lastConsumed = buffer[amount - 1];
                hasLastConsumed = true;
            }
            buffer.RemoveRange(0, amount);
            return amount;
        }

        /// <summary>
        /// Peeks an element in the buffer.
        /// </summary>
        /// <param name="amount">The amount to peek forward. 0 is the next element.</param>
        /// <returns>The peeked element.</returns>
        public T Peek(int amount = 0)
        {
            if (!TryPeek(out var peeked, amount))
            {
                throw new InvalidOperationException("The buffer had no element to peek!");
            }
            return peeked;
        }

        /// <summary>
        /// Peeks an element in the buffer and returns the peeked element, or a default one,
        /// if the buffer contains no element to peek.
        /// </summary>
        /// <param name="amount">The amount to peek forward. 0 is the next element.</param>
        /// <param name="default">The default element to return if there's no element to peek.</param>
        /// <returns>The peeked element, or the default.</returns>
        public T? PeekOrDefault(int amount = 0, T @default = default)
        {
            if (TryPeek(out var peeked, amount)) return peeked;
            return @default;
        }

        /// <summary>
        /// Tries to peek a given amount forward in the source.
        /// </summary>
        /// <param name="value">The peeked value.</param>
        /// <param name="amount">The amount to peek forward. 0 is the next element.</param>
        /// <returns>True, if the source contained enough elements to peek the given amount forward.</returns>
        public bool TryPeek([MaybeNullWhen(false)] out T value, int amount = 0)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be a non-negative number!");
            }
            while (buffer.Count <= amount)
            {
                if (sourceEnded) break;
                var element = ReadNextInternal();
                Debug.Assert(element != null);
                buffer.Add(element);
            }
            if (buffer.Count <= amount)
            {
                value = default;
                return false;
            }
            else
            {
                value = buffer[amount];
                return true;
            }
        }

        /// <summary>
        /// Retrieves the last consumed element.
        /// </summary>
        /// <returns>The last consumed element.</returns>
        public T Prev()
        {
            if (!TryPrev(out var prev))
            {
                throw new InvalidOperationException("The buffer hasn't consumed any elements yet!");
            }
            return prev;
        }

        /// <summary>
        /// Retrieves the last consumed element or the default, if there was nothing consumed.
        /// </summary>
        /// <param name="default">The default to return in case there was no consumed element.</param>
        /// <returns>The last consumed or the default.</returns>
        public T? PrevOrDefault(T @default = default)
        {
            if (!TryPrev(out var prev)) return @default;
            return prev;
        }

        /// <summary>
        /// Tries to retrieve the last consumed element.
        /// </summary>
        /// <param name="value">The last consumed element.</param>
        /// <returns>True, if there was a last consumed element.</returns>
        public bool TryPrev([MaybeNullWhen(false)] out T value)
        {
            value = lastConsumed;
            return hasLastConsumed;
        }

        private T? ReadNextInternal()
        {
            if (sourceEnded) return default;
            var result = source.Current;
            sourceEnded = !source.MoveNext();
            return result;
        }
    }
}

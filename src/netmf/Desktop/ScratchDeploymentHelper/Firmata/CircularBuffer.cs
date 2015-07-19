//-------------------------------------------------------------------------
//  (c) 2015 Pervasive Digital LLC
//
//  This file is part of Scratch for .Net Micro Framework
//
//  "Scratch for .Net Micro Framework" is free software: you can 
//  redistribute it and/or modify it under the terms of the 
//  GNU General Public License as published by the Free Software 
//  Foundation, either version 3 of the License, or (at your option) 
//  any later version.
//
//  "Scratch for .Net Micro Framework" is distributed in the hope that
//  it will be useful, but WITHOUT ANY WARRANTY; without even the implied
//  warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
//  the GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with "Scratch for .Net Micro Framework". If not, 
//  see <http://www.gnu.org/licenses/>.
//
//  This file has also been distributed previously under an Apache 2.0
//  license and you may elect to use that license with this file. This 
//  does not affect the licensing of any other file in Scratch for
//  .Net Micro Framework.
//-------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.DeploymentHelper.Firmata
{
    internal class CircularBuffer<T> : ICollection where T : struct, System.IComparable<T>
    {
        // Capacity growth multiplier and constant. Capacity grows as _capacity * _growM + _growC
        private readonly int _growM;
        private readonly int _growC;

        // current capacity
        private int _capacity;
        // ring pointers
        private int _head;
        private int _tail;
        // current occupied space (count of Ts in the ring)
        private int _size;
        // the actual data
        private T[] _buffer;

        private object _syncRoot = new object();

        public CircularBuffer(int capacity, int growthMultiplier, int growthConstant)
        {
            if (capacity==0 || growthMultiplier==0 || (growthMultiplier==1 && growthConstant==0))
                throw new ArgumentException("capacity must be non-zero and 1*growthMultiplier+growthConstant must be non-zero");

            _size = 0;
            _head = 0;
            _tail = 0;
            _buffer = new T[capacity];
            _growM = growthMultiplier;
            _growC = growthConstant;
        }

        public int Capacity
        {
            get { return _capacity; }
            set
            {
                if (value == _capacity)
                    return;

                if (value < _size)
                    throw new ArgumentOutOfRangeException("value", "New capacity is smaller than current size");

                var dest = new T[value];
                if (_size > 0)
                    CopyTo(dest, 0);
                _buffer = dest;
                _capacity = value;
            }
        }

        public int Size
        {
            get { return _size; }
        }

        public void Clear()
        {
            _head = _tail = _size = 0;
        }

        public bool Contains(T item)
        {
            int idx = _head;
            for (int i = 0; i < _size; i++, idx++)
            {
                if (idx == _capacity)
                    idx = 0;

                if (_buffer[idx].CompareTo(item)==0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Find the first occurrence of a T in the buffer
        /// </summary>
        /// <param name="item">The value to search for</param>
        /// <returns>The offset of the found value, or -1 if not found</returns>
        public int IndexOf(T item)
        {
            int idx = _head;
            for (int i = 0; i < _size; i++, idx++)
            {
                if (idx == _capacity)
                    idx = 0;

                if (_buffer[idx].CompareTo(item)==0)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// A greedy sequence matcher that will match the first occurrence of seq in the circular buffer.  This routine
        /// returns the offset of the matched sequence or -1 if the sequence does not appear in the stream.
        /// </summary>
        /// <param name="seq">The sequence of Ts to search for</param>
        /// <returns>Offset of the first match, or -1 if not found</returns>
        public int IndexOf(T[] seq)
        {
            // can't have a match, so don't bother searching
            if (_size < seq.Length)
                return -1;

            int iOffsetFirst = -1;  // offset of first matched char
            int idxFirst = -1; // index of first matched char

            var idxSeq = 0;
            var lenSeq = seq.Length;
            int idx = _head;

            int iOffset = 0;
            while (iOffset < _size)
            {
                if (idx == _capacity)
                    idx = 0;

                if (_buffer[idx].CompareTo(seq[idxSeq])==0)
                {
                    // Mark where we found the first character so that we can restart the search there if the match fails,
                    //  or so that we can return the offset of the first matched char.
                    if (idxSeq == 0)
                    {
                        iOffsetFirst = iOffset;
                        idxFirst = idx;
                    }
                    // did we reach the end of the matching sequence?  If so, return the offset of the first char we matched.
                    if (++idxSeq >= lenSeq)
                        return iOffsetFirst;
                }
                else
                {
                    // mismatch - reset the search so that we pick up at the first char after the start of the current failed match
                    idxSeq = 0;
                    // if we had begun a match, pick up the search again on the first char after the start of the broken match candidate
                    if (idxFirst != -1)
                    {
                        iOffset = iOffsetFirst;
                        idx = idxFirst;
                        idxFirst = -1;
                        iOffsetFirst = -1;
                    }
                }
                ++iOffset;
                ++idx;
            }

            return -1;
        }

        public int Put(T[] src)
        {
            return Put(src, 0, src.Length);
        }

        public int Put(T[] src, int offset, int count)
        {
            if (count > _capacity - _size)
            {
                Grow(_size + count);
            }

            int srcIndex = offset;
            int segmentLength = count;
            if ((_capacity - _tail) < segmentLength)
                segmentLength = _capacity - _tail;

            // First segment
            Array.Copy(src, srcIndex, _buffer, _tail, segmentLength);
            _tail += segmentLength;
            if (_tail >= _capacity)
                _tail -= _capacity;

            // Optionally, a second segment
            srcIndex += segmentLength;
            segmentLength = count - segmentLength;
            if (segmentLength > 0)
            {
                Array.Copy(src, srcIndex, _buffer, _tail, segmentLength);
                _tail += segmentLength;
                if (_tail >= _capacity)
                    _tail -= _capacity;
            }

            _size = _size + count;
            return count;
        }

        public void Put(T b)
        {
            if (1 > _capacity - _size)
            {
                Grow(_size + 1);
            }

            if (_tail == _capacity)
                _tail = 0;
            _buffer[_tail++] = b;
            _size = _size + 1;
        }

        private void Grow(int target)
        {
            int newCapacity = _capacity;
            while (newCapacity < target)
            {
                newCapacity = (newCapacity * _growM) + _growC;
            }
            this.Capacity = newCapacity;
        }

        public void Skip(int count)
        {
            if (count > _size)
                throw new ArgumentOutOfRangeException("count", "Skip count:" + count + " Size:" + _size);

            _head += count;
            _size -= count;

            if (_head >= _capacity)
                _head -= _capacity;
        }

        public T[] Get(int count)
        {
            var dest = new T[count];
            Get(dest);
            return dest;
        }

        public int Get(T[] dst)
        {
            return Get(dst, 0, dst.Length);
        }

        public int Get(T[] dest, int offset, int count)
        {
            if (count > _size)
                throw new ArgumentOutOfRangeException("count","Requested items =" + count + " Available items=" + _size);
            int actualCount = System.Math.Min(count, _size);
            int dstIndex = offset;
            for (int i = 0; i < actualCount; i++, _head++, dstIndex++)
            {
                if (_head == _capacity)
                    _head = 0;
                dest[dstIndex] = _buffer[_head];
            }
            _size -= actualCount;
            return actualCount;
        }

        public T Get()
        {
            if (_size == 0)
                throw new InvalidOperationException("Empty");

            var item = _buffer[_head];
            if (++_head == _capacity)
                _head = 0;
            _size--;
            return item;
        }

        object ICollection.SyncRoot
        {
            get { return _syncRoot; }
        }

        public void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }

        public void CopyTo(Array array, int index)
        {
            CopyTo(0, (T[])array, index, _size);
        }

        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            if (count > _size)
                throw new ArgumentOutOfRangeException("count", "Count too large");

            int bufferIndex = _head;
            for (int i = 0; i < count; i++, bufferIndex++, arrayIndex++)
            {
                if (bufferIndex == _capacity)
                    bufferIndex = 0;
                array[arrayIndex] = _buffer[bufferIndex];
            }
        }
        int ICollection.Count
        {
            get { return _size; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        public IEnumerator GetEnumerator()
        {
            int bufferIndex = _head;
            for (int i = 0; i < _size; i++, bufferIndex++)
            {
                if (bufferIndex == _capacity)
                    bufferIndex = 0;

                yield return _buffer[bufferIndex];
            }
        }
    }
}

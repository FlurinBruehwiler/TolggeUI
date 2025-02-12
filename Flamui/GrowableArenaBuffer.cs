using System.Collections;

namespace Flamui;

//todo maybe increase chunk size over time like x1.5 or x2

/// <summary>
/// Linked list of chunks of size "chunkSize" allocated on an arena
/// </summary>
/// <typeparam name="T"></typeparam>
public unsafe class GrowableArenaBuffer<T> : IEnumerable<T> where T : unmanaged
{
    private readonly Arena _arena;
    private readonly int _chunkSize;

    private Chunk* _currentChunk;
    private readonly Chunk* _firstChunk;

    public int Count { get; private set; }

    public GrowableArenaBuffer(Arena arena, int chunkSize)
    {
        _arena = arena;
        _chunkSize = chunkSize;

        _firstChunk = AppendNewChunk();
    }

    public void Clear()
    {
        Count = 0;

        //set all counts to zero and zero initialize slices
        var current = _firstChunk;
        while (true)
        {
            current->Count = 0;
            current->Items.MemZero();
            if (current->NextChunk == null)
            {
                break;
            }

            current = current->NextChunk;
        }

        _currentChunk = _firstChunk;
    }

    public void Add(T item)
    {
        if (_currentChunk->IsFull())
        {
            if (_currentChunk->NextChunk != null)
            {
                _currentChunk = _currentChunk->NextChunk;
            }
            else
            {
                AppendNewChunk();
            }
        }

        _currentChunk->Items[_currentChunk->Count] = item;
        _currentChunk->Count++;
        Count++;
    }

    public Slice<T> ToSlice()
    {
        var slice = _arena.AllocateSlice<T>(Count);

        int idx = 0;

        var current = _firstChunk;
        while (true)
        {
            current->Items.Span.Slice(0, current->Count).CopyTo(slice.Span.Slice(idx));
            idx += current->Count;

            if (current->NextChunk == null)
            {
                break;
            }

            current = current->NextChunk;
        }

        return slice;
    }

    private Chunk* AppendNewChunk()
    {
        var c = new Chunk
        {
            Items = _arena.AllocateSlice<T>(_chunkSize),
            NextChunk = null
        };

        var newChunk = _arena.Allocate(c);

        if (_currentChunk != null)
        {
            _currentChunk->NextChunk = newChunk;
        }

        _currentChunk = newChunk;

        return newChunk;
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public struct Enumerator : IEnumerator<T>
    {
        public T Current => _current;

        private Chunk* _currentChunk;
        private int _indexInChunk;

        private readonly GrowableArenaBuffer<T> _buffer;
        object IEnumerator.Current => _current;

        private T _current;

        public Enumerator(GrowableArenaBuffer<T> buffer)
        {
            _buffer = buffer;
            _currentChunk = _buffer._firstChunk;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (_indexInChunk >= _buffer._chunkSize)
            {
                if (_currentChunk->NextChunk == null)
                {
                    _current = default;
                    return false;
                }

                _currentChunk = _currentChunk->NextChunk;
            }

            if (_indexInChunk >= _currentChunk->Count)
            {
                _current = default;
                return false;
            }

            _current = _currentChunk->Items[_indexInChunk];
            _indexInChunk++;
            return true;
        }

        public void Reset()
        {
            _current = default;
            _indexInChunk = 0;
        }
    }

    struct Chunk
    {
        public Chunk* NextChunk;
        public Slice<T> Items;
        public int Count;

        public bool IsFull()
        {
            return Count >= Items.Count;
        }
    }
}
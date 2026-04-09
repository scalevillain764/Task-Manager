using System.ComponentModel;

namespace m_list // не буду им пользоваться, хотя изначально планировалось
{
    public class m_list<T>
    {
        public T[] buffer;
        public int size { get; private set; }
        public int capacity { get; private set; }

        public m_list()
        {
            size = 0;
            capacity = 10;
            buffer = new T[10];
        }

        public m_list(int capacity)
        {
            this.capacity = capacity;
            size = 0;
            buffer = new T[capacity];
        }

        public m_list(int size, T value)
        {
            this.size = size;
            capacity = size * 2;
            buffer = new T[capacity];
            for(int i = 0; i < size; i++)
            {
                buffer[i] = value;
            }
        }

        public List<T> ToList()
        {
            List<T> rez = new List<T>();
            for(int i = 0; i < size; i++)
            {
                rez.Add(buffer[i]);
            }
            return rez;
        }

        public void AddLast(T value)
        {
            if(size >= capacity)
            {
                capacity *= 2;
                T[] values = new T[capacity];
                Array.Copy(buffer, values, size);
                buffer = values;
            }
            buffer[size++] = value;
        } 

        public m_list<T> Dcopy(Func<T, T> copier) // deep copy
        {
            m_list<T> rez = new m_list<T>(capacity);
            for(int i = 0; i < size; i++)
            {
                rez.AddLast(copier(buffer[i]));
            }
            rez.size = size;
            return rez;
        } 

        public void AddBefore(T value)
        {
            if (size >= capacity)
            {
                capacity *= 2;
                T[] values = new T[capacity];
                Array.Copy(buffer, values, size);
                buffer = values;
            }

            for(int i = size; i >= 1; i--)
            {
                buffer[i] = buffer[i - 1];
            }
            buffer[0] = value;
            size++;
        }

        public T this[int index]
        {
            get => buffer[index];
            set => buffer[index] = value;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= size) return;
            for(int i = index; i < size - 1; i++)
            {
                buffer[i] = buffer[i + 1];
            }
            size--;
        }

        public void Remove(T value)
        {
            for(int i = 0; i < size; i++)
            {
                if (EqualityComparer<T>.Default.Equals(buffer[i], value))
                {
                    for(int j = i; j < size - 1; j++)
                    {
                        buffer[j] = buffer[j + 1];
                    }
                    size--;
                    break;
                }
            }
        }

    }
}
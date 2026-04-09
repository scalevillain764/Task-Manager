using m_list;
namespace _hash_table
{
    public class Node<T>
    {
        public int Key { get; private set; }
        public T value;
        public Node<T>? next;
        public Node(int key, T value)
        {
            Key = key;
            this.value = value;
            next = null;
        }
    }

    public class HashTable <T>
    {
        public m_list<Node<T>> buffer;
        public int size { get; private set; }
        public int capacity { get; private set; }
   
        public List<T> GetAll()
        {
            List<T> new_list = new List<T>();
            for(int i = 0; i < capacity; i++)
            {
                Node<T> cur = buffer[i];
                while(cur != null)
                {
                    new_list.Add(cur.value);
                    cur = cur.next;
                }
            }
            return new_list;
        }

        private void ReHash() {
            int old_size = size;
            int old_capacity = capacity;
            capacity *= 2;
            m_list<Node<T>> new_buffer = new m_list<Node<T>>(capacity);
            for(int i = 0; i < old_capacity; i++)
            {
                Node<T> cur = buffer[i];
                while(cur != null)
                {
                    int index = hash(cur.Key);
                    Node<T> new_node = new Node<T>(index, cur.value);
                    if (new_buffer.buffer[index] == null)
                        new_buffer.buffer[index] = new_node;
                    else
                    {
                        new_node.next = new_buffer.buffer[index];
                        new_buffer.buffer[index] = new_node;
                    }
                    cur = cur.next;
                }
            }
            buffer = new_buffer;
        }

        public bool empty() => size == 0;

        private int hash (int key)
        {
            return key.GetHashCode() % capacity;
        }
        public HashTable(int cap)
        {
            buffer = new m_list<Node<T>>(cap);
            size = 0;
            capacity = cap;
        }
        
        public Node<T> this[int index]
        {
            get => buffer[index];
            set => buffer[index] = value;
        }

        public HashTable()
        {
            buffer = new m_list<Node<T>>();
            size = 0;
            capacity = buffer.capacity;
        }

        public void Add(int k, T value)
        {
            int key = hash(k);

            Node<T> new_node = new Node<T>(key, value);
            if (buffer[key] != null)
            {
                new_node.next = buffer[key];
                buffer[key] = new_node;
            }
            else buffer[key] = new_node;

            size++;

            if((double) size / capacity > 0.7) ReHash();
        }

        public T find(int id)
        {
            int key = hash(id);
            Node<T> cur = buffer[key];
            while (cur != null)
            {
                if (cur.Key == id) return cur.value;
                cur = cur.next;
            }
            throw new KeyNotFoundException("Элемент не найден");
        }

        public void Remove(int key)
        {
            int index = hash(key);
            Node<T> cur = buffer[index];
            Node<T>? prev = null;
            while (cur != null)
            {
                if(cur.Key == key)
                {
                    if (prev != null) prev.next = cur.next;
                    else buffer[index] = buffer[index].next;
                    size--;
                    break;
                }
                prev = cur;
                cur = cur.next;
            }
        }
    }
}
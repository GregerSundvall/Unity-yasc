using System.Collections.Generic;
using UnityEngine;

public class ListNode<T>
{
    public T content;
    public ListNode<T> next;
}


public class LinkedList<T>
{
    private ListNode<T> head;
    private ListNode<T> tail;
    private ListNode<T> current;

    public T Head()
    {
        return head.content;
    }
    public int Count { get; private set; }
    
    public T Tail()
    {
        return tail.content;
    }
    
    public T GetByIndex(int index)
    {
        current = head;
        for (int i = 0; i < index -1; i++)
        {
            current = current.next;
        }
        return current.content;
    }

    public void AddNewHead(T item)
    {
        var newNode = new ListNode<T> {content = item};
        if (Count == 0)
        {
            head = newNode;
            tail = newNode;
        }
        else
        {
            newNode.next = head;
            head = newNode;
        }
        Count++;
    }
    

    
    public void AddToEnd(T item)
    {
        if (Count == 0)
        {
            head = new ListNode<T> { content = item };
            tail = head;
            Count++;
            return;
        }

        tail.next = new ListNode<T> { content = item };
        tail = tail.next;
        Count++;
    }

    public void RemoveTail()
    {
        switch (Count)
        {
            case 0:
                Debug.Log("Cannot remove, list is empty.");
                break;
            case 1:
                head = null;
                tail = null;
                Count = 0;
                break;
            case >1:
                current = head;
                for (int i = 0; i < Count; i++)
                {
                    if (i == Count -2)
                    {
                        tail = current;
                        current.next = null;
                    }
                    else
                    {
                        current = current.next;
                    }
                }
                Count--;
                break;
        }
    }
}

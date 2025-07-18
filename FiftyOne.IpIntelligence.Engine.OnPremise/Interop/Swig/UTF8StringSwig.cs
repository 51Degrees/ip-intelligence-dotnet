//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (https://www.swig.org).
// Version 4.3.1
//
// Do not make changes to this file unless you know what you are doing - modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace FiftyOne.IpIntelligence.Engine.OnPremise.Interop {

internal class UTF8StringSwig : global::System.IDisposable, global::System.Collections.IEnumerable, global::System.Collections.Generic.IList<byte>
 {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal UTF8StringSwig(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(UTF8StringSwig obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  internal static global::System.Runtime.InteropServices.HandleRef swigRelease(UTF8StringSwig obj) {
    if (obj != null) {
      if (!obj.swigCMemOwn)
        throw new global::System.ApplicationException("Cannot release ownership as memory is not owned");
      global::System.Runtime.InteropServices.HandleRef ptr = obj.swigCPtr;
      obj.swigCMemOwn = false;
      obj.Dispose();
      return ptr;
    } else {
      return new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
    }
  }

  ~UTF8StringSwig() {
    Dispose(false);
  }

  public void Dispose() {
    Dispose(true);
    global::System.GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          IpIntelligenceEngineModulePINVOKE.delete_UTF8StringSwig(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public UTF8StringSwig(global::System.Collections.IEnumerable c) : this() {
    if (c == null)
      throw new global::System.ArgumentNullException("c");
    foreach (byte element in c) {
      this.Add(element);
    }
  }

  public UTF8StringSwig(global::System.Collections.Generic.IEnumerable<byte> c) : this() {
    if (c == null)
      throw new global::System.ArgumentNullException("c");
    foreach (byte element in c) {
      this.Add(element);
    }
  }

  public bool IsFixedSize {
    get {
      return false;
    }
  }

  public bool IsReadOnly {
    get {
      return false;
    }
  }

  public byte this[int index]  {
    get {
      return getitem(index);
    }
    set {
      setitem(index, value);
    }
  }

  public int Capacity {
    get {
      return (int)capacity();
    }
    set {
      if (value < 0 || (uint)value < size())
        throw new global::System.ArgumentOutOfRangeException("Capacity");
      reserve((uint)value);
    }
  }

  public bool IsEmpty {
    get {
      return empty();
    }
  }

  public int Count {
    get {
      return (int)size();
    }
  }

  public bool IsSynchronized {
    get {
      return false;
    }
  }

  public void CopyTo(byte[] array)
  {
    CopyTo(0, array, 0, this.Count);
  }

  public void CopyTo(byte[] array, int arrayIndex)
  {
    CopyTo(0, array, arrayIndex, this.Count);
  }

  public void CopyTo(int index, byte[] array, int arrayIndex, int count)
  {
    if (array == null)
      throw new global::System.ArgumentNullException("array");
    if (index < 0)
      throw new global::System.ArgumentOutOfRangeException("index", "Value is less than zero");
    if (arrayIndex < 0)
      throw new global::System.ArgumentOutOfRangeException("arrayIndex", "Value is less than zero");
    if (count < 0)
      throw new global::System.ArgumentOutOfRangeException("count", "Value is less than zero");
    if (array.Rank > 1)
      throw new global::System.ArgumentException("Multi dimensional array.", "array");
    if (index+count > this.Count || arrayIndex+count > array.Length)
      throw new global::System.ArgumentException("Number of elements to copy is too large.");
    for (int i=0; i<count; i++)
      array.SetValue(getitemcopy(index+i), arrayIndex+i);
  }

  public byte[] ToArray() {
    byte[] array = new byte[this.Count];
    this.CopyTo(array);
    return array;
  }

  global::System.Collections.Generic.IEnumerator<byte> global::System.Collections.Generic.IEnumerable<byte>.GetEnumerator() {
    return new UTF8StringSwigEnumerator(this);
  }

  global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() {
    return new UTF8StringSwigEnumerator(this);
  }

  public UTF8StringSwigEnumerator GetEnumerator() {
    return new UTF8StringSwigEnumerator(this);
  }

  // Type-safe enumerator
  /// Note that the IEnumerator documentation requires an InvalidOperationException to be thrown
  /// whenever the collection is modified. This has been done for changes in the size of the
  /// collection but not when one of the elements of the collection is modified as it is a bit
  /// tricky to detect unmanaged code that modifies the collection under our feet.
  public sealed class UTF8StringSwigEnumerator : global::System.Collections.IEnumerator
    , global::System.Collections.Generic.IEnumerator<byte>
  {
    private UTF8StringSwig collectionRef;
    private int currentIndex;
    private object currentObject;
    private int currentSize;

    public UTF8StringSwigEnumerator(UTF8StringSwig collection) {
      collectionRef = collection;
      currentIndex = -1;
      currentObject = null;
      currentSize = collectionRef.Count;
    }

    // Type-safe iterator Current
    public byte Current {
      get {
        if (currentIndex == -1)
          throw new global::System.InvalidOperationException("Enumeration not started.");
        if (currentIndex > currentSize - 1)
          throw new global::System.InvalidOperationException("Enumeration finished.");
        if (currentObject == null)
          throw new global::System.InvalidOperationException("Collection modified.");
        return (byte)currentObject;
      }
    }

    // Type-unsafe IEnumerator.Current
    object global::System.Collections.IEnumerator.Current {
      get {
        return Current;
      }
    }

    public bool MoveNext() {
      int size = collectionRef.Count;
      bool moveOkay = (currentIndex+1 < size) && (size == currentSize);
      if (moveOkay) {
        currentIndex++;
        currentObject = collectionRef[currentIndex];
      } else {
        currentObject = null;
      }
      return moveOkay;
    }

    public void Reset() {
      currentIndex = -1;
      currentObject = null;
      if (collectionRef.Count != currentSize) {
        throw new global::System.InvalidOperationException("Collection modified.");
      }
    }

    public void Dispose() {
        currentIndex = -1;
        currentObject = null;
    }
  }

  public UTF8StringSwig() : this(IpIntelligenceEngineModulePINVOKE.new_UTF8StringSwig__SWIG_0(), true) {
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
  }

  public UTF8StringSwig(UTF8StringSwig other) : this(IpIntelligenceEngineModulePINVOKE.new_UTF8StringSwig__SWIG_1(UTF8StringSwig.getCPtr(other)), true) {
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
  }

  public void Clear() {
    IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_Clear(swigCPtr);
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
  }

  public void Add(byte x) {
    IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_Add(swigCPtr, x);
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
  }

  private uint size() {
    uint ret = IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_size(swigCPtr);
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private bool empty() {
    bool ret = IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_empty(swigCPtr);
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private uint capacity() {
    uint ret = IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_capacity(swigCPtr);
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private void reserve(uint n) {
    IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_reserve(swigCPtr, n);
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
  }

  public UTF8StringSwig(int capacity) : this(IpIntelligenceEngineModulePINVOKE.new_UTF8StringSwig__SWIG_2(capacity), true) {
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
  }

  private byte getitemcopy(int index) {
    byte ret = IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_getitemcopy(swigCPtr, index);
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private byte getitem(int index) {
    byte ret = IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_getitem(swigCPtr, index);
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private void setitem(int index, byte val) {
    IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_setitem(swigCPtr, index, val);
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
  }

  public void AddRange(UTF8StringSwig values) {
    IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_AddRange(swigCPtr, UTF8StringSwig.getCPtr(values));
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
  }

  public UTF8StringSwig GetRange(int index, int count) {
    global::System.IntPtr cPtr = IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_GetRange(swigCPtr, index, count);
    UTF8StringSwig ret = (cPtr == global::System.IntPtr.Zero) ? null : new UTF8StringSwig(cPtr, true);
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void Insert(int index, byte x) {
    IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_Insert(swigCPtr, index, x);
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
  }

  public void InsertRange(int index, UTF8StringSwig values) {
    IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_InsertRange(swigCPtr, index, UTF8StringSwig.getCPtr(values));
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
  }

  public void RemoveAt(int index) {
    IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_RemoveAt(swigCPtr, index);
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
  }

  public void RemoveRange(int index, int count) {
    IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_RemoveRange(swigCPtr, index, count);
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
  }

  public static UTF8StringSwig Repeat(byte value, int count) {
    global::System.IntPtr cPtr = IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_Repeat(value, count);
    UTF8StringSwig ret = (cPtr == global::System.IntPtr.Zero) ? null : new UTF8StringSwig(cPtr, true);
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public void Reverse() {
    IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_Reverse__SWIG_0(swigCPtr);
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
  }

  public void Reverse(int index, int count) {
    IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_Reverse__SWIG_1(swigCPtr, index, count);
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
  }

  public void SetRange(int index, UTF8StringSwig values) {
    IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_SetRange(swigCPtr, index, UTF8StringSwig.getCPtr(values));
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
  }

  public bool Contains(byte value) {
    bool ret = IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_Contains(swigCPtr, value);
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public int IndexOf(byte value) {
    int ret = IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_IndexOf(swigCPtr, value);
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public int LastIndexOf(byte value) {
    int ret = IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_LastIndexOf(swigCPtr, value);
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool Remove(byte value) {
    bool ret = IpIntelligenceEngineModulePINVOKE.UTF8StringSwig_Remove(swigCPtr, value);
    if (IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Pending) throw IpIntelligenceEngineModulePINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

}

}

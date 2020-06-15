# Ascentis.Infrastructure Library

This library offers a a few general purpose components:

## Ascentis.ComPlusCache
It provides functionality similar to what MemoryCache provides.

When to use this?
If you have a model where you run in one machine a farm of processes and want a drop-in easy way to implement a shared cache of data with functionality like the one provided by MemoryCache this class can help fulfill your needs.

Some details to consider:

Strongly recommended to store data using primitive types.
String supported more efficiently using the overloaded methods that receive string as parameter. The rest of primitives supported using the other overload that receives an object (OleVariant if pure COM calls are made) as parameter.
The class supports serialized COM objects but performance is about 10x slower than serializing your object as text (JSON o XML) and then caching the text.
When passing a COM object to store in cache, a shallow copy will be performed and the resulting copied object will be stored. Currently only public properties are copied over and there's no test to verify the behavior if within those properties there's a subobject.
Current tests cover only the case of a COM DTO containing only primitive public properties.

## Ascentis.AsyncDisposer

Use this static class to dispose of IDisposable objects using async semantics.
When to use this?
If you use third party software which you can't modify that has caused you trouble when disposing objects on an event which later makes software beyond your control throw exceptions because somewhere in the call stack it tries to use the diposed object.
With this class you can enqueue the IDisposable instance in the AsyncDisposer queue and it will be guaranteed to stay in queue for a specified global period of time before being take for disposal.
Tune the timing in a way that gives time enough for the calling procedure in the stack to complete and return to the caller. 

A second class part of this package is ConcurrentObjectAccessor<>. This class allows to wrap an object within it and allow for protected access
to execute functions within a readlock. This allows usage of method SwapAndExecute() which allow to swap safely the underlying reference wrapped.
When to use this? When dealing with COM, COM+ or DCOM objects prone to crashing or suffering from network partitions. With this class the COM
object can be isolated behing a wrapper relying on ConcurrentObjectAccessor and the low level object can be replaced transparently upon detection
of partition exception.
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

## Ascentis.Retrier

This class allows to wrap function or procedure calls in a try-catch block that would retry operations based on on a delegate function returning
true or false. Can be combined with ConcurrentObjectAccessor to replace transparently a low level object upon certain conditions.

## Ascentis.ConccurentObjectAccessor

This class allows to control usage of an object reference permitting multiple concurrent threads using it for reading its value (and executing code with it),
while only one thread can write to the reference at the same time. The main purpose is to replace a failing object transparently upon detection of a faulty condition.
A concrete example is a COM+ object which crashed. The reference to this object is worthless until replaced by a new COM+ object instance.

## Ascentis.SolidComPlus

This class encapsulates a COM+ object instance allowing the user to implement automatic swapping of a dead object. 

## SimpleMemoryCache

Replacement for standard .NET MemoryCache based on ConcurrentDictionary. After opening the hood on MemoryCache found it over-complicated for most uses compared with ConcurrentDictionary.
This class is based on ConcurrentDictionary and provides expiration like MemoryCache using an ancillary timer.

## BoundedParallel

Use instead of standard static Parallel class to gain control of the number of concurrent calls that be executed using Parallel static class. This allows to control the number of threads
from the default .NET threadpool to avoid a runaway scenario where .NET has to keep trying to add more threads. If a high number of callers attempt multiple concurrent calls to Parallel methods
it can fall into what's called "Hill Climbing" algorihtm causing high contention as .NET tries to add more threads slowly
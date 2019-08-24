# Ascentis.ExternalCache Component

This component offers a subset of the functionality you get by using MemoryCache in your application's default process and AppDomain.

Why would you use this class?

If you have a model where you run in one machine a farm of processes and want a drop-in easy way to implement a shared cache of data with functionality like the one provided by MemoryCache this class can help fulfill your needs.

Some details to consider:

Strongly recommended to store data using primitive types (only string supported for now).
The class supports serialized COM objects but performance is about 10x slower than serializing your object as text (JSON o XML) and then caching the text.
When passing a COM object to store in cache, a shallow copy will be performed and the resulting copied object will be stored. Currently only public properties are copied over and there's no test to verify the behavior if within those properties there's a subobject.
Current tests cover only the case of a COM DTO containing only primitive public properties.
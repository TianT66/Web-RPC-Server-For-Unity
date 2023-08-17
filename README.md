# Web-RPC-Server-For-Unity
A very simple interface-based RPC framework:

// Usage:
1. Import the plugin into your Unity project.
2. Access the service-side project 'WebRPCServer.sln' by visiting 'https://github.com/TianT66/Web-RPC-Server-For-Unity.git'.
3. Start the "WebRPCServer.sln" service project.

// Server-side protocol definition:
1. Create an interface protocol on the server and use the [WebRPC] attribute. The interface must have a namespace.
2. The return type of the methods defined in the interface must be Task or Task<T>.
3. The parameters of the methods defined in the interface can be basic types and their corresponding collections, such as int, string, int[], List<int>, string[], List<string>, etc. They can also be custom structures, but these structures must use the [WebRPCParam] attribute.

// Client-side invocation:
1. Copy the interface protocol and custom structures to the Unity client.
2. var res = await WebRPC.Call<Interface>().Fun(arg1 , arg2 , ...)

// Important note:
1. In Unity 2021.3, the "Newtonsoft.Json.dll" is already included. For other lower versions, please compile the server project first, then locate the dynamic library in the "WebRPCServer\bin\Debug\netcoreapp3.1" folder, and copy it to the "Assets/Plugins" folder.

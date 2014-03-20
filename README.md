Yodii overview
=====

#At a glance

Yodii is yet another Dependency Injection initiative. But different :-).

This project is (one of) the result of our work on the Civikey platform: it is based on the Civikey kernel (in terms of ideas as well as original code) and once fully operational, Civikey will be powered by Yodii. 

To understand Yodii you need a basic understanding of what [Dependency Injection](http://martinfowler.com/articles/injection.html) is and how it is implemented by today’s frameworks ([Castle Windsor](http://docs.castleproject.org/Windsor.MainPage.ashx), [Unity](http://unity.codeplex.com), [NInject](http://www.ninject.org/), [Spring.Net](http://www.springframework.net/), [Autofac](https://code.google.com/p/autofac/), etc.). Do not hesitate to read "[Dependency Injection. NET](http://www.manning.com/seemann/)" for more information about dependency injection in general and in the context of .Net in particular.

Yodii differs from all those classical approaches (and also from Microsoft Extension Framework – [MEF](http://msdn.microsoft.com/en-us/library/dd460648.aspx) – that is itself quite different from the others) and is somewhat closer to [OSGi framework](http://en.wikipedia.org/wiki/OSGi) in terms of goals. 

- - -
In (very) short, Yodii is a “runtime engine” for Plugins that rely on Services implemented by other Plugins, all of them supporting to be dynamically started, stopped or disabled.
- - -

#Who is it for?

We believe that Yodii can be used for a lot of desktop .Net applications (both classical .Net ones and Windows Pro tablets applications). We also believe that the way we modeled the engine and its runtime status, resolved dependencies and, more generally, designed the whole system, may be adapted to other platforms.

Architects (and developers) that have already met the requirement for a “System” or an “Application” to be more “dynamically composed” by different “Modules”, know how such objective is easy to describe… and difficult to achieve. Yodii aims to offer them a solid (yet simple) foundation for a modular application.

#Main ideas
##Basic Yodii principles are:
The short list below defines the key point of what Yodii is about. It also exposes what we consider to be “welcomed limitations”, the kind of restrictions that offer more (at the end) than they take away (at first glance).

- Services are abstractions that are defined by an interface that extends IDynamicService interface marker.
  - A Service (an abstraction) is identified by the full name of the interface (.Net type).
- Plugins are implemented by a concrete Class that supports a IPlugin interface (to support the 2 phases start/stop).
  - A Plugin (an implementation) is identified by a GUID.
- A Plugin can implement at most one Service.
- A Plugin implementation can use 0 or more Services (in order to do its job).
- The relationship between a Plugin and a Service it wants to use can be: Optional, OptionalTryStart, MustExist, MustExistInitialStart or MustExistAndRun.
- At any time, no more than one implementation of a given Service can be running.

Yoddi engine is in charge of computing and maintaining the best configuration of what must be running or stopped.

##More advanced aspects are:
- The runtime supports (and enforces) requirements about Services and/or Plugin statuses (like Plugin A must not Run, B must be Runnable, and Service S must be Running).
- The runtime exposes at any time for each Plugin or Service a running status that is one among Disabled, Stopped, Running or RunningLocked.
- Plugin or Service can be started (when Stopped) or stopped (when Running) by any actor at any time (the code of a Plugin or the user acting on the system).
- Specialization of Services is supported: Service interface can extend another Service.
- The runtime takes into account that starting a Plugin that supports a specialized Service makes the base Service also available.
- Plugin code is lightly isolated from the Services it uses thanks to dynamically generated proxies. Such Service proxies enable:
  -  Logging capability of any Plugin/Service interactions (dynamically enabled at runtime).
  - Error protection by intercepting exceptions (dynamically enabled at runtime).
  - Easy to use handling of optional Plugin-to-Service requirement runtime behavior: an event exposes the Service status changes and enables the Plugin to react accordingly.
  - Transparent handling of cycling dependencies (we consider this as being a requirement to actually support “Programming against Abstraction”).
  - “Hot swapping” of the implementation of a Service by another Plugin.
  
##Implementation, Choices & Frame of Works

Yodii is a VS2012 solution. It currently targets .Net framework 4.0 but should compile against other frameworks. We use quite standard C# coding conventions (with minor differences): this [Visual Studio settings](https://github.com/Invenietis/ck-core/blob/master/CKTextEditor.vssettings) should be used to define them. NUnit is used for the unit tests, NuGet for package management.

Yodii is split into two assemblies: Yodii.Engine and Yodii.Model that exposes the abstractions. Both of them rely on as few dependencies as possible.

Yodii uses two namespaces:  CK.Plugin hosts primary objects of the model (like IPlugin, IDynamicService, etc.). Runtime only objects are defined in CK.Plugin.Hosting (like the Planner or ServiceHost).

Yodii does not use multiple threads nor does it support any kind of multi-threaded architecture. This is a deliberate choice: the engine runs in a deterministic world and produces deterministic results. Plugins may nonetheless use multiple threads provided that calls to the engine are serialized.

#Current Project Status
Project started on March 2013, the 15<super>th</super>.

###January 23rd, 2014 (0.0.1):

- Engine is functional, but running propagations still need some work for advanced usage. See Yodii.Lab.Tests/Samples/InvalidLoop.xml.
- Lab is functional. Still has some graph issues with overlap removal. Can display, save, load, and add/remove/edit components and configuration. Engine integration is mostly complete: You can start the Engine and play around with components while it's running.
- Host needs implementation.

Any help is appreciated.







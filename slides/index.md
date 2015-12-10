- title : Hopac
- description : Introduction to Hopac and Synchronous Rendezvous
- author : Marcus Griep
- theme : Moon
- transition : dissolve

***

# Hopac

## Concurrency and Synchronous Rendezvous

<br/>
<br/>

### Marcus Griep

***
### [Agenda](index.html)

1. <span class="nextsegment">[Concurrency](concurrency.html)</span>
2. [Hopac](hopac.html)
3. [Alternatives](alternatives.html)
 - [Fibonacci Sequence](fibonacci.html)
 - [How do they work?](altDeepDive.html)
4. [IVar, MVar, Mailbox...](remainder.html)

***
### Other implementations

- Concurrent ML
- Communicating Sequential Processes (CSP)
- occam
- Clojure: [core.async][]
 - Uses unbounded buffers by default

***
### Resources

- [Hopac][]
 - [Programming Guide][HopacProg]
 - [Cancellation of Async on Negative Acknowledgement][HopacAsync]
- [Go][]

  [RPik12v]:https://vimeo.com/49718712
  [Hopac]:https://hopac.github.io/Hopac/Hopac.html
  [HopacProg]:https://github.com/Hopac/Hopac/blob/master/Docs/Programming.md
  [HopacAsync]:https://github.com/Hopac/Hopac/blob/master/Docs/Alternatives.md
  [Go]:http://golang.org/
  [core.async]:http://clojure.com/blog/2013/06/28/clojure-core-async-channels.html
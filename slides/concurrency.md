- title : Hopac - Concurrency
- description : Introduction to Hopac and Synchronous Rendezvous
- author : Marcus Griep
- theme : Moon
- transition : dissolve

***
### What is Concurrency?

- Not Parallelism
- Multiple threads of execution that can be interleaved
 - Concurrency can happen on a single core
 - Does not mean executing at the same time

---
### Concurrency vs. Parallelism

> Concurrency is the composition of independently executing 'processes'.
>
> Parallelism is the simultaneous execution of (possibly related) computations.

—Rob Pike, [Concurrency is not Parallelism][RPik12v]

---
### Concurrency vs. Parallelism

> Concurrency is about the way we structure programs.
>
> Parallelism is about the way they run.

—Rob Pike, [Concurrency is not Parallelism][RPik12v]

***
### What is Synchronous?

> Existing or occurring at the same time.

Computations must meet to coordinate.

If one is not available, the other must wait (synchronize).

---
### What is Asynchronous?

> Not existing or happening at the same time.

Computations do not need to meet to coordinate.

If one is not available, the other does not need to wait.

---
### Synchronous vs. Asynchronous

- Viewpoint-based
- Events may appear asynchronous to one party and synchronous to the other
 - A client doesn't need to wait for the server
 - A server must wait for messages

***
### Blocking vs. Non-Blocking

An operation is

- Blocking if it prevents a thread from executing
- Non-blocking if it allows a thread to continue executing

while waiting for an event

***

### A <span class="yellowlight">concurrent</span> process<br/>is better able to leverage<br/><span class="yellowlight">parallel</span> execution

---

### A <span class="redlight">blocking</span> operation<br/>precludes<br/><span class="yellowlight">concurrent</span> execution

---

### A <span class="greenlight">synchronous</span> operation<br/>*does not* preclude<br/><span class="yellowlight">concurrent</span> execution

***
### Asynchronous Message Passing

#### The client does not need to wait<br/>for the server to be ready to send a message.

---
### Synchronous Message Passing

#### The client needs to wait<br/>for the server to be ready to give a message.

---
### Synchronous Rendezvous

#### The client needs to wait<br/>for the server to be ready to give a message.

---
<div class="span5">

#### Asynchronous<br/>Message Passing

- Client sends
- Server receives
- Buffer is a mailbox

</div>
<div class="span5">

#### <br/>Synchronous Rendezvous

- Client gives
- Server takes
- Buffer is a channel
</div>

---
<div class="span5">

#### <span class="greenlight">Asynchronous</span><br/>Message Passing

- <span class="greenlight">Client sends</span>
- <span class="redlight">Server receives</span>
- Buffer is a mailbox
</div>
<div class="span5">

#### <br/><span class="redlight">Synchronous</span> Rendezvous

- <span class="redlight">Client gives</span>
- <span class="redlight">Server takes</span>
- Over a channel
</div>

---
<div class="span5">

#### <span class="greenlight">Asynchronous</span><br/>Message Passing

- <span class="greenlight">Client sends</span>
- <span class="redlight">Server receives</span>
- Buffer is a mailbox<br/>(unbounded)

![Mailbox](images/CanadaPostMailbox.png)

</div>
<div class="span5">

#### <br/><span class="redlight">Synchronous</span> Rendezvous

- <span class="redlight">Client gives</span>
- <span class="redlight">Server takes</span>
- Over a channel<br/>(no buffer)

![Telephone](images/telephone.png)

</div>

***
### [Agenda](index.html)

1. [Concurrency](concurrency.html)
2. <span class="nextsegment">[Hopac](hopac.html)</span>
3. [Alternatives](alternatives.html)
 - [Fibonacci Sequence](fibonacci.html)
 - [How do they work?](altDeepDive.html)
4. [IVar, MVar, Mailbox...](remainder.html)

 [RPik12v]:https://vimeo.com/49718712

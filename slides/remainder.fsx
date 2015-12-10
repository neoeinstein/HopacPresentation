(**
- title : Hopac - Remainders
- description : Introduction to Hopac and Synchronous Rendezvous
- author : Marcus Griep
- theme : Moon
- transition : dissolve

***
*)
(*** hide ***)
open System
open System.Threading
open System.Collections.Generic
#I "../packages/presentation/Hopac/lib/net45"
#r "Hopac.Core.dll"
#r "Hopac.dll"
open Hopac
open Hopac.Infixes
(**
### IVar
#### Write-once variable
*)
(*** define-output:ivar ***)
let ivar = IVar ()
queue <| IVar.read ivar ^-> printfn "Read from IVar %i"
queue <| IVar.read ivar ^-> printfn "Read from IVar %i"
queue <| IVar.read ivar ^-> printfn "Read from IVar %i"
run <| IVar.fill ivar 5
(*** include-output:ivar ***)
(**
Reads block until filled; Once filled, all reads proceed

*Optimized as a lightweight substitute for a reply channel*

---
### MVar
#### Serialized access variable
*)
(*** define-output:mvar ***)
let mvar = MVar ()
run <| MVar.fill mvar 5
run <| MVar.take mvar ^-> printfn "Read from MVar %i"
queue <| MVar.take mvar ^=> (fun i ->
  printfn "Read from MVar %i" i ; MVar.fill mvar i)
queue <| MVar.read mvar ^-> printfn "Read from MVar %i"
run <| MVar.fill mvar 13
(*** include-output:mvar ***)
(**
Takes block until filled; Once filled, first take empties

`read` is a composition of `take` and `fill`

---
### Mailbox
#### Unbounded buffer
*)
let mbx = Mailbox<string>()
Mailbox.Global.send mbx "message"
(**
An asynchronous, unbounded buffer mailbox.

---
### BoundedMb
#### Bounded buffer
*)
let boundedMb = run <| BoundedMb.create 100
run <| BoundedMb.put boundedMb "message"
(**
A synchronous, bounded mailbox for many to many communication.

Useful for coordinating work; Provide slack for producer/consumer;
Back-pressure blocking producers if full

If buffering not necessary, use a channel. If unbounded buffering is ok, prefer a mailbox.

---
### Stream
#### Choice stream
*)
(*** define-output:streamProp ***)
let property = Stream.Property("first")
let propStream = property.Tap ()
Stream.consumeFun (printfn "%s") propStream
property.Value <- "second"
property.Value <- "third"
(*** include-output:streamProp ***)
(**
A mutable property that generates a stream of values and property change notifications as a side-effect

---
### Stream
#### Choice stream
*)
(*** define-output:streamEvt ***)
let evtSrc = Stream.Src.create ()
let srcStream = Stream.Src.tap evtSrc
Stream.consumeFun (printfn "First: %s") srcStream
run <| Stream.Src.value evtSrc "Almost"
Stream.consumeFun (printfn "Second: %s") <| Stream.Src.tap evtSrc
run <| Stream.Src.value evtSrc "Done"
run <| Stream.Src.close evtSrc
(*** include-output:streamEvt ***)
(**
An imperative source of a stream of values called a stream source

***
### <span class="nextsegment">[Agenda](index.html#/2)</span>

1. [Concurrency](concurrency.html)
2. [Hopac](hopac.html)
3. [Alternatives](alternatives.html)
 - [Fibonacci Sequence](fibonacci.html)
 - [How do they work?](altDeepDive.html)
4. [IVar, MVar, Mailbox...](remainder.html)

*)
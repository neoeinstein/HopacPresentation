(**
- title : Hopac - What is Hopac?
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
(**
### What is [Hopac][]?

- F# library <small>(not a framework)</small>
 - with a C# core
- Enables concurrency
- Provides composable primitives
 - `Ch`, `Job`, `Alt`
 - Jobs can spawn other jobs
- Uses synchronous rendezvous
- Inspired by Concurrent ML

---
### Akka.Net
- C# framework
 - with some F# bindings
- Provides actor scaffold, routing, supervision, delivery
 - `ReceiveActor`, `UntypedActor`
 - Actors can spawn other actors
- Uses asynchronous message passing
- Inspired by Akka

---
### Hopac Execution Model

- *Jobs*
 - `Job<'x>`
 - *Extremely* lightweight threads
 - Jobs can start other jobs
 - May be millions executing concurrently
 - Thread pre-emption using continuations

---
### Hopac Primitives
#### *Channels* and *Alternatives*

- First-class
- Higher-order
- Selective
- Synchronous
- Lightweight

---
### Hopac Applications

> Hopac is designed and optimized to scale as the number of such relatively
independent lightweight elements is increased.

#### Specifically

- Build systems
- Web servers
- UI
- Simulations

***
### Hopac
*)
#I "../packages/presentation/Hopac/lib/net45"
#r "Hopac.Core.dll"
#r "Hopac.dll"
open Hopac
(**
---

### Updatable Storage Cell
    type Cell<'a>
    val cell: 'a -> Job<Cell<'a>>
    val get: Cell<'a> -> Job<'a>
    val set: Cell<'a> -> 'a -> Job<unit>

---
*)
type Request<'a> =
  | Get
  | Put of 'a

type Cell<'a> =
  { reqCh: Ch<Request<'a>>
    replyCh: Ch<'a> }
(**
---
*)
let put (c:Cell<'a>) (x:'a) : Job<unit> = job {
  return! Ch.give c.reqCh (Put x)
}

let get (c:Cell<'a>) : Job<'a> = job {
  do! Ch.give c.reqCh Get
  return! Ch.take c.replyCh
}
(**
---
*)
let cell (x:'a) : Job<Cell<'a>> = job {
  let c = {reqCh = Ch (); replyCh = Ch ()}
  let rec server x = job {
    let! req = Ch.take c.reqCh
    match req with
    | Get ->
      do! Ch.give c.replyCh x
      return! server x
    | Put v ->
      return! server v
  }
  do! Job.start (server x)
  return c
}
(**
---
*)
(*** define-output:cell1 ***)
let c = run (cell 1)
run (get c) |> printfn "%i"

run (put c 2)
run (get c) |> printfn "%i"
(**
#### Output
*)
(*** include-output:cell1 ***)
(**
---
*)
(*** define-output:cell2, module:cell2 ***)
let printCell c = job {
  let! v = get c
  do printfn "%i" v
}
let doubleCell c = job {
  let! v = get c
  do! put c (v * 2)
}

let c = run (cell 3)
let double = doubleCell c
let print = printCell c

run double
run print
run (put c 15)
run print
run double
run print
(**
#### Output
*)
(*** include-output:cell2 ***)
(**
***
### Memory Pressure
*)
(*** hide ***)
let dumpMemory () =
  GC.GetTotalMemory true |> printfn "Memory used: %i bytes"
(*** define-output:cellGc ***)
dumpMemory ()
let mutable cs = (List.init 1000000 <| fun i -> run (cell i))
dumpMemory ()
cs <- []
dumpMemory ()
(**
#### Output
*)
(*** include-output:cellGc ***)
(**
---
### Memory Pressure

For $m$ jobs communicating with synchronous message passing over $n$ channels,
the memory required is tightly bounded by

$$$
\Theta(m + n)

*)
(**
***
### Hopac Forms

<div class="span5">

#### Job Workflow Builder
*)
(*** module:dblWorkflow ***)
let doubleCell c = job {
  let! v = get c
  do! put c (v * 2)
}
(**
</div>
<div class="span5">

#### Monadic Combinators
*)
(*** module:dblMonadic ***)
open Hopac.Infixes

let doubleCell c =
  get c
  >>= fun v -> put c (v * 2)
(**
</div>
*)
(**
---
### Cell Operations Revisited
#### Monadic Combinators
*)
(*** module:cellMonad ***)
let put (c:Cell<'a>) (x:'a) : Job<unit> =
  upcast Ch.give c.reqCh (Put x)

let get (c:Cell<'a>) : Job<'a> =
  Ch.give c.reqCh (Get)
  >>=. Ch.take c.replyCh

let cell (x:'a) : Job<Cell<'a>> =
  Job.delay <| fun () ->
    let c = {reqCh = Ch (); replyCh = Ch ()}
    let rec server x =
      Ch.take c.reqCh
      >>= function
        | Get ->
          Ch.give c.replyCh x
          >>=. server x
        | Put v ->
          server v
    Job.start (server x)
    >>-. c
(**
---
### Cell Operations Revisited
#### Hopac Custom Operators
*)
(*** module:cellCustOp ***)
let put c x = c.reqCh *<- Put x

let get c = c.reqCh *<- Get >>=. c.replyCh

let cell x = Job.delay <| fun () ->
  let c = {reqCh = Ch (); replyCh = Ch ()}
  Job.iterateServer x (fun x ->
    c.reqCh >>= function
      | Get -> c.replyCh *<- x >>-. x
      | Put v -> Job.result v)
  >>-. c
(**
***
### [Agenda](index.html)

1. [Concurrency](concurrency.html)
2. [Hopac](hopac.html)
3. <span class="nextsegment">[Alternatives](alternatives.html)</span>
 - [Fibonacci Sequence](fibonacci.html)
 - [How do they work?](altDeepDive.html)
4. [IVar, MVar, Mailbox...](remainder.html)

  [Hopac]:https://hopac.github.io/Hopac/Hopac.html

*)
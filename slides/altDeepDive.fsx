(**
- title : Hopac - Alternatives: How do they work?
- description : Introduction to Hopac and Synchronous Rendezvous
- author : Marcus Griep
- theme : Moon
- transition : dissolve

***
## Alternatives
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

![Science](images/sciencedog.jpg)

### How do they work?

---
### What are alternatives?

An alternative, `Alt<'x>`, represents the possibility of communicating a
value of type `'x` from one concurrent entity to another. How that value is
computed and when that value is available are details encapsulated by the
alternative. Alternatives can be created and combined in many ways allowing
alternatives to encapsulate complex communication protocols.

---
### Binding

    do! Ch.give aChannel aValue

    let! aValue = Ch.take aChannel

Conceptually, binding an alternative operation *instantiates* the given
alternative, *waits until* the alternative becomes *available* and then
*commits* to the alternative and returns the value communicated by the
alternative.

---
### Choice and Wrapping
*)
type Button = { Pressed : Alt<unit> }
type Dialog = { Yes : Button; No : Button; Show : Job<unit>}

let askUser dlg = job {
  do! dlg.Show
  let! wasYes =
    Alt.choose [
      dlg.Yes.Pressed ^=> fun () -> Job.result true
      dlg.No.Pressed  ^=> fun () -> Job.result false
    ]
  return
    if wasYes then
      "Ok"
    else
      "I'm sorry, Dave, but I can't do that"
}
(**
---
### Choice and Wrapping
*)
(*** define-output:choiceWrap ***)
let yesCh, noCh = Ch (), Ch ()
let yesBtn = { Pressed = yesCh }
let noBtn = { Pressed = noCh }
let dlg = { Yes = yesBtn; No = noBtn; Show = Job.unit () }

let futureResult = run (Promise.queue <| askUser dlg)
run <| Ch.give noCh ()
run <| Promise.read futureResult
(*** include-it:choiceWrap ***)
(**
---
### Choice and Wrapping

- Alt.choose (<|>)
 - Wait for first available
 - Commit to one
 - Deterministic
- Alt.chooser (<~>)
 - Non-deterministic
- Alt.afterJob (^=>)
 - Extends an alternative to wrap additional operations after commit

***
### Prepare Job
*)
type Ticks = int64
let mutable curTicks : Ticks = 0L
let timerReqCh : Ch<Ticks * Ch<unit>> = Ch ()

let atTime (t : Ticks) : Alt<unit> =
  Alt.prepareJob <| fun () ->
    let replyCh = Ch ()
    Ch.send timerReqCh (t, replyCh) >>-.
    Ch.take replyCh
(**
***
### Negative acknowledgements
*)
(*** define-output:altNack ***)
let verbose c alt = Alt.withNackJob <| fun nack ->
  printf "%s Instantiated and " c
  Job.start (nack >>- fun () -> printfn "aborted.") >>-.
  alt ^-> fun x -> printfn "committed to." ; x

run <| Alt.choose [verbose "A" <| Alt.always 1; Alt.always 2]
run <| Alt.choose [verbose "B" <| Alt.never (); Alt.always 2]
run <| Alt.choose [Alt.always 1; verbose "C" <| Alt.always 2]
(*** include-output:altNack ***)
(**
---
### Lock server
*)
type Lock = Lock of int64

type LockRequest =
  | Acquire of int64 * replyCh : Ch<unit> * abortAlt : Alt<unit>
  | Release of int64

type LockServer =
  { mutable nextId : int64
    reqCh : Ch<LockRequest> }
(**
---
### Lock server
#### Client
*)
let createLock s =
  Lock (Interlocked.Increment &s.nextId)

let release s (Lock l) =
  Ch.give s.reqCh (Release l)

let acquire s (Lock l) =
  Alt.withNackJob <| fun abortAlt ->
    let replyCh = Ch ()
    Ch.send s.reqCh (Acquire (l, replyCh, abortAlt))
    >>-. Ch.take replyCh
(**
---
### Lock server
#### Server
*)
let lockHeld (queue : Queue<_>) replyCh abortAlt =
  queue.Enqueue (replyCh,abortAlt)
  Alt.unit ()
let lockFree (locksDict : Dictionary<_,_>) lock replyCh abortAlt =
  Alt.choose [
    Ch.give replyCh () ^-> fun () -> locksDict.Add (lock, Queue<_>())
    abortAlt ]
let releaseNoWaiting = Alt.unit ()
let releaseWithWaiting (locksDict : Dictionary<_,_>) (queue : Queue<_>) lock =
  let rec assign () =
    if queue.Count = 0 then
      locksDict.Remove lock |> ignore
      Alt.unit ()
    else
      let (replyCh,abortAlt) = queue.Dequeue ()
      Alt.choose [
        Ch.give replyCh ()
        abortAlt ^=> assign ]
  assign ()
(**
---
### Lock server
#### Server
*)
let startLockServer = Job.delay <| fun () ->
  let locks = Dictionary<int64, Queue<Ch<unit> * Alt<unit>>>()
  let s = { nextId = 0L; reqCh = Ch () }
  Job.foreverServer
    (Ch.take s.reqCh >>= function
      | Acquire(lock, replyCh, abortAlt) ->
        match locks.TryGetValue lock with
        | (true, pend) -> lockHeld pend replyCh abortAlt
        | _ -> lockFree locks lock replyCh abortAlt
      | Release lock ->
        match locks.TryGetValue lock with
        | (true, pend) -> releaseWithWaiting locks pend lock
        | _ -> releaseNoWaiting)
  >>-. s
(**
---
### Lock server
#### Example
*)
let withLock (s: LockServer) (l: Lock) (xJ: Job<'x>) : Alt<'x> =
  acquire s l ^=> fun () -> Job.tryFinallyJob xJ (release s l)

let lockSrv = run <| startLockServer
let withMyLock = withLock lockSrv (createLock lockSrv)
run <| Alt.choose [
    withMyLock (Job.delay <| fun () -> Job.lift printfn "Hello")
    timeOutMillis 50 ^-> fun () -> printfn "Timed Out" ]
(**
---
### On Fairness and Determinism

Concurrent ML emphasizes *fairness* and *non-determinism*

Hopac emphasizes *performance* and *co-operation*

*)
(*** define-output:fairness ***)
Alt.choose
  [ Alt.prepareFun <| fun () -> (printfn "A" ; Alt.always 1)
    Alt.prepareFun <| fun () -> (printfn "B" ; Alt.always 2) ]
(**
Concurrent ML would print "A" & "B"

Hopac prints
*)
(*** include-output:fairness ***)
(**
Concurrent ML evaluates alternatives *eagerly*

Hopac evaluates *lazily*

***
### [Agenda](index.html)

1. [Concurrency](concurrency.html)
2. [Hopac](hopac.html)
3. [Alternatives](alternatives.html)
 - [Fibonacci Sequence](fibonacci.html)
 - [How do they work?](altDeepDive.html)
4. <span class="nextsegment">[IVar, MVar, Mailbox...](remainder.html)</span>

*)
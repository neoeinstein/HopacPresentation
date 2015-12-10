(**
- title : Hopac - Alternatives
- description : Introduction to Hopac and Synchronous Rendezvous
- author : Marcus Griep
- theme : Moon
- transition : dissolve

***
### Using Alternatives
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
(*** module:cellAlt ***)
type Cell<'a> =
  { getCh: Ch<'a>
    putCh: Ch<'a> }
(**
---
### Using Alternatives
*)
(*** module:cellAlt ***)
let put c v = Ch.give c.putCh v
let get c = Ch.take c.getCh

let cell x = Job.delay <| fun () ->
  let c = {getCh = Ch (); putCh = Ch ()}
  let rec server x =
    Alt.choose
      [ Ch.take c.putCh   ^=> fun v -> server v
        Ch.give c.getCh x ^=> fun () -> server x ]
  Job.start (server x) >>-. c
(**
---
### Using Alternatives
*)
(*** module:cellAltIter ***)
let cell x = Job.delay <| fun () ->
  let c = {getCh = Ch (); putCh = Ch ()}
  Job.iterateServer x <| fun x ->
        Alt.choose
          [ Ch.take c.putCh
            Ch.give c.getCh x ^->. x ]
  >>-. c
(**
***
## Kismet

<div class="span2">

![Compare Bool](https://udn.epicgames.com/Three/rsrc/Three/KismetUserGuide/condition.jpg)
</div>
<div class="span6">

![Simple Sequence](https://udn.epicgames.com/Three/rsrc/Three/KismetUserGuide/simple_sequence.jpg)
</div>
<div class="span2">

![Event](https://udn.epicgames.com/Three/rsrc/Three/KismetUserGuide/event.jpg)
</div>

---
### Compare Bool

![Compare Bool](https://udn.epicgames.com/Three/rsrc/Three/KismetUserGuide/condition.jpg)

---
### Compare Bool

![Compare Bool](https://udn.epicgames.com/Three/rsrc/Three/KismetUserGuide/condition.jpg)
*)
let CompareBool (comparand : ref<bool>)
                (input : Alt<'x>)
                (onTrue : 'x -> #Job<unit>)
                (onFalse : 'x -> #Job<unit>) : Job<unit> =
  input >>= fun x ->
    if !comparand then
      onTrue x :> Job<unit>
    else
      onFalse x :> Job<unit>
(**
---
### Delay
*)
(*** do-not-eval ***)
let Delay (duration : ref<TimeSpan>)
          (start : Alt<'x>)
          (stop : Alt<'y>)
          (onFinished : 'x -> #Job<unit>)
          (onAborted : 'y -> #Job<unit>) : Job<unit>
(**
---
### Delay
*)
let Delay (duration : ref<TimeSpan>)
          (start : Alt<'x>)
          (stop : Alt<'y>)
          (onFinished : 'x -> #Job<unit>)
          (onAborted : 'y -> #Job<unit>) : Job<unit> =
  start >>= fun x ->
    Alt.choose
      [ stop                ^=> fun y -> onAborted y
        timeOut (!duration) ^=> fun () -> onFinished x ]
(**
---
### Wiring it together
*)
(*** hide ***)
let ch1 = Ch<int> ()
let ch2 = Ch<int> ()
let ch3 = Ch<int> ()

let bMoved = ref false
let delayT = ref TimeSpan.Zero
(**
<div class="span5">

![Flow Diagram](images/Kismet.png)
</div>
<div class="span5">
*)
CompareBool
  bMoved
  (Ch.take ch1)
  (Ch.give ch2)
  (fun _ -> Alt.unit ())
|> Job.forever |> server

Delay
  delayT
  (Ch.take ch2)
  (Alt.never ())
  (Ch.give ch3)
  (fun _ -> Alt.unit ())
|> Job.forever |> server
(**
</div>

***
### Waiting for Jobs
*)
(*** define-output:waitJob ***)
let hello name = job {
  for i = 1 to 3 do
    do! timeOutMillis 1000
    do printfn "Hello from %s" name
}

run <| job {
  do! Job.start (hello "Job 1")
  do! timeOutMillis 500
  do! Job.start (hello "Job 2")
}
(*** include-output:waitJob ***)
(*** hide ***)
Thread.Sleep 6000
(**
---
### Waiting for a Promise
*)
(*** define-output:promise ***)
run <| job {
  let! p1 = Promise.start (hello "Job 1")
  do! timeOutMillis 500
  let! p2 = Promise.start (hello "Job 2")
  do! Promise.read p1
  do! Promise.read p2
}
(*** include-output:promise ***)
(**
---
### Waiting for a Promise
*)
(*** define-output:promiseAlt ***)
run <| job {
  let! p1 = Promise.start (hello "Job 1")
  do! timeOutMillis 500
  let! p2 = Promise.start (hello "Job 2")
  do! Alt.choose
        [ Promise.read p1 ^=> fun () ->
            printfn "Job 1 finished first"
            Promise.read p2
          Promise.read p2 ^=> fun () ->
            printfn "Job 2 finished first"
            Promise.read p1 ]
}
(*** include-output:promiseAlt ***)
(**
---
### Waiting for multiple jobs
*)
(*** define-output:conIgnore ***)
[ timeOutMillis   0 >>=. hello "Job 1"
  timeOutMillis 333 >>=. hello "Job 2"
  timeOutMillis 667 >>=. hello "Job 3"]
|> Job.conIgnore |> run
(*** include-output:conIgnore ***)
(**
***
### [Agenda](index.html)

1. [Concurrency](concurrency.html)
2. [Hopac](hopac.html)
3. [Alternatives](alternatives.html)
 - <span class="nextsegment">[Fibonacci Sequence](fibonacci.html)</span>
 - [How do they work?](altDeepDive.html)
4. [IVar, MVar, Mailbox...](remainder.html)

*)